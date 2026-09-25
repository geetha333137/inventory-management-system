using Microsoft.EntityFrameworkCore;
using OrganisationProfile.OrganisationProfile.Models;

namespace OrganisationProfile.OrganisationProfile.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Organisation> Organisations { get; set; }
    }
}