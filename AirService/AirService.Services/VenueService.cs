using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;
#if UNIT_TEST
using Sql = System.Math;
#else
// Without EDMX file support, we cannot map the SQL function when the query isn't really using database
// SqlFunctions has no default implementation at client side and will fail to evaluate the query 
// if the expression was used for in-memory query.
using Sql = System.Data.Objects.SqlClient.SqlFunctions;
#endif

namespace AirService.Services
{
    public class VenueService : SimpleService<Venue>, IVenueService
    {
        private readonly IService<Country> _countryService;
        private readonly INotificationService _notificationService;
        private readonly IStateService _stateService;
        private readonly IService<VenueType> _venueTypeService;

        public VenueService(IRepository<Venue> venueRepository,
                            IService<VenueType> venueTypeService,
                            IStateService stateService,
                            IService<Country> countryService,
                            INotificationService notificationService)
        {
            this.Repository = venueRepository;
            this._venueTypeService = venueTypeService;
            this._stateService = stateService;
            this._countryService = countryService;
            this._notificationService = notificationService;
        }

        #region IVenueService Members

        public List<Venue> FindVenuesByTitle(string title)
        {
            const int evalAccountType = (int)VenueAccountTypes.AccountTypeEvaluation;
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var venues = this.Repository.FindAllIncluding(
                venue => venue.Country,
                venue => venue.State
                );

            return (from venue in venues
                    where venue.IsActive && venue.Status == SimpleModel.StatusActive && venue.Title.Contains(title)
                          && venue.VenueAccountType > 0 && (venue.VenueAccountType & evalAccountType) == 0
                    select venue).Take(50).ToList();
        }

        public List<Venue> FindVenuesByLocation(double lat, double lng, double radius = 5, string titleCriteria = null)
        {
            var startTime = DateTime.Now;
            if (radius < 1)
            {
                radius = 1;
            }

            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var venues = this.Repository.FindAll(); 
            // excerpt from http://blog.peoplesdns.com/archives/24
            // We assume the following distances in relation to our earth's radius (R)
            // 6378137 meters, 6378.137 km, 3963.191 miles, 3441.596 nautical miles
            // We will use these in our computation for distance from point if we want 
            // to use miles, kilo's or meters from our starting point, if you really 
            // wanted to get crazy then 6378137 meters = 20925646.3 feet so you could 
            // literally search for something within several hundred feet of yourself.
            double pi = Math.PI; // used instead of Sql.Pi() to support unit test. 

            const int evalAccountType = (int) VenueAccountTypes.AccountTypeEvaluation;
            var result = (from nearByVenue in venues
                          join item in
                              from venue in venues
                              where 
                                venue.IsActive && 
                                venue.Status == SimpleModel.StatusActive  && 
                                venue.VenueAccountType > 0 && 
                                (venue.VenueAccountType & evalAccountType) == 0
                              select new
                                         {
                                             venue.Id,
                                             Distance =
                                  Sql.Acos((Sql.Sin(pi*lat/180)*
                                            Sql.Sin(pi*
                                                    venue.LatitudePosition/180)) +
                                           (Sql.Cos(pi*lat/180)*
                                            Sql.Cos(pi*
                                                    venue.LatitudePosition/180)*
                                            Sql.Cos(pi*
                                                    venue.LongitudePosition/180 -
                                                    pi*lng/180)))*
                                  6378.137
                                         }
                              on nearByVenue.Id equals item.Id
                          where
                              item.Distance <= radius &&
                              (titleCriteria == null || nearByVenue.Title.Contains(titleCriteria))
                          orderby item.Distance ascending
                          select nearByVenue).Include(venue => venue.Country)
                          .Include(venue => venue.State)
                          .Take(50);

            var venuesFound = result.ToList();
            if (ConfigurationManager.AppSettings["EnableLocationSearchLog"] == "true")
            { 
                try
                {
                    using(var context = new AirServiceEntityFrameworkContext())
                    {
//SET ANSI_NULLS ON
//GO

//SET QUOTED_IDENTIFIER ON
//GO

//SET ANSI_PADDING ON
//GO

//CREATE TABLE [dbo].[LocationSearchLogs](
//    [Id] [int] IDENTITY(1,1) NOT NULL,
//    [LogDate] [datetime] NOT NULL,
//    [Lat] [float] NOT NULL,
//    [Lng] [float] NOT NULL,
//    [VenueNames] [varchar](max) NOT NULL,
//    [UserAgent] [varchar](max) NULL,
// CONSTRAINT [PK_LocationSearchLogs] PRIMARY KEY CLUSTERED 
//(
//    [Id] ASC
//)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
//) ON [PRIMARY]

//GO

//SET ANSI_PADDING OFF
//GO

//ALTER TABLE [dbo].[LocationSearchLogs] ADD  CONSTRAINT [DF_LocationSearchLogs_LogDate]  DEFAULT (getdate()) FOR [LogDate]
//GO
                        string venueNames = string.Join(",",
                                                        venuesFound.Select(v => v.Title).ToArray());
                        string userAgent = HttpContext.Current == null
                                               ? string.Empty
                                               : HttpContext.Current.Request.UserAgent ?? "";
                        context.Database.ExecuteSqlCommand(
                            string.Format(
                                @"
INSERT INTO dbo.LocationSearchLogs
(Lat, Lng, VenueNames, UserAgent, Duration)
VALUES({0},{1},'{2}','{3}', {4})",
                                lat,
                                lng,
                                venueNames.Replace("'",
                                                   "''"),
                                userAgent.Replace("'",
                                                  "''"),
                                DateTime.Now.Subtract(startTime).TotalMilliseconds)); 
                    }
                }
                catch(Exception e)
                {
                }
            } 

            return venuesFound;
        }

