using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ToDoApp.Server.Controllers;
using ToDoApp.Server.Models;
using ToDoApp.Server.Services;
using Xunit;

namespace ToDoApp.Server.Tests
{
	public class TaskControllerTests
	{
		[Fact]
		public async Task GetAll_ReturnsMappedDtos()
		{
			var dtos = new List<ToDoTaskDto> 
			{
				new ToDoTaskDto { Id = Guid.NewGuid(), Name = "Test1", Status = ToDoTaskStatus.NotStarted, Priority = 1 },
				new ToDoTaskDto { Id = Guid.NewGuid(), Name = "Test2", Status = ToDoTaskStatus.NotStarted, Priority = 1 },
				new ToDoTaskDto { Id = Guid.NewGuid(), Name = "Test3", Status = ToDoTaskStatus.NotStarted, Priority = 1 }
			};
			
			var service = new Mock<ITaskService>();
			service.Setup(s => s.GetAllTasksAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
			
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			
			var controller = new TaskController(service.Object, logger.Object);

			var result = await controller.GetAllTasks(CancellationToken.None);
			
			var response = Assert.IsType<OkObjectResult>(result);
			var tasks = Assert.IsType<List<ToDoTaskDto>>(response.Value);
			tasks.Should().HaveCount(3);
		}

		[Fact]
		public async Task GetById_ReturnsOk_WhenExists()
		{
			var id = Guid.NewGuid();
			var dto = new ToDoTaskDto { Id = id, Name = "Test", Status = ToDoTaskStatus.NotStarted, Priority = 1 };
			
			var service = new Mock<ITaskService>();
			service.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
			service.Setup(s => s.GetTaskAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
			
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			
			var controller = new TaskController(service.Object, logger.Object);

			var result = await controller.GetTask(id, CancellationToken.None);
			
			var response = Assert.IsType<OkObjectResult>(result);
			var task = Assert.IsType<ToDoTaskDto>(response.Value);
			task.Id.Should().Be(id);
		}

		[Fact]
		public async Task GetById_Returns_InternalServerError_WhenServiceThrows()
		{
			var id = Guid.NewGuid();
			var service = new Mock<ITaskService>();
			service.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
			service.Setup(s => s.GetTaskAsync(id, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			var controller = new TaskController(service.Object, logger.Object);

			var result = await controller.GetTask(id, CancellationToken.None);
			
			var response = Assert.IsType<ObjectResult>(result);
			response.StatusCode.Should().Be(500);
			response.Value.Should().Be($"Request for task with {id} resulted in exception");

			logger.Verify(l => l.Log(
				Microsoft.Extensions.Logging.LogLevel.Error,
				It.IsAny<Microsoft.Extensions.Logging.EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Request for task with {id} resulted in exception: boom")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
		}

		[Fact]
		public async Task GetAll_Returns_InternalServerError_WhenServiceThrows()
		{
			var service = new Mock<ITaskService>();
			service.Setup(s => s.GetAllTasksAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
			
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			
			var controller = new TaskController(service.Object, logger.Object);

			var result = await controller.GetAllTasks(CancellationToken.None);
			
			var response = Assert.IsType<ObjectResult>(result);
			response.StatusCode.Should().Be(500);
			response.Value.Should().Be("Request for tasks resulted in exception");

			logger.Verify(l => l.Log(
				Microsoft.Extensions.Logging.LogLevel.Error,
				It.IsAny<Microsoft.Extensions.Logging.EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Request for tasks resulted in exception: boom")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
		}

		[Fact]
		public async Task GetById_ReturnsNotFound_WhenMissing()
		{
			var service = new Mock<ITaskService>();
			var id = Guid.NewGuid();
			
			service.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
			
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			
			var controller = new TaskController(service.Object, logger.Object);

			var result = await controller.GetTask(id, CancellationToken.None);
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task Create_Returns_CreatedAtRoute()
		{
			var service = new Mock<ITaskService>();
			service.Setup(s => s.AddTaskAsync(It.IsAny<ToDoTaskDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
			
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			
			var controller = new TaskController(service.Object, logger.Object);
			var task = new ToDoTaskRequest { Name = "Test", Priority = 2, Status = ToDoTaskStatus.NotStarted };
			
			var result = await controller.CreateTask(task, CancellationToken.None);
			
			var response = Assert.IsType<CreatedAtRouteResult>(result);
			response.RouteName.Should().Be("GetTaskById");
		}

		[Fact]
		public async Task Create_Returns_InternalServerError_WhenServiceThrows()
		{
			var service = new Mock<ITaskService>();
			service.Setup(s => s.AddTaskAsync(It.IsAny<ToDoTaskDto>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));

			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();

			var controller = new TaskController(service.Object, logger.Object);

			var task = new ToDoTaskRequest { Name = "Test", Priority = 2, Status = ToDoTaskStatus.NotStarted };

			var result = await controller.CreateTask(task, CancellationToken.None);
			var response = Assert.IsType<ObjectResult>(result);
			response.StatusCode.Should().Be(500);
			response.Value.Should().Be("Creation of new task resulted in exception");

			logger.Verify(l => l.Log(
				Microsoft.Extensions.Logging.LogLevel.Error,
				It.IsAny<Microsoft.Extensions.Logging.EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creation of new task resulted in exception: boom")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
		}

		[Fact]
		public async Task Update_ReturnsOk_WhenExists()
		{
			var id = Guid.NewGuid();
			var service = new Mock<ITaskService>();
			service.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
			service.Setup(s => s.UpdateTaskAsync(It.IsAny<ToDoTaskDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			
			var controller = new TaskController(service.Object, logger.Object);

			var task = new ToDoTaskRequest { Name = "Test", Priority = 2, Status = ToDoTaskStatus.InProgress };
			var res = await controller.UpdateTask(task, id, CancellationToken.None);
			Assert.IsType<OkResult>(res);
		}

		[Fact]
		public async Task Update_Returns_BadRequest_WhenMissing()
		{
			var id = Guid.NewGuid();
			var service = new Mock<ITaskService>();
			service.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			var controller = new TaskController(service.Object, logger.Object);

			var task = new ToDoTaskRequest { Name = "Test", Priority = 2, Status = ToDoTaskStatus.InProgress };
			
			var result = await controller.UpdateTask(task, id, CancellationToken.None);
			
			var response = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains(id.ToString(), response.Value?.ToString() ?? string.Empty);
		}

		[Fact]
		public async Task Update_Returns_InternalServerError_WhenServiceThrows()
		{
			var id = Guid.NewGuid();
			
			var service = new Mock<ITaskService>();
			service.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
			service.Setup(s => s.UpdateTaskAsync(It.IsAny<ToDoTaskDto>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
			
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			
			var controller = new TaskController(service.Object, logger.Object);

			var task = new ToDoTaskRequest { Name = "Test", Priority = 2, Status = ToDoTaskStatus.InProgress };
			
			var result = await controller.UpdateTask(task, id, CancellationToken.None);
			
			var response = Assert.IsType<ObjectResult>(result);
			response.StatusCode.Should().Be(500);
			response.Value.Should().Be("Update of the task resulted in exception");

			logger.Verify(l => l.Log(
				Microsoft.Extensions.Logging.LogLevel.Error,
				It.IsAny<Microsoft.Extensions.Logging.EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Update of the task resulted in exception: boom")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
		}

		[Fact]
		public async Task Delete_Returns_BadRequest_WhenCannotDelete()
		{
			var id = Guid.NewGuid();

			var service = new Mock<ITaskService>();
			service.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
			service.Setup(s => s.GetTaskAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new ToDoTaskDto { Id = id, Name = "Test", Status = ToDoTaskStatus.InProgress, Priority = 1 });
			
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			
			var controller = new TaskController(service.Object, logger.Object);

			var result = await controller.DeleteTask(id, CancellationToken.None);
			
			var response = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("cannot be deleted", response.Value?.ToString() ?? string.Empty);
		}

		[Fact]
		public async Task Delete_Returns_BadRequest_WhenMissing()
		{
			var id = Guid.NewGuid();

			var service = new Mock<ITaskService>();
			service.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
			
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			
			var controller = new TaskController(service.Object, logger.Object);

			var result = await controller.DeleteTask(id, CancellationToken.None);
			var response = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("does not exist", response.Value?.ToString() ?? string.Empty);
		}

		[Fact]
		public async Task Delete_Returns_InternalServerError_WhenServiceThrows()
		{
			var id = Guid.NewGuid();

			var service = new Mock<ITaskService>();
			service.Setup(s => s.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
			service.Setup(s => s.GetTaskAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new ToDoTaskDto { Id = id, Name = "Test", Status = ToDoTaskStatus.Completed, Priority = 1 });
			service.Setup(s => s.DeleteTaskAsync(id, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
			
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskController>>();
			
			var controller = new TaskController(service.Object, logger.Object);

			var result = await controller.DeleteTask(id, CancellationToken.None);
			
			var response = Assert.IsType<ObjectResult>(result);
			response.StatusCode.Should().Be(500);
			response.Value.Should().Be("Deletion of new task resulted in exception");

			logger.Verify(l => l.Log(
				Microsoft.Extensions.Logging.LogLevel.Error,
				It.IsAny<Microsoft.Extensions.Logging.EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deletion of new task resulted in exception: boom")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
		}
	}
}
