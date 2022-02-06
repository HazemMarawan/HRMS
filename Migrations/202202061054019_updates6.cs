namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationRequests", "vacation_from", c => c.DateTime());
            AddColumn("dbo.VacationRequests", "vacation_to", c => c.DateTime());
            DropColumn("dbo.VacationRequests", "vacation_date");
        }
        
        public override void Down()
        {
            AddColumn("dbo.VacationRequests", "vacation_date", c => c.DateTime());
            DropColumn("dbo.VacationRequests", "vacation_to");
            DropColumn("dbo.VacationRequests", "vacation_from");
        }
    }
}
