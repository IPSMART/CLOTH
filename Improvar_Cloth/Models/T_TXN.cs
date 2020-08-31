namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXN")]
    public partial class T_TXN
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Required]
        [StringLength(5)]
        public string DOCCD { get; set; }

        public DateTime? DOCDT { get; set; }

        [Required]
        [StringLength(6)]
        public string DOCNO { get; set; }

        [Required]
        [StringLength(2)]
        public string DOCTAG { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(8)]
        public string CONSLCD { get; set; }

        [StringLength(3)]
        public string CURR_CD { get; set; }

         public double? CURRRT { get; set; }

         public double? BLAMT { get; set; }

        public DateTime? PREFDT { get; set; }

        [StringLength(16)]
        public string PREFNO { get; set; }

        [StringLength(1)]
        public string REVCHRG { get; set; }

         public double? ROAMT { get; set; }

        [StringLength(1)]
        public string ROYN { get; set; }

        [Required]
        [StringLength(6)]
        public string GOCD { get; set; }

        [StringLength(2)]
        public string JOBCD { get; set; }

        [StringLength(16)]
        public string MANSLIPNO { get; set; }

        public byte? DUEDAYS { get; set; }

         public double? TCSPER { get; set; }

         public double? TCSAMT { get; set; }

        [StringLength(8)]
        public string PARGLCD { get; set; }

        [StringLength(8)]
        public string GLCD { get; set; }

        [StringLength(8)]
        public string CLASS1CD { get; set; }

        [StringLength(8)]
        public string CLASS2CD { get; set; }

        [StringLength(5)]
        public string LINECD { get; set; }

        [StringLength(1)]
        public string BARGENTYPE { get; set; }

         public double? WPPER { get; set; }

         public double? RPPER { get; set; }

        [StringLength(50)]
        public string MENU_PARA { get; set; }
    }
}
