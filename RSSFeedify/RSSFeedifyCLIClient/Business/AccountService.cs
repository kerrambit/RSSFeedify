using CommandParsonaut.CommandHewAwayTool;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCLIClient.Models;
using RSSFeedifyClientCore.Business;
using RSSFeedifyClientCore.Business.Errors;
using RSSFeedifyClientCore.Services;
using RSSFeedifyClientCore.Services.Networking;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RSSFeedifyCLIClient.Business
{
    public class AccountService
    {
        private IWriter _writer;
        private IReader _reader;
        private ApplicationErrorWriter _errorWriter;

        private CommandParser _parser;
        private HTTPService _httpService;

        private HttpResponseMessageValidator _httpResponseMessageValidatorJson = new HttpResponseMessageValidatorBuilder()
            .AddStatusCodeCheck(HTTPService.StatusCode.OK)
            .AddContentTypeCheck(HTTPService.ContentType.AppJson)
            .Build();

        private HttpResponseMessageValidator _httpResponseMessageValidatorTxt = new HttpResponseMessageValidatorBuilder()
            .AddStatusCodeCheck(HTTPService.StatusCode.OK)
            .AddContentTypeCheck(HTTPService.ContentType.TxtPlain)
            .Build();

        public ApplicationUser User { get; private set; } = new();

        public AccountService(IWriter writer, IReader reader, ApplicationErrorWriter errorWriter, CommandParser parser, HTTPService httpService)
        {
            _writer = writer;
            _reader = reader;
            _errorWriter = errorWriter;
            _parser = parser;
            _httpService = httpService;
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

            var requestResult = await _httpService.PostAsync(Endpoints.BuildUri(Endpoints.EndPoint.ApplicationUser, "register"), JsonConvertor.ConvertObjectToJsonString(registartionData));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.NetworkGeneral, requestResult.GetError);
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
                _errorWriter.RenderErrorMessage(validationResult.GetError, await HTTPService.RetrieveAndStringifyContent(response));
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

            var requestResult = await _httpService.PostAsync(Endpoints.BuildUri(Endpoints.EndPoint.ApplicationUser, "logout"), JsonConvertor.ConvertObjectToJsonString(logoutData));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.NetworkGeneral, requestResult.GetError);
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
                _errorWriter.RenderErrorMessage(validationResult.GetError, await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var content = await HTTPService.RetrieveAndStringifyContent(response);
            _writer.RenderMessage(content);

            User.Logoff();
        }

        private async Task Login(LoginDTO loginData)
        {
            var requestResult = await _httpService.PostAsync(Endpoints.BuildUri(Endpoints.EndPoint.ApplicationUser, "login"), JsonConvertor.ConvertObjectToJsonString(loginData));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.NetworkGeneral, requestResult.GetError);
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
                _errorWriter.RenderErrorMessage(validationResult.GetError, await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var jsonResult = await JsonFromHttpResponseReader.ReadJson<LoginResponseDTO>(response);
            if (jsonResult.IsError)
            {
                _errorWriter.RenderErrorMessage(jsonResult.GetError);
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

                password = ReadPassword();
                firstAttempt = false;
            } while (password.Length > passwordRequierements.maximalLength || password.Length < passwordRequierements.minimalLength);

            return password;
        }

        private string ReadPassword()
        {
            string password = string.Empty;
            ConsoleKeyInfo key;

            _writer.RenderBareText(">>> ", false);
            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    _writer.RenderBareText("*", newLine: false);
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Remove(password.Length - 1);

                    Console.CursorLeft--;
                    _writer.RenderBareText(" ", newLine: false);
                    Console.CursorLeft--;
                }

            } while (key.Key != ConsoleKey.Enter);

            _writer.RenderBareText("");
            return password;
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
                _parser.GetUnprocessedInput(out email);

                firstAttempt = false;
            } while (!IsEmailValid(email));

            return email;
        }

        // Implementation from https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format.
        private bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                string DomainMapper(Match match)
                {
                    var idn = new IdnMapping();
                    string domainName = idn.GetAscii(match.Groups[2].Value);
                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
