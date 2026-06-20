namespace ToDoApp.Server.Models
{
	public record TokenResponse(string JwtToken, int ExpiresIn)
	{
	}
}
