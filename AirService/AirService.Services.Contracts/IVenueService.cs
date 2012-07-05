using System;
using System.Collections.Generic;
using System.Data;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IVenueService : IService<Venue>
    {
        List<Venue> FindVenuesByTitle(string title);

        List<Venue> FindVenuesByLocation(double lat, double lng, double radius = 5, string titleCriteria = null);

        VenueConnection Connect(int venueId, int customerId);

        Venue GetVenueById(int venueId);

        VenueConnection AcceptConnection(int venueId, int customerId);

        List<VenueConnection> GetAllVenueConnections(int venueId);

        int GetVenueConnectionCount(int venueId);

        List<VenueConnection> GetModifiedVenueConnectionsSince(int venueId, DateTime fromDate);

        Customer GetCustomerFromCurrentVenueConnections(int venueId, int customerId);

        VenueConnection GetVenueConnectionByCustomerId(int venueId, int customerId);

        VenueConnection FreezeCustomer(int venueId, int customerId, int minutes);

        VenueConnection UnfreezeCustomer(int venueId, int customerId);

        void EnableService(int venueId, bool enable);

        void RejectConnection(int venueId,
                              int customerId,
                              string message);

        void ResetConnections(int venueId);

        void UpdateCustomerLocation(int venueId,
                                    int venueAreaId,
                                    int customerId);

        Venue Create();

        void UpdateModelForVenueTypes(Venue venue, int[] selectedVenueTypes);

        VenueConnection GetVenueConnectionById(int id);

        VenueConnection GetVenueConnectionByRandom();

        DataSet GetVenueSummaries(int sortColumn, bool ascending);

        DataSet GetStatistics();

        List<VenueConnection> GetConnectedVenueIdsForCustomer(int customerId);
    }
}