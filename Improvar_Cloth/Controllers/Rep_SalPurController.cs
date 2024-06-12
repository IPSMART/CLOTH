using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class Rep_SalPurController : Controller
    {
        public static string[,] headerArray;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Sales_func = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_SalPur
        public ActionResult Rep_SalPur()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Sale Purchase Report";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    //string selgrp = MasterHelp.GetUserITGrpCd().Replace("','", ",");
                    //string[] selgrpcd = selgrp.Split(',');

                    VE.DropDown_list = (from i in DB.M_GROUP  select new DropDown_list() { value = i.ITGRPCD, text = i.ITGRPNM }).OrderBy(s => s.text).ToList();

                    VE.DefaultView = true;
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        [HttpPost]
        public ActionResult Rep_SalPur(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string fdt = FC["FDT"].retDateStr(), tdt = FC["TDT"].retDateStr();
                string itgrpcd = "'" + VE.TEXTBOX1 + "'";
                string reptype = FC["reptype"].ToString();
                string sql = "";
                string txntag = "'SB','SR'";
                DataTable rsTbl = new DataTable();
                int maxR = 0, i = 0, rNo = 0;

                if (VE.Checkbox1 == true)
                {
                    #region fifo
                     rsTbl= Sales_func.GenStocktblwithVal("FIFO", tdt, "", "", itgrpcd, "", "", true, "", false, "", "", "", "", false);
                    //sql = "select a.autono, a.doctag, a.docno, a.docdt, a.slcd, a.slnm, a.slno, a.itcd, a.itnm, a.uomnm, a.batchautono, a.batchno, " + Environment.NewLine; ;
                    //sql += "a.qnty, a.txblval, (a.rate*a.qnty) - ((a.discamt/a.iqnty)*a.qnty) btxblval,  nvl(b.othamt,0) othamt, " + Environment.NewLine; ;
                    //sql += "a.pblno, a.pdocdt, a.prate,a.stkdrcr,a.comploccd  from " + Environment.NewLine; ;

                    //sql += "( select b.autono,b.stkdrcr, c.doctag, nvl(c.pblno,d.docno) docno, d.docdt, c.slcd, e.slnm, b.slno, b.itcd, f.itnm, h.uomnm, ''batchautono, ''batchno, " + Environment.NewLine; ;
                    //sql += "b.qnty, b.qnty iqnty, b.rate, nvl(b.discamt,0)+nvl(b.stddiscamt,0) discamt, " + Environment.NewLine; ;
                    //sql += "b.basamt-nvl(b.discamt,0)-nvl(b.stddiscamt,0) txblval,c.pblno, c.pbldt pdocdt, b.rate prate,d.compcd||d.loccd comploccd " + Environment.NewLine; ;
                    //sql += "from " + scm1 + ".t_txndtl b, " + scm1 + ".t_txn c, " + scm1 + ".t_cntrl_hdr d, " + Environment.NewLine; ;
                    //sql += scmf + ".m_subleg e, " + scm1 + ".m_sitem f, " + scmf + ".m_uom h, " + Environment.NewLine; ;
                    //sql += scm1 + ".m_group k " + Environment.NewLine; ;
                    //sql += "where b.autono=c.autono and b.autono=d.autono and " + Environment.NewLine; ;
                    //sql += "f.uomcd=h.uomcd(+) and c.itgrpcd=k.itgrpcd(+) and " + Environment.NewLine; ;
                    //sql += "d.compcd='" + COM + "' and d.loccd='" + LOC + "' and nvl(d.cancel,'N')='N' and c.doctag in ('OP','PB','SR','SI') and " + Environment.NewLine; ;
                    //sql += "d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and  " + Environment.NewLine; ;
                    //sql += "c.slcd=e.slcd and b.itcd=f.itcd(+) and c.itgrpcd in (" + itgrpcd + ") ) a, " + Environment.NewLine; ;

                    //sql += "( select a.autono, sum(case c.addless when 'A' then a.amtrate when 'L' then a.amtrate*-1 end) othamt " + Environment.NewLine; ;
                    //sql += "from " + scm1 + ".t_txnamt a, " + scm1 + ".t_cntrl_hdr b, " + scm1 + ".m_amttype c " + Environment.NewLine; ;
                    //sql += "where a.autono=b.autono and a.amtcd=c.amtcd and b.compcd='" + COM + "' and b.loccd='" + LOC + "' " + Environment.NewLine; ;
                    //sql += "group by a.autono ) b " + Environment.NewLine; ;

                    //sql += "where a.autono=b.autono(+) and a.stkdrcr='D' " + Environment.NewLine;
                    ////sql += "and itcd='M002000003' ";
                    //sql += "order by comploccd, itcd, docdt,docno " + Environment.NewLine;
                    //DataTable Rs_In = MasterHelp.SQLquery(sql);

                    //sql = "select a.autono, a.doctag, a.docno, a.docdt, a.slcd, a.slnm, a.slno, a.itcd, a.itnm, a.uomnm, a.batchautono, a.batchno, " + Environment.NewLine; ;
                    //sql += "decode(a.doctag,'SR',-1,1)*a.qnty qnty, a.rate, a.txblval, (a.rate*a.qnty) - ((a.discamt/a.iqnty)*a.qnty) btxblval,  nvl(b.othamt,0) othamt, " + Environment.NewLine; ;
                    //sql += "a.pblno, a.pdocdt, a.srate, a.stkdrcr,a.comploccd  from " + Environment.NewLine; ;

                    //sql += "( select b.autono,b.stkdrcr, c.doctag, nvl(c.pblno,d.docno) docno, d.docdt, c.slcd, e.slnm, b.slno, b.itcd, f.itnm, h.uomnm, ''batchautono, ''batchno, " + Environment.NewLine; ;
                    //sql += "b.qnty, b.qnty iqnty, b.rate, nvl(b.discamt,0)+nvl(b.stddiscamt,0) discamt, " + Environment.NewLine; ;
                    //sql += "b.basamt-nvl(b.discamt,0)-nvl(b.stddiscamt,0) txblval,c.pblno, c.pbldt pdocdt, b.rate srate, d.compcd||d.loccd comploccd " + Environment.NewLine; ;
                    //sql += "from " + scm1 + ".t_txndtl b, " + scm1 + ".t_txn c, " + scm1 + ".t_cntrl_hdr d, " + Environment.NewLine; ;
                    //sql += scmf + ".m_subleg e, " + scm1 + ".m_sitem f, " + scmf + ".m_uom h, " + Environment.NewLine; ;
                    //sql += scm1 + ".m_group k " + Environment.NewLine; ;
                    //sql += "where b.autono=c.autono and b.autono=d.autono and " + Environment.NewLine; ;
                    //sql += "f.uomcd=h.uomcd(+) and c.itgrpcd=k.itgrpcd(+) and " + Environment.NewLine; ;
                    //sql += "d.compcd='" + COM + "' and d.loccd='" + LOC + "' and nvl(d.cancel,'N')='N' and " + Environment.NewLine; ;
                    //sql += "c.doctag in (" + txntag + ") and  ";
                    //sql += "d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and  " + Environment.NewLine; ;
                    //sql += "c.slcd=e.slcd and b.itcd=f.itcd(+) and c.itgrpcd in (" + itgrpcd + ") ) a, " + Environment.NewLine; ;

                    //sql += "( select a.autono, sum(case c.addless when 'A' then a.amtrate when 'L' then a.amtrate*-1 end) othamt " + Environment.NewLine; ;
                    //sql += "from " + scm1 + ".t_txnamt a, " + scm1 + ".t_cntrl_hdr b, " + scm1 + ".m_amttype c " + Environment.NewLine; ;
                    //sql += "where a.autono=b.autono and a.amtcd=c.amtcd and b.compcd='" + COM + "' and b.loccd='" + LOC + "' " + Environment.NewLine; ;
                    //sql += "group by a.autono ) b " + Environment.NewLine; ;

                    //sql += "where a.autono=b.autono(+) " + Environment.NewLine; //and a.stkdrcr='C' " + Environment.NewLine;
                    ////sql += "and itcd='M002000003' ";
                    //sql += "order by comploccd, itcd, docdt, docno " + Environment.NewLine;
                    //DataTable Rs_Out = MasterHelp.SQLquery(sql);


                    //DataTable fixRs = new DataTable("stock");
                    //fixRs.Columns.Add("AUTONO", typeof(string), "");
                    //fixRs.Columns.Add("DOCTAG", typeof(string), "");
                    //fixRs.Columns.Add("DOCNO", typeof(string), "");
                    //fixRs.Columns.Add("DOCDT", typeof(string), "");
                    //fixRs.Columns.Add("PBLNO", typeof(string), "");
                    //fixRs.Columns.Add("PDOCDT", typeof(string), "");
                    //fixRs.Columns.Add("SLCD", typeof(string), "");
                    //fixRs.Columns.Add("SLNM", typeof(string), "");
                    //fixRs.Columns.Add("ITCD", typeof(string), "");
                    //fixRs.Columns.Add("ITNM", typeof(string), "");
                    //fixRs.Columns.Add("UOMNM", typeof(string), "");
                    //fixRs.Columns.Add("QNTY", typeof(double), "");
                    //fixRs.Columns.Add("TXBLVAL", typeof(double), "");
                    //fixRs.Columns.Add("BTXBLVAL", typeof(double), "");
                    //fixRs.Columns.Add("OTHAMT", typeof(double), "");
                    //fixRs.Columns.Add("PRATE", typeof(double), "");
                    //fixRs.Columns.Add("STKDRCR", typeof(string), "");
                    //fixRs.Columns.Add("BALQNTY", typeof(double), "");
                    //fixRs.Columns.Add("BALAMT", typeof(double), "");
                    //fixRs.Columns.Add("comploccd", typeof(string), "");
                    //fixRs.Columns.Add("OUTQNTY", typeof(double), "");
                    //fixRs.Columns.Add("OUTAUTONO", typeof(string), "");
                    //fixRs.Columns.Add("OUTDT", typeof(string), "");
                    //fixRs.Columns.Add("OUTNO", typeof(string), "");
                    //fixRs.Columns.Add("OUTSL_CD", typeof(string), "");
                    //fixRs.Columns.Add("OUTSL_NM", typeof(string), "");
                    //fixRs.Columns.Add("OUTRATE", typeof(double), "");
                    //fixRs.Columns.Add("OUTAMT", typeof(double), "");
                    //fixRs.Columns.Add("FALSEREC", typeof(string), "");
                    //fixRs.Columns.Add("RECNO", typeof(string), "");

                    //string comploccd, itcd, itnm, uomnm, autono, Docno, Pblno, stkdrcr, Pdocdt, slcd, slnm, RecNo;
                    //int lnNo;
                    //Double SalQty, BalQnty, AvRate = 0, SAvRate = 0, crSalAmt, BalAmt, xx, prate;
                    //DateTime Docdt;

                    //lnNo = 0;
                    //maxR = Rs_In.Rows.Count - 1; i = 0; rNo = 0;
                    //while (i <= maxR)
                    //{
                    //    comploccd = Rs_In.Rows[i]["comploccd"].retStr();
                    //    autono = Rs_In.Rows[i]["autono"].retStr();
                    //    while (Rs_In.Rows[i]["comploccd"].retStr() == comploccd)
                    //    {
                    //        itcd = Rs_In.Rows[i]["ITCD"].retStr();
                    //        itnm = Rs_In.Rows[i]["ITNM"].retStr();
                    //        uomnm = Rs_In.Rows[i]["UOMNM"].retStr();
                    //        int fixstartno = -1;
                    //        while (Rs_In.Rows[i]["comploccd"].retStr() == comploccd && Rs_In.Rows[i]["ITCD"].retStr() == itcd)
                    //        {
                    //            lnNo = lnNo + 1;
                    //            fixRs.Rows.Add(""); rNo = fixRs.Rows.Count - 1;
                    //            if (fixstartno == -1) fixstartno = rNo;
                    //            fixRs.Rows[rNo]["comploccd"] = Rs_In.Rows[i]["comploccd"].retStr();
                    //            fixRs.Rows[rNo]["ITCD"] = Rs_In.Rows[i]["ITCD"].retStr();
                    //            fixRs.Rows[rNo]["ITNM"] = Rs_In.Rows[i]["ITNM"].retStr();
                    //            fixRs.Rows[rNo]["UOMNM"] = Rs_In.Rows[i]["UOMNM"].retStr();
                    //            fixRs.Rows[rNo]["autoNO"] = Rs_In.Rows[i]["autoNO"].retStr();
                    //            fixRs.Rows[rNo]["DOCDT"] = Rs_In.Rows[i]["DOCDT"].retStr().Remove(10);
                    //            fixRs.Rows[rNo]["DOCNO"] = Rs_In.Rows[i]["DOCNO"].retStr();
                    //            fixRs.Rows[rNo]["PBLNO"] = Rs_In.Rows[i]["PBLNO"].retStr();
                    //            fixRs.Rows[rNo]["PDOCDT"] = Rs_In.Rows[i]["PDOCDT"].retStr() == "" ? "" : Rs_In.Rows[i]["PDOCDT"].retStr().Remove(10);
                    //            fixRs.Rows[rNo]["STKDRCR"] = Rs_In.Rows[i]["STKDRCR"].retStr();
                    //            fixRs.Rows[rNo]["SLCD"] = Rs_In.Rows[i]["SLCD"].retStr();
                    //            fixRs.Rows[rNo]["SLNM"] = Rs_In.Rows[i]["SLNM"].retStr();
                    //            fixRs.Rows[rNo]["QNTY"] = Rs_In.Rows[i]["QNTY"].retDbl();
                    //            fixRs.Rows[rNo]["PRATE"] = Rs_In.Rows[i]["PRATE"].retDbl();
                    //            fixRs.Rows[rNo]["BTXBLVAL"] = Rs_In.Rows[i]["BTXBLVAL"].retDbl();
                    //            fixRs.Rows[rNo]["BALQNTY"] = fixRs.Rows[rNo]["QNTY"].retDbl();
                    //            fixRs.Rows[rNo]["BALAMT"] = fixRs.Rows[rNo]["BTXBLVAL"].retDbl();
                    //            fixRs.Rows[rNo]["RECno"] = "PR" + lnNo.ToString().PadLeft(6, '0');
                    //            i = i + 1;
                    //            if (i > maxR) break;
                    //        }
                    //        if (Rs_Out.Rows.Count > 0)
                    //        {
                    //            rNo = fixstartno;
                    //            string sel1 = "comploccd='" + comploccd + "' and itcd='" + itcd + "'";
                    //            var rm1 = Rs_Out.Select(sel1);
                    //            int j = 0;
                    //            while (j <= rm1.Count() - 1)
                    //            {
                    //                SalQty = rm1[j]["QNTY"].retDbl();
                    //                crSalAmt = rm1[j]["BTXBLVAL"].retDbl();
                    //                SAvRate = (crSalAmt / SalQty).toRound(4);

                    //                DataView dv = fixRs.DefaultView;
                    //                dv.Sort = "comploccd,recno,itcd";
                    //                fixRs = dv.ToTable();
                    //                sel1 = "comploccd='" + comploccd + "' and itcd='" + itcd + "'";
                    //                var rm2 = fixRs.Select(sel1);
                    //                while (SalQty > 0 && rm2.Count() > 0)
                    //                {
                    //                    int k = 0;
                    //                    while (k <= rm2.Count() - 1)
                    //                    {
                    //                        if (rm2[k]["BALQNTY"].retDbl() != 0)
                    //                        {
                    //                            if (rm2[k]["BALQNTY"].retDbl() >= SalQty || rm2[k]["OUTQNTY"].retDbl() != 0)
                    //                            {
                    //                                if (rm2[k]["OUTQNTY"].retDbl() != 0)
                    //                                {
                    //                                    RecNo = rm2[k]["RECno"].retStr();
                    //                                    BalQnty = rm2[k]["BALQNTY"].retDbl();
                    //                                    BalAmt = rm2[k]["BALAMT"].retDbl();
                    //                                    Docdt = Convert.ToDateTime(rm2[k]["DOCDT"].retStr());
                    //                                    Docno = rm2[k]["DOCNO"].retStr();
                    //                                    Pblno = rm2[k]["PBLNO"].retStr();
                    //                                    Pdocdt = rm2[k]["PDOCDT"].retStr() == "" ? "" : rm2[k]["PDOCDT"].retStr();
                    //                                    stkdrcr = rm2[k]["STKDRCR"].retStr();
                    //                                    slcd = rm2[k]["slcd"].retStr();
                    //                                    slnm = rm2[k]["slnm"].retStr();
                    //                                    prate = rm2[k]["prate"].retDbl();
                    //                                    rm2[k]["BALQNTY"] = 0;
                    //                                    rm2[k]["BALAMT"] = 0;

                    //                                    fixRs.Rows.Add(""); rNo = fixRs.Rows.Count - 1;
                    //                                    fixRs.Rows[rNo]["comploccd"] = comploccd;
                    //                                    fixRs.Rows[rNo]["ITCD"] = itcd;
                    //                                    fixRs.Rows[rNo]["ITNM"] = itnm;
                    //                                    fixRs.Rows[rNo]["UOMNM"] = uomnm;
                    //                                    fixRs.Rows[rNo]["DOCDT"] = Docdt.retDateStr();
                    //                                    fixRs.Rows[rNo]["DOCNO"] = Docno;
                    //                                    fixRs.Rows[rNo]["PBLNO"] = Pblno;
                    //                                    fixRs.Rows[rNo]["PDOCDT"] = Pdocdt.retStr();
                    //                                    fixRs.Rows[rNo]["STKDRCR"] = stkdrcr;
                    //                                    fixRs.Rows[rNo]["slcd"] = slcd;
                    //                                    fixRs.Rows[rNo]["slnm"] = slnm;
                    //                                    fixRs.Rows[rNo]["prate"] = prate;
                    //                                    fixRs.Rows[rNo]["RECno"] = RecNo;
                    //                                    fixRs.Rows[rNo]["FALSEREC"] = "Y";
                    //                                    fixRs.Rows[rNo]["BALQNTY"] = BalQnty;
                    //                                    fixRs.Rows[rNo]["BALAMT"] = BalAmt;

                    //                                    dv = fixRs.DefaultView;
                    //                                    dv.Sort = "comploccd,recno,itcd";
                    //                                    fixRs = dv.ToTable();
                    //                                    sel1 = "comploccd='" + comploccd + "' and itcd='" + itcd + "'";
                    //                                    rm2 = fixRs.Select(sel1);

                    //                                    k++;
                    //                                }
                    //                                if (rm2[k]["BALQNTY"].retDbl() >= SalQty)
                    //                                {
                    //                                    if (rm2[k]["BALQNTY"].retDbl() == 0)
                    //                                    {
                    //                                        AvRate = 0;
                    //                                    }
                    //                                    else
                    //                                    {
                    //                                        AvRate = (rm2[k]["BALAMT"].retDbl() / rm2[k]["BALQNTY"].retDbl()).toRound(4);
                    //                                    }

                    //                                    rm2[k]["BALQNTY"] = rm2[k]["BALQNTY"].retDbl() - SalQty;
                    //                                    rm2[k]["BALAMT"] = (rm2[k]["BALQNTY"].retDbl() * AvRate).toRound(2);
                    //                                    rm2[k]["OUTDT"] = rm1[j]["DOCDT"].retDateStr();
                    //                                    rm2[k]["OUTNO"] = rm1[j]["DOCNO"].retStr();
                    //                                    rm2[k]["OUTAUTONO"] = rm1[j]["autoNO"].retStr();
                    //                                    rm2[k]["OUTSL_CD"] = rm1[j]["SLCD"].retStr();
                    //                                    rm2[k]["OUTSL_NM"] = rm1[j]["SLNM"].retStr();
                    //                                    rm2[k]["OUTSL_NM"] = rm1[j]["SLNM"].retStr();
                    //                                    rm2[k]["OUTRATE"] = rm1[j]["Rate"].retDbl();
                    //                                    rm2[k]["OUTQNTY"] = SalQty;
                    //                                    rm2[k]["OUTAMT"] = crSalAmt;

                    //                                    SalQty = 0;
                    //                                    crSalAmt = 0;
                    //                                    break;
                    //                                }
                    //                                else
                    //                                {
                    //                                    if (rm2[k]["BALQNTY"].retDbl() == 0)
                    //                                    {
                    //                                        AvRate = 0;
                    //                                    }
                    //                                    else
                    //                                    {
                    //                                        AvRate = (rm2[k]["BALAMT"].retDbl() / rm2[k]["BALQNTY"].retDbl()).toRound(4);
                    //                                    }
                    //                                    rm2[k]["BALAMT"] = 0;

                    //                                    rm2[k]["OUTDT"] = rm1[j]["DOCDT"].retStr().retDateStr();
                    //                                    rm2[k]["OUTNO"] = rm1[j]["DOCNO"].retStr();
                    //                                    rm2[k]["OUTAUTONO"] = rm1[j]["autoNO"].retStr();
                    //                                    rm2[k]["OUTSL_CD"] = rm1[j]["SLCD"].retStr();
                    //                                    rm2[k]["OUTSL_NM"] = rm1[j]["SLNM"].retStr();// Left(FIlc(rm1[j]["SLNM), 8)
                    //                                    rm2[k]["OUTQNTY"] = rm2[k]["BALQNTY"].retDbl();
                    //                                    rm2[k]["OUTRATE"] = rm1[j]["Rate"].retDbl();
                    //                                    rm2[k]["OUTAMT"] = crSalAmt;
                    //                                    xx = (crSalAmt / SalQty).toRound(4);
                    //                                    xx = (xx * rm2[k]["BALQNTY"].retDbl()).toRound(2);
                    //                                    crSalAmt = crSalAmt - xx;
                    //                                    SalQty = SalQty - rm2[k]["BALQNTY"].retDbl();
                    //                                    rm2[k]["OUTAMT"] = xx;
                    //                                    rm2[k]["BALQNTY"] = 0;
                    //                                }
                    //                            }
                    //                            else
                    //                            {
                    //                                if (rm2[k]["BALQNTY"].retDbl() == 0)
                    //                                {
                    //                                    AvRate = 0;
                    //                                }
                    //                                else
                    //                                {
                    //                                    AvRate = (rm2[k]["BALAMT"].retDbl() / rm2[k]["BALQNTY"].retDbl()).toRound(4);
                    //                                }

                    //                                rm2[k]["BALAMT"] = 0;
                    //                                rm2[k]["OUTDT"] = rm1[j]["DOCDT"].retStr().Remove(10);
                    //                                rm2[k]["OUTNO"] = rm1[j]["DOCNO"];
                    //                                rm2[k]["OUTAUTONO"] = rm1[j]["autoNO"];
                    //                                rm2[k]["OUTSL_CD"] = rm1[j]["SLCD"];
                    //                                rm2[k]["OUTSL_NM"] = rm1[j]["SLNM"];
                    //                                rm2[k]["OUTQNTY"] = rm2[k]["BALQNTY"].retDbl();
                    //                                rm2[k]["OUTRATE"] = rm1[j]["Rate"].retDbl();
                    //                                xx = (crSalAmt / SalQty).toRound(4);
                    //                                xx = (xx * rm2[k]["BALQNTY"].retDbl()).toRound(2);
                    //                                crSalAmt = crSalAmt - xx;
                    //                                SalQty = SalQty - rm2[k]["BALQNTY"].retDbl();
                    //                                rm2[k]["OUTAMT"] = xx;
                    //                                rm2[k]["BALQNTY"] = 0;
                    //                            }
                    //                        }
                    //                        k = k + 1;
                    //                        if (k > rm2.Count() - 1) break;
                    //                        if (rm2[k]["comploccd"].retStr() + rm2[k]["itcd"].retStr() != comploccd + itcd) break;
                    //                    }
                    //                    if (k > rm2.Count() - 1) break;
                    //                    if (rm2[k]["comploccd"].retStr() + rm2[k]["itcd"].retStr() != comploccd + itcd) break;
                    //                }
                    //                j = j + 1;
                    //                if (j > rm1.Count() - 1) break;
                    //                if (rm1[j]["comploccd"].retStr() + rm1[j]["itcd"].retStr() != comploccd + itcd) break;
                    //            }
                    //        }
                    //        if (i > maxR) break;
                    //    }
                    //}
                    ////MasterHelp.DataTbltoExcel("aa", "bb", "", fixRs);

                    //var rsList = (from DataRow dr in fixRs.Rows
                    //              where dr["OUTDT"].retStr() != "" &&
                    //              Convert.ToDateTime(dr["OUTDT"]) >= Convert.ToDateTime(fdt) && Convert.ToDateTime(dr["OUTDT"]) <= Convert.ToDateTime(tdt)
                    //              orderby dr["OUTDT"], dr["OUTNO"]
                    //              group dr by new
                    //              {
                    //                  docno = dr["OUTNO"],
                    //                  docdt = Convert.ToDateTime(dr["OUTDT"]),
                    //                  slcd = dr["OUTSL_CD"],
                    //                  slnm = dr["OUTSL_NM"],
                    //                  itcd = dr["ITCD"],
                    //                  itnm = dr["ITNM"],
                    //                  UOMNM = dr["UOMNM"],
                    //                  //pblno = dr["PBLNO"],
                    //                  doctag = dr["doctag"],
                    //                  //prate = dr["PRATE"],
                    //                  autono = dr["OUTAUTONO"],
                    //              } into X
                    //              select new
                    //              {
                    //                  docno = X.Key.docno,
                    //                  docdt = X.Key.docdt,
                    //                  slcd = X.Key.slcd,
                    //                  slnm = X.Key.slnm,
                    //                  itcd = X.Key.itcd,
                    //                  itnm = X.Key.itnm,
                    //                  UOMNM = X.Key.UOMNM,
                    //                  qnty = X.Sum(Z => Z.Field<double>("OUTQNTY")),
                    //                  //btxblval = X.Sum(Z => (Z.Field<double>("OUTQNTY") * Z.Field<double>("OUTRATE")).toRound(2)),
                    //                  btxblval = X.Sum(Z => Z.Field<double>("OUTAMT")),
                    //                  //prate = (X.Sum(Z => Z.Field<double>("OUTAMT")) / X.Sum(Z => Z.Field<double>("OUTQNTY"))).toRound(4),
                    //                  prate = X.Sum(Z => (Z.Field<double>("OUTQNTY") * Z.Field<double>("PRATE")).toRound(2)) / X.Sum(Z => Z.Field<double>("OUTQNTY")),
                    //                  //pblno = X.Key.pblno,
                    //                  pblno = string.Join(",", X.Where(a => a.Field<string>("pblno") != "").Select(a => a.Field<string>("pblno")).Distinct()),
                    //                  doctag = X.Key.doctag,
                    //              }).OrderBy(A => A.docdt).ThenBy(a => a.docno).ToList();


                    //rsTbl = ListToDatatable.LINQResultToDataTable(rsList);
                    //DataView dv1 = rsTbl.DefaultView;
                    //dv1.Sort = "docdt, docno";
                    //rsTbl = dv1.ToTable();

                    #endregion
                }

                else {
                    #region normal



                    sql += "select a.autono, a.doctag, a.docno, a.docdt, a.slcd, a.slnm, a.slno, a.itcd, a.itnm, a.uomnm, a.batchautono, a.batchno, " + Environment.NewLine; ;
                    sql += "a.qnty, a.txblval, ROUND(((a.rate*a.qnty) + ((a.discamt/a.iqnty)*a.qnty)),2) btxblval,  nvl(b.othamt,0) othamt, " + Environment.NewLine; ;
                    sql += "a.pblno, a.pdocdt, a.prate  from " + Environment.NewLine; ;

                    sql += "( select a.autono, c.doctag, nvl(c.prefno,d.docno) docno, d.docdt, c.slcd, e.slnm, b.slno, b.itcd, f.itnm, h.uomnm, ''batchautono, i.batchno, " + Environment.NewLine; ;
                    //sql += "decode(k.rateqntybag,'B',nvl(a.nos,b.nos),nvl(a.qnty,b.qnty)) qnty, decode(k.rateqntybag,'B',nvl(a.nos,b.nos),nvl(a.qnty,b.qnty)) iqnty, b.rate, nvl(b.discamt,0)-nvl(b.stddiscamt,0) discamt, " + Environment.NewLine; ;
                    sql += "nvl(a.qnty,b.qnty) qnty, nvl(b.qnty,a.qnty) iqnty, b.rate, nvl(b.discamt,0)-nvl(b.SCMDISCAMT,0) discamt, " + Environment.NewLine; ;
                    sql += "b.amt-nvl(b.discamt,0)-nvl(b.SCMDISCAMT,0) txblval, nvl(j.prefno,l.docno) pblno, l.docdt pdocdt, i.rate+nvl(i.othrate,0) prate " + Environment.NewLine; ;
                    sql += "from " + scm1 + ".t_batchdtl a, " + scm1 + ".t_txndtl b, " + scm1 + ".t_txn c, " + scm1 + ".t_cntrl_hdr d, " + Environment.NewLine; ;
                    sql += scmf + ".m_subleg e, " + scm1 + ".m_sitem f, " + scmf + ".m_uom h, " + Environment.NewLine; ;
                    sql += scm1 + ".t_batchmst i, " + scm1 + ".t_txn j, " + scm1 + ".m_group k, " + scm1 + ".t_cntrl_hdr l " + Environment.NewLine; ;
                    sql += "where b.autono=a.autono(+) and b.slno=a.txnslno(+) and b.autono=c.autono and b.autono=d.autono and " + Environment.NewLine; ;
                    sql += "f.uomcd=h.uomcd(+) and  a.barno=i.barno(+) and i.autono=j.autono(+) and i.autono=l.autono(+) and f.itgrpcd=k.itgrpcd(+) and " + Environment.NewLine; ;
                    sql += "d.compcd='" + COM + "' and d.loccd='" + LOC + "' and nvl(d.cancel,'N')='N' and " + Environment.NewLine; ;
                    sql += "c.doctag in (" + txntag + ") and  ";
                    sql += "d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and  " + Environment.NewLine; ;
                    sql += "c.slcd=e.slcd and b.itcd=f.itcd(+) and f.itgrpcd in (" + itgrpcd + ") ) a, " + Environment.NewLine; ;

                    sql += "( select a.autono, sum(case c.addless when 'A' then a.amtrate when 'L' then a.amtrate*-1 end) othamt " + Environment.NewLine; ;
                    sql += "from " + scm1 + ".t_txnamt a, " + scm1 + ".t_cntrl_hdr b, " + scm1 + ".m_amttype c " + Environment.NewLine; ;
                    sql += "where a.autono=b.autono and a.amtcd=c.amtcd and b.compcd='" + COM + "' and b.loccd='" + LOC + "' " + Environment.NewLine; ;
                    sql += "group by a.autono ) b " + Environment.NewLine; ;

                    sql += "where a.autono=b.autono(+) " + Environment.NewLine; ;
                    sql += "order by docdt, docno " + Environment.NewLine; ;
                    rsTbl = MasterHelp.SQLquery(sql);
                    #endregion
                }




                if (rsTbl.Rows.Count == 0)
                {
                    return Content("no records..");
                }

                DataTable IR = new DataTable("mstrep");
                PrintViewer PV = new PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
                HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Doc Date");
                HC.GetPrintHeader(IR, "slcd", "string", "c,10", "Code");
                HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name");
                HC.GetPrintHeader(IR, "itcd", "string", "c,10", "Item Cd");
                HC.GetPrintHeader(IR, "itnm", "string", "c,35", "Item Name");
                HC.GetPrintHeader(IR, "uom", "string", "c,4", "uom");
                HC.GetPrintHeader(IR, "qnty", "double", "n,12,4", "Qnty");
                HC.GetPrintHeader(IR, "srate", "double", "n,12,4", "S.Rate");
                HC.GetPrintHeader(IR, "samt", "double", "n,12,2", "Gross Amt");
                HC.GetPrintHeader(IR, "prate", "double", "n,12,4", "P.Rate");
                HC.GetPrintHeader(IR, "pamt", "double", "n,12,2", "Cost Amt");
                HC.GetPrintHeader(IR, "diffamt", "double", "n,16,2:###,##,##,##0.00", "Profit/;Loss(-)");
                HC.GetPrintHeader(IR, "pblno", "string", "c,16", "Purch;Doc No");

                i = 0; maxR = 0; rNo = 0;
                maxR = rsTbl.Rows.Count - 1;
                double tqnty = 0, tsamt = 0, tpamt = 0, tdiffamt = 0;
                while (i <= maxR)
                {
                    double imult = 1;
                    if (rsTbl.Rows[i]["doctag"].ToString() == "SR") imult = -1;

                    double srate = Math.Round(rsTbl.Rows[i]["btxblval"].retDbl() / rsTbl.Rows[i]["qnty"].retDbl(), 6);
                    double prate = Math.Round(rsTbl.Rows[i]["prate"].retDbl(), 6);
                    double pamt = Math.Round(prate * rsTbl.Rows[i]["qnty"].retDbl(), 2);
                    double samt = 0;

                    samt = rsTbl.Rows[i]["btxblval"].retDbl() * imult;

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["docno"] = rsTbl.Rows[i]["docno"];
                    IR.Rows[rNo]["docdt"] = rsTbl.Rows[i]["docdt"].retDateStr();
                    IR.Rows[rNo]["slcd"] = rsTbl.Rows[i]["slcd"];
                    IR.Rows[rNo]["slnm"] = rsTbl.Rows[i]["slnm"];
                    IR.Rows[rNo]["itcd"] = rsTbl.Rows[i]["itcd"];
                    IR.Rows[rNo]["itnm"] = rsTbl.Rows[i]["itnm"];
                    IR.Rows[rNo]["uom"] = rsTbl.Rows[i]["uomnm"];
                    IR.Rows[rNo]["qnty"] = rsTbl.Rows[i]["qnty"].retDbl() * imult;
                    IR.Rows[rNo]["srate"] = srate;
                    IR.Rows[rNo]["samt"] = samt;
                    IR.Rows[rNo]["prate"] = prate;
                    IR.Rows[rNo]["pamt"] = pamt;
                    Double diffamt = samt - pamt;
                    if (rsTbl.Rows[i]["doctag"].ToString() == "SR") diffamt = (samt * -1) - pamt;
                    IR.Rows[rNo]["diffamt"] = diffamt;
                    IR.Rows[rNo]["pblno"] = rsTbl.Rows[i]["pblno"];
                    IR.Rows[rNo]["celldesign"] = "diffamt=font-weight:bold;font-size:13px;";

                    tqnty = tqnty + (rsTbl.Rows[i]["qnty"].retDbl() * imult);
                    tsamt = tsamt + samt;
                    tpamt = tpamt + pamt;
                    tdiffamt = tdiffamt + diffamt;

                    i++;
                }

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = "";
                IR.Rows[rNo]["slnm"] = "Grand Totals";
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                IR.Rows[rNo]["qnty"] = tqnty;
                IR.Rows[rNo]["samt"] = tsamt;
                IR.Rows[rNo]["pamt"] = tpamt;
                IR.Rows[rNo]["diffamt"] = tdiffamt;

                string pghdr1 = "Sale Purchase Statement from " + fdt + " to " + tdt;
                string rephdr = "SalPurStmt";
                PV = HC.ShowReport(IR, rephdr, pghdr1, "", true, true, "L", false);

                TempData[rephdr] = PV;
                TempData[rephdr + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = rephdr });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
    }
}
