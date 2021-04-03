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

namespace Improvar.Controllers
{
    public class T_SALEController : Controller
    {
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_TXN TXN; T_TXNTRANS TXNTRN; T_TXNOTH TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR; T_TXN_LINKNO TTXNLINKNO; T_TXNEINV TTXNEINV;
        SMS SMS = new SMS(); string sql = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
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
                    ViewBag.formname = MenuDescription(VE.MENU_PARA).Rows[0]["FORMNAME"].ToString();
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);

                    VE.BL_TYPE = masterHelp.BL_TYPE();
                    VE.DropDown_list_StkType = masterHelp.STK_TYPE();
                    VE.DISC_TYPE = masterHelp.DISC_TYPE();
                    VE.DISC_TYPE1 = masterHelp.DISC_TYPE1();
                    VE.BARGEN_TYPE = masterHelp.BARGEN_TYPE();
                    VE.INVTYPE_list = masterHelp.INVTYPE_list();
                    VE.EXPCD_list = masterHelp.EXPCD_list((VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? "P" : "S");
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
                            if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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

                        VE.IndexKey = (from p in DB.T_TXN
                                       join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       orderby p.DOCDT, p.DOCNO
                                       where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YR1
                                       select new IndexKey() { Navikey = p.AUTONO }).ToList();
                        if (VE.MENU_PARA == "SB" && op == "V" && searchValue != "")
                        {
                            var chk_autono = VE.IndexKey.Where(a => a.Navikey == searchValue).ToList();
                            if (chk_autono.Count == 0)
                            {
                                searchValue = "";
                            }
                        }

                        //var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                        var MSYSCNFG = salesfunc.M_SYSCNFG();
                        if (MSYSCNFG == null)
                        {

                            VE.DefaultView = false;
                            VE.DefaultDay = 0;
                            ViewBag.ErrorMessage = "Data add in Configuaration Setup->Posting/Terms Setup";
                            return View(VE);
                        }
                        VE.M_SYSCNFG = MSYSCNFG;

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
                            VE.T_TXN = TXN;
                            VE.T_TXNTRANS = TXNTRN;
                            VE.T_TXNOTH = TXNOTH;
                            VE.T_CNTRL_HDR = TCH;
                            VE.T_CNTRL_HDR_REM = SLR;
                            VE.T_TXN_LINKNO = TTXNLINKNO;
                            VE.T_TXNEINV = TTXNEINV;
                            if (loadOrder.retStr().Length > 1)
                            {
                                VE.T_TXN.AUTONO = "";
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
                                TTXN.DOCDT = Cn.getCurrentDate(VE.mindate);
                                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                                {
                                    TTXN.PREFDT = Cn.getCurrentDate(VE.mindate);
                                }
                                TTXN.GOCD = TempData["LASTGOCD" + VE.MENU_PARA].retStr();
                                string ROUNDOFF = TempData["LASTROUNDOFF" + VE.MENU_PARA].retStr();
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
                                if (VE.MENU_PARA == "PJBL")
                                {
                                    string menudoccd = VE.DOC_CODE;
                                    var M_JOBMST = (from a in DB.M_JOBMST where a.FLAG1 == menudoccd select new { a.JOBCD, a.JOBNM, a.HSNCODE, a.EXPGLCD }).FirstOrDefault();
                                    if (M_JOBMST != null)
                                    {
                                        TXN.JOBCD = M_JOBMST.JOBCD;
                                        VE.JOBNM = M_JOBMST.JOBNM;
                                        VE.JOBHSNCODE = M_JOBMST.HSNCODE;
                                        VE.JOBEXPGLCD = M_JOBMST.EXPGLCD;
                                    }
                                }
                                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR")
                                {
                                    TTXN.BARGENTYPE = VE.M_SYSCNFG.COMMONUNIQBAR.retStr() == "E" ? "E" : "C";
                                }
                                VE.T_TXN = TTXN;

                                T_TXNOTH TXNOTH = new T_TXNOTH(); VE.T_TXNOTH = TXNOTH;

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

                        var mtrljobcd = (from a in DB.M_MTRLJOBMST
                                         join b in DB.M_CNTRL_HDR on a.M_AUTONO equals b.M_AUTONO
                                         where b.INACTIVE_TAG == "N"
                                         select new { a.MTRLJOBCD, a.MTRLJOBNM, a.MTBARCODE }).ToList();
                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (TCH != null) if (TCH.DOCDT != null) docdt = TCH.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_TXN.DOCCD, docdt, VE.DefaultAction, VE.T_TXN.DOCNO, VE);
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
            var dr = dt.Select("MENUPARA='" + MENU_PARA + "'");
            if (dr != null && dr.Count() > 0) return dr.CopyToDataTable(); else return dt;
        }
        public TransactionSaleEntry Navigation(TransactionSaleEntry VE, ImprovarDB DB, int index, string searchValue, string loadOrder = "N")
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE);

            TXN = new T_TXN(); TXNTRN = new T_TXNTRANS(); TXNOTH = new T_TXNOTH(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM(); TTXNLINKNO = new T_TXN_LINKNO(); TTXNEINV = new T_TXNEINV();

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
                TXNTRN = DB.T_TXNTRANS.Find(TXN.AUTONO);
                TXNOTH = DB.T_TXNOTH.Find(TXN.AUTONO);
                TTXNEINV = DBF.T_TXNEINV.Find(TXN.AUTONO);
                if (VE.MENU_PARA == "SB" && loadOrder == "N")
                {
                    TTXNLINKNO = (from a in DB.T_TXN_LINKNO where a.AUTONO == TXN.AUTONO select a).FirstOrDefault();
                    if (TTXNLINKNO != null)
                    {
                        VE.LINKDOCNO = (from a in DB.T_CNTRL_HDR where a.AUTONO == TTXNLINKNO.LINKAUTONO select a).FirstOrDefault().DOCNO;
                    }
                }
                string panno = "";
                if (TXN.SLCD.retStr() != "")
                {
                    string slcd = TXN.SLCD;
                    var subleg = (from a in DBF.M_SUBLEG where a.SLCD == slcd select new { a.SLNM, a.SLAREA, a.DISTRICT, a.GSTNO, a.PSLCD, a.TCSAPPL, a.PANNO, a.PARTYCD }).FirstOrDefault();
                    VE.SLNM = subleg.SLNM;
                    VE.SLAREA = subleg.SLAREA == "" ? subleg.DISTRICT : subleg.SLAREA;
                    VE.GSTNO = subleg.GSTNO;
                    VE.PSLCD = subleg.PSLCD;
                    VE.PARTYCD = subleg.PARTYCD;
                    VE.TCSAPPL = subleg.TCSAPPL;
                    if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") VE.TCSAPPL = "N";
                    VE.TCSAUTOCAL = VE.TCSAPPL.retStr() == "Y" ? true : false;
                    panno = subleg.PANNO;

                    string DOCTAG = MenuDescription(VE.MENU_PARA).Rows[0]["DOCTAG"].ToString().retSqlformat();
                    var party_data = salesfunc.GetSlcdDetails(TXN.SLCD.retStr(), TXN.DOCDT.retStr().Remove(10), "", DOCTAG);
                    if (party_data != null && party_data.Rows.Count > 0)
                    {
                        string scmdisctype = party_data.Rows[0]["scmdisctype"].retStr() == "P" ? "%" : party_data.Rows[0]["scmdisctype"].retStr() == "N" ? "Nos" : party_data.Rows[0]["scmdisctype"].retStr() == "Q" ? "Qnty" : party_data.Rows[0]["scmdisctype"].retStr() == "F" ? "Fixed" : "";
                        VE.SLDISCDESC = (party_data.Rows[0]["scmdiscrate"].retStr() + " " + scmdisctype + " " + (party_data.Rows[0]["lastbldt"].retStr() == "" ? "" : party_data.Rows[0]["lastbldt"].retStr().Remove(10)));
                    }
                }

                VE.CONSLNM = TXN.CONSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXN.CONSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.AGSLNM = TXNOTH.AGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.AGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.SAGSLNM = TXNOTH.SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.SAGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.GONM = TXN.GOCD.retStr() == "" ? "" : DBF.M_GODOWN.Where(a => a.GOCD == TXN.GOCD).Select(b => b.GONM).FirstOrDefault();
                VE.PRCNM = TXNOTH.PRCCD.retStr() == "" ? "" : DBF.M_PRCLST.Where(a => a.PRCCD == TXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();

                VE.TRANSLNM = TXNTRN.TRANSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.TRANSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.MUTSLNM = TXNOTH.MUTSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.MUTSLCD).Select(b => b.SLNM).FirstOrDefault();
                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);

                //var party_data = salesfunc.GetSlcdDetails(TXN.SLCD.retStr(), TXN.DOCDT.retStr().Remove(10));
                //if (party_data != null && party_data.Rows.Count > 0)
                //{
                //    TXNOTH.TAXGRPCD = party_data.Rows[0]["TAXGRPCD"].retStr();
                //}
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
                VE.AMT = VE.AMT.retDbl() > VE.TDSLIMIT.retDbl() ? VE.TDSLIMIT.retDbl() : VE.AMT.retDbl();
                if (TXN.TDSCODE.retStr() != "")
                {
                    VE.TDSNM = DBF.M_TDS_CNTRL.Where(e => e.TDSCODE == TXN.TDSCODE).FirstOrDefault()?.TDSNM;
                }
                //
                if (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN")
                {
                    VE.EXPGLCD = DBF.T_VCH_GST.Where(a => a.AUTONO == TXN.AUTONO).Select(b => b.EXPGLCD).FirstOrDefault();
                    VE.EXPGLNM = VE.EXPGLCD.retStr() == "" ? "" : DBF.M_GENLEG.Where(a => a.GLCD == VE.EXPGLCD).Select(b => b.GLNM).FirstOrDefault();
                }
                if (VE.MENU_PARA == "PJBL" && TXN.JOBCD.retStr() != "")
                {
                    var jobdata = (from a in DB.M_JOBMST where a.JOBCD == TXN.JOBCD select new { a.JOBNM, a.EXPGLCD, a.HSNCODE }).FirstOrDefault();
                    if (jobdata != null)
                    {
                        VE.JOBNM = jobdata.JOBNM;
                        VE.JOBEXPGLCD = jobdata.EXPGLCD;
                        VE.JOBHSNCODE = jobdata.HSNCODE;
                    }
                }
                string Scm = CommVar.CurSchema(UNQSNO);
                string str1 = "";
                str1 += "select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, ";
                str1 += "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE, ";
                str1 += "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,i.ORDAUTONO,i.ORDSLNO,r.DOCNO ORDDOCNO,r.DOCDT ORDDOCDT,n.RPPRICEGEN, ";
                str1 += "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,nvl(k.NEGSTOCK,n.NEGSTOCK)NEGSTOCK ";
                str1 += ",s.AGDOCNO,s.AGDOCDT,s.PAGENO,s.PAGESLNO,i.PCSTYPE,s.glcd,s.BLUOMCD,j.COMMONUNIQBAR,j.FABITCD,t.ITNM FABITNM,n.WPPER,n.RPPER,i.RECPROGSLNO ";
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".M_STKTYPE q," + Scm + ".T_CNTRL_HDR r ";
                str1 += "," + Scm + ".T_TXNDTL s, " + Scm + ".M_SITEM t ";
                str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) and i.ORDAUTONO=r.AUTONO(+) and j.fabitcd=t.itcd(+) ";
                str1 += "and i.autono=s.autono and i.txnslno=s.slno ";
                str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ";
                str1 += "order by i.SLNO ";
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
                                    WPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPRATE"].retDbl() : (double?)null,
                                    RPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPRATE"].retDbl() : (double?)null,
                                    ITREM = dr["ITREM"].retStr(),
                                    ORDAUTONO = dr["ORDAUTONO"].retStr(),
                                    ORDSLNO = dr["ORDSLNO"].retStr() == "" ? (short?)null : dr["ORDSLNO"].retShort(),
                                    ORDDOCNO = dr["ORDDOCNO"].retStr(),
                                    ORDDOCDT = dr["ORDDOCDT"].retStr() == "" ? "" : dr["ORDDOCDT"].retStr().Remove(10),
                                    WPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPPRICEGEN"].retStr() : "",
                                    RPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPPRICEGEN"].retStr() : "",
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
                                    RECPROGSLNO = dr["RECPROGSLNO"].retShort(),
                                }).OrderBy(s => s.SLNO).ToList();

                str1 = "";
                str1 += "select i.SLNO,j.ITGRPCD,k.ITGRPNM,i.MTRLJOBCD,l.MTRLJOBNM,l.MTBARCODE,i.ITCD,j.ITNM,j.STYLENO,j.UOMCD,i.STKTYPE,m.STKNAME,i.NOS,i.QNTY,i.FLAGMTR, ";
                str1 += "i.BLQNTY,i.RATE,i.AMT,i.DISCTYPE,i.DISCRATE,i.DISCAMT,i.TDDISCTYPE,i.TDDISCRATE,i.TDDISCAMT,i.SCMDISCTYPE,i.SCMDISCRATE,i.SCMDISCAMT, ";
                str1 += "i.TXBLVAL,i.IGSTPER,i.CGSTPER,i.SGSTPER,i.CESSPER,i.IGSTAMT,i.CGSTAMT,i.SGSTAMT,i.CESSAMT,i.NETAMT,i.HSNCODE,i.BALENO,i.GLCD,i.BALEYR,i.TOTDISCAMT, ";
                str1 += "i.ITREM,i.AGDOCNO,i.AGDOCDT,i.LISTPRICE,i.LISTDISCPER,i.PAGENO,i.PAGESLNO,i.BLUOMCD ";
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
                                  BLUOMCD = dr["BLUOMCD"].retStr(),
                              }).OrderBy(s => s.SLNO).ToList();

                VE.B_T_QNTY = VE.TBATCHDTL.Sum(a => a.QNTY).retDbl();
                VE.B_T_NOS = VE.TBATCHDTL.Sum(a => a.NOS).retDbl();
                VE.T_NOS = VE.TTXNDTL.Sum(a => a.NOS).retDbl();
                VE.T_QNTY = VE.TTXNDTL.Sum(a => a.QNTY).retDbl();
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


                //fill prodgrpgstper in t_batchdtl
                DataTable allprodgrpgstper_data = new DataTable();
                string BARNO = (from a in VE.TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
                string ITCD = (from a in VE.TBATCHDTL select a.ITCD).ToArray().retSqlfromStrarray();
                //string MTRLJOBCD = (from a in VE.TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                string ITGRPCD = (from a in VE.TBATCHDTL select a.ITGRPCD).ToArray().retSqlfromStrarray();
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                {
                    allprodgrpgstper_data = salesfunc.GetBarHelp(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retStr(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA, "", "", false, false, true, "", false);

                }
                else
                {
                    allprodgrpgstper_data = salesfunc.GetStock(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", TXN.AUTONO.retSqlformat(), ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);
                }

                //var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                var MSYSCNFG = salesfunc.M_SYSCNFG();
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
                                    ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl());
                                    if (ALL_GSTPER.retStr() != "")
                                    {
                                        var gst = ALL_GSTPER.Split(',').ToList();
                                        GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                                    }
                                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                                    v.ALL_GSTPER = ALL_GSTPER;
                                    v.GSTPER = GSTPER.retDbl();
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

                    //checking childdata exist against barno
                    //var chk_child = (from a in DB.T_BATCHDTL where a.BARNO == v.BARNO && a.AUTONO != TXN.AUTONO select a).ToList();
                    //if (chk_child.Count() > 0)
                    //{
                    //    v.ChildData = "Y";
                    //}

                    var chk_child = ChildRecordCheck(TXN.AUTONO);  //modify by mithun
                    if (chk_child != "")
                    {
                        v.ChildData = "Y";
                    }


                    //if ((VE.MENU_PARA == "PB") && ((TXN.BARGENTYPE == "E") || (TXN.BARGENTYPE == "C" && v.BARGENTYPE == "E")))
                    //{
                    //    v.WPRATE = v.RATE * VE.WPPER;
                    //    v.RPRATE = v.RATE * VE.WPPER;
                    //}

                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {

                        if (v.WPPRICEGEN.retStr() == "" || v.RPPRICEGEN.retStr() == "" || v.WPPER.retDbl() == 0 || v.RPPER.retDbl() == 0)
                        {
                            DataTable syscnfgdata = salesfunc.GetSyscnfgData(TXN.DOCDT.retDateStr());

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
                //fill prodgrpgstper in t_txndtl
                double IGST_PER = 0; double CGST_PER = 0; double SGST_PER = 0; double CESS_PER = 0; double DUTY_PER = 0;
                foreach (var v in VE.TTXNDTL)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "";
                    var tax_data = (from a in VE.TBATCHDTL
                                    where a.TXNSLNO == v.SLNO
                                    select new { a.PRODGRPGSTPER, a.ALL_GSTPER, a.FABITCD, a.FABITNM, a.RATE, a.RECPROGSLNO }).FirstOrDefault();
                    if (tax_data != null)
                    {
                        PRODGRPGSTPER = tax_data.PRODGRPGSTPER.retStr();
                        ALL_GSTPER = tax_data.ALL_GSTPER.retStr();
                        v.FABITCD = tax_data.FABITCD.retStr();
                        v.FABITNM = tax_data.FABITNM.retStr();
                        v.RECPROGSLNO = tax_data.RECPROGSLNO.retShort();
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
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") { S_P = "P"; } else { S_P = "S"; }
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

                    A_TOTAL_CURR = A_TOTAL_CURR + VE.TTXNAMT[p].CURR_AMT.Value;
                    A_TOTAL_AMOUNT = A_TOTAL_AMOUNT + VE.TTXNAMT[p].AMT.Value;
                    A_IGST_AMT = A_IGST_AMT + VE.TTXNAMT[p].IGSTAMT.Value;
                    A_CGST_AMT = A_CGST_AMT + VE.TTXNAMT[p].CGSTAMT.Value;
                    A_SGST_AMT = A_SGST_AMT + VE.TTXNAMT[p].SGSTAMT.Value;
                    A_CESS_AMT = A_CESS_AMT + VE.TTXNAMT[p].CESSAMT.Value;
                    A_DUTY_AMT = A_DUTY_AMT + VE.TTXNAMT[p].DUTYAMT.Value;
                    VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT.Value + VE.TTXNAMT[p].IGSTAMT.Value + VE.TTXNAMT[p].CGSTAMT.Value + VE.TTXNAMT[p].SGSTAMT.Value + VE.TTXNAMT[p].CESSAMT.Value + VE.TTXNAMT[p].DUTYAMT.Value;
                    A_TOTAL_NETAMOUNT = A_TOTAL_NETAMOUNT + VE.TTXNAMT[p].NETAMT.Value;
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

                if (VE.MENU_PARA == "PJBL")
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
            return VE;
        }
        public ActionResult SearchPannelData(TransactionSaleEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
                Cn.getQueryString(VE);
                List<DocumentType> DocumentType = new List<DocumentType>();
                DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
                string sql = "";

                sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.slcd, c.slnm, c.district, nvl(a.blamt,0) blamt,a.PREFDT,a.PREFno ";
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
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                {
                    var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Bill Amt" + Cn.GCS() + "Pblno" + Cn.GCS() + "Pbldt" + Cn.GCS() + "AUTO NO";
                    for (int j = 0; j <= tbl.Rows.Count - 1; j++)
                    {
                        SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td class='text-right'>" + Convert.ToDouble(tbl.Rows[j]["blamt"]) + " </td><td>" + tbl.Rows[j]["PREFNO"] + " </td><td>" + tbl.Rows[j]["PREFDT"].retStr().Remove(10) + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
                    }
                    return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "6", "6"));
                }
                else
                {
                    var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Bill Amt" + Cn.GCS() + "AUTO NO";
                    for (int j = 0; j <= tbl.Rows.Count - 1; j++)
                    {
                        SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td class='text-right'>" + Convert.ToDouble(tbl.Rows[j]["blamt"]) + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
                    }
                    return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetTDSDetails(string val, string TAG, string PARTY, string AUTONO)
        {
            try
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                string linktdscode = "'Y','Z'";
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") linktdscode = "'X'";
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
        public ActionResult GetSubLedgerDetails(string val, string Code, string Autono, string linktdscode)
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
                        if (code_data[1] == "")
                        {
                            return Content("Please Select Agent !!");
                        }
                        else
                        {
                            Code = code_data[0];
                        }
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
                        string TCSPER = "", TCSCODE = "", TCSNM = "", TDSLIMIT = "", TDSCALCON = "", AMT = "", TDSROUNDCAL = "";
                        if (str.IndexOf(Cn.GCS()) > 0)
                        {
                            string DOCTAG = MenuDescription(VE.MENU_PARA).Rows[0]["DOCTAG"].ToString().retSqlformat();
                            var party_data = salesfunc.GetSlcdDetails(val, code_data[1], "", DOCTAG);
                            if (party_data != null && party_data.Rows.Count > 0)
                            {
                                if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") party_data.Rows[0]["TCSAPPL"] = "N";
                                if (party_data.Rows[0]["TCSAPPL"].retStr() == "Y")
                                {
                                    linktdscode = linktdscode.retStr() == "" ? "'Y'" : linktdscode.retStr().retSqlformat();

                                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") linktdscode = "'X'";
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

                                string scmdisctype = party_data.Rows[0]["scmdisctype"].retStr() == "P" ? "%" : party_data.Rows[0]["scmdisctype"].retStr() == "N" ? "Nos" : party_data.Rows[0]["scmdisctype"].retStr() == "Q" ? "Qnty" : party_data.Rows[0]["scmdisctype"].retStr() == "F" ? "Fixed" : "";
                                str += "^" + "SLDISCDESC" + "=^" + (party_data.Rows[0]["scmdiscrate"].retStr() + " " + scmdisctype + " " + (party_data.Rows[0]["lastbldt"].retStr() == "" ? "" : party_data.Rows[0]["lastbldt"].retStr().Remove(10))) + Cn.GCS();
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
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
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
                if(data[7].retStr() == "Bar")
                {
                    barnoOrStyle = barnoOrStyle.ToUpper();
                }
                string str = masterHelp.T_TXN_BARNO_help(barnoOrStyle, VE.MENU_PARA, DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD, "", exactbarno, "", BARNO, AUTONO);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1) return Content(str);
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
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
                var str = masterHelp.BaleNo_help(val, tdt, gocd, "", true, true);
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
        public ActionResult GetGeneralLedgerDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.GENERALLEDGER(val));
                }
                else
                {
                    string str = masterHelp.GENERALLEDGER(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetJobDetails(string val)
        {
            try
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                var str = masterHelp.JOBCD_JOBMST_help(val, VE.DOC_CODE.retStr().retSqlformat());
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
                    x.PDESIGN = x.PDESIGN.retStr();
                    x.RECPROGSLNO = x.RECPROGSLNO.retShort();
                });
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
                                  x.PDESIGN,
                                  x.RECPROGSLNO,
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
                                  //AMT = P.Sum(A => A.BLQNTY).retDbl() == 0 ? (P.Sum(A => A.QNTY).retDbl() - P.Sum(A => A.FLAGMTR).retDbl()) * P.Key.RATE.retDbl() : P.Sum(A => A.BLQNTY).retDbl() * P.Key.RATE.retDbl(),
                                  HSNCODE = P.Key.HSNCODE,
                                  PRODGRPGSTPER = P.Key.PRODGRPGSTPER,
                                  BALENO = P.Key.BALENO,
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
                              }).OrderBy(a => a.SLNO).ToList();
                //chk duplicate slno
                var allslno = VE.TTXNDTL.Select(a => a.SLNO).Count();
                var distnctslno = VE.TTXNDTL.Select(a => a.SLNO).Distinct().Count();
                if (allslno != distnctslno)
                {
                    #region error checking
                    //DataTable tbl = ListToDatatable.LINQResultToDataTable(VE.TTXNDTL);
                    #endregion

                    string slno = string.Join(",", (VE.TTXNDTL.GroupBy(x => x.SLNO)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList()));
                    return Content(slno);
                }

                for (int p = 0; p <= VE.TTXNDTL.Count - 1; p++)
                {
                    if (VE.TTXNDTL[p].PRODGRPGSTPER.retStr() != "")
                    {
                        var gstdata = salesfunc.retGstPer(VE.TTXNDTL[p].PRODGRPGSTPER.retStr(), VE.TTXNDTL[p].RATE.retDbl());
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
                }

                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_DETAIL", VE);
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
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") { S_P = "P"; } else { S_P = "S"; }

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
                    A_T_CURR_AMT = A_T_CURR_AMT + VE.TTXNAMT[p].CURR_AMT.Value;
                    A_T_AMT = A_T_AMT + VE.TTXNAMT[p].AMT.Value;
                    A_T_IGST_AMT = A_T_IGST_AMT + VE.TTXNAMT[p].IGSTAMT.Value;
                    A_T_CGST_AMT = A_T_CGST_AMT + VE.TTXNAMT[p].CGSTAMT.Value;
                    A_T_SGST_AMT = A_T_SGST_AMT + VE.TTXNAMT[p].SGSTAMT.Value;
                    A_T_CESS_AMT = A_T_CESS_AMT + VE.TTXNAMT[p].CESSAMT.Value;
                    A_T_DUTY_AMT = A_T_DUTY_AMT + VE.TTXNAMT[p].DUTYAMT.Value;
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
                                       }).ToList();
                    string ITCDLIST = VE.PENDINGORDER.Select(a => a.ITCD).Distinct().ToArray().retSqlfromStrarray();

                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    {
                        PRODGRPDATA = salesfunc.GetBarHelp(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retStr(), "", ITCDLIST, MTRLJOBCD.retStr(), "", "", "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);
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
                            if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
                                a.LISTDISCPER.retStr() == VE.TBATCHDTL[i].LISTDISCPER.retStr()
                           ).Select(b => b.TXNSLNO).FirstOrDefault();
                        if (VE.TBATCHDTL[i].TXNSLNO == 0)
                        {
                            txnslno++;
                            VE.TBATCHDTL[i].TXNSLNO = txnslno.retShort();

                        }
                    }
                    if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && (VE.TBATCHDTL[i].BARGENTYPE == "E" || VE.T_TXN.BARGENTYPE == "E"))
                    {
                        VE.TBATCHDTL[i].BarImages = "";
                        VE.TBATCHDTL[i].BarImagesCount = "";
                    }
                    VE.TBATCHDTL[i].MTRLJOBCD = MTRLJOBCD;
                    VE.TBATCHDTL[i].MTRLJOBNM = MTRLJOBNM;
                    VE.TBATCHDTL[i].MTBARCODE = MTBARCODE;
                }
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
        public ActionResult cancelRecords(TransactionSaleEntry VE, string par1)
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
        public ActionResult GetRateHistoryDetails(string SLCD, string PARTYCD, string ITCD, string ITNM, string TAG)
        {
            try
            {
                RateHistory RH = new RateHistory();
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                var DTRateHistory = salesfunc.GetRateHistory(SLCD.retStr(), PARTYCD.retStr(), VE.DOC_CODE.retStr().retSqlformat(), ITCD.retStr());
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
        public ActionResult CheckBillNumber(TransactionSaleEntry VE, string BILL_NO, string SUPPLIER, string AUTO_NO)
        {
            Cn.getQueryString(VE);
            if (VE.DefaultAction == "A") AUTO_NO = "";
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM_CD = CommVar.Compcd(UNQSNO);
            if (BILL_NO.retStr() == "") return Content("0");

            var query = (from c in DB.T_TXN
                         join d in DB.T_CNTRL_HDR on c.AUTONO equals d.AUTONO
                         where (c.PREFNO == BILL_NO && c.SLCD == SUPPLIER && c.AUTONO != AUTO_NO && d.COMPCD == COM_CD)
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
        public ActionResult GetTTXNDTLDetails(TransactionSaleEntry VE, string FDT, string TDT, string R_DOCNO, string R_BARNO, string TAXGRPCD, string SLCD, string datachng)
        {
            DataTable dt = new DataTable();
            dt = (DataTable)TempData["TXNDTLDetails" + VE.MENU_PARA]; TempData.Keep();
            if (datachng == "Y" || dt == null || dt.Rows.Count == 0)
            {
                Cn.getQueryString(VE); string scm = CommVar.CurSchema(UNQSNO);
                string doctag = VE.MENU_PARA.retStr() == "SR" ? "SB" : "PB";

                sql += "select x.SLNO,x.TXNSLNO,x.ITGRPCD,x.ITGRPNM,x.BARGENTYPE,x.MTRLJOBCD,x.MTRLJOBNM,x.MTBARCODE,x.ITCD,x.ITNM,x.UOMCD,x.STYLENO,x.PARTCD,x.PARTNM, ";
                sql += "x.PRTBARCODE,x.STKTYPE,x.STKNAME,x.BARNO,x.COLRCD,x.COLRNM,x.CLRBARCODE,x.SIZECD,x.SIZENM,x.SZBARCODE,x.SHADE,x.QNTY,x.NOS,x.RATE,x.DISCRATE, ";
                sql += "x.DISCTYPE,x.TDDISCRATE,x.TDDISCTYPE,x.SCMDISCTYPE,nvl(x.SCMDISCRATE,0)SCMDISCRATE,x.HSNCODE,x.BALENO,x.PDESIGN,x.OURDESIGN,x.FLAGMTR,x.LOCABIN,x.BALEYR ";
                sql += ",x.SALGLCD,x.PURGLCD,x.SALRETGLCD,x.PURRETGLCD,x.WPRATE,x.RPRATE,x.ITREM,x.RPPRICEGEN,X.DOCNO,X.DOCDT,x.WPPER,x.RPPER, ";
                sql += "x.WPPRICEGEN,x.LISTPRICE,x.LISTDISCPER,x.CUTLENGTH,x.PAGENO,x.PAGESLNO,x.PCSTYPE,x.AUTONO,x.PREFNO,x.GLCD,x.GSTPER,y.prodgrpgstper,z.barimage,z.barimagecount from";

                sql += "(select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.prodgrpcd,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, ";
                sql += "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE, ";
                sql += "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                sql += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,n.RPPRICEGEN,(s.IGSTPER+s.CGSTPER+s.SGSTPER) GSTPER, ";
                sql += "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,s.PAGENO,s.PAGESLNO,i.PCSTYPE,t.docno,t.docdt,r.AUTONO,t.PREFNO,s.GLCD,n.WPPER,n.RPPER ";
                sql += "from " + scm + ".T_BATCHDTL i, " + scm + ".T_BATCHMST j, " + scm + ".M_SITEM k, " + scm + ".M_SIZE l, " + scm + ".M_COLOR m, ";
                sql += scm + ".M_GROUP n," + scm + ".M_MTRLJOBMST o," + scm + ".M_PARTS p," + scm + ".M_STKTYPE q," + scm + ".T_CNTRL_HDR r ";
                sql += "," + scm + ".T_TXNDTL s," + scm + ".T_TXN t ";
                sql += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                sql += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) and i.AUTONO=r.AUTONO(+) ";
                sql += "and i.autono=s.autono and i.txnslno=s.slno and s.autono=t.autono ";
                sql += "and t.doctag in('" + doctag + "')  ";
                if (R_DOCNO.retStr() != "") sql += " and " + (VE.MENU_PARA.retStr() == "SR" ? "r.doconlyno in(" + R_DOCNO + ") " : "t.prefno in('" + R_DOCNO + "') ");
                if (FDT.retDateStr() != "") sql += "and r.docdt >= to_date('" + FDT + "', 'dd/mm/yyyy') ";
                if (TDT.retDateStr() != "") sql += " and r.docdt <= to_date('" + TDT + "', 'dd/mm/yyyy')  ";
                if (R_BARNO.retStr() != "") sql += "and i.barno = '" + R_BARNO + "' ";
                if (SLCD.retStr() != "") sql += "and t.slcd = '" + SLCD + "' ";
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
                sql += "where a.prodgrpcd=b.prodgrpcd(+) and a.effdt=b.effdt(+) and b.taxgrpcd='" + TAXGRPCD + "' ";
                sql += "group by a.prodgrpcd ) y, ";

                //sql += "(select a.barno, count(*) barimagecount, ";
                //sql += "listagg(a.doc_flname||'~'||a.doc_desc,chr(179)) ";
                //sql += "within group (order by a.barno) as barimage from ";
                ////sql += "listagg(a.imgbarno||chr(181)||a.imgslno||chr(181)||a.doc_flname||chr(181)||a.doc_extn||chr(181)||substr(a.doc_desc,50),chr(179)) ";
                //sql += "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
                //sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
                //sql += "from " + scm + ".m_batch_img_hdr a ";
                //sql += "union ";
                //sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
                //sql += "from " + scm + ".m_batch_img_hdr_link a, " + scm + ".m_batch_img_hdr b ";
                //sql += "where a.mainbarno=b.barno(+) ) a, ";
                //sql += "" + scm + ".m_batch_img_hdr b ";
                //sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ";
                //sql += "union ";
                //sql += "select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
                //sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
                //sql += "from " + scm + ".t_batch_img_hdr a ";
                //sql += "union ";
                //sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
                //sql += "from " + scm + ".t_batch_img_hdr_link a, " + scm + ".t_batch_img_hdr b ";
                //sql += "where a.mainbarno=b.barno(+) ) a, ";
                //sql += "" + scm + ".t_batch_img_hdr b ";
                //sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ) a ";
                //sql += "group by a.barno ) z ";
                sql += "(select a.barno, count(*) barimagecount,";
                sql += "listagg(a.doc_flname||'~'||a.doc_desc,chr(179)) ";
                sql += "within group (order by a.barno) as barimage from ";
                sql += "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
                sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
                sql += "from " + scm + ".t_batch_img_hdr a ";
                sql += "union ";
                sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
                sql += "from " + scm + ".t_batch_img_hdr_link a, " + scm + ".t_batch_img_hdr b ";
                sql += "where a.mainbarno=b.barno(+) ) a, ";
                sql += "" + scm + ".t_batch_img_hdr b ";
                sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ) a ";
                sql += "group by a.barno ) z ";

                sql += "where x.prodgrpcd=y.prodgrpcd(+) and x.barno=z.barno(+) ";
                sql += "order by x.docdt, x.docno,x.txnslno ";
                dt = masterHelp.SQLquery(sql);

                TempData["TXNDTLDetails" + VE.MENU_PARA] = dt;
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                var aa = dt.Rows[0]["rate"].GetType();
                var aa1 = dt.Rows[0]["scmdiscrate"].GetType();
                VE.TTXNDTLPOPUP = (from r1 in dt.AsEnumerable()
                                   group r1 by new
                                   {
                                       AUTONO = r1.Field<string>("autono").retStr(),
                                       AGDOCNO = VE.MENU_PARA.retStr() == "SR" ? r1.Field<string>("docno").retStr() : r1.Field<string>("PREFNO").retStr(),
                                       AGDOCDT = r1.Field<DateTime>("docdt").retDateStr(),
                                       BARNO = r1.Field<string>("barno").retStr(),
                                       ITCD = r1.Field<string>("itcd").retStr(),
                                       ITSTYLE = r1.Field<string>("styleno").retStr() + " " + r1.Field<string>("itnm").retStr(),
                                       RATE = r1.Field<double>("rate").retDbl(),
                                       SCMDISCTYPE_DESC = r1.Field<string>("scmdisctype").retStr(),
                                       SCMDISCRATE = r1.Field<decimal>("scmdiscrate").retDbl(),
                                   } into g
                                   select new TTXNDTLPOPUP
                                   {
                                       AUTONO = g.Key.AUTONO,
                                       AGDOCNO = g.Key.AGDOCNO,
                                       AGDOCDT = g.Key.AGDOCDT,
                                       BARNO = g.Key.BARNO,
                                       ITCD = g.Key.ITCD,
                                       ITSTYLE = g.Key.ITSTYLE,
                                       QNTY = g.Sum(x => x.Field<double>("QNTY")),
                                       RATE = g.Key.RATE,
                                       SCMDISCTYPE_DESC = g.Key.SCMDISCTYPE_DESC.retStr() == "P" ? "%" : g.Key.SCMDISCTYPE_DESC.retStr() == "N" ? "Nos" : g.Key.SCMDISCTYPE_DESC.retStr() == "Q" ? "Qnty" : "Fixed",
                                       SCMDISCRATE = g.Key.SCMDISCRATE,
                                   }).ToList();

                int slno = 0;
                for (int p = 0; p <= VE.TTXNDTLPOPUP.Count - 1; p++)
                {
                    slno++;
                    VE.TTXNDTLPOPUP[p].SLNO = slno.retShort();
                }

            }


            VE.DefaultView = true;
            return PartialView("_T_SALE_POPUP", VE);
        }
        public ActionResult SelectTTXNDTLDetails(TransactionSaleEntry VE)
        {
            try
            {
                DataTable dt = (DataTable)TempData["TXNDTLDetails" + VE.MENU_PARA]; TempData.Keep();
                var selectedautoslno = VE.TTXNDTLPOPUP.Where(r => r.P_Checked == true).Select(a => a.AUTONO + a.BARNO).Distinct().ToList();

                var TBATCHDTL = (from DataRow dr in dt.Rows
                                 where selectedautoslno.Contains(dr["autono"].retStr() + dr["barno"].retStr())
                                 select new TBATCHDTL
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
                                     ITREM = dr["ITREM"].retStr(),
                                     AGDOCNO = VE.MENU_PARA.retStr() == "SR" ? dr["docno"].retStr() : dr["PREFNO"].retStr(),
                                     AGDOCDT = Convert.ToDateTime(dr["docdt"].retStr()),
                                     LISTPRICE = dr["LISTPRICE"].retDbl(),
                                     LISTDISCPER = dr["LISTDISCPER"].retDbl(),
                                     CUTLENGTH = dr["CUTLENGTH"].retDbl(),
                                     PAGENO = dr["PAGENO"].retInt(),
                                     PAGESLNO = dr["PAGESLNO"].retInt(),
                                     PCSTYPE = dr["PCSTYPE"].retStr(),
                                     BarImages = dr["BarImage"].retStr(),
                                     BarImagesCount = dr["barimagecount"].retStr(),
                                     PRODGRPGSTPER = dr["PRODGRPGSTPER"].retStr(),
                                     GSTPER = dr["GSTPER"].retDbl(),
                                 }).ToList();
                if (VE.TBATCHDTL == null)
                {
                    VE.TBATCHDTL = TBATCHDTL;
                }
                else
                {
                    VE.TBATCHDTL.AddRange(TBATCHDTL);
                }
                if (VE.TBATCHDTL != null)
                {
                    for (int i = 0; i < VE.TBATCHDTL.Count; i++)
                    {
                        VE.TBATCHDTL[i].SLNO = (i + 1).retShort();
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
            return PartialView("_T_SALE_BarTab", VE);

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

                    //update to t_txntrans//
                    sql = "update " + schnm + ". t_txntrans set TRANSLCD='" + VE.T_TXNTRANS.TRANSLCD + "',TRANSMODE='" + VE.T_TXNTRANS.TRANSMODE + "', ";
                    sql += " CRSLCD ='" + VE.T_TXNTRANS.CRSLCD + "',EWAYBILLNO ='" + VE.T_TXNTRANS.EWAYBILLNO + "',LRNO ='" + VE.T_TXNTRANS.LRNO + "', ";
                    sql += " LRDT =to_date('" + VE.T_TXNTRANS.LRDT.retDateStr() + "', 'dd/mm/yyyy'),LORRYNO ='" + VE.T_TXNTRANS.LORRYNO + "',GRWT ='" + VE.T_TXNTRANS.GRWT + "', ";
                    sql += " TRWT ='" + VE.T_TXNTRANS.TRWT + "',NTWT ='" + VE.T_TXNTRANS.NTWT + "',DESTN ='" + VE.T_TXNTRANS.DESTN + "', ";
                    sql += " RECVPERSON ='" + VE.T_TXNTRANS.RECVPERSON + "',VECHLTYPE ='" + VE.T_TXNTRANS.VECHLTYPE + "',GATEENTNO ='" + VE.T_TXNTRANS.GATEENTNO + "'  "
                         + " where AUTONO='" + VE.T_TXN.AUTONO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    //update to T_TXNOTH//
                    sql = "update " + schnm + ". T_TXNOTH set CASENOS='" + VE.T_TXNOTH.CASENOS + "', NOOFCASES='" + VE.T_TXNOTH.NOOFCASES + "' ";
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
                                    HSNCODE = (VE.MENU_PARA == "PJBL") ? VE.JOBHSNCODE : dr["HSNCODE"].retStr(),
                                    BALENO = dr["BALENO"].retStr(),
                                    PDESIGN = dr["PDESIGN"].retStr(),
                                    OURDESIGN = dr["OURDESIGN"].retStr(),
                                    FLAGMTR = dr["FLAGMTR"].retDbl(),
                                    LOCABIN = dr["LOCABIN"].retStr(),
                                    BALEYR = dr["BALEYR"].retStr(),
                                    BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                    GLCD = (VE.MENU_PARA == "PJBL") ? VE.JOBEXPGLCD : dr["GLCD"].retStr(),
                                    WPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPRATE"].retDbl() : (double?)null,
                                    RPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPRATE"].retDbl() : (double?)null,
                                    ITREM = dr["ITREM"].retStr(),
                                    ORDAUTONO = dr["ORDAUTONO"].retStr(),
                                    ORDSLNO = dr["ORDSLNO"].retStr() == "" ? (short?)null : dr["ORDSLNO"].retShort(),
                                    ORDDOCNO = dr["ORDDOCNO"].retStr(),
                                    ORDDOCDT = dr["ORDDOCDT"].retStr() == "" ? "" : dr["ORDDOCDT"].retStr().Remove(10),
                                    WPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPPRICEGEN"].retStr() : "",
                                    RPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPPRICEGEN"].retStr() : "",
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
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
                                    ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl());
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

                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
                string AUTONO = DB.T_BALE.Where(a => a.BALENO == VE.BALENO_HELP && a.GOCD == VE.T_TXN.GOCD).Select(a => a.AUTONO).Distinct().ToArray().retSqlfromStrarray();
                string Scm = CommVar.CurSchema(UNQSNO);
                string str1 = "";
                DataTable tbl = new DataTable();

                str1 += "select i.AUTONO,i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, ";
                str1 += "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,sum(nvl(i.QNTY,0))QNTY,sum(nvl(i.NOS,0))NOS,i.RATE,i.DISCRATE, ";
                str1 += "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,sum(nvl(i.FLAGMTR,0))FLAGMTR,i.LOCABIN,i.BALEYR ";
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,i.ORDAUTONO,i.ORDSLNO,r.DOCNO ORDDOCNO,r.DOCDT ORDDOCDT,n.RPPRICEGEN, ";
                str1 += "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,nvl(k.NEGSTOCK,n.NEGSTOCK)NEGSTOCK ";
                str1 += ",s.AGDOCNO,s.AGDOCDT,s.PAGENO,s.PAGESLNO,i.PCSTYPE,s.glcd,s.BLUOMCD,j.COMMONUNIQBAR,j.FABITCD,t.ITNM FABITNM,n.WPPER,n.RPPER,u.BLSLNO ";
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".M_STKTYPE q," + Scm + ".T_CNTRL_HDR r ";
                str1 += "," + Scm + ".T_TXNDTL s, " + Scm + ".M_SITEM t, " + Scm + ".T_BALE u ";
                str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) and i.ORDAUTONO=r.AUTONO(+) and j.fabitcd=t.itcd(+) ";
                str1 += "and i.autono=s.autono and i.txnslno=s.slno and i.autono=u.autono(+) and i.slno=u.slno(+) and i.baleno=u.baleno(+) ";
                str1 += "and i.AUTONO in (" + AUTONO + ") and i.BALENO='" + VE.BALENO_HELP + "' and i.GOCD='" + VE.T_TXN.GOCD + "' ";
                str1 += "and i.SLNO <= 1000 ";
                str1 += "group by i.AUTONO,i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, ";
                str1 += "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.RATE,i.DISCRATE, ";
                str1 += "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.LOCABIN,i.BALEYR ";
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,i.ORDAUTONO,i.ORDSLNO,r.DOCNO,r.DOCDT,n.RPPRICEGEN, ";
                str1 += "n.WPPRICEGEN,i.LISTPRICE,i.LISTDISCPER,i.CUTLENGTH,nvl(k.NEGSTOCK,n.NEGSTOCK) ";
                str1 += ",s.AGDOCNO,s.AGDOCDT,s.PAGENO,s.PAGESLNO,i.PCSTYPE,s.glcd,s.BLUOMCD,j.COMMONUNIQBAR,j.FABITCD,t.ITNM,n.WPPER,n.RPPER,u.BLSLNO ";
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
                                    HSNCODE = dr["HSNCODE"].retStr(),
                                    BALENO = dr["BALENO"].retStr(),
                                    PDESIGN = dr["PDESIGN"].retStr(),
                                    OURDESIGN = dr["OURDESIGN"].retStr(),
                                    FLAGMTR = dr["FLAGMTR"].retDbl(),
                                    LOCABIN = dr["LOCABIN"].retStr(),
                                    BALEYR = dr["BALEYR"].retStr(),
                                    BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                    GLCD = dr["GLCD"].retStr(),
                                    WPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPRATE"].retDbl() : (double?)null,
                                    RPRATE = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPRATE"].retDbl() : (double?)null,
                                    ITREM = dr["ITREM"].retStr(),
                                    ORDAUTONO = dr["ORDAUTONO"].retStr(),
                                    ORDSLNO = dr["ORDSLNO"].retStr() == "" ? (short?)null : dr["ORDSLNO"].retShort(),
                                    ORDDOCNO = dr["ORDDOCNO"].retStr(),
                                    ORDDOCDT = dr["ORDDOCDT"].retStr() == "" ? "" : dr["ORDDOCDT"].retStr().Remove(10),
                                    WPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["WPPRICEGEN"].retStr() : "",
                                    RPPRICEGEN = (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") ? dr["RPPRICEGEN"].retStr() : "",
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
                                    RECPROGSLNO = dr["BLSLNO"].retShort(),
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
                string PRCCD = VE.T_TXNOTH.PRCCD.retStr() == "" ? "WP" : VE.T_TXNOTH.PRCCD.retStr();
                string TAXGRPCD = VE.T_TXNOTH.TAXGRPCD.retStr() == "" ? "C001" : VE.T_TXNOTH.TAXGRPCD.retStr();
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                {
                    allprodgrpgstper_data = salesfunc.GetBarHelp(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retStr(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);

                }
                else
                {
                    allprodgrpgstper_data = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);
                }
                //var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                var MSYSCNFG = salesfunc.M_SYSCNFG();
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
                                    ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl());
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
                                if (VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "SBEXP")
                                {
                                    v.RATE = tax_data.Rows[0]["RATE"].retDbl();
                                }
                                if ((VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "SBEXP") && MSYSCNFG.MNTNLISTPRICE == "Y")
                                {
                                    v.LISTPRICE = tax_data.Rows[0]["RATE"].retDbl();
                                }
                            }
                        }
                    }

                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
        public dynamic SAVE(TransactionSaleEntry VE, string othr_para = "")
        {
            //Cn.getQueryString(VE);
            //Oracle Queries
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
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

                DataTable baledata = new DataTable();
                if ((VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "PJBL") && balenocount > 0)
                {
                    var baleno = VE.TTXNDTL.Select(a => a.BALENO).Distinct().ToArray().retSqlfromStrarray();
                    string str = "select rslno,blautono,blslno,lrdt,lrno,baleyr,gocd,baleopen,baleno from " + scm1 + ".t_bale where baleno in (" + baleno + ") ";
                    baledata = masterHelp.SQLquery(str);
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
                    //checking barcode & txndtl pge itcd wise qnty, nos should match

                    if (VE.TBATCHDTL != null && VE.TTXNDTL != null)
                    {
                        var barcodedata = (from x in VE.TBATCHDTL
                                           group x by new { x.ITCD } into P
                                           select new
                                           {
                                               ITCD = P.Key.ITCD,
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
                            OraTrans.Rollback();
                            OraCon.Dispose();
                            ContentFlg = "Barcode grid & Detail grid itcd [" + diffitcd + "] wise qnty, nos should match !!";
                            goto dbnotsave;
                        }
                    }


                    T_TXN TTXN = new T_TXN();
                    T_TXNTRANS TXNTRANS = new T_TXNTRANS();
                    T_TXNOTH TTXNOTH = new T_TXNOTH();
                    T_TXN_LINKNO TTXNLINKNO = new T_TXN_LINKNO();

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
                            stkdrcr = "N"; blactpost = true; blgstpost = true; break;
                        case "SB":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Sale" + strqty; break;
                        case "SBDIR":
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
                            stkdrcr = "0"; blactpost = false; blgstpost = true; break;
                        case "PB":
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
                        case "SCN":
                            stkdrcr = "N"; blactpost = false; blgstpost = true; break;
                        case "SDN":
                            stkdrcr = "N"; blactpost = false; blgstpost = true; break;
                        case "PCN":
                            stkdrcr = "N"; blactpost = false; blgstpost = true; break;
                        case "PDN":
                            stkdrcr = "N"; blactpost = false; blgstpost = true; break;
                    }

                    string slcdlink = "", slcdpara = VE.MENU_PARA;
                    if (VE.MENU_PARA == "PR") slcdpara = "PB";
                    //M_SYSCNFG MSYSCNFG = DB.M_SYSCNFG.FirstOrDefault();
                    M_SYSCNFG MSYSCNFG = salesfunc.M_SYSCNFG();
                    string sql = "";
                    if (MSYSCNFG == null)
                    {
                        ContentFlg = "Debtor/Creditor code not setup/No data found at m_syscnfg"; goto dbnotsave;
                    }
                    string prodglcd = (VE.TTXNDTL != null) ? VE.TTXNDTL[0].GLCD.retStr() : "";

                    sql = "select b.rogl, b.tcsgl, a.class1cd, null class2cd, nvl(c.crlimit,0) crlimit, nvl(c.crdays,0) crdays, ";
                    sql += "'" + prodglcd.retStr() + "' prodglcd, ";
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") sql += "b.igst_p igstcd, b.cgst_p cgstcd, b.sgst_p sgstcd, b.cess_p cesscd, b.duty_p dutycd, ";
                    else sql += "b.igst_s igstcd, b.cgst_s cgstcd, b.sgst_s sgstcd, b.cess_s cesscd, b.duty_s dutycd, ";
                    if (slcdpara == "PB" || slcdpara == "OP" || slcdpara == "OTH" || VE.MENU_PARA == "PJRC") sql += "a.purdebglcd parglcd, "; else sql += "a.saldebglcd parglcd, ";
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
                        TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                        if (DOCPATTERN.retStr().Length > 16 && (VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR"))
                        {
                            ContentFlg = "Document No. length should be less than 16.change from Document type master "; goto dbnotsave;
                        }
                        auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
                        TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                        Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        TempData["LASTGOCD" + VE.MENU_PARA] = VE.T_TXN.GOCD;
                        TempData["LASTROUNDOFF" + VE.MENU_PARA] = VE.RoundOff == true ? "Y" : "N";
                        //TCH = Cn.T_CONTROL_HDR(TTXN.DOCCD, TTXN.DOCDT, TTXN.DOCNO, TTXN.AUTONO, Month, DOCPATTERN, VE.DefaultAction, scm1, null, TTXN.SLCD, TTXN.BLAMT.Value, null);
                    }
                    else
                    {
                        ContentFlg = ChildRecordCheck(VE.T_TXN.AUTONO);
                        if (ContentFlg != "") goto dbnotsave;
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        TTXN.DOCNO = VE.T_TXN.DOCNO;
                        TTXN.AUTONO = VE.T_TXN.AUTONO;
                        Month = VE.T_CNTRL_HDR.MNTHCD;
                        TTXN.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                        DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        TTXN.DTAG = "E";
                    }
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
                    if (VE.DefaultAction == "E")
                    {
                        dbsql = masterHelp.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_batchmst_price", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                        var comp = DB1.T_BATCHMST.Where(x => x.AUTONO == TTXN.AUTONO).OrderBy(s => s.AUTONO).ToList();
                        foreach (var v in comp)
                        {
                            if (!VE.TBATCHDTL.Where(s => s.BARNO == v.BARNO && s.SLNO == v.SLNO).Any())
                            {
                                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
                        dbsql = masterHelp.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        if (VE.MENU_PARA == "SB" || VE.MENU_PARA == "PJBL")
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
                    //----------------------------------------------------------//

                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD, VE.DISPBLAMT.retDbl());
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(TTXN, VE.DefaultAction);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TXNTRANS);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TTXNOTH);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    if (VE.MENU_PARA == "SB")
                    {
                        TTXNLINKNO.EMD_NO = TTXN.EMD_NO;
                        TTXNLINKNO.CLCD = TTXN.CLCD;
                        TTXNLINKNO.DTAG = TTXN.DTAG;
                        TTXNLINKNO.TTAG = TTXN.TTAG;
                        TTXNLINKNO.AUTONO = TTXN.AUTONO;
                        TTXNLINKNO.LINKAUTONO = VE.T_TXN_LINKNO.LINKAUTONO;
                    }
                    else if (VE.MENU_PARA == "PJBL")
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
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD, VE.DISPBLAMT.retDbl());
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        double currrt = 0;
                        if (TTXN.CURRRT != null) currrt = Convert.ToDouble(TTXN.CURRRT);
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
                        TTXNEWB.TRANSLCD = VE.T_TXNTRANS.TRANSLCD;
                        TTXNEWB.EWAYBILLNO = VE.T_TXNTRANS.EWAYBILLNO;
                        TTXNEWB.LRNO = VE.T_TXNTRANS.LRNO;
                        TTXNEWB.LRDT = VE.T_TXNTRANS.LRDT;
                        TTXNEWB.LORRYNO = VE.T_TXNTRANS.LORRYNO;
                        TTXNEWB.TRANSMODE = VE.T_TXNTRANS.TRANSMODE;
                        TTXNEWB.VECHLTYPE = VE.T_TXNTRANS.VECHLTYPE;
                        TTXNEWB.GOCD = VE.T_TXN.GOCD;
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
                    if (balenocount > 0)
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
                                TTXNDTL.ITREM = VE.TTXNDTL[i].ITREM;
                                TTXNDTL.PCSREM = VE.TTXNDTL[i].PCSREM;
                                TTXNDTL.FREESTK = VE.TTXNDTL[i].FREESTK;
                                TTXNDTL.BATCHNO = VE.TTXNDTL[i].BATCHNO;
                                TTXNDTL.BALEYR = VE.TTXNDTL[i].BALENO.retStr() == "" ? "" : VE.BALEYR;// VE.TTXNDTL[i].BALEYR;
                                TTXNDTL.BALENO = VE.TTXNDTL[i].BALENO;
                                TTXNDTL.GOCD = VE.T_TXN.GOCD;
                                TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
                                TTXNDTL.NOS = VE.TTXNDTL[i].NOS == null ? 0 : VE.TTXNDTL[i].NOS;
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

                                if ((VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") && VE.TTXNDTL[i].AGDOCNO.retStr() != "" && VE.TTXNDTL[i].AGDOCDT.retStr() != "")
                                {
                                    TTXNDTL.AGDOCNO = VE.TTXNDTL[i].AGDOCNO;
                                    TTXNDTL.AGDOCDT = VE.TTXNDTL[i].AGDOCDT;
                                }
                                TTXNDTL.LISTPRICE = VE.TTXNDTL[i].LISTPRICE;
                                TTXNDTL.LISTDISCPER = VE.TTXNDTL[i].LISTDISCPER;
                                TTXNDTL.PAGENO = VE.TTXNDTL[i].PAGENO;
                                TTXNDTL.PAGESLNO = VE.TTXNDTL[i].PAGESLNO;
                                TTXNDTL.BLUOMCD = VE.TTXNDTL[i].BLUOMCD;
                                dbsql = masterHelp.RetModeltoSql(TTXNDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                dbqty = dbqty + Convert.ToDouble(VE.TTXNDTL[i].QNTY);
                                igst = igst + Convert.ToDouble(VE.TTXNDTL[i].IGSTAMT);
                                cgst = cgst + Convert.ToDouble(VE.TTXNDTL[i].CGSTAMT);
                                sgst = sgst + Convert.ToDouble(VE.TTXNDTL[i].SGSTAMT);
                                cess = cess + Convert.ToDouble(VE.TTXNDTL[i].CESSAMT);
                                duty = duty + Convert.ToDouble(VE.TTXNDTL[i].DUTYAMT);

                                //if (VE.TTXNDTL[i].BALENO.retStr() != "" && (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP"))
                                if (VE.TTXNDTL[i].BALENO.retStr() != "")
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
                                    if ((VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "PR") && baledata.Rows.Count > 0)
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
                                if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E"))
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
                                    if (VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
                                    if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && ((VE.T_TXN.BARGENTYPE == "E") || (VE.T_TXN.BARGENTYPE == "C" && VE.TBATCHDTL[i].BARGENTYPE == "E")))
                                    {
                                        TBATCHMST.WPRATE = VE.TBATCHDTL[i].WPRATE;
                                        TBATCHMST.RPRATE = VE.TBATCHDTL[i].RPRATE;
                                    }
                                    //dbsql = masterHelp.RetModeltoSql(TBATCHMST);
                                    TBATCHMST.COMMONUNIQBAR = (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E") ? "E" : "";

                                    if (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E")
                                    {
                                        TBATCHMST.HSNCODE = VE.TBATCHDTL[i].HSNCODE;
                                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                                        {
                                            TBATCHMST.OURDESIGN = VE.TBATCHDTL[i].OURDESIGN;
                                        }
                                    }
                                    dbsql = masterHelp.RetModeltoSql(TBATCHMST, Action, "", SqlCondition);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                    if (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E")
                                    {
                                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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

                                //add price
                                if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E"))
                                {
                                    T_BATCHMST_PRICE TBATCHMSTPRICE = new T_BATCHMST_PRICE();
                                    TBATCHMSTPRICE.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMSTPRICE.CLCD = TTXN.CLCD;
                                    TBATCHMSTPRICE.DTAG = TTXN.DTAG;
                                    TBATCHMSTPRICE.TTAG = TTXN.TTAG;
                                    TBATCHMSTPRICE.BARNO = barno;
                                    TBATCHMSTPRICE.PRCCD = "CP";
                                    TBATCHMSTPRICE.EFFDT = TTXN.DOCDT;
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
                                        TBATCHMSTPRICE1.EFFDT = TTXN.DOCDT;
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
                                        TBATCHMSTPRICE2.EFFDT = TTXN.DOCDT;
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
                                TBATCHDTL.GOCD = VE.T_TXN.GOCD;
                                TBATCHDTL.BARNO = barno;
                                TBATCHDTL.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
                                TBATCHDTL.PARTCD = VE.TBATCHDTL[i].PARTCD;
                                TBATCHDTL.HSNCODE = VE.TBATCHDTL[i].HSNCODE;
                                TBATCHDTL.STKDRCR = stkdrcr;
                                TBATCHDTL.NOS = VE.TBATCHDTL[i].NOS;
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
                                TBATCHDTL.BALEYR = VE.TBATCHDTL[i].BALENO.retStr() == "" ? "" : VE.BALEYR;// VE.TBATCHDTL[i].BALEYR;
                                TBATCHDTL.BALENO = VE.TBATCHDTL[i].BALENO;
                                if (VE.MENU_PARA == "SBPCK")
                                {
                                    TBATCHDTL.ORDAUTONO = VE.TBATCHDTL[i].ORDAUTONO;
                                    TBATCHDTL.ORDSLNO = VE.TBATCHDTL[i].ORDSLNO;
                                }
                                TBATCHDTL.LISTPRICE = VE.TBATCHDTL[i].LISTPRICE;
                                TBATCHDTL.LISTDISCPER = VE.TBATCHDTL[i].LISTDISCPER;
                                TBATCHDTL.CUTLENGTH = VE.TBATCHDTL[i].CUTLENGTH;
                                TBATCHDTL.STKTYPE = VE.TBATCHDTL[i].STKTYPE;

                                if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && MSYSCNFG.MNTNPCSTYPE == "Y")
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
                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                        {
                            if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                            {
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
                                igst = igst + Convert.ToDouble(VE.TTXNAMT[i].IGSTAMT);
                                cgst = cgst + Convert.ToDouble(VE.TTXNAMT[i].CGSTAMT);
                                sgst = sgst + Convert.ToDouble(VE.TTXNAMT[i].SGSTAMT);
                                cess = cess + Convert.ToDouble(VE.TTXNAMT[i].CESSAMT);
                                duty = duty + Convert.ToDouble(VE.TTXNAMT[i].DUTYAMT);
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
                        if (VE.MENU_PARA == "SBPCK" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBDIR" || VE.MENU_PARA == "SR" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "PJBL") salpur = "S";
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
                                           }).Where(a => a.QTY != 0).ToList();

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
                                    dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                            AMTGLCD[i].CLASS1CD, "", AMTGLCD[i].TXBLVAL.retDbl(), 0, strrem);
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
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
                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                        tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
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
                                    dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), null,
                                            tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion
                        // TCS
                        dbamt = Convert.ToDouble(VE.T_TXN.TCSAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;

                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tbl.Rows[0]["tcsgl"].ToString(), sslcd,
                                    dbamt, strrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                            dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                    null, null, Convert.ToDouble(VE.T_TXN.TCSAMT), 0, strrem + " TCS @ " + VE.T_TXN.TCSPER.ToString() + "%");
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
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
                        isl = 1; //isl + 1;
                        dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr,
                            tbl.Rows[0]["parglcd"].ToString(), sslcd, Convert.ToDouble(VE.T_TXN.BLAMT), strrem, tbl.Rows[0]["prodglcd"].ToString(),
                            null, dbqty, 0, dbcurramt);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        if (dr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
                        else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), Convert.ToDouble(VE.T_TXN.BLAMT), dbcurramt, strrem);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        strblno = ""; strbldt = ""; strduedt = ""; strvtype = ""; strrefno = "";
                        if (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "PCN")
                        {
                            strvtype = "CN";
                        }
                        else if (VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PDN")
                        {
                            strvtype = "DN";
                        }
                        else
                        {
                            strvtype = "BL";
                        }
                        strduedt = Convert.ToDateTime(TTXN.DOCDT.Value).AddDays(Convert.ToDouble(TTXN.DUEDAYS)).ToString().retDateStr();
                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH")
                        {
                            strblno = TTXN.PREFNO;
                            strbldt = TTXN.PREFDT.ToString();
                        }
                        else
                        {
                            strbldt = TTXN.DOCDT.ToString();
                            strblno = DOCPATTERN;
                        }
                        string blconslcd = TTXN.CONSLCD;
                        if (TTXN.SLCD != sslcd) blconslcd = TTXN.SLCD;
                        if (blconslcd == sslcd) blconslcd = "";
                        dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, dr,
                                tbl.Rows[0]["parglcd"].ToString(), sslcd, blconslcd, TTXNOTH.AGSLCD, tbl.Rows[0]["class1cd"].ToString(), Convert.ToSByte(isl),
                                VE.T_TXN.BLAMT.retDbl(), strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), itamt, TTXNOTH.POREFNO,
                                TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), VE.T_TXN.BLAMT.retDbl(),
                                VE.T_TXNTRANS.LRNO, VE.T_TXNTRANS.LRDT == null ? "" : VE.T_TXNTRANS.LRDT.ToString().retDateStr(), VE.TransporterName, "", "", VE.T_TXNOTH.BLTYPE);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                    }
                    if (blgstpost == true)
                    {
                        #region TVCHGST Table update    

                        int gs = 0;
                        string dncntag = ""; string exemptype = "";
                        if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "SRPOS") dncntag = "SC";
                        if (VE.MENU_PARA == "PR") dncntag = "PD";
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
                                    TVCHGST.SLNO = gs.retShort();
                                    TVCHGST.PCODE = TTXN.SLCD;
                                    TVCHGST.BLNO = strblno;
                                    if (strbldt.retStr() != "")
                                    {
                                        TVCHGST.BLDT = Convert.ToDateTime(strbldt);
                                    }
                                    TVCHGST.HSNCODE = VE.TTXNDTL[i].HSNCODE;
                                    TVCHGST.ITNM = VE.TTXNDTL[i].ITSTYLE;
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
                                    TVCHGST.UOM = VE.TTXNDTL[i].BLUOMCD.retStr() != "" ? VE.TTXNDTL[i].BLUOMCD : VE.TTXNDTL[i].UOM;
                                    TVCHGST.AGSTDOCNO = VE.TTXNDTL[i].AGDOCNO;
                                    TVCHGST.AGSTDOCDT = VE.TTXNDTL[i].AGDOCDT;
                                    TVCHGST.SALPUR = salpur;
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
                                    TVCHGST.EXPGLCD = (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN") ? VE.EXPGLCD : expglcd;
                                    TVCHGST.INPTCLAIM = "Y";
                                    TVCHGST.LUTNO = VE.T_VCH_GST.LUTNO;
                                    TVCHGST.LUTDT = VE.T_VCH_GST.LUTDT;
                                    TVCHGST.TCSPER = TTXN.TCSPER.retDbl();
                                    TVCHGST.TCSAMT = gtcsamt;
                                    TVCHGST.BASAMT = VE.TTXNDTL[i].AMT.retDbl();
                                    TVCHGST.DISCAMT = VE.TTXNDTL[i].TOTDISCAMT.retDbl();
                                    TVCHGST.RATE = VE.TTXNDTL[i].RATE.retDbl();
                                    TVCHGST.PINV = pinv;
                                    TVCHGST.GOOD_SERV = (VE.MENU_PARA == "PJBL") ? "S" : "";
                                    dbsql = masterHelp.RetModeltoSql(TVCHGST, "A", CommVar.FinSchema(UNQSNO));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                    //dbsql = masterHelp.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                    //        Convert.ToSByte(isl), Convert.ToSByte(gs), TTXN.SLCD, strblno, strbldt, VE.TTXNDTL[i].HSNCODE, VE.TTXNDTL[i].ITNM, (VE.TTXNDTL[i].BLQNTY == null || VE.TTXNDTL[i].BLQNTY == 0 ? VE.TTXNDTL[i].QNTY.retDbl() : VE.TTXNDTL[i].BLQNTY.retDbl()), (VE.TTXNDTL[i].UOM == null ? VE.TTXNDTL[i].UOM : VE.TTXNDTL[i].UOM),
                                    //        VE.TTXNDTL[i].NETAMT.retDbl(), VE.TTXNDTL[i].IGSTPER.retDbl(), VE.TTXNDTL[i].IGSTAMT.retDbl(), VE.TTXNDTL[i].CGSTPER.retDbl(), VE.TTXNDTL[i].CGSTAMT.retDbl(), VE.TTXNDTL[i].SGSTPER.retDbl(), VE.TTXNDTL[i].SGSTAMT.retDbl(),
                                    //        VE.TTXNDTL[i].CESSPER.retDbl(), VE.TTXNDTL[i].CESSAMT.retDbl(), salpur, groamt, gblamt, 0, (VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD), VE.POS, VE.TTXNDTL[i].AGDOCNO, VE.TTXNDTL[i].AGDOCDT.ToString(), TTXNOTH.DNCNCD,
                                    //        VE.GSTSLNM, VE.GSTNO, "", "", "", VE.T_VCH_GST.EXPCD, VE.T_VCH_GST.SHIPDOCNO, SHIPDOCDT, VE.T_VCH_GST.PORTCD, dncntag, TTXN.CONSLCD, exemptype, 0, expglcd, "Y");
                                    //OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    gblamt = 0; groamt = 0; gtcsamt = 0;
                                }
                            }
                        }
                        string AGDOCNO = (VE.TTXNDTL != null) ? VE.TTXNDTL[0].AGDOCNO : "";

                        if (VE.TTXNAMT != null)
                        {
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
                                    TVCHGST1.AMT = VE.TTXNAMT[i].AMT.retDbl();
                                    TVCHGST1.CGSTPER = VE.TTXNAMT[i].CGSTPER.retDbl();
                                    TVCHGST1.SGSTPER = VE.TTXNAMT[i].SGSTPER.retDbl();
                                    TVCHGST1.IGSTPER = VE.TTXNAMT[i].IGSTPER.retDbl();
                                    TVCHGST1.CGSTAMT = VE.TTXNAMT[i].CGSTAMT.retDbl();
                                    TVCHGST1.SGSTAMT = VE.TTXNAMT[i].SGSTAMT.retDbl();
                                    TVCHGST1.IGSTAMT = VE.TTXNAMT[i].IGSTAMT.retDbl();
                                    TVCHGST1.CESSPER = VE.TTXNAMT[i].CESSPER.retDbl();
                                    TVCHGST1.CESSAMT = VE.TTXNAMT[i].CESSAMT.retDbl();
                                    TVCHGST1.DRCR = cr;
                                    TVCHGST1.QNTY = 0;
                                    TVCHGST1.UOM = "OTH";
                                    TVCHGST1.AGSTDOCNO = AGDOCNO;
                                    if (VE.TTXNDTL != null)
                                    {
                                        TVCHGST1.AGSTDOCDT = VE.TTXNDTL[0].AGDOCDT;
                                    }
                                    TVCHGST1.SALPUR = salpur;
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
                                    TVCHGST1.GOOD_SERV = (VE.MENU_PARA == "PJBL") ? "S" : good_serv;
                                    //TVCHGST1.EXPGLCD = ((VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN") && (VE.TBATCHDTL == null && VE.TTXNDTL == null)) ? VE.TTXNAMT[0].GLCD : expglcd;
                                    TVCHGST1.EXPGLCD = (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN") ? VE.EXPGLCD : expglcd;
                                    TVCHGST1.INPTCLAIM = "Y";
                                    TVCHGST1.LUTNO = VE.T_VCH_GST.LUTNO;
                                    TVCHGST1.LUTDT = VE.T_VCH_GST.LUTDT;
                                    TVCHGST1.TCSPER = TTXN.TCSPER.retDbl();
                                    TVCHGST1.TCSAMT = gtcsamt.retDbl();
                                    TVCHGST1.BASAMT = VE.TTXNAMT[i].AMT.retDbl();
                                    TVCHGST1.RATE = VE.TTXNAMT[i].AMTRATE.retDbl();
                                    TVCHGST1.PINV = pinv;
                                    dbsql = masterHelp.RetModeltoSql(TVCHGST1, "A", CommVar.FinSchema(UNQSNO));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


                                    //dbsql = masterHelp.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                    //        Convert.ToSByte(isl), Convert.ToSByte(gs), TTXN.SLCD, strblno, strbldt, VE.TTXNAMT[i].HSNCODE, VE.TTXNAMT[i].AMTNM, 0, "OTH",
                                    //        VE.TTXNAMT[i].AMT.retDbl(), VE.TTXNAMT[i].IGSTPER.retDbl(), VE.TTXNAMT[i].IGSTAMT.retDbl(), VE.TTXNAMT[i].CGSTPER.retDbl(), VE.TTXNAMT[i].CGSTAMT.retDbl(), VE.TTXNAMT[i].SGSTPER.retDbl(), VE.TTXNAMT[i].SGSTAMT.retDbl(),
                                    //        VE.TTXNAMT[i].CESSPER.retDbl(), VE.TTXNAMT[i].CESSAMT.retDbl(), salpur, groamt, gblamt, 0, (VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD), VE.POS, VE.TTXNDTL[0].AGDOCNO, VE.TTXNDTL[0].AGDOCDT.ToString(), TTXNOTH.DNCNCD,
                                    //        VE.GSTSLNM, VE.GSTNO, "", "", "", VE.T_VCH_GST.EXPCD, VE.T_VCH_GST.SHIPDOCNO, SHIPDOCDT, VE.T_VCH_GST.PORTCD, dncntag, TTXN.CONSLCD, exemptype, 0, VE.TTXNAMT[i].GLCD, "Y");
                                    //OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    gblamt = 0; groamt = 0;
                                }
                            }
                        }


                        #endregion
                    }
                    if (VE.MENU_PARA != "OP" && VE.MENU_PARA != "OTH" && VE.MENU_PARA != "PJRC" && VE.MENU_PARA != "PJIS" && VE.MENU_PARA != "PJRT")
                    {
                        if (igst != 0 && (cgst + sgst) != 0)
                        {
                            ContentFlg = "We can't add igst+cgst+sgst for the same party.";
                            goto dbnotsave;
                        }
                        else if (igst + cgst + sgst == 0)
                        {
                            if (VE.MENU_PARA == "SCN" || VE.MENU_PARA == "SDN" || VE.MENU_PARA == "PCN" || VE.MENU_PARA == "PDN")
                            {
                                ContentFlg = "Please enter tax % in Amount tab";
                            }
                            else
                            {
                                ContentFlg = "TAX amount not found. Please add tax with item.";
                            }
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
                        ContentFlg = "1" + " (Bill No. " + DOCPATTERN + ")~" + TTXN.AUTONO;
                    }
                    else if (VE.DefaultAction == "E")
                    {
                        ContentFlg = "2";
                    }
                    OraTrans.Commit();
                    goto dbsave;
                }
                else if (VE.DefaultAction == "V")
                {
                    ContentFlg = ChildRecordCheck(VE.T_TXN.AUTONO); if (ContentFlg != "") goto dbnotsave;
                    dbsql = masterHelp.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
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
                            else if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC") && IsTransactionFound == "")
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

                    dbsql = masterHelp.TblUpdt("t_txnamt", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txntrans", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    if (VE.MENU_PARA == "SB" || VE.MENU_PARA == "PJBL")
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
                    if (VE.MENU_PARA != "PJRC" && VE.MENU_PARA != "PJIS" && VE.MENU_PARA != "PJRT")
                    {
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "F", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    }
                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
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
        private string ChildRecordCheck(string autono)
        {
            string message = "";
            string scm = CommVar.CurSchema(UNQSNO);
            string fcm = CommVar.FinSchema(UNQSNO);
            sql = "";
            sql += "select a.autono,b.docno,b.docdt,c.docnm ";
            sql += "from " + scm + ".T_BALE a,  " + scm + ".t_cntrl_hdr b,  " + scm + ".m_doctype c ";
            sql += "where a.autono = B.AUTONO and b.doccd = c.DOCCD and a.blautono = '" + autono + "' ";
            sql += "and a.autono not in ('" + autono + "')";
            sql += "union all ";
            sql += "select a.autono,b.docno,b.docdt,c.docnm  ";
            sql += "from  " + fcm + ".T_vch_bl_adj a," + scm + ".t_cntrl_hdr b ," + scm + ".m_doctype c  ";
            sql += "where a.autono=B.AUTONO and b.doccd=c.DOCCD and  a.i_autono='" + autono + "'  ";
            DataTable dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                message = "Clild record found at docno:" + dt.Rows[0]["docno"].ToString() + " docdt:" + dt.Rows[0]["docdt"].retDateStr() + " docnm:" + dt.Rows[0]["docnm"].ToString() + " autono:" + dt.Rows[0]["autono"].ToString() + " ";
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
            var m_syscnfg = salesfunc.M_SYSCNFG();
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
    }
}