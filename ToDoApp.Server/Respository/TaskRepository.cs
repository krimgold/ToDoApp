namespace ToDoApp.Server.Respository
{
	public class TaskRepository : ITaskRepository
	{
		private readonly List<ToDoTask> tasks = new List<ToDoTask>();

		public async Task AddTaskAsync(ToDoTask toDoTask, CancellationToken token = default) => tasks.Add(toDoTask);

		public async Task DeleteTaskAsync(Guid taskId, CancellationToken token = default)
		{
			var removedTask = await GetTaskAsync(taskId, token);
			
			if (removedTask is not null)
			{
				tasks.Remove(removedTask);
			}
		}

		public async Task<ToDoTask?> GetTaskAsync(Guid taskId, CancellationToken token = default) => tasks.SingleOrDefault(t => t.Id == taskId);
		public async Task UpdateTaskAsync(ToDoTask newTask, CancellationToken token = default)
		{
			await DeleteTaskAsync(newTask.Id, token);
			await AddTaskAsync(newTask, token);
		}

		public async Task<IEnumerable<ToDoTask>> GetAllTasksAsync(CancellationToken token = default) => tasks;

		public async Task<bool> ExistsAsync(Guid taskId, CancellationToken cancellationToken = default) => tasks.Exists(t => t.Id == taskId);
	}
}
