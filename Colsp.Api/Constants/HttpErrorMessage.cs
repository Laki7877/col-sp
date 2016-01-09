using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Constants
{
    public static class HttpErrorMessage
    {
        public static readonly string FORMAT_ERROR = "This request is not properly formatted";
        public static readonly string MISSING_PARAMETER_ERROR = "Required parameter(s) is/are missing";
        public static readonly string INTERNAL_SERVER_ERROR = "Internal server error. Please try again later";
        public static readonly string NOT_FOUND = "Cannot find searching object";
        public static readonly string DATE_FORMAT_ERROR = "Invalid date format";
        public static readonly string TIME_FORMAT_ERROR = "Invalid time format";
    }
}