using CommandParsonaut.Interfaces;

namespace RSSFeedifyCLIClient.IO
{
    /// <summary>
    /// Implements CommandHewAwayTool IWriter interface.
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
            Console.WriteLine($"[{Name}][Warning]: {message}");
        }

        public void RenderErrorMessage(in string message)
        {
            Console.WriteLine($"[{Name}][Error]: {message}");
        }

        public void RenderDebugMessage(in string message)
        {
            Console.WriteLine($"[{Name}][Debug]: {message}");
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
