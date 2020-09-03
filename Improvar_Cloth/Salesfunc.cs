using System;
using System.Collections.Generic;
using System.Linq;
using Improvar.Models;
using System.Data;

namespace Improvar
{
    public class Salesfunc : MasterHelpFa
    {
        Connection Cn = new Connection();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public DataTable GetSlcdDetails(string slcd, string docdt, string linkcd = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (docdt == null) docdt = "";

            string sql = "";
            string itgrpcd = "";

            sql += "select z.slcd, b.taxgrpcd, a.agslcd, a.areacd, a.prccd, a.discrtcd, a.crdays, a.crlimit, a.cod, a.gstno, a.docth, b.trslcd, b.courcd, nvl(c.agslcd,a.agslcd) agslcd, ";
            sql += "g.slnm, g.slarea, h.slnm agslnm, i.slnm trslnm, e.taxgrpnm, f.prcnm, ";
            //sql += "f.prcnm, "; // c.prcdesc, c.effdt, c.itmprccd, ";
            sql += "nvl(a.crdays,0) crdays, nvl(a.crlimit,0) crlimit ";
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

            //sql += "(select a.effdt, a.prccd, a.itmprccd, a.prcdesc from ";
            //sql += "(select a.effdt, a.prccd, a.itmprccd, a.prcdesc, ";
            //sql += "row_number() over (partition by a.prccd order by a.effdt desc) as rn ";
            //sql += "from " + scm + ".m_itemplist a ";
            //sql += "where a.itgrpcd='" + itgrpcd + "' ";
            //if (docdt != "") sql += "and a.effdt <= to_date('" + docdt.Substring(0, 10) + "','dd/mm/yyyy') ";
            //sql += ") a where a.rn=1) d, ";

            sql += "" + scmf + ".m_taxgrp e, " + scmf + ".m_prclst f, " + scmf + ".m_subleg g, " + scmf + ".m_subleg h, " + scmf + ".m_subleg i, " + scmf + ".m_subleg j ";
            sql += "where z.slcd=a.slcd(+) and z.slcd=b.slcd(+) and z.slcd=c.slcd(+) and ";
            sql += "b.taxgrpcd=e.taxgrpcd(+) and a.prccd=f.prccd(+) and ";
            sql += "z.slcd=g.slcd(+) and a.agslcd=h.slcd(+) and b.trslcd=i.slcd(+) and b.courcd=j.slcd(+) ";

            tbl = SQLquery(sql);

            return tbl;
        }


