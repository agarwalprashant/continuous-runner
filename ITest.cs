﻿using System;

namespace TestRunner
{
    using Data;

    public interface ITest
    {
        Guid Id { get; }

        string Name { get; }

        TestSuite Suite { get; }

        TestResult Result { get; set; }

        TestResult Run();
    }
}