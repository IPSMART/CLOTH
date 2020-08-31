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
    public class M_PaymentController : Controller
    {
        Connection Cn = new Connection();
        string CS = null;
        M_PAYMENT sl;
        M_CNTRL_HDR sll;
        M_GENLEG mgl;
        MasterHelp Master_Help = new MasterHelp();
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_Payment
        public ActionResult M_Payment(FormCollection FC, string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            //testing
            try
            {//test
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Payment Received";
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    PaymentReceivedEntry VE = new PaymentReceivedEntry();

                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    VE.DefaultAction = op;

                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_PAYMENT orderby p.PYMTCD select new IndexKey() { Navikey = p.PYMTCD }).ToList();

                        if (op == "E" || op == "D" || op == "V")
                        {
                            VE.Searchpannel_State = true;
                            if (searchValue.Length != 0)
                            {
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
                            VE.M_PAYMENT = sl;
                            VE.M_GENLEG = mgl;
                            VE.M_CNTRL_HDR = sll;
                        }
                       
                        return View(VE);
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                        return View(VE);
                    }
                }
            }
            catch (Exception ex)
            {
                PaymentReceivedEntry VE = new PaymentReceivedEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public PaymentReceivedEntry Navigation(PaymentReceivedEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            try
            {
                sl = new M_PAYMENT();
                sll = new M_CNTRL_HDR();
                mgl = new M_GENLEG();
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
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
                    sl = DB.M_PAYMENT.Find(aa[0]);
                    mgl = DB.M_GENLEG.Find(sl.GLCD);
                    sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);

                    if (sll.INACTIVE_TAG == "Y")
                    {
                        VE.Checked = true;
                    }
                    else
                    {
                        VE.Checked = false;
                    }
                  
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var MDT = (from j in DB.M_PAYMENT
                       join o in DB.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                       join k in DB.M_GENLEG on j.GLCD equals(k.GLCD)
                       where (j.M_AUTONO == o.M_AUTONO)
                       select new
                       {
                           PYMTCD = j.PYMTCD,
                           PYMTNM = j.PYMTNM,
                           GLCD = j.GLCD,
                           GLNM= k.GLNM,
                           M_AUTONO = o.M_AUTONO
                       }).OrderBy(s => s.PYMTCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Payment Code" + Cn.GCS() + "Payment Name" + Cn.GCS() + "Ledger Name" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].PYMTCD + "</td><td>" + MDT[j].PYMTNM + "</td><td>"+ MDT[j].GLNM +"["+ MDT[j].GLCD +"]"+"</td><td>" + MDT[j].M_AUTONO + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0", "3"));
        }
        //public ActionResult GetGenLedger()
        //{
        //    ImprovarDB DBfin = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        //    return PartialView("_Help2", Master_HelpFa.GLCD_help(DBfin));
        //}
        public ActionResult GetGenLedgerDetails(string val)
        {
            var str = Master_Help.GLCD_help(val);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public ActionResult GenLedger(string val)
        {
            ImprovarDB DBfin = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var query = (from c in DBfin.M_GENLEG where (c.GLCD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.GLCD + Cn.GCS() + i.GLNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
       
        public ActionResult SAVE(FormCollection FC, PaymentReceivedEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_PAYMENT MPAYMENT = new M_PAYMENT();
                        MPAYMENT.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MPAYMENT.EMD_NO = 0;
                            MPAYMENT.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                            MPAYMENT.PYMTCD = Cn.GenMasterCode("M_PAYMENT", "PYMTCD","", 2);
                        }
                        if (VE.DefaultAction == "E")
                        {

                            MPAYMENT.PYMTCD = VE.M_PAYMENT.PYMTCD;
                            MPAYMENT.DTAG = "E";
                            MPAYMENT.M_AUTONO = VE.M_PAYMENT.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_PAYMENT where p.M_AUTONO == VE.M_PAYMENT.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MPAYMENT.EMD_NO = 0;
                            }
                            else
                            {
                                MPAYMENT.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                            
                        }
                        MPAYMENT.PYMTNM = VE.M_PAYMENT.PYMTNM;
                        MPAYMENT.GLCD = VE.M_PAYMENT.GLCD;
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_PAYMENT", MPAYMENT.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_PAYMENT.Add(MPAYMENT);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MPAYMENT).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        DB.SaveChanges();
                        transaction.Commit();
                        string ContentFlg = "";
                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1";
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        return Content(ContentFlg);
                    }
                    else if (VE.DefaultAction == "V")
                    {
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_PAYMENT", VE.M_PAYMENT.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();
                        DB.M_PAYMENT.Where(x => x.M_AUTONO == VE.M_PAYMENT.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_PAYMENT.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        DB.M_PAYMENT.RemoveRange(DB.M_PAYMENT.Where(x => x.M_AUTONO == VE.M_PAYMENT.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_PAYMENT.M_AUTONO));
                        DB.SaveChanges();
                        transaction.Commit();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                }
                catch (Exception ex)
                {
                    Cn.SaveException(ex, "");
                    return Content(ex.Message);
                }
            }
        }
        [HttpPost]
        public ActionResult M_Payment(FormCollection FC)
        {
            try
            {
                string dbname = CommVar.CurSchema(UNQSNO).ToString();

                string query = "select a.PYMTCD, a.PYMTNM,GLCD,GLNM  ";
                query = query + " from " + dbname + ".M_PAYMENT a, " + dbname + ".m_cntrl_hdr c ," + dbname + ".m_genleg d ";
                query = query + "where  a.m_autono=c.m_autono(+) and a.GLCD=d.GLCD(+) and nvl(c.inactive_tag,'N') = 'N' ";
                query = query + "order by PYMTCD, a.PYMTNM";

                CS = Cn.GetConnectionString();

                Cn.con = new OracleConnection(CS);
                if ((Cn.ds.Tables["mst_rep"] == null) == false)
                {
                    Cn.ds.Tables["mst_rep"].Clear();
                }
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }

                string str = query;
                Cn.com = new OracleCommand(str, Cn.con);
                Cn.da.SelectCommand = Cn.com;
                bool bu = Convert.ToBoolean(Cn.da.Fill(Cn.ds, "mst_rep"));
                Cn.con.Close();
                var record = Master_Help.SQLquery(query);
                if (bu)
                {
                    DataTable IR = new DataTable("mstrep");

                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();

                    HC.RepStart(IR, 2);
                    HC.GetPrintHeader(IR, "PYMTNM", "string", "c,20", "Name");
                    HC.GetPrintHeader(IR, "PYMTCD", "string", "c,7", "Code");
                    //HC.GetPrintHeader(IR, "brandnm", "string", "c,20", "Brande");
                    HC.GetPrintHeader(IR, "GLNM", "string", "c,10", "Ledger Name");

                    for (int i = 0; i <= Cn.ds.Tables["mst_rep"].Rows.Count - 1; i++)
                    {
                        DataRow dr = IR.NewRow();
                        dr["PYMTNM"] = Cn.ds.Tables["mst_rep"].Rows[i]["PYMTNM"];
                        dr["PYMTCD"] = Cn.ds.Tables["mst_rep"].Rows[i]["PYMTCD"];
                        //dr["brandnm"] = Cn.ds.Tables["mst_rep"].Rows[i]["brandnm"];
                        dr["GLNM"] = Cn.ds.Tables["mst_rep"].Rows[i]["GLNM"]+"["+ Cn.ds.Tables["mst_rep"].Rows[i]["GLCD"] +"]";
                        dr["Flag"] = " class='grid_td'";
                        IR.Rows.Add(dr);
                    }
                    string pghdr1 = "";
                    string repname = CommFunc.retRepname("Group Master Details");
                    pghdr1 = "Group Master Details";
                    PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });

                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return View();
        }

        private ActionResult ResponsivePrintReport()
        {
            throw new NotImplementedException();
        }

        public ActionResult PrintReport()
        {
            return RedirectToAction("PrintViewer", "RPTViewer");
        }
    }
}