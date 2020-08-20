using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class M_SubBrandController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        M_SUBBRAND sl;
        M_BRAND slb;
        M_CNTRL_HDR sll;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_SubBrand
        public ActionResult M_SubBrand(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Sub Brand Master";
                    string SCHEMA = Cn.Getschema;
                    Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    SubBrandEntry VE = new SubBrandEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    //VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    VE.DefaultAction = op;
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_SUBBRAND orderby p.SBRANDCD select new IndexKey() { Navikey = p.SBRANDCD }).ToList();

                        if (op == "E" || op == "D" || op == "V")
                        {

                            if (searchValue.Length != 0)
                            {
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, 0, searchValue);
                            }
                            else
                            {
                                if (key == "" || key == "F")
                                {
                                    VE.Index = 0;
                                    VE = Navigation(VE, DB, 0, searchValue);
                                }
                                else if (key == "L")
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
                            VE.M_SUBBRAND = sl;
                            VE.M_BRAND = slb;
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
            catch (Exception ex)
            {
                SubBrandEntry VE = new SubBrandEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public SubBrandEntry Navigation(SubBrandEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_SUBBRAND(); slb = new M_BRAND(); sll = new M_CNTRL_HDR();
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
                sl = DB.M_SUBBRAND.Find(aa[0]);
                slb = DB.M_BRAND.Find(sl.MBRANDCD);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);

                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
                VE.UploadDOC = Cn.GetUploadImage(CommVar.CurSchema(UNQSNO).ToString(), Convert.ToInt32(sl.M_AUTONO));

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
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var MDT = (from j in DB.M_SUBBRAND
                       join o in DB.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                       join p in DB.M_BRAND on j.MBRANDCD equals (p.BRANDCD)
                       where (j.M_AUTONO == o.M_AUTONO)
                       select new
                       {
                           SBRANDCD = j.SBRANDCD,
                           SBRANDNM = j.SBRANDNM,
                           MBRANDCD = j.MBRANDCD,
                           BRANDNM = p.BRANDNM,
                           M_AUTONO = o.M_AUTONO
                       }).OrderBy(s => s.SBRANDCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Sub Brand Code" + Cn.GCS() + "Sub Brand Name" + Cn.GCS() + "Brand Code" + Cn.GCS() + "Brand Name" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].SBRANDCD + "</td><td>" + MDT[j].SBRANDNM + "</td><td>" + MDT[j].MBRANDCD + "</td><td>" + MDT[j].BRANDNM + "</td><td>" + MDT[j].M_AUTONO + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0", "4"));
        }
        public ActionResult GetBrandDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.BRANDCD_help(val));
                }
                else
                {
                    string str = Master_Help.BRANDCD_help(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult CheckSubBrandCode(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_SUBBRAND where (c.SBRANDCD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }

        }
        public ActionResult AddDOCRow(SubBrandEntry VE)
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
        public ActionResult DeleteDOCRow(SubBrandEntry VE)
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
        public ActionResult SAVE(FormCollection FC, SubBrandEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_SUBBRAND MSUBBRAND = new M_SUBBRAND();
                        MSUBBRAND.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MSUBBRAND.EMD_NO = 0;
                            MSUBBRAND.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                        }
                        if (VE.DefaultAction == "E")
                        {
                            MSUBBRAND.DTAG = "E";
                            MSUBBRAND.M_AUTONO = VE.M_SUBBRAND.M_AUTONO;


                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO));

                            var MAXEMDNO = (from p in DB.M_SUBBRAND where p.M_AUTONO == VE.M_SUBBRAND.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MSUBBRAND.EMD_NO = 0;
                            }
                            else
                            {
                                MSUBBRAND.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }

                        }
                        MSUBBRAND.SBRANDCD = VE.M_SUBBRAND.SBRANDCD.ToUpper();
                        MSUBBRAND.SBRANDNM = VE.M_SUBBRAND.SBRANDNM;
                        MSUBBRAND.MBRANDCD = VE.M_SUBBRAND.MBRANDCD;
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SUBBRAND", MSUBBRAND.M_AUTONO.Value, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());

                        if (VE.DefaultAction == "A")
                        {
                            DB.M_SUBBRAND.Add(MSUBBRAND);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MSUBBRAND).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_SUBBRAND", VE.UploadDOC, MSUBBRAND.M_AUTONO.Value, MSUBBRAND.EMD_NO.Value);
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

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SUBBRAND", VE.M_CNTRL_HDR.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_SUBBRAND.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_SUBBRAND.RemoveRange(DB.M_SUBBRAND.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SUBBRAND.M_AUTONO));
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
                    Cn.SaveException(ex, "");
                    return Content(ex.Message);
                }
            }

        }

    }
}