using HousingRegisterSearchListener.Boundary;
using System.Threading.Tasks;

namespace HousingRegisterSearchListener.UseCase.Interfaces
{
    public interface IMessageProcessing
    {
        Task ProcessMessageAsync(EntityEventSns message);
    }
}
