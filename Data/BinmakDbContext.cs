using BinmakAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BinmakBackEnd.Entities;
using BinmakBackEnd.Areas.Kwenza.Entities;

namespace BinmakAPI.Data
{
    public class BinmakDbContext : IdentityDbContext<ApplicationUser>
    {

        public BinmakDbContext(DbContextOptions<BinmakDbContext> options) : base(options)
        {

        }


        public DbSet<AssetNode> AssetNodes { get; set; }
        public DbSet<AssetNodeType> AssetNodeTypes { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<AssetUser> AssetUsers { get; set; }
        public DbSet<ReferenceLookup> ReferenceLookups { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<BinmakModule> BinmakModules { get; set; }
        public DbSet<BinmakModuleAccess> BinmakModuleAccesses { get; set; }
        public DbSet<FormulaCreation> FormulaCreations { get; set; }
        public DbSet<MathematicalOperator> MathematicalOperators { get; set; }

        //Production Flow
        public DbSet<BinmakBackEnd.Areas.ProductionFlow.Entities.ProductionFlowAsset> ProductionFlowAssets { get; set; }
        public DbSet<BinmakBackEnd.Areas.ProductionFlow.Entities.Action> Actions { get; set; }
        public DbSet<BinmakBackEnd.Areas.ProductionFlow.Entities.DailyTask> DailyTasks { get; set; }
        public DbSet<BinmakBackEnd.Areas.ProductionFlow.Entities.Reading> Readings { get; set; }
        public DbSet<BinmakBackEnd.Areas.ProductionFlow.Entities.ProductionFlowAssetUser> ProductionFlowAssetUsers { get; set; }
        public DbSet<BinmakBackEnd.Areas.ProductionFlow.Entities.ClientAsset> ClientAssetNames { get; set; }
        public DbSet<BinmakBackEnd.Areas.ProductionFlow.Entities.FunctionUnit> FunctionUnits { get; set; }
        public DbSet<BinmakBackEnd.Areas.ProductionFlow.Entities.FunctionUnitChildren> FunctionUnitChildrens { get; set; }

        //New Production Flow
        public DbSet<BinmakBackEnd.Areas.Kwenza.Entities.Frequency> Frequencies { get; set; }
        public DbSet<BinmakBackEnd.Areas.Kwenza.Entities.KeyProcessArea> KeyProcessAreas { get; set; }
        public DbSet<BinmakBackEnd.Areas.Kwenza.Entities.KeyProcessAreaType> KeyProcessAreaTypes { get; set; }
        public DbSet<BinmakBackEnd.Areas.Kwenza.Entities.Process> Processes { get; set; }
        public DbSet<BinmakBackEnd.Areas.Kwenza.Entities.Target> Targets { get; set; }
        public DbSet<BinmakBackEnd.Areas.Kwenza.Entities.Production> Productions { get; set; }
        public DbSet<BinmakBackEnd.Areas.Kwenza.Entities.ColorPallete> ColorPalletes { get; set; }

    }

}

