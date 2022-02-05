namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_permission : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkPermissionRequests", "approved_by_super_admin", c => c.Int());
            AddColumn("dbo.WorkPermissionRequests", "approved_by_super_admin_at", c => c.DateTime());
            AddColumn("dbo.WorkPermissionRequests", "approved_by_branch_admin", c => c.Int());
            AddColumn("dbo.WorkPermissionRequests", "approved_by_branch_admin_at", c => c.DateTime());
            AddColumn("dbo.WorkPermissionRequests", "approved_by_team_leader", c => c.Int());
            AddColumn("dbo.WorkPermissionRequests", "approved_by_team_leader_at", c => c.DateTime());
            AddColumn("dbo.WorkPermissionRequests", "rejected_by", c => c.Int());
            AddColumn("dbo.WorkPermissionRequests", "rejected_by_at", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkPermissionRequests", "rejected_by_at");
            DropColumn("dbo.WorkPermissionRequests", "rejected_by");
            DropColumn("dbo.WorkPermissionRequests", "approved_by_team_leader_at");
            DropColumn("dbo.WorkPermissionRequests", "approved_by_team_leader");
            DropColumn("dbo.WorkPermissionRequests", "approved_by_branch_admin_at");
            DropColumn("dbo.WorkPermissionRequests", "approved_by_branch_admin");
            DropColumn("dbo.WorkPermissionRequests", "approved_by_super_admin_at");
            DropColumn("dbo.WorkPermissionRequests", "approved_by_super_admin");
        }
    }
}
