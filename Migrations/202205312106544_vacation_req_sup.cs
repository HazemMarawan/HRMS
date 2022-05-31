namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class vacation_req_sup : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.VacationRequests", "approved_by_technical_manager", "approved_by_supervisor");
            RenameColumn("dbo.VacationRequests", "approved_by_technical_manager_at", "approved_by_supervisor_at");
        }
        
        public override void Down()
        {
        }
    }
}
