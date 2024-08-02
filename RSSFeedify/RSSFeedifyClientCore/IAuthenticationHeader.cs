using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyClientCore
{
    public interface IAuthenticationHeader
    {
        string AuthScheme { get; }
        string AuthToken { get; }
        AuthenticationTypeName AuthSchemeType { get; }
    }
}
