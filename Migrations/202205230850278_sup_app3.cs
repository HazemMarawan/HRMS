namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sup_app3 : DbMigration
    {
        public override void Up()
        {
          
            RenameColumn("dbo.WorkPermissionRequests", "approved_by_technical_manager", "approved_by_supervisor");
            RenameColumn("dbo.WorkPermissionRequests", "approved_by_technical_manager_at", "approved_by_supervisor_at");
        }
        
        public override void Down()
        {
        }
    }
}
