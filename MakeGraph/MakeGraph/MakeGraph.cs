
////////////////////////////////////////////////////////////////////
//Copyright @2012 Robin Baines
//MakeGraph.
//Generate graph of tables views and datasets.
//The 5th parameter changes the output. Using MDG means make a directed graph file for use by Bunch. 
//Anything but MDG means ouput is a dot file.

//20130902 Create a dot file per dataset.
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
        static string strHighLight;
        static string strApplicationPath;
        static string strDataSetOutputPath;
        static string strServer;
        static string strOutput;
        static bool blnWait;

         const string MDGSTRING = "MDG";
         const string JSSTRING = "JS";

        static List<Node> Nodes = new List<Node>();
        static List<string> NodesWritten = new List<string>();

        static void Main(string[] args)
        {
            bool blnRemoveUtilities = false;
            strOutput = "";
            blnWait = true;

            strApplicationPath = "c:\\projects\\spits\\GIS\\gis-gui\\GIS\\";

           // strApplicationPath = "c:\\Projects\\AppsWild\\WorkingCopy\\RAP\\RAP_GUI\\RAP\\";
            strDataSetOutputPath = "c:\\Projects\\GenUI\\RUN\\OUTPUT\\";
            if (args == null || args.Length < 4)
            {
                //strOutput = MDGSTRING;  //MDG means just make a directed graph file. Not MDG is a dot file.
                //strOutput = "";
                strOutput = JSSTRING;
                strDatabase = "gis2";
                strHighLight = "v_zinr_edit";
                strHighLight = "NO";
                strServer = "RPB5\\SQLDEV2012";
                //Server server = new Server("RPB4");
                //strApplicationPath = "d:\\projects\\AppsWild\\WorkingCopy\\MRPTool\\MRPTool_GUI\\LS\\";
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
                if (args.Length > 5)
                    strApplicationPath = args[5];
            }

            if (strOutput.ToUpper() == "")
                FileHeader(null);
            if (strOutput == JSSTRING)
            {
                Console.WriteLine("//Javascript file created by MakeGraph.");
                Console.WriteLine("var links_org = [");
            }

            Server server = new Server(strServer);
            DatabaseCollection dbs = server.Databases;
            XsdFiles xsdFiles= new XsdFiles(strApplicationPath);
            
            //allow lines with same destination to merge.
            //Console.WriteLine("concentrate = true;");
            Database db = dbs[strDatabase];
            foreach (Table t in db.Tables)
            {
                if (IsUtility(blnRemoveUtilities, t.Name) == false)
                {
                    //just write the name of the table so that it is shown even if not used further.
                    if (strOutput.ToUpper() == "")
                        Console.WriteLine(t.Name.Replace(' ', '_').Replace('$', '_') + ";");

                    ConnectedViews(db, t.Name, true);
                    ConnectedDataSets(xsdFiles, t.Name);
                }
            }

            foreach (View t in db.Views)
            {
                if (t.IsSystemObject == false)
                {
                    //just write the name of the table so that it is shown even if not used further.
                    if (strOutput.ToUpper() == "") Console.WriteLine(t.Name.Replace(' ', '_').Replace('$', '_') + ";");
                    ConnectedViews(db, t.Name, false);
                    ConnectedDataSets(xsdFiles, t.Name);
                }
            }
            server.ConnectionContext.Disconnect();
            if (strOutput.ToUpper() == "")Console.WriteLine(" }");

            foreach (XsdFile xsdF in xsdFiles.xsdFiles)
            {
                FilterOnDataSet(xsdF.GetDataSetName());
            }
            //filter on dataset and make a file.

            if (strOutput == JSSTRING)
            {
                Console.WriteLine("];");
            }

            //Console.WriteLine("Press any key.");
            if (blnWait == true) Console.ReadKey();
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
                    //This one will identify an inline select to a 'FROM table' even if it is commented out in the original.
                    //so try to avoid --, SELECT order_id FROM b_order WHERE...
                    if (t.Name != parent && t.TextBody.ToUpper().Contains(parent.ToUpper()))
                    {
                        //if (t.Name == "v_order_facility_month")
                        //{
                        //    Console.WriteLine(t.TextBody);
                        //}

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
                                    if (strOutput.ToUpper() == MDGSTRING)
                                    {
                                        Console.WriteLine(Output_parent + " " + Output_Name);
                                       
                                    }
                                    else
                                    {
                                        if (strOutput.ToUpper() == JSSTRING)
                                        {
                                            string strType = "suit";
                                            if (ParentIsTable)
                                                strType = "licensing";
                                            Console.WriteLine("{ source: \"" + Output_parent + "\", target: \"" + Output_Name + "\", type: \"" + strType + "\" },");
                                        }
                                        else
                                        {
                                            if (ParentIsTable)
                                                Console.WriteLine(Output_parent + " -> " + Output_Name + "[style=bold, color=blue];");
                                            else
                                                Console.WriteLine(Output_parent + " -> " + Output_Name + ";");
                                        }
                                    }
                                    Nodes.Add(new Node(Output_parent, Output_Name, false, ParentIsTable, !ParentIsTable));
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
                    {
                        if (strOutput.ToUpper() == MDGSTRING)
                        {
                            Console.WriteLine(strTable.Replace(' ', '_') + " " + xsdF.GetDataSetName());
                        }
                        else
                        {
                            if (strOutput.ToUpper() == JSSTRING)
                            {
                                string strType = "suit";
                                //if (ParentIsTable)
                                    strType = "licensing";
                                    Console.WriteLine("{ source: \"" + strTable.Replace(' ', '_') + "\", target: \"" + xsdF.GetDataSetName() + "\", type: \"" + strType + "\" },");
                            }
                            else
                                Console.WriteLine(strTable.Replace(' ', '_') + " -> " + xsdF.GetDataSetName() + "[style=bold, color=" + xsdF.GetColor() + "];");
                        }
                         Nodes.Add(new Node(strTable.Replace(' ', '_'), xsdF.GetDataSetName(), true, false, false));
                    }
                }
            }
        }


       static private void FilterOnDataSet(string strDataSet)
        {

           StreamWriter outfile = new StreamWriter(strDataSetOutputPath + strDataSet + ".txt");
           FileHeader(outfile);
       
           List<Node> CheckedNodes = new List<Node>();
          foreach(Node kvp in Nodes )
          {
              try
              {
                  CheckedNodes.Clear();
                  if (CheckNext(kvp, strDataSet, kvp, CheckedNodes) == true)
                  {
                      if (!NodesWritten.Contains(kvp.Key))
                      {
                          NodesWritten.Add(kvp.Key);
                          outfile.Write(kvp.Key);
                          outfile.WriteLine(";");
                      }

                      outfile.Write(kvp.Key + " -> " + kvp.Value);
                      if (kvp.Value == strDataSet)
                          outfile.WriteLine("[style=bold, color=Salmon];");
                      else
                          if (kvp.IsTable == true)
                            outfile.WriteLine("[style=bold, color=blue];");
                          else
                              outfile.WriteLine(";");
                  }
              }
              catch
              {
                  break;
              }
          }
          FileTail(outfile);
          outfile.Close();
        }

       //make a list of Nodes whihc have Key = Value of current Node. Is important not to repeat Nodes to avoid loops in the graph for the Root Node.
       //CheckedNodes contains the list of Nodes already checked for this Root Node.
       static private List<Node> ContainsValue(Node kvp, Node org_kvp, List<Node> CheckedNodes, ref List<Node> LocalNodes)
       {
           try
           {
               foreach (Node N in Nodes)
               {
                   if (N.ContainsValue(kvp, org_kvp) == true)
                   {
                       if (!CheckedNodes.Contains(N))
                       {
                           if (!LocalNodes.Contains(N))
                           {
                               LocalNodes.Add(N);
                               if (LocalNodes.Count > 10)
                               {
                                   string str = N.Key;
                               }
                           }
                       CheckedNodes.Add(N);
                       }
                   }
               }
               return LocalNodes;
           }
           catch
           {
           }
           return null;
       }

       //recursive function which returns true if the strDataSet is on the graph under the Node kvp.
       static private bool CheckNext(Node kvp, string strDataSet, Node org_kvp, List<Node> CheckedNodes)
        {
            bool blnRet = false;
              if(kvp.Value == strDataSet)
                  {
                    return true;
                  }

                //look for all the Nodes which have a Key = to the Value of the kvp.            
              List<Node> LocalNodes = new List<Node>();
              ContainsValue(kvp, org_kvp, CheckedNodes,  ref LocalNodes);
              if (LocalNodes == null) return false;
              if (LocalNodes.Count == 0)
                  blnRet = false;
              else
              {
                  foreach (Node N in LocalNodes)
                  {
                      if (CheckNext(N, strDataSet, org_kvp, CheckedNodes) == true)
                      {
                          blnRet = true;
                          break;
                      }
                  }
              }
            LocalNodes.Clear();
            LocalNodes = null;
            return blnRet;
        }

        #region "Files"
       static private void FileHeader(StreamWriter outfile)
       {
           string str0 = "/*MakeGraph  server database applicationpath  highlight*/";
           string str1 ="/* server = " + strServer + " database = " + strDatabase + " applicationpath = " + strApplicationPath + " highlight = " + strHighLight + "*/";
           string str2 = "digraph " + strDatabase + " {";
          
           //In dot, this gives the desired rank separation, in inches. This is the minimum vertical distance between the bottom 
           //of the nodes in one rank and the tops of nodes in the next. If the value contains "equally", the centers of all ranks 
           //are spaced equally apart. Note that both settings are possible, e.g., ranksep = "1.2 equally". 
           string str3 = "ranksep = \"1.5 equally\";";
           string str31 = "nodesep = \"0.25\"";
           string str32 = "fontsize = \"18.0\"";
           string str33 = "fontname = \"Helvetica\"";
           string str4 = strHighLight + " [shape = polygon, sides = 5, peripheries = 3, color = lightblue, style = filled];";
           if (outfile == null)
           {
               Console.WriteLine(str0);
               Console.WriteLine(str1);
               Console.WriteLine(str2);
               //vertical rank separation. 1.0 is default.
               Console.WriteLine(str3);
               Console.WriteLine(str31);
               Console.WriteLine(str32);
               Console.WriteLine(str33);
               if (strHighLight.ToUpper() != "NO")
                   Console.WriteLine(str4);
           }
           else
           {
               outfile.WriteLine(str1);
               outfile.WriteLine(str2);
               //vertical rank separation. 1.0 is default.
               outfile.WriteLine(str3);
               outfile.WriteLine(str31);
               outfile.WriteLine(str32);
               outfile.WriteLine(str33);
               if (strHighLight.ToUpper() != "NO")
                   outfile.WriteLine(str4);
           }
       }

       static private void FileTail(StreamWriter outfile)
       {
                if (outfile == null) Console.WriteLine(" }");
                else outfile.WriteLine(" }");
       }
        #endregion
    }


    }


