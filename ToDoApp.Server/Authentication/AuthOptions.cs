namespace ToDoApp.Server.Authentication
{
	public class AuthOptions
	{
		public const string AuthSection = "Auth";
		public const string UsernameSection = "Username";
		public const string PasswordSection = "Password";
		public const string TokenIssuerSection = "TokenIssuer";


		public string? Username { get; set; }
		public string? Password { get; set; }
		public string? TokenIssuer { get; set; }
	}
}
