﻿using Com.Danliris.Service.Sales.Lib.Utilities.BaseClass;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Com.Danliris.Service.Sales.Lib.Models.SalesInvoice
{
    public class SalesInvoiceModel : BaseModel
    {
        [MaxLength(255)]
        public string Code { get; set; }
        public long AutoIncreament { get; set; }
        [MaxLength(255)]
        public string SalesInvoiceNo { get; set; }
        [MaxLength(255)]
        public string SalesInvoiceType { get; set; }
        public DateTimeOffset SalesInvoiceDate { get; set; }
        public DateTimeOffset DueDate { get; set; }

        [MaxLength(255)]
        public string DeliveryOrderNo { get; set; }
        [MaxLength(255)]
        public string DebtorIndexNo { get; set; }

        /*DO Sales*/
        public int DOSalesId { get; set; }
        [MaxLength(255)]
        public string DOSalesNo { get; set; }

        /*Buyer*/
        public int BuyerId { get; set; }
        [MaxLength(255)]
        public string BuyerName { get; set; }
        [MaxLength(1000)]
        public string BuyerAddress { get; set; }
        [MaxLength(255)]
        public string BuyerNPWP { get; set; }
        [MaxLength(255)]
        public string IDNo { get; set; }

        /*Currency*/
        public int CurrencyId { get; set; }
        [MaxLength(255)]
        public string CurrencyCode { get; set; }
        [MaxLength(255)]
        public string CurrencySymbol { get; set; }
        public double CurrencyRate { get; set; }

        [MaxLength(255)]
        public string Disp { get; set; }
        [MaxLength(255)]
        public string Op { get; set; }
        [MaxLength(255)]
        public string Sc { get; set; }
        public bool UseVat { get; set; }
        [MaxLength(1000)]
        public string Remark { get; set; }


        public virtual ICollection<SalesInvoiceDetailModel> SalesInvoiceDetails { get; set; }
    }
}
