using CommandParsonaut.CommandHewAwayTool;
using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.Business;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCLIClient.Repository;
using RSSFeedifyCLIClient.Services;

namespace RSSFeedifyCLIClient
{
    public class Application
    {
        public static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private static async Task RunAsync()
        {
            // Create commands repository.
            var commands = CommandsRepository.InitCommands();

            // Initialize the writer and reader.
            IWriter writer = new Writer();
            IReader reader = new Reader();

            // Create CommandParser and fill it with commands from the commands repository.
            var parser = new CommandParser(writer, reader);
            parser.AddCommands(commands.Values.ToList());

            // Create HttpClient using HttpClientHandler (need to turn off the certificate validation as the localhost certificate is only self-signed).
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            var client = new HttpClient(clientHandler);

            // Create RSSFeedService that runs all commands logic. Also, HTTPService must be initialized.
            RSSFeedService rSSFeedService = new(writer, new HTTPService(client));

            // And finally, run the application.
            try
            {
                await StartAndRunApplicationAsync(writer, parser, rSSFeedService);
            } catch (Exception)
            {
                writer.RenderErrorMessage("Unhandled exception occured. Application had to be terminated. Please, report the bug and send the logs to the support.");
            }
        }

        private static async Task StartAndRunApplicationAsync(IWriter writer, CommandParser parser, RSSFeedService rSSFeedService)
        {
            RenderASCIIPicture(writer);

            bool appRunning = true;
            while (appRunning)
            {
                Command receivedCommand;
                IList<ParameterResult> parameters;
                string _;
                if (parser.GetCommand(out receivedCommand, out parameters, out _))
                {
                    switch (receivedCommand.Name)
                    {
                        case "quit":
                            appRunning = false;
                            break;
                        case "list-feeds":
                            await rSSFeedService.GetFeedsAsync();
                            break;
                        case "add-feed":
                            await rSSFeedService.AddNewFeed(parameters);
                            break;
                        case "get-feed":
                            await rSSFeedService.GetFeedItemsAsync(parameters);
                            break;
                        case "read-article":
                            await rSSFeedService.ShowArticle(parameters);
                            break;
                        case "next":
                            await rSSFeedService.Next();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static void RenderASCIIPicture(IWriter writer)
        {
           writer.RenderBareText("  _____   _____ _____ ______            _ _  __       \r\n |  __ \\ / ____/ ____|  ____|          | (_)/ _|      \r\n | |__) | (___| (___ | |__ ___  ___  __| |_| |_ _   _ \r\n |  _  / \\___ \\\\___ \\|  __/ _ \\/ _ \\/ _` | |  _| | | |\r\n | | \\ \\ ____) |___) | | |  __/  __/ (_| | | | | |_| |\r\n |_|  \\_\\_____/_____/|_|  \\___|\\___|\\__,_|_|_|  \\__, |\r\n                                                 __/ |\r\n                                                |___/ \r\n\r\n");
        }
    }
}
