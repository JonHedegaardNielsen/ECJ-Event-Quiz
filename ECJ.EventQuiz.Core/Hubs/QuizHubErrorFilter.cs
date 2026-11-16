using Microsoft.AspNetCore.SignalR;

namespace ECJ.EventQuiz.Core.Hubs;

/// <summary>
/// Sends rule errors (InvalidOperationException) to the client as readable messages.
/// Without this, SignalR hides the message and the client only sees "An unexpected error occurred".
/// </summary>
public class QuizHubErrorFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (InvalidOperationException ex)
        {
            throw new HubException(ex.Message);
        }
    }
}
