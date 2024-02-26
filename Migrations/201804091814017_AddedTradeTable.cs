namespace ED.Atlas.Svc.TC.Ice.FE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTradeTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Trades",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContractId = c.String(),
                        FixMessage = c.String(),
                        AcquiredDateTimeUtc = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Trades");
        }
    }
}
