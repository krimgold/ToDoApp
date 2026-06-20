namespace ToDoApp.Server.Respository
{
	public interface ITaskRepository
	{
		Task<IEnumerable<ToDoTask>> GetAllTasksAsync(CancellationToken token = default);
		Task<ToDoTask?> GetTaskAsync(Guid taskId, CancellationToken token = default);
		Task AddTaskAsync(ToDoTask toDoTask, CancellationToken token = default);
		Task DeleteTaskAsync(Guid taskId, CancellationToken token = default);
		Task<bool> ExistsAsync(Guid taskId, CancellationToken cancellationToken = default);
		Task UpdateTaskAsync(ToDoTask task, CancellationToken token = default);
	}
}
