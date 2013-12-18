using System;
using Dev2.Data.Binary_Objects;

namespace Dev2.DataList.Contract.Binary_Objects
{
    [Serializable]
    public class Dev2Column 
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the column description.
        /// </summary>
        /// <value>
        /// The column description.
        /// </value>
        public string ColumnDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is editable; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets the argument direction.
        /// </summary>
        /// <value>
        /// The argument direction.
        /// </value>
        public enDev2ColumnArgumentDirection ColumnIODirection { get; set; }

        #endregion Properties
        
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Dev2Column" /> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="columnDesciption">The column desciption.</param>
        internal Dev2Column(string columnName, string columnDesciption)
            : this(columnName,columnDesciption,true)
        {          
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dev2Column" /> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="ioDir">The io dir.</param>
        internal Dev2Column(string columnName, enDev2ColumnArgumentDirection ioDir)
            : this(columnName, string.Empty, true, ioDir)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Dev2Column" /> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="columnDesciption">The column desciption.</param>
        /// <param name="ioDir">The io dir.</param>
        internal Dev2Column(string columnName, string columnDesciption, enDev2ColumnArgumentDirection ioDir)
            : this(columnName,columnDesciption,true, ioDir)
        {          
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="Dev2Column" /> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="columnDesciption">The column desciption.</param>
        /// <param name="isEditable">if set to <c>true</c> [is editable].</param>
        internal Dev2Column(string columnName, string columnDesciption,bool isEditable) : this(columnName, columnDesciption, isEditable, enDev2ColumnArgumentDirection.None)
        {
        }

        // <summary>
        /// <summary>
        /// Initializes a new instance of the <see cref="Dev2Column" /> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="columnDesciption">The column desciption.</param>
        /// <param name="isEditable">if set to <c>true</c> [is editable].</param>
        /// <param name="dir">The column dir.</param>
        internal Dev2Column(string columnName, string columnDesciption, bool isEditable, enDev2ColumnArgumentDirection dir)
        {
            ColumnName = columnName;
            ColumnDescription = columnDesciption;
            IsEditable = isEditable;
            ColumnIODirection = dir; 
        }

        #endregion Ctor       
    }
}
