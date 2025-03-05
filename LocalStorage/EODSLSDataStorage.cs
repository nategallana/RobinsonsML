using RobinsonMallLinkingWBOX.Constants;
using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.Models;
using System.Data;

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
            //Error when running -- Index (zero based) must be greater than or equal to zero and less than the size of the argument list.
            //string query = string.Format(Queries.SELECT_COUNT_WHERE);
            //return GetDataTable(query);

            string query;

            if (string.IsNullOrEmpty(date))
            {
                query = string.Format("SELECT COUNT(*) FROM {0} WHERE {1} IS NOT NULL", table, column);
            }
            else
            {
                query = string.Format(Queries.SELECT_COUNT_WHERE, table, column, date);
            }

            return GetDataTable(query);

        }

        public DataTable SelectPERTRANS(string table, string column, string date)
        {
            //string query = string.Format(Queries.SELECT_COUNT_WHERE);
            string query;
            if (string.IsNullOrEmpty(date))
            {
                query = string.Format("SELECT COUNT(*) FROM {0} WHERE {1} IS NOT NULL", table, column);
            }
            else
            {
                query = string.Format(Queries.SELECT_COUNT_WHERE, table, column, date);
            }
            return GetDataTable(query);
        }
    }
}
