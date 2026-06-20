using ToDoApp.Server.Models;

namespace ToDoApp.Server.Services
{
	public interface ITaskService
	{
		Task<IEnumerable<ToDoTaskDto>> GetAllTasksAsync(CancellationToken token = default);
		Task<ToDoTaskDto?> GetTaskAsync(Guid taskId, CancellationToken token = default);
		Task<Guid> AddTaskAsync(ToDoTaskDto toDoTask, CancellationToken token = default);
		Task DeleteTaskAsync(Guid taskId, CancellationToken token = default);
		Task<bool> ExistsAsync(Guid taskId, CancellationToken token = default);
		Task UpdateTaskAsync(ToDoTaskDto task, CancellationToken token = default);
		Task<IEnumerable<ToDoTaskDto>> GetTasksByName(string name, CancellationToken token = default);

	}
}
