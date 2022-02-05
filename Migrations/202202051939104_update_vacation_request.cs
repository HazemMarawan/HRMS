namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_vacation_request : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationRequests", "approved_by_super_admin", c => c.Int());
            AddColumn("dbo.VacationRequests", "approved_by_super_admin_at", c => c.DateTime());
            AddColumn("dbo.VacationRequests", "approved_by_branch_admin", c => c.Int());
            AddColumn("dbo.VacationRequests", "approved_by_branch_admin_at", c => c.DateTime());
            AddColumn("dbo.VacationRequests", "approved_by_team_leader", c => c.Int());
            AddColumn("dbo.VacationRequests", "approved_by_team_leader_at", c => c.DateTime());
            AddColumn("dbo.VacationRequests", "rejected_by", c => c.Int());
            AddColumn("dbo.VacationRequests", "rejected_by_at", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VacationRequests", "rejected_by_at");
            DropColumn("dbo.VacationRequests", "rejected_by");
            DropColumn("dbo.VacationRequests", "approved_by_team_leader_at");
            DropColumn("dbo.VacationRequests", "approved_by_team_leader");
            DropColumn("dbo.VacationRequests", "approved_by_branch_admin_at");
            DropColumn("dbo.VacationRequests", "approved_by_branch_admin");
            DropColumn("dbo.VacationRequests", "approved_by_super_admin_at");
            DropColumn("dbo.VacationRequests", "approved_by_super_admin");
        }
    }
}
