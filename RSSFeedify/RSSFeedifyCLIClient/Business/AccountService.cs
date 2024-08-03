using CommandParsonaut.CommandHewAwayTool;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCLIClient.Models;
using RSSFeedifyClientCore;
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

            var requestResult = await _httpService.PostAsync(Endpoints.BuildUri(Endpoints.EndPoint.ApplicationUser, "login"), JsonConvertor.ConvertObjectToJsonString(loginData));

            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;

            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);

            // TODO: create system which will recognize different static codes and handle situation according to them.
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                //RenderErrorMessage(await data.response.Content.ReadAsStringAsync());
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<LoginResponseDTO>(response);
            if (result.IsError)
            {
                _errorWriter.RenderErrorMessage(result.GetError);
            }

            string bearerToken = result.GetValue.JWT;
            _writer.RenderDebugMessage($"JWT: {bearerToken}");
            User.Email = email;
            User.Login(bearerToken);
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
                _errorWriter.RenderErrorMessage(ApplicationError.Network, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                //RenderErrorMessage(await data.response.Content.ReadAsStringAsync());
                Console.WriteLine(response);
                Console.WriteLine("Response Content:");
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                return;
            }

            Console.WriteLine(response);

            //var result = await ReadJson<RSSFeed>(data.response);
            //if (result is not null)
            //{
            //    _writer.RenderBareText("New RSSFeed was registered:");
            //    RenderRSSFeed(result);
            //}
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
