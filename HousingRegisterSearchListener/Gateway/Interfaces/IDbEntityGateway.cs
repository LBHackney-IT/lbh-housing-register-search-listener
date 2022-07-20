using Amazon.DynamoDBv2.DataModel;
using HousingRegisterApi.V1.Domain;
using HousingRegisterSearchListener.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HousingRegisterSearchListener.Gateway.Interfaces
{
    public interface IDbEntityGateway
    {
        Task<Application> GetEntityAsync(Guid id);

        public IDynamoDBContext DynamoDbContext { get; }

    }
}
