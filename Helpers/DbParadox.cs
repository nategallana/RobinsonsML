using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobMLWBOX.Helpers
{
    public class DbParadox
    {
        private string DatabasePath { get; set; }
        private string Password { get; set; }

        public DbParadox(string databasePath, string password)
        {
            if (string.IsNullOrEmpty(databasePath))
            {
                throw new ArgumentNullException("filename", "Directory of database is empty");
            }
            if (!Directory.Exists(databasePath))
            {
                Directory.CreateDirectory(databasePath);
            }
            DatabasePath = databasePath;
            Password = password;
        }


        public string GetConnectionString()
        {
            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new ArgumentNullException("filename", "Directory of database is empty");
            }
            return GetConnectionString(DatabasePath, Password);
        }

        public string GetConnectionString(string databasePath, string password)
        {
            if (string.IsNullOrEmpty(databasePath))
            {
                throw new ArgumentNullException("filename", "Directory of database is empty");
            }

            return String.Format("Driver={{Microsoft Paradox Driver (*.db )}};DriverID=538;Fil=Paradox 5.X;DefaultDir={0};Dbq={0};CollatingSequence=ASCII;PWD={1};", databasePath, password);

        }

        public OdbcConnection GetConnection(string filename, string password)
        {
            string connString = GetConnectionString(filename, password);
            OdbcConnection conn = new OdbcConnection(connString);
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            return conn;
        }

        public OdbcConnection GetConnection()
        {
            string connString = GetConnectionString();
            OdbcConnection connection = new OdbcConnection(connString);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            return connection;
        }

        public int ExecuteNonQuery(string sql)
        {
            int result = 0;
            try
            {
                using (OdbcConnection conn = GetConnection())
                {
                    using (OdbcCommand cmd = new OdbcCommand(sql, conn))
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public string ExecuteScalar(string sql)
        {
            string result;
            using (OdbcConnection conn = GetConnection())
            {
                OdbcCommand cmd = new OdbcCommand(sql, conn);
                object obj = cmd.ExecuteScalar();
                conn.Close();
                if (obj != null)
                {
                    result = obj.ToString();
                    return result;
                }
            }
            result = string.Empty;
            return result;
        }

        public bool CreateTable(string tableName, Dictionary<string, string> data)
        {
            bool result = false;
            if (!IsTableExist(tableName))
            {
                string text = "";
                foreach (KeyValuePair<string, string> current in data)
                {
                    text += string.Format(" {0} {1},", current.Key, current.Value);
                }
                text = text.Substring(0, text.Length - 1);
                try
                {
                    string sql = string.Format("CREATE TABLE {0}({1});", tableName, text);
                    ExecuteNonQuery(sql);
                    result = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return result;
        }

        public bool DropTable(string tableName)
        {
            string query = string.Format("DROP TABLE {0};", tableName);
            int result = ExecuteNonQuery(query);
            return result > 0;
        }

        public bool Update(string tableName, Dictionary<string, string> data, string where)
        {
            string text = "";
            bool result = true;
            if (data.Count >= 1)
            {
                foreach (KeyValuePair<string, string> current in data)
                {
                    text += string.Format(" [{0}] = {1},", current.Key, current.Value);
                }
                text = text.Substring(0, text.Length - 1);
            }
            try
            {
                ExecuteNonQuery(string.Format("update [{0}] set {1} where {2};", tableName, text, where));
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public bool Delete(string tableName, string where)
        {
            bool result = true;
            try
            {
                ExecuteNonQuery(string.Format("delete from [{0}] where {1};", tableName, where));
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public bool Insert(string tableName, Dictionary<string, string> data)
        {
            string columns = "";
            string values = "";
            bool result = true;
            foreach (KeyValuePair<string, string> current in data)
            {
                columns += string.Format(" [{0}],", current.Key);
                if (String.IsNullOrEmpty(current.Value))
                {
                    values += string.Format(" {0},", "NULL");
                }
                else
                {
                    values += string.Format(" {0},", current.Value);
                }
            }
            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);
            try
            {
                string sql = string.Format("insert into [{0}]({1}) values({2});", tableName, columns, values);
                ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public bool AddColumns(string tablename, string newcolumnname, string dataType)
        {
            string sql = string.Format("ALTER TABLE {0} ADD COLUMN {1} {2}", tablename, newcolumnname, dataType);
            bool result;
            try
            {
                int num = ExecuteNonQuery(sql);
                result = (num > 0);
            }
            catch
            {
                result = false;
            }
            return result;
        }


        public bool IsTableExist(string tablename)
        {
            bool result = false;
            using (OdbcConnection conn = GetConnection())
            {
                DataTable schema = conn.GetSchema("Tables");
                foreach (DataRow dataRow in schema.Rows)
                {
                    if (dataRow.ItemArray[2].ToString().Equals(tablename))
                    {
                        result = true;
                        break;
                    }
                    result = false;
                }
            }
            return result;
        }

        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                using (OdbcConnection conn = GetConnection())
                {
                    using (OdbcCommand cmd = new OdbcCommand(sql, conn))
                    {
                        using (OdbcDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\n Query: {1} \n Database: {2}", ex.Message, sql, DatabasePath));
                throw new Exception(string.Format("{0}\n Query: {1} \n Database: {2}", ex.Message, sql, DatabasePath));
            }
            return dt;
        }

        public DataTable GetDataTable(string sql, string tablename)
        {
            DataTable dt = new DataTable(tablename);
            try
            {
                using (OdbcConnection conn = GetConnection())
                {
                    OdbcCommand cmd = new OdbcCommand(sql, conn);
                    OdbcDataReader reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return dt;
        }

        public DataSet GetDataSet(string sql)
        {
            DataSet dataSet = new DataSet();
            using (OdbcConnection conn = GetConnection())
            {
                OdbcDataAdapter adapter = new OdbcDataAdapter(sql, conn);
                adapter.Fill(dataSet);
                conn.Close();
                conn.Dispose();
                adapter.Dispose();
            }
            return dataSet;
        }

        public DataTable GetDataTable(string sql, out OdbcDataAdapter oleDbAdapter)
        {
            DataTable result = new DataTable();
            DataSet dataSet = new DataSet();
            try
            {
                using (OdbcConnection conn = GetConnection())
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    OdbcDataAdapter adapter = new OdbcDataAdapter(sql, conn);
                    adapter.Fill(dataSet);
                    oleDbAdapter = adapter;
                    result = dataSet.Tables[0];
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public DataTable GetDataTable(string sql, object filename, string password)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (OdbcConnection conn = GetConnection(filename.ToString(), password))
                {
                    OdbcCommand cmd = new OdbcCommand(sql, conn);
                    OdbcDataReader reader = cmd.ExecuteReader();
                    dataTable.Load(reader);
                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return dataTable;
        }

        public List<string> GetTables()
        {
            List<string> list = new List<string>();
            using (OdbcConnection conn = GetConnection())
            {
                DataTable schema = conn.GetSchema("Tables");
                foreach (DataRow dataRow in schema.Rows)
                {
                    string table = (string)dataRow[2];
                    if (!string.IsNullOrEmpty(table))
                    {
                        list.Add(table);
                    }
                }
                conn.Close();
            }
            return list;
        }

        public string GetColumnValue(string tablename, string columnname, string condition)
        {
            string result = string.Empty;
            try
            {
                DataTable dataTable = GetDataTable(string.Format("Select {0} from {1} Where {2}", columnname, tablename, condition));
                if (dataTable != null)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            result = dataRow[0].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public List<string> GetColumns(string tablename)
        {
            List<string> list = new List<string>();
            string cmdText = string.Format("SELECT * FROM [{0}]", tablename);
            using (OdbcConnection conn = GetConnection())
            {
                OdbcCommand cmd = new OdbcCommand(cmdText, conn);
                OdbcDataReader reader = cmd.ExecuteReader();
                for (int i = 0; i <= reader.FieldCount - 1; i++)
                {
                    list.Add(reader.GetName(i).ToString());
                }
                conn.Close();
            }
            return list;
        }

        public int GetStatusCount(string tablename, string where_condition)
        {
            string cmdText = string.Format("SELECT COUNT(*) FROM [{0}] WHERE {1}", tablename, where_condition);
            using (OdbcConnection conn = GetConnection())
            {
                OdbcCommand cmd = new OdbcCommand(cmdText, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int GetStatusCount(string tablename)
        {
            string cmdText = string.Format("SELECT COUNT(*) FROM [{0}]", tablename);
            using (OdbcConnection conn = GetConnection())
            {
                OdbcCommand cmd = new OdbcCommand(cmdText, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool IsFieldExist(string columnName, string tableName)
        {
            bool result = false;
            using (OdbcConnection conn = GetConnection())
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                DataTable dataTable = new DataTable();
                string cmdText = "Select TOP 1 * from " + tableName;
                OdbcDataAdapter adapter = new OdbcDataAdapter(cmdText, conn);
                adapter.Fill(dataTable);
                int num = dataTable.Columns.IndexOf(columnName);
                result = (num != -1);
                dataTable.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public bool IsColumnExist(string columnName, string tableName)
        {
            bool result;
            using (OdbcConnection conn = GetConnection())
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                string cmdText = string.Format("TABLE_NAME='{0}' AND COLUMN_NAME='{1}'", tableName, columnName);
                DataTable schema = conn.GetSchema("COLUMNS");
                DataRow[] array = schema.Select(cmdText);
                conn.Close();
                conn.Dispose();
                if (array.Length > 0)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public bool IsExist(string tablename, string where_condition)
        {
            int num = 0;
            string cmdText = string.Format("SELECT COUNT(*) FROM [{0}] WHERE {1}", tablename, where_condition);

            using (OdbcConnection conn = GetConnection())
            {
                OdbcCommand cmd = new OdbcCommand(cmdText, conn);
                num = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return num >= 1;

        }
        public bool IsValueExist(string tablename, string column, string value)
        {
            string cmdText = string.Format("SELECT * FROM [{0}] WHERE {1}={2}", tablename, column, value);
            DataTable dt = GetDataTable(cmdText);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsEmpty(string tablename)
        {
            DataTable dataTable = GetDataTable("SELECT * FROM " + tablename);
            if (dataTable == null)
            {
                return true;
            }
            return dataTable.Rows.Count <= 0;
        }

        public int GetLastID(string tableName)
        {
            string cmdText = string.Format("SELECT TOP 1 ID FROM {0} ORDER BY ID DESC;", tableName);
            DataTable dataTable = GetDataTable(cmdText);
            int result;
            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    int num = 0;
                    if (int.TryParse(dataRow[0].ToString(), out num))
                    {
                        result = num;
                        return result;
                    }
                }
            }
            result = 0;
            return result;
        }
    }
}

