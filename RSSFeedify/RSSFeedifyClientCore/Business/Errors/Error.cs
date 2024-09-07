namespace ClientNetLib.Business.Errors
{
    [Flags]
    public enum Error
    {
        General,
        NetworkGeneral,
        NetworkUnexpectedStatusCode,
        NetworkBadRequest,
        NetworkForbidden,
        NetworkNotFound,
        NetworkUnauthorized,
        NetworkNotSupported,
        NetworkServerError,
        NetworkUnexpectedDataType,
        InvalidJsonFormat
    }

    public static class ApplicationErrorExtensions
    {
        public static DetailedError ConvertToDetailedApplicationError(this Error error, string details)
        {
            return new DetailedError(error, details);
        }

        public static DetailedError ConvertToDetailedApplicationError(this Error error)
        {
            return new DetailedError(error, string.Empty);
        }
    }
}
