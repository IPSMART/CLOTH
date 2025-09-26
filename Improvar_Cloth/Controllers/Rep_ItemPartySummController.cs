using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Collections;
using System.Data;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace Improvar.Controllers
{
    public class Rep_ItemPartySummController : Controller
    {
        // GET: M_Grpmas
        Connection Cn = new Connection(); string CS = null;
        MasterHelp masterHelp = new MasterHelp();
        MasterHelpFa masterHelpFa = new MasterHelpFa();
        DropDownHelp dropDownHelp = new DropDownHelp();
        Salesfunc Sales_func = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_ItemPartySumm()
        {

            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    string com = CommVar.Compcd(UNQSNO);
                    ViewBag.formname = "Design Summary";
                    PartyitemSummReport VE = new PartyitemSummReport();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                    VE.DropDown_list_ITGRP = dropDownHelp.GetItgrpcdforSelection();
                    VE.ITGRPCD = masterHelp.ComboFill("ITGRPCD", VE.DropDown_list_ITGRP, 0, 1);

                    VE.DropDown_list = (from i in DBF.M_LOCA
                                        where i.COMPCD == com
                                        select new DropDown_list() { value = i.LOCCD, text = i.LOCNM }).Distinct().OrderBy(s => s.text).ToList();// location
                    VE.LOCATION = masterHelp.ComboFill("loccd", VE.DropDown_list, 0, 1);

                    VE.DefaultView = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                PartyitemSummReport VE = new PartyitemSummReport();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                return View(VE);
            }
        }
        public ActionResult GetBarCodeDetails(string val)
        {
            try
            {
                string str = masterHelp.ITCD_help(val, "");
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1)
                    {
                        return Content(str = "");
                    }
                    else
                    {
                        DataTable bardet = Sales_func.GetBarHelp(System.DateTime.Now.Date.retDateStr(), "", "", val.retSqlformat());
                        if (bardet != null && bardet.Rows.Count > 0)
                        {
                            str += masterHelp.ToReturnFieldValues("", bardet);

                            string barno = str.retCompValue("BARNO").retStr();
                            DataTable pricedet = Sales_func.GetLastPriceFrmMaster(barno);
                            if (pricedet != null && pricedet.Rows.Count > 0)
                            {
                                double rprate = (from DataRow dr in pricedet.Rows where dr["prccd"].retStr() == "RP" select dr["rate"].retDbl()).FirstOrDefault();
                                double wprate = (from DataRow dr in pricedet.Rows where dr["prccd"].retStr() == "WP" select dr["rate"].retDbl()).FirstOrDefault();
                                str += "^WPRATE=^" + wprate + Cn.GCS();
                                str += "^RPRATE=^" + rprate + Cn.GCS();
                            }
                        }

                        var BarImages = str.retCompValue("BARIMAGE").retStr().Split((char)179);
                        string BARIMAGEPATH = "";
                        foreach (var v in BarImages)
                        {
                            if (v.retStr() != "")
                            {
                                if (BARIMAGEPATH != "")
                                {
                                    BARIMAGEPATH += (char)179;
                                }
                                var temp = v.Split('~');
                                BARIMAGEPATH += CommVar.WebUploadDocURL(temp[0]) + "~" + temp[1];
                            }
                        }
                        str += "^BARIMAGEPATH=^" + BARIMAGEPATH + Cn.GCS();
                        return Content(str);
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
                Code = "D,C";
                var str = masterHelp.SLCD_help(val, Code);
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


        public string BtnSubmit(string itcd, string itnm, string fdt, string tdt, string check, string itgrpcd, string location, string salpur)
        {
            try
            {
                string url = "";
                var PreviousUrl = System.Web.HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
                var uri = new Uri(PreviousUrl);//Create Virtually Query String
                var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                if (queryString.Get("ITCD") == null)
                {
                    url = Request.UrlReferrer.ToString() + "&ITCD=" + itcd + "&ITNM=" + itnm + "&FDT=" + fdt + "&TDT=" + tdt + "&CHECK=" + check + "&ITGRPCD=" + itgrpcd + "&LOCCD=" + location + "&SALPUR=" + salpur;
                }
                else
                {
                    string dd = Request.UrlReferrer.ToString();
                    int pos = Request.UrlReferrer.ToString().IndexOf("&ITCD=");
                    url = dd.Substring(0, pos);
                    url = url + "&ITCD=" + itcd + "&ITNM=" + itnm + "&FDT=" + fdt + "&TDT=" + tdt + "&CHECK=" + check + "&ITGRPCD=" + itgrpcd + "&LOCCD=" + location + "&SALPUR=" + salpur;
                }
                return url;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public ActionResult GetItemData(string slcd = "", string fdt = "", string tdt = "", string check = "", string itgrpcd = "", string itcd = "", string itnm = "", string LOCATION = "", string SALPUR = "")
        {
            try
            {
                PartyitemSummReport VE = new PartyitemSummReport();
                if (itgrpcd.retStr() != "") itgrpcd = itgrpcd.retSqlformat();
                if (LOCATION.retStr() != "") LOCATION = LOCATION.retSqlformat();
                if (slcd.retStr() != "") slcd = slcd.retSqlformat();
                if (itcd.retStr() != "") itcd = itcd.retSqlformat();

                DataTable dt = GetData(slcd, fdt, tdt, itgrpcd, check, itcd, LOCATION, SALPUR);
                string doctag = SALPUR == "S" ? "SB" : "PB";
                VE.ItmDet = (from DataRow DR in dt.Rows
                             group DR by new
                             {
                                 refdt = DR["docdt"].retDateStr(),
                                 refno = DR["docno"].retStr(),
                                 slnm = DR["slnm"].retStr(),
                                 Design = DR["itstyle"].retStr(),
                                 docnm = DR["docnm"].retStr(),
                                 colrnm = DR["colrnm"].retStr(),
                                 itrem = DR["itrem"].retStr(),
                                 disc = DR["scmdiscrate"].retDbl(),
                                 rate = DR["rate"].retDbl(),
                                 doctag = DR["doctag"].retStr(),
                             } into X
                             select new ItmDet()
                             {
                                 refdt = X.Key.refdt.retStr(),
                                 refno = X.Key.refno.retStr(),
                                 slnm = X.Key.slnm.retStr(),
                                 Design = X.Key.Design.retStr(),
                                 docnm = X.Key.docnm.retStr(),
                                 colrnm = X.Key.colrnm.retStr(),
                                 itrem = X.Key.itrem.retStr(),
                                 disc = X.Key.disc.retDbl(),
                                 rate = X.Key.rate.retDbl(),
                                 qnty = X.Key.doctag.retStr() == doctag ? X.Sum(Z => Z.Field<decimal>("qnty").retDbl()) : (X.Sum(Z => Z.Field<decimal>("qnty").retDbl()) * -1),
                                 amt = X.Key.doctag.retStr() == doctag ? X.Sum(Z => Z.Field<double>("amt").retDbl()) : (X.Sum(Z => Z.Field<double>("amt").retDbl()) * -1),
                             }).OrderBy(A => A.Design).OrderBy(A => A.refdt).OrderBy(A => A.refno).ToList();

                VE.T_qnty = VE.ItmDet.Sum(a => a.qnty).retDbl();
                VE.T_amt = VE.ItmDet.Sum(a => a.amt).retDbl();

                string jobrt = "";
                itnm = "";
                if (dt != null && dt.Rows.Count > 0)
                {
                    itnm = dt.Rows[0]["itstyle"].retStr();
                    jobrt = dt.Rows[0]["jobrt"].retStr();
                }
                ViewBag.ITNM = itnm;
                ViewBag.JOBRT = jobrt;

                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_Rep_ItemPartySumm_Item_Det", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

        public DataTable GetData(string SLCD = "", string FDT = "", string TDT = "", string ITGRPCD = "", string CHECK = "", string ITCD = "", string LOCATION = "", string SALPUR = "")
        {
            string prccd = "WP";
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string txntag = "'SB','SR','SD','SC'";

            if (SALPUR == "S")
            {
                if (CHECK == "Y")
                {
                    txntag = "'SB'";
                }
                else
                {
                    txntag = "'SB','SR','SD','SC'";
                }
            }
            else
            {
                if (CHECK == "Y")
                {
                    txntag = "'PB'";
                }
                else
                {
                    txntag = "'PB','PR','PD','PC'";
                }
            }
            string sql = "";
            sql += " select a.autono, a.doccd, a.docno,a.doctag, a.cancel,a.docdt,a.agslcd, " + Environment.NewLine;
            sql += "a.prefno, a.prefdt, a.slcd, a.slnm,a.slarea,a.agslnm,a.sagslnm,a.nm,a.mobile,a.gstno, a.district, " + Environment.NewLine;

            sql += "a.roamt,a.blamt,a.tcsamt, " + Environment.NewLine;

            sql += "a.slno,a.stkdrcr,a.itgrpnm, a.itcd,a.qnty,a.rate,a.amt, " + Environment.NewLine;
            sql += "a.itnm,a.itstyle, a.itrem,a.barno, a.barimagecount, a.barimage, a.hsncode,a.uomcd,a.uomnm, a.decimals,a.colrcd,a.colrnm, " + Environment.NewLine;

            sql += "(case when a.doctag = 'SB' then nvl(a.nos, 0) else 0 end)snos," + Environment.NewLine;
            sql += "(case when a.doctag = 'SR' then nvl(a.nos, 0) else 0 end)srnos," + Environment.NewLine;

            sql += "(case when a.doctag = 'SB' then nvl(a.qnty, 0) else 0 end)sqnty," + Environment.NewLine;
            sql += "(case when a.doctag = 'SR' then nvl(a.qnty, 0) else 0 end)srqnty," + Environment.NewLine;

            sql += "(case when a.doctag = 'SB' then nvl(a.rate, 0) else 0 end)srate," + Environment.NewLine;
            sql += "(case when a.doctag = 'SR' then nvl(a.rate, 0) else 0 end)srrate," + Environment.NewLine;

            sql += "(case when a.doctag = 'SB' then nvl(a.amt, 0) else 0 end)samt," + Environment.NewLine;
            sql += "(case when a.doctag = 'SR' then nvl(a.amt, 0) else 0 end)sramt," + Environment.NewLine;


            sql += " a.amt,a.scmdiscrate,a.scmdiscamt, a.tddiscamt, a.discamt,a.TXBLVAL, " + Environment.NewLine;


            sql += " a.conslcd, a.cslnm,a.cgstno, a.cdistrict, " + Environment.NewLine;
            sql += "a.trslnm,a.lrno,a.lrdt,a.GRWT,a.TRWT,a.NTWT,a.ordrefno,a.ordrefdt," + Environment.NewLine;

            sql += "a.igstper,a.igstamt,a.cgstper,a.cgstamt,a.sgstper,a.sgstamt,a.cessper,a.cessamt,a.blqnty,a.NETAMT,a.gstper,a.gstamt," + Environment.NewLine;
            //}


            sql += "a.ackno,a.ackdt,a.pageno,a.PAGESLNO,a.baleno,a.docrem,a.bltype,a.docnm,a.wprate,a.rprate,x.jobrt  " + Environment.NewLine;
            sql += "from ( " + Environment.NewLine;


            sql += " select a.autono, a.doccd, a.docno,a.doctag, a.cancel,to_char(a.docdt,'DD/MM/YYYY')docdt,a.docdt tchdocdt,h.agslcd, " + Environment.NewLine;
            sql += "  a.prefno, nvl(to_char(a.prefdt,'dd/mm/yyyy'),'')prefdt,a.prefdt prefdate, a.slcd, c.slnm,c.slarea,l.slnm agslnm,m.slnm sagslnm,nvl(i.nm,p.rtdebnm) nm,i.mobile,c.gstno, c.district, nvl(a.roamt, 0) roamt, " + Environment.NewLine;
            sql += " nvl(a.tcsamt, 0) tcsamt, a.blamt, " + Environment.NewLine;
            sql += "   b.slno,b.stkdrcr,o.itgrpnm, b.itcd, " + Environment.NewLine;
            sql += "   b.itnm,b.itstyle, b.itrem,b.barno, b.hsncode,nvl(b.bluomcd,b.uomcd)uomcd, nvl(b.bluomnm,b.uomnm)uomnm, nvl(nullif(b.bldecimals,0),b.decimals) decimals,b.colrcd,b.colrnm, b.nos, " + Environment.NewLine;
            sql += " nvl(nullif(b.blqnty,0),b.qnty)qnty, b.rate, b.amt,b.scmdiscrate,b.scmdiscamt, b.tddiscamt, b.discamt,b.TXBLVAL, g.conslcd, d.slnm cslnm, d.gstno cgstno, d.district cdistrict, " + Environment.NewLine;
            sql += " e.slnm trslnm, f.lrno,nvl(to_char(f.lrdt,'dd/mm/yyyy'),'')lrdt,f.GRWT,f.TRWT,f.NTWT, '' ordrefno, to_char(nvl('', ''), 'dd/mm/yyyy') ordrefdt, b.igstper, b.igstamt, b.cgstper, " + Environment.NewLine;
            sql += " b.cgstamt,b.sgstamt, b.cessper, b.cessamt,b.blqnty,b.NETAMT,b.sgstper,b.igstper+b.cgstper+b.sgstper gstper,b.igstamt + b.cgstamt + b.sgstamt gstamt,k.ackno,nvl(to_char(k.ackdt,'dd/mm/yyyy'),'')ackdt,b.pageno,b.PAGESLNO,b.baleno,h.docrem,h.bltype,  " + Environment.NewLine;
            sql += "row_number() over(partition by a.autono order by b.slno)rn,j.docnm, y.barimagecount, y.barimage,v.rate wprate,w.rate rprate,a.slcd||b.itcd slitcd " + Environment.NewLine;
            sql += " from ( " + Environment.NewLine;
            sql += " select a.autono,a.doctag, b.doccd, b.docno, b.cancel, " + Environment.NewLine;
            sql += "b.docdt, " + Environment.NewLine;
            sql += "a.prefno, a.prefdt, a.slcd, a.roamt, a.tcsamt, a.blamt  " + Environment.NewLine;

            sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b, " + Environment.NewLine;
            sql += " " + scmf + ".m_subleg c " + Environment.NewLine;
            sql += "  where a.autono = b.autono and a.slcd = c.slcd(+) " + Environment.NewLine;
            sql += "  and  b.compcd = '" + COM + "' " + Environment.NewLine;
            if (LOCATION.retStr() != "")
            {
                sql += " and b.loccd in (" + LOCATION + ") " + Environment.NewLine;
            }
            else
            {
                sql += " and b.loccd = '" + LOC + "'  " + Environment.NewLine;
            }

            if (FDT != "") sql += "and b.docdt >= to_date('" + FDT + "','dd/mm/yyyy')   " + Environment.NewLine;
            if (TDT != "") sql += "and b.docdt <= to_date('" + TDT + "','dd/mm/yyyy')   " + Environment.NewLine;

            sql += "and a.doctag in (" + txntag + ") " + Environment.NewLine;
            sql += " ) a,  " + Environment.NewLine;

            sql += "(select distinct a.autono,a.stkdrcr, a.slno, a.itcd, a.itrem,d.barno, " + Environment.NewLine;
            sql += " b.itnm,b.styleno||' '||b.itnm itstyle,nvl(a.hsncode,b.hsncode) hsncode, b.uomcd, c.uomnm, c.decimals,a.colrcd,g.colrnm, " + Environment.NewLine;
            sql += "  a.nos, a.qnty, a.rate, a.amt,a.scmdiscrate,a.scmdiscamt,a.tddiscamt,a.discamt,a.TXBLVAL,a.NETAMT,   " + Environment.NewLine;
            sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,a.blqnty,a.bluomcd,f.uomnm bluomnm,f.decimals bldecimals,a.pageno,a.pageslno,a.baleno  from " + scm1 + ".t_txndtl a, " + Environment.NewLine;
            sql += "" + scm1 + ".m_sitem b, " + scmf + ".m_uom c, " + scm1 + ".t_batchdtl d, " + scm1 + ".t_batchmst e, " + scmf + ".m_uom f, " + scm1 + ".m_color g " + Environment.NewLine;
            sql += " where a.itcd = b.itcd  and b.uomcd = c.uomcd and a.autono = d.autono(+) and a.slno=d.txnslno and d.barno = e.barno(+) and  a.bluomcd= f.uomcd(+) and a.colrcd=g.colrcd(+) " + Environment.NewLine;
            sql += " group by " + Environment.NewLine;
            sql += " a.autono,a.stkdrcr, a.slno, a.itcd, a.itrem,d.barno, " + Environment.NewLine;
            sql += "  b.itnm, nvl(a.hsncode,b.hsncode), b.uomcd, c.uomnm, c.decimals,a.colrcd,g.colrnm, a.nos, a.qnty, a.rate, a.amt,a.scmdiscrate,a.scmdiscamt,  " + Environment.NewLine;
            sql += " a.tddiscamt, a.discamt,a.TXBLVAL,a.NETAMT, a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,a.blqnty,a.bluomcd,f.uomnm,f.decimals,b.styleno||' '||b.itnm,a.pageno,a.PAGESLNO,a.baleno " + Environment.NewLine;
            sql += " ) b, " + scmf + ".m_subleg c, " + scmf + ".m_subleg d, " + scmf + ".m_subleg e, " + scm1 + ".t_txntrans f, " + Environment.NewLine;
            sql += "" + scm1 + ".t_txn g, " + scm1 + ".t_txnoth h ," + scm1 + ".t_txnmemo i ," + scm1 + ".m_doctype j," + scmf + ".t_txneinv k," + scmf + ".m_subleg l, " + Environment.NewLine;
            sql += "" + scmf + ".m_subleg m ," + scm1 + ".m_sitem n," + scm1 + ".M_GROUP o, " + scmf + ".M_RETDEB p, " + Environment.NewLine;

            sql += "(select a.barno, count(*) barimagecount," + Environment.NewLine;
            sql += "listagg(a.doc_flname||'~'||a.doc_desc,chr(179)) " + Environment.NewLine;
            sql += "within group (order by a.barno) as barimage from " + Environment.NewLine;
            sql += "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from " + Environment.NewLine;
            sql += "(select a.barno, a.barno imgbarno, a.slno imgslno " + Environment.NewLine;
            sql += "from " + scm1 + ".t_batch_img_hdr a " + Environment.NewLine;
            sql += "union " + Environment.NewLine;
            sql += "select a.barno, b.barno imgbarno, b.slno imgslno " + Environment.NewLine;
            sql += "from " + scm1 + ".t_batch_img_hdr_link a, " + scm1 + ".t_batch_img_hdr b " + Environment.NewLine;
            sql += "where a.mainbarno=b.barno(+) ) a, " + Environment.NewLine;
            sql += "" + scm1 + ".t_batch_img_hdr b " + Environment.NewLine;
            sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ) a " + Environment.NewLine;
            sql += "group by a.barno ) y, " + Environment.NewLine;

            for (int x = 0; x <= 1; x++)
            {
                string sqlals = "";
                switch (x)
                {
                    case 0:
                        sqlals = "v "; break;
                    case 1:
                        prccd = "RP"; sqlals = "w "; break;

                }
                sql += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from " + Environment.NewLine;
                sql += "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from " + Environment.NewLine;
                sql += "(select a.barno, a.prccd, a.effdt, " + Environment.NewLine;
                sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn " + Environment.NewLine;
                sql += "from " + scm1 + ".t_batchmst_price a where nvl(a.rate,0) <> 0 " + Environment.NewLine;
                if (TDT.retStr() != "") sql += "and a.effdt <= to_date('" + TDT + "','dd/mm/yyyy') " + Environment.NewLine;
                sql += ") " + Environment.NewLine;
                sql += "a, " + scm1 + ".t_batchmst_price b, " + scm1 + ".t_batchmst c " + Environment.NewLine;
                sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and a.barno=c.barno(+) " + Environment.NewLine;
                sql += ") a where prccd='" + prccd + "' " + Environment.NewLine;
                sql += ") " + sqlals;
                if (x != 1) sql += ", ";
            }

            sql += "where a.autono = b.autono(+) and a.slcd = c.slcd and g.conslcd = d.slcd(+) and a.autono = f.autono(+) and h.agslcd = l.slcd(+)  and h.sagslcd = m.slcd(+) " + Environment.NewLine;
            sql += "and f.translcd = e.slcd(+) and a.autono = f.autono(+) and a.autono = g.autono(+) and a.autono = h.autono(+) and  g.autono = i.autono(+) and a.doccd = j.doccd(+) and a.autono = k.autono(+) and b.itcd=n.itcd(+) and n.itgrpcd=o.itgrpcd(+) and i.rtdebcd=p.rtdebcd(+) and b.barno=y.barno(+) and b.barno=v.barno(+) and b.barno=w.barno(+) " + Environment.NewLine;

            if (SLCD.retStr() != "") sql += " and a.slcd in (" + SLCD + ") " + Environment.NewLine;
            if (ITGRPCD.retStr() != "") sql += " and n.itgrpcd in (" + ITGRPCD + ") " + Environment.NewLine;
            if (ITCD.retStr() != "") sql += " and b.itcd in (" + ITCD + ") " + Environment.NewLine;

            sql += " and b.stkdrcr in ('D','C') " + Environment.NewLine;
            sql += ") a, " + Environment.NewLine;

            sql += "(select a.slcd, b.slnm, nvl(b.slarea,b.district) slarea, a.pdesign, max(a.jobrt) jobrt,a.itcd,a.slcd||a.itcd slitcd ";
            sql += "from " + scm1 + ".m_sitem_slcd a, " + scmf + ".m_subleg b ";
            sql += "where a.slcd = b.slcd(+)  ";
            sql += "group by a.slcd, b.slnm, a.pdesign, b.slarea, b.district,a.itcd)x ";
            sql += "where a.slitcd=x.slitcd(+) " + Environment.NewLine;

            sql += "order by itstyle,itnm,itcd ";

            DataTable tbl = masterHelp.SQLquery(sql);


            return tbl;

        }


    }
}