        public string GetPendOrderSql(string slcd = "", string ordupto = "", string ordautono = "", string txnupto = "", string skipautono = "", string menupara = "SB", string brandcd = "", bool OnlyBal = true, string ordfromdt = "", string itcd = "", string agslcd = "", string slmslcd = "", bool Showasperpslip = false, string itgrpcd = "", string curschema = "", string finschema = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (curschema != "") scm = curschema;
            if (finschema != "") scmf = finschema;

            string doctype = "SORD";
            if (menupara == "SB") doctype = "SORD"; else doctype = "PORD";

            if (ordupto == "") ordupto = txnupto;
            if (ordupto != "") ordupto = ordupto.retDateStr();
            if (skipautono == null) skipautono = "";
            if (slcd == null) slcd = "";
            if (slcd != "") { if (slcd.IndexOf("'") < 0) slcd = "'" + slcd + "'"; }

            string sqlc = "";
            sqlc += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' and ";
            if (slcd != "") sqlc += "c.slcd in (" + slcd + ") and ";
            if (skipautono != "") sqlc += "a.autono <> '" + skipautono + "' and ";
            sqlc += "nvl(b.cancel,'N')='N' ";

            string sql = "";

            sql += "select a.autono, a.slcd, j.slnm, j.district, j.slarea, a.doccd, a.docno, a.docdt, nvl(a.stktype,'') stktype, nvl(a.freestk,'') freestk, nvl(a.rate,0) rate, ";
            sql += "a.agslcd, a.slmslcd, k.slnm agslnm, l.slnm slmslnm, ";
            sql += "d.styleno, d.mixsize, a.itcd, a.sizecd, a.colrcd, d.itnm, nvl(d.pcsperbox,0) pcsperbox, nvl(d.pcsperset,0) pcsperset, nvl(d.colrperset,0) colrperset, ";
            sql += "d.uomcd, g.uomnm, g.decimals, d.itgrpcd, h.itgrpnm, h.brandcd, i.brandnm, ";
            sql += "(select sizecdgrp from " + scm + ".v_msitem_sizegrp where itcd = a.itcd and sizecdgrp like '%^' || a.sizecd || '^%') sizecdgrp, ";
            sql += "e.sizenm, e.print_seq, f.colrnm, nvl(a.ordqnty,0) ordqnty, ";
            sql += "nvl(a.ordqnty, 0) - nvl(b.qnty, 0) - nvl(z.qnty,0) balaspslip, ";
            sql += "nvl(a.ordqnty,0) - nvl(b.qnty,0) -nvl(c.qnty,0) balqnty from ";

            sql += "( select a.autono, c.agslcd, c.slmslcd, c.slcd, b.doccd, b.docno, b.docdt, a.stktype, a.freestk, a.rate, ";
            sql += "nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize, ";
            sql += "a.itcd, a.sizecd, a.colrcd, sum(a.qnty) ordqnty ";
            sql += "from " + scm + ".t_sorddtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_sord c, " + scm + ".m_doctype d ";
            sql += "where a.autono=b.autono and a.autono=c.autono and b.doccd=d.doccd and d.doctype = '" + doctype + "' and ";
            if (ordautono != "") sql += "a.autono in (" + ordautono + ") and ";
            if (ordfromdt.retStr() != "") sql += "b.docdt >= to_date('" + ordfromdt + "','dd/mm/yyyy') and ";
            if (ordupto != "") sql += "b.docdt <= to_date('" + ordupto + "','dd/mm/yyyy') and ";
            if (agslcd != "") sql += "c.agslcd in (" + agslcd + ") and ";
            if (slmslcd != "") sql += "c.slmslcd in (" + slmslcd + ") and ";
            if (itcd != "") sql += "a.itcd in (" + itcd + ") and ";
            sql += sqlc;
            sql += "group by a.autono, c.agslcd, c.slmslcd, c.slcd, b.doccd, b.docno, b.docdt, a.stktype, a.freestk, a.rate, ";
            sql += "nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,''), ";
            sql += "a.itcd, a.sizecd, a.colrcd ) a, ";

            //Ord Canc
            sql += "( select a.ordautono, nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize, ";
            sql += "sum(case a.stkdrcr when 'C' then a.qnty when 'D' then a.qnty*-1 end) qnty ";
            sql += "from " + scm + ".t_sorddtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_sord_canc c, " + scm + ".m_doctype d ";
            sql += "where a.autono=b.autono and a.autono=c.autono and b.doccd=d.doccd and d.doctype <> '" + doctype + "' and ";
            if (OnlyBal == false) sql += "a.autono='xx' and ";
            if (txnupto != "") sql += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
            sql += sqlc;
            sql += "group by a.ordautono, nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') ) b, ";

            //DO
            sql += "( select a.ordautono, nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize, ";
            sql += "sum(case a.stkdrcr when 'C' then a.qnty when 'D' then a.qnty*-1 end) qnty ";
            sql += "from " + scm + ".t_dodtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_do c ";
            sql += "where a.autono=b.autono and a.autono=c.autono and ";
            if (OnlyBal == false) sql += "a.autono='xx' and ";
            if (txnupto != "") sql += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
            sql += sqlc;
            sql += "group by a.ordautono, nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') ) c, ";

            //Pslip
            sql += "( select a.ordautono, nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize, ";
            sql += "sum(case a.stkdrcr when 'C' then a.qnty when 'D' then a.qnty*-1 end) qnty ";
            sql += "from " + scm + ".t_pslipdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_pslip c ";
            sql += "where a.autono=b.autono and a.autono=c.autono and ";
            if (OnlyBal == false) sql += "a.autono='xx' and ";
            if (txnupto != "") sql += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
            sql += sqlc;
            sql += "group by a.ordautono, nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') ) z, ";

            sql += scm + ".m_sitem d, " + scm + ".m_size e, " + scm + ".m_color f, " + scmf + ".m_uom g, " + scm + ".m_group h, " + scm + ".m_brand i, " + scmf + ".m_subleg j, ";
            sql += scmf + ".m_subleg k, " + scmf + ".m_subleg l ";
            sql += "where a.autono=b.ordautono(+) and a.autono=c.ordautono(+) and a.autono=z.ordautono(+) and ";
            sql += "a.itcolsize=b.itcolsize(+) and a.itcolsize=c.itcolsize(+) and a.itcolsize=z.itcolsize(+) and ";
            sql += "a.itcd=d.itcd(+) and a.sizecd=e.sizecd(+) and a.colrcd=f.colrcd(+) and d.uomcd=g.uomcd(+) and d.itgrpcd=h.itgrpcd(+) and h.brandcd=i.brandcd(+) and a.slcd=j.slcd(+) and ";
            sql += "a.agslcd=k.slcd(+) and a.slmslcd=l.slcd(+) and ";
            if (brandcd != "") sql += "h.brandcd in (" + brandcd + ") and ";
            if (itgrpcd != "") sql += "d.itgrpcd in (" + itgrpcd + ") and ";
            if (Showasperpslip == true) sql += "nvl(a.ordqnty,0) - nvl(b.qnty,0) - nvl(z.qnty,0) <> 0 ";
            else sql += "nvl(a.ordqnty,0) - nvl(b.qnty,0) - nvl(c.qnty,0) <> 0 ";
            sql += "order by styleno, print_seq, sizenm";
            return sql;
        }
        public DataTable GetPendOrder(string slcd = "", string ordupto = "", string ordautono = "", string txnupto = "", string skipautono = "", string menupara = "SB", string brandcd = "", bool OnlyBal = true, string ordfromdt = "", string itcd = "", string agslcd = "", string slmslcd = "", bool Showasperpslip = false, string itgrpcd = "", string curschema = "", string finschema = "")
        {
            string sql = GetPendOrderSql(slcd, ordupto, ordautono, txnupto, skipautono, menupara, brandcd, OnlyBal, ordfromdt, itcd, agslcd, slmslcd, Showasperpslip, itgrpcd, curschema, finschema);
            DataTable tbl = new DataTable();
            tbl = SQLquery(sql);
            return tbl;
        }
        public DataTable GetPendDO(string slcd = "", string doupto = "", string doautono = "", string ordautono = "", string txnupto = "", string skipautono = "", string brandcd = "", bool OnlyBal = true)
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

            string doctype = "SDO";
            //string prccd = "EXFI", effdt = "23/05/2019";
            if (doupto == "") doupto = txnupto;

            string sqlc = "";
            sqlc += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' and ";
            if (slcd != null && slcd != "") sqlc += "c.slcd='" + slcd + "' and ";
            if (skipautono.retStr() != "") sqlc += "a.autono <> '" + skipautono + "' and ";
            sqlc += "nvl(b.cancel,'N')='N' ";

            string sql = "";

