using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Asset Manager")]
    public class OrganisationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public OrganisationController(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // VIEW ORGANISATION PROFILE
        public async Task<IActionResult> Index()
        {
            var organisation = await _context.Organisations
                .FirstOrDefaultAsync();

            if (organisation == null)
            {
                return RedirectToAction(nameof(Create));
            }

            ViewData["Title"] = "StockSphere Organisation";
            ViewData["PageSubtitle"] = "Organisation identity, contact details and operating information.";
            return View(organisation);
        }

        public IActionResult Add()
        {
            return RedirectToAction(nameof(Create));
        }

        public async Task<IActionResult> Details(int id)
        {
            var organisation = await _context.Organisations
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organisation == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "StockSphere Details";
            ViewData["PageSubtitle"] = "Complete organisation profile information.";
            return View("Index", organisation);
        }

        // SHOW CREATE FORM
        public IActionResult Create()
        {
            return View(new Organisation
            {
                OrganisationName = "StockSphere",
                OrganisationCode = "STKSPH",
                Address = "25 Knowledge Park",
                City = "Hyderabad",
                State = "Telangana",
                Country = "India",
                PostalCode = "500081",
                Phone = "+91 40 4000 0000",
                Email = "hello@stocksphere.com",
                Website = "https://www.stocksphere.com"
            });
        }

        // SAVE NEW ORGANISATION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Organisation organisation,
            IFormFile? logoFile)
        {
            if (!ModelState.IsValid)
            {
                return View(organisation);
            }

            // Only one organisation profile is allowed
            if (await _context.Organisations.AnyAsync())
            {
                TempData["Error"] =
                    "Organisation profile already exists.";

                return RedirectToAction(nameof(Index));
            }

            // Save logo if uploaded
            if (logoFile != null)
            {
                organisation.LogoPath =
                    await SaveLogoAsync(logoFile);
            }

            // Check validation again after logo validation
            if (!ModelState.IsValid)
            {
                return View(organisation);
            }

            organisation.UpdatedDate = DateTime.UtcNow;

            _context.Organisations.Add(organisation);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Organisation profile created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // SHOW EDIT FORM
        public async Task<IActionResult> Edit()
        {
            var organisation = await _context.Organisations
                .FirstOrDefaultAsync();

            if (organisation == null)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(organisation);
        }

        // SAVE EDITED ORGANISATION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Organisation updatedOrganisation,
            IFormFile? logoFile)
        {
            if (id != updatedOrganisation.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedOrganisation);
            }

            var organisation =
                await _context.Organisations.FindAsync(id);

            if (organisation == null)
            {
                return NotFound();
            }

            // Update organisation details
            organisation.OrganisationName =
                updatedOrganisation.OrganisationName;

            organisation.OrganisationCode =
                updatedOrganisation.OrganisationCode;

            organisation.Address =
                updatedOrganisation.Address;

            organisation.City =
                updatedOrganisation.City;

            organisation.State =
                updatedOrganisation.State;

            organisation.Country =
                updatedOrganisation.Country;

            organisation.PostalCode =
                updatedOrganisation.PostalCode;

            organisation.Phone =
                updatedOrganisation.Phone;

            organisation.Email =
                updatedOrganisation.Email;

            organisation.Website =
                updatedOrganisation.Website;

            // Replace logo if a new logo is uploaded
            if (logoFile != null)
            {
                var newLogoPath =
                    await SaveLogoAsync(logoFile);

                if (!ModelState.IsValid)
                {
                    return View(updatedOrganisation);
                }

                DeleteLogo(organisation.LogoPath);

                organisation.LogoPath = newLogoPath;
            }

            organisation.UpdatedDate =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Organisation profile updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var organisation = await _context.Organisations
        .FirstOrDefaultAsync(o => o.Id == id);

    if (organisation == null)
    {
        TempData["Error"] = "Organisation not found.";
        return RedirectToAction(nameof(Index));
    }

    // Delete the uploaded logo file if one exists
    DeleteLogo(organisation.LogoPath);

    _context.Organisations.Remove(organisation);

    await _context.SaveChangesAsync();

    TempData["Success"] =
        "Organisation deleted successfully.";

    return RedirectToAction(nameof(Index));
}

        // SAVE LOGO FILE
        private async Task<string?> SaveLogoAsync(
            IFormFile logoFile)
        {
            if (logoFile.Length == 0)
            {
                return null;
            }

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension =
                Path.GetExtension(logoFile.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "logoFile",
                    "Only JPG, JPEG, PNG and WEBP files are allowed.");

                return null;
            }

            // Maximum file size = 2 MB
            if (logoFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "logoFile",
                    "Logo size must be less than 2 MB.");

                return null;
            }

            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "organisation");

            Directory.CreateDirectory(folderPath);

            var fileName =
                Guid.NewGuid().ToString() + extension;

            var filePath =
                Path.Combine(folderPath, fileName);

            using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);

            await logoFile.CopyToAsync(stream);

            return "/uploads/organisation/" + fileName;
        }

        // DELETE OLD LOGO
        private void DeleteLogo(string? logoPath)
        {
            if (string.IsNullOrEmpty(logoPath))
            {
                return;
            }

            var relativePath =
                logoPath.TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar);

            var fullPath =
                Path.Combine(
                    _environment.WebRootPath,
                    relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}