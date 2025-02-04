using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.LocalStorage
{
    public class ItemDataStorage : BaseStorage<ItemData>
    {
        private string _tableName = new ItemData().GetTableName();
        public ItemDataStorage() : base(Program.DatabaseFile)
        {
            CreateTable(new ItemData());
        }
    }
}
