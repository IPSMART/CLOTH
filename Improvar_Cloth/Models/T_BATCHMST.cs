namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("T_BATCHMST")]
    public partial class T_BATCHMST
    {
        public int? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string BATCHAUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BATCHSLNO { get; set; }

        [Required]
        [StringLength(30)]
        public string AUTONO { get; set; }

        public int SLNO { get; set; }

        [Required]
        [StringLength(5)]
        public string DOCCD { get; set; }

        [Required]
        [StringLength(6)]
        public string DOCNO { get; set; }

        public DateTime? DOCDT { get; set; }

        [StringLength(40)]
        public string BATCHNO { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [Required]
        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        public double? NOS { get; set; }

        public double? QNTY { get; set; }

        public double? STKQNTY { get; set; }

        public double? RATE { get; set; }

        [StringLength(30)]
        public string ORGBATCHAUTONO { get; set; }

        public int? ORGBATCHSLNO { get; set; }

        public int? GSM { get; set; }

        [StringLength(5)]
        public string TEXTURE { get; set; }

        public int? GAUGE { get; set; }

        public int? LL { get; set; }

        [StringLength(15)]
        public string MCHNNAME { get; set; }

        public double? DIA { get; set; }

        public double? CUTLENGTH { get; set; }

        [StringLength(1)]
        public string QNTYIN { get; set; }

        [StringLength(15)]
        public string COLRNM { get; set; }

        [StringLength(15)]
        public string MILLNM { get; set; }        

        [Required]
        [StringLength(1)]
        public string STKTYPE { get; set; }
        
        [StringLength(2)]
        public string JOBCD { get; set; }

        [StringLength(1)]
        public string FABTYPE { get; set; }
    }
}
