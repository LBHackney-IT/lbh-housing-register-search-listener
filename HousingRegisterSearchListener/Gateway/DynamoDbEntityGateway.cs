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
using Amazon.DynamoDBv2.DocumentModel;
using System.Linq;
using System.Collections.Generic;

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

        [LogCall]
        public async Task<(List<Application>, string)> GetApplicationsPaged(string paginationToken, int pageSize = 10)
        {
            var config = new ScanOperationConfig
            {
                Limit = pageSize,
                PaginationToken = paginationToken
            };

            // query dynamodb
            AsyncSearch<ApplicationDbEntity> searchHandle = _dynamoDbContext.FromScanAsync<ApplicationDbEntity>(config);
            var resultSet = await searchHandle.GetNextSetAsync();
            var applicationDomainObjects = resultSet.Select(r => r.ToDomain()).ToList();
            return (applicationDomainObjects, config.PaginationToken);


        }
    }
}
