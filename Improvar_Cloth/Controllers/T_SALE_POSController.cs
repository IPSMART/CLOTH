﻿using System;
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

namespace Improvar.Controllers
{
    public class T_SALE_POSController : Controller
    {
        // GET: T_SALE_POS_POS
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl(); DropDownHelp dropDownHelp = new DropDownHelp();
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_SALE_POS
        public ActionResult T_SALE_POS(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no", string loadOrder = "N")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Cash Sale (Pos)";
                    TransactionSalePosEntry VE = (parkID == "") ? new TransactionSalePosEntry() : (Improvar.ViewModels.TransactionSalePosEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    switch (VE.MENU_PARA)
                    {
                        case "SBCM":
                            ViewBag.formname = "Cash Memo"; break;
                        case "SBCMR":
                            ViewBag.formname = "Cash Memo Credit Note"; break;
                        case "CADV":
                            ViewBag.formname = "Party Order Advance"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    VE.BL_TYPE = dropDownHelp.DropDownBLTYPE();
                    VE.DropDown_list_StkType = masterHelp.STK_TYPE();
                    //VE.DISC_TYPE = masterHelp.DISC_TYPE();

                    VE.BARGEN_TYPE = masterHelp.BARGEN_TYPE();
                    VE.INVTYPE_list = masterHelp.INVTYPE_list();
                    //VE.EXPCD_list = masterHelp.EXPCD_list(VE.MENU_PARA == "PB" ? "P" : "S");
                    VE.Reverse_Charge = masterHelp.REV_CHRG();

                    VE.DISC_TYPE = masterHelp.DISC_TYPE();
                    VE.PCSActionList = masterHelp.PCSAction();

                    VE.Database_Combo1 = (from n in DB.T_TXNOTH
                                          select new Database_Combo1() { FIELD_VALUE = n.SELBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.Database_Combo2 = (from n in DB.T_TXNOTH
                                          select new Database_Combo2() { FIELD_VALUE = n.DEALBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.Database_Combo3 = (from n in DB.T_TXNOTH
                                          select new Database_Combo3() { FIELD_VALUE = n.PACKBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.HSN_CODE = (from n in DBF.M_HSNCODE
                                   select new HSN_CODE() { text = n.HSNDESCN, value = n.HSNCODE }).OrderBy(s => s.text).Distinct().ToList();

                    //VE.DropDown_list_MTRLJOBCD = (from i in DB.M_MTRLJOBMST select new DropDown_list_MTRLJOBCD() { MTRLJOBCD = i.MTRLJOBCD, MTRLJOBNM = i.MTRLJOBNM }).OrderBy(s => s.MTRLJOBNM).ToList();
                    VE.DropDown_list_MTRLJOBCD = masterHelp.MTRLJOBCD_List();
                    if (VE.DropDown_list_MTRLJOBCD.Count() == 1)
                    {
                        VE.DropDown_list_MTRLJOBCD[0].Checked = true;
                    }
                    else
                    {
                        foreach (var v in VE.DropDown_list_MTRLJOBCD)
                        {
                            if (v.MTRLJOBCD == "FS") { v.Checked = true; }
                        }
                    }
                    VE.SHOWMTRLJOBCD = VE.DropDown_list_MTRLJOBCD.Count() > 1 ? "Y" : "N";
                    //=========For Registraion Type===========//
                    List<RegistrationType> RT = new List<RegistrationType>();
                    RegistrationType RT1 = new RegistrationType();
                    RT1.Value = "R";
                    RT1.Text = "Registered";
                    RT.Add(RT1);
                    RegistrationType RT2 = new RegistrationType();
                    RT2.Value = "U";
                    RT2.Text = "Unregisterd";
                    RT.Add(RT2);
                    RegistrationType RT3 = new RegistrationType();
                    RT3.Value = "C";
                    RT3.Text = "Composite";
                    RT.Add(RT3);
                    RegistrationType RT4 = new RegistrationType();
                    RT4.Value = "N";
                    RT4.Text = "UAN";
                    RT.Add(RT4);
                    VE.RegistrationType = RT;
                    //=========End Registraion Type===========//
                    string sql = "select NVL(GSPCLIENTAPP,GSPAPPID) GSPCLIENTAPP from ms_ipsmart";
                    DataTable dt = masterHelp.SQLquery(sql);
                    if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["GSPCLIENTAPP"].ToString() != "")
                    {
                        VE.IsAPIEnabled = true;
                    }
                    //VE.PRCNM
                    string[] autoEntryWork = ThirdParty.Split('~');// for zooming
                    if (autoEntryWork[0] == "yes")
                    {
                        autoEntryWork[2] = autoEntryWork[2].Replace("$$$$$$$$", "&");
                    }
                    if (autoEntryWork[0] == "yes")
                    {
                        if (autoEntryWork[4] == "Y")
                        {
                            DocumentType dp = new DocumentType();
                            dp.text = autoEntryWork[2];
                            dp.value = autoEntryWork[1];
                            VE.DocumentType.Clear();
                            VE.DocumentType.Add(dp);
                            VE.Edit = "E";
                            VE.Delete = "D";
                        }
                    }
                    VE.SrcFlagCaption = "Mobile No.";
                    if (op.Length != 0)
                    {
                        string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();

                        VE.IndexKey = (from p in DB.T_TXN
                                       join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       orderby p.DOCDT, p.DOCNO
                                       where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YR1
                                       select new IndexKey() { Navikey = p.AUTONO }).ToList();
                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
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

                            if (loadOrder.retStr().Length > 1)
                            {
                                VE.T_TXN.AUTONO = "";
                                if (VE.T_TXN_LINKNO == null) VE.T_TXN_LINKNO = new T_TXN_LINKNO();
                                VE.LINKDOCNO = loadOrder.retStr();
                                VE.T_TXN_LINKNO.LINKAUTONO = searchValue.retStr();
                            }
                            if (VE.T_CNTRL_HDR != null && VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }
                        if (op.ToString() == "A" && loadOrder == "N")
                        {
                            if (parkID == "")
                            {

                                T_TXN TTXN = new T_TXN();
                                if (VE.DocumentType.Count == 1)
                                {
                                    TTXN.DOCCD = VE.DocumentType[0].value;
                                }
                                Cn.getdocmaxmindate(TTXN.DOCCD, "", VE.DefaultAction, "", VE);
                                TTXN.DOCDT = Cn.getCurrentDate(VE.mindate);
                                TTXN.GOCD = TempData["LASTGOCD" + VE.MENU_PARA].retStr();
                                if (VE.MENU_PARA == "CADV")
                                {
                                    TTXN.PARGLCD = TempData[GetLastTempdataName("LASTPARGLCD", TTXN.DOCCD)].retStr();
                                    TTXN.GLCD = TempData[GetLastTempdataName("LASTGLCD", TTXN.DOCCD)].retStr();
                                }
                                TempData.Keep();
                                if (TTXN.GOCD.retStr() == "")
                                {
                                    if (VE.DocumentType.Count() > 0)
                                    {
                                        string doccd = VE.DocumentType.FirstOrDefault().value;
                                        TTXN.GOCD = DB.T_TXN.Where(a => a.DOCCD == doccd).Select(b => b.GOCD).FirstOrDefault();
                                    }
                                }
                                string gocd = TTXN.GOCD.retStr();
                                if (gocd != "")
                                {
                                    VE.GONM = DBF.M_GODOWN.Where(a => a.GOCD == gocd).Select(b => b.GONM).FirstOrDefault();
                                }
                                if (VE.MENU_PARA == "CADV")
                                {
                                    string PARGLCD = TTXN.PARGLCD.retStr();
                                    if (PARGLCD != "")
                                    {
                                        VE.PARGLNM = DBF.M_GENLEG.Where(a => a.GLCD == PARGLCD).Select(b => b.GLNM).FirstOrDefault();
                                    }

                                    string GLCD = TTXN.GLCD.retStr();
                                    if (GLCD != "")
                                    {
                                        VE.GLNM = DBF.M_GENLEG.Where(a => a.GLCD == GLCD).Select(b => b.GLNM).FirstOrDefault();
                                    }
                                }

                                T_TXNOTH TXNOTH = new T_TXNOTH(); T_TXNMEMO TXNMEMO = new T_TXNMEMO();
                                DataTable syscnfgdt = salesfunc.GetSyscnfgData(TTXN.DOCDT.retDateStr());
                                if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                                {
                                    TXNMEMO.RTDEBCD = syscnfgdt.Rows[0]["RTDEBCD"].retStr();
                                    TXNMEMO.NM = syscnfgdt.Rows[0]["RTDEBNM"].retStr();
                                    TXNMEMO.MOBILE = syscnfgdt.Rows[0]["MOBILE"].retStr();

                                    VE.RTDEBNM = syscnfgdt.Rows[0]["RTDEBNM"].retStr();
                                    var addrs = syscnfgdt.Rows[0]["add1"].retStr() + " " + syscnfgdt.Rows[0]["add2"].retStr() + " " + syscnfgdt.Rows[0]["add3"].retStr();
                                    VE.ADDR = addrs + "/" + syscnfgdt.Rows[0]["city"].retStr();
                                    VE.MOBILE = syscnfgdt.Rows[0]["MOBILE"].retStr();
                                    TXNMEMO.ADDR = addrs;
                                    TXNMEMO.CITY = syscnfgdt.Rows[0]["city"].retStr();
                                    VE.INC_RATE = syscnfgdt.Rows[0]["INC_RATE"].retStr() == "Y" ? true : false;
                                    VE.INCLRATEASK = syscnfgdt.Rows[0]["INC_RATE"].retStr();
                                    VE.INCLRATEASKMASTER = syscnfgdt.Rows[0]["INC_RATE"].retStr();
                                    VE.RETDEBSLCD = syscnfgdt.Rows[0]["retdebslcd"].retStr();
                                    TXNOTH.TAXGRPCD = syscnfgdt.Rows[0]["TAXGRPCD"].retStr();
                                    TXNOTH.PRCCD = syscnfgdt.Rows[0]["prccd"].retStr(); VE.PRCNM = syscnfgdt.Rows[0]["prcnm"].retStr();
                                    VE.EFFDT = syscnfgdt.Rows[0]["effdt"].retDateStr();
                                }
                                VE.T_TXNMEMO = TXNMEMO;
                                VE.T_TXNOTH = TXNOTH;
                                VE.T_TXN = TTXN;
                                VE.RoundOff = true;

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
                            VE = (TransactionSalePosEntry)Cn.CheckPark(VE, VE.MENU_DETAILS, LOC, COM, CommVar.CurSchema(UNQSNO), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        if ((op.ToString() == "A" || op.ToString() == "E") && parkID == "" && loadOrder == "N")
                        {
                            FreightCharges(VE, VE.T_TXN?.AUTONO);
                            if (VE.TTXNSLSMN == null || VE.TTXNSLSMN.Count == 0)
                            {
                                List<TTXNSLSMN> TTXNSLSMN = new List<TTXNSLSMN>();
                                for (int i = 0; i <= 2; i++)
                                {
                                    TTXNSLSMN TTXNSLM = new TTXNSLSMN();
                                    TTXNSLM.SLNO = Convert.ToInt16(i + 1);
                                    if (i == 0) TTXNSLM.PER = 100;
                                    TTXNSLSMN.Add(TTXNSLM);
                                    VE.TTXNSLSMN = TTXNSLSMN;
                                }
                                VE.TTXNSLSMN = TTXNSLSMN;
                            }
                            if (VE.TTXNPYMT == null || VE.TTXNPYMT.Count == 0)
                            {
                                var MPAYMENT = (from i in DB.M_PAYMENT join j in DB.M_CNTRL_HDR on i.M_AUTONO equals j.M_AUTONO where j.INACTIVE_TAG == "N" select new { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD, PYMTTYPE = i.PYMTTYPE }).OrderBy(a => a.PYMTCD).ToList();
                                if (MPAYMENT.Count > 0)
                                {
                                    if (VE.MENU_PARA == "CADV")
                                    {
                                        MPAYMENT = MPAYMENT.Where(a => a.PYMTCD != "AD").ToList();
                                    }
                                }
                                if (MPAYMENT.Count > 0)
                                {
                                    VE.TTXNPYMT = (from i in MPAYMENT select new TTXNPYMT { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD, PYMTTYPE = i.PYMTTYPE }).ToList();
                                    for (int p = 0; p <= VE.TTXNPYMT.Count - 1; p++)
                                    {
                                        VE.TTXNPYMT[p].SLNO = Convert.ToInt16(p + 1);
                                    }
                                }
                                else
                                {
                                    int slno = 0;
                                    List<TTXNPYMT> TTXNPYMNT = new List<TTXNPYMT>();
                                    TTXNPYMT TXNPYMT = new TTXNPYMT();
                                    TXNPYMT.SLNO = Convert.ToInt16(slno + 1);
                                    TTXNPYMNT.Add(TXNPYMT);
                                    VE.TTXNPYMT = TTXNPYMNT;
                                }
                            }

                        }
                        if (parkID == "" && loadOrder == "N")
                        {
                            //var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                            var MSYSCNFG = salesfunc.M_SYSCNFG(VE.T_TXN?.DOCDT.retDateStr());
                            VE.M_SYSCNFG = MSYSCNFG;
                            FreightCharges(VE, VE.T_TXN?.AUTONO); VE.RETAMT = VE.RETAMT.retStr() == "" ? 0 : VE.RETAMT.retDbl().toRound(2);
                            //VE.RETAMT = VE.R_T_NET;
                            //VE.OTHAMT = VE.A_T_NET;
                            //if (VE.T_TXN != null) VE.PAYABLE = (VE.T_TXN.BLAMT - VE.RETAMT).retDbl().toRound(2); else VE.PAYABLE = 0.retDbl().toRound(2);
                            //VE.NETDUE = (VE.PAYABLE - VE.PAYAMT).retDbl().toRound(2);
                        }
                        VE.SHOWBLTYPE = VE.BL_TYPE.Count > 0 ? "Y" : "N";
                        var MSYSCNFG1 = salesfunc.M_SYSCNFG(VE.T_TXN?.DOCDT.retDateStr());
                        VE.MergeItem = MSYSCNFG1.MERGEINDTL.retStr() == "Y" ? true : false;
                        // VE.MergeItem = (CommVar.ModuleCode().retStr().IndexOf("SALESCLOTH") != -1) ? false : true;
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (VE.T_CNTRL_HDR != null) if (VE.T_CNTRL_HDR.DOCDT != null) docdt = VE.T_CNTRL_HDR.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_TXN?.DOCCD, docdt, VE.DefaultAction, VE.T_TXN?.DOCNO, VE);
                    if ((op.ToString() == "A" && loadOrder == "N" && parkID == "") || ((op == "E" || op == "D" || op == "V") && loadOrder.retStr().Length > 1))
                    {
                        VE.T_TXN.DOCDT = Cn.getCurrentDate(VE.mindate);
                        VE.T_TXN.PREFDT = Cn.getCurrentDate(VE.mindate);
                    }
                    VE.Last_DOCDT = VE.T_TXN?.DOCDT.retDateStr();
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                TransactionSalePosEntry VE = new TransactionSalePosEntry();
                Cn.SaveException(ex, "");
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionSalePosEntry Navigation(TransactionSalePosEntry VE, ImprovarDB DB, int index, string searchValue, string loadOrder = "N")
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE);
            string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);

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
                VE.T_TXN = DB.T_TXN.Find(aa[0].Trim());
                VE.T_CNTRL_HDR = DB.T_CNTRL_HDR.Find(VE.T_TXN.AUTONO);
                if (VE.T_TXN.ROYN == "Y") VE.RoundOff = true;
                else VE.RoundOff = false;
                VE.T_TXNTRANS = DB.T_TXNTRANS.Find(VE.T_TXN.AUTONO);
                VE.T_TXNOTH = DB.T_TXNOTH.Find(VE.T_TXN.AUTONO);
                VE.T_TXNMEMO = DB.T_TXNMEMO.Find(VE.T_TXN.AUTONO);
                VE.T_TXNEINV = DBF.T_TXNEINV.Find(VE.T_TXN.AUTONO);
                VE.T_VCH_GST = DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).FirstOrDefault();
                //var MGP = DB.M_GROUP.Find(   VE.T_TXN.ITGRPCD);

                VE.T_TXN_LINKNO = (from a in DB.T_TXN_LINKNO where a.AUTONO == VE.T_TXN.AUTONO select a).FirstOrDefault();

                //VE.LINKDOCNO = (from a in DB.T_CNTRL_HDR where a.AUTONO == TTXNLINKNO.LINKAUTONO select a).FirstOrDefault().DOCNO;


                VE.RTDEBNM = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.RTDEBNM).FirstOrDefault();
                VE.MOBILE = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.MOBILE).FirstOrDefault();
                var add1 = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.ADD1).FirstOrDefault();
                var add2 = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.ADD2).FirstOrDefault();
                var add3 = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.ADD3).FirstOrDefault();
                var city = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.CITY).FirstOrDefault();
                VE.ADDR = add1 + " " + add2 + " " + add3 + "/" + city;
                VE.EFFDT = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DB.M_SYSCNFG.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.EFFDT).FirstOrDefault().retDateStr();

                VE.PARGLNM = VE.T_TXN.PARGLCD.retStr() == "" ? "" : DBF.M_GENLEG.Where(a => a.GLCD == VE.T_TXN.PARGLCD).Select(b => b.GLNM).FirstOrDefault();
                VE.GLNM = VE.T_TXN.GLCD.retStr() == "" ? "" : DBF.M_GENLEG.Where(a => a.GLCD == VE.T_TXN.GLCD).Select(b => b.GLNM).FirstOrDefault();

                string sql1 = "";
                sql1 += " select a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate,C.TAXGRPCD,a.retdebslcd,b.city,b.add1,b.add2,b.add3,effdt ";
                sql1 += "  from  " + scm + ".M_SYSCNFG a, " + scmf + ".M_RETDEB b, " + scm + ".M_SUBLEG_SDDTL c";
                sql1 += " where a.RTDEBCD=b.RTDEBCD and a.effdt in(select max(effdt) effdt from  " + scm + ".M_SYSCNFG) and a.retdebslcd=C.SLCD";
                DataTable syscnfgdt = masterHelp.SQLquery(sql1);
                if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                {
                    VE.RETDEBSLCD = syscnfgdt.Rows[0]["retdebslcd"].retStr();
                    //VE.EFFDT = syscnfgdt.Rows[0]["effdt"].retDateStr();
                }

                VE.INC_RATE = VE.T_TXN.INCL_RATE == "Y".retStr() ? true : false;
                VE.INCLRATEASK = VE.T_TXN.INCL_RATE.retStr();
                VE.AGSLNM = VE.T_TXNOTH.AGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == VE.T_TXNOTH.AGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.SAGSLNM = VE.T_TXNOTH.SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == VE.T_TXNOTH.SAGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.GONM = VE.T_TXN.GOCD.retStr() == "" ? "" : DBF.M_GODOWN.Where(a => a.GOCD == VE.T_TXN.GOCD).Select(b => b.GONM).FirstOrDefault();
                //VE.GOCD = VE.T_TXN.GOCD.retStr() == "" ? "" : VE.T_TXN.GOCD;
                VE.PRCNM = VE.T_TXNOTH.PRCCD.retStr() == "" ? "" : DBF.M_PRCLST.Where(a => a.PRCCD == VE.T_TXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();
                //VE.GSTNO = VE.T_TXN.AUTONO.retStr() == "" ? "" : DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.GSTNO).FirstOrDefault();
                //VE.GSTSLNM = VE.T_TXN.AUTONO.retStr() == "" ? "" : DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.GSTSLNM).FirstOrDefault();
                //VE.GSTSLADD1 = VE.T_TXN.AUTONO.retStr() == "" ? "" : DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.GSTSLADD1).FirstOrDefault();
                //VE.GSTSLDIST = VE.T_TXN.AUTONO.retStr() == "" ? "" : DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.GSTSLDIST).FirstOrDefault();
                //VE.GSTSLPIN = VE.T_TXN.AUTONO.retStr() == "" ? "" : DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.GSTSLPIN).FirstOrDefault();

                VE.T_CNTRL_HDR_REM = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), VE.T_TXN.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), VE.T_TXN.AUTONO);

                string Scm = CommVar.CurSchema(UNQSNO);
                string str1 = "";
                str1 += "select distinct i.autono, i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.NEGSTOCK,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM,p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,i.INCLRATE,i.PCSACTION,i.ORDSLNO,i.ORDAUTONO,r.PRCCD,s.docno ORDNO,s.docdt ORDDT,  ";
                str1 += "j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE,i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM ,r.DISCAMT,r.TDDISCAMT,r.SCMDISCAMT,r.TXBLVAL,r.IGSTPER,r.CGSTPER,r.SGSTPER,r.CESSPER,r.IGSTAMT,r.CGSTAMT,r.SGSTAMT,r.CESSAMT,r.NETAMT,r.TOTDISCAMT ,r.AMT,r.BLQNTY,r.STKDRCR,r.AGDOCNO,r.AGDOCDT ";
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".M_STKTYPE q ," + Scm + ".T_TXNDTL r," + Scm + ".T_CNTRL_HDR s ";
                str1 += "where i.AUTONO = r.AUTONO(+) and i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) and i.ORDAUTONO = s.AUTONO(+) and i.txnslno=r.slno(+) ";
                str1 += "and i.AUTONO = '" + VE.T_TXN.AUTONO + "'";
                str1 += "order by i.SLNO ";
                DataTable tbl = masterHelp.SQLquery(str1);

                VE.TsalePos_TBATCHDTL = (from DataRow dr in tbl.Rows
                                             //where dr["SLNO"].retShort() <= 2000
                                         where dr["SLNO"].retShort() <= 1000
                                         select new TsalePos_TBATCHDTL()
                                         {
                                             SLNO = dr["SLNO"].retShort(),
                                             TXNSLNO = dr["TXNSLNO"].retShort(),
                                             ITGRPCD = dr["ITGRPCD"].retStr(),
                                             ITGRPNM = dr["ITGRPNM"].retStr(),
                                             NEGSTOCK = dr["NEGSTOCK"].retStr(),
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
                                             DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                             DISCAMT = dr["DISCAMT"].retDbl(),
                                             TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                             TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                             TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                             SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                             SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                             SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                             SCMDISCAMT = dr["SCMDISCAMT"].retDbl(),
                                             STKTYPE = dr["STKTYPE"].retStr(),
                                             STKNAME = dr["STKNAME"].retStr(),
                                             BARNO = dr["BARNO"].retStr(),
                                             HSNCODE = dr["HSNCODE"].retStr(),
                                             PDESIGN = dr["PDESIGN"].retStr(),
                                             OURDESIGN = dr["OURDESIGN"].retStr(),
                                             FLAGMTR = dr["FLAGMTR"].retDbl(),
                                             LOCABIN = dr["LOCABIN"].retStr(),
                                             BALEYR = dr["BALEYR"].retStr(),
                                             BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                             GLCD = dr["SALGLCD"].retStr(),
                                             ITREM = dr["ITREM"].retStr(),
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
                                             TOTDISCAMT = dr["TOTDISCAMT"].retDbl(),
                                             GROSSAMT = dr["AMT"].retDbl(),
                                             INCLRATE = dr["INCLRATE"].retDbl(),
                                             PCSACTION = dr["PCSACTION"].retStr(),
                                             ORDAUTONO = dr["ORDAUTONO"].retStr(),
                                             ORDSLNO = dr["ORDSLNO"].retDbl() == 0 ? (short?)null : dr["ORDSLNO"].retShort(),
                                             ORDDOCNO = dr["ORDNO"].retStr(),
                                             ORDDOCDT = dr["ORDDT"].retDateStr(),
                                             STKDRCR = dr["STKDRCR"].retStr()
                                         }).OrderBy(s => s.SLNO).DistinctBy(s => s.SLNO).ToList();

                #region Return tab

                VE.TsalePos_TBATCHDTL_RETURN = (from DataRow dr in tbl.Rows
                                                    //where dr["SLNO"].retShort() > 2000
                                                where dr["SLNO"].retShort() > 1000
                                                select new TsalePos_TBATCHDTL_RETURN()
                                                {
                                                    SLNO = dr["SLNO"].retShort(),
                                                    TXNSLNO = dr["TXNSLNO"].retShort(),
                                                    ITGRPCD = dr["ITGRPCD"].retStr(),
                                                    ITGRPNM = dr["ITGRPNM"].retStr(),
                                                    NEGSTOCK = dr["NEGSTOCK"].retStr(),
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
                                                    DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                                    DISCAMT = dr["DISCAMT"].retDbl(),
                                                    TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                                    TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                                    TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                                    SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                                    SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                                    SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                                    STKTYPE = dr["STKTYPE"].retStr(),
                                                    STKNAME = dr["STKNAME"].retStr(),
                                                    BARNO = dr["BARNO"].retStr(),
                                                    HSNCODE = dr["HSNCODE"].retStr(),
                                                    PDESIGN = dr["PDESIGN"].retStr(),
                                                    OURDESIGN = dr["OURDESIGN"].retStr(),
                                                    FLAGMTR = dr["FLAGMTR"].retDbl(),
                                                    LOCABIN = dr["LOCABIN"].retStr(),
                                                    BALEYR = dr["BALEYR"].retStr(),
                                                    BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                                    GLCD = dr["SALGLCD"].retStr(),
                                                    ITREM = dr["ITREM"].retStr(),
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
                                                    TOTDISCAMT = dr["TOTDISCAMT"].retDbl(),
                                                    GROSSAMT = dr["AMT"].retDbl(),
                                                    INCLRATE = dr["INCLRATE"].retDbl(),
                                                    PCSACTION = dr["PCSACTION"].retStr(),
                                                    ORDAUTONO = dr["ORDAUTONO"].retStr(),
                                                    ORDSLNO = dr["ORDSLNO"].retStr() == "0" ? "" : dr["ORDSLNO"].retStr(),
                                                    ORDDOCNO = dr["ORDNO"].retStr(),
                                                    ORDDOCDT = dr["ORDDT"].retDateStr(),
                                                    AGDOCNO = dr["AGDOCNO"].retStr(),
                                                    AGDOCDT = dr["AGDOCDT"].retDateStr()
                                                }).OrderBy(s => s.SLNO).DistinctBy(s => s.SLNO).ToList();

