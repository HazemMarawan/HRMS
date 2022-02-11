namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates10 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationRequests", "approved_by_technical_manager", c => c.Int());
            AddColumn("dbo.VacationRequests", "approved_by_technical_manager_at", c => c.DateTime());
            AddColumn("dbo.WorkPermissionRequests", "approved_by_technical_manager", c => c.Int());
            AddColumn("dbo.WorkPermissionRequests", "approved_by_technical_manager_at", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkPermissionRequests", "approved_by_technical_manager_at");
            DropColumn("dbo.WorkPermissionRequests", "approved_by_technical_manager");
            DropColumn("dbo.VacationRequests", "approved_by_technical_manager_at");
            DropColumn("dbo.VacationRequests", "approved_by_technical_manager");
        }
    }
}
