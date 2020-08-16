namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_COMPTYPE")]
    public partial class MS_COMPTYPE
    {
        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [StringLength(2)]
        public string COMPTYCD { get; set; }

        [Required]
        [StringLength(25)]
        public string COMPTYNM { get; set; }

        [Required]
        [StringLength(40)]
        public string USR_ID { get; set; }

        public DateTime? USR_ENTDT { get; set; }

        [Required]
        [StringLength(15)]
        public string USR_LIP { get; set; }

        [Required]
        [StringLength(15)]
        public string USR_SIP { get; set; }

        [StringLength(50)]
        public string USR_OS { get; set; }

        [StringLength(50)]
        public string USR_MNM { get; set; }

        [StringLength(40)]
        public string LM_USR_ID { get; set; }

        public DateTime? LM_USR_ENTDT { get; set; }

        [StringLength(15)]
        public string LM_USR_LIP { get; set; }

        [StringLength(15)]
        public string LM_USR_SIP { get; set; }

        [StringLength(50)]
        public string LM_USR_OS { get; set; }

        [StringLength(50)]
        public string LM_USR_MNM { get; set; }

        [StringLength(40)]
        public string DEL_USR_ID { get; set; }

        public DateTime? DEL_USR_ENTDT { get; set; }

        [StringLength(15)]
        public string DEL_USR_LIP { get; set; }

        [StringLength(15)]
        public string DEL_USR_SIP { get; set; }

        [StringLength(50)]
        public string DEL_USR_OS { get; set; }

        [StringLength(50)]
        public string DEL_USR_MNM { get; set; }
        [StringLength(1)]
        public string LTDIND { get; set; }
        [StringLength(1)]
        public string DEDTYPE { get; set; }
    }
}
