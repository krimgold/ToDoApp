using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using ToDoApp.Server.Mappers;
using ToDoApp.Server.Models;
using ToDoApp.Server.Services;

namespace ToDoApp.Server.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/tasks")]
	public class TaskController : ControllerBase
	{
		private readonly ITaskService _taskService;
		private readonly ILogger<TaskController> _logger;

		private const string GetTaskById = "GetTaskById";

		public TaskController(ITaskService taskService, ILogger<TaskController> logger)
		{
			_taskService = taskService;
			_logger = logger;
		}

		[HttpGet]
		[ProducesResponseType<IEnumerable<ToDoTaskDto>>(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult> GetAllTasks(CancellationToken token = default)
		{
			try
			{
				return Ok(await _taskService.GetAllTasksAsync(token));
			}
			catch (Exception ex)
			{
				_logger.LogError($"Request for tasks resulted in exception: {ex.Message}");
				return StatusCode(StatusCodes.Status500InternalServerError, $"Request for tasks resulted in exception");
			}
		}

		[HttpGet("{id:guid}", Name = GetTaskById)]
		[ProducesResponseType<ToDoTaskDto>(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult> GetTask(Guid id, CancellationToken token = default)
		{
			if (!await _taskService.ExistsAsync(id, token))
			{
				_logger.LogWarning($"Task with id {id} does not exist");
				return NotFound($"Task with id {id} does not exist");
			}

			try
			{
				return Ok(await _taskService.GetTaskAsync(id, token));
			}
			catch (Exception ex)
			{
				_logger.LogError($"Request for task with {id} resulted in exception: {ex.Message}");
				return StatusCode(StatusCodes.Status500InternalServerError, $"Request for task with {id} resulted in exception");
			}
		}

		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult> CreateTask([FromBody] ToDoTaskRequest task, CancellationToken token = default)
		{
			if ((await _taskService.GetTasksByName(task.Name, token)).Any())
			{
				_logger.LogWarning($"Task with name {task.Name} already exists");
				return BadRequest($"Task with name {task.Name} already exists");
			}

			try
			{
				var taskId = await _taskService.AddTaskAsync(task.MapToDto(), token);
				return CreatedAtRoute(GetTaskById, new { id = taskId }, taskId);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Creation of new task resulted in exception: {ex.Message}");
				return StatusCode(StatusCodes.Status500InternalServerError, $"Creation of new task resulted in exception");
			}

		}

		[HttpPut("{id:guid}")]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult> UpdateTask([FromBody] ToDoTaskRequest task, [FromRoute] Guid id, CancellationToken token = default)
		{
			if (!await _taskService.ExistsAsync(id, token))
			{
				_logger.LogWarning($"Task with id {id} does not exist");
				return BadRequest($"Task with id {id} does not exist");
			}
			
			try
			{
				var taskDto = task.MapToDto();
				taskDto.Id = id;
				await _taskService.UpdateTaskAsync(taskDto, token);
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError($"Update of the task resulted in exception: {ex.Message}");
				return StatusCode(StatusCodes.Status500InternalServerError, $"Update of the task resulted in exception");
			}

		}

		[HttpDelete("{id:guid}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult> DeleteTask(Guid id, CancellationToken token = default)
		{
			if (!await _taskService.ExistsAsync(id, token))
			{
				_logger.LogWarning($"Task with id {id} does not exist");
				return BadRequest($"Task with id {id} does not exist");
			}
			
			var task = await _taskService.GetTaskAsync(id, token);
			if (task?.Status != ToDoTaskStatus.Completed)
			{
				_logger.LogWarning($"Task with id {id} is not completed and cannot be deleted");
				return BadRequest($"Task with id {id} is not completed and cannot be deleted");
			}
			try
			{
				await _taskService.DeleteTaskAsync(id, token);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Deletion of new task resulted in exception: {ex.Message}");
				return StatusCode(StatusCodes.Status500InternalServerError, $"Deletion of new task resulted in exception");
			}

			return Ok();
		}
	}
}
