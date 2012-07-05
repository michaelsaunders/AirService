using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class StateService : SimpleService<State>, IStateService
    {
        public StateService(IRepository<State> stateRepository)
        {
            this.Repository = stateRepository;
        }

        public IQueryable<State> FindAllByCountry(int countryId)
        {
            return this.FindAll().Where(state => state.CountryId == countryId);
        }

    }
}