using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using System.Data.Entity.Validation;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace Improvar.Controllers
{
    public class T_StockAdjController : Controller
    {
        Connection Cn = new Connection(); MasterHelp Master_Help = new MasterHelp(); Salesfunc Salesfunc = new Salesfunc(); DataTable GRIDDT = new DataTable();
        T_TXN sl; T_TXNOTH OTH; T_CNTRL_HDR sll; T_CNTRL_HDR_REM TCHR;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        Salesfunc salesfunc = new Salesfunc();
        // GET: T_StockAdj
        public ActionResult T_StockAdj(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    StockAdjustmentsConversionEntry VE = (parkID == "") ? new StockAdjustmentsConversionEntry() : (Improvar.ViewModels.StockAdjustmentsConversionEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.MENU_PARA)
                    {
                        case "ADJ": ViewBag.formname = "Stock Adjustments"; break;
                        case "CNV": ViewBag.formname = "Stock Conversion"; break;
                        case "WAS": ViewBag.formname = "Wastage Generation"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    VE.BARGEN_TYPE = Master_Help.BARGEN_TYPE();
                    VE.DropDown_list_StkType = Master_Help.STK_TYPE();
                    if (op.Length != 0)
                    {
                        string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();
                        switch (VE.MENU_PARA)
                        { case "ADJ": TempData["ADJ"] = XYZ; break; case "CNV": TempData["CNV"] = XYZ; break; case "WAS": TempData["WAS"] = XYZ; break; }

                        VE.IndexKey = (from p in DB.T_TXN
                                       join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       orderby p.DOCDT, p.DOCNO
                                       where (XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM)
                                       select new IndexKey() { Navikey = p.AUTONO }).ToList();

                        if (op == "E" || op == "D" || op == "V")
                        {
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
                            VE.T_TXN = sl;
                            VE.T_TXNOTH = OTH;
                            VE.T_CNTRL_HDR = sll;
                            VE.T_CNTRL_HDR_REM = TCHR;
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {
                                T_TXN TTXN = new T_TXN();
                                TTXN.DOCDT = System.DateTime.Now;
                                VE.T_TXN = TTXN;

                                List<TBATCHDTL> TBATCHDTL_OUT = new List<TBATCHDTL>();
                                for (int i = 0; i < 20; i++)
                                {
                                    TBATCHDTL OUT = new TBATCHDTL();
                                    OUT.SLNO = Convert.ToInt16(i + 1);
                                    //OUT.DropDown_list2 = Master_Help.STOCK_TYPE();
                                    TBATCHDTL_OUT.Add(OUT);
                                }
                                VE.TBATCHDTL_OUT = TBATCHDTL_OUT;

                                List<TBATCHDTL> TBATCHDTL = new List<TBATCHDTL>();
                                for (int i = 1000; i < 1020; i++)
                                {
                                    TBATCHDTL IN = new TBATCHDTL();
                                    IN.SLNO = Convert.ToInt16(i + 1);
                                    //IN.DropDown_list2 = Master_Help.STOCK_TYPE();
                                    TBATCHDTL.Add(IN);
                                }
                                VE.TBATCHDTL = TBATCHDTL;

                                List<STOCK_ADJUSTMENT> STOCK_ADJUSTMENT = new List<STOCK_ADJUSTMENT>();
                                for (int i = 0; i <= VE.DropDown_list_StkType.Count() - 1; i++)
                                {
                                    STOCK_ADJUSTMENT STOCK_ADJ1 = new STOCK_ADJUSTMENT();
                                    STOCK_ADJ1.SLNO = Convert.ToInt16(i + 1);
                                    STOCK_ADJ1.STKTYPE = VE.DropDown_list_StkType[i].text;
                                    STOCK_ADJ1.STKTYPE_VALUE = VE.DropDown_list_StkType[i].value;
                                    STOCK_ADJUSTMENT.Add(STOCK_ADJ1);
                                }
                                VE.STOCK_ADJUSTMENT = STOCK_ADJUSTMENT;


                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;
                            }
                            else
                            {
                                if (VE.UploadDOC != null)
                                {
                                    foreach (var i in VE.UploadDOC)
                                    {
                                        i.DocumentType = Cn.DOC_TYPE();
                                    }
                                }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (StockAdjustmentsConversionEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            List<STOCK_ADJUSTMENT> STOCK_ADJUSTMENT = new List<STOCK_ADJUSTMENT>();
                            for (int i = 0; i <= VE.DropDown_list_StkType.Count() - 1; i++)
                            {
                                STOCK_ADJUSTMENT STOCK_ADJ1 = new STOCK_ADJUSTMENT();
                                STOCK_ADJ1.SLNO = Convert.ToInt16(i + 1);
                                STOCK_ADJ1.STKTYPE = VE.DropDown_list_StkType[i].text;
                                STOCK_ADJ1.STKTYPE_VALUE = VE.DropDown_list_StkType[i].value;
                                STOCK_ADJUSTMENT.Add(STOCK_ADJ1);
                            }
                            VE.STOCK_ADJUSTMENT = STOCK_ADJUSTMENT;
                        }
                        var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                        VE.M_SYSCNFG = MSYSCNFG;
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (VE.T_CNTRL_HDR != null) if (VE.T_CNTRL_HDR.DOCDT != null) docdt = VE.T_CNTRL_HDR.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_TXN.DOCCD, docdt, VE.DefaultAction, VE.T_TXN.DOCNO, VE);
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                StockAdjustmentsConversionEntry VE = new StockAdjustmentsConversionEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public StockAdjustmentsConversionEntry Navigation(StockAdjustmentsConversionEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), CommVar.InvSchema(UNQSNO));
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            sl = new T_TXN(); OTH = new T_TXNOTH(); sll = new T_CNTRL_HDR(); TCHR = new T_CNTRL_HDR_REM();
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
                sl = DB.T_TXN.Find(aa[0].Trim());
                sll = DB.T_CNTRL_HDR.Find(sl.AUTONO);
                if (sl != null)
                {
                    OTH = DB.T_TXNOTH.Find(sl.AUTONO);
                    VE.GodownName = (from z in DBF.M_GODOWN where z.GOCD == sl.GOCD select z.GONM).SingleOrDefault();
                }
                TCHR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), sl.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), sl.AUTONO);
                string Scm = CommVar.CurSchema(UNQSNO);

                #region IN TAB DATA
                string str = "";
                str += "select i.SLNO,i.BARNO,k.ITGRPCD,l.ITGRPNM,l.BARGENTYPE,j.ITCD,k.ITNM,k.STYLENO,k.UOMCD,j.STKTYPE,i.PARTCD,m.PARTNM,m.PRTBARCODE, ";
                str += "j.COLRCD,o.CLRBARCODE,o.COLRNM,j.SIZECD,n.SIZENM,n.SZBARCODE,i.QNTY,i.MTRLJOBCD,p.MTRLJOBNM,p.MTBARCODE ";
                str += "from " + Scm + ".T_BATCHDTL i," + Scm + ".T_TXNDTL j," + Scm + ".M_SITEM k," + Scm + ".M_GROUP l," + Scm + ".M_PARTS m, " + Scm + ".M_SIZE n, " + Scm + ".M_COLOR o, ";
                str += Scm + ".M_MTRLJOBMST p ";
                str += "where i.autono=j.autono and i.txnslno=j.slno and j.ITCD=k.ITCD and k.ITGRPCD=l.ITGRPCD and i.PARTCD=m.PARTCD(+) and j.SIZECD=n.SIZECD(+) and j.COLRCD=o.COLRCD(+) and i.MTRLJOBCD=p.MTRLJOBCD(+) ";
                str += "and i.AUTONO = '" + sl.AUTONO + "' and i.SLNO > 1000 ";
                str += "order by i.SLNO ";
                DataTable tbl_in = Master_Help.SQLquery(str);

                VE.TBATCHDTL = (from DataRow dr in tbl_in.Rows
                                select new TBATCHDTL()
                                {
                                    SLNO = dr["SLNO"].retShort(),
                                    BARNO = dr["BARNO"].retStr(),
                                    ITGRPCD = dr["ITGRPCD"].retStr(),
                                    ITGRPNM = dr["ITGRPNM"].retStr(),
                                    BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                    ITCD = dr["ITCD"].retStr(),
                                    ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                    UOM = dr["UOMCD"].retStr(),
                                    STKTYPE = dr["STKTYPE"].retStr(),
                                    PARTCD = dr["PARTCD"].retStr(),
                                    PARTNM = dr["PARTNM"].retStr(),
                                    PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                    COLRCD = dr["COLRCD"].retStr(),
                                    COLRNM = dr["COLRNM"].retStr(),
                                    CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                    SIZECD = dr["SIZECD"].retStr(),
                                    SIZENM = dr["SIZENM"].retStr(),
                                    SZBARCODE = dr["SZBARCODE"].retStr(),
                                    //WPRATE = dr["WPRATE"].retDbl(),
                                    //RPRATE = dr["RPRATE"].retDbl(),
                                    QNTY = dr["QNTY"].retDbl(),
                                    MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                    MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                    MTBARCODE = dr["MTBARCODE"].retStr(),
                                }).ToList();

                VE.IN_T_QNTY = VE.TBATCHDTL.Sum(a => a.QNTY).retDbl();
                #endregion

                #region OUT TAB DATA
                str = "";
                str += "select i.SLNO,i.BARNO,k.ITGRPCD,l.ITGRPNM,l.BARGENTYPE,j.ITCD,k.ITNM,k.STYLENO,k.UOMCD,j.STKTYPE,i.PARTCD,m.PARTNM,m.PRTBARCODE, ";
                str += "j.COLRCD,o.CLRBARCODE,o.COLRNM,j.SIZECD,n.SIZENM,n.SZBARCODE,i.QNTY,i.MTRLJOBCD,p.MTRLJOBNM,p.MTBARCODE ";
                str += "from " + Scm + ".T_BATCHDTL i," + Scm + ".T_TXNDTL j," + Scm + ".M_SITEM k," + Scm + ".M_GROUP l," + Scm + ".M_PARTS m, " + Scm + ".M_SIZE n, " + Scm + ".M_COLOR o, ";
                str += Scm + ".M_MTRLJOBMST p ";
                str += "where  i.autono=j.autono and i.txnslno=j.slno and j.ITCD=k.ITCD and k.ITGRPCD=l.ITGRPCD and i.PARTCD=m.PARTCD(+) and j.SIZECD=n.SIZECD(+) and j.COLRCD=o.COLRCD(+) and i.MTRLJOBCD=p.MTRLJOBCD(+) ";
                str += "and i.AUTONO = '" + sl.AUTONO + "' and i.SLNO < 1000 ";
                str += "order by i.SLNO ";
                DataTable tbl_out = Master_Help.SQLquery(str);

                VE.TBATCHDTL_OUT = (from DataRow dr in tbl_out.Rows
                                    select new TBATCHDTL()
                                    {
                                        SLNO = dr["SLNO"].retShort(),
                                        BARNO = dr["BARNO"].retStr(),
                                        ITGRPCD = dr["ITGRPCD"].retStr(),
                                        ITGRPNM = dr["ITGRPNM"].retStr(),
                                        BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                        ITCD = dr["ITCD"].retStr(),
                                        ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                        UOM = dr["UOMCD"].retStr(),
                                        STKTYPE = dr["STKTYPE"].retStr(),
                                        PARTCD = dr["PARTCD"].retStr(),
                                        PARTNM = dr["PARTNM"].retStr(),
                                        PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                        COLRCD = dr["COLRCD"].retStr(),
                                        COLRNM = dr["COLRNM"].retStr(),
                                        CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                        SIZECD = dr["SIZECD"].retStr(),
                                        SIZENM = dr["SIZENM"].retStr(),
                                        SZBARCODE = dr["SZBARCODE"].retStr(),
                                        QNTY = dr["QNTY"].retDbl(),
                                        MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                        MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                        MTBARCODE = dr["MTBARCODE"].retStr(),
                                    }).ToList();

                VE.OUT_T_QNTY = VE.TBATCHDTL_OUT.Sum(a => a.QNTY).retDbl();
                #endregion

                if (sll.CANCEL == "Y")
                {
                    VE.CancelRecord = true;
                }
                else
                {
                    VE.CancelRecord = false;
                }
            }
            else
            {

            }
            return VE;
        }
        public ActionResult SearchPannelData(TransactionSaleEntry VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
                Cn.getQueryString(VE);
                List<DocumentType> DocumentType = new List<DocumentType>();
                DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
                string sql = "";

                sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.gocd, c.gonm ";
                sql += "from " + scm + ".t_txn a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_godown c ";
                sql += "where a.autono=b.autono and a.gocd=c.gocd(+) and b.doccd in (" + doccd + ") and ";
                sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
                sql += "order by docdt, docno ";
                DataTable tbl = Master_Help.SQLquery(sql);

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Godown Name" + Cn.GCS() + "AUTO NO";
                for (int j = 0; j <= tbl.Rows.Count - 1; j++)
                {
                    SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["gonm"] + "</b> [" + tbl.Rows[j]["gocd"] + "] </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
                }
                return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "3", "3"));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
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
        public ActionResult GetBarCodeDetailsOutGrid(string val, string Code)
        {
            return BarCodeDetails(val, Code, "SB");
        }
        public ActionResult GetBarCodeDetailsInGrid(string val, string Code)
        {
            return BarCodeDetails(val, Code, "PB");
        }
        public ActionResult BarCodeDetails(string val, string Code, string menupara)
        {
            try
            {
                //sequence MTRLJOBCD/PARTCD/DOCDT/TAXGRPCD/GOCD/PRCCD/ALLMTRLJOBCD
                TransactionOutIssProcess VE = new TransactionOutIssProcess();
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string barnoOrStyle = val.retStr();
                string DOCDT = data[0].retStr();
                string GOCD = data[1].retStr() == "" ? "" : data[1].retStr().retSqlformat();
                string MTRLJOBCD = data[2].retStr() == "" ? "" : data[2].retStr().retSqlformat();

                string str = Master_Help.T_TXN_BARNO_help(barnoOrStyle, menupara, DOCDT, "C001", GOCD, "WP", MTRLJOBCD);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1) return Content(str);

                    string glcd = "";
                    glcd = str.retCompValue("SALGLCD");
                    str += "^GLCD=^" + glcd + Cn.GCS();
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetMaterialDetails(string val)
        {
            try
            {
                var str = Master_Help.MTRLJOBCD_help(val);
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
        public ActionResult AddGridRow(StockAdjustmentsConversionEntry VE, string TABLE)
        {
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                VE.DropDown_list_StkType = Master_Help.STK_TYPE();
                if (TABLE == "_T_StockAdj_IN_TAB_GRID")
                {
                    List<TBATCHDTL> TBATCHDTL = new List<TBATCHDTL>();
                    if (VE.TBATCHDTL == null)
                    {
                        TBATCHDTL TXNDTL = new TBATCHDTL(); TXNDTL.SLNO = 1001; TBATCHDTL.Add(TXNDTL); VE.TBATCHDTL = TBATCHDTL;
                    }
                    else
                    {
                        for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++) { TBATCHDTL TXNDTL = new TBATCHDTL(); TXNDTL = VE.TBATCHDTL[i]; TBATCHDTL.Add(TXNDTL); }
                        TBATCHDTL TXNDTL1 = new TBATCHDTL();
                        TXNDTL1.SLNO = (Convert.ToInt32(VE.TBATCHDTL.Max(a => Convert.ToInt32(a.SLNO))) + 1).retShort();
                        TBATCHDTL.Add(TXNDTL1);
                        VE.TBATCHDTL = TBATCHDTL;
                    }
                    //VE.TBATCHDTL.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); });
                    ModelState.Clear();
                    VE.DefaultView = true;
                    return PartialView("_T_StockAdj_IN_TAB", VE);
                }
                else
                {
                    List<TBATCHDTL> TBATCHDTL_OUT = new List<TBATCHDTL>();
                    if (VE.TBATCHDTL_OUT == null)
                    {
                        TBATCHDTL OUT = new TBATCHDTL(); OUT.SLNO = 1; TBATCHDTL_OUT.Add(OUT); VE.TBATCHDTL_OUT = TBATCHDTL_OUT;
                    }
                    else
                    {
                        for (int i = 0; i <= VE.TBATCHDTL_OUT.Count - 1; i++) { TBATCHDTL OUT = new TBATCHDTL(); OUT = VE.TBATCHDTL_OUT[i]; TBATCHDTL_OUT.Add(OUT); }
                        TBATCHDTL OUT1 = new TBATCHDTL();
                        OUT1.SLNO = (Convert.ToInt32(VE.TBATCHDTL_OUT.Max(a => Convert.ToInt32(a.SLNO))) + 1).retShort();
                        TBATCHDTL_OUT.Add(OUT1);
                        VE.TBATCHDTL_OUT = TBATCHDTL_OUT;
                    }
                    //VE.TBATCHDTL_OUT.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); });
                    ModelState.Clear();
                    VE.DefaultView = true;
                    return PartialView("_T_StockAdj_OUT_TAB", VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteGridRow(StockAdjustmentsConversionEntry VE, string TABLE)
        {
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                VE.DropDown_list_StkType = Master_Help.STK_TYPE();
                if (TABLE == "_T_StockAdj_IN_TAB_GRID")
                {
                    List<TBATCHDTL> TBATCHDTL = new List<TBATCHDTL>();
                    int count = 1000;
                    for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++) { if (VE.TBATCHDTL[i].Checked == false) { count += 1; TBATCHDTL item = new TBATCHDTL(); item = VE.TBATCHDTL[i]; item.SLNO = (count).retShort(); TBATCHDTL.Add(item); } }
                    VE.TBATCHDTL = TBATCHDTL;
                    //VE.TBATCHDTL.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); });
                    ModelState.Clear();
                    VE.DefaultView = true;
                    return PartialView("_T_StockAdj_IN_TAB", VE);
                }
                else
                {
                    List<TBATCHDTL> TBATCHDTL_OUT = new List<TBATCHDTL>();
                    int count = 0;
                    for (int i = 0; i <= VE.TBATCHDTL_OUT.Count - 1; i++) { if (VE.TBATCHDTL_OUT[i].Checked == false) { count += 1; TBATCHDTL item = new TBATCHDTL(); item = VE.TBATCHDTL_OUT[i]; item.SLNO = (count).retShort(); TBATCHDTL_OUT.Add(item); } }
                    VE.TBATCHDTL_OUT = TBATCHDTL_OUT;
                    //VE.TBATCHDTL_OUT.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); });
                    ModelState.Clear();
                    VE.DefaultView = true;
                    return PartialView("_T_StockAdj_OUT_TAB", VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(StockAdjustmentsConversionEntry VE)
        {
            List<UploadDOC> UploadDOC = new List<UploadDOC>();
            if (VE.UploadDOC == null)
            {
                UploadDOC UploadDOC1 = new UploadDOC();
                UploadDOC1.DocumentType = Cn.DOC_TYPE();
                UploadDOC.Add(UploadDOC1);
                VE.UploadDOC = UploadDOC;
            }
            else
            {
                for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                {
                    UploadDOC UploadDOC1 = new UploadDOC();
                    UploadDOC1 = VE.UploadDOC[i];
                    UploadDOC1.DocumentType = Cn.DOC_TYPE();
                    UploadDOC.Add(UploadDOC1);
                }
                UploadDOC UploadDOC2 = new UploadDOC();
                UploadDOC2.DocumentType = Cn.DOC_TYPE();
                UploadDOC.Add(UploadDOC2);
                VE.UploadDOC = UploadDOC;
            }
            VE.DefaultView = true;
            return PartialView("_UPLOADDOCUMENTS", VE);
        }
        public ActionResult DeleteDOCRow(StockAdjustmentsConversionEntry VE)
        {
            List<UploadDOC> UploadDOC = new List<UploadDOC>();
            for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
            {
                if (VE.UploadDOC[i].chk == false)
                {
                    UploadDOC IFSC = new UploadDOC();
                    IFSC = VE.UploadDOC[i];
                    IFSC.DocumentType = Cn.DOC_TYPE();
                    UploadDOC.Add(IFSC);
                }
            }
            VE.UploadDOC = UploadDOC;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_UPLOADDOCUMENTS", VE);
        }
        public Tuple<List<T_BATCH_IMG_HDR>> SaveBarImage(string BarImage, string BARNO, short EMD)
        {
            List<T_BATCH_IMG_HDR> doc = new List<T_BATCH_IMG_HDR>();
            int slno = 0;
            try
            {
                var BarImages = BarImage.retStr().Split((char)179);
                foreach (string image in BarImages)
                {
                    if (image != "")
                    {
                        var imagedes = image.Split('~');
                        T_BATCH_IMG_HDR mdoc = new T_BATCH_IMG_HDR();
                        mdoc.CLCD = CommVar.ClientCode(UNQSNO);
                        mdoc.EMD_NO = EMD;
                        mdoc.SLNO = Convert.ToByte(++slno);
                        mdoc.DOC_CTG = "PRODUCT";
                        var extension = Path.GetExtension(imagedes[0]);
                        mdoc.DOC_FLNAME = BARNO + "_" + slno + extension;
                        mdoc.DOC_DESC = imagedes[1].retStr().Replace('~', ' ');
                        mdoc.BARNO = BARNO;
                        mdoc.DOC_EXTN = extension;
                        doc.Add(mdoc);
                        string topath = CommVar.SaveFolderPath() + "/ItemImages/" + mdoc.DOC_FLNAME;
                        topath = Path.Combine(topath, "");
                        string frompath = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments/" + imagedes[0]);
                        Cn.CopyImage(frompath, topath);
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, BarImage);
            }
            var result = Tuple.Create(doc);
            return result;
        }
        public ActionResult SAVE(FormCollection FC, StockAdjustmentsConversionEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            Cn.getQueryString(VE);
            //Oracle Queries
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "";
            string[] dbsql1;
            string dberrmsg = "";
            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;

            try
            {
                try
                {
                    OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        #region HEADER ENTRY
                        T_TXN TTXN = new T_TXN();
                        T_TXNOTH TTXNOTH = new T_TXNOTH();
                        string DOCPATTERN = ""; string auto_no = ""; string Month = "";
                        TTXN.DOCDT = VE.T_TXN.DOCDT;
                        string Ddate = Convert.ToString(TTXN.DOCDT);
                        TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            TTXN.EMD_NO = 0;
                            TTXN.DOCCD = VE.T_TXN.DOCCD;
                            TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                            DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                            auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
                            TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        }
                        else
                        {
                            TTXN.DOCCD = VE.T_TXN.DOCCD;
                            TTXN.DOCNO = VE.T_TXN.DOCNO;
                            TTXN.AUTONO = VE.T_TXN.AUTONO;
                            Month = VE.T_CNTRL_HDR.MNTHCD;
                            var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == TTXN.AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null) { TTXN.EMD_NO = 0; } else { TTXN.EMD_NO = Convert.ToByte(MAXEMDNO + 1); }
                        }
                        TTXN.GOCD = VE.T_TXN.GOCD;
                        TTXN.DOCTAG = VE.MENU_PARA.Substring(0, 2);
                        TTXN.BARGENTYPE = VE.T_TXN.BARGENTYPE;
                        if (VE.DefaultAction == "E")
                        {
                            dbsql = Master_Help.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                            var comp = DB1.T_BATCHMST.Where(x => x.AUTONO == TTXN.AUTONO).OrderBy(s => s.AUTONO).ToList();
                            foreach (var v in comp)
                            {
                                if (!VE.TBATCHDTL.Where(s => s.BARNO == v.BARNO && s.SLNO == v.SLNO).Any())
                                {
                                    dbsql = Master_Help.TblUpdt("T_BATCH_IMG_HDR_LINK", "", "E", "", "barno='" + v.BARNO + "'");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                    dbsql = Master_Help.TblUpdt("T_BATCH_IMG_HDR", "", "E", "", "barno='" + v.BARNO + "'");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                    dbsql = Master_Help.TblUpdt("t_batchmst", TTXN.AUTONO, "E", "S", "BARNO = '" + v.BARNO + "' and SLNO = '" + v.SLNO + "' ");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                                }
                            }
                            dbsql = Master_Help.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = Master_Help.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = Master_Help.TblUpdt("t_cntrl_hdr_rem", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = Master_Help.TblUpdt("t_cntrl_hdr_doc_dtl", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = Master_Help.TblUpdt("t_cntrl_hdr_doc", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        }

                        TTXNOTH.AUTONO = TTXN.AUTONO;
                        TTXNOTH.EMD_NO = TTXN.EMD_NO;
                        TTXNOTH.CLCD = TTXN.CLCD;
                        TTXNOTH.DTAG = TTXN.DTAG;
                        TTXNOTH.DOCREM = VE.T_TXNOTH.DOCREM;

                        dbsql = Master_Help.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = Master_Help.RetModeltoSql(TTXN, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        dbsql = Master_Help.RetModeltoSql(TTXNOTH);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        #endregion

                        string lbatchini = "";
                        string sql = "select lbatchini from " + CommVar.FinSchema(UNQSNO) + ".m_loca where loccd='" + CommVar.Loccd(UNQSNO) + "' and compcd='" + CommVar.Compcd(UNQSNO) + "'";
                        DataTable dt = Master_Help.SQLquery(sql);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            lbatchini = dt.Rows[0]["lbatchini"].retStr();
                        }

                        string docbarcode = ""; string UNIQNO = salesfunc.retVchrUniqId(TTXN.DOCCD, TTXN.AUTONO);
                        sql = "select doccd,docbarcode from " + CommVar.CurSchema(UNQSNO) + ".m_doctype_bar where doccd='" + TTXN.DOCCD + "'";
                        dt = Master_Help.SQLquery(sql);
                        if (dt != null && dt.Rows.Count > 0) docbarcode = dt.Rows[0]["docbarcode"].retStr();

                        // SAVE T_CNTRL_HDR_UNIQNO

                        if (VE.DefaultAction == "A")
                        {
                            T_CNTRL_HDR_UNIQNO TCHUNIQNO = new T_CNTRL_HDR_UNIQNO();
                            TCHUNIQNO.CLCD = TTXN.CLCD;
                            TCHUNIQNO.AUTONO = TTXN.AUTONO;
                            TCHUNIQNO.UNIQNO = UNIQNO;
                            dbsql = Master_Help.RetModeltoSql(TCHUNIQNO);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        }
                        //END T_CNTRL_HDR_UNIQNO 

                        #region IN TAB ENTRY
                        double IN_TOTAL_QNTY = 0;
                        if (VE.TBATCHDTL != null)
                        {
                            var GROUPDATA_IN = (from a in VE.TBATCHDTL
                                                where a.BARNO.retStr() != ""
                                                group a by new
                                                {
                                                    a.ITCD,
                                                    a.STKTYPE,
                                                    a.PARTCD,
                                                    a.COLRCD,
                                                    a.SIZECD,
                                                    a.MTRLJOBCD
                                                }
                                                into P
                                                select new TTXNDTL()
                                                {
                                                    ITCD = P.Key.ITCD,
                                                    STKTYPE = P.Key.STKTYPE,
                                                    PARTCD = P.Key.PARTCD,
                                                    COLRCD = P.Key.COLRCD,
                                                    SIZECD = P.Key.SIZECD,
                                                    QNTY = P.Sum(a => a.QNTY),
                                                    MTRLJOBCD = P.Key.MTRLJOBCD,
                                                    SLNO = 0,
                                                }
                                              ).ToList();
                            int COUNTER_IN = 1000;
                            for (int i = 0; i <= GROUPDATA_IN.Count - 1; i++)
                            {
                                if (GROUPDATA_IN[i].ITCD.retStr() != "" && GROUPDATA_IN[i].QNTY.retDbl() != 0)
                                {
                                    COUNTER_IN = COUNTER_IN + 1;
                                    GROUPDATA_IN[i].SLNO = COUNTER_IN.retShort();
                                    T_TXNDTL TTXNDTL = new T_TXNDTL();
                                    TTXNDTL.CLCD = TTXN.CLCD;
                                    TTXNDTL.EMD_NO = TTXN.EMD_NO;
                                    TTXNDTL.DTAG = TTXN.DTAG;
                                    TTXNDTL.AUTONO = TTXN.AUTONO;
                                    TTXNDTL.STKDRCR = "D";
                                    TTXNDTL.GOCD = TTXN.GOCD;
                                    TTXNDTL.SLNO = GROUPDATA_IN[i].SLNO;
                                    TTXNDTL.ITCD = GROUPDATA_IN[i].ITCD;
                                    TTXNDTL.STKTYPE = GROUPDATA_IN[i].STKTYPE;
                                    TTXNDTL.PARTCD = GROUPDATA_IN[i].PARTCD;
                                    TTXNDTL.COLRCD = GROUPDATA_IN[i].COLRCD;
                                    TTXNDTL.SIZECD = GROUPDATA_IN[i].SIZECD;
                                    TTXNDTL.QNTY = GROUPDATA_IN[i].QNTY;
                                    TTXNDTL.MTRLJOBCD = GROUPDATA_IN[i].MTRLJOBCD;

                                    dbsql = Master_Help.RetModeltoSql(TTXNDTL);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                            bool recoexist = false;
                            if (VE.TBATCHDTL != null && VE.TBATCHDTL.Count > 0)
                            {
                                for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                                {
                                    if (VE.TBATCHDTL[i].ITCD != null && VE.TBATCHDTL[i].QNTY != 0)
                                    {

                                        bool flagbatch = false;
                                        string barno = "";
                                        if (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E")
                                        {
                                            barno = salesfunc.TranBarcodeGenerate(TTXN.DOCCD, lbatchini, docbarcode, UNIQNO, (VE.TBATCHDTL[i].SLNO));
                                            flagbatch = true;
                                        }
                                        else
                                        {
                                            barno = salesfunc.GenerateBARNO(VE.TBATCHDTL[i].ITCD, VE.TBATCHDTL[i].CLRBARCODE, VE.TBATCHDTL[i].SZBARCODE);
                                            sql = "  select ITCD,BARNO from " + CommVar.CurSchema(UNQSNO) + ".M_SITEM_BARCODE where barno='" + barno + "'";
                                            dt = Master_Help.SQLquery(sql);
                                            if (dt.Rows.Count == 0)
                                            {
                                                dberrmsg = "Barno:" + barno + " not found at Item master at rowno:" + VE.TBATCHDTL[i].SLNO + " and itcd=" + VE.TBATCHDTL[i].ITCD; goto dbnotsave;
                                            }
                                            sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                            OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                            if (recoexist == false) flagbatch = true;
                                        }

                                        //checking barno exist or not
                                        string Action = "", SqlCondition = "";
                                        if (VE.DefaultAction == "A")
                                        {
                                            Action = VE.DefaultAction;
                                        }
                                        else
                                        {
                                            sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where autono='" + TTXN.AUTONO + "' and slno = " + VE.TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
                                            OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                            if (recoexist == true)
                                            {
                                                Action = "E";
                                                SqlCondition = "autono = '" + TTXN.AUTONO + "' and slno = " + VE.TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
                                                flagbatch = true;
                                                barno = VE.TBATCHDTL[i].BARNO;
                                            }
                                            else
                                            {
                                                Action = "A";
                                            }
                                        }
                                        if (flagbatch == true)
                                        {
                                            T_BATCHMST TBATCHMST = new T_BATCHMST();
                                            TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                            TBATCHMST.CLCD = TTXN.CLCD;
                                            TBATCHMST.DTAG = TTXN.DTAG;
                                            TBATCHMST.TTAG = TTXN.TTAG;
                                            TBATCHMST.AUTONO = TTXN.AUTONO;
                                            TBATCHMST.SLNO = VE.TBATCHDTL[i].SLNO;
                                            TBATCHMST.BARNO = barno;
                                            TBATCHMST.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
                                            TBATCHMST.STKTYPE = VE.TBATCHDTL[i].STKTYPE;
                                            TBATCHMST.ITCD = VE.TBATCHDTL[i].ITCD;
                                            TBATCHMST.PARTCD = VE.TBATCHDTL[i].PARTCD;
                                            TBATCHMST.SIZECD = VE.TBATCHDTL[i].SIZECD;
                                            TBATCHMST.COLRCD = VE.TBATCHDTL[i].COLRCD;
                                            TBATCHMST.QNTY = VE.TBATCHDTL[i].QNTY;
                                            TBATCHMST.WPRATE = VE.TBATCHDTL[i].WPRATE;
                                            TBATCHMST.RPRATE = VE.TBATCHDTL[i].RPRATE;

                                            dbsql = Master_Help.RetModeltoSql(TBATCHMST, Action, "", SqlCondition);
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                            if (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E")
                                            {

                                                if (VE.TBATCHDTL[i].BarImages.retStr() != "")
                                                {
                                                    dbsql = Master_Help.TblUpdt("T_BATCH_IMG_HDR", "", "E", "", "barno='" + barno + "'");
                                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                                    dbsql = Master_Help.TblUpdt("T_BATCH_IMG_HDR_LINK", "", "E", "", "barno='" + barno + "'");
                                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                                    var barimg = SaveBarImage(VE.TBATCHDTL[i].BarImages, barno, TTXN.EMD_NO.retShort());
                                                    dbsql = Master_Help.RetModeltoSql(barimg);

                                                    for (int tr = 0; tr <= barimg.Item1.Count - 1; tr++)
                                                    {
                                                        dbsql = Master_Help.RetModeltoSql(barimg.Item1[tr]);
                                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                                    }
                                                    var disntImgHdr = barimg.Item1.GroupBy(u => u.BARNO).Select(r => r.First()).ToList();
                                                    foreach (var imgbar in disntImgHdr)
                                                    {
                                                        T_BATCH_IMG_HDR_LINK m_batchImglink = new T_BATCH_IMG_HDR_LINK();
                                                        m_batchImglink.CLCD = TTXN.CLCD;
                                                        m_batchImglink.EMD_NO = TTXN.EMD_NO;
                                                        m_batchImglink.BARNO = imgbar.BARNO;
                                                        m_batchImglink.MAINBARNO = imgbar.BARNO;

                                                        dbsql = Master_Help.RetModeltoSql(m_batchImglink);
                                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                                    }
                                                }

                                            }
                                        }
                                        var txnslno = GROUPDATA_IN.Where(a => a.ITCD == VE.TBATCHDTL[i].ITCD &&
                                                    a.STKTYPE == VE.TBATCHDTL[i].STKTYPE &&
                                                    a.PARTCD == VE.TBATCHDTL[i].PARTCD &&
                                                    a.COLRCD == VE.TBATCHDTL[i].COLRCD &&
                                                    a.SIZECD == VE.TBATCHDTL[i].SIZECD &&
                                                    a.MTRLJOBCD == VE.TBATCHDTL[i].MTRLJOBCD).Select(b => b.SLNO).SingleOrDefault();

                                        T_BATCHDTL TBATCHDTL = new T_BATCHDTL();
                                        TBATCHDTL.EMD_NO = TTXN.EMD_NO;
                                        TBATCHDTL.CLCD = TTXN.CLCD;
                                        TBATCHDTL.DTAG = TTXN.DTAG;
                                        TBATCHDTL.TTAG = TTXN.TTAG;
                                        TBATCHDTL.AUTONO = TTXN.AUTONO;
                                        TBATCHDTL.SLNO = VE.TBATCHDTL[i].SLNO;
                                        TBATCHDTL.BARNO = barno;
                                        TBATCHDTL.TXNSLNO = txnslno;
                                        TBATCHDTL.STKDRCR = "D";
                                        TBATCHDTL.GOCD = TTXN.GOCD;
                                        if (!string.IsNullOrEmpty(VE.TBATCHDTL[i].MTRLJOBCD))
                                        {
                                            TBATCHDTL.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
                                        }
                                        else
                                        {
                                            TBATCHDTL.MTRLJOBCD = VE.MENU_PARA == "WAS" ? "WA" : "FS";
                                        }
                                        TBATCHDTL.PARTCD = VE.TBATCHDTL[i].COLRCD;
                                        TBATCHDTL.PARTCD = VE.TBATCHDTL[i].PARTCD;
                                        TBATCHDTL.QNTY = VE.TBATCHDTL[i].QNTY;
                                        TBATCHDTL.STKTYPE = VE.TBATCHDTL[i].STKTYPE;
                                        IN_TOTAL_QNTY = IN_TOTAL_QNTY + VE.TBATCHDTL[i].QNTY.retDbl();

                                        dbsql = Master_Help.RetModeltoSql(TBATCHDTL);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        #endregion
                        #region OUT TAB ENTRY
                        double OUT_TOTAL_QNTY = 0;
                        if (VE.TBATCHDTL_OUT != null)
                        {
                            var GROUPDATA_OUT = (from a in VE.TBATCHDTL_OUT
                                                 where a.BARNO.retStr() != ""
                                                 group a by new
                                                 {
                                                     a.ITCD,
                                                     a.STKTYPE,
                                                     a.PARTCD,
                                                     a.COLRCD,
                                                     a.SIZECD,
                                                     a.MTRLJOBCD
                                                 }
                                                into P
                                                 select new TTXNDTL()
                                                 {
                                                     ITCD = P.Key.ITCD,
                                                     STKTYPE = P.Key.STKTYPE,
                                                     PARTCD = P.Key.PARTCD,
                                                     COLRCD = P.Key.COLRCD,
                                                     SIZECD = P.Key.SIZECD,
                                                     QNTY = P.Sum(a => a.QNTY),
                                                     MTRLJOBCD = P.Key.MTRLJOBCD,
                                                     SLNO = 0,
                                                 }
                                              ).ToList();
                            int COUNTER_OUT = 0;
                            for (int i = 0; i <= GROUPDATA_OUT.Count - 1; i++)
                            {
                                if (GROUPDATA_OUT[i].ITCD.retStr() != "" && GROUPDATA_OUT[i].QNTY.retDbl() != 0)
                                {
                                    COUNTER_OUT = COUNTER_OUT + 1;
                                    GROUPDATA_OUT[i].SLNO = COUNTER_OUT.retShort();
                                    T_TXNDTL TTXNDTL = new T_TXNDTL();
                                    TTXNDTL.CLCD = TTXN.CLCD;
                                    TTXNDTL.EMD_NO = TTXN.EMD_NO;
                                    TTXNDTL.DTAG = TTXN.DTAG;
                                    TTXNDTL.AUTONO = TTXN.AUTONO;
                                    TTXNDTL.STKDRCR = "C";
                                    TTXNDTL.GOCD = TTXN.GOCD;
                                    TTXNDTL.SLNO = GROUPDATA_OUT[i].SLNO;
                                    TTXNDTL.ITCD = GROUPDATA_OUT[i].ITCD;
                                    TTXNDTL.STKTYPE = GROUPDATA_OUT[i].STKTYPE;
                                    TTXNDTL.PARTCD = GROUPDATA_OUT[i].PARTCD;
                                    TTXNDTL.COLRCD = GROUPDATA_OUT[i].COLRCD;
                                    TTXNDTL.SIZECD = GROUPDATA_OUT[i].SIZECD;
                                    TTXNDTL.QNTY = GROUPDATA_OUT[i].QNTY;
                                    TTXNDTL.MTRLJOBCD = GROUPDATA_OUT[i].MTRLJOBCD;
                                    dbsql = Master_Help.RetModeltoSql(TTXNDTL);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                            bool recoexist = false;
                            if (VE.TBATCHDTL_OUT != null && VE.TBATCHDTL_OUT.Count > 0)
                            {
                                for (int i = 0; i <= VE.TBATCHDTL_OUT.Count - 1; i++)
                                {
                                    if (VE.TBATCHDTL_OUT[i].ITCD != null && VE.TBATCHDTL_OUT[i].QNTY != 0)
                                    {

                                        bool flagbatch = false;
                                        string barno = "";
                                        //if (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL_OUT[i].BARGENTYPE == "E")
                                        //{
                                        //    barno = salesfunc.TranBarcodeGenerate(TTXN.DOCCD, lbatchini, docbarcode, UNIQNO, (VE.TBATCHDTL_OUT[i].SLNO));
                                        //    flagbatch = true;
                                        //}
                                        //else
                                        //{
                                        barno = salesfunc.GenerateBARNO(VE.TBATCHDTL_OUT[i].ITCD, VE.TBATCHDTL_OUT[i].CLRBARCODE, VE.TBATCHDTL_OUT[i].SZBARCODE);
                                        sql = "  select ITCD,BARNO from " + CommVar.CurSchema(UNQSNO) + ".M_SITEM_BARCODE where barno='" + barno + "'";
                                        dt = Master_Help.SQLquery(sql);
                                        if (dt.Rows.Count == 0)
                                        {
                                            dberrmsg = "Barno:" + barno + " not found at Item master at rowno:" + VE.TBATCHDTL_OUT[i].SLNO + " and itcd=" + VE.TBATCHDTL_OUT[i].ITCD; goto dbnotsave;
                                        }
                                        sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                        OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                        if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                        if (recoexist == false) flagbatch = true;
                                        //}

                                        //checking barno exist or not
                                        string Action = "", SqlCondition = "";
                                        if (VE.DefaultAction == "A")
                                        {
                                            Action = VE.DefaultAction;
                                        }
                                        else
                                        {
                                            sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where autono='" + TTXN.AUTONO + "' and slno = " + VE.TBATCHDTL_OUT[i].SLNO + " and barno='" + barno + "' ";
                                            OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                            if (recoexist == true)
                                            {
                                                Action = "E";
                                                SqlCondition = "autono = '" + TTXN.AUTONO + "' and slno = " + VE.TBATCHDTL_OUT[i].SLNO + " and barno='" + barno + "' ";
                                                flagbatch = true;
                                                barno = VE.TBATCHDTL_OUT[i].BARNO;
                                            }
                                            else
                                            {
                                                Action = "A";
                                            }
                                        }
                                        if (flagbatch == true)
                                        {
                                            T_BATCHMST TBATCHMST = new T_BATCHMST();
                                            TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                            TBATCHMST.CLCD = TTXN.CLCD;
                                            TBATCHMST.DTAG = TTXN.DTAG;
                                            TBATCHMST.TTAG = TTXN.TTAG;
                                            TBATCHMST.AUTONO = TTXN.AUTONO;
                                            TBATCHMST.SLNO = VE.TBATCHDTL_OUT[i].SLNO;
                                            TBATCHMST.BARNO = barno;
                                            TBATCHMST.MTRLJOBCD = VE.TBATCHDTL_OUT[i].MTRLJOBCD;
                                            TBATCHMST.STKTYPE = VE.TBATCHDTL_OUT[i].STKTYPE;
                                            TBATCHMST.ITCD = VE.TBATCHDTL_OUT[i].ITCD;
                                            TBATCHMST.PARTCD = VE.TBATCHDTL_OUT[i].PARTCD;
                                            TBATCHMST.SIZECD = VE.TBATCHDTL_OUT[i].SIZECD;
                                            TBATCHMST.COLRCD = VE.TBATCHDTL_OUT[i].COLRCD;
                                            TBATCHMST.QNTY = VE.TBATCHDTL_OUT[i].QNTY;
                                            TBATCHMST.WPRATE = VE.TBATCHDTL_OUT[i].WPRATE;
                                            TBATCHMST.RPRATE = VE.TBATCHDTL_OUT[i].RPRATE;

                                            dbsql = Master_Help.RetModeltoSql(TBATCHMST, Action, "", SqlCondition);
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                            if (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL_OUT[i].BARGENTYPE == "E")
                                            {

                                                if (VE.TBATCHDTL_OUT[i].BarImages.retStr() != "")
                                                {
                                                    dbsql = Master_Help.TblUpdt("T_BATCH_IMG_HDR", "", "E", "", "barno='" + barno + "'");
                                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                                    dbsql = Master_Help.TblUpdt("T_BATCH_IMG_HDR_LINK", "", "E", "", "barno='" + barno + "'");
                                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                                    var barimg = SaveBarImage(VE.TBATCHDTL_OUT[i].BarImages, barno, TTXN.EMD_NO.retShort());
                                                    dbsql = Master_Help.RetModeltoSql(barimg);

                                                    for (int tr = 0; tr <= barimg.Item1.Count - 1; tr++)
                                                    {
                                                        dbsql = Master_Help.RetModeltoSql(barimg.Item1[tr]);
                                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                                    }
                                                    var disntImgHdr = barimg.Item1.GroupBy(u => u.BARNO).Select(r => r.First()).ToList();
                                                    foreach (var imgbar in disntImgHdr)
                                                    {
                                                        T_BATCH_IMG_HDR_LINK m_batchImglink = new T_BATCH_IMG_HDR_LINK();
                                                        m_batchImglink.CLCD = TTXN.CLCD;
                                                        m_batchImglink.EMD_NO = TTXN.EMD_NO;
                                                        m_batchImglink.BARNO = imgbar.BARNO;
                                                        m_batchImglink.MAINBARNO = imgbar.BARNO;

                                                        dbsql = Master_Help.RetModeltoSql(m_batchImglink);
                                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                                    }
                                                }

                                            }
                                        }
                                        var txnslno = GROUPDATA_OUT.Where(a => a.ITCD == VE.TBATCHDTL_OUT[i].ITCD &&
                                                    a.STKTYPE == VE.TBATCHDTL_OUT[i].STKTYPE &&
                                                    a.PARTCD == VE.TBATCHDTL_OUT[i].PARTCD &&
                                                    a.COLRCD == VE.TBATCHDTL_OUT[i].COLRCD &&
                                                    a.SIZECD == VE.TBATCHDTL_OUT[i].SIZECD &&
                                                    a.MTRLJOBCD == VE.TBATCHDTL_OUT[i].MTRLJOBCD).Select(b => b.SLNO).SingleOrDefault();

                                        T_BATCHDTL TBATCHDTL_OUT = new T_BATCHDTL();
                                        TBATCHDTL_OUT.EMD_NO = TTXN.EMD_NO;
                                        TBATCHDTL_OUT.CLCD = TTXN.CLCD;
                                        TBATCHDTL_OUT.DTAG = TTXN.DTAG;
                                        TBATCHDTL_OUT.TTAG = TTXN.TTAG;
                                        TBATCHDTL_OUT.AUTONO = TTXN.AUTONO;
                                        TBATCHDTL_OUT.SLNO = VE.TBATCHDTL_OUT[i].SLNO;
                                        TBATCHDTL_OUT.TXNSLNO = txnslno;
                                        TBATCHDTL_OUT.BARNO = barno;
                                        TBATCHDTL_OUT.STKDRCR = "C";
                                        TBATCHDTL_OUT.GOCD = TTXN.GOCD;
                                        if (!string.IsNullOrEmpty(VE.TBATCHDTL_OUT[i].MTRLJOBCD))
                                        {
                                            TBATCHDTL_OUT.MTRLJOBCD = VE.TBATCHDTL_OUT[i].MTRLJOBCD;
                                        }
                                        else
                                        {
                                            TBATCHDTL_OUT.MTRLJOBCD = VE.MENU_PARA == "WAS" ? "WA" : "FS";
                                        }
                                        TBATCHDTL_OUT.PARTCD = VE.TBATCHDTL_OUT[i].COLRCD;
                                        TBATCHDTL_OUT.PARTCD = VE.TBATCHDTL_OUT[i].PARTCD;
                                        TBATCHDTL_OUT.QNTY = VE.TBATCHDTL_OUT[i].QNTY;
                                        TBATCHDTL_OUT.STKTYPE = VE.TBATCHDTL_OUT[i].STKTYPE;
                                        OUT_TOTAL_QNTY = OUT_TOTAL_QNTY + VE.TBATCHDTL_OUT[i].QNTY.retDbl();

                                        dbsql = Master_Help.RetModeltoSql(TBATCHDTL_OUT);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        #endregion
                        #region COMMON DATA ENTRY
                        if (VE.UploadDOC != null)// add
                        {
                            var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TTXN.AUTONO, TTXN.EMD_NO.Value);
                            if (img.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= img.Item1.Count - 1; tr++)
                                {
                                    dbsql = Master_Help.RetModeltoSql(img.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                                for (int tr = 0; tr <= img.Item2.Count - 1; tr++)
                                {
                                    dbsql = Master_Help.RetModeltoSql(img.Item2[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                        {
                            var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TTXN.AUTONO, TTXN.CLCD, TTXN.EMD_NO.Value);
                            if (NOTE.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
                                {
                                    dbsql = Master_Help.RetModeltoSql(NOTE.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion
                        if (VE.MENU_PARA == "CNV")
                        {
                            if (IN_TOTAL_QNTY != OUT_TOTAL_QNTY)
                            {
                                OraTrans.Rollback();
                                OraTrans.Dispose();
                                return Content("Total Quantity of Out Tab and Total Quantity of In Tab Doesn't Match, Total Quantity must be Equal ! Please Maintain the Ratio !!");
                            }
                        }


                        DB.SaveChanges();
                        ModelState.Clear();
                        OraTrans.Commit();
                        OraTrans.Dispose();
                        string ContentFlg = "";
                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1" + "<br/><br/>Voucher Number : " + TTXN.DOCCD + TTXN.DOCNO + "~" + TTXN.AUTONO;
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        return Content(ContentFlg);
                    }
                    else if (VE.DefaultAction == "V")
                    {
                        dbsql = Master_Help.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        if (VE.TBATCHDTL != null)
                        {
                            foreach (var v in VE.TBATCHDTL)
                            {
                                var IsTransactionFound = salesfunc.IsTransactionFound("", v.BARNO.retSqlformat(), VE.T_TXN.AUTONO.retSqlformat());
                                if (IsTransactionFound != "")
                                {
                                    dberrmsg = "We cant delete this Bill. Transaction found at " + IsTransactionFound; goto dbnotsave;
                                }
                                else if (IsTransactionFound == "")
                                {
                                    dbsql = Master_Help.TblUpdt("T_BATCH_IMG_HDR", "", "D", "", "barno='" + v.BARNO + "'");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                    dbsql = Master_Help.TblUpdt("T_BATCH_IMG_HDR_LINK", "", "D", "", "barno='" + v.BARNO + "'");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                    dbsql = Master_Help.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D", "", "barno='" + v.BARNO + "' and autono='" + VE.T_TXN.AUTONO + "'");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                                }
                            }
                        }
                        if (VE.TBATCHDTL_OUT != null)
                        {
                            foreach (var v in VE.TBATCHDTL_OUT)
                            {
                                var IsTransactionFound = salesfunc.IsTransactionFound("", v.BARNO.retSqlformat(), VE.T_TXN.AUTONO.retSqlformat());
                                if (IsTransactionFound != "")
                                {
                                    dberrmsg = "We cant delete this Bill. Transaction found at " + IsTransactionFound; goto dbnotsave;
                                }
                                else if (IsTransactionFound == "")
                                {
                                    dbsql = Master_Help.TblUpdt("T_BATCH_IMG_HDR", "", "D", "", "barno='" + v.BARNO + "'");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                    dbsql = Master_Help.TblUpdt("T_BATCH_IMG_HDR_LINK", "", "D", "", "barno='" + v.BARNO + "'");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                    dbsql = Master_Help.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D", "", "barno='" + v.BARNO + "' and autono='" + VE.T_TXN.AUTONO + "'");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                                }
                            }
                        }
                        dbsql = Master_Help.TblUpdt("t_txndtl", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = Master_Help.TblUpdt("t_cntrl_hdr_uniqno", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = Master_Help.TblUpdt("t_txnoth", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = Master_Help.TblUpdt("t_txn", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = Master_Help.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = Master_Help.TblUpdt("t_cntrl_hdr_doc", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = Master_Help.TblUpdt("t_cntrl_hdr_rem", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = Master_Help.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                        ModelState.Clear();
                        OraTrans.Commit();
                        OraTrans.Dispose();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                dbnotsave:;
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    return Content(dberrmsg);
                }
                catch (DbEntityValidationException ex)
                {
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    // Retrieve the error messages as a list of strings.
                    var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    var fullErrorMessage = string.Join("&quot;", errorMessages);

                    // Combine the original exception message with the new one.
                    var exceptionMessage = string.Concat(ex.Message, "<br/> &quot; The validation errors are: &quot;", fullErrorMessage);

                    // Throw a new DbEntityValidationException with the improved exception message.
                    throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
                }
            }
            catch (Exception ex)
            {
                OraTrans.Rollback();
                OraTrans.Dispose();
                ModelState.Clear();
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult Print(StockAdjustmentsConversionEntry TSP)
        {
            try
            {
                Cn.getQueryString(TSP);
                Rep_Doc_Print repDoc = new Rep_Doc_Print();
                string capname, reptype = "";
                if (TSP.MENU_PARA == "CNV") capname = "Stock Conversion"; else capname = "Stock Adjustment";
                capname += " Printing"; reptype = "";
                repDoc.DOCCD = TSP.T_TXN.DOCCD;
                repDoc.FDOCNO = TSP.T_TXN.DOCNO;
                repDoc.TDOCNO = TSP.T_TXN.DOCNO;
                repDoc.FDT = TSP.T_TXN.DOCDT.ToString().retDateStr();
                repDoc.TDT = TSP.T_TXN.DOCDT.ToString().retDateStr();
                repDoc.AskSlCd = true;
                repDoc.CaptionName = capname;
                repDoc.ActionName = "Rep_StkAdj_Print";
                repDoc.RepType = reptype;
                repDoc.OtherPara = TSP.MENU_PARA + "," + TSP.T_TXN.GOCD;
                if (TempData["printparameter"] != null)
                {
                    TempData.Remove("printparameter");
                }
                TempData["printparameter"] = repDoc;
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