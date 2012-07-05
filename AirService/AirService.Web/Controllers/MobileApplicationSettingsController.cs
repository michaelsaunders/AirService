using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Web.Helpers;
using AirService.Web.Infrastructure;
using AirService.Web.Infrastructure.Filters;
using AirService.Web.ViewModels;

namespace AirService.Web.Controllers
{
    [PaidCustomerOnly(Roles = WellKnownSecurityRoles.SystemAdministratorsAndVenueAdministrators)]
    public class MobileApplicationSettingsController : Controller
    {
        private readonly MobileApplicationSettingsService _mobileApplicationSettingsService;

        public MobileApplicationSettingsController(IService<MobileApplicationSettings> mobileApplicationSettingsService)
        {
            this._mobileApplicationSettingsService = (MobileApplicationSettingsService)mobileApplicationSettingsService;
        }

        //
        // GET: /MobileApplicationSettings/Edit/5

        public ActionResult Edit()
        {
            var venueId = ((AirServiceIdentity)User.Identity).VenueId;
            var mobileApplicationSettings = _mobileApplicationSettingsService.FindByVenue(venueId);

            return View(new MobileApplicationSettingsViewModel
                {
                    MobileApplicationSettings = mobileApplicationSettings,
                    SelectedHeaderImage = mobileApplicationSettings.HeaderImage,
                    SelectedBackgroundImage = mobileApplicationSettings.BackgroundImage,
                    SelectedThemeColour = mobileApplicationSettings.Theme
                });
        }

        //
        // POST: /MobileApplicationSettings/Edit/5

        [HttpPost]
        public ActionResult Edit(MobileApplicationSettingsViewModel mobileApplicationSettingsViewModel)
        {
            if (ModelState.IsValid)
            {
                var mobileApplicationSettings = mobileApplicationSettingsViewModel.MobileApplicationSettings;
                mobileApplicationSettings.HeaderImage = mobileApplicationSettingsViewModel.SelectedHeaderImage;
                mobileApplicationSettings.BackgroundImage = mobileApplicationSettingsViewModel.SelectedBackgroundImage;
                mobileApplicationSettings.Theme = mobileApplicationSettingsViewModel.SelectedThemeColour;

                _mobileApplicationSettingsService.InsertOrUpdate(mobileApplicationSettingsViewModel.MobileApplicationSettings);
                _mobileApplicationSettingsService.Save();
                this.TempData["tempMessage"] = "Saved.";
            }

            return View(mobileApplicationSettingsViewModel);
        }
         
        public ActionResult UploadImageFile(string qqfile, int? day)
        {
            var result = new FileUploadResult();

            var isMultipartFormUpload = Request.Files.Count > 0;
            var fileExtension = Path.GetExtension(isMultipartFormUpload ? Request.Files[0].FileName : qqfile);
            var fileName = Guid.NewGuid().ToString("N") + fileExtension;
            var venueId = ((AirServiceIdentity)User.Identity).VenueId;
            var directoryName = Path.Combine(Server.MapPath(ConfigurationManager.AppSettings["CustomImageLocation"]), venueId.ToString());
            var path = Path.Combine(directoryName, fileName);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            try
            {
                if (isMultipartFormUpload)
                {
                    this.Request.Files[0].SaveAs(path);
                }
                else
                {
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        const int length = 4096;
                        int bytesRead = 0;
                        var buffer = new Byte[length];
                        do
                        {
                            bytesRead = System.Web.HttpContext.Current.Request.InputStream.Read(buffer, 0, length);
                            fileStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead > 0);
                    }
                }

                fileName = ControllerHelper.ResizeImage(path);
                result.success = true;
                var url = Path.Combine(ConfigurationManager.AppSettings["CustomImageLocation"],
                                       venueId.ToString() + Path.AltDirectorySeparatorChar, fileName);
                result.url = url;
                if (day.HasValue)
                {
                    result.dataIndex = day.Value;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // log error hinting to set the write permission of ASPNET or the identity accessing the code
                result.success = false;
                result.url = ex.Message;
            }

            if (isMultipartFormUpload)
            {
                return Json(result, "text/html");
            }

            return Json(result);
        }

    }
}