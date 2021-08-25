using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
using System.Data;
using System.IO;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

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
                        //VE.DropDown_list_MTRLJOBCD = (from i in DB.M_MTRLJOBMST select new DropDown_list_MTRLJOBCD() { MTRLJOBCD = i.MTRLJOBCD, MTRLJOBNM = i.MTRLJOBNM }).OrderBy(s => s.MTRLJOBNM).ToList();
                        VE.DropDown_list_MTRLJOBCD = masterHelp.MTRLJOBCD_List();
                        foreach (var v in VE.DropDown_list_MTRLJOBCD)
                        {
                            if (v.MTRLJOBCD == "FS" || v.MTRLJOBCD == "PL")
                            {
                                v.Checked = true;
                            }

                        }
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
            var sql = "SELECT LISTAGG(barno,',') WITHIN GROUP (ORDER BY barno) AS barno FROM " + CommVar.CurSchema(UNQSNO) + ".t_ where itcd='" + ITCD + "'";
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
        public ActionResult GetBarCodeDetails(string val, string Code)
        {
            try
            {
                ImageLinkBarcode VE = new ImageLinkBarcode();
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string DOCDT = System.DateTime.Now.ToShortDateString();
                string BARNO = data[0].retSqlformat();
                //string GOCD = data[2].retStr() == "" ? "" : data[2].retStr().retSqlformat();
                //string PRCCD = data[3].retStr();
                string MTRLJOBCD = data[2].retSqlformat();
                bool exactbarno = data[1].retStr() == "Bar" ? true : false;
                if (MTRLJOBCD == "" || val == "") { MTRLJOBCD = data[3].retStr(); }
                var str = masterHelp.T_TXN_BARNO_help(val, VE.MENU_PARA, DOCDT,"","","", MTRLJOBCD,"", exactbarno,"", BARNO);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    string BARIMAGE = str.retCompValue("BARIMAGE");
                    if (BARIMAGE != "")
                    {
                        string newBarImgstr = "";
                        var brimgs = BARIMAGE.retStr().Split((char)179);
                        foreach (var barimg in brimgs)
                        {
                            string barfilename = barimg.Split('~')[0];
                            string barimgdesc = barimg.Split('~')[1];
                            newBarImgstr += (char)179 + CommVar.WebUploadDocURL(barfilename) + "~" + barimgdesc;
                            string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                            FROMpath = Path.Combine(FROMpath, "");
                            string TOPATH = CommVar.LocalUploadDocPath() + barfilename;
                            Cn.CopyImage(FROMpath, TOPATH);
                        }
                        newBarImgstr = newBarImgstr.TrimStart((char)179);
                        str = str.Replace(BARIMAGE, newBarImgstr);
                    }
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        private string barImagesCard(string id, string result,string ImageDesc)
        {
            var htm = "";
            htm += "<div class='col-lg-4' id='" + id + "'>";
            htm += "       <div class='thumbnail'>";
            htm += "           <button type='button' style='position:absolute;top:5px;right:11px;padding:0px 5px;cursor:pointer;border-radius:10px;' class='btn-danger' onclick= deleteBarImages('" + id + "')>X</button>";
            htm += "           <a href='" + result + "' target='_blank'>";
            htm += "                <img src='" + result + "' alt='' style='width:100%;height:300px;'>";
            htm += "                <div class='caption'>";
            htm += "                   " + ImageDesc;
            htm += "          </div>";
            htm += "      </a>";
            htm += "  </div>";
            htm += "</div>";
            return htm;
        }

        public Tuple<List<T_BATCH_IMG_HDR>> SaveBarImage(string BarImage, string BARNO, short EMD)
        {
            List<T_BATCH_IMG_HDR> doc = new List<T_BATCH_IMG_HDR>();
            int slno = 0;
            try
            {
                var BarImages = BarImage.retStr().Trim(Convert.ToChar(Cn.GCS())).Split(Convert.ToChar(Cn.GCS()));
                foreach (string image in BarImages)
                {
                    if (image != "")
                    {
                        var imagedes = image.Split('~');
                        T_BATCH_IMG_HDR mdoc = new T_BATCH_IMG_HDR();
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
                        string tmpdir = CommVar.SaveFolderPath() + "/ItemImages/";
                        if (!Directory.Exists(tmpdir)) Directory.CreateDirectory(tmpdir);
                        string topath = tmpdir + mdoc.DOC_FLNAME;
                        topath = Path.Combine(topath, "");
                        var addarr = imagedes[0].Split('/');
                        var tempimgName = (addarr[addarr.Length - 1]);
                        string frompath = CommVar.LocalUploadDocPath(tempimgName);
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
                //var link = Cn.SaveImage(ImageStr, "/UploadDocuments/" + filename);
                //return Content("/UploadDocuments/" + filename);
                var folderpath = CommVar.LocalUploadDocPath(filename);
                var link = Cn.SaveImage(ImageStr, folderpath);
                var path = CommVar.WebUploadDocURL(filename);
                return Content(path);
            }
            catch (Exception ex)
            {
                return Content("//.");
            }

        }
        public ActionResult Save(FormCollection FC, ImageLinkBarcode VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));          
            string dbsql = "", sql = "", query = "";
            Int16 emdno = 1;
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    var schnm = CommVar.CurSchema(UNQSNO);
                    var CLCD = CommVar.ClientCode(UNQSNO);
                    //sql = "update " + schnm + ". T_BATCH_IMG_HDR set  BARNO= '" + VE.BARNO + "', CLCD='" + CLCD + "',DOC_DESC='F' ";
                    //sql += " where BARNO='" + VE.BARNO + "' ";

                    DB.T_BATCH_IMG_HDR.Where(x => x.BARNO == VE.BARNO).ToList().ForEach(x => { x.DTAG = "E"; });
                    DB.T_BATCH_IMG_HDR.RemoveRange(DB.T_BATCH_IMG_HDR.Where(x => x.BARNO == VE.BARNO));
                    DB.T_BATCH_IMG_HDR_LINK.Where(x => x.BARNO == VE.BARNO).ToList().ForEach(x => { x.DTAG = "E"; });
                    DB.T_BATCH_IMG_HDR_LINK.RemoveRange(DB.T_BATCH_IMG_HDR_LINK.Where(x => x.BARNO == VE.BARNO));
                    DB.SaveChanges();
                    if (VE.BarImages.retStr() != "")
                    {
                        var barimg = SaveBarImage(VE.BarImages, VE.BARNO, emdno);
                        DB.T_BATCH_IMG_HDR.AddRange(barimg.Item1);
                        DB.SaveChanges();
                        var disntImgHdr = barimg.Item1.GroupBy(u => u.BARNO).Select(r => r.First()).ToList();
                        foreach (var imgbar in disntImgHdr)
                        {
                            T_BATCH_IMG_HDR_LINK m_batchImglink = new T_BATCH_IMG_HDR_LINK();
                            m_batchImglink.CLCD = CommVar.ClientCode(UNQSNO);
                            m_batchImglink.EMD_NO = emdno;
                            m_batchImglink.BARNO = imgbar.BARNO;
                            m_batchImglink.MAINBARNO = imgbar.BARNO;
                            DB.T_BATCH_IMG_HDR_LINK.Add(m_batchImglink);
                        }
                    }
                    ContentFlg = "1";
                    DB.SaveChanges();
                    transaction.Commit();
                    return Content(ContentFlg);
                }
                catch (Exception ex)
                {
                    DB.SaveChanges();
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
    }
}