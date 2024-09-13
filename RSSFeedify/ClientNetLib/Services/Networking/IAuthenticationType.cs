namespace ClientNetLib.Services.Networking
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
