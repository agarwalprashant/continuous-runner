﻿using System;
using System.Collections.Generic;
using System.Linq;
using Magnum;
using NLog;
using System.IO;

namespace ContinuousRunner.Impl
{
    public class ScriptLoader : IScriptLoader
    {
        #region Constructors

        public ScriptLoader(
            IInstanceContext instanceContext,
            IModuleReader moduleReader,
            IScriptParser scriptParser,
            ISourceSet sourceSet)
        {
            Guard.AgainstNull(instanceContext, nameof(instanceContext));
            _instanceContext = instanceContext;

            Guard.AgainstNull(moduleReader, nameof(moduleReader));
            _moduleReader = moduleReader;

            Guard.AgainstNull(scriptParser, nameof(scriptParser));
            _scriptParser = scriptParser;

            Guard.AgainstNull(sourceSet, nameof(sourceSet));
            _SourceSet = sourceSet;
        }

        #endregion

        #region Private members

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IInstanceContext _instanceContext;

        private readonly IModuleReader _moduleReader;

        private readonly IScriptParser _scriptParser;

        private readonly ISourceSet _SourceSet;

        #endregion


        #region Implementation of IScriptLoader

        public IEnumerable<IScript> GetScripts()
        {
            var files = _instanceContext.ScriptsRoot.GetFiles(Constants.FilenameFilter, SearchOption.AllDirectories);

            return files.Select(TryLoad).Where(s => s != null).ToList();
        }

        public IEnumerable<IScript> GetTestScripts()
        {
            var files = _instanceContext.ScriptsRoot.GetFiles(Constants.FilenameFilter, SearchOption.AllDirectories);

            files = files.Where(f => Constants.SearchExpression.IsMatch(f.Name)).ToArray();

            return files.Select(TryLoad).Where(s => s != null).ToList();
        }

        public IScript Load(FileInfo script)
        {
            return new Script(_moduleReader, _scriptParser)
                   {
                       File = script,
                       SyntaxTree = _scriptParser.Parse(script)
                   };
        }

        #endregion

        #region Private methods

        private IScript TryLoad(FileInfo fileInfo)
        {
            try
            {
                var script = _SourceSet.GetScript(fileInfo);
                if (script == null)
                {
                    script = Load(fileInfo);
                }
                else
                {
                    script.Reload();
                }

                return script;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to load script: {fileInfo.FullName}: {ex.Message}");

                return null;
            }
        }

        #endregion
    }
}