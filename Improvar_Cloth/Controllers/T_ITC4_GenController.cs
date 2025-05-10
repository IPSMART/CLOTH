using Improvar.Models;
using Improvar.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data.Entity.Validation;

namespace Improvar.Controllers
{
    public class T_ITC4_GenController : Controller
    {
        // GET: T_ITC4_Gen
        Connection Cn = new Connection();
        string CS = null;
        MasterHelp masterHelp = new MasterHelp();
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        T_ITC4_HDR sl; T_CNTRL_HDR sll; ImprovarDB DB; T_CNTRL_HDR_REM SLR;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string issdoctype = "", recdoctype = "", mergedoctype = "";
        public ActionResult T_ITC4_Gen(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ITC4Generation VE = (parkID == "") ? new ITC4Generation() : (ITC4Generation)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ViewBag.formname = "Generate ITC4";
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);
                    ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    string YRCD = Convert.ToString(Convert.ToDouble(CommVar.YearCode(UNQSNO)));

                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    //VE.Database_Combo1 = (from n in DB.T_ITC4_HDR
                    //                      select new Database_Combo1() { FIELD_VALUE = n.BUDGCTG }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    if (op.Length != 0)
                    {
                        var IndexKey_tmp = (from p in DB.T_ITC4_HDR
                                            join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                            orderby p.DOCDT, p.AUTONO
                                            where (q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YRCD)
                                            select new
                                            {
                                                AUTONO = p.AUTONO,
                                                DOCNO = p.DOCNO,
                                                DOCDT = q.DOCDT
                                            }).OrderBy(s => s.DOCDT).ToList();

                        VE.IndexKey = (from p in IndexKey_tmp
                                       select new IndexKey()
                                       {
                                           Navikey = p.AUTONO
                                       }).OrderBy(s => s.Navikey).ToList();

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
                            VE.T_ITC4_HDR = sl;
                            VE.T_CNTRL_HDR = sll;
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {
                                T_ITC4_HDR TITC4 = new T_ITC4_HDR();
                                TITC4.DOCDT = Cn.getCurrentDate(VE.mindate);
                                VE.T_ITC4_HDR = TITC4;
                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                var doctP = (from i in DBI.MS_DOCCTG
                                                 //where i.DOC_CTG == VE.ACK_DOCCTG
                                             select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = doctP;
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
                                // VE.DOC_ID = VE.DOC_CD;
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }


                            VE = (ITC4Generation)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        string docdt = "";
                        if (VE.T_CNTRL_HDR != null) if (VE.T_CNTRL_HDR.DOCDT != null) docdt = VE.T_CNTRL_HDR.DOCDT.ToString().Remove(10);
                        Cn.getdocmaxmindate(VE.T_ITC4_HDR.DOCCD, docdt, VE.DefaultAction, VE.T_ITC4_HDR.DOCNO, VE);
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
                ITC4Generation VE = new ITC4Generation();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public ITC4Generation Navigation(ITC4Generation VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            sl = new T_ITC4_HDR(); sll = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM();
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
                sl = DB.T_ITC4_HDR.Find(aa[0]);
                sll = DB.T_CNTRL_HDR.Find(sl.AUTONO);

                VE.TITC4DTL = (from i in DB.T_ITC4_DTL
                               join j in DB.M_JOBMST on i.JOBCD equals j.JOBCD
                               join k in DB.M_DOCTYPE on i.DOCCD equals k.DOCCD
                               join l in DB.M_DTYPE on k.DOCTYPE equals l.DCD
                               where (i.AUTONO == sl.AUTONO)
                               select new TITC4DTL()
                               {
                                   AUTONO = i.AUTONO,
                                   SLNO = i.SLNO,
                                   DOCCD = i.DOCCD,
                                   DOCNM = k.DOCNM,
                                   JOBCD = i.JOBCD,
                                   JOBNM = j.JOBNM,
                                   QNTY = i.QNTY,
                                   SHORTQNTY = i.SHORTQNTY,
                                   RATE = i.RATE,
                                   UOM = i.UOMCD,
                                   JOBSEQ = j.JOBSEQ,
                                   FLAG1 = l.FLAG1
                               }).OrderBy(s => s.SLNO).ToList();

                for (int q = 0; q <= VE.TITC4DTL.Count - 1; q++)
                {
                    VE.TITC4DTL[q].SLNO = Convert.ToSByte(q + 1);
                }

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
            ITC4Generation VE = new ITC4Generation();
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string LOC = CommVar.Loccd(UNQSNO);
            string COM = CommVar.Compcd(UNQSNO);
            string YRCD = Convert.ToString(Convert.ToDouble(CommVar.YearCode(UNQSNO)));
            string query = "";
            query += "select distinct p.AUTONO,p.DOCNO,p.DOCCD,p.DOCDT from " + CommVar.CurSchema(UNQSNO) + ".T_ITC4_HDR p, " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR q ";
            query += "where P.AUTONO = Q.AUTONO(+) and Q.COMPCD = '" + COM + "' ";
            query += "and Q.LOCCD = '" + LOC + "'and Q.YR_CD = '" + YRCD + "'order by P.AUTONO ";
            DataTable Record = masterHelp.SQLquery(query);

            var MDT = (from DataRow dr in Record.Rows
                       select new
                       {
                           AUTONO = dr["AUTONO"].ToString(),
                           DOCNO = dr["DOCNO"].ToString(),
                           DOCCD = dr["DOCCD"].ToString(),
                           DOCDT = dr["DOCDT"].retDateStr(),
                       }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Doc Code" + Cn.GCS() + "Doc No" + Cn.GCS() + "Doc Date" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].DOCCD + "</td><td>" + MDT[j].DOCNO + "</td><td>" + MDT[j].DOCDT + "</td><td>" + MDT[j].AUTONO + "</td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "3", "3"));
        }

        public ActionResult DataGather(ITC4Generation VE)
        {
            try
            {
                if (VE.T_ITC4_HDR.FROMDT > VE.T_ITC4_HDR.TODT)
                {
                    return Content("To date should be gater than or equal from date ");
                }
                string fdt = VE.T_ITC4_HDR.FROMDT.ToString().Remove(10), tdt = VE.T_ITC4_HDR.TODT.ToString().Remove(10);
                string sql = "", scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

                sql = "select slcd from " + scmf + ".m_subleg_link a where a.linkcd='I'";
                DataTable tbllink = masterHelp.SQLquery(sql);
                string linkslcd = string.Join(",", (from DataRow dr in tbllink.Rows select dr["slcd"].ToString()).Distinct());
                linkslcd = linkslcd.retSqlformat();

                issdoctype = "'OPRI','OEMI','OJWI','OSTI','OIRI'";
                recdoctype = "'ODYR','ODYU','OPRR','OPRU','OEMR','OEMU','OJWR','OJWU','OSTR','OSTU','OIRR','OOIU' ";
                mergedoctype = issdoctype + "," + recdoctype;

                sql = "select nvl(jobseq,0) jobseq, jobcd, jobnm, flag1, doccd, docnm, uomcd, sum(qnty)qnty, sum(shortqnty) shortqnrty  from( ";
                sql += "select a.autono, c.docdt, b.jobcd, f.jobnm, f.jobseq, g.flag1, c.doccd, d.docnm, b.prefno, b.prefdt, b.slcd, ";
                sql += "a.itcd, e.uomcd, nvl(a.qnty, 0) qnty, nvl(a.shortqnty, 0) shortqnty from ";
                sql += "(select a.autono, a.itcd, sum(a.qnty) qnty, sum(a.shortqnty) shortqnty ";
                sql += "from " + scm + ".t_txndtl a ";
                sql += "group by a.autono, a.itcd) a, ";
                sql += scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_doctype d, " + scm + ".m_sitem e, ";
                sql += scm + ".m_jobmst f, " + scm + ".m_dtype g ";
                sql += "where d.doctype in (" + mergedoctype + ") and b.jobcd=f.jobcd(+) and d.doctype=g.dcd(+)   ";
                if (linkslcd.retStr() != "") sql += "and b.slcd not in (" + linkslcd + ") ";
                sql += "and a.autono = b.autono and a.autono = c.autono and c.doccd = d.doccd and nvl(d.pro, 'I') <> 'I' and a.itcd = e.itcd(+) and ";
                sql += "c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and nvl(c.cancel, 'N') = 'N' and ";
                sql += "c.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and c.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ) ";
                sql += "group by jobseq, jobcd, jobnm, flag1, doccd, docnm, uomcd ";
                sql += "order by jobseq, jobnm, docnm, uomcd ";
                var FILTER_DATA = Master_HelpFa.SQLquery(sql);
                List<TITC4DTL> olddata = new List<TITC4DTL>();
                if (VE.TITC4DTL != null)
                {
                    olddata = (from dr in VE.TITC4DTL
                               select new TITC4DTL()
                               {
                                   DOCCD = dr.DOCCD,
                                   UOM = dr.UOM,
                                   RATE = dr.RATE
                               }).ToList();
                }

                Int32 i = 0;
                VE.TITC4DTL = (from DataRow dr in FILTER_DATA.Rows
                               select new TITC4DTL()
                               {
                                   JOBCD = dr["jobcd"].ToString(),
                                   JOBNM = dr["jobnm"].ToString(),
                                   DOCCD = dr["doccd"].ToString(),
                                   DOCNM = dr["docnm"].ToString(),
                                   UOM = dr["uomcd"].ToString(),
                                   QNTY = dr["qnty"].retDcml(),
                                   SHORTQNTY = dr["shortqnrty"].retDcml(),
                                   JOBSEQ = dr["jobseq"].retDbl(),
                                   FLAG1 = dr["flag1"].ToString()
                               }).ToList();
                for (int q = 0; q <= VE.TITC4DTL.Count - 1; q++)
                {
                    VE.TITC4DTL[q].SLNO = (q + 1).retShort();
                    if (VE.TITC4DTL != null)
                    {
                        var rate = (from dr in olddata
                                    where dr.DOCCD == VE.TITC4DTL[q].DOCCD && dr.UOM == VE.TITC4DTL[q].UOM
                                    select new
                                    {
                                        RATE = dr.RATE
                                    }).ToList();
                        if (rate.Count > 0) VE.TITC4DTL[q].RATE = rate[0].RATE;
                    }
                }
                ModelState.Clear();
                return PartialView("_T_ITC4_Gen", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SAVE(FormCollection FC, ITC4Generation VE)
        {
            string sql = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            if (OraCon.State == ConnectionState.Closed)
            {
                OraCon.Open();
            }
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                {
                    using (var transaction = DB.Database.BeginTransaction())
                    {
                        try
                        {
                            try
                            {
                                //Oracle Queries
                                string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
                                string dbsql = "";
                                //sql = "select slcd from " + scmf + ".m_subleg_link a where a.linkcd='I'";
                                //DataTable tbllink = masterHelp.SQLquery(sql);
                                //string linkslcd = string.Join(",", (from DataRow dr in tbllink.Rows select dr["slcd"].ToString()).Distinct());
                                //linkslcd = linkslcd.retSqlformat();

                                //if (CommVar.Compcd(UNQSNO) == "REFA") linkslcd = "'xx'";
                                string linkslcd = "";
                                Cn.getQueryString(VE);
                                DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                                if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                                {
                                    T_ITC4_HDR TITC4HDR = new T_ITC4_HDR();
                                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                                    string DOCPATTERN = "";
                                    TITC4HDR.DOCDT = VE.T_ITC4_HDR.DOCDT;
                                    string Ddate = TITC4HDR.DOCDT.retStr();
                                    TITC4HDR.CLCD = CommVar.ClientCode(UNQSNO);
                                    string auto_no = ""; string Month = "";
                                    if (VE.DefaultAction == "A")
                                    {
                                        TITC4HDR.EMD_NO = 0;
                                        TITC4HDR.DOCCD = VE.T_ITC4_HDR.DOCCD;
                                        TITC4HDR.DOCNO = Cn.MaxDocNumber(TITC4HDR.DOCCD, Ddate);
                                        DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TITC4HDR.DOCNO), TITC4HDR.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                                        auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TITC4HDR.DOCNO, TITC4HDR.DOCCD, Ddate);
                                        TITC4HDR.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                                        Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                                        VE.T_ITC4_HDR.AUTONO = TITC4HDR.AUTONO;
                                    }
                                    else
                                    {
                                        TITC4HDR.DOCCD = VE.T_ITC4_HDR.DOCCD;
                                        TITC4HDR.DOCNO = VE.T_ITC4_HDR.DOCNO;
                                        TITC4HDR.AUTONO = VE.T_ITC4_HDR.AUTONO;
                                        Month = VE.T_CNTRL_HDR.MNTHCD;
                                        var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == TITC4HDR.AUTONO select p.EMD_NO).Max();
                                        if (MAXEMDNO == null) { TITC4HDR.EMD_NO = 0; } else { TITC4HDR.EMD_NO = Convert.ToByte(MAXEMDNO + 1); }
                                    }
                                    TITC4HDR.FROMDT = VE.T_ITC4_HDR.FROMDT;
                                    TITC4HDR.TODT = VE.T_ITC4_HDR.TODT;
                                    TITC4HDR.WASHRT = VE.T_ITC4_HDR.WASHRT;
                                    if (VE.DefaultAction == "E")
                                    {
                                        TITC4HDR.DTAG = "E";
                                        DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == TITC4HDR.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == TITC4HDR.AUTONO));

                                        DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == TITC4HDR.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == TITC4HDR.AUTONO));

                                        //DB.T_ITC4_DTL.Where(x => x.AUTONO == TITC4HDR.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_ITC4_DTL.RemoveRange(DB.T_ITC4_DTL.Where(x => x.AUTONO == TITC4HDR.AUTONO));

                                        DB.SaveChanges();
                                    }
                                    //TCH = Cn.T_CONTROL_HDR(TITC4HDR.DOCCD, TITC4HDR.DOCDT, TITC4HDR.DOCNO, TITC4HDR.AUTONO, Month, DOCPATTERN, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, null, 0, null);
                                    //TCH = Cn.Model_T_Cntrl_Hdr(DB, TITC4HDR.AUTONO, TITC4HDR.DOCCD, TITC4HDR.DOCDT.Value, TITC4HDR.DOCNO, Month, DOCPATTERN, VE.DefaultAction);
                                    TCH = Cn.Model_T_Cntrl_Hdr(DB, TITC4HDR.AUTONO, TITC4HDR.DOCCD, TITC4HDR.DOCDT.Value, TITC4HDR.DOCNO, Month, DOCPATTERN, VE.DefaultAction, "", "", 0, "", "", "", VE.Audit_REM);

                                    if (VE.DefaultAction == "A")
                                    {
                                        DB.T_CNTRL_HDR.Add(TCH);
                                        DB.T_ITC4_HDR.Add(TITC4HDR);
                                    }
                                    else if (VE.DefaultAction == "E")
                                    {
                                        DB.Entry(TITC4HDR).State = System.Data.Entity.EntityState.Modified;
                                        //DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    DB.SaveChanges();

                                    for (int i = 0; i <= VE.TITC4DTL.Count - 1; i++)
                                    {
                                        if (VE.TITC4DTL[i].SLNO != 0 && VE.TITC4DTL[i].DOCCD != null)
                                        {
                                            T_ITC4_DTL TITC4DTL = new T_ITC4_DTL();
                                            TITC4DTL.EMD_NO = TITC4HDR.EMD_NO;
                                            TITC4DTL.CLCD = TITC4HDR.CLCD;
                                            TITC4DTL.DTAG = TITC4HDR.DTAG;
                                            TITC4DTL.TTAG = TITC4HDR.TTAG;
                                            TITC4DTL.AUTONO = TITC4HDR.AUTONO;
                                            TITC4DTL.SLNO = VE.TITC4DTL[i].SLNO;
                                            TITC4DTL.JOBCD = VE.TITC4DTL[i].JOBCD;
                                            TITC4DTL.DOCCD = VE.TITC4DTL[i].DOCCD;
                                            TITC4DTL.QNTY = VE.TITC4DTL[i].QNTY;
                                            TITC4DTL.SHORTQNTY = VE.TITC4DTL[i].SHORTQNTY;
                                            TITC4DTL.UOMCD = VE.TITC4DTL[i].UOM;
                                            TITC4DTL.RATE = VE.TITC4DTL[i].RATE;
                                            DB.T_ITC4_DTL.Add(TITC4DTL);
                                        }
                                    }

                                    if (VE.UploadDOC != null)// add DOCUMENT
                                    {
                                        var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TITC4HDR.AUTONO, TITC4HDR.EMD_NO.Value);
                                        if (img.Item1.Count != 0)
                                        {
                                            DB.T_CNTRL_HDR_DOC.AddRange(img.Item1);
                                            DB.T_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                                        }
                                    }
                                    DB.SaveChanges();
                                    string fdt = VE.T_ITC4_HDR.FROMDT.ToString().Remove(10), tdt = VE.T_ITC4_HDR.TODT.ToString().Remove(10);
                                    //Delete data from finance ITC4 Table
                                    dbsql = "delete from " + scmf + ".t_gstitc4_rec where proc_autono='" + VE.T_ITC4_HDR.AUTONO + "'";
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                                    dbsql = "delete from " + scmf + ".t_gstitc4_iss where proc_autono='" + VE.T_ITC4_HDR.AUTONO + "'";
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                                    //dbsql = "delete from " + scmf + ".t_cntrl_hdr where doccd='" + VE.T_ITC4_HDR.DOCCD + "' and (docdt >= to_date('" + fdt + "','dd/mm/yyyy') and  docdt <= to_date('" + tdt + "','dd/mm/yyyy')) ";
                                    //OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    //ModelState.Clear();
                                    //#region Update in finance table

                                    #region Issue for Job work
                                    issdoctype = "'OPRI','OEMI','OJWI','OSTI','OIRI'";

                                    sql = "";
                                    sql += "select a.autono, c.docno, c.doconlyno, c.docdt, c.mnthcd, c.vchrno, c.yr_cd, c.usr_id, c.usr_entdt, ";
                                    sql += "b.jobcd, g.jobnm, g.jobseq, h.flag1, c.doccd, d.docnm, b.slcd, e.gstno, ";
                                    sql += "a.itgrpcd, a.partcd, f.itgrpnm, f.hsncode hsnsaccd, f.prodgrpcd, a.uomcd, a.mtrljobcd, ";
                                    sql += "nvl(a.qnty, 0) qnty, nvl(a.shortqnty, 0) shortqnty from ";

                                    sql += "(select autono, uomcd, itgrpcd, partcd, mtrljobcd, sum(qnty) qnty, sum(shortqnty) shortqnty from ";
                                    sql += "(select a.autono, b.uomcd, b.itgrpcd, a.partcd, a.mtrljobcd, ";
                                    sql += "(select max(a.jobcd) jobcd from " + scm + ".t_batchdtl x, " + scm + ".t_batchmst y ";
                                    sql += "where x.barno=y.barno and x.autono=a.autono ) xmtrljobcd, ";
                                    sql += "a.qnty, a.shortqnty ";
                                    sql += "from " + scm + ".t_txndtl a, " + scm + ".m_sitem b where a.itcd=b.itcd(+) )  ";
                                    sql += "group by autono, uomcd, itgrpcd, partcd, mtrljobcd) a, ";

                                    sql += scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_doctype d, " + scmf + ".m_subleg e, ";
                                    sql += scm + ".m_group f, " + scm + ".m_jobmst g, " + scm + ".m_dtype h ";
                                    sql += "where d.doctype in (" + issdoctype + ") and b.slcd=e.slcd(+)   ";
                                    if (linkslcd.retStr() != "") sql += "and b.slcd not in (" + linkslcd + ") ";
                                    sql += "and b.jobcd=g.jobcd(+) and d.doctype=h.dcd(+) and ";
                                    sql += "a.autono = b.autono and a.autono = c.autono and c.doccd = d.doccd and nvl(d.pro, 'I') <> 'I' and a.itgrpcd = f.itgrpcd(+) and ";
                                    sql += "c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and nvl(c.cancel, 'N') = 'N' and ";
                                    sql += "c.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and c.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
                                    sql += "order by jobseq, jobnm, doccd, docdt, docno, autono, itgrpnm, partcd ";
                                    DataTable tbliss = masterHelp.SQLquery(sql);

                                    List<T_CNTRL_HDR> TCHOLDlist = DB.T_CNTRL_HDR.ToList();

                                    Int32 t = 0, maxR = tbliss.Rows.Count - 1;
                                    while (t <= maxR)
                                    {
                                        string doccd = tbliss.Rows[t]["doccd"].ToString();
                                        while (tbliss.Rows[t]["doccd"].ToString() == doccd)
                                        {
                                            string autono = tbliss.Rows[t]["autono"].ToString();

                                            T_CNTRL_HDR TCHOLD = TCHOLDlist.Where(m => m.AUTONO == autono).FirstOrDefault();
                                            TCHOLD.DOCCD = TITC4HDR.DOCCD;

                                            sql = "select autono from " + scmf + ".t_cntrl_hdr where autono='" + autono + "'";
                                            OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                            if (OraReco.HasRows == false)
                                            {
                                                //dbsql = Master_HelpFa.Instcntrl_hdr(autono, "A", "F", TCHOLD.MNTHCD, TITC4HDR.DOCCD, TCHOLD.DOCNO, TCHOLD.DOCDT.ToString(), TCHOLD.EMD_NO.Value, TCHOLD.DTAG, TCHOLD.DOCONLYNO, TCHOLD.VCHRNO, "Y", TCHOLD.VCHRSUFFIX, TCHOLD.GLCD, TCHOLD.SLCD, TCHOLD.DOCAMT.Value);
                                                dbsql = Master_HelpFa.Instcntrl_hdr(autono, "A", "F", TCHOLD.MNTHCD, TITC4HDR.DOCCD, TCHOLD.DOCNO, TCHOLD.DOCDT.ToString(), TCHOLD.EMD_NO.retShort(), TCHOLD.DTAG, TCHOLD.DOCONLYNO, TCHOLD.VCHRNO, "Y", TCHOLD.VCHRSUFFIX, TCHOLD.GLCD, TCHOLD.SLCD, TCHOLD.DOCAMT.Value, null, VE.Audit_REM);
                                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                            }
                                            OraReco.Close();
                                            //DBF.SaveChanges();
                                            short isnlo = 1;
                                            double amt = 0;
                                            string partcd = tbliss.Rows[t]["partcd"].ToString();
                                            while (tbliss.Rows[t]["autono"].ToString() == autono)
                                            {
                                                if (tbliss.Rows[t]["partcd"].ToString() == partcd)
                                                {
                                                    double rate = getRate(doccd, tbliss.Rows[t]["uomcd"].ToString(), VE);
                                                    if (tbliss.Rows[t]["docno"].ToString() == "HPAL/II/05-0007")
                                                    {
                                                        var ty = "";
                                                    }
                                                    if (tbliss.Rows[t]["mtrljobcd"].ToString() == "WS")
                                                    {
                                                        rate = rate + VE.T_ITC4_HDR.WASHRT.Value;
                                                    }

                                                    amt = (tbliss.Rows[t]["qnty"].retDbl() * rate).toRound(2);

                                                    dbsql = Master_HelpFa.InsITC4_Iss(autono, tbliss.Rows[t]["doccd"].ToString(), tbliss.Rows[t]["doconlyno"].ToString(), tbliss.Rows[t]["docdt"].ToString(), isnlo, "4", TITC4HDR.AUTONO, tbliss.Rows[t]["slcd"].ToString(), tbliss.Rows[t]["gstno"].ToString(), tbliss.Rows[t]["docno"].ToString(), tbliss.Rows[t]["docdt"].ToString(), tbliss.Rows[t]["hsnsaccd"].ToString(), tbliss.Rows[t]["itgrpnm"].ToString(), tbliss.Rows[t]["uomcd"].ToString(), tbliss.Rows[t]["qnty"].retDbl(), amt, 2.5, 2.5, 0, 0, 0);
                                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                                    isnlo++;
                                                }
                                                t++;
                                                if (t > maxR) break;
                                            }
                                            if (t > maxR) break;
                                        }
                                    }
                                    #endregion
                                    #region Receive/Return from Job work
                                    recdoctype = "'ODYR','ODYU','OPRR','OPRU','OEMR','OEMU','OJWR','OJWU','OSTR','OSTU','OIRR','OOIU' ";
                                    string qtyfld = "nvl(a.qnty,b.qnty)-nvl(b.shortqnty,0)";

                                    sql = "select a.autono, c.docno, c.doconlyno, c.docdt, b.jobcd, g.jobnm, g.jobseq, h.flag1, c.doccd, d.docnm, b.slcd, e.gstno, ";
                                    sql += "a.itgrpcd, a.partcd, f.itgrpnm, f.hsncode hsnsaccd, f.prodgrpcd, a.uomcd, i.docno orgdocno, i.docdt orgdocdt, ";
                                    sql += "nvl(a.qnty, 0) qnty, nvl(a.shortqnty, 0) shortqnty from ";
                                    sql += "(select a.autono, a.progautono, d.uomcd, d.itgrpcd, b.partcd, sum(nvl(b.shortqnty,0)) shortqnty, ";
                                    sql += "sum(nvl((case e.jobcd when 'DY' then " + qtyfld + " when 'BL' then " + qtyfld + " ";
                                    sql += "when 'KT' then " + qtyfld + " when 'YD' then " + qtyfld + " else ";
                                    sql += "nvl(b.qnty,a.qnty) end ),0)) qnty ";
                                    sql += "from " + scm + ".t_progdtl a, " + scm + ".t_txndtl b, " + scm + ".t_txn c, " + scm + ".m_sitem d, " + scm + ".t_batchmst e ";
                                    sql += "where a.autono=b.autono(+) and a.slno=b.slno(+) and ";
                                    //sql += "where a.autono=b.recprogautono(+) and a.slno=b.recprogslno(+) and ";
                                    sql += "a.autono=c.autono(+) and b.itcd=d.itcd and a.progautono=e.autono(+) and a.progslno=e.slno(+) ";
                                    sql += "group by a.autono, a.progautono, d.uomcd, d.itgrpcd, b.partcd) a, ";
                                    sql += scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_doctype d, " + scmf + ".m_subleg e, ";
                                    sql += scm + ".m_group f, " + scm + ".m_jobmst g, " + scm + ".m_dtype h, " + scm + ".t_cntrl_hdr i ";
                                    sql += "where d.doctype in (" + recdoctype + ") and b.slcd=e.slcd(+)   ";
                                    if (linkslcd.retStr() != "") sql += "and b.slcd not in (" + linkslcd + ") ";
                                    sql += "and b.jobcd=g.jobcd(+) and d.doctype=h.dcd(+) and ";
                                    sql += "a.autono = b.autono and a.autono = c.autono and c.doccd = d.doccd and ";
                                    sql += "nvl(d.pro, 'I') <> 'I' and a.itgrpcd = f.itgrpcd(+) and a.progautono=i.autono(+) and ";
                                    sql += "c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and nvl(c.cancel, 'N') = 'N' and ";
                                    sql += "c.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and c.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
                                    sql += "order by jobseq, jobnm, doccd, docdt, docno, autono, orgdocno, itgrpnm, partcd ";
                                    tbliss = masterHelp.SQLquery(sql);

                                    t = 0; maxR = tbliss.Rows.Count - 1;
                                    while (t <= maxR)
                                    {
                                        string doccd = tbliss.Rows[t]["doccd"].ToString();

                                        while (tbliss.Rows[t]["doccd"].ToString() == doccd)
                                        {
                                            string autono = tbliss.Rows[t]["autono"].ToString();
                                            string partcd = tbliss.Rows[t]["partcd"].ToString();

                                            T_CNTRL_HDR TCHOLD = TCHOLDlist.Where(m => m.AUTONO == autono).FirstOrDefault();
                                            TCHOLD.DOCCD = TITC4HDR.DOCCD;

                                            sql = "select autono from " + scmf + ".t_cntrl_hdr where autono='" + autono + "'";
                                            OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                            if (OraReco.HasRows == false)
                                            {
                                                //dbsql = Master_HelpFa.Instcntrl_hdr(autono, "A", "F", TCHOLD.MNTHCD, TITC4HDR.DOCCD, TCHOLD.DOCNO, TCHOLD.DOCDT.ToString(), TCHOLD.EMD_NO.Value, TCHOLD.DTAG, TCHOLD.DOCONLYNO, TCHOLD.VCHRNO, "Y", TCHOLD.VCHRSUFFIX, TCHOLD.GLCD, TCHOLD.SLCD, TCHOLD.DOCAMT.Value);
                                                dbsql = Master_HelpFa.Instcntrl_hdr(autono, "A", "F", TCHOLD.MNTHCD, TITC4HDR.DOCCD, TCHOLD.DOCNO, TCHOLD.DOCDT.ToString(), TCHOLD.EMD_NO.retShort(), TCHOLD.DTAG, TCHOLD.DOCONLYNO, TCHOLD.VCHRNO, "Y", TCHOLD.VCHRSUFFIX, TCHOLD.GLCD, TCHOLD.SLCD, TCHOLD.DOCAMT.Value, null, VE.Audit_REM);
                                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                            }
                                            OraReco.Close();
                                            short isnlo = 1;
                                            while (tbliss.Rows[t]["autono"].ToString() == autono)
                                            {
                                                double amt = 0;
                                                string orgdocno = tbliss.Rows[t]["orgdocno"].ToString();
                                                while (tbliss.Rows[t]["autono"].ToString() == autono && tbliss.Rows[t]["orgdocno"].ToString() == orgdocno)
                                                {
                                                    if (tbliss.Rows[t]["partcd"].ToString() == partcd && tbliss.Rows[t]["qnty"].retDbl() + tbliss.Rows[t]["shortqnty"].retDbl() > 0)
                                                    {
                                                        double rate = getRate(doccd, tbliss.Rows[t]["uomcd"].ToString(), VE);
                                                        T_GSTITC4_REC TITC4ISS = new T_GSTITC4_REC();
                                                        amt = (tbliss.Rows[t]["qnty"].retDbl() * rate).toRound(2);

                                                        double shortqnty = tbliss.Rows[t]["shortqnty"].retDbl(), qnty = tbliss.Rows[t]["qnty"].retDbl();
                                                        if (shortqnty < 0)
                                                        {
                                                            //qnty = qnty + Math.Abs(shortqnty);
                                                            shortqnty = 0;
                                                        }
                                                        string shortuomcd = "";
                                                        if (shortqnty != 0) shortuomcd = tbliss.Rows[t]["uomcd"].ToString();
                                                        dbsql = Master_HelpFa.InsITC4_Rec(autono, tbliss.Rows[t]["doccd"].ToString(), tbliss.Rows[t]["doconlyno"].ToString(), tbliss.Rows[t]["docdt"].ToString(), isnlo, "5A", TITC4HDR.AUTONO, tbliss.Rows[t]["slcd"].ToString(), tbliss.Rows[t]["gstno"].ToString(), tbliss.Rows[t]["jobnm"].ToString(), tbliss.Rows[t]["orgdocno"].ToString(), tbliss.Rows[t]["orgdocdt"].ToString(), tbliss.Rows[t]["docno"].ToString(), tbliss.Rows[t]["docdt"].ToString(), tbliss.Rows[t]["hsnsaccd"].ToString(), tbliss.Rows[t]["itgrpnm"].ToString(), tbliss.Rows[t]["uomcd"].ToString(), tbliss.Rows[t]["qnty"].retDbl(), amt, shortqnty, shortuomcd);
                                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                                        isnlo++;
                                                    }
                                                    t++;
                                                    if (t > maxR) break;
                                                }
                                                if (t > maxR) break;
                                            }
                                            if (t > maxR) break;
                                        }
                                    }
                                    #endregion
                                    #region Cutter Receive voucher adjust on FIFO
                                    //sql = "select a.autono, a.partcd, c.docdt, c.docno, c.doconlyno, c.doccd, b.doctag, b.slcd, g.slnm, g.gstno, ";
                                    //sql += "a.itgrptype, a.itgrpcd, f.itgrpnm, f.hsnsaccd, a.stkdrcr, a.uomcd, b.jobcd, h.jobnm, ";
                                    //sql += "nvl(b.cloth_used,0) cloth_used, nvl(b.cloth_was,0) cloth_was, ";
                                    //sql += "nvl(a.wght, 0) wght, nvl(decode(f.itgrptype, 'F', a.qnty, 0),0) pcs, ";
                                    //sql += "(case f.itgrptype when 'T' then 0 when 'W' then 0 when 'F' then 0 else nvl(a.qnty, 0) end) qnty, ";
                                    //sql += "nvl(decode(f.itgrptype, 'T', a.qnty,0),0) foldwt, nvl(decode(f.itgrptype, 'W', a.qnty),0) waswt from ";

                                    //sql += "( select a.autono, e.itgrpcd, e.uomcd, f.itgrptype, a.partcd, a.stkdrcr, sum(a.wght) wght, sum(a.qnty) qnty ";
                                    //sql += "from " + scm + ".t_txndtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".m_doctype c, " + scm + ".t_txn d, ";
                                    //sql += scm + ".m_sitem e, " + scm + ".m_group f ";
                                    //sql += "where a.autono=b.autono(+) and b.doccd=c.doccd(+) and a.autono=d.autono(+) and ";
                                    //sql += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' and nvl(b.cancel,'N')='N' and nvl(c.pro,'O') <> 'I' and ";
                                    //sql += "b.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and a.itcd=e.itcd and e.itgrpcd=f.itgrpcd(+) and ";
                                    //sql += "c.doctype in ('OCTI','OCTR','OCTU') and d.doctag in ('JC','JR','JU') and b.slcd not in (" + linkslcd + ") ";
                                    //sql += "group by a.autono, e.itgrpcd, e.uomcd, f.itgrptype, a.partcd, a.stkdrcr ) a, ";
                                    //sql += "" + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_parts e, ";
                                    //sql += "" + scm + ".m_group f, " + scmf + ".m_subleg g, " + scm + ".m_jobmst h ";
                                    //sql += "where a.autono=b.autono and b.autono=c.autono and a.itgrpcd=f.itgrpcd and ";
                                    //sql += "a.partcd=e.partcd(+) and b.slcd=g.slcd(+) and b.jobcd=h.jobcd(+) ";
                                    //sql += "order by slcd, docdt, docno ";

                                    //DataTable rsiss = new DataTable("stock");
                                    //rsiss.Columns.Add("autono", typeof(string), "");
                                    //rsiss.Columns.Add("uomcd", typeof(string), "");
                                    //rsiss.Columns.Add("itgrpcd", typeof(string), "");
                                    //rsiss.Columns.Add("itgrpnm", typeof(string), "");
                                    //rsiss.Columns.Add("slcd", typeof(string), "");
                                    //rsiss.Columns.Add("gstno", typeof(string), "");
                                    //rsiss.Columns.Add("docno", typeof(string), "");
                                    //rsiss.Columns.Add("docdt", typeof(string), "");
                                    //rsiss.Columns.Add("balqnty", typeof(double), "");
                                    //rsiss.Columns.Add("qnty", typeof(double), "");
                                    //rsiss.Columns.Add("befrecqnty", typeof(double), "");
                                    //rsiss.Columns.Add("recqnty", typeof(double), "");
                                    //rsiss.Columns.Add("shortuomcd", typeof(string), "");
                                    //rsiss.Columns.Add("shortqnty", typeof(double), "");
                                    //rsiss.Columns.Add("recautono", typeof(string), "");

                                    //string sqld = "select autono, slcd, gstno, itgrpcd, itgrpnm, partcd, uomcd, qnty, docno, docdt, doccd ";
                                    //sqld += "from ( " + sql + ") where doctag='JC' ";
                                    //sqld += "order by slcd, uomcd, docdt, docno ";
                                    //DataTable fifotbl = masterHelp.SQLquery(sqld);
                                    //Int32 f = 0, rNo = 0, oldf = 0;
                                    //maxR = fifotbl.Rows.Count - 1;
                                    //while (f <= maxR)
                                    //{
                                    //    rsiss.Rows.Add(); rNo = rsiss.Rows.Count - 1;
                                    //    rsiss.Rows[rNo]["autono"] = fifotbl.Rows[f]["autono"];
                                    //    rsiss.Rows[rNo]["slcd"] = fifotbl.Rows[f]["slcd"];
                                    //    rsiss.Rows[rNo]["gstno"] = fifotbl.Rows[f]["gstno"];
                                    //    rsiss.Rows[rNo]["uomcd"] = fifotbl.Rows[f]["uomcd"];
                                    //    rsiss.Rows[rNo]["itgrpcd"] = fifotbl.Rows[f]["itgrpcd"];
                                    //    rsiss.Rows[rNo]["itgrpnm"] = fifotbl.Rows[f]["itgrpnm"];
                                    //    rsiss.Rows[rNo]["docno"] = fifotbl.Rows[f]["docno"];
                                    //    rsiss.Rows[rNo]["docdt"] = fifotbl.Rows[f]["docdt"].ToString().retDateStr();
                                    //    rsiss.Rows[rNo]["qnty"] = fifotbl.Rows[f]["qnty"].retDbl();
                                    //    rsiss.Rows[rNo]["balqnty"] = fifotbl.Rows[f]["qnty"].retDbl();
                                    //    rsiss.Rows[rNo]["befrecqnty"] = 0;
                                    //    rsiss.Rows[rNo]["recqnty"] = 0;
                                    //    rsiss.Rows[rNo]["shortqnty"] = 0;
                                    //    f++;
                                    //}

                                    //sqld = "select autono, itgrpcd, itgrpnm, partcd, uomcd, qnty, slcd, docno, docdt, doccd, jobnm, ";
                                    //sqld += "hsnsaccd, doconlyno, wght, pcs, waswt, foldwt, cloth_used, cloth_was ";
                                    //sqld += "from ( " + sql + ") where doctag <> 'JC' and docdt >= to_date('" + CommVar.FinStartDate(UNQSNO) + "','dd/mm/yyyy') ";
                                    //sqld += "order by slcd, docdt, docno, partcd ";
                                    //DataTable fiforec = masterHelp.SQLquery(sqld);
                                    //f = 0; maxR = fiforec.Rows.Count - 1; rNo = 0;
                                    //while (f <= maxR)
                                    //{
                                    //    string autono = fiforec.Rows[f]["autono"].ToString();
                                    //    string slcd = fiforec.Rows[f]["slcd"].ToString();
                                    //    string partcd = "";
                                    //    DateTime recdt = Convert.ToDateTime(fiforec.Rows[f]["docdt"].retStr());
                                    //    double cl_used = fiforec.Rows[f]["cloth_used"].retDbl();
                                    //    double cl_was = fiforec.Rows[f]["cloth_was"].retDbl();
                                    //    double r_qty = 0, w_qty = 0;
                                    //    bool hdrrecoins = true;

                                    //    while (fiforec.Rows[f]["autono"].ToString() == autono)
                                    //    {
                                    //        bool recins = true;
                                    //        if (recins == true)
                                    //        {
                                    //            r_qty = r_qty + fiforec.Rows[f]["wght"].retDbl() + fiforec.Rows[f]["foldwt"].retDbl();
                                    //            w_qty = w_qty + fiforec.Rows[f]["waswt"].retDbl();
                                    //        }
                                    //        f++;
                                    //        if (f > maxR) break;
                                    //    }
                                    //    oldf = f;
                                    //    f = f - 1;
                                    //    Int32 maxl = 0;
                                    //    if (cl_used + cl_was > 0) maxl = 1;
                                    //    for (Int32 l = 0; l <= maxl; l++)
                                    //    {
                                    //        Int16 isnlo = 1;
                                    //        string chkuomcd = "KGS";
                                    //        double chkqty = r_qty, chkwas = w_qty;

                                    //        if (l == 1)
                                    //        {
                                    //            isnlo = 51;
                                    //            chkuomcd = "MTR";
                                    //            chkqty = cl_used; chkwas = cl_was;
                                    //        }
                                    //        double checkqnty = chkqty + chkwas, rplqnty = 0;
                                    //        for (Int32 z = 0; z <= rsiss.Rows.Count - 1; z++)
                                    //        {
                                    //            double balqnty = rsiss.Rows[z]["balqnty"].retDbl();
                                    //            if (rsiss.Rows[z]["slcd"].ToString() == slcd && rsiss.Rows[z]["uomcd"].ToString() == chkuomcd && balqnty > 0)
                                    //            {
                                    //                if (balqnty <= checkqnty)
                                    //                {
                                    //                    checkqnty = checkqnty - balqnty; rplqnty = balqnty; balqnty = 0;
                                    //                    balqnty = 0;
                                    //                }
                                    //                else
                                    //                {
                                    //                    balqnty = balqnty - checkqnty; rplqnty = checkqnty; checkqnty = 0;
                                    //                }
                                    //                rsiss.Rows[z]["balqnty"] = balqnty;
                                    //                if (recdt < Convert.ToDateTime(fdt))
                                    //                {
                                    //                    rsiss.Rows[z]["befrecqnty"] = rsiss.Rows[z]["befrecqnty"].retDbl() + rplqnty;
                                    //                }
                                    //                else
                                    //                {
                                    //                    rsiss.Rows[z]["recqnty"] = rsiss.Rows[z]["recqnty"].retDbl() + rplqnty;
                                    //                    //
                                    //                    //DataTable tblold = masterHelp.SQLquery("select * from " + scmf + ".t_cntrl_hdr where autono='" + autono + "'");
                                    //                    if (hdrrecoins == true)
                                    //                    {

                                    //                        T_CNTRL_HDR TCHOLD = TCHOLDlist.Where(m => m.AUTONO == autono).FirstOrDefault();
                                    //                        TCHOLD.DOCCD = TITC4HDR.DOCCD;

                                    //                        //T_CNTRL_HDR TCHOLD = new T_CNTRL_HDR();
                                    //                        //TCHOLD = DB.T_CNTRL_HDR.Find(autono);
                                    //                        //TCHOLD.DOCCD = TITC4HDR.DOCCD;

                                    //                        sql = "select autono from " + scmf + ".t_cntrl_hdr where autono='" + autono + "'";
                                    //                        OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                    //                        if (OraReco.HasRows == false)
                                    //                        {
                                    //                            //dbsql = Master_HelpFa.Instcntrl_hdr(autono, "A", "F", TCHOLD.MNTHCD, TITC4HDR.DOCCD, TCHOLD.DOCNO, TCHOLD.DOCDT.ToString(), TCHOLD.EMD_NO.Value, TCHOLD.DTAG, TCHOLD.DOCONLYNO, TCHOLD.VCHRNO, "Y", TCHOLD.VCHRSUFFIX, TCHOLD.GLCD, TCHOLD.SLCD, TCHOLD.DOCAMT.Value);
                                    //                            dbsql = Master_HelpFa.Instcntrl_hdr(autono, "A", "F", TCHOLD.MNTHCD, TITC4HDR.DOCCD, TCHOLD.DOCNO, TCHOLD.DOCDT.ToString(), TCHOLD.EMD_NO.retShort(), TCHOLD.DTAG, TCHOLD.DOCONLYNO, TCHOLD.VCHRNO, "Y", TCHOLD.VCHRSUFFIX, TCHOLD.GLCD, TCHOLD.SLCD, TCHOLD.DOCAMT.Value, null, VE.Audit_REM);
                                    //                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    //                        }
                                    //                        OraReco.Close();
                                    //                        hdrrecoins = false;
                                    //                    }
                                    //                    //DBF.T_CNTRL_HDR.Add(TCHOLD);

                                    //                    if (rplqnty != 0)
                                    //                    {
                                    //                        double rate = getRate(fiforec.Rows[f]["doccd"].ToString(), chkuomcd, VE);
                                    //                        T_GSTITC4_REC TITC4ISS = new T_GSTITC4_REC();
                                    //                        double amt = (rplqnty * rate).toRound(2);

                                    //                        double shortqnty = chkwas, qnty = rplqnty;
                                    //                        string shortuomcd = "";
                                    //                        if (shortqnty > rplqnty)
                                    //                        {
                                    //                            shortqnty = 0;
                                    //                        }
                                    //                        else
                                    //                        {
                                    //                            rplqnty = rplqnty - shortqnty;
                                    //                            chkwas = 0;
                                    //                        }
                                    //                        qnty = rplqnty;
                                    //                        if (shortqnty < 0)
                                    //                        {
                                    //                            //qnty = rplqnty + Math.Abs(shortqnty);
                                    //                            shortqnty = 0;
                                    //                        }

                                    //                        if (shortqnty > 0) shortuomcd = rsiss.Rows[z]["uomcd"].ToString();
                                    //                        dbsql = Master_HelpFa.InsITC4_Rec(autono, fiforec.Rows[f]["doccd"].ToString(), fiforec.Rows[f]["doconlyno"].ToString(), fiforec.Rows[f]["docdt"].ToString(), isnlo, "5A", TITC4HDR.AUTONO, slcd, rsiss.Rows[z]["gstno"].ToString(), fiforec.Rows[f]["jobnm"].ToString(), rsiss.Rows[z]["docno"].ToString(), rsiss.Rows[z]["docdt"].ToString(), fiforec.Rows[f]["docno"].ToString(), fiforec.Rows[f]["docdt"].ToString(), fiforec.Rows[z]["hsnsaccd"].ToString(), rsiss.Rows[z]["itgrpnm"].ToString(), rsiss.Rows[z]["uomcd"].ToString(), qnty, amt, shortqnty, shortuomcd);
                                    //                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    //                        isnlo++;
                                    //                    }
                                    //                    //
                                    //                }
                                    //                if (checkqnty == 0) break;
                                    //            }
                                    //        }
                                    //    } //kgs/mtr loop
                                    //    f++;
                                    //    if (f > maxR) break;
                                    //}
                                    #endregion

                                    transaction.Commit();
                                    OraTrans.Commit();
                                    OraCon.Dispose();
                                    string ContentFlg = "";
                                    if (VE.DefaultAction == "A")
                                    {
                                        ContentFlg = "1" + "<br/><br/>" + TITC4HDR.DOCCD + TITC4HDR.DOCNO + "~" + TITC4HDR.AUTONO;
                                    }
                                    else if (VE.DefaultAction == "E")
                                    {
                                        ContentFlg = "2";
                                    }
                                    return Content(ContentFlg);
                                }
                                else if (VE.DefaultAction == "V")
                                {
                                    //Cn.Model_T_Cntrl_Hdr(DB, VE.T_ITC4_HDR.AUTONO, "", System.DateTime.Now.Date, "", "", "", VE.DefaultAction);
                                    Cn.Model_T_Cntrl_Hdr(DB, VE.T_ITC4_HDR.AUTONO, "", System.DateTime.Now.Date, "", "", "", VE.DefaultAction, "", "", 0, "", "", "", VE.Audit_REM);

                                    DB.T_ITC4_HDR.Where(x => x.AUTONO == VE.T_ITC4_HDR.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                                    DB.T_ITC4_DTL.Where(x => x.AUTONO == VE.T_ITC4_HDR.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                                    DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_ITC4_HDR.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                                    DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.T_ITC4_HDR.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                                    DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.T_ITC4_HDR.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                                    DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.T_ITC4_HDR.AUTONO));
                                    DB.SaveChanges();

                                    DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.T_ITC4_HDR.AUTONO));
                                    DB.SaveChanges();

                                    DB.T_ITC4_DTL.RemoveRange(DB.T_ITC4_DTL.Where(x => x.AUTONO == VE.T_ITC4_HDR.AUTONO));
                                    DB.SaveChanges();

                                    DB.T_ITC4_HDR.RemoveRange(DB.T_ITC4_HDR.Where(x => x.AUTONO == VE.T_ITC4_HDR.AUTONO));
                                    DB.SaveChanges();

                                    DB.T_CNTRL_HDR.RemoveRange(DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_ITC4_HDR.AUTONO));
                                    DB.SaveChanges();

