﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using Autofac;
using ContinuousRunner.Data;
using ContinuousRunner.Tests.Mock;
using ContinuousRunner.Work;

using FluentAssertions;

using Jint.Parser.Ast;

using Magnum.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace ContinuousRunner.Tests.Loading
{
    public class ScriptLoaderTests : BaseTest
    {
        public ScriptLoaderTests(ITestOutputHelper helper)
            : base(helper)
        {}

        [Fact]
        public void LoadSimpleRequireJsAndJasmineScript()
        {
            RunWithSimpleTest(
                (container, script) =>
                {
                    // Do a brief verification that we were able to parse the script into a reasonable syntax tree
                    script.ExpressionTree.Should().NotBeNull();
                    script.ExpressionTree.Root.Type.Should().Be(SyntaxNodes.Program);
                    script.ExpressionTree.Root.As<Program>().Should().NotBeNull();

                    script.Module.Should().NotBeNull();

                    var module = script.Module;

                    // Module name should be picked up from the filename and root namespace
                    module.ModuleName.Should()
                          .Be($"{nameof(Tests)}/{Path.GetFileNameWithoutExtension(script.File.Name)}");

                    // There are no dependencies in the define() statement--
                    module.References.Count().Should().Be(0);
                });
        }

        [Fact]
        public void RunJavaScriptTestAndVerifyResult()
        {
            var resultEvent = new AutoResetEvent(false);

            RunWithSimpleTest(
                (container, script) =>
                {
                    var tests = script.Suites.ToArray();

                    var testWriter = container.Resolve<ITestWriter>();

                    testWriter.Write(tests);

                    var runner = container.Resolve<IConcurrentExecutor>();

                    runner.Push(new ExecuteScriptWork(script));

                    Thread.Sleep(TimeSpan.FromSeconds(20d));

                    testWriter.Write(tests);

                    if (resultEvent.WaitOne(TimeSpan.FromSeconds(30d)) == false)
                    {
                        throw new TestException("No test result was ever received!");
                    }
                },
                result =>
                {
                    result.Status.Should().Be(Status.Success);

                    // the test calls console.log, make sure we can see that output
                    result.Logs.Count.Should().Be(1);

                    resultEvent.Set();
                });
        }

        private class ResultHandler : ISubscription<TestResult>
        {
            private readonly Action<TestResult> _handler;

            public ResultHandler(Action<TestResult> handler)
            {
                _handler = handler;
            }

            #region Implementation of ISubscription<in TestResult>

            public void Handle(TestResult @event)
            {
                _handler(@event);
            }

            #endregion
        }
        private void RunWithSimpleTest(Action<IComponentContext, IScript> f, Action<TestResult> resultHandler = null)
        {
            var handler = new ResultHandler(resultHandler);

            Action<ContainerBuilder> build = cb => cb.RegisterInstance(handler).As<ISubscription<TestResult>>();

            using (var container = CreateTypicalContainer(MockFile.TempFile<DirectoryInfo>(), build))
            {
                const string content =
                    @"define('Foo', [], function () {
                        describe('Foo', function () {
                          it('Bar', function () {
                            console.log('Test output');
                          });
                        });
                      });";

                var fileInfo = container.Resolve<IMockFile>().FromString("js", content);

                var loader = container.Resolve<ILoader<IScript>>();

                var script = loader.Load(fileInfo);
                script.Should().NotBeNull();

                f(container, script);
            }
        }
    }
}