
namespace RSSFeedifyCLIClient.Business
{
    public class ApplicationError
    {
        private ClientNetLib.Business.Errors.Error _networkError;
        private Errors.Error _applicationError;
        public string Details { get; set; } = string.Empty;

        private enum ChosenError
        {
            Network,
            Application
        }

        private ChosenError _chosenError;

        public ApplicationError(ClientNetLib.Business.Errors.Error error)
        {
            _networkError = error;
            _chosenError = ChosenError.Network;
        }

        public ApplicationError(ClientNetLib.Business.Errors.Error error, string details)
        {
            _networkError = error;
            _chosenError = ChosenError.Network;
            Details = details;
        }
        public ApplicationError(Errors.Error error)
        {
            _applicationError = error;
            _chosenError = ChosenError.Application;
        }
        public ApplicationError(Errors.Error error, string details)
        {
            _applicationError = error;
            _chosenError= ChosenError.Application;
            Details = details;
        }

        public ApplicationError(ClientNetLib.Business.Errors.DetailedError detailed)
        {
            _networkError = detailed.Error;
            _chosenError = ChosenError.Network;
            Details = detailed.Details;
        }

        public string GetErrorMessage()
        {
            string message = string.Empty;
            if (_chosenError == ChosenError.Network)
            {
                message = _networkError switch
                {
                    ClientNetLib.Business.Errors.Error.General => "Error occurred!",
                    ClientNetLib.Business.Errors.Error.NetworkGeneral => "Unable to send the request!",
                    ClientNetLib.Business.Errors.Error.NetworkUnexpectedStatusCode => "Unexpected status code received!",
                    ClientNetLib.Business.Errors.Error.NetworkBadRequest => "Bad request!",
                    ClientNetLib.Business.Errors.Error.NetworkForbidden => "Access forbidden!",
                    ClientNetLib.Business.Errors.Error.NetworkNotFound => "Not found!",
                    ClientNetLib.Business.Errors.Error.NetworkUnauthorized => "Unauthorized access!",
                    ClientNetLib.Business.Errors.Error.NetworkNotSupported => "Operation not supported!",
                    ClientNetLib.Business.Errors.Error.NetworkUnexpectedDataType => "Wrong data type received!",
                    ClientNetLib.Business.Errors.Error.InvalidJsonFormat => "Invalid Json format!",
                    _ => "Error occurred!"
                };
            } else
            {
                message = _applicationError switch
                {
                    Errors.Error.General => "Error occured!",
                    Errors.Error.UserNotLoggedIn => "You are not logged in!",
                    Errors.Error.InvalidUrl => $"Provided URL does not seem to be valid URL and thus the link was refused to be opened in a browser.",
                    _ => "Error occurred!"
                };
            }

            return message;
        }
    }
}
