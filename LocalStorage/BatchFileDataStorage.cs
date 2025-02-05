using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
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
    }
}
