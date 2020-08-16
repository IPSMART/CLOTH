using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class ControlHeader
    {

        //ClassicDB DB = new ClassicDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //public ControlHeader(M_GENLEG MGENLEG)
        //{
        //    DTAG = "";
        //    USR_ENTDT = System.DateTime.Now;

            
        //}
        public string DTAG { get; set; }
        public long M_AUTONO { get; set; }
        public string M_TBLNM { get; set; }
        public string USR_ID { get; set; }
        public DateTime? USR_ENTDT { get; set; }
        public string USR_LIP { get; set; }
        public string USR_SIP { get; set; }
        public string USR_OS { get; set; }
        public string USR_MNM { get; set; }
        public string LM_USR_ID { get; set; }
        public DateTime? LM_USR_ENTDT { get; set; }
        public string LM_USR_LIP { get; set; }
        public string LM_USR_SIP { get; set; }
        public string LM_USR_OS { get; set; }
        public string LM_USR_MNM { get; set; }
        public string DEL_USR_ID { get; set; }
        public DateTime? DEL_USR_ENTDT { get; set; }
        public string DEL_USR_LIP { get; set; }
        public string DEL_USR_SIP { get; set; }
        public string DEL_USR_OS { get; set; }
        public string DEL_USR_MNM { get; set; }
        public string INACTIVE_TAG { get; set; }
        public DateTime? NONOP_DT { get; set; }
        public string NONOP_REM { get; set; }
        //public M_GENLEG M_GENLEG { get; set; }
    }
}