using HousingRegisterApi.V1.Domain;
using HousingRegisterSearchListener.Domain;
using HousingRegisterSearchListener.Factories;
using HousingRegisterSearchListener.Gateway.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HousingRegisterSearchListener.Gateway
{
    public class SearchGateway : ISearchGateway
    {
        private readonly ILogger<SearchGateway> _logger;
        private ElasticClient _client;

        const string HousingRegisterReadAlias = "housing-register-applications";

        public SearchGateway(ILogger<SearchGateway> logger, IConfiguration configuration)
        {
            _logger = logger;
            _client = new ElasticClient(new Uri(configuration["SEARCHDOMAIN"]));
        }

        public async Task<bool> IndexApplication(Application application)
        {
            var searchEntity = application.ToSearch();

            var indexResult = await _client.IndexAsync(searchEntity, i => i.RequireAlias(true).Index(HousingRegisterReadAlias));

            if (indexResult.IsValid)
            {
                return true;
            }
            else
            {
                throw indexResult.OriginalException ?? new Exception($"Server error status code {indexResult.ServerError.Status} - {indexResult.ServerError.Error.Type} - {indexResult.ServerError.Error.Reason}- {indexResult.ServerError.Error.RootCause}");
            }
        }

        public async Task<string> CreateMapping(string buildIdentifier = "local")
        {
            string indexName = $"housing-register-applications-{buildIdentifier}";
            //Creates an index in elasticsearch based on a build number
            var createIndexResponse = await _client.Indices.CreateAsync(indexName, c => c
                .Map<ApplicationSearchEntity>(m => m
                    .AutoMap()
                    .Properties(ps => ps
                        .Text(p => p.Name(nn => nn.FirstName))
                        .Text(p => p.Name(nn => nn.MiddleName))
                        .Text(p => p.Name(nn => nn.Surname))
                        .Text(p => p.Name(nn => nn.NationalInsuranceNumber))
                        .Keyword(p => p.Name(nn => nn.ApplicationId))
                        .Keyword(p => p.Name(nn => nn.AssignedTo))
                        .Keyword(p => p.Name(nn => nn.Status))
                        .Nested<ApplicationOtherMembersSearchEntity>(n => n.Name(nn => nn.OtherMembers)
                            .Properties(ips => ips
                                .Text(ip => ip.Name(inn => inn.FirstName))
                                .Text(p => p.Name(nn => nn.MiddleName))
                                .Text(p => p.Name(nn => nn.Surname))
                                .Text(p => p.Name(nn => nn.NationalInsuranceNumber))
                                .Keyword(p => p.Name(nn => nn.Id))
                            )//Properties
                        )//Nested
                    )//Properties
                )//Map
            );//Create

            if (createIndexResponse.IsValid)
            {
                return indexName;
            }
            else
            {
                throw createIndexResponse.OriginalException ?? new Exception($"Server error status code {createIndexResponse.ServerError.Status} - {createIndexResponse.ServerError.Error.Type} - {createIndexResponse.ServerError.Error.Reason}- {createIndexResponse.ServerError.Error.RootCause}");
            }
        }

        public async Task SetReadAlias(string indexName)
        {
            await _client.Indices.PutAliasAsync(new PutAliasRequest(Indices.Parse(indexName), new Name(HousingRegisterReadAlias)));
        }
    }
}
