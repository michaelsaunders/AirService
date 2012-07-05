using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{ 
    public class CountryRepository : SimpleRepository<Country>
    {
        public CountryRepository(IAirServiceContext context)
            : base(context)
        { 
        }
    }
}