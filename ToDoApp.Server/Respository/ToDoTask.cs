using ToDoApp.Server.Models;

namespace ToDoApp.Server.Respository
{
	public record ToDoTask
	{
		public required Guid Id { get; init; }
		public required string Name { get; init; }
		public required ToDoTaskStatus Status { get; init; }
		public required int Priority { get; init; }
	}
}