            sql += "select a.autono, a.ordautono, a.doccd, a.docno, a.docdt, a.ordno, a.orddt, nvl(a.stktype,'') stktype, nvl(a.freestk,'') freestk, ";
            //sql += "nvl(z.addper,0) addper, nvl(nvl(z.rate,y.rate),0) rate, nvl(y.rate,0) plistrate, ";
            sql += "nvl(a.addper,0) addper, nvl(y.rate,0) plistrate, ";
            sql += "(case when nvl(z.rate,0)=0 and nvl(a.addper,0) <> 0 then nvl(y.rate,0)+round((nvl(y.rate,0)*nvl(a.addper,0))/100,2) when nvl(z.rate,0)=0 and nvl(a.addper,0) = 0 then nvl(y.rate,0)+round((nvl(y.rate,0)*nvl(a.addper,0))/100,2) else nvl(nvl(z.rate,y.rate),0) end) rate, ";
            sql += "d.styleno, a.itcd, a.sizecd, a.colrcd, d.itnm, nvl(d.pcsperbox,0) pcsperbox, nvl(d.pcsperset,0) pcsperset, nvl(d.colrperset,0) colrperset, ";
            sql += "d.uomcd, g.uomnm, g.decimals, d.itgrpcd, h.itgrpnm, h.brandcd, i.brandnm, a.prccd, a.prceffdt, ";
            sql += "e.sizenm, e.print_seq, f.colrnm, nvl(a.doqnty,0) doqnty, ";
            sql += "nvl(a.doqnty,0) - nvl(b.qnty,0) -nvl(c.qnty,0) balqnty from ";
            sql += "( select a.autono, a.ordautono, b.doccd, b.docno, b.docdt, e.docno ordno, e.docdt orddt, a.stktype, a.freestk, a.rate, f.prccd, f.prceffdt, ";
            sql += "nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize, ";
            sql += "f.prccd||f.prceffdt||nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') plistitcolsize, ";
            sql += "a.itcd, a.sizecd, a.colrcd, nvl(g.addper,0) addper, sum(a.qnty) doqnty ";
            sql += "from " + scm + ".t_dodtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_do c, " + scm + ".m_doctype d, ";
            sql += scm + ".t_cntrl_hdr e, " + scm + ".t_sord f, " + scm + ".t_sorddtl_app g ";
            sql += "where a.autono=b.autono and a.autono=c.autono and b.doccd=d.doccd and d.doctype = '" + doctype + "' and ";
            sql += "a.ordautono=e.autono(+) and a.ordautono=f.autono(+) and a.ordautono=g.autono(+) and ";
            if (doautono != "") sql += "a.autono in (" + doautono + ") and ";
            if (ordautono != "") sql += "a.ordautono in (" + ordautono + ") and ";
            if (doupto != "") sql += "b.docdt <= to_date('" + doupto + "','dd/mm/yyyy') and ";
            sql += sqlc;
            sql += "group by a.autono, a.ordautono, b.doccd, b.docno, b.docdt, e.docno, e.docdt, a.stktype, a.freestk, a.rate, f.prccd, f.prceffdt, ";
            sql += "nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,''), ";
            sql += "f.prccd||f.prceffdt||nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,''), ";
            sql += "a.itcd, a.sizecd, a.colrcd, nvl(g.addper,0) ) a, ";

            sql += "( select a.doautono, a.ordautono, nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize, ";
            sql += "sum(case a.stkdrcr when 'C' then a.qnty when 'D' then a.qnty*-1 end) qnty ";
            sql += "from " + scm + ".t_dodtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_do_canc c, " + scm + ".m_doctype d ";
            sql += "where a.autono=b.autono and a.autono=c.autono and b.doccd=d.doccd and d.doctype <> '" + doctype + "' and ";
            if (OnlyBal == false) sql += "a.autono='xx' and ";
            if (txnupto != "") sql += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
            sql += sqlc;
            sql += "group by a.doautono, a.ordautono, nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') ) b, ";

            sql += "( select a.doautono, a.ordautono, nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize, ";
            sql += "sum(case a.stkdrcr when 'C' then a.qnty when 'D' then a.qnty*-1 end) qnty ";
            sql += "from " + scm + ".t_pslipdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_pslip c ";
            sql += "where a.autono=b.autono and a.autono=c.autono and ";
            if (OnlyBal == false) sql += "a.autono='xx' and ";
            if (txnupto != "") sql += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
            sql += sqlc;
            sql += "group by a.doautono, a.ordautono, nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') ) c, ";

            sql += "(select a.autono ordautono, nvl(a.stktype,'F')||'N'||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize, a.rate ";
            sql += "from " + scm + ".t_sorddtl_appdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_sord c, " + scm + ".t_sorddtl_app d ";
            sql += "where a.autono=b.autono and a.autono=c.autono and a.autono=d.autono(+) and ";
            sql += sqlc;
            sql += " ) z, ";

            sql += "(select a.prccd, a.effdt, a.itcd, a.sizecd, a.colrcd, a.rate, ";
            sql += "a.prccd||a.effdt||'F'||'N'||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') plistitcolsize ";
            sql += "from " + scm + ".m_itemplistdtl a ) y, "; // where a.prccd = '" +prccd + " and a.effdt = to_date('" + effdt + "', 'dd/mm/yyyy') ) y, ";

            sql += scm + ".m_sitem d, " + scm + ".m_size e, " + scm + ".m_color f, " + scmf + ".m_uom g, " + scm + ".m_group h, " + scm + ".m_brand i ";
            sql += "where a.autono=b.doautono(+) and a.autono=c.doautono(+) and a.plistitcolsize=y.plistitcolsize(+) and ";
            sql += "a.ordautono=b.ordautono(+) and a.ordautono=c.ordautono(+) and a.ordautono=z.ordautono(+) and ";
            sql += "a.itcolsize=b.itcolsize(+) and a.itcolsize=c.itcolsize(+) and a.itcolsize=z.itcolsize(+) and ";
            sql += "a.itcd=d.itcd(+) and a.sizecd=e.sizecd(+) and a.colrcd=f.colrcd(+) and d.uomcd=g.uomcd(+) and d.itgrpcd=h.itgrpcd(+) and h.brandcd=i.brandcd(+) and ";
            if (brandcd != "") sql += "h.brandcd in (" + brandcd + ") and ";
            sql += "nvl(a.doqnty,0) - nvl(b.qnty,0) -nvl(c.qnty,0) <> 0 ";
            sql += "order by styleno, print_seq, sizenm";
            tbl = SQLquery(sql);

            return tbl;
        }
        public DataTable GetPendPackSlip(string slcd = "", string pslipupto = "", string pslipautono = "", string ordautono = "", string txnupto = "", string skipautono = "", string brandcd = "", bool OnlyBal = true)
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

            string doctype = "SPSLP";
            brandcd = brandcd.retStr();

            if (pslipupto == "") pslipupto = txnupto;

            string sqlc = "";
            sqlc += "b.compcd='" + COM + "' and b.loccd='" + LOC + "' and ";
            if (slcd != null && slcd != "") sqlc += "c.slcd='" + slcd + "' and ";
            if (skipautono.retStr() != "") sqlc += "a.autono <> '" + skipautono + "' and ";
            sqlc += "nvl(b.cancel,'N')='N' ";

            string sql = "";

