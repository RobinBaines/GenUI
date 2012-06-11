////////////////////////////////////////////////////////////////////
//Copyright @2008 Robin Baines
//June - Oct 2008
//dbUtilities.
//Generate user interface in basic from database.
////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace GenUI
{
    class dbUtilities
    {
        public bool blnIsBoolean(Column c)
        {
            bool blnRet = false;
            if (c.DataType.SqlDataType == SqlDataType.UserDefinedDataType)
                blnRet = c.DataType.Name.ToLower().Contains("bool");
            else
            {
            if (c.DataType.SqlDataType == SqlDataType.Bit )
                blnRet = true;
            }
            return blnRet;
        }

        public string strGetWidth(Column c)
        {
            string strWidth = "GENWIDTH";
            if (c.DataType.Name.ToUpper().Contains("TIMESTAMP")) strWidth = "MONTHWIDTH";
            if (c.DataType.Name.ToUpper().Contains("MONTH")) strWidth = "MONTHWIDTH";
            if (c.DataType.Name.ToUpper().Contains("BOOL")) strWidth = "FLOATWIDTH";
            if (c.DataType.Name.ToUpper().Contains("MONEY")) strWidth = "FLOATWIDTH";
            if (c.DataType.Name.ToUpper().Contains("INT")) strWidth = "FLOATWIDTH";
            if (c.DataType.Name.ToUpper().Contains("FLOAT")) strWidth = "FLOATWIDTH";
            if (c.DataType.Name.ToUpper().Contains("REMARK")) strWidth = "REMARKWIDTH";
            return strWidth;
        }
        /// <summary>
        /// Use database table property Type to decide width of the field in a datagrid.
        /// </summary>
        public string strGetColumnWidth(Column c, bool blnUseType)
        {
            string strWidth = "REMARKWIDTH";

            //Return the const names used in dgColumns to define the width of columns in datagrids.
            //UseType means return the type defined in SQL.
            if (blnUseType == true)
            {
                if (c.DataType.SqlDataType == SqlDataType.UserDefinedDataType)
                    strWidth = c.DataType.Name;
                else
                {
                //    if (c.DataType.SqlDataType == SqlDataType.Numeric)
                        strWidth = c.DataType.SqlDataType.ToString();
                    
                }
            }
            else
            {
                if (c.DataType.Name.Contains("_TYPE_TIMESTAMP")) strWidth = "MONTHWIDTH";
                if (c.DataType.Name.Contains("MONTH")) strWidth = "MONTHWIDTH";
                if (c.DataType.Name.Contains("_BOOL")) strWidth = "FLOATWIDTH";
                if (c.DataType.Name.Contains("_TYPE_INT")) strWidth = "FLOATWIDTH";
                if (c.DataType.Name.Contains("_TYPE_FLOAT")) strWidth = "FLOATWIDTH";
                if (c.DataType.Name.Contains("_TYPE_NSTRINGSHORT")) strWidth = "SMALLWIDTH";
                if (c.DataType.Name.Contains("_TYPE_REVSTRING")) strWidth = "SMALLWIDTH";
                if (c.DataType.Name.Contains("_TYPE_MATERIAL")) strWidth = "MATERIALWIDTH";
                if (c.DataType.Name.Contains("_TYPE_NSTRING")) strWidth = "MATERIALWIDTH";
                if (c.DataType.Name.Contains("_TYPE_NSTRINGLONG")) strWidth = "SMALLREMARKWIDTH";
                if (c.DataType.Name.Contains("float")) strWidth = "FLOATWIDTH";
                if (c.DataType.Name.Contains("int")) strWidth = "FLOATWIDTH";

            }
            return strWidth;
        }

        /// <summary>
        /// strExtendedProperty.
        /// Return true if the Extended property has the Value.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public bool blnExtendedProperty(Column c, string strPropertyName, string strValue)
        {
            bool blnRet = false;
            ExtendedPropertyCollection EP = c.ExtendedProperties;
            int i = 0;
            ExtendedProperty row;
            while (i < EP.Count)
            {
                row = EP[i];
                if (EP[i].Name.ToString() == strPropertyName)
                {
                    blnRet = (EP[i].Value.ToString() == strValue);
                }
                i++;
            }
            return blnRet;
        }

        /// <summary>
        /// Return true if the Extended property of the Table has the Value.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="strPropertyName"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public bool blnExtendedProperty(Table t, string strPropertyName, string strValue)
        {
            bool blnRet = false;
            ExtendedPropertyCollection EP = t.ExtendedProperties;
            int i = 0;
            ExtendedProperty row;
            while (i < EP.Count)
            {
                row = EP[i];
                if (EP[i].Name.ToString() == strPropertyName)
                {
                    blnRet = (EP[i].Value.ToString() == strValue);
                }
                i++;
            }
            return blnRet;
        }

        /// <summary>
        /// Remove [] and spaces from string name.
        /// </summary>
        public string CleanUp(string strFieldName)
        {
            string strName;
            strName = strFieldName.Replace("[", "");
            strName = strName.Replace("]", "");
            strName = strName.Replace(" ", "");
            return strName;
        }
        /// <summary>
        /// Use the table property type to decide on the decimal presentation in a datagrid.
        /// </summary>
        public  string strGetFormat(Column c, bool blnUseType)
        {

            //Return the const names used in dgColumns to define the width of columns in datagrids.
            string strWidth = "\"\"";
            if (blnUseType == true)
            {
                if (c.DataType.SqlDataType == SqlDataType.UserDefinedDataType)
                    strWidth = "MainDefs.strGetFormat(\"" + c.DataType.Name + "\")";
                else
                {
                    if (c.DataType.SqlDataType == SqlDataType.Numeric)
                        strWidth = "MainDefs.strGetFormat(\"" + c.Name + "\")";
                    else
                        if (c.DataType.SqlDataType == SqlDataType.Money)
                            strWidth = "MainDefs.strGetFormat(\"" + c.Name + "\")";
                        else strWidth = "MainDefs.strGetFormat(\"" + c.DataType.SqlDataType.ToString() + "\")";
                }
            }
            else
            {
                if (c.DataType.Name.Contains("_TYPE_BOOL")) strWidth = "\"\"";
                if (c.DataType.Name.Contains("_TYPE_INT")) strWidth = "\"N0\"";
                if (c.DataType.Name.Contains("_TYPE_FLOAT")) strWidth = "\"N2\"";
                if (c.DataType.Name.Contains("float")) strWidth = "\"N2\"";
                if (c.DataType.Name.Contains("money")) strWidth = "\"N2\"";
                if (c.DataType.Name.Contains("int")) strWidth = "\"N0\"";
            }
            return strWidth;
        }

        /// <summary>
        /// Return index of the FK in t.ForeignKeys collection if the column in c is part of a foreign key.
        /// Otherwise return -1.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public string strFCField(Table t, Column c)
        {
            int i = 0;
            int columns = 0;
            string strRet = "";
            //if (t.Name == "IHWTag")
            {

                while (i < t.ForeignKeys.Count)
                {
                    //Second check is not necessary because the FKName is unique.
                    columns = 0;
                    while (columns < t.ForeignKeys[i].Columns.Count)
                    {
                        string strReferencedTable = t.ForeignKeys[i].ReferencedTable.ToString();
                        string strT = CleanUp(t.ForeignKeys[i].Columns[columns].ToString());
                        if (c.Name == strT)
                            strRet = strReferencedTable;
                        columns++;
                    }

                    i++;
                }
            }
            return strRet;
        }
        
        public string strGetReferencedTable(Table t, Column c)
        {
            string strRet = "";
            string strType = t.GetType().ToString();
            if (strType.Contains("Table"))
            {
                strRet = strFCField((Table)t, c);
            }
            return strRet;
        }


       
        ///
        ///strGetPK 
        /// 
        public string strGetPK(Table t)
        {
            string strT;
            if (t == null) return ""; 
            if (t.Name == "Lokatie")
                strT = t.Name;
            string strRet = "";
            foreach (Index index in t.Indexes)
            {
                //Needed to Reference .SmoEnum and .SQLEnum to get this into the smo namespace.
                if (index.IndexKeyType == IndexKeyType.DriPrimaryKey)
                {
                    foreach (IndexedColumn iC in index.IndexedColumns)
                    {
                        
                        if (strRet.Length != 0) strRet += ".";
                        strRet += iC.Name;
                    }
                }
            }
            return strRet;
        }

        ///
        ///IsThePK
        /// Return true if the column is the PK of the table.
        /// 
        public bool IsThePK(Table t, Column c)
        {
            bool blnRet = false;
            foreach (Index index in t.Indexes)
            {
                //Needed to Reference .SmoEnum and .SQLEnum to get this into the smo namespace.
                if (index.IndexKeyType == IndexKeyType.DriPrimaryKey)
                {
                    if (index.IndexedColumns.Count == 1)
                    {
                        foreach (IndexedColumn iC in index.IndexedColumns)
                        {
                            if (c.Name == iC.Name) blnRet = true;
                        }
                    }
                }
            }
            return blnRet;
        }
    }
}
