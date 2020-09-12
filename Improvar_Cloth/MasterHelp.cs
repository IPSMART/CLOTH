using System;
using System.Collections.Generic;
using System.Linq;
using Improvar.Models;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Ajax.Utilities;
using Microsoft.Win32;
using System.Web;
using System.Text;
using System.Reflection;
using ClosedXML.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace Improvar
{
    public class MasterHelp : MasterHelpFa
    {
        Connection Cn = new Connection();
        Salesfunc salesfunc = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public string ITCD_help(string val, string ITGTYPE, string ITGRPCD = "", string FABITCD = "", string DOC_EFF_DT = "", string JOB_CD = "")
        {
            try
            {
                string scm1 = CommVar.CurSchema(UNQSNO);
                string valsrch = val.ToUpper().Trim();
                string sql = "";
                if (ITGTYPE.retStr() != "")
                {
                    if (ITGTYPE.IndexOf(',') == -1 && ITGTYPE.IndexOf("'") == -1) ITGTYPE = "'" + ITGTYPE + "'";
                }
                sql += "select a.itcd, a.itnm, a.uomcd, a.itgrpcd, b.itgrpnm, b.itgrptype,a.styleno, a.PCSPERSET,a.hsncode, a.fabitcd, c.itnm fabitnm, a.itnm||' '||a.styleno itstyle ";
                sql += "from " + scm1 + ".m_sitem a, " + scm1 + ".m_group b, " + scm1 + ".m_sitem c ";
                sql += "where a.itgrpcd=b.itgrpcd and a.fabitcd=c.itcd(+) ";
                if (DOC_EFF_DT.retStr() != "" || JOB_CD.retStr() != "")
                {
                    sql += "and a.itcd = (select distinct y.itcd from " + scm1 + ".v_sjobmst_stdrt y where a.itcd=y.itcd ";
                    if (JOB_CD.retStr() != "") sql += "and y.jobcd='" + JOB_CD.retStr() + "' ";
                    if (DOC_EFF_DT.retStr() != "") sql += "and y.bomeffdt <= to_date('" + DOC_EFF_DT.retStr() + "','dd/mm/yyyy')  ";
                    sql += ") ";
                }
                if (ITGRPCD.retStr() != "") sql += "and a.ITGRPCD ='" + ITGRPCD + "' ";
                if (ITGTYPE.retStr() != "") sql += "and b.itgrptype in (" + ITGTYPE + ") ";
                if (valsrch.retStr() != "") sql += "and ( upper(a.itcd) like '%" + valsrch + "%' or upper(a.itnm) like '%" + valsrch + "%' or upper(a.styleno) like '%" + valsrch + "%' or upper(a.uomcd) like '%" + valsrch + "%'  )  ";

                DataTable rsTmp = SQLquery(sql);

                if (val.retStr() == "" || rsTmp.Rows.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= rsTmp.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + rsTmp.Rows[i]["styleno"] + "</td><td>" + rsTmp.Rows[i]["itnm"] + "</td><td>" + rsTmp.Rows[i]["itcd"] + "</td><td>" + rsTmp.Rows[i]["uomcd"] + "</td><td>" + rsTmp.Rows[i]["itgrpnm"] + "</td><td>" + rsTmp.Rows[i]["itgrpcd"] + "</td><td>" + rsTmp.Rows[i]["fabitnm"] + "</td><td>" + rsTmp.Rows[i]["fabitcd"] + "</td></tr>");
                    }
                    var hdr = "Design No." + Cn.GCS() + "Item Name" + Cn.GCS() + "Item Code" + Cn.GCS() + "UOM" + Cn.GCS() + "Item Group Name" + Cn.GCS() + "Item Group Code" + Cn.GCS() + "Fabric Item Name" + Cn.GCS() + "Fabric Item Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    string str = "";
                    if (rsTmp.Rows.Count > 0)
                    {
                        str = ToReturnFieldValues("", rsTmp);
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
        public string AUTHCD_help(ImprovarDB DB)
        {
            using (DB)
            {
                var query = (from c in DB.M_SIGN_AUTH
                             join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                             where i.INACTIVE_TAG == "N"
                             select new
                             {
                                 Code = c.AUTHCD,
                                 Description = c.AUTHNM
                             }).ToList();
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr ><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
                }
                var hdr = "Authority Name" + Cn.GCS() + "Authority Code";
                return Generate_help(hdr, SB.ToString());
            }
        }
        //public string PRCCD_help(string val)
        //{
        //    var UNQSNO = Cn.getQueryStringUNQSNO();
        //    using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
        //    {
        //        var query = (from c in DB.M_PRCLST
        //                     join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
        //                     where i.INACTIVE_TAG == "N"
        //                     select new
        //                     {
        //                         Code = c.PRCCD,
        //                         Description = c.PRCNM
        //                     }).ToList();
        //        if (val == null)
        //        {
        //            System.Text.StringBuilder SB = new System.Text.StringBuilder();
        //            for (int i = 0; i <= query.Count - 1; i++)
        //            {
        //                SB.Append("<tr><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
        //            }
        //            var hdr = "Price Name" + Cn.GCS() + "Price code";
        //            return Generate_help(hdr, SB.ToString());
        //        }
        //        else
        //        {
        //            query = query.Where(a => a.Code == val).ToList();
        //            if (query.Any())
        //            {
        //                string str = "";
        //                foreach (var i in query)
        //                {
        //                    str = i.Code + Cn.GCS() + i.Description;
        //                }
        //                return str;
        //            }
        //            else
        //            {
        //                return "Invalid Price Code ! Please Enter a Valid Price Code !!";
        //            }

        //        }
        //    }
        //}
        public string DISCRTCD_help(ImprovarDB DB)
        {
            using (DB)
            {
                var query = (from c in DB.M_DISCRT
                             join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                             where i.INACTIVE_TAG == "N"
                             select new
                             {
                                 code = c.DISCRTCD,
                                 name = c.DISCRTNM,
                             }).ToList();
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].name + "</td><td>" + query[i].code + "</td></tr>");
                }
                var hdr = "Discount Code" + Cn.GCS() + "Discount Name";
                return Generate_help(hdr, SB.ToString());
            }
        }
        public string ITGRPCD_help(string ITGRPCD, string GRPTYPE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                string valsrch = ITGRPCD.ToUpper().Trim();
                string sql = "select * from " + CommVar.CurSchema(UNQSNO) + ".M_GROUP where 1=1";
                if (GRPTYPE.retStr() != "") sql += " and ITGRPTYPE in (" + GRPTYPE.retSqlformat() + ") ";
                if (valsrch.retStr() != "") sql += "and ( upper(ITGRPCD) like '%" + valsrch + "%' or upper(ITGRPNM) like '%" + valsrch + "%' ) ";

                DataTable dt = SQLquery(sql);
                if (ITGRPCD.retStr() == "" || dt.Rows.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= dt.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + dt.Rows[i]["ITGRPNM"] + "</td><td>" + dt.Rows[i]["ITGRPCD"] + "</td><td>" + dt.Rows[i]["ITGRPTYPE"] + "</td></tr>");
                    }
                    var hdr = "Item Group Name" + Cn.GCS() + "Item Group Code" + Cn.GCS() + "Item Group Type";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        return ToReturnFieldValues("", dt);
                    }
                    else
                    {
                        return "Invalid Item Group Code ! Please Enter a Valid Item Group Code !!";
                    }
                }
            }
        }
        public string SIZE(string val, string itcd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var query = (from c in DB.M_SIZE join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO where i.INACTIVE_TAG == "N" select new { SIZECD = c.SIZECD, SIZENM = c.SIZENM, SZBARCODE = c.SZBARCODE }).ToList();

                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].SIZENM + "</td><td>" + query[i].SIZECD + "</td><td>" + query[i].SZBARCODE + "</td></tr>");
                    }
                    var hdr = "Size Name" + Cn.GCS() + "Size Code" + Cn.GCS() + "Size Bar Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.SIZECD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.SIZECD + Cn.GCS() + i.SIZENM + Cn.GCS() + i.SZBARCODE;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Size Code ! Please Enter a Valid Size Code !!";
                    }
                }
            }
        }
        public string COLOR(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var query = (from c in DB.M_COLOR join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO where i.INACTIVE_TAG == "N" select new { COLRCD = c.COLRCD, COLRNM = c.COLRNM, c.CLRBARCODE }).ToList();
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].COLRNM + "</td><td>" + query[i].COLRCD + "</td><td>" + query[i].CLRBARCODE + "</td></tr>");
                    }
                    var hdr = "Color Name" + Cn.GCS() + "Color Code" + Cn.GCS() + "Color Bar Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.COLRCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.COLRCD + Cn.GCS() + i.COLRNM + Cn.GCS() + i.CLRBARCODE;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Color Code ! Please Enter a Valid Color Code !!";
                    }
                }
            }
        }
        public string COLLECTION(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var query = (from c in DB.M_COLLECTION
                             join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                             where i.INACTIVE_TAG == "N"
                             select new
                             {
                                 COLLCD = c.COLLCD,
                                 COLLNM = c.COLLNM
                             }).ToList();

                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].COLLNM + "</td><td>" + query[i].COLLCD + "</td></tr>");
                    }
                    var hdr = "Collection Name" + Cn.GCS() + "Collection Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.COLLCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.COLLCD + Cn.GCS() + i.COLLNM;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Collection Code ! Please Enter a Valid Collection Code !!";
                    }
                }
            }
        }
        public string PARTS(string val, string itcd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var query = (from c in DB.M_PARTS join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO where i.INACTIVE_TAG == "N" select new { PARTCD = c.PARTCD, PARTNM = c.PARTNM }).ToList();
                if (itcd != "")
                {
                    query = (from c in DB.M_SITEM_PARTS join k in DB.M_PARTS on c.PARTCD equals k.PARTCD join i in DB.M_CNTRL_HDR on k.M_AUTONO equals i.M_AUTONO where (k.M_AUTONO == i.M_AUTONO && i.INACTIVE_TAG == "N" && c.ITCD == itcd) select new { PARTCD = k.PARTCD, PARTNM = k.PARTNM }).ToList();
                }
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].PARTNM + "</td><td>" + query[i].PARTCD + "</td></tr>");
                    }
                    var hdr = "Part Name" + Cn.GCS() + "Part Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.PARTCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.PARTCD + Cn.GCS() + i.PARTNM;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Part Code ! Please Enter a Valid Part Code !!";
                    }
                }
            }
        }
        public string SBRAND(string val, string BRAND)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var query = (from c in DB.M_SUBBRAND
                             join o in DB.M_BRAND on c.MBRANDCD equals (o.BRANDCD)
                             where o.BRANDCD == BRAND
                             select new
                             {
                                 SBRANDCD = c.SBRANDCD,
                                 SBRANDNM = c.SBRANDNM,
                                 BRANDNM = o.BRANDNM
                             }).ToList();
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].SBRANDNM + "</td><td>" + query[i].SBRANDCD + "</td><td>" + query[i].BRANDNM + "</td></tr>");
                    }
                    var hdr = "Sub Brand Name" + Cn.GCS() + "Sub Brand Code" + Cn.GCS() + "Brand Name";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.SBRANDCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.SBRANDCD + Cn.GCS() + i.SBRANDNM;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Sub Brand Code ! Please Enter a Valid Sub Brand Code !!";
                    }
                }
            }
        }

        public string PRODGRPCD_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var query = (from c in DB.M_PRODGRP join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO where i.INACTIVE_TAG == "N" select new { PRODGRPCD = c.PRODGRPCD, PRODGRPNM = c.PRODGRPNM, }).ToList();
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].PRODGRPNM + "</td><td>" + query[i].PRODGRPCD + "</td></tr>");
                    }
                    var hdr = "Product Group Name" + Cn.GCS() + "Product Group Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.PRODGRPCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.PRODGRPCD + Cn.GCS() + i.PRODGRPNM;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Product Group Code ! Please Select / Enter a Valid Product Group Code !!";
                    }
                }
            }
        }

        public string BRANDCD_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var tbldatadoc = (from c in DB.M_BRAND
                                  join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                                  where i.INACTIVE_TAG == "N"
                                  select new
                                  {
                                      M_AUTONO = c.M_AUTONO,
                                      Code = c.BRANDCD,
                                      Description = c.BRANDNM,
                                  }).ToList();

                List<TEMP_helpimg> TEMP_helpimg1 = new List<TEMP_helpimg>();
                foreach (var i in tbldatadoc)
                {
                    TEMP_helpimg UPL = new TEMP_helpimg();

                    var image = (from h in DB.M_CNTRL_HDR_DOC_DTL
                                 where h.M_AUTONO == i.M_AUTONO && h.SLNO == 1
                                 select h).OrderBy(d => d.RSLNO).ToList();

                    var hh = image.GroupBy(x => x.M_AUTONO).Select(x => new
                    {
                        namefl = string.Join("", x.Select(n => n.DOC_STRING))
                    });
                    foreach (var ii in hh)
                    {
                        UPL.DOC_FILE = ii.namefl;
                    }
                    UPL.M_AUTONO = i.M_AUTONO;
                    TEMP_helpimg1.Add(UPL);
                }
                var query = (from p in tbldatadoc
                             join q in TEMP_helpimg1 on p.M_AUTONO equals q.M_AUTONO
                             select new
                             {
                                 Code = p.Code,
                                 Description = p.Description,
                                 DOC_FILE = q.DOC_FILE,
                                 DOC_NAME = q.DOC_NAME
                             }).ToList();
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td><td>  <img src='" + query[i].DOC_FILE + "' style='width:20px;height:20px;' />   </td></tr>");
                    }
                    var hdr = "Brand Name" + Cn.GCS() + "Brand Code" + Cn.GCS() + "Image";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.Code == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.Code + Cn.GCS() + i.Description;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Brand Code ! Please Select / Enter a Valid Brand Code !!";
                    }
                }
            }
        }
        public string GSTUOM(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            var query = (from c in DB.MS_GSTUOM select new { GUOMCD = c.GUOMCD, GUOMNM = c.GUOMNM }).ToList();
            if (val == null)
            {

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].GUOMNM + "</td><td>" + query[i].GUOMCD + "</td></tr>");
                }
                var hdr = "GST UOM Name" + Cn.GCS() + "GST UOM Code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                query = query.Where(a => a.GUOMCD == val).ToList();
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.GUOMCD + Cn.GCS() + i.GUOMNM;
                    }
                    return str;
                }
                else
                {
                    return "Invalid GST UOM Code ! Please Select / Enter a Valid GST UOM Code !!";
                }
            }

        }
        public string TAXGRPCD_help(ImprovarDB DB)
        {
            using (DB)
            {
                var query = (from i in DB.M_TAXGRP
                             join j in DB.M_CNTRL_HDR on i.M_AUTONO equals j.M_AUTONO
                             where j.INACTIVE_TAG == "N"
                             select new { CODE = i.TAXGRPCD, NAME = i.TAXGRPNM }).OrderBy(s => s.NAME).ToList();
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].NAME + "</td><td>" + query[i].CODE + "</td></tr>");
                }
                var hdr = "Tax Group Name" + Cn.GCS() + "Tax Group Code";
                return Generate_help(hdr, SB.ToString());
            }
        }
        public string PARK_ENTRIES(string COM_CD, string LOC_CD, string menuID, string menuIndex, string uid, string path)
        {
            Connection cn = new Connection();
            var UNQSNO = cn.getQueryStringUNQSNO();
            var PreviousUrl = HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
            var uri = new Uri(PreviousUrl);//Create Virtually Query String
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            var PermissionID1 = queryString.Get("MNUDET").ToString().Replace(" ", "+");
            string PermissionID = cn.Decrypt_URL(PermissionID1);
            string[] PermissionIDArray = PermissionID.Split('~');
            string currentPath = HttpContext.Current.Request.Path;
            currentPath = currentPath.Replace("Home", PermissionIDArray[2]);
            currentPath = currentPath.Replace("ReadParkRecord", "RetrivePark");
            string url = currentPath;
            string ID = menuID + menuIndex + CommVar.Loccd(UNQSNO) + CommVar.Compcd(UNQSNO) + CommVar.CurSchema(UNQSNO);
            string Userid = uid;
            INI Handel_ini = new INI();
            string[] keys = Handel_ini.GetEntryNames(uid, path);
            string[] entries = Array.FindAll(keys, element => element.StartsWith(ID, StringComparison.Ordinal));
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            SB.Append("<table id='helpmnu' class='table  table-striped table-bordered table-hover compact' cellpadding='3px' cellspacing='3px' width='100%'><thead style='background-color:#2965aa; color:white'><tr>");
            SB.Append("<th align='center' tabindex='0'>" + "Sl.No." + "</th>");
            SB.Append("<th  tabindex='1'>" + "Select Record(s) from park" + "</th>");
            SB.Append("<th  tabindex='2'>" + "Option" + "</th>");
            SB.Append("</tr></thead><tbody>");
            for (int i = 0; i <= entries.Length - 1; i++)
            {
                string[] spl = entries[i].Split('*');
                string datetime = spl[1].Replace('_', ' ');
                DateTime dt = DateTime.ParseExact(datetime, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                dt = dt.AddHours(72);
                if (dt < DateTime.Now)
                {
                    SB.Append("<tr><td>" + (i + 1).ToString() + "</td><td>" + spl[1] + "&nbsp;<span style='color:red'>(72 hours over)</span>" + "</td><td align='center' title='Delete Park' style='color:red' onclick =return&nbsp;deleteparkfromhelp('" + entries[i].ToString() + "','" + menuID + "','" + menuIndex + "');><strong>X</strong></td></tr>");
                }
                else
                {
                    SB.Append("<tr><td>" + (i + 1).ToString() + "</td><td onclick =return&nbsp;getparkfromhelp('" + entries[i].ToString() + "','" + url + "');>" + spl[1] + "</td><td align='center' title='Delete Park' style='color:red' onclick =return&nbsp;deleteparkfromhelp('" + entries[i].ToString() + "','" + menuID + "','" + menuIndex + "');><strong>X</strong></td></tr>");
                }
            }
            SB.Append("</tbody></table>");
            return SB.ToString();
        }
        public List<DocumentThrough> DocumentThrough()
        {
            List<DocumentThrough> DDL = new List<DocumentThrough>();
            DocumentThrough DDL1 = new DocumentThrough();
            DDL1.text = "Direct";
            DDL1.value = "D";
            DDL.Add(DDL1);
            DocumentThrough DDL2 = new DocumentThrough();
            DDL2.text = "Agent";
            DDL2.value = "A";
            DDL.Add(DDL2);
            DocumentThrough DDL3 = new DocumentThrough();
            DDL3.text = "Bank";
            DDL3.value = "B";
            DDL.Add(DDL3);
            DocumentThrough DDL4 = new DocumentThrough();
            DDL4.text = "Others";
            DDL4.value = "O";
            DDL.Add(DDL4);
            DocumentThrough DDL5 = new DocumentThrough();
            DDL5.text = "Branch";
            DDL5.value = "N";
            DDL.Add(DDL5);
            DocumentThrough DDL6 = new DocumentThrough();
            DDL6.text = "Hold";
            DDL6.value = "H";
            DDL.Add(DDL6);
            DocumentThrough DDL7 = new DocumentThrough();
            DDL7.text = "Agst Pymt";
            DDL7.value = "P";
            DDL.Add(DDL7);
            DocumentThrough DDL8 = new DocumentThrough();
            DDL8.text = " ";
            DDL8.value = " ";
            DDL.Add(DDL8);
            return DDL;
        }
        public string SUBLEDGER(string val, string LINKCD = "A,C,D,E,G,J,M,O,P,S,T")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string sql = "";
            string linkcd = LINKCD.retSqlformat();

            sql += "select distinct a.slcd, a.slnm, a.gstno, nvl(a.slarea,a.district) district ";
            sql += "from " + scmf + ".m_subleg a, " + scmf + ".m_subleg_link b, " + scmf + ".m_cntrl_hdr c, " + scmf + ".m_cntrl_loca d ";
            sql += "where a.slcd=b.slcd(+) and a.m_autono=c.m_autono(+) and a.m_autono=d.m_autono(+) and ";
            if (val.retStr() != "") sql += "a.slcd='" + val + "' and ";
            sql += "b.linkcd in (" + linkcd + ") and ";
            sql += "(d.compcd='" + COM + "' or d.compcd is null) and (d.loccd='" + LOC + "' or d.loccd is null) and ";
            sql += "nvl(c.inactive_tag,'N') = 'N' ";
            sql += "order by slnm,slcd";
            DataTable tbl = SQLquery(sql);

            string Caption = "";
            if (LINKCD == "D") { Caption = "Party"; }
            else if (LINKCD == "T") { Caption = "Transporter"; }
            else if (LINKCD == "U") { Caption = "Courier"; }
            else if (LINKCD == "M") { Caption = "SalesMen"; }
            else if (LINKCD == "A") { Caption = "Agent"; }
            else if (LINKCD == "C") { Caption = "Creditor"; }
            else if (LINKCD == "E") { Caption = "Employee"; } else { Caption = "Subledger"; }
            if (val == null)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["slnm"] + " </td><td>" + tbl.Rows[i]["slcd"] + " </td><td>" + tbl.Rows[i]["gstno"] + " </td><td>" + tbl.Rows[i]["district"] + " </td></tr>");
                }
                var hdr = "" + Caption + " Name" + Cn.GCS() + "" + Caption + " Code" + Cn.GCS() + " GSTNO" + Cn.GCS() + " Area";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = "";
                    if (LINKCD == "D")
                    {
                        str = tbl.Rows[0]["slcd"] + Cn.GCS() + tbl.Rows[0]["slnm"] + Cn.GCS() + tbl.Rows[0]["district"];
                    }
                    else
                    {
                        str = tbl.Rows[0]["slcd"] + Cn.GCS() + tbl.Rows[0]["slnm"];
                    }
                    return str;
                }
                else
                {
                    return "Invalid " + Caption + " Code ! Please Enter a Valid " + Caption + " Code !!";
                }
            }
        }
        public string AREACD_help(ImprovarDB DB)
        {

            var query = (from c in DB.M_AREACD
                         join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                         where i.INACTIVE_TAG == "N"
                         select new
                         {
                             AREACD = c.AREACD,
                             AREANM = c.AREANM
                         }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr><td>" + query[i].AREANM + "</td><td>" + query[i].AREACD + "</td></tr>");
            }
            var hdr = "Area Name" + Cn.GCS() + "Area Code";
            return Generate_help(hdr, SB.ToString());
        }

        public string COMPANY_HELP(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                var query = (from c in DB.M_COMP select new { COMPCD = c.COMPCD, COMPNM = c.COMPNM }).ToList();
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].COMPNM + "</td><td>" + query[i].COMPCD + "</td></tr>");
                    }
                    var hdr = "Company Name" + Cn.GCS() + "Company Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.COMPCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.COMPCD + Cn.GCS() + i.COMPNM;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Company Code ! Please Select / Enter a Valid Company Code !!";
                    }
                }
            }
        }
        public string DOCCD_help(string val, string doctype = "", string doccd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            var query = (from c in DB.M_DOCTYPE orderby c.DOCNM select new { DOCTYPE = c.DOCTYPE, DOCNM = c.DOCNM, DOCCD = c.DOCCD, }).ToList();

            if (doctype != "") { string[] DOC_TYPE = doctype.Split(','); query = query.Where(a => a != null && query.Count > 0 && DOC_TYPE.Contains(a.DOCTYPE)).ToList(); }

            if (doccd != "") { string[] DOC_CODE = doccd.Split(','); query = query.Where(a => a != null && query.Count > 0 && DOC_CODE.Contains(a.DOCCD)).ToList(); }

            if (val == null)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].DOCNM + "</td><td>" + query[i].DOCCD + "</td></tr>");
                }
                var hdr = "Document Name" + Cn.GCS() + "Document Code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                query = query.Where(a => a.DOCCD == val).ToList();
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = ToReturnFieldValues(query);
                    }
                    return str;
                }
                else
                {
                    return "Invalid Document Code ! Please Select / Enter a Valid Document Code !!";
                }
            }
        }
        public string GATE_ENTRY(string AUTONO, string DOCNO)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string LOC = CommVar.Loccd(UNQSNO);
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.InvSchema(UNQSNO)))
            {
                var query = (from X in DB.T_GENTRY
                             join Y in DB.T_CNTRL_HDR on X.AUTONO equals Y.AUTONO
                             where Y.LOCCD == LOC
                             select new { GATEENTNUMBER = X.DOCNO, GATEENTDT = Y.USR_ENTDT, GATEENTNO = X.AUTONO, RECVPERSON = X.PERSNM, LORRYNO = X.VEHLNO }).ToList();

                if (DOCNO == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].GATEENTNUMBER + "</td><td>" + query[i].GATEENTDT + "</td><td>" + query[i].RECVPERSON + "</td><td>" + query[i].LORRYNO + "</td><td>" + query[i].GATEENTNO + "</td></tr>");
                    }
                    var hdr = "Gate Entry Number" + Cn.GCS() + "Gate Entry Date" + Cn.GCS() + "Receiving Person" + Cn.GCS() + "Vehicle Number" + Cn.GCS() + "AUTONO";
                    return Generate_help(hdr, SB.ToString(), "4");
                }
                else
                {
                    query = query.Where(a => a.GATEENTNUMBER == DOCNO).ToList();
                    //query = query.Where(a => a.GATEENTNO == AUTONO).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = ToReturnFieldValues(query);
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Gate Entry Number ! Please Enter a Valid Gate Entry Number !!";
                    }
                }
            }
        }

        public string ComboFill<T>(string name, IEnumerable<T> Linqlist, int textindex, int valindex, int width = 219)
        {
            PropertyInfo[] columns = null;
            foreach (T i in Linqlist)
            {
                columns = ((Type)i.GetType()).GetProperties();
                break;
            }
            int lastindex = columns == null ? 0 : columns.Count();
            string textwidth = (width - 38).ToString() + "px";
            string buttonwidth = width.ToString() + "px";
            string hiddentext = name + "text";
            string hiddenval = name + "value";
            string unselval = name + "unselvalue";
            string search = name + "src";
            string maindiv = name + "maindiv";
            string tablename = name + "cstable";
            string button = "<button type='button' class='btn btn-default' value='' style='height:35px; width:" + buttonwidth + "' onclick=csTableOnOff('" + maindiv + "','" + search + "');>";
            button = button + "<input id='" + hiddentext + "' type='text' value='' placeholder='Nothing selected' style='width:" + textwidth + ";border:none;outline:none;background-color:initial;cursor:pointer;font-size:12px' readonly='readonly' />";
            //button = button + "<span class='bs-caret' style='float:right;text-align:center'><span class='caret'></span></span><input id='" + hiddenval + "' type='hidden' value='' /></button>";
            button = button + "<span class='bs-caret' style='float:right;text-align:center'><span class='caret'></span></span><input id='" + hiddenval + "' type='hidden' value='' /></button>";
            button = button + "<input id='" + unselval + "' type='hidden' value='' /></button>";

            string div = "<div id='" + maindiv + "' style='border:1px solid #e6e4e4;position:absolute;z-index:2;background-color:#ffffff;display:none;min-width:219px'><div class='row' style='padding:5px;width:auto'><input id='" + search + "' autocomplete='off' type='text' placeholder='Search..' class='form-control' onkeyup=filterCSTABLETable(event,'" + tablename + "'); /></div>";

            div = div + "<div class='row' style='padding:1px;width:auto'><div class='col-sm-6'><input type='button' class='btn btn-default' title='Select all' value='Select all' style='width:100%' onclick=cstableSelect('" + tablename + "'," + textindex + "," + valindex + "," + lastindex + ",'" + name + "'); /></div>";
            div = div + "<div class='col-sm-6'><input type='button' class='btn btn-default' title='Deselect all' value='Deselect all' style='width:100%' onclick=cstableDeselect('" + tablename + "'," + textindex + "," + valindex + "," + lastindex + ",'" + name + "'); /></div></div>";
            string str = "";
            if (columns != null)
            {
                str = "<div class='sticky-table sticky-ltr-cells' style='border-top:none'><div class='row' style='padding-left:5px;padding-right:5px;width:auto;height:auto; max-height:250px;overflow-y:auto;margin-bottom:5px;margin-top:5px'><table id='" + tablename + "' class='cstable' cellspacing='0' cellpadding='2' style='width:100%;table-layout:auto'><thead><tr class='sticky-header'>";
                foreach (PropertyInfo GetProperty in columns)
                {
                    string getname = (GetProperty.Name == "text" ? "Description" : (GetProperty.Name == "value" ? "Code" : GetProperty.Name));
                    str = str + "<th style='outline:none;background-color:#fff;color:#767777'>" + getname + "</th>";
                }
                str = str + "<th style='width:50px;outline:none;background-color:#fff;'></th></tr></thead><tbody>";
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int x = 0; x <= Linqlist.Count() - 1; x++)
                {
                    var t = Linqlist.ElementAt(x);
                    SB.Append("<tr tabIndex='100' onclick=rowtoggle(" + textindex + "," + valindex + "," + lastindex + ",this,'" + name + "');>");
                    columns = ((Type)t.GetType()).GetProperties();
                    foreach (PropertyInfo GetProperty in columns)
                    {
                        SB.Append("<td>" + GetProperty.GetValue(t, null) + "</td>");
                    }
                    SB.Append("<td align='center'><span class=''></span></td></tr>");
                }
                str = str + SB.ToString() + "</tbody></table><script>SortableColumncsTable('" + tablename + "');</script></div></div>";
            }
            str = str + "</div>";
            return button + div + str;
        }
        public void ExcelfromDataTables(DataTable[] dt, string[] sheetname, string filename, bool isRowHighlight)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    for (int i = 0; i < dt.Length; i++)
                    {
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add(sheetname[i]);
                        if (isRowHighlight)
                        {
                            ws.Cells["A1"].LoadFromDataTable(dt[i], true, TableStyles.Medium15); //You can Use TableStyles property of your desire.    
                        }
                        else
                        {
                            ws.Cells["A1"].LoadFromDataTable(dt[i], true);
                        }
                    }
                    //Read the Excel file in a byte array    
                    Byte[] fileBytes = pck.GetAsByteArray();
                    HttpContext.Current.Response.ClearContent();
                    HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=" + filename.retRepname() + ".xlsx");
                    HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    HttpContext.Current.Response.BinaryWrite(fileBytes);
                    HttpContext.Current.Response.End();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
        }
        public string TblUpdt(string tblname, string autono, string dtag, string modcd = "", string SqlCondition = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            if (autono.IndexOf("'") < 0) autono = "'" + autono + "'";
            if (SqlCondition != "") { SqlCondition = " and " + SqlCondition; }
            string scmf = "";
            if (modcd == "") modcd = Module.MODCD;
            switch (modcd)
            {
                case "F":
                    scmf = CommVar.FinSchema(UNQSNO); break;
                case "I":
                    scmf = CommVar.InvSchema(UNQSNO); break;
                case "S":
                    scmf = CommVar.SaleSchema(UNQSNO); break;
                case "P":
                    scmf = CommVar.PaySchema(UNQSNO); break;
            }
            string sql = "";
            if (dtag == "D") sql += "update " + scmf + "." + tblname + " set dtag='" + dtag + "' where autono in (" + autono + ") " + SqlCondition + "~";
            sql += "delete from " + scmf + "." + tblname + " where autono in (" + autono + ") " + SqlCondition;

            return sql;
        }
        //public List<DropDown_list> OTHER_REC_MODE()
        //{
        //    List<DropDown_list> DDL = new List<DropDown_list>();
        //    DropDown_list DDL1 = new DropDown_list();
        //    DDL1.text = "TelePhone";
        //    DDL1.value = "T";
        //    DDL.Add(DDL1);
        //    DropDown_list DDL2 = new DropDown_list();
        //    DDL2.text = "Email";
        //    DDL2.value = "E";
        //    DDL.Add(DDL2);
        //    DropDown_list DDL3 = new DropDown_list();
        //    DDL3.text = "Whatsapp";
        //    DDL3.value = "W";
        //    DDL.Add(DDL3);
        //    DropDown_list DDL4 = new DropDown_list();
        //    DDL4.text = "Online";
        //    DDL4.value = "O";
        //    DDL.Add(DDL4);
        //    return DDL;
        //}
        public List<DropDown_list> OTHER_REC_MODE()
        {
            List<DropDown_list> DDL = new List<DropDown_list>();
            DropDown_list DDL1 = new DropDown_list();
            DDL1.text = "Whatsapp";
            DDL1.value = "W";
            DDL.Add(DDL1);
            DropDown_list DDL2 = new DropDown_list();
            DDL2.text = "Verbal";
            DDL2.value = "V";
            DDL.Add(DDL2);
            DropDown_list DDL3 = new DropDown_list();
            DDL3.text = "Email";
            DDL3.value = "E";
            DDL.Add(DDL3);
            DropDown_list DDL4 = new DropDown_list();
            DDL4.text = "Physical (Static)";
            DDL4.value = "P";
            DDL.Add(DDL4);
            return DDL;
        }
        public List<CashOnDelivery> CashOnDelivery()
        {
            List<CashOnDelivery> DDL = new List<CashOnDelivery>();
            CashOnDelivery DDL1 = new CashOnDelivery();
            DDL1.text = "Yes";
            DDL1.value = "Y";
            DDL.Add(DDL1);
            CashOnDelivery DDL2 = new CashOnDelivery();
            DDL2.text = "No";
            DDL2.value = "N";
            DDL.Add(DDL2);
            return DDL;
        }
        public List<DropDown_list2> STOCK_TYPE()
        {
            List<DropDown_list2> STK_DDL = new List<DropDown_list2>();
            DropDown_list2 STK_DDL0 = new DropDown_list2();
            STK_DDL0.text = "";
            STK_DDL0.value = "F";
            STK_DDL.Add(STK_DDL0);
            DropDown_list2 STK_DDL1 = new DropDown_list2();
            STK_DDL1.text = "Raka";
            STK_DDL1.value = "R";
            STK_DDL.Add(STK_DDL1);
            DropDown_list2 STK_DDL2 = new DropDown_list2();
            STK_DDL2.text = "Loose";
            STK_DDL2.value = "L";
            STK_DDL.Add(STK_DDL2);
            DropDown_list2 STK_DDL3 = new DropDown_list2();
            STK_DDL3.text = "Destroy";
            STK_DDL3.value = "D";
            STK_DDL.Add(STK_DDL3);
            return STK_DDL;
        }
        public List<DropDown_list3> FREE_STOCK()
        {
            List<DropDown_list3> FREE_STK_DDL = new List<DropDown_list3>();
            DropDown_list3 STK_DDL0 = new DropDown_list3();
            STK_DDL0.text = "";
            STK_DDL0.value = "N";
            FREE_STK_DDL.Add(STK_DDL0);
            DropDown_list3 STK_DDL1 = new DropDown_list3();
            STK_DDL1.text = "Yes";
            STK_DDL1.value = "Y";
            FREE_STK_DDL.Add(STK_DDL1);
            return FREE_STK_DDL;
        }
        public string TRNS_BRAND(ImprovarDB DB, string val, string DOC_CD = "")
        {
            using (DB)
            {
                var query = (from c in DB.M_BRAND join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO where i.INACTIVE_TAG == "N" select new { BRANDCD = c.BRANDCD, BRANDNM = c.BRANDNM }).ToList();
                var BRAND_LINK_DATA = (from Z in DB.M_DOC_BRAND where Z.DOCCD == DOC_CD select Z).ToList();
                if (BRAND_LINK_DATA != null && BRAND_LINK_DATA.Count > 0)
                {
                    query = (from X in DB.M_DOC_BRAND
                             join Y in DB.M_BRAND on X.BRANDCD equals Y.BRANDCD into Z
                             from Y in Z.DefaultIfEmpty()
                             where X.BRANDCD == Y.BRANDCD && X.DOCCD == DOC_CD
                             select new { BRANDCD = X.BRANDCD, BRANDNM = Y.BRANDNM }).ToList();
                }
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].BRANDNM + "</td><td>" + query[i].BRANDCD + "</td></tr>");
                    }
                    var hdr = "Brand Name" + Cn.GCS() + "Brand Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.BRANDCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.BRANDCD + Cn.GCS() + i.BRANDNM;
                        }
                        return str;
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
        }
        public string PRICELIST(string val, string Code, string TAG, string BRAND = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            //string SCHEMA = DB.CacheKey.ToString();
            //string SQL = "select a.slcd, a.agslcd, a.agslnm, b.prccd, b.prcnm, b.areacd, b.areanm, b.trslcd, b.trslnm, b.courcd, b.cournm, b.crlimit, b.crdays ";
            //SQL += "from (select a.slcd, nvl(b.agslcd, a.agslcd) agslcd, c.slnm agslnm ";
            //SQL += "from " + SCHEMA + ".m_subleg_com a, " + SCHEMA + ".m_subleg_brand b, " + FSCHEMA + ".m_subleg c ";
            //SQL += "where a.slcd = '" + Code + "' and a.compcd = '" + COMP + "' and a.slcd = b.slcd(+) and a.compcd = b.compcd(+) and ";
            //SQL += "nvl(b.agslcd, a.agslcd) = c.slcd and(b.brandcd = '" + BRAND + "' or b.brandcd = null) ) a, ";
            //SQL += "(select a.slcd, a.prccd, c.prcnm, a.areacd, d.areanm, b.trslcd, e.slnm trslnm, b.courcd, f.slnm cournm, ";
            //SQL += "nvl(a.crlimit, 0) crlimit, nvl(a.crdays, 0) crdays ";
            //SQL += " from " + SCHEMA + ".m_subleg_com a," + SCHEMA + ".m_subleg_sddtl b," + FSCHEMA + ".m_prclst c," + FSCHEMA + ".m_areacd d," + FSCHEMA + ".m_subleg e, ";
            //SQL += "" + FSCHEMA + ".m_subleg f ";
            //SQL += "where a.slcd = '" + Code + "' and a.compcd = '" + COMP + "' and a.slcd = b.slcd(+) and ";
            //SQL += "(a.compcd = b.compcd or b.compcd is null) and(b.loccd = '" + LOC + "' or b.loccd is null) and ";
            //SQL += "a.prccd = c.prccd(+) and a.areacd = d.areacd(+) and(b.trslcd = e.slcd or e.slcd is null) and(b.courcd = f.slcd or f.slcd is null) and ";
            //SQL += "(b.trslcd = e.slcd or e.slcd is null) ) b ";
            //SQL += "where a.slcd = b.slcd(+) ";  

            string scm1 = CommVar.CurSchema(UNQSNO);
            string scmf = CommVar.FinSchema(UNQSNO);
            string COM = CommVar.Compcd(UNQSNO);
            string LOC = CommVar.Loccd(UNQSNO);

            if (BRAND != "") BRAND = "'" + BRAND + "'";

            string sql = "";
            sql += "select a.prccd, a.prcnm, a.discrtcd, a.discrtnm, a.trslcd, a.trslnm, a.courcd, a.cournm, a.taxgrpcd, a.taxgrpnm, ";
            sql += "b.agslcd, c.slnm agslnm, a.areacd, a.areanm, a.docth,a.cod from ";

            sql += "( select a.slcd, a.prccd, b.prcnm, a.discrtcd, c.discrtnm, g.trslcd, d.slnm trslnm, g.courcd, e.slnm cournm, g.taxgrpcd, f.taxgrpnm, ";
            sql += "a.areacd, h.areanm, a.agslcd, a.docth,a.cod ";
            sql += "from " + scm1 + ".m_subleg_com a, " + scmf + ".m_prclst b, " + scmf + ".m_discrt c, " + scmf + ".m_subleg d,";
            sql += "" + scmf + ".m_subleg e, " + scmf + ".m_taxgrp f, " + scm1 + ".m_subleg_sddtl g, " + scmf + ".m_areacd h ";
            sql += "where a.slcd='" + Code + "' and (g.compcd='" + COM + "' or g.compcd is null) and (g.loccd='" + LOC + "' or g.loccd is null) and ";
            sql += "a.prccd=b.prccd(+) and a.discrtcd=c.discrtcd(+) and g.trslcd=d.slcd(+) and g.courcd=e.slcd(+) and g.taxgrpcd=f.taxgrpcd(+) and ";
            sql += "a.slcd=g.slcd(+) and a.areacd=h.areacd(+) ) a, ";

            sql += "( select a.slcd, nvl(a.bagslcd,a.agslcd) agslcd from ";
            sql += "(select a.slcd, a.agslcd, b.agslcd bagslcd ";
            sql += "from " + scm1 + ".m_subleg_com a, " + scm1 + ".m_subleg_brand b ";
            sql += "where a.slcd=b.slcd(+) and (b.brandcd is null ";
            if (BRAND != "") sql += "or b.brandcd in(" + BRAND + ")";
            sql += ") ) a ) b, ";

            sql += scmf + ".m_subleg c ";
            sql += "where a.slcd=b.slcd(+) and b.agslcd=c.slcd(+) ";

            var QUERY_DATA = SQLquery(sql);

            var query = (from DataRow dr in QUERY_DATA.Rows
                         select new
                         {
                             PRCCD = dr["prccd"].ToString(),
                             PRCNM = dr["prcnm"].ToString(),
                             AGSLCD = dr["agslcd"].ToString(),
                             AGSLNM = dr["agslnm"].ToString(),
                             TRSLCD = dr["trslcd"].ToString(),
                             TRSLNM = dr["trslnm"].ToString(),
                             COURCD = dr["courcd"].ToString(),
                             COURNM = dr["cournm"].ToString(),
                             DISCRTCD = dr["discrtcd"].ToString(),
                             DISCRTNM = dr["discrtnm"].ToString(),
                             AREACD = dr["areacd"].ToString(),
                             AREANM = dr["areanm"].ToString(),
                             TAXGRPCD = dr["taxgrpcd"].ToString(),
                             DOCTH = dr["docth"].ToString(),
                             TAXGRPNM = dr["taxgrpnm"].ToString(),
                             COD = dr["cod"].ToString()
                         }).ToList();

            if (val == null && TAG == "")
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].PRCNM + "</td><td>" + query[i].PRCCD + "</td></tr>");
                }
                var hdr = "Price List code" + Cn.GCS() + "Price List Name";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (TAG == "")
                {
                    query = query.Where(a => a.PRCCD == val).ToList();
                }
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        if (TAG == "FORBUYER")
                        {
                            str = i.TAXGRPCD + Cn.GCS() + i.TAXGRPNM + Cn.GCS() + i.COD;
                        }
                        else
                        {
                            str = i.PRCCD + Cn.GCS() + i.PRCNM + Cn.GCS() + i.AGSLCD + Cn.GCS() + i.AGSLNM + Cn.GCS() + i.TRSLCD + Cn.GCS() + i.TRSLNM + Cn.GCS() + i.COURCD + Cn.GCS() + i.COURNM + Cn.GCS() + i.DISCRTCD + Cn.GCS() + i.DISCRTNM + Cn.GCS() + i.AREACD + Cn.GCS() + i.AREANM + Cn.GCS() + i.DOCTH;
                        }
                    }
                    return str;
                }
                else
                {
                    return "Invalid Price List Code ! Please Select / Enter a Valid Price List Code !!";
                }
            }
        }
        public List<DropDown_list1> EFFECTIVE_DATE(string PRC_CD, string DOC_DT)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            List<DropDown_list1> DDL = new List<DropDown_list1>();
            string SCHEMA = CommVar.CurSchema(UNQSNO).ToString();
            string SQL = "select distinct a.effdt ";
            SQL = SQL + "from " + SCHEMA + ".m_itemplist a, " + SCHEMA + ".m_cntrl_hdr b ";
            SQL = SQL + "where a.prccd = '" + PRC_CD + "' and a.m_autono = b.m_autono(+) and ";
            SQL = SQL + "a.effdt <= to_date('" + DOC_DT.Substring(0, 10).Replace('-', '/') + "', 'dd/mm/yyyy') and nvl(b.inactive_tag, 'N')= 'N'";
            var EFFECTIVE_DATE = SQLquery(SQL);
            if (EFFECTIVE_DATE != null)
            {
                DDL = (from DataRow DR in EFFECTIVE_DATE.Rows
                       orderby DR["effdt"].ToString() descending
                       select new DropDown_list1
                       {
                           value = DR["effdt"].ToString().Substring(0, 10).Replace("-", "/"),
                           text = DR["effdt"].ToString().Substring(0, 10).Replace("-", "/")
                       }).ToList();
            }
            return DDL;
        }
        public List<DropDown_list4> DISC_EFFECTIVE_DATE(string DISC_CD, string DOC_DT)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            List<DropDown_list4> DDL = new List<DropDown_list4>();
            string SCHEMA = CommVar.CurSchema(UNQSNO).ToString();
            string SQL = "select distinct a.effdt ";
            SQL = SQL + "from " + SCHEMA + ".m_discrthdr a, " + SCHEMA + ".m_cntrl_hdr b ";
            SQL = SQL + "where a.DISCRTCD = '" + DISC_CD + "' and a.m_autono = b.m_autono(+) and ";
            SQL = SQL + "a.effdt <= to_date('" + DOC_DT.Substring(0, 10).Replace('-', '/') + "', 'dd/mm/yyyy') and nvl(b.inactive_tag, 'N')= 'N'";
            var DIS_EFFECTIVE_DATE = SQLquery(SQL);
            if (DIS_EFFECTIVE_DATE != null)
            {
                DDL = (from DataRow DR in DIS_EFFECTIVE_DATE.Rows
                       orderby DR["effdt"].ToString() descending
                       select new DropDown_list4
                       {
                           value = DR["effdt"].ToString().Substring(0, 10).Replace("-", "/"),
                           text = DR["effdt"].ToString().Substring(0, 10).Replace("-", "/")
                       }).ToList();
            }
            return DDL;
        }
        public string DOCNO_ORDER_help(string DOCCD, string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                {
                    var query = (from c in DB.T_SORD
                                 join i in DB.T_CNTRL_HDR on c.AUTONO equals i.AUTONO
                                 where i.CANCEL != "Y" && i.DOCCD == DOCCD
                                 select new { c.DOCNO, c.DOCDT, c.SLCD, i.AUTONO }).ToList().Select(s => new
                                 {
                                     DOCNO = s.DOCNO,
                                     DOCDT = s.DOCDT,
                                     SLCD = s.SLCD,
                                     SLNM = (from P in DBF.M_SUBLEG where P.SLCD == s.SLCD select P.SLNM).SingleOrDefault(),
                                     AUTONO = s.AUTONO
                                 }).ToList();
                    if (val == null)
                    {
                        System.Text.StringBuilder SB = new System.Text.StringBuilder();
                        for (int i = 0; i <= query.Count - 1; i++)
                        {
                            SB.Append("<tr><td>" + query[i].DOCNO + "</td><td>" + query[i].DOCDT.ToString().retDateStr() + "</td><td>" + query[i].SLCD + "</td><td>" + query[i].SLNM + "</td><td>" + query[i].AUTONO + "</td></tr>");
                        }
                        var hdr = "Doc No" + Cn.GCS() + "Doc Dt" + Cn.GCS() + "Party Code" + Cn.GCS() + "Party Name" + Cn.GCS() + "Autono";
                        return Generate_help(hdr, SB.ToString(), "4");
                    }
                    else
                    {
                        query = query.Where(a => a.DOCNO == val).ToList();
                        if (query.Any())
                        {
                            string str = "";
                            foreach (var i in query)
                            {
                                str = ToReturnFieldValues(query);
                            }
                            return str;
                        }
                        else
                        {
                            return "Invalid Order No. ! Please Select / Enter a Valid Order No. !!";
                        }
                    }
                }
            }
        }
        //public string PAY_TERMS(ImprovarDB DB, string val)
        //{
        //    using (DB)
        //    {
        //        var query = (from c in DB.M_PAYTRMS
        //                     select new
        //                     {
        //                         PAYTRMCD = c.PAYTRMCD,
        //                         PAYTRMNM = c.PAYTRMNM
        //                     }).ToList();
        //        if (val == null)
        //        {
        //            System.Text.StringBuilder SB = new System.Text.StringBuilder();
        //            for (int i = 0; i <= query.Count - 1; i++)
        //            {
        //                SB.Append("<tr><td>" + query[i].PAYTRMNM + "</td><td>" + query[i].PAYTRMCD + "</td></tr>");
        //            }
        //            var hdr = "Pay Term Name" + Cn.GCS() + "Pay Term Code";
        //            return Generate_help(hdr, SB.ToString());
        //        }
        //        else
        //        {
        //            query = query.Where(a => a.PAYTRMCD == val).ToList();
        //            if (query.Any())
        //            {
        //                string str = "";
        //                foreach (var i in query)
        //                {
        //                    str = i.PAYTRMCD + Cn.GCS() + i.PAYTRMNM;
        //                }
        //                return str;
        //            }
        //            else
        //            {
        //                return "0";
        //            }
        //        }
        //    }
        //}
        public string RetriveParkFromFile(string value, string Path, string UserID, string ViewClassName)
        {
            try
            {
                int op = 0;
                INI inifile = new INI();
                string text = System.IO.File.ReadAllText(Path);
                string[] keys = inifile.GetEntryNames(UserID, Path);
                string[] SectionName = inifile.GetSectionNames(Path);
                int Sindex = Array.IndexOf(SectionName, UserID);
                string NextSecnm = "";
                if (SectionName.Length - 1 > Sindex)
                {
                    NextSecnm = SectionName[Sindex + 1];
                    int SSindex = text.IndexOf("[" + UserID + "]");
                    int ESindex = text.IndexOf("[" + NextSecnm + "]");
                    text = text.Substring(SSindex, ESindex - SSindex);
                }
                else
                {
                    int SSindex = text.IndexOf("[" + UserID + "]");
                    text = text.Substring(SSindex);
                }
                int afterIndex = Array.IndexOf(keys, value);
                string nextKeys = "";
                if (keys.Length - 1 > afterIndex)
                {
                    nextKeys = keys[afterIndex + 1];
                }
                int start = text.IndexOf(value);
                int end = nextKeys.Length == 0 ? 0 : text.IndexOf(nextKeys);
                int lg = text.Length;
                string stream = end == 0 ? text.Substring(start + value.Length + 1) : text.Substring(start + value.Length + 1, end - start - 1 - nextKeys.Length);
                stream = Cn.Decrypt(stream);
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                Type typeclass = Type.GetType(ViewClassName);
                var helpM1 = Activator.CreateInstance(typeclass);
                helpM1 = javaScriptSerializer.Deserialize(stream, typeclass);
                //Cn.getQueryString(helpM1);               
                if (HttpContext.Current.Session[value] != null)
                {
                    HttpContext.Current.Session.Remove(value);
                }
                HttpContext.Current.Session.Add(value, helpM1);
                string url = "";
                var PreviousUrl = System.Web.HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
                var uri = new Uri(PreviousUrl);//Create Virtually Query String
                var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                if (queryString.Get("parkID") == null)
                {
                    url = HttpContext.Current.Request.UrlReferrer.ToString() + "&parkID=" + value;
                }
                else
                {
                    string dd = HttpContext.Current.Request.UrlReferrer.ToString();
                    int pos = HttpContext.Current.Request.UrlReferrer.ToString().IndexOf("&parkID=");
                    url = dd.Substring(0, pos);
                    url = url + "&parkID=" + value;
                }
                return url;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<SizeLink> SIZE_LINK()
        {
            List<SizeLink> SLNK = new List<SizeLink>();
            SizeLink SLNK1 = new SizeLink();
            SLNK1.text = "Yes";
            SLNK1.value = "Y";
            SLNK.Add(SLNK1);
            SizeLink SLNK2 = new SizeLink();
            SLNK2.text = "No";
            SLNK2.value = "N";
            SLNK.Add(SLNK2);
            return SLNK;
        }

        public string MACHINE_HELP(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var query = (from c in DB.M_MACHINE
                             join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                             where i.INACTIVE_TAG == "N"
                             select new
                             {
                                 MCCD = c.MCCD,
                                 MCNM = c.MCNM
                             }).ToList();
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].MCNM + "</td><td>" + query[i].MCCD + "</td></tr>");
                    }
                    var hdr = "Machine Name" + Cn.GCS() + "Machine Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.MCCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.MCCD + Cn.GCS() + i.MCNM;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Machine Code ! Please Select / Enter a Valid Machine Code !!";
                    }
                }
            }
        }
        public string SUBJOB(ImprovarDB DB, string VAL, string VAL1)
        {
            using (DB)
            {
                var tbldatadoc = (from c in DB.M_JOBMSTSUB
                                  join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                                  join j in DB.M_JOBMST on c.JOBCD equals j.JOBCD
                                  where i.INACTIVE_TAG == "N" && c.JOBCD == j.JOBCD
                                  select new
                                  {
                                      M_AUTONO = c.M_AUTONO,
                                      SJOBSTYLE = c.SJOBSTYLE,
                                      SCATE = c.SCATE,
                                      SJOBCD = c.SJOBCD,
                                      SJOBNM = c.SJOBNM,
                                      JOBNM = j.JOBNM,
                                      SJOBMCHN = c.SJOBMCHN,
                                      SJOBSIZE = c.SJOBSIZE
                                  }).ToList();

                //List<TEMP_helpimg> TEMP_helpimg1 = new List<TEMP_helpimg>();
                //foreach (var i in tbldatadoc)
                //{
                //    TEMP_helpimg UPL = new TEMP_helpimg();

                //    var image = (from h in DB.M_CNTRL_HDR_DOC_DTL
                //                 where h.M_AUTONO == i.M_AUTONO && h.SLNO == 1
                //                 select h).OrderBy(d => d.RSLNO).ToList();

                //    var hh = image.GroupBy(x => x.M_AUTONO).Select(x => new
                //    {
                //        namefl = string.Join("", x.Select(n => n.DOC_STRING))
                //    });
                //    foreach (var ii in hh)
                //    {
                //        UPL.DOC_FILE = ii.namefl;
                //    }
                //    UPL.M_AUTONO = i.M_AUTONO;
                //    TEMP_helpimg1.Add(UPL);
                //}
                var query = (from c in tbldatadoc

                             select new { M_AUTONO = c.M_AUTONO, SJOBSTYLE = c.SJOBSTYLE, SCATE = c.SCATE, SJOBCD = c.SJOBCD, SJOBNM = c.SJOBNM, JOBNM = c.JOBNM, SJOBMCHN = c.SJOBMCHN, SJOBSIZE = c.SJOBSIZE }).ToList();

                if (VAL != "" && VAL1 == "")
                {
                    query = (from c in tbldatadoc
                             where c.SJOBSTYLE == VAL
                             select new { M_AUTONO = c.M_AUTONO, SJOBSTYLE = c.SJOBSTYLE, SCATE = c.SCATE, SJOBCD = c.SJOBCD, SJOBNM = c.SJOBNM, JOBNM = c.JOBNM, SJOBMCHN = c.SJOBMCHN, SJOBSIZE = c.SJOBSIZE }).ToList();
                }
                else if (VAL == "" && VAL1 != "")
                {
                    query = (from c in tbldatadoc
                             where c.SCATE == VAL1
                             select new { M_AUTONO = c.M_AUTONO, SJOBSTYLE = c.SJOBSTYLE, SCATE = c.SCATE, SJOBCD = c.SJOBCD, SJOBNM = c.SJOBNM, JOBNM = c.JOBNM, SJOBMCHN = c.SJOBMCHN, SJOBSIZE = c.SJOBSIZE }).ToList();
                }
                else if (VAL != "" && VAL1 != "")
                {
                    query = (from c in tbldatadoc
                             where c.SCATE == VAL1
                             select new { M_AUTONO = c.M_AUTONO, SJOBSTYLE = c.SJOBSTYLE, SCATE = c.SCATE, SJOBCD = c.SJOBCD, SJOBNM = c.SJOBNM, JOBNM = c.JOBNM, SJOBMCHN = c.SJOBMCHN, SJOBSIZE = c.SJOBSIZE }).ToList();
                }
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr ><td>" + query[i].SJOBSTYLE + "</td><td>" + query[i].SCATE + "</td><td>" + query[i].SJOBNM + "</td><td>" + query[i].SJOBCD + "</td><td>" + query[i].JOBNM + "</td><td>" + query[i].SJOBMCHN + "</td><td>" + query[i].SJOBSIZE + "</td></tr>");
                }
                var hdr = "Style Name" + Cn.GCS() + "Category Name" + Cn.GCS() + "Sub Job Name" + Cn.GCS() + "Sub Job Code" + Cn.GCS() + "Job Name" + Cn.GCS() + "Sub Job Machine" + Cn.GCS() + "Sub Job Size";
                return Generate_help(hdr, SB.ToString());
            }
        }
        public string JOBCD_help(ImprovarDB DB)
        {
            using (DB)
            {
                var query = (from c in DB.M_JOBMST
                             join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                             where i.INACTIVE_TAG == "N"
                             select new
                             {
                                 Code = c.JOBCD,
                                 Description = c.JOBNM
                             }).ToList();
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
                }
                var hdr = "Job Description" + Cn.GCS() + "Job Code";
                return Generate_help(hdr, SB.ToString());
            }
        }
        public string PREFJOBBER(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                var query = (from c in DB.M_SUBLEG join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO where i.INACTIVE_TAG == "N" select new { SLCD = c.SLCD, SLNM = c.SLNM }).ToList();

                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].SLNM + "</td><td>" + query[i].SLCD + "</td></tr>");
                    }
                    var hdr = "Preferred Jobber / Supplier Name" + Cn.GCS() + "Preferred Jobber / Supplier Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.SLCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.SLCD + Cn.GCS() + i.SLNM;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Preferred Jobber / Supplier Code ! Please Enter a Valid Preferred Jobber / Supplier Code !!";
                    }
                }
            }
        }
        public string STYLE(ImprovarDB DB)
        {
            using (DB)
            {
                var query = (from c in DB.M_JOBMSTSUB
                             join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                             where i.INACTIVE_TAG == "N"
                             select new
                             {
                                 SJOBSTYLE = c.SJOBSTYLE
                             }).DistinctBy(a => a.SJOBSTYLE).ToList();

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].SJOBSTYLE + "</td></tr>");
                }
                var hdr = "Style Name";
                return Generate_help(hdr, SB.ToString());
            }
        }
        public string CATEGORY(ImprovarDB DB, string VAL)
        {
            using (DB)
            {
                var query = (from c in DB.M_JOBMSTSUB
                             join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                             where i.INACTIVE_TAG == "N" && c.SJOBSTYLE == VAL
                             select new
                             {
                                 SCATE = c.SCATE
                             }).DistinctBy(a => a.SCATE).ToList();

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr ><td>" + query[i].SCATE + "</td></tr>");
                }
                var hdr = "Category Name";
                return Generate_help(hdr, SB.ToString());
            }
        }
        public string RTDEBCD_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                var query = (from c in DB.M_RETDEB join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO where i.INACTIVE_TAG == "N" select new { RTDEBCD = c.RTDEBCD, RTDEBNM = c.RTDEBNM }).ToList();
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].RTDEBNM + "</td><td>" + query[i].RTDEBCD + "</td></tr>");
                    }
                    var hdr = "Ref Retail Name" + Cn.GCS() + "Ref Retail Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.RTDEBCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.RTDEBCD + Cn.GCS() + i.RTDEBNM;
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Ref Retail Code ! Please Select / Enter a Valid Ref Retail Code !!";
                    }
                }
            }
        }
        public string ITGRPCD_FABITCD_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {
                string scm1 = CommVar.CurSchema(UNQSNO);
                string valsrch = val.ToUpper().Trim();
                string sql = "";

                sql += "select distinct b.itgrpcd, b.itgrpnm, a.fabitcd, c.itnm fabitnm, b.grpnm ";
                sql += "from " + scm1 + ".m_sitem a, " + scm1 + ".m_group b, " + scm1 + ".m_sitem c ";
                sql += "where a.itgrpcd = b.itgrpcd(+) and a.fabitcd = c.itcd(+) ";
                if (valsrch.retStr() != "") sql += "and ( upper(b.itgrpcd) like '%" + valsrch + "%' or upper(b.itgrpnm) like '%" + valsrch + "%' or upper(a.fabitcd) like '%" + valsrch + "%' or upper(c.itnm) like '%" + valsrch + "%'  )  ";
                sql += "order by grpnm, itgrpnm, fabitnm ";


                DataTable rsTmp = SQLquery(sql);

                if (val.retStr() == "" || rsTmp.Rows.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= rsTmp.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + rsTmp.Rows[i]["itgrpnm"] + "</td><td>" + rsTmp.Rows[i]["itgrpcd"] + "</td><td>" + rsTmp.Rows[i]["fabitnm"] + "</td><td>" + rsTmp.Rows[i]["fabitcd"] + "</td><td>" + rsTmp.Rows[i]["grpnm"] + "</td></tr>");
                    }
                    var hdr = "Item Group Name" + Cn.GCS() + "Item Group Code" + Cn.GCS() + "Fabric Item Name" + Cn.GCS() + "Fabric Item Code" + Cn.GCS() + "Group Name";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    string str = "";
                    if (rsTmp.Rows.Count > 0)
                    {
                        str = ToReturnFieldValues("", rsTmp);
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
        public string JOBPRCCD_help(ImprovarDB DB)
        {
            using (DB)
            {
                var query = (from c in DB.M_JOBPRCCD
                             join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                             where i.INACTIVE_TAG == "N"
                             select new
                             {
                                 Code = c.JOBPRCCD,
                                 Description = c.JOBPRCNM
                             }).ToList();
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
                }
                var hdr = "Job Price Name" + Cn.GCS() + "Job Price Code";
                return Generate_help(hdr, SB.ToString());
            }
        }
        public string FLOOR(ImprovarDB DB)
        {
            var query = (from c in DB.M_FLRLOCA
                         join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                         where i.INACTIVE_TAG == "N"
                         select new
                         {
                             FLRCD = c.FLRCD,
                             FLRNM = c.FLRNM
                         }).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr><td>" + query[i].FLRNM + "</td><td>" + query[i].FLRCD + "</td></tr>");
            }
            var hdr = "Floor Name" + Cn.GCS() + "Floor Code";
            return Generate_help(hdr, SB.ToString());
        }
        public string GOCD_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            string valsrch = val.ToUpper().Trim();

            sql = "";
            sql += "select a.GOCD,a.GONM ";
            sql += "from " + scm + ".M_GODOWN a, " + scm + ".M_CNTRL_HDR b ";
            sql += "where a.M_AUTONO=b.M_AUTONO(+) and b.INACTIVE_TAG = 'N' ";
            if (valsrch.retStr() != "") sql += "and ( upper(a.GOCD) = '" + valsrch + "' ) ";
            sql += "order by a.GOCD,a.GONM";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["GONM"] + "</td><td>" + tbl.Rows[i]["GOCD"] + " </td></tr>");
                }
                var hdr = "Godown Name" + Cn.GCS() + "Godown Code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Godown Code ! Please Select / Enter a Valid Godown Code !!";
                }
            }
        }
        public string PRCCD_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.FinSchema(UNQSNO);
            string sql = "";
            string valsrch = val.ToUpper().Trim();

            sql = "";
            sql += "select a.PRCCD,a.PRCNM ";
            sql += "from " + scm + ".M_PRCLST a, " + scm + ".M_CNTRL_HDR b ";
            sql += "where a.M_AUTONO=b.M_AUTONO(+) and b.INACTIVE_TAG = 'N' ";
            if (valsrch.retStr() != "") sql += "and ( upper(a.PRCCD) like '%" + valsrch + "%' or upper(a.PRCNM) like '%" + valsrch + "%' ) ";
            sql += "order by a.PRCCD,a.PRCNM";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["PRCNM"] + "</td><td>" + tbl.Rows[i]["PRCCD"] + " </td></tr>");
                }
                var hdr = "Price Name" + Cn.GCS() + "Price code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Price Code ! Please Enter a Valid Price Code !!";
                }
            }
        }
        public string MTRLJOBCD_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            string valsrch = val.ToUpper().Trim();

            sql = "";
            sql += "select a.MTRLJOBCD,a.MTRLJOBNM,a.MTBARCODE ";
            sql += "from " + scm + ".M_MTRLJOBMST a, " + scm + ".M_CNTRL_HDR b ";
            sql += "where a.M_AUTONO=b.M_AUTONO(+) and b.INACTIVE_TAG = 'N' ";
            if (valsrch.retStr() != "") sql += "and ( upper(a.MTRLJOBCD) like '%" + valsrch + "%' or upper(a.MTRLJOBNM) like '%" + valsrch + "%' ) ";
            sql += "order by a.MTRLJOBCD,a.MTRLJOBNM";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["MTRLJOBNM"] + "</td><td>" + tbl.Rows[i]["MTRLJOBCD"] + " </td></tr>");
                }
                var hdr = "Material Job Name" + Cn.GCS() + "Material Job code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Material Job Code ! Please Enter a Valid Material Job Code !!";
                }
            }
        }
        public string PARTCD_help(string val, string itcd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            string valsrch = val.retStr().ToUpper().Trim();
            sql = "";
            sql += "select a.PARTCD,a.PARTNM,a.PRTBARCODE ";
            sql += "from " + scm + ".M_PARTS a, " + scm + ".M_CNTRL_HDR b ";
            if (itcd.retStr() != "") sql += ", " + scm + ".M_SITEM_PARTS c ";
            sql += "where a.M_AUTONO=b.M_AUTONO(+) and b.INACTIVE_TAG = 'N' ";
            if (itcd.retStr() != "") sql += "and  a.PARTCD = c.PARTCD(+) and c.itcd = '" + itcd + "' ";
            if (valsrch.retStr() != "") sql += "and ( upper(a.PARTCD) like '%" + valsrch + "%' or upper(a.PARTNM) like '%" + valsrch + "%' ) ";
            sql += "order by a.PARTCD,a.PARTNM";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["PARTNM"] + "</td><td>" + tbl.Rows[i]["PARTCD"] + " </td></tr>");
                }
                var hdr = "Part Name" + Cn.GCS() + "Part code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Part Code ! Please Enter a Valid Part Code !!";
                }
            }
        }
        public string COLRCD_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            string valsrch = val.ToUpper().Trim();

            sql = "";
            sql += "select a.COLRCD,a.COLRNM,a.CLRBARCODE ";
            sql += "from " + scm + ".M_COLOR a, " + scm + ".M_CNTRL_HDR b ";
            sql += "where a.M_AUTONO=b.M_AUTONO(+) and b.INACTIVE_TAG = 'N' ";
            if (valsrch.retStr() != "") sql += "and ( upper(a.COLRCD) = '" + valsrch + "' ) ";
            sql += "order by a.COLRCD,a.COLRNM";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["COLRNM"] + "</td><td>" + tbl.Rows[i]["COLRCD"] + " </td><td>" + tbl.Rows[i]["CLRBARCODE"] + " </td></tr>");
                }
                var hdr = "Color Name" + Cn.GCS() + "Color code" + Cn.GCS() + "Color Bar Code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Color Code ! Please Enter a Valid Color Code !!";
                }
            }
        }
        public string SIZECD_help(string val, string itcd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            string valsrch = val.ToUpper().Trim();

            sql = "";
            sql += "select a.SIZECD,a.SIZENM,a.SZBARCODE ";
            sql += "from " + scm + ".M_SIZE a, " + scm + ".M_CNTRL_HDR b ";
            if (itcd.retStr() != "") sql += ", " + scm + ".M_SITEM_SIZE c ";
            sql += "where a.M_AUTONO=b.M_AUTONO(+) and b.INACTIVE_TAG = 'N' ";
            if (itcd.retStr() != "") sql += "and  a.SIZECD = c.SIZECD(+) and c.itcd = '" + itcd + "' ";
            if (valsrch.retStr() != "") sql += "and  upper(a.SIZECD) = '" + valsrch + "'  ";
            sql += "order by a.SIZECD,a.SIZENM";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["SIZENM"] + "</td><td>" + tbl.Rows[i]["SIZECD"] + " </td><td>" + tbl.Rows[i]["SZBARCODE"] + " </td></tr>");
                }
                var hdr = "Size Name" + Cn.GCS() + "Size code" + Cn.GCS() + "Size Bar Code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Size Code ! Please Enter a Valid Size Code !!";
                }
            }
        }
        public string STKTYPE_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            string valsrch = val.ToUpper().Trim();

            sql = "";
            sql += "select a.STKTYPE,a.STKNAME ";
            sql += "from " + scm + ".M_STKTYPE a, " + scm + ".M_CNTRL_HDR b ";
            sql += "where a.M_AUTONO=b.M_AUTONO(+) and b.INACTIVE_TAG = 'N' ";
            if (valsrch.retStr() != "") sql += "and ( upper(a.STKTYPE) = '" + valsrch + "' ) ";
            sql += "order by a.STKTYPE,a.STKNAME";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["STKNAME"] + "</td><td>" + tbl.Rows[i]["STKTYPE"] + " </td></tr>");
                }
                var hdr = "Stock Type Name" + Cn.GCS() + "Stock Type code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Stock Type ! Please Enter a Valid Stock Type !!";
                }
            }
        }
        public string LOCABIN_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            string valsrch = val.ToUpper().Trim();

            sql = "";
            sql += "select a.LOCABIN,a.GOCD,c.GONM ";
            sql += "from " + scm + ".M_LOCABIN a, " + scm + ".M_CNTRL_HDR b, " + scm + ".M_GODOWN c ";
            sql += "where a.M_AUTONO=b.M_AUTONO(+) and a.GOCD=c.GOCD(+) and b.INACTIVE_TAG = 'N' ";
            if (valsrch.retStr() != "") sql += "and ( upper(a.LOCABIN) like '%" + valsrch + "%' or upper(a.GOCD) like '%" + valsrch + "%' ) ";
            sql += "order by a.LOCABIN,a.GOCD";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["GONM"] + "</td><td>" + tbl.Rows[i]["GOCD"] + "</td><td>" + tbl.Rows[i]["LOCABIN"] + " </td></tr>");
                }
                var hdr = "Godown Name" + Cn.GCS() + "Godown Code" + Cn.GCS() + "Location Bin";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Stock Type ! Please Enter a Valid Stock Type !!";
                }
            }
        }
        public List<BARGEN_TYPE> BARGEN_TYPE()
        {
            List<BARGEN_TYPE> DDL = new List<BARGEN_TYPE>();
            BARGEN_TYPE DDL1 = new BARGEN_TYPE();
            DDL1.Text = "Common";
            DDL1.Value = "C";
            DDL.Add(DDL1);
            BARGEN_TYPE DDL2 = new BARGEN_TYPE();
            DDL2.Text = "Entry";
            DDL2.Value = "E";
            DDL.Add(DDL2);
            return DDL;
        }
        public string CLASS1(string val)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                string scmf = CommVar.FinSchema(UNQSNO);
                string valsrch = val.ToUpper().Trim();
                //var query = (from c in DB.M_CLASS1 select new { CLASS1CD = c.CLASS1CD, CLASS1NM = c.CLASS1NM }).ToList();
                string sql = "";
                sql += "select CLASS1CD,CLASS1NM from " + scmf + ".M_CLASS1 ";
                if (valsrch.retStr() != "") sql += "where upper(CLASS1CD) = '" + valsrch + "'  ";
                DataTable tbl = SQLquery(sql);
                if (val.retStr() == "" || tbl.Rows.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + tbl.Rows[i]["CLASS1NM"] + "</td><td>" + tbl.Rows[i]["CLASS1CD"] + "</td></tr>");
                    }
                    var hdr = "Class 1 Name" + Cn.GCS() + "Class 1 Code";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    string str = "";
                    if (tbl.Rows.Count > 0)
                    {
                        str = ToReturnFieldValues("", tbl);
                    }
                    else
                    {
                        str = "Invalid Class 1 Code ! Please Select / Enter a Valid Class 1 Code !!";
                    }
                    return str;
                }
            }
            catch (Exception ex)
            {
                return ex.Message + " " + ex.InnerException;
            }

        }
        public List<DISC_TYPE> DISC_TYPE()
        {
            List<DISC_TYPE> DTYP = new List<DISC_TYPE>();
            DISC_TYPE DTYP3 = new DISC_TYPE();
            DTYP3.Text = "%";
            DTYP3.Value = "P";
            DTYP.Add(DTYP3);
            DISC_TYPE DTYP2 = new DISC_TYPE();
            DTYP2.Text = "Nos";
            DTYP2.Value = "N";
            DTYP.Add(DTYP2);
            DISC_TYPE DTYP1 = new DISC_TYPE();
            DTYP1.Text = "Qnty";
            DTYP1.Value = "Q";
            DTYP.Add(DTYP1);
            DISC_TYPE DTYP4 = new DISC_TYPE();
            DTYP4.Text = "Fixed";
            DTYP4.Value = "F";
            DTYP.Add(DTYP4);
            return DTYP;
        }
        public List<BL_TYPE> BL_TYPE()
        {
            string scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            sql += "select BLTYPE from " + scm + ".M_BLTYPE ";
            DataTable dt = SQLquery(sql);
            List<BL_TYPE> BL_TYPE_list = new List<BL_TYPE>();
            BL_TYPE_list = (from DataRow dr in dt.Rows
                            select new BL_TYPE() { Text = dr["BLTYPE"].retStr(), Value = dr["BLTYPE"].retStr() }).OrderBy(s => s.Text).Distinct().ToList();
            return BL_TYPE_list;
        }
        public List<DropDown_list_StkType> STK_TYPE()
        {
            string scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            sql += "select STKTYPE,decode(STKTYPE,'F','',STKNAME)STKNAME from " + scm + ".M_stktype ORDER BY case when STKTYPE = 'F' then 1 else 2  end  ";
            DataTable dt = SQLquery(sql);
            List<DropDown_list_StkType> DropDown_list_StkType = new List<DropDown_list_StkType>();
            DropDown_list_StkType = (from DataRow dr in dt.Rows
                                     select new DropDown_list_StkType() { text = dr["STKNAME"].retStr(), value = dr["STKTYPE"].retStr() }).OrderBy(s => s.text).Distinct().ToList();
            return DropDown_list_StkType;
        }
        public string BARCODE_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            string valsrch = val.ToUpper().Trim();

            sql = "";
            sql += "select c.BARNO,a.ITGRPCD,d.ITGRPNM,a.ITCD,a.ITNM,a.STYLENO,c.COLRCD,e.COLRNM,e.CLRBARCODE,c.SIZECD,f.SIZENM,f.SZBARCODE,a.UOMCD,NVL(D.HSNCODE,a.HSNCODE) HSNCODE,a.itnm||' '||a.styleno itstyle ";
            sql += "from " + scm + ".M_SITEM a, " + scm + ".M_CNTRL_HDR b, " + scm + ".M_SITEM_BARCODE c, " + scm + ".M_GROUP d, " + scm + ".M_COLOR e, " + scm + ".M_SIZE f ";
            sql += "where a.M_AUTONO=b.M_AUTONO(+) and b.INACTIVE_TAG = 'N'and a.ITCD=c.ITCD(+) and a.ITGRPCD=d.ITGRPCD(+) and c.COLRCD=e.COLRCD(+) and c.SIZECD=f.SIZECD(+) ";
            if (valsrch.retStr() != "") sql += "and ( upper(c.BARNO) = '" + valsrch + "' ) ";
            sql += "order by c.BARNO";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["BARNO"] + "</td><td>" + tbl.Rows[i]["ITNM"] + " </td><td>" + tbl.Rows[i]["ITCD"] + " </td><td>" + tbl.Rows[i]["STYLENO"] + " </td></tr>");
                }
                var hdr = "Bar Code" + Cn.GCS() + "Item Name" + Cn.GCS() + "Item code" + Cn.GCS() + "Design No.";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Bar Code ! Please Enter a Valid Bar Code !!";
                }
            }
        }
        public string INVENTORY_ITEM(string BH_CD = "", string SH_CD = "", string val = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var query = (from c in DB.M_SITEM
                             join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                             where i.INACTIVE_TAG == "N"
                             select new { ITCD = c.ITCD, ITDESCN = c.ITNM, UOM = c.UOMCD }).ToList();

                //if (query != null && query.Count > 0 && BH_CD != "") { query = query.Where(a => a.BHCD == BH_CD).ToList(); }
                //if (query != null && query.Count > 0 && SH_CD != "") { query = query.Where(a => a.SHCD == SH_CD).ToList(); }

                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].ITDESCN + "</td><td>" + query[i].ITCD + "</td><td>" + query[i].UOM + "</td></tr>");
                    }
                    var hdr = "Item Description" + Cn.GCS() + "Item Code" + Cn.GCS() + "UOM";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.ITCD == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.ITCD + Cn.GCS() + i.ITDESCN + Cn.GCS() + i.UOM;
                        }
                        return str;
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
        }
        public string DOCNO_T_TXN_help(ImprovarDB DB, string doccd)
        {
            var query = (from c in DB.T_TXN
                         orderby c.DOCNO, c.DOCDT
                         where c.DOCCD == doccd
                         select new
                         {
                             DOCNO = c.DOCNO,
                             DOCDT = c.DOCDT
                         }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr><td>" + query[i].DOCDT + "</td><td>" + query[i].DOCNO + "</td></tr>");
            }
            var hdr = "Doc Date" + Cn.GCS() + "Doc No";
            return Generate_help(hdr, SB.ToString());
        }
        public string T_TXN_BARNO_help(string val, string menupara, string DOCDT, string TAXGRPCD, string GOCD, string PRCCD, string MTRLJOBCD)
        {
            DataTable tbl = new DataTable();
            if (menupara == "PB")
            {
                val = val.retStr() == "" ? "" : val.retStr().retSqlformat();
                tbl = salesfunc.GetBarHelp(DOCDT.retStr(), GOCD.retStr(), val, "", MTRLJOBCD.retStr(), "", "", "", PRCCD.retStr(), TAXGRPCD.retStr());

            }
            else
            {
                tbl = salesfunc.GetStock(DOCDT.retStr(), GOCD.retStr(), val, "", MTRLJOBCD.retStr(), "", "", "", PRCCD.retStr(), TAXGRPCD.retStr());

            }


            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["BARNO"] + "</td><td>" + tbl.Rows[i]["ITNM"] + " </td><td>" + tbl.Rows[i]["ITCD"] + " </td><td>" + tbl.Rows[i]["STYLENO"] + " </td></tr>");
                }
                var hdr = "Bar Code" + Cn.GCS() + "Item Name" + Cn.GCS() + "Item code" + Cn.GCS() + "Design No.";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Bar Code ! Please Enter a Valid Bar Code !!";
                }
            }
        }
        public string PACKSLIPAUTONO_help(string val, string autono, string docdt, string slcd)
        {

            DataTable tbl = salesfunc.getPendingPackslip(docdt, slcd);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["docno"] + "</td><td>" + tbl.Rows[i]["docdt"].retStr().Remove(10) + " </td><td><b>" + tbl.Rows[i]["slnm"] + "</b> [" + tbl.Rows[i]["district"] + "] (" + tbl.Rows[i]["slcd"] + ") </td><td>" + tbl.Rows[i]["autono"] + " </td></tr>");
                }
                var hdr = "Packing Slip No." + Cn.GCS() + "Packing Slip Date" + Cn.GCS() + "Party" + Cn.GCS() + "Autono";
                return Generate_help(hdr, SB.ToString(), "3");
            }
            else
            {
                string str1 = "";
                if (autono.retStr() != "") str1 = "and autono = '" + autono + "' ";
                tbl = tbl.Select("docno = '" + val + "'" + str1).CopyToDataTable();
                if (tbl.Rows.Count > 0)
                {
                    string str = ToReturnFieldValues("", tbl);
                    return str;
                }
                else
                {
                    return "Invalid Packing Slip No. ! Please Enter a Valid Packing Slip No. !!";
                }
            }
        }
        public string DOCNO_help(string val, string doccd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var query = (from c in DB.T_CNTRL_HDR where c.DOCCD == doccd orderby c.DOCDT, c.DOCONLYNO select new { DOCONLYNO = c.DOCONLYNO, DOCDT = c.DOCDT }).ToList();

                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].DOCONLYNO + "</td><td>" + query[i].DOCDT.ToString().Substring(0, 10) + "</td></tr>");
                    }
                    var hdr = "Document Number" + Cn.GCS() + "Document Date";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    query = query.Where(a => a.DOCONLYNO == val).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = ToReturnFieldValues(query);
                        }
                        return str;
                    }
                    else
                    {
                        return "Invalid Document Number ! Please Select / Enter a Valid Document Number !!";
                    }
                }
            }
        }
        public List<INVTYPE_list> INVTYPE_list()
        {
            List<INVTYPE_list> INVTYPE_list = new List<INVTYPE_list> {
                new INVTYPE_list { Value = "01", Text = "Regular" },
                new INVTYPE_list { Value = "02", Text = "SEZ supplies with payment" },
                new INVTYPE_list { Value = "03", Text = "SEZ supplies without payment" },
                new INVTYPE_list { Value = "04", Text = "Deemed Exp" },
            };
            return (INVTYPE_list);
        }
        public List<EXPCD_list> EXPCD_list(string salpur = "S")
        {
            if (salpur == "P")
            {
                List<EXPCD_list> EXPCD_list = new List<EXPCD_list>
                    {
                        new EXPCD_list { Value = "11", Text = "IGWPAY" },
                        new EXPCD_list { Value = "12", Text = "IGWOPAY"},
                        new EXPCD_list { Value = "13", Text = "ISWPAY"},
                    };
                return (EXPCD_list);
            }
            else
            {
                List<EXPCD_list> EXPCD_list = new List<EXPCD_list>
                    {
                        new EXPCD_list { Value = "01", Text = "WPAY" },
                        new EXPCD_list { Value = "02", Text = "WOPAY"},
                    };
                return (EXPCD_list);
            }
        }
        public List<REV_CHRG> REV_CHRG()
        {
            List<REV_CHRG> Reverse = new List<REV_CHRG>();//add reverse charge option
            REV_CHRG Reverse0 = new REV_CHRG();
            Reverse0.Text = "No"; Reverse0.Value = "";
            REV_CHRG Reverse1 = new REV_CHRG();
            Reverse1.Text = "Yes"; Reverse1.Value = "Y";
            REV_CHRG Reverse2 = new REV_CHRG();
            Reverse2.Text = "Not Applicable"; Reverse2.Value = "N";
            //Reverse.Add(Reverse0);
            Reverse.Add(Reverse1); Reverse.Add(Reverse2);
            return Reverse;
        }
    }
}