namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_area : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.Areas", "mvoh", c => c.Double());
            AddColumn("dbo.Areas", "lvoh", c => c.Double());
            AddColumn("dbo.Areas", "mvug", c => c.Double());
            AddColumn("dbo.Areas", "lvug", c => c.Double());
            AddColumn("dbo.UserProjects", "task_id", c => c.Int());
            CreateIndex("dbo.UserProjects", "task_id");
            AddForeignKey("dbo.UserProjects", "task_id", "dbo.Tasks", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserProjects", "task_id", "dbo.Tasks");
            DropIndex("dbo.UserProjects", new[] { "task_id" });
            DropColumn("dbo.UserProjects", "task_id");
            DropColumn("dbo.Areas", "lvug");
            DropColumn("dbo.Areas", "mvug");
            DropColumn("dbo.Areas", "lvoh");
            DropColumn("dbo.Areas", "mvoh");
            DropTable("dbo.Tasks");
        }
    }
}
