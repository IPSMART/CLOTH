using Improvar.Models;
using Improvar.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Improvar.Controllers
{
    public class T_SORDCANCController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        SchemeCal Scheme_Cal = new SchemeCal();
        Salesfunc SALES_FUNC = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_SORDCANC

        T_SORD_CANC sl; T_DO_CANC sl1; T_CNTRL_HDR sll; T_CNTRL_HDR_REM TCHR; M_JOBMST MJOB; M_FLRLOCA MFLOOR; M_DOCTYPE DOCTYP;
        public ActionResult T_SORDCANC(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    OrderCancelEntry VE = (parkID == "") ? new OrderCancelEntry() : (Improvar.ViewModels.OrderCancelEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.DOC_CODE)
                    {
                        case "SOCAN":
                            ViewBag.formname = "Sales Order Cancel Part / Full"; ViewBag.GridHeader = "Sales Order Cancel"; break;
                        case "DOCAN":
                            ViewBag.formname = "Delivery Order Cancel Part / Full"; ViewBag.GridHeader = "Delivery Order Cancel"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    string COMP = CommVar.Compcd(UNQSNO);
                    string LOC = CommVar.Loccd(UNQSNO);
                    string yr1 = CommVar.YearCode(UNQSNO);
                    VE.DropDown_list = Master_Help.OTHER_REC_MODE();
                    List<DropDown_list1> DDL = new List<DropDown_list1>();
                    VE.DropDown_list1 = DDL;
                    List<DropDown_list4> DDL4 = new List<DropDown_list4>();
                    VE.DropDown_list4 = DDL4;
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    String YR1 = CommVar.YearCode(UNQSNO);
                    if (op.Length != 0)
                    {
                        string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();
                        if (VE.DOC_CODE == "SOCAN")
                        {
                            VE.IndexKey = (from p in DB.T_SORD_CANC join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO) where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COMP && q.YR_CD == YR1 select new IndexKey() { Navikey = p.AUTONO }).OrderBy(a => a.Navikey).ToList();
                        }
                        else
                        {
                            VE.IndexKey = (from p in DB.T_DO_CANC join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO) where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COMP && q.YR_CD == YR1 select new IndexKey() { Navikey = p.AUTONO }).OrderBy(a => a.Navikey).ToList();
                        }

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
                            if (VE.DOC_CODE == "SOCAN")
                            {
                                VE.T_SORD_CANC = sl;
                            }
                            else
                            {
                                VE.T_DO_CANC = sl1;
                            }
                            VE.T_CNTRL_HDR = sll;
                            VE.T_CNTRL_HDR_REM = TCHR;
                            VE.M_DOCTYPE = DOCTYP;
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {
                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = doctP;
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;

                                List<TSORDDTL_CANC> TSORDDTL_CANC = new List<TSORDDTL_CANC>();
                                for (int i = 0; i <= 99; i++)
                                {
                                    TSORDDTL_CANC SORDDTL = new TSORDDTL_CANC();
                                    SORDDTL.SLNO = Convert.ToInt16(i + 1);
                                    SORDDTL.DropDown_list2 = Master_Help.STOCK_TYPE();
                                    SORDDTL.DropDown_list3 = Master_Help.FREE_STOCK();
                                    TSORDDTL_CANC.Add(SORDDTL);
                                }
                                VE.TSORDDTL_CANC = TSORDDTL_CANC;
                                if (VE.DOC_CODE == "SOCAN")
                                {
                                    T_SORD_CANC TSORDCANC = new T_SORD_CANC();
                                    TSORDCANC.DOCDT = System.DateTime.Now.Date;
                                    VE.T_SORD_CANC = TSORDCANC;
                                }
                                else
                                {
                                    T_DO_CANC TDOCANC = new T_DO_CANC();
                                    TDOCANC.DOCDT = System.DateTime.Now.Date;
                                    VE.T_DO_CANC = TDOCANC;
                                }
                                if (VE.DocumentType.Count > 0)
                                {
                                    VE.DOC_ID = VE.DocumentType[0].value;
                                }
                            }
                            else
                            {
                                if (VE.UploadDOC != null)
                                {
                                    foreach (var i in VE.UploadDOC)
                                    {
                                        i.DocumentType = doctP;
                                    }
                                }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (OrderCancelEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COMP, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        string docdt = "";
                        if (VE.T_CNTRL_HDR != null) if (VE.T_CNTRL_HDR.DOCDT != null) docdt = VE.T_CNTRL_HDR.DOCDT.ToString().Remove(10);
                        if (VE.DOC_CODE == "SOCAN")
                        {
                            Cn.getdocmaxmindate(VE.T_SORD_CANC.DOCCD, docdt, VE.DefaultAction, VE.T_SORD_CANC.DOCNO, VE);
                        }
                        else
                        {
                            Cn.getdocmaxmindate(VE.T_DO_CANC.DOCCD, docdt, VE.DefaultAction, VE.T_DO_CANC.DOCNO, VE);
                        }
                        ModelState.Clear();
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
                OrderCancelEntry VE = new OrderCancelEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public OrderCancelEntry Navigation(OrderCancelEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            sl = new T_SORD_CANC(); sl1 = new T_DO_CANC(); sll = new T_CNTRL_HDR(); TCHR = new T_CNTRL_HDR_REM(); MJOB = new M_JOBMST(); MFLOOR = new M_FLRLOCA();
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
                string AUTO_NO = ""; string DOC_CODE = "";
                if (VE.DOC_CODE == "SOCAN")
                {
                    sl = DB.T_SORD_CANC.Find(aa[0].Trim());
                    AUTO_NO = sl.AUTONO;
                    DOC_CODE = sl.DOCCD;
                }
                else
                {
                    sl1 = DB.T_DO_CANC.Find(aa[0].Trim());
                    AUTO_NO = sl1.AUTONO;
                    DOC_CODE = sl1.DOCCD;
                }
                sll = DB.T_CNTRL_HDR.Find(AUTO_NO);
                DOCTYP = DB.M_DOCTYPE.Find(DOC_CODE);
                TCHR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), AUTO_NO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), AUTO_NO);
                if (sl != null)
                {
                    VE.DOC_ID = sl.DOCCD;
                    var Party = DBF.M_SUBLEG.Find(sl.SLCD); if (Party != null) { VE.PartyName = Party.SLNM; }
                }
                if (VE.DOC_CODE == "SOCAN")
                {
                    VE.TSORDDTL_CANC = (from i in DB.T_SORDDTL
                                        join j in DB.M_SITEM on i.ITCD equals j.ITCD into x
                                        from j in x.DefaultIfEmpty()
                                        join k in DB.M_SIZE on i.SIZECD equals k.SIZECD into y
                                        from k in y.DefaultIfEmpty()
                                        join l in DB.M_COLOR on i.COLRCD equals l.COLRCD into z
                                        from l in z.DefaultIfEmpty()
                                        where i.AUTONO == AUTO_NO
                                        select new TSORDDTL_CANC()
                                        {
                                            AUTONO = i.AUTONO,
                                            SLNO = i.SLNO,
                                            EMD_NO = i.EMD_NO,
                                            CLCD = i.CLCD,
                                            DTAG = i.DTAG,
                                            TTAG = i.TTAG,
                                            STKDRCR = i.STKDRCR,
                                            STKTYPE = i.STKTYPE,
                                            STKTYP_HIDDEN = i.STKTYPE,
                                            FREESTK = i.FREESTK,
                                            FREESTK_HIDDEN = i.FREESTK,
                                            ITCD = i.ITCD,
                                            ITNM = j.ITNM,
                                            ARTNO = j.STYLENO,
                                            //TOTAL_PCS = j.PCSPERBOX,
                                            UOM = j.UOMCD,
                                            SIZECD = i.SIZECD,
                                            ALL_SIZE = i.SIZECD,
                                            SIZENM = k.SIZENM,
                                            COLRCD = i.COLRCD,
                                            COLRNM = l.COLRNM,
                                            CANCQNTY = i.QNTY,
                                            DISCAMT = i.DISCAMT,
                                            ORDAUTONO = i.ORDAUTONO
                                        }).OrderBy(s => s.SLNO).ToList();
                }
                else
                {
                    VE.TSORDDTL_CANC = (from i in DB.T_DODTL
                                        join j in DB.M_SITEM on i.ITCD equals j.ITCD into x
                                        from j in x.DefaultIfEmpty()
                                        join k in DB.M_SIZE on i.SIZECD equals k.SIZECD into y
                                        from k in y.DefaultIfEmpty()
                                        join l in DB.M_COLOR on i.COLRCD equals l.COLRCD into z
                                        from l in z.DefaultIfEmpty()
                                        where i.AUTONO == AUTO_NO
                                        select new TSORDDTL_CANC()
                                        {
                                            AUTONO = i.AUTONO,
                                            SLNO = i.SLNO,
                                            EMD_NO = i.EMD_NO.retShort(),
                                            CLCD = i.CLCD,
                                            DTAG = i.DTAG,
                                            TTAG = i.TTAG,
                                            STKDRCR = i.STKDRCR,
                                            STKTYPE = i.STKTYPE,
                                            STKTYP_HIDDEN = i.STKTYPE,
                                            ITCD = i.ITCD,
                                            ITNM = j.ITNM,
                                            ARTNO = j.STYLENO,
                                            //TOTAL_PCS = j.PCSPERBOX,
                                            UOM = j.UOMCD,
                                            SIZECD = i.SIZECD,
                                            ALL_SIZE = i.SIZECD,
                                            SIZENM = k.SIZENM,
                                            COLRCD = i.COLRCD,
                                            COLRNM = l.COLRNM,
                                            CANCQNTY = i.QNTY,
                                            DISCAMT = i.DISCAMT,
                                            ORDAUTONO = i.ORDAUTONO
                                        }).OrderBy(s => s.SLNO).ToList();
                }
                foreach (var z in VE.TSORDDTL_CANC)
                {
                    string ITEM = z.ITCD; string STK_TYPE = z.STKTYPE; string FREE_STK = z.FREESTK;
                    VE.TSORDDTL_CANC_SIZE = (from i in VE.TSORDDTL_CANC
                                             join j in DB.M_SIZE on i.SIZECD equals j.SIZECD into x
                                             from j in x.DefaultIfEmpty()
                                             where i.ITCD == ITEM && i.STKTYPE == STK_TYPE && i.FREESTK == FREE_STK
                                             select new TSORDDTL_CANC_SIZE()
                                             {
                                                 PRINT_SEQ = j.PRINT_SEQ,
                                                 SIZECD = i.SIZECD,
                                                 SIZENM = j.SIZENM,
                                                 COLRCD = i.COLRCD,
                                                 COLRNM = i.COLRNM,
                                                 RATE = i.RATE,
                                                 QNTY = i.QNTY,
                                                 CANCQNTY = i.CANCQNTY,
                                                 AUTONO = i.AUTONO,
                                                 ITCD = i.ITCD,
                                                 SLNO = i.SLNO,
                                                 ITCOLSIZE = i.ITCD + i.COLRCD + i.SIZECD,
                                                 PCS_HIDDEN = z.TOTAL_PCS.Value
                                             }).OrderBy(s => s.PRINT_SEQ).ToList();
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string JR = javaScriptSerializer.Serialize(VE.TSORDDTL_CANC_SIZE);
                    z.ChildData = JR;
                }
                VE.TSORDDTL_CANC = (from p in VE.TSORDDTL_CANC
                                    group p by new { p.ORDAUTONO, p.ITCD, p.ITNM, p.ARTNO, p.TOTAL_PCS, p.NOOFSETS, p.UOM, p.STKTYPE, p.FREESTK, p.ChildData } into z
                                    select new TSORDDTL_CANC
                                    {
                                        ORDAUTONO = z.Key.ORDAUTONO,
                                        ITCD = z.Key.ITCD,
                                        ITNM = z.Key.ITNM,
                                        ARTNO = z.Key.ARTNO,
                                        TOTAL_PCS = z.Key.TOTAL_PCS,
                                        NOOFSETS = z.Key.NOOFSETS,
                                        UOM = z.Key.UOM,
                                        STKTYPE = z.Key.STKTYPE,
                                        STKTYP_HIDDEN = z.Key.STKTYPE,
                                        FREESTK = z.Key.FREESTK,
                                        FREESTK_HIDDEN = z.Key.FREESTK,
                                        ChildData = z.Key.ChildData,
                                        CANCQNTY = z.Sum(x => x.CANCQNTY),
                                        ALL_SIZE = string.Join(",", z.Select(i => i.SIZECD))
                                    }).ToList();
                VE.TSORDDTL_CANC = (from X in VE.TSORDDTL_CANC orderby X.ARTNO + X.CANCQNTY select X).ToList();
                for (int z = 0; z <= VE.TSORDDTL_CANC.Count - 1; z++)
                {
                    VE.TSORDDTL_CANC[z].DropDown_list2 = Master_Help.STOCK_TYPE();
                    VE.TSORDDTL_CANC[z].DropDown_list3 = Master_Help.FREE_STOCK();
                    VE.TSORDDTL_CANC[z].SLNO = Convert.ToInt16(z + 1);
                }
                VE.T_CNTRL_HDR = sll;
                if (VE.T_CNTRL_HDR.CANCEL == "Y") { VE.CancelRecord = true; } else { VE.CancelRecord = false; }
            }
            else
            {

            }
            return VE;
        }
        public ActionResult SearchPannelData(PackingSlipEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string COMP_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            String YR1 = CommVar.YearCode(UNQSNO);
            VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();
            var MDT = (dynamic)null;
            if (VE.DOC_CODE == "SOCAN")
            {
                MDT = (from j in DB.T_SORD_CANC
                       join p in DB.T_CNTRL_HDR on j.AUTONO equals (p.AUTONO)
                       where (XYZ.Contains(p.DOCCD) && p.AUTONO == j.AUTONO && p.LOCCD == LOC_CD && p.COMPCD == COMP_CD && p.YR_CD == YR1)
                       orderby j.AUTONO
                       select new { j.AUTONO, p.DOCNO, j.DOCDT, j.DOCCD, j.SLCD }).ToList().Select(x => new
                       {
                           AUTONO = x.AUTONO,
                           DOCNO = x.DOCNO,
                           DOCDT = x.DOCDT.Value.ToString().Replace('-', '/').Substring(0, 10),
                           DOCCD = x.DOCCD,
                           SLCD = x.SLCD,
                           SLNM = (from Z in DBF.M_SUBLEG where Z.SLCD == x.SLCD select Z.SLNM).SingleOrDefault(),
                           DISTRICT = (from Z in DBF.M_SUBLEG where Z.SLCD == x.SLCD select Z.DISTRICT).SingleOrDefault(),
                       }).Distinct().OrderBy(s => s.AUTONO).ToList();
            }
            else
            {
                MDT = (from j in DB.T_DO_CANC
                       join p in DB.T_CNTRL_HDR on j.AUTONO equals (p.AUTONO)
                       where (XYZ.Contains(p.DOCCD) && p.AUTONO == j.AUTONO && p.LOCCD == LOC_CD && p.COMPCD == COMP_CD && p.YR_CD == YR1)
                       orderby j.AUTONO
                       select new { j.AUTONO, p.DOCNO, j.DOCDT, j.DOCCD, j.SLCD }).ToList().Select(x => new
                       {
                           AUTONO = x.AUTONO,
                           DOCNO = x.DOCNO,
                           DOCDT = x.DOCDT.Value.ToString().Replace('-', '/').Substring(0, 10),
                           DOCCD = x.DOCCD,
                           SLCD = x.SLCD,
                           SLNM = (from Z in DBF.M_SUBLEG where Z.SLCD == x.SLCD select Z.SLNM).SingleOrDefault(),
                           DISTRICT = (from Z in DBF.M_SUBLEG where Z.SLCD == x.SLCD select Z.DISTRICT).SingleOrDefault(),
                       }).Distinct().OrderBy(s => s.AUTONO).ToList();
            }
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Document Code" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].DOCNO + "</td><td>" + MDT[j].DOCDT + "</td><td>" + MDT[j].DOCCD + "</td><td>" + MDT[j].AUTONO + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "3", "3"));
        }
        public ActionResult changeDocumentType(Permission VE, string DOC_CD, string DOCDT = "", bool AllowBDATE = false, string DOCNO = "")
        {
            Cn.getQueryString(VE);
            string str = "";
            if (DOC_CD != "0")
            {
                str = str + Cn.GCS() + "^" + Cn.GCS() + Cn.getdocmaxmindate(DOC_CD, DOCDT, VE.DefaultAction, DOCNO, VE);
            }
            return Content(str);

        }
        public ActionResult GetPartyDetails(OrderCancelEntry VE, string val)
        {
            try
            {
                Cn.getQueryString(VE);
                string LINK = ""; if (VE.MENU_PARA == "SB") { LINK = "D"; } else { LINK = "C"; }
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.SUBLEDGER(val, LINK));
                }
                else
                {
                    string str = Master_Help.SUBLEDGER(val, LINK);
                    return Content(str);
                }

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult GetOrderDetails(OrderCancelEntry VE, string Code)
        {
            try
            {
                string[] VALUE = Code.Split(Convert.ToChar(Cn.GCS()));
                Cn.getQueryString(VE);

                DataTable PENDING_ORDER = SALES_FUNC.GetPendOrder(VALUE[0], VALUE[1], "", "", "","", VE.MENU_PARA, "");

                var query = (from DataRow dr in PENDING_ORDER.Rows
                             select new
                             {
                                 AUTONO = dr["AUTONO"].ToString(),
                                 ORDNO = dr["DOCNO"].ToString(),
                                 ORDDT = dr["DOCDT"] == null ? "" : dr["DOCDT"].ToString().Substring(0, 10),
                             }).DistinctBy(a => a.AUTONO).ToList();

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].ORDNO + "</td><td>" + query[i].ORDDT + "</td></tr>");
                }
                var hdr = "Order Number" + Cn.GCS() + "Order Date";
                return PartialView("_Help2", Master_Help.Generate_help(hdr, SB.ToString()));

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult GetGridData(OrderCancelEntry VE, string PARTY, string DOC_DT, string ORDERNO, string ORDERASONDATE)
        {
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

                var ORD_AUTO = (dynamic)null;
                if (ORDERNO != "")
                {
                    ORD_AUTO = (from X in DB.T_CNTRL_HDR where X.DOCNO == ORDERNO select X.AUTONO).ToList();
                }

                DataTable PENDING_ORDER = SALES_FUNC.GetPendOrder(PARTY, ORDERASONDATE, ORD_AUTO == null || ORD_AUTO.Count == 0 ? "" : "'" + ORD_AUTO[0] + "'" != null && ORD_AUTO.Count > 0 ? "'" + ORD_AUTO[0] + "'" : "", DOC_DT, "","", VE.MENU_PARA, "");
                VE.TSORDDTL_CANC = (from DataRow dr in PENDING_ORDER.Rows
                                    select new TSORDDTL_CANC
                                    {
                                        ORDAUTONO = dr["AUTONO"].ToString(),
                                        ARTNO = dr["STYLENO"].ToString(),
                                        ITCD = dr["ITCD"].ToString(),
                                        ITNM = dr["ITNM"].ToString(),
                                        SIZECD = dr["SIZECD"].ToString(),
                                        SIZENM = dr["SIZENM"].ToString(),
                                        COLRCD = dr["COLRCD"].ToString(),
                                        COLRNM = dr["COLRNM"].ToString(),
                                        ALL_SIZE = dr["SIZECD"].ToString(),
                                        TOTAL_PCS = dr["PCSPERBOX"] == null ? 0 : Convert.ToDouble(dr["PCSPERBOX"].ToString()),
                                        UOM = dr["UOMCD"].ToString(),
                                        STKTYPE = dr["STKTYPE"].ToString(),
                                        STKTYP_HIDDEN = dr["STKTYPE"].ToString(),
                                        FREESTK = dr["FREESTK"].ToString(),
                                        FREESTK_HIDDEN = dr["FREESTK"].ToString(),
                                        QNTY = dr["BALQNTY"] == null ? 0 : Convert.ToDouble(dr["BALQNTY"].ToString()),
                                    }).ToList();

                VE.TSORDDTL_CANC = (from p in VE.TSORDDTL_CANC group p by new { p.ORDAUTONO, p.ITCD, p.ITNM, p.ARTNO, p.TOTAL_PCS, p.UOM, p.STKTYPE, p.FREESTK, p.ChildData, p.QNTY } into z select new TSORDDTL_CANC { ORDAUTONO = z.Key.ORDAUTONO, ITCD = z.Key.ITCD, ITNM = z.Key.ITNM, ARTNO = z.Key.ARTNO, TOTAL_PCS = z.Key.TOTAL_PCS, UOM = z.Key.UOM, STKTYPE = z.Key.STKTYPE, FREESTK = z.Key.FREESTK, ChildData = z.Key.ChildData, QNTY = z.Sum(x => x.QNTY), ALL_SIZE = string.Join(",", z.Select(i => i.SIZECD)) }).ToList();

                for (int i = 0; i <= VE.TSORDDTL_CANC.Count - 1; i++)
                {
                    string ITEM = VE.TSORDDTL_CANC[i].ITCD; string AUTO_NO = VE.TSORDDTL_CANC[i].ORDAUTONO; string FREE_STK = VE.TSORDDTL_CANC[i].FREESTK; string STK_TYP = VE.TSORDDTL_CANC[i].STKTYPE;
                    VE.TSORDDTL_CANC_SIZE = (from DataRow dr in PENDING_ORDER.Rows
                                             where dr["ITCD"].ToString() == ITEM && dr["AUTONO"].ToString() == AUTO_NO && dr["FREESTK"].ToString() == FREE_STK && dr["STKTYPE"].ToString() == STK_TYP
                                             select new TSORDDTL_CANC_SIZE()
                                             {
                                                 PRINT_SEQ = dr["PRINT_SEQ"].ToString(),
                                                 SIZECD = dr["SIZECD"].ToString(),
                                                 SIZENM = dr["SIZENM"].ToString(),
                                                 COLRCD = dr["COLRCD"].ToString(),
                                                 COLRNM = dr["COLRNM"].ToString(),
                                                 QNTY = dr["BALQNTY"] == null ? 0 : Convert.ToDouble(dr["BALQNTY"].ToString()),
                                                 AUTONO = dr["AUTONO"].ToString(),
                                                 ITCD = dr["ITCD"].ToString(),
                                                 STKTYPE_HIDDEN = dr["STKTYPE"].ToString(),
                                                 FREESTK_HIDDEN = dr["FREESTK"].ToString(),
                                                 ITCOLSIZE = dr["ITCD"].ToString() + dr["COLRCD"].ToString() + dr["SIZECD"].ToString(),
                                                 PCS_HIDDEN = dr["PCSPERBOX"] == null ? 0 : Convert.ToDouble(dr["PCSPERBOX"].ToString())
                                             }).OrderBy(s => s.PRINT_SEQ).ToList();
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string JR = javaScriptSerializer.Serialize(VE.TSORDDTL_CANC_SIZE);
                    VE.TSORDDTL_CANC[i].ChildData = JR;

                    //var TOTAL_QNTY = (from p in VE.TSORDDTL_CANC_SIZE
                    //                  group p by new { p.AUTONO } into z
                    //                  select new TSORDDTL_CANC
                    //                  {
                    //                      QNTY = z.Sum(x => x.QNTY),
                    //                      ALL_SIZE = string.Join(",", z.Select(A => A.SIZECD))
                    //                  }).ToList();

                    //if (TOTAL_QNTY != null && TOTAL_QNTY.Count > 0)
                    //{
                    //    VE.TSORDDTL_CANC[i].QNTY = TOTAL_QNTY[0].QNTY;
                    //    VE.TSORDDTL_CANC[i].ALL_SIZE = TOTAL_QNTY[0].ALL_SIZE;
                    //}
                }
                VE.TSORDDTL_CANC.ToList().ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); a.DropDown_list3 = Master_Help.FREE_STOCK(); });
                for (int i = 0; i <= VE.TSORDDTL_CANC.Count - 1; i++) { VE.TSORDDTL_CANC[i].SLNO = Convert.ToInt16(i + 1); }
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_T_SORDCANC_MAIN", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult OPENSIZE(OrderCancelEntry VE, short SerialNo, string ITEM, string ITEM_NM, string ART_NO, string UOM, double? PCS, double? QNTY, string STK_TYPE, string FREE_STK, string AUTO_NO)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string COM = CommVar.Compcd(UNQSNO); string LOC = CommVar.Loccd(UNQSNO); string scm1 = DB.CacheKey.ToString();
            Cn.getQueryString(VE);
            try
            {
                ART_NO = ART_NO.Replace('μ', '+'); ART_NO = ART_NO.Replace('‡', '&');
                string Header = "";
                if (VE.DOC_CODE == "DOCAN") { Header = "Delivery Order Cancel"; } else { Header = "Sales Order Cancel"; }
                ViewBag.Header = Header; ViewBag.Article = ART_NO; ViewBag.Pcs = PCS; short POSRL = Convert.ToInt16(SerialNo);
                if (VE.DefaultAction == "V")
                {
                    if (VE.TSORDDTL_CANC != null && VE.TSORDDTL_CANC.Count > 0)
                    {
                        for (int i = 0; i <= VE.TSORDDTL_CANC.Count - 1; i++)
                        {
                            VE.TSORDDTL_CANC[i].STKTYPE = VE.TSORDDTL_CANC[i].STKTYP_HIDDEN;
                            VE.TSORDDTL_CANC[i].FREESTK = VE.TSORDDTL_CANC[i].FREESTK_HIDDEN;
                        }
                    }
                }
                var query = (from c in VE.TSORDDTL_CANC where (c.ITCD == ITEM) select c).ToList();
                if (query != null)
                {
                    VE.TSORDDTL_CANC = (from x in VE.TSORDDTL_CANC where x.ITCD == ITEM && x.STKTYPE == STK_TYPE && x.FREESTK == FREE_STK && x.ORDAUTONO == AUTO_NO select x).ToList();
                    if (VE.TSORDDTL_CANC != null && VE.TSORDDTL_CANC.Count > 0)
                    {
                        for (int i = 0; i <= VE.TSORDDTL_CANC.Count - 1; i++)
                        {
                            var helpMT = new List<Improvar.Models.TSORDDTL_CANC_SIZE>();
                            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            helpMT = javaScriptSerializer.Deserialize<List<Improvar.Models.TSORDDTL_CANC_SIZE>>(VE.TSORDDTL_CANC[i].ChildData);

                            helpMT.ForEach(a =>
                            {
                                a.ParentSerialNo = SerialNo; a.ITCD_HIDDEN = ITEM;
                                a.ITNM_HIDDEN = ITEM_NM; a.ARTNO_HIDDEN = ART_NO; a.UOM_HIDDEN = UOM; a.PCS_HIDDEN = PCS.Value;
                                a.QNTY_HIDDEN = QNTY;
                                //a.STKTYPE_HIDDEN = STK_TYPE; a.FREESTK_HIDDEN = FREE_STK;
                            });
                            if (helpMT != null && helpMT.Count > 0)
                            {
                                VE.TSORDDTL_CANC_SIZE = helpMT;
                            }
                        }
                    }
                    VE.TSORDDTL_CANC_SIZE = VE.TSORDDTL_CANC_SIZE.OrderBy(a => a.PRINT_SEQ).ToList();
                    for (int Z = 0; Z <= VE.TSORDDTL_CANC_SIZE.Count - 1; Z++)
                    {
                        VE.TSORDDTL_CANC_SIZE[Z].SLNO = Convert.ToInt16(Z + 1);
                    }
                    var javaScriptSerializer_FINAL = new System.Web.Script.Serialization.JavaScriptSerializer();
                    VE.TSORDDTL_CANC_SIZE.ForEach(a => a.ITCOLSIZE = a.ITCD_HIDDEN + a.COLRCD + a.SIZECD);
                    string JR_FINAL = javaScriptSerializer_FINAL.Serialize(VE.TSORDDTL_CANC_SIZE);
                    query.ForEach(a => a.ChildData = JR_FINAL);
                    VE.SERIAL = SerialNo;
                    VE.DefaultView = true;
                    ModelState.Clear();
                    return PartialView("_T_SORDCANC_SIZE", VE);
                }
                else
                {
                    VE.SERIAL = SerialNo;
                    VE.DefaultView = true;
                    ModelState.Clear();
                    return PartialView("_T_SORDCANC_SIZE", VE);
                }
            }
            catch (Exception ex)
            {
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public ActionResult CLOSESIZE(OrderCancelEntry VE, FormCollection FC)
        {
            Cn.getQueryString(VE);
            List<TSORDDTL_CANC_SIZE> DTL_SIZE = new List<TSORDDTL_CANC_SIZE>();
            DTL_SIZE = VE.TSORDDTL_CANC_SIZE.Where(a => a.CANCQNTY != null).Where(a => a.CANCQNTY > 0).ToList();
            if (DTL_SIZE != null)
            {
                var FILTER_DATA = (from p in DTL_SIZE
                                   group p by new
                                   { p.AUTONO, p.ITCD_HIDDEN, p.ITNM_HIDDEN, p.ARTNO_HIDDEN, p.PCS_HIDDEN, p.UOM_HIDDEN, p.STKTYPE_HIDDEN, p.FREESTK_HIDDEN } into g
                                   select new
                                   {
                                       AUTONO = g.Key.AUTONO,
                                       ITCD_HIDDEN = g.Key.ITCD_HIDDEN,
                                       ITNM_HIDDEN = g.Key.ITNM_HIDDEN,
                                       ARTNO_HIDDEN = g.Key.ARTNO_HIDDEN,
                                       PCS_HIDDEN = g.Key.PCS_HIDDEN,
                                       UOM_HIDDEN = g.Key.UOM_HIDDEN,
                                       STKTYPE_HIDDEN = g.Key.STKTYPE_HIDDEN,
                                       FREESTK_HIDDEN = g.Key.FREESTK_HIDDEN,
                                       QNTY = g.Sum(x => x.QNTY),
                                       CANCQNTY = g.Sum(x => x.CANCQNTY),
                                       ALL_SIZE = string.Join(",", g.Select(i => i.SIZECD))
                                   }).ToList();

                var TSORDDTL_CANC_FINALDATA = (from a in FILTER_DATA
                                               select new TSORDDTL_CANC
                                               {
                                                   ORDAUTONO = a.AUTONO,
                                                   ITCD = a.ITCD_HIDDEN,
                                                   ITNM = a.ITNM_HIDDEN,
                                                   ARTNO = a.ARTNO_HIDDEN,
                                                   TOTAL_PCS = a.PCS_HIDDEN,
                                                   UOM = a.UOM_HIDDEN,
                                                   STKTYPE = a.STKTYPE_HIDDEN,
                                                   FREESTK = a.FREESTK_HIDDEN,
                                                   QNTY = a.QNTY,
                                                   CANCQNTY = a.CANCQNTY,
                                                   ALL_SIZE = a.ALL_SIZE
                                               }).OrderBy(a => a.ARTNO).ToList();

                if (TSORDDTL_CANC_FINALDATA != null && TSORDDTL_CANC_FINALDATA.Count > 0)
                {
                    VE.TSORDDTL_CANC.RemoveAll(x => x.ITCD == TSORDDTL_CANC_FINALDATA[0].ITCD && x.ORDAUTONO == TSORDDTL_CANC_FINALDATA[0].ORDAUTONO && x.FREESTK == TSORDDTL_CANC_FINALDATA[0].FREESTK && x.STKTYPE == TSORDDTL_CANC_FINALDATA[0].STKTYPE);

                    VE.TSORDDTL_CANC.InsertRange(0, TSORDDTL_CANC_FINALDATA);
                }
                string ITEM = ""; string AUTO_NO = ""; string FREE_STK = ""; string STK_TYPE = "";
                if (DTL_SIZE != null && DTL_SIZE.Count > 0)
                {
                    ITEM = DTL_SIZE[0].ITCD_HIDDEN;
                    AUTO_NO = DTL_SIZE[0].AUTONO;
                    FREE_STK = DTL_SIZE[0].FREESTK_HIDDEN;
                    STK_TYPE = DTL_SIZE[0].STKTYPE_HIDDEN;
                }
                var query = (from c in VE.TSORDDTL_CANC where (c.ITCD == ITEM && c.ORDAUTONO == AUTO_NO && c.FREESTK == FREE_STK && c.STKTYPE == STK_TYPE) select c).ToList();
                if (query != null && query.Count > 0)
                {
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string JR = javaScriptSerializer.Serialize(DTL_SIZE);
                    query.ForEach(a => a.ChildData = JR);
                }
            }
            else
            {
                string ITEM = FC["hiddenITEM"].ToString();
                TSORDDTL_CANC query = (from c in VE.TSORDDTL_CANC where (c.ITCD == ITEM) select c).SingleOrDefault();
                if (query != null)
                {
                    query.ChildData = null;
                }
            }

            VE.TSORDDTL_CANC.ToList().ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); a.DropDown_list3 = Master_Help.FREE_STOCK(); });

            VE.TSORDDTL_CANC = VE.TSORDDTL_CANC.Where(m => m.ARTNO != null).OrderBy(m => m.ARTNO + m.QNTY).Concat(VE.TSORDDTL_CANC.Where(m => m.ARTNO == null)).ToList();

            for (int i = 0; i <= VE.TSORDDTL_CANC.Count - 1; i++) { VE.TSORDDTL_CANC[i].SLNO = Convert.ToInt16(i + 1); }

            VE.DefaultView = true;
            ModelState.Clear();
            return PartialView("_T_SORDCANC_MAIN", VE);
        }
        public ActionResult AddDOCRow(OrderCancelEntry VE)
        {
            string SCHEMA = Cn.Getschema;
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
            var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
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
        public ActionResult DeleteDOCRow(OrderCancelEntry VE)
        {
            string SCHEMA = Cn.Getschema;
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
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
        public ActionResult AddRow(OrderCancelEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            if (VE.TSORDDTL_CANC == null)
            {
                List<TSORDDTL_CANC> TSORDDTL_CANC1 = new List<TSORDDTL_CANC>();
                TSORDDTL_CANC SORDDTL = new TSORDDTL_CANC();
                SORDDTL.SLNO = 1;
                TSORDDTL_CANC1.Add(SORDDTL);
                VE.TSORDDTL_CANC = TSORDDTL_CANC1;
            }
            else
            {
                List<TSORDDTL_CANC> SORDDTL = new List<TSORDDTL_CANC>();
                for (int i = 0; i <= VE.TSORDDTL_CANC.Count - 1; i++)
                {
                    TSORDDTL_CANC ORDDTL = new TSORDDTL_CANC();
                    ORDDTL = VE.TSORDDTL_CANC[i];
                    SORDDTL.Add(ORDDTL);
                }
                TSORDDTL_CANC ORDDTL1 = new TSORDDTL_CANC();
                var max = VE.TSORDDTL_CANC.Max(a => Convert.ToInt32(a.SLNO));
                int SLNO = Convert.ToInt32(max) + 1;
                ORDDTL1.SLNO = Convert.ToSByte(SLNO);
                SORDDTL.Add(ORDDTL1);
                VE.TSORDDTL_CANC = SORDDTL;
            }

            VE.TSORDDTL_CANC.ToList().ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); a.DropDown_list3 = Master_Help.FREE_STOCK(); });
            VE.DefaultView = true;
            return PartialView("_T_SORD_MAIN", VE);
        }
        public ActionResult DeleteRow(OrderCancelEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            List<TSORDDTL_CANC> SORDDTL = new List<TSORDDTL_CANC>();
            int count = 0;
            for (int i = 0; i <= VE.TSORDDTL_CANC.Count - 1; i++)
            {
                if (VE.TSORDDTL_CANC[i].Checked == false)
                {
                    count += 1;
                    TSORDDTL_CANC item = new TSORDDTL_CANC();
                    item = VE.TSORDDTL_CANC[i];
                    item.SLNO = Convert.ToSByte(count);
                    SORDDTL.Add(item);
                }
            }
            VE.TSORDDTL_CANC = SORDDTL;
            VE.TSORDDTL_CANC.ToList().ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); a.DropDown_list3 = Master_Help.FREE_STOCK(); });
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_T_SORD_MAIN", VE);

        }
        public ActionResult SAVE(FormCollection FC, OrderCancelEntry VE)
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
                            T_SORD_CANC TSORDCANC = new T_SORD_CANC();
                            T_DO_CANC TDOCANC = new T_DO_CANC();
                            T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                            string DOCPATTERN = ""; string auto_no = ""; string Month = ""; string Ddate = "";
                            if (VE.DOC_CODE == "SOCAN")
                            {
                                TSORDCANC.DOCDT = VE.T_SORD_CANC.DOCDT;
                                Ddate = Convert.ToString(TSORDCANC.DOCDT);
                                TSORDCANC.CLCD = CommVar.ClientCode(UNQSNO);
                                if (VE.DefaultAction == "A")
                                {
                                    TSORDCANC.EMD_NO = 0;
                                    TSORDCANC.DOCCD = VE.DOC_ID;
                                    TSORDCANC.DOCNO = Cn.MaxDocNumber(TSORDCANC.DOCCD, Ddate);
                                    DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TSORDCANC.DOCNO), TSORDCANC.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                                    auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TSORDCANC.DOCNO, TSORDCANC.DOCCD, Ddate);
                                    TSORDCANC.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                                    Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                                }
                                else
                                {
                                    TSORDCANC.DTAG = "E";
                                    TSORDCANC.DOCCD = VE.DOC_ID;
                                    TSORDCANC.DOCNO = VE.T_SORD_CANC.DOCNO;
                                    TSORDCANC.AUTONO = VE.T_SORD_CANC.AUTONO;
                                    Month = VE.T_CNTRL_HDR.MNTHCD;
                                    var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == TSORDCANC.AUTONO select p.EMD_NO).Max();
                                    if (MAXEMDNO == null) { TSORDCANC.EMD_NO = 0; } else { TSORDCANC.EMD_NO = Convert.ToByte(MAXEMDNO + 1); }
                                }
                                TSORDCANC.SLCD = VE.T_SORD_CANC.SLCD;
                                TSORDCANC.REM = VE.T_SORD_CANC.REM;
                            }
                            else
                            {
                                TDOCANC.DOCDT = VE.T_DO_CANC.DOCDT;
                                Ddate = Convert.ToString(TDOCANC.DOCDT);
                                TDOCANC.CLCD = CommVar.ClientCode(UNQSNO);
                                if (VE.DefaultAction == "A")
                                {
                                    TDOCANC.EMD_NO = 0;
                                    TDOCANC.DOCCD = VE.DOC_ID;
                                    TDOCANC.DOCNO = Cn.MaxDocNumber(TDOCANC.DOCCD, Ddate);
                                    DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TDOCANC.DOCNO), TDOCANC.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                                    auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TDOCANC.DOCNO, TDOCANC.DOCCD, Ddate);
                                    TDOCANC.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                                    Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                                }
                                else
                                {
                                    TDOCANC.DTAG = "E";
                                    TDOCANC.DOCCD = VE.DOC_ID;
                                    TDOCANC.DOCNO = VE.T_DO_CANC.DOCNO;
                                    TDOCANC.AUTONO = VE.T_DO_CANC.AUTONO;
                                    Month = VE.T_CNTRL_HDR.MNTHCD;
                                    var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == TDOCANC.AUTONO select p.EMD_NO).Max();
                                    if (MAXEMDNO == null) { TDOCANC.EMD_NO = 0; } else { TDOCANC.EMD_NO = Convert.ToByte(MAXEMDNO + 1); }
                                }
                                TDOCANC.SLCD = VE.T_DO_CANC.SLCD;
                                TDOCANC.REM = VE.T_DO_CANC.REM;
                            }
                            if (VE.DefaultAction == "E")
                            {
                                if (VE.DOC_CODE == "SOCAN")
                                {
                                    DB.T_SORDDTL.Where(x => x.AUTONO == TSORDCANC.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                    DB.T_SORDDTL.RemoveRange(DB.T_SORDDTL.Where(x => x.AUTONO == TSORDCANC.AUTONO));
                                }
                                else
                                {
                                    DB.T_DODTL.Where(x => x.AUTONO == TDOCANC.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                    DB.T_DODTL.RemoveRange(DB.T_DODTL.Where(x => x.AUTONO == TDOCANC.AUTONO));
                                }

                                DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == TSORDCANC.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_CNTRL_HDR_REM.RemoveRange(DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == TSORDCANC.AUTONO));

                                DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == TSORDCANC.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == TSORDCANC.AUTONO));

                                DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == TSORDCANC.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == TSORDCANC.AUTONO));
                                DB.SaveChanges();
                            }
                            if (VE.DOC_CODE == "SOCAN")
                            {
                                TCH = Cn.T_CONTROL_HDR(TSORDCANC.DOCCD, TSORDCANC.DOCDT, TSORDCANC.DOCNO, TSORDCANC.AUTONO, Month, DOCPATTERN, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, null, 0, null);
                            }
                            else
                            {
                                TCH = Cn.T_CONTROL_HDR(TDOCANC.DOCCD, TDOCANC.DOCDT, TDOCANC.DOCNO, TDOCANC.AUTONO, Month, DOCPATTERN, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, null, 0, null);
                            }
                            if (VE.DefaultAction == "A")
                            {
                                if (VE.DOC_CODE == "SOCAN")
                                {
                                    DB.T_SORD_CANC.Add(TSORDCANC);
                                }
                                else
                                {
                                    DB.T_DO_CANC.Add(TDOCANC);
                                }
                                DB.T_CNTRL_HDR.Add(TCH);
                            }
                            else if (VE.DefaultAction == "E")
                            {
                                if (VE.DOC_CODE == "SOCAN")
                                {
                                    DB.Entry(TSORDCANC).State = System.Data.Entity.EntityState.Modified;
                                }
                                else
                                {
                                    DB.Entry(TDOCANC).State = System.Data.Entity.EntityState.Modified;
                                }
                                DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                            }
                            int COUNTER = 0;
                            for (int i = 0; i <= VE.TSORDDTL_CANC.Count - 1; i++)
                            {
                                if (VE.TSORDDTL_CANC[i].SLNO != 0 && VE.TSORDDTL_CANC[i].ITCD != null)
                                {
                                    var SIZE_CODE = VE.TSORDDTL_CANC[i].ALL_SIZE.Split(',');
                                    if (VE.TSORDDTL_CANC[i].ChildData != null)
                                    {
                                        string data = VE.TSORDDTL_CANC[i].ChildData;
                                        var helpM = new List<Improvar.Models.TSORDDTL_CANC_SIZE>();
                                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                        helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.TSORDDTL_CANC_SIZE>>(data);
                                        helpM = helpM.Where(a => SIZE_CODE.Contains(a.SIZECD)).ToList();
                                        for (int j = 0; j <= helpM.Count - 1; j++)
                                        {
                                            if (helpM[j].SLNO != 0 && helpM[j].CANCQNTY != null && helpM[j].CANCQNTY != 0)
                                            {
                                                COUNTER = COUNTER + 1;
                                                if (VE.DOC_CODE == "SOCAN")
                                                {
                                                    T_SORDDTL TSORDDTL_CANC = new T_SORDDTL();
                                                    TSORDDTL_CANC.EMD_NO = TSORDCANC.EMD_NO;
                                                    TSORDDTL_CANC.CLCD = TSORDCANC.CLCD;
                                                    TSORDDTL_CANC.DTAG = TSORDCANC.DTAG;
                                                    TSORDDTL_CANC.TTAG = TSORDCANC.TTAG;
                                                    TSORDDTL_CANC.AUTONO = TSORDCANC.AUTONO;
                                                    TSORDDTL_CANC.ORDAUTONO = VE.TSORDDTL_CANC[i].ORDAUTONO;
                                                    TSORDDTL_CANC.SLNO = Convert.ToInt16(COUNTER);
                                                    if (VE.MENU_PARA == "SB") { TSORDDTL_CANC.STKDRCR = "C"; } else { TSORDDTL_CANC.STKDRCR = "D"; }
                                                    TSORDDTL_CANC.STKTYPE = VE.TSORDDTL_CANC[i].STKTYPE;
                                                    TSORDDTL_CANC.FREESTK = VE.TSORDDTL_CANC[i].FREESTK;
                                                    TSORDDTL_CANC.ITCD = VE.TSORDDTL_CANC[i].ITCD;
                                                    TSORDDTL_CANC.SIZECD = helpM[j].SIZECD;
                                                    TSORDDTL_CANC.COLRCD = helpM[j].COLRCD;
                                                    TSORDDTL_CANC.QNTY = helpM[j].CANCQNTY;
                                                    TSORDDTL_CANC.TAXAMT = 0;
                                                    DB.T_SORDDTL.Add(TSORDDTL_CANC);
                                                }
                                                else
                                                {
                                                    T_DODTL TDODTL_CANC = new T_DODTL();
                                                    TDODTL_CANC.EMD_NO = TSORDCANC.EMD_NO;
                                                    TDODTL_CANC.CLCD = TSORDCANC.CLCD;
                                                    TDODTL_CANC.DTAG = TSORDCANC.DTAG;
                                                    TDODTL_CANC.TTAG = TSORDCANC.TTAG;
                                                    TDODTL_CANC.AUTONO = TSORDCANC.AUTONO;
                                                    TDODTL_CANC.ORDAUTONO = VE.TSORDDTL_CANC[i].ORDAUTONO;
                                                    TDODTL_CANC.SLNO = Convert.ToInt16(COUNTER);
                                                    if (VE.MENU_PARA == "SB") { TDODTL_CANC.STKDRCR = "C"; } else { TDODTL_CANC.STKDRCR = "D"; }
                                                    TDODTL_CANC.STKTYPE = VE.TSORDDTL_CANC[i].STKTYPE;
                                                    TDODTL_CANC.FREESTK = VE.TSORDDTL_CANC[i].FREESTK;
                                                    TDODTL_CANC.ITCD = VE.TSORDDTL_CANC[i].ITCD;
                                                    TDODTL_CANC.SIZECD = helpM[j].SIZECD;
                                                    TDODTL_CANC.COLRCD = helpM[j].COLRCD;
                                                    TDODTL_CANC.QNTY = helpM[j].CANCQNTY;
                                                    TDODTL_CANC.TAXAMT = 0;
                                                    DB.T_DODTL.Add(TDODTL_CANC);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (VE.UploadDOC != null)// add DOCUMENT
                            {
                                var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TSORDCANC.AUTONO, TSORDCANC.EMD_NO.Value);
                                if (img.Item1.Count != 0)
                                {
                                    DB.T_CNTRL_HDR_DOC.AddRange(img.Item1);
                                    DB.T_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                                }
                            }
                            if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                            {
                                var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TSORDCANC.AUTONO, TSORDCANC.CLCD, TSORDCANC.EMD_NO.Value);
                                if (NOTE.Item1.Count != 0)
                                {
                                    DB.T_CNTRL_HDR_REM.AddRange(NOTE.Item1);
                                }
                            }
                            DB.SaveChanges();
                            ModelState.Clear();
                            transaction.Commit();
                            string ContentFlg = "";
                            if (VE.DefaultAction == "A")
                            {
                                ContentFlg = "1 ~" + TSORDCANC.AUTONO;
                            }
                            else if (VE.DefaultAction == "E")
                            {
                                ContentFlg = "2";
                            }
                            return Content(ContentFlg);
                        }
                        else if (VE.DefaultAction == "V")
                        {
                            if (VE.DOC_CODE == "SOCAN")
                            {
                                DB.T_SORD_CANC.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.T_SORDDTL.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            }
                            else
                            {
                                DB.T_DO_CANC.Where(x => x.AUTONO == VE.T_DO_CANC.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.T_DODTL.Where(x => x.AUTONO == VE.T_DO_CANC.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            }
                            DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO));
                            DB.SaveChanges();

                            DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO));
                            DB.SaveChanges();

                            DB.T_CNTRL_HDR_REM.RemoveRange(DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO));
                            DB.SaveChanges();
                            if (VE.DOC_CODE == "SOCAN")
                            {
                                DB.T_SORDDTL.RemoveRange(DB.T_SORDDTL.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO));
                                DB.SaveChanges();
                                DB.T_SORD_CANC.RemoveRange(DB.T_SORD_CANC.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO));
                                DB.SaveChanges();
                            }
                            else
                            {
                                DB.T_DODTL.RemoveRange(DB.T_DODTL.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO));
                                DB.SaveChanges();
                                DB.T_DO_CANC.RemoveRange(DB.T_DO_CANC.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO));
                                DB.SaveChanges();
                            }
                            DB.T_CNTRL_HDR.RemoveRange(DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO));
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
    }
}