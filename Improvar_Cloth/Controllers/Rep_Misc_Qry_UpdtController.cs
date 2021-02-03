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
        public ActionResult GetBaleNoDetails(string val, string BALEAUTONO = "")
        {
            try
            {
                var tdt = CommVar.CurrDate(UNQSNO);
                if (val == null)
                {
                    return PartialView("_Help2", MasterHelp.BaleNo_help(val, tdt, BALEAUTONO));
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
                    string ContentFlg = "", sql2="";
                    var schnm = CommVar.CurSchema(UNQSNO);
                    var CLCD = CommVar.ClientCode(UNQSNO);
                    if (VE.TEXTBOX1 == "Change Style")
                    {
                        string sql1 = "select * from " + schnm + ". M_SITEM where STYLENO='" + VE.OLDSTYLENO + "' ";
                        DataTable dt = MasterHelp.SQLquery(sql1);
                        var itcd = (from DataRow dr in dt.Rows select new { itcd = dr["itcd"]    
                        }).FirstOrDefault();
                        string itcd_ = itcd.itcd.retStr();
                        var msitm = (from i in DB.M_SITEM where i.STYLENO== VE.OLDSTYLENO select i).FirstOrDefault();
                        var mitemuncnv = (from i in DB.M_ITEM_UN_CNV where i.ITCD == itcd_ select i).FirstOrDefault();
                        //var mscitngrp = (from i in DB.M_SCMITMGRP where i.ITCD == itcd_ select i).FirstOrDefault();
                        var msitmbom = (from i in DB.M_SITEMBOM where i.ITCD == itcd_ select i).FirstOrDefault();

                        sql2 = "insert into " + CommVar.FinSchema(UNQSNO) + ".M_SITEM(EMD_NO,CLCD,DTAG,TTAG,ITCD,ITNM,FABITCD,QUALITY,ITGRPCD,STYLENO,BRANDCD,SBRANDCD,GENDER,COLLCD,HSNCODE,COLRCD,PCSPERSET,UOMCD,FEATURE,DMNSN,SZWISEDTL,COLRWISEDTL,MINPURQTY,COLRPERSET,LINKITCD,NEGSTOCK,M_AUTONO,PRODGRPCD,SAPCODE)"
                                       + " values('" + msitm.EMD_NO + "','" + msitm.CLCD + "','" + msitm.DTAG + "','" + msitm.TTAG+"','"
                                       + VE.NEWSTYLENO + "','" + msitm.ITNM + "','" + msitm.FABITCD + "','" + msitm.QUALITY + "','" + msitm.ITGRPCD 
                                       + "','" + msitm.STYLENO + "','" + msitm.BRANDCD + "','" + msitm.SBRANDCD + "','" + msitm.GENDER + "','" 
                                       + msitm.COLLCD + "','" + msitm.PCSPERSET + "','" + msitm.UOMCD + "','" + msitm.FEATURE + "','" + msitm.DMNSN 
                                       + "','" + msitm.SZWISEDTL + "','" + msitm.COLRWISEDTL + "','" + msitm.MINPURQTY + "','" + msitm.COLRPERSET 
                                       + "','" + msitm.LINKITCD + "','" + msitm.NEGSTOCK + "','" + msitm.M_AUTONO + "','" + msitm.PRODGRPCD + "','" 
                                       + msitm.SAPCODE + "' )  ";
                        OraCmd.CommandText = sql2; OraCmd.ExecuteNonQuery();

                        sql2 = "insert into " + CommVar.FinSchema(UNQSNO) + ".M_ITEM_UN_CNV(EMD_NO,CLCD,DTAG,TTAG,ITCD,UOMCD,QNTY,CONV_FACT)"
                                    + " values('" + mitemuncnv.EMD_NO + "','" + mitemuncnv.CLCD + "','" + mitemuncnv.DTAG + "','" + mitemuncnv.TTAG + "','"
                                    + VE.NEWSTYLENO + "','" + mitemuncnv.UOM + "','" + mitemuncnv.QNTY + "','" + mitemuncnv.CONV_FACT + "')  ";
                        OraCmd.CommandText = sql2; OraCmd.ExecuteNonQuery();

                        sql2 = "insert into " + CommVar.FinSchema(UNQSNO) + ".M_ITEM_UN_CNV(EMD_NO,CLCD,DTAG,TTAG,ITCD,UOMCD,QNTY,CONV_FACT)"
                                   + " values('" + mitemuncnv.EMD_NO + "','" + mitemuncnv.CLCD + "','" + mitemuncnv.DTAG + "','" + mitemuncnv.TTAG + "','"
                                   + VE.NEWSTYLENO + "','" + mitemuncnv.UOM + "','" + mitemuncnv.QNTY + "','" + mitemuncnv.CONV_FACT + "')  ";
                        OraCmd.CommandText = sql2; OraCmd.ExecuteNonQuery();



                        sql = "update " + schnm + ". M_SITEM set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
                       + " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". M_ITEM_UN_CNV set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
                 + " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                  //      sql = "update " + schnm + ". M_SCMITMGRP set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
                  //+ " where ITCD='" + itcd.itcd + "' ";
                  //      OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". M_SITEMBOM set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
         + " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();


                        sql = "update " + schnm + ". M_SITEMBOMMTRL set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
                       + " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". M_SITEM_BARCODE set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
      + " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();


                        sql = "update " + schnm + ". M_SITEM_MEASURE set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
   + " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();



                        sql = "update " + schnm + ". M_SITEM_PARTS set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
+ " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();


                        sql = "update " + schnm + ". T_BATCHMST set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
      + " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();


                        sql = "update " + schnm + ". T_INHISSMST set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
+ " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_INHRECMST set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
+ " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();


                        sql = "update " + schnm + ". T_KARDTL set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
+ " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();


                        sql = "update " + schnm + ". T_PREVYR_DTL set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
+ " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                        sql = "update " + schnm + ". T_PROGBOM set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
+ " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_PROGMAST set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
+ " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();


                        sql = "update " + schnm + ". T_SORDDTL set ITCD= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
              + " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_TXNDTL set ITCD= '" + VE.NEWSTYLENO + "', BALEYR= '" + VE.BALEYR1 + "', CLCD='" + CLCD + "' "
                        + " where BALENO='" + VE.BALENO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_BATCHDTL set  BALEYR= '" + VE.BALEYR1 + "', CLCD='" + CLCD + "' "
                            + " where BALENO='" + VE.BALENO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_BILTY set  BALEYR= '" + VE.BALEYR1 + "', BLAUTONO= '" + VE.BLAUTONO1 + "',LRDT= to_date('" + VE.LRDT1 + "','dd/mm/yyyy'),LRNO= '" + VE.LRNO1 + "', CLCD='" + CLCD + "' "
               + " where BALENO='" + VE.BALENO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();


                        sql = "update " + schnm + ". T_BALE set  BALEYR= '" + VE.BALEYR1 + "', BLAUTONO= '" + VE.BLAUTONO1 + "',LRDT= to_date('" + VE.LRDT1 + "','dd/mm/yyyy'),LRNO= '" + VE.LRNO1 + "', CLCD='" + CLCD + "' "
                          + " where BALENO='" + VE.BALENO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                        

                    }
                    else
                    {
                        var a = VE.NEWPAGENO.Split('/');
                        var PAGENO = a[0];
                        var PAGESLNO = a[1];
                        sql = "update " + schnm + ". T_BALE set  BALEYR= '" + VE.BALEYR2 + "', BLAUTONO= '" + VE.BLAUTONO2 + "',LRDT= to_date('" + VE.LRDT2 + "','dd/mm/yyyy'),LRNO= '" + VE.LRNO2 + "', CLCD='" + CLCD + "' "
                        + " where BALENO='" + VE.BALENO2 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_BILTY set  BALEYR= '" + VE.BALEYR2 + "', BLAUTONO= '" + VE.BLAUTONO2 + "',LRDT= to_date('" + VE.LRDT2 + "','dd/mm/yyyy'),LRNO= '" + VE.LRNO2 + "', CLCD='" + CLCD + "' "
                              + " where BALENO='" + VE.BALENO2 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_BATCHDTL set  BALEYR= '" + VE.BALEYR2 + "', CLCD='" + CLCD + "' "
                              + " where BALENO='" + VE.BALENO2 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_TXNDTL set  BALEYR= '" + VE.BALEYR2 + "', CLCD='" + CLCD + "',PAGENO='" + VE.NEWPAGENO + "',PAGESLNO='" + VE.NEWPAGESLNO + "'  "
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