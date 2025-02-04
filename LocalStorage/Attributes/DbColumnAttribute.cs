using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarsier.Database.Enums;

namespace RobinsonMallLinkingWBOX.LocalStorage.Attributes
{
    #region Attributes
    /// <summary>
    /// Use this attribute to decorate the properties on your model class.
    /// Only those properties that are having exactly the same column name of a DB table.
    /// </summary>
    public class DbColumnAttribute : Attribute
    {
        /// <summary>
        /// Set true if implicit conversion is required.
        /// </summary>
        public bool Convert { get; set; }
        /// <summary>
        /// Set true if ID is auto increment
        /// </summary>
        public bool AutoIncrement { get; set; }
        /// <summary>
        /// Set true if the property is primary key in the table
        /// </summary>
        public bool IsPrimary { get; set; }
        /// <summary>
        /// Denotes if the field is an identity type or not.
        /// </summary>
        public bool IsIdentity { get; set; }
        /// <summary>
        /// Set column data type
        /// </summary>
        public ColType ColumnType { get; set; }
        /// <summary>
        /// Indicate column not null
        /// </summary>
        public bool NotNull { get; set; }
        /// <summary>
        /// Set default value
        /// </summary>
        public object DefaultValue { get; set; } = string.Empty;

    }
    #endregion
}
