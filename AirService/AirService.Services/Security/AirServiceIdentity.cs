using System;
using System.Security.Principal;
using System.Web.Security;
using AirService.Model;

namespace AirService.Services.Security
{
    [Serializable]
    public class AirServiceIdentity : IIdentity
    {
        private readonly string _name;
        private readonly FormsAuthenticationTicket _ticket;

        public AirServiceIdentity(string name)
        {
            this._name = name; 
        }

        public AirServiceIdentity(FormsAuthenticationTicket ticket)
        {
            this._ticket = ticket;
            this._name = ticket.Name;
        } 

        public Guid ProviderKey
        {
            get;
            set;
        }

        public Guid UserId
        {
            get
            {
                var value = _ticket.UserData.Split('|')[0];
                return Guid.Parse(value);
            }
        } 
         
        public virtual int VenueId
        {
            get
            {
                var value = _ticket.UserData.Split('|')[1];
                return int.Parse(value);
            }
        }
         
        public VenueAccountTypes? AccountType { get; set; }

        public string DisplayName
        {
            get
            {
                var values = _ticket.UserData.Split('|');
                if (values.Length >= 3)
                {
                    return values[2];
                }

                return this.Name;
            }
        }

        /// <summary>
        /// User who are impersonating current login user. 
        /// </summary>
        public Guid? AdminUserId
        {
            get
            {
                var values = this._ticket.UserData.Split('|');
                if (values.Length >= 4)
                {
                    return new Guid(values[3]);
                }

                return null;
            }
        }

        #region IIdentity Members

        public string AuthenticationType
        {
            get { return "AirService"; }
        }

        public bool IsAuthenticated
        {
            get { return true; }
        }

        public string Name
        {
            get { return this._name; }
        }

        public int? SecondsFromGmt { get; set; }

        #endregion
    }

}