            sql += "select a.autono, a.doautono, a.ordautono, a.doccd, a.docno, a.docdt, a.ordno, a.orddt, nvl(a.stktype,'') stktype, nvl(a.freestk,'') freestk, nvl(a.rate,0) rate, ";
            sql += "d.styleno, a.itcd, a.sizecd, a.colrcd, d.itnm, nvl(d.pcsperbox,0) pcsperbox, nvl(d.pcsperset,0) pcsperset, nvl(d.colrperset,0) colrperset, j.prccd, j.prceffdt, k.prcnm, ";
            sql += "d.uomcd, g.uomnm, d.itgrpcd, h.itgrpnm, h.brandcd, i.brandnm, ";
            sql += "e.sizenm, e.print_seq, f.colrnm, nvl(a.pslipqnty,0) pslipqnty, ";
            sql += "nvl(a.pslipqnty,0) balqnty from ";
            sql += "( select a.autono, a.doautono, a.ordautono, b.doccd, b.docno, b.docdt, e.docno ordno, e.docdt orddt, a.stktype, a.freestk, a.rate, ";
            sql += "nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize, ";
            sql += "a.itcd, a.sizecd, a.colrcd, sum(a.qnty) pslipqnty ";
            sql += "from " + scm + ".t_pslipdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_pslip c, " + scm + ".m_doctype d, " + scm + ".t_cntrl_hdr e ";
            sql += "where a.autono=b.autono and a.autono=c.autono and b.doccd=d.doccd and d.doctype = '" + doctype + "' and a.ordautono=e.autono(+) and ";
            if (pslipautono != "") sql += "a.autono in (" + pslipautono + ") and ";
            if (ordautono != "") sql += "a.ordautono in (" + ordautono + ") and ";
            if (pslipupto != "") sql += "b.docdt <= to_date('" + pslipupto + "','dd/mm/yyyy') and ";
            if (OnlyBal == true)
            {
                sql += "a.autono not in (";
                sql += "select a.linkautono from " + scm + ".t_txn_linkno a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c ";
                sql += "where a.autono=b.autono and a.autono=c.autono(+) and ";
                sql += sqlc;
                sql += " ) and ";
            }
            sql += sqlc;
            sql += "group by a.autono, a.doautono, a.ordautono, b.doccd, b.docno, b.docdt, e.docno, e.docdt, a.stktype, a.freestk, a.rate, ";
            sql += "nvl(a.stktype,'F')||nvl(a.freestk,'N')||a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,''), ";
            sql += "a.itcd, a.sizecd, a.colrcd ) a, ";

            sql += scm + ".m_sitem d, " + scm + ".m_size e, " + scm + ".m_color f, " + scmf + ".m_uom g, " + scm + ".m_group h, " + scm + ".m_brand i, " + scm + ".t_sord j, " + scmf + ".m_prclst k ";
            sql += "where a.itcd=d.itcd(+) and a.sizecd=e.sizecd(+) and a.colrcd=f.colrcd(+) and d.uomcd=g.uomcd(+) and d.itgrpcd=h.itgrpcd(+) and h.brandcd=i.brandcd(+) and ";
            if (brandcd != "") sql += "h.brandcd in (" + brandcd + ") and ";
            sql += "nvl(a.pslipqnty,0) <> 0 and a.ordautono=j.autono(+) and j.prccd=k.prccd(+) ";
            sql += "order by styleno, print_seq, sizenm";
            tbl = SQLquery(sql);

