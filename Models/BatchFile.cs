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
        public int Count { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string TransDate { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string TransTime { get; set; }

        [DbColumn(ColumnType = ColType.Text, NotNull = true)]
        public string Status { get; set; }
    }
}
