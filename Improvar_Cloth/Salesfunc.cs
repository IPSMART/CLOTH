using System;
using System.Collections.Generic;
using System.Linq;
using Improvar.Models;
using System.Data;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;
using Improvar.ViewModels;

namespace Improvar
{
    public class Salesfunc : MasterHelpFa
    {
        Connection Cn = new Connection();
        MasterHelpFa masterHelpFa = new MasterHelpFa();
        string UNQSNO = CommVar.getQueryStringUNQSNO();//
        public DataTable GetSlcdDetails(string slcd, string docdt, string linkcd = "", string doctag = "", string bltype = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (docdt == null) docdt = "";

            string sql = "";
            string itgrpcd = "";

            sql += "select z.slcd, b.taxgrpcd, a.agslcd, a.areacd, a.prccd, a.discrtcd, a.crdays, a.crlimit, a.cod, a.gstno, a.docth, b.trslcd, b.courcd, nvl(c.agslcd,a.agslcd) agslcd, ";
            sql += "g.slnm,nvl(g.slarea,g.district) slarea, h.slnm agslnm, i.slnm trslnm, e.taxgrpnm, f.prcnm, ";
            //sql += "f.prcnm, "; // c.prcdesc, c.effdt, c.itmprccd, ";
            sql += "y.docdt lastbldt, y.scmdiscrate, y.scmdisctype,y.listdiscper, ";
            //sql += "nvl(a.crdays,0) crdays, nvl(a.crlimit,0) crlimit,g.pslcd,g.tcsappl,g.panno,g.partycd ";
            sql += "nvl(a.crdays,0) crdays, nvl(a.crlimit,0) crlimit,g.pslcd,g.panno,g.partycd, ";
            sql += "(case when to_date('" + docdt + "', 'dd/mm/yyyy') < to_date('01/07/2021', 'dd/mm/yyyy') then nvl(g.tcsappl, 'Y') else  decode(nvl(g.tot194q, 'N'), 'Y', 'N', 'Y') end ) tcsappl ";
            sql += "from ";

            sql += "(select a.slcd from " + scmf + ".m_subleg a where a.slcd='" + slcd + "' ) z, ";

            sql += "(select a.slcd, a.agslcd, a.areacd, a.prccd, a.discrtcd, a.crdays, a.crlimit, a.cod, a.docth, b.gstno ";
            sql += "from " + scm + ".m_subleg_com a, " + scmf + ".m_subleg b ";
            sql += "where b.slcd='" + slcd + "' and a.slcd=b.slcd(+) and (a.compcd='" + COM + "' or a.compcd is null) ) a, ";

            sql += "(select a.slcd, a.taxgrpcd, a.trslcd, a.courcd ";
            sql += "from " + scm + ".m_subleg_sddtl a, " + scmf + ".m_subleg b ";
            sql += "where b.slcd='" + slcd + "' and a.slcd=b.slcd(+) and (a.compcd='" + COM + "' or a.compcd is null) and (a.loccd='" + LOC + "'  or a.loccd is null) ) b, ";

            sql += "(select b.slcd, b.agslcd ";
            sql += "from " + scm + ".m_subleg_brand b ";
            sql += "where b.slcd='" + slcd + "' and b.compcd='" + COM + "' ) c, ";

            sql += "( select slcd,docdt, scmdiscrate, scmdisctype, listdiscper from ( ";
            sql += "select b.slcd, c.docdt, a.scmdiscrate, scmdisctype, listdiscper, ";
            sql += "row_number() over(partition by b.slcd order by c.docdt desc) as rn ";
            sql += "from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scmf + ".m_subleg d, " + scm + ".t_txnoth e ";
            sql += "where a.autono = b.autono(+) and a.autono = c.autono(+) and b.slcd = d.slcd(+) and listdiscper<>0 and b.autono=e.autono(+) and  ";
            if (docdt != "") sql += "c.docdt <= to_date('" + docdt + "','dd/mm/yyyy') and ";
            //sql += "c.compcd='" + COM + "' and d.slcd = '" + slcd + "' and b.doctag in ('SB') ";
            sql += "c.compcd='" + COM + "' and d.slcd = '" + slcd + "' ";
            if (doctag.retStr() != "") sql += "and b.doctag in (" + doctag + ") ";
            if (bltype.retStr() != "") sql += "and e.bltype in (" + bltype + ") ";
            sql += "group by b.slcd, c.docdt, a.scmdiscrate,a.scmdisctype,a.listdiscper ";
            if (CommVar.LastYearSchema(UNQSNO) != "")
            {
                sql += "union all ";
                sql += "select b.slcd, c.docdt, a.scmdiscrate, scmdisctype, listdiscper, ";
                sql += "row_number() over(partition by b.slcd order by c.docdt desc) as rn ";
                sql += "from " + CommVar.LastYearSchema(UNQSNO) + ".t_txndtl a, " + CommVar.LastYearSchema(UNQSNO) + ".t_txn b, " + CommVar.LastYearSchema(UNQSNO) + ".t_cntrl_hdr c, " + CommVar.FinSchemaPrevYr(UNQSNO) + ".m_subleg d, " + CommVar.LastYearSchema(UNQSNO) + ".t_txnoth e ";
                sql += "where a.autono = b.autono(+) and a.autono = c.autono(+) and b.slcd = d.slcd(+) and listdiscper<>0 and b.autono=e.autono(+) and  ";
                if (docdt != "") sql += "c.docdt <= to_date('" + docdt + "','dd/mm/yyyy') and ";
                //sql += "c.compcd='" + COM + "' and d.slcd = '" + slcd + "' and b.doctag in ('SB') ";
                sql += "c.compcd='" + COM + "' and d.slcd = '" + slcd + "' ";
                if (doctag.retStr() != "") sql += "and b.doctag in (" + doctag + ") ";
                if (bltype.retStr() != "") sql += "and e.bltype in (" + bltype + ") ";
                sql += "group by b.slcd, c.docdt, a.scmdiscrate,a.scmdisctype,a.listdiscper ";
            }
            sql += ") where rn = 1 ) y, ";


            //sql += "(select a.effdt, a.prccd, a.itmprccd, a.prcdesc from ";
            //sql += "(select a.effdt, a.prccd, a.itmprccd, a.prcdesc, ";
            //sql += "row_number() over (partition by a.prccd order by a.effdt desc) as rn ";
            //sql += "from " + scm + ".m_itemplist a ";
            //sql += "where a.itgrpcd='" + itgrpcd + "' ";
            //if (docdt != "") sql += "and a.effdt <= to_date('" + docdt.Substring(0, 10) + "','dd/mm/yyyy') ";
            //sql += ") a where a.rn=1) d, ";

            sql += "" + scmf + ".m_taxgrp e, " + scmf + ".m_prclst f, " + scmf + ".m_subleg g, " + scmf + ".m_subleg h, " + scmf + ".m_subleg i, " + scmf + ".m_subleg j ";
            sql += "where z.slcd=a.slcd(+) and z.slcd=b.slcd(+) and z.slcd=c.slcd(+) and ";
            sql += "b.taxgrpcd=e.taxgrpcd(+) and a.prccd=f.prccd(+) and a.slcd=y.slcd(+) and ";
            sql += "z.slcd=g.slcd(+) and a.agslcd=h.slcd(+) and b.trslcd=i.slcd(+) and b.courcd=j.slcd(+) ";

            tbl = SQLquery(sql);

            return tbl;
        }
        public List<DropDown_list_DelvType> GetforDelvTypeSelection(string delvtype = "")
        {
            List<DropDown_list_DelvType> DropDown_list_desType = new List<DropDown_list_DelvType>();
            DropDown_list_DelvType DropDown_list0 = new DropDown_list_DelvType();
            DropDown_list0.value = "C";
            DropDown_list0.text = "COD";
            DropDown_list_desType.Add(DropDown_list0);
            DropDown_list_DelvType DropDown_list1 = new DropDown_list_DelvType();
            DropDown_list1.value = "T";
            DropDown_list1.text = "TO PAY";
            DropDown_list_desType.Add(DropDown_list1);
            DropDown_list_DelvType DropDown_list2 = new DropDown_list_DelvType();
            DropDown_list2.value = "P";
            DropDown_list2.text = "PAID BILTY";
            DropDown_list_desType.Add(DropDown_list2);
            DropDown_list_DelvType DropDown_list3 = new DropDown_list_DelvType();
            DropDown_list3.value = "H";
            DropDown_list3.text = "HOLD BILTY";
            DropDown_list_desType.Add(DropDown_list3);
            DropDown_list_DelvType DropDown_list4 = new DropDown_list_DelvType();
            DropDown_list4.value = "";
            DropDown_list4.text = "";
            DropDown_list_desType.Add(DropDown_list4);
            return DropDown_list_desType;
        }
        public string retDelvTypeDesc(string delvtype)
        {
            string rval = "";
            switch (delvtype)
            {
                case "C":
                    rval = "COD"; break;
                case "T":
                    rval = "TO PAY"; break;
                case "P":
                    rval = "PAID BILTY"; break;
                case "H":
                    rval = "HOLD BILTY"; break;
                default:
                    rval = ""; break;
            }
            return rval;
        }
        public DataTable GetPendChallans(string jobcd = "", string slcd = "", string chlnpupto = "", string blautono = "", string txnupto = "", string skipautono = "", bool OnlyBal = true, bool shortallowadj = false, string curschema = "", string finschema = "", string fdt = "", string tdt = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (curschema != "") scm1 = curschema;
            if (finschema != "") scmf = finschema;

            if (chlnpupto == "") chlnpupto = txnupto;

            if (slcd.retStr() != "" && slcd.IndexOf("'") < 0) slcd = "'" + slcd + "'";
            string sql = "";
            if (fdt == null) fdt = "";
            if (tdt == null) tdt = "";
            sql += "select a.autono, b.itcd, b.partcd, f.itgrpcd, i.itgrpnm, f.styleno, f.itnm, f.hsncode, f.uomcd, j.uomnm, j.decimals, g.partnm, c.slcd, e.slnm, ";
            if (shortallowadj == false) sql += "b.qnty, nvl(b.shortqnty,0) shortqnty, nvl(y.short_allow,0) short_allow, ";
            else sql += "b.qnty+nvl(y.short_allow,0) qnty, nvl(b.shortqnty,0)-nvl(y.short_allow,0) shortqnty, nvl(y.short_allow,0) short_allow, ";
            sql += "d.docno, d.docdt, c.prefno, c.prefdt, b.progautono, b.progautono issautono, h.docno issdocno, h.docdt issdocdt,b.nos,b.rate from ";

            sql += "(select a.autono ";
            sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b ";
            sql += "where a.autono=b.autono and b.compcd='" + COM + "' and ";
            sql += "b.docdt <= to_date('" + chlnpupto + "','dd/mm/yyyy') and a.doctag in ('JR') and ";
            if (jobcd.retStr() != "") sql += "a.jobcd ='" + jobcd + "' and ";
            if (slcd.retStr() != "") sql += "a.slcd in (" + slcd + ") and ";
            if (skipautono.retStr() != "") sql += "a.autono <> '" + skipautono + "' and ";
            sql += "nvl(b.cancel,'N')='N' ) a, ";

            sql += "(select a.autono, a.progautono, d.itcd, b.partcd, a.progautono||c.itcd||nvl(b.partcd,'') progitcd, sum(b.nos) nos, sum(b.qnty) qnty, sum(b.shortqnty) shortqnty,b.rate ";
            sql += "from " + scm1 + ".t_progdtl a, " + scm1 + ".t_batchdtl b, " + scm1 + ".t_progmast c, " + scm1 + ".t_batchmst d ";
            sql += "where a.autono=b.autono(+) and a.progautono=b.recprogautono(+) and a.progslno=b.recprogslno(+) and a.progautono||a.progslno=c.autono||c.slno and b.barno=d.barno(+) and nvl(c.sample,'N') <> 'Y' ";
            sql += "group by a.autono, a.progautono, d.itcd, b.partcd, a.progautono||c.itcd||nvl(b.partcd,''),b.rate ) b, ";

            sql += "(select a.recautono, a.progautono, b.itcd, b.partcd, a.progautono||b.itcd||nvl(b.partcd,'') progitcd, sum(a.short_allow) short_allow ";
            sql += "from " + scm1 + ".t_prog_close a, " + scm1 + ".t_progmast b where a.progautono=b.autono(+) and a.progslno=b.slno(+) and a.recautono is not null ";
            sql += "group by a.recautono, a.progautono, b.itcd, b.partcd, a.progautono||b.itcd||nvl(b.partcd,'') ) y, ";

            sql += "(select a.linkautono, max(a.autono) autono ";
            sql += "from " + scm1 + ".t_txn_linkno a, " + scm1 + ".t_cntrl_hdr b ";
            sql += "where a.autono=b.autono and ";
            if (txnupto.retStr() != "") sql += "b.docdt <= to_date('" + txnupto + "',dd/mm/yyyy') and ";
            if (blautono.retStr() != "") sql += "a.autono = '" + blautono + "' and ";
            sql += "nvl(b.cancel,'N')='N' group by a.linkautono ) z, ";

            sql += "" + scm1 + ".t_txn c, " + scm1 + ".t_cntrl_hdr d, " + scmf + ".m_subleg e, " + scm1 + ".m_sitem f, " + scm1 + ".m_parts g, ";
            sql += scm1 + ".t_cntrl_hdr h, " + scm1 + ".m_group i, " + scmf + ".m_uom j ";
            sql += "where a.autono=b.autono(+) and b.progautono=h.autono(+) and ";
            sql += "a.autono=c.autono and a.autono=d.autono and c.slcd=e.slcd(+) and ";
            sql += "b.itcd is not null and f.itgrpcd=i.itgrpcd(+) and f.uomcd=j.uomcd(+) and ";
            if (fdt != "") sql += "d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and ";
            if (tdt != "") sql += "d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
            sql += "b.itcd=f.itcd(+) and b.partcd=g.partcd(+) and ";
            sql += "b.autono=y.recautono(+) and b.progitcd=y.progitcd(+) and ";
            sql += "a.autono=z.linkautono(+) and ";
            if (blautono.retStr() == "") sql += "z.autono is null "; else sql += "z.autono is not null ";
            sql += "order by docdt, docno ";

            tbl = SQLquery(sql);

            return tbl;
        }
        public double Disc_Cal(string TYPE, double RATE, double QNTY, double BOX, double AMOUNT)
        {
            double DISC_AMT = 0;
            if (TYPE == "Q") { DISC_AMT = RATE * QNTY; }
            else if (TYPE == "B") { DISC_AMT = RATE * BOX * QNTY; }
            else if (TYPE == "P") { DISC_AMT = (AMOUNT * RATE) / 100; }
            else if (TYPE == "F") { DISC_AMT = RATE; }
            else { DISC_AMT = 0; }
            return DISC_AMT;
        }
        public DataTable getPendProg(string tdt, string txnupto = "", string slcd = "", string itcd = "", string jobcd = "", string skipautono = "", string progfromdt = "", string linecd = "", string curschema = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (curschema != "") scm = curschema;

            string sql = "";
            sql += "select a.progautono, a.progslno, a.progautoslno, d.slcd, i.slnm, nvl(i.slarea,i.district) slarea, ";
            sql += "d.slcd||nvl(d.linecd,'') repslcd, i.slnm||decode(k.linenm,null,'',' ['||k.linenm||']') repslnm, ";
            sql += "e.docno, e.docdt, d.itcd, f.styleno, f.itnm, f.itgrpcd, g.itgrpnm,g.bargentype, f.uomcd, j.itnm fabitnm, ";
            sql += "d.sizecd, d.partcd, d.colrcd,  h.colrnm, d.cutlength, d.dia, d.shade, d.ordautono, d.ordslno, d.barno, m.commonuniqbar, d.linecd, l.print_seq, ";
            sql += "a.balqnty, a.balnos,d.itremark,d.proguniqno,d.sample,l.sizenm from ";

            sql += "(select a.progautono, a.progslno, a.progautono||a.progslno progautoslno, ";
            sql += "sum(case a.stkdrcr when 'C' then a.qnty when 'D' then a.qnty*-1 end) balqnty, ";
            sql += "sum(case a.stkdrcr when 'C' then a.nos when 'D' then a.nos*-1 end) balnos ";
            sql += "from " + scm + ".t_progdtl a, " + scm + ".t_progmast b, " + scm + ".t_cntrl_hdr c ";
            sql += "where a.progautono=b.autono(+) and a.progslno=b.slno(+) and a.autono=c.autono(+) and ";
            sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
            if (skipautono.retStr() != "") sql += "a.autono <> '" + skipautono + "' and ";
            if (jobcd.retStr() != "") sql += "b.jobcd in (" + jobcd + ") and ";
            if (txnupto.retStr() != "") sql += "b.docdt <= to_date('" + txnupto + "', 'dd/mm/yyyy') and ";
            sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' ";
            sql += "group by a.progautono, a.progslno, a.progautono||a.progslno ) a, ";

            sql += scm + ".t_progmast d, " + scm + ".t_cntrl_hdr e, ";
            sql += scm + ".m_sitem f, " + scm + ".m_group g, " + scm + ".m_color h, " + scmf + ".m_subleg i, " + scm + ".m_sitem j, ";
            sql += scm + ".m_linemast k, " + scm + ".m_size l, " + scm + ".t_batchmst m ";
            sql += "where a.progautono=d.autono(+) and a.progslno=d.slno(+) and a.progautono=e.autono(+) and d.barno=m.barno(+) and ";
            sql += "d.itcd=f.itcd(+) and f.itgrpcd=g.itgrpcd(+) and d.colrcd=h.colrcd(+) and d.slcd=i.slcd(+) and ";
            if (progfromdt.retStr() != "") sql += "e.docdt >= to_date('" + progfromdt + "', 'dd/mm/yyyy') and ";
            if (slcd.retStr() != "") sql += "d.slcd in (" + slcd + ") and ";
            if (linecd.retStr() != "") sql += "d.linecd in (" + linecd + ") and ";
            if (itcd.retStr() != "") sql += "d.itcd in (" + itcd + ") and ";
            sql += "nvl(a.balqnty,0) <> 0 ";
            sql += "and f.fabitcd=j.itcd(+) and d.linecd=k.linecd(+) and d.sizecd=l.sizecd(+) ";
            sql += "order by styleno, itnm, itcd, partcd, print_seq, sizenm ";
            DataTable tbl = SQLquery(sql);
            return tbl;
        }
        public DataTable GetShortageExcessData(string JOBBER, string curautono = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string scm = CommVar.CurSchema(UNQSNO); string query = "";
            query += "select a.autono, a.slno, a.autoslno, a.qnty, b.recautono, f.docno, f.docdt, e.itcd, g.styleno, g.itnm, e.partcd, e.linecd, h.docno recdocno, h.docdt recdodt, ";
            query += "h.docno recdocno, h.docdt recdodt, b.recslno, nvl(b.qnty, 0) recqnty, nvl(b.shortqnty, 0) shortqnty, nvl(d.short_allow, 0) short_allow ";
            query += "from ";
            query += "(select a.autono, a.slno, a.autono || a.slno autoslno, a.qnty from " + scm + ".t_progmast a ) a, ";
            query += "(select a.progautono, a.progslno, a.progautono || a.progslno progautoslno, nvl(b.recprogautono, b.autono) recautono, nvl(b.recprogslno, b.slno) recslno, ";
            query += "sum(b.qnty) qnty, sum(b.shortqnty) shortqnty ";
            query += "from " + scm + ".t_progdtl a, " + scm + ".t_txndtl b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr z ";
            query += "where a.autono = b.autono(+) and a.slno = b.slno(+) and ";
            query += "a.autono = c.autono(+) and c.doctag in ('JR','JU') and a.autono = z.autono(+) and nvl(z.cancel, 'N')= 'N' ";
            query += "group by a.progautono, a.progslno, a.progautono || a.progslno, nvl(b.recprogautono, b.autono), nvl(b.recprogslno, b.slno) ) b, ";
            query += "(select b.progautono, b.progslno, b.progautono || b.progslno progautoslno,";
            query += "sum(b.short_allow) short_allow ";
            query += "from " + scm + ".t_prog_close b, " + scm + ".t_prog_close_hdr c, " + scm + ".t_cntrl_hdr z ";
            query += "where b.autono = c.autono(+) and b.autono = z.autono(+) and nvl(z.cancel, 'N')= 'N' ";
            if (curautono != "") { query += "and b.autono <> '" + curautono + "' "; }
            query += "group by b.progautono, b.progslno, b.progautono || b.progslno) d, ";
            query += "" + scm + ".t_progmast e, " + scm + ".t_cntrl_hdr f, " + scm + ".m_sitem g, " + scm + ".t_cntrl_hdr h ";
            query += "where a.autoslno = b.progautoslno(+) and a.autoslno = d.progautoslno(+) and a.autono = e.autono(+) and a.slno = e.slno(+) and ";
            query += "e.slcd = '" + JOBBER + "' and ";
            query += "a.autono = f.autono(+) and e.itcd = g.itcd(+) and b.recautono = h.autono(+) ";
            query += "order by autoslno ";

            DataTable tbl = SQLquery(query);
            return tbl;
        }
        public string retsizedata(DataTable tblsizedata, string itcd, string mixsize, string sizestr, double pcsperbox)
        {
            string rval = "";
            sizestr = sizestr.Substring(0, sizestr.Length - 1);
            //string sizedata = chk1 + Cn.GCS() + tbl.Rows[i]["mixsize"] + Cn.GCS() + "" + Cn.GCS() + tbl.Rows[1]["pcsperbox"] + Cn.GCS() + tbl.Rows[1]["pcsperset"] + Cn.GCS(); ;
            if (mixsize == "S")
            {
                rval = sizestr;
            }
            else
            {
                rval = sizestr;
            }
            return rval;
        }
        public DataTable retSizeGrpData(string itcd = "", string itgrpcd = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string sql = "";
            string scm = CommVar.CurSchema(UNQSNO);
            string selitcd = itcd.retStr();
            string selitgrpcd = itgrpcd.retStr();

            sql += "select * from " + scm + ".v_msitem_sizegrp a where ";
            if (selitcd != "") sql += "a.itcd in (" + selitcd + ") and ";
            if (selitgrpcd != "") sql += "a.itgrpcd in (" + selitgrpcd + ") and ";
            sql += "'a'='a' ";
            sql += "order by itgrpcd, itgrpnm ";
            DataTable tbl = masterHelpFa.SQLquery(sql);
            return tbl;
        }
        public DataTable GetPendChallansPurchase(string slcd = "", string chlnpupto = "", string blautono = "", string txnupto = "", string skipautono = "", bool OnlyBal = true, bool shortallowadj = false, string curschema = "", string finschema = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (curschema != "") scm1 = curschema;
            if (finschema != "") scmf = finschema;

            if (chlnpupto == "") chlnpupto = txnupto;

            if (slcd.retStr() != "" && slcd.IndexOf("'") < 0) slcd = "'" + slcd + "'";
            string sql = "";


            sql += "select a.autono, b.itcd, b.partcd, f.styleno, f.itnm, f.hsnsaccd, f.uomcd, f.pcsperbox, g.partnm, c.slcd, e.slnm, ";
            if (shortallowadj == false) sql += "b.qnty, nvl(b.shortqnty,0) shortqnty, 0 short_allow, ";
            else sql += "b.qnty qnty, nvl(b.shortqnty,0) shortqnty, 0 short_allow, ";
            sql += "d.docno, d.docdt, c.prefno, c.prefdt, b.progautono, b.progautono issautono, h.docno issdocno, h.docdt issdocdt from ";

            sql += "(select a.autono ";
            sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b ";
            sql += "where a.autono=b.autono and b.compcd='" + COM + "' and ";
            sql += "b.docdt <= to_date('" + chlnpupto + "','dd/mm/yyyy') and a.doctag in ('PB') and ";

            if (slcd.retStr() != "") sql += "a.slcd in (" + slcd + ") and ";
            if (skipautono.retStr() != "") sql += "a.autono <> '" + skipautono + "' and ";
            sql += "nvl(b.cancel,'N')='N' ) a, ";

            sql += "(select b.autono, b.autono progautono, b.itcd, b.partcd, b.autono||b.itcd||nvl(b.partcd,'') progitcd, sum(b.qnty) qnty, sum(b.shortqnty) shortqnty ";
            sql += "from " + scm1 + ".t_txndtl b, " + scm1 + ".m_sitem c, " + scm1 + ".m_group d ";
            sql += "where b.itcd=c.itcd(+) and c.itgrpcd=d.itgrpcd(+) and d.itgrptype in ('F') ";
            sql += "group by b.autono, b.autono, b.itcd, b.partcd, b.autono||b.itcd||nvl(b.partcd,'') ) b, ";

            sql += "" + scm1 + ".t_txn c, " + scm1 + ".t_cntrl_hdr d, " + scmf + ".m_subleg e, " + scm1 + ".m_sitem f, " + scm1 + ".m_parts g, " + scm1 + ".t_cntrl_hdr h ";
            sql += "where a.autono=b.autono(+) and b.progautono=h.autono(+) and ";
            sql += "a.autono=c.autono and a.autono=d.autono and c.slcd=e.slcd(+) and ";
            sql += "b.itcd=f.itcd(+) and b.partcd=g.partcd(+) ";

            if (blautono.retStr() == "")
            {
                sql += "and a.autono not in ( ";
                sql += "select a.linkautono ";
                sql += "from " + scm1 + ".t_txn_linkno a, " + scm1 + ".t_cntrl_hdr b ";
                sql += "where a.autono=b.autono and ";
                if (txnupto.retStr() != "") sql += "b.docdt <= to_date('" + txnupto + "',dd/mm/yyyy') and ";
                sql += "nvl(b.cancel,'N')='N' ) ";
            }
            else
            {
                sql += "and a.autono in ( ";
                sql += "select a.linkautono ";
                sql += "from " + scm1 + ".t_txn_linkno a, " + scm1 + ".t_cntrl_hdr b ";
                sql += "where a.autono=b.autono and ";
                if (txnupto.retStr() != "") sql += "b.docdt <= to_date('" + txnupto + "',dd/mm/yyyy') and ";
                sql += "a.autono = '" + blautono + "' ) ";
            }
            sql += "order by docdt, docno ";

            tbl = SQLquery(sql);

            return tbl;
        }

