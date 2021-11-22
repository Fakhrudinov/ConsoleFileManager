using System;

namespace ReportingLib.Models
{
    public class DirectoryInfoModel
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModyfied { get; set; }
        public string[] Attributes { get; set; }
    }
}
