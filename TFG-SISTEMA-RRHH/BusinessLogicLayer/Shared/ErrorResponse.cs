using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Shared
{
    public class ErrorResponse
    {
        public bool Success => false;
        public string Code { get; set; }
        public string Message { get; set; }

        public ErrorResponse(string code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}