using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController : ControllerBase {
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService) {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TaskResponseDto>>> GetAllAsync([FromQuery] TaskListQueryDto query) {
        var userId = GetUserId();
        
        var tasks = await _taskService.GetAllAsync(userId, query);

        return Ok(tasks);
    }

    [HttpGet("{id:guid}", Name = "GetTaskById")]
    public async Task<ActionResult<TaskResponseDto>> GetByIdAsync(Guid id) {
        var userId = GetUserId();

        var task = await _taskService.GetByIdAsync(id, userId);

        if (task is null) {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponseDto>> CreateAsync([FromBody] CreateTaskDto createTaskDto) {
        var userId = GetUserId();

        var createdTask = await _taskService.CreateAsync(userId, createTaskDto);

        return CreatedAtRoute("GetTaskById", new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateTaskDto updateTaskDto) {
        var userId = GetUserId();

        var updated = await _taskService.UpdateAsync(id, userId, updateTaskDto);

        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id) {
        var userId = GetUserId();

        var deleted = await _taskService.DeleteAsync(id, userId);

        return deleted ? NoContent() : NotFound();
    }

    private Guid GetUserId() {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(claimValue, out var userId)) {
            throw new UnauthorizedAccessException("Token invalido.");
        }

        return userId;
    }
}