﻿using System;
using System.Collections.Generic;

namespace ContinuousRunner
{
    public interface IResultFactory
    {
        /// <summary>
        /// Create a state indicating that the test has never been run
        /// </summary>
        TestResult Indeterminate();

        TestResult Deleted();

        TestResult FailedToRun(Exception exception);

        TestResult Success(IEnumerable<string> logs);

        TestResult AssertFailure(Assertion assertion, IEnumerable<string> logs);
    }
}
