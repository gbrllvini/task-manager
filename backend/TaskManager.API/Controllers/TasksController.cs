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
        var tasks = await _taskService.GetAllAsync(status);
        return Ok(tasks);
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
        var createdTask = await _taskService.CreateAsync(createTaskDto);
        return CreatedAtRoute("GetTaskById", new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateTaskDto updateTaskDto) {
        var updated = await _taskService.UpdateAsync(id, updateTaskDto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id) {
        var deleted = await _taskService.DeleteAsync(id);
        
        return deleted ? NoContent() : NotFound();
    }
}