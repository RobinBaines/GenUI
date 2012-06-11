////////////////////////////////////////////////////////////////////
//Copyright @2008 Robin Baines
//June - Oct 2008
//GenUI.
//Generate user interface in basic from database.
//20091130 Added MaxLength
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
        static String strApplication;
        static dbUtilities ut = new dbUtilities();
        static Lookups lookups = new Lookups();


        static Table strGetTable(Database db, string strTableName)
        {
            foreach (Table t in db.Tables)
            {
                if (t.Name == strTableName)
                    return t;
            }
            return null;
        }
        /// <summary>
        /// Create a file to define the columns to be dispayed in a datagrid.
        /// </summary>
        static string strCreateTableFile(Database db, StreamWriter strW, TableViewBase t, bool blnView, string strTable, string strDG, string TheDataSet)
        {
            //if (strTable == "b_machine_medication_list")
            {

            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("'Name: Module " + strTable + ".vb.");
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

            strW.WriteLine("Public Class " + strTable);
            strW.WriteLine("Inherits dgColumns");
            Console.WriteLine(" Table: " + strTable);
            //Console.WriteLine("  Columns: ");
            string strLastReferencedTable = "";
            string strColName = "";
            try
            {
            //Declare the DataGrid Columns.
            foreach (Column c in t.Columns)
            {

                strColName = c.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                if (blnView == false && ut.strGetReferencedTable((Table)t, c).Length != 0)
                    strW.WriteLine("Friend WithEvents " + strDG + strColName + " As System.Windows.Forms.DataGridViewComboBoxColumn");
                else
                {
                    if (ut.blnIsBoolean(c) == true)
                        strW.WriteLine("Friend WithEvents " + strDG + strColName + " As System.Windows.Forms.DataGridViewCheckBoxColumn");
                    else
                        strW.WriteLine("Friend WithEvents " + strDG + strColName + " As System.Windows.Forms.DataGridViewTextBoxColumn");
                }
            }
                       
            //Create binding sources to combobox dropdown tables.
            foreach (Column c in t.Columns)
                {
                    //Replace underscores in field names as they generate errors in generated code.
                    strColName = c.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                    string strReferencedTable = "";
                    if (blnView == false) 
                        strReferencedTable = ut.strGetReferencedTable((Table)t, c).Replace("-", "_").Replace(" ", "_").Replace("%", "_");

                    if (strReferencedTable.Length != 0)
                    {
                        if (strLastReferencedTable != strReferencedTable)
                        {
                            strLastReferencedTable = strReferencedTable;
                            strW.WriteLine("Public bs" + strReferencedTable + " As BindingSource");
                            //strW.WriteLine("Friend WithEvents " + strReferencedTable + "TableAdapter As " +
                            //    strDatabase + "." + strDatabase + "DataSetTableAdapters." + strReferencedTable + "TableAdapter");
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
                
                //if (iVersion == 2)
                //{
                //    strW.WriteLine("ByVal _Controls As Control.ControlCollection, ByVal _frmStandard As frmStandard)");
                //}
                //else
                {
                    strW.WriteLine("ByVal _Controls As Control.ControlCollection, ByVal _frmStandard As frmStandard, _");
                    strW.WriteLine("ByVal blnFilters As Boolean)");
                }
                string sSort1 = "";
                string sSort2 = "";

                //this will fail if a view
                try
                {
                Table aTable;
                aTable = (Table)t;
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

            //20081126 Modify the connection string using a dgcolumns function.
            strW.WriteLine("_ta.Connection.ConnectionString = GetConnectionString()");
            //Create the combobox dropdown tableadapters.
            try
            {
                strLastReferencedTable = "";
                foreach (Column c in t.Columns)
                {
                    //Replace underscores in field names as they generate errors in generated code.
                    strColName = c.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                    string strReferencedTable = "";
                    if (blnView == false)
                        strReferencedTable = ut.strGetReferencedTable((Table)t, c).Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                    if (strReferencedTable.Length != 0)
                    {
                        if (strLastReferencedTable != strReferencedTable)
                        {
                            strLastReferencedTable = strReferencedTable;
                            strW.WriteLine("Me.bs" + strReferencedTable + " = New System.Windows.Forms.BindingSource(_components)");
                            strW.WriteLine(strReferencedTable + "TableAdapter = New " + TheDataSet + "TableAdapters." + strReferencedTable + "TableAdapter");

                            //20081126 Modify the connection string using a dgcolumns function.
                            strW.WriteLine(strReferencedTable + "TableAdapter.Connection.ConnectionString = GetConnectionString()");
                            strW.WriteLine("Me.bs" + strReferencedTable + ".DataMember = \"" + strReferencedTable + "\"");
                            strW.WriteLine("Me.bs" + strReferencedTable + ".DataSource = ds");
                        }
                    }
                }
            }
            catch { }   //if a view
            strW.WriteLine("End Sub");
            
            //Make the CreateColumns call.
            strW.WriteLine("Public Overrides Sub Createcolumns()");
            foreach (Column c in t.Columns)
            {
                //Replace underscores in field names as they generate errors in generated code.
                strColName = c.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                string strReferencedTable = "";
                if (blnView == false)
                    strReferencedTable = ut.strGetReferencedTable((Table)t, c).Replace("-", "_").Replace(" ", "_").Replace("%", "_");

                if (strReferencedTable.Length != 0)
                    strW.WriteLine(strDG + strColName + " = New System.Windows.Forms.DataGridViewComboBoxColumn");
                else
                {
                    if (ut.blnIsBoolean(c) == true)
                        strW.WriteLine(strDG + strColName + " = New System.Windows.Forms.DataGridViewCheckBoxColumn");
                    else
                        strW.WriteLine(strDG + strColName + " = New System.Windows.Forms.DataGridViewTextBoxColumn");
                }
            }

            //Decide which columns to include in datagrid.
            //20090113 Replaced by PutColumnsInGrid()
            //strW.WriteLine("dg.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() { _");
            //bool blnFirstTime = true;
            //foreach (Column c in t.Columns)
            //{
            //    strColName = c.Name.Replace("-", "_");
            //    string strV = "";
            //    if (blnFirstTime == true) blnFirstTime = false;
            //    else strV = ", ";
            //    strW.WriteLine(strV + strDG + strColName + " _ ");
            //}
            //strW.WriteLine("})");
            strW.WriteLine("End Sub");

            //Make the AdjustColumns call.
            strW.WriteLine("Public Overrides Sub Adjustcolumns(ByVal blnAdjustWidth As Boolean)");
            string strLastColumn = "";

            foreach (Column c in t.Columns)
            {

                strColName = c.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");

                //string strPrint = "MainDefs.strGetPrintString(\"" + t.Name + "\", \"" + c.Name + "\"), ";
                //string strVisible = "MainDefs.blnGetVisibility(\"" + t.Name + "\", \"" + c.Name + "\"), ";
                //DECIDED NOT TO USE ExtendedProperty use separate lookup object instead.
                //if (blnExtendedProperty(c, "print", "No"))
                //    strPrint = "MainDefs.DONOTPRINT()";

                //Give fields which refer to a parent table a different colour.
                string strReferencedTable = "";
                if (blnView == false)
                    strReferencedTable = ut.strGetReferencedTable((Table)t, c).Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                
                //The ReadOnly and visible must resp should be false if the field is an identity.
                string strROVisible = "MainDefs.blnGetRO(blnRO, \"" + strColName + "\"), ";
                if (c.Identity == true)
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
                            strReferencedColumn = c.Name;
                       
                        strW.WriteLine("DefineComboBoxColumn(" + strDG + strColName +
                        ", " + ut.strGetFormat(c, true) + ", True, \"" + c.Name +
                        "\", \"\", FieldWidths." + ut.strGetWidth(c) + ", blnRO, true, \"\", " +
                        " bs" + strReferencedTable + ", \"" + strReferencedColumn + "\" ,\"" + strReferencedColumn + "\", Color.Lavender)");
                        strW.WriteLine("If blnRO = True Then " + strDG + strColName + ".DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing");
                    }
                else
                    //c.Name instead of ColName to get the true SQL name including spaces.
                       strW.WriteLine("DefineColumn(" + strDG + strColName +
                            ", \"" + c.Name +
                            "\", blnRO, ds." + strTable + "." + strColName + "Column.MaxLength)");
                    
                strLastColumn = strDG + strColName;
            }

            //20090113 Added this so that sequence is read from m_columns
            strW.WriteLine("PutColumnsInGrid()");
            //20090327 Changed call to AdjustDataGridWidth
            strW.WriteLine("AdjustDataGridWidth(blnAdjustWidth)");
            //strW.WriteLine("If blnAdjustWidth = True Then");
            //strW.WriteLine("    AdjustDataGridWidth(true)");
            //strW.WriteLine("End If");
            strW.WriteLine("RefreshCombos()");
            strW.WriteLine("End Sub");
            //strW.WriteLine(strLastColumn + ".AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill");

            //20081009
            strW.WriteLine("Public Overrides Sub RefreshCombos()");
            //Fill the combobox dropdown tableadapters.
            //Count the number of combo boxes and store in property iComboCount.

            //20081207 Call mybase. sub
            strW.WriteLine("MyBase.RefreshCombos()");
            strLastReferencedTable = "";
            int iComboCount = 0;
            foreach (Column c in t.Columns)
            {
                
                //Replace underscores in field names as they generate errors in generated code.
                string strReferencedTable = "";
                if (blnView == false)
                    strReferencedTable = ut.strGetReferencedTable((Table)t, c).Replace("-", "_").Replace(" ", "_").Replace("%", "_");

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

            //RPB 20081021 Removed Handles dg.UserDeletingRow because this is present in the overridable sub in dgColumns.
//strW.WriteLine("Public Overrides Sub dg_UserDeletingRow(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewRowCancelEventArgs) Handles dg.UserDeletingRow");
        strW.WriteLine("Public Overrides Sub dg_UserDeletingRow(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewRowCancelEventArgs)");
        strW.WriteLine("Try");

        //20090129 RPB Changed strDatabase + DataSet to TheDataSet.
        strW.WriteLine("Dim tadap As " + TheDataSet + "TableAdapters." + strTable + "TableAdapter");
        //strW.WriteLine("Dim tadap As " + strDatabase + "DataSetTableAdapters." + t.Name + "TableAdapter");
        strW.WriteLine("tadap = CType(ta, " + TheDataSet + "TableAdapters." + strTable + "TableAdapter)");
        //strW.WriteLine("tadap = CType(ta, " + strDatabase + "DataSetTableAdapters." + t.Name + "TableAdapter)");
        strW.Write("tadap.Delete(");
        bool blnFirstOne = true;
        foreach (string s in strPKs)
        {
            if (blnFirstOne == false) strW.Write(",");
            blnFirstOne = false;
            //20090907 Use e instead of direct reference. Then multi select delete will work.
            strW.Write("e.Row.Cells(dg.Columns(\"" + s + "\").Index).Value.ToString()");
            //strW.Write("dg.CurrentRow.Cells(dg.Columns(\"" + s + "\").Index).Value.ToString()");
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
                foreach (Column c in t.Columns)
                {
                    try  
                    {
                        if (c.DefaultConstraint.Text.Length != 0) //can be null
                        {
                            if (blnDefaultsNeeded == false)
                            {
                                blnDefaultsNeeded = true;
                                strW.WriteLine("With e.Row");
                            }

                            //20120608 
                            if (c.DataType.Name == DataType.Bit.Name)
                            {
                                strW.WriteLine(".Cells(\"" + c.Name + "\").Value =  " + c.DefaultConstraint.Text );
                            }
                            else
                            {
                                strW.WriteLine(".Cells(\"" + c.Name + "\").Value =  " + c.DefaultConstraint.Text );
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
        static string strCreateFilterFile(StreamWriter strW, TableViewBase t, string strTable, string strDG)
        {
            //strW.WriteLine("'------------------------------------------------");
            //strW.WriteLine("'Name: Module genfilter_" + strTable + ".vb.");
            //strW.WriteLine("'Function: ");
            //strW.WriteLine("'Copyright Robin Baines 2008. All rights reserved.");
            //strW.WriteLine("'Created " + System.DateTime.Today + ".");
            //strW.WriteLine("'Notes: ");
            //strW.WriteLine("'Modifications:");
            //strW.WriteLine("'------------------------------------------------");
            //strW.WriteLine("Imports Utilities");
            //strW.WriteLine("Public Class filter_" + strTable);
            strW.WriteLine("#Region \"Filter\"");
            //Console.WriteLine(" Table: " + strTable);
            string strColName = "";

            strW.WriteLine("Public Overrides Sub CreateFilterBoxes(ByVal _Controls As Control.ControlCollection)");

            //20100304 Added this to add possibilities.
            strW.WriteLine("MyBase.CreateFilterBoxes(_Controls)");

            foreach (Column c in t.Columns)
            {
                strColName = c.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                if (ut.blnIsBoolean(c) == true)
                    strW.WriteLine("CreateACheckBox(cb" + strColName + "Find, \"" + strColName + "\", AddressOf cbFind_CheckChanged, _Controls)");
                else
                    strW.WriteLine("CreateAFilterBox(tb" + strColName + "Find, \"" + strColName + "\", AddressOf tbFind_TextChanged, _Controls)");
            }
            strW.WriteLine("End Sub");

            foreach (Column c in t.Columns)
            {
                strColName = c.Name.Replace("-", "_").Replace(" ", "_").Replace("%", "_");
                if (ut.blnIsBoolean(c) == true)
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
            //make the event handlers
            //foreach (Column c in t.Columns)
            //{
            //    //Replace underscores in field names as they generate errors in generated code.
            //    strColName = c.Name.Replace("-", "_");
            //    if (ut.blnIsBoolean(c) == true)
            //        strW.WriteLine("Private Sub cb" + strColName + "Find_CheckChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFind.CheckedChanged");
            //    else
            //        strW.WriteLine("Private Sub tb" + strColName + "Find_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbFind.TextChanged");

            //    strW.WriteLine("MakeFilter(False)");
            //    //strW.WriteLine("ut.MakeFilter(bs, FindTbs)");
            //    strW.WriteLine("End Sub");
            //}

            strW.WriteLine("#End Region");
            strW.WriteLine("End Class");
            return "";
        }


        
        /// 
        /// <summary>
        /// Configure a ToolStripMenuItem.
        /// </summary>
        static string strFC(Database db, StreamWriter strW, int iD, string strReferencedTable, string strTable, string strFKname)
        {
            //The name of the table which has the FK constraint.
            //The name of the FK.
            Table t;
            t = db.Tables[strTable];
            string strPK = ut.strGetPK(db.Tables[strReferencedTable]);
            {
                int i = 0;
                int c = 0;
                String strT;
                //t.ForeignKeys represents a collection of ForeignKey objects. 
                //Each ForeignKey object represents a foreign key defined on the table. 
                
                while (i < t.ForeignKeys.Count)
                {
                    //Second check is not necessary because the FKName is unique.
                    if (t.ForeignKeys[i].Name == strFKname && t.ForeignKeys[i].ReferencedTable.ToString() == strReferencedTable)
                    {
                        strW.WriteLine("Me.ToolStripMenuItem" + iD.ToString() + "  = New System.Windows.Forms.ToolStripMenuItem");
                        strW.WriteLine("Me.ToolStripMenuItem" + iD.ToString() + ".Size = New System.Drawing.Size(152, 22)");
                        strW.WriteLine("Me.ToolStripMenuItem" + iD.ToString() + ".Text = MainDefs.strGetTableText(\"" + strTable + "\")");
                        strW.WriteLine("Me.ToolStripMenuItem" + iD.ToString() + ".Tag = \"" + strTable + "." + strPK + "\"");
                        strW.WriteLine("Me.ToolStripMenuItem" + iD.ToString() + ".ShortcutKeys = Keys.F" + (iD + 1).ToString());
                        strW.WriteLine("Me.ToolStripMenuItem" + iD.ToString() + ".ShowShortcutKeys = True");

                        //Call the Toolstrip the target table name with the PK(s) by putting in Name.
                        strT = strTable;
                        while (c < t.ForeignKeys[i].Columns.Count)
                        {
                            if (strT.Length != 0) strT = strT + ".";
                            strT = strT + ut.CleanUp(t.ForeignKeys[i].Columns[c].ToString());
                            c++;
                        }
                        strW.WriteLine("Me.ToolStripMenuItem" + iD.ToString() + ".Name = \"" + strT + "\"");
                        strW.WriteLine("Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItem" + iD.ToString() + "})");
                    }
                    i++;
                }
            }
            return "";
        }

        /// <summary>
        /// Create a Form File based on a Table with FKs.
        /// </summary>
        static string strCreateFormFile(Database db, StreamWriter strW, Table t, string strTable, string strDG, string strActualTableName, string strTheDataSet)
        {
            //Write the header and New sub.
           // if (iVersion >= 2) 
                strCreateFormFileHeader_version2(t, strW, strTable, strDG, strActualTableName, strTheDataSet);
           
            
            //else
            //{
            //    strCreateFormFileHeader(t, strW, strTable, strDG, strActualTableName);

            //    //Build the ToolStripMenu to reference child tables.
            //    strW.WriteLine("#Region \"ToolStrip\"");
            //    strW.WriteLine("'Add a ToolStripMenu Item for every table.field for which this table is used as foreign key.");
            //    System.Data.DataTable FK;
            //    try
            //    {
            //        //Enumerates a list of foreign keys defined on the table
            //        FK = t.EnumForeignKeys();
            //        int i = 0;
            //        System.Data.DataRow row;
            //        while (i < FK.Rows.Count)
            //        {
            //            row = FK.Rows[i];
            //            //Define the ToolStripMenuItem
            //            strW.WriteLine("Friend WithEvents ToolStripMenuItem" + i.ToString() + " As System.Windows.Forms.ToolStripMenuItem");

            //            //Create the Click event handler.
            //            strW.WriteLine("Private Sub ToolStripMenuItem" + i.ToString() + "_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem" + i.ToString() + ".Click");
            //            strW.WriteLine("OpenFilteredForm(sender)");
            //            strW.WriteLine("End Sub");
            //            i = i + 1;
            //        }

            //        //Configure the ContextStrip adding entries ofr each table which references this parent table.
            //        strW.WriteLine("Protected Overrides Sub ConfigureContextStrip()");
            //        strW.WriteLine("'the contextstrip1 is bound to dgParent and is shown automatically on right mouse click.");
            //        FK = t.EnumForeignKeys();
            //        i = 0;
            //        while (i < FK.Rows.Count)
            //        {
            //            row = FK.Rows[i];

            //            //[1] is the name of the child table which references this parent table.
            //            //[2] is the name of the FK.
            //            strFC(db, strW, i, strTable, row[1].ToString(), row[2].ToString());
            //            i = i + 1;
            //        }
            //        strW.WriteLine("End Sub");
            //        strW.WriteLine("#End Region");
            //    }

            //    catch (Exception e)
            //    {
            //        Console.WriteLine("{0} Exception caught.", e);
            //    }
            //}
            strCreateFormFileTail(t, strW, strTable, strDG);
                return "";
        }

        /// <summary>
        /// Write the header and New sub.
        /// //20120608 strTable is with spaces replaced by _ while strActualTableName is with spaces if the database table actually has them in the name.
        /// </summary>
        static string strCreateFormFileHeader(TableViewBase t, StreamWriter strW, string strTable, string strDG, string strActualTableName)
        {
            strW.WriteLine("'------------------------------------------------");
            strW.WriteLine("'Name: Module genForm_" + strTable + ".vb.");
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

            strW.WriteLine("Public Class form_" + strTable);
            //20090407 Changed to ProjectGenericform
            strW.WriteLine("Inherits ProjectGenericform");
            

            strW.WriteLine("#Region \"New\"");
            //Console.WriteLine(" Table: " + strTable);
            //20100218 RPB removed dsFormLevels, 
            //strW.WriteLine("Public Sub New(ByVal tsb As ToolStripButton, ByVal dsFormLevels As DataSet _");
             strW.WriteLine("Public Sub New(ByVal tsb As ToolStripButton _");
            strW.WriteLine(", ByVal strSecurityName As String, ByVal _MainDefs As MainDefinitions, _");
            strW.WriteLine("Fields as Dictionary(Of String, String), _blnFullSize as Boolean, _bRO as boolean)");

            //20100218 RPB removed dsFormLevels, 
            //strW.WriteLine("MyBase.New(tsb, dsFormLevels, strSecurityName, _MainDefs, Fields, _blnFullSize, _bRO)");
            strW.WriteLine("MyBase.New(tsb, strSecurityName, _MainDefs, Fields, _blnFullSize, _bRO)");

            //20090129 RPB Changed strDatabase + DataSet to TheDataSet.
            strW.WriteLine("CType(Me.TheDataSet, System.ComponentModel.ISupportInitialize).BeginInit()");
            //strW.WriteLine("CType(Me." + strDatabase + "DataSet, System.ComponentModel.ISupportInitialize).BeginInit()");
            strW.WriteLine("Me.taParent = New TheDataSetTableAdapters." + strTable + "TableAdapter");
            //strW.WriteLine("Me.taParent = New " + strDatabase + "DataSetTableAdapters." + strTable + "TableAdapter");
            strW.WriteLine("Me.bsParent.DataMember = \"" + strActualTableName + "\"");
            strW.WriteLine("Me.bsParent.DataSource = Me.TheDataSet");
            
            //Last parameter activates the filtering.
            //20081229 added strSecurityName for the frmColumn functionality.

            //20090117 The 1st parameter is the form name used in the Columns table so make this equal 
            //to the name displayed on the form header using the lookup table.
            strW.WriteLine("vParent = New " + strTable + "(MainDefs.strGetTableText(strSecurityName), Me.bsParent, Me.dgParent, _");
            //strW.WriteLine("vParent = New " + strTable + "(strSecurityName, Me.bsParent, Me.dgParent, _");
            
            strW.WriteLine("taParent, _");

            //20090129 RPB Changed strDatabase + DataSet to TheDataSet.
            strW.WriteLine("Me.TheDataSet, _");
            //strW.WriteLine("Me." + strDatabase + "DataSet, _");

            strW.WriteLine("Me._components, _");
            strW.WriteLine("_MainDefs, blnRO, True)");
            strW.WriteLine("vParent.CreateFilterBoxes(Me.Controls)");

            //20090329 Removed Protected Overrides Sub InitializeComponent() as this prevents the visual designer working.
            //strW.WriteLine("End Sub");

            //strW.WriteLine("Protected Overrides Sub InitializeComponent()");
            //strW.WriteLine("MyBase.InitializeComponent()");

            //strW.WriteLine("Me.bsParent.DataSource = Me." + strDatabase + "DataSet");
            strW.WriteLine("End Sub");
            strW.WriteLine("#End Region");

            strW.WriteLine("#Region \"Load\"");
            strW.WriteLine("Protected Overrides Sub FillTableAdapter()");

            //20090129 RPB Changed strDatabase + DataSet to TheDataSet.
            //20100326 Store the row position and restore after refresh.
                    strW.WriteLine("vParent.RefreshCombos()");
                    strW.WriteLine("vParent.StoreRowIndexWithFocus()");
                    strW.WriteLine("Me.taParent.Fill(Me.TheDataSet." + strTable + ")");
                    strW.WriteLine("vParent.ResetFocusRow()");
            //strW.WriteLine("Me.taParent.Fill(Me." + strDatabase + "DataSet." + strTable + ")");

            strW.WriteLine("'Set the filter if necessary.");
            strW.WriteLine("vParent.ColumnDoubleClick(FilterFields)");
            strW.WriteLine("End Sub");

            //20081009
            strW.WriteLine("Protected Overrides Sub RefreshCombos()");
            strW.WriteLine("vParent.RefreshCombos()");
            strW.WriteLine("End Sub");
            strW.WriteLine("#End Region");
            return "";
            }

            /// <summary>
            /// Write the header and New sub.
            /// </summary>
        static string strCreateFormFileHeader_version2(TableViewBase t, StreamWriter strW, string strTable, string strDG, string strActualTableName, string TheDataSet)
            {
                strW.WriteLine("'------------------------------------------------");
                strW.WriteLine("'Name: Module genForm_" + strTable + ".vb.");
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
                //20090407 Changed to ProjectGenericform
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

                strW.WriteLine("Public Sub New(ByVal tsb As ToolStripButton _");
                strW.WriteLine(", ByVal strSecurityName As String, ByVal _MainDefs As MainDefinitions, _");
                strW.WriteLine("Fields as Dictionary(Of String, String), _blnFullSize as Boolean, _bRO as boolean, blnFilter as boolean)");
                strW.WriteLine("MyBase.New(tsb, strSecurityName, _MainDefs, Fields, _blnFullSize, _bRO)");
                strW.WriteLine("InitializeComponent()");
                strW.WriteLine("vParent = New " + strTable + "(MainDefs.strGetTableText(strSecurityName), Me.bsParent, Me.dgParent, _");
                strW.WriteLine("taParent, _");
                strW.WriteLine("Me." + TheDataSet+ ", _");
                strW.WriteLine("Me._components, _");
                strW.WriteLine("_MainDefs, blnRO, Me.Controls, Me, blnFilter)");
                //20120531 No longer needed when blnFilter is in New. strW.WriteLine("vParent.CreateFilterBoxes(Me.Controls)");
                strW.WriteLine("End Sub");
                strW.WriteLine("#End Region");

                strW.WriteLine("#Region \"Load\"");
                strW.WriteLine("Protected Overrides Sub FillTableAdapter()");

                strW.WriteLine("vParent.RefreshCombos()");
                //20100326 Store the row position and restore after refresh.
                strW.WriteLine("vParent.StoreRowIndexWithFocus()");
                strW.WriteLine("Me.taParent.Fill(Me." + TheDataSet+ "." + strTable + ")");
                strW.WriteLine("vParent.ResetFocusRow()");

                strW.WriteLine("'Set the filter if necessary.");
                strW.WriteLine("vParent.ColumnDoubleClick(FilterFields)");
                strW.WriteLine("End Sub");

                //20081009
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
            //if (iVersion >= 2) 
                strCreateFormFileHeader_version2(t, strW, strTable, strDG, strActualTableName, strDataSet);
            //else strCreateFormFileHeader(t, strW, strTable, strDG, strActualTableName);
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
            strW = new StreamWriter(strOutputDirectory + "gen_" + strDataSet + "_" + strTableName + ".vb");
            strCreateTableFile(db, strW, t, false, strTableName, "dg", strDataSet);
            strCreateEditing(strW, t, strTableName, "dg", strDataSet);
            strCreateFilterFile(strW, t, strTableName, "dg");
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
            strCreateFilterFile(strW, t, strTableName, "dg");
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
        static void Main(string[] args)
        {
            bool blnDoTables =  true;
            bool blnDoViews = true;
//            blnDoViews = false;
             //blnDoTables = false;
             //Server server = new Server("localhost");
            Server server = new Server("RPB4\\SQLDEV");
            DatabaseCollection dbs = server.Databases;

            //20120608 Set a reference to the project directory in order to check whether tables or views are needed for the application.
            //Fill with "" if this check is not required.
            string strApplicationPath = "d:\\projects\\AppsWild\\WorkingCopy\\MRPTool\\MRPTool_GUI\\LS\\";
            strApplication = "";

            //strDatabase = "HWE";
            // strDatabase = "TPIData";
            //strDatabase = "MulCh";
            //strDatabase = "TPIDataClient";
            //strApplication = "TPINet";
            //iVersion = 2;
            //blnDoViews = false;
            //blnDoTables = false;
            //strDatabase = "CJPT";
            //strDatabase = "ABC";
      
            //strDatabase = "RED2";
            //strApplication = "RED";

            
            //strDatabase = "KMAP";
            //strDatabase = "GRASSEMES";
            //strApplication = "VIA";
            //strDatabase = "RAP2";
            //strApplication = "RAP";
            //strDatabase = "dispensercheck";

            //strDatabase = "TPIDataClient";
            //strDatabase = "TekLogixForce";
            //strApplication = "SFC";
           //strDatabase = "batchcalc";
            //strApplication = "BCNet";
           // strDatabase = "ERPInterface";
            //strApplication = "LOPNet"; 
            //iVersion= 3;

            //strDatabase = "gis";
            //strApplication = "";
     

            //Define the application name for reference to the dataset.
            //if (strApplication.Length == 0)
            //    strApplication = strDatabase;
            
            //except for Utilities
            //strDatabase = "Utilities";
           // strApplication = "";

            XsdFiles xsdFiles = new XsdFiles(strApplicationPath);
            strDatabase = "longshort2";
            strApplication = "MRPTool";
            string strOutputDirectory = "d:\\projects\\GenUI\\OUTPUT\\";

            Database db = dbs[strDatabase];    //Database db = dbs["LONGSHORT2"];
            Dictionary<string, string> CreateAs = new Dictionary<string, string>();
            {
                StreamWriter strW;
                Console.WriteLine(db.Name);
                if (blnDoTables == true)
                {
                    foreach (Table t in db.Tables)
                    {
                        try
                        {
                            {
                              // if (t.Name == "Storage Loc")
                                
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
                }


                if (blnDoViews == true)
                {

                    foreach (View t in db.Views)
                    {

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


            }
            server.ConnectionContext.Disconnect();
            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }
    }
}
