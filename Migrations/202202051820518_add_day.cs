namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_day : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkPermissionRequests", "day", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkPermissionRequests", "day");
        }
    }
}
