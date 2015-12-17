﻿using System.ComponentModel.Composition;
using Autofac;
using Microsoft.ClearScript;

namespace ContinuousRunner.Frameworks.RequireJs
{
    public class FrameworkImpl : IFramework
    {
        [Import] private readonly IComponentContext _componentContext;

        public Framework Framework => Framework.RequireJs;

        public void Install(IProjectSource source, ScriptEngine engine)
        {
            if (source is IScript == false)
            {
                return;
            }

            var script = (IScript) source;

            var requireImpl = _componentContext.Resolve<RequireDefine>(
                new TypedParameter(typeof (ScriptEngine), engine),
                new TypedParameter(typeof (IScript), script));

            engine.AddHostObject(nameof(requireImpl), requireImpl);

            engine.Execute(
                @"var require = function (names, callback) {
                    console.log('loading: ' + names + ' ' + names[0] + ' ' + names[1]);

                    var list = new mscorlib.System.Collections.ArrayList();

                    for (var i = 0; i < names.length; ++i) {
                      list.Add(names[i]);
                    }

                    var loaded = names.length > 0
                        ? requireImpl.RequireMultiple(list)
                        : requireImpl.RequireSingle(names);
  
                    if (typeof callback === 'function') {
                      return callback.apply(null, loaded);
                    }

                    return callack;
                  };");
            
            engine.Execute(
              @"var define = function (name, deps, body) {
                  if (typeof name !== 'string') {
                      body = deps;
                      deps = name;
                      name = null;
                  }

                  if (deps instanceof Array === false) {
                    if (body != null) {
                      throw new Error('define() was called with incomprehensible arguments');
                    }

                    body = deps;

                    deps = [];
                  }
 
                  var CallbackType = mscorlib.System.Func(mscorlib.System.Collections.ArrayList, mscorlib.System.Object);

                  var cb = new CallbackType(function(deps) {
                    if (typeof body === 'function') {
                      var transformed = [];
                      for (var i = 0; i < deps.Count; ++i) {
                        transformed.push(deps[i]);
                      }

                      return body.apply(window, deps);
                    }

                    return body;
                  });

                  var list = new mscorlib.System.Collections.ArrayList();

                  for (var i = 0; i < deps.length; ++i) {
                    list.Add(deps[i]);
                  }
    
                  requireImpl.DefineModule(name, list, cb);
                };

                define['amd'] = true;");
        }
    }
}
