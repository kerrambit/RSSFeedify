using CommandParsonaut.Core.Types;

namespace RSSFeedifyCLIClient.Repository
{
    /// <summary>
    /// Stores all commands in the map.
    /// </summary>
    public static class CommandsRepository
    {
        public static Dictionary<string, Command> InitCommands()
        {
            Dictionary<string, Command> commands = new Dictionary<string, Command>();

            Command listFeeds = new Command("list-feeds", "", "Lists all available RSS feeds.", []);
            Command addRSSFeed = new Command("add-feed", "name:STRING description:STRING url:URI polling-interval-in-minutes:INTEGER", "Adds new RSS feed into the database.", new List<ParameterType> { ParameterType.String, ParameterType.String, ParameterType.String, ParameterType.IntegerRange }, new List<(int, int)> { (10, 60)});

            Command getRSSFeedItems = new Command("get-feed", "guid:STRING", "Displays RSSFeed items.", new List<ParameterType> { ParameterType.String });
            Command readArticle = new Command("read-article", "guid:STRING", "Opens a link of the RSSFeedItem in the default browser.", new List<ParameterType> { ParameterType.String });

            Command next = new Command("next", "", "Shows new page when listing", []);
            Command quit = new Command("quit", "", "Quits the application.", new List<ParameterType> { });

            commands["list-feeds"] = listFeeds;
            commands["add-feed"] = addRSSFeed;

            commands["get-feed"] = getRSSFeedItems;
            commands["show-article"] = readArticle;

            commands["next"] = next;
            commands["quit"] = quit;

            return commands;
        }
    }
}
