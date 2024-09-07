using ClientNetLib.Business;
using ClientNetLib.Business.Errors;
using ClientNetLib.Services.Json;
using ClientNetLib.Services.Networking;
using CommandParsonaut;
using CommandParsonaut.Core;
using CommandParsonaut.Interfaces;
using CommandParsonaut.OtherToolkit;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCommon.Models;
using Serilog;

namespace RSSFeedifyCLIClient.Business
{
    public class AccountService
    {
        private readonly IWriter _writer;
        private readonly IReader _reader;
        private readonly ApplicationErrorWriter _errorWriter;
        private readonly PasswordReader _passwordReader;

        private readonly CommandParser _parser;
        private readonly HTTPService _httpService;

        private readonly HttpResponseMessageValidator _httpResponseMessageValidatorJson = new HttpResponseMessageValidatorBuilder()
            .AddStatusCodeCheck(HTTPService.StatusCode.OK)
            .AddContentTypeCheck(HTTPService.ContentType.AppJson)
            .Build();

        private readonly HttpResponseMessageValidator _httpResponseMessageValidatorTxt = new HttpResponseMessageValidatorBuilder()
            .AddStatusCodeCheck(HTTPService.StatusCode.OK)
            .AddContentTypeCheck(HTTPService.ContentType.TxtPlain)
            .Build();

        private readonly UriResourceCreator _uriResourceCreator;

        private readonly ILogger _logger;

        public ApplicationUser User { get; private set; } = new();

        public AccountService(IWriter writer, IReader reader, ApplicationErrorWriter errorWriter, CommandParser parser, HTTPService httpService, Uri baseUri, ILogger logger)
        {
            _writer = writer;
            _reader = reader;
            _passwordReader = new(_reader, _writer);
            _errorWriter = errorWriter;
            _parser = parser;
            _httpService = httpService;
            _uriResourceCreator = new(baseUri);
            _logger = logger.ForContext<AccountService>();
        }

        public async Task Login()
        {
            var loginData = new LoginDTO();
            var email = GetEmail();

            loginData.Email = email;
            loginData.Password = GetPassword((6, 100));
            loginData.RememberMe = true;

            await Login(loginData);
        }

        public async Task Register()
        {
            var registartionData = new RegisterDTO();
            registartionData.Email = GetEmail();
            string password = GetConfirmedPassword((6, 100));
            registartionData.Password = password;
            registartionData.ConfirmPassword = password;

            var requestResult = await _httpService.PostAsync(_uriResourceCreator.BuildUri(EndPoint.ApplicationUser.ConvertToString(), "register"), JsonConvertor.ConvertObjectToJsonString(registartionData));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(Error.NetworkGeneral, requestResult.GetError));
                return;
            }

            var response = requestResult.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            var validationResult = _httpResponseMessageValidatorTxt.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(validationResult.GetError), await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var content = await HTTPService.RetrieveAndStringifyContent(response);
            _writer.RenderMessage(content);

            var loginData = new LoginDTO();
            loginData.Email = registartionData.Email;
            loginData.Password = registartionData.Password;
            loginData.RememberMe = true;

            await Login(loginData);
        }

        public async Task Logout()
        {
            var accessToken = User.GetAccessToken();
            if (accessToken.IsError)
            {
                _errorWriter.RenderErrorMessage(accessToken.GetError);
                return;
            }

            var logoutData = new LogoutDTO();
            logoutData.JWT = accessToken.GetValue;

            var requestResult = await _httpService.PostAsync(_uriResourceCreator.BuildUri(EndPoint.ApplicationUser.ConvertToString(), "logout"), JsonConvertor.ConvertObjectToJsonString(logoutData));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(Error.NetworkGeneral, requestResult.GetError));
                return;
            }

            var response = requestResult.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            var validationResult = _httpResponseMessageValidatorTxt.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(validationResult.GetError), await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var content = await HTTPService.RetrieveAndStringifyContent(response);
            _writer.RenderMessage(content);

            User.Logoff();
        }

        private async Task Login(LoginDTO loginData)
        {
            var requestResult = await _httpService.PostAsync(_uriResourceCreator.BuildUri(EndPoint.ApplicationUser.ConvertToString(), "login"), JsonConvertor.ConvertObjectToJsonString(loginData));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(Error.NetworkGeneral, requestResult.GetError));
                return;
            }

            var response = requestResult.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            var validationResult = _httpResponseMessageValidatorJson.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(validationResult.GetError), await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var jsonResult = await JsonFromHttpResponseReader.ReadJson<LoginResponseDTO>(response);
            if (jsonResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(jsonResult.GetError));
            }

            _writer.RenderMessage("You have been successfully logged in.");

            string bearerToken = jsonResult.GetValue.JWT;
            User.Email = loginData.Email;
            User.Login(bearerToken);
        }

        private string GetConfirmedPassword((int minimalLength, int maximalLength) passwordRequierements)
        {
            string password = GetPassword(passwordRequierements);
            bool firstAttempt = true;
            do
            {
                if (!firstAttempt)
                {
                    _writer.RenderErrorMessage("The passwords are not the same.");
                }

                firstAttempt = false;
            } while (!ConfirmPassword(password));

            return password;
        }

        private string GetPassword((int minimalLength, int maximalLength) passwordRequierements, bool confirmationPassword = false)
        {
            string password = string.Empty;
            bool firstAttempt = true;
            do
            {
                if (!firstAttempt)
                {
                    _writer.RenderErrorMessage($"Password does not meet the length criteria. Minimal length has to be at least {passwordRequierements.minimalLength} and maximal length is {passwordRequierements.maximalLength}.");
                }

                if (confirmationPassword)
                {
                    _writer.RenderBareText("Confirm your password: ");
                }
                else
                {
                    _writer.RenderBareText($"Enter password: ");
                }

                _writer.RenderBareText(_parser.TerminalPromt, newLine: false);
                password = ReadPassword();
                firstAttempt = false;
            } while (password.Length > passwordRequierements.maximalLength || password.Length < passwordRequierements.minimalLength);

            return password;
        }

        private string ReadPassword()
        {
            return _passwordReader.ReadPassword();
        }

        private bool ConfirmPassword(string password)
        {
            string confirmationPassword = GetPassword((6, 100), confirmationPassword: true);
            return string.Equals(password, confirmationPassword);
        }

        private string GetEmail()
        {
            string email;
            bool firstAttempt = true;
            do
            {
                if (!firstAttempt)
                {
                    _writer.RenderErrorMessage("Invalid email format!");
                }
                _writer.RenderBareText($"Enter your email: ");
                _writer.RenderBareText(_parser.TerminalPromt, newLine: false);
                _parser.GetUnprocessedInput(out email);

                firstAttempt = false;
            } while (!InputParser.ParseEmail(email, out _));
            return email;
        }
    }
}
