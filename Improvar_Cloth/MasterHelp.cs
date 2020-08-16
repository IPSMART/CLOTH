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
        string CS = null;
        Connection Cn = new Connection();
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
       public string PRCCD_help(ImprovarDB DB)
        {
            using (DB)
            {
                var query = (from c in DB.M_PRCLST
                             join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                             where i.INACTIVE_TAG == "N"
                             select new
                             {
                                 Code = c.PRCCD,
                                 Description = c.PRCNM
                             }).ToList();
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
                }
                var hdr = "Price code" + Cn.GCS() + "Price Name";
                return Generate_help(hdr, SB.ToString());
            }
        }
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
        public string GROUP(ImprovarDB DB, string GRPTYPE = "F", string brandcd = "")
        {
            using (DB)
            {
                string[] itgrptype = GRPTYPE.Split(',');

                var query = (from c in DB.M_GROUP
                             where (itgrptype.Contains(c.ITGRPTYPE))
                             select new
                             {
                                 ITGRPCD = c.ITGRPCD,
                                 ITGRPNM = c.ITGRPNM,
                                 ITGRPTYPE = c.ITGRPTYPE
                             }).OrderBy(c => c.ITGRPNM).ToList();
                if (brandcd != "" && query != null)
                {
                    query = query.ToList();
                }

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].ITGRPNM + "</td><td>" + query[i].ITGRPCD + "</td><td>" + query[i].ITGRPTYPE + "</td></tr>");
                }
                var hdr = "Item Group Name" + Cn.GCS() + "Item Group Code" + Cn.GCS() + "Item Group Type";
                return Generate_help(hdr, SB.ToString());
            }
        }
        public string SIZE(string val, string itcd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                var query = (from c in DB.M_SIZE join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO where i.INACTIVE_TAG == "N" select new { SIZECD = c.SIZECD, SIZENM = c.SIZENM }).ToList();
                if (itcd != "")
                {
                    query = (from c in DB.M_SITEM_SIZE join k in DB.M_SIZE on c.SIZECD equals k.SIZECD join i in DB.M_CNTRL_HDR on k.M_AUTONO equals i.M_AUTONO where (k.M_AUTONO == i.M_AUTONO && i.INACTIVE_TAG == "N" && c.ITCD == itcd) select new { SIZECD = k.SIZECD, SIZENM = k.SIZENM }).ToList();
                }
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].SIZENM + "</td><td>" + query[i].SIZECD + "</td></tr>");
                    }
                    var hdr = "Size Name" + Cn.GCS() + "Size Code";
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
                            str = i.SIZECD + Cn.GCS() + i.SIZENM;
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
                var query = (from c in DB.M_COLOR join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO where i.INACTIVE_TAG == "N" select new { COLRCD = c.COLRCD, COLRNM = c.COLRNM }).ToList();
                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= query.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + query[i].COLRNM + "</td><td>" + query[i].COLRCD + "</td></tr>");
                    }
                    var hdr = "Color Name" + Cn.GCS() + "Color Code";
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
                            str = i.COLRCD + Cn.GCS() + i.COLRNM;
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

        public string BRANDCD_help(ImprovarDB DB)
        {
            using (DB)
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

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td><td>  <img src='" + query[i].DOC_FILE + "' style='width:20px;height:20px;' />   </td></tr>");
                }
                var hdr = "Brand Name" + Cn.GCS() + "Brand Code" + Cn.GCS() + "Image";
                return Generate_help(hdr, SB.ToString());
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
            string COM = CommVar.Compcd(UNQSNO), LOC =  CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
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
            string LOC =  CommVar.Loccd(UNQSNO);
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
    }
}