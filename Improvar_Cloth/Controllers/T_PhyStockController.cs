using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using System.Reflection;

namespace Improvar.Controllers
{
    public class T_PhyStockController : Controller
    {
        // GET: T_PhyStock
        Connection Cn = new Connection(); MasterHelp Master_Help = new MasterHelp(); MasterHelpFa MasterHelpFa = new MasterHelpFa(); SchemeCal Scheme_Cal = new SchemeCal(); Salesfunc salesfunc = new Salesfunc(); DataTable DT = new DataTable(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_PHYSTK_HDR TPH; T_PHYSTK TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR;
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();

        public ActionResult T_PhyStock(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Physical Stock";
                    TransactionPhyStockEntry VE = (parkID == "") ? new TransactionPhyStockEntry() : (Improvar.ViewModels.TransactionPhyStockEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    VE.DropDown_list_StkType = Master_Help.STK_TYPE();
                    VE.DropDown_list_MTRLJOBCD = Master_Help.MTRLJOBCD_List();
                    if (op.Length != 0)
                    {
                        string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();

                        VE.IndexKey = (from p in DB.T_PHYSTK_HDR
                                       join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       orderby q.DOCDT, q.DOCNO
                                       where XYZ.Contains(q.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YR1
                                       select new IndexKey() { Navikey = p.AUTONO }).ToList();
                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
                        if (op == "E" || op == "D" || op == "V")
                        {
                            if (searchValue.Length != 0)
                            {
                                if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
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
                            VE.T_PHYSTK_HDR = TPH;
                            VE.T_CNTRL_HDR = TCH;
                            VE.T_CNTRL_HDR_REM = SLR;
                            if (VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {

                                T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                                TCH.DOCDT = Cn.getCurrentDate(VE.mindate);
                                if (VE.DocumentType.Count == 1)
                                {
                                    TCH.DOCCD = VE.DocumentType[0].value;
                                }
                                VE.T_CNTRL_HDR = TCH;

                                T_PHYSTK_HDR TPHYSTKHDR = new T_PHYSTK_HDR();
                                TPHYSTKHDR.GOCD = TempData["LASTGOCD" + VE.MENU_PARA].retStr();
                                TPHYSTKHDR.PRCCD = TempData["LASTPRCCD" + VE.MENU_PARA].retStr();
                                TempData.Keep();
                                string doccd = "";
                                if (VE.DocumentType.Count() > 0)
                                {
                                    doccd = VE.DocumentType.FirstOrDefault().value;
                                }
                                var T_PHYSTK_HDR = (from a in DB.T_PHYSTK_HDR where a.DOCCD == doccd select new { a.GOCD, a.PRCCD }).FirstOrDefault();


                                if (TPHYSTKHDR.GOCD.retStr() == "")
                                {
                                    TPHYSTKHDR.GOCD = T_PHYSTK_HDR.GOCD;
                                }
                                string gocd = TPHYSTKHDR.GOCD.retStr();
                                if (gocd != "")
                                {
                                    VE.GONM = DBF.M_GODOWN.Where(a => a.GOCD == gocd).Select(b => b.GONM).FirstOrDefault();
                                }

                                if (TPHYSTKHDR.PRCCD.retStr() == "")
                                {
                                    TPHYSTKHDR.PRCCD = T_PHYSTK_HDR.PRCCD;
                                }
                                string prccd = TPHYSTKHDR.PRCCD.retStr();
                                if (prccd != "")
                                {
                                    VE.PRCNM = DBF.M_PRCLST.Where(a => a.PRCCD == prccd).Select(b => b.PRCNM).FirstOrDefault();
                                }
                                VE.T_PHYSTK_HDR = TPHYSTKHDR;

                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;

                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (TransactionPhyStockEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);
                        //VE.PRCCD = "WP";
                        //var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                        //VE.M_SYSCNFG = MSYSCNFG;
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (TCH != null) if (TCH.DOCDT != null) docdt = TCH.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_CNTRL_HDR.DOCCD, docdt, VE.DefaultAction, VE.T_CNTRL_HDR.DOCONLYNO, VE);
                    if (op.ToString() == "A" && parkID == "")
                    {
                        VE.T_CNTRL_HDR.DOCDT = Cn.getCurrentDate(VE.mindate);
                    }
                    var MSYSCNFG = salesfunc.M_SYSCNFG(VE.T_CNTRL_HDR.DOCDT.retDateStr());
                    if (MSYSCNFG == null)
                    {

                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                        ViewBag.ErrorMessage = "Data add in Configuaration Setup->Posting/Terms Setup";
                        return View(VE);
                    }
                    VE.M_SYSCNFG = MSYSCNFG;
                    VE.AUTOADDROW = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                TransactionPhyStockEntry VE = new TransactionPhyStockEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionPhyStockEntry Navigation(TransactionPhyStockEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE);

            TPH = new T_PHYSTK_HDR(); TXNOTH = new T_PHYSTK(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM();
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
                TPH = DB.T_PHYSTK_HDR.Find(aa[0].Trim());
                TCH = DB.T_CNTRL_HDR.Find(TPH.AUTONO);
                VE.GONM = TPH.GOCD.retStr() == "" ? "" : DBF.M_GODOWN.Where(a => a.GOCD == TPH.GOCD).Select(b => b.GONM).FirstOrDefault();
                if (TPH.PRCCD.retStr() != "")
                {
                    VE.PRCNM = DBF.M_PRCLST.Where(a => a.PRCCD == TPH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();
                }
                string scmf = CommVar.FinSchema(UNQSNO); string Scm = CommVar.CurSchema(UNQSNO);
                //string sql = "";
                //sql += " select a.prccd, a.prcnm ";
                //sql += "  from " + scmf + ".m_prclst a ";
                //sql += " where  a.prccd='WP' ";

                //DataTable prcslist = Master_Help.SQLquery(sql);
                //if (prcslist != null && prcslist.Rows.Count > 0)
                //{

                //VE.PRCCD = "WP"; 
                //prcslist.Rows[0]["prccd"].retStr();
                //    VE.PRCNM = prcslist.Rows[0]["prcnm"].retStr();

                //}
                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TPH.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TPH.AUTONO);

                string str = "";
                str += "select a.autono,b.itcd,a.slno,a.barno,a.stktype,a.mtrljobcd,a.partcd,a.nos,a.qnty,a.itrem,a.rate,a.cutlength,a.locabin,a.shade,a.baleyr,a.baleno,c.styleno||' '||c.itnm itstyle,c.styleno ";
                str += " from " + Scm + ".T_PHYSTK a," + Scm + ".t_batchmst b," + Scm + ".m_sitem c ";
                str += " where  a.autono='" + TPH.AUTONO + "' and a.barno=b.barno(+) and b.itcd=c.itcd(+)   ";
                str += "order by a.slno ";

                DataTable TPHYSTKtbl = Master_Help.SQLquery(str);
                VE.TPHYSTK = (from DataRow dr in TPHYSTKtbl.Rows
                              select new TPHYSTK()
                              {
                                  SLNO = Convert.ToInt16(dr["slno"]),
                                  BARNO = dr["barno"].retStr(),
                                  STYLENO = dr["STYLENO"].retStr(),
                                  SHADE = dr["shade"].retStr(),
                                  MTRLJOBCD = dr["mtrljobcd"].retStr(),
                                  PARTCD = dr["partcd"].retStr(),
                                  CUTLENGTH = dr["cutlength"].retDbl(),
                                  NOS = dr["nos"].retDbl(),
                                  QNTY = dr["qnty"].retDbl(),
                                  RATE = dr["rate"].retDbl(),
                                  STKTYPE = dr["stktype"].retStr(),
                                  ITREM = dr["itrem"].retStr(),
                                  BALENO = dr["baleno"].retStr(),
                                  BALEYR = dr["baleyr"].retStr(),
                                  ITSTYLE = dr["itstyle"].retStr(),
                              }).OrderBy(s => s.SLNO).ToList();
                VE.B_T_QNTY = VE.TPHYSTK.Sum(a => a.QNTY).retDbl();
                VE.B_T_NOS = VE.TPHYSTK.Sum(a => a.NOS).retDbl();
            }
            //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
            if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            return VE;
        }
        public ActionResult SearchPannelData(TransactionPhyStockEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            Cn.getQueryString(VE);

            List<DocumentType> DocumentType = new List<DocumentType>();
            DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
            string sql = "";

            sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd,a.gocd,c.gonm,sum(nvl(d.qnty,0))qnty,b.usr_id ";
            sql += "from " + scm + ".T_PHYSTK_HDR a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_godown c, " + scm + ".T_PHYSTK d ";
            sql += "where a.autono=b.autono and a.gocd=c.gocd(+) and a.autono=d.autono and b.doccd in (" + doccd + ") and ";
            if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
            // if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
            sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
            sql += "group by a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy'), b.doccd,a.gocd,c.gonm,b.usr_id ";
            sql += "order by docdt, docno ";
            DataTable tbl = Master_Help.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Godown Name" + Cn.GCS() + "Quantity" + Cn.GCS() + "Created By" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {
                SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["gonm"] + "</b> [" + tbl.Rows[j]["gocd"] + "]</td><td>" + tbl.Rows[j]["qnty"] + " </td><td>" + tbl.Rows[j]["usr_id"] + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "5", "5"));
        }
        public ActionResult GetBarCodeDetails(string val, string Code)
        {
            try
            {
                //sequence MTRLJOBCD/PARTCD/DOCDT/TAXGRPCD/GOCD/PRCCD/ALLMTRLJOBCD/HelpFrom
                TransactionPhyStockEntry VE = new TransactionPhyStockEntry();
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string barnoOrStyle = val.retStr();
                string MTRLJOBCD = data[0].retSqlformat();
                string PARTCD = data[1].retStr();
                string DOCDT = data[2].retStr();
                string TAXGRPCD = data[3].retStr();
                string GOCD = data[2].retStr() == "" ? "" : data[4].retStr().retSqlformat();
                string PRCCD = data[5].retStr();
                string BARNO = data[8].retStr() == "" || val.retStr() == "" ? "" : data[8].retStr().retSqlformat();
                bool exactbarno = data[7].retStr() == "Bar" ? true : false;
                if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = data[6].retStr(); }
                //if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = "FS".retSqlformat(); }
                if (PRCCD.retStr() == "") return Content("Please Select / Enter Price Code");
                string str = Master_Help.T_TXN_BARNO_help(barnoOrStyle, VE.MENU_PARA, DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD, "", exactbarno, "", BARNO);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    //if (str.IndexOf(Cn.GCS()) == -1) return Content(str);
                    //if (str.retCompValue("UOMCD").retStr() == "PCS")
                    //{
                    //    str = str.ReplaceHelpStr("BALQNTY", "1");
                    //}
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPartDetails(string val)
        {
            try
            {
                var str = Master_Help.PARTCD_help(val);
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
        public ActionResult GetMaterialJobDetails(string val)
        {
            try
            {
                string str = Master_Help.MTRLJOBCD_help(val);
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
        public static string RenderRazorViewToString(ControllerContext controllerContext, string viewName, object model)
        {
            controllerContext.Controller.ViewData.Model = model;
            using (var stringWriter = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, stringWriter);
                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return stringWriter.GetStringBuilder().ToString();
            }
        }
        public ActionResult DeleteRow(TransactionPhyStockEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<TPHYSTK> ITEMSIZE = new List<TPHYSTK>();
                int count = 0;
                for (int i = 0; i <= VE.TPHYSTK.Count - 1; i++)
                {
                    if (VE.TPHYSTK[i].Checked == false)
                    {
                        count += 1;
                        TPHYSTK item = new TPHYSTK();
                        item = VE.TPHYSTK[i];
                        item.SLNO = count.retShort();
                        ITEMSIZE.Add(item);
                    }

                }
                VE.TPHYSTK = ITEMSIZE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_PhyStock_BarTab", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(TransactionPhyStockEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            var doctP = (from i in DB1.MS_DOCCTG
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            if (VE.UploadDOC == null)
            {
                List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                UploadDOC MLI = new UploadDOC();
                MLI.DocumentType = doctP;
                MLocIFSC1.Add(MLI);
                VE.UploadDOC = MLocIFSC1;
            }
            else
            {
                List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                {
                    UploadDOC MLI = new UploadDOC();
                    MLI = VE.UploadDOC[i];
                    MLI.DocumentType = doctP;
                    MLocIFSC1.Add(MLI);
                }
                UploadDOC MLI1 = new UploadDOC();
                MLI1.DocumentType = doctP;
                MLocIFSC1.Add(MLI1);
                VE.UploadDOC = MLocIFSC1;
            }
            VE.DefaultView = true;
            return PartialView("_UPLOADDOCUMENTS", VE);

        }
        public ActionResult DeleteDOCRow(TransactionPhyStockEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            var doctP = (from i in DB1.MS_DOCCTG
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            List<UploadDOC> LOCAIFSC = new List<UploadDOC>();
            int count = 0;
            for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
            {
                if (VE.UploadDOC[i].chk == false)
                {
                    count += 1;
                    UploadDOC IFSC = new UploadDOC();
                    IFSC = VE.UploadDOC[i];
                    IFSC.DocumentType = doctP;
                    LOCAIFSC.Add(IFSC);
                }
            }
            VE.UploadDOC = LOCAIFSC;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_UPLOADDOCUMENTS", VE);

        }
        public ActionResult cancelRecords(TransactionPhyStockEntry VE, string par1)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                Cn.getQueryString(VE);
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_PHYSTK_HDR.AUTONO, CommVar.CurSchema(UNQSNO));
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_PHYSTK_HDR.AUTONO, CommVar.CurSchema(UNQSNO), par1);
                    }
                    DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();
                    transaction.Commit();
                    return Content("1");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult ParkRecord(FormCollection FC, TransactionPhyStockEntry stream)
        {
            try
            {
                Cn.getQueryString(stream);
                if (stream.T_CNTRL_HDR.DOCCD.retStr() != "")
                {
                    stream.T_CNTRL_HDR.DOCCD = stream.T_CNTRL_HDR.DOCCD.retStr();
                }
                string MNUDET = stream.MENU_DETAILS;
                var menuID = MNUDET.Split('~')[0];
                var menuIndex = MNUDET.Split('~')[1];
                string ID = menuID + menuIndex + CommVar.Loccd(UNQSNO) + CommVar.Compcd(UNQSNO) + CommVar.CurSchema(UNQSNO) + "*" + DateTime.Now;
                ID = ID.Replace(" ", "_");
                string Userid = Session["UR_ID"].ToString();
                INI Handel_ini = new INI();
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JR = javaScriptSerializer.Serialize(stream);
                Handel_ini.IniWriteValue(Userid, ID, Cn.Encrypt(JR), Server.MapPath("~/Park.ini"));
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public string RetrivePark(string value)
        {
            try
            {
                string url = Master_Help.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionPhyStockEntry");
                return url;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message;
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionPhyStockEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //Oracle Queries
            string dberrmsg = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "", postdt = "", weekrem = "", duedatecalcon = "", sql = "";
            string[] dbsql1;
            double dbDrAmt = 0, dbCrAmt = 0;

            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            //
            DB.Configuration.ValidateOnSaveEnabled = false;
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    //DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                    OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
                    String query = "";
                    string dr = ""; string cr = ""; int isl = 0; string strrem = "";
                    double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
                    Int32 z = 0; Int32 maxR = 0;
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        T_PHYSTK_HDR TBHDR = new T_PHYSTK_HDR();
                        T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                        string DOCPATTERN = "";
                        TCH.DOCDT = VE.T_CNTRL_HDR.DOCDT;
                        string Ddate = Convert.ToString(TCH.DOCDT);
                        TBHDR.CLCD = CommVar.ClientCode(UNQSNO);
                        string auto_no = ""; string Month = "", DOCNO = "", DOCCD = "";
                        if (VE.DefaultAction == "A")
                        {
                            TBHDR.EMD_NO = 0;
                            DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            DOCNO = Cn.MaxDocNumber(DOCCD, Ddate);
                            DOCPATTERN = Cn.DocPattern(Convert.ToInt32(DOCNO), DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                            auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), DOCNO, DOCCD, Ddate);
                            TBHDR.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();

                            TempData["LASTGOCD" + VE.MENU_PARA] = VE.T_PHYSTK_HDR.GOCD;
                            TempData["LASTPRCCD" + VE.MENU_PARA] = VE.T_PHYSTK_HDR.PRCCD;

                        }
                        else
                        {
                            DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            DOCNO = VE.T_CNTRL_HDR.DOCONLYNO;
                            TBHDR.AUTONO = VE.T_PHYSTK_HDR.AUTONO;
                            Month = VE.T_CNTRL_HDR.MNTHCD;
                            var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == VE.T_PHYSTK_HDR.AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null) { TBHDR.EMD_NO = 0; } else { TBHDR.EMD_NO = Convert.ToInt16(MAXEMDNO + 1); }
                        }
                        TBHDR.DOCCD = DOCCD;
                        TBHDR.DOCDT = TCH.DOCDT;
                        TBHDR.DOCNO = DOCNO;
                        TBHDR.GOCD = VE.T_PHYSTK_HDR.GOCD;
                        TBHDR.TREM = VE.T_PHYSTK_HDR.TREM;
                        TBHDR.PRCCD = VE.T_PHYSTK_HDR.PRCCD;
                        if (VE.DefaultAction == "E")
                        {
                            dbsql = MasterHelpFa.TblUpdt("T_PHYSTK", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                        }

                        //----------------------------------------------------------//
                        dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(TBHDR.AUTONO, VE.DefaultAction, "S", Month, DOCCD, DOCPATTERN, TCH.DOCDT.retStr(), TBHDR.EMD_NO.retShort(), DOCNO, Convert.ToDouble(DOCNO), null, null, null, null, VE.T_CNTRL_HDR.DOCAMT.retDbl());
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = MasterHelpFa.RetModeltoSql(TBHDR, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        int COUNTER = 0;

                        for (int i = 0; i <= VE.TPHYSTK.Count - 1; i++)
                        {
                            if (VE.TPHYSTK[i].SLNO != 0 && VE.TPHYSTK[i].QNTY != 0)
                            {
                                COUNTER = COUNTER + 1;
                                T_PHYSTK TPHYSTK = new T_PHYSTK();
                                TPHYSTK.CLCD = TBHDR.CLCD;
                                TPHYSTK.AUTONO = TBHDR.AUTONO;
                                TPHYSTK.SLNO = VE.TPHYSTK[i].SLNO;
                                TPHYSTK.BARNO = VE.TPHYSTK[i].BARNO;
                                TPHYSTK.STKTYPE = VE.TPHYSTK[i].STKTYPE;
                                TPHYSTK.MTRLJOBCD = VE.TPHYSTK[i].MTRLJOBCD;
                                TPHYSTK.PARTCD = VE.TPHYSTK[i].PARTCD;
                                TPHYSTK.NOS = VE.TPHYSTK[i].NOS;
                                TPHYSTK.QNTY = VE.TPHYSTK[i].QNTY;
                                TPHYSTK.RATE = VE.TPHYSTK[i].RATE;
                                TPHYSTK.SHADE = VE.TPHYSTK[i].SHADE;
                                TPHYSTK.CUTLENGTH = VE.TPHYSTK[i].CUTLENGTH;
                                TPHYSTK.ITREM = VE.TPHYSTK[i].ITREM;
                                TPHYSTK.DIA = VE.TPHYSTK[i].DIA;
                                TPHYSTK.LOCABIN = VE.TPHYSTK[i].LOCABIN;
                                TPHYSTK.BALEYR = VE.TPHYSTK[i].BALEYR;
                                TPHYSTK.BALENO = VE.TPHYSTK[i].BALENO;
                                dbsql = MasterHelpFa.RetModeltoSql(TPHYSTK);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            }
                        }

                        if (VE.UploadDOC != null)// add
                        {
                            var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TBHDR.AUTONO, TBHDR.EMD_NO.Value);
                            if (img.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= img.Item1.Count - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(img.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                                for (int tr = 0; tr <= img.Item2.Count - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(img.Item2[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                        {
                            var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TBHDR.AUTONO, TBHDR.CLCD, TBHDR.EMD_NO.Value);
                            if (NOTE.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(NOTE.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }


                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1" + " (Physical Stock No. " + DOCCD + DOCNO + ")~" + TBHDR.AUTONO;
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        transaction.Commit();
                        OraTrans.Commit();
                        OraCon.Dispose();
                        return Content(ContentFlg);
                    }
                    else if (VE.DefaultAction == "V")
                    {
                        dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_PHYSTK_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", VE.T_PHYSTK_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", VE.T_PHYSTK_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = MasterHelpFa.TblUpdt("t_batchmst_price", VE.T_PHYSTK_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("T_PHYSTK", VE.T_PHYSTK_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("T_PHYSTK_HDR", VE.T_PHYSTK_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                        dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(VE.T_PHYSTK_HDR.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


                        ModelState.Clear();
                        transaction.Commit();
                        OraTrans.Commit();
                        OraCon.Dispose();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                    goto dbok;
                    dbnotsave:;
                    transaction.Rollback();
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    return Content(dberrmsg);
                    dbok:;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        public ActionResult GetGodownDetails(string val)
        {
            try
            {
                var str = Master_Help.GOCD_help(val);
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
        public ActionResult GetPriceDetails(string val)
        {
            try
            {
                var str = Master_Help.PRCCD_help(val);
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
        public ActionResult GetLocationBinDetails(string val)
        {
            try
            {
                var str = Master_Help.LOCABIN_help(val);
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
        public ActionResult DocumentDateChng(TransactionPhyStockEntry VE, string docdt)
        {
            try
            {
                string str = "";
                var dt = salesfunc.GetSyscnfgData(docdt);
                if (dt.Rows.Count > 0)
                {
                    str = Master_Help.ToReturnFieldValues("", dt);
                }
                VE.M_SYSCNFG = salesfunc.M_SYSCNFG(docdt);
                ModelState.Clear();
                VE.DefaultView = true;
                var GRID_DATA = RenderRazorViewToString(ControllerContext, "_T_PhyStock_BarTab", VE);
                return Content(str + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + GRID_DATA);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult UpdatePrice(TransactionPhyStockEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string dbsql = "";
            string[] dbsql1;
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {
                    DataTable M_SYSCNFG = salesfunc.GetSyscnfgData(VE.POPUPEFFDT.retDateStr());
                    var TPHYSTK = (from a in VE.TPHYSTK
                                   select new
                                   {
                                       BARNO = a.BARNO,
                                       RATE = a.RATE
                                   }).Distinct().ToList();
                    string ContentFlg = "";
                    for (int i = 0; i <= TPHYSTK.Count - 1; i++)
                    {
                        double WPRATE = 0, RPRATE = 0;
                        string PRCCD = VE.POPUPPRCCD;
                        double RATE = TPHYSTK[i].RATE.retDbl();
                        double WPRPRATE = 0;

                        #region wp_rprate calculation
                        if (PRCCD == "WP")
                        {
                            double WPPER = VE.POPUPINCDECPER.retDbl();
                            if (WPPER == 0)
                            {
                                WPPER = M_SYSCNFG.Rows[0]["WPPER"].retDbl();
                            }
                            string WPPRICEGEN = M_SYSCNFG.Rows[0]["WPPRICEGEN"].retStr();
                            if (WPPER != 0)
                            {
                                var wprt = ((RATE.retDbl() * WPPER.retDbl()) / 100) + RATE.retDbl();
                                var B_WPRATE = CommFunc.CharmPrice((WPPRICEGEN.retStr() == "" ? "" : WPPRICEGEN.retStr().Substring(0, 2)), wprt.retInt(), (WPPRICEGEN.retStr() == "" ? "" : WPPRICEGEN.retStr().Substring(2)));
                                WPRATE = B_WPRATE.retDbl().toRound(2);
                            }
                            WPRPRATE = WPRATE.retDbl();
                        }
                        else if (PRCCD == "RP")
                        {
                            double RPPER = VE.POPUPINCDECPER.retDbl();
                            if (RPPER == 0)
                            {
                                RPPER = M_SYSCNFG.Rows[0]["RPPER"].retDbl();
                            }
                            string RPPRICEGEN = M_SYSCNFG.Rows[0]["RPPRICEGEN"].retStr();
                            if (RPPER != 0)
                            {
                                var rprt = ((RATE.retDbl() * RPPER.retDbl()) / 100) + RATE.retDbl();
                                var B_RPRATE = CommFunc.CharmPrice((RPPRICEGEN.retStr() == "" ? "" : RPPRICEGEN.retStr().Substring(0, 2)), rprt.retInt(), (RPPRICEGEN.retStr() == "" ? "" : RPPRICEGEN.retStr().Substring(2)));
                                RPRATE = B_RPRATE.retDbl().toRound(2);
                            }
                            WPRPRATE = RPRATE.retDbl();
                        }
                        #endregion

                        if (WPRPRATE != 0)
                        {
                            bool dataexist = false;
                            string barno = TPHYSTK[i].BARNO;
                            var chkdata = DB1.T_BATCHMST_PRICE.Where(a => a.BARNO == barno
                            && a.EFFDT == VE.POPUPEFFDT && a.PRCCD == PRCCD && a.AUTONO != VE.T_PHYSTK_HDR.AUTONO).Select(a => a.BARNO).Distinct().ToList();
                            if (chkdata.Count > 0) dataexist = true;

                            if (dataexist == false)
                            {
                                var chk = DB1.T_BATCHMST_PRICE.Where(a => a.BARNO == barno
                                && a.EFFDT == VE.POPUPEFFDT && a.PRCCD == PRCCD && a.AUTONO == VE.T_PHYSTK_HDR.AUTONO).Select(a => a.BARNO).Distinct().ToList();
                                if (chk.Count > 0)
                                {
                                    dbsql = Master_Help.TblUpdt("t_batchmst_price", VE.T_PHYSTK_HDR.AUTONO, "E", "", "barno='" + barno + "' and effdt =to_date('" + VE.POPUPEFFDT.retDateStr() + "','dd/mm/yyyy') and prccd='" + PRCCD + "' ");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                                }

                                T_BATCHMST_PRICE TBATCHMSTPRICE = new T_BATCHMST_PRICE();
                                TBATCHMSTPRICE.EMD_NO = 0;
                                TBATCHMSTPRICE.CLCD = CommVar.ClientCode(UNQSNO);
                                TBATCHMSTPRICE.EFFDT = Convert.ToDateTime(VE.POPUPEFFDT);
                                TBATCHMSTPRICE.BARNO = TPHYSTK[i].BARNO;
                                TBATCHMSTPRICE.PRCCD = PRCCD;
                                TBATCHMSTPRICE.RATE = WPRPRATE;
                                TBATCHMSTPRICE.AUTONO = VE.T_PHYSTK_HDR.AUTONO;
                                dbsql = Master_Help.RetModeltoSql(TBATCHMSTPRICE, "A");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    ModelState.Clear();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    ContentFlg = "1";
                    return Content(ContentFlg);
                }
                catch (Exception ex)
                {
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        //public ActionResult UpdatePrice(TransactionPhyStockEntry VE)
        //{
        //    Cn.getQueryString(VE);
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    string dbsql = "";
        //    string[] dbsql1;
        //    OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
        //    OraCon.Open();
        //    OracleCommand OraCmd = OraCon.CreateCommand();
        //    using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
        //    {
        //        OraCmd.Transaction = OraTrans;
        //        try
        //        {
        //            DataTable M_SYSCNFG = salesfunc.GetSyscnfgData(VE.EFFDT.retDateStr());
        //            var TPHYSTK = (from a in VE.TPHYSTK
        //                           select new
        //                           {
        //                               BARNO = a.BARNO,
        //                               RATE = a.RATE
        //                           }).Distinct().ToList();
        //            string ContentFlg = "";
        //            for (int i = 0; i <= TPHYSTK.Count - 1; i++)
        //            {
        //                double WPRATE = 0, RPRATE = 0;

        //                #region wp_rprate calculation
        //                double WPPER = VE.WPPERMANUAL.retDbl();
        //                if (WPPER == 0)
        //                {
        //                    WPPER = M_SYSCNFG.Rows[0]["WPPER"].retDbl();
        //                }
        //                double RPPER = VE.RPPERMANUAL.retDbl();
        //                if (RPPER == 0)
        //                {
        //                    RPPER = M_SYSCNFG.Rows[0]["RPPER"].retDbl();
        //                }
        //                double RATE = TPHYSTK[i].RATE.retDbl();
        //                string WPPRICEGEN = M_SYSCNFG.Rows[0]["WPPRICEGEN"].retStr();
        //                string RPPRICEGEN = M_SYSCNFG.Rows[0]["RPPRICEGEN"].retStr();
        //                if (WPPER != 0)
        //                {
        //                    var wprt = ((RATE.retDbl() * WPPER.retDbl()) / 100) + RATE.retDbl();
        //                    var B_WPRATE = CommFunc.CharmPrice((WPPRICEGEN.retStr() == "" ? "" : WPPRICEGEN.retStr().Substring(0, 2)), wprt.retInt(), (WPPRICEGEN.retStr() == "" ? "" : WPPRICEGEN.retStr().Substring(2)));
        //                    WPRATE = B_WPRATE.retDbl().toRound(2);
        //                }
        //                if (RPPER != 0)
        //                {
        //                    var rprt = ((RATE.retDbl() * RPPER.retDbl()) / 100) + RATE.retDbl();
        //                    var B_RPRATE = CommFunc.CharmPrice((RPPRICEGEN.retStr() == "" ? "" : RPPRICEGEN.retStr().Substring(0, 2)), rprt.retInt(), (RPPRICEGEN.retStr() == "" ? "" : RPPRICEGEN.retStr().Substring(2)));
        //                    RPRATE = B_RPRATE.retDbl().toRound(2);
        //                }
        //                #endregion

        //                for (int j = 0; j <= 1; j++)
        //                {
        //                    string PRCCD = j == 0 ? "WP" : "RP";
        //                    double WPRPRATE = j == 0 ? WPRATE.retDbl() : RPRATE.retDbl();

        //                    if (WPRPRATE != 0)
        //                    {
        //                        bool dataexist = false;
        //                        string barno = TPHYSTK[i].BARNO;
        //                        var chkdata = DB1.T_BATCHMST_PRICE.Where(a => a.BARNO == barno
        //                        && a.EFFDT == VE.EFFDT && a.PRCCD == PRCCD && a.AUTONO != VE.T_PHYSTK_HDR.AUTONO).Select(a => a.BARNO).Distinct().ToList();
        //                        if (chkdata.Count > 0) dataexist = true;

        //                        if (dataexist == false)
        //                        {
        //                            var chk = DB1.T_BATCHMST_PRICE.Where(a => a.BARNO == barno
        //                            && a.EFFDT == VE.EFFDT && a.PRCCD == PRCCD && a.AUTONO == VE.T_PHYSTK_HDR.AUTONO).Select(a => a.BARNO).Distinct().ToList();
        //                            if (chk.Count > 0)
        //                            {
        //                                dbsql = Master_Help.TblUpdt("t_batchmst_price", VE.T_PHYSTK_HDR.AUTONO, "E", "", "barno='" + barno + "' and effdt =to_date('" + VE.EFFDT.retDateStr() + "','dd/mm/yyyy') and prccd='" + PRCCD + "' ");
        //                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                            }

        //                            T_BATCHMST_PRICE TBATCHMSTPRICE = new T_BATCHMST_PRICE();
        //                            TBATCHMSTPRICE.EMD_NO = 0;
        //                            TBATCHMSTPRICE.CLCD = CommVar.ClientCode(UNQSNO);
        //                            TBATCHMSTPRICE.EFFDT = VE.EFFDT;
        //                            TBATCHMSTPRICE.BARNO = TPHYSTK[i].BARNO;
        //                            TBATCHMSTPRICE.PRCCD = PRCCD;
        //                            TBATCHMSTPRICE.RATE = WPRPRATE;
        //                            TBATCHMSTPRICE.AUTONO = VE.T_PHYSTK_HDR.AUTONO;
        //                            dbsql = Master_Help.RetModeltoSql(TBATCHMSTPRICE, "A");
        //                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                        }
        //                    }
        //                }


        //            }

        //            ModelState.Clear();
        //            OraTrans.Commit();
        //            OraTrans.Dispose();
        //            ContentFlg = "1";
        //            return Content(ContentFlg);
        //        }
        //        catch (Exception ex)
        //        {
        //            OraTrans.Rollback();
        //            OraTrans.Dispose();
        //            Cn.SaveException(ex, "");
        //            return Content(ex.Message + ex.InnerException);
        //        }
        //    }
        //}
        public ActionResult Print(TransactionPhyStockEntry VE, FormCollection FC, string DOCNO, string DOC_CD, string DOCDT)
        {
            try
            {
                Cn.getQueryString(VE);
                ReportViewinHtml ind = new ReportViewinHtml();
                ind.DOCCD = DOC_CD;
                ind.FDOCNO = DOCNO;
                ind.TDOCNO = DOCNO;
                ind.FDT = DOCDT;
                ind.TDT = DOCDT;
                ind.MENU_PARA = VE.MENU_PARA;
                if (TempData["printparameter"] != null)
                {
                    TempData.Remove("printparameter");
                }
                TempData["printparameter"] = ind;
                return Content("");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
    }
}