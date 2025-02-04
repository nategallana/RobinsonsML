using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.LocalStorage
{
    public class DiscountDataStorage : BaseStorage<DiscountData>
    {
        private string _tableName = new DiscountData().GetTableName();
        public DiscountDataStorage() : base(Program.DatabaseFile)
        {
            CreateTable(new DiscountData());
        }
    }
}
