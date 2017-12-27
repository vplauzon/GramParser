using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    public interface IRuleProperties
    {
        bool? HasInterleave { get; }

        bool? IsRecursive { get; }

        /// <summary><c>true</c> iif there are no rules being fired by this one.</summary>
        bool IsTerminalRule { get; }
    }
}