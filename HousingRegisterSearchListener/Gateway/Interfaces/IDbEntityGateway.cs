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

        Task<(List<Application>, string)> GetApplicationsPaged(string paginationToken, int pageSize = 10);
    }
}
