using CommandParsonaut.Interfaces;

namespace RSSFeedifyCLIClient.IO
{
    /// <summary>
    /// Implements CommandHewAwayTool IReader interface.
    /// </summary>
    public class Reader : IReader
    {
        public string? ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
