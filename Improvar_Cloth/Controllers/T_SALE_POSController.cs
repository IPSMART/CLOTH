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

namespace Improvar.Controllers
{
    public class T_SALE_POSController : Controller
    {
        // GET: T_SALE_POS_POS
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp(); Cloth cloth = new Cloth();
        Salesfunc salesfunc = new Salesfunc(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
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
                        default: ViewBag.formname = ""; break;
                    }
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    VE.DropDown_list_StkType = masterHelp.STK_TYPE();
                    //VE.DISC_TYPE = masterHelp.DISC_TYPE();
                    VE.BARGEN_TYPE = masterHelp.BARGEN_TYPE();
                    VE.INVTYPE_list = masterHelp.INVTYPE_list();
                    //VE.EXPCD_list = masterHelp.EXPCD_list(VE.MENU_PARA == "PB" ? "P" : "S");
                    VE.Reverse_Charge = masterHelp.REV_CHRG();
                    VE.Database_Combo1 = (from n in DB.T_TXNOTH
                                          select new Database_Combo1() { FIELD_VALUE = n.SELBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.Database_Combo2 = (from n in DB.T_TXNOTH
                                          select new Database_Combo2() { FIELD_VALUE = n.DEALBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.Database_Combo3 = (from n in DB.T_TXNOTH
                                          select new Database_Combo3() { FIELD_VALUE = n.PACKBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.HSN_CODE = (from n in DBF.M_HSNCODE
                                   select new HSN_CODE() { text = n.HSNDESCN, value = n.HSNCODE }).OrderBy(s => s.text).Distinct().ToList();

                    VE.DropDown_list_MTRLJOBCD = (from i in DB.M_MTRLJOBMST select new DropDown_list_MTRLJOBCD() { MTRLJOBCD = i.MTRLJOBCD, MTRLJOBNM = i.MTRLJOBNM }).OrderBy(s => s.MTRLJOBNM).ToList();
                    foreach (var v in VE.DropDown_list_MTRLJOBCD)
                    {
                        if (v.MTRLJOBCD == "FS") { v.Checked = true; }
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

                                List<TsalePos_TBATCHDTL> sd = new List<TsalePos_TBATCHDTL>(); sd.Add(new TsalePos_TBATCHDTL() { SLNO = 1, DISC_TYPE = masterHelp.DISC_TYPE(), PCSection = masterHelp.PCSAction(), });
                                VE.TsalePos_TBATCHDTL = sd;
                                T_TXN TTXN = new T_TXN();
                                TTXN.DOCDT = Cn.getCurrentDate(VE.mindate);
                                TTXN.GOCD = TempData["LASTGOCD" + VE.MENU_PARA].retStr();
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
                                    VE.GONM = DB.M_GODOWN.Where(a => a.GOCD == gocd).Select(b => b.GONM).FirstOrDefault();
                                }
                                T_TXNOTH TXNOTH = new T_TXNOTH(); T_TXNMEMO TXNMEMO = new T_TXNMEMO();
                                string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);
                                string sql = "";
                                sql += " select a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate,C.TAXGRPCD";
                                sql += "  from  " + scm + ".M_SYSCNFG a, " + scmf + ".M_RETDEB b, " + scm + ".M_SUBLEG_SDDTL c";
                                sql += " where a.RTDEBCD=b.RTDEBCD and a.effdt in(select max(effdt) effdt from  " + scm + ".M_SYSCNFG) and a.retdebslcd=C.SLCD";

                                DataTable syscnfgdt = masterHelp.SQLquery(sql);
                                if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                                {
                                    TXNMEMO.RTDEBCD = syscnfgdt.Rows[0]["RTDEBCD"].retStr();
                                    VE.RTDEBNM = syscnfgdt.Rows[0]["RTDEBNM"].retStr();
                                    VE.MOBILE = syscnfgdt.Rows[0]["MOBILE"].retStr();
                                    VE.INC_RATE = syscnfgdt.Rows[0]["INC_RATE"].retStr() == "Y" ? true : false;
                                    VE.INCLRATEASK = syscnfgdt.Rows[0]["INC_RATE"].retStr();
                                    TXNOTH.TAXGRPCD = syscnfgdt.Rows[0]["TAXGRPCD"].retStr();
                                    TXNOTH.PRCCD = "RP"; VE.PRCNM = "Retail Price";
                                }
                                VE.T_TXNMEMO = TXNMEMO;
                                VE.T_TXNOTH = TXNOTH;
                                VE.T_TXN = TTXN;

                                List<TTXNSLSMN> TTXNSLSMN = new List<TTXNSLSMN>();
                                for (int i = 0; i <= 2; i++)
                                {
                                    TTXNSLSMN TTXNSLM = new TTXNSLSMN();
                                    TTXNSLM.SLNO = Convert.ToByte(i + 1);
                                    if (i == 0) TTXNSLM.PER = 100;
                                    TTXNSLSMN.Add(TTXNSLM);
                                    VE.TTXNSLSMN = TTXNSLSMN;
                                }
                                VE.TTXNSLSMN = TTXNSLSMN;
                                var pymtcd = DB.M_PAYMENT.Select(b => b.PYMTCD).Max();
                                if (pymtcd != null)
                                {
                                    var MPAYMENT = (from i in DB.M_PAYMENT where i.PYMTCD == pymtcd select new { i.PYMTCD, i.PYMTNM, i.GLCD }).ToList();

                                    if (MPAYMENT.Count > 0)
                                    {
                                        VE.TTXNPYMT = (from i in MPAYMENT select new TTXNPYMT { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD }).ToList();
                                    }
                                    //else if (TTXNPAYMT.Count > 0)
                                    //{ VE.TTXNPYMT = (from i in TTXNPAYMT select new TTXNPYMT { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD, INSTNO = i.INSTNO, PYMTREM = i.PYMTREM, CARDNO = i.CARDNO, AMT = i.AMT, INSTDT = i.INSTDT.retDateStr() }).ToList(); }
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
                                    TXNPYMT.SLNO = Convert.ToByte(slno + 1);
                                    TTXNPYMNT.Add(TXNPYMT);
                                    VE.TTXNPYMT = TTXNPYMNT;
                                }

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
                        if (parkID == "" && loadOrder == "N")
                        {
                            FreightCharges(VE, VE.T_TXN?.AUTONO);
                        }
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (VE.T_CNTRL_HDR != null) if (VE.T_CNTRL_HDR.DOCDT != null) docdt = VE.T_CNTRL_HDR.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_TXN?.DOCCD, docdt, VE.DefaultAction, VE.T_TXN?.DOCNO, VE);
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
            VE = cloth.T_SALE_POS_Navigation(VE, DB, index, searchValue, loadOrder);
            return VE;
        }
        public ActionResult SearchPannelData(TransactionSalePosEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
                Cn.getQueryString(VE);
                List<DocumentType> DocumentType = new List<DocumentType>();
                DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
                string sql = "";

                sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.slcd, c.slnm, c.district, nvl(a.blamt,0) blamt ";
                sql += "from " + scm + ".t_txn a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_subleg c ";
                sql += "where a.autono=b.autono and a.slcd=c.slcd(+) and b.doccd in (" + doccd + ") and ";
                if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
                if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
                if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
                if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
                sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
                sql += "order by docdt, docno ";
                DataTable tbl = masterHelp.SQLquery(sql);

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Bill Amt" + Cn.GCS() + "AUTO NO";
                for (int j = 0; j <= tbl.Rows.Count - 1; j++)
                {
                    SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td class='text-right'>" + Convert.ToDouble(tbl.Rows[j]["blamt"]).ToINRFormat() + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
                }
                return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
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
        public ActionResult GetRefRetailDetails(string val)
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
                            ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, RATE);
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
                string str = masterHelp.T_TXN_BARNO_help(barnoOrStyle, VE.MENU_PARA, DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    //if (str.IndexOf(Cn.GCS()) == -1)
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
                                  x.ALL_GSTPER,
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
                                  ALL_GSTPER = P.Key.ALL_GSTPER.retStr() == "" ? P.Key.GSTPER.retStr() : P.Key.ALL_GSTPER,
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
                VE.T_GROSS_AMT = VE.TTXNDTL.Select(a => a.TXBLVAL).Sum().retDbl();
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
                sql += "a.amtdesc, b.glcd, b.hsncode, a.amtrate, a.curr_amt, a.amt,a.igstper, a.igstamt, ";
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
                    VE.TTXNAMT[p].IGSTPER = IGST_PER;
                    VE.TTXNAMT[p].CGSTPER = CGST_PER;
                    VE.TTXNAMT[p].SGSTPER = SGST_PER;
                    VE.TTXNAMT[p].CESSPER = CESS_PER;
                    A_T_CURR_AMT = A_T_CURR_AMT + VE.TTXNAMT[p].CURR_AMT.Value;
                    A_T_AMT = A_T_AMT + VE.TTXNAMT[p].AMT.Value;
                    A_T_IGST_AMT = A_T_IGST_AMT + VE.TTXNAMT[p].IGSTAMT.Value;
                    A_T_CGST_AMT = A_T_CGST_AMT + VE.TTXNAMT[p].CGSTAMT.Value;
                    A_T_SGST_AMT = A_T_SGST_AMT + VE.TTXNAMT[p].SGSTAMT.Value;
                    A_T_CESS_AMT = A_T_CESS_AMT + VE.TTXNAMT[p].CESSAMT.Value;
                    A_T_DUTY_AMT = A_T_DUTY_AMT + VE.TTXNAMT[p].DUTYAMT.Value;
                    A_T_NET_AMT = A_T_NET_AMT + A_T_AMT + A_T_IGST_AMT + A_T_CGST_AMT + A_T_SGST_AMT + A_T_CESS_AMT + A_T_DUTY_AMT;
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
        public ActionResult DeleteRowBarno(TransactionSalePosEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                List<TsalePos_TBATCHDTL> TsalePos_TBATCHDTL = new List<TsalePos_TBATCHDTL>();
                int count = 0;
                for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                {
                    if (VE.TsalePos_TBATCHDTL[i].Checked == false)
                    {
                        count += 1;
                        TsalePos_TBATCHDTL item = new TsalePos_TBATCHDTL();
                        item = VE.TsalePos_TBATCHDTL[i];
                        if (VE.DefaultAction == "A")
                        {
                            item.SLNO = Convert.ToSByte(count);
                        }

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
        public ActionResult DeleteRow(TransactionSalePosEntry VE)
        {
            try
            {
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
        public ActionResult SAVE(FormCollection FC, TransactionSalePosEntry VE)
        {
            string ContentFlg = cloth.T_SALE_POS_Save(FC, VE);
            ModelState.Clear();
            return Content(ContentFlg);
        }
        public ActionResult Print(TransactionSalePosEntry VE, FormCollection FC, string DOCNO, string DOC_CD, string DOCDT)
        {
            try
            {
                ReportViewinHtml ind = new ReportViewinHtml();
                ind.DOCCD = DOC_CD;
                ind.FDOCNO = DOCNO;
                ind.TDOCNO = DOCNO;
                ind.FDT = DOCDT;
                ind.TDT = DOCDT;
                ind.MENU_PARA = "SALES";
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
                return Content("/UploadDocuments/" + filename);
            }
            catch (Exception ex)
            {
                return Content("//.");
            }
        }
    }
}