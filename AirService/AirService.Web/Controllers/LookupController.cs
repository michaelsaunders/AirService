using System;
using System.Linq;
using System.Web.Mvc;
using AirService.Services.Contracts;

namespace AirService.Web.Controllers
{
    public class LookupController : Controller
    {
        private readonly IStateService _stateService;

        public LookupController(IStateService stateService)
        {
            this._stateService = stateService;
        }


        public ActionResult GetStatesByCountry(int countryId)
        {
            var states = this._stateService.FindAll().Where(country => country.CountryId == countryId);
            return new JsonResult
                       {
                           Data = states.Select(s => new {s.Id, Name = s.Title}),
                           JsonRequestBehavior = JsonRequestBehavior.AllowGet
                       };
        }
    }
}