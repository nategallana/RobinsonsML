using RobinsonMallLinkingWBOX.LocalStorage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarsier.Database.Enums;

namespace RobinsonMallLinkingWBOX.Models
{
    public class VoidData
    {
        [DbColumn(IsIdentity = true, IsPrimary = true, AutoIncrement = true)]
        public int ID { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string EmpID { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string OrderID { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string CloudID { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string Reason { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string EmpName { get; set; }
    }
}