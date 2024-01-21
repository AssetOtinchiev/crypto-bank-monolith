using WebApi.Integration.Tests.Fixtures;

namespace WebApi.Integration.Tests.Features;

[CollectionDefinition(Name)]
public class AccountTestsCollection : ICollectionFixture<TestFixture>
{
    public const string Name = nameof(AccountTestsCollection);
}
