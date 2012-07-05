using System;
using System.Collections.Generic;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IVenueReportService
    {
        List<VenueDeviceReportData> GetVenueDeviceOrderSummary(int venueId, DateTime? dateFrom, DateTime? dateTo,
                                                               string timeFrom, string timeTo, int[] selectediPads,
                                                               int[] selectedMenus);
    }
}