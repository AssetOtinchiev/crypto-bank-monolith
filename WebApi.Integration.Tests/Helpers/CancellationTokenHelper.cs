namespace WebApi.Integration.Tests.Helpers;

public class CancellationTokenHelper
{
    public CancellationToken GetCancellationToken()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(60));

        return cts.Token;
    }
}