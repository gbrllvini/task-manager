using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Tests.Application;

public class TaskServiceTests {
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly TaskService _taskService;

    public TaskServiceTests() {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _taskService = new TaskService(_taskRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldApplyFiltersSortAndPagination() {
        var taskA = new TaskItem("Task A", "Desc A", TaskPriority.High, DateTime.UtcNow.AddDays(2));
        var taskB = new TaskItem("Task B", "Desc B", TaskPriority.High, DateTime.UtcNow.AddDays(3));
        var taskC = new TaskItem("Task C", "Desc C", TaskPriority.Low, DateTime.UtcNow.AddDays(4));
        taskC.UpdateStatus(TaskStatus.InProgress);

        _taskRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync([taskB, taskC, taskA]);

        var query = new TaskListQueryDto {
            Status = TaskStatus.Pending,
            Priority = TaskPriority.High,
            SortBy = "title",
            SortDirection = "asc",
            Page = 1,
            PageSize = 1
        };

        var result = await _taskService.GetAllAsync(query);

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal("Task A", result.Items.Single().Title);
    }

    [Fact]
    public async Task GetAllAsync_WhenPageIsInvalid_ShouldThrowArgumentException() {
        var query = new TaskListQueryDto { Page = 0 };

        await Assert.ThrowsAsync<ArgumentException>(() => _taskService.GetAllAsync(query));
    }

    [Fact]
    public async Task CreateAsync_WhenTitleIsInvalid_ShouldThrowArgumentException() {
        var createTaskDto = new CreateTaskDto {
            Title = "  ",
            Description = "Description",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _taskService.CreateAsync(createTaskDto));
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskDoesNotExist_ShouldReturnFalse() {
        _taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((TaskItem?)null);

        var updateTaskDto = new UpdateTaskDto {
            Title = "Updated title",
            Description = "Updated description",
            Priority = TaskPriority.High,
            Status = TaskStatus.InProgress,
            DueDate = DateTime.UtcNow.AddDays(2)
        };

        var result = await _taskService.UpdateAsync(Guid.NewGuid(), updateTaskDto);

        Assert.False(result);
        _taskRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskDoesNotExist_ShouldReturnFalse() {
        _taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((TaskItem?)null);

        var result = await _taskService.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
        _taskRepositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<TaskItem>()), Times.Never);
    }
}