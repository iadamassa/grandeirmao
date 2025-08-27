using ClosedXML.Excel;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Infrastructure.Services;

public interface IExcelExportService
{
    byte[] ExportSubjectsToExcel(IEnumerable<SubjectToResearchDto> subjects);
    byte[] ExportSiteCategoriesToExcel(IEnumerable<SiteCategoryDto> categories);
    byte[] ExportSitesToExcel(IEnumerable<SiteDto> sites);
    byte[] ExportAnomaliesToExcel(IEnumerable<AnomalyDto> anomalies);
}

public class ExcelExportService : IExcelExportService
{
    public byte[] ExportSubjectsToExcel(IEnumerable<SubjectToResearchDto> subjects)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Assuntos a Pesquisar");

        // Headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Nome";
        worksheet.Cell(1, 3).Value = "Descrição";
        worksheet.Cell(1, 4).Value = "Status";
        worksheet.Cell(1, 5).Value = "Criado em";
        worksheet.Cell(1, 6).Value = "Atualizado em";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Data
        int row = 2;
        foreach (var subject in subjects)
        {
            worksheet.Cell(row, 1).Value = subject.Id;
            worksheet.Cell(row, 2).Value = subject.Name;
            worksheet.Cell(row, 3).Value = subject.Description;
            worksheet.Cell(row, 4).Value = subject.IsActive ? "Ativo" : "Inativo";
            worksheet.Cell(row, 5).Value = subject.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cell(row, 6).Value = subject.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "";
            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportSiteCategoriesToExcel(IEnumerable<SiteCategoryDto> categories)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Categorias de Sites");

        // Headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Nome";
        worksheet.Cell(1, 3).Value = "Descrição";
        worksheet.Cell(1, 4).Value = "Cor";
        worksheet.Cell(1, 5).Value = "Status";
        worksheet.Cell(1, 6).Value = "Criado em";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

        // Data
        int row = 2;
        foreach (var category in categories)
        {
            worksheet.Cell(row, 1).Value = category.Id;
            worksheet.Cell(row, 2).Value = category.Name;
            worksheet.Cell(row, 3).Value = category.Description;
            worksheet.Cell(row, 4).Value = category.Color;
            worksheet.Cell(row, 5).Value = category.IsActive ? "Ativo" : "Inativo";
            worksheet.Cell(row, 6).Value = category.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportSitesToExcel(IEnumerable<SiteDto> sites)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sites");

        // Headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Nome";
        worksheet.Cell(1, 3).Value = "URL";
        worksheet.Cell(1, 4).Value = "Descrição";
        worksheet.Cell(1, 5).Value = "Status";
        worksheet.Cell(1, 6).Value = "Última Verificação";
        worksheet.Cell(1, 7).Value = "Criado em";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightYellow;

        // Data
        int row = 2;
        foreach (var site in sites)
        {
            worksheet.Cell(row, 1).Value = site.Id;
            worksheet.Cell(row, 2).Value = site.Name;
            worksheet.Cell(row, 3).Value = site.Url;
            worksheet.Cell(row, 4).Value = site.Description;
            worksheet.Cell(row, 5).Value = site.IsActive ? "Ativo" : "Inativo";
            worksheet.Cell(row, 6).Value = site.LastVerification?.ToString("dd/MM/yyyy HH:mm") ?? "";
            worksheet.Cell(row, 7).Value = site.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportAnomaliesToExcel(IEnumerable<AnomalyDto> anomalies)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Anomalias");

        // Headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Site Link";
        worksheet.Cell(1, 3).Value = "URL do Link";
        worksheet.Cell(1, 4).Value = "Assunto";
        worksheet.Cell(1, 5).Value = "Data/Hora Estimativa";
        worksheet.Cell(1, 6).Value = "Assunto Identificado";
        worksheet.Cell(1, 7).Value = "Exemplo/Motivo";
        worksheet.Cell(1, 8).Value = "Criado em";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightCoral;

        // Data
        int row = 2;
        foreach (var anomaly in anomalies)
        {
            worksheet.Cell(row, 1).Value = anomaly.Id;
            worksheet.Cell(row, 2).Value = anomaly.SiteLinkName;
            worksheet.Cell(row, 3).Value = anomaly.SiteLinkUrl ?? ""; // Ensure not null
            worksheet.Cell(row, 4).Value = anomaly.SubjectName;
            worksheet.Cell(row, 5).Value = anomaly.EstimatedDateTime.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cell(row, 6).Value = anomaly.IdentifiedSubject;
            worksheet.Cell(row, 7).Value = anomaly.ExampleOrReason;
            worksheet.Cell(row, 8).Value = anomaly.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}