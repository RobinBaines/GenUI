
////////////////////////////////////////////////////////////////////
//Copyright @2012 Robin Baines
//MakeGraph.
//Generate graph of tables views and datasets.
//The 5th paramter changes the output. Using MDG means make a directed graph file for use by Bunch. 
//Anything but MDG means ouput is a dot file.
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
    class MakeGraph
    {
        static String strDatabase;
        static string strHighLight;
        static string strApplicationPath;
        static string strServer;
        static string strOutput;
        static bool blnWait;
        static void Main(string[] args)
        {
            bool blnRemoveUtilities = false;
            strOutput = "";
            blnWait = true;
            if (args == null || args.Length < 4)
            {
                strOutput = "MDG";  //MDG means just make a directed graph file. Not MDG is a dot file.
                if (strOutput.ToUpper() != "MDG")Console.WriteLine("/*MakeGraph  server database applicationpath  highlight*/"); // Check for null array
                strDatabase = "longshort2";
                strHighLight = "vComponent";
                strServer = "RPB4\\SQLDEV";
                //Server server = new Server("RPB4");
                //strApplicationPath = "d:\\projects\\AppsWild\\WorkingCopy\\MRPTool\\MRPTool_GUI\\LS\\";
                strApplicationPath = "NO";
                
            }
            else
            {
                blnWait = false;
                strServer = args[0];
                strDatabase = args[1];
                strApplicationPath = args[2];
                strHighLight = args[3];
                if (args.Length > 4)
                    strOutput = args[4];
            }

            if (strOutput.ToUpper() != "MDG")
                Console.WriteLine("/* server = " + strServer + " database = " + strDatabase + " applicationpath = " + strApplicationPath + " highlight = " + strHighLight + "*/");
            Server server = new Server(strServer);
                           
             DatabaseCollection dbs = server.Databases;
             XsdFiles xsdFiles= new XsdFiles(strApplicationPath);
            
            //allow lines with same destination to merge.
            //Console.WriteLine("concentrate = true;");


            if (strOutput.ToUpper() != "MDG")
            {
                Console.WriteLine("digraph " + strDatabase + " {");
                //vertical rank separation. 1.0 is default.
                Console.WriteLine("ranksep = \"1.5 equally\";");

                //highlight this node.
                if (strHighLight.ToUpper() != "NO")
                    Console.WriteLine(strHighLight + " [shape = polygon, sides = 5, peripheries = 3, color = lightblue, style = filled];");
            }
            Database db = dbs[strDatabase];
            foreach (Table t in db.Tables)
            {
                if (IsUtility(blnRemoveUtilities, t.Name)==false)
                {
                    //just write the name of the able so that it is shown even if not used further.
                    if (strOutput.ToUpper() != "MDG") Console.WriteLine(t.Name.Replace(' ', '_').Replace('$', '_') + ";");

                    ConnectedViews(db, t.Name, true);
                        ConnectedDataSets(xsdFiles, t.Name);
                }
            }

            foreach (View t in db.Views)
            {
                if (t.IsSystemObject == false)
                {
                    //just write the name of the able so that it is shown even if not used further.
                    if (strOutput.ToUpper() != "MDG") Console.WriteLine(t.Name.Replace(' ', '_').Replace('$', '_') + ";");
                    ConnectedViews(db, t.Name, false);
                    ConnectedDataSets(xsdFiles, t.Name);
                }
            }
            server.ConnectionContext.Disconnect();
            if (strOutput.ToUpper() != "MDG")Console.WriteLine(" }");
            //Console.WriteLine("Press any key.");
            if(blnWait == true)Console.ReadKey();
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
            string Output_parent = parent.Replace(' ', '_').Replace('$', '_');
            foreach (View t in db.Views)
            {
                if (t.IsSystemObject == false)
                {
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
                                    if (strOutput.ToUpper() == "MDG")
                                    {
                                        Console.WriteLine(Output_parent + " " + Output_Name);
                                    }
                                    else
                                    {
                                        if (ParentIsTable)
                                            Console.WriteLine(Output_parent + " -> " + Output_Name + "[style=bold, color=blue];");
                                        else
                                            Console.WriteLine(Output_parent + " -> " + Output_Name + ";");
                                    }

                                    break;
                                }
                        }
                    }
                }
            }
        }

        static private void ConnectedDataSets(XsdFiles xsdFiles, string strTable)
        {
            if (strApplicationPath.Length > 0 && strApplicationPath.ToUpper() != "NO")
            {
                foreach (XsdFile xsdF in xsdFiles.xsdFiles)
                {
                    if (xsdF.findtable(strTable))
                        if (strOutput.ToUpper() == "MDG")
                        {
                            Console.WriteLine(strTable.Replace(' ', '_') + " " + xsdF.GetDataSetName());
                        }
                        else
                        {
                            Console.WriteLine(strTable.Replace(' ', '_') + " -> " + xsdF.GetDataSetName() + "[style=bold, color=" + xsdF.GetColor() + "];");
                        }
                }
            }
        }

       
    }

}
