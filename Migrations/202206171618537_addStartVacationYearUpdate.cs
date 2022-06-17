namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStartVacationYearUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationYears", "start_year", c => c.DateTime());
            AddColumn("dbo.VacationYears", "end_year", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VacationYears", "end_year");
            DropColumn("dbo.VacationYears", "start_year");
        }
    }
}
