using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class VenueReportViewModel
    {
        public VenueReportViewModel()
        {
            this.SelectedMenuIds = new int[] {};
            this.SelectediPadIds = new int[] {};
        }

        public int[] SelectediPadIds
        {
            get;
            set;
        }

        public int[] SelectedMenuIds
        {
            get;
            set;
        }

        public DateTime? DateFrom
        {
            get;
            set;
        }

        public DateTime? DateTo
        {
            get;
            set;
        }

        [Range(0, 2359, ErrorMessage = "Please enter a valid 'from' time (24 hour format)")]
        [RegularExpression("[0-2][0-9][0-5][0-9]",
            ErrorMessage = "Please enter a valid 'from' time (24 hour format - 0000)")]
        public string TimeFrom
        {
            get;
            set;
        }

        [Range(0, 2359, ErrorMessage = "Please enter a valid 'to' time (24 hour format)")]
        [RegularExpression("[0-2][0-9][0-5][0-9]",
            ErrorMessage = "Please enter a valid 'to' time (24 hour format - 0000)")]
        public string TimeTo
        {
            get;
            set;
        }

        public List<iPad> iPads
        {
            get;
            set;
        }

        public List<Menu> Menus
        {
            get;
            set;
        }

        public List<VenueDeviceReportData> DeviceReportData
        {
            get;
            set;
        }

        public string GetDeviceReportDataAsCsv()
        {
            var stringBuilder = new StringBuilder();
            var charsNeedToBeEncoded = new[]
                                           {
                                               '\n',
                                               '\r',
                                               '"',
                                               ','
                                           };
            Action<StringBuilder, string> writeStringField = (sb, value) =>
                                                                 {
                                                                     if (value == null)
                                                                     {
                                                                         return;
                                                                     }

                                                                     if (value.IndexOfAny(charsNeedToBeEncoded) > -1)
                                                                     {
                                                                         sb.Append("\"");
                                                                         sb.Append(value.Replace("\"",
                                                                                                 "\"\""));
                                                                         sb.Append("\"");
                                                                     }
                                                                     else
                                                                     {
                                                                         sb.Append(value);
                                                                     }
                                                                 };
            writeStringField(stringBuilder, this.GetReportHeader());
            stringBuilder.Append("\r\n");
            foreach (VenueDeviceReportData data in this.DeviceReportData)
            {
                writeStringField(stringBuilder,
                                 data.DeviceName);
                stringBuilder.Append(",");
                writeStringField(stringBuilder,
                                 data.Menu);
                stringBuilder.Append(",");
                writeStringField(stringBuilder,
                                 data.Category);
                stringBuilder.Append(",");
                writeStringField(stringBuilder,
                                 data.Item);
                stringBuilder.Append(",");
                stringBuilder.Append(data.NumOrders);
                stringBuilder.Append(",");
                stringBuilder.Append(data.Total);
                stringBuilder.Append("\r\n");
            }

            return stringBuilder.ToString();
        }

        public string GetReportHeader()
        {
            var header = new StringBuilder();
            header.Append("Current Report: ");

            if (this.DateFrom.HasValue)
            {
                if (this.DateTo.HasValue)
                {
                    DateTime dateFrom, dateTo;
                    if (this.DateFrom > this.DateTo)
                    {
                        dateFrom = this.DateTo.Value;
                        dateTo = this.DateFrom.Value;
                    }
                    else
                    {
                        dateFrom = this.DateFrom.Value;
                        dateTo = this.DateTo.Value;
                    }

                    header.Append(dateFrom.ToString("D"));
                    header.Append(", ");
                    header.Append(dateTo.ToString("D"));
                }
                else
                {
                    header.Append(this.DateFrom.Value.ToString("D"));
                    header.Append(", ");
                    header.Append(DateTime.Now.ToString("D"));
                }
            }
            else if (this.DateTo.HasValue)
            {
                header.Append("Up to ");
                header.Append(this.DateTo.Value.ToString("D"));
            }
            else
            {
                header.Append("Up to ");
                header.Append(DateTime.Now.ToString("D"));
            }

            if (!string.IsNullOrWhiteSpace(this.TimeFrom) || !string.IsNullOrWhiteSpace(this.TimeTo))
            {
                header.Append("; ");
                if (!string.IsNullOrWhiteSpace(this.TimeFrom))
                {
                    header.Append(DateTime.ParseExact(this.TimeFrom,
                                                      "HHmm",
                                                      CultureInfo.CurrentCulture).ToString("h:mmtt"));
                }
                else
                {
                    header.Append("12:00AM");
                }

                header.Append(" to ");
                if (!string.IsNullOrWhiteSpace(this.TimeTo))
                {
                    header.Append(DateTime.ParseExact(this.TimeTo,
                                                      "HHmm",
                                                      CultureInfo.CurrentCulture).ToString("h:mmtt"));
                }
                else
                {
                    header.Append("12:00AM");
                }
            }

            header.Append("; Details current as at: ");
            header.Append(DateTime.Now.ToString("F"));
            return header.ToString();
        }
    }
}