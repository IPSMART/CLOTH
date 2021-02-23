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
    public class T_BiltyR_MutiaController : Controller
    {
        // GET: T_BiltyR_Mutia
        Connection Cn = new Connection(); MasterHelp Master_Help = new MasterHelp(); MasterHelpFa MasterHelpFa = new MasterHelpFa(); SchemeCal Scheme_Cal = new SchemeCal(); Salesfunc salesfunc = new Salesfunc(); DataTable DT = new DataTable(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_BALE_HDR TBH; T_TXNTRANS TXNTRN; T_TXNOTH TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR;
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_BiltyR_Mutia
        public ActionResult T_BiltyR_Mutia(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Receive from Mutia";
                    TransactionBiltyRMutiaEntry VE = (parkID == "") ? new TransactionBiltyRMutiaEntry() : (Improvar.ViewModels.TransactionBiltyRMutiaEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    //================= For Issue ================//
                    List<DropDown_list1> ISSUE = new List<DropDown_list1>();
                    DropDown_list1 ISSUE1 = new DropDown_list1();
                    ISSUE1.text = "Issue";
                    ISSUE1.value = "D";
                    ISSUE.Add(ISSUE1);
                    DropDown_list1 ISSUE2 = new DropDown_list1();
                    ISSUE2.text = "Receive";
                    ISSUE2.value = "C";
                    ISSUE.Add(ISSUE2);
                    VE.DropDown_list1 = ISSUE;
                    // ================= For Issue ================//
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
                            VE = (TransactionBiltyRMutiaEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
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
                TransactionBiltyRMutiaEntry VE = new TransactionBiltyRMutiaEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionBiltyRMutiaEntry Navigation(TransactionBiltyRMutiaEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE);

            TBH = new T_BALE_HDR(); TXNTRN = new T_TXNTRANS(); TXNOTH = new T_TXNOTH(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM();
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
                TCH = DB.T_CNTRL_HDR.Find(TBH.AUTONO);
                if (TBH.MUTSLCD.retStr() != "")
                {
                    string slcd = TBH.MUTSLCD;
                    var subleg = (from a in DBF.M_SUBLEG where a.SLCD == slcd select new { a.SLNM, a.REGMOBILE }).FirstOrDefault();
                    VE.SLNM = subleg.SLNM;
                    VE.REGMOBILE = subleg.REGMOBILE.ToString();
                }

                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TBH.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TBH.AUTONO);
                string Scm = CommVar.CurSchema(UNQSNO);
                string str = "";
                str += "select distinct a.autono,a.blautono,a.slno,a.drcr,a.lrdt,a.lrno,a.baleyr,a.baleno,a.blslno,a.rslno, ";
                str += "b.itcd, c.styleno, c.itnm,c.uomcd,b.nos,b.qnty,b.pageno,b.pageslno,c.itnm||' '||c.styleno itstyle,d.prefno,d.prefdt  ";
                str += " from " + Scm + ".T_BALE a," + Scm + ".T_TXNDTL b," + Scm + ".M_SITEM c," + Scm + ".T_TXN d  ";
                str += " where a.blautono=b.autono(+) and b.itcd=c.itcd(+) and a.blautono=d.autono(+) and a.baleno=b.baleno(+) and a.blslno=b.slno(+) and a.autono='" + TBH.AUTONO + "'  ";
                str += "order by a.slno ";

                DataTable TBILTYRtbl = Master_Help.SQLquery(str);
                VE.TBILTYR = (from DataRow dr in TBILTYRtbl.Rows
                              select new TBILTYR()
                              {
                                  SLNO = Convert.ToInt16(dr["slno"]),
                                  BLAUTONO = dr["blautono"].retStr(),
                                  ITCD = dr["itcd"].retStr(),
                                  ITNM = dr["itstyle"].retStr(),
                                  UOMCD = dr["uomcd"].retStr(),
                                  NOS = dr["nos"].retStr(),
                                  QNTY = dr["qnty"].retStr(),
                                  PAGENO = dr["pageno"].retStr(),
                                  PAGESLNO = dr["pageslno"].retStr(),
                                  LRDT = dr["lrdt"].retDateStr(),
                                  LRNO = dr["lrno"].retStr(),
                                  BALENO = dr["baleno"].retStr(),
                                  BALEYR = dr["baleyr"].retStr(),
                                  BLSLNO = dr["blslno"].retShort(),
                                  RSLNO = dr["rslno"].retShort(),
                                  PBLNO = dr["prefno"].retStr(),
                                  PBLDT = dr["prefdt"].retDateStr()
                              }).OrderBy(s => s.SLNO).ToList();
                string concatStr = "";
                for (int i = 0; i <= VE.TBILTYR.Count - 1; i++)
                {
                    if (VE.TBILTYR[i].PAGENO != "" && VE.TBILTYR[i].PAGESLNO != "") concatStr = "/"; else concatStr = "";
                    VE.TBILTYR[i].PAGENO = VE.TBILTYR[i].PAGENO + concatStr + VE.TBILTYR[i].PAGESLNO;

                }

            }
            //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
            if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            return VE;
        }
        public ActionResult SearchPannelData(TransactionBiltyRMutiaEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
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
            DataTable tbl = Master_Help.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Registered Mobile No." + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {
                SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["mutslcd"] + ") </td><td>" + tbl.Rows[j]["regmobile"] + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
        }
        public ActionResult GetSubLedgerDetails(string val, string code)
        {
            try
            {
                var str = Master_Help.SLCD_help(val, code);
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
        public ActionResult GetPendingData(TransactionBiltyRMutiaEntry VE, string DOCDT, string MUTSLCD)
        {
            try
            {
                var GetPendig_Data = salesfunc.getPendRecfromMutia(DOCDT, MUTSLCD); DataView dv = new DataView(GetPendig_Data);
                string[] COL = new string[] { "blautono", "lrno", "lrdt", "baleno", "prefno", "prefdt" };
                GetPendig_Data = dv.ToTable(true, COL);
                VE.TBILTYR_POPUP = (from DataRow dr in GetPendig_Data.Rows
                                    select new TBILTYR_POPUP
                                    {
                                        BLAUTONO = dr["blautono"].retStr(),
                                        LRNO = dr["lrno"].retStr(),
                                        LRDT = dr["lrdt"].retDateStr(),
                                        BALENO = dr["baleno"].retStr(),
                                        PREFNO = dr["prefno"].retStr(),
                                        PREFDT = dr["prefdt"].retDateStr()
                                    }).Distinct().OrderBy(a => a.BALENO).ThenBy(a => a.PREFNO).ToList();
                for (int p = 0; p <= VE.TBILTYR_POPUP.Count - 1; p++)
                {
                    VE.TBILTYR_POPUP[p].SLNO = Convert.ToInt16(p + 1);
                }
                if (VE.TBILTYR_POPUP.Count != 0)
                {
                    VE.DefaultView = true;
                    return PartialView("_T_BiltyR_Mutia_PopUp", VE);
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
        public ActionResult SelectPendingLRNO(TransactionBiltyRMutiaEntry VE, string DOCDT, string MUTSLCD)
        {
            Cn.getQueryString(VE);
            try
            {
                string GC = Cn.GCS();
                List<string> blautonos = new List<string>();
                foreach (var i in VE.TBILTYR_POPUP)
                {
                    if (i.Checked == true)
                    {
                        blautonos.Add(i.BLAUTONO);
                    }
                }
                var sqlbillautonos = string.Join(",", blautonos).retSqlformat();
                var GetPendig_Data = salesfunc.getPendRecfromMutia(DOCDT, MUTSLCD, sqlbillautonos);
                string concatStr = "";
                //  VE.TBILTYR = (from DataRow dr in GetPendig_Data.Rows
                //                      select new TBILTYR
                //                      {
                //                          BLAUTONO = dr["blautono"].retStr(),
                //                          ITCD= dr["itcd"].retStr(),
                //                          ITNM = dr["itstyle"].retStr(),
                //                          NOS = dr["nos"].retStr(),
                //                          QNTY = dr["qnty"].retStr(),
                //                          UOMCD = dr["uomcd"].retStr(),
                //                          SHADE = dr["shade"].retStr(),
                //                          BALENO = dr["baleno"].retStr(),
                //                          PAGENO = dr["pageno"].retStr(),
                //                          PAGESLNO = dr["pageslno"].retStr(),
                //                          LRNO = dr["lrno"].retStr(),
                //                          LRDT = dr["lrdt"].retDateStr(),
                //                          BALEYR = dr["baleyr"].retStr(),
                //                          BLSLNO = dr["blslno"].retShort(),
                //                          PBLNO = dr["prefno"].retStr(),
                //                          PBLDT = dr["prefdt"].retDateStr(),
                //                      }).Distinct().ToList();
                //  var startno = VE.T_BALE_HDR.STARTNO;
                //  if (startno == null) startno = 0;
                //  for (int i = 0; i <= VE.TBILTYR.Count - 1; i++)
                //  {if (VE.TBILTYR[i].PAGENO != "" && VE.TBILTYR[i].PAGESLNO!="") concatStr = "/"; else concatStr = "";
                //      VE.TBILTYR[i].PAGENO = VE.TBILTYR[i].PAGENO + concatStr + VE.TBILTYR[i].PAGESLNO;
                //      VE.TBILTYR[i].SLNO = Convert.ToInt16(i + 1);
                //      VE.TBILTYR[i].RSLNO = (startno + Convert.ToInt32(i + 1)).retShort();
                //  }

                if (VE.TBILTYR == null)
                {
                    VE.TBILTYR = (from DataRow dr in GetPendig_Data.Rows
                                  select new TBILTYR
                                  {
                                      BLAUTONO = dr["blautono"].retStr(),
                                      ITCD = dr["itcd"].retStr(),
                                      ITNM = dr["itstyle"].retStr(),
                                      NOS = dr["nos"].retStr(),
                                      QNTY = dr["qnty"].retStr(),
                                      UOMCD = dr["uomcd"].retStr(),
                                      SHADE = dr["shade"].retStr(),
                                      BALENO = dr["baleno"].retStr(),
                                      PAGENO = dr["pageno"].retStr(),
                                      PAGESLNO = dr["pageslno"].retStr(),
                                      LRNO = dr["lrno"].retStr(),
                                      LRDT = dr["lrdt"].retDateStr(),
                                      BALEYR = dr["baleyr"].retStr(),
                                      BLSLNO = dr["blslno"].retShort(),
                                      PBLNO = dr["prefno"].retStr(),
                                      PBLDT = dr["prefdt"].retDateStr()
                                  }).Distinct().OrderBy(a => a.BALENO).ThenBy(a => a.PREFNO).ToList();
                    var startno = VE.T_BALE_HDR.STARTNO;
                    if (startno == null) startno = 0;
                    for (int i = 0; i <= VE.TBILTYR.Count - 1; i++)
                    {
                        if (VE.TBILTYR[i].PAGENO != "" && VE.TBILTYR[i].PAGESLNO != "") concatStr = "/"; else concatStr = "";
                        VE.TBILTYR[i].PAGENO = VE.TBILTYR[i].PAGENO + concatStr + VE.TBILTYR[i].PAGESLNO;
                        VE.TBILTYR[i].SLNO = Convert.ToInt16(i + 1);
                        VE.TBILTYR[i].RSLNO = (startno + Convert.ToInt32(i + 1)).retShort();
                    }
                }
                else
                {
                    var tbilyr = (from DataRow dr in GetPendig_Data.Rows
                                  select new TBILTYR
                                  {
                                      BLAUTONO = dr["blautono"].retStr(),
                                      ITCD = dr["itcd"].retStr(),
                                      ITNM = dr["itstyle"].retStr(),
                                      NOS = dr["nos"].retStr(),
                                      QNTY = dr["qnty"].retStr(),
                                      UOMCD = dr["uomcd"].retStr(),
                                      SHADE = dr["shade"].retStr(),
                                      BALENO = dr["baleno"].retStr(),
                                      PAGENO = dr["pageno"].retStr(),
                                      PAGESLNO = dr["pageslno"].retStr(),
                                      LRNO = dr["lrno"].retStr(),
                                      LRDT = dr["lrdt"].retDateStr(),
                                      BALEYR = dr["baleyr"].retStr(),
                                      BLSLNO = dr["blslno"].retShort(),
                                      PBLNO = dr["prefno"].retStr(),
                                      PBLDT = dr["prefdt"].retDateStr()
                                  }).Distinct().OrderBy(a => a.BALENO).ThenBy(a => a.PREFNO).ToList();

                    List<TBILTYR> MLocIFSC1 = new List<TBILTYR>();
                    for (int i = 0; i <= VE.TBILTYR.Count - 1; i++)
                    {
                        TBILTYR MLI = new TBILTYR();
                        MLI = VE.TBILTYR[i];
                        MLocIFSC1.Add(MLI);
                    }
                    var startno = VE.T_BALE_HDR.STARTNO;
                    if (startno == null) startno = 0;
                    foreach (var j in tbilyr)
                    {
                        TBILTYR MLI1 = new TBILTYR();
                        int SERIAL = Convert.ToInt32(MLocIFSC1.Max(a => Convert.ToInt32(a.SLNO)));
                        MLI1.Checked = true;
                        MLI1.BLAUTONO = j.BLAUTONO.retStr();
                        MLI1.ITCD = j.ITCD.retStr();
                        MLI1.ITNM = j.ITNM.retStr();
                        MLI1.NOS = j.NOS.retStr();
                        MLI1.QNTY = j.QNTY.retStr();
                        MLI1.UOMCD = j.UOMCD.retStr();
                        MLI1.SHADE = j.SHADE.retStr();
                        MLI1.BALENO = j.BALENO.retStr();
                        MLI1.PAGENO = j.PAGENO.retStr();
                        MLI1.PAGESLNO = j.PAGESLNO.retStr();
                        MLI1.LRNO = j.LRNO.retStr();
                        MLI1.LRDT = j.LRDT.retDateStr();
                        MLI1.BALEYR = j.BALEYR.retStr();
                        MLI1.BLSLNO = j.BLSLNO.retShort();
                        MLI1.PBLNO = j.PBLNO.retStr();
                        MLI1.PBLDT = j.PBLDT.retDateStr();
                        if (j.PAGENO != "" && j.PAGESLNO != "") concatStr = "/"; else concatStr = "";
                        MLI1.PAGENO = MLI1.PAGENO + concatStr + MLI1.PAGESLNO;
                        MLI1.SLNO = (SERIAL + 1).retShort();
                        MLI1.RSLNO = (startno + (SERIAL + 1)).retShort();

                        MLocIFSC1.Add(MLI1);
                    }
                    VE.TBILTYR = MLocIFSC1;
                }


                ModelState.Clear();
                VE.DefaultView = true;
                var GRN_MAIN = RenderRazorViewToString(ControllerContext, "_T_BiltyR_Mutia_Main", VE);
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
        public ActionResult DeleteRow(TransactionBiltyRMutiaEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<TBILTYR> ITEMSIZE = new List<TBILTYR>();
                int count = 0;
                for (int i = 0; i <= VE.TBILTYR.Count - 1; i++)
                {
                    if (VE.TBILTYR[i].Checked == false)
                    {
                        count += 1;
                        TBILTYR item = new TBILTYR();
                        item = VE.TBILTYR[i];
                        item.SLNO = count.retShort();
                        ITEMSIZE.Add(item);
                    }

                }
                VE.TBILTYR = ITEMSIZE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_BiltyR_Mutia_Main", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(TransactionBiltyRMutiaEntry VE)
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
        public ActionResult DeleteDOCRow(TransactionBiltyRMutiaEntry VE)
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
        public ActionResult cancelRecords(TransactionBiltyRMutiaEntry VE, string par1)
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
        public ActionResult ParkRecord(FormCollection FC, TransactionBiltyRMutiaEntry stream, string menuID, string menuIndex)
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
                string url = Master_Help.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionBiltyRMutiaEntry");
                return url;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionBiltyRMutiaEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //Oracle Queries
            string dberrmsg = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "", postdt = "", weekrem = "", duedatecalcon = "", sql = "";
            string[] dbsql1;
            double dbDrAmt = 0, dbCrAmt = 0;

            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            //
            DB.Configuration.ValidateOnSaveEnabled = false;
            using (var transaction = DB.Database.BeginTransaction())
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
                        T_BALE_HDR TBHDR = new T_BALE_HDR();
                        T_BILTY_HDR TBLTHDR = new T_BILTY_HDR();
                        T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                        string DOCPATTERN = "";
                        TCH.DOCDT = VE.T_CNTRL_HDR.DOCDT;
                        string Ddate = Convert.ToString(TCH.DOCDT);
                        TBHDR.CLCD = CommVar.ClientCode(UNQSNO); TBLTHDR.CLCD = CommVar.ClientCode(UNQSNO);
                        string auto_no = ""; string Month = "", DOCNO = "", DOCCD = "";
                        if (VE.DefaultAction == "A")
                        {
                            TBHDR.EMD_NO = 0;
                            TBLTHDR.EMD_NO = 0;
                            DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            DOCNO = Cn.MaxDocNumber(DOCCD, Ddate);
                            DOCPATTERN = Cn.DocPattern(Convert.ToInt32(DOCNO), DOCCD, CommVar.CurSchema(UNQSNO), CommVar.FinSchema(UNQSNO), Ddate);
                            auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), DOCNO, DOCCD, Ddate);
                            TBHDR.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                            TBLTHDR.AUTONO = TBHDR.AUTONO;
                        }
                        else
                        {
                            DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            DOCNO = VE.T_CNTRL_HDR.DOCONLYNO;
                            TBHDR.AUTONO = VE.T_BALE_HDR.AUTONO;
                            TBLTHDR.AUTONO = VE.T_BALE_HDR.AUTONO;
                            Month = VE.T_CNTRL_HDR.MNTHCD;
                            var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == VE.T_BALE_HDR.AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null) { TBHDR.EMD_NO = 0; } else { TBHDR.EMD_NO = Convert.ToInt16(MAXEMDNO + 1); }
                        }

                        TBHDR.MUTSLCD = VE.T_BALE_HDR.MUTSLCD;
                        TBHDR.STARTNO = VE.T_BALE_HDR.STARTNO;
                        TBHDR.TXTAG = "RC";
                        TBLTHDR.MUTSLCD = VE.T_BALE_HDR.MUTSLCD;

                        if (VE.DefaultAction == "E")
                        {
                            dbsql = MasterHelpFa.TblUpdt("t_bale", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                            //dbsql = MasterHelpFa.TblUpdt("t_cntrl_doc_pass", TBHDR.AUTONO, "E");
                            //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        }

                        //----------------------------------------------------------//
                        dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(TBHDR.AUTONO, VE.DefaultAction, "S", Month, DOCCD, DOCPATTERN, TCH.DOCDT.retStr(), TBHDR.EMD_NO.retShort(), DOCNO, Convert.ToDouble(DOCNO), null, null, null, TBHDR.MUTSLCD);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = MasterHelpFa.RetModeltoSql(TBHDR, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        dbsql = MasterHelpFa.RetModeltoSql(TBLTHDR, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        int COUNTER = 0;

                        for (int i = 0; i <= VE.TBILTYR.Count - 1; i++)
                        {
                            if (VE.TBILTYR[i].SLNO != 0)
                            {
                                COUNTER = COUNTER + 1;
                                T_BALE TBILTYR = new T_BALE();
                                TBILTYR.CLCD = TBHDR.CLCD;
                                TBILTYR.AUTONO = TBHDR.AUTONO;
                                TBILTYR.SLNO = VE.TBILTYR[i].SLNO;
                                TBILTYR.RSLNO = VE.TBILTYR[i].RSLNO;
                                TBILTYR.BLSLNO = VE.TBILTYR[i].BLSLNO;
                                TBILTYR.BLAUTONO = VE.TBILTYR[i].BLAUTONO;
                                TBILTYR.DRCR = "C";
                                TBILTYR.LRDT = Convert.ToDateTime(VE.TBILTYR[i].LRDT);
                                TBILTYR.LRNO = VE.TBILTYR[i].LRNO;
                                TBILTYR.BALEYR = VE.TBILTYR[i].BALEYR;
                                TBILTYR.BALENO = VE.TBILTYR[i].BALENO;
                                dbsql = MasterHelpFa.RetModeltoSql(TBILTYR);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


                            }
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
                            ContentFlg = "1" + " (Issue No. " + DOCCD + DOCNO + ")~" + TBHDR.AUTONO;
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        transaction.Commit();
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

                        dbsql = MasterHelpFa.TblUpdt("t_bale", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("T_BILTY_HDR", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("T_BALE_HDR", VE.T_BALE_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }



                        dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(VE.T_BALE_HDR.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


                        ModelState.Clear();
                        transaction.Commit();
                        OraTrans.Commit();
                        OraCon.Dispose();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                    goto dbok;
                    dbnotsave:;
                    transaction.Rollback();
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    return Content(dberrmsg);
                    dbok:;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        [HttpPost]
        public ActionResult T_BiltyR_Mutia(FormCollection FC, TransactionBiltyRMutiaEntry VE)
        {
            try
            {

                DataTable tbl;
                string Scm = CommVar.CurSchema(UNQSNO);
                string str = "";
                str += "select distinct a.autono,a.blautono,a.slno,a.drcr,a.lrdt,a.lrno,a.baleyr,a.baleno,a.blslno,a.rslno, z.pcstype, ";
                str += "b.itcd, c.styleno, c.itnm,c.uomcd,b.nos,b.qnty,b.pageno,b.pageslno,c.itnm||' '||c.styleno itstyle,d.prefno,d.prefdt from ";

                str += "( select distinct a.autono, a.blautono, a.slno, a.baleno, a.blslno ";
                str += "from " + Scm + ".T_BALE a ";
                str += "where a.autono='" + VE.T_BALE_HDR.AUTONO + "'  ) y, ";

                str += "( select a.autono, a.txnslno, max(a.pcstype) pcstype ";
                str += "from " + Scm + ".t_batchdtl a group by a.autono, a.txnslno) z, ";

                str += Scm + ".T_BALE a," + Scm + ".T_TXNDTL b," + Scm + ".M_SITEM c," + Scm + ".T_TXN d ";

                str += "where y.autono=a.autono(+) and y.slno=a.slno(+) and y.autono=z.autono(+) and y.slno=z.txnslno(+) and ";
                str += "y.blautono=b.autono(+) and b.itcd=c.itcd(+) and y.blautono=d.autono(+) and y.baleno=b.baleno(+) and y.blslno=b.slno(+) and ";
                str += "a.autono='" + VE.T_BALE_HDR.AUTONO + "'  ";
                str += "order by prefno, prefdt, slno ";
                tbl = Master_Help.SQLquery(str);

                if (tbl.Rows.Count != 0)
                {
                    DataTable IR = new DataTable("mstrep");

                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();

                    HC.RepStart(IR, 2);
                    HC.GetPrintHeader(IR, "prefno", "string", "c,12", "Bill No.");
                    HC.GetPrintHeader(IR, "prefdt", "string", "c,12", "Date");
                    HC.GetPrintHeader(IR, "itstyle", "string", "c,25", "Short No.");
                    HC.GetPrintHeader(IR, "pcstype", "string", "c,10", "Ctg");
                    HC.GetPrintHeader(IR, "slno", "string", "c,7", "Slno");
                    HC.GetPrintHeader(IR, "baleno", "string", "c,12", "Bale");
                    HC.GetPrintHeader(IR, "nos", "double", "c,15", "Nos");
                    HC.GetPrintHeader(IR, "qnty", "double", "c,15,3", "Qnty");
                    HC.GetPrintHeader(IR, "uomcd", "string", "c,15", "Uom");
                    HC.GetPrintHeader(IR, "lrno", "string", "c,12", "Lrno");
                    HC.GetPrintHeader(IR, "pageno", "string", "c,12", "PageNo/PageSlNo.");

                    Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                    i = 0; maxR = tbl.Rows.Count - 1;

                    while (i <= maxR)
                    {
                        string blautono = tbl.Rows[i]["blautono"].retStr();
                        while (tbl.Rows[i]["blautono"].retStr() == blautono)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["prefno"] = tbl.Rows[i]["prefno"].retStr();
                            IR.Rows[rNo]["prefdt"] = tbl.Rows[i]["prefdt"].retDateStr();
                            IR.Rows[rNo]["itstyle"] = tbl.Rows[i]["itstyle"].retStr();
                            IR.Rows[rNo]["pcstype"] = tbl.Rows[i]["pcstype"].retStr();
                            IR.Rows[rNo]["slno"] = tbl.Rows[i]["slno"].retStr();
                            IR.Rows[rNo]["baleno"] = tbl.Rows[i]["baleno"].retStr();
                            IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].retDbl();
                            IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                            IR.Rows[rNo]["uomcd"] = tbl.Rows[i]["uomcd"].retStr();
                            IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"].retStr();
                            IR.Rows[rNo]["pageno"] = tbl.Rows[i]["pageno"].retStr() + "/" + tbl.Rows[i]["pageslno"].retStr();
                            i = i + 1;
                            if (i > maxR) break;
                        }
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dammy"] = " ";
                        IR.Rows[rNo]["flag"] = " height:14px; ";
                    }

                    string pghdr1 = "";

                    pghdr1 = "Receive from Mutia Details as on " + VE.T_CNTRL_HDR.DOCDT.retDateStr() + " ";

                    PV = HC.ShowReport(IR, "T_BiltyR_Mutia", pghdr1, "", true, true, "P", false);

                    TempData["T_BiltyR_Mutia"] = PV;
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = "T_BiltyR_Mutia" });
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