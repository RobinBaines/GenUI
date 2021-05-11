////////////////////////////////////////////////////////////////////
//Copyright @2012 Robin Baines
//MakeGraph.
//Generate linked list of tables views and datasets.
////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.IO;
using System.Collections.ObjectModel;
using System.Drawing;
using GenLib;

namespace MakeGraph
{
    class Node
    {
        public Node(string _Key, string _Value, bool _IsDataSet, bool _IsTable, bool _IsView)
        {
            Key = _Key;
            Value = _Value;
           IsDataSet= _IsDataSet;
           IsTable= _IsTable;
           IsView = _IsView;
        }

        public bool ContainsValue(Node kvp, Node org_kvp)
        {
            if (Key == kvp.Value && Key != org_kvp.Key) return true;
            return false;
        }

       public string Key;
       public string Value;
       public bool IsDataSet;
       public bool IsTable;
       public bool IsView;
    }

    class MakeGraph
    {
        static String strDatabase;
        static string strApplicationPath;
        static string strServer;
        static bool blnWait;
        
        static List<Node> Nodes = new List<Node>();
        static List<string> NodesWritten = new List<string>();

        static void Main(string[] args)
        {
            blnWait = true;
            if (args == null || args.Length < 3)
            {

                Console.WriteLine("makegraph Server Database ApplicationPath Highlight OutputType OutputPath ");
                Console.WriteLine("Use redirection to write javascript to a file.");
                Console.WriteLine("Server - name of the SQL Server.");
                Console.WriteLine("Database - name of the SQL Server database.");
                Console.WriteLine("ApplicationPath - path to the Visual Studio xsd files. Default is no path.");
                return;
            }
            else
            {
                strApplicationPath = "";
                blnWait = false;
                strServer = args[0];
                strDatabase = args[1];
                if (args.Length > 2)
                    strApplicationPath = args[2];
          
            }

            Console.WriteLine("//Javascript file created by MakeGraph.");
            Console.WriteLine("var links_org = [");
            Server server = new Server(strServer);
            DatabaseCollection dbs = server.Databases;

            //builds an empty list if Application Path is not a path.
            XsdFiles xsdFiles= new XsdFiles(strApplicationPath);
            Database db = dbs[strDatabase];
            foreach (Table t in db.Tables)
            {
                ConnectedViews(db, t.Name, true);
                ConnectedDataSets(xsdFiles, t.Name, true);
            }

            foreach (View t in db.Views)
            {
                if (t.IsSystemObject == false)
                {
                    ConnectedViews(db, t.Name, false);
                    ConnectedDataSets(xsdFiles, t.Name, false);
                }
            }
            server.ConnectionContext.Disconnect();
            Console.WriteLine("];");
            if (blnWait == true) Console.ReadKey();
        }      
              
        static void ConnectedViews(Database db, string parent, bool ParentIsTable)
        {
            string strParent = parent.ToUpper();
            string Output_parent = parent.Replace(' ', '_').Replace('$', '_');
            foreach (View t in db.Views)
            {
                if (t.IsSystemObject == false)
                {
                    //This one will identify an inline select to a 'FROM table' even if it is commented out in the original.
                    //so try to avoid --, SELECT order_id FROM b_order WHERE...
                    if (t.Name != parent && t.TextBody.ToUpper().Contains(parent.ToUpper()))
                    {
                        string strTables = t.TextBody.ToUpper();
                        int iFrom = strTables.IndexOf("FROM");
                        if (iFrom >= 0)
                        {
                        strTables = strTables.Substring(iFrom);
                        string[] fields = strTables.Split(new Char[] { '.',' ', ',', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string strT in fields)
                            if (strT == strParent || strT.Replace(" ", "") == "[" + strParent + "]")
                            {
                                string Output_Name = t.Name.Replace(' ', '_').Replace('$', '_');
                                string strType = "view";
                                if (ParentIsTable)
                                        strType = "table";
                                Console.WriteLine("{ source: \"" + Output_parent + "\", target: \"" + Output_Name + "\", type: \"" + strType + "\" },");
                                Nodes.Add(new Node(Output_parent, Output_Name, false, ParentIsTable, !ParentIsTable));
                                break;
                            }
                        }
                    }
                }
            }
        }

        static private void ConnectedDataSets(XsdFiles xsdFiles, string strTable, bool ParentIsTable)
        {
            if (strApplicationPath.Length > 0)
            {
                foreach (XsdFile xsdF in xsdFiles.xsdFiles)
                {
                    if (xsdF.findtable(strTable))
                    {
                    string strType = "view";
                    if (ParentIsTable)
                        strType = "table";
                    Console.WriteLine("{ source: \"" + strTable.Replace(' ', '_') + "\", target: \"" + xsdF.GetDataSetName() + "\", type: \"" + strType + "\" },");
                    Nodes.Add(new Node(strTable.Replace(' ', '_'), xsdF.GetDataSetName(), true, false, false));
                    }
                }
            }
        }

    }


    }


