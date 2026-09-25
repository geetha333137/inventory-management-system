using InventoryManagementSystem.Authorization;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Services.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers;

[PermissionAuthorize("VIEW_REPORT")]
public class ReportsController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly AppDbContext _context;

    public ReportsController(IDashboardService dashboardService, AppDbContext context)
    {
        _dashboardService = dashboardService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Reports";
        ViewData["PageSubtitle"] = "Review inventory totals, asset status and operational distribution.";

        var report = await _dashboardService.GetDashboardAsync();
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var report = await _dashboardService.GetDashboardAsync();
        var assets = await _context.Assets
            .Include(asset => asset.Category)
            .OrderBy(asset => asset.AssetCode)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var summary = workbook.Worksheets.Add("Summary");
        var status = workbook.Worksheets.Add("Status Breakdown");
        var categories = workbook.Worksheets.Add("Categories");
        var assetDetails = workbook.Worksheets.Add("Asset Details");

        AddTitle(summary, "StockSphere Inventory Report");
        summary.Cell("A3").Value = "Metric";
        summary.Cell("B3").Value = "Value";
        StyleHeader(summary.Range("A3:B3"));

        var summaryRows = new[]
        {
            ("Total Assets", report.TotalAssets),
            ("Inventory Items", report.TotalInventoryItems),
            ("Active Assets", report.ActiveAssets),
            ("Inactive Assets", report.InactiveAssets),
            ("Available Assets", report.AvailableAssets),
            ("Assigned Assets", report.AssignedAssets),
            ("Under Maintenance", report.MaintenanceAssets),
            ("Retired Assets", report.RetiredAssets),
            ("Categories", report.CategoryCount)
        };

        for (var index = 0; index < summaryRows.Length; index++)
        {
            summary.Cell(index + 4, 1).Value = summaryRows[index].Item1;
            summary.Cell(index + 4, 2).Value = summaryRows[index].Item2;
        }

        AddTitle(status, "Asset Status Breakdown");
        status.Cell("A3").Value = "Status";
        status.Cell("B3").Value = "Count";
        StyleHeader(status.Range("A3:B3"));
        AddSlices(status, report.AssetsByStatus);

        AddTitle(categories, "Assets by Category");
        categories.Cell("A3").Value = "Category";
        categories.Cell("B3").Value = "Asset Count";
        StyleHeader(categories.Range("A3:B3"));
        AddSlices(categories, report.AssetsByCategory);

        AddTitle(assetDetails, "Complete Asset Details");
        var assetHeaders = new[]
        {
            "Asset Code", "Name", "Category", "Status", "Location", "Department",
            "Assigned To", "Serial Number", "Purchase Date", "Purchase Cost", "Description"
        };
        for (var index = 0; index < assetHeaders.Length; index++)
        {
            assetDetails.Cell(3, index + 1).Value = assetHeaders[index];
        }

        StyleHeader(assetDetails.Range("A3:K3"));
        for (var index = 0; index < assets.Count; index++)
        {
            var asset = assets[index];
            var row = index + 4;
            assetDetails.Cell(row, 1).Value = asset.AssetCode;
            assetDetails.Cell(row, 2).Value = asset.AssetName;
            assetDetails.Cell(row, 3).Value = asset.Category?.Name ?? "Uncategorised";
            assetDetails.Cell(row, 4).Value = asset.Status;
            assetDetails.Cell(row, 5).Value = asset.Location ?? string.Empty;
            assetDetails.Cell(row, 6).Value = asset.Department ?? string.Empty;
            assetDetails.Cell(row, 7).Value = asset.AssignedTo ?? "Not assigned";
            assetDetails.Cell(row, 8).Value = asset.SerialNumber ?? string.Empty;
            assetDetails.Cell(row, 9).Value = asset.PurchaseDate?.ToString("dd MMM yyyy") ?? string.Empty;
            assetDetails.Cell(row, 10).Value = asset.PurchaseCost;
            assetDetails.Cell(row, 11).Value = asset.Description ?? string.Empty;
        }

        foreach (var worksheet in workbook.Worksheets)
        {
            worksheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        Response.Headers.CacheControl = "no-store, no-cache";
        Response.Headers.Pragma = "no-cache";

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"StockSphere-Report-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.xlsx");
    }

    private static void AddTitle(IXLWorksheet worksheet, string title)
    {
        worksheet.Cell("A1").Value = title;
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 16;
        worksheet.Cell("A2").Value = $"Generated: {DateTime.Now:dd MMM yyyy HH:mm}";
    }

    private static void StyleHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#0f766e");
        range.Style.Font.FontColor = XLColor.White;
    }

    private static void AddSlices(IXLWorksheet worksheet, IEnumerable<Models.ViewModels.ChartSlice> slices)
    {
        var row = 4;
        foreach (var slice in slices)
        {
            worksheet.Cell(row, 1).Value = slice.Label;
            worksheet.Cell(row, 2).Value = slice.Value;
            row++;
        }
    }
}