////////////////////////////////////////////////////////////////////
//Copyright @2008 Robin Baines
//June - Oct 2008
//LookUps.
////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.IO;

namespace GenUI
{
    class Lookups
    {
        static dbUtilities ut = new dbUtilities();

        /// <summary>
        /// Create OpenForm which opens a form when called with the name of the form.
        /// So 'HWTag' will open form HWTag.
        /// </summary>
        public string strCreateOpenForm(Database db, StreamWriter strW)
        {
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("'Name: Class OpenForm.vb.");
            strW.WriteLine("'Function: Open a Form or bring to front if already open.");
            strW.WriteLine("'Copyright Robin Baines 2008. All rights reserved.");
            strW.WriteLine("'Created " + System.DateTime.Today + ".");
            strW.WriteLine("'Notes: ");
            strW.WriteLine("'Modifications:");
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("Imports Utilities");
            strW.WriteLine("Public Class OpenForm");

            strW.WriteLine("Dim ut As New Utilities.Utilities");
            strW.WriteLine("Public Sub New()");
            strW.WriteLine("End Sub");

            //This version of ShowForm gets the Header of the form from the Form name.
            strW.WriteLine("Public Sub ShowForm(ByVal pParent As Form, ByVal strForm As String, ByVal MainDefs As MainDefinitions, Fields as Dictionary(Of String, String))");
            strW.WriteLine("Dim strHeader = MainDefs.strGetTableText(strForm)");
            strW.WriteLine("ShowForm(pParent, strHeader, strForm, MainDefs, Fields)");
            strW.WriteLine("    End Sub");

            strW.WriteLine("Public Sub ShowForm(ByVal pParent As Form, ByVal strHeader As String, ByVal strForm As String, ByVal MainDefs As MainDefinitions, Fields as Dictionary(Of String, String))");

            //20090408 RPB altered ToString ProjectGenericForm from GenericForm.
            strW.WriteLine("Dim f As ProjectGenericForm");
            strW.WriteLine("f = ut.frmCanShowMDIForm(pParent, strForm.ToLower, True)");
            strW.WriteLine("If f Is Nothing Then");
            strW.WriteLine("Select Case strForm.ToLower");
            try
            {
                foreach (Table t in db.Tables)
                {
                    if (ut.blnExtendedProperty(t, "CreateForm", "No") == false)
                    //if (t.Name != "bUsers" && t.Name != "bFormLevels" && t.Name != "bForms" && t.Name != "bLevels" && t.Name != "bLog" && t.Name != "bParameters" && t.Name != "sysdiagrams")
                    {
                        strW.WriteLine("Case \"" + t.Name.ToLower() + "\"");
                        //RO = false as override
                        strW.WriteLine("f = New form_" + t.Name + "(Nothing, MainDefs.dsFormLevels, strForm, MainDefs, Fields, True, false)");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
            strW.WriteLine("End Select");
            strW.WriteLine("If Not f Is Nothing Then");
            strW.WriteLine("ut.DisplayAForm(pParent, f, strHeader, strForm.ToLower, FormWindowState.Normal)");
            strW.WriteLine("End If");
            strW.WriteLine("else");
            strW.WriteLine("f.SetFilter(Fields)");
            strW.WriteLine("f.Text = strHeader");
            strW.WriteLine("End If");
            strW.WriteLine("    End Sub");
            strCreateOpenFormWithField(db, strW);
            strW.WriteLine("End Class");
            return "";
        }

        /// <summary>
        /// Create OpenForm.ShowFormWithField to open a form when called with the name of a field.
        /// </summary>

        string strCreateOpenFormWithField(Database db, StreamWriter strW)
        {
            strW.WriteLine("Public Sub ShowFormWithField(ByVal pParent As Form, _");
            strW.WriteLine("ByVal strForm As String, _");
            strW.WriteLine("ByVal strField As String, _");
            strW.WriteLine("ByVal MainDefs As MainDefinitions, _");
            strW.WriteLine("ByVal Fields As Dictionary(Of String, String))");
            strW.WriteLine("Dim f As GenericForm");
            strW.WriteLine("Dim strCase As String");
            strW.WriteLine("strCase = strForm.ToLower() + \" \" + strField.ToLower()");

            strW.WriteLine("Select Case strCase");
            try
            {
                foreach (Table t in db.Tables)
                {
                    //strW.WriteLine("f = ut.frmCanShowMDIForm(pParent, strForm.ToLower, True)");
                    //strW.WriteLine("If f Is Nothing Then");
                    //Check whether an extended property has been created to prevent this table being included.
                    if (ut.blnExtendedProperty(t, "CreateForm", "No") == false)
                    {
                        // if (t.Name == "IHWTag")
                        {
                            int i = 0;
                            while (i < t.ForeignKeys.Count)
                            {
                                //Second check is not necessary because the FKName is unique.
                                int columns = 0;
                                while (columns < t.ForeignKeys[i].Columns.Count)
                                {
                                    string strRefTable = t.ForeignKeys[i].ReferencedTable.ToString();
                                    string strRefColumn = ut.CleanUp(t.ForeignKeys[i].Columns[columns].ToString());
                                    strW.WriteLine("Case \"" + t.Name.ToLower() + " " + strRefColumn.ToLower() + "\"");
                                    strW.WriteLine("f = ut.frmCanShowMDIForm(pParent,  \"" + strRefTable.ToLower() + "\", True)");
                                    strW.WriteLine("If f Is Nothing Then");
                                    strW.WriteLine("f = New form_" + strRefTable + "(Nothing, MainDefs.dsFormLevels,  \"" + strRefTable + "\", MainDefs, Fields, False, false)");
                                    strW.WriteLine("If Not f Is Nothing Then");
                                    strW.WriteLine("ut.DisplayAForm(pParent, f, \"" + strRefTable + "\",  \"" + strRefTable.ToLower() + "\")");
                                    strW.WriteLine("End If");
                                    strW.WriteLine("Else");
                                    strW.WriteLine("f.SetFilter(Fields)");
                                    strW.WriteLine("f.Text = \"" + strRefTable.ToLower() + "\"");
                                    strW.WriteLine("End If");

                                    columns++;
                                }
                                i++;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
            strW.WriteLine("End Select");
            strW.WriteLine("End Sub");
            return "";
        }
        /// <summary>
        /// Returns the display name of a table.
        /// The database table name is returned as the default value.
        /// The user should modify this by adding the display names of the tables. 
        /// So be careful to generate this only when necessary.
        /// </summary>
        /// <param name="db">The database</param>
        /// <param name="strW">The output stream</param>
        public string strCreateListOfTablesFile(Database db, StreamWriter strW)
        {
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("'Name: Class GetTableText.vb.");
            strW.WriteLine("'Function: Is a list of all tables used. Replace the strTableText with the displayed table name.");
            strW.WriteLine("'The database table name is returned as the default value.");
            strW.WriteLine("'Copyright Robin Baines 2008. All rights reserved.");
            strW.WriteLine("'Created " + System.DateTime.Today + ".");
            strW.WriteLine("'Notes: ");
            strW.WriteLine("'Modifications:");
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("Public Class GetTableText");
            strW.WriteLine("Public Sub New()");
            strW.WriteLine("End Sub");
            strW.WriteLine("Public Function strGetTableText(ByVal strTableName As String) As String");
            strW.WriteLine("Dim strTableText As String = strTableName");
            strW.WriteLine("Select Case strTableName.ToLower()");
            List<string> tables = new List<string>();
            foreach (Table t in db.Tables)
            {
                try
                {
                    if (tables.Contains(t.Name) != true)
                        tables.Add(t.Name);
                }
                catch { }
            }
            foreach (string table in tables)
            {
                strW.WriteLine("Case \"" + table.ToLower() + "\"");
                strW.WriteLine("strTableText = \"" + table + "\"");
            }
            strW.WriteLine("End Select");
            strW.WriteLine("Return strTableText");
            strW.WriteLine("End Function");
            strW.WriteLine("End Class");
            return "";
        }

        /// <summary>
        /// Returns the Print String for a Table.Column.
        /// The empty string is returned as the default value.
        /// The user should modify this by adding the display names of the tables. 
        /// So be careful to generate this only when necessary.
        /// </summary>
        /// <param name="db">The database</param>
        /// <param name="strW">The output stream</param>
        public string strCreateListOfPrintStringsFile(Database db, StreamWriter strW)
        {
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("'Name: Class GetPrintDisplayVisibility.vb.");
            strW.WriteLine("'Function: Is a list of all tables used. Replace the strTableText with the displayed table name.");
            strW.WriteLine("'The database table name is returned as the default value.");
            strW.WriteLine("'Copyright Robin Baines 2008. All rights reserved.");
            strW.WriteLine("'Created " + System.DateTime.Today + ".");
            strW.WriteLine("'Notes: ");
            strW.WriteLine("'Modifications:");
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("Public Class GetPrintDisplayVisibility");
            strW.WriteLine("Public Sub New()");
            strW.WriteLine("End Sub");
            List<string> table_columns = new List<string>();
            string strT = "";
            foreach (Table t in db.Tables)
            {
                try
                {
                    foreach (Column c in t.Columns)
                    {
                        strT = t.Name.ToLower() + "." + c.Name.ToLower();
                        if (table_columns.Contains(strT) != true)
                            table_columns.Add(strT);
                    }
                }
                catch { }
            }
            strW.WriteLine("Public Function strGetPrintString(ByVal MainDefs as MainDefinitions, ByVal strTableName As String, ByVal strColumnName As String) As String");
            strW.WriteLine("Dim strPrintString As String = \"\"");
            strW.WriteLine("Select Case strTableName.ToLower() + \".\" + strColumnName.ToLower()");

            foreach (string table_column in table_columns)
            {
                strW.WriteLine("Case \"" + table_column + "\"");
                strW.WriteLine("strPrintString = \"\"");
            }
            strW.WriteLine("End Select");
            strW.WriteLine("Return strPrintString");
            strW.WriteLine("End Function");

            strW.WriteLine("Public Function blnGetVisibility(ByVal strTableName As String, ByVal strColumnName As String) As Boolean");
            strW.WriteLine("Dim blnVisible As Boolean = true");
            strW.WriteLine("Select Case strTableName.ToLower() + \".\" + strColumnName.ToLower()");

            foreach (string table_column in table_columns)
            {
                strW.WriteLine("Case \"" + table_column + "\"");
                strW.WriteLine("blnVisible = true");
            }
            strW.WriteLine("End Select");
            strW.WriteLine("Return blnVisible");
            strW.WriteLine("End Function");
            strW.WriteLine("End Class");
            return "";
        }
        /// <summary>
        /// strCreateListOfColumnsFile
        /// Returns the header text to be shown in a column of a table. 
        /// This simple version uses the field name not the table name.
        /// Returning "" uses the field name as column header.
        /// The user should modify this by adding the column names. 
        /// So be careful to generate this only when necessary.
        /// </summary>
        /// <param name="db">The database</param>
        /// <param name="strW">The output stream</param>
        public string strCreateListOfColumnsFile(Database db, StreamWriter strW)
        {
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("'Name: Class GetColumnHeader.vb.");
            strW.WriteLine("'Function: The user should enter the column headers at each case statement.");
            strW.WriteLine("'The function return the empty string which means the field name is displayed in the column header ");
            strW.WriteLine("'Copyright Robin Baines 2008. All rights reserved.");
            strW.WriteLine("'Created " + System.DateTime.Today + ".");
            strW.WriteLine("'Notes: ");
            strW.WriteLine("'Modifications:");
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("Public Class GetColumnHeader");
            strW.WriteLine("Public Sub New()");
            strW.WriteLine("End Sub");
            strW.WriteLine("Public Function strGetColumnHeader(ByVal strFieldName As String) As String");
            strW.WriteLine("Dim strColumnHeader As String = \"\"");
            strW.WriteLine("Select Case strFieldName");
            string strT;
            List<string> cols = new List<string>();
            foreach (Table t in db.Tables)
            {
                //Dictionary<string, int> cols = new Dictionary<string, string>();

                try
                {
                    foreach (Column c in t.Columns)
                    {
                        if (c.Name == "est")
                            strT = c.Name;
                        if (cols.Contains(c.Name) != true)
                            cols.Add(c.Name);
                    }

                }
                catch { }
            }
            foreach (string col in cols)
            {
                strW.WriteLine("Case \"" + col + "\"");
                strW.WriteLine("return \"\"");
            }
            strW.WriteLine("End Select");
            strW.WriteLine("Return strColumnHeader");
            strW.WriteLine("End Function");
            strW.WriteLine("End Class");
            return "";
        }
        /// <summary>
        /// Returns the Print String for a Table.Column.
        /// The empty string is returned as the default value.
        /// The user should modify this by adding the display names of the tables. 
        /// So be careful to generate this only when necessary.
        /// </summary>
        /// <param name="db">The database</param>
        /// <param name="strW">The output stream</param>
        public string strCreateListOfDisplayMembers(Database db, StreamWriter strW)
        {
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("'Name: Class GetDisplayMember.vb.");
            strW.WriteLine("'Function: Is a list of all tables and PK field if there is just one field in the PK.");
            strW.WriteLine("'Use to return the DisplayMembers when this PK is referenced by an FK.");
            strW.WriteLine("'The PK is returned as the default value.");
            strW.WriteLine("'Copyright Robin Baines 2008. All rights reserved.");
            strW.WriteLine("'Created " + System.DateTime.Today + ".");
            strW.WriteLine("'Notes: ");
            strW.WriteLine("'Modifications:");
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("Public Class GetDisplayMember");
            strW.WriteLine("Public Sub New()");
            strW.WriteLine("End Sub");
            List<string> table_columns = new List<string>();
            string strT = "";
            foreach (Table t in db.Tables)
            {
                try
                {
                    foreach (Column c in t.Columns)
                    {
                        if (ut.IsThePK(t, c) == true)
                        {
                            strT = t.Name.ToLower() + "." + c.Name.ToLower();
                            if (table_columns.Contains(strT) != true)
                                table_columns.Add(strT);
                        }
                    }
                }
                catch { }
            }
            strW.WriteLine("Public Function strGetDisplayMember(ByVal MainDefs as MainDefinitions, ByVal strTableName As String, ByVal strColumnName As String) As String");
            strW.WriteLine("Dim strDisplayMember As String = \"\"");
            strW.WriteLine("Select Case strTableName.ToLower() + \".\" + strColumnName.ToLower()");

            foreach (string table_column in table_columns)
            {
                    strW.WriteLine("Case \"" + table_column + "\"");
                    strW.WriteLine("strDisplayMember = strColumnName");
            }
            strW.WriteLine("End Select");
            strW.WriteLine("Return strDisplayMember");
            strW.WriteLine("End Function");
            strW.WriteLine("End Class");
            return "";
        }
    }
}
