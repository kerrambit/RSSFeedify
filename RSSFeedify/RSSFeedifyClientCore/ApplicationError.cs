﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyClientCore
{
    public enum ApplicationError
    {
        General,
        NetworkGeneral,
        NetworkUnexpectedStatusCode,
        NetworkBadRequest,
        NetworkForbidden,
        NetworkNotFound,
        NetworkUnauthorized,
        NetworkNotSupported,
        NetworkServerError,
        NetworkUnexpectedDataType,
        InvalidJsonFormat,
        UserNotLoggedIn
    }

    public static class ApplicationErrorExtensions
    {
        public static DetailedApplicationError ConvertToDetailedApplicationError(this ApplicationError error, string details)
        {
            return new DetailedApplicationError(error, details);
        }

        public static DetailedApplicationError ConvertToDetailedApplicationError(this ApplicationError error)
        {
            return new DetailedApplicationError(error, string.Empty);
        }
    }
}
