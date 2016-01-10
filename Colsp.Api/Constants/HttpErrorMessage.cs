using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Constants
{
    public static class HttpErrorMessage
    {
        public static readonly string FormatError = "This request is not properly formatted";
        public static readonly string MissingParameterError = "Required parameter(s) is/are missing";
        public static readonly string InternalServerError = "Internal server error. Please try again later";
        public static readonly string NotFound = "Cannot find searching object";
        public static readonly string DateFormatError = "Invalid date format";
        public static readonly string TimeFormatError = "Invalid time format";
    }
}