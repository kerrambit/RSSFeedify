using System.Net.Http.Headers;
using System.Text;

namespace ClientNetLib.Services.Networking
{
    public static class AuthenticationHeaderExtensions
    {
        public static AuthenticationHeaderValue ConvertToDotNetHttpHeader(this IAuthenticationHeader authenticationHeader)
        {
            switch (authenticationHeader.AuthSchemeType)
            {
                case AuthenticationTypeName.NoAuth:
                    throw new ArgumentException("AuthSchemeType cannot be NoAuth!");
                case AuthenticationTypeName.BearerToken:
                    return new AuthenticationHeaderValue("Bearer", authenticationHeader.AuthToken);
                case AuthenticationTypeName.BasicAuth:
                    var parameter = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationHeader.AuthToken));
                    return new AuthenticationHeaderValue("Basic", parameter);
                default:
                    throw new NotSupportedException($"AuthSchemeType {authenticationHeader.AuthSchemeType} is not supported!");
            }
        }
    }
}
