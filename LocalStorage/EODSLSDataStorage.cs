using RobinsonMallLinkingWBOX.Constants;
using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.Models;
using System;
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

        public DataTable GetDataEodDate()
        {
            string query = string.Format(Queries.SELECT_LAST_EODDATE);
            return GetDataTable(query);
        }

        public DataTable FirstEOD()
        {
            string query = string.Format(Queries.SELECT_FIRST_DATA);
            return GetDataTable(query);
        }
        public DataTable SelectEOD(string tableName, string columnName, string dateValue)
        {
            string query;

            if (string.IsNullOrEmpty(dateValue))
            {
                query = string.Format("SELECT COUNT(*) FROM {0} WHERE {1} IS NOT NULL", tableName, columnName);
            }
            else
            {
                query = string.Format(Queries.SELECT_COUNT_WHERE, tableName, columnName, dateValue);
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
