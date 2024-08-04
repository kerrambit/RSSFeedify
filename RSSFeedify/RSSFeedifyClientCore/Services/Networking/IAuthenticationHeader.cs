namespace RSSFeedifyClientCore.Services.Networking
{
    public interface IAuthenticationHeader
    {
        string AuthScheme { get; }
        string AuthToken { get; }
        AuthenticationTypeName AuthSchemeType { get; }
    }
}
