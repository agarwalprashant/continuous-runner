﻿using System.Linq;

using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

using Microsoft.ClearScript;

namespace ContinuousRunner
{
    using Frameworks.RequireJs;
    using Impl;
    using Work;

    public class ContinuousRunnerModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var except = new[] {typeof (ExecuteScriptWork)};

            var singleInstanceRegisters = new[]
                                          {
                                              typeof (CachedScripts),
                                              typeof (ScriptCollection),
                                              typeof (RequirePackageSystem)
                                          };

            foreach (var r in singleInstanceRegisters)
            {
                builder.RegisterType(r)
                       .AsImplementedInterfaces()
                       .SingleInstance()
                       .OnActivating(ActivateInject);
            }

            builder.Register((c, p) => new TestCollection(p.TypedAs<IScript>()))
                   .AsImplementedInterfaces()
                   .OnActivating(ActivateInject);

            var completeRegister = builder.RegisterAssemblyTypes(typeof(ContinuousRunnerModule).Assembly)
                   .AsImplementedInterfaces()
                   .OnActivating(ActivateInject);

            builder.Register((c, p) => new ExecuteScriptWork(c.Resolve<IRunner<IScript>>(), p.TypedAs<IScript>(), p.TypedAs<string>()))
                   .As<ExecuteScriptWork>();
            
            var declareExcept = typeof(Autofac.RegistrationExtensions).GetMethod("Except",
                new[] {typeof(IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>)});

            completeRegister =
                singleInstanceRegisters.Concat(except)
                                       .Select(r => declareExcept.MakeGenericMethod(r))
                                       .Aggregate(completeRegister,
                                                  (current, method) =>
                                                  method.Invoke(current, new object[] {current}) as
                                                  IRegistrationBuilder
                                                      <object, ScanningActivatorData, DynamicRegistrationStyle>);

            builder.Register(
                c =>
                {
                    var scriptLoader = c.Resolve<ICachedScripts>();

                    return c.Resolve<IRequireConfigurationLoader>().Load(c.Resolve<IScriptCollection>().GetScriptFiles(), f => scriptLoader.Load(f));
                })
                   .As<IRequireConfiguration>()
                   .SingleInstance()
                   .OnActivating(ActivateInject);

            builder.Register((c, p) =>
                             {
                                 var parameters = p.ToArray();

                                 return new RequireDefine(parameters.TypedAs<ScriptEngine>(),
                                                          parameters.TypedAs<IScript>());
                             })
                   .OnActivating(ActivateInject);
        }

        private static void ActivateInject<T>(IActivatingEventArgs<T> args)
        {
            PropertyInjector.InjectProperties(args.Context, args.Instance);
        }
    }
}