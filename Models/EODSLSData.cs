using RobinsonMallLinkingWBOX.LocalStorage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarsier.Database.Enums;

namespace RobinsonMallLinkingWBOX.Models
{
    public class EODSLSData
    {
        [DbColumn(IsIdentity = true, IsPrimary = true, AutoIncrement = true)]
        public int ID { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string TenantID { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string TermNo { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Gross { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TotVat { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TotVoid { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int NoVoid { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TotDisc { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int NoDisc { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TotRfnd { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int NoRfnd { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal OthrNegaAdjst { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int NoNegaAdjst { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TotSrvChrg { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int PrvEODcntr { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal PrvGrndTot { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int CurrEODcntr { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal CurrGrndTot { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string TransDate { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Novelty { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Misc { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal LocTax { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TotCrdSls { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TotCrdVat { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TotNonVat { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal PhrmSls { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal NonPhrmSls { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal DisabDisc { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal PercRent { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TotAmntRePrnt { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int NoRePrnt { get; set; }

    }
}
