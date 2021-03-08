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
    public class M_StktypeController : Controller
    {
        Connection Cn = new Connection();
        string CS = null;
        M_STKTYPE sl;
        M_CNTRL_HDR sll;
        M_BRAND sBRND;
        M_PRODGRP sPROD;
        MasterHelp Master_Help = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_Stktype  
        public ActionResult M_Stktype(FormCollection FC, string op = "", string key = "", int Nindex = 0, string searchValue = "")
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
                    ViewBag.formname = "Stock Type";
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    StockTypeEntry VE = new StockTypeEntry();

                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    //VE.Database_Combo1 = (from i in DB.M_STKTYPE select new Database_Combo1() { FIELD_VALUE = i.GRPNM }).Distinct().OrderBy(s => s.FIELD_VALUE).ToList();
                    
                    VE.DefaultAction = op;

                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_STKTYPE orderby p.STKTYPE select new IndexKey() { Navikey = p.STKTYPE }).ToList();

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
                            if (sll.INACTIVE_TAG == "Y")
                            {
                                VE.Deactive = true;
                            }
                            else
                            {
                                VE.Deactive = false;
                            }
                            VE.M_STKTYPE = sl;
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
                StockTypeEntry VE = new StockTypeEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public StockTypeEntry Navigation(StockTypeEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            try
            {
                sl = new M_STKTYPE();
                sll = new M_CNTRL_HDR();
                sBRND = new M_BRAND();
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();

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
                    sl = DB.M_STKTYPE.Find(aa[0]);
                    sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);

                    if (sl.ISDEFAULT == "Y")
                    {
                        VE.Checked = true;
                    }
                    else
                    {
                        VE.Checked = false;
                    }
                    if (sll.INACTIVE_TAG == "Y")
                    {
                        VE.Deactive = true;
                    }
                    else
                    {
                        VE.Deactive = false;
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
            var MDT = (from j in DB.M_STKTYPE
                       join o in DB.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                       where (j.M_AUTONO == o.M_AUTONO)
                       select new
                       {
                           STKTYPE = j.STKTYPE,
                           STKNAME = j.STKNAME,
                           M_AUTONO = o.M_AUTONO
                       }).OrderBy(s => s.STKTYPE).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Stock Type" + Cn.GCS() + "Stock Name" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].STKTYPE + "</td><td>" + MDT[j].STKNAME + "</td><td>" + MDT[j].M_AUTONO + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0", "2"));
        }

     
      
        public ActionResult ProductGrp(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_PRODGRP where (c.PRODGRPCD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.PRODGRPCD + Cn.GCS() + i.PRODGRPNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult CheckStockType(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_STKTYPE where (c.STKTYPE == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        //public ActionResult AddDOCRow(StockTypeEntry VE)
        //{
        //    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
        //    var doctP = (from i in DB1.MS_DOCCTG
        //                 select new DocumentType()
        //                 {
        //                     value = i.DOC_CTG,
        //                     text = i.DOC_CTG
        //                 }).OrderBy(s => s.text).ToList();
        //    if (VE.UploadDOC == null)
        //    {
        //        List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
        //        UploadDOC MLI = new UploadDOC();
        //        MLI.DocumentType = doctP;
        //        MLocIFSC1.Add(MLI);
        //        VE.UploadDOC = MLocIFSC1;
        //    }
        //    else
        //    {
        //        List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
        //        for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
        //        {
        //            UploadDOC MLI = new UploadDOC();
        //            MLI = VE.UploadDOC[i];
        //            MLI.DocumentType = doctP;
        //            MLocIFSC1.Add(MLI);
        //        }
        //        UploadDOC MLI1 = new UploadDOC();
        //        MLI1.DocumentType = doctP;
        //        MLocIFSC1.Add(MLI1);
        //        VE.UploadDOC = MLocIFSC1;
        //    }
        //    VE.DefaultView = true;
        //    return PartialView("_UPLOADDOCUMENTS", VE);

        //}
        //public ActionResult DeleteDOCRow(StockTypeEntry VE)
        //{
        //    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
        //    var doctP = (from i in DB1.MS_DOCCTG
        //                 select new DocumentType()
        //                 {
        //                     value = i.DOC_CTG,
        //                     text = i.DOC_CTG
        //                 }).OrderBy(s => s.text).ToList();
        //    List<UploadDOC> LOCAIFSC = new List<UploadDOC>();
        //    int count = 0;
        //    for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
        //    {
        //        if (VE.UploadDOC[i].chk == false)
        //        {
        //            count += 1;
        //            UploadDOC IFSC = new UploadDOC();
        //            IFSC = VE.UploadDOC[i];
        //            IFSC.DocumentType = doctP;
        //            LOCAIFSC.Add(IFSC);
        //        }
        //    }
        //    VE.UploadDOC = LOCAIFSC;
        //    ModelState.Clear();
        //    VE.DefaultView = true;
        //    return PartialView("_UPLOADDOCUMENTS", VE);

        //}
        public ActionResult SAVE(FormCollection FC, StockTypeEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_STKTYPE MSTKTYPE = new M_STKTYPE();
                        MSTKTYPE.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MSTKTYPE.EMD_NO = 0;
                            MSTKTYPE.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                            
                        }
                        if (VE.DefaultAction == "E")
                        {
                            MSTKTYPE.DTAG = "E";
                            MSTKTYPE.M_AUTONO = VE.M_STKTYPE.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_STKTYPE where p.M_AUTONO == VE.M_STKTYPE.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MSTKTYPE.EMD_NO = 0;
                            }
                            else
                            {
                                MSTKTYPE.EMD_NO = Convert.ToInt16(MAXEMDNO + 1);
                            }

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_STKTYPE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_STKTYPE.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_STKTYPE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_STKTYPE.M_AUTONO));

                        }
                        MSTKTYPE.STKTYPE = VE.M_STKTYPE.STKTYPE.ToUpper().Trim();
                        MSTKTYPE.STKNAME = VE.M_STKTYPE.STKNAME;
                        MSTKTYPE.SHORTNM = VE.M_STKTYPE.SHORTNM;
                        MSTKTYPE.FLAG1 = VE.M_STKTYPE.FLAG1;
                        if (VE.Checked==true) { MSTKTYPE.ISDEFAULT = "Y"; } else { MSTKTYPE.ISDEFAULT = null; }
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_STKTYPE", MSTKTYPE.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_STKTYPE.Add(MSTKTYPE);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MSTKTYPE).State = System.Data.Entity.EntityState.Modified;
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

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_STKTYPE", VE.M_STKTYPE.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_STKTYPE.Where(x => x.M_AUTONO == VE.M_STKTYPE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_STKTYPE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                       
                        DB.M_STKTYPE.RemoveRange(DB.M_STKTYPE.Where(x => x.M_AUTONO == VE.M_STKTYPE.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_STKTYPE.M_AUTONO));
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
        public ActionResult M_Stktype(FormCollection FC, StockTypeEntry VE)
        {
            try
            {
                string dbname = CommVar.CurSchema(UNQSNO).ToString();

                string query = "select a.STKTYPE, a.STKNAME,SHORTNM,FLAG1 ";
                query = query + " from " + dbname + ".M_STKTYPE a, " + dbname + ".m_cntrl_hdr c ";
                query = query + "where  a.m_autono=c.m_autono(+) and nvl(c.inactive_tag,'N') = 'N' and a.STKTYPE= '"+VE.M_STKTYPE.STKTYPE+"' ";
                query = query + "order by a.STKTYPE, a.STKNAME";

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
                    HC.GetPrintHeader(IR, "STKNAME", "string", "c,20", "Stock Name");
                    HC.GetPrintHeader(IR, "STKTYPE", "string", "c,7", "Stock Type");
                    //HC.GetPrintHeader(IR, "brandnm", "string", "c,20", "Brande");
                    HC.GetPrintHeader(IR, "SHORTNM", "string", "c,10", "Short Name");
                    HC.GetPrintHeader(IR, "FLAG1", "string", "c,10", "Flag");

                    for (int i = 0; i <= Cn.ds.Tables["mst_rep"].Rows.Count - 1; i++)
                    {
                        DataRow dr = IR.NewRow();
                        dr["STKNAME"] = Cn.ds.Tables["mst_rep"].Rows[i]["STKNAME"];
                        dr["STKTYPE"] = Cn.ds.Tables["mst_rep"].Rows[i]["STKTYPE"];
                        //dr["brandnm"] = Cn.ds.Tables["mst_rep"].Rows[i]["brandnm"];
                        dr["SHORTNM"] = Cn.ds.Tables["mst_rep"].Rows[i]["SHORTNM"];
                        dr["FLAG1"] = Cn.ds.Tables["mst_rep"].Rows[i]["FLAG1"];
                        dr["Flag"] = " class='grid_td'";
                        IR.Rows.Add(dr);
                    }
                    string pghdr1 = "";
                    string repname = CommFunc.retRepname("Stock Type Details");
                    pghdr1 = "Stock Type Details";
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