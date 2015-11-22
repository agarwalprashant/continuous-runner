﻿using System;
using System.IO;

namespace TestRunner
{
    public class ScriptParser : IScriptParser
    {
        #region Implementation of IScriptParser

        public SyntaxTree Parse(FileInfo fileInfo)
        {
            var parser = new Jint.Parser.JavaScriptParser();

            try
            {
                return new SyntaxTree
                {
                    Root = parser.Parse(GetScript(fileInfo))
                };
            }
            catch (Exception ex)
            {
                throw new TestException($"Failed to parse JavaScript file: {fileInfo}: {ex.Message}", ex);
            }
        }

        #endregion

        #region Private methods

        private static string GetScript(FileInfo script)
        {
            using (var stream = script.OpenRead())
            {
                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        #endregion
    }
}
