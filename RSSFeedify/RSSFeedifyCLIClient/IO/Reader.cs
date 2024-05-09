using CommandParsonaut.Interfaces;

namespace RSSFeedifyCLIClient.IO
{
    /// <summary>
    /// Implements ComamndParsonaut IReader interface.
    /// </summary>
    public class Reader : IReader
    {
        public string? ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
