namespace ClientNetLib.Business.Errors
{
    public record DetailedError(Error Error, string Details)
    {
        public bool HasDetails()
        {
            return Details.Length != 0;
        }
    }
}
