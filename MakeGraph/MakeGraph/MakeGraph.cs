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
    class MakeGraph
    {
        static String strDatabase;
        static string strApplication;
        static string strApplicationPath;
        static void Main(string[] args)
        {
            List<string> NotIncluded = new List<string>();
            NotIncluded.Add("b_app_parameters");
            NotIncluded.Add("b_app_color");


            strDatabase = "longshort2";
            strApplication = "MRPTool";
            Server server = new Server("RPB4\\SQLDEV");
            strApplicationPath = "d:\\projects\\AppsWild\\WorkingCopy\\MRPTool\\MRPTool_GUI\\LS\\";
            bool blnRemoveUtilities = true;

            //strDatabase = "rap2";
            //strApplication = "rap2";
            //Server server = new Server("RPB4");
            //strApplicationPath = "d:\\projects\\AppsWild\\WorkingCopy\\RAP\\RAP_GUI\\RAP\\";
            //bool blnRemoveUtilities = true;

            //strDatabase = "Utilities";
            //strApplication = "TestApp";
            //Server server = new Server("RPB4");
            //strApplicationDirectory = "d:\\projects\\AppsLibs\\WorkingDirectory\\Libs\\Utilities\\";
            //bool blnRemoveUtilities = false;

            DatabaseCollection dbs = server.Databases;
            XsdFiles xsdFiles = new XsdFiles(strApplicationPath);
            Console.WriteLine("digraph " + strDatabase + " {");

            //allow lines with same destination to merge.
            Console.WriteLine("concentrate = true;");

            //vertical rank separation. 1.0 is default.
            Console.WriteLine("ranksep = \"1.5 equally\";");

            Database db = dbs[strDatabase];
            foreach (Table t in db.Tables)
            {
                //Console.WriteLine("Testing " + t.Name);
                if (IsUtility(blnRemoveUtilities, t.Name)==false)
                {
                    Console.WriteLine(strDatabase + " -> " + t.Name + ";");
                    ConnectedViews(db, t.Name, true);
                    ConnectedDataSets(xsdFiles, t.Name);
                }

//               Console.WriteLine(t.Name);
            }

            foreach (View t in db.Views)
            {
                if (t.IsSystemObject == false)
                {
                    ConnectedViews(db, t.Name, false);
                    if (strApplicationPath.Length > 0)
                    {
                        ConnectedDataSets(xsdFiles, t.Name);
                    }
                }
            }
            server.ConnectionContext.Disconnect();
            Console.WriteLine(" }");
            //Console.WriteLine("Press any key.");
            Console.ReadKey();
           
        }

            
        static bool IsUtility(bool blnRemoveUtilities, string tablename)
        {
            if (blnRemoveUtilities == false) return false;
            if (tablename.StartsWith("m_")) return true;
            return false;
        }

        static void ConnectedViews(Database db, string parent, bool ParentIsTable)
        {
            string strParent = parent.ToUpper();
            foreach (View t in db.Views)
            {
                if (t.IsSystemObject == false)
                {
                    if (t.Name != parent && t.TextBody.ToUpper().Contains(parent.ToUpper()))
                    {
                        string strTables = t.TextBody.ToUpper();
                        int iFrom = strTables.LastIndexOf("FROM");
                        if (iFrom >= 0)
                        {
                            strTables = strTables.Substring(iFrom);
                            string[] fields = strTables.Split(new Char[] { '.',' ', ',', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string strT in fields)
                                if (strT == strParent || strT.Replace(" ", "") == "[" + strParent + "]")
                                {
                                   if (ParentIsTable)
                                       Console.WriteLine(parent + " -> " + t.Name + "[style=bold, color=blue];");
                                   else
                                        Console.WriteLine(parent + " -> " + t.Name + ";");
                                    

                                    break;
                                }
                        }
                    }
                }
            }
        }

        static private void ConnectedDataSets(XsdFiles xsdFiles, string strTable)
        {
            if (strApplicationPath.Length > 0)
            {
                foreach (XsdFile xsdF in xsdFiles.xsdFiles)
                {
                    if (xsdF.findtable(strTable))
                        Console.WriteLine(strTable + " -> " + xsdF.GetDataSetName() + "[style=bold, color=" + xsdF.GetColor() + "];"); 
                }
            }
        }

       
    }

}
