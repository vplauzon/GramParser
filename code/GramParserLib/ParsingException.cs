using System;
using System.Collections.Generic;
using System.Text;

namespace GramParserLib
{
    public class ParsingException : Exception
    {
        public ParsingException(string message) : base(message)
        {
        }
    }
}