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
    public class TransactionsDataStorage : BaseStorage<OrderData>
    {
        private string _tableName = new OrderData().GetTableName();
        public TransactionsDataStorage() : base(Program.DatabaseFile)
        {
            CreateTable(new OrderData());
        }

        public DataTable GetDataAccDate() 
        { 
            string query = string.Format(Queries.SELECT_LAST_DATE);
            return GetDataTable(query);
        }

        public DataTable GetALLDATAS(string processDate)
        {
            string query = string.Format(Queries.SELECT_ALL_DATA, processDate);
            return GetDataTable(query);
        }

        public DataTable GetDataYesterday(string processDate)
        {
            string query = string.Format(Queries.SELECT_DATA_YESTERDAY, processDate);
            return GetDataTable(query);
        }

        public DataTable GetDISCOUNT(string processDate) 
        {
            string query = string.Format(Queries.SELECT_DISCOUNT, processDate);
            return GetDataTable(query);
        }
    }
}