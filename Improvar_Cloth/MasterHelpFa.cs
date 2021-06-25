using System;
using System.Linq;
using Improvar.Models;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Reflection;

namespace Improvar
{
    public class MasterHelpFa
    {
        string CS = null;
        Connection Cn = new Connection();
        string UNQSNO = CommVar.getQueryStringUNQSNO();

        public string Generate_SearchPannel(string th, string tbody, string returnSEQ = "", string hiddenSEQ = "")
        {
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            if (hiddenSEQ != "")
            {
                var Hfldind = hiddenSEQ.Split(Convert.ToChar(Cn.GCS()));
                int i = 1;
                for (i = 1; i <= Hfldind.Length; i++)
                {
                    SB.Append("<input type='hidden' id='RefHiddenColumn1' value='" + Hfldind[i - 1] + "' />");
                }
                for (; i <= 3; i++)
                {
                    SB.Append("<input type='hidden' id='RefHiddenColumn" + i + "' value='N' />");
                }
            }
            SB.Append("<input type='hidden' id='ReturnIndexs' value='" + returnSEQ + "' />");
            SB.Append("<table id='helpmnu' class='table  table-striped table-bordered table-hover compact' cellpadding='3px' cellspacing='3px' width='100%'><thead style='background-color:#2965aa; color:white'><tr>");
            var hdrRow = th.Split(Convert.ToChar(Cn.GCS()));
            foreach (var v in hdrRow)
            {
                SB.Append("<th  tabindex='-1'>" + v.ToString() + "</th>");
            }
            SB.Append("</tr></thead><tbody>");
            SB.Append(tbody);
            SB.Append("</tbody><tfoot><tr>");
            foreach (var v in hdrRow)
            {
                SB.Append("<th>" + v.ToString() + "</th>");
            }
            SB.Append("</tr></tfoot></table>");
            return SB.ToString();
        }

        //public string Generate_help(string th, string tbody, string hiddenSEQ = "")
        //{
        //    System.Text.StringBuilder SB = new System.Text.StringBuilder();
        //    if (hiddenSEQ != "")
        //    {
        //        var Hfldind = hiddenSEQ.Split(Convert.ToChar(Cn.GCS()));
        //        int i = 1;
        //        for (i = 1; i <= Hfldind.Length; i++)
        //        {
        //            SB.Append("<input type='hidden' id='RefHiddenColumn1' value='" + Hfldind[i - 1] + "' />");
        //        }
        //        for (; i <= 3; i++)
        //        {
        //            SB.Append("<input type='hidden' id='RefHiddenColumn" + i + "' value='N' />");
        //        }
        //    }
        //    SB.Append("<input type='hidden' id='RefHiddenColumn1' value='N' />");
        //    SB.Append("<input type='hidden' id='RefHiddenColumn2' value='N' />");
        //    SB.Append("<input type='hidden' id='RefHiddenColumn3' value='N' />");//compact class
        //    SB.Append("<table id='helpmnu' class='table  table-striped table-bordered table-hover ' cellpadding='3px' cellspacing='3px' width='100%'><thead style='background-color:#2965aa; color:white'><tr>");
        //    var hdrRow = th.Split(Convert.ToChar(Cn.GCS()));
        //    foreach (var v in hdrRow)
        //    {
        //        SB.Append("<th  tabindex='-1'>" + v.ToString() + "</th>");
        //    }
        //    SB.Append("</tr></thead><tbody>");
        //    SB.Append(tbody);
        //    SB.Append("</tbody><tfoot><tr>");
        //    foreach (var v in hdrRow)
        //    {
        //        SB.Append("<th>" + v.ToString() + "</th>");
        //    }
        //    SB.Append("</tr></tfoot></table>");
        //    return SB.ToString();
        //}

        public string Generate_help(string th, string tbody, string hiddenSEQ = "")
        {
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            if (hiddenSEQ != "")
            {
                var Hfldind = hiddenSEQ.Split(Convert.ToChar(Cn.GCS()));
                int i = 1;
                for (i = 1; i <= Hfldind.Length; i++)
                {
                    SB.Append("<input type='hidden' id='RefHiddenColumn" + i + "' value='" + Hfldind[i - 1] + "' />");
                }
                for (; i <= 3; i++)
                {
                    SB.Append("<input type='hidden' id='RefHiddenColumn" + i + "' value='N' />");
                }
            }
            SB.Append("<table id='helpmnu' class='table  table-striped table-bordered table-hover compact' cellpadding='3px' cellspacing='3px' width='100%'><thead style='background-color:#2965aa; color:white'><tr>");
            var hdrRow = th.Split(Convert.ToChar(Cn.GCS()));
            foreach (var v in hdrRow)
            {
                SB.Append("<th  tabindex='-1'>" + v.ToString() + "</th>");
            }
            SB.Append("</tr></thead><tbody>");
            SB.Append(tbody);
            SB.Append("</tbody><tfoot><tr>");
            foreach (var v in hdrRow)
            {
                SB.Append("<th>" + v.ToString() + "</th>");
            }
            SB.Append("</tr></tfoot></table>");
            return SB.ToString();
        }
        public DataTable SQLquery(string SQL)
        {
            DataTable dt = new DataTable();
            try
            {
                string CS = "";
                DataTable RECORD1 = new DataTable();
                string SCHEMA = Cn.Getschema;
                CS = Cn.GetConnectionString();
                Cn.con = new OracleConnection(CS);
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }
                string SQLQuery = SQL;
                Cn.com = new OracleCommand(SQLQuery, Cn.con);
                Cn.da.SelectCommand = Cn.com;
                Cn.da.Fill(dt);
                Cn.com.Dispose();
                Cn.con.Close();
                Cn.con.Dispose();
                return dt;
            }
            catch (Exception ex)
            {
                Cn.com.Dispose();
                Cn.con.Close();
                Cn.con.Dispose();
                Cn.SaveException(ex, SQL);
                return null;
            }
        }

