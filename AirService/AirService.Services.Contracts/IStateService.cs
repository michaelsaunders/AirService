using System.Linq;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IStateService: IService<State>
    {
        IQueryable<State> FindAllByCountry(int countryId);
    }
}