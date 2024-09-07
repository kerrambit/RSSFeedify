using ClientNetLib.Business.Errors;
using ClientNetLib.Types;
using static ClientNetLib.Services.Networking.HTTPService;

namespace ClientNetLib.Services.Networking
{
    public class HttpResponseMessageValidator
    {
        private StatusCode _exptectedStatusCode = StatusCode.OK;
        private ContentType _exptectedContentType = ContentType.TxtPlain;

        public HttpResponseMessageValidator() { }

        public HttpResponseMessageValidator(StatusCode exptectedStatusCode, ContentType exptectedContentType)
        {
            _exptectedStatusCode = exptectedStatusCode;
            _exptectedContentType = exptectedContentType;
        }

        public void SetExpectedStatusCode(StatusCode statusCode)
        {
            _exptectedStatusCode = statusCode;
        }

        public void SetExpectedContentType(ContentType contentType)
        {
            _exptectedContentType = contentType;
        }

        public Result<string, DetailedError> Validate(HttpServiceResponseMessageMetaData httpResponseMessage)
        {
            switch (httpResponseMessage.StatusCode)
            {
                case StatusCode.BadRequest:
                    return Result.Error<string, DetailedError>(Error.NetworkBadRequest.ConvertToDetailedApplicationError());
                case StatusCode.Unauthorized:
                    return Result.Error<string, DetailedError>(Error.NetworkUnauthorized.ConvertToDetailedApplicationError());
                case StatusCode.Forbidden:
                    return Result.Error<string, DetailedError>(Error.NetworkForbidden.ConvertToDetailedApplicationError());
                case StatusCode.NotFound:
                    return Result.Error<string, DetailedError>(Error.NetworkNotFound.ConvertToDetailedApplicationError());
                case StatusCode.InternalServerError:
                    return Result.Error<string, DetailedError>(Error.NetworkServerError.ConvertToDetailedApplicationError());
            }

            if (httpResponseMessage.StatusCode != _exptectedStatusCode)
            {
                return Result.Error<string, DetailedError>(Error.NetworkUnexpectedStatusCode.ConvertToDetailedApplicationError($"Expected status code {_exptectedStatusCode}, actual: {httpResponseMessage.StatusCode}."));
            }

            if (httpResponseMessage.ContentType != _exptectedContentType)
            {
                return Result.Error<string, DetailedError>(Error.NetworkUnexpectedDataType.ConvertToDetailedApplicationError($"Expected content type {_exptectedContentType}, actual: {httpResponseMessage.ContentType}."));
            }

            return Result.Ok<string, DetailedError>($"HttpResponseMessage is valid (StatusCode: {_exptectedStatusCode}, ContentType: {_exptectedContentType}).");
        }
    }

    public class HttpResponseMessageValidatorBuilder
    {
        private HttpResponseMessageValidator _validator = new();

        public HttpResponseMessageValidatorBuilder()
        {
            Reset();
        }

        public void Reset()
        {
            _validator = new();
        }

        public HttpResponseMessageValidatorBuilder AddStatusCodeCheck(StatusCode expected)
        {
            _validator.SetExpectedStatusCode(expected);
            return this;
        }

        public HttpResponseMessageValidatorBuilder AddContentTypeCheck(ContentType expected)
        {
            _validator.SetExpectedContentType(expected);
            return this;
        }

        public HttpResponseMessageValidator Build()
        {
            var toReturn = _validator;
            Reset();
            return toReturn;
        }
    }
}
