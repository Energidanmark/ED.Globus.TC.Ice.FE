namespace ED.Atlas.Svc.TC.Ice.FE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MarketTypeToPropertyMaps : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MarketTypeToPropertyMaps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MarketTypeName = c.String(),
                        Property = c.String(),
                        MapValue = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MarketTypeToPropertyMaps");
        }
    }
}
