using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyClientCore
{
    public enum AuthenticationTypeName
    {
        NoAuth,
        BearerToken,
        BasicAuth
    }

    public interface IAuthenticationType
    {
        string GetPayload();
        AuthenticationTypeName GetAuthType();
    }
}
