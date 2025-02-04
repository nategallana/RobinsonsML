using RobinsonMallLinkingWBOX.Constants;
using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.LocalStorage.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tarsier.Database.SQLite;

namespace RobinsonMallLinkingWBOX.LocalStorage
{
    public abstract class BaseStorage<T> where T : class, new()
    {
        private SQLiteHelper sqlite;
        private SQLiteTable table;
        private string defaultTable = new T().GetTableName();

        public string Table
        {
            get
            {
                return defaultTable;
            }
        }
        #region Constructor
        public BaseStorage(string filename)
        {
            sqlite = new SQLiteHelper(filename);
            CreateTable(new T());
        }
        #endregion
        public DbColumnAttribute GetAttrib(PropertyInfo pi)
        {
            foreach (object attrib in pi.GetCustomAttributes(typeof(DbColumnAttribute), false))
            {
                DbColumnAttribute entityAttrib = attrib as DbColumnAttribute;
                if (entityAttrib != null)
                {
                    return entityAttrib;
                }
            }
            return null;
        }
        public void CreateTable(T entity)
        {
            table = new SQLiteTable(defaultTable);
            Dictionary<string, string> tableSchema = new Dictionary<string, string>();

            foreach (PropertyInfo pi in entity.GetType().GetProperties())
            {
                var customAttribute = GetAttrib(pi);
                if (customAttribute != null)
                {
                    if (customAttribute.AutoIncrement)
                    {
                        table.AddColumn(new SQLiteColumn(pi.Name, true));
                    }
                    else
                    {
                        table.AddColumn(new SQLiteColumn(pi.Name, customAttribute.ColumnType));
                    }
                }
            }
            sqlite.CreateTable(table);
        }

        public void Add(T entity, bool overwriteNull)
        {

            Dictionary<string, object> data = new Dictionary<string, object>();
            string code = string.Empty;
            foreach (PropertyInfo pi in entity.GetType().GetProperties())
            {
                object value = pi.GetValue(entity, null);
                if (value != null && !string.IsNullOrEmpty(Convert.ToString(value)))
                {
                    if (!pi.Name.Equals("ID"))
                    { //Ignore ID field because of autoincrement attribute
                        data.Add(pi.Name, value);
                    }
                }
                else
                {
                    DbColumnAttribute attrib = GetAttrib(pi);
                    if (attrib.DefaultValue != null)
                    {
                        data.Add(pi.Name, attrib.DefaultValue);
                    }
                    else
                    {
                        if (overwriteNull)
                        {
                            data.Add(pi.Name, null);
                        }
                    }
                }
            }
            sqlite.Insert(defaultTable, data);
        }

        public void Update(T entity, string id, bool overwriteNull)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                string code = string.Empty;
                foreach (PropertyInfo pi in entity.GetType().GetProperties())
                {
                    object value = pi.GetValue(entity, null);
                    if (value != null && !string.IsNullOrEmpty(Convert.ToString(value)))
                    {
                        if (!pi.Name.Equals("ID"))
                        { //Ignore ID field because of autoincrement attribute
                            data.Add(pi.Name, value);
                        }
                    }
                    else
                    {
                        if (overwriteNull)
                        {
                            data.Add(pi.Name, null);
                        }
                    }
                }
                sqlite.Update(defaultTable, data, "ID", id);
            }
        }

        public void UpdateByDate(Dictionary<string, object> data, string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (data != null && data.Count > 0)
                {
                    sqlite.Update(defaultTable, data, "ID", id);
                }
            }
        }

        public void UpdateByDate(T entity, string date, bool overwriteNull)
        {
            if (!string.IsNullOrEmpty(date))
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                string code = string.Empty;
                foreach (PropertyInfo pi in entity.GetType().GetProperties())
                {
                    object value = pi.GetValue(entity, null);
                    if (value != null && !string.IsNullOrEmpty(Convert.ToString(value)))
                    {
                        if (!pi.Name.Equals("ID"))
                        { //Ignore ID field because of autoincrement attribute
                            data.Add(pi.Name, value);
                        }
                    }
                    else
                    {
                        if (overwriteNull)
                        {
                            data.Add(pi.Name, null);
                        }
                    }
                }
                sqlite.Update(defaultTable, data, "TRN_DATE", date);
            }
        }

        public void Update(Dictionary<string, object> data, string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (data != null && data.Count > 0)
                {
                    sqlite.Update(defaultTable, data, "ID", id);
                }
            }
        }

        public void UpdateByDocket(Dictionary<string, object> data, string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (data != null && data.Count > 0)
                {
                    sqlite.Update(defaultTable, data, "ID", id);
                }
            }
        }

        public bool TagAsDelete(string id)
        {
            try
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                data.Add("Active", false);
                sqlite.Update(defaultTable, data, "ID", id);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool DeleteById(string id)
        {
            return Delete("ID=" + id);
        }

        public bool Delete(string whereCondition)
        {
            return sqlite.Delete(defaultTable, whereCondition);
        }

        public bool ClearAll()
        {
            try
            {
                if (sqlite.IsTableExist(defaultTable))
                {
                    sqlite.DropTable(defaultTable);
                    return true;
                }
            }
            catch
            {
            }
            return false;

        }

        public bool IsExistValue(string columnName, string columnValue)
        {
            string sqlQuery = string.Format(Queries.SELECT_TABLE_WHERE, defaultTable, columnName + "= '" + columnValue + "'");
            DataTable dt = GetDataTable(sqlQuery);
            if (dt != null)
            {
                return dt.Rows.Count > 0;
            }
            return false;
        }

        public string GetColumnValue(string columnname, string condition)
        {
            string result = string.Empty;
            try
            {
                DataTable dataTable = GetDataTable(string.Format("Select {0} from {1} Where {2}", columnname, defaultTable, condition));
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

        public DataTable GetDataTable(bool orderDesc)
        {
            if (string.IsNullOrEmpty(defaultTable))
            {
                return null;
            }
            if (orderDesc)
            {
                return sqlite.Select(string.Format(Queries.SELECT_TABLE_DESC, defaultTable, "ID"));
            }
            return sqlite.GetDataTable(defaultTable);
        }

        public DataTable GetDataTable(string sqlQuery)
        {
            return sqlite.Select(sqlQuery);
        }

        public int RowsCount()
        {
            DataTable dt = GetDataTable(false);
            if (dt != null)
            {
                return dt.Rows.Count;
            }
            return 0;
        }
    }
}
