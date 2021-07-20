using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DohomeApplicationAPI.Models.RequestPriceTagModel
{
    public class IGetMasterArticle
    {
        [Required]
        public string code { get; set; }

        [RegularExpression("^[1-2]*$", ErrorMessage = "statusTypeSearch request number 1 or 2")]
        public int statusTypeSearch { get; set; }
        [RegularExpression("^[0-1]*$", ErrorMessage = "statusGetBincode request number 0 or 1")]
        public int statusGetBincode { get; set; }
        [Required]
        [StringLength(4, MinimumLength =4)]
        public string siteCode { get; set; } 
        
        [StringLength(4, MinimumLength =4)]
        public string slocCode { get; set; } 
    }
}