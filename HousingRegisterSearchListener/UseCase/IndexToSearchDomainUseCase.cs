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

namespace HousingRegisterSearchListener.UseCase
{
    public class IndexToSearchDomainUseCase : IDoSomethingUseCase
    {
        private readonly IDbEntityGateway _gateway;
        private readonly ILogger<IndexToSearchDomainUseCase> _logger;

        public IndexToSearchDomainUseCase(IDbEntityGateway gateway, ILogger<IndexToSearchDomainUseCase> logger)
        {
            _gateway = gateway;
            _logger = logger;
        }

        [LogCall]
        public Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            //// TODO - Implement use case logic
            //DomainEntity entity = await _gateway.GetEntityAsync(message.EntityId).ConfigureAwait(false);
            //if (entity is null) throw new EntityNotFoundException<DomainEntity>(message.EntityId);

            //entity.Description = "Updated";

            //// Save updated entity
            //await _gateway.SaveEntityAsync(entity).ConfigureAwait(false);

            string objectData = JsonConvert.SerializeObject(message.EventData.NewData);

            _logger.LogInformation(objectData);

            return Task.CompletedTask;
        }
    }
}
