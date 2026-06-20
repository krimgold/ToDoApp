using System;
using ToDoApp.Server.Mappers;
using ToDoApp.Server.Models;
using ToDoApp.Server.Respository;
using Xunit;

namespace ToDoApp.Server.Tests
{
	public class ToDoTaskMapperTests
	{
		[Fact]
		public void MapToDto_FromTask_MapsAllFields()
		{
			var task = new ToDoTask { Id = Guid.NewGuid(), Name = "Test", Status = ToDoTaskStatus.InProgress, Priority = 3 };
			var dto = task.MapToDto();
			Assert.Equal(task.Id, dto.Id);
			Assert.Equal(task.Name, dto.Name);
			Assert.Equal(task.Status, dto.Status);
			Assert.Equal(task.Priority, dto.Priority);
		}

		[Fact]
		public void MapToTask_FromDto_MapsAllFields()
		{
			var dto = new ToDoTaskDto { Id = Guid.NewGuid(), Name = "Test", Status = ToDoTaskStatus.Completed, Priority = 5 };
			var task = dto.MapToTask();
			Assert.Equal(dto.Id, task.Id);
			Assert.Equal(dto.Name, task.Name);
			Assert.Equal(dto.Status, task.Status);
			Assert.Equal(dto.Priority, task.Priority);
		}

		[Fact]
		public void MapToDto_FromRequest_SetsEmptyId_AndMapsFields()
		{
			var req = new ToDoTaskRequest { Name = "Test", Status = ToDoTaskStatus.NotStarted, Priority = 1 };
			var dto = req.MapToDto();
			Assert.Equal(Guid.Empty, dto.Id);
			Assert.Equal(req.Name, dto.Name);
			Assert.Equal(req.Status, dto.Status);
			Assert.Equal(req.Priority, dto.Priority);
		}
	}
}
