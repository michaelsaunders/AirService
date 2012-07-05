using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AirService.Web.Infrastructure
{
    public static class ControllerHelper
    {
        public static ActionResult ResponseWithJsonError(this Controller controller, string message)
        {
            controller.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            return new JsonResult
                       {
                           Data = new
                                      {
                                          HasError = true,
                                          ErrorMessage = message
                                      }
                       };
        }


        public static JsonResult ResponseWithJsonErrors(this Controller controller, Exception exception = null, Func<int,string> dbError=null)
        {
            while (exception != null && !(exception is SqlException) && exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            var sqlException = exception as SqlException;
            var errorProperies = new List<string>();
            var errorMessages = new List<string>();

            if (sqlException != null)
            {
                if (dbError != null)
                {
                    var message = dbError(sqlException.Number);
                    if (message != null)
                        errorMessages.Add(message);
                }
            }

            foreach (var state in controller.ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    errorProperies.Add(state.Key);
                    errorMessages.Add(string.Join(", ",
                                                  state.Value.Errors.Select(e => e.ErrorMessage).ToArray()));
                }
            }

            if (errorMessages.Count == 0)
            {
                errorProperies.Add("");
                errorMessages.Add("Unexpected error. Please try again");
            }

            controller.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            return new JsonResult
                       {
                           Data = new
                                      {
                                          HasErrors = true,
                                          ErrorKeys = errorProperies,
                                          ErrorMessages = errorMessages
                                      }
                       };
        }

        public static IEnumerable<string> GetMenuCategoryStockImages(this HttpServerUtilityBase server)
        {
            return from file in Directory.GetFiles(server.MapPath("~/Content/Shared/MenuCategories"))
                   select "/Content/Shared/MenuCategories/" + Path.GetFileName(file);
        }

        public static IEnumerable<string> GetMenuItemStockImages(this HttpServerUtilityBase server)
        {
            return from file in Directory.GetFiles(server.MapPath("~/Content/Shared/MenuItems"))
                   select "/Content/Shared/MenuItems/" + Path.GetFileName(file);
        }

        /// <summary>
        /// Default to 640px wide, if original image is smaller than that, it does nothing. 
        /// </summary>
        /// <returns>returns resized file name (or original file name if no resizing occurred)</returns>
        public static string ResizeImage(string filePath, float ratioXOverY = 0)
        {
            using (var image = Image.FromFile(filePath))
            {
                var cropImage = ratioXOverY > 0;
                var resizeImage = image.Width > 640 || cropImage;
                if (resizeImage)
                {
                    int width = Math.Min(640, image.Width);
                    int height = (int) (cropImage ? width / ratioXOverY : image.Height * width / (float)image.Width);
                    var extension = Path.GetExtension(filePath);
                    var outputFilePath = Path.ChangeExtension(filePath, "resize") + extension;
                    using (var newBitmap = new Bitmap(width, height))
                    {
                        var g = Graphics.FromImage(newBitmap);
                        if (cropImage)
                        {
                            RectangleF sourceRect = new RectangleF(0, 0, image.Width, image.Height);
                            if(sourceRect.Width / ratioXOverY > sourceRect.Height)
                            {
                                //need to crop horizontally
                                var newWidth = sourceRect.Height*ratioXOverY;
                                sourceRect.X = (sourceRect.Width - newWidth)/2;
                                sourceRect.Width = newWidth;
                            }
                            else
                            {
                                var newHeight = sourceRect.Width/ratioXOverY;
                                sourceRect.Y = (sourceRect.Height - newHeight)/2;
                                sourceRect.Height = newHeight;
                            }

                            g.DrawImage(image, new RectangleF(0, 0, width, height), sourceRect, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            g.DrawImage(image, 0, 0, width, height);
                        }

                        newBitmap.Save(outputFilePath);
                    }

                    return Path.GetFileName(outputFilePath);
                }

                return Path.GetFileName(filePath);
            }
        }

        public static RedirectToAbsoluteUriResult RedirectToAbsoluteUri(this Controller controller,
                                                                        string actionName,
                                                                        string controllerName,
                                                                        bool useSecureChannel)
        {
            return new RedirectToAbsoluteUriResult(actionName,
                                                   controllerName,
                                                   controller.RouteData.Values,
                                                   useSecureChannel);
        }
    }
}