namespace WebApi.Integration.Tests.Helpers;

public class CancellationTokenHelper
{
    public CancellationToken GetCancellationToken()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        return cts.Token;
    }
}