﻿using System;
using System.Collections.Generic;

namespace ContinuousRunner.Data
{
    public class TestResult
    {
        /// <summary>
        /// The overall status of whether the test succeeded or failed
        /// </summary>
        /// <value>The status.</value>
        public TestStatus Status { get; set; }

        /// <summary>
        /// Any log output generated by running this test
        /// </summary>
        /// <value>The logs.</value>
        public IList<Tuple<DateTime, string>> Logs { get; set; }
    }
}
