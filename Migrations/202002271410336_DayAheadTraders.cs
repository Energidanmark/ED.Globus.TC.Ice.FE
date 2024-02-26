namespace ED.Atlas.Svc.TC.Ice.FE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DayAheadTraders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DayAheadTraders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TraderName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DayAheadTraders");
        }
    }
}
