using RobinsonMallLinkingWBOX.Constants;
using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.LocalStorage
{
    public class VoidDataStorage : BaseStorage<VoidData>
    {
        private string _tableName = new VoidData().GetTableName();
        public VoidDataStorage() : base(Program.DatabaseFile)
        {
            CreateTable(new VoidData());
        }

        public DataTable GetVOID(string processDate)
        {
            string query = string.Format(Queries.SELECT_VOID, processDate);
            return GetDataTable(query);
        }
    }
}
