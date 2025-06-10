using System.Collections.Generic;
using System.Linq;
using Improvar.Models;
using System.Data;

namespace Improvar
{
    public class DropDownHelp
    {
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public List<DropDown_list_GLCD> DropDown_list_GLCD(string linkcd, string slcdmust = "N,Y")
        {
            List<DropDown_list_GLCD> dropDownlistGLCD = new List<DropDown_list_GLCD>();
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

            string gllinkcd = linkcd;
            string glslcdmust = "Y,N";
            if (slcdmust != "") glslcdmust = slcdmust;
            if (linkcd == "") linkcd = "B,S,D,C,O";

            string[] lstlinkcd = linkcd.Split(',');
            string[] lstslcdmst = glslcdmust.Split(',');

            dropDownlistGLCD = (from j in DBF.M_GENLEG
                                where lstlinkcd.Contains(j.LINKCD) && lstslcdmst.Contains(j.SLCDMUST)
                                select new DropDown_list_GLCD
                                {
                                    text = j.GLNM,
                                    value = j.GLCD
                                }).ToList().OrderBy(s => s.text).ToList();
            return dropDownlistGLCD;
        }
        //public List<DropDown_list_SLCD> GetSlcdforSelection(string linkcd="")
        //{
        //    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(),  CommVar.CurSchema(UNQSNO));
        //    string COM = CommVar.Compcd(UNQSNO), LOC =  CommVar.Loccd(UNQSNO);

        //    string[] linkcode = linkcd.Split(',');
        //    List<DropDown_list_SLCD> sllist = new List<DropDown_list_SLCD>();
        //    sllist = (from a in DBF.M_SUBLEG
        //              join b in DBF.M_SUBLEG_LINK on a.SLCD equals b.SLCD into x
        //              from b in x.DefaultIfEmpty()
        //              join i in DBF.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
        //              join c in DBF.M_CNTRL_LOCA on a.M_AUTONO equals c.M_AUTONO into g
        //              from c in g.DefaultIfEmpty()
        //              where (linkcode.Contains(b.LINKCD) && c.COMPCD == COM && c.LOCCD == LOC && i.INACTIVE_TAG == "N" || linkcode.Contains(b.LINKCD) && c.COMPCD == null && c.LOCCD == null && i.INACTIVE_TAG == "N")
        //              select new DropDown_list_SLCD()
        //              {
        //                  text = a.SLNM,
        //                  text1 = a.DISTRICT,
        //                  value = a.SLCD
        //              }).ToList();
        //    return sllist;
        //}
        public List<DropDown_list_Class1> DropDown_list_Class1()
        {
            List<DropDown_list_Class1> dropDownlistsLClass1 = new List<DropDown_list_Class1>();
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            dropDownlistsLClass1 = (from j in DBF.M_CLASS1 select new DropDown_list_Class1() { text = j.CLASS1NM, value = j.CLASS1CD }).ToList();
            return dropDownlistsLClass1;
        }
        public List<DropDown_list_AGSLCD> GetAgSlcdforSelection()
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

            string[] linkcode = { "A" };
            List<DropDown_list_AGSLCD> sllist = new List<DropDown_list_AGSLCD>();
            sllist = (from a in DBF.M_SUBLEG
                      join b in DBF.M_SUBLEG_LINK on a.SLCD equals b.SLCD into x
                      from b in x.DefaultIfEmpty()
                      join i in DBF.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
                      join c in DBF.M_CNTRL_LOCA on a.M_AUTONO equals c.M_AUTONO into g
                      from c in g.DefaultIfEmpty()
                      where (linkcode.Contains(b.LINKCD) && c.COMPCD == COM && c.LOCCD == LOC && i.INACTIVE_TAG == "N" || linkcode.Contains(b.LINKCD) && c.COMPCD == null && c.LOCCD == null && i.INACTIVE_TAG == "N")
                      select new DropDown_list_AGSLCD()
                      {
                          text = a.SLNM,
                          value = a.SLCD
                      }).ToList();
            return sllist;
        }
        public List<DropDown_list_SLMSLCD> GetSlmSlcdforSelection()
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

