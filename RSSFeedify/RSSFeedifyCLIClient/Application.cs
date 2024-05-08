using CommandParsonaut.CommandHewAwayTool;
using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCLIClient.Repository;

namespace RSSFeedifyCLIClient
{
    public class Application
    {
        public static void Main(string[] args)
        {
            var commands = CommandsRepository.InitCommands();
            IWriter writer = new Writer();
            IReader reader = new Reader();

            var parser = new CommandParser(writer, reader);
            parser.AddCommands(commands.Values.ToList());

            bool appRunning = true;
            while (appRunning)
            {
                Command receivedCommand;
                IList<ParameterResult> parameters;
                string unprocessedInput;
                if (parser.GetCommand(out receivedCommand, out parameters, out unprocessedInput))
                {
                    switch (receivedCommand.Name)
                    {
                        case "quit":
                            appRunning = false;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
