using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DohomeApplicationAPI.Models.RequestPriceTagModel
{
    public class OGetMasterArticle
    {
        public bool status { get; set; }
        public string message { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public string promotionDoc { get; set; }
        public List<string> listBinCode { get; set; }
        public List<UnitItem> listUnit { get; set; }
    }
    


}