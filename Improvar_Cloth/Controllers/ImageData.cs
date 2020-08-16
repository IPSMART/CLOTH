using System;
using System.Linq;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Controllers
{
    public class ImageData
    {
        Connection Cn = new Connection();
        DataTable Imgdt = new DataTable("imgdata");
        string CS = "";
        Improvar.Models.ImageData Img = new Models.ImageData();
        public void GetImageDataStarts()
        {
            Imgdt.Columns.Add("autono", typeof(string));
            Imgdt.Columns.Add("doc_ctg", typeof(string));
            Imgdt.Columns.Add("slno", typeof(long));
            Imgdt.Columns.Add("doc_flname", typeof(string));
            Imgdt.Columns.Add("doc_desc", typeof(string));
            Imgdt.Columns.Add("doc_extn", typeof(string));
            Imgdt.Columns.Add("doc_string", typeof(string));

        }
        public void GetImageData(string Doc_Ctg, string schema = "C", string tblname = "", string mautono = "", string tautono = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            schema = schema.ToUpper();
            string scm;

            switch (schema)
            {
                case "S":
                    scm = CommVar.SaleSchema(UNQSNO) ; break;
                case "F":
                    scm = CommVar.FinSchema(UNQSNO); ; break;
                case "I":
                    scm = CommVar.InvSchema(UNQSNO); ; break;
                case "P":
                    scm = CommVar.PaySchema(UNQSNO); ; break;
                default:
                    scm = CommVar.CurSchema(UNQSNO).ToString(); ; break;
            }

            string mquery = "";
            tblname = tblname.ToUpper();

            if (tblname != "")
            {
                if (tblname.Substring(0, 1) != "'") tblname = "'" + tblname + "'";
            }

            mquery = "select to_char(a.m_autono) m_autono, a.doc_ctg, a.slno, a.doc_flname, a.doc_desc, a.doc_extn, b.doc_string ";
            mquery = mquery + "from " + scm + ".m_cntrl_hdr_doc a, " + scm + ".m_cntrl_hdr_doc_dtl b ";
            mquery = mquery + "where a.m_autono=b.m_autono(+) and a.slno=b.slno and ";
            if (mautono != "")
            { mquery = mquery + "to_char(a.m_autono) in (" + mautono + ") and "; }
            if (tblname != "")
            { mquery = mquery + "a.m_tblnm in (" + tblname + ") and "; }
            mquery = mquery + "a.doc_ctg in (" + Doc_Ctg + ") ";
            mquery = mquery + "order by m_autono, a.slno, b.rslno";

            DataTable tmpdata = new DataTable("tmpdata");
            CS = Cn.GetConnectionString();
            Cn.con = new OracleConnection(CS);
            if ((Cn.ds.Tables["tmpdata"] == null) == false)
            {
                Cn.ds.Tables["tmpdata"].Clear();
            }
            if (Cn.con.State == ConnectionState.Closed)
            {
                Cn.con.Open();
            }
            Cn.com = new OracleCommand(mquery, Cn.con);
            Cn.da.SelectCommand = Cn.com;
            bool bu = Convert.ToBoolean(Cn.da.Fill(Cn.ds, "tmpdata"));
            Cn.con.Close();

            int maxR = Cn.ds.Tables["tmpdata"].Rows.Count - 1;
            int i = 0; long j = 0;
            string docs = "";
            string auton; long sln;

            while (i <= maxR)
            {
                auton = Cn.ds.Tables["tmpdata"].Rows[i]["m_autono"].ToString();
                sln = Convert.ToInt32(Cn.ds.Tables["tmpdata"].Rows[i]["slno"]);
                docs = "";

                while (Cn.ds.Tables["tmpdata"].Rows[i]["m_autono"].ToString() == auton && Convert.ToInt32(Cn.ds.Tables["tmpdata"].Rows[i]["slno"]) == sln)
                {
                    docs = docs + Cn.ds.Tables["tmpdata"].Rows[i]["doc_string"].ToString();
                    i += 1;
                    if (i > maxR)
                    {
                        break;
                    }
                }

                DataRow dtr = Imgdt.NewRow();
                dtr["autono"] = schema+auton; // Convert.ToInt32(Cn.ds.Tables["tmpdata"].Rows[i-1]["m_autono"]);
                dtr["slno"] = Convert.ToInt32(Cn.ds.Tables["tmpdata"].Rows[i-1]["slno"]);
                dtr["doc_ctg"] = Cn.ds.Tables["tmpdata"].Rows[i-1]["doc_ctg"].ToString();
                dtr["doc_flname"] = Cn.ds.Tables["tmpdata"].Rows[i-1]["doc_flname"].ToString();
                dtr["doc_desc"] = Cn.ds.Tables["tmpdata"].Rows[i-1]["doc_desc"].ToString();
                dtr["doc_extn"] = Cn.ds.Tables["tmpdata"].Rows[i-1]["doc_extn"].ToString();
                dtr["doc_string"] = docs;
                Imgdt.Rows.Add(dtr);
            }
            Img.ImageTable = Imgdt;
        }

        public string FindImageData(string schema, string autono, long slno=0)
        {
            string autos = "autono='" + schema+ autono + "'";
            var dt = Img.ImageTable.Select(autos).ToList();
            string docs = "";
            if (dt.Count() > 0 )
            {
                docs = dt[0]["doc_string"].ToString();
            }
            return docs;
        }
    }
}