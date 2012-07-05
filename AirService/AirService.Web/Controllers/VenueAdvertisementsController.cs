using System;
using System.Web.Mvc;
using AirService.Services;
using AirService.Services.Security;
using AirService.Web.Infrastructure;
using AirService.Web.Infrastructure.Filters;
using AirService.Web.ViewModels;


namespace AirService.Web.Controllers
{
    [PaidCustomerOnly(Roles = WellKnownSecurityRoles.SystemAdministratorsAndVenueAdministrators)]
    public class VenueAdvertisementsController : Controller
    {
        private readonly VenueAdvertisementService _venueAdvertisementService;

        public VenueAdvertisementsController(VenueAdvertisementService venueAdvertisementService)
        {
            this._venueAdvertisementService = venueAdvertisementService;
        }

        //
        // GET: /VenueAdvertisements/Edit/5
        [AjaxOnly]
        public ActionResult Edit()
        {
            var venueId = ((AirServiceIdentity) User.Identity).VenueId;
            var viewModel = new VenueAdvertisementViewModel
            {
                VenueAdvertisements = _venueAdvertisementService.FindAllByVenue(venueId)
            };
            foreach (var venueAdvertisement in viewModel.VenueAdvertisements)
            {
                switch (venueAdvertisement.AdvertisedDay)
                {
                    case 0:
                        viewModel.SelectedImageSunday = venueAdvertisement.Image;
                        break;
                    case 1:
                        viewModel.SelectedImageMonday = venueAdvertisement.Image;
                        break;
                    case 2:
                        viewModel.SelectedImageTuesday = venueAdvertisement.Image;
                        break;
                    case 3:
                        viewModel.SelectedImageWednesday = venueAdvertisement.Image;
                        break;
                    case 4:
                        viewModel.SelectedImageThursday = venueAdvertisement.Image;
                        break;
                    case 5:
                        viewModel.SelectedImageFriday = venueAdvertisement.Image;
                        break;
                    case 6:
                        viewModel.SelectedImageSaturday = venueAdvertisement.Image;
                        break;
                    default:
                        break;
                }

            }
            return PartialView("_VenueAdvertisements", viewModel);
        }

        //
        // POST: /VenueAdvertisements/Edit/5 
        [HttpPost, AjaxOnly]
        public ActionResult Edit(VenueAdvertisementViewModel venueAdvertisementViewModel)
        {
            if (ModelState.IsValid) {
                var venueId = ((AirServiceIdentity)User.Identity).VenueId;
                var venueAdvertisements = _venueAdvertisementService.FindAllByVenue(venueId);

                foreach (var venueAdvertisement in venueAdvertisements)
                {
                    var updatedImage = String.Empty;
                    switch (venueAdvertisement.AdvertisedDay)
                    {
                        case 0:
                            updatedImage = venueAdvertisementViewModel.SelectedImageSunday;
                            break;
                        case 1:
                            updatedImage = venueAdvertisementViewModel.SelectedImageMonday;
                            break;
                        case 2:
                            updatedImage = venueAdvertisementViewModel.SelectedImageTuesday;
                            break;
                        case 3:
                            updatedImage = venueAdvertisementViewModel.SelectedImageWednesday;
                            break;
                        case 4:
                            updatedImage = venueAdvertisementViewModel.SelectedImageThursday;
                            break;
                        case 5:
                            updatedImage = venueAdvertisementViewModel.SelectedImageFriday;
                            break;
                        case 6:
                            updatedImage = venueAdvertisementViewModel.SelectedImageSaturday;
                            break;
                        default:
                            break;
                    }

                    if (venueAdvertisement.Image != updatedImage)
                    {
                        venueAdvertisement.Image = updatedImage;
                        _venueAdvertisementService.InsertOrUpdate(venueAdvertisement);
                    }
                }
                
                _venueAdvertisementService.Save();
                return Json(true);
            }

            return this.ResponseWithJsonErrors();
        }

        //
        // POST: /VenueAdvertisements/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            _venueAdvertisementService.Delete(id);
            _venueAdvertisementService.Save();

            return RedirectToAction("Index");
        }
    }
}

