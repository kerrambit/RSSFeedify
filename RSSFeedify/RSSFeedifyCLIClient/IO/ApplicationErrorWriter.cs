using CommandParsonaut.Interfaces;
using RSSFeedifyClientCore;

namespace RSSFeedifyCLIClient.IO
{
    public class ApplicationErrorWriter
    {
        private readonly IWriter _writer;

        public ApplicationErrorWriter(IWriter writer)
        {
            _writer = writer;
        }
        public void RenderErrorMessage()
        {
            RenderErrorMessage(ApplicationError.General);
        }

        public void RenderErrorMessage(string error)
        {
            _writer.RenderErrorMessage(error);
        }

        public void RenderErrorMessage(ApplicationError error, string details)
        {
            _writer.RenderErrorMessage(CreateErrorMessage(error, details));
        }

        public void RenderErrorMessage(ApplicationError error)
        {
            _writer.RenderErrorMessage(CreateErrorMessage(error));
        }

        public void RenderErrorMessage(DetailedApplicationError error)
        {
            _writer.RenderErrorMessage(CreateErrorMessage(error.Error, error.Details));
        }

        public void RenderErrorMessage(DetailedApplicationError error, string? overrideDetails = null)
        {
            var details = error.Details;
            if (!error.HasDetails() && overrideDetails is not null)
            {
                details = overrideDetails;
            }

            _writer.RenderErrorMessage(CreateErrorMessage(error.Error, details));
        }

        // TODO: should be replaces by DetailedApplicationError
        public void RenderInvalidUrlWarningMessage(string link)
        {
            _writer.RenderWarningMessage($"Provided URL '{link}' does not seem to be valid URL and thus the link was refused to be opened in a browser.");
        }

        private string CreateErrorMessage(ApplicationError error, string? details = null)
        {
            string message = error switch
            {
                ApplicationError.General => "Error occurred!",
                ApplicationError.NetworkGeneral => "Unable to send the request!",
                ApplicationError.NetworkUnexpectedStatusCode => "Unexpected status code received!",
                ApplicationError.NetworkBadRequest => "Bad request!",
                ApplicationError.NetworkForbidden => "Access forbidden!",
                ApplicationError.NetworkNotFound => "Not found!",
                ApplicationError.NetworkUnauthorized => "Unauthorized access!",
                ApplicationError.NetworkNotSupported => "Operation not supported!",
                ApplicationError.NetworkServerError => "Server error!",
                ApplicationError.NetworkUnexpectedDataType => "Wrong data type received!",
                ApplicationError.InvalidJsonFormat => "Invalid Json format!",
                ApplicationError.UserNotLoggedIn => "You are not logged in!",
                _ => "Error occurred!"
            };

            if (!string.IsNullOrEmpty(details))
            {
                message += $" Detailed message: '{details}'.";
            }

            return message;
        }
    }
}
