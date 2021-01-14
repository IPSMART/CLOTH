using System;
using System.Linq;
using System.Web.Mvc;                                                   //Modified By Mithun Das
using Improvar.Models;                                                  // Modified By Prakash Kunwar Dated 24-11-2018
using Improvar.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Microsoft.Ajax.Utilities;

namespace Improvar.Controllers
{
    public class Sys_StkTrnfController : Controller
    {
        public static string[,] headerArray;
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();
        Salesfunc Salesfunc = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Sys_StkTrnf()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Product Balance Transfer from old year to next year";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    // VE = (ReportViewinHtml)Cn.EntryCommonLoading(VE, VE.PermissionID);

                    VE.DropDown_list = (from i in DB.M_JOBMST
                                        select new DropDown_list()
                                        { value = i.JOBCD, text = i.JOBNM })
                    .OrderBy(s => s.text).ToList();

                    VE.DefaultView = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message + ex.InnerException);
            }
        }

        public ActionResult Sys_StkTrnf1(ReportViewinHtml VE)
        {
            string retmsg = "", oldschema = "", newschema = "", yrcd = CommVar.YearCode(UNQSNO);
            yrcd = (Convert.ToDecimal(yrcd) - 1).ToString();
            newschema = CommVar.CurSchema(UNQSNO);
            oldschema = CommVar.CurSchema(UNQSNO).Substring(0, CommVar.CurSchema(UNQSNO).Length - 4) + yrcd;
            newschema = newschema.ToUpper(); oldschema = oldschema.ToUpper();
            retmsg = Stock_Transfer(VE.TEXTBOX1, VE, oldschema, newschema);
            return Content(retmsg);
        }
        public ActionResult GetMtrlJobDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", MasterHelp.MTRLJOBCD_help(val));
                }
                else
                {
                    string str = MasterHelp.MTRLJOBCD_help(val);
                    return Content(str);
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        public string Stock_Transfer(string itgrpcd, ReportViewinHtml VE, string oldschema, string newschema)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            yrcd = (Convert.ToDecimal(yrcd) - 1).ToString();
            scm1 = oldschema;

            string fdt = "";
            string RateQntyBAg = "B";
            string query = ""; string query1 = "";
            string chkval, chkval1 = "";
            string dbsql = ""; string[] dbsql1;
            string tdt = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).AddDays(-1).retDateStr();
            string defdoccd = "", docno = ""; int mxdocno = 0;
            string startdt = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).AddDays(-1).retDateStr();

            scm1 = newschema;
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), newschema);
            ImprovarDB DBOLD = new ImprovarDB(Cn.GetConnectionString(), oldschema);

            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string sql = "";

            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;

            string autono; string doccd;
            DataTable tbl = new DataTable();

            string seldoccd = "";
            DataTable rstmp = new DataTable();
            //DataTable tblcheck = new DataTable();
            bool recoexist;

            Int32 i = 0; Int32 maxR = 0;
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    string sqlc = "", dberrmsg = "";

                    #region Transfer Pending Job Challans
                    dberrmsg = "";
                    string jobcd = VE.TEXTBOX1;

                    #region Pending Job Challan Other than Cutter
                    if (VE.Checkbox3 == true && jobcd != "CT")
                    {
                        string dtype = "";
                        string oldfinschema = CommVar.FinSchema(UNQSNO);
                        switch (jobcd)
                        {
                            case "ST":
                                dtype = "'OSTI'"; break;
                            case "EM":
                                dtype = "'OEMI'"; break;
                            case "PR":
                                dtype = "'OPRI'"; break;
                            case "IR":
                                dtype = "'OIRI'"; break;
                            case "DY":
                                dtype = "'ODYI'"; break;
                            case "BL":
                                dtype = "'OBLI'"; break;
                            case "KT":
                                dtype = "'OKTI'"; break;
                            case "WA":
                                dtype = "'OWAI'"; break;
                            case "YD":
                                dtype = "'OYDI'"; break;
                        }

                        tbl = Salesfunc.getPendProg(tdt, "", "", "", "'" + jobcd + "'", "", "", "", CommVar.CurSchema(UNQSNO));
                        tbl.DefaultView.Sort = "progautono, progslno, styleno, itnm, itcd, partnm, partcd, print_seq, sizenm";
                        tbl = tbl.DefaultView.ToTable();

                        sql = "select doccd from " + oldschema + ".m_doctype where doctype in ('OSTI','OEMI','OPRI','OIRI','ODYI','OBLI','OKTI','OWAI','OYDI')";
                        sql = "select doccd from " + oldschema + ".m_doctype where doctype not in ('ISI','ISR','ISLC','ISLW')";
                        rstmp = MasterHelp.SQLquery(sql);
                        seldoccd = string.Join("','", (from DataRow dr in rstmp.Rows select dr["doccd"].ToString()).Distinct());
                        string[] strdocd = seldoccd.Replace("'", "").Split(',');

                        sql = "select doccd from " + oldschema + ".m_doctype where doctype in (" + dtype + ") ";
                        rstmp = MasterHelp.SQLquery(sql);
                        seldoccd = string.Join("','", (from DataRow dr in rstmp.Rows select dr["doccd"].ToString()).Distinct());

                        if (seldoccd.Length > 0) seldoccd = "'" + seldoccd + "'";

                        var vTCNTRLHDR = (from p in DBOLD.T_CNTRL_HDR
                                          where (strdocd.Contains(p.DOCCD))
                                          select p).ToList();

                        var vTTXN = (from p in DBOLD.T_TXN
                                     join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                     where (strdocd.Contains(q.DOCCD))
                                     select p).ToList();

                        var vTBATCHMST = (from p in DBOLD.T_BATCHMST
                                          join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                          where (strdocd.Contains(q.DOCCD))
                                          select p).ToList();

                        //var vTBATCHDTL = (from p in DBOLD.T_BATCHDTL
                        //                  join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                        //                  where (strdocd.Contains(q.DOCCD))
                        //                  select p).ToList();

                        var vTPROGMAST = (from p in DBOLD.T_PROGMAST
                                          join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                          where (strdocd.Contains(q.DOCCD))
                                          select p).ToList();

                        //var vTPROGDTL = (from p in DBOLD.T_PROGDTL
                        //                 join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                        //                 where (strdocd.Contains(q.DOCCD))
                        //                 select p).ToList();

                        #region Record delete if found

                        sql = "alter table " + newschema + ".t_progdtl disable constraint fkey_t_progdtl_progautono";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sqlc = "select a.autono from " + newschema + ".t_cntrl_hdr a where a.doccd in (" + seldoccd + ") and ";
                        sqlc += "a.yr_cd < '" + CommVar.YearCode(UNQSNO) + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' ";
                        DataTable tbldel = MasterHelp.SQLquery(sqlc);

                        query = "delete from " + newschema + ".t_progdtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_progmast where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_txn where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_cntrl_hdr_doc where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_cntrl_hdr_doc_dtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_cntrl_hdr where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                        //for (Int32 q = 0; q <= tbldel.Rows.Count - 1; q++)
                        //{
                        //    query = "delete from " + newschema + ".t_cntrl_hdr where autono='" + tbldel.Rows[q]["autono"].ToString() + "' ";
                        //    dberrmsg += MasterHelp.SQLNonQuery(query);
                        //}
                        if (dberrmsg != "") goto dbnotsave;
                        #endregion
                        //DB.Database.ExecuteSqlCommand("lock table " + newschema + ".T_CNTRL_HDR in  row share mode");
                        //Insert Cutter Rec. data
                        DataTable tblrec = tbl.Copy();
                        tblrec.DefaultView.Sort = "orgbatchautono, orgbatchslno, styleno, itnm, itcd, partnm, partcd, print_seq, sizenm";
                        tblrec = tbl.DefaultView.ToTable();
                        maxR = tbl.Rows.Count - 1; i = 0;
                        DataTable temtdt = new DataTable();
                        while (i <= maxR)
                        {
                            chkval = tbl.Rows[i]["orgbatchautono"].ToString();
                            if (chkval != "")
                            {
                                sql = "select autono from " + newschema + ".t_cntrl_hdr where autono='" + chkval + "'";
                                //Cn.com = new OracleCommand(sql, Cn.con);
                                //Cn.da.SelectCommand = Cn.com;
                                ////Cn.da.Fill(tblcheck);
                                //OraCmd.CommandText = sql;
                                OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                if (recoexist == false)
                                {
                                    //var TCHOLD = DBOLD.T_CNTRL_HDR.Where(x => x.AUTONO == chkval).ToList();
                                    var TCHOLD = vTCNTRLHDR.Where(x => x.AUTONO == chkval).ToList();
                                    if (TCHOLD.Count != 0)
                                    {
                                        for (int tr = 0; tr <= TCHOLD.Count() - 1; tr++)
                                        {
                                            dbsql = MasterHelpFa.RetModeltoSql(TCHOLD[tr], (recoexist == false ? "A" : "E"));
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                        }
                                    }

                                    var TTXN = vTTXN.Where(x => x.AUTONO == chkval).ToList();
                                    if (TTXN.Count != 0)
                                    {
                                        for (int tr = 0; tr <= TTXN.Count() - 1; tr++)
                                        {
                                            dbsql = MasterHelpFa.RetModeltoSql(TTXN[tr], (recoexist == false ? "A" : "E"));
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                        }
                                    }

                                    while (tblrec.Rows[i]["orgbatchautono"].ToString() == chkval)
                                    {
                                        int sln = Convert.ToInt16(tblrec.Rows[i]["orgbatchslno"]);
                                        string checkval = chkval + sln.ToString();

                                        var TBATCHMST = vTBATCHMST.Where(x => x.AUTONO == chkval && x.SLNO == sln).ToList();
                                        if (TBATCHMST.Count != 0)
                                        {
                                            for (int tr = 0; tr <= TBATCHMST.Count() - 1; tr++)
                                            {
                                                dbsql = MasterHelpFa.RetModeltoSql(TBATCHMST[tr]);
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                            }
                                        }
                                        while (tblrec.Rows[i]["orgbatchautono"].ToString() + Convert.ToInt16(tblrec.Rows[i]["orgbatchslno"]) == checkval)
                                        {
                                            i++;
                                            if (i > maxR) break;
                                            if (tblrec.Rows[i]["orgbatchautono"].ToString() == "") break;
                                        }
                                        if (i > maxR) break;
                                    }
                                }
                                else
                                {
                                    i++;
                                }
                            }
                            else
                            {
                                i++;
                            }
                        }
                        //Insert Rec. Ref No. Details
                        tblrec = tbl.Copy();
                        tblrec.DefaultView.Sort = "recautono, styleno, itnm, itcd, partnm, partcd, print_seq, sizenm";
                        tblrec = tbl.DefaultView.ToTable();
                        maxR = tbl.Rows.Count - 1; i = 0;
                        while (i <= maxR)
                        {
                            chkval = tbl.Rows[i]["recautono"].ToString();

                            sql = "select autono from " + newschema + ".t_cntrl_hdr where autono='" + chkval + "'";
                            OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                            if (recoexist == false && chkval != "")
                            {
                                var TCHOLD = vTCNTRLHDR.Where(x => x.AUTONO == chkval).ToList();
                                if (TCHOLD.Count != 0)
                                {
                                    for (int tr = 0; tr <= TCHOLD.Count() - 1; tr++)
                                    {
                                        dbsql = MasterHelpFa.RetModeltoSql(TCHOLD[tr], (recoexist == false ? "A" : "E"));
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }
                                var TTXN = vTTXN.Where(x => x.AUTONO == chkval).ToList();
                                if (TTXN.Count != 0)
                                {
                                    for (int tr = 0; tr <= TTXN.Count() - 1; tr++)
                                    {
                                        dbsql = MasterHelpFa.RetModeltoSql(TTXN[tr], (recoexist == false ? "A" : "E"));
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }
                                while (tblrec.Rows[i]["recautono"].ToString() == chkval)
                                {
                                    i++;
                                    if (i > maxR) break;
                                }
                            }
                            else
                            {
                                i++;
                            }
                        }

                        maxR = tbl.Rows.Count - 1; i = 0;
                        while (i <= maxR)
                        {
                            chkval = tbl.Rows[i]["progautono"].ToString();

                            sql = "select autono from " + newschema + ".t_cntrl_hdr where autono='" + chkval + "'";
                            OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                            var TCHOLD = vTCNTRLHDR.Where(x => x.AUTONO == chkval).ToList();
                            if (TCHOLD.Count != 0)
                            {
                                for (int tr = 0; tr <= TCHOLD.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TCHOLD[tr], (recoexist == false ? "A" : "E"));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            var TTXN = vTTXN.Where(x => x.AUTONO == chkval).ToList();
                            if (TTXN.Count != 0)
                            {
                                for (int tr = 0; tr <= TTXN.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TTXN[tr], (recoexist == false ? "A" : "E"));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            int sl = 1;
                            while (tbl.Rows[i]["progautono"].ToString() == chkval)
                            {
                                sl = Convert.ToInt16(tbl.Rows[i]["progslno"]);

                                var TPROGMAST = vTPROGMAST.Where(x => x.AUTONO == chkval && x.SLNO == sl).ToList();
                                if (TPROGMAST.Count != 0)
                                {
                                    for (int tr = 0; tr <= TPROGMAST.Count() - 1; tr++)
                                    {
                                        dbsql = MasterHelpFa.RetModeltoSql(TPROGMAST[tr]);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }
                                T_PROGDTL TPROGDTL = new T_PROGDTL();

                                TPROGDTL.EMD_NO = TPROGMAST[0].EMD_NO;
                                TPROGDTL.CLCD = TPROGMAST[0].CLCD;
                                TPROGDTL.DTAG = TPROGMAST[0].DTAG;
                                TPROGDTL.TTAG = TPROGMAST[0].TTAG;
                                TPROGDTL.AUTONO = TPROGMAST[0].AUTONO;
                                //TPROGDTL.DOCCD = TPROGMAST[0].DOCCD;
                                //TPROGDTL.DOCNO = TPROGMAST[0].DOCNO;
                                TPROGDTL.SLNO = TPROGMAST[0].SLNO;
                                TPROGDTL.PROGAUTONO = TPROGMAST[0].AUTONO;
                                TPROGDTL.PROGSLNO = TPROGMAST[0].SLNO;
                                TPROGDTL.STKDRCR = "D";
                                TPROGDTL.RATE = 0;
                                TPROGDTL.NOS = tbl.Rows[i]["balnos"].retDbl();
                                TPROGDTL.QNTY = tbl.Rows[i]["balqnty"].retDbl();

                                dbsql = MasterHelpFa.RetModeltoSql(TPROGDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                i++;
                                if (i > maxR) break;
                            }
                        }
                    }
                    #endregion
                    #region Pending Job Challan Cutter
                    else if (VE.Checkbox3 == true && jobcd == "CT")
                    {
                        sql = "select slcd from " + CommVar.FinSchema(UNQSNO) + ".m_subleg_link a where a.linkcd='I'";
                        DataTable tbllink = MasterHelp.SQLquery(sql);
                        string InHouseSlcd = string.Join(",", (from DataRow dr in tbllink.Rows select dr["slcd"].ToString()).Distinct());
                        InHouseSlcd = InHouseSlcd.retSqlformat();

                        tbl = Salesfunc.retCutterBalFifo("", tdt, oldschema);

                        sql = "select doccd from " + oldschema + ".m_doctype where doctype in ('OSTI','OEMI','OPRI','OIRI','ODYI','OBLI','OKTI','OWAI','OYDI')";
                        sql = "select doccd from " + oldschema + ".m_doctype where doctype not in ('ISI','ISR','ISLC','ISLW')";
                        rstmp = MasterHelp.SQLquery(sql);
                        seldoccd = string.Join("','", (from DataRow dr in rstmp.Rows select dr["doccd"].ToString()).Distinct());
                        string[] strdocd = seldoccd.Replace("'", "").Split(',');

                        string dtype = "'OCTI'";
                        sql = "select doccd from " + oldschema + ".m_doctype where doctype in (" + dtype + ") ";
                        rstmp = MasterHelp.SQLquery(sql);
                        seldoccd = string.Join("','", (from DataRow dr in rstmp.Rows select dr["doccd"].ToString()).Distinct());

                        if (seldoccd.Length > 0) seldoccd = "'" + seldoccd + "'";

                        var vTCNTRLHDR = (from p in DBOLD.T_CNTRL_HDR
                                          where (strdocd.Contains(p.DOCCD))
                                          select p).ToList();

                        var vTTXN = (from p in DBOLD.T_TXN
                                     join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                     where (strdocd.Contains(q.DOCCD))
                                     select p).ToList();

                        #region Record delete if found

                        sql = "alter table " + newschema + ".t_progdtl disable constraint fkey_t_progdtl_progautono";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sqlc = "select a.autono from " + newschema + ".t_cntrl_hdr a where a.doccd in (" + seldoccd + ") and ";
                        sqlc += "a.yr_cd < '" + CommVar.YearCode(UNQSNO) + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' ";
                        DataTable tbldel = MasterHelp.SQLquery(sqlc);

                        query = "delete from " + newschema + ".t_txndtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_txn where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_cntrl_hdr_doc where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_cntrl_hdr where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                        double ftqty = 0, tfqty = 0;

                        if (dberrmsg != "") goto dbnotsave;
                        #endregion
                        maxR = tbl.Rows.Count - 1; i = 0;
                        startloop:
                        while (i <= maxR)
                        {
                            if (tbl.Rows[i]["balqnty"].retDbl() == 0) { i++; goto startloop; };
                            chkval = tbl.Rows[i]["autono"].ToString();

                            sql = "select autono from " + newschema + ".t_cntrl_hdr where autono='" + chkval + "'";
                            OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                            var TCHOLD = vTCNTRLHDR.Where(x => x.AUTONO == chkval).ToList();
                            if (TCHOLD.Count != 0)
                            {
                                for (int tr = 0; tr <= TCHOLD.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TCHOLD[tr], (recoexist == false ? "A" : "E"));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            var TTXN = vTTXN.Where(x => x.AUTONO == chkval).ToList();
                            if (TTXN.Count != 0)
                            {
                                for (int tr = 0; tr <= TTXN.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TTXN[tr], (recoexist == false ? "A" : "E"));
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            int sl = 1;
                            string itcd = "";
                            startloopdtl:
                            while (tbl.Rows[i]["autono"].ToString() == chkval)
                            {
                                if (tbl.Rows[i]["balqnty"].retDbl() == 0) { i++; goto startloopdtl; };
                                sl++;
                                itcd = tbl.Rows[i]["itgrpcd"].ToString() == "F007" ? "C000022" : "C000003";
                                double qty = tbl.Rows[i]["balqnty"].retDbl();
                                if (itcd == "C000022") tfqty = tfqty + qty; else ftqty = ftqty + qty;
                                T_TXNDTL TTXNDTL = new T_TXNDTL();
                                TTXNDTL.AUTONO = TTXN[0].AUTONO;
                                TTXNDTL.SLNO = sl.retShort();
                                TTXNDTL.CLCD = TTXN[0].CLCD;
                                TTXNDTL.ITCD = itcd;
                                //TTXNDTL.DOCCD = TTXN[0].DOCCD;
                                //TTXNDTL.DOCNO = TTXN[0].DOCNO;
                                //TTXNDTL.DOCDT = TTXN[0].DOCDT;
                                TTXNDTL.GOCD = TTXN[0].GOCD;
                                if (qty < 0) TTXNDTL.STKDRCR = "D"; else TTXNDTL.STKDRCR = "C";
                                TTXNDTL.STKTYPE = "F";
                                TTXNDTL.MTRLJOBCD = itcd == "C000003" ? "TF" : "FT";
                                TTXNDTL.JOBCD = "CT";
                                //
                                TTXNDTL.SCMDISCRATE = 0; TTXNDTL.SCMDISCAMT = 0;
                                TTXNDTL.DISCRATE = 0; TTXNDTL.DISCAMT = 0;
                                TTXNDTL.TDDISCRATE = 0; TTXNDTL.TDDISCAMT = 0;
                                TTXNDTL.AMT = 0;
                                //
                                //TTXNDTL.SIZECD = tbl.Rows[i]["sizecd"].ToString();
                                //TTXNDTL.COLRCD = tbl.Rows[i]["colrcd"].ToString();
                                //TTXNDTL.STKQNTY = null;
                                if (qty < 0) qty = qty * -1;
                                TTXNDTL.QNTY = qty;

                                dbsql = MasterHelpFa.RetModeltoSql(TTXNDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                i++;
                                if (i > maxR) break;
                            }
                        }
                    }
                    #endregion
                    #endregion

                    #region Transfer Pending Orders
                    dberrmsg = "";
                    if (VE.Checkbox2 == true)
                    {
                        string dtype = "";
                        string oldfinschema = CommVar.FinSchema(UNQSNO);
                        dtype = "'SORD'";

                        tbl = Salesfunc.GetPendOrder("", "", "", "", "", "SB", "", "", true, "", "", "", "", "", oldschema, oldfinschema);
                        tbl.DefaultView.Sort = "autono, styleno, itnm, itcd, print_seq, sizenm";
                        tbl = tbl.DefaultView.ToTable();

                        sql = "select doccd from " + oldschema + ".m_doctype where doctype in (" + dtype + ") ";
                        rstmp = MasterHelp.SQLquery(sql);
                        seldoccd = "";

                        seldoccd = string.Join("','", (from DataRow dr in rstmp.Rows select dr["doccd"].ToString()).Distinct());
                        string[] strdocd = seldoccd.Replace("'", "").Split(',');

                        if (seldoccd.Length > 0) seldoccd = "'" + seldoccd + "'";
                        #region Record delete if found
                        //sqlc = "select a.autono from " + newschema + ".t_cntrl_hdr a, " + newschema + ".t_txn b where a.doccd in (" + seldoccd + ") and ";
                        //sqlc += "a.autono=b.autono(+) and a.yr_cd <= '" + CommVar.YearCode(UNQSNO) + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' ";
                        sqlc = "select a.autono from " + newschema + ".t_cntrl_hdr a where a.doccd in (" + seldoccd + ") and ";
                        sqlc += "a.yr_cd < '" + CommVar.YearCode(UNQSNO) + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' ";
                        DataTable tbldel = MasterHelp.SQLquery(sqlc);

                        query = "delete from " + newschema + ".t_sorddtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_sord_scheme where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_sorddtl_appdtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_sorddtl_app where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_sord where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_cntrl_hdr_doc where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_cntrl_hdr_doc_dtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_cntrl_hdr where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                        //for (Int32 q = 0; q <= tbldel.Rows.Count - 1; q++)
                        //{
                        //    query = "delete from " + newschema + ".t_cntrl_hdr where autono='" + tbldel.Rows[q]["autono"].ToString() + "' ";
                        //    dberrmsg += MasterHelp.SQLNonQuery(query);
                        //}
                        if (dberrmsg != "") goto dbnotsave;
                        #endregion

                        var vTCNTRLHDR = (from p in DBOLD.T_CNTRL_HDR
                                          where (strdocd.Contains(p.DOCCD))
                                          select p).ToList();

                        var vTSORD = (from p in DBOLD.T_SORD
                                      select p).ToList();

                        var vTSORDSCHEME = (from p in DBOLD.T_SORD_SCHEME
                                            select p).ToList();

                        //var vTSORDAPP = (from p in DBOLD.T_SORDDTL_APP
                        //                 select p).ToList();

                        //var vTSORDAPPDTL = (from p in DBOLD.T_SORDDTL_APPDTL
                        //                    select p).ToList();

                        //DB.Database.ExecuteSqlCommand("lock table " + newschema + ".T_CNTRL_HDR in  row share mode");
                        maxR = tbl.Rows.Count - 1; i = 0;
                        while (i <= maxR)
                        {
                            chkval = tbl.Rows[i]["autono"].ToString();

                            var TCHOLD = vTCNTRLHDR.Where(x => x.AUTONO == chkval).ToList();
                            if (TCHOLD.Count != 0)
                            {
                                for (int tr = 0; tr <= TCHOLD.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TCHOLD[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            var TSORD = vTSORD.Where(x => x.AUTONO == chkval).ToList();
                            if (TCHOLD.Count != 0)
                            {
                                for (int tr = 0; tr <= TSORD.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TSORD[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            int sl = 1;
                            while (tbl.Rows[i]["autono"].ToString() == chkval)
                            {
                                T_SORDDTL TSORDDTL = new T_SORDDTL();
                                Int32 chki = i;

                                TSORDDTL.EMD_NO = TSORD[0].EMD_NO;
                                TSORDDTL.CLCD = TSORD[0].CLCD;
                                TSORDDTL.DTAG = TSORD[0].DTAG;
                                TSORDDTL.TTAG = TSORD[0].TTAG;
                                TSORDDTL.AUTONO = TSORD[0].AUTONO;
                                TSORDDTL.SLNO = sl.retShort();
                                TSORDDTL.STKDRCR = "D";
                                TSORDDTL.STKTYPE = tbl.Rows[i]["stktype"].ToString();
                                TSORDDTL.FREESTK = tbl.Rows[i]["freestk"].ToString();
                                TSORDDTL.ITCD = tbl.Rows[i]["itcd"].ToString();
                                TSORDDTL.SIZECD = tbl.Rows[i]["sizecd"].ToString();
                                TSORDDTL.COLRCD = tbl.Rows[i]["colrcd"].ToString();
                                TSORDDTL.QNTY = tbl.Rows[i]["balqnty"].retDbl();
                                TSORDDTL.RATE = tbl.Rows[i]["rate"].retDbl();
                                TSORDDTL.SCMDISCAMT = 0;
                                TSORDDTL.DISCAMT = 0;
                                TSORDDTL.TAXAMT = 0;
                                dbsql = MasterHelpFa.RetModeltoSql(TSORDDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                sl = sl + 1;
                                i++;
                                if (i > maxR) break;
                            }

                            var TSORDSCHEME = vTSORDSCHEME.Where(x => x.AUTONO == chkval).ToList();
                            if (TSORDSCHEME.Count != 0)
                            {
                                for (int tr = 0; tr <= TSORDSCHEME.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TSORDSCHEME[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            //var TSORDAPP = vTSORDAPP.Where(x => x.AUTONO == chkval).ToList();
                            //if (TSORDAPP.Count != 0)
                            //{
                            //    for (int tr = 0; tr <= TSORDAPP.Count() - 1; tr++)
                            //    {
                            //        dbsql = MasterHelpFa.RetModeltoSql(TSORDAPP[tr]);
                            //        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            //    }
                            //}
                            //var TSORDAPPDTL = vTSORDAPPDTL.Where(x => x.AUTONO == chkval).ToList();
                            //if (TSORDAPPDTL.Count != 0)
                            //{
                            //    for (int tr = 0; tr <= TSORDAPPDTL.Count() - 1; tr++)
                            //    {
                            //        dbsql = MasterHelpFa.RetModeltoSql(TSORDAPPDTL[tr]);
                            //        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            //    }
                            //}
                        }
                    }
                    #endregion

                    #region Transfer Pcs Rate Balance
                    dberrmsg = "";
                    if (VE.Checkbox6 == true)
                    {
                        string dtype = "";
                        string oldfinschema = CommVar.FinSchema(UNQSNO);
                        dtype = "'ISI'";

                        sql = "select distinct a.autono, a.batchno from " + oldschema + ".t_inhissmst a, " + oldschema + ".t_cntrl_hdr b ";
                        sql += "where a.autono=b.autono and a.batchno not in (select batchno from " + oldschema + ".t_inhlotclose) and ";
                        sql += "b.docdt >= to_date('15/02/2020','dd/mm/yyyy') and b.compcd='" + COM + "' and b.loccd='" + LOC + "' ";
                        sql += "order by autono ";

                        tbl = MasterHelp.SQLquery(sql);

                        sql = "select doccd from " + oldschema + ".m_doctype where doctype in (" + dtype + ") ";
                        rstmp = MasterHelp.SQLquery(sql);
                        seldoccd = string.Join("','", (from DataRow dr in rstmp.Rows select dr["doccd"].ToString()).Distinct());
                        string[] strdocd = seldoccd.Replace("'", "").Split(',');
                        if (seldoccd.Length > 0) seldoccd = "'" + seldoccd + "'";

                        string selbatchno = string.Join("','", (from DataRow dr in tbl.Rows select dr["batchno"].ToString()).Distinct());
                        string[] strbatchno = selbatchno.Replace("'", "").Split(',');
                        if (selbatchno.Length > 0) selbatchno = "'" + selbatchno + "'";

                        #region Record delete if found
                        sqlc = "select a.autono from " + newschema + ".t_cntrl_hdr a where a.doccd in (" + seldoccd + ") and ";
                        sqlc += "a.yr_cd < '" + CommVar.YearCode(UNQSNO) + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' ";
                        DataTable tbldel = MasterHelp.SQLquery(sqlc);
                        //
                        query = "delete from " + newschema + ".t_inhissmstdtl where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        query = "delete from " + newschema + ".t_inhissmstsjob where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        query = "delete from " + newschema + ".t_inhissmst where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        query = "delete from " + newschema + ".t_inhiss where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        query = "delete from " + newschema + ".t_cntrl_hdr_doc where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        query = "delete from " + newschema + ".t_cntrl_hdr_doc_dtl where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        query = "delete from " + newschema + ".t_cntrl_hdr where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        //
                        sql = "select doccd from " + oldschema + ".m_doctype where doctype in ('ISR') ";
                        rstmp = MasterHelp.SQLquery(sql);
                        string selrecdoccd = string.Join("','", (from DataRow dr in rstmp.Rows select dr["doccd"].ToString()).Distinct());
                        string[] strrecdocd = selrecdoccd.Replace("'", "").Split(',');
                        if (selrecdoccd.Length > 0) selrecdoccd = "'" + selrecdoccd + "'";
                        sqlc = "select a.autono from " + newschema + ".t_cntrl_hdr a where a.doccd in (" + selrecdoccd + ") and ";
                        sqlc += "a.yr_cd < '" + CommVar.YearCode(UNQSNO) + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' ";

                        query = "delete from " + newschema + ".t_inhrecmstdtl where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        query = "delete from " + newschema + ".t_inhrecmst where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        query = "delete from " + newschema + ".t_inhrec where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        query = "delete from " + newschema + ".t_cntrl_hdr where autono in (" + sqlc + ") ";
                        dberrmsg += MasterHelp.SQLNonQuery(query);
                        //
                        if (dberrmsg != "") goto dbnotsave;
                        #endregion

                        var vTCNTRLHDR = (from p in DBOLD.T_CNTRL_HDR
                                          where (strdocd.Contains(p.DOCCD))
                                          select p).ToList();

                        var vTINHISS = (from p in DBOLD.T_INHISS
                                        join q in DBOLD.T_INHISSMST on p.AUTONO equals (q.AUTONO)
                                        where (strbatchno.Contains(q.BATCHNO))
                                        select p).ToList();

                        var vTINHISSMST = (from p in DBOLD.T_INHISSMST
                                           where (strbatchno.Contains(p.BATCHNO))
                                           select p).ToList();

                        var vTINHISSMSTSJOB = (from p in DBOLD.T_INHISSMSTSJOB
                                               join q in DBOLD.T_INHISSMST on p.AUTONO equals (q.AUTONO)
                                               where (strbatchno.Contains(q.BATCHNO))
                                               select p).ToList();

                        //var vTINHISSMSTDTL = (from p in DBOLD.T_INHISSMSTDTL
                        //                      join q in DBOLD.T_INHISSMST on p.AUTONO equals (q.AUTONO)
                        //                      where (strbatchno.Contains(q.BATCHNO))
                        //                      select p).ToList();

                        //var vTINHRECMSTDTL = (from p in DBOLD.T_INHRECMSTDTL
                        //                      join q in DBOLD.T_INHRECMST on p.AUTONO equals (q.AUTONO)
                        //                      where (strbatchno.Contains(q.BATCHNO))
                        //                      select p).ToList();

                        var vTINHRECMST = (from p in DBOLD.T_INHRECMST
                                           where (strbatchno.Contains(p.BATCHNO))
                                           select p).ToList();

                        //var vTINHREC = (from p in DBOLD.T_INHREC
                        //                join q in DBOLD.T_INHRECMST on p.AUTONO equals (q.AUTONO)
                        //                where (strbatchno.Contains(q.BATCHNO))
                        //                select p).ToList();

                        var vTCNTRLHDRREC = (from p in DBOLD.T_CNTRL_HDR
                                             join q in DBOLD.T_INHRECMST on p.AUTONO equals (q.AUTONO)
                                             where (strbatchno.Contains(q.BATCHNO))
                                             select p).ToList();
                        #region Insert Sticher Inhouse Records
                        maxR = tbl.Rows.Count - 1; i = 0;
                        while (i <= maxR)
                        {
                            chkval = tbl.Rows[i]["autono"].ToString();

                            var TCHOLD = vTCNTRLHDR.Where(x => x.AUTONO == chkval).ToList();
                            if (TCHOLD.Count != 0)
                            {
                                for (int tr = 0; tr <= TCHOLD.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TCHOLD[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            var TINHISS = vTINHISS.Where(x => x.AUTONO == chkval).ToList();
                            if (TINHISS.Count != 0)
                            {
                                for (int tr = 0; tr <= TINHISS.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TINHISS[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            var TINHISSMST = vTINHISSMST.Where(x => x.AUTONO == chkval).ToList();
                            if (TINHISSMST.Count != 0)
                            {
                                for (int tr = 0; tr <= TINHISSMST.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TINHISSMST[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            var TINHISSMSTSJOB = vTINHISSMSTSJOB.Where(x => x.AUTONO == chkval).ToList();
                            if (TINHISSMSTSJOB.Count != 0)
                            {
                                for (int tr = 0; tr <= TINHISSMSTSJOB.Count() - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TINHISSMSTSJOB[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            //var TINHISSMSTDTL = vTINHISSMSTDTL.Where(x => x.AUTONO == chkval).ToList();
                            //if (TINHISSMSTDTL.Count != 0)
                            //{
                            //    for (int tr = 0; tr <= TINHISSMSTDTL.Count() - 1; tr++)
                            //    {
                            //        dbsql = MasterHelpFa.RetModeltoSql(TINHISSMSTDTL[tr]);
                            //        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            //    }
                            //}

                            int sl = 1;
                            while (tbl.Rows[i]["autono"].ToString() == chkval)
                            {
                                i++;
                                if (i > maxR) break;
                            }
                        }
                        #endregion
                        #region Insert Sticher Receive entry
                        var TCHOLDREC = vTCNTRLHDRREC.ToList();
                        if (TCHOLDREC.Count != 0)
                        {
                            for (int tr = 0; tr <= TCHOLDREC.Count() - 1; tr++)
                            {
                                dbsql = MasterHelpFa.RetModeltoSql(TCHOLDREC[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }

                        //var TINHREC = vTINHREC.ToList();
                        //if (TINHREC.Count != 0)
                        //{
                        //    for (int tr = 0; tr <= TINHREC.Count() - 1; tr++)
                        //    {
                        //        dbsql = MasterHelpFa.RetModeltoSql(TINHREC[tr]);
                        //        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        //    }
                        //}

                        var TINHRECMST = vTINHRECMST.ToList();
                        if (TINHRECMST.Count != 0)
                        {
                            for (int tr = 0; tr <= TINHRECMST.Count() - 1; tr++)
                            {
                                dbsql = MasterHelpFa.RetModeltoSql(TINHRECMST[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }

                        //var TINHRECMSTDTL = vTINHRECMSTDTL.ToList();
                        //if (TINHRECMSTDTL.Count != 0)
                        //{
                        //    for (int tr = 0; tr <= TINHRECMSTDTL.Count() - 1; tr++)
                        //    {
                        //        dbsql = MasterHelpFa.RetModeltoSql(TINHRECMSTDTL[tr]);
                        //        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        //    }
                        //}

                        #endregion
                    }

                    #endregion

                    #region Opening Stock
                    if (VE.Checkbox4 == true)
                    {
                        string dtype = "'FOSTK'";
                        string mtrljobcd = "FS";
                        if (VE.TEXTBOX3.retStr() != "") { mtrljobcd = VE.TEXTBOX3; dtype = "'OOSTK'"; }
                        sql = "select doccd from " + oldschema + ".m_doctype where doctype in (" + dtype + ") ";
                        rstmp = MasterHelp.SQLquery(sql);
                        seldoccd = rstmp.Rows[0]["doccd"].ToString();
                        string[] strdocd = seldoccd.Replace("'", "").Split(',');

                        defdoccd = seldoccd;
                        if (seldoccd.Length > 0) seldoccd = "'" + seldoccd + "'";
                        #region Record delete if found
                        sqlc = "select a.autono from " + newschema + ".t_cntrl_hdr a where a.doccd in (" + seldoccd + ") and a.calauto='T' and ";
                        sqlc += "a.yr_cd = '" + CommVar.YearCode(UNQSNO) + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' ";

                        sqlc = "select distinct a.autono from " + newschema + ".t_cntrl_hdr a, " + newschema + ".t_txndtl b ";
                        sqlc += "where a.autono=b.autono(+) and a.calauto='T' and b.mtrljobcd='" + mtrljobcd + "' and ";
                        sqlc += "a.yr_cd = '" + CommVar.YearCode(UNQSNO) + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' ";
                        DataTable tbldel = MasterHelp.SQLquery(sqlc);

                        query = "delete from " + newschema + ".t_txndtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_txn where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_cntrl_hdr where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                        if (dberrmsg != "") goto dbnotsave;
                        #endregion

                        string stktype = "'F','R','L','D'";
                        DataTable tblgocd = MasterHelpFa.SQLquery("select gocd from " + newschema + ".m_godown");

                        docno = Cn.MaxDocNumber(defdoccd, startdt);
                        //docno = "000001";
                        mxdocno = Convert.ToInt16(docno) - 1;

                        for (int iq = 0; iq <= tblgocd.Rows.Count - 1; iq++)
                        {
                            tbl = Salesfunc.GetStock(tdt, "'" + tblgocd.Rows[iq]["gocd"].ToString() + "'", "", "'" + mtrljobcd + "'", "", "", "", "", "", stktype, "", oldschema, true, false, "", "", false, false);
                            tbl.DefaultView.Sort = "brandnm, brandcd, itgrpnm, itgrpcd, styleno, itcd, stktype, print_seq, sizecdgrp, sizenm, partcd";
                            tbl = tbl.DefaultView.ToTable();

                            //DB.Database.ExecuteSqlCommand("lock table " + newschema + ".T_CNTRL_HDR in  row share mode");
                            maxR = tbl.Rows.Count - 1; i = 0;
                            while (i <= maxR)
                            {
                                mxdocno++;
                                docno = Convert.ToString(mxdocno).PadLeft(6, '0');
                                string DOCPATTERN = Cn.DocPattern(Convert.ToInt32(docno), defdoccd, CommVar.CurSchema(UNQSNO), CommVar.FinSchema(UNQSNO), startdt);
                                string auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), docno, defdoccd, startdt);
                                string tautono = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                                string Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();

                                T_CNTRL_HDR TCHOLD = new T_CNTRL_HDR();
                                TCHOLD = Cn.T_CONTROL_HDR(defdoccd, Convert.ToDateTime(startdt), docno, tautono, Month, DOCPATTERN, "A", CommVar.CurSchema(UNQSNO), null, null, 0, "T");
                                dbsql = MasterHelpFa.RetModeltoSql(TCHOLD);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                T_TXN TTXN = new T_TXN();
                                TTXN.AUTONO = TCHOLD.AUTONO;
                                TTXN.DOCCD = TCHOLD.DOCCD;
                                TTXN.DOCDT = TCHOLD.DOCDT;
                                TTXN.DOCNO = TCHOLD.DOCONLYNO;
                                TTXN.CLCD = TCHOLD.CLCD;
                                TTXN.GOCD = tblgocd.Rows[iq]["gocd"].ToString();
                                TTXN.DOCTAG = "OP";
                                dbsql = MasterHelpFa.RetModeltoSql(TTXN);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                string itcd = tbl.Rows[i]["itcd"].ToString();
                                string istktype = tbl.Rows[i]["stktype"].ToString();
                                int sl = 0;
                                while (tbl.Rows[i]["itcd"].ToString() == itcd && tbl.Rows[i]["stktype"].ToString() == istktype)
                                {
                                    sl++;
                                    double qty = tbl.Rows[i]["qnty"].retDbl();

                                    T_TXNDTL TTXNDTL = new T_TXNDTL();
                                    TTXNDTL.AUTONO = TTXN.AUTONO;
                                    TTXNDTL.SLNO = sl.retShort();
                                    TTXNDTL.CLCD = TTXN.CLCD;
                                    TTXNDTL.ITCD = itcd;
                                    //TTXNDTL.DOCCD = TTXN.DOCCD;
                                    //TTXNDTL.DOCNO = TTXN.DOCNO;
                                    //TTXNDTL.DOCDT = TTXN.DOCDT;
                                    TTXNDTL.GOCD = TTXN.GOCD;
                                    if (qty < 0) TTXNDTL.STKDRCR = "C"; else TTXNDTL.STKDRCR = "D";
                                    TTXNDTL.STKTYPE = tbl.Rows[i]["stktype"].ToString();
                                    TTXNDTL.MTRLJOBCD = mtrljobcd;
                                    //
                                    TTXNDTL.SCMDISCRATE = 0; TTXNDTL.SCMDISCAMT = 0;
                                    TTXNDTL.DISCRATE = 0; TTXNDTL.DISCAMT = 0;
                                    TTXNDTL.TDDISCRATE = 0; TTXNDTL.TDDISCAMT = 0;
                                    TTXNDTL.AMT = 0;
                                    //
                                    TTXNDTL.SIZECD = tbl.Rows[i]["sizecd"].ToString();
                                    TTXNDTL.COLRCD = tbl.Rows[i]["colrcd"].ToString();
                                    //TTXNDTL.STKQNTY = null;
                                    if (qty < 0) qty = qty * -1;
                                    TTXNDTL.QNTY = qty;

                                    dbsql = MasterHelpFa.RetModeltoSql(TTXNDTL);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                    i++;
                                    if (i > maxR) break;
                                }
                                if (i > maxR) break;
                            }
                        }
                    }
                    #endregion

                    #region Opening Stock (Batch wise)
                    if (VE.Checkbox5 == true)
                    {
                        if (VE.TEXTBOX3.retStr() == "") return "Material Job code not selected";

                        sql = "select doccd from " + oldschema + ".m_doctype where doctype in ('OOSTK') ";
                        rstmp = MasterHelp.SQLquery(sql);
                        defdoccd = rstmp.Rows[0]["doccd"].ToString();

                        sql = "select doccd from " + oldschema + ".m_doctype where doctype in ('OSTI','OEMI','OPRI','OIRI','ODYI','OBLI','OKTI','OWAI','OYDI')";
                        sql = "select doccd from " + oldschema + ".m_doctype where doctype not in ('ISI','ISR','ISLC','ISLW')";
                        rstmp = MasterHelp.SQLquery(sql);
                        seldoccd = string.Join("','", (from DataRow dr in rstmp.Rows select dr["doccd"].ToString()).Distinct());
                        string[] strdocd = seldoccd.Replace("'", "").Split(',');

                        //sql = "select doccd from " + oldschema + ".m_doctype where doctype in (" + dtype + ") ";
                        //rstmp = MasterHelp.SQLquery(sql);
                        //seldoccd = string.Join("','", (from DataRow dr in rstmp.Rows select dr["doccd"].ToString()).Distinct());

                        if (seldoccd.Length > 0) seldoccd = "'" + seldoccd + "'";

                        var vTCNTRLHDR = (from p in DBOLD.T_CNTRL_HDR
                                          where (strdocd.Contains(p.DOCCD))
                                          select p).ToList();

                        var vTTXN = (from p in DBOLD.T_TXN
                                     join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                     where (strdocd.Contains(q.DOCCD))
                                     select p).ToList();

                        var vTBATCHMST = (from p in DBOLD.T_BATCHMST
                                          join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                          where (strdocd.Contains(q.DOCCD))
                                          select p).ToList();

                        #region Record delete if found
                        sqlc = "select distinct a.autono from " + newschema + ".t_cntrl_hdr a, " + newschema + ".t_txndtl b ";
                        sqlc += "where a.autono=b.autono(+) and a.calauto='T' and b.mtrljobcd='" + VE.TEXTBOX3 + "' and ";
                        sqlc += "a.yr_cd = '" + CommVar.YearCode(UNQSNO) + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' ";
                        DataTable tbldel = MasterHelp.SQLquery(sqlc);

                        query = "delete from " + newschema + ".t_batchdtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        //query = "delete from " + newschema + ".t_batchmst where autono in (" + sqlc + ") ";
                        //OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_txndtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        //query = "delete from " + newschema + ".t_txn where autono in (" + sqlc + ") ";
                        //OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        //query = "delete from " + newschema + ".t_cntrl_hdr where autono in (" + sqlc + ") ";
                        //OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                        if (dberrmsg != "") goto dbnotsave;
                        #endregion

                        string stktype = "'F','R','L','D'";
                        DataTable tblgocd = MasterHelpFa.SQLquery("select gocd from " + newschema + ".m_godown");

                        docno = Cn.MaxDocNumber(defdoccd, startdt);
                        mxdocno = Convert.ToInt16(docno) - 1;

                        for (int iq = 0; iq <= tblgocd.Rows.Count - 1; iq++)
                        {
                            tbl = Salesfunc.GetStock(tdt, "'" + tblgocd.Rows[iq]["gocd"].ToString() + "'", "", "'" + VE.TEXTBOX3 + "'", "", "", "", "", "", stktype, "", oldschema);

                            #region insert of orgbatchautono
                            tbl.DefaultView.Sort = "orgbatchautono, orgbatchslno";
                            DataTable tblorg = tbl.DefaultView.ToTable();
                            maxR = tblorg.Rows.Count - 1; i = 0;
                            orgstart:
                            while (i <= maxR)
                            {
                                chkval = tblorg.Rows[i]["orgbatchautono"].retStr();
                                if (chkval.retStr() == "") { i++; goto orgstart; }
                                sql = "select autono from " + newschema + ".t_cntrl_hdr where autono='" + chkval + "'";
                                OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                if (recoexist == false)
                                {
                                    var TCHOLD = vTCNTRLHDR.Where(x => x.AUTONO == chkval).ToList();
                                    if (TCHOLD.Count != 0)
                                    {
                                        for (int tr = 0; tr <= TCHOLD.Count() - 1; tr++)
                                        {
                                            TCHOLD[tr].CALAUTO = "T";
                                            dbsql = MasterHelpFa.RetModeltoSql(TCHOLD[tr], (recoexist == false ? "A" : "E"));
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                sql = "select autono from " + newschema + ".t_txn where autono='" + chkval + "'";
                                OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                if (recoexist == false)
                                {
                                    var TXN = vTTXN.Where(x => x.AUTONO == chkval).ToList();
                                    if (TXN.Count != 0)
                                    {
                                        for (int tr = 0; tr <= TXN.Count() - 1; tr++)
                                        {
                                            dbsql = MasterHelpFa.RetModeltoSql(TXN[tr], (recoexist == false ? "A" : "E"));
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                                while (tblorg.Rows[i]["orgbatchautono"].retStr() == chkval)
                                {
                                    int oslno = tblorg.Rows[i]["orgbatchslno"].retInt();
                                    sql = "select autono from " + newschema + ".t_batchmst where autono='" + chkval + "' and slno=" + oslno;
                                    OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                    if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                    if (recoexist == false)
                                    {
                                        var BATCHMST = vTBATCHMST.Where(x => x.AUTONO == chkval & x.SLNO == oslno).ToList();
                                        if (BATCHMST.Count != 0)
                                        {
                                            for (int tr = 0; tr <= BATCHMST.Count() - 1; tr++)
                                            {
                                                dbsql = MasterHelpFa.RetModeltoSql(BATCHMST[tr], (recoexist == false ? "A" : "E"));
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                    while (tblorg.Rows[i]["orgbatchautono"].retStr() == chkval && tblorg.Rows[i]["orgbatchslno"].retInt() == oslno)
                                    {
                                        i++;
                                        if (i > maxR) break;
                                    }
                                    if (i > maxR) break;
                                }
                            }
                            #endregion

                            #region insert of batchautono qnty
                            tbl.DefaultView.Sort = "itcd, batchautono, batchslno";
                            tbl = tbl.DefaultView.ToTable();
                            maxR = tbl.Rows.Count - 1; i = 0;
                            while (i <= maxR)
                            {
                                mxdocno++;
                                docno = Convert.ToString(mxdocno).PadLeft(6, '0');
                                string DOCPATTERN = Cn.DocPattern(Convert.ToInt32(docno), defdoccd, CommVar.CurSchema(UNQSNO), CommVar.FinSchema(UNQSNO), startdt);
                                string auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), docno, defdoccd, startdt);
                                string tautono = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                                string Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();

                                T_CNTRL_HDR xTCHOLD = new T_CNTRL_HDR();
                                xTCHOLD = Cn.T_CONTROL_HDR(defdoccd, Convert.ToDateTime(startdt), docno, tautono, Month, DOCPATTERN, "A", CommVar.CurSchema(UNQSNO), null, null, 0, null);
                                xTCHOLD.CALAUTO = "T";
                                dbsql = MasterHelpFa.RetModeltoSql(xTCHOLD);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                T_TXN TTXN = new T_TXN();
                                TTXN.AUTONO = xTCHOLD.AUTONO;
                                TTXN.DOCCD = xTCHOLD.DOCCD;
                                TTXN.DOCDT = xTCHOLD.DOCDT;
                                TTXN.DOCNO = xTCHOLD.DOCONLYNO;
                                TTXN.CLCD = xTCHOLD.CLCD;
                                TTXN.GOCD = tblgocd.Rows[iq]["gocd"].ToString();
                                TTXN.DOCTAG = "OP";
                                dbsql = MasterHelpFa.RetModeltoSql(TTXN);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                string itcd = tbl.Rows[i]["itcd"].retStr();
                                int tslno = 0;
                                bool insbatch = false;
                                while (tbl.Rows[i]["itcd"].retStr() == itcd)
                                {
                                    chkval = tbl.Rows[i]["batchautono"].retStr();
                                    int slno = tbl.Rows[i]["batchslno"].retInt();
                                    tslno++;
                                    insbatch = chkval.retStr() == "" ? true : false;
                                    if (chkval.retStr() != "")
                                    {
                                        sql = "select autono from " + newschema + ".t_cntrl_hdr where autono='" + chkval + "'";
                                        OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                        if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                        if (recoexist == false)
                                        {
                                            var TCHOLD = vTCNTRLHDR.Where(x => x.AUTONO == chkval).ToList();
                                            if (TCHOLD.Count != 0)
                                            {
                                                for (int tr = 0; tr <= TCHOLD.Count() - 1; tr++)
                                                {
                                                    TCHOLD[tr].CALAUTO = "T";
                                                    TCHOLD[tr].LOCCD = CommVar.Loccd(UNQSNO);
                                                    dbsql = MasterHelpFa.RetModeltoSql(TCHOLD[tr], (recoexist == false ? "A" : "E"));
                                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                        sql = "select autono from " + newschema + ".t_txn where autono='" + chkval + "'";
                                        OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                        if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                        if (recoexist == false)
                                        {
                                            var TXN = vTTXN.Where(x => x.AUTONO == chkval).ToList();
                                            if (TXN.Count != 0)
                                            {
                                                for (int tr = 0; tr <= TXN.Count() - 1; tr++)
                                                {
                                                    dbsql = MasterHelpFa.RetModeltoSql(TXN[tr], (recoexist == false ? "A" : "E"));
                                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        chkval = xTCHOLD.AUTONO;
                                        slno = tslno;
                                    }

                                    sql = "select autono from " + newschema + ".t_batchmst where autono='" + chkval + "' and slno=" + slno;
                                    OraCmd.CommandText = sql; var xOraReco = OraCmd.ExecuteReader();
                                    if (xOraReco.HasRows == false) recoexist = false; else recoexist = true; xOraReco.Dispose();
                                    var BATCHMST = vTBATCHMST.Where(x => x.AUTONO == chkval & x.SLNO == slno).ToList();
                                    if (recoexist == false)
                                    {
                                        if (BATCHMST.Count != 0)
                                        {
                                            for (int tr = 0; tr <= BATCHMST.Count() - 1; tr++)
                                            {
                                                dbsql = MasterHelpFa.RetModeltoSql(BATCHMST[tr], (recoexist == false ? "A" : "E"));
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                    if (insbatch == true)
                                    {
                                        T_BATCHMST xTBATCHMST = new T_BATCHMST();
                                        xTBATCHMST.CLCD = xTCHOLD.CLCD;
                                        xTBATCHMST.AUTONO = xTCHOLD.AUTONO;
                                        xTBATCHMST.SLNO = tslno;
                                        //xTBATCHMST.BATCHAUTONO = xTCHOLD.AUTONO;
                                        //xTBATCHMST.BATCHSLNO = tslno;
                                        //xTBATCHMST.DOCCD = xTCHOLD.DOCCD;
                                        //xTBATCHMST.DOCDT = xTCHOLD.DOCDT;
                                        //xTBATCHMST.DOCNO = xTCHOLD.DOCONLYNO;
                                        xTBATCHMST.STKTYPE = tbl.Rows[i]["stktype"].ToString();
                                        xTBATCHMST.ITCD = itcd;
                                        xTBATCHMST.PARTCD = tbl.Rows[i]["partcd"].ToString();
                                        xTBATCHMST.SIZECD = tbl.Rows[i]["sizecd"].ToString();
                                        xTBATCHMST.BATCHNO = tbl.Rows[i]["batchno"].retStr();
                                        xTBATCHMST.NOS = Math.Abs(tbl.Rows[i]["nos"].retDbl());
                                        xTBATCHMST.QNTY = Math.Abs(tbl.Rows[i]["qnty"].retDbl());
                                        xTBATCHMST.MTRLJOBCD = VE.TEXTBOX3;
                                        dbsql = MasterHelpFa.RetModeltoSql(xTBATCHMST);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }

                                    T_TXNDTL TTXNDTL = new T_TXNDTL();
                                    TTXNDTL.AUTONO = xTCHOLD.AUTONO;
                                    TTXNDTL.SLNO = tslno.retShort();
                                    TTXNDTL.CLCD = xTCHOLD.CLCD;
                                    TTXNDTL.ITCD = itcd;
                                    //TTXNDTL.DOCCD = xTCHOLD.DOCCD;
                                    //TTXNDTL.DOCNO = xTCHOLD.DOCONLYNO;
                                    //TTXNDTL.DOCDT = xTCHOLD.DOCDT;
                                    TTXNDTL.GOCD = tblgocd.Rows[iq]["gocd"].ToString();
                                    TTXNDTL.STKDRCR = tbl.Rows[i]["qnty"].retDbl() < 0 ? "C" : "D";
                                    TTXNDTL.STKTYPE = tbl.Rows[i]["stktype"].ToString();
                                    TTXNDTL.MTRLJOBCD = VE.TEXTBOX3;
                                    //
                                    TTXNDTL.SCMDISCRATE = 0; TTXNDTL.SCMDISCAMT = 0;
                                    TTXNDTL.DISCRATE = 0; TTXNDTL.DISCAMT = 0;
                                    TTXNDTL.TDDISCRATE = 0; TTXNDTL.TDDISCAMT = 0;
                                    TTXNDTL.AMT = 0;
                                    //
                                    TTXNDTL.SIZECD = tbl.Rows[i]["sizecd"].ToString();
                                    TTXNDTL.COLRCD = tbl.Rows[i]["colrcd"].ToString();
                                    TTXNDTL.PARTCD = tbl.Rows[i]["partcd"].ToString();
                                    //TTXNDTL.STKQNTY = null;
                                    TTXNDTL.QNTY = Math.Abs(tbl.Rows[i]["qnty"].retDbl());

                                    dbsql = MasterHelpFa.RetModeltoSql(TTXNDTL);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                    T_BATCHDTL BATCHDTL = new T_BATCHDTL();
                                    BATCHDTL.CLCD = xTCHOLD.CLCD;
                                    BATCHDTL.AUTONO = xTCHOLD.AUTONO;
                                    BATCHDTL.SLNO = tslno.retShort();
                                    //BATCHDTL.BATCHAUTONO = insbatch == true ? xTCHOLD.AUTONO : BATCHMST[0].BATCHAUTONO;
                                    //BATCHDTL.BATCHSLNO = insbatch == true ? tslno : BATCHMST[0].BATCHSLNO;
                                    //BATCHDTL.DOCCD = xTCHOLD.DOCCD;
                                    //BATCHDTL.DOCDT = xTCHOLD.DOCDT;
                                    //BATCHDTL.DOCNO = xTCHOLD.DOCONLYNO;
                                    BATCHDTL.BATCHNO = tbl.Rows[i]["batchno"].retStr();
                                    BATCHDTL.STKDRCR = tbl.Rows[i]["qnty"].retDbl() < 0 ? "C" : "D";
                                    BATCHDTL.NOS = Math.Abs(tbl.Rows[i]["nos"].retDbl());
                                    BATCHDTL.QNTY = Math.Abs(tbl.Rows[i]["qnty"].retDbl());
                                    dbsql = MasterHelpFa.RetModeltoSql(BATCHDTL);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    i++;
                                    if (i > maxR) break;
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion

                    goto dbok;
                    dbnotsave:;
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    return sql + Cn.GCS() + dberrmsg;
                    dbok:;
                    ModelState.Clear();
                    transaction.Commit();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    if (VE.Checkbox3 == true)
                    {
                        //sql = "alter table " + newschema + ".t_progdtl enable constraint fkey_t_progdtl_progautono";
                        //OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }
                    return "Transferred sucessfully";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    return ex.Message + ex.InnerException;
                }
            }
        }
    }
}
