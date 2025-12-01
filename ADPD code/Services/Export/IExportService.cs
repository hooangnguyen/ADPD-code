using ADPD_code.Models;

namespace ADPD_code.Services.Export
{
    public interface IExportService
    {              
      byte[] ExportStudentsToExcel(List<StudentExportModel> students);
    }
}