                VE.R_T_QNTY = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.QNTY).retDbl();
                VE.R_T_NOS = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.NOS).retDbl();
                VE.R_T_AMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.TXBLVAL).retDbl();
                VE.T_R_GROSSAMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.GROSSAMT).retDbl();
                VE.R_T_IGST_AMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.IGSTAMT).retDbl();
                VE.R_T_CGST_AMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.CGSTAMT).retDbl();
                VE.R_T_SGST_AMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.SGSTAMT).retDbl();
                VE.R_T_CESS_AMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.CESSAMT).retDbl();
                VE.R_T_NET = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.NETAMT).retDbl();
                VE.R_T_NET_AMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.NETAMT).retDbl();
                VE.R_T_GROSS_AMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.GROSSAMT).retDbl();
                VE.R_T_DISCAMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.DISCAMT).retDbl();
                VE.R_T_FLAGMTR = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.FLAGMTR).retDbl();

                #endregion

                VE.B_T_QNTY = VE.TsalePos_TBATCHDTL.Sum(a => a.QNTY).retDbl();
                VE.B_T_NOS = VE.TsalePos_TBATCHDTL.Sum(a => a.NOS).retDbl();
                VE.T_NOS = VE.TsalePos_TBATCHDTL.Sum(a => a.NOS).retDbl();
                VE.T_QNTY = VE.TsalePos_TBATCHDTL.Sum(a => a.QNTY).retDbl();
                VE.T_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.TXBLVAL).retDbl();
                VE.T_DISCAMT = VE.TsalePos_TBATCHDTL.Sum(a => a.DISCAMT).retDbl();
                VE.T_SCMDISCAMT = VE.TsalePos_TBATCHDTL.Sum(a => a.SCMDISCAMT).retDbl();
                VE.T_B_GROSSAMT = VE.TsalePos_TBATCHDTL.Sum(a => a.GROSSAMT).retDbl();
                VE.T_IGST_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.IGSTAMT).retDbl();
                VE.T_CGST_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.CGSTAMT).retDbl();
                VE.T_SGST_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.SGSTAMT).retDbl();
                VE.T_CESS_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.CESSAMT).retDbl();
                VE.B_T_NET = VE.TsalePos_TBATCHDTL.Sum(a => a.NETAMT).retDbl();
                VE.T_NET_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.NETAMT).retDbl();
                VE.T_GROSSAMT = VE.TsalePos_TBATCHDTL.Sum(a => a.GROSSAMT).retDbl();
                VE.T_FLAGMTR = VE.TsalePos_TBATCHDTL.Sum(a => a.FLAGMTR).retDbl();

                // fill prodgrpgstper in t_batchdtl
                DataTable allprodgrpgstper_data = new DataTable();
                string BARNO = (from a in VE.TsalePos_TBATCHDTL select a.BARNO).Distinct().ToArray().retSqlfromStrarray();
                string ITCD = (from a in VE.TsalePos_TBATCHDTL select a.ITCD).Distinct().ToArray().retSqlfromStrarray();
                string MTRLJOBCD = (from a in VE.TsalePos_TBATCHDTL select a.MTRLJOBCD).Distinct().ToArray().retSqlfromStrarray();
                string ITGRPCD = (from a in VE.TsalePos_TBATCHDTL select a.ITGRPCD).Distinct().ToArray().retSqlfromStrarray();

                allprodgrpgstper_data = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), MTRLJOBCD, VE.T_TXN.AUTONO.retSqlformat(), ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);

                foreach (var v in VE.TsalePos_TBATCHDTL)
                {
                    //v.INCL_DISC = (v.INCLRATE.retDbl() * v.QNTY.retDbl()) - v.NETAMT.retDbl();
                    //v.DISC_TYPE = masterHelp.DISC_TYPE();
                    //v.PCSActionList = masterHelp.PCSAction();
                    v.GSTAMT = (v.IGSTAMT + v.CGSTAMT + v.SGSTAMT).retDbl().toRound(2);
                    string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "";
                    v.GSTPER = VE.TsalePos_TBATCHDTL.Where(a => a.SLNO == v.TXNSLNO).Sum(b => b.IGSTPER + b.CGSTPER + b.SGSTPER).retDbl();
                    if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
                    {
                        //var q = (from DataRow dr in allprodgrpgstper_data.Rows
                        //         select new
                        //         {
                        //             BALSTOCK = dr["BALQNTY"].retDbl(),
                        //             NEGSTOCK = dr["negstock"].retStr()
                        //         }).FirstOrDefault();

                        var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "' ");
                        if (DATA.Count() > 0)
                        {
                            DataTable tax_data = DATA.CopyToDataTable();
                            if (tax_data != null && tax_data.Rows.Count > 0)
                            {
                                v.BALSTOCK = tax_data.Rows[0]["BALQNTY"].retDbl();
                                v.NEGSTOCK = tax_data.Rows[0]["negstock"].retStr();
                                PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                                if (PRODGRPGSTPER != "")
                                {
                                    ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl(), v.DISCTYPE.retStr(), v.DISCRATE.retDbl());
                                    if (ALL_GSTPER.retStr() != "")
                                    {
                                        var gst = ALL_GSTPER.Split(',').ToList();
                                        GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                                    }
                                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                                    //v.GSTPER = GSTPER.retDbl();
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
                    v.INCL_DISC = ((v.DISCAMT.retDbl() * (v.GSTPER + 100)) / 100).retDbl().toRound(2);
                    v.DISCONBILLPERROW = ((v.SCMDISCAMT.retDbl() * (v.GSTPER + 100)) / 100).retDbl().toRound(2);
                }
                VE.T_GSTAMT = VE.TsalePos_TBATCHDTL.Sum(a => a.GSTAMT).retDbl();
                VE.T_INCL_DISC = VE.TsalePos_TBATCHDTL.Sum(a => a.INCL_DISC).retDbl();
                #region Return tab
                // fill prodgrpgstper in t_batchdtl
                DataTable R_allprodgrpgstper_data = new DataTable();
                string R_BARNO = (from a in VE.TsalePos_TBATCHDTL_RETURN select a.BARNO).ToArray().retSqlfromStrarray();
                string R_ITCD = (from a in VE.TsalePos_TBATCHDTL_RETURN select a.ITCD).ToArray().retSqlfromStrarray();
                string R_MTRLJOBCD = (from a in VE.TsalePos_TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                string R_ITGRPCD = (from a in VE.TsalePos_TBATCHDTL_RETURN select a.ITGRPCD).ToArray().retSqlfromStrarray();

                R_allprodgrpgstper_data = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retSqlformat(), R_BARNO.retStr(), R_ITCD.retStr(), R_MTRLJOBCD.retStr(), VE.T_TXN.AUTONO.retSqlformat(), R_ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);

                foreach (var v in VE.TsalePos_TBATCHDTL_RETURN)
                {
                    //v.DISC_TYPE = masterHelp.DISC_TYPE();
                    //v.PCSActionList = masterHelp.PCSAction();
                    v.GSTAMT = (v.IGSTAMT + v.CGSTAMT + v.SGSTAMT).retDbl().toRound(2);
                    string R_PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "";
                    v.GSTPER = VE.TsalePos_TBATCHDTL_RETURN.Where(a => a.SLNO == v.TXNSLNO).Sum(b => b.IGSTPER + b.CGSTPER + b.SGSTPER).retDbl();
                    if (R_allprodgrpgstper_data != null && R_allprodgrpgstper_data.Rows.Count > 0)
                    {
                        //var q = (from DataRow dr in R_allprodgrpgstper_data.Rows
                        //         select new
                        //         {
                        //             BALSTOCK = dr["BALQNTY"].retDbl(),
                        //             NEGSTOCK = dr["negstock"].retStr()
                        //         }).FirstOrDefault();

                        var R_DATA = R_allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "' ");
                        if (R_DATA.Count() > 0)
                        {
                            DataTable R_tax_data = R_DATA.CopyToDataTable();
                            if (R_tax_data != null && R_tax_data.Rows.Count > 0)
                            {
                                v.BALSTOCK = R_tax_data.Rows[0]["BALQNTY"].retDbl();
                                v.NEGSTOCK = R_tax_data.Rows[0]["negstock"].retStr();
                                R_PRODGRPGSTPER = R_tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                                if (R_PRODGRPGSTPER != "")
                                {
                                    ALL_GSTPER = salesfunc.retGstPer(R_PRODGRPGSTPER, v.RATE.retDbl(), v.DISCTYPE.retStr(), v.DISCRATE.retDbl());
                                    if (ALL_GSTPER.retStr() != "")
                                    {
                                        var gst = ALL_GSTPER.Split(',').ToList();
                                        GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                                    }
                                    v.PRODGRPGSTPER = R_PRODGRPGSTPER;
                                    v.GSTPER = GSTPER.retDbl();
                                }
                                if (R_tax_data.Rows[0]["barimage"].retStr() != "")
                                {
                                    string barimage = R_tax_data.Rows[0]["barimage"].retStr();
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
                    v.INCL_DISC = ((v.DISCAMT.retDbl() * (v.GSTPER + 100)) / 100).retDbl().toRound(2);
                }
                VE.R_T_GSTAMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.GSTAMT).retDbl();
                VE.R_T_INCL_DISC = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.INCL_DISC).retDbl();
                #endregion

                // fill prodgrpgstper in t_txndtl
                foreach (var v in VE.TsalePos_TBATCHDTL)
                {
                    string PRODGRPGSTPER = "";
                    var tax_data = (from a in VE.TsalePos_TBATCHDTL
                                    where a.TXNSLNO == v.SLNO && a.ITGRPCD == v.ITGRPCD && a.ITCD == a.ITCD && a.STKTYPE == v.STKTYPE
                                    && a.RATE == v.RATE && a.DISCTYPE == v.DISCTYPE && a.DISCRATE == v.DISCRATE && a.TDDISCTYPE == v.TDDISCTYPE
                                     && a.TDDISCRATE == v.TDDISCRATE && a.SCMDISCTYPE == v.SCMDISCTYPE && a.SCMDISCRATE == v.SCMDISCRATE
                                    select new { a.PRODGRPGSTPER }).FirstOrDefault();
                    if (tax_data != null)
                    {
                        PRODGRPGSTPER = tax_data.PRODGRPGSTPER.retStr();
                    }
                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                }
                #region Return tab
                // fill prodgrpgstper in t_txndtl
                foreach (var v in VE.TsalePos_TBATCHDTL_RETURN)
                {
                    string R_PRODGRPGSTPER = "";
                    var R_tax_data = (from a in VE.TsalePos_TBATCHDTL_RETURN
                                      where a.TXNSLNO == v.SLNO && a.ITGRPCD == v.ITGRPCD && a.ITCD == a.ITCD && a.STKTYPE == v.STKTYPE
                                      && a.RATE == v.RATE && a.DISCTYPE == v.DISCTYPE && a.DISCRATE == v.DISCRATE && a.TDDISCTYPE == v.TDDISCTYPE
                                       && a.TDDISCRATE == v.TDDISCRATE && a.SCMDISCTYPE == v.SCMDISCTYPE && a.SCMDISCRATE == v.SCMDISCRATE
                                      select new { a.PRODGRPGSTPER }).FirstOrDefault();
                    if (R_tax_data != null)
                    {
                        R_PRODGRPGSTPER = R_tax_data.PRODGRPGSTPER.retStr();
                    }
                    v.PRODGRPGSTPER = R_PRODGRPGSTPER;
                }
                #endregion

                // total amt
                VE.TOTNOS = VE.T_NOS.retDbl();
                VE.TOTQNTY = VE.T_QNTY.retDbl();
                VE.TOTTAXVAL = VE.T_B_GROSSAMT.retDbl() + VE.A_T_AMOUNT.retDbl();
                VE.TOTTAX = VE.T_CGST_AMT.retDbl() + VE.A_T_CGST.retDbl() + VE.T_SGST_AMT.retDbl() + VE.A_T_SGST.retDbl() + VE.T_IGST_AMT.retDbl() + VE.A_T_IGST.retDbl();

                string str = "select a.SLMSLCD,b.SLNM,a.PER,a.ITAMT,a.BLAMT,a.SLNO from " + Scm + ".t_txnslsmn a," + scmf + ".m_subleg b ";
                str += "where a.SLMSLCD=b.slcd and a.autono='" + VE.T_TXN.AUTONO + "' order by a.slno ";
                var SALSMN_DATA = masterHelp.SQLquery(str);
                VE.TTXNSLSMN = (from DataRow dr in SALSMN_DATA.Rows
                                select new TTXNSLSMN()
                                {
                                    SLMSLCD = dr["SLMSLCD"].retStr(),
                                    SLMSLNM = dr["SLNM"].retStr(),
                                    PER = dr["PER"].retDbl(),
                                    ITAMT = dr["ITAMT"].retDbl(),
                                    BLAMT = dr["BLAMT"].retDbl(),
                                    SLNO = Convert.ToByte(dr["SLNO"]),
                                }).ToList();
                if (VE.DefaultAction != "V" && VE.TTXNSLSMN != null && VE.TTXNSLSMN.Count > 0)
                {
                    int COUNT = 0;
                    List<TTXNSLSMN> TPROGDTL = new List<TTXNSLSMN>();
                    for (int i = 0; i <= VE.TTXNSLSMN.Count - 1; i++)
                    {
                        TTXNSLSMN MBILLDET = new TTXNSLSMN();
                        MBILLDET = VE.TTXNSLSMN[i];
                        TPROGDTL.Add(MBILLDET);
                        COUNT++;
                    }
                    TTXNSLSMN MBILLDET1 = new TTXNSLSMN();

                    int SERIAL = Convert.ToInt32(VE.TTXNSLSMN.Max(a => Convert.ToInt32(a.SLNO)));
                    for (int j = COUNT; j <= 2; j++)
                    {
                        SERIAL = SERIAL + 1;
                        TTXNSLSMN OPENING_BL = new TTXNSLSMN();
                        OPENING_BL.SLNO = SERIAL.retShort();
                        TPROGDTL.Add(OPENING_BL);
                    }

                    VE.TTXNSLSMN = TPROGDTL;
                }
                double S_T_GROSS_AMT = 0; double T_BILL_AMT = 0;
                //c
                for (int p = 0; p <= VE.TTXNSLSMN.Count - 1; p++)
                {
                    //VE.TTXNSLSMN[p].SLNO = (p + 1).retShort();

                    S_T_GROSS_AMT = S_T_GROSS_AMT + VE.TTXNSLSMN[p].ITAMT.retDbl();
                    T_BILL_AMT = T_BILL_AMT + VE.TTXNSLSMN[p].BLAMT.retDbl();
                }
                VE.T_ITAMT = S_T_GROSS_AMT.retDbl();
                VE.T_BLAMT = T_BILL_AMT.retDbl();

                string str2 = "select a.SLNO,a.PYMTCD,c.PYMTNM,a.AMT,a.CARDNO,a.INSTNO,a.INSTDT,a.PYMTREM,a.GLCD,c.PYMTTYPE from " + Scm + ".t_txnpymt a," + Scm + ".t_txnpymt_hdr b," + Scm + ".m_payment c ";
                str2 += "where a.autono=b.autono and  a.PYMTCD=c.PYMTCD and a.autono='" + VE.T_TXN.AUTONO + "' order by a.PYMTCD ";
                var PYMT_DATA = masterHelp.SQLquery(str2);
                VE.TTXNPYMT = (from DataRow dr in PYMT_DATA.Rows
                               select new TTXNPYMT()
                               {
                                   SLNO = dr["SLNO"].retShort(),
                                   PYMTCD = dr["PYMTCD"].retStr(),
                                   PYMTNM = dr["PYMTNM"].retStr(),
                                   AMT = dr["AMT"].retDbl(),
                                   CARDNO = dr["CARDNO"].retStr(),
                                   INSTNO = dr["INSTNO"].retStr(),
                                   INSTDT = dr["INSTDT"].retDateStr(),
                                   PYMTREM = dr["PYMTREM"].retStr(),
                                   GLCD = dr["GLCD"].retStr(),
                                   PYMTTYPE = dr["PYMTTYPE"].retStr(),
                               }).ToList();
                if (VE.DefaultAction == "E" && VE.MENU_PARA != "CADV")
                {
                    var chk = VE.TTXNPYMT.Where(a => a.PYMTCD == "AD").Count();
                    if (chk == 0)
                    {
                        Int16 SERIAL = 1;
                        if (VE.TTXNPYMT != null && VE.TTXNPYMT.Count > 0)
                        {
                            SERIAL = Convert.ToInt16(VE.TTXNPYMT.Max(a => Convert.ToInt32(a.SLNO)));
                            SERIAL++;
                        }
                        var MPAYMENT = (from i in DB.M_PAYMENT join k in DB.M_CNTRL_HDR on i.M_AUTONO equals k.M_AUTONO where k.INACTIVE_TAG == "N" && i.PYMTCD == "AD" select new { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD, PYMTTYPE = i.PYMTTYPE }).OrderBy(a => a.PYMTCD).ToList();
                        if (MPAYMENT != null)
                        {
                            if (VE.TTXNPYMT == null || VE.TTXNPYMT.Count == 0)
                            {
                                VE.TTXNPYMT = (from i in MPAYMENT select new TTXNPYMT { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD, PYMTTYPE = i.PYMTTYPE, SLNO = SERIAL }).ToList();
                            }
                            else
                            {
                                VE.TTXNPYMT.AddRange((from i in MPAYMENT select new TTXNPYMT { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD, PYMTTYPE = i.PYMTTYPE, SLNO = SERIAL }).ToList());
                            }
                        }
                    }
                }
                double T_PYMT_AMT = 0;

                for (int p = 0; p <= VE.TTXNPYMT.Count - 1; p++)
                {
                    T_PYMT_AMT = T_PYMT_AMT + VE.TTXNPYMT[p].AMT.retDbl();

                }
                VE.T_PYMT_AMT = T_PYMT_AMT;
                VE.PAYAMT = T_PYMT_AMT.toRound(2);

                //VE.RETAMT = VE.RETAMT.retStr() == "" ? 0 : VE.RETAMT.retDbl().toRound(2);
                //VE.RETAMT = VE.R_T_NET + VE.A_T_NET;
                VE.RETAMT = VE.TsalePos_TBATCHDTL_RETURN.Sum(a => a.NETAMT).retDbl();
                VE.MEMOAMT = VE.TsalePos_TBATCHDTL.Sum(a => a.NETAMT).retDbl();
                VE.NETDUE = (VE.T_TXN.BLAMT - VE.PAYAMT).retDbl().toRound(2);

                if (VE.MENU_PARA != "CADV")
                {
                    #region Party Order Advance
                    string advno = DB.T_TXN_LINKNO.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(a => a.LINKAUTONO).ToArray().retSqlfromStrarray();
                    if (advno.retStr() != "")
                    {
                        //var data = salesfunc.getPendingPOAdvance("", TXN.DOCDT.retDateStr(), TXN.SLCD, advno, TXN.AUTONO.retSqlformat());
                        var data = salesfunc.getPendingPOAdvance("", VE.T_TXN.DOCDT.retDateStr(), "", advno, VE.T_TXN.AUTONO.retSqlformat());

                        VE.AdvanceList = (from DataRow DR in data.Rows
                                          join b in DB.T_TXN_LINKNO on DR["autono"].retStr() equals b.LINKAUTONO
                                          where b.AUTONO == VE.T_TXN.AUTONO
                                          group DR by new { tchdocno = DR["tchdocno"].ToString(), docdt = DR["docdt"].ToString(), autono = DR["autono"].ToString(), slcd = DR["slcd"].ToString(), SLNM = DR["SLNM"].ToString(), ADJAMT = b.AMT } into X
                                          select new AdvanceList()
                                          {
                                              ADVDOCNO = X.Key.tchdocno,
                                              ADVDOCDT = X.Key.docdt,
                                              ADVAUTONO = X.Key.autono,
                                              SLCD = X.Key.slcd,
                                              SLNM = X.Key.SLNM,
                                              ADVAMT = X.Sum(Z => Z.Field<decimal>("advamt")).retDbl(),
                                              PRVADJAMT = X.Sum(Z => Z.Field<decimal>("adjamt")).retDbl(),
                                              BALADVAMT = X.Sum(Z => Z.Field<decimal>("balamt")).retDbl(),
                                              ADJAMT = X.Key.ADJAMT
                                          }).ToList();
                        for (int p = 0; p <= VE.AdvanceList.Count - 1; p++)
                        {
                            VE.AdvanceList[p].SLNO = Convert.ToInt16(p + 1);
                        }
                        VE.TOTADVAMT = VE.AdvanceList.Select(a => a.ADVAMT).Sum();
                        VE.TOTPRVADJAMT = VE.AdvanceList.Select(a => a.PRVADJAMT).Sum();
                        VE.TOTADJAMT = VE.AdvanceList.Select(a => a.ADJAMT).Sum();
                        //VE.NETPAY = TXN.BLAMT - VE.TOTADJAMT;
                    }
                    #endregion
                }

                if (VE.T_CNTRL_HDR.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            }
            //Cn.DateLock_Entry(VE, DB,   VE.T_CNTRL_HDR.DOCDT.Value);
            return VE;
        }
        public ActionResult SearchPannelData(TransactionSalePosEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT, string SRC_FLAG)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
                Cn.getQueryString(VE);
                List<DocumentType> DocumentType = new List<DocumentType>();
                DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
                string sql = "";

                sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.slcd, c.slnm, c.district, nvl(a.blamt,0) blamt,d.RTDEBCD,e.RTDEBNM,d.nm,d.mobile,nvl(b.cancel,'N')cancel ";
                sql += "from " + scm + ".t_txn a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_subleg c , " + scm + ".T_TXNMEMO d," + scmf + ".M_RETDEB e ";
                sql += "where a.autono=b.autono and a.slcd=c.slcd(+) and a.autono=d.autono(+) and d.RTDEBCD=e.RTDEBCD(+) and  b.doccd in (" + doccd + ") and ";
                if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
                if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
                if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.doconlyno like '%" + SRC_DOCNO.retStr() + "%') and ";
                if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%' or d.RTDEBCD like ' % " + SRC_SLCD.retStr().ToUpper() + " % ' or upper(d.nm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and  ";
                if (SRC_FLAG.retStr() != "") sql += "d.mobile like '%" + SRC_FLAG.retStr() + "%' and ";
                sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
                sql += "order by docdt, docno ";
                DataTable tbl = masterHelp.SQLquery(sql);

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Retail Name" + Cn.GCS() + "Party Name" + Cn.GCS() + "Bill Amt" + Cn.GCS() + "AUTO NO";
                for (int j = 0; j <= tbl.Rows.Count - 1; j++)
                {
                    string cancel = tbl.Rows[j]["cancel"].retStr() == "Y" ? "<b> (Cancelled)</b>" : "";
                    SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + cancel + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["RTDEBNM"] + "</b> (" + tbl.Rows[j]["RTDEBCD"] + ") </td><td>" + tbl.Rows[j]["nm"] + " </td><td class='text-right'>" + Convert.ToDouble(tbl.Rows[j]["blamt"]).ToINRFormat() + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
                }
                return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "5", "5"));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetData(string EFFDT)
        {
            try
            {
                DataTable dt = salesfunc.GetSyscnfgData(EFFDT.retStr());
                if (dt != null && dt.Rows.Count == 1)
                {
                    return Content(masterHelp.ToReturnFieldValues("", dt));
                }
                else {
                    return Content("No Data Found");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        private string TranBarcodeGenerate(string doccd, string lbatchini, string docbarcode, string UNIQNO, int slno)
        {//YRCODE	2,lbatchini	2,TXN UNIQ NO	7,SLNO	4
            var yrcd = CommVar.YearCode(UNQSNO).Substring(2, 2);
            return yrcd + lbatchini + UNIQNO + slno.ToString().PadLeft(4, '0');
        }
        public ActionResult GetTDSDetails(string val, string TAG, string PARTY)
        {
            try
            {
                string linktdscode = "'Y','Z'", menu_para = "SB";
                if (menu_para == "PB" || menu_para == "PR") linktdscode = "'X'";
                if (TAG.retStr() == "") return Content("Enter Document Date");
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.TDSCODE_help(TAG, val, PARTY, "TCS", linktdscode));
                }
                else
                {
                    return Content(masterHelp.TDSCODE_help(TAG, val, PARTY, "TCS", linktdscode));
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
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                var str = masterHelp.SLCD_help(val, Code);
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
        public ActionResult GetPaymentDetails(string val)
        {
            try
            {
                var str = masterHelp.PAYMTCD_help(val);
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
        public ActionResult GetRefRetailDetails(string val, string Code)
        {
            try
            {
                var str = masterHelp.RTDEBCD_help(val);

                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    var MSG = str.IndexOf(Cn.GCS());
                    if (MSG >= 0)
                    {
                        DataTable Taxgrpcd = salesfunc.GetSlcdDetails(Code, "");
                        if (Taxgrpcd.Rows.Count > 0)
                        { str += "^TAXGRPCD=^" + Taxgrpcd.Rows[0]["taxgrpcd"] + Cn.GCS(); }

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
        public ActionResult GetGodownDetails(string val)
        {
            try
            {
                var str = masterHelp.GOCD_help(val);
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
                TransactionSalePosEntry VE = new TransactionSalePosEntry();
                Cn.getQueryString(VE);
                string str = masterHelp.ITGRPCD_help(val, "", Code);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    string glcd = "";
                    switch (VE.MENU_PARA)
                    {
                        case "SBPCK"://Packing Slip
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "SB"://Sales Bill (Agst Packing Slip)
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "SBDIR"://Sales Bill
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "SR"://Sales Return (SRM)
                            glcd = str.retCompValue("SALRETGLCD"); break;
                        case "SBCM"://Cash Memo
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "SBCMR"://Cash Memo Return Note
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "SBEXP"://Sales Bill (Export)
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "PI"://Proforma Invoice
                            glcd = ""; break;
                        case "PB"://Purchase Bill
                            glcd = str.retCompValue("PURGLCD"); break;
                        case "PR"://Purchase Return (PRM)
                            glcd = str.retCompValue("PURRETGLCD"); break;
                        default: glcd = ""; break;
                    }
                    str += "^GLCD=^" + glcd + Cn.GCS();
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
                TransactionSalePosEntry VE = new TransactionSalePosEntry();
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string ITGRPCD = data[0].retStr() == "" ? "" : data[0].retStr().retSqlformat();
                string DOCDT = data[1].retStr();
                if (DOCDT == "")
                {
                    return Content("Please Select Document Date");
                }
                string TAXGRPCD = data[2].retStr();
                string GOCD = data[3].retStr() == "" ? "" : data[3].retStr().retSqlformat();
                string PRCCD = data[4].retStr();
                string MTRLJOBCD = data[5].retStr() == "" ? "" : data[5].retStr().retSqlformat();
                string BARNO = data[6].retStr() == "" ? "" : data[6].retStr().retSqlformat();
                double RATE = data[7].retDbl();
                string DISCTYPE = data[8].retStr();
                double DISCRATE = data[9].retDbl();
                var str = masterHelp.ITCD_help(val, "", data[0].retStr());
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else if (str.IndexOf(Convert.ToChar(Cn.GCS())) >= 0)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "", BARIMAGE = "";
                    DataTable tax_data = new DataTable();
                    if (VE.MENU_PARA == "PB")
                    {
                        tax_data = salesfunc.GetBarHelp(DOCDT.retStr(), GOCD.retStr(), BARNO.retStr(), val.retStr().retSqlformat(), MTRLJOBCD.retStr(), "", ITGRPCD, "", PRCCD.retStr(), TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);

                    }
                    else
                    {
                        tax_data = salesfunc.GetStock(DOCDT.retStr(), GOCD.retStr(), BARNO.retStr(), val.retStr().retSqlformat(), MTRLJOBCD.retStr(), "", ITGRPCD, "", PRCCD.retStr(), TAXGRPCD.retStr());

                    }
                    if (tax_data != null && tax_data.Rows.Count > 0)
                    {
                        PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                        BARIMAGE = tax_data.Rows[0]["BARIMAGE"].retStr();
                        if (PRODGRPGSTPER != "")
                        {
                            ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, RATE, DISCTYPE, DISCRATE);
                            if (ALL_GSTPER.retStr() != "")
                            {
                                var gst = ALL_GSTPER.Split(',').ToList();
                                GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                            }
                        }
                    }
                    str += "^PRODGRPGSTPER=^" + PRODGRPGSTPER + Cn.GCS();
                    str += "^BARIMAGE=^" + BARIMAGE + Cn.GCS();
                    str += "^ALL_GSTPER=^" + ALL_GSTPER + Cn.GCS();
                    str += "^GSTPER=^" + GSTPER + Cn.GCS();
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
                TransactionSalePosEntry VE = new TransactionSalePosEntry();
                //sequence MTRLJOBCD/PARTCD/DOCDT/TAXGRPCD/GOCD/PRCCD/ALLMTRLJOBCD
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string barnoOrStyle = val.retStr();
                string MTRLJOBCD = data[0].retSqlformat();
                string PARTCD = data[1].retStr();
                string DOCDT = data[2].retStr();
                string TAXGRPCD = data[3].retStr();
                string GOCD = data[2].retStr() == "" ? "" : data[4].retStr().retSqlformat();
                string PRCCD = data[5].retStr();
                if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = data[6].retStr(); }
                string BARNO = data[8].retStr() == "" || val.retStr() == "" ? "" : data[8].retStr().ToUpper().retSqlformat();
                bool exactbarno = data[7].retStr() == "Bar" ? true : false;
                if (data[7].retStr() == "Bar")
                {
                    barnoOrStyle = barnoOrStyle.ToUpper();
                }
                if (GOCD == "")
                {
                    return Content("Please fill Godown");
                }
                string str = masterHelp.T_TXN_BARNO_help(barnoOrStyle, VE.MENU_PARA, DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD, "", exactbarno, "", BARNO);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1) return Content(str);
                    string glcd = "";
                    glcd = str.retCompValue("SALGLCD");

                    str += "^GLCD=^" + glcd + Cn.GCS();
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public List<DropDown_list2> DropDown_list2()
        {
            List<DropDown_list2> DTYP = new List<DropDown_list2>();
            DropDown_list2 DTYP3 = new DropDown_list2();
            DTYP3.text = "%";
            DTYP3.value = "P";
            DTYP.Add(DTYP3);
            DropDown_list2 DTYP2 = new DropDown_list2();
            DTYP2.text = "Nos";
            DTYP2.value = "N";
            DTYP.Add(DTYP2);
            DropDown_list2 DTYP1 = new DropDown_list2();
            DTYP1.text = "Qnty";
            DTYP1.value = "Q";
            DTYP.Add(DTYP1);
            DropDown_list2 DTYP4 = new DropDown_list2();
            DTYP4.text = "Fixed";
            DTYP4.value = "F";
            DTYP.Add(DTYP4);
            return DTYP;
        }
        public List<DropDown_list3> DropDown_list3()
        {
            List<DropDown_list3> DTYP = new List<DropDown_list3>();
            DropDown_list3 DTYP3 = new DropDown_list3();
            DTYP3.text = "%";
            DTYP3.value = "P";
            DTYP.Add(DTYP3);
            DropDown_list3 DTYP2 = new DropDown_list3();
            DTYP2.text = "Nos";
            DTYP2.value = "N";
            DTYP.Add(DTYP2);
            DropDown_list3 DTYP1 = new DropDown_list3();
            DTYP1.text = "Qnty";
            DTYP1.value = "Q";
            DTYP.Add(DTYP1);
            DropDown_list3 DTYP4 = new DropDown_list3();
            DTYP4.text = "Fixed";
            DTYP4.value = "F";
            DTYP.Add(DTYP4);
            return DTYP;
        }
        public ActionResult GetOrderDetails(string val, string Code)
        {
            try
            {
                TransactionSalePosEntry VE = new TransactionSalePosEntry();
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string Orderautono = data[2].retStr();
                string Itcd = data[0].retSqlformat();
                string Orderslno = data[1].retStr();
                string slcd = data[3].retStr();
                var str = masterHelp.GetOrderDetails(Orderautono.retSqlformat(), val, VE.MENU_PARA, Itcd.retSqlformat(), Orderslno.retSqlformat(), slcd.retSqlformat());
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
        public ActionResult FillDetailData(TransactionSalePosEntry VE)
        {
            try
            {
                Cn.getQueryString(VE);
                VE.TTXNDTL = (from x in VE.TsalePos_TBATCHDTL
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
                                  x.FLAGMTR,
                                  x.HSNCODE,
                                  x.PRODGRPGSTPER,
                                  x.BALENO,
                                  x.GLCD,
                                  x.ITREM,
                              } into P
                              select new TTXNDTL
                              {
                                  SLNO = P.Key.TXNSLNO.retShort(),
                                  ITGRPCD = P.Key.ITGRPCD,
                                  ITGRPNM = P.Key.ITGRPNM,
                                  MTRLJOBCD = P.Key.MTRLJOBCD,
                                  MTRLJOBNM = P.Key.MTRLJOBNM,
                                  // MTBARCODE = P.Key.MTBARCODE,
                                  ITCD = P.Key.ITCD,
                                  //  ITNM = P.Key.STYLENO + "" + P.Key.ITNM,
                                  ITSTYLE = P.Key.ITSTYLE,
                                  STYLENO = P.Key.STYLENO,
                                  STKTYPE = P.Key.STKTYPE,
                                  UOM = P.Key.UOM,
                                  NOS = P.Sum(A => A.NOS),
                                  QNTY = P.Sum(A => A.QNTY),
                                  FLAGMTR = P.Sum(A => A.FLAGMTR),
                                  BLQNTY = P.Sum(A => A.BLQNTY),
                                  RATE = P.Key.RATE,
                                  DISCTYPE = P.Key.DISCTYPE,
                                  DISCTYPE_DESC = P.Key.DISCTYPE_DESC,
                                  DISCRATE = P.Key.DISCRATE,
                                  TDDISCRATE = P.Key.TDDISCRATE,
                                  TDDISCTYPE = P.Key.TDDISCTYPE,
                                  TDDISCTYPE_DESC = P.Key.TDDISCTYPE_DESC,
                                  SCMDISCRATE = P.Key.SCMDISCRATE,
                                  SCMDISCTYPE = P.Key.SCMDISCTYPE,
                                  SCMDISCTYPE_DESC = P.Key.SCMDISCTYPE_DESC,
                                  AMT = P.Sum(A => A.BLQNTY).retDbl() == 0 ? (P.Sum(A => A.QNTY).retDbl() - P.Sum(A => A.FLAGMTR).retDbl()) * P.Key.RATE.retDbl() : P.Sum(A => A.BLQNTY).retDbl() * P.Key.RATE.retDbl(),
                                  HSNCODE = P.Key.HSNCODE,
                                  PRODGRPGSTPER = P.Key.PRODGRPGSTPER,
                                  BALENO = P.Key.BALENO,
                                  GLCD = P.Key.GLCD,
                                  ITREM = P.Key.ITREM,
                              }).ToList();

                for (int p = 0; p <= VE.TTXNDTL.Count - 1; p++)
                {
                    if (VE.TTXNDTL[p].ALL_GSTPER.retStr() != "")
                    {
                        var tax_data = VE.TTXNDTL[p].ALL_GSTPER.Split(',').ToList();
                        if (tax_data.Count == 1)
                        {
                            VE.TTXNDTL[p].IGSTPER = tax_data[0].retDbl() / 3;
                            VE.TTXNDTL[p].CGSTPER = tax_data[0].retDbl() / 3;
                            VE.TTXNDTL[p].SGSTPER = tax_data[0].retDbl() / 3;
                        }
                        else
                        {
                            VE.TTXNDTL[p].IGSTPER = tax_data[0].retDbl();
                            VE.TTXNDTL[p].CGSTPER = tax_data[1].retDbl();
                            VE.TTXNDTL[p].SGSTPER = tax_data[2].retDbl();
                        }

                    }
                }
                VE.T_NOS = VE.TTXNDTL.Select(a => a.NOS).Sum().retDbl();
                VE.T_QNTY = VE.TTXNDTL.Select(a => a.QNTY).Sum().retDbl();
                VE.T_AMT = VE.TTXNDTL.Select(a => a.AMT).Sum().retDbl();
                VE.T_DISCAMT = VE.TTXNDTL.Select(a => a.DISCAMT).Sum().retDbl();
                VE.T_B_GROSSAMT = VE.TTXNDTL.Select(a => a.TXBLVAL).Sum().retDbl();
                VE.T_IGST_AMT = VE.TTXNDTL.Select(a => a.IGSTAMT).Sum().retDbl();
                VE.T_CGST_AMT = VE.TTXNDTL.Select(a => a.CGSTAMT).Sum().retDbl();
                VE.T_SGST_AMT = VE.TTXNDTL.Select(a => a.SGSTAMT).Sum().retDbl();
                VE.T_CESS_AMT = VE.TTXNDTL.Select(a => a.CESSAMT).Sum().retDbl();
                VE.T_NET_AMT = VE.TTXNDTL.Select(a => a.NETAMT).Sum().retDbl();
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_POS_DETAIL", VE);
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
        private void FreightCharges(TransactionSalePosEntry VE, string AUTO_NO)
        {
            try
            {
                double A_T_CURR_AMT = 0; double A_T_AMT = 0; double A_T_TAXABLE = 0; double A_T_IGST_AMT = 0; double A_T_CGST_AMT = 0;
                double A_T_SGST_AMT = 0; double A_T_CESS_AMT = 0; double A_T_DUTY_AMT = 0; double A_T_NET_AMT = 0; double IGST_PER = 0; double CGST_PER = 0; double SGST_PER = 0;
                double CESS_PER = 0; double DUTY_PER = 0;
                string sql = "", scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
                Cn.getQueryString(VE);
                string S_P = "";
                S_P = "S";

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
                    VE.TTXNAMT[p].SLNO = Convert.ToInt16(p + 1);

                    VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT.Value + VE.TTXNAMT[p].IGSTAMT.Value + VE.TTXNAMT[p].CGSTAMT.Value + VE.TTXNAMT[p].SGSTAMT.Value + VE.TTXNAMT[p].CESSAMT.Value + VE.TTXNAMT[p].DUTYAMT.Value;
                    //VE.TTXNAMT[p].IGSTPER = IGST_PER;
                    //VE.TTXNAMT[p].CGSTPER = CGST_PER;
                    //VE.TTXNAMT[p].SGSTPER = SGST_PER;
                    //VE.TTXNAMT[p].CESSPER = CESS_PER;
                    A_T_CURR_AMT = A_T_CURR_AMT + VE.TTXNAMT[p].CURR_AMT.Value;
                    A_T_AMT = A_T_AMT + VE.TTXNAMT[p].AMT.Value;
                    A_T_IGST_AMT = A_T_IGST_AMT + VE.TTXNAMT[p].IGSTAMT.Value;
                    A_T_CGST_AMT = A_T_CGST_AMT + VE.TTXNAMT[p].CGSTAMT.Value;
                    A_T_SGST_AMT = A_T_SGST_AMT + VE.TTXNAMT[p].SGSTAMT.Value;
                    A_T_CESS_AMT = A_T_CESS_AMT + VE.TTXNAMT[p].CESSAMT.Value;
                    A_T_DUTY_AMT = A_T_DUTY_AMT + VE.TTXNAMT[p].DUTYAMT.Value;
                    //A_T_NET_AMT = A_T_NET_AMT + A_T_AMT + A_T_IGST_AMT + A_T_CGST_AMT + A_T_SGST_AMT + A_T_CESS_AMT + A_T_DUTY_AMT;
                    A_T_NET_AMT += VE.TTXNAMT[p].NETAMT.retDbl();
                }
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

        public ActionResult DocumentDateChng(TransactionSalePosEntry VE, string docdt)
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
                VE.INCLRATEASK = VE.M_SYSCNFG.INC_RATE.retStr();
                ModelState.Clear();
                VE.DefaultView = true;
                VE.DISC_TYPE = masterHelp.DISC_TYPE();
                VE.PCSActionList = masterHelp.PCSAction();
                var GRID_DATA = RenderRazorViewToString(ControllerContext, "_T_SALE_POS_PRODUCT", VE);
                var RGRID_DATA = RenderRazorViewToString(ControllerContext, "_T_SALE_POS_RETURN", VE);
                return Content(str + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + GRID_DATA + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + RGRID_DATA);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteRowBarno(TransactionSalePosEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                List<TsalePos_TBATCHDTL> TsalePos_TBATCHDTL = new List<TsalePos_TBATCHDTL>();
                VE.DISC_TYPE = masterHelp.DISC_TYPE();
                VE.PCSActionList = masterHelp.PCSAction();
                int count = 0;
                for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                {
                    if (VE.TsalePos_TBATCHDTL[i].Checked == false)
                    {
                        count += 1;
                        TsalePos_TBATCHDTL item = new TsalePos_TBATCHDTL();
                        item = VE.TsalePos_TBATCHDTL[i];
                        item.SLNO = Convert.ToInt16(count);
                        //item.DISC_TYPE = masterHelp.DISC_TYPE();
                        //item.PCSActionList = masterHelp.PCSAction();
                        var brimgs = VE.TsalePos_TBATCHDTL[i].BarImages.retStr().Split((char)179);
                        string a = brimgs[0].retStr();
                        if (a != "")
                        { item.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr(); }
                        else { item.BarImagesCount = ""; }

                        TsalePos_TBATCHDTL.Add(item);
                    }
                }
                VE.TsalePos_TBATCHDTL = TsalePos_TBATCHDTL;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_POS_PRODUCT", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteReturnRow(TransactionSalePosEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                List<TsalePos_TBATCHDTL_RETURN> TsalePos_TBATCHDTL = new List<TsalePos_TBATCHDTL_RETURN>();
                int count = 1000;
                VE.DISC_TYPE = masterHelp.DISC_TYPE();
                VE.PCSActionList = masterHelp.PCSAction();
                for (int i = 0; i <= VE.TsalePos_TBATCHDTL_RETURN.Count - 1; i++)
                {
                    if (VE.TsalePos_TBATCHDTL_RETURN[i].Checked == false)
                    {
                        count += 1;
                        TsalePos_TBATCHDTL_RETURN item = new TsalePos_TBATCHDTL_RETURN();
                        item = VE.TsalePos_TBATCHDTL_RETURN[i];
                        item.SLNO = Convert.ToInt16(count);
                        //item.DISC_TYPE = masterHelp.DISC_TYPE();
                        //item.PCSActionList = masterHelp.PCSAction();
                        var brimgs = VE.TsalePos_TBATCHDTL_RETURN[i].BarImages.retStr().Split((char)179);
                        string a = brimgs[0].retStr();
                        if (a != "")
                        { item.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr(); }
                        else { item.BarImagesCount = ""; }
                        TsalePos_TBATCHDTL.Add(item);
                    }
                }
                VE.TsalePos_TBATCHDTL_RETURN = TsalePos_TBATCHDTL;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_POS_RETURN", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteRow(TransactionSalePosEntry VE, string TABLE = "")
        {
            try
            {
                if (TABLE == "tbl_TSalePayment")
                {
                    #region tbl_TSalePayment
                    List<TTXNPYMT> TTXNPYMT = new List<TTXNPYMT>();
                    int count = 0;
                    for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                    {
                        if (VE.TTXNPYMT[i].Checked == false)
                        {
                            count += 1;
                            TTXNPYMT item = new TTXNPYMT();
                            item = VE.TTXNPYMT[i];
                            item.SLNO = Convert.ToInt16(count);
                            TTXNPYMT.Add(item);
                        }
                    }
                    VE.TTXNPYMT = TTXNPYMT;
                    ModelState.Clear();
                    VE.DefaultView = true;
                    return PartialView("_T_Sale_Payment", VE);
                    #endregion
                }
                else {
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    List<TTXNSLSMN> ITEMSIZE = new List<TTXNSLSMN>();
                    int count = 0;
                    for (int i = 0; i <= VE.TTXNSLSMN.Count - 1; i++)
                    {
                        if (VE.TTXNSLSMN[i].Checked == false)
                        {
                            count += 1;
                            TTXNSLSMN item = new TTXNSLSMN();
                            item = VE.TTXNSLSMN[i];
                            item.SLNO = count.retShort();
                            ITEMSIZE.Add(item);
                        }

                    }
                    VE.TTXNSLSMN = ITEMSIZE;
                    ModelState.Clear();
                    VE.DefaultView = true;
                    return PartialView("_T_SALE_POS_SALESMAN", VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddRowPYMT(TransactionSalePosEntry VE, int COUNT, string TAG)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Cn.getQueryString(VE);
            if (VE.TTXNPYMT == null)
            {
                List<TTXNPYMT> TPROGDTL1 = new List<TTXNPYMT>();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = 0;
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        TTXNPYMT MBILLDET = new TTXNPYMT();
                        MBILLDET.SLNO = SERIAL.retShort();
                        TPROGDTL1.Add(MBILLDET);
                    }
                }
                else
                {
                    TTXNPYMT MBILLDET = new TTXNPYMT();
                    MBILLDET.SLNO = 1;
                    TPROGDTL1.Add(MBILLDET);
                }
                VE.TTXNPYMT = TPROGDTL1;
            }
            else
            {
                List<TTXNPYMT> TPROGDTL = new List<TTXNPYMT>();
                for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                {
                    TTXNPYMT MBILLDET = new TTXNPYMT();
                    MBILLDET = VE.TTXNPYMT[i];
                    TPROGDTL.Add(MBILLDET);
                }
                TTXNPYMT MBILLDET1 = new TTXNPYMT();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = Convert.ToInt32(VE.TTXNPYMT.Max(a => Convert.ToInt32(a.SLNO)));
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        TTXNPYMT OPENING_BL = new TTXNPYMT();
                        OPENING_BL.SLNO = SERIAL.retShort();
                        TPROGDTL.Add(OPENING_BL);
                    }
                }
                else
                {
                    MBILLDET1.SLNO = Convert.ToInt16(Convert.ToByte(VE.TTXNPYMT.Max(a => Convert.ToInt32(a.SLNO))) + 1);
                    TPROGDTL.Add(MBILLDET1);
                }
                VE.TTXNPYMT = TPROGDTL;
            }
            //VE.TPROGDTL.ForEach(a => a.DRCRTA = masterHelp.DR_CR().OrderByDescending(s => s.text).ToList());
            VE.DefaultView = true;
            return PartialView("_T_SALE_POS_PAYMENT", VE);
        }
        public ActionResult AddRow(TransactionSalePosEntry VE, int COUNT, string TAG)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Cn.getQueryString(VE);
            if (VE.TTXNSLSMN == null)
            {
                List<TTXNSLSMN> TPROGDTL1 = new List<TTXNSLSMN>();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = 0;
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        TTXNSLSMN MBILLDET = new TTXNSLSMN();
                        MBILLDET.SLNO = SERIAL.retShort();
                        TPROGDTL1.Add(MBILLDET);
                    }
                }
                else
                {
                    TTXNSLSMN MBILLDET = new TTXNSLSMN();
                    MBILLDET.SLNO = 1;
                    TPROGDTL1.Add(MBILLDET);
                }
                VE.TTXNSLSMN = TPROGDTL1;
            }
            else
            {
                List<TTXNSLSMN> TPROGDTL = new List<TTXNSLSMN>();
                for (int i = 0; i <= VE.TTXNSLSMN.Count - 1; i++)
                {
                    TTXNSLSMN MBILLDET = new TTXNSLSMN();
                    MBILLDET = VE.TTXNSLSMN[i];
                    TPROGDTL.Add(MBILLDET);
                }
                TTXNSLSMN MBILLDET1 = new TTXNSLSMN();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = Convert.ToInt32(VE.TTXNSLSMN.Max(a => Convert.ToInt32(a.SLNO)));
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        TTXNSLSMN OPENING_BL = new TTXNSLSMN();
                        OPENING_BL.SLNO = SERIAL.retShort();
                        TPROGDTL.Add(OPENING_BL);
                    }
                }
                else
                {
                    MBILLDET1.SLNO = Convert.ToInt16(Convert.ToByte(VE.TTXNSLSMN.Max(a => Convert.ToInt32(a.SLNO))) + 1);
                    TPROGDTL.Add(MBILLDET1);
                }
                VE.TTXNSLSMN = TPROGDTL;
            }
            //VE.TPROGDTL.ForEach(a => a.DRCRTA = masterHelp.DR_CR().OrderByDescending(s => s.text).ToList());
            VE.DefaultView = true;
            return PartialView("_T_SALE_POS_SALESMAN", VE);
        }
        public ActionResult DeleteRowPYMT(TransactionSalePosEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<TTXNPYMT> ITEMSIZE = new List<TTXNPYMT>();
                int count = 0;
                for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                {
                    if (VE.TTXNPYMT[i].Checked == false)
                    {
                        count += 1;
                        TTXNPYMT item = new TTXNPYMT();
                        item = VE.TTXNPYMT[i];
                        item.SLNO = count.retShort();
                        ITEMSIZE.Add(item);
                    }

                }
                VE.TTXNPYMT = ITEMSIZE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_POS_PAYMENT", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(TransactionSalePosEntry VE)
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
        public ActionResult DeleteDOCRow(TransactionSalePosEntry VE)
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
        public ActionResult cancelRecords(TransactionSalePosEntry VE, string par1)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                Cn.getQueryString(VE);
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
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult ParkRecord(FormCollection FC, TransactionSalePosEntry stream, string MNUDET, string UNQSNO)
        {
            try
            {
                if (stream.T_TXN.DOCCD.retStr() != "")
                {
                    stream.T_CNTRL_HDR.DOCCD = stream.T_TXN.DOCCD.retStr();
                }
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
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionSalePosEntry");
                return url;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message;
            }
        }
        public dynamic SAVE(FormCollection FC, TransactionSalePosEntry VE, string othr_para = "")
        {
            Cn.getQueryString(VE);
            string ContentFlg = "";
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //  Oracle Queries
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "";
            string[] dbsql1;
            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            DB.Configuration.ValidateOnSaveEnabled = false;
            T_CNTRL_HDR TCH = new T_CNTRL_HDR();
            try
            {
                DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();

                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);

                if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                {
                    DataTable syscnfgdt = salesfunc.GetSyscnfgData(VE.T_TXN.DOCDT.retDateStr());
                    //if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                    //{
                    //    if (VE.T_TXNMEMO.NM.retStr() == "")
                    //    {
                    //        VE.T_TXNMEMO.NM = syscnfgdt.Rows[0]["RTDEBNM"].retStr();
                    //    }
                    //    if (VE.T_TXNMEMO.MOBILE.retStr() == "")
                    //    {
                    //        VE.T_TXNMEMO.MOBILE = syscnfgdt.Rows[0]["MOBILE"].retStr();
                    //    }
                    //    if (VE.T_TXNMEMO.ADDR.retStr() == "")
                    //    {
                    //        var addrs = syscnfgdt.Rows[0]["add1"].retStr() + " " + syscnfgdt.Rows[0]["add2"].retStr() + " " + syscnfgdt.Rows[0]["add3"].retStr();
                    //        VE.T_TXNMEMO.ADDR = addrs;
                    //    }
                    //    if (VE.T_TXNMEMO.CITY.retStr() == "")
                    //    {
                    //        VE.T_TXNMEMO.CITY = syscnfgdt.Rows[0]["city"].retStr();
                    //    }
                    //}

                    T_TXN TTXN = new T_TXN();
                    T_TXNMEMO TTXNMEMO = new T_TXNMEMO();
                    T_TXNPYMT_HDR TTXNPYMTHDR = new T_TXNPYMT_HDR();
                    T_TXNOTH TTXNOTH = new T_TXNOTH();
                    //T_TXN_LINKNO TTXNLINKNO = new T_TXN_LINKNO();
                    T_CNTRL_DOC_PASS TCDP = new T_CNTRL_DOC_PASS();

                    string DOCPATTERN = "";
                    string docpassrem = "";
                    bool blactpost = true, blgstpost = true;
                    TTXN.DOCDT = VE.T_TXN.DOCDT;
                    string Ddate = Convert.ToString(TTXN.DOCDT);
                    TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                    string auto_no = ""; string Month = "";

                    #region Posting to finance Preparation
                    string expglcd = "";
                    string stkdrcr = "C", strqty = " Q=" + VE.TOTQNTY.toRound(2).ToString();
                    string trcd = "TR";
                    string revcharge = "";
                    string dr = ""; string cr = ""; int isl = 0; string strrem = "";
                    double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double rdbqty = 0; double dbamt = 0; double dbcurramt = 0;
                    double dbDrAmt = 0, dbCrAmt = 0;
                    blactpost = true; blgstpost = true;
                    if (VE.MENU_PARA == "CADV")
                    {
                        blactpost = false; blgstpost = false;
                    }
                    /* string parglcd = "saldebglcd"*/
                    string parglcd = "", parclass1cd = "", class2cd = "", tcsgl = "", prodglcd = "", prodrglcd = "", rogl = "", glcd = "", rglcd = "", slmslcd = "";
                    string strblno = "", strbldt = "", strduedt = "", strrefno = "", strvtype = "BL";
                    dr = "D"; cr = "C";
                    if (VE.MENU_PARA == "CADV")
                    {
                        strvtype = "ADV";
                    }
                    TTXN.SLCD = VE.RETDEBSLCD;
                    if (TTXN.SLCD.retStr() == "")
                    {
                        ContentFlg = "Debtor/Creditor code not setup"; goto dbnotsave;
                    }
                    string sslcd = TTXN.SLCD;
                    if (VE.PSLCD.retStr() != "") sslcd = VE.PSLCD.ToString();
                    if (VE.TTXNSLSMN != null)
                    { if (VE.TTXNSLSMN[0].SLMSLCD.retStr() != "") slmslcd = VE.TTXNSLSMN[0].SLMSLCD.retStr(); }
                    if (VE.TsalePos_TBATCHDTL != null) if (VE.TsalePos_TBATCHDTL[0].GLCD.retStr() != "") glcd = VE.TsalePos_TBATCHDTL[0].GLCD.retStr();
                    if (VE.TsalePos_TBATCHDTL_RETURN != null) if (VE.TsalePos_TBATCHDTL_RETURN[0].GLCD.retStr() != "") rglcd = VE.TsalePos_TBATCHDTL_RETURN[0].GLCD.retStr();

                    switch (VE.MENU_PARA)
                    {
                        case "SBCM":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Cash Memo" + strqty; break;
                        case "SBCMR":
                            stkdrcr = "D"; dr = "C"; cr = "D"; trcd = "SC"; strrem = "Cash Memo Credit Note" + strqty; break;
                        case "CADV":
                            stkdrcr = "N"; trcd = "SB"; strrem = "Party Order Advance" + strqty; break;
                    }

                    string slcdlink = "", slcdpara = VE.MENU_PARA;
                    //if (VE.MENU_PARA == "PR") slcdpara = "PB";
                    string sql = "";
                    //sql = "select class1cd, retdebslcd, saldebglcd glcd from " + CommVar.CurSchema(UNQSNO) + ".m_syscnfg ";
                    //DataTable tblsys = masterHelp.SQLquery(sql);
                    //if (tblsys.Rows.Count == 0)
                    //{
                    //    dberrmsg = "Debtor/Creditor code not setup"; goto dbnotsave;
                    //}
                    //sslcd = tblsys.Rows[0]["retdebslcd"].retStr();

                    sql = "select b.rogl, b.tcsgl, a.class1cd, null class2cd, ";
                    sql += "'" + glcd + "' prodglcd,'" + rglcd + "' prodrglcd, ";
                    sql += "b.igst_s igstcd, b.cgst_s cgstcd, b.sgst_s sgstcd, b.cess_s cesscd, b.duty_s dutycd, ";
                    sql += "a.saldebglcd parglcd, ";
                    sql += "igst_rvi, cgst_rvi, sgst_rvi, cess_rvi, igst_rvo, cgst_rvo, sgst_rvo, cess_rvo ";
                    sql += "from " + scm1 + ".m_syscnfg a, " + scmf + ".m_post b, " + scm1 + ".m_subleg_com c ";
                    sql += "where c.slcd in('" + sslcd + "',null) and ";
                    sql += "c.compcd in ('" + COM + "',null) ";
                    DataTable tbl = masterHelp.SQLquery(sql);
                    if (tbl != null && tbl.Rows.Count > 0)
                    {
                        parglcd = tbl.Rows[0]["parglcd"].retStr() == "" ? "" : tbl.Rows[0]["parglcd"].retStr(); parclass1cd = tbl.Rows[0]["class1cd"].retStr() == "" ? "" : tbl.Rows[0]["class1cd"].retStr();
                        class2cd = tbl.Rows[0]["class2cd"].retStr() == "" ? "" : tbl.Rows[0]["class2cd"].retStr();
                        tcsgl = tbl.Rows[0]["tcsgl"].retStr() == "" ? "" : tbl.Rows[0]["tcsgl"].retStr();
                        prodglcd = tbl.Rows[0]["prodglcd"].retStr() == "" ? "" : tbl.Rows[0]["prodglcd"].retStr();
                        prodrglcd = tbl.Rows[0]["prodrglcd"].retStr() == "" ? "" : tbl.Rows[0]["prodrglcd"].retStr();
                        rogl = tbl.Rows[0]["rogl"].retStr() == "" ? "" : tbl.Rows[0]["rogl"].retStr();
                    }
                    if (VE.MENU_PARA == "CADV")
                    {
                        parglcd = VE.T_TXN.PARGLCD.retStr();
                        prodglcd = VE.T_TXN.GLCD.retStr();
                        glcd = VE.T_TXN.GLCD.retStr();
                    }
                    //   Calculate Others Amount from amount tab to distrubute into txndtl table
                    double _amtdist = 0, _baldist = 0, _Rbaldist = 0, _rpldist = 0, _Rrpldist = 0, _mult = 1;
                    double _amtdistq = 0, _baldistq = 0, _Rbaldistq = 0, _rpldistq = 0, _Rrpldistq = 0;
                    double titamt = 0, titqty = 0, rtitamt = 0, rtitqty = 0;
                    int lastitemno = 0, rlastitemno = 0;
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
                    if (VE.TsalePos_TBATCHDTL != null)
                    {
                        for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                        {
                            if (VE.TsalePos_TBATCHDTL[i].SLNO != 0 && VE.TsalePos_TBATCHDTL[i].ITCD != null)
                            {
                                #region Negetive Stock Chking
                                if (VE.MENU_PARA == "SBCM")
                                {
                                    if (VE.TsalePos_TBATCHDTL[i].QNTY != 0)
                                    {
                                        var BALSTOCK = VE.TsalePos_TBATCHDTL[i].BALSTOCK.retDbl();
                                        var NEGSTOCK = VE.TsalePos_TBATCHDTL[i].NEGSTOCK;
                                        var balancestock = BALSTOCK - VE.TsalePos_TBATCHDTL[i].QNTY;
                                        if (balancestock < 0)
                                        {
                                            if (NEGSTOCK != "Y")
                                            {
                                                ContentFlg = "Quantity should not be grater than Stock at slno " + VE.TsalePos_TBATCHDTL[i].SLNO;
                                                goto dbnotsave;
                                            }
                                        }
                                    }
                                }
                                #endregion

                                titamt = titamt + VE.TsalePos_TBATCHDTL[i].GROSSAMT.retDbl();
                                titqty = titqty + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].QNTY);
                                lastitemno = i;
                            }
                        }
                    }
                    if (VE.TsalePos_TBATCHDTL_RETURN != null)
                    {
                        for (int i = 0; i <= VE.TsalePos_TBATCHDTL_RETURN.Count - 1; i++)
                        {
                            if (VE.TsalePos_TBATCHDTL_RETURN[i].SLNO != 0 && VE.TsalePos_TBATCHDTL_RETURN[i].ITCD != null)
                            {
                                rtitamt = rtitamt + VE.TsalePos_TBATCHDTL_RETURN[i].GROSSAMT.retDbl();
                                rtitqty = rtitqty + Convert.ToDouble(VE.TsalePos_TBATCHDTL_RETURN[i].QNTY);
                                rlastitemno = i;
                            }
                        }
                    }

                    _baldist = _amtdist; _baldistq = _amtdistq; _Rbaldistq = _amtdistq; _Rbaldist = _amtdist;
                    #endregion
                    if (VE.DefaultAction == "A")
                    {
                        TTXN.EMD_NO = 0;
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        //TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        if (VE.M_SLIP_NO != null)
                        {
                            TTXN.DOCNO = Convert.ToString(VE.M_SLIP_NO).PadLeft(6, '0');
                        }
                        else
                        {
                            TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        }
                        DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                        auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
                        TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                        Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        //TempData["LASTGOCD" + VE.MENU_PARA] = VE.T_TXN.GOCD;
                        //TCH = Cn.T_CONTROL_HDR(TTXN.DOCCD, TTXN.DOCDT, TTXN.DOCNO, TTXN.AUTONO, Month, DOCPATTERN, VE.DefaultAction, scm1, null, null, "".retDbl(), null);
                        TCH = Cn.T_CONTROL_HDR(TTXN.DOCCD, TTXN.DOCDT, TTXN.DOCNO, TTXN.AUTONO, Month, DOCPATTERN, VE.DefaultAction, scm1, null, null, "".retDbl(), null, VE.Audit_REM);
                        if (VE.MENU_PARA == "CADV")
                        {
                            TempData[GetLastTempdataName("LASTPARGLCD", VE.T_TXN.DOCCD)] = VE.T_TXN.PARGLCD;
                            TempData[GetLastTempdataName("LASTGLCD", VE.T_TXN.DOCCD)] = VE.T_TXN.GLCD;
                        }
                    }
                    else
                    {
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        TTXN.DOCNO = VE.T_TXN.DOCNO;
                        TTXN.AUTONO = VE.T_TXN.AUTONO;
                        Month = VE.T_CNTRL_HDR.MNTHCD;
                        TTXN.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                        DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        TTXN.DTAG = "E";
                    }
                    TTXN.DOCTAG = VE.MENU_PARA.retStr().Length > 2 ? VE.MENU_PARA.retStr().Remove(2) : VE.MENU_PARA.retStr();
                    if (VE.MENU_PARA == "CADV")
                    {
                        TTXN.DOCTAG = "CAD";
                        TTXN.GLCD = prodglcd;
                    }

                    TTXN.GOCD = VE.T_TXN.GOCD;
                    TTXN.DUEDAYS = VE.T_TXN.DUEDAYS;
                    TTXN.PARGLCD = parglcd;
                    TTXN.CLASS1CD = parclass1cd;
                    TTXN.MENU_PARA = VE.T_TXN.MENU_PARA;
                    TTXN.REVCHRG = VE.T_TXN.REVCHRG;
                    TTXN.ROAMT = VE.T_TXN.ROAMT;
                    TTXN.BLAMT = VE.T_TXN.BLAMT;
                    TTXN.INCL_RATE = VE.INC_RATE == true ? "Y" : "";
                    if (VE.RoundOff == true) { TTXN.ROYN = "Y"; } else { TTXN.ROYN = "N"; }
                    if (VE.DefaultAction == "E")
                    {
                        dbsql = masterHelp.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        //dbsql = masterHelp.TblUpdt("t_batchmst_price", TTXN.AUTONO, "E");
                        //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                        var comp = DB1.T_BATCHMST.Where(x => x.AUTONO == TTXN.AUTONO).OrderBy(s => s.AUTONO).ToList();
                        foreach (var v in comp)
                        {
                            if (!VE.TsalePos_TBATCHDTL.Where(s => s.BARNO == v.BARNO && s.SLNO == v.SLNO).Any())
                            {

                                dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E", "S", "BARNO = '" + v.BARNO + "' and SLNO = '" + v.SLNO + "' ");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                                dbsql = masterHelp.TblUpdt("t_batchmst_price", TTXN.AUTONO, "E", "S", "BARNO = '" + v.BARNO + "'");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            }
                            if (!VE.TsalePos_TBATCHDTL_RETURN.Where(s => s.BARNO == v.BARNO && s.SLNO == v.SLNO).Any())
                            {

                                dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E", "S", "BARNO = '" + v.BARNO + "' and SLNO = '" + v.SLNO + "' ");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                                dbsql = masterHelp.TblUpdt("t_batchmst_price", TTXN.AUTONO, "E", "S", "BARNO = '" + v.BARNO + "'");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            }
                        }

                        dbsql = masterHelp.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnmemo", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_txn_linkno", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_txnamt", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnpymt", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnslsmn", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        // finance
                        dbsql = masterHelp.finTblUpdt("t_vch_gst", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_bl_adj", TTXN.AUTONO, "E");
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

                    // -------------------------Other Info--------------------------//
                    TTXNOTH.AUTONO = TTXN.AUTONO;
                    TTXNOTH.EMD_NO = TTXN.EMD_NO;
                    TTXNOTH.CLCD = TTXN.CLCD;
                    TTXNOTH.DTAG = TTXN.DTAG;
                    TTXNOTH.TAXGRPCD = VE.T_TXNOTH.TAXGRPCD;
                    TTXNOTH.PRCCD = VE.T_TXNOTH.PRCCD;
                    TTXNOTH.BLTYPE = VE.T_TXNOTH.BLTYPE;

                    //----------------------------------------------------------//

                    TTXNMEMO.EMD_NO = TTXN.EMD_NO;
                    TTXNMEMO.CLCD = TTXN.CLCD;
                    TTXNMEMO.DTAG = TTXN.DTAG;
                    TTXNMEMO.TTAG = TTXN.TTAG;
                    TTXNMEMO.AUTONO = TTXN.AUTONO;
                    TTXNMEMO.RTDEBCD = VE.T_TXNMEMO.RTDEBCD;
                    TTXNMEMO.NM = VE.T_TXNMEMO.NM.retStr();
                    TTXNMEMO.MOBILE = VE.T_TXNMEMO.MOBILE;
                    TTXNMEMO.CITY = VE.T_TXNMEMO.CITY;
                    TTXNMEMO.ADDR = VE.T_TXNMEMO.ADDR;
                    //----------------------------------------------------------//
                    // -------------------------T_TXNPYMT_HDR--------------------------//   
                    TTXNPYMTHDR.EMD_NO = TTXN.EMD_NO;
                    TTXNPYMTHDR.CLCD = TTXN.CLCD;
                    TTXNPYMTHDR.DTAG = TTXN.DTAG;
                    TTXNPYMTHDR.TTAG = TTXN.TTAG;
                    TTXNPYMTHDR.AUTONO = TTXN.AUTONO;
                    TTXNPYMTHDR.RTDEBCD = VE.T_TXNMEMO.RTDEBCD;
                    TTXNPYMTHDR.DRCR = stkdrcr;
                    //----------------------------------------------------------//

                    //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD, TTXN.BLAMT.retDbl(), VE.Audit_REM);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(TTXN, VE.DefaultAction);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(TTXNOTH);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(TTXNMEMO);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(TTXNPYMTHDR, VE.DefaultAction);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    //  create header record in finschema
                    if (blactpost == true || blgstpost == true || VE.MENU_PARA == "CADV")
                    {
                        Cn.Create_DOCCD(UNQSNO, "F", TTXN.DOCCD);
                        //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD, TTXN.BLAMT.retDbl(), VE.Audit_REM);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        double currrt = 111;
                        //if (TTXN.CURRRT != null) currrt = Convert.ToDouble(TTXN.CURRRT);
                        dbsql = masterHelp.InsVch_Hdr(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, null, null, "Y", null, trcd, "", "", TTXN.CURR_CD, currrt, "", revcharge);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        T_TXNEWB TTXNEWB = new T_TXNEWB();
                        string action = "A";

                        ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                        var txnweb_data = DB1.T_TXNEWB.Where(a => a.AUTONO == TTXN.AUTONO).Select(b => b.AUTONO).ToList();
                        if (txnweb_data != null && txnweb_data.Count > 0)
                        {
                            action = "E";
                        }

                        //-------------------------EWB--------------------------//
                        TTXNEWB.AUTONO = TTXN.AUTONO;
                        TTXNEWB.EMD_NO = TTXN.EMD_NO;
                        TTXNEWB.CLCD = TTXN.CLCD;
                        TTXNEWB.DTAG = TTXN.DTAG;
                        //TTXNEWB.TRANSLCD = VE.T_TXNTRANS.TRANSLCD;
                        //TTXNEWB.EWAYBILLNO = VE.T_TXNTRANS.EWAYBILLNO;
                        //TTXNEWB.LRNO = VE.T_TXNTRANS.LRNO;
                        //TTXNEWB.LRDT = VE.T_TXNTRANS.LRDT;
                        //TTXNEWB.LORRYNO = VE.T_TXNTRANS.LORRYNO;
                        //TTXNEWB.TRANSMODE = VE.T_TXNTRANS.TRANSMODE;
                        //TTXNEWB.VECHLTYPE = VE.T_TXNTRANS.VECHLTYPE;
                        TTXNEWB.TRANSLCD = null;
                        TTXNEWB.EWAYBILLNO = null;
                        TTXNEWB.LRNO = null;
                        TTXNEWB.LRDT = null;
                        TTXNEWB.LORRYNO = null;
                        TTXNEWB.TRANSMODE = null;
                        TTXNEWB.VECHLTYPE = null;
                        TTXNEWB.GOCD = VE.T_TXN.GOCD;
                        //----------------------------------------------------------//

                        dbsql = masterHelp.RetModeltoSql(TTXNEWB, action, CommVar.FinSchema(UNQSNO));
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    }

                    bool recoexist = false;
                    bool isNewBatch = false;
                    string barno = "";
                    string Action = "A", SqlCondition = "";
                    VE.BALEYR = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).Year.retStr();
                    if (VE.TsalePos_TBATCHDTL != null)
                    {
                        for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                        {
                            if (VE.TsalePos_TBATCHDTL[i].SLNO != 0 && VE.TsalePos_TBATCHDTL[i].ITCD != null && VE.TsalePos_TBATCHDTL[i].QNTY != 0 && VE.TsalePos_TBATCHDTL[i].MTRLJOBCD != null && VE.TsalePos_TBATCHDTL[i].STKTYPE != null)
                            {
                                bool free = false;//for sachi saree free item
                                if (VE.TsalePos_TBATCHDTL[i].TXBLVAL.retDbl() == 0) free = true;
                                if (VE.TsalePos_TBATCHDTL[i].IGSTAMT.retDbl() + VE.TsalePos_TBATCHDTL[i].CGSTAMT.retDbl() + VE.TsalePos_TBATCHDTL[i].SGSTAMT.retDbl() == 0 && VE.T_TXN.REVCHRG != "N" && free == false)
                                {
                                    ContentFlg = "TAX amount not found Main Tab. Please add tax at slno " + VE.TsalePos_TBATCHDTL[i].SLNO;
                                    goto dbnotsave;
                                }
                                else if (VE.TsalePos_TBATCHDTL[i].CGSTAMT.retDbl() == 0 && VE.TsalePos_TBATCHDTL[i].SGSTAMT.retDbl() != 0 && VE.T_TXN.REVCHRG != "N" && free == false)
                                {
                                    ContentFlg = "Cgst amount not found Main Tab. Please add tax at slno " + VE.TsalePos_TBATCHDTL[i].SLNO;
                                    goto dbnotsave;
                                }
                                else if (VE.TsalePos_TBATCHDTL[i].CGSTAMT.retDbl() != 0 && VE.TsalePos_TBATCHDTL[i].SGSTAMT.retDbl() == 0 && VE.T_TXN.REVCHRG != "N" && free == false)
                                {
                                    ContentFlg = "Sgst amount not found Main Tab. Please add tax at slno " + VE.TsalePos_TBATCHDTL[i].SLNO;
                                    goto dbnotsave;
                                }
                                if (i == lastitemno) { _rpldist = _baldist; _rpldistq = _baldistq; }
                                else
                                {
                                    if (_amtdist + _amtdistq == 0) { _rpldist = 0; _rpldistq = 0; }
                                    else
                                    {
                                        _rpldist = ((_amtdist / titamt) * VE.TsalePos_TBATCHDTL[i].GROSSAMT).retDbl().toRound();
                                        _rpldistq = ((_amtdistq / titqty) * Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].QNTY)).toRound();
                                    }
                                }
                                _baldist = _baldist - _rpldist; _baldistq = _baldistq - _rpldistq;


                                #region T_TXNDTL
                                T_TXNDTL TTXNDTL = new T_TXNDTL();
                                TTXNDTL.CLCD = TTXN.CLCD;
                                TTXNDTL.EMD_NO = TTXN.EMD_NO;
                                TTXNDTL.DTAG = TTXN.DTAG;
                                TTXNDTL.AUTONO = TTXN.AUTONO;
                                TTXNDTL.SLNO = VE.TsalePos_TBATCHDTL[i].SLNO;
                                TTXNDTL.MTRLJOBCD = VE.TsalePos_TBATCHDTL[i].MTRLJOBCD;
                                TTXNDTL.ITCD = VE.TsalePos_TBATCHDTL[i].ITCD;
                                TTXNDTL.PARTCD = VE.TsalePos_TBATCHDTL[i].PARTCD;
                                TTXNDTL.COLRCD = VE.TsalePos_TBATCHDTL[i].COLRCD;
                                TTXNDTL.SIZECD = VE.TsalePos_TBATCHDTL[i].SIZECD;
                                TTXNDTL.STKDRCR = cr;
                                TTXNDTL.STKTYPE = VE.TsalePos_TBATCHDTL[i].STKTYPE;
                                TTXNDTL.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;
                                TTXNDTL.ITREM = VE.TsalePos_TBATCHDTL[i].ITREM;
                                TTXNDTL.BATCHNO = VE.TsalePos_TBATCHDTL[i].BATCHNO;
                                TTXNDTL.BALEYR = VE.BALEYR;// VE.TTXNDTL[i].BALEYR;
                                                           //TTXNDTL.BALENO = VE.TTXNDTL[i].BALENO;
                                TTXNDTL.GOCD = VE.T_TXN.GOCD;
                                TTXNDTL.NOS = VE.TsalePos_TBATCHDTL[i].NOS == null ? 0 : VE.TsalePos_TBATCHDTL[i].NOS;
                                TTXNDTL.QNTY = VE.TsalePos_TBATCHDTL[i].QNTY;
                                TTXNDTL.BLQNTY = VE.TsalePos_TBATCHDTL[i].BLQNTY;
                                TTXNDTL.RATE = VE.TsalePos_TBATCHDTL[i].RATE;
                                TTXNDTL.AMT = VE.TsalePos_TBATCHDTL[i].GROSSAMT;
                                TTXNDTL.FLAGMTR = VE.TsalePos_TBATCHDTL[i].FLAGMTR;
                                TTXNDTL.TOTDISCAMT = VE.TsalePos_TBATCHDTL[i].TOTDISCAMT;
                                TTXNDTL.TXBLVAL = VE.TsalePos_TBATCHDTL[i].TXBLVAL;
                                TTXNDTL.IGSTPER = VE.TsalePos_TBATCHDTL[i].IGSTPER;
                                TTXNDTL.CGSTPER = VE.TsalePos_TBATCHDTL[i].CGSTPER;
                                TTXNDTL.SGSTPER = VE.TsalePos_TBATCHDTL[i].SGSTPER;
                                TTXNDTL.CESSPER = VE.TsalePos_TBATCHDTL[i].CESSPER;
                                TTXNDTL.DUTYPER = VE.TsalePos_TBATCHDTL[i].DUTYPER;
                                TTXNDTL.IGSTAMT = VE.TsalePos_TBATCHDTL[i].IGSTAMT;
                                TTXNDTL.CGSTAMT = VE.TsalePos_TBATCHDTL[i].CGSTAMT;
                                TTXNDTL.SGSTAMT = VE.TsalePos_TBATCHDTL[i].SGSTAMT;
                                TTXNDTL.CESSAMT = VE.TsalePos_TBATCHDTL[i].CESSAMT;
                                TTXNDTL.DUTYAMT = VE.TsalePos_TBATCHDTL[i].DUTYAMT;
                                TTXNDTL.NETAMT = VE.TsalePos_TBATCHDTL[i].NETAMT;
                                TTXNDTL.OTHRAMT = _rpldist + _rpldistq;
                                TTXNDTL.DISCTYPE = VE.TsalePos_TBATCHDTL[i].DISCTYPE;
                                TTXNDTL.DISCRATE = VE.TsalePos_TBATCHDTL[i].DISCRATE;
                                TTXNDTL.DISCAMT = VE.TsalePos_TBATCHDTL[i].DISCAMT;
                                TTXNDTL.SCMDISCTYPE = VE.TsalePos_TBATCHDTL[i].SCMDISCTYPE;
                                TTXNDTL.SCMDISCRATE = VE.TsalePos_TBATCHDTL[i].SCMDISCRATE;
                                TTXNDTL.SCMDISCAMT = VE.TsalePos_TBATCHDTL[i].SCMDISCAMT;
                                TTXNDTL.PRCCD = VE.T_TXNOTH.PRCCD;
                                TTXNDTL.GLCD = VE.TsalePos_TBATCHDTL[i].GLCD;
                                dbsql = masterHelp.RetModeltoSql(TTXNDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                #endregion

                                #region T_BATCH
                                double mtrlcost = (((VE.TsalePos_TBATCHDTL[i].TXBLVAL + _amtdist) / VE.TsalePos_TBATCHDTL[i].QNTY) * VE.TsalePos_TBATCHDTL[i].QNTY).retDbl().toRound(2);
                                double batchamt = (((VE.TsalePos_TBATCHDTL[i].TXBLVAL) / VE.TsalePos_TBATCHDTL[i].QNTY) * VE.TsalePos_TBATCHDTL[i].QNTY).retDbl().toRound();

                                //bool flagbatch = false;
                                //string barno = salesfunc.GenerateBARNO(VE.TsalePos_TBATCHDTL[i].ITCD, VE.TsalePos_TBATCHDTL[i].CLRBARCODE, VE.TsalePos_TBATCHDTL[i].SZBARCODE);
                                //sql = "  select ITCD,BARNO from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHmst where barno='" + barno + "'";
                                //DataTable dt = masterHelp.SQLquery(sql);
                                //if (dt.Rows.Count == 0)
                                //{
                                //    ContentFlg = "Barno:" + barno + " not found at Item master at rowno:" + VE.TsalePos_TBATCHDTL[i].SLNO + " and itcd=" + VE.TsalePos_TBATCHDTL[i].ITCD; goto dbnotsave;
                                //}
                                //sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                //OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                //if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                //if (recoexist == false) flagbatch = true;

                                ////checking barno exist or not
                                //string Action = "", SqlCondition = "";
                                //if (VE.DefaultAction == "A")
                                //{
                                //    Action = VE.DefaultAction;
                                //}
                                //else
                                //{
                                //    sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where autono='" + TTXN.AUTONO + "' and slno = " + VE.TsalePos_TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
                                //    OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                //    if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                //    if (recoexist == true)
                                //    {
                                //        Action = "E";
                                //        SqlCondition = "autono = '" + TTXN.AUTONO + "' and slno = " + VE.TsalePos_TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
                                //        flagbatch = true;
                                //        barno = VE.TsalePos_TBATCHDTL[i].BARNO;
                                //    }
                                //    else
                                //    {
                                //        Action = "A";
                                //    }
                                //}
                                isNewBatch = false;
                                barno = "";
                                Action = "A";
                                SqlCondition = "";
                                if (string.IsNullOrEmpty(VE.TsalePos_TBATCHDTL[i].BARNO))
                                {
                                    barno = salesfunc.GenerateBARNO(VE.TsalePos_TBATCHDTL[i].ITCD, VE.TsalePos_TBATCHDTL[i].CLRBARCODE, VE.TsalePos_TBATCHDTL[i].SZBARCODE);
                                }
                                else
                                {
                                    barno = VE.TsalePos_TBATCHDTL[i].BARNO;
                                }
                                sql = "  select ITCD,BARNO from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHmst where barno='" + barno + "'";
                                var dt = masterHelp.SQLquery(sql);
                                if (dt.Rows.Count == 0)
                                {
                                    ContentFlg = "Barno:" + barno + " not found at Item master at rowno:" + VE.TsalePos_TBATCHDTL[i].SLNO + " and itcd=" + VE.TsalePos_TBATCHDTL[i].ITCD; goto dbnotsave;
                                }

                                sql = "Select BARNO from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                OraCmd.CommandText = sql;
                                using (var OraReco = OraCmd.ExecuteReader())
                                {
                                    if (OraReco.HasRows == false) { isNewBatch = true; Action = "A"; }
                                }


                                if (isNewBatch == true)
                                {
                                    T_BATCHMST TBATCHMST = new T_BATCHMST();
                                    T_BATCHMST_PRICE TBATCHMSTPRICE = new T_BATCHMST_PRICE();
                                    TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMST.CLCD = TTXN.CLCD;
                                    TBATCHMST.DTAG = TTXN.DTAG;
                                    TBATCHMST.TTAG = TTXN.TTAG;
                                    TBATCHMST.SLNO = VE.TsalePos_TBATCHDTL[i].SLNO; // ++COUNTERBATCH;
                                    TBATCHMST.AUTONO = TTXN.AUTONO;
                                    TBATCHMST.SLCD = TTXN.SLCD;
                                    TBATCHMST.MTRLJOBCD = VE.TsalePos_TBATCHDTL[i].MTRLJOBCD;
                                    //TBATCHMST.STKTYPE = VE.TsalePos_TBATCHDTL[i].STKTYPE;
                                    TBATCHMST.JOBCD = TTXN.JOBCD;
                                    TBATCHMST.BARNO = barno;
                                    TBATCHMST.ITCD = VE.TsalePos_TBATCHDTL[i].ITCD;
                                    TBATCHMST.PARTCD = VE.TsalePos_TBATCHDTL[i].PARTCD;
                                    TBATCHMST.SIZECD = VE.TsalePos_TBATCHDTL[i].SIZECD;
                                    TBATCHMST.COLRCD = VE.TsalePos_TBATCHDTL[i].COLRCD;
                                    TBATCHMST.NOS = VE.TsalePos_TBATCHDTL[i].NOS;
                                    TBATCHMST.QNTY = VE.TsalePos_TBATCHDTL[i].QNTY;
                                    TBATCHMST.RATE = VE.TsalePos_TBATCHDTL[i].RATE;
                                    TBATCHMST.AMT = batchamt;
                                    TBATCHMST.FLAGMTR = VE.TsalePos_TBATCHDTL[i].FLAGMTR;
                                    TBATCHMST.MTRL_COST = mtrlcost;
                                    //TBATCHMST.OTH_COST = VE.TBATCHDTL[i].OTH_COST;
                                    TBATCHMST.ITREM = VE.TsalePos_TBATCHDTL[i].ITREM;
                                    TBATCHMST.PDESIGN = VE.TsalePos_TBATCHDTL[i].PDESIGN;

                                    TBATCHMST.ORDAUTONO = VE.TsalePos_TBATCHDTL[i].ORDAUTONO;
                                    TBATCHMST.ORDSLNO = VE.TsalePos_TBATCHDTL[i].ORDSLNO;

                                    //TBATCHMST.ORGBATCHAUTONO = VE.TBATCHDTL[i].ORGBATCHAUTONO;
                                    //TBATCHMST.ORGBATCHSLNO = VE.TBATCHDTL[i].ORGBATCHSLNO;
                                    TBATCHMST.DIA = VE.TsalePos_TBATCHDTL[i].DIA;
                                    TBATCHMST.CUTLENGTH = VE.TsalePos_TBATCHDTL[i].CUTLENGTH;
                                    TBATCHMST.LOCABIN = VE.TsalePos_TBATCHDTL[i].LOCABIN;
                                    TBATCHMST.SHADE = VE.TsalePos_TBATCHDTL[i].SHADE;
                                    TBATCHMST.MILLNM = VE.TsalePos_TBATCHDTL[i].MILLNM;
                                    TBATCHMST.BATCHNO = VE.TsalePos_TBATCHDTL[i].BATCHNO;
                                    TBATCHMST.ORDAUTONO = VE.TsalePos_TBATCHDTL[i].ORDAUTONO;
                                    TBATCHMST.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;
                                    TBATCHMST.OURDESIGN = VE.TsalePos_TBATCHDTL[i].OURDESIGN;
                                    //save to T_BATCHMST_PRICE//
                                    TBATCHMSTPRICE.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMSTPRICE.CLCD = TTXN.CLCD;
                                    TBATCHMSTPRICE.DTAG = TTXN.DTAG;
                                    TBATCHMSTPRICE.TTAG = TTXN.TTAG;
                                    TBATCHMSTPRICE.AUTONO = TTXN.AUTONO;
                                    TBATCHMSTPRICE.BARNO = barno;
                                    TBATCHMSTPRICE.PRCCD = "RP";
                                    TBATCHMSTPRICE.RATE = VE.TsalePos_TBATCHDTL[i].RATE;
                                    TBATCHMSTPRICE.EFFDT = Convert.ToDateTime(TTXN.DOCDT);
                                    //end

                                    dbsql = masterHelp.RetModeltoSql(TBATCHMST, Action, "", SqlCondition);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    dbsql = masterHelp.RetModeltoSql(TBATCHMSTPRICE, Action, "", SqlCondition);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


                                }
                                T_BATCHDTL TsalePos_TBATCHDTL = new T_BATCHDTL();

                                TsalePos_TBATCHDTL.EMD_NO = TTXN.EMD_NO;
                                TsalePos_TBATCHDTL.CLCD = TTXN.CLCD;
                                TsalePos_TBATCHDTL.DTAG = TTXN.DTAG;
                                TsalePos_TBATCHDTL.TTAG = TTXN.TTAG;
                                TsalePos_TBATCHDTL.AUTONO = TTXN.AUTONO;
                                TsalePos_TBATCHDTL.SLNO = VE.TsalePos_TBATCHDTL[i].SLNO;  //COUNTER.retShort();
                                TsalePos_TBATCHDTL.TXNSLNO = VE.TsalePos_TBATCHDTL[i].SLNO;
                                TsalePos_TBATCHDTL.GOCD = VE.T_TXN.GOCD;
                                TsalePos_TBATCHDTL.BARNO = VE.TsalePos_TBATCHDTL[i].BARNO;
                                TsalePos_TBATCHDTL.MTRLJOBCD = VE.TsalePos_TBATCHDTL[i].MTRLJOBCD;
                                TsalePos_TBATCHDTL.PARTCD = VE.TsalePos_TBATCHDTL[i].PARTCD;
                                TsalePos_TBATCHDTL.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;
                                TsalePos_TBATCHDTL.STKDRCR = cr;
                                TsalePos_TBATCHDTL.NOS = VE.TsalePos_TBATCHDTL[i].NOS;
                                TsalePos_TBATCHDTL.QNTY = VE.TsalePos_TBATCHDTL[i].QNTY;
                                TsalePos_TBATCHDTL.BLQNTY = VE.TsalePos_TBATCHDTL[i].BLQNTY;
                                TsalePos_TBATCHDTL.FLAGMTR = VE.TsalePos_TBATCHDTL[i].FLAGMTR;
                                TsalePos_TBATCHDTL.ITREM = VE.TsalePos_TBATCHDTL[i].ITREM;
                                TsalePos_TBATCHDTL.RATE = VE.TsalePos_TBATCHDTL[i].RATE;
                                TsalePos_TBATCHDTL.DISCRATE = VE.TsalePos_TBATCHDTL[i].DISCRATE;
                                TsalePos_TBATCHDTL.DISCTYPE = VE.TsalePos_TBATCHDTL[i].DISCTYPE;
                                TsalePos_TBATCHDTL.SCMDISCRATE = VE.TsalePos_TBATCHDTL[i].SCMDISCRATE;
                                TsalePos_TBATCHDTL.SCMDISCTYPE = VE.TsalePos_TBATCHDTL[i].SCMDISCTYPE;
                                TsalePos_TBATCHDTL.TDDISCRATE = VE.TsalePos_TBATCHDTL[i].TDDISCRATE;
                                TsalePos_TBATCHDTL.TDDISCTYPE = VE.TsalePos_TBATCHDTL[i].TDDISCTYPE;
                                TsalePos_TBATCHDTL.ORDAUTONO = VE.TsalePos_TBATCHDTL[i].ORDAUTONO;
                                TsalePos_TBATCHDTL.ORDSLNO = VE.TsalePos_TBATCHDTL[i].ORDSLNO.retShort();
                                TsalePos_TBATCHDTL.DIA = VE.TsalePos_TBATCHDTL[i].DIA;
                                TsalePos_TBATCHDTL.CUTLENGTH = VE.TsalePos_TBATCHDTL[i].CUTLENGTH;
                                TsalePos_TBATCHDTL.LOCABIN = VE.TsalePos_TBATCHDTL[i].LOCABIN;
                                TsalePos_TBATCHDTL.SHADE = VE.TsalePos_TBATCHDTL[i].SHADE;
                                TsalePos_TBATCHDTL.MILLNM = VE.TsalePos_TBATCHDTL[i].MILLNM;
                                TsalePos_TBATCHDTL.BATCHNO = VE.TsalePos_TBATCHDTL[i].BATCHNO;
                                TsalePos_TBATCHDTL.BALEYR = VE.BALEYR;// VE.TsalePos_TBATCHDTL[i].BALEYR;
                                TsalePos_TBATCHDTL.TXNSLNO = VE.TsalePos_TBATCHDTL[i].SLNO;
                                TsalePos_TBATCHDTL.INCLRATE = VE.TsalePos_TBATCHDTL[i].INCLRATE.retDbl();
                                TsalePos_TBATCHDTL.PCSACTION = VE.TsalePos_TBATCHDTL[i].PCSACTION.retStr();
                                TsalePos_TBATCHDTL.STKTYPE = VE.TsalePos_TBATCHDTL[i].STKTYPE.retStr();
                                TsalePos_TBATCHDTL.OTHRAMT = _rpldist + _rpldistq;
                                TsalePos_TBATCHDTL.TXBLVAL = VE.TsalePos_TBATCHDTL[i].TXBLVAL;

                                dbsql = masterHelp.RetModeltoSql(TsalePos_TBATCHDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                #endregion

                                dbqty = dbqty + VE.TsalePos_TBATCHDTL[i].QNTY.retDbl();
                                igst = igst + VE.TsalePos_TBATCHDTL[i].IGSTAMT.retDbl();
                                cgst = cgst + VE.TsalePos_TBATCHDTL[i].CGSTAMT.retDbl();
                                sgst = sgst + VE.TsalePos_TBATCHDTL[i].SGSTAMT.retDbl();
                                cess = cess + VE.TsalePos_TBATCHDTL[i].CESSAMT.retDbl();
                                duty = duty + VE.TsalePos_TBATCHDTL[i].DUTYAMT.retDbl();
                            }
                        }
                    }
                    #region Return Tab
                    if (VE.TsalePos_TBATCHDTL_RETURN != null)
                    {
                        for (int i = 0; i <= VE.TsalePos_TBATCHDTL_RETURN.Count - 1; i++)
                        {
                            if (VE.TsalePos_TBATCHDTL_RETURN[i].SLNO != 0 && VE.TsalePos_TBATCHDTL_RETURN[i].ITCD != null && VE.TsalePos_TBATCHDTL_RETURN[i].QNTY != 0 && VE.TsalePos_TBATCHDTL_RETURN[i].MTRLJOBCD != null && VE.TsalePos_TBATCHDTL_RETURN[i].STKTYPE != null)
                            {
                                if (VE.TsalePos_TBATCHDTL_RETURN[i].IGSTAMT.retDbl() + VE.TsalePos_TBATCHDTL_RETURN[i].CGSTAMT.retDbl() + VE.TsalePos_TBATCHDTL_RETURN[i].SGSTAMT.retDbl() == 0 && VE.T_TXN.REVCHRG != "N")
                                {
                                    ContentFlg = "TAX amount not found Return Tab. Please add tax at slno " + VE.TsalePos_TBATCHDTL_RETURN[i].SLNO;
                                    goto dbnotsave;
                                }
                                else if (VE.TsalePos_TBATCHDTL_RETURN[i].CGSTAMT.retDbl() == 0 && VE.TsalePos_TBATCHDTL_RETURN[i].SGSTAMT.retDbl() != 0 && VE.T_TXN.REVCHRG != "N")
                                {
                                    ContentFlg = "Cgst amount not found Return Tab. Please add tax at slno " + VE.TsalePos_TBATCHDTL_RETURN[i].SLNO;
                                    goto dbnotsave;
                                }
                                else if (VE.TsalePos_TBATCHDTL_RETURN[i].CGSTAMT.retDbl() != 0 && VE.TsalePos_TBATCHDTL_RETURN[i].SGSTAMT.retDbl() == 0 && VE.T_TXN.REVCHRG != "N")
                                {
                                    ContentFlg = "Sgst amount not found Return Tab. Please add tax at slno " + VE.TsalePos_TBATCHDTL_RETURN[i].SLNO;
                                    goto dbnotsave;
                                }

                                if (i == rlastitemno) { _Rrpldist = _Rbaldist; _Rrpldistq = _Rbaldistq; }
                                else
                                {
                                    if (_amtdist + _amtdistq == 0) { _Rrpldist = 0; _Rrpldistq = 0; }
                                    else
                                    {
                                        _Rrpldist = ((_amtdist / rtitamt) * VE.TsalePos_TBATCHDTL_RETURN[i].GROSSAMT).retDbl().toRound();
                                        _Rrpldistq = ((_amtdistq / rtitqty) * Convert.ToDouble(VE.TsalePos_TBATCHDTL_RETURN[i].QNTY)).toRound();
                                    }
                                }
                                _Rbaldist = _Rbaldist - _Rrpldist; _Rbaldistq = _Rbaldistq - _Rrpldistq;

                                #region T_TXNDTL
                                T_TXNDTL R_TTXNDTL = new T_TXNDTL();
                                R_TTXNDTL.CLCD = TTXN.CLCD;
                                R_TTXNDTL.EMD_NO = TTXN.EMD_NO;
                                R_TTXNDTL.DTAG = TTXN.DTAG;
                                R_TTXNDTL.AUTONO = TTXN.AUTONO;
                                R_TTXNDTL.SLNO = VE.TsalePos_TBATCHDTL_RETURN[i].SLNO;
                                R_TTXNDTL.MTRLJOBCD = VE.TsalePos_TBATCHDTL_RETURN[i].MTRLJOBCD;
                                R_TTXNDTL.ITCD = VE.TsalePos_TBATCHDTL_RETURN[i].ITCD;
                                R_TTXNDTL.PARTCD = VE.TsalePos_TBATCHDTL_RETURN[i].PARTCD;
                                R_TTXNDTL.COLRCD = VE.TsalePos_TBATCHDTL_RETURN[i].COLRCD;
                                R_TTXNDTL.SIZECD = VE.TsalePos_TBATCHDTL_RETURN[i].SIZECD;
                                R_TTXNDTL.STKDRCR = VE.MENU_PARA == "SBCMR" ? cr : dr;
                                R_TTXNDTL.STKTYPE = VE.TsalePos_TBATCHDTL_RETURN[i].STKTYPE;
                                R_TTXNDTL.HSNCODE = VE.TsalePos_TBATCHDTL_RETURN[i].HSNCODE;
                                R_TTXNDTL.ITREM = VE.TsalePos_TBATCHDTL_RETURN[i].ITREM;
                                //TTXNDTL.PCSREM = VE.TsalePos_TBATCHDTL[i].PCSREM;
                                //TTXNDTL.FREESTK = VE.TsalePos_TBATCHDTL[i].FREESTK;
                                R_TTXNDTL.BATCHNO = VE.TsalePos_TBATCHDTL_RETURN[i].BATCHNO;
                                R_TTXNDTL.BALEYR = VE.BALEYR;// VE.TTXNDTL[i].BALEYR;
                                                             //TTXNDTL.BALENO = VE.TTXNDTL[i].BALENO;
                                R_TTXNDTL.GOCD = VE.T_TXN.GOCD;
                                //TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
                                R_TTXNDTL.NOS = VE.TsalePos_TBATCHDTL_RETURN[i].NOS == null ? 0 : VE.TsalePos_TBATCHDTL_RETURN[i].NOS.retDbl();
                                R_TTXNDTL.QNTY = VE.TsalePos_TBATCHDTL_RETURN[i].QNTY;
                                R_TTXNDTL.BLQNTY = VE.TsalePos_TBATCHDTL_RETURN[i].BLQNTY;
                                R_TTXNDTL.RATE = VE.TsalePos_TBATCHDTL_RETURN[i].RATE;
                                R_TTXNDTL.AMT = VE.TsalePos_TBATCHDTL_RETURN[i].GROSSAMT;
                                R_TTXNDTL.FLAGMTR = VE.TsalePos_TBATCHDTL_RETURN[i].FLAGMTR;
                                R_TTXNDTL.TOTDISCAMT = VE.TsalePos_TBATCHDTL_RETURN[i].TOTDISCAMT;
                                R_TTXNDTL.TXBLVAL = VE.TsalePos_TBATCHDTL_RETURN[i].TXBLVAL;
                                R_TTXNDTL.IGSTPER = VE.TsalePos_TBATCHDTL_RETURN[i].IGSTPER;
                                R_TTXNDTL.CGSTPER = VE.TsalePos_TBATCHDTL_RETURN[i].CGSTPER;
                                R_TTXNDTL.SGSTPER = VE.TsalePos_TBATCHDTL_RETURN[i].SGSTPER;
                                R_TTXNDTL.CESSPER = VE.TsalePos_TBATCHDTL_RETURN[i].CESSPER;
                                R_TTXNDTL.DUTYPER = VE.TsalePos_TBATCHDTL_RETURN[i].DUTYPER;
                                R_TTXNDTL.IGSTAMT = VE.TsalePos_TBATCHDTL_RETURN[i].IGSTAMT;
                                R_TTXNDTL.CGSTAMT = VE.TsalePos_TBATCHDTL_RETURN[i].CGSTAMT;
                                R_TTXNDTL.SGSTAMT = VE.TsalePos_TBATCHDTL_RETURN[i].SGSTAMT;
                                R_TTXNDTL.CESSAMT = VE.TsalePos_TBATCHDTL_RETURN[i].CESSAMT;
                                R_TTXNDTL.DUTYAMT = VE.TsalePos_TBATCHDTL_RETURN[i].DUTYAMT;
                                R_TTXNDTL.NETAMT = VE.TsalePos_TBATCHDTL_RETURN[i].NETAMT;
                                R_TTXNDTL.OTHRAMT = _Rrpldist + _Rrpldistq;
                                R_TTXNDTL.AGDOCNO = VE.TsalePos_TBATCHDTL_RETURN[i].AGDOCNO;
                                if (VE.TsalePos_TBATCHDTL_RETURN[i].AGDOCDT != null) R_TTXNDTL.AGDOCDT = Convert.ToDateTime(VE.TsalePos_TBATCHDTL_RETURN[i].AGDOCDT);
                                //TTXNDTL.SHORTQNTY = VE.TsalePos_TBATCHDTL[i].SHORTQNTY;
                                R_TTXNDTL.DISCTYPE = VE.TsalePos_TBATCHDTL_RETURN[i].DISCTYPE;
                                R_TTXNDTL.DISCRATE = VE.TsalePos_TBATCHDTL_RETURN[i].DISCRATE;
                                R_TTXNDTL.DISCAMT = VE.TsalePos_TBATCHDTL_RETURN[i].DISCAMT;
                                //TTXNDTL.SCMDISCTYPE = VE.TsalePos_TBATCHDTL[i].SCMDISCTYPE;
                                //TTXNDTL.SCMDISCRATE = VE.TsalePos_TBATCHDTL[i].SCMDISCRATE;
                                //TTXNDTL.SCMDISCAMT = VE.TsalePos_TBATCHDTL[i].SCMDISCAMT;
                                //TTXNDTL.TDDISCTYPE = VE.TsalePos_TBATCHDTL[i].TDDISCTYPE;
                                //TTXNDTL.TDDISCRATE = VE.TsalePos_TBATCHDTL[i].TDDISCRATE;
                                //TTXNDTL.TDDISCAMT = VE.TsalePos_TBATCHDTL[i].TDDISCAMT;
                                R_TTXNDTL.PRCCD = VE.T_TXNOTH.PRCCD;
                                //TTXNDTL.PRCEFFDT = VE.TsalePos_TBATCHDTL[i].PRCEFFDT;
                                R_TTXNDTL.GLCD = VE.TsalePos_TBATCHDTL_RETURN[i].GLCD;
                                dbsql = masterHelp.RetModeltoSql(R_TTXNDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                #endregion

                                #region T_BATCH
                                double mtrlcost = (((VE.TsalePos_TBATCHDTL_RETURN[i].TXBLVAL + _amtdist) / VE.TsalePos_TBATCHDTL_RETURN[i].QNTY) * VE.TsalePos_TBATCHDTL_RETURN[i].QNTY).retDbl().toRound(2);
                                double batchamt = (((VE.TsalePos_TBATCHDTL_RETURN[i].TXBLVAL) / VE.TsalePos_TBATCHDTL_RETURN[i].QNTY) * VE.TsalePos_TBATCHDTL_RETURN[i].QNTY).retDbl().toRound();


                                //bool flagbatch = false;
                                //string barno = salesfunc.GenerateBARNO(VE.TsalePos_TBATCHDTL_RETURN[i].ITCD, VE.TsalePos_TBATCHDTL_RETURN[i].CLRBARCODE, VE.TsalePos_TBATCHDTL_RETURN[i].SZBARCODE);
                                //sql = "  select ITCD,BARNO from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHmst where barno='" + barno + "'";
                                //DataTable dt = masterHelp.SQLquery(sql);
                                //if (dt.Rows.Count == 0)
                                //{
                                //    ContentFlg = "Barno:" + barno + " not found at Item master at rowno:" + VE.TsalePos_TBATCHDTL_RETURN[i].SLNO + " and itcd=" + VE.TsalePos_TBATCHDTL_RETURN[i].ITCD; goto dbnotsave;
                                //}
                                //sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                //OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                //if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                //if (recoexist == false) flagbatch = true;

                                ////checking barno exist or not
                                //string Action = "", SqlCondition = "";
                                //if (VE.DefaultAction == "A")
                                //{
                                //    Action = VE.DefaultAction;
                                //}
                                //else
                                //{
                                //    sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where autono='" + TTXN.AUTONO + "' and slno = " + VE.TsalePos_TBATCHDTL_RETURN[i].SLNO + " and barno='" + barno + "' ";
                                //    OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                //    if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                //    if (recoexist == true)
                                //    {
                                //        Action = "E";
                                //        SqlCondition = "autono = '" + TTXN.AUTONO + "' and slno = " + VE.TsalePos_TBATCHDTL_RETURN[i].SLNO + " and barno='" + barno + "' ";
                                //        flagbatch = true;
                                //        barno = VE.TsalePos_TBATCHDTL_RETURN[i].BARNO;
                                //    }
                                //    else
                                //    {
                                //        Action = "A";
                                //    }
                                //}
                                isNewBatch = false;
                                barno = "";
                                Action = "A";
                                SqlCondition = "";
                                if (string.IsNullOrEmpty(VE.TsalePos_TBATCHDTL_RETURN[i].BARNO))
                                {
                                    barno = salesfunc.GenerateBARNO(VE.TsalePos_TBATCHDTL_RETURN[i].ITCD, VE.TsalePos_TBATCHDTL_RETURN[i].CLRBARCODE, VE.TsalePos_TBATCHDTL_RETURN[i].SZBARCODE);
                                }
                                else
                                {
                                    barno = VE.TsalePos_TBATCHDTL_RETURN[i].BARNO;
                                }
                                sql = "  select ITCD,BARNO from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHmst where barno='" + barno + "'";
                                var dt = masterHelp.SQLquery(sql);
                                if (dt.Rows.Count == 0)
                                {
                                    ContentFlg = "Barno:" + barno + " not found at Item master at rowno:" + VE.TsalePos_TBATCHDTL_RETURN[i].SLNO + " and itcd=" + VE.TsalePos_TBATCHDTL_RETURN[i].ITCD; goto dbnotsave;
                                }

                                sql = "Select BARNO from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                OraCmd.CommandText = sql;
                                using (var OraReco = OraCmd.ExecuteReader())
                                {
                                    if (OraReco.HasRows == false) { isNewBatch = true; Action = "A"; }
                                }
                                if (isNewBatch == true)
                                {
                                    T_BATCHMST TBATCHMST = new T_BATCHMST();
                                    T_BATCHMST_PRICE TBATCHMSTPRICE = new T_BATCHMST_PRICE();
                                    TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMST.CLCD = TTXN.CLCD;
                                    TBATCHMST.DTAG = TTXN.DTAG;
                                    TBATCHMST.TTAG = TTXN.TTAG;
                                    TBATCHMST.SLNO = VE.TsalePos_TBATCHDTL_RETURN[i].SLNO; // ++COUNTERBATCH;
                                    TBATCHMST.AUTONO = TTXN.AUTONO;
                                    TBATCHMST.SLCD = TTXN.SLCD;
                                    TBATCHMST.MTRLJOBCD = VE.TsalePos_TBATCHDTL_RETURN[i].MTRLJOBCD;
                                    //TBATCHMST.STKTYPE = VE.TsalePos_TBATCHDTL_RETURN[i].STKTYPE;
                                    TBATCHMST.JOBCD = TTXN.JOBCD;
                                    TBATCHMST.BARNO = barno;
                                    TBATCHMST.ITCD = VE.TsalePos_TBATCHDTL_RETURN[i].ITCD;
                                    TBATCHMST.PARTCD = VE.TsalePos_TBATCHDTL_RETURN[i].PARTCD;
                                    TBATCHMST.SIZECD = VE.TsalePos_TBATCHDTL_RETURN[i].SIZECD;
                                    TBATCHMST.COLRCD = VE.TsalePos_TBATCHDTL_RETURN[i].COLRCD;
                                    TBATCHMST.NOS = VE.TsalePos_TBATCHDTL_RETURN[i].NOS;
                                    TBATCHMST.QNTY = VE.TsalePos_TBATCHDTL_RETURN[i].QNTY;
                                    TBATCHMST.RATE = VE.TsalePos_TBATCHDTL_RETURN[i].RATE;
                                    TBATCHMST.AMT = batchamt;
                                    TBATCHMST.FLAGMTR = VE.TsalePos_TBATCHDTL_RETURN[i].FLAGMTR;
                                    TBATCHMST.MTRL_COST = mtrlcost;
                                    //TBATCHMST.OTH_COST = VE.TBATCHDTL[i].OTH_COST;
                                    TBATCHMST.ITREM = VE.TsalePos_TBATCHDTL_RETURN[i].ITREM;
                                    TBATCHMST.PDESIGN = VE.TsalePos_TBATCHDTL_RETURN[i].PDESIGN;

                                    //TBATCHMST.ORDAUTONO = VE.TsalePos_TBATCHDTL_RETURN[i].ORDAUTONO;
                                    //TBATCHMST.ORDSLNO = VE.TsalePos_TBATCHDTL_RETURN[i].ORDSLNO;

                                    //TBATCHMST.ORGBATCHAUTONO = VE.TBATCHDTL[i].ORGBATCHAUTONO;
                                    //TBATCHMST.ORGBATCHSLNO = VE.TBATCHDTL[i].ORGBATCHSLNO;
                                    TBATCHMST.DIA = VE.TsalePos_TBATCHDTL_RETURN[i].DIA;
                                    TBATCHMST.CUTLENGTH = VE.TsalePos_TBATCHDTL_RETURN[i].CUTLENGTH;
                                    TBATCHMST.LOCABIN = VE.TsalePos_TBATCHDTL_RETURN[i].LOCABIN;
                                    TBATCHMST.SHADE = VE.TsalePos_TBATCHDTL_RETURN[i].SHADE;
                                    TBATCHMST.MILLNM = VE.TsalePos_TBATCHDTL_RETURN[i].MILLNM;
                                    TBATCHMST.BATCHNO = VE.TsalePos_TBATCHDTL_RETURN[i].BATCHNO;
                                    TBATCHMST.ORDAUTONO = VE.TsalePos_TBATCHDTL_RETURN[i].ORDAUTONO;

                                    TBATCHMST.HSNCODE = VE.TsalePos_TBATCHDTL_RETURN[i].HSNCODE;
                                    TBATCHMST.OURDESIGN = VE.TsalePos_TBATCHDTL_RETURN[i].OURDESIGN;
                                    //save to T_BATCHMST_PRICE//
                                    TBATCHMSTPRICE.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMSTPRICE.CLCD = TTXN.CLCD;
                                    TBATCHMSTPRICE.DTAG = TTXN.DTAG;
                                    TBATCHMSTPRICE.TTAG = TTXN.TTAG;
                                    TBATCHMSTPRICE.AUTONO = TTXN.AUTONO;
                                    TBATCHMSTPRICE.BARNO = barno;
                                    TBATCHMSTPRICE.PRCCD = "RP";
                                    TBATCHMSTPRICE.RATE = VE.TsalePos_TBATCHDTL_RETURN[i].RATE;
                                    TBATCHMSTPRICE.EFFDT = Convert.ToDateTime(TTXN.DOCDT);
                                    //end

                                    dbsql = masterHelp.RetModeltoSql(TBATCHMST, Action, "", SqlCondition);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    dbsql = masterHelp.RetModeltoSql(TBATCHMSTPRICE, Action, "", SqlCondition);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                }
                                T_BATCHDTL TsalePos_TBATCHDTL_RETURN = new T_BATCHDTL();

                                TsalePos_TBATCHDTL_RETURN.EMD_NO = TTXN.EMD_NO;
                                TsalePos_TBATCHDTL_RETURN.CLCD = TTXN.CLCD;
                                TsalePos_TBATCHDTL_RETURN.DTAG = TTXN.DTAG;
                                TsalePos_TBATCHDTL_RETURN.TTAG = TTXN.TTAG;
                                TsalePos_TBATCHDTL_RETURN.AUTONO = TTXN.AUTONO;
                                TsalePos_TBATCHDTL_RETURN.SLNO = VE.TsalePos_TBATCHDTL_RETURN[i].SLNO;  //COUNTER.retShort();
                                TsalePos_TBATCHDTL_RETURN.TXNSLNO = VE.TsalePos_TBATCHDTL_RETURN[i].SLNO;
                                TsalePos_TBATCHDTL_RETURN.GOCD = VE.T_TXN.GOCD;
                                TsalePos_TBATCHDTL_RETURN.BARNO = VE.TsalePos_TBATCHDTL_RETURN[i].BARNO;
                                TsalePos_TBATCHDTL_RETURN.MTRLJOBCD = VE.TsalePos_TBATCHDTL_RETURN[i].MTRLJOBCD;
                                TsalePos_TBATCHDTL_RETURN.PARTCD = VE.TsalePos_TBATCHDTL_RETURN[i].PARTCD;
                                TsalePos_TBATCHDTL_RETURN.HSNCODE = VE.TsalePos_TBATCHDTL_RETURN[i].HSNCODE;
                                TsalePos_TBATCHDTL_RETURN.STKDRCR = VE.MENU_PARA == "SBCMR" ? cr : dr;
                                TsalePos_TBATCHDTL_RETURN.NOS = VE.TsalePos_TBATCHDTL_RETURN[i].NOS;
                                TsalePos_TBATCHDTL_RETURN.QNTY = VE.TsalePos_TBATCHDTL_RETURN[i].QNTY;
                                TsalePos_TBATCHDTL_RETURN.BLQNTY = VE.TsalePos_TBATCHDTL_RETURN[i].BLQNTY;
                                TsalePos_TBATCHDTL_RETURN.FLAGMTR = VE.TsalePos_TBATCHDTL_RETURN[i].FLAGMTR;
                                TsalePos_TBATCHDTL_RETURN.ITREM = VE.TsalePos_TBATCHDTL_RETURN[i].ITREM;
                                TsalePos_TBATCHDTL_RETURN.RATE = VE.TsalePos_TBATCHDTL_RETURN[i].RATE;
                                TsalePos_TBATCHDTL_RETURN.DISCRATE = VE.TsalePos_TBATCHDTL_RETURN[i].DISCRATE;
                                TsalePos_TBATCHDTL_RETURN.DISCTYPE = VE.TsalePos_TBATCHDTL_RETURN[i].DISCTYPE;
                                TsalePos_TBATCHDTL_RETURN.SCMDISCRATE = VE.TsalePos_TBATCHDTL_RETURN[i].SCMDISCRATE;
                                TsalePos_TBATCHDTL_RETURN.SCMDISCTYPE = VE.TsalePos_TBATCHDTL_RETURN[i].SCMDISCTYPE;
                                TsalePos_TBATCHDTL_RETURN.TDDISCRATE = VE.TsalePos_TBATCHDTL_RETURN[i].TDDISCRATE;
                                TsalePos_TBATCHDTL_RETURN.TDDISCTYPE = VE.TsalePos_TBATCHDTL_RETURN[i].TDDISCTYPE;
                                TsalePos_TBATCHDTL_RETURN.ORDAUTONO = VE.TsalePos_TBATCHDTL_RETURN[i].ORDAUTONO;
                                TsalePos_TBATCHDTL_RETURN.ORDSLNO = VE.TsalePos_TBATCHDTL_RETURN[i].ORDSLNO.retShort();
                                TsalePos_TBATCHDTL_RETURN.DIA = VE.TsalePos_TBATCHDTL_RETURN[i].DIA;
                                TsalePos_TBATCHDTL_RETURN.CUTLENGTH = VE.TsalePos_TBATCHDTL_RETURN[i].CUTLENGTH;
                                TsalePos_TBATCHDTL_RETURN.LOCABIN = VE.TsalePos_TBATCHDTL_RETURN[i].LOCABIN;
                                TsalePos_TBATCHDTL_RETURN.SHADE = VE.TsalePos_TBATCHDTL_RETURN[i].SHADE;
                                TsalePos_TBATCHDTL_RETURN.MILLNM = VE.TsalePos_TBATCHDTL_RETURN[i].MILLNM;
                                TsalePos_TBATCHDTL_RETURN.BATCHNO = VE.TsalePos_TBATCHDTL_RETURN[i].BATCHNO;
                                TsalePos_TBATCHDTL_RETURN.BALEYR = VE.BALEYR;// VE.TsalePos_TBATCHDTL[i].BALEYR;
                                TsalePos_TBATCHDTL_RETURN.TXNSLNO = VE.TsalePos_TBATCHDTL_RETURN[i].SLNO;
                                TsalePos_TBATCHDTL_RETURN.INCLRATE = VE.TsalePos_TBATCHDTL_RETURN[i].INCLRATE.retDbl();
                                TsalePos_TBATCHDTL_RETURN.PCSACTION = VE.TsalePos_TBATCHDTL_RETURN[i].PCSACTION.retStr();
                                TsalePos_TBATCHDTL_RETURN.STKTYPE = VE.TsalePos_TBATCHDTL_RETURN[i].STKTYPE.retStr();
                                TsalePos_TBATCHDTL_RETURN.OTHRAMT = _Rrpldist + _Rrpldistq;
                                TsalePos_TBATCHDTL_RETURN.TXBLVAL = VE.TsalePos_TBATCHDTL_RETURN[i].TXBLVAL;

                                dbsql = masterHelp.RetModeltoSql(TsalePos_TBATCHDTL_RETURN);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                #endregion

                                //dbqty = dbqty - VE.TsalePos_TBATCHDTL_RETURN[i].QNTY.retDbl();
                                rdbqty = rdbqty + VE.TsalePos_TBATCHDTL_RETURN[i].QNTY.retDbl();
                                if (VE.MENU_PARA == "SBCMR")
                                {
                                    igst = igst + VE.TsalePos_TBATCHDTL_RETURN[i].IGSTAMT.retDbl();
                                    cgst = cgst + VE.TsalePos_TBATCHDTL_RETURN[i].CGSTAMT.retDbl();
                                    sgst = sgst + VE.TsalePos_TBATCHDTL_RETURN[i].SGSTAMT.retDbl();
                                    cess = cess + VE.TsalePos_TBATCHDTL_RETURN[i].CESSAMT.retDbl();
                                    duty = duty + VE.TsalePos_TBATCHDTL_RETURN[i].DUTYAMT.retDbl();
                                }
                                else
                                {
                                    igst = igst - VE.TsalePos_TBATCHDTL_RETURN[i].IGSTAMT.retDbl();
                                    cgst = cgst - VE.TsalePos_TBATCHDTL_RETURN[i].CGSTAMT.retDbl();
                                    sgst = sgst - VE.TsalePos_TBATCHDTL_RETURN[i].SGSTAMT.retDbl();
                                    cess = cess - VE.TsalePos_TBATCHDTL_RETURN[i].CESSAMT.retDbl();
                                    duty = duty - VE.TsalePos_TBATCHDTL_RETURN[i].DUTYAMT.retDbl();
                                }

                            }
                        }
                    }
                    #endregion

                    //if (dbqty == 0)
                    //{
                    //    ContentFlg = "Quantity not entered"; goto dbnotsave;
                    //}
                    if (dbqty < 0 && rdbqty < 0)
                    {
                        ContentFlg = "Quantity not entered"; goto dbnotsave;
                    }
                    dbqty = dbqty - rdbqty;
                    isl = 1;

                    if (VE.TTXNAMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                        {
                            if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                            {
                                if (VE.TTXNAMT[i].IGSTAMT.retDbl() + VE.TTXNAMT[i].CGSTAMT.retDbl() + VE.TTXNAMT[i].SGSTAMT.retDbl() == 0 && VE.T_TXN.REVCHRG != "N")
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
                                            Convert.ToDouble(VE.TTXNAMT[i].AMT), strrem, parglcd, TTXN.SLCD, 0, 0, Convert.ToDouble(VE.TTXNAMT[i].CURR_AMT));
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    if (adrcr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);
                                    else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);

                                    if (VE.TsalePos_TBATCHDTL != null)
                                    {
                                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                                VE.TsalePos_TBATCHDTL[0].CLASS1CD, "", Convert.ToDouble(VE.TTXNAMT[i].AMT), 0, strrem);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    }
                                    #region Return tab
                                    if (VE.TsalePos_TBATCHDTL_RETURN != null)
                                    {
                                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1001, Convert.ToSByte(isl), sslcd,
                                                VE.TsalePos_TBATCHDTL_RETURN[0].CLASS1CD, "", Convert.ToDouble(VE.TTXNAMT[i].AMT), 0, strrem);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    }
                                    #endregion
                                }
                                igst = igst + Convert.ToDouble(VE.TTXNAMT[i].IGSTAMT);
                                cgst = cgst + Convert.ToDouble(VE.TTXNAMT[i].CGSTAMT);
                                sgst = sgst + Convert.ToDouble(VE.TTXNAMT[i].SGSTAMT);
                                cess = cess + Convert.ToDouble(VE.TTXNAMT[i].CESSAMT);
                                duty = duty + Convert.ToDouble(VE.TTXNAMT[i].DUTYAMT);
                            }
                        }
                    }
                    if (VE.TTXNPYMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                        {
                            if (VE.TTXNPYMT[i].SLNO != 0 && VE.TTXNPYMT[i].AMT.retDbl() != 0)
                            {
                                T_TXNPYMT TTXNPYMNT = new T_TXNPYMT();
                                TTXNPYMNT.AUTONO = TTXN.AUTONO;
                                TTXNPYMNT.SLNO = VE.TTXNPYMT[i].SLNO;
                                TTXNPYMNT.EMD_NO = TTXN.EMD_NO;
                                TTXNPYMNT.CLCD = TTXN.CLCD;
                                TTXNPYMNT.DTAG = TTXN.DTAG;
                                TTXNPYMNT.PYMTCD = VE.TTXNPYMT[i].PYMTCD;
                                TTXNPYMNT.AMT = VE.TTXNPYMT[i].AMT.retDbl();
                                TTXNPYMNT.CARDNO = VE.TTXNPYMT[i].CARDNO;
                                TTXNPYMNT.INSTNO = VE.TTXNPYMT[i].INSTNO;
                                if (VE.TTXNPYMT[i].INSTDT.retStr() != "")
                                {
                                    TTXNPYMNT.INSTDT = Convert.ToDateTime(VE.TTXNPYMT[i].INSTDT);
                                }
                                TTXNPYMNT.PYMTREM = VE.TTXNPYMT[i].PYMTREM;
                                TTXNPYMNT.GLCD = VE.TTXNPYMT[i].GLCD;
                                dbsql = masterHelp.RetModeltoSql(TTXNPYMNT);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    if (VE.MENU_PARA != "CADV")
                    {
                        if (VE.AdvanceList != null)
                        {
                            for (int i = 0; i <= VE.AdvanceList.Count - 1; i++)
                            {
                                if (VE.AdvanceList[i].SLNO != 0 && VE.AdvanceList[i].ADVAUTONO.retStr() != "")
                                {
                                    //-------------------------Party Order Advance--------------------------//
                                    T_TXN_LINKNO TTXNLINKNO = new T_TXN_LINKNO();
                                    TTXNLINKNO.AUTONO = TTXN.AUTONO;
                                    TTXNLINKNO.EMD_NO = TTXN.EMD_NO;
                                    TTXNLINKNO.CLCD = TTXN.CLCD;
                                    TTXNLINKNO.DTAG = TTXN.DTAG;
                                    TTXNLINKNO.LINKAUTONO = VE.AdvanceList[i].ADVAUTONO;
                                    TTXNLINKNO.AMT = VE.AdvanceList[i].ADJAMT.retDbl();
                                    dbsql = masterHelp.RetModeltoSql(TTXNLINKNO);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }

                    }
                    if (VE.TTXNSLSMN != null)
                    {
                        for (int i = 0; i <= VE.TTXNSLSMN.Count - 1; i++)
                        {
                            if (VE.TTXNSLSMN[i].SLNO != 0 && VE.TTXNSLSMN[i].SLMSLCD != null)
                            {
                                T_TXNSLSMN TTXNSLSMN = new T_TXNSLSMN();
                                TTXNSLSMN.AUTONO = TTXN.AUTONO;
                                TTXNSLSMN.SLNO = Convert.ToByte(VE.TTXNSLSMN[i].SLNO);
                                TTXNSLSMN.EMD_NO = TTXN.EMD_NO;
                                TTXNSLSMN.CLCD = TTXN.CLCD;
                                TTXNSLSMN.DTAG = TTXN.DTAG;
                                TTXNSLSMN.SLMSLCD = VE.TTXNSLSMN[i].SLMSLCD;
                                TTXNSLSMN.PER = VE.TTXNSLSMN[i].PER.retDbl();
                                TTXNSLSMN.ITAMT = VE.TTXNSLSMN[i].ITAMT.retDbl();
                                TTXNSLSMN.BLAMT = VE.TTXNSLSMN[i].BLAMT.retDbl();
                                dbsql = masterHelp.RetModeltoSql(TTXNSLSMN);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    #region Document Passing checking
                    // -----------------------DOCUMENT PASSING DATA---------------------------//
                    double TRAN_AMT = Convert.ToDouble(TTXN.BLAMT);
                    if (docpassrem != "")
                    {
                        docpassrem = "Doc # " + TTXN.DOCNO.ToString() + " Party Name # " + VE.SLNM + " [" + VE.SLAREA + "]".ToString() + "~" + docpassrem;
                        var TCDP_DATA1 = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, 0, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO), docpassrem);
                        if (TCDP_DATA1.Item1.Count != 0)
                        {
                            DB.T_CNTRL_DOC_PASS.AddRange(TCDP_DATA1.Item1);
                        }
                    }
                    else
                    {
                        var TCDP_DATA = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, TRAN_AMT, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO));
                        if (TCDP_DATA.Item1.Count != 0)
                        {
                            DB.T_CNTRL_DOC_PASS.AddRange(TCDP_DATA.Item1);
                        }
                    }
                    if (docpassrem != "") DB.T_CNTRL_DOC_PASS.Add(TCDP);
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
                    if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
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
                        salpur = "S";
                        string prodrem = ""; expglcd = "";

                        List<TsalePos_TBATCHDTL> MAINAMTGLCD = new List<Models.TsalePos_TBATCHDTL>();
                        List<TsalePos_TBATCHDTL> RAMTGLCD = new List<Models.TsalePos_TBATCHDTL>();
                        if (VE.TsalePos_TBATCHDTL != null)
                        {
                            MAINAMTGLCD = (from x in VE.TsalePos_TBATCHDTL
                                           group x by new { x.GLCD, x.CLASS1CD, x.UOM } into P
                                           select new TsalePos_TBATCHDTL()
                                           {
                                               GLCD = P.Key.GLCD,
                                               CLASS1CD = P.Key.CLASS1CD,
                                               UOM = P.Key.UOM,
                                               QNTY = P.Sum(A => A.QNTY),
                                               TXBLVAL = P.Sum(A => A.TXBLVAL)
                                           }).Where(a => a.QNTY != 0).ToList();
                        }
                        if (VE.TsalePos_TBATCHDTL_RETURN != null)
                        {
                            RAMTGLCD = (from x in VE.TsalePos_TBATCHDTL_RETURN
                                        group x by new { x.GLCD, x.CLASS1CD, x.UOM } into P
                                        select new TsalePos_TBATCHDTL()
                                        {
                                            GLCD = P.Key.GLCD,
                                            CLASS1CD = P.Key.CLASS1CD,
                                            UOM = P.Key.UOM,
                                            QNTY = P.Sum(A => A.QNTY),
                                            TXBLVAL = (P.Sum(A => A.TXBLVAL)) * (VE.MENU_PARA == "SBCM" ? -1 : 1),
                                        }).Where(a => a.QNTY != 0).ToList();
                        }
                        MAINAMTGLCD.AddRange(RAMTGLCD);

                        var UOMWISEQNTY = (from x in MAINAMTGLCD
                                           group x by new { x.UOM } into P
                                           select new
                                           {
                                               UOM = P.Key.UOM,
                                               QNTY = P.Sum(A => A.QNTY),
                                           }).Where(a => a.QNTY != 0).ToList();
                        foreach (var v in UOMWISEQNTY)
                        {
                            if (prodrem != "") prodrem += ",";
                            prodrem += (VE.MENU_PARA == "SBCM" ? "Cash Memo" : "CM Return Note") + " Q=" + v.QNTY + " " + v.UOM;
                        }

                        var AMTGLCD = (from x in MAINAMTGLCD
                                       group x by new { x.GLCD, x.CLASS1CD } into P
                                       select new
                                       {
                                           GLCD = P.Key.GLCD,
                                           CLASS1CD = P.Key.CLASS1CD,
                                           QNTY = P.Sum(A => A.QNTY),
                                           TXBLVAL = P.Sum(A => A.TXBLVAL)
                                       }).Where(a => a.QNTY != 0).ToList();
                        string negamt = "", proddrcr = "";
                        if (AMTGLCD != null && AMTGLCD.Count > 0)
                        {
                            for (int i = 0; i <= AMTGLCD.Count - 1; i++)
                            {
                                isl = isl + 1;
                                negamt = AMTGLCD[i].TXBLVAL.retDbl() < 0 ? "Y" : "N";
                                proddrcr = negamt == "Y" ? dr : cr;
                                //if (negamt == "Y" && VE.MENU_PARA == "SBCMR") proddrcr = cr;

                                dbamt = AMTGLCD[i].TXBLVAL.retDbl() * (negamt == "Y" ? -1 : 1);
                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), proddrcr, AMTGLCD[i].GLCD, sslcd,
                                        dbamt, prodrem, parglcd, TTXN.SLCD, AMTGLCD[i].QNTY.retDbl(), 0, 0);
                                OraCmd.CommandText = dbsql;
                                OraCmd.ExecuteNonQuery();

                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                        AMTGLCD[i].CLASS1CD, "", dbamt, 0, strrem);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                itamt = itamt + dbamt.retDbl();
                                expglcd = AMTGLCD[i].GLCD;

                                if (proddrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                                else dbCrAmt = dbCrAmt + dbamt;
                            }
                        }
                        if (itamt < 0) itamt = itamt * -1;

                        #region  GST Financial Part
                        string[] gstpostcd = new string[5];
                        double[] gstpostamt = new double[5];
                        for (int gt = 0; gt < 5; gt++)
                        {
                            gstpostcd[gt] = ""; gstpostamt[gt] = 0;
                        }
                        //gstpostamt[0] = igst; gstpostamt[1] = cgst; gstpostamt[2] = sgst; gstpostamt[3] = cess; gstpostamt[4] = duty;
                        int gi = 0;
                        if (revcharge != "Y")
                        {
                            if (tbl != null && tbl.Rows.Count > 0)
                            {
                                gstpostcd[gi] = tbl.Rows[0]["igstcd"].ToString(); gstpostamt[gi] = igst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["cgstcd"].ToString(); gstpostamt[gi] = cgst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["sgstcd"].ToString(); gstpostamt[gi] = sgst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["cesscd"].ToString(); gstpostamt[gi] = cess; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                            }
                        }
                        else
                        {
                            if (tbl != null && tbl.Rows.Count > 0)
                            {
                                gstpostcd[gi] = tbl.Rows[0]["igst_rvi"].ToString(); gstpostamt[gi] = igst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["cgst_rvi"].ToString(); gstpostamt[gi] = cgst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["sgst_rvi"].ToString(); gstpostamt[gi] = sgst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["cess_rvi"].ToString(); gstpostamt[gi] = cess; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                            }
                        }

                        for (int gt = 0; gt < 5; gt++)
                        {
                            if (gstpostamt[gt] != 0)
                            {
                                isl = isl + 1;
                                negamt = gstpostamt[gt] < 0 ? "Y" : "N";
                                proddrcr = negamt == "Y" ? dr : cr;
                                //if (negamt == "Y" && VE.MENU_PARA == "SBCMR") proddrcr = cr;
                                dbamt = gstpostamt[gt].retDbl() * (negamt == "Y" ? -1 : 1);

                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), proddrcr, gstpostcd[gt], sslcd,
                                   dbamt, prodrem, parglcd, TTXN.SLCD, dbqty, 0, 0);
                                OraCmd.CommandText = dbsql;
                                OraCmd.ExecuteNonQuery();

                                if (proddrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                                else dbCrAmt = dbCrAmt + dbamt;

                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                        parclass1cd, class2cd, dbamt, 0, strrem);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            }
                        }
                        #endregion
                        //  TCS
                        dbamt = Convert.ToDouble(VE.T_TXN.TCSAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;

                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tcsgl, sslcd,
                                    dbamt, prodrem, parglcd, TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                            dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                    null, null, Convert.ToDouble(VE.T_TXN.TCSAMT), 0, strrem + " TCS @ " + VE.T_TXN.TCSPER.ToString() + "%");
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        }
                        //Ronded off
                        dbamt = Convert.ToDouble(VE.T_TXN.ROAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;
                            if (dbamt < 0) adrcr = dr;
                            if (dbamt < 0) dbamt = dbamt * -1;

                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, rogl, null,
                                    dbamt, prodrem, parglcd, TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                        }

                        //  Party wise posting
                        isl = 1;
                        negamt = VE.T_TXN.BLAMT.retDbl() < 0 ? "Y" : "N";
                        proddrcr = negamt == "Y" ? cr : dr;
                        //if (negamt == "Y" && VE.MENU_PARA == "SBCMR") proddrcr = dr;
                        dbamt = VE.T_TXN.BLAMT.retDbl() * (negamt == "Y" ? -1 : 1);

                        dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), proddrcr,
                            parglcd, sslcd, dbamt, prodrem, prodglcd,
                            null, dbqty, 0, dbcurramt);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        if (proddrcr == "D") dbDrAmt = dbDrAmt + dbamt.retDbl();
                        else dbCrAmt = dbCrAmt + dbamt.retDbl();

                        if (parclass1cd.retStr() != "" || class2cd.retStr() != "")
                        {
                            dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                    parclass1cd, class2cd, dbamt.retDbl(), dbcurramt, strrem);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        }

                        strblno = ""; strbldt = ""; strduedt = ""; strvtype = ""; strrefno = "";
                        strvtype = "BL";
                        if (VE.MENU_PARA == "CADV")
                        {
                            strvtype = "ADV";
                        }
                        strduedt = Convert.ToDateTime(TTXN.DOCDT.Value).AddDays(Convert.ToDouble(TTXN.DUEDAYS)).ToString().retDateStr();

                        strbldt = TTXN.DOCDT.ToString();
                        strblno = DOCPATTERN;

                        string blconslcd = TTXN.CONSLCD;
                        if (TTXN.SLCD != sslcd) blconslcd = TTXN.SLCD;
                        if (blconslcd == sslcd) blconslcd = "";
                        string agslcd = VE.TTXNSLSMN == null ? "" : VE.TTXNSLSMN[0].SLMSLCD;
                        dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, proddrcr,
                               parglcd, sslcd, blconslcd, agslcd, parclass1cd, Convert.ToSByte(isl),
                                dbamt, strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), itamt, TTXNOTH.POREFNO,
                                TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), dbamt.retDbl(),
                                "", "", "", "", VE.T_TXNMEMO.RTDEBCD == null ? "" : VE.T_TXNMEMO.RTDEBCD, VE.T_TXNOTH.BLTYPE);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                    }

                    if (blgstpost == true)
                    {
                        #region TVCHGST Table update    

                        int gs = 0;
                        string dncntag = VE.MENU_PARA == "SBCMR" ? "SC" : ""; string exemptype = "";
                        double gblamt = TTXN.BLAMT.retDbl(); double groamt = TTXN.ROAMT.retDbl();

                        string negamt = TTXN.BLAMT.retDbl() < 0 ? "Y" : "N";
                        //string proddrcr = negamt == "Y" ? dr : cr;
                        //if (negamt == "Y" && VE.MENU_PARA == "SBCMR") proddrcr = dr;
                        gblamt = gblamt.retDbl() * (negamt == "Y" ? -1 : 1);
                        string proddrcr = "";
                        if (VE.TsalePos_TBATCHDTL != null)
                        {
                            for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                            {
                                if (VE.TsalePos_TBATCHDTL[i].SLNO != 0 && VE.TsalePos_TBATCHDTL[i].ITCD != null)
                                {
                                    gs = gs + 1;
                                    if (VE.TsalePos_TBATCHDTL[i].GSTPER.retDbl() == 0) exemptype = "N";
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
                                    TVCHGST.SLNO = gs.retShort();
                                    TVCHGST.PCODE = TTXN.SLCD;
                                    TVCHGST.BLNO = strblno;
                                    if (strbldt.retStr() != "")
                                    {
                                        TVCHGST.BLDT = Convert.ToDateTime(strbldt);
                                    }
                                    TVCHGST.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;
                                    TVCHGST.ITNM = (VE.TsalePos_TBATCHDTL[i].ITNM.retStr() + " ").TrimStart(' ') + VE.TsalePos_TBATCHDTL[i].ITSTYLE;
                                    TVCHGST.AMT = VE.TsalePos_TBATCHDTL[i].TXBLVAL.retDbl();
                                    TVCHGST.CGSTPER = VE.TsalePos_TBATCHDTL[i].CGSTPER.retDbl();
                                    TVCHGST.SGSTPER = VE.TsalePos_TBATCHDTL[i].SGSTPER.retDbl();
                                    TVCHGST.IGSTPER = VE.TsalePos_TBATCHDTL[i].IGSTPER.retDbl();
                                    TVCHGST.CGSTAMT = VE.TsalePos_TBATCHDTL[i].CGSTAMT.retDbl();
                                    TVCHGST.SGSTAMT = VE.TsalePos_TBATCHDTL[i].SGSTAMT.retDbl();
                                    TVCHGST.IGSTAMT = VE.TsalePos_TBATCHDTL[i].IGSTAMT.retDbl();
                                    TVCHGST.CESSPER = VE.TsalePos_TBATCHDTL[i].CESSPER.retDbl();
                                    TVCHGST.CESSAMT = VE.TsalePos_TBATCHDTL[i].CESSAMT.retDbl();
                                    TVCHGST.DRCR = cr;
                                    TVCHGST.QNTY = (VE.TsalePos_TBATCHDTL[i].BLQNTY.retDbl() == 0 ? VE.TsalePos_TBATCHDTL[i].QNTY.retDbl() : VE.TsalePos_TBATCHDTL[i].BLQNTY.retDbl());
                                    TVCHGST.UOM = VE.TsalePos_TBATCHDTL[i].UOM;
                                    TVCHGST.AGSTDOCNO = VE.TsalePos_TBATCHDTL[i].AGDOCNO;
                                    TVCHGST.AGSTDOCDT = VE.TsalePos_TBATCHDTL[i].AGDOCDT.retStr() == "" ? (DateTime?)null : Convert.ToDateTime(VE.TsalePos_TBATCHDTL[i].AGDOCDT);
                                    TVCHGST.SALPUR = salpur;
                                    TVCHGST.INVTYPECD = VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD;
                                    TVCHGST.DNCNCD = TTXNOTH.DNCNCD;
                                    TVCHGST.EXPCD = VE.T_VCH_GST.EXPCD;
                                    TVCHGST.GSTSLNM = VE.T_VCH_GST.GSTSLNM;
                                    TVCHGST.GSTNO = VE.T_VCH_GST.GSTNO;
                                    //TVCHGST.POS = VE.POS;
                                    if (VE.T_VCH_GST.GSTNO != null)
                                    { TVCHGST.POS = VE.T_VCH_GST.GSTNO.Substring(0, 2); }
                                    else { TVCHGST.POS = null; }
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
                                    TVCHGST.EXPGLCD = VE.TsalePos_TBATCHDTL[i].GLCD;
                                    TVCHGST.INPTCLAIM = "Y";
                                    TVCHGST.LUTNO = VE.T_VCH_GST.LUTNO;
                                    TVCHGST.LUTDT = VE.T_VCH_GST.LUTDT;
                                    TVCHGST.TCSPER = TTXN.TCSPER;
                                    TVCHGST.BASAMT = VE.TsalePos_TBATCHDTL[i].GROSSAMT;
                                    TVCHGST.DISCAMT = VE.TsalePos_TBATCHDTL[i].DISCAMT;
                                    TVCHGST.RATE = VE.TsalePos_TBATCHDTL[i].RATE;

                                    TVCHGST.GSTSLADD1 = VE.T_VCH_GST.GSTSLADD1;
                                    TVCHGST.GSTSLDIST = VE.T_VCH_GST.GSTSLDIST;
                                    TVCHGST.GSTSLPIN = VE.T_VCH_GST.GSTSLPIN;
                                    TVCHGST.GSTSLADD2 = VE.T_VCH_GST.GSTSLADD2;
                                    TVCHGST.GSTREGNTYPE = VE.T_VCH_GST.GSTREGNTYPE;
                                    dbsql = masterHelp.RetModeltoSql(TVCHGST, "A", CommVar.FinSchema(UNQSNO));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    gblamt = 0; groamt = 0;
                                }
                            }
                        }
                        #region Return tab
                        if (VE.TsalePos_TBATCHDTL_RETURN != null)
                        {
                            double multamt = 1;
                            //if (VE.MENU_PARA == "SBCM")
                            //{
                            //    if (negamt == "Y")
                            //    {
                            //        proddrcr = cr;
                            //        multamt = -1;
                            //    }
                            //    else
                            //    {
                            //        proddrcr = dr;
                            //    }

                            //}
                            //else
                            //{
                            //    proddrcr = negamt == "Y" ? cr : dr;
                            //}
                            proddrcr = VE.MENU_PARA == "SBCM" ? dr : cr;
                            multamt = negamt == "Y" ? -1 : 1;
                            for (int i = 0; i <= VE.TsalePos_TBATCHDTL_RETURN.Count - 1; i++)
                            {
                                if (VE.TsalePos_TBATCHDTL_RETURN[i].SLNO != 0 && VE.TsalePos_TBATCHDTL_RETURN[i].ITCD != null)
                                {
                                    gs = gs + 1;
                                    if (VE.TsalePos_TBATCHDTL_RETURN[i].GSTPER.retDbl() == 0) exemptype = "N";
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
                                    TVCHGST.SLNO = gs.retShort();
                                    TVCHGST.PCODE = TTXN.SLCD;
                                    TVCHGST.BLNO = strblno;
                                    if (strbldt.retStr() != "")
                                    {
                                        TVCHGST.BLDT = Convert.ToDateTime(strbldt);
                                    }
                                    TVCHGST.HSNCODE = VE.TsalePos_TBATCHDTL_RETURN[i].HSNCODE;
                                    TVCHGST.ITNM = (VE.TsalePos_TBATCHDTL_RETURN[i].ITNM.retStr() + " ").TrimStart(' ') + VE.TsalePos_TBATCHDTL_RETURN[i].ITSTYLE;
                                    TVCHGST.AMT = VE.TsalePos_TBATCHDTL_RETURN[i].TXBLVAL;
                                    TVCHGST.CGSTPER = VE.TsalePos_TBATCHDTL_RETURN[i].CGSTPER.retDbl();
                                    TVCHGST.SGSTPER = VE.TsalePos_TBATCHDTL_RETURN[i].SGSTPER.retDbl();
                                    TVCHGST.IGSTPER = VE.TsalePos_TBATCHDTL_RETURN[i].IGSTPER.retDbl();
                                    TVCHGST.CGSTAMT = VE.TsalePos_TBATCHDTL_RETURN[i].CGSTAMT.retDbl();
                                    TVCHGST.SGSTAMT = VE.TsalePos_TBATCHDTL_RETURN[i].SGSTAMT.retDbl();
                                    TVCHGST.IGSTAMT = VE.TsalePos_TBATCHDTL_RETURN[i].IGSTAMT.retDbl();
                                    TVCHGST.CESSPER = VE.TsalePos_TBATCHDTL_RETURN[i].CESSPER.retDbl();
                                    TVCHGST.CESSAMT = VE.TsalePos_TBATCHDTL_RETURN[i].CESSAMT.retDbl();
                                    TVCHGST.DRCR = proddrcr;// dr;
                                    TVCHGST.QNTY = (VE.TsalePos_TBATCHDTL_RETURN[i].BLQNTY.retDbl() == 0 ? VE.TsalePos_TBATCHDTL_RETURN[i].QNTY.retDbl() : VE.TsalePos_TBATCHDTL_RETURN[i].BLQNTY.retDbl()) * multamt;
                                    TVCHGST.UOM = VE.TsalePos_TBATCHDTL_RETURN[i].UOM;
                                    TVCHGST.AGSTDOCNO = VE.TsalePos_TBATCHDTL_RETURN[i].AGDOCNO;
                                    TVCHGST.AGSTDOCDT = VE.TsalePos_TBATCHDTL_RETURN[i].AGDOCDT.retStr() == "" ? (DateTime?)null : Convert.ToDateTime(VE.TsalePos_TBATCHDTL_RETURN[i].AGDOCDT);
                                    TVCHGST.SALPUR = salpur;
                                    TVCHGST.INVTYPECD = VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD;
                                    TVCHGST.DNCNCD = TTXNOTH.DNCNCD;
                                    TVCHGST.EXPCD = VE.T_VCH_GST.EXPCD;
                                    TVCHGST.GSTSLNM = VE.T_VCH_GST.GSTSLNM;
                                    TVCHGST.GSTNO = VE.T_VCH_GST.GSTNO;
                                    //TVCHGST.POS = VE.POS;
                                    if (VE.T_VCH_GST.GSTNO != null)
                                    { TVCHGST.POS = VE.T_VCH_GST.GSTNO.Substring(0, 2); }
                                    else { TVCHGST.POS = null; }

                                    TVCHGST.SHIPDOCNO = VE.T_VCH_GST.SHIPDOCNO;
                                    TVCHGST.SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT;
                                    TVCHGST.PORTCD = VE.T_VCH_GST.PORTCD;
                                    TVCHGST.OTHRAMT = 0;
                                    TVCHGST.ROAMT = groamt * multamt;
                                    TVCHGST.BLAMT = gblamt * multamt;
                                    TVCHGST.DNCNSALPUR = dncntag;
                                    TVCHGST.CONSLCD = TTXN.CONSLCD;
                                    TVCHGST.APPLTAXRATE = 0;
                                    TVCHGST.EXEMPTEDTYPE = exemptype;
                                    TVCHGST.EXPGLCD = VE.TsalePos_TBATCHDTL_RETURN[i].GLCD;
                                    TVCHGST.INPTCLAIM = "Y";
                                    TVCHGST.LUTNO = VE.T_VCH_GST.LUTNO;
                                    TVCHGST.LUTDT = VE.T_VCH_GST.LUTDT;
                                    TVCHGST.TCSPER = TTXN.TCSPER;
                                    TVCHGST.BASAMT = VE.TsalePos_TBATCHDTL_RETURN[i].GROSSAMT;
                                    TVCHGST.DISCAMT = VE.TsalePos_TBATCHDTL_RETURN[i].DISCAMT;
                                    TVCHGST.RATE = VE.TsalePos_TBATCHDTL_RETURN[i].RATE;
                                    TVCHGST.GSTSLADD1 = VE.T_VCH_GST.GSTSLADD1;
                                    TVCHGST.GSTSLDIST = VE.T_VCH_GST.GSTSLDIST;
                                    TVCHGST.GSTSLPIN = VE.T_VCH_GST.GSTSLPIN;
                                    TVCHGST.GSTSLADD2 = VE.T_VCH_GST.GSTSLADD2;
                                    TVCHGST.GSTREGNTYPE = VE.T_VCH_GST.GSTREGNTYPE;
                                    dbsql = masterHelp.RetModeltoSql(TVCHGST, "A", CommVar.FinSchema(UNQSNO));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    gblamt = 0; groamt = 0;
                                }
                            }
                        }

                        #endregion

                        if (VE.TTXNAMT != null)
                        {
                            proddrcr = negamt == "Y" ? dr : cr;
                            for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                            {

                                string good_serv = "G";
                                if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                                {
                                    if (VE.TTXNAMT[i].HSNCODE.retStr().Length > 0)
                                    {
                                        good_serv = "S";
                                    }
                                    else
                                    {
                                        string HSNforAmount = "";
                                        if (VE.MENU_PARA == "SBCM")
                                        {
                                            HSNforAmount = (from a in VE.TsalePos_TBATCHDTL
                                                            group a by new
                                                            {
                                                                HSNSACCD = a.HSNCODE
                                                            } into X
                                                            select new
                                                            {
                                                                HSNSACCD = X.Key.HSNSACCD,
                                                                AMOUNT = X.Sum(Z => Z.GROSSAMT).retDbl(),
                                                            }).OrderByDescending(a => a.AMOUNT).FirstOrDefault()?.HSNSACCD;
                                        }
                                        else
                                        {
                                            HSNforAmount = (from a in VE.TsalePos_TBATCHDTL_RETURN
                                                            group a by new
                                                            {
                                                                HSNSACCD = a.HSNCODE
                                                            } into X
                                                            select new
                                                            {
                                                                HSNSACCD = X.Key.HSNSACCD,
                                                                AMOUNT = X.Sum(Z => Z.GROSSAMT).retDbl(),
                                                            }).OrderByDescending(a => a.AMOUNT).FirstOrDefault()?.HSNSACCD;
                                        }
                                        VE.TTXNAMT[i].HSNCODE = HSNforAmount;
                                    }

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
                                    TVCHGST1.AMT = VE.TTXNAMT[i].AMT;
                                    TVCHGST1.CGSTPER = VE.TTXNAMT[i].CGSTPER.retDbl();
                                    TVCHGST1.SGSTPER = VE.TTXNAMT[i].SGSTPER.retDbl();
                                    TVCHGST1.IGSTPER = VE.TTXNAMT[i].IGSTPER.retDbl();
                                    TVCHGST1.CGSTAMT = VE.TTXNAMT[i].CGSTAMT.retDbl();
                                    TVCHGST1.SGSTAMT = VE.TTXNAMT[i].SGSTAMT.retDbl();
                                    TVCHGST1.IGSTAMT = VE.TTXNAMT[i].IGSTAMT.retDbl();
                                    TVCHGST1.CESSPER = VE.TTXNAMT[i].CESSPER.retDbl();
                                    TVCHGST1.CESSAMT = VE.TTXNAMT[i].CESSAMT.retDbl();
                                    TVCHGST1.DRCR = proddrcr;// cr;
                                    TVCHGST1.QNTY = 0;
                                    TVCHGST1.UOM = "OTH";
                                    TVCHGST1.AGSTDOCNO = VE.MENU_PARA == "SBCM" ? VE.TsalePos_TBATCHDTL[0].AGDOCNO : VE.TsalePos_TBATCHDTL_RETURN[0].AGDOCNO;
                                    string AGDOCDT = VE.MENU_PARA == "SBCM" ? VE.TsalePos_TBATCHDTL[0].AGDOCDT : VE.TsalePos_TBATCHDTL_RETURN[0].AGDOCDT;
                                    if (AGDOCDT.retStr() != "")
                                    {
                                        TVCHGST1.AGSTDOCDT = Convert.ToDateTime(AGDOCDT);
                                    }
                                    TVCHGST1.SALPUR = salpur;
                                    TVCHGST1.INVTYPECD = VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD;
                                    TVCHGST1.DNCNCD = TTXNOTH.DNCNCD;
                                    TVCHGST1.EXPCD = VE.T_VCH_GST.EXPCD;
                                    TVCHGST1.GSTSLNM = VE.T_VCH_GST.GSTSLNM;
                                    TVCHGST1.GSTNO = VE.T_VCH_GST.GSTNO;
                                    //TVCHGST1.POS = VE.POS;
                                    if (VE.T_VCH_GST.GSTNO != null)
                                    { TVCHGST1.POS = VE.T_VCH_GST.GSTNO.Substring(0, 2); }
                                    else { TVCHGST1.POS = null; }

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
                                    TVCHGST1.GOOD_SERV = good_serv;
                                    //TVCHGST1.EXPGLCD = VE.TTXNAMT[i].GLCD;
                                    TVCHGST1.EXPGLCD = VE.TTXNAMT[i].GLCD.retStr() == "" ? expglcd : VE.TTXNAMT[i].GLCD.retStr();
                                    TVCHGST1.INPTCLAIM = "Y";
                                    TVCHGST1.LUTNO = VE.T_VCH_GST.LUTNO;
                                    TVCHGST1.LUTDT = VE.T_VCH_GST.LUTDT;
                                    TVCHGST1.TCSPER = TTXN.TCSPER;
                                    TVCHGST1.BASAMT = VE.TTXNAMT[i].AMT;
                                    TVCHGST1.RATE = VE.TTXNAMT[i].AMTRATE;
                                    TVCHGST1.GSTSLADD1 = VE.T_VCH_GST.GSTSLADD1;
                                    TVCHGST1.GSTSLDIST = VE.T_VCH_GST.GSTSLDIST;
                                    TVCHGST1.GSTSLPIN = VE.T_VCH_GST.GSTSLPIN;
                                    TVCHGST1.GSTSLADD2 = VE.T_VCH_GST.GSTSLADD2;
                                    TVCHGST1.GSTREGNTYPE = VE.T_VCH_GST.GSTREGNTYPE;
                                    dbsql = masterHelp.RetModeltoSql(TVCHGST1, "A", CommVar.FinSchema(UNQSNO));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    gblamt = 0; groamt = 0;
                                }
                            }
                        }
                        #endregion
                    }

                    #region Payment tab
                    double recdamt = 0; short pslno = 0, adjslno = 0;
                    if (blactpost == true || VE.MENU_PARA == "CADV")
                    {
                        if (VE.TTXNPYMT != null)
                        {
                            string agslcd = VE.TTXNSLSMN.retStr() == "" ? "" : VE.TTXNSLSMN[0].SLMSLCD;

                            int paymntsl = 0; double pymtamt = 0; string pymtrem = "", pymtno = "";

                            for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                            {
                                if (VE.TTXNPYMT[i].GLCD.retStr() != "" && VE.TTXNPYMT[i].AMT.retDbl() != 0)
                                {
                                    pymtrem = "Cash";
                                    if (VE.TTXNPYMT[i].PYMTTYPE.retStr() == "C")
                                    {
                                        pymtrem = "Cash";
                                    }
                                    else if (VE.TTXNPYMT[i].PYMTTYPE.retStr() == "R")
                                    {
                                        pymtrem = "Card";
                                    }
                                    else if (VE.TTXNPYMT[i].PYMTTYPE.retStr() == "B")
                                    {
                                        pymtrem = "Bank";
                                    }
                                    else if (VE.TTXNPYMT[i].PYMTTYPE.retStr() == "U")
                                    {
                                        pymtrem = "UPI";
                                    }
                                    else if (VE.TTXNPYMT[i].PYMTTYPE.retStr() == "V")
                                    {
                                        pymtrem = "Voucher";
                                    }
                                    else if (VE.TTXNPYMT[i].PYMTTYPE.retStr() == "O")
                                    {
                                        pymtrem = "Others";
                                    }
                                    else
                                    {
                                        pymtrem = "Cash";
                                    }
                                    if (VE.MENU_PARA == "CADV")
                                    {
                                        pymtamt += VE.TTXNPYMT[i].AMT.retDbl();
                                        paymntsl++;
                                        dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                           parglcd, sslcd, null, agslcd, parclass1cd, Convert.ToSByte(paymntsl),
                                            VE.TTXNPYMT[i].AMT.retDbl(), strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), 0, TTXNOTH.POREFNO,
                                            TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), VE.TTXNPYMT[i].AMT.retDbl(),
                                            "", "", "", "", VE.T_TXNMEMO.RTDEBCD == null ? "" : VE.T_TXNMEMO.RTDEBCD);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();


                                        T_VCH_DET TVCHDET = new T_VCH_DET();
                                        TVCHDET.AUTONO = TTXN.AUTONO; TVCHDET.CLCD = TTXN.CLCD; TVCHDET.DTAG = TTXN.DTAG; TVCHDET.EMD_NO = TTXN.EMD_NO;
                                        TVCHDET.DOCCD = TTXN.DOCCD; TVCHDET.DOCDT = TTXN.DOCDT; TVCHDET.DOCNO = TTXN.DOCNO;
                                        TVCHDET.SLNO = paymntsl + 100; TVCHDET.DRCR = dr; TVCHDET.GLCD = VE.TTXNPYMT[i].GLCD; TVCHDET.AMT = VE.TTXNPYMT[i].AMT; TVCHDET.R_SLCD = sslcd; TVCHDET.R_GLCD = parglcd;
                                        TVCHDET.T_REM = pymtrem; TVCHDET.CHQNO = VE.TTXNPYMT[i].INSTNO; TVCHDET.OSLNO = paymntsl;
                                        if (VE.TTXNPYMT[i].INSTDT.retDateStr() != "") TVCHDET.CHQDT = Convert.ToDateTime(VE.TTXNPYMT[i].INSTDT.retDateStr());

                                        dbsql = masterHelp.RetModeltoSql(TVCHDET, "A", scmf);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                    }
                                    else
                                    {
                                        pslno++; adjslno++;
                                        double mult = 1;
                                        if (VE.MENU_PARA == "SBCMR") mult = -1;
                                        dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(pslno + 100), cr,
                                            parglcd, sslcd, VE.TTXNPYMT[i].AMT.retDbl(), pymtrem, VE.TTXNPYMT[i].GLCD);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                        if (parclass1cd.retStr() != "" || class2cd.retStr() != "")
                                        {
                                            dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(pslno + 100), sslcd,
                                                    parclass1cd, class2cd, VE.TTXNPYMT[i].AMT.retDbl());
                                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                        }
                                        //string agslcd = VE.TTXNSLSMN == null ? "" : VE.TTXNSLSMN[0].SLMSLCD;
                                        dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                           parglcd, sslcd, null, agslcd, parclass1cd, Convert.ToSByte(pslno + 100),
                                            VE.TTXNPYMT[i].AMT.retDbl(), strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), 0, TTXNOTH.POREFNO,
                                            TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), VE.TTXNPYMT[i].AMT.retDbl(),
                                            "", "", "", "", VE.T_TXNMEMO.RTDEBCD == null ? "" : VE.T_TXNMEMO.RTDEBCD);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                                        dbsql = masterHelp.InsVch_Bl_Adj(TTXN.AUTONO, TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(adjslno), TTXN.AUTONO, 1, dbamt.retDbl(), TTXN.AUTONO, Convert.ToSByte(pslno + 100), VE.TTXNPYMT[i].AMT.retDbl(), (VE.TTXNPYMT[i].AMT.retDbl() * mult));
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();


                                        T_VCH_DET TVCHDET = new T_VCH_DET();
                                        TVCHDET.AUTONO = TTXN.AUTONO; TVCHDET.CLCD = TTXN.CLCD; TVCHDET.DTAG = TTXN.DTAG; TVCHDET.EMD_NO = TTXN.EMD_NO;
                                        TVCHDET.DOCCD = TTXN.DOCCD; TVCHDET.DOCDT = TTXN.DOCDT; TVCHDET.DOCNO = TTXN.DOCNO;
                                        TVCHDET.SLNO = pslno + 200; TVCHDET.DRCR = dr; TVCHDET.GLCD = VE.TTXNPYMT[i].GLCD; TVCHDET.AMT = VE.TTXNPYMT[i].AMT; TVCHDET.R_SLCD = sslcd; TVCHDET.R_GLCD = parglcd;
                                        TVCHDET.T_REM = pymtrem; TVCHDET.CHQNO = VE.TTXNPYMT[i].INSTNO; TVCHDET.OSLNO = pslno + 100;
                                        if (VE.TTXNPYMT[i].INSTDT.retDateStr() != "") TVCHDET.CHQDT = Convert.ToDateTime(VE.TTXNPYMT[i].INSTDT.retDateStr());

                                        dbsql = masterHelp.RetModeltoSql(TVCHDET, "A", scmf);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                        recdamt = recdamt + VE.TTXNPYMT[i].AMT.retDbl();
                                    }
                                }
                            }
                            if (VE.MENU_PARA == "CADV")
                            {
                                pymtrem = VE.T_TXNMEMO.NM.retStr() == "" ? VE.SLNM : VE.T_TXNMEMO.NM.retStr();

                                pymtrem = VE.T_TXNMEMO.NM.retStr() == "" ? VE.SLNM : VE.T_TXNMEMO.NM.retStr();
                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(2), cr,
                                       glcd, sslcd, pymtamt, pymtrem);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    #endregion

                    if (VE.MENU_PARA != "CADV")
                    {
                        if (igst + cgst + sgst == 0)
                        {
                            ContentFlg = "TAX amount not found. Please add tax with item.";
                            goto dbnotsave;
                        }
                    }
                    if (Math.Round(dbDrAmt, 2) != Math.Round(dbCrAmt, 2))
                    {
                        ContentFlg = "Debit " + Math.Round(dbDrAmt, 2) + " & Credit " + Math.Round(dbCrAmt, 2) + " not matching please click on rounded off...";
                        goto dbnotsave;
                    }

                    #endregion
                    if (VE.DefaultAction == "A")
                    {
                        ContentFlg = "1" + " (Voucher No. " + DOCPATTERN + ")~" + TTXN.AUTONO;
                    }
                    else if (VE.DefaultAction == "E")
                    {
                        ContentFlg = "2";
                    }
                    goto dbsave;

                }
                else if (VE.DefaultAction == "V")
                {
                    dbsql = masterHelp.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    if (VE.TsalePos_TBATCHDTL != null)
                    {
                        foreach (var v in VE.TsalePos_TBATCHDTL)
                        {
                            var IsTransactionFound = salesfunc.IsTransactionFound("", v.BARNO.retSqlformat(), VE.T_TXN.AUTONO.retSqlformat());
                            if (IsTransactionFound != "")
                            {
                                ContentFlg = "We cant delete this Bill. Transaction found at " + IsTransactionFound; goto dbnotsave;
                            }
                            else
                            {
                                dbsql = masterHelp.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D", "", "barno='" + v.BARNO + "' and autono='" + VE.T_TXN.AUTONO + "'");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                                dbsql = masterHelp.TblUpdt("t_batchmst_price", VE.T_TXN.AUTONO, "D", "", "barno='" + v.BARNO + "' and autono='" + VE.T_TXN.AUTONO + "'");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            }
                        }
                    }
                    if (VE.TsalePos_TBATCHDTL_RETURN != null)
                    {
                        foreach (var v in VE.TsalePos_TBATCHDTL_RETURN)
                        {
                            var IsTransactionFound = salesfunc.IsTransactionFound("", v.BARNO.retSqlformat(), VE.T_TXN.AUTONO.retSqlformat());
                            if (IsTransactionFound != "")
                            {
                                ContentFlg = "We cant delete this Bill. Transaction found at " + IsTransactionFound; goto dbnotsave;
                            }
                            else
                            {
                                dbsql = masterHelp.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D", "", "barno='" + v.BARNO + "' and autono='" + VE.T_TXN.AUTONO + "'");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                                dbsql = masterHelp.TblUpdt("t_batchmst_price", VE.T_TXN.AUTONO, "D", "", "barno='" + v.BARNO + "' and autono='" + VE.T_TXN.AUTONO + "'");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            }
                        }
                    }
                    dbsql = masterHelp.TblUpdt("t_txnoth", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    dbsql = masterHelp.TblUpdt("T_TXNPYMT_HDR", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("T_TXNMEMO", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("T_TXNTRANS", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    dbsql = masterHelp.TblUpdt("T_CNTRL_HDR_UNIQNO", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                    dbsql = masterHelp.TblUpdt("t_txnslsmn", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnpymt", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnamt", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txn_linkno", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txndtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txn", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    //   Delete from financial schema
                    dbsql = masterHelp.finTblUpdt("T_TXNEWB", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_auth", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.finTblUpdt("t_vch_gst", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_bl_adj", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_bl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_class", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_det", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_hdr", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    //dbsql = masterHelp.finTblUpdt("t_cntrl_hdr", VE.T_TXN.AUTONO, "D");
                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "F", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null, null, null, null, null, VE.T_TXN.BLAMT.retDbl(), VE.Audit_REM);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                    //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null, null, null, null, null, VE.T_TXN.BLAMT.retDbl(), VE.Audit_REM);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                    ContentFlg = "3";
                    goto dbsave;
                }
                else
                {
                    return Content("");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                ContentFlg = ex.Message + ex.StackTrace;
                goto dbnotsave;
            }
            dbsave:
            {
                OraTrans.Commit();
                OraCon.Dispose();
                if (othr_para == "")
                    return Content(ContentFlg);
                else if (othr_para == "SAVEANDPRINT")
                {
                    // string sequence doccd~docdt~docno
                    ContentFlg = TCH.DOCCD + "~" + TCH.DOCDT.retDateStr() + "~" + TCH.DOCONLYNO;
                    return (ContentFlg);
                }
                else
                    return ContentFlg;
            }
            dbnotsave:
            {
                OraTrans.Rollback();
                OraCon.Dispose();
                if (othr_para == "")
                    return Content(ContentFlg);
                else if (othr_para == "SAVEANDPRINT")
                    return (ContentFlg);
                else
                    return ContentFlg;
            }
        }
        public ActionResult Print(TransactionSalePosEntry VE, FormCollection FC, string DOCNO, string DOC_CD, string DOCDT, string MENUPARA)
        {
            try
            {
                ReportViewinHtml ind = new ReportViewinHtml();
                ind.DOCCD = DOC_CD;
                ind.FDOCNO = DOCNO;
                ind.TDOCNO = DOCNO;
                ind.FDT = DOCDT;
                ind.TDT = DOCDT;
                ind.MENU_PARA = MENUPARA;
                if (TempData["printparameter"] != null)
                {
                    TempData.Remove("printparameter");
                }
                TempData["printparameter"] = ind;
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
                var link = Cn.SaveImage(ImageStr, "/UploadDocuments/" + filename);
                var path = CommVar.WebUploadDocURL(filename);
                return Content(path);
            }
            catch (Exception ex)
            {
                return Content("//.");
            }
        }
        public ActionResult GetTTXNDTLDetails(TransactionSalePosEntry VE, string FDT, string TDT, string R_DOCNO, string R_BARNO, string R_DOCCD, string R_TAXGRPCD)
        {
            Cn.getQueryString(VE); string scm = CommVar.CurSchema(UNQSNO);
            string sql = "select x.autono,x.docno,x.docdt,x.itcd,x.itnm,x.styleno,x.itgrpcd,x.itgrpnm,x.qnty,x.uomcd,x.stktype, ";
            sql += "x.barno,x.TXBLVAL,x.IGSTPER,x.CGSTPER,x.SGSTPER,x.CESSPER,x.SALGLCD,y.prodgrpgstper,x.HSNCODE,x.DISCTYPE,x.DISCRATE,x.INCLRATE,x.rate from ";

            sql += "(select a.autono,a.docno,a.docdt,b.itcd,e.itnm,e.styleno,e.prodgrpcd,e.itgrpcd,f.itgrpnm,b.qnty,e.uomcd,b.stktype, ";
            sql += "d.barno,b.TXBLVAL,b.IGSTPER,b.CGSTPER,b.SGSTPER,b.CESSPER,f.SALGLCD,b.HSNCODE,b.DISCTYPE,b.DISCRATE,d.INCLRATE,b.rate from  " + scm + ".T_TXN a, ";
            sql += "" + scm + ".T_TXNDTL b," + scm + ".T_CNTRL_HDR c, " + scm + ".T_BATCHDTL d ," + scm + ".M_SITEM e ," + scm + ".M_GROUP f," + scm + ".M_DOCTYPE g ";
            sql += "where a.autono = c.autono(+) and a.autono = b.autono(+) and b.autono = d.autono(+) and a.doccd=g.doccd(+) ";
            //sql += "and b.slno = d.txnslno(+)and b.itcd = e.itcd(+) and e.itgrpcd = f.itgrpcd(+) and a.doccd in('" + R_DOCCD + "')  ";
            sql += "and b.slno = d.txnslno(+)and b.itcd = e.itcd(+) and e.itgrpcd = f.itgrpcd(+) and g.doctype in('SBCM')  ";
            if (R_DOCNO.retStr() != "") sql += " and a.docno in('" + R_DOCNO + "') ";
            if (FDT.retDateStr() != "") sql += "and a.docdt >= to_date('" + FDT + "', 'dd/mm/yyyy') ";
            if (TDT.retDateStr() != "") sql += " and a.docdt <= to_date('" + TDT + "', 'dd/mm/yyyy')  ";
            if (R_BARNO.retStr() != "") sql += "and d.barno = '" + R_BARNO + "' ";
            sql += ")x, ";

            sql += "(select a.prodgrpcd, ";
            //sql += "listagg(b.fromrt||chr(181)||b.tort||chr(181)||b.igstper||chr(181)||b.cgstper||chr(181)||b.sgstper,chr(179)) ";
            sql += "listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179)) ";
            sql += "within group (order by a.prodgrpcd) as prodgrpgstper ";
            sql += "from ";
            sql += "(select prodgrpcd, effdt from ";
            sql += "(select a.prodgrpcd, a.effdt, ";
            sql += "row_number() over (partition by a.prodgrpcd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".m_prodtax a ";
            if (TDT.retDateStr() != "") sql += "where a.effdt <= to_date('" + TDT + "','dd/mm/yyyy')  ";
            sql += ")where rn=1 ) a, " + scm + ".m_prodtax b ";
            sql += "where a.prodgrpcd=b.prodgrpcd(+) and a.effdt=b.effdt(+) and b.taxgrpcd='" + R_TAXGRPCD + "' ";
            sql += "group by a.prodgrpcd ) y ";

            sql += "where x.prodgrpcd=y.prodgrpcd(+) ";
            sql += "order by x.docdt, x.docno ";
            DataTable dt = masterHelp.SQLquery(sql);
            DataTable PRODGRPDATA = new DataTable();
            if (dt != null && dt.Rows.Count > 0)
            {

                VE.TTXNDTLPOPUP = (from DataRow dr in dt.Rows
                                   select new TTXNDTLPOPUP
                                   {
                                       AUTONO = dr["autono"].retStr(),
                                       ITCD = dr["itcd"].retStr(),
                                       BARNO = dr["barno"].retStr(),
                                       ITSTYLE = dr["styleno"].retStr() + " " + dr["itnm"].retStr(),
                                       QNTY = dr["qnty"].retDbl(),
                                       AGDOCNO = dr["docno"].retStr(),
                                       AGDOCDT = dr["docdt"].retDateStr(),
                                       ITGRPCD = dr["itgrpcd"].retStr(),
                                       ITGRPNM = dr["itgrpnm"].retStr(),
                                       IGSTPER = dr["IGSTPER"].retDbl(),
                                       CGSTPER = dr["CGSTPER"].retDbl(),
                                       SGSTPER = dr["SGSTPER"].retDbl(),
                                       CESSPER = dr["CESSPER"].retDbl(),
                                       STKTYP = dr["stktype"].retStr(),
                                       UOM = dr["uomcd"].retStr(),
                                       PRODGRPGSTPER = dr["PRODGRPGSTPER"].retStr(),
                                       GLCD = dr["SALGLCD"].retStr(),
                                       HSNCODE = dr["HSNCODE"].retStr(),
                                       DISCTYPE = dr["DISCTYPE"].retStr(),
                                       DISCRATE = dr["DISCRATE"].retDbl(),
                                       INCLRATE = dr["INCLRATE"].retDbl(),
                                       RATE = dr["RATE"].retDbl()

                                   }).ToList();
                int slno = 0;
                for (int p = 0; p <= VE.TTXNDTLPOPUP.Count - 1; p++)
                {
                    slno++;
                    VE.TTXNDTLPOPUP[p].SLNO = slno.retShort();
                }

            }


            VE.DefaultView = true;
            return PartialView("_T_SALE_POS_RETURN_POPUP", VE);
        }
        public ActionResult SelectTTXNDTLDetails(TransactionSalePosEntry VE)
        {
            try
            {
                Cn.getQueryString(VE); ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                var _TTXNDTLPOPUP = VE.TTXNDTLPOPUP.Where(r => r.P_Checked == true).ToList(); int slno = 0, SERIAL = 0;
                List<TsalePos_TBATCHDTL_RETURN> tsalePosReturnList = new List<TsalePos_TBATCHDTL_RETURN>();
                VE.DISC_TYPE = masterHelp.DISC_TYPE();
                VE.PCSActionList = masterHelp.PCSAction();
                foreach (var row in _TTXNDTLPOPUP)
                {
                    TsalePos_TBATCHDTL_RETURN tsalePos_TBATCHDTL_RETURN = new TsalePos_TBATCHDTL_RETURN();
                    //tsalePos_TBATCHDTL_RETURN.SLNO = (SERIAL + 1).retShort();
                    tsalePos_TBATCHDTL_RETURN.AUTONO = row.AUTONO.retStr();
                    tsalePos_TBATCHDTL_RETURN.AGDOCNO = row.AGDOCNO.retStr();
                    tsalePos_TBATCHDTL_RETURN.AGDOCDT = row.AGDOCDT.retDateStr();
                    tsalePos_TBATCHDTL_RETURN.BARNO = row.BARNO.retStr();
                    tsalePos_TBATCHDTL_RETURN.ITCD = row.ITCD.retStr();
                    tsalePos_TBATCHDTL_RETURN.ITSTYLE = row.ITSTYLE.retStr();
                    tsalePos_TBATCHDTL_RETURN.QNTY = row.QNTY.retDbl();
                    tsalePos_TBATCHDTL_RETURN.INCLRATE = row.INCLRATE.retDbl();
                    tsalePos_TBATCHDTL_RETURN.RATE = row.RATE.retDbl();
                    tsalePos_TBATCHDTL_RETURN.UOM = row.UOM.retStr();
                    tsalePos_TBATCHDTL_RETURN.STKTYPE = row.STKTYP.retStr();
                    tsalePos_TBATCHDTL_RETURN.ITGRPCD = row.ITGRPCD.retStr();
                    tsalePos_TBATCHDTL_RETURN.ITGRPNM = row.ITGRPNM.retStr();
                    tsalePos_TBATCHDTL_RETURN.IGSTPER = row.IGSTPER.retDbl();
                    tsalePos_TBATCHDTL_RETURN.CGSTPER = row.CGSTPER.retDbl();
                    tsalePos_TBATCHDTL_RETURN.SGSTPER = row.SGSTPER.retDbl();
                    tsalePos_TBATCHDTL_RETURN.CESSPER = row.CESSPER.retDbl();
                    tsalePos_TBATCHDTL_RETURN.PRODGRPGSTPER = row.PRODGRPGSTPER.retStr();
                    tsalePos_TBATCHDTL_RETURN.GLCD = row.GLCD.retStr();
                    tsalePos_TBATCHDTL_RETURN.HSNCODE = row.HSNCODE.retStr();
                    tsalePos_TBATCHDTL_RETURN.DISCTYPE = row.DISCTYPE.retStr();
                    tsalePos_TBATCHDTL_RETURN.DISCTYPE_DESC = row.DISCTYPE.retStr() == "P" ? "%" : row.DISCTYPE.retStr() == "N" ? "Nos" : row.DISCTYPE.retStr() == "Q" ? "Qnty" : "Fixed";
                    tsalePos_TBATCHDTL_RETURN.DISCRATE = row.DISCRATE.retDbl();
                    //tsalePos_TBATCHDTL_RETURN.TXBLVAL = row.TXBLVAL.retDbl();
                    if (VE.TsalePos_TBATCHDTL_RETURN == null) VE.TsalePos_TBATCHDTL_RETURN = new List<TsalePos_TBATCHDTL_RETURN>();
                    VE.TsalePos_TBATCHDTL_RETURN.Add(tsalePos_TBATCHDTL_RETURN);
                }
                if (VE.TsalePos_TBATCHDTL_RETURN != null)
                {
                    for (int i = 0; i < VE.TsalePos_TBATCHDTL_RETURN.Count; i++)
                    {
                        var itcd = VE.TsalePos_TBATCHDTL_RETURN[i].ITCD.retStr();
                        var autono = VE.TsalePos_TBATCHDTL_RETURN[i].AUTONO.retStr();
                        VE.TsalePos_TBATCHDTL_RETURN[i].SLNO = (1001 + i).retShort();
                        var gstper = (VE.TsalePos_TBATCHDTL_RETURN[i].IGSTPER.retDbl() + VE.TsalePos_TBATCHDTL_RETURN[i].CGSTPER.retDbl() + VE.TsalePos_TBATCHDTL_RETURN[i].SGSTPER.retDbl() + VE.TsalePos_TBATCHDTL_RETURN[i].CESSPER.retDbl());
                        VE.TsalePos_TBATCHDTL_RETURN[i].GSTPER = gstper.retDbl();
                        //VE.TsalePos_TBATCHDTL_RETURN[i].DISC_TYPE = masterHelp.DISC_TYPE();
                        //VE.TsalePos_TBATCHDTL_RETURN[i].PCSActionList = masterHelp.PCSAction();
                        VE.TsalePos_TBATCHDTL_RETURN[i].COLRNM = (from j in DB.T_TXNDTL
                                                                  join k in DB.M_COLOR on j.COLRCD equals k.COLRCD
                                                                  where j.ITCD == itcd && j.AUTONO == autono
                                                                  select k.COLRNM).FirstOrDefault();
                        VE.TsalePos_TBATCHDTL_RETURN[i].SIZECD = (from j in DB.T_TXNDTL
                                                                  join k in DB.M_SIZE on j.SIZECD equals k.SIZECD
                                                                  where j.ITCD == itcd && j.AUTONO == autono
                                                                  select k.SIZECD).FirstOrDefault();
                        VE.TsalePos_TBATCHDTL_RETURN[i].PARTCD = (from j in DB.T_TXNDTL
                                                                  join k in DB.M_PARTS on j.PARTCD equals k.PARTCD
                                                                  where j.ITCD == itcd && j.AUTONO == autono
                                                                  select k.PARTCD).FirstOrDefault();
                        if (autono != "")
                        {
                            VE.TsalePos_TBATCHDTL_RETURN[i].MTRLJOBCD = (from j in DB.T_TXNDTL
                                                                         where j.AUTONO == autono && j.ITCD == itcd
                                                                         select j.MTRLJOBCD).FirstOrDefault();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

            VE.DefaultView = true;
            ModelState.Clear();
            return PartialView("_T_SALE_POS_RETURN", VE);

        }
        [HttpPost]
        public ActionResult T_SALE_POS(FormCollection FC, TransactionSalePosEntry VE)
        {
            try
            {
                var AUTONO = VE.T_TXN.AUTONO;
                var sql = "select * from " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr where autono='" + AUTONO + "'";
                DataTable dt = masterHelp.SQLquery(sql);

                SaleBill_PrintController RepPos = new SaleBill_PrintController();
                ReportViewinHtml RVH = new ReportViewinHtml();
                RVH.DOCCD = dt.Rows[0]["DOCCD"].retStr();
                RVH.TDT = dt.Rows[0]["DOCdt"].retDateStr();
                RVH.FDT = dt.Rows[0]["DOCdt"].retDateStr();
                RVH.DOCNO = dt.Rows[0]["doconlyno"].retStr();
                RVH.FDOCNO = dt.Rows[0]["doconlyno"].retStr();
                RVH.TDOCNO = dt.Rows[0]["doconlyno"].retStr();
                RVH.MENU_PARA = VE.MENU_PARA;
                DataTable repformat = salesfunc.getRepFormat("CASHMEMO");
                if (repformat != null && repformat.Rows.Count > 0)
                {
                    RVH.TEXTBOX6 = repformat.Rows[0]["value"].retStr();
                }
                RVH.TEXTBOX7 = "Half";
                RVH.TEXTBOX10 = "2";
                return RepPos.SaleBill_Print(RVH, FC, "PrintToPrinter");
            }
            catch
            {

            }
            return null;
            //string retmag = SAVE(FC, VE, "SAVEANDPRINT");
            //var retarr = retmag.Split('~');
            //if (retarr.Length == 1)
            //{
            //    return Content(retmag);
            //}
            //else
            //{

            //}

        }
        public JsonResult GetGstInfo(string GSTNO)
        {
            try
            {
                AdaequareGSP adaequareGSP = new AdaequareGSP();
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var AdqrRespGstInfo = adaequareGSP.AdqrGstInfo(GSTNO);
                //var AdqrRespGstInfo = adaequareGSP.AdqrGstInfoTestMode(GSTNO);

                if (AdqrRespGstInfo.success == true && AdqrRespGstInfo.result != null)
                {
                    dic.Add("message", "ok");
                    dic.Add("Gstin", AdqrRespGstInfo.result.Gstin);
                    string StateCd = AdqrRespGstInfo.result.Gstin.Substring(0, 2);
                    string StateNm = DB1.MS_STATE.Find(StateCd)?.STATENM;
                    string panno = AdqrRespGstInfo.result.Gstin.Substring(2, 10);
                    string comtype = panno.Substring(3, 1);
                    dic.Add("StateCd", StateCd);
                    dic.Add("StateNm", StateNm);
                    dic.Add("Panno", panno);
                    dic.Add("Comptype", Getcomptype(comtype));
                    dic.Add("TradeName", AdqrRespGstInfo.result.TradeName);
                    if (AdqrRespGstInfo.result.TradeName == AdqrRespGstInfo.result.LegalName)
                    {
                        dic.Add("LegalName", "");
                    }
                    else
                    {
                        dic.Add("LegalName", AdqrRespGstInfo.result.LegalName);
                    }
                    dic.Add("AddrBnm", AdqrRespGstInfo.result.AddrBnm);
                    dic.Add("AddrBno", AdqrRespGstInfo.result.AddrBno);
                    dic.Add("AddrFlno", AdqrRespGstInfo.result.AddrFlno);
                    dic.Add("AddrSt", AdqrRespGstInfo.result.AddrSt.retStr());
                    dic.Add("AddrLoc", AdqrRespGstInfo.result.AddrLoc);
                    dic.Add("AddrPncd", AdqrRespGstInfo.result.AddrPncd.retStr());
                    dic.Add("TxpType", AdqrRespGstInfo.result.TxpType);
                }
                else
                {
                    dic.Add("message", AdqrRespGstInfo.message);
                }
                ModelState.Clear();
                return Json(dic, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Json(ex.Message + ex.InnerException, JsonRequestBehavior.AllowGet);
            }
        }
        private string Getcomptype(string comtype)
        {
            string VARR = "";
            switch (comtype)//01
            {
                case "C":
                    VARR = "02"; break;
                case "P":
                    VARR = "05"; break;
                case "F":
                    VARR = "03"; break;
                case "H":
                    VARR = "08"; break;
                case "T":
                    VARR = "10"; break;
                default: VARR = ""; break;
            }
            return VARR;
        }
        public ActionResult Update_Mobile(TransactionSalePosEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string sql = "";
            string dbsql = "";
            string[] dbsql1;
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

                    var EMD_NO = DB.T_CNTRL_HDR.Select(a => a.EMD_NO).Max() + 1;

                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "E", "S", VE.T_CNTRL_HDR.MNTHCD, VE.T_TXN.DOCCD, VE.T_CNTRL_HDR.DOCNO, VE.T_TXN.DOCDT.retStr(), EMD_NO.retShort(), VE.T_TXN.DOCNO, Convert.ToDouble(VE.T_TXN.DOCNO), null, null, null, VE.T_TXN.SLCD);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    //update to T_TXNMEMO//
                    sql = "update " + schnm + ". T_TXNMEMO set MOBILE='" + VE.T_TXNMEMO.MOBILE + "',EMD_NO='" + EMD_NO + "'  ";
                    sql += " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
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
        public ActionResult RowBind(TransactionSalePosEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Cn.getQueryString(VE);
            if (VE.TTXNSLSMN == null)
            {
                if (VE.TTXNSLSMN == null || VE.TTXNSLSMN.Count == 0)
                {
                    List<TTXNSLSMN> TTXNSLSMN = new List<TTXNSLSMN>();
                    for (int i = 0; i <= 2; i++)
                    {
                        TTXNSLSMN TTXNSLM = new TTXNSLSMN();
                        TTXNSLM.SLNO = Convert.ToInt16(i + 1);
                        if (i == 0) TTXNSLM.PER = 100;
                        TTXNSLSMN.Add(TTXNSLM);
                        VE.TTXNSLSMN = TTXNSLSMN;
                    }
                    VE.TTXNSLSMN = TTXNSLSMN;
                }
            }
            else if (VE.TTXNSLSMN != null && VE.TTXNSLSMN.Count > 0)
            {
                int COUNT = 0;
                List<TTXNSLSMN> TPROGDTL = new List<TTXNSLSMN>();
                for (int i = 0; i <= VE.TTXNSLSMN.Count - 1; i++)
                {
                    TTXNSLSMN MBILLDET = new TTXNSLSMN();
                    MBILLDET = VE.TTXNSLSMN[i];
                    TPROGDTL.Add(MBILLDET);
                    COUNT++;
                }
                TTXNSLSMN MBILLDET1 = new TTXNSLSMN();

                int SERIAL = Convert.ToInt32(VE.TTXNSLSMN.Max(a => Convert.ToInt32(a.SLNO)));
                for (int j = COUNT; j <= 2; j++)
                {
                    SERIAL = SERIAL + 1;
                    TTXNSLSMN OPENING_BL = new TTXNSLSMN();
                    OPENING_BL.SLNO = SERIAL.retShort();
                    TPROGDTL.Add(OPENING_BL);
                }

                VE.TTXNSLSMN = TPROGDTL;
            }
            VE.DefaultView = true;
            return PartialView("_T_SALE_POS_SALESMAN", VE);
        }
        public ActionResult Update_SlnmDet(TransactionSalePosEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string sql = "";
            string dbsql = "";
            string[] dbsql1;
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
                    var schnmF = CommVar.FinSchema(UNQSNO);
                    var CLCD = CommVar.ClientCode(UNQSNO);

                    if (VE.TTXNSLSMN != null && VE.TTXNSLSMN.Count > 0)
                    {

                        var EMD_NO = DB.T_CNTRL_HDR.Select(a => a.EMD_NO).Max() + 1;
                        dbsql = masterHelp.TblUpdt("t_txnslsmn", VE.T_TXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }





                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "E", "S", VE.T_CNTRL_HDR.MNTHCD, VE.T_TXN.DOCCD, VE.T_CNTRL_HDR.DOCNO, VE.T_TXN.DOCDT.retStr(), EMD_NO.retShort(), VE.T_TXN.DOCNO, Convert.ToDouble(VE.T_TXN.DOCNO), null, null, null, VE.T_TXN.SLCD);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();



                        for (int i = 0; i <= VE.TTXNSLSMN.Count - 1; i++)
                        {
                            if (VE.TTXNSLSMN[i].SLNO != 0 && VE.TTXNSLSMN[i].SLMSLCD != null)
                            {
                                sql = "insert into " + scm1 + ".T_TXNSLSMN (clcd, autono, EMD_NO, DTAG, slno, SLMSLCD, PER, ITAMT, BLAMT) values ('" + CommVar.ClientCode(UNQSNO) + "','" + VE.T_TXN.AUTONO + "','" + EMD_NO + "','E','" + VE.TTXNSLSMN[i].SLNO + "','" + VE.TTXNSLSMN[i].SLMSLCD + "','" + VE.TTXNSLSMN[i].PER + "','" + VE.TTXNSLSMN[i].ITAMT + "','" + VE.TTXNSLSMN[i].BLAMT + "')";

                                OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                            }
                        }

                        sql = "update " + schnmF + ". t_vch_bl set AGSLCD='" + VE.TTXNSLSMN[0].SLMSLCD + "' "
                               + " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
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
        public ActionResult GetOrderAdvance(TransactionSalePosEntry VE)
        {
            try
            {
                DataTable PendingPOAdvancetbl = salesfunc.getPendingPOAdvance("", VE.T_TXN.DOCDT.retDateStr(), "", "", VE.T_TXN.AUTONO.retSqlformat());
                VE.PopupAdvanceList = (from DataRow DR in PendingPOAdvancetbl.Rows
                                       group DR by new { tchdocno = DR["tchdocno"].ToString(), docdt = DR["docdt"].ToString(), autono = DR["autono"].ToString(), slcd = DR["slcd"].ToString(), SLNM = DR["SLNM"].ToString(), } into X
                                       select new AdvanceList()
                                       {
                                           ADVDOCNO = X.Key.tchdocno,
                                           ADVDOCDT = X.Key.docdt,
                                           ADVAUTONO = X.Key.autono,
                                           SLCD = X.Key.slcd,
                                           SLNM = X.Key.SLNM,
                                           BALADVAMT = X.Sum(Z => Z.Field<decimal>("balamt")).retDbl(),
                                           ADVAMT = X.Sum(Z => Z.Field<decimal>("ADVAMT")).retDbl(),
                                           PRVADJAMT = X.Sum(Z => Z.Field<decimal>("ADJAMT")).retDbl(),
                                       }).ToList();
                if (VE.PopupAdvanceList != null && VE.PopupAdvanceList.Count > 0)
                {
                    if (VE.FROMDT.retStr() != "" || VE.TODT.retStr() != "")
                    {
                        if (VE.FROMDT.retStr() != "") VE.PopupAdvanceList = (from a in VE.PopupAdvanceList where Convert.ToDateTime(a.ADVDOCDT) >= VE.FROMDT select a).ToList();
                        if (VE.TODT.retStr() != "") VE.PopupAdvanceList = (from a in VE.PopupAdvanceList where Convert.ToDateTime(a.ADVDOCDT) <= VE.TODT select a).ToList();
                    }
                }
                if (VE.AdvanceList != null)
                {//checked when opend secone times.
                    var selectedbillautoslno = VE.AdvanceList.Select(e => e.ADVAUTONO).Distinct().ToList();
                    VE.PopupAdvanceList.Where(x => selectedbillautoslno.Contains(x.ADVAUTONO)).ForEach(a => a.Checked = true);
                }
                for (int p = 0; p <= VE.PopupAdvanceList.Count - 1; p++)
                {
                    VE.PopupAdvanceList[p].SLNO = Convert.ToInt16(p + 1);
                }

                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_POS_OrderAdvancePopup", VE);

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SelectOrderAdvance(TransactionSalePosEntry VE)
        {
            try
            {
                VE.AdvanceList = (from a in VE.PopupAdvanceList
                                  where a.Checked == true
                                  select new AdvanceList()
                                  {
                                      ADVDOCNO = a.ADVDOCNO,
                                      ADVDOCDT = a.ADVDOCDT,
                                      ADVAUTONO = a.ADVAUTONO,
                                      ADVAMT = a.ADVAMT,
                                      BALADVAMT = a.BALADVAMT,
                                      PRVADJAMT = a.PRVADJAMT,
                                      ADJAMT = a.BALADVAMT
                                  }).ToList();

                for (int p = 0; p <= VE.AdvanceList.Count - 1; p++)
                {
                    VE.AdvanceList[p].SLNO = Convert.ToInt16(p + 1);
                }
                string str = "";
                if (VE.AdvanceList != null && VE.AdvanceList.Count > 0)
                {
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.SaleSchema(UNQSNO));

                    string ADVAUTONO = VE.AdvanceList.Select(a => a.ADVAUTONO).FirstOrDefault();
                    var T_TXNMEMO = (from a in DB.T_TXNMEMO where a.AUTONO == ADVAUTONO select new { a.NM, a.MOBILE, a.ADDR, a.CITY }).ToList();
                    if (T_TXNMEMO != null && T_TXNMEMO.Count > 0)
                    {
                        str = masterHelp.ToReturnFieldValues(T_TXNMEMO);
                    }
                }
                ModelState.Clear();
                VE.DefaultView = true;
                var WORKNO_GRID = RenderRazorViewToString(ControllerContext, "_T_SALE_POS_OrderAdvance", VE);
                return Content(WORKNO_GRID + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + str);
                //return PartialView("_T_Sale_OrderAdvance", VE);

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
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

        public string GetLastTempdataName(string tempdataname, string doccd = "")
        {
            TransactionSaleEntry VE = new TransactionSaleEntry();
            Cn.getQueryString(VE);
            string LOC = CommVar.Loccd(UNQSNO);
            string COM = CommVar.Compcd(UNQSNO);
            //tempdataname = tempdataname + VE.MENU_PARA + COM + LOC;
            tempdataname = tempdataname + VE.MENU_PARA + COM + LOC + doccd;
            return tempdataname;
        }

    }
}