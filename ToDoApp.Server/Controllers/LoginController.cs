using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ToDoApp.Server.Authentication;
using ToDoApp.Server.Models;

namespace ToDoApp.Server.Controllers
{
	[ApiController]
	[Route("api/login")]
	public class LoginController : ControllerBase
	{
		private readonly RsaSecurityKey _secretKey;
		private readonly AuthOptions _authSettings;

		public LoginController(RsaSecurityKey secretKey, IOptions<AuthOptions> authSettings)
		{
			_secretKey = secretKey;
			_authSettings = authSettings.Value;
		}

		[HttpPost]
		[ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public IActionResult Login(TokenRequest request)
		{
			if (request.Username != _authSettings.Username || request.Password != _authSettings.Password)
				return Unauthorized();

			var signingCreds = new SigningCredentials(_secretKey,
				SecurityAlgorithms.RsaSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, request.Username)
			};

			const int tokenLifetimeMinutes = 20;

			var jwt = new JwtSecurityToken(
				issuer: _authSettings.TokenIssuer,
				audience: _authSettings.TokenIssuer,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(tokenLifetimeMinutes),
				signingCredentials: signingCreds);

			return Ok(new TokenResponse(
				new JwtSecurityTokenHandler().WriteToken(jwt),
				tokenLifetimeMinutes * 60
			));
		}
	}
}
