namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_supervisor_2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MissionRequests", "approved_by_supervisor", c => c.Int());
            AddColumn("dbo.MissionRequests", "approved_by_supervisor_at", c => c.DateTime());
            AddColumn("dbo.VacationRequests", "approved_by_supervisor", c => c.Int());
            AddColumn("dbo.VacationRequests", "approved_by_supervisor_at", c => c.DateTime());
            AddColumn("dbo.WorkPermissionRequests", "approved_by_supervisor", c => c.Int());
            AddColumn("dbo.WorkPermissionRequests", "approved_by_supervisor_at", c => c.DateTime());
            DropColumn("dbo.MissionRequests", "approved_by_supervisor");
            DropColumn("dbo.MissionRequests", "approved_by_supervisor_at");
            DropColumn("dbo.VacationRequests", "approved_by_supervisor");
            DropColumn("dbo.VacationRequests", "approved_by_supervisor_at");
            DropColumn("dbo.WorkPermissionRequests", "approved_by_supervisor");
            DropColumn("dbo.WorkPermissionRequests", "approved_by_supervisor_at");
        }
        
        public override void Down()
        {
            AddColumn("dbo.WorkPermissionRequests", "approved_by_supervisor_at", c => c.DateTime());
            AddColumn("dbo.WorkPermissionRequests", "approved_by_supervisor", c => c.Int());
            AddColumn("dbo.VacationRequests", "approved_by_supervisor_at", c => c.DateTime());
            AddColumn("dbo.VacationRequests", "approved_by_supervisor", c => c.Int());
            AddColumn("dbo.MissionRequests", "approved_by_supervisor_at", c => c.DateTime());
            AddColumn("dbo.MissionRequests", "approved_by_supervisor", c => c.Int());
            DropColumn("dbo.WorkPermissionRequests", "approved_by_supervisor_at");
            DropColumn("dbo.WorkPermissionRequests", "approved_by_supervisor");
            DropColumn("dbo.VacationRequests", "approved_by_supervisor_at");
            DropColumn("dbo.VacationRequests", "approved_by_supervisor");
            DropColumn("dbo.MissionRequests", "approved_by_supervisor_at");
            DropColumn("dbo.MissionRequests", "approved_by_supervisor");
        }
    }
}
