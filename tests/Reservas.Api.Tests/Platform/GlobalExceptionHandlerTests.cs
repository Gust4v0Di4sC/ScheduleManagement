using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reservas.Api.Modules.Platform;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Tests.Platform;

[TestClass]
public sealed class GlobalExceptionHandlerTests
{
    [TestMethod]
    public async Task TryHandleAsync_DomainRule_ReturnsSafeProblemDetailsWithStableCode()
    {
        ProblemDetails? writtenProblem = null;
        var problemDetailsService = new Mock<IProblemDetailsService>(MockBehavior.Strict);
        problemDetailsService.Setup(service => service.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(context => writtenProblem = context.ProblemDetails)
            .ReturnsAsync(true);
        var handler = new GlobalExceptionHandler(
            problemDetailsService.Object,
            NullLogger<GlobalExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new DomainRuleException("appointment_unavailable", "The requested period is unavailable."),
            CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        writtenProblem.Should().NotBeNull();
        writtenProblem!.Extensions["code"].Should().Be("appointment_unavailable");
        writtenProblem.Detail.Should().NotContain("stack");
    }
}
