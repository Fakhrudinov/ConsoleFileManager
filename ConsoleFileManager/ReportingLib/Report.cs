using ReportingLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using TemplateEngine.Docx;

namespace ReportingLib
{
    public class Report : IReport
    {
        public Report()
        {
            //Check directory exist
            string pathResultedReport = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(pathResultedReport))
            {
                try
                {
                    Directory.CreateDirectory(pathResultedReport);
                }
                catch
                {

                }                
            }
        }

        public void ReportAboutCurrentFile(FileInfoModel file)
        {
            if (file is null)
            {
                return;
            }

            string now = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

            string pathResultedReport = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Reports",
                "Report-File-" + now + ".docx");

            CopyTemlateToResultDir("File", pathResultedReport);

            List<TableRowContent> rows = GetDrivesInfoRows(file.FullName);

            var valuesToFill = new Content(
                new FieldContent("dateTime", now),
                new FieldContent("ShortName", file.Name),
                new FieldContent("FullName", file.FullName),
                new FieldContent("CreationDate", file.Created.ToString()),
                new FieldContent("LastModifiedDate", file.LastModyfied.ToString()),
                new FieldContent("IsReadOnly", file.IsReadOnly.ToString()),
                new FieldContent("FileSize", file.FileSize.ToString()),
                TableContent.Create("TableDisks", rows)
            );

            ListContent listItems = new ListContent("AttributesList");
            listItems = GetAttributes(file.Attributes, listItems);            
            valuesToFill.Lists.Add(listItems);

            SetValuesToFile(pathResultedReport, valuesToFill);
        }

        public void ReportAboutCurrentDirectory(DirectoryInfoModel directory)
        {
            if (directory is null)
            {
                return;
            }

            string now = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

            string pathResultedReport = Path.Combine(
                Directory.GetCurrentDirectory(), 
                "Reports", 
                "Report-Dir-" + now + ".docx");

            CopyTemlateToResultDir("Dir", pathResultedReport);

            List<TableRowContent> rows = GetDrivesInfoRows(directory.FullName);

            var valuesToFill = new Content(
                new FieldContent("dateTime", now),
                new FieldContent("ShortName", directory.Name),
                new FieldContent("FullName", directory.FullName),
                new FieldContent("CreationDate", directory.Created.ToString()),
                new FieldContent("LastModifiedDate", directory.LastModyfied.ToString()),
                TableContent.Create("TableDisks", rows)
            );
            
            ListContent listItems = new ListContent("AttributesList");
            listItems = GetAttributes(directory.Attributes, listItems);            
            valuesToFill.Lists.Add(listItems);

            SetValuesToFile(pathResultedReport, valuesToFill);
        }

        private void CopyTemlateToResultDir(string type, string pathResultedReport)
        {

            string pathToTemplate = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Templates",
                "ReportWith" + type + "Temlate.docx");

            File.Copy(pathToTemplate, pathResultedReport);
        }


        private ListContent GetAttributes(string[] attributes, ListContent listItems)
        {
            if (attributes != null)
            {
                foreach (string attr in attributes)
                {
                    listItems.AddItem(new FieldContent("AttrItem", attr));
                }
            }
            else
            {
                listItems.AddItem(new FieldContent("AttrItem", "нет атрибутов"));
            }

            return listItems;
        }

        private void SetValuesToFile(string pathResultedReport, Content valuesToFill)
        {
            using (var outputDocument = new TemplateProcessor(pathResultedReport).SetRemoveContentControls(true))
            {
                outputDocument.FillContent(valuesToFill);
                outputDocument.SaveChanges();
            }
        }

        private List<TableRowContent> GetDrivesInfoRows(string path)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            List<TableRowContent> rows = new List<TableRowContent>();

            foreach (DriveInfo drive in drives)
            {
                string isCurrent = "";
                if (drive.Name.ToLower().Contains(path.ToLower().Substring(0, 2)))
                {
                    isCurrent = "Current";
                }

                rows.Add(new TableRowContent(new List<FieldContent>()
                {
                    new FieldContent("diskName",        drive.Name),
                    new FieldContent("diskCurrent",     isCurrent),
                    new FieldContent("diskLabel",       drive.VolumeLabel),
                    new FieldContent("diskFormat",      drive.DriveFormat),
                    new FieldContent("diskType",        drive.DriveType.ToString()),
                    new FieldContent("diskReady",       drive.IsReady.ToString()),
                    new FieldContent("diskTotalSize",   drive.AvailableFreeSpace.ToString()),
                    new FieldContent("diskFreeSize",    drive.TotalSize.ToString()),
                }));
            }

            return rows;
        }
    }
}
