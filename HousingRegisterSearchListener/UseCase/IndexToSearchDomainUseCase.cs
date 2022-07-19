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
        private readonly ISearchGateway _searchGateway;

        public IndexToSearchDomainUseCase(IDbEntityGateway gateway, ILogger<IndexToSearchDomainUseCase> logger, ISearchGateway searchGateway)
        {
            _gateway = gateway;
            _logger = logger;
            _searchGateway = searchGateway;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {

            if (message is null) throw new ArgumentNullException(nameof(message));

            Application entity = await _gateway.GetEntityAsync(message.EntityId).ConfigureAwait(false);

            _logger.LogInformation($"Received notification of change to applicationID {entity.Id}");

            if (entity is null) throw new EntityNotFoundException<Application>(message.EntityId);

            var success = await _searchGateway.IndexApplication(entity);

            if (success)
            {
                _logger.LogInformation($"Successfully indexed ApplicationID {entity.Id}");
            }
        }
    }
}