        public string retsizemaxmin(string sizecdgrp)
        {
            string chkval = sizecdgrp.Replace("^", "");
            string rval = "";
            string[] chk1 = chkval.Split(',');
            rval = chk1[0];
            if (chk1.Count() > 1)
            {
                rval = rval + "-" + chk1[chk1.Count() - 1];
            }
            if (rval == "") rval = sizecdgrp;
            return rval;
        }
        public string retSDGlcd(string slcd)
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            double rtval = 0;
            string sql = "", glcd = "", scm = CommVar.CurSchema(UNQSNO);
            sql = "select max(parglcd) glcd ";
            sql += "from " + scm + ".t_txn a, " + scm + ".t_cntrl_hdr b, " + scm + ".m_doctype c ";
            sql += "where a.autono=b.autono and b.doccd=c.doccd and c.doctype in ('SBILL') ";
            DataTable tbl = SQLquery(sql);
            glcd = tbl.Rows[0]["glcd"].retStr();
            return glcd;
        }
        public DataTable retCutterBalFifo(string selslcd = "", string tdt = "", string curschema = "")
        {
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string sql = "", fdt = CommVar.FinStartDate(UNQSNO);
            if (curschema != "") scm = curschema;

            sql = "select a.autono, a.partcd, c.docdt, c.docno, c.doconlyno, c.doccd, b.doctag, b.slcd, g.slnm, g.gstno, ";
            sql += "a.itgrptype, a.itgrpcd, f.itgrpnm, f.hsnsaccd, a.stkdrcr, a.uomcd, b.jobcd, h.jobnm, ";
            sql += "nvl(b.cloth_used,0) cloth_used, nvl(b.cloth_was,0) cloth_was, ";
            sql += "nvl(a.wght, 0) wght, nvl(decode(f.itgrptype, 'F', a.qnty, 0),0) pcs, ";
            sql += "(case f.itgrptype when 'T' then 0 when 'W' then 0 when 'F' then 0 else nvl(a.qnty, 0) end) qnty, ";
            sql += "nvl(decode(f.itgrptype, 'T', a.qnty,0),0) foldwt, nvl(decode(f.itgrptype, 'W', a.qnty),0) waswt from ";

            sql += "( select a.autono, e.itgrpcd, e.uomcd, f.itgrptype, a.partcd, a.stkdrcr, sum(a.wght) wght, sum(a.qnty) qnty ";
            sql += "from " + scm + ".t_txndtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".m_doctype c, " + scm + ".t_txn d, ";
            sql += scm + ".m_sitem e, " + scm + ".m_group f ";
            sql += "where a.autono=b.autono(+) and b.doccd=c.doccd(+) and a.autono=d.autono(+) and ";
            sql += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' and nvl(b.cancel,'N')='N' and "; // nvl(c.pro,'O') <> 'I' and ";
            if (tdt != "") sql += "b.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
            sql += "a.itcd=e.itcd and e.itgrpcd=f.itgrpcd(+) and ";
            if (selslcd != "") sql += "b.slcd in (" + selslcd + ") and ";
            sql += "c.doctype in ('OCTI','OCTR','OCTU') and d.doctag in ('JC','JR','JU') ";
            sql += "group by a.autono, e.itgrpcd, e.uomcd, f.itgrptype, a.partcd, a.stkdrcr ) a, ";
            sql += "" + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_parts e, ";
            sql += "" + scm + ".m_group f, " + scmf + ".m_subleg g, " + scm + ".m_jobmst h ";
            sql += "where a.autono=b.autono and b.autono=c.autono and a.itgrpcd=f.itgrpcd and ";
            if (tdt != "") sql += "c.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and ";
            sql += "a.partcd=e.partcd(+) and b.slcd=g.slcd(+) and b.jobcd=h.jobcd(+) ";
            sql += "order by slcd, docdt, docno ";

            int maxR = 0, i = 0;

            DataTable rsiss = new DataTable("stock");
            rsiss.Columns.Add("autono", typeof(string), "");
            rsiss.Columns.Add("uomcd", typeof(string), "");
            rsiss.Columns.Add("itgrpcd", typeof(string), "");
            rsiss.Columns.Add("itgrpnm", typeof(string), "");
            rsiss.Columns.Add("slcd", typeof(string), "");
            rsiss.Columns.Add("gstno", typeof(string), "");
            rsiss.Columns.Add("docno", typeof(string), "");
            rsiss.Columns.Add("docdt", typeof(string), "");
            rsiss.Columns.Add("balqnty", typeof(double), "");
            rsiss.Columns.Add("qnty", typeof(double), "");
            rsiss.Columns.Add("befrecqnty", typeof(double), "");
            rsiss.Columns.Add("recqnty", typeof(double), "");
            rsiss.Columns.Add("shortuomcd", typeof(string), "");
            rsiss.Columns.Add("shortqnty", typeof(double), "");
            rsiss.Columns.Add("recautono", typeof(string), "");

            string sqld = "select autono, slcd, gstno, itgrpcd, itgrpnm, partcd, uomcd, qnty, docno, docdt, doccd ";
            sqld += "from ( " + sql + ") where doctag = 'JC' ";
            sqld += "order by slcd, uomcd, docdt, docno ";
            DataTable fifotbl = masterHelpFa.SQLquery(sqld);
            Int32 f = 0, rNo = 0, oldf = 0;
            maxR = fifotbl.Rows.Count - 1;
            while (f <= maxR)
            {
                rsiss.Rows.Add(); rNo = rsiss.Rows.Count - 1;
                rsiss.Rows[rNo]["autono"] = fifotbl.Rows[f]["autono"];
                rsiss.Rows[rNo]["slcd"] = fifotbl.Rows[f]["slcd"];
                rsiss.Rows[rNo]["gstno"] = fifotbl.Rows[f]["gstno"];
                rsiss.Rows[rNo]["uomcd"] = fifotbl.Rows[f]["uomcd"];
                rsiss.Rows[rNo]["itgrpcd"] = fifotbl.Rows[f]["itgrpcd"];
                rsiss.Rows[rNo]["itgrpnm"] = fifotbl.Rows[f]["itgrpnm"];
                rsiss.Rows[rNo]["docno"] = fifotbl.Rows[f]["docno"];
                rsiss.Rows[rNo]["docdt"] = fifotbl.Rows[f]["docdt"].ToString().retDateStr();
                rsiss.Rows[rNo]["qnty"] = Convert.ToDouble(fifotbl.Rows[f]["qnty"]);
                rsiss.Rows[rNo]["balqnty"] = Convert.ToDouble(fifotbl.Rows[f]["qnty"]);
                rsiss.Rows[rNo]["befrecqnty"] = 0;
                rsiss.Rows[rNo]["recqnty"] = 0;
                rsiss.Rows[rNo]["shortqnty"] = 0;
                f++;
            }

            sqld = "select autono, itgrpcd, itgrpnm, partcd, uomcd, qnty, slcd, docno, docdt, doccd, jobnm, ";
            sqld += "hsnsaccd, doconlyno, wght, pcs, waswt, foldwt, cloth_used, cloth_was ";
            sqld += "from ( " + sql + ") where doctag <> 'JC' ";
            //and docdt >= to_date('" + CommVar.FinStartDate(UNQSNO) + "','dd/mm/yyyy') ";
            sqld += "order by slcd, docdt, docno, partcd ";
            DataTable fiforec = masterHelpFa.SQLquery(sqld);
            f = 0; maxR = fiforec.Rows.Count - 1; rNo = 0;
            while (f <= maxR)
            {
                string autono = fiforec.Rows[f]["autono"].ToString();
                string slcd = fiforec.Rows[f]["slcd"].ToString();
                string partcd = "";
                DateTime recdt = Convert.ToDateTime(fiforec.Rows[f]["docdt"].ToString());
                double cl_used = Convert.ToDouble(fiforec.Rows[f]["cloth_used"].ToString());
                double cl_was = Convert.ToDouble(fiforec.Rows[f]["cloth_was"].ToString());
                double r_qty = 0, w_qty = 0;
                bool hdrrecoins = true;

                while (fiforec.Rows[f]["autono"].ToString() == autono)
                {
                    bool recins = true;
                    if (recins == true)
                    {
                        r_qty = r_qty + (fiforec.Rows[f]["wght"].retDbl() == 0 ? fiforec.Rows[f]["qnty"].retDbl() : fiforec.Rows[f]["wght"].retDbl()) + fiforec.Rows[f]["foldwt"].retDbl();
                        w_qty = w_qty + fiforec.Rows[f]["waswt"].retDbl();
                    }
                    f++;
                    if (f > maxR) break;
                }
                oldf = f;
                f = f - 1;
                Int32 maxl = 0;
                if (cl_used + cl_was > 0) maxl = 1;
                for (Int32 l = 0; l <= maxl; l++)
                {
                    Int16 isnlo = 1;
                    string chkuomcd = "KGS";
                    double chkqty = r_qty, chkwas = w_qty;

                    if (l == 1)
                    {
                        isnlo = 51;
                        chkuomcd = "MTR";
                        chkqty = cl_used; chkwas = cl_was;
                    }
                    double checkqnty = chkqty + chkwas, rplqnty = 0;
                    for (Int32 z = 0; z <= rsiss.Rows.Count - 1; z++)
                    {
                        double balqnty = Convert.ToDouble(rsiss.Rows[z]["balqnty"].ToString());
                        if (rsiss.Rows[z]["slcd"].ToString() == slcd && rsiss.Rows[z]["uomcd"].ToString() == chkuomcd && balqnty > 0)
                        {
                            if (balqnty <= checkqnty)
                            {
                                checkqnty = checkqnty - balqnty; rplqnty = balqnty; balqnty = 0;
                                balqnty = 0;
                            }
                            else
                            {
                                balqnty = balqnty - checkqnty; rplqnty = checkqnty; checkqnty = 0;
                            }
                            rsiss.Rows[z]["balqnty"] = balqnty;
                            if (recdt < Convert.ToDateTime(fdt))
                            {
                                rsiss.Rows[z]["befrecqnty"] = Convert.ToDouble(rsiss.Rows[z]["befrecqnty"]) + rplqnty;
                            }
                            else
                            {
                                rsiss.Rows[z]["recqnty"] = Convert.ToDouble(rsiss.Rows[z]["recqnty"]) + rplqnty;
                                if (hdrrecoins == true)
                                {
                                    hdrrecoins = false;
                                }

                                if (rplqnty != 0)
                                {
                                    double rate = 0;
                                    double amt = (rplqnty * rate).toRound(2);

                                    double shortqnty = chkwas, qnty = rplqnty;
                                    string shortuomcd = "";
                                    if (shortqnty > rplqnty)
                                    {
                                        shortqnty = 0;
                                    }
                                    else
                                    {
                                        rplqnty = rplqnty - shortqnty;
                                        chkwas = 0;
                                    }
                                    qnty = rplqnty;
                                    if (shortqnty < 0)
                                    {
                                        //qnty = rplqnty + Math.Abs(shortqnty);
                                        shortqnty = 0;
                                    }

                                    if (shortqnty > 0) shortuomcd = rsiss.Rows[z]["uomcd"].ToString();
                                    isnlo++;
                                }
                                //
                            }
                            if (checkqnty == 0) break;
                        }
                    }
                } //kgs/mtr loop
                f++;
                if (f > maxR) break;
            }
            return rsiss;
        }
        public string retCaseNos(DataTable pslipdata)
        {
            string lastdocno = "";
            string casenos = "";
            if (pslipdata != null)
            {
                if (pslipdata.Rows.Count > 0)
                {
                    casenos = "";
                    casenos += pslipdata.Rows[0]["doconlyno"].ToString();
                    double startno = 0, checkno = 0, usedchkno = 0;
                    startno = Convert.ToDouble(pslipdata.Rows[0]["vchrno"]);
                    checkno = startno;
                    usedchkno = checkno;
                    for (int x = 0; x <= pslipdata.Rows.Count - 1; x++)
                    {
                        startno = Convert.ToDouble(pslipdata.Rows[x]["vchrno"]);
                        if (startno != checkno)
                        {
                            if (checkno - 1 != usedchkno) casenos += "-" + (checkno - 1).ToString(); // pslipdata.Rows[x]["doconlyno"].ToString();
                            checkno = Convert.ToDouble(pslipdata.Rows[x]["vchrno"]);
                            casenos = casenos + "," + pslipdata.Rows[x]["doconlyno"].ToString();
                            usedchkno = checkno;
                        }
                        lastdocno = pslipdata.Rows[x]["doconlyno"].ToString();
                        checkno++;
                        //noofcases++;
                    }
                    casenos += "-" + lastdocno;
                }
            }
            return casenos;
        }
        public double ConvPcstoBox(double pcs, double pcsperbox)
        {
            double box = 0;
            double dbDzn, dbPcs, zDzn = 0;
            string txt1, txt2 = "";
            if (pcsperbox == 0)
                return 0;
            if (pcs == 0) dbPcs = 0; else dbPcs = Cn.Roundoff(pcs / pcsperbox, 2);

            txt1 = dbPcs.ToString("0.00");
            txt1 = txt1.Substring(0, txt1.Length - 3);
            txt2 = (pcs - Convert.ToDouble(txt1) * pcsperbox).ToString("0");

            if (pcsperbox != 10)
            {
                if (Convert.ToDouble(txt2) < 10) box = Convert.ToDouble(txt1 + ".0" + txt2.Substring(txt2.Length - 1));
                else box = Convert.ToDouble(txt1 + "." + txt2);
            }
            else
            {
                box = Convert.ToDouble(txt1 + ".0" + txt2.Substring(txt2.Length - 1));
            }
            return box;
        }
        public double ConvPcstoSet(double pcs, double pcsperset)
        {
            double box = 0;
            double dbDzn, dbPcs, zDzn = 0;
            string txt1, txt2 = "";
            if (pcsperset == 0)
                return 0;
            if (pcs == 0) dbPcs = 0; else dbPcs = Cn.Roundoff(pcs / pcsperset, 2);

            txt1 = dbPcs.ToString("0.00");
            txt1 = txt1.Substring(0, txt1.Length - 3);
            txt2 = (pcs - Convert.ToDouble(txt1) * pcsperset).ToString("0");

            if (pcsperset != 10)
            {
                if (Convert.ToDouble(txt2) < 10) box = Convert.ToDouble(txt1 + ".0" + txt2.Substring(txt2.Length - 1));
                else box = Convert.ToDouble(txt1 + "." + txt2);
            }
            else
            {
                box = Convert.ToDouble(txt1 + ".0" + txt2.Substring(txt2.Length - 1));
            }
            return box;
        }
        public double retMaxLateDays(DataTable ostbl, string slcd = "", bool tblnull = false)
        {
            double rtval = 0;
            if (tblnull == true)
            {
                string glcd = retSDGlcd(slcd);
                ostbl = GenOSTbl(glcd, slcd, "", "", "", "", "", "", "Y", "", "", "", "", "", true);
            }

            if (ostbl.Rows.Count == 0) return rtval;

            DateTime[] dt = (from DataRow dr in ostbl.Rows
                             where dr["lrdt"].retStr() != "" && dr["drcr"].retStr() == "D"
                             select Convert.ToDateTime(dr["lrdt"])).ToArray();

            if (dt.Count() > 0)
            {
                DateTime minlrdt = dt.Distinct().Min();
                TimeSpan TSdys = System.DateTime.Now - minlrdt;
                rtval = TSdys.Days;
            }
            if (rtval < 0) rtval = 0;
            return rtval;
        }
        //public DataTable GetStock(string tdt, string gocd = "", string barno = "", string itcd = "", string mtrljobcd = "'FS'", string skipautono = "", string itgrpcd = "", string stylelike = "", string prccd = "WP", string taxgrpcd = "C001", string stktype = "", string brandcd = "", bool pendpslipconsider = true, bool shownilstock = false, string curschema = "", string finschema = "", bool mergeitem = false, bool mergeloca = false, bool exactbarno = true, string partcd = "", bool showallitems = false)
        //{
        //    //showbatchno = true;
        //    string UNQSNO = CommVar.getQueryStringUNQSNO();
        //    DataTable tbl = new DataTable();
        //    string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
        //    string sql = "";
        //    //bool showallitems = true;

        //    if (curschema != "") scm = curschema;
        //    if (finschema != "") scmf = finschema;
        //    string itcdqry = "a.itcd ", itcdpipe = "a.itcd";
        //    if (mergeitem == true)
        //    {
        //        itcdqry = "nvl(e.linkitcd,a.itcd) ";
        //        itcdpipe = "nvl(e.linkitcd,a.itcd)";
        //    }
        //    #region Generate Stock Table
        //    DataTable rsstk = new DataTable();
        //    rsstk.Columns.Add("key", typeof(string), "");
        //    rsstk.Columns.Add("itcd", typeof(string), "");
        //    rsstk.Columns.Add("partcd", typeof(string), "");
        //    rsstk.Columns.Add("partnm", typeof(string), "");
        //    rsstk.Columns.Add("mtrljobcd", typeof(string), "");
        //    rsstk.Columns.Add("jobnm", typeof(string), "");
        //    rsstk.Columns.Add("jobseq", typeof(string), "");
        //    rsstk.Columns.Add("stktype", typeof(string), "");
        //    rsstk.Columns.Add("sizecd", typeof(string), "");
        //    rsstk.Columns.Add("colrcd", typeof(string), "");
        //    rsstk.Columns.Add("itcolsize", typeof(string), "");
        //    rsstk.Columns.Add("rate", typeof(double), "");
        //    rsstk.Columns.Add("itnm", typeof(string), "");
        //    rsstk.Columns.Add("styleno", typeof(string), "");
        //    rsstk.Columns.Add("mixsize", typeof(string), "");
        //    rsstk.Columns.Add("uomcd", typeof(string), "");
        //    rsstk.Columns.Add("uomnm", typeof(string), "");
        //    rsstk.Columns.Add("pcsperbox", typeof(double), "");
        //    rsstk.Columns.Add("decimals", typeof(double), "");
        //    rsstk.Columns.Add("pcsperset", typeof(double), "");
        //    rsstk.Columns.Add("mergepcs", typeof(double), "");
        //    rsstk.Columns.Add("itgrpcd", typeof(string), "");
        //    rsstk.Columns.Add("itgrpnm", typeof(string), "");
        //    rsstk.Columns.Add("brandcd", typeof(string), "");
        //    rsstk.Columns.Add("brandnm", typeof(string), "");
        //    rsstk.Columns.Add("slcd", typeof(string), "");
        //    rsstk.Columns.Add("slnm", typeof(string), "");
        //    rsstk.Columns.Add("docno", typeof(string), "");
        //    rsstk.Columns.Add("doccd", typeof(string), "");
        //    rsstk.Columns.Add("doconlyno", typeof(string), "");
        //    rsstk.Columns.Add("docdt", typeof(string), "");
        //    rsstk.Columns.Add("dia", typeof(double), "");
        //    rsstk.Columns.Add("ll", typeof(double), "");
        //    rsstk.Columns.Add("orgbatchno", typeof(string), "");
        //    rsstk.Columns.Add("orgbatchslno", typeof(double), "");
        //    rsstk.Columns.Add("batchautono", typeof(string), "");
        //    rsstk.Columns.Add("batchslno", typeof(double), "");
        //    rsstk.Columns.Add("batchno", typeof(string), "");
        //    rsstk.Columns.Add("orgbatchdocno", typeof(string), "");
        //    rsstk.Columns.Add("orgbatchdocdt", typeof(string), "");
        //    rsstk.Columns.Add("orgbatchdoccd", typeof(string), "");
        //    rsstk.Columns.Add("texture", typeof(string), "");
        //    rsstk.Columns.Add("gsm", typeof(string), "");
        //    rsstk.Columns.Add("mchnname", typeof(string), "");
        //    rsstk.Columns.Add("fabtype", typeof(string), "");
        //    rsstk.Columns.Add("colrnm", typeof(string), "");
        //    rsstk.Columns.Add("millnm", typeof(string), "");
        //    rsstk.Columns.Add("sizecdgrp", typeof(string), "");
        //    rsstk.Columns.Add("sizenm", typeof(string), "");
        //    rsstk.Columns.Add("print_seq", typeof(string), "");
        //    rsstk.Columns.Add("nos", typeof(double), "");
        //    rsstk.Columns.Add("qnty", typeof(double), "");
        //    #endregion

