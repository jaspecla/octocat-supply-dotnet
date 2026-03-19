using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        if (context.Suppliers.Any())
            return;

        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                SupplierId = 1,
                Name = "PurrTech Innovations",
                Description = "Leading supplier of premium smart cat technology",
                ContactPerson = "Felix Whiskerton",
                Email = "felix@purrtech.co",
                Phone = "555-0101",
                Active = true,
                Verified = true
            },
            new Supplier
            {
                SupplierId = 2,
                Name = "WhiskerWare Systems",
                Description = "Advanced feline-focused smart product supplier",
                ContactPerson = "Tabitha Pawson",
                Email = "tabitha@whiskerware.com",
                Phone = "555-0102",
                Active = true,
                Verified = false
            },
            new Supplier
            {
                SupplierId = 3,
                Name = "CatNip Creations",
                Description = "Supplier of eco-friendly cat toys and accessories",
                ContactPerson = "Nina Nibbles",
                Email = "nina@catnip.com",
                Phone = "555-0103",
                Active = false,
                Verified = false
            }
        };
        context.Suppliers.AddRange(suppliers);

        var headquarters = new List<Headquarters>
        {
            new Headquarters
            {
                HeadquartersId = 1,
                Name = "CatTech Global HQ",
                Description = "Feline tech innovations headquarters",
                Address = "123 Whisker Lane, Purrington District",
                ContactPerson = "Catherine Purrston",
                Email = "catherine@octocat.com",
                Phone = "555-0001"
            }
        };
        context.Headquarters.AddRange(headquarters);

        var branches = new List<Branch>
        {
            new Branch
            {
                BranchId = 1,
                HeadquartersId = 1,
                Name = "Meowtown Branch",
                Description = "Main downtown cat tech showroom",
                Address = "456 Purrfect Plaza",
                ContactPerson = "Chloe Whiskers",
                Email = "cwhiskers@octocat.com",
                Phone = "555-0201"
            },
            new Branch
            {
                BranchId = 2,
                HeadquartersId = 1,
                Name = "Tabby Terrace Branch",
                Description = "Western district cat tech hub",
                Address = "789 Feline Avenue",
                ContactPerson = "Tom Pouncer",
                Email = "tpouncer@octocat.com",
                Phone = "555-0202"
            }
        };
        context.Branches.AddRange(branches);

        var products = new List<Product>
        {
            new Product
            {
                ProductId = 1,
                SupplierId = 3,
                Name = "SmartFeeder One",
                Description = "This AI-powered feeder learns your cat's snack schedule based on nap cycles and mealtime habits. It detects overeating, undernapping, and auto-updates a Feline Health Repo.",
                Price = 129.99m,
                Sku = "CAT-FEED-001",
                Unit = "piece",
                ImgName = "feeder.png",
                Discount = 0.25m
            },
            new Product
            {
                ProductId = 2,
                SupplierId = 3,
                Name = "AutoClean Litter Dome",
                Description = "A self-cleaning litter box that detects patterns in your cat's... commits. Sends you a health report and Slack alert if things look off.",
                Price = 199.99m,
                Sku = "CAT-LITTER-001",
                Unit = "piece",
                ImgName = "litter-box.png",
                Discount = 0.25m
            },
            new Product
            {
                ProductId = 3,
                SupplierId = 2,
                Name = "CatFlix Entertainment Portal",
                Description = "On-demand laser shows, motion videos, and bird-watching streams - customized per cat using AI interest tracking. Think Netflix, but for felines.",
                Price = 89.99m,
                Sku = "CAT-FLIX-001",
                Unit = "piece",
                ImgName = "catflix.png",
                Discount = 0.0m
            },
            new Product
            {
                ProductId = 4,
                SupplierId = 2,
                Name = "PawTrack Smart Collar",
                Description = "GPS and activity tracker with AI-powered mood detection based on tail position, purring frequency, and movement patterns. Syncs with your phone for walk stats and zoomie alerts.",
                Price = 79.99m,
                Sku = "CAT-COLLAR-001",
                Unit = "piece",
                ImgName = "smart-collar.png",
                Discount = 0.0m
            },
            new Product
            {
                ProductId = 5,
                SupplierId = 1,
                Name = "WhiskerCam Pro",
                Description = "Monitor your cat's daily shenanigans with this 360-degree camera featuring motion alerts, treat dispensing, and live streaming. Includes night vision for midnight mischief detection.",
                Price = 149.99m,
                Sku = "CAT-CAM-001",
                Unit = "piece",
                ImgName = "chirp-cam.png",
                Discount = 0.15m
            },
            new Product
            {
                ProductId = 6,
                SupplierId = 1,
                Name = "ThermoNest Deluxe",
                Description = "Self-heating pet bed with temperature sensors that adjust to your cat's preferred warmth. Includes memory foam and a built-in purr simulator for ultimate comfort.",
                Price = 99.99m,
                Sku = "CAT-BED-001",
                Unit = "piece",
                ImgName = "sleep-nest.png",
                Discount = 0.0m
            },
            new Product
            {
                ProductId = 7,
                SupplierId = 1,
                Name = "ClimbCast Cat Tree",
                Description = "Multi-level climbing structure with integrated speakers, charging stations, and modular perches. Designed for modern cats who appreciate both function and style.",
                Price = 299.99m,
                Sku = "CAT-TREE-001",
                Unit = "piece",
                ImgName = "scratch-pad.png",
                Discount = 0.10m
            },
            new Product
            {
                ProductId = 8,
                SupplierId = 2,
                Name = "HydroFlow Smart Bowl",
                Description = "AI-powered water fountain that monitors hydration levels and sends health alerts. Features filtration system and customizable flow patterns to entice picky drinkers.",
                Price = 119.99m,
                Sku = "CAT-WATER-001",
                Unit = "piece",
                ImgName = "smart-fountain.png",
                Discount = 0.0m
            },
            new Product
            {
                ProductId = 9,
                SupplierId = 3,
                Name = "PurrFect Groomer Bot",
                Description = "Robotic grooming assistant with gentle brushes and nail trimmers. Uses AI to detect grooming preferences and stress levels for a spa-like experience.",
                Price = 399.99m,
                Sku = "CAT-GROOM-001",
                Unit = "piece",
                ImgName = "auto-groomer.png",
                Discount = 0.20m
            },
            new Product
            {
                ProductId = 10,
                SupplierId = 1,
                Name = "MemoryFoam Recovery Pod",
                Description = "Therapeutic resting pod with memory foam and heat therapy for senior cats or post-surgery recovery. Includes monitoring sensors for health tracking.",
                Price = 179.99m,
                Sku = "CAT-POD-001",
                Unit = "piece",
                ImgName = "snack-vault.png",
                Discount = 0.0m
            },
            new Product
            {
                ProductId = 11,
                SupplierId = 3,
                Name = "DoorDash Smart Portal",
                Description = "Smart cat door with facial recognition and time-based access. Prevents midnight squirrel parties and tracks in/out commits to your dashboard.",
                Price = 159.99m,
                Sku = "CAT-DOOR-001",
                Unit = "piece",
                ImgName = "door-dash.png",
                Discount = 0.0m
            },
            new Product
            {
                ProductId = 12,
                SupplierId = 2,
                Name = "ZoomieTracker AI Mat",
                Description = "A motion-sensing mat that detects zoomies, spins up chase lights, and logs agility bursts to a weekly health report. Yes, it graphs zoomies per hour.",
                Price = 79.99m,
                Sku = "CAT-TRACKER-001",
                Unit = "piece",
                ImgName = "tracker-mat.png",
                Discount = 0.0m
            }
        };
        context.Products.AddRange(products);

        context.SaveChanges();
    }
}
