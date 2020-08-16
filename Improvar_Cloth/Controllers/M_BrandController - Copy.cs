using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Collections;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Net;
using System.Net.Sockets;

namespace Improvar.Controllers
{
    public class M_BrandController : Controller
    {
        Connection Cn = new Connection();
        M_BRAND sl;
        M_CNTRL_HDR sll;
        // GET: M_Brand
        //public const char Separator = ((char)181);
        public ActionResult M_Brand(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ViewBag.formname = "Brand Master";
                string SCHEMA = Cn.Getschema;
                Cn.M_AUTONO(Session["DatabaseSchemaName"].ToString());
                string loca1 = Session["CompanyLocationCode"].ToString().Trim();
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
                var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Session["DatabaseSchemaName"].ToString());
                BrandEntry VE = new BrandEntry();

                string[] PAGE_DATA = Cn.PAGE_DATA(Request.Url.AbsoluteUri, VE);
                VE = (BrandEntry)Cn.EntryCommonLoading(VE, VE.PermissionID);
                VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);


                VE.DefaultAction = op;
                if (op.Length != 0)
                {
                    VE = (BrandEntry)Cn.EntryCommonLoadingAfterAction(VE, op);

                    VE.IndexKey = (from p in DB.M_BRAND select new IndexKey() { Navikey = p.BRANDCD }).OrderBy(a => a.Navikey).ToList();

                    if (op == "E" || op == "D" || op == "V")
                    {
                        var MDT = (from j in DB.M_BRAND
                                   join o in DB.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                                   where (j.M_AUTONO == o.M_AUTONO)
                                   select new Searching()
                                   {
                                       Col1 = j.BRANDCD,
                                       Col2 = j.BRANDNM,
                                       Col11 = o.M_AUTONO
                                   }).OrderBy(s => s.Col1).ToList();

                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string JR = javaScriptSerializer.Serialize(MDT);
                        VE.JsonString = JR;
                        VE.Header = "Brand Name" + Cn.GCS() + "Brand Code" + Cn.GCS() + "AUTO NO";
                        VE.NativHeader = "Col2" + Cn.GCS() + "Col1" + Cn.GCS() + "Col11";
                        VE.hidecolumn = "2";
                        VE.returnindex = "1";
                        if (searchValue.Length != 0)
                        {
                            VE.Index = Nindex;
                            VE = Navigation(VE, DB, 0, searchValue);
                        }
                        else
                        {
                            if (key == "F")
                            {
                                VE.Index = 0;
                                VE = Navigation(VE, DB, 0, searchValue);
                            }
                            else if (key == "" || key == "L")
                            {
                                VE.Index = VE.IndexKey.Count - 1;
                                VE = Navigation(VE, DB, VE.IndexKey.Count - 1, searchValue);
                            }
                            else if (key == "P")
                            {
                                Nindex -= 1;
                                if (Nindex < 0)
                                {
                                    Nindex = 0;
                                }
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, Nindex, searchValue);
                            }
                            else if (key == "N")
                            {
                                Nindex += 1;
                                if (Nindex > VE.IndexKey.Count - 1)
                                {
                                    Nindex = VE.IndexKey.Count - 1;
                                }
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, Nindex, searchValue);
                            }
                        }
                        VE.M_BRAND = sl;
                        VE.M_CNTRL_HDR = sll;
                    }
                    if (op.ToString() == "A")
                    {
                        List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                        UploadDOC UPL = new UploadDOC();
                        UPL.DocumentType = doctP;
                        UploadDOC1.Add(UPL);
                        VE.UploadDOC = UploadDOC1;
                    }
                    return View(VE);

                }
                else
                {
                    VE.DefaultView = false;
                    VE.DefaultDay = 0;
                    return View(VE);
                }
            }
        }
        public BrandEntry Navigation(BrandEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_BRAND(); sll = new M_CNTRL_HDR();
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
            if (VE.IndexKey.Count != 0)
            {
                string[] aa = null;
                if (searchValue.Length == 0)
                {
                    aa = VE.IndexKey[index].Navikey.Split(Convert.ToChar(Cn.GCS()));
                }
                else
                {
                    aa = searchValue.Split(Convert.ToChar(Cn.GCS()));
                }
                sl = DB.M_BRAND.Find(aa[0]);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);

                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
                VE.UploadDOC = Cn.GetUploadImage(Session["DatabaseSchemaName"].ToString(), Convert.ToInt32(sl.M_AUTONO));

                if (VE.UploadDOC.Count == 0)
                {
                    List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                    UploadDOC UPL = new UploadDOC();
                    UPL.DocumentType = doctP;
                    UploadDOC1.Add(UPL);
                    VE.UploadDOC = UploadDOC1;
                }
            }

            return VE;
        }
        public ActionResult CheckBrandCode(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Session["DatabaseSchemaName"].ToString());
            var query = (from c in DB.M_BRAND where (c.BRANDCD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }

        }
        public ActionResult AddDOCRow(BrandEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
            var doctP = (from i in DB1.MS_DOCCTG
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            if (VE.UploadDOC == null)
            {
                List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                UploadDOC MLI = new UploadDOC();
                MLI.DocumentType = doctP;
                MLocIFSC1.Add(MLI);
                VE.UploadDOC = MLocIFSC1;
            }
            else
            {
                List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                {
                    UploadDOC MLI = new UploadDOC();
                    MLI = VE.UploadDOC[i];
                    MLI.DocumentType = doctP;
                    MLocIFSC1.Add(MLI);
                }
                UploadDOC MLI1 = new UploadDOC();
                MLI1.DocumentType = doctP;
                MLocIFSC1.Add(MLI1);
                VE.UploadDOC = MLocIFSC1;
            }
            VE.DefaultView = true;
            return PartialView("_UPLOADDOCUMENTS", VE);

        }
        public ActionResult DeleteDOCRow(BrandEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
            var doctP = (from i in DB1.MS_DOCCTG
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            List<UploadDOC> LOCAIFSC = new List<UploadDOC>();
            int count = 0;
            for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
            {
                if (VE.UploadDOC[i].chk == false)
                {
                    count += 1;
                    UploadDOC IFSC = new UploadDOC();
                    IFSC = VE.UploadDOC[i];
                    IFSC.DocumentType = doctP;
                    LOCAIFSC.Add(IFSC);
                }
            }
            VE.UploadDOC = LOCAIFSC;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_UPLOADDOCUMENTS", VE);

        }
        public ActionResult SAVE(FormCollection FC, BrandEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Session["DatabaseSchemaName"].ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + Session["DatabaseSchemaName"].ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_BRAND MBRAND = new M_BRAND();
                        MBRAND.CLCD = Session["CLIENT_CODE"].ToString();
                        if (VE.DefaultAction == "A")
                        {
                            MBRAND.EMD_NO = 0;
                            MBRAND.M_AUTONO = Cn.M_AUTONO(Session["DatabaseSchemaName"].ToString());
                        }
                        if (VE.DefaultAction == "E")
                        {
                            MBRAND.DTAG = "E";
                            MBRAND.M_AUTONO = VE.M_BRAND.M_AUTONO;

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO));

                            var MAXEMDNO = (from p in DB.M_BRAND where p.M_AUTONO == VE.M_BRAND.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MBRAND.EMD_NO = 0;
                            }
                            else
                            {
                                MBRAND.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                        }
                        MBRAND.BRANDCD = VE.M_BRAND.BRANDCD.ToUpper();
                        MBRAND.BRANDNM = VE.M_BRAND.BRANDNM;
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_BRAND", MBRAND.M_AUTONO, VE.DefaultAction, Session["DatabaseSchemaName"].ToString());

                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.M_BRAND.Add(MBRAND);
                            DB.SaveChanges();
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MBRAND).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_BRAND", VE.UploadDOC, MBRAND.M_AUTONO, MBRAND.EMD_NO.Value);
                            DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                            DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                        }
                        DB.SaveChanges();
                        transaction.Commit();
                        string ContentFlg = "";
                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1";
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        return Content(ContentFlg);
                    }
                    else if (VE.DefaultAction == "V")
                    {
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_BRAND", VE.M_BRAND.M_AUTONO, VE.DefaultAction, Session["DatabaseSchemaName"].ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_BRAND.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_BRAND.RemoveRange(DB.M_BRAND.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_BRAND.M_AUTONO));
                        DB.SaveChanges();
                        transaction.Commit();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                }
                catch (Exception ex)
                {
                    return Content(ex.Message + ex.InnerException);
                }
            }

        }
    }
}