using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyClientCore
{
    public static class AuthenticationHeaderExtensions
    {
        public static System.Net.Http.Headers.AuthenticationHeaderValue ConvertToDotNetHttpHeader(this IAuthenticationHeader authenticationHeader)
        {
            return new System.Net.Http.Headers.AuthenticationHeaderValue(authenticationHeader.AuthScheme, authenticationHeader.AuthToken);
        }
    }
}
