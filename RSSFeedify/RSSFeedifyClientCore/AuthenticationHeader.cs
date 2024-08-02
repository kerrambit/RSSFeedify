
namespace RSSFeedifyClientCore
{
    public class AuthenticationHeader : IAuthenticationHeader
    {
        private readonly IAuthenticationType _authType;

        public AuthenticationHeader(IAuthenticationType authenticationType)
        {
            _authType = authenticationType;
        }

        public string AuthScheme => _authType.GetAuthType().ToString();

        public AuthenticationTypeName AuthSchemeType => _authType.GetAuthType();

        public string AuthToken => _authType.GetPayload();
    }
}
