using System;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class ServiceProviderService : SimpleService<ServiceProvider>, IServiceProviderService
    {
        public ServiceProviderService(IRepository<ServiceProvider> serviceProviderRepository)
        {
            this.Repository = serviceProviderRepository;
        }

        #region IServiceProviderService Members

        public Venue GetVenueByVenueAdminUserName(Guid userId)
        {
            return (from provider in this.Repository.FindAll()
                    where provider.UserId == userId
                    select provider.Venue).FirstOrDefault();
        }

        public ServiceProvider FindByVenue(int venueId)
        {
            var serviceProvider = (from provider in this.Repository.FindAll()
                         where provider.VenueId == venueId
                         select provider).FirstOrDefault();
            return serviceProvider;
        }

        public ServiceProvider FindByUserId(Guid userId)
        {
            var serviceProvider = (from provider in this.Repository.FindAll()
                                   where provider.UserId == userId
                                   select provider).FirstOrDefault();
            return serviceProvider;
        }

        #endregion
    }
}