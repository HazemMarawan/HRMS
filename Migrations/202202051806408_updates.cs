namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationRequests", "vacation_date", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VacationRequests", "vacation_date");
        }
    }
}
