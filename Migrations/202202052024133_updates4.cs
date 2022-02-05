namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkPermissionRequests", "reason", c => c.String());
            AddColumn("dbo.WorkPermissionRequests", "date", c => c.DateTime());
            DropColumn("dbo.WorkPermissionRequests", "day");
        }
        
        public override void Down()
        {
            AddColumn("dbo.WorkPermissionRequests", "day", c => c.DateTime());
            DropColumn("dbo.WorkPermissionRequests", "date");
            DropColumn("dbo.WorkPermissionRequests", "reason");
        }
    }
}
