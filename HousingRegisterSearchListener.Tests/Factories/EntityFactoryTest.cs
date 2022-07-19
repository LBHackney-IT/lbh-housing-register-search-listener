using AutoFixture;
using HousingRegisterSearchListener.Domain;
using HousingRegisterSearchListener.Factories;
using HousingRegisterSearchListener.Infrastructure;
using FluentAssertions;
using Xunit;
using HousingRegisterApi.V1.Domain;
using HousingRegisterApi.V1.Infrastructure;

namespace HousingRegisterSearchListener.Tests.Factories
{
    public class EntityFactoryTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CanMapADatabaseEntityToADomainObject()
        {
            var databaseEntity = _fixture.Create<ApplicationDbEntity>();
            var entity = databaseEntity.ToDomain();

            databaseEntity.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public void CanMapADomainEntityToADatabaseObject()
        {
            var entity = _fixture.Create<Application>();
            var databaseEntity = entity.ToDatabase();

            databaseEntity.Should().BeEquivalentTo(entity);
        }
    }
}