            string[] linkcode = { "M" };
            List<DropDown_list_SLMSLCD> sllist = new List<DropDown_list_SLMSLCD>();
            sllist = (from a in DBF.M_SUBLEG
                      join b in DBF.M_SUBLEG_LINK on a.SLCD equals b.SLCD into x
                      from b in x.DefaultIfEmpty()
                      join i in DBF.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
                      join c in DBF.M_CNTRL_LOCA on a.M_AUTONO equals c.M_AUTONO into g
                      from c in g.DefaultIfEmpty()
                      where (linkcode.Contains(b.LINKCD) && c.COMPCD == COM && c.LOCCD == LOC && i.INACTIVE_TAG == "N" || linkcode.Contains(b.LINKCD) && c.COMPCD == null && c.LOCCD == null && i.INACTIVE_TAG == "N")
                      select new DropDown_list_SLMSLCD()
                      {
                          text = a.SLNM,
                          value = a.SLCD
                      }).ToList();
            return sllist;
        }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list_EMPCD> GetEmpcdforSelection()
        {
            ImprovarDB DBP = new ImprovarDB(Cn.GetConnectionString(), CommVar.PaySchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

            List<DropDown_list_EMPCD> sllist = new List<DropDown_list_EMPCD>();
            sllist = (from a in DBP.M_EMPMAS
                      select new DropDown_list_EMPCD()
                      {
                          text = a.ENM,
                          value = a.EMPCD,
                      }).ToList();
            return sllist;
        }
        public List<DropDown_list_SLCD> GetSlcdforSelection(string linkcd = "")
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (linkcd == "ALL" || linkcd == "")
            {
                linkcd = string.Join(",", (from a in DBF.M_SUBLEG_LINK select a.LINKCD).Distinct().ToList());
            }

            string[] linkcode = linkcd.Split(',');
            List<DropDown_list_SLCD> sllist = new List<DropDown_list_SLCD>();
            sllist = (from a in DBF.M_SUBLEG
                      join b in DBF.M_SUBLEG_LINK on a.SLCD equals b.SLCD into x
                      from b in x.DefaultIfEmpty()
                      join i in DBF.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
                      join c in DBF.M_CNTRL_LOCA on a.M_AUTONO equals c.M_AUTONO into g
                      from c in g.DefaultIfEmpty()
                      where (linkcode.Contains(b.LINKCD) && c.COMPCD == COM && c.LOCCD == LOC && i.INACTIVE_TAG == "N" || linkcode.Contains(b.LINKCD) && c.COMPCD == null && c.LOCCD == null && i.INACTIVE_TAG == "N")
                      select new DropDown_list_SLCD()
                      {
                          text = a.SLNM,
                          value = a.SLCD,
                          //text1 = a.DISTRICT,
                          slarea = a.SLAREA == null ? a.DISTRICT : a.SLAREA,
                          City = a.SLAREA == null ? a.DISTRICT : a.SLAREA
                      }).Distinct().OrderBy(A => A.text).ToList();
            return sllist;
        }

        public List<DropDown_list_BRAND> GetBrandcddforSelection()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

            List<DropDown_list_BRAND> sllist = new List<DropDown_list_BRAND>();
            sllist = (from a in DB.M_BRAND
                      select new DropDown_list_BRAND()
                      {
                          text = a.BRANDNM,
                          value = a.BRANDCD
                      }).OrderBy(A => A.text).ToList();
            return sllist;
        }
        public List<DropDown_list_GODOWN> GetGocdforSelection()
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

            List<DropDown_list_GODOWN> sllist = new List<DropDown_list_GODOWN>();
            sllist = (from a in DBF.M_GODOWN
                      select new DropDown_list_GODOWN()
                      {
                          text = a.GONM,
                          value = a.GOCD
                      }).OrderBy(A => A.text).ToList();
            return sllist;
        }
        public List<DropDown_list_TXN> GetTxnforSelection(string doctype, string hdrtbl)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string sql = "";
            if (doctype.IndexOf("'") <= 0) doctype = "'" + doctype + "'";

            sql = "";
            sql += "select  a.autono, a.docno, a.docdt, b.slcd, c.slnm, nvl(c.slarea,c.district) slarea, to_char(a.docdt,'dd/mm/yyyy') docdt ";
            sql += "from " + scm + ".t_cntrl_hdr a, " + scm + "." + hdrtbl + " b, " + scmf + ".m_subleg c, " + scm + ".m_doctype d ";
            sql += "where a.autono=b.autono and b.slcd=c.slcd(+) and a.compcd='" + COM + "' and a.loccd = '" + LOC + "' and ";
            sql += "nvl(a.cancel,'N')='N' and a.doccd=d.doccd(+) ";
            if(doctype!= "''") sql += "and d.doctype in (" + doctype + ") ";
            DataTable tbl = MasterHelp.SQLquery(sql);

            List<DropDown_list_TXN> sllist = new List<DropDown_list_TXN>();
            sllist = (from DataRow a in tbl.Rows
                      select new DropDown_list_TXN()
                      {
                          text = a["docno"].ToString(),
                          value = a["autono"].ToString(),
                          slcd = a["slcd"].ToString(),
                          slnm = a["slnm"].ToString(),
                          slarea = a["slarea"].ToString(),
                          docdt = a["docdt"].retDateStr()
                      }).OrderBy(A => A.text).ToList();
            return sllist;
        }

        public List<DropDown_list_DOCCD> GetDocCdforSelection(string doctype = "")
        {
            List<DropDown_list_DOCCD> sllist = new List<DropDown_list_DOCCD>();
            string sql = "", scm = CommVar.CurSchema(UNQSNO);

            sql += "select a.doccd, a.docnm, b.dname ";
            sql += "from " + scm + ".m_doctype a,  " + scm + ".m_dtype b where a.doctype=b.dcd(+) ";
            if (doctype != "") sql += "and a.doctype in (" + doctype + ") ";
            sql += "order by docnm ";
            DataTable tbl = MasterHelp.SQLquery(sql);

            sllist = (from DataRow dr in tbl.Rows
                      select new DropDown_list_DOCCD()
                      {
                          text = dr["docnm"].ToString() + (dr["dname"] == null ? "" : "[" + dr["dname"].ToString() + "]"),
                          value = dr["doccd"].ToString(),
                      }).ToList();
            return sllist;
        }
        public List<DropDown_list_SLCD> DropDown_list_SLCD()
        {
            List<DropDown_list_SLCD> dropDownlistsLCD = new List<DropDown_list_SLCD>();
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                dropDownlistsLCD = (from j in DBF.M_SUBLEG select new DropDown_list_SLCD() { text = j.SLNM, value = j.SLCD, City = j.DISTRICT }).OrderBy(s => s.text).ToList();
            }
            return dropDownlistsLCD;
        }
        public List<DropDown_list_SubLegGrp> GetSubLegGrpforSelection(string grpcd = "")
        {
            List<DropDown_list_SubLegGrp> sllist = new List<DropDown_list_SubLegGrp>();
            string sql = "", scmf = CommVar.FinSchema(UNQSNO);

            sql += "select a.grpcd, b.grpnm, a.slcdgrpcd, a.slcdgrpnm, b.class1cd from ";
            sql += "(select a.grpcd, a.slcdgrpcd, decode(nvl(b.slcdgrpnm,''),'','',b.slcdgrpnm||' - ')||a.slcdgrpnm slcdgrpnm, a.grpcdfull ";
            sql += "from " + scmf + ".m_subleg_grp a, " + scmf + ".m_subleg_grp b ";
            //sql += "where a.parentcd=b.slcdgrpcd(+) and a.slcd is null and a.parentcd is not null ) a, ";
            sql += "where a.parentcd=b.slcdgrpcd(+) and a.slcd is null ) a, ";

            sql += "(select a.grpcd, a.grpnm, b.class1cd ";
            sql += "from " + scmf + ".m_subleg_grphdr a, " + scmf + ".m_subleg_grpclass1 b ";
            sql += "where a.grpcd=b.grpcd(+) ) b ";
            sql += "where a.grpcd=b.grpcd(+) ";

            sql = "select distinct grpcd, grpnm, slcdgrpcd, slcdgrpnm from ( " + sql + " ) order by grpnm, slcdgrpnm ";
            DataTable tbl = MasterHelp.SQLquery(sql);

            sllist = (from DataRow dr in tbl.Rows
                      select new DropDown_list_SubLegGrp()
                      {
                          text = dr["slcdgrpnm"].ToString(),
                          value = dr["slcdgrpcd"].ToString(),
                          grpnm = dr["grpnm"].ToString()
                      }).ToList();
            return sllist;
        }
        public List<DropDown_list_ITEM> GetItcdforSelection(string itgrpcd = "")
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            List<DropDown_list_ITEM> sllist = new List<DropDown_list_ITEM>();
            string sql = "select (STYLENO || ITNM)ITSTYLE,ITCD from " + CommVar.CurSchema(UNQSNO) + ".M_SITEM ";
            DataTable dt = MasterHelp.SQLquery(sql);
            sllist = (from DataRow dr in dt.Rows
                      select new DropDown_list_ITEM()
                      {
                          text = dr["ITSTYLE"].retStr(),
                          value = dr["ITCD"].retStr()
                      }).OrderBy(A => A.text).ToList();
            return sllist;
        }
        public List<DropDown_list_COLLNM> GetCOLLNMforSelection(string collnm = "")
        {          
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var LineList = (from c in DB.M_COLLECTION
                            join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                            where i.INACTIVE_TAG == "N"
                            select new DropDown_list_COLLNM
                            {
                                value = c.COLLCD,
                                text = c.COLLNM
                            }).ToList();
            return LineList;
        }
        public List<DropDown_list_ITGRP> GetItgrpcdforSelection(string itgrptype = "")
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

            if (itgrptype == "")
            {
                itgrptype = string.Join(",", (from a in DB.M_GROUP select a.ITGRPTYPE).Distinct().ToList());
            }
            string[] selgrpcd = itgrptype.Split(',');
            List<DropDown_list_ITGRP> sllist = new List<DropDown_list_ITGRP>();
            sllist = (from a in DB.M_GROUP
                      where (selgrpcd.Contains(a.ITGRPTYPE))
                      select new DropDown_list_ITGRP()
                      {
                          text = a.ITGRPNM,
                          value = a.ITGRPCD
                      }).OrderBy(A => A.text).ToList();
            return sllist;
        }
        public List<DropDown_list_LINECD> GetLinecdforSelection()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            List<DropDown_list_LINECD> LineList = new List<DropDown_list_LINECD>();
            LineList = (from a in DB.M_LINEMAST join i in DB.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO where i.INACTIVE_TAG == "N" select new DropDown_list_LINECD() { text = a.LINENM, value = a.LINECD }).ToList();
            return LineList;
        }
        public List<DropDown_list_JOBCD> DropDown_JOBCD()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var LineList = (from c in DB.M_JOBMST
                            join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                            where i.INACTIVE_TAG == "N"
                            select new DropDown_list_JOBCD
                            {
                                value = c.JOBCD,
                                text = c.JOBNM
                            }).ToList();
            return LineList;
        }
        public List<DropDown_list_RTCD> GetRtDebCdforSelection()
        {
            List<DropDown_list_RTCD> sllist = new List<DropDown_list_RTCD>();
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string sql = "";

                sql = "select distinct a.rtdebcd, a.rtdebnm, a.mobile, a.area ";
                sql += "from " + scmf + ".m_retdeb a, " + scmf + ".m_cntrl_hdr b, " + scmf + ".m_cntrl_loca c ";
                sql += "where a.m_autono=b.m_autono(+) and a.m_autono=c.m_autono(+) and ";
                sql += "nvl(b.inactive_tag,'N')='N' and ";
                sql += "(c.compcd='" + COM + "' or c.compcd is null) and (c.loccd='" + LOC + "' or c.loccd is null) ";
                sql += "order by rtdebnm ";
                DataTable tbl = MasterHelp.SQLquery(sql);

                sllist = (from DataRow dr in tbl.Rows
                          select new DropDown_list_RTCD()
                          {
                              text = dr["rtdebnm"].ToString(),
                              value = dr["rtdebcd"].ToString(),
                              mobile = dr["mobile"].ToString(),
                              add = dr["area"].ToString(),
                          }).ToList();
            }
            return sllist;
        }
        public List<DropDown_list_BLTYPE> DropDownBLTYPE()
        {
            string scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            sql += "select BLTYPE from " + scm + ".M_BLTYPE ";
            DataTable dt = MasterHelp.SQLquery(sql);
            List<DropDown_list_BLTYPE> BL_TYPE_list = new List<DropDown_list_BLTYPE>();
            BL_TYPE_list = (from DataRow dr in dt.Rows
                            select new DropDown_list_BLTYPE() { Text = dr["BLTYPE"].retStr(), Value = dr["BLTYPE"].retStr() }).OrderBy(s => s.Text).Distinct().ToList();
            return BL_TYPE_list;
        }
        public List<DropDown_list_GLCD> GetGlcdforSelection(string linkcd = "", string slcdmust = "'N','Y'", string gltag = "")
        {
            List<DropDown_list_GLCD> sllist = new List<DropDown_list_GLCD>();
            string gllinkcd = linkcd;
            string glslcdmust = "'Y','N'";
            if (slcdmust != "") glslcdmust = slcdmust;
            if (linkcd == "") linkcd = "'B','S','D','C','O'";

            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scmc = CommVar.CommSchema();
            string sql = "";

            sql = "select distinct a.glcd, a.glnm ";
            sql += "from " + scmf + ".M_GENLEG a, " + scmf + ".m_cntrl_hdr b, " + scmf + ".m_cntrl_loca c, " + scmc + ".MS_GLHDGRP d ";
            sql += "where a.m_autono=b.m_autono(+) and a.m_autono=c.m_autono(+) and a.GLHDGRPCD=d.GLHDGRPCD(+) ";
            sql += "and nvl(b.inactive_tag,'N')='N' ";
            sql += "and a.LINKCD in (" + linkcd + ") and a.SLCDMUST in (" + glslcdmust + ") ";
            if (gltag.retStr() != "") sql += "and d.gltag in (" + gltag + ") ";
            sql += "and (c.compcd='" + COM + "' or c.compcd is null) and (c.loccd='" + LOC + "' or c.loccd is null) ";
            sql += "order by glnm ";
            DataTable tbl = MasterHelp.SQLquery(sql);

            sllist = (from DataRow dr in tbl.Rows
                      select new DropDown_list_GLCD()
                      {
                          text = dr["glnm"].ToString(),
                          value = dr["glcd"].ToString(),
                      }).ToList();

            return sllist;
        }
        public List<DropDown_list_LOCCD> DropDownLoccation()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var locnmlist = (from a in DB.M_LOCA
                             select new DropDown_list_LOCCD
                             {
                                 Value = a.LOCCD,
                                 Text = a.LOCNM
                             }).Distinct().ToList();
            return locnmlist;
        }

    }
}