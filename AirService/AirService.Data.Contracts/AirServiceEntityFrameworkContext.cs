using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using AirService.Model;

namespace AirService.Data.Contracts
{
    public enum ContextOptions
    {
        Default = 0,
        LazyLoading = 1,
        ProxyCreation = 2
    }

    public interface IAirServiceContext : IDisposable, IObjectContextAdapter
    {
        DbContext DbContext { get; }
        IDbSet<Venue> Venues { get; set; }
        IDbSet<VenueArea> VenueAreas { get; set; }
        IDbSet<ServiceProvider> ServiceProviders { get; set; }
        IDbSet<Country> Countries { get; set; }
        IDbSet<State> States { get; set; }
        IDbSet<Customer> Customers { get; set; }
        IDbSet<Menu> Menus { get; set; }
        IDbSet<MenuCategory> MenuCategories { get; set; }
        IDbSet<MenuItem> MenuItems { get; set; }
        IDbSet<MenuItemOption> MenuItemOptions { get; set; }
        IDbSet<Order> Orders { get; set; }
        IDbSet<OrderItem> OrderItems { get; set; }
        IDbSet<iPad> iPads { get; set; }
        IDbSet<DeviceAdmin> DeviceAdmins { get; set; }
        IDbSet<VenueConnection> VenueConnections { get; set; }
        IDbSet<MobileApplicationSettings> MobileApplicationSettings { get; set; }
        IDbSet<VenueAdvertisement> VenueAdvertisements { get; set; }
        IDbSet<VenueType> VenueTypes { get; set; }
        IDbSet<NotificationToken> NotificationTokens { get; set; }
        IDbSet<Notification> Notifications { get; set; }
        IDbSet<FailedPayment> FailedPayments { get; set; }
        IDbSet<VenueTransaction> VenueTransactions { get; set; }
        IDbSet<DeviceMenuOption> DeviceMenuOptions { get; set; }
        IDbSet<DeviceMenuItemOption> DeviceMenuItemOptions { get; set; } 
        IDbSet<T> Set<T>() where T : class;
        IEnumerable<T> GetEntitiesByStates<T>(EntityState states) where T : class;
        int SaveChanges();
        void SetState<T>(T entity, EntityState modified) where T : class;
        void SetOption(ContextOptions option, bool enable);

        int ExecuteQuery(string commandText, params object[] parameters);
        IAirServiceContext Clone();
        EntityState GetState<T>(T entity) where T : class;
    }

    public class AirServiceEntityFrameworkContext : DbContext, IAirServiceContext
    {
        private readonly bool _validateModels;

