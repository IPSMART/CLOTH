using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Collections;
using System.Data;
using System.Net;
using System.Net.Sockets;
namespace Improvar.Controllers
{
    public class MS_UOMController : Controller
    {
        Connection Cn = new Connection();
        M_UOM sl; M_CNTRL_HDR sll; MS_GSTUOM SG1;
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: MS_UOM
        public ActionResult MS_UOM(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                UOMMasterEntry VE = new UOMMasterEntry();
                Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                ViewBag.formname = "UOM Master";
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(VE.UNQSNO));
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);             
              
                VE.DefaultAction = op;
                if (op.Length != 0)
                {
                  
                    VE.IndexKey = (from p in DB.M_UOM orderby p.UOMCD select new IndexKey() { Navikey = p.UOMCD }).ToList();

                    if (op == "E" || op == "D" || op == "V")
                    {
                        if (searchValue.Length != 0)
                        {
                            VE.Index = Nindex;
                            VE = Navigation(VE, DB, DB1, 0, searchValue);
                        }
                        else
                        {
                            if (key == "F")
                            {
                                VE.Index = 0;
                                VE = Navigation(VE, DB, DB1, 0, searchValue);
                            }
                            else if (key == "" || key == "L")
                            {
                                VE.Index = VE.IndexKey.Count - 1;
                                VE = Navigation(VE, DB, DB1, VE.IndexKey.Count - 1, searchValue);
                            }
                            else if (key == "P")
                            {
                                Nindex -= 1;
                                if (Nindex < 0)
                                {
                                    Nindex = 0;
                                }
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, DB1, Nindex, searchValue);
                            }
                            else if (key == "N")
                            {
                                Nindex += 1;
                                if (Nindex > VE.IndexKey.Count - 1)
                                {
                                    Nindex = VE.IndexKey.Count - 1;
                                }
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, DB1, Nindex, searchValue);
                            }
                        }
                        VE.M_UOM = sl;
                        VE.MS_GSTUOM = SG1;
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
        public UOMMasterEntry Navigation(UOMMasterEntry VE, ImprovarDB DB, ImprovarDB DB1, int index, string searchValue)
        {
            sl = new M_UOM(); sll = new M_CNTRL_HDR(); SG1 = new MS_GSTUOM();
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
                sl = DB.M_UOM.Find(aa[0]);
                SG1 = DB1.MS_GSTUOM.Find(sl.GST_UOMCD);
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
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var MDT = (from j in DBF.M_UOM
                       join o in DBF.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                       where (o.M_AUTONO == j.M_AUTONO)
                       select new
                       {
                           UOMCD = j.UOMCD,
                           UOMNM = j.UOMNM
                       }).OrderBy(s => s.UOMCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "UOM Code" + Cn.GCS() + "UOM Name";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].UOMCD + "</td><td>" + MDT[j].UOMNM + "</td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "0"));
        }
        public ActionResult CheckUOMCode(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            try
            {
                var query = (from c in DB.M_UOM where (c.UOMCD == val) select c);
                if (query.Any())
                {
                    return Content("1");
                }
                else
                {
                    return Content("0");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");

                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult GetGSTUOMCD()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            try
            {
                return PartialView("_Help2", masterHelp.GSTUOM(null));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GSTUOMCD(string val)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            try
            {
                var query = (from c in DB1.MS_GSTUOM where (c.GUOMCD == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.GUOMCD + "/" + i.GUOMNM;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }
            catch (Exception e)
            {
                Cn.SaveException(e, "");

                return Content(e.Message);

            }
        }
        public ActionResult SAVE(FormCollection FC, UOMMasterEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.FinSchema(UNQSNO) + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_UOM MUOM = new M_UOM();
                        MUOM.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MUOM.EMD_NO = 0;
                            MUOM.M_AUTONO = Cn.M_AUTONO(CommVar.FinSchema(UNQSNO));
                        }
                        if (VE.DefaultAction == "E")
                        {
                            MUOM.DTAG = "E";
                            MUOM.M_AUTONO = VE.M_UOM.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_UOM where p.M_AUTONO == VE.M_UOM.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MUOM.EMD_NO = 0;
                            }
                            else
                            {
                                MUOM.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                        }
                        MUOM.UOMCD = VE.M_UOM.UOMCD.ToUpper();
                        MUOM.UOMNM = VE.M_UOM.UOMNM;
                        MUOM.GST_UOMCD = VE.M_UOM.GST_UOMCD;
                        MUOM.DECIMALS = VE.M_UOM.DECIMALS;
                        MUOM.GST_QNTYCONV = VE.M_UOM.GST_QNTYCONV;
                        MUOM.TAREWT = VE.M_UOM.TAREWT;
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_UOM", MUOM.M_AUTONO.Value, VE.DefaultAction, CommVar.FinSchema(UNQSNO), VE.Audit_REM);

                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.M_UOM.Add(MUOM);
                            DB.SaveChanges();
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MUOM).State = System.Data.Entity.EntityState.Modified;
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_UOM", VE.M_UOM.M_AUTONO.Value, VE.DefaultAction, CommVar.FinSchema(UNQSNO), VE.Audit_REM);
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();
                        DB.M_UOM.Where(x => x.M_AUTONO == VE.M_UOM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                        DB.M_UOM.RemoveRange(DB.M_UOM.Where(x => x.M_AUTONO == VE.M_UOM.M_AUTONO));
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_UOM.M_AUTONO));
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
                    return Content(ex.Message + ex.InnerException);
                }
            }

        }
        [HttpPost]
        public ActionResult MS_UOM(FormCollection FC, UOMMasterEntry VE)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                string scm1 = CommVar.CurSchema(UNQSNO);
                string scm2 = Cn.Getschema;
                string sql = "";
                DataTable tbl;

