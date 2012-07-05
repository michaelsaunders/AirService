using System.Collections.Generic;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class StateViewModel
    {
        public State State { get; set; }
        public IList<Country> Countries { get; set; }
    }
}