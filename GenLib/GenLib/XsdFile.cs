//20120608 
//Make a class to scan .Net xsd files for Table and view names.
//It creates a list on initialisation and then checks whether a table or view is in the list.
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;

namespace GenLib
{

public class XsdFiles
{
    public List<XsdFile> xsdFiles = new List<XsdFile>();
     public XsdFiles(string strApplicationPath)
      {
            
      //20120608 Create the lists of tables required if an strApplicationDirectory is defined.
            List<string> strFiles = new List<string>();

            if (strApplicationPath.Length > 0 && strApplicationPath.ToUpper() != "NO")
            {
                int iCount = 0;
                strFiles.AddRange(Directory.GetFiles(strApplicationPath, "*.Xsd"));
                foreach (string strFile in strFiles)
                {
                    xsdFiles.Add(new XsdFile(strFile, iCount));
                    iCount++;
                }
            }
     }

}

    public class XsdFile
    {
        XmlDocument xmlDoc = new XmlDocument();
        string DataSetName;
        List<string> strTables = new List<string>();
        Color color;
        XmlNamespaceManager nsmgr;
        public XsdFile(string strFilename, int iCount)
        {
            color = GetColor(iCount);
            xmlDoc.Load(strFilename);
            string strNameSpace = "http://www.w3.org/2001/XMLSchema";
            nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("xs", strNameSpace);
            string strNameSpace2 = "urn:schemas-microsoft-com:xml-msdatasource";
            nsmgr.AddNamespace("so", strNameSpace2);

            DataSetName = TheDataSetName(Path.GetFileName(strFilename));
            FillTables();
      
        }

        //get the dataset name by default from the file name but also look for the schema id.
        private string TheDataSetName(string strFilename)
        {
            string _DataSetName = "";
            XmlNodeList oSchemas = xmlDoc.SelectNodes("xs:schema", nsmgr);
            foreach (XmlNode nd in oSchemas)
            {
                foreach (XmlNode att in nd.Attributes)
                {
                    if (att.Name == "id")
                        _DataSetName=att.InnerText;
                }
            }
            return _DataSetName;
        }


        private void FillTables()
        {
            strTables.Clear();
            XmlNodeList oTPIs = xmlDoc.SelectNodes("xs:schema/xs:annotation/xs:appinfo", nsmgr);
            ///xs:appinfo/DataSource/Tables
            foreach (XmlNode node in oTPIs)
            {
                XmlNodeList oTabs = node.SelectNodes("so:DataSource", nsmgr);
                ///Tables/TableAdapter/MainSource/DbSource
                foreach (XmlNode nod in oTabs)
                {
                    XmlNodeList oTbs = nod.SelectNodes("so:Tables/so:TableAdapter/so:MainSource/so:DbSource", nsmgr);
                    ///TableAdapter/MainSource/DbSource
                    ///

                    foreach (XmlNode nd in oTbs)
                    {
                        foreach (XmlNode att in nd.Attributes)
                        {
                            if (att.Name == "DbObjectName")
                                strTables.Add(att.InnerText.ToUpper());
                        }
                    }
                }
            }

        }

        public string GetColor()
        {
            return color.Name;
        }

        //parse the data set name from the file name. Would be better to get from the xsd.
        public string GetDataSetName()
        {
            return DataSetName;
        }

        //return true if the table (or view) name is in the list.
        //strTables array contains sources in upper case.
        public bool findtable(string tablename)
        {
            string strTable = tablename.ToUpper();   //.Replace(" ", "_");
            foreach (string strTs in strTables)
            {
                if (strTs.Contains(strTable))
                {
                string[] fields = strTs.Split(new Char[] { '.', ' ', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string strT in fields)
                    {
                        if (strT == strTable)
                        {
                            return true;
                        }
                    }

                    //we may have a table with spaces in the name.
                    fields = strTs.Split(new Char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string strT in fields)
                    {
                        if (strT == strTable)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
       }


        static private Color GetColor(int iONr)
        {
            if (iONr % 7 == 0)
                return Color.Peru;
            if (iONr % 7 == 1)
                return Color.Salmon;
            if (iONr % 7 == 2)
                return Color.Brown;
            if (iONr % 7 == 3)
                return Color.Tomato;
            if (iONr % 7 == 4)
                return Color.RosyBrown;
            if (iONr % 7 == 5)
                return Color.SandyBrown;
            if (iONr % 7 == 6)
                return Color.LightCoral;
            return Color.Red;


            // Color col  = Color.AntiqueWhite;
            // Byte iAlpha  = 100;
            // Byte iRed  = 0;
            // Byte iGreen  = 0;
            // Byte iBlue  = 0;
            // int iW  = 50;
            // int iFa  = 10;

            // //'Ensure that colour does not exceed 255.
            // iONr = iONr % (255 - iW) / iFa;


            //     int iMod  = iONr % 7;
            //     if (iMod == 0)
            //         iRed = (byte) (iONr * iFa + iW);
            //     {
            //         if (iMod == 1)
            //             iGreen = (byte) (iONr * iFa + iW);
            //         else
            //         {
            //             if (iMod == 2)
            //                 iBlue = (byte) (iONr * iFa + iW);
            //         }
            //     }
            //     if (iMod == 3)
            //     {
            //         iRed = (byte) (iONr * iFa + iW);
            //         iGreen = (byte) (iONr * iFa + iW);
            //     }
            //     else
            //     {
            //         if (iMod == 4)
            //         {
            //             iRed = (byte) (iONr * iFa + iW);
            //             iBlue = (byte) (iONr * iFa + iW);
            //         }
            //         else
            //         {
            //             if (iMod == 5)
            //             {
            //                 iBlue = (byte) (iONr * iFa + iW);
            //                 iGreen = (byte) (iONr * iFa + iW);
            //                 iRed =(byte) (iONr * iFa + iW);
            //             }

            //     }
            // }
            //     if (iMod == 6)
            //     {
            //         iBlue = (byte) (iONr * iFa + iW);
            //         iGreen = (byte) (iONr * iFa + iW);
            //     }

            // return Color.FromArgb(iAlpha, iRed, iGreen, iBlue);
            //}
        }

    }
}

