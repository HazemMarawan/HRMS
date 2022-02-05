namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_permission : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WorkPermissionMonthYears",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        year = c.Int(),
                        month = c.Int(),
                        user_id = c.Int(),
                        permission_count = c.Int(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Users", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.WorkPermissionRequests",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        year = c.Int(),
                        month = c.Int(),
                        user_id = c.Int(),
                        minutes = c.Int(),
                        status = c.Int(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Users", t => t.user_id)
                .Index(t => t.user_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WorkPermissionRequests", "user_id", "dbo.Users");
            DropForeignKey("dbo.WorkPermissionMonthYears", "user_id", "dbo.Users");
            DropIndex("dbo.WorkPermissionRequests", new[] { "user_id" });
            DropIndex("dbo.WorkPermissionMonthYears", new[] { "user_id" });
            DropTable("dbo.WorkPermissionRequests");
            DropTable("dbo.WorkPermissionMonthYears");
        }
    }
}
