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

        public void RenderErrorMessage(ApplicationError error)
        {
            switch (error)
            {
                case ApplicationError.Network:
                    _writer.RenderErrorMessage($"Unable to send the request!");
                    break;
                case ApplicationError.DataType:
                    _writer.RenderErrorMessage("Wrong data type received!");
                    break;
                case ApplicationError.InvalidJsonFormat:
                    _writer.RenderErrorMessage("Invalid Json format!");
                    break;
                case ApplicationError.General:
                default:
                    _writer.RenderErrorMessage("Error occured!");
                    break;
            }
        }

        public void RenderErrorMessage(string error)
        {
            _writer.RenderErrorMessage(error);
        }

        public void RenderErrorMessage()
        {
            RenderErrorMessage(ApplicationError.General);
        }

        public void RenderInvalidUrlWarningMessage(string link)
        {
            _writer.RenderWarningMessage($"Provided URL '{link}' does not seem to be valid URL and thus the link was refused to be opened in a browser.");
        }
    }
}
