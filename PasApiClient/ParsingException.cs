using System;
using System.Collections.Generic;
using System.Text;

namespace PasApiClient
{
    public class ParsingException : Exception
    {
        public ParsingException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }
}