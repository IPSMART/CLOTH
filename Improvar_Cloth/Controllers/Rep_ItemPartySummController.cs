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
        public ActionResult Rep_ItemPartySumm(string ITCD = "", string ITNM = "", string FDT = "", string TDT = "", string CHECK = "", string ITGRPCD = "", string LOCCD = "", string SALPUR = "")
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
                    ViewBag.formname = "Design & Party Summary";
                    PartyitemSummReport VE = new PartyitemSummReport();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    string scmf = CommVar.FinSchema(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                    if (ITCD.retStr() == "")
                    {
                        VE.DropDown_list_ITGRP = dropDownHelp.GetItgrpcdforSelection();
                        VE.ITGRPCD = masterHelp.ComboFill("ITGRPCD", VE.DropDown_list_ITGRP, 0, 1);

                        VE.DropDown_list = (from i in DBF.M_LOCA
                                            where i.COMPCD == com
                                            select new DropDown_list() { value = i.LOCCD, text = i.LOCNM }).Distinct().OrderBy(s => s.text).ToList();// location
                        VE.LOCATION = masterHelp.ComboFill("loccd", VE.DropDown_list, 0, 1);
                    }
                    else
                    {
                        VE.ITCD = ITCD; VE.FDT2 = FDT; VE.TDT2 = TDT; VE.ITGRPCD = ITGRPCD;

                        VE.ITCD2 = ITCD; VE.ITGRPCD2 = ITGRPCD; VE.ONLYSALES2 = CHECK; VE.LOCATION2 = LOCCD; VE.SALPUR2 = SALPUR;

                        if (CHECK == "Y")
                        {
                            VE.CHECK = true;
                        }
                        else
                        {
                            VE.CHECK = false;
                        }
                        ViewBag.ITNM = ITNM + " [" + ITCD + "]";
                        if (ITCD.retStr() != "") ITCD = ITCD.retSqlformat();
                        if (ITGRPCD.retStr() != "") ITGRPCD = ITGRPCD.retSqlformat();
                        if (LOCCD.retStr() != "") LOCCD = LOCCD.retSqlformat();
                        DataTable dt = GetData(ITCD, FDT, TDT, ITGRPCD, CHECK, "", LOCCD, SALPUR);

                        VE.billdet = (from DataRow DR in dt.Rows
                                      select new billdet()
                                      {
                                          WPrate = DR["wprate"].ToString().retStr(),
                                          RPrate = DR["rprate"].ToString().retStr(),
                                          BarImages = DR["barimage"].ToString().retStr(),
                                          DesignNo = DR["itstyle"].ToString().retStr(),
                                      }).Distinct().OrderBy(A => A.styleno).ToList();
                        //sqnty = X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()),
                        //samt = X.Sum(Z => Z.Field<decimal>("samt").retDbl()),
                        //srate = X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()) == 0 ? 0 : (X.Sum(Z => Z.Field<decimal>("samt").retDbl()) / X.Sum(Z => Z.Field<decimal>("sqnty").retDbl())).toRound(2),

                        //rqnty = X.Sum(Z => Z.Field<decimal>("srqnty").retDbl()),
                        //ramt = X.Sum(Z => Z.Field<decimal>("sramt").retDbl()),
                        //rrate = X.Sum(Z => Z.Field<decimal>("srqnty").retDbl()) == 0 ? 0 : (X.Sum(Z => Z.Field<decimal>("sramt").retDbl()) / X.Sum(Z => Z.Field<decimal>("srqnty").retDbl())).toRound(2),

                        //netqnty = (X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()) - X.Sum(Z => Z.Field<decimal>("srqnty").retDbl())).toRound(2),
                        //netamt = (X.Sum(Z => Z.Field<decimal>("samt").retDbl()) - X.Sum(Z => Z.Field<decimal>("sramt").retDbl())).toRound(2),
                        //netrate = ((X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()) - X.Sum(Z => Z.Field<decimal>("srqnty").retDbl())).toRound(2)) == 0 ? 0 : (((X.Sum(Z => Z.Field<decimal>("samt").retDbl()) - X.Sum(Z => Z.Field<decimal>("sramt").retDbl())).toRound(2)) / ((X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()) - X.Sum(Z => Z.Field<decimal>("srqnty").retDbl())).toRound(2))).toRound(2),


                        foreach (var v in VE.billdet)
                        {
                            if (v.BarImages.retStr() != "")
                            {
                                var brimgs = v.BarImages.retStr().Split((char)179);
                                v.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                                string BarImages = "";
                                foreach (var barimg in brimgs)
                                {
                                    string barfilename = barimg.Split('~')[0];
                                    string barimgdesc = barimg.Split('~')[1];
                                    BarImages += (char)179 + CommVar.WebUploadDocURL(barfilename) + "~" + barimgdesc;
                                    string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                                    FROMpath = Path.Combine(FROMpath, "");
                                    string TOPATH = CommVar.LocalUploadDocPath() + barfilename;
                                    Cn.CopyImage(FROMpath, TOPATH);
                                }
                                v.BarImages = BarImages.retStr().TrimStart((char)179);
                            }
                        }


                        //VE.T_sqnty = VE.billdet.Sum(a => a.sqnty).retDbl();
                        //VE.T_samt = VE.billdet.Sum(a => a.samt).retDbl();
                        //VE.T_rqnty = VE.billdet.Sum(a => a.rqnty).retDbl();
                        //VE.T_ramt = VE.billdet.Sum(a => a.ramt).retDbl();
                        //VE.T_netqnty = VE.billdet.Sum(a => a.netqnty).retDbl();
                        //VE.T_netamt = VE.billdet.Sum(a => a.netamt).retDbl();

                        //VE.MGRPNM = DB.M_TMGRP.Where(a => a.MGRPCD == MGRPCD).Select(a => a.MGRPNM).SingleOrDefault();
                        //var base64 = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD).ToList();
                        //if (base64.Any())
                        //{
                        //    VE.FOUNDMGRP = true;
                        //}
                        //var group = (from i in DB.M_TGRP
                        //             where (i.MGRPCD == MGRPCD && i.GLCD == null)
                        //             select i).OrderBy(a => a.ROOTCD).ThenBy(a => a.GRPCDFULL).ToList();

                        //if (group.Any())
                        //{
                        //    List<Temp_TGRP> MLIST = new List<Temp_TGRP>();
                        //    VE.Tree = GenerateTree(group, ref MLIST);
                        //    VE.MLIST = MLIST;
                        //}
                        //var Mtype = DB.M_TMGRP.Where(a => a.MGRPCD == MGRPCD).Select(a => a.MGRPTYPE).SingleOrDefault();
                        //DataTable dt = new DataTable();
                        //if (Mtype == "SL")
                        //{
                        //    //string str = "select * from (select glcd, '' slcd, '' class1cd, glnm,'' slnm, '' class1nm from " + scmf + ".m_genleg where nvl(slcdmust,'N') = 'N' union all ";
                        //    //str = str + " select a.glcd, '', '' class1cd, a.glnm, c.slnm, '' class1nm from " + CommVar.CurSchema(UNQSNO) + ".m_genleg a, " + scmf + ".m_subleg_gl b, " + scmf + ".m_subleg c where a.glcd = b.glcd ";
                        //    //str = str + " and nvl(slcdmust,'N') = 'Y' and b.slcd = c.slcd) where glcd||slcd||class1cd not in (select glcd||slcd||class1cd from " + CommVar.CurSchema(UNQSNO) + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and glcd||slcd||class1cd is not null)";
                        //    //DataTable dt = masterHelp.SQLquery(str);
                        //    //VE.AvailableGroup = (from i in dt.AsEnumerable()
                        //    //                     select new AvailableACGroup()
                        //    //                     {
                        //    //                         Checked = false,
                        //    //                         CLASS1CD = i.Field<string>("class1cd"),
                        //    //                         CLASS1NM = i.Field<string>("class1nm"),
                        //    //                         GLCD = i.Field<string>("glcd"),
                        //    //                         GLNM = i.Field<string>("glnm"),
                        //    //                         SLCD = i.Field<string>("slcd"),
                        //    //                         SLNM = i.Field<string>("slnm"),
                        //    //                     }).ToList();
                        //}
                        //else if (Mtype == "GL")
                        //{
                        //    string str = "select glcd, '' class1cd, glnm, '' class1nm from " + scmf + ".m_genleg where glcd not in (select glcd from " + scmf + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and glcd is not null) order by glnm";
                        //    dt = masterHelp.SQLquery(str);
                        //}
                        //else if (Mtype == "CL" || Mtype == "SL")
                        //{
                        //    string str = "select * from (select glcd, '' class1cd, glnm, '' class1nm from " + scmf + ".m_genleg where nvl(class1cdmust,'N') = 'N' union all ";
                        //    str = str + "select distinct a.glcd, b.class1cd, c.glnm, e.class1nm from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_class b, " + scmf + ".m_genleg c, ";
                        //    str = str + scmf + ".m_class1 e where a.autono = b.autono and a.slno = b.dslno and a.glcd = c.glcd and b.class1cd = e.class1cd and nvl(c.class1cdmust, 'N')= 'Y' order by glcd,class1cd) ";
                        //    str = str + " where glcd|| class1cd not in (select glcd || class1cd from " + scmf + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and glcd|| class1cd is not null)";
                        //    dt = masterHelp.SQLquery(str);
                        //}
                        //VE.AvailableGroup = (from i in dt.AsEnumerable()
                        //                     select new AvailableACGroup()
                        //                     {
                        //                         Checked = false,
                        //                         CLASS1CD = i.Field<string>("class1cd"),
                        //                         CLASS1NM = i.Field<string>("class1nm"),
                        //                         GLCD = i.Field<string>("glcd"),
                        //                         GLNM = i.Field<string>("glnm"),
                        //                     }).ToList();
                    }
                    VE.DefaultView = true;
                    //List<DropDown_list1> drplist = new List<DropDown_list1>();
                    //DropDown_list1 drop1 = new DropDown_list1();
                    //drop1.text = "Yes"; drop1.value = "Y"; drplist.Add(drop1);
                    //DropDown_list1 drop2 = new DropDown_list1();
                    //drop2.text = "No"; drop2.value = "N"; drplist.Add(drop2);
                    //DropDown_list1 drop3 = new DropDown_list1();
                    //drop3.text = "Main"; drop3.value = "M"; drplist.Add(drop3);
                    //VE.SchedulePart = drplist;
                    //VE.LEGDTLSKP = false;

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
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                var agent = Code.Split(Convert.ToChar(Cn.GCS()));
                if (agent.Count() > 1)
                {
                    if (agent[1] == "")
                    {
                        return Content("Please Select Agent !!");
                    }
                    else
                    {
                        Code = agent[0];
                    }
                }
                else if (Code == "party")
                {
                    PartyitemSummReport VE = new PartyitemSummReport();
                    Cn.getQueryString(VE);
                    switch (VE.DOC_CODE)
                    {
                        case "SORD": Code = "D"; break;
                        case "PORD": Code = "C"; break;
                        default: Code = "D"; break;
                    }

                }
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
        public string ITCD_help(string val, string ITGTYPE, string ITGRPCD = "", string FABITCD = "", string DOC_EFF_DT = "", string JOB_CD = "")
        {
            try
            {

                string scm1 = CommVar.CurSchema(UNQSNO);
                string valsrch = val.ToUpper().Trim();
                string sql = "", prccd = "WP";
                if (ITGTYPE.retStr() != "")
                {
                    if (ITGTYPE.IndexOf(',') == -1 && ITGTYPE.IndexOf("'") == -1) ITGTYPE = "'" + ITGTYPE + "'";
                }
                sql += "select a.itcd, a.itnm, a.uomcd, a.itgrpcd, b.itgrpnm,b.bargentype, b.itgrptype,a.styleno, a.PCSPERSET,nvl(a.hsncode,b.hsncode)hsncode, " + Environment.NewLine;
                sql += "a.fabitcd, c.itnm fabitnm, a.styleno||' '||a.itnm itstyle,a.convqtypunit,a.convuomcd " + Environment.NewLine;
                sql += " from " + scm1 + ".m_sitem a, " + scm1 + ".m_group b, " + scm1 + ".m_sitem c " + Environment.NewLine;

                //sql += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from " + Environment.NewLine;
                //sql += "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from " + Environment.NewLine;
                //sql += "(select a.barno, a.prccd, a.effdt, " + Environment.NewLine;
                //sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn " + Environment.NewLine;
                //sql += "from " + scm1 + ".t_batchmst_price a where nvl(a.rate,0) <> 0 ) " + Environment.NewLine;
                //sql += "a, " + scm1 + ".t_batchmst_price b, " + scm1 + ".t_batchmst c " + Environment.NewLine;
                //sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and a.barno=c.barno(+) " + Environment.NewLine;
                //sql += ") a where prccd='" + prccd + "') d, " + Environment.NewLine;

                //for (int x = 0; x <= 1; x++)
                //{
                //    string sqlals = "";
                //    switch (x)
                //    {
                //        case 0:
                //            sqlals = "v "; break;
                //        case 1:
                //            prccd = "RP"; sqlals = "w "; break;

                //    }
                //    sql += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from " + Environment.NewLine;
                //    sql += "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from " + Environment.NewLine;
                //    sql += "(select a.barno, a.prccd, a.effdt, " + Environment.NewLine;
                //    sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn " + Environment.NewLine;
                //    sql += "from " + scm1 + ".t_batchmst_price a where nvl(a.rate,0) <> 0 " + Environment.NewLine;
                //    sql += ") " + Environment.NewLine;
                //    sql += "a, " + scm1 + ".t_batchmst_price b, " + scm1 + ".t_batchmst c " + Environment.NewLine;
                //    sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and a.barno=c.barno(+) " + Environment.NewLine;
                //    sql += ") a where prccd='" + prccd + "' " + Environment.NewLine;
                //    sql += ") " + sqlals + Environment.NewLine;
                //    if (x != 1) sql += ", " + Environment.NewLine;
                //}

                sql += "where a.itgrpcd=b.itgrpcd and a.fabitcd=c.itcd(+) " + Environment.NewLine;
                if (DOC_EFF_DT.retStr() != "" || JOB_CD.retStr() != "")
                {
                    sql += "and a.itcd = (select distinct y.itcd from " + scm1 + ".v_sjobmst_stdrt y where a.itcd=y.itcd " + Environment.NewLine;
                    if (JOB_CD.retStr() != "") sql += "and y.jobcd='" + JOB_CD.retStr() + "' ";
                    if (DOC_EFF_DT.retStr() != "") sql += "and y.bomeffdt <= to_date('" + DOC_EFF_DT.retStr() + "','dd/mm/yyyy')  " + Environment.NewLine;
                    sql += ") " + Environment.NewLine;
                }
                if (ITGRPCD.retStr() != "") sql += "and a.ITGRPCD ='" + ITGRPCD + "' " + Environment.NewLine;
                if (ITGTYPE.retStr() != "") sql += "and b.itgrptype in (" + ITGTYPE + ") " + Environment.NewLine;//else sql += "and b.itgrptype not in ('F','C') ";

                if (valsrch.retStr() != "") sql += "and ( upper(a.itcd) like '%" + valsrch + "%' or upper(a.itnm) like '%" + valsrch + "%' or upper(a.styleno) like '%" + valsrch + "%' or upper(a.uomcd) like '%" + valsrch + "%'or upper(a.styleno||' '||a.itnm) like '%" + valsrch + "%'  )  ";

                DataTable rsTmp = masterHelpFa.SQLquery(sql);

                if (val.retStr() == "" || rsTmp.Rows.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= rsTmp.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + rsTmp.Rows[i]["itstyle"] + "</td><td>" + rsTmp.Rows[i]["itnm"] + "</td><td>" + rsTmp.Rows[i]["itcd"] + "</td><td>" + rsTmp.Rows[i]["uomcd"] + "</td><td>" + rsTmp.Rows[i]["itgrpnm"] + "</td><td>" + rsTmp.Rows[i]["itgrpcd"] + "</td><td>" + rsTmp.Rows[i]["fabitnm"] + "</td><td>" + rsTmp.Rows[i]["fabitcd"] + "</td></tr>");
                    }
                    var hdr = "Design No." + Cn.GCS() + "Item Name" + Cn.GCS() + "Item Code" + Cn.GCS() + "UOM" + Cn.GCS() + "Item Group Name" + Cn.GCS() + "Item Group Code" + Cn.GCS() + "Fabric Item Name" + Cn.GCS() + "Fabric Item Code";
                    return masterHelpFa.Generate_help(hdr, SB.ToString());
                }
                else
                {
                    string str = "";
                    if (rsTmp.Rows.Count > 0)
                    {
                        str = masterHelpFa.ToReturnFieldValues("", rsTmp);
                    }
                    else
                    {
                        str = "Invalid Item Code. Please Enter a Valid Item Code !!";
                    }
                    return str;
                }
            }
            catch (Exception ex)
            {
                return ex.Message + " " + ex.InnerException;
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
                //string itcd = (from a in VE.billdet where a.Checked == true select a.itcd).ToArray().retSqlfromStrarray();
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
                                 amt = X.Key.doctag.retStr() == doctag ? X.Sum(Z => Z.Field<decimal>("amt").retDbl()) : (X.Sum(Z => Z.Field<decimal>("amt").retDbl()) * -1),
                             }).OrderBy(A => A.Design).OrderBy(A => A.refdt).OrderBy(A => A.refno).ToList();

                VE.T_qnty = VE.ItmDet.Sum(a => a.qnty).retDbl();
                VE.T_amt = VE.ItmDet.Sum(a => a.amt).retDbl();

                //VE.T_sqntyi = VE.ItmDet.Sum(a => a.sqnty).retDbl();
                //VE.T_samti = VE.ItmDet.Sum(a => a.samt).retDbl();
                //VE.T_rqntyi = VE.ItmDet.Sum(a => a.rqnty).retDbl();
                //VE.T_ramti = VE.ItmDet.Sum(a => a.ramt).retDbl();
                string jobrt="";
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
            //if (CHECK == "Y")
            //{
            //    txntag = "'SB'";
            //}
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
            sql += " union " + Environment.NewLine;
            sql += "select a.autono,";
            sql += "(case when d.doctype in ('SBILD','SPSLP','SBCM','SBPOS','SBCMR','SPRM') then 'C' else 'D' end ) " + Environment.NewLine;
            sql += " stkdrcr, a.slno + 1000 slno, a.amtcd itcd, '' itrem ,'' barno, b.amtnm itnm,b.amtnm itstyle ,a.hsncode,  " + Environment.NewLine;
            sql += " 'OTH' uomcd, 'OTH' uomnm, 0 decimals,'' colrcd,'' colrnm, 0 nos, 0 qnty, a.amtrate rate, a.amt,0 scmdiscrate, 0 scmdiscamt, 0 tddiscamt, 0 discamt,a.amt TXBLVAL,0 NETAMT, a.igstper, a.igstamt, " + Environment.NewLine;
            sql += " a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,0 blqnty,'' bluomcd,''bluomnm,0 bldecimals,0 pageno,0 PAGESLNO,''baleno " + Environment.NewLine;
            sql += " from " + scm1 + ".t_txnamt a, " + scm1 + ".m_amttype b, " + scm1 + ".t_cntrl_hdr c, " + scm1 + ".m_doctype d " + Environment.NewLine;
            sql += " where a.amtcd = b.amtcd and a.autono=c.autono(+) and c.doccd=d.doccd(+) " + Environment.NewLine;
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

    }
}