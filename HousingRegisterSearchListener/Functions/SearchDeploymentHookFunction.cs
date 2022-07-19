using Amazon.Lambda.Core;
using HousingRegisterApi.V1.Infrastructure;
using HousingRegisterSearchListener.Factories;
using HousingRegisterSearchListener.Gateway.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingRegisterSearchListener.Functions
{
    public class SearchDeploymentHookFunction : BaseFunction
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public SearchDeploymentHookFunction() : base() { }

        public async Task<string> Handle(string buildNumber, ILambdaContext context)
        {
            var dynamoDBGateway = ServiceProvider.GetService<IDbEntityGateway>();
            var searchGateway = ServiceProvider.GetService<ISearchGateway>();
            int documentsIndexed = 0;

            //Get the name of the current index
            var oldIndexNames = await searchGateway.GetReadAliasTarget();

            //Ensure the cluster setting to not auto create indices is set
            await searchGateway.SetRecommendedServerSettings();

            //Create new mapping based on build number parameter
            var newIndexName = await searchGateway.CreateNewIndex(buildNumber);

            //Index documents into the raw index

            //Grab a page from dynamodb
            var dynamoDbContext = dynamoDBGateway.DynamoDbContext;

            var scanHandle = dynamoDbContext.ScanAsync<ApplicationDbEntity>(null);

            var resultsPage = await scanHandle.GetNextSetAsync();

            //Keep looping until there are no results
            while (resultsPage.Any())
            {
                _ = await searchGateway.BulkIndexApplications(resultsPage.Select(r=>r.ToDomain()).ToList(), newIndexName);

                documentsIndexed += resultsPage.Count;            

                resultsPage =  await scanHandle.GetNextSetAsync();
            }

            //Move alias target to new index
            await searchGateway.SetReadAlias(newIndexName);

            //Remove the old indices that the alias was pointing to

            foreach (var oldIndexName in oldIndexNames)
            {
                await searchGateway.DropIndex(oldIndexName);
            }

            return $"Indexed {documentsIndexed} documents";
        }
    }
}
