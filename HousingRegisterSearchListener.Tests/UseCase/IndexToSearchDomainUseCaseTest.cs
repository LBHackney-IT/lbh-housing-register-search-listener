using AutoFixture;
using HousingRegisterSearchListener.Boundary;
using HousingRegisterSearchListener.Domain;
using HousingRegisterSearchListener.Gateway.Interfaces;
using HousingRegisterSearchListener.Infrastructure.Exceptions;
using HousingRegisterSearchListener.UseCase;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;

namespace HousingRegisterSearchListener.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class IndexToSearchDomainUseCaseTest
    {
        private readonly Mock<IDbEntityGateway> _mockGateway;
        private readonly Mock<ILogger<IndexToSearchDomainUseCase>> _mockLogger;
        private readonly IndexToSearchDomainUseCase _sut;
        private readonly DomainEntity _domainEntity;

        private readonly EntityEventSns _message;

        private readonly Fixture _fixture;

        public IndexToSearchDomainUseCaseTest()
        {
            _fixture = new Fixture();

            _mockGateway = new Mock<IDbEntityGateway>();
            _mockLogger = new Mock<ILogger<IndexToSearchDomainUseCase>>();
            _sut = new IndexToSearchDomainUseCase(_mockGateway.Object, _mockLogger.Object);

            _domainEntity = _fixture.Create<DomainEntity>();
            _message = CreateMessage(_domainEntity.Id);

            _mockGateway.Setup(x => x.GetEntityAsync(_domainEntity.Id)).ReturnsAsync(_domainEntity);
        }

        private EntityEventSns CreateMessage(Guid id, string eventType = EventTypes.HousingApplicationUpdatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EntityId, id)
                           .With(x => x.EventType, eventType)
                           .Create();
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestEntityIdNotFoundThrows()
        {
            _mockGateway.Setup(x => x.GetEntityAsync(_domainEntity.Id)).ReturnsAsync((DomainEntity) null);
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<DomainEntity>>();
        }

        //[Fact]
        //public void ProcessMessageAsyncTestSaveEntityThrows()
        //{
        //    var exMsg = "This is the last error";
        //    _mockGateway.Setup(x => x.SaveEntityAsync(It.IsAny<DomainEntity>()))
        //                .ThrowsAsync(new Exception(exMsg));

        //    Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
        //    func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

        //    _mockGateway.Verify(x => x.GetEntityAsync(_domainEntity.Id), Times.Once);
        //    _mockGateway.Verify(x => x.SaveEntityAsync(_domainEntity), Times.Once);
        //}

        //[Fact]
        //public async Task ProcessMessageAsyncTestSaveEntitySucceeds()
        //{
        //    await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

        //    _mockGateway.Verify(x => x.GetEntityAsync(_domainEntity.Id), Times.Once);
        //    _mockGateway.Verify(x => x.SaveEntityAsync(_domainEntity), Times.Once);
        //}
    }
}
