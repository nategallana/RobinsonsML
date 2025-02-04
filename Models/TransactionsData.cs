using RobinsonMallLinkingWBOX.LocalStorage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarsier.Database.Enums;

namespace RobinsonMallLinkingWBOX.Models
{
    public class OrderData
    {
        [DbColumn(IsIdentity = true, IsPrimary = true, AutoIncrement = true)]
        public int ID { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string Version { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string StoreName { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string OrderNo { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Subtotal { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Tips { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal ServiceCharge { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Total { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int Void { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string AccDate { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public int SalesType { get; set; }

        // public List<DiscountData> Discount { get; set; }
        // public List<TaxData> Taxes { get; set; }
        // public List<ItemData> Items { get; set; }
        // public List<PaymentData> Payments { get; set; }
    }
}
