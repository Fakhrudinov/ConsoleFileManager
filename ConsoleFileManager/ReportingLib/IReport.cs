using ReportingLib.Models;

namespace ReportingLib
{
    public interface IReport
    {
        void ReportAboutCurrentDirectory(DirectoryInfoModel dir);
        void ReportAboutCurrentFile(FileInfoModel file);
    }
}
