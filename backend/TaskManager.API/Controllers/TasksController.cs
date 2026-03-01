using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services.Interfaces;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase {
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService) {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAllAsync([FromQuery] TaskStatus? status) {
        try {
            var tasks = await _taskService.GetAllAsync(status);

            return Ok(tasks);
        }
        catch (ArgumentException ex) {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}", Name = "GetTaskById")]
    public async Task<ActionResult<TaskResponseDto>> GetByIdAsync(Guid id) {
        var task = await _taskService.GetByIdAsync(id);

        if (task is null) {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponseDto>> CreateAsync([FromBody] CreateTaskDto createTaskDto) {
        try {
            var createdTask = await _taskService.CreateAsync(createTaskDto);

            return CreatedAtRoute("GetTaskById", new { id = createdTask.Id }, createdTask);
        }
        catch (ArgumentException ex) {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateTaskDto updateTaskDto) {
        try {
            var updated = await _taskService.UpdateAsync(id, updateTaskDto);

            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex) {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id) {
        var deleted = await _taskService.DeleteAsync(id);
        
        return deleted ? NoContent() : NotFound();
    }
}