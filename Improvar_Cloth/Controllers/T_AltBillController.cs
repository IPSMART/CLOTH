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
    public class T_AltBillController : Controller
    {
        // GET: T_AltBill
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp(); MasterHelpFa MasterHelpFa = new MasterHelpFa(); SchemeCal Scheme_Cal = new SchemeCal(); Salesfunc salesfunc = new Salesfunc(); DataTable DT = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR; T_TXN sl; T_TXNMEMO Smemo; T_TXNOTH sOTH;
        SMS SMS = new SMS(); string sql = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_AltBill
        public ActionResult T_AltBill(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    // M_PARA CAPTION DOCCD JOBCD.FLAG1
                    // SSTB    SSTB
                    //SATB    SATB
                    TranAltBill VE = (parkID == "") ? new TranAltBill() : (Improvar.ViewModels.TranAltBill)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.MENU_PARA)
                    {
                        case "ST":
                            ViewBag.formname = "Stiching"; break;
                        case "AT":
                            ViewBag.formname = "Alteration"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
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

                        VE.IndexKey = (from p in DB.T_TXNMEMO
                                       join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       orderby q.DOCDT, q.DOCNO
                                       where XYZ.Contains(q.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YR1
                                       select new IndexKey() { Navikey = p.AUTONO }).ToList();
                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
                        if (op == "E" || op == "D" || op == "V")
                        {
                            if (searchValue.Length != 0)
                            {
                                if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
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
                            VE.T_TXN = sl;
                            VE.T_TXNMEMO = Smemo;
                            VE.T_CNTRL_HDR = TCH;
                            VE.T_CNTRL_HDR_REM = SLR;
                            VE.T_TXNOTH = sOTH;
                            if (VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }
                        if ((op.ToString() == "A" || op.ToString() == "E") && parkID == "")
                        {

                            if (VE.TTXNPYMT == null || VE.TTXNPYMT.Count == 0)
                            {
                                var MPAYMENT = (from i in DB.M_PAYMENT join j in DB.M_CNTRL_HDR on i.M_AUTONO equals j.M_AUTONO where j.INACTIVE_TAG == "N" select new { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD, PYMTTYPE = i.PYMTTYPE }).OrderBy(a => a.PYMTCD).ToList();
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
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {
                                T_TXNOTH TXNOTH = new T_TXNOTH(); T_TXNMEMO TXNMEMO = new T_TXNMEMO();
                                DataTable syscnfgdt = salesfunc.GetSyscnfgData(CommVar.CurrDate(UNQSNO).retDateStr());
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
                                    VE.RETDEBSLCD = syscnfgdt.Rows[0]["retdebslcd"].retStr();
                                    TXNOTH.TAXGRPCD = syscnfgdt.Rows[0]["TAXGRPCD"].retStr();
                                    TXNOTH.PRCCD = syscnfgdt.Rows[0]["prccd"].retStr();
                                    VE.EFFDT = syscnfgdt.Rows[0]["effdt"].retDateStr();
                                }
                                VE.T_TXNMEMO = TXNMEMO;
                                VE.T_TXNOTH = TXNOTH;
                                VE.RoundOff = true;

                                T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                                TCH.DOCDT = Cn.getCurrentDate(VE.mindate);
                                VE.T_CNTRL_HDR = TCH;
                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;

                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (TranAltBill)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        VE.VECHLTYPE = masterHelp.VECHLTYPE();
                        VE.TRANSMODE = masterHelp.TRANSMODE();
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }

                    FreightCharges(VE, VE.T_TXN?.AUTONO);
                    string docdt = "";
                    if (TCH != null) if (TCH.DOCDT != null) docdt = TCH.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_CNTRL_HDR.DOCCD, docdt, VE.DefaultAction, VE.T_CNTRL_HDR.DOCONLYNO, VE);
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                TranAltBill VE = new TranAltBill();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TranAltBill Navigation(TranAltBill VE, ImprovarDB DB, int index, string searchValue)
        {
            string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);

            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM(); sl = new T_TXN(); Smemo = new T_TXNMEMO();
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
                sl = DB.T_TXN.Find(aa[0]);
                Smemo = DB.T_TXNMEMO.Find(sl.AUTONO);
                TCH = DB.T_CNTRL_HDR.Find(sl.AUTONO);
                sOTH = DB.T_TXNOTH.Find(sl.AUTONO);
                T_TXNDTL tDtl = DB.T_TXNDTL.Where(m => m.AUTONO == sl.AUTONO).FirstOrDefault();

                VE.T_TXNMEMO = Smemo;
                VE.RTDEBNM = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.RTDEBNM).FirstOrDefault();
                VE.MOBILE = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.MOBILE).FirstOrDefault();
                var add1 = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.ADD1).FirstOrDefault();
                var add2 = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.ADD2).FirstOrDefault();
                var add3 = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.ADD3).FirstOrDefault();
                var city = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.CITY).FirstOrDefault();
                VE.ADDR = add1 + " " + add2 + " " + add3 + "/" + city;
                VE.EFFDT = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DB.M_SYSCNFG.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.EFFDT).FirstOrDefault().retDateStr();

                string sql1 = "";
                sql1 += " select a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate,C.TAXGRPCD,a.retdebslcd,b.city,b.add1,b.add2,b.add3,effdt ";
                sql1 += "  from  " + scm + ".M_SYSCNFG a, " + scmf + ".M_RETDEB b, " + scm + ".M_SUBLEG_SDDTL c";
                sql1 += " where a.RTDEBCD=b.RTDEBCD and a.effdt in(select max(effdt) effdt from  " + scm + ".M_SYSCNFG) and a.retdebslcd=C.SLCD";
                DataTable syscnfgdt = masterHelp.SQLquery(sql1);
                if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                {
                    VE.RETDEBSLCD = syscnfgdt.Rows[0]["retdebslcd"].retStr();
                }

                var jobdata = (from a in DB.M_JOBMST where a.JOBCD == sl.JOBCD select new { a.JOBNM, a.EXPGLCD, a.HSNCODE }).FirstOrDefault();
                if (jobdata != null)
                {
                    VE.JOBNM = jobdata.JOBNM;
                    VE.JOBEXPGLCD = jobdata.EXPGLCD;
                    VE.JOBHSNCODE = jobdata.HSNCODE;
                }

                VE.TAXABVAL = tDtl.TXBLVAL.retDbl();
                VE.GSTPER = tDtl.IGSTPER.retDbl() + tDtl.CGSTPER.retDbl() + tDtl.SGSTPER.retDbl();
                VE.IGSTPER = tDtl.IGSTPER.retDbl();
                VE.TOTTAX = tDtl.IGSTAMT.retDbl() + tDtl.CGSTAMT.retDbl() + tDtl.SGSTAMT.retDbl();
                VE.RoundOff = sl.ROYN == "Y" ? true : false;
                VE.INC_RATE = sl.INCL_RATE == "Y".retStr() ? true : false;
                VE.INCLRATEASK = sl.INCL_RATE.retStr();

                if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
                string Scm = CommVar.CurSchema(UNQSNO);
                string str2 = "select a.SLNO,a.PYMTCD,c.PYMTNM,a.AMT,a.CARDNO,a.INSTNO,a.INSTDT,a.PYMTREM,a.GLCD,c.PYMTTYPE from " + Scm + ".t_txnpymt a," + Scm + ".t_txnpymt_hdr b," + Scm + ".m_payment c ";
                str2 += "where a.autono=b.autono and  a.PYMTCD=c.PYMTCD and a.autono='" + sl.AUTONO + "' order by a.PYMTCD ";
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
                double T_PYMT_AMT = 0;

                for (int p = 0; p <= VE.TTXNPYMT.Count - 1; p++)
                {
                    T_PYMT_AMT = T_PYMT_AMT + VE.TTXNPYMT[p].AMT.Value;

                }
                VE.T_PYMT_AMT = T_PYMT_AMT;
                VE.PAYAMT = T_PYMT_AMT.toRound(2);
            }
            return VE;
        }
        public ActionResult SearchPannelData(TranAltBill VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            Cn.getQueryString(VE);

            List<DocumentType> DocumentType = new List<DocumentType>();
            DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
            string sql = "";

            sql = "select a.autono,a.BLAMT, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, c.slnm, c.district,c.regmobile ";
            sql += "from " + scm + ".T_txn a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_subleg c  ";
            sql += "where a.autono=b.autono and a.slcd=c.slcd(+) and b.doccd in (" + doccd + ") and ";
            if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
            if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
            sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
            sql += "order by docdt, docno ";
            DataTable tbl = masterHelp.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Registered Mobile No." + Cn.GCS() + "Bill Amount" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {
                SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] </td><td>" + tbl.Rows[j]["regmobile"] + " </td><td><b>" + tbl.Rows[j]["BLAMT"] + "</b> </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "5", "5"));
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
        public ActionResult GetJobDetails(string val, string Code)
        {
            try
            {
                //  DOCDT / TAXGRPCD
                string docdt = "", taxgrpcd = "", prodgrpcd = "", GSTPER = "", IGSTPER = "";
                if (!string.IsNullOrEmpty(Code))
                {
                    var tmpCode = Code.Split(Convert.ToChar(Cn.GCS()));
                    docdt = tmpCode[0];
                    taxgrpcd = tmpCode[1];
                }
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                var str = masterHelp.JOBCD_JOBMST_help(val, VE.DOC_CODE.retStr().retSqlformat());
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    prodgrpcd = str.retCompValue("prodgrpcd");
                    var dt = salesfunc.GetTax(docdt, taxgrpcd, prodgrpcd);
                    if (dt.Rows.Count > 0)
                    {
                        GSTPER = dt.Rows[0]["gstper"].retStr();
                        IGSTPER = dt.Rows[0]["IGSTPER"].retStr();
                        str += "^GSTPER=^" + GSTPER + Cn.GCS();
                        str += "^IGSTPER=^" + IGSTPER + Cn.GCS();
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
        private void FreightCharges(TranAltBill VE, string AUTO_NO)
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
        public ActionResult DeleteDOCRow(TranAltBill VE)
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
        public ActionResult cancelRecords(TranAltBill VE, string par1)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                Cn.getQueryString(VE);
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_CNTRL_HDR.AUTONO, CommVar.CurSchema(UNQSNO));
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_CNTRL_HDR.AUTONO, CommVar.CurSchema(UNQSNO), par1);
                    }
                    DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();
                    transaction.Commit();
                    return Content("1");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult ParkRecord(FormCollection FC, TranAltBill stream, string menuID, string menuIndex)
        {
            try
            {
                Connection cn = new Connection();
                string ID = menuID + menuIndex + CommVar.Loccd(UNQSNO) + CommVar.Compcd(UNQSNO) + CommVar.CurSchema(UNQSNO).ToString() + "*" + DateTime.Now;
                ID = ID.Replace(" ", "_");
                string Userid = Session["UR_ID"].ToString();
                INI Handel_ini = new INI();
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JR = javaScriptSerializer.Serialize(stream);
                Handel_ini.IniWriteValue(Userid, ID, cn.Encrypt(JR), Server.MapPath("~/Park.ini"));
                return Content("1");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        public string RetrivePark(string value)
        {
            try
            {
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TranAltBill");
                return url;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public ActionResult SAVE(FormCollection FC, TranAltBill VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //  Oracle Queries
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "";
            string[] dbsql1;
            string dberrmsg = "";

            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            DB.Configuration.ValidateOnSaveEnabled = false;
            try
            {
                DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();

                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string ContentFlg = "";
                if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                {
                    T_TXN TTXN = new T_TXN();
                    T_TXNMEMO TTXNMEMO = new T_TXNMEMO();
                    T_TXNPYMT_HDR TTXNPYMTHDR = new T_TXNPYMT_HDR();
                    T_TXNOTH TTXNOTH = new T_TXNOTH();
                    T_TXN_LINKNO TTXNLINKNO = new T_TXN_LINKNO();
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    T_CNTRL_DOC_PASS TCDP = new T_CNTRL_DOC_PASS();

                    string DOCPATTERN = "";
                    string docpassrem = "";
                    bool blactpost = true, blgstpost = true;
                    TTXN.DOCDT = VE.T_CNTRL_HDR.DOCDT;
                    string Ddate = Convert.ToString(TTXN.DOCDT);
                    TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                    string auto_no = ""; string Month = "";

                    #region Posting to finance Preparation
                    string expglcd = "";
                    string stkdrcr = "C", strqty = " Q=" + 0.ToString();
                    string trcd = "TR";
                    string revcharge = "";
                    string dr = ""; string cr = ""; int isl = 0; string strrem = "";
                    double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
                    double dbDrAmt = 0, dbCrAmt = 0;
                    /* string parglcd = "saldebglcd"*/
                    string parglcd = "", parclass1cd = "", class2cd = "", tcsgl = "", prodglcd = "", prodrglcd = "", rogl = "", glcd = "", rglcd = "", slmslcd = "";
                    string strblno = "", strbldt = "", strduedt = "", strrefno = "", strvtype = "BL";
                    dr = "D"; cr = "C";



                    TTXN.SLCD = VE.RETDEBSLCD;
                    if (TTXN.SLCD.retStr() == "")
                    {
                        dberrmsg = "Debtor/Creditor code not setup"; goto dbnotsave;
                    }
                    string sslcd = TTXN.SLCD;
                    if (VE.PSLCD.retStr() != "") sslcd = VE.PSLCD.ToString();

                    switch (VE.MENU_PARA)
                    {
                        case "SBCM":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Cash Memo" + strqty; break;
                        case "SBCMR":
                            stkdrcr = "D"; dr = "C"; cr = "D"; trcd = "SC"; strrem = "Cash Memo Credit Note" + strqty; break;
                    }

                    string slcdlink = "", slcdpara = VE.MENU_PARA;

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

                    //   Calculate Others Amount from amount tab to distrubute into txndtl table
                    double _amtdist = 0, _baldist = 0, _Rbaldist = 0, _rpldist = 0, _Rrpldist = 0, _mult = 1;
                    double _amtdistq = 0, _baldistq = 0, _Rbaldistq = 0, _rpldistq = 0, _Rrpldistq = 0;
                    double titamt = 0, titqty = 0, rtitamt = 0, rtitqty = 0;
                    int lastitemno = 0, rlastitemno = 0;
                    string negamt = "", proddrcr = "";
                    _baldist = _amtdist; _baldistq = _amtdistq; _Rbaldistq = _amtdistq; _Rbaldist = _amtdist;
                    #endregion
                    if (VE.DefaultAction == "A")
                    {
                        TTXN.EMD_NO = 0;
                        TTXN.DOCCD = VE.T_CNTRL_HDR.DOCCD;
                        TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                        auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
                        TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                        Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        //TempData["LASTGOCD" + VE.MENU_PARA] = VE.T_TXN.GOCD;
                        TCH = Cn.T_CONTROL_HDR(TTXN.DOCCD, TTXN.DOCDT, TTXN.DOCNO, TTXN.AUTONO, Month, DOCPATTERN, VE.DefaultAction, scm1, null, null, "".retDbl(), null);
                    }
                    else
                    {
                        TTXN.DOCCD = VE.T_CNTRL_HDR.DOCCD;
                        TTXN.DOCNO = VE.T_CNTRL_HDR.DOCONLYNO;
                        TTXN.AUTONO = VE.T_CNTRL_HDR.AUTONO;
                        Month = VE.T_CNTRL_HDR.MNTHCD;
                        TTXN.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                        DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        TTXN.DTAG = "E";
                    }
                    TTXN.DOCTAG = VE.MENU_PARA.retStr().Length > 2 ? VE.MENU_PARA.retStr().Remove(2) : VE.MENU_PARA.retStr();

                    TTXN.GOCD = "SHOP";// VE.T_TXN.GOCD;
                    TTXN.DUEDAYS = VE.T_TXN.DUEDAYS;
                    TTXN.PARGLCD = parglcd;
                    TTXN.CLASS1CD = parclass1cd;
                    TTXN.MENU_PARA = VE.T_TXN.MENU_PARA;
                    TTXN.REVCHRG = VE.T_TXN.REVCHRG;
                    TTXN.JOBCD = VE.T_TXN.JOBCD;
                    TTXN.ROAMT = VE.T_TXN.ROAMT;
                    TTXN.BLAMT = VE.T_TXN.BLAMT;
                    TTXN.INCL_RATE = VE.INC_RATE == true ? "Y" : "";
                    if (VE.RoundOff == true) { TTXN.ROYN = "Y"; } else { TTXN.ROYN = "N"; }
                    if (VE.DefaultAction == "E")
                    {

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
                    TTXNOTH.POREFNO = VE.T_TXNOTH.POREFNO;
                    TTXNOTH.POREFDT = VE.T_TXNOTH.POREFDT;
                    TTXNOTH.DOCREM = VE.T_TXNOTH.DOCREM;
                    //----------------------------------------------------------//

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

                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
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
                    if (blactpost == true || blgstpost == true)
                    {
                        Cn.Create_DOCCD(UNQSNO, "F", TTXN.DOCCD);
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
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
                        TTXNEWB.GOCD = TTXN.GOCD;
                        //----------------------------------------------------------//

                        dbsql = masterHelp.RetModeltoSql(TTXNEWB, action, CommVar.FinSchema(UNQSNO));
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    }

                    bool recoexist = false;
                    VE.BALEYR = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).Year.retStr();

                    T_TXNDTL TTXNDTL = new T_TXNDTL();
                    TTXNDTL.CLCD = TTXN.CLCD;
                    TTXNDTL.EMD_NO = TTXN.EMD_NO;
                    TTXNDTL.DTAG = TTXN.DTAG;
                    TTXNDTL.AUTONO = TTXN.AUTONO;
                    TTXNDTL.SLNO = 1;
                    TTXNDTL.MTRLJOBCD = "FS";
                    TTXNDTL.STKDRCR = cr;
                    TTXNDTL.STKTYPE = "F";
                    TTXNDTL.HSNCODE = VE.JOBHSNCODE;
                    TTXNDTL.GOCD = TTXN.GOCD;
                    TTXNDTL.TXBLVAL = VE.TAXABVAL; // IGSTPER
                    TTXNDTL.ITREM = VE.JOBNM;
                    double GSTPER = 0, GSTAMT = 0; ;
                    if (VE.IGSTPER == 0)
                    {
                        GSTPER = VE.GSTPER / 2;
                        GSTAMT = (VE.TAXABVAL * GSTPER / 100).toRound(2);
                        TTXNDTL.CGSTAMT = GSTAMT;
                        TTXNDTL.SGSTAMT = GSTAMT;
                        TTXNDTL.CGSTPER = GSTPER;
                        TTXNDTL.SGSTPER = GSTPER;
                        TTXNDTL.IGSTPER = 0;
                    }
                    else
                    {
                        GSTPER = VE.GSTPER;
                        GSTAMT = (VE.TAXABVAL * GSTPER / 100).toRound(2);
                        TTXNDTL.IGSTAMT = GSTAMT;
                        TTXNDTL.IGSTPER = GSTPER;
                        TTXNDTL.CGSTPER = 0;
                        TTXNDTL.SGSTPER = 0;
                    }
                    igst = igst + Convert.ToDouble(TTXNDTL.IGSTAMT);
                    cgst = cgst + Convert.ToDouble(TTXNDTL.CGSTAMT);
                    sgst = sgst + Convert.ToDouble(TTXNDTL.SGSTAMT);

                    TTXNDTL.NETAMT = VE.T_BLAMT;
                    TTXNDTL.GLCD = VE.JOBEXPGLCD;
                    dbsql = masterHelp.RetModeltoSql(TTXNDTL);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    isl = 1;
                    //if (AMTGLCD != null && AMTGLCD.Count > 0)
                    //{
                    //    for (int i = 0; i <= AMTGLCD.Count - 1; i++)
                    //   
                    string prodrem = ""; expglcd = "";
                    if (blactpost == true)
                    {
                        isl = isl + 1;
                        prodrem += (VE.MENU_PARA == "AT" ? "Alteration" : "Stiching ");
                        negamt = TTXNDTL.TXBLVAL.retDbl() < 0 ? "Y" : "N";
                        proddrcr = negamt == "Y" ? dr : cr;
                        if (negamt == "Y" && VE.MENU_PARA == "SBCMR") proddrcr = cr;

                        dbamt = TTXNDTL.TXBLVAL.retDbl() * (negamt == "Y" ? -1 : 1);
                        dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), proddrcr, TTXNDTL.GLCD, sslcd,
                                dbamt, prodrem, parglcd, TTXN.SLCD, 0.retDbl(), 0, 0);
                        OraCmd.CommandText = dbsql;
                        OraCmd.ExecuteNonQuery();

                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                               "", "", dbamt, 0, strrem);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        //itamt = itamt + dbamt.retDbl();
                        expglcd = TTXNDTL.GLCD;
                        if (proddrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                        else dbCrAmt = dbCrAmt + dbamt;
                    }
                    //    }
                    //}


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
                                            Convert.ToDouble(VE.TTXNAMT[i].AMT), strrem, parglcd, TTXN.SLCD, 0, 0, Convert.ToDouble(VE.TTXNAMT[i].CURR_AMT));
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    if (adrcr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);
                                    else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);
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

                    #region Document Passing checking
                    // -----------------------DOCUMENT PASSING DATA---------------------------//
                    double TRAN_AMT = Convert.ToDouble(TTXN.BLAMT);
                    if (docpassrem != "")
                    {
                        docpassrem = "Doc # " + TTXN.DOCNO.ToString() + " Party Name # " + VE.SLNM + " [" + "" + "]".ToString() + "~" + docpassrem;
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
                    string salpur = "";
                    double itamt = 0;
                    if (blactpost == true)
                    {
                        salpur = "S";
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
                                if (negamt == "Y" && VE.MENU_PARA == "SBCMR") proddrcr = cr;
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
                        ////  TCS
                        //dbamt = Convert.ToDouble(VE.T_TXN.TCSAMT);
                        //if (dbamt != 0)
                        //{
                        //    string adrcr = cr;

                        //    isl = isl + 1;
                        //    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tcsgl, sslcd,
                        //            dbamt, prodrem, parglcd, TTXN.SLCD, 0, 0, 0);
                        //    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        //    if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                        //    else dbCrAmt = dbCrAmt + dbamt;
                        //    dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                        //            null, null, Convert.ToDouble(VE.T_TXN.TCSAMT), 0, strrem + " TCS @ " + VE.T_TXN.TCSPER.ToString() + "%");
                        //    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        //}
                        ////Ronded off
                        //dbamt = Convert.ToDouble(VE.T_TXN.ROAMT);
                        //if (dbamt != 0)
                        //{
                        //    string adrcr = cr;
                        //    if (dbamt < 0) adrcr = dr;
                        //    if (dbamt < 0) dbamt = dbamt * -1;

                        //    isl = isl + 1;
                        //    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, rogl, null,
                        //            dbamt, prodrem, parglcd, TTXN.SLCD, 0, 0, 0);
                        //    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        //    if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                        //    else dbCrAmt = dbCrAmt + dbamt;
                        //}

                        ////  Party wise posting
                        //isl = isl + 1;

                        //dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr,
                        //    parglcd, sslcd, dbamt, prodrem, prodglcd,
                        //    null, dbqty, 0, dbcurramt);
                        //OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        //if (dr == "D") dbDrAmt = dbDrAmt + dbamt.retDbl();
                        //else dbCrAmt = dbCrAmt + dbamt.retDbl();

                        //if (parclass1cd.retStr() != "" || class2cd.retStr() != "")
                        //{
                        //    dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                        //            parclass1cd, class2cd, dbamt.retDbl(), dbcurramt, strrem);
                        //    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        //}

                        //strblno = ""; strbldt = ""; strduedt = ""; strvtype = ""; strrefno = "";
                        //strvtype = "BL";
                        //strduedt = Convert.ToDateTime(TTXN.DOCDT.Value).AddDays(Convert.ToDouble(TTXN.DUEDAYS)).ToString().retDateStr();

                        //strbldt = TTXN.DOCDT.ToString();
                        //strblno = DOCPATTERN;

                        //string blconslcd = TTXN.CONSLCD;
                        //if (TTXN.SLCD != sslcd) blconslcd = TTXN.SLCD;
                        //if (blconslcd == sslcd) blconslcd = "";
                        //dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                        //       parglcd, sslcd, blconslcd, TTXNOTH.AGSLCD, parclass1cd, Convert.ToSByte(isl),
                        //        dbamt, strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), itamt, TTXNOTH.POREFNO,
                        //        TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), dbamt.retDbl(),
                        //        "", "", "", "", VE.T_TXNMEMO.RTDEBCD == null ? "" : VE.T_TXNMEMO.RTDEBCD);
                        //OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();



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
                        if (negamt == "Y" && VE.MENU_PARA == "SBCMR") proddrcr = dr;
                        dbamt = VE.T_TXN.BLAMT.retDbl() * 1;

                        dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), proddrcr,
                            parglcd, sslcd, dbamt, prodrem, prodglcd,
                            null, dbqty, 0, dbcurramt);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        if (dr == "D") dbDrAmt = dbDrAmt + dbamt.retDbl();
                        else dbCrAmt = dbCrAmt + dbamt.retDbl();

                        if (parclass1cd.retStr() != "" || class2cd.retStr() != "")
                        {
                            dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                    parclass1cd, class2cd, dbamt.retDbl(), dbcurramt, strrem);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        }

                        strblno = ""; strbldt = ""; strduedt = ""; strvtype = ""; strrefno = "";
                        strvtype = "BL";
                        strduedt = Convert.ToDateTime(TTXN.DOCDT.Value).AddDays(Convert.ToDouble(TTXN.DUEDAYS)).ToString().retDateStr();

                        strbldt = TTXN.DOCDT.ToString();
                        strblno = DOCPATTERN;

                        string blconslcd = TTXN.CONSLCD;
                        if (TTXN.SLCD != sslcd) blconslcd = TTXN.SLCD;
                        if (blconslcd == sslcd) blconslcd = "";
                        dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, proddrcr,
                               parglcd, sslcd, blconslcd, TTXNOTH.AGSLCD, parclass1cd, Convert.ToSByte(isl),
                                dbamt, strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), itamt, TTXNOTH.POREFNO,
                                TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), dbamt.retDbl(),
                                "", "", "", "", VE.T_TXNMEMO.RTDEBCD == null ? "" : VE.T_TXNMEMO.RTDEBCD);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();




                    }
                    if (blgstpost == true)
                    {
                        #region TVCHGST Table update    

                        int gs = 0;
                        string dncntag = ""; string exemptype = "";
                        double gblamt = TTXN.BLAMT.retDbl(); double groamt = TTXN.ROAMT.retDbl();

                        negamt = TTXN.BLAMT.retDbl() < 0 ? "Y" : "N";
                        proddrcr = negamt == "Y" ? dr : cr;
                        gblamt = gblamt.retDbl() * (negamt == "Y" ? -1 : 1);
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
                        TVCHGST.HSNCODE = TTXNDTL.HSNCODE;
                        TVCHGST.IGSTPER = TTXNDTL.IGSTPER.retDbl();
                        TVCHGST.IGSTAMT = TTXNDTL.IGSTAMT.retDbl();
                        TVCHGST.CGSTPER = TTXNDTL.CGSTPER.retDbl();
                        TVCHGST.CGSTAMT = TTXNDTL.CGSTAMT.retDbl();
                        TVCHGST.SGSTPER = TTXNDTL.SGSTPER.retDbl();
                        TVCHGST.SGSTAMT = TTXNDTL.SGSTAMT.retDbl();
                        //TVCHGST.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;
                        //TVCHGST.ITNM = (VE.TsalePos_TBATCHDTL[i].ITNM.retStr() + " ").TrimStart(' ') + VE.TsalePos_TBATCHDTL[i].ITSTYLE;
                        //TVCHGST.AMT = VE.TsalePos_TBATCHDTL[i].TXBLVAL.retDbl();
                        //TVCHGST.CGSTPER = VE.TsalePos_TBATCHDTL[i].CGSTPER;
                        //TVCHGST.SGSTPER = VE.TsalePos_TBATCHDTL[i].SGSTPER;
                        //TVCHGST.IGSTPER = VE.TsalePos_TBATCHDTL[i].IGSTPER;
                        //TVCHGST.CGSTAMT = VE.TsalePos_TBATCHDTL[i].CGSTAMT;
                        //TVCHGST.SGSTAMT = VE.TsalePos_TBATCHDTL[i].SGSTAMT;
                        //TVCHGST.IGSTAMT = VE.TsalePos_TBATCHDTL[i].IGSTAMT;
                        //TVCHGST.CESSPER = VE.TsalePos_TBATCHDTL[i].CESSPER;
                        //TVCHGST.CESSAMT = VE.TsalePos_TBATCHDTL[i].CESSAMT;
                        //TVCHGST.DRCR = proddrcr;//cr;
                        //TVCHGST.QNTY = (VE.TsalePos_TBATCHDTL[i].BLQNTY.retDbl() == 0 ? VE.TsalePos_TBATCHDTL[i].QNTY.retDbl() : VE.TsalePos_TBATCHDTL[i].BLQNTY.retDbl());
                        //TVCHGST.UOM = VE.TsalePos_TBATCHDTL[i].UOM;
                        //TVCHGST.AGSTDOCNO = VE.TsalePos_TBATCHDTL[i].AGDOCNO;
                        //TVCHGST.AGSTDOCDT = VE.TsalePos_TBATCHDTL[i].AGDOCDT.retStr() == "" ? (DateTime?)null : Convert.ToDateTime(VE.TsalePos_TBATCHDTL[i].AGDOCDT);
                        //TVCHGST.SALPUR = salpur;
                        //TVCHGST.INVTYPECD = VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD;
                        //TVCHGST.DNCNCD = TTXNOTH.DNCNCD;
                        //TVCHGST.EXPCD = VE.T_VCH_GST.EXPCD;
                        //TVCHGST.GSTSLNM = VE.GSTSLNM;
                        //TVCHGST.GSTNO = VE.GSTNO;
                        //TVCHGST.POS = VE.POS;
                        //TVCHGST.SHIPDOCNO = VE.T_VCH_GST.SHIPDOCNO;
                        //TVCHGST.SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT;
                        //TVCHGST.PORTCD = VE.T_VCH_GST.PORTCD;
                        //TVCHGST.OTHRAMT = 0;
                        //TVCHGST.ROAMT = groamt;
                        //TVCHGST.BLAMT = gblamt;
                        //TVCHGST.DNCNSALPUR = dncntag;
                        //TVCHGST.CONSLCD = TTXN.CONSLCD;
                        //TVCHGST.APPLTAXRATE = 0;
                        //TVCHGST.EXEMPTEDTYPE = exemptype;
                        //TVCHGST.EXPGLCD = VE.TsalePos_TBATCHDTL[i].GLCD;
                        //TVCHGST.INPTCLAIM = "Y";
                        //TVCHGST.LUTNO = VE.T_VCH_GST.LUTNO;
                        //TVCHGST.LUTDT = VE.T_VCH_GST.LUTDT;
                        //TVCHGST.TCSPER = TTXN.TCSPER;
                        //TVCHGST.BASAMT = VE.TsalePos_TBATCHDTL[i].GROSSAMT;
                        //TVCHGST.DISCAMT = VE.TsalePos_TBATCHDTL[i].DISCAMT;
                        //TVCHGST.RATE = VE.TsalePos_TBATCHDTL[i].RATE;

                        //TVCHGST.GSTSLADD1 = VE.GSTSLADD1;
                        //TVCHGST.GSTSLDIST = VE.GSTSLDIST;
                        //TVCHGST.GSTSLPIN = VE.GSTSLPIN;
                        dbsql = masterHelp.RetModeltoSql(TVCHGST, "A", CommVar.FinSchema(UNQSNO));
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        gblamt = 0; groamt = 0;

                        #endregion
                    }
                    #region Payment tab
                    double recdamt = 0; short pslno = 0, adjslno = 0;

                    if (VE.TTXNPYMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                        {
                            if (VE.TTXNPYMT[i].GLCD.retStr() != "" && VE.TTXNPYMT[i].AMT.retDbl() != 0)
                            {
                                string pymtrem = "Cash";
                                pslno++; adjslno++;

                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(pslno + 100), cr,
                                    parglcd, sslcd, VE.TTXNPYMT[i].AMT.retDbl(), pymtrem, VE.TTXNPYMT[i].GLCD);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                if (parclass1cd.retStr() != "" || class2cd.retStr() != "")
                                {
                                    dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(pslno + 100), sslcd,
                                            parclass1cd, class2cd, VE.TTXNPYMT[i].AMT.retDbl());
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                }

                                dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                   parglcd, sslcd, null, TTXNOTH.AGSLCD, parclass1cd, Convert.ToSByte(pslno + 100),
                                    VE.TTXNPYMT[i].AMT.retDbl(), strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), 0, TTXNOTH.POREFNO,
                                    TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), VE.TTXNPYMT[i].AMT.retDbl(),
                                    "", "", "", "", VE.T_TXNMEMO.RTDEBCD == null ? "" : VE.T_TXNMEMO.RTDEBCD);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                                dbsql = masterHelp.InsVch_Bl_Adj(TTXN.AUTONO, TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(adjslno), TTXN.AUTONO, 1, dbamt.retDbl(), TTXN.AUTONO, Convert.ToSByte(pslno + 100), VE.TTXNPYMT[i].AMT.retDbl(), VE.TTXNPYMT[i].AMT.retDbl());
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
                    #endregion
                    if (Math.Round(dbDrAmt, 2) != Math.Round(dbCrAmt, 2))
                    {
                        dberrmsg = "Debit " + Math.Round(dbDrAmt, 2) + " & Credit " + Math.Round(dbCrAmt, 2) + " not matching please click on rounded off...";
                        goto dbnotsave;
                    }

                    if (VE.DefaultAction == "A")
                    {
                        ContentFlg = "1" + " (Voucher No. " + DOCPATTERN + ")~" + TTXN.AUTONO;
                    }
                    else if (VE.DefaultAction == "E")
                    {
                        ContentFlg = "2";
                    }
                    OraTrans.Commit();
                    OraCon.Dispose();
                    return Content(ContentFlg);
                }
                else if (VE.DefaultAction == "V")
                {
                    dbsql = masterHelp.TblUpdt("t_txnoth", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("T_TXNPYMT_HDR", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("T_TXNMEMO", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnpymt", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnamt", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txndtl", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txn", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    //   Delete from financial schema
                    dbsql = masterHelp.finTblUpdt("T_TXNEWB", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_auth", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.finTblUpdt("t_vch_gst", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_bl_adj", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_bl", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_class", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_det", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_hdr", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_cntrl_hdr", VE.T_CNTRL_HDR.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_CNTRL_HDR.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retDateStr(), null, null, null);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                    OraTrans.Commit();
                    OraCon.Dispose();
                    return Content("3");
                }
                else
                {
                    return Content("");
                }
                dbnotsave:;
                OraTrans.Rollback();
                OraCon.Dispose();
                return Content(dberrmsg);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                OraTrans.Rollback();
                OraCon.Dispose();
                return Content(ex.Message + ex.InnerException);
            }
        }

        public ActionResult Print(TranAltBill VE, FormCollection FC, string DOCNO, string DOC_CD, string DOCDT)
        {
            try
            {
                Cn.getQueryString(VE);
                ReportViewinHtml ind = new ReportViewinHtml();
                ind.DOCCD = DOC_CD;
                ind.FDOCNO = DOCNO;
                ind.TDOCNO = DOCNO;
                ind.FDT = DOCDT;
                ind.TDT = DOCDT;
                ind.MENU_PARA = VE.MENU_PARA;
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
    }
}
