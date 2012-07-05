using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Globalization;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class VenueReportService : IVenueReportService
    {
        private readonly IRepository<Order> _orderRepository;

        public VenueReportService(IRepository<Order> orderRepository)
        {
            this._orderRepository = orderRepository;
        }

        #region IVenueReportService Members

        public List<VenueDeviceReportData> GetVenueDeviceOrderSummary(int venueId, DateTime? dateFrom, DateTime? dateTo,
                                                                      string timeFrom, string timeTo,
                                                                      int[] selectediPads, int[] selectedMenus)
        {
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
            {
                DateTime? temp = dateFrom;
                dateFrom = dateTo;
                dateTo = temp;
            }

            this._orderRepository.SetContextOption(ContextOptions.ProxyCreation,
                                                   false);

            var orderItems = from order in this._orderRepository.FindAll()
                             from item in order.OrderItems
                             where
                                 order.OrderStatus == Order.OrderStatusFinalized &&
                                 item.OrderStatus == Order.OrderStatusFinalized &&
                                 order.VenueId == venueId
                             select item;

            // now will continue to build expression.. 
            // has start date filter?
            if (dateFrom.HasValue)
            {
                // just for sanity check remove time portion
                var date = new DateTime(dateFrom.Value.Year,
                                        dateFrom.Value.Month,
                                        dateFrom.Value.Day);
                orderItems = orderItems.Where(item => item.CreateDate >= date);
            }

            // has end date filter?
            if (dateTo.HasValue)
            {
                var date = new DateTime(dateTo.Value.Year,
                                        dateTo.Value.Month,
                                        dateTo.Value.Day).AddDays(1);
                orderItems = orderItems.Where(item => item.CreateDate < date);
            }

            // filter with start time if provided
            DateTime? startTime = string.IsNullOrWhiteSpace(timeFrom)
                                      ? (DateTime?) null
                                      : DateTime.ParseExact(timeFrom,
                                                            "HHmm",
                                                            CultureInfo.CurrentCulture);
            DateTime? endTime = string.IsNullOrWhiteSpace(timeTo)
                                    ? (DateTime?) null
                                    : DateTime.ParseExact(timeTo,
                                                          "HHmm",
                                                          CultureInfo.CurrentCulture);

            var timeRangeSpans2Days = startTime.HasValue && endTime.HasValue && endTime < startTime &&
                                      (endTime.Value.Hour != 0 || endTime.Value.Minute != 0);
            if (timeRangeSpans2Days)
            {
                int startTimeInSeconds = startTime.Value.Hour * 3600 + startTime.Value.Minute * 60;
                int endTimeInSeconds = endTime.Value.Hour * 3600 + endTime.Value.Minute * 60;
                orderItems = orderItems.Where(
                    item => (
                                SqlFunctions.DatePart("Hour",
                                                      item.CreateDate) * 3600 +
                                SqlFunctions.DatePart("Minute",
                                                      item.CreateDate) * 60 >= startTimeInSeconds &&
                                SqlFunctions.DatePart("Hour",
                                                      item.CreateDate) * 3600 +
                                SqlFunctions.DatePart("Minute",
                                                      item.CreateDate) * 60 <= 86399.999f
                            ) ||
                            (
                                SqlFunctions.DatePart("Hour",
                                                      item.CreateDate) * 3600 +
                                SqlFunctions.DatePart("Minute",
                                                      item.CreateDate) * 60 >= 0 &&
                                SqlFunctions.DatePart("Hour",
                                                      item.CreateDate) * 3600 +
                                SqlFunctions.DatePart("Minute",
                                                      item.CreateDate) * 60 <= endTimeInSeconds
                            ));
            }
            else
            {
                if (startTime.HasValue)
                {
                    int startTimeInMinutes = startTime.Value.Hour * 60 + startTime.Value.Minute;
                    orderItems = orderItems.Where(item => (SqlFunctions.DatePart("Hour",
                                                                                 item.CreateDate) * 60 +
                                                           SqlFunctions.DatePart("Minute",
                                                                                 item.CreateDate)) >= startTimeInMinutes);
                }

                // filter with end time if provided
                if (endTime.HasValue)
                {
                    float endTimeInSeconds = endTime.Value.Hour * 3600 + endTime.Value.Minute * 60;
                    if ((int) endTimeInSeconds == 0)
                    {
                        endTimeInSeconds = 86399.999f; //23:59:59.999
                    }

                    orderItems = orderItems.Where(item =>
                                                  (SqlFunctions.DatePart("Hour",
                                                                         item.CreateDate) * 3600 +
                                                   SqlFunctions.DatePart("Minute",
                                                                         item.CreateDate) * 60) <= endTimeInSeconds);
                }
            }

            // filter with selected ipads
            if (selectediPads != null && selectediPads.Length > 0)
            {
                orderItems = orderItems.Where(item => item.iPadId.HasValue && selectediPads.Contains(item.iPadId.Value));
            }

            if (selectedMenus != null && selectedMenus.Length > 0)
            {
                orderItems = orderItems.Where(item => selectedMenus.Contains(item.MenuItem.MenuCategory.MenuId));
            }

            // group order items by ipad and menu 
            var reportQuery = from item in orderItems
                              group item by new
                                                {
                                                    item.iPad,
                                                    item.MenuItem
                                                }
                              into g
                              let menuCategory = g.Key.MenuItem.MenuCategory
                              let menu = menuCategory.Menu
                              select new VenueDeviceReportData
                                         {
                                             DeviceName = g.Key.iPad.Name,
                                             Menu = menu.DisplayTitle,
                                             Category = menuCategory.Title,
                                             Item = g.Key.MenuItem.Title,
                                             NumOrders = g.Sum(item => item.Quantity),
                                             Total = g.Sum(item => item.Price)
                                         };

            // execute query and return result.
            return reportQuery.ToList();
        }

        #endregion
    }
}