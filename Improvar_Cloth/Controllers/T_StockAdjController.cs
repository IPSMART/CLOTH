using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using System.Data.Entity.Validation;

namespace Improvar.Controllers
{
    public class T_StockAdjController : Controller
    {
        Connection Cn = new Connection(); MasterHelp Master_Help = new MasterHelp(); Salesfunc Salesfunc = new Salesfunc(); DataTable GRIDDT = new DataTable();
        T_TXN sl; T_TXNOTH OTH; T_CNTRL_HDR sll; T_CNTRL_HDR_REM TCHR;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
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

                                List<TTXNDTL_OUT> TTXNDTL_OUT = new List<TTXNDTL_OUT>();
                                for (int i = 0; i < 20; i++)
                                {
                                    TTXNDTL_OUT OUT = new TTXNDTL_OUT();
                                    OUT.SLNO = Convert.ToInt16(i + 1);
                                    OUT.DropDown_list2 = Master_Help.STOCK_TYPE();
                                    TTXNDTL_OUT.Add(OUT);
                                }
                                VE.TTXNDTL_OUT = TTXNDTL_OUT;

                                List<TTXNDTL> TTXNDTL = new List<TTXNDTL>();
                                for (int i = 0; i < 20; i++)
                                {
                                    TTXNDTL IN = new TTXNDTL();
                                    IN.SLNO = Convert.ToInt16(i + 1);
                                    IN.DropDown_list2 = Master_Help.STOCK_TYPE();
                                    TTXNDTL.Add(IN);
                                }
                                VE.TTXNDTL = TTXNDTL;

