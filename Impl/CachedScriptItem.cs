﻿using System;
using System.Runtime.InteropServices;

namespace ContinuousRunner.Impl
{
    [StructLayout(LayoutKind.Auto, Pack = 1)]
    public struct CachedScriptItem
    {
        /// <summary>
        /// The time that this script was loaded and parsed
        /// </summary>
        public DateTime Updated;

        public DateTime Accessed;

        /// <summary>
        /// A checksum or hash of the file contents the last time we saw it
        /// </summary>
        public Guid Hash;

        /// <summary>
        /// The complete in-memory representation of the parsed, loaded script
        /// </summary>
        public IScript Script;
    }
}