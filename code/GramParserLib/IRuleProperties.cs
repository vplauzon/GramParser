﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GramParserLib
{
    public interface IRuleProperties
    {
        bool? HasInterleave { get; }

        /// <summary><c>true</c> iif there are no rules being fired by this one.</summary>
        bool IsTerminalRule { get; }
        
        bool? IsCaseSensitive { get; }
    }
}