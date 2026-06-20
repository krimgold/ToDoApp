using ToDoApp.Server.Models;
using ToDoApp.Server.Respository;

namespace ToDoApp.Server.Mappers
{
	public static class ToDoTaskMapper
	{
		public static ToDoTaskDto MapToDto(this ToDoTask task) 
			=> new ToDoTaskDto 
			{ 
				Id = task.Id, 
				Name = task.Name, 
				Status = task.Status, 
				Priority = task.Priority 
			};

		public static ToDoTask MapToTask(this ToDoTaskDto taskDto) 
			=> new ToDoTask
			{
				Id = taskDto.Id,
				Name = taskDto.Name,
				Status = taskDto.Status,
				Priority= taskDto.Priority
			};

		public static ToDoTaskDto MapToDto(this ToDoTaskRequest taskDto)
			=> new ToDoTaskDto
			{
				Id = Guid.Empty,
				Name = taskDto.Name,
				Status = taskDto.Status,
				Priority = taskDto.Priority
			};
	}
}
