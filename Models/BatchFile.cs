using RobinsonMallLinkingWBOX.LocalStorage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarsier.Database.Enums;

namespace RobinsonMallLinkingWBOX.Models
{
    public class BatchFile
    {
        [DbColumn(IsIdentity = true, IsPrimary = true, AutoIncrement = true)]
        public int ID { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string FileName { get; set; }

        [DbColumn(ColumnType = ColType.Integer, NotNull = true)]
        public string Count { get; set; }

        [DbColumn(ColumnType = ColType.DateTime, NotNull = true)]
        public decimal TransDate { get; set; }

        [DbColumn(ColumnType = ColType.DateTime, NotNull = true)]
        public decimal TransTime { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string Status { get; set; }
    }
}
