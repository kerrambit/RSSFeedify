using CommandParsonaut.Core;
using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.Business;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCLIClient.Repository;
using RSSFeedifyClientCore.Services;
using RSSFeedifyClientCore.Services.Networking;
using RSSFeedifyCommon.Services;

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

            // Load directory for .env files and for other setting files.
            var configFilesDirectoryResult = ConfigDirectoryService.GetConfigFilesDirectory();
            if (configFilesDirectoryResult.IsError)
            {
                writer.RenderErrorMessage(configFilesDirectoryResult.GetError);
                return;
            }

            // Create logging service.
            LoggingService loggerService = new LoggingService(configFilesDirectoryResult.GetValue);
            Serilog.ILogger logger = loggerService.Logger.ForContext<Application>();

            // Load .env file.
            var envFilePathResult = ConfigDirectoryService.GetEnvironmentFilePath(configFilesDirectoryResult.GetValue);
            if (envFilePathResult.IsError)
            {
                logger.Fatal(envFilePathResult.GetError);
                writer.RenderErrorMessage(configFilesDirectoryResult.GetError);
                return;
            }
            logger.Information("Loaded .env file from '{Path}'.", envFilePathResult.GetValue);

            // Setup the base URL for API.
            Uri baseUri = new(@"http://localhost:32000/api/");
            var envResult = EnvironmentVariablesLoader.LoadEnvironmentVariable(envFilePathResult.GetValue, "RSSFEEDIFY_CLI_CLIENT_BASE_URL");
            if (envResult.IsError)
            {
                logger.Warning($"Using default base URL '{baseUri}' because the custom URL was not found. Detailed message: '{envResult.GetError}'");
            } else
            {
                baseUri = new(envResult.GetValue);
            }
            logger.Information("The base URL is '{URL}'.", baseUri.ToString());

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

            // Create AccountService for managing logged users.
            AccountService accountService = new(writer, reader, errorWriter, parser, httpService, baseUri, loggerService.Logger);

            // Create RSSFeedService that runs all commands logic. Also, HTTPService must be initialized.
            RSSFeedService rSSFeedService = new(writer, errorWriter, httpService, accountService, baseUri);

            // And finally, run the application.
            try
            {
                logger.Information("Application has started.");
                await StartAndRunApplicationAsync(writer, parser, rSSFeedService, accountService, loggerService);
            }
            catch (Exception e)
            {
                logger.Fatal("Unhandled exception occured. Application had to be terminated. Detailed message: '{Message}'", e.Message);
                writer.RenderErrorMessage("Unhandled exception occured. Application had to be terminated. Please, report the bug and send the logs to the support.");
            }
        }

        private static async Task StartAndRunApplicationAsync(IWriter writer, CommandParser parser, RSSFeedService rSSFeedService, AccountService accountService, LoggingService loggerService)
        {
            RenderASCIIPicture(writer);

            parser.InputGiven += (sender, data) => loggerService.Logger.ForContext<CommandParser>().Information(data);

            bool appRunning = true;
            while (appRunning)
            {
                Command receivedCommand;
                IList<ParameterResult> parameters;
                string rawInput;
                if (parser.GetCommand(out receivedCommand, out parameters, out rawInput))
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
