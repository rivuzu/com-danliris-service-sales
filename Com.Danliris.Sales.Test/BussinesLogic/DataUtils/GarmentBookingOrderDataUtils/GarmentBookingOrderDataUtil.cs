﻿using Com.Danliris.Sales.Test.BussinesLogic.Utils;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Facades.GarmentBookingOrderFacade;
using Com.Danliris.Service.Sales.Lib.Models.GarmentBookingOrderModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Danliris.Sales.Test.BussinesLogic.DataUtils.GarmentBookingOrderDataUtils
{
    public class GarmentBookingOrderDataUtil : BaseDataUtil<GarmentBookingOrderFacade, GarmentBookingOrder>
    {
        public GarmentBookingOrderDataUtil(GarmentBookingOrderFacade facade) : base(facade)
        {
        }

        public override GarmentBookingOrder GetNewData()
        {
            var bookingOrder = new GarmentBookingOrder
            {
                BuyerId = 1,
                BookingOrderNo = "BA1234",
                BuyerCode = "UnitCode",
                BuyerName = "UnitName",
                SectionId = 1,
                SectionCode = "SectionCode",
                SectionName = "SectionName",
                ConfirmedQuantity = 1,
                CanceledQuantity = 55,
                IsBlockingPlan = true,
                OrderQuantity = 9,
                ExpiredBookingQuantity = 9,
                HadConfirmed = true,
                Remark = "Remark",
                Items = new List<GarmentBookingOrderItem>()
            };

            bookingOrder.Items.Add(new GarmentBookingOrderItem
            {
                ComodityId = 1,
                ComodityCode = "ComodityCode",
                ComodityName = "ComodityName",
                ConfirmQuantity = 55,
                Remark = "Remark",
            });
            return bookingOrder;
        }
    }
}