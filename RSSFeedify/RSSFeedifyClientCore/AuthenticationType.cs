namespace RSSFeedifyClientCore
{
    public abstract class AuthenticationType : IAuthenticationType
    {
        protected string? _payload;
        protected AuthenticationTypeName _typeName = AuthenticationTypeName.NoAuth;

        public string GetPayload()
        {
            if (_payload is null)
            {
                throw new ArgumentNullException("Authentication payload value cannot be null.");
            }
            return _payload;
        }

        public AuthenticationTypeName GetAuthType()
        {
            return _typeName;
        }
    }

    public class NoAuth : AuthenticationType
    {
        public NoAuth()
        {
            _payload = string.Empty;
            _typeName = AuthenticationTypeName.NoAuth; 
        }
    }

    public class BearerToken : AuthenticationType
    {
        public BearerToken(string token)
        {
            _payload = $"Bearer {token}";
            _typeName = AuthenticationTypeName.BearerToken;
        }
    }
}
