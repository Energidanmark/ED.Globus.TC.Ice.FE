namespace ED.Atlas.Svc.TC.Ice.FE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBrokerMap : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BrokerMaps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IceValue = c.String(),
                        MapValue = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BrokerMaps");
        }
    }
}
