using CommandParsonaut.Interfaces;

namespace RSSFeedifyCLIClient.IO
{
    /// <summary>
    /// Implements ComamndParsonaut IWriter interface.
    /// </summary>
    public class Writer : IWriter
    {
        public string Name { get; set; }

        public Writer()
        {
            Name = "TERMINAL";
        }

        public void RenderWarningMessage(in string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{Name}][Warning]: {message}");
            Console.ResetColor();
        }

        public void RenderErrorMessage(in string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{Name}][Error]: {message}");
            Console.ResetColor();
        }

        public void RenderDebugMessage(in string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"[{Name}][Debug]: {message}");
            Console.ResetColor();
        }

        public void RenderMessage(in string message)
        {
            Console.WriteLine($"[{Name}]: {message}");
        }

        public void RenderBareText(in string message, bool newLine = true)
        {
            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message);
        }
    }
}
