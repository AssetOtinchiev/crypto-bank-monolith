namespace WebApi.Integration.Tests.Helpers;

public static class CancellationTokenHelper
{
    public static CancellationToken GetCancellationToken()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(60));

        return cts.Token;
    }
}