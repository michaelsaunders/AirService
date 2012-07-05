using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class DeviceAdminService : IDeviceAdminService
    {
        private readonly IRepository<DeviceAdmin> _repository;

        public DeviceAdminService(IRepository<DeviceAdmin> repository)
        {
            this._repository = repository;
        }

        #region IDeviceAdminService Members

        public DeviceAdmin GetDeviceAdmin(string email,
                                          string password)
        {
            var venue = (from deviceAdmin in this._repository.Set<DeviceAdmin>().Include(d => d.iPads)
                         where deviceAdmin.Email == email &&
                               deviceAdmin.Password == password
                         select deviceAdmin
                        ).FirstOrDefault();

            return venue;
        }

        public IEnumerable<DeviceAdmin> FindAllByVenueId(int venueId)
        {
            return from deviceAdmin in this._repository.FindAll().Include(a => a.iPads)
                   where deviceAdmin.VenueId == venueId && deviceAdmin.Status == SimpleModel.StatusActive
                   select deviceAdmin;
        }

        public void Insert(DeviceAdmin deviceAdmin)
        {
            this._repository.Insert(deviceAdmin);
            this._repository.Save();
        }

        public void Update(DeviceAdmin deviceAdmin)
        {
            this._repository.Update(deviceAdmin);
            this._repository.Save();
        }

        public void Delete(int venueId,
                           int deviceAdminId)
        {
            var deviceAdmin = this.FindByDeviceAdminId(venueId,
                                                       deviceAdminId);
            if (deviceAdmin == null)
            {
                throw new InvalidOperationException();
            }

            this._repository.Set<DeviceAdmin>().Remove(deviceAdmin);
            this._repository.Save();
        }

        public DeviceAdmin FindByDeviceAdminUserEmail(int venueId,
                                                      string deviceAdminEmail)
        {
            return this._repository.FindAll().Include(a => a.iPads).Include(a => a.Venue)
                .FirstOrDefault(a => a.Email == deviceAdminEmail && a.VenueId == venueId);
        }

        public bool SendPasswordEmail(string deviceAdminEmail,
                                      string emailTemplatePath)
        {
            this._repository.SetContextOption(ContextOptions.ProxyCreation,
                                              false);
            Logger.Trace("SendPasswordEmail: " + deviceAdminEmail);
            var deviceAdmins = this._repository.FindAll()
                .Include(a => a.iPads)
                .Include(a => a.Venue)
                .Where(a => a.Email == deviceAdminEmail).ToList();
            if (deviceAdmins.Count == 0)
            {
                throw new ServiceFaultException(1043,
                                                Resources.Err1043EmailNotFound);
            }

            foreach (var deviceAdmin in deviceAdmins)
            {
                this.SendPasswordEmail(deviceAdmin,
                                       emailTemplatePath);
            }

            return true;
        }

        public iPad GetAdminDeviceByMatchingPin(string deviceAdminEmail,
                                                string devicePin)
        {
            return (from admin in this._repository.FindAll()
                    from ipad in admin.iPads
                    where admin.Email == deviceAdminEmail &&
                          ipad.Pin == devicePin
                    select ipad).FirstOrDefault();
        }

        public DeviceAdmin FindByDeviceAdminId(int venueId,
                                               int deviceAdminId)
        {
            return
                this._repository.FindAll().Where(
                    d => d.Id == deviceAdminId && d.VenueId == venueId && d.Status == SimpleModel.StatusActive).
                    FirstOrDefault();
        }

        public bool SendPasswordEmail(DeviceAdmin deviceAdmin, string emailTemplatePath)
        {
            try
            {
                var content = System.IO.File.ReadAllText(emailTemplatePath);
                content = content.Replace("{Name}",
                                          HttpUtility.HtmlEncode(deviceAdmin.UserName));
                content = content.Replace("{Password}",
                                          deviceAdmin.Password);
                content = content.Replace("{VenueName}",
                                          deviceAdmin.Venue.Title);

                var iPadDetails = new StringBuilder();
                foreach (var ipad in deviceAdmin.iPads)
                {
                    iPadDetails.Append(HttpUtility.HtmlEncode(ipad.Name));
                    iPadDetails.Append("'s PIN: ");
                    iPadDetails.Append(ipad.Pin);
                    iPadDetails.Append("<br/>");
                }

                content = content.Replace("{iPadDetails}",
                                          iPadDetails.ToString());

                var client = new SmtpClient();
                var message =
                    new MailMessage(
                        ConfigurationManager.AppSettings["DefaultEmailFromAddress"] ?? "noreply@airservice.com.au",
                        deviceAdmin.Email);
                message.Subject = "New AirService Administrator";
                message.Body = content;
                message.IsBodyHtml = true;
                client.Send(message);
                Logger.Trace("Sent password email to " + deviceAdmin.Email);
                return true;
            }
            catch(Exception e)
            {
                Logger.Log("SendPasswordEmail Failed with " + e.Message, e);
                return false;
            }
        }
        #endregion
    }
}