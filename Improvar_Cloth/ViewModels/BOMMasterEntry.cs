using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class BOMMasterEntry : Permission
    {
        public string MSG { get; set; }
        public M_ITEM M_ITEM { get; set; }
        public M_COLOR M_COLOR { get; set; }
        public M_SIZE M_SIZE { get; set; }
        public M_SITEM M_SITEM { get; set; }
        public M_SITEMBOM M_SITEMBOM { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_SITEMBOMMTRL M_SITEMBOMMTRL { get; set; }
        public M_SITEMBOMPART M_SITEMBOMPART { get; set; }
        public M_SITEMBOMSUPLR M_SITEMBOMSUPLR { get; set; }
        public M_GROUP M_GROUP { get; set; }
        public M_UOM M_UOM { get; set; }
        //public M_SITEM_INVCD M_SITEM_INVCD { get; set; }
        public List<MSITEMBOMPART> MSITEMBOMPART { get; set; }
        public List<MSITEMBOMSJOB> MSITEMBOMSJOB { get; set; }
        public List<MSITEMBOMMTRL> MSITEMBOMMTRL { get; set; }
        public List<MSITEMBOMMTRL_RMPM> MSITEMBOMMTRL_RMPM { get; set; }
        public List<MSITEMBOMSUPLR> MSITEMBOMSUPLR { get; set; }
        public List<MSITEMBOM_PRINT> MSITEMBOM_PRINT { get; set; }
        public List<SizeLink> SizeLink { get; set; }
        //public List<IndexKey1> IndexKey1 { get; set; }
        //public List<IndexKey01> IndexKey01 { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public bool Checked { get; set; }
        public int SERIAL { get; set; }
        public string STYLENUMBER { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_NAME { get; set; }
        public string UOM_CODE { get; set; }
        public bool APPLY_TO_ALL_SIZE { get; set; }
        public string COPY_SIZE { get; set; }
    }
}