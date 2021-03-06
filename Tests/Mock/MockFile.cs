﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using NLog;

namespace ContinuousRunner.Tests.Mock
{
    public class MockFile : IMockFile
    {
        [Import] private readonly IInstanceContext _instanceContext;

        private readonly TempFileCollection _collection;

        private readonly Random _random = new Random();

        public MockFile(IInstanceContext instanceContext)
        {
            _instanceContext = instanceContext;

            _collection = new TempFileCollection(_instanceContext.ScriptsRoot.FullName);
        }

        public FileInfo FromString(string extension, string content)
        {
            var path = _collection.AddExtension($"{Convert.ToInt32(_random.Next())}.{extension}");

            var fs = new FileInfo(path);

            using (var stream = fs.OpenWrite())
            {
                var bytes = Encoding.UTF8.GetBytes(content);

                stream.Write(bytes, 0, bytes.Length);
            }

            return fs;
        }

        /// <summary>
        /// Get a reference to a test file (T FileInfo) or a test directory (T DirectoryInfo).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="components"></param>
        /// <returns></returns>
        public T TestFile<T>(params string[] components) where T : FileSystemInfo
        {
            var path = new List<string>
                       {
                           _instanceContext.ScriptsRoot.FullName
                       };

            path.AddRange(components);

            return (T) Activator.CreateInstance(typeof (T), Path.Combine(path.ToArray()));
        }

        public static T TempFile<T>(params string[] components) where T : FileSystemInfo
        {
            var executingFrom = AppDomain.CurrentDomain.BaseDirectory;

            var path = new List<string>
                       {
                           executingFrom,
                           @"..",
                           @".."
                       };

            path.AddRange(components);

            return (T) Activator.CreateInstance(typeof (T), Path.Combine(path.ToArray()));
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_collection != null)
            {
                try
                {
                    _collection.Delete();
                }
                catch (IOException ex)
                {
                    var logger = LogManager.GetCurrentClassLogger();

                    logger.Error(ex, $"Failed to delete temporary files produced by tests: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
