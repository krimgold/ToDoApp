
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using ToDoApp.Server.Authentication;
using ToDoApp.Server.Respository;
using ToDoApp.Server.Services;

namespace ToDoApp.Server
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddLogging(builder => builder.AddConsole());
			builder.Services.AddToDoRepositories();
			builder.Services.AddToDoServices();

			builder.Services
				.AddControllers()
				.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

			builder.Services.AddOpenApi();

			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
				{
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT",
					Description = "JWT Authorization header using the Bearer scheme."
				});

				options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
				{
					[new OpenApiSecuritySchemeReference("bearer", document)] = []
				});
			});

			//Authentication / Authorization settings
			builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.AuthSection));

			using var rsa = RSA.Create(2048);
			var secretKey = new RsaSecurityKey(rsa.ExportParameters(true));
			builder.Services.AddSingleton(secretKey);

			builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = builder.Configuration.GetValue<string>($"{AuthOptions.AuthSection}:{AuthOptions.TokenIssuerSection}"),    
					ValidAudience = builder.Configuration.GetValue<string>($"{AuthOptions.AuthSection}:{AuthOptions.TokenIssuerSection}"), 
					IssuerSigningKey = secretKey
				};
			});
			builder.Services.AddAuthorization();

			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowFrontend", policy =>
				{
					policy.WithOrigins("https://localhost:55913", "https://localhost:5173")
						  .AllowAnyHeader()
						  .AllowAnyMethod();
				});
			});

			var app = builder.Build();

			app.UseDefaultFiles();
			app.MapStaticAssets();

			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();
			app.UseCors("AllowFrontend");

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllers();

			app.MapFallbackToFile("/index.html");

			app.Run();
		}
	}
}
