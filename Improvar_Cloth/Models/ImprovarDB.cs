using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Models
{
    public class ImprovarDB : DbContext, IDbModelCacheKeyProvider
    {
        public string SchemaDBO { get; private set; }
        internal ImprovarDB(string Constring, string schemaname) : base(new OracleConnection(Constring), true)
        {
            SchemaDBO = schemaname;
        }
        public ImprovarDB() : base("name=local1")
        {
            Database.SetInitializer<ImprovarDB>(null);
        }
        public virtual DbSet<M_PAYMENT> M_PAYMENT { get; set; }
        public virtual DbSet<M_STKTYPE> M_STKTYPE { get; set; }
        public virtual DbSet<M_JOBPRCCD> M_JOBPRCCD { get; set; }
        public virtual DbSet<M_MTRLJOBMST> M_MTRLJOBMST { get; set; }
        public virtual DbSet<M_RETDEB> M_RETDEB { get; set; }
        public virtual DbSet<M_ITEMPLIST_ADD> M_ITEMPLIST_ADD { get; set; }
        public virtual DbSet<M_ITEMPLISTDTL> M_ITEMPLISTDTL { get; set; }
        public virtual DbSet<M_SITEMBOM> M_SITEMBOM { get; set; }
        public virtual DbSet<M_SITEMBOMAPPRVL> M_SITEMBOMAPPRVL { get; set; }
        public virtual DbSet<M_SITEMBOMMTRL> M_SITEMBOMMTRL { get; set; }
        public virtual DbSet<M_SITEMBOMPART> M_SITEMBOMPART { get; set; }
        public virtual DbSet<M_SITEMBOMSJOB> M_SITEMBOMSJOB { get; set; }
        public virtual DbSet<M_SITEMBOMSUPLR> M_SITEMBOMSUPLR { get; set; }
        public virtual DbSet<M_SITEM_SIZE> M_SITEM_SIZE { get; set; }
        public virtual DbSet<M_AMTTYPE> M_AMTTYPE { get; set; }
        public virtual DbSet<M_BRAND> M_BRAND { get; set; }
        public virtual DbSet<M_CNTRL_AUTH> M_CNTRL_AUTH { get; set; }
        public virtual DbSet<M_CNTRL_HDR> M_CNTRL_HDR { get; set; }
        public virtual DbSet<M_CNTRL_HDR_DOC> M_CNTRL_HDR_DOC { get; set; }
        public virtual DbSet<M_CNTRL_HDR_DOC_DTL> M_CNTRL_HDR_DOC_DTL { get; set; }
        public virtual DbSet<M_CNTRL_LOCA> M_CNTRL_LOCA { get; set; }
        public virtual DbSet<M_COLLECTION> M_COLLECTION { get; set; }
        public virtual DbSet<M_COLOR> M_COLOR { get; set; }
        public virtual DbSet<M_DISCRTDTL> M_DISCRTDTL { get; set; }
        public virtual DbSet<M_DOC_BRAND> M_DOC_BRAND { get; set; }
        public virtual DbSet<M_DOCTYPE> M_DOCTYPE { get; set; }
        public virtual DbSet<M_DTYPE> M_DTYPE { get; set; }
        public virtual DbSet<M_GODOWN> M_GODOWN { get; set; }
        public virtual DbSet<M_GROUP> M_GROUP { get; set; }
        public virtual DbSet<M_ITEM_UN_CNV> M_ITEM_UN_CNV { get; set; }
        public virtual DbSet<M_JOBMST> M_JOBMST { get; set; }
        public virtual DbSet<M_LOCKDATA> M_LOCKDATA { get; set; }
        public virtual DbSet<M_MACHINE> M_MACHINE { get; set; }
        public virtual DbSet<M_MONTH> M_MONTH { get; set; }
        public virtual DbSet<M_PARTS> M_PARTS { get; set; }
        public virtual DbSet<M_PRODGRP> M_PRODGRP { get; set; }
        public virtual DbSet<M_PRODTAX> M_PRODTAX { get; set; }
        public virtual DbSet<M_SCHEME> M_SCHEME { get; set; }
        public virtual DbSet<M_SCHEME_HDR> M_SCHEME_HDR { get; set; }
        public virtual DbSet<M_SITEM> M_SITEM { get; set; }
        public virtual DbSet<M_SITEM_BARCODE> M_SITEM_BARCODE { get; set; }
        public virtual DbSet<M_SITEM_COLOR> M_SITEM_COLOR { get; set; }
        public virtual DbSet<M_SITEM_MEASURE> M_SITEM_MEASURE { get; set; }
        public virtual DbSet<M_SITEM_PARTS> M_SITEM_PARTS { get; set; }
        public virtual DbSet<M_SIZE> M_SIZE { get; set; }
        public virtual DbSet<M_SIZEGRP> M_SIZEGRP { get; set; }
        public virtual DbSet<M_USR_ACS_DOCCD> M_USR_ACS_DOCCD { get; set; }
        public virtual DbSet<T_PSLIP> T_PSLIP { get; set; }
        public virtual DbSet<T_TXN> T_TXN { get; set; }

        public virtual DbSet<M_PRCLST> M_PRCLST { get; set; }
        public virtual DbSet<T_TXNSTATUS> T_TXNSTATUS { get; set; }
        public virtual DbSet<M_CNTRL_HDR_REM> M_CNTRL_HDR_REM { get; set; }
        public virtual DbSet<M_POST> M_POST { get; set; }
        public virtual DbSet<MS_PORTCD> MS_PORTCD { get; set; }
        public virtual DbSet<M_GODLINK> M_GODLINK { get; set; }
        public virtual DbSet<M_TDS_CNTRL> M_TDS_CNTRL { get; set; }
        public virtual DbSet<T_CNTRL_AUTH> T_CNTRL_AUTH { get; set; }
        public virtual DbSet<T_CNTRL_DOC_PASS> T_CNTRL_DOC_PASS { get; set; }
        public virtual DbSet<M_CLASS1> M_CLASS1 { get; set; }
        public virtual DbSet<M_CLASS2> M_CLASS2 { get; set; }
        public virtual DbSet<M_SIGN_AUTH> M_SIGN_AUTH { get; set; }
        public virtual DbSet<M_DOC_AUTH> M_DOC_AUTH { get; set; }
        public virtual DbSet<M_SDR_QUERY> M_SDR_QUERY { get; set; }
        public virtual DbSet<M_SDR_USERID> M_SDR_USERID { get; set; }
        public virtual DbSet<M_SUBLEG_BRAND> M_SUBLEG_BRAND { get; set; }
        public virtual DbSet<M_SUBLEG_COM> M_SUBLEG_COM { get; set; }
        public virtual DbSet<M_SUBLEG_SDDTL> M_SUBLEG_SDDTL { get; set; }
        public virtual DbSet<SD_COMPANY> SD_COMPANY { get; set; }
        public virtual DbSet<USER_GUIDE> USER_GUIDE { get; set; }
        public virtual DbSet<M_SUBLEG_LOCOTH> M_SUBLEG_LOCOTH { get; set; }
        public virtual DbSet<MS_CURRENCY> MS_CURRENCY { get; set; }
        public virtual DbSet<M_USR_ACS_GRPDTL> M_USR_ACS_GRPDTL { get; set; }
        public virtual DbSet<M_TAXGRP> M_TAXGRP { get; set; }
        public virtual DbSet<M_DESPMDCD> M_DESPMDCD { get; set; }
        public virtual DbSet<M_EMPMAS> M_EMPMAS { get; set; }
        public virtual DbSet<M_EMPMAS_DET> M_EMPMAS_DET { get; set; }
        public virtual DbSet<M_PARTYGRP> M_PARTYGRP { get; set; }
        public virtual DbSet<M_DISCRTHDR> M_DISCRTHDR { get; set; }
        public virtual DbSet<M_AREACD> M_AREACD { get; set; }
        public virtual DbSet<M_DISCRT> M_DISCRT { get; set; }
        public virtual DbSet<USER_APPL> USER_APPL { get; set; }
        public virtual DbSet<USER_ACTIVITY> USER_ACTIVITY { get; set; }
        public virtual DbSet<M_USR_ACS> M_USR_ACS { get; set; }
        public virtual DbSet<MS_DOCCTG> MS_DOCCTG { get; set; }
        public virtual DbSet<M_GENLEG> M_GENLEG { get; set; }
        public virtual DbSet<MS_LINK> MS_LINK { get; set; }
        public virtual DbSet<MS_COMPTYPE> MS_COMPTYPE { get; set; }
        public virtual DbSet<M_COMP> M_COMP { get; set; }
        public virtual DbSet<M_LOCA> M_LOCA { get; set; }
        public virtual DbSet<MS_NATBUSCODES> MS_NATBUSCODES { get; set; }
        public virtual DbSet<M_SUBLEG_BUSNAT> M_SUBLEG_BUSNAT { get; set; }
        public virtual DbSet<MS_GSTUOM> MS_GSTUOM { get; set; }
        public virtual DbSet<MS_STATE> MS_STATE { get; set; }
        public virtual DbSet<MS_COUNTRY> MS_COUNTRY { get; set; }
        public virtual DbSet<M_DISTRICT> M_DISTRICT { get; set; }
        public virtual DbSet<M_SUBLEG> M_SUBLEG { get; set; }
        public virtual DbSet<M_SUBLEG_GL> M_SUBLEG_GL { get; set; }
        public virtual DbSet<M_SUBLEG_TAX> M_SUBLEG_TAX { get; set; }
        public virtual DbSet<M_SUBLEG_CONT> M_SUBLEG_CONT { get; set; }
        public virtual DbSet<M_DESIGNATION> M_DESIGNATION { get; set; }
        public virtual DbSet<MS_BANKIFSC> MS_BANKIFSC { get; set; }
        public virtual DbSet<M_SUBLEG_IFSC> M_SUBLEG_IFSC { get; set; }
        public virtual DbSet<M_SUBLEG_LINK> M_SUBLEG_LINK { get; set; }
        public virtual DbSet<M_SUBBRAND> M_SUBBRAND { get; set; }
        public virtual DbSet<M_ITEM> M_ITEM { get; set; }
        public virtual DbSet<M_UOM> M_UOM { get; set; }
        public virtual DbSet<T_DLYTARACH> T_DLYTARACH { get; set; }
        public virtual DbSet<T_DLYTARACH_DTL> T_DLYTARACH_DTL { get; set; }
        public virtual DbSet<T_DLYTARACH_DTL_ITM> T_DLYTARACH_DTL_ITM { get; set; }
        public virtual DbSet<T_INHISS> T_INHISS { get; set; }
        public virtual DbSet<T_CNTRL_HDR> T_CNTRL_HDR { get; set; }
        public virtual DbSet<T_CNTRL_HDR_REM> T_CNTRL_HDR_REM { get; set; }
        public virtual DbSet<T_CNTRL_HDR_DOC> T_CNTRL_HDR_DOC { get; set; }
        public virtual DbSet<T_CNTRL_HDR_DOC_DTL> T_CNTRL_HDR_DOC_DTL { get; set; }
        public virtual DbSet<T_DO> T_DO { get; set; }
        public virtual DbSet<T_DODTL> T_DODTL { get; set; }
        public virtual DbSet<T_DO_CANC> T_DO_CANC { get; set; }
        public virtual DbSet<T_BATCHMST> T_BATCHMST { get; set; }
        public virtual DbSet<T_BATCHDTL> T_BATCHDTL { get; set; }
        public virtual DbSet<T_DLY_ATN> T_DLY_ATN { get; set; }
        public virtual DbSet<V_SJOBMST_STDRT> V_SJOBMST_STDRT { get; set; }
        public virtual DbSet<T_PSLIPDTL> T_PSLIPDTL { get; set; }
        public virtual DbSet<T_GENTRY> T_GENTRY { get; set; }
        public virtual DbSet<MS_MUSRACS> MS_MUSRACS { get; set; }
        public virtual DbSet<T_SORD> T_SORD { get; set; }
        public virtual DbSet<T_SORD_SCHEME> T_SORD_SCHEME { get; set; }
        public virtual DbSet<T_SORDDTL> T_SORDDTL { get; set; }
        public virtual DbSet<M_LINEMAST> M_LINEMAST { get; set; }
        public virtual DbSet<M_FLRLOCA> M_FLRLOCA { get; set; }
        public virtual DbSet<M_JOBMSTSUB> M_JOBMSTSUB { get; set; }
        public virtual DbSet<T_SORD_CANC> T_SORD_CANC { get; set; }
        public virtual DbSet<M_JOBMSTSUB_STDRT> M_JOBMSTSUB_STDRT { get; set; }
        public virtual DbSet<T_INHISSMST> T_INHISSMST { get; set; }
        public virtual DbSet<T_INHISSMSTSJOB> T_INHISSMSTSJOB { get; set; }
        public virtual DbSet<T_INHRECMST> T_INHRECMST { get; set; }
        public virtual DbSet<T_TXN_LINKNO> T_TXN_LINKNO { get; set; }
        public virtual DbSet<T_TXNACK> T_TXNACK { get; set; }
        public virtual DbSet<T_TXNAMT> T_TXNAMT { get; set; }
        public virtual DbSet<T_TXNDTL> T_TXNDTL { get; set; }
        public virtual DbSet<T_TXNMEMO> T_TXNMEMO { get; set; }
        public virtual DbSet<T_TXNOTH> T_TXNOTH { get; set; }
        public virtual DbSet<T_TXNPYMT> T_TXNPYMT { get; set; }
        public virtual DbSet<T_TXNTRANS> T_TXNTRANS { get; set; }
        public virtual DbSet<T_BATCH_IMG_HDR> T_BATCH_IMG_HDR { get; set; }
        public virtual DbSet<T_BATCH_IMG_HDR_DTL> T_BATCH_IMG_HDR_DTL { get; set; }
        public virtual DbSet<T_BATCH_IMG_HDR_LINK> T_BATCH_IMG_HDR_LINK { get; set; }
        public virtual DbSet<T_BATCHMST_PRICE> T_BATCHMST_PRICE { get; set; }
        public virtual DbSet<M_BLTYPE> M_BLTYPE { get; set; }
        public virtual DbSet<M_SITEM_SLCD> M_SITEM_SLCD { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (SchemaDBO != null)
            {
                Database.SetInitializer<ImprovarDB>(null);
                modelBuilder.HasDefaultSchema(SchemaDBO);
            }

            base.OnModelCreating(modelBuilder);
        }
        public string CacheKey
        {
            get { return SchemaDBO; }
        }

    }
}