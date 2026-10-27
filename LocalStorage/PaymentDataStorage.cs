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
    public class PaymentDataStorage : BaseStorage<PaymentData>
    {
        private string _tableName = new PaymentData().GetTableName();
        public PaymentDataStorage() : base(Program.DatabaseFile)
        {
            CreateTable(new PaymentData());
        }

        public DataTable getPayment(string processDate)
        {
            string query = string.Format(Queries.SELECT_PAYMENT_ONLY, processDate);
            return GetDataTable(query);
        }
    }
}