        public VenueConnection Connect(int venueId, int customerId)
        {
            var venues = this.Repository.FindAllIncluding(
                v => v.Country,
                v => v.State
                );

            var venue = (from v in venues
                         where v.Id == venueId && v.Status == SimpleModel.StatusActive
                         select v).FirstOrDefault();

            venue.ValidateVenueStatus();
            if (venue.VenueAccountType == (int)VenueAccountTypes.AccountTypeEvaluation)
            {
                throw new ServiceFaultException(1048,
                                              Resources.Err1048CannotConnectToVenueWhichIsNotFullAccountType);
            }

            var customer = this.Repository.Set<Customer>().Find(customerId); 
            customer.ValidateCustomerStatus();

            var previousConnection = (from connection in this.Repository.Set<VenueConnection>()
                                      where
                                          connection.VenueId == venueId &&
                                          connection.CustomerId == customerId &&
                                          connection.Status != VenueConnection.Disconnected
                                      select connection).FirstOrDefault();
            if (previousConnection != null)
            {
                return previousConnection;
            }
             
            var newConnection = new VenueConnection
                                    {
                                        CreateDate = DateTime.Now,
                                        UpdateDate = DateTime.Now,
                                        VenueId = venueId,
                                        Venue = venue, 
                                        CustomerId = customerId,
                                        Customer = customer, 
                                        Status = VenueConnection.StatusWaiting
                                    };

            this.Repository.Insert(newConnection);
            this.Repository.Save();
            this._notificationService.NotifyVenueDevicesForNewConnectionRequest(newConnection.Id);
            return newConnection;
        }

        public Venue GetVenueById(int venueId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation, false);
            var venues = this.Repository.FindAllIncluding(v => v.Country,
                                                          v => v.State);
            var venue = (from v in venues
                         where v.Id == venueId
                         select v).FirstOrDefault();
            if (venue == null || venue.Status != SimpleModel.StatusActive)
            {
                throw new ServiceFaultException(1012,
                                                Resources.Err1012VenueNotFound);
            }

            if (!venue.IsActive)
            {
                throw new ServiceFaultException(1011,
                                                Resources.Err1011VenueTemporarilyNotAvailable);
            }

            return venue;
        }

        public VenueConnection AcceptConnection(int venueId, int customerId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation, false);
            var dbSet = this.Repository.Set<VenueConnection>().Include(v => v.Customer);
            var connectionRequest = (from connection in dbSet
                                     where
                                         connection.VenueId == venueId &&
                                         connection.CustomerId == customerId && 
                                         connection.Status != VenueConnection.Disconnected
                                     select connection).FirstOrDefault();
            if (connectionRequest == null)
            {
                // we don't have conneciton request...
                throw new ServiceFaultException(1003,
                                                Resources.Err1003CustomerNotFound);
            }

            if (connectionRequest.ConnectedSince != null)
            {
                throw new ServiceFaultException(1010,
                                                Resources.Err1010VenueConnectionAlreadyMade);
            }

