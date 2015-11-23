﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ContinuousRunner
{
    using Data;

    public interface ISourceSet : IDisposable
    {
        IScript GetScript(FileInfo fileInfo);

        IScript GetScriptFromModuleReference(string absoluteReference);

        IEnumerable<IScript> GetDependencies(IScript origin);

        IEnumerable<TestSuite> GetSuites();
    }
}