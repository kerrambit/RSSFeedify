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

        public int ReadKey()
        {
            return (int)Console.ReadKey().Key;
        }

        ConsoleKeyInfo IReader.ReadKey()
        {
            return Console.ReadKey();
        }

        public ConsoleKeyInfo ReadSecretKey()
        {
            return Console.ReadKey(intercept: true);
        }

        public void CursorLeft()
        {
            Console.CursorLeft--;
        }

        public void CursorRight()
        {
            Console.CursorLeft++;
        }

        public bool IsAnyKeyAvailable()
        {
            return Console.KeyAvailable;
        }

        public int GetCursorLeftPosition()
        {
            return Console.CursorLeft;
        }
    }
}
