using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ToDoApp.Server.Controllers;
using ToDoApp.Server.Models;
using Xunit;

namespace ToDoApp.Server.Tests
{
	public class LoginControllerTests
	{
		[Fact]
		public void Login_ReturnsTokenResponse_OnValidCredentials()
		{
			var authOptions = new Authentication.AuthOptions { Username = "user", Password = "pass", TokenIssuer = "test" };
			var options = Options.Create(authOptions);
			using var rsa = System.Security.Cryptography.RSA.Create(2048);
			var key = new RsaSecurityKey(rsa);
			var controller = new LoginController(key, options);

			var req = new TokenRequest("user","pass");
			var result = controller.Login(req);
			var ok = Assert.IsType<OkObjectResult>(result);
			var tr = Assert.IsType<TokenResponse>(ok.Value);
			tr.JwtToken.Should().NotBeNullOrWhiteSpace();

			var handler = new JwtSecurityTokenHandler();
			handler.ReadJwtToken(tr.JwtToken); // should not throw
		}

		[Fact]
		public void Login_ReturnsUnauthorized_OnInvalidCredentials()
		{
			var authOptions = new Authentication.AuthOptions { Username = "user", Password = "pass", TokenIssuer = "test" };
			var options = Options.Create(authOptions);
			using var rsa = System.Security.Cryptography.RSA.Create(2048);
			var key = new RsaSecurityKey(rsa);
			var controller = new LoginController(key, options);

			var req = new TokenRequest("bad","creds");
			var result = controller.Login(req);
			Assert.IsType<UnauthorizedResult>(result);
		}
	}
}
