using System.Runtime.ConstrainedExecution;

namespace ToDoApp.Server.Models
{
	public class ToDoTaskDto
	{
		public Guid Id { get; set; } 
		public required string Name { get; init; }
		public required ToDoTaskStatus Status { get; init; }
		public required int Priority { get; init; }
	}
}
