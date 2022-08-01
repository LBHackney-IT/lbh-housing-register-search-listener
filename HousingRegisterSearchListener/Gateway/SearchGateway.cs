using HousingRegisterApi.V1.Domain;
using HousingRegisterSearchListener.Domain;
using HousingRegisterSearchListener.Factories;
using HousingRegisterSearchListener.Gateway.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingRegisterSearchListener.Gateway
{
    public class SearchGateway : ISearchGateway
    {
        private readonly ILogger<SearchGateway> _logger;
        private ElasticClient _client;

        const string HousingRegisterReadAlias = "housing-register-applications";
        const string HousingRegisteeWriteAlias = "housing-register-applications-write";

        public SearchGateway(ILogger<SearchGateway> logger, IConfiguration configuration)
        {
            _logger = logger;
            _client = new ElasticClient(new Uri(configuration["SEARCHDOMAIN"]));

            _client.ConnectionSettings.IdProperties.Add(typeof(ApplicationSearchEntity), "ApplicationId");
        }

        public async Task<bool> IndexApplication(Application application, bool requireAlias = true)
        {
            var searchEntity = application.ToSearch();

            var indexResult = await _client.IndexAsync(searchEntity, i => i.RequireAlias(requireAlias).Index(HousingRegisteeWriteAlias));

            if (indexResult.IsValid)
            {
                return true;
            }
            else
            {
                throw indexResult.OriginalException ?? new Exception($"Server error status code {indexResult.ServerError.Status} - {indexResult.ServerError.Error.Type} - {indexResult.ServerError.Error.Reason}- {indexResult.ServerError.Error.RootCause}");
            }
        }

        public async Task<string> CreateNewIndex(string uniquePostfix = "local")
        {
            string indexName = $"housing-register-applications-{uniquePostfix}";
            //Creates an index in elasticsearch based on a build number
            var createIndexResponse = await _client.Indices.CreateAsync(indexName, c => c
                .Map<ApplicationSearchEntity>(m => m
                    .AutoMap()
                    .Properties(ps => ps
                        .Text(p => p.Name(nn => nn.FirstName))
                        .Text(p => p.Name(nn => nn.MiddleName))
                        .Text(p => p.Name(nn => nn.Surname))
                        .Text(p => p.Name(nn => nn.NationalInsuranceNumber))
                        .Text(p => p.Name(nn => nn.Reference)
                        .Fields(ff=>ff.Keyword(k=>k.Name("keyword"))))
                        .Keyword(p => p.Name(nn => nn.ApplicationId))
                        .Keyword(p => p.Name(nn => nn.AssignedTo))
                        .Keyword(p => p.Name(nn => nn.Status))
                        .Number(p => p.Name(nn => nn.BiddingNumber).Type(NumberType.Integer))
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
            List<string> currentReadAliasTargets = await GetReadAliasTargets();

            List<IAliasAction> aliasActions = new List<IAliasAction>();

            //Remove any existing alias targets
            foreach (var aliasTarget in currentReadAliasTargets)
            {
                aliasActions.Add(new AliasRemoveAction { Remove = new AliasRemoveOperation { Index = aliasTarget, Alias = HousingRegisterReadAlias } });
            }

            //Add the new alias target
            aliasActions.Add(new AliasAddAction { Add = new AliasAddOperation { Index = indexName, Alias = HousingRegisterReadAlias } });

            //Apply add/removes transactionally
            await _client.Indices.BulkAliasAsync(new BulkAliasRequest
            {
                Actions = aliasActions
            });
        }

        public async Task SetWriteAlias(string indexName)
        {
            List<string> currentReadAliasTargets = await GetWriteAliasTargets();

            List<IAliasAction> aliasActions = new List<IAliasAction>();

            //Remove any existing alias targets
            foreach (var aliasTarget in currentReadAliasTargets)
            {
                aliasActions.Add(new AliasRemoveAction { Remove = new AliasRemoveOperation { Index = aliasTarget, Alias = HousingRegisteeWriteAlias } });
            }

            //Add the new alias target
            aliasActions.Add(new AliasAddAction { Add = new AliasAddOperation { Index = indexName, Alias = HousingRegisteeWriteAlias } });

            //Apply add/removes transactionally
            await _client.Indices.BulkAliasAsync(new BulkAliasRequest
            {
                Actions = aliasActions
            });
        }

        public async Task<List<string>> GetReadAliasTargets()
        {
            var result = await _client.GetIndicesPointingToAliasAsync(new Names(new[] { HousingRegisterReadAlias }));

            return result.ToList();
        }

        public async Task<List<string>> GetWriteAliasTargets()
        {
            var result = await _client.GetIndicesPointingToAliasAsync(new Names(new[] { HousingRegisteeWriteAlias }));

            return result.ToList();
        }

        public async Task<bool> BulkIndexApplications(List<Application> applications, string indexNameOverride = null)
        {
            var searchEntities = applications.Select(a => a.ToSearch());

            var bulkIndexResult = await _client.IndexManyAsync<ApplicationSearchEntity>(searchEntities, indexNameOverride ?? HousingRegisterReadAlias);

            if (bulkIndexResult.IsValid)
            {
                return true;
            }
            else
            {
                throw bulkIndexResult.OriginalException ?? new Exception($"Server error status code {bulkIndexResult.ServerError.Status} - {bulkIndexResult.ItemsWithErrors.Count()} items had errors - {bulkIndexResult.ServerError.Error.Type} - {bulkIndexResult.ServerError.Error.Reason}- {bulkIndexResult.ServerError.Error.RootCause}");
            }
        }

        public async Task SetRecommendedServerSettings()
        {
            var response = await _client.Cluster.PutSettingsAsync(new ClusterPutSettingsRequest
            {
                Persistent = new Dictionary<string, object> { { "action.auto_create_index", false } }
            });

            if (!response.Acknowledged)
            {
                throw response.OriginalException ?? new Exception($"Server error status code {response.ServerError.Status} - {response.ServerError.Error.Type} - {response.ServerError.Error.Reason}- {response.ServerError.Error.RootCause}");
            }
        }

        public async Task DropIndex(string indexName)
        {
            var response = await _client.Indices.DeleteAsync(indexName);

            if (!response.Acknowledged)
            {
                throw response.OriginalException ?? new Exception($"Server error status code {response.ServerError.Status} - {response.ServerError.Error.Type} - {response.ServerError.Error.Reason}- {response.ServerError.Error.RootCause}");
            }
        }
    }
}
