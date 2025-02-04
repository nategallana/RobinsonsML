using RobinsonMallLinkingWBOX.LocalStorage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarsier.Database.Enums;

namespace RobinsonMallLinkingWBOX.Models
{
    public class TaxData
    {
        [DbColumn(IsIdentity = true, IsPrimary = true, AutoIncrement = true)]
        public int ID { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string OrderNo { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string Type { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string Name { get; set; }

        [DbColumn(ColumnType = ColType.Decimal, NotNull = true)]
        public decimal Value { get; set; }
    }
}