                sql = "";
                sql += "select a.UOMCD,a.UOMNM,a.GST_UOMCD,b.GUOMNM,a.DECIMALS,a.TAREWT,a.GST_QNTYCONV from " + scm1 + ".M_UOM a," + scm2 + ".MS_GSTUOM b where a.GST_UOMCD=b.GUOMCD(+) ";
                tbl = masterHelp.SQLquery(sql);

                if (tbl.Rows.Count != 0)
                {
                    DataTable IR = new DataTable("mstrep");

                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();

                    HC.RepStart(IR, 2);
                    HC.GetPrintHeader(IR, "UOMCD", "string", "c,5", "UOM Code");
                    HC.GetPrintHeader(IR, "UOMNM", "string", "c,7", "UOM Name");
                    HC.GetPrintHeader(IR, "GSTUOMCD", "string", "c,5", "GST UOM Code");
                    HC.GetPrintHeader(IR, "GSTUOMNM", "string", "c,30", "GST UOM Name");
                    HC.GetPrintHeader(IR, "DECIMAL", "string", "c,8", "Decimals");
                    HC.GetPrintHeader(IR, "TWEIGHT", "string", "c,7", "Tare Weight");
                    HC.GetPrintHeader(IR, "GSTQNTY", "string", "c,6", "GST Qnty Conver Factor");

                    Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                    i = 0; maxR = tbl.Rows.Count - 1;

                    while (i <= maxR)
                    {


                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["UOMCD"] = tbl.Rows[i]["UOMCD"];
                        IR.Rows[rNo]["UOMNM"] = tbl.Rows[i]["UOMNM"];
                        IR.Rows[rNo]["GSTUOMCD"] = tbl.Rows[i]["GST_UOMCD"];
                        IR.Rows[rNo]["GSTUOMNM"] = tbl.Rows[i]["GUOMNM"];
                        IR.Rows[rNo]["DECIMAL"] = tbl.Rows[i]["DECIMALS"];
                        IR.Rows[rNo]["TWEIGHT"] = tbl.Rows[i]["TAREWT"];
                        IR.Rows[rNo]["GSTQNTY"] = tbl.Rows[i]["GST_QNTYCONV"];
                        i = i + 1;
                    }

                    string pghdr1 = "";

                    pghdr1 = "UOM Details";

                    PV = HC.ShowReport(IR, "MS_UOM", pghdr1, "", true, true, "P", false);

                    TempData["MS_UOM"] = PV;
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = "MS_UOM" });



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