        public AirServiceEntityFrameworkContext()
            : base("AirService")
        {
            // by default we will validate. 
            // we need to flip the logic before we go live. 
            this._validateModels = string.Compare(ConfigurationManager.AppSettings["ValidateEntityModels"],
                                                  "true",
                                                  true) == 0;
            if (this._validateModels)
            {
                Database.SetInitializer(new AirServiceInitializer());
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            if (!this._validateModels)
            {
                modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            }

            modelBuilder.Entity<iPad>()
                .HasRequired(ipad => ipad.Venue)
                .WithMany(venue => venue.iPads)
                .HasForeignKey(ipad => ipad.VenueId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Venue>()
                .HasRequired(venue => venue.Country)
                .WithMany()
                .HasForeignKey(venue => venue.CountryId)
                .WillCascadeOnDelete(false);
            
            modelBuilder.Entity<Menu>()
                .HasRequired(menu => menu.Venue)
                .WithMany(venue=>venue.Menus)
                .HasForeignKey(m => m.VenueId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.VenueConnection)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VenueConnectionId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<VenueTransaction>()
                .HasRequired(t => t.Venue)
                .WithMany()
                .HasForeignKey(t => t.VenueId)
                .WillCascadeOnDelete(false);

            //var ipadMenuOptionConfig = modelBuilder.Entity<DeviceMenuOption>();
            //ipadMenuOptionConfig.Map(m => m.ToTable("iPadMenus"));
            //ipadMenuOptionConfig.Property(p => p.MenuId).HasColumnName("Menu_Id");
            //ipadMenuOptionConfig.Property(p => p.iPadId).HasColumnName("iPad_Id");

            ////var ipadMenuItemOptionConfig = modelBuilder.Entity<DeviceMenuItemOption>();
            ////ipadMenuItemOptionConfig.Map(m => m.ToTable("iPadMenuItems"));
            ////ipadMenuItemOptionConfig.Property(p=>p.iPadId).IsRequired().

            ////    HasRequired(m=>m.iPadId).WithMany().HasForeignKey()
            ////ipadMenuItemOptionConfig.Map(m => m.ToTable(""));
        }

        #region IAirServiceContext Members

        public DbContext DbContext
        {
            get
            {
                return this;
            }
        }

        public IDbSet<Venue> Venues { get; set; }
        public IDbSet<VenueArea> VenueAreas { get; set; }
        public IDbSet<ServiceProvider> ServiceProviders { get; set; }
        public IDbSet<Country> Countries { get; set; }
        public IDbSet<State> States { get; set; }
        public IDbSet<Customer> Customers { get; set; }
        public IDbSet<Menu> Menus { get; set; }
        public IDbSet<MenuCategory> MenuCategories { get; set; }
        public IDbSet<MenuItem> MenuItems { get; set; }
        public IDbSet<MenuItemOption> MenuItemOptions { get; set; }
        public IDbSet<Order> Orders { get; set; }
        public IDbSet<OrderItem> OrderItems { get; set; }
        public IDbSet<iPad> iPads { get; set; }
        public IDbSet<VenueConnection> VenueConnections { get; set; }
        public IDbSet<MobileApplicationSettings> MobileApplicationSettings { get; set; }
        public IDbSet<DeviceAdmin> DeviceAdmins { get; set; }
        public IDbSet<VenueAdvertisement> VenueAdvertisements { get; set; }
        public IDbSet<VenueType> VenueTypes { get; set; }
        public IDbSet<NotificationToken> NotificationTokens { get; set; }
        public IDbSet<Notification> Notifications { get; set; }
        public IDbSet<FailedPayment> FailedPayments { get; set; }
        public IDbSet<VenueTransaction> VenueTransactions { get; set; }
        public IDbSet<DeviceMenuOption> DeviceMenuOptions { get; set; }
        public IDbSet<DeviceMenuItemOption> DeviceMenuItemOptions { get; set; }

        IDbSet<T> IAirServiceContext.Set<T>()
        {
            return this.Set<T>();
        }

        void IAirServiceContext.SetState<T>(T entity, EntityState modified)
        {
            this.Entry(entity).State = modified;
        }

        public EntityState GetState<T>(T entity) where T : class
        {
            return this.Entry(entity).State;
        }

        void IAirServiceContext.SetOption(ContextOptions option, bool enable)
        {
            switch (option)
            {
                case ContextOptions.LazyLoading:
                    this.Configuration.LazyLoadingEnabled = enable;
                    break;

                case ContextOptions.ProxyCreation:
                    this.Configuration.ProxyCreationEnabled = enable;
                    break;
            }
        }

        public IEnumerable<T> GetEntitiesByStates<T>(EntityState states) where T : class
        {
            return from entry in this.ChangeTracker.Entries<T>()
                   where (entry.State & (states)) > 0
                   select entry.Entity;
        }

        public int ExecuteQuery(string commandText, params object[] parameters)
        {
            return this.Database.ExecuteSqlCommand(commandText, parameters);
        }

        public IAirServiceContext Clone()
        {
            return new AirServiceEntityFrameworkContext();
        }

        #endregion
    }

    public class AirServiceInitializer : DropCreateDatabaseIfModelChanges<AirServiceEntityFrameworkContext>
    {
        protected override void Seed(AirServiceEntityFrameworkContext context)
        { 
            #region Venue Types
            var venueTypes = new List<VenueType>
            {
                new VenueType { Title = "Hotel/Motel" },
                new VenueType { Title = "Bar" },
                new VenueType { Title = "Nightclub" },
                new VenueType { Title = "Club" },
                new VenueType { Title = "Resort" },
                new VenueType { Title = "Pub" },
                new VenueType { Title = "Restaurant" },
                new VenueType { Title = "Cafe" },
                new VenueType { Title = "Casino" },
                new VenueType { Title = "Spa" },
                new VenueType { Title = "Other" }
            };
            venueTypes.ForEach(v => context.VenueTypes.Add(v));


            context.Database.ExecuteSqlCommand(
                @"
CREATE TABLE [dbo].[LocationSearchLogs](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [LogDate] [datetime] NOT NULL,
    [Lat] [float] NOT NULL,
    [Lng] [float] NOT NULL,
    [VenueNames] [varchar](max) NOT NULL,
    [UserAgent] [varchar](max) NULL,
 CONSTRAINT [PK_LocationSearchLogs] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]");

            context.Database.ExecuteSqlCommand(
                @"
ALTER TABLE [dbo].[LocationSearchLogs] ADD  CONSTRAINT [DF_LocationSearchLogs_LogDate]  DEFAULT (getdate()) FOR [LogDate]");

            context.Database.ExecuteSqlCommand(@"
SET IDENTITY_INSERT Countries ON
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (1, 'AU', 'Australia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (2, 'US', 'United States', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (3, 'CA', 'Canada', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (4, 'AF', 'Afghanistan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (5, 'AL', 'Albania', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (6, 'DZ', 'Algeria', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (7, 'DS', 'American Samoa', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (8, 'AD', 'Andorra', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (9, 'AO', 'Angola', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (10, 'AI', 'Anguilla', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (11, 'AQ', 'Antarctica', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (12, 'AG', 'Antigua and/or Barbuda', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (13, 'AR', 'Argentina', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (14, 'AM', 'Armenia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (15, 'AW', 'Aruba', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (16, 'AT', 'Austria', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (17, 'AZ', 'Azerbaijan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (18, 'BS', 'Bahamas', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (19, 'BH', 'Bahrain', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (20, 'BD', 'Bangladesh', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (21, 'BB', 'Barbados', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (22, 'BY', 'Belarus', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (23, 'BE', 'Belgium', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (24, 'BZ', 'Belize', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (25, 'BJ', 'Benin', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (26, 'BM', 'Bermuda', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (27, 'BT', 'Bhutan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (28, 'BO', 'Bolivia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (29, 'BA', 'Bosnia and Herzegovina', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (30, 'BW', 'Botswana', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (31, 'BV', 'Bouvet Island', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (32, 'BR', 'Brazil', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (33, 'IO', 'British lndian Ocean Territory', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (34, 'BN', 'Brunei Darussalam', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (35, 'BG', 'Bulgaria', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (36, 'BF', 'Burkina Faso', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (37, 'BI', 'Burundi', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (38, 'KH', 'Cambodia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (39, 'CM', 'Cameroon', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (40, 'CV', 'Cape Verde', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (41, 'KY', 'Cayman Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (42, 'CF', 'Central African Republic', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (43, 'TD', 'Chad', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (44, 'CL', 'Chile', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (45, 'CN', 'China', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (46, 'CX', 'Christmas Island', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (47, 'CC', 'Cocos (Keeling) Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (48, 'CO', 'Colombia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (49, 'KM', 'Comoros', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (50, 'CG', 'Congo', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (51, 'CK', 'Cook Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (52, 'CR', 'Costa Rica', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (53, 'HR', 'Croatia (Hrvatska)', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (54, 'CU', 'Cuba', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (55, 'CY', 'Cyprus', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (56, 'CZ', 'Czech Republic', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (57, 'DK', 'Denmark', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (58, 'DJ', 'Djibouti', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (59, 'DM', 'Dominica', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (60, 'DO', 'Dominican Republic', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (61, 'TP', 'East Timor', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (62, 'EC', 'Ecudaor', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (63, 'EG', 'Egypt', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (64, 'SV', 'El Salvador', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (65, 'GQ', 'Equatorial Guinea', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (66, 'ER', 'Eritrea', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (67, 'EE', 'Estonia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (68, 'ET', 'Ethiopia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (69, 'FK', 'Falkland Islands (Malvinas)', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (70, 'FO', 'Faroe Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (71, 'FJ', 'Fiji', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (72, 'FI', 'Finland', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (73, 'FR', 'France', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (74, 'FX', 'France, Metropolitan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (75, 'GF', 'French Guiana', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (76, 'PF', 'French Polynesia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (77, 'TF', 'French Southern Territories', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (78, 'GA', 'Gabon', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (79, 'GM', 'Gambia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (80, 'GE', 'Georgia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (81, 'DE', 'Germany', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (82, 'GH', 'Ghana', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (83, 'GI', 'Gibraltar', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (84, 'GR', 'Greece', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (85, 'GL', 'Greenland', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (86, 'GD', 'Grenada', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (87, 'GP', 'Guadeloupe', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (88, 'GU', 'Guam', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (89, 'GT', 'Guatemala', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (90, 'GN', 'Guinea', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (91, 'GW', 'Guinea-Bissau', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (92, 'GY', 'Guyana', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (93, 'HT', 'Haiti', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (94, 'HM', 'Heard and Mc Donald Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (95, 'HN', 'Honduras', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (96, 'HK', 'Hong Kong', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (97, 'HU', 'Hungary', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (98, 'IS', 'Iceland', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (99, 'IN', 'India', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (100, 'ID', 'Indonesia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (101, 'IR', 'Iran (Islamic Republic of)', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (102, 'IQ', 'Iraq', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (103, 'IE', 'Ireland', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (104, 'IL', 'Israel', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (105, 'IT', 'Italy', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (106, 'CI', 'Ivory Coast', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (107, 'JM', 'Jamaica', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (108, 'JP', 'Japan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (109, 'JO', 'Jordan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (110, 'KZ', 'Kazakhstan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (111, 'KE', 'Kenya', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (112, 'KI', 'Kiribati', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (113, 'KP', 'Korea, Democratic People''s Republic of', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (114, 'KR', 'Korea, Republic of', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (115, 'KW', 'Kuwait', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (116, 'KG', 'Kyrgyzstan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (117, 'LA', 'Lao People''s Democratic Republic', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (118, 'LV', 'Latvia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (119, 'LB', 'Lebanon', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (120, 'LS', 'Lesotho', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (121, 'LR', 'Liberia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (122, 'LY', 'Libyan Arab Jamahiriya', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (123, 'LI', 'Liechtenstein', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (124, 'LT', 'Lithuania', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (125, 'LU', 'Luxembourg', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (126, 'MO', 'Macau', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (127, 'MK', 'Macedonia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (128, 'MG', 'Madagascar', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (129, 'MW', 'Malawi', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (130, 'MY', 'Malaysia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (131, 'MV', 'Maldives', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (132, 'ML', 'Mali', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (133, 'MT', 'Malta', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (134, 'MH', 'Marshall Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (135, 'MQ', 'Martinique', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (136, 'MR', 'Mauritania', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (137, 'MU', 'Mauritius', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (138, 'TY', 'Mayotte', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (139, 'MX', 'Mexico', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (140, 'FM', 'Micronesia, Federated States of', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (141, 'MD', 'Moldova, Republic of', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (142, 'MC', 'Monaco', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (143, 'MN', 'Mongolia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (144, 'MS', 'Montserrat', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (145, 'MA', 'Morocco', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (146, 'MZ', 'Mozambique', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (147, 'MM', 'Myanmar', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (148, 'NA', 'Namibia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (149, 'NR', 'Nauru', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (150, 'NP', 'Nepal', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (151, 'NL', 'Netherlands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (152, 'AN', 'Netherlands Antilles', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (153, 'NC', 'New Caledonia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (154, 'NZ', 'New Zealand', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (155, 'NI', 'Nicaragua', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (156, 'NE', 'Niger', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (157, 'NG', 'Nigeria', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (158, 'NU', 'Niue', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (159, 'NF', 'Norfork Island', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (160, 'MP', 'Northern Mariana Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (161, 'NO', 'Norway', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (162, 'OM', 'Oman', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (163, 'PK', 'Pakistan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (164, 'PW', 'Palau', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (165, 'PA', 'Panama', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (166, 'PG', 'Papua New Guinea', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (167, 'PY', 'Paraguay', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (168, 'PE', 'Peru', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (169, 'PH', 'Philippines', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (170, 'PN', 'Pitcairn', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (171, 'PL', 'Poland', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (172, 'PT', 'Portugal', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (173, 'PR', 'Puerto Rico', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (174, 'QA', 'Qatar', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (175, 'RE', 'Reunion', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (176, 'RO', 'Romania', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (177, 'RU', 'Russian Federation', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (178, 'RW', 'Rwanda', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (179, 'KN', 'Saint Kitts and Nevis', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (180, 'LC', 'Saint Lucia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (181, 'VC', 'Saint Vincent and the Grenadines', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (182, 'WS', 'Samoa', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (183, 'SM', 'San Marino', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (184, 'ST', 'Sao Tome and Principe', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (185, 'SA', 'Saudi Arabia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (186, 'SN', 'Senegal', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (187, 'SC', 'Seychelles', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (188, 'SL', 'Sierra Leone', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (189, 'SG', 'Singapore', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (190, 'SK', 'Slovakia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (191, 'SI', 'Slovenia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (192, 'SB', 'Solomon Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (193, 'SO', 'Somalia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (194, 'ZA', 'South Africa', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (195, 'GS', 'South Georgia South Sandwich Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (196, 'ES', 'Spain', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (197, 'LK', 'Sri Lanka', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (198, 'SH', 'St. Helena', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (199, 'PM', 'St. Pierre and Miquelon', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (200, 'SD', 'Sudan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (201, 'SR', 'Suriname', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (202, 'SJ', 'Svalbarn and Jan Mayen Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (203, 'SZ', 'Swaziland', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (204, 'SE', 'Sweden', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (205, 'CH', 'Switzerland', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (206, 'SY', 'Syrian Arab Republic', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (207, 'TW', 'Taiwan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (208, 'TJ', 'Tajikistan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (209, 'TZ', 'Tanzania, United Republic of', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (210, 'TH', 'Thailand', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (211, 'TG', 'Togo', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (212, 'TK', 'Tokelau', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (213, 'TO', 'Tonga', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (214, 'TT', 'Trinidad and Tobago', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (215, 'TN', 'Tunisia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (216, 'TR', 'Turkey', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (217, 'TM', 'Turkmenistan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (218, 'TC', 'Turks and Caicos Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (219, 'TV', 'Tuvalu', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (220, 'UG', 'Uganda', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (221, 'UA', 'Ukraine', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (222, 'AE', 'United Arab Emirates', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (223, 'GB', 'United Kingdom', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (224, 'UM', 'United States minor outlying islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (225, 'UY', 'Uruguay', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (226, 'UZ', 'Uzbekistan', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (227, 'VU', 'Vanuatu', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (228, 'VA', 'Vatican City State', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (229, 'VE', 'Venezuela', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (230, 'VN', 'Vietnam', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (231, 'VG', 'Virigan Islands (British)', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (232, 'VI', 'Virgin Islands (U.S.)', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (233, 'WF', 'Wallis and Futuna Islands', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (234, 'EH', 'Western Sahara', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (235, 'YE', 'Yemen', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (236, 'YU', 'Yugoslavia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (237, 'ZR', 'Zaire', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (238, 'ZM', 'Zambia', 1, getDate(), getDate());
INSERT INTO Countries (id, Code, Title, [Status], CreateDate, UpdateDate) VALUES (239, 'ZW', 'Zimbabwe', 1, getDate(), getDate());
SET IDENTITY_INSERT Countries OFF

SET IDENTITY_INSERT States ON
INSERT INTO States (id, Code, Title, CountryId, [Status], CreateDate, UpdateDate) VALUES (1, 'ACT', 'Australian Capital Territory', 1, 1, getDate(), getDate());
INSERT INTO States (id, Code, Title, CountryId, [Status], CreateDate, UpdateDate) VALUES (2, 'NSW', 'New South Wales', 1, 1, getDate(), getDate());
INSERT INTO States (id, Code, Title, CountryId, [Status], CreateDate, UpdateDate) VALUES (3, 'NT', 'Northern Territory', 1, 1, getDate(), getDate());
INSERT INTO States (id, Code, Title, CountryId, [Status], CreateDate, UpdateDate) VALUES (4, 'QLD', 'Queensland', 1, 1, getDate(), getDate());
INSERT INTO States (id, Code, Title, CountryId, [Status], CreateDate, UpdateDate) VALUES (5, 'SA', 'South Australia', 1, 1, getDate(), getDate());
INSERT INTO States (id, Code, Title, CountryId, [Status], CreateDate, UpdateDate) VALUES (6, 'TAS', 'Tasmania', 1, 1, getDate(), getDate());
INSERT INTO States (id, Code, Title, CountryId, [Status], CreateDate, UpdateDate) VALUES (7, 'VIC', 'Victoria', 1, 1, getDate(), getDate());
INSERT INTO States (id, Code, Title, CountryId, [Status], CreateDate, UpdateDate) VALUES (8, 'WA', 'Western Australia', 1, 1, getDate(), getDate());
SET IDENTITY_INSERT States OFF

");

            #endregion
        }

    }
}