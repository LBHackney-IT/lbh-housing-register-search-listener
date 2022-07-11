using Hackney.Core.Testing.Shared;
using Xunit;

namespace HousingRegisterSearchListener.Tests
{
    [CollectionDefinition("LogCall collection")]
    public class LogCallAspectFixtureCollection : ICollectionFixture<LogCallAspectFixture>
    { }
}