        //    sql = "";


        //    sql += "select a.gocd,m.gonm, (case when a.mtrljobcd is null and e.itgrptype = 'C' then 'PL' when a.mtrljobcd is null and e.itgrptype <> 'C' then 'FS' else a.mtrljobcd end) mtrljobcd, a.stktype, a.barno, a.itcd, a.partcd, a.colrcd, a.sizecd, a.shade, a.cutlength, a.dia, ";
        //    sql += "c.slcd, g.slnm, h.docdt, h.docno, b.prccd, b.effdt, b.rate, e.bargentype, ";
        //    sql += "d.itnm,nvl(d.negstock,e.negstock)negstock, d.styleno, d.styleno||' '||d.itnm itstyle,c.fabitcd, n.itnm fabitnm, d.itgrpcd, e.itgrpnm,e.salglcd,e.purglcd,e.salretglcd,e.purretglcd, f.colrnm,f.clrbarcode, d.prodgrpcd, z.prodgrpgstper, y.barimagecount, y.barimage, ";
        //    sql += "(case e.bargentype when 'E' then nvl(c.hsncode,nvl(d.hsncode,e.hsncode)) else nvl(d.hsncode,e.hsncode) end) hsncode, ";
        //    sql += "i.mtrljobnm,i.mtbarcode, d.uomcd, k.stkname, j.partnm,j.prtbarcode, c.pdesign, c.flagmtr, c.dia, c.locabin,balqnty, balnos,l.sizenm,l.szbarcode, e.wppricegen, e.rppricegen ";
        //    sql += "from ";

        //    sql += "( ";
        //    sql += "select b.gocd, b.mtrljobcd, nvl(b.stktype,'F') stktype, a.barno, a.itcd, b.partcd, a.colrcd, a.sizecd, a.shade, a.cutlength, a.dia, b.balqnty, b.balnos from ";

        //    sql += "( select a.barno, a.itcd, a.colrcd, a.sizecd, a.shade, a.cutlength, a.dia from ";
        //    sql += scm + ".t_batchmst a ";
        //    sql += "where a.barno is not null and ";
        //    if (barno.retStr() != "") sql += "upper(a.barno) in (" + barno + ") and ";
        //    if (itcd.retStr() != "") sql += "a.itcd in (" + itcd + ") and ";
        //    sql += "a.barno is not null ";
        //    sql += ") a, ";

        //    sql += "( select gocd, mtrljobcd, stktype, barno, partcd, sum(balqnty) balqnty, sum(balnos) balnos from ";
        //    sql += "( ";
        //    sql += "select a.gocd, a.mtrljobcd, a.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia, ";
        //    sql += "sum(case a.stkdrcr when 'D' then a.qnty when 'C' then a.qnty*-1 end) balqnty, ";
        //    sql += "sum(case a.stkdrcr when 'D' then a.nos when 'C' then a.nos*-1 end) balnos ";
        //    sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_cntrl_hdr c ";
        //    sql += "where a.barno=b.barno(+) and a.autono=c.autono(+) and ";
        //    sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and a.stkdrcr in ('D','C') and ";
        //    if (gocd.retStr() != "") sql += "a.gocd in (" + gocd + ") and ";
        //    if (barno.retStr() != "") sql += "upper(a.barno) in (" + barno + ") and ";
        //    if (itcd.retStr() != "") sql += "b.itcd in (" + itcd + ") and ";
        //    if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
        //    if (mtrljobcd.retStr() != "") sql += "a.mtrljobcd in (" + mtrljobcd + ") and ";
        //    sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
        //    sql += "group by a.gocd, a.mtrljobcd, a.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia ";
        //    if (pendpslipconsider == true)
        //    {
        //        sql += "union all ";
        //        sql += "select a.gocd, a.mtrljobcd, a.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia, ";
        //        sql += "sum(a.qnty*-1) balqnty, sum(a.nos*-1) balnos ";
        //        sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_cntrl_hdr c, ";
        //        sql += "" + scm + ".m_doctype d, " + scm + ".t_txn_linkno e ";
        //        sql += "where a.barno=b.barno(+) and a.autono=c.autono(+) and ";
        //        sql += "c.doccd=d.doccd(+) and d.doctype in ('SPSLP') and a.autono=e.linkautono(+) and e.autono is null and ";
        //        sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and a.stkdrcr in ('D','C') and ";
        //        if (gocd.retStr() != "") sql += "a.gocd in (" + gocd + ") and ";
        //        if (barno.retStr() != "") sql += "upper(a.barno) in (" + barno + ") and ";
        //        if (itcd.retStr() != "") sql += "b.itcd in (" + itcd + ") and ";
        //        if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
        //        if (mtrljobcd.retStr() != "") sql += "a.mtrljobcd in (" + mtrljobcd + ") and ";
        //        sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
        //        sql += "group by a.gocd, a.mtrljobcd, a.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia ";
        //    }
        //    sql += ") group by gocd, mtrljobcd, stktype, barno, partcd ";
        //    sql += " ) b ";
        //    sql += "where a.barno=b.barno(+) ";
        //    if (shownilstock == false) sql += "and nvl(b.balqnty,0) <> 0 ";
        //    sql += ") a, ";

        //    sql += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from ";
        //    sql += "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from ";
        //    sql += "(select a.barno, a.prccd, a.effdt, ";
        //    sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn ";
        //    sql += "from " + scm + ".t_batchmst_price a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + tdt + "','dd/mm/yyyy') ) ";
        //    sql += "a, " + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c ";
        //    sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and a.barno=c.barno(+) ";
        //    sql += ") a where prccd='" + prccd + "') b, ";

        //    sql += "(select a.barno, count(*) barimagecount,";
        //    sql += "listagg(a.doc_flname||'~'||a.doc_desc,chr(179)) ";
        //    //sql += "listagg(a.imgbarno||chr(181)||a.imgslno||chr(181)||a.doc_flname||chr(181)||a.doc_extn||chr(181)||substr(a.doc_desc,50),chr(179)) ";
        //    sql += "within group (order by a.barno) as barimage from ";
        //    sql += "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
        //    sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
        //    sql += "from " + scm + ".m_batch_img_hdr a ";
        //    sql += "union ";
        //    sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
        //    sql += "from " + scm + ".m_batch_img_hdr_link a, " + scm + ".m_batch_img_hdr b ";
        //    sql += "where a.mainbarno=b.barno(+) ) a, ";
        //    sql += "" + scm + ".m_batch_img_hdr b ";
        //    sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ";
        //    sql += "union ";
        //    sql += "select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
        //    sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
        //    sql += "from " + scm + ".t_batch_img_hdr a ";
        //    sql += "union ";
        //    sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
        //    sql += "from " + scm + ".t_batch_img_hdr_link a, " + scm + ".t_batch_img_hdr b ";
        //    sql += "where a.mainbarno=b.barno(+) ) a, ";
        //    sql += "" + scm + ".t_batch_img_hdr b ";
        //    sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ) a ";
        //    sql += "group by a.barno ) y, ";

        //    sql += "(select a.prodgrpcd, ";
        //    //sql += "listagg(b.fromrt||chr(181)||b.tort||chr(181)||b.igstper||chr(181)||b.cgstper||chr(181)||b.sgstper,chr(179)) ";
        //    sql += "listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179)) ";
        //    sql += "within group (order by a.prodgrpcd) as prodgrpgstper ";
        //    sql += "from ";
        //    sql += "(select prodgrpcd, effdt from ";
        //    sql += "(select a.prodgrpcd, a.effdt, ";
        //    sql += "row_number() over (partition by a.prodgrpcd order by a.effdt desc) as rn ";
        //    sql += "from " + scm + ".m_prodtax a where a.effdt <= to_date('" + tdt + "','dd/mm/yyyy') ) ";
        //    sql += "where rn=1 ) a, " + scm + ".m_prodtax b ";
        //    sql += "where a.prodgrpcd=b.prodgrpcd(+) and a.effdt=b.effdt(+) and b.taxgrpcd='" + taxgrpcd + "' ";
        //    sql += "group by a.prodgrpcd ) z, ";

        //    sql += "" + scm + ".t_batchmst c, " + scm + ".m_sitem d, " + scm + ".m_group e, " + scm + ".m_color f, ";
        //    sql += "" + scmf + ".m_subleg g, " + scm + ".t_cntrl_hdr h, ";
        //    sql += scm + ".m_mtrljobmst i, " + scm + ".m_parts j, " + scm + ".m_stktype k, " + scm + ".m_size l," + scmf + ".m_godown m, "+ scm + ".m_sitem n ";
        //    sql += "where a.barno=c.barno(+) and a.barno=b.barno(+) and d.prodgrpcd=z.prodgrpcd(+) and a.barno=y.barno(+) and ";
        //    sql += "a.itcd=d.itcd(+) and d.itgrpcd=e.itgrpcd(+) and c.fabitcd=n.itcd(+) and ";
        //    if (stylelike.retStr() != "")
        //    {
        //        if (exactbarno == true)
        //        {
        //            sql += " (a.barno=" + stylelike + ") and ";
        //        }
        //        else
        //        {
        //            sql += " (d.styleno like '%" + stylelike.Replace("'", "") + "%') and ";
        //        }
        //    }
        //    //if (stylelike.retStr() != "") sql += " (a.barno=" + stylelike + " or d.styleno like '%" + stylelike.Replace("'", "") + "%') and ";
        //    //if (stylelike.retStr() != "") sql += "d.styleno like '%" + stylelike + "%' and ";
        //    if (itgrpcd.retStr() != "") sql += "d.itgrpcd in (" + itgrpcd + ") and ";
        //    if (brandcd.retStr() != "") sql += "d.brandcd in (" + brandcd + ") and ";
        //    sql += "a.colrcd=f.colrcd(+) and c.autono=h.autono(+) and c.slcd=g.slcd(+) and ";
        //    sql += "a.mtrljobcd=i.mtrljobcd(+) and a.partcd=j.partcd(+) and a.stktype=k.stktype(+)and a.sizecd=l.sizecd(+) and a.gocd=m.gocd(+)  ";
        //    if (partcd.retStr() != "") sql += "and a.partcd='" + partcd + "'  ";
        //    tbl = masterHelpFa.SQLquery(sql);
        //    return tbl;

        //}
        public DataTable GetStock(string tdt, string gocd = "", string barno = "", string itcd = "", string mtrljobcd = "'FS'", string skipautono = "", string itgrpcd = "", string stylelike = "", string prccd = "WP", string taxgrpcd = "C001", string stktype = "", string brandcd = "", bool pendpslipconsider = true, bool shownilstock = false, string curschema = "", string finschema = "", bool mergeitem = false, bool mergeloca = false, bool exactbarno = true, string partcd = "", bool showallitems = false, string doctag = "", string SLCD = "")
        {
            //showbatchno = true;
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string sql = "";
            //bool showallitems = true;

            if (curschema != "") scm = curschema;
            if (finschema != "") scmf = finschema;
            string itcdqry = "a.itcd ", itcdpipe = "a.itcd";
            if (mergeitem == true)
            {
                itcdqry = "nvl(e.linkitcd,a.itcd) ";
                itcdpipe = "nvl(e.linkitcd,a.itcd)";
            }
            if (barno.retStr() != "")
            {
                barno = barno.ToUpper();
            }
            #region Generate Stock Table
            DataTable rsstk = new DataTable();
            rsstk.Columns.Add("key", typeof(string), "");
            rsstk.Columns.Add("itcd", typeof(string), "");
            rsstk.Columns.Add("partcd", typeof(string), "");
            rsstk.Columns.Add("partnm", typeof(string), "");
            rsstk.Columns.Add("mtrljobcd", typeof(string), "");
            rsstk.Columns.Add("jobnm", typeof(string), "");
            rsstk.Columns.Add("jobseq", typeof(string), "");
            rsstk.Columns.Add("stktype", typeof(string), "");
            rsstk.Columns.Add("sizecd", typeof(string), "");
            rsstk.Columns.Add("colrcd", typeof(string), "");
            rsstk.Columns.Add("itcolsize", typeof(string), "");
            rsstk.Columns.Add("rate", typeof(double), "");
            rsstk.Columns.Add("itnm", typeof(string), "");
            rsstk.Columns.Add("styleno", typeof(string), "");
            rsstk.Columns.Add("mixsize", typeof(string), "");
            rsstk.Columns.Add("uomcd", typeof(string), "");
            rsstk.Columns.Add("uomnm", typeof(string), "");
            rsstk.Columns.Add("pcsperbox", typeof(double), "");
            rsstk.Columns.Add("decimals", typeof(double), "");
            rsstk.Columns.Add("pcsperset", typeof(double), "");
            rsstk.Columns.Add("mergepcs", typeof(double), "");
            rsstk.Columns.Add("itgrpcd", typeof(string), "");
            rsstk.Columns.Add("itgrpnm", typeof(string), "");
            rsstk.Columns.Add("brandcd", typeof(string), "");
            rsstk.Columns.Add("brandnm", typeof(string), "");
            rsstk.Columns.Add("slcd", typeof(string), "");
            rsstk.Columns.Add("slnm", typeof(string), "");
            rsstk.Columns.Add("docno", typeof(string), "");
            rsstk.Columns.Add("doccd", typeof(string), "");
            rsstk.Columns.Add("doconlyno", typeof(string), "");
            rsstk.Columns.Add("docdt", typeof(string), "");
            rsstk.Columns.Add("dia", typeof(double), "");
            rsstk.Columns.Add("ll", typeof(double), "");
            rsstk.Columns.Add("orgbatchno", typeof(string), "");
            rsstk.Columns.Add("orgbatchslno", typeof(double), "");
            rsstk.Columns.Add("batchautono", typeof(string), "");
            rsstk.Columns.Add("batchslno", typeof(double), "");
            rsstk.Columns.Add("batchno", typeof(string), "");
            rsstk.Columns.Add("orgbatchdocno", typeof(string), "");
            rsstk.Columns.Add("orgbatchdocdt", typeof(string), "");
            rsstk.Columns.Add("orgbatchdoccd", typeof(string), "");
            rsstk.Columns.Add("texture", typeof(string), "");
            rsstk.Columns.Add("gsm", typeof(string), "");
            rsstk.Columns.Add("mchnname", typeof(string), "");
            rsstk.Columns.Add("fabtype", typeof(string), "");
            rsstk.Columns.Add("colrnm", typeof(string), "");
            rsstk.Columns.Add("millnm", typeof(string), "");
            rsstk.Columns.Add("sizecdgrp", typeof(string), "");
            rsstk.Columns.Add("sizenm", typeof(string), "");
            rsstk.Columns.Add("print_seq", typeof(string), "");
            rsstk.Columns.Add("nos", typeof(double), "");
            rsstk.Columns.Add("qnty", typeof(double), "");
            #endregion

            sql = "";


            sql += "select a.gocd,m.gonm, (case when a.mtrljobcd is null and e.itgrptype = 'C' then 'PL' when a.mtrljobcd is null and e.itgrptype <> 'C' then 'FS' else a.mtrljobcd end) mtrljobcd, a.stktype, a.barno, a.itcd, a.partcd, a.colrcd, a.sizecd, a.shade, a.cutlength, a.dia, " + Environment.NewLine;
            sql += "c.slcd, g.slnm, h.docdt, h.docno,h.autono,p.doctag, b.prccd, b.effdt, b.rate,q.rate rprate, e.bargentype, " + Environment.NewLine;
            sql += "d.itnm,d.convqtypunit,d.convuomcd,nvl(d.negstock,e.negstock)negstock, d.styleno, d.styleno||' '||d.itnm itstyle,c.fabitcd, n.itnm fabitnm, d.itgrpcd, e.itgrpnm,e.salglcd,e.purglcd,e.salretglcd,e.purretglcd, f.colrnm,f.clrbarcode, d.prodgrpcd, z.prodgrpgstper, y.barimagecount, y.barimage, " + Environment.NewLine;
            sql += "(case nvl(c.commonuniqbar,e.bargentype) when 'E' then nvl(c.hsncode,nvl(d.hsncode,e.hsncode)) else nvl(d.hsncode,e.hsncode) end) hsncode, " + Environment.NewLine;
            sql += "i.mtrljobnm,i.mtbarcode, d.uomcd, k.stkname, j.partnm,j.prtbarcode, c.pdesign, c.flagmtr, c.dia, c.locabin,balqnty, balnos,l.sizenm,l.szbarcode, e.wppricegen, e.rppricegen,x.scmdiscrate,x.scmdisctype,c.commonuniqbar " + Environment.NewLine;
            sql += ",p.conslcd,p.prefno,p.prefdt,c.slno " + Environment.NewLine;
            sql += "from " + Environment.NewLine;

            sql += "( " + Environment.NewLine;
            sql += "select b.gocd, b.mtrljobcd, nvl(b.stktype,'F') stktype, a.barno, a.itcd, b.partcd, a.colrcd, a.sizecd, a.shade, a.cutlength, a.dia, b.balqnty, b.balnos from " + Environment.NewLine;

            sql += "( select a.barno, a.itcd, a.colrcd, a.sizecd, a.shade, a.cutlength, a.dia from " + Environment.NewLine;
            sql += scm + ".t_batchmst a " + Environment.NewLine;
            sql += "where a.barno is not null and " + Environment.NewLine;
            if (barno.retStr() != "") sql += "upper(a.barno) in (" + barno + ") and " + Environment.NewLine;
            if (itcd.retStr() != "") sql += "a.itcd in (" + itcd + ") and " + Environment.NewLine;
            sql += "a.barno is not null " + Environment.NewLine;
            sql += ") a, " + Environment.NewLine;

            sql += "( select gocd, mtrljobcd, stktype, barno, partcd, sum(balqnty) balqnty, sum(balnos) balnos from " + Environment.NewLine;
            sql += "( " + Environment.NewLine;
            sql += "select a.gocd, a.mtrljobcd, a.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia, " + Environment.NewLine;
            sql += "sum(case a.stkdrcr when 'D' then a.qnty when 'C' then a.qnty*-1 end) balqnty, " + Environment.NewLine;
            sql += "sum(case a.stkdrcr when 'D' then a.nos when 'C' then a.nos*-1 end) balnos " + Environment.NewLine;
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_cntrl_hdr c " + Environment.NewLine;
            sql += "where a.barno=b.barno(+) and a.autono=c.autono(+) and " + Environment.NewLine;
            sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and a.stkdrcr in ('D','C') and " + Environment.NewLine;
            if (gocd.retStr() != "") sql += "a.gocd in (" + gocd + ") and " + Environment.NewLine;
            if (barno.retStr() != "") sql += "upper(a.barno) in (" + barno + ") and " + Environment.NewLine;
            if (itcd.retStr() != "") sql += "b.itcd in (" + itcd + ") and " + Environment.NewLine;
            if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and " + Environment.NewLine;
            if (mtrljobcd.retStr() != "") sql += "a.mtrljobcd in (" + mtrljobcd + ") and " + Environment.NewLine;
            sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') " + Environment.NewLine;
            sql += "and a.baleno||a.baleyr not in (select a.baleno||a.baleyr from " + scm + ".t_bale a) ";
            sql += "group by a.gocd, a.mtrljobcd, a.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia " + Environment.NewLine;
            if (pendpslipconsider == true)
            {
                sql += "union all " + Environment.NewLine;
                sql += "select a.gocd, a.mtrljobcd, a.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia, " + Environment.NewLine;
                sql += "sum(a.qnty*-1) balqnty, sum(a.nos*-1) balnos " + Environment.NewLine;
                sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_cntrl_hdr c, " + Environment.NewLine;
                sql += "" + scm + ".m_doctype d, " + scm + ".t_txn_linkno e " + Environment.NewLine;
                sql += "where a.barno=b.barno(+) and a.autono=c.autono(+) and " + Environment.NewLine;
                sql += "c.doccd=d.doccd(+) and d.doctype in ('SPSLP') and a.autono=e.linkautono(+) and e.autono is null and " + Environment.NewLine;
                sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and a.stkdrcr in ('D','C') and " + Environment.NewLine;
                if (gocd.retStr() != "") sql += "a.gocd in (" + gocd + ") and " + Environment.NewLine;
                if (barno.retStr() != "") sql += "upper(a.barno) in (" + barno + ") and ";
                if (itcd.retStr() != "") sql += "b.itcd in (" + itcd + ") and " + Environment.NewLine;
                if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and " + Environment.NewLine;
                if (mtrljobcd.retStr() != "") sql += "a.mtrljobcd in (" + mtrljobcd + ") and " + Environment.NewLine;
                sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') " + Environment.NewLine;
                sql += "and a.baleno||a.baleyr not in (select a.baleno||a.baleyr from " + scm + ".t_bale a) ";
                sql += "group by a.gocd, a.mtrljobcd, a.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia " + Environment.NewLine;
            }
            sql += ") group by gocd, mtrljobcd, stktype, barno, partcd " + Environment.NewLine;
            sql += " ) b " + Environment.NewLine;
            sql += "where a.barno=b.barno(+) " + Environment.NewLine;
            if (shownilstock == false) sql += "and nvl(b.balqnty,0) <> 0 " + Environment.NewLine;
            sql += ") a, " + Environment.NewLine;
            for (int x = 0; x <= 1; x++)
            {
                string sqlals = "";
                switch (x)
                {
                    case 0:
                       sqlals = "b"; break;
                    case 1:
                        prccd = "RP"; sqlals = "q"; break;
                   
                }
                sql += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from " + Environment.NewLine;
                sql += "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from " + Environment.NewLine;
                sql += "(select a.barno, a.prccd, a.effdt, " + Environment.NewLine;
                sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn " + Environment.NewLine;
                sql += "from " + scm + ".t_batchmst_price a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + tdt + "','dd/mm/yyyy') ) " + Environment.NewLine;
                sql += "a, " + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c " + Environment.NewLine;
                sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and a.barno=c.barno(+) " + Environment.NewLine;
                sql += ") a where prccd='" + prccd + "' " + Environment.NewLine;
                sql += ") " + sqlals;
                if (x != 2) sql += ", ";
            }
            sql += "(select a.barno, count(*) barimagecount," + Environment.NewLine;
            sql += "listagg(a.doc_flname||'~'||a.doc_desc,chr(179)) " + Environment.NewLine;
            sql += "within group (order by a.barno) as barimage from " + Environment.NewLine;
            sql += "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from " + Environment.NewLine;
            sql += "(select a.barno, a.barno imgbarno, a.slno imgslno " + Environment.NewLine;
            sql += "from " + scm + ".t_batch_img_hdr a " + Environment.NewLine;
            sql += "union " + Environment.NewLine;
            sql += "select a.barno, b.barno imgbarno, b.slno imgslno " + Environment.NewLine;
            sql += "from " + scm + ".t_batch_img_hdr_link a, " + scm + ".t_batch_img_hdr b " + Environment.NewLine;
            sql += "where a.mainbarno=b.barno(+) ) a, " + Environment.NewLine;
            sql += "" + scm + ".t_batch_img_hdr b " + Environment.NewLine;
            sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ) a " + Environment.NewLine;
            sql += "group by a.barno ) y, " + Environment.NewLine;

