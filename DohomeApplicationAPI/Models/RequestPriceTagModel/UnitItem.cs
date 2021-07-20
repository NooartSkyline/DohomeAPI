using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DohomeApplicationAPI.Models.RequestPriceTagModel
{
    public class UnitItem
    {
        public string unitCode { get; set; }
        public string unitName { get; set; }
        public string barcode { get; set; }
        public string priceDate { get; set; }
        public decimal unitPrice { get; set; }
    }
}