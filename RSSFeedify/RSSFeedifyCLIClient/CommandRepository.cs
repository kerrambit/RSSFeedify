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

            Command listStations = new Command("list-stations", "", "Lists all the stations.", new List<ParameterType> { });
            Command listUnits = new Command("list-units", "", "Lists all the units.", new List<ParameterType> { });
            Command listIncidents = new Command("list-incidents", "", "Lists all the current running incidents.", new List<ParameterType>() { });
            Command addMember = new Command("add-member", "station:STRING unit:STRING name:STRING", "Adds member with a given name to the unit.", new List<ParameterType> { ParameterType.String, ParameterType.String, ParameterType.String, });
            Command removeMember = new Command("remove-member", "station:STRING unit:STRING member:STRING", "Removes member from unit.", new List<ParameterType> { ParameterType.String, ParameterType.String, ParameterType.String, });
            Command reassignMember = new Command("reassign-member", "station-from:STRING unit-from:STRING member:STRING station-to:STRING unit-to:STRING", "Reassigns member between units and stations.", new List<ParameterType> { ParameterType.String, ParameterType.String, ParameterType.String, ParameterType.String, ParameterType.String });
            Command reassignUnit = new Command("reassign-unit", "station-from:STRING unit-from:STRING station-to:STRING", "Reassigns unit.", new List<ParameterType> { ParameterType.String, ParameterType.String, ParameterType.String });
            Command statistics = new Command("statistics", "", "Shows the statistics of the repository.", new List<ParameterType>());
            Command quit = new Command("quit", "", "Quits the application.", new List<ParameterType> { });

            commands["list-stations"] = listStations;
            commands["list-units"] = listUnits;
            commands["list-incidents"] = listIncidents;
            commands["add-member"] = addMember;
            commands["remove-member"] = removeMember;
            commands["reassign-member"] = reassignMember;
            commands["reassign-unit"] = reassignUnit;
            commands["statistics"] = statistics;
            commands["quit"] = quit;

            return commands;
        }
    }
}