            sql += "(select a.prodgrpcd, " + Environment.NewLine;
            //sql += "listagg(b.fromrt||chr(181)||b.tort||chr(181)||b.igstper||chr(181)||b.cgstper||chr(181)||b.sgstper,chr(179)) ";
            sql += "listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179)) " + Environment.NewLine;
            sql += "within group (order by a.prodgrpcd) as prodgrpgstper " + Environment.NewLine;
            sql += "from " + Environment.NewLine;
            sql += "(select prodgrpcd, effdt from " + Environment.NewLine;
            sql += "(select a.prodgrpcd, a.effdt, " + Environment.NewLine;
            sql += "row_number() over (partition by a.prodgrpcd order by a.effdt desc) as rn " + Environment.NewLine;
            sql += "from " + scm + ".m_prodtax a where a.effdt <= to_date('" + tdt + "','dd/mm/yyyy') ) " + Environment.NewLine;
            sql += "where rn=1 ) a, " + scm + ".m_prodtax b " + Environment.NewLine;
            sql += "where a.prodgrpcd=b.prodgrpcd(+) and a.effdt=b.effdt(+) and b.taxgrpcd='" + taxgrpcd + "' " + Environment.NewLine;
            sql += "group by a.prodgrpcd ) z, " + Environment.NewLine;


            sql += "( select barno, scmdiscrate, scmdisctype from ( " + Environment.NewLine;
            sql += "select distinct a.barno, b.docdt, a.scmdiscrate, a.scmdisctype, " + Environment.NewLine;
            sql += "row_number() over(partition by a.barno order by b.docdt) as rn " + Environment.NewLine;
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_txn b " + Environment.NewLine;
            sql += "where a.autono = b.autono and b.doctag = '" + doctag + "') where rn = 1 ) x, " + Environment.NewLine;


