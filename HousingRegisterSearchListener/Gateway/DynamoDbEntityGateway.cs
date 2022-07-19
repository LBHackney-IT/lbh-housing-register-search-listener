using Amazon.DynamoDBv2.DataModel;
using HousingRegisterSearchListener.Domain;
using HousingRegisterSearchListener.Factories;
using HousingRegisterSearchListener.Gateway.Interfaces;
using HousingRegisterSearchListener.Infrastructure;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using HousingRegisterApi.V1.Domain;
using HousingRegisterApi.V1.Infrastructure;

namespace HousingRegisterSearchListener.Gateway
{
    public class DynamoDbEntityGateway : IDbEntityGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<DynamoDbEntityGateway> _logger;

        public DynamoDbEntityGateway(IDynamoDBContext dynamoDbContext, ILogger<DynamoDbEntityGateway> logger)
        {
            _logger = logger;
            _dynamoDbContext = dynamoDbContext;
        }

        [LogCall]
        public async Task<Application> GetEntityAsync(Guid id)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for id {id}");
            var dbEntity = await _dynamoDbContext.LoadAsync<ApplicationDbEntity>(id).ConfigureAwait(false);
            return dbEntity?.ToDomain();
        }
    }
}
