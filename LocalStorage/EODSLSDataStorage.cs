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
    public class EODSLSDataStorage : BaseStorage<EODSLSData>
    {
        private string _tableName = new EODSLSData().GetTableName();
        public EODSLSDataStorage() : base(Program.DatabaseFile)
        {
            CreateTable(new EODSLSData());
        }

        public DataTable CheckEODtable()
        {
            string query = string.Format(Queries.COUNT_EOD);
            return GetDataTable(query);
        }

        public DataTable SelectEOD(string table, string column, string date)
        {
            string query = string.Format(Queries.SELECT_COUNT_WHERE);
            return GetDataTable(query);
        }

        public DataTable SelectPERTRANS(string table, string column, string date)
        {
            string query = string.Format(Queries.SELECT_COUNT_WHERE);
            return GetDataTable(query);
        }
    }
}
