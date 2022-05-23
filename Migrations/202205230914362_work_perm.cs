namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class work_perm : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkPermissionRequests", "from_time", c => c.Time(precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkPermissionRequests", "from_time");
        }
    }
}
