using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Hackney.Core.DynamoDb;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace HousingRegisterSearchListener.Tests
{
    // TODO - Remove DynamoDb parts if not required

    public class MockApplicationFactory
    {
        private readonly List<TableDef> _tables = new List<TableDef>
        {
            // Define all tables required by the application here.
            // The definition should be exactly the same as that used in real deployed environments
            new TableDef {
                Name = "HousingRegister",
                KeyName = "id",
                KeyType = ScalarAttributeType.S }
        };
        public IDynamoDbFixture DynamoDbFixture { get; private set; }
        public IAmazonDynamoDB DynamoDb { get; private set; }


        private readonly IHost _host;

        public MockApplicationFactory()
        {
            EnsureEnvVarConfigured("DynamoDb_LocalMode", "true");
            EnsureEnvVarConfigured("DynamoDb_LocalServiceUrl", "http://localhost:8000");

            _host = CreateHostBuilder().Build();
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
                if (DynamoDbFixture != null)
                    DynamoDbFixture.Dispose();

                if (null != _host)
                {
                    _host.StopAsync().GetAwaiter().GetResult();
                    _host.Dispose();
                }

                _disposed = true;
            }
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }

        public IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder(null)
           .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
           .ConfigureServices((hostContext, services) =>
           {
               services.ConfigureDynamoDB();
               services.ConfigureDynamoDbFixture();

               var serviceProvider = services.BuildServiceProvider();

               LogCallAspectFixture.SetupLogCallAspect();

               DynamoDbFixture = serviceProvider.GetRequiredService<IDynamoDbFixture>();
               DynamoDbFixture.EnsureTablesExist(_tables);
               DynamoDb = serviceProvider.GetRequiredService<IAmazonDynamoDB>();

               CreateDynamoDbTable();
           });

        private void CreateDynamoDbTable()
        {
            var housingRegIndex = new GlobalSecondaryIndex
            {
                IndexName = "HousingRegIndex",
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = (long) 10,
                    WriteCapacityUnits = (long) 1
                },
                Projection = new Projection { ProjectionType = "ALL" }
            };

            foreach (var table in _tables)
            {
                try
                {
                    /*var indexKeySchema = new List<KeySchemaElement> {
                        {new KeySchemaElement { AttributeName = "status", KeyType = "HASH"}},  //Partition key
                        {new KeySchemaElement{ AttributeName = "submittedAt", KeyType = "RANGE"}}  //Sort key
                    };*/

                    //housingRegIndex.KeySchema = indexKeySchema;

                    var request = new CreateTableRequest(table.Name,
                        new List<KeySchemaElement> { new KeySchemaElement(table.KeyName, KeyType.HASH) },
                        new List<AttributeDefinition> { new AttributeDefinition(table.KeyName, table.KeyType) },
                        new ProvisionedThroughput(3, 3)
                        );
                    //request.GlobalSecondaryIndexes.Add(housingRegIndex);
                    _ = DynamoDb.CreateTableAsync(request).GetAwaiter().GetResult();
                }
                catch (ResourceInUseException)
                {
                    // It already exists :-)
                }
            }
        }
    }
}
