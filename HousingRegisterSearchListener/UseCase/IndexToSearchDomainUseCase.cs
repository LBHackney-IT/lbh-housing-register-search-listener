using HousingRegisterSearchListener.Boundary;
using HousingRegisterSearchListener.Domain;
using HousingRegisterSearchListener.Gateway.Interfaces;
using HousingRegisterSearchListener.Infrastructure.Exceptions;
using HousingRegisterSearchListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HousingRegisterApi.V1.Domain;

namespace HousingRegisterSearchListener.UseCase
{
    public class IndexToSearchDomainUseCase : IIndexToSearchDomainUseCase
    {
        private readonly IDbEntityGateway _gateway;
        private readonly ILogger<IndexToSearchDomainUseCase> _logger;

        public IndexToSearchDomainUseCase(IDbEntityGateway gateway, ILogger<IndexToSearchDomainUseCase> logger)
        {
            _gateway = gateway;
            _logger = logger;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            Application entity = await _gateway.GetEntityAsync(message.EntityId).ConfigureAwait(false);

            if (entity is null) throw new EntityNotFoundException<Application>(message.EntityId);

            _logger.LogInformation($"Received notification of change to applicationID {entity.Id}");
        }
    }
}
