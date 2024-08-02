using CommandParsonaut.CommandHewAwayTool;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.Models;
using RSSFeedifyClientCore;
using System.Globalization;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RSSFeedifyCLIClient.Business
{
    public class AccountService
    {
        private IWriter _writer;
        private IReader _reader;
        private CommandParser _parser;
        private HTTPService _httpService;

        public AccountService(IWriter writer, IReader reader, CommandParser parser, HTTPService httpService)
        {
            _writer = writer;
            _reader = reader;
            _parser = parser;
            _httpService = httpService;
        }

        public async Task Login()
        {

            var loginData = new LoginDTO();
            loginData.Email = "marek.eibel@seznam.com";
            loginData.Password = "Aa*12345";
            loginData.RememberMe = true;

            var data = await _httpService.PostAsync(Endpoints.BuildUri(Endpoints.EndPoint.ApplicationUser, "login"), JsonConvertor.ConvertObjectToJsonString(loginData));

            Console.WriteLine(JsonConvertor.ConvertObjectToJsonString(loginData));

            if (!data.success)
            {
                //RenderErrorMessage(Error.Network);
                return;
            }
            Console.WriteLine(data.response);
            Console.WriteLine("Response Content:");
            string responseContent = await data.response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
            if (data.response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                //RenderErrorMessage(await data.response.Content.ReadAsStringAsync());
                
                return;
            }

            Console.WriteLine(data.response);

            //var result = await ReadJson<RSSFeed>(data.response);
            //if (result is not null)
            //{
            //    _writer.RenderBareText("New RSSFeed was registered:");
            //    RenderRSSFeed(result);
            //}
        }

        public async Task Register()
        {
            string email = GetEmail();

            string password = GetPassword((6, 100));
            bool firstAttempt = true;
            do
            {
                if (!firstAttempt)
                {
                    _writer.RenderErrorMessage("The passwords are not the same.");
                }

                firstAttempt = false;
            } while (!ConfirmPassword(password));

            _writer.RenderDebugMessage("Registration starts...");
            _writer.RenderDebugMessage($"Email: {email}");
            _writer.RenderDebugMessage($"Password: {password}");

            var registartionData = new RegisterDTO();
            registartionData.Email = email;
            registartionData.Password = password;
            registartionData.ConfirmPassword = password;

            var data = await _httpService.PostAsync(Endpoints.BuildUri(Endpoints.EndPoint.ApplicationUser, "register"), JsonConvertor.ConvertObjectToJsonString(registartionData));

            Console.WriteLine(JsonConvertor.ConvertObjectToJsonString(registartionData));

            if (!data.success)
            {
                //RenderErrorMessage(Error.Network);
                return;
            }
            if (data.response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                //RenderErrorMessage(await data.response.Content.ReadAsStringAsync());
                Console.WriteLine(data.response);
                Console.WriteLine("Response Content:");
                string responseContent = await data.response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                return;
            }

            Console.WriteLine(data.response);

            //var result = await ReadJson<RSSFeed>(data.response);
            //if (result is not null)
            //{
            //    _writer.RenderBareText("New RSSFeed was registered:");
            //    RenderRSSFeed(result);
            //}
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
            }
            while (password.Length > passwordRequierements.maximalLength || password.Length < passwordRequierements.minimalLength);

            return password;
        }

        private string ReadPassword()
        {
            string password = string.Empty;
            ConsoleKeyInfo key;

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
