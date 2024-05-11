using CommandParsonaut.CommandHewAwayTool;
using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.Business;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCLIClient.Repository;
using RSSFeedifyCLIClient.Services;
using System.Security.Authentication;

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
            var commands = CommandsRepository.InitCommands();
            IWriter writer = new Writer();
            IReader reader = new Reader();

            var parser = new CommandParser(writer, reader);
            parser.AddCommands(commands.Values.ToList());

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            var client = new HttpClient(clientHandler);

            RSSFeedService rSSFeedService = new(writer, new HTTPService(client));

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
