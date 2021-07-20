using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DohomeApplicationAPI.Models.RequestPriceTagModel
{
    public class ICheckPomotionPrice
    {
        [Required]
        public string productCode { get; set; }
        [Required]
        [StringLength(4, MinimumLength =4)]
        public string siteCode { get; set; } 
        [Required]
        public string unitCode { get; set; } 
    }
}