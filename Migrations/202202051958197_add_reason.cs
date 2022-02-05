namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_reason : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkPermissionRequests", "reason", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkPermissionRequests", "reason");
        }
    }
}
