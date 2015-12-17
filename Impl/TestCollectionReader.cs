﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using ContinuousRunner.Frameworks;

using Microsoft.ClearScript;

namespace ContinuousRunner.Impl
{
    using Frameworks.Jasmine;

    public class TestCollectionReader : ITestCollectionReader
    {
        [Import] private readonly IRuntimeFactory<ScriptEngine> _runtimeFactory;

        [Import] private readonly IJasmineReflection _reflector;

        [Import] private readonly IEnumerable<IFramework> _frameworks;

        #region Implementation of ISuiteReader

        public ITestCollection DefineTests(IScript script)
        {
            using (var engine = _runtimeFactory.GetRuntime())
            {
                var collection = _reflector.Reflect(script, engine);

                foreach (var framework in _frameworks.Where(framework => framework.Framework != Framework.Jasmine)) // reflector is jasmine impl
                {
                    framework.Install(script, engine);
                }

                engine.Execute(script.Content);

                return collection;
            }
        }

        #endregion
    }
}
