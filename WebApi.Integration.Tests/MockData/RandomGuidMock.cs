namespace WebApi.Integration.Tests.Features.Users.MockData;

public static class RandomGuidMock
{
    public static IEnumerable<object[]> Guids
    {
        get
        {
            yield return new object[] {Guid.Parse("b3548ecf-8f31-4b4a-a120-1536fea7b3a7")};
            yield return new object[] {Guid.Parse("7dd27c42-87fb-4d9c-a06f-38cd0fc7de0a")};
            yield return new object[] {Guid.Empty};
        }
    }
}