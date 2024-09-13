using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.Business;
using RSSFeedifyCLIClient.Business.Errors;

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
            RenderErrorMessage(new ApplicationError(Error.General));
        }

        public void RenderErrorMessage(string error)
        {
            _writer.RenderErrorMessage(error);
        }

        public void RenderErrorMessage(ApplicationError error)
        {
            _writer.RenderErrorMessage(CreateErrorMessage(error));
        }

        public void RenderErrorMessage(ApplicationError error, string? overrideDetails = null)
        {
            if (error.Details.Length != 0 && overrideDetails is not null)
            {
                error.Details = overrideDetails;
            }

            _writer.RenderErrorMessage(CreateErrorMessage(error));
        }

        private string CreateErrorMessage(ApplicationError error)
        {
            string message = error.GetErrorMessage();
            if (!string.IsNullOrEmpty(error.Details))
            {
                message += $" Detailed message: '{error.Details}'.";
            }

            return message;
        }
    }
}
