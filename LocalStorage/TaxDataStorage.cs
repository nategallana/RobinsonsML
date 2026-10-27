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
    public class TaxDataStorage : BaseStorage<TaxData>
    {
        private string _tableName = new TaxData().GetTableName();
        public TaxDataStorage() : base(Program.DatabaseFile)
        {
            CreateTable(new TaxData());
        }

        public DataTable GetTaxAmount(string processDate)
        {
            string query = String.Format(Queries.SELECT_TABLE_INNER_WHERE, _tableName, "orderdata",processDate);
            return GetDataTable(query);
        }
    }
}
