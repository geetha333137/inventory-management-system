using InvertoryManagementSystem.Models.ViewModels;

namespace InvertoryManagementSystem.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardAsync();
    }
}
