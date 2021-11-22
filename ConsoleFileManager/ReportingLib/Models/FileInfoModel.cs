using System;

namespace ReportingLib.Models
{
    public class FileInfoModel
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool IsReadOnly { get; set; }
        public long FileSize { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModyfied { get; set; }
        public string[] Attributes { get; set; }
    }
}
