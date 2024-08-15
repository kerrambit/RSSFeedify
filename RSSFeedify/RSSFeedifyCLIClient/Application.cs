using CommandParsonaut.Core;
using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.Business;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCLIClient.Repository;
using RSSFeedifyClientCore.Services.Networking;

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

            // Initialize application error writer.
            ApplicationErrorWriter errorWriter = new ApplicationErrorWriter(writer);

            // Create CommandParser and fill it with commands from the commands repository.
            var parser = new CommandParser(writer, reader);
            parser.AddCommands(commands.Values.ToList());

            // Create HttpClient using HttpClientHandler (need to turn off the certificate validation as the localhost certificate is only self-signed).
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            var client = new HttpClient(clientHandler);

            // Create HTTP service.
            var httpService = new HTTPService(client);

            // Setup the base URL for API (URL can be loaded from environment variable).
            Uri baseUri = new(@"http://localhost:32000/api/");
            try
            {
                var projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                var envFilePath = Path.Combine(projectDirectory, ".env");
                DotNetEnv.Env.Load(envFilePath);
                var baseUriFromEnvVariable = Environment.GetEnvironmentVariable("RSSFEEDIFY_CLI_CLIENT_BASE_URL");
                if (baseUriFromEnvVariable != null)
                {
                    baseUri = new Uri(baseUriFromEnvVariable);
                }
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is PathTooLongException || e is FileNotFoundException || e is DirectoryNotFoundException || e is NotSupportedException || e is System.Security.SecurityException)
            {
                writer.RenderErrorMessage("Exception occured when loading environment variable(s). Application continues with the default base URL.");
                writer.RenderDebugMessage(e.Message); // Until the logging is finished.
            }
            catch (Exception e)
            {
                writer.RenderErrorMessage("Unexpected exception occured when loading environment variable(s). Application had to be terminated. Please, report the bug and send the logs to the support.");
                writer.RenderDebugMessage(e.Message); // Until the logging is finished.
            }

            // Create AccountService for managing logged users.
            AccountService accountService = new(writer, reader, errorWriter, parser, httpService, baseUri);

            // Create RSSFeedService that runs all commands logic. Also, HTTPService must be initialized.
            RSSFeedService rSSFeedService = new(writer, errorWriter, httpService, accountService, baseUri);

            // And finally, run the application.
            try
            {
                await StartAndRunApplicationAsync(writer, parser, rSSFeedService, accountService);
            }
            catch (Exception e)
            {
                writer.RenderErrorMessage("Unhandled exception occured. Application had to be terminated. Please, report the bug and send the logs to the support.");
                writer.RenderDebugMessage(e.Message); // Until the logging is finished.
            }
        }

        private static async Task StartAndRunApplicationAsync(IWriter writer, CommandParser parser, RSSFeedService rSSFeedService, AccountService accountService)
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
                        case "delete-feed":
                            await rSSFeedService.DeleteFeedAsync(parameters);
                            break;
                        case "edit-feed":
                            await rSSFeedService.EditFeedAsync(parameters);
                            break;
                        case "read-article":
                            await rSSFeedService.ReadArticle(parameters);
                            break;
                        case "next":
                            await rSSFeedService.Next();
                            break;
                        case "settings":
                            rSSFeedService.Settings();
                            break;
                        case "register":
                            await accountService.Register();
                            break;
                        case "login":
                            await accountService.Login();
                            break;
                        case "logout":
                            await accountService.Logout();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// ASCII text generation was made with the help of https://patorjk.com/software/taag/#p=display&h=3&v=0&f=Big&t=.
        /// </summary>
        /// <param name="writer">IWriter that renders the text.</param>
        private static void RenderASCIIPicture(IWriter writer)
        {
            writer.RenderBareText("\r\n\r\n  _____   _____ _____ ______            _ _  __         __  __   ___  \r\n |  __ \\ / ____/ ____|  ____|          | (_)/ _|       /_ |/_ | / _ \\ \r\n | |__) | (___| (___ | |__ ___  ___  __| |_| |_ _   _   | | | || | | |\r\n |  _  / \\___ \\\\___ \\|  __/ _ \\/ _ \\/ _` | |  _| | | |  | | | || | | |\r\n | | \\ \\ ____) ____) | | |  __|  __| (_| | | | | |_| |  | |_| || |_| |\r\n |_|  \\_|_____|_____/|_|  \\___|\\___|\\__,_|_|_|  \\__, |  |_(_|_(_\\___/ \r\n                                                 __/ |                \r\n                                                |___/                 \r\n\r\n");
        }
    }
}
