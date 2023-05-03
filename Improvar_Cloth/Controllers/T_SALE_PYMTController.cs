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
    public class T_SALE_PYMTController : Controller
    {
        // GET: T_SALE_PYMT
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp(); SchemeCal Scheme_Cal = new SchemeCal(); Salesfunc salesfunc = new Salesfunc(); DataTable DT = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_TXNPYMT_HDR sl; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR;
        SMS SMS = new SMS(); string sql = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();

        public ActionResult T_SALE_PYMT(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {

                    SalePymtEntry VE = (parkID == "") ? new SalePymtEntry() : (Improvar.ViewModels.SalePymtEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.MENU_PARA)
                    {
                        case "PYMT":
                            ViewBag.formname = " Pymt Recd from Retail Debtor"; break;
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


                        VE.IndexKey = (from p in DB.T_TXNPYMT_HDR
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

                            VE.T_TXNPYMT_HDR = sl;
                            VE.T_CNTRL_HDR = TCH;
                            VE.T_CNTRL_HDR_REM = SLR;
                            if (VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            { //
                                T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                                TCH.DOCDT = Cn.getCurrentDate(VE.mindate);
                                VE.T_CNTRL_HDR = TCH;
                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;
                                T_TXNOTH TXNOTH = new T_TXNOTH(); T_TXNPYMT_HDR TXNMEMO = new T_TXNPYMT_HDR();
                                string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);
                                string sql = "";
                                sql += " select a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate,C.TAXGRPCD,a.retdebslcd,b.city,b.add1,b.add2,b.add3, a.effdt, d.prccd, e.prcnm ";
                                sql += "  from  " + scm + ".M_SYSCNFG a, " + scmf + ".M_RETDEB b, " + scm + ".M_SUBLEG_SDDTL c, " + scm + ".m_subleg_com d, " + scmf + ".m_prclst e ";
                                sql += " where a.RTDEBCD=b.RTDEBCD and  a.retdebslcd=d.slcd(+)  and  ";//and a.retdebslcd='DR00021'
                                sql += "a.retdebslcd=C.SLCD(+) and c.compcd='" + COM + "' and c.loccd='" + LOC + "' and d.prccd=e.prccd(+) ";

                                DataTable syscnfgdt = masterHelp.SQLquery(sql);
                                if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                                {
                                    TXNMEMO.RTDEBCD = syscnfgdt.Rows[0]["RTDEBCD"].retStr();
                                    VE.RTDEBNM = syscnfgdt.Rows[0]["RTDEBNM"].retStr();
                                    var addrs = syscnfgdt.Rows[0]["add1"].retStr() + " " + syscnfgdt.Rows[0]["add2"].retStr() + " " + syscnfgdt.Rows[0]["add3"].retStr();
                                    VE.ADDR = addrs + "/" + syscnfgdt.Rows[0]["city"].retStr();
                                    VE.MOBILE = syscnfgdt.Rows[0]["MOBILE"].retStr();
                                    VE.INC_RATE = syscnfgdt.Rows[0]["INC_RATE"].retStr() == "Y" ? true : false;
                                    //VE.INCLRATEASK = syscnfgdt.Rows[0]["INC_RATE"].retStr();
                                    VE.RETDEBSLCD = syscnfgdt.Rows[0]["retdebslcd"].retStr();
                                    TXNOTH.TAXGRPCD = syscnfgdt.Rows[0]["TAXGRPCD"].retStr();
                                    TXNOTH.PRCCD = syscnfgdt.Rows[0]["prccd"].retStr();
                                    //VE.PRCNM = syscnfgdt.Rows[0]["prcnm"].retStr();
                                    //VE.EFFDT = syscnfgdt.Rows[0]["effdt"].retDateStr();
                                    string rtdebcd = "";
                                    if (VE.T_TXNPYMT_HDR!=null)
                                    { rtdebcd = VE.T_TXNPYMT_HDR.RTDEBCD; }
                                    else { rtdebcd = TXNMEMO.RTDEBCD; }
                                   
                                    VE = GetOutstInvoice(VE, VE.RETDEBSLCD, rtdebcd, "");
                                }
                                VE.T_TXNPYMT_HDR = TXNMEMO;

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
                                //GetInvoice(VE);
                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (SalePymtEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        VE.VECHLTYPE = masterHelp.VECHLTYPE();
                        VE.TRANSMODE = masterHelp.TRANSMODE();
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (TCH != null) if (TCH.DOCDT != null) docdt = TCH.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_CNTRL_HDR.DOCCD, docdt, VE.DefaultAction, VE.T_CNTRL_HDR.DOCONLYNO, VE);
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                SalePymtEntry VE = new SalePymtEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public SalePymtEntry Navigation(SalePymtEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            Cn.getQueryString(VE);
            sl = new T_TXNPYMT_HDR(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM();
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
                sl = DB.T_TXNPYMT_HDR.Find(aa[0].Trim());
                TCH = DB.T_CNTRL_HDR.Find(sl.AUTONO);

                VE.RTDEBNM = sl.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == sl.RTDEBCD).Select(b => b.RTDEBNM).FirstOrDefault();
                VE.MOBILE = sl.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == sl.RTDEBCD).Select(b => b.MOBILE).FirstOrDefault();
                var add1 = sl.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == sl.RTDEBCD).Select(b => b.ADD1).FirstOrDefault();
                var add2 = sl.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == sl.RTDEBCD).Select(b => b.ADD2).FirstOrDefault();
                var add3 = sl.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == sl.RTDEBCD).Select(b => b.ADD3).FirstOrDefault();
                var city = sl.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == sl.RTDEBCD).Select(b => b.CITY).FirstOrDefault();
                VE.ADDR = add1 + " " + add2 + " " + add3 + "/" + city;

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

                string str = "select a.SLMSLCD,b.SLNM,a.PER,a.ITAMT,a.BLAMT from " + scm + ".t_txnslsmn a," + scmf + ".m_subleg b ";
                str += "where a.SLMSLCD=b.slcd and a.autono='" + sl.AUTONO + "'";
                var SALSMN_DATA = masterHelp.SQLquery(str);
                VE.TTXNSLSMN = (from DataRow dr in SALSMN_DATA.Rows
                                select new TTXNSLSMN()
                                {
                                    SLMSLCD = dr["SLMSLCD"].retStr(),
                                    SLMSLNM = dr["SLNM"].retStr(),
                                    PER = dr["PER"].retDbl(),
                                    ITAMT = dr["ITAMT"].retDbl(),
                                    BLAMT = dr["BLAMT"].retDbl(),
                                }).ToList();
                double S_T_GROSS_AMT = 0; double T_BILL_AMT = 0;
                //c
                for (int p = 0; p <= VE.TTXNSLSMN.Count - 1; p++)
                {
                    VE.TTXNSLSMN[p].SLNO = (p + 1).retShort();

                    S_T_GROSS_AMT = S_T_GROSS_AMT + VE.TTXNSLSMN[p].ITAMT.Value;
                    T_BILL_AMT = T_BILL_AMT + VE.TTXNSLSMN[p].BLAMT.Value;
                }
                VE.T_ITAMT = S_T_GROSS_AMT.retDbl();
                VE.T_BLAMT = T_BILL_AMT.retDbl();

                string str2 = "select a.SLNO,a.PYMTCD,c.PYMTNM,a.AMT,a.CARDNO,a.INSTNO,a.INSTDT,a.PYMTREM,a.GLCD,c.PYMTTYPE from " + scm + ".t_txnpymt a," + scm + ".t_txnpymt_hdr b," + scm + ".m_payment c ";
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
                VE.T_CNTRL_HDR = TCH;
                VE = GetOutstInvoice(VE, VE.RETDEBSLCD, sl.RTDEBCD, sl.AUTONO);
                var TVCHBLADJ = DBF.T_VCH_BL_ADJ.Where(m => m.AUTONO == sl.AUTONO).ToList();
                double TOT_bill_AMT = 0, TOT_PRE_ADJ = 0, TOT_ADJ = 0, TOT_BAL = 0;
                foreach (var outs in VE.SLPYMTADJ)
                {
                    var adjdet = TVCHBLADJ.Where(t => t.I_AUTONO == outs.I_AUTONO && t.I_SLNO == outs.I_SLNO).FirstOrDefault();
                    if (adjdet != null)
                    {
                        outs.ADJ_AMT = adjdet.ADJ_AMT; // total pending
                        TOT_ADJ += outs.ADJ_AMT.retDbl();
                        outs.Checked = true;
                    }
                    TOT_bill_AMT += outs.AMT.retDbl(); TOT_PRE_ADJ += outs.PRE_ADJ_AMT.retDbl(); TOT_BAL += outs.BAL_AMT.retDbl();
                }
                if (VE.DefaultAction == "V")
                {
                    if (VE.SLPYMTADJ != null && VE.SLPYMTADJ.Count > 0)
                    {
                        VE.SLPYMTADJ = VE.SLPYMTADJ.Where(a => a.ADJ_AMT.retDbl() != 0).ToList();
                    }
                }
                //VE.TOT_AMT = TOT_bill_AMT; VE.TOT_PRE_ADJ = TOT_PRE_ADJ; VE.TOT_ADJ = TOT_ADJ; VE.TOT_BAL = TOT_BAL;
                VE.TOT_AMT = VE.SLPYMTADJ.Select(a => a.AMT).Sum().retDbl();
                VE.TOT_PRE_ADJ = VE.SLPYMTADJ.Select(a => a.PRE_ADJ_AMT).Sum().retDbl();
                VE.TOT_ADJ = VE.SLPYMTADJ.Select(a => a.ADJ_AMT).Sum().retDbl();
                VE.TOT_BAL = VE.SLPYMTADJ.Select(a => a.BAL_AMT).Sum().retDbl();

                if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            }
            return VE;
        }
        public ActionResult SearchPannelData(SalePymtEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            Cn.getQueryString(VE);

            List<DocumentType> DocumentType = new List<DocumentType>();
            DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
            string sql = "";

            sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.RTDEBCD, c.RTDEBNM, c.state,c.mobile ";
            sql += "from " + scm + ".T_TXNPYMT_HDR a, " + scm + ".t_cntrl_hdr b, " + scmf + ".M_RETDEB c  ";
            sql += "where a.autono=b.autono and a.RTDEBCD=c.RTDEBCD(+) and b.doccd in (" + doccd + ") and ";
            //if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
            //if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
            //if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
            //if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
            sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
            sql += "order by docdt, docno ";
            DataTable tbl = masterHelp.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Registered Mobile No." + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {
                SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["RTDEBNM"] + "</b> [" + tbl.Rows[j]["state"] + "] (" + tbl.Rows[j]["RTDEBCD"] + ") </td><td>" + tbl.Rows[j]["mobile"] + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
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
        public ActionResult GetInvoice(SalePymtEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            try
            {
                //GetOutstInvoice(VE);
                Cn.getQueryString(VE);
                //VE.PARENT_SLNO = slno.retInt();
                string glcd = "";// VE.T_PYTHDR.GLCD;
                string slcd = VE.RETDEBSLCD;
                string autono = "";// VE.T_PYTHDR?.AUTONO;
                var OSDATA = masterHelp.GenOSTbl(glcd, slcd, VE.T_CNTRL_HDR.DOCDT.ToString(), "", "", "", "", "", "Y", "", "", "", "", "", false, false, "", "", "", "", autono, "");
                var RTR = OSDATA.Rows[0]["slno"].GetType();
                var OSList = (from customer in OSDATA.AsEnumerable()
                              where (customer.Field<string>("VCHTYPE") == "BL")
                              select new SLPYMTADJ
                              {
                                  DOCNO = customer.Field<string>("VCHTYPE"),
                                  DOCDT = customer.Field<DateTime>("docdt").retDateStr(),
                                  BILLNO = customer.Field<string>("BLNO").retStr() == "" ? customer.Field<string>("doccd") + customer.Field<string>("docno") : customer.Field<string>("BLNO"),
                                  BILLDT = customer.Field<string>("BLDT").retStr() == "" ? customer.Field<DateTime>("docdt").retDateStr() : customer.Field<string>("BLDT"),
                                  I_AUTONO = customer.Field<string>("AUTONO"),
                                  I_SLNO = customer.Field<Int16>("SLNO"),
                                  //re = customer.Field<string>("BLREM"),                       
                                  I_AMT = (customer.Field<decimal>("AMT") * -1).retDbl(),
                                  PRE_ADJ_AMT = (customer.Field<decimal>("PRV_ADJ") * -1).retDbl(),
                                  BAL_AMT = (customer.Field<decimal>("bal_amt") * -1).retDbl(),
                                  //DUE_DT = (customer.Field<decimal>("bal_amt") * -1).retDbl(),
                              }).ToList();
                VE.SLPYMTADJ = OSList;
            }
            catch
            {

            }
            //var doctP = (from i in DB1.MS_DOCCTG
            //             select new DocumentType()
            //             {
            //                 value = i.DOC_CTG,
            //                 text = i.DOC_CTG
            //             }).OrderBy(s => s.text).ToList();
            //if (VE.UploadDOC == null)
            //{
            //    List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
            //    UploadDOC MLI = new UploadDOC();
            //    MLI.DocumentType = doctP;
            //    MLocIFSC1.Add(MLI);
            //    VE.UploadDOC = MLocIFSC1;
            //}
            //else
            //{
            //    List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
            //    for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
            //    {
            //        UploadDOC MLI = new UploadDOC();
            //        MLI = VE.UploadDOC[i];
            //        MLI.DocumentType = doctP;
            //        MLocIFSC1.Add(MLI);
            //    }
            //    UploadDOC MLI1 = new UploadDOC();
            //    MLI1.DocumentType = doctP;
            //    MLocIFSC1.Add(MLI1);
            //    VE.UploadDOC = MLocIFSC1;
            //}
            VE.DefaultView = true;
            ModelState.Clear();
            return PartialView("_T_SALE_PYMT_Adjustment", VE);

        }
        public SalePymtEntry GetOutstInvoice(SalePymtEntry VE, string slcd, string rtdebcd, string autono)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            try
            {
                Cn.getQueryString(VE);
                string glcd = "";// VE.T_PYTHDR.GLCD;
                rtdebcd = rtdebcd.retStr() == "" ? "" : rtdebcd.retStr().retSqlformat();
                string vhautoslno = "";
                if (VE.DefaultAction == "V")
                {
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    vhautoslno = string.Join(",", DBF.T_VCH_DET.Where(a => a.AUTONO == autono).Select(s => s.AUTONO + s.SLNO).ToArray()).retSqlformat();
                }
                var OSDATA = masterHelp.GenOSTbl(glcd, slcd, VE.T_CNTRL_HDR.DOCDT.retDateStr(), "", vhautoslno, "", "", "", "Y", "", "", "", "", "", false, false, "", "", "", "", autono, rtdebcd);
                if(OSDATA.Rows.Count<0)
                { var RTR = OSDATA.Rows[0]["slno"].GetType(); }
               
                var OSList = (from customer in OSDATA.AsEnumerable()
                              where (customer.Field<string>("VCHTYPE") == "BL")
                              select new SLPYMTADJ
                              {
                                  DOCNO = customer.Field<string>("docno"),
                                  DOCDT = customer.Field<DateTime>("docdt").retDateStr(),
                                  BILLNO = customer.Field<string>("BLNO").retStr() == "" ? customer.Field<string>("doccd") + customer.Field<string>("docno") : customer.Field<string>("BLNO"),
                                  BILLDT = customer.Field<string>("BLDT").retStr() == "" ? customer.Field<DateTime>("docdt").retDateStr() : customer.Field<string>("BLDT"),
                                  I_AUTONO = customer.Field<string>("AUTONO"),
                                  I_SLNO = customer.Field<Int16>("SLNO"),
                                  //re = customer.Field<string>("BLREM"),                       
                                  AMT = (customer.Field<decimal>("AMT")).retDbl(),
                                  PRE_ADJ_AMT = (customer.Field<decimal>("PRV_ADJ")).retDbl(),
                                  BAL_AMT = (customer.Field<decimal>("bal_amt")).retDbl(),
                                  DUE_DT = (customer.Field<string>("DUEDT")),
                              }).ToList();
                VE.SLPYMTADJ = OSList;

            }
            catch
            {

            }
            //var doctP = (from i in DB1.MS_DOCCTG
            //             select new DocumentType()
            //             {
            //                 value = i.DOC_CTG,
            //                 text = i.DOC_CTG
            //             }).OrderBy(s => s.text).ToList();
            //if (VE.UploadDOC == null)
            //{
            //    List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
            //    UploadDOC MLI = new UploadDOC();
            //    MLI.DocumentType = doctP;
            //    MLocIFSC1.Add(MLI);
            //    VE.UploadDOC = MLocIFSC1;
            //}
            //else
            //{
            //    List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
            //    for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
            //    {
            //        UploadDOC MLI = new UploadDOC();
            //        MLI = VE.UploadDOC[i];
            //        MLI.DocumentType = doctP;
            //        MLocIFSC1.Add(MLI);
            //    }
            //    UploadDOC MLI1 = new UploadDOC();
            //    MLI1.DocumentType = doctP;
            //    MLocIFSC1.Add(MLI1);
            //    VE.UploadDOC = MLocIFSC1;
            //}
            VE.DefaultView = true;
            ModelState.Clear();
            //return PartialView("_T_SALE_PYMT_Adjustment", VE);
            return VE;
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
        public ActionResult AddRowPYMT(SalePymtEntry VE, int COUNT, string TAG)
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
            return PartialView("_T_SALE_PYMT_PAYMENT", VE);
        }
        public ActionResult DeleteRowPYMT(SalePymtEntry VE)
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
                return PartialView("_T_SALE_PYMT_PAYMENT", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteRow(SalePymtEntry VE)
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
                return PartialView("_T_SALE_PYMT_SALESMAN", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddRow(SalePymtEntry VE, int COUNT, string TAG)
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
            return PartialView("_T_SALE_PYMT_SALESMAN", VE);
        }
        public ActionResult AddDOCRow(SalePymtEntry VE)
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

        public ActionResult DeleteDOCRow(SalePymtEntry VE)
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
        public ActionResult cancelRecords(SalePymtEntry VE, string par1)
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
                        TCH = Cn.T_CONTROL_HDR(VE.T_TXNPYMT_HDR.AUTONO, CommVar.CurSchema(UNQSNO));
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_TXNPYMT_HDR.AUTONO, CommVar.CurSchema(UNQSNO), par1);
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
        public ActionResult ParkRecord(FormCollection FC, SalePymtEntry stream, string menuID, string menuIndex)
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
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.SalePymtEntry");
                return url;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public ActionResult SAVE(FormCollection FC, SalePymtEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DBb = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string fscm = CommVar.FinSchema(UNQSNO);
            //Oracle Queries
            string dberrmsg = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            string dbsql = "";
            string[] dbsql1;
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();

                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        T_TXNPYMT_HDR TBHDR = new T_TXNPYMT_HDR();
                        T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                        T_VCH_HDR TVCHHDR = new T_VCH_HDR();
                        string DOCPATTERN = ""; string trcd = "TR";
                        TCH.DOCDT = VE.T_CNTRL_HDR.DOCDT;
                        TCH.SLCD = VE.RETDEBSLCD;
                        string Ddate = Convert.ToString(TCH.DOCDT);
                        TBHDR.CLCD = CommVar.ClientCode(UNQSNO);
                        TBHDR.RTDEBCD = VE.T_TXNPYMT_HDR.RTDEBCD;
                        TBHDR.DRCR = "D";
                        string auto_no = ""; string Month = "";
                        if (VE.DefaultAction == "A")
                        {
                            TBHDR.EMD_NO = 0;
                            TCH.DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            TCH.DOCNO = Cn.MaxDocNumber(TCH.DOCCD, Ddate);
                            DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TCH.DOCNO), TCH.DOCCD, CommVar.CurSchema(UNQSNO), CommVar.FinSchema(UNQSNO), Ddate);
                            auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TCH.DOCNO, TCH.DOCCD, Ddate);
                            TBHDR.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            TCH.AUTONO = TBHDR.AUTONO;
                            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        }
                        else
                        {
                            DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                            TCH.DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            TCH.DOCNO = VE.T_CNTRL_HDR.DOCONLYNO;
                            TBHDR.AUTONO = VE.T_TXNPYMT_HDR.AUTONO;
                            TCH.AUTONO = VE.T_TXNPYMT_HDR.AUTONO;
                            Month = VE.T_CNTRL_HDR.MNTHCD;
                            var MAXEMDNO = (from p in DBb.T_CNTRL_HDR where p.AUTONO == VE.T_TXNPYMT_HDR.AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null) { TBHDR.EMD_NO = 0; } else { TBHDR.EMD_NO = Convert.ToInt16(MAXEMDNO + 1); }
                            TBHDR.DTAG = "E";
                        }

                        TBHDR.AUTONO = TBHDR.AUTONO;
                        TBHDR.CLCD = TBHDR.CLCD;
                        TBHDR.AUTONO = TBHDR.AUTONO;
                        TBHDR.EMD_NO = TBHDR.EMD_NO;
                        TBHDR.DTAG = "E";
                        if (VE.DefaultAction == "E")
                        {

                            dbsql = masterHelp.TblUpdt("T_TXNPYMT", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("T_TXNSLSMN", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            //finance
                            dbsql = masterHelp.finTblUpdt("T_VCH_BL_ADJ", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                            dbsql = masterHelp.finTblUpdt("T_VCH_BL", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                            dbsql = masterHelp.finTblUpdt("t_vch_det", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                            dbsql = masterHelp.finTblUpdt("t_vch_hdr", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                        }

                        //----------------------------------------------------------//
                        //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TBHDR.AUTONO, VE.DefaultAction, "S", Month, TCH.DOCCD, DOCPATTERN, TCH.DOCDT.retStr(), TBHDR.EMD_NO.retShort(), TCH.DOCNO, Convert.ToDouble(TCH.DOCNO), null, null, null, VE.RETDEBSLCD);
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TBHDR.AUTONO, VE.DefaultAction, "S", Month, TCH.DOCCD, DOCPATTERN, TCH.DOCDT.retStr(), TBHDR.EMD_NO.retShort(), TCH.DOCNO, Convert.ToDouble(TCH.DOCNO), null, null, null, VE.RETDEBSLCD,TCH.DOCAMT,VE.Audit_REM);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = masterHelp.RetModeltoSql(TBHDR, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        if (VE.TTXNPYMT != null)
                        {
                            for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                            {
                                if (VE.TTXNPYMT[i].SLNO != 0 && VE.TTXNPYMT[i].AMT.retDbl() != 0)
                                {
                                    T_TXNPYMT TTXNPYMNT = new T_TXNPYMT();
                                    TTXNPYMNT.AUTONO = TBHDR.AUTONO;
                                    TTXNPYMNT.SLNO = VE.TTXNPYMT[i].SLNO;
                                    TTXNPYMNT.EMD_NO = TBHDR.EMD_NO;
                                    TTXNPYMNT.CLCD = TBHDR.CLCD;
                                    TTXNPYMNT.DTAG = TBHDR.DTAG;
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
                        if (VE.TTXNSLSMN != null)
                        {
                            for (int i = 0; i <= VE.TTXNSLSMN.Count - 1; i++)
                            {
                                if (VE.TTXNSLSMN[i].SLNO != 0 && VE.TTXNSLSMN[i].SLMSLCD != null)
                                {
                                    T_TXNSLSMN TTXNSLSMN = new T_TXNSLSMN();
                                    TTXNSLSMN.AUTONO = TBHDR.AUTONO;
                                    TTXNSLSMN.EMD_NO = TBHDR.EMD_NO;
                                    TTXNSLSMN.CLCD = TBHDR.CLCD;
                                    TTXNSLSMN.DTAG = TBHDR.DTAG;
                                    TTXNSLSMN.SLNO = Convert.ToByte(VE.TTXNSLSMN[i].SLNO);
                                    TTXNSLSMN.SLMSLCD = VE.TTXNSLSMN[i].SLMSLCD;
                                    TTXNSLSMN.PER = VE.TTXNSLSMN[i].PER.retDbl();
                                    TTXNSLSMN.ITAMT = VE.TTXNSLSMN[i].ITAMT.retDbl();
                                    TTXNSLSMN.BLAMT = VE.TTXNSLSMN[i].BLAMT.retDbl();
                                    dbsql = masterHelp.RetModeltoSql(TTXNSLSMN);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        #region Adjustment
                        Cn.Create_DOCCD(UNQSNO, "F", TCH.DOCCD);
                        //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TBHDR.AUTONO, VE.DefaultAction, "F", Month, TCH.DOCCD, DOCPATTERN, TCH.DOCDT.retStr(), TCH.EMD_NO.retShort(), TCH.DOCNO, Convert.ToDouble(TCH.DOCNO), null, null, null, TCH.SLCD);
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TBHDR.AUTONO, VE.DefaultAction, "F", Month, TCH.DOCCD, DOCPATTERN, TCH.DOCDT.retStr(), TCH.EMD_NO.retShort(), TCH.DOCNO, Convert.ToDouble(TCH.DOCNO), null, null, null, TCH.SLCD,TCH.DOCAMT,VE.Audit_REM);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        double currrt = 0;
                        //if (TTXN.CURRRT != null) currrt = Convert.ToDouble(TTXN.CURRRT);
                        dbsql = masterHelp.InsVch_Hdr(TBHDR.AUTONO, TCH.DOCCD, TCH.DOCNO, TCH.DOCDT.ToString(), TCH.EMD_NO.retShort(), TCH.DTAG, null, null, "Y", null, trcd, "", "", "", currrt, "", "");
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        #region Payment tab
                        short pslno = 0, adjslno = 0;
                        string dr = "D", cr = "C";
                        string parglcd = "", parclass1cd = "", class2cd = "", tcsgl = "", prodglcd = "", prodrglcd = "", rogl = "", glcd = "", rglcd = "", slmslcd = "", strrefno = "";
                        string sslcd = VE.RETDEBSLCD;
                        string strblno = DOCPATTERN;
                        string strbldt = TCH.DOCDT.ToString();
                        string strduedt = Convert.ToDateTime(TCH.DOCDT.Value).AddDays(0).ToString().retDateStr();
                        string strvtype = "BL";

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

                        //List<TTXNPYMT> vchadtTemp = new List<TTXNPYMT>();

                        if (VE.TTXNPYMT != null)
                        {
                            for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                            {
                                if (VE.TTXNPYMT[i].GLCD.retStr() != "" && VE.TTXNPYMT[i].AMT.retDbl() != 0)
                                {
                                    //string pymtrem = "Cash";
                                    //string pymtrem = VE.TTXNPYMT[i].PYMTNM;
                                    string pymtrem = VE.RTDEBNM.retStr()+" "+ VE.TTXNPYMT[i].PYMTREM.retStr();
                                    pslno++; adjslno++;
                                    dbsql = masterHelp.InsVch_Det(TCH.AUTONO, TCH.DOCCD, TCH.DOCNO, TCH.DOCDT.ToString(), TCH.EMD_NO.retShort(), TCH.DTAG, Convert.ToSByte(pslno + 100), cr,
                                        parglcd, sslcd, VE.TTXNPYMT[i].AMT.retDbl(), pymtrem, VE.TTXNPYMT[i].GLCD);
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    if (parclass1cd.retStr() != "" || class2cd.retStr() != "")
                                    {
                                        dbsql = masterHelp.InsVch_Class(TCH.AUTONO, TCH.DOCCD, TCH.DOCNO, TCH.DOCDT.ToString(), TCH.EMD_NO.retShort(), TCH.DTAG, 1, Convert.ToSByte(pslno + 100), sslcd,
                                                parclass1cd, class2cd, VE.TTXNPYMT[i].AMT.retDbl());
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    }
                                    string agslcd = VE.TTXNSLSMN == null ? "" : VE.TTXNSLSMN[0].SLMSLCD;
                                    dbsql = masterHelp.InsVch_Bl(TCH.AUTONO, TCH.DOCCD, TCH.DOCNO, TCH.DOCDT.ToString(), TCH.EMD_NO.retShort(), TCH.DTAG, cr,
                                       parglcd, sslcd, null, agslcd, parclass1cd, Convert.ToSByte(pslno + 100),
                                        VE.TTXNPYMT[i].AMT.retDbl(), strblno, strbldt, strrefno, strduedt, strvtype, 0, 0, "",
                                         "", VE.TTXNPYMT[i].AMT.retDbl(),
                                        "", "", "", "", VE.T_TXNPYMT_HDR.RTDEBCD == null ? "" : VE.T_TXNPYMT_HDR.RTDEBCD);
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();



                                    T_VCH_DET TVCHDET = new T_VCH_DET();
                                    TVCHDET.AUTONO = TCH.AUTONO; TVCHDET.CLCD = TBHDR.CLCD; TVCHDET.DTAG = TCH.DTAG; TVCHDET.EMD_NO = TCH.EMD_NO;
                                    TVCHDET.DOCCD = TCH.DOCCD; TVCHDET.DOCDT = TCH.DOCDT; TVCHDET.DOCNO = TCH.DOCNO;
                                    TVCHDET.SLNO = pslno + 200; TVCHDET.DRCR = dr; TVCHDET.GLCD = VE.TTXNPYMT[i].GLCD; TVCHDET.AMT = VE.TTXNPYMT[i].AMT; TVCHDET.R_SLCD = sslcd; TVCHDET.R_GLCD = parglcd;
                                    TVCHDET.T_REM = pymtrem; TVCHDET.CHQNO = VE.TTXNPYMT[i].INSTNO; TVCHDET.OSLNO = pslno + 100;
                                    if (VE.TTXNPYMT[i].INSTDT.retDateStr() != "") TVCHDET.CHQDT = Convert.ToDateTime(VE.TTXNPYMT[i].INSTDT.retDateStr());
                                    dbsql = masterHelp.RetModeltoSql(TVCHDET, "A", scmf);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                    //vchadtTemp.Add(new TTXNPYMT() { SLNO = (pslno + 100).retShort(), AMT = TVCHDET.AMT, ADJAMT = 0 });
                                }
                            }
                        }
                        #endregion

                        if (VE.SLPYMTADJ != null && VE.SLPYMTADJ.Count > 0)
                        {
                            int COUNTERADJ = 0;
                            for (int i = 0; i <= VE.SLPYMTADJ.Count - 1; i++)
                            {
                                if (VE.SLPYMTADJ[i].Checked == true && VE.SLPYMTADJ[i].I_SLNO != 0 && VE.SLPYMTADJ[i].ADJ_AMT != null && VE.SLPYMTADJ[i].ADJ_AMT != 0)
                                {
                                    //double willAdjust = 0;
                                    T_VCH_BL_ADJ TVCHBLADJ = new T_VCH_BL_ADJ();
                                    TVCHBLADJ.EMD_NO = TBHDR.EMD_NO;
                                    TVCHBLADJ.DTAG = TBHDR.DTAG;
                                    TVCHBLADJ.CLCD = TBHDR.CLCD;
                                    TVCHBLADJ.AUTONO = TBHDR.AUTONO;
                                    TVCHBLADJ.SLNO = ++COUNTERADJ;
                                    TVCHBLADJ.I_AUTONO = VE.SLPYMTADJ[i].I_AUTONO;
                                    TVCHBLADJ.I_SLNO = VE.SLPYMTADJ[i].I_SLNO;
                                    TVCHBLADJ.I_AMT = VE.SLPYMTADJ[i].I_AMT;
                                    TVCHBLADJ.R_AUTONO = TBHDR.AUTONO;
                                    //var curadj = vchadtTemp.Where(m => m.ADJAMT != 0).FirstOrDefault();
                                    //vchadtTemp.Where(m => m.SLNO == curadj.SLNO).ForEach(m => m.ADJAMT = 0);
                                    TVCHBLADJ.R_SLNO = 101;
                                    TVCHBLADJ.R_AMT = VE.T_PYMT_AMT; // + Convert.ToDouble(VE.TOTAL_DISCAMT_AMT);
                                    //if(curadj.ADJAMT> VE.SLPYMTADJ[i].ADJ_AMT)
                                    TVCHBLADJ.ADJ_AMT = VE.SLPYMTADJ[i].ADJ_AMT;
                                    dbsql = masterHelp.RetModeltoSql(TVCHBLADJ, "A", fscm);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                }
                            }
                        }

                        //if (VE.TTXNPYMT != null)
                        //{
                        //    for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                        //    {
                        //        if (VE.TTXNPYMT[i].SLNO != 0 && VE.TTXNPYMT[i].AMT.retDbl() != 0)
                        //        {
                        //            dbsql = masterHelp.InsVch_Det(TCH.AUTONO, TCH.DOCCD, TCH.DOCNO, TCH.DOCDT.ToString(), TCH.EMD_NO.retShort(), TCH.DTAG, Convert.ToSByte(1), "D", VE.TTXNPYMT[i].GLCD, TCH.SLCD,
                        //      VE.TOT_ADJ, "Rem", "", TCH.SLCD, 0, 0, 0);
                        //            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        //        }
                        //    }
                        //}

                        //dbsql = masterHelp.InsVch_Bl(TCH.AUTONO, TCH.DOCCD, TCH.DOCNO, TCH.DOCDT.ToString(), TCH.EMD_NO.retShort(), TCH.DTAG, "D",
                        //        "", TCH.SLCD, "", "", "", Convert.ToSByte(1), VE.T_PYMT_AMT, "", "", "", "", "", 0, 0, "", "", VE.T_PYMT_AMT, "", "", "");
                        //OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        //if (VE.SLPYMTADJ != null && VE.SLPYMTADJ.Count > 0)
                        //{
                        //    int COUNTERADJ = 0;
                        //    for (int i = 0; i <= VE.SLPYMTADJ.Count - 1; i++)
                        //    {
                        //        if (VE.SLPYMTADJ[i].Checked == true && VE.SLPYMTADJ[i].I_SLNO != 0 && VE.SLPYMTADJ[i].ADJ_AMT != null && VE.SLPYMTADJ[i].ADJ_AMT != 0)
                        //        {
                        //            T_VCH_BL_ADJ TVCHBLADJ = new T_VCH_BL_ADJ();
                        //            TVCHBLADJ.EMD_NO = TBHDR.EMD_NO;
                        //            TVCHBLADJ.CLCD = TBHDR.CLCD;
                        //            TVCHBLADJ.AUTONO = TBHDR.AUTONO;
                        //            TVCHBLADJ.SLNO = ++COUNTERADJ;
                        //            TVCHBLADJ.I_AUTONO = VE.SLPYMTADJ[i].I_AUTONO;
                        //            TVCHBLADJ.I_SLNO = VE.SLPYMTADJ[i].I_SLNO;
                        //            TVCHBLADJ.I_AMT = VE.SLPYMTADJ[i].I_AMT;
                        //            TVCHBLADJ.R_AUTONO = TBHDR.AUTONO;
                        //            TVCHBLADJ.R_SLNO = 1;
                        //            TVCHBLADJ.R_AMT = VE.T_PYMT_AMT; // + Convert.ToDouble(VE.TOTAL_DISCAMT_AMT);
                        //            TVCHBLADJ.ADJ_AMT = VE.SLPYMTADJ[i].ADJ_AMT;
                        //            dbsql = masterHelp.RetModeltoSql(TVCHBLADJ, "A", fscm);
                        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        //        }
                        //    }
                        //}

                        #endregion

                        if (VE.UploadDOC != null)// add
                        {
                            var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TBHDR.AUTONO, TBHDR.EMD_NO.Value);
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
                            var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TBHDR.AUTONO, TBHDR.CLCD, TBHDR.EMD_NO.Value);
                            if (NOTE.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
                                {
                                    dbsql = masterHelp.RetModeltoSql(NOTE.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }


                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1" + " (Doc No.: " + DOCPATTERN + ")~" + TBHDR.AUTONO;
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
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_TXNPYMT_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_TXNPYMT_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_TXNPYMT_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("T_TXNPYMT", VE.T_TXNPYMT_HDR.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("T_TXNSLSMN", VE.T_TXNPYMT_HDR.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("T_TXNPYMT_HDR", VE.T_TXNPYMT_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("T_TXNTRANS", VE.T_TXNPYMT_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("T_txn", VE.T_TXNPYMT_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXNPYMT_HDR.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null);
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXNPYMT_HDR.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null,null,null,null,null,0,VE.Audit_REM);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                        dbsql = masterHelp.finTblUpdt("T_VCH_BL_ADJ", VE.T_TXNPYMT_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("T_VCH_BL", VE.T_TXNPYMT_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_det", VE.T_TXNPYMT_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_hdr", VE.T_TXNPYMT_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXNPYMT_HDR.AUTONO, "D", "F", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null);
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXNPYMT_HDR.AUTONO, "D", "F", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null,null,null,null,null,0,VE.Audit_REM);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


                        ModelState.Clear();
                        OraTrans.Commit();
                        OraCon.Dispose();
                        return Content("3");
                    }
                    else
                    {
                        OraTrans.Rollback();
                        OraCon.Dispose();
                        return Content("");
                    }
                    goto dbok;
                dbnotsave:;
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    return Content(dberrmsg);
                dbok:;
                }
                catch (Exception ex)
                {
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        public ActionResult Print(SalePymtEntry VE, FormCollection FC, string DOCNO, string DOC_CD, string DOCDT)
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
        public ActionResult UpDateAdjustment(SalePymtEntry VE, string slcd, string rtdebcd)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Cn.getQueryString(VE);
            VE = GetOutstInvoice(VE, VE.RETDEBSLCD, rtdebcd, "");
           
            VE.DefaultView = true;
            return PartialView("_T_SALE_PYMT_Adjustment", VE);
        }
    }
}
