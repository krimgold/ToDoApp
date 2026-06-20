using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ToDoApp.Server.Services
{
	public static class TaskServicesInjectionExtension
	{
		public static IServiceCollection AddToDoServices(this IServiceCollection services)
		{
			services.AddSingleton<ITaskService, TaskService>();

			return services;
		}
	}
}
