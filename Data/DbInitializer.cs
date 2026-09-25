using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Organisations.Any())
            {
                context.Organisations.Add(new Organisation
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
                    Website = "https://www.stocksphere.com",
                    UpdatedBy = "System",
                    UpdatedDate = DateTime.UtcNow
                });

                context.SaveChanges();
            }

            if (!context.InventoryItems.Any())
            {
                context.InventoryItems.AddRange(
                    new InventoryItem { Name = "Dell Latitude 5440", Category = "Laptop", Quantity = 18, Price = 78500 },
                    new InventoryItem { Name = "HP LaserJet Pro M404", Category = "Printer", Quantity = 6, Price = 28500 },
                    new InventoryItem { Name = "Dell 24-inch Monitor", Category = "Monitor", Quantity = 24, Price = 14500 },
                    new InventoryItem { Name = "Ergonomic Office Chair", Category = "Furniture", Quantity = 9, Price = 12500 }
                );

                context.SaveChanges();
            }

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Laptop", Description = "Portable computers issued to staff.", IsActive = true },
                    new Category { Name = "Desktop", Description = "Workstation computers.", IsActive = true },
                    new Category { Name = "Printer", Description = "Printers and multifunction devices.", IsActive = true },
                    new Category { Name = "Monitor", Description = "External display screens.", IsActive = true },
                    new Category { Name = "Networking", Description = "Routers, switches and access points.", IsActive = true },
                    new Category { Name = "Furniture", Description = "Office desks, chairs and cabinets.", IsActive = true },
                    new Category { Name = "Mobile", Description = "Mobile phones and tablets.", IsActive = true },
                    new Category { Name = "Other", Description = "Assets that do not fit other categories.", IsActive = true });

                context.SaveChanges();
            }

            if (context.Assets.Any())
            {
                return;
            }

            var categories = context.Categories.ToList();
            if (!categories.Any())
            {
                return;
            }

            int Id(string name) =>
                categories.FirstOrDefault(c => c.Name == name)?.Id ?? categories[0].Id;

            var laptop = Id("Laptop");
            var desktop = categories.First(c => c.Name == "Desktop").Id;
            var printer = categories.First(c => c.Name == "Printer").Id;
            var monitor = categories.First(c => c.Name == "Monitor").Id;
            var networking = categories.First(c => c.Name == "Networking").Id;
            var furniture = categories.First(c => c.Name == "Furniture").Id;
            var mobile = categories.First(c => c.Name == "Mobile").Id;
            var other = categories.First(c => c.Name == "Other").Id;

            var now = DateTime.UtcNow;

            var assets = new List<Asset>
            {
                CreateAsset("AST-000101", "Dell Latitude 5440", laptop, "Available", "Hyderabad Office", "Information Technology", null, 78500, "SN-DL5440-101"),
                CreateAsset("AST-000102", "HP EliteBook 840", laptop, "Assigned", "Hyderabad Office", "Finance", "Priya Sharma", 82000, "SN-HP840-102"),
                CreateAsset("AST-000103", "Lenovo ThinkPad T14", laptop, "Assigned", "Bengaluru Office", "Information Technology", "Rahul Verma", 91000, "SN-LN-T14-103"),
                CreateAsset("AST-000104", "Apple MacBook Air M2", laptop, "Under Maintenance", "Hyderabad Office", "Design", "Ananya Iyer", 115000, "SN-MBA-104"),
                CreateAsset("AST-000105", "Dell XPS 15", laptop, "Available", "Pune Office", "Engineering", null, 132000, "SN-XPS-105"),
                CreateAsset("AST-000201", "Dell OptiPlex 7010", desktop, "Assigned", "Hyderabad Office", "Human Resources", "Kiran Rao", 54000, "SN-OPT-201"),
                CreateAsset("AST-000202", "HP ProDesk 400", desktop, "Available", "Chennai Office", "Operations", null, 48000, "SN-PD-202"),
                CreateAsset("AST-000203", "Lenovo ThinkCentre M70", desktop, "Retired", "Hyderabad Office", "Finance", null, 39000, "SN-TC-203"),
                CreateAsset("AST-000301", "HP LaserJet Pro M404", printer, "Assigned", "Hyderabad Office", "Administration", "Front Desk", 28500, "SN-LJ-301"),
                CreateAsset("AST-000302", "Canon Pixma G3010", printer, "Available", "Bengaluru Office", "Marketing", null, 16400, "SN-CN-302"),
                CreateAsset("AST-000303", "Epson EcoTank L3250", printer, "Damaged", "Pune Office", "Operations", null, 15200, "SN-EP-303"),
                CreateAsset("AST-000401", "Dell 24-inch Monitor", monitor, "Assigned", "Hyderabad Office", "Information Technology", "Rahul Verma", 14500, "SN-MN-401"),
                CreateAsset("AST-000402", "LG UltraWide 34-inch", monitor, "Available", "Bengaluru Office", "Design", null, 38900, "SN-LG-402"),
                CreateAsset("AST-000403", "Samsung 27-inch LED", monitor, "Assigned", "Chennai Office", "Engineering", "Meera Nair", 18200, "SN-SM-403"),
                CreateAsset("AST-000501", "Cisco Catalyst Switch", networking, "Assigned", "Hyderabad Office", "Information Technology", "Network Team", 62000, "SN-CS-501"),
                CreateAsset("AST-000502", "TP-Link Access Point", networking, "Available", "Pune Office", "Information Technology", null, 8900, "SN-AP-502"),
                CreateAsset("AST-000503", "Ubiquiti UniFi Router", networking, "Under Maintenance", "Bengaluru Office", "Information Technology", null, 21400, "SN-UB-503"),
                CreateAsset("AST-000601", "Ergonomic Office Chair", furniture, "Assigned", "Hyderabad Office", "Human Resources", "Kiran Rao", 12500, "CHR-601"),
                CreateAsset("AST-000602", "Standing Desk", furniture, "Available", "Bengaluru Office", "Engineering", null, 18600, "DSK-602"),
                CreateAsset("AST-000603", "Filing Cabinet", furniture, "Assigned", "Chennai Office", "Administration", "Records Room", 7400, "CAB-603"),
                CreateAsset("AST-000701", "Samsung Galaxy S24", mobile, "Assigned", "Hyderabad Office", "Sales", "Vikram Patel", 72000, "IMEI-701"),
                CreateAsset("AST-000702", "iPhone 15", mobile, "Assigned", "Bengaluru Office", "Marketing", "Sneha Kulkarni", 79900, "IMEI-702"),
                CreateAsset("AST-000703", "iPad 10th Gen", mobile, "Available", "Pune Office", "Design", null, 42900, "SN-IP-703"),
                CreateAsset("AST-000801", "Epson Projector EB-X51", other, "Available", "Hyderabad Office", "Training", null, 38500, "SN-PJ-801"),
                CreateAsset("AST-000802", "Microsoft Office License", other, "Assigned", "Hyderabad Office", "Information Technology", "Software Pool", 12500, "LIC-802"),
                CreateAsset("AST-000803", "Conference Webcam", other, "Lost", "Bengaluru Office", "Operations", null, 6400, "SN-CAM-803")
            };

            foreach (var asset in assets)
            {
                asset.CreatedDate = now.AddDays(-Random.Shared.Next(5, 90));
                asset.CreatedByName = "System";
            }

            context.Assets.AddRange(assets);
            context.SaveChanges();

            context.AuditLogs.AddRange(
                assets.Take(8).Select(a => new AuditLog
                {
                    Action = "Created",
                    EntityName = "Asset",
                    EntityId = a.Id,
                    PerformedBy = "System",
                    Description = $"Asset {a.AssetName} – {a.AssetCode} has been added to the inventory.",
                    CreatedDate = a.CreatedDate
                }));

            context.SaveChanges();

            static Asset CreateAsset(
                string code,
                string name,
                int categoryId,
                string status,
                string location,
                string department,
                string? assignedTo,
                decimal cost,
                string serial)
            {
                return new Asset
                {
                    AssetCode = code,
                    AssetName = name,
                    CategoryId = categoryId,
                    Status = status,
                    Location = location,
                    Department = department,
                    AssignedTo = assignedTo,
                    PurchaseCost = cost,
                    PurchaseDate = DateTime.UtcNow.Date.AddDays(-Random.Shared.Next(30, 400)),
                    SerialNumber = serial,
                    Description = $"{name} registered in the organisation inventory."
                };
            }
        }
    }
}
