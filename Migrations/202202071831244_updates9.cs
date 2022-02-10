namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates9 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationRequests", "days", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VacationRequests", "days");
        }
    }
}
