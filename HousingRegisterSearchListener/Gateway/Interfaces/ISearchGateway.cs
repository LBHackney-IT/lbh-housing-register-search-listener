using HousingRegisterApi.V1.Domain;
using HousingRegisterSearchListener.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HousingRegisterSearchListener.Gateway.Interfaces
{
    public interface ISearchGateway
    {
        Task<bool> IndexApplication(Application application, bool requireAlias = true);

        Task<string> CreateNewIndex(string buildIdentifier = "local");

        Task SetReadAlias(string indexName);

        Task<bool> BulkIndexApplications(List<Application> applications, string indexNameOverride = null);

        Task<List<string>> GetReadAliasTarget();

        Task SetRecommendedServerSettings();

        Task DropIndex(string indexName);
    }
}
