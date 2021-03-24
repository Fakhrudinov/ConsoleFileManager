using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text;
using System.Xml;

namespace ConsoleFileManager
{
    public class DataXML
    {
        public int XMLConsoleWidth { get; set; }
        public int XMLConsoleHeight { get; set; }
        public string XMLStartDirectoryLeft { get; set; }
        public string XMLStartDirectoryRight { get; set; }

        public void GetDataFromXML()
        {
            string pathConfigXML = "Resources/Config.xml";

            //Get first 'ready' drive and set it as default
            string defaultDrive = "";
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady)
                {
                    defaultDrive = drive.Name;
                    break;
                }
            }

            XmlDocument xmlConfig = new XmlDocument();            

            //Config XML Exist?
            if (!File.Exists(pathConfigXML))//if can not find file
            {
                //File with configuration info not found. Set default values

                XMLConsoleWidth = 100;//default value
                XMLConsoleHeight = 40;//default value
                XMLStartDirectoryLeft = defaultDrive;
                XMLStartDirectoryRight = defaultDrive;

                CreateFile(pathConfigXML, defaultDrive);
            }
            else
            {
                try
                {
                    xmlConfig.Load(pathConfigXML);
                }
                catch (Exception)
                {
                    //check has root
                    if (xmlConfig.DocumentElement == null)
                    {
                        // Create the root element
                        XmlElement rootNode = xmlConfig.CreateElement("Settings");
                        xmlConfig.AppendChild(rootNode);
                        xmlConfig.Save(pathConfigXML);
                    }

                    //check XmlDeclaration
                    if (xmlConfig.FirstChild.NodeType != XmlNodeType.XmlDeclaration)
                    {
                        XmlDeclaration xmlDeclaration = xmlConfig.CreateXmlDeclaration("1.0", "utf-8", null);
                        xmlConfig.InsertBefore(xmlDeclaration, xmlConfig.DocumentElement);
                        xmlConfig.Save(pathConfigXML);
                    }

                    xmlConfig.Load(pathConfigXML);
                }

                //console width
                XmlNode nodeWidth = xmlConfig.SelectSingleNode("//Width");
                if (nodeWidth != null)
                {
                    string consoleWidth = nodeWidth.InnerText;
                    if (Int32.TryParse(consoleWidth, out int intWidth))
                    {
                        XMLConsoleWidth = intWidth;
                    }
                    else
                    {
                        XMLConsoleWidth = 100;
                        SetValueToXML(xmlConfig, nodeWidth, pathConfigXML, "100");
                    }
                }
                else
                {
                    XMLConsoleWidth = 100;//default value

                    CreateNodeXML(xmlConfig, pathConfigXML, "Width", "100");
                }

                //console height
                XmlNode nodeHeight = xmlConfig.SelectSingleNode("//Height");
                if (nodeHeight != null)
                {
                    string consoleHeight = nodeHeight.InnerText;
                    if (Int32.TryParse(consoleHeight, out int intHeight))
                    {
                        XMLConsoleHeight = intHeight;
                    }
                    else
                    {
                        XMLConsoleHeight = 40;
                        SetValueToXML(xmlConfig, nodeHeight, pathConfigXML, "40");
                    }
                }
                else
                {
                    XMLConsoleHeight = 40;//default value

                    CreateNodeXML(xmlConfig, pathConfigXML, "Height", "40");
                }

                //file manager last left directory 
                XmlNode nodeLastDirL = xmlConfig.SelectSingleNode("//StartDirLeft");
                if (nodeLastDirL != null)
                {
                    string LastDir = nodeLastDirL.InnerText;
                    if (Directory.Exists(LastDir))
                    {
                        XMLStartDirectoryLeft = LastDir;
                    }
                    else
                    {
                        XMLStartDirectoryLeft = defaultDrive;//default value
                        SetValueToXML(xmlConfig, nodeLastDirL, pathConfigXML, defaultDrive);
                    }
                }
                else
                {
                    XMLStartDirectoryLeft = defaultDrive;//default value

                    CreateNodeXML(xmlConfig, pathConfigXML, "StartDirLeft", defaultDrive);
                }

                //file manager last right directory  
                XmlNode nodeLastDirR = xmlConfig.SelectSingleNode("//StartDirRight");
                if (nodeLastDirR != null)
                {
                    string LastDir = nodeLastDirR.InnerText;
                    if (Directory.Exists(LastDir))
                    {
                        XMLStartDirectoryRight = LastDir;
                    }
                    else
                    {
                        XMLStartDirectoryRight = defaultDrive;//default value
                        SetValueToXML(xmlConfig, nodeLastDirR, pathConfigXML, defaultDrive);
                    }
                }
                else
                {
                    XMLStartDirectoryRight = defaultDrive;//default value

                    CreateNodeXML(xmlConfig, pathConfigXML, "StartDirRight", defaultDrive);
                }
            }
        }

        private void CreateFile(string pathConfigXML, string defaultDrive)
        {
            string text = $"" +
                $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Settings>\r\n" +
                "   <StartDirLeft>" + defaultDrive + "</StartDirLeft>\r\n" +
                "   <StartDirRight>" + defaultDrive + "</StartDirRight>\r\n" +
                "   <!--<ShowHiddenFiles>1</ShowHiddenFiles>-->\r\n" +
                "   <Width>100</Width>\r\n" +
                "   <Height>40</Height>\r\n" +
                "</Settings>\r\n";
            try
            {
                using (StreamWriter sw = new StreamWriter(pathConfigXML, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(text);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void CreateNodeXML(XmlDocument xmlConfig, string pathConfigXML, string nodeName, string nodeValue)
        {
            //создать ноду в xml
            XmlNode newNode = xmlConfig.CreateNode("element", nodeName, "");
            newNode.InnerText = nodeValue;//default value

            XmlElement root = xmlConfig.DocumentElement;
            root.AppendChild(newNode);
            xmlConfig.Save(pathConfigXML);
        }

        private void SetValueToXML(XmlDocument xmlConfig, XmlNode nodeWidth, string pathConfigXML, string nodeValue)
        {
            nodeWidth.InnerText = nodeValue;
            xmlConfig.Save(pathConfigXML);
        }
    }
}
