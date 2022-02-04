namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAreaTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Areas",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        project_id = c.Int(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Projects", t => t.project_id)
                .Index(t => t.project_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Areas", "project_id", "dbo.Projects");
            DropIndex("dbo.Areas", new[] { "project_id" });
            DropTable("dbo.Areas");
        }
    }
}
