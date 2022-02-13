namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_missions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MissionMonthYears",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        year = c.Int(),
                        month = c.Int(),
                        user_id = c.Int(),
                        destination = c.String(),
                        cost = c.Double(),
                        mission_count = c.Int(),
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
                "dbo.MissionRequests",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        year = c.Int(),
                        month = c.Int(),
                        user_id = c.Int(),
                        reason = c.String(),
                        destination = c.String(),
                        cost = c.Double(),
                        date = c.DateTime(),
                        minutes = c.Int(),
                        status = c.Int(),
                        approved_by_super_admin = c.Int(),
                        approved_by_super_admin_at = c.DateTime(),
                        approved_by_branch_admin = c.Int(),
                        approved_by_branch_admin_at = c.DateTime(),
                        approved_by_team_leader = c.Int(),
                        approved_by_team_leader_at = c.DateTime(),
                        approved_by_technical_manager = c.Int(),
                        approved_by_technical_manager_at = c.DateTime(),
                        rejected_by = c.Int(),
                        rejected_by_at = c.DateTime(),
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
            DropForeignKey("dbo.MissionRequests", "user_id", "dbo.Users");
            DropForeignKey("dbo.MissionMonthYears", "user_id", "dbo.Users");
            DropIndex("dbo.MissionRequests", new[] { "user_id" });
            DropIndex("dbo.MissionMonthYears", new[] { "user_id" });
            DropTable("dbo.MissionRequests");
            DropTable("dbo.MissionMonthYears");
        }
    }
}
