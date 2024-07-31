using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RSSFeedify.Models;
using RSSFeedify.Types;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RSSFeedify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redisConnection;

        public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IConnectionMultiplexer redisConnection)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _redisConnection = redisConnection;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                result = await _userManager.AddToRoleAsync(user, "RegularUser");
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                return Ok();
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user is null)
                    {
                        return ControllersHelper.GetResultForInvalidLoginAttempt();
                    }

                    var jwtResult = GenerateJwtToken(user, await _userManager.GetRolesAsync(user));
                    if (jwtResult.IsError)
                    {
                        return ControllersHelper.GenerateBadRequest(jwtResult.GetError);
                    }

                    return Ok(new { jwt = jwtResult.GetValue });
                }
                return ControllersHelper.GetResultForInvalidLoginAttempt();
            }
            return BadRequest(ModelState);
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout([FromBody] LogoutDTO model)
        {
            if (ModelState.IsValid)
            {
                await _signInManager.SignOutAsync();

                var db = _redisConnection.GetDatabase();
                await db.StringSetAsync(model.JWT, $"Blacklisted at {DateTime.UtcNow}.", TimeSpan.FromMinutes(Convert.ToInt32(_configuration["Jwt:ExpireMinutes"])));

                return ControllersHelper.GetResultForSuccessfulLoggedOut();
            }
            return BadRequest(ModelState);
        }

        private Result<string, string> GenerateJwtToken(ApplicationUser user, IList<string> userRoles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            if (_configuration["Jwt:Key"] is null)
            {
                return Result.Error<string, string>("Could not generate JWT Bearer.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            if (userRoles.Count <= 0)
            {
                return Result.Error<string, string>("User has no roles. Authorization process cannot be finished.");
            }

            claims.AddRange(userRoles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: creds
            );

            var stringifiedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return Result.Ok<string, string>(stringifiedToken);
        }
    }
}
