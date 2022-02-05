namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_day : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkPermissionRequests", "date", c => c.DateTime());
            DropColumn("dbo.WorkPermissionRequests", "day");
        }
        
        public override void Down()
        {
            AddColumn("dbo.WorkPermissionRequests", "day", c => c.DateTime());
            DropColumn("dbo.WorkPermissionRequests", "date");
        }
    }
}
