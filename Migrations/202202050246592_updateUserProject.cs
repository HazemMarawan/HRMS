namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateUserProject : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProjects", "area_id", c => c.Int());
            CreateIndex("dbo.UserProjects", "area_id");
            AddForeignKey("dbo.UserProjects", "area_id", "dbo.Areas", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserProjects", "area_id", "dbo.Areas");
            DropIndex("dbo.UserProjects", new[] { "area_id" });
            DropColumn("dbo.UserProjects", "area_id");
        }
    }
}
