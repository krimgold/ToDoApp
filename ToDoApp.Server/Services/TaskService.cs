using ToDoApp.Server.Mappers;
using ToDoApp.Server.Models;
using ToDoApp.Server.Respository;

namespace ToDoApp.Server.Services
{
	public class TaskService : ITaskService
	{
		private readonly ITaskRepository _taskRepository;

		public TaskService(ITaskRepository taskRepository)
		{
			_taskRepository = taskRepository;
		}

		public async Task<Guid> AddTaskAsync(ToDoTaskDto toDoTaskDto, CancellationToken token = default)
		{
			toDoTaskDto.Id = Guid.NewGuid();
			var newTask = toDoTaskDto.MapToTask();
			await _taskRepository.AddTaskAsync(newTask, token);
			return newTask.Id;
		}

		public async Task UpdateTaskAsync(ToDoTaskDto toDoTaskDto, CancellationToken token = default)
		{
			var updatedTask = toDoTaskDto.MapToTask();
			await _taskRepository.UpdateTaskAsync(updatedTask, token);
		}

		public async Task DeleteTaskAsync(Guid taskId, CancellationToken token = default) => await _taskRepository.DeleteTaskAsync(taskId, token);

		public async Task<ToDoTaskDto?> GetTaskAsync(Guid taskId, CancellationToken token = default) => (await _taskRepository.GetTaskAsync(taskId, token))?.MapToDto();

		public async Task<IEnumerable<ToDoTaskDto>> GetAllTasksAsync(CancellationToken token = default) 
			=> (await _taskRepository.GetAllTasksAsync(token)).Select(t => t.MapToDto()).OrderBy(t => t.Priority);

		public async Task<bool> ExistsAsync(Guid taskId, CancellationToken token = default) => await _taskRepository.ExistsAsync(taskId, token);
	}
}
