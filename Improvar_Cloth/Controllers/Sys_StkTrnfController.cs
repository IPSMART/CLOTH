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
        MasterHelp masterHelp = new MasterHelp();
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
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    string selgrp = masterHelp.GetItemITGrpCd().Replace("','", ",");
                    string[] selgrpcd = selgrp.Split(',');

                    VE.DropDown_list = (from i in DB.M_GROUP where (selgrpcd.Contains(i.ITGRPCD)) select new DropDown_list() { value = i.ITGRPCD, text = i.ITGRPNM }).OrderBy(s => s.text).ToList();

                    VE.DefaultView = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

        public ActionResult Sys_StkTrnf1(ReportViewinHtml VE)
        {
            string retmsg = "", oldschema = "", oldFinschema = "", newschema = "", newFinschema = "", yrcd = CommVar.YearCode(UNQSNO);
            yrcd = (Convert.ToDecimal(yrcd) - 1).ToString();
            newschema = CommVar.CurSchema(UNQSNO);
            oldschema = CommVar.CurSchema(UNQSNO).Substring(0, CommVar.CurSchema(UNQSNO).Length - 4) + yrcd;
            oldFinschema = CommVar.FinSchema(UNQSNO).Substring(0, CommVar.FinSchema(UNQSNO).Length - 4) + yrcd;
            newschema = newschema.ToUpper(); oldschema = oldschema.ToUpper();
            oldschema = oldschema.ToUpper(); oldFinschema = oldFinschema.ToUpper();
            retmsg = Stock_Transfer(VE.TEXTBOX1, oldschema, oldFinschema, newschema, newFinschema, VE);
            return Content(retmsg);
        }
        public string Stock_Transfer(string itgrpcd, string oldschema, string oldFinschema, string newschema, string newFinschema, ReportViewinHtml VE)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf1 = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            yrcd = (Convert.ToDecimal(yrcd) - 1).ToString();
            scm1 = oldschema;
            scmf1 = oldFinschema;

            string fdt = "";
            string RateQntyBAg = "B";
            string query = ""; string query1 = "";
            string chkval, chkval1 = "";
            string dbsql = ""; string[] dbsql1;

            //scm1 = newschema;
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

            using (var transaction = DB.Database.BeginTransaction())
            {
                var errorAutono = "";
                try
                {
                    string sqlc = "", dberrmsg = "";
                    Int32 i = 0; Int32 maxR = 0;
                    DataTable tbl = new DataTable();

                    string lastdayofprvyear = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).AddDays(-1).retDateStr();

                    #region Stock Transfer
                    DataTable tbltmp = new DataTable();

                    bool batchwise = true;
                    if (VE.Checkbox2 == true || VE.Checkbox7 == true)
                    {
                        sql = "select distinct a.gocd from " + oldschema + ".t_txn a, " + oldschema + ".t_cntrl_hdr b, " + oldschema + ".t_txndtl c , " + oldschema + ".m_sitem d ";
                        sql += "where a.autono = b.autono and a.autono=c.autono and c.itcd=d.itcd and b.compcd = '" + CommVar.Compcd(UNQSNO) + "' and b.loccd = '" + CommVar.Loccd(UNQSNO) + "'  ";
                        tbltmp = masterHelp.SQLquery(sql);

                        string[] selgocd = string.Join(",", (from DataRow dr in tbltmp.Rows select dr["gocd"].ToString()).Distinct()).Split(',');
                        string gocd = selgocd.retSqlfromStrarray();
                        if (selgocd[0] == "") gocd = "";
                        if (VE.Checkbox7 == true)
                        {
                            //tbl = Salesfunc.GetStockFifo("FIFO", lastdayofprvyear, "", "", "", "", gocd, false, "", false, "", scm1, scmf1, "", "CP");
                            tbl = Salesfunc.GenStocktblwithVal("FIFO", lastdayofprvyear, "", "", "", "", gocd, false,"", false,"", scm1,"", scmf1);
                        }
                        else
                        {
                            tbl = Salesfunc.GetStock(lastdayofprvyear, gocd, "", "", "FS".retSqlformat(), "", "", "", "CP", "C001", "", "", true, false, scm1, scmf1);
                        }

                        DataView dv = tbl.DefaultView;
                        dv.Sort = "autono,gocd,barno";
                        tbl = dv.ToTable();

                        i = 0; maxR = 0;
                        maxR = tbl.Rows.Count - 1;

                        sqlc = "select distinct a.autono from " + newschema + ".t_txn a, " + newschema + ".t_cntrl_hdr b, " + newschema + ".m_doctype c, " + newschema + ".t_txndtl d , ";
                        sqlc += newschema + ".m_sitem e, " + newschema + ".t_batchdtl f ";
                        sqlc += "where b.doccd=c.doccd(+) and a.autono=b.autono(+) and a.autono=d.autono and d.itcd=e.itcd and d.autono=f.autono(+) and d.slno=f.txnslno(+) and ";
                        //sqlc += "(b.yr_cd < '" + CommVar.YearCode(UNQSNO) + "' or c.doctype in ('FOSTK')) and ";
                        sqlc += "f.millnm in ('OPSTOCK') and ";
                        sqlc += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' ";
                        DataTable tbldel = masterHelp.SQLquery(sqlc);

                        query = "delete from " + newschema + ".t_batchdtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        for (Int32 q = 0; q <= tbldel.Rows.Count - 1; q++)
                        {
                            query = "delete from " + newschema + ".t_txndtl where autono='" + tbldel.Rows[q]["autono"].ToString() + "' ";
                            OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                            query = "delete from " + newschema + ".T_TXNOTH where autono='" + tbldel.Rows[q]["autono"].ToString() + "' ";
                            OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                            query = "delete from " + newschema + ".T_TXNTRANS where autono='" + tbldel.Rows[q]["autono"].ToString() + "' ";
                            OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        }


                        if (dberrmsg != "") goto dbnotsave;

                        string defdoccd = "", docno = ""; int mxdocno = 0;

                        sql = "select a.doccd from " + oldschema + ".m_doctype a where a.doctype = 'FOSTK' ";
                        tbltmp = masterHelp.SQLquery(sql);
                        if (tbltmp.Rows.Count > 0)
                        {
                            defdoccd = tbltmp.Rows[0]["doccd"].retStr();
                            docno = Cn.MaxDocNumber(defdoccd, lastdayofprvyear);
                            mxdocno = Convert.ToInt16(docno) - 1;
                        }
                        else
                        {
                            OraTrans.Rollback();
                            OraTrans.Dispose();
                            return "Please add **OPENING DOCTYPE**";
                        }
                        var vTXN = (from p in DBOLD.T_TXN
                                    join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                    where (q.COMPCD == COM)
                                    select p).ToList();

                        var vTXNOTH = (from p in DBOLD.T_TXNOTH
                                       join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       where (q.COMPCD == COM)
                                       select p).ToList();

                        var vTXNTRANS = (from p in DBOLD.T_TXNTRANS
                                         join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                         where (q.COMPCD == COM)
                                         select p).ToList();

                        var vTBATCHMSTPRICE = (from p in DBOLD.T_BATCHMST_PRICE
                                               select p).ToList();

                        var vTBATCHMST = (from p in DBOLD.T_BATCHMST
                                          select p).ToList();

                        i = 0; maxR = 0;
                        maxR = tbl.Rows.Count - 1;
                        string orgdocdt = "", godown = "";
                        string newautono = "", oldautono = "";
                        OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
                        while (i <= maxR)
                        {
                            orgdocdt = tbl.Rows[i]["docdt"].retDateStr() == "" ? lastdayofprvyear : tbl.Rows[i]["docdt"].retDateStr();

                            oldautono = tbl.Rows[i]["autono"].ToString();

                            T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                            T_CNTRL_HDR TCHOLD = new T_CNTRL_HDR();

                            mxdocno++;
                            docno = Convert.ToString(mxdocno).PadLeft(6, '0');
                            string DOCPATTERN = Cn.DocPattern(Convert.ToInt32(docno), defdoccd, CommVar.CurSchema(UNQSNO), CommVar.FinSchema(UNQSNO), lastdayofprvyear);
                            string auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), docno, defdoccd, lastdayofprvyear);
                            string tautono = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            string Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                            TCHOLD = Cn.T_CONTROL_HDR(defdoccd, Convert.ToDateTime(orgdocdt), docno, tautono, Month, DOCPATTERN, "A", CommVar.CurSchema(UNQSNO), null, null, 0, null);
                            newautono = TCHOLD.AUTONO;

                            dbsql = MasterHelpFa.RetModeltoSql(TCHOLD);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            T_TXN TTXN = new T_TXN();
                            bool insTxn = (oldautono == "" ? true : false);
                            if (oldautono != "")
                            {
                                var TXNOLD = vTXN.Where(x => x.AUTONO == oldautono).ToList();
                                if (TXNOLD.Count == 0)
                                {
                                    insTxn = true;
                                }
                                else
                                {
                                    TXNOLD[0].AUTONO = TCHOLD.AUTONO;
                                    TXNOLD[0].GOCD = tbl.Rows[i]["gocd"].ToString();
                                    TXNOLD[0].DOCDT = Convert.ToDateTime(orgdocdt);
                                    TXNOLD[0].DOCCD = TCHOLD.DOCCD;
                                    TXNOLD[0].DOCTAG = tbl.Rows[i]["doctag"].ToString() == "" ? "OP" : tbl.Rows[i]["doctag"].ToString();
                                    TXNOLD[0].DOCNO = TCHOLD.DOCONLYNO;
                                    TXNOLD[0].SLCD = tbl.Rows[i]["slcd"].ToString();
                                    TXNOLD[0].CONSLCD = tbl.Rows[i]["conslcd"].ToString();
                                    TXNOLD[0].PREFNO = tbl.Rows[i]["prefno"].ToString();
                                    string prefdt = tbl.Rows[i]["prefdt"].ToString();
                                    if (prefdt != "") TXNOLD[0].PREFDT = Convert.ToDateTime(prefdt);
                                    TTXN = TXNOLD[0];

                                    //if (recoexist == false)
                                    //{
                                    dbsql = MasterHelpFa.RetModeltoSql(TXNOLD[0]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    //}
                                    //else
                                    //{
                                    //    dbsql = MasterHelpFa.RetModeltoSql(TXNOLD[0], "E");
                                    //    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    //}
                                }
                            }

                            if (insTxn == true)
                            {
                                TTXN.DOCDT = Convert.ToDateTime(orgdocdt);
                                TTXN.DOCCD = TCHOLD.DOCCD;
                                TTXN.DOCTAG = tbl.Rows[i]["doctag"].ToString() == "" ? "OP" : tbl.Rows[i]["doctag"].ToString();
                                TTXN.DOCNO = TCHOLD.DOCONLYNO;
                                TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                                TTXN.EMD_NO = null;
                                TTXN.AUTONO = TCHOLD.AUTONO;
                                TTXN.SLCD = tbl.Rows[i]["slcd"].ToString();
                                TTXN.CONSLCD = tbl.Rows[i]["conslcd"].ToString();
                                TTXN.PREFNO = tbl.Rows[i]["prefno"].ToString();// (batchwise == true ? tbl.Rows[i]["pblno"].ToString() : tbl.Rows[i]["prefno"].ToString());
                                string prefdt = tbl.Rows[i]["prefdt"].ToString();// (batchwise == true ? tbl.Rows[i]["pbldt"].ToString() : tbl.Rows[i]["prefdt"].ToString());
                                if (prefdt != "") TTXN.PREFDT = Convert.ToDateTime(prefdt);
                                TTXN.GOCD = tbl.Rows[i]["gocd"].ToString();
                                TTXN.BLAMT = 0;
                                //if (recoexist == false)
                                //{
                                dbsql = MasterHelpFa.RetModeltoSql(TTXN);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                //}
                                //else
                                //{
                                //    dbsql = MasterHelpFa.RetModeltoSql(TTXN, "E");
                                //    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                //}
                            }

                            //if (recoexist == false)
                            //{
                            //if (orgautono != "")
                            //{
                            var TXNOTHOLD = vTXNOTH.Where(x => x.AUTONO == oldautono).ToList();
                            if (TXNOTHOLD.Count > 0)
                            {
                                TXNOTHOLD[0].AUTONO = TCHOLD.AUTONO;
                                dbsql = MasterHelpFa.RetModeltoSql(TXNOTHOLD[0]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                            //}

                            //if (orgautono != "")
                            //{
                            var TXNTRANSOLD = vTXNTRANS.Where(x => x.AUTONO == oldautono).ToList();
                            if (TXNTRANSOLD.Count > 0)
                            {
                                TXNTRANSOLD[0].AUTONO = TCHOLD.AUTONO;
                                dbsql = MasterHelpFa.RetModeltoSql(TXNTRANSOLD[0]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                            //}
                            //}

                            int sl = 1;
                            while (tbl.Rows[i]["autono"].ToString() == oldautono)
                            {
                                errorAutono = tbl.Rows[i]["barno"].retStr();
                                if (errorAutono == "11700190")
                                {
                                    var aa = "";
                                }
                                string itcd = tbl.Rows[i]["itcd"].retStr();
                                string barno = tbl.Rows[i]["barno"].retStr();
                                double iqty = 0; int inos = 0; Int32 chki = i;
                                double slno = 0;
                                if (tbl.Rows[i]["slno"].retDbl() != 0) slno = Convert.ToDouble(tbl.Rows[i]["slno"]);

                                errorAutono = tbl.Rows[i]["barno"].retStr();
                                if (errorAutono == "10000304")
                                {
                                    var aa = "";
                                }
                                double basamt = 0, rate = 0, othramt = 0, othrate = 0;

                                if (batchwise == true)
                                {
                                    bool recoexistbatch = false;

                                    sql = "select barno from " + newschema + ".t_batchmst where barno='" + tbl.Rows[i]["barno"].ToString() + "'";
                                    OraCmd.CommandText = sql; var OraRecoBatch = OraCmd.ExecuteReader();
                                    if (OraRecoBatch.HasRows == false) recoexistbatch = false; else recoexistbatch = true; OraRecoBatch.Dispose();

                                    T_BATCHMST TBATCHMST = new T_BATCHMST();
                                    var BATCHMST = vTBATCHMST.Where(x => x.BARNO == barno).ToList();
                                    if (BATCHMST.Count > 0)
                                    {
                                        BATCHMST[0].AUTONO = TTXN.AUTONO;
                                        BATCHMST[0].SLNO = tbl.Rows[i]["slno"].retDbl() == 0 ? 1 : tbl.Rows[i]["slno"].retInt();
                                        TBATCHMST = BATCHMST[0];

                                        if (recoexistbatch == false)
                                        {
                                            dbsql = MasterHelpFa.RetModeltoSql(TBATCHMST);
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                        }
                                        //else
                                        //{
                                        //    dbsql = MasterHelpFa.RetModeltoSql(TBATCHMST, "E");
                                        //    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                        //}

                                    }
                                }
                                if (batchwise == true)
                                {
                                    barno = tbl.Rows[i]["barno"].ToString();
                                    var TBATCHMSTPRICEOLD = vTBATCHMSTPRICE.Where(x => x.BARNO == barno).ToList();
                                    if (TBATCHMSTPRICEOLD.Count > 0)
                                    {
                                        foreach (var v in TBATCHMSTPRICEOLD)
                                        {
                                            bool recoexistbatchprice = false;

                                            sql = "select barno from " + newschema + ".t_batchmst_price where barno='" + tbl.Rows[i]["barno"].ToString() + "' and prccd = '" + v.PRCCD + "' and effdt =  to_date('" + v.EFFDT.retDateStr() + "','dd/mm/yyyy')";
                                            OraCmd.CommandText = sql; var OraRecoBatchprice = OraCmd.ExecuteReader();
                                            if (OraRecoBatchprice.HasRows == false) recoexistbatchprice = false; else recoexistbatchprice = true; OraRecoBatchprice.Dispose();

                                            T_BATCHMST_PRICE TBATCHMSTPRICE = new T_BATCHMST_PRICE();
                                            v.AUTONO = TCHOLD.AUTONO;
                                            TBATCHMSTPRICE = v;

                                            if (recoexistbatchprice == false)
                                            {
                                                dbsql = MasterHelpFa.RetModeltoSql(TBATCHMSTPRICE);
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                            }
                                            //else
                                            //{
                                            //    dbsql = MasterHelpFa.RetModeltoSql(TBATCHMSTPRICE, "E");
                                            //    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                            //}
                                        }

                                    }
                                }

                                //int rsl = 1;
                                godown = tbl.Rows[i]["gocd"].ToString();
                                int rsl = (from DataRow dr in tbl.Rows where dr["autono"].ToString() == oldautono && dr["slno"].retDbl() == slno && dr["gocd"].ToString() == godown select dr["slno"].retDbl()).Max().retInt();
                                rsl = rsl.retDbl() == 0 ? 1 : rsl;
                                var aa1 = tbl.Rows[i]["autono"].ToString() + "/" + tbl.Rows[i]["slno"].retDbl() + "/" + tbl.Rows[i]["gocd"].ToString();
                                while (tbl.Rows[i]["autono"].ToString() == oldautono && tbl.Rows[i]["slno"].retDbl() == slno && tbl.Rows[i]["gocd"].ToString() == godown)
                                {
                                    errorAutono = tbl.Rows[i]["barno"].retStr();
                                    if (errorAutono == "11700190")
                                    {
                                        var aa = "";
                                    }
                                    if (batchwise == true) inos = tbl.Rows[i]["balnos"].retShort();
                                    iqty = Convert.ToDouble(tbl.Rows[i]["balqnty"]);
                                    rate = tbl.Rows[i]["rate"].retDbl();
                                    basamt = (rate * iqty).toRound();
                                    T_TXNDTL TTXNDTL = new T_TXNDTL();
                                    TTXNDTL.CLCD = TTXN.CLCD;
                                    TTXNDTL.EMD_NO = TTXN.EMD_NO;
                                    TTXNDTL.DTAG = TTXN.DTAG;
                                    TTXNDTL.AUTONO = TTXN.AUTONO;
                                    TTXNDTL.SLNO = tbl.Rows[i]["slno"].retDbl() == 0 ? rsl.retShort() : tbl.Rows[i]["slno"].retShort();
                                    var tmpitcd = itcd.Trim();
                                    if (tmpitcd.Length > 10)
                                    {
                                        tmpitcd = tmpitcd.Remove(10);
                                    }
                                    TTXNDTL.ITCD = tmpitcd;
                                    TTXNDTL.STKDRCR = (iqty < .0001 ? "C" : "D");
                                    TTXNDTL.NOS = TTXNDTL.STKDRCR == "C" ? inos * -1 : inos * 1;
                                    TTXNDTL.QNTY = TTXNDTL.STKDRCR == "C" ? iqty * -1 : iqty * 1;
                                    TTXNDTL.SHORTQNTY = 0;
                                    TTXNDTL.RATE = tbl.Rows[i]["rate"].retDbl();
                                    TTXNDTL.AMT = basamt;
                                    TTXNDTL.MTRLJOBCD = tbl.Rows[i]["MTRLJOBCD"].retStr();
                                    TTXNDTL.STKTYPE = tbl.Rows[i]["stktype"].retStr();
                                    TTXNDTL.GOCD = TTXN.GOCD;

                                    dbsql = MasterHelpFa.RetModeltoSql(TTXNDTL);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    if (batchwise == true)
                                    {
                                        T_BATCHDTL BATCHDTL = new T_BATCHDTL();
                                        BATCHDTL.AUTONO = TTXN.AUTONO;
                                        BATCHDTL.SLNO = TTXNDTL.SLNO;
                                        BATCHDTL.TXNSLNO = TTXNDTL.SLNO;
                                        BATCHDTL.EMD_NO = TTXN.EMD_NO;
                                        BATCHDTL.DTAG = TTXN.DTAG;
                                        BATCHDTL.CLCD = TTXN.CLCD;
                                        BATCHDTL.NOS = TTXNDTL.NOS;// Convert.ToInt32(tbl.Rows[i]["nos"]);
                                        BATCHDTL.QNTY = TTXNDTL.QNTY;// tbl.Rows[i]["qnty"].retDbl();
                                        BATCHDTL.RATE = TTXNDTL.RATE;
                                        BATCHDTL.OTHRAMT = 0;
                                        BATCHDTL.GOCD = TTXN.GOCD;
                                        BATCHDTL.BARNO = tbl.Rows[i]["barno"].ToString();
                                        BATCHDTL.MTRLJOBCD = TTXNDTL.MTRLJOBCD;
                                        BATCHDTL.STKTYPE = TTXNDTL.STKTYPE;
                                        BATCHDTL.STKDRCR = TTXNDTL.STKDRCR;
                                        BATCHDTL.MILLNM = "OPSTOCK";

                                        dbsql = MasterHelpFa.RetModeltoSql(BATCHDTL);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


                                    }
                                    if (tbl.Rows[i]["slno"].retDbl() == 0)
                                    {
                                        rsl = rsl + 1;
                                    }
                                    i += 1;
                                    if (i > maxR) break;
                                    //if (chkval == "") break;
                                }
                                sl = sl + 1;
                                if (i > maxR) break;
                                if (oldautono == "") break;
                            }
                        }
                    }
                    #endregion

                    #region Bale Stock Transfer
                    tbltmp = new DataTable();
                    batchwise = true;
                    if (VE.Checkbox8 == true)
                    {
                        sql = "select distinct a.gocd from " + oldschema + ".t_txn a, " + oldschema + ".t_cntrl_hdr b, " + oldschema + ".t_txndtl c , " + oldschema + ".m_sitem d ";
                        sql += "where a.autono = b.autono and a.autono=c.autono and c.itcd=d.itcd and b.compcd = '" + CommVar.Compcd(UNQSNO) + "' and b.loccd = '" + CommVar.Loccd(UNQSNO) + "' ";
                        tbltmp = masterHelp.SQLquery(sql);

                        string[] selgocd = string.Join(",", (from DataRow dr in tbltmp.Rows select dr["gocd"].ToString()).Distinct()).Split(',');
                        string gocd = selgocd.retSqlfromStrarray();
                        if (selgocd[0] == "") gocd = "";
                        tbl = Salesfunc.GetBaleStock(lastdayofprvyear, gocd, "", "", "", "", "", "", scm1, scmf1);

                        DataView dv = tbl.DefaultView;
                        dv.Sort = "blautono, blslno, baleyr, baleno";
                        tbl = dv.ToTable();

                        i = 0; maxR = 0;
                        maxR = tbl.Rows.Count - 1;

                        sqlc = "select distinct a.autono,d.slno from " + newschema + ".t_txn a, " + newschema + ".t_cntrl_hdr b, " + newschema + ".m_doctype c, " + newschema + ".t_txndtl d , ";
                        sqlc += newschema + ".m_sitem e, " + newschema + ".t_batchdtl f," + newschema + ".t_bale g ";
                        sqlc += "where  b.doccd=c.doccd(+) and a.autono=b.autono(+) and a.autono=d.autono and d.itcd=e.itcd and d.autono=f.autono(+) and d.slno=f.txnslno(+) ";
                        sqlc += "and a.autono=g.autono(+)  and d.slno = g.slno and ";
                        sqlc += "b.yr_cd < '" + CommVar.YearCode(UNQSNO) + "' and f.millnm in ('BALESTOCK') and ";
                        sqlc += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' ";
                        sqlc += "and g.baleno||g.baleyr not in (select a.baleno || a.baleyr from " + newschema + ".t_bale a," + newschema + ".t_cntrl_hdr b where a.autono = b.autono and b.yr_cd = '" + CommVar.YearCode(UNQSNO) + "') ";
                        sqlc += "order by a.autono,d.slno ";
                        DataTable tbldel = masterHelp.SQLquery(sqlc);

                        string sqlc1 = "select distinct autono from (" + sqlc + ") ";
                        query = "delete from " + newschema + ".t_batchdtl where autono in (" + sqlc1 + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                        i = 0; maxR = 0;
                        maxR = tbldel.Rows.Count - 1;
                        while (i <= maxR)
                        {
                            string autono1 = tbldel.Rows[i]["autono"].retStr();
                            while (tbldel.Rows[i]["autono"].retStr() == autono1)
                            {
                                errorAutono = tbldel.Rows[i]["autono"].retStr();
                                if (errorAutono == "2021SNFPKOLKSSFOST0000000600")
                                {
                                    var aa = "";
                                }
                                query = "delete from " + newschema + ".t_batchdtl where  autono='" + tbldel.Rows[i]["autono"].ToString() + "' and txnslno= " + tbldel.Rows[i]["slno"].ToString() + "  ";
                                OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                                query = "delete from " + newschema + ".t_bale where  autono='" + tbldel.Rows[i]["autono"].ToString() + "' and slno= " + tbldel.Rows[i]["slno"].ToString() + "  ";
                                OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                                query = "delete from " + newschema + ".t_txndtl where autono='" + tbldel.Rows[i]["autono"].ToString() + "' and slno= " + tbldel.Rows[i]["slno"].ToString() + "  ";
                                OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                                i += 1;
                                if (i > maxR) break;
                            }
                            query = "delete from " + newschema + ".t_bale_hdr where  autono='" + tbldel.Rows[i - 1]["autono"].ToString()
                                + "' and autono not in (select a.blautono from " + newschema + ".t_bale a," + newschema + ".t_cntrl_hdr b where a.autono = b.autono and b.yr_cd = '" + CommVar.YearCode(UNQSNO) + "')";

                            OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                            //query = "delete from " + newschema + ".T_TXNOTH where autono='" + tbldel.Rows[i - 1]["autono"].ToString() + "' ";
                            //OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                            //query = "delete from " + newschema + ".T_TXNTRANS where autono='" + tbldel.Rows[i - 1]["autono"].ToString() + "' ";
                            //OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();

                            if (i > maxR) break;
                        }

                        if (dberrmsg != "") goto dbnotsave;

                        string defdoccd = "", docno = ""; int mxdocno = 0;

                        var vTXN = (from p in DBOLD.T_TXN
                                    join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                    join r in DBOLD.M_DOCTYPE on q.DOCCD equals (r.DOCCD)
                                    where (q.COMPCD == COM && r.DOCTYPE != "SBILD")
                                    select p).ToList();

                        var vTXNOTH = (from p in DBOLD.T_TXNOTH
                                       join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       join r in DBOLD.M_DOCTYPE on q.DOCCD equals (r.DOCCD)
                                       where (q.COMPCD == COM && r.DOCTYPE != "SBILD")
                                       select p).ToList();

                        var vTXNTRANS = (from p in DBOLD.T_TXNTRANS
                                         join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                         join r in DBOLD.M_DOCTYPE on q.DOCCD equals (r.DOCCD)
                                         where (q.COMPCD == COM && r.DOCTYPE != "SBILD")
                                         select p).ToList();

                        var vTBALEHDR = (from p in DBOLD.T_BALE_HDR
                                         join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                         join r in DBOLD.M_DOCTYPE on q.DOCCD equals (r.DOCCD)
                                         where (q.COMPCD == COM && r.DOCTYPE != "SBILD")
                                         select p).ToList();

                        var vTBALE = (from p in DBOLD.T_BALE
                                      join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                      join r in DBOLD.M_DOCTYPE on q.DOCCD equals (r.DOCCD)
                                      where (q.COMPCD == COM && r.DOCTYPE != "SBILD")
                                      select p).ToList();

                        var vTBATCHDTL = (from p in DBOLD.T_BATCHDTL
                                          join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                          join r in DBOLD.M_DOCTYPE on q.DOCCD equals (r.DOCCD)
                                          where (q.COMPCD == COM && r.DOCTYPE != "SBILD")
                                          select p).ToList();

                        var vTTXNDTL = (from p in DBOLD.T_TXNDTL
                                        join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                        join r in DBOLD.M_DOCTYPE on q.DOCCD equals (r.DOCCD)
                                        where (q.COMPCD == COM && r.DOCTYPE != "SBILD")
                                        select p).ToList();

                        //var vTBATCHMSTPRICE = (from p in DBOLD.T_BATCHMST_PRICE
                        //                       select p).ToList();

                        i = 0; maxR = 0;
                        maxR = tbl.Rows.Count - 1;
                        string orgdocdt = "", blautono = "", godown = "";
                        while (i <= maxR)
                        {
                            chkval = tbl.Rows[i]["blautono"].ToString();
                            blautono = chkval;
                            godown = tbl.Rows[i]["gocd"].ToString();
                            errorAutono = tbl.Rows[i]["blautono"].retStr();

                            if (errorAutono == "2021SNFPKOLKSSSPBL2112000182")
                            {
                                var aa = "";
                            }
                            T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                            T_CNTRL_HDR TCHOLD = new T_CNTRL_HDR();

                            TCHOLD = DBOLD.T_CNTRL_HDR.Where(a => a.AUTONO == chkval).FirstOrDefault();

                            bool recoexist = false;
                            sql = "select autono from " + newschema + ".t_cntrl_hdr where autono='" + chkval + "'";
                            OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                            if (recoexist == false)
                            {
                                dbsql = MasterHelpFa.RetModeltoSql(TCHOLD);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }

                            T_TXN TTXN = new T_TXN();
                            bool insTxn = (blautono == "" ? true : false);

                            recoexist = false;
                            sql = "select autono from " + newschema + ".t_txn where autono='" + chkval + "'";
                            OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                            if (recoexist == false)
                            {
                                var TXNOLD = vTXN.Where(x => x.AUTONO == blautono).ToList();
                                if (TXNOLD.Count > 0)
                                {
                                    TXNOLD[0].AUTONO = TCHOLD.AUTONO;
                                    TXNOLD[0].GOCD = tbl.Rows[i]["gocd"].ToString();
                                    TTXN = TXNOLD[0];
                                    dbsql = MasterHelpFa.RetModeltoSql(TTXN);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            T_TXNOTH TTXNOTH = new T_TXNOTH();
                            recoexist = false;
                            sql = "select distinct autono from " + newschema + ".T_TXNOTH where autono = '" + chkval + "' ";
                            OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                            if (recoexist == false)
                            {
                                var TXNOTHOLD = vTXNOTH.Where(x => x.AUTONO == blautono).ToList();
                                if (recoexist == false && TXNOTHOLD.Count > 0) ;
                                {
                                    TXNOTHOLD[0].AUTONO = TCHOLD.AUTONO;
                                    TTXNOTH = TXNOTHOLD[0];
                                    dbsql = MasterHelpFa.RetModeltoSql(TTXNOTH);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            T_TXNTRANS TTXNTRANS = new T_TXNTRANS();
                            sql = "select distinct autono from " + newschema + ".T_TXNTRANS where autono = '" + chkval + "' ";
                            OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                            if (recoexist == false)
                            {
                                var TXNTRANSOLD = vTXNTRANS.Where(x => x.AUTONO == blautono).ToList();
                                if (TXNTRANSOLD.Count > 0)
                                {
                                    TXNTRANSOLD[0].AUTONO = TCHOLD.AUTONO;
                                    TTXNTRANS = TXNTRANSOLD[0];

                                    dbsql = MasterHelpFa.RetModeltoSql(TTXNTRANS);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            T_BALE_HDR TBALEHDR = new T_BALE_HDR();

                            recoexist = false;
                            sql = "select distinct autono from " + newschema + ".t_bale_hdr where autono = '" + chkval + "' ";
                            OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                            if (recoexist == false)
                            {
                                var TBALEHDROLD = vTBALEHDR.Where(x => x.AUTONO == blautono).ToList();
                                if (TBALEHDROLD.Count > 0)
                                {

                                    TBALEHDROLD[0].AUTONO = TCHOLD.AUTONO;
                                    TBALEHDR = TBALEHDROLD[0];
                                    dbsql = MasterHelpFa.RetModeltoSql(TBALEHDR);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }

                            int sl = 1;
                            while (tbl.Rows[i]["blautono"].ToString() == blautono)
                            {
                                chkval1 = tbl.Rows[i]["blautono"].ToString();
                                string baleno = tbl.Rows[i]["baleno"].retStr();
                                string baleyr = tbl.Rows[i]["baleyr"].retStr();
                                double blslno = 0;
                                if (tbl.Rows[i]["blslno"].retDbl() != 0) blslno = Convert.ToDouble(tbl.Rows[i]["blslno"]);

                                double iqty = 0; int inos = 0; Int32 chki = i;
                                errorAutono = tbl.Rows[i]["blautono"].retStr();
                                if (errorAutono == "2021BNBHKOLKSSSPBL0000000612")
                                {
                                    var aa = "";
                                }

                                recoexist = false;
                                sql = "select distinct autono, slno from " + newschema + ".t_txndtl where autono = '" + blautono + "' and slno = " + blslno + " and baleyr='" + baleyr + "' and baleno='" + baleno + "' ";
                                OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                if (recoexist == false)
                                {
                                    T_TXNDTL TTXNDTL = new T_TXNDTL();
                                    var TTXNDTLOLD = vTTXNDTL.Where(x => x.AUTONO == blautono && x.SLNO == blslno && x.BALEYR == baleyr && x.BALENO == baleno).ToList();

                                    if (TTXNDTLOLD.Count > 0)
                                    {
                                        for (int rc = 0; rc <= TTXNDTLOLD.Count - 1; rc++)
                                        {
                                            TTXNDTLOLD[rc].AUTONO = blautono;
                                            TTXNDTLOLD[rc].SLNO = blslno.retShort();
                                            //TTXNDTLOLD[rc].STKDRCR = (iqty < .0001 ? "C" : "D");
                                            TTXNDTLOLD[rc].GOCD = tbl.Rows[i]["gocd"].retStr();

                                            dbsql = MasterHelpFa.RetModeltoSql(TTXNDTLOLD[rc]);
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                recoexist = false;
                                sql = "select distinct autono, slno from " + newschema + ".t_bale where autono = '" + blautono + "' and slno = " + blslno + " and baleyr='" + baleyr + "' and baleno='" + baleno + "' ";
                                OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                if (recoexist == false)
                                {
                                    T_BALE TBALE = new T_BALE();
                                    var TBALEOLD = vTBALE.Where(x => x.AUTONO == blautono && x.SLNO == blslno && x.BALEYR == baleyr && x.BALENO == baleno).ToList();

                                    if (TBALEOLD.Count > 0)
                                    {
                                        for (int rc = 0; rc <= TBALEOLD.Count - 1; rc++)
                                        {
                                            TBALEOLD[rc].AUTONO = blautono;
                                            TBALEOLD[rc].SLNO = blslno.retShort();
                                            //TBALEOLD[rc].DRCR = (iqty < .0001 ? "C" : "D");
                                            TBALEOLD[rc].GOCD = tbl.Rows[i]["gocd"].retStr();

                                            dbsql = MasterHelpFa.RetModeltoSql(TBALEOLD[rc]);
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                recoexist = false;
                                sql = "select distinct autono, slno from " + newschema + ".t_batchdtl where autono = '" + blautono + "' and txnslno = " + blslno + " and baleyr='" + baleyr + "' and baleno='" + baleno + "' ";
                                OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                if (recoexist == false)
                                {
                                    T_BATCHDTL TBATCHDTL = new T_BATCHDTL();
                                    var TBATCHDTLOLD = vTBATCHDTL.Where(x => x.AUTONO == blautono && x.TXNSLNO == blslno && x.BALEYR == baleyr && x.BALENO == baleno).ToList();

                                    if (TBATCHDTLOLD.Count > 0)
                                    {
                                        for (int rc = 0; rc <= TBATCHDTLOLD.Count - 1; rc++)
                                        {
                                            TBATCHDTLOLD[rc].AUTONO = blautono;
                                            TBATCHDTLOLD[rc].MILLNM = "BALESTOCK";
                                            //TBATCHDTLOLD[rc].STKDRCR = (iqty < .0001 ? "C" : "D");
                                            TBATCHDTLOLD[rc].GOCD = tbl.Rows[i]["gocd"].retStr();

                                            dbsql = MasterHelpFa.RetModeltoSql(TBATCHDTLOLD[rc]);
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                while (tbl.Rows[i]["blautono"].ToString() == blautono && tbl.Rows[i]["blslno"].retDbl() == blslno && tbl.Rows[i]["baleyr"].ToString() == baleyr && tbl.Rows[i]["baleno"].ToString() == baleno)
                                {
                                    i++;
                                    if (i > maxR) break;
                                }
                                if (i > maxR) break;
                            }
                            if (i > maxR) break;
                        }
                    }
                    #endregion

                    #region Cumulative Sales Transfer
                    sql = "select a.itgrpcd, a.doccd ";
                    sql += "from " + oldschema + ".m_groupdoccd a, " + oldschema + ".m_doctype b ";
                    sql += "where a.doccd = b.doccd and a.itgrpcd = '" + itgrpcd + "' and b.doctype = 'CUMSL'";
                    tbltmp = masterHelp.SQLquery(sql);

                    if (VE.Checkbox3 == true && tbltmp.Rows.Count == 1)
                    {
                        scm1 = oldschema;
                        string cfdt = CommVar.FinStartDate(UNQSNO);
                        cfdt = "01/01/" + cfdt.Substring(6, 4);
                        query = "";
                        query = query + "select a.itcd, f.itnm, f.packsize, f.nosinbag, i.rateqntybag, f.uomcd, f.prodcd, f.prodgrpcd, h.prodgrpnm, ";
                        query = query + "nvl(e.itmprccd,b.itmprccd) itmprccd, nvl(e.slcd,b.slcd) pslcd, g.prcdesc, ";
                        query = query + "sum(decode(b.doctag,'SR',nvl(d.nos,a.nos)*-1,nvl(d.nos,a.nos))) nos, sum(decode(b.doctag,'SR',nvl(d.qnty,a.qnty)*-1,nvl(d.qnty,a.qnty))) qnty ";
                        query = query + "from " + scm1 + ".t_txndtl a, " + scm1 + ".t_txn b, " + scm1 + ".t_cntrl_hdr c, " + scm1 + ".t_batchdtl d, " + scm1 + ".t_batchmst e, ";
                        query = query + "" + scm1 + ".m_sitem f, " + scm1 + ".m_itemplist g, " + scm1 + ".m_prodgrp h, " + scm1 + ".m_group i ";
                        query = query + "where a.autono=b.autono(+) and a.autono=c.autono(+) and b.doctag in ('SB','CS','SR') and  ";
                        query = query + "a.autono=d.autono(+) and a.slno=d.slno(+) and d.batchautono=e.batchautono(+) and ";
                        query = query + "a.itcd=f.itcd and nvl(e.itmprccd,b.itmprccd) = g.itmprccd and f.prodgrpcd=h.prodgrpcd(+) and f.itgrpcd=i.itgrpcd(+) and ";
                        query = query + "nvl(c.cancel,'N') = 'N' and b.itgrpcd='" + itgrpcd + "' and c.compcd='" + COM + "' and c.loccd='" + LOC + "' and ";
                        query = query + "c.docdt >= to_date('" + cfdt + "','dd/mm/yyyy')  ";
                        //query = query + "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
                        query = query + "group by a.itcd, f.itnm, f.packsize, f.nosinbag, i.rateqntybag, f.uomcd, f.prodcd, f.prodgrpcd, h.prodgrpnm, ";
                        query = query + "nvl(e.itmprccd,b.itmprccd), nvl(e.slcd,b.slcd), g.prcdesc ";
                        query = query + "order by pslcd,itcd,itmprccd ";
                        //
                        tbl = masterHelp.SQLquery(query);

                        i = 0; maxR = 0;
                        maxR = tbl.Rows.Count - 1;

                        sqlc = "select a.autono from " + newschema + ".t_txn a, " + newschema + ".t_cntrl_hdr b, " + newschema + ".m_doctype c ";
                        sqlc += "where a.itgrpcd='" + itgrpcd + "' and b.doccd=c.doccd(+) and ";
                        sqlc += "c.doctype in ('CUMSL') and ";
                        sqlc += "a.autono=b.autono(+) and b.yr_cd <= '" + CommVar.YearCode(UNQSNO) + "' and b.compcd='" + COM + "' and b.loccd='" + LOC + "' ";
                        DataTable tbldel = masterHelp.SQLquery(sqlc);

                        query = "delete from " + newschema + ".t_txndtl where autono in (" + sqlc + ") ";
                        dberrmsg += masterHelp.SQLNonQuery(query);
                        query = "delete from " + newschema + ".t_txn where autono in (" + sqlc + ") ";
                        dberrmsg += masterHelp.SQLNonQuery(query);

                        for (Int32 q = 0; q <= tbldel.Rows.Count - 1; q++)
                        {
                            query = "delete from " + newschema + ".t_cntrl_hdr where autono='" + tbldel.Rows[q]["autono"].ToString() + "' ";
                            dberrmsg += masterHelp.SQLNonQuery(query);
                        }
                        if (dberrmsg != "") goto dbnotsave;

                        //DB.Database.ExecuteSqlCommand("lock table " + newschema + ".T_CNTRL_HDR in  row share mode");
                        i = 0; maxR = 0;
                        maxR = tbl.Rows.Count - 1;

                        string cumdoccd = tbltmp.Rows[0]["doccd"].retStr();
                        string cumDocdt = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).AddDays(-1).retDateStr();
                        string cumDocno = Cn.MaxDocNumber(cumdoccd, cumDocdt);

                        while (i <= maxR)
                        {
                            if (i > 0)
                            {
                                int dn = Convert.ToInt32(cumDocno);
                                dn += 1;
                                cumDocno = dn.ToString();
                                cumDocno = cumDocno.PadLeft(6, '0');
                            }
                            string cumdocpattrn = Cn.DocPattern(Convert.ToInt32(cumDocno), cumdoccd, CommVar.CurSchema(UNQSNO), CommVar.FinSchema(UNQSNO), cumDocdt);
                            string cum = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), cumDocno, cumdoccd, cumDocdt);
                            string cumAutoNo = cum.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            string cumMonth = cum.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                            string itcd = tbl.Rows[i]["itcd"].ToString(), itmprccd = tbl.Rows[i]["itmprccd"].ToString();
                            string pslcd = tbl.Rows[i]["pslcd"].ToString();
                            //DOCTYPE CUMSL
                            T_CNTRL_HDR cumtcH = new T_CNTRL_HDR();
                            cumtcH = Cn.T_CONTROL_HDR(cumdoccd, Convert.ToDateTime(cumDocdt), cumDocno, cumAutoNo, cumMonth, cumdocpattrn, "A", CommVar.CurSchema(UNQSNO), null, null, 0, "Y");

                            dbsql = MasterHelpFa.RetModeltoSql(cumtcH);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            T_TXN TTXN = new T_TXN();
                            TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                            TTXN.AUTONO = cumAutoNo;
                            //TTXN.ITGRPCD = itgrpcd;
                            //TTXN.ITMPRCCD = itmprccd;
                            TTXN.DOCTAG = "CS";
                            TTXN.DOCNO = cumDocno;
                            TTXN.DOCCD = cumdoccd;
                            TTXN.SLCD = pslcd;
                            TTXN.DOCDT = Convert.ToDateTime(cumDocdt);

                            dbsql = MasterHelpFa.RetModeltoSql(TTXN);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            int sln = 0;
                            while (tbl.Rows[i]["itcd"].ToString() == itcd && tbl.Rows[i]["itmprccd"].ToString() == itmprccd && tbl.Rows[i]["pslcd"].ToString() == pslcd)
                            {
                                //T_TXNDTL
                                sln++;
                                T_TXNDTL TTXNDTL = new T_TXNDTL();
                                TTXNDTL.CLCD = TTXN.CLCD;
                                TTXNDTL.EMD_NO = TTXN.EMD_NO;
                                TTXNDTL.AUTONO = TTXN.AUTONO;
                                TTXNDTL.SLNO = Convert.ToInt16(sln);
                                //TTXNDTL.DOCCD = TTXN.DOCCD;
                                //TTXNDTL.DOCNO = TTXN.DOCNO;
                                //TTXNDTL.DOCDT = TTXN.DOCDT;
                                TTXNDTL.ITCD = tbl.Rows[i]["itcd"].ToString();
                                TTXNDTL.NOS = Convert.ToInt32(tbl.Rows[i]["nos"]);
                                TTXNDTL.QNTY = Convert.ToDouble(tbl.Rows[i]["qnty"]);

                                TTXNDTL.STKDRCR = "N";
                                TTXNDTL.RATE = 0;
                                //TTXNDTL.CURR_AMT = 0;
                                TTXNDTL.AMT = 0;
                                //TTXNDTL.STDDISCRATE = 0;

                                dbsql = MasterHelpFa.RetModeltoSql(TTXNDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                i++;
                                if (i > maxR) break;
                            }
                        }
                    }
                    #endregion

                    #region Pendorder Order Transfer
                    if (VE.Checkbox4 == true)
                    {
                        tbl = Salesfunc.GetPendOrder("", lastdayofprvyear, "", "", "", "SB", "'" + itgrpcd + "'", "", true, "", oldschema);
                        DataView dv = tbl.DefaultView;
                        dv.Sort = "autono,slno";
                        tbl = dv.ToTable();
                        i = 0; maxR = 0;
                        maxR = tbl.Rows.Count - 1;

                        sqlc = "select a.autono from " + newschema + ".t_sord a, " + newschema + ".t_cntrl_hdr b, " + newschema + ".m_doctype c ";
                        sqlc += "where a.itgrpcd='" + itgrpcd + "' and b.doccd=c.doccd(+) and a.autono=b.autono(+) and a.ordtag in ('SB') and ";
                        sqlc += "(b.yr_cd < '" + CommVar.YearCode(UNQSNO) + "' ) and "; // or c.doctype in ('OPNG')) and ";
                        sqlc += "b.compcd='" + COM + "' "; // and b.loccd='" + LOC + "' ";
                        DataTable tbldel = masterHelp.SQLquery(sqlc);

                        query = "delete from " + newschema + ".t_sorddtl where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_sorddelvdt where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".t_sord where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        for (Int32 q = 0; q <= tbldel.Rows.Count - 1; q++)
                        {
                            query = "delete from " + newschema + ".t_cntrl_hdr where autono='" + tbldel.Rows[q]["autono"].ToString() + "' ";
                            OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        }
                        if (dberrmsg != "") goto dbnotsave;

                        //string defdoccd = "", docno = ""; int mxdocno = 0;

                        //sql = "select a.itgrpcd, a.doccd ";
                        //sql += "from " + oldschema + ".m_groupdoccd a, " + oldschema + ".m_doctype b ";
                        //sql += "where a.doccd = b.doccd and a.itgrpcd = '" + itgrpcd + "' and b.doctype = 'SBILL'";
                        //tbltmp = masterHelp.SQLquery(sql);
                        //if (tbltmp.Rows.Count > 0)
                        //{
                        //    defdoccd = tbltmp.Rows[0]["doccd"].retStr();
                        //    docno = Cn.MaxDocNumber(defdoccd, lastdayofprvyear);
                        //    mxdocno = Convert.ToInt16(docno) - 1;
                        //}

                        var vTXN = (from p in DBOLD.T_TXN
                                    join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                    where (q.COMPCD == COM)
                                    select p).ToList();

                        var vTXNDTL = (from p in DBOLD.T_TXNDTL
                                       join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       where (q.COMPCD == COM)
                                       select p).ToList();

                        var vTSORD = (from p in DBOLD.T_SORD
                                      join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                      where (q.COMPCD == COM)
                                      select p).ToList();

                        var vTSORDDTL = (from p in DBOLD.T_SORDDTL
                                         join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                         where (q.COMPCD == COM)
                                         select p).ToList();


                        sql = "select a.autono, a.slno, a.ordautoslno, b.docno, b.docdt, ";
                        sql += "sum(case c.doctag when 'SB' then nvl(a.blqnty,a.qnty) when 'PB' then nvl(a.blqnty,a.qnty) else nvl(a.blqnty,a.qnty)*-1 end) qnty ";
                        sql += "from " + oldschema + ".t_txndtl a, " + oldschema + ".t_cntrl_hdr b, " + oldschema + ".t_txn c," + oldschema + ".m_doctype d ";
                        sql += "where a.autono=b.autono and a.autono=c.autono and ";
                        sql += "b.docdt <= to_date('" + lastdayofprvyear + "','dd/mm/yyyy') AND B.doccd=d.doccd and d.doctype not in('PROF') and ";
                        sql += "b.compcd = '" + CommVar.Compcd(UNQSNO) + "' and nvl(b.cancel,'N')='N' ";
                        sql += "group by a.autono, a.slno, a.ordautoslno, b.docno, b.docdt ";
                        sql += "order by docdt, docno ";
                        DataTable tbltxn = masterHelp.SQLquery(sql);

                        i = 0; maxR = 0;
                        maxR = tbl.Rows.Count - 1;
                        string orgdocdt = "", orgautono = "";
                        bool insTxn = true;
                        while (i <= maxR)
                        {
                            orgautono = tbl.Rows[i]["autono"].retStr();
                            orgdocdt = tbl.Rows[i]["docdt"].retDateStr() == "" ? lastdayofprvyear : tbl.Rows[i]["docdt"].retDateStr();

                            T_CNTRL_HDR TCHOLD = new T_CNTRL_HDR();

                            TCHOLD = DBOLD.T_CNTRL_HDR.Find(orgautono);
                            dbsql = MasterHelpFa.RetModeltoSql(TCHOLD);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            var TSORD = vTSORD.Where(x => x.AUTONO == orgautono).ToList();
                            insTxn = true;
                            if (TSORD.Count != 0)
                            {
                                dbsql = MasterHelpFa.RetModeltoSql(TSORD[0]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                            while (tbl.Rows[i]["autono"].retStr() == orgautono)
                            {
                                int orgslno = tbl.Rows[i]["slno"].retInt();
                                var TSORDDTl = vTSORDDTL.Where(x => x.AUTONO == orgautono && x.SLNO == orgslno).ToList();
                                if (TSORDDTl.Count != 0)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(TSORDDTl[0]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }

                                #region Checking with transactios
                                var txn = tbltxn.Select("ordautoslno='" + tbl.Rows[i]["autoslno"] + "'");
                                if (txn.Count() > 0)
                                {
                                    int t = 0, maxT = txn.Count() - 1;
                                    while (t <= maxT)
                                    {

                                        bool recoexist = false;
                                        string txnautono = txn[t]["autono"].retStr();
                                        int txnslno = txn[t]["slno"].retInt();

                                        sql = "select autono from " + newschema + ".t_cntrl_hdr where autono='" + txnautono + "'";
                                        OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                        if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                        if (recoexist == false)
                                        {
                                            T_CNTRL_HDR TTCH = new T_CNTRL_HDR();
                                            TTCH = DBOLD.T_CNTRL_HDR.Find(txnautono);
                                            dbsql = MasterHelpFa.RetModeltoSql(TTCH);
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                            var TTXN = vTXN.Where(x => x.AUTONO == txnautono).ToList();
                                            insTxn = true;
                                            if (TTXN.Count != 0)
                                            {
                                                dbsql = MasterHelpFa.RetModeltoSql(TTXN[0]);
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                            }
                                        }

                                        sql = "select autono from " + newschema + ".t_txndtl where autono='" + txnautono + "' and slno=" + txnslno;
                                        OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                        if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                        if (recoexist == false)
                                        {
                                            var TTXNDTL = vTXNDTL.Where(x => x.AUTONO == txnautono && x.SLNO == txnslno).ToList();
                                            if (TTXNDTL.Count != 0)
                                            {
                                                TTXNDTL[0].STKDRCR = "N";
                                                dbsql = MasterHelpFa.RetModeltoSql(TTXNDTL[0]);
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                            }
                                        }
                                        t++;
                                    }
                                }
                                #endregion

                                i++;
                                if (i > maxR) break;
                            }
                        }
                    }

                    #endregion

                    #region Pending Job from Prev Yr
                    if (VE.Checkbox5 == true)
                    {

                        DataTable PendingJobDT = new DataTable("PendingJob");
                        PendingJobDT.Columns.Add("autono", typeof(string), "");
                        PendingJobDT.Columns.Add("jobautoslno", typeof(string), "");
                        PendingJobDT.Columns.Add("docdt", typeof(string), "");
                        PendingJobDT.Columns.Add("docno", typeof(string), "");
                        PendingJobDT.Columns.Add("PBLNO", typeof(string), "");
                        PendingJobDT.Columns.Add("PBLdt", typeof(string), "");
                        PendingJobDT.Columns.Add("slcd", typeof(string), "");
                        PendingJobDT.Columns.Add("slnm", typeof(string), "");
                        PendingJobDT.Columns.Add("itcd", typeof(string), "");
                        PendingJobDT.Columns.Add("itrem", typeof(string), "");
                        //IR.Columns.Add("itnm", typeof(string), "");
                        PendingJobDT.Columns.Add("uomcd", typeof(string), "");
                        PendingJobDT.Columns.Add("issqnty", typeof(double), "");
                        PendingJobDT.Columns.Add("issrate", typeof(double), "");
                        PendingJobDT.Columns.Add("issamt", typeof(double), "");
                        PendingJobDT.Columns.Add("balqnty", typeof(double), "");

                        scm1 = CommVar.CurSchema(UNQSNO);
                        string issdoctype = "'OJISS','JBREC','IJISS'", recdoctype = "'OJREC','OJRET','JBCHL','JBRET','IJREC','IJRET'";
                        // if (IssueDoctype == "OJISS")
                        // {
                        //     issdoctype = "'OJISS'"; recdoctype = "'OJREC','OJRET'";

                        // }
                        // if (IssueDoctype == "JBREC")
                        // {
                        //     issdoctype = "'JBREC'"; recdoctype = "'JBCHL','JBRET'";
                        // }
                        // if (IssueDoctype == "IJISS")
                        // {
                        //     issdoctype = "'IJISS'"; recdoctype = "'IJREC','IJRET'";
                        // }
                        sql = "";
                        sql += "select a.autono,a.jobautoslno, a.slno,a.doccd, a.itcd, a.qnty, a.scrapeqnty, h.lorryno, ";
                        sql += "a.rate, a.basamt amt, b.usr_id, c.slcd,c.PBLNO,c.PBLdt, b.docno, b.docdt ";
                        //sql += "nvl(d.fullname,d.slnm) slnm ";
                        sql += "from (select a.autono,a.slno,a.jobautoslno,a.doccd, a.itcd, a.itrem, a.qnty, 0 scrapeqnty, a.rate, a.basamt, a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt  ";
                        sql += " from " + oldschema + ".t_txndtl a where  a.slno>1000 ";
                        sql += "union all ";
                        sql += "select a.autono, a.slno,a.jobautoslno,a.doccd, a.itcd, '' itrem, a.qnty, nvl(a.scrapeqnty,0) scrapeqnty, a.rate, a.amt basamt, a.igstper, a.igstamt, a.cgstper, a.sgstamt,  ";
                        sql += "a.cgstamt, a.sgstper from " + oldschema + ".t_txnproddtl a  ";
                        sql += " ) a,  ";
                        sql += oldschema + ".t_cntrl_hdr b, " + oldschema + ".t_txn c,  ";
                        sql += oldschema + ".m_doctype g, " + oldschema + ".t_txntrans h ";
                        sql += "where a.autono=b.autono and a.autono=c.autono  and a.autono=h.autono(+) and ";
                        sql += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' and b.doccd=g.doccd(+) ";
                        sql += " and g.doctype in (" + issdoctype + ")  and nvl(b.cancel,'N') = 'N'  ";
                        sql += " order by docdt,jobautoslno,itcd ";
                        DataTable tblISS = masterHelp.SQLquery(sql);

                        sql = "";
                        sql += "select a.autono,a.jobautoslno, a.slno rec_slno, a.doccd, a.itcd rec_itcd, a.itrem, a.qnty rec_qnty, a.scrapeqnty, h.lorryno rec_lorryno, ";
                        sql += "a.rate rec_rate, a.basamt rec_amt, b.usr_id, c.slcd,c.PBLNO rec_pblno, b.docno rec_docno, b.docdt rec_docdt ";
                        sql += "from (select a.autono,a.jobautoslno,a.slno,a.doccd, a.itcd, a.itrem, a.qnty, 0 scrapeqnty, a.rate, a.basamt, a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt  ";
                        sql += " from " + oldschema + ".t_txndtl a where a.slno>1000 ";
                        sql += "union all ";
                        sql += "select a.autono,a.jobautoslno, a.slno,a.doccd, a.itcd, '' itrem, a.qnty, nvl(a.scrapeqnty,0) scrapeqnty, a.rate,a.amt basamt, a.igstper, a.igstamt, a.cgstper, a.sgstamt,  ";
                        sql += "a.cgstamt, a.sgstper from " + oldschema + ".t_txnproddtl a  ";
                        sql += " ) a,  ";
                        sql += oldschema + ".t_cntrl_hdr b, " + oldschema + ".t_txn c, ";
                        sql += oldschema + ".m_doctype g, " + oldschema + ".t_txntrans h ";
                        sql += "where a.autono=b.autono and a.autono=c.autono and a.autono=h.autono(+) and ";
                        sql += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' and b.doccd=g.doccd(+) ";
                        sql += " and g.doctype in (" + recdoctype + ")  and nvl(b.cancel,'N') = 'N'  ";
                        sql += "order  by rec_docdt,jobautoslno,itcd  ";
                        DataTable rec_date = masterHelp.SQLquery(sql);

                        Int32 rNo = 0;
                        maxR = tblISS.Rows.Count - 1;
                        while (i <= maxR)
                        {
                            autono = tblISS.Rows[i]["autono"].ToString();
                            if (autono == "2020RATNCHATSSIJI00000000007")
                            {

                            }
                            string itcd = tblISS.Rows[i]["itcd"].ToString();
                            string jobautoslno = tblISS.Rows[i]["jobautoslno"].ToString();
                            while (tblISS.Rows[i]["jobautoslno"].ToString() == jobautoslno && tblISS.Rows[i]["itcd"].ToString() == itcd)
                            {
                                DataRow[] results = rec_date.Select("JOBAUTOSLNO='" + jobautoslno + "'  and  rec_itcd= '" + itcd + "'"); double recqnty = 0;
                                if (results.Length != 0)
                                {
                                    foreach (DataRow dr in results) { recqnty += dr["rec_qnty"].retDbl(); }
                                }
                                if (tblISS.Rows[i]["qnty"].retDbl() - recqnty > 0)
                                {
                                    PendingJobDT.Rows.Add(""); rNo = PendingJobDT.Rows.Count - 1;
                                    PendingJobDT.Rows[rNo]["autono"] = tblISS.Rows[i]["autono"];
                                    PendingJobDT.Rows[rNo]["jobautoslno"] = tblISS.Rows[i]["jobautoslno"];
                                    PendingJobDT.Rows[rNo]["docdt"] = tblISS.Rows[i]["docdt"].retDateStr();
                                    PendingJobDT.Rows[rNo]["docno"] = tblISS.Rows[i]["docno"];
                                    PendingJobDT.Rows[rNo]["PBLNO"] = tblISS.Rows[i]["PBLNO"];
                                    PendingJobDT.Rows[rNo]["PBLdt"] = tblISS.Rows[i]["PBLdt"].retDateStr();
                                    PendingJobDT.Rows[rNo]["slcd"] = tblISS.Rows[i]["slcd"];
                                    PendingJobDT.Rows[rNo]["itcd"] = tblISS.Rows[i]["itcd"];
                                    PendingJobDT.Rows[rNo]["issqnty"] = tblISS.Rows[i]["qnty"].retDbl();
                                    PendingJobDT.Rows[rNo]["issrate"] = tblISS.Rows[i]["rate"].retDbl();
                                    PendingJobDT.Rows[rNo]["issamt"] = tblISS.Rows[i]["amt"].retDbl();
                                    PendingJobDT.Rows[rNo]["balqnty"] = tblISS.Rows[i]["qnty"].retDbl() - recqnty;
                                }
                                i++;
                                if (i > maxR) break;
                            }
                        }
                        sqlc = "select b.autono from " + newschema + ".t_cntrl_hdr b, " + newschema + ".m_doctype c ";
                        sqlc += "where b.doccd=c.doccd and ";
                        sqlc += "b.yr_cd < '" + CommVar.YearCode(UNQSNO) + "'  and c.doctype in (" + issdoctype + ") and ";
                        sqlc += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' ";
                        DataTable tbldel = masterHelp.SQLquery(sqlc);

                        query = "delete from " + newschema + ".T_TXNTRANS where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".T_TXNOTH where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        //query = "delete from " + newschema + ".T_TXNPRODDTL where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".T_JOBMST where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".T_TXNDTL where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        query = "delete from " + newschema + ".T_TXN where autono in (" + sqlc + ") ";
                        OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        for (Int32 q = 0; q <= tbldel.Rows.Count - 1; q++)
                        {
                            query = "delete from " + newschema + ".t_cntrl_hdr where autono='" + tbldel.Rows[q]["autono"].ToString() + "' ";
                            OraCmd.CommandText = query; OraCmd.ExecuteNonQuery();
                        }
                        if (dberrmsg != "") goto dbnotsave;

                        var vTXN = (from p in DBOLD.T_TXN
                                    join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                    where (q.COMPCD == COM)
                                    select p).ToList();

                        var vTXNDTL = (from p in DBOLD.T_TXNDTL
                                       join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       where (q.COMPCD == COM)
                                       select p).ToList();

                        //var vTXNPRODDTL = (from p in DBOLD.T_TXNPRODDTL
                        //                   join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                        //                   where (q.COMPCD == COM)
                        //                   select p).ToList();

                        var vTXNOTH = (from p in DBOLD.T_TXNOTH
                                       join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       where (q.COMPCD == COM)
                                       select p).ToList();

                        var vTXNTRANS = (from p in DBOLD.T_TXNTRANS
                                         join q in DBOLD.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                         where (q.COMPCD == COM)
                                         select p).ToList();

                        string[] selgocd = string.Join(",", (from DataRow dr in PendingJobDT.Rows select dr["jobautoslno"].ToString()).Distinct()).Split(',');
                        string jobautonos = selgocd.retSqlfromStrarray();

                        DataView dtView = new DataView(PendingJobDT);
                        dtView.RowFilter = "jobautoslno in (" + jobautonos + ")";
                        string[] itcol = new string[2];
                        itcol[0] = "autono";
                        itcol[1] = "jobautoslno";
                        DataTable jobautoRefDisttbl = dtView.ToTable(true, itcol);

                        bool recoexist = false;

                        i = 0; maxR = 0;
                        maxR = PendingJobDT.Rows.Count - 1;
                        while (i <= maxR)
                        {
                            string jobautoslno = PendingJobDT.Rows[i]["jobautoslno"].ToString();
                            sql = " ";
                            //sql += " select a.autono,a.slno,a.jobautoslno  from " + oldschema + ".t_txnproddtl a where a.jobautoslno='" + jobautoslno + "' ";
                            sql += " union all";
                            sql += " select a.autono,a.slno,a.jobautoslno  from " + oldschema + ".t_txndtl a where a.jobautoslno='" + jobautoslno + "' ";
                            DataTable JobIssRec = masterHelp.SQLquery(sql);
                            //DataRow[] autodrow = jobautoRefDisttbl.Select("jobautoslno='" + jobautoslno + "'");
                            for (int au = 0; au < JobIssRec.Rows.Count; au++)
                            {
                                autono = JobIssRec.Rows[au]["autono"].retStr();
                                int slno = JobIssRec.Rows[au]["slno"].retInt();
                                sql = "select autono from " + newschema + ".t_cntrl_hdr where autono='" + autono + "'";
                                OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                if (recoexist == false)
                                {
                                    T_CNTRL_HDR TTCH = DBOLD.T_CNTRL_HDR.Find(autono);
                                    dbsql = MasterHelpFa.RetModeltoSql(TTCH);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                    var TTxn = vTXN.Where(m => m.AUTONO == autono).First();
                                    dbsql = MasterHelpFa.RetModeltoSql(TTxn);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }

                                sql = "select autono from " + newschema + ".T_TXNDTL where autono='" + autono + "' and slno=" + slno + "";
                                OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                if (recoexist == false)
                                {
                                    var TXNDTL = vTXNDTL.Where(m => m.AUTONO == autono && m.SLNO == slno).FirstOrDefault();
                                    if (TXNDTL != null)
                                    {
                                        TXNDTL.STKDRCR = "N";
                                        dbsql = MasterHelpFa.RetModeltoSql(TXNDTL);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }

                                sql = "select autono from " + newschema + ".T_TXNPRODDTL where autono='" + autono + "' and slno=" + slno + "";
                                OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                if (recoexist == false)
                                {
                                    //var TXNPRODDTL = vTXNPRODDTL.Where(m => m.AUTONO == autono && m.SLNO == slno).FirstOrDefault();
                                    //if (TXNPRODDTL != null)
                                    //{
                                    //    dbsql = MasterHelpFa.RetModeltoSql(TXNPRODDTL);
                                    //    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    //}
                                }

                                sql = "select autono from " + newschema + ".T_TXNOTH where autono='" + autono + "'";
                                OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                if (recoexist == false)
                                {

                                    var TXNOTH = vTXNOTH.Where(m => m.AUTONO == autono).FirstOrDefault();
                                    if (TXNOTH != null)
                                    {
                                        dbsql = MasterHelpFa.RetModeltoSql(TXNOTH);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }
                                sql = "select autono from " + newschema + ".T_TXNTRANS where autono='" + autono + "'";
                                OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();
                                if (recoexist == false)
                                {
                                    var TXNTRANS = vTXNTRANS.Where(m => m.AUTONO == autono).FirstOrDefault();
                                    if (TXNTRANS != null)
                                    {
                                        dbsql = MasterHelpFa.RetModeltoSql(TXNTRANS);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }
                            }//for JobIssRec.Rows.Count
                            i++;
                            if (i > maxR) break;
                        }//PendingJobDT.Rows.Count
                    }//checkbox5
                    #endregion

                    ModelState.Clear();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    return "Transferred sucessfully";
                    dbnotsave:;
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    return sql + Cn.GCS() + dberrmsg;
                }
                catch (Exception ex)
                {
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    Cn.SaveException(ex, errorAutono);
                    return ex.Message + ex.InnerException + errorAutono;
                }
            }
        }
    }
}
