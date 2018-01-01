using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    public class ParsingException : Exception
    {
        public ParsingException(string message) : base(message)
        {
        }
    }
}