namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateParts2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Parts", "area_id", c => c.Int());
            CreateIndex("dbo.Parts", "area_id");
            AddForeignKey("dbo.Parts", "area_id", "dbo.Areas", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Parts", "area_id", "dbo.Areas");
            DropIndex("dbo.Parts", new[] { "area_id" });
            DropColumn("dbo.Parts", "area_id");
        }
    }
}
