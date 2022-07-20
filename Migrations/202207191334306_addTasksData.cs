namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTasksData : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaskClassifications",
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
            
            CreateTable(
                "dbo.UserTasks",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        task_classification_id = c.Int(),
                        user_id = c.Int(),
                        is_favourite_by_owner = c.Int(),
                        is_favourite_by_assignee = c.Int(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.TaskClassifications", t => t.task_classification_id)
                .Index(t => t.task_classification_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserTasks", "task_classification_id", "dbo.TaskClassifications");
            DropIndex("dbo.UserTasks", new[] { "task_classification_id" });
            DropTable("dbo.UserTasks");
            DropTable("dbo.TaskClassifications");
        }
    }
}
