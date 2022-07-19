using Amazon.Lambda.Core;
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

        public async Task<int> Handle(string buildNumber, ILambdaContext context)
        {
            var dynamoDBGateway = ServiceProvider.GetService<IDbEntityGateway>();
            var searchGateway = ServiceProvider.GetService<ISearchGateway>();
            int documentsIndexed = 0;
            int pageSize = 10;

            //Ensure the cluster setting to not auto create indices is set
            await searchGateway.SetRecommendedServerSettings();

            //Create new mapping based on build number parameter
            var newIndexName = await searchGateway.CreateNewIndex(buildNumber);

            //Index documents into the raw index
            string paginationToken = null;

            //Grab a page from dynamodb
            var resultsPage = await dynamoDBGateway.GetApplicationsPaged(paginationToken, pageSize);

            //Keep looping until there are no results
            while (resultsPage.Item1.Any())
            {
                _ = await searchGateway.BulkIndexApplications(resultsPage.Item1, newIndexName);

                documentsIndexed += resultsPage.Item1.Count;

                paginationToken = resultsPage.Item2;

                if(paginationToken == null)
                {
                    break;
                }

                resultsPage = await dynamoDBGateway.GetApplicationsPaged(paginationToken, pageSize);
            }

            //Move alias target to new index
            await searchGateway.SetReadAlias(newIndexName);

            return documentsIndexed;
        }
    }
}
