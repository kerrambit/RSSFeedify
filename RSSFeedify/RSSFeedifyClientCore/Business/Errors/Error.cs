namespace ClientNetLib.Business.Errors
{
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
        InvalidJsonFormat,
        ConfigFileDirectoryLoadingError,
        EnvironmentFileLoadingError,
        UnkownEnvironmentVariable
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
