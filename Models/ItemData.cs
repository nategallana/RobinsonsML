using RobinsonMallLinkingWBOX.LocalStorage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarsier.Database.Enums;

namespace RobinsonMallLinkingWBOX.Models
{
    public class ItemData
    {
        [DbColumn(IsIdentity = true, IsPrimary = true, AutoIncrement = true)]
        public int ID { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int SeqNo { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string OrderNo { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string MenuKey { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string MenuNo { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string MenuName { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int Qty { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal PriceBefDisc { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal PriceAftDisc { get; set; }

        //[DbColumn(ColumnType = ColType.Text, NotNull = true)]
        //public string? DiscCode { get; set; }

        //[DbColumn(ColumnType = ColType.Text, NotNull = true)]
        //public string? DiscName { get; set; }

        //[DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        //public decimal? DiscAmount { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string TaxCode { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TotalTax { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Price { get; set; }
    }
}
