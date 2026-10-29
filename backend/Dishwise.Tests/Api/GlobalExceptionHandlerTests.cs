using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Dishwise.Api;
using System.Text.Json;
using Xunit;

namespace Dishwise.Tests.Api;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    [Fact]
    public async Task TryHandleAsync_ShouldHideMessage_ForUnexpectedExceptions()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        var handled = await _handler.TryHandleAsync(context, new Exception("simulated failure with sensitive detail"), CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var detail = await ReadDetailAsync(context);
        Assert.DoesNotContain("sensitive detail", detail);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldExposeMessage_ForBadRequests()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await _handler.TryHandleAsync(context, new ArgumentException("Name is too long"), CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Contains("Name is too long", await ReadDetailAsync(context));
    }

    private static async Task<string> ReadDetailAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        return document.RootElement.GetProperty("detail").GetString() ?? string.Empty;
    }
}
