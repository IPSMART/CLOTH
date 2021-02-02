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
        public ActionResult GetBaleNoDetails(string val,string BALEAUTONO="")
        {
            try
            {
                var tdt = CommVar.CurrDate(UNQSNO);
                if (val == null)
                {
                    return PartialView("_Help2", MasterHelp.BaleNo_help(val,tdt,BALEAUTONO));
                }
                else
                {
                    string str = MasterHelp.BaleNo_help(val, tdt, BALEAUTONO);
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
                   if(VE.TEXTBOX1== "Change Style")
                    {
                        //MasterHelp.SQLNonQuery("update " + schnm + ". T_TXNDTL set ITCD= '" + VE.NEWSTYLENO + "', BALEYR= '" + VE.BALEYR1 + "', CLCD='" + CLCD + "' "
                        //    + " where BALENO='" + VE.BALENO1 + "' ");

                        sql = "update " + schnm + ". T_TXNDTL set ITCD= '" + VE.NEWSTYLENO + "', BALEYR= '" + VE.BALEYR1 + "', CLCD='" + CLCD + "' "
                            + " where BALENO='" + VE.BALENO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_BATCHDTL set  BALEYR= '" + VE.BALEYR1 + "', CLCD='" + CLCD + "' "
                               + " where BALENO='" + VE.BALENO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                        //MasterHelp.SQLNonQuery("update " + schnm + ". T_BATCHDTL set  BALEYR= '" + VE.BALEYR1 + "', CLCD='" + CLCD + "' "
                        //       + " where BALENO='" + VE.BALENO1 + "' ");


                        sql = "update " + schnm + ". T_BILTY set  BALEYR= '" + VE.BALEYR1 + "', BLAUTONO= '" + VE.BLAUTONO1 + "',LRDT= '" + VE.LRDT1 + "',LRNO= '" + VE.LRNO1 + "', CLCD='" + CLCD + "' "
                             + " where BALENO='" + VE.BALENO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                        //MasterHelp.SQLNonQuery("update " + schnm + ". T_BILTY set  BALEYR= '" + VE.BALEYR1 + "', BLAUTONO= '" + VE.BLAUTONO1 + "',LRDT= '" + VE.LRDT1 + "',LRNO= '" + VE.LRNO1 + "', CLCD='" + CLCD + "' "
                        //      + " where BALENO='" + VE.BALENO1 + "' ");
                        sql = "update " + schnm + ". T_BALE set  BALEYR= '" + VE.BALEYR1 + "', BLAUTONO= '" + VE.BLAUTONO1 + "',LRDT= '" + VE.LRDT1 + "',LRNO= '" + VE.LRNO1 + "', CLCD='" + CLCD + "' "
                           + " where BALENO='" + VE.BALENO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                        //MasterHelp.SQLNonQuery("update " + schnm + ". T_BALE set  BALEYR= '" + VE.BALEYR1 + "', BLAUTONO= '" + VE.BLAUTONO1 + "',LRDT= '" + VE.LRDT1 + "',LRNO= '" + VE.LRNO1 + "', CLCD='" + CLCD + "' "
                        //   + " where BALENO='" + VE.BALENO1 + "' ");

                    }
                    else
                    {
                        var a = VE.NEWSTYLENO.Split('/');
                        var PAGENO = a[0];
                        var PAGESLNO = a[1];

                        sql = "update " + schnm + ". T_TXNDTL set  BALEYR= '" + VE.BALEYR2 + "', CLCD='" + CLCD + "',PAGENO='" + PAGENO + "',PAGESLNO='" + PAGESLNO + "'  "
                        + " where BALENO='" + VE.BALENO2 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                        sql = "update " + schnm + ". T_BATCHDTL set  BALEYR= '" + VE.BALEYR2 + "', CLCD='" + CLCD + "' "
                               + " where BALENO='" + VE.BALENO2 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                        sql = "update " + schnm + ". T_BILTY set  BALEYR= '" + VE.BALEYR2 + "', BLAUTONO= '" + VE.BLAUTONO2 + "',LRDT= '" + VE.LRDT2 + "',LRNO= '" + VE.LRNO2 + "', CLCD='" + CLCD + "' "
                             + " where BALENO='" + VE.BALENO2 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                        sql = "update " + schnm + ". T_BALE set  BALEYR= '" + VE.BALEYR2 + "', BLAUTONO= '" + VE.BLAUTONO2 + "',LRDT= '" + VE.LRDT2 + "',LRNO= '" + VE.LRNO2 + "', CLCD='" + CLCD + "' "
                           + " where BALENO='" + VE.BALENO2 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        //MasterHelp.SQLNonQuery("update " + schnm + ". T_TXNDTL set  BALEYR= '" + VE.BALEYR2 + "', CLCD='" + CLCD + "',PAGENO='" + PAGENO + "',PAGESLNO='" + PAGESLNO + "'  "
                        //+ " where BALENO='" + VE.BALENO2 + "' ");
                        //MasterHelp.SQLNonQuery("update " + schnm + ". T_BATCHDTL set  BALEYR= '" + VE.BALEYR2 + "', CLCD='" + CLCD + "' "
                        //       + " where BALENO='" + VE.BALENO2 + "' ");
                        //MasterHelp.SQLNonQuery("update " + schnm + ". T_BILTY set  BALEYR= '" + VE.BALEYR2 + "', BLAUTONO= '" + VE.BLAUTONO2 + "',LRDT= '" + VE.LRDT2 + "',LRNO= '" + VE.LRNO2 + "', CLCD='" + CLCD + "' "
                        //     + " where BALENO='" + VE.BALENO2 + "' ");
                        //MasterHelp.SQLNonQuery("update " + schnm + ". T_BALE set  BALEYR= '" + VE.BALEYR2 + "', BLAUTONO= '" + VE.BLAUTONO2 + "',LRDT= '" + VE.LRDT2 + "',LRNO= '" + VE.LRNO2 + "', CLCD='" + CLCD + "' "
                        //   + " where BALENO='" + VE.BALENO2 + "' ");
                    }



                    //DB.SaveChanges();
                    //ContentFlg = "Update sucessfully";
                    //DB.SaveChanges();
                    //transaction.Commit();
                    ModelState.Clear();
                    transaction.Commit();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    ContentFlg = "Update sucessfully";
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