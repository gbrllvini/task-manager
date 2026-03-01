using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services.Interfaces;

namespace TaskManager.Tests.API;

public class AuthControllerTests {
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests() {
        _authServiceMock = new Mock<IAuthService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _refreshTokenServiceMock.Setup(service => service.GetRefreshTokenDays()).Returns(30);

        _controller = new AuthController(_authServiceMock.Object, _refreshTokenServiceMock.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnOkAndSetRefreshCookie() {
        var session = new AuthSessionDto {
            RefreshToken = "raw-refresh-token",
            Response = new AuthResponseDto {
                AccessToken = "access-token",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
                User = new CurrentUserDto {
                    Id = Guid.NewGuid(),
                    DisplayName = "Teste",
                    Email = "teste@local"
                }
            }
        };

        _authServiceMock
            .Setup(service => service.RegisterAsync(It.IsAny<RegisterRequestDto>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _controller.RegisterAsync(new RegisterRequestDto {
            DisplayName = "Teste",
            Email = "teste@local",
            Password = "senha123"
        }, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("access-token", payload.AccessToken);
        Assert.Contains("tm_refresh_token", _controller.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task MeAsync_WhenUserExists_ShouldReturnOk() {
        var userId = Guid.NewGuid();
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            "test-auth"));

        _authServiceMock
            .Setup(service => service.GetCurrentUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrentUserDto {
                Id = userId,
                DisplayName = "Teste",
                Email = "teste@local"
            });

        var result = await _controller.MeAsync(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<CurrentUserDto>(okResult.Value);
        Assert.Equal(userId, payload.Id);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeCurrentSessionAndReturnNoContent() {
        var userId = Guid.NewGuid();
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            "test-auth"));
        _controller.ControllerContext.HttpContext.Request.Headers.Cookie = "tm_refresh_token=cookie-refresh-token";

        var result = await _controller.LogoutAsync(CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _authServiceMock.Verify(service => service.LogoutAsync(userId, "cookie-refresh-token", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
