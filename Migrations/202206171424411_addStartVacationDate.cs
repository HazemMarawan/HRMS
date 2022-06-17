namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStartVacationDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "start_vacation_date", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "start_vacation_date");
        }
    }
}
