using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Improvar.Controllers
{
    public class T_SALEController : Controller
    {
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl(); DropDownHelp dropDownHelp = new DropDownHelp();
        T_TXN TXN; T_TXNTRANS TXNTRN; T_TXNOTH TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR; T_TXN_LINKNO TTXNLINKNO; T_TXNEINV TTXNEINV; T_TDSTXN TTDS; T_TXNMEMO TXNMEMO; T_VCH_GST VCHGST; T_STKTRNF TSTKTRNF; T_VCH_BL TVCHBL;
        SMS SMS = new SMS(); string sql = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        DataTable ITEMDT = new DataTable();
        // GET: T_SALE
        public ActionResult T_SALE(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no", string loadOrder = "N")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    TransactionSaleEntry VE = (parkID == "") ? new TransactionSaleEntry() : (Improvar.ViewModels.TransactionSaleEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    VE.Modify = VE.Edit;
                    ViewBag.formname = MenuDescription(VE.MENU_PARA).Rows[0]["FORMNAME"].ToString();
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    //VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE, VE.DefaultAction);

                    VE.BL_TYPE = dropDownHelp.DropDownBLTYPE();
                    VE.DropDown_list_StkType = masterHelp.STK_TYPE();
                    VE.DISC_TYPE = masterHelp.DISC_TYPE();
                    VE.DISC_TYPE1 = masterHelp.DISC_TYPE1();
                    VE.BARGEN_TYPE = masterHelp.BARGEN_TYPE();
                    VE.INVTYPE_list = masterHelp.INVTYPE_list();
                    VE.LOW_TDS_list = masterHelp.LOW_TDS_list();
                    VE.EXPCD_list = masterHelp.EXPCD_list((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? "P" : "S");
                    VE.Reverse_Charge = masterHelp.REV_CHRG();
                    VE.Database_Combo1 = (from n in DB.T_TXNOTH
                                          select new Database_Combo1() { FIELD_VALUE = n.SELBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.Database_Combo2 = (from n in DB.T_TXNOTH
                                          select new Database_Combo2() { FIELD_VALUE = n.DEALBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.Database_Combo3 = (from n in DB.T_TXNOTH
                                          select new Database_Combo3() { FIELD_VALUE = n.PACKBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.Database_Combo4 = (from n in DB.T_BATCHDTL
                                          where n.PCSTYPE != null
                                          select new Database_Combo4() { FIELD_VALUE = n.PCSTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.Database_Combo5 = (from n in DB.T_TXNOTH
                                          select new Database_Combo5() { FIELD_VALUE = n.TOPAY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.PCSTYPEVALUE = PCSTYPE_BIND(VE.Database_Combo4);

                    VE.DropDown_list_MTRLJOBCD = masterHelp.MTRLJOBCD_List();
                    if (VE.DropDown_list_MTRLJOBCD.Count() == 1)
                    {
                        VE.DropDown_list_MTRLJOBCD[0].Checked = true;
                    }
                    else
                    {
                        foreach (var v in VE.DropDown_list_MTRLJOBCD)
                        {
                            if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                            {
                                if (v.MTRLJOBCD == "FS" || v.MTRLJOBCD == "PL" || v.MTRLJOBCD == "DY")
                                {
                                    v.Checked = true;
                                }
                            }
                            else
                            {
                                if (v.MTRLJOBCD == "FS")
                                {
                                    v.Checked = true;
                                }
                            }
                        }
                    }
                    VE.VECHLTYPE = masterHelp.VECHLTYPE();
                    VE.TRANSMODE = masterHelp.TRANSMODE();
                    VE.DropDown_list1 = DISCTYPEINRATE();
                    List<DropDown_list2> RETURN_TYPE = new List<DropDown_list2> {
                    new DropDown_list2 { value = "N", text = "NO"},
                    new DropDown_list2 { value = "Y", text = "YES"},
                    };
                    VE.RETURN_TYPE = RETURN_TYPE;
                    //string[] autoEntryWork = ThirdParty.Split('~');// for zooming
                    //if (autoEntryWork[0] == "yes")
                    //{
                    //    autoEntryWork[2] = autoEntryWork[2].Replace("$$$$$$$$", "&");
                    //}
                    //if (autoEntryWork[0] == "yes")
                    //{
                    //    if (autoEntryWork[4] == "Y")
                    //    {
                    //        DocumentType dp = new DocumentType();
                    //        dp.text = autoEntryWork[2];
                    //        dp.value = autoEntryWork[1];
                    //        VE.DocumentType.Clear();
                    //        VE.DocumentType.Add(dp);
                    //        VE.Edit = "E";
                    //        VE.Delete = "D";
                    //    }
                    //}
                    if (op.Length != 0)
                    {
                        string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();

                        if (CommVar.ClientCode(UNQSNO) == "DIWH")
                        {
                            VE.IndexKey = (from p in DB.T_TXN
                                           join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                           orderby p.DOCCD, p.DOCNO, p.DOCDT
                                           where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YR1
                                           select new IndexKey() { Navikey = p.AUTONO }).ToList();
                        }
                        else
                        {
                            VE.IndexKey = (from p in DB.T_TXN
                                           join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                           orderby p.DOCDT, p.DOCNO
                                           where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YR1
                                           select new IndexKey() { Navikey = p.AUTONO }).ToList();
                        }

                        //if ((VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR") && op != "A" && searchValue != "")
                        //{
                        //    var chk_autono = VE.IndexKey.Where(a => a.Navikey == searchValue).ToList();
                        //    if (chk_autono.Count == 0)
                        //    {
                        //        searchValue = "";
                        //    }
                        //}

                        //var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                        //var MSYSCNFG = salesfunc.M_SYSCNFG();
                        //if (MSYSCNFG == null)
                        //{

                        //    VE.DefaultView = false;
                        //    VE.DefaultDay = 0;
                        //    ViewBag.ErrorMessage = "Data add in Configuaration Setup->Posting/Terms Setup";
                        //    return View(VE);
                        //}
                        //VE.M_SYSCNFG = MSYSCNFG;

                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
                        VE.SrcFlagCaption = "Bale No.";
                        //if (TempData["LoadFromExisting" + VE.MENU_PARA].retStr() != "")
                        //{
                        //    loadOrder = "N";
                        //    TempData.Remove("LoadFromExisting" + VE.MENU_PARA);
                        //}
                        if (op == "E" || op == "D" || op == "V" || loadOrder.retStr().Length > 1)
                        {
                            if (searchValue.Length != 0)
                            {
                                if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, 0, searchValue, loadOrder);
                            }
                            else
                            {
                                if (key == "F")
                                {
                                    VE.Index = 0;
                                    VE = Navigation(VE, DB, 0, searchValue, loadOrder);
                                }
                                else if (key == "" || key == "L")
                                {
                                    VE.Index = VE.IndexKey.Count - 1;
                                    VE = Navigation(VE, DB, VE.IndexKey.Count - 1, searchValue, loadOrder);
                                }
                                else if (key == "P")
                                {
                                    Nindex -= 1;
                                    if (Nindex < 0)
                                    {
                                        Nindex = 0;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DB, Nindex, searchValue, loadOrder);
                                }
                                else if (key == "N")
                                {
                                    Nindex += 1;
                                    if (Nindex > VE.IndexKey.Count - 1)
                                    {
                                        Nindex = VE.IndexKey.Count - 1;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DB, Nindex, searchValue, loadOrder);
                                }
                            }
                            VE.T_TXN = TXN;
                            VE.T_TXNTRANS = TXNTRN;
                            VE.T_TXNOTH = TXNOTH;
                            VE.T_VCH_BL = TVCHBL;
                            VE.T_CNTRL_HDR = TCH;
                            VE.T_CNTRL_HDR_REM = SLR;
                            VE.T_TXN_LINKNO = TTXNLINKNO;
                            VE.T_TXNEINV = TTXNEINV;
                            VE.T_TDSTXN = TTDS;
                            VE.T_VCH_GST = VCHGST;
                            VE.T_STKTRNF = TSTKTRNF;
                            if (VE.MENU_PARA == "SBPOS")
                            {
                                VE.T_TXNMEMO = TXNMEMO;
                            }
                            var MSYSCNFG = salesfunc.M_SYSCNFG(VE.T_TXN.DOCDT.retDateStr());
                            if (MSYSCNFG == null)
                            {

                                VE.DefaultView = false;
                                VE.DefaultDay = 0;
                                ViewBag.ErrorMessage = "Data add in Configuaration Setup->Posting/Terms Setup";
                                return View(VE);
                            }
                            VE.M_SYSCNFG = MSYSCNFG;
                            if (loadOrder.retStr().Length > 1)
                            {
                                if (VE.MENU_PARA != "SBDIR" && VE.MENU_PARA != "ISS")
                                {
                                    if (VE.MENU_PARA == "REC")
                                    {
                                        VE.T_STKTRNF.OTHAUTONO = VE.T_TXN.AUTONO;
                                        VE.T_TXN.SLCD = "";
                                        VE.Last_SLCD = "";
                                        VE.SLNM = "";
                                        VE.SLAREA = "";
                                        VE.GSTNO = "";
                                        VE.SLDISCDESC = "";
                                    }
                                    VE.T_TXN.AUTONO = "";
                                }
                                if (VE.T_TXN_LINKNO == null) VE.T_TXN_LINKNO = new T_TXN_LINKNO();
                                VE.LINKDOCNO = loadOrder.retStr();
                                VE.T_TXN_LINKNO.LINKAUTONO = searchValue.retStr();
                                VE.T_TXN.DOCDT = Cn.getCurrentDate(VE.mindate);
                            }
                            if (VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }
                        if (op.ToString() == "A" && loadOrder == "N")
                        {
                            if (parkID == "")
                            {
                                T_TXN TTXN = new T_TXN();
                                T_TXNOTH TTXNOTH = new T_TXNOTH();
                                if (VE.DocumentType.Count == 1)
                                {
                                    TTXN.DOCCD = VE.DocumentType[0].value;
                                }
                                Cn.getdocmaxmindate(TTXN.DOCCD, "", VE.DefaultAction, "", VE, VE.MENU_PARA == "OP" ? true : false, TTXN.AUTONO);
                                TTXN.DOCDT = Cn.getCurrentDate(VE.mindate);
                                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                                {
                                    TTXN.PREFDT = Cn.getCurrentDate(VE.mindate);
                                }
                                var MSYSCNFG = salesfunc.M_SYSCNFG(TTXN.DOCDT.retDateStr());
                                if (MSYSCNFG == null)
                                {

                                    VE.DefaultView = false;
                                    VE.DefaultDay = 0;
                                    ViewBag.ErrorMessage = "Data add in Configuaration Setup->Posting/Terms Setup";
                                    return View(VE);
                                }
                                VE.M_SYSCNFG = MSYSCNFG;
                                string doccd1 = VE.DocumentType.Count() == 0 ? "" : VE.DocumentType.FirstOrDefault().value;
                                var T_TXN = (from a in DB.T_TXN
                                             join b in DB.T_CNTRL_HDR on a.AUTONO equals b.AUTONO
                                             where a.DOCCD == doccd1 && b.LOCCD == LOC && b.COMPCD == COM
                                             orderby a.AUTONO descending
                                             select a).FirstOrDefault();
                                if (CommVar.ClientCode(UNQSNO) == "SNFP" && (VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS"))
                                {
                                    TTXN.GOCD = "SHOP";
                                }
                                else {
                                    TTXN.GOCD = TempData["LASTGOCD" + VE.MENU_PARA].retStr();
                                }
                                if (VE.MENU_PARA == "ISS")
                                {
                                    T_STKTRNF T_STKTRNF = new T_STKTRNF();
                                    T_STKTRNF.SCOMPCD = CommVar.Compcd(UNQSNO);
                                    VE.SCOMPNM = CommVar.CompName(UNQSNO);
                                    T_STKTRNF.SLOCCD = CommVar.Loccd(UNQSNO);
                                    VE.SLOCNM = CommVar.LocName(UNQSNO);
                                    VE.T_STKTRNF = T_STKTRNF;
                                }
                                TTXN.PARGLCD = TempData["LASTPARGLCD" + VE.MENU_PARA].retStr();
                                TTXNOTH.PAYTERMS = TempData["LASTPAYTERMS" + VE.MENU_PARA].retStr();
                                TTXNOTH.SELBY = TempData["LASTSELBY" + VE.MENU_PARA].retStr();
                                TTXNOTH.PACKBY = TempData["LASTPACKBY" + VE.MENU_PARA].retStr();
                                TTXNOTH.DEALBY = TempData["LASTDEALBY" + VE.MENU_PARA].retStr();
                                string ROUNDOFF = TempData["LASTROUNDOFF" + VE.MENU_PARA].retStr();
                                string MERGEINDTL = TempData["LASTMERGEINDTL" + VE.MENU_PARA].retStr();
                                string STKDRCR = TempData["LASTSTKDRCR" + VE.MENU_PARA].retStr();
                                TempData.Keep();
                                if (TTXN.GOCD.retStr() == "")
                                {
                                    if (VE.DocumentType.Count() > 0)
                                    {
                                        string doccd = VE.DocumentType.FirstOrDefault().value;
                                        TTXN.GOCD = DB.T_TXN.Where(a => a.DOCCD == doccd).OrderByDescending(a => a.AUTONO).Select(b => b.GOCD).FirstOrDefault();
                                    }
                                }
                                string gocd = TTXN.GOCD.retStr();

                                if (gocd != "")
                                {
                                    VE.GONM = DBF.M_GODOWN.Where(a => a.GOCD == gocd).Select(b => b.GONM).FirstOrDefault();
                                    if (VE.MENU_PARA == "ISS")
                                    {
                                        VE.SGONM = DBF.M_GODOWN.Where(a => a.GOCD == gocd).Select(b => b.GONM).FirstOrDefault();
                                    }
                                }

                                if (ROUNDOFF == "")
                                {
                                    if (VE.DocumentType.Count() > 0)
                                    {
                                        string doccd = VE.DocumentType.FirstOrDefault().value;
                                        ROUNDOFF = DB.T_TXN.Where(a => a.DOCCD == doccd).OrderByDescending(a => a.AUTONO).Select(b => b.ROYN).FirstOrDefault();
                                    }
                                }
                                VE.RoundOff = ROUNDOFF == "Y" ? true : false;
                                if (MERGEINDTL == "")
                                {
                                    if (VE.DocumentType.Count() > 0)
                                    {
                                        string doccd = VE.DocumentType.FirstOrDefault().value;
                                        MERGEINDTL = DB.T_TXN.Where(a => a.DOCCD == doccd).OrderByDescending(a => a.AUTONO).Select(b => b.MERGEINDTL).FirstOrDefault();
                                    }
                                    if (MERGEINDTL == null || MERGEINDTL.retStr() == "")
                                    {
                                        MERGEINDTL = VE.M_SYSCNFG.MERGEINDTL.retStr();
                                    }
                                }
                                VE.MERGEINDTL = MERGEINDTL.retStr() == "Y" ? true : false;

                                if (STKDRCR == "")
                                {
                                    if (VE.DocumentType.Count() > 0)
                                    {
                                        string doccd = VE.DocumentType.FirstOrDefault().value;
                                        var Getstkdrcr = (from i in DB.T_TXN join j in DB.T_TXNDTL on i.AUTONO equals j.AUTONO where i.DOCCD == doccd select new { STKDRCR = j.STKDRCR, AUTONO = i.AUTONO }).OrderByDescending(s => s.AUTONO).FirstOrDefault();
                                        if (Getstkdrcr != null)
                                        { STKDRCR = Getstkdrcr.STKDRCR; }
                                    }
                                }
                                if (CommVar.ModuleCode() == "SALESCLOTH")
                                {
                                    VE.STOCKHOLD = true;
                                }
                                else
                                {
                                    VE.STOCKHOLD = STKDRCR == "C" ? true : false;
                                }

                                if (T_TXN != null)
                                {
                                    if (TTXN.PARGLCD.retStr() == "")
                                    {
                                        TTXN.PARGLCD = T_TXN.PARGLCD;

                                    }
                                }
                                string PARGLCD = TTXN.PARGLCD.retStr();
                                if (PARGLCD != "")
                                {
                                    VE.PARGLNM = DBF.M_GENLEG.Where(a => a.GLCD == PARGLCD).Select(b => b.GLNM).FirstOrDefault();
                                }


                                if (VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR")
                                {
                                    string doccd = "";
                                    if (VE.DocumentType.Count() > 0)
                                    {
                                        doccd = VE.DocumentType.FirstOrDefault().value;
                                    }
                                    var M_JOBMST = (from a in DB.M_JOBMST
                                                    join b in DB.M_DOCTYPE on a.JOBCD equals b.FLAG1 into x
                                                    from b in x.DefaultIfEmpty()
                                                    where b.DOCCD == doccd
                                                    select new { a.JOBCD, a.JOBNM, a.HSNCODE, a.EXPGLCD }).FirstOrDefault();
                                    if (M_JOBMST != null)
                                    {
                                        TTXN.JOBCD = M_JOBMST.JOBCD;
                                        VE.JOBNM = M_JOBMST.JOBNM;
                                        VE.JOBHSNCODE = M_JOBMST.HSNCODE;
                                        VE.JOBEXPGLCD = M_JOBMST.EXPGLCD;
                                    }
                                }
                                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "PR")
                                {
                                    TTXN.BARGENTYPE = VE.M_SYSCNFG.COMMONUNIQBAR.retStr() == "E" ? "E" : "C";
                                }
                                if (VE.MENU_PARA == "SBPOS")
                                {
                                    T_TXNMEMO TTXNMEMO = new T_TXNMEMO();
                                    TTXN.SLCD = TempData["LASTSLCD" + VE.MENU_PARA].retStr();
                                    string bltype = TempData["LASTBLTYPE" + VE.MENU_PARA].retStr();
                                    if (TTXN.SLCD.retStr() == "")
                                    {
                                        if (VE.DocumentType.Count() > 0)
                                        {
                                            string doccd = VE.DocumentType.FirstOrDefault().value;
                                            TTXN.SLCD = DB.T_TXN.Where(a => a.DOCCD == doccd).OrderByDescending(a => a.AUTONO).Select(b => b.SLCD).FirstOrDefault();
                                            bltype = (from a in DB.T_TXN
                                                      join b in DB.T_TXNOTH on a.AUTONO equals b.AUTONO into x
                                                      from b in x.DefaultIfEmpty()
                                                      where a.DOCCD == doccd
                                                      orderby a.AUTONO descending
                                                      select b.BLTYPE
                                                     ).FirstOrDefault();
                                        }
                                    }
                                    string slcd = TTXN.SLCD.retStr();

                                    if (slcd != "")
                                    {
                                        var subleg = (from a in DBF.M_SUBLEG where a.SLCD == slcd select new { a.SLNM, a.SLAREA, a.DISTRICT, a.GSTNO, a.PSLCD, a.TCSAPPL, a.PANNO, a.PARTYCD, a.REGMOBILE, a.ADD1, a.ADD2, a.ADD3, a.ADD4, a.ADD5, a.ADD6, a.ADD7 }).FirstOrDefault();
                                        VE.SLNM = subleg.SLNM;
                                        VE.SLAREA = subleg.SLAREA == "" ? subleg.DISTRICT : subleg.SLAREA;
                                        VE.GSTNO = subleg.GSTNO;
                                        VE.PSLCD = subleg.PSLCD;
                                        VE.PARTYCD = subleg.PARTYCD;
                                        //VE.TCSAPPL = subleg.TCSAPPL;
                                        //if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") VE.TCSAPPL = "N";
                                        //VE.TCSAUTOCAL = VE.TCSAPPL.retStr() == "Y" ? true : false;
                                        string panno = subleg.PANNO;

                                        string DOCTAG = MenuDescription(VE.MENU_PARA).Rows[0]["DOCTAG"].ToString().retSqlformat();
                                        bltype = bltype.retStr() == "" ? "" : bltype.retSqlformat();
                                        var party_data = salesfunc.GetSlcdDetails(TTXN.SLCD.retStr(), TTXN.DOCDT.retStr().Remove(10), "", DOCTAG, bltype);
                                        if (party_data != null && party_data.Rows.Count > 0)
                                        {
                                            VE.TCSAPPL = party_data.Rows[0]["TCSAPPL"].retStr();
                                            if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") VE.TCSAPPL = "N";
                                            VE.TCSAUTOCAL = VE.TCSAPPL.retStr() == "Y" ? true : false;

                                            string scmdisctype = party_data.Rows[0]["scmdisctype"].retStr() == "P" ? "%" : party_data.Rows[0]["scmdisctype"].retStr() == "N" ? "Nos" : party_data.Rows[0]["scmdisctype"].retStr() == "Q" ? "Qnty" : party_data.Rows[0]["scmdisctype"].retStr() == "F" ? "Fixed" : "";
                                            VE.SLDISCDESC = (party_data.Rows[0]["listdiscper"].retStr() + " " + party_data.Rows[0]["scmdiscrate"].retStr() + " " + scmdisctype + " " + (party_data.Rows[0]["lastbldt"].retStr() == "" ? "" : party_data.Rows[0]["lastbldt"].retStr().Remove(10)));
                                            TTXNOTH.TAXGRPCD = party_data.Rows[0]["TAXGRPCD"].retStr();
                                            VE.Last_TAXGRPCD = TTXNOTH.TAXGRPCD;
                                            TTXNOTH.PRCCD = party_data.Rows[0]["PRCCD"].retStr();
                                            if (TTXNOTH.PRCCD.retStr() != "")
                                            {
                                                VE.PRCNM = DBF.M_PRCLST.Where(a => a.PRCCD == TTXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();
                                            }
                                            VE.Last_PRCCD = TTXNOTH.PRCCD;
                                        }
                                        TTXNMEMO.NM = VE.SLNM;
                                        TTXNMEMO.MOBILE = subleg.REGMOBILE.retStr();
                                        var addrs = subleg.ADD1.retStr() == "" ? "" : (subleg.ADD1 + " ");
                                        addrs += subleg.ADD2.retStr() == "" ? "" : (subleg.ADD2 + " ");
                                        addrs += subleg.ADD3.retStr() == "" ? "" : (subleg.ADD3 + " ");
                                        addrs += subleg.ADD4.retStr() == "" ? "" : (subleg.ADD4 + " ");
                                        addrs += subleg.ADD5.retStr() == "" ? "" : (subleg.ADD5 + " ");
                                        addrs += subleg.ADD6.retStr() == "" ? "" : (subleg.ADD6 + " ");
                                        addrs += subleg.ADD7.retStr() == "" ? "" : (subleg.ADD7 + " ");
                                        TTXNMEMO.ADDR = addrs.TrimEnd();
                                        TTXNMEMO.CITY = subleg.DISTRICT;
                                    }
                                    VE.T_TXNMEMO = TTXNMEMO;
                                }
                                VE.T_TXN = TTXN;
                                //VE.MERGEINDTL = VE.M_SYSCNFG.MERGEINDTL.retStr() == "Y" ? true : false;
                                //T_TXNOTH TXNOTH = new T_TXNOTH();
                                if ((VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS") && CommVar.ClientCode(UNQSNO) == "BNBH") TTXNOTH.PAYTERMS = "NETT CASH, NO LESS";
                                VE.T_TXNOTH = TTXNOTH;
                                if ((VE.MENU_PARA == "PR") && (CommVar.ClientCode(UNQSNO) == "SACH" || CommVar.ClientCode(UNQSNO) == "LALF"))
                                { VE.ReturnAdjustwithBill = true; }
                                //if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH")
                                //{
                                //    DataTable data = salesfunc.GetSyscnfgData(VE.T_TXN.DOCDT.retDateStr());
                                //    if (data != null && data.Rows.Count > 0)
                                //    {
                                //        VE.WPPER = data.Rows[0]["WPPER"].retDbl();
                                //        VE.RPPER = data.Rows[0]["RPPER"].retDbl();
                                //    }
                                //}
                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;

                                List<PACKING_SLIP> PACKING_SLIP = new List<PACKING_SLIP>();
                                for (int i = 0; i < 10; i++)
                                {
                                    PACKING_SLIP PACKINGSLIP = new PACKING_SLIP();
                                    PACKINGSLIP.SLNO = Convert.ToInt16(i + 1);
                                    PACKING_SLIP.Add(PACKINGSLIP);
                                }
                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (TransactionSaleEntry)Cn.CheckPark(VE, VE.MENU_DETAILS, LOC, COM, CommVar.CurSchema(UNQSNO), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        else
                        {
                            if (VE.T_TXNEINV != null && VE.T_TXNEINV.IRNNO.retStr() != "") { VE.Delete = "NOT ALLOW Irn generated."; VE.Edit = "NOT ALLOW Irn generated."; }
                        }
                        if (parkID == "" && loadOrder == "N")
                        {
                            FreightCharges(VE, VE.T_TXN.AUTONO);
                        }
                        if (CommVar.ModuleCode() == "SALESCLOTH")
                        {
                            VE.ReturnAdjustwithBill = true;
                        }
                        var mtrljobcd = (from a in DB.M_MTRLJOBMST
                                         join b in DB.M_CNTRL_HDR on a.M_AUTONO equals b.M_AUTONO
                                         where b.INACTIVE_TAG == "N"
                                         select new { a.MTRLJOBCD, a.MTRLJOBNM, a.MTBARCODE }).ToList();
                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                        {
                            if (mtrljobcd.Count() == 1)
                            {
                                VE.MTRLJOBCD = mtrljobcd[0].MTRLJOBCD;
                                VE.MTRLJOBNM = mtrljobcd[0].MTRLJOBNM;
                                VE.MTBARCODE = mtrljobcd[0].MTBARCODE;
                            }
                            else
                            {
                                VE.MTRLJOBCD = "FS";
                                VE.MTRLJOBNM = mtrljobcd.Where(a => a.MTRLJOBCD == "FS").Select(b => b.MTRLJOBNM).FirstOrDefault();
                                VE.MTBARCODE = mtrljobcd.Where(a => a.MTRLJOBCD == "FS").Select(b => b.MTBARCODE).FirstOrDefault();
                            }
                        }
                        VE.SHOWMTRLJOBCD = mtrljobcd.Count() > 1 ? "Y" : "N";
                        VE.SHOWBLTYPE = VE.BL_TYPE.Count > 0 ? "Y" : "N";
                        VE.SHOWSTKTYPE = VE.DropDown_list_StkType.Count > 1 ? "Y" : "N";
                        //if (CommVar.ClientCode(UNQSNO) == "SACH")
                        //{
                        //    VE.MERGEINDTL = false;
                        //}
                        //else
                        //{
                        //    VE.MERGEINDTL = true;
                        //}
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (TCH != null) if (TCH.DOCDT != null) docdt = TCH.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_TXN.DOCCD, docdt, VE.DefaultAction, VE.T_TXN.DOCNO, VE, VE.MENU_PARA == "OP" ? true : false, VE.T_TXN.AUTONO);
                    if ((op.ToString() == "A" && loadOrder == "N" && parkID == "") || ((op == "E" || op == "D" || op == "V") && loadOrder.retStr().Length > 1))
                    {
                        VE.T_TXN.DOCDT = Cn.getCurrentDate(VE.mindate);
                        VE.T_TXN.PREFDT = Cn.getCurrentDate(VE.mindate);
                    }
                    VE.Last_DOCDT = VE.T_TXN.DOCDT.retDateStr();
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.SaveException(ex, "");
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        private DataTable MenuDescription(string MENU_PARA)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MENUPARA", typeof(string));
            dt.Columns.Add("FORMNAME", typeof(string));
            dt.Columns.Add("GLCD", typeof(string));
            dt.Columns.Add("sds2", typeof(string));
            dt.Columns.Add("sds3", typeof(string));
            dt.Columns.Add("sds4", typeof(string));
            dt.Columns.Add("sds5", typeof(string));
            dt.Columns.Add("sds6", typeof(string));
            dt.Columns.Add("PARTYLINKCD", typeof(string));
            dt.Columns.Add("DOCTAG", typeof(string));
            dt.Rows.Add("SBPCK", "Packing Slip", "SALGLCD", "", "", "", "", "", "D", "SB");
            dt.Rows.Add("SB", "Sales Bill (Agst Packing Slip)", "SALGLCD", "", "", "", "", "", "D", "SB");
            dt.Rows.Add("SBDIR", "Sales Bill", "SALGLCD", "", "", "", "", "", "D", "SB");
            dt.Rows.Add("SR", "Sales Return (SRM)", "SALRETGLCD", "", "", "", "", "", "D", "SR");
            dt.Rows.Add("SBEXP", "Sales Bill (Export)", "SALGLCD", "", "", "", "", "", "D", "SB");
            dt.Rows.Add("PI", "Proforma Invoice", "SALGLCD", "", "", "", "", "", "", "PI");
            dt.Rows.Add("PB", "Purchase Bill", "PURGLCD", "", "", "", "", "", "C", "PB");
            dt.Rows.Add("PR", "Purchase Return (PRM)", "PURRETGLCD", "", "", "", "", "", "C", "PR");
            dt.Rows.Add("OP", "Opening Stock", "PURGLCD", "", "", "", "", "", "", "OP");
            dt.Rows.Add("SCN", "Sales C/N (w/o Qty)", "SALRETGLCD", "", "", "", "", "", "D", "SC");//sales ret=cn 
            dt.Rows.Add("SDN", "Sales D/N (w/o Qty)", "SALGLCD", "", "", "", "", "", "D", "SD");
            dt.Rows.Add("PCN", "Purchase C/N (w/o Qty)", "PURGLCD", "", "", "", "", "", "C", "PC");
            dt.Rows.Add("PDN", "Purchase D/N (w/o Qty)", "PURRETGLCD", "", "", "", "", "", "C", "PD");//purchase ret = dn 
            dt.Rows.Add("OTH", "Other Product Opening Stock", "PURGLCD", "", "", "", "", "", "", "OT");
            dt.Rows.Add("PJRC", "Receive from Party for Job", "PURGLCD", "", "", "", "", "", "", "JR");
            dt.Rows.Add("PJIS", "Issue to Party after Job", "PURGLCD", "", "", "", "", "", "", "JI");
            dt.Rows.Add("PJRT", "Return to Party w/o Job", "PURGLCD", "", "", "", "", "", "", "JU");
            dt.Rows.Add("PJBL", "Job Bill raised to Party", "SALGLCD", "", "", "", "", "", "D", "JB");
            dt.Rows.Add("PJBR", "Job Bill Return to Party", "SALGLCD", "", "", "", "", "", "D", "JQ");
            dt.Rows.Add("SBPOS", "Cash Sales", "SALGLCD", "", "", "", "", "", "D", "SB");
            dt.Rows.Add("ISS", "Stock Transfer Issue", "SALGLCD", "", "", "", "", "", "D", "TO");//sales
            dt.Rows.Add("REC", "Stock Transfer Receive", "PURGLCD", "", "", "", "", "", "C", "TI");//purchase

            var dr = dt.Select("MENUPARA='" + MENU_PARA + "'");
            if (dr != null && dr.Count() > 0) return dr.CopyToDataTable(); else return dt;
        }
        public TransactionSaleEntry Navigation(TransactionSaleEntry VE, ImprovarDB DB, int index, string searchValue, string loadOrder = "N")
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBC = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE);

            TXN = new T_TXN(); TXNTRN = new T_TXNTRANS(); TXNOTH = new T_TXNOTH(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM(); TTXNLINKNO = new T_TXN_LINKNO(); TTXNEINV = new T_TXNEINV(); TTDS = new T_TDSTXN(); TXNMEMO = new T_TXNMEMO(); VCHGST = new T_VCH_GST(); TSTKTRNF = new T_STKTRNF(); TVCHBL = new T_VCH_BL();

            if (VE.IndexKey.Count != 0 || loadOrder.retStr().Length > 1)
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
                TXN = DB.T_TXN.Find(aa[0].Trim());
                TCH = DB.T_CNTRL_HDR.Find(TXN.AUTONO);
                if (TXN.ROYN == "Y") VE.RoundOff = true;
                else VE.RoundOff = false;
                VE.MERGEINDTL = TXN.MERGEINDTL == "Y" ? true : false;
                TXNTRN = DB.T_TXNTRANS.Find(TXN.AUTONO);

                TSTKTRNF = DB.T_STKTRNF.Find(TXN.AUTONO);
                VE.Last_PREFNO = TXN.PREFNO;
                VE.Last_PREFDT = TXN.PREFDT.retDateStr();
                if (VE.MENU_PARA == "ISS" || VE.MENU_PARA == "REC")
                {
                    if (VE.MENU_PARA == "REC")
                    {
                        if (loadOrder != "N")
                        {
                            TXN.GOCD = TSTKTRNF.TOGOCD;
                        }
                        else
                        {
                            VE.OTHDOCNO = (from a in DB.T_CNTRL_HDR where a.AUTONO == TSTKTRNF.OTHAUTONO select a.DOCNO).FirstOrDefault();
                            VE.OTHDOCDT = (from a in DB.T_CNTRL_HDR where a.AUTONO == TSTKTRNF.OTHAUTONO select a.DOCDT).FirstOrDefault();
                        }
                    }
                    if (TSTKTRNF != null && TSTKTRNF.SCOMPCD != null) { VE.SCOMPNM = DBF.M_COMP.Find(TSTKTRNF.SCOMPCD).COMPNM; }
                    if (TSTKTRNF != null && TSTKTRNF.TOCOMPCD != null) { VE.TCOMPNM = DBF.M_COMP.Find(TSTKTRNF.TOCOMPCD).COMPNM; }
                    if (TSTKTRNF != null && TSTKTRNF.SLOCCD != null) { VE.SLOCNM = DBF.M_LOCA.Find(TSTKTRNF.SLOCCD, TSTKTRNF.SCOMPCD).LOCNM; }
                    if (TSTKTRNF != null && TSTKTRNF.TOLOCCD != null) VE.TLOCNM = DBF.M_LOCA.Find(TSTKTRNF.TOLOCCD, TSTKTRNF.TOCOMPCD).LOCNM;
                    if (TSTKTRNF != null && TSTKTRNF.SGOCD != null) VE.SGONM = DBF.M_GODOWN.Find(TSTKTRNF.SGOCD).GONM;
                    if (TSTKTRNF != null && TSTKTRNF.TOGOCD != null) VE.TGONM = DBF.M_GODOWN.Find(TSTKTRNF.TOGOCD).GONM;
                }
                if (TXNTRN == null)
                {
                    T_TXNTRANS tempTXNTRN = new T_TXNTRANS();
                    TXNTRN = tempTXNTRN;
                }
                TXNOTH = DB.T_TXNOTH.Find(TXN.AUTONO);
                TVCHBL = DBF.T_VCH_BL.Find(TXN.AUTONO, 1);
                if (TXNOTH == null)
                {
                    T_TXNOTH tempTXNOTH = new T_TXNOTH();
                    TXNOTH = tempTXNOTH;
                }
                VE.Last_TAXGRPCD = TXNOTH.TAXGRPCD;
                VE.Last_PRCCD = TXNOTH.PRCCD;
                TTXNEINV = DBF.T_TXNEINV.Find(TXN.AUTONO);
                TTDS = DBF.T_TDSTXN.Where(m => m.AUTONO == TXN.AUTONO).FirstOrDefault();
                VCHGST = (from x in DBF.T_VCH_GST where x.AUTONO == TXN.AUTONO select x).FirstOrDefault();
                if (VCHGST != null)
                {
                    VE.PORTNM = DBC.MS_PORTCD.Find(VCHGST.PORTCD)?.PORTNM;
                    VE.GSTSLNM = VCHGST.GSTSLNM;
                    VE.POS = VCHGST.POS;
                }
                if (VE.MENU_PARA == "SB" && loadOrder == "N")
                {
                    TTXNLINKNO = (from a in DB.T_TXN_LINKNO where a.AUTONO == TXN.AUTONO select a).FirstOrDefault();
                    if (TTXNLINKNO != null)
                    {
                        VE.LINKDOCNO = (from a in DB.T_CNTRL_HDR where a.AUTONO == TTXNLINKNO.LINKAUTONO select a).FirstOrDefault().DOCNO;
                    }
                }
                if (VE.MENU_PARA == "SB" && loadOrder != "N")
                {
                    VE.M_SLIP_NO = TCH.DOCONLYNO;
                }
                string panno = "";
                if (TXN.SLCD.retStr() != "")
                {
                    string slcd = TXN.SLCD;
                    VE.Last_SLCD = slcd;
                    var subleg = (from a in DBF.M_SUBLEG where a.SLCD == slcd select new { a.SLNM, a.SLAREA, a.DISTRICT, a.GSTNO, a.PSLCD, a.TCSAPPL, a.PANNO, a.PARTYCD }).FirstOrDefault();
                    VE.SLNM = subleg.SLNM;
                    VE.SLAREA = subleg.SLAREA == "" ? subleg.DISTRICT : subleg.SLAREA;
                    VE.GSTNO = subleg.GSTNO;
                    VE.PSLCD = subleg.PSLCD;
                    VE.PARTYCD = subleg.PARTYCD;
                    //VE.TCSAPPL = subleg.TCSAPPL;
                    //if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") VE.TCSAPPL = "N";
                    //VE.TCSAUTOCAL = VE.TCSAPPL.retStr() == "Y" ? true : false;
                    panno = subleg.PANNO;

                    string DOCTAG = MenuDescription(VE.MENU_PARA).Rows[0]["DOCTAG"].ToString().retSqlformat();
                    string bltype = TXNOTH.BLTYPE.retStr() == "" ? "" : TXNOTH.BLTYPE.retStr().retSqlformat();
                    var party_data = salesfunc.GetSlcdDetails(TXN.SLCD.retStr(), TXN.DOCDT.retStr().Remove(10), "", DOCTAG, bltype);
                    if (party_data != null && party_data.Rows.Count > 0)
                    {
                        string scmdisctype = party_data.Rows[0]["scmdisctype"].retStr() == "P" ? "%" : party_data.Rows[0]["scmdisctype"].retStr() == "N" ? "Nos" : party_data.Rows[0]["scmdisctype"].retStr() == "Q" ? "Qnty" : party_data.Rows[0]["scmdisctype"].retStr() == "F" ? "Fixed" : "";
                        VE.SLDISCDESC = (party_data.Rows[0]["listdiscper"].retStr() + " " + party_data.Rows[0]["scmdiscrate"].retStr() + " " + scmdisctype + " " + (party_data.Rows[0]["lastbldt"].retStr() == "" ? "" : party_data.Rows[0]["lastbldt"].retStr().Remove(10)));

                        VE.TCSAPPL = party_data.Rows[0]["TCSAPPL"].retStr();
                        if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") VE.TCSAPPL = "N";
                        VE.TCSAUTOCAL = VE.TCSAPPL.retStr() == "Y" ? true : false;
                    }
                }

                VE.CONSLNM = TXN.CONSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXN.CONSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.AGSLNM = TXNOTH.AGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.AGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.SAGSLNM = TXNOTH.SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.SAGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.GONM = TXN.GOCD.retStr() == "" ? "" : DBF.M_GODOWN.Where(a => a.GOCD == TXN.GOCD).Select(b => b.GONM).FirstOrDefault();
                VE.PRCNM = TXNOTH.PRCCD.retStr() == "" ? "" : DBF.M_PRCLST.Where(a => a.PRCCD == TXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();

                VE.TRANSLNM = TXNTRN.TRANSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.TRANSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.MUTSLNM = TXNOTH.MUTSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.MUTSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.PARGLNM = TXN.PARGLCD.retStr() == "" ? "" : DBF.M_GENLEG.Where(a => a.GLCD == TXN.PARGLCD).Select(b => b.GLNM).FirstOrDefault();
                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);

                //tcsdata
                string TDSCODE = TXN.TDSCODE.retStr() == "" ? "" : TXN.TDSCODE.retStr().retSqlformat();
                var tdsdt = getTDS(TXN.DOCDT.retStr().Remove(10), TXN.SLCD, TDSCODE);
                if (tdsdt != null && tdsdt.Rows.Count > 0)
                {
                    VE.TDSLIMIT = tdsdt.Rows[0]["TDSLIMIT"].retDbl();
                    VE.TDSCALCON = tdsdt.Rows[0]["TDSCALCON"].retStr();
                    VE.TDSROUNDCAL = tdsdt.Rows[0]["TDSROUNDCAL"].retStr();
                }
                VE.AMT = salesfunc.getSlcdTCSonCalc(panno.retStr(), TXN.DOCDT.retStr().Remove(10), VE.MENU_PARA, TXN.AUTONO).retDbl();
                VE.BuyerTotTurnover = VE.AMT;
                VE.AMT = VE.AMT.retDbl() > VE.TDSLIMIT.retDbl() ? VE.TDSLIMIT.retDbl() : VE.AMT.retDbl();
                if (TXN.TDSCODE.retStr() != "")
                {
                    VE.TDSNM = DBF.M_TDS_CNTRL.Where(e => e.TDSCODE == TXN.TDSCODE).FirstOrDefault()?.TDSNM;
                }
                if (TTDS != null && TTDS.TDSCODE.retStr() != "")
                {
                    VE.TDSNM1 = DBF.M_TDS_CNTRL.Where(e => e.TDSCODE == TTDS.TDSCODE).FirstOrDefault()?.TDSNM;
                }
                if (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "REC")
                {
                    VE.EXPGLCD = DBF.T_VCH_GST.Where(a => a.AUTONO == TXN.AUTONO).Select(b => b.EXPGLCD).FirstOrDefault();
                    VE.EXPGLNM = VE.EXPGLCD.retStr() == "" ? "" : DBF.M_GENLEG.Where(a => a.GLCD == VE.EXPGLCD).Select(b => b.GLNM).FirstOrDefault();
                }
                if ((VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR") && TXN.JOBCD.retStr() != "")
                {
                    var jobdata = (from a in DB.M_JOBMST where a.JOBCD == TXN.JOBCD select new { a.JOBNM, a.EXPGLCD, a.HSNCODE }).FirstOrDefault();
                    if (jobdata != null)
                    {
                        VE.JOBNM = jobdata.JOBNM;
                        VE.JOBEXPGLCD = jobdata.EXPGLCD;
                        VE.JOBHSNCODE = jobdata.HSNCODE;
                    }
                }
                if (VE.MENU_PARA == "SBPOS")
                {
                    TXNMEMO = DB.T_TXNMEMO.Find(TXN.AUTONO);
                }
                //var Getstkdrcr = (from i in DB.T_TXN join j in DB.T_TXNDTL on i.AUTONO equals j.AUTONO where i.AUTONO == TXN.AUTONO select new { STKDRCR = j.STKDRCR, AUTONO = i.AUTONO }).OrderBy(s => s.AUTONO).FirstOrDefault();
                //if (Getstkdrcr != null)
                //{
                //    VE.STOCKHOLD = Getstkdrcr.STKDRCR == "C" ? true : false;
                //}
                var GetAdjBill = (from i in DBF.T_VCH_BL_ADJ where i.AUTONO == TXN.AUTONO select i).ToList();
                if (GetAdjBill != null && GetAdjBill.Count > 0) VE.ReturnAdjustwithBill = true;

                string Scm = CommVar.CurSchema(UNQSNO);
                string str1 = "";
                str1 += "select a.SLNO,a.TXNSLNO,a.ITGRPCD,a.ITGRPNM,a.BARGENTYPE,a.MTRLJOBCD,a.MTRLJOBNM,a.MTBARCODE,a.ITCD,a.ITNM,a.UOMCD,a.STYLENO,a.PARTCD,p.PARTNM, ";
                str1 += "p.PRTBARCODE,a.STKTYPE,a.STKNAME,a.BARNO,a.COLRCD,m.COLRNM,m.CLRBARCODE,a.SIZECD,l.SIZENM,l.SZBARCODE,a.SHADE,a.QNTY,a.NOS,a.RATE,a.DISCRATE, ";
                str1 += "a.DISCTYPE,a.TDDISCRATE,a.TDDISCTYPE,a.SCMDISCTYPE,a.SCMDISCRATE,a.HSNCODE,a.BALENO,a.PDESIGN,a.OURDESIGN,a.FLAGMTR,a.LOCABIN,a.BALEYR ";
                str1 += ",a.SALGLCD,a.PURGLCD,a.SALRETGLCD,a.PURRETGLCD,a.WPRATE,a.RPRATE,a.ITREM,a.ORDAUTONO,a.ORDSLNO,a.ORDDOCNO,a.ORDDOCDT,a.RPPRICEGEN, ";
                str1 += "a.WPPRICEGEN,a.LISTPRICE,a.LISTDISCPER,a.CUTLENGTH,a.NEGSTOCK ";
                str1 += ",a.AGDOCNO,a.AGDOCDT,a.PAGENO,a.PAGESLNO,a.PCSTYPE,a.glcd,a.BLUOMCD,a.COMMONUNIQBAR,a.FABITCD,a.FABITNM,a.WPPER,a.RPPER,a.RECPROGSLNO,a.BLQNTY, ";
                str1 += "a.CONVQTYPUNIT,a.FREESTK from(";

                str1 += "select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,nvl(i.PARTCD,j.PARTCD)PARTCD, ";
                str1 += "i.STKTYPE,q.STKNAME,i.BARNO,nvl(i.COLRCD,j.COLRCD)COLRCD,nvl(i.SIZECD,j.SIZECD)SIZECD,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE, ";
                str1 += "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,i.ORDAUTONO,i.ORDSLNO,r.DOCNO ORDDOCNO,r.DOCDT ORDDOCDT,n.RPPRICEGEN, ";
                str1 += "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,nvl(k.NEGSTOCK,n.NEGSTOCK)NEGSTOCK ";
                str1 += ",s.AGDOCNO,s.AGDOCDT,s.PAGENO,s.PAGESLNO,i.PCSTYPE,s.glcd,s.BLUOMCD,j.COMMONUNIQBAR,j.FABITCD,t.ITNM FABITNM,n.WPPER,n.RPPER,i.RECPROGSLNO,i.BLQNTY, ";
                str1 += "k.CONVQTYPUNIT,nvl(s.FREESTK,'N')FREESTK ";
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, ";
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_STKTYPE q," + Scm + ".T_CNTRL_HDR r ";
                str1 += "," + Scm + ".T_TXNDTL s, " + Scm + ".M_SITEM t ";
                str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+)  and k.ITGRPCD=n.ITGRPCD(+) ";
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+)  and i.STKTYPE=q.STKTYPE(+) and i.ORDAUTONO=r.AUTONO(+) and j.fabitcd=t.itcd(+) ";
                str1 += "and i.autono=s.autono and i.txnslno=s.slno ";
                str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ) a, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m," + Scm + ".M_PARTS p ";
                str1 += "where   a.SIZECD = l.SIZECD(+) and a.COLRCD = m.COLRCD(+) and a.PARTCD=p.PARTCD(+) ";
                str1 += "order by a.SLNO ";
                DataTable tbl = masterHelp.SQLquery(str1);

                VE.TBATCHDTL = (from DataRow dr in tbl.Rows
                                select new TBATCHDTL()
                                {
                                    SLNO = dr["SLNO"].retShort(),
                                    TXNSLNO = dr["TXNSLNO"].retShort(),
                                    ITGRPCD = dr["ITGRPCD"].retStr(),
                                    ITGRPNM = dr["ITGRPNM"].retStr(),
                                    MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                    MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                    MTBARCODE = dr["MTBARCODE"].retStr(),
                                    ITCD = dr["ITCD"].retStr(),
                                    ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                    UOM = dr["UOMCD"].retStr(),
                                    STYLENO = dr["STYLENO"].retStr(),
                                    PARTCD = dr["PARTCD"].retStr(),
                                    PARTNM = dr["PARTNM"].retStr(),
                                    PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                    COLRCD = dr["COLRCD"].retStr(),
                                    COLRNM = dr["COLRNM"].retStr(),
                                    CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                    SIZECD = dr["SIZECD"].retStr(),
                                    SIZENM = dr["SIZENM"].retStr(),
                                    SZBARCODE = dr["SZBARCODE"].retStr(),
                                    SHADE = dr["SHADE"].retStr(),
                                    QNTY = dr["QNTY"].retDbl(),
                                    NOS = dr["NOS"].retDbl(),
                                    RATE = dr["RATE"].retDbl(),
                                    DISCRATE = dr["DISCRATE"].retDbl(),
                                    DISCTYPE = dr["DISCTYPE"].retStr(),
                                    DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : dr["DISCTYPE"].retStr() == "A" ? "AftDsc%" : dr["DISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                    TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                    TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : dr["TDDISCTYPE"].retStr() == "A" ? "AftDsc%" : dr["TDDISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                    TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                    SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : dr["SCMDISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                    SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                    SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                    STKTYPE = dr["STKTYPE"].retStr(),
                                    STKNAME = dr["STKNAME"].retStr(),
                                    BARNO = dr["BARNO"].retStr(),
                                    HSNCODE = dr["HSNCODE"].retStr(),
                                    BALENO = dr["BALENO"].retStr(),
                                    PDESIGN = dr["PDESIGN"].retStr(),
                                    OURDESIGN = dr["OURDESIGN"].retStr(),
                                    FLAGMTR = dr["FLAGMTR"].retDbl(),
                                    LOCABIN = dr["LOCABIN"].retStr(),
                                    BALEYR = dr["BALEYR"].retStr(),
                                    BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                    GLCD = dr["GLCD"].retStr(),
                                    WPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPRATE"].retDbl() : (double?)null,
                                    RPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPRATE"].retDbl() : (double?)null,
                                    ITREM = dr["ITREM"].retStr(),
                                    ORDAUTONO = dr["ORDAUTONO"].retStr(),
                                    ORDSLNO = dr["ORDSLNO"].retStr() == "" ? (short?)null : dr["ORDSLNO"].retShort(),
                                    ORDDOCNO = dr["ORDDOCNO"].retStr(),
                                    ORDDOCDT = dr["ORDDOCDT"].retStr() == "" ? "" : dr["ORDDOCDT"].retStr().Remove(10),
                                    WPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPPRICEGEN"].retStr() : "",
                                    RPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPPRICEGEN"].retStr() : "",
                                    AGDOCNO = (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "PJBR") ? dr["AGDOCNO"].retStr() : "",
                                    AGDOCDT = (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "PJBR") ? (dr["AGDOCDT"].retStr() == "" ? (DateTime?)null : Convert.ToDateTime(dr["AGDOCDT"].retDateStr())) : (DateTime?)null,
                                    LISTPRICE = dr["LISTPRICE"].retDbl(),
                                    LISTDISCPER = dr["LISTDISCPER"].retDbl(),
                                    CUTLENGTH = dr["CUTLENGTH"].retDbl(),
                                    PAGENO = dr["PAGENO"].retInt(),
                                    PAGESLNO = dr["PAGESLNO"].retInt(),
                                    PCSTYPE = dr["PCSTYPE"].retStr(),
                                    NEGSTOCK = dr["NEGSTOCK"].retStr(),
                                    BLUOMCD = dr["BLUOMCD"].retStr() == "" ? "" : dr["CONVQTYPUNIT"].retStr() + " " + dr["BLUOMCD"].retStr(),
                                    COMMONUNIQBAR = dr["COMMONUNIQBAR"].retStr(),
                                    FABITCD = dr["FABITCD"].retStr(),
                                    FABITNM = dr["FABITNM"].retStr(),
                                    WPPER = dr["WPPER"].retDbl(),
                                    RPPER = dr["RPPER"].retDbl(),
                                    RECPROGSLNO = dr["RECPROGSLNO"].retShort(),
                                    BLQNTY = dr["BLQNTY"].retDbl(),
                                    CONVQTYPUNIT = dr["CONVQTYPUNIT"].retDbl(),
                                    FREESTK = dr["FREESTK"].retStr(),
                                }).OrderBy(s => s.SLNO).ToList();
                //int count = 0; var latbaleno = "";
                //for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                //{
                //    var baleno = VE.TBATCHDTL[i].BALENO;
                //    var salebalno = (from j in VE.TBATCHDTL where j.BALENO == baleno select j.BALENO).Distinct().FirstOrDefault();
                //    if (salebalno != null && latbaleno != salebalno)
                //    { count = count + 1; latbaleno = salebalno; }
                //}
                //VE.TOTBALENO = count.retDbl();
                if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC") && VE.DefaultAction == "V")
                {
                    VE.TOTBALENO = (from j in VE.TBATCHDTL where j.BALENO.retStr() != "" select j.BALENO).Distinct().Count().retDbl();
                }

                str1 = "";
                str1 += "select i.SLNO,j.ITGRPCD,k.ITGRPNM,i.MTRLJOBCD,l.MTRLJOBNM,l.MTBARCODE,i.ITCD,j.ITNM,j.STYLENO,j.UOMCD,i.STKTYPE,m.STKNAME,i.NOS,i.QNTY,i.FLAGMTR, ";
                str1 += "i.BLQNTY,i.RATE,i.AMT,i.DISCTYPE,i.DISCRATE,i.DISCAMT,i.TDDISCTYPE,i.TDDISCRATE,i.TDDISCAMT,i.SCMDISCTYPE,i.SCMDISCRATE,i.SCMDISCAMT, ";
                str1 += "i.TXBLVAL,i.IGSTPER,i.CGSTPER,i.SGSTPER,i.CESSPER,i.IGSTAMT,i.CGSTAMT,i.SGSTAMT,i.CESSAMT,i.NETAMT,i.HSNCODE,i.BALENO,i.GLCD,i.BALEYR,i.TOTDISCAMT, ";
                str1 += "i.ITREM,i.AGDOCNO,i.AGDOCDT,i.LISTPRICE,i.LISTDISCPER,i.PAGENO,i.PAGESLNO,i.BLUOMCD,j.CONVQTYPUNIT,nvl(i.FREESTK,'N')FREESTK,i.STKDRCR ";
                str1 += "from " + Scm + ".T_TXNDTL i, " + Scm + ".M_SITEM j, " + Scm + ".M_GROUP k, " + Scm + ".M_MTRLJOBMST l, " + Scm + ".M_STKTYPE m ";
                str1 += "where i.ITCD = j.ITCD(+) and j.ITGRPCD=k.ITGRPCD(+) and i.MTRLJOBCD=l.MTRLJOBCD(+)  and i.STKTYPE=m.STKTYPE(+)  ";
                str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ";
                str1 += "order by i.SLNO ";
                tbl = masterHelp.SQLquery(str1);

                VE.TTXNDTL = (from DataRow dr in tbl.Rows
                              select new TTXNDTL()
                              {
                                  SLNO = dr["SLNO"].retShort(),
                                  ITGRPCD = dr["ITGRPCD"].retStr(),
                                  ITGRPNM = dr["ITGRPNM"].retStr(),
                                  MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                  MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                  //MTBARCODE = dr["MTBARCODE"].retStr(),
                                  ITCD = dr["ITCD"].retStr(),
                                  ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                  UOM = dr["UOMCD"].retStr(),
                                  STKTYPE = dr["STKTYPE"].retStr(),
                                  STKNAME = dr["STKNAME"].retStr(),
                                  NOS = dr["NOS"].retDbl(),
                                  QNTY = dr["QNTY"].retDbl(),
                                  FLAGMTR = dr["FLAGMTR"].retDbl(),
                                  BLQNTY = dr["BLQNTY"].retDbl(),
                                  RATE = dr["RATE"].retDbl(),
                                  AMT = dr["AMT"].retDbl(),
                                  DISCTYPE = dr["DISCTYPE"].retStr(),
                                  DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : dr["DISCTYPE"].retStr() == "A" ? "AftDsc%" : dr["DISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                  DISCRATE = dr["DISCRATE"].retDbl(),
                                  DISCAMT = dr["DISCAMT"].retDbl(),
                                  TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : dr["TDDISCTYPE"].retStr() == "A" ? "AftDsc%" : dr["TDDISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                  TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                  TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                  TDDISCAMT = dr["TDDISCAMT"].retDbl(),
                                  SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : dr["SCMDISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                  SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                  SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                  SCMDISCAMT = dr["SCMDISCAMT"].retDbl(),
                                  TXBLVAL = dr["TXBLVAL"].retDbl(),
                                  IGSTPER = dr["IGSTPER"].retDbl(),
                                  CGSTPER = dr["CGSTPER"].retDbl(),
                                  SGSTPER = dr["SGSTPER"].retDbl(),
                                  CESSPER = dr["CESSPER"].retDbl(),
                                  IGSTAMT = dr["IGSTAMT"].retDbl(),
                                  CGSTAMT = dr["CGSTAMT"].retDbl(),
                                  SGSTAMT = dr["SGSTAMT"].retDbl(),
                                  CESSAMT = dr["CESSAMT"].retDbl(),
                                  NETAMT = dr["NETAMT"].retDbl(),
                                  HSNCODE = dr["HSNCODE"].retStr(),
                                  BALENO = dr["BALENO"].retStr(),
                                  GLCD = dr["GLCD"].retStr(),
                                  BALEYR = dr["BALEYR"].retStr(),
                                  TOTDISCAMT = dr["TOTDISCAMT"].retDbl(),
                                  ITREM = dr["ITREM"].retStr(),
                                  AGDOCNO = dr["AGDOCNO"].retStr(),
                                  AGDOCDT = dr["AGDOCDT"].retStr() == "" ? (DateTime?)null : Convert.ToDateTime(dr["AGDOCDT"].retDateStr()),
                                  LISTPRICE = dr["LISTPRICE"].retDbl(),
                                  LISTDISCPER = dr["LISTDISCPER"].retDbl(),
                                  PAGENO = dr["PAGENO"].retInt(),
                                  PAGESLNO = dr["PAGESLNO"].retInt(),
                                  BLUOMCD = dr["CONVQTYPUNIT"].retStr() + " " + dr["BLUOMCD"].retStr(),
                                  CONVQTYPUNIT = dr["CONVQTYPUNIT"].retDbl(),
                                  FREESTK = dr["FREESTK"].retStr(),
                                  STKDRCR = dr["STKDRCR"].retStr(),
                              }).OrderBy(s => s.SLNO).ToList();

                VE.B_T_QNTY = VE.TBATCHDTL.Sum(a => a.QNTY).retDbl();
                VE.B_T_NOS = VE.TBATCHDTL.Sum(a => a.NOS).retDbl();
                VE.T_NOS = VE.TTXNDTL.Sum(a => a.NOS).retDbl();
                VE.T_QNTY = VE.TTXNDTL.Sum(a => a.QNTY).retDbl();
                VE.T_FLAGMTR = VE.TTXNDTL.Sum(a => a.FLAGMTR).retDbl();
                VE.T_AMT = VE.TTXNDTL.Sum(a => a.AMT).retDbl();
                VE.T_DISCAMT = VE.TTXNDTL.Sum(a => a.DISCAMT).retDbl();
                VE.T_TDDISCAMT = VE.TTXNDTL.Sum(a => a.TDDISCAMT).retDbl();
                VE.T_SCMDISCAMT = VE.TTXNDTL.Sum(a => a.SCMDISCAMT).retDbl();
                VE.T_GROSS_AMT = VE.TTXNDTL.Sum(a => a.TXBLVAL).retDbl();
                VE.T_IGST_AMT = VE.TTXNDTL.Sum(a => a.IGSTAMT).retDbl();
                VE.T_CGST_AMT = VE.TTXNDTL.Sum(a => a.CGSTAMT).retDbl();
                VE.T_SGST_AMT = VE.TTXNDTL.Sum(a => a.SGSTAMT).retDbl();
                VE.T_CESS_AMT = VE.TTXNDTL.Sum(a => a.CESSAMT).retDbl();
                VE.T_NET_AMT = VE.TTXNDTL.Sum(a => a.NETAMT).retDbl();
                if (VE.MENU_PARA == "PI" && VE.TTXNDTL != null && VE.TTXNDTL.Count > 0)
                {
                    VE.STOCKHOLD = VE.TTXNDTL[0].STKDRCR == "C" ? true : false;
                }
                //fill prodgrpgstper in t_batchdtl
                DataTable allprodgrpgstper_data = new DataTable();
                DataTable stockdata = new DataTable();
                string BARNO = (from a in VE.TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
                string ITCD = (from a in VE.TBATCHDTL select a.ITCD).ToArray().retSqlfromStrarray();
                //string MTRLJOBCD = (from a in VE.TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                string ITGRPCD = (from a in VE.TBATCHDTL select a.ITGRPCD).ToArray().retSqlfromStrarray();
                string BARNO_ = "", ITCD_ = "", ITGRPCD_ = "";
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                {
                    if (BARNO.Length > 1000)
                    {
                        BARNO_ = "";

                    }
                    else
                    {
                        BARNO_ = BARNO;
                    }
                    if (ITCD.Length > 1000)
                    {
                        ITCD_ = "";
                    }
                    else
                    {
                        ITCD_ = ITCD;
                    }
                    if (ITGRPCD.Length > 1000)
                    {
                        ITGRPCD_ = "";
                    }
                    else
                    {
                        ITGRPCD_ = ITGRPCD;

                    }

                    allprodgrpgstper_data = salesfunc.GetBarHelp(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retStr(), BARNO_, ITCD_.retStr(), "", "", ITGRPCD_, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA, "", "", false, false, true, "", false);
                    stockdata = salesfunc.GetStock(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retSqlformat(), BARNO_, ITCD_.retStr(), "", TXN.AUTONO.retSqlformat(), ITGRPCD_, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);


                    if (BARNO.Length > 1000)
                    {
                        allprodgrpgstper_data = (from DataRow dr in allprodgrpgstper_data.Rows where BARNO.Contains(dr["barno"].retStr()) select dr).ToList().CopyToDataTable();
                        stockdata = (from DataRow dr in stockdata.Rows where BARNO.Contains(dr["barno"].retStr()) select dr).ToList().CopyToDataTable();
                    }
                    if (ITCD.Length > 1000)
                    {
                        allprodgrpgstper_data = (from DataRow dr in allprodgrpgstper_data.Rows where ITCD.Contains(dr["itcd"].retStr()) select dr).ToList().CopyToDataTable();
                        stockdata = (from DataRow dr in stockdata.Rows where ITCD.Contains(dr["itcd"].retStr()) select dr).ToList().CopyToDataTable();
                    }
                    if (ITGRPCD.Length > 1000)
                    {
                        allprodgrpgstper_data = (from DataRow dr in allprodgrpgstper_data.Rows where ITGRPCD.Contains(dr["itgrpcd"].retStr()) select dr).ToList().CopyToDataTable();
                        stockdata = (from DataRow dr in stockdata.Rows where ITGRPCD.Contains(dr["itgrpcd"].retStr()) select dr).ToList().CopyToDataTable();
                    }



                }
                else
                {
                    allprodgrpgstper_data = salesfunc.GetStock(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", TXN.AUTONO.retSqlformat(), ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);
                }
                DataTable syscnfgdata = salesfunc.GetSyscnfgData(TXN.DOCDT.retDateStr());
                var chk_child = ChildRecordCheck(TXN.AUTONO);  //modify by mithun

                foreach (var v in VE.TBATCHDTL)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "";
                    v.GSTPER = VE.TTXNDTL.Where(a => a.SLNO == v.TXNSLNO).Sum(b => b.IGSTPER + b.CGSTPER + b.SGSTPER).retDbl();
                    if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
                    {
                        var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "'");
                        if (DATA.Count() > 0)
                        {
                            DataTable tax_data = DATA.CopyToDataTable();
                            if (tax_data != null && tax_data.Rows.Count > 0)
                            {
                                v.BALSTOCK = tax_data.Rows[0]["BALQNTY"].retDbl();
                                PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                                if (PRODGRPGSTPER != "")
                                {
                                    ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl(), v.SCMDISCTYPE, v.SCMDISCRATE.retDbl());
                                    if (ALL_GSTPER.retStr() != "")
                                    {
                                        var gst = ALL_GSTPER.Split(',').ToList();
                                        GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                                    }
                                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                                    v.ALL_GSTPER = ALL_GSTPER;
                                    v.GSTPER = GSTPER.retDbl();
                                }
                                if (TXN.REVCHRG == "N")
                                {
                                    var a = PRODGRPGSTPER.Split('~')[0];
                                    var b = PRODGRPGSTPER.Split('~')[1];
                                    var c = PRODGRPGSTPER.Split('~')[2];
                                    var d = PRODGRPGSTPER.Split('~')[3];
                                    var e = PRODGRPGSTPER.Split('~')[4];
                                    var str = a + "~" + b + "~" + 0 + "~" + 0 + "~" + 0;
                                    v.PRODGRPGSTPER = str;
                                    v.ALL_GSTPER = 0 + "," + 0 + "," + 0;
                                    v.GSTPER = 0;
                                }
                                if (tax_data.Rows[0]["barimage"].retStr() != "")
                                {
                                    string barimage = tax_data.Rows[0]["barimage"].retStr();
                                    var brimgs = barimage.Split((char)179);
                                    v.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                                    foreach (var barimg in brimgs)
                                    {
                                        string barfilename = barimg.Split('~')[0];
                                        string barimgdesc = barimg.Split('~')[1];
                                        v.BarImages += (char)179 + CommVar.WebUploadDocURL(barfilename) + "~" + barimgdesc;
                                        string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                                        FROMpath = Path.Combine(FROMpath, "");
                                        string TOPATH = CommVar.LocalUploadDocPath() + barfilename;
                                        Cn.CopyImage(FROMpath, TOPATH);
                                    }
                                    v.BarImages = v.BarImages.retStr().TrimStart((char)179);
                                }
                            }
                        }
                    }
                    //if purchase stock det
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {
                        var stkDATA = stockdata.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "'");
                        if (stkDATA != null && stkDATA.Count() > 0)
                        {
                            v.BALSTOCK = stkDATA[0]["BALQNTY"].retDbl();
                        }
                    }
                    //var chk_child = ChildRecordCheck(TXN.AUTONO);  //modify by mithun
                    if (chk_child != "")
                    {
                        v.ChildData = "Y";
                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC") VE.ChildData = "child record found";
                    }

                    if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && (syscnfgdata != null && syscnfgdata.Rows.Count > 0))
                    {

                        if (v.WPPRICEGEN.retStr() == "" || v.RPPRICEGEN.retStr() == "" || v.WPPER.retDbl() == 0 || v.RPPER.retDbl() == 0)
                        {
                            if (v.WPPRICEGEN.retStr() == "")
                            {
                                v.WPPRICEGEN = syscnfgdata.Rows[0]["wppricegen"].retStr();
                            }
                            if (v.RPPRICEGEN.retStr() == "")
                            {
                                v.RPPRICEGEN = syscnfgdata.Rows[0]["rppricegen"].retStr();
                            }
                            if (v.WPPER.retDbl() == 0)
                            {
                                v.WPPER = syscnfgdata.Rows[0]["WPPER"].retDbl();
                            }
                            if (v.RPPER.retDbl() == 0)
                            {
                                v.RPPER = syscnfgdata.Rows[0]["RPPER"].retDbl();
                            }
                        }
                    }
                }
                //fill prodgrpgstper in t_txndtl
                double IGST_PER = 0; double CGST_PER = 0; double SGST_PER = 0; double CESS_PER = 0; double DUTY_PER = 0;
                foreach (var v in VE.TTXNDTL)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "";

                    //if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
                    //{
                    //    var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "'");
                    //    if (DATA.Count() > 0)
                    //    {
                    //        DataTable stock_data = DATA.CopyToDataTable();
                    //        if (stock_data != null && stock_data.Rows.Count > 0)
                    //        {
                    //            v.BALSTOCK = stock_data.Rows[0]["BALQNTY"].retDbl();
                    //        }
                    //    }
                    //}

                    var tax_data = (from a in VE.TBATCHDTL
                                    where a.TXNSLNO == v.SLNO
                                    select new { a.PRODGRPGSTPER, a.ALL_GSTPER, a.FABITCD, a.FABITNM, a.RATE, a.RECPROGSLNO, a.BALSTOCK, a.NEGSTOCK }).FirstOrDefault();
                    if (tax_data != null)
                    {
                        PRODGRPGSTPER = tax_data.PRODGRPGSTPER.retStr();
                        ALL_GSTPER = tax_data.ALL_GSTPER.retStr();
                        v.FABITCD = tax_data.FABITCD.retStr();
                        v.FABITNM = tax_data.FABITNM.retStr();
                        v.RECPROGSLNO = tax_data.RECPROGSLNO.retShort();
                        v.BALSTOCK = tax_data.BALSTOCK;
                    }
                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                    v.ALL_GSTPER = ALL_GSTPER;

                    double IGST = v.IGSTPER.retDbl();
                    double CGST = v.CGSTPER.retDbl();
                    double SGST = v.SGSTPER.retDbl();
                    double CESS = v.CESSPER.retDbl();
                    double DUTY = v.DUTYPER.retDbl();

                    if (IGST > IGST_PER)
                    {
                        IGST_PER = IGST;
                    }
                    if (CGST > CGST_PER)
                    {
                        CGST_PER = CGST;
                    }
                    if (SGST > SGST_PER)
                    {
                        SGST_PER = SGST;
                    }
                    if (CESS > CESS_PER)
                    {
                        CESS_PER = CESS;
                    }
                    if (DUTY > DUTY_PER)
                    {
                        DUTY_PER = DUTY;
                    }
                }
                string S_P = "";
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") { S_P = "P"; } else { S_P = "S"; }
                string sql = "select a.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, a.amtdesc, ";
                sql += "b.glcd, a.hsncode, a.amtrate, a.curr_amt, a.amt,a.igstper, a.igstamt, a.sgstper, a.sgstamt, ";
                sql += "a.cgstper, a.cgstamt,a.cessper, a.cessamt, a.dutyper, a.dutyamt ";
                sql += "from " + Scm + ".t_txnamt a, " + Scm + ".m_amttype b ";
                sql += "where a.amtcd=b.amtcd(+) and b.salpur='" + S_P + "' and a.autono='" + TXN.AUTONO + "' ";
                sql += "union ";
                sql += "select b.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, '' amtdesc, ";
                sql += "b.glcd, b.hsncode, 0 amtrate, 0 curr_amt, 0 amt,0 igstper, 0 igstamt, 0 sgstper, 0 sgstamt, ";
                sql += "0 cgstper, 0 cgstamt, 0 CESSPER, 0 CESSAMT, 0 dutyper, 0 dutyamt ";
                sql += "from " + Scm + ".m_amttype b, " + Scm + ".m_cntrl_hdr c ";
                sql += "where b.m_autono=c.m_autono(+) and b.salpur='" + S_P + "'  and nvl(c.inactive_tag,'N') = 'N' and ";
                sql += "b.amtcd not in (select amtcd from " + Scm + ".t_txnamt where autono='" + TXN.AUTONO + "') ";
                var AMOUNT_DATA = masterHelp.SQLquery(sql);

                VE.TTXNAMT = (from DataRow dr in AMOUNT_DATA.Rows
                              select new TTXNAMT()
                              {
                                  AMTCD = dr["amtcd"].ToString(),
                                  ADDLESS = dr["addless"].ToString(),
                                  TAXCODE = dr["taxcode"].ToString(),
                                  GLCD = dr["GLCD"].ToString(),
                                  AMTNM = dr["amtnm"].ToString(),
                                  CALCCODE = dr["calccode"].ToString(),
                                  CALCTYPE = dr["CALCTYPE"].ToString(),
                                  CALCFORMULA = dr["calcformula"].ToString(),
                                  AMTDESC = dr["amtdesc"].ToString(),
                                  HSNCODE = dr["hsncode"].ToString(),
                                  AMTRATE = Convert.ToDouble(dr["amtrate"] == DBNull.Value ? null : dr["amtrate"].ToString()),
                                  CURR_AMT = Convert.ToDouble(dr["curr_amt"] == DBNull.Value ? null : dr["curr_amt"].ToString()),
                                  AMT = Convert.ToDouble(dr["amt"] == DBNull.Value ? null : dr["amt"].ToString()),
                                  IGSTPER = Convert.ToDouble(dr["igstper"] == DBNull.Value ? null : dr["igstper"].ToString()),
                                  IGSTPER_DESP = Convert.ToDouble(dr["igstper"] == DBNull.Value ? null : dr["igstper"].ToString()),
                                  CGSTPER = Convert.ToDouble(dr["cgstper"] == DBNull.Value ? null : dr["cgstper"].ToString()),
                                  CGSTPER_DESP = Convert.ToDouble(dr["cgstper"] == DBNull.Value ? null : dr["cgstper"].ToString()),
                                  SGSTPER = Convert.ToDouble(dr["sgstper"] == DBNull.Value ? null : dr["sgstper"].ToString()),
                                  SGSTPER_DESP = Convert.ToDouble(dr["sgstper"] == DBNull.Value ? null : dr["sgstper"].ToString()),
                                  CESSPER = Convert.ToDouble(dr["CESSPER"] == DBNull.Value ? null : dr["CESSPER"].ToString()),
                                  CESSPER_DESP = Convert.ToDouble(dr["CESSPER"] == DBNull.Value ? null : dr["CESSPER"].ToString()),
                                  DUTYPER = Convert.ToDouble(dr["dutyper"] == DBNull.Value ? null : dr["dutyper"].ToString()),
                                  IGSTAMT = Convert.ToDouble(dr["igstamt"] == DBNull.Value ? null : dr["igstamt"].ToString()),
                                  IGSTAMT_DESP = Convert.ToDouble(dr["igstamt"] == DBNull.Value ? null : dr["igstamt"].ToString()),
                                  CGSTAMT = Convert.ToDouble(dr["cgstamt"] == DBNull.Value ? null : dr["cgstamt"].ToString()),
                                  CGSTAMT_DESP = Convert.ToDouble(dr["cgstamt"] == DBNull.Value ? null : dr["cgstamt"].ToString()),
                                  SGSTAMT = Convert.ToDouble(dr["sgstamt"] == DBNull.Value ? null : dr["sgstamt"].ToString()),
                                  SGSTAMT_DESP = Convert.ToDouble(dr["sgstamt"] == DBNull.Value ? null : dr["sgstamt"].ToString()),
                                  CESSAMT = Convert.ToDouble(dr["CESSAMT"] == DBNull.Value ? null : dr["CESSAMT"].ToString()),
                                  CESSAMT_DESP = Convert.ToDouble(dr["CESSAMT"] == DBNull.Value ? null : dr["CESSAMT"].ToString()),
                                  DUTYAMT = Convert.ToDouble(dr["dutyamt"] == DBNull.Value ? null : dr["dutyamt"].ToString()),
                              }).ToList();

                double A_IGST_AMT = 0; double A_CGST_AMT = 0; double A_SGST_AMT = 0; double A_CESS_AMT = 0; double A_DUTY_AMT = 0; double A_TOTAL_CURR = 0; double A_TOTAL_AMOUNT = 0; double A_TOTAL_NETAMOUNT = 0;

                for (int p = 0; p <= VE.TTXNAMT.Count - 1; p++)
                {
                    VE.TTXNAMT[p].SLNO = Convert.ToInt16(p + 1);

                    if (VE.TTXNAMT[p].IGSTPER == 0) { VE.TTXNAMT[p].IGSTPER = IGST_PER; }
                    if (VE.TTXNAMT[p].IGSTPER_DESP == 0) { VE.TTXNAMT[p].IGSTPER_DESP = IGST_PER; }
                    if (VE.TTXNAMT[p].CGSTPER == 0) { VE.TTXNAMT[p].CGSTPER = CGST_PER; }
                    if (VE.TTXNAMT[p].CGSTPER_DESP == 0) { VE.TTXNAMT[p].CGSTPER_DESP = CGST_PER; }
                    if (VE.TTXNAMT[p].SGSTPER == 0) { VE.TTXNAMT[p].SGSTPER = SGST_PER; }
                    if (VE.TTXNAMT[p].SGSTPER_DESP == 0) { VE.TTXNAMT[p].SGSTPER_DESP = SGST_PER; }
                    if (VE.TTXNAMT[p].CESSPER == 0) { VE.TTXNAMT[p].CESSPER = CESS_PER; }
                    if (VE.TTXNAMT[p].CESSPER_DESP == 0) { VE.TTXNAMT[p].CESSPER_DESP = CESS_PER; }
                    if (VE.TTXNAMT[p].DUTYPER == 0) { VE.TTXNAMT[p].DUTYPER = DUTY_PER; }

                    double amult = 1;
                    if (VE.TTXNAMT[p].ADDLESS == "L") amult = -1; else amult = 1;
                    A_TOTAL_CURR = A_TOTAL_CURR + (VE.TTXNAMT[p].CURR_AMT.retDbl() * amult);
                    A_TOTAL_AMOUNT = A_TOTAL_AMOUNT + (VE.TTXNAMT[p].AMT.retDbl() * amult);
                    A_IGST_AMT = A_IGST_AMT + (VE.TTXNAMT[p].IGSTAMT.retDbl() * amult);
                    A_CGST_AMT = A_CGST_AMT + (VE.TTXNAMT[p].CGSTAMT.retDbl() * amult);
                    A_SGST_AMT = A_SGST_AMT + (VE.TTXNAMT[p].SGSTAMT.retDbl() * amult);
                    A_CESS_AMT = A_CESS_AMT + (VE.TTXNAMT[p].CESSAMT.retDbl() * amult);
                    A_DUTY_AMT = A_DUTY_AMT + (VE.TTXNAMT[p].DUTYAMT.retDbl() * amult);
                    VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT.Value + VE.TTXNAMT[p].IGSTAMT.Value + VE.TTXNAMT[p].CGSTAMT.Value + VE.TTXNAMT[p].SGSTAMT.Value + VE.TTXNAMT[p].CESSAMT.Value + VE.TTXNAMT[p].DUTYAMT.Value;
                    A_TOTAL_NETAMOUNT = A_TOTAL_NETAMOUNT + (VE.TTXNAMT[p].NETAMT.retDbl() * amult);
                }
                VE.A_T_CURR = A_TOTAL_CURR;
                VE.A_T_AMOUNT = A_TOTAL_AMOUNT;
                VE.A_T_NET = A_TOTAL_NETAMOUNT;
                VE.A_T_IGST = A_IGST_AMT;
                VE.A_T_CGST = A_CGST_AMT;
                VE.A_T_SGST = A_SGST_AMT;
                VE.A_T_CESS = A_CESS_AMT;
                VE.A_T_DUTY = A_DUTY_AMT;

                //total amt
                VE.TOTNOS = VE.T_NOS.retDbl();
                VE.TOTQNTY = VE.T_QNTY.retDbl();
                VE.TOTTAXVAL = VE.T_GROSS_AMT.retDbl() + VE.A_T_AMOUNT.retDbl();
                VE.TOTTAX = VE.T_CGST_AMT.retDbl() + VE.A_T_CGST.retDbl() + VE.T_SGST_AMT.retDbl() + VE.A_T_SGST.retDbl() + VE.T_IGST_AMT.retDbl() + VE.A_T_IGST.retDbl();
                VE.DISPBLAMT = TXN.BLAMT;
                VE.DISPTCSAMT = TXN.TCSAMT;

                if (VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR")
                {
                    string scmf = CommVar.FinSchema(UNQSNO);
                    sql = "";
                    sql += "select b.DOCNO,b.DOCDT,b.SLCD,b.AUTONO ,c.SLNM from " + Scm + ".T_TXN_LINKNO a," + Scm + ".T_CNTRL_HDR b,  ";
                    sql += scmf + ".M_SUBLEG c where a.LINKAUTONO= b.AUTONO(+) and b.SLCD=c.SLCD(+)  and a.autono='" + TXN.AUTONO + "' ";
                    sql += "  order by b.DOCNO,b.DOCDT ";
                    tbl = masterHelp.SQLquery(sql);
                    VE.PENDING_ISSUE = (from DataRow dr in tbl.Rows
                                        select new PENDING_ISSUE
                                        {
                                            Checked = true,
                                            ISSUEAUTONO = dr["autono"].ToString(),
                                            ISSUEDOCNO = dr["docno"].ToString(),
                                            ISSUEDOCDT = dr["docdt"] == null ? "" : dr["docdt"].ToString().Substring(0, 10),
                                        }).DistinctBy(a => a.ISSUEDOCNO).ToList();

                    short SL_NO = 0; VE.PENDING_ISSUE.Where(A => A != null && VE.PENDING_ISSUE.Count > 0).ForEach(a => a.SLNO = ++SL_NO);
                }

            }
            //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
            if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            if (VE.MENU_PARA == "PI")
            {
                #region if PROFOMA used in sale bill then PROFOMA not able to uncancell. 
                var CanRemChk = TCH.CANC_REM.retStr().Split("[]".ToCharArray());
                if (CanRemChk.Count() > 1 && CanRemChk[1].retStr() != "")
                {
                    var docnoo = CanRemChk[1].retStr();
                    var chkautono = DB.T_CNTRL_HDR.Where(a => a.DOCNO == docnoo).Select(b => b.AUTONO).ToList();
                    if (chkautono != null && chkautono.Count > 0) VE.PI_tag = "Y";
                }
                #endregion
            }
            if (VE.MENU_PARA == "ISS" || VE.MENU_PARA == "REC")
            {
                string scm1 = CommVar.CurSchema(UNQSNO);
                sql = "select distinct a.autono, b.cancel from " + scm1 + ".T_STKTRNF a, " + scm1 + ".t_cntrl_hdr b ";
                sql += "where a.autono=b.autono  and a.othautono='" + TXN.AUTONO + "' ";
                DataTable tbl1 = masterHelp.SQLquery(sql);
                if (tbl1.Rows.Count > 0)
                {
                    VE.Edit = "";
                    if (tbl1.Rows[0]["cancel"].retStr() != "Y") VE.Delete = "";
                }
            }


            return VE;
        }
        public ActionResult SearchPannelData(TransactionSaleEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT, string SRC_FLAG)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
                Cn.getQueryString(VE);
                List<DocumentType> DocumentType = new List<DocumentType>();
                DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
                VE.SHOWBLTYPE = dropDownHelp.DropDownBLTYPE().Count > 0 ? "Y" : "N";
                string sql = "";

                sql = "select distinct a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.slcd, c.slnm, c.district, nvl(a.blamt,0) blamt,a.PREFDT,a.PREFno,e.bltype,nvl(b.cancel,'N')cancel,b.doconlyno,b.docdt dt,g.docno issdocno,to_char(g.docdt,'dd/mm/yyyy') issdocdt ";
                sql += "from " + scm + ".t_txn a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_subleg c, " + scm + ".t_txndtl d, " + scm + ".t_txnoth e, " + scm + ".T_STKTRNF f, " + scm + ".t_cntrl_hdr g ";
                sql += "where a.autono=b.autono and a.slcd=c.slcd(+) and b.doccd in (" + doccd + ") and a.autono=d.autono(+) and a.autono=e.autono(+) and a.autono=f.autono(+) and f.othautono=g.autono(+) and ";
                if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
                if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
                if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%' or a.prefno like '%" + SRC_DOCNO.retStr() + "%') and  ";
                if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
                if (SRC_FLAG.retStr() != "") sql += "(upper(d.baleno) like '%" + SRC_FLAG.retStr().ToUpper() + "%') and ";
                sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
                if (CommVar.ClientCode(UNQSNO) == "DIWH") sql += "order by b.doccd,b.doconlyno,b.docdt "; else sql += "order by b.docdt, b.docno ";//diwans heritage wants docno wise nav
                DataTable tbl = masterHelp.SQLquery(sql);

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC" || VE.MENU_PARA == "SR")
                {
                    var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Pblno" + Cn.GCS() + "Pbldt" + Cn.GCS() + "Bill Amt"
                        + (VE.SHOWBLTYPE.retStr() == "Y" ? (Cn.GCS() + "Bill Type") : "") + Cn.GCS() + "AUTO NO";
                    for (int j = 0; j <= tbl.Rows.Count - 1; j++)
                    {
                        string cancel = tbl.Rows[j]["cancel"].retStr() == "Y" ? "<b> (Cancelled)</b>" : "";
                        SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + cancel + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>"
                            + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td>" + tbl.Rows[j]["PREFNO"] + " </td><td>"
                            + (tbl.Rows[j]["PREFDT"].retStr() == "" ? "" : tbl.Rows[j]["PREFDT"].retStr().Remove(10)) + " </td><td class='text-right'>" + Convert.ToDouble(tbl.Rows[j]["blamt"]) + " </td>"
                            + (VE.SHOWBLTYPE.retStr() == "Y" ? "<td>" + tbl.Rows[j]["bltype"] + " </td>" : "")
                            + "<td>" + tbl.Rows[j]["autono"] + " </td></tr>");
                    }
                    return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), (VE.SHOWBLTYPE.retStr() == "Y" ? "7" : "6"), (VE.SHOWBLTYPE.retStr() == "Y" ? "7" : "6")));
                }
                else if (VE.MENU_PARA == "REC")
                {
                    var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Pblno" + Cn.GCS() + "Pbldt" + Cn.GCS() + "Bill Amt"
                        + (VE.SHOWBLTYPE.retStr() == "Y" ? (Cn.GCS() + "Bill Type") : "") + Cn.GCS() + "AUTO NO" + Cn.GCS() + "Issue Doc No" + Cn.GCS() + "Issue Doc Date";
                    for (int j = 0; j <= tbl.Rows.Count - 1; j++)
                    {
                        string cancel = tbl.Rows[j]["cancel"].retStr() == "Y" ? "<b> (Cancelled)</b>" : "";
                        SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + cancel + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>"
                            + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td>" + tbl.Rows[j]["PREFNO"] + " </td><td>"
                            + (tbl.Rows[j]["PREFDT"].retStr() == "" ? "" : tbl.Rows[j]["PREFDT"].retStr().Remove(10)) + " </td><td class='text-right'>" + Convert.ToDouble(tbl.Rows[j]["blamt"]) + " </td>"
                            + (VE.SHOWBLTYPE.retStr() == "Y" ? "<td>" + tbl.Rows[j]["bltype"] + " </td>" : "")
                            + "<td>" + tbl.Rows[j]["autono"] + " </td><td>" + tbl.Rows[j]["issdocno"] + " </td><td>" + tbl.Rows[j]["issdocdt"] + " </td></tr>");
                    }
                    return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), (VE.SHOWBLTYPE.retStr() == "Y" ? "7" : "6"), (VE.SHOWBLTYPE.retStr() == "Y" ? "7" : "6")));
                }
                else
                {
                    var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Bill Amt"
                        + (VE.SHOWBLTYPE.retStr() == "Y" ? (Cn.GCS() + "Bill Type") : "") + Cn.GCS() + "AUTO NO";
                    for (int j = 0; j <= tbl.Rows.Count - 1; j++)
                    {
                        string cancel = tbl.Rows[j]["cancel"].retStr() == "Y" ? "<b> (Cancelled)</b>" : "";
                        SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + cancel + " </td><td>" + tbl.Rows[j]["docdt"]
                            + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td class='text-right'>"
                            + Convert.ToDouble(tbl.Rows[j]["blamt"]) + " </td>"
                            + (VE.SHOWBLTYPE.retStr() == "Y" ? "<td>" + tbl.Rows[j]["bltype"] + " </td>" : "")
                            + "<td>" + tbl.Rows[j]["autono"] + " </td></tr>");
                    }
                    return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), (VE.SHOWBLTYPE.retStr() == "Y" ? "5" : "4"), (VE.SHOWBLTYPE.retStr() == "Y" ? "5" : "4")));
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetTCSDetails(string val, string TAG, string PARTY, string AUTONO)
        {
            try
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                string linktdscode = "'Y','Z'";
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") linktdscode = "'X'";
                if (TAG.retStr() == "") return Content("Enter Document Date");
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.TDSCODE_help(TAG, val, PARTY, "TCS", linktdscode));
                }
                else
                {
                    string str = masterHelp.TDSCODE_help(TAG, val, PARTY, "TCS", linktdscode);
                    if (str.IndexOf(Convert.ToChar(Cn.GCS())) >= 0)
                    {
                        double TDSLIMIT = str.retCompValue("TDSLIMIT").retDbl();

                        ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO).ToString());
                        string panno = DBF.M_SUBLEG.Where(a => a.SLCD == PARTY).Select(b => b.PANNO).FirstOrDefault();
                        string AMT = salesfunc.getSlcdTCSonCalc(panno.retStr(), TAG, VE.MENU_PARA, AUTONO.retStr()).ToString();
                        AMT = AMT.retDbl() > TDSLIMIT.retDbl() ? TDSLIMIT.retStr() : AMT.retStr();
                        str += "^AMT=^" + AMT + Cn.GCS();
                    }

                    return Content(str);
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        public DataTable getTDS(string docdt, string slcd, string linktdscode = "")
        {
            string scmf = CommVar.FinSchema(UNQSNO);
            string sql = "select a.tdscode, a.edate, a.tdsper, a.tdspernoncmp, ";
            if (slcd.retStr() != "") sql += "(select CMPNONCMP from  " + scmf + ".m_subleg where slcd='" + slcd + "') as CMPNONCMP, "; else sql += "'' CMPNONCMP, ";
            sql += " b.tdsnm, b.secno, b.glcd,a.tdslimit,a.tdscalcon,a.tdsroundcal from ";
            sql += "(select tdscode, edate, tdsper, tdspernoncmp,tdslimit,tdscalcon,tdsroundcal from ";
            sql += "(select a.tdscode, a.edate, a.tdsper, a.tdspernoncmp,a.tdslimit,a.tdscalcon,a.tdsroundcal, ";
            sql += "row_number() over(partition by a.tdscode order by a.edate desc) as rn ";
            sql += "from " + scmf + ".m_tds_cntrl_dtl a ";
            sql += "where  edate <= to_date('" + docdt + "', 'dd/mm/yyyy')  ";
            if (linktdscode.retStr() != "") sql += " and tdscode in (" + linktdscode + ") ";
            sql += ") where rn = 1 ) a, ";
            sql += "" + scmf + ".m_tds_cntrl b ";
            sql += "where a.tdscode = b.tdscode(+) ";

            DataTable dt = masterHelp.SQLquery(sql);
            return dt;
        }
        public ActionResult GetSubLedgerDetails(string val, string Code, string Autono, string linktdscode, string bltype, string doccd)
        {
            try
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                var code_data = Code.Split(Convert.ToChar(Cn.GCS()));
                string caption = "";
                if (code_data.Count() > 1)
                {
                    if (code_data[0].retStr() == "Party")
                    {
                        if (code_data[1] == "")
                        {
                            return Content("Please Select Document Date !!");
                        }
                        else
                        {
                            Code = MenuDescription(VE.MENU_PARA).Rows[0]["PARTYLINKCD"].ToString();
                            caption = Code.retStr() == "D" ? "Buyer" : Code.retStr() == "C" ? "Supplier" : "Party";
                        }
                    }
                    else
                    {
                        //if (code_data[1] == "")
                        //{
                        //    return Content("Please Select Agent !!");
                        //}
                        //else
                        //{
                        Code = code_data[0];
                        //}
                    }


                }

                var str = masterHelp.SLCD_help(val, Code, caption);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (code_data[0].retStr() == "Party")
                    {
                        string TCSPER = "", TCSCODE = "", TCSNM = "", TDSLIMIT = "", TDSCALCON = "", AMT = "", TDSROUNDCAL = "", TOTTURNOVER = "";
                        if (str.IndexOf(Cn.GCS()) > 0)
                        {
                            string DOCTAG = MenuDescription(VE.MENU_PARA).Rows[0]["DOCTAG"].ToString().retSqlformat();
                            bltype = bltype.retStr() == "" ? "" : bltype.retSqlformat();
                            var party_data = salesfunc.GetSlcdDetails(val, code_data[1], "", DOCTAG, bltype);
                            if (party_data != null && party_data.Rows.Count > 0)
                            {
                                if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") party_data.Rows[0]["TCSAPPL"] = "N";
                                if (party_data.Rows[0]["TCSAPPL"].retStr() == "Y")
                                {
                                    linktdscode = linktdscode.retStr() == "" ? "'Y'" : linktdscode.retStr().retSqlformat();

                                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") linktdscode = "'X'";
                                    var tdsdt = getTDS(code_data[1], val, linktdscode);
                                    if (tdsdt != null && tdsdt.Rows.Count > 0)
                                    {
                                        TCSPER = tdsdt.Rows[0]["TDSPER"].retStr();
                                        TCSCODE = tdsdt.Rows[0]["TDSCODE"].retStr();
                                        TCSNM = tdsdt.Rows[0]["TDSNM"].retStr();
                                        TDSLIMIT = tdsdt.Rows[0]["TDSLIMIT"].retStr();
                                        TDSCALCON = tdsdt.Rows[0]["TDSCALCON"].retStr();
                                        TDSROUNDCAL = tdsdt.Rows[0]["TDSROUNDCAL"].retStr();
                                    }
                                    AMT = salesfunc.getSlcdTCSonCalc(party_data.Rows[0]["PANNO"].retStr(), code_data[1], VE.MENU_PARA, Autono.retStr()).ToString();
                                    TOTTURNOVER = AMT;
                                    AMT = AMT.retDbl() > TDSLIMIT.retDbl() ? TDSLIMIT.retStr() : AMT.retStr();
                                }
                                str = masterHelp.ToReturnFieldValues("", party_data);
                                str += "^" + "TCSPER" + "=^" + TCSPER + Cn.GCS();
                                str += "^" + "TDSLIMIT" + "=^" + TDSLIMIT + Cn.GCS();
                                str += "^" + "TDSCALCON" + "=^" + TDSCALCON + Cn.GCS();
                                str += "^" + "AMT" + "=^" + AMT + Cn.GCS();
                                str += "^" + "TDSROUNDCAL" + "=^" + TDSROUNDCAL + Cn.GCS();
                                str += "^" + "TCSCODE" + "=^" + TCSCODE + Cn.GCS();
                                str += "^" + "TCSNM" + "=^" + TCSNM + Cn.GCS();
                                str += "^" + "TOTTURNOVER" + "=^" + TOTTURNOVER + Cn.GCS();

                                string scmdisctype = party_data.Rows[0]["scmdisctype"].retStr() == "P" ? "%" : party_data.Rows[0]["scmdisctype"].retStr() == "N" ? "Nos" : party_data.Rows[0]["scmdisctype"].retStr() == "Q" ? "Qnty" : party_data.Rows[0]["scmdisctype"].retStr() == "F" ? "Fixed" : "";
                                str += "^" + "SLDISCDESC" + "=^" + (party_data.Rows[0]["listdiscper"].retStr() + " " + party_data.Rows[0]["scmdiscrate"].retStr() + " " + scmdisctype + " " + (party_data.Rows[0]["lastbldt"].retStr() == "" ? "" : party_data.Rows[0]["lastbldt"].retStr().Remove(10))) + Cn.GCS();
                                if (VE.MENU_PARA == "PR")
                                {
                                    str = str.ReplaceHelpStr("PRCCD", "CP");
                                    str = str.ReplaceHelpStr("PRCNM", "CP");
                                }
                                if (CommVar.ClientCode(UNQSNO) == "DIWH" && (VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS"))
                                {
                                    string sql = "select a.PARGLCD,c.glnm PARGLNM from " + CommVar.CurSchema(UNQSNO) + ".t_txn a," + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr b," + CommVar.FinSchema(UNQSNO) + ".m_genleg c ";
                                    sql += "where a.autono=b.autono and a.PARGLCD=c.glcd and  a.DOCCD = '" + doccd + "' and b.LOCCD = '" + CommVar.Loccd(UNQSNO) + "' and b.COMPCD = '" + CommVar.Compcd(UNQSNO) + "' and a.slcd ='" + val + "' ";
                                    sql += "order by a.AUTONO desc ";
                                    DataTable dt = masterHelp.SQLquery(sql);
                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        str += "^" + "PARGLCD" + "=^" + dt.Rows[0]["PARGLCD"].retStr() + Cn.GCS();
                                        str += "^" + "PARGLNM" + "=^" + dt.Rows[0]["PARGLNM"].retStr() + Cn.GCS();
                                    }
                                }
                                return Content(str);
                            }
                            else
                            {
                                return Content("Invalid Ledger Code ! Please Enter a Valid Ledger Code !!");
                            }
                        }
                        else
                        {
                            return Content(str);
                        }
                    }
                    else
                    {
                        return Content(str);
                    }

                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetGodownDetails(string val, string Code)
        {
            try
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                string SkipGocd = (VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "SR") ? "'TR'" : "";
                //if ((VE.MENU_PARA == "ISS" || VE.MENU_PARA == "REC") && Code.retStr() != "")
                //{
                //    SkipGocd += SkipGocd == "" ? "" : ",";
                //    SkipGocd += Code.retSqlformat();
                //}
                var str = masterHelp.GOCD_help(val, SkipGocd);
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
        public ActionResult GetItemGroupDetails(string val, string Code)
        {
            try
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                string[] data = Code.Split(Convert.ToChar(Cn.GCS()));
                string str = masterHelp.ITGRPCD_help(val, "", data[0]);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    string glcd = "";

                    glcd = str.retCompValue(MenuDescription(VE.MENU_PARA).Rows[0]["glcd"].retStr());
                    str += "^GLCD=^" + glcd + Cn.GCS();
                    //pricegen
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {
                        string WPPRICEGEN = str.retCompValue("WPPRICEGEN");
                        string RPPRICEGEN = str.retCompValue("RPPRICEGEN");
                        string WPPER = str.retCompValue("WPPER");
                        string RPPER = str.retCompValue("RPPER");

                        if (WPPRICEGEN.retStr() == "" || RPPRICEGEN.retStr() == "" || WPPER.retDbl() == 0 || RPPER.retDbl() == 0)
                        {
                            DataTable syscnfgdata = salesfunc.GetSyscnfgData(data[1].retStr());

                            if (WPPRICEGEN.retStr() == "")
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {
                                    string grppricegenstr = "^WPPRICEGEN=^" + WPPRICEGEN + Cn.GCS();
                                    string syspricegenstr = "^WPPRICEGEN=^" + syscnfgdata.Rows[0]["wppricegen"].retStr() + Cn.GCS();
                                    str = str.Replace(grppricegenstr.retStr(), syspricegenstr.retStr());
                                }

                            }
                            if (RPPRICEGEN.retStr() == "")
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {

                                    string grppricegenstr = "^RPPRICEGEN=^" + RPPRICEGEN + Cn.GCS();
                                    string syspricegenstr = "^RPPRICEGEN=^" + syscnfgdata.Rows[0]["rppricegen"].retStr() + Cn.GCS();
                                    str = str.Replace(grppricegenstr.retStr(), syspricegenstr.retStr());
                                }

                            }
                            if (WPPER.retDbl() == 0)
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {

                                    string grppricegenstr = "^WPPER=^" + WPPER + Cn.GCS();
                                    string syspricegenstr = "^WPPER=^" + syscnfgdata.Rows[0]["WPPER"].retStr() + Cn.GCS();
                                    str = str.Replace(grppricegenstr.retStr(), syspricegenstr.retStr());
                                }

                            }
                            if (RPPER.retDbl() == 0)
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {

                                    string grppricegenstr = "^RPPER=^" + RPPER + Cn.GCS();
                                    string syspricegenstr = "^RPPER=^" + syscnfgdata.Rows[0]["RPPER"].retStr() + Cn.GCS();
                                    str = str.Replace(grppricegenstr.retStr(), syspricegenstr.retStr());
                                }

                            }
                        }
                    }

                    //
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetMaterialJobDetails(string val)
        {
            try
            {
                string str = masterHelp.MTRLJOBCD_help(val);
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
        public ActionResult GetItemDetails(string val, string Code)
        {
            try
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string ITGRPCD = data[0].retStr() == "" ? "" : data[0].retStr().retSqlformat();
                string DOCDT = data[1].retStr();
                if (DOCDT == "") { return Content("Please Select Document Date"); }
                string TAXGRPCD = data[2].retStr();
                string GOCD = data[3].retStr() == "" ? "" : data[3].retStr().retSqlformat();
                string PRCCD = data[4].retStr();
                string MTRLJOBCD = data[5].retStr() == "" ? "" : data[5].retStr().retSqlformat();
                string BARNO = data[6].retStr() == "" ? "" : data[6].retStr().retSqlformat();
                double RATE = data[7].retDbl();
                if (TAXGRPCD.retStr() == "") return Content("Please select Party !");
                var str = masterHelp.ITCD_help(val, "", data[0].retStr());
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else if (str.IndexOf(Convert.ToChar(Cn.GCS())) >= 0)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "", BARIMAGE = "";
                    string glcd = "";
                    DataTable tax_data = new DataTable();
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {
                        tax_data = salesfunc.GetBarHelp(DOCDT.retStr(), GOCD.retStr(), BARNO.retStr(), val.retStr().retSqlformat(), MTRLJOBCD.retStr(), "", ITGRPCD, "", PRCCD.retStr(), TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);

                    }
                    else
                    {
                        tax_data = salesfunc.GetStock(DOCDT.retStr(), GOCD.retStr(), BARNO.retStr(), val.retStr().retSqlformat(), MTRLJOBCD.retStr(), "", ITGRPCD, "", PRCCD.retStr(), TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);

                    }
                    if (tax_data != null && tax_data.Rows.Count > 0)
                    {
                        PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                        BARIMAGE = tax_data.Rows[0]["BARIMAGE"].retStr();
                        if (PRODGRPGSTPER != "")
                        {
                            ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, RATE);
                            if (ALL_GSTPER.retStr() != "")
                            {
                                var gst = ALL_GSTPER.Split(',').ToList();
                                GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                            }
                        }

                        glcd = tax_data.Rows[0][MenuDescription(VE.MENU_PARA).Rows[0]["glcd"].retStr()].retStr();

                    }
                    str += "^PRODGRPGSTPER=^" + PRODGRPGSTPER + Cn.GCS();
                    str += "^BARIMAGE=^" + BARIMAGE + Cn.GCS();
                    str += "^ALL_GSTPER=^" + ALL_GSTPER + Cn.GCS();
                    str += "^GSTPER=^" + GSTPER + Cn.GCS();
                    str += "^GLCD=^" + glcd + Cn.GCS();

                    return Content(str);
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
        public ActionResult GetFabItemDetails(string val, string Code)
        {
            try
            {
                var str = masterHelp.ITCD_help(val, "C");
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
        public ActionResult GetPartDetails(string val)
        {
            try
            {
                var str = masterHelp.PARTCD_help(val);
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
        public ActionResult GetColorDetails(string val)
        {
            try
            {
                var str = masterHelp.COLRCD_help(val);
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
        public ActionResult GetSizeDetails(string val)
        {
            try
            {
                var str = masterHelp.SIZECD_help(val);
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
        public ActionResult GetStockDetails(string val)
        {
            try
            {
                var str = masterHelp.STKTYPE_help(val);
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
        public ActionResult GetLocationBinDetails(string val)
        {
            try
            {
                var str = masterHelp.LOCABIN_help(val);
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
        public ActionResult GetBarCodeDetails(string val, string Code)
        {
            try
            {
                //sequence of Code variable MTRLJOBCD/PARTCD/DOCDT/TAXGRPCD/GOCD/PRCCD/ALLMTRLJOBCD/HelpFrom
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE); string str = ""; bool showonlycommonbar = true;
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                //if new barcode generate then commonbarcode=E transaction barcode comes on blure only. (For Lal Fashion) change by Neha
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                {
                    if (val.retStr() != "") { showonlycommonbar = false; }
                }
                //end
                string barnoOrStyle = val.retStr();
                string MTRLJOBCD = data[0].retSqlformat();
                string PARTCD = data[1].retStr();
                string DOCDT = data[2].retStr();
                string TAXGRPCD = data[3].retStr();
                string GOCD = data[2].retStr() == "" ? "" : data[4].retStr().retSqlformat();
                string PRCCD = data[5].retStr();
                string BARNO = data[8].retStr() == "" || val.retStr() == "" ? "" : data[8].retStr().ToUpper().retSqlformat();
                bool exactbarno = data[7].retStr() == "Bar" ? true : false;
                if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = data[6].retStr(); }
                string AUTONO = data[9].retStr() == "" ? "" : data[9].retStr().retSqlformat();
                string SLCD = "";
                if ((VE.MENU_PARA == "PR" || VE.MENU_PARA == "SR") && data[11].retStr() == "E")
                {
                    SLCD = data[10].retStr() == "" ? "" : data[10].retStr().retSqlformat();
                }

                if (data[7].retStr() == "Bar")
                {
                    barnoOrStyle = barnoOrStyle.ToUpper();
                }
                string MENU_PARA = VE.MENU_PARA;
                if (VE.MENU_PARA == "ISS")
                {
                    MENU_PARA = "SBDIR";
                }
                if (VE.MENU_PARA == "REC")
                {
                    MENU_PARA = "PB";
                }
                str = masterHelp.T_TXN_BARNO_help(barnoOrStyle, MENU_PARA, DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD, "", exactbarno, "", BARNO, AUTONO, showonlycommonbar, SLCD);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1) return Content(str);
                    if (VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "PI")
                    {
                        if (str.retCompValue("PRODGRPGSTPER").retStr() == "")
                        {
                            return Content("Please link up Product Group with Tax Rate for this Item (" + str.retCompValue("STYLENO").retStr() + "" + str.retCompValue("ITNM").retStr() + ") !!");
                        }
                    }
                    string glcd = "";
                    glcd = str.retCompValue(MenuDescription(VE.MENU_PARA).Rows[0]["glcd"].retStr());
                    str += "^GLCD=^" + glcd + Cn.GCS();

                    string tnpBARIMAGE = str.retCompValue("BARIMAGE");
                    if (tnpBARIMAGE != "")
                    {// start Image section
                        string newBarImgstr = "";
                        string barimage = tnpBARIMAGE;
                        var brimgs = barimage.Split((char)179);
                        foreach (var barimg in brimgs)
                        {//27600455_1.jpg~wew*27600455_1.jpg~wew*27600455_1.jpg~wew
                            string barfilename = barimg.Split('~')[0];
                            string barimgdesc = barimg.Split('~')[1];
                            newBarImgstr += (char)179 + CommVar.WebUploadDocURL(barfilename) + "~" + barimgdesc;
                            string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                            FROMpath = Path.Combine(FROMpath, "");
                            string TOPATH = CommVar.LocalUploadDocPath() + barfilename;
                            Cn.CopyImage(FROMpath, TOPATH);
                        }
                        newBarImgstr = newBarImgstr.TrimStart((char)179);
                        str = str.Replace(tnpBARIMAGE, newBarImgstr);
                    }// end Image section

                    //pricegen
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {
                        string WPPRICEGEN = str.retCompValue("WPPRICEGEN");
                        string RPPRICEGEN = str.retCompValue("RPPRICEGEN");
                        string WPPER = str.retCompValue("WPPER");
                        string RPPER = str.retCompValue("RPPER");
                        if (WPPRICEGEN.retStr() == "" || RPPRICEGEN.retStr() == "" || WPPER.retDbl() == 0 || RPPER.retDbl() == 0)
                        {
                            DataTable syscnfgdata = salesfunc.GetSyscnfgData(DOCDT.retStr());
                            if (WPPRICEGEN.retStr() == "")
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {
                                    string grppricegenstr = "^WPPRICEGEN=^" + WPPRICEGEN + Cn.GCS();
                                    string syspricegenstr = "^WPPRICEGEN=^" + syscnfgdata.Rows[0]["wppricegen"].retStr() + Cn.GCS();
                                    str = str.Replace(grppricegenstr.retStr(), syspricegenstr.retStr());
                                }
                            }
                            if (RPPRICEGEN.retStr() == "")
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {

                                    string grppricegenstr = "^RPPRICEGEN=^" + RPPRICEGEN + Cn.GCS();
                                    string syspricegenstr = "^RPPRICEGEN=^" + syscnfgdata.Rows[0]["rppricegen"].retStr() + Cn.GCS();
                                    str = str.Replace(grppricegenstr.retStr(), syspricegenstr.retStr());
                                }
                            }
                            if (WPPER.retDbl() == 0)
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {

                                    string grppricegenstr = "^WPPER=^" + WPPER + Cn.GCS();
                                    string syspricegenstr = "^WPPER=^" + syscnfgdata.Rows[0]["WPPER"].retStr() + Cn.GCS();
                                    str = str.Replace(grppricegenstr.retStr(), syspricegenstr.retStr());
                                }
                            }
                            if (RPPER.retDbl() == 0)
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {

                                    string grppricegenstr = "^RPPER=^" + RPPER + Cn.GCS();
                                    string syspricegenstr = "^RPPER=^" + syscnfgdata.Rows[0]["RPPER"].retStr() + Cn.GCS();
                                    str = str.Replace(grppricegenstr.retStr(), syspricegenstr.retStr());
                                }
                            }
                        }
                    }
                    //if purchase stock det
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {
                        var stock_data = salesfunc.GetStock(DOCDT, GOCD, BARNO, "", MTRLJOBCD, AUTONO, "", barnoOrStyle, PRCCD, TAXGRPCD, "", "", true, true, "", "", false, false, exactbarno, "", true);
                        if (stock_data != null && stock_data.Rows.Count > 0)
                        {
                            string grppricegenstr = "^BALQNTY=^" + str.retCompValue("BALQNTY") + Cn.GCS();
                            string syspricegenstr = "^BALQNTY=^" + stock_data.Rows[0]["balqnty"] + Cn.GCS();
                            str = str.Replace(grppricegenstr.retStr(), syspricegenstr.retStr());
                        }
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
        public ActionResult GetUOMDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.UOM_help(val));
                }
                else
                {
                    string str = masterHelp.UOM_help(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetBaleNoDetails(string val, string code)
        {
            try
            {
                var data = code.Split(Convert.ToChar(Cn.GCS()));
                string tdt = data[0].retStr();
                string gocd = data[1].retStr() == "" ? "" : data[1].retSqlformat();
                string autono = data[2].retStr();
                if (val.retStr() != "")
                {
                    string sql = "select distinct baleno||baleyr baleno from " + CommVar.CurSchema(UNQSNO) + ".T_BALE where BALENO like'%" + val + "%'  ";
                    if (autono.retStr() != "") sql += "and blautono ='" + autono + "'";
                    DataTable dt = masterHelp.SQLquery(sql);
                    if (dt.Rows.Count > 0)
                    {
                        val = (from DataRow dr in dt.Rows
                               select dr["baleno"].retStr()).ToArray().retSqlfromStrarray();
                    }
                }
                var str = masterHelp.BaleNo_help(val, tdt, gocd, "", true, true, "", true, true);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) >= 0)
                    {
                        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                        string blautono = str.retCompValue("blautono");
                        string docno = DB.T_CNTRL_HDR.Where(a => a.AUTONO == blautono).Select(b => b.DOCNO).FirstOrDefault();
                        str += "^BLDOCNO=^" + docno + Cn.GCS();
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
        //public ActionResult GetGeneralLedgerDetails(string val)
        //{
        //    try
        //    {
        //        if (val == null)
        //        {
        //            return PartialView("_Help2", masterHelp.GENERALLEDGER(val));
        //        }
        //        else
        //        {
        //            string str = masterHelp.GENERALLEDGER(val);
        //            return Content(str);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        public ActionResult GetJobDetails(string val)
        {
            try
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                var str = masterHelp.JOBCD_JOBMST_help(val, "");
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
        public ActionResult FillDetailData(TransactionSaleEntry VE)
        {
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

                string str = "";
                var ordcnt = VE.TBATCHDTL.Where(a => a.ORDAUTONO.retStr() != "").Count();
                if (ordcnt > 0)
                {
                    var orddet = (from a in VE.TBATCHDTL
                                  join b in DB.T_SORD on a.ORDAUTONO equals b.AUTONO into x
                                  from b in x.DefaultIfEmpty()
                                  join c in DB.T_CNTRL_HDR on (b != null ? b.AUTONO : "") equals c.AUTONO into y
                                  from c in y.DefaultIfEmpty()
                                  where a.ORDAUTONO.retStr() != ""
                                  select new
                                  {
                                      PREFNO = b.PREFNO.retStr() == "" ? c.DOCNO : b.PREFNO,
                                      PREFDT = b.PREFNO.retStr() == "" ? c.DOCDT : b.PREFDT,
                                  }).FirstOrDefault();
                    if (orddet != null)
                    {
                        str += "^PREFNO=^" + orddet.PREFNO + Cn.GCS();
                        str += "^PREFDT=^" + orddet.PREFDT.retDateStr() + Cn.GCS();
                    }
                }
                VE.TBATCHDTL.ForEach(x =>
                {
                    x.RATE = x.RATE.retDbl();
                    x.DISCRATE = x.DISCRATE.retDbl();
                    x.SCMDISCRATE = x.SCMDISCRATE.retDbl();
                    x.TDDISCRATE = x.TDDISCRATE.retDbl();
                    x.GSTPER = x.GSTPER.retDbl();
                    x.FLAGMTR = x.FLAGMTR.retDbl();
                    x.LISTPRICE = x.LISTPRICE.retDbl();
                    x.LISTDISCPER = x.LISTDISCPER.retDbl();
                    x.PAGENO = x.PAGENO.retInt();
                    x.PAGESLNO = x.PAGESLNO.retInt();
                    x.BLUOMCD = x.BLUOMCD.retStr();
                    x.FABITCD = x.FABITCD.retStr();
                    x.FABITNM = x.FABITNM.retStr();
                    //x.PDESIGN = x.PDESIGN.retStr();
                    x.RECPROGSLNO = x.RECPROGSLNO.retShort();
                    x.BLQNTY = x.BLQNTY.retDbl();
                    x.CONVQTYPUNIT = x.CONVQTYPUNIT.retDbl();
                });
                #region if Merge Same Item false thn blslno should not duplicate
                if (VE.MERGEINDTL == false)
                {
                    var duplicateslno = string.Join(",", VE.TBATCHDTL
                    .GroupBy(s => s.TXNSLNO)
                    .Where(g => g.Count() > 1).Select(y => y.Key).ToArray());
                    if (duplicateslno.retStr() != "")
                    {
                        return Content("if Merge Same Item is Uncheck in Main tab then Bill Sl (" + duplicateslno + ") Should not duplicate in barcode tab!");
                    }

                }
                #endregion
                VE.TTXNDTL = (from x in VE.TBATCHDTL
                              group x by new
                              {
                                  x.TXNSLNO,
                                  x.ITGRPCD,
                                  x.ITGRPNM,
                                  x.MTRLJOBCD,
                                  x.MTRLJOBNM,
                                  x.MTBARCODE,
                                  x.ITCD,
                                  x.ITSTYLE,
                                  x.STYLENO,
                                  x.DISCTYPE,
                                  x.DISCTYPE_DESC,
                                  x.TDDISCTYPE,
                                  x.TDDISCTYPE_DESC,
                                  x.SCMDISCTYPE,
                                  x.SCMDISCTYPE_DESC,
                                  x.UOM,
                                  x.STKTYPE,
                                  x.RATE,
                                  x.DISCRATE,
                                  x.SCMDISCRATE,
                                  x.TDDISCRATE,
                                  x.GSTPER,
                                  x.ALL_GSTPER,
                                  x.HSNCODE,
                                  x.PRODGRPGSTPER,
                                  x.BALENO,
                                  x.BALEYR,
                                  x.GLCD,
                                  x.ITREM,
                                  x.AGDOCNO,
                                  x.AGDOCDT,
                                  x.LISTPRICE,
                                  x.LISTDISCPER,
                                  x.PAGENO,
                                  x.PAGESLNO,
                                  x.BLUOMCD,
                                  x.FABITCD,
                                  x.FABITNM,
                                  //x.PDESIGN,
                                  x.RECPROGSLNO,
                                  x.CONVQTYPUNIT,
                                  //x.BLQNTY,
                                  x.FREESTK
                              } into P
                              select new TTXNDTL
                              {
                                  SLNO = P.Key.TXNSLNO.retShort(),
                                  ITGRPCD = P.Key.ITGRPCD,
                                  ITGRPNM = P.Key.ITGRPNM,
                                  MTRLJOBCD = P.Key.MTRLJOBCD,
                                  MTRLJOBNM = P.Key.MTRLJOBNM,
                                  //MTBARCODE = P.Key.MTBARCODE,
                                  ITCD = P.Key.ITCD,
                                  //ITNM = P.Key.STYLENO + "" + P.Key.ITNM,
                                  ITSTYLE = P.Key.ITSTYLE,
                                  STYLENO = P.Key.STYLENO,
                                  STKTYPE = P.Key.STKTYPE,
                                  UOM = P.Key.UOM,
                                  NOS = P.Sum(A => (A.UOM == "MTR" && A.NOS.retDbl() == 0) ? 1 : A.NOS),
                                  //NOS = P.Sum(A => A.NOS),
                                  QNTY = P.Sum(A => A.QNTY),
                                  BALSTOCK = P.Sum(A => A.BALSTOCK),
                                  FLAGMTR = P.Sum(A => A.FLAGMTR),
                                  BLQNTY = P.Sum(A => A.BLQNTY),
                                  //BLQNTY = P.Key.BLQNTY,
                                  RATE = P.Key.RATE,
                                  DISCTYPE = P.Key.DISCTYPE,
                                  DISCTYPE_DESC = P.Key.DISCTYPE_DESC,
                                  //DISCRATE = P.Key.DISCRATE,
                                  DISCRATE = P.Key.DISCTYPE.retStr() == "F" ? P.Sum(A => A.DISCRATE) : P.Key.DISCRATE,
                                  //TDDISCRATE = P.Key.TDDISCRATE,
                                  TDDISCRATE = P.Key.TDDISCTYPE.retStr() == "F" ? P.Sum(A => A.TDDISCRATE) : P.Key.TDDISCRATE,
                                  TDDISCTYPE = P.Key.TDDISCTYPE,
                                  TDDISCTYPE_DESC = P.Key.TDDISCTYPE_DESC,
                                  //SCMDISCRATE = P.Key.SCMDISCRATE,
                                  SCMDISCRATE = P.Key.SCMDISCTYPE.retStr() == "F" ? P.Sum(A => A.SCMDISCRATE) : P.Key.SCMDISCRATE,
                                  SCMDISCTYPE = P.Key.SCMDISCTYPE,
                                  SCMDISCTYPE_DESC = P.Key.SCMDISCTYPE_DESC,
                                  ALL_GSTPER = P.Key.ALL_GSTPER.retStr() == "" ? P.Key.GSTPER.retStr() : P.Key.ALL_GSTPER,
                                  //AMT = P.Sum(A => A.BLQNTY).retDbl() == 0 ? (P.Sum(A => A.QNTY).retDbl() - P.Sum(A => A.FLAGMTR).retDbl()) * P.Key.RATE.retDbl() : P.Sum(A => A.BLQNTY).retDbl() * P.Key.RATE.retDbl(),
                                  HSNCODE = P.Key.HSNCODE,
                                  PRODGRPGSTPER = P.Key.PRODGRPGSTPER,
                                  BALENO = P.Key.BALENO,
                                  BALEYR = P.Key.BALEYR,
                                  GLCD = P.Key.GLCD,
                                  ITREM = P.Key.ITREM,
                                  AGDOCNO = P.Key.AGDOCNO,
                                  AGDOCDT = P.Key.AGDOCDT,
                                  LISTPRICE = P.Key.LISTPRICE,
                                  LISTDISCPER = P.Key.LISTDISCPER,
                                  PAGENO = P.Key.PAGENO,
                                  PAGESLNO = P.Key.PAGESLNO,
                                  BLUOMCD = P.Key.BLUOMCD,
                                  FABITCD = P.Key.FABITCD,
                                  FABITNM = P.Key.FABITNM,
                                  RECPROGSLNO = P.Key.RECPROGSLNO,
                                  CONVQTYPUNIT = P.Key.CONVQTYPUNIT,
                                  FREESTK = P.Key.FREESTK,
                              }).OrderBy(a => a.SLNO).ToList();
                //chk duplicate slno
                var allslno = VE.TTXNDTL.Select(a => a.SLNO).Count();
                var distnctslno = VE.TTXNDTL.Select(a => a.SLNO).Distinct().Count();
                if (allslno != distnctslno)
                {
                    #region error checking
                    DataTable tbl = ListToDatatable.LINQResultToDataTable(VE.TTXNDTL);
                    #endregion

                    string slno = string.Join(",", (VE.TTXNDTL.GroupBy(x => x.SLNO)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList()));
                    //return Content(slno);
                    return Content("Bill Sl (" + slno + ") duplicate in barcode tab!");
                }

                for (int p = 0; p <= VE.TTXNDTL.Count - 1; p++)
                {
                    if (VE.TTXNDTL[p].PRODGRPGSTPER.retStr() != "")
                    {
                        var gstdata = salesfunc.retGstPer(VE.TTXNDTL[p].PRODGRPGSTPER.retStr(), VE.TTXNDTL[p].RATE.retDbl(), VE.TTXNDTL[p].SCMDISCTYPE.retStr(), VE.TTXNDTL[p].SCMDISCRATE.retDbl());
                        if (gstdata.retStr() != "")
                        {
                            var gst = gstdata.Split(',');
                            if (gst.Count() > 0)
                            {
                                VE.TTXNDTL[p].IGSTPER = gst[0].retDbl();
                                VE.TTXNDTL[p].CGSTPER = gst[1].retDbl();
                                VE.TTXNDTL[p].SGSTPER = gst[2].retDbl();
                            }

                        }
                    }
                    if (VE.T_TXN.REVCHRG == "N")
                    {
                        VE.TTXNDTL[p].IGSTPER = 0;
                        VE.TTXNDTL[p].CGSTPER = 0;
                        VE.TTXNDTL[p].SGSTPER = 0;
                    }
                }
                VE.DropDown_list1 = DISCTYPEINRATE();
                List<DropDown_list2> RETURN_TYPE = new List<DropDown_list2> {
                    new DropDown_list2 { value = "N", text = "NO"},
                    new DropDown_list2 { value = "Y", text = "YES"},
                    };
                VE.RETURN_TYPE = RETURN_TYPE;

                ModelState.Clear();
                VE.DefaultView = true;
                //return PartialView("_T_SALE_DETAIL", VE);

                var GRID_DATA = RenderRazorViewToString(ControllerContext, "_T_SALE_DETAIL", VE);
                return Content(str + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + GRID_DATA);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPackingSlip(string val, string Code)
        {
            try
            {
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string autono = data[0];
                string docdt = data[1];
                string slcd = data[2];
                var str = masterHelp.PACKSLIPAUTONO_help(val, autono, docdt, slcd);
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
        private void FreightCharges(TransactionSaleEntry VE, string AUTO_NO)
        {
            try
            {
                double A_T_CURR_AMT = 0; double A_T_AMT = 0; double A_T_TAXABLE = 0; double A_T_IGST_AMT = 0; double A_T_CGST_AMT = 0;
                double A_T_SGST_AMT = 0; double A_T_CESS_AMT = 0; double A_T_DUTY_AMT = 0; double A_T_NET_AMT = 0; double IGST_PER = 0; double CGST_PER = 0; double SGST_PER = 0;
                double CESS_PER = 0; double DUTY_PER = 0;
                string sql = "", scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
                Cn.getQueryString(VE);
                string S_P = "";
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") { S_P = "P"; } else { S_P = "S"; }

                sql = "";
                sql += "select a.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, ";
                sql += "a.amtdesc, b.glcd, a.hsncode, a.amtrate, a.curr_amt, a.amt,a.igstper, a.igstamt, ";
                sql += "a.sgstper, a.sgstamt, a.cgstper, a.cgstamt,a.cessper, a.cessamt, a.dutyper, a.dutyamt ";
                sql += "from " + scm + ".t_txnamt a, " + scm + ".m_amttype b ";
                sql += "where a.amtcd=b.amtcd(+) and b.salpur='" + S_P + "' and a.autono='" + AUTO_NO + "' ";
                sql += "union ";
                sql += "select b.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, ";
                sql += "'' amtdesc, b.glcd, b.hsncode, 0 amtrate, 0 curr_amt, 0 amt,0 igstper, 0 igstamt, ";
                sql += "0 sgstper, 0 sgstamt, 0 cgstper, 0 cgstamt, 0 cessper, 0 cessamt, 0 dutyper, 0 dutyamt ";
                sql += "from " + scm + ".m_amttype b, " + scm + ".m_cntrl_hdr c ";
                sql += "where b.m_autono=c.m_autono(+) and b.salpur='" + S_P + "'  and nvl(c.inactive_tag,'N') = 'N' ";
                sql += "and b.amtcd not in (select amtcd from " + scm + ".t_txnamt where autono='" + AUTO_NO + "')";
                var AMOUNT_DATA = masterHelp.SQLquery(sql);


                VE.TTXNAMT = (from DataRow dr in AMOUNT_DATA.Rows
                              select new TTXNAMT()
                              {
                                  AMTCD = dr["amtcd"].ToString(),
                                  ADDLESS = dr["addless"].ToString(),
                                  TAXCODE = dr["taxcode"].ToString(),
                                  GLCD = dr["GLCD"].ToString(),
                                  AMTNM = dr["amtnm"].ToString(),
                                  CALCCODE = dr["calccode"].ToString(),
                                  CALCTYPE = dr["CALCTYPE"].ToString(),
                                  CALCFORMULA = dr["calcformula"].ToString(),
                                  AMTDESC = dr["amtdesc"].ToString(),
                                  HSNCODE = dr["hsncode"].ToString(),
                                  AMTRATE = dr["amtrate"].retDbl(),
                                  CURR_AMT = dr["curr_amt"].retDbl(),
                                  AMT = dr["amt"].retDbl(),
                                  IGSTPER = dr["igstper"].retDbl(),
                                  CGSTPER = dr["cgstper"].retDbl(),
                                  SGSTPER = dr["sgstper"].retDbl(),
                                  CESSPER = dr["cessper"].retDbl(),
                                  DUTYPER = dr["dutyper"].retDbl(),
                                  IGSTAMT = dr["igstamt"].retDbl(),
                                  CGSTAMT = dr["cgstamt"].retDbl(),
                                  SGSTAMT = dr["sgstamt"].retDbl(),
                                  CESSAMT = dr["cessamt"].retDbl(),
                                  DUTYAMT = dr["dutyamt"].retDbl(),
                              }).ToList();

                for (int p = 0; p <= VE.TTXNAMT.Count - 1; p++)
                {
                    double mult = VE.TTXNAMT[p].ADDLESS == "A" ? 1 : -1;

                    VE.TTXNAMT[p].SLNO = Convert.ToInt16(p + 1);

                    VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT.retDbl() + VE.TTXNAMT[p].IGSTAMT.retDbl() + VE.TTXNAMT[p].CGSTAMT.retDbl() + VE.TTXNAMT[p].SGSTAMT.retDbl() + VE.TTXNAMT[p].CESSAMT.retDbl() + VE.TTXNAMT[p].DUTYAMT.retDbl();
                    A_T_CURR_AMT = A_T_CURR_AMT + (VE.TTXNAMT[p].CURR_AMT.retDbl() * mult);
                    A_T_AMT = A_T_AMT + (VE.TTXNAMT[p].AMT.retDbl() * mult);
                    A_T_IGST_AMT = A_T_IGST_AMT + (VE.TTXNAMT[p].IGSTAMT.retDbl() * mult);
                    A_T_CGST_AMT = A_T_CGST_AMT + (VE.TTXNAMT[p].CGSTAMT.retDbl() * mult);
                    A_T_SGST_AMT = A_T_SGST_AMT + (VE.TTXNAMT[p].SGSTAMT.retDbl() * mult);
                    A_T_CESS_AMT = A_T_CESS_AMT + (VE.TTXNAMT[p].CESSAMT.retDbl() * mult);
                    A_T_DUTY_AMT = A_T_DUTY_AMT + (VE.TTXNAMT[p].DUTYAMT.retDbl() * mult);
                }
                A_T_NET_AMT = A_T_NET_AMT + A_T_AMT + A_T_IGST_AMT + A_T_CGST_AMT + A_T_SGST_AMT + A_T_CESS_AMT + A_T_DUTY_AMT;
                VE.A_T_CURR = A_T_CURR_AMT;
                VE.A_T_AMOUNT = A_T_AMT;
                VE.A_T_IGST = A_T_IGST_AMT;
                VE.A_T_CGST = A_T_CGST_AMT;
                VE.A_T_SGST = A_T_SGST_AMT;
                VE.A_T_CESS = A_T_CESS_AMT;
                VE.A_T_DUTY = A_T_DUTY_AMT;
                VE.A_T_NET = A_T_NET_AMT;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
        }
        public ActionResult GetPendOrder(TransactionSaleEntry VE, string SLCD, string SUBMITBTN, string ITCD, string MTRLJOBCD)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            Cn.getQueryString(VE);
            DataTable dt = new DataTable();
            if (SUBMITBTN == "SHOWBTN")
            {
                VE.PENDINGORDER = (List<PENDINGORDER>)TempData["PENDORDER" + VE.MENU_PARA]; TempData.Keep();
                if (ITCD.retStr() != "" && VE.itemfilter == true)
                {
                    VE.PENDINGORDER = VE.PENDINGORDER.Where(a => a.ITCD == ITCD).ToList();
                }

                foreach (var v in VE.PENDINGORDER)
                {
                    double currentadjqnty = 0;
                    if (VE.TBATCHDTL != null)
                    {
                        currentadjqnty = VE.TBATCHDTL.Where(a => a.ORDAUTONO == v.ORDAUTONO && a.ORDSLNO.retStr() == v.ORDSLNO).Sum(b => b.QNTY).retDbl();
                    }
                    v.CURRENTADJQTY = currentadjqnty.retDbl();
                }
                VE.PENDINGORDER = VE.PENDINGORDER.Where(a => a.BALQTY - a.CURRENTADJQTY > 0).ToList();
                if (VE.PENDINGORDER != null)
                {
                    int slno = 1;
                    for (int p = 0; p <= VE.PENDINGORDER.Count - 1; p++)
                    {
                        VE.PENDINGORDER[p].SLNO = slno.retShort();
                        slno++;
                    }
                }
            }
            else
            {
                dt = salesfunc.GetPendOrder(SLCD, "", "", "", "", "", VE.MENU_PARA);
                string glcd = MenuDescription(VE.MENU_PARA).Rows[0]["glcd"].ToString();
                DataTable PRODGRPDATA = new DataTable();
                DataTable BARMASTERDATA = new DataTable();
                if (dt != null && dt.Rows.Count > 0)
                {

                    VE.PENDINGORDER = (from DataRow dr in dt.Rows
                                       select new PENDINGORDER
                                       {
                                           ORDDOCNO = dr["DOCNO"].retStr(),
                                           ORDAUTONO = dr["AUTONO"].retStr(),
                                           ORDSLNO = dr["SLNO"].retStr(),
                                           ORDDOCDT = dr["DOCDT"].retStr(),
                                           ITGRPNM = dr["ITGRPNM"].retStr(),
                                           ITSTYLE = dr["styleno"].retStr() + " " + dr["ITNM"].retStr(),
                                           COLRNM = dr["COLRNM"].retStr(),
                                           SIZECD = dr["SIZECD"].retStr(),
                                           ORDQTY = dr["ORDQNTY"].retDbl(),
                                           BALQTY = dr["BALQNTY"].retDbl(),
                                           ITCD = dr["ITCD"].retStr(),
                                           COLRCD = dr["COLRCD"].retStr(),
                                           PDESIGN = dr["PDESIGN"].retStr(),
                                           RATE = dr["RATE"].retDbl(),
                                           ITGRPCD = dr["ITGRPCD"].retStr(),
                                           BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                           PARTCD = dr["PARTCD"].retStr(),
                                           PARTNM = dr["PARTNM"].retStr(),
                                           PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                           CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                           SIZENM = dr["SIZENM"].retStr(),
                                           SZBARCODE = dr["SZBARCODE"].retStr(),
                                           UOM = dr["UOMCD"].retStr(),
                                           HSNCODE = dr["HSNCODE"].retStr(),
                                           NEGSTOCK = dr["NEGSTOCK"].retStr(),
                                       }).ToList();
                    string ITCDLIST = VE.PENDINGORDER.Select(a => a.ITCD).Distinct().ToArray().retSqlfromStrarray();
                    BARMASTERDATA = salesfunc.GetBarHelp(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retStr(), "", ITCDLIST, MTRLJOBCD.retStr(), "", "", "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);

                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {
                        PRODGRPDATA = BARMASTERDATA;
                        //PRODGRPDATA = salesfunc.GetBarHelp(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retStr(), "", ITCDLIST, MTRLJOBCD.retStr(), "", "", "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);
                    }
                    //else if (VE.MENU_PARA == "ALL")
                    //{
                    //    PRODGRPDATA = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retStr().retSqlformat(), "", ITCDLIST, MTRLJOBCD.retStr(), "", "", "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, false);
                    //}
                    else
                    {
                        PRODGRPDATA = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retStr().retSqlformat(), "", ITCDLIST, MTRLJOBCD.retStr(), "", "", "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);
                    }

                }
                if (VE.PENDINGORDER != null)
                {
                    string scm = CommVar.CurSchema(UNQSNO);
                    string mtrljobcd = "FS";
                    string mtrljobnm = DB.M_MTRLJOBMST.Find("FS").MTRLJOBNM;
                    string mtbarcode = DB.M_MTRLJOBMST.Find("FS").MTBARCODE;
                    int slno = 1;
                    DataTable syscnfgdata = salesfunc.GetSyscnfgData(VE.T_TXN.DOCDT.retStr().Remove(10));
                    string itgrpcdarr = VE.PENDINGORDER.Select(a => a.ITGRPCD).ToArray().retSqlfromStrarray();
                    string sql = "select " + glcd + ",itgrpcd from " + scm + ".m_group where itgrpcd in (" + itgrpcdarr + ") ";
                    DataTable groupdata = masterHelp.SQLquery(sql);
                    for (int p = 0; p <= VE.PENDINGORDER.Count - 1; p++)
                    {
                        VE.PENDINGORDER[p].SLNO = slno.retShort();
                        string itcd = VE.PENDINGORDER[p].ITCD.retStr();
                        string itgrpcd = VE.PENDINGORDER[p].ITGRPCD.retStr();
                        if (PRODGRPDATA != null)
                        {

                            VE.PENDINGORDER[p].PRODGRPGSTPER = PRODGRPDATA.AsEnumerable().Where(a => a.Field<string>("itcd") == itcd).Select(b => b.Field<string>("prodgrpgstper")).FirstOrDefault();
                            if (VE.PENDINGORDER[p].PRODGRPGSTPER.retStr() != "")
                            {
                                var gstper = salesfunc.retGstPer(VE.PENDINGORDER[p].PRODGRPGSTPER.retStr(), VE.PENDINGORDER[p].RATE.retDbl());
                                if (gstper.retStr() != "")
                                {
                                    VE.PENDINGORDER[p].GSTPER = gstper.Split(',').Sum(a => a.retDbl());
                                }
                            }
                            var barimage = PRODGRPDATA.AsEnumerable().Where(a => a.Field<string>("itcd") == itcd).Select(b => b.Field<string>("barimage")).FirstOrDefault();
                            if (barimage.retStr() != "")
                            {
                                VE.PENDINGORDER[p].BarImages = barimage.retStr();
                                var brimgs = VE.PENDINGORDER[p].BarImages.retStr().Split((char)179);
                                VE.PENDINGORDER[p].BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                                foreach (var barimg in brimgs)
                                {
                                    string barfilename = barimg.Split('~')[0];
                                    string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                                    FROMpath = Path.Combine(FROMpath, "");
                                    string TOPATH = System.Web.Hosting.HostingEnvironment.MapPath("~/UploadDocuments/" + barfilename);
                                    Cn.CopyImage(FROMpath, TOPATH);
                                }
                            }
                            if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                            {
                                VE.PENDINGORDER[p].WPPRICEGEN = PRODGRPDATA.AsEnumerable().Where(a => a.Field<string>("itcd") == itcd).Select(b => b.Field<string>("WPPRICEGEN")).FirstOrDefault();
                                VE.PENDINGORDER[p].RPPRICEGEN = PRODGRPDATA.AsEnumerable().Where(a => a.Field<string>("itcd") == itcd).Select(b => b.Field<string>("RPPRICEGEN")).FirstOrDefault();
                                if (VE.PENDINGORDER[p].WPPRICEGEN.retStr() == "" || VE.PENDINGORDER[p].RPPRICEGEN.retStr() == "")
                                {

                                    if (VE.PENDINGORDER[p].WPPRICEGEN.retStr() == "")
                                    {
                                        if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                        {
                                            VE.PENDINGORDER[p].WPPRICEGEN = syscnfgdata.Rows[0]["wppricegen"].retStr();
                                            VE.PENDINGORDER[p].RPPRICEGEN = syscnfgdata.Rows[0]["rppricegen"].retStr();
                                        }
                                    }
                                }
                            }
                            VE.PENDINGORDER[p].GLCD = groupdata.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == itgrpcd).Select(b => b.Field<string>(glcd)).FirstOrDefault();

                        }
                        VE.PENDINGORDER[p].MTRLJOBCD = mtrljobcd;
                        VE.PENDINGORDER[p].MTRLJOBNM = mtrljobnm;
                        VE.PENDINGORDER[p].MTBARCODE = mtbarcode;
                        if (BARMASTERDATA != null)
                        {
                            VE.PENDINGORDER[p].BARNO = BARMASTERDATA.AsEnumerable().Where(a => a.Field<string>("itcd") == itcd).Select(b => b.Field<string>("barno")).FirstOrDefault();
                        }
                        slno++;
                    }
                }
            }



            VE.DefaultView = true;
            return PartialView("_T_SALE_PENDINGORDER", VE);
        }
        public ActionResult SelectPendOrder(TransactionSaleEntry VE, string SUBMITBTN)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            Cn.getQueryString(VE);
            if (SUBMITBTN == "SHOWBTN")
            {
                var dt = (from a in VE.PENDINGORDER
                          where a.Ord_Checked == true
                          select new TBATCHDTL
                          {
                              ORDDOCNO = a.ORDDOCNO.retStr(),
                              ORDAUTONO = a.ORDAUTONO.retStr(),
                              ORDSLNO = a.ORDSLNO.retShort(),
                              ORDDOCDT = a.ORDDOCDT.retStr(),
                              ITGRPNM = a.ITGRPNM.retStr(),
                              ITGRPCD = a.ITGRPCD.retStr(),
                              ITCD = a.ITCD.retStr(),
                              ITSTYLE = a.ITSTYLE.retStr(),
                              PARTCD = a.PARTCD.retStr(),
                              PARTNM = a.PARTNM.retStr(),
                              PRTBARCODE = a.PRTBARCODE.retStr(),
                              COLRCD = a.COLRCD.retStr(),
                              COLRNM = a.COLRNM.retStr(),
                              CLRBARCODE = a.CLRBARCODE.retStr(),
                              SIZECD = a.SIZECD.retStr(),
                              SIZENM = a.SIZENM.retStr(),
                              SZBARCODE = a.SZBARCODE.retStr(),
                              QNTY = a.BALQTY.retDbl() - a.CURRENTADJQTY.retDbl(),
                              UOM = a.UOM,
                              RATE = a.RATE.retDbl(),
                              HSNCODE = a.HSNCODE,
                              PDESIGN = a.PDESIGN,
                              BARGENTYPE = a.BARGENTYPE.retStr(),
                              GLCD = a.GLCD,
                              PRODGRPGSTPER = a.PRODGRPGSTPER.retStr(),
                              GSTPER = a.GSTPER.retDbl(),
                              WPPRICEGEN = a.WPPRICEGEN.retStr(),
                              RPPRICEGEN = a.RPPRICEGEN.retStr(),
                              STKTYPE = "F",
                              BarImages = a.BarImages.retStr(),
                              BarImagesCount = a.BarImagesCount.retStr(),
                              DISCTYPE = "P",
                              DISCTYPE_DESC = "%",
                              TDDISCTYPE = "P",
                              TDDISCTYPE_DESC = "%",
                              SCMDISCTYPE = "P",
                              SCMDISCTYPE_DESC = "%",
                              NEGSTOCK = a.NEGSTOCK.retStr(),
                              BARNO = a.BARNO.retStr()
                          }).ToList();

                if (VE.TBATCHDTL != null)
                {
                    VE.TBATCHDTL.AddRange(dt);
                }
                else
                {
                    VE.TBATCHDTL = dt;
                }
                int slno = 0, txnslno = 0;
                if (VE.TBATCHDTL.Count() > 1)
                {
                    slno = Convert.ToInt32(VE.TBATCHDTL.Max(a => Convert.ToInt32(a.SLNO)));
                    txnslno = Convert.ToInt32(VE.TBATCHDTL.Max(a => Convert.ToInt32(a.TXNSLNO)));
                }
                string MTRLJOBCD = "FS";
                string MTRLJOBNM = DB.M_MTRLJOBMST.Find("FS").MTRLJOBNM;
                string MTBARCODE = DB.M_MTRLJOBMST.Find("FS").MTBARCODE;
                for (int i = 0; i <= VE.TBATCHDTL.Count() - 1; i++)
                {
                    if (VE.TBATCHDTL[i].SLNO.retDbl() == 0)
                    {
                        slno++;
                        VE.TBATCHDTL[i].SLNO = (slno).retShort();
                        VE.TBATCHDTL[i].TXNSLNO = VE.TBATCHDTL.Where(a => a.ITGRPCD.retStr() == VE.TBATCHDTL[i].ITGRPCD.retStr() &&
                             a.MTRLJOBCD.retStr() == VE.TBATCHDTL[i].MTRLJOBCD.retStr() &&
                                a.ITCD.retStr() == VE.TBATCHDTL[i].ITCD.retStr() &&
                                a.DISCTYPE.retStr() == VE.TBATCHDTL[i].DISCTYPE.retStr() &&
                                a.TDDISCTYPE.retStr() == VE.TBATCHDTL[i].TDDISCTYPE.retStr() &&
                                a.SCMDISCTYPE.retStr() == VE.TBATCHDTL[i].SCMDISCTYPE.retStr() &&
                                a.UOM.retStr() == VE.TBATCHDTL[i].UOM.retStr() &&
                                a.STKTYPE.retStr() == VE.TBATCHDTL[i].STKTYPE.retStr() &&
                                a.RATE.retStr() == VE.TBATCHDTL[i].RATE.retStr() &&
                                a.DISCRATE.retStr() == VE.TBATCHDTL[i].DISCRATE.retStr() &&
                                a.SCMDISCRATE.retStr() == VE.TBATCHDTL[i].SCMDISCRATE.retStr() &&
                                a.TDDISCRATE.retStr() == VE.TBATCHDTL[i].TDDISCRATE.retStr() &&
                                a.GSTPER.retStr() == VE.TBATCHDTL[i].GSTPER.retStr() &&
                                a.FLAGMTR.retStr() == VE.TBATCHDTL[i].FLAGMTR.retStr() &&
                                a.HSNCODE.retStr() == VE.TBATCHDTL[i].HSNCODE.retStr() &&
                                a.BALENO.retStr() == VE.TBATCHDTL[i].BALENO.retStr() &&
                                a.GLCD.retStr() == VE.TBATCHDTL[i].GLCD.retStr() &&
                                a.ITREM.retStr() == VE.TBATCHDTL[i].ITREM.retStr() &&
                                a.AGDOCNO.retStr() == VE.TBATCHDTL[i].AGDOCNO.retStr() &&
                                a.AGDOCDT.retStr() == VE.TBATCHDTL[i].AGDOCDT.retStr() &&
                                a.LISTPRICE.retStr() == VE.TBATCHDTL[i].LISTPRICE.retStr() &&
                                a.LISTDISCPER.retStr() == VE.TBATCHDTL[i].LISTDISCPER.retStr() &&
                                a.BARNO.retStr() == VE.TBATCHDTL[i].BARNO.retStr()
                           ).Select(b => b.TXNSLNO).FirstOrDefault();
                        if (VE.TBATCHDTL[i].TXNSLNO == 0)
                        {
                            txnslno++;
                            VE.TBATCHDTL[i].TXNSLNO = txnslno.retShort();

                        }
                    }
                    if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && (VE.TBATCHDTL[i].BARGENTYPE == "E" || VE.T_TXN.BARGENTYPE == "E"))
                    {
                        VE.TBATCHDTL[i].BarImages = "";
                        VE.TBATCHDTL[i].BarImagesCount = "";
                    }
                    VE.TBATCHDTL[i].MTRLJOBCD = MTRLJOBCD;
                    VE.TBATCHDTL[i].MTRLJOBNM = MTRLJOBNM;
                    VE.TBATCHDTL[i].MTBARCODE = MTBARCODE;
                }
                VE.Database_Combo4 = (from n in DB.T_BATCHDTL
                                      select new Database_Combo4() { FIELD_VALUE = n.PCSTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                VE.DefaultView = true;
                return PartialView("_T_SALE_BarTab", VE);
            }
            else
            {
                var dt = (from a in VE.PENDINGORDER
                          where a.Ord_Checked == true
                          select new PENDINGORDER
                          {
                              ORDDOCNO = a.ORDDOCNO.retStr(),
                              ORDAUTONO = a.ORDAUTONO.retStr(),
                              ORDSLNO = a.ORDSLNO.retStr(),
                              ORDDOCDT = a.ORDDOCDT.retStr(),
                              ITGRPNM = a.ITGRPNM.retStr(),
                              ITSTYLE = a.ITSTYLE.retStr(),
                              COLRNM = a.COLRNM.retStr(),
                              SIZECD = a.SIZECD.retStr(),
                              ORDQTY = a.ORDQTY.retDbl(),
                              BALQTY = a.BALQTY.retDbl(),
                              ITCD = a.ITCD.retStr(),
                              COLRCD = a.COLRCD.retStr(),
                              PDESIGN = a.PDESIGN.retStr(),
                              RATE = a.RATE.retDbl(),
                              ITGRPCD = a.ITGRPCD.retStr(),
                              BARGENTYPE = a.BARGENTYPE.retStr(),
                              PARTCD = a.PARTCD.retStr(),
                              PARTNM = a.PARTNM.retStr(),
                              PRTBARCODE = a.PRTBARCODE.retStr(),
                              CLRBARCODE = a.CLRBARCODE.retStr(),
                              SIZENM = a.SIZENM.retStr(),
                              SZBARCODE = a.SZBARCODE.retStr(),
                              UOM = a.UOM.retStr(),
                              HSNCODE = a.HSNCODE.retStr(),
                              GLCD = a.GLCD,
                              PRODGRPGSTPER = a.PRODGRPGSTPER.retStr(),
                              GSTPER = a.GSTPER.retDbl(),
                              WPPRICEGEN = a.WPPRICEGEN.retStr(),
                              RPPRICEGEN = a.RPPRICEGEN.retStr(),
                              BarImages = a.BarImages.retStr(),
                              BarImagesCount = a.BarImagesCount.retStr(),
                              MTRLJOBCD = a.MTRLJOBCD.retStr(),
                              MTRLJOBNM = a.MTRLJOBNM.retStr(),
                              MTBARCODE = a.MTBARCODE.retStr(),
                              NEGSTOCK = a.NEGSTOCK.retStr(),
                              BARNO = a.BARNO.retStr()
                          }).ToList();
                TempData["PENDORDER" + VE.MENU_PARA] = dt;
                return Content("0");
            }
        }
        public ActionResult GetOrderDetails(TransactionSaleEntry VE, string val, string Code)
        {
            try
            {
                Cn.getQueryString(VE);
                var tbl = (List<PENDINGORDER>)TempData["PENDORDER" + VE.MENU_PARA]; TempData.Keep();
                if (tbl == null || tbl.Count == 0)
                {
                    return Content("Please Select Order!");
                }
                else
                {

                    foreach (var v in tbl)
                    {
                        double currentadjqnty = 0;
                        if (VE.TBATCHDTL != null)
                        {
                            currentadjqnty = VE.TBATCHDTL.Where(a => a.ORDAUTONO == v.ORDAUTONO && a.ORDSLNO.retStr() == v.ORDSLNO).Sum(b => b.QNTY).retDbl();
                        }
                        v.CURRENTADJQTY = currentadjqnty.retDbl();
                    }
                    tbl = tbl.Where(a => a.BALQTY - a.CURRENTADJQTY > 0).ToList();

                }
                if (Code.retStr() != "")
                {
                    tbl = tbl.Where(a => a.ITCD == Code).ToList();
                }
                if (val.retStr() != "")
                {
                    tbl = tbl.Where(a => a.ORDDOCNO == val).ToList();
                }
                tbl.Select(a => new { a.ORDAUTONO, a.ORDSLNO, a.ORDDOCNO, a.ORDDOCDT }).Distinct().ToList();
                if (val.retStr() == "" || tbl.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= tbl.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + tbl[i].ORDDOCNO + "</td><td>" + tbl[i].ORDDOCDT + " </td></tr>");
                    }
                    var hdr = "Order No." + Cn.GCS() + "Order Date";
                    return PartialView("_Help2", masterHelp.Generate_help(hdr, SB.ToString()));
                }
                else
                {
                    if (tbl.Count > 0)
                    {
                        string str = masterHelp.ToReturnFieldValues(tbl);
                        return Content(str);
                    }
                    else
                    {
                        return Content("Invalid Order No ! Please Enter a Valid Order No !!");
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteRowBarno(TransactionSaleEntry VE)
        {
            try
            {
                int slno = 1;
                var list = (from x in VE.TBATCHDTL
                            where x.Checked != true
                            group x by new
                            {
                                x.ITGRPCD,
                                x.MTRLJOBCD,
                                x.ITCD,
                                x.DISCTYPE,
                                x.TDDISCTYPE,
                                x.SCMDISCTYPE,
                                x.UOM,
                                x.STKTYPE,
                                x.RATE,
                                x.DISCRATE,
                                x.SCMDISCRATE,
                                x.TDDISCRATE,
                                x.GSTPER,
                                x.FLAGMTR,
                                x.HSNCODE,
                                x.BALENO,
                                x.GLCD,
                                x.ITREM,
                                x.AGDOCNO,
                                x.AGDOCDT,
                                x.LISTPRICE,
                                x.LISTDISCPER,
                            } into P
                            select new TBATCHDTL
                            {
                                ITGRPCD = P.Key.ITGRPCD,
                                MTRLJOBCD = P.Key.MTRLJOBCD,
                                ITCD = P.Key.ITCD,
                                STKTYPE = P.Key.STKTYPE,
                                UOM = P.Key.UOM,
                                RATE = P.Key.RATE,
                                DISCTYPE = P.Key.DISCTYPE,
                                DISCRATE = P.Key.DISCRATE,
                                TDDISCRATE = P.Key.TDDISCRATE,
                                TDDISCTYPE = P.Key.TDDISCTYPE,
                                SCMDISCRATE = P.Key.SCMDISCRATE,
                                SCMDISCTYPE = P.Key.SCMDISCTYPE,
                                HSNCODE = P.Key.HSNCODE,
                                BALENO = P.Key.BALENO,
                                GLCD = P.Key.GLCD,
                                ITREM = P.Key.ITREM,
                                AGDOCNO = P.Key.AGDOCNO,
                                AGDOCDT = P.Key.AGDOCDT,
                                LISTPRICE = P.Key.LISTPRICE,
                                LISTDISCPER = P.Key.LISTDISCPER,
                                GSTPER = P.Key.GSTPER,
                                FLAGMTR = P.Key.FLAGMTR,
                            }).ToList();
                list.ForEach(x => { x.TXNSLNO = slno++.retShort(); });

                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                List<TBATCHDTL> TBATCHDTL = new List<TBATCHDTL>();
                int count = 0;
                for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                {
                    if (VE.TBATCHDTL[i].Checked == false)
                    {
                        count += 1;
                        TBATCHDTL item = new TBATCHDTL();
                        item = VE.TBATCHDTL[i];
                        if (VE.DefaultAction == "A")
                        {
                            var blslno = list.Where(a => a.ITGRPCD.retStr() == VE.TBATCHDTL[i].ITGRPCD.retStr() &&
                              a.MTRLJOBCD.retStr() == VE.TBATCHDTL[i].MTRLJOBCD.retStr() &&
                                 a.ITCD.retStr() == VE.TBATCHDTL[i].ITCD.retStr() &&
                                 a.DISCTYPE.retStr() == VE.TBATCHDTL[i].DISCTYPE.retStr() &&
                                 a.TDDISCTYPE.retStr() == VE.TBATCHDTL[i].TDDISCTYPE.retStr() &&
                                 a.SCMDISCTYPE.retStr() == VE.TBATCHDTL[i].SCMDISCTYPE.retStr() &&
                                 a.UOM.retStr() == VE.TBATCHDTL[i].UOM.retStr() &&
                                 a.STKTYPE.retStr() == VE.TBATCHDTL[i].STKTYPE.retStr() &&
                                 a.RATE.retStr() == VE.TBATCHDTL[i].RATE.retStr() &&
                                 a.DISCRATE.retStr() == VE.TBATCHDTL[i].DISCRATE.retStr() &&
                                 a.SCMDISCRATE.retStr() == VE.TBATCHDTL[i].SCMDISCRATE.retStr() &&
                                 a.TDDISCRATE.retStr() == VE.TBATCHDTL[i].TDDISCRATE.retStr() &&
                                 a.GSTPER.retStr() == VE.TBATCHDTL[i].GSTPER.retStr() &&
                                 a.FLAGMTR.retStr() == VE.TBATCHDTL[i].FLAGMTR.retStr() &&
                                 a.HSNCODE.retStr() == VE.TBATCHDTL[i].HSNCODE.retStr() &&
                                 a.BALENO.retStr() == VE.TBATCHDTL[i].BALENO.retStr() &&
                                 a.GLCD.retStr() == VE.TBATCHDTL[i].GLCD.retStr() &&
                                 a.ITREM.retStr() == VE.TBATCHDTL[i].ITREM.retStr() &&
                                 a.AGDOCNO.retStr() == VE.TBATCHDTL[i].AGDOCNO.retStr() &&
                                 a.AGDOCDT.retStr() == VE.TBATCHDTL[i].AGDOCDT.retStr() &&
                                 a.LISTPRICE.retStr() == VE.TBATCHDTL[i].LISTPRICE.retStr() &&
                                 a.LISTDISCPER.retStr() == VE.TBATCHDTL[i].LISTDISCPER.retStr()
                            ).Select(b => b.TXNSLNO).FirstOrDefault();

                            item.SLNO = Convert.ToSByte(count);
                            item.TXNSLNO = blslno;
                        }
                        var brimgs = VE.TBATCHDTL[i].BarImages.retStr().Split((char)179);
                        VE.TBATCHDTL[i].BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                        TBATCHDTL.Add(item);
                    }
                }
                VE.TBATCHDTL = TBATCHDTL;

                VE.Database_Combo4 = (from n in DB.T_BATCHDTL
                                      select new Database_Combo4() { FIELD_VALUE = n.PCSTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                List<DropDown_list2> RETURN_TYPE = new List<DropDown_list2> {
                    new DropDown_list2 { value = "N", text = "NO"},
                    new DropDown_list2 { value = "Y", text = "YES"},
                    };
                VE.RETURN_TYPE = RETURN_TYPE;

                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_BarTab", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(TransactionSaleEntry VE)
        {
            try
            {
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
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
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteDOCRow(TransactionSaleEntry VE)
        {
            try
            {
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
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
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPORTCD(string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.PORTCD_help(val));
                }
                else
                {
                    var query = (from i in DB.MS_PORTCD where i.PORTCD == val select new { PORTCD = i.PORTCD, PORTNM = i.PORTNM }).OrderBy(s => s.PORTCD).FirstOrDefault();
                    if (query != null)
                    {
                        return Content(query.PORTCD + Cn.GCS() + query.PORTNM);
                    }
                    else
                    {
                        return Content("0");
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public dynamic cancelRecords(TransactionSaleEntry VE, string par1)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                //Cn.getQueryString(VE);
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString(), par1);
                    }
                    DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();
                    transaction.Commit();
                }
                if (VE.MENU_PARA != "PI")
                {
                    using (var transaction = DB.Database.BeginTransaction())
                    {
                        DBF.Database.ExecuteSqlCommand("lock table " + CommVar.FinSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                        T_CNTRL_HDR TCH1 = new T_CNTRL_HDR();
                        if (par1 == "*#*")
                        {
                            TCH1 = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.FinSchema(UNQSNO));
                        }
                        else
                        {
                            TCH1 = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.FinSchema(UNQSNO), par1);
                        }
                        DBF.Entry(TCH1).State = System.Data.Entity.EntityState.Modified;
                        DBF.SaveChanges();
                        transaction.Commit();
                    }
                }
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        [ValidateInput(false)]
        public ActionResult ParkRecord(FormCollection FC, TransactionSaleEntry stream)
        {
            try
            {
                Cn.getQueryString(stream);
                if (stream.T_TXN.DOCCD.retStr() != "")
                {
                    stream.T_CNTRL_HDR.DOCCD = stream.T_TXN.DOCCD.retStr();
                }
                string MNUDET = stream.MENU_DETAILS;
                var menuID = MNUDET.Split('~')[0];
                var menuIndex = MNUDET.Split('~')[1];
                string ID = menuID + menuIndex + CommVar.Loccd(UNQSNO) + CommVar.Compcd(UNQSNO) + CommVar.CurSchema(UNQSNO) + "*" + DateTime.Now;
                ID = ID.Replace(" ", "_");
                string Userid = Session["UR_ID"].ToString();
                INI Handel_ini = new INI();
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JR = javaScriptSerializer.Serialize(stream);
                Handel_ini.IniWriteValue(Userid, ID, Cn.Encrypt(JR), Server.MapPath("~/Park.ini"));
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public string RetrivePark(string value)
        {
            try
            {
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionSaleEntry");
                return url;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message;
            }
        }
        public ActionResult GetRateHistoryDetails(string SLCD, string PARTYCD, string ITCD, string ITNM, string TAG, string BARNO)
        {
            try
            {
                RateHistory RH = new RateHistory();
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                string doctype = "'" + VE.DOC_CODE.retStr() + "'" + ",'SRET','PROF'";
                var DTRateHistory = salesfunc.GetRateHistory(SLCD.retStr().retSqlformat(), PARTYCD.retStr().retSqlformat(), doctype, ITCD.retStr().retSqlformat());
                DataTable dt = salesfunc.GetLastPriceFrmMaster(BARNO);
                var doctP = (from DataRow dr in DTRateHistory.Rows
                             select new RateHistoryGrid()
                             {
                                 AUTONO = dr["AUTONO"].ToString(),
                                 DOCDT = dr["DOCDT"].retDateStr(),
                                 DOCNO = dr["DOCNO"].ToString(),
                                 QNTY = dr["QNTY"].ToString(),
                                 RATE = dr["RATE"].ToString(),
                                 SLCD = dr["SLCD"].ToString(),
                                 SLNM = dr["SLNM"].ToString(),
                                 CITY = dr["CITY"].ToString(),
                                 SCMDISCTYPE = dr["scmdiscrate"].retDbl() == 0 ? "" : dr["SCMDISCTYPE"].ToString(),
                                 SCMDISCRATE = dr["scmdiscrate"].retDbl() == 0 ? "" : dr["scmdiscrate"].retStr(),
                             }).ToList();

                if (TAG == "GRID")
                {
                    ViewBag.ITEM = ITCD.retStr() == "" ? "" : ITNM + " (" + ITCD + ")";
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        ViewBag.MASTERRATE += "[" + dt.Rows[0]["effdt"].retDateStr() + " ";
                        for (int p = 0; p <= dt.Rows.Count - 1; p++)
                        {
                            if (p != 0) ViewBag.MASTERRATE += ", ";
                            ViewBag.MASTERRATE += dt.Rows[p]["prccd"].retStr() + " : " + dt.Rows[p]["rate"].retStr();
                        }
                        ViewBag.MASTERRATE += "]";
                    }
                    VE.RateHistoryGrid = doctP.Take(5).ToList();
                    ModelState.Clear();
                    return PartialView("_T_SALE_RateHistoryGrid", VE);
                }
                else
                {
                    RH.RateHistoryGrid = doctP;
                    ModelState.Clear();
                    return PartialView("_T_SALE_RateHistory", RH);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

        public ActionResult GetPaymentDetails(TransactionSaleEntry VE, string SLCD, string PARGLCD)
        {
            try
            {
                Cn.getQueryString(VE);
                var UNQSNO = Cn.getQueryStringUNQSNO();
                string scm = CommVar.CurSchema(UNQSNO);
                string scmf = CommVar.FinSchema(UNQSNO);
                var COMPCD = CommVar.Compcd(UNQSNO);
                var LOCCD = CommVar.Loccd(UNQSNO);


                string sql = "";
                sql += "select * from( " + Environment.NewLine;
                sql += "select b.DOCDT,b.DOCNO,a.T_REM,a.CHQNO,c.DOCNM,a.BANK_DT,a.AMT,a.DRCR " + Environment.NewLine;
                sql += "from " + scmf + ".T_VCH_DET a, " + scmf + ".T_CNTRL_HDR b, " + scmf + ".M_DOCTYPE c  " + Environment.NewLine;
                sql += "where a.AUTONO=b.AUTONO(+) and b.DOCCD=c.DOCCD(+) and b.compcd='" + COMPCD + "' and b.loccd='" + LOCCD + "' and nvl(b.cancel, 'N') = 'N' " + Environment.NewLine;
                sql += "and a.SLCD in('" + SLCD + "') and a.GLCD in('" + PARGLCD + "') " + Environment.NewLine;
                sql += "order by b.DOCDT desc )a " + Environment.NewLine;
                sql += "WHERE ROWNUM <= 10" + Environment.NewLine;

                DataTable tbl = masterHelp.SQLquery(sql);
                VE.PAYMENT_DETAILS = (from DataRow dr in tbl.Rows
                                      select new PAYMENT_DETAILS
                                      {
                                          DOCDT = dr["DOCDT"].retDateStr(),
                                          DOCNO = dr["DOCNO"].ToString(),
                                          REMARKS = dr["T_REM"].ToString(),
                                          CHQNO = dr["CHQNO"].ToString(),
                                          DOCTYPE = dr["DOCNM"].ToString(),
                                          DRAMT = dr["DRCR"].ToString() == "D" ? dr["AMT"].ToString() : "",
                                          CRAMT = dr["DRCR"].ToString() == "C" ? dr["AMT"].ToString() : "",

                                      }).ToList();

                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_T_SALE_PaymentDetails", VE);



            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult CheckBillNumber(TransactionSaleEntry VE, string BILL_NO, string SUPPLIER, string AUTO_NO)
        {
            Cn.getQueryString(VE);
            if (VE.DefaultAction == "A") AUTO_NO = "";
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM_CD = CommVar.Compcd(UNQSNO);
            string YR_CD = CommVar.YearCode(UNQSNO);
            DateTime FinStartDate = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO));
            DateTime FinEndDate = Convert.ToDateTime(CommVar.FinEndDate(UNQSNO));
            string DOCTAG = MenuDescription(VE.MENU_PARA).Rows[0]["DOCTAG"].ToString();
            if (BILL_NO.retStr() == "") return Content("0");

            var query = (from c in DB.T_TXN
                         join d in DB.T_CNTRL_HDR on c.AUTONO equals d.AUTONO
                         where (c.PREFNO == BILL_NO && c.SLCD == SUPPLIER && c.AUTONO != AUTO_NO && d.COMPCD == COM_CD
                         && d.YR_CD == YR_CD && d.DOCDT >= FinStartDate && d.DOCDT <= FinEndDate //for sachi saree same billno already used in opening stock so add logic yr and finyr dt
                         && c.DOCTAG == DOCTAG
                         )
                         select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult GetOSBillNumberDetails(string val, string Code)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var arr = Code.Split(Convert.ToChar(Cn.GCS()));
            var slcd = arr[0]; string blautono = "";
            if (arr.Length > 1) blautono = arr[1];
            if (val.retStr() == "")
            {
                return PartialView("_Help2", masterHelp.OS_Bill_help(val, "", slcd));
            }
            else
            {
                string str = masterHelp.OS_Bill_help(val, "", slcd, blautono);
                return Content(str);
            }
        }

        public ActionResult GetTTXNDTLDetails(TransactionSaleEntry VE, string FDT, string TDT, string R_DOCNO, string R_BARNO, string TAXGRPCD, string SLCD, string datachng)
        {
            DataTable dt = new DataTable();
            //dt = (DataTable)TempData["TXNDTLDetails" + VE.MENU_PARA]; TempData.Keep();
            string scm_prevyr = "", scmf_prevyr = "";
            if (datachng == "Y" || dt == null || dt.Rows.Count == 0)
            {
                Cn.getQueryString(VE); string scm = CommVar.CurSchema(UNQSNO);
                string doctag = VE.MENU_PARA.retStr() == "SR" ? "SB" : VE.MENU_PARA.retStr() == "PJBR" ? "JB" : "PB";
                string retdoctag = VE.MENU_PARA.retStr() == "SR" ? "SR" : VE.MENU_PARA.retStr() == "PJBR" ? "JQ" : "PR";
                string stkdrcr = VE.MENU_PARA.retStr() == "SR" ? "'C'" : "";

                sql += "select a.TXNSLNO,a.ITGRPCD,a.ITGRPNM,a.BARGENTYPE,a.MTRLJOBCD,a.MTRLJOBNM,a.MTBARCODE,a.ITCD,a.ITNM,a.UOMCD,a.STYLENO,a.PARTCD,a.PARTNM, ";
                sql += Environment.NewLine + "a.PRTBARCODE,a.STKTYPE,a.STKNAME,a.BARNO,a.COLRCD,a.COLRNM,a.CLRBARCODE,a.SIZECD,a.SIZENM,a.SZBARCODE,a.SHADE,a.QNTY,a.NOS,a.RATE,a.DISCRATE, ";
                sql += Environment.NewLine + "a.DISCTYPE,a.TDDISCRATE,a.TDDISCTYPE,a.SCMDISCTYPE,a.SCMDISCRATE,a.HSNCODE,a.BALENO,a.PDESIGN,a.OURDESIGN,a.FLAGMTR,a.LOCABIN,a.BALEYR ";
                sql += Environment.NewLine + ",a.SALGLCD,a.PURGLCD,a.SALRETGLCD,a.PURRETGLCD,a.WPRATE,a.RPRATE,a.ITREM,a.RPPRICEGEN,a.DOCNO,a.DOCDT,a.WPPER,a.RPPER, ";
                sql += Environment.NewLine + "a.WPPRICEGEN,a.LISTPRICE,a.LISTDISCPER,a.CUTLENGTH,a.PAGENO,a.PAGESLNO,a.PCSTYPE,a.AUTONO,a.PREFNO,a.PREFDT,a.GLCD,a.GSTPER,a.prodgrpgstper,a.barimage,a.barimagecount,a.FABITCD,a.FABITNM,a.BLQNTY,a.CONVQTYPUNIT,a.BLUOMCD,a.NEGSTOCK,a.agslcd,a.sagslcd,a.balqnty,a.balnos,a.balcutlength,a.BLSLNO,a.BLTYPE from ( ";


                sql += "select a.TXNSLNO,a.ITGRPCD,a.ITGRPNM,a.BARGENTYPE,a.MTRLJOBCD,a.MTRLJOBNM,a.MTBARCODE,a.ITCD,a.ITNM,a.UOMCD,a.STYLENO,a.PARTCD,a.PARTNM, ";
                sql += Environment.NewLine + "a.PRTBARCODE,a.STKTYPE,a.STKNAME,a.BARNO,a.COLRCD,a.COLRNM,a.CLRBARCODE,a.SIZECD,a.SIZENM,a.SZBARCODE,a.SHADE,nvl(a.QNTY,0)qnty,a.NOS,a.RATE,a.DISCRATE, ";
                sql += Environment.NewLine + "a.DISCTYPE,a.TDDISCRATE,a.TDDISCTYPE,a.SCMDISCTYPE,a.SCMDISCRATE,a.HSNCODE,a.BALENO,a.PDESIGN,a.OURDESIGN,a.FLAGMTR,a.LOCABIN,a.BALEYR ";
                sql += Environment.NewLine + ",a.SALGLCD,a.PURGLCD,a.SALRETGLCD,a.PURRETGLCD,a.WPRATE,a.RPRATE,a.ITREM,a.RPPRICEGEN,a.DOCNO,a.DOCDT,a.WPPER,a.RPPER, ";
                sql += Environment.NewLine + "a.WPPRICEGEN,a.LISTPRICE,a.LISTDISCPER,a.CUTLENGTH,a.PAGENO,a.PAGESLNO,a.PCSTYPE,a.AUTONO,a.PREFNO,a.PREFDT,a.GLCD,a.GSTPER,a.prodgrpgstper,a.barimage,a.barimagecount,a.FABITCD,a.FABITNM,a.BLQNTY,a.CONVQTYPUNIT,a.BLUOMCD,a.NEGSTOCK,a.agslcd,a.sagslcd,(nvl(a.qnty,0)-nvl(b.qnty,0))balqnty,(nvl(a.nos,0)-nvl(b.nos,0))balnos,(nvl(a.CUTLENGTH,0)-nvl(b.CUTLENGTH,0))balcutlength,a.BLSLNO,a.BLTYPE from ( ";

                sql += Environment.NewLine + "select x.TXNSLNO,x.ITGRPCD,x.ITGRPNM,x.BARGENTYPE,x.MTRLJOBCD,x.MTRLJOBNM,x.MTBARCODE,x.ITCD,x.ITNM,x.UOMCD,x.STYLENO,x.PARTCD,x.PARTNM, ";
                sql += Environment.NewLine + "x.PRTBARCODE,x.STKTYPE,x.STKNAME,x.BARNO,x.COLRCD,x.COLRNM,x.CLRBARCODE,x.SIZECD,x.SIZENM,x.SZBARCODE,x.SHADE,sum(nvl(x.QNTY,0))qnty,sum(nvl(x.NOS,0))nos,sum(nvl(x.CUTLENGTH,0))CUTLENGTH,x.RATE,x.DISCRATE, ";
                sql += Environment.NewLine + "x.DISCTYPE,x.TDDISCRATE,x.TDDISCTYPE,x.SCMDISCTYPE,nvl(x.SCMDISCRATE,0)SCMDISCRATE,x.HSNCODE,x.BALENO,x.PDESIGN,x.OURDESIGN,x.FLAGMTR,x.LOCABIN,x.BALEYR ";
                sql += Environment.NewLine + ",x.SALGLCD,x.PURGLCD,x.SALRETGLCD,x.PURRETGLCD,x.WPRATE,x.RPRATE,x.ITREM,x.RPPRICEGEN,X.DOCNO,X.DOCDT,x.WPPER,x.RPPER, ";
                sql += Environment.NewLine + "x.WPPRICEGEN,x.LISTPRICE,x.LISTDISCPER,x.PAGENO,x.PAGESLNO,x.PCSTYPE,x.AUTONO,x.PREFNO,X.PREFDT,x.GLCD,x.GSTPER,y.prodgrpgstper,z.barimage,z.barimagecount,x.FABITCD,x.FABITNM,sum(x.BLQNTY)BLQNTY,x.CONVQTYPUNIT,x.BLUOMCD,x.NEGSTOCK,x.agslcd,x.sagslcd,x.BLSLNO,x.BLTYPE from";

                sql += Environment.NewLine + "(select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.prodgrpcd,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, ";
                sql += Environment.NewLine + "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,nvl(i.QNTY,0)qnty,nvl(i.NOS,0)nos,nvl(i.CUTLENGTH,0)CUTLENGTH,i.RATE,i.DISCRATE, ";
                sql += Environment.NewLine + "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                sql += Environment.NewLine + ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,n.RPPRICEGEN,(s.IGSTPER+s.CGSTPER+s.SGSTPER) GSTPER, ";
                sql += Environment.NewLine + "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,s.PAGENO,s.PAGESLNO,i.PCSTYPE,r.docno,t.docdt,r.AUTONO,t.PREFNO,t.PREFDT,s.GLCD,n.WPPER,n.RPPER ";
                sql += Environment.NewLine + ",j.FABITCD,u.ITNM FABITNM,i.BLQNTY,k.CONVQTYPUNIT,s.BLUOMCD,nvl(k.NEGSTOCK,n.NEGSTOCK)NEGSTOCK,v.agslcd,v.sagslcd,w.BLSLNO,v.BLTYPE ";
                //sql Environment.NewLine++= "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,s.PAGENO,s.PAGESLNO,i.PCSTYPE,t.docno,t.docdt,r.AUTONO,t.PREFNO,t.PREFDT,s.GLCD,n.WPPER,n.RPPER ";
                sql += Environment.NewLine + "from " + scm + ".T_BATCHDTL i, " + scm + ".T_BATCHMST j, " + scm + ".M_SITEM k, " + scm + ".M_SIZE l, " + scm + ".M_COLOR m, ";
                sql += Environment.NewLine + scm + ".M_GROUP n," + scm + ".M_MTRLJOBMST o," + scm + ".M_PARTS p," + scm + ".M_STKTYPE q," + scm + ".T_CNTRL_HDR r ";
                sql += Environment.NewLine + "," + scm + ".T_TXNDTL s," + scm + ".T_TXN t, " + scm + ".M_SITEM u," + scm + ".T_TXNOTH v, " + scm + ".T_BALE w ";
                sql += Environment.NewLine + "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                sql += Environment.NewLine + "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) and i.AUTONO=r.AUTONO(+)and t.autono=v.autono(+) ";
                sql += Environment.NewLine + "and i.autono=s.autono and i.txnslno=s.slno and s.autono=t.autono and j.fabitcd=u.itcd(+) and i.autono=w.autono(+) and i.txnslno=w.slno(+) and i.baleno=w.baleno(+) ";
                sql += Environment.NewLine + "and t.doctag in('" + doctag + "')  ";
                if (stkdrcr.retStr() != "") sql += Environment.NewLine + "and s.stkdrcr in (" + stkdrcr + ") ";
                if (R_DOCNO.retStr() != "") sql += Environment.NewLine + " and " + ((VE.MENU_PARA.retStr() == "SR" || VE.MENU_PARA.retStr() == "PJBR") ? "r.doconlyno in(" + R_DOCNO + ") " : "t.prefno in('" + R_DOCNO + "') ");
                if (FDT.retDateStr() != "") sql += Environment.NewLine + "and " + ((VE.MENU_PARA.retStr() == "SR" || VE.MENU_PARA.retStr() == "PJBR") ? "r.docdt >= to_date('" + FDT + "', 'dd/mm/yyyy') " : "t.PREFDT >= to_date('" + FDT + "', 'dd/mm/yyyy') ");
                if (TDT.retDateStr() != "") sql += Environment.NewLine + " and " + ((VE.MENU_PARA.retStr() == "SR" || VE.MENU_PARA.retStr() == "PJBR") ? "r.docdt <= to_date('" + TDT + "', 'dd/mm/yyyy')  " : "t.PREFDT <= to_date('" + TDT + "', 'dd/mm/yyyy') ");
                if (R_BARNO.retStr() != "") sql += Environment.NewLine + "and i.barno = '" + R_BARNO + "' ";
                if (SLCD.retStr() != "") sql += Environment.NewLine + "and t.slcd = '" + SLCD + "' ";
                if (VE.T_TXN.GOCD.retStr() != "") sql += Environment.NewLine + "and t.gocd = '" + VE.T_TXN.GOCD + "' ";

                sql += ")x, ";

                sql += "(select a.prodgrpcd, ";
                //sql += "listagg(b.fromrt||chr(181)||b.tort||chr(181)||b.igstper||chr(181)||b.cgstper||chr(181)||b.sgstper,chr(179)) ";
                sql += Environment.NewLine + "listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179)) ";
                sql += Environment.NewLine + "within group (order by a.prodgrpcd) as prodgrpgstper ";
                sql += Environment.NewLine + "from ";
                sql += Environment.NewLine + "(select prodgrpcd, effdt from ";
                sql += Environment.NewLine + "(select a.prodgrpcd, a.effdt, ";
                sql += Environment.NewLine + "row_number() over (partition by a.prodgrpcd order by a.effdt desc) as rn ";
                sql += Environment.NewLine + "from " + scm + ".m_prodtax a ";
                if (TDT.retDateStr() != "") sql += Environment.NewLine + "where a.effdt <= to_date('" + TDT + "','dd/mm/yyyy')  ";
                sql += Environment.NewLine + ")where rn=1 ) a, " + scm + ".m_prodtax b ";
                sql += Environment.NewLine + "where a.prodgrpcd=b.prodgrpcd(+) and a.effdt=b.effdt(+) and b.taxgrpcd='" + TAXGRPCD + "' ";
                sql += Environment.NewLine + "group by a.prodgrpcd ) y, ";

                sql += Environment.NewLine + "(select a.barno, count(*) barimagecount,";
                sql += Environment.NewLine + "listagg(a.doc_flname||'~'||a.doc_desc,chr(179)) ";
                sql += Environment.NewLine + "within group (order by a.barno) as barimage from ";
                sql += Environment.NewLine + "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
                sql += Environment.NewLine + "(select a.barno, a.barno imgbarno, a.slno imgslno ";
                sql += Environment.NewLine + "from " + scm + ".t_batch_img_hdr a ";
                sql += Environment.NewLine + "union ";
                sql += Environment.NewLine + "select a.barno, b.barno imgbarno, b.slno imgslno ";
                sql += Environment.NewLine + "from " + scm + ".t_batch_img_hdr_link a, " + scm + ".t_batch_img_hdr b ";
                sql += Environment.NewLine + "where a.mainbarno=b.barno(+) ) a, ";
                sql += Environment.NewLine + "" + scm + ".t_batch_img_hdr b ";
                sql += Environment.NewLine + "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ) a ";
                sql += Environment.NewLine + "group by a.barno ) z ";

                sql += "where x.prodgrpcd=y.prodgrpcd(+) and x.barno=z.barno(+) ";

                sql += Environment.NewLine + "group by x.TXNSLNO,x.ITGRPCD,x.ITGRPNM,x.BARGENTYPE,x.MTRLJOBCD,x.MTRLJOBNM,x.MTBARCODE,x.ITCD,x.ITNM,x.UOMCD,x.STYLENO,x.PARTCD,x.PARTNM, ";
                sql += Environment.NewLine + "x.PRTBARCODE,x.STKTYPE,x.STKNAME,x.BARNO,x.COLRCD,x.COLRNM,x.CLRBARCODE,x.SIZECD,x.SIZENM,x.SZBARCODE,x.SHADE,x.RATE,x.DISCRATE, ";
                sql += Environment.NewLine + "x.DISCTYPE,x.TDDISCRATE,x.TDDISCTYPE,x.SCMDISCTYPE,nvl(x.SCMDISCRATE,0),x.HSNCODE,x.BALENO,x.PDESIGN,x.OURDESIGN,x.FLAGMTR,x.LOCABIN,x.BALEYR ";
                sql += Environment.NewLine + ",x.SALGLCD,x.PURGLCD,x.SALRETGLCD,x.PURRETGLCD,x.WPRATE,x.RPRATE,x.ITREM,x.RPPRICEGEN,X.DOCNO,X.DOCDT,x.WPPER,x.RPPER, ";
                sql += Environment.NewLine + "x.WPPRICEGEN,x.LISTPRICE,x.LISTDISCPER,x.CUTLENGTH,x.PAGENO,x.PAGESLNO,x.PCSTYPE,x.AUTONO,x.PREFNO,X.PREFDT,x.GLCD,x.GSTPER,y.prodgrpgstper,z.barimage,z.barimagecount,x.FABITCD,x.FABITNM,x.CONVQTYPUNIT,x.BLUOMCD,x.NEGSTOCK,x.agslcd,x.sagslcd,x.BLSLNO,x.BLTYPE ";

                #region lastyr data
                for (int a = 0; a < 2; a++)
                {
                    scm_prevyr = ""; scmf_prevyr = "";
                    if (a == 0)
                    {
                        scm_prevyr = CommVar.LastYearSchema(UNQSNO);
                        scmf_prevyr = CommVar.FinSchemaPrevYr(UNQSNO);
                    }
                    else
                    {
                        scm_prevyr = salesfunc.PrevSchema(CommVar.LastYearSchema(UNQSNO));
                        scmf_prevyr = salesfunc.PrevSchema(CommVar.FinSchemaPrevYr(UNQSNO), "FIN_COMPANY");
                    }
                    if (scm_prevyr.retStr() != "")
                    {
                        sql += Environment.NewLine + "union all ";
                        sql += Environment.NewLine + "select x.TXNSLNO,x.ITGRPCD,x.ITGRPNM,x.BARGENTYPE,x.MTRLJOBCD,x.MTRLJOBNM,x.MTBARCODE,x.ITCD,x.ITNM,x.UOMCD,x.STYLENO,x.PARTCD,x.PARTNM, ";
                        sql += Environment.NewLine + "x.PRTBARCODE,x.STKTYPE,x.STKNAME,x.BARNO,x.COLRCD,x.COLRNM,x.CLRBARCODE,x.SIZECD,x.SIZENM,x.SZBARCODE,x.SHADE,sum(nvl(x.QNTY,0))qnty,sum(nvl(x.NOS,0))nos,sum(nvl(x.CUTLENGTH,0))CUTLENGTH,x.RATE,x.DISCRATE, ";
                        sql += Environment.NewLine + "x.DISCTYPE,x.TDDISCRATE,x.TDDISCTYPE,x.SCMDISCTYPE,nvl(x.SCMDISCRATE,0)SCMDISCRATE,x.HSNCODE,x.BALENO,x.PDESIGN,x.OURDESIGN,x.FLAGMTR,x.LOCABIN,x.BALEYR ";
                        sql += Environment.NewLine + ",x.SALGLCD,x.PURGLCD,x.SALRETGLCD,x.PURRETGLCD,x.WPRATE,x.RPRATE,x.ITREM,x.RPPRICEGEN,X.DOCNO,X.DOCDT,x.WPPER,x.RPPER, ";
                        sql += Environment.NewLine + "x.WPPRICEGEN,x.LISTPRICE,x.LISTDISCPER,x.PAGENO,x.PAGESLNO,x.PCSTYPE,x.AUTONO,x.PREFNO,X.PREFDT,x.GLCD,x.GSTPER,y.prodgrpgstper,z.barimage,z.barimagecount,x.FABITCD,x.FABITNM,sum(x.BLQNTY)BLQNTY,x.CONVQTYPUNIT,x.BLUOMCD,x.NEGSTOCK,x.agslcd,x.sagslcd,x.BLSLNO,x.BLTYPE from";

                        sql += Environment.NewLine + "(select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.prodgrpcd,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, ";
                        sql += Environment.NewLine + "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,nvl(i.QNTY,0)qnty,nvl(i.NOS,0)nos,nvl(i.CUTLENGTH,0)CUTLENGTH,i.RATE,i.DISCRATE, ";
                        sql += Environment.NewLine + "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                        sql += Environment.NewLine + ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,n.RPPRICEGEN,(s.IGSTPER+s.CGSTPER+s.SGSTPER) GSTPER, ";
                        sql += Environment.NewLine + "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,s.PAGENO,s.PAGESLNO,i.PCSTYPE,r.docno,t.docdt,r.AUTONO,t.PREFNO,t.PREFDT,s.GLCD,n.WPPER,n.RPPER ";
                        sql += Environment.NewLine + ",j.FABITCD,u.ITNM FABITNM,i.BLQNTY,k.CONVQTYPUNIT,s.BLUOMCD,nvl(k.NEGSTOCK,n.NEGSTOCK)NEGSTOCK,v.agslcd,v.sagslcd,w.BLSLNO,v.BLTYPE ";
                        //sql += "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,s.PAGENO,s.PAGESLNO,i.PCSTYPE,t.docno,t.docdt,r.AUTONO,t.PREFNO,t.PREFDT,s.GLCD,n.WPPER,n.RPPER ";
                        sql += Environment.NewLine + "from " + scm_prevyr + ".T_BATCHDTL i, " + scm_prevyr + ".T_BATCHMST j, " + scm_prevyr + ".M_SITEM k, " + scm_prevyr + ".M_SIZE l, " + scm_prevyr + ".M_COLOR m, ";
                        sql += Environment.NewLine + scm_prevyr + ".M_GROUP n," + scm_prevyr + ".M_MTRLJOBMST o," + scm_prevyr + ".M_PARTS p," + scm_prevyr + ".M_STKTYPE q," + scm_prevyr + ".T_CNTRL_HDR r ";
                        sql += Environment.NewLine + "," + scm_prevyr + ".T_TXNDTL s," + scm_prevyr + ".T_TXN t, " + scm_prevyr + ".M_SITEM u," + scm_prevyr + ".T_TXNOTH v, " + scm_prevyr + ".T_BALE w ";
                        sql += Environment.NewLine + "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                        sql += Environment.NewLine + "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) and i.AUTONO=r.AUTONO(+) ";
                        sql += Environment.NewLine + "and i.autono=s.autono and i.txnslno=s.slno and s.autono=t.autono and j.fabitcd=u.itcd(+)and t.autono=v.autono(+) and i.autono=w.autono(+) and i.txnslno=w.slno(+) and i.baleno=w.baleno(+) ";
                        sql += Environment.NewLine + "and t.doctag in('" + doctag + "') ";
                        if (stkdrcr.retStr() != "") sql += Environment.NewLine + "and s.stkdrcr in (" + stkdrcr + ") ";
                        if (R_DOCNO.retStr() != "") sql += Environment.NewLine + " and " + ((VE.MENU_PARA.retStr() == "SR" || VE.MENU_PARA.retStr() == "PJBR") ? "r.doconlyno in(" + R_DOCNO + ") " : "t.prefno in('" + R_DOCNO + "') ");
                        //if (FDT.retDateStr() != "") sql += "and " + ((VE.MENU_PARA.retStr() == "SR" || VE.MENU_PARA.retStr() == "PJBR") ? "r.docdt >= to_date('" + FDT + "', 'dd/mm/yyyy') " : "t.PREFDT >= to_date('" + FDT + "', 'dd/mm/yyyy') ");
                        if (TDT.retDateStr() != "") sql += Environment.NewLine + " and " + ((VE.MENU_PARA.retStr() == "SR" || VE.MENU_PARA.retStr() == "PJBR") ? "r.docdt <= to_date('" + TDT + "', 'dd/mm/yyyy')  " : "t.PREFDT <= to_date('" + TDT + "', 'dd/mm/yyyy') ");
                        if (R_BARNO.retStr() != "") sql += Environment.NewLine + "and i.barno = '" + R_BARNO + "' ";
                        if (SLCD.retStr() != "") sql += Environment.NewLine + "and t.slcd = '" + SLCD + "' ";
                        if (VE.T_TXN.GOCD.retStr() != "") sql += Environment.NewLine + "and t.gocd = '" + VE.T_TXN.GOCD + "' ";
                        sql += Environment.NewLine + ")x, ";

                        sql += Environment.NewLine + "(select a.prodgrpcd, ";
                        //sql Environment.NewLine++= "listagg(b.fromrt||chr(181)||b.tort||chr(181)||b.igstper||chr(181)||b.cgstper||chr(181)||b.sgstper,chr(179)) ";
                        sql += Environment.NewLine + "listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179)) ";
                        sql += Environment.NewLine + "within group (order by a.prodgrpcd) as prodgrpgstper ";
                        sql += Environment.NewLine + "from ";
                        sql += Environment.NewLine + "(select prodgrpcd, effdt from ";
                        sql += Environment.NewLine + "(select a.prodgrpcd, a.effdt, ";
                        sql += Environment.NewLine + "row_number() over (partition by a.prodgrpcd order by a.effdt desc) as rn ";
                        sql += Environment.NewLine + "from " + scm_prevyr + ".m_prodtax a ";
                        if (TDT.retDateStr() != "") sql += Environment.NewLine + "where a.effdt <= to_date('" + TDT + "','dd/mm/yyyy')  ";
                        sql += Environment.NewLine + ")where rn=1 ) a, " + scm_prevyr + ".m_prodtax b ";
                        sql += Environment.NewLine + "where a.prodgrpcd=b.prodgrpcd(+) and a.effdt=b.effdt(+) and b.taxgrpcd='" + TAXGRPCD + "' ";
                        sql += Environment.NewLine + "group by a.prodgrpcd ) y, ";

                        sql += Environment.NewLine + "(select a.barno, count(*) barimagecount,";
                        sql += Environment.NewLine + "listagg(a.doc_flname||'~'||a.doc_desc,chr(179)) ";
                        sql += Environment.NewLine + "within group (order by a.barno) as barimage from ";
                        sql += Environment.NewLine + "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
                        sql += Environment.NewLine + "(select a.barno, a.barno imgbarno, a.slno imgslno ";
                        sql += Environment.NewLine + "from " + scm_prevyr + ".t_batch_img_hdr a ";
                        sql += Environment.NewLine + "union ";
                        sql += Environment.NewLine + "select a.barno, b.barno imgbarno, b.slno imgslno ";
                        sql += Environment.NewLine + "from " + scm_prevyr + ".t_batch_img_hdr_link a, " + scm_prevyr + ".t_batch_img_hdr b ";
                        sql += Environment.NewLine + "where a.mainbarno=b.barno(+) ) a, ";
                        sql += Environment.NewLine + "" + scm_prevyr + ".t_batch_img_hdr b ";
                        sql += Environment.NewLine + "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ) a ";
                        sql += Environment.NewLine + "group by a.barno ) z ";

                        sql += Environment.NewLine + "where x.prodgrpcd=y.prodgrpcd(+) and x.barno=z.barno(+) ";

                        sql += Environment.NewLine + "group by x.TXNSLNO,x.ITGRPCD,x.ITGRPNM,x.BARGENTYPE,x.MTRLJOBCD,x.MTRLJOBNM,x.MTBARCODE,x.ITCD,x.ITNM,x.UOMCD,x.STYLENO,x.PARTCD,x.PARTNM, ";
                        sql += Environment.NewLine + "x.PRTBARCODE,x.STKTYPE,x.STKNAME,x.BARNO,x.COLRCD,x.COLRNM,x.CLRBARCODE,x.SIZECD,x.SIZENM,x.SZBARCODE,x.SHADE,x.RATE,x.DISCRATE, ";
                        sql += Environment.NewLine + "x.DISCTYPE,x.TDDISCRATE,x.TDDISCTYPE,x.SCMDISCTYPE,nvl(x.SCMDISCRATE,0),x.HSNCODE,x.BALENO,x.PDESIGN,x.OURDESIGN,x.FLAGMTR,x.LOCABIN,x.BALEYR ";
                        sql += Environment.NewLine + ",x.SALGLCD,x.PURGLCD,x.SALRETGLCD,x.PURRETGLCD,x.WPRATE,x.RPRATE,x.ITREM,x.RPPRICEGEN,X.DOCNO,X.DOCDT,x.WPPER,x.RPPER, ";
                        sql += Environment.NewLine + "x.WPPRICEGEN,x.LISTPRICE,x.LISTDISCPER,x.CUTLENGTH,x.PAGENO,x.PAGESLNO,x.PCSTYPE,x.AUTONO,x.PREFNO,X.PREFDT,x.GLCD,x.GSTPER,y.prodgrpgstper,z.barimage,z.barimagecount,x.FABITCD,x.FABITNM,x.CONVQTYPUNIT,x.BLUOMCD,x.NEGSTOCK,x.agslcd,x.sagslcd,x.BLSLNO,x.BLTYPE ";

                    }
                }
                #endregion
                //sql += ") a order by a.docdt, a.docno,a.txnslno ";


                sql += ") a, ";

                //get return data
                sql += Environment.NewLine + "(select sum(nvl(a.qnty,0))qnty,sum(nvl(a.nos,0))nos,sum(nvl(a.CUTLENGTH,0))CUTLENGTH,a.AGDOCNO,a.AGDOCDT,a.itcd,a.barno from(";
                for (int a = 0; a <= 2; a++)
                {
                    scm_prevyr = scm;
                    if (a == 1)
                    {
                        scm_prevyr = CommVar.LastYearSchema(UNQSNO);
                    }
                    else if (a == 2)
                    {
                        scm_prevyr = salesfunc.PrevSchema(CommVar.LastYearSchema(UNQSNO));
                    }
                    if (scm_prevyr != "")
                    {
                        if (a != 0)
                        {
                            sql += Environment.NewLine + "union all";
                        }
                        sql += Environment.NewLine + "select nvl(a.qnty,0)qnty,nvl(a.nos,0)nos,nvl(b.CUTLENGTH,0)CUTLENGTH,a.AGDOCNO,a.AGDOCDT,a.itcd,b.barno from ";
                        sql += Environment.NewLine + scm_prevyr + ".t_txndtl a," + scm_prevyr + ".T_BATCHDTL b ," + scm_prevyr + ".t_txn c," + scm_prevyr + ".t_cntrl_hdr d ";
                        sql += Environment.NewLine + "where a.autono=b.autono and a.slno=b.txnslno and a.autono=c.autono and c.autono=d.autono(+) and nvl(d.cancel,'N')='N' and c.doctag in('" + retdoctag + "') ";

                    }
                }
                sql += Environment.NewLine + " )a ";
                sql += Environment.NewLine + "group by a.AGDOCNO,a.AGDOCDT,a.itcd,a.barno )b ";
                sql += Environment.NewLine + " where a.docno=b.AGDOCNO(+) and a.docdt=b.AGDOCDT(+) and a.itcd=b.itcd(+) and a.barno=b.barno(+))a   ";
                sql += "where balqnty>0 ";
                sql += "order by a.docdt, a.docno,a.txnslno ";

                dt = masterHelp.SQLquery(sql);

                TempData["TXNDTLDetails" + VE.MENU_PARA] = dt;
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                VE.TTXNDTLPOPUP = (from r1 in dt.AsEnumerable()
                                   group r1 by new
                                   {
                                       AUTONO = r1["autono"].retStr(),
                                       AGDOCNO = (VE.MENU_PARA.retStr() == "SR" || VE.MENU_PARA.retStr() == "PJBR") ? r1["docno"].retStr() : r1["PREFNO"].retStr(),
                                       AGDOCDT = (VE.MENU_PARA.retStr() == "SR" || VE.MENU_PARA.retStr() == "PJBR") ? r1["docdt"].retStr() : r1["PREFDT"].retStr(),
                                       BARNO = r1["barno"].retStr(),
                                       ITCD = r1["itcd"].retStr(),
                                       ITSTYLE = r1["styleno"].retStr() + " " + r1["itnm"].retStr(),
                                       RATE = r1["rate"].retDbl(),
                                       SCMDISCTYPE_DESC = r1["scmdisctype"].retStr(),
                                       SCMDISCRATE = r1["scmdiscrate"].retDbl(),
                                   } into g
                                   select new TTXNDTLPOPUP
                                   {
                                       AUTONO = g.Key.AUTONO,
                                       AGDOCNO = g.Key.AGDOCNO,
                                       AGDOCDT = g.Key.AGDOCDT,
                                       BARNO = g.Key.BARNO,
                                       ITCD = g.Key.ITCD,
                                       ITSTYLE = g.Key.ITSTYLE,
                                       //QNTY = g.Sum(x => x["QNTY"].retDbl()),
                                       QNTY = g.Sum(x => x["balQNTY"].retDbl()),
                                       RATE = g.Key.RATE,
                                       SCMDISCTYPE_DESC = g.Key.SCMDISCTYPE_DESC.retStr() == "P" ? "%" : g.Key.SCMDISCTYPE_DESC.retStr() == "N" ? "Nos" : g.Key.SCMDISCTYPE_DESC.retStr() == "Q" ? "Qnty" : "Fixed",
                                       SCMDISCRATE = g.Key.SCMDISCRATE,
                                   }).ToList();

                int slno = 0;
                for (int p = 0; p <= VE.TTXNDTLPOPUP.Count - 1; p++)
                {
                    slno++;
                    VE.TTXNDTLPOPUP[p].SLNO = slno.retShort();
                    VE.TTXNDTLPOPUP[p].AGDOCDT = VE.TTXNDTLPOPUP[p].AGDOCDT.retStr() == "" ? "" : VE.TTXNDTLPOPUP[p].AGDOCDT.retStr().Remove(10);
                }

            }


            VE.DefaultView = true;
            return PartialView("_T_SALE_POPUP", VE);
        }
        public ActionResult SelectTTXNDTLDetails(TransactionSaleEntry VE)
        {
            try
            {
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                DataTable dt = (DataTable)TempData["TXNDTLDetails" + VE.MENU_PARA]; //TempData.Keep();
                TempData["TXNDTLDetails" + VE.MENU_PARA] = null;
                var selectedautoslno = VE.TTXNDTLPOPUP.Where(r => r.P_Checked == true).Select(a => a.AUTONO + a.BARNO).Distinct().ToList();

                var TBATCHDTL = (from DataRow dr in dt.Rows
                                 where selectedautoslno.Contains(dr["autono"].retStr() + dr["barno"].retStr())
                                 select new TBATCHDTL
                                 {
                                     //SLNO = dr["SLNO"].retShort(),
                                     TXNSLNO = dr["TXNSLNO"].retShort(),
                                     ITGRPCD = dr["ITGRPCD"].retStr(),
                                     ITGRPNM = dr["ITGRPNM"].retStr(),
                                     MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                     MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                     MTBARCODE = dr["MTBARCODE"].retStr(),
                                     ITCD = dr["ITCD"].retStr(),
                                     ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                     UOM = dr["UOMCD"].retStr(),
                                     STYLENO = dr["STYLENO"].retStr(),
                                     PARTCD = dr["PARTCD"].retStr(),
                                     PARTNM = dr["PARTNM"].retStr(),
                                     PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                     COLRCD = dr["COLRCD"].retStr(),
                                     COLRNM = dr["COLRNM"].retStr(),
                                     CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                     SIZECD = dr["SIZECD"].retStr(),
                                     SIZENM = dr["SIZENM"].retStr(),
                                     SZBARCODE = dr["SZBARCODE"].retStr(),
                                     SHADE = dr["SHADE"].retStr(),
                                     QNTY = dr["balQNTY"].retDbl(),
                                     NOS = dr["NOS"].retDbl(),
                                     RATE = dr["RATE"].retDbl(),
                                     DISCRATE = dr["DISCRATE"].retDbl(),
                                     DISCTYPE = dr["DISCTYPE"].retStr(),
                                     DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : dr["DISCTYPE"].retStr() == "A" ? "AftDsc%" : dr["DISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                     TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                     TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : dr["TDDISCTYPE"].retStr() == "A" ? "AftDsc%" : dr["TDDISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                     TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                     SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : dr["SCMDISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                     SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                     SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                     STKTYPE = dr["STKTYPE"].retStr(),
                                     STKNAME = dr["STKNAME"].retStr(),
                                     BARNO = dr["BARNO"].retStr(),
                                     HSNCODE = dr["HSNCODE"].retStr(),
                                     BALENO = dr["BALENO"].retStr(),
                                     PDESIGN = dr["PDESIGN"].retStr(),
                                     OURDESIGN = dr["OURDESIGN"].retStr(),
                                     FLAGMTR = dr["FLAGMTR"].retDbl(),
                                     LOCABIN = dr["LOCABIN"].retStr(),
                                     BALEYR = dr["BALEYR"].retStr(),
                                     BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                     GLCD = dr[MenuDescription(VE.MENU_PARA).Rows[0]["glcd"].retStr()].retStr(),
                                     ITREM = dr["ITREM"].retStr(),
                                     AGDOCNO = (VE.MENU_PARA.retStr() == "SR" || VE.MENU_PARA.retStr() == "PJBR") ? dr["docno"].retStr() : dr["PREFNO"].retStr(),
                                     AGDOCDT = Convert.ToDateTime((VE.MENU_PARA.retStr() == "SR" || VE.MENU_PARA.retStr() == "PJBR") ? dr["docdt"].retStr() : dr["PREFDT"].retStr()),
                                     LISTPRICE = dr["LISTPRICE"].retDbl(),
                                     LISTDISCPER = dr["LISTDISCPER"].retDbl(),
                                     CUTLENGTH = dr["balcutlength"].retDbl(),
                                     PAGENO = dr["PAGENO"].retInt(),
                                     PAGESLNO = dr["PAGESLNO"].retInt(),
                                     PCSTYPE = dr["PCSTYPE"].retStr(),
                                     BarImages = dr["BarImage"].retStr(),
                                     BarImagesCount = dr["barimagecount"].retStr(),
                                     PRODGRPGSTPER = dr["PRODGRPGSTPER"].retStr(),
                                     GSTPER = dr["GSTPER"].retDbl(),
                                     BLUOMCD = dr["CONVQTYPUNIT"].retStr() + " " + dr["BLUOMCD"].retStr(),
                                     BLQNTY = dr["BLQNTY"].retDbl(),
                                     CONVQTYPUNIT = dr["CONVQTYPUNIT"].retDbl(),
                                     FABITCD = dr["FABITCD"].retStr(),
                                     FABITNM = dr["FABITNM"].retStr(),
                                     NEGSTOCK = dr["NEGSTOCK"].retStr(),
                                     RECPROGSLNO = dr["BLSLNO"].retShort(),
                                 }).ToList();
                if (VE.TBATCHDTL == null)
                {
                    VE.TBATCHDTL = TBATCHDTL;
                }
                else
                {
                    VE.TBATCHDTL.AddRange(TBATCHDTL);
                }
                string BLTYPE = "";
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (CommVar.ClientCode(UNQSNO) == "BNBH")
                    {
                        BLTYPE = (from DataRow dr in dt.Rows where dr["bltype"].retStr() != "" orderby dr["docdt"].retStr() select dr["bltype"].retStr()).FirstOrDefault();
                    }
                }
                string str = "";
                string AGSLCD = (from DataRow dr in dt.Rows where selectedautoslno.Contains(dr["autono"].retStr() + dr["barno"].retStr()) select dr["agslcd"].retStr()).FirstOrDefault();
                string SAGSLCD = (from DataRow dr in dt.Rows where selectedautoslno.Contains(dr["autono"].retStr() + dr["barno"].retStr()) select dr["sagslcd"].retStr()).FirstOrDefault();
                string AGSLNM = AGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == AGSLCD).Select(b => b.SLNM).FirstOrDefault();
                string SAGSLNM = SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == SAGSLCD).Select(b => b.SLNM).FirstOrDefault();

                str += "^AGSLCD=^" + AGSLCD + Cn.GCS();
                str += "^SAGSLCD=^" + SAGSLCD + Cn.GCS();
                str += "^AGSLNM=^" + AGSLNM + Cn.GCS();
                str += "^SAGSLNM=^" + SAGSLNM + Cn.GCS();
                str += "^BLTYPE=^" + BLTYPE + Cn.GCS();

                //fill stock in t_batchdtl
                string BARNO = (from a in VE.TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
                string ITCD = (from a in VE.TBATCHDTL select a.ITCD).ToArray().retSqlfromStrarray();
                //string MTRLJOBCD = (from a in VE.TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                string ITGRPCD = (from a in VE.TBATCHDTL select a.ITGRPCD).ToArray().retSqlfromStrarray();
                string autono = VE.T_TXN.AUTONO.retStr() == "" ? "" : VE.T_TXN.AUTONO.retStr().retSqlformat();
                var data = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retStr().retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", autono, ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);

                if (VE.TBATCHDTL != null)
                {
                    for (int i = 0; i < VE.TBATCHDTL.Count; i++)
                    {
                        VE.TBATCHDTL[i].SLNO = (i + 1).retShort();

                        if (data != null && data.Rows.Count > 0)
                        {
                            var DATA = data.Select("barno = '" + VE.TBATCHDTL[i].BARNO + "' and itcd= '" + VE.TBATCHDTL[i].ITCD + "' and itgrpcd = '" + VE.TBATCHDTL[i].ITGRPCD + "'");
                            if (DATA.Count() > 0)
                            {
                                DataTable stock_data = DATA.CopyToDataTable();
                                VE.TBATCHDTL[i].BALSTOCK = stock_data.Rows[0]["BALQNTY"].retDbl();
                            }
                        }
                    }
                }
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                VE.Database_Combo4 = (from n in DB.T_BATCHDTL
                                      select new Database_Combo4() { FIELD_VALUE = n.PCSTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                ModelState.Clear();
                VE.DefaultView = true;
                var GRID_DATA = RenderRazorViewToString(ControllerContext, "_T_SALE_BarTab", VE);
                return Content(str + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + GRID_DATA);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
            //VE.DefaultView = true;
            //ModelState.Clear();
            //return PartialView("_T_SALE_BarTab", VE);

        }
        public ActionResult GetDOCNO_PI(string val, string code)
        {
            var str = masterHelp.DOCNO_SALPUR_help(val);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public string PCSTYPE_BIND(List<Database_Combo4> PCSTYPE)
        {
            string str = "";
            foreach (var v in PCSTYPE)
            {
                str += "<option>" + v.FIELD_VALUE.retStr() + "</option>";
            }
            return str;

        }
        public ActionResult Update_PagenoSlno(TransactionSaleEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string sql = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    var schnm = CommVar.CurSchema(UNQSNO);
                    var CLCD = CommVar.ClientCode(UNQSNO);

                    for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                    {
                        sql = "update " + schnm + ". T_TXNDTL set PAGENO='" + VE.TTXNDTL[i].PAGENO + "',PAGESLNO='" + VE.TTXNDTL[i].PAGESLNO + "' "
                                 + " where AUTONO='" + VE.T_TXN.AUTONO + "' and SLNO='" + VE.TTXNDTL[i].SLNO.retStr() + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }

                    ModelState.Clear();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    ContentFlg = "1";
                    return Content(ContentFlg);
                }
                catch (Exception ex)
                {
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        public ActionResult Update_Transport(TransactionSaleEntry VE)
        {
            Cn.getQueryString(VE);
            string ContentFlg = ChildRecordCheck(VE.T_TXN.AUTONO, "Y");
            if (ContentFlg != "") return Content(ContentFlg);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string sql = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    ContentFlg = "";
                    var schnm = CommVar.CurSchema(UNQSNO);
                    var fschnm = CommVar.FinSchema(UNQSNO);
                    var CLCD = CommVar.ClientCode(UNQSNO);

                    //update to t_txntrans//
                    sql = "update " + schnm + ". t_txntrans set TRANSLCD='" + VE.T_TXNTRANS.TRANSLCD + "',TRANSMODE='" + VE.T_TXNTRANS.TRANSMODE + "', ";
                    sql += " CRSLCD ='" + VE.T_TXNTRANS.CRSLCD + "',EWAYBILLNO ='" + VE.T_TXNTRANS.EWAYBILLNO + "',LRNO ='" + VE.T_TXNTRANS.LRNO + "', ";
                    sql += " LRDT =to_date('" + VE.T_TXNTRANS.LRDT.retDateStr() + "', 'dd/mm/yyyy'),LORRYNO ='" + VE.T_TXNTRANS.LORRYNO + "',GRWT ='" + VE.T_TXNTRANS.GRWT + "', ";
                    sql += " TRWT ='" + VE.T_TXNTRANS.TRWT + "',NTWT ='" + VE.T_TXNTRANS.NTWT + "',DESTN ='" + VE.T_TXNTRANS.DESTN + "', ";
                    sql += " RECVPERSON ='" + VE.T_TXNTRANS.RECVPERSON + "',VECHLTYPE ='" + VE.T_TXNTRANS.VECHLTYPE + "',GATEENTNO ='" + VE.T_TXNTRANS.GATEENTNO + "',ACTFRGHTPAID ='" + VE.T_TXNTRANS.ACTFRGHTPAID + "' "
                         + " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    //update to T_TXNOTH//
                    sql = "update " + schnm + ". T_TXNOTH set CASENOS='" + VE.T_TXNOTH.CASENOS + "', NOOFCASES='" + VE.T_TXNOTH.NOOFCASES + "', MUTSLCD='" + VE.T_TXNOTH.MUTSLCD + "', TOPAY='" + VE.T_TXNOTH.TOPAY + "', NOOFCASES_REM ='" + VE.T_TXNOTH.NOOFCASES_REM + "' ";
                    sql += " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    //update to T_TXNEWB//
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    var txnweb_data = DB1.T_TXNEWB.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.AUTONO).ToList();
                    if (txnweb_data != null && txnweb_data.Count > 0)
                    {
                        sql = "update " + fschnm + ". T_TXNEWB set TRANSLCD='" + VE.T_TXNTRANS.TRANSLCD + "',TRANSMODE='" + VE.T_TXNTRANS.TRANSMODE + "', ";
                        sql += " EWAYBILLNO ='" + VE.T_TXNTRANS.EWAYBILLNO + "',LRNO ='" + VE.T_TXNTRANS.LRNO + "', ";
                        sql += " LRDT =to_date('" + VE.T_TXNTRANS.LRDT.retDateStr() + "', 'dd/mm/yyyy'),LORRYNO ='" + VE.T_TXNTRANS.LORRYNO + "', ";
                        sql += " VECHLTYPE ='" + VE.T_TXNTRANS.VECHLTYPE + "'  "
                             + " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }
                    //update to T_BALE_HDR//
                    int balenocount = 0;
                    if (VE.TTXNDTL != null && VE.TTXNDTL.Count > 0)
                    {
                        balenocount = VE.TTXNDTL.Where(a => a.BALENO.retStr() != "").Count();
                    }
                    if (balenocount > 0)
                    {
                        sql = "update " + schnm + ". T_BALE_HDR set MUTSLCD='" + VE.T_TXNTRANS.TRANSLCD + "' ";
                        sql += " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }

                    //update to T_VCH_BL//
                    var tvchbl_data = DB1.T_VCH_BL.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.AUTONO).ToList();
                    if (tvchbl_data != null && tvchbl_data.Count > 0)
                    {
                        sql = "update " + fschnm + ". T_VCH_BL set LRNO ='" + VE.T_TXNTRANS.LRNO + "',LRDT =to_date('" + VE.T_TXNTRANS.LRDT.retDateStr() + "', 'dd/mm/yyyy'), ";
                        sql += " TRANSNM ='" + VE.TRANSLNM + "' ";
                        sql += " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }
                    ModelState.Clear();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    ContentFlg = "1";
                    return Content(ContentFlg);
                }
                catch (Exception ex)
                {
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        public ActionResult Update_Agent(TransactionSaleEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string sql = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    var schnm = CommVar.CurSchema(UNQSNO);
                    var fschnm = CommVar.FinSchema(UNQSNO);
                    var CLCD = CommVar.ClientCode(UNQSNO);

                    //update to T_TXN//
                    sql = "update " + schnm + ". T_TXN set PREFNO='" + VE.T_TXN.PREFNO + "', PREFDT=to_date('" + VE.T_TXN.PREFDT.retDateStr() + "', 'dd/mm/yyyy') ";
                    sql += " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    //update to t_vch_gst//
                    sql = "update " + fschnm + ". t_vch_gst set BLNO='" + VE.T_TXN.PREFNO + "', BLDT=to_date('" + VE.T_TXN.PREFDT.retDateStr() + "', 'dd/mm/yyyy') ";
                    sql += " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    //update to t_vch_bl//
                    sql = "update " + fschnm + ". t_vch_bl set BLNO='" + VE.T_TXN.PREFNO + "', BLDT=to_date('" + VE.T_TXN.PREFDT.retDateStr() + "', 'dd/mm/yyyy') ";
                    sql += " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    if (VE.Last_PREFNO.retStr() != "")
                    {
                        sql = "update " + fschnm + ". t_vch_det set T_REM=replace(replace(T_REM, '" + VE.Last_PREFNO + "', '" + VE.T_TXN.PREFNO + "'),'" + VE.Last_PREFDT + "','" + VE.T_TXN.PREFDT.retDateStr() + "') ";
                        sql += " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }
                    //update to T_TXNOTH//
                    sql = "update " + schnm + ". T_TXNOTH set AGSLCD='" + VE.T_TXNOTH.AGSLCD + "', SAGSLCD='" + VE.T_TXNOTH.SAGSLCD + "' ";
                    sql += " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    //update to T_VCH_BL//
                    var tvchbl_data = DBF.T_VCH_BL.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.AUTONO).ToList();
                    if (tvchbl_data != null && tvchbl_data.Count > 0)
                    {
                        sql = "update " + fschnm + ". T_VCH_BL set AGSLCD ='" + VE.T_TXNOTH.AGSLCD + "', SAGSLCD='" + VE.T_TXNOTH.SAGSLCD + "' ";
                        sql += " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }
                    ModelState.Clear();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    ContentFlg = "1";
                    return Content(ContentFlg);
                }
                catch (Exception ex)
                {
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        public ActionResult GetPendingIssueData(TransactionSaleEntry VE)
        {
            try
            {
                Cn.getQueryString(VE);
                var UNQSNO = Cn.getQueryStringUNQSNO();
                string scm = CommVar.CurSchema(UNQSNO);
                string scmf = CommVar.FinSchema(UNQSNO);
                var COMPCD = CommVar.Compcd(UNQSNO);
                var LOCCD = CommVar.Loccd(UNQSNO);
                string sql = "";
                sql += "select b.DOCNO,a.DOCDT,a.SLCD,b.AUTONO ,c.SLNM from " + scm + ".T_TXN a," + scm + ".T_CNTRL_HDR b,  ";
                sql += scmf + ".M_SUBLEG c where a.AUTONO=b.AUTONO(+) and a.SLCD=c.SLCD(+)  and a.doctag ='JI' and b.compcd='" + COMPCD + "' and b.loccd='" + LOCCD + "' ";
                sql += "and a.autono not in (select distinct linkautono from " + scm + ".T_TXN_LINKNO   ";
                if (VE.T_TXN.AUTONO.retStr() != "") sql += "where autono not in ('" + VE.T_TXN.AUTONO + "')  ";
                sql += " )order by a.DOCNO,a.DOCDT ";
                DataTable tbl = masterHelp.SQLquery(sql);
                VE.PENDING_ISSUE = (from DataRow dr in tbl.Rows
                                    select new PENDING_ISSUE
                                    {
                                        ISSUEAUTONO = dr["autono"].ToString(),
                                        ISSUEDOCNO = dr["docno"].ToString(),
                                        ISSUEDOCDT = dr["docdt"] == null ? "" : dr["docdt"].ToString().Substring(0, 10),
                                    }).DistinctBy(a => a.ISSUEDOCNO).ToList();

                short SL_NO = 0; VE.PENDING_ISSUE.Where(A => A != null && VE.PENDING_ISSUE.Count > 0).ForEach(a => a.SLNO = ++SL_NO);
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_T_SALE_PendingIssue", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SelectPendingIssueData(TransactionSaleEntry VE)
        {
            try
            {
                Cn.getQueryString(VE);
                var UNQSNO = Cn.getQueryStringUNQSNO();
                string scm = CommVar.CurSchema(UNQSNO);
                string scmf = CommVar.FinSchema(UNQSNO);
                var COMPCD = CommVar.Compcd(UNQSNO);
                var LOCCD = CommVar.Loccd(UNQSNO);
                string ISSAUTONO = VE.PENDING_ISSUE.Where(a => a.Checked == true).Select(b => b.ISSUEAUTONO).ToArray().retSqlfromStrarray();
                string Scm = CommVar.CurSchema(UNQSNO);
                string str1 = "";
                DataTable tbl = new DataTable();

                str1 += "select i.AUTONO,i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, ";
                str1 += "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,sum(nvl(i.QNTY,0))QNTY,sum(nvl(i.NOS,0))NOS,i.RATE,i.DISCRATE, ";
                str1 += "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,sum(nvl(i.FLAGMTR,0))FLAGMTR,i.LOCABIN,i.BALEYR ";
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,i.ORDAUTONO,i.ORDSLNO,r.DOCNO ORDDOCNO,r.DOCDT ORDDOCDT,n.RPPRICEGEN, ";
                str1 += "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,nvl(k.NEGSTOCK,n.NEGSTOCK)NEGSTOCK ";
                str1 += ",s.AGDOCNO,s.AGDOCDT,s.PAGENO,s.PAGESLNO,i.PCSTYPE,s.glcd,s.BLUOMCD,j.COMMONUNIQBAR,j.FABITCD,t.ITNM FABITNM,n.WPPER,n.RPPER ";
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".M_STKTYPE q," + Scm + ".T_CNTRL_HDR r ";
                str1 += "," + Scm + ".T_TXNDTL s, " + Scm + ".M_SITEM t ";
                str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) and i.ORDAUTONO=r.AUTONO(+) and j.fabitcd=t.itcd(+) ";
                str1 += "and i.autono=s.autono and i.txnslno=s.slno ";
                str1 += "and i.AUTONO in (" + ISSAUTONO + ") ";
                str1 += "group by i.AUTONO,i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, ";
                str1 += "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.RATE,i.DISCRATE, ";
                str1 += "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.LOCABIN,i.BALEYR ";
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,i.ORDAUTONO,i.ORDSLNO,r.DOCNO,r.DOCDT,n.RPPRICEGEN, ";
                str1 += "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,nvl(k.NEGSTOCK,n.NEGSTOCK) ";
                str1 += ",s.AGDOCNO,s.AGDOCDT,s.PAGENO,s.PAGESLNO,i.PCSTYPE,s.glcd,s.BLUOMCD,j.COMMONUNIQBAR,j.FABITCD,t.ITNM,n.WPPER,n.RPPER ";
                str1 += "order by k.ITGRPCD ,i.MTRLJOBCD,k.ITCD ,i.DISCTYPE,i.TDDISCTYPE,i.SCMDISCTYPE,k.UOMCD ,i.STKTYPE ,i.RATE,i.DISCRATE,i.SCMDISCRATE,i.TDDISCRATE,i.HSNCODE ,s.GLCD,j.FABITCD ,j.PDESIGN  ";
                tbl = masterHelp.SQLquery(str1);

                VE.TBATCHDTL = (from DataRow dr in tbl.Rows
                                select new TBATCHDTL()
                                {
                                    SLNO = dr["SLNO"].retShort(),
                                    TXNSLNO = dr["TXNSLNO"].retShort(),
                                    ITGRPCD = dr["ITGRPCD"].retStr(),
                                    ITGRPNM = dr["ITGRPNM"].retStr(),
                                    MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                    MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                    MTBARCODE = dr["MTBARCODE"].retStr(),
                                    ITCD = dr["ITCD"].retStr(),
                                    ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                    UOM = dr["UOMCD"].retStr(),
                                    STYLENO = dr["STYLENO"].retStr(),
                                    PARTCD = dr["PARTCD"].retStr(),
                                    PARTNM = dr["PARTNM"].retStr(),
                                    PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                    COLRCD = dr["COLRCD"].retStr(),
                                    COLRNM = dr["COLRNM"].retStr(),
                                    CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                    SIZECD = dr["SIZECD"].retStr(),
                                    SIZENM = dr["SIZENM"].retStr(),
                                    SZBARCODE = dr["SZBARCODE"].retStr(),
                                    SHADE = dr["SHADE"].retStr(),
                                    QNTY = dr["QNTY"].retDbl(),
                                    NOS = dr["NOS"].retDbl(),
                                    RATE = dr["RATE"].retDbl(),
                                    DISCRATE = dr["DISCRATE"].retDbl(),
                                    DISCTYPE = dr["DISCTYPE"].retStr(),
                                    DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : dr["DISCTYPE"].retStr() == "A" ? "AftDsc%" : dr["DISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                    TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                    TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : dr["TDDISCTYPE"].retStr() == "A" ? "AftDsc%" : dr["TDDISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                    TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                    SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : dr["SCMDISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                    SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                    SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                    STKTYPE = dr["STKTYPE"].retStr(),
                                    STKNAME = dr["STKNAME"].retStr(),
                                    BARNO = dr["BARNO"].retStr(),
                                    HSNCODE = (VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR") ? VE.JOBHSNCODE : dr["HSNCODE"].retStr(),
                                    BALENO = dr["BALENO"].retStr(),
                                    PDESIGN = dr["PDESIGN"].retStr(),
                                    OURDESIGN = dr["OURDESIGN"].retStr(),
                                    FLAGMTR = dr["FLAGMTR"].retDbl(),
                                    LOCABIN = dr["LOCABIN"].retStr(),
                                    BALEYR = dr["BALEYR"].retStr(),
                                    BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                    GLCD = (VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR") ? VE.JOBEXPGLCD : dr[MenuDescription(VE.MENU_PARA).Rows[0]["glcd"].retStr()].retStr(),
                                    WPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPRATE"].retDbl() : (double?)null,
                                    RPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPRATE"].retDbl() : (double?)null,
                                    ITREM = dr["ITREM"].retStr(),
                                    ORDAUTONO = dr["ORDAUTONO"].retStr(),
                                    ORDSLNO = dr["ORDSLNO"].retStr() == "" ? (short?)null : dr["ORDSLNO"].retShort(),
                                    ORDDOCNO = dr["ORDDOCNO"].retStr(),
                                    ORDDOCDT = dr["ORDDOCDT"].retStr() == "" ? "" : dr["ORDDOCDT"].retStr().Remove(10),
                                    WPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPPRICEGEN"].retStr() : "",
                                    RPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPPRICEGEN"].retStr() : "",
                                    AGDOCNO = (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") ? dr["AGDOCNO"].retStr() : "",
                                    AGDOCDT = (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") ? (dr["AGDOCDT"].retStr() == "" ? (DateTime?)null : Convert.ToDateTime(dr["AGDOCDT"].retDateStr())) : (DateTime?)null,
                                    LISTPRICE = dr["LISTPRICE"].retDbl(),
                                    LISTDISCPER = dr["LISTDISCPER"].retDbl(),
                                    CUTLENGTH = dr["CUTLENGTH"].retDbl(),
                                    PAGENO = dr["PAGENO"].retInt(),
                                    PAGESLNO = dr["PAGESLNO"].retInt(),
                                    PCSTYPE = dr["PCSTYPE"].retStr(),
                                    NEGSTOCK = dr["NEGSTOCK"].retStr(),
                                    BLUOMCD = dr["BLUOMCD"].retStr(),
                                    COMMONUNIQBAR = dr["COMMONUNIQBAR"].retStr(),
                                    FABITCD = dr["FABITCD"].retStr(),
                                    FABITNM = dr["FABITNM"].retStr(),
                                    WPPER = dr["WPPER"].retDbl(),
                                    RPPER = dr["RPPER"].retDbl(),
                                }).ToList();
                int j = 0;
                string ITGRPCD = "", MTRLJOBCD = "", ITCD = "", DISCTYPE = "", TDDISCTYPE = "", SCMDISCTYPE = "", UOM = "",
                    STKTYPE = "", GLCD = "", FABITCD = "",
                    PDESIGN = "", HSNCODE = "", PRODGRPGSTPER = "";
                double RATE = 0, DISCRATE = 0, SCMDISCRATE = 0, TDDISCRATE = 0, GSTPER = 0;
                int TXNSLNO = 1, SLNO = 1;
                while (j <= VE.TBATCHDTL.Count - 1)
                {
                    ITGRPCD = VE.TBATCHDTL[j].ITGRPCD.retStr();
                    MTRLJOBCD = VE.TBATCHDTL[j].MTRLJOBCD.retStr();
                    ITCD = VE.TBATCHDTL[j].ITCD.retStr();
                    DISCTYPE = VE.TBATCHDTL[j].DISCTYPE.retStr();
                    TDDISCTYPE = VE.TBATCHDTL[j].TDDISCTYPE.retStr();
                    SCMDISCTYPE = VE.TBATCHDTL[j].SCMDISCTYPE.retStr();
                    UOM = VE.TBATCHDTL[j].UOM.retStr();
                    STKTYPE = VE.TBATCHDTL[j].STKTYPE.retStr();
                    RATE = VE.TBATCHDTL[j].RATE.retDbl();
                    DISCRATE = VE.TBATCHDTL[j].DISCRATE.retDbl();
                    SCMDISCRATE = VE.TBATCHDTL[j].SCMDISCRATE.retDbl();
                    TDDISCRATE = VE.TBATCHDTL[j].TDDISCRATE.retDbl();
                    GSTPER = VE.TBATCHDTL[j].GSTPER.retDbl();
                    HSNCODE = VE.TBATCHDTL[j].HSNCODE.retStr();
                    PRODGRPGSTPER = VE.TBATCHDTL[j].PRODGRPGSTPER.retStr();
                    GLCD = VE.TBATCHDTL[j].GLCD.retStr();
                    FABITCD = VE.TBATCHDTL[j].FABITCD.retStr();
                    PDESIGN = VE.TBATCHDTL[j].PDESIGN.retStr();


                    while (ITGRPCD == VE.TBATCHDTL[j].ITGRPCD.retStr() && MTRLJOBCD == VE.TBATCHDTL[j].MTRLJOBCD.retStr()
                        && ITCD == VE.TBATCHDTL[j].ITCD.retStr() &&
             DISCTYPE == VE.TBATCHDTL[j].DISCTYPE.retStr() && TDDISCTYPE == VE.TBATCHDTL[j].TDDISCTYPE.retStr() &&
              SCMDISCTYPE == VE.TBATCHDTL[j].SCMDISCTYPE.retStr() && UOM == VE.TBATCHDTL[j].UOM.retStr() && STKTYPE == VE.TBATCHDTL[j].STKTYPE.retStr() && RATE == VE.TBATCHDTL[j].RATE.retDbl() &&
             DISCRATE == VE.TBATCHDTL[j].DISCRATE.retDbl() && SCMDISCRATE == VE.TBATCHDTL[j].SCMDISCRATE.retDbl() && TDDISCRATE == VE.TBATCHDTL[j].TDDISCRATE.retDbl() && GSTPER == VE.TBATCHDTL[j].GSTPER.retDbl() &&
             HSNCODE == VE.TBATCHDTL[j].HSNCODE.retStr() && PRODGRPGSTPER == VE.TBATCHDTL[j].PRODGRPGSTPER.retStr() &&
             GLCD == VE.TBATCHDTL[j].GLCD.retStr() && FABITCD == VE.TBATCHDTL[j].FABITCD.retStr() && PDESIGN == VE.TBATCHDTL[j].PDESIGN.retStr())
                    {
                        VE.TBATCHDTL[j].TXNSLNO = TXNSLNO.retShort();
                        VE.TBATCHDTL[j].SLNO = SLNO.retShort();
                        SLNO++;
                        j++;
                        if (j >= VE.TBATCHDTL.Count - 1) break;
                    }
                    TXNSLNO++;
                }
                //fill prodgrpgstper in t_batchdtl
                DataTable allprodgrpgstper_data = new DataTable();
                string BARNO = (from a in VE.TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
                ITCD = (from a in VE.TBATCHDTL select a.ITCD).ToArray().retSqlfromStrarray();
                //string MTRLJOBCD = (from a in VE.TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                ITGRPCD = (from a in VE.TBATCHDTL select a.ITGRPCD).ToArray().retSqlfromStrarray();
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                {
                    allprodgrpgstper_data = salesfunc.GetBarHelp(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retStr(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);

                }
                else
                {
                    allprodgrpgstper_data = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);
                }
                foreach (var v in VE.TBATCHDTL)
                {
                    PRODGRPGSTPER = "";
                    string ALL_GSTPER = "";
                    GSTPER = 0;
                    if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
                    {
                        var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "'");
                        if (DATA.Count() > 0)
                        {
                            DataTable tax_data = DATA.CopyToDataTable();
                            if (tax_data != null && tax_data.Rows.Count > 0)
                            {
                                v.BALSTOCK = tax_data.Rows[0]["BALQNTY"].retDbl();
                                PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                                if (PRODGRPGSTPER != "")
                                {
                                    ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl(), v.SCMDISCTYPE, v.SCMDISCRATE.retDbl());
                                    if (ALL_GSTPER.retStr() != "")
                                    {
                                        var gst = ALL_GSTPER.Split(',').ToList();
                                        GSTPER = (from a in gst select a.retDbl()).Sum().retDbl();
                                    }
                                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                                    v.ALL_GSTPER = ALL_GSTPER;
                                    v.GSTPER = GSTPER;
                                }
                                if (tax_data.Rows[0]["barimage"].retStr() != "")
                                {
                                    v.BarImages = tax_data.Rows[0]["barimage"].retStr();
                                    var brimgs = v.BarImages.retStr().Split((char)179);
                                    v.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                                    foreach (var barimg in brimgs)
                                    {
                                        string barfilename = barimg.Split('~')[0];
                                        string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                                        FROMpath = Path.Combine(FROMpath, "");
                                        string TOPATH = System.Web.Hosting.HostingEnvironment.MapPath("~/UploadDocuments/" + barfilename);
                                        Cn.CopyImage(FROMpath, TOPATH);
                                    }
                                }
                            }
                        }
                    }

                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {

                        if (v.WPPRICEGEN.retStr() == "" || v.RPPRICEGEN.retStr() == "" || v.WPPER.retDbl() == 0 || v.RPPER.retDbl() == 0)
                        {
                            DataTable syscnfgdata = salesfunc.GetSyscnfgData(VE.T_TXN.DOCDT.retDateStr());

                            if (v.WPPRICEGEN.retStr() == "")
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {
                                    v.WPPRICEGEN = syscnfgdata.Rows[0]["wppricegen"].retStr();
                                }

                            }
                            if (v.RPPRICEGEN.retStr() == "")
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {
                                    v.RPPRICEGEN = syscnfgdata.Rows[0]["rppricegen"].retStr();
                                }
                            }
                            if (v.WPPER.retDbl() == 0)
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {
                                    v.WPPER = syscnfgdata.Rows[0]["WPPER"].retDbl();
                                }
                            }
                            if (v.RPPER.retDbl() == 0)
                            {
                                if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                {
                                    v.RPPER = syscnfgdata.Rows[0]["RPPER"].retDbl();
                                }
                            }
                        }
                    }
                }
                VE.B_T_QNTY = VE.TBATCHDTL.Sum(a => a.QNTY).retDbl();
                VE.B_T_NOS = VE.TBATCHDTL.Sum(a => a.NOS).retDbl();

                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                VE.Database_Combo4 = (from n in DB.T_BATCHDTL
                                      select new Database_Combo4() { FIELD_VALUE = n.PCSTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                VE.DropDown_list1 = DISCTYPEINRATE();
                List<DropDown_list2> RETURN_TYPE = new List<DropDown_list2> {
                    new DropDown_list2 { value = "N", text = "NO"},
                    new DropDown_list2 { value = "Y", text = "YES"},
                    };
                VE.RETURN_TYPE = RETURN_TYPE;

                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_T_SALE_BarTab", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetBaleData(TransactionSaleEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                Cn.getQueryString(VE);
                string scm = CommVar.CurSchema(UNQSNO);
                string scmf = CommVar.FinSchema(UNQSNO);
                var COMPCD = CommVar.Compcd(UNQSNO);
                var LOCCD = CommVar.Loccd(UNQSNO);
                //string AUTONO = DB.T_BALE.Where(a => a.BALENO == VE.BALENO_HELP && a.GOCD == VE.T_TXN.GOCD).Select(a => a.AUTONO).Distinct().ToArray().retSqlfromStrarray();
                string AUTONO = (from a in DB.T_BALE
                                 join b in DB.T_CNTRL_HDR on a.AUTONO equals b.AUTONO
                                 where a.BALENO == VE.BALENO_HELP && a.GOCD == VE.T_TXN.GOCD
                                 orderby b.DOCDT
                                 select a.AUTONO).FirstOrDefault();
                string Scm = CommVar.CurSchema(UNQSNO);
                string str1 = "";
                DataTable tbl = new DataTable();

                str1 += "select i.AUTONO,i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, " + Environment.NewLine;
                str1 += "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,sum(nvl(i.QNTY,0))QNTY,sum(nvl(i.NOS,0))NOS,i.RATE,i.DISCRATE, " + Environment.NewLine;
                str1 += "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,sum(nvl(i.FLAGMTR,0))FLAGMTR,i.LOCABIN,i.BALEYR " + Environment.NewLine;
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,i.ORDAUTONO,i.ORDSLNO,r.DOCNO ORDDOCNO,r.DOCDT ORDDOCDT,n.RPPRICEGEN, " + Environment.NewLine;
                str1 += "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,nvl(k.NEGSTOCK,n.NEGSTOCK)NEGSTOCK " + Environment.NewLine;
                str1 += ",s.AGDOCNO,s.AGDOCDT,s.PAGENO,s.PAGESLNO,i.PCSTYPE,s.glcd,nvl(s.BLUOMCD,k.CONVUOMCD)BLUOMCD,j.COMMONUNIQBAR,j.FABITCD,t.ITNM FABITNM,n.WPPER,n.RPPER,u.BLSLNO,k.CONVQTYPUNIT,i.BLQNTY " + Environment.NewLine;
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, " + Environment.NewLine;
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".M_STKTYPE q," + Scm + ".T_CNTRL_HDR r " + Environment.NewLine;
                str1 += "," + Scm + ".T_TXNDTL s, " + Scm + ".M_SITEM t, " + Scm + ".T_BALE u," + Scm + ".T_CNTRL_HDR v," + Scm + ".t_txn w " + Environment.NewLine;
                str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) " + Environment.NewLine;
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) and i.ORDAUTONO=r.AUTONO(+) and j.fabitcd=t.itcd(+) " + Environment.NewLine;
                str1 += "and i.autono=s.autono and i.txnslno=s.slno and i.autono=u.autono(+) and i.txnslno=u.slno(+) and i.baleno=u.baleno(+) and i.autono=v.autono and s.autono=w.autono " + Environment.NewLine;
                str1 += "and i.AUTONO in ('" + AUTONO + "') and i.BALENO='" + VE.BALENO_HELP + "' and i.GOCD='" + VE.T_TXN.GOCD + "' " + Environment.NewLine;
                str1 += "and i.SLNO <= 1000 and nvl(v.cancel, 'N') = 'N' and w.doctag not in ('SB','SR') " + Environment.NewLine;
                str1 += "group by i.AUTONO,i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, " + Environment.NewLine;
                str1 += "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.RATE,i.DISCRATE, " + Environment.NewLine;
                str1 += "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.LOCABIN,i.BALEYR " + Environment.NewLine;
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,i.ORDAUTONO,i.ORDSLNO,r.DOCNO,r.DOCDT,n.RPPRICEGEN, " + Environment.NewLine;
                str1 += "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,nvl(k.NEGSTOCK,n.NEGSTOCK) " + Environment.NewLine;
                str1 += ",s.AGDOCNO,s.AGDOCDT,s.PAGENO,s.PAGESLNO,i.PCSTYPE,s.glcd,nvl(s.BLUOMCD,k.CONVUOMCD),j.COMMONUNIQBAR,j.FABITCD,t.ITNM,n.WPPER,n.RPPER,u.BLSLNO,k.CONVQTYPUNIT,i.BLQNTY " + Environment.NewLine;
                str1 += "order by k.ITGRPCD ,i.MTRLJOBCD,k.ITCD ,i.DISCTYPE,i.TDDISCTYPE,i.SCMDISCTYPE,k.UOMCD ,i.STKTYPE ,i.RATE,i.DISCRATE,i.SCMDISCRATE,i.TDDISCRATE,i.HSNCODE ,s.GLCD,j.FABITCD ,j.PDESIGN  " + Environment.NewLine;
                tbl = masterHelp.SQLquery(str1);

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    var TBATCHDTL = (from DataRow dr in tbl.Rows
                                     select new TBATCHDTL()
                                     {
                                         SLNO = dr["SLNO"].retShort(),
                                         TXNSLNO = dr["TXNSLNO"].retShort(),
                                         ITGRPCD = dr["ITGRPCD"].retStr(),
                                         ITGRPNM = dr["ITGRPNM"].retStr(),
                                         MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                         MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                         MTBARCODE = dr["MTBARCODE"].retStr(),
                                         ITCD = dr["ITCD"].retStr(),
                                         ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                         UOM = dr["UOMCD"].retStr(),
                                         STYLENO = dr["STYLENO"].retStr(),
                                         PARTCD = dr["PARTCD"].retStr(),
                                         PARTNM = dr["PARTNM"].retStr(),
                                         PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                         COLRCD = dr["COLRCD"].retStr(),
                                         COLRNM = dr["COLRNM"].retStr(),
                                         CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                         SIZECD = dr["SIZECD"].retStr(),
                                         SIZENM = dr["SIZENM"].retStr(),
                                         SZBARCODE = dr["SZBARCODE"].retStr(),
                                         SHADE = dr["SHADE"].retStr(),
                                         QNTY = dr["QNTY"].retDbl(),
                                         NOS = dr["NOS"].retDbl(),
                                         RATE = dr["RATE"].retDbl(),
                                         //DISCRATE = dr["DISCRATE"].retDbl(),
                                         //DISCTYPE = dr["DISCTYPE"].retStr() == "" ? "A" : dr["DISCTYPE"].retStr(),
                                         //DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : (dr["DISCTYPE"].retStr() == "A" || dr["DISCTYPE"].retStr() == "") ? "AftDsc%" : dr["DISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                         //TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                         //TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : (dr["TDDISCTYPE"].retStr() == "A" || dr["TDDISCTYPE"].retStr() == "") ? "AftDsc%" : dr["TDDISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                         //TDDISCTYPE = dr["TDDISCTYPE"].retStr() == "" ? "A" : dr["TDDISCTYPE"].retStr(),
                                         //SCMDISCTYPE_DESC = (dr["SCMDISCTYPE"].retStr() == "P" || dr["SCMDISCTYPE"].retStr() == "") ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : dr["SCMDISCTYPE"].retStr() == "F" ? "Fixed" : "",
                                         //SCMDISCTYPE = dr["SCMDISCTYPE"].retStr() == "" ? "P" : dr["SCMDISCTYPE"].retStr(),
                                         //SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                         STKTYPE = dr["STKTYPE"].retStr(),
                                         STKNAME = dr["STKNAME"].retStr(),
                                         BARNO = dr["BARNO"].retStr(),
                                         HSNCODE = dr["HSNCODE"].retStr(),
                                         BALENO = dr["BALENO"].retStr(),
                                         PDESIGN = dr["PDESIGN"].retStr(),
                                         OURDESIGN = dr["OURDESIGN"].retStr(),
                                         FLAGMTR = dr["FLAGMTR"].retDbl(),
                                         LOCABIN = dr["LOCABIN"].retStr(),
                                         BALEYR = dr["BALEYR"].retStr(),
                                         BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                         GLCD = dr[MenuDescription(VE.MENU_PARA).Rows[0]["glcd"].retStr()].retStr(),
                                         WPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPRATE"].retDbl() : (double?)null,
                                         RPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPRATE"].retDbl() : (double?)null,
                                         ITREM = dr["ITREM"].retStr(),
                                         ORDAUTONO = dr["ORDAUTONO"].retStr(),
                                         ORDSLNO = dr["ORDSLNO"].retStr() == "" ? (short?)null : dr["ORDSLNO"].retShort(),
                                         ORDDOCNO = dr["ORDDOCNO"].retStr(),
                                         ORDDOCDT = dr["ORDDOCDT"].retStr() == "" ? "" : dr["ORDDOCDT"].retStr().Remove(10),
                                         WPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPPRICEGEN"].retStr() : "",
                                         RPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPPRICEGEN"].retStr() : "",
                                         AGDOCNO = (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") ? dr["AGDOCNO"].retStr() : "",
                                         AGDOCDT = (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") ? (dr["AGDOCDT"].retStr() == "" ? (DateTime?)null : Convert.ToDateTime(dr["AGDOCDT"].retDateStr())) : (DateTime?)null,
                                         LISTPRICE = dr["LISTPRICE"].retDbl(),
                                         LISTDISCPER = dr["LISTDISCPER"].retDbl(),
                                         CUTLENGTH = dr["CUTLENGTH"].retDbl(),
                                         PAGENO = dr["PAGENO"].retInt(),
                                         PAGESLNO = dr["PAGESLNO"].retInt(),
                                         PCSTYPE = dr["PCSTYPE"].retStr(),
                                         NEGSTOCK = dr["NEGSTOCK"].retStr(),
                                         COMMONUNIQBAR = dr["COMMONUNIQBAR"].retStr(),
                                         FABITCD = dr["FABITCD"].retStr(),
                                         FABITNM = dr["FABITNM"].retStr(),
                                         WPPER = dr["WPPER"].retDbl(),
                                         RPPER = dr["RPPER"].retDbl(),
                                         RECPROGSLNO = dr["BLSLNO"].retShort(),
                                         //BLQNTY = dr["BLQNTY"].retDbl(),
                                         BLQNTY = ((dr["CUTLENGTH"].retDbl() == 0 || dr["NOS"].retDbl() == 0) && dr["CONVQTYPUNIT"].retDbl() != 0 && dr["BLQNTY"].retDbl() == 0) ? (dr["QNTY"].retDbl() / dr["CONVQTYPUNIT"].retDbl()) : 0,
                                         CONVQTYPUNIT = dr["CONVQTYPUNIT"].retDbl(),
                                         BLUOMCD = dr["BLUOMCD"].retStr() == "" ? "" : dr["CONVQTYPUNIT"].retStr() + " " + dr["BLUOMCD"].retStr(),
                                     }).ToList();
                    if (VE.TBATCHDTL != null && VE.TBATCHDTL.Count > 0)
                    {
                        VE.TBATCHDTL.AddRange(TBATCHDTL);
                    }
                    else
                    {
                        VE.TBATCHDTL = TBATCHDTL;
                    }
                    DataTable dt = ListToDatatable.LINQResultToDataTable(VE.TBATCHDTL);

                    string ITGRPCD = "", MTRLJOBCD = "", ITCD = "", DISCTYPE = "", TDDISCTYPE = "", SCMDISCTYPE = "", UOM = "",
                        STKTYPE = "", GLCD = "", FABITCD = "",
                        PDESIGN = "", HSNCODE = "", PRODGRPGSTPER = "", BLUOMCD = "", RECPROGSLNO = "", BALENO = "";
                    double RATE = 0, DISCRATE = 0, SCMDISCRATE = 0, TDDISCRATE = 0, GSTPER = 0;
                    //fill prodgrpgstper in t_batchdtl
                    DataTable allprodgrpgstper_data = new DataTable();
                    string BARNO = (from a in VE.TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
                    ITCD = (from a in VE.TBATCHDTL select a.ITCD).ToArray().retSqlfromStrarray();
                    //string MTRLJOBCD = (from a in VE.TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                    ITGRPCD = (from a in VE.TBATCHDTL select a.ITGRPCD).ToArray().retSqlfromStrarray();
                    string PRCCD = VE.T_TXNOTH.PRCCD.retStr() == "" ? "WP" : VE.T_TXNOTH.PRCCD.retStr();
                    string TAXGRPCD = VE.T_TXNOTH.TAXGRPCD.retStr() == "" ? "C001" : VE.T_TXNOTH.TAXGRPCD.retStr();
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {
                        allprodgrpgstper_data = salesfunc.GetBarHelp(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retStr(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);

                    }
                    else
                    {
                        //allprodgrpgstper_data = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);
                        allprodgrpgstper_data = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true, "", "", true);
                    }
                    //var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                    var MSYSCNFG = salesfunc.M_SYSCNFG(VE.T_TXN.DOCDT.retDateStr());
                    foreach (var v in VE.TBATCHDTL)
                    {
                        PRODGRPGSTPER = "";
                        string ALL_GSTPER = "";
                        GSTPER = 0;
                        if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
                        {
                            var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "'");
                            if (DATA.Count() > 0)
                            {
                                DataTable tax_data = DATA.CopyToDataTable();
                                if (tax_data != null && tax_data.Rows.Count > 0)
                                {
                                    v.BALSTOCK = tax_data.Rows[0]["BALQNTY"].retDbl();
                                    PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                                    if (PRODGRPGSTPER != "")
                                    {
                                        ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl(), v.SCMDISCTYPE, v.SCMDISCRATE.retDbl());
                                        if (ALL_GSTPER.retStr() != "")
                                        {
                                            var gst = ALL_GSTPER.Split(',').ToList();
                                            GSTPER = (from a in gst select a.retDbl()).Sum().retDbl();
                                        }
                                        v.PRODGRPGSTPER = PRODGRPGSTPER;
                                        v.ALL_GSTPER = ALL_GSTPER;
                                        v.GSTPER = GSTPER;
                                    }
                                    if (tax_data.Rows[0]["barimage"].retStr() != "")
                                    {
                                        v.BarImages = tax_data.Rows[0]["barimage"].retStr();
                                        var brimgs = v.BarImages.retStr().Split((char)179);
                                        v.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                                        foreach (var barimg in brimgs)
                                        {
                                            string barfilename = barimg.Split('~')[0];
                                            string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                                            FROMpath = Path.Combine(FROMpath, "");
                                            string TOPATH = System.Web.Hosting.HostingEnvironment.MapPath("~/UploadDocuments/" + barfilename);
                                            Cn.CopyImage(FROMpath, TOPATH);
                                        }
                                    }
                                    if (VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "SBPOS")
                                    {
                                        v.RATE = tax_data.Rows[0]["RATE"].retDbl();
                                    }
                                    if ((VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "SBPOS") && MSYSCNFG.MNTNLISTPRICE == "Y")
                                    {
                                        v.LISTPRICE = tax_data.Rows[0]["RATE"].retDbl();
                                    }
                                }
                            }
                        }

                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                        {

                            if (v.WPPRICEGEN.retStr() == "" || v.RPPRICEGEN.retStr() == "" || v.WPPER.retDbl() == 0 || v.RPPER.retDbl() == 0)
                            {
                                DataTable syscnfgdata = salesfunc.GetSyscnfgData(VE.T_TXN.DOCDT.retDateStr());

                                if (v.WPPRICEGEN.retStr() == "")
                                {
                                    if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                    {
                                        v.WPPRICEGEN = syscnfgdata.Rows[0]["wppricegen"].retStr();
                                    }

                                }
                                if (v.RPPRICEGEN.retStr() == "")
                                {
                                    if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                    {
                                        v.RPPRICEGEN = syscnfgdata.Rows[0]["rppricegen"].retStr();
                                    }
                                }
                                if (v.WPPER.retDbl() == 0)
                                {
                                    if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                    {
                                        v.WPPER = syscnfgdata.Rows[0]["WPPER"].retDbl();
                                    }
                                }
                                if (v.RPPER.retDbl() == 0)
                                {
                                    if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                                    {
                                        v.RPPER = syscnfgdata.Rows[0]["RPPER"].retDbl();
                                    }
                                }
                            }
                        }
                    }
                    VE.B_T_QNTY = VE.TBATCHDTL.Sum(a => a.QNTY).retDbl();
                    VE.B_T_NOS = VE.TBATCHDTL.Sum(a => a.NOS).retDbl();
                    int j = 0;

                    int TXNSLNO = 1, SLNO = 1;
                    while (j <= VE.TBATCHDTL.Count - 1)
                    {
                        ITGRPCD = VE.TBATCHDTL[j].ITGRPCD.retStr();
                        MTRLJOBCD = VE.TBATCHDTL[j].MTRLJOBCD.retStr();
                        ITCD = VE.TBATCHDTL[j].ITCD.retStr();
                        DISCTYPE = VE.TBATCHDTL[j].DISCTYPE.retStr() == "" ? "A" : VE.TBATCHDTL[j].DISCTYPE.retStr();
                        TDDISCTYPE = VE.TBATCHDTL[j].TDDISCTYPE.retStr() == "" ? "A" : VE.TBATCHDTL[j].TDDISCTYPE.retStr();
                        SCMDISCTYPE = VE.TBATCHDTL[j].SCMDISCTYPE.retStr() == "" ? "P" : VE.TBATCHDTL[j].SCMDISCTYPE.retStr();
                        UOM = VE.TBATCHDTL[j].UOM.retStr();
                        STKTYPE = VE.TBATCHDTL[j].STKTYPE.retStr();
                        RATE = VE.TBATCHDTL[j].RATE.retDbl();
                        DISCRATE = VE.TBATCHDTL[j].DISCRATE.retDbl();
                        SCMDISCRATE = VE.TBATCHDTL[j].SCMDISCRATE.retDbl();
                        TDDISCRATE = VE.TBATCHDTL[j].TDDISCRATE.retDbl();
                        GSTPER = VE.TBATCHDTL[j].GSTPER.retDbl();
                        HSNCODE = VE.TBATCHDTL[j].HSNCODE.retStr();
                        PRODGRPGSTPER = VE.TBATCHDTL[j].PRODGRPGSTPER.retStr();
                        GLCD = VE.TBATCHDTL[j].GLCD.retStr();
                        FABITCD = VE.TBATCHDTL[j].FABITCD.retStr();
                        PDESIGN = VE.TBATCHDTL[j].PDESIGN.retStr();
                        BLUOMCD = VE.TBATCHDTL[j].BLUOMCD.retStr();
                        RECPROGSLNO = VE.TBATCHDTL[j].RECPROGSLNO.retStr();
                        BALENO = VE.TBATCHDTL[j].BALENO.retStr();

                        while (ITGRPCD == VE.TBATCHDTL[j].ITGRPCD.retStr() && MTRLJOBCD == VE.TBATCHDTL[j].MTRLJOBCD.retStr()
                            && ITCD == VE.TBATCHDTL[j].ITCD.retStr() &&
                 DISCTYPE == (VE.TBATCHDTL[j].DISCTYPE.retStr() == "" ? "A" : VE.TBATCHDTL[j].DISCTYPE.retStr()) && TDDISCTYPE == (VE.TBATCHDTL[j].TDDISCTYPE.retStr() == "" ? "A" : VE.TBATCHDTL[j].TDDISCTYPE.retStr()) &&
                  SCMDISCTYPE == (VE.TBATCHDTL[j].SCMDISCTYPE.retStr() == "" ? "P" : VE.TBATCHDTL[j].SCMDISCTYPE.retStr()) && UOM == VE.TBATCHDTL[j].UOM.retStr() && STKTYPE == VE.TBATCHDTL[j].STKTYPE.retStr() && RATE == VE.TBATCHDTL[j].RATE.retDbl() &&
                 DISCRATE == VE.TBATCHDTL[j].DISCRATE.retDbl() && SCMDISCRATE == VE.TBATCHDTL[j].SCMDISCRATE.retDbl() && TDDISCRATE == VE.TBATCHDTL[j].TDDISCRATE.retDbl() && GSTPER == VE.TBATCHDTL[j].GSTPER.retDbl() &&
                 HSNCODE == VE.TBATCHDTL[j].HSNCODE.retStr() && PRODGRPGSTPER == VE.TBATCHDTL[j].PRODGRPGSTPER.retStr() &&
                 GLCD == VE.TBATCHDTL[j].GLCD.retStr() && FABITCD == VE.TBATCHDTL[j].FABITCD.retStr() && PDESIGN == VE.TBATCHDTL[j].PDESIGN.retStr()
                 && BLUOMCD == VE.TBATCHDTL[j].BLUOMCD.retStr() && RECPROGSLNO == VE.TBATCHDTL[j].RECPROGSLNO.retStr() && BALENO == VE.TBATCHDTL[j].BALENO.retStr())
                        {
                            VE.TBATCHDTL[j].TXNSLNO = VE.MERGEINDTL == true ? TXNSLNO.retShort() : SLNO.retShort();
                            VE.TBATCHDTL[j].SLNO = SLNO.retShort();
                            SLNO++;
                            j++;
                            if (j > VE.TBATCHDTL.Count - 1) break;
                        }
                        TXNSLNO++;
                        if (j > VE.TBATCHDTL.Count - 1) break;
                    }
                }
                VE.Database_Combo4 = (from n in DB.T_BATCHDTL
                                      select new Database_Combo4() { FIELD_VALUE = n.PCSTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_T_SALE_BarTab", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult CopyBarGridRow(TransactionSaleEntry VE, int BarSlno, double NOOFROWCOPY)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                int MAXSLNO = VE.TBATCHDTL.Select(a => a.SLNO).Max();
                int MAXBLSLNO = VE.TBATCHDTL.Select(a => a.TXNSLNO).Max();
                TBATCHDTL TBATCHDTLobjtemp = VE.TBATCHDTL.Where(m => m.SLNO == BarSlno).First();
                TBATCHDTL TBATCHDTLobj = new TBATCHDTL();
                for (int i = 0; i <= NOOFROWCOPY - 1; i++)
                {
                    TBATCHDTLobj = new TBATCHDTL();
                    foreach (PropertyInfo propA in TBATCHDTLobjtemp.GetType().GetProperties())
                    {
                        PropertyInfo propB = TBATCHDTLobjtemp.GetType().GetProperty(propA.Name);
                        propB.SetValue(TBATCHDTLobj, propA.GetValue(TBATCHDTLobjtemp, null), null);
                    }
                    TBATCHDTLobj.SLNO = (++MAXSLNO).retShort();
                    if (VE.MERGEINDTL == false)
                    {
                        TBATCHDTLobj.TXNSLNO = (++MAXBLSLNO).retShort();
                    }
                    TBATCHDTLobj.NOOFROWCOPY = "";
                    VE.TBATCHDTL.Add(TBATCHDTLobj);
                }
                VE.Database_Combo4 = (from n in DB.T_BATCHDTL
                                      select new Database_Combo4() { FIELD_VALUE = n.PCSTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_BarTab", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult LastAgentTransport(string Party, string Doccd, string Comesfrom)
        {
            try
            {
                string str = "", caption = "";
                string scm = CommVar.CurSchema(UNQSNO);
                string scmf = CommVar.FinSchema(UNQSNO);
                //string scm_prevyr = CommVar.LastYearSchema(UNQSNO);
                //string scmf_prevyr = CommVar.FinSchemaPrevYr(UNQSNO);
                string sql = "";
                if (Comesfrom == "A")
                {
                    //sql += "select * from ( select b.agslcd slcd,c.slnm from " + scm + ".t_txn a," + scm + ".t_txnoth b," + scmf + ".m_subleg c ";
                    //sql += "where a.autono=b.autono(+) and b.agslcd=c.slcd(+) and a.slcd='" + Party + "' and a.doccd='" + Doccd + "' and  b.agslcd is not null order by a.autono desc ) where rownum=1 ";

                    sql += "select * from (select * from ( select a.autono,b.agslcd slcd,c.slnm from " + scm + ".t_txn a," + scm + ".t_txnoth b," + scmf + ".m_subleg c ";
                    sql += "where a.autono=b.autono(+) and b.agslcd=c.slcd(+) and a.slcd='" + Party + "' and a.doccd='" + Doccd + "' and  b.agslcd is not null ";
                    for (int a = 0; a < 2; a++)
                    {
                        string scm_prevyr = "", scmf_prevyr = "";
                        if (a == 0)
                        {
                            scm_prevyr = CommVar.LastYearSchema(UNQSNO);
                            scmf_prevyr = CommVar.FinSchemaPrevYr(UNQSNO);
                        }
                        else
                        {
                            scm_prevyr = salesfunc.PrevSchema(CommVar.LastYearSchema(UNQSNO));
                            scmf_prevyr = salesfunc.PrevSchema(CommVar.FinSchemaPrevYr(UNQSNO), "FIN_COMPANY");
                        }
                        if (scm_prevyr.retStr() != "")
                        {
                            sql += "union all ";
                            sql += "select a.autono,b.agslcd slcd,c.slnm from " + scm_prevyr + ".t_txn a," + scm_prevyr + ".t_txnoth b," + scmf_prevyr + ".m_subleg c ";
                            sql += "where a.autono=b.autono(+) and b.agslcd=c.slcd(+) and a.slcd='" + Party + "' and a.doccd='" + Doccd + "' and  b.agslcd is not null ";
                        }
                    }
                    sql += ")order by autono desc)  where rownum=1  ";

                    caption = "Agent";
                }
                else if (Comesfrom == "T")
                {
                    //sql += "select * from ( select b.translcd slcd,c.slnm from " + scm + ".t_txn a," + scm + " .t_txntrans b," + scmf + ".m_subleg c ";
                    //sql += "where a.autono=b.autono(+) and b.translcd=c.slcd(+) and a.slcd='" + Party + "' and a.doccd='" + Doccd + "' and  b.translcd is not null order by a.autono desc) where rownum=1 ";

                    sql += "select * from (select * from ( select a.autono,b.translcd slcd,c.slnm from " + scm + ".t_txn a," + scm + " .t_txntrans b," + scmf + ".m_subleg c ";
                    sql += "where a.autono=b.autono(+) and b.translcd=c.slcd(+) and a.slcd='" + Party + "' and a.doccd='" + Doccd + "' and  b.translcd is not null  ";
                    for (int a = 0; a < 2; a++)
                    {
                        string scm_prevyr = "", scmf_prevyr = "";
                        if (a == 0)
                        {
                            scm_prevyr = CommVar.LastYearSchema(UNQSNO);
                            scmf_prevyr = CommVar.FinSchemaPrevYr(UNQSNO);
                        }
                        else
                        {
                            scm_prevyr = salesfunc.PrevSchema(CommVar.LastYearSchema(UNQSNO));
                            scmf_prevyr = salesfunc.PrevSchema(CommVar.FinSchemaPrevYr(UNQSNO), "FIN_COMPANY");
                        }
                        if (scm_prevyr.retStr() != "")
                        {
                            sql += "union all ";
                            sql += "select a.autono,b.translcd slcd,c.slnm from " + scm_prevyr + ".t_txn a," + scm_prevyr + " .t_txntrans b," + scmf_prevyr + ".m_subleg c ";
                            sql += "where a.autono=b.autono(+) and b.translcd=c.slcd(+) and a.slcd='" + Party + "' and a.doccd='" + Doccd + "' and  b.translcd is not null  ";
                        }
                    }
                    sql += ")order by autono desc)  where rownum=1  ";
                    caption = "Transporter";
                }
                var data = masterHelp.SQLquery(sql);
                if (data != null && data.Rows.Count > 0)
                {
                    str = masterHelp.ToReturnFieldValues("", data);
                }
                else
                {
                    str = caption + " not found respect of this party !!";
                }
                return Content(str);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult GetTDSDetails(string val, string Code)
        {
            try
            {
                string slcd = "", DOCDT = "";
                var temp = Code.Split(Convert.ToChar(Cn.GCS()));
                if (temp.Length > 1)
                {
                    slcd = temp[0];
                    DOCDT = temp[1];
                }
                if (DOCDT.retStr() == "") return Content("Enter Document Date");
                if (slcd.retStr() == "") return Content("Enter Party Code");
                if (val == "")
                {
                    return PartialView("_Help2", masterHelp.TDSCODE_help(DOCDT, val, slcd));
                }
                else
                {
                    return Content(masterHelp.TDSCODE_help(DOCDT, val, slcd));
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }

        public dynamic SAVE(TransactionSaleEntry VE, string othr_para = "")
        {
            //Cn.getQueryString(VE);
            //Oracle Queries
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            ImprovarDB DBF1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "";
            string[] dbsql1;
            string ContentFlg = "";

            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            try
            {
                OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);

                int balenocount = 0;
                if (VE.TTXNDTL != null && VE.TTXNDTL.Count > 0)
                {
                    balenocount = VE.TTXNDTL.Where(a => a.BALENO.retStr() != "").Count();
                }
                int minhsnlen = 0;
                var sql = "select minhsnlen from " + CommVar.FinSchema(UNQSNO) + ".m_comp where compcd='" + CommVar.Compcd(UNQSNO) + "'";
                var compdt = masterHelp.SQLquery(sql);
                if (compdt != null)
                {
                    minhsnlen = compdt.Rows[0]["minhsnlen"].retInt();
                }
                DataTable baledata = new DataTable();
                if ((VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "SBPOS" || VE.MENU_PARA == "PJBR" || VE.MENU_PARA == "SR") && balenocount > 0)
                {
                    var baleno = VE.TTXNDTL.Select(a => a.BALENO).Distinct().ToArray().retSqlfromStrarray();
                    string str = "select rslno,blautono,blslno,lrdt,lrno,baleyr,gocd,baleopen,baleno from " + scm1 + ".t_bale where baleno in (" + baleno + ") ";
                    baledata = masterHelp.SQLquery(str);
                }

                DataTable baledatas = new DataTable();
                DataTable lr_det = new DataTable();
                if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC") && balenocount > 0)
                {
                    var baleno = VE.TTXNDTL.Select(a => a.BALENO).Distinct().ToArray().retSqlfromStrarray();
                    string str = "select a.autono,b.docno,a.baleno from " + scm1 + ".T_BATCHDTL a," + scm1 + ".t_cntrl_hdr b where a.autono=b.autono and b.doccd='" + VE.T_TXN.DOCCD + "' and baleno in (" + baleno + ") ";
                    if (VE.T_TXN.AUTONO != null) str += " and a.autono <>'" + VE.T_TXN.AUTONO + "' ";
                    baledatas = masterHelp.SQLquery(str);
                    if (baledatas.Rows.Count > 0)
                    {
                        string dublicate_baleno = (from DataRow dr in baledatas.Rows select dr["baleno"].retStr()).Distinct().ToArray().retSqlfromStrarray();

                        string exsist_bale_docno = (from DataRow dr in baledatas.Rows select dr["docno"].retStr()).Distinct().ToArray().retSqlfromStrarray();


                        ContentFlg = "Bale No. [" + dublicate_baleno + "] already exsist in these Doc No.[" + exsist_bale_docno + "] "; goto dbnotsave;
                    }

                    //string str1 = "select a.autono,b.docno,a.lrno from " + scm1 + ".T_TXNTRANS a," + scm1 + ".t_cntrl_hdr b where a.autono=b.autono and b.doccd='" + VE.T_TXN.DOCCD + "' and a.LRNO ='" + VE.T_TXNTRANS.LRNO + "' ";
                    //if (VE.T_TXN.AUTONO != null) str1 += " and a.autono <>'" + VE.T_TXN.AUTONO + "' ";
                    //lr_det = masterHelp.SQLquery(str1);
                    //if (lr_det.Rows.Count > 0)
                    //{                        
                    //    string exsist_Lr_docno = (from DataRow dr in lr_det.Rows select dr["docno"].retStr()).Distinct().ToArray().retSqlfromStrarray();
                    //    ContentFlg = "LR No. [" + VE.T_TXNTRANS.LRNO + "] already exsist in these Doc No.[" + exsist_Lr_docno + "] "; goto dbnotsave;
                    //}

                }

                string MasterTblDataExist = IsMasterTblDataExist(VE);
                if (MasterTblDataExist != "")
                {
                    string msg = "Please Fill data in ";
                    var data = MasterTblDataExist.Split(Convert.ToChar(Cn.GCS()));
                    foreach (var a in data)
                    {
                        if (a != "")
                        {
                            msg += a + ", ";
                        }
                    }
                    ContentFlg = msg; goto dbnotsave;
                }
                if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                {
                    if (((VE.Last_TAXGRPCD.retStr() != VE.T_TXNOTH.TAXGRPCD.retStr()) || (VE.Last_PRCCD.retStr() != VE.T_TXNOTH.PRCCD.retStr())) && othr_para == "")
                    {
                        ContentFlg = "Entry Can't Save ! Previous Tax Group/Price and Current Tax Group/Price not match ";
                        goto dbnotsave;
                    }
                    var subleg = (from a in DBF1.M_SUBLEG where a.SLCD == VE.T_TXN.SLCD select new { a.SLNM, a.SLAREA, a.DISTRICT, a.GSTNO, a.PSLCD, a.TCSAPPL, a.PANNO, a.PARTYCD, a.REGMOBILE, a.ADD1, a.ADD2, a.ADD3, a.ADD4, a.ADD5, a.ADD6, a.ADD7 }).FirstOrDefault();

                    if (VE.MENU_PARA == "SBPOS")
                    {
                        if (VE.T_TXNMEMO.NM.retStr() == "")
                        {
                            VE.T_TXNMEMO.NM = subleg.SLNM;
                        }
                        if (VE.T_TXNMEMO.MOBILE.retStr() == "")
                        {
                            VE.T_TXNMEMO.MOBILE = subleg.REGMOBILE.retStr();
                        }
                        if (VE.T_TXNMEMO.ADDR.retStr() == "")
                        {
                            var addrs = subleg.ADD1.retStr() == "" ? "" : (subleg.ADD1 + " ");
                            addrs += subleg.ADD2.retStr() == "" ? "" : (subleg.ADD2 + " ");
                            addrs += subleg.ADD3.retStr() == "" ? "" : (subleg.ADD3 + " ");
                            addrs += subleg.ADD4.retStr() == "" ? "" : (subleg.ADD4 + " ");
                            addrs += subleg.ADD5.retStr() == "" ? "" : (subleg.ADD5 + " ");
                            addrs += subleg.ADD6.retStr() == "" ? "" : (subleg.ADD6 + " ");
                            addrs += subleg.ADD7.retStr() == "" ? "" : (subleg.ADD7 + " ");
                            VE.T_TXNMEMO.ADDR = addrs.TrimEnd();
                        }
                        if (VE.T_TXNMEMO.CITY.retStr() == "")
                        {
                            VE.T_TXNMEMO.CITY = subleg.DISTRICT;
                        }
                    }
                    string PIAUTONO = "";
                    if ((VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS") && VE.DefaultAction == "A")
                    {
                        PIAUTONO = VE.T_TXN.AUTONO;
                    }

                    if (VE.TBATCHDTL != null && VE.TTXNDTL != null)
                    {
                        //var barcodedata = (from x in VE.TBATCHDTL
                        //                   group x by new { x.ITCD } into P
                        //                   select new
                        //                   {
                        //                       ITCD = P.Key.ITCD,
                        //                       QTY = P.Sum(A => A.QNTY).retDbl().toRound(3),
                        //                       NOS = P.Sum(A => A.NOS).retDbl()
                        //                   }).Where(a => a.QTY != 0).ToList();
                        var barcodedata = (from x in VE.TBATCHDTL
                                           select new
                                           {
                                               ITCD = x.ITCD,
                                               NOS = (x.UOM == "MTR" && x.NOS.retDbl() == 0) ? 1 : x.NOS,
                                               QNTY = x.QNTY
                                           }).ToList().GroupBy(l => l.ITCD).Select(P => new
                                           {
                                               ITCD = P.Key,
                                               QTY = P.Sum(A => A.QNTY).retDbl().toRound(3),
                                               NOS = P.Sum(A => A.NOS).retDbl()
                                           }).Where(a => a.QTY != 0).ToList();

                        var txndtldata = (from x in VE.TTXNDTL
                                          group x by new { x.ITCD } into P
                                          select new
                                          {
                                              ITCD = P.Key.ITCD,
                                              QTY = P.Sum(A => A.QNTY).retDbl().toRound(3),
                                              NOS = P.Sum(A => A.NOS).retDbl()
                                          }).Where(a => a.QTY != 0).ToList();

                        var difList = barcodedata.Where(a => !txndtldata.Any(a1 => a1.ITCD == a.ITCD && a1.QTY == a.QTY && a1.NOS == a.NOS))
                         .Union(txndtldata.Where(a => !barcodedata.Any(a1 => a1.ITCD == a.ITCD && a1.QTY == a.QTY && a1.NOS == a.NOS))).ToList();
                        if (difList != null && difList.Count > 0)
                        {
                            string diffitcd = difList.Select(a => a.ITCD).Distinct().ToArray().retSqlfromStrarray();
                            //OraTrans.Rollback();
                            // OraCon.Dispose();
                            ContentFlg = "Barcode grid & Detail grid itcd [" + diffitcd + "] wise qnty, nos should match !!";
                            goto dbnotsave;
                        }
                    }


                    T_TXN TTXN = new T_TXN();
                    T_TXNTRANS TXNTRANS = new T_TXNTRANS();
                    T_TXNOTH TTXNOTH = new T_TXNOTH();
                    T_TXN_LINKNO TTXNLINKNO = new T_TXN_LINKNO();
                    T_TXNMEMO TTXNMEMO = new T_TXNMEMO();
                    T_STKTRNF TSTKTRNF = new T_STKTRNF();

                    string DOCPATTERN = "";
                    string docpassrem = "";
                    bool blactpost = true, blgstpost = true;
                    TTXN.DOCDT = VE.T_TXN.DOCDT;
                    string Ddate = Convert.ToString(TTXN.DOCDT);
                    TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                    string auto_no = ""; string Month = "";

                    #region Posting to finance Preparation
                    int COUNTER = 0;
                    string expglcd = "";
                    string stkdrcr = "C", strqty = " Q=" + VE.TOTQNTY.toRound(2).ToString();
                    string trcd = "TR";
                    string revcharge = "";
                    string dr = ""; string cr = ""; int isl = 0; string strrem = "";
                    double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
                    double dbDrAmt = 0, dbCrAmt = 0;
                    blactpost = true; blgstpost = true;
                    string parglcd = "saldebglcd", parclass1cd = "";
                    string strblno = "", strbldt = "", strduedt = "", strrefno = "", strvtype = "BL";
                    dr = "D"; cr = "C";
                    string sslcd = VE.T_TXN.SLCD;
                    if (VE.PSLCD.retStr() != "") sslcd = VE.PSLCD.ToString();

                    switch (VE.MENU_PARA)
                    {
                        case "SBPCK":
                            stkdrcr = "N"; blactpost = false; blgstpost = false; break;
                        case "SB":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Sale" + strqty; break;
                        case "SBDIR":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Sale" + strqty; break;
                        case "ISS":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Sale" + strqty; break;
                        case "SR":
                            stkdrcr = "D"; dr = "C"; cr = "D"; trcd = "SC"; strrem = "SRM agst " + VE.T_TXN.PREFNO + " dtd. " + VE.T_TXN.PREFDT.ToString().retDateStr() + strqty; break;
                        case "SBCM":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Cash Sale" + strqty; break;
                        case "SBCMR":
                            stkdrcr = "D"; dr = "C"; cr = "D"; trcd = "SC"; strrem = "Cash Sale Return" + strqty; break;
                        case "SBEXP":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Sale Export" + strqty; break;
                        case "PI":
                            stkdrcr = VE.STOCKHOLD == true ? "C" : "0"; blactpost = false; blgstpost = false; break;
                        case "PB":
                            stkdrcr = "D"; parglcd = "purdebglcd"; dr = "C"; cr = "D"; trcd = "PB"; strrem = "Purchase Blno " + VE.T_TXN.PREFNO + " dtd. " + VE.T_TXN.PREFDT.ToString().retDateStr() + strqty; break;
                        case "REC":
                            stkdrcr = "D"; parglcd = "purdebglcd"; dr = "C"; cr = "D"; trcd = "PB"; strrem = "Purchase Blno " + VE.T_TXN.PREFNO + " dtd. " + VE.T_TXN.PREFDT.ToString().retDateStr() + strqty; break;
                        case "PR":
                            stkdrcr = "C"; parglcd = "purdebglcd"; trcd = "PD"; strrem = "PRM agst " + VE.T_TXN.PREFNO + " dtd. " + VE.T_TXN.PREFDT.ToString().retDateStr() + strqty; break;
                        case "OP":
                            stkdrcr = "D"; blactpost = false; blgstpost = false; break;
                        case "OTH":
                            stkdrcr = "D"; blactpost = false; blgstpost = true; break;
                        case "PJRC":
                            stkdrcr = "D"; blactpost = false; blgstpost = false; break;
                        case "PJIS":
                            stkdrcr = "C"; blactpost = false; blgstpost = false; break;
                        case "PJRT":
                            stkdrcr = "C"; blactpost = false; blgstpost = false; break;
                        case "PJBL":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Sale" + strqty; break;
                        case "PJBR":
                            stkdrcr = "D"; dr = "C"; cr = "D"; trcd = "SB"; strrem = "Sale" + strqty; break;
                        case "SCN":
                            stkdrcr = "N"; dr = "C"; cr = "D"; blactpost = true; blgstpost = true; break;
                        case "SDN":
                            stkdrcr = "N"; blactpost = true; blgstpost = true; break;
                        case "PCN":
                            stkdrcr = "N"; dr = "C"; cr = "D"; blactpost = true; blgstpost = true; break;
                        case "PDN":
                            stkdrcr = "N"; blactpost = true; blgstpost = true; break;
                        case "SBPOS":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Cash Sale" + strqty; break;
                    }
                    if (VE.GSTNO.retStr() == CommVar.GSTNO(UNQSNO))
                    {
                        if (VE.MENU_PARA == "REC")
                        {
                            blactpost = false; blgstpost = false;
                        }
                        else if (VE.MENU_PARA == "ISS")
                        {
                            blactpost = false; blgstpost = true;
                        }

                    }
                    string slcdlink = "", slcdpara = VE.MENU_PARA;
                    if (VE.MENU_PARA == "PR") slcdpara = "PB";
                    //M_SYSCNFG MSYSCNFG = DB.M_SYSCNFG.FirstOrDefault();
                    M_SYSCNFG MSYSCNFG = salesfunc.M_SYSCNFG(VE.T_TXN.DOCDT.retDateStr());
                    sql = "";
                    if (MSYSCNFG == null)
                    {
                        ContentFlg = "Debtor/Creditor code not setup/No data found at m_syscnfg"; goto dbnotsave;
                    }
                    string prodglcd = (VE.TTXNDTL != null) ? VE.TTXNDTL[0].GLCD.retStr() : "";

                    sql = "select b.rogl, b.tcsgl,b.TCSPURGLCD, a.class1cd, null class2cd, nvl(c.crlimit,0) crlimit, nvl(c.crdays,0) crdays, ";
                    sql += "'" + prodglcd.retStr() + "' prodglcd, ";
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") sql += "b.igst_p igstcd, b.cgst_p cgstcd, b.sgst_p sgstcd, b.cess_p cesscd, b.duty_p dutycd, ";
                    else sql += "b.igst_s igstcd, b.cgst_s cgstcd, b.sgst_s sgstcd, b.cess_s cesscd, b.duty_s dutycd, ";
                    //if (slcdpara == "PB" || slcdpara == "OP" || slcdpara == "OTH" || VE.MENU_PARA == "PJRC") sql += "a.purdebglcd parglcd, "; else sql += "a.saldebglcd parglcd, ";
                    if (othr_para.retStr() != "")
                    {
                        if (slcdpara == "PB" || VE.MENU_PARA == "REC" || slcdpara == "OP" || slcdpara == "OTH" || VE.MENU_PARA == "PJRC") sql += "a.purdebglcd parglcd, "; else sql += "a.saldebglcd parglcd, ";
                    }
                    else
                    {
                        sql += "" + VE.T_TXN.PARGLCD.retStr() + " parglcd, ";
                    }

                    sql += "igst_rvi, cgst_rvi, sgst_rvi, cess_rvi, igst_rvo, cgst_rvo, sgst_rvo, cess_rvo ";
                    sql += "from " + scm1 + ".m_syscnfg a, " + scmf + ".m_post b, " + scm1 + ".m_subleg_com c ";
                    sql += "where c.slcd in('" + VE.T_TXN.SLCD + "',null) and ";
                    sql += "c.compcd in ('" + COM + "',null) ";
                    DataTable tbl = masterHelp.SQLquery(sql);
                    if (tbl.Rows.Count == 0)
                    {
                        ContentFlg = " Please add Tax/Price code master for this location "; goto dbnotsave;
                    }
                    parglcd = tbl.Rows[0]["parglcd"].retStr(); parclass1cd = tbl.Rows[0]["class1cd"].retStr();

                    //Calculate Others Amount from amount tab to distrubute into txndtl table
                    double _amtdist = 0, _baldist = 0, _rpldist = 0, _mult = 1;
                    double _amtdistq = 0, _baldistq = 0, _rpldistq = 0;
                    double titamt = 0, titqty = 0;
                    int lastitemno = 0;

                    if (VE.TBATCHDTL != null)
                    {
                        for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                        {
                            if (VE.TBATCHDTL[i].SLNO != 0 && VE.TBATCHDTL[i].ITCD != null)
                            {
                                #region Negetive Stock Chking
                                if (VE.MENU_PARA != "SR" && VE.MENU_PARA != "SBCMR" && VE.MENU_PARA != "PB" && VE.MENU_PARA != "REC" && VE.MENU_PARA != "OP" && VE.MENU_PARA != "OTH" && VE.MENU_PARA != "PJRC" && VE.TBATCHDTL[i].QNTY != 0)
                                {
                                    if (VE.TBATCHDTL[i].QNTY != 0)
                                    {
                                        var BALSTOCK = VE.TBATCHDTL[i].BALSTOCK.retDbl();
                                        var NEGSTOCK = VE.TBATCHDTL[i].NEGSTOCK;
                                        var balancestock = BALSTOCK - VE.TBATCHDTL[i].QNTY;
                                        if (balancestock < 0)
                                        {
                                            if (NEGSTOCK != "Y")
                                            {
                                                ContentFlg = "Quantity should not be grater than Stock at slno " + VE.TBATCHDTL[i].SLNO;
                                                goto dbnotsave;
                                            }
                                        }
                                    }
                                }

                                #endregion


                            }
                        }
                    }





                    double _baldist_b = 0, _baldistq_b = 0, _rpldist_b = 0, _rpldistq_b = 0, _baldisttxblval_b = 0, _rpldisttxblval_b = 0;
                    if (VE.TTXNAMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                        {
                            if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                            {
                                if (VE.TTXNAMT[i].ADDLESS == "L") _mult = -1; else _mult = 1;
                                if (VE.TTXNAMT[i].TAXCODE == "TC") _amtdistq = _amtdistq + (Convert.ToDouble(VE.TTXNAMT[i].AMT) * _mult);
                                else _amtdist = _amtdist + (Convert.ToDouble(VE.TTXNAMT[i].AMT) * _mult);
                            }
                        }
                    }
                    if (VE.TTXNDTL != null)
                    {
                        for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                        {
                            if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
                            {
                                titamt = titamt + VE.TTXNDTL[i].AMT.retDbl();
                                titqty = titqty + Convert.ToDouble(VE.TTXNDTL[i].QNTY);
                                lastitemno = i;
                            }
                        }
                    }
                    //
                    _baldist = _amtdist; _baldistq = _amtdistq;

                    #endregion
                    if (VE.DefaultAction == "A")
                    {
                        TTXN.EMD_NO = 0;
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        //TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        if (VE.M_SLIP_NO.retStr().Trim(' ') != "")
                        {
                            TTXN.DOCNO = Convert.ToString(VE.M_SLIP_NO).PadLeft(6, '0');
                        }
                        else
                        {
                            TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        }
                        DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                        if (DOCPATTERN.retStr().Length > 16 && (VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS"))
                        {
                            ContentFlg = "Document No. length should be less than 16.change from Document type master "; goto dbnotsave;
                        }
                        auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
                        TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                        Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        TempData["LASTGOCD" + VE.MENU_PARA] = VE.T_TXN.GOCD;
                        TempData["LASTROUNDOFF" + VE.MENU_PARA] = VE.RoundOff == true ? "Y" : "N";
                        TempData["LASTSLCD" + VE.MENU_PARA] = VE.T_TXN.SLCD;
                        TempData["LASTMERGEINDTL" + VE.MENU_PARA] = VE.MERGEINDTL == true ? "Y" : "N";
                        TempData["LASTSTKDRCR" + VE.MENU_PARA] = stkdrcr;
                        TempData["LASTBLTYPE" + VE.MENU_PARA] = VE.T_TXNOTH.BLTYPE;
                        TempData["LASTPARGLCD" + VE.MENU_PARA] = VE.T_TXN.PARGLCD;
                        TempData["LASTPAYTERMS" + VE.MENU_PARA] = VE.T_TXNOTH.PAYTERMS;
                        TempData["LASTSELBY" + VE.MENU_PARA] = VE.T_TXNOTH.SELBY;
                        TempData["LASTPACKBY" + VE.MENU_PARA] = VE.T_TXNOTH.PACKBY;
                        TempData["LASTDEALBY" + VE.MENU_PARA] = VE.T_TXNOTH.DEALBY;

                        //TCH = Cn.T_CONTROL_HDR(TTXN.DOCCD, TTXN.DOCDT, TTXN.DOCNO, TTXN.AUTONO, Month, DOCPATTERN, VE.DefaultAction, scm1, null, TTXN.SLCD, TTXN.BLAMT.Value, null);
                    }
                    else
                    {
                        if (!(VE.MENU_PARA == "PB" && VE.ChildData == "child record found"))
                        {
                            ContentFlg = ChildRecordCheck(VE.T_TXN.AUTONO);
                        }
                        if (ContentFlg != "") goto dbnotsave;
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        TTXN.DOCNO = VE.T_TXN.DOCNO;
                        TTXN.AUTONO = VE.T_TXN.AUTONO;
                        Month = VE.T_CNTRL_HDR.MNTHCD;
                        TTXN.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                        DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        TTXN.DTAG = "E";
                    }
                    #region check Packing Slip and Sales Bill (Agst Packing Slip) docno same
                    if (VE.MENU_PARA == "SB" && VE.T_TXN_LINKNO.LINKAUTONO.retStr() != "" && VE.DefaultAction == "A" && CommVar.ClientCode(UNQSNO) == "TRES")
                    {
                        var packslipdocno = DB1.T_CNTRL_HDR.Where(a => a.AUTONO == VE.T_TXN_LINKNO.LINKAUTONO).Select(a => a.DOCONLYNO).FirstOrDefault();
                        if (packslipdocno != TTXN.DOCNO)
                        {
                            ContentFlg = "Packing Slip Document No (" + packslipdocno + ") and Sales Bill (Agst Packing Slip) Document No (" + TTXN.DOCNO + ") not match !!";
                            goto dbnotsave;
                        }
                    }
                    #endregion
                    //TTXN.DOCTAG = VE.MENU_PARA.retStr().Length > 2 ? VE.MENU_PARA.retStr().Remove(2) : VE.MENU_PARA.retStr();
                    TTXN.DOCTAG = MenuDescription(VE.MENU_PARA).Rows[0]["DOCTAG"].ToString();
                    TTXN.SLCD = VE.T_TXN.SLCD;
                    TTXN.CONSLCD = VE.T_TXN.CONSLCD;
                    TTXN.CURR_CD = VE.T_TXN.CURR_CD;
                    TTXN.CURRRT = VE.T_TXN.CURRRT;
                    TTXN.BLAMT = VE.T_TXN.BLAMT;
                    TTXN.PREFDT = VE.T_TXN.PREFDT;
                    TTXN.PREFNO = VE.T_TXN.PREFNO;
                    TTXN.REVCHRG = VE.T_TXN.REVCHRG;
                    TTXN.ROAMT = VE.T_TXN.ROAMT;
                    if (VE.RoundOff == true) { TTXN.ROYN = "Y"; } else { TTXN.ROYN = "N"; }
                    TTXN.GOCD = VE.T_TXN.GOCD;
                    TTXN.JOBCD = VE.T_TXN.JOBCD;
                    TTXN.MANSLIPNO = VE.T_TXN.MANSLIPNO;
                    TTXN.DUEDAYS = VE.T_TXN.DUEDAYS;
                    TTXN.PARGLCD = parglcd; // VE.T_TXN.PARGLCD;
                    TTXN.GLCD = VE.T_TXN.GLCD;
                    TTXN.CLASS1CD = parclass1cd; // VE.T_TXN.CLASS1CD;
                    TTXN.CLASS2CD = VE.T_TXN.CLASS2CD;
                    TTXN.LINECD = VE.T_TXN.LINECD;
                    TTXN.BARGENTYPE = VE.T_TXN.BARGENTYPE;
                    //TTXN.WPPER = VE.T_TXN.WPPER;
                    //TTXN.RPPER = VE.T_TXN.RPPER;
                    TTXN.MENU_PARA = VE.T_TXN.MENU_PARA;
                    TTXN.TCSPER = VE.T_TXN.TCSPER;
                    TTXN.TCSAMT = VE.T_TXN.TCSAMT;
                    TTXN.TCSON = VE.T_TXN.TCSON;
                    TTXN.TDSCODE = VE.T_TXN.TDSCODE;
                    TTXN.JOBCD = VE.T_TXN.JOBCD;
                    TTXN.MERGEINDTL = VE.MERGEINDTL == true ? "Y" : "N";
                    if (VE.DefaultAction == "E")
                    {
                        dbsql = masterHelp.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_batchmst_price", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_stktrnf", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        //ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                        var comp = DB1.T_BATCHMST.Where(x => x.AUTONO == TTXN.AUTONO).OrderBy(s => s.AUTONO).ToList();
                        foreach (var v in comp)
                        {
                            if (!VE.TBATCHDTL.Where(s => s.BARNO == v.BARNO && s.SLNO == v.SLNO).Any())
                            {
                                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                                {
                                    dbsql = masterHelp.TblUpdt("T_BATCH_IMG_HDR_LINK", "", "E", "", "barno='" + v.BARNO + "'");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                    dbsql = masterHelp.TblUpdt("T_BATCH_IMG_HDR", "", "E", "", "barno='" + v.BARNO + "'");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                }

                                dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E", "S", "BARNO = '" + v.BARNO + "' and SLNO = '" + v.SLNO + "' ");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            }
                        }
                        //if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP")
                        //{
                        dbsql = masterHelp.TblUpdt("T_BALE", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("T_BALE_HDR", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        //}
                        dbsql = masterHelp.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txntrans", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        if (VE.MENU_PARA == "SBPOS")
                        {
                            dbsql = masterHelp.TblUpdt("t_txnmemo", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        }
                        dbsql = masterHelp.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        if (VE.MENU_PARA == "SB" || VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR")
                        {
                            dbsql = masterHelp.TblUpdt("t_txn_linkno", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        }
                        dbsql = masterHelp.TblUpdt("t_txnamt", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        //finance
                        if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "PJBR")
                        {
                            dbsql = masterHelp.finTblUpdt("t_vch_bl_adj", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        }
                        dbsql = masterHelp.finTblUpdt("t_vch_gst", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_bl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_class", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_det", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_hdr", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                    }

                    //-------------------------Transport--------------------------//
                    TXNTRANS.AUTONO = TTXN.AUTONO;
                    TXNTRANS.EMD_NO = TTXN.EMD_NO;
                    TXNTRANS.CLCD = TTXN.CLCD;
                    TXNTRANS.DTAG = TTXN.DTAG;
                    TXNTRANS.TRANSLCD = VE.T_TXNTRANS.TRANSLCD;
                    TXNTRANS.TRANSMODE = VE.T_TXNTRANS.TRANSMODE;
                    TXNTRANS.CRSLCD = VE.T_TXNTRANS.CRSLCD;
                    TXNTRANS.EWAYBILLNO = VE.T_TXNTRANS.EWAYBILLNO;
                    TXNTRANS.LRNO = VE.T_TXNTRANS.LRNO;
                    TXNTRANS.LRDT = VE.T_TXNTRANS.LRDT;
                    TXNTRANS.LORRYNO = VE.T_TXNTRANS.LORRYNO;
                    TXNTRANS.GRWT = VE.T_TXNTRANS.GRWT;
                    TXNTRANS.ACTFRGHTPAID = VE.T_TXNTRANS.ACTFRGHTPAID;
                    TXNTRANS.TRWT = VE.T_TXNTRANS.TRWT;
                    TXNTRANS.NTWT = VE.T_TXNTRANS.NTWT;
                    TXNTRANS.DESTN = VE.T_TXNTRANS.DESTN;
                    TXNTRANS.RECVPERSON = VE.T_TXNTRANS.RECVPERSON;
                    TXNTRANS.VECHLTYPE = VE.T_TXNTRANS.VECHLTYPE;
                    TXNTRANS.GATEENTNO = VE.T_TXNTRANS.GATEENTNO;
                    //----------------------------------------------------------//
                    //-------------------------Other Info--------------------------//
                    TTXNOTH.AUTONO = TTXN.AUTONO;
                    TTXNOTH.EMD_NO = TTXN.EMD_NO;
                    TTXNOTH.CLCD = TTXN.CLCD;
                    TTXNOTH.DTAG = TTXN.DTAG;
                    TTXNOTH.DOCREM = VE.T_TXNOTH.DOCREM;
                    TTXNOTH.DNCNCD = VE.T_TXNOTH.DNCNCD;
                    TTXNOTH.DNSALPUR = VE.T_TXNOTH.DNSALPUR;
                    TTXNOTH.AGSLCD = VE.T_TXNOTH.AGSLCD;
                    TTXNOTH.SAGSLCD = VE.T_TXNOTH.SAGSLCD;
                    TTXNOTH.BLTYPE = VE.T_TXNOTH.BLTYPE;
                    TTXNOTH.DESTN = VE.T_TXNOTH.DESTN;
                    TTXNOTH.PLSUPPLY = VE.T_TXNOTH.PLSUPPLY;
                    TTXNOTH.OTHADD1 = VE.T_TXNOTH.OTHADD1;
                    TTXNOTH.OTHADD2 = VE.T_TXNOTH.OTHADD2;
                    TTXNOTH.OTHADD3 = VE.T_TXNOTH.OTHADD3;
                    TTXNOTH.OTHADD4 = VE.T_TXNOTH.OTHADD4;
                    TTXNOTH.INSBY = VE.T_TXNOTH.INSBY;
                    TTXNOTH.PAYTERMS = VE.T_TXNOTH.PAYTERMS;
                    TTXNOTH.CASENOS = VE.T_TXNOTH.CASENOS;
                    TTXNOTH.NOOFCASES_REM = VE.T_TXNOTH.NOOFCASES_REM;
                    TTXNOTH.NOOFCASES = VE.T_TXNOTH.NOOFCASES;
                    TTXNOTH.PRCCD = VE.T_TXNOTH.PRCCD;
                    TTXNOTH.OTHNM = VE.T_TXNOTH.OTHNM;
                    TTXNOTH.COD = VE.T_TXNOTH.COD;
                    TTXNOTH.DOCTH = VE.T_TXNOTH.DOCTH;
                    TTXNOTH.POREFNO = VE.T_TXNOTH.POREFNO;
                    TTXNOTH.POREFDT = VE.T_TXNOTH.POREFDT;
                    TTXNOTH.ECOMM = VE.T_TXNOTH.ECOMM;
                    TTXNOTH.EXPCD = VE.T_TXNOTH.EXPCD;
                    TTXNOTH.GSTNO = VE.T_TXNOTH.GSTNO;
                    TTXNOTH.PNM = VE.T_TXNOTH.PNM;
                    TTXNOTH.POS = VE.T_TXNOTH.POS;
                    TTXNOTH.PACKBY = VE.T_TXNOTH.PACKBY;
                    TTXNOTH.SELBY = VE.T_TXNOTH.SELBY;
                    TTXNOTH.DEALBY = VE.T_TXNOTH.DEALBY;
                    TTXNOTH.DESPBY = VE.T_TXNOTH.DESPBY;
                    TTXNOTH.TAXGRPCD = VE.T_TXNOTH.TAXGRPCD;
                    TTXNOTH.TDSHD = VE.T_TXNOTH.TDSHD;
                    TTXNOTH.TDSON = VE.T_TXNOTH.TDSON;
                    TTXNOTH.TDSPER = VE.T_TXNOTH.TDSPER;
                    TTXNOTH.TDSAMT = VE.T_TXNOTH.TDSAMT;
                    TTXNOTH.POREFNO = VE.T_TXNOTH.POREFNO;
                    TTXNOTH.POREFDT = VE.T_TXNOTH.POREFDT;
                    TTXNOTH.MUTSLCD = VE.T_TXNOTH.MUTSLCD;
                    TTXNOTH.TOPAY = VE.T_TXNOTH.TOPAY;
                    //----------------------------------------------------------//

                    //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD, VE.DISPBLAMT.retDbl());
                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD, VE.DISPBLAMT.retDbl(), VE.Audit_REM);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(TTXN, VE.DefaultAction);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TXNTRANS);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TTXNOTH);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    if (VE.MENU_PARA == "SBPOS")
                    {
                        //--------------------------T_TXNMEMO--------------------------------//

                        TTXNMEMO.EMD_NO = TTXN.EMD_NO;
                        TTXNMEMO.CLCD = TTXN.CLCD;
                        TTXNMEMO.DTAG = TTXN.DTAG;
                        TTXNMEMO.TTAG = TTXN.TTAG;
                        TTXNMEMO.AUTONO = TTXN.AUTONO;
                        TTXNMEMO.RTDEBCD = VE.T_TXNMEMO.RTDEBCD;
                        TTXNMEMO.NM = VE.T_TXNMEMO.NM;
                        TTXNMEMO.MOBILE = VE.T_TXNMEMO.MOBILE;
                        TTXNMEMO.CITY = VE.T_TXNMEMO.CITY;
                        TTXNMEMO.ADDR = VE.T_TXNMEMO.ADDR;
                        dbsql = masterHelp.RetModeltoSql(TTXNMEMO);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        //----------------------------------------------------------//
                    }
                    //-------------------------SOCK TRANSFER IN OUT--------------------------//                                                        
                    if (VE.MENU_PARA == "ISS" || VE.MENU_PARA == "REC")
                    {
                        TSTKTRNF.EMD_NO = TTXN.EMD_NO;
                        TSTKTRNF.CLCD = TTXN.CLCD;
                        TSTKTRNF.DTAG = TTXN.DTAG;
                        TSTKTRNF.TTAG = TTXN.TTAG;
                        TSTKTRNF.SCOMPCD = VE.T_STKTRNF.SCOMPCD;
                        TSTKTRNF.TOCOMPCD = VE.T_STKTRNF.TOCOMPCD;
                        TSTKTRNF.SLOCCD = VE.T_STKTRNF.SLOCCD;
                        TSTKTRNF.TOLOCCD = VE.T_STKTRNF.TOLOCCD;
                        TSTKTRNF.SGOCD = VE.MENU_PARA == "ISS" ? TTXN.GOCD : VE.T_STKTRNF.SGOCD; //VE.T_TXN.GOCD;//
                        TSTKTRNF.TOGOCD = VE.MENU_PARA == "REC" ? TTXN.GOCD : VE.T_STKTRNF.TOGOCD;
                        TSTKTRNF.AUTONO = TTXN.AUTONO;
                        if (VE.MENU_PARA == "REC")
                        {
                            TSTKTRNF.OTHAUTONO = VE.T_STKTRNF.OTHAUTONO;
                        }
                        //----------------------------------------------------------//
                        dbsql = masterHelp.RetModeltoSql(TSTKTRNF);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    }
                    if (VE.MENU_PARA == "SB")
                    {
                        TTXNLINKNO.EMD_NO = TTXN.EMD_NO;
                        TTXNLINKNO.CLCD = TTXN.CLCD;
                        TTXNLINKNO.DTAG = TTXN.DTAG;
                        TTXNLINKNO.TTAG = TTXN.TTAG;
                        TTXNLINKNO.AUTONO = TTXN.AUTONO;
                        TTXNLINKNO.LINKAUTONO = VE.T_TXN_LINKNO.LINKAUTONO;
                    }
                    else if (VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR")
                    {
                        if (VE.PENDING_ISSUE != null)
                        {
                            VE.PENDING_ISSUE = VE.PENDING_ISSUE.Where(a => a.Checked == true).ToList();
                            for (int i = 0; i <= VE.PENDING_ISSUE.Count - 1; i++)
                            {
                                T_TXN_LINKNO TTXNLINKNO1 = new T_TXN_LINKNO();

                                TTXNLINKNO1.EMD_NO = TTXN.EMD_NO;
                                TTXNLINKNO1.CLCD = TTXN.CLCD;
                                TTXNLINKNO1.DTAG = TTXN.DTAG;
                                TTXNLINKNO1.TTAG = TTXN.TTAG;
                                TTXNLINKNO1.AUTONO = TTXN.AUTONO;
                                TTXNLINKNO1.LINKAUTONO = VE.PENDING_ISSUE[i].ISSUEAUTONO;

                                dbsql = masterHelp.RetModeltoSql(TTXNLINKNO1);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    if (VE.MENU_PARA == "SB")
                    {
                        dbsql = masterHelp.RetModeltoSql(TTXNLINKNO);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    }
                    // SAVE T_CNTRL_HDR_UNIQNO
                    string docbarcode = ""; string UNIQNO = salesfunc.retVchrUniqId(TTXN.DOCCD, TTXN.AUTONO);
                    sql = "select doccd,docbarcode from " + CommVar.CurSchema(UNQSNO) + ".m_doctype_bar where doccd='" + TTXN.DOCCD + "'";
                    DataTable dt = masterHelp.SQLquery(sql);
                    if (dt != null && dt.Rows.Count > 0) docbarcode = dt.Rows[0]["docbarcode"].retStr();
                    if (VE.DefaultAction == "A" && docbarcode.retStr() != "")
                    {
                        T_CNTRL_HDR_UNIQNO TCHUNIQNO = new T_CNTRL_HDR_UNIQNO();
                        TCHUNIQNO.CLCD = TTXN.CLCD;
                        TCHUNIQNO.AUTONO = TTXN.AUTONO;
                        TCHUNIQNO.EMD_NO = TTXN.EMD_NO;
                        TCHUNIQNO.DTAG = TTXN.DTAG;
                        TCHUNIQNO.UNIQNO = UNIQNO;
                        dbsql = masterHelp.RetModeltoSql(TCHUNIQNO);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    }
                    string lbatchini = "";
                    sql = "select lbatchini from " + CommVar.FinSchema(UNQSNO) + ".m_loca where loccd='" + CommVar.Loccd(UNQSNO) + "' and compcd='" + CommVar.Compcd(UNQSNO) + "'";
                    dt = masterHelp.SQLquery(sql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        lbatchini = dt.Rows[0]["lbatchini"].retStr();
                    }
                    //END T_CNTRL_HDR_UNIQNO 
                    // create header record in finschema
                    if (blactpost == true || blgstpost == true)
                    {
                        Cn.Create_DOCCD(UNQSNO, "F", TTXN.DOCCD);
                        //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD, VE.DISPBLAMT.retDbl());
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD, VE.DISPBLAMT.retDbl(), VE.Audit_REM);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        double currrt = 0;
                        if (TTXN.CURRRT != null) currrt = Convert.ToDouble(TTXN.CURRRT);
                        dbsql = masterHelp.InsVch_Hdr(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, null, null, "Y", null, trcd, "", "", TTXN.CURR_CD, currrt, "", revcharge);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        T_TXNEWB TTXNEWB = new T_TXNEWB();
                        string action = "A";

                        //ImprovarDB DBF1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                        var txnweb_data = DBF1.T_TXNEWB.Where(a => a.AUTONO == TTXN.AUTONO).Select(b => b.AUTONO).ToList();
                        if (txnweb_data != null && txnweb_data.Count > 0)
                        {
                            action = "E";
                        }

                        //-------------------------EWB--------------------------//
                        TTXNEWB.AUTONO = TTXN.AUTONO;
                        TTXNEWB.EMD_NO = TTXN.EMD_NO;
                        TTXNEWB.CLCD = TTXN.CLCD;
                        TTXNEWB.DTAG = TTXN.DTAG;
                        TTXNEWB.TRANSLCD = VE.T_TXNTRANS.TRANSLCD;
                        TTXNEWB.EWAYBILLNO = VE.T_TXNTRANS.EWAYBILLNO;
                        TTXNEWB.LRNO = VE.T_TXNTRANS.LRNO;
                        TTXNEWB.LRDT = VE.T_TXNTRANS.LRDT;
                        TTXNEWB.LORRYNO = VE.T_TXNTRANS.LORRYNO;
                        TTXNEWB.TRANSMODE = VE.T_TXNTRANS.TRANSMODE;
                        TTXNEWB.VECHLTYPE = VE.T_TXNTRANS.VECHLTYPE;
                        TTXNEWB.GOCD = TTXN.GOCD;// VE.T_TXN.GOCD;
                        //----------------------------------------------------------//

                        dbsql = masterHelp.RetModeltoSql(TTXNEWB, action, CommVar.FinSchema(UNQSNO));
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    }


                    VE.BALEYR = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).Year.retStr();
                    if (VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {
                        VE.BALEYR = (VE.BALEYR.retInt() - 1).retStr();
                    }

                    //if (balenocount > 0 && (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP"))
                    //if (balenocount > 0 && VE.T_TXN.GOCD != "SHOP")//shop logic for snfp,bale out two times
                    if (balenocount > 0 && TTXN.GOCD != "SHOP")//shop logic for snfp,bale out two times

                    {
                        T_BALE_HDR TBALEHDR = new T_BALE_HDR();
                        TBALEHDR.EMD_NO = TTXN.EMD_NO;
                        TBALEHDR.CLCD = TTXN.CLCD;
                        TBALEHDR.DTAG = TTXN.DTAG;
                        TBALEHDR.TTAG = TTXN.TTAG;
                        TBALEHDR.AUTONO = TTXN.AUTONO;
                        TBALEHDR.MUTSLCD = TXNTRANS.TRANSLCD;
                        //TBALEHDR.TREM = ;
                        //TBALEHDR.STARTNO = ;
                        TBALEHDR.TXTAG = TTXN.DOCTAG;

                        dbsql = masterHelp.RetModeltoSql(TBALEHDR);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    }
                    if (VE.TTXNDTL != null)
                    {
                        for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                        {
                            if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD.retStr() != "" && VE.TTXNDTL[i].MTRLJOBCD.retStr() != "" && VE.TTXNDTL[i].STKTYPE.retStr() != "")
                            {


                                //if (!(VE.MENU_PARA == "PJBL" && VE.TTXNDTL[i].FREESTK.retStr() == "Y"))
                                bool taxchking = true;
                                if (VE.MENU_PARA == "PJBL" && VE.TTXNDTL[i].FREESTK.retStr() == "Y") taxchking = false;
                                if (VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC" || VE.MENU_PARA == "PJIS" || VE.MENU_PARA == "PJRT") taxchking = false;
                                if (taxchking == true)
                                {
                                    if (VE.TTXNDTL[i].IGSTAMT.retDbl() + VE.TTXNDTL[i].CGSTAMT.retDbl() + VE.TTXNDTL[i].SGSTAMT.retDbl() == 0 && VE.T_TXN.REVCHRG != "N" && VE.MENU_PARA != "ISS" && VE.MENU_PARA != "REC")
                                    {
                                        ContentFlg = "TAX amount not found. Please add tax at slno " + VE.TTXNDTL[i].SLNO;
                                        goto dbnotsave;
                                    }
                                    else if (VE.TTXNDTL[i].CGSTAMT.retDbl() == 0 && VE.TTXNDTL[i].SGSTAMT.retDbl() != 0 && VE.T_TXN.REVCHRG != "N")
                                    {
                                        ContentFlg = "Cgst amount not found. Please add tax at slno " + VE.TTXNDTL[i].SLNO;
                                        goto dbnotsave;
                                    }
                                    else if (VE.TTXNDTL[i].CGSTAMT.retDbl() != 0 && VE.TTXNDTL[i].SGSTAMT.retDbl() == 0 && VE.T_TXN.REVCHRG != "N")
                                    {
                                        ContentFlg = "Sgst amount not found. Please add tax at slno " + VE.TTXNDTL[i].SLNO;
                                        goto dbnotsave;
                                    }
                                    if (VE.TTXNDTL[i].CGSTAMT.retDbl() != 0 && VE.TTXNDTL[i].SGSTAMT.retDbl() != 0 && VE.TTXNDTL[i].IGSTAMT.retDbl() != 0)
                                    {
                                        ContentFlg = "Cgst+Sgst+Igst 3 amount found. Please check tax at slno " + VE.TTXNDTL[i].SLNO;
                                        goto dbnotsave;
                                    }
                                }
                                if (VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "PI")
                                {
                                    if (VE.TTXNDTL[i].PRODGRPGSTPER.retStr() == "")
                                    {
                                        ContentFlg = "Please link up Product Group with Tax Rate for this Item (" + VE.TTXNDTL[i].ITSTYLE.retStr() + ") !!"; goto dbnotsave;
                                    }
                                }
                                if (i == lastitemno) { _rpldist = _baldist; _rpldistq = _baldistq; }
                                else
                                {
                                    if (_amtdist + _amtdistq == 0) { _rpldist = 0; _rpldistq = 0; }
                                    else
                                    {
                                        _rpldist = ((_amtdist / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                        _rpldistq = ((_amtdistq / titqty) * Convert.ToDouble(VE.TTXNDTL[i].QNTY)).toRound();
                                    }
                                }
                                _baldist = _baldist - _rpldist; _baldistq = _baldistq - _rpldistq;

                                COUNTER = COUNTER + 1;
                                T_TXNDTL TTXNDTL = new T_TXNDTL();
                                TTXNDTL.CLCD = TTXN.CLCD;
                                TTXNDTL.EMD_NO = TTXN.EMD_NO;
                                TTXNDTL.DTAG = TTXN.DTAG;
                                TTXNDTL.AUTONO = TTXN.AUTONO;
                                TTXNDTL.SLNO = VE.TTXNDTL[i].SLNO;
                                TTXNDTL.MTRLJOBCD = VE.TTXNDTL[i].MTRLJOBCD;
                                TTXNDTL.ITCD = VE.TTXNDTL[i].ITCD;
                                TTXNDTL.PARTCD = VE.TTXNDTL[i].PARTCD;
                                TTXNDTL.COLRCD = VE.TTXNDTL[i].COLRCD;
                                TTXNDTL.SIZECD = VE.TTXNDTL[i].SIZECD;
                                TTXNDTL.STKDRCR = stkdrcr;
                                TTXNDTL.STKTYPE = VE.TTXNDTL[i].STKTYPE;

                                TTXNDTL.HSNCODE = VE.TTXNDTL[i].HSNCODE;
                                if (minhsnlen != 0 && TTXNDTL.HSNCODE.retStr().Length < minhsnlen)
                                {
                                    ContentFlg = "HSNCODE(" + TTXNDTL.HSNCODE + ") less than " + minhsnlen + " at Item master at rowno:" + VE.TBATCHDTL[i].SLNO + " and itcd=" + VE.TBATCHDTL[i].ITCD + ". Make sure hsn length should be" + minhsnlen; goto dbnotsave;

                                }
                                TTXNDTL.ITREM = VE.TTXNDTL[i].ITREM;
                                TTXNDTL.PCSREM = VE.TTXNDTL[i].PCSREM;
                                TTXNDTL.FREESTK = VE.TTXNDTL[i].FREESTK;
                                TTXNDTL.BATCHNO = VE.TTXNDTL[i].BATCHNO;
                                //TTXNDTL.BALEYR = VE.TTXNDTL[i].BALENO.retStr() == "" ? "" : VE.BALEYR;// VE.TTXNDTL[i].BALEYR;
                                TTXNDTL.BALEYR = VE.TTXNDTL[i].BALENO.retStr() == "" ? "" : VE.TTXNDTL[i].BALEYR.retStr() == "" ? VE.BALEYR : VE.TTXNDTL[i].BALEYR;
                                TTXNDTL.BALENO = VE.TTXNDTL[i].BALENO;
                                TTXNDTL.GOCD = TTXN.GOCD;// VE.T_TXN.GOCD;
                                TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
                                TTXNDTL.NOS = (VE.TTXNDTL[i].UOM == "MTR" && VE.TTXNDTL[i].NOS.retDbl() == 0) ? 1 : VE.TTXNDTL[i].NOS; /*VE.TTXNDTL[i].NOS == null ? 0 : VE.TTXNDTL[i].NOS;*/
                                TTXNDTL.QNTY = VE.TTXNDTL[i].QNTY;
                                TTXNDTL.BLQNTY = VE.TTXNDTL[i].BLQNTY;
                                TTXNDTL.RATE = VE.TTXNDTL[i].RATE;
                                TTXNDTL.AMT = VE.TTXNDTL[i].AMT;
                                TTXNDTL.FLAGMTR = VE.TTXNDTL[i].FLAGMTR;
                                TTXNDTL.TOTDISCAMT = VE.TTXNDTL[i].TOTDISCAMT;
                                TTXNDTL.TXBLVAL = VE.TTXNDTL[i].TXBLVAL;
                                TTXNDTL.IGSTPER = VE.TTXNDTL[i].IGSTPER;
                                TTXNDTL.CGSTPER = VE.TTXNDTL[i].CGSTPER;
                                TTXNDTL.SGSTPER = VE.TTXNDTL[i].SGSTPER;
                                TTXNDTL.CESSPER = VE.TTXNDTL[i].CESSPER;
                                TTXNDTL.DUTYPER = VE.TTXNDTL[i].DUTYPER;
                                TTXNDTL.IGSTAMT = VE.TTXNDTL[i].IGSTAMT;
                                TTXNDTL.CGSTAMT = VE.TTXNDTL[i].CGSTAMT;
                                TTXNDTL.SGSTAMT = VE.TTXNDTL[i].SGSTAMT;
                                TTXNDTL.CESSAMT = VE.TTXNDTL[i].CESSAMT;
                                TTXNDTL.DUTYAMT = VE.TTXNDTL[i].DUTYAMT;
                                TTXNDTL.NETAMT = VE.TTXNDTL[i].NETAMT;
                                TTXNDTL.OTHRAMT = _rpldist + _rpldistq;
                                VE.TTXNDTL[i].OTHRAMT = _rpldist + _rpldistq;
                                TTXNDTL.SHORTQNTY = VE.TTXNDTL[i].SHORTQNTY;
                                TTXNDTL.DISCTYPE = VE.TTXNDTL[i].DISCTYPE;
                                TTXNDTL.DISCRATE = VE.TTXNDTL[i].DISCRATE;
                                TTXNDTL.DISCAMT = VE.TTXNDTL[i].DISCAMT;
                                TTXNDTL.SCMDISCTYPE = VE.TTXNDTL[i].SCMDISCTYPE;
                                TTXNDTL.SCMDISCRATE = VE.TTXNDTL[i].SCMDISCRATE;
                                TTXNDTL.SCMDISCAMT = VE.TTXNDTL[i].SCMDISCAMT;
                                TTXNDTL.TDDISCTYPE = VE.TTXNDTL[i].TDDISCTYPE;
                                TTXNDTL.TDDISCRATE = VE.TTXNDTL[i].TDDISCRATE;
                                TTXNDTL.TDDISCAMT = VE.TTXNDTL[i].TDDISCAMT;
                                TTXNDTL.PRCCD = VE.T_TXNOTH.PRCCD;
                                TTXNDTL.PRCEFFDT = VE.TTXNDTL[i].PRCEFFDT;
                                TTXNDTL.GLCD = VE.TTXNDTL[i].GLCD;
                                TTXNDTL.CLASS1CD = VE.TTXNDTL[i].CLASS1CD;

                                //if ((VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") && VE.TTXNDTL[i].AGDOCNO.retStr() != "" && VE.TTXNDTL[i].AGDOCDT.retStr() != "")
                                //{
                                if (!string.IsNullOrEmpty(VE.ADJWITH_BLNO) && !string.IsNullOrEmpty(VE.ADJWITH_BLDT.retStr()))
                                {
                                    TTXNDTL.AGDOCNO = VE.ADJWITH_BLNO;
                                    TTXNDTL.AGDOCDT = Convert.ToDateTime(VE.ADJWITH_BLDT);
                                }
                                else
                                {
                                    TTXNDTL.AGDOCNO = VE.TTXNDTL[i].AGDOCNO;
                                    TTXNDTL.AGDOCDT = VE.TTXNDTL[i].AGDOCDT;
                                }
                                // }
                                TTXNDTL.LISTPRICE = VE.TTXNDTL[i].LISTPRICE;
                                TTXNDTL.LISTDISCPER = VE.TTXNDTL[i].LISTDISCPER;
                                TTXNDTL.PAGENO = VE.TTXNDTL[i].PAGENO;
                                TTXNDTL.PAGESLNO = VE.TTXNDTL[i].PAGESLNO;
                                string BLUOMCD = "";
                                if (VE.TTXNDTL[i].BLUOMCD.retStr() != "")
                                {
                                    BLUOMCD = Regex.Replace(VE.TTXNDTL[i].BLUOMCD, @"[^A-Z]+", String.Empty);
                                    BLUOMCD = BLUOMCD.Trim();
                                }
                                TTXNDTL.BLUOMCD = BLUOMCD;
                                //TTXNDTL.BLUOMCD = VE.TTXNDTL[i].BLUOMCD;
                                dbsql = masterHelp.RetModeltoSql(TTXNDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                dbqty = dbqty + Convert.ToDouble(VE.TTXNDTL[i].QNTY);
                                igst = igst + Convert.ToDouble(VE.TTXNDTL[i].IGSTAMT);
                                cgst = cgst + Convert.ToDouble(VE.TTXNDTL[i].CGSTAMT);
                                sgst = sgst + Convert.ToDouble(VE.TTXNDTL[i].SGSTAMT);
                                cess = cess + Convert.ToDouble(VE.TTXNDTL[i].CESSAMT);
                                duty = duty + Convert.ToDouble(VE.TTXNDTL[i].DUTYAMT);

                                //if (VE.TTXNDTL[i].BALENO.retStr() != "" && (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP"))
                                //if (VE.TTXNDTL[i].BALENO.retStr() != "" && VE.T_TXN.GOCD != "SHOP")//shop logic for snfp,bale out two times
                                if (VE.TTXNDTL[i].BALENO.retStr() != "" && TTXN.GOCD != "SHOP")//shop logic for snfp,bale out two times
                                {
                                    T_BALE TBALE = new T_BALE();
                                    TBALE.EMD_NO = TTXN.EMD_NO;
                                    TBALE.CLCD = TTXN.CLCD;
                                    TBALE.DTAG = TTXN.DTAG;
                                    TBALE.TTAG = TTXN.TTAG;
                                    TBALE.AUTONO = TTXN.AUTONO;
                                    TBALE.SLNO = TTXNDTL.SLNO;
                                    TBALE.DRCR = TTXNDTL.STKDRCR;
                                    TBALE.GOCD = TTXN.GOCD;
                                    TBALE.BALENO = TTXNDTL.BALENO;
                                    if ((VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "SBPOS" || VE.MENU_PARA == "SR") && baledata.Rows.Count > 0)
                                    {
                                        string baleno = VE.TTXNDTL[i].BALENO.retStr();
                                        var data = baledata.Select("baleno = '" + baleno + "' and blslno = " + VE.TTXNDTL[i].RECPROGSLNO + "");
                                        if (data.Count() > 0)
                                        {
                                            TBALE.RSLNO = data[0]["RSLNO"].retShort();
                                            TBALE.BLAUTONO = data[0]["BLAUTONO"].retStr();
                                            TBALE.BLSLNO = data[0]["BLSLNO"].retShort();
                                            TBALE.LRDT = data[0]["LRDT"].retStr() == "" ? (DateTime?)null : Convert.ToDateTime(data[0]["LRDT"].retStr());
                                            TBALE.LRNO = data[0]["LRNO"].retStr();
                                            TBALE.BALEYR = data[0]["BALEYR"].retStr();
                                            TBALE.BALEOPEN = data[0]["BALEOPEN"].retStr();
                                        }
                                        else
                                        {
                                            TBALE.RSLNO = TTXNDTL.SLNO;
                                            TBALE.BLAUTONO = TTXN.AUTONO;
                                            TBALE.BLSLNO = TTXNDTL.SLNO;
                                            TBALE.LRDT = TXNTRANS.LRDT;
                                            TBALE.LRNO = TXNTRANS.LRNO;
                                            TBALE.BALEYR = TTXNDTL.BALEYR;
                                        }
                                    }
                                    else
                                    {
                                        TBALE.RSLNO = TTXNDTL.SLNO;
                                        TBALE.BLAUTONO = TTXN.AUTONO;
                                        TBALE.BLSLNO = TTXNDTL.SLNO;
                                        TBALE.LRDT = TXNTRANS.LRDT;
                                        TBALE.LRNO = TXNTRANS.LRNO;
                                        TBALE.BALEYR = TTXNDTL.BALEYR;
                                    }

                                    dbsql = masterHelp.RetModeltoSql(TBALE);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    COUNTER = 0; int COUNTERBATCH = 0; bool recoexist = false; bool isNewBatch = false;

                    _baldistq_b = 0; _baldist_b = 0; _baldisttxblval_b = 0;
                    if (VE.TBATCHDTL != null && VE.TBATCHDTL.Count > 0)
                    {
                        VE.TBATCHDTL.OrderBy(a => a.TXNSLNO);
                        int i = 0;
                        batchdtlstart:
                        while (i <= VE.TBATCHDTL.Count - 1)
                        {
                            if (VE.TBATCHDTL[i].ITCD.retStr() == "") { i++; goto batchdtlstart; }
                            int txnsln = VE.TBATCHDTL[i].TXNSLNO;
                            var TTXNDTLmp = (from x in VE.TTXNDTL
                                             where x.SLNO == VE.TBATCHDTL[i].TXNSLNO
                                             select new TTXNDTL
                                             {
                                                 TXBLVAL = x.TXBLVAL,
                                                 OTHRAMT = x.OTHRAMT,
                                                 QNTY = x.QNTY,
                                             }).FirstOrDefault();

                            _baldisttxblval_b = TTXNDTLmp.TXBLVAL.retDbl(); _baldist_b = TTXNDTLmp.OTHRAMT.retDbl();
                            while (VE.TBATCHDTL[i].TXNSLNO == txnsln)
                            {
                                if (VE.TBATCHDTL[i].ITCD.retStr() == "") { i++; goto batchdtlstart; }
                                int j = i;

                                int bi = 1, maxBi = 0;
                                while (VE.TBATCHDTL[i].TXNSLNO == txnsln)
                                {
                                    maxBi++;
                                    i++;
                                    if (i > VE.TBATCHDTL.Count - 1) break;
                                }
                                i = j;

                                if (bi == maxBi)
                                {
                                    _rpldisttxblval_b = _baldisttxblval_b; _rpldist_b = _baldist_b;
                                }
                                else
                                {
                                    _rpldisttxblval_b = ((TTXNDTLmp.TXBLVAL / TTXNDTLmp.QNTY) * VE.TBATCHDTL[i].QNTY).retDbl().toRound();
                                    _rpldist_b = ((TTXNDTLmp.OTHRAMT / TTXNDTLmp.QNTY) * VE.TBATCHDTL[i].QNTY).retDbl().toRound();
                                    _baldisttxblval_b = _baldisttxblval_b - _rpldisttxblval_b; _baldist_b = _baldist_b - _rpldist_b;
                                }

                                double mtrlcost = (((TTXNDTLmp.TXBLVAL + _amtdist) / TTXNDTLmp.QNTY) * VE.TBATCHDTL[i].QNTY).retDbl().toRound(2);
                                isNewBatch = false;
                                string barno = "";
                                //if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH") && (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E"))
                                //{
                                //    if (string.IsNullOrEmpty(VE.TBATCHDTL[i].BARNO))
                                //    {
                                //        barno = salesfunc.TranBarcodeGenerate(TTXN.DOCCD, lbatchini, docbarcode, UNIQNO, (VE.TBATCHDTL[i].SLNO));
                                //    }
                                //    else
                                //    {
                                //        barno = VE.TBATCHDTL[i].BARNO;
                                //    }
                                //}
                                string Action = "A", SqlCondition = "";
                                if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E"))
                                {
                                    if (VE.TBATCHDTL[i].COMMONUNIQBAR.retStr() != "E")
                                    {
                                        if (docbarcode.retStr() == "")
                                        {
                                            ContentFlg = "docbarcode not found in M_DOCTYPE_BAR table "; goto dbnotsave;
                                        }
                                        barno = salesfunc.TranBarcodeGenerate(TTXN.DOCCD, lbatchini, docbarcode, UNIQNO, (VE.TBATCHDTL[i].SLNO));
                                    }
                                    else
                                    {
                                        barno = VE.TBATCHDTL[i].BARNO;
                                        Action = "E";
                                        SqlCondition = "barno = '" + barno + "' and autono='" + TTXN.AUTONO + "' ";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(VE.TBATCHDTL[i].BARNO))
                                    {
                                        barno = salesfunc.GenerateBARNO(VE.TBATCHDTL[i].ITCD, VE.TBATCHDTL[i].CLRBARCODE, VE.TBATCHDTL[i].SZBARCODE);
                                    }
                                    else
                                    {
                                        barno = VE.TBATCHDTL[i].BARNO;
                                    }
                                    sql = "  select ITCD,BARNO from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHmst where barno='" + barno + "'";
                                    dt = masterHelp.SQLquery(sql);
                                    if (dt.Rows.Count == 0)
                                    {
                                        ContentFlg = "Barno:" + barno + " not found at Item master at rowno:" + VE.TBATCHDTL[i].SLNO + " and itcd=" + VE.TBATCHDTL[i].ITCD; goto dbnotsave;
                                    }
                                }

                                sql = "Select BARNO from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                OraCmd.CommandText = sql;
                                using (var OraReco = OraCmd.ExecuteReader())
                                {
                                    if (OraReco.HasRows == false) { isNewBatch = true; Action = "A"; }
                                }

                                //checking barno exist or not
                                if (Action == "E" && SqlCondition != "") isNewBatch = true;

                                if (isNewBatch == true)
                                {
                                    T_BATCHMST TBATCHMST = new T_BATCHMST();
                                    TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMST.CLCD = TTXN.CLCD;
                                    TBATCHMST.DTAG = TTXN.DTAG;
                                    TBATCHMST.TTAG = TTXN.TTAG;
                                    TBATCHMST.SLNO = VE.TBATCHDTL[i].SLNO; // ++COUNTERBATCH;
                                    TBATCHMST.AUTONO = TTXN.AUTONO;
                                    TBATCHMST.SLCD = TTXN.SLCD;
                                    TBATCHMST.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
                                    //TBATCHMST.STKTYPE = VE.TBATCHDTL[i].STKTYPE;
                                    TBATCHMST.JOBCD = TTXN.JOBCD;
                                    TBATCHMST.BARNO = barno;
                                    TBATCHMST.ITCD = VE.TBATCHDTL[i].ITCD;
                                    TBATCHMST.FABITCD = VE.TBATCHDTL[i].FABITCD;
                                    TBATCHMST.PARTCD = VE.TBATCHDTL[i].PARTCD;
                                    TBATCHMST.SIZECD = VE.TBATCHDTL[i].SIZECD;
                                    TBATCHMST.COLRCD = VE.TBATCHDTL[i].COLRCD;
                                    TBATCHMST.NOS = VE.TBATCHDTL[i].NOS;
                                    TBATCHMST.QNTY = VE.TBATCHDTL[i].QNTY;
                                    TBATCHMST.RATE = VE.TBATCHDTL[i].RATE;
                                    TBATCHMST.AMT = (VE.TBATCHDTL[i].QNTY.retDbl() * VE.TBATCHDTL[i].RATE.retDbl()).toRound();
                                    TBATCHMST.FLAGMTR = VE.TBATCHDTL[i].FLAGMTR;
                                    TBATCHMST.MTRL_COST = VE.TBATCHDTL[i].TXBLVAL;
                                    TBATCHMST.OTH_COST = VE.TBATCHDTL[i].OTHRAMT;
                                    TBATCHMST.ITREM = VE.TBATCHDTL[i].ITREM;
                                    TBATCHMST.PDESIGN = VE.TBATCHDTL[i].PDESIGN;
                                    if (VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                                    {
                                        TBATCHMST.ORDAUTONO = VE.TBATCHDTL[i].ORDAUTONO;
                                        TBATCHMST.ORDSLNO = VE.TBATCHDTL[i].ORDSLNO;
                                    }
                                    //TBATCHMST.ORGBATCHAUTONO = VE.TBATCHDTL[i].ORGBATCHAUTONO;
                                    //TBATCHMST.ORGBATCHSLNO = VE.TBATCHDTL[i].ORGBATCHSLNO;
                                    TBATCHMST.DIA = VE.TBATCHDTL[i].DIA;
                                    TBATCHMST.CUTLENGTH = VE.TBATCHDTL[i].CUTLENGTH;
                                    TBATCHMST.LOCABIN = VE.TBATCHDTL[i].LOCABIN;
                                    TBATCHMST.SHADE = VE.TBATCHDTL[i].SHADE;
                                    TBATCHMST.MILLNM = VE.TBATCHDTL[i].MILLNM;
                                    TBATCHMST.BATCHNO = VE.TBATCHDTL[i].BATCHNO;
                                    TBATCHMST.ORDAUTONO = VE.TBATCHDTL[i].ORDAUTONO;
                                    if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && ((VE.T_TXN.BARGENTYPE == "E") || (VE.T_TXN.BARGENTYPE == "C" && VE.TBATCHDTL[i].BARGENTYPE == "E")))
                                    {
                                        TBATCHMST.WPRATE = VE.TBATCHDTL[i].WPRATE;
                                        TBATCHMST.RPRATE = VE.TBATCHDTL[i].RPRATE;
                                    }
                                    //dbsql = masterHelp.RetModeltoSql(TBATCHMST);
                                    TBATCHMST.COMMONUNIQBAR = (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E") ? "E" : "";

                                    if (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E")
                                    {
                                        TBATCHMST.HSNCODE = VE.TBATCHDTL[i].HSNCODE;
                                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                                        {
                                            TBATCHMST.OURDESIGN = VE.TBATCHDTL[i].OURDESIGN;
                                        }
                                    }
                                    dbsql = masterHelp.RetModeltoSql(TBATCHMST, Action, "", SqlCondition);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                    if (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E")
                                    {
                                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                                        {
                                            if (VE.TBATCHDTL[i].BarImages.retStr() != "")
                                            {
                                                dbsql = masterHelp.TblUpdt("T_BATCH_IMG_HDR", "", "E", "", "barno='" + barno + "'");
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                                dbsql = masterHelp.TblUpdt("T_BATCH_IMG_HDR_LINK", "", "E", "", "barno='" + barno + "'");
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                                var barimg = SaveBarImage(VE.TBATCHDTL[i].BarImages, barno, TTXN.EMD_NO.retShort());
                                                dbsql = masterHelp.RetModeltoSql(barimg);

                                                for (int tr = 0; tr <= barimg.Item1.Count - 1; tr++)
                                                {
                                                    dbsql = masterHelp.RetModeltoSql(barimg.Item1[tr]);
                                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                                }
                                                var disntImgHdr = barimg.Item1.GroupBy(u => u.BARNO).Select(r => r.First()).ToList();
                                                foreach (var imgbar in disntImgHdr)
                                                {
                                                    T_BATCH_IMG_HDR_LINK m_batchImglink = new T_BATCH_IMG_HDR_LINK();
                                                    m_batchImglink.CLCD = TTXN.CLCD;
                                                    m_batchImglink.EMD_NO = TTXN.EMD_NO;
                                                    m_batchImglink.BARNO = imgbar.BARNO;
                                                    m_batchImglink.MAINBARNO = imgbar.BARNO;

                                                    dbsql = masterHelp.RetModeltoSql(m_batchImglink);
                                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }
                                bool pricedataexist = false;

                                sql = "Select BARNO from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHMST_PRICE where barno='" + barno + "' and effdt =to_date('" + TTXN.DOCDT.retDateStr() + "','dd/mm/yyyy') ";
                                OraCmd.CommandText = sql;
                                using (var OraReco = OraCmd.ExecuteReader())
                                {
                                    if (OraReco.HasRows == true) { pricedataexist = true; }
                                }


                                //add price
                                if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E") && pricedataexist == false)
                                {
                                    T_BATCHMST_PRICE TBATCHMSTPRICE = new T_BATCHMST_PRICE();
                                    TBATCHMSTPRICE.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMSTPRICE.CLCD = TTXN.CLCD;
                                    TBATCHMSTPRICE.DTAG = TTXN.DTAG;
                                    TBATCHMSTPRICE.TTAG = TTXN.TTAG;
                                    TBATCHMSTPRICE.BARNO = barno;
                                    TBATCHMSTPRICE.PRCCD = "CP";
                                    TBATCHMSTPRICE.EFFDT = Convert.ToDateTime(TTXN.DOCDT);
                                    TBATCHMSTPRICE.AUTONO = TTXN.AUTONO;
                                    TBATCHMSTPRICE.RATE = VE.TBATCHDTL[i].RATE;

                                    dbsql = masterHelp.RetModeltoSql(TBATCHMSTPRICE);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                    if (VE.TBATCHDTL[i].WPRATE.retDbl() != 0)
                                    {
                                        //WPRATE
                                        T_BATCHMST_PRICE TBATCHMSTPRICE1 = new T_BATCHMST_PRICE();
                                        TBATCHMSTPRICE1.EMD_NO = TTXN.EMD_NO;
                                        TBATCHMSTPRICE1.CLCD = TTXN.CLCD;
                                        TBATCHMSTPRICE1.DTAG = TTXN.DTAG;
                                        TBATCHMSTPRICE1.TTAG = TTXN.TTAG;
                                        TBATCHMSTPRICE1.BARNO = barno;
                                        TBATCHMSTPRICE1.PRCCD = "WP";
                                        TBATCHMSTPRICE1.EFFDT = Convert.ToDateTime(TTXN.DOCDT);
                                        TBATCHMSTPRICE1.AUTONO = TTXN.AUTONO;
                                        TBATCHMSTPRICE1.RATE = VE.TBATCHDTL[i].WPRATE;

                                        dbsql = masterHelp.RetModeltoSql(TBATCHMSTPRICE1);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }

                                    if (VE.TBATCHDTL[i].RPRATE.retDbl() != 0)
                                    {
                                        //RPRATE
                                        T_BATCHMST_PRICE TBATCHMSTPRICE2 = new T_BATCHMST_PRICE();
                                        TBATCHMSTPRICE2.EMD_NO = TTXN.EMD_NO;
                                        TBATCHMSTPRICE2.CLCD = TTXN.CLCD;
                                        TBATCHMSTPRICE2.DTAG = TTXN.DTAG;
                                        TBATCHMSTPRICE2.TTAG = TTXN.TTAG;
                                        TBATCHMSTPRICE2.BARNO = barno;
                                        TBATCHMSTPRICE2.PRCCD = "RP";
                                        TBATCHMSTPRICE2.EFFDT = Convert.ToDateTime(TTXN.DOCDT);
                                        TBATCHMSTPRICE2.AUTONO = TTXN.AUTONO;
                                        TBATCHMSTPRICE2.RATE = VE.TBATCHDTL[i].RPRATE;

                                        dbsql = masterHelp.RetModeltoSql(TBATCHMSTPRICE2);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }

                                COUNTER = COUNTER + 1;
                                T_BATCHDTL TBATCHDTL = new T_BATCHDTL();
                                TBATCHDTL.EMD_NO = TTXN.EMD_NO;
                                TBATCHDTL.CLCD = TTXN.CLCD;
                                TBATCHDTL.DTAG = TTXN.DTAG;
                                TBATCHDTL.TTAG = TTXN.TTAG;
                                TBATCHDTL.AUTONO = TTXN.AUTONO;
                                TBATCHDTL.TXNSLNO = VE.TBATCHDTL[i].TXNSLNO;
                                TBATCHDTL.SLNO = VE.TBATCHDTL[i].SLNO;  //COUNTER.retShort();
                                TBATCHDTL.GOCD = TTXN.GOCD;// VE.T_TXN.GOCD;
                                TBATCHDTL.BARNO = barno;
                                TBATCHDTL.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
                                TBATCHDTL.PARTCD = VE.TBATCHDTL[i].PARTCD;
                                TBATCHDTL.COLRCD = VE.TBATCHDTL[i].COLRCD;
                                TBATCHDTL.SIZECD = VE.TBATCHDTL[i].SIZECD;
                                TBATCHDTL.HSNCODE = VE.TBATCHDTL[i].HSNCODE;
                                TBATCHDTL.STKDRCR = stkdrcr;
                                TBATCHDTL.NOS = (VE.TBATCHDTL[i].UOM == "MTR" && VE.TBATCHDTL[i].NOS.retDbl() == 0) ? 1 : VE.TBATCHDTL[i].NOS; /*VE.TBATCHDTL[i].NOS;*/
                                TBATCHDTL.QNTY = VE.TBATCHDTL[i].QNTY;
                                TBATCHDTL.BLQNTY = VE.TBATCHDTL[i].BLQNTY;
                                TBATCHDTL.FLAGMTR = VE.TBATCHDTL[i].FLAGMTR;
                                TBATCHDTL.ITREM = VE.TBATCHDTL[i].ITREM;
                                TBATCHDTL.RATE = VE.TBATCHDTL[i].RATE;
                                TBATCHDTL.DISCRATE = VE.TBATCHDTL[i].DISCRATE;
                                TBATCHDTL.DISCTYPE = VE.TBATCHDTL[i].DISCTYPE;
                                TBATCHDTL.SCMDISCRATE = VE.TBATCHDTL[i].SCMDISCRATE;
                                TBATCHDTL.SCMDISCTYPE = VE.TBATCHDTL[i].SCMDISCTYPE;
                                TBATCHDTL.TDDISCRATE = VE.TBATCHDTL[i].TDDISCRATE;
                                TBATCHDTL.TDDISCTYPE = VE.TBATCHDTL[i].TDDISCTYPE;
                                TBATCHDTL.DIA = VE.TBATCHDTL[i].DIA;
                                TBATCHDTL.CUTLENGTH = VE.TBATCHDTL[i].CUTLENGTH;
                                TBATCHDTL.LOCABIN = VE.TBATCHDTL[i].LOCABIN;
                                TBATCHDTL.SHADE = VE.TBATCHDTL[i].SHADE;
                                TBATCHDTL.MILLNM = VE.TBATCHDTL[i].MILLNM;
                                TBATCHDTL.BATCHNO = VE.TBATCHDTL[i].BATCHNO;
                                //TBATCHDTL.BALEYR = VE.TBATCHDTL[i].BALENO.retStr() == "" ? "" : VE.BALEYR;// VE.TBATCHDTL[i].BALEYR;
                                TBATCHDTL.BALEYR = VE.TBATCHDTL[i].BALENO.retStr() == "" ? "" : VE.TBATCHDTL[i].BALEYR.retStr() == "" ? VE.BALEYR : VE.TBATCHDTL[i].BALEYR.retStr();
                                TBATCHDTL.BALENO = VE.TBATCHDTL[i].BALENO;
                                if (VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SB")
                                {
                                    TBATCHDTL.ORDAUTONO = VE.TBATCHDTL[i].ORDAUTONO;
                                    TBATCHDTL.ORDSLNO = VE.TBATCHDTL[i].ORDSLNO;
                                }
                                TBATCHDTL.LISTPRICE = VE.TBATCHDTL[i].LISTPRICE;
                                TBATCHDTL.LISTDISCPER = VE.TBATCHDTL[i].LISTDISCPER;
                                TBATCHDTL.CUTLENGTH = VE.TBATCHDTL[i].CUTLENGTH;
                                TBATCHDTL.STKTYPE = VE.TBATCHDTL[i].STKTYPE;

                                if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && MSYSCNFG.MNTNPCSTYPE == "Y")
                                {
                                    TBATCHDTL.PCSTYPE = VE.TBATCHDTL[i].PCSTYPE;
                                }
                                TBATCHDTL.OTHRAMT = _rpldist_b;
                                TBATCHDTL.TXBLVAL = _rpldisttxblval_b;
                                TBATCHDTL.STKTYPE = VE.TBATCHDTL[i].STKTYPE;
                                TBATCHDTL.RECPROGSLNO = VE.TBATCHDTL[i].RECPROGSLNO;
                                dbsql = masterHelp.RetModeltoSql(TBATCHDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


                                i++;
                                if (i > VE.TBATCHDTL.Count - 1) break;
                            }
                            //i++;
                            if (i > VE.TBATCHDTL.Count - 1) break;
                        }
                    }
                    if (isNewBatch == true && docbarcode.retStr() == "")
                    {
                        //ContentFlg = "Please add doccd[" + TTXN.DOCCD + "]  in M_DOCTYPE_BAR table"; goto dbnotsave;
                    }
                    if (dbqty == 0 && (VE.MENU_PARA != "SCN" && VE.MENU_PARA != "SDN" && VE.MENU_PARA != "PCN" && VE.MENU_PARA != "PDN"))
                    {
                        ContentFlg = "Quantity not entered"; goto dbnotsave;
                    }
                    if ((VE.MENU_PARA == "PB") && VE.T_TXN.PREFNO == null)
                    {
                        ContentFlg = "Purchase Bill No not entered"; goto dbnotsave;
                    }
                    isl = 1;

                    if (VE.TTXNAMT != null)
                    {
                        bool taxcheck = true;
                        if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC") && CommVar.ClientCode(UNQSNO) == "SNFP") taxcheck = false;
                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                        {
                            if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                            {
                                if (taxcheck == true)
                                {
                                    //if (VE.TTXNAMT[i].IGSTAMT.retDbl() + VE.TTXNAMT[i].CGSTAMT.retDbl() + VE.TTXNAMT[i].SGSTAMT.retDbl() == 0 && VE.T_TXN.REVCHRG != "N")
                                    if (VE.TTXNAMT[i].IGSTAMT.retDbl() + VE.TTXNAMT[i].CGSTAMT.retDbl() + VE.TTXNAMT[i].SGSTAMT.retDbl() == 0 && VE.T_TXN.REVCHRG != "N" && VE.MENU_PARA != "PB")//ANKAN WHATSAPP 06/05/2025 SHIPPING AMT W/O GST
                                    {
                                        ContentFlg = "TAX amount not found at amount tab. Please add tax at slno " + VE.TTXNAMT[i].SLNO;
                                        goto dbnotsave;
                                    }
                                    else if (VE.TTXNAMT[i].CGSTAMT.retDbl() == 0 && VE.TTXNAMT[i].SGSTAMT.retDbl() != 0 && VE.T_TXN.REVCHRG != "N")
                                    {
                                        ContentFlg = "Cgst amount not found at amount tab. Please add tax at slno " + VE.TTXNAMT[i].SLNO;
                                        goto dbnotsave;
                                    }
                                    else if (VE.TTXNAMT[i].CGSTAMT.retDbl() != 0 && VE.TTXNAMT[i].SGSTAMT.retDbl() == 0 && VE.T_TXN.REVCHRG != "N")
                                    {
                                        ContentFlg = "Sgst amount not found at amount tab. Please add tax at slno " + VE.TTXNAMT[i].SLNO;
                                        goto dbnotsave;
                                    }
                                }
                                T_TXNAMT TTXNAMT = new T_TXNAMT();
                                TTXNAMT.AUTONO = TTXN.AUTONO;
                                TTXNAMT.SLNO = VE.TTXNAMT[i].SLNO;
                                TTXNAMT.EMD_NO = TTXN.EMD_NO;
                                TTXNAMT.CLCD = TTXN.CLCD;
                                TTXNAMT.DTAG = TTXN.DTAG;
                                TTXNAMT.AMTCD = VE.TTXNAMT[i].AMTCD;
                                TTXNAMT.AMTDESC = VE.TTXNAMT[i].AMTDESC;
                                TTXNAMT.AMTRATE = VE.TTXNAMT[i].AMTRATE;
                                TTXNAMT.HSNCODE = VE.TTXNAMT[i].HSNCODE;
                                TTXNAMT.CURR_AMT = VE.TTXNAMT[i].CURR_AMT;
                                TTXNAMT.AMT = VE.TTXNAMT[i].AMT;
                                TTXNAMT.IGSTPER = VE.TTXNAMT[i].IGSTPER;
                                TTXNAMT.IGSTAMT = VE.TTXNAMT[i].IGSTAMT;
                                TTXNAMT.CGSTPER = VE.TTXNAMT[i].CGSTPER;
                                TTXNAMT.CGSTAMT = VE.TTXNAMT[i].CGSTAMT;
                                TTXNAMT.SGSTPER = VE.TTXNAMT[i].SGSTPER;
                                TTXNAMT.SGSTAMT = VE.TTXNAMT[i].SGSTAMT;
                                TTXNAMT.CESSPER = VE.TTXNAMT[i].CESSPER;
                                TTXNAMT.CESSAMT = VE.TTXNAMT[i].CESSAMT;
                                TTXNAMT.DUTYPER = VE.TTXNAMT[i].DUTYPER;
                                TTXNAMT.DUTYAMT = VE.TTXNAMT[i].DUTYAMT;
                                dbsql = masterHelp.RetModeltoSql(TTXNAMT);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                if (blactpost == true)
                                {
                                    isl = isl + 1;
                                    string adrcr = cr;
                                    if (VE.TTXNAMT[i].ADDLESS == "L") adrcr = dr;
                                    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, VE.TTXNAMT[i].GLCD, sslcd,
                                            Convert.ToDouble(VE.TTXNAMT[i].AMT), strrem, parglcd, TTXN.SLCD, 0, 0, VE.TTXNAMT[i].CURR_AMT.retDbl());
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    if (adrcr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);
                                    else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);

                                    if (VE.TTXNDTL != null && VE.TTXNDTL[0].CLASS1CD != null)
                                    {
                                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                                VE.TTXNDTL[0].CLASS1CD, "", Convert.ToDouble(VE.TTXNAMT[i].AMT), 0, strrem);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    }
                                }
                                int amult = 1;
                                if (VE.TTXNAMT[i].ADDLESS == "L") amult = -1;
                                igst = igst + Convert.ToDouble(VE.TTXNAMT[i].IGSTAMT * amult);
                                cgst = cgst + Convert.ToDouble(VE.TTXNAMT[i].CGSTAMT * amult);
                                sgst = sgst + Convert.ToDouble(VE.TTXNAMT[i].SGSTAMT * amult);
                                cess = cess + Convert.ToDouble(VE.TTXNAMT[i].CESSAMT * amult);
                                duty = duty + Convert.ToDouble(VE.TTXNAMT[i].DUTYAMT * amult);
                            }
                        }
                    }
                    #region Document Passing checking
                    //-----------------------DOCUMENT PASSING DATA---------------------------//
                    double TRAN_AMT = Convert.ToDouble(TTXN.BLAMT);
                    if (docpassrem != "" && VE.MENU_PARA == "SB")
                    {
                        docpassrem = "Doc # " + TTXN.DOCNO.ToString() + " Party Name # " + VE.SLNM + " [" + VE.SLAREA + "]".ToString() + "~" + docpassrem;
                        var TCDP_DATA1 = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, 0, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO), docpassrem);
                        if (TCDP_DATA1.Item1.Count != 0)
                        {
                            for (int tr = 0; tr <= TCDP_DATA1.Item1.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(TCDP_DATA1.Item1[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        var TCDP_DATA = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, TRAN_AMT, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO));
                        if (TCDP_DATA.Item1.Count != 0)
                        {
                            for (int tr = 0; tr <= TCDP_DATA.Item1.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(TCDP_DATA.Item1[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    #endregion

                    if (VE.UploadDOC != null)// add
                    {
                        var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TTXN.AUTONO, TTXN.EMD_NO.Value);
                        if (img.Item1.Count != 0)
                        {
                            for (int tr = 0; tr <= img.Item1.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(img.Item1[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                            for (int tr = 0; tr <= img.Item2.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(img.Item2[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    if (VE.T_CNTRL_HDR_REM != null && VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                    {
                        var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TTXN.AUTONO, TTXN.CLCD, TTXN.EMD_NO.Value);
                        if (NOTE.Item1.Count != 0)
                        {
                            for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(NOTE.Item1[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    #region Financial Posting
                    string salpur = "";
                    double itamt = 0;
                    if (blactpost == true)
                    {
                        if (VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "SR" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "SBPOS" || VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PJBR") salpur = "S";
                        else salpur = "P";
                        string prodrem = strrem; expglcd = "";
                        if (VE.TTXNDTL != null)
                        {
                            var AMTGLCD = (from x in VE.TTXNDTL
                                           group x by new { x.GLCD, x.CLASS1CD } into P
                                           select new
                                           {
                                               GLCD = P.Key.GLCD,
                                               CLASS1CD = P.Key.CLASS1CD,
                                               QTY = P.Sum(A => A.QNTY),
                                               TXBLVAL = P.Sum(A => A.TXBLVAL)
                                           }).ToList();

                            if (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN")
                            {
                                AMTGLCD = AMTGLCD.Where(a => a.TXBLVAL != 0).ToList();
                            }
                            else
                            {
                                AMTGLCD = AMTGLCD.Where(a => a.QTY != 0).ToList();
                            }
                            if (AMTGLCD != null && AMTGLCD.Count > 0)
                            {
                                for (int i = 0; i <= AMTGLCD.Count - 1; i++)
                                {
                                    isl = isl + 1;
                                    dbamt = AMTGLCD[i].TXBLVAL.retDbl();
                                    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), cr, AMTGLCD[i].GLCD, sslcd,
                                            dbamt, prodrem, parglcd, TTXN.SLCD, AMTGLCD[i].QTY.retDbl(), 0, 0);
                                    OraCmd.CommandText = dbsql;
                                    OraCmd.ExecuteNonQuery();
                                    if (cr == "D") dbDrAmt = dbDrAmt + dbamt;
                                    else dbCrAmt = dbCrAmt + dbamt;
                                    if (salesfunc.IsClassMandatoryInGlcd(AMTGLCD[i].GLCD) == "Y")
                                    {
                                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                            AMTGLCD[i].CLASS1CD, "", AMTGLCD[i].TXBLVAL.retDbl(), 0, strrem);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    }
                                    itamt = itamt + AMTGLCD[i].TXBLVAL.retDbl();
                                    expglcd = AMTGLCD[i].GLCD;
                                }
                            }
                        }
                        #region  GST Financial Part
                        string[] gstpostcd = new string[5];
                        double[] gstpostamt = new double[5];
                        for (int gt = 0; gt < 5; gt++)
                        {
                            gstpostcd[gt] = ""; gstpostamt[gt] = 0;
                        }

                        int gi = 0;
                        if (revcharge != "Y")
                        {
                            gstpostcd[gi] = tbl.Rows[0]["igstcd"].ToString(); gstpostamt[gi] = igst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cgstcd"].ToString(); gstpostamt[gi] = cgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["sgstcd"].ToString(); gstpostamt[gi] = sgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cesscd"].ToString(); gstpostamt[gi] = cess; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                        }
                        else
                        {
                            gstpostcd[gi] = tbl.Rows[0]["igst_rvi"].ToString(); gstpostamt[gi] = igst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cgst_rvi"].ToString(); gstpostamt[gi] = cgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["sgst_rvi"].ToString(); gstpostamt[gi] = sgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cess_rvi"].ToString(); gstpostamt[gi] = cess; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                        }

                        for (int gt = 0; gt < 5; gt++)
                        {
                            if (gstpostamt[gt] != 0)
                            {
                                isl = isl + 1;
                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), cr, gstpostcd[gt], sslcd,
                                        gstpostamt[gt], prodrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, dbqty, 0, 0);
                                OraCmd.CommandText = dbsql;
                                OraCmd.ExecuteNonQuery();
                                if (cr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
                                else dbCrAmt = dbCrAmt + gstpostamt[gt];
                                if (salesfunc.IsClassMandatoryInGlcd(gstpostcd[gt]) == "Y")
                                {
                                    dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                        tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        if (revcharge == "Y")
                        {
                            gi = 0;
                            gstpostcd[gi] = tbl.Rows[0]["igst_rvo"].ToString(); gstpostamt[gi] = igst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cgst_rvo"].ToString(); gstpostamt[gi] = cgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["sgst_rvo"].ToString(); gstpostamt[gi] = sgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cess_rvo"].ToString(); gstpostamt[gi] = cess; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                            for (int gt = 0; gt < 5; gt++)
                            {
                                if (gstpostamt[gt] != 0)
                                {
                                    isl = isl + 1;
                                    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr, gstpostcd[gt], null,
                                            gstpostamt[gt], prodrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, dbqty, 0, 0);
                                    OraCmd.CommandText = dbsql;
                                    OraCmd.ExecuteNonQuery();
                                    if (dr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
                                    else dbCrAmt = dbCrAmt + gstpostamt[gt];
                                    if (salesfunc.IsClassMandatoryInGlcd(gstpostcd[gt]) == "Y")
                                    {
                                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), null,
                                            tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        #endregion
                        // TCS
                        dbamt = Convert.ToDouble(VE.T_TXN.TCSAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;
                            //TCSPURGLCD
                            var TCSGLCD = VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" ? tbl.Rows[0]["TCSPURGLCD"].ToString() : tbl.Rows[0]["tcsgl"].ToString();
                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, TCSGLCD, sslcd,
                                    dbamt, strrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                            if (salesfunc.IsClassMandatoryInGlcd(TCSGLCD) == "Y")
                            {
                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                    null, null, Convert.ToDouble(VE.T_TXN.TCSAMT), 0, strrem + " TCS @ " + VE.T_TXN.TCSPER.ToString() + "%");
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            }
                        }


                        // Ronded off
                        dbamt = Convert.ToDouble(VE.T_TXN.ROAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;
                            if (dbamt < 0) adrcr = dr;
                            if (dbamt < 0) dbamt = dbamt * -1;

                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tbl.Rows[0]["rogl"].ToString(), null,
                                    dbamt, strrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                        }
                        //Party wise posting
                        int Partyisl = 1; //isl + 1;
                        dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(Partyisl), dr,
                            tbl.Rows[0]["parglcd"].ToString(), sslcd, Convert.ToDouble(VE.T_TXN.BLAMT), strrem, tbl.Rows[0]["prodglcd"].ToString(),
                            null, dbqty, 0, dbcurramt);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        if (dr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
                        else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
                        if (salesfunc.IsClassMandatoryInGlcd(tbl.Rows[0]["parglcd"].ToString()) == "Y")
                        {
                            dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(Partyisl), sslcd,
                                tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), Convert.ToDouble(VE.T_TXN.BLAMT), dbcurramt, strrem);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        }
                        strblno = ""; strbldt = ""; strduedt = ""; strvtype = ""; strrefno = "";
                        if (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "SR" || VE.MENU_PARA == "PJBR")
                        {
                            strvtype = "CN";
                        }
                        else if (VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PDN" || VE.MENU_PARA == "PR")
                        {
                            strvtype = "DN";
                        }
                        else
                        {
                            strvtype = "BL";
                        }
                        strduedt = Convert.ToDateTime(TTXN.DOCDT.Value).AddDays(Convert.ToDouble(TTXN.DUEDAYS)).ToString().retDateStr();
                        //if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "SR")
                        //if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                        //{
                        //    strblno = TTXN.PREFNO;
                        //    strbldt = TTXN.PREFDT.ToString();
                        //}
                        //else 
                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC" || VE.MENU_PARA == "PR")
                        {
                            if (TTXN.PREFNO.retStr() != "")
                            {
                                strblno = TTXN.PREFNO;
                                strbldt = TTXN.PREFDT.ToString();
                            }
                            else
                            {
                                strbldt = TTXN.DOCDT.ToString();
                                strblno = DOCPATTERN;
                            }
                        }
                        else
                        {
                            strbldt = TTXN.DOCDT.ToString();
                            strblno = DOCPATTERN;
                        }
                        string blconslcd = TTXN.CONSLCD;
                        if (TTXN.SLCD != sslcd) blconslcd = TTXN.SLCD;
                        if (blconslcd == sslcd) blconslcd = "";
                        string blrem = "";
                        double CASHDISCPR = 0;
                        if (VE.T_VCH_BL != null)
                        {
                            CASHDISCPR = VE.T_VCH_BL.CASHDISCPR.retDbl();
                        }
                        if (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN") blrem = VE.T_TXNOTH.DOCREM;
                        dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, dr,
                           tbl.Rows[0]["parglcd"].ToString(), sslcd, blconslcd, TTXNOTH.AGSLCD, tbl.Rows[0]["class1cd"].ToString(), Convert.ToSByte(Partyisl),
                           VE.T_TXN.BLAMT.retDbl(), strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), itamt, TTXNOTH.POREFNO,
                           TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), VE.T_TXN.BLAMT.retDbl(),
                           //VE.T_TXNTRANS.LRNO, VE.T_TXNTRANS.LRDT == null ? "" : VE.T_TXNTRANS.LRDT.ToString().retDateStr(), VE.TransporterName, "", "",
                           VE.T_TXNTRANS.LRNO, VE.T_TXNTRANS.LRDT == null ? "" : VE.T_TXNTRANS.LRDT.ToString().retDateStr(), VE.TRANSLNM, "", "",
                           VE.T_TXNOTH.BLTYPE, blrem, VE.T_TXNOTH.SAGSLCD, CASHDISCPR);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();


                        // TDS on purchase
                        if (VE.T_TDSTXN != null)
                        {
                            dbamt = VE.T_TDSTXN.TDSAMT.retDbl();
                        }
                        else
                        {
                            dbamt = 0;
                        }
                        if (dbamt != 0)
                        {
                            string adrcr = cr;
                            string tdsglcd = "";
                            sql = "select glcd from " + CommVar.FinSchema(UNQSNO) + ".m_tds_cntrl where tdscode='" + VE.T_TDSTXN.TDSCODE + "' and glcd is not null";
                            DataTable tmpdt = masterHelp.SQLquery(sql);
                            if (tmpdt.Rows.Count == 1) tdsglcd = tmpdt.Rows[0]["glcd"].retStr();

                            if (string.IsNullOrEmpty(tdsglcd))
                            {
                                ContentFlg = "tdsglcd Should not empty. Glcd not found at Tds master"; goto dbnotsave;
                            }
                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToInt16(isl), dr, tdsglcd, sslcd,
                                    dbamt, strrem + " TDS @ " + VE.T_TDSTXN.TDSPER.ToString() + "%", tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (salesfunc.IsClassMandatoryInGlcd(tdsglcd) == "Y")
                            {
                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToInt16(isl), sslcd,
                                    null, null, dbamt, 0, strrem + " TDS @ " + VE.T_TDSTXN.TDSPER.ToString() + "%");
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            }
                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToInt16(isl), cr, tbl.Rows[0]["parglcd"].ToString(), sslcd,
                                    dbamt, strrem + " TDS @ " + VE.T_TDSTXN.TDSPER.ToString() + "%", tdsglcd, TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (salesfunc.IsClassMandatoryInGlcd(tbl.Rows[0]["parglcd"].ToString()) == "Y")
                            {
                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToInt16(isl), sslcd,
                                    null, null, dbamt, 0, strrem + " TDS @ " + VE.T_TDSTXN.TDSPER.ToString() + "%");
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            }

                            string blconslcd1 = TTXN.CONSLCD;
                            if (TTXN.SLCD != sslcd) blconslcd1 = TTXN.SLCD;
                            if (blconslcd1 == sslcd) blconslcd1 = "";
                            dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                    tbl.Rows[0]["parglcd"].ToString(), sslcd, blconslcd1, TTXNOTH.AGSLCD, tbl.Rows[0]["class1cd"].ToString(), Convert.ToInt16(2),
                                    dbamt, strblno, strbldt, strrefno, strduedt, "TD", TTXN.DUEDAYS.Value, 0, TTXN.PREFNO,
                                    TTXN.PREFDT == null ? "" : TTXN.PREFDT.ToString().retDateStr(), 0,
                                    //VE.T_TXNTRANS.LRNO, VE.T_TXNTRANS.LRDT == null ? "" : VE.T_TXNTRANS.LRDT.ToString().retDateStr(), VE.TransporterName);
                                    VE.T_TXNTRANS.LRNO, VE.T_TXNTRANS.LRDT == null ? "" : VE.T_TXNTRANS.LRDT.ToString().retDateStr(), VE.TRANSLNM);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                            string blno = DOCPATTERN, bldt = TTXN.DOCDT.ToString().retDateStr(); double tdson = VE.T_TDSTXN.TDSON.retDbl();
                            dbsql = masterHelp.InsVch_TdsTxn(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, blno, bldt, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD,
                               VE.T_TDSTXN.TDSCODE, Convert.ToInt16(isl), TTXN.BLAMT.retDbl(), tdson, VE.T_TDSTXN.TDSPER.retDbl(), dbamt, bldt, null, "", "TDS");
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                            ////TVCHBLADJ1 start
                            T_VCH_BL_ADJ TVCHBLADJ1 = new T_VCH_BL_ADJ();
                            TVCHBLADJ1.EMD_NO = TTXN.EMD_NO;
                            TVCHBLADJ1.DTAG = TTXN.DTAG;
                            TVCHBLADJ1.CLCD = TTXN.CLCD;
                            TVCHBLADJ1.AUTONO = TTXN.AUTONO;
                            TVCHBLADJ1.SLNO = 1;
                            TVCHBLADJ1.I_AUTONO = TTXN.AUTONO;
                            TVCHBLADJ1.I_SLNO = 1;
                            TVCHBLADJ1.R_AUTONO = TTXN.AUTONO;
                            TVCHBLADJ1.R_SLNO = 2;
                            TVCHBLADJ1.I_AMT = TTXN.BLAMT;
                            TVCHBLADJ1.R_AMT = VE.T_TDSTXN.TDSAMT;
                            TVCHBLADJ1.ADJ_AMT = VE.T_TDSTXN.TDSAMT;
                            TVCHBLADJ1.FLAG = "TDS";
                            TVCHBLADJ1.PYMTREM = "TDS";
                            dbsql = masterHelp.RetModeltoSql(TVCHBLADJ1, "A", CommVar.FinSchema(UNQSNO));
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            ////TVCHBLADJ1 end
                        }
                    }
                    if (blgstpost == true)
                    {

                        #region TVCHGST Table update    

                        #region if amount tab Amount is negative 
                        //double? AmountTab_Txbl = 0; double CalcPropAmt = 0;

                        //if (VE.TTXNAMT != null)
                        //{

                        //    for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                        //    {
                        //        if (VE.TTXNAMT[i].HSNCODE == null && VE.TTXNAMT[i].ADDLESS == "L")
                        //        {
                        //            AmountTab_Txbl = AmountTab_Txbl + VE.TTXNAMT[i].AMT.retDbl();

                        //        }
                        //    }
                        //}
                        #endregion

                        int gs = 0;
                        string dncntag = ""; string exemptype = "";
                        if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "SRPOS" || VE.MENU_PARA == "SCN") dncntag = "SC";
                        if (VE.MENU_PARA == "SDN") dncntag = "SD";
                        if (VE.MENU_PARA == "PCN") dncntag = "PC";
                        if (VE.MENU_PARA == "PR" || VE.MENU_PARA == "PDN") dncntag = "PD";

                        double gblamt = TTXN.BLAMT.retDbl(); double groamt = TTXN.ROAMT.retDbl(); double gtcsamt = TTXN.TCSAMT.retDbl();
                        string pinv = VE.MENU_PARA == "PI" ? "Y" : "";
                        if (VE.TTXNDTL != null)
                        {
                            for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                            {
                                if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
                                {
                                    gs = gs + 1;
                                    if ((VE.TTXNDTL[i].IGSTPER.retDbl() + VE.TTXNDTL[i].CGSTPER.retDbl() + VE.TTXNDTL[i].SGSTPER.retDbl()) == 0)
                                    {
                                        exemptype = "N";
                                    }
                                    else
                                    {
                                        exemptype = "";
                                    }
                                    string SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT.retStr() == "" ? "" : VE.T_VCH_GST.SHIPDOCDT.retDateStr();

                                    T_VCH_GST TVCHGST = new T_VCH_GST();
                                    TVCHGST.EMD_NO = TTXN.EMD_NO;
                                    TVCHGST.CLCD = TTXN.CLCD;
                                    TVCHGST.DTAG = TTXN.DTAG;
                                    TVCHGST.AUTONO = TTXN.AUTONO;
                                    TVCHGST.DOCCD = TTXN.DOCCD;
                                    TVCHGST.DOCNO = TTXN.DOCNO;
                                    TVCHGST.DOCDT = TTXN.DOCDT;
                                    TVCHGST.DSLNO = isl.retShort();
                                    //TVCHGST.SLNO = gs.retShort();
                                    TVCHGST.SLNO = VE.TTXNDTL[i].SLNO.retShort();
                                    TVCHGST.PCODE = TTXN.SLCD;
                                    TVCHGST.BLNO = strblno;
                                    if (strbldt.retStr() != "")
                                    {
                                        TVCHGST.BLDT = Convert.ToDateTime(strbldt);
                                    }
                                    TVCHGST.HSNCODE = VE.TTXNDTL[i].HSNCODE;
                                    TVCHGST.ITNM = VE.TTXNDTL[i].ITSTYLE;

                                    #region if amount tab Amount is negative
                                    //CalcPropAmt = CalcPropAmt + (VE.T_GROSS_AMT.retDbl() - ((VE.TTXNDTL[i].TXBLVAL.retDbl() / VE.T_GROSS_AMT.retDbl()) * AmountTab_Txbl)).retDbl().toRound(2);
                                    //TVCHGST.AMT = (VE.T_GROSS_AMT.retDbl() - ((VE.TTXNDTL[i].TXBLVAL.retDbl() / VE.T_GROSS_AMT.retDbl()) * AmountTab_Txbl)).retDbl().toRound(2);
                                    #endregion
                                    TVCHGST.AMT = VE.TTXNDTL[i].TXBLVAL.retDbl();
                                    TVCHGST.CGSTPER = VE.TTXNDTL[i].CGSTPER.retDbl();
                                    TVCHGST.SGSTPER = VE.TTXNDTL[i].SGSTPER.retDbl();
                                    TVCHGST.IGSTPER = VE.TTXNDTL[i].IGSTPER.retDbl();
                                    TVCHGST.CGSTAMT = VE.TTXNDTL[i].CGSTAMT.retDbl();
                                    TVCHGST.SGSTAMT = VE.TTXNDTL[i].SGSTAMT.retDbl();
                                    TVCHGST.IGSTAMT = VE.TTXNDTL[i].IGSTAMT.retDbl();
                                    TVCHGST.CESSPER = VE.TTXNDTL[i].CESSPER.retDbl();
                                    TVCHGST.CESSAMT = VE.TTXNDTL[i].CESSAMT.retDbl();
                                    TVCHGST.DRCR = cr;
                                    TVCHGST.QNTY = (VE.TTXNDTL[i].BLQNTY.retDbl() == 0 ? VE.TTXNDTL[i].QNTY.retDbl() : VE.TTXNDTL[i].BLQNTY.retDbl());
                                    string BLUOMCD = "";
                                    if (VE.TTXNDTL[i].BLUOMCD.retStr() != "" && (VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "SR" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "PI" || VE.MENU_PARA == "SBPOS"))
                                    {
                                        BLUOMCD = Regex.Replace(VE.TTXNDTL[i].BLUOMCD, @"[^A-Z]+", String.Empty);
                                        BLUOMCD = BLUOMCD.Trim();
                                    }
                                    TVCHGST.UOM = BLUOMCD.retStr() != "" ? BLUOMCD : VE.TTXNDTL[i].UOM;
                                    //TVCHGST.UOM = VE.TTXNDTL[i].BLUOMCD.retStr() != "" ? VE.TTXNDTL[i].BLUOMCD : VE.TTXNDTL[i].UOM;
                                    TVCHGST.AGSTDOCNO = VE.TTXNDTL[i].AGDOCNO;
                                    TVCHGST.AGSTDOCDT = VE.TTXNDTL[i].AGDOCDT;
                                    //TVCHGST.SALPUR = salpur;
                                    TVCHGST.SALPUR = ((VE.MENU_PARA == "ISS" || VE.MENU_PARA == "REC") && VE.GSTNO.retStr() == CommVar.GSTNO(UNQSNO)) ? "" : salpur;
                                    TVCHGST.INVTYPECD = VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD;
                                    TVCHGST.DNCNCD = TTXNOTH.DNCNCD;
                                    TVCHGST.EXPCD = VE.T_VCH_GST.EXPCD;
                                    TVCHGST.GSTSLNM = VE.GSTSLNM;
                                    TVCHGST.SHIPDOCNO = VE.T_VCH_GST.SHIPDOCNO;
                                    TVCHGST.SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT;
                                    TVCHGST.PORTCD = VE.T_VCH_GST.PORTCD;
                                    TVCHGST.OTHRAMT = 0;
                                    TVCHGST.ROAMT = groamt;
                                    TVCHGST.BLAMT = gblamt;
                                    TVCHGST.DNCNSALPUR = dncntag;
                                    TVCHGST.CONSLCD = TTXN.CONSLCD;
                                    TVCHGST.APPLTAXRATE = 0;
                                    TVCHGST.EXEMPTEDTYPE = exemptype;
                                    //TVCHGST.EXPGLCD = expglcd;
                                    TVCHGST.EXPGLCD = (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "REC") ? VE.EXPGLCD : expglcd;
                                    TVCHGST.INPTCLAIM = "Y";
                                    TVCHGST.LUTNO = VE.T_VCH_GST.LUTNO;
                                    TVCHGST.LUTDT = VE.T_VCH_GST.LUTDT;
                                    TVCHGST.TCSPER = TTXN.TCSPER.retDbl();
                                    TVCHGST.TCSAMT = gtcsamt;
                                    TVCHGST.BASAMT = VE.TTXNDTL[i].AMT.retDbl();
                                    TVCHGST.DISCAMT = VE.TTXNDTL[i].TOTDISCAMT.retDbl();
                                    TVCHGST.RATE = VE.TTXNDTL[i].RATE.retDbl();
                                    TVCHGST.PINV = pinv;
                                    TVCHGST.GOOD_SERV = (VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR") ? "S" : "";
                                    if (VE.MENU_PARA == "PR" || VE.MENU_PARA == "SR")
                                    {
                                        TVCHGST.PREFDT = VE.T_TXN.PREFDT;
                                        TVCHGST.PREFNO = VE.T_TXN.PREFNO;
                                    }
                                    dbsql = masterHelp.RetModeltoSql(TVCHGST, "A", CommVar.FinSchema(UNQSNO));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    gblamt = 0; groamt = 0; gtcsamt = 0;

                                    if (gs == 1 && VE.ReturnAdjustwithBill == true)
                                    {
                                        #region if return then auto adjustment
                                        if ((VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "PJBR"))
                                        {
                                            DataTable OSDATA = masterHelp.GenOSTbl(tbl.Rows[0]["parglcd"].ToString(), VE.T_TXN.SLCD, VE.T_TXN.DOCDT.retDateStr(), "", "", "", "", "", "Y", "", "", "", "", "", false, false, "", "", "", "", VE.T_TXN.AUTONO, "");
                                            if (OSDATA.Rows.Count > 0)
                                            {
                                                string AGAUTONO = "", AGBLAMT = "";
                                                string agdocno = "", agdocdt = "";
                                                var cntagdocno = VE.TBATCHDTL.Select(a => a.AGDOCNO).Distinct().ToList();
                                                if (cntagdocno.Count > 1)
                                                {//allow only one against docno per invoice
                                                    agdocno = string.Join(",", VE.TBATCHDTL.Select(a => a.AGDOCNO).Distinct());
                                                    ContentFlg = "Allow only one Invoice (" + agdocno + ") to adjusting !!";
                                                    goto dbnotsave;
                                                }
                                                if (!string.IsNullOrEmpty(VE.ADJWITH_BLNO) && !string.IsNullOrEmpty(VE.ADJWITH_BLDT.retStr()))
                                                {
                                                    agdocno = VE.ADJWITH_BLNO; agdocdt = VE.ADJWITH_BLDT.retDateStr();
                                                }
                                                else
                                                {
                                                    agdocno = VE.TTXNDTL[i].AGDOCNO; agdocdt = VE.TTXNDTL[i].AGDOCDT.retDateStr();
                                                }
                                                sql = "select a.autono,a.blamt from " + scmf + ".t_vch_bl a ";
                                                sql += "where a.blno = '" + agdocno + "'  ";
                                                sql += "and a.bldt = to_date('" + agdocdt + "','dd/mm/yyyy')";

                                                DataTable dt1 = masterHelp.SQLquery(sql);
                                                if (dt1 != null && dt1.Rows.Count > 0)
                                                {
                                                    AGAUTONO = dt1.Rows[0]["autono"].retStr();
                                                    AGBLAMT = dt1.Rows[0]["BLAMT"].retStr();

                                                    dbsql = masterHelp.InsVch_Bl_Adj(TTXN.AUTONO, TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), AGAUTONO, 1, AGBLAMT.retDbl(), TTXN.AUTONO, 1, VE.T_TXN.BLAMT.retDbl(), VE.T_TXN.BLAMT.retDbl());
                                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                                }
                                            }//    if (OSDATA.Rows.Count > 0)
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                        string AGDOCNO = (VE.TTXNDTL != null) ? VE.TTXNDTL[0].AGDOCNO : "";

                        if (VE.TTXNAMT != null)
                        {
                            gs = VE.TTXNDTL.Select(a => a.SLNO).Max();
                            for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                            {

                                string good_serv = "G";
                                if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                                {
                                    #region //if amount tab Amount is negative then amount tab value should be skip .
                                    //if (VE.TTXNAMT[i].HSNCODE == null && VE.TTXNAMT[i].ADDLESS == "L")
                                    //{

                                    //}
                                    //else
                                    //{
                                    //    if (VE.TTXNAMT[i].HSNCODE.retStr().Length > 0)
                                    //    {
                                    //        good_serv = "S";
                                    //    }
                                    //    else
                                    //    {
                                    //        string HSNforAmount = "";
                                    //        HSNforAmount = (from a in VE.TTXNDTL
                                    //                        group a by new
                                    //                        {
                                    //                            HSNSACCD = a.HSNCODE
                                    //                        } into X
                                    //                        select new
                                    //                        {
                                    //                            HSNSACCD = X.Key.HSNSACCD,
                                    //                            AMOUNT = X.Sum(Z => Z.AMT).retDbl(),
                                    //                        }).OrderByDescending(a => a.AMOUNT).FirstOrDefault()?.HSNSACCD;
                                    //        VE.TTXNAMT[i].HSNCODE = HSNforAmount;
                                    //    }
                                    //    if ((VE.TTXNAMT[i].IGSTPER.retDbl() + VE.TTXNAMT[i].CGSTPER.retDbl() + VE.TTXNAMT[i].SGSTPER.retDbl()) == 0)
                                    //    {
                                    //        exemptype = "N";
                                    //    }
                                    //    else
                                    //    {
                                    //        exemptype = "";
                                    //    }
                                    //    int gmult = 1;
                                    //    if (VE.TTXNAMT[i].ADDLESS == "L") gmult = -1;
                                    //    gs = gs + 1;
                                    //    T_VCH_GST TVCHGST1 = new T_VCH_GST();
                                    //    TVCHGST1.EMD_NO = TTXN.EMD_NO;
                                    //    TVCHGST1.CLCD = TTXN.CLCD;
                                    //    TVCHGST1.DTAG = TTXN.DTAG;
                                    //    TVCHGST1.AUTONO = TTXN.AUTONO;
                                    //    TVCHGST1.DOCCD = TTXN.DOCCD;
                                    //    TVCHGST1.DOCNO = TTXN.DOCNO;
                                    //    TVCHGST1.DOCDT = TTXN.DOCDT;
                                    //    TVCHGST1.DSLNO = isl.retShort();
                                    //    TVCHGST1.SLNO = gs.retShort();
                                    //    TVCHGST1.PCODE = TTXN.SLCD;
                                    //    TVCHGST1.BLNO = strblno;
                                    //    if (strbldt.retStr() != "")
                                    //    {
                                    //        TVCHGST1.BLDT = Convert.ToDateTime(strbldt);
                                    //    }

                                    //    TVCHGST1.HSNCODE = VE.TTXNAMT[i].HSNCODE;
                                    //    TVCHGST1.ITNM = VE.TTXNAMT[i].AMTNM;
                                    //    TVCHGST1.AMT = VE.TTXNAMT[i].AMT.retDbl() * gmult;
                                    //    TVCHGST1.CGSTPER = VE.TTXNAMT[i].CGSTPER.retDbl();
                                    //    TVCHGST1.SGSTPER = VE.TTXNAMT[i].SGSTPER.retDbl();
                                    //    TVCHGST1.IGSTPER = VE.TTXNAMT[i].IGSTPER.retDbl();
                                    //    TVCHGST1.CGSTAMT = VE.TTXNAMT[i].CGSTAMT.retDbl() * gmult;
                                    //    TVCHGST1.SGSTAMT = VE.TTXNAMT[i].SGSTAMT.retDbl() * gmult;
                                    //    TVCHGST1.IGSTAMT = VE.TTXNAMT[i].IGSTAMT.retDbl() * gmult;
                                    //    TVCHGST1.CESSPER = VE.TTXNAMT[i].CESSPER.retDbl();
                                    //    TVCHGST1.CESSAMT = VE.TTXNAMT[i].CESSAMT.retDbl() * gmult;
                                    //    TVCHGST1.DRCR = cr;
                                    //    TVCHGST1.QNTY = 0;
                                    //    TVCHGST1.UOM = "OTH";
                                    //    TVCHGST1.AGSTDOCNO = AGDOCNO;
                                    //    if (VE.TTXNDTL != null)
                                    //    {
                                    //        TVCHGST1.AGSTDOCDT = VE.TTXNDTL[0].AGDOCDT;
                                    //    }
                                    //    TVCHGST1.SALPUR = salpur;
                                    //    TVCHGST1.INVTYPECD = VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD;
                                    //    TVCHGST1.DNCNCD = TTXNOTH.DNCNCD;
                                    //    TVCHGST1.EXPCD = VE.T_VCH_GST.EXPCD;
                                    //    TVCHGST1.GSTSLNM = VE.GSTSLNM;
                                    //    TVCHGST1.SHIPDOCNO = VE.T_VCH_GST.SHIPDOCNO;
                                    //    TVCHGST1.SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT;
                                    //    TVCHGST1.PORTCD = VE.T_VCH_GST.PORTCD;
                                    //    TVCHGST1.OTHRAMT = 0;
                                    //    TVCHGST1.ROAMT = groamt;
                                    //    TVCHGST1.BLAMT = gblamt;
                                    //    TVCHGST1.DNCNSALPUR = dncntag;
                                    //    TVCHGST1.CONSLCD = TTXN.CONSLCD;
                                    //    TVCHGST1.APPLTAXRATE = 0;
                                    //    TVCHGST1.EXEMPTEDTYPE = exemptype;
                                    //    TVCHGST1.GOOD_SERV = (VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR") ? "S" : good_serv;
                                    //    //TVCHGST1.EXPGLCD = ((VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN") && (VE.TBATCHDTL == null && VE.TTXNDTL == null)) ? VE.TTXNAMT[0].GLCD : expglcd;
                                    //    TVCHGST1.EXPGLCD = (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN") ? VE.EXPGLCD : expglcd;
                                    //    TVCHGST1.INPTCLAIM = "Y";
                                    //    TVCHGST1.LUTNO = VE.T_VCH_GST.LUTNO;
                                    //    TVCHGST1.LUTDT = VE.T_VCH_GST.LUTDT;
                                    //    TVCHGST1.TCSPER = TTXN.TCSPER.retDbl();
                                    //    TVCHGST1.TCSAMT = gtcsamt.retDbl();
                                    //    TVCHGST1.BASAMT = VE.TTXNAMT[i].AMT.retDbl() * gmult;
                                    //    TVCHGST1.RATE = VE.TTXNAMT[i].AMTRATE.retDbl();
                                    //    TVCHGST1.PINV = pinv;
                                    //    if (VE.MENU_PARA == "PR")
                                    //    {
                                    //        TVCHGST1.PREFDT = VE.T_TXN.PREFDT;
                                    //        TVCHGST1.PREFNO = VE.T_TXN.PREFNO;
                                    //    }
                                    //    dbsql = masterHelp.RetModeltoSql(TVCHGST1, "A", CommVar.FinSchema(UNQSNO));
                                    //    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    //    gblamt = 0; groamt = 0;
                                    //}

                                    #endregion


                                    if (VE.TTXNAMT[i].HSNCODE.retStr().Length > 0)
                                    {
                                        good_serv = "S";
                                    }
                                    else
                                    {
                                        string HSNforAmount = "";
                                        HSNforAmount = (from a in VE.TTXNDTL
                                                        group a by new
                                                        {
                                                            HSNSACCD = a.HSNCODE
                                                        } into X
                                                        select new
                                                        {
                                                            HSNSACCD = X.Key.HSNSACCD,
                                                            AMOUNT = X.Sum(Z => Z.AMT).retDbl(),
                                                        }).OrderByDescending(a => a.AMOUNT).FirstOrDefault()?.HSNSACCD;
                                        VE.TTXNAMT[i].HSNCODE = HSNforAmount;
                                    }
                                    if ((VE.TTXNAMT[i].IGSTPER.retDbl() + VE.TTXNAMT[i].CGSTPER.retDbl() + VE.TTXNAMT[i].SGSTPER.retDbl()) == 0)
                                    {
                                        exemptype = "N";
                                    }
                                    else
                                    {
                                        exemptype = "";
                                    }
                                    int gmult = 1;
                                    if (VE.TTXNAMT[i].ADDLESS == "L") gmult = -1;
                                    gs = gs + 1;
                                    T_VCH_GST TVCHGST1 = new T_VCH_GST();
                                    TVCHGST1.EMD_NO = TTXN.EMD_NO;
                                    TVCHGST1.CLCD = TTXN.CLCD;
                                    TVCHGST1.DTAG = TTXN.DTAG;
                                    TVCHGST1.AUTONO = TTXN.AUTONO;
                                    TVCHGST1.DOCCD = TTXN.DOCCD;
                                    TVCHGST1.DOCNO = TTXN.DOCNO;
                                    TVCHGST1.DOCDT = TTXN.DOCDT;
                                    TVCHGST1.DSLNO = isl.retShort();
                                    TVCHGST1.SLNO = gs.retShort();
                                    TVCHGST1.PCODE = TTXN.SLCD;
                                    TVCHGST1.BLNO = strblno;
                                    if (strbldt.retStr() != "")
                                    {
                                        TVCHGST1.BLDT = Convert.ToDateTime(strbldt);
                                    }

                                    TVCHGST1.HSNCODE = VE.TTXNAMT[i].HSNCODE;
                                    TVCHGST1.ITNM = VE.TTXNAMT[i].AMTNM;
                                    TVCHGST1.AMT = VE.TTXNAMT[i].AMT.retDbl() * gmult;
                                    TVCHGST1.CGSTPER = VE.TTXNAMT[i].CGSTPER.retDbl();
                                    TVCHGST1.SGSTPER = VE.TTXNAMT[i].SGSTPER.retDbl();
                                    TVCHGST1.IGSTPER = VE.TTXNAMT[i].IGSTPER.retDbl();
                                    TVCHGST1.CGSTAMT = VE.TTXNAMT[i].CGSTAMT.retDbl() * gmult;
                                    TVCHGST1.SGSTAMT = VE.TTXNAMT[i].SGSTAMT.retDbl() * gmult;
                                    TVCHGST1.IGSTAMT = VE.TTXNAMT[i].IGSTAMT.retDbl() * gmult;
                                    TVCHGST1.CESSPER = VE.TTXNAMT[i].CESSPER.retDbl();
                                    TVCHGST1.CESSAMT = VE.TTXNAMT[i].CESSAMT.retDbl() * gmult;
                                    TVCHGST1.DRCR = cr;
                                    TVCHGST1.QNTY = 0;
                                    TVCHGST1.UOM = "OTH";
                                    TVCHGST1.AGSTDOCNO = AGDOCNO;
                                    if (VE.TTXNDTL != null)
                                    {
                                        TVCHGST1.AGSTDOCDT = VE.TTXNDTL[0].AGDOCDT;
                                    }
                                    //TVCHGST1.SALPUR = salpur;
                                    TVCHGST1.SALPUR = ((VE.MENU_PARA == "ISS" || VE.MENU_PARA == "REC") && VE.GSTNO.retStr() == CommVar.GSTNO(UNQSNO)) ? "" : salpur;
                                    TVCHGST1.INVTYPECD = VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD;
                                    TVCHGST1.DNCNCD = TTXNOTH.DNCNCD;
                                    TVCHGST1.EXPCD = VE.T_VCH_GST.EXPCD;
                                    TVCHGST1.GSTSLNM = VE.GSTSLNM;
                                    TVCHGST1.SHIPDOCNO = VE.T_VCH_GST.SHIPDOCNO;
                                    TVCHGST1.SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT;
                                    TVCHGST1.PORTCD = VE.T_VCH_GST.PORTCD;
                                    TVCHGST1.OTHRAMT = 0;
                                    TVCHGST1.ROAMT = groamt;
                                    TVCHGST1.BLAMT = gblamt;
                                    TVCHGST1.DNCNSALPUR = dncntag;
                                    TVCHGST1.CONSLCD = TTXN.CONSLCD;
                                    TVCHGST1.APPLTAXRATE = 0;
                                    TVCHGST1.EXEMPTEDTYPE = exemptype;
                                    TVCHGST1.GOOD_SERV = (VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR") ? "S" : good_serv;
                                    //TVCHGST1.EXPGLCD = ((VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN") && (VE.TBATCHDTL == null && VE.TTXNDTL == null)) ? VE.TTXNAMT[0].GLCD : expglcd;
                                    TVCHGST1.EXPGLCD = (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "REC") ? VE.EXPGLCD : expglcd;
                                    TVCHGST1.INPTCLAIM = "Y";
                                    TVCHGST1.LUTNO = VE.T_VCH_GST.LUTNO;
                                    TVCHGST1.LUTDT = VE.T_VCH_GST.LUTDT;
                                    TVCHGST1.TCSPER = TTXN.TCSPER.retDbl();
                                    TVCHGST1.TCSAMT = gtcsamt.retDbl();
                                    TVCHGST1.BASAMT = VE.TTXNAMT[i].AMT.retDbl() * gmult;
                                    TVCHGST1.RATE = VE.TTXNAMT[i].AMTRATE.retDbl();
                                    TVCHGST1.PINV = pinv;
                                    if (VE.MENU_PARA == "PR" || VE.MENU_PARA == "SR")
                                    {
                                        TVCHGST1.PREFDT = VE.T_TXN.PREFDT;
                                        TVCHGST1.PREFNO = VE.T_TXN.PREFNO;
                                    }
                                    dbsql = masterHelp.RetModeltoSql(TVCHGST1, "A", CommVar.FinSchema(UNQSNO));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    gblamt = 0; groamt = 0;
                                }
                            }
                        }


                        #endregion
                    }
                    if (VE.MENU_PARA != "OP" && VE.MENU_PARA != "OTH" && VE.MENU_PARA != "PJRC" && VE.MENU_PARA != "PJIS" && VE.MENU_PARA != "PJRT" && VE.MENU_PARA != "PJBL" && VE.MENU_PARA != "PJBR")
                    {
                        if (igst != 0 && (cgst + sgst) != 0)
                        {
                            ContentFlg = "We can't add igst+cgst+sgst for the same party.";
                            goto dbnotsave;
                        }
                        else if (igst + cgst + sgst == 0 && VE.T_TXN.REVCHRG != "N" && VE.MENU_PARA != "ISS" && VE.MENU_PARA != "REC")
                        {
                            //if (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN")
                            //{
                            //    ContentFlg = "Please enter tax % in Amount tab";
                            //}
                            //else
                            //{
                            ContentFlg = "TAX amount not found. Please add tax with item.";
                            //}
                            goto dbnotsave;
                        }
                    }
                    if (Math.Round(dbDrAmt, 2) != Math.Round(dbCrAmt, 2))
                    {
                        ContentFlg = "Debit " + Math.Round(dbDrAmt, 2) + " & Credit " + Math.Round(dbCrAmt, 2) + " not matching please click on rounded off...";
                        goto dbnotsave;
                    }

                    #endregion

                    #region STOCK CHECKING
                    //if ((VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "SBPOS"))
                    if ((VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "SBPOS" || VE.MENU_PARA == "REC"))
                    {
                        bool stockcheck = true;
                        if (VE.MENU_PARA == "PI") stockcheck = false;
                        else if (VE.MENU_PARA == "SR" && VE.DefaultAction == "A") stockcheck = false;
                        else if (VE.MENU_PARA == "REC" && VE.DefaultAction == "A") stockcheck = false;
                        //if (VE.MENU_PARA != "PI")
                        if (stockcheck == true)
                        {
                            GRIDDATA();
                            if (VE.TBATCHDTL != null && VE.TBATCHDTL.Count > 0)
                            {
                                for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                                {
                                    if (VE.TBATCHDTL[i].ITCD.retStr() != "")
                                    {


                                        DataRow ROWDATA = ITEMDT.NewRow();
                                        ROWDATA["ITCD"] = VE.TBATCHDTL[i].ITCD.retStr();
                                        ROWDATA["BALENO"] = VE.TBATCHDTL[i].BALENO.retStr();
                                        ROWDATA["QNTY"] = VE.TBATCHDTL[i].QNTY.retDbl();


                                        ITEMDT.Rows.Add(ROWDATA);

                                    }
                                }
                            }
                            string GCS = Cn.GCS();
                            //var ITCD = string.Join(",", (from Z in VE.TTXNDTL where Z.ITCD.retStr() != "" select "'" + Z.ITCD + "'").Distinct());
                            var ITCDList = (from Z in VE.TBATCHDTL where Z.ITCD.retStr() != "" select Z.ITCD).Distinct().ToArray();
                            // var linkitcd = (from Z in VE.TBATCHDTL where Z.LINKITCD.retStr() != "" select Z.LINKITCD).Distinct().ToArray();
                            var ITCD = ITCDList.retSqlfromStrarray(); //+ (linkitcd.Count() > 0 ? "," + linkitcd.retSqlfromStrarray() : "");
                            string BARNO = (from a in VE.TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
                            string ITGRPCD = (from a in VE.TBATCHDTL select a.ITGRPCD).Distinct().ToArray().retSqlfromStrarray();
                            var stocktype = string.Join(",", (from Z in VE.TBATCHDTL where Z.STKTYPE.retStr() != "" select "'" + Z.STKTYPE + "'").Distinct());
                            var bale = (from a in VE.TBATCHDTL select a.BALENO).ToList();

                            var BALENO = (from a in VE.TBATCHDTL where a.BALENO != null select a.BALENO + GCS + a.BALEYR).Distinct().ToArray().retSqlfromStrarray();
                            if (BALENO.retStr() != "")
                            {
                                var balno = string.Join("", BALENO.Split(Convert.ToChar(Cn.GCS())));

                                var mtrljobcd = VE.MTRLJOBCD.retStr();
                                if (mtrljobcd.retStr() == "")
                                {
                                    mtrljobcd = "'FS'";
                                }
                                else
                                {
                                    mtrljobcd = mtrljobcd.retSqlformat();

                                }
                                DataTable ITEM_STOCK_DATA = new DataTable();

                                //ITEM_STOCK_DATA = salesfunc.GetBaleStock(VE.T_TXN.DOCDT.retDateStr(), TTXN.GOCD.retSqlformat(), balno, ITCD, mtrljobcd, VE.DefaultAction == "E" ? TTXN.AUTONO : "", ITGRPCD, "", "", "", false, "", "", true);
                                ITEM_STOCK_DATA = salesfunc.GetBaleStock("", TTXN.GOCD.retSqlformat(), balno, ITCD, mtrljobcd, VE.DefaultAction == "E" ? TTXN.AUTONO : "", ITGRPCD, "", "", "", false, "", "", true, true);



                                string ERROR_MESSAGE = ""; int ERROR_COUNT = 0;


                                var txndata = ITEMDT.AsEnumerable().Where(a => a.Field<string>("BALENO").retStr() != "")
                                                                .GroupBy(g => new { itcd = g["itcd"], BALENO = g["BALENO"] })
                                                                .Select(g => new
                                                                {
                                                                    ITCD = g.Key.itcd.retStr(),
                                                                    BALENO = g.Key.BALENO.retStr(),
                                                                    QNTY = g.Sum(r => r.Field<double>("qnty")),
                                                                }).ToList();
                                if (txndata != null && txndata.Count > 0)
                                {
                                    for (int i = 0; i <= txndata.Count - 1; i++)
                                    {
                                        string ITEM = txndata[i].ITCD;
                                        string BALE_NO = txndata[i].BALENO;

                                        string QNTY = ""; double STOCK_QNTY = 0; double SHORTAGE_QNTY = 0; double saleqnty = 0;

                                        var vQNTY = ITEM_STOCK_DATA.AsEnumerable()
                                                              .Where(g => g.Field<string>("itcd") == ITEM && g.Field<string>("BALENO") == BALE_NO)
                                                                  .GroupBy(g => new { itcd = g["itcd"], BALENO = g["BALENO"] })
                                                                   .Select(g =>
                                                                   {
                                                                       var row = ITEM_STOCK_DATA.NewRow();
                                                                       row["qnty"] = g.Sum(r => r.Field<decimal>("qnty"));
                                                                       return row;
                                                                   });
                                        if (vQNTY != null && vQNTY.Count() > 0)
                                        {
                                            var vQNTY1 = vQNTY.CopyToDataTable();
                                            QNTY = vQNTY1.Rows[0]["qnty"].retStr();
                                        }
                                        STOCK_QNTY = QNTY.retDbl();
                                        saleqnty = txndata[i].QNTY.retDbl();
                                        STOCK_QNTY = (VE.MENU_PARA == "SR" || VE.MENU_PARA == "REC") ? STOCK_QNTY + saleqnty.retDbl() : STOCK_QNTY - saleqnty.retDbl();

                                        if (STOCK_QNTY < 0)
                                        {
                                            ERROR_COUNT += 1; SHORTAGE_QNTY = QNTY.retDbl() - saleqnty.retDbl();

                                            ERROR_MESSAGE = ERROR_MESSAGE + "(" + ERROR_COUNT + ") " + "Item/Link Item : " + txndata[i].ITCD + " , Baleno : " + txndata[i].BALENO + " , Bale Stock Quantity : " + QNTY + " , Entered Quantity : " + saleqnty + ", Shortage Quantity : " + SHORTAGE_QNTY + "</br>";
                                        }
                                    }

                                }
                                if (ERROR_MESSAGE.Length > 0)
                                {
                                    ContentFlg = "Entry Can't Save ! Stock Not Available for Following :-</br></br>" + ERROR_MESSAGE + "";
                                    goto dbnotsave;
                                }

                            }
                        }
                    }

                    #endregion




                    if (VE.DefaultAction == "A")
                    {
                        ContentFlg = "1" + " (Bill No. " + DOCPATTERN + ")~" + TTXN.AUTONO;
                    }
                    else if (VE.DefaultAction == "E")
                    {
                        ContentFlg = "2";
                    }
                    OraTrans.Commit();
                    //if ((VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR") && VE.DefaultAction == "A") TempData["LoadFromExisting" + VE.MENU_PARA] = TTXN.AUTONO.retStr();
                    if ((VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS") && VE.DefaultAction == "A" && PIAUTONO.retStr() != "" && CommVar.ModuleCode() != "SALESSAREE")//need to delete profoma when salebill done from profma
                    {
                        T_SALEController TSCntlr = new T_SALEController();
                        VE.DefaultAction = "V";
                        VE.MENU_PARA = "PI";
                        string deletemsg = (string)TSCntlr.SAVE(VE, "PI");
                    }
                    if ((VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "ISS") && VE.DefaultAction == "A" && PIAUTONO.retStr() != "" && CommVar.ModuleCode() == "SALESSAREE")//need to cancel profoma when salebill done
                    {
                        T_SALEController TSCntlr = new T_SALEController();
                        VE.DefaultAction = "V";
                        VE.MENU_PARA = "PI";
                        var deletemsg = TSCntlr.cancelRecords(VE, "[" + DOCPATTERN + "]" + "[" + TTXN.DOCDT.retDateStr() + "]");
                    }
                    goto dbsave;
                }
                else if (VE.DefaultAction == "V")
                {
                    #region STOCK CHECKING
                    if (VE.MENU_PARA == "REC")
                    {
                        bool stockcheck = true;
                        if (stockcheck == true)
                        {
                            GRIDDATA();
                            if (VE.TBATCHDTL != null && VE.TBATCHDTL.Count > 0)
                            {
                                for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                                {
                                    if (VE.TBATCHDTL[i].ITCD.retStr() != "")
                                    {


                                        DataRow ROWDATA = ITEMDT.NewRow();
                                        ROWDATA["ITCD"] = VE.TBATCHDTL[i].ITCD.retStr();
                                        ROWDATA["BALENO"] = VE.TBATCHDTL[i].BALENO.retStr();
                                        ROWDATA["QNTY"] = VE.TBATCHDTL[i].QNTY.retDbl();
                                        ROWDATA["NEGSTOCK"] = VE.TBATCHDTL[i].NEGSTOCK.retStr();
                                        ITEMDT.Rows.Add(ROWDATA);

                                    }
                                }
                            }
                            string GCS = Cn.GCS();
                            var ITCDList = (from Z in VE.TBATCHDTL where Z.ITCD.retStr() != "" select Z.ITCD).Distinct().ToArray();
                            var ITCD = ITCDList.retSqlfromStrarray();
                            string BARNO = (from a in VE.TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
                            string ITGRPCD = (from a in VE.TBATCHDTL select a.ITGRPCD).Distinct().ToArray().retSqlfromStrarray();
                            var stocktype = string.Join(",", (from Z in VE.TBATCHDTL where Z.STKTYPE.retStr() != "" select "'" + Z.STKTYPE + "'").Distinct());


                            var mtrljobcd = VE.MTRLJOBCD.retStr();
                            if (mtrljobcd.retStr() == "")
                            {
                                mtrljobcd = "'FS'";
                            }
                            else
                            {
                                mtrljobcd = mtrljobcd.retSqlformat();

                            }

                            DataTable ITEM_STOCK_DATA = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);

                            string ERROR_MESSAGE = ""; int ERROR_COUNT = 0;


                            var txndata = ITEMDT.AsEnumerable().Where(a => a.Field<string>("itcd").retStr() != "")
                                                            .GroupBy(g => new { itcd = g["itcd"], NEGSTOCK = g["NEGSTOCK"] })
                                                            .Select(g => new
                                                            {
                                                                ITCD = g.Key.itcd.retStr(),
                                                                NEGSTOCK = g.Key.NEGSTOCK.retStr(),
                                                                QNTY = g.Sum(r => r.Field<double>("qnty")),
                                                            }).ToList();
                            if (txndata != null && txndata.Count > 0)
                            {
                                for (int i = 0; i <= txndata.Count - 1; i++)
                                {
                                    string ITEM = txndata[i].ITCD;

                                    string QNTY = ""; double STOCK_QNTY = 0; double SHORTAGE_QNTY = 0; double saleqnty = 0;

                                    var vQNTY = ITEM_STOCK_DATA.AsEnumerable()
                                                          .Where(g => g.Field<string>("itcd") == ITEM)
                                                              .GroupBy(g => new { itcd = g["itcd"] })
                                                               .Select(g =>
                                                               {
                                                                   var row = ITEM_STOCK_DATA.NewRow();
                                                                   row["balqnty"] = g.Sum(r => r.Field<decimal>("balqnty"));
                                                                   return row;
                                                               });
                                    if (vQNTY != null && vQNTY.Count() > 0)
                                    {
                                        var vQNTY1 = vQNTY.CopyToDataTable();
                                        QNTY = vQNTY1.Rows[0]["balqnty"].retStr();
                                        STOCK_QNTY = QNTY.retDbl();

                                        saleqnty = txndata[i].QNTY.retDbl();


                                        STOCK_QNTY = STOCK_QNTY - saleqnty.retDbl();
                                    }



                                    if (STOCK_QNTY < 0 && txndata[i].NEGSTOCK != "Y")
                                    {
                                        ERROR_COUNT += 1; SHORTAGE_QNTY = QNTY.retDbl() - saleqnty.retDbl();

                                        ERROR_MESSAGE = ERROR_MESSAGE + "(" + ERROR_COUNT + ") " + "Item/Link Item : " + txndata[i].ITCD + " , Stock Quantity : " + QNTY + " , Entered Quantity : " + saleqnty + ", Shortage Quantity : " + SHORTAGE_QNTY + "</br>";
                                    }
                                }

                            }
                            if (ERROR_MESSAGE.Length > 0)
                            {
                                ContentFlg = "Entry Can't Delete ! Stock Not Available for Following :-</br></br>" + ERROR_MESSAGE + "";
                                goto dbnotsave;
                            }

                        }
                    }

                    #endregion
                    ContentFlg = ChildRecordCheck(VE.T_TXN.AUTONO); if (ContentFlg != "") goto dbnotsave;
                    dbsql = masterHelp.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {
                        dbsql = masterHelp.TblUpdt("t_batchmst_price", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    }
                    if (VE.TBATCHDTL != null)
                    {
                        foreach (var v in VE.TBATCHDTL)
                        {
                            var IsTransactionFound = salesfunc.IsTransactionFound("", v.BARNO.retSqlformat(), VE.T_TXN.AUTONO.retSqlformat());
                            if (IsTransactionFound != "")
                            {
                                ContentFlg = "We cant delete this Bill. Transaction found at " + IsTransactionFound; goto dbnotsave;
                            }
                            else if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && IsTransactionFound == "")
                            {
                                dbsql = masterHelp.TblUpdt("T_BATCH_IMG_HDR", "", "D", "", "barno='" + v.BARNO + "'");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                dbsql = masterHelp.TblUpdt("T_BATCH_IMG_HDR_LINK", "", "D", "", "barno='" + v.BARNO + "'");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                            }
                            if (IsTransactionFound == "")
                            {
                                dbsql = masterHelp.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D", "", "barno='" + v.BARNO + "' and autono='" + VE.T_TXN.AUTONO + "'");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            }
                        }
                    }
                    dbsql = masterHelp.TblUpdt("t_txnoth", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_stktrnf", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    dbsql = masterHelp.TblUpdt("t_txnamt", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txntrans", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    if (VE.MENU_PARA == "SBPOS")
                    {
                        dbsql = masterHelp.TblUpdt("T_TXNMEMO", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    }
                    if (VE.MENU_PARA == "SB" || VE.MENU_PARA == "PJBL" || VE.MENU_PARA == "PJBR")
                    {
                        dbsql = masterHelp.TblUpdt("t_txn_linkno", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    }
                    //if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP")
                    //{
                    dbsql = masterHelp.TblUpdt("T_BALE", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    dbsql = masterHelp.TblUpdt("T_BALE_HDR", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    //}
                    dbsql = masterHelp.TblUpdt("t_txndtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    dbsql = masterHelp.TblUpdt("t_txnstatus", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txn", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    dbsql = masterHelp.TblUpdt("t_cntrl_auth", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_uniqno", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    // Delete from financial schema
                    if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "PJBR")
                    {
                        dbsql = masterHelp.finTblUpdt("t_vch_bl_adj", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    }
                    dbsql = masterHelp.finTblUpdt("T_TXNEWB", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.finTblUpdt("t_vch_gst", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_bl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_class", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_det", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_hdr", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    if (VE.MENU_PARA != "PJRC" && VE.MENU_PARA != "PJIS" && VE.MENU_PARA != "PJRT" && VE.MENU_PARA != "OP" && VE.MENU_PARA != "SBPCK" && VE.MENU_PARA != "PI")
                    {
                        //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "F", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "F", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null, null, null, null, null, VE.DISPBLAMT, VE.Audit_REM);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    }
                    //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null, null, null, null, null, VE.DISPBLAMT, VE.Audit_REM);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


                    ModelState.Clear();
                    OraTrans.Commit();
                    OraCon.Dispose();
                    ContentFlg = "3";
                    goto dbsave;
                }
                else
                {
                    goto dbnotsave;
                }

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, ""); ContentFlg = ex.Message + ex.InnerException;
                goto dbnotsave;
            }
            dbsave:
            {
                OraCon.Dispose();
                if (othr_para == "")
                    return Content(ContentFlg);
                else
                    return ContentFlg;
            }
            dbnotsave:
            {
                OraTrans.Rollback();
                OraCon.Dispose();
                if (othr_para == "")
                    return Content(ContentFlg);
                else
                    return ContentFlg;
            }

        }
        [ValidateInput(false)]
        public ActionResult Print(TransactionSaleEntry VE, FormCollection FC, string DOCNO, string DOC_CD, string DOCDT)
        {
            try
            {
                if (VE.MENU_PARA == "PJIS" || VE.MENU_PARA == "PJRC" || VE.MENU_PARA == "PJRT")
                {
                    Rep_Doc_Print repDoc = new Rep_Doc_Print();
                    string capname, reptype = "";
                    capname = "Issue Challan Printing"; reptype = "JOBISSUE";
                    repDoc.DOCCD = VE.T_TXN.DOCCD;
                    repDoc.FDOCNO = VE.T_TXN.DOCNO;
                    repDoc.TDOCNO = VE.T_TXN.DOCNO;
                    repDoc.FDT = VE.T_TXN.DOCDT.ToString().retDateStr();
                    repDoc.TDT = VE.T_TXN.DOCDT.ToString().retDateStr();
                    repDoc.SLCD = VE.T_TXN.SLCD;
                    repDoc.SLNM = VE.SLNM;
                    repDoc.AskSlCd = true;
                    repDoc.CaptionName = capname;
                    repDoc.ActionName = "Rep_PartyIssueChallan_Print";
                    repDoc.RepType = reptype;
                    repDoc.OtherPara = VE.MENU_PARA + "," + VE.T_TXN.GOCD;
                    if (TempData["printparameter"] != null)
                    {
                        TempData.Remove("printparameter");
                    }
                    TempData["printparameter"] = repDoc;
                }
                else
                {
                    ReportViewinHtml ind = new ReportViewinHtml();
                    ind.BARNO = VE.TBATCHDTL.Select(a => a.BARNO).Distinct().ToArray().retSqlfromStrarray();
                    ind.DOCCD = DOC_CD;
                    ind.FDOCNO = DOCNO;
                    ind.TDOCNO = DOCNO;
                    ind.FDT = DOCDT;
                    ind.TDT = DOCDT;
                    ind.TEXTBOX3 = VE.T_TXN.SLCD;
                    ind.TEXTBOX4 = VE.SLNM;
                    ind.MENU_PARA = "SALES";
                    if (TempData["printparameter"] != null)
                    {
                        TempData.Remove("printparameter");
                    }
                    TempData["printparameter"] = ind;
                }

                return Content("");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult UploadImages(string ImageStr, string ImageName, string ImageDesc)
        {
            try
            {
                var extension = Path.GetExtension(ImageName);
                string filename = "I".retRepname() + extension;
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
        public Tuple<List<T_BATCH_IMG_HDR>> SaveBarImage(string BarImage, string BARNO, short EMD)
        {
            List<T_BATCH_IMG_HDR> doc = new List<T_BATCH_IMG_HDR>();
            int slno = 0;
            try
            {
                var BarImages = BarImage.retStr().Split((char)179);
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
                        string topath = CommVar.SaveFolderPath() + "/ItemImages/" + mdoc.DOC_FLNAME;
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
        private string ChildRecordCheck(string autono, string Updatetrns = "")
        {
            TransactionSaleEntry VE = new TransactionSaleEntry();
            Cn.getQueryString(VE);
            string message = "";
            string scm = CommVar.CurSchema(UNQSNO);
            string fcm = CommVar.FinSchema(UNQSNO);

            if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC")
            {
                string doctype = VE.MENU_PARA == "PB" || VE.MENU_PARA == "REC" ? "'BLTG'" : "";
                var dt1 = salesfunc.GetBaleHistory("", "", "", doctype);
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    var chk = (from DataRow dr in dt1.Rows where dr["blautono"].retStr() == autono select new { docno = dr["docno"].retStr(), docdt = dr["docdt"].retStr(), docnm = dr["docnm"].retStr(), autono = dr["autono"].retStr() }).ToList();
                    if (chk != null && chk.Count > 0)
                    {
                        message = "Child record found at docno:" + chk[0].docno.ToString() + " docdt:" + chk[0].docdt.retDateStr() + " docnm:" + chk[0].docnm.ToString() + " autono:" + chk[0].autono.ToString() + " ";
                        return message;
                    }
                }
            }

            sql = "";
            sql += "select a.autono,b.docno,b.docdt,c.docnm ";
            sql += "from " + scm + ".T_BALE a,  " + scm + ".t_cntrl_hdr b,  " + scm + ".m_doctype c ";
            sql += "where a.autono = B.AUTONO and b.doccd = c.DOCCD and nvl(b.cancel,'N') = 'N'  and a.blautono = '" + autono + "'  ";
            sql += "and a.autono not in ('" + autono + "')";
            if (Updatetrns.retStr() != "Y")
            {
                sql += "union all ";
                sql += "select a.autono,b.docno,b.docdt,c.docnm  ";
                sql += "from  " + fcm + ".T_vch_bl_adj a," + fcm + ".t_cntrl_hdr b ," + fcm + ".m_doctype c  ";
                sql += "where a.autono=B.AUTONO and b.doccd=c.DOCCD  and nvl(b.cancel,'N') = 'N'  and (a.i_autono='" + autono + "' OR a.r_autono='" + autono + "'  )and a.autono not in ('" + autono + "')";
            }
            DataTable dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                message = "Child record found at docno:" + dt.Rows[0]["docno"].ToString() + " docdt:" + dt.Rows[0]["docdt"].retDateStr() + " docnm:" + dt.Rows[0]["docnm"].ToString() + " autono:" + dt.Rows[0]["autono"].ToString() + " ";
                return message;
            }
            return message;
        }
        public string IsMasterTblDataExist(TransactionSaleEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string str = "";
            //var m_syscnfg = DB.M_SYSCNFG.Count();
            var m_syscnfg = salesfunc.M_SYSCNFG(VE.T_TXN.DOCDT.retDateStr());
            if (m_syscnfg == null)
            {
                str += Cn.GCS() + "Posting/Terms Setup in Sales module(M_SYSCNFG)";
            }
            var m_post = DBF.M_POST.Select(m => m.CGST_S).ToList();
            if (m_post.Count == 0)
            {
                str += Cn.GCS() + "A/c Code Setup in Finance module (M_POST)";
            }

            return str;
        }
        public List<DropDown_list1> DISCTYPEINRATE()
        {
            List<DropDown_list1> DTYP = new List<DropDown_list1>();
            DropDown_list1 DTYP3 = new DropDown_list1();
            DTYP3.text = "%";
            DTYP3.value = "P";
            DTYP.Add(DTYP3);
            DropDown_list1 DTYP2 = new DropDown_list1();
            DTYP2.text = "Per Unit";
            DTYP2.value = "U";
            DTYP.Add(DTYP2);
            return DTYP;
        }
        public ActionResult Update_WpRpRate(TransactionSaleEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string sql = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    var schnm = CommVar.CurSchema(UNQSNO);
                    var CLCD = CommVar.ClientCode(UNQSNO);

                    short EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                    for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                    {
                        sql = "update " + schnm + ". T_BATCHMST set WPRATE='" + VE.TBATCHDTL[i].WPRATE + "',RPRATE='" + VE.TBATCHDTL[i].RPRATE + "' "
                                + " where BARNO='" + VE.TBATCHDTL[i].BARNO + "' and AUTONO='" + VE.T_TXN.AUTONO + "'   ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        string barno = VE.TBATCHDTL[i].BARNO.retStr();
                        if (VE.TBATCHDTL[i].WPRATE.retDbl() != 0)
                        {

                            var T_BATCHMST_PRICE = DB1.T_BATCHMST_PRICE.Where(x => x.BARNO == barno && x.AUTONO == VE.T_TXN.AUTONO && x.PRCCD == "WP").ToList();
                            if (T_BATCHMST_PRICE.Any())
                            {
                                sql = "update " + schnm + ".T_BATCHMST_PRICE set RATE =" + VE.TBATCHDTL[i].WPRATE + " "
                                        + " where BARNO='" + VE.TBATCHDTL[i].BARNO + "' and AUTONO='" + VE.T_TXN.AUTONO + "' and PRCCD='WP' and EFFDT=to_date('" + VE.T_TXN.DOCDT.retDateStr() + "','dd/mm/yyyy')  ";
                                OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                            }
                            else
                            {
                                sql = "insert into " + schnm + ".T_BATCHMST_PRICE  (EMD_NO, CLCD, DTAG, TTAG, BARNO,PRCCD, EFFDT, AUTONO, RATE) Values (" + EMD_NO + ", '" + CLCD + "', 'E', NULL, '" + VE.TBATCHDTL[i].BARNO + "','WP', TO_DATE('" + VE.T_TXN.DOCDT.retDateStr() + "','dd/mm/yyyy'), '" + VE.T_TXN.AUTONO + "', " + VE.TBATCHDTL[i].WPRATE + ")";
                                OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                            }
                        }
                        if (VE.TBATCHDTL[i].RPRATE.retDbl() != 0)
                        {
                            var T_BATCHMST_PRICE = DB1.T_BATCHMST_PRICE.Where(x => x.BARNO == barno && x.AUTONO == VE.T_TXN.AUTONO && x.PRCCD == "RP").ToList();
                            if (T_BATCHMST_PRICE.Any())
                            {
                                sql = "update " + schnm + ".T_BATCHMST_PRICE set RATE =" + VE.TBATCHDTL[i].RPRATE + " "
                                        + " where BARNO='" + VE.TBATCHDTL[i].BARNO + "' and AUTONO='" + VE.T_TXN.AUTONO + "' and PRCCD='RP' and EFFDT=to_date('" + VE.T_TXN.DOCDT.retDateStr() + "','dd/mm/yyyy')  ";
                                OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                            }
                            else
                            {
                                sql = "insert into " + schnm + ".T_BATCHMST_PRICE  (EMD_NO, CLCD, DTAG, TTAG, BARNO,PRCCD, EFFDT, AUTONO, RATE) Values (" + EMD_NO + ", '" + CLCD + "', 'E', NULL, '" + VE.TBATCHDTL[i].BARNO + "','RP', TO_DATE('" + VE.T_TXN.DOCDT.retDateStr() + "','dd/mm/yyyy'), '" + VE.T_TXN.AUTONO + "', " + VE.TBATCHDTL[i].RPRATE + ")";
                                OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    ModelState.Clear();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    ContentFlg = "1";
                    return Content(ContentFlg);
                }
                catch (Exception ex)
                {
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        public ActionResult ChangeBltype(TransactionSaleEntry VE)
        {
            try
            {
                Cn.getQueryString(VE);
                string bltype = VE.T_TXNOTH.BLTYPE.retStr() == "" ? "" : VE.T_TXNOTH.BLTYPE.retSqlformat();
                string slcd = VE.T_TXN.SLCD;
                string docdt = VE.T_TXN.DOCDT.retDateStr();
                string DOCTAG = MenuDescription(VE.MENU_PARA).Rows[0]["DOCTAG"].ToString().retSqlformat();
                var party_data = salesfunc.GetSlcdDetails(slcd, docdt, "", DOCTAG, bltype);
                string SLDISCDESC = "";
                if (party_data != null && party_data.Rows.Count > 0)
                {
                    string scmdisctype = party_data.Rows[0]["scmdisctype"].retStr() == "P" ? "%" : party_data.Rows[0]["scmdisctype"].retStr() == "N" ? "Nos" : party_data.Rows[0]["scmdisctype"].retStr() == "Q" ? "Qnty" : party_data.Rows[0]["scmdisctype"].retStr() == "F" ? "Fixed" : "";
                    SLDISCDESC += "^" + "SLDISCDESC" + "=^" + (party_data.Rows[0]["listdiscper"].retStr() + " " + party_data.Rows[0]["scmdiscrate"].retStr() + " " + scmdisctype + " " + (party_data.Rows[0]["lastbldt"].retStr() == "" ? "" : party_data.Rows[0]["lastbldt"].retStr().Remove(10))) + Cn.GCS();
                }
                return Content(SLDISCDESC);
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        public ActionResult DocumentDateChng(TransactionSaleEntry VE, string docdt)
        {
            try
            {
                string str = "";
                var dt = salesfunc.GetSyscnfgData(docdt);
                if (dt.Rows.Count > 0)
                {
                    str = masterHelp.ToReturnFieldValues("", dt);
                }
                VE.M_SYSCNFG = salesfunc.M_SYSCNFG(docdt);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                VE.Database_Combo4 = (from n in DB.T_BATCHDTL
                                      select new Database_Combo4() { FIELD_VALUE = n.PCSTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                List<DropDown_list2> RETURN_TYPE = new List<DropDown_list2> {
                    new DropDown_list2 { value = "N", text = "NO"},
                    new DropDown_list2 { value = "Y", text = "YES"},
                    };
                VE.RETURN_TYPE = RETURN_TYPE;
                ModelState.Clear();
                VE.DefaultView = true;
                var GRID_DATA = RenderRazorViewToString(ControllerContext, "_T_SALE_BarTab", VE);
                var DGRID_DATA = RenderRazorViewToString(ControllerContext, "_T_SALE_DETAIL", VE);
                return Content(str + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + GRID_DATA + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + DGRID_DATA);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public static string RenderRazorViewToString(ControllerContext controllerContext, string viewName, object model)
        {
            controllerContext.Controller.ViewData.Model = model;

            using (var stringWriter = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, stringWriter);
                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return stringWriter.GetStringBuilder().ToString();
            }
        }
        public ActionResult GetGeneralLedgerDetails(string val, string Code)
        {
            try
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                string caption = "General Ledger", linkcd = "C,D";
                var code_data = Code.Split(Convert.ToChar(Cn.GCS()));
                if (code_data.Count() > 1)
                {
                    linkcd = code_data[0];
                    caption = code_data[1];
                    if ((VE.MENU_PARA == "ISS" || VE.MENU_PARA == "REC") && caption == "Party Gen.Leder")
                    {
                        linkcd += ",O";
                    }
                }

                var str = masterHelp.GLCD_help(val, linkcd);
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
        private void GRIDDATA()
        {
            ITEMDT.Columns.Add("ITCD", typeof(string));
            ITEMDT.Columns.Add("BALENO", typeof(string));
            ITEMDT.Columns.Add("QNTY", typeof(double));
            ITEMDT.Columns.Add("NEGSTOCK", typeof(string));

        }
        public ActionResult GetStkIssue(string Code, string val, string Autono)
        {
            //ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            //var DOCCD = DB.M_DOCTYPE.Where(sd => sd.DOCTYPE == "SSTKO").FirstOrDefault()?.DOCCD;
            if (val == null)
            {
                return PartialView("_Help2", masterHelp.PENDINGSALES_T_STKTRNFISS_help("'TRFI'", val, Autono.retStr()));
            }
            else
            {
                string str = masterHelp.PENDINGSALES_T_STKTRNFISS_help("'TRFI'", val, Autono.retStr());
                return Content(str);
            }
        }
        public ActionResult GetCompanyDetails(string val, string Code)
        {
            try
            {

                var str = masterHelp.COMPCDLOCCD_help(val, Code);
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
        public ActionResult UpdateRemarks(TransactionSaleEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string sql = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    var schnm = CommVar.CurSchema(UNQSNO);

                    sql = "update " + schnm + ". T_TXNOTH set DOCREM='" + VE.T_TXNOTH.DOCREM + "'  "
                    + " where  AUTONO='" + VE.T_TXN.AUTONO + "'   ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    ModelState.Clear();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    ContentFlg = "1";
                    return Content(ContentFlg);
                }
                catch (Exception ex)
                {
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        public ActionResult CheckAuthRecord(TransactionSaleEntry VE, string callingfor, string remarks)
        {
            try
            {
                if (callingfor == "Check")
                {
                    T_TXNSTATUS t_TXNSTATUS = new T_TXNSTATUS();
                    t_TXNSTATUS.AUTONO = VE.T_TXN.AUTONO;
                    t_TXNSTATUS.CLCD = CommVar.ClientCode(UNQSNO);
                    t_TXNSTATUS.STSREM = remarks;
                    t_TXNSTATUS.USR_ID = CommVar.UserID();
                    t_TXNSTATUS.USR_ENTDT = System.DateTime.Now;
                    string sql = "";
                    t_TXNSTATUS.FLAG1 = "Check";
                    t_TXNSTATUS.STSTYPE = "K";
                    sql = "delete from  " + CommVar.CurSchema(UNQSNO) + ".t_txnstatus where autono='" + t_TXNSTATUS.AUTONO + "'  and ststype='U' ";
                    masterHelp.SQLNonQuery(sql);
                    sql = masterHelp.RetModeltoSql(t_TXNSTATUS, "A");
                    masterHelp.SQLNonQuery(sql);
                }
                else if (callingfor == "UnCheck")
                {
                    T_TXNSTATUS t_TXNSTATUS = new T_TXNSTATUS();
                    t_TXNSTATUS.AUTONO = VE.T_TXN.AUTONO;
                    t_TXNSTATUS.CLCD = CommVar.ClientCode(UNQSNO);
                    t_TXNSTATUS.STSREM = remarks;
                    t_TXNSTATUS.USR_ID = CommVar.UserID();
                    t_TXNSTATUS.USR_ENTDT = System.DateTime.Now;
                    string sql = "";
                    t_TXNSTATUS.FLAG1 = "UnChk";
                    t_TXNSTATUS.STSTYPE = "U";
                    sql = "delete from  " + CommVar.CurSchema(UNQSNO) + ".t_txnstatus where autono='" + t_TXNSTATUS.AUTONO + "'  and ststype='K' ";
                    masterHelp.SQLNonQuery(sql);
                    sql = masterHelp.RetModeltoSql(t_TXNSTATUS, "A");
                    masterHelp.SQLNonQuery(sql);
                }
                else if (callingfor == "Authorise")
                {
                    Cn.DocumentAuthorisation_Save(VE.T_TXN.DOCCD, VE.T_TXN.AUTONO, 1, VE.T_TXN.BLAMT.retDbl(), VE.T_CNTRL_HDR.EMD_NO.retShort(), "Auth", remarks, Convert.ToByte(VE.AuthorisationLVL.retInt()), "A");
                }
                else if (callingfor == "Unauthorise")
                {
                    Cn.DocumentAuthorisation_Save(VE.T_TXN.DOCCD, VE.T_TXN.AUTONO, 1, VE.T_TXN.BLAMT.retDbl(), VE.T_CNTRL_HDR.EMD_NO.retShort(), "UnAth", remarks, Convert.ToByte(VE.AuthorisationLVL.retInt()), "N");
                }
                return Content(callingfor);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }


    }
}