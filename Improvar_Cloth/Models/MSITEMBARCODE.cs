using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSITEMBARCODE
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [StringLength(8)]
        public string ITCD { get; set; }

        [Key]
        [StringLength(20)]
        public string BARNO { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

         public double? STDRT { get; set; }
        public bool Checked { get; set; }
        public byte SRLNO { get; set; }
        [StringLength(10)]
        public string SIZENM { get; set; }
        [StringLength(20)]
        public string COLRNM { get; set; }
        [StringLength(3)]        
        public string SZBARCODE { get; set; }
        public string CLRBARCODE { get; set; }
    }
}