using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
using System.Data;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Newtonsoft.Json;
using Microsoft.Ajax.Utilities;

namespace Improvar.Controllers
{
    public class Gen_EinvIRNCancelController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        AdaequareGSP adaequareGSP = new AdaequareGSP();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string sql = "";
        string LOC = CommVar.Loccd(CommVar.getQueryStringUNQSNO()), COM = CommVar.Compcd(CommVar.getQueryStringUNQSNO()), scm1 = CommVar.CurSchema(CommVar.getQueryStringUNQSNO()), scmf = CommVar.FinSchema(CommVar.getQueryStringUNQSNO());
        // GET: Gen_EinvIRNCancel
        public ActionResult Gen_EinvIRNCancel(string reptype = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    GenEinvIRNCancel VE = new GenEinvIRNCancel();
                    ViewBag.formname = "Cancel E-Invoice";
                    VE.FROMDT = CommVar.CurrDate(UNQSNO);
                    VE.TODT = CommVar.CurrDate(UNQSNO);
                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                GenEinvIRNCancel VE = new GenEinvIRNCancel();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }

        public ActionResult ShowList(GenEinvIRNCancel VE, FormCollection FC)
        {
            try
            {
                string tdt = VE.TODT;
                string fdt = VE.FROMDT;
                sql = "";

                sql = "";
                string scmf = CommVar.FinSchema(UNQSNO);
                sql = "select distinct a.autono,c.doctype,d.docno,d.docdt,b.SLCD,b.SLNM,sum(a.BLAMT ) BLAMT,e.IRNNO from " + scmf + ".t_vch_gst a,"
                + scmf + ".m_subleg b," + scmf + ".m_doctype c," + scmf + ".t_cntrl_hdr d ," + scmf + ".t_txneinv e "
                   + " where a.pcode=b.slcd and  a.doccd=c.doccd and  a.autono=d.autono and  a.autono=e.autono and ";
                sql += "  A.docdt >= TO_DATE('" + fdt + "', 'DD/MM/YYYY') AND A.docdt <= TO_DATE('" + tdt + "', 'DD/MM/YYYY') AND  ";
                sql += " b.regntype in ('R') and a.salpur='S' and nvl(a.exemptedtype,' ') <> 'Z' and a.expcd is null ";
                sql += " and a.autono in (select autono from " + scmf + ".t_txneinv) ";
                sql += " and d.compcd='" + CommVar.Compcd(UNQSNO) + "' and d.loccd='" + CommVar.Loccd(UNQSNO) + "' ";
                sql += " group by a.autono,c.doctype,d.docno,d.docdt,b.SLCD,b.SLNM,e.IRNNO ";
                sql += " order by a.autono ";
                DataTable txn = masterHelp.SQLquery(sql);
                DataTable dt = masterHelp.SQLquery(sql);
                VE.GenEinvIRNGrid = (from DataRow dr in dt.Rows
                                     select new GenEinvIRNGrid
                                     {
                                         AUTONO = dr["AUTONO"].retStr(),
                                         IRNNO = dr["IRNNO"].retStr(),
                                         BLNO = dr["docno"].retStr(),
                                         BLDT = dr["docdt"].retDateStr(),
                                         SLNM = dr["SLNM"].retStr() + "[" + dr["SLCD"].retStr() + "]",
                                         BLAMT = dr["BLAMT"].retDbl()
                                     }).DistinctBy(a => a.AUTONO).OrderBy(a => a.BLDT).ToList();
                int slno = 1;
                for (int p = 0; p <= VE.GenEinvIRNGrid.Count - 1; p++)
                {
                    VE.GenEinvIRNGrid[p].SLNO = (slno + p).retInt();
                }
                ModelState.Clear();
                return PartialView("_Gen_EinvIRNCancel", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }

        public ActionResult GenerateEinvIRN(GenEinvIRNCancel VE)
        {
            try
            {
                VE = GetIRNdetails(VE);
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_Gen_EinvIRNCancel", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException + "  ");
            }
        }
        private GenEinvIRNCancel GetIRNdetails(GenEinvIRNCancel VE)
        {
            try
            {
                for (int gridindex = 0; gridindex < VE.GenEinvIRNGrid.Count; gridindex++)
                {
                    string autono = VE.GenEinvIRNGrid[gridindex].AUTONO;
                    if (VE.GenEinvIRNGrid[gridindex].Checked == true && VE.GenEinvIRNGrid[gridindex].Remarks != null)
                    {
                        AdaequareIRNCancel adaequareIRNCancel = new AdaequareIRNCancel();
                        adaequareIRNCancel.irn = VE.GenEinvIRNGrid[gridindex].IRNNO;
                        adaequareIRNCancel.cnlrem = VE.GenEinvIRNGrid[gridindex].Remarks;
                        adaequareIRNCancel.cnlrsn = "1";
                        string jsonstr = JsonConvert.SerializeObject(adaequareIRNCancel);
                        Cn.SaveTextFile(jsonstr);
                        string status = "";
                        AdqrRespGenIRNCancel adqrRespGenIRN = adaequareGSP.AdqrGenIRNCancel(jsonstr);
                        if (adqrRespGenIRN != null && adqrRespGenIRN.result != null)
                        {
                            sql = "update " + CommVar.FinSchema(UNQSNO) + ".t_txneinv set CANCELRSN='" + adaequareIRNCancel.cnlrsn
                                + "', CANCELDT =to_date('" + adqrRespGenIRN.result.CancelDate + "','yyyy-mm-dd hh24:mi:ss'),CANCELREM='" + adqrRespGenIRN.message
                                + "' where autono='" + autono + "'";
                            masterHelp.SQLquery(sql);
                            cancelRecords(autono, adaequareIRNCancel.cnlrem);
                            status = "" + adqrRespGenIRN.message + "";
                            VE.GenEinvIRNGrid[gridindex].IRNNO = adqrRespGenIRN.result.Irn.ToString();                 
                        }
                        VE.GenEinvIRNGrid[gridindex].MESSAGE = adqrRespGenIRN.message;
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                VE.Message = ex.Message;
            }
            return VE;
        }
        private void cancelRecords(string autono, string remarks)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    TCH = Cn.T_CONTROL_HDR(autono, CommVar.CurSchema(UNQSNO), remarks);
                    DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();
                    transaction.Commit();
                }
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DBF.Database.ExecuteSqlCommand("lock table " + CommVar.FinSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH1 = new T_CNTRL_HDR();
                    TCH1 = Cn.T_CONTROL_HDR(autono, CommVar.FinSchema(UNQSNO), remarks);
                    DBF.Entry(TCH1).State = System.Data.Entity.EntityState.Modified;
                    DBF.SaveChanges();
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
        }
    }
}