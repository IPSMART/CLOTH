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
    public class T_Bilty_KhasraController : Controller
    {
        // GET: T_Bilty_Khasra
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp(); MasterHelpFa MasterHelpFa = new MasterHelpFa(); SchemeCal Scheme_Cal = new SchemeCal(); Salesfunc salesfunc = new Salesfunc(); DataTable DT = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_BALE_HDR TBH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR; T_TXNTRANS TXNTRN; T_TXN TXN;
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_Bilty_Khasra
        public ActionResult T_Bilty_Khasra(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {

                    TransactionKhasraEntry VE = (parkID == "") ? new TransactionKhasraEntry() : (Improvar.ViewModels.TransactionKhasraEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.MENU_PARA)
                    {
                        case "KHSR":
                            ViewBag.formname = "Khasra"; break;
                        case "TRFB":
                            ViewBag.formname = "Sotck Transfer Bale"; break;
                        case "TRWB":
                            ViewBag.formname = "Stk Trnf with Waybill (Bale)"; break;
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

                        VE.IndexKey = (from p in DB.T_BALE_HDR
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

                            VE.T_BALE_HDR = TBH;
                            VE.T_TXN = TXN;
                            VE.T_TXNTRANS = TXNTRN;
                            VE.T_CNTRL_HDR = TCH;
                            VE.T_CNTRL_HDR_REM = SLR;
                            if (VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {
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
                            VE = (TransactionKhasraEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
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
                TransactionKhasraEntry VE = new TransactionKhasraEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionKhasraEntry Navigation(TransactionKhasraEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE);
            TXNTRN = new T_TXNTRANS(); TBH = new T_BALE_HDR(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM(); TXN = new T_TXN();
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
                TBH = DB.T_BALE_HDR.Find(aa[0].Trim());
                TXNTRN = DB.T_TXNTRANS.Find(aa[0].Trim());
                TCH = DB.T_CNTRL_HDR.Find(TBH.AUTONO);
                TXN = DB.T_TXN.Find(TBH.AUTONO);
                if (VE.MENU_PARA == "TRWB" || VE.MENU_PARA == "TRFB")
                {
                    string slcd = TBH.MUTSLCD;
                    string slcd1 = TXN.SLCD;
                    if (slcd != null) { var subleg = (from a in DBF.M_SUBLEG where a.SLCD == slcd select new { a.SLNM }).FirstOrDefault(); VE.SLNM = subleg.SLNM; }
                      
                    if (slcd1 != null)
                    {var subleg1 = (from a in DBF.M_SUBLEG where a.SLCD == slcd1 select new { a.SLNM }).FirstOrDefault(); VE.SLNM1 = subleg1.SLNM; }
                    
                   
                   
                }
                if (TXNTRN.TRANSLCD!=null)
                { var trnslcd = TXNTRN.TRANSLCD;
                    var subleg2 = (from a in DBF.M_SUBLEG where a.SLCD == trnslcd select new { a.SLNM }).FirstOrDefault(); VE.TRANSLNM = subleg2.SLNM;
                }
                var TBALE = DB.T_BALE.Where(t => t.AUTONO == TBH.AUTONO).FirstOrDefault();

                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TBH.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TBH.AUTONO);
                string Scm = CommVar.CurSchema(UNQSNO);
                string scmf = CommVar.FinSchema(UNQSNO);
                string str = "";
                str += "select a.autono,a.blautono,a.slno,a.drcr,a.lrdt,a.lrno,a.baleyr,a.baleno,a.blslno,decode(a.baleopen,'Y',b.gocd,a.gocd)gocd,decode(a.baleopen,'Y',b.gonm,a.gonm)gonm,a.flag1,  ";
                str += "a.itcd, a.styleno, a.itnm,a.uomcd,a.nos,a.qnty,a.rate,a.pageno,a.pageslno,a.itstyle,a.prefno,a.prefdt,a.baleopen from ( ";

                str += "select a.autono,a.blautono,a.slno,a.drcr,a.lrdt,a.lrno,a.baleyr,a.baleno,a.blslno,a.gocd,b.gonm,b.flag1,  ";
                str += "c.itcd, d.styleno, d.itnm,d.uomcd,c.nos,c.qnty,c.rate,c.pageno,c.pageslno,d.styleno||' '||d.itnm itstyle,e.prefno,e.prefdt,a.baleopen  ";
                str += " from " + Scm + ".T_BALE a," + scmf + ".M_GODOWN b, " + Scm + ".T_TXNDTL c," + Scm + ".M_SITEM d," + Scm + ".T_TXN e  ";
                str += " where a.blautono=c.autono(+) and c.itcd=d.itcd(+) and a.blautono=e.autono(+) and a.gocd=b.gocd(+)  and ";
                //str += "A.BLSLNO=c.slno and a.autono='" + TBH.AUTONO + "' and a.slno <= 1000 ";
                str += "A.BLSLNO=c.slno and a.autono='" + TBH.AUTONO + "' ";
                str += "order by a.slno ) a, ";

                str += "(select a.gocd,b.gonm,a.slno+1000  slno,a.autono from " + Scm + ".T_TXNDTL a," + scmf + ".M_GODOWN b where a.gocd=b.gocd and a.autono='" + TBH.AUTONO + "' and a.slno <= 1000)b ";
                str += "where a.autono=b.autono(+) and a.slno=b.slno(+)  ";

                DataTable TBILTYKHASRAtbl = masterHelp.SQLquery(str);
                VE.TBILTYKHASRA = (from DataRow dr in TBILTYKHASRAtbl.Rows
                                   where (dr["BALEOPEN"].retStr() == "Y" ? dr["slno"].retDbl() >= 1000 : dr["slno"].retDbl() <= 1000)
                                   select new TBILTYKHASRA()
                                   {
                                       SLNO = dr["BALEOPEN"].retStr() == "Y"? (dr["slno"].retDbl()-1000).retShort():Convert.ToInt16(dr["slno"]),
                                       BLAUTONO = dr["blautono"].retStr(),
                                       LRDT = dr["lrdt"].retDateStr(),
                                       LRNO = dr["lrno"].retStr(),
                                       BALENO = dr["baleno"].retStr(),
                                       BALEYR = dr["baleyr"].retStr(),
                                       BLSLNO = dr["blslno"].retShort(),
                                       GOCD = dr["gocd"].retStr(),
                                       GONM = dr["gonm"].retStr(),
                                       FLAG1 = dr["FLAG1"].retStr(),
                                       ITCD = dr["itcd"].retStr(),
                                       ITNM = dr["itstyle"].retStr(),
                                       UOMCD = dr["uomcd"].retStr(),
                                       NOS = dr["nos"].retStr(),
                                       QNTY = dr["qnty"].retStr(),
                                       RATE = dr["RATE"].retStr(),
                                       PAGENO = (dr["pageno"].retStr() == "" && dr["pageslno"].retStr() == "") ? "" : dr["pageno"].retStr() + "/" + dr["pageslno"].retStr(),
                                       PBLNO = dr["prefno"].retStr(),
                                       PBLDT = dr["prefdt"].retDateStr(),
                                       BALEOPEN = dr["BALEOPEN"].retStr(),
                                   }).OrderBy(s => s.SLNO).ToList();
                foreach (var v in VE.TBILTYKHASRA)
                {
                    v.CheckedBALEOPEN = v.BALEOPEN.retStr() == "Y" ? true : false;
                }
                //if (TBALE != null)
                //{
                //    VE.BALEOPEN = TBALE.BALEOPEN == "Y" ? true : false;
                //}
                if (TXN != null)
                {
                    VE.GONM = DBF.M_GODOWN.Where(a => a.GOCD == TXN.GOCD).Select(b => b.GONM).FirstOrDefault();
                }
                if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            }
            return VE;
        }
        public ActionResult SearchPannelData(TransactionKhasraEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            Cn.getQueryString(VE);

            List<DocumentType> DocumentType = new List<DocumentType>();
            DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
            string sql = "";

            sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.mutslcd, c.slnm, c.district,c.regmobile ";
            sql += "from " + scm + ".T_BALE_HDR a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_subleg c  ";
            sql += "where a.autono=b.autono and a.mutslcd=c.slcd(+) and b.doccd in (" + doccd + ") and ";
            if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
            if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
            sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
            sql += "order by docdt, docno ";
            DataTable tbl = masterHelp.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Registered Mobile No." + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {
                SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["mutslcd"] + ") </td><td>" + tbl.Rows[j]["regmobile"] + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
        }
        public ActionResult GetSubLedgerDetails(string val, string code)
        {
            try
            {
                var str = masterHelp.SLCD_help(val, code);
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
        public ActionResult GetGodownDetails(string val, string code)
        {
            try
            {
                string SkipGocd = "'TR'" + (code.retStr() == "" ? "" : "," + code.retSqlformat());
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
        public ActionResult GetData(TransactionKhasraEntry VE)
        {
            try
            {
                Cn.getQueryString(VE);
                DataTable dt = new DataTable();
                if (VE.MENU_PARA == "KHSR")
                {
                    var GetPendig_Data = salesfunc.getPendKhasra(VE.T_CNTRL_HDR.DOCDT.retDateStr(), "", (VE.T_BALE_HDR.AUTONO.retStr() == "" ? "" : VE.T_BALE_HDR.AUTONO.retSqlformat()));
                    DataView dv = new DataView(GetPendig_Data);
                    string[] COL = new string[] { "blautono", "lrno", "lrdt", "baleno", "prefno", "prefdt" };
                    dt = dv.ToTable(true, COL);
                }
                else if (VE.MENU_PARA == "TRFB" || VE.MENU_PARA == "TRWB")
                {
                    dt = salesfunc.GetBaleStock(VE.T_CNTRL_HDR.DOCDT.retDateStr(), VE.T_TXN.GOCD.retSqlformat(), "", "", "", VE.T_BALE_HDR.AUTONO.retStr());
                }

                VE.TBILTYKHASRA_POPUP = (from DataRow dr in dt.Rows
                                         select new TBILTYKHASRA_POPUP
                                         {
                                             BLAUTONO = dr["blautono"].retStr(),
                                             LRNO = dr["lrno"].retStr(),
                                             LRDT = dr["lrdt"].retDateStr(),
                                             BALENO = dr["baleno"].retStr(),
                                             PREFNO = dr["prefno"].retStr(),
                                             PREFDT = dr["PREFDT"].retDateStr()
                                         }).Distinct().ToList();
                if (VE.TBILTYKHASRA != null)
                {//checked when opend secone times.
                    var selectedbill = VE.TBILTYKHASRA.Select(e => e.BLAUTONO).Distinct().ToList();
                    var selectedbillbaleno = VE.TBILTYKHASRA.Select(e => e.BALENO).Distinct().ToList();
                    VE.TBILTYKHASRA_POPUP.Where(x => selectedbill.Contains(x.BLAUTONO) && selectedbillbaleno.Contains(x.BALENO)).ForEach(e => e.Checked = true);
                }
                for (int p = 0; p <= VE.TBILTYKHASRA_POPUP.Count - 1; p++)
                {
                    if (VE.MENU_PARA == "TRFB" || VE.MENU_PARA == "TRWB") { VE.TBILTYKHASRA_POPUP[p].SLNO = Convert.ToInt16(p + 1001); }
                    else { VE.TBILTYKHASRA_POPUP[p].SLNO = Convert.ToInt16(p + 1); }
                    VE.TBILTYKHASRA_POPUP[p].SLNO = Convert.ToInt16(p + 1);
                }
                if (VE.TBILTYKHASRA_POPUP.Count != 0)
                {
                    VE.DefaultView = true;
                    return PartialView("_T_Bilty_Khasra_PopUp", VE);

                }
                else {
                    VE.DefaultView = true;
                    return Content("0");
                }

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SelectPendingLRNO(TransactionKhasraEntry VE, string DOCDT)
        {
            Cn.getQueryString(VE);
            try
            {
                string GC = Cn.GCS();
                List<string> baleno = new List<string>();
                List<string> blautonos = new List<string>();
                List<string> existingbale = new List<string>();
                foreach (var i in VE.TBILTYKHASRA_POPUP)
                {
                    if (i.Checked == true ||i.CheckedAll==true)
                    {
                        blautonos.Add(i.BLAUTONO);
                        baleno.Add(i.BALENO);
                    }
                }
                DataTable dt = new DataTable();
                var sqlbillautonos = string.Join(",", blautonos).retSqlformat();
                if (VE.MENU_PARA == "KHSR")
                {
                    var GetPendig_Data = salesfunc.getPendKhasra(DOCDT, sqlbillautonos, (VE.T_BALE_HDR.AUTONO.retStr() == "" ? "" : VE.T_BALE_HDR.AUTONO.retSqlformat()));
                    DataView dv = new DataView(GetPendig_Data);
                    dt = dv.ToTable(true);
                }
                else if (VE.MENU_PARA == "TRFB" || VE.MENU_PARA == "TRWB")
                {
                    var GetPendig_Data = salesfunc.GetBaleStock(DOCDT, VE.T_TXN.GOCD.retSqlformat(), "", "", "", VE.T_BALE_HDR.AUTONO.retStr());
                    DataView dv = new DataView(GetPendig_Data);
                    dt = dv.ToTable(true);
                }
                var existingdata = VE.TBILTYKHASRA;
                if (VE.TBILTYKHASRA != null)
                {
                    existingbale = VE.TBILTYKHASRA.Select(a => a.BLAUTONO + a.BALENO).ToList();
                }
                var newdata = (from DataRow dr in dt.Rows
                               where !existingbale.Contains(dr["blautono"].retStr() + dr["baleno"].retStr())
                               && baleno.Contains(dr["baleno"].retStr())
                               && blautonos.Contains(dr["blautono"].retStr())
                               select new TBILTYKHASRA
                               {
                                   BLAUTONO = dr["blautono"].retStr(),
                                   ITCD = dr["itcd"].retStr(),
                                   ITNM = dr["itstyle"].retStr(),
                                   NOS = dr["nos"].retStr(),
                                   QNTY = dr["qnty"].retStr(),
                                   RATE = dr["RATE"].retStr(),
                                   UOMCD = dr["uomcd"].retStr(),
                                   SHADE = dr["shade"].retStr(),
                                   BALENO = dr["baleno"].retStr(),
                                   PAGENO = (dr["pageno"].retStr() == "" && dr["pageslno"].retStr() == "") ? "" : dr["pageno"].retStr() + "/" + dr["pageslno"].retStr(),
                                   LRNO = dr["lrno"].retStr(),
                                   LRDT = dr["lrdt"].retDateStr(),
                                   BALEYR = dr["baleyr"].retStr(),
                                   BLSLNO = dr["blslno"].retShort(),
                                   PBLNO = dr["prefno"].retStr(),
                                   PBLDT = dr["prefdt"].retDateStr()
                               }).Distinct().ToList();
                if (VE.TBILTYKHASRA == null)
                {
                    VE.TBILTYKHASRA = newdata;
                }
                else
                {
                    VE.TBILTYKHASRA.AddRange(newdata);
                }
                for (int i = 0; i <= VE.TBILTYKHASRA.Count - 1; i++)
                {
                    VE.TBILTYKHASRA[i].SLNO = Convert.ToInt16(i + 1);
                }
                ModelState.Clear();
                VE.DefaultView = true;
                var GRN_MAIN = RenderRazorViewToString(ControllerContext, "_T_Bilty_Khasra_Main", VE);
                return Content(GRN_MAIN);
            }
            catch (Exception ex)
            {
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
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
        public ActionResult DeleteRow(TransactionKhasraEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<TBILTYKHASRA> ITEMSIZE = new List<TBILTYKHASRA>();
                int count = 0;
                for (int i = 0; i <= VE.TBILTYKHASRA.Count - 1; i++)
                {
                    if (VE.TBILTYKHASRA[i].Checked == false)
                    {
                        count += 1;
                        TBILTYKHASRA item = new TBILTYKHASRA();
                        item = VE.TBILTYKHASRA[i];
                        item.SLNO = count.retShort();
                        ITEMSIZE.Add(item);
                    }

                }
                VE.TBILTYKHASRA = ITEMSIZE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_Bilty_Khasra_Main", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(TransactionKhasraEntry VE)
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
        public ActionResult DeleteDOCRow(TransactionKhasraEntry VE)
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
        public ActionResult cancelRecords(TransactionKhasraEntry VE, string par1)
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
                        TCH = Cn.T_CONTROL_HDR(VE.T_BALE_HDR.AUTONO, CommVar.CurSchema(UNQSNO));
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_BALE_HDR.AUTONO, CommVar.CurSchema(UNQSNO), par1);
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
        public ActionResult ParkRecord(FormCollection FC, TransactionKhasraEntry stream, string menuID, string menuIndex)
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
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionKhasraEntry");
                return url;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionKhasraEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DBb = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //Oracle Queries
            string dberrmsg = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            string dbsql = "", postdt = "", weekrem = "", duedatecalcon = "", sql = "";
            string[] dbsql1;
            double dbDrAmt = 0, dbCrAmt = 0;
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    //DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                    OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
                    String query = "";
                    string dr = ""; string cr = ""; int isl = 0; string strrem = "";
                    double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
                    Int32 z = 0; Int32 maxR = 0;
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        #region datatable create
                        double BLAMT = 0, ROAMT = 0;
                        if (VE.MENU_PARA.retStr() == "TRWB")
                        {
                            var itcdarr = VE.TBILTYKHASRA.Select(a => a.ITCD).Distinct().ToArray();
                            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                            var tax_data = salesfunc.GetBarHelp(VE.T_CNTRL_HDR.DOCDT.retDateStr(), VE.T_TXN.GOCD.retStr(), "", itcdarr.retSqlfromStrarray(), "", "", "", "", "", "C001", "", "", true, true);
                            var itemdata = (from a in DB.M_SITEM where itcdarr.Contains(a.ITCD) select new { a.ITCD, a.ITNM, a.STYLENO, a.HSNCODE }).ToList();
                            DTNEW();
                            for (int i = 0; i <= VE.TBILTYKHASRA.Count - 1; i++)
                            {
                                if (VE.TBILTYKHASRA[i].SLNO != 0 && VE.TBILTYKHASRA[i].ITCD.retStr() != "")
                                {
                                    string itcd = VE.TBILTYKHASRA[i].ITCD.retStr();
                                    string PRODGRPGSTPER = tax_data.AsEnumerable().Where(a => a.Field<string>("itcd") == itcd).Select(b => b.Field<string>("PRODGRPGSTPER")).FirstOrDefault();
                                    string[] gst = new string[3];
                                    if (PRODGRPGSTPER != "")
                                    {
                                        string ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, VE.TBILTYKHASRA[i].RATE.retDbl());
                                        if (ALL_GSTPER.retStr() != "")
                                        {
                                            gst = ALL_GSTPER.Split(',');
                                        }
                                    }

                                    var item = itemdata.Where(a => a.ITCD == itcd).FirstOrDefault();

                                    double basamt = (VE.TBILTYKHASRA[i].QNTY.retDbl() * VE.TBILTYKHASRA[i].RATE.retDbl()).toRound(2);
                                    double igstamt = ((basamt * gst[0].retDbl()) / 100).toRound(2);
                                    double cgstamt = ((basamt * gst[1].retDbl()) / 100).toRound(2);
                                    double sgstamt = ((basamt * gst[2].retDbl()) / 100).toRound(2);


                                    DataRow dr1 = DT.NewRow();
                                    dr1["SLNO"] = VE.TBILTYKHASRA[i].SLNO;
                                    dr1["ITCD"] = VE.TBILTYKHASRA[i].ITCD;
                                    dr1["ITSTYLE"] = item.STYLENO + "" + item.ITNM;
                                    dr1["HSNCODE"] = item.HSNCODE;
                                    dr1["BASAMT"] = basamt;
                                    dr1["IGSTPER"] = gst[0].retDbl();
                                    dr1["IGSTAMT"] = igstamt;
                                    dr1["CGSTPER"] = gst[1].retDbl();
                                    dr1["CGSTAMT"] = cgstamt;
                                    dr1["SGSTPER"] = gst[2].retDbl();
                                    dr1["SGSTAMT"] = sgstamt;
                                    dr1["NETAMT"] = (basamt + igstamt + cgstamt + sgstamt).toRound(2);
                                    DT.Rows.Add(dr1);
                                }
                            }
                            double totalbillamt = DT.AsEnumerable().Sum(a => a.Field<double>("NETAMT"));
                            double R_TOTAL_BILL_AMOUNT = Math.Round(totalbillamt);
                            double TOTAL_ROUND = R_TOTAL_BILL_AMOUNT - totalbillamt;
                            BLAMT = (R_TOTAL_BILL_AMOUNT).toRound(2);
                            ROAMT = (TOTAL_ROUND).toRound(2);
                        }
                        #endregion

                        T_TXN TTXN = new T_TXN();
                        T_BALE_HDR TBHDR = new T_BALE_HDR();
                        T_TXNTRANS TXNTRANS = new T_TXNTRANS();
                        T_TXNOTH TTXNOTH = new T_TXNOTH();
                        T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                        string DOCPATTERN = "";
                        TCH.DOCDT = VE.T_CNTRL_HDR.DOCDT;
                        string Ddate = Convert.ToString(TCH.DOCDT);
                        TBHDR.CLCD = CommVar.ClientCode(UNQSNO);
                        TBHDR.MUTSLCD = VE.T_BALE_HDR.MUTSLCD;
                        TBHDR.TXTAG = VE.MENU_PARA == "KHSR" ? "KH" : "TB";
                        string auto_no = ""; string Month = "", DOCNO = "", DOCCD = "";
                        if (VE.DefaultAction == "A")
                        {
                            TBHDR.EMD_NO = 0;
                            DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            DOCNO = Cn.MaxDocNumber(DOCCD, Ddate);
                            DOCPATTERN = Cn.DocPattern(Convert.ToInt32(DOCNO), DOCCD, CommVar.CurSchema(UNQSNO), CommVar.FinSchema(UNQSNO), Ddate);
                            auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), DOCNO, DOCCD, Ddate);
                            TBHDR.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        }
                        else
                        {
                            DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                            DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            DOCNO = VE.T_CNTRL_HDR.DOCONLYNO;
                            TBHDR.AUTONO = VE.T_BALE_HDR.AUTONO;
                            Month = VE.T_CNTRL_HDR.MNTHCD;
                            var MAXEMDNO = (from p in DBb.T_CNTRL_HDR where p.AUTONO == VE.T_BALE_HDR.AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null) { TBHDR.EMD_NO = 0; } else { TBHDR.EMD_NO = Convert.ToInt16(MAXEMDNO + 1); }
                        }
                        string stkdrcr = "", doctag = "";
                        int slno = 0;

                        if (VE.MENU_PARA == "KHSR") { stkdrcr = "D"; doctag = "KH"; slno = 0; }
                        else if (VE.MENU_PARA == "TRFB" || VE.MENU_PARA == "TRWB") { stkdrcr = "C"; doctag = "TR"; slno = 1000; }

                        TTXN.AUTONO = TBHDR.AUTONO;
                        TTXN.CLCD = TBHDR.CLCD;
                        TTXN.DOCCD = DOCCD;
                        TTXN.DOCNO = DOCNO;
                        TTXN.DOCDT = TCH.DOCDT;
                        TTXN.AUTONO = TBHDR.AUTONO;
                        TTXN.EMD_NO = TBHDR.EMD_NO;
                        TTXN.DTAG = "E";
                        TTXN.DOCTAG = doctag;
                        //TTXN.SLCD = VE.T_BALE_HDR.MUTSLCD;
                        TTXN.SLCD = VE.T_TXN.SLCD;
                        TTXN.CONSLCD = VE.T_TXN.CONSLCD;
                        TTXN.BLAMT = VE.T_TXN.BLAMT;
                        TTXN.PREFDT = VE.T_TXN.PREFDT;
                        TTXN.PREFNO = VE.T_TXN.PREFNO;
                        if (string.IsNullOrEmpty(VE.T_TXN.GOCD))
                        {
                            TTXN.GOCD = VE.TBILTYKHASRA[0].GOCD;
                        }
                        else
                        {
                            TTXN.GOCD = VE.T_TXN.GOCD;
                        }
                        TTXN.BARGENTYPE = VE.T_TXN.BARGENTYPE;
                        TTXN.MENU_PARA = VE.T_TXN.MENU_PARA;
                        TTXN.BLAMT = BLAMT;
                        TTXN.ROAMT = ROAMT;

                        if (VE.DefaultAction == "E")
                        {
                            dbsql = MasterHelpFa.TblUpdt("T_BALE", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("T_BATCHDTL", VE.T_BALE_HDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("T_txndtl", VE.T_BALE_HDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("T_TXNTRANS", VE.T_BALE_HDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            if (VE.MENU_PARA == "TRWB")
                            {
                                dbsql = masterHelp.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            }
                               

                            dbsql = MasterHelpFa.TblUpdt("T_txn", VE.T_BALE_HDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            #region finance data posting
                            if (VE.MENU_PARA.retStr() == "TRWB")
                            {
                                //finance
                                dbsql = masterHelp.finTblUpdt("t_vch_gst", TTXN.AUTONO, "E");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                dbsql = masterHelp.finTblUpdt("t_vch_hdr", TTXN.AUTONO, "E");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                            }
                            #endregion
                        }

                        //----------------------------------------------------------//
                        dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(TBHDR.AUTONO, VE.DefaultAction, "S", Month, DOCCD, DOCPATTERN, TCH.DOCDT.retStr(), TBHDR.EMD_NO.retShort(), DOCNO, Convert.ToDouble(DOCNO), null, null, null, TBHDR.MUTSLCD);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = MasterHelpFa.RetModeltoSql(TBHDR, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = masterHelp.RetModeltoSql(TTXN);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        #region finance data posting
                        if (VE.MENU_PARA.retStr() == "TRWB")
                        {
                            Cn.Create_DOCCD(UNQSNO, "F", TTXN.DOCCD);

                            dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            dbsql = masterHelp.InsVch_Hdr(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, null, null, "Y", null, "SB", "", "", TTXN.CURR_CD);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        }
                        #endregion

                        

                        int gs = 0; string strblno = "", strbldt = "", exemptype = "";
                        strbldt = TTXN.DOCDT.ToString();
                        strblno = DOCPATTERN;

                        string gocd = "";
                        int bslno = 0, mxlp = 1;
                        for (int lp = 0; lp <= mxlp; lp++)
                        {
                            for (int i = 0; i <= VE.TBILTYKHASRA.Count - 1; i++)
                            {
                                if (VE.TBILTYKHASRA[i].SLNO != 0)
                                {
                                    stkdrcr = lp == 0 ? "D" : "C";
                                    if (lp == 0) gocd = VE.TBILTYKHASRA[i].GOCD; else gocd = (VE.MENU_PARA == "KHSR" ? "TR" : VE.T_TXN.GOCD);
                                    bool baleflag = (VE.TBILTYKHASRA[i].CheckedBALEOPEN == true && stkdrcr == "D") ? false : true;

                                    if (baleflag == true)
                                    {
                                        T_BALE TBILTYKHASRA = new T_BALE();
                                        TBILTYKHASRA.CLCD = TBHDR.CLCD;
                                        TBILTYKHASRA.AUTONO = TBHDR.AUTONO;
                                        TBILTYKHASRA.SLNO = (VE.TBILTYKHASRA[i].SLNO + (lp == 0 ? 0 : 1000)).retShort();
                                        TBILTYKHASRA.BLAUTONO = VE.TBILTYKHASRA[i].BLAUTONO;
                                        TBILTYKHASRA.DRCR = stkdrcr;
                                        TBILTYKHASRA.LRDT = Convert.ToDateTime(VE.TBILTYKHASRA[i].LRDT);
                                        TBILTYKHASRA.LRNO = VE.TBILTYKHASRA[i].LRNO;
                                        TBILTYKHASRA.BALEYR = VE.TBILTYKHASRA[i].BALEYR;
                                        TBILTYKHASRA.BALENO = VE.TBILTYKHASRA[i].BALENO;
                                        TBILTYKHASRA.BLSLNO = VE.TBILTYKHASRA[i].BLSLNO;
                                        TBILTYKHASRA.GOCD = gocd;
                                        TBILTYKHASRA.BALEOPEN = VE.TBILTYKHASRA[i].CheckedBALEOPEN == true ? "Y" : null;

                                        dbsql = MasterHelpFa.RetModeltoSql(TBILTYKHASRA);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }

                                    //T_TXNDTL TTXNDTL = DBb.T_TXNDTL.Where(r => r.AUTONO == TBILTYKHASRA.BLAUTONO && r.SLNO == TBILTYKHASRA.BLSLNO).FirstOrDefault();
                                    string BLAUTONO = VE.TBILTYKHASRA[i].BLAUTONO;
                                    short BLSLNO = VE.TBILTYKHASRA[i].BLSLNO;
                                    T_TXNDTL TTXNDTL = DBb.T_TXNDTL.Where(r => r.AUTONO == BLAUTONO && r.SLNO == BLSLNO).FirstOrDefault();
                                    TTXNDTL.AUTONO = TBHDR.AUTONO;
                                    TTXNDTL.SLNO = (VE.TBILTYKHASRA[i].SLNO.retInt() + (lp == 0 ? 0 : 1000)).retShort();
                                    TTXNDTL.STKDRCR = stkdrcr;
                                    TTXNDTL.GOCD = gocd;
                                    if (VE.TBILTYKHASRA[i].CheckedBALEOPEN == false)
                                    {
                                        TTXNDTL.BALENO = VE.TBILTYKHASRA[i].BALENO;
                                    }

                                    dbsql = MasterHelpFa.RetModeltoSql(TTXNDTL);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                    //var TBATCHDTlst = DBb.T_BATCHDTL.Where(r => r.AUTONO == TBILTYKHASRA.BLAUTONO && r.TXNSLNO == TBILTYKHASRA.BLSLNO).ToList();
                                    var TBATCHDTlst = DBb.T_BATCHDTL.Where(r => r.AUTONO == BLAUTONO && r.TXNSLNO == BLSLNO).ToList();
                                    for (var dtl = 0; dtl < TBATCHDTlst.Count; dtl++)
                                    {
                                        bslno++;
                                        TBATCHDTlst[dtl].AUTONO = TBHDR.AUTONO;
                                        TBATCHDTlst[dtl].SLNO = Convert.ToInt16(bslno + (lp == 0 ? 0 : 1000));
                                        TBATCHDTlst[dtl].GOCD = gocd;
                                        TBATCHDTlst[dtl].STKDRCR = stkdrcr;
                                        TBATCHDTlst[dtl].TXNSLNO = (VE.TBILTYKHASRA[i].SLNO.retInt() + (lp == 0 ? 0 : 1000)).retShort();
                                        if (VE.TBILTYKHASRA[i].CheckedBALEOPEN == true && lp == 0) TBATCHDTlst[dtl].BALENO = null;
                                        dbsql = masterHelp.RetModeltoSql(TBATCHDTlst[dtl]);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }

                                    #region finance data posting
                                    if (VE.MENU_PARA.retStr() == "TRWB" && VE.TBILTYKHASRA[i].ITCD.retStr() != "" && lp == 1)
                                    {
                                        gs = gs + 1;
                                        string itcd = VE.TBILTYKHASRA[i].ITCD.retStr();
                                        int sl = VE.TBILTYKHASRA[i].SLNO.retInt();
                                        var data = DT.AsEnumerable().Where(a => a.Field<string>("itcd") == itcd && a.Field<int>("slno") == sl).CopyToDataTable();

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
                                        TVCHGST.DSLNO = 1;
                                        //TVCHGST.PCODE = VE.T_BALE_HDR.MUTSLCD;
                                        TVCHGST.PCODE = VE.T_TXN.SLCD;
                                        TVCHGST.BLNO = strblno;
                                        if (strbldt.retStr() != "")
                                        {
                                            TVCHGST.BLDT = Convert.ToDateTime(strbldt);
                                        }
                                        TVCHGST.HSNCODE = data.Rows[0]["HSNCODE"].retStr();
                                        TVCHGST.ITNM = data.Rows[0]["ITSTYLE"].retStr();
                                        TVCHGST.AMT = data.Rows[0]["BASAMT"].retDbl();
                                        TVCHGST.CGSTPER = data.Rows[0]["CGSTPER"].retDbl();
                                        TVCHGST.SGSTPER = data.Rows[0]["SGSTPER"].retDbl();
                                        TVCHGST.IGSTPER = data.Rows[0]["IGSTPER"].retDbl();
                                        TVCHGST.CGSTAMT = data.Rows[0]["CGSTAMT"].retDbl();
                                        TVCHGST.SGSTAMT = data.Rows[0]["SGSTAMT"].retDbl();
                                        TVCHGST.IGSTAMT = data.Rows[0]["IGSTAMT"].retDbl();
                                        TVCHGST.DRCR = "C";
                                        TVCHGST.QNTY = VE.TBILTYKHASRA[i].QNTY.retDbl();
                                        TVCHGST.UOM = VE.TBILTYKHASRA[i].UOMCD;
                                        TVCHGST.SALPUR = "S";
                                        TVCHGST.OTHRAMT = 0;
                                        TVCHGST.ROAMT = ROAMT;
                                        TVCHGST.BLAMT = BLAMT;
                                        TVCHGST.CONSLCD = TTXN.CONSLCD;
                                        TVCHGST.APPLTAXRATE = 0;
                                        TVCHGST.EXEMPTEDTYPE = exemptype;
                                        TVCHGST.INPTCLAIM = "Y";
                                        TVCHGST.TCSPER = TTXN.TCSPER;
                                        TVCHGST.BASAMT = data.Rows[0]["BASAMT"].retDbl();
                                        TVCHGST.RATE = VE.TBILTYKHASRA[i].RATE.retDbl();

                                        dbsql = masterHelp.RetModeltoSql(TVCHGST, "A", CommVar.FinSchema(UNQSNO));
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                        BLAMT = 0; ROAMT = 0;
                                    }
                                    #endregion
                                }
                            }
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
                        if (VE.MENU_PARA == "TRWB")
                        {
                            TTXNOTH.AUTONO = TTXN.AUTONO;
                            TTXNOTH.EMD_NO = TTXN.EMD_NO;
                            TTXNOTH.CLCD = TTXN.CLCD;
                            TTXNOTH.DTAG = TTXN.DTAG;
                            TTXNOTH.MUTSLCD = VE.T_BALE_HDR.MUTSLCD;
                        }
                      
                        //----------------------------------------------------------//
                        dbsql = masterHelp.RetModeltoSql(TXNTRANS);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        if (VE.MENU_PARA == "TRWB")
                        {
                            dbsql = masterHelp.RetModeltoSql(TTXNOTH);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        }
                           

                        if (VE.UploadDOC != null)// add
                        {
                            var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TBHDR.AUTONO, TBHDR.EMD_NO.Value);
                            if (img.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= img.Item1.Count - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(img.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                                for (int tr = 0; tr <= img.Item2.Count - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(img.Item2[tr]);
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
                                    dbsql = MasterHelpFa.RetModeltoSql(NOTE.Item1[tr]);
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
                        dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = MasterHelpFa.TblUpdt("T_BALE", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = MasterHelpFa.TblUpdt("T_BALE_HDR", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = MasterHelpFa.TblUpdt("T_BATCHDTL", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = MasterHelpFa.TblUpdt("T_txndtl", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        if (VE.MENU_PARA == "TRWB")
                        {
                            dbsql = MasterHelpFa.TblUpdt("t_txnoth", VE.T_BALE_HDR.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        }
                       
                        dbsql = MasterHelpFa.TblUpdt("T_TXNTRANS", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = MasterHelpFa.TblUpdt("T_txn", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        #region finance data posting
                        if (VE.MENU_PARA.retStr() == "TRWB")
                        {
                            dbsql = masterHelp.finTblUpdt("t_vch_gst", VE.T_BALE_HDR.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                            dbsql = masterHelp.finTblUpdt("t_vch_hdr", VE.T_BALE_HDR.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                            dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_BALE_HDR.AUTONO, "D", "F", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                        }
                        #endregion

                        dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(VE.T_BALE_HDR.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null);
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
        public ActionResult Print(TransactionKhasraEntry VE, FormCollection FC, string DOCNO, string DOC_CD, string DOCDT)
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
        private void DTNEW()
        {
            DT.Columns.Add("SLNO", typeof(int));
            DT.Columns.Add("ITCD", typeof(string));
            DT.Columns.Add("ITSTYLE", typeof(string));
            DT.Columns.Add("HSNCODE", typeof(string));
            DT.Columns.Add("BASAMT", typeof(double));
            DT.Columns.Add("IGSTPER", typeof(double));
            DT.Columns.Add("IGSTAMT", typeof(double));
            DT.Columns.Add("CGSTPER", typeof(double));
            DT.Columns.Add("CGSTAMT", typeof(double));
            DT.Columns.Add("SGSTPER", typeof(double));
            DT.Columns.Add("SGSTAMT", typeof(double));
            DT.Columns.Add("NETAMT", typeof(double));
        }
        public ActionResult GetLastGodown(TransactionKhasraEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                string str = "",yr="";
                List<string> gocd = new List<string>();
                DataTable dt = new DataTable();
                var baleyr = (from i in DB.T_BALE where i.BALENO == VE.BALENO select new { i.BALEYR,i.GOCD }).Distinct().ToList();
                foreach(var v in baleyr)
                { yr = v.BALEYR;
                    gocd.Add(v.GOCD);
                }
                var sqlgocds = string.Join(",", gocd).retSqlformat();
                var balenoyr = VE.BALENO + yr;
                dt = salesfunc.GetBaleStock(VE.T_CNTRL_HDR.DOCDT.retDateStr(),sqlgocds,balenoyr.retSqlformat(), "", "", VE.T_BALE_HDR.AUTONO.retStr());
            if(dt.Rows.Count>0)
                {
                    var data = (from DataRow dr in dt.Rows
                                select new
                                {
                                    gonm = dr["gonm"].retStr()
                                }).Distinct().OrderByDescending(a => a.gonm).ToList().FirstOrDefault();
                    var gonm = data.gonm;
                    if (data != null) str = str += "^GONM=^" + gonm + Cn.GCS();
                    return Content(str);
                }
               else return Content(str);

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
    }
}