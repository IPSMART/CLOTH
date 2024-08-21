using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class M_disc_rateController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        M_DISCRTHDR sl;
        M_DISCRT sMDCRT;
        M_CNTRL_HDR sll;
        ImprovarDB DBF;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_disc_rate
        public ActionResult M_disc_rate(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Discount Rate Master";
                    DiscountRateMasterEntry VE = new DiscountRateMasterEntry();
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DBIMP = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DBIMP.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                  
                    //=================For Type  ================//
                    List<DropDown_list> DDl = new List<DropDown_list>();
                    DropDown_list DDl1 = new DropDown_list();
                    DDl1.text = "Pcs";
                    DDl1.value = "P";
                    DDl.Add(DDl1);
                    DropDown_list DDl2 = new DropDown_list();
                    DDl2.text = "Box";
                    DDl2.value = "B";
                    DDl.Add(DDl2);
                    DropDown_list DDl3 = new DropDown_list();
                    DDl3.text = "Per";
                    DDl3.value = "R";
                    DDl.Add(DDl3);
                    VE.DropDown_list = DDl;
                    //=================End ================//


                    if (op.Length != 0)
                    {
                        string GCS = Cn.GCS();
                        VE.IndexKey = (from p in DB.M_DISCRTHDR orderby p.DISCRTCD select new IndexKey() { Navikey = p.DISCRTCD + GCS + p.EFFDT }).ToList();
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



                            if (sll.INACTIVE_TAG == "Y")
                            {
                                VE.Deactive = true;
                            }
                            else
                            {
                                VE.Deactive = false;
                            }
                            VE.M_DISCRTHDR = sl;
                            VE.M_DISCRT = sMDCRT;
                            VE.M_CNTRL_HDR = sll;
                        }

                        if (VE.DefaultAction == "A")
                        {
                            //dtl
                            List<MDISCRTDTL> EMPDPNLIST_OBJ = new List<MDISCRTDTL>();
                            MDISCRTDTL EMPDPNLIST_OBJ1 = new MDISCRTDTL();
                            EMPDPNLIST_OBJ1.SLNO = 1;
                            EMPDPNLIST_OBJ.Add(EMPDPNLIST_OBJ1);
                            VE.MDISCRTDTL = EMPDPNLIST_OBJ;

                            MDISCRTDTL EMPDPNLIST_OBJ2 = new MDISCRTDTL();
                            var EMPDPNLIST_OBJmax = VE.MDISCRTDTL.Max(a => Convert.ToInt32(a.SLNO));
                            int EMPDPNLIST_OBJSLNO = Convert.ToInt32(EMPDPNLIST_OBJmax) + 1;
                            EMPDPNLIST_OBJ2.SLNO = Convert.ToByte(EMPDPNLIST_OBJSLNO);
                            EMPDPNLIST_OBJ.Add(EMPDPNLIST_OBJ2);
                            VE.MDISCRTDTL = EMPDPNLIST_OBJ;

                            //document
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
                DiscountRateMasterEntry VE = new DiscountRateMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public DiscountRateMasterEntry Navigation(DiscountRateMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_DISCRTHDR();
            sMDCRT = new M_DISCRT();
            sll = new M_CNTRL_HDR();
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
                DateTime EDT = Convert.ToDateTime(aa[1]);

                sl = DB.M_DISCRTHDR.Find(aa[0].Trim(), EDT);
                sMDCRT = DBF.M_DISCRT.Find(sl.DISCRTCD);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Deactive = true;
                }
                else
                {
                    VE.Deactive = false;
                }
                VE.UploadDOC = Cn.GetUploadImage(CommVar.CurSchema(UNQSNO).ToString(), sl.M_AUTONO);
                VE.MDISCRTDTL = (from i in DB.M_DISCRTDTL
                                 join j in DB.M_SCMITMGRP_HDR on i.SCMITMGRPCD equals (j.SCMITMGRPCD)
                                 where (i.DISCRTCD == sl.DISCRTCD && i.EFFDT == sl.EFFDT)
                                 select new MDISCRTDTL()
                                 {
                                     SCMITMGRPCD = i.SCMITMGRPCD,
                                     SCMITMGRPNM = j.SCMITMGRPNM,
                                     DISCPER = i.DISCPER,
                                     DISCRATE = i.DISCRATE
                                 }).OrderBy(s => s.SCMITMGRPCD).ToList();              
                for (int i = 0; i <= VE.MDISCRTDTL.Count - 1; i++)
                {
                    VE.MDISCRTDTL[i].SLNO = Convert.ToByte(i + 1);
                }
            }
            else
            {

            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var MDT = (from j in DB.M_DISCRTHDR
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       where (j.M_AUTONO == p.M_AUTONO)
                       orderby j.M_AUTONO
                       select new { j.DISCRTCD, j.EFFDT }).ToList().Select(x => new 
                       {
                           DISCRTCD = x.DISCRTCD,
                           EFFDT = x.EFFDT.ToString().Replace('-', '/').Substring(0, 10)
                       }).Distinct().OrderBy(s => s.DISCRTCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Discount Rate Code" + Cn.GCS() + "Effective date";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].DISCRTCD + "</td><td>" + MDT[j].EFFDT + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0" + Cn.GCS() + "1"));
        }
        public ActionResult checkDISCRTCD_EFFDaTe(string val1, string val2)
        {
            DateTime dtval;
            //  string VALUE = val.ToUpper();
            if (val2 == "")
            {
                val2 = DateTime.Now.ToString();
                dtval = Convert.ToDateTime(val2);
            }
            else {
                dtval = Convert.ToDateTime(val2);
            }

            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_DISCRTHDR where (c.DISCRTCD == val1 && c.EFFDT == dtval) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }

        }
        public ActionResult GetDiscountCode()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

            return PartialView("_Help2", Master_Help.DISCRTCD_help(DB));
        }
        public ActionResult DiscountCode(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var query = (from c in DB.M_DISCRT where (c.DISCRTCD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.DISCRTCD + Cn.GCS() + i.DISCRTNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult GetDiscountItmGrpCode()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            return PartialView("_Help2", Master_Help.SCMITMGRPCD_help(DB));
        }
        public ActionResult DiscountItmGrpCode(string val)
        {
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_SCMITMGRP_HDR
                         join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                         join k in DB.M_CNTRL_LOCA on c.M_AUTONO equals k.M_AUTONO into g
                         from k in g.DefaultIfEmpty()
                         where (c.SCMITMGRPCD == val && i.INACTIVE_TAG == "N" && (k.COMPCD == COM_CD || k.COMPCD == null) && (k.LOCCD == LOC_CD || k.LOCCD == null)) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.SCMITMGRPCD + Cn.GCS() + i.SCMITMGRPNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult AddrowDiscntRate(DiscountRateMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());


            List<MDISCRTDTL> MTMQTY = new List<MDISCRTDTL>();
            if (VE.MDISCRTDTL == null)
            {
                List<MDISCRTDTL> MDISL = new List<MDISCRTDTL>();
                MDISCRTDTL MDISL1 = new MDISCRTDTL();
                MDISL1.SLNO = 1;
                MDISL.Add(MDISL1);
                VE.MDISCRTDTL = MDISL;
            }
            else {
                for (int i = 0; i <= VE.MDISCRTDTL.Count - 1; i++)
                {
                    MDISCRTDTL MIQ = new MDISCRTDTL();
                    MIQ = VE.MDISCRTDTL[i];
                    MTMQTY.Add(MIQ);
                }
                MDISCRTDTL MIL = new MDISCRTDTL();
                var max = VE.MDISCRTDTL.Max(a => Convert.ToInt32(a.SLNO));
                int SLNO = Convert.ToInt32(max) + 1;
                MIL.SLNO = Convert.ToByte(SLNO);
                MTMQTY.Add(MIL);
                VE.MDISCRTDTL = MTMQTY;
            }
            VE.DefaultView = true;
            return PartialView("_M_disc_rate_MAIN", VE);
        }
        public ActionResult DeleterowDiscntRate(DiscountRateMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            List<MDISCRTDTL> MTMQTY = new List<MDISCRTDTL>();
            int count = 0;
            for (int i = 0; i <= VE.MDISCRTDTL.Count - 1; i++)
            {
                if (VE.MDISCRTDTL[i].Checked == false || (VE.MDISCRTDTL[i].Checked == false))
                {
                    count += 1;
                    MDISCRTDTL item = new MDISCRTDTL();
                    item = VE.MDISCRTDTL[i];
                    item.SLNO = Convert.ToByte(count);
                    MTMQTY.Add(item);
                }
            }
            VE.MDISCRTDTL = MTMQTY;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_disc_rate_MAIN", VE);
        }
        public ActionResult AddDOCRow(DiscountRateMasterEntry VE)
        {
            string SCHEMA = Cn.Getschema;
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
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
        public ActionResult DeleteDOCRow(DiscountRateMasterEntry VE)
        {
            string SCHEMA = Cn.Getschema;
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
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
        public ActionResult SAVE(FormCollection FC, DiscountRateMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_DISCRTHDR MDISHD = new M_DISCRTHDR();
                        MDISHD.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MDISHD.EMD_NO = 0;
                            MDISHD.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                        }
                        else
                        {
                            MDISHD.M_AUTONO = VE.M_DISCRTHDR.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MDISHD.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MDISHD.EMD_NO = 0;
                            }
                            else
                            {
                                MDISHD.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                            MDISHD.DTAG = "E";
                        }

                        MDISHD.DISCRTCD = VE.M_DISCRTHDR.DISCRTCD.ToUpper();
                        MDISHD.EFFDT = VE.M_DISCRTHDR.EFFDT;
                        MDISHD.REMARKS = VE.M_DISCRTHDR.REMARKS;
                        MDISHD.DISCCALCTYPE = FC["DISCCALCTYPE"].ToString();

                        //allowance and deduction grid saving
                        if (VE.DefaultAction == "E")
                        {
                            DB.M_DISCRTDTL.Where(x => x.DISCRTCD == MDISHD.DISCRTCD && x.EFFDT == VE.M_DISCRTHDR.EFFDT).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_DISCRTDTL.RemoveRange(DB.M_DISCRTDTL.Where(x => x.DISCRTCD == MDISHD.DISCRTCD && x.EFFDT == VE.M_DISCRTHDR.EFFDT));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO));
                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO));                       

                        }
                       


                        //Control header 
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_DISCRTHDR", MDISHD.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(),VE.Audit_REM);
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_DISCRTHDR.Add(MDISHD);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MDISHD).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        DB.SaveChanges();

                        //allowance grid saving
                        if (VE.MDISCRTDTL != null)
                        {
                            for (int i = 0; i <= VE.MDISCRTDTL.Count - 1; i++)
                            {
                                if (VE.MDISCRTDTL[i].SLNO != 0 && VE.MDISCRTDTL[i].SCMITMGRPCD != null && VE.MDISCRTDTL[i].DISCRATE != 0 && VE.MDISCRTDTL[i].DISCPER != 0)
                                {
                                    M_DISCRTDTL MdsL = new M_DISCRTDTL();
                                    MdsL.EMD_NO = MDISHD.EMD_NO;
                                    MdsL.CLCD = MDISHD.CLCD;
                                    MdsL.DTAG = MDISHD.DTAG;
                                    MdsL.DISCRTCD = VE.M_DISCRTHDR.DISCRTCD;
                                    MdsL.EFFDT = VE.M_DISCRTHDR.EFFDT;
                                    MdsL.SCMITMGRPCD = VE.MDISCRTDTL[i].SCMITMGRPCD;
                                    MdsL.DISCPER = VE.MDISCRTDTL[i].DISCPER;
                                    MdsL.DISCRATE = VE.MDISCRTDTL[i].DISCRATE;
                                    DB.M_DISCRTDTL.Add(MdsL);
                                }
                            }
                        }
                        //end allawance saving


                        //document
                        if (VE.UploadDOC != null)// add
                        {
                            var img = Cn.SaveUploadImage("M_DISCRTHDR", VE.UploadDOC, MDISHD.M_AUTONO, MDISHD.EMD_NO.Value);
                            if (img.Item1.Count != 0)
                            {
                                DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                                DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                            }
                        }
                        //end document

                        DB.SaveChanges();
                        ModelState.Clear();
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

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_DISCRTHDR", VE.M_DISCRTHDR.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), VE.Audit_REM);
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_DISCRTHDR.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_DISCRTDTL.Where(x => x.DISCRTCD == VE.M_DISCRTHDR.DISCRTCD && x.EFFDT == VE.M_DISCRTHDR.EFFDT).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_DISCRTDTL.RemoveRange(DB.M_DISCRTDTL.Where(x => x.DISCRTCD == VE.M_DISCRTHDR.DISCRTCD && x.EFFDT == VE.M_DISCRTHDR.EFFDT));
                        DB.SaveChanges();
                        DB.M_DISCRTHDR.RemoveRange(DB.M_DISCRTHDR.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_DISCRTHDR.M_AUTONO));
                        DB.SaveChanges();


                        ModelState.Clear();
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
                    return Content(ex.Message + ex.InnerException);
                }


            }

        }

    }
}