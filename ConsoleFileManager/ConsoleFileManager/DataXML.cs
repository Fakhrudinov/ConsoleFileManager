using System;
using System.IO;
using System.Xml;

namespace ConsoleFileManager
{
    internal class DataXML
    {
        internal int XMLConsoleWidth { get; set; }
        internal int XMLConsoleHeight { get; set; }
        internal int XMLLeftActiveItem { get; set; }
        internal int XMLRightActiveItem { get; set; }
        internal bool LeftIsActive { get; set; }
        internal string XMLStartDirectoryLeft { get; set; }
        internal string XMLStartDirectoryRight { get; set; }

        internal void GetDataFromXML()
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
                XMLLeftActiveItem = 0;
                XMLRightActiveItem = 0;
                LeftIsActive = true;            

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

                //is left panel active
                XmlNode nodeActive = xmlConfig.SelectSingleNode("//LeftIsActive");
                if (nodeActive != null)
                {
                    string isActive = nodeActive.InnerText;
                    if (Boolean.TryParse(isActive, out bool boolActive))
                    {
                        LeftIsActive = boolActive;
                    }
                    else
                    {
                        LeftIsActive = true;
                        SetValueToXML(xmlConfig, nodeActive, pathConfigXML, "true");
                    }
                }
                else
                {
                    LeftIsActive = true;//default value

                    CreateNodeXML(xmlConfig, pathConfigXML, "LeftIsActive", "true");
                }

                //right panel active item
                XmlNode nodeRightActive = xmlConfig.SelectSingleNode("//RightActiveItem");
                if (nodeRightActive != null)
                {
                    string rightActive = nodeRightActive.InnerText;
                    if (Int32.TryParse(rightActive, out int intRightActive))
                    {
                        XMLRightActiveItem = intRightActive;
                    }
                    else
                    {
                        XMLRightActiveItem = 0;
                        SetValueToXML(xmlConfig, nodeRightActive, pathConfigXML, "0");
                    }
                }
                else
                {
                    XMLRightActiveItem = 0;//default value

                    CreateNodeXML(xmlConfig, pathConfigXML, "RightActiveItem", "0");
                }

                //left panel active item
                XmlNode nodeLeftActive = xmlConfig.SelectSingleNode("//LeftActiveItem");
                if (nodeLeftActive != null)
                {
                    string leftActive = nodeLeftActive.InnerText;
                    if (Int32.TryParse(leftActive, out int intLeftActive))
                    {
                        XMLLeftActiveItem = intLeftActive;
                    }
                    else
                    {
                        XMLLeftActiveItem = 0;
                        SetValueToXML(xmlConfig, nodeLeftActive, pathConfigXML, "0");
                    }
                }
                else
                {
                    XMLLeftActiveItem = 0;//default value

                    CreateNodeXML(xmlConfig, pathConfigXML, "LeftActiveItem", "0");
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
                XmlNode nodeLastDirL = xmlConfig.SelectSingleNode("//LeftStartDir");
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

                    CreateNodeXML(xmlConfig, pathConfigXML, "LeftStartDir", defaultDrive);
                }

                //file manager last right directory  
                XmlNode nodeLastDirR = xmlConfig.SelectSingleNode("//RightStartDir");
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

                    CreateNodeXML(xmlConfig, pathConfigXML, "RightStartDir", defaultDrive);
                }
            }
        }

        /// <summary>
        /// set value and save xml doc
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="nodeToChange"></param>
        /// <param name="pathConfigXML"></param>
        /// <param name="nodeValue"></param>
        private void SetValueToXML(XmlDocument xmlDoc, XmlNode nodeToChange, string pathConfigXML, string nodeValue)
        {
            nodeToChange.InnerText = nodeValue;
            xmlDoc.Save(pathConfigXML);
        }

        /// <summary>
        /// create default file
        /// </summary>
        /// <param name="pathConfigXML"></param>
        /// <param name="defaultDrive"></param>
        private void CreateFile(string pathConfigXML, string defaultDrive)
        {
            string text = $"" +
                $"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                "<Settings>" + Environment.NewLine +
                "   <LeftIsActive>True</LeftIsActive>" + Environment.NewLine +
                "   <LeftStartDir>" + defaultDrive + "</LeftStartDir>" + Environment.NewLine +
                "   <RightStartDir>" + defaultDrive + "</RightStartDir>" + Environment.NewLine +
                "   <LeftActiveItem>0</LeftActiveItem>" + Environment.NewLine +
                "   <RightActiveItem>0</RightActiveItem>" + Environment.NewLine +
                "   <Width>100</Width>" + Environment.NewLine +
                "   <Height>40</Height>" + Environment.NewLine +
                "</Settings>" + Environment.NewLine;
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
                ClassLibrary.Do.ShowAlert($"Save to configuration XML file {pathConfigXML} Error - " + e.Message, 20);
            }
        }

        /// <summary>
        /// new node to xml
        /// </summary>
        /// <param name="xmlConfig"></param>
        /// <param name="pathConfigXML"></param>
        /// <param name="nodeName"></param>
        /// <param name="nodeValue"></param>
        private void CreateNodeXML(XmlDocument xmlConfig, string pathConfigXML, string nodeName, string nodeValue)
        {
            //create node in xml
            XmlNode newNode = xmlConfig.CreateNode("element", nodeName, "");
            newNode.InnerText = nodeValue;//default value

            XmlElement root = xmlConfig.DocumentElement;
            root.AppendChild(newNode);
            xmlConfig.Save(pathConfigXML);
        }
    }
}