                                List<STOCK_ADJUSTMENT> STOCK_ADJUSTMENT = new List<STOCK_ADJUSTMENT>();
                                var stktype = Master_Help.STOCK_TYPE();
                                for (int i = 0; i < stktype.Count() - 1; i++)
                                {
                                    STOCK_ADJUSTMENT STOCK_ADJ1 = new STOCK_ADJUSTMENT();
                                    STOCK_ADJ1.SLNO = Convert.ToInt16(i + 1);
                                    STOCK_ADJ1.STKTYPE = stktype[i].text;
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
                            //List<STOCK_ADJUSTMENT> STOCK_ADJUSTMENT = new List<STOCK_ADJUSTMENT>();
                            //STOCK_ADJUSTMENT STOCK_ADJ1 = new STOCK_ADJUSTMENT();
                            //STOCK_ADJ1.SLNO = 1;
                            //STOCK_ADJ1.STKTYPE = "";
                            //STOCK_ADJUSTMENT.Add(STOCK_ADJ1);
                            //STOCK_ADJUSTMENT STOCK_ADJ2 = new STOCK_ADJUSTMENT();
                            //STOCK_ADJ2.SLNO = 2;
                            //STOCK_ADJ2.STKTYPE = "Raka";
                            //STOCK_ADJUSTMENT.Add(STOCK_ADJ2);
                            //STOCK_ADJUSTMENT STOCK_ADJ3 = new STOCK_ADJUSTMENT();
                            //STOCK_ADJ3.SLNO = 3;
                            //STOCK_ADJ3.STKTYPE = "Loose";
                            //STOCK_ADJUSTMENT.Add(STOCK_ADJ3);
                            //STOCK_ADJUSTMENT STOCK_ADJ4 = new STOCK_ADJUSTMENT();
                            //STOCK_ADJ4.SLNO = 4;
                            //STOCK_ADJ4.STKTYPE = "Destroy";
                            //STOCK_ADJUSTMENT.Add(STOCK_ADJ4);
                            //VE.STOCK_ADJUSTMENT = STOCK_ADJUSTMENT;

                            List<STOCK_ADJUSTMENT> STOCK_ADJUSTMENT = new List<STOCK_ADJUSTMENT>();
                            var stktype = Master_Help.STOCK_TYPE();
                            for (int i = 0; i < stktype.Count() - 1; i++)
                            {
                                STOCK_ADJUSTMENT STOCK_ADJ1 = new STOCK_ADJUSTMENT();
                                STOCK_ADJ1.SLNO = Convert.ToInt16(i + 1);
                                STOCK_ADJ1.STKTYPE = stktype[i].text;
                                STOCK_ADJUSTMENT.Add(STOCK_ADJ1);
                            }
                            VE.STOCK_ADJUSTMENT = STOCK_ADJUSTMENT;
                        }
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
                    VE.GodownName = (from z in DB.M_GODOWN where z.GOCD == sl.GOCD select z.GONM).SingleOrDefault();
                }
                TCHR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), sl.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), sl.AUTONO);

                #region IN TAB DATA
                double? TOTAL_BOXES = 0; double? TOTAL_QNTY = 0; double? TOTAL_SETS = 0;
                VE.TTXNDTL = (from i in DB.T_TXNDTL
                              join j in DB.M_SITEM on i.ITCD equals j.ITCD into x
                              from j in x.DefaultIfEmpty()
                              join k in DB.M_SIZE on i.SIZECD equals k.SIZECD into y
                              from k in y.DefaultIfEmpty()
                              join l in DB.M_COLOR on i.COLRCD equals l.COLRCD into z
                              from l in z.DefaultIfEmpty()
                              join m in DB.M_PARTS on i.PARTCD equals m.PARTCD into A
                              from m in A.DefaultIfEmpty()
                              where i.AUTONO == sl.AUTONO && i.SLNO > 1000
                              select new TTXNDTL()
                              {
                                  AUTONO = i.AUTONO,
                                  SLNO = i.SLNO,
                                  EMD_NO = i.EMD_NO,
                                  CLCD = i.CLCD,
                                  DTAG = i.DTAG,
                                  TTAG = i.TTAG,
                                  STKDRCR = i.STKDRCR,
                                  STKTYPE = i.STKTYPE,
                                  ITCD = i.ITCD,
                                  ITNM = j.ITNM,
                                  STYLENO = j.STYLENO,
                                  PARTCD = i.PARTCD,
                                  PARTNM = m.PARTNM,
                                  //PCSBOX = j.PCSPERBOX,
                                  //PCSPERSET = j.PCSPERSET,
                                  UOM = j.UOMCD,
                                  SIZECD = i.SIZECD,
                                  //ALL_SIZE = i.SIZECD,
                                  SIZENM = k.SIZENM,
                                  COLRCD = i.COLRCD,
                                  COLRNM = l.COLRNM,
                                  QNTY = i.QNTY,
                                  MTRLJOBCD = i.MTRLJOBCD
                              }).OrderBy(s => s.SLNO).ToList();

                //if (VE.TTXNDTL.Count > 0)
                //{
                //    foreach (var z in VE.TTXNDTL)
                //    {
                //        if (z.SIZECD != null)
                //        {
                //            string ITEM = z.ITCD;
                //            VE.TTXNDTL_IN_SIZE = (from i in VE.TTXNDTL
                //                                  join j in DB.M_SIZE on i.SIZECD equals j.SIZECD into x
                //                                  from j in x.DefaultIfEmpty()
                //                                  where i.ITCD == ITEM
                //                                  select new TTXNDTL_IN_SIZE() { PRINT_SEQ = j.PRINT_SEQ, SIZECD = i.SIZECD, SIZENM = j.SIZENM, COLRCD = i.COLRCD, RATE = i.RATE, QNTY = i.QNTY, AUTONO = i.AUTONO, ITCD = i.ITCD, SLNO = i.SLNO, ITCOLSIZE = i.ITCD + i.COLRCD + i.SIZECD, PCSPERBOX_HIDDEN = z.PCSBOX.Value, PCSPERSET_HIDDEN = z.PCSPERSET.retDbl() }).OrderBy(s => s.PRINT_SEQ).ToList();
                //            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer(); string JR = javaScriptSerializer.Serialize(VE.TTXNDTL_IN_SIZE); z.ChildData = JR;
                //        }
                //    }
                //}
                //VE.TTXNDTL = (from p in VE.TTXNDTL
                //              group p by new
                //              { p.ITCD, p.ITNM, p.STYLENO, p.PCSBOX, p.PCSPERSET, p.UOMNM, p.STKTYPE, p.MTRLJOBCD, p.ChildData } into z
                //              select new TTXNDTL { ITCD = z.Key.ITCD, ITNM = z.Key.ITNM, STYLENO = z.Key.STYLENO, PCSBOX = z.Key.PCSBOX, PCSPERSET = z.Key.PCSPERSET, UOMNM = z.Key.UOMNM, STKTYPE = z.Key.STKTYPE, MTRLJOBCD = z.Key.MTRLJOBCD, ChildData = z.Key.ChildData, QNTY = z.Sum(x => x.QNTY), ALL_SIZE = string.Join(",", z.Select(i => i.SIZECD)) }).OrderBy(A => A.STYLENO).ToList();

                for (int z = 0; z <= VE.TTXNDTL.Count - 1; z++)
                {
                    VE.TTXNDTL[z].DropDown_list2 = Master_Help.STOCK_TYPE(); VE.TTXNDTL[z].SLNO = Convert.ToInt16(z + 1);
                    //if (VE.TTXNDTL[z].STKTYPE == "F")
                    //{
                    //    VE.TTXNDTL[z].BOXES = Salesfunc.ConvPcstoBox(VE.TTXNDTL[z].QNTY == null ? 0 : VE.TTXNDTL[z].QNTY.Value, VE.TTXNDTL[z].PCSBOX == null ? 0 : VE.TTXNDTL[z].PCSBOX.Value);
                    //    VE.TTXNDTL[z].SETS = Salesfunc.ConvPcstoSet(VE.TTXNDTL[z].QNTY == null ? 0 : VE.TTXNDTL[z].QNTY.Value, VE.TTXNDTL[z].PCSPERSET.retDbl());
                    //}
                    //else
                    //{
                    //    VE.TTXNDTL[z].BOXES = 0; VE.TTXNDTL[z].SETS = 0;
                    //}
                    //TOTAL_SETS = TOTAL_SETS + (VE.TTXNDTL[z].SETS == null ? 0 : VE.TTXNDTL[z].SETS);
                    TOTAL_QNTY = TOTAL_QNTY + (VE.TTXNDTL[z].QNTY == null ? 0 : VE.TTXNDTL[z].QNTY);
                    //TOTAL_BOXES = TOTAL_BOXES + (VE.TTXNDTL[z].BOXES == null ? 0 : VE.TTXNDTL[z].BOXES);
                    string MTRLJOBCD = VE.TTXNDTL[z].MTRLJOBCD;
                    VE.TTXNDTL[z].MTRLJOBNM = (from a in DB.M_MTRLJOBMST where a.MTRLJOBCD == MTRLJOBCD select a.MTRLJOBNM).SingleOrDefault();
                }
                VE.TOTAL_IN_BOXES = TOTAL_BOXES.Value;
                VE.TOTAL_IN_QNTY = TOTAL_QNTY.Value;
                VE.TOTAL_IN_SETS = TOTAL_SETS.Value;
                #endregion
                #region OUT TAB DATA
                TOTAL_BOXES = 0; TOTAL_QNTY = 0; TOTAL_SETS = 0;
                VE.TTXNDTL_OUT = (from i in DB.T_TXNDTL
                                  join j in DB.M_SITEM on i.ITCD equals j.ITCD into x
                                  from j in x.DefaultIfEmpty()
                                  join k in DB.M_SIZE on i.SIZECD equals k.SIZECD into y
                                  from k in y.DefaultIfEmpty()
                                  join l in DB.M_COLOR on i.COLRCD equals l.COLRCD into z
                                  from l in z.DefaultIfEmpty()
                                  join m in DB.M_PARTS on i.PARTCD equals m.PARTCD into A
                                  from m in A.DefaultIfEmpty()
                                  where i.AUTONO == sl.AUTONO && i.SLNO <= 1000
                                  select new TTXNDTL_OUT()
                                  {
                                      AUTONO = i.AUTONO,
                                      SLNO = i.SLNO,
                                      EMD_NO = i.EMD_NO,
                                      CLCD = i.CLCD,
                                      DTAG = i.DTAG,
                                      TTAG = i.TTAG,
                                      STKDRCR = i.STKDRCR,
                                      STKTYPE = i.STKTYPE,
                                      ITCD = i.ITCD,
                                      ITNM = j.ITNM,
                                      STYLENO = j.STYLENO,
                                      PARTCD = i.PARTCD,
                                      PARTNM = m.PARTNM,
                                      //PCSBOX = j.PCSPERBOX,
                                      //PCSPERSET = j.PCSPERSET,
                                      UOM = j.UOMCD,
                                      SIZECD = i.SIZECD,
                                      //ALL_SIZE = i.SIZECD,
                                      SIZENM = k.SIZENM,
                                      COLRCD = i.COLRCD,
                                      COLRNM = l.COLRNM,
                                      QNTY = i.QNTY,
                                      MTRLJOBCD = i.MTRLJOBCD
                                  }).OrderBy(s => s.SLNO).ToList();
                //foreach (var z in VE.TTXNDTL_OUT)
                //{
                //    if (z.SIZECD != null)
                //    {
                //        string ITEM = z.ITCD;
                //        VE.TTXNDTL_IN_SIZE = (from i in VE.TTXNDTL_OUT
                //                              join j in DB.M_SIZE on i.SIZECD equals j.SIZECD into x
                //                              from j in x.DefaultIfEmpty()
                //                              where i.ITCD == ITEM
                //                              select new TTXNDTL_IN_SIZE() { PRINT_SEQ = j.PRINT_SEQ, SIZECD = i.SIZECD, SIZENM = j.SIZENM, COLRCD = i.COLRCD, RATE = i.RATE, QNTY = i.QNTY, AUTONO = i.AUTONO, ITCD = i.ITCD, SLNO = i.SLNO, ITCOLSIZE = i.ITCD + i.COLRCD + i.SIZECD, PCSPERBOX_HIDDEN = z.PCSBOX == null ? 0 : z.PCSBOX.Value, PCSPERSET_HIDDEN = z.PCSPERSET.retDbl() }).OrderBy(s => s.PRINT_SEQ).ToList();
                //        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer(); string JR = javaScriptSerializer.Serialize(VE.TTXNDTL_IN_SIZE); z.ChildData = JR;
                //    }
                //}
                //VE.TTXNDTL_OUT = (from p in VE.TTXNDTL_OUT
                //                  group p by new
                //                  { p.ITCD, p.ITNM, p.STYLENO, p.PCSBOX, p.PCSPERSET, p.UOMNM, p.STKTYPE, p.ChildData, p.MTRLJOBCD } into z
                //                  select new TTXNDTL_OUT
                //                  {
                //                      ITCD = z.Key.ITCD,
                //                      ITNM = z.Key.ITNM,
                //                      STYLENO = z.Key.STYLENO,
                //                      PCSBOX = z.Key.PCSBOX
                //                  ,
                //                      PCSPERSET = z.Key.PCSPERSET,
                //                      UOMNM = z.Key.UOMNM,
                //                      STKTYPE = z.Key.STKTYPE,
                //                      ChildData = z.Key.ChildData
                //                  ,
                //                      QNTY = z.Sum(x => x.QNTY),
                //                      ALL_SIZE = string.Join(",", z.Select(i => i.SIZECD)),
                //                      MTRLJOBCD = z.Key.MTRLJOBCD
                //                  }).OrderBy(A => A.STYLENO).ToList();

                for (int z = 0; z <= VE.TTXNDTL_OUT.Count - 1; z++)
                {
                    VE.TTXNDTL_OUT[z].DropDown_list2 = Master_Help.STOCK_TYPE(); VE.TTXNDTL_OUT[z].SLNO = Convert.ToInt16(z + 1);
                    //if (VE.TTXNDTL_OUT[z].STKTYPE == "F")
                    //{
                    //    VE.TTXNDTL_OUT[z].BOXES = Salesfunc.ConvPcstoBox(VE.TTXNDTL_OUT[z].QNTY == null ? 0 : VE.TTXNDTL_OUT[z].QNTY.Value, VE.TTXNDTL_OUT[z].PCSBOX == null ? 0 : VE.TTXNDTL_OUT[z].PCSBOX.Value);
                    //    VE.TTXNDTL_OUT[z].SETS = Salesfunc.ConvPcstoSet(VE.TTXNDTL_OUT[z].QNTY == null ? 0 : VE.TTXNDTL_OUT[z].QNTY.Value, VE.TTXNDTL_OUT[z].PCSPERSET.retDbl());
                    //}
                    //else
                    //{
                    //    VE.TTXNDTL_OUT[z].BOXES = 0; VE.TTXNDTL_OUT[z].SETS = 0;
                    //}
                    //TOTAL_SETS = TOTAL_SETS + (VE.TTXNDTL_OUT[z].SETS == null ? 0 : VE.TTXNDTL_OUT[z].SETS);
                    TOTAL_QNTY = TOTAL_QNTY + (VE.TTXNDTL_OUT[z].QNTY == null ? 0 : VE.TTXNDTL_OUT[z].QNTY);
                    //TOTAL_BOXES = TOTAL_BOXES + (VE.TTXNDTL_OUT[z].BOXES == null ? 0 : VE.TTXNDTL_OUT[z].BOXES);
                    string MTRLJOBCD = VE.TTXNDTL_OUT[z].MTRLJOBCD;
                    VE.TTXNDTL_OUT[z].MTRLJOBNM = (from a in DB.M_MTRLJOBMST where a.MTRLJOBCD == MTRLJOBCD select a.MTRLJOBNM).SingleOrDefault();
                }
                VE.TOTAL_OUT_BOXES = TOTAL_BOXES.Value;
                VE.OUT_T_QNTY = TOTAL_QNTY.Value;
                VE.TOTAL_OUT_SETS = TOTAL_SETS.Value;
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
        public ActionResult SearchPannelData()
        {
            TransactionSaleEntry VE = new TransactionSaleEntry();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string LOC = CommVar.Loccd(UNQSNO);
            string COM = CommVar.Compcd(UNQSNO);
            Cn.getQueryString(VE);
            string[] XYZ = new string[200];
            switch (VE.MENU_PARA)
            {
                case "ADJ": XYZ = (string[])TempData["ADJ"]; break;
                case "CNV": XYZ = (string[])TempData["CNV"]; break;
                case "WAS": XYZ = (string[])TempData["WAS"]; break;
            }
            TempData.Keep();
            var MDT = (from X in DB.T_TXN
                       join Y in DB.T_CNTRL_HDR on X.AUTONO equals Y.AUTONO
                       where (X.AUTONO == Y.AUTONO && Y.LOCCD == LOC && Y.COMPCD == COM && XYZ.Contains(Y.DOCCD))
                       orderby Y.AUTONO
                       select new { Y.AUTONO, Y.DOCCD, Y.DOCNO, Y.DOCONLYNO, Y.DOCDT, X.SLCD }).ToList().Select(Z => new
                       {
                           AUTONO = Z.AUTONO,
                           DOCCD = Z.DOCCD,
                           DOCNO = Z.DOCNO,
                           DOCONLUNO = Z.DOCONLYNO,
                           DOCDT = Z.DOCDT.ToString().Substring(0, 10),
                           SLCD = Z.SLCD,
                           SLNM = (from A in DBF.M_SUBLEG where A.SLCD == Z.SLCD select A.SLNM).SingleOrDefault()
                       }).OrderBy(P => P.DOCONLUNO).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Document Code" + Cn.GCS() + "Party Name" + Cn.GCS() + "Party Code" + Cn.GCS() + "AUTONO";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].DOCNO + "</td><td>" + MDT[j].DOCDT + "</td><td>" + MDT[j].DOCCD + "</td><td><b>" + MDT[j].SLNM + "</b></td><td>" + MDT[j].SLCD + "</td><td>" + MDT[j].AUTONO + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "5", "5"));
        }
        public ActionResult AddGridRow(StockAdjustmentsConversionEntry VE, string TABLE)
        {
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                if (TABLE == "_T_StockAdj_IN_TAB_GRID")
                {
                    List<TTXNDTL> TTXNDTL = new List<TTXNDTL>();
                    if (VE.TTXNDTL == null)
                    {
                        TTXNDTL TXNDTL = new TTXNDTL(); TXNDTL.SLNO = 1; TTXNDTL.Add(TXNDTL); VE.TTXNDTL = TTXNDTL;
                    }
                    else
                    {
                        for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++) { TTXNDTL TXNDTL = new TTXNDTL(); TXNDTL = VE.TTXNDTL[i]; TTXNDTL.Add(TXNDTL); }
                        TTXNDTL TXNDTL1 = new TTXNDTL();
                        TXNDTL1.SLNO = Convert.ToSByte(Convert.ToInt32(VE.TTXNDTL.Max(a => Convert.ToInt32(a.SLNO))) + 1);
                        TTXNDTL.Add(TXNDTL1);
                        VE.TTXNDTL = TTXNDTL;
                    }
                    VE.TTXNDTL.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); });
                    ModelState.Clear();
                    VE.DefaultView = true;
                    return PartialView("_T_StockAdj_IN_TAB", VE);
                }
                else
                {
                    List<TTXNDTL_OUT> TTXNDTL_OUT = new List<TTXNDTL_OUT>();
                    if (VE.TTXNDTL_OUT == null)
                    {
                        TTXNDTL_OUT OUT = new TTXNDTL_OUT(); OUT.SLNO = 1; TTXNDTL_OUT.Add(OUT); VE.TTXNDTL_OUT = TTXNDTL_OUT;
                    }
                    else
                    {
                        for (int i = 0; i <= VE.TTXNDTL_OUT.Count - 1; i++) { TTXNDTL_OUT OUT = new TTXNDTL_OUT(); OUT = VE.TTXNDTL_OUT[i]; TTXNDTL_OUT.Add(OUT); }
                        TTXNDTL_OUT OUT1 = new TTXNDTL_OUT();
                        OUT1.SLNO = Convert.ToSByte(Convert.ToInt32(VE.TTXNDTL_OUT.Max(a => Convert.ToInt32(a.SLNO))) + 1);
                        TTXNDTL_OUT.Add(OUT1);
                        VE.TTXNDTL_OUT = TTXNDTL_OUT;
                    }
                    VE.TTXNDTL_OUT.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); });
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
                if (TABLE == "_T_StockAdj_IN_TAB_GRID")
                {
                    List<TTXNDTL> TTXNDTL = new List<TTXNDTL>();
                    int count = 0;
                    for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++) { if (VE.TTXNDTL[i].Checked == false) { count += 1; TTXNDTL item = new TTXNDTL(); item = VE.TTXNDTL[i]; item.SLNO = Convert.ToSByte(count); TTXNDTL.Add(item); } }
                    VE.TTXNDTL = TTXNDTL;
                    VE.TTXNDTL.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); });
                    ModelState.Clear();
                    VE.DefaultView = true;
                    return PartialView("_T_StockAdj_IN_TAB", VE);
                }
                else
                {
                    List<TTXNDTL_OUT> TTXNDTL_OUT = new List<TTXNDTL_OUT>();
                    int count = 0;
                    for (int i = 0; i <= VE.TTXNDTL_OUT.Count - 1; i++) { if (VE.TTXNDTL_OUT[i].Checked == false) { count += 1; TTXNDTL_OUT item = new TTXNDTL_OUT(); item = VE.TTXNDTL_OUT[i]; item.SLNO = Convert.ToSByte(count); TTXNDTL_OUT.Add(item); } }
                    VE.TTXNDTL_OUT = TTXNDTL_OUT;
                    VE.TTXNDTL_OUT.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); });
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

        public ActionResult GetMtrlJobDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.MTRLJOBCD_help(val));
                }
                else
                {
                    string str = Master_Help.MTRLJOBCD_help(val);
                    return Content(str);
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
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
        public ActionResult GetGodownDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.GOCD_help(val));
                }
                else
                {
                    string str = Master_Help.GOCD_help(val);
                    return Content(str);
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        public ActionResult GetArticleItemDetails(StockAdjustmentsConversionEntry VE, string val, string TAG)
        {
            try
            {
                Cn.getQueryString(VE);
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.ITCD_help(val, "", "", "", TAG));
                }
                else
                {
                    string str = Master_Help.ITCD_help(val, "", "", "", TAG);
                    return Content(str);
                    //var SIZE_DATA = Salesfunc.getSizeData(val, "", VE.T_TXN.DOCDT.ToString().Remove(10));
                }

            }
            catch (Exception e)
            {
                Cn.SaveException(e, "");
                return Content(e.ToString());
            }
        }
        public ActionResult GetPartDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.PARTS(val));
                }
                else
                {
                    string str = Master_Help.PARTS(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        //public ActionResult OPENSIZE(StockAdjustmentsConversionEntry VE, short SerialNo, string ITEM, string ITEM_NM, string STYLE_NO, string UOM, string PART_CD, string PART_NM, double? PCSPERBOX, string STK_TYPE, double PCS_PERSET)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    string COM = CommVar.Compcd(UNQSNO); string LOC = CommVar.Loccd(UNQSNO); string scm1 = DB.CacheKey.ToString();
        //    try
        //    {
        //        STYLE_NO = STYLE_NO.Replace('μ', '+'); STYLE_NO = STYLE_NO.Replace('‡', '&'); PART_CD = PART_CD == "" ? null : PART_CD;
        //        ViewBag.Article = STYLE_NO; ViewBag.Pcs = PCSPERBOX;
        //        short POSRL = Convert.ToInt16(SerialNo);
        //        var query = (from c in VE.TTXNDTL where (c.ITCD == ITEM && c.STKTYPE == STK_TYPE && c.PARTCD == PART_CD) select c).ToList();
        //        if (query != null)
        //        {
        //            var SIZE_DATA = Salesfunc.getSizeData(ITEM, "", VE.T_TXN.DOCDT.ToString().Remove(10));

        //            VE.TTXNDTL_IN_SIZE = (from DataRow dr in SIZE_DATA.Rows
        //                                  select new TTXNDTL_IN_SIZE()
        //                                  {
        //                                      MIXSIZE = dr["mixsize"].ToString(),
        //                                      PRINT_SEQ = dr["print_seq"].ToString(),
        //                                      SIZECD = dr["sizecd"].ToString(),
        //                                      SIZENM = dr["sizenm"].ToString(),
        //                                      COLRCD = dr["colrcd"].ToString(),
        //                                      COLRNM = dr["colrnm"].ToString(),
        //                                      ParentSerialNo = SerialNo,
        //                                      ITCD_HIDDEN = ITEM,
        //                                      ITNM_HIDDEN = ITEM_NM,
        //                                      STYLENO_HIDDEN = STYLE_NO,
        //                                      UOM_HIDDEN = UOM,
        //                                      PARTCD_HIDDEN = PART_CD,
        //                                      PARTNM_HIDDEN = PART_NM,
        //                                      PCSPERBOX_HIDDEN = PCSPERBOX.Value,
        //                                      PCSPERSET_HIDDEN = PCS_PERSET,
        //                                      STKTYPE_HIDDEN = STK_TYPE,
        //                                      ITCOLSIZE = ITEM + dr["colrcd"].ToString() + dr["sizecd"].ToString(),
        //                                      QNTY = 0
        //                                  }).OrderBy(a => a.PRINT_SEQ).ToList();

        //            if (query[0].ChildData == null)
        //            {
        //                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                string JR = javaScriptSerializer.Serialize(VE.TTXNDTL_IN_SIZE);
        //                query.ForEach(a => a.ChildData = JR);
        //            }
        //            else
        //            {
        //                if (VE.TTXNDTL != null)
        //                {
        //                    string SIZECD = "";
        //                    var query1 = (from c in VE.TTXNDTL where (c.ITCD == ITEM && c.STKTYPE == STK_TYPE && c.PARTCD == PART_CD) select c).ToList();
        //                    if (query1 != null)
        //                    {
        //                        var helpMT = new List<Improvar.Models.TTXNDTL_IN_SIZE>();
        //                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                        helpMT = javaScriptSerializer.Deserialize<List<Improvar.Models.TTXNDTL_IN_SIZE>>(query1[0].ChildData);
        //                        if (helpMT != null && helpMT.Count > 0)
        //                        {
        //                            for (int z = 0; z <= helpMT.Count - 1; z++)
        //                            {
        //                                if (helpMT[z].QNTY != null)
        //                                {
        //                                    SIZECD = SIZECD + Cn.GCS() + helpMT[z].SIZECD;
        //                                }
        //                            }
        //                        }
        //                        var SIZE_CODE = SIZECD.Split(Convert.ToChar(Cn.GCS())).ToList();
        //                        if (SIZECD != "") { SIZE_CODE = SIZECD.Substring(1).Split(Convert.ToChar(Cn.GCS())).ToList(); }
        //                        VE.TTXNDTL_IN_SIZE = VE.TTXNDTL_IN_SIZE.Where(a => !SIZE_CODE.Contains(a.SIZECD)).ToList();
        //                        helpMT.ForEach(a =>
        //                        {
        //                            a.ParentSerialNo = SerialNo; a.ITCD_HIDDEN = ITEM; a.ITNM_HIDDEN = ITEM_NM; a.STYLENO_HIDDEN = STYLE_NO; a.UOM_HIDDEN = UOM;
        //                            a.PARTCD_HIDDEN = PART_CD; a.PARTNM_HIDDEN = PART_NM; a.PCSPERBOX_HIDDEN = PCSPERBOX.Value; a.STKTYPE_HIDDEN = STK_TYPE; a.PCSPERSET_HIDDEN = PCS_PERSET;
        //                        });
        //                        helpMT.ToList().Where(a => a.QNTY_HIDDEN == 0).ForEach(a => { a.QNTY_HIDDEN = null; a.QNTY = null; });
        //                        VE.TTXNDTL_IN_SIZE.AddRange(helpMT);
        //                        var javaScriptSerializer_New = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                        string JR_New = javaScriptSerializer_New.Serialize(VE.TTXNDTL_IN_SIZE);
        //                        query.ForEach(a => a.ChildData = JR_New);
        //                    }
        //                }
        //            }
        //            string SIZE = "";
        //            VE.TTXNDTL = (from x in VE.TTXNDTL where x.ITCD == ITEM && x.STKTYPE == STK_TYPE && x.PARTCD == PART_CD select x).ToList();
        //            if (VE.TTXNDTL != null && VE.TTXNDTL.Count > 0)
        //            {
        //                for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
        //                {
        //                    var helpMT = new List<Improvar.Models.TTXNDTL_IN_SIZE>();
        //                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                    helpMT = javaScriptSerializer.Deserialize<List<Improvar.Models.TTXNDTL_IN_SIZE>>(VE.TTXNDTL[i].ChildData);
        //                    if (helpMT != null && helpMT.Count > 0)
        //                    {
        //                        for (int z = 0; z <= helpMT.Count - 1; z++)
        //                        {
        //                            if (helpMT[z].QNTY != null)
        //                            {
        //                                SIZE = SIZE + Cn.GCS() + helpMT[z].SIZECD;
        //                            }
        //                        }
        //                    }
        //                    helpMT.ForEach(a =>
        //                    {
        //                        a.ParentSerialNo = SerialNo; a.ITCD_HIDDEN = ITEM; a.ITNM_HIDDEN = ITEM_NM; a.STYLENO_HIDDEN = STYLE_NO; a.UOM_HIDDEN = UOM;
        //                        a.PARTCD_HIDDEN = PART_CD; a.PARTNM_HIDDEN = PART_NM; a.PCSPERBOX = PCSPERBOX.Value; a.STKTYPE_HIDDEN = STK_TYPE; a.PCSPERSET_HIDDEN = PCS_PERSET;
        //                    });
        //                    helpMT.ToList().Where(a => a.QNTY_HIDDEN == 0).ForEach(a => { a.QNTY_HIDDEN = null; a.QNTY = null; });
        //                    var SIZE_CODE = SIZE.Split(Convert.ToChar(Cn.GCS())).ToList();
        //                    if (SIZE != "")
        //                    {
        //                        SIZE_CODE = SIZE.Substring(1).Split(Convert.ToChar(Cn.GCS())).ToList();
        //                    }
        //                    VE.TTXNDTL_IN_SIZE = VE.TTXNDTL_IN_SIZE.Where(a => !SIZE_CODE.Contains(a.SIZECD)).ToList();
        //                    VE.TTXNDTL_IN_SIZE.AddRange(helpMT);
        //                }
        //            }
        //            VE.TTXNDTL_IN_SIZE = VE.TTXNDTL_IN_SIZE.OrderBy(a => a.PRINT_SEQ).ToList();
        //            for (int Z = 0; Z <= VE.TTXNDTL_IN_SIZE.Count - 1; Z++)
        //            {
        //                VE.TTXNDTL_IN_SIZE[Z].SLNO = Convert.ToInt16(Z + 1);
        //            }
        //            var javaScriptSerializer_FINAL = new System.Web.Script.Serialization.JavaScriptSerializer();
        //            VE.TTXNDTL_IN_SIZE.ForEach(a => a.ITCOLSIZE = a.ITCD_HIDDEN + a.COLRCD + a.SIZECD);
        //            string JR_FINAL = javaScriptSerializer_FINAL.Serialize(VE.TTXNDTL_IN_SIZE);
        //            query.ForEach(a => a.ChildData = JR_FINAL);
        //            VE.SERIAL = SerialNo;
        //            VE.DefaultView = true;
        //            ModelState.Clear();
        //            return PartialView("_T_StockAdj_IN_SIZE", VE);
        //        }
        //        else
        //        {
        //            VE.SERIAL = SerialNo;
        //            VE.DefaultView = true;
        //            ModelState.Clear();
        //            return PartialView("_T_StockAdj_IN_SIZE", VE);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        VE.DefaultView = false;
        //        VE.DefaultDay = 0;
        //        ViewBag.ErrorMessage = ex.Message + ex.InnerException;
        //        Cn.SaveException(ex, "");
        //        return View(VE);
        //    }
        //}
        //public ActionResult CLOSESIZE(StockAdjustmentsConversionEntry VE, FormCollection FC)
        //{
        //    Cn.getQueryString(VE);
        //    List<TTXNDTL_IN_SIZE> DTL_IN_SIZE = new List<TTXNDTL_IN_SIZE>();
        //    if (VE.TTXNDTL_IN_SIZE != null)
        //    {
        //        DTL_IN_SIZE = VE.TTXNDTL_IN_SIZE.Where(a => a.QNTY != null).Where(a => a.QNTY > 0).ToList();
        //        if (DTL_IN_SIZE != null)
        //        {
        //            var FILTER_DATA = (from p in DTL_IN_SIZE
        //                               group p by new
        //                               { p.ITCD_HIDDEN, p.ITNM_HIDDEN, p.STYLENO_HIDDEN, p.UOM_HIDDEN, p.PARTCD_HIDDEN, p.PARTNM_HIDDEN, p.PCSPERSET_HIDDEN, p.PCSPERBOX_HIDDEN, p.STKTYPE_HIDDEN } into g
        //                               select new
        //                               {
        //                                   ITCD_HIDDEN = g.Key.ITCD_HIDDEN,
        //                                   ITNM_HIDDEN = g.Key.ITNM_HIDDEN,
        //                                   STYLENO_HIDDEN = g.Key.STYLENO_HIDDEN,
        //                                   UOM_HIDDEN = g.Key.UOM_HIDDEN,
        //                                   PARTCD_HIDDEN = g.Key.PARTCD_HIDDEN,
        //                                   PARTNM_HIDDEN = g.Key.PARTNM_HIDDEN,
        //                                   PCSPERSET_HIDDEN = g.Key.PCSPERSET_HIDDEN,
        //                                   PCSPERBOX_HIDDEN = g.Key.PCSPERBOX_HIDDEN,
        //                                   STKTYPE_HIDDEN = g.Key.STKTYPE_HIDDEN,
        //                                   QNTY = g.Sum(x => x.QNTY),
        //                                   ALL_SIZE = string.Join(",", g.Select(i => i.SIZECD))
        //                               }).ToList();

        //            var TTXNDTL_FINALDATA = (from a in FILTER_DATA
        //                                     select new TTXNDTL
        //                                     {
        //                                         ITCD = a.ITCD_HIDDEN,
        //                                         ITNM = a.ITNM_HIDDEN,
        //                                         STYLENO = a.STYLENO_HIDDEN,
        //                                         UOMNM = a.UOM_HIDDEN,
        //                                         PARTCD = a.PARTCD_HIDDEN,
        //                                         PARTNM = a.PARTNM_HIDDEN,
        //                                         PCSBOX = a.PCSPERBOX_HIDDEN,
        //                                         PCSPERSET = a.PCSPERSET_HIDDEN,
        //                                         STKTYPE = a.STKTYPE_HIDDEN,
        //                                         QNTY = a.QNTY,
        //                                         ALL_SIZE = a.ALL_SIZE
        //                                     }).ToList();

        //            if (TTXNDTL_FINALDATA != null && TTXNDTL_FINALDATA.Count > 0)
        //            {
        //                VE.TTXNDTL.RemoveAll(x => x.ITCD == TTXNDTL_FINALDATA[0].ITCD && x.STKTYPE == TTXNDTL_FINALDATA[0].STKTYPE);
        //                var INDEX = VE.TTXNDTL.FindIndex(Z => Z.ITCD == null);
        //                if (INDEX == -1)
        //                {
        //                    VE.TTXNDTL.AddRange(TTXNDTL_FINALDATA);
        //                }
        //                else
        //                {
        //                    VE.TTXNDTL.InsertRange(INDEX, TTXNDTL_FINALDATA);
        //                }
        //            }
        //            string ITEM = "";
        //            if (DTL_IN_SIZE != null && DTL_IN_SIZE.Count > 0)
        //            {
        //                ITEM = DTL_IN_SIZE[0].ITCD_HIDDEN;
        //            }
        //            var query = (from c in VE.TTXNDTL where (c.ITCD == ITEM) select c).ToList();
        //            if (query != null && query.Count > 0)
        //            {
        //                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                string JR = javaScriptSerializer.Serialize(DTL_IN_SIZE);
        //                query.ForEach(a => a.ChildData = JR);
        //            }
        //        }
        //        else
        //        {
        //            string ITEM = FC["hiddenITEM"].ToString();
        //            TTXNDTL query = (from c in VE.TTXNDTL where (c.ITCD == ITEM) select c).SingleOrDefault();
        //            if (query != null)
        //            {
        //                query.ChildData = null;
        //            }
        //        }
        //        short SL_NO = 0;
        //        var SIZE_NULL_COUNT = VE.TTXNDTL_IN_SIZE.Where(a => a.QNTY != null).Where(a => a.QNTY == 0 || a.QNTY == null).ToList();
        //        VE.TTXNDTL.ToList().ForEach(a =>
        //        {
        //            a.DropDown_list2 = Master_Help.STOCK_TYPE(); a.SLNO = ++SL_NO;
        //            if (a.STKTYPE == "F")
        //            {
        //                a.BOXES = Salesfunc.ConvPcstoBox(a.QNTY == null ? 0 : a.QNTY.Value, a.PCSBOX == null ? 0 : a.PCSBOX.Value);
        //                a.SETS = Salesfunc.ConvPcstoSet(a.QNTY == null ? 0 : a.QNTY.Value, a.PCSPERSET.retDbl());
        //            }
        //            else
        //            {
        //                a.BOXES = 0; a.SETS = 0;
        //            }
        //        });
        //    }
        //    else
        //    {

        //        string ITEM = FC["hiddenITEM"].ToString();
        //        TTXNDTL query = (from c in VE.TTXNDTL where (c.ITCD == ITEM) select c).SingleOrDefault();
        //        if (query != null)
        //        {
        //            query.ChildData = null;
        //        }
        //        VE.TTXNDTL.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); });
        //    }
        //    VE.DefaultView = true;
        //    ModelState.Clear();
        //    return PartialView("_T_StockAdj_IN_TAB", VE);
        //}
        //public ActionResult OPENSIZE_OUT(StockAdjustmentsConversionEntry VE, short SerialNo, string ITEM, string ITEM_NM, string STYLE_NO, string UOM, string PART_CD, string PART_NM, double? PCSPERBOX, string STK_TYPE, double PCS_PERSET)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    string COM = CommVar.Compcd(UNQSNO); string LOC = CommVar.Loccd(UNQSNO); string scm1 = DB.CacheKey.ToString();
        //    try
        //    {
        //        STYLE_NO = STYLE_NO.Replace('μ', '+'); STYLE_NO = STYLE_NO.Replace('‡', '&'); PART_CD = PART_CD == "" ? null : PART_CD;
        //        ViewBag.Article = STYLE_NO; ViewBag.Pcs = PCSPERBOX;
        //        short POSRL = Convert.ToInt16(SerialNo);
        //        var query = (from c in VE.TTXNDTL_OUT where (c.ITCD == ITEM && c.STKTYPE == STK_TYPE && c.PARTCD == PART_CD) select c).ToList();
        //        if (query != null)
        //        {
        //            var SIZE_DATA = Salesfunc.getSizeData(ITEM, "", VE.T_TXN.DOCDT.ToString().Remove(10));

        //            VE.TTXNDTL_OUT_SIZE = (from DataRow dr in SIZE_DATA.Rows
        //                                   select new TTXNDTL_OUT_SIZE()
        //                                   {
        //                                       MIXSIZE = dr["mixsize"].ToString(),
        //                                       PRINT_SEQ = dr["print_seq"].ToString(),
        //                                       SIZECD = dr["sizecd"].ToString(),
        //                                       SIZENM = dr["sizenm"].ToString(),
        //                                       COLRCD = dr["colrcd"].ToString(),
        //                                       COLRNM = dr["colrnm"].ToString(),
        //                                       ParentSerialNo = SerialNo,
        //                                       ITCD_HIDDEN = ITEM,
        //                                       ITNM_HIDDEN = ITEM_NM,
        //                                       STYLENO_HIDDEN = STYLE_NO,
        //                                       UOM_HIDDEN = UOM,
        //                                       PARTCD_HIDDEN = PART_CD,
        //                                       PARTNM_HIDDEN = PART_NM,
        //                                       PCSPERBOX_HIDDEN = PCSPERBOX.Value,
        //                                       PCSPERSET_HIDDEN = PCS_PERSET,
        //                                       STKTYPE_HIDDEN = STK_TYPE,
        //                                       ITCOLSIZE = ITEM + dr["colrcd"].ToString() + dr["sizecd"].ToString(),
        //                                       QNTY = 0
        //                                   }).OrderBy(a => a.PRINT_SEQ).ToList();

        //            if (query[0].ChildData == null)
        //            {
        //                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                string JR = javaScriptSerializer.Serialize(VE.TTXNDTL_OUT_SIZE);
        //                query.ForEach(a => a.ChildData = JR);
        //            }
        //            else
        //            {
        //                if (VE.TTXNDTL_OUT != null)
        //                {
        //                    string SIZECD = "";
        //                    var query1 = (from c in VE.TTXNDTL_OUT where (c.ITCD == ITEM && c.STKTYPE == STK_TYPE && c.PARTCD == PART_CD) select c).ToList();
        //                    if (query1 != null)
        //                    {
        //                        var helpMT = new List<Improvar.Models.TTXNDTL_OUT_SIZE>();
        //                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                        helpMT = javaScriptSerializer.Deserialize<List<Improvar.Models.TTXNDTL_OUT_SIZE>>(query1[0].ChildData);
        //                        if (helpMT != null && helpMT.Count > 0)
        //                        {
        //                            for (int z = 0; z <= helpMT.Count - 1; z++)
        //                            {
        //                                if (helpMT[z].QNTY != null)
        //                                {
        //                                    SIZECD = SIZECD + Cn.GCS() + helpMT[z].SIZECD;
        //                                }
        //                            }
        //                        }
        //                        var SIZE_CODE = SIZECD.Split(Convert.ToChar(Cn.GCS())).ToList();
        //                        if (SIZECD != "") { SIZE_CODE = SIZECD.Substring(1).Split(Convert.ToChar(Cn.GCS())).ToList(); }
        //                        VE.TTXNDTL_OUT_SIZE = VE.TTXNDTL_OUT_SIZE.Where(a => !SIZE_CODE.Contains(a.SIZECD)).ToList();
        //                        helpMT.ForEach(a =>
        //                        {
        //                            a.ParentSerialNo = SerialNo; a.ITCD_HIDDEN = ITEM; a.ITNM_HIDDEN = ITEM_NM; a.STYLENO_HIDDEN = STYLE_NO; a.UOM_HIDDEN = UOM;
        //                            a.PARTCD_HIDDEN = PART_CD; a.PARTNM_HIDDEN = PART_NM; a.PCSPERBOX_HIDDEN = PCSPERBOX.Value; a.STKTYPE_HIDDEN = STK_TYPE; a.PCSPERSET_HIDDEN = PCS_PERSET;
        //                        });
        //                        helpMT.ToList().Where(a => a.QNTY_HIDDEN == 0).ForEach(a => { a.QNTY_HIDDEN = null; a.QNTY = null; });
        //                        VE.TTXNDTL_OUT_SIZE.AddRange(helpMT);
        //                        var javaScriptSerializer_New = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                        string JR_New = javaScriptSerializer_New.Serialize(VE.TTXNDTL_OUT_SIZE);
        //                        query.ForEach(a => a.ChildData = JR_New);
        //                    }
        //                }
        //            }
        //            string SIZE = "";
        //            VE.TTXNDTL_OUT = (from x in VE.TTXNDTL_OUT where x.ITCD == ITEM && x.STKTYPE == STK_TYPE && x.PARTCD == PART_CD select x).ToList();
        //            if (VE.TTXNDTL != null && VE.TTXNDTL_OUT.Count > 0)
        //            {
        //                for (int i = 0; i <= VE.TTXNDTL_OUT.Count - 1; i++)
        //                {
        //                    var helpMT = new List<Improvar.Models.TTXNDTL_OUT_SIZE>();
        //                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                    helpMT = javaScriptSerializer.Deserialize<List<Improvar.Models.TTXNDTL_OUT_SIZE>>(VE.TTXNDTL_OUT[i].ChildData);
        //                    if (helpMT != null && helpMT.Count > 0)
        //                    {
        //                        for (int z = 0; z <= helpMT.Count - 1; z++)
        //                        {
        //                            if (helpMT[z].QNTY != null)
        //                            {
        //                                SIZE = SIZE + Cn.GCS() + helpMT[z].SIZECD;
        //                            }
        //                        }
        //                    }
        //                    helpMT.ForEach(a =>
        //                    {
        //                        a.ParentSerialNo = SerialNo; a.ITCD_HIDDEN = ITEM; a.ITNM_HIDDEN = ITEM_NM; a.STYLENO_HIDDEN = STYLE_NO; a.UOM_HIDDEN = UOM;
        //                        a.PARTCD_HIDDEN = PART_CD; a.PARTNM_HIDDEN = PART_NM; a.PCSPERBOX = PCSPERBOX.Value; a.STKTYPE_HIDDEN = STK_TYPE; a.PCSPERSET_HIDDEN = PCS_PERSET;
        //                    });
        //                    helpMT.ToList().Where(a => a.QNTY_HIDDEN == 0).ForEach(a => { a.QNTY_HIDDEN = null; a.QNTY = null; });
        //                    var SIZE_CODE = SIZE.Split(Convert.ToChar(Cn.GCS())).ToList();
        //                    if (SIZE != "")
        //                    {
        //                        SIZE_CODE = SIZE.Substring(1).Split(Convert.ToChar(Cn.GCS())).ToList();
        //                    }
        //                    VE.TTXNDTL_OUT_SIZE = VE.TTXNDTL_OUT_SIZE.Where(a => !SIZE_CODE.Contains(a.SIZECD)).ToList();
        //                    VE.TTXNDTL_OUT_SIZE.AddRange(helpMT);
        //                }
        //            }
        //            VE.TTXNDTL_OUT_SIZE = VE.TTXNDTL_OUT_SIZE.OrderBy(a => a.PRINT_SEQ).ToList();
        //            for (int Z = 0; Z <= VE.TTXNDTL_OUT_SIZE.Count - 1; Z++)
        //            {
        //                VE.TTXNDTL_OUT_SIZE[Z].SLNO = Convert.ToInt16(Z + 1);
        //            }
        //            var javaScriptSerializer_FINAL = new System.Web.Script.Serialization.JavaScriptSerializer();
        //            VE.TTXNDTL_OUT_SIZE.ForEach(a => a.ITCOLSIZE = a.ITCD_HIDDEN + a.COLRCD + a.SIZECD);
        //            string JR_FINAL = javaScriptSerializer_FINAL.Serialize(VE.TTXNDTL_OUT_SIZE);
        //            query.ForEach(a => a.ChildData = JR_FINAL);
        //            VE.SERIAL = SerialNo;
        //            VE.DefaultView = true;
        //            ModelState.Clear();
        //            return PartialView("_T_StockAdj_OUT_SIZE", VE);
        //        }
        //        else
        //        {
        //            VE.SERIAL = SerialNo;
        //            VE.DefaultView = true;
        //            ModelState.Clear();
        //            return PartialView("_T_StockAdj_OUT_SIZE", VE);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        VE.DefaultView = false;
        //        VE.DefaultDay = 0;
        //        ViewBag.ErrorMessage = ex.Message + ex.InnerException;
        //        Cn.SaveException(ex, "");
        //        return View(VE);
        //    }
        //}
        //public ActionResult CLOSESIZE_OUT(StockAdjustmentsConversionEntry VE, FormCollection FC)
        //{
        //    Cn.getQueryString(VE);
        //    List<TTXNDTL_OUT_SIZE> DTL_OUT_SIZE = new List<TTXNDTL_OUT_SIZE>();
        //    if (VE.TTXNDTL_OUT_SIZE != null)
        //    {
        //        DTL_OUT_SIZE = VE.TTXNDTL_OUT_SIZE.Where(a => a.QNTY != null).Where(a => a.QNTY > 0).ToList();
        //        if (DTL_OUT_SIZE != null)
        //        {
        //            var FILTER_DATA = (from p in DTL_OUT_SIZE
        //                               group p by new
        //                               { p.ITCD_HIDDEN, p.ITNM_HIDDEN, p.STYLENO_HIDDEN, p.UOM_HIDDEN, p.PARTCD_HIDDEN, p.PARTNM_HIDDEN, p.PCSPERSET_HIDDEN, p.PCSPERBOX_HIDDEN, p.STKTYPE_HIDDEN } into g
        //                               select new
        //                               {
        //                                   ITCD_HIDDEN = g.Key.ITCD_HIDDEN,
        //                                   ITNM_HIDDEN = g.Key.ITNM_HIDDEN,
        //                                   STYLENO_HIDDEN = g.Key.STYLENO_HIDDEN,
        //                                   UOM_HIDDEN = g.Key.UOM_HIDDEN,
        //                                   PARTCD_HIDDEN = g.Key.PARTCD_HIDDEN,
        //                                   PARTNM_HIDDEN = g.Key.PARTNM_HIDDEN,
        //                                   PCSPERSET_HIDDEN = g.Key.PCSPERSET_HIDDEN,
        //                                   PCSPERBOX_HIDDEN = g.Key.PCSPERBOX_HIDDEN,
        //                                   STKTYPE_HIDDEN = g.Key.STKTYPE_HIDDEN,
        //                                   QNTY = g.Sum(x => x.QNTY),
        //                                   ALL_SIZE = string.Join(",", g.Select(i => i.SIZECD))
        //                               }).ToList();

        //            var TTXNDTL_OUT_FINALDATA = (from a in FILTER_DATA
        //                                         select new TTXNDTL_OUT
        //                                         {
        //                                             ITCD = a.ITCD_HIDDEN,
        //                                             ITNM = a.ITNM_HIDDEN,
        //                                             STYLENO = a.STYLENO_HIDDEN,
        //                                             UOMNM = a.UOM_HIDDEN,
        //                                             PARTCD = a.PARTCD_HIDDEN,
        //                                             PARTNM = a.PARTNM_HIDDEN,
        //                                             PCSBOX = a.PCSPERBOX_HIDDEN,
        //                                             PCSPERSET = a.PCSPERSET_HIDDEN,
        //                                             STKTYPE = a.STKTYPE_HIDDEN,
        //                                             QNTY = a.QNTY,
        //                                             ALL_SIZE = a.ALL_SIZE
        //                                         }).ToList();

        //            if (TTXNDTL_OUT_FINALDATA != null && TTXNDTL_OUT_FINALDATA.Count > 0)
        //            {
        //                VE.TTXNDTL_OUT.RemoveAll(x => x.ITCD == TTXNDTL_OUT_FINALDATA[0].ITCD && x.STKTYPE == TTXNDTL_OUT_FINALDATA[0].STKTYPE && x.PARTCD == TTXNDTL_OUT_FINALDATA[0].PARTCD);
        //                var INDEX = VE.TTXNDTL_OUT.FindIndex(Z => Z.ITCD == null);
        //                if (INDEX == -1)
        //                {
        //                    VE.TTXNDTL_OUT.AddRange(TTXNDTL_OUT_FINALDATA);
        //                }
        //                else
        //                {
        //                    VE.TTXNDTL_OUT.InsertRange(INDEX, TTXNDTL_OUT_FINALDATA);
        //                }
        //            }
        //            string ITEM = "";
        //            if (DTL_OUT_SIZE != null && DTL_OUT_SIZE.Count > 0)
        //            {
        //                ITEM = DTL_OUT_SIZE[0].ITCD_HIDDEN;
        //            }
        //            var query = (from c in VE.TTXNDTL_OUT where (c.ITCD == ITEM) select c).ToList();
        //            if (query != null && query.Count > 0)
        //            {
        //                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                string JR = javaScriptSerializer.Serialize(DTL_OUT_SIZE);
        //                query.ForEach(a => a.ChildData = JR);
        //            }
        //        }
        //        else
        //        {
        //            string ITEM = FC["hiddenITEM"].ToString();
        //            TTXNDTL_OUT query = (from c in VE.TTXNDTL_OUT where (c.ITCD == ITEM) select c).SingleOrDefault();
        //            if (query != null)
        //            {
        //                query.ChildData = null;
        //            }
        //        }
        //        short SL_NO = 0;
        //        var SIZE_NULL_COUNT = VE.TTXNDTL_OUT_SIZE.Where(a => a.QNTY != null).Where(a => a.QNTY == 0 || a.QNTY == null).ToList();
        //        VE.TTXNDTL_OUT.ToList().ForEach(a =>
        //        {
        //            a.DropDown_list2 = Master_Help.STOCK_TYPE(); a.SLNO = ++SL_NO;
        //            if (a.STKTYPE == "F")
        //            {
        //                a.BOXES = Salesfunc.ConvPcstoBox(a.QNTY == null ? 0 : a.QNTY.Value, a.PCSBOX == null ? 0 : a.PCSBOX.Value);
        //                a.SETS = Salesfunc.ConvPcstoSet(a.QNTY == null ? 0 : a.QNTY.Value, a.PCSPERSET.retDbl());
        //            }
        //            else
        //            {
        //                a.BOXES = 0; a.SETS = 0;
        //            }
        //        });
        //    }
        //    else
        //    {
        //        string ITEM = FC["hiddenITEM"].ToString();
        //        TTXNDTL_OUT query = (from c in VE.TTXNDTL_OUT where (c.ITCD == ITEM) select c).SingleOrDefault();
        //        if (query != null)
        //        {
        //            query.ChildData = null;
        //        }
        //        VE.TTXNDTL_OUT.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); });
        //    }
        //    VE.DefaultView = true;
        //    ModelState.Clear();
        //    return PartialView("_T_StockAdj_OUT_TAB", VE);
        //}
        public ActionResult SAVE(FormCollection FC, StockAdjustmentsConversionEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    try
                    {
                        Cn.getQueryString(VE);
                        DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                        if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                        {
                            #region HEADER ENTRY
                            T_TXN TTXN = new T_TXN();
                            T_TXNOTH TTXNOTH = new T_TXNOTH();
                            T_CNTRL_HDR TCH = new T_CNTRL_HDR();
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
                            if (VE.DefaultAction == "E")
                            {
                                TTXN.DTAG = "E";

                                DB.T_TXNDTL.Where(x => x.AUTONO == TTXN.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_TXNDTL.RemoveRange(DB.T_TXNDTL.Where(x => x.AUTONO == TTXN.AUTONO));

                                DB.T_TXNOTH.Where(x => x.AUTONO == TTXN.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.SaveChanges();
                                DB.T_TXNOTH.RemoveRange(DB.T_TXNOTH.Where(x => x.AUTONO == TTXN.AUTONO));

                                DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == TTXN.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_CNTRL_HDR_REM.RemoveRange(DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == TTXN.AUTONO));

                                DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == TTXN.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == TTXN.AUTONO));

                                DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == TTXN.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == TTXN.AUTONO));
                                DB.SaveChanges();
                            }

                            TTXNOTH.AUTONO = TTXN.AUTONO;
                            TTXNOTH.EMD_NO = TTXN.EMD_NO;
                            TTXNOTH.CLCD = TTXN.CLCD;
                            TTXNOTH.DTAG = TTXN.DTAG;
                            TTXNOTH.DOCREM = VE.T_TXNOTH.DOCREM;

                            TCH = Cn.T_CONTROL_HDR(TTXN.DOCCD, TTXN.DOCDT, TTXN.DOCNO, TTXN.AUTONO, Month, DOCPATTERN, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, null, 0, null);
                            if (VE.DefaultAction == "A")
                            {
                                DB.T_TXN.Add(TTXN);
                                DB.T_CNTRL_HDR.Add(TCH);
                            }
                            else if (VE.DefaultAction == "E")
                            {
                                DB.Entry(TTXN).State = System.Data.Entity.EntityState.Modified;
                                DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                            }
                            DB.T_TXNOTH.Add(TTXNOTH);
                            #endregion
                            #region IN TAB ENTRY
                            int COUNTER_IN = 1000; double IN_TOTAL_QNTY = 0;
                            for (int i = 0; VE.TTXNDTL != null && i <= VE.TTXNDTL.Count - 1; i++)
                            {
                                if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
                                {
                                    //var SIZE_CODE = "".Split(',');
                                    //if (VE.TTXNDTL[i].ALL_SIZE != null) SIZE_CODE = VE.TTXNDTL[i].ALL_SIZE.Split(',');
                                    //if (VE.TTXNDTL[i].ChildData != null)
                                    //{
                                    //    var SIZE_CODE = VE.TTXNDTL[i].ALL_SIZE.Split(',');
                                    //    string data = VE.TTXNDTL[i].ChildData;
                                    //    var helpM = new List<Improvar.Models.TTXNDTL_IN_SIZE>();
                                    //    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                    //    helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.TTXNDTL_IN_SIZE>>(data);
                                    //    helpM = helpM.Where(a => SIZE_CODE.Contains(a.SIZECD)).ToList();
                                    //    for (int j = 0; j <= helpM.Count - 1; j++)
                                    //    {
                                    //        if (helpM[j].SLNO != 0 && helpM[j].QNTY != null && helpM[j].QNTY != 0)
                                    //        {
                                    //            COUNTER_IN = COUNTER_IN + 1;
                                    //            T_TXNDTL TTXNDTL = new T_TXNDTL();
                                    //            TTXNDTL.EMD_NO = TTXN.EMD_NO;
                                    //            TTXNDTL.CLCD = TTXN.CLCD;
                                    //            TTXNDTL.DTAG = TTXN.DTAG;
                                    //            TTXNDTL.TTAG = TTXN.TTAG;
                                    //            TTXNDTL.AUTONO = TTXN.AUTONO;
                                    //            //TTXNDTL.DOCCD = TTXN.DOCCD;
                                    //            //TTXNDTL.DOCNO = TTXN.DOCNO;
                                    //            //TTXNDTL.DOCDT = TTXN.DOCDT;
                                    //            TTXNDTL.SLNO = Convert.ToInt16(COUNTER_IN);
                                    //            TTXNDTL.STKDRCR = "D";
                                    //            TTXNDTL.STKTYPE = VE.TTXNDTL[i].STKTYPE;
                                    //            TTXNDTL.GOCD = TTXN.GOCD;
                                    //            if (!string.IsNullOrEmpty(VE.TTXNDTL[i].MTRLJOBCD))
                                    //            {
                                    //                TTXNDTL.MTRLJOBCD = VE.TTXNDTL[i].MTRLJOBCD;
                                    //            }
                                    //            else
                                    //            {
                                    //                TTXNDTL.MTRLJOBCD = VE.MENU_PARA == "WAS" ? "WA" : "FS";
                                    //            }
                                    //            TTXNDTL.ITCD = VE.TTXNDTL[i].ITCD;
                                    //            TTXNDTL.SIZECD = helpM[j].SIZECD;
                                    //            TTXNDTL.COLRCD = helpM[j].COLRCD;
                                    //            TTXNDTL.PARTCD = helpM[j].COLRCD;
                                    //            TTXNDTL.PARTCD = VE.TTXNDTL[i].PARTCD;
                                    //            TTXNDTL.QNTY = helpM[j].QNTY;
                                    //            IN_TOTAL_QNTY = IN_TOTAL_QNTY + helpM[j].QNTY.Value;
                                    //            DB.T_TXNDTL.Add(TTXNDTL);
                                    //        }
                                    //    }
                                    //}
                                    //else 
                                    if (VE.TTXNDTL[i].QNTY != 0)
                                    {
                                        COUNTER_IN = COUNTER_IN + 1;
                                        T_TXNDTL TTXNDTL = new T_TXNDTL();
                                        TTXNDTL.EMD_NO = TTXN.EMD_NO;
                                        TTXNDTL.CLCD = TTXN.CLCD;
                                        TTXNDTL.DTAG = TTXN.DTAG;
                                        TTXNDTL.TTAG = TTXN.TTAG;
                                        TTXNDTL.AUTONO = TTXN.AUTONO;
                                        //TTXNDTL.DOCCD = TTXN.DOCCD;
                                        //TTXNDTL.DOCNO = TTXN.DOCNO;
                                        //TTXNDTL.DOCDT = TTXN.DOCDT;
                                        TTXNDTL.SLNO = Convert.ToInt16(COUNTER_IN);
                                        TTXNDTL.STKDRCR = "D";
                                        TTXNDTL.STKTYPE = VE.TTXNDTL[i].STKTYPE;
                                        TTXNDTL.GOCD = TTXN.GOCD;
                                        if (!string.IsNullOrEmpty(VE.TTXNDTL[i].MTRLJOBCD))
                                        {
                                            TTXNDTL.MTRLJOBCD = VE.TTXNDTL[i].MTRLJOBCD;
                                        }
                                        else { TTXNDTL.MTRLJOBCD = VE.MENU_PARA == "WAS" ? "WA" : "FS"; }
                                        TTXNDTL.ITCD = VE.TTXNDTL[i].ITCD;
                                        TTXNDTL.PARTCD = VE.TTXNDTL[i].PARTCD;
                                        TTXNDTL.QNTY = VE.TTXNDTL[i].QNTY.Value;
                                        IN_TOTAL_QNTY = IN_TOTAL_QNTY + VE.TTXNDTL[i].QNTY.Value;
                                        DB.T_TXNDTL.Add(TTXNDTL);
                                    }
                                }
                            }
                            #endregion
                            #region OUT TAB ENTRY
                            int COUNTER_OUT = 0; double OUT_TOTAL_QNTY = 0;
                            if (VE.TTXNDTL_OUT != null)
                            {
                                for (int i = 0; i <= VE.TTXNDTL_OUT.Count - 1; i++)
                                {
                                    if (VE.TTXNDTL_OUT[i].SLNO != 0 && VE.TTXNDTL_OUT[i].ITCD != null)
                                    {
                                        //if (VE.TTXNDTL_OUT[i].ChildData != null)
                                        //{
                                        //    var SIZE_CODE = VE.TTXNDTL_OUT[i].ALL_SIZE.Split(',');
                                        //    string data = VE.TTXNDTL_OUT[i].ChildData;
                                        //    var helpM = new List<Improvar.Models.TTXNDTL_OUT_SIZE>();
                                        //    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                        //    helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.TTXNDTL_OUT_SIZE>>(data);
                                        //    helpM = helpM.Where(a => SIZE_CODE.Contains(a.SIZECD)).ToList();
                                        //    for (int j = 0; j <= helpM.Count - 1; j++)
                                        //    {
                                        //        if (helpM[j].SLNO != 0 && helpM[j].QNTY != null && helpM[j].QNTY != 0)
                                        //        {
                                        //            COUNTER_OUT = COUNTER_OUT + 1;
                                        //            T_TXNDTL TTXNDTL_OUT = new T_TXNDTL();
                                        //            TTXNDTL_OUT.EMD_NO = TTXN.EMD_NO;
                                        //            TTXNDTL_OUT.CLCD = TTXN.CLCD;
                                        //            TTXNDTL_OUT.DTAG = TTXN.DTAG;
                                        //            TTXNDTL_OUT.TTAG = TTXN.TTAG;
                                        //            TTXNDTL_OUT.AUTONO = TTXN.AUTONO;
                                        //            TTXNDTL_OUT.DOCCD = TTXN.DOCCD;
                                        //            TTXNDTL_OUT.DOCNO = TTXN.DOCNO;
                                        //            TTXNDTL_OUT.DOCDT = TTXN.DOCDT;
                                        //            TTXNDTL_OUT.SLNO = Convert.ToInt16(COUNTER_OUT);
                                        //            TTXNDTL_OUT.STKDRCR = "C";
                                        //            TTXNDTL_OUT.STKTYPE = VE.TTXNDTL_OUT[i].STKTYPE;
                                        //            if (!string.IsNullOrEmpty(VE.TTXNDTL_OUT[i].MTRLJOBCD))
                                        //            {
                                        //                TTXNDTL_OUT.MTRLJOBCD = VE.TTXNDTL_OUT[i].MTRLJOBCD;
                                        //            }
                                        //            else
                                        //            {
                                        //                TTXNDTL_OUT.MTRLJOBCD = VE.MENU_PARA == "WAS" ? "WA" : "FS";
                                        //            }
                                        //            TTXNDTL_OUT.GOCD = TTXN.GOCD;
                                        //            TTXNDTL_OUT.ITCD = VE.TTXNDTL_OUT[i].ITCD;
                                        //            TTXNDTL_OUT.SIZECD = helpM[j].SIZECD;
                                        //            TTXNDTL_OUT.COLRCD = helpM[j].COLRCD;
                                        //            TTXNDTL_OUT.PARTCD = VE.TTXNDTL_OUT[i].PARTCD;
                                        //            TTXNDTL_OUT.QNTY = helpM[j].QNTY.retDbl();
                                        //            OUT_TOTAL_QNTY = OUT_TOTAL_QNTY + helpM[j].QNTY.Value;
                                        //            DB.T_TXNDTL.Add(TTXNDTL_OUT);
                                        //        }
                                        //    }
                                        //}
                                        //else 
                                        if (VE.TTXNDTL_OUT[i].QNTY != 0)
                                        {
                                            COUNTER_OUT = COUNTER_OUT + 1;
                                            T_TXNDTL TTXNDTL_OUT = new T_TXNDTL();
                                            TTXNDTL_OUT.EMD_NO = TTXN.EMD_NO;
                                            TTXNDTL_OUT.CLCD = TTXN.CLCD;
                                            TTXNDTL_OUT.DTAG = TTXN.DTAG;
                                            TTXNDTL_OUT.TTAG = TTXN.TTAG;
                                            TTXNDTL_OUT.AUTONO = TTXN.AUTONO;
                                            //TTXNDTL_OUT.DOCCD = TTXN.DOCCD;
                                            //TTXNDTL_OUT.DOCNO = TTXN.DOCNO;
                                            //TTXNDTL_OUT.DOCDT = TTXN.DOCDT;
                                            TTXNDTL_OUT.SLNO = Convert.ToInt16(COUNTER_OUT);
                                            TTXNDTL_OUT.STKDRCR = "C";
                                            TTXNDTL_OUT.STKTYPE = VE.TTXNDTL_OUT[i].STKTYPE;
                                            TTXNDTL_OUT.GOCD = TTXN.GOCD;
                                            if (!string.IsNullOrEmpty(VE.TTXNDTL_OUT[i].MTRLJOBCD))
                                            {
                                                TTXNDTL_OUT.MTRLJOBCD = VE.TTXNDTL_OUT[i].MTRLJOBCD;
                                            }
                                            else { TTXNDTL_OUT.MTRLJOBCD = VE.MENU_PARA == "WAS" ? "WA" : "FS"; }
                                            TTXNDTL_OUT.ITCD = VE.TTXNDTL_OUT[i].ITCD;
                                            TTXNDTL_OUT.PARTCD = VE.TTXNDTL_OUT[i].PARTCD;
                                            TTXNDTL_OUT.QNTY = VE.TTXNDTL_OUT[i].QNTY.Value;
                                            IN_TOTAL_QNTY = IN_TOTAL_QNTY + VE.TTXNDTL_OUT[i].QNTY.Value;
                                            DB.T_TXNDTL.Add(TTXNDTL_OUT);
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region COMMON DATA ENTRY
                            if (VE.UploadDOC != null)
                            {
                                var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TTXN.AUTONO, TTXN.EMD_NO.Value);
                                if (img.Item1.Count != 0)
                                {
                                    DB.T_CNTRL_HDR_DOC.AddRange(img.Item1);
                                    DB.T_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                                }
                            }
                            if (VE.T_CNTRL_HDR_REM.DOCREM != null)
                            {
                                var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TTXN.AUTONO, TTXN.CLCD, TTXN.EMD_NO.Value);
                                if (NOTE.Item1.Count != 0)
                                {
                                    DB.T_CNTRL_HDR_REM.AddRange(NOTE.Item1);
                                }
                            }
                            #endregion
                            if (VE.MENU_PARA == "CNV")
                            {
                                if (IN_TOTAL_QNTY != OUT_TOTAL_QNTY)
                                {
                                    transaction.Rollback();
                                    return Content("Total Quantity of Out Tab and Total Quantity of In Tab Doesn't Match, Total Quantity must be Equal ! Please Maintain the Ratio !!");
                                }
                            }

                            //#region STOCK CHECKING INTAB
                            //var ITCD = string.Join(",", (from Z in VE.TTXNDTL select "'" + Z.ITCD + "'").Distinct());
                            //var stocktype = string.Join(",", (from Z in VE.TTXNDTL where Z.STKTYPE.retStr() != "" select "'" + Z.STKTYPE + "'").Distinct());
                            //var mtrljobcd = string.Join(",", (from Z in VE.TTXNDTL where Z.MTRLJOBCD.retStr() != "" select "'" + Z.MTRLJOBCD + "'").Distinct());
                            //if (mtrljobcd.retStr() == "")
                            //{
                            //    mtrljobcd = "'FS'";
                            //}
                            //var ITEM_STOCK_DATA = Salesfunc.GetStock(VE.T_TXN.DOCDT.Value.ToString("dd/MM/yyyy"), "'" + VE.T_TXN.GOCD + "'", ITCD, mtrljobcd, (VE.T_TXN.AUTONO == null ? "" : VE.T_TXN.AUTONO), "", "", "", "", stocktype, "", "", "", true, true);

                            //string ERROR_MESSAGE = ""; int ERROR_COUNT = 0;
                            //if (VE.TTXNDTL != null && VE.TTXNDTL.Count > 0)
                            //{
                            //    for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                            //    {
                            //        string STKTYPE = VE.TTXNDTL[i].STKTYPE;
                            //        string ITEM = VE.TTXNDTL[i].ITCD;

                            //        string QNTY = "";

                            //        if (VE.TTXNDTL[i].ALL_SIZE.retStr() != "")
                            //        {
                            //            string[] SIZE = VE.TTXNDTL[i].ALL_SIZE.Split(',');
                            //            if (VE.TTXNDTL[i].ChildData != null)
                            //            {
                            //                string data = VE.TTXNDTL[i].ChildData;
                            //                var helpM = new List<Improvar.Models.TTXNDTL_OUT_SIZE>();
                            //                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            //                helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.TTXNDTL_OUT_SIZE>>(data);
                            //                if (SIZE != null) helpM = helpM.Where(a => SIZE.Contains(a.SIZECD)).ToList();
                            //                for (int j = 0; j <= helpM.Count - 1; j++)
                            //                {
                            //                    if (helpM[j].SLNO != 0 && helpM[j].QNTY != null && helpM[j].QNTY != 0)
                            //                    {
                            //                        QNTY = "";
                            //                        var vQNTY = ITEM_STOCK_DATA.AsEnumerable()
                            //                           .Where(g => g.Field<string>("itcd") == ITEM && g.Field<string>("stktype") == STKTYPE && g.Field<string>("sizecd") == helpM[j].SIZECD)
                            //                               .GroupBy(g => new { itcd = g["itcd"], stktype = g["stktype"] })
                            //                                .Select(g =>
                            //                                {
                            //                                    var row = ITEM_STOCK_DATA.NewRow();
                            //                                    row["qnty"] = g.Sum(r => r.Field<decimal>("qnty"));
                            //                                    return row;
                            //                                });
                            //                        if (vQNTY != null && vQNTY.Count() > 0)
                            //                        {
                            //                            var vQNTY1 = vQNTY.CopyToDataTable();
                            //                            QNTY = vQNTY1.Rows[0]["qnty"].retStr();
                            //                        }

                            //                        var STOCK_QNTY = QNTY.retDbl(); double SHORTAGE_QNTY = 0;
                            //                        STOCK_QNTY = STOCK_QNTY + helpM[j].QNTY.retDbl();
                            //                        if (STOCK_QNTY < 0)
                            //                        {
                            //                            ERROR_COUNT += 1; SHORTAGE_QNTY = QNTY.retDbl() - helpM[j].QNTY.retDbl();

                            //                            ERROR_MESSAGE = ERROR_MESSAGE + "(" + ERROR_COUNT + ") " + "Article Number : " + VE.TTXNDTL[i].STYLENO + " (" + VE.TTXNDTL[i].ITNM + ")" + " , Stock Type : " + VE.TTXNDTL[i].STKTYPE + " , Size : " + helpM[j].SIZECD.retStr() + " , Stock Quantity : " + QNTY.retDbl() + ", Entered Quantity : " + helpM[j].QNTY + ", Shortage Quantity : " + SHORTAGE_QNTY + "</br>";
                            //                        }

                            //                    }
                            //                }

                            //            }
                            //        }
                            //        else
                            //        {
                            //            var vQNTY = ITEM_STOCK_DATA.AsEnumerable()
                            //                                 .Where(g => g.Field<string>("itcd") == ITEM && g.Field<string>("stktype") == STKTYPE)
                            //                                     .GroupBy(g => new { itcd = g["itcd"], stktype = g["stktype"] })
                            //                                      .Select(g =>
                            //                                      {
                            //                                          var row = ITEM_STOCK_DATA.NewRow();
                            //                                          row["qnty"] = g.Sum(r => r.Field<decimal>("qnty"));
                            //                                          return row;
                            //                                      });
                            //            if (vQNTY != null && vQNTY.Count() > 0)
                            //            {
                            //                var vQNTY1 = vQNTY.CopyToDataTable();
                            //                QNTY = vQNTY1.Rows[0]["qnty"].retStr();
                            //            }

                            //            var STOCK_QNTY = QNTY.retDbl(); double SHORTAGE_QNTY = 0;
                            //            STOCK_QNTY = STOCK_QNTY + VE.TTXNDTL[i].QNTY.retDbl();
                            //            if (STOCK_QNTY < 0)
                            //            {
                            //                ERROR_COUNT += 1; SHORTAGE_QNTY = QNTY.retDbl() - VE.TTXNDTL[i].QNTY.retDbl();

                            //                ERROR_MESSAGE = ERROR_MESSAGE + "(" + ERROR_COUNT + ") " + "Article Number : " + VE.TTXNDTL[i].STYLENO + " (" + VE.TTXNDTL[i].ITNM + ")" + " , Stock Type : " + VE.TTXNDTL[i].STKTYPE + " , Stock Quantity : " + QNTY.retDbl() + ", Entered Quantity : " + VE.TTXNDTL[i].QNTY + ", Shortage Quantity : " + SHORTAGE_QNTY + "</br>";
                            //            }
                            //        }





                            //    }
                            //    if (ERROR_MESSAGE.Length > 0)
                            //    {
                            //        transaction.Rollback();
                            //        return Content("Entry Can't Save ! Stock Not Available in In Tab for Following :-</br></br>" + ERROR_MESSAGE + "");
                            //    }

                            //}
                            //#endregion

                            //#region STOCK CHECKING OUTTAB
                            //ITCD = string.Join(",", (from Z in VE.TTXNDTL_OUT select "'" + Z.ITCD + "'").Distinct());
                            //stocktype = string.Join(",", (from Z in VE.TTXNDTL_OUT where Z.STKTYPE.retStr() != "" select "'" + Z.STKTYPE + "'").Distinct());
                            //mtrljobcd = string.Join(",", (from Z in VE.TTXNDTL_OUT where Z.MTRLJOBCD.retStr() != "" select "'" + Z.MTRLJOBCD + "'").Distinct());
                            //if (mtrljobcd.retStr() == "")
                            //{
                            //    mtrljobcd = "'FS'";
                            //}
                            //ITEM_STOCK_DATA = Salesfunc.GetStock(VE.T_TXN.DOCDT.Value.ToString("dd/MM/yyyy"), "'" + VE.T_TXN.GOCD + "'", ITCD, mtrljobcd, (VE.T_TXN.AUTONO == null ? "" : VE.T_TXN.AUTONO), "", "", "", "", stocktype, "", "", "", true);

                            //ERROR_MESSAGE = ""; ERROR_COUNT = 0;
                            //if (VE.TTXNDTL_OUT != null && VE.TTXNDTL_OUT.Count > 0)
                            //{
                            //    for (int i = 0; i <= VE.TTXNDTL_OUT.Count - 1; i++)
                            //    {
                            //        string STKTYPE = VE.TTXNDTL_OUT[i].STKTYPE;
                            //        string ITEM = VE.TTXNDTL_OUT[i].ITCD;

                            //        string QNTY = "";

                            //        if (VE.TTXNDTL_OUT[i].ALL_SIZE.retStr() != "")
                            //        {
                            //            string[] SIZE = VE.TTXNDTL_OUT[i].ALL_SIZE.Split(',');
                            //            if (VE.TTXNDTL_OUT[i].ChildData != null)
                            //            {
                            //                string data = VE.TTXNDTL_OUT[i].ChildData;
                            //                var helpM = new List<Improvar.Models.TTXNDTL_OUT_SIZE>();
                            //                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            //                helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.TTXNDTL_OUT_SIZE>>(data);
                            //                if (SIZE != null) helpM = helpM.Where(a => SIZE.Contains(a.SIZECD)).ToList();
                            //                for (int j = 0; j <= helpM.Count - 1; j++)
                            //                {
                            //                    if (helpM[j].SLNO != 0 && helpM[j].QNTY != null && helpM[j].QNTY != 0)
                            //                    {
                            //                        QNTY = "";
                            //                        var vQNTY = ITEM_STOCK_DATA.AsEnumerable()
                            //                           .Where(g => g.Field<string>("itcd") == ITEM && g.Field<string>("stktype") == STKTYPE && g.Field<string>("sizecd") == helpM[j].SIZECD)
                            //                               .GroupBy(g => new { itcd = g["itcd"], stktype = g["stktype"] })
                            //                                .Select(g =>
                            //                                {
                            //                                    var row = ITEM_STOCK_DATA.NewRow();
                            //                                    row["qnty"] = g.Sum(r => r.Field<decimal>("qnty"));
                            //                                    return row;
                            //                                });
                            //                        if (vQNTY != null && vQNTY.Count() > 0)
                            //                        {
                            //                            var vQNTY1 = vQNTY.CopyToDataTable();
                            //                            QNTY = vQNTY1.Rows[0]["qnty"].retStr();
                            //                        }

                            //                        var STOCK_QNTY = QNTY.retDbl(); double SHORTAGE_QNTY = 0;
                            //                        STOCK_QNTY = STOCK_QNTY - helpM[j].QNTY.retDbl();
                            //                        if (STOCK_QNTY < 0)
                            //                        {
                            //                            ERROR_COUNT += 1; SHORTAGE_QNTY = QNTY.retDbl() - helpM[j].QNTY.retDbl();

                            //                            ERROR_MESSAGE = ERROR_MESSAGE + "(" + ERROR_COUNT + ") " + "Article Number : " + VE.TTXNDTL_OUT[i].STYLENO + " (" + VE.TTXNDTL_OUT[i].ITNM + ")" + " , Stock Type : " + VE.TTXNDTL_OUT[i].STKTYPE + " , Size : " + helpM[j].SIZECD.retStr() + " , Stock Quantity : " + QNTY.retDbl() + ", Entered Quantity : " + helpM[j].QNTY + ", Shortage Quantity : " + SHORTAGE_QNTY + "</br>";
                            //                        }

                            //                    }
                            //                }

                            //            }
                            //        }
                            //        else
                            //        {
                            //            var vQNTY = ITEM_STOCK_DATA.AsEnumerable()
                            //                                 .Where(g => g.Field<string>("itcd") == ITEM && g.Field<string>("stktype") == STKTYPE)
                            //                                     .GroupBy(g => new { itcd = g["itcd"], stktype = g["stktype"] })
                            //                                      .Select(g =>
                            //                                      {
                            //                                          var row = ITEM_STOCK_DATA.NewRow();
                            //                                          row["qnty"] = g.Sum(r => r.Field<decimal>("qnty"));
                            //                                          return row;
                            //                                      });
                            //            if (vQNTY != null && vQNTY.Count() > 0)
                            //            {
                            //                var vQNTY1 = vQNTY.CopyToDataTable();
                            //                QNTY = vQNTY1.Rows[0]["qnty"].retStr();
                            //            }

                            //            var STOCK_QNTY = QNTY.retDbl(); double SHORTAGE_QNTY = 0;
                            //            STOCK_QNTY = STOCK_QNTY - VE.TTXNDTL_OUT[i].QNTY.retDbl();
                            //            if (STOCK_QNTY < 0)
                            //            {
                            //                ERROR_COUNT += 1; SHORTAGE_QNTY = QNTY.retDbl() - VE.TTXNDTL_OUT[i].QNTY.retDbl();

                            //                ERROR_MESSAGE = ERROR_MESSAGE + "(" + ERROR_COUNT + ") " + "Article Number : " + VE.TTXNDTL_OUT[i].STYLENO + " (" + VE.TTXNDTL_OUT[i].ITNM + ")" + " , Stock Type : " + VE.TTXNDTL_OUT[i].STKTYPE + " , Stock Quantity : " + QNTY.retDbl() + ", Entered Quantity : " + VE.TTXNDTL_OUT[i].QNTY + ", Shortage Quantity : " + SHORTAGE_QNTY + "</br>";
                            //            }
                            //        }





                            //    }
                            //    if (ERROR_MESSAGE.Length > 0)
                            //    {
                            //        transaction.Rollback();
                            //        return Content("Entry Can't Save ! Stock Not Available in Out Tab for Following :-</br></br>" + ERROR_MESSAGE + "");
                            //    }

                            //}
                            //#endregion
                            DB.SaveChanges();
                            ModelState.Clear();
                            transaction.Commit();
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
                            T_CNTRL_HDR TCH = Cn.T_CONTROL_HDR(VE.T_TXN.DOCCD, VE.T_TXN.DOCDT, VE.T_TXN.DOCNO, VE.T_TXN.AUTONO, "", "", VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, null, 0, null);
                            DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                            DB.SaveChanges();

                            DB.T_TXN.Where(x => x.AUTONO == VE.T_TXN.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            DB.T_TXNDTL.Where(x => x.AUTONO == VE.T_TXN.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            DB.T_TXNOTH.Where(x => x.AUTONO == VE.T_TXN.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_TXN.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.T_TXN.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.T_TXN.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == VE.T_TXN.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.T_TXN.AUTONO));
                            DB.SaveChanges();

                            DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.T_TXN.AUTONO));
                            DB.SaveChanges();

                            DB.T_CNTRL_HDR_REM.RemoveRange(DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == VE.T_TXN.AUTONO));
                            DB.SaveChanges();

                            DB.T_TXNDTL.RemoveRange(DB.T_TXNDTL.Where(x => x.AUTONO == VE.T_TXN.AUTONO));
                            DB.SaveChanges();

                            DB.T_TXNOTH.RemoveRange(DB.T_TXNOTH.Where(x => x.AUTONO == VE.T_TXN.AUTONO));
                            DB.SaveChanges();

                            DB.T_TXN.RemoveRange(DB.T_TXN.Where(x => x.AUTONO == VE.T_TXN.AUTONO));
                            DB.SaveChanges();

                            DB.T_CNTRL_HDR.RemoveRange(DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_TXN.AUTONO));
                            DB.SaveChanges();

                            ModelState.Clear();
                            transaction.Commit();
                            return Content("3");
                        }
                        else
                        {
                            return Content("");
                        }
                    }
                    catch (DbEntityValidationException ex)
                    {
                        transaction.Rollback();
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
                    transaction.Rollback();
                    ModelState.Clear();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
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