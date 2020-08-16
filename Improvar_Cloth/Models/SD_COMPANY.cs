namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SD_COMPANY")]
    public partial class SD_COMPANY
    {
        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SRLNO { get; set; }

        [StringLength(60)]
        public string COMPNM { get; set; }

        [StringLength(50)]
        public string LOCNM { get; set; }

        public DateTime? FROM_DATE { get; set; }

        public DateTime? UPTO_DATE { get; set; }

        [StringLength(4)]
        public string YEAR_CODE { get; set; }

        [StringLength(4)]
        public string COMPCD { get; set; }

        [StringLength(4)]
        public string LOCCD { get; set; }

        [StringLength(40)]
        public string SCHEMA_NAME { get; set; }

        public short? DATA_VERSION { get; set; }

        [StringLength(1)]
        public string EXPORT_TAG { get; set; }

        [StringLength(1)]
        public string IMPORT_TAG { get; set; }

        [StringLength(1)]
        public string MERGE_TAG { get; set; }

        [StringLength(1)]
        public string MIRROR_TAG { get; set; }

        [StringLength(40)]
        public string SALES_SCHEMA { get; set; }

        [StringLength(40)]
        public string PAY_SCHEMA { get; set; }

        [StringLength(40)]
        public string INVENTORY_SCHEMA { get; set; }

        [StringLength(4)]
        public string CLIENT_CODE { get; set; }

        [StringLength(40)]
        public string FIN_SCHEMA { get; set; }
    }
}
