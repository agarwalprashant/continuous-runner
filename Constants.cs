﻿using System.Text.RegularExpressions;

namespace TestRunner
{
    public class Constants
    {
        /// <summary>
        /// The expression we will use to search for TypeScript tests
        /// </summary>
        public readonly Regex SearchExpression = new Regex("(spec|tests|test).(ts|js)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// An overly-broad string to narrow the list of files that we care about watching (but the regular expression above
        /// is the final word on whether a file will be considered for inclusion in the watch set, not this filter; this
        /// is just a performance optimization).
        /// </summary>
        public const string FileFilter = @"*.?s";

        /// <summary>
        /// A rough guess of what number of source files we should optimize for
        /// </summary>
        public const int EstimatedFiles = 1000;
    }
}
