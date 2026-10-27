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
    public class BatchFileDataStorage : BaseStorage<BatchFile>
    {
        private string _tableName = new BatchFile().GetTableName();
        public BatchFileDataStorage() : base(Program.DatabaseFile)
        {
            CreateTable(new BatchFile());
        }
        public DataTable CheckBatchFileName()
        {
            string query = string.Format(Queries.SELECT_BATCHNAME);
            return GetDataTable(query);
        }

        public int GetLatestBatchCount(string datePrefix)
        {
            string query = $"SELECT MAX([Count]) FROM {_tableName} WHERE FileName LIKE '{datePrefix}.%'";
            object result = ExecuteScalar(query);
            return result != DBNull.Value && result != null ? Convert.ToInt32(result) : 0;
        }

        public DataTable getUnsentFile()
        {
            string query = string.Format(Queries.SELECT_UNSENTBATCH,"Unsent");
            return GetDataTable(query);
        }

    }

}
