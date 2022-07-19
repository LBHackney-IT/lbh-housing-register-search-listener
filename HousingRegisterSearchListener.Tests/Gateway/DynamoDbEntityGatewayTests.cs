using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using HousingRegisterSearchListener.Domain;
using HousingRegisterSearchListener.Factories;
using HousingRegisterSearchListener.Gateway;
using HousingRegisterSearchListener.Infrastructure;
using FluentAssertions;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using HousingRegisterApi.V1.Domain;
using FluentAssertions.Equivalency;

namespace HousingRegisterSearchListener.Tests.Gateway
{
    [Collection("AppTest collection")]
    public class DynamoDbEntityGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ILogger<DynamoDbEntityGateway>> _logger;
        private readonly DynamoDbEntityGateway _classUnderTest;
        private readonly IDynamoDbFixture _dbFixture;
        private IDynamoDBContext DynamoDb => _dbFixture.DynamoDbContext;
        private readonly List<Action> _cleanup = new List<Action>();

        public DynamoDbEntityGatewayTests(MockApplicationFactory appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _logger = new Mock<ILogger<DynamoDbEntityGateway>>();
            _classUnderTest = new DynamoDbEntityGateway(DynamoDb, _logger.Object);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var action in _cleanup)
                    action();

                _disposed = true;
            }
        }

        private async Task InsertDatatoDynamoDB(Application entity)
        {
            await _dbFixture.SaveEntityAsync(entity.ToDatabase()).ConfigureAwait(false);
        }

        private Application ConstructDomainEntity()
        {
            var entity = _fixture.Build<Application>()
                                 .With(x => x.Id, Guid.NewGuid())
                                 .Create();
            return entity;
        }

        [Fact]
        public async Task GetEntityAsyncTestReturnsRecord()
        {
            var domainEntity = ConstructDomainEntity();
            await InsertDatatoDynamoDB(domainEntity).ConfigureAwait(false);

            var result = await _classUnderTest.GetEntityAsync(domainEntity.Id).ConfigureAwait(false);

            result.Should().BeEquivalentTo(domainEntity, ExcludeDates);
            result.Id.Should().Be(domainEntity.Id);

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.LoadAsync for id {domainEntity.Id}", Times.Once());
        }

        private EquivalencyAssertionOptions<Application> ExcludeDates(EquivalencyAssertionOptions<Application> arg)
        {
            arg.Excluding(x => x.SubmittedAt);
            arg.Excluding(x => x.VerifyExpiresAt);
            arg.Excluding(x => x.CreatedAt);
            return arg;
        }

        [Fact]
        public async Task GetEntityAsyncTestReturnsNullWhenNotFound()
        {
            var id = Guid.NewGuid();
            var result = await _classUnderTest.GetEntityAsync(id).ConfigureAwait(false);

            result.Should().BeNull();

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.LoadAsync for id {id}", Times.Once());
        }
    }
}
