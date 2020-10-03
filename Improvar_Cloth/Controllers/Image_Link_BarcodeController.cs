using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
using System.Data;
using System.IO;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Image_Link_BarcodeController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        //BarCode CnBarCode = new BarCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string sql = "";
        string LOC = CommVar.Loccd(CommVar.getQueryStringUNQSNO()), COM = CommVar.Compcd(CommVar.getQueryStringUNQSNO()), scm1 = CommVar.CurSchema(CommVar.getQueryStringUNQSNO()), scmf = CommVar.FinSchema(CommVar.getQueryStringUNQSNO());

        // GET: Image_Link_Barcode
        public ActionResult Image_Link_Barcode(string reptype = "")
        {
            ImageLinkBarcode VE = new ImageLinkBarcode();
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {                   
                    ViewBag.formname = "Image Linkup with Barcode";
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    //ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(),  CommVar.CurSchema(UNQSNO));
                    using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                    {
                        DataTable repformat = Salesfunc.getRepFormat(VE.RepType, VE.DOCCD);
                        if (repformat != null)
                        {
                            VE.DropDown_list1 = (from DataRow dr in repformat.Rows
                                                 select new DropDown_list1()
                                                 {
                                                     text = dr["text"].ToString(),
                                                     value = dr["value"].ToString()
                                                 }).ToList();
                        }
                        else
                        {
                            List<DropDown_list1> drplst = new List<DropDown_list1>();
                            VE.DropDown_list1 = drplst;
                        }

                        VE.DOCNM = (from j in DB.M_DOCTYPE where j.DOCCD == VE.DOCCD select j.DOCNM).SingleOrDefault();
                        //VE = (Image_Link_Barcode)Cn.EntryCommonLoading(VE, VE.PermissionID);
                        VE.DefaultView = true;
                        VE.ExitMode = 1;
                        VE.DefaultDay = 0;
                        return View(VE);
                    }
                }
            }
            catch (Exception ex)
            {
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }

        }
        public ActionResult DeleteRowMEASURE(ItemMasterEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                VE.Database_Combo2 = (from n in DB.M_SITEM_MEASURE
                                      select new Database_Combo2() { FIELD_VALUE = n.MDESC }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();


                List<MSITEMMEASURE> ITEMMEASURE = new List<MSITEMMEASURE>();
                int count = 0;
                for (int i = 0; i <= VE.MSITEMMEASURE.Count - 1; i++)
                {
                    if (VE.MSITEMMEASURE[i].Checked == false)
                    {
                        count += 1;
                        MSITEMMEASURE MEASURE = new MSITEMMEASURE();
                        MEASURE = VE.MSITEMMEASURE[i];
                        MEASURE.SLNO = Convert.ToByte(count);
                        ITEMMEASURE.Add(MEASURE);
                    }

                }
                VE.MSITEMMEASURE = ITEMMEASURE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_M_FinProduct_Measurement", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        private string getbarno(string ITCD)
        {
            var sql = "SELECT LISTAGG(barno,',') WITHIN GROUP (ORDER BY barno) AS barno FROM " + CommVar.CurSchema(UNQSNO) + ".M_SITEM_BARCODE where itcd='" + ITCD + "'";
            DataTable dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["barno"].retStr();
            }
            else
            {
                return "";
            }
        }


        public Tuple<List<M_BATCH_IMG_HDR>> SaveBarImage(string BarImage, string BARNO, short EMD)
        {
            List<M_BATCH_IMG_HDR> doc = new List<M_BATCH_IMG_HDR>();
            int slno = 0;
            try
            {
                var BarImages = BarImage.retStr().Trim(Convert.ToChar(Cn.GCS())).Split(Convert.ToChar(Cn.GCS()));
                foreach (string image in BarImages)
                {
                    if (image != "")
                    {
                        var imagedes = image.Split('~');
                        M_BATCH_IMG_HDR mdoc = new M_BATCH_IMG_HDR();
                        mdoc.CLCD = CommVar.ClientCode(UNQSNO);
                        mdoc.EMD_NO = EMD;
                        mdoc.SLNO = Convert.ToByte(++slno);
                        mdoc.DOC_CTG = "PRODUCT";
                        var extension = Path.GetExtension(imagedes[0]);
                        mdoc.DOC_FLNAME = BARNO + "_" + slno + extension;
                        mdoc.DOC_DESC = imagedes[1].retStr().Replace('~', ' ');
                        mdoc.BARNO = BARNO;
                        mdoc.DOC_EXTN = extension;
                        doc.Add(mdoc);
                        string topath = CommVar.SaveFolderPath() + "/ItemImages/" + mdoc.DOC_FLNAME;
                        topath = Path.Combine(topath, "");
                        string frompath = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments/" + imagedes[0]);
                        Cn.CopyImage(frompath, topath);
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, BarImage);
            }
            var result = Tuple.Create(doc);
            return result;
        }
        public ActionResult UploadImages(string ImageStr, string ImageName, string ImageDesc)
        {
            try
            {
                var extension = Path.GetExtension(ImageName);
                string filename = "I".retRepname() + extension;
                var link = Cn.SaveImage(ImageStr, "/UploadDocuments/" + filename);
                return Content("/UploadDocuments/" + filename);
            }
            catch (Exception ex)
            {
                return Content("//.");
            }

        }
    }
}