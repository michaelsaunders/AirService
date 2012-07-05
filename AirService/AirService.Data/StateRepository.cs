using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{ 
    public class StateRepository : SimpleRepository<State>
    {
        public StateRepository(IAirServiceContext context)
            : base(context)
        { 
        } 
    }
}