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
                        case "POCAN":
                            ViewBag.formname = "Purchase Order Cancel Part / Full"; ViewBag.GridHeader = "Purchase Order Cancel"; break;
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
                    var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                    VE.M_SYSCNFG = MSYSCNFG;
                    if (op.Length != 0)
                    {
                        string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();

                        VE.IndexKey = (from p in DB.T_SORD_CANC join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO) where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COMP && q.YR_CD == YR1 select new IndexKey() { Navikey = p.AUTONO }).OrderBy(a => a.Navikey).ToList();


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

                            VE.T_SORD_CANC = sl;


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

                                T_SORD_CANC TSORDCANC = new T_SORD_CANC();
                                TSORDCANC.DOCDT = System.DateTime.Now.Date;
                                VE.T_SORD_CANC = TSORDCANC;


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

                        Cn.getdocmaxmindate(VE.T_SORD_CANC.DOCCD, docdt, VE.DefaultAction, VE.T_SORD_CANC.DOCNO, VE);


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

                sl = DB.T_SORD_CANC.Find(aa[0].Trim());
                AUTO_NO = sl.AUTONO;
                DOC_CODE = sl.DOCCD;


                sll = DB.T_CNTRL_HDR.Find(AUTO_NO);
                DOCTYP = DB.M_DOCTYPE.Find(DOC_CODE);
                TCHR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), AUTO_NO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), AUTO_NO);
                if (sl != null)
                {
                    VE.DOC_ID = sl.DOCCD;
                    var Party = DBF.M_SUBLEG.Find(sl.SLCD); if (Party != null) { VE.PartyName = Party.SLNM; }
                }
                string SCM = CommVar.CurSchema(UNQSNO);

                string str1 = "";
                str1 += "select a.SLNO,a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, c.ITNM, c.STYLENO, c.PCSPERSET, c.UOMCD, ";
                str1 += "a.sizecd, a.rate, a.scmdiscamt, a.discamt, a.qnty,A.DELVDT,a.ITREM,a.PDESIGN,c.itgrpcd, d.itgrpnm,c.fabitcd, ";
                str1 += "e.itnm fabitnm,a.colrcd,a.partcd,f.colrnm,g.sizenm,h.partnm,a.rate,a.frghtamt,a.ORDAUTONO,a.ORDSLNO,i.qnty ordqnty,j.docno orddocno,j.docdt orddocdt from ";
                str1 += SCM + ".T_SORDDTL a, " + SCM + ".T_CNTRL_HDR b, ";
                str1 += SCM + ".m_sitem c, " + SCM + ".m_group d, " + SCM + ".m_sitem e, " + SCM + ".m_color f, " + SCM + ".m_size g, " + SCM + ".m_parts h, " + SCM + ".T_SORDDTL i, " + SCM + ".T_CNTRL_HDR j ";
                str1 += "where a.autono = b.autono(+) and a.itcd = c.itcd(+) and c.itgrpcd=d.itgrpcd and c.fabitcd=e.itcd(+) ";
                str1 += "and a.colrcd=f.colrcd(+) and a.sizecd=g.sizecd(+) and a.partcd=h.partcd(+) and a.ORDAUTONO=i.autono(+) and a.ORDSLNO=i.slno(+) and i.autono=j.autono(+) and a.autono='" + sl.AUTONO + "' ";
                str1 += "order by styleno ";

                DataTable tbl = Master_Help.SQLquery(str1);
                VE.TSORDDTL_CANC = (from DataRow dr in tbl.Rows
                                    select new TSORDDTL_CANC()
                                    {
                                        AUTONO = dr["AUTONO"].retStr(),
                                        SLNO = dr["SLNO"].retShort(),
                                        STKDRCR = dr["STKDRCR"].retStr(),
                                        ITCD = dr["ITCD"].retStr(),
                                        ITNM = dr["ITNM"].retStr(),
                                        ARTNO = dr["STYLENO"].retStr(),
                                        UOM = dr["UOMCD"].retStr(),
                                        CANCQNTY = dr["QNTY"].retDbl(),
                                        QNTY = dr["ordqnty"].retDbl(),
                                        COLRCD = dr["COLRCD"].ToString(),
                                        COLRNM = dr["COLRNM"].ToString(),
                                        SIZECD = dr["SIZECD"].ToString(),
                                        SIZENM = dr["SIZENM"].ToString(),
                                        PARTCD = dr["PARTCD"].ToString(),
                                        PARTNM = dr["PARTNM"].ToString(),
                                        RATE = dr["RATE"].retDbl(),
                                        ORDAUTONO = dr["ORDAUTONO"].ToString(),
                                        ORDSLNO = dr["ORDSLNO"].retShort(),
                                    }).OrderBy(s => s.SLNO).ToList();


                VE.ORDNO = tbl.Rows[0]["orddocno"].retStr();
                VE.ORDDT = Convert.ToDateTime(tbl.Rows[0]["orddocdt"].retDateStr());
                for (int z = 0; z <= VE.TSORDDTL_CANC.Count - 1; z++)
                {
                    VE.TSORDDTL_CANC[z].SLNO = Convert.ToInt16(z + 1);
                }
                VE.TOTAL_QNTY = VE.TSORDDTL_CANC.Sum(a => a.QNTY).retDbl();
                VE.TOTAL_CANCQNTY = VE.TSORDDTL_CANC.Sum(a => a.CANCQNTY).retDbl();
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

                DataTable PENDING_ORDER = SALES_FUNC.GetPendOrder(VALUE[0], VALUE[1], "", "", "", "", VE.MENU_PARA, "");

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

                DataTable PENDING_ORDER = SALES_FUNC.GetPendOrder(PARTY, ORDERASONDATE, ORD_AUTO == null || ORD_AUTO.Count == 0 ? "" : "'" + ORD_AUTO[0] + "'" != null && ORD_AUTO.Count > 0 ? "'" + ORD_AUTO[0] + "'" : "", "", DOC_DT, VE.T_SORD_CANC.AUTONO.retStr(), VE.MENU_PARA, "");
                VE.TSORDDTL_CANC = (from DataRow dr in PENDING_ORDER.Rows
                                    select new TSORDDTL_CANC
                                    {
                                        ORDAUTONO = dr["AUTONO"].ToString(),
                                        ORDSLNO = dr["slno"].retShort(),
                                        ARTNO = dr["STYLENO"].ToString(),
                                        ITCD = dr["ITCD"].ToString(),
                                        ITNM = dr["ITNM"].ToString(),
                                        SIZECD = dr["SIZECD"].ToString(),
                                        SIZENM = dr["SIZENM"].ToString(),
                                        COLRCD = dr["COLRCD"].ToString(),
                                        COLRNM = dr["COLRNM"].ToString(),
                                        PARTCD = dr["PARTCD"].ToString(),
                                        PARTNM = dr["PARTNM"].ToString(),
                                        ALL_SIZE = dr["SIZECD"].ToString(),
                                        UOM = dr["UOMCD"].ToString(),
                                        QNTY = dr["BALQNTY"] == null ? 0 : Convert.ToDouble(dr["BALQNTY"].ToString()),
                                    }).ToList();


                for (int i = 0; i <= VE.TSORDDTL_CANC.Count - 1; i++) { VE.TSORDDTL_CANC[i].SLNO = Convert.ToInt16(i + 1); }
                VE.TOTAL_QNTY = VE.TSORDDTL_CANC.Sum(a => a.QNTY).retDbl();
                VE.TOTAL_CANCQNTY = VE.TSORDDTL_CANC.Sum(a => a.CANCQNTY).retDbl();

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
            return PartialView("_T_SORDCANC_MAIN", VE);
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
            return PartialView("_T_SORDCANC_MAIN", VE);

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
                            T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                            string DOCPATTERN = ""; string auto_no = ""; string Month = ""; string Ddate = "";

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


                            if (VE.DefaultAction == "E")
                            {

                                DB.T_SORDDTL.Where(x => x.AUTONO == TSORDCANC.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_SORDDTL.RemoveRange(DB.T_SORDDTL.Where(x => x.AUTONO == TSORDCANC.AUTONO));

                                DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == TSORDCANC.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_CNTRL_HDR_REM.RemoveRange(DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == TSORDCANC.AUTONO));

                                DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == TSORDCANC.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == TSORDCANC.AUTONO));

                                DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == TSORDCANC.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == TSORDCANC.AUTONO));
                                DB.SaveChanges();
                            }

                            TCH = Cn.T_CONTROL_HDR(TSORDCANC.DOCCD, TSORDCANC.DOCDT, TSORDCANC.DOCNO, TSORDCANC.AUTONO, Month, DOCPATTERN, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, TSORDCANC.SLCD, 0, null, VE.Audit_REM);


                            if (VE.DefaultAction == "A")
                            {

                                DB.T_SORD_CANC.Add(TSORDCANC);


                                DB.T_CNTRL_HDR.Add(TCH);
                            }
                            else if (VE.DefaultAction == "E")
                            {

                                DB.Entry(TSORDCANC).State = System.Data.Entity.EntityState.Modified;


                                DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                            }
                            int COUNTER = 0;
                            for (int i = 0; i <= VE.TSORDDTL_CANC.Count - 1; i++)
                            {
                                if (VE.TSORDDTL_CANC[i].CANCQNTY.retDbl() != 0)
                                {
                                    T_SORDDTL TSORDDTL = new T_SORDDTL();
                                    TSORDDTL.EMD_NO = TSORDCANC.EMD_NO;
                                    TSORDDTL.CLCD = TSORDCANC.CLCD;
                                    TSORDDTL.DTAG = TSORDCANC.DTAG;
                                    TSORDDTL.TTAG = TSORDCANC.TTAG;
                                    TSORDDTL.AUTONO = TSORDCANC.AUTONO;
                                    TSORDDTL.SLNO = Convert.ToInt16(COUNTER);
                                    TSORDDTL.BLSLNO = Convert.ToInt16(COUNTER);
                                    if (VE.MENU_PARA == "SB")
                                    {
                                        TSORDDTL.STKDRCR = "C";
                                    }
                                    else
                                    {
                                        TSORDDTL.STKDRCR = "D";
                                    }
                                    TSORDDTL.STKTYPE = "F";
                                    TSORDDTL.ITCD = VE.TSORDDTL_CANC[i].ITCD;
                                    TSORDDTL.SIZECD = VE.TSORDDTL_CANC[i].SIZECD;
                                    TSORDDTL.COLRCD = VE.TSORDDTL_CANC[i].COLRCD;
                                    TSORDDTL.PARTCD = VE.TSORDDTL_CANC[i].PARTCD;
                                    TSORDDTL.QNTY = VE.TSORDDTL_CANC[i].CANCQNTY;
                                    TSORDDTL.RATE = VE.TSORDDTL_CANC[i].RATE;
                                    TSORDDTL.SCMDISCAMT = VE.TSORDDTL_CANC[i].SCMDISCAMT;
                                    TSORDDTL.DISCAMT = VE.TSORDDTL_CANC[i].DISCAMT;
                                    TSORDDTL.TAXAMT = 0;
                                    TSORDDTL.ORDAUTONO = VE.TSORDDTL_CANC[i].ORDAUTONO;
                                    TSORDDTL.ORDSLNO = VE.TSORDDTL_CANC[i].ORDSLNO;
                                    TSORDDTL.PRCCD = "WP";
                                    DB.T_SORDDTL.Add(TSORDDTL);
                                    COUNTER++;

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
                            T_CNTRL_HDR TCH = Cn.T_CONTROL_HDR(VE.T_SORD_CANC.DOCCD, VE.T_SORD_CANC.DOCDT, VE.T_SORD_CANC.DOCNO, VE.T_SORD_CANC.AUTONO, VE.T_CNTRL_HDR.MNTHCD, "", VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, VE.T_SORD_CANC.SLCD, 0, null, VE.Audit_REM);
                            DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                            DB.SaveChanges();

                            if (VE.DOC_CODE == "SOCAN")
                            {
                                DB.T_SORD_CANC.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.T_SORDDTL.Where(x => x.AUTONO == VE.T_SORD_CANC.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
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