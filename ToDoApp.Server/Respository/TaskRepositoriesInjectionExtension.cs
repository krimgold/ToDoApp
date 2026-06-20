namespace ToDoApp.Server.Respository
{
	public static class TaskRepositoriesInjectionExtension
	{
		public static IServiceCollection AddToDoRepositories(this IServiceCollection services)
		{
			services.AddSingleton<ITaskRepository, TaskRepository>();

			return services;
		}
	}
}
