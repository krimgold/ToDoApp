using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Server.Models
{
	public class ToDoTaskRequest
	{
		[Required]
		public required string Name { get; set; }
		[Required]
		public required ToDoTaskStatus Status { get; set; }
		[Required]
		public required int Priority { get; set; }
	}
}
