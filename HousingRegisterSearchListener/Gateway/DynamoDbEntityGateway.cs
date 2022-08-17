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
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace HousingRegisterSearchListener.Gateway
{
    public class DynamoDbEntityGateway : IDbEntityGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<DynamoDbEntityGateway> _logger;
        private readonly IAmazonDynamoDB _client;
        private const string HousingRegisterTableName = "HousingRegister";

        public IDynamoDBContext DynamoDbContext => _dynamoDbContext;

        public DynamoDbEntityGateway(IDynamoDBContext dynamoDbContext, ILogger<DynamoDbEntityGateway> logger, IAmazonDynamoDB client)
        {
            _logger = logger;
            _client = client;
            _dynamoDbContext = dynamoDbContext;
        }

        [LogCall]
        public async Task<Application> GetEntityAsync(Guid id)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for id {id}");
            var dbEntity = await DynamoDbContext.LoadAsync<ApplicationDbEntity>(id).ConfigureAwait(false);
            return dbEntity?.ToDomain();
        }

        [LogCall]
        public async Task SetReferenceNumber(Guid id, string newReferenceNumber)
        {
            _logger.LogDebug($"Setting reference number for applicationID {id} to {newReferenceNumber}");

            string tableName = HousingRegisterTableName;

            var request = new UpdateItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>() { { "Id", new AttributeValue { S = id.ToString() } } },
                ExpressionAttributeNames = new Dictionary<string, string>()    {
                    {"#R", "Reference"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":newref",new AttributeValue {S = newReferenceNumber}},
                },
                UpdateExpression = "SET #R =:newref"
            };

            _ = await _client.UpdateItemAsync(request);

        }

        public async Task<bool> SetLastIssuedBiddingNumberIfNotSet(long lastIssuedBiddingNumber)
        {
            bool biddingNumberSet = false;

            try
            {
                var putItemResult = await _client.PutItemAsync(new PutItemRequest
                {
                    TableName = HousingRegisterTableName,
                    Item = new Dictionary<string, AttributeValue> {
                    { "id", new AttributeValue("HousingRegister#BiddingNumberAtomicCounter") },
                    { "lastIssuedBiddingNumber", new AttributeValue(lastIssuedBiddingNumber.ToString()) }
                },
                    ConditionExpression = "attribute_not_exists(id)"
                }).ConfigureAwait(false);

                biddingNumberSet = true;
            }
            catch (ConditionalCheckFailedException)
            {
                biddingNumberSet = false;
                _logger.LogInformation("Bidding number already set");
            }

            return biddingNumberSet;
        }

    }
}
