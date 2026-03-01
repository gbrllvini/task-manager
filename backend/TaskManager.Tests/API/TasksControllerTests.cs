using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services.Interfaces;
using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Tests.API;

public class TasksControllerTests {
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly TasksController _tasksController;
    private readonly Guid _userId = Guid.NewGuid();

    public TasksControllerTests() {
        _taskServiceMock = new Mock<ITaskService>();
        _tasksController = new TasksController(_taskServiceMock.Object);

        _tasksController.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, _userId.ToString())],
                    "test-auth"))
            }
        };
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOkWithPagedResult() {
        var pagedResult = new PagedResultDto<TaskResponseDto> {
            Items = [
                new TaskResponseDto {
                    Id = Guid.NewGuid(),
                    Title = "Task 1",
                    Priority = TaskPriority.Medium,
                    Status = TaskStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                }
            ],
            Page = 1,
            PageSize = 10,
            TotalItems = 1,
            TotalPages = 1
        };

        _taskServiceMock
            .Setup(service => service.GetAllAsync(_userId, It.IsAny<TaskListQueryDto>()))
            .ReturnsAsync(pagedResult);

        var result = await _tasksController.GetAllAsync(new TaskListQueryDto());

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<PagedResultDto<TaskResponseDto>>(okResult.Value);
        Assert.Single(payload.Items);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskDoesNotExist_ShouldReturnNotFound() {
        _taskServiceMock
            .Setup(service => service.GetByIdAsync(It.IsAny<Guid>(), _userId))
            .ReturnsAsync((TaskResponseDto?)null);

        var result = await _tasksController.GetByIdAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedAtRoute() {
        var createdTask = new TaskResponseDto {
            Id = Guid.NewGuid(),
            Title = "Task 1",
            Priority = TaskPriority.High,
            Status = TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _taskServiceMock
            .Setup(service => service.CreateAsync(_userId, It.IsAny<CreateTaskDto>()))
            .ReturnsAsync(createdTask);

        var result = await _tasksController.CreateAsync(new CreateTaskDto {
            Title = "Task 1",
            Priority = TaskPriority.High
        });

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        Assert.Equal("GetTaskById", createdResult.RouteName);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskExists_ShouldReturnNoContent() {
        _taskServiceMock
            .Setup(service => service.UpdateAsync(It.IsAny<Guid>(), _userId, It.IsAny<UpdateTaskDto>()))
            .ReturnsAsync(true);

        var result = await _tasksController.UpdateAsync(Guid.NewGuid(), new UpdateTaskDto {
            Title = "Updated task",
            Priority = TaskPriority.Low,
            Status = TaskStatus.InProgress
        });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskDoesNotExist_ShouldReturnNotFound() {
        _taskServiceMock
            .Setup(service => service.UpdateAsync(It.IsAny<Guid>(), _userId, It.IsAny<UpdateTaskDto>()))
            .ReturnsAsync(false);

        var result = await _tasksController.UpdateAsync(Guid.NewGuid(), new UpdateTaskDto {
            Title = "Updated task",
            Priority = TaskPriority.Low,
            Status = TaskStatus.InProgress
        });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskExists_ShouldReturnNoContent() {
        _taskServiceMock
            .Setup(service => service.DeleteAsync(It.IsAny<Guid>(), _userId))
            .ReturnsAsync(true);

        var result = await _tasksController.DeleteAsync(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }
}
