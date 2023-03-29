using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace Improvar.Controllers
{
    public class T_StockAdj_MobController : Controller
    {
        public static string[,] headerArray;
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        DropDownHelp dropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        Salesfunc salesfunc = new Salesfunc();
        // GET: T_StockAdj_Mob
        T_MOBDTL sl; T_CNTRL_HDR sll;
        public ActionResult T_StockAdj_Mob(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    TransactionStkConvMobEntry VE = (parkID == "") ? new TransactionStkConvMobEntry() : (Improvar.ViewModels.TransactionStkConvMobEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    ViewBag.formname = "Stock Conversion Mobile";
                    ViewBag.Title = "Stock Conversion Mobile";
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);

                    string Scm1 = CommVar.CurSchema(UNQSNO);
                    string QUERY = "";
                    QUERY += "select distinct b.itgrpcd, b.itgrpnm  ";
                    QUERY += "from " + Scm1 + ".m_sitem a, " + Scm1 + ".m_group b," + Scm1 + ".m_cntrl_loca c, " + Scm1 + ".m_cntrl_loca d," + Scm1 + ".m_cntrl_hdr e, " + Scm1 + ".m_cntrl_hdr f ";
                    QUERY += "where a.itgrpcd=b.itgrpcd(+) and a.m_autono=c.m_autono(+) and b.m_autono=d.m_autono(+)  and a.m_autono=e.m_autono(+) and b.m_autono=f.m_autono(+) ";
                    QUERY += "and (c.compcd='" + COM + "' or c.compcd is null) and (c.loccd='" + LOC + "' or c.loccd is null) ";
                    QUERY += "and (d.compcd='" + COM + "' or d.compcd is null) and (d.loccd='" + LOC + "' or d.loccd is null) ";
                    QUERY += "and nvl(e.inactive_tag,'N') = 'N' and nvl(f.inactive_tag,'N') = 'N' ";
                    QUERY += "and nvl(a.favitem,'N')='Y' ";
                    QUERY += "order by b.itgrpnm ";
                    DataTable rsTmp = masterHelp.SQLquery(QUERY);

                    VE.DropDown_list1 = (from DataRow dr in rsTmp.Rows
                                         select new DropDown_list1
                                         {
                                             value = dr["itgrpcd"].ToString(),
                                             text = dr["itgrpnm"].ToString(),
                                         }).ToList();
                    VE.DropDown_list_GODOWN = dropDownHelp.GetGocdforSelection();

                    string doccd = "";
                    if (op.Length != 0)
                    {
                        if (op == "E" || op == "D" || op == "V")
                        {
                            if (searchValue.Length != 0)
                            {
                                VE = Navigation(VE, DB, 0, searchValue);
                            }

                            VE.T_MOBDTL = sl;
                            VE.T_CNTRL_HDR = sll;
                            if (VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }

                        if (op.ToString() == "A")
                        {
                            var DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                            if (DocumentType == null || DocumentType.Count == 0)
                            {
                                VE.DefaultView = false;
                                VE.DefaultDay = 0;
                                ViewBag.ErrorMessage = "Please Create Document Type";
                                return View(VE);
                            }
                            else if (DocumentType.Count > 1)
                            {
                                VE.DefaultView = false;
                                VE.DefaultDay = 0;
                                ViewBag.ErrorMessage = "Please Create Only One Document Type";
                                return View(VE);
                            }
                            else
                            {
                                doccd = DocumentType[0].value;
                            }


                            if (parkID == "")
                            {
                                T_CNTRL_HDR TCNTRLHDR = new T_CNTRL_HDR();
                                TCNTRLHDR.DOCCD = doccd;
                                VE.T_CNTRL_HDR = TCNTRLHDR;
                                T_MOBDTL T_MOBDTL = new T_MOBDTL();
                                T_MOBDTL.GOCD = TempData["LASTGOCD" + VE.MENU_PARA].retStr();
                                if (T_MOBDTL.GOCD.retStr() == "")
                                {
                                    if (doccd != "")
                                    {
                                        T_MOBDTL.GOCD = DB.T_TXN.Where(a => a.DOCCD == doccd).OrderByDescending(a => a.AUTONO).Select(b => b.GOCD).FirstOrDefault();
                                    }
                                }
                                VE.T_MOBDTL = T_MOBDTL;
                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (TransactionStkConvMobEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }

                    string docdt = "";
                    if (sll != null) if (sll.DOCDT != null) docdt = sll.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_CNTRL_HDR.DOCCD, docdt, VE.DefaultAction, VE.T_CNTRL_HDR.DOCONLYNO, VE);
                    if (VE.DefaultAction == "A")
                    {
                        if (parkID == "")
                        {
                            VE.T_CNTRL_HDR.DOCDT = Cn.getCurrentDate(VE.mindate);

                        }
                    }
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                TransactionStkConvMobEntry VE = new TransactionStkConvMobEntry();
                Cn.SaveException(ex, "");
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionStkConvMobEntry Navigation(TransactionStkConvMobEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new T_MOBDTL(); sll = new T_CNTRL_HDR();

            string[] aa = null;
            if (searchValue.Length == 0)
            {
                aa = VE.IndexKey[index].Navikey.Split(Convert.ToChar(Cn.GCS()));
            }
            else
            {
                aa = searchValue.Split(Convert.ToChar(Cn.GCS()));
            }
            string autono = aa[0].retStr();
            sl = DB.T_MOBDTL.Where(a => a.AUTONO == autono).FirstOrDefault();
            sll = DB.T_CNTRL_HDR.Find(sl.AUTONO);
            string scm = CommVar.CurSchema(UNQSNO);
            string scmf = CommVar.FinSchema(UNQSNO);

            string sql = "select b.docno,to_char(b.docdt,'DD/MM/YYYY')DOCDT,b.autono,a.gocd,c.gonm,e.itgrpcd,e.itgrpnm,d.itcd,d.styleno||' '||d.itnm itstyle,d.FAVCOLR,a.barno,b.slcd,nvl(f.slarea,f.district) slarea,f.slnm,nvl(a.qnty,0)qnty ";
            sql += "from " + scm + ".T_MOBDTL a," + scm + ".T_CNTRL_HDR b," + scmf + ".M_GODOWN c," + scm + ".t_batchmst c," + scm + ".m_sitem d," + scm + ".m_group e," + scmf + ".m_subleg f ";
            sql += "where a.AUTONO=b.AUTONO and a.gocd=c.gocd and a.barno=c.barno and c.itcd=d.itcd and d.itgrpcd=e.itgrpcd and b.slcd=f.slcd(+) ";
            sql += "and a.autono='" + sl.AUTONO + "' ";
            DataTable tbl = masterHelp.SQLquery(sql);
            if (tbl != null && tbl.Rows.Count > 0)
            {
                VE.TMOBDTL = (from DataRow dr in tbl.Rows
                              select new TMOBDTL
                              {
                                  ITCD = dr["ITCD"].ToString(),
                                  ITSTYLE = dr["ITSTYLE"].ToString(),
                                  FAVCOLR = dr["FAVCOLR"].ToString(),
                                  CNTBARNO = 1,
                                  QNTY = dr["qnty"].retDbl(),
                                  BARNO = dr["BARNO"].ToString(),
                              }).ToList();
                VE.ITGRPCD = tbl.Rows[0]["itgrpcd"].retStr();
                VE.SLNM = tbl.Rows[0]["slnm"].retStr();
                if (tbl.Rows[0]["slarea"].retStr().retStr() != "")
                {
                    VE.SLNM = VE.SLNM + " (" + tbl.Rows[0]["slarea"].retStr() + ")";
                }
                string slcd = sll.SLCD.retStr() == "" ? "" : sll.SLCD.retStr().retSqlformat();
                var dt = salesfunc.GetStock(sll.DOCDT.retDateStr(), sl.GOCD.retSqlformat(), "", "", "'FS'", "", VE.ITGRPCD.retSqlformat(), "", "WP", "COO1", "", "", true, true, "", "", false, false, true, "", true, "", slcd, false, true);
                TempData["ItemDetails_T_StockAdj_Mob" + VE.MENU_PARA] = dt;
            }

            return VE;
        }
        public ActionResult SAVE(TransactionStkConvMobEntry VE)
        {
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

                if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                {

                    T_MOBDTL TMOBDTL = new T_MOBDTL();

                    string DOCPATTERN = "", DOCCD = "", DOCONLYNO = "";
                    string Ddate = VE.T_CNTRL_HDR.DOCDT.retDateStr();
                    TMOBDTL.CLCD = CommVar.ClientCode(UNQSNO);
                    string auto_no = ""; string Month = "";
                    DOCCD = VE.T_CNTRL_HDR.DOCCD;
                    if (VE.DefaultAction == "A")
                    {
                        TMOBDTL.EMD_NO = 0;
                        DOCONLYNO = Cn.MaxDocNumber(DOCCD, Ddate);
                        DOCPATTERN = Cn.DocPattern(Convert.ToInt32(DOCONLYNO), DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                        auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), DOCONLYNO, DOCCD, Ddate);
                        TMOBDTL.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                        Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                    }
                    else
                    {
                        DOCONLYNO = VE.T_CNTRL_HDR.DOCONLYNO;
                        TMOBDTL.AUTONO = VE.T_MOBDTL.AUTONO;
                        Month = VE.T_CNTRL_HDR.MNTHCD;
                        TMOBDTL.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                        DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        TMOBDTL.DTAG = "E";

                        #region delete grid data from table
                        dbsql = masterHelp.TblUpdt("T_MOBDTL", TMOBDTL.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        #endregion
                    }

                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TMOBDTL.AUTONO, VE.DefaultAction, "S", Month, DOCCD, DOCPATTERN, Ddate, TMOBDTL.EMD_NO.retShort(), DOCONLYNO, Convert.ToDouble(DOCONLYNO), null, null, null, VE.T_CNTRL_HDR.SLCD);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


                    if (VE.TMOBDTL != null)
                    {
                        DataTable dt = (DataTable)TempData["ItemDetails_T_StockAdj_Mob" + VE.MENU_PARA]; TempData.Keep();
                        VE.TMOBDTL = (from a in VE.TMOBDTL
                                      join DataRow dr in dt.Rows on a.ITCD equals dr["itcd"].retStr()
                                      where a.QNTY.retDbl() != 0
                                      select new TMOBDTL
                                      {
                                          BARNO = a.BARNO,
                                          QNTY = a.QNTY,
                                          STKTYPE = dr["STKTYPE"].retStr(),
                                          MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                          //STKDRCR = dr["STKDRCR"].retStr(),
                                      }).ToList();

                        int slno = 1;
                        VE.TMOBDTL = VE.TMOBDTL.OrderBy(a => a.ITCD).ToList();
                        int i = 0, maxR = VE.TMOBDTL.Count - 1;
                        while (i <= maxR)
                        {
                            string itcd = VE.TMOBDTL[i].ITCD.retStr();
                            int txnslno = 1;
                            while (VE.TMOBDTL[i].ITCD.retStr() == itcd)
                            {
                                if (VE.TMOBDTL[i].QNTY.retDbl() != 0)
                                {
                                    T_MOBDTL TMOBDTL1 = new T_MOBDTL();
                                    TMOBDTL1.EMD_NO = TMOBDTL.EMD_NO;
                                    TMOBDTL1.CLCD = TMOBDTL.CLCD;
                                    TMOBDTL1.DTAG = TMOBDTL.DTAG;
                                    TMOBDTL1.TTAG = TMOBDTL.DTAG;
                                    TMOBDTL1.AUTONO = TMOBDTL.AUTONO;
                                    TMOBDTL1.SLNO = (slno++).retShort();
                                    TMOBDTL1.TXNSLNO = txnslno.retShort();
                                    TMOBDTL1.GOCD = VE.T_MOBDTL.GOCD;
                                    TMOBDTL1.BARNO = VE.TMOBDTL[i].BARNO;
                                    TMOBDTL1.STKTYPE = VE.TMOBDTL[i].STKTYPE;
                                    TMOBDTL1.MTRLJOBCD = VE.TMOBDTL[i].MTRLJOBCD;
                                    TMOBDTL1.STKDRCR = "C";// VE.TMOBDTL[i].STKDRCR;
                                    TMOBDTL1.QNTY = VE.TMOBDTL[i].QNTY;

                                    //TMOBDTL1.PARTCD = VE.TMOBDTL[i].PARTCD;
                                    //TMOBDTL1.HSNCODE = VE.TMOBDTL[i].HSNCODE;
                                    //TMOBDTL1.NOS = VE.TMOBDTL[i].NOS;
                                    //TMOBDTL1.ITREM = VE.TMOBDTL[i].ITREM;
                                    //TMOBDTL1.CUTLENGTH = VE.TMOBDTL[i].CUTLENGTH;
                                    //TMOBDTL1.SHADE = VE.TMOBDTL[i].SHADE;
                                    //TMOBDTL1.RATE = VE.TMOBDTL[i].RATE;
                                    //TMOBDTL1.ADJAUTONO = VE.TMOBDTL[i].ADJAUTONO;

                                    dbsql = masterHelp.RetModeltoSql(TMOBDTL1);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                }
                                i++;
                                if (i > maxR) break;
                            }
                            txnslno++;
                            if (i > maxR) break;
                        }
                    }

                    if (VE.DefaultAction == "A")
                    {
                        ContentFlg = "1" + " (Doc No. " + DOCPATTERN + ")~" + TMOBDTL.AUTONO;
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
                    dbsql = masterHelp.TblUpdt("T_MOBDTL", VE.T_MOBDTL.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_MOBDTL.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


                    ModelState.Clear();
                    OraTrans.Commit();
                    OraCon.Dispose();
                    ContentFlg = "4";
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
                return Content(ContentFlg);
            }
            dbnotsave:
            {
                OraTrans.Rollback();
                OraCon.Dispose();
                return Content(ContentFlg);
            }

        }
        public ActionResult SearchPannelData()
        {
            try
            {
                string scm = CommVar.CurSchema(UNQSNO);
                string scmf = CommVar.FinSchema(UNQSNO);
                string sql = "select distinct b.docno,to_char(b.docdt,'DD/MM/YYYY')DOCDT,b.autono,a.gocd,c.gonm,e.itgrpcd,e.itgrpnm ";
                sql += "from " + scm + ".T_MOBDTL a," + scm + ".T_CNTRL_HDR b," + scmf + ".M_GODOWN c," + scm + ".t_batchmst c," + scm + ".m_sitem d," + scm + ".m_group e ";
                sql += "where a.AUTONO=b.AUTONO and a.gocd=c.gocd and a.barno=c.barno and c.itcd=d.itcd and d.itgrpcd=e.itgrpcd ";
                DataTable tbl = masterHelp.SQLquery(sql);
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Godown" + Cn.GCS() + "Group" + Cn.GCS() + "autono";
                for (int j = 0; j <= tbl.Rows.Count - 1; j++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[j]["docno"] + "</td><td>" + tbl.Rows[j]["docdt"] + " </td><td>" + tbl.Rows[j]["itgrpnm"] + " (" + tbl.Rows[j]["itgrpcd"] + ")" + "</td><td>" + tbl.Rows[j]["gonm"] + " (" + tbl.Rows[j]["gocd"] + ")" + "</td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
                }
                return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSubLedgerDetails(string val)
        {
            try
            {
                var str = masterHelp.SLCD_help(val, "", "Party");
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) > 0)
                    {
                        string area = str.retCompValue("slarea");
                        if (area.retStr() != "")
                        {
                            string slnm = str.retCompValue("slnm") + " (" + str.retCompValue("slarea") + ")";
                            str = str.ReplaceHelpStr("SLNM", slnm);
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
        public ActionResult GetItemDetails(string val)
        {
            try
            {
                var str = masterHelp.ITCD_help(val, "");
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
        public ActionResult GetItemData(string docdt, string slcd, string gocd, string itgrpcd)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                TransactionStkConvMobEntry VE = new TransactionStkConvMobEntry();
                gocd = gocd.retStr() == "" ? "" : gocd.retStr().retSqlformat();
                slcd = slcd.retStr() == "" ? "" : slcd.retStr().retSqlformat();
                itgrpcd = itgrpcd.retStr() == "" ? "" : itgrpcd.retStr().retSqlformat();

                var tbl = salesfunc.GetStock(docdt.retStr(), gocd.retStr(), "", "", "'FS'", "", itgrpcd, "", "WP", "COO1", "", "", true, true, "", "", false, false, true, "", true, "", slcd, false, true);
                TempData["ItemDetails_T_StockAdj_Mob" + VE.MENU_PARA] = tbl;

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    VE.TMOBDTL = (from DataRow dr in tbl.Rows
                                  group dr by new { ITCD = dr["ITCD"].ToString(), ITSTYLE = dr["ITSTYLE"].ToString(), FAVCOLR = dr["FAVCOLR"].ToString() } into X
                                  select new TMOBDTL
                                  {
                                      ITCD = X.Key.ITCD,
                                      ITSTYLE = X.Key.ITSTYLE,
                                      FAVCOLR = X.Key.FAVCOLR,
                                      CNTBARNO = X.Count(),
                                      BARNO = X.Select(Z => Z.Field<string>("BARNO")).FirstOrDefault()
                                  }).OrderBy(a => a.ITCD).ToList();

                }
                VE.DefaultView = true;
                return PartialView("_T_StockAdj_Mob_Main", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
    }
}
