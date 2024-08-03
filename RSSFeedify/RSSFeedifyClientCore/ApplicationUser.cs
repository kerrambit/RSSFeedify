
namespace RSSFeedifyClientCore
{
    public class ApplicationUser
    {
        public string Email { get; set; } = string.Empty;
        private string _accessToken = string.Empty;
        public bool SignedIn { get; private set; } = false;

        public ApplicationUser() {}

        public void Login(string accessToken)
        {
            _accessToken = accessToken;
            SignedIn = true;
        }

        public void Logoff()
        {
            _accessToken = string.Empty;
            SignedIn = false;
        }

        public Result<string, string> GetAccessToken()
        {
            if (!SignedIn)
            {
                return Result.Error<string, string>("You're not logged in.");
            }

            return Result.Ok<string, string>(_accessToken);
        }
    }
}
