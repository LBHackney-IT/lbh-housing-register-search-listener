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
        Task<bool> IndexApplication(Application application);

        Task<string> CreateMapping(string buildIdentifier = "local");

        Task SetReadAlias(string indexName);
    }
}
