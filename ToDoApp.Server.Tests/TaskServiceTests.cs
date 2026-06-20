using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using ToDoApp.Server.Respository;
using ToDoApp.Server.Services;
using ToDoApp.Server.Models;
using Xunit;
using System.Collections.Generic;

namespace ToDoApp.Server.Tests
{
	public class TaskServiceTests
	{
		[Fact]
		public async Task AddTaskAsync_AssignsId_AndCallsRepository()
		{
			var repository = new Mock<ITaskRepository>();
			repository.Setup(r => r.AddTaskAsync(It.IsAny<ToDoTask>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			
			var service = new TaskService(repository.Object);

			var task = new ToDoTaskDto { Name = "Test", Status = ToDoTaskStatus.NotStarted, Priority = 1 };
			var id = await service.AddTaskAsync(task);

			id.Should().NotBe(Guid.Empty);
			repository.Verify(r => r.AddTaskAsync(It.Is<ToDoTask>(t => t.Name == "Test" && t.Priority == 1), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetTaskAsync_ReturnsMappedDto_WhenRepositoryHasTask()
		{
			var id = Guid.NewGuid();
			var task = new ToDoTask { Id = id, Name = "Test", Status = ToDoTaskStatus.Completed, Priority = 3 };
			
			var repository = new Mock<ITaskRepository>();
			repository.Setup(r => r.GetTaskAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
			
			var service = new TaskService(repository.Object);

			var result = await service.GetTaskAsync(id);
			result.Should().NotBeNull();
			result!.Id.Should().Be(task.Id);
			result.Name.Should().Be(task.Name);
			result.Status.Should().Be(task.Status);
			result.Priority.Should().Be(task.Priority);
		}

		[Fact]
		public async Task UpdateTaskAsync_CallsRepository()
		{
			var repository = new Mock<ITaskRepository>();
			repository.Setup(r => r.UpdateTaskAsync(It.IsAny<ToDoTask>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			var service = new TaskService(repository.Object);

			var task = new ToDoTaskDto { Id = Guid.NewGuid(), Name = "Test", Status = ToDoTaskStatus.InProgress, Priority = 2 };
			await service.UpdateTaskAsync(task);

			repository.Verify(r => r.UpdateTaskAsync(It.Is<ToDoTask>(t => t.Id == task.Id && t.Status == task.Status), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetAllTasksAsync_MapsAndOrdersByPriority()
		{
			var tasks = new List<ToDoTask> {
				new ToDoTask { Id = Guid.NewGuid(), Name = "Test1", Status = ToDoTaskStatus.NotStarted, Priority = 1 },
				new ToDoTask { Id = Guid.NewGuid(), Name = "Test2", Status = ToDoTaskStatus.NotStarted, Priority = 5 }
			};
			var repository = new Mock<ITaskRepository>();
			repository.Setup(r => r.GetAllTasksAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tasks);
			var service = new TaskService(repository.Object);

			var result = (await service.GetAllTasksAsync()).ToList();
			result.Should().HaveCount(2);
			result.First().Priority.Should().BeLessOrEqualTo(result.Last().Priority);
		}

		[Fact]
		public async Task ExistsAsync_DelegatesToRepository()
		{
			var repository = new Mock<ITaskRepository>();
			var id = Guid.NewGuid();
			repository.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
			var service = new TaskService(repository.Object);

			var exists = await service.ExistsAsync(id);
			exists.Should().BeTrue();
		}
	}
}
