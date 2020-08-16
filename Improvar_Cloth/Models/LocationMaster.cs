using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Improvar.Models
{
    public class LocationMaster : Permission
    {
        [Display(Name = "Ledger Code")]
        public string SLCODE { get; set; }

        [Display(Name = "Ledger Name")]
        public string SLNM { get; set; }

        [Display(Name = "Short Name")]
        public string SHORTNM { get; set; }

        [Display(Name = "Full Name (Optional)")]

        [StringLength(35)]
        public string SLFN { get; set; }

        public string Address { get; set; }

        [Display(Name = "Building No.")]
        public string BUILDNO { get; set; }

        [Display(Name = "Floor No.")]
        public string FLNO { get; set; }

        [Display(Name = "Locality")]
        public string LOCAL { get; set; }

        [Display(Name = "Address (Additional)")]
        public string ADDADDI { get; set; }

        [Display(Name = "State")]
        public string STAT { get; set; }

        [Display(Name = "District")]
        public string DISTRT { get; set; }

        [Display(Name = "Pin")]
        public string PIN { get; set; }

        [Display(Name = "Country")]
        public string CNTRY { get; set; }

        [Display(Name = "Ledger Name")]
        public string LNAME { get; set; }

        [Display(Name = "Permises")]
        public string PERMSS { get; set; }

        [Display(Name = "Road Name")]
        public string RDNM { get; set; }


        [Display(Name = "LandMark")]
        public string LNDMRK { get; set; }

        [Display(Name = "Nature of business activities")]
        public bool NOBABCO { get; set; }

        [Display(Name = "Products deals in as per GST")]
        public bool PDIAPGST { get; set; }
        public List<ProductDeals> ProductDeals { get; set; }

        [Display(Name = "Short Name")]
        public string SRTNM { get; set; }

        [Display(Name = "GST No.")]
        public string GSTNO { get; set; }

        [Required]
        [Display(Name = "GST Date")]
        [DataType(DataType.Date)]
        public DateTime? GSTDATE { get; set; }


        [Display(Name = "TAN No.")]

        public string TANNO { get; set; }

        [Display(Name = "State Code (TDS)")]
        public string STATCD { get; set; }

        [Display(Name = "GPS Location")]
        public string GPSL { get; set; }

        [Display(Name = "Latitude")]
        public string LATITUD { get; set; }

        [Display(Name = "Longtitude")]
        public string LONGTUD { get; set; }

        [Display(Name = "Website")]
        public string WEBST { get; set; }

        [Display(Name = "Office Phone No.")]
        public string OFFPHNO { get; set; }

        [Display(Name = "Office Email")]
        public string OFFEMAIL { get; set; }

        [Display(Name = "Std")]
        public string STD1 { get; set; }

        [Display(Name = "No.")]
        public string NO1 { get; set; }


        [Display(Name = "Phone 2")]

        public string PH2 { get; set; }

        [Display(Name = "Std")]
        public string STD2 { get; set; }

        [Display(Name = "No.")]
        public string NO2 { get; set; }

        [Display(Name = "Phone 3")]

        public string PH3 { get; set; }

        [Display(Name = "Std")]
        public string STD3 { get; set; }

        [Display(Name = "No.")]
        public string NO3 { get; set; }
        public string Help { get; set; }

        [Display(Name = "Document Type")]

        public string DOCTYPE { get; set; }
        public List<DOCTYPE> Document { get; set; }

        [Display(Name = "Document Description")]
        public string DOCDESC { get; set; }


    }
}