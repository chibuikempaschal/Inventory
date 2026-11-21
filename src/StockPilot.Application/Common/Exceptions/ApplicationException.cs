using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockPilot.Application.Common.Exceptions
{
    public class ApplicationException : Exception
    {

        public int StatusCode { get; }

        public ApplicationException(string message, int statusCode = 400)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public class AlreadyExistException : ApplicationException
        {
            public AlreadyExistException(string message) : base(message, 409) { }
        }

        public class NotFoundException : ApplicationException
        {
            public NotFoundException(string message) : base(message, 404) { }
        }

        public class ForbiddenException : ApplicationException
        {
            public ForbiddenException(string message) : base(message, 403) { }
        }
    }

}