            return tbl;
        }
        public string retCompLogo()
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string complogo = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + ".png";
            if (!System.IO.File.Exists(complogo)) complogo = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + ".jpg";
            return complogo;
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
            if (jobcd == "CT")
            {
                sql += "select a.autono, b.itcd, b.partcd, f.styleno, f.itnm, f.hsnsaccd, f.uomcd, f.pcsperbox, g.partnm, c.slcd, e.slnm, ";
                if (shortallowadj == false) sql += "b.qnty, nvl(b.shortqnty,0) shortqnty, 0 short_allow, ";
                else sql += "b.qnty qnty, nvl(b.shortqnty,0) shortqnty, 0 short_allow, ";
                sql += "d.docno, d.docdt, c.prefno, c.prefdt, b.progautono, b.progautono issautono, h.docno issdocno, h.docdt issdocdt from ";

                sql += "(select a.autono ";
                sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b ";
                sql += "where a.autono=b.autono and b.compcd='" + COM + "' and ";
                sql += "b.docdt <= to_date('" + chlnpupto + "','dd/mm/yyyy') and a.doctag in ('JR') and ";
                if (jobcd.retStr() != "") sql += "a.jobcd ='" + jobcd + "' and ";
                if (slcd.retStr() != "") sql += "a.slcd in (" + slcd + ") and ";
                if (skipautono.retStr() != "") sql += "a.autono <> '" + skipautono + "' and ";
                sql += "nvl(b.cancel,'N')='N' ) a, ";

                sql += "(select b.autono, b.autono progautono, b.itcd, b.partcd, b.autono||b.itcd||nvl(b.partcd,'') progitcd, sum(b.qnty) qnty, sum(b.shortqnty) shortqnty ";
                sql += "from " + scm1 + ".t_txndtl b, " + scm1 + ".m_sitem c, " + scm1 + ".m_group d ";
                sql += "where b.itcd=c.itcd(+) and c.itgrpcd=d.itgrpcd(+) and d.itgrptype in ('F') ";
                sql += "group by b.autono, b.autono, b.itcd, b.partcd, b.autono||b.itcd||nvl(b.partcd,'') ) b, ";

                sql += "" + scm1 + ".t_txn c, " + scm1 + ".t_cntrl_hdr d, " + scmf + ".m_subleg e, " + scm1 + ".m_sitem f, " + scm1 + ".m_parts g, " + scm1 + ".t_cntrl_hdr h ";
                sql += "where a.autono=b.autono(+) and b.progautono=h.autono(+) and b.qnty <> 0 and ";
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
            }
            else
            {
                sql += "select a.autono, b.itcd, b.partcd, f.styleno, f.itnm, f.hsnsaccd, f.uomcd, f.pcsperbox, g.partnm, c.slcd, e.slnm, ";
                if (shortallowadj == false) sql += "b.qnty, nvl(b.shortqnty,0) shortqnty, nvl(y.short_allow,0) short_allow, ";
                else sql += "b.qnty+nvl(y.short_allow,0) qnty, nvl(b.shortqnty,0)-nvl(y.short_allow,0) shortqnty, nvl(y.short_allow,0) short_allow, ";
                sql += "d.docno, d.docdt, c.prefno, c.prefdt, b.progautono, b.progautono issautono, h.docno issdocno, h.docdt issdocdt from ";

                sql += "(select a.autono ";
                sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b ";
                sql += "where a.autono=b.autono and b.compcd='" + COM + "' and ";
                sql += "b.docdt <= to_date('" + chlnpupto + "','dd/mm/yyyy') and a.doctag in ('JR') and ";
                if (jobcd.retStr() != "") sql += "a.jobcd ='" + jobcd + "' and ";
                if (slcd.retStr() != "") sql += "a.slcd in (" + slcd + ") and ";
                if (skipautono.retStr() != "") sql += "a.autono <> '" + skipautono + "' and ";
                sql += "nvl(b.cancel,'N')='N' ) a, ";

                sql += "(select a.autono, a.progautono, b.itcd, b.partcd, a.progautono||b.itcd||nvl(b.partcd,'') progitcd, sum(b.qnty) qnty, sum(b.shortqnty) shortqnty ";
                sql += "from " + scm1 + ".t_progdtl a, " + scm1 + ".t_txndtl b ";
                sql += "where a.autono=b.recprogautono(+) and a.slno=b.recprogslno(+) ";
                sql += "group by a.autono, a.progautono, b.itcd, b.partcd, a.progautono||b.itcd||nvl(b.partcd,'') ) b, ";

                sql += "(select a.recautono, a.progautono, b.itcd, b.partcd, a.progautono||b.itcd||nvl(b.partcd,'') progitcd, sum(a.short_allow) short_allow ";
                sql += "from " + scm1 + ".t_prog_close a, " + scm1 + ".t_progmast b where a.progautono=b.autono(+) and a.progslno=b.slno(+) and a.recautono is not null ";
                sql += "group by a.recautono, a.progautono, b.itcd, b.partcd, a.progautono||b.itcd||nvl(b.partcd,'') ) y, ";

                sql += "(select a.linkautono, max(a.autono) autono ";
                sql += "from " + scm1 + ".t_txn_linkno a, " + scm1 + ".t_cntrl_hdr b ";
                sql += "where a.autono=b.autono and ";
                if (txnupto.retStr() != "") sql += "b.docdt <= to_date('" + txnupto + "',dd/mm/yyyy') and ";
                if (blautono.retStr() != "") sql += "a.autono = '" + blautono + "' and ";
                sql += "nvl(b.cancel,'N')='N' group by a.linkautono ) z, ";

                sql += "" + scm1 + ".t_txn c, " + scm1 + ".t_cntrl_hdr d, " + scmf + ".m_subleg e, " + scm1 + ".m_sitem f, " + scm1 + ".m_parts g, " + scm1 + ".t_cntrl_hdr h ";
                sql += "where a.autono=b.autono(+) and b.progautono=h.autono(+) and ";
                sql += "a.autono=c.autono and a.autono=d.autono and c.slcd=e.slcd(+) and ";
                sql += "b.itcd is not null and ";
                if (fdt != "") sql += "d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and ";
                if (tdt != "") sql += "d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
                sql += "b.itcd=f.itcd(+) and b.partcd=g.partcd(+) and b.autono=y.recautono(+) and "; // and b.progitcd=y.progitcd(+) ";
                sql += "a.autono=z.linkautono(+) and ";
                if (blautono.retStr() == "") sql += "z.autono is null "; else sql += "z.autono is not null ";
                sql += "order by docdt, docno ";
            }

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
        public DataTable getSizeData(string itcd, string prccd = "", string prceffdt = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string scm1 = CommVar.CurSchema(UNQSNO);
            string sql = "";
            sql += "select a.itcd, a.styleno, a.pcsperbox, a.pcsperset, a.sizecd, a.sizenm, a.mixsize, a.print_seq, a.colrcd, a.colrnm, a.slno, ";
            if (prccd.retStr() != "") sql += "nvl(b.rate,0) rate "; else sql += "0 rate ";
            sql += "from ";
            sql += "( select a.itcd, a.styleno, a.pcsperbox, a.pcsperset, b.sizecd, d.sizenm, a.mixsize, d.print_seq, c.colrcd, c.slno, e.colrnm ";
            sql += "from " + scm1 + ".m_sitem a, " + scm1 + ".m_sitem_size b, " + scm1 + ".m_sitem_color c, ";
            sql += scm1 + ".m_size d, " + scm1 + ".m_color e, " + scm1 + ".m_cntrl_hdr f ";
            sql += "where a.itcd = '" + itcd + "' and a.itcd = b.itcd(+) and a.itcd = c.itcd(+) and a.m_autono=f.m_autono(+) and nvl(f.inactive_tag,'N')='N' and ";
            sql += "b.sizecd = d.sizecd(+) and c.colrcd = e.colrcd(+) and ";
            sql += "nvl(b.inactive_tag, 'N')= 'N' and nvl(c.inactive_tag, 'N')= 'N' ) a ";
            sql += "where a.sizecd is not null ";
            if (prccd.retStr() != "")
            {
                sql += ", ( select a.itcd, a.sizecd, a.rate ";
                sql += "from " + scm1 + ".m_itemplistdtl a ";
                sql += "where a.prccd='" + prccd + "' and ";
                if (prceffdt.retStr() != "") sql += "a.effdt=to_date('" + prceffdt + "','dd/mm/yyyy') and ";
                sql += "a.itcd='" + itcd + "' ) b ";
                sql += "and a.itcd=b.itcd(+) and a.sizecd=b.sizecd(+) ";
            }
            sql += "order by print_seq, slno ";
            DataTable tbl = SQLquery(sql);
            return tbl;
        }
        public DataTable getPendProg(string tdt, string txnupto = "", string slcd = "", string itcd = "", string jobcd = "", string skipautono = "", string progfromdt = "", string brandcd = "", string itgrpcd = "", string linecd = "", string curschema = "", string finschema = "", string stktype = "", string recslcd = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (curschema != "") scm = curschema;
            if (finschema != "") scmf = finschema;

            string sql = "";
            sql += "select a.autono, a.autono progautono, a.slno progslno, d.jobcd, l.jobnm, l.jobseq, d.batchno, a.docno, a.docdt, a.doconlyno, a.doccd, ";
            sql += "d.slcd, d.linecd, e.slnm, e.propname, e.regmobile, k.linenm, f.itgrpcd, m.itgrpnm, m.brandcd, n.brandnm, ";
            sql += "d.slcd||nvl(d.linecd,'') repslcd, e.slnm||decode(k.linenm,null,'',' ['||k.linenm||']') repslnm, ";
            sql += "d.itcd||nvl(d.partcd,'') repitcd, nvl(f.styleno,f.itnm)||decode(h.partnm,null,'',' ['||h.partnm||']') repitnm, ";
            sql += "d.itcd, d.stktype, f.itnm, f.styleno, nvl(f.pcsperbox,0) pcsperbox, nvl(f.pcsperset,0) pcsperset, f.uomcd, g.uomnm, d.partcd, h.partnm, d.sizecd, i.sizenm, i.print_seq, ";
            sql += "d.dia, d.ll, d.gsm, d.texture, d.millnm, d.colrnm, d.mchnname, d.fabtype, nvl(d.cutlength,0) cutlength, nvl(d.stkqnty,0) stkqnty, d.qntyin, d.proguniqno, ";
            sql += "nvl(o.prefno,r.docno) recdocno, r.docdt recdocdt, decode(nvl(f.mergepcs,0),0,1,f.mergepcs) mergepcs, ";
            sql += "(select sizecdgrp from " + scm + ".v_msitem_sizegrp where itcd = d.itcd and sizecdgrp like '%^' || d.sizecd || '^%') sizecdgrp, ";
            sql += "o.slcd||nvl(o.linecd,'') recslcd, p.slnm||decode(q.linenm,null,'',' ['||q.linenm||']') recslnm, ";
            sql += "d.recautono, d.orgbatchautono, d.orgbatchslno, d.gauge, d.itremark, a.qnty, a.nos, ";
            sql += "nvl(b.drnos,0) drnos, nvl(b.crnos,0) crnos,  nvl(b.drnos,0)-nvl(b.crnos,0) balnos, ";
            sql += "nvl(b.drqnty,0) drqnty, nvl(b.crqnty,0) crqnty, nvl(c.short_qnty,0) short_qnty, nvl(c.short_allow,0) short_allow, ";
            sql += "nvl(b.drqnty,0) - nvl(b.crqnty,0) - nvl(b.tshortqnty,0) - nvl(c.short_allow,0) balqnty from ";

            sql += "(select a.autono, a.slno, a.qnty, a.nos, b.docno, b.docdt, b.doconlyno, b.doccd ";
            sql += "from " + scm + ".t_progmast a, " + scm + ".t_cntrl_hdr b ";
            sql += "where a.autono=b.autono and b.compcd='" + COM + "' and b.loccd='" + LOC + "' and ";
            if (jobcd.retStr() != "") sql += "a.jobcd in (" + jobcd + ") and ";
            if (progfromdt.retStr() != "") sql += "b.docdt >= to_date('" + progfromdt + "', 'dd/mm/yyyy') and ";
            if (tdt != "") sql += "b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and ";
            sql += "nvl(b.cancel,'N')='N' ) a, ";

            sql += "(select a.progautono, a.progslno, sum(nvl(c.shortqnty,0)) tshortqnty, ";
            sql += "sum(decode(a.stkdrcr,'D',nvl(c.nos,a.nos),0)) drnos, sum(decode(a.stkdrcr,'C',nvl(c.nos,a.nos),0)) crnos, ";
            sql += "sum(decode(a.stkdrcr,'D',nvl(c.qnty,a.qnty),0)) drqnty, sum(decode(a.stkdrcr,'C',nvl(c.qnty,a.qnty),0)) crqnty ";
            sql += "from " + scm + ".t_progdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txndtl c ";
            sql += "where a.autono=b.autono and a.autono=c.recprogautono(+) and a.slno=c.recprogslno(+) and ";
            if (skipautono.retStr() != "") sql += "a.autono <> '" + skipautono + "' and ";
            if (txnupto.retStr() != "") sql += "b.docdt <= to_date('" + txnupto + "', 'dd/mm/yyyy') and ";
            sql += "nvl(b.cancel,'N')='N' ";
            sql += "group by a.progautono, a.progslno) b, ";

            sql += "( select a.progautono, a.progslno, ";
            sql += "sum(a.short_qnty) short_qnty, sum(a.short_allow) short_allow ";
            sql += "from " + scm + ".t_prog_close a, " + scm + ".t_cntrl_hdr b ";
            sql += "where a.autono=b.autono and a.recautono is null and ";
            if (skipautono.retStr() != "") sql += "a.autono <> '" + skipautono + "' and ";
            if (txnupto.retStr() != "") sql += "b.docdt <= to_date('" + txnupto + "', 'dd/mm/yyyy') and ";
            sql += "nvl(b.cancel,'N')='N' ";
            sql += "group by a.progautono, a.progslno ) c, ";

            sql += "" + scm + ".t_progmast d, " + scmf + ".m_subleg e, " + scm + ".m_sitem f, " + scmf + ".m_uom g, ";
            sql += "" + scm + ".m_parts h, " + scm + ".m_size i, " + scm + ".m_color j, " + scm + ".m_linemast k, " + scm + ".m_jobmst l, ";
            sql += scm + ".m_group m, " + scm + ".m_brand n," + scm + ".t_txn o, " + scmf + ".m_subleg p, " + scm + ".m_linemast q, " + scm + ".t_cntrl_hdr r ";
            sql += "where a.autono=b.progautono(+) and a.slno=b.progslno(+) and ";
            sql += "a.autono=c.progautono(+) and a.slno=c.progslno(+) and d.recautono=o.autono(+) and o.slcd=p.slcd(+) and o.linecd=q.linecd(+) and d.recautono=r.autono(+) and ";
            if (slcd.retStr() != "") sql += "d.slcd in (" + slcd + ") and ";
            if (linecd.retStr() != "") sql += "d.linecd in (" + linecd + ") and ";
            if (itcd.retStr() != "") sql += "d.itcd in (" + itcd + ") and ";
            if (itgrpcd.retStr() != "") sql += "f.itgrpcd in (" + itgrpcd + ") and ";
            if (brandcd.retStr() != "") sql += "m.brandcd in (" + brandcd + ") and ";
            if (stktype.retStr() != "") sql += "d.stktype in (" + stktype + ") and ";
            if (recslcd.retStr() != "") sql += "o.slcd in (" + recslcd + ") and ";
            sql += "a.autono=d.autono(+) and a.slno=d.slno(+) and d.slcd=e.slcd(+) and d.itcd=f.itcd(+) and  f.uomcd=g.uomcd(+) and ";
            sql += "d.partcd=h.partcd(+) and d.sizecd=i.sizecd(+) and d.colrcd=j.colrcd(+) and d.linecd=k.linecd(+) and d.jobcd=l.jobcd(+) and ";
            sql += "f.itgrpcd=m.itgrpcd(+) and m.brandcd=n.brandcd(+) and ";
            sql += "nvl(b.drqnty,0) - nvl(b.crqnty,0) - nvl(b.tshortqnty,0) - nvl(c.short_qnty,0) - nvl(c.short_allow,0) > 0 ";
            sql += "order by styleno, itnm, itcd, partnm, partcd, print_seq, sizenm ";
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

            //sql += "select a.itcd, c.styleno, c.itnm, c.mixsize, c.pcsperbox, c.pcsperset, c.itgrpcd, d.itgrpnm, e.print_seq, b.sizegrp, b.sizecdgrp from ";
            //sql += "(select a.itcd ";
            //sql += "from " + scm + ".m_sitem a ) a, ";

            //sql += "(select a.itcd, nvl(a.sizegrp,'A0') sizegrp, ";
            //sql += "listagg(a.sizecd,',') within group (order by a.itcd,b.print_seq) as sizecdgrp ";
            //sql += "from " + scm + ".m_sitem_box a, " + scm + ".m_size b, " + scm + ".m_sitem c ";
            //sql += "where a.sizecd=b.sizecd(+) and a.itcd=c.itcd and c.mixsize='M' ";
            //sql += "group by a.itcd, nvl(a.sizegrp,'A0') ";
            //sql += "union all ";
            //sql += "select a.itcd, 'A0' sizegrp, ";
            //sql += "listagg(a.sizecd,',') within group (order by a.itcd,b.print_seq) as sizecdgrp ";
            //sql += "from " + scm + ".m_sitem_size a, " + scm + ".m_size b, " + scm + ".m_sitem c ";
            //sql += "where a.sizecd=b.sizecd(+) and nvl(a.inactive_tag,'N')='N' and a.itcd=c.itcd and c.mixsize='M' and ";
            //sql += "a.itcd not in (select itcd from " + scm + ".m_sitem_box) ";
            //sql += "group by a.itcd,'A0' ";
            //sql += "union all ";
            //sql += "select a.itcd, a.sizecd sizegrp, ";
            //sql += "listagg(a.sizecd,',') within group (order by a.itcd,b.print_seq) as sizecdgrp ";
            //sql += "from " + scm + ".m_sitem_size a, " + scm + ".m_size b, " + scm + ".m_sitem c ";
            //sql += "where a.sizecd=b.sizecd(+) and nvl(a.inactive_tag,'N')='N' and a.itcd=c.itcd and c.mixsize='S' ";
            //sql += "group by a.itcd,a.sizecd ";

            //sql += "order by itcd) b, ";
            //sql += "" + scm + ".m_sitem c, " + scm + ".m_group d, " + scm + ".m_size e ";
            //sql += "where a.itcd=b.itcd(+) and a.itcd=c.itcd and b.sizegrp=e.sizecd(+) and ";
            //if (selitcd != "") sql += "a.itcd in (" + selitcd + ") and ";
            //if (selitgrpcd != "") sql += "a.itgrpcd in (" + selitgrpcd + ") and ";
            //sql += "c.itgrpcd=d.itgrpcd(+) "; // c.mixsize='M' ";
            //sql += "order by itgrpcd,e.print_seq ";
            DataTable tbl = MasterHelpFa.SQLquery(sql);
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
            DataTable fifotbl = MasterHelpFa.SQLquery(sqld);
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
            DataTable fiforec = MasterHelpFa.SQLquery(sqld);
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
        public DataTable GetStock(string tdt, string gocd = "", string barno = "", string itcd = "", string mtrljobcd = "'FS'", string skipautono = "", string itgrpcd = "", string stylelike = "", string prccd = "WP", string taxgrpcd = "C001", string stktype = "", string brandcd = "", bool pendpslipconsider = true, bool shownilstock = false, string curschema = "", string finschema = "", bool mergeitem = false, bool mergeloca = false)
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

            sql += "select a.gocd, a.mtrljobcd, a.stktype, a.barno, a.itcd, a.partcd, a.colrcd, a.sizecd, a.shade, a.cutlength, a.dia, ";
            sql += "c.slcd, g.slnm, h.docdt, h.docno, b.prccd, b.effdt, b.rate, e.bargentype, ";
            sql += "d.itnm, d.styleno, d.itgrpcd, e.itgrpnm, f.colrnm, e.prodgrpcd, z.prodgrpgstper, y.barimage, ";
            sql += "(case e.bargentype when 'E' then nvl(c.hsncode,nvl(d.hsncode,e.hsncode)) else nvl(d.hsncode,e.hsncode) end) hsncode, ";
            sql += "i.mtrljobcd, d.uomcd, ''STKNAME, ''PARTNM,'' PDESIGN, FLAGMTR, 0 GSTPER, ";
            sql += "c.locabin ";
            sql += "from ";
            sql += "( ";
            sql += "select gocd, mtrljobcd, stktype, barno, itcd, partcd, colrcd, sizecd, shade, cutlength, dia, ";
            sql += "sum(balqnty) balqnty, sum(balnos) balnos from ";
            sql += "( ";
            sql += "select a.gocd, a.mtrljobcd, b.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia, ";
            sql += "sum(case a.stkdrcr when 'D' then a.qnty when 'C' then a.qnty*-1 end) balqnty, ";
            sql += "sum(case a.stkdrcr when 'D' then a.nos when 'C' then a.nos*-1 end) balnos ";
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_cntrl_hdr c ";
            sql += "where a.barno=b.barno(+) and a.autono=c.autono(+) and ";
            sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and a.stkdrcr in ('D','C') and ";
            if (gocd.retStr() != "") sql += "a.gocd in (" + gocd + ") and ";
            if (barno.retStr() != "") sql += "a.barno in (" + barno + ") and ";
            if (itcd.retStr() != "") sql += "b.itcd in (" + itcd + ") and ";
            if (skipautono.retStr() != "") sql += "a.autono not in ('" + skipautono + ") and ";
            sql += "a.mtrljobcd in (" + mtrljobcd + ") and ";
            sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
            sql += "group by a.gocd, a.mtrljobcd, b.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia ";
            if (pendpslipconsider == true)
            {
                sql += "union all ";
                sql += "select a.gocd, a.mtrljobcd, b.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia, ";
                sql += "sum(a.qnty*-1) balqnty, sum(a.nos*-1) balnos ";
                sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_cntrl_hdr c, ";
                sql += "" + scm + ".m_doctype d, " + scm + ".t_txn_linkno e ";
                sql += "where a.barno=b.barno(+) and a.autono=c.autono(+) and ";
                sql += "c.doccd=d.doccd(+) and d.doctype in ('SPSLP') and a.autono=e.linkautono(+) and e.autono is null and ";
                sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and a.stkdrcr in ('D','C') and ";
                if (gocd.retStr() != "") sql += "a.gocd in (" + gocd + ") and ";
                if (barno.retStr() != "") sql += "a.barno in (" + barno + ") and ";
                if (itcd.retStr() != "") sql += "b.itcd in (" + itcd + ") and ";
                if (skipautono.retStr() != "") sql += "a.autono not in ('" + skipautono + ") and ";
                sql += "a.mtrljobcd in (" + mtrljobcd + ") and ";
                sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
                sql += "group by a.gocd, a.mtrljobcd, b.stktype, a.barno, b.itcd, a.partcd, b.colrcd, b.sizecd, b.shade, b.cutlength, b.dia ";
            }
            sql += ") ";
            if (shownilstock == false) sql += "where nvl(balqnty,0) <> 0 ";
            sql += "group by gocd, mtrljobcd, stktype, barno, itcd, partcd, colrcd, sizecd, shade,cutlength,dia ";
            sql += ") a, ";

            sql += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from ";
            sql += "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from ";
            sql += "(select a.barno, a.prccd, a.effdt, ";
            sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".m_itemplistdtl a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
            sql += ") a, " + scm + ".m_itemplistdtl b, " + scm + ".m_sitem_barcode c ";
            sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.barno=c.barno(+) and a.rn=1 ";
            sql += "union all ";
            sql += "select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from ";
            sql += "(select a.barno, a.prccd, a.effdt, ";
            sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".t_batchmst_price a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + tdt + "','dd/mm/yyyy') ) ";
            sql += "a, " + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c,  " + scm + ".m_sitem_barcode d ";
            sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and ";
            sql += "a.barno=c.barno(+) and a.barno=d.barno(+) and d.barno is null ";
            sql += ") a where prccd='" + prccd + "') b, ";

            sql += "(select a.barno, ";
            sql += "listagg(a.imgbarno||chr(181)||a.imgslno||chr(181)||a.doc_flname||chr(181)||a.doc_extn||chr(181)||substr(a.doc_desc,50),chr(179)) ";
            sql += "within group (order by a.barno) as barimage from ";
            sql += "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
            sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
            sql += "from " + scm + ".m_batch_img_hdr a ";
            sql += "union ";
            sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
            sql += "from " + scm + ".m_batch_img_hdr_link a, " + scm + ".m_batch_img_hdr b ";
            sql += "where a.mainbarno=b.barno(+) ) a, ";
            sql += "" + scm + ".m_batch_img_hdr b ";
            sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ";
            sql += "union ";
            sql += "select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
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
            sql += "listagg(b.fromrt||chr(181)||b.tort||chr(181)||b.igstper||chr(181)||b.cgstper||chr(181)||b.sgstper,chr(179)) ";
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
            sql += scm + ".m_mtrljobcd i ";
            sql += "where a.barno=c.barno(+) and a.barno=b.barno(+) and e.prodgrpcd=z.prodgrpcd(+) and a.barno=y.barno(+) and ";
            sql += "a.itcd=d.itcd(+) and d.itgrpcd=e.itgrpcd(+) and ";
            if (stylelike.retStr() != "") sql += "d.styleno like '%" + stylelike + "%' and ";
            if (itgrpcd.retStr() != "") sql += "d.itgrpcd in (" + itgrpcd + ") and ";
            if (brandcd.retStr() != "") sql += "d.brandcd in (" + brandcd + ") and ";
            sql += "a.colrcd=f.colrcd(+) and c.autono=h.autono(+) ";
            sql += "a.mtrljobcd=i.mtrljobcd(+) ";
            tbl = MasterHelpFa.SQLquery(sql);
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
            sql += "nvl(b.igstper, 0) + nvl(b.igstper, 0) + nvl(b.sgstper, 0) gstper from ";
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
        public string retGstPer(string SearchStr, double rate = 0)
        {
            //Searchstr value like listagg(b.fromrt||chr(181)||b.tort||chr(181)||b.igstper||chr(181)||b.cgstper||chr(181)||b.sgstper,chr(179))
            double fromrt = 0, tort = 0; int selrow = -1;
            string[] mgstrate = new string[5];
            string rtval = "0,0,0"; //igstper,cgst,sgst
            char SP = ((char)179);

            string[] mrates = SearchStr.Split(Convert.ToChar(SP)).ToArray();
            for (int x = 0; x <= mrates.Count() - 1; x++)
            {
                mgstrate = mrates[x].Split(Convert.ToChar(Cn.GCS())).ToArray();
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
            sql = "select a.docbarcode from " + scm + ".m_doctype_bar a where a.doccd='" + doccd + "' where docbarcode is not null";
            DataTable tbl = SQLquery(sql);
            if (tbl.Rows.Count == 1) dcdbarcode = tbl.Rows[0]["docbarcode"].retStr();

            if (autono != "")
            {
                sql = "select b.doccd, max(a.uniqno) uniqno ";
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
                unqno = tbl.Rows[0]["uniqno"].retStr();
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



    }
}