using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.LocalStorage
{
    public class TaxDataStorage : BaseStorage<TaxData>
    {
        private string _tableName = new TaxData().GetTableName();
        public TaxDataStorage() : base(Program.DatabaseFile)
        {
            CreateTable(new TaxData());
        }
    }
}
