using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.LocalStorage
{
    public class EODDataStorage : BaseStorage<EODData>
    {
        private string _tableName = new EODData().GetTableName();
        public EODDataStorage() : base(Program.DatabaseFile)
        {
            CreateTable(new EODData());
        }
    }
}