            DateTime connectedSince = DateTime.Now;
            connectionRequest.Status = VenueConnection.StatusActive;
            connectionRequest.ConnectedSince = connectedSince;
            connectionRequest.UpdateDate = connectedSince; 
            this.Repository.Update(connectionRequest);
            this.Repository.Save();
            this._notificationService.NotifyCustomerConnectionAccepted(connectionRequest.Id);
            return connectionRequest;
        }

        public void RejectConnection(int venueId,
                                     int customerId,
                                     string message)
        {
            var connectionRequest = (from connection in this.Repository.Set<VenueConnection>()
                                     where
                                         connection.VenueId == venueId &&
                                         connection.CustomerId == customerId &&
                                         connection.Status != VenueConnection.Disconnected
                                     select connection).FirstOrDefault();
            if (connectionRequest == null)
            {
                // we don't have conneciton request...
                throw new ServiceFaultException(1003,
                                                Resources.Err1003CustomerNotFound);
            }

            if (connectionRequest.ConnectionStatus != VenueConnection.StatusWaiting)
            {
                throw new ServiceFaultException(1034,
                                                Resources.Err1034CannotRejectConnectionOnceConnected);
            }
             
            connectionRequest.Status = VenueConnection.Disconnected;
            connectionRequest.UpdateDate = DateTime.Now;
            this.Repository.Update(connectionRequest);
            this.Repository.Save();
            this._notificationService.NotifyCustomerConnectionReject(connectionRequest.Id,
                                                                     message);
        }

        public void ResetConnections(int venueId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             true);
            var connections = (from c in this.Repository.Set<VenueConnection>()
                               where
                                   c.VenueId == venueId &&
                                   c.Status != VenueConnection.Disconnected
                               select c).ToList();
            foreach (var connection in connections)
            {
                var orders = connection.Orders.ToList();
                foreach (var order in orders)
                {
                    var unprocessedItems = order.OrderItems.Where(item => item.OrderStatus != Order.OrderStatusProcessed &&
                                                                          item.OrderStatus != Order.OrderStatusFinalized).ToList();
                    var processedItems = order.OrderItems.Where(item => item.OrderStatus == Order.OrderStatusProcessed ||
                                                                        item.OrderStatus == Order.OrderStatusFinalized).ToList();
                    foreach (var orderItem in unprocessedItems)
                    {
                        this.Repository.Set<OrderItem>().Remove(orderItem);
                    }

                    if(processedItems.Count == 0)
                    {
                        this.Repository.Set<Order>().Remove(order);
                    }
                    else
                    {
                        decimal total = 0;
                        foreach (var processedItem in processedItems)
                        {
                            processedItem.OrderStatus = Order.OrderStatusFinalized;
                            processedItem.UpdateDate = DateTime.Now;
                            total += processedItem.Price;
                        }

                        order.OrderStatus = Order.OrderStatusFinalized;
                        order.TotalAmount = total;
                        order.UpdateDate = DateTime.Now;
                    }
                }

                connection.Status = VenueConnection.Disconnected;
            }

            this.Repository.Save();
        }

        public void UpdateCustomerLocation(int venueId,
                                           int venueAreaId,
                                           int customerId)
        {
            VenueConnection connection = (from c in this.Repository.Set<VenueConnection>()
                                          where
                                              c.VenueId == venueId &&
                                              c.CustomerId == customerId &&
                                              c.Status != VenueConnection.Disconnected
                                          select c).FirstOrDefault();
            connection.ValidateVenueConnection();
            var venueArea = (from v in this.Repository.Set<VenueArea>()
                             where v.VenueId == venueId && v.Id == venueAreaId &&
                                   v.Status == SimpleModel.StatusActive
                             select v).FirstOrDefault();
            if (venueArea == null)
            {
                throw new ServiceFaultException(1029,
                                                Resources.Err1029VenueAreaNotFoundOrUnavailable);
            }

            connection.VenueArea = venueArea;
            connection.VenueAreaId = venueAreaId;
            connection.UpdateDate = DateTime.Now;
            this.Repository.Update(connection);
            this.Repository.Save();
        }

        /// <summary>
        /// Get entire list of current connections
        /// </summary>
        public List<VenueConnection> GetAllVenueConnections(int venueId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var venueConnections =
                this.Repository.Set<VenueConnection>().Include(c => c.VenueArea).Include(c => c.Customer).Include(
                    c => c.Venue);
            return (from connection in venueConnections
                    where connection.VenueId == venueId &&
                          connection.Status != VenueConnection.Disconnected
                    select connection).ToList();
        }

        public int GetVenueConnectionCount(int venueId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            return (from connection in this.Repository.Set<VenueConnection>()
                    where connection.VenueId == venueId &&
                          connection.Status != VenueConnection.Disconnected
                    select connection).Count();
        }

        /// <summary>
        /// Get only modified connections since the particular date
        /// </summary>
        public List<VenueConnection> GetModifiedVenueConnectionsSince(int venueId, DateTime fromDate)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var venueConnections =
                this.Repository.Set<VenueConnection>().Include(c => c.VenueArea).Include(c => c.Customer).Include(
                    c => c.Venue);
            return (from connection in venueConnections
                    where connection.VenueId == venueId && connection.UpdateDate > fromDate
                    select connection).ToList();
        }

        public Customer GetCustomerFromCurrentVenueConnections(int venueId, int customerId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            return (from connection in this.Repository.Set<VenueConnection>()
                    where
                        connection.VenueId == venueId &&
                        connection.CustomerId == customerId
                    orderby connection.UpdateDate descending
                    select connection.Customer).FirstOrDefault();
        }

        public VenueConnection GetVenueConnectionByCustomerId(int venueId, int customerId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                                             false);
            var venueConnections = this.Repository.Set<VenueConnection>()
                .Include(c => c.Customer)
                .Include(c => c.Venue)
                .Include(c => c.VenueArea)
                .Include(v => v.Venue.Country)
                .Include(v => v.Venue.State)
                .Include(c => c.Orders)
                .Include("Orders.OrderItems");
            return (from connection in venueConnections
                    where
                        connection.VenueId == venueId &&
                        connection.CustomerId == customerId &&
                        connection.Status != VenueConnection.Disconnected
                    select connection).FirstOrDefault();
        }

        public VenueConnection FreezeCustomer(int venueId, int customerId, int minutes)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var venueConnections =
              this.Repository.Set<VenueConnection>().Include(c => c.VenueArea).Include(c => c.Customer).Include(
                  c => c.Venue);
            var venueConnection = (from connection in venueConnections
                                   where
                                       connection.VenueId == venueId &&
                                       connection.CustomerId == customerId &&
                                       connection.Status != VenueConnection.Disconnected
                                   select connection).FirstOrDefault();
            if (venueConnection == null)
            {
                throw new ServiceFaultException(1003,
                                                Resources.Err1003CustomerNotFound);
            }

            venueConnection.FreezeUtil = DateTime.Now.AddMinutes(minutes);
            venueConnection.UpdateDate = DateTime.Now;
            this.Repository.Update(venueConnection);
            this.Repository.Save();
            return venueConnection;
        }

        public VenueConnection UnfreezeCustomer(int venueId, int customerId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var venueConnections =
              this.Repository.Set<VenueConnection>().Include(c => c.VenueArea).Include(c => c.Customer).Include(
                  c => c.Venue);
            VenueConnection venueConnection = (from connection in venueConnections
                                               where
                                                   connection.VenueId == venueId &&
                                                   connection.CustomerId == customerId &&
                                                   connection.Status != VenueConnection.Disconnected
                                               select connection).FirstOrDefault();
            if (venueConnection == null)
            {
                throw new ServiceFaultException(1003,
                                                Resources.Err1003CustomerNotFound);
            }

            if (venueConnection.FreezeUtil.HasValue)
            {
                venueConnection.FreezeUtil = null;
                venueConnection.UpdateDate = DateTime.Now;
                this.Repository.Update(venueConnection);
                this.Repository.Save();
            }

            return venueConnection;
        }

        public void EnableService(int venueId, bool enable)
        {
            var venue = this.Find(venueId);
            venue.IsActive = enable;
            this.Repository.Update(venue);
            this.Repository.Save();
        }

         
        #endregion

        public override void InsertOrUpdate(Venue entity)
        {
            var state = this._stateService.Find(entity.StateId);
            var country = this._countryService.Find(entity.CountryId).Title;
            var address = string.IsNullOrWhiteSpace(entity.Address1)
                              ? ""
                              : (entity.Address1 + " ");
            address += string.IsNullOrWhiteSpace(entity.Address2)
                           ? ""
                           : entity.Address2;
            address = string.Format("{0}, {1}, {2} {3}, {4}",
                                    address.Trim(),
                                    entity.Suburb,
                                    state.Code,
                                    entity.Postcode,
                                    country);
            try
            {
                var coordinates = GeoLocation.GetCoordinates(address);
                entity.LatitudePosition = coordinates.Latitude;
                entity.LongitudePosition = coordinates.Longitude; 
            }
            catch(Exception e)
            {
                Logger.Log("Failed to geo code the given address",
                           e);
            }

            if (entity.MobileApplicationSettingsId == default(int))
            {
                entity.MobileApplicationSettings = MobileApplicationSettingsService.Create();
            }

            base.InsertOrUpdate(entity);
        }

        public Venue Create()
        {
            var venue = new Venue
                            {
                                CreateDate = DateTime.Now,
                                UpdateDate = DateTime.Now,
                                Status = SimpleModel.StatusActive,
                                Menus = new List<Menu>(),
                                VenueAreas = new List<VenueArea>(),
                                VenueTypes = new List<VenueType>(),
                                MobileApplicationSettings = MobileApplicationSettingsService.Create(),
                                VenueAccountType = (int)VenueAccountTypes.AccountTypeEvaluation
                            };
            return venue;
        }

        public void UpdateModelForVenueTypes(Venue venue, int[] selectedVenueTypes)
        {
            if (selectedVenueTypes == null)
            {
                return;
            }

            venue.VenueTypes = venue.VenueTypes ?? new List<VenueType>();

            // remove deleted items
            var deletedVenueTypes = venue.VenueTypes.Where(v => !selectedVenueTypes.Contains(v.Id)).ToList();
            foreach (var deletedVenueType in deletedVenueTypes)
            {
                venue.VenueTypes.Remove(deletedVenueType);
                deletedVenueType.Venues.Remove(venue);
            }

            // add new items
            foreach (var selectedVenueType in selectedVenueTypes)
            {
                if (venue.VenueTypes.FirstOrDefault(v => v.Id == selectedVenueType) == null)
                {
                    var venueType = _venueTypeService.Find(selectedVenueType);
                    venue.VenueTypes.Add(venueType);
                    venueType.Venues.Add(venue);
                }
            }
        }

        public VenueConnection GetVenueConnectionById(int id)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation, false);
            var repository = this.Repository.Set<VenueConnection>()
                .Include(v => v.Customer)
                .Include(v => v.Venue)
                .Include(v => v.VenueArea)
                .Include(v => v.Venue.Country)
                .Include(v => v.Venue.State)
                .Include(v => v.Orders)
                .Include("Orders.OrderItems");
            return repository.FirstOrDefault(c => c.Id == id);
        }

        public VenueConnection GetVenueConnectionByRandom()
        {
            var totalActiveConnections = (from c in this.Repository.Set<VenueConnection>()
                                          where c.Status == VenueConnection.StatusActive ||
                                                c.Status == VenueConnection.StatusFrozen
                                          select c).Count();
            if (totalActiveConnections == 0)
            {
                return null;
            }

            var randomNumber = new Random(DateTime.Now.Millisecond).Next(0, totalActiveConnections - 1);
            var repository = this.Repository.Set<VenueConnection>()
                .Include(v => v.Customer)
                .Include(v => v.Venue)
                .Include(v => v.Venue.Country)
                .Include(v => v.Venue.State);

            return (from c in repository
                    where c.Status == VenueConnection.StatusActive ||
                          c.Status == VenueConnection.StatusFrozen
                    orderby c.Id
                    select c).Skip(randomNumber).First();
        }

        public DataSet GetVenueSummaries(int sortColumn, bool ascending)
        {
            var parameters = new Dictionary<string, object>
                                 {
                                     {"@sortColumn", sortColumn},
                                     {"@ascending", @ascending}
                                 };
            var dataSet = this.Repository.ExecuteDataSet("uspGetVenueSummaryList",
                                                         CommandType.StoredProcedure,
                                                         parameters);
            var columnsToRemove = new List<string>();
            var columns = dataSet.Tables[0].Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i].ColumnName.StartsWith("_"))
                {
                    columnsToRemove.Add(columns[i].ColumnName);
                }
            }

            foreach (var columnName in columnsToRemove)
            {
                columns.Remove(columnName);
            }

            return dataSet;
        }

        public DataSet GetStatistics()
        {
            return this.Repository.ExecuteDataSet("uspGetStatistics", CommandType.StoredProcedure);
        }

        public List<VenueConnection> GetConnectedVenueIdsForCustomer(int customerId)
        {
            return (from connection in this.Repository.Set<VenueConnection>().Include(v => v.Orders)
                    where
                        connection.CustomerId == customerId &&
                        connection.Status != VenueConnection.Disconnected &&
                        connection.Status != VenueConnection.StatusWaiting
                    select connection).ToList();
        }
    }
}