        public string IFSCCODE_help(ImprovarDB DB)
        {
            var query = (from c in DB.MS_BANKIFSC
                         select new
                         {
                             IFSC = c.IFSCCODE,
                             BANKNAME = c.BANKNAME,
                             BRANCH = c.BRANCH,
                             CITY = c.CITY
                         }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr><td>" + query[i].BANKNAME + "</td><td>" + query[i].CITY + "</td><td>" + query[i].IFSC + "</td><td>" + query[i].BRANCH + "</td></tr>");
            }
            var hdr = "Bank Name" + Cn.GCS() + "City" + Cn.GCS() + "IFSC Code" + Cn.GCS() + "Branch";
            return Generate_help(hdr, SB.ToString());
        }
        public string COMPCD_help(ImprovarDB DB)
        {
            var query = (from c in DB.M_COMP
                         select new
                         {
                             Code = c.COMPCD,
                             Description = c.COMPNM
                         }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
            }
            var hdr = "Company Name" + Cn.GCS() + "Company Code";
            return Generate_help(hdr, SB.ToString());
        }
        public string SQLNonQuery(string SQL)
        {
            //by hook        
            try
            {
                string CS = Cn.GetConnectionString();
                Cn.con = new OracleConnection(CS);
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }
                Cn.com = new OracleCommand(SQL, Cn.con);
                Cn.com.ExecuteNonQuery();
                Cn.con.Close();
                return "";
            }
            catch (Exception ex)
            {
                Cn.con.Close();
                return ex.ToString();
            }
        }

        public string DISTCD_help(ImprovarDB DB)
        {
            var query = (from c in DB.M_DISTRICT
                         select new
                         {
                             Code = c.DISTCD,
                             Description = c.DISTNM
                         }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr ><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
            }
            var hdr = "District Name" + Cn.GCS() + "District Code";
            return Generate_help(hdr, SB.ToString());
        }
        public string STATECD_help(ImprovarDB DB)
        {
            var query = (from c in DB.MS_STATE
                         select new
                         {
                             STATECD = c.STATECD,
                             STATENM = c.STATENM
                         }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr><td>" + query[i].STATENM + "</td><td>" + query[i].STATECD + "</td></tr>");
            }
            var hdr = "State Name" + Cn.GCS() + "State Code";
            return Generate_help(hdr, SB.ToString());
        }
        public string CURRCD_help(ImprovarDB DB)
        {
            var query = (from c in DB.MS_CURRENCY select new { CURRCD = c.CURRCD, CURRNM = c.CURRNM }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr ><td>" + query[i].CURRNM + "</td><td>" + query[i].CURRCD + "</td></tr>");
            }
            var hdr = "Currency Name" + Cn.GCS() + "Currency Code";
            return Generate_help(hdr, SB.ToString());
        }
        public string CISDCD_help(ImprovarDB DB)
        {
            var query = (from c in DB.MS_COUNTRY
                         select new
                         {
                             Code = c.CISDCD,
                             Description = c.CNAME
                         }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr ><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
            }
            var hdr = "COUNTRY Name" + Cn.GCS() + "COUNTRY Code";
            return Generate_help(hdr, SB.ToString());
        }
        public string USER_ID_help(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);

            var query = (from c in DB.USER_APPL
                         select new
                         {
                             USER_ID = c.USER_ID,
                             USER_NAME = c.USER_NAME
                         }).ToList();
            if (val == null)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].USER_ID + "</td><td>" + query[i].USER_NAME + "</td></tr>");
                }
                var hdr = "User Id" + Cn.GCS() + "User Name";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                query = query.Where(a => a.USER_ID == val).ToList();
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.USER_ID + Cn.GCS() + i.USER_NAME;
                    }
                    return str;
                }
                else
                {
                    return "Invalid User Id ! Please Select / Enter a Valid User Id !!";
                }
            }

        }
        public string USER_HELP(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            var query = (from c in DB.USER_APPL select new { USER_ID = c.USER_ID, USER_NAME = c.USER_NAME }).ToList();
            if (val == null)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].USER_NAME + "</td><td>" + query[i].USER_ID + "</td></tr>");
                }
                var hdr = "User Name" + Cn.GCS() + "User Id";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                query = query.Where(a => a.USER_ID == val).ToList();
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.USER_ID + Cn.GCS() + i.USER_NAME;
                    }
                    return str;
                }
                else
                {
                    return "Invalid User Id ! Please Select / Enter a Valid User Id !!";
                }
            }
        }
        public string SLCD_help(ImprovarDB DB)
        {
            var query = (from c in DB.M_SUBLEG
                         join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                         where i.INACTIVE_TAG == "N"
                         select new
                         {
                             SLCD = c.SLCD,
                             SLNM = c.SLNM,
                         }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr ><td>" + query[i].SLNM + "</td><td>" + query[i].SLCD + "</td></tr>");
            }
            var hdr = "Sub ledger Description" + Cn.GCS() + "Sub ledger Code";
            return Generate_help(hdr, SB.ToString());
        }
        public string SubLeg_Help(string val, string LINK_CD = "", string CAPTION = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string[] linkcode = LINK_CD.Split(',');
                string sql = "";
                string linkcd = LINK_CD.retSqlformat();

                sql += "select distinct a.slcd, a.slnm, a.gstno ";
                sql += "from " + scmf + ".m_subleg a, " + scmf + ".m_subleg_link b, " + scmf + ".m_cntrl_hdr c, " + scmf + ".m_cntrl_loca d ";
                sql += "where a.slcd=b.slcd(+) and a.m_autono=c.m_autono(+) and a.m_autono=d.m_autono(+) and ";
                if (val.retStr() != "") sql += "a.slcd='" + val + "' and ";
                sql += "b.linkcd in (" + linkcd + ") and ";
                sql += "(d.compcd='" + COM + "' or d.compcd is null) and (d.loccd='" + LOC + "' or d.loccd is null) and ";
                sql += "nvl(c.inactive_tag,'N') = 'N' ";
                sql += "order by slnm,slcd";
                DataTable tbl = SQLquery(sql);

                //var query = (from a in DB.M_SUBLEG
                //             join b in DB.M_SUBLEG_LINK on a.SLCD equals b.SLCD into x
                //             from b in x.DefaultIfEmpty()
                //             join i in DB.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
                //             join c in DB.M_CNTRL_LOCA on a.M_AUTONO equals c.M_AUTONO into g
                //             from c in g.DefaultIfEmpty()
                //             where (linkcode.Contains(b.LINKCD) && c.COMPCD == COM && c.LOCCD == LOC && i.INACTIVE_TAG == "N" || linkcode.Contains(b.LINKCD) && c.COMPCD == null && c.LOCCD == null && i.INACTIVE_TAG == "N")
                //             select new
                //             {
                //                 SLNM = a.SLNM,
                //                 SLCD = a.SLCD,
                //                 GSTNO = a.GSTNO
                //             }).ToList();

                //if (val == null)
                //{
                //    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                //    for (int i = 0; i <= query.Count - 1; i++)
                //    {
                //        SB.Append("<tr><td>" + query[i].SLNM + "</td><td>" + query[i].SLCD + "</td><td>" + query[i].GSTNO + "</td></tr>");
                //    }
                //    var hdr = "" + CAPTION + " Name" + Cn.GCS() + "" + CAPTION + " Code" + Cn.GCS() + "GST Number";
                //    return Generate_help(hdr, SB.ToString());
                //}
                //else
                //{
                //    query = query.Where(a => a.SLCD == val).ToList();
                //    if (query.Any())
                //    {
                //        string str = "";
                //        foreach (var i in query)
                //        {
                //            str = i.SLCD + Cn.GCS() + i.SLNM;
                //        }
                //        return str;
                //    }
                //    else
                //    {
                //        return "Invalid " + CAPTION + " Code ! Please Enter a Valid " + CAPTION + " Code !!";
                //    }
                //}

                if (val == null)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + tbl.Rows[i]["slnm"] + "</td><td>" + tbl.Rows[i]["slcd"] + " </td><td>" + tbl.Rows[i]["gstno"] + " </td></tr>");
                    }
                    var hdr = "" + CAPTION + " Name" + Cn.GCS() + "" + CAPTION + " Code" + Cn.GCS() + "GST Number";
                    return Generate_help(hdr, SB.ToString());
                }
                else
                {
                    if (tbl.Rows.Count > 0)
                    {
                        string str = "";
                        str = tbl.Rows[0]["slcd"] + Cn.GCS() + tbl.Rows[0]["slnm"];
                        return str;
                    }
                    else
                    {
                        return "Invalid " + CAPTION + " Code ! Please Enter a Valid " + CAPTION + " Code !!";
                    }
                }
            }
        }
        public string UOM_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var query = (from c in DB.M_UOM
                         join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                         where i.INACTIVE_TAG == "N"
                         select new
                         {
                             UOMCD = c.UOMCD,
                             UOMNM = c.UOMNM
                         }).ToList();
            if (val == null)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].UOMNM + "</td><td>" + query[i].UOMCD + "</td></tr>");
                }
                var hdr = "UOM Name" + Cn.GCS() + "UOM Code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                query = query.Where(a => a.UOMCD == val).ToList();
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.UOMCD + Cn.GCS() + i.UOMNM;
                    }
                    return str;
                }
                else
                {
                    return "Invalid UOM Code ! Please Enter a Valid UOM Code !!";
                }
            }
        }
        public string DOCUMENT1(ImprovarDB DB)
        {
            string DOCCD = System.Web.HttpContext.Current.Session["menudoccd"].ToString();

            string[] XYZ = DOCCD.ToString().Split(',');

            var query = (from i in DB.M_DOCTYPE
                         join j in DB.M_CNTRL_HDR on i.M_AUTONO equals j.M_AUTONO
                         where (XYZ.Contains(i.DOCTYPE) && j.INACTIVE_TAG == "N")
                         select new DocumentType()
                         {
                             value = i.DOCCD,
                             text = i.DOCNM
                         }).OrderBy(s => s.text).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr><td>" + query[i].text + "</td><td>" + query[i].value + "</td></tr>");
            }
            var hdr = "Document Name" + Cn.GCS() + "Document Code";
            return Generate_help(hdr, SB.ToString());
        }
        public string fild(string chr)
        {
            string chrd = "";
            if (chr == null || chr == "") chrd = "null";
            else chrd = "to_date('" + chr.ToString().Substring(0, 10).Replace("-", "/") + "','dd/mm/yyyy') ";
            return chrd;
        }
        public string filc(string chr)
        {
            if (chr == null || chr == "") chr = "null";
            else chr = "'" + chr.Replace("'", "''") + "'";
            return chr;
        }
        public string filnd(double? chr)
        {
            string chrd = "";
            if (chr == null) chrd = "null";
            else chrd = chr.ToString();
            return chrd;
        }
        public string filndc(decimal? chr)
        {
            string chrd = "";
            if (chr == null) chrd = "null";
            else chrd = chr.ToString();
            return chrd;
        }
        public string finTblUpdt(string tblname, string autono, string dtag, string modcd = "F")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            if (autono.IndexOf("'") < 0) autono = "'" + autono + "'";

            string scmf = "";
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
            sql = "update " + scmf + "." + tblname + " set dtag='" + dtag + "' where autono in (" + autono + ") ";
            sql = sql + "~" + "delete from " + scmf + "." + tblname + " where autono in (" + autono + ") ";

            return sql;
        }
        public string InsVch_Hdr(string autono, string doccd, string docno, string docdt, short emd_no = 0, string dtag = null, string class1cd = null, string class2cd = null, string autogen = null, string vl_dt = null,
               string trcd = "TR", string pay_by = null, string paid_to = null, string currcd = null, double currrt = 0, string bank_code = null, string revchg = null, string inptclaim = "Y")
        {
            string bl = "";
            try
            {
                string scmf = CommVar.FinSchema(UNQSNO);
                string clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";

                sql = "insert into " + scmf + ".t_vch_hdr (emd_no, clcd, dtag, ttag, autono, doccd, docno, docdt, vl_dt, class1cd, class2cd, pay_by, paid_to, currcd, ";
                sql = sql + "currrt, bank_code, autogen, revchg, inptclaim, trcd) values (";
                sql = sql + emd_no;
                sql = sql + "," + filc(clcd);
                sql = sql + "," + filc(dtag);
                sql = sql + "," + filc(null);
                sql = sql + "," + filc(autono);
                sql = sql + "," + filc(doccd);
                sql = sql + "," + filc(docno);
                sql = sql + "," + fild(docdt);
                sql = sql + "," + fild(vl_dt);
                sql = sql + "," + filc(class1cd);
                sql = sql + "," + filc(class2cd);
                sql = sql + "," + filc(pay_by);
                sql = sql + "," + filc(paid_to);
                sql = sql + "," + filc(currcd);
                sql = sql + "," + currrt;
                sql = sql + "," + filc(bank_code);
                sql = sql + "," + filc(autogen);
                sql = sql + "," + filc(revchg);
                sql = sql + "," + filc(inptclaim);
                sql = sql + "," + filc(trcd);
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }

        public string InsVch_Det(string autono, string doccd, string docno, string docdt, short emd_no, string dtag, short slno, string drcr, string glcd, string slcd, double amt,
            string t_rem, string r_glcd = null, string r_slcd = null, double qty = 0, double curr_rate = 0, double curr_amt = 0, string postdt = "")
        {
            string bl = "";
            try
            {
                string scmf = CommVar.FinSchema(UNQSNO);
                string clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";

                sql = "insert into " + scmf + ".t_vch_det (emd_no, clcd, dtag, ttag, autono, doccd, docno, docdt, slno, drcr, glcd, slcd, amt, t_rem, r_glcd, r_slcd, qty, ";
                sql = sql + "curr_rate, curr_amt, postdt) values (";
                sql = sql + emd_no;
                sql = sql + "," + filc(clcd);
                sql = sql + "," + filc(dtag);
                sql = sql + "," + filc(null);
                sql = sql + "," + filc(autono);
                sql = sql + "," + filc(doccd);
                sql = sql + "," + filc(docno);
                sql = sql + "," + fild(docdt);
                sql = sql + "," + slno;
                sql = sql + "," + filc(drcr);
                sql = sql + "," + filc(glcd);
                sql = sql + "," + filc(slcd);
                sql = sql + "," + amt;
                sql = sql + "," + filc(t_rem);
                sql = sql + "," + filc(r_glcd);
                sql = sql + "," + filc(r_slcd);
                sql = sql + "," + qty;
                sql = sql + "," + curr_rate;
                sql = sql + "," + curr_amt;
                sql = sql + "," + fild(postdt);
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }

        public string InsVch_Class(string autono, string doccd, string docno, string docdt, short emd_no, string dtag, short slno, short dslno,
            string refslcd, string class1cd, string class2cd, double amt, double curr_amt = 0, string rem1 = "")
        {
            string bl = "";
            try
            {
                string scmf = CommVar.FinSchema(UNQSNO);
                string clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";

                sql = "insert into " + scmf + ".t_vch_class (emd_no, clcd, dtag, ttag, autono, doccd, docno, docdt, slno, dslno, refcd, class1cd, class2cd, amt, curr_amt, rem1) values (";
                sql = sql + emd_no;
                sql = sql + "," + filc(clcd);
                sql = sql + "," + filc(dtag);
                sql = sql + "," + filc(null);
                sql = sql + "," + filc(autono);
                sql = sql + "," + filc(doccd);
                sql = sql + "," + filc(docno);
                sql = sql + "," + fild(docdt);
                sql = sql + "," + slno;
                sql = sql + "," + dslno;
                sql = sql + "," + filc(refslcd);
                sql = sql + "," + filc(class1cd);
                sql = sql + "," + filc(class2cd);
                sql = sql + "," + amt;
                sql = sql + "," + curr_amt;
                sql = sql + "," + filc(rem1);
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }

        public string InsVch_Bl(string autono, string doccd, string docno, string docdt, short emd_no, string dtag, string drcr, string glcd, string slcd, string conslcd,
                string agslcd, string class1cd, short slno, double amt, string blno, string bldt, string refno, string duedt, string vchtype, double crdays = 0, double itamt = 0,
                string ordno = "", string orddt = "", double blamt = 0, string lrno = "", string lrdt = "", string transnm = "", string flag = "", string rtdebcd = "", string bltype = "", string blrem = "")
        {
            string bl = "";
            try
            {
                string scmf = CommVar.FinSchema(UNQSNO);
                string clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";
                if (blamt == 0) blamt = amt;

                sql = "insert into " + scmf + ".t_vch_bl (emd_no, clcd, dtag, ttag, autono, doccd, docno, docdt, drcr, glcd, slcd, conslcd, agslcd, class1cd, ";
                sql = sql + "slno, amt, blno, bldt, refno, duedt, crdays, itamt, vchtype, blamt, ordno, orddt, lrno, lrdt, transnm, flag,rtdebcd,bltype,blrem) values (";
                sql = sql + emd_no;
                sql = sql + "," + filc(clcd);
                sql = sql + "," + filc(dtag);
                sql = sql + "," + filc(null);
                sql = sql + "," + filc(autono);
                sql = sql + "," + filc(doccd);
                sql = sql + "," + filc(docno);
                sql = sql + "," + fild(docdt);
                sql = sql + "," + filc(drcr);
                sql = sql + "," + filc(glcd);
                sql = sql + "," + filc(slcd);
                sql = sql + "," + filc(conslcd);
                sql = sql + "," + filc(agslcd);
                sql = sql + "," + filc(class1cd);
                sql = sql + "," + slno;
                sql = sql + "," + amt;
                sql = sql + "," + filc(blno);
                sql = sql + "," + fild(bldt);
                sql = sql + "," + filc(refno);
                sql = sql + "," + fild(duedt);
                sql = sql + "," + crdays;
                sql = sql + "," + itamt;
                sql = sql + "," + filc(vchtype);
                sql = sql + "," + blamt;
                sql = sql + "," + filc(ordno);
                sql = sql + "," + fild(orddt);
                sql = sql + "," + filc(lrno);
                sql = sql + "," + fild(lrdt);
                sql = sql + "," + filc(transnm);
                sql = sql + "," + filc(flag);
                sql = sql + "," + filc(rtdebcd);
                sql = sql + "," + filc(bltype);
                sql = sql + "," + filc(blrem);
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }

        public string InsVch_Bl_Adj(string autono, short emd_no, string dtag, short slno, string i_autono, short i_slno, double i_amt, string r_autono, short r_slno, double r_amt, double adj_amt, string flag = "")
        {
            string bl = "";
            try
            {
                string scmf = CommVar.FinSchema(UNQSNO);
                string clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";

                sql = "insert into " + scmf + ".t_vch_bl_adj (emd_no, clcd, dtag, ttag, autono, slno, i_autono, i_slno, i_amt, r_autono, r_slno, r_amt, adj_amt, flag) values (";
                sql = sql + emd_no;
                sql = sql + "," + filc(clcd);
                sql = sql + "," + filc(dtag);
                sql = sql + "," + filc(null);
                sql = sql + "," + filc(autono);
                sql = sql + "," + slno;
                sql = sql + "," + filc(i_autono);
                sql = sql + "," + i_slno;
                sql = sql + "," + i_amt;
                sql = sql + "," + filc(r_autono);
                sql = sql + "," + r_slno;
                sql = sql + "," + r_amt;
                sql = sql + "," + adj_amt;
                sql = sql + "," + filc(flag);
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }
        public string InsVch_GST(string autono, string doccd, string docno, string docdt, short emd_no, string dtag, string drcr, int dslno, int slno, string pcode,
            string blno, string bldt, string hsncode, string itnm, double qnty, string uom, double amt, double igstper, double igstamt, double cgstper, double cgstamt, double sgstper, double sgstamt, double cessper, double cessamt,
            string salpur, double roamt, double blamt, double othramt = 0, string invtypecd = "", string pos = "", string agstdocno = "", string agstdocdt = "", string dncncd = "",
            string gstslnm = "", string gstno = "", string gstsladd1 = "", string gstsldist = "", string gstslpin = "", string expcd = "", string shipdocno = "", string shipdocdt = "", string portcd = "", string dncnsalpur = "",
            string conslcd = "", string exemptedtype = "", double appltaxrate = 0, string expglcd = "", string inptclaim = "")
        {
            string bl = "";
            try
            {
                string scmf = CommVar.FinSchema(UNQSNO), clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";

                string _itnm = itnm;
                if (expcd.retStr() != "") exemptedtype = "";

                sql = "insert into " + scmf + ".t_vch_gst (emd_no, clcd, dtag, ttag, autono, doccd, docno, docdt, dslno, slno, drcr, pcode, blno, bldt, hsncode, itnm, qnty, ";
                sql = sql + "uom, amt, igstper, igstamt, cgstper, cgstamt, sgstper, sgstamt, cessper, cessamt, othramt, roamt, blamt, salpur, invtypecd, dncncd, agstdocno, agstdocdt, ";
                sql = sql + "pos, gstslnm, gstno,gstsladd1,gstsldist,gstslpin,expcd, shipdocno, shipdocdt, portcd, dncnsalpur, conslcd, exemptedtype, appltaxrate, expglcd, inptclaim) values (";
                sql = sql + emd_no;
                sql = sql + "," + filc(clcd);
                sql = sql + "," + filc(dtag);
                sql = sql + "," + filc(null);
                sql = sql + "," + filc(autono);
                sql = sql + "," + filc(doccd);
                sql = sql + "," + filc(docno);
                sql = sql + "," + fild(docdt);
                sql = sql + "," + dslno;
                sql = sql + "," + slno;
                sql = sql + "," + filc(drcr);
                sql = sql + "," + filc(pcode);
                sql = sql + "," + filc(blno);
                sql = sql + "," + fild(bldt);
                sql = sql + "," + filc(hsncode);
                sql = sql + "," + filc(_itnm);
                sql = sql + "," + qnty;
                sql = sql + "," + filc(uom);
                sql = sql + "," + amt;
                sql = sql + "," + igstper;
                sql = sql + "," + igstamt;
                sql = sql + "," + cgstper;
                sql = sql + "," + cgstamt;
                sql = sql + "," + sgstper;
                sql = sql + "," + sgstamt;
                sql = sql + "," + cessper;
                sql = sql + "," + cessamt;
                sql = sql + "," + othramt;
                sql = sql + "," + roamt;
                sql = sql + "," + blamt;
                sql = sql + "," + filc(salpur);
                sql = sql + "," + filc(invtypecd);
                sql = sql + "," + filc(dncncd);
                sql = sql + "," + filc(agstdocno);
                sql = sql + "," + fild(agstdocdt);
                sql = sql + "," + filc(pos);
                sql = sql + "," + filc(gstslnm);
                sql = sql + "," + filc(gstno);
                sql = sql + "," + filc(gstsladd1);
                sql = sql + "," + filc(gstsldist);
                sql = sql + "," + filc(gstslpin);
                sql = sql + "," + filc(expcd);
                sql = sql + "," + filc(shipdocno);
                sql = sql + "," + fild(shipdocdt);
                sql = sql + "," + filc(portcd);
                sql = sql + "," + filc(dncnsalpur);
                sql = sql + "," + filc(conslcd);
                sql = sql + "," + filc(exemptedtype);
                sql = sql + "," + appltaxrate;
                sql = sql + "," + filc(expglcd);
                sql = sql + "," + filc(inptclaim);
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }

        public string InsVch_TdsTxn(string autono, string doccd, string docno, string docdt, short emd_no, string dtag, string blno, string bldt, string glcd, string slcd, string tdscode,
               short slno, double blamt, double tdson, double tdsper, double tdsamt, string tdsdt, string low_tds, string class1cd, string tdstcs = "", double advadj = 0, string expglcd = "")
        {
            string bl = ""; var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {
                string scmf = CommVar.FinSchema(UNQSNO), clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";
                if (tdstcs == "") tdstcs = "";

                sql = "insert into " + scmf + ".t_tdstxn (emd_no, clcd, dtag, ttag, autono, doccd, docno, docdt, blno, bldt, glcd, slcd, tdscode, ";
                sql = sql + "slno, blamt, tdson, tdsper, tdsamt, tdsdt, low_tds, class1cd, tdstcs, advadj, expglcd) values (";
                sql = sql + emd_no;
                sql = sql + "," + filc(clcd);
                sql = sql + "," + filc(dtag);
                sql = sql + "," + filc(null);
                sql = sql + "," + filc(autono);
                sql = sql + "," + filc(doccd);
                sql = sql + "," + filc(docno);
                sql = sql + "," + fild(docdt);
                sql = sql + "," + filc(blno);
                sql = sql + "," + fild(bldt);
                sql = sql + "," + filc(glcd);
                sql = sql + "," + filc(slcd);
                sql = sql + "," + filc(tdscode);
                sql = sql + "," + slno;
                sql = sql + "," + blamt;
                sql = sql + "," + tdson;
                sql = sql + "," + tdsper;
                sql = sql + "," + tdsamt;
                sql = sql + "," + fild(tdsdt);
                sql = sql + "," + filc(low_tds);
                sql = sql + "," + filc(class1cd);
                sql = sql + "," + filc(tdstcs);
                sql = sql + "," + advadj;
                sql = sql + "," + filc(expglcd);
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }

        public string Instcntrl_hdr(string autono, string vchrmod, string modcd, string mnthcd, string doccd, string docno, string docdt, short emd_no, string dtag, string doconlyno, double vchrno,
                string calauto = null, string vchrprefix = null, string glcd = null, string slcd = null, double docamt = 0, string cancel = null)

        {
            string bl = "";
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);
            string[] fin = CommVar.FinPeriod(UNQSNO).Split('-');
            string YEARCD = fin[0].Substring(6).Trim();
            string uid = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
            string uIP = Cn.GetIp();
            try
            {
                string scmf = "";
                switch (modcd)
                {
                    case "F":
                        scmf = CommVar.FinSchema(UNQSNO); break;
                    case "I":
                        scmf = CommVar.InvSchema(UNQSNO); break;
                }
                string clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";
                string pkgmodcd = "S"; //System.Web.HttpContext.Current.Session["ModuleCode"].ToString();

                string _docdt = "to_date('" + docdt.ToString().Substring(0, 10).Replace("-", "/") + "','dd/mm/yyyy') ";
                //if (vchrmod == "E")
                //{
                DataTable rsTmp = SQLquery("select autono from " + scmf + ".t_cntrl_hdr where autono='" + autono + "'");
                if (rsTmp.Rows.Count <= 0) vchrmod = "A";
                //}

                if (vchrmod == "A")
                {
                    sql = "insert into " + scmf + ".t_cntrl_hdr (emd_no, clcd, dtag, ttag, autono, modcd, compcd, loccd, yr_cd, doccd, docno, docdt, doconlyno, vchrno, vchrsuffix, mnthcd, ";
                    sql = sql + "calauto, glcd, slcd, docamt, cancel, usr_id, usr_entdt, usr_lip, usr_sip, usr_os, usr_mnm) values (";
                    sql = sql + emd_no;
                    sql = sql + "," + filc(clcd);
                    sql = sql + "," + filc(dtag);
                    sql = sql + "," + filc(null);
                    sql = sql + "," + filc(autono);
                    sql = sql + "," + filc(pkgmodcd);
                    sql = sql + "," + filc(COM);
                    sql = sql + "," + filc(LOC);
                    sql = sql + "," + filc(YEARCD);
                    sql = sql + "," + filc(doccd);
                    sql = sql + "," + filc(docno);
                    sql = sql + "," + _docdt;
                    sql = sql + "," + filc(doconlyno);
                    sql = sql + "," + vchrno;
                    sql = sql + "," + filc(vchrprefix);
                    sql = sql + "," + filc(mnthcd);
                    sql = sql + "," + filc(calauto);
                    sql = sql + "," + filc(glcd);
                    sql = sql + "," + filc(slcd);
                    sql = sql + "," + docamt;
                    sql = sql + "," + filc(cancel);
                    sql = sql + "," + filc(uid);
                    sql = sql + ", SYSDATE ";
                    sql = sql + "," + filc(uIP);
                    sql = sql + "," + filc(Cn.GetStaticIp());
                    sql = sql + "," + filc(null);
                    sql = sql + "," + filc(Cn.DetermineCompName(uIP));
                    sql = sql + ")";
                }
                else
                {
                    sql = "update " + scmf + ".t_cntrl_hdr set ";
                    sql = sql + "emd_no=" + emd_no;
                    sql = sql + ", dtag=" + filc(dtag);
                    sql = sql + ", docdt=" + _docdt;
                    sql = sql + ", docno=" + filc(docno);
                    sql = sql + ", doconlyno=" + filc(doconlyno);
                    sql = sql + ", vchrno=" + vchrno;
                    sql = sql + ", vchrsuffix=" + filc(vchrprefix);
                    sql = sql + ", mnthcd=" + filc(mnthcd);
                    sql = sql + ", calauto=" + filc(calauto);
                    sql = sql + ", glcd=" + filc(glcd);
                    sql = sql + ", slcd=" + filc(slcd);
                    sql = sql + ", docamt=" + docamt;
                    sql = sql + ", lm_usr_id=" + filc(uIP);
                    sql = sql + ", lm_usr_entdt=SYSDATE";
                    sql = sql + ", lm_usr_lip=" + filc(uIP);
                    sql = sql + ", lm_usr_sip=" + filc(Cn.GetStaticIp());
                    sql = sql + ", lm_usr_os=" + filc(null);
                    sql = sql + ", lm_usr_mnm=" + filc(Cn.DetermineCompName(uIP));
                    sql = sql + ", cancel=" + filc(cancel);
                    sql = sql + " where autono=" + filc(autono);
                }

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }
        public string InsTCntrlHdrDocPass(string autono, long slno = 1, long pass_level = 1, short emd_no = 0, string dtag = null, string authcd = null, string authrem = null)
        {
            string bl = "";
            try
            {
                string scm1 = CommVar.CurSchema(UNQSNO);
                string clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";

                sql = "insert into " + scm1 + ".t_cntrl_doc_pass (emd_no, clcd, dtag, ttag, autono, slno, pass_level, authcd, authrem) values (";
                sql = sql + emd_no;
                sql = sql + "," + filc(clcd);
                sql = sql + "," + filc(dtag);
                sql = sql + "," + filc(null);
                sql = sql + "," + filc(autono);
                sql = sql + "," + slno;
                sql = sql + "," + pass_level;
                sql = sql + "," + filc(authcd);
                sql = sql + "," + filc(authrem);
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }
        public decimal slcdbal(string slcd = "", string glcd = "", string docdt = "", string class1cd = "", string skipautono = "")
        {
            string COM = CommVar.Compcd(UNQSNO);
            string scmf = CommVar.FinSchema(UNQSNO);
            string sql = "";

            if (glcd == null) glcd = "";
            if (glcd != "") if (glcd.IndexOf("'") < 0) glcd = "'" + glcd + "'";
            if (slcd == null) slcd = "";
            if (slcd != "") if (slcd.IndexOf("'") < 0) slcd = "'" + slcd + "'";
            if (class1cd == null) class1cd = "";
            if (class1cd != "") if (class1cd.IndexOf("'") < 0) class1cd = "'" + class1cd + "'";

            sql = "";
            sql += "select sum(case a.drcr when 'D' then nvl(c.amt,a.amt) when 'C' then nvl(c.amt,a.amt)*-1 end) balamt, ";
            sql += "sum(case a.drcr when 'D' then c.amt when 'C' then c.amt*-1 end) class1amt ";
            sql += "from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_hdr b, " + scmf + ".t_vch_class c, " + scmf + ".t_cntrl_hdr d ";
            sql += "where a.autono=b.autono and a.autono=c.autono(+) and a.slno=c.dslno(+) and a.autono=d.autono and ";
            sql += "a.drcr in ('D','C') and nvl(d.cancel,'N') = 'N' and ";
            if (glcd != "") sql += "a.glcd in (" + glcd + ") and ";
            if (slcd != "") sql += "a.slcd in (" + slcd + ") and ";
            if (class1cd != "") sql += "c.class1cd in (" + class1cd + ") and ";
            if (skipautono != "" && skipautono != null) sql += "d.autono <> '" + skipautono + "' and ";
            if (docdt != null && docdt != "") sql += "d.docdt <= to_date('" + docdt.Substring(0, 10) + "','dd/mm/yyyy') and ";
            sql += "d.compcd='" + COM + "' ";

            decimal lgbal = 0;
            DataTable rsTmp = SQLquery(sql);

            if (rsTmp.Rows.Count > 0 && rsTmp.Rows[0]["BALAMT"].ToString() != "")
            {
                if (class1cd != "") lgbal = Convert.ToDecimal(rsTmp.Rows[0]["class1amt"]);
                else lgbal = Convert.ToDecimal(rsTmp.Rows[0]["balamt"]);
            }
            return lgbal;
        }

        public DataTable retslcdCont(string slcd, string dept = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "";
            string scmf = CommVar.FinSchema(UNQSNO);
            string sslcd = slcd;
            string deptt = dept;
            string modcd = CommVar.ModuleCode();

            if (slcd.IndexOf(',') < 0) sslcd = "'" + slcd + "'";
            if (dept == "")
            {
                if (modcd == "FIN") deptt = "F";
                else if (modcd == "INV") deptt = "P";
                else if (modcd == "PAY") deptt = "A";
                else deptt = "S";
            }

            sql += "select a.slcd, nvl(c.fullname,c.slnm) slnm, nvl(b.persemail,a.regemailid) regemailid, ";
            sql += "nvl(b.mobile1,a.regmobile) regmobile, c.gstno, c.panno, c.statecd, ";
            sql += "c.add1, c.add2, c.add3, c.add4, c.add5, c.add6, c.add7, ";
            sql += "c.district, c.pin, c.state, c.country, b.cperson, b.desig, ";
            sql += "d.ifsccode, d.bankname, d.branch, d.address, d.bankactno from ";
            sql += "(select a.slcd, a.regemailid, decode(nvl(a.regmobile,0),0,null,to_char(a.regmobile)) regmobile ";
            sql += "from " + scmf + ".m_subleg a ) a, ";

            sql += "(select a.slcd, a.cperson, a.desig, a.persemail, a.mobile1 from ";
            sql += "(select a.slcd, a.cperson, a.desig, a.persemail, decode(nvl(a.mobile1,0),0,null,to_char(a.mobile1)) mobile1, ";
            sql += "row_number() over (partition by a.slcd order by a.slno desc) as rn ";
            sql += "from " + scmf + ".m_subleg_cont a ";
            sql += "where a.dept='F') a ";
            sql += "where a.rn=1 ) b, ";

            sql += "(select a.slcd, a.ifsccode, a.bankname, a.branch, a.address, a.bankactno from ";
            sql += "(select a.slcd, a.ifsccode, a.bankname, a.branch, a.address, a.bankactno, ";
            sql += "row_number() over (partition by a.slcd order by a.slno desc) as rn ";
            sql += "from " + scmf + ".m_subleg_ifsc a ";
            sql += "where a.defltbank='T') a ";
            sql += "where a.rn=1 ) d, ";

            sql += "" + scmf + ".m_subleg c ";
            sql += "where a.slcd=b.slcd(+) and a.slcd=c.slcd(+) and a.slcd=d.slcd(+) ";
            if (sslcd != "") sql += "and a.slcd in (" + sslcd + ") ";

            DataTable tbl = SQLquery(sql);

            return tbl;
        }
        public string ToReturnFieldValues<T>(IEnumerable<T> Linqlist, DataTable tbl = null)
        {
            if (tbl == null) tbl = ListToDatatable.LINQResultToDataTable(Linqlist);
            string str = "";
            if (tbl == null) return "";
            if (tbl.Rows.Count < 0) return "";
            for (int c = 0; c <= tbl.Columns.Count - 1; c++)
            {
                string colname = tbl.Columns[c].ColumnName.ToUpper();
                str += "^" + colname + "=^" + tbl.Rows[0][colname] + Cn.GCS();
            }
            return str;
        }
        public string retStrforHelp<T>(IEnumerable<T> Linqlist, DataTable tbl = null)
        {
            if (tbl == null) tbl = ListToDatatable.LINQResultToDataTable(Linqlist);
            string str = "";
            if (tbl == null) return "";
            if (tbl.Rows.Count < 0) return "";
            for (int c = 0; c <= tbl.Columns.Count - 1; c++)
            {
                string colname = tbl.Columns[c].ColumnName.ToUpper();
                str += colname + "=^" + tbl.Rows[0][colname] + Cn.GCS();
            }
            return str;
        }
        public string[] glslcdbal(string glcd, string slcd = "", string docdt = "", string class1cd = "", string skipautono = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO);
            string scmf = CommVar.FinSchema(UNQSNO);
            string sql = "";

            if (glcd == null) glcd = "";
            if (slcd == null) slcd = "";
            if (slcd != "") if (slcd.IndexOf("'") < 0) slcd = "'" + slcd + "'";
            if (class1cd == null) class1cd = "";
            //if (class1cd != "") if (class1cd.IndexOf("'") < 0) class1cd = "'" + class1cd + "'";

            sql += "select sum(case a.drcr when 'D' then nvl(c.amt,a.amt) when 'C' then nvl(c.amt,a.amt)*-1 end) balamt, ";
            sql += "sum(case a.drcr when 'D' then nvl(c.amt,0) when 'C' then nvl(c.amt,0)*-1 end) class1amt ";
            sql += "from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_hdr b, " + scmf + ".t_vch_class c, " + scmf + ".t_cntrl_hdr d ";
            sql += "where a.autono=b.autono and a.autono=c.autono(+) and a.slno=c.dslno(+) and a.autono=d.autono and ";
            sql += "a.drcr in ('D','C') and nvl(d.cancel,'N') = 'N' and ";
            sql += "a.glcd = '" + glcd + "' and ";
            if (slcd != "") sql += "a.slcd in (" + slcd + ") and ";
            if (class1cd != "") sql += "(c.class1cd = '" + class1cd + "' or c.class1cd is null) and ";
            if (skipautono != "" && skipautono != null) sql = sql + "d.autono <> '" + skipautono + "' and ";
            if (docdt != null && docdt != "") sql += "d.docdt <= to_date('" + docdt.Substring(0, 10) + "','dd/mm/yyyy') and ";
            sql += "d.compcd='" + COM + "' and b.trcd not in (" + CommVar.SkipTrCd() + ") ";
            decimal lgbal = 0;
            DataTable rsTmp = SQLquery(sql);

            if (rsTmp.Rows.Count > 0 && rsTmp.Rows[0]["BALAMT"].ToString() != "")
            {
                if (class1cd != "") lgbal = Convert.ToDecimal(rsTmp.Rows[0]["class1amt"]);
                else lgbal = Convert.ToDecimal(rsTmp.Rows[0]["balamt"]);
            }
            string lgbalstr = Cn.Indian_Number_format(Math.Abs(lgbal).ToString(), "####,##,##,##0.00");
            if (lgbal < 0) lgbalstr = lgbalstr + " Cr"; else lgbalstr = lgbalstr + " Dr";
            lgbalstr = lgbal.ToString() + Cn.GCS() + lgbalstr;
            return lgbalstr.Split(Convert.ToChar(Cn.GCS()));
        }
        public DataTable GenGlSlcdbalTbl(string glcd = "", string slcd = "", string docdt = "", string class1cd = "", string linkcd = "", string comploccd = "", bool loccdshow = true, string skipautono = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO);
            string scmf = CommVar.FinSchema(UNQSNO);

            if (glcd == null) glcd = "";
            if (slcd == null) slcd = "";
            if (slcd != "") if (slcd.IndexOf("'") < 0) slcd = "'" + slcd + "'";
            if (class1cd == null) class1cd = "";
            if (class1cd != "") if (class1cd.IndexOf("'") < 0) class1cd = "'" + class1cd + "'";

            string sql = "select ";
            if (loccdshow == true) sql += "nvl(a.subloccd,d.loccd) loccd, ";
            else sql += "'' loccd, ";
            sql += "d.compcd, a.glcd, e.glnm, sum(case a.drcr when 'D' then nvl(c.amt,a.amt) when 'C' then nvl(c.amt,a.amt)*-1 end) balamt, ";
            sql += "sum(case a.drcr when 'D' then nvl(c.amt,0) when 'C' then nvl(c.amt,0)*-1 end) class1amt ";
            sql += "from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_hdr b, " + scmf + ".t_vch_class c, " + scmf + ".t_cntrl_hdr d, ";
            sql += scmf + ".m_genleg e ";
            sql += "where a.autono=b.autono and a.autono=c.autono(+) and a.slno=c.dslno(+) and a.autono=d.autono and ";
            sql += "a.drcr in ('D','C') and nvl(d.cancel,'N') = 'N' and a.glcd=e.glcd and ";
            if (glcd != "") sql += "a.glcd in (" + glcd + ") and ";
            if (slcd != "") sql += "a.slcd in (" + slcd + ") and ";
            if (class1cd != "") sql += "(c.class1cd = '" + class1cd + "' or c.class1cd is null) and ";
            if (skipautono != "" && skipautono != null) sql = sql + "d.autono <> '" + skipautono + "' and ";
            if (docdt != null && docdt != "") sql += "d.docdt <= to_date('" + docdt.Substring(0, 10) + "','dd/mm/yyyy') and ";
            if (comploccd == "") sql += "d.compcd='" + COM + "' and ";
            else sql += "d.compcd||d.loccd in (" + comploccd + ") and ";
            if (linkcd != "") sql += "e.linkcd in (" + linkcd + ") and ";
            sql += "b.trcd not in (" + CommVar.SkipTrCd() + ") ";
            sql += "group by ";
            if (loccdshow == true) sql += "nvl(a.subloccd,d.loccd), ";
            else sql += "'', ";
            sql += "d.compcd, a.glcd, e.glnm ";
            sql += "order by glnm, glcd, compcd, loccd ";
            DataTable rsTmp = SQLquery(sql);

            return rsTmp;
        }
        public DataTable GenOSTbl(string selglcd = "", string selslcd = "", string billupto = "", string txnupto = "", string currautono = "",
                 string billfrom = "", string selclass1cd = "", string linkcd = "", string OnlyOSBill = "Y", string selagslcd = "", string selstate = "",
                 string seldistrict = "", string scmf = "", string selflag = "", bool crbaladjauton = false, bool showconslcdasslcd = false, string unselslcd = "",
                 string selslgrpcd = "", bool showagent = false, string blno = "", string skipautono = "", string selrtdebcd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "", sqlc = "";
            try
            {
                DataTable tbl = new DataTable();

                string COM = CommVar.Compcd(UNQSNO);
                if (scmf == "") scmf = CommVar.FinSchema(UNQSNO);
                if (crbaladjauton == false) showagent = false;

                if (currautono.retStr() != "") if (currautono.IndexOf("'") < 0) currautono = "'" + currautono + "'";
                if (billupto.retStr() != "") billupto = Convert.ToDateTime(billupto.Substring(0, 10)).ToString("dd/MM/yyyy");
                if (billfrom.retStr() != "") billfrom = Convert.ToDateTime(billfrom.Substring(0, 10)).ToString("dd/MM/yyyy");
                if (txnupto.retStr() != "") txnupto = Convert.ToDateTime(txnupto.Substring(0, 10)).ToString("dd/MM/yyyy");
                if (selglcd.retStr() != "") if (selglcd.IndexOf("'") < 0) selglcd = "'" + selglcd + "'";
                if (selslcd.retStr() != "") if (selslcd.IndexOf("'") < 0) selslcd = "'" + selslcd + "'";
                if (selclass1cd.retStr() != "") if (selclass1cd.IndexOf("'") < 0) selclass1cd = "'" + selclass1cd + "'";
                if (skipautono.retStr() != "") if (skipautono.IndexOf("'") < 0) skipautono = "'" + skipautono + "'";
                //if (skipautono.retStr() == "") skipautono = "null";
                string curdocautono = currautono;
                if (curdocautono == "") curdocautono = "null";

                //sqlc = "";
                //sqlc += "select a.i_autono||a.i_slno autoslno, ";
                //sqlc += "sum(decode(a.r_autono||a.r_slno," + curdocautono + ",a.adj_amt,0) * decode(c.drcr,d.linkcd,1,-1)) cur_adj, ";
                //sqlc += "sum(decode(a.r_autono||a.r_slno," + curdocautono + ",0,a.adj_amt) * decode(c.drcr,d.linkcd,1,-1)) adj_amt ";
                //sqlc += "from " + scmf + ".t_vch_bl_adj a, " + scmf + ".t_cntrl_hdr b, " + scmf + ".t_vch_bl c, " + scmf + ".m_genleg d ";
                //sqlc += "where a.i_autono||a.i_slno=c.autono||c.slno and c.glcd=d.glcd and ";
                //if (txnupto.retStr() != "") sqlc += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
                //sqlc += "a.autono=b.autono and nvl(b.cancel,'N')='N' and b.compcd='" + COM + "' ";
                //sqlc += "group by a.i_autono||a.i_slno ";

                sqlc = "";
                sqlc += "select a.i_autono||a.i_slno autoslno, ";
                sqlc += "max((case when a.r_autono||a.r_slno in (" + curdocautono + ") then a.pymtrem when a.autono in (" + (skipautono.retStr() == "" ? "null" : skipautono) + ") then a.pymtrem else null end)) pymtrem, ";
                //sqlc += "sum((case when a.r_autono||a.r_slno in (" + curdocautono + ") then a.adj_amt when a.autono in (" + skipautono + ") and (a.autono <> a.i_autono or a.autono <> a.r_autono) then a.adj_amt else 0 end) * decode(c.drcr,d.linkcd,1,-1)) cur_adj, ";
                //sqlc += "sum((case when a.r_autono||a.r_slno in (" + curdocautono + ") then 0 when a.autono in (" + skipautono + ")  and (a.autono <> a.i_autono or a.autono <> a.r_autono) then 0 else a.adj_amt end) * decode(c.drcr,d.linkcd,1,-1)) adj_amt ";
                //sqlc += "sum((case when a.r_autono||a.r_slno in (" + curdocautono + ") then a.adj_amt else 0 end) * decode(c.drcr,d.linkcd,1,-1)) cur_adj, ";
                sqlc += "sum((case when a.r_autono||a.r_slno in (" + curdocautono + ") then 0 when a.autono in (" + (skipautono.retStr() == "" ? "null" : skipautono) + ") then 0 else a.adj_amt end) * decode(c.drcr,d.linkcd,1,-1)) adj_amt ";
                sqlc += "from " + scmf + ".t_vch_bl_adj a, " + scmf + ".t_cntrl_hdr b, " + scmf + ".t_vch_bl c, " + scmf + ".m_genleg d ";
                sqlc += "where a.i_autono||a.i_slno=c.autono||c.slno and c.glcd=d.glcd and ";
                if (txnupto.retStr() != "") sqlc += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
                //if (skipautono.retStr() != "") sqlc += "a.autono not in (" + skipautono + ") and ";
                sqlc += "a.autono=b.autono and nvl(b.cancel,'N')='N' and b.compcd='" + COM + "' ";
                sqlc += "group by a.i_autono||a.i_slno ";

                string oraslcd = "a.slcd";
                if (showconslcdasslcd == true) oraslcd = "decode(k.pslcd,null,a.slcd,nvl(a.conslcd,a.slcd))";

                sql = "";
                sql += "select a.glslcd, a.glslagcd, a.glcd, a.slcd, a.slcdclass1cd, s.parentcd, s.parentnm, s.grpcdfull, a.glslagdrcr, ";
                sql += "a.rtdebcd, l.rtdebnm, l.mobile rtdebmobile, nvl(l.area,l.city) retdebarea, m.docno tchdocno, ";
                sql += "a.drcr, a.autono, a.autoslno, a.doccd, a.docno, a.docdt, a.slno, nvl(a.blamt,0)-nvl(a.amt,0) oprecdamt, a.amt, a.blamt, a.itamt, a.lrno, a.lrdt, a.transnm, ";
                if (selslgrpcd.retStr() != "") sql += "s.parentcd agslcd, s.parentnm agslnm, '' agslcity, '' agphno, ";
                else sql += "a.agslcd, i.slnm agslnm,i.shortnm agshortnm, i.district agslcity, nvl(i.regmobile,i.phno1) agphno, ";
                sql += "a.blno, a.bldt, nvl(a.bldt1,a.docdt) bldt1, a.ordno, a.orddt, to_char(nvl(a.duedt,a.docdt),'dd/mm/yyyy') duedt, a.crdays, a.bal_amt, a.class1cd, a.prv_adj, a.cur_adj, ";
                sql += "g.class1nm, a.vchtype, a.bltype, a.loccd, a.conslcd, a.pymtrem, a.blrem, b.blrem billrem, ";
                sql += "e.glnm, e.linkcd, f.slnm, nvl(f.slarea,f.district) slcity, nvl(f.regmobile,f.phno1) phno, j.slnm conslnm, nvl(j.slarea,j.district) conslarea from ";

                //single query starts
                sql += "(select a.glcd||" + oraslcd + " glslcd, a.glcd||" + oraslcd + (showagent == true ? "||nvl(a.agslcd,'')" : "") + " glslagcd, a.glcd, " + oraslcd + " slcd, ";
                sql += oraslcd + "||nvl(nvl(a.class1cd,d.class1cda),' ') slcdclass1cd, a.conslcd, a.flag, c.pymtrem, a.rtdebcd, ";
                //sql += "a.glcd||" + oraslcd + "||" + (showagent == true? "nvl(a.agslcd, '')||":"") + "(case when (a.amt-nvl(c.adj_amt,0)-nvl(c.cur_adj,0)) * decode(a.drcr,e.linkcd,1,-1) < 0 then 'C' else 'D' end) glslagdrcr, ";
                sql += "a.glcd||" + oraslcd + "||" + (showagent == true ? "nvl(a.agslcd, '')||" : "") + "(case when (a.amt-nvl(c.adj_amt,0)-nvl(f.cur_adj,0)) * decode(a.drcr,e.linkcd,1,-1) < 0 then decode(e.linkcd,'D','C','D') else decode(e.linkcd,'D','D','C') end) glslagdrcr, ";
                sql += "(a.amt-nvl(c.adj_amt,0)-nvl(f.cur_adj,0)) * decode(a.drcr,e.linkcd,1,-1) bal_amt, ";
                sql += "a.drcr, a.autono, a.autono||a.slno autoslno, b.doccd, a.docno, b.docdt, a.slno, nvl(a.amt,0) * decode(a.drcr,e.linkcd,1,-1) amt, ";
                sql += "a.blamt, a.itamt, a.lrno, to_char(a.lrdt,'dd/mm/yyyy') lrdt, a.transnm, a.agslcd, a.blrem, ";
                sql += "a.blno, to_char(a.bldt,'dd/mm/yyyy') bldt, a.bldt bldt1, a.ordno, nvl(to_char(a.orddt,'dd/mm/yyyy'),'') orddt, ";
                sql += "nvl(a.duedt,a.bldt) duedt, nvl(a.crdays,0) crdays, ";
                sql += "nvl(c.adj_amt,0) * decode(a.drcr,e.linkcd,1,-1) prv_adj, nvl(f.cur_adj,0) * decode(a.drcr,e.linkcd,1,-1) cur_adj, ";
                sql += "nvl(a.class1cd,d.class1cda) class1cd, a.vchtype, a.bltype, b.loccd ";
                sql += "from " + scmf + ".t_vch_bl a, " + scmf + ".t_cntrl_hdr b, ";
                sql += "( ";
                sql += "select a.autoslno, a.pymtrem, sum(a.adj_amt) adj_amt from ( ";
                sql += sqlc;

                sql += "union all ";
                sql += sqlc.Replace("i_", "r_").Replace(",1,-1", ",-1,1");

                sql += ") a group by a.autoslno, a.pymtrem ";
                sql += ") c, ";

                sql += "(select a.autono||a.dslno autoslno, max(a.class1cd) class1cda ";
                sql += "from " + scmf + ".t_vch_class a ";
                sql += "where a.datatag='BL' ";
                sql += "group by a.autono||a.dslno ) d, ";

                sql += "( select a.autoslno, a.adj_amt cur_adj from ";
                sql += "(select a.i_autono||i_slno autoslno, sum(case when nvl(a.adj_amt,0) < 0 then a.adj_amt*-1 else a.adj_amt end) adj_amt from " + scmf + ".t_vch_bl_adj a where a.autono in (";
                if (skipautono.retStr() != "") sql += skipautono; else sql += "'xx'";
                sql += ") group by a.i_autono||i_slno ";
                sql += "union all ";
                sql += "select a.r_autono||r_slno autoslno, sum(case when nvl(a.adj_amt,0) < 0 then a.adj_amt*-1 else a.adj_amt end) adj_amt from " + scmf + ".t_vch_bl_adj a where a.autono in (";
                if (skipautono.retStr() != "") sql += skipautono; else sql += "'xx'";
                sql += ") group by a.r_autono||r_slno ) a ";
                sql += " ) f, ";

                sql += scmf + ".m_genleg e, " + scmf + ".m_subleg k ";
                sql += "where a.autono=b.autono and a.autono||a.slno=c.autoslno(+) and a.glcd=e.glcd(+) and a.slcd=k.slcd(+) and ";
                if (billfrom.retStr() != "") sql += "b.docdt >= to_date('" + billfrom + "','dd/mm/yyyy') and ";
                if (billupto.retStr() != "") sql += "b.docdt <= to_date('" + billupto + "','dd/mm/yyyy') and ";
                if (txnupto.retStr() != "") sql += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
                if (selglcd.retStr() != "") sql += "a.glcd in (" + selglcd + ") and ";
                if (selslcd.retStr() != "") sql += "a.slcd in (" + selslcd + ") and ";
                if (selrtdebcd.retStr() != "") sql += "a.rtdebcd in (" + selrtdebcd + ") and ";
                if (unselslcd.retStr() != "") sql += "a.slcd not in (" + unselslcd + ") and ";
                if (selclass1cd.retStr() != "") sql += "nvl(a.class1cd,d.class1cda) in (" + selclass1cd + ") and ";
                if (selagslcd.retStr() != "") sql += "a.agslcd in (" + selagslcd + ") and ";
                if (OnlyOSBill == "A") sql += "";
                else if (OnlyOSBill == "N") sql += "(a.amt-nvl(c.adj_amt,0)-nvl(f.cur_adj,0) = 0 or nvl(f.cur_adj,0) <> 0) and ";
                else sql += "(a.amt-nvl(c.adj_amt,0)-nvl(f.cur_adj,0) <> 0 or nvl(f.cur_adj,0) <> 0) and ";
                if (currautono.retStr() != "") sql += "a.autono||a.slno not in (" + currautono + ") and ";
                if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
                sql += "nvl(b.cancel,'N')='N' and b.compcd='" + COM + "' and ";
                sql += "a.autono||a.slno=d.autoslno(+) and a.autono||a.slno=f.autoslno(+) ) a, ";
                //single query end

                //Get Bill wise Remarks
                sql += "( select a.autono, a.slno, a.autoslno, nvl(b.pymtrem, a.blrem) blrem from ";
                sql += "( select a.autono, a.slno, a.autono||a.slno autoslno, a.blrem ";
                sql += "from " + scmf + ".t_vch_bl a, " + scmf + ".t_cntrl_hdr b ";
                sql += "where a.autono=b.autono(+) and b.compcd='" + COM + "') a, ";
                sql += "( select autoslno, pymtrem from ";
                sql += "( select a.i_autono||a.i_slno autoslno, b.docdt, a.pymtrem, ";
                sql += "row_number() over (partition by a.i_autono, a.i_slno order by b.docdt desc) as rn ";
                sql += "from " + scmf + ".t_vch_bl_adj a, " + scmf + ".t_cntrl_hdr b ";
                sql += "where a.autono=b.autono(+) and b.compcd='" + COM + "' and a.pymtrem is not null ) where rn=1) b ";
                sql += "where a.autoslno=b.autoslno(+) ) b, ";
                //

                sql += "(select a.grpcd, a.parentcd, c.slcdgrpnm||'  '||b.slcdgrpnm parentnm, ";
                sql += "a.grpcdfull, a.slcdgrpnm, a.class1cd, a.slcd, a.slcdclass1cd from ";

                sql += "(select a.grpcd, a.slcdgrpcd parentcd, a.parentcd rootcd, a.grpcdfull, a.slcdgrpnm, b.class1cd, a.slcd, ";
                sql += "a.slcd||nvl(b.class1cd,' ') slcdclass1cd ";
                sql += "from " + scmf + ".m_subleg_grp a, " + scmf + ".m_subleg_grpclass1 b ";
                sql += "where a.grpcd=b.grpcd(+) and a.slcd is not null ) a, ";

                sql += "(select distinct a.slcdgrpcd, a.slcdgrpnm, a.grpcdfull ";
                sql += "from " + scmf + ".m_subleg_grp a where a.slcd is null) b, ";

                sql += "(select distinct a.slcdgrpcd, a.slcdgrpnm, a.grpcdfull ";
                sql += "from " + scmf + ".m_subleg_grp a where a.slcd is null) c ";

                sql += "where a.rootcd = c.slcdgrpcd(+) and a.parentcd=b.slcdgrpcd(+) ) s, ";

                //sql += "( select a.grpcd, a.slcdgrpcd parentcd, d.slcdgrpnm||'  '||c.slcdgrpnm parentnm, a.grpcdfull, a.slcdgrpnm, b.class1cd, ";
                //sql += "a.slcd, a.slcd||nvl(b.class1cd,' ') slcdclass1cd ";
                //sql += "from " + scmf + ".m_subleg_grp a, " + scmf + ".m_subleg_grpclass1 b, " + scmf + ".m_subleg_grp c, " + scmf + ".m_subleg_grp d ";
                //sql += "where a.parentcd||a.slcdgrpcd = c.grpcdfull(+) and a.parentcd = d.grpcdfull(+) and a.grpcd=b.grpcd and a.slcd is not null ) s, ";

                sql += scmf + ".m_genleg e, " + scmf + ".m_subleg f, " + scmf + ".m_class1 g, ";
                sql += scmf + ".m_subleg i, " + scmf + ".m_subleg j, " + scmf + ".m_subleg k, " + scmf + ".m_retdeb l, " + scmf + ".t_cntrl_hdr m ";
                sql += "where a.glcd=e.glcd(+) and a.slcd=f.slcd and a.class1cd=g.class1cd(+) and a.autono=m.autono(+) and ";
                sql += "a.autoslno=b.autoslno(+) and a.rtdebcd=l.rtdebcd(+) and ";
                if (selstate.retStr() != "") sql += "f.state in (" + selstate + ") and ";
                if (seldistrict.retStr() != "") sql += "f.district in (" + seldistrict + ") and ";
                if (selflag.retStr() != "") sql += "a.flag in (" + selflag + ") and ";
                if (selslgrpcd.retStr() != "") sql += "(s.parentcd in (" + selslgrpcd + ") ) and ";
                if (linkcd.retStr() != "") sql += "e.linkcd='" + linkcd + "' and ";
                sql += "a.agslcd=i.slcd(+) and a.conslcd=j.slcd(+) and a.slcd=k.slcd and a.slcdclass1cd=s.slcdclass1cd(+) ";
                if (blno.retStr() != "") sql += "and a.blno='" + blno + "' ";

                sql += "order by glcd,slnm,slcd,bldt1,blno,docdt,docno";

                if (crbaladjauton == false) tbl = SQLquery(sql);

                #region Generate table if Credit Balance adjustment (Fifo) true
                if (crbaladjauton == true)
                {
                    #region Datatable structure define
                    tbl.Columns.Add("drcr", typeof(string));
                    tbl.Columns.Add("glslcd", typeof(string));
                    tbl.Columns.Add("glslagcd", typeof(string));
                    tbl.Columns.Add("autono", typeof(string));
                    tbl.Columns.Add("autoslno", typeof(string));
                    tbl.Columns.Add("doccd", typeof(string));
                    tbl.Columns.Add("tchdocno", typeof(string));
                    tbl.Columns.Add("docno", typeof(string));
                    tbl.Columns.Add("docdt", typeof(DateTime));
                    tbl.Columns.Add("slno", typeof(double));
                    tbl.Columns.Add("amt", typeof(double));
                    tbl.Columns.Add("blamt", typeof(double));
                    tbl.Columns.Add("oprecdamt", typeof(double));
                    tbl.Columns.Add("itamt", typeof(double));
                    tbl.Columns.Add("lrno", typeof(string));
                    tbl.Columns.Add("lrdt", typeof(string));
                    tbl.Columns.Add("transnm", typeof(string));
                    tbl.Columns.Add("agslcd", typeof(string));
                    tbl.Columns.Add("agslnm", typeof(string));
                    tbl.Columns.Add("agshortnm", typeof(string));
                    tbl.Columns.Add("agslcity", typeof(string));
                    tbl.Columns.Add("agphno", typeof(double));
                    tbl.Columns.Add("blno", typeof(string));
                    tbl.Columns.Add("bldt", typeof(string));
                    tbl.Columns.Add("bldt1", typeof(string));
                    tbl.Columns.Add("ordno", typeof(string));
                    tbl.Columns.Add("orddt", typeof(string));
                    tbl.Columns.Add("duedt", typeof(string));
                    tbl.Columns.Add("crdays", typeof(double));
                    tbl.Columns.Add("prv_adj", typeof(double));
                    tbl.Columns.Add("cur_adj", typeof(double));
                    tbl.Columns.Add("bal_amt", typeof(double));
                    tbl.Columns.Add("glcd", typeof(string));
                    tbl.Columns.Add("slcd", typeof(string));
                    tbl.Columns.Add("class1cd", typeof(string));
                    tbl.Columns.Add("class1nm", typeof(string));
                    tbl.Columns.Add("vchtype", typeof(string));
                    tbl.Columns.Add("bltype", typeof(string));
                    tbl.Columns.Add("glnm", typeof(string));
                    tbl.Columns.Add("linkcd", typeof(string));
                    tbl.Columns.Add("slnm", typeof(string));
                    tbl.Columns.Add("slcity", typeof(string));
                    tbl.Columns.Add("phno", typeof(double));
                    tbl.Columns.Add("loccd", typeof(string));
                    tbl.Columns.Add("conslcd", typeof(string));
                    tbl.Columns.Add("grpcdfull", typeof(string));
                    tbl.Columns.Add("parentcd", typeof(string));
                    tbl.Columns.Add("parentnm", typeof(string));
                    tbl.Columns.Add("pymtrem", typeof(string));
                    tbl.Columns.Add("blrem", typeof(string));
                    tbl.Columns.Add("billrem", typeof(string));
                    tbl.Columns.Add("rtdebcd", typeof(string));
                    tbl.Columns.Add("rtdebarea", typeof(string));
                    tbl.Columns.Add("rtdebnm", typeof(string));
                    tbl.Columns.Add("rtdebmobile", typeof(string));

                    #endregion
                    DataTable rstbl = new DataTable();
                    rstbl = SQLquery(sql);
                    Int32 rNo = 0;

                    string sqltmp = "";
                    sqltmp = "select distinct glslcd, glslagcd, linkcd, glcd, slnm, slcd from ( " + sql + " ) order by glcd, slnm, slcd";
                    DataTable rsglsl = SQLquery(sqltmp);

                    Int32 gi = 0, maxG = rsglsl.Rows.Count - 1;
                    while (gi <= maxG)
                    {
                        string bldrcr = "D", opdrcr = "C";
                        if (rsglsl.Rows[gi]["linkcd"].ToString() == "C")
                        {
                            bldrcr = "C"; opdrcr = "D";
                        }
                        string selglslagcd = rsglsl.Rows[gi]["glslagcd"].ToString();
                        string selglslagbldrcr = selglslagcd + bldrcr;
                        string selglslagopdrcr = selglslagcd + opdrcr;

                        DataTable tbldr = new DataTable();
                        var rowsdr = rstbl.AsEnumerable()
                            .Where(t => ((string)t["glslagdrcr"]) == selglslagbldrcr);
                        if (rowsdr.Any()) tbldr = rowsdr.CopyToDataTable();

                        DataTable tblcr = new DataTable();
                        var rowscr = rstbl.AsEnumerable()
                            .Where(t => ((string)t["glslagdrcr"]) == selglslagopdrcr);
                        if (rowscr.Any()) tblcr = rowscr.CopyToDataTable();

                        //Type tt = tblcr.Rows[0]["amt"].GetType();
                        var cramt = tblcr.AsEnumerable().Sum(dr => dr.Field<decimal>("bal_amt"));
                        double balcramt = Convert.ToDouble(cramt) * -1;
                        Int32 i = 0, maxR = tbldr.Rows.Count - 1;
                        while (i <= maxR)
                        {
                            bool recoins = true;
                            double balamt = Convert.ToDouble(tbldr.Rows[i]["bal_amt"] == DBNull.Value ? 0 : tbldr.Rows[i]["bal_amt"]);
                            if (balamt < balcramt)
                            {
                                recoins = false;
                                balcramt = balcramt - balamt;
                            }
                            else
                            {
                                balamt = balamt - balcramt;
                                balcramt = 0;
                                recoins = true;
                            }
                            if (recoins == true)
                            {
                                tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                                tbl.Rows[rNo]["drcr"] = tbldr.Rows[i]["drcr"].ToString();
                                tbl.Rows[rNo]["autono"] = tbldr.Rows[i]["autono"].ToString();
                                tbl.Rows[rNo]["autoslno"] = tbldr.Rows[i]["autoslno"].ToString();
                                tbl.Rows[rNo]["doccd"] = tbldr.Rows[i]["doccd"].ToString();
                                tbl.Rows[rNo]["tchdocno"] = tbldr.Rows[i]["tchdocno"].ToString();
                                tbl.Rows[rNo]["docno"] = tbldr.Rows[i]["docno"].ToString();
                                tbl.Rows[rNo]["docdt"] = Convert.ToDateTime(tbldr.Rows[i]["docdt"]);
                                tbl.Rows[rNo]["slno"] = Convert.ToDouble(tbldr.Rows[i]["slno"] == DBNull.Value ? 0 : tbldr.Rows[i]["slno"]);
                                tbl.Rows[rNo]["amt"] = Convert.ToDouble(tbldr.Rows[i]["amt"] == DBNull.Value ? 0 : tbldr.Rows[i]["amt"]);
                                tbl.Rows[rNo]["blamt"] = Convert.ToDouble(tbldr.Rows[i]["blamt"] == DBNull.Value ? 0 : tbldr.Rows[i]["blamt"]);
                                tbl.Rows[rNo]["oprecdamt"] = Convert.ToDouble(tbldr.Rows[i]["oprecdamt"] == DBNull.Value ? 0 : tbldr.Rows[i]["oprecdamt"]);
                                tbl.Rows[rNo]["itamt"] = Convert.ToDouble(tbldr.Rows[i]["itamt"] == DBNull.Value ? 0 : tbldr.Rows[i]["itamt"]);
                                tbl.Rows[rNo]["blrem"] = tbldr.Rows[i]["blrem"].ToString();
                                tbl.Rows[rNo]["billrem"] = tbldr.Rows[i]["billrem"].ToString();
                                tbl.Rows[rNo]["lrno"] = tbldr.Rows[i]["lrno"].ToString();
                                tbl.Rows[rNo]["lrdt"] = tbldr.Rows[i]["lrdt"].ToString();
                                tbl.Rows[rNo]["transnm"] = tbldr.Rows[i]["transnm"].ToString();
                                tbl.Rows[rNo]["agslcd"] = tbldr.Rows[i]["agslcd"].ToString();
                                tbl.Rows[rNo]["agslnm"] = tbldr.Rows[i]["agslnm"].ToString();
                                tbl.Rows[rNo]["agshortnm"] = tbldr.Rows[i]["agshortnm"].ToString();
                                tbl.Rows[rNo]["agslcity"] = tbldr.Rows[i]["agslcity"].ToString();
                                tbl.Rows[rNo]["agphno"] = Convert.ToDouble(tbldr.Rows[i]["agphno"] == DBNull.Value ? 0 : tbldr.Rows[i]["agphno"]);
                                tbl.Rows[rNo]["blno"] = tbldr.Rows[i]["blno"].ToString();
                                tbl.Rows[rNo]["bldt"] = tbldr.Rows[i]["bldt"].ToString();
                                tbl.Rows[rNo]["bldt1"] = tbldr.Rows[i]["bldt1"].ToString();
                                tbl.Rows[rNo]["ordno"] = tbldr.Rows[i]["ordno"].ToString();
                                tbl.Rows[rNo]["orddt"] = tbldr.Rows[i]["orddt"].ToString();
                                tbl.Rows[rNo]["duedt"] = tbldr.Rows[i]["duedt"].ToString();
                                tbl.Rows[rNo]["crdays"] = Convert.ToDouble(tbldr.Rows[i]["crdays"] == DBNull.Value ? 0 : tbldr.Rows[i]["crdays"]);
                                tbl.Rows[rNo]["prv_adj"] = Convert.ToDouble(tbldr.Rows[i]["prv_adj"] == DBNull.Value ? 0 : tbldr.Rows[i]["prv_adj"]);
                                tbl.Rows[rNo]["cur_adj"] = Convert.ToDouble(tbldr.Rows[i]["cur_adj"] == DBNull.Value ? 0 : tbldr.Rows[i]["cur_adj"]);
                                tbl.Rows[rNo]["bal_amt"] = balamt;
                                tbl.Rows[rNo]["glcd"] = tbldr.Rows[i]["glcd"].ToString();
                                tbl.Rows[rNo]["slcd"] = tbldr.Rows[i]["slcd"].ToString();
                                tbl.Rows[rNo]["class1cd"] = tbldr.Rows[i]["class1cd"].ToString();
                                tbl.Rows[rNo]["class1nm"] = tbldr.Rows[i]["class1nm"].ToString();
                                tbl.Rows[rNo]["vchtype"] = tbldr.Rows[i]["vchtype"].ToString();
                                tbl.Rows[rNo]["bltype"] = tbldr.Rows[i]["bltype"].ToString();
                                tbl.Rows[rNo]["glnm"] = tbldr.Rows[i]["glnm"].ToString();
                                tbl.Rows[rNo]["linkcd"] = tbldr.Rows[i]["linkcd"].ToString();
                                tbl.Rows[rNo]["slnm"] = tbldr.Rows[i]["slnm"].ToString();
                                tbl.Rows[rNo]["slcity"] = tbldr.Rows[i]["slcity"].ToString();
                                tbl.Rows[rNo]["phno"] = Convert.ToDouble(tbldr.Rows[i]["phno"] == DBNull.Value ? 0 : tbldr.Rows[i]["phno"]);
                                tbl.Rows[rNo]["loccd"] = tbldr.Rows[i]["loccd"].ToString();
                                tbl.Rows[rNo]["conslcd"] = tbldr.Rows[i]["conslcd"].ToString();
                                tbl.Rows[rNo]["parentcd"] = tbldr.Rows[i]["parentcd"].ToString();
                                tbl.Rows[rNo]["parentnm"] = tbldr.Rows[i]["parentnm"].ToString();
                                tbl.Rows[rNo]["grpcdfull"] = tbldr.Rows[i]["grpcdfull"].ToString();
                                tbl.Rows[rNo]["pymtrem"] = tbldr.Rows[i]["pymtrem"].ToString();
                                tbl.Rows[rNo]["rtdebcd"] = tbldr.Rows[i]["rtdebcd"].ToString();
                                tbl.Rows[rNo]["rtdebnm"] = tbldr.Rows[i]["rtdebnm"].ToString();
                                tbl.Rows[rNo]["rtdebmobile"] = tbldr.Rows[i]["rtdebmobile"].ToString();
                            }
                            i++;
                        }
                        #region if credit balance excess then debit
                        if (balcramt != 0 && tblcr.Rows.Count != 0)
                        {
                            i = tblcr.Rows.Count - 1; maxR = tblcr.Rows.Count - 1;
                            while (i >= maxR)
                            {
                                bool recoins = false;
                                double balamt = Convert.ToDouble(tblcr.Rows[i]["bal_amt"] == DBNull.Value ? 0 : tblcr.Rows[i]["bal_amt"]) * -1;
                                if (balcramt != 0)
                                {
                                    if (balamt < balcramt)
                                    {
                                        recoins = true;
                                        balamt = balcramt;
                                        balcramt = 0;
                                    }
                                    else
                                    {
                                        balamt = balcramt;
                                        balcramt = 0;
                                        recoins = true;
                                    }
                                }
                                if (recoins == true)
                                {
                                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                                    tbl.Rows[rNo]["drcr"] = tblcr.Rows[i]["drcr"].ToString();
                                    tbl.Rows[rNo]["autono"] = tblcr.Rows[i]["autono"].ToString();
                                    tbl.Rows[rNo]["autoslno"] = tblcr.Rows[i]["autoslno"].ToString();
                                    tbl.Rows[rNo]["doccd"] = tblcr.Rows[i]["doccd"].ToString();
                                    tbl.Rows[rNo]["tchdocno"] = tblcr.Rows[i]["docno"].ToString();
                                    tbl.Rows[rNo]["docno"] = tblcr.Rows[i]["docno"].ToString();
                                    tbl.Rows[rNo]["docdt"] = Convert.ToDateTime(tblcr.Rows[i]["docdt"]);
                                    tbl.Rows[rNo]["slno"] = Convert.ToDouble(tblcr.Rows[i]["slno"] == DBNull.Value ? 0 : tblcr.Rows[i]["slno"]);
                                    tbl.Rows[rNo]["amt"] = Convert.ToDouble(tblcr.Rows[i]["amt"] == DBNull.Value ? 0 : tblcr.Rows[i]["amt"]);
                                    tbl.Rows[rNo]["blamt"] = Convert.ToDouble(tblcr.Rows[i]["blamt"] == DBNull.Value ? 0 : tblcr.Rows[i]["blamt"]);
                                    tbl.Rows[rNo]["itamt"] = Convert.ToDouble(tblcr.Rows[i]["itamt"] == DBNull.Value ? 0 : tblcr.Rows[i]["itamt"]);
                                    tbl.Rows[rNo]["lrno"] = tblcr.Rows[i]["lrno"].ToString();
                                    tbl.Rows[rNo]["lrdt"] = tblcr.Rows[i]["lrdt"].ToString();
                                    tbl.Rows[rNo]["transnm"] = tblcr.Rows[i]["transnm"].ToString();
                                    tbl.Rows[rNo]["agslcd"] = tblcr.Rows[i]["agslcd"].ToString();
                                    tbl.Rows[rNo]["agslnm"] = tblcr.Rows[i]["agslnm"].ToString();
                                    tbl.Rows[rNo]["agshortnm"] = tblcr.Rows[i]["agshortnm"].ToString();
                                    tbl.Rows[rNo]["agslcity"] = tblcr.Rows[i]["agslcity"].ToString();
                                    tbl.Rows[rNo]["agphno"] = Convert.ToDouble(tblcr.Rows[i]["agphno"] == DBNull.Value ? 0 : tblcr.Rows[i]["agphno"]);
                                    tbl.Rows[rNo]["blno"] = tblcr.Rows[i]["blno"].ToString();
                                    tbl.Rows[rNo]["bldt"] = tblcr.Rows[i]["bldt"].ToString();
                                    tbl.Rows[rNo]["bldt1"] = tblcr.Rows[i]["bldt1"].ToString();
                                    tbl.Rows[rNo]["ordno"] = tblcr.Rows[i]["ordno"].ToString();
                                    tbl.Rows[rNo]["orddt"] = tblcr.Rows[i]["orddt"].ToString();
                                    tbl.Rows[rNo]["duedt"] = tblcr.Rows[i]["duedt"].ToString();
                                    tbl.Rows[rNo]["crdays"] = Convert.ToDouble(tblcr.Rows[i]["crdays"] == DBNull.Value ? 0 : tblcr.Rows[i]["crdays"]);
                                    tbl.Rows[rNo]["prv_adj"] = Convert.ToDouble(tblcr.Rows[i]["prv_adj"] == DBNull.Value ? 0 : tblcr.Rows[i]["prv_adj"]);
                                    tbl.Rows[rNo]["cur_adj"] = Convert.ToDouble(tblcr.Rows[i]["cur_adj"] == DBNull.Value ? 0 : tblcr.Rows[i]["cur_adj"]);
                                    tbl.Rows[rNo]["bal_amt"] = balamt * -1;
                                    tbl.Rows[rNo]["glcd"] = tblcr.Rows[i]["glcd"].ToString();
                                    tbl.Rows[rNo]["slcd"] = tblcr.Rows[i]["slcd"].ToString();
                                    tbl.Rows[rNo]["class1cd"] = tblcr.Rows[i]["class1cd"].ToString();
                                    tbl.Rows[rNo]["class1nm"] = tblcr.Rows[i]["class1nm"].ToString();
                                    tbl.Rows[rNo]["vchtype"] = tblcr.Rows[i]["vchtype"].ToString();
                                    tbl.Rows[rNo]["bltype"] = tblcr.Rows[i]["bltype"].ToString();
                                    tbl.Rows[rNo]["glnm"] = tblcr.Rows[i]["glnm"].ToString();
                                    tbl.Rows[rNo]["linkcd"] = tblcr.Rows[i]["linkcd"].ToString();
                                    tbl.Rows[rNo]["slnm"] = tblcr.Rows[i]["slnm"].ToString();
                                    tbl.Rows[rNo]["slcity"] = tblcr.Rows[i]["slcity"].ToString();
                                    tbl.Rows[rNo]["phno"] = Convert.ToDouble(tblcr.Rows[i]["phno"] == DBNull.Value ? 0 : tblcr.Rows[i]["phno"]);
                                    tbl.Rows[rNo]["loccd"] = tblcr.Rows[i]["loccd"].ToString();
                                    tbl.Rows[rNo]["conslcd"] = tblcr.Rows[i]["conslcd"].ToString();
                                    tbl.Rows[rNo]["parentcd"] = tblcr.Rows[i]["parentcd"].ToString();
                                    tbl.Rows[rNo]["parentnm"] = tblcr.Rows[i]["parentnm"].ToString();
                                    tbl.Rows[rNo]["grpcdfull"] = tblcr.Rows[i]["grpcdfull"].ToString();
                                    tbl.Rows[rNo]["rtdebcd"] = tblcr.Rows[i]["rtdebcd"].ToString();
                                    tbl.Rows[rNo]["rtdebnm"] = tblcr.Rows[i]["rtdebnm"].ToString();
                                    tbl.Rows[rNo]["rtdebmobile"] = tblcr.Rows[i]["rtdebmobile"].ToString();
                                }
                                i--;
                            }
                        }
                        #endregion
                        gi++;
                    }
                }
                #endregion
                return tbl;
            }
            catch (Exception Ex)
            {
                DataTable tbl = new DataTable();
                tbl.Columns.Add("exception", typeof(string));
                tbl.Rows.Add("");
                tbl.Rows[0][0] = Ex.Message + Cn.GCS() + sql;
                return tbl;
            }
        }

        public string CheckOSTbl(DataTable tbl)
        {
            string msg = "OK";
            if (tbl.Columns.Count == 1)
            {
                msg = tbl.Rows[0][0].ToString();
            }
            return msg;
        }
        public string InsITC4_Iss(string autono, string doccd, string docno, string docdt, short slno, string sheetnm, string proc_autono,
                string slcd, string gstno, string chno, string chdt, string hsncode, string itnm, string uomcd,
                double qnty, double amt, double cgstper, double sgstper, double igstper, double cessper = 0, double cessamt = 0)
        {
            string bl = ""; var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {
                string scmf = CommVar.FinSchema(UNQSNO);
                string clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";
                if (hsncode == "") hsncode = "00";

                sql = "insert into " + scmf + ".t_gstitc4_iss (clcd, autono, doccd, docno, docdt, slno, sheetnm, proc_autono, slcd, gstno, chno, chdt, hsncode, itnm, uomcd, qnty, amt, cgstper, sgstper, igstper, cessper, cessamt) values (";
                sql = sql + filc(clcd);
                sql = sql + "," + filc(autono);
                sql = sql + "," + filc(doccd);
                sql = sql + "," + filc(docno);
                sql = sql + "," + fild(docdt);
                sql = sql + "," + slno;
                sql = sql + "," + filc(sheetnm);
                sql = sql + "," + filc(proc_autono);
                sql = sql + "," + filc(slcd);
                sql = sql + "," + filc(gstno);
                sql = sql + "," + filc(chno);
                sql = sql + "," + fild(chdt);
                sql = sql + "," + filc(hsncode);
                sql = sql + "," + filc(itnm);
                sql = sql + "," + filc(uomcd);
                sql = sql + "," + qnty;
                sql = sql + "," + amt;
                sql = sql + "," + cgstper;
                sql = sql + "," + sgstper;
                sql = sql + "," + igstper;
                sql = sql + "," + cessper;
                sql = sql + "," + cessamt;
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }

        public string InsITC4_Rec(string autono, string doccd, string docno, string docdt, short slno, string sheetnm, string proc_autono,
                  string slcd, string gstno, string jobnm, string orgchno, string orgchdt, string chno, string chdt, string hsncode, string itnm, string uomcd,
                  double qnty, double amt, double shortqnty, string shortuomcd)
        {
            string bl = ""; var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {
                string scmf = CommVar.FinSchema(UNQSNO);
                string clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";
                if (hsncode == "") hsncode = "00";

                sql = "insert into " + scmf + ".t_gstitc4_rec (clcd, autono, doccd, docno, docdt, slno, sheetnm, proc_autono, slcd, gstno, jobnm,  orgchno, orgchdt, chno, chdt, hsncode, itnm, uomcd, qnty, amt, shortqnty, shortuomcd) values (";
                sql = sql + filc(clcd);
                sql = sql + "," + filc(autono);
                sql = sql + "," + filc(doccd);
                sql = sql + "," + filc(docno);
                sql = sql + "," + fild(docdt);
                sql = sql + "," + slno;
                sql = sql + "," + filc(sheetnm);
                sql = sql + "," + filc(proc_autono);
                sql = sql + "," + filc(slcd);
                sql = sql + "," + filc(gstno);
                sql = sql + "," + filc(jobnm);
                sql = sql + "," + filc(orgchno);
                sql = sql + "," + fild(orgchdt);
                sql = sql + "," + filc(chno);
                sql = sql + "," + fild(chdt);
                sql = sql + "," + filc(hsncode);
                sql = sql + "," + filc(itnm);
                sql = sql + "," + filc(uomcd);
                sql = sql + "," + qnty;
                sql = sql + "," + amt;
                sql = sql + "," + shortqnty;
                sql = sql + "," + filc(shortuomcd);
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }
        public string TblUpdt(string tblname, string autono, string dtag, string modcd = "", string whereClause = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            if (autono.IndexOf("'") < 0) autono = "'" + autono + "'";

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
            if (whereClause.ToString() == "") whereClause = "autono in (" + autono + ")";
            sql += "update " + scmf + "." + tblname + " set dtag='" + dtag + "' where " + whereClause + "~";
            sql += "delete from " + scmf + "." + tblname + " where  " + whereClause;
            return sql;
        }
        public string T_Cntrl_Hdr_Updt_Ins(string autono, string vchrmod, string modcd, string mnthcd, string doccd, string docno, string docdt, short? emd_no, string doconlyno, double? vchrno,
                string calauto = null, string vchrprefix = null, string glcd = null, string slcd = null, double? docamt = 0)

        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string bl = "";
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);
            string[] fin = CommVar.FinPeriod(UNQSNO).Split('-');
            string YEARCD = fin[0].Substring(6).Trim();
            string uid = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
            string uIP = Cn.GetIp();
            try
            {
                string scmf = "";
                switch (modcd)
                {
                    case "F":
                        scmf = CommVar.FinSchema(UNQSNO); break;
                    case "I":
                        scmf = CommVar.InvSchema(UNQSNO); break;
                    case "S":
                        scmf = CommVar.CurSchema(UNQSNO); break;
                }
                string clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";
                string pkgmodcd = "S"; //System.Web.HttpContext.Current.Session["ModuleCode"].ToString();

                string _docdt = "to_date('" + docdt.ToString().Substring(0, 10).Replace("-", "/") + "','dd/mm/yyyy') ";
                //if (vchrmod == "E")
                //{
                DataTable rsTmp = SQLquery("select autono from " + scmf + ".t_cntrl_hdr where autono='" + autono + "'");
                if (rsTmp.Rows.Count <= 0) vchrmod = "A";
                //}

                if (vchrmod == "A")
                {
                    sql = "insert into " + scmf + ".t_cntrl_hdr (emd_no, clcd, dtag, ttag, autono, modcd, compcd, loccd, yr_cd, doccd, docno, docdt, doconlyno, vchrno, vchrsuffix, mnthcd, ";
                    sql = sql + "calauto, glcd, slcd, docamt, usr_id, usr_entdt, usr_lip, usr_sip, usr_os, usr_mnm) values (";
                    sql = sql + filnd(emd_no);
                    sql = sql + "," + filc(clcd);
                    sql = sql + "," + filc(null);
                    sql = sql + "," + filc(null);
                    sql = sql + "," + filc(autono);
                    sql = sql + "," + filc(pkgmodcd);
                    sql = sql + "," + filc(COM);
                    sql = sql + "," + filc(LOC);
                    sql = sql + "," + filc(YEARCD);
                    sql = sql + "," + filc(doccd);
                    sql = sql + "," + filc(docno);
                    sql = sql + "," + _docdt;
                    sql = sql + "," + filc(doconlyno);
                    sql = sql + "," + vchrno;
                    sql = sql + "," + filc(vchrprefix);
                    sql = sql + "," + filc(mnthcd);
                    sql = sql + "," + filc(calauto);
                    sql = sql + "," + filc(glcd);
                    sql = sql + "," + filc(slcd);
                    sql = sql + "," + docamt;
                    sql = sql + "," + filc(uid);
                    sql = sql + ", SYSDATE ";
                    sql = sql + "," + filc(uIP);
                    sql = sql + "," + filc(Cn.GetStaticIp());
                    sql = sql + "," + filc(null);
                    sql = sql + "," + filc(Cn.DetermineCompName(uIP));
                    sql = sql + ")";
                }
                else if (vchrmod == "E")
                {
                    sql = "update " + scmf + ".t_cntrl_hdr set ";
                    sql = sql + "emd_no=" + filnd(emd_no);
                    sql = sql + ", dtag=" + filc("E");
                    sql = sql + ", docdt=" + _docdt;
                    if (docno.retStr() != "") sql = sql + ", docno=" + filc(docno);
                    sql = sql + ", doconlyno=" + filc(doconlyno);
                    sql = sql + ", vchrno=" + vchrno;
                    sql = sql + ", vchrsuffix=" + filc(vchrprefix);
                    sql = sql + ", mnthcd=" + filc(mnthcd);
                    sql = sql + ", calauto=" + filc(calauto);
                    sql = sql + ", glcd=" + filc(glcd);
                    sql = sql + ", slcd=" + filc(slcd);
                    sql = sql + ", docamt=" + docamt;
                    sql = sql + ", lm_usr_id=" + filc(uid);
                    sql = sql + ", lm_usr_entdt=SYSDATE";
                    sql = sql + ", lm_usr_lip=" + filc(uIP);
                    sql = sql + ", lm_usr_sip=" + filc(Cn.GetStaticIp());
                    sql = sql + ", lm_usr_os=" + filc(null);
                    sql = sql + ", lm_usr_mnm=" + filc(Cn.DetermineCompName(uIP));
                    sql = sql + " where autono=" + filc(autono);
                }
                else if (vchrmod == "D")
                {
                    sql = "update " + scmf + ".t_cntrl_hdr set ";
                    sql = sql + "dtag=" + filc("D");
                    sql = sql + ", docdt=" + _docdt;
                    if (docno.retStr() != "") sql = sql + ", docno=" + filc(docno);
                    sql = sql + ", del_usr_id=" + filc(uid);
                    sql = sql + ", del_usr_entdt=SYSDATE";
                    sql = sql + ", del_usr_lip=" + filc(uIP);
                    sql = sql + ", del_usr_sip=" + filc(Cn.GetStaticIp());
                    sql = sql + ", del_usr_os=" + filc(null);
                    sql = sql + ", del_usr_mnm=" + filc(Cn.DetermineCompName(uIP));
                    sql = sql + " where autono=" + filc(autono);
                    sql = sql + "~" + "delete from " + scmf + ".t_cntrl_hdr where autono in ('" + autono + "') ";
                }

                bl = sql;
                return bl;
            }
            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }
        public string RetModeltoSql(object model, string mode = "A", string scm = "", string wherecluase = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "", sql1 = "", sql2 = "", tblnm = "";
            if (scm == "") scm = CommVar.CurSchema(UNQSNO);
            try
            {
                foreach (PropertyInfo prop in model.GetType().GetProperties())
                {
                    if (tblnm == "") tblnm = prop.ReflectedType.Name;
                    var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    if (sql1 != "") sql1 += ", ";
                    if (sql2 != "" && mode == "A") sql2 += ", ";
                    sql1 += prop.Name.ToString();

                    var tyy = prop.GetValue(model, null); //.ToString();
                    if (mode == "E" && wherecluase == "" && prop.Name == "AUTONO") wherecluase = "autono='" + tyy.retStr() + "'";
                    if (type == typeof(DateTime))
                    {
                        if (mode == "A") sql2 += fild(tyy.retStr()); else sql1 += "=" + fild(tyy.retStr());
                    }
                    else if (type == typeof(double))
                    {
                        var varval = (tyy == null ? "null" : tyy.ToString());
                        if (mode == "A") sql2 += varval; else sql1 += "=" + varval;
                    }
                    else if (type == typeof(decimal))
                    {
                        var varval = (tyy == null ? "null" : tyy.ToString());
                        if (mode == "A") sql2 += varval; else sql1 += "=" + varval;
                    }
                    else if (type == typeof(int))
                    {
                        var varval = (tyy == null ? "null" : tyy.ToString());
                        if (mode == "A") sql2 += varval; else sql1 += "=" + varval;
                    }
                    else if (type == typeof(short))
                    {
                        var varval = (tyy == null ? "null" : tyy.ToString());
                        if (mode == "A") sql2 += varval; else sql1 += "=" + varval;
                    }
                    else
                    {
                        if (mode == "A") sql2 += filc(tyy.retStr()); else sql1 += "=" + filc(tyy.retStr());
                    }
                }
                if (mode == "A") sql = "insert into " + scm + "." + tblnm + " (" + sql1 + ") values (" + sql2 + ")";
                else sql = "update " + scm + "." + tblnm + " set " + sql1 + " where " + wherecluase;

                return sql;
            }
            catch (Exception ex)
            {
                return sql;
            }
        }
        //public string TDSCODE_help(string docdt, string val, string slcd)
        //{
        //    try
        //    {
        //        var UNQSNO = Cn.getQueryStringUNQSNO();
        //        ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        //        string scmf = CommVar.FinSchema(UNQSNO);
        //        string sql = "select a.tdscode, a.edate, a.tdsper, a.tdspernoncmp, ";
        //        if (slcd.retStr() != "") sql += "(select CMPNONCMP from  " + scmf + ".m_subleg where slcd='" + slcd + "') as CMPNONCMP, "; else sql += "'' CMPNONCMP, ";
        //        sql += " b.tdsnm, b.secno, b.glcd from ";
        //        sql += "(select tdscode, edate, tdsper, tdspernoncmp from ";
        //        sql += "(select a.tdscode, a.edate, a.tdsper, a.tdspernoncmp, ";
        //        sql += "row_number() over(partition by a.tdscode order by a.edate desc) as rn ";
        //        sql += "from " + scmf + ".m_tds_cntrl_dtl a ";
        //        sql += "where  edate <= to_date('" + docdt + "', 'dd/mm/yyyy')  ";
        //        if (val.retStr() != "") sql += " and tdscode = '" + val + "' ";
        //        sql += ")where rn = 1 ) a, ";
        //        sql += "" + scmf + ".m_tds_cntrl b ";
        //        sql += "where a.tdscode = b.tdscode(+) ";

        //        DataTable dt = SQLquery(sql);
        //        if (val == null)
        //        {
        //            System.Text.StringBuilder SB = new System.Text.StringBuilder();
        //            for (int i = 0; i <= dt.Rows.Count - 1; i++)
        //            {
        //                SB.Append("<tr ><td>" + dt.Rows[i]["TDSNM"].retStr() + "</td><td>" + dt.Rows[i]["TDSCODE"].retStr() + " </td><td> " + dt.Rows[i]["TDSPER"].retStr() + "</td><td>" + dt.Rows[i]["TDSPERNONCMP"].retStr() + " </td><td> " + dt.Rows[i]["SECNO"].retStr() + "</td></tr>");
        //            }
        //            var hdr = "TDS Name" + Cn.GCS() + "TDS Code" + Cn.GCS() + "TDS % Company" + Cn.GCS() + "TDS % Non Company" + Cn.GCS() + "Sec No.";
        //            return Generate_help(hdr, SB.ToString());
        //        }
        //        else
        //        {
        //            if (dt.Rows.Count != 0)
        //            {
        //                string str = "";
        //                str = ToReturnFieldValues("", dt);
        //                return str;
        //            }
        //        }
        //        return "Invalid TDS Code ! Please Enter a Valid Code ";
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return ex.Message + ex.InnerException;
        //    }
        //}
        public string SLCD_help(string val, string LINK_CD = "", string CAPTION = "", string GLCD = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string sql = "";
            string linkcd = LINK_CD.retSqlformat();
            string valsrch = val.ToUpper().Trim();
            if (CAPTION.retStr() == "")
            {
                if (LINK_CD == "D") { CAPTION = "Party"; }
                else if (LINK_CD == "T") { CAPTION = "Transporter"; }
                else if (LINK_CD == "U") { CAPTION = "Courier"; }
                else if (LINK_CD == "M") { CAPTION = "SalesMen"; }
                else if (LINK_CD == "A") { CAPTION = "Agent"; }
                else if (LINK_CD == "C") { CAPTION = "Creditor"; }
                else if (LINK_CD == "E") { CAPTION = "Employee"; } else { CAPTION = "Subledger"; }
            }
            sql = "";
            sql += "select distinct a.slcd, a.slnm, a.gstno, nvl(a.slarea,a.district) slarea,a.statecd,a.district,a.regmobile ";
            sql += "from " + scmf + ".m_subleg a, " + scmf + ".m_subleg_link b, " + scmf + ".m_cntrl_hdr c, " + scmf + ".m_cntrl_loca d , " + scmf + ".m_subleg_gl f ";
            sql += "where a.slcd=b.slcd(+) and a.m_autono=c.m_autono(+) and a.m_autono=d.m_autono(+) and a.slcd=f.slcd(+) and ";
            if (valsrch.retStr() != "") sql += "( upper(a.slcd) like '%" + valsrch + "%' or upper(a.slnm) like '%" + valsrch + "%' or upper(a.gstno) like '%" + valsrch + "%' or upper(nvl(a.slarea,a.district)) like '%" + valsrch + "%' ) and ";
            if (GLCD.retStr() != "") sql += "f.glcd = '" + GLCD + "' and ";
            if (linkcd != "") sql += "b.linkcd in (" + linkcd + ") and ";
            sql += "(d.compcd='" + COM + "' or d.compcd is null) and (d.loccd='" + LOC + "' or d.loccd is null) and ";
            sql += "nvl(c.inactive_tag,'N') = 'N' "; // and rownum<50000";
            sql += "order by slnm,slcd";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["slnm"] + "</td><td>" + tbl.Rows[i]["slcd"] + " </td><td>" + tbl.Rows[i]["gstno"] + " </td><td>" + tbl.Rows[i]["slarea"] + " </td><td>" + tbl.Rows[i]["statecd"] + " </td></tr>");
                }
                var hdr = "" + CAPTION + " Name" + Cn.GCS() + "" + CAPTION + " Code" + Cn.GCS() + "GST Number" + Cn.GCS() + "Area" + Cn.GCS() + "State Code";
                return (Generate_help(hdr, SB.ToString(), "4"));
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
                    return "Invalid " + CAPTION + " Code ! Please Enter a Valid " + CAPTION + " Code !!";
                }
            }
        }
        public string GLCD_help(string val, string LINK_CD = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string scmf = CommVar.FinSchema(UNQSNO);
            string valsrch = val.ToUpper().Trim();
            string linkcd = LINK_CD.retSqlformat();
            string sql = "";
            sql += "select c.GLCD,c.GLNM from " + scmf + ".M_GENLEG c," + scmf + ".M_CNTRL_HDR i where c.M_AUTONO= i.M_AUTONO(+) ";
            sql += "and i.INACTIVE_TAG ='N' ";
            if (valsrch.retStr() != "") sql += "and upper(c.GLCD) = '" + valsrch + "'  ";
            if (linkcd != "") sql += " and c.linkcd in (" + linkcd + ") ";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["GLNM"] + "</td><td>" + tbl.Rows[i]["GLCD"] + " </td></tr>");
                }
                var hdr = "Creditor Ledger Name" + Cn.GCS() + "Creditor Ledger Code";
                return (Generate_help(hdr, SB.ToString()));
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
                    return "Invalid Ledger Code ! Please Enter a Valid Ledger Code !!";
                }
            }

        }

        //public string GLCD_help(ImprovarDB DB)
        //{
        //    var query = (from c in DB.M_GENLEG
        //                 join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
        //                 where i.INACTIVE_TAG == "N"
        //                 select new
        //                 {
        //                     Code = c.GLCD,
        //                     Description = c.GLNM
        //                 }).ToList();
        //    System.Text.StringBuilder SB = new System.Text.StringBuilder();
        //    for (int i = 0; i <= query.Count - 1; i++)
        //    {
        //        SB.Append("<tr ><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
        //    }
        //    var hdr = "Creditor Ledger Name" + Cn.GCS() + "Creditor Ledger Code";
        //    return Generate_help(hdr, SB.ToString());
        //}
        public DataTable getRepFormat(string reptype, string doccd = "", string itgrpcd = "")
        {
            try
            {
                string sql = "";
                sql = "select a.repdesc text, a.formname value from " + CommVar.CurSchema(UNQSNO) + ".m_repformat a where a.reptype='" + reptype + "' and ";
                if (itgrpcd != "") sql += "a.itgrpcd='" + itgrpcd + "' and ";
                if (doccd != "") sql += "(a.itgrpcd in (select itgrpcd from " + CommVar.CurSchema(UNQSNO) + ".m_groupdoccd where doccd='" + doccd + "' ) or a.itgrpcd is null) and ";
                sql += "(a.compcd='" + CommVar.Compcd(UNQSNO) + "' or a.compcd is null) ";
                sql += "order by a.repdefault ";
                DataTable tbl = SQLquery(sql);
                return tbl;
            }
            catch
            {
                return null;
            }
        }
        public string retCompAddress(string gocd = "", string grpemailid = "")
        {
            string Scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string str, sql, goadd = "";
            string fssailicno = "";
            if (gocd == null) gocd = "";
            if (gocd != "")
            {
                sql = "select goadd1, goadd2, goadd3, gophno, gonm, fssailicno, loccd from " + Scmf + ".m_godown where gocd='" + gocd + "'";
                DataTable tbl = SQLquery(sql);
                if (tbl.Rows.Count > 0)
                {
                    goadd = tbl.Rows[0]["goadd1"].ToString() + " " + tbl.Rows[0]["goadd2"].ToString() + " " + tbl.Rows[0]["goadd3"].ToString();
                    goadd = goadd.Trim();
                    if (tbl.Rows[0]["gophno"].ToString() != "") goadd = goadd + " Phone : " + tbl.Rows[0]["gophno"].ToString();
                    fssailicno = tbl.Rows[0]["fssailicno"].ToString();
                    if (tbl.Rows[0]["loccd"].retStr() == LOC) goadd = "";
                }
            }

            DataTable comptbl = retComptbl();

            string compstat = "", compadd = "", locaadd = "", locastat = "", cregadd = "", stremail = "", legalname = "", corpadd = "";
            string locacommu = "", compcommu = "", corpcommu = "";
            string locaaddtype = "", regaddtype = "Regd Office", corpaddtype = "";
            string mfld = "";
            //Location Address
            for (int f = 1; f <= 6; f++)
            {
                mfld = "ladd" + Convert.ToString(f).ToString();
                if (comptbl.Rows[0][mfld].ToString() != "")
                {
                    compadd = compadd + comptbl.Rows[0][mfld].ToString() + " ";
                }
            }
            //Registered Office Address
            for (int f = 1; f <= 6; f++)
            {
                mfld = "add" + Convert.ToString(f).ToString();
                if (comptbl.Rows[0][mfld].ToString() != "")
                {
                    cregadd = cregadd + comptbl.Rows[0][mfld].ToString() + " ";
                }
            }
            cregadd = cregadd + " " + comptbl.Rows[0]["state"].ToString();
            if (comptbl.Rows[0]["addtype"].retStr() != "") locaaddtype = comptbl.Rows[0]["addtype"].retStr();

            //Corporate Office
            if (comptbl.Rows[0]["linkloccd"].ToString() != "")
            {
                DataTable corpaddtbl = retComptbl(comptbl.Rows[0]["linkloccd"].ToString());
                for (int f = 1; f <= 6; f++)
                {
                    mfld = "ladd" + Convert.ToString(f).ToString();
                    if (corpaddtbl.Rows[0][mfld].ToString() != "")
                    {
                        corpadd = corpadd + corpaddtbl.Rows[0][mfld].ToString() + " ";
                    }
                }
                corpadd += " " + corpaddtbl.Rows[0]["lstate"].ToString() + " [" + comptbl.Rows[0]["lstatecd"].ToString() + "]";
                if (corpaddtbl.Rows[0]["addtype"].retStr() != "") corpaddtype = corpaddtbl.Rows[0]["addtype"].retStr();
                corpadd = (corpaddtype == "" ? "" : corpaddtype + " : ") + corpadd;
                corpadd = corpadd.Trim();

                if (corpaddtbl.Rows[0]["phno1"].ToString() != "") corpcommu += "Phone : " + (corpaddtbl.Rows[0]["phno1std"].ToString() == "" ? "" : corpaddtbl.Rows[0]["phno1std"].ToString() + "-") + corpaddtbl.Rows[0]["phno1"].ToString();
                if (corpaddtbl.Rows[0]["phno3"].ToString() != "") corpcommu += ", Fax : " + corpaddtbl.Rows[0]["phno3"].ToString();
                if (corpaddtbl.Rows[0]["regemailid"].ToString() != "") corpcommu += ", email : " + corpaddtbl.Rows[0]["regemailid"].ToString();
            }
            //
            if (grpemailid == "") stremail = comptbl.Rows[0]["regemailid"].ToString(); else stremail = grpemailid;
            compadd += " " + comptbl.Rows[0]["lstate"].ToString() + " [" + comptbl.Rows[0]["lstatecd"].ToString() + "]";

            if (comptbl.Rows[0]["phno1"].ToString() != "") locacommu += "Phone : " + (comptbl.Rows[0]["phno1std"].ToString() == "" ? "" : comptbl.Rows[0]["phno1std"].ToString() + "-") + comptbl.Rows[0]["phno1"].ToString();
            if (comptbl.Rows[0]["phno2"].ToString() != "") locacommu += " " + (comptbl.Rows[0]["phno2std"].ToString() == "" ? "" : comptbl.Rows[0]["phno2std"].ToString() + "-") + comptbl.Rows[0]["phno2"].ToString();
            if (comptbl.Rows[0]["phno3"].ToString() != "") locacommu += ", Fax : " + comptbl.Rows[0]["phno3"].ToString();
            if (stremail != "") locacommu += ", email : " + stremail;

            if (comptbl.Rows[0]["panno"].ToString() != "") compstat = "PAN # " + comptbl.Rows[0]["panno"].ToString() + " ";
            if (comptbl.Rows[0]["cinno"].ToString() != "") compstat = compstat + "CIN # " + comptbl.Rows[0]["cinno"].ToString() + " ";

            locastat = "GST # " + comptbl.Rows[0]["gstno"].ToString();
            if (fssailicno != "") locastat = locastat + "   FSSAI LICENCE # " + fssailicno;

            locaadd = (locaaddtype == "" ? "" : locaaddtype + " : ") + compadd;
            compadd = (regaddtype == "" ? "" : regaddtype + " : ") + cregadd;
            if (comptbl.Rows[0]["regdoffsame"].ToString() == "Y") { compadd = locaadd; compcommu = locacommu; locaadd = ""; locacommu = ""; }

            if (goadd.retStr() != "") locaadd = "Godown : " + goadd;
            if (comptbl.Rows[0]["regdoffsame"].ToString() == "Y" && goadd.retStr() == "") { compstat += " " + locastat; locastat = ""; }
            if (comptbl.Rows[0]["propname"].ToString().retStr() != "") legalname = "Prop. " + comptbl.Rows[0]["propname"].ToString();

            str = "";
            str += "^COMPNM=^" + comptbl.Rows[0]["compnm"].ToString() + Cn.GCS();
            str += "^COMPADD=^" + compadd + Cn.GCS();
            str += "^COMPCOMMU=^" + compcommu + Cn.GCS();
            str += "^COMPSTAT=^" + compstat + Cn.GCS();
            str += "^LOCAADD=^" + locaadd + Cn.GCS();
            str += "^LOCACOMMU=^" + locacommu + Cn.GCS();
            str += "^LOCASTAT=^" + locastat + Cn.GCS();
            str += "^LEGALNAME=^" + legalname + Cn.GCS();
            str += "^EMAIL=^" + stremail + Cn.GCS();
            str += "^CORPADD=^" + corpadd + Cn.GCS();
            str += "^CORPCOMMU=^" + corpcommu + Cn.GCS();
            return str;
        }
        public DataTable retComptbl(string loccd = "")
        {
            string sql = "", scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (loccd.retStr() != "") LOC = loccd;
            sql += "select b.compnm, b.add1, b.add2, b.add3, b.add4, b.add5, b.add6, b.state, b.country, b.panno, b.cinno, b.propname, ";
            sql += "nvl(a.regdoffsame,'Y') regdoffsame, a.addtype, a.linkloccd, ";
            sql += "a.add1 ladd1, a.add2 ladd2, a.add3 ladd3, a.add4 ladd4, a.add5 ladd5, a.add6 ladd6, ";
            sql += "a.state lstate, a.country lcountry, a.statecd lstatecd, a.phno3, a.phno1std,a.phno2std, ";
            sql += "a.gstno, a.phno1, a.phno2, a.regemailid ";
            sql += "from " + scmf + ".m_loca a, " + scmf + ".m_comp b ";
            sql += "where a.compcd='" + COM + "' and a.loccd='" + LOC + "' and a.compcd=b.compcd(+) ";
            DataTable tbl = SQLquery(sql);
            return tbl;
        }
        public DataTable retslcdCont(string slcd, string dept = "", bool getallEmailid = false)
        {
            string sql = "";
            string scmf = CommVar.FinSchema(UNQSNO);
            string sslcd = slcd;
            string deptt = dept;
            string modcd = CommVar.ModuleCode();

            if (slcd.IndexOf(',') < 0) sslcd = "'" + slcd + "'";
            if (dept == "")
            {
                if (modcd == "FIN") deptt = "F";
                else if (modcd == "INV") deptt = "P";
                else if (modcd == "PAY") deptt = "A";
                else deptt = "S";
            }

            sql += "select a.slcd, nvl(c.fullname,c.slnm) slnm, nvl(b.persemail,a.regemailid) regemailid, ";
            sql += "nvl(b.mobile1,a.regmobile) regmobile, c.gstno, c.panno, c.statecd, ";
            sql += "c.add1, c.add2, c.add3, c.add4, c.add5, c.add6, c.add7, ";
            sql += "c.district, c.pin, c.state, c.country, b.cperson, b.desig, ";
            sql += "d.ifsccode, d.bankname, d.branch, d.address, d.bankactno from ";
            sql += "(select a.slcd, a.regemailid, decode(nvl(a.regmobile,0),0,null,to_char(a.regmobile)) regmobile ";
            sql += "from " + scmf + ".m_subleg a ) a, ";

            sql += "(select a.slcd, a.cperson, a.desig, a.persemail, a.mobile1 from ";
            sql += "(select a.slcd, a.cperson, a.desig, a.persemail, decode(nvl(a.mobile1,0),0,null,to_char(a.mobile1)) mobile1, ";
            sql += "row_number() over (partition by a.slcd order by a.slno desc) as rn ";
            sql += "from " + scmf + ".m_subleg_cont a ";
            sql += "where a.dept='" + deptt + "') a ";
            if (getallEmailid == false) sql += "where a.rn=1 ";
            sql += ") b, ";

            sql += "(select a.slcd, a.ifsccode, a.bankname, a.branch, a.address, a.bankactno from ";
            sql += "(select a.slcd, a.ifsccode, a.bankname, a.branch, a.address, a.bankactno, ";
            sql += "row_number() over (partition by a.slcd order by a.slno desc) as rn ";
            sql += "from " + scmf + ".m_subleg_ifsc a ";
            sql += "where a.defltbank='T') a ";
            sql += "where a.rn=1 ) d, ";

            sql += "" + scmf + ".m_subleg c ";
            sql += "where a.slcd=b.slcd(+) and a.slcd=c.slcd(+) and a.slcd=d.slcd(+) ";
            if (sslcd != "") sql += "and a.slcd in (" + sslcd + ") ";

            DataTable tbl = SQLquery(sql);

            return tbl;
        }
        public string retCompLogo()
        {
            string complogo = "C:\\IpSmart\\Logo\\" + CommVar.Compcd(UNQSNO) + ".png";
            if (!System.IO.File.Exists(complogo)) complogo = "c:\\IpSmart\\Logo\\" + CommVar.Compcd(UNQSNO) + ".jpg"; else { return complogo; }
            if (!System.IO.File.Exists(complogo)) complogo = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + ".png"; else { return complogo; }
            if (!System.IO.File.Exists(complogo)) complogo = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + ".jpg"; else { return complogo; }
            if (!System.IO.File.Exists(complogo)) complogo = "";
            return complogo;
        }
        public string retCompLogo1()
        {
            string complogo = "C:\\IpSmart\\Logo\\" + CommVar.Compcd(UNQSNO) + "1.png";
            if (!System.IO.File.Exists(complogo)) complogo = "c:\\IpSmart\\Logo\\" + CommVar.Compcd(UNQSNO) + "1.jpg"; else { return complogo; }
            if (!System.IO.File.Exists(complogo)) complogo = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + "1.jpg"; else { return complogo; }
            if (!System.IO.File.Exists(complogo)) complogo = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + "1.jpg"; else { return complogo; }
            if (!System.IO.File.Exists(complogo)) complogo = "";
            return complogo;
        }
        public string PARTYCD_help(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string scmf = CommVar.FinSchema(UNQSNO);
            string valsrch = val.ToUpper().Trim();
            string sql = "";
            sql += "select c.PARTYCD,c.PARTYNM from " + scmf + ".M_PARTYGRP c," + scmf + ".M_CNTRL_HDR i where c.M_AUTONO= i.M_AUTONO(+) ";
            sql += "and i.INACTIVE_TAG ='N' ";
            if (valsrch.retStr() != "") sql += "and upper(c.PARTYCD) = '" + valsrch + "'  ";
            sql += "order by  c.PARTYCD,c.PARTYNM ";
            DataTable tbl = SQLquery(sql);
            if (val.retStr() == "" || tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["PARTYNM"] + "</td><td>" + tbl.Rows[i]["PARTYCD"] + " </td></tr>");
                }
                var hdr = "Party Group Name" + Cn.GCS() + "Party Group Code";
                return (Generate_help(hdr, SB.ToString()));
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
                    return "Invalid Party Group Code ! Please Enter a Valid Party Group Code !!";
                }
            }
        }
        public string InsTXN_TRANS(string autono, short? emd_no, string dtag = "", string ttag = "", string translcd = "", string ewaybillno = "",
         string lrno = "", string lrdt = "", string lorryno = "", string transmode = "", string vechltype = "", string crslcd = "", double? DISTANCE = null, string EWAYBILLDT = "", string EWAYBILLVALID = "")
        {
            string bl = "";
            try
            {
                string scmf = CommVar.FinSchema(UNQSNO), clcd = CommVar.ClientCode(UNQSNO);
                string sql = "";


                sql = "insert into " + scmf + ".t_txntrans (emd_no, clcd, dtag, ttag, autono, translcd, ewaybillno, lrno, lrdt, lorryno, transmode, vechltype, crslcd,DISTANCE,EWAYBILLDT,EWAYBILLVALID ) values (";
                sql = sql + emd_no;
                sql = sql + "," + filc(clcd);
                sql = sql + "," + filc(dtag);
                sql = sql + "," + filc(ttag);
                sql = sql + "," + filc(autono);
                sql = sql + "," + filc(translcd);
                sql = sql + "," + filc(ewaybillno);
                sql = sql + "," + filc(lrno);
                sql = sql + "," + fild(lrdt);
                sql = sql + "," + filc(lorryno);
                sql = sql + "," + filc(transmode);
                sql = sql + "," + filc(vechltype);
                sql = sql + "," + filc(crslcd);
                sql = sql + "," + DISTANCE.retDbl();
                sql = sql + "," + fild(EWAYBILLDT);
                sql = sql + "," + filc(EWAYBILLVALID);
                sql = sql + ")";

                bl = sql;
                return bl;
            }

            catch (Exception e)
            {
                bl = e.Message;
                return bl;
            }
        }
        public string USER_ID_HELP(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            var query = (from c in DB.USER_APPL select new { USER_ID = c.USER_ID, USER_NAME = c.USER_NAME }).ToList();
            if (val == null)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].USER_NAME + "</td><td>" + query[i].USER_ID + "</td></tr>");
                }
                var hdr = "User Name" + Cn.GCS() + "User Id";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                query = query.Where(a => a.USER_ID == val).ToList();
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.USER_ID + Cn.GCS() + i.USER_NAME;
                    }
                    return str;
                }
                else
                {
                    return "Invalid User Id ! Please Select / Enter a Valid User Id !!";
                }
            }
        }
        public void insT_TXNSTATUS(string Auto_Number, string ststype, string stsrem)
        {

            Connection Cn = new Connection();
            var UNQSNO = Cn.getQueryStringUNQSNO();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Models.T_TXNSTATUS TCH = new Models.T_TXNSTATUS();

            var data = (from p in DB.T_TXNSTATUS where (p.AUTONO == Auto_Number && p.STSTYPE == ststype) select new { p.EMD_NO, p.FLAG1 }).ToList();
            double FLAG1 = 0; short MAXEMDNO = 0;
            if (data.Count > 0)
            {
                MAXEMDNO = data.Select(a => a.EMD_NO).Max().retShort();
                FLAG1 = data.Select(a => a.FLAG1.retDbl()).Max();
            }

            short emdno = 0;
            if (MAXEMDNO == 0) emdno = 0; else emdno = Convert.ToByte(MAXEMDNO + 1);
            string flag1 = FLAG1.retDbl() == 0 ? "1" : (FLAG1.retDbl() + 1).retStr();

            TCH.AUTONO = Auto_Number;
            TCH.STSTYPE = ststype;
            TCH.FLAG1 = flag1;
            TCH.CLCD = CommVar.ClientCode(UNQSNO);
            TCH.STSREM = stsrem;
            TCH.USR_ID = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
            TCH.USR_ENTDT = System.DateTime.Now;
            TCH.USR_LIP = Cn.GetIp();
            TCH.USR_SIP = Cn.GetStaticIp();
            TCH.USR_OS = null;
            TCH.USR_MNM = Cn.DetermineCompName(Cn.GetIp());  //GetMachin;
            TCH.DTAG = "";
            TCH.EMD_NO = emdno;

            DB.T_TXNSTATUS.Add(TCH);
            DB.SaveChanges();
            return;
        }
      
    }
}