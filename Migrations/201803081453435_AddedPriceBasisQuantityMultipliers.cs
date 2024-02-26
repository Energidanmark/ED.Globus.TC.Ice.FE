namespace ED.Atlas.Svc.TC.Ice.FE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPriceBasisQuantityMultipliers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PriceBasisQuantityMultipliers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PriceBasis = c.String(),
                        QuantityMultiplier = c.Decimal(nullable: false, precision: 38, scale: 16),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PriceBasisQuantityMultipliers");
        }
    }
}
