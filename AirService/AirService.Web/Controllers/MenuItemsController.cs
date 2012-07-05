using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using AirService.Services.Security;
using AirService.Web.Helpers;
using AirService.Web.Infrastructure;
using AirService.Web.Infrastructure.Filters;

namespace AirService.Web.Controllers
{
    [PaidCustomerOnly(Roles = WellKnownSecurityRoles.SystemAdministratorsAndVenueAdministrators)]
    public class MenuItemsController : Controller
    {
        [HttpPost]
        public ActionResult UploadImageFile(string qqfile)
        {
            var result = new FileUploadResult();
            var isMultipartFormUpload = Request.Files.Count > 0;
            var fileExtension = Path.GetExtension(isMultipartFormUpload ? Request.Files[0].FileName : qqfile);
            var fileName = Guid.NewGuid().ToString("N") + fileExtension; 
            var venueId = ((AirServiceIdentity)User.Identity).VenueId;
            var directoryName = Path.Combine(Server.MapPath(ConfigurationManager.AppSettings["CustomMenuItemImageLocation"]), venueId.ToString());
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
                        int length = 4096;
                        int bytesRead;
                        Byte[] buffer = new Byte[length];
                        do
                        {
                            bytesRead = System.Web.HttpContext.Current.Request.InputStream.Read(buffer, 0, length);
                            fileStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead > 0);
                    }
                }

                fileName = ControllerHelper.ResizeImage(path, 1.97f);
                result.success = true;
                result.url = Path.Combine(ConfigurationManager.AppSettings["CustomMenuItemImageLocation"], venueId.ToString() + Path.AltDirectorySeparatorChar, Url.Encode(fileName));
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

