using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Services
{
    public class CountryService : SimpleService<Country>
    {
        public CountryService(IRepository<Country> countryRepository)
        {
            this.Repository = countryRepository;
        }

    }
}