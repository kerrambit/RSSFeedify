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

            Command register = new Command("register", "", "Register new account.", []);
            Command login = new Command("login", "", "Login the user.", []);
            Command logout = new Command("logout", "", "Log out the user.", []);

            Command listFeeds = new Command("list-feeds", "", "Lists all available RSS feeds.", []);
            Command addRSSFeed = new Command("add-feed", "name:STRING description:STRING url:URI polling-interval-in-minutes:INTEGER", "Adds new RSS feed into the database.", new List<ParameterType> { ParameterType.String, ParameterType.String, ParameterType.Uri, ParameterType.DoubleRange }, new List<(double, double)> { (10.0, 60.0) });
            Command deleteRSSFeed = new Command("delete-feed", "guid:STRING", "Deletes RSSFeed.", new List<ParameterType> { ParameterType.String });
            Command editRSSFeed = new Command("edit-feed", "guid:STRING new-name:STRING new-description:STRING new-polling-interval-in-minutes:INTEGER", "Edits RSS feed in the database.", new List<ParameterType> { ParameterType.String, ParameterType.String, ParameterType.String, ParameterType.DoubleRange }, new List<(double, double)> { (10.0, 60.0) });

            Command getRSSFeedItems = new Command("get-feed", "guid:STRING", "Displays RSSFeed items.", new List<ParameterType> { ParameterType.String });
            Command readArticle = new Command("read-article", "guid:STRING", "Opens a link of the RSSFeedItem in the default browser.", new List<ParameterType> { ParameterType.String });

            Command next = new Command("next", "", "Shows new page when listing", []);
            Command quit = new Command("quit", "", "Quits the application.", new List<ParameterType> { });
            Command settings = new Command("settings", "", "Shows current settings of the application.", []);

            commands["settings"] = settings;

            commands["register"] = register;
            commands["login"] = login;
            commands["logout"] = logout;

            commands["list-feeds"] = listFeeds;
            commands["add-feed"] = addRSSFeed;
            commands["delete-feed"] = deleteRSSFeed;
            commands["edit-feed"] = editRSSFeed;

            commands["get-feed"] = getRSSFeedItems;
            commands["show-article"] = readArticle;

            commands["next"] = next;
            commands["quit"] = quit;

            return commands;
        }
    }
}
