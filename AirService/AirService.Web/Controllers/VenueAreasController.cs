using System;
using System.Linq;
using System.Web.Mvc;
using AirService.Model;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Web.Infrastructure.Filters;

namespace AirService.Web.Controllers
{
    [PaidCustomerOnly(Roles = WellKnownSecurityRoles.SystemAdministratorsAndVenueAdministrators)]
    public class VenueAreasController : Controller
    {
        private readonly IVenueAreaService _venueAreaService;
        private int _venueId;

        public VenueAreasController(IVenueAreaService venueAreaService)
        {
            this._venueAreaService = venueAreaService;
        }

        //
        // GET: /VenueAreas/

        public ViewResult Index()
        {
            this._venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            return View(this._venueAreaService.FindAll().Where(v => v.VenueId == this._venueId));
        }

        //
        // GET: /VenueAreas/Create
        [AjaxOnly]
        public ActionResult Create()
        {
            this._venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            return this.PartialView("_VenueArea",
                                    new VenueArea
                                        {VenueId = this._venueId, CreateDate = DateTime.Now, UpdateDate = DateTime.Now});
        }

        //
        // POST: /VenueAreas/Create
        [AjaxOnly]
        [HttpPost]
        public ActionResult Create(VenueArea venueArea)
        {
            if (this.ModelState.IsValid)
            {
                this._venueAreaService.InsertOrUpdate(venueArea);
                this._venueAreaService.Save();
                this._venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
                return this.PartialView("_VenueAreas",
                                        this._venueAreaService.FindAll().Where(v => v.VenueId == this._venueId));
            }

            return this.PartialView("_VenueArea", venueArea);
        }

        //
        // GET: /VenueAreas/Edit/5
        [AjaxOnly]
        public ActionResult Edit(int id)
        {
            return this.PartialView("_VenueArea", this._venueAreaService.Find(id));
        }

        //
        // POST: /VenueAreas/Edit/5
        [HttpPost, AjaxOnly]
        public ActionResult Edit(VenueArea venueArea)
        {
            if (this.ModelState.IsValid)
            {
                this._venueAreaService.InsertOrUpdate(venueArea);
                this._venueAreaService.Save();
                this._venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
                return this.PartialView("_VenueAreas",
                                        this._venueAreaService.FindAll().Where(v => v.VenueId == this._venueId));
            }

            return this.PartialView("_VenueArea", venueArea);
        }


        //
        // POST: /VenueAreas/Delete/5
        [AjaxOnly]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            this._venueAreaService.Delete(id);
            this._venueAreaService.Save();
            this._venueId = ((AirServiceIdentity) this.User.Identity).VenueId;
            return this.PartialView("_VenueAreas",
                                    this._venueAreaService.FindAll().Where(v => v.VenueId == this._venueId));
        }
    }
}