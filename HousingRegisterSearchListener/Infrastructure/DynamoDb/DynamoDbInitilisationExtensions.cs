using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using HousingRegisterSearchListener.Gateway;
using HousingRegisterSearchListener.Gateway.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HousingRegisterApi.V1.Infrastructure
{
    // TODO: Use Hackney.Core NuGet package...

    public static class DynamoDbInitilisationExtensions
    {
        public static void ConfigureDynamoDB(this IServiceCollection services)
        {
            services.AddTransient<IDbEntityGateway, DynamoDbEntityGateway>();

            _ = bool.TryParse(Environment.GetEnvironmentVariable("DynamoDb_LocalMode"), out var localMode);

            if (localMode)
            {
                var url = Environment.GetEnvironmentVariable("DynamoDb_LocalServiceUrl");
                services.AddSingleton<IAmazonDynamoDB>(sp =>
                {
                    var clientConfig = new AmazonDynamoDBConfig { ServiceURL = url };
                    return new AmazonDynamoDBClient(clientConfig);
                });
            }
            else
            {
                services.AddAWSService<IAmazonDynamoDB>();
            }

            services.AddScoped<IDynamoDBContext>(sp =>
            {
                var db = sp.GetService<IAmazonDynamoDB>();
                return new DynamoDBContext(db);
            });
        }
    }
}
