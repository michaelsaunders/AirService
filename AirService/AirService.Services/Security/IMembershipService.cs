using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using AirService.Services.Contracts;

namespace AirService.Services.Security
{
    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        IPaymentService PaymentService { get; }

        bool ValidateUser(string userName, string password);

        MembershipUser GetUser(string userName, string password);

        MembershipUser GetUser(Guid userId);

        MembershipUser GetUserByEmail(string email);

        MembershipCreateStatus CreateUser(string userName, string password, string email);

        bool ChangePassword(string userName, string oldPassword, string newPassword);

        string[] GetUserRoles(string userName);

        void SendAccountConfirmationEmail(string emailAddress, string templatePath, Uri activationUri);

        void ResetPassword(string emailAddress, string templatePath);

        void SignIn(HttpContextBase context, string userName, bool createPersistentCookie, string userData);

        void SignOut();

        bool SuspendSubscription(int venueId, out string errorMessage);

        void EnableSubscription(int venueId);

        bool HasLockedAccount(int venueId);

        void UnlockAllUsers(int venueId);

        void LoginAsVenueAdmin(HttpContextBase context, int venueId, Guid userId);
        
        void LoginAsSystemAdmin(HttpContextBase context, Guid adminUserId);

        bool IsUserInRole(string userName, string roleName);

        List<MembershipUser> GetVenueAdmins(int venueId);
    }
}
