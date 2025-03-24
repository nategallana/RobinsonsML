using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.Constants
{
    public class Queries
    {
        public const string SELECT_TABLE_WHERE = @"SELECT * FROM {0} WHERE {1}";
        public const string SELECT_TABLE_DESC = @"SELECT * FROM {0} ORDER BY {1} DESC";

        //public const string SELECT_LAST_DATE = @"SELECT orderdata.AccDate AS TranscDate FROM orderdata WHERE orderdata.AccDate = '20241227' GROUP BY orderdata.AccDate";
        public const string SELECT_LAST_DATE = @"SELECT max(orderdata.AccDate) AS TranscDate FROM orderdata GROUP BY orderdata.AccDate ORDER BY orderdata.AccDate DESC LIMIT 1";
        public const string SELECT_LAST_EODDATE = @"SELECT MAX(TransDate) AS TransDate FROM eodslsdata";

        public const string SELECT_ALL_DATA = @"SELECT 
                                                    orderdata.OrderNo,
                                                    orderdata.AccDate AS TranscDate,
                                                    SUM(orderdata.Subtotal) AS Subtotal, 
                                                    SUM(orderdata.Tips) AS Tips, 
                                                    SUM(orderdata.ServiceCharge) AS ServiceCharge,
                                                    SUM(ABS(IFNULL(discountdata.Amount, 0))) AS DiscountValue,
                                                    SUM(IFNULL(taxdata.Value, 0)) AS TaxAmount,
                                                    -- VAT exempt calculation
                                                    SUM(orderdata.Subtotal + orderdata.ServiceCharge - ABS(IFNULL(discountdata.Amount, 0)) - IFNULL(taxdata.Value, 0)) AS VATexempt,
                                                    -- Vattable Sales calculation
                                                    SUM(orderdata.Subtotal + orderdata.ServiceCharge - ABS(IFNULL(discountdata.Amount, 0))) AS VattableSales,
                                                    -- Total calculation
                                                    SUM(orderdata.Subtotal + orderdata.ServiceCharge - ABS(IFNULL(discountdata.Amount, 0))) AS Total
                                                FROM 
                                                    orderdata
                                                LEFT JOIN 
                                                    discountdata ON orderdata.OrderNo = discountdata.OrderNo
                                                LEFT JOIN 
                                                    taxdata ON orderdata.OrderNo = taxdata.OrderNo
                                                WHERE
                                                    orderdata.AccDate = {0}
                                                GROUP BY 
                                                    orderdata.AccDate
                                                    -- , orderdata.OrderNo";

        public const string SELECT_DATA_YESTERDAY = @"SELECT * FROM eodslsdata WHERE TransDate = '{0}'";

        public const string SELECT_DISCOUNT = @"SELECT 
                                                    COUNT(orderdata.OrderNo) AS CountDiscount,
                                                    SUM(ABS(IFNULL(discountdata.Amount, 0))) AS DiscountValue
                                                FROM 
                                                    orderdata
                                                INNER JOIN 
                                                    discountdata ON orderdata.OrderNo = discountdata.OrderNo
                                                WHERE   
                                                    orderdata.AccDate = '{0}'";

        public const string SELECT_VOID = @"SELECT 
	                                            COUNT(orderdata.OrderNo) AS CountVoid,
	                                            SUM(ABS(IFNULL(orderdata.Total, 0))) AS TotalVoid
                                            FROM 
	                                            orderdata
                                            INNER JOIN 
	                                            voiddata ON orderdata.OrderNo = voiddata.OrderID
                                            WHERE   
	                                            orderdata.AccDate = '{0}'";


        public const string COUNT_EOD = @"SELECT
                                                COUNT(*) AS CountEOD
                                          FROM
                                                eodslsdata";

        public const string SELECT_COUNT_WHERE = @"SELECT 
                                                        COUNT(*)
                                                   FROM 
                                                        {0} 
                                                   WHERE 
                                                        {1} = '{2}'";

        public const string SELECT_BATCHNAME = @"SELECT
                                                    batchfile.FileName,
                                                    batchfile.Count
                                                FROM 
                                                    batchfile";

        public const string INSERT_BATCHFILE = @"INSERT INTO batchfile (FileName, Count, TransTime, Status) 
                                                         VALUES ({0},{1},{2},{3})";

    }
}