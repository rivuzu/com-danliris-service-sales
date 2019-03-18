﻿using Com.Danliris.Service.Sales.Lib.BusinessLogic.Interface.GarmentBookingOrderInterface;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Logic.GarmentBookingOrderLogics;
using Com.Danliris.Service.Sales.Lib.Helpers;
using Com.Danliris.Service.Sales.Lib.Models.GarmentBookingOrderModel;
using Com.Danliris.Service.Sales.Lib.Services;
using Com.Danliris.Service.Sales.Lib.ViewModels.GarmentBookingOrderViewModels;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Com.Danliris.Service.Sales.Lib.BusinessLogic.Facades.GarmentBookingOrderFacade
{
    public class GarmentBookingOrderMonitoringFacade : IGarmentBookingOrderMonitoringInterface
    {
        private readonly SalesDbContext DbContext;
        private readonly DbSet<GarmentBookingOrder> DbSet;
        private IdentityService IdentityService;

        public GarmentBookingOrderMonitoringFacade(IServiceProvider serviceProvider, SalesDbContext dbContext)
        {
            this.DbContext = dbContext;
            this.DbSet = this.DbContext.Set<GarmentBookingOrder>(); ;
            this.IdentityService = serviceProvider.GetService<IdentityService>();
        }

        public IQueryable<GarmentBookingOrderMonitoringViewModel> GetReportQuery(string section, string no, string buyerCode, string comodityCode, string statusConfirm, string statusBookingOrder, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;
            var today = DateTime.Today;
            List<GarmentBookingOrderMonitoringViewModel> listGarmentBookingMonitoring = new List<GarmentBookingOrderMonitoringViewModel>();
            List<GarmentBookingOrderMonitoringViewModel> listGarmentBookingMonitoringFilter = new List<GarmentBookingOrderMonitoringViewModel>();


            var Query = (from a in DbContext.GarmentBookingOrders
                         join b in DbContext.GarmentBookingOrderItems on a.Id equals b.BookingOrderId into temp
                         from b in temp.DefaultIfEmpty()

                         where a.IsDeleted == false
                             && b.IsDeleted == false
                             && a.IsCanceled == false
                             && b.IsCanceled == false
                             && a.OrderQuantity > 0
                             && a.SectionCode == (string.IsNullOrWhiteSpace(section) ? a.SectionCode : section)
                             && a.BookingOrderNo == (string.IsNullOrWhiteSpace(no) ? a.BookingOrderNo : no)
                             && a.BuyerCode == (string.IsNullOrWhiteSpace(buyerCode) ? a.BuyerCode : buyerCode)
                             && b.ComodityCode == (string.IsNullOrWhiteSpace(comodityCode) ? b.ComodityCode : comodityCode)
                             && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
                             && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                         select new GarmentBookingOrderMonitoringViewModel
                         {
                             CreatedUtc = a.CreatedUtc,
                             BookingOrderNo = a.BookingOrderNo,
                             BookingOrderDate = a.BookingOrderDate,
                             BuyerName = a.BuyerName,
                             OrderQuantity = a.OrderQuantity,
                             DeliveryDate = a.DeliveryDate,
                             ComodityName = b.ComodityName,
                             ConfirmQuantity = b.ConfirmQuantity,
                             DeliveryDateItems = b.DeliveryDate,
                             ConfirmDate = b.ConfirmDate,
                             Remark = b.Remark,
                             StatusConfirm = a.ConfirmedQuantity == 0 ? "Belum Dikonfirmasi" : a.ConfirmedQuantity > 0 ? "Sudah Dikonfirmasi" : "-",
                             StatusBooking = a.IsBlockingPlan == true ? "Sudah Dibuat Master Plan" : a.ConfirmedQuantity == 0 && a.IsBlockingPlan == false ? "Booking" : a.ConfirmedQuantity > 0 && a.IsBlockingPlan == false ? "Confirmed" : "-",
                             OrderLeft = (a.OrderQuantity - a.ConfirmedQuantity).ToString(),
                             DateDiff = ((TimeSpan)(a.DeliveryDate.AddHours(offset) - today)).Days <= 45 && ((TimeSpan)(a.DeliveryDate.AddHours(offset) - today)).Days >= 0 ? ((TimeSpan)(a.DeliveryDate.AddHours(offset) - today)).Days.ToString() : "-",
                             row_count = 1,
                             LastModifiedUtc = a.LastModifiedUtc
                         }
            );

            foreach(var query in Query)
            {
                if (statusConfirm == "Belum Dikonfirmasi")
                {
                    if (query.StatusConfirm == statusConfirm)
                    {
                        listGarmentBookingMonitoring.Add(query);
                    }
                } else if (statusConfirm == "Sudah Dikonfirmasi")
                {
                    if (query.StatusConfirm == statusConfirm)
                    {
                        listGarmentBookingMonitoring.Add(query);
                    }
                } else
                {
                    listGarmentBookingMonitoring.Add(query);
                }
            }

            foreach(var queryFilter in listGarmentBookingMonitoring)
            {
                if (statusBookingOrder == "Sudah Dibuat Master Plan")
                {
                    if (queryFilter.StatusBooking == statusBookingOrder)
                    {
                        listGarmentBookingMonitoringFilter.Add(queryFilter);
                    }
                } else if (statusBookingOrder == "Booking")
                {
                    if (queryFilter.StatusBooking == statusBookingOrder)
                    {
                        listGarmentBookingMonitoringFilter.Add(queryFilter);
                    }
                } else if (statusBookingOrder == "Confirmed")
                {
                    if (queryFilter.StatusBooking == statusBookingOrder)
                    {
                        listGarmentBookingMonitoringFilter.Add(queryFilter);
                    }
                } else
                {
                    listGarmentBookingMonitoringFilter.Add(queryFilter);
                }
            }

            return listGarmentBookingMonitoringFilter.AsQueryable();
        }

        public Tuple<List<GarmentBookingOrderMonitoringViewModel>, int> Read(string section, string no, string buyerCode, string comodityCode, string statusConfirm, string statusBookingOrder, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
        {
            var Query = GetReportQuery(section, no, buyerCode, comodityCode, statusConfirm, statusBookingOrder, dateFrom, dateTo, offset);

            var Data = Query.OrderByDescending(b => b.LastModifiedUtc).ToList();

            Pageable<GarmentBookingOrderMonitoringViewModel> pageable = new Pageable<GarmentBookingOrderMonitoringViewModel>(Data, page - 1, size);
            List<GarmentBookingOrderMonitoringViewModel> Data_ = pageable.Data.ToList<GarmentBookingOrderMonitoringViewModel>();

            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data_, TotalData);
        }

        public MemoryStream GenerateExcel(string section, string no, string buyerCode, string comodityCode, string statusConfirm, string statusBookingOrder, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQuery(section, no, buyerCode, comodityCode, statusConfirm, statusBookingOrder, dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "Kode Booking", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Booking", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Buyer", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Order", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Pengeriman (booking)", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Komoditi", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Confirm", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Pengiriman (confirm)", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Confirm", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Status Confirm", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Status Booking Order", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Sisa Order (Belum Confirm)", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Seilisih Hari (dari Tanggal Pengiriman)", DataType = typeof(string) });

            List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                string temp_No = "";
                int rowPosition = 1;
                int counterTemp = 1;

                foreach (var item in Query.ToList())
                {
                    index++;
                    rowPosition++;
                    DateTimeOffset bookingOrderDate = item.BookingOrderDate ?? new DateTime(1970, 1, 1);
                    DateTimeOffset deliveryDate = item.DeliveryDate ?? new DateTime(1970, 1, 1);
                    DateTimeOffset confirmDate = item.ConfirmDate ?? new DateTime(1970, 1, 1);
                    DateTimeOffset deliveryDateItems = item.DeliveryDateItems ?? new DateTime(1970, 1, 1);

                    string BookingOrderDate = bookingOrderDate == new DateTime(1970, 1, 1) ? "-" : bookingOrderDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID"));
                    string DeliveryDate = deliveryDate == new DateTime(1970, 1, 1) ? "-" : deliveryDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID"));
                    string ConfirmDate = confirmDate == new DateTime(1970, 1, 1) ? "-" : confirmDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID"));
                    string DeliveryDateItems = deliveryDateItems == new DateTime(1970, 1, 1) ? "-" : deliveryDateItems.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID"));

                    if (temp_No == item.BookingOrderNo)
                    {
                        item.BookingOrderNo = null;
                        BookingOrderDate = null;
                        item.BuyerName = null;
                        item.OrderQuantity = null;
                        DeliveryDate = null;
                        item.StatusConfirm = null;
                        item.StatusBooking = null;
                        item.OrderLeft = null;
                        item.DateDiff = null;

                        counterTemp++;

                        result.Rows.Add(item.BookingOrderNo, BookingOrderDate, item.BuyerName, item.OrderQuantity, DeliveryDate, item.ComodityName, item.ConfirmQuantity,
                        DeliveryDateItems, ConfirmDate, item.Remark, item.StatusConfirm, item.StatusBooking, item.OrderLeft, item.DateDiff);

                       
                    } else
                    {
                        result.Rows.Add(item.BookingOrderNo, BookingOrderDate, item.BuyerName, item.OrderQuantity, DeliveryDate, item.ComodityName, item.ConfirmQuantity,
                        DeliveryDateItems, ConfirmDate, item.Remark, item.StatusConfirm, item.StatusBooking, item.OrderLeft, item.DateDiff);

                        if (counterTemp > 1)
                        {
                            mergeCells.Add(($"A{rowPosition - counterTemp}:A{rowPosition - 1}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                            mergeCells.Add(($"B{rowPosition - counterTemp}:B{rowPosition - 1}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                            mergeCells.Add(($"C{rowPosition - counterTemp}:C{rowPosition - 1}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                            mergeCells.Add(($"D{rowPosition - counterTemp}:D{rowPosition - 1}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                            mergeCells.Add(($"E{rowPosition - counterTemp}:E{rowPosition - 1}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                            mergeCells.Add(($"K{rowPosition - counterTemp}:K{rowPosition - 1}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                            mergeCells.Add(($"L{rowPosition - counterTemp}:L{rowPosition - 1}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                            mergeCells.Add(($"M{rowPosition - counterTemp}:M{rowPosition - 1}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                            mergeCells.Add(($"N{rowPosition - counterTemp}:N{rowPosition - 1}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));

                            counterTemp = 1;
                        }
                        temp_No = item.BookingOrderNo;
                    }
                }

                if (counterTemp > 1)
                {
                    mergeCells.Add(($"A{rowPosition + 1 - counterTemp}:A{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                    mergeCells.Add(($"B{rowPosition + 1 - counterTemp}:B{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                    mergeCells.Add(($"C{rowPosition + 1 - counterTemp}:C{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                    mergeCells.Add(($"D{rowPosition + 1 - counterTemp}:D{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                    mergeCells.Add(($"E{rowPosition + 1 - counterTemp}:E{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                    mergeCells.Add(($"K{rowPosition + 1 - counterTemp}:K{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                    mergeCells.Add(($"L{rowPosition + 1 - counterTemp}:L{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                    mergeCells.Add(($"M{rowPosition + 1 - counterTemp}:M{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
                    mergeCells.Add(($"N{rowPosition + 1 - counterTemp}:N{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Left, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));

                    counterTemp = 1;
                }
            }
            return Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "Report", mergeCells) }, true);
        }
    }
}
