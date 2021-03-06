using Improvar.Models;
using Improvar.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;


namespace Improvar.Controllers
{
    public class T_AckController : Controller
    {
        // GET: T_Ack
        Connection Cn = new Connection();
        string CS = null;
        MasterHelp masterHelp = new MasterHelp();
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        T_TXNACK sl; T_CNTRL_HDR sll; ImprovarDB DB; T_CNTRL_HDR_REM SLR;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult T_Ack(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {

                    TTxnAckEntry VE = (parkID == "") ? new TTxnAckEntry() : (TTxnAckEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    string[] param = VE.MENU_PARA.Split(',');
                    if (param.Length > 1)
                    {
                        VE.ACK_FLAG1 = param[0];
                        VE.ACK_DOCCTG = param[1];
                    }
                    switch (VE.ACK_FLAG1)
                    {
                        case "COUR": ViewBag.formname = "Courier Entry"; break;
                        case "SIGN": ViewBag.formname = "Signed Chalan"; break;
                    }
                    //string SCHEMA = Cn.Getschema;
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    //ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string YRCD = Convert.ToString(Convert.ToDouble(CommVar.YearCode(UNQSNO)));

                    //VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    //VE.Database_Combo1 = (from n in DB.T_TXNACK
                    //                      select new Database_Combo1() { FIELD_VALUE = n.BUDGCTG }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    if (op.Length != 0)
                    {
                        var IndexKey_tmp = (from p in DB.T_TXNACK
                                            join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                            orderby p.DOCDT, p.AUTONO
                                            where (q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YRCD && p.FLAG1 == VE.ACK_FLAG1)
                                            select new
                                            {
                                                AUTONO = p.AUTONO,
                                                FLAG1 = p.FLAG1,
                                                DOCDT = q.DOCDT
                                            }).OrderBy(s => s.DOCDT).ToList();

                        VE.IndexKey = (from p in IndexKey_tmp
                                       select new IndexKey()
                                       {
                                           Navikey = p.AUTONO + Cn.GCS() + p.FLAG1
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
                            VE.T_TXNACK = sl;
                            VE.T_CNTRL_HDR = sll;
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {
                                T_TXNACK TTXNACK = new T_TXNACK();
                                TTXNACK.DOCDT = Cn.getCurrentDate(VE.mindate);
                                VE.T_TXNACK = TTXNACK;
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


                            VE = (TTxnAckEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        string docdt = "";
                        if (VE.T_CNTRL_HDR != null) if (VE.T_CNTRL_HDR.DOCDT != null) docdt = VE.T_CNTRL_HDR.DOCDT.ToString().Remove(10);
                        Cn.getdocmaxmindate(VE.BLDOCCD, docdt, VE.DefaultAction, VE.BLDOCNO, VE);
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
                TTxnAckEntry VE = new TTxnAckEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public TTxnAckEntry Navigation(TTxnAckEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            try
            {
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                sl = new T_TXNACK(); sll = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM();
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
                    sl = DB.T_TXNACK.Find(aa[0], aa[1]);
                    sll = DB.T_CNTRL_HDR.Find(sl.AUTONO);
                    if (sl.TRSLCD != null)
                    {
                        VE.TRSLNM = DBF.M_SUBLEG.Find(sl.TRSLCD).SLNM;
                    }
                    string sql = "";
                    sql += " select a.doccd,b.docnm,a.docno,a.docdt,a.slcd,c.slnm,d.translcd,e.slnm translnm,D.LRNO,D.LRDT ";
                    sql += " from " + CommVar.CurSchema(UNQSNO) + ".t_txn a," + CommVar.CurSchema(UNQSNO) + ".m_doctype b," + CommVar.FinSchema(UNQSNO) + ".m_subleg c," + CommVar.CurSchema(UNQSNO) + ".t_txntrans d," + CommVar.FinSchema(UNQSNO) + ".m_subleg e ";
                    sql += " where a.doccd=b.doccd and a.slcd=c.slcd and d.translcd=e.slcd(+)  ";
                    sql += " and a.autono='" + aa[0] + "' and D.AUTONO='" + aa[0] + "'  ";
                    sql += " and rownum=1  ";
                    var dt = masterHelp.SQLquery(sql);
                    foreach (DataRow row in dt.Rows)
                    {
                        VE.BLDOCCD = row["doccd"].ToString();
                        VE.BLDOCNM = row["docnm"].ToString();
                        VE.BLDOCNO = row["docno"].ToString();
                        VE.BLDOCDT = row["docdt"].ToString().Remove(10);
                        VE.BLSLCD = row["slcd"].ToString();
                        VE.BLSLNM = row["slnm"].ToString();
                        VE.BLTRANSLCD = row["translcd"].ToString();
                        VE.BLTRANSLNM = row["translnm"].ToString();
                        VE.BLLRNO = row["LRNO"].ToString();
                        VE.BLLRDT = row["LRDT"].ToString() == "" ? "" : row["LRDT"].ToString().Remove(10);
                    }
                    VE.BLAUTONO = aa[0];
                    VE.UploadDOC = GetUploadImageTransaction_ACK(CommVar.CurSchema(UNQSNO).ToString(), sl.AUTONO, VE.ACK_DOCCTG);

                    //var splnm = DBF.M_SUBLEG.Find(sl.TRSLCD);
                    //if (splnm != null)
                    //{
                    //    VE.SLNM = splnm.SLNM;
                    //}
                    //if (sll.CANCEL == "Y")
                    //{
                    //    VE.CancelRecord = true;
                    //}
                    //else
                    //{
                    //    VE.CancelRecord = false;
                    //}
                }
                else
                {


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
            TTxnAckEntry VE = new TTxnAckEntry();
            Cn.getQueryString(VE);
            string[] param = VE.MENU_PARA.Split(',');
            if (param.Length > 1)
            {
                VE.ACK_FLAG1 = param[0];
                VE.ACK_DOCCTG = param[1];
            }
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string LOC = CommVar.Loccd(UNQSNO);
            string COM = CommVar.Compcd(UNQSNO);
            string YRCD = Convert.ToString(Convert.ToDouble(CommVar.YearCode(UNQSNO)));
            var query = (from p in DB.T_TXNACK
                         join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                         join j in DB.T_TXN on p.AUTONO equals (j.AUTONO)
                         where (q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YRCD && p.FLAG1 == VE.ACK_FLAG1)
                         orderby p.AUTONO
                         select new { p.AUTONO, q.DOCNO, q.DOCCD, p.DOCDT, j.SLCD, p.FLAG1 }).ToList().Select(x => new
                         {
                             AUTONO = x.AUTONO,
                             DOCNO = x.DOCNO,
                             DOCCD = x.DOCCD,
                             DOCDT = x.DOCDT.ToString(),
                             SLCD = x.SLCD,
                             FLAG1 = x.FLAG1
                         }).Distinct().OrderBy(s => s.AUTONO).ToList();
            var tquery = from q in DBF.M_SUBLEG
                         select q;
            var MDT = (from a in query
                       join b in tquery on a.SLCD equals b.SLCD
                       select new
                       {
                           DOCCD = a.DOCCD,
                           DOCNO = a.DOCNO,
                           DOCDT = a.DOCDT,
                           SLCD = a.SLCD,
                           SLNM = b.SLNM,
                           AUTONO = a.AUTONO,
                           FLAG1 = a.FLAG1
                       }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Doc Code" + Cn.GCS() + "Doc No" + Cn.GCS() + "Doc Date" + Cn.GCS() + "Party Code" + Cn.GCS() + "Party Name" + Cn.GCS() + "AUTO NO" + Cn.GCS() + "FLAG 1";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].DOCCD + "</td><td>" + MDT[j].DOCNO + "</td><td>" + MDT[j].DOCDT.retStr().Remove(10) + "</td><td>" + MDT[j].SLCD + "</td><td>" + MDT[j].SLNM + "</td><td>" + MDT[j].AUTONO + "</td><td>" + MDT[j].FLAG1 + "</td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "5" + Cn.GCS() + "6", "5" + Cn.GCS() + 6));
        }

        public ActionResult GetDOC_Code(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.DOCCD_help(val, "", ""));
                }
                else
                {
                    string str = masterHelp.DOCCD_help(val, "", "");
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetDOC_Number(string val, string Code)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.DOCNO_help(val, Code));
                }
                else
                {
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                    var query1 = (from c in DB.T_CNTRL_HDR
                                  join j in DB.T_TXNTRANS on c.AUTONO equals j.AUTONO into g
                                  from j in g.DefaultIfEmpty()
                                  where c.DOCCD == Code && c.DOCONLYNO == val
                                  orderby c.DOCDT, c.DOCONLYNO
                                  select new
                                  {
                                      DOCONLYNO = c.DOCONLYNO,
                                      DOCDT = c.DOCDT,
                                      AUTONO = c.AUTONO,
                                      SLCD = c.SLCD,
                                      TRANSLCD = j.TRANSLCD,
                                      LRNO = j.LRNO,
                                      LRDT = j.LRDT
                                  }).ToList();
                    var tquery1 = from q in DBF.M_SUBLEG
                                  select q;
                    var tquery2 = from q in DBF.M_SUBLEG
                                  select q;
                    var query = (from a in query1
                                 join b in tquery1 on a.SLCD equals b.SLCD
                                 //join c in tquery2 on a.TRANSLCD equals c.SLCD into g
                                 //from c in g.DefaultIfEmpty()
                                 select new
                                 {
                                     DOCONLYNO = a.DOCONLYNO,
                                     DOCDT = a.DOCDT != null ? a.DOCDT.ToString().Remove(10) : null,
                                     AUTONO = a.AUTONO,
                                     SLCD = a.SLCD,
                                     TRANSLCD = a.TRANSLCD,
                                     LRNO = a.LRNO,
                                     LRDT = a.LRDT != null ? a.LRDT.ToString().Remove(10) : null,
                                     SLNM = b.SLNM + "  [" + b.DISTRICT + "]",
                                     //    TRANSLNM = c.SLNM + "[" + c.DISTRICT + "]",
                                 }
                               ).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            string trslnm = "";
                            str = masterHelp.ToReturnFieldValues(query);
                            if (i.TRANSLCD != null)
                            {
                                var M_SUBLEG = DBF.M_SUBLEG.Find(i.TRANSLCD);
                                trslnm = M_SUBLEG.SLNM + "  [" + M_SUBLEG.DISTRICT + "]";
                            }
                            str += "^TRANSLNM=^" + trslnm + Cn.GCS();
                        }
                        return Content(str);
                    }
                    else
                    {
                        return Content("Invalid Document Number ! Please Select / Enter a Valid Document Number !!");
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                var str = Master_HelpFa.SLCD_help(val, Code);
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
        public ActionResult AddDOCRow(TTxnAckEntry VE)
        {
            try
            {
                ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                var doctP = (from i in DBI.MS_DOCCTG
                             where i.DOC_CTG == VE.ACK_DOCCTG
                             select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();

                if (VE.UploadDOC == null)
                {
                    List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                    UploadDOC UPL = new UploadDOC();
                    UPL.DocumentType = doctP;
                    MLocIFSC1.Add(UPL);
                    VE.UploadDOC = MLocIFSC1;
                }
                else
                {
                    List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                    for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                    {
                        UploadDOC updc = new UploadDOC();
                        updc = VE.UploadDOC[i];
                        updc.DocumentType = Cn.DOC_TYPE();
                        MLocIFSC1.Add(updc);
                    }
                    UploadDOC MLI1 = new UploadDOC();
                    MLI1.DocumentType = doctP;
                    MLocIFSC1.Add(MLI1);
                    VE.UploadDOC = MLocIFSC1;
                }
                VE.DefaultView = true;
                return PartialView("_UPLOADDOCUMENTS", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult DeleteDOCRow(TTxnAckEntry VE)
        {
            try
            {
                ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                var doctP = (from i in DBI.MS_DOCCTG
                             where i.DOC_CTG == VE.ACK_DOCCTG
                             select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();

                List<UploadDOC> LOCAIFSC = new List<UploadDOC>();
                int count = 0;
                for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                {
                    if (VE.UploadDOC[i].chk == false)
                    {
                        count += 1;
                        UploadDOC updc = new UploadDOC();
                        updc = VE.UploadDOC[i];
                        updc.DocumentType = doctP;
                        LOCAIFSC.Add(updc);
                    }
                }
                VE.UploadDOC = LOCAIFSC;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_UPLOADDOCUMENTS", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

        public ActionResult SAVE(FormCollection FC, TTxnAckEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    try
                    {
                        Cn.getQueryString(VE);
                        //    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                        if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                        {
                            T_TXNACK TTXNACK = new T_TXNACK();
                            TTXNACK.EMD_NO = 0;
                            TTXNACK.CLCD = CommVar.ClientCode(UNQSNO);
                            TTXNACK.AUTONO = VE.BLAUTONO;
                            TTXNACK.FLAG1 = VE.ACK_FLAG1;
                            TTXNACK.DOCDT = VE.T_TXNACK.DOCDT;
                            TTXNACK.TRSLCD = VE.T_TXNACK.TRSLCD;
                            TTXNACK.REFNO = VE.T_TXNACK.REFNO;
                            TTXNACK.REFDT = VE.T_TXNACK.REFDT;
                            TTXNACK.PERSNAME = VE.T_TXNACK.PERSNAME;
                            TTXNACK.REMARKS = VE.T_TXNACK.REMARKS;
                            TTXNACK.WT = VE.T_TXNACK.WT;
                            TTXNACK.AMT = VE.T_TXNACK.AMT;
                            TTXNACK.USR_ID = CommVar.UserID();
                            TTXNACK.USR_ENTDT = Convert.ToDateTime(CommVar.CurrDate(UNQSNO));
                            TTXNACK.USR_LIP = Cn.GetIp();
                            TTXNACK.USR_SIP = Cn.GetStaticIp();
                            TTXNACK.USR_OS = null;
                            TTXNACK.USR_MNM = Cn.DetermineCompName(Cn.GetIp());  //GetMachin; 
                            var doclst = (from p in DB.T_CNTRL_HDR_DOC where p.AUTONO == TTXNACK.AUTONO select p.SLNO).ToList();
                            int max_doc_slno = 0;
                            if (doclst.Count != 0)
                            {
                                max_doc_slno = doclst.Max();
                            }
                            if (VE.DefaultAction == "A")
                            {
                                TTXNACK.EMD_NO = 0;
                            }
                            else
                            {
                                var MAXEMDNO = (from p in DB.T_TXNACK where p.AUTONO == TTXNACK.AUTONO select p.EMD_NO).Max();
                                if (MAXEMDNO == null) TTXNACK.EMD_NO = 0;
                                else TTXNACK.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                                byte[] slno = (from p in DB.T_CNTRL_HDR_DOC where p.AUTONO == TTXNACK.AUTONO && p.DOC_CTG == VE.ACK_DOCCTG select p.SLNO).ToArray();
                                foreach (var v in slno)
                                {
                                    DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == TTXNACK.AUTONO && x.SLNO == v).ToList().ForEach(x => { x.DTAG = "E"; });
                                    DB.SaveChanges();
                                    DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == TTXNACK.AUTONO && x.SLNO == v));

                                    DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == TTXNACK.AUTONO && x.SLNO == v).ToList().ForEach(x => { x.DTAG = "E"; });
                                    DB.SaveChanges();
                                    DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == TTXNACK.AUTONO && x.SLNO == v));

                                }

                            }
                            if (VE.DefaultAction == "A")
                            {
                                DB.T_TXNACK.Add(TTXNACK);
                            }
                            else if (VE.DefaultAction == "E")
                            {
                                DB.Entry(TTXNACK).State = System.Data.Entity.EntityState.Modified;
                            }
                            if (VE.UploadDOC != null)// add
                            {
                                var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TTXNACK.AUTONO, TTXNACK.EMD_NO.Value, max_doc_slno);
                                if (img.Item1.Count != 0)
                                {
                                    DB.T_CNTRL_HDR_DOC.AddRange(img.Item1);
                                    DB.T_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                                }
                            }
                            DB.SaveChanges();
                            ModelState.Clear();
                            transaction.Commit();
                            string ContentFlg = "";
                            if (VE.DefaultAction == "A")
                            {
                                ContentFlg = "1 " + "<br/>Ref Number : " + TTXNACK.REFNO + "~" + TTXNACK.AUTONO + Cn.GCS() + TTXNACK.FLAG1;
                            }
                            else if (VE.DefaultAction == "E")
                            {
                                ContentFlg = "2";
                            }
                            return Content(ContentFlg);
                        }
                        else if (VE.DefaultAction == "V")
                        {
                            DB.T_TXNACK.Where(x => x.AUTONO == VE.BLAUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                            byte[] slno = (from p in DB.T_CNTRL_HDR_DOC where p.AUTONO == VE.BLAUTONO && p.DOC_CTG == VE.ACK_DOCCTG select p.SLNO).ToArray();
                            foreach (var v in slno)
                            {
                                DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.BLAUTONO && x.SLNO == v).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.BLAUTONO && x.SLNO == v));
                                DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.BLAUTONO && x.SLNO == v).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.SaveChanges();
                                DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.BLAUTONO && x.SLNO == v));
                            }
                            DB.T_TXNACK.RemoveRange(DB.T_TXNACK.Where(x => x.AUTONO == VE.BLAUTONO));
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
        public List<Models.UploadDOC> GetUploadImageTransaction_ACK(string schema, string AutoNO, string docctg = "")
        {
            Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            var doctP = (from i in DB1.MS_DOCCTG
                         where i.DOC_CTG == docctg
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(Cn.GetConnectionString(), schema);

            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
            var doc = (from s in DB.T_CNTRL_HDR_DOC where s.AUTONO == AutoNO && s.DOC_CTG == docctg select s).ToList();
            foreach (var i in doc)
            {
                UploadDOC UPL = new UploadDOC();
                UPL.DocumentType = doctP;
                UPL.docID = i.DOC_CTG;
                UPL.DOC_FILE_NAME = i.DOC_FLNAME;
                UPL.DOC_DESC = i.DOC_DESC;
                var image = (from h in DB.T_CNTRL_HDR_DOC_DTL
                             where h.AUTONO == i.AUTONO && h.SLNO == i.SLNO
                             select h).OrderBy(d => d.RSLNO).ToList();

                var hh = image.GroupBy(x => x.AUTONO).Select(x => new
                {
                    namefl = string.Join("", x.Select(n => n.DOC_STRING))
                });
                foreach (var ii in hh)
                {
                    UPL.DOC_FILE = ii.namefl;
                }
                UploadDOC1.Add(UPL);
            }

            return UploadDOC1;
        }

        public ActionResult cancelRecords(TTxnAckEntry VE, string par1)
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
                        TCH = Cn.T_CONTROL_HDR(VE.T_TXNACK.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_TXNACK.AUTONO, CommVar.CurSchema(UNQSNO).ToString(), par1);
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
        public ActionResult ParkRecord(FormCollection FC, TTxnAckEntry stream, string MNUDET, string UNQSNO)
        {
            try
            {
                if (FC["doctype"] != null)
                {
                    stream.T_CNTRL_HDR.DOCCD = FC["doctype"].ToString();
                }
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
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TTxnAckEntry");
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