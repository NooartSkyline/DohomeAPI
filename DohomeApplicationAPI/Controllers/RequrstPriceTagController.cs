using DohomeApplicationAPI.MMO001_GetBinAssignloc;
using DohomeApplicationAPI.Models.RequestPriceTagModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace DohomeApplicationAPI.Controllers
{
    public class RequrstPriceTagController : ApiController
    {
        [ResponseType(typeof(OGetMasterArticle))]
        [Route("RequrstPriceTag/GetMasterArticle")]
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult GetMasterArticle([FromUri] IGetMasterArticle inputItem)
        {
            ZMM_ASSIGNLOC[] arrAssign = null;
            OGetMasterArticle output = new OGetMasterArticle();
            //ค้นหาบินโค้ดด้วย
            if (inputItem.statusGetBincode == 1)
            {
                try
                {
                    SI_MMO001_ZHH_GET_BIN_ASSIGNLOC_Sync_oubClient client = new SI_MMO001_ZHH_GET_BIN_ASSIGNLOC_Sync_oubClient();
                    client.ClientCredentials.UserName.UserName = System.Configuration.ConfigurationManager.AppSettings["PIUserName"];
                    client.ClientCredentials.UserName.Password = System.Configuration.ConfigurationManager.AppSettings["PIPassword"];
                    string productCode;
                    if (inputItem.statusTypeSearch == 2)
                    {
                        productCode = inputItem.code;
                    }
                    else
                    {
                        using (DBContext.DBMasterDataContext db = new DBContext.DBMasterDataContext())
                        {
                            productCode = (from bar in db.TBMaster_Barcodes where bar.BARCODE == inputItem.code select bar.PRODUCTCODE).SingleOrDefault();
                        }
                        if (productCode == null)
                        {
                            output = new OGetMasterArticle();
                            output.status = false;
                            output.message = "ไม่พบรายการสินค้าของ Barcode : " + inputItem.code + " ติดต่อพัฒนาระบบ";
                            return Json(output);
                        }

                    }
                    client.SI_MMO001_ZHH_GET_BIN_ASSIGNLOC_Sync_oub("", inputItem.slocCode, productCode, inputItem.siteCode, ref arrAssign);
                    if (arrAssign == null)
                    {
                        output = new OGetMasterArticle();
                        output.status = false;
                        output.message = "ไม่พบตำแหน่งจัดเก็บของรายการสินค้า  : " + productCode + " ติดต่อพัฒนาระบบ\\nถ้าหากไม่ต้องการอ้างอิงตำแหน่งกรุณาติ๊ก Select Storage Bin ออก";
                        return Json(output);
                    }
                }
                catch (Exception ex)
                {
                    output = new OGetMasterArticle();
                    output.status = false;
                    output.message = ex.Message;
                    return Json(output);
                }
            }

            if (inputItem.statusTypeSearch == 1)//barcode
            {
                try
                {
                    //get article code from barcode
                    using (DBContext.DBMasterDataContext db = new DBContext.DBMasterDataContext())
                    {
                        output = (from bar in db.TBMaster_Barcodes
                                  join pro in db.TBMaster_Products on bar.PRODUCTCODE equals pro.CODE
                                  where bar.BARCODE == inputItem.code
                                  select new OGetMasterArticle
                                  {
                                      promotionDoc = GetProPice(inputItem.siteCode, bar.PRODUCTCODE, bar.UNITCODE),
                                      productCode = pro.CODE,
                                      productName = pro.NAMETH,
                                      listUnit = getListUnit(pro.CODE, false, inputItem.siteCode, db)
                                      //listUnit = getListUnit(inputItem.code, true, inputItem.siteCode, db)
                                  }).FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    output = new OGetMasterArticle();
                    output.status = false;
                    output.message = ex.Message;
                    return Json(output);
                }
            }
            else if (inputItem.statusTypeSearch == 2)//article
            {
                try
                {
                    using (DBContext.DBMasterDataContext db = new DBContext.DBMasterDataContext())
                    {
                        output = (from pro in db.TBMaster_Products
                                  where pro.CODE == inputItem.code
                                  select new OGetMasterArticle
                                  {
                                      productCode = pro.CODE,
                                      productName = pro.NAMETH,
                                      listUnit = getListUnit(pro.CODE, false, inputItem.siteCode, db)
                                  }).FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    output = new OGetMasterArticle();
                    output.status = false;
                    output.message = ex.Message;
                    return Json(output);
                }
            }
            else
            {
                output.status = false;
                output.message = "ฟิว statusTypeSearch ต้อง = 1 หรือ 2";
                return Json(output);
            }
            //เช็คว่าเจอรายการสินค้าไหม
            if (output == null)
            {
                output = new OGetMasterArticle();
                output.status = false;
                if (inputItem.statusTypeSearch == 1)
                    output.message = "ไม่พบรายการบาร์โค้ด : " + inputItem.code + " ติดต่อพัฒนาระบบ";
                else
                    output.message = "ไม่พบรายการสินค้า : " + inputItem.code + " ติดต่อพัฒนาระบบ";
                return Json(output);
            }
            else
            {
                if (output.listUnit == null ? true : output.listUnit.Count == 0)
                {
                    output.status = false;

                    if (inputItem.statusTypeSearch == 1)
                        output.message = "ไม่พบหน่วยบาร์โค้ด : " + inputItem.code + " ติดต่อพัฒนาระบบ";
                    else
                        output.message = "ไม่พบหน่วยสินค้า : " + inputItem.code + " ติดต่อพัฒนาระบบ";
                    return Json(output);
                }
            }

            output.status = true;
            if (inputItem.statusGetBincode == 1)
                output.listBinCode = createBinCode(arrAssign);
            return Json(output);
        }

        private string GetProPice(string siteCode, string productCode, string unitCode)
        {
            using (DBContext.DBMasterDataContext db = new DBContext.DBMasterDataContext())
            {
                string promotionCode = (from db_hd in db.TBMaster_Promotion_Price_HDs
                                        join db_dt in db.TBMaster_Promotion_Price_DTs on db_hd.DOCNO equals db_dt.DOCNO
                                        where db_hd.SITECODE == siteCode &&
                                              db_dt.MATNR == productCode &&
                                              db_dt.UNIT_CODE == unitCode &&
                                              db_hd.STATUS == "APPROVE" &&
                                             (DateTime.Now.Date >= db_hd.VALID_FROM && DateTime.Now.Date <= db_hd.VALID_TO)
                                        select db_hd.DOCNO).FirstOrDefault();

                return promotionCode;
            }
        }

        [ResponseType(typeof(OCheckPomotionPrice))]
        [Route("RequrstPriceTag/CheckPomotionPrice")]
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult CheckPomotionPrice([FromUri] ICheckPomotionPrice inputItem)
        {
            OCheckPomotionPrice output = new OCheckPomotionPrice();
            try
            {
                using (DBContext.DBMasterDataContext db = new DBContext.DBMasterDataContext())
                {
                    string promotionCode = (from db_hd in db.TBMaster_Promotion_Price_HDs
                                            join db_dt in db.TBMaster_Promotion_Price_DTs on db_hd.DOCNO equals db_dt.DOCNO
                                            where db_hd.SITECODE == inputItem.siteCode &&
                                                  db_dt.MATNR == inputItem.productCode &&
                                                  db_dt.UNIT_CODE == inputItem.unitCode &&
                                                  db_hd.STATUS == "APPROVE" &&
                                                 (DateTime.Now.Date >= db_hd.VALID_FROM && DateTime.Now.Date <= db_hd.VALID_TO)
                                            select db_hd.DOCNO).FirstOrDefault();
                    if (string.IsNullOrEmpty(promotionCode))
                    {
                        output.status = false;
                        output.message = "ไม่พบ Promotion รหัสสินค้า : " + inputItem.productCode + " หน่วย : " + inputItem.unitCode + " site : " + inputItem.siteCode;
                    }
                    else
                    {
                        output.status = true;
                        output.promotionCode = promotionCode;
                    }
                    return Json(output);
                }
            }
            catch (Exception ex)
            {
                output = new OCheckPomotionPrice();
                output.status = false;
                output.message = ex.Message;
                return Json(output);
            }

        }

        private List<string> createBinCode(ZMM_ASSIGNLOC[] arrAssign)
        {
            List<string> listBin = new List<string>();
            foreach (ZMM_ASSIGNLOC item in arrAssign)
            {
                listBin.Add(item.BIN_CODE);
            }
            return listBin;
        }

        private List<UnitItem> getListUnit(string cODE, bool isBarcode, string siteCode, DBContext.DBMasterDataContext db)
        {
            if (isBarcode)
            {
                return ((from un in db.TBMaster_Product_Units
                         join masUn in db.TBMaster_Units on un.UNITCODE equals masUn.CODE
                         join bar in db.TBMaster_Barcodes on new { un.UNITCODE, un.PRODUCTCODE } equals new { bar.UNITCODE, bar.PRODUCTCODE }

                         where bar.BARCODE == cODE
                         select new UnitItem
                         {
                             unitCode = un.UNITCODE,
                             unitName = masUn.MYNAME,
                             barcode = bar.BARCODE,
                             priceDate = GetdatePrice(siteCode, un.PRODUCTCODE, un.UNITCODE, db),//DateTime.Now.ToString("yyyy-MM-dd"),
                             unitPrice = CheckPro(siteCode, un.PRODUCTCODE, un.UNITCODE, db)

                         }).ToList());
            }
            else
            {
                return ((from un in db.TBMaster_Product_Units
                         join masUn in db.TBMaster_Units on un.UNITCODE equals masUn.CODE
                         join bar in db.TBMaster_Barcodes on new { un.UNITCODE, un.PRODUCTCODE } equals new { bar.UNITCODE, bar.PRODUCTCODE }

                         where un.PRODUCTCODE == cODE
                         select new UnitItem
                         {
                             unitCode = un.UNITCODE,
                             unitName = masUn.MYNAME,
                             barcode = bar.BARCODE,
                             priceDate = GetdatePrice(siteCode, un.PRODUCTCODE, un.UNITCODE, db),//DateTime.Now.ToString("yyyy-MM-dd"),
                             unitPrice = CheckPro(siteCode, un.PRODUCTCODE, un.UNITCODE, db)



                         }).ToList());
            }


        }

        private decimal getPrice(string cODE, string uNITCODE, string siteCode, DBContext.DBMasterDataContext db)
        {
            decimal sResult = (from sp in db.TBMaster_Sale_Prices
                               where sp.SITECODE == siteCode && sp.PRODUCTCODE == cODE && sp.UNITCODE == (uNITCODE) && sp.PAYMENTCODE == "0000"
                               && (DateTime.Now >= sp.BEGINDATE && DateTime.Now <= sp.ENDDATE)
                               orderby sp.CREATEDATE descending
                               select sp.SALEPRICE).FirstOrDefault();
            //conn = configserver.StringConn(site); 


            return sResult;
        }
        public decimal GetproPice(string siteCode, string proDuctCode, string unitCode)
        {
            OCheckPomotionPrice output = new OCheckPomotionPrice();
            decimal proPice;
            try
            {
                using (DBContext.DBMasterDataContext db = new DBContext.DBMasterDataContext())
                {
                    string promotionCode = (from db_hd in db.TBMaster_Promotion_Price_HDs
                                            join db_dt in db.TBMaster_Promotion_Price_DTs on db_hd.DOCNO equals db_dt.DOCNO
                                            where db_hd.SITECODE == siteCode &&
                                                  db_dt.MATNR == proDuctCode &&
                                                  db_dt.UNIT_CODE == unitCode &&
                                                  db_hd.STATUS == "APPROVE" &&
                                                 (DateTime.Now.Date >= db_hd.VALID_FROM && DateTime.Now.Date <= db_hd.VALID_TO)
                                            select db_dt.PROMO_PRICE.ToString()).FirstOrDefault();
                    if (string.IsNullOrEmpty(promotionCode))
                    {
                        proPice = 0;
                    }
                    else
                    {
                        proPice = decimal.Parse(promotionCode);
                    }
                    return proPice;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        private string GetdatePrice(string site, string productCode, string unitcode, DBContext.DBMasterDataContext db)
        {
            decimal proPrice = GetproPice(site, productCode, unitcode);
            DateTime sDate;
            if (proPrice == 0)
            {
                sDate = (from da in db.TBMaster_Sale_Prices
                         where da.SITECODE == site && da.PRODUCTCODE == productCode && da.UNITCODE == unitcode && da.PAYMENTCODE == "0000"
                        && (DateTime.Now >= da.BEGINDATE && DateTime.Now <= da.ENDDATE)
                         orderby da.CREATEDATE descending
                         select da.BEGINDATE).FirstOrDefault();
            }
            else
            {

               sDate = (DateTime)(from db_hd in db.TBMaster_Promotion_Price_HDs
                                        join db_dt in db.TBMaster_Promotion_Price_DTs on db_hd.DOCNO equals db_dt.DOCNO
                                        where db_hd.SITECODE == site &&
                                              db_dt.MATNR == productCode &&
                                              db_dt.UNIT_CODE == unitcode &&
                                              db_hd.STATUS == "APPROVE" &&
                                             (DateTime.Now.Date >= db_hd.VALID_FROM && DateTime.Now.Date <= db_hd.VALID_TO)
                                        select db_hd.VALID_FROM).FirstOrDefault();
            }

            return sDate.ToString("yyyy-MM-dd");
        }


        private decimal CheckPro(string site, string procode, string unitcode, DBContext.DBMasterDataContext db)
        {
            decimal sss = GetproPice(site, procode, unitcode) == 0 ? getPrice(procode, unitcode, site, db) : GetproPice(site, procode, unitcode);
            return sss;
        }
    }
}
