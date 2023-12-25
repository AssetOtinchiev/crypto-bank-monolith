using System.Web;

namespace WebApi.Integration.Tests.Helpers;

public class CookieHelper
{
    public string GetCookie(HttpResponseMessage message)
    {
        message.Headers.TryGetValues("Set-Cookie", out var setCookie);
        var setCookieString = setCookie.Single();
        var cookieTokens = setCookieString.Split(';');
        var firstCookie = cookieTokens.FirstOrDefault();
        var keyValueTokens = firstCookie.Split('=');
        var valueString = keyValueTokens[1];
        var cookieValue = HttpUtility.UrlDecode(valueString);
        return cookieValue;
    }
}