using RobinsonMallLinkingWBOX.LocalStorage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarsier.Database.Enums;

namespace RobinsonMallLinkingWBOX.Models
{
    public class EODData
    {
        [DbColumn(IsIdentity = true, IsPrimary = true, AutoIncrement = true)]
        public int ID { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string OrderNo { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string TransDate { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string Subtotal { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Tips { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal ServiceCharge { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Discount { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal TaxAmount { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Vatexempt { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal VattableSales { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Total { get; set; }

    }
}