                                    string fdt = VE.T_ITC4_HDR.FROMDT.ToString().Remove(10), tdt = VE.T_ITC4_HDR.TODT.ToString().Remove(10);
                                    //Delete data from finance ITC4 Table
                                    dbsql = "delete from " + scmf + ".t_gstitc4_rec where proc_autono='" + VE.T_ITC4_HDR.AUTONO + "'";
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                                    dbsql = "delete from " + scmf + ".t_gstitc4_iss where proc_autono='" + VE.T_ITC4_HDR.AUTONO + "'";
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                                    dbsql = "update " + scmf + ".t_cntrl_hdr set LM_REM='" + VE.Audit_REM + "' where doccd='" + VE.T_ITC4_HDR.DOCCD + "' and (compcd='" + COM + "' and loccd='" + LOC + "' and docdt >= to_date('" + fdt + "','dd/mm/yyyy') and  docdt <= to_date('" + tdt + "','dd/mm/yyyy')) ";
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                                    dbsql = "delete from " + scmf + ".t_cntrl_hdr where doccd='" + VE.T_ITC4_HDR.DOCCD + "' and (compcd='" + COM + "' and loccd='" + LOC + "' and docdt >= to_date('" + fdt + "','dd/mm/yyyy') and  docdt <= to_date('" + tdt + "','dd/mm/yyyy')) ";
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

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
                            }
                            catch (DbEntityValidationException ex)
                            {
                                transaction.Rollback();
                                OraTrans.Rollback();
                                OraCon.Dispose();
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
                            OraTrans.Rollback();
                            OraCon.Dispose();
                            ModelState.Clear();
                            Cn.SaveException(ex, "");
                            return Content(ex.Message + ex.InnerException + " " + sql);
                        }
                    }

                }
            }
        }

        public double getRate(string doccd, String uomcd, ITC4Generation VE)
        {
            double rate = 0;
            double jobseq = 0; string flag1 = "";
            var jobrow = (from p in VE.TITC4DTL
                          where p.DOCCD == doccd && p.UOM == uomcd
                          select new
                          {
                              jobseq = p.JOBSEQ,
                              flag1 = p.FLAG1,
                              rate = p.RATE,
                          }).ToList();

            if (jobrow.Count > 0)
            {
                jobseq = jobrow[0].jobseq.retDbl();
                flag1 = jobrow[0].flag1;
                rate = rate + jobrow[0].rate.retDbl();
            }

            //var docrate = (from p in VE.TITC4DTL
            //               where p.JOBSEQ <= jobseq
            //               group p by 0 into gcs1
            //               select new
            //               {
            //                   rate = gcs1.Sum(x => x.RATE),
            //               }).ToList();
            //if (docrate.Count > 0)
            //{
            //    rate = Convert.ToDouble(docrate[0].rate);
            //}
            //var docrate = (from p in VE.TITC4DTL
            //               where p.JOBSEQ <= jobseq && p.RATE > 0
            //               select new
            //               {
            //                   rate = p.RATE,
            //                   flag1 = p.FLAG1,
            //                   jobseq = p.JOBSEQ
            //               }).OrderBy(a => a.jobseq).OrderBy(b => b.flag1).ToList();
            //for (int i = 0; i <= docrate.Count() - 1; i++)
            //{
            //    rate = rate + Convert.ToDouble(docrate[i].rate);
            //}
            return rate;
        }

        public ActionResult cancelRecords(ITC4Generation VE, string par1)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                Cn.getQueryString(VE);
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_ITC4_HDR.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_ITC4_HDR.AUTONO, CommVar.CurSchema(UNQSNO).ToString(), par1);
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
        public ActionResult ParkRecord(FormCollection FC, ITC4Generation stream, string menuID, string menuIndex)
        {
            try
            {
                Connection cn = new Connection();
                string ID = menuID + menuIndex + CommVar.Loccd(UNQSNO) + CommVar.Compcd(UNQSNO) + CommVar.CurSchema(UNQSNO).ToString() + "*" + DateTime.Now;
                ID = ID.Replace(" ", "_");
                string Userid = Session["UR_ID"].ToString();
                INI Handel_ini = new INI();
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JR = javaScriptSerializer.Serialize(stream);
                Handel_ini.IniWriteValue(Userid, ID, cn.Encrypt(JR), Server.MapPath("~/Park.ini"));
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public string RetrivePark(string value)
        {
            try
            {
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.ITC4Generation");
                return url;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return null;
            }
        }

    }
}