            sql += "" + scm + ".t_batchmst c, " + scm + ".m_sitem d, " + scm + ".m_group e, " + scm + ".m_color f, " + Environment.NewLine;
            sql += "" + scmf + ".m_subleg g, " + scm + ".t_cntrl_hdr h, " + Environment.NewLine;
            sql += scm + ".m_mtrljobmst i, " + scm + ".m_parts j, " + scm + ".m_stktype k, " + scm + ".m_size l," + scmf + ".m_godown m, " + scm + ".m_sitem n, " + scm + ".m_cntrl_hdr o, " + scm + ".t_txn p " + Environment.NewLine;
            sql += "where a.barno=c.barno(+) and a.barno=b.barno(+) and a.barno=q.barno(+) and d.prodgrpcd=z.prodgrpcd(+) and a.barno=y.barno(+) and " + Environment.NewLine;
            sql += "a.itcd=d.itcd(+) and d.itgrpcd=e.itgrpcd(+) and d.fabitcd=n.itcd(+) and " + Environment.NewLine; //c.fabitcd=n.itcd(+) 
            sql += "a.barno=x.barno(+) and " + Environment.NewLine;
            if (stylelike.retStr() != "")
            {
                if (exactbarno == true)
                {
                    sql += " (upper(a.barno)=" + stylelike.ToUpper() + ") and " + Environment.NewLine;
                }
                else
                {
                    sql += " (d.styleno like '%" + stylelike.Replace("'", "") + "%') and " + Environment.NewLine;
                }
            }
            //if (stylelike.retStr() != "") sql += " (a.barno=" + stylelike + " or d.styleno like '%" + stylelike.Replace("'", "") + "%') and ";
            //if (stylelike.retStr() != "") sql += "d.styleno like '%" + stylelike + "%' and ";
            if (itgrpcd.retStr() != "") sql += "d.itgrpcd in (" + itgrpcd + ") and " + Environment.NewLine;
            if (brandcd.retStr() != "") sql += "d.brandcd in (" + brandcd + ") and " + Environment.NewLine;
            if (SLCD.retStr() != "") sql += "c.slcd in (" + SLCD + ") and " + Environment.NewLine;
            sql += "a.colrcd=f.colrcd(+) and c.autono=h.autono(+) and c.slcd=g.slcd(+) and " + Environment.NewLine;
            sql += "a.mtrljobcd=i.mtrljobcd(+) and a.partcd=j.partcd(+) and a.stktype=k.stktype(+)and a.sizecd=l.sizecd(+) and a.gocd=m.gocd(+) and h.autono=p.autono(+) " + Environment.NewLine;
            if (partcd.retStr() != "") sql += "and a.partcd='" + partcd + "'  " + Environment.NewLine;
            sql += "and d.m_autono=o.m_autono(+) and nvl(o.inactive_tag,'N')='N' " + Environment.NewLine;
            tbl = masterHelpFa.SQLquery(sql);
            return tbl;

        }
        public DataTable GetTax(string docdt, string taxgrpcd, string prodgrpcd = "", double rate = 0)
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string sql = "";
            sql += "select a.prodgrpcd, a.effdt, b.fromrt, b.tort, ";
            sql += "b.igstper, b.cgstper, b.sgstper, ";
            sql += "nvl(b.igstper, 0) + nvl(b.cgstper, 0) + nvl(b.sgstper, 0) gstper from ";
            sql += "(select prodgrpcd, effdt from ";
            sql += "(select a.prodgrpcd, a.effdt, ";
            sql += "row_number() over(partition by a.prodgrpcd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".m_prodtax a where a.effdt <= to_date('" + docdt + "', 'dd/mm/yyyy') ) ";
            sql += "where rn = 1 ) a, " + scm + ".m_prodtax b ";
            sql += "where a.prodgrpcd = b.prodgrpcd(+) and a.effdt = b.effdt(+) and b.taxgrpcd = '" + taxgrpcd + "' ";
            sql += "and 1000 between b.fromrt and b.tort ";
            tbl = SQLquery(sql);
            return tbl;
        }
        public string retGstPer(string prodgrpgstper, double rate = 0, string disctype = "", double discrate = 0)
        {
            //Searchstr value like listagg(b.fromrt||chr(181)||b.tort||chr(181)||b.igstper||chr(181)||b.cgstper||chr(181)||b.sgstper,chr(179))
            //Searchstr value like listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179))
            if (discrate.retDbl() != 0 && disctype.retStr() != "")
            {//less discount from rate
                if (disctype == "P")
                {
                    rate = (rate.retDbl() - ((rate.retDbl() * discrate.retDbl()) / 100).retDbl()).retDbl().toRound(3);
                }
                else {
                    rate = (rate.retDbl() - discrate.retDbl()).retDbl().toRound(3);
                }
            }
            double fromrt = 0, tort = 0; int selrow = -1;
            string[] mgstrate = new string[5];
            string rtval = "0,0,0"; //igstper,cgst,sgst
            char SP = ((char)179);

            string[] mrates = prodgrpgstper.Split(Convert.ToChar(SP)).ToArray();
            for (int x = 0; x <= mrates.Count() - 1; x++)
            {
                //mgstrate = mrates[x].Split(Convert.ToChar(Cn.GCS())).ToArray();
                mgstrate = mrates[x].Split('~').ToArray();
                fromrt = mgstrate[0].retDbl(); tort = mgstrate[1].retDbl();
                if (rate >= fromrt && rate <= tort) { selrow = x; break; }
            }
            if (selrow != -1) rtval = mgstrate[2].retDbl().retStr() + "," + mgstrate[3].retDbl().retStr() + "," + mgstrate[4].retDbl().retStr();
            return rtval;
        }

        public string retVchrUniqId(string doccd, string autono = "")
        {
            string rtval = "";
            string sql = "", scm = CommVar.CurSchema(UNQSNO);
            string dcdbarcode = "", unqno = "";
            sql = "select a.docbarcode from " + scm + ".m_doctype_bar a where a.doccd='" + doccd + "' and docbarcode is not null";
            DataTable tbl = SQLquery(sql);
            if (tbl.Rows.Count == 1) dcdbarcode = tbl.Rows[0]["docbarcode"].retStr();

            if (autono != "")
            {
                sql = "select  max(a.uniqno) uniqno ";
                sql += "from " + scm + ".t_cntrl_hdr_uniqno a, " + scm + ".t_cntrl_hdr b ";
                sql += "where a.autono = b.autono(+) and a.autono='" + autono + "' ";
                tbl = SQLquery(sql);
                if (tbl.Rows.Count == 1) unqno = tbl.Rows[0]["uniqno"].retStr();
            }
            bool getuniqid = true;
            if (autono != "" && unqno == "") getuniqid = true; else { getuniqid = false; }
            if (dcdbarcode == "") getuniqid = false;
            if (getuniqid == true)
            {
                sql = "select b.doccd, max(a.uniqno) uniqno ";
                sql += "from " + scm + ".t_cntrl_hdr_uniqno a, " + scm + ".t_cntrl_hdr b ";
                sql += "where a.autono = b.autono(+) and ";
                sql += "a.uniqno like '" + dcdbarcode + "%' ";
                sql += "group by b.doccd ";
                tbl = SQLquery(sql);
                if (tbl.Rows.Count == 1) unqno = tbl.Rows[0]["uniqno"].retStr();
                double newno = 1;
                if (unqno.retStr() != "")
                {
                    string aftval = unqno.Substring(dcdbarcode.Length);
                    newno = Convert.ToDouble(aftval) + 1;
                }
                rtval = dcdbarcode + newno.ToString().PadLeft(5, '0');
            }
            else
            {
                rtval = unqno;
            }
            return rtval;
        }

        public DataTable GetBarHelp(string tdt, string gocd = "", string barno = "", string itcd = "", string mtrljobcd = "'FS'", string skipautono = "", string itgrpcd = "", string stylelike = "", string prccd = "WP", string taxgrpcd = "C001", string stktype = "", string brandcd = "", bool pendpslipconsider = true, bool shownilstock = false, string menupara = "", string curschema = "", string finschema = "", bool mergeitem = false, bool mergeloca = false, bool exactbarno = true, string partcd = "", bool showonlycommonbar = true)
        {
            //showbatchno = true;
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string sql = "";
            if (curschema != "") scm = curschema;
            if (finschema != "") scmf = finschema;
            string itcdqry = "a.itcd ", itcdpipe = "a.itcd";
            if (mergeitem == true)
            {
                itcdqry = "nvl(e.linkitcd,a.itcd) ";
                itcdpipe = "nvl(e.linkitcd,a.itcd)";
            }
            if (barno.retStr() != "")
            {
                barno = barno.ToUpper();
            }

            sql = "";

            sql += "select a.gocd, a.mtrljobcd, a.stktype, a.barno, a.itcd, a.partcd, a.colrcd, a.sizecd, a.shade, a.cutlength, a.dia, ";
            sql += "c.slcd, g.slnm, h.docdt, h.docno, b.prccd, b.effdt, b.rate, e.bargentype, d.styleno||' '||d.itnm itstyle,c.fabitcd, n.itnm fabitnm, ";
            sql += "d.itnm,d.convqtypunit,d.convuomcd,nvl(d.negstock,e.negstock)negstock, d.styleno, d.itgrpcd, e.itgrpnm,e.salglcd,e.purglcd,e.salretglcd,e.purretglcd, f.colrnm,d.prodgrpcd, z.prodgrpgstper, y.barimagecount, y.barimage, ";
            sql += "(case nvl(c.commonuniqbar,e.bargentype) when 'E' then nvl(c.hsncode,nvl(d.hsncode,e.hsncode)) else nvl(d.hsncode,e.hsncode) end) hsncode, ";
            sql += "i.mtrljobnm, d.uomcd, k.stkname, j.partnm, c.pdesign, c.flagmtr, c.dia, c.locabin,balqnty, balnos,i.mtbarcode,j.prtbarcode,f.clrbarcode,l.szbarcode,l.sizenm, e.wppricegen, e.rppricegen, m.decimals,c.commonuniqbar,e.wpper,e.rpper ";
            sql += "from ";
            sql += "( ";
            if (menupara != "PB" || barno != "")
            {
                sql += "select distinct '' gocd, '' mtrljobcd, '' stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, '' shade, 0 cutlength, 0 dia, 0 balqnty, 0 balnos ";
                sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_cntrl_hdr c ";
                sql += "where a.barno=b.barno(+) and a.autono=c.autono(+) and ";
                sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and a.stkdrcr in ('D','C') and ";
                if (barno.retStr() != "") sql += "upper(a.barno) in (" + barno + ") and ";
                if (itcd.retStr() != "") sql += "b.itcd in (" + itcd + ") and ";
                if (mtrljobcd.retStr() != "") sql += "a.mtrljobcd in (" + mtrljobcd + ") and ";
                sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
                sql += "union ";
            }
            //sql += "select '' gocd, '' mtrljobcd, '' stktype, a.barno, b.itcd, '' partcd, a.colrcd, a.sizecd, '' shade, 0 cutlength, 0 dia, 0 balqnty, 0 balnos ";
            //sql += "from " + scm + ".T_BATCHmst a, " + scm + ".m_sitem b, " + scm + ".m_cntrl_hdr c, " + scm + ".m_cntrl_loca d ";
            //sql += "where a.itcd=b.itcd(+) and b.m_autono=c.m_autono(+) and b.m_autono=d.m_autono(+) and ";
            //sql += "nvl(a.inactive_tag,'N')='N' and nvl(c.inactive_tag,'N')='N' and ";
            //if (barno.retStr() != "") sql += "upper(a.barno) in (" + barno + ") and ";
            //if (itcd.retStr() != "") sql += "a.itcd in (" + itcd + ") and ";
            //sql += "(d.compcd = '" + COM + "' or d.compcd is null) and (d.loccd='" + LOC + "' or d.loccd is null) ";
            //sql += ") a, ";

            sql += "select '' gocd, '' mtrljobcd, '' stktype, a.barno, b.itcd, '' partcd, a.colrcd, a.sizecd, '' shade, 0 cutlength, 0 dia, 0 balqnty, 0 balnos ";
            sql += "from " + scm + ".t_batchmst a, " + scm + ".m_sitem b, " + scm + ".m_cntrl_hdr c, " + scm + ".m_cntrl_loca d ";
            sql += "where a.itcd=b.itcd(+) and b.m_autono=c.m_autono(+) and b.m_autono=d.m_autono(+) and ";
            sql += "nvl(c.inactive_tag,'N')='N' and ";
            if (barno.retStr() != "") sql += "upper(a.barno) in (" + barno + ") and ";
            if (itcd.retStr() != "") sql += "a.itcd in (" + itcd + ") and ";
            sql += "(d.compcd = '" + COM + "' or d.compcd is null) and (d.loccd='" + LOC + "' or d.loccd is null) ";
            sql += ") a, ";


            sql += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from ";
            sql += "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from ";
            sql += "(select a.barno, a.prccd, a.effdt, ";
            sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".t_batchmst_price a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + tdt + "','dd/mm/yyyy') ) ";
            sql += "a, " + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c ";
            sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and a.barno=c.barno(+) ";
            sql += ") a where prccd='" + prccd + "') b, ";

            sql += "(select a.barno, count(*) barimagecount, ";
            sql += "listagg(a.doc_flname||'~'||a.doc_desc,chr(179)) ";
            sql += "within group (order by a.barno) as barimage from ";
            sql += "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
            sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
            sql += "from " + scm + ".t_batch_img_hdr a ";
            sql += "union ";
            sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
            sql += "from " + scm + ".t_batch_img_hdr_link a, " + scm + ".t_batch_img_hdr b ";
            sql += "where a.mainbarno=b.barno(+) ) a, ";
            sql += "" + scm + ".t_batch_img_hdr b ";
            sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ) a ";
            sql += "group by a.barno ) y, ";

            sql += "(select a.prodgrpcd, ";
            sql += "listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179)) ";
            sql += "within group (order by a.prodgrpcd) as prodgrpgstper ";
            sql += "from ";
            sql += "(select prodgrpcd, effdt from ";
            sql += "(select a.prodgrpcd, a.effdt, ";
            sql += "row_number() over (partition by a.prodgrpcd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".m_prodtax a where a.effdt <= to_date('" + tdt + "','dd/mm/yyyy') ) ";
            sql += "where rn=1 ) a, " + scm + ".m_prodtax b ";
            sql += "where a.prodgrpcd=b.prodgrpcd(+) and a.effdt=b.effdt(+) and b.taxgrpcd='" + taxgrpcd + "' ";
            sql += "group by a.prodgrpcd ) z, ";

            sql += "" + scm + ".t_batchmst c, " + scm + ".m_sitem d, " + scm + ".m_group e, " + scm + ".m_color f, ";
            sql += "" + scmf + ".m_subleg g, " + scm + ".t_cntrl_hdr h, ";
            sql += scm + ".m_mtrljobmst i, " + scm + ".m_parts j, " + scm + ".m_stktype k , " + scm + ".m_size l, " + scmf + ".m_uom m, " + scm + ".m_sitem n ";
            sql += "where a.barno=c.barno(+) and a.barno=b.barno(+) and d.prodgrpcd=z.prodgrpcd(+) and a.barno=y.barno(+) and ";
            sql += "a.itcd=d.itcd(+) and d.itgrpcd=e.itgrpcd(+) and ";
            if (stylelike.retStr() != "")
            {
                if (exactbarno == true)
                {
                    sql += " (upper(a.barno) =" + stylelike.ToUpper() + ") and ";
                }
                else
                {
                    sql += " (d.styleno like '%" + stylelike.Replace("'", "") + "%' or d.styleno||' '||d.itnm like '%" + stylelike.Replace("'", "") + "%') and ";
                }
            }

            //if (stylelike.retStr() != "") sql += " (a.barno=" + stylelike + " or d.styleno like '%" + stylelike.Replace("'", "") + "%') and ";
            //if (stylelike.retStr() != "") sql += "  d.styleno like '%" + stylelike + "%' and ";
            if (itgrpcd.retStr() != "") sql += "d.itgrpcd in (" + itgrpcd + ") and ";
            if (brandcd.retStr() != "") sql += "d.brandcd in (" + brandcd + ") and ";
            if (partcd.retStr() != "") sql += "a.partcd='" + partcd + "' and ";
            sql += "a.colrcd=f.colrcd(+) and c.autono=h.autono(+) and c.slcd=g.slcd(+) and "; //c.fabitcd=n.itcd(+) and 
            sql += "a.mtrljobcd=i.mtrljobcd(+) and a.partcd=j.partcd(+) and a.stktype=k.stktype(+) and a.sizecd=l.sizecd(+)  and d.uomcd=m.uomcd(+) and d.fabitcd=n.itcd(+)  ";
            if (showonlycommonbar == true) sql += "and c.commonuniqbar <> 'E' ";
            tbl = masterHelpFa.SQLquery(sql);
            return tbl;
        }
        //public DataTable getPendBiltytoIssue(string docdt, string blautono = "", string skipautono = "", string schema = "", string translcd = "", string lrnoLike = "")
        //{
        //    //showbatchno = true;
        //    string UNQSNO = CommVar.getQueryStringUNQSNO();
        //    DataTable tbl = new DataTable();
        //    string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
        //    if (schema == "") schema = scm;
        //    string sql = "";

        //    sql = "";
        //    sql += "select distinct a.autono, a.baleno, a.baleyr, c.lrno, c.lrdt,	";
        //    sql += "d.prefno, d.prefdt, 1 - nvl(b.bnos, 0) bnos,c.TRANSLCD,e.slnm TRANSLNM,g.styleno,f.qnty,g.uomcd,f.pageno from ";

        //    sql += "(select distinct a.autono, b.baleno, b.baleyr, b.baleyr || b.baleno balenoyr ";
        //    sql += "from " + schema + ".t_txn a, " + schema + ".t_txndtl b, " + schema + ".t_cntrl_hdr d ";
        //    sql += "where a.autono = b.autono(+) and a.autono = d.autono(+) and ";
        //    sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and ";
        //    if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
        //    sql += "d.docdt <= to_date('" + docdt + "', 'dd/mm/yyyy') and ";
        //    // sql += "a.autono not in (select blautono from " + schema + ".t_bale) and ";
        //    sql += "a.doctag in ('PB','OP') and b.baleno is not null  and b.gocd='TR' ) a, ";

        //    sql += "(select a.blautono, a.baleno, a.baleyr, a.baleyr || a.baleno balenoyr, ";
        //    sql += "sum(case a.drcr when 'D' then 1 when 'C' then - 1 end) bnos ";
        //    sql += " from " + schema + ".t_bilty a, " + scm + ".t_bilty_hdr b, " + schema + ".t_cntrl_hdr d ";
        //    sql += "where a.autono = b.autono(+) and ";
        //    if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
        //    sql += "a.autono = d.autono(+) ";
        //    sql += "group by a.blautono, a.baleno, a.baleyr, a.baleyr || a.baleno) b, ";

        //    sql += "" + schema + ".t_txntrans c, " + schema + ".t_txn d ," + scmf + ".m_subleg e," + schema + ".t_txndtl f," + schema + ".m_sitem g ";
        //    sql += "where a.autono = b.blautono(+) and a.balenoyr = b.balenoyr(+) and c.TRANSLCD = e.slcd(+)and a.baleno=f.baleno(+) and f.itcd = g.itcd(+) and ";
        //    sql += "a.autono = c.autono(+) and a.autono = d.autono(+) and c.lrno is not null  ";
        //    if (blautono.retStr() != "") sql += " and a.autono in(" + blautono + ")  ";
        //    if (translcd.retStr() != "") sql += " and c.TRANSLCD in(" + translcd + ")  ";
        //    if (lrnoLike.retStr() != "") sql += "and c.lrno like '%" + lrnoLike.retStr() + "%'  ";
        //    sql += " and 1 - nvl(b.bnos, 0) > 0 ";
        //    tbl = masterHelpFa.SQLquery(sql);
        //    return tbl;
        //}
        public DataTable getPendBiltytoIssue(string docdt, string blautono = "", string skipautono = "", string schema = "", string translcd = "", string lrnoLike = "")
        {
            //showbatchno = true;
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (schema == "") schema = scm;
            string sql = "";

            sql = "";
            sql += "select distinct a.autono, a.baleno, a.baleyr, c.lrno, c.lrdt,	";
            sql += "d.prefno, d.prefdt, 1 - nvl(b.bnos, 0) bnos,c.TRANSLCD,e.slnm TRANSLNM,g.styleno,f.qnty,g.uomcd,f.pageno,f.pageslno from ";

            sql += "(select distinct a.autono, b.baleno, b.baleyr, b.baleyr || b.baleno balenoyr ";
            sql += "from " + schema + ".t_txn a, " + schema + ".t_txndtl b, " + schema + ".t_cntrl_hdr d ";
            sql += "where a.autono = b.autono(+) and a.autono = d.autono(+) and ";
            sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and ";
            if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
            sql += "d.docdt <= to_date('" + docdt + "', 'dd/mm/yyyy') and ";
            // sql += "a.autono not in (select blautono from " + schema + ".t_bale) and ";
            sql += "a.doctag in ('PB','OP') and b.baleno is not null  and b.gocd='TR' ) a, ";

            sql += "(select a.blautono, a.baleno, a.baleyr, a.baleyr || a.baleno balenoyr, ";
            sql += "sum(case a.drcr when 'D' then 1 when 'C' then - 1 end) bnos ";
            sql += " from " + schema + ".t_bilty a, " + scm + ".t_bilty_hdr b, " + schema + ".t_cntrl_hdr d ";
            sql += "where a.autono = b.autono(+) and ";
            if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
            sql += "a.autono = d.autono(+) ";
            sql += "group by a.blautono, a.baleno, a.baleyr, a.baleyr || a.baleno) b, ";

            sql += " (select a.blautono, a.balenoyr,  ";
            sql += " sum(case a.drcr when 'C' then 1 when 'D' then - 1 end) bnos  from ";
            sql += " (select a.blautono, a.baleyr || a.baleno balenoyr, a.drcr ";
            sql += " from " + schema + ".t_bale a, " + schema + ".t_bale_hdr b, " + schema + ".t_cntrl_hdr d  ";
            sql += " where a.autono = b.autono(+) and a.autono = d.autono(+) and ";
            if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
            sql += "b.txtag = 'RC' and nvl(d.cancel, 'N')= 'N'  ) a ";
            sql += " group by a.blautono, a.balenoyr) h,	";

            sql += "" + schema + ".t_txntrans c, " + schema + ".t_txn d ," + scmf + ".m_subleg e," + schema + ".t_txndtl f," + schema + ".m_sitem g ";
            sql += "where a.autono = b.blautono(+) and a.balenoyr = b.balenoyr(+) and c.TRANSLCD = e.slcd(+)and a.baleno=f.baleno(+) and f.itcd = g.itcd(+) and ";
            sql += "a.autono = c.autono(+) and a.autono = d.autono(+) and c.lrno is not null  ";
            sql += "and a.autono = h.blautono(+) and a.balenoyr = h.balenoyr(+) ";
            if (blautono.retStr() != "") sql += " and a.autono in(" + blautono + ")  ";
            if (translcd.retStr() != "") sql += " and c.TRANSLCD in(" + translcd + ")  ";
            if (lrnoLike.retStr() != "") sql += "and c.lrno like '%" + lrnoLike.retStr() + "%'  ";
            sql += " and 1 - nvl(b.bnos, 0) > 0 and 1 - nvl(h.bnos, 0) > 0 ";
            tbl = masterHelpFa.SQLquery(sql);
            return tbl;
        }
        public DataTable getPendingPackslip(string docdt, string slcd, string skipautono = "")
        {
            DataTable tbl;
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string sql = "";
            sql = "";
            sql += "select a.autono, c.docno, c.docdt, c.slcd, g.slnm, nvl(g.slarea,g.district) district ";
            sql += "from " + scm + ".t_txn a, " + scm + ".t_cntrl_hdr c, ";
            sql += scm + ".m_doctype d, " + scm + ".t_txn_linkno e, " + scmf + ".m_subleg g ";
            sql += "where a.autono=c.autono(+) and c.doccd=d.doccd(+) and d.doctype in ('SPSLP') and a.slcd=g.slcd(+) and ";
            sql += "a.autono=e.linkautono(+) and e.autono is null and ";
            if (skipautono.retStr() != "") sql += "e.autono not in (" + skipautono + ") and ";
            sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and ";
            sql += "c.docdt <= to_date('" + docdt + "','dd/mm/yyyy') ";
            if (slcd.retStr() != "") sql += "and c.slcd = '" + slcd + "' ";
            tbl = masterHelpFa.SQLquery(sql);
            return tbl;
        }
        public DataTable getPendRecfromMutia(string docdt, string mutslcd = "", string blautono = "", string skipautono = "", string schema = "", string lrnoLike = "", bool ShowOnlyAfterIssue = false)
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (schema == "") schema = scm;
            string sql = "";

            sql = "";
            sql += " select a.blautono, a.mutslcd, a.trem, j.slnm mutianm, j.regmobile, a.baleno, a.baleyr, e.lrno, e.lrdt,	 ";
            sql += " g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, g.slno blslno, g.nos, g.qnty,	";
            sql += " '' shade, g.pageno, g.pageslno, ";
            sql += " f.prefno, f.prefdt, nvl(b.bnos, 0)-nvl(c.bnos,0) bnos, h.styleno||' '||h.itnm  itstyle,a.status,a.docdt from ";

            sql += "( ";
            sql += "select distinct a.blautono, b.mutslcd, b.trem, a.baleno, a.baleyr, a.baleyr || a.baleno balenoyr,'Issued' status,d.docdt ";
            sql += "from " + schema + ".t_bilty a, " + schema + ".t_bilty_hdr b, " + schema + ".t_cntrl_hdr d ";
            sql += "where a.autono = b.autono(+) and a.autono = d.autono(+) and ";
            sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and  ";
            sql += "d.docdt <= to_date('" + docdt + "', 'dd/mm/yyyy') ";
            if (ShowOnlyAfterIssue == false)
            {
                sql += "union all ";

                //sql += "select distinct a.autono blautono, '' mutslcd, '' trem, b.baleno, b.baleyr, b.baleyr || b.baleno balenoyr ";
                sql += "select distinct a.autono blautono, e.translcd mutslcd, '' trem, b.baleno, b.baleyr, b.baleyr || b.baleno balenoyr,'Direct' status,a.docdt ";
                sql += "from " + schema + ".t_txn a, " + schema + ".t_txndtl b, " + schema + ".t_cntrl_hdr d, " + schema + ".t_txntrans e ";
                sql += "where a.autono = b.autono(+) and a.autono = d.autono(+) and a.autono = e.autono(+) and ";
                sql += "a.autono not in (select distinct blautono from " + schema + ".t_bilty ) and ";
                sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and ";
                if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
                sql += "d.docdt <= to_date('" + docdt + "', 'dd/mm/yyyy') and ";
                sql += "a.doctag in ('PB','OP') and b.baleno is not null  and b.gocd='TR' ";
            }
            sql += ") a, ";

            sql += " (select a.blautono, a.baleno, a.baleyr, a.baleyr || a.baleno balenoyr,	";
            sql += " sum(case a.drcr when 'D' then 1 when 'C' then - 1 end) bnos  ";
            sql += " from " + schema + ".t_bilty a, " + schema + ".t_bilty_hdr b, " + schema + ".t_cntrl_hdr d ";
            sql += " where a.autono = b.autono(+) and ";
            if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
            sql += "a.autono = d.autono(+)  ";
            sql += " group by a.blautono, a.baleno, a.baleyr, a.baleyr || a.baleno) b,  ";

            sql += " (select a.blautono, a.balenoyr,  ";
            sql += " sum(case a.drcr when 'C' then 1 when 'D' then - 1 end) bnos  from ";
            sql += " (select a.blautono, a.baleyr || a.baleno balenoyr, a.drcr ";
            sql += " from " + schema + ".t_bale a, " + schema + ".t_bale_hdr b, " + schema + ".t_cntrl_hdr d  ";
            sql += " where a.autono = b.autono(+) and a.autono = d.autono(+) and ";
            if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
            sql += "b.txtag = 'RC' and nvl(d.cancel, 'N')= 'N'  ) a ";
            sql += " group by a.blautono, a.balenoyr) c,	";

            sql += " " + schema + ".t_txntrans e, " + schema + ".t_txn f, " + schema + ".t_txndtl g,  ";
            sql += " " + schema + ".m_sitem h, " + schema + ".m_group i, " + scmf + ".m_subleg j  ";
            sql += " where a.blautono = b.blautono(+) and a.balenoyr = b.balenoyr(+) and  ";
            sql += " a.blautono = c.blautono(+) and a.balenoyr = c.balenoyr(+) and  ";
            sql += " a.blautono = e.autono(+) and a.blautono = f.autono(+) and a.blautono = g.autono(+) and a.baleno=g.baleno(+) and ";
            sql += " g.itcd = h.itcd(+) and h.itgrpcd = i.itgrpcd(+) and a.mutslcd = j.slcd(+) ";
            if (mutslcd.retStr() != "") sql += " and a.mutslcd in (" + mutslcd + ")  ";
            if (blautono.retStr() != "") sql += " and a.blautono in(" + blautono + ")";
            if (lrnoLike.retStr() != "") sql += "and e.lrno like '%" + lrnoLike.retStr() + "%'  ";
            //sql += " and ( nvl(b.bnos, 0)-nvl(c.bnos,0) > 0 or b.bnos is null) ";
            sql += " and ( (nvl(b.bnos, 0)-nvl(c.bnos,0) > 0) or (b.bnos is null and 1 - nvl(c.bnos, 0) > 0)) ";
            tbl = masterHelpFa.SQLquery(sql);
            return tbl;
        }
        public DataTable getPendKhasra(string docdt, string blautono = "", string skipautono = "", string schema = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (schema.retStr() != "") scm = schema;
            string sql = "";

            sql = "";
            sql += "select a.autono, a.docno, a.docdt, a.blautono, a.blslno, a.baleno, a.baleyr, e.lrno, e.lrdt, ";
            sql += "g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, g.nos, g.qnty, h.styleno||' '||h.itnm  itstyle, ";
            sql += "listagg(j.shade,',') within group (order by j.autono, j.txnslno) as shade, ";
            sql += "g.pageno, g.pageslno, g.rate, ";
            sql += "f.prefno, f.prefdt, nvl(b.bnos, 0) bnos from ";

            sql += "  (select distinct a.autono, d.docdt, d.docno, a.blautono, a.blslno, a.baleno, a.baleyr, a.baleyr || a.baleno balenoyr ";
            sql += "  from " + scm + ".t_bale a, " + scm + ".t_bale_hdr b, " + scm + ".t_cntrl_hdr d ";
            sql += "  where a.autono = b.autono(+) and a.autono = d.autono(+) and b.txtag = 'RC' and nvl(d.cancel, 'N') = 'N' and ";
            sql += "  d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and ";
            if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
            sql += "  d.docdt <= to_date('" + docdt + "', 'dd/mm/yyyy') ) a, ";

            sql += "(select a.blautono, a.blslno, a.baleyr || a.baleno balenoyr, ";
            sql += "sum(case a.drcr when 'D' then 1 when 'C' then - 1 end) bnos ";
            sql += "from " + scm + ".t_bale a, " + scm + ".t_bale_hdr b, " + scm + ".t_cntrl_hdr d ";
            sql += "where a.autono = b.autono(+) and a.autono = d.autono(+) and ";
            if (skipautono.retStr() != "") sql += "a.autono not in (" + skipautono + ") and ";
            //sql += "b.txtag = 'KH' and nvl(d.cancel, 'N')= 'N' and a.slno > 1000 ";
            sql += "b.txtag = 'KH' and nvl(d.cancel, 'N')= 'N' and a.slno > 5000 ";
            sql += " group by a.blautono, a.blslno, a.baleno, a.baleyr, a.baleyr || a.baleno) b, ";

            sql += scm + ".t_txntrans e, " + scm + ".t_txn f, " + scm + ".t_txndtl g, ";
            sql += scm + ".m_sitem h, " + scm + ".m_group i, " + scm + ".t_batchdtl j ";
            sql += "where a.blautono = b.blautono(+) and a.balenoyr = b.balenoyr(+) and ";
            sql += "a.blautono = e.autono(+) and a.blautono = f.autono(+) and g.autono=j.autono(+) and g.slno=j.txnslno(+) and ";
            sql += "a.blautono = g.autono(+) and a.blslno = g.slno(+) and ";
            if (blautono.retStr() != "") sql += "a.blautono in(" + blautono + ") and ";
            sql += "g.itcd = h.itcd(+) and h.itgrpcd = i.itgrpcd(+) and 1+nvl(b.bnos, 0) > 0 ";
            sql += "group by a.autono, a.docno, a.docdt, a.blautono, a.blslno, a.baleno, a.baleyr, e.lrno, e.lrdt, ";
            sql += "g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, g.nos, g.qnty, h.styleno||' '||h.itnm, ";
            sql += "g.pageno, g.pageslno,g.rate, f.prefno, f.prefdt, nvl(b.bnos, 0) ";
            tbl = masterHelpFa.SQLquery(sql);
            return tbl;
        }
        public string IsTransactionFound(string ITCD, string BARNO, string skipautono)
        {
            var sql = "";
            sql += "select b.AUTONO,c.docno,c.docdt from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHMST a," + CommVar.CurSchema(UNQSNO) + ".T_BATCHDTL b," + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR c ";
            sql += "WHERE a.barno=b.barno and b.autono=C.AUTONO(+) ";
            if (ITCD.retStr() != "") sql += "and  a.ITCD in(" + ITCD + ")  ";
            if (BARNO.retStr() != "") sql += "and  a.BARNO in(" + BARNO + ")  ";
            if (skipautono.retStr() != "") sql += "and b.autono not in(" + skipautono + ") and  a.autono in(" + skipautono + ")  ";
            sql += "and ROWNUM = 1 ";
            DataTable dt = masterHelpFa.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                return "</br>" + "DOCNO : " + dt.Rows[0]["docno"].retStr() + "</br>" + "DOCDATE : " + dt.Rows[0]["docdt"].retStr().Remove(10) + "</br>" +
                    "AUTONO : " + dt.Rows[0]["AUTONO"].retStr() + "</br>";
            }
            else
            {
                return "";
            }
        }
        public string GetPendOrderSql(string slcd = "", string ordupto = "", string ordautono = "", string orderslno = "", string txnupto = "", string skipautono = "", string menupara = "SB", string brandcd = "", bool OnlyBal = true, string ordfromdt = "", string itcd = "", string agslcd = "", string slmslcd = "", string itgrpcd = "", string curschema = "", string finschema = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (curschema != "") scm = curschema;
            if (finschema != "") scmf = finschema;

            string doctype = "SORD";
            if (menupara.Substring(0, 2) == "SB") doctype = "SORD"; else doctype = "PORD";

            if (ordupto == "") ordupto = txnupto;
            if (ordupto != "") ordupto = ordupto.retDateStr();
            if (skipautono == null) skipautono = "";
            if (slcd == null) slcd = "";
            if (slcd != "") { if (slcd.IndexOf("'") < 0) slcd = "'" + slcd + "'"; }

            string sqlc = "";
            sqlc += "b.compcd='" + COM + "' and "; // b.loccd='" + LOC + "' and ";
            if (slcd != "") sqlc += "c.slcd in (" + slcd + ") and ";
            if (skipautono != "") sqlc += "a.autono <> '" + skipautono + "' and ";
            sqlc += "nvl(b.cancel,'N')='N' ";

            string sql = "";

            sql += "select a.autono,a.slno, n.slcd, j.slnm, j.district, j.slarea, o.doccd, o.docno, to_char(o.docdt,'dd/mm/yyyy') docdt, nvl(m.stktype,'') stktype, nvl(m.freestk,'') freestk, nvl(m.rate,0) rate, ";
            sql += "n.agslcd, n.slmslcd, k.slnm agslnm, l.slnm slmslnm, ";
            sql += "d.styleno, m.itcd, m.sizecd, m.colrcd, d.itnm, m.delvdt, m.itrem, ";
            sql += "d.uomcd, g.uomnm, g.decimals, d.itgrpcd, h.itgrpnm,h.bargentype, d.brandcd, i.brandnm, ";
            sql += "e.sizenm, e.print_seq, f.colrnm, nvl(a.qnty,0) ordqnty, ";
            sql += "nvl(a.qnty,0) - nvl(b.qnty,0) -nvl(c.qnty,0) balqnty,m.pdesign,m.partcd,p.partnm ,p.prtbarcode,f.clrbarcode,e.szbarcode,nvl(d.hsncode,h.hsncode)hsncode,nvl(d.negstock,h.negstock)negstock from ";

            sql += "( select a.autono, a.slno, a.qnty ";
            sql += "from " + scm + ".t_sorddtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_sord c, " + scm + ".m_doctype d ";
            sql += "where a.autono=b.autono and a.autono=c.autono and b.doccd=d.doccd and d.doctype = '" + doctype + "' and ";
            if (ordautono != "") sql += "a.autono in (" + ordautono + ") and ";
            if (orderslno != "") sql += "a.slno in (" + orderslno + ") and ";
            if (ordfromdt.retStr() != "") sql += "b.docdt >= to_date('" + ordfromdt + "','dd/mm/yyyy') and ";
            if (ordupto != "") sql += "b.docdt <= to_date('" + ordupto + "','dd/mm/yyyy') and ";
            if (agslcd != "") sql += "c.agslcd in (" + agslcd + ") and ";
            if (slmslcd != "") sql += "c.slmslcd in (" + slmslcd + ") and ";
            if (itcd != "") sql += "a.itcd in (" + itcd + ") and ";
            sql += sqlc;
            sql += ") a, ";

            //Ord Canc
            sql += "( select a.ordautono, a.ordslno, ";
            sql += "sum(case a.stkdrcr when 'C' then a.qnty when 'D' then a.qnty*-1 end) qnty ";
            sql += "from " + scm + ".t_sorddtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_sord_canc c, " + scm + ".m_doctype d ";
            sql += "where a.autono=b.autono and a.autono=c.autono and b.doccd=d.doccd and d.doctype <> '" + doctype + "' and ";
            if (OnlyBal == false) sql += "a.autono='xx' and ";
            if (txnupto != "") sql += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
            sql += sqlc;
            sql += "group by a.ordautono, a.ordslno ) b, ";

            //Pslip or Bill
            sql += "( select a.ordautono, a.ordslno, ";
            sql += "sum(case a.stkdrcr when 'C' then a.qnty when 'D' then a.qnty*-1 end) qnty ";
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c, " + scm + ".m_doctype d ";
            sql += "where a.autono=b.autono and a.autono=c.autono and b.doccd=d.doccd(+) and ";
            if (OnlyBal == false) sql += "a.autono='xx' and ";
            if (txnupto != "") sql += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
            sql += "d.doctype in ('SPSLP','SBILL','SBILD','SBCM','SBEXP','SPBL') and ";
            sql += sqlc;
            sql += "group by a.ordautono, a.ordslno ) c, ";

            sql += scm + ".m_sitem d, " + scm + ".m_size e, " + scm + ".m_color f, " + scmf + ".m_uom g, " + scm + ".m_group h, " + scm + ".m_brand i, " + scmf + ".m_subleg j, ";
            sql += scmf + ".m_subleg k, " + scmf + ".m_subleg l, " + scm + ".t_sorddtl m, " + scm + ".t_sord n, " + scm + ".t_cntrl_hdr o, " + scm + ".m_parts p ";
            sql += "where a.autono=b.ordautono(+) and a.autono=c.ordautono(+) and ";
            sql += "a.slno=b.ordslno(+) and a.slno=c.ordslno(+) and ";
            sql += "a.autono=m.autono(+) and a.slno=m.slno(+) and a.autono=n.autono(+) and a.autono=o.autono(+) and ";
            sql += "m.itcd=d.itcd(+) and m.sizecd=e.sizecd(+) and m.colrcd=f.colrcd(+) and d.uomcd=g.uomcd(+) and d.itgrpcd=h.itgrpcd(+) and d.brandcd=i.brandcd(+) and n.slcd=j.slcd(+) and ";
            sql += "n.agslcd=k.slcd(+) and n.slmslcd=l.slcd(+)  and m.partcd=p.partcd(+)  and ";
            if (brandcd != "") sql += "h.brandcd in (" + brandcd + ") and ";
            if (itgrpcd != "") sql += "d.itgrpcd in (" + itgrpcd + ") and ";
            sql += "nvl(a.qnty,0) - nvl(b.qnty,0) - nvl(c.qnty,0) <> 0 ";
            sql += "order by styleno, print_seq, sizenm";
            return sql;
        }
        public DataTable GetPendOrder(string slcd = "", string ordupto = "", string ordautono = "", string orderslno = "", string txnupto = "", string skipautono = "", string menupara = "SB", string brandcd = "", bool OnlyBal = true, string ordfromdt = "", string itcd = "", string agslcd = "", string slmslcd = "", string itgrpcd = "", string curschema = "", string finschema = "")
        {
            string sql = GetPendOrderSql(slcd, ordupto, ordautono, orderslno, txnupto, skipautono, menupara, brandcd, OnlyBal, ordfromdt, itcd, agslcd, slmslcd, itgrpcd, curschema, finschema);
            DataTable tbl = new DataTable();
            tbl = SQLquery(sql);
            return tbl;
        }

        public string GenerateBARNO(string ITCD, string CLRBARCODE, string SZBARCODE)
        {
            //itcd last 7  7
            //color clrbarcode  3
            //size szbarcode   2
            //return ITCD.retStr().Substring(1, 7) + MTBARCODE.retStr() + PRTBARCODE.retStr() + CLRBARCODE.retStr() + SZBARCODE.retStr();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COLRCD = "", SIZECD = "";
            if (CLRBARCODE.retStr() != "")
            {
                string clrbarcode = CLRBARCODE.retStr();
                COLRCD = DB.M_COLOR.Where(a => a.CLRBARCODE == clrbarcode).Select(b => b.COLRCD).FirstOrDefault();
            }

            if (SZBARCODE.retStr() != "")
            {
                string szbarcode = SZBARCODE.retStr();
                SIZECD = DB.M_SIZE.Where(a => a.SZBARCODE == szbarcode).Select(b => b.SIZECD).FirstOrDefault();
            }
            string barno = DB.T_BATCHMST.Where(a => a.COLRCD == COLRCD && a.SIZECD == SIZECD && a.ITCD == ITCD && a.COMMONUNIQBAR == "C").Select(b => b.BARNO).FirstOrDefault();
            if (barno == null)
            {
                //barno = ITCD.retStr().Substring(1, 8) + CLRBARCODE.retStr() + SZBARCODE.retStr();
                barno = ITCD.retStr().Substring(1, ITCD.Length - 1) + CLRBARCODE.retStr() + SZBARCODE.retStr();
            }
            return barno;
        }
        //public DataTable GetSyscnfgData(string EFFDT)
        //{
        //    string Scm = CommVar.CurSchema(UNQSNO);
        //    string compcd = CommVar.Compcd(UNQSNO);
        //    string str = "";
        //    str += "select a.effdt, b.wppricegen, b.rppricegen, b.wpper, b.rpper, b.priceincode from ";
        //    str += "(select a.m_autono, a.effdt, row_number() over(order by a.effdt desc) as rn ";
        //    str += "from " + Scm + ".m_syscnfg a ";
        //    str += "where (a.compcd = '" + compcd + "' or a.compcd is null) and ";
        //    str += "a.effdt <= to_date('" + EFFDT + "', 'dd/mm/yyyy') ) a, ";
        //    str += Scm + ".m_syscnfg b ";
        //    str += "where a.m_autono = b.m_autono(+) and a.rn = 1 ";

        //    DataTable dt = masterHelpFa.SQLquery(str);
        //    return dt;

        //}
        public double getSlcdTCSonCalc(string slcdpanno, string docdt, string menupara = "SB", string autono = "")
        {
            double rtval = 0;
            string sql = "", COM = CommVar.Compcd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string trcd = "'SB','SD'";
            string salpur = "S";
            if (menupara == "PB" || menupara == "PD" || menupara == "OP" || menupara == "OTH") { trcd = "'PB','PD'"; salpur = "P"; }

            sql = "select sum(nvl(amt,0)) amt from (";

            sql += "select sum(a.amt) amt ";
            sql += "from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_hdr b, " + scmf + ".t_cntrl_hdr c, " + scmf + ".m_subleg d, " + scmf + ".m_genleg e ";
            sql += "where a.autono = b.autono(+) and a.autono = c.autono(+) and a.slcd = d.slcd(+) and d.panno = '" + slcdpanno + "' and ";
            sql += "c.compcd = '" + COM + "' and b.trcd in (" + trcd + ") and nvl(c.cancel, 'N')= 'N' and ";
            if (autono.retStr() != "") sql += "a.autono not in ('" + autono + "') and ";
            sql += "a.glcd=e.glcd(+) and e.linkcd in (" + (salpur == "P" ? "'C'" : "'D'") + ") and ";
            sql += "c.docdt <= to_date('" + docdt + "', 'dd/mm/yyyy') ";

            sql += "union all ";

            sql += "select sum(case a.drcr when e.linkcd then a.amt else a.amt*-1 end) amt ";
            sql += "from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_hdr b, " + scmf + ".t_cntrl_hdr c, " + scmf + ".m_doctype d, ";
            sql += scmf + ".m_genleg e, " + scmf + ".m_subleg f ";
            sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and c.doccd=d.doccd(+) and ";
            sql += "c.compcd='" + COM + "' and nvl(c.cancel,'N')='N' and d.doctype in ('AOPEN') and a.drcr in ('D','C') and ";
            sql += "a.glcd=e.glcd(+) and a.slcd=f.slcd(+) and e.linkcd in (" + (salpur == "P" ? "'C'" : "'D'") + ") and f.panno='" + slcdpanno + "' ";

            sql += ") ";

            DataTable tbl = SQLquery(sql);
            if (tbl.Rows.Count == 1) rtval = tbl.Rows[0]["amt"].retDbl();
            return rtval;
        }
        public DataTable GetRateHistory(string slcd, string partycd, string doctype, string itcd, string fdt = "", string tdt = "")
        {
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string scm_prevyr = CommVar.LastYearSchema(UNQSNO), scmf_prevyr = CommVar.FinSchemaPrevYr(UNQSNO);
            string sql = "";
            sql = "select a.slcd, a.autono, a.docno, a.docdt, a.qnty, a.rate, a.slnm, a.city, ";
            sql += "a.scmdiscrate, a.scmdisctype,a.pcstype from (  ";

            sql += "select a.slcd, a.autono, d.docno, d.docdt, sum(nvl(f.qnty,0)) qnty, b.rate, e.slnm, e.district city, ";
            sql += "(case when nvl(b.listdiscper,0)=0 then nvl(b.scmdiscrate,0) else nvl(b.listdiscper,0) end) scmdiscrate, b.scmdisctype,f.pcstype  ";
            sql += "from " + scm + ".t_txn a," + scm + ".t_txndtl b," + scm + ".m_doctype c," + scm + ".t_cntrl_hdr d ," + scmf + ".m_subleg e," + scm + ".T_BATCHDTL f," + scm + ".M_SITEM g  ";
            sql += "where a.autono=b.autono and a.autono=d.autono and a.doccd=c.doccd(+) and a.slcd=e.slcd and a.autono=f.autono  and b.itcd=g.itcd(+) and b.slno=f.txnslno and d.compcd='" + COM + "'  ";
            if (itcd.retStr() != "") sql += " and b.itcd in(" + itcd + ") ";
            if (slcd.retStr() != "") sql += " and (e.slcd in(" + slcd + ") ";
            if (partycd.retStr() != "") sql += "or e.partycd=" + partycd + " ";
            if (slcd.retStr() != "" || partycd.retStr() != "") sql += ") ";
            sql += "  and c.doctype in (" + doctype + ") ";
            if (fdt != "") sql += "and d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') ";
            if (tdt != "") sql += "and d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
            sql += "  group by a.slcd, a.autono, d.docno, d.docdt, b.rate, e.slnm, e.district,(case when nvl(b.listdiscper,0)=0 then nvl(b.scmdiscrate,0) else nvl(b.listdiscper,0) end) , b.scmdisctype,f.pcstype ";
            //sql += "order by d.docdt,d.docno desc ";

            if (CommVar.LastYearSchema(UNQSNO) != "")
            {
                sql += "union all ";
                sql += "select a.slcd, a.autono, d.docno, d.docdt, sum(nvl(f.qnty,0)) qnty, b.rate, e.slnm, e.district city, ";
                sql += "(case when nvl(b.listdiscper,0)=0 then nvl(b.scmdiscrate,0) else nvl(b.listdiscper,0) end) scmdiscrate, b.scmdisctype,f.pcstype  ";
                sql += "from " + scm_prevyr + ".t_txn a," + scm_prevyr + ".t_txndtl b," + scm_prevyr + ".m_doctype c," + scm_prevyr + ".t_cntrl_hdr d ," + scmf_prevyr + ".m_subleg e," + scm_prevyr + ".T_BATCHDTL f," + scm_prevyr + ".M_SITEM g  ";
                sql += "where a.autono=b.autono and a.autono=d.autono and a.doccd=c.doccd(+) and a.slcd=e.slcd and a.autono=f.autono  and b.itcd=g.itcd(+) and b.slno=f.txnslno and d.compcd='" + COM + "'  ";
                if (itcd.retStr() != "") sql += " and b.itcd in(" + itcd + ") ";
                if (slcd.retStr() != "") sql += " and (e.slcd in(" + slcd + ") ";
                if (partycd.retStr() != "") sql += "or e.partycd=" + partycd + " ";
                if (slcd.retStr() != "" || partycd.retStr() != "") sql += ") ";
                sql += "  and c.doctype in (" + doctype + ") ";
                if (fdt != "") sql += "and d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') ";
                if (tdt != "") sql += "and d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
                sql += "  group by a.slcd, a.autono, d.docno, d.docdt, b.rate, e.slnm, e.district,(case when nvl(b.listdiscper,0)=0 then nvl(b.scmdiscrate,0) else nvl(b.listdiscper,0) end) , b.scmdisctype,f.pcstype ";

            }
            sql += " )a order by (a.docdt)desc,(a.docno) desc ";
            var dt = masterHelpFa.SQLquery(sql);
            return dt;
        }
        public bool IsValidBarno(string barno)
        {
            string scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            sql += " select distinct a.SLCD,a.autono,d.docno,d.docdt,b.qnty,b.rate,e.SLNM,e.district city ";
            //sql += " from " + scm + ".t_txn a," + scm + ".t_txndtl b," + scm + ".m_doctype c," + scm + ".t_cntrl_hdr d ," + scmf + ".m_subleg e  ";
            //sql += " where a.autono=b.autono and a.autono=d.autono and a.doccd=c.DOCCD and a.slcd=e.slcd  and d.compcd='" + COM + "' and d.loccd='" + LOC + "' and itcd='" + itcd + "' and c.doctype='" + doctype + "' ";
            //sql += " order by d.docdt,d.docno desc ";
            var dt = masterHelpFa.SQLquery(sql);
            return true;
        }
        public string TranBarcodeGenerate(string doccd, string lbatchini, string docbarcode, string UNIQNO, int slno)
        {//YRCODE	2,lbatchini	2,TXN UNIQ NO	7,SLNO	4
            var yrcd = CommVar.YearCode(UNQSNO).Substring(2, 2);
            return yrcd + lbatchini + UNIQNO + slno.ToString().PadLeft(4, '0');
        }
        public DataTable GetSemiItems(DataTable finitems, string jobcd = "", string itcd = "", string asondt = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            //datatable finitems must have itcd, partcd, sizecd, itsizecd, qnty field;
            string sql = "";
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scmi = CommVar.InvSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

            string selitcd = CommFunc.retSqlformat(string.Join(",", (from DataRow dr in finitems.Rows select dr["itcd"].ToString()).Distinct()));

            DataTable rsTmp = new DataTable();

            if (asondt == "") asondt = System.DateTime.Now.Date.ToString("dd/MM/yyyy");

            sql = "";
            sql += "select a.bomcd, a.effdt, a.itcd, a.baseqnty, b.slno, b.partcd, b.sizecd, a.itcd||nvl(b.partcd,'')||nvl(b.sizecd,'') itsizecd, ";
            sql += "c.modcd, c.itcd ritcd, c.itnm ritnm, c.uomcd ruomcd, nvl(d.decimals,0) decimals, d.uomnm ruomnm, nvl(c.std_qty,0) std_qty, c.qnty rqnty,c.mtrljobcd,e.mtrljobnm,c.rslno from ";
            sql += "(select a.bomcd, a.effdt, a.itcd, a.baseqnty from ";
            sql += "(select a.bomcd, a.effdt, a.itcd, a.baseqnty, ";
            sql += "row_number() over (partition by a.itcd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".m_sitembom a, " + scm + ".m_cntrl_hdr b ";
            sql += "where a.m_autono=b.m_autono and nvl(b.inactive_tag,'N')='N' and ";
            sql += "a.effdt <= to_date('" + asondt + "','dd/mm/yyyy') and a.itcd in (" + selitcd + ")  ";
            sql += ") a where a.rn=1 ) a, ";
            sql += "(select a.bomcd, a.slno, a.partcd, a.sizecd ";
            sql += "from " + scm + ".m_sitembompart a where a.jobcd='" + jobcd + "') b, ";
            sql += "(select a.bomcd, a.slno, 'S' modcd, a.rslno, a.itcd, b.itnm, a.qnty, b.uomcd, b.minpurqty std_qty,a.mtrljobcd ";
            sql += "from " + scm + ".m_sitembommtrl a, " + scm + ".m_sitem b ";
            sql += "where a.itcd=b.itcd) c, " + scmf + ".m_uom d, " + scm + ".m_mtrljobmst e ";
            sql += "where a.bomcd=b.bomcd and b.bomcd=c.bomcd and b.slno=c.slno and c.uomcd=d.uomcd(+) and c.mtrljobcd=e.mtrljobcd(+) ";

            rsTmp = SQLquery(sql);

            DataTable IR = new DataTable();
            IR.Columns.Add("itcd", typeof(string));
            IR.Columns.Add("itnm", typeof(string));
            IR.Columns.Add("partcd", typeof(string));
            IR.Columns.Add("sizecd", typeof(string));
            IR.Columns.Add("colrcd", typeof(string));
            IR.Columns.Add("modcd", typeof(string));
            IR.Columns.Add("ritcd", typeof(string));
            IR.Columns.Add("ritnm", typeof(string));
            IR.Columns.Add("ruomcd", typeof(string));
            IR.Columns.Add("ruomnm", typeof(string));
            IR.Columns.Add("rqty", typeof(double));

            IR.Columns.Add("slno", typeof(int));
            IR.Columns.Add("rslno", typeof(int));
            IR.Columns.Add("mtrljobcd", typeof(string));
            IR.Columns.Add("mtrljobnm", typeof(string));
            string sitcd = "", ssizecd = "", spartcd = "", sitsizecd = "", sitnm = "";

            Int32 maxF = 0, f = 0;
            Int32 maxR = 0, i = 0;
            Int32 rNo = 0;
            maxF = finitems.Rows.Count - 1;
            while (f <= maxF)
            {
                sitcd = finitems.Rows[f]["itcd"].ToString();
                ssizecd = finitems.Rows[f]["sizecd"].ToString();
                spartcd = finitems.Rows[f]["partcd"].ToString();
                sitsizecd = finitems.Rows[f]["itcd"].ToString() + finitems.Rows[f]["partcd"].ToString() + finitems.Rows[f]["sizecd"].ToString();
                sitnm = finitems.Rows[f]["itnm"].ToString();
                DataTable bomdata = new DataTable();
                var rows1 = rsTmp.AsEnumerable()
                              .Where(x => ((string)x["itsizecd"]) == sitsizecd);
                if (rows1.Any()) bomdata = rows1.CopyToDataTable();

                maxR = bomdata.Rows.Count - 1; i = 0;
                int rslno = 1;
                while (i <= maxR)
                {
                    double rqty = 0, bqty = 0, iqty = 0, reqqty = 0;
                    rqty = bomdata.Rows[i]["rqnty"].retDbl();
                    bqty = bomdata.Rows[i]["baseqnty"].retDbl();
                    iqty = finitems.Rows[f]["qnty"].retDbl();
                    //bqty = Convert.ToDouble(bomdata.Rows[f]["baseqnty"]);
                    //iqty = Convert.ToDouble(finitems.Rows[f]["qnty"]);
                    reqqty = Cn.Roundoff((iqty / bqty) * rqty, Convert.ToInt16(bomdata.Rows[i]["decimals"]));

                    bool recadd = true;
                    string slno = finitems.Rows[f]["slno"].retStr();
                    for (int q = 0; q <= IR.Rows.Count - 1; q++)
                    {
                        if (IR.Rows[q]["ritcd"].ToString() == bomdata.Rows[i]["ritcd"].ToString() && IR.Rows[q]["slno"].ToString() == slno)
                        {
                            IR.Rows[q]["rqty"] = IR.Rows[q]["rqty"].retDbl() + reqqty;
                            recadd = false;
                            break;
                        }
                    }
                    if (recadd == true)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["itcd"] = sitcd;
                        IR.Rows[rNo]["itnm"] = sitnm;
                        IR.Rows[rNo]["sizecd"] = ssizecd;
                        IR.Rows[rNo]["partcd"] = spartcd;
                        IR.Rows[rNo]["modcd"] = bomdata.Rows[i]["modcd"];
                        IR.Rows[rNo]["ritcd"] = bomdata.Rows[i]["ritcd"];
                        IR.Rows[rNo]["ritnm"] = bomdata.Rows[i]["ritnm"];
                        IR.Rows[rNo]["ruomcd"] = bomdata.Rows[i]["ruomcd"];
                        IR.Rows[rNo]["ruomnm"] = bomdata.Rows[i]["ruomnm"];
                        IR.Rows[rNo]["rqty"] = reqqty;

                        IR.Rows[rNo]["mtrljobcd"] = bomdata.Rows[i]["mtrljobcd"];
                        IR.Rows[rNo]["mtrljobnm"] = bomdata.Rows[i]["mtrljobnm"];
                        IR.Rows[rNo]["slno"] = slno;
                        IR.Rows[rNo]["rslno"] = rslno;
                        rslno++;
                    }
                    i++;
                }
                f++;
            }
            return IR;
        }
        public List<DropDown_list_StkType> GetforStkTypeSelection(string stktype = "")
        {
            List<DropDown_list_StkType> DropDown_list_StkType = new List<DropDown_list_StkType>();

            DropDown_list_StkType DropDown_list1 = new DropDown_list_StkType();
            DropDown_list1.value = "R";
            DropDown_list1.text = "Raka";
            DropDown_list_StkType.Add(DropDown_list1);
            DropDown_list_StkType DropDown_list2 = new DropDown_list_StkType();
            DropDown_list2.value = "L";
            DropDown_list2.text = "Loose";
            DropDown_list_StkType.Add(DropDown_list2);
            DropDown_list_StkType DropDown_list3 = new DropDown_list_StkType();
            DropDown_list3.value = "D";
            DropDown_list3.text = "Destroy";
            DropDown_list_StkType.Add(DropDown_list3);
            DropDown_list_StkType DropDown_list4 = new DropDown_list_StkType();
            DropDown_list4.value = "F";
            DropDown_list4.text = "";
            DropDown_list_StkType.Add(DropDown_list4);
            return DropDown_list_StkType;
        }
        public string genStockinExcel(DataTable rsPendOrd, string fld = "qnty", string hd = "", double checkpcs = 0, bool onlynegstk = false)
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            int testloop = 0;
            int testloop1 = 0;
            int testloop3 = 0;
            try
            {
                rsPendOrd.DefaultView.Sort = "itgrpnm, itgrpcd, styleno, itcd, print_seq, sizecdgrp, sizenm";
                rsPendOrd = rsPendOrd.DefaultView.ToTable();

                string selitcd = "";
                selitcd = "'" + string.Join("','", (from DataRow dr in rsPendOrd.AsEnumerable() select dr.Field<string>("itcd")).Distinct()) + "'";
                DataTable tbl = new DataTable();

                Int32 maxR = rsPendOrd.Rows.Count - 1;
                Int32 maxC = 0, rNo = 0;
                Int32 i = 0; double totbox = 0, approxvalue = 0;

                string excelstr = "";
                excelstr += "<table> ";

                if (hd != "") excelstr += "<thead><tr><td colspan=5 style='font-size:18px;font-weight: bold'>" + hd + "</td></tr></thead><tbody>";
                var rsDtl = rsPendOrd.Select("");

                maxC = rsDtl.Length - 1;
                int slno = 0;
                double tbox = 0, tpcs = 0, tset = 0;
                selitcd = "'" + string.Join("','", (from DataRow dr in rsDtl.AsEnumerable() select dr.Field<string>("itcd")).Distinct()) + "'";
                // Checking for size grouping
                DataTable rssize = retSizeGrpData(selitcd);
                int j = 0;

                while (j <= maxC)
                {
                    ++testloop;
                    if (testloop == 22)
                    {
                        //testloop = 0;
                    }
                    string itgrpcd = rsDtl[j]["itgrpcd"].ToString();
                    string itgrpnm = rsDtl[j]["itgrpnm"].ToString();
                    var rssizehead = (from DataRow dr in rssize.Rows
                                      select new
                                      {
                                          itgrpcd = dr["itgrpcd"],
                                          sizecdgrp = dr["sizecdgrp"],
                                          print_seq = dr["print_seq"]
                                      }).Where(x => x.itgrpcd.ToString() == itgrpcd).Distinct().OrderBy(x => x.print_seq.ToString()).ToList();

                    DataTable tblsizegrp = ListToDatatable.LINQResultToDataTable(rssizehead);

                    string exlhd = "";

                    DataTable IR = new DataTable();
                    IR.Columns.Add("itgrpcd", typeof(string), "");
                    IR.Columns.Add("itgrpnm", typeof(string), "");
                    IR.Columns.Add("itcd", typeof(string), "");
                    IR.Columns.Add("stktype", typeof(string), "");
                    IR.Columns.Add("negstk", typeof(string), "");
                    IR.Columns.Add("styleno", typeof(string), "");
                    IR.Columns.Add("sizecd", typeof(string), "");
                    IR.Columns.Add("rate", typeof(double), "");
                    IR.Columns.Add("sizecdgrp", typeof(string), "");
                    IR.Columns.Add("pcsperbox", typeof(double), "");
                    IR.Columns.Add("pcsperset", typeof(double), "");
                    IR.Columns.Add("mixsize", typeof(string), "");
                    IR.Columns.Add("itmexist", typeof(string), "");
                    IR.Columns.Add("tqnty", typeof(double), "");
                    for (int z = 0; z <= tblsizegrp.Rows.Count - 1; z++)
                    {
                        string sznm = retsizemaxmin(tblsizegrp.Rows[z]["sizecdgrp"].ToString());
                        IR.Columns.Add(sznm, typeof(double), "");
                    }
                    bool itgrpshowthis = false;
                    testloop1 = 0;
                    while (rsDtl[j]["itgrpcd"].ToString() == itgrpcd)
                    {
                        ++testloop1;
                        if (testloop == 22 && testloop1 == 488)
                        {
                            //testloop1 = 0;
                        }
                        string check1 = rsDtl[j]["itcd"].ToString() + rsDtl[j]["stktype"].ToString();
                        double ibox = 0, ipcs = 0, rate = 0, ordqnty = 0, chkpcs = 0;

                        string itcd = rsDtl[j]["itcd"].ToString();
                        string stktype = rsDtl[j]["stktype"].ToString();

                        string itmexist = "Y";
                        bool itemshowthis = false, negstock = false;

                        double box; testloop3 = 0;
                        while (rsDtl[j]["itcd"].ToString() == itcd && rsDtl[j]["stktype"].ToString() == stktype)
                        {
                            ++testloop3;
                            if (testloop == 22 && testloop1 == 7 && testloop3 == 4)
                            {
                                //testloop3 = 0;
                            }
                            string sizecdgrp = rsDtl[j]["sizecdgrp"].ToString(), sizes = "", boxes = "";
                            ordqnty = 0;
                            while (rsDtl[j]["itcd"].ToString() == itcd && rsDtl[j]["stktype"].ToString() == stktype && rsDtl[j]["sizecdgrp"].ToString() == sizecdgrp)
                            {
                                ordqnty = ordqnty + Convert.ToDouble(rsDtl[j][fld]);
                                approxvalue += Math.Round(rate * Convert.ToDouble(rsDtl[j][fld]), 2);
                                j++;
                                if (j > maxC) break;
                            }

                            box = ConvPcstoBox(ordqnty, Convert.ToDouble(rsDtl[j - 1]["pcsperbox"]));
                            string szfld = retsizemaxmin(sizecdgrp);
                            if (szfld == "")
                            {
                                var sql1 = "select * from " + CommVar.CurSchema(UNQSNO) + ".t_txndtl where sizecd='" + rsDtl[j]["sizecd"].ToString() + "' and stktype='" + stktype + "'  and rownum=1";
                                var dt = masterHelpFa.SQLquery(sql1);
                                if (dt.Rows.Count > 0)
                                {
                                    return "Sizecd:" + rsDtl[j]["sizecd"].ToString() + " not found in the Item master( article no:" + rsDtl[j]["styleno"].ToString() + ") "
                                        + rsDtl[j]["sizecd"].ToString() + " found in [autono:" + dt.Rows[0]["autono"].retStr() + " date:" + dt.Rows[0]["docdt"].retDateStr() + "]";
                                }
                            }

                            bool showthis = true;
                            showthis = true;
                            if (checkpcs > 0 && box < checkpcs) showthis = false;
                            if (onlynegstk == true && box > 0) showthis = false;
                            if (ordqnty == 0) showthis = false;
                            if (ordqnty < 0 && onlynegstk == false)
                            {
                                negstock = true;
                            }
                            if (showthis == true)
                            {
                                if (itgrpshowthis == false)
                                {
                                    exlhd += "<thead><tr><td colspan=5 style='font-size:15px;font-weight: bold'>" + itgrpnm + " [" + itgrpcd + "]" + "</td></tr></thead>";
                                    itgrpshowthis = true;
                                }
                                if (itemshowthis == false)
                                {
                                    itemshowthis = true;
                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    IR.Rows[rNo]["itcd"] = itcd;
                                    IR.Rows[rNo]["styleno"] = rsDtl[j - 1]["styleno"];
                                    IR.Rows[rNo]["stktype"] = rsDtl[j - 1]["stktype"];
                                    IR.Rows[rNo]["itmexist"] = "Y";
                                    if (negstock == true) IR.Rows[rNo]["negstk"] = "Y";
                                    //IR.Rows[rNo]["rate"] = 0; // Convert.ToDouble(rsDtl[j]["rate"]);
                                }
                                IR.Rows[rNo][szfld] = box;
                                ipcs = ipcs + ordqnty;
                            }
                            if (j > maxC) break;
                        }
                        if (itemshowthis == true)
                        {
                            box = ConvPcstoBox(ipcs, Convert.ToDouble(rsDtl[j - 1]["pcsperbox"]));
                            ibox = ibox + box;
                            IR.Rows[rNo]["tqnty"] = ibox;
                            tbox = tbox + ibox;
                            tpcs = tpcs + ipcs;
                        }
                        if (j > maxC) break;
                    }
                    Int32 s = 0, maxS = 0;
                    maxS = IR.Rows.Count - 1;

                    if (itgrpshowthis == true)
                    {
                        excelstr += exlhd + "<thead><tr>";

                        excelstr += "<th style='border:0.5pt solid;width:130px'>" + "Style No" + "</th>";
                        for (int es = 13; es <= IR.Columns.Count - 1; es++)
                        {
                            string colnm = IR.Columns[es].ColumnName;
                            excelstr += "<th style='border:0.5pt solid;width:60px'>" + colnm + "</th>";
                        }
                        excelstr += "</tr></thead><tbody>";

                        while (s <= maxS)
                        {
                            excelstr += "<tr>";
                            string itmcolr = "";
                            if (IR.Rows[s]["itmexist"].ToString() == "N") itmcolr = "color:red;";
                            if (IR.Rows[s]["negstk"].ToString() == "Y") itmcolr = "color:red;";
                            excelstr += "<td style='border:0.1pt solid;" + itmcolr + "'>" + IR.Rows[s]["styleno"].ToString() + "</td>";
                            for (int es = 13; es <= IR.Columns.Count - 1; es++)
                            {
                                excelstr += "<td style='border:0.1pt solid;" + itmcolr + "'>" + IR.Rows[s][es] + "</td>";
                            }
                            excelstr += "</tr>";
                            s++;
                        }
                        excelstr += "</tbody><tr>" + "</tr>";
                    }
                    //
                }
                excelstr += "<tr>" + CommFunc.retHtmlCell("") + "</tr>";

                excelstr += "<tr>";
                excelstr += CommFunc.retHtmlCell("Total Boxes", "C", true, 14);
                excelstr += CommFunc.retHtmlCell(tbox.ToString(), "N", true, 16);
                excelstr += "</tr>";

                excelstr = excelstr + "</table>";

                return (excelstr);
            }
            catch (Exception ex)
            {
                var tyd = testloop;
                return "";
            }
        }
        public DataTable GetBaleStock(string tdt, string gocd = "", string baleno = "", string itcd = "", string mtrljobcd = "'FS'", string skipautono = "", string itgrpcd = "", string stylelike = "", string curschema = "", string finschema = "", bool mergeloca = false, string schema = "", string pagenoslno = "", bool balStockOnly = true)
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();

            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            //if (schema.retStr() != "") scm = schema;
            if (curschema != "") scm = curschema;
            if (finschema != "") scmf = finschema;
            string sql = "";
            //sql += "select a.gocd, a.baleno, a.baleyr, a.itcd, a.shade, a.nos, a.qnty, ";
            //sql += "b.docno, b.docdt, b.prefno, b.prefdt, b.rate, g.gonm, f.styleno, f.itnm, f.itgrpcd, f.uomcd, ";

            //sql += "b.pageno, b.pageslno, b.lrno from ";

            //sql += "( select a.gocd, b.baleno, b.baleyr, b.itcd, a.shade, b.baleno||b.baleyr||b.itcd baleitcd, ";
            //sql += "sum(a.nos*decode(a.stkdrcr,'D',1,-1)) nos, sum(a.qnty*decode(a.stkdrcr,'D',1,-1)) qnty ";
            //sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_txndtl b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d ";
            //sql += "where a.autono=b.autono(+) and a.txnslno=b.slno(+) and a.autono=c.autono(+) and a.autono=d.autono(+) and ";
            //sql += "d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and a.stkdrcr in ('D','C') and ";
            //sql += "a.mtrljobcd in (" + mtrljobcd + ") and ";
            //if (mergeloca == false) sql += "d.loccd='" + LOC + "' and ";
            //sql += "d.compcd='" + COM + "' and nvl(d.cancel,'N')='N' and a.baleno is not null ";
            //sql += "group by a.gocd, b.baleno, b.baleyr, b.itcd, a.shade, b.baleno||b.baleyr||b.itcd ) a, ";

            //sql += "( select a.rate, c.lrno, nvl(b.prefno,d.docno) prefno, nvl(b.prefdt,d.docdt) prefdt, ";
            //sql += "d.docno, d.docdt, a.pageno, a.pageslno, a.baleno||a.baleyr||a.itcd baleitcd ";
            //sql += "from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_txntrans c, " + scm + ".t_cntrl_hdr d ";
            //sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and a.autono=d.autono(+) and nvl(a.pageno,0) <> 0 ) b, ";
            //sql += "" + scm + ".m_sitem f, " + scmf + ".m_godown g ";
            //sql += "where a.baleitcd=b.baleitcd(+) and a.itcd=f.itcd(+) and ";
            //if (itgrpcd != "") sql += "f.itgrpcd in (" + itgrpcd + ") and ";
            //if (itcd != "") sql += "a.itcd in (" + itcd + ") and ";
            //if (baleno != "") sql += "a.baleno||baleyr in (" + baleno + ") and ";
            //sql += "a.gocd=g.gocd(+) ";
            //sql += "order by baleyr, baleno, styleno ";

            sql += "select a.gocd, k.gonm, a.blautono, a.blslno, a.baleno, a.baleyr, a.baleno||a.baleyr BaleNoBaleYrcd, e.lrno,to_char(e.lrdt,'dd/mm/yyyy') lrdt,  " + Environment.NewLine;
            sql += "g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, " + Environment.NewLine;
            sql += "g.nos, g.qnty, h.styleno||' '||h.itnm  itstyle, listagg(j.shade,',') within group (order by j.autono, j.txnslno) as shade, " + Environment.NewLine;
            sql += "g.pageno, g.pageslno, g.rate, g.txblval, g.othramt, f.prefno, f.prefdt,g.pageno||'/'||g.pageslno pagenoslno " + Environment.NewLine;
            sql += "from  ( " + Environment.NewLine;
            sql += "select c.gocd, a.blautono, a.blslno, a.baleno, a.baleyr, a.baleyr || a.baleno balenoyr, " + Environment.NewLine;
            sql += "sum(case c.stkdrcr when 'D' then c.qnty when 'C' then c.qnty*-1 end) qnty " + Environment.NewLine;
            sql += "from " + scm + ".t_bale a, " + scm + ".t_bale_hdr b, " + scm + ".t_txndtl c, " + scm + ".t_cntrl_hdr d " + Environment.NewLine;
            sql += "where a.autono = b.autono(+) and a.autono = d.autono(+) and " + Environment.NewLine;
            sql += "a.autono=c.autono(+) and a.slno=c.slno(+) and c.stkdrcr in ('D','C') and " + Environment.NewLine;
            sql += "d.compcd = '" + COM + "' and nvl(d.cancel, 'N') = 'N' and " + Environment.NewLine;
            if (mergeloca == false) sql += "d.loccd='" + LOC + "' and " + Environment.NewLine;
            sql += "d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') " + Environment.NewLine;
            if (skipautono.retStr() != "") sql += "and b.autono not in ('" + skipautono + "') " + Environment.NewLine;
            sql += "group by c.gocd, a.blautono, a.blslno, a.baleno, a.baleyr, a.baleyr || a.baleno " + Environment.NewLine;

            sql += ") a, " + Environment.NewLine;
            sql += "" + scm + ".t_txntrans e, " + scm + ".t_txn f, " + scm + ".t_txndtl g, " + scm + ".m_sitem h, " + scm + ".m_group i, " + scm + ".t_batchdtl j, " + scmf + ".m_godown k " + Environment.NewLine;
            sql += "where a.blautono = e.autono(+) and a.blautono = f.autono(+) and " + Environment.NewLine;
            sql += "g.autono=j.autono(+) and g.slno=j.txnslno(+) and a.blautono = g.autono(+) and a.blslno = g.slno(+) and g.itcd = h.itcd(+) and " + Environment.NewLine;
            if (itgrpcd != "") sql += "h.itgrpcd in (" + itgrpcd + ") and " + Environment.NewLine;
            if (itcd != "") sql += "g.itcd in (" + itcd + ") and " + Environment.NewLine;
            if (baleno != "") sql += "a.baleno||a.baleyr in (" + baleno + ") and " + Environment.NewLine;
            if (gocd != "") sql += "a.gocd in (" + gocd + ") and " + Environment.NewLine;
            if (pagenoslno.retStr() != "") sql += "g.pageno||'/'||g.pageslno in (" + pagenoslno + ") and " + Environment.NewLine;
            sql += "h.itgrpcd = i.itgrpcd(+) and a.gocd=k.gocd(+) " + Environment.NewLine;
            if (balStockOnly == true) sql += "and nvl(a.qnty, 0) > 0 " + Environment.NewLine;
            sql += "group by a.gocd, k.gonm, a.blautono, a.blslno, a.baleno, a.baleyr, a.baleno||a.baleyr, e.lrno, e.lrdt, g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, " + Environment.NewLine;
            sql += "g.nos, g.qnty, h.styleno||' '||h.itnm, g.pageno, g.pageslno, g.rate, g.txblval, g.othramt, f.prefno, f.prefdt,g.pageno||'/'||g.pageslno " + Environment.NewLine;
            sql += "order by baleyr, baleno, styleno " + Environment.NewLine;
            DataTable tbl = masterHelpFa.SQLquery(sql);
            return tbl;
        }
        public M_GROUP CreateGroup(string grpnm, string ITGRPTYPE, string BARGENTYPE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            M_GROUP MGROUP = new M_GROUP();
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            try
            {
                string DefaultAction = "A"; grpnm = grpnm.ToUpper();
                var tMGROU = DB.M_GROUP.Where(m => m.ITGRPNM == grpnm).FirstOrDefault();
                if (tMGROU != null)
                {
                    return tMGROU;
                }
                MGROUP.CLCD = CommVar.ClientCode(UNQSNO);
                MGROUP.EMD_NO = 0;
                MGROUP.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());

                string txtst = grpnm.Substring(0, 1).Trim().ToUpper();
                MGROUP.ITGRPNM = grpnm.ToUpper();
                string sql = " select max(SUBSTR(ITGRPCD, 2)) ITGRPCD FROM " + CommVar.CurSchema(UNQSNO) + ".M_GROUP";
                string sql1 = " select max(GRPBARCODE) GRPBARCODE FROM " + CommVar.CurSchema(UNQSNO) + ".M_GROUP";
                var tbl = masterHelpFa.SQLquery(sql);
                if (tbl.Rows[0]["ITGRPCD"].ToString() != "")
                {
                    MGROUP.ITGRPCD = txtst + ((tbl.Rows[0]["ITGRPCD"]).retInt() + 1).ToString("D3");
                }
                else
                {
                    MGROUP.ITGRPCD = txtst + (100).ToString("D3");
                }
                var tb1l = masterHelpFa.SQLquery(sql1);
                if (tb1l.Rows[0]["GRPBARCODE"].ToString() != "")
                {
                    MGROUP.GRPBARCODE = ((tb1l.Rows[0]["GRPBARCODE"]).retInt() + 1).ToString("D3");
                }
                else
                {
                    MGROUP.GRPBARCODE = (100).ToString("D3");
                }
                var fMGROU = DB.M_GROUP.FirstOrDefault();
                if (fMGROU == null) Cn.SaveTextFile("Add a row in the Group master");
                else
                {
                    MGROUP.SALGLCD = fMGROU.SALGLCD;
                    MGROUP.PURGLCD = fMGROU.PURGLCD;
                    MGROUP.NEGSTOCK = fMGROU.NEGSTOCK;
                }
                MGROUP.ITGRPTYPE = ITGRPTYPE == "" ? "F" : ITGRPTYPE;
                MGROUP.PRODGRPCD = "G001";
                MGROUP.BARGENTYPE = BARGENTYPE == "" ? "C" : BARGENTYPE;//c=common,e=entry

                OraCon.Open();
                OracleCommand OraCmd = OraCon.CreateCommand();
                using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_GROUP", MGROUP.M_AUTONO, DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                    var dbsql = masterHelpFa.RetModeltoSql(MCH, "A", CommVar.CurSchema(UNQSNO));
                    var dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelpFa.RetModeltoSql(MGROUP, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    OraTrans.Commit();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            OraCon.Dispose();
            return MGROUP;
        }
        public ItemDet CreateItem(string style, string UOM, string grpnm, string HSNCODE, string FABITCD, string BARNO, string ITGRPTYPE, string BARGENTYPE, string ITNM)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string DefaultAction = "A";
            ItemDet ItemDet = new ItemDet();
            M_SITEM MSITEM = new M_SITEM(); M_GROUP MGROUP = new M_GROUP();
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            try
            {
                OraCon.Open();
                MSITEM.CLCD = CommVar.ClientCode(UNQSNO);
                var STYLEdt = (from g in DB.M_SITEM
                               join h in DB.M_GROUP on g.ITGRPCD equals h.ITGRPCD
                               join i in DB.T_BATCHMST on g.ITCD equals i.ITCD
                               where (g.ITNM == ITNM && g.STYLENO == style) || i.BARNO == BARNO
                               select new
                               {
                                   ITCD = g.ITCD,
                                   PURGLCD = h.PURGLCD,
                                   BARNO = i.BARNO,
                               }).FirstOrDefault();
                if (STYLEdt != null)
                {
                    ItemDet.ITCD = STYLEdt.ITCD;
                    ItemDet.PURGLCD = STYLEdt.PURGLCD;
                    ItemDet.BARNO = STYLEdt.BARNO;
                    return ItemDet;
                }
                if (CommVar.NextYearSchema(UNQSNO) != "")
                {
                    return ItemDet;
                }
                MGROUP = CreateGroup(grpnm, ITGRPTYPE, BARGENTYPE);
                MSITEM.EMD_NO = 0;
                MSITEM.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                string sql = "select max(itcd)itcd from " + CommVar.CurSchema(UNQSNO) + ".m_sitem where itcd like('" + MGROUP.ITGRPTYPE + MGROUP.GRPBARCODE + "%') ";
                var tbl = masterHelpFa.SQLquery(sql);
                if (tbl.Rows[0]["itcd"].ToString() == "")
                {
                    MSITEM.ITCD = MGROUP.ITGRPTYPE + MGROUP.GRPBARCODE + "00001";
                }
                else
                {
                    string s = tbl.Rows[0]["itcd"].ToString();
                    string digits = new string(s.Where(char.IsDigit).ToArray());
                    string letters = new string(s.Where(char.IsLetter).ToArray());
                    int number;
                    if (!int.TryParse(digits, out number))                   //int.Parse would do the job since only digits are selected
                    {
                        Console.WriteLine("Something weired happened");
                    }
                    MSITEM.ITCD = letters + (++number).ToString("D7");
                }
                MSITEM.ITGRPCD = MGROUP.ITGRPCD;
                MSITEM.ITNM = ITNM;
                MSITEM.STYLENO = style.retStr().Trim();
                MSITEM.UOMCD = UOM;
                MSITEM.HSNCODE = HSNCODE;
                MSITEM.FABITCD = FABITCD;
                MSITEM.NEGSTOCK = MGROUP.NEGSTOCK;
                var MPRODGRP = DB.M_PRODGRP.FirstOrDefault();
                MSITEM.PRODGRPCD = MPRODGRP?.PRODGRPCD;

                T_BATCHMST TBATCHMST = new T_BATCHMST();
                TBATCHMST.EMD_NO = MSITEM.EMD_NO;
                TBATCHMST.CLCD = MSITEM.CLCD;
                TBATCHMST.DTAG = MSITEM.DTAG;
                TBATCHMST.TTAG = MSITEM.TTAG;
                TBATCHMST.COMMONUNIQBAR = BARGENTYPE == "" ? "C" : BARGENTYPE;  //C=COMMON  E=ENTRY/UNIQUE
                if (string.IsNullOrEmpty(BARNO))
                {
                    TBATCHMST.BARNO = GenerateBARNO(MSITEM.ITCD, "", "");
                }
                else
                {
                    TBATCHMST.BARNO = BARNO;
                }
                TBATCHMST.ITCD = MSITEM.ITCD;

                OracleCommand OraCmd = OraCon.CreateCommand();
                using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_SITEM", MSITEM.M_AUTONO, DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                    var dbsql = masterHelpFa.RetModeltoSql(MCH, "A", CommVar.CurSchema(UNQSNO));
                    var dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelpFa.RetModeltoSql(MSITEM, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    //dbsql = masterHelpFa.RetModeltoSql(TBATCHMST, "A", CommVar.CurSchema(UNQSNO));
                    //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelpFa.RetModeltoSql(TBATCHMST, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    OraTrans.Commit();
                }
                ItemDet.ITCD = MSITEM.ITCD;
                ItemDet.BARNO = TBATCHMST.BARNO;
                ItemDet.PURGLCD = MGROUP.PURGLCD;
                OraCon.Dispose();
                return ItemDet;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                OraCon.Dispose();
                return ItemDet;
            }
        }
        public string CreatePricelist(string BARNO, string EFFDT, double CPRate, double WPRate, double RPRate)
        {
            try
            {
                T_BATCHMST_PRICE MIP = new T_BATCHMST_PRICE();
                MIP.EMD_NO = 0;
                MIP.CLCD = CommVar.ClientCode(UNQSNO);
                MIP.EFFDT = Convert.ToDateTime(EFFDT);
                MIP.BARNO = BARNO;
                OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
                OraCon.Open();
                OracleCommand OraCmd = OraCon.CreateCommand();
                using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    MIP.PRCCD = "CP";
                    MIP.RATE = CPRate;
                    var dbsql = masterHelpFa.RetModeltoSql(MIP, "A", CommVar.CurSchema(UNQSNO));
                    var dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    MIP.PRCCD = "WP";
                    MIP.RATE = WPRate;
                    dbsql = masterHelpFa.RetModeltoSql(MIP, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    MIP.PRCCD = "RP";
                    MIP.RATE = RPRate;
                    dbsql = masterHelpFa.RetModeltoSql(MIP, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    OraTrans.Commit();
                }
                OraCon.Dispose();
                return "ok";
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, BARNO);
                return ex.Message;
            }
        }
        public DataTable GenStocktblwithVal(string calctype = "FIFO", string tdt = "", string barno = "", string mtrljobcd = "", string itgrpcd = "", string selitcd = "", string gocd = "", bool skipStkTrnf = true, string skipautono = "", bool summary = false, string unselitcd = "", string schema = "", string LOCCD = "")
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);
            string sql = "", sqlc = "";
            if (schema.retStr() != "") scm = schema;

            sql = "";
            sql += "select distinct y.mtrljobcd, e.mtrljobnm, z.barno, a.itcd, a.itnm, a.styleno,a.styleno||' '||a.itnm itstyle, z.pdesign, z.ourdesign, a.uomcd, a.itgrpcd, c.itgrpnm, d.uomnm, d.decimals, z.fabitcd, q.itnm fabitnm " + Environment.NewLine;
            sql += "from " + scm + ".m_sitem a, " + scm + ".m_group c, " + scmf + ".m_uom d, " + scm + ".m_sitem q, " + Environment.NewLine;
            sql += scm + ".t_batchdtl y, " + scm + ".t_batchmst z, " + scm + ".m_mtrljobmst e " + Environment.NewLine;
            sql += "where z.itcd=a.itcd(+) and z.barno=y.barno(+) and a.itgrpcd=c.itgrpcd(+) and z.fabitcd=q.itcd(+) and " + Environment.NewLine;
            if (mtrljobcd.retStr() != "") sql += "y.mtrljobcd in (" + mtrljobcd + ") and " + Environment.NewLine;
            if (itgrpcd.retStr() != "") sql += "c.itgrpcd in (" + itgrpcd + ") and " + Environment.NewLine;
            if (selitcd.retStr() != "") sql += "a.itcd in (" + selitcd + ") and " + Environment.NewLine;
            if (unselitcd.retStr() != "") sql += "a.itcd not in (" + unselitcd + ") and " + Environment.NewLine;
            if (barno.retStr() != "") sql += "z.barno in (" + barno + ") and " + Environment.NewLine;
            sql += "a.uomcd=d.uomcd(+) and y.mtrljobcd=e.mtrljobcd(+) " + Environment.NewLine;
            sql += "order by mtrljobnm, mtrljobcd, itgrpnm, itgrpcd, itnm, itcd, styleno, barno " + Environment.NewLine;
            DataTable rsitem = SQLquery(sql);

            string sqld = "", sqldgrp = "";
            if (gocd.retStr() != "") { sqld += "a.gocd, "; sqldgrp = ", a.gocd " + Environment.NewLine; }
            else
            {
                sqld += "'' gocd, "; sqldgrp = ", ''" + Environment.NewLine;
            }
            skipStkTrnf = false;
            if (gocd != "") skipStkTrnf = false;

            sqlc = "";
            sqlc += "where a.autono=b.autono and nvl(b.cancel,'N') = 'N' and a.autono=c.autono(+) and a.barno=h.barno(+) and " + Environment.NewLine;
            if (tdt.retStr() != "") sqlc += "b.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and " + Environment.NewLine;
            sqlc += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' and " + Environment.NewLine;
            if (LOCCD != "") { sqlc += " b.loccd in (" + LOCCD + ") and " + Environment.NewLine; } else { sqlc += " b.loccd='" + LOC + "' and " + Environment.NewLine; }
            if (skipautono.retStr() != "") sqlc += "a.autono not in (" + skipautono + ") and " + Environment.NewLine;
            if (gocd.retStr() != "") sqlc += "a.gocd in (" + gocd + ") and " + Environment.NewLine;
            if (selitcd.retStr() != "") sqlc += "d.itcd in (" + selitcd + ") and " + Environment.NewLine;
            if (unselitcd.retStr() != "") sqlc += "d.itcd not in (" + unselitcd + ") and " + Environment.NewLine;
            if (itgrpcd.retStr() != "") sqlc += "d.itgrpcd in (" + itgrpcd + ") and " + Environment.NewLine;
            if (barno.retStr() != "") sql += "a.barno in (" + barno + ") and " + Environment.NewLine;
            sqlc += "h.itcd=d.itcd(+) and d.itgrpcd=e.itgrpcd(+) " + Environment.NewLine;

            sql = "";
            sql += "select  a.autono,A.TXNSLNO, c.doctag, conslcd, c.slcd, g.slnm, b.doccd, b.docdt, b.docno, substr(nvl(c.prefno,b.docno),16) blno, nvl(c.prefdt,b.docdt) bldt, " + Environment.NewLine; //a.slno, a.autono||a.slno autoslno
            sql += "a.mtrljobcd, h.itcd, a.barno, h.pdesign, a.mtrljobcd||h.itcd||a.barno itbarno, a.rate,  " + Environment.NewLine; /*nvl(a.txblval,0) + nvl(a.mtrlcost,0) + nvl(a.othramt,0) netamt, "; */
            sql += sqld;
            sql += "nvl(f.txblval,0) + nvl(f.othramt,0) netamt, " + Environment.NewLine;
            sql += "sum(a.qnty) qnty, sum(a.nos)nos " + Environment.NewLine;//"a.qnty, a.nos ";
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c, " + scmf + ".m_subleg g, " + scm + ".t_batchmst h, ";
            sql += scm + ".m_sitem d, " + scm + ".m_group e,"+ scm + ".T_TXNDTL F  ";
            sql += sqlc ;
            if (skipStkTrnf == true) sql += "c.doctag not in ('SI','SO') and " + Environment.NewLine;
            sql += "and A.AUTONO = F.AUTONO(+) AND A.TXNSLNO = F.SLNO(+) AND c.doctag in ('PB','OP','PD','JR','SI','KH','TR') and c.slcd=g.slcd(+) and a.stkdrcr in ('D','C') " + Environment.NewLine;
            sql += " group by a.autono, A.TXNSLNO, c.doctag, conslcd, c.slcd, g.slnm, b.doccd, b.docdt, b.docno, " + Environment.NewLine;
            sql += "c.prefno,b.docno, c.prefdt,b.docdt,a.mtrljobcd, h.itcd, a.barno, h.pdesign,a.rate,nvl(f.txblval,0) + nvl(f.othramt,0) " + Environment.NewLine;
            if (gocd.retStr() != "") sql += ",a.gocd " + Environment.NewLine;
            sql += "order by itcd, docdt, autono " + Environment.NewLine; //slno
            if (calctype == "LIFO") sql += "desc " + Environment.NewLine;

            string str = "";
            str = "select  autono||TXNSLNO AUTONO, doctag, conslcd, doctag, slcd, slnm, doccd, docdt, docno, blno, bldt, gocd, " + Environment.NewLine;//slno, autoslno
            str += "mtrljobcd, itcd, barno, itbarno, pdesign, rate, netamt, itbarno||gocd itgocd, nvl(nos,0) nos, nvl(qnty,0) qnty from (" + Environment.NewLine;
            str += sql + ") " + Environment.NewLine;
            if (barno.retStr() != "") str += "where barno in (" + barno + ") " + Environment.NewLine;
            DataTable rsIn = SQLquery(str);

            sql = "";
            sql += "select a.mtrljobcd, a.barno, h.itcd, a.mtrljobcd||h.itcd||a.barno itbarno, " + Environment.NewLine;
            sql += sqld;
            sql += "sum(case a.stkdrcr when 'C' then a.nos else a.nos*-1 end) nos, " + Environment.NewLine;
            sql += "sum(case a.stkdrcr when 'C' then a.qnty else a.qnty*-1 end) qnty " + Environment.NewLine;
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c, " + scm + ".t_batchmst h, " + Environment.NewLine;
            sql += scm + ".m_sitem d, " + scm + ".m_group e " + Environment.NewLine;
            sql += sqlc + " and ";
            if (skipStkTrnf == true) sql += "c.doctag not in ('PB','OP','PD','JR') and "; else sql += "c.doctag not in ('PB','OP','PD','JR','SI','KH','TR') and " + Environment.NewLine;
            sql += "a.stkdrcr in ('D','C') " + Environment.NewLine;
            sql += "group by a.mtrljobcd, a.barno, h.itcd, a.mtrljobcd||h.itcd||a.barno " + Environment.NewLine;
            sql += sqldgrp;
            sql += "order by itcd " + Environment.NewLine;

            str = "select mtrljobcd, itcd, barno, gocd, itbarno||gocd itgocd, nvl(qnty,0) qnty from (" + Environment.NewLine;
            str += sql + " )";
            DataTable rsOut = SQLquery(str);

            var varGodown = rsIn.AsEnumerable().Union(rsOut.AsEnumerable()).OrderBy(d => d.Field<string>("GOCD")).Select(A => new
            {
                GOCD = A["GOCD"].ToString(),
            }).DistinctBy(s => s.GOCD).ToList().Select(q => new
            {
                GOCD = q.GOCD,
                GONM = (from W in DBF.M_GODOWN where W.GOCD == q.GOCD select W.GONM).SingleOrDefault()
            }).ToList();

            #region //Create Datatable rsstock
            DataTable rsStock = new DataTable("stock");
            //rsStock.Columns.Add("mtrljobcd", typeof(string), "");
            //rsStock.Columns.Add("mtrljobnm", typeof(string), "");
            //rsStock.Columns.Add("doctag", typeof(string), "");
            //rsStock.Columns.Add("conslcd", typeof(string), "");
            rsStock.Columns.Add("autono", typeof(string), "");
            //rsStock.Columns.Add("slno", typeof(string), "");
            //rsStock.Columns.Add("autoslno", typeof(string), "");
            //rsStock.Columns.Add("doccd", typeof(string), "");
            rsStock.Columns.Add("docno", typeof(string), "");
            rsStock.Columns.Add("docdt", typeof(string), "");
            //rsStock.Columns.Add("prefno", typeof(string), "");
            //rsStock.Columns.Add("prefdt", typeof(string), "");
            rsStock.Columns.Add("slcd", typeof(string), "");
            rsStock.Columns.Add("slnm", typeof(string), "");
            rsStock.Columns.Add("itcd", typeof(string), "");
            //rsStock.Columns.Add("itnm", typeof(string), "");
            //rsStock.Columns.Add("barno", typeof(string), "");
            //rsStock.Columns.Add("styleno", typeof(string), "");
            //rsStock.Columns.Add("pdesign", typeof(string), "");
            //rsStock.Columns.Add("nos", typeof(double), "");
            rsStock.Columns.Add("balqnty", typeof(double), "");
            //rsStock.Columns.Add("basrate", typeof(double), "");
            rsStock.Columns.Add("rate", typeof(double), "");
            //rsStock.Columns.Add("amt", typeof(double), "");
            rsStock.Columns.Add("itgrpcd", typeof(string), "");
            rsStock.Columns.Add("itgrpnm", typeof(string), "");
            //rsStock.Columns.Add("fabitcd", typeof(string), "");
            //rsStock.Columns.Add("fabitnm", typeof(string), "");
            rsStock.Columns.Add("uomcd", typeof(string), "");
            rsStock.Columns.Add("uomnm", typeof(string), "");
            //rsStock.Columns.Add("decimals", typeof(double), "");
            rsStock.Columns.Add("gocd", typeof(string), "");
            rsStock.Columns.Add("gonm", typeof(string), "");
            rsStock.Columns.Add("itstyle", typeof(string), "");
            #endregion

            double balqty = 0, outqty = 0, avrate = 0, stkamt = 0;
            string strmtrljobcd = "", strbarno = "", stritcd = "", strgocd = "";
            Int32 i = 0, rNo = 0, it = 0, ig = 0;
            int maxR = rsitem.Rows.Count - 1;
            while (it <= rsitem.Rows.Count - 1)
            {
                strbarno = rsitem.Rows[it]["barno"].ToString();
                stritcd = rsitem.Rows[it]["itcd"].ToString();
                strmtrljobcd = rsitem.Rows[it]["mtrljobcd"].ToString();
                ig = 0;
                double isqty = 0, isamt = 0, israte = 0;
                while (ig <= varGodown.Count - 1)
                {
                    strgocd = varGodown[ig].GOCD;
                    string data = "itgocd = '" + strmtrljobcd + stritcd + strbarno + strgocd + "'";
                    var var1 = rsIn.Select(data);
                    bool itrecofound = false;
                    isqty = 0; isamt = 0; israte = 0;
                    DataTable tbl1 = new DataTable();
                    if (var1 != null && var1.Count() > 0)
                    {
                        tbl1 = var1.CopyToDataTable();
                        itrecofound = true;
                    }

                    outqty = 0;
                    var tbl2 = rsOut.Select(data);
                    if (tbl2 != null)
                    {
                        if (tbl2.Count() != 0) outqty = Convert.ToDouble(tbl2[0]["qnty"]);
                    }
                    if (outqty != 0) itrecofound = true;

                    if (itrecofound == true)
                    {
                        balqty = outqty;
                        bool recoins = true;
                        maxR = tbl1.Rows.Count - 1;
                        i = 0;
                        while (i <= maxR)
                        {
                            double chkqty = Math.Round(Convert.ToDouble(tbl1.Rows[i]["qnty"]), 6);
                            balqty = Math.Round(balqty, 6);
                            if (chkqty > balqty)
                            {
                                chkqty = chkqty - balqty;
                                balqty = 0;
                                recoins = true;
                            }
                            else
                            {
                                balqty = balqty - chkqty; recoins = false;
                            }
                            if (recoins == true)
                            {
                                isqty = isqty + chkqty;
                                double amtcal = Convert.ToDouble(tbl1.Rows[i]["netamt"]);
                                avrate = (amtcal / Convert.ToDouble(tbl1.Rows[i]["qnty"])).toRound(6);
                                stkamt = (chkqty * avrate).toRound();
                                isamt = isamt + stkamt;
                                if (summary == false)
                                {
                                    rsStock.Rows.Add(""); rNo = rsStock.Rows.Count - 1;
                                    rsStock.Rows[rNo]["autono"] = tbl1.Rows[i]["autono"];
                                    //rsStock.Rows[rNo]["slno"] = tbl1.Rows[i]["slno"];
                                    //rsStock.Rows[rNo]["autoslno"] = tbl1.Rows[i]["autoslno"];
                                    //rsStock.Rows[rNo]["doccd"] = tbl1.Rows[i]["doccd"];
                                    rsStock.Rows[rNo]["docno"] = tbl1.Rows[i]["docno"];
                                    rsStock.Rows[rNo]["docdt"] = tbl1.Rows[i]["docdt"];
                                    //rsStock.Rows[rNo]["prefno"] = tbl1.Rows[i]["blno"];
                                    //rsStock.Rows[rNo]["prefdt"] = tbl1.Rows[i]["bldt"];
                                    //rsStock.Rows[rNo]["doctag"] = tbl1.Rows[i]["doctag"];
                                    //rsStock.Rows[rNo]["conslcd"] = tbl1.Rows[i]["conslcd"];
                                    rsStock.Rows[rNo]["slcd"] = tbl1.Rows[i]["slcd"];
                                    rsStock.Rows[rNo]["slnm"] = tbl1.Rows[i]["slnm"];
                                    //rsStock.Rows[rNo]["basrate"] = tbl1.Rows[i]["rate"];
                                    rsStock.Rows[rNo]["balqnty"] = chkqty;
                                    rsStock.Rows[rNo]["rate"] = avrate;
                                    //rsStock.Rows[rNo]["amt"] = stkamt;
                                    rsStock.Rows[rNo]["itcd"] = stritcd;
                                    //rsStock.Rows[rNo]["itnm"] = rsitem.Rows[it]["itnm"];
                                    rsStock.Rows[rNo]["itgrpcd"] = rsitem.Rows[it]["itgrpcd"];
                                    rsStock.Rows[rNo]["itgrpnm"] = rsitem.Rows[it]["itgrpnm"];
                                    rsStock.Rows[rNo]["uomcd"] = rsitem.Rows[it]["uomcd"];
                                    //rsStock.Rows[rNo]["uomnm"] = rsitem.Rows[it]["uomnm"];
                                    //rsStock.Rows[rNo]["decimals"] = rsitem.Rows[it]["decimals"];
                                    rsStock.Rows[rNo]["gocd"] = strgocd;
                                    rsStock.Rows[rNo]["gonm"] = varGodown[ig].GONM;

                                    //rsStock.Rows[rNo]["barno"] = rsitem.Rows[it]["barno"];
                                    //rsStock.Rows[rNo]["mtrljobcd"] = rsitem.Rows[it]["mtrljobcd"];
                                    //rsStock.Rows[rNo]["mtrljobnm"] = rsitem.Rows[it]["mtrljobnm"];
                                    //rsStock.Rows[rNo]["styleno"] = rsitem.Rows[it]["styleno"];
                                    rsStock.Rows[rNo]["itstyle"] = rsitem.Rows[it]["itstyle"];
                                    //rsStock.Rows[rNo]["fabitcd"] = rsitem.Rows[it]["fabitcd"];
                                    //rsStock.Rows[rNo]["fabitnm"] = rsitem.Rows[it]["fabitnm"];
                                    //rsStock.Rows[rNo]["pdesign"] = (rsitem.Rows[it]["ourdesign"].retStr() == "" ? "" : rsitem.Rows[it]["ourdesign"].retStr() + "/") + rsitem.Rows[it]["pdesign"].retStr();
                                }
                            }
                            i++;
                        }
                        if (balqty != 0)
                        {
                            isqty = isqty - balqty;
                            if (summary == false)
                            {
                                rsStock.Rows.Add(""); rNo = rsStock.Rows.Count - 1;
                                //rsStock.Rows[rNo]["slno"] = 1;
                                rsStock.Rows[rNo]["itcd"] = stritcd;
                                //rsStock.Rows[rNo]["itnm"] = rsitem.Rows[it]["itnm"]; ;
                                rsStock.Rows[rNo]["balqnty"] = balqty * -1;
                                rsStock.Rows[rNo]["rate"] = 0;
                                //rsStock.Rows[rNo]["amt"] = 0;
                                rsStock.Rows[rNo]["itgrpcd"] = rsitem.Rows[it]["itgrpcd"];
                                rsStock.Rows[rNo]["itgrpnm"] = rsitem.Rows[it]["itgrpnm"];
                                rsStock.Rows[rNo]["uomcd"] = rsitem.Rows[it]["uomcd"];
                                //rsStock.Rows[rNo]["uomnm"] = rsitem.Rows[it]["uomnm"];
                                //rsStock.Rows[rNo]["decimals"] = rsitem.Rows[it]["decimals"];
                                rsStock.Rows[rNo]["gocd"] = strgocd;
                                rsStock.Rows[rNo]["gonm"] = varGodown[ig].GONM;

                                //rsStock.Rows[rNo]["barno"] = rsitem.Rows[it]["barno"];
                                //rsStock.Rows[rNo]["mtrljobcd"] = rsitem.Rows[it]["mtrljobcd"];
                                //rsStock.Rows[rNo]["mtrljobnm"] = rsitem.Rows[it]["mtrljobnm"];
                                //rsStock.Rows[rNo]["styleno"] = rsitem.Rows[it]["styleno"];
                                rsStock.Rows[rNo]["itstyle"] = rsitem.Rows[it]["itstyle"];
                                //rsStock.Rows[rNo]["fabitcd"] = rsitem.Rows[it]["fabitcd"];
                                //rsStock.Rows[rNo]["fabitnm"] = rsitem.Rows[it]["fabitnm"];
                                //rsStock.Rows[rNo]["pdesign"] = (rsitem.Rows[it]["ourdesign"].retStr() == "" ? "" : rsitem.Rows[it]["ourdesign"].retStr() + "/") + rsitem.Rows[it]["pdesign"].retStr();
                            }
                        }
                        if (summary == true)
                        {
                            rsStock.Rows.Add(""); rNo = rsStock.Rows.Count - 1;
                            //rsStock.Rows[rNo]["slno"] = 1;
                            rsStock.Rows[rNo]["itcd"] = stritcd;
                            //rsStock.Rows[rNo]["itnm"] = rsitem.Rows[it]["itnm"]; ;
                            rsStock.Rows[rNo]["balqnty"] = isqty;
                            rsStock.Rows[rNo]["rate"] = (isqty == 0 ? 0 : (isamt / isqty).toRound(4));
                            //rsStock.Rows[rNo]["amt"] = isamt;
                            rsStock.Rows[rNo]["itgrpcd"] = rsitem.Rows[it]["itgrpcd"];
                            rsStock.Rows[rNo]["itgrpnm"] = rsitem.Rows[it]["itgrpnm"];
                            rsStock.Rows[rNo]["uomcd"] = rsitem.Rows[it]["uomcd"];
                            //rsStock.Rows[rNo]["uomnm"] = rsitem.Rows[it]["uomnm"];
                            //rsStock.Rows[rNo]["decimals"] = rsitem.Rows[it]["decimals"];
                            rsStock.Rows[rNo]["gocd"] = strgocd;
                            rsStock.Rows[rNo]["gonm"] = varGodown[ig].GONM;

                            //rsStock.Rows[rNo]["barno"] = rsitem.Rows[it]["barno"];
                            //rsStock.Rows[rNo]["mtrljobcd"] = rsitem.Rows[it]["mtrljobcd"];
                            //rsStock.Rows[rNo]["mtrljobnm"] = rsitem.Rows[it]["mtrljobnm"];
                            //rsStock.Rows[rNo]["styleno"] = rsitem.Rows[it]["styleno"];
                            rsStock.Rows[rNo]["itstyle"] = rsitem.Rows[it]["itstyle"];
                            //rsStock.Rows[rNo]["fabitcd"] = rsitem.Rows[it]["fabitcd"];
                            //rsStock.Rows[rNo]["fabitnm"] = rsitem.Rows[it]["fabitnm"];
                            //rsStock.Rows[rNo]["pdesign"] = (rsitem.Rows[it]["ourdesign"].retStr() == "" ? "" : rsitem.Rows[it]["ourdesign"].retStr() + "/") + rsitem.Rows[it]["pdesign"].retStr();
                        }
                    }
                    ig++;
                }
                it++;
            }
            return rsStock;
        }
        
        public M_SYSCNFG M_SYSCNFG(string effdt = "")
        {
            try
            {
                var dtconfig = (System.Data.DataTable)System.Web.HttpContext.Current.Session["M_SYSCNFG"];
                if (effdt.retStr() != "")
                {
                    var dt = (from DataRow dr in dtconfig.Rows
                              orderby dr["effdt"] descending
                              where Convert.ToDateTime(dr["effdt"]) <= Convert.ToDateTime(effdt)
                              select dr);
                    if (dt != null && dt.Count() > 0)
                    {
                        dtconfig = dt.CopyToDataTable();
                    }
                    else
                    {
                        dtconfig = dtconfig.Clone();
                    }
                }

                M_SYSCNFG M_SYSCG = dtconfig.DataTableToListConvertion<M_SYSCNFG>().OrderByDescending(a => a.EFFDT).First();
                return M_SYSCG;
            }
            catch
            {
                return new M_SYSCNFG();
            }
        }
        public DataTable GetSyscnfgData(string EFFDT)
        {
            //string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);
            //string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);
            //string sql = "";
            //sql += " select a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate,C.TAXGRPCD,a.retdebslcd,b.city,b.add1,b.add2,b.add3, a.effdt, d.prccd, e.prcnm,a.wppricegen,a.rppricegen,a.wpper, a.rpper, a.priceincode, ";
            //sql += "a.COMPCD,a.SALDEBGLCD,a.PURDEBGLCD,a.CLASS1CD,a.DEALSIN,a.INSPOLDESC,a.BLTERMS,a.DUEDATECALCON,a.BANKSLNO,a.PRICEINCODECOST,a.DESIGNPATH,a.MNTNSIZE,a.MNTNPART,a.MNTNCOLOR, ";
            //sql += "a.MNTNFLAGMTR,a.MNTNLISTPRICE,a.MNTNDISC1,a.MNTNDISC2,a.MNTNSHADE,a.MNTNWPRPPER,a.MNTNOURDESIGN,a.MNTNBALE,a.MNTNPCSTYPE,a.MNTNBARNO,a.COMMONUNIQBAR,a.CMROFFTYPE,a.SHORTAGE_GLCD, ";
            //sql += "a.CMCASHRECDAUTO,a.MERGEINDTL ";
            //sql += "  from  " + scm + ".M_SYSCNFG a, " + scmf + ".M_RETDEB b, " + scm + ".M_SUBLEG_SDDTL c, " + scm + ".m_subleg_com d, " + scmf + ".m_prclst e ";
            //sql += " where a.RTDEBCD=b.RTDEBCD and a.effdt in(select max(effdt) effdt from  " + scm + ".M_SYSCNFG  where effdt<=to_date('" + EFFDT + "','dd/mm/yyyy') ) and a.retdebslcd=d.slcd(+) and ";
            //sql += "a.retdebslcd=C.SLCD(+) and c.compcd='" + COM + "' and c.loccd='" + LOC + "' and d.prccd=e.prccd(+) ";

            //DataTable syscnfgdt = masterHelpFa.SQLquery(sql);

            var syscnfgdt = (System.Data.DataTable)System.Web.HttpContext.Current.Session["M_SYSCNFG"];
            var dt = (from DataRow dr in syscnfgdt.Rows
                      orderby dr["effdt"] descending
                      where Convert.ToDateTime(dr["effdt"]) <= Convert.ToDateTime(EFFDT)
                      select dr).Take(1);
            if (dt != null && dt.Count() > 0)
            {
                syscnfgdt = dt.CopyToDataTable();
            }
            else
            {
                syscnfgdt = syscnfgdt.Clone();
            }
            return syscnfgdt;

        }
        public string CreateSizeMaster(string SIZENM)
        {
            try
            {
                string sizecd = "";
                SIZENM = SIZENM.retStr().ToUpper().Trim();
                sizecd = SIZENM.Trim(' ');
                if (sizecd.Length > 4) sizecd = sizecd.Substring(0, 4);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                var tMGROU = DB.M_SIZE.Where(m => m.SIZECD == sizecd).FirstOrDefault();
                if (tMGROU == null)
                {

                    OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
                    OraCon.Open();
                    OracleCommand OraCmd = OraCon.CreateCommand();
                    using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        M_SIZE MSIZE = new M_SIZE();
                        MSIZE.CLCD = CommVar.ClientCode(UNQSNO);
                        MSIZE.EMD_NO = 0;
                        MSIZE.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                        MSIZE.SZBARCODE = Cn.GenMasterCode("M_SIZE", "SZBARCODE", "", 2);
                        MSIZE.SIZECD = sizecd;
                        MSIZE.SIZENM = SIZENM;
                        MSIZE.ALTSIZENM = "";
                        MSIZE.PRINT_SEQ = "0";

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_SIZE", MSIZE.M_AUTONO, "A", CommVar.CurSchema(UNQSNO).ToString());
                        string dbsql = masterHelpFa.RetModeltoSql(MCH, "A", CommVar.CurSchema(UNQSNO));
                        var dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = masterHelpFa.RetModeltoSql(MSIZE, "A", CommVar.CurSchema(UNQSNO));
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        OraTrans.Commit();
                    }
                    OraCon.Dispose();
                }
                return sizecd;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return "";
            }
        }
    }
}