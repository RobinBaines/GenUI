////////////////////////////////////////////////////////////////////
//Copyright @2008 Robin Baines
//June - Oct 2008
//GenUI.
//Generate user interface in basic from database.
//20091130 Added MaxLength
//20130212 Add try catch over AdjustColumns.
//20170907 Change default value for Bits to true or false. 
////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.IO;
using System.Collections.ObjectModel;
using GenLib;

namespace GenUI
{
    class GenUI
    {
        static String strDatabase;
        //static String strApplication;
        static dbUtilities ut = new dbUtilities();
        static Lookups lookups = new Lookups();


        static Table strGetTable(Database db, string strTableName)
        {
            foreach (Table theTable in db.Tables)
            {
                if (theTable.Name == strTableName)
                    return theTable;
            }
            return null;
        }
        /// <summary>
        /// Create a file to define the columns to be dispayed in a datagrid.
        /// </summary>
        static string strCreateTableFile(Database db,
            StreamWriter strW, 
            TableViewBase theTables, 
            bool blnView, 
            string strTable, 
            string strDG, 
            string TheDataSet)
        {
            //if (strTable == "b_machine_medication_list")
            {

            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("'Name: Module gen_" + TheDataSet + "_" + strTable + ".vb.");
            strW.WriteLine("'Function: ");
            strW.WriteLine("'Copyright Robin Baines 2008. All rights reserved.");
            strW.WriteLine("'Created " + System.DateTime.Today + ".");
            strW.WriteLine("'Notes: ");
            strW.WriteLine("'Modifications:");
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("Imports Utilities");

            //20100214 RPB added these 2.
            strW.WriteLine("Imports System.Windows.Forms");
            strW.WriteLine("Imports System.Drawing");

            if (blnIncludeDataSetInClassName == true) strW.WriteLine("Public Class " + TheDataSet + "_" + strTable);
            else strW.WriteLine("Public Class " + strTable);

            strW.WriteLine("Inherits dgColumns");

            //if the object contains comboboxes then it must implement disposable.
            if (theTables.Columns.Count != 0 && blnView == false)
            {
                strW.WriteLine("Implements IDisposable");
            }

            Console.WriteLine(" Table: " + strTable);
            //Console.WriteLine("  Columns: ");
            string strLastReferencedTable = "";
            string strColName = "";
            try
            {
            //Declare the DataGrid Columns.
            foreach (Column theColumn in theTables.Columns)
            {

                strColName = theColumn.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                if (blnView == false && ut.strGetReferencedTable((Table)theTables, theColumn).Length != 0)
                    strW.WriteLine("Friend WithEvents " + strDG + strColName + " As System.Windows.Forms.DataGridViewComboBoxColumn");
                else
                {
                    if (ut.blnIsBoolean(theColumn) == true)
                        strW.WriteLine("Friend WithEvents " + strDG + strColName + " As System.Windows.Forms.DataGridViewCheckBoxColumn");
                    else
                        strW.WriteLine("Friend WithEvents " + strDG + strColName + " As System.Windows.Forms.DataGridViewTextBoxColumn");
                }
            }
                       
            //Create binding sources to combobox dropdown tables.
            foreach (Column theColumn in theTables.Columns)
                {
                    //Replace underscores in field names as they generate errors in generated code.
                    strColName = theColumn.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                    string strReferencedTable = "";
                    if (blnView == false) 
                        strReferencedTable = ut.strGetReferencedTable((Table)theTables, theColumn).Replace("-", "_").Replace(" ", "_").Replace("%", "_");

                    if (strReferencedTable.Length != 0)
                    {
                        if (strLastReferencedTable != strReferencedTable)
                        {
                            strLastReferencedTable = strReferencedTable;
                            strW.WriteLine("Public bs" + strReferencedTable + " As BindingSource");
                            strW.WriteLine("Friend WithEvents " + strReferencedTable + "TableAdapter As " 
                                 + TheDataSet + "TableAdapters." + strReferencedTable + "TableAdapter");
                        }
                    }
                }
            }
            catch   { }

                {
                strW.WriteLine("Public Sub New(ByVal strForm As String, ByVal _bs As BindingSource, ByVal _dg As dgvEnter, _");

                strW.WriteLine("ByVal _ta As " + TheDataSet + "TableAdapters." + strTable + "TableAdapter, _");
                strW.WriteLine("ByVal _ds As DataSet, _");
                strW.WriteLine("ByVal _components As System.ComponentModel.Container, _");
                strW.WriteLine("ByVal _MainDefs As MainDefinitions, _");
                strW.WriteLine("ByVal blnRO As Boolean, _");
                strW.WriteLine("ByVal _Controls As Control.ControlCollection, ByVal _frmStandard As frmStandard, _");
                strW.WriteLine("ByVal blnFilters As Boolean)");
        
                string sSort1 = "";
                string sSort2 = "";

                //this will fail if a view
                try
                {
                Table aTable;
                aTable = (Table)theTables;
                string strPK = ut.strGetPK(aTable);
                string[] strPKs = strPK.Split(new Char[] { '.' });
                if (strPKs.Length > 0)
                    sSort1 = strPKs[0];
                if (strPKs.Length > 1)
                    sSort2 = strPKs[1];
                }
                catch { }
                                          
                strW.WriteLine("MyBase.New(strForm, \"" + strTable + "\", _bs, _dg, _ta, _ds, _MainDefs, blnRO, _");
                strW.WriteLine("\"" + sSort1 + "\",\"" + sSort2 + "\",_Controls, _frmStandard, blnFilters)"); 
            }

            strW.WriteLine("_ta.Connection.ConnectionString = GetConnectionString()");
            //the combo box objects.
            try
            {
                strLastReferencedTable = "";
                foreach (Column theColumn in theTables.Columns)
                {
                    //Replace underscores in field names as they generate errors in generated code.
                    strColName = theColumn.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                    string strReferencedTable = "";
                    string strRefTable = "";
                    if (blnView == false)
                    {
                        strRefTable = ut.strGetReferencedTable((Table)theTables, theColumn).Replace("-", "_");
                        strReferencedTable = strRefTable.Replace(" ", "_").Replace("%", "_");
                    }
                    if (strReferencedTable.Length != 0)
                    {
                        if (strLastReferencedTable != strReferencedTable)
                        {
                            strLastReferencedTable = strReferencedTable;
                            strW.WriteLine("Me.bs" + strReferencedTable + " = New System.Windows.Forms.BindingSource(_components)");
                        }
                    }
                }
            }
            catch { }   //if a view

            strW.WriteLine("End Sub");
            
            //Make the CreateColumns call.
            strW.WriteLine("Public Overrides Sub Createcolumns()");
            foreach (Column theColumns in theTables.Columns)
            {
                //Replace underscores in field names as they generate errors in generated code.
                strColName = theColumns.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                string strReferencedTable = "";
                if (blnView == false)
                    strReferencedTable = ut.strGetReferencedTable((Table)theTables, theColumns).Replace("-", "_").Replace(" ", "_").Replace("%", "_");

                if (strReferencedTable.Length != 0)
                    strW.WriteLine(strDG + strColName + " = New System.Windows.Forms.DataGridViewComboBoxColumn");
                else
                {
                    if (ut.blnIsBoolean(theColumns) == true)
                        strW.WriteLine(strDG + strColName + " = New System.Windows.Forms.DataGridViewCheckBoxColumn");
                    else
                        strW.WriteLine(strDG + strColName + " = New System.Windows.Forms.DataGridViewTextBoxColumn");
                }
            }
            strW.WriteLine("End Sub");

            //Make the AdjustColumns call.
            strW.WriteLine("Public Overrides Sub Adjustcolumns(ByVal blnAdjustWidth As Boolean)");
            string strLastColumn = "";

            strW.WriteLine(" MyBase.Adjustcolumns(blnAdjustWidth)");
            strW.WriteLine(" Try");

            //the combo box objects.
            try
            {
                strLastReferencedTable = "";
                foreach (Column theColumn in theTables.Columns)
                {
                    //Replace underscores in field names as they generate errors in generated code.
                    strColName = theColumn.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                    string strReferencedTable = "";
                    string strRefTable = "";
                    if (blnView == false)
                    {
                        strRefTable = ut.strGetReferencedTable((Table)theTables, theColumn).Replace("-", "_");
                        strReferencedTable = strRefTable.Replace(" ", "_").Replace("%", "_");
                    }
                    if (strReferencedTable.Length != 0)
                    {
                        if (strLastReferencedTable != strReferencedTable)
                        {
                            strLastReferencedTable = strReferencedTable;
                            strW.WriteLine(strReferencedTable + "TableAdapter = New " + TheDataSet + "TableAdapters." + strReferencedTable + "TableAdapter");

                            //20081126 Modify the connection string using a dgcolumns function.
                            strW.WriteLine(strReferencedTable + "TableAdapter.Connection.ConnectionString = GetConnectionString()");
                            strW.WriteLine("Me.bs" + strReferencedTable + ".DataMember = \"" + strRefTable + "\"");
                            strW.WriteLine("Me.bs" + strReferencedTable + ".DataSource = ds");
                        }
                    }
                }
            }
            catch { }   //if a view

            foreach (Column theColumn in theTables.Columns)
            {

                strColName = theColumn.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");

                //DECIDED NOT TO USE ExtendedProperty use separate lookup object instead.
                //if (blnExtendedProperty(c, "print", "No"))
                //    strPrint = "MainDefs.DONOTPRINT()";

                //Give fields which refer to a parent table a different colour.
                string strReferencedTable = "";
                if (blnView == false)
                    strReferencedTable = ut.strGetReferencedTable((Table)theTables, theColumn).Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                
                //The ReadOnly and visible must resp should be false if the field is an identity.
                string strROVisible = "MainDefs.blnGetRO(blnRO, \"" + strColName + "\"), ";
                if (theColumn.Identity == true)
                    strROVisible = "true, ";

                if (strReferencedTable.Length != 0)   //this fails for a view so only do for a table.
                    {
                        //The Combo box has a binding source to the parent table and needs to define
                        //the column(s) referred to.

                        string strReferencedColumn = ut.strGetPK(strGetTable(db, strReferencedTable)).Replace("-", "_").Replace(" ", "_").Replace("%", "_");    //db.Tables[strReferencedTable]);
                        //strReferencedColumn = c.Name; This is not sufficient. Goes wrong if referring field
                        //does not have same name as referenced field.

                        //Need a solution for this. 
                        //strReferencedColumn has the PKs. If strReferencedColumn is composite then use the c.Name
                        //otherwise use the PK. (This latter is needed because HWE may have 2 separate fields each as FK to other table.
                        //and in this case each field will have a different name.
                        if (strReferencedColumn.Contains(".") == true)
                            strReferencedColumn = theColumn.Name;
                       
                        strW.WriteLine("DefineComboBoxColumn(" + strDG + strColName +
                        ", " + ut.strGetFormat(theColumn, true) + ", True, \"" + theColumn.Name +
                        "\", \"\", FieldWidths." + ut.strGetWidth(theColumn) + ", blnRO, true, \"\", " +
                        " bs" + strReferencedTable + ", \"" + strReferencedColumn + "\" ,\"" + strReferencedColumn + "\", Color.Lavender)");
                        strW.WriteLine("If blnRO = True Then " + strDG + strColName + ".DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing");
                    }
                else

                    //c.Name instead of ColName to get the true SQL name including spaces.
                       strW.WriteLine("DefineColumn(" + strDG + strColName +
                            ", \"" + theColumn.Name +
                            "\", blnRO, ds." + strTable + "." + strColName + "Column.MaxLength)");
                    
                strLastColumn = strDG + strColName;
            }


            strW.WriteLine("PutColumnsInGrid()");
            strW.WriteLine("AdjustDataGridWidth(blnAdjustWidth)");
            strW.WriteLine("RefreshCombos()");
             strW.WriteLine("    Catch ex As Exception");
            strW.WriteLine("MsgBox(ex.Message)");
            strW.WriteLine("End Try");
            strW.WriteLine("End Sub");
            
            strW.WriteLine("Public Overrides Sub RefreshCombos()");
            //Fill the combobox dropdown tableadapters.
            //Count the number of combo boxes and store in property iComboCount.

            strW.WriteLine("MyBase.RefreshCombos()");
            strLastReferencedTable = "";
            int iComboCount = 0;
            foreach (Column theColumn in theTables.Columns)
            {
                
                //Replace underscores in field names as they generate errors in generated code.
                string strReferencedTable = "";
                if (blnView == false)
                    strReferencedTable = ut.strGetReferencedTable((Table)theTables, theColumn).Replace("-", "_").Replace(" ", "_").Replace("%", "_");

                if (strReferencedTable.Length != 0)
                {
                    if (strLastReferencedTable != strReferencedTable)
                    {
                        strLastReferencedTable = strReferencedTable;
                        iComboCount++;
                        strW.WriteLine("Me." + strReferencedTable + "TableAdapter.Fill(Me.ds." + strReferencedTable + ")");
                    }
                }
            }
            //20080909 This resets any field which was being edited and prevents modification.
            strW.WriteLine("dg.CancelEdit()");
            strW.WriteLine("iComboCount = " + iComboCount.ToString());
            strW.WriteLine("End sub");
            }
            return "";
        }

        /// <summary>
        /// Create Editing Region.
        /// </summary>
        /// 
        static string strCreateEditing(StreamWriter strW, Table t, string strTable, string strDG, string TheDataSet)
        {
        
        string strPK = ut.strGetPK(t);
        string [] strPKs = strPK.Split(new Char [] {'.'});
        strW.WriteLine("#Region \"Editing\"");

        strW.WriteLine("Public Overrides Sub dg_UserDeletingRow(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewRowCancelEventArgs)");
        strW.WriteLine("Try");

        strW.WriteLine("Dim tadap As " + TheDataSet + "TableAdapters." + strTable + "TableAdapter");
        strW.WriteLine("tadap = CType(ta, " + TheDataSet + "TableAdapters." + strTable + "TableAdapter)");
        strW.Write("tadap.Delete(");
        bool blnFirstOne = true;
        foreach (string s in strPKs)
        {
            if (blnFirstOne == false) strW.Write(",");
            blnFirstOne = false;
            strW.Write("e.Row.Cells(dg.Columns(\"" + s + "\").Index).Value.ToString()");
        }
            
        strW.WriteLine(")");
        strW.WriteLine("MyBase.dg_UserDeletingRow(sender, e)");
        strW.WriteLine("Catch ex As Exception");

        //RPB 20080926. Most common cause of failure is FK.
        strW.WriteLine("MsgBox(\"Delete failed. Most common cause is that record is in use in another table.\" + ex.message)");
        strW.WriteLine("e.Cancel = True");
        strW.WriteLine("End Try");
        strW.WriteLine("End Sub");
            
        //Add a function to set default values based on the default values in the table.
        strW.WriteLine("Private Sub dg_DefaultValuesNeeded(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewRowEventArgs) Handles dg.DefaultValuesNeeded");
        bool blnDefaultsNeeded=false;
        //if (t.Name.ToLower() == "basistekeningen")
            {
                foreach (Column theColumn in t.Columns)
                {
                    try  
                    {
                        if (theColumn.DefaultConstraint.Text.Length != 0) //can be null
                        {
                            if (blnDefaultsNeeded == false)
                            {
                                blnDefaultsNeeded = true;
                                strW.WriteLine("With e.Row");
                            }

                            //20120608 
                            if (theColumn.DataType.Name == DataType.Bit.Name)
                            {
                                //20170907 Change default value for Bits to true or false. 
                                string Constraint ="false";
                                if (theColumn.DefaultConstraint.Text.Contains("1"))
                                    Constraint = "true";

                                strW.WriteLine(".Cells(\"" + theColumn.Name + "\").Value =  " + Constraint);
                            }
                            else
                            {
                                string default_string = theColumn.DefaultConstraint.Text;
                                if (default_string.Contains("getdate")) default_string = "DateTime.Now()";
                                strW.WriteLine(".Cells(\"" + theColumn.Name + "\").Value =  " + default_string);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                if (blnDefaultsNeeded == true)
                {
                strW.WriteLine("End With");
                }
                strW.WriteLine("End Sub");
            }
            strW.WriteLine("#End Region");
            return "";
        }

        /// <summary>
        /// Add the filter code to the column definition file.
        /// </summary>
        static string strCreateFilterFile(StreamWriter strW, TableViewBase theTables, string strTable,
                        bool blnView, 
                        string strDG)
        {
            strW.WriteLine("#Region \"Filter\"");
            string strColName = "";
            string strSQLColName = "";

            strW.WriteLine("Public Overrides Sub CreateFilterBoxes(ByVal _Controls As Control.ControlCollection)");
            strW.WriteLine("MyBase.CreateFilterBoxes(_Controls)");

            foreach (Column theColumns in theTables.Columns)
            {
                strSQLColName = theColumns.Name.Replace("-", "_");
                strColName = theColumns.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                if (ut.blnIsBoolean(theColumns) == true)
                    strW.WriteLine("CreateACheckBox(cb" + strColName + "Find, \"" + strSQLColName + "\", AddressOf cbFind_CheckChanged, _Controls)");
                else
                    strW.WriteLine("CreateAFilterBox(tb" + strColName + "Find, \"" + strSQLColName + "\", AddressOf tbFind_TextChanged, _Controls)");
            }
            strW.WriteLine("End Sub");

            foreach (Column theColumn in theTables.Columns)
            {
                strColName = theColumn.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                if (ut.blnIsBoolean(theColumn) == true)
                    strW.WriteLine("Friend WithEvents cb" + strColName + "Find As System.Windows.Forms.CheckBox");
                else
                    strW.WriteLine("Friend WithEvents tb" + strColName + "Find As System.Windows.Forms.TextBox");
            }
            strW.WriteLine("Private Sub cbFind_CheckChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)");
            strW.WriteLine("MakeFilter(False)");
            strW.WriteLine("End Sub");
            strW.WriteLine("Private Sub tbFind_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)");
            strW.WriteLine(" MakeFilter(False)");
            strW.WriteLine("End Sub");
            strW.WriteLine("#End Region");

            //add the disposable code if there are comboboxes.
            if (theTables.Columns.Count != 0 && blnView == false)
            {
                strW.WriteLine("");
                strW.WriteLine("#Region \"IDisposable Support\"");
                strW.WriteLine("Private disposedValue As Boolean ' To detect redundant calls");

                strW.WriteLine("' IDisposable");
                strW.WriteLine("Protected Overridable Sub Dispose(disposing As Boolean)");

                strW.WriteLine("If Not Me.disposedValue Then");
                strW.WriteLine(" If disposing Then");
                strW.WriteLine("  If Not bsb_cassette_type Is Nothing Then");
                strW.WriteLine("   bsb_cassette_type.Dispose()");
                strW.WriteLine("  End If");
                strW.WriteLine(" End If");
                strW.WriteLine("End If");
                strW.WriteLine("Me.disposedValue = True");
                strW.WriteLine("End Sub");

                strW.WriteLine(" ' This code added by Visual Basic to correctly implement the disposable pattern.");
                strW.WriteLine("Public Sub Dispose() Implements IDisposable.Dispose");
                strW.WriteLine("    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.");
                strW.WriteLine("Dispose(True)");
                strW.WriteLine("GC.SuppressFinalize(Me)");
                strW.WriteLine("End Sub");
                strW.WriteLine("#End Region");
            }
            strW.WriteLine("End Class");
            return "";
        }


        /// <summary>
        /// Create a Form File based on a Table with FKs.
        /// </summary>
        static string strCreateFormFile(Database db, StreamWriter strW, Table t, string strTable, string strDG, string strActualTableName, string strTheDataSet)
        {
            //Write the header and New sub.
            strCreateFormFileHeader_version2(t, strW, strTable, strDG, strActualTableName, strTheDataSet);
            strCreateFormFileTail(t, strW, strTable, strDG);
            return "";
        }

            /// <summary>
            /// Write the header and New sub.
            /// </summary>
        static string strCreateFormFileHeader_version2(TableViewBase t, StreamWriter strW, string strTable, string strDG, string strActualTableName, string TheDataSet)
            {
                strW.WriteLine("'------------------------------------------------");
                strW.WriteLine("'Name: Module gen_Form_" + TheDataSet + "_" + strTable + ".vb.");
                strW.WriteLine("'Function: ");
                strW.WriteLine("'Copyright Robin Baines 2010. All rights reserved.");
                strW.WriteLine("'Created " + System.DateTime.Today + ".");
                strW.WriteLine("'Notes: ");
                strW.WriteLine("'Modifications:");
                strW.WriteLine("'------------------------------------------------");
                strW.WriteLine("Imports Utilities");
                strW.WriteLine("Imports System.Windows.Forms");
                strW.WriteLine("Imports System.Drawing");

                strW.WriteLine("Public Class form_" + strTable);
                strW.WriteLine("Inherits Genericform");
                strW.WriteLine("Friend WithEvents " + TheDataSet + " As "  + TheDataSet);
                strW.WriteLine("Protected WithEvents taParent As " + TheDataSet + "TableAdapters." + strTable + "TableAdapter");
                strW.WriteLine("Protected Overrides Sub Dispose(ByVal disposing As Boolean)");
                strW.WriteLine("If disposing AndAlso components IsNot Nothing Then");
                strW.WriteLine("components.Dispose()");
                strW.WriteLine("End If");
                strW.WriteLine("MyBase.Dispose(disposing)");
                strW.WriteLine("End Sub");
                strW.WriteLine("Private components As System.ComponentModel.IContainer");
                strW.WriteLine("Private Sub InitializeComponent()");
                strW.WriteLine("Me." + TheDataSet + " = New " + TheDataSet);
                strW.WriteLine("components = New System.ComponentModel.Container");
                strW.WriteLine("Me.taParent = New " + TheDataSet + "TableAdapters." + strTable + "TableAdapter");
                strW.WriteLine("Me.bsParent.DataMember = \"" + strActualTableName + "\"");
                strW.WriteLine("Me.bsParent.DataSource = Me." + TheDataSet);
                strW.WriteLine("End Sub");

                strW.WriteLine("#Region \"New\"");
                strW.WriteLine("'This for the designer?!");
                strW.WriteLine("Public Sub New()");
                strW.WriteLine("MyBase.New()");
                strW.WriteLine("InitializeComponent()");
                strW.WriteLine("End Sub");


            //20130123 Added strForm parameter to the New generic form call.
                strW.WriteLine("Public Sub New(ByVal tsb As ToolStripItem _");
                strW.WriteLine(", ByVal strForm As String, ByVal strParentName As String, ByVal _MainDefs As MainDefinitions, _");
                strW.WriteLine("Fields as Dictionary(Of String, String), _blnFullSize as Boolean, _bRO as boolean, blnFilter as boolean)");
                strW.WriteLine("MyBase.New(tsb, strForm, strParentName, _MainDefs, Fields, _blnFullSize, _bRO)");
                strW.WriteLine("InitializeComponent()");
                if (blnIncludeDataSetInClassName == true) strW.WriteLine("vParent = New " + TheDataSet + "_" + strTable + "(strForm, Me.bsParent, Me.dgParent, _");
                else strW.WriteLine("vParent = New " + strTable + "(strForm, Me.bsParent, Me.dgParent, _");

                strW.WriteLine("taParent, _");
                strW.WriteLine("Me." + TheDataSet+ ", _");
                strW.WriteLine("Me._components, _");
                strW.WriteLine("_MainDefs, blnRO, Me.Controls, Me, blnFilter)");

                //20151111Added these as they are no longer needed.
                strW.WriteLine("Me.SwitchOffPrint()");
                strW.WriteLine("Me.SwitchOffUpdate()");
                //20120531 No longer needed when blnFilter is in New. strW.WriteLine("vParent.CreateFilterBoxes(Me.Controls)");
                strW.WriteLine("End Sub");
                strW.WriteLine("#End Region");

                strW.WriteLine("#Region \"Load\"");
                strW.WriteLine("Protected Overrides Sub FillTableAdapter()");

                strW.WriteLine("vParent.RefreshCombos()");
                strW.WriteLine("vParent.StoreRowIndexWithFocus()");
                strW.WriteLine("Me.taParent.Fill(Me." + TheDataSet+ "." + strTable + ")");
                strW.WriteLine("vParent.ResetFocusRow()");

                strW.WriteLine("'Set the filter if necessary.");
                strW.WriteLine("vParent.ColumnDoubleClick(FilterFields)");
                strW.WriteLine("End Sub");

                strW.WriteLine("Protected Overrides Sub RefreshCombos()");
                strW.WriteLine("vParent.RefreshCombos()");
                strW.WriteLine("End Sub");
                strW.WriteLine("#End Region");
                return "";
            }

            /// <summary>
            /// Create form tail. One version for a table uses the PK(s) to construct the 
            /// delete columns.
            /// </summary>
            static string strCreateFormFileTail(Table t, StreamWriter strW, string strTable, string strDG)
            {
            
            strW.WriteLine("End Class");
            return "";
            }

           static string strCreateFormFileTail(StreamWriter strW, string strTable, string strDG)
            {
            strW.WriteLine("End Class");
            return "";
        }

        /// <summary>
        /// Create a form file for a view; meaning no FK stuff.
        /// </summary>
           static string strCreateFormFile(TableViewBase t, StreamWriter strW, string strTable, string strDG, string strActualTableName, string strDataSet)
        {
            strCreateFormFileHeader_version2(t, strW, strTable, strDG, strActualTableName, strDataSet);
            strCreateFormFileTail(strW, strTable, strDG);
            return "";
        }


        static private string ConnectedDataSets(XsdFiles xsdFiles, string strTable)
        {
            foreach (XsdFile xsdF in xsdFiles.xsdFiles)
            {
                if (xsdF.findtable(strTable)) 
                    return xsdF.GetDataSetName();
            }
            return "";
        }

        static private void CreateTableFiles(Database db, Table t, string strOutputDirectory, string strDataSet)
        {
            string strTableName = t.Name.Replace(" ", "_");
            StreamWriter strW;
            strW = new StreamWriter(strOutputDirectory + "gen_combo" + strDataSet + "_" + strTableName + ".vb");
            strCreateTableFile(db, strW, t, false, strTableName, "dg", strDataSet);
            strCreateEditing(strW, t, strTableName, "dg", strDataSet);
            strCreateFilterFile(strW, t, strTableName, false, "dg");
            strW.Close();
            strW = null;

            strW = new StreamWriter(strOutputDirectory + "gen_" + strDataSet + "_" + strTableName + ".vb");
            strCreateTableFile(db, strW, t, true, strTableName, "dg", strDataSet);
            strCreateEditing(strW, t, strTableName, "dg", strDataSet);
            strCreateFilterFile(strW, t, strTableName, true, "dg");
            strW.Close();
            strW = null;


            strW = new StreamWriter(strOutputDirectory + "gen_Form_" + strDataSet + "_" + strTableName + ".vb");
            strCreateFormFile(db, strW, t, strTableName, "dg", t.Name, strDataSet);
            strW.Close();
            strW = null;

        }

        static private void CreateViewFiles(Database db, View t, string strOutputDirectory, string strDataSet)
        {
            string strTableName = t.Name.Replace(" ", "_");
            StreamWriter strW;
            strW = new StreamWriter(strOutputDirectory + "gen_" + strDataSet + "_" + strTableName + ".vb");
            strCreateTableFile(db, strW, t, true, strTableName, "dg", strDataSet);
            strCreateFilterFile(strW, t, strTableName, true, "dg");
            strW.Close();
            strW = null;

            strW = new StreamWriter(strOutputDirectory + "gen_Form_" + strDataSet + "_" + strTableName + ".vb");
            strCreateFormFile(t, strW, strTableName, "dg", t.Name, strDataSet);
            strW.Close();
            strW = null;
        }

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        /// 
        static bool blnIncludeDataSetInClassName = true;
        static void Main(string[] args)
        {
            blnIncludeDataSetInClassName = true;

            Server server;  
            string strApplicationPath="";
            string strOutputDirectory;
            string strServer = "";
                if (args == null || args.Length < 3)
                {
                    Console.WriteLine("GenUI  server database applicationpath OutputDirectory");
                    return;
                }
                else
                {
                    strServer = args[0];
                    strDatabase = args[1];
                    strApplicationPath = args[2];
                    strOutputDirectory = args[3];
                }
            //}

            //Make a list of all the xsd files with a list of the tables and views in each one.
            //These are used to identify which tables and views in the database should generate code.
            XsdFiles xsdFiles = new XsdFiles(strApplicationPath);


            server = new Server(strServer);
            DatabaseCollection dbs = server.Databases;
            Database db = dbs[strDatabase];   
            //Dictionary<string, string> CreateAs = new Dictionary<string, string>();

                StreamWriter strW;
                Console.WriteLine(db.Name);

            //Iterate through the database tables.
                    foreach (Table t in db.Tables)
                    {
                        try
                        {
                            {
                              
                                {
                                    //only check if file is needed if the strApplicationDirectory is defined.
                                    if (strApplicationPath.Length > 0)
                                    {
                                        foreach (XsdFile xsdF in xsdFiles.xsdFiles)
                                        {
                                            if (xsdF.findtable(t.Name))CreateTableFiles(db, t, strOutputDirectory, xsdF.GetDataSetName());
                                        }

                                    }
                                    else CreateTableFiles(db, t, strOutputDirectory, "TheDataSet");
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                    strW = new StreamWriter(strOutputDirectory + "gen_OpenForm.vb");
                    lookups.strCreateOpenForm(db, strW);
                    strW.Close();
                    strW = null;
                

                //Iterate through the database views.
                    foreach (View t in db.Views)
                        if (t.IsSystemObject == false)
                        {
                            try
                            {
                                //following throws if table is not in the CreateAs array.
                                if (strApplicationPath.Length > 0)
                                {
                                    foreach (XsdFile xsdF in xsdFiles.xsdFiles)
                                    {
                                        if (xsdF.findtable(t.Name))
                                        {
                                            CreateViewFiles(db, t, strOutputDirectory, xsdF.GetDataSetName());
                                        }
                                    }
                                }
                                else 
                                {
                                    CreateViewFiles(db, t, strOutputDirectory, "TheDataSet");
                                }
                            }

                            catch
                            {
                            }
                        }



                ////Create list of all column names.
                ////Renamed to aaa... to prevent copying over project versions.
                //strW = new StreamWriter("..\\..\\..\\TestUI\\GenUI\\Agen_GetColumnHeader.vb");
                //lookups.strCreateListOfColumnsFile(db, strW);
                //strW.Close();
                //strW = null;

                ////Create list of tables for table display name look up
                //strW = new StreamWriter("..\\..\\..\\TestUI\\GenUI\\Agen_GetTableTexts.vb");
                //lookups.strCreateListOfTablesFile(db, strW);
                //strW.Close();
                //strW = null;

                ////Create list of table and columns to adjust the print string.
                //strW = new StreamWriter("..\\..\\..\\TestUI\\GenUI\\Agen_GetPrintDisplayVisibility.vb");
                //lookups.strCreateListOfPrintStringsFile(db, strW);
                //strW.Close();
                //strW = null;

                ////Create list of table and columns to adjust the print string.
                //strW = new StreamWriter("..\\..\\..\\TestUI\\GenUI\\Agen_GetDisplayMembers.vb");
                //lookups.strCreateListOfDisplayMembers(db, strW);
                //strW.Close();
                //strW = null;


            server.ConnectionContext.Disconnect();
            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }
    }
}
