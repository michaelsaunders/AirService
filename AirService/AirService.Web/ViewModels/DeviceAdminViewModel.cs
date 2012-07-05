using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class DeviceAdminViewModel
    {
        public DeviceAdminViewModel()
        {
            this.DeviceAdmin = new DeviceAdmin
                                   {
                                       Status = SimpleModel.StatusActive
                                   };
        }

        public DeviceAdminViewModel(DeviceAdmin deviceAdmin)
        {
            this.Id = deviceAdmin.Id;
            this.DeviceAdmin = deviceAdmin;
            this.EmailConfirm = deviceAdmin.Email; 
        }

        public int Id
        {
            get;
            set;
        }

        public DeviceAdmin DeviceAdmin
        {
            get;
            private set;
        }

        [Required]
        [DisplayName("Name")]
        public string UserName
        {
            get
            {
                return this.DeviceAdmin.UserName;
            }
            set
            {
                this.DeviceAdmin.UserName = value;
            }
        }

        [Required]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*",
            ErrorMessage = "A valid email address is required")]
        public string Email
        {
            get
            {
                return this.DeviceAdmin.Email;
            }
            set
            {
                this.DeviceAdmin.Email = value;
            }
        }

        [Required]
        [DisplayName("Password")]
        public string Password
        {
            get
            {
                return this.DeviceAdmin.Password;
            }
            set
            {
                this.DeviceAdmin.Password = value;
            }
        }

        [DisplayName("Confirm Email Address")]
        [Compare("Email", ErrorMessage = "Email and Email confirmation do not match.")]
        public string EmailConfirm
        {
            get;
            set;
        }

        public string[] SelectedDeviceNames
        {
            get;
            set;
        }

        public int[] SelectedDeviceIds
        {
            get;
            set;
        }

        public List<iPad> AlliPads
        {
            get;
            set;
        }
    }
}