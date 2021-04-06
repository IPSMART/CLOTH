using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class M_SubLeg_SDdtlController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        MasterHelpFa masterHelpFa = new MasterHelpFa();
        Salesfunc salesfunc = new Salesfunc();
        ImprovarDB DB, DBF, DBI;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        M_SUBLEG_SDDTL sl1; M_CNTRL_HDR sll; M_SUBLEG_COM sl; M_SUBLEG msub; M_PRCLST mprc; M_AREACD marea; M_DISCRT mDiscrt;
        // GET: M_SubLeg_SDdtl
        public ActionResult M_SubLeg_SDdtl(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Tax & Credit Limit Party & Company  Wise";
                    DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema); string comp = CommVar.Compcd(UNQSNO);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    SubLedgerSDdtlMasterEntry VE = new SubLedgerSDdtlMasterEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    VE.DocumentThrough = masterHelp.DocumentThrough();
                    VE.DropDown_list_DelvType = salesfunc.GetforDelvTypeSelection();

                    if (op.Length != 0)
                    {
                        string gcs = Cn.GCS();
                        VE.IndexKey = (from p in DB.M_SUBLEG_COM orderby p.SLCD select new IndexKey() { Navikey = p.SLCD + gcs + p.COMPCD }).Distinct().ToList();
                        if (op == "E" || op == "D" || op == "V")
                        {
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
                            if (sll.INACTIVE_TAG == "Y")
                            {
                                VE.Deactive = true;
                            }
                            else
                            {
                                VE.Deactive = false;
                            }
                            VE.M_SUBLEG_COM = sl;
                            VE.M_CNTRL_HDR = sll;
                            VE.M_SUBLEG = msub;
                            VE.M_PRCLST = mprc;
                            VE.M_AREACD = marea;
                            VE.M_DISCRT = mDiscrt;
                        }
                        if (VE.DefaultAction == "A")
                        {

                            List<MSUBLEGBRAND> SUBLEGBRAND_OBJ = new List<MSUBLEGBRAND>();
                            MSUBLEGBRAND SUBLEGBRAND_OBJ1 = new MSUBLEGBRAND();
                            SUBLEGBRAND_OBJ1.SLNO = 1;
                            SUBLEGBRAND_OBJ.Add(SUBLEGBRAND_OBJ1);
                            VE.MSUBLEGBRAND = SUBLEGBRAND_OBJ;

                            List<MSUBLEGSDDTL> MSUBLEGSDDTL_OBJ = new List<MSUBLEGSDDTL>();
                            MSUBLEGSDDTL MSUBLEGSDDTL_OBJ1 = new MSUBLEGSDDTL();
                            MSUBLEGSDDTL_OBJ1.SLNO = 1;
                            MSUBLEGSDDTL_OBJ.Add(MSUBLEGSDDTL_OBJ1);
                            VE.MSUBLEGSDDTL = MSUBLEGSDDTL_OBJ;

                            List<MSUBLEGSDDTL> MSUBLEGSDDTL = new List<MSUBLEGSDDTL>();
                            VE.MSUBLEGSDDTL = (from i in DBF.M_LOCA
                                               where i.COMPCD == comp
                                               select new MSUBLEGSDDTL()
                                               {
                                                   LOCCD = i.LOCCD,
                                                   LOCNM = i.LOCNM
                                               })
                            .OrderBy(s => s.LOCCD).Distinct().ToList();
                            int k = 0;
                            foreach (var v in VE.MSUBLEGSDDTL)
                            {
                                v.SLNO = Convert.ToInt16(k + 1);
                                k++;
                            }

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
                SubLedgerSDdtlMasterEntry VE = new SubLedgerSDdtlMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public SubLedgerSDdtlMasterEntry Navigation(SubLedgerSDdtlMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl1 = new M_SUBLEG_SDDTL(); sll = new M_CNTRL_HDR(); sl = new M_SUBLEG_COM();
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string LOCCD = CommVar.Loccd(UNQSNO);
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
                sl = DB.M_SUBLEG_COM.Find(aa[0].Trim(), aa[1].Trim());
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                msub = DBF.M_SUBLEG.Find(sl.SLCD);
                mprc = DBF.M_PRCLST.Find(sl.PRCCD);
                marea = DBF.M_AREACD.Find(sl.AREACD);
                mDiscrt = DBF.M_DISCRT.Find(sl.DISCRTCD);

                var Agent = DBF.M_SUBLEG.Find(sl.AGSLCD);
                if (Agent != null)
                {
                    VE.AGSLNM = Agent.SLNM;
                }
                var query = (from j in DB.M_SUBLEG_BRAND
                             join k in DB.M_BRAND on j.BRANDCD equals (k.BRANDCD) into brand
                             from k in brand.DefaultIfEmpty()
                             where (j.SLCD == sl.SLCD)
                             select new MSUBLEGBRAND()
                             {
                                 BRANDCD = j.BRANDCD,
                                 BRANDNM = k.BRANDNM,
                                 AGSLCD = j.AGSLCD
                             }).ToList();

                var m_subleg = from q in DBF.M_SUBLEG
                               select q;
                VE.MSUBLEGBRAND = (from a in query
                                   join b in m_subleg on a.AGSLCD equals b.SLCD
                                   select new MSUBLEGBRAND
                                   {
                                       BRANDCD = a.BRANDCD,
                                       BRANDNM = a.BRANDNM,
                                       AGSLCD = a.AGSLCD,
                                       SLNM = b.SLNM
                                   }).ToList();

                if (VE.MSUBLEGBRAND != null)
                {
                    for (int i = 0; i <= VE.MSUBLEGBRAND.Count - 1; i++)
                    {
                        VE.MSUBLEGBRAND[i].SLNO = Convert.ToByte(i + 1);
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

                VE.MSUBLEGSDDTL = (from j in DB.M_SUBLEG_SDDTL
                                   where (j.SLCD == sl.SLCD && j.COMPCD == sl.COMPCD)
                                   select new MSUBLEGSDDTL()
                                   {
                                       SLCD = j.SLCD,
                                       LOCCD = j.LOCCD,
                                       TAXGRPCD = j.TAXGRPCD,
                                       TRSLCD = j.TRSLCD,
                                       COURCD = j.COURCD,
                                       COMPCD = j.COMPCD
                                   }).ToList();

                foreach (var v in VE.MSUBLEGSDDTL)
                {
                    if (v.SLCD != null) v.SLNM = DBF.M_SUBLEG.Find(v.SLCD).SLNM;
                    if (v.COURCD != null) v.COURNM = DBF.M_SUBLEG.Find(v.COURCD).SLNM;
                    if (v.TRSLCD != null) v.TRSLNM = DBF.M_SUBLEG.Find(v.TRSLCD).SLNM;
                    if (v.TAXGRPCD != null) v.TAXGRPNM = DBF.M_TAXGRP.Find(v.TAXGRPCD).TAXGRPNM;
                    if (v.LOCCD != null) v.LOCNM = DBF.M_LOCA.Find(v.LOCCD, v.COMPCD).LOCNM;
                }


                if (VE.MSUBLEGSDDTL.Count == 0)
                {
                    List<MSUBLEGSDDTL> MSUBLEGSDDTL = new List<MSUBLEGSDDTL>();
                    VE.MSUBLEGSDDTL = (from i in DBF.M_LOCA
                                       select new MSUBLEGSDDTL()
                                       {
                                           LOCCD = i.LOCCD,
                                           LOCNM = i.LOCNM
                                       })
                    .OrderBy(s => s.LOCCD).ToList();
                }
                int l = 0;
                foreach (var v in VE.MSUBLEGSDDTL)
                {
                    v.SLNO = Convert.ToInt16(l + 1);
                    l++;
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
            else
            {
                List<MSUBLEGSDDTL> MSUBLEGSDDTL = new List<MSUBLEGSDDTL>();
                VE.MSUBLEGSDDTL = (from i in DBF.M_LOCA
                                   select new MSUBLEGSDDTL()
                                   {
                                       LOCCD = i.LOCCD,
                                       LOCNM = i.LOCNM
                                   })
                .OrderBy(s => s.LOCCD).ToList();
                int k = 0;
                foreach (var v in VE.MSUBLEGSDDTL)
                {
                    v.SLNO = Convert.ToInt16(k + 1);
                }


                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                UploadDOC UPL = new UploadDOC();
                UPL.DocumentType = doctP;
                UploadDOC1.Add(UPL);
                VE.UploadDOC = UploadDOC1;

            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var query = (from j in DB.M_SUBLEG_COM
                         join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                         where (p.M_AUTONO == j.M_AUTONO)
                         select new
                         {
                             SLCD = j.SLCD,
                             PRCCD = j.PRCCD,
                             COMPCD = j.COMPCD
                         }).Distinct().OrderBy(s => s.SLCD).ToList();

            var m_subleg = from q in DBF.M_SUBLEG
                           select q;
            var MDT = (from a in query
                       join b in m_subleg on a.SLCD equals b.SLCD
                       select new
                       {
                           SLCD = a.SLCD,
                           SLNM = b.SLNM,
                           PRCCD = a.PRCCD,
                           COMPCD = a.COMPCD
                       }).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Sub Ledger Code" + Cn.GCS() + "Sub Ledger Name" + Cn.GCS() + "Price list Code" + Cn.GCS() + "Company Code";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].SLCD + "</td><td>" + MDT[j].SLNM + "</td><td>" + MDT[j].PRCCD + "</td><td>" + MDT[j].COMPCD + "</td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelpFa.Generate_SearchPannel(hdr, SB.ToString(), "0" + Cn.GCS() + "3", "3"));

        }
        public ActionResult GetPriceDetails(string val)
        {
            try
            {
                var str = masterHelp.PRCCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        //public ActionResult GetPRCCDhelp(string val)
        //{
        //    DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        //    if (val == null)
        //    {
        //        return PartialView("_Help2", masterHelp.PRCCD_help(val));
        //    }
        //    else
        //    {
        //        var query = (from c in DBF.M_PRCLST where (c.PRCCD == val) select c);
        //        if (query.Any())
        //        {
        //            string str = "";
        //            foreach (var i in query)
        //            {
        //                str = i.PRCCD + Cn.GCS() + i.PRCNM;
        //            }
        //            return Content(str);
        //        }
        //        else
        //        {
        //            return Content("0");
        //        }
        //    }

        //}
        public ActionResult GetSLCDhelp(string val, string Code)
        {
            DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            if (val == null)
            {
                return PartialView("_Help2", masterHelp.SUBLEDGER(val));
            }
            else
            {
                var query = (
                    from c in DBF.M_SUBLEG
                        //join d in DBF.M_SUBLEG_LINK on c.SLCD equals (d.SLCD)
                        //where (c.SLCD == val && d.LINKCD == Code)
                    where (c.SLCD == val)
                    select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        if (Code == "")
                        {
                            string COMPCD = Convert.ToString(Session["CompanyCode"]);
                            var slcd = DB.M_SUBLEG_COM.Find(i.SLCD, COMPCD);
                            if (slcd != null)
                            {
                                return Content("1");
                            }
                        }
                        str = i.SLCD + Cn.GCS() + i.SLNM;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }

        }
        public ActionResult GetTAXGRPCDhelp(string val)
        {
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            if (val == null)
            {
                return PartialView("_Help2", masterHelp.TAXGRPCD_help(DBF));
            }
            else
            {
                var query = (from c in DBF.M_TAXGRP where (c.TAXGRPCD == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.TAXGRPCD + Cn.GCS() + i.TAXGRPNM;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }

        }
        public ActionResult GetAreaDetails()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            return PartialView("_Help2", masterHelp.AREACD_help(DB));
        }
        public ActionResult AREACode(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var query = (from c in DB.M_AREACD where (c.AREACD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.AREACD + Cn.GCS() + i.AREANM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult GetDiscRateDetails()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            return PartialView("_Help2", masterHelp.DISCRTCD_help(DB));
        }
        public ActionResult DISCRATECode(string val)
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
        public ActionResult GetBrandDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.BRANDCD_help(val));
                }
                else
                {
                    string str = masterHelp.BRANDCD_help(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

        public ActionResult Addrow(SubLedgerSDdtlMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());


            List<MSUBLEGBRAND> SUBLEGBRAND = new List<MSUBLEGBRAND>();
            if (VE.MSUBLEGBRAND == null)
            {
                List<MSUBLEGBRAND> SUBLEGBRAND1 = new List<MSUBLEGBRAND>();
                MSUBLEGBRAND SCMIT = new MSUBLEGBRAND();
                SCMIT.SLNO = 1;
                SUBLEGBRAND1.Add(SCMIT);
                VE.MSUBLEGBRAND = SUBLEGBRAND1;
            }
            else {
                for (int i = 0; i <= VE.MSUBLEGBRAND.Count - 1; i++)
                {
                    MSUBLEGBRAND SIDTL = new MSUBLEGBRAND();
                    SIDTL = VE.MSUBLEGBRAND[i];
                    SUBLEGBRAND.Add(SIDTL);
                }
                MSUBLEGBRAND SIL = new MSUBLEGBRAND();
                var max = VE.MSUBLEGBRAND.Max(a => Convert.ToInt32(a.SLNO));
                int SLNO = Convert.ToInt32(max) + 1;
                SIL.SLNO = Convert.ToByte(SLNO);
                SUBLEGBRAND.Add(SIL);
                VE.MSUBLEGBRAND = SUBLEGBRAND;
            }
            VE.DefaultView = true;
            return PartialView("_M_SubLeg_SDdtl_Brand", VE);
        }
        public ActionResult Deleterow(SubLedgerSDdtlMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            List<MSUBLEGBRAND> SUBLEGBRAND = new List<MSUBLEGBRAND>();
            int count = 0;
            for (int i = 0; i <= VE.MSUBLEGBRAND.Count - 1; i++)
            {
                if (VE.MSUBLEGBRAND[i].Checked == false || (VE.MSUBLEGBRAND[i].Checked == false))
                {
                    count += 1;
                    MSUBLEGBRAND item = new MSUBLEGBRAND();
                    item = VE.MSUBLEGBRAND[i];
                    item.SLNO = Convert.ToByte(count);
                    SUBLEGBRAND.Add(item);
                }
            }
            VE.MSUBLEGBRAND = SUBLEGBRAND;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_SubLeg_SDdtl_Brand", VE);
        }
        public ActionResult AddDOCRow(SubLedgerSDdtlMasterEntry VE)
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
        public ActionResult DeleteDOCRow(SubLedgerSDdtlMasterEntry VE)
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
        public ActionResult SAVE(FormCollection FC, SubLedgerSDdtlMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_SUBLEG_COM MSUBLEGCOM = new M_SUBLEG_COM();
                        MSUBLEGCOM.CLCD = CommVar.ClientCode(UNQSNO);
                        MSUBLEGCOM.COMPCD = CommVar.Compcd(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MSUBLEGCOM.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                            MSUBLEGCOM.EMD_NO = 0;
                        }
                        else
                        {
                            MSUBLEGCOM.M_AUTONO = VE.M_SUBLEG_COM.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MSUBLEGCOM.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MSUBLEGCOM.EMD_NO = 0;
                            }
                            else
                            {
                                MSUBLEGCOM.EMD_NO = Convert.ToInt16(MAXEMDNO + 1);
                            }
                        }
                        MSUBLEGCOM.SLCD = VE.M_SUBLEG_COM.SLCD;
                        MSUBLEGCOM.AGSLCD = VE.M_SUBLEG_COM.AGSLCD;
                        MSUBLEGCOM.CRLIMIT = VE.M_SUBLEG_COM.CRLIMIT;
                        MSUBLEGCOM.CRDAYS = VE.M_SUBLEG_COM.CRDAYS;
                        MSUBLEGCOM.PRCCD = VE.M_SUBLEG_COM.PRCCD;
                        MSUBLEGCOM.AREACD = VE.M_SUBLEG_COM.AREACD;
                        MSUBLEGCOM.DISCRTCD = VE.M_SUBLEG_COM.DISCRTCD;
                        MSUBLEGCOM.COD = VE.M_SUBLEG_COM.COD;
                        MSUBLEGCOM.DOCTH = VE.M_SUBLEG_COM.DOCTH;
                        MSUBLEGCOM.SAPCODE = VE.M_SUBLEG_COM.SAPCODE;
                        //grid saving
                        if (VE.DefaultAction == "E")
                        {
                            MSUBLEGCOM.DTAG = "E";
                            DB.M_SUBLEG_BRAND.Where(x => x.SLCD == MSUBLEGCOM.SLCD && x.COMPCD == MSUBLEGCOM.COMPCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_SUBLEG_BRAND.RemoveRange(DB.M_SUBLEG_BRAND.Where(x => x.SLCD == MSUBLEGCOM.SLCD));

                            DB.M_SUBLEG_SDDTL.Where(x => x.SLCD == MSUBLEGCOM.SLCD && x.COMPCD == MSUBLEGCOM.COMPCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_SUBLEG_SDDTL.RemoveRange(DB.M_SUBLEG_SDDTL.Where(x => x.SLCD == MSUBLEGCOM.SLCD));

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO));
                        }
                        //Control header
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_SUBLEG_COM", MSUBLEGCOM.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.M_SUBLEG_COM.Add(MSUBLEGCOM);
                            DB.SaveChanges();
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MSUBLEGCOM).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        //brand grid saving
                        if (VE.MSUBLEGBRAND != null)
                        {
                            for (int i = 0; i <= VE.MSUBLEGBRAND.Count - 1; i++)
                            {
                                if (VE.MSUBLEGBRAND[i].SLNO != 0 && VE.MSUBLEGBRAND[i].BRANDCD != null && VE.MSUBLEGBRAND[i].AGSLCD != null)
                                {
                                    M_SUBLEG_BRAND MSUBLEGBRAND = new M_SUBLEG_BRAND();
                                    MSUBLEGBRAND.EMD_NO = MSUBLEGCOM.EMD_NO;
                                    MSUBLEGBRAND.CLCD = MSUBLEGCOM.CLCD;
                                    MSUBLEGBRAND.SLCD = VE.M_SUBLEG_COM.SLCD;
                                    MSUBLEGBRAND.COMPCD = MSUBLEGCOM.COMPCD;
                                    MSUBLEGBRAND.BRANDCD = VE.MSUBLEGBRAND[i].BRANDCD;
                                    MSUBLEGBRAND.AGSLCD = VE.MSUBLEGBRAND[i].AGSLCD;
                                    DB.M_SUBLEG_BRAND.Add(MSUBLEGBRAND);
                                }
                            }
                        }
                        //brand end saving

                        bool flag1 = false;
                        //location grid saving
                        if (VE.MSUBLEGSDDTL != null)
                        {
                            for (int i = 0; i <= VE.MSUBLEGSDDTL.Count - 1; i++)
                            {
                                if (VE.MSUBLEGSDDTL[i].SLNO != 0 && VE.MSUBLEGSDDTL[i].LOCCD != null && (VE.MSUBLEGSDDTL[i].TAXGRPCD != null || VE.MSUBLEGSDDTL[i].TRSLCD != null || VE.MSUBLEGSDDTL[i].COURCD != null))
                                {
                                    M_SUBLEG_SDDTL MSUBLEGSDDTL = new M_SUBLEG_SDDTL();
                                    MSUBLEGSDDTL.EMD_NO = MSUBLEGCOM.EMD_NO;
                                    MSUBLEGSDDTL.CLCD = MSUBLEGCOM.CLCD;
                                    MSUBLEGSDDTL.SLCD = VE.M_SUBLEG_COM.SLCD;
                                    MSUBLEGSDDTL.COMPCD = MSUBLEGCOM.COMPCD;
                                    MSUBLEGSDDTL.LOCCD = VE.MSUBLEGSDDTL[i].LOCCD;
                                    MSUBLEGSDDTL.TAXGRPCD = VE.MSUBLEGSDDTL[i].TAXGRPCD;
                                    MSUBLEGSDDTL.TRSLCD = VE.MSUBLEGSDDTL[i].TRSLCD;
                                    MSUBLEGSDDTL.COURCD = VE.MSUBLEGSDDTL[i].COURCD;

                                    DB.M_SUBLEG_SDDTL.Add(MSUBLEGSDDTL);
                                    flag1 = true;
                                }
                            }
                        }
                        if (!flag1)
                        {
                            transaction.Rollback();
                            return Content("Please fill Location Tax Code Master Details");
                        }
                        //location end saving
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_SUBLEG_COM", VE.UploadDOC, MSUBLEGCOM.M_AUTONO, MSUBLEGCOM.EMD_NO.Value);
                            DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                            DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                        }

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
                        var COMPCD = CommVar.Compcd(UNQSNO);
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_SUBLEG_COM", VE.M_SUBLEG_COM.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_SUBLEG_COM.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SUBLEG_BRAND.Where(x => x.SLCD == VE.M_SUBLEG_COM.SLCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SUBLEG_SDDTL.Where(x => x.SLCD == VE.M_SUBLEG_COM.SLCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO));
                        DB.SaveChanges();


                        DB.M_SUBLEG_BRAND.RemoveRange(DB.M_SUBLEG_BRAND.Where(x => x.SLCD == VE.M_SUBLEG_COM.SLCD && x.COMPCD == COMPCD));
                        DB.SaveChanges();
                        DB.M_SUBLEG_SDDTL.RemoveRange(DB.M_SUBLEG_SDDTL.Where(x => x.SLCD == VE.M_SUBLEG_COM.SLCD && x.COMPCD == COMPCD));
                        DB.SaveChanges();
                        DB.M_SUBLEG_COM.RemoveRange(DB.M_SUBLEG_COM.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SUBLEG_COM.M_AUTONO));
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
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }


            }

        }
        [HttpPost]
        public ActionResult M_SubLeg_SDdtl(FormCollection FC, SubLedgerSDdtlMasterEntry VE)
        {
            try
            {
                string COMPCD = Convert.ToString(Session["CompanyCode"]);
                string LOCCD = Convert.ToString(Session["CompanyLocationCode"]);
                string DBF = CommVar.FinSchema(UNQSNO);
                string DB = CommVar.CurSchema(UNQSNO).ToString();

                DataTable tbl;

                string SQLQuery = "";
                SQLQuery = SQLQuery + "select a.slcd, b.slnm, b.add1||' '||b.add2||' '||b.add3||' '||b.add4||' '||b.add5||' '||b.add6 add1, ";
                SQLQuery = SQLQuery + "b.partygrp, b.statecd, b.gstno, e.taxgrpcd, a.crdays, a.crlimit, c.taxgrpnm, e.trslcd, d.slnm trslnm, a.m_autono ";
                SQLQuery = SQLQuery + "from " + DB + ".m_subleg_com a, " + DBF + ".m_subleg b, " + DBF + ".m_taxgrp c, " + DBF + ".m_subleg d, " + DB + ".m_subleg_sddtl e ";
                SQLQuery = SQLQuery + "where a.compcd='" + COMPCD + "' and a.compcd=e.compcd(+) and a.slcd=b.slcd(+) and ";
                SQLQuery = SQLQuery + "e.taxgrpcd=c.taxgrpcd(+) and e.trslcd=d.slcd(+) ";

                tbl = masterHelp.SQLquery(SQLQuery);

                if (tbl.Rows.Count != 0)
                {
                    DataTable IR = new DataTable("mstrep");
                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();

                    HC.RepStart(IR, 2);
                    HC.GetPrintHeader(IR, "SLCD", "string", "c,8", "Customer Code");
                    HC.GetPrintHeader(IR, "SLNM", "string", "c,28", "Customer Name");
                    HC.GetPrintHeader(IR, "ADD1", "string", "c,40", "Address");
                    HC.GetPrintHeader(IR, "STATECD", "string", "c,6", "State Code");
                    HC.GetPrintHeader(IR, "GSTNO", "string", "c,16", "GST NO.");
                    HC.GetPrintHeader(IR, "TAXGRPCD", "string", "c,6", "Tax Group Code");
                    HC.GetPrintHeader(IR, "TAXGRPNM", "string", "c,8", "Tax Group Name");
                    HC.GetPrintHeader(IR, "CRDAYS", "string", "c,6", "Credit Days");
                    HC.GetPrintHeader(IR, "CRLIMIT", "double", "n,6", "Credit Limit");
                    HC.GetPrintHeader(IR, "TRSLCD", "string", "c,10", "Transporter Code");
                    HC.GetPrintHeader(IR, "TRSLNM", "string", "c,20", "Transporter Name");



                    Int32 i = 0; Int32 maxR = 0;
                    i = 0; maxR = tbl.Rows.Count - 1;
                    while (i <= maxR)
                    {
                        IR.Rows.Add("");
                        IR.Rows[i]["SLCD"] = tbl.Rows[i]["slcd"];
                        IR.Rows[i]["SLNM"] = tbl.Rows[i]["slnm"];
                        IR.Rows[i]["ADD1"] = tbl.Rows[i]["add1"];
                        IR.Rows[i]["STATECD"] = tbl.Rows[i]["statecd"];
                        IR.Rows[i]["GSTNO"] = tbl.Rows[i]["gstno"];
                        IR.Rows[i]["TAXGRPCD"] = tbl.Rows[i]["taxgrpcd"];
                        IR.Rows[i]["TAXGRPNM"] = tbl.Rows[i]["taxgrpnm"];
                        IR.Rows[i]["CRDAYS"] = tbl.Rows[i]["crdays"];
                        IR.Rows[i]["CRLIMIT"] = tbl.Rows[i]["crlimit"];
                        IR.Rows[i]["TRSLCD"] = tbl.Rows[i]["trslcd"];
                        IR.Rows[i]["TRSLNM"] = tbl.Rows[i]["trslnm"];
                        i = i + 1;
                    }

                    string pghdr1 = "";
                    pghdr1 = "Tax & Credit Limit Details On " + DateTime.Now.ToShortDateString();
                    PV = HC.ShowReport(IR, "M_SubLeg_SDdtl", pghdr1, "", true, true, "P", false);

                    TempData["M_SubLeg_SDdtl"] = PV;
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = "M_SubLeg_SDdtl" });
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return View();
        }
    }
}
