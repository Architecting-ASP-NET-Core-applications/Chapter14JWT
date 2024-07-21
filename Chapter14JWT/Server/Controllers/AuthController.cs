using Chapter14JWT.Client.Models;
using Chapter14JWT.Server.Models;
using Chapter14JWT.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Chapter14JWT.Server.Controllers;

public static class ApiLoginController
{
    public static void AddLoginController(this IEndpointRouteBuilder app)
    {
        _ = app.MapPost("api/v1/login", (UserForAuthenticationDto userForAuthentication,
            ITokenService tokenService) =>
        {
            IResult ret = Results.Ok();
            var user = new User()
            {
                UserName = userForAuthentication.Username,
                Email = userForAuthentication.Username
            };

            try
            {
                // User verification
                user = tokenService.VerifyUserIdentity(userForAuthentication);
                if (user.Token != null)
                {
                    ret = Results.Ok(new AuthenticationResponseDto
                    {
                        IsAuthenticationSuccessful = true,
                        Token = user.Token,
                        RefreshToken = user.RefreshToken,
                        StatusCode = HttpStatusCode.OK
                    });
                }
                else
                {
                    ret = Results.Unauthorized();
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                s += "";
            }
            return ret;
        });
    }
}


//[ApiController]
//[Route("[controller]")]
//public class AuthController : ControllerBase
//{
//    private readonly IConfiguration _configuration;

//    public AuthController(IConfiguration configuration)
//    {
//        _configuration = configuration;
//    }

//    [AllowAnonymous]
//    [HttpPost("login")]
//    public IActionResult Login([FromBody] UserLogin userLogin)
//    {
//        if (userLogin.Username == "test" && userLogin.Password == "password")
//        {
//            var token = GenerateJwtToken();
//            return Ok(new { Token = token });
//        }

//        return Unauthorized();
//    }

//    private string GenerateJwtToken()
//    {
//        var jwtSettings = _configuration.GetSection("Jwt");
//        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

//        var tokenDescriptor = new SecurityTokenDescriptor
//        {
//            Subject = new ClaimsIdentity(new[] { new Claim("id", "1") }),
//            Expires = DateTime.UtcNow.AddHours(1),
//            Issuer = jwtSettings["Issuer"],
//            Audience = jwtSettings["Audience"],
//            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//        };

//        var tokenHandler = new JwtSecurityTokenHandler();
//        var token = tokenHandler.CreateToken(tokenDescriptor);
//        return tokenHandler.WriteToken(token);
//    }
//}

//public class UserLogin
//{
//    public string Username { get; set; }
//    public string Password { get; set; }
//}

