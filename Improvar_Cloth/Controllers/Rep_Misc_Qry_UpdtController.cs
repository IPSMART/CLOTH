using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Controllers
{
    public class Rep_Misc_Qry_UpdtController : Controller
    {
        // GET: Rep_Misc_Qry_Updt
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false;
        string modulecode = CommVar.ModuleCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_Misc_Qry_Updt()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Misc Update Queries";
                    RepMiscQryUpdt VE = new RepMiscQryUpdt();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string com = CommVar.Compcd(UNQSNO); string gcs = Cn.GCS();
                    List<DropDown_list1> CHNGSTYL = new List<DropDown_list1>();
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change Style", text = "Change Style No in Bale" });
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change Pageno", text = "Change Pageno in bale" });
                    VE.DropDown_list1 = CHNGSTYL;
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
        //public ActionResult GetBaleNoDetails(string val, string BLSLNO = "")
        //{
        //    try
        //    {
               
        //        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //        var tdt = CommVar.CurrDate(UNQSNO);
        //        if (val == null)
        //        {
        //            return PartialView("_Help2", MasterHelp.BaleNo_help(val, tdt, BLSLNO));
        //        }
        //        else
        //        { string sql = "select distinct BALENO,BALEYR,BLSLNO from " + CommVar.CurSchema(UNQSNO) + ".T_BALE where BALENO='" + val + "' ";
        //            if (BLSLNO.retStr() != "") sql += "and BLSLNO='" + BLSLNO + "' ";
        //            DataTable dt = MasterHelp.SQLquery(sql);
                  
        //            if(dt.Rows.Count>1)
        //            { return Content("");
        //            }
        //            else {
        //                var balenoyr = (from DataRow dr in dt.Rows
        //                                select new
        //                                {
        //                                    BALENO = dr["BALENO"].retStr() + dr["BALEYR"].retStr(),
        //                                    BALESLNO = dr["BLSLNO"].retStr()
        //                            }).FirstOrDefault();
        //                val = balenoyr.BALENO;
        //                BLSLNO = balenoyr.BALESLNO;


        //            }
        //            string str = MasterHelp.BaleNo_help(val, tdt, BLSLNO);
        //            return Content(str);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        public ActionResult GetBaleNoDetails(string val,string code)
        {
            try
            {
                var data = code.Split(Convert.ToChar(Cn.GCS()));
                string gocd = data[0].retSqlformat()==""?"": data[0].retSqlformat();
                string itcd = data[1].retSqlformat() == "" ? "" : data[1].retSqlformat();
                var tdt = CommVar.CurrDate(UNQSNO);
                if (val != "")
                {
                    string sql = "select distinct BALENO,BALEYR,BLSLNO from " + CommVar.CurSchema(UNQSNO) + ".T_BALE where BALENO='" + val + "' ";
                    //if (BLSLNO.retStr() != "") sql += "and BLSLNO='" + BLSLNO + "' ";
                    DataTable dt = MasterHelp.SQLquery(sql);
                    if (dt.Rows.Count > 0)
                    {
                        var balenoyr = (from DataRow dr in dt.Rows
                                        select new
                                        {
                                            BALENO = dr["BALENO"].retStr() + dr["BALEYR"].retStr(),
                                            BALESLNO = dr["BLSLNO"].retStr()
                                        }).FirstOrDefault();
                        val = balenoyr.BALENO;
                }


                    // BLSLNO = balenoyr.BALESLNO;
                }
                var str = MasterHelp.BaleNo_help(val,tdt, gocd,itcd);
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
                string DOCDT = CommVar.CurrDate(UNQSNO); /*data[2].retStr()*/
                string TAXGRPCD = data[3].retStr();
                string GOCD = DOCDT.retDateStr() == "" ? "" : data[4].retStr().retSqlformat();   /*data[2].retStr() == "" ? "" : data[4].retStr().retSqlformat();*/
                string PRCCD = data[5].retStr();
                if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = data[6].retStr(); }
                string BARNO = data[8].retStr() == "" || val.retStr() == "" ? "" : data[8].retStr().retSqlformat();
              
                string str = MasterHelp.T_TXN_BARNO_help(barnoOrStyle, VE.MENU_PARA, DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD, "", false, PARTCD, BARNO);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1) return Content(str);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult Save(RepMiscQryUpdt VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string dbsql = "", sql = "", query = "";
            Int16 emdno = 1;
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    var schnm = CommVar.CurSchema(UNQSNO);
                    var CLCD = CommVar.ClientCode(UNQSNO);
                    if (VE.TEXTBOX1 == "Change Style")
                    {
                      
                        sql = "update " + schnm + ". T_TXNDTL set  ITCD= '" + VE.ITCD2 + "' "
                        + " where AUTONO='" + VE.BLAUTONO1 + "' and  ITCD= '" + VE.ITCD1 + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                 
                    }
                    else
                    {
                        sql = "update " + schnm + ". T_TXNDTL set PAGENO='" + VE.NEWPAGENO + "',PAGESLNO='" + VE.NEWPAGESLNO + "' "
                       + " where AUTONO='" + VE.BLAUTONO2 + "' and PAGENO='" + VE.OLDPAGENO.retStr() + "' and PAGESLNO='" + VE.OLDPAGESLNO.retStr() + "'   ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    }
                    ModelState.Clear();
                    transaction.Commit();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    ContentFlg = "1";
                    return Content(ContentFlg);
                }
                catch (Exception ex)
                {
                    DB.SaveChanges();
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
    }
}