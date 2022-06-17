namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateVacationRequestsYearID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationRequests", "vacation_year_id", c => c.Int());
            CreateIndex("dbo.VacationRequests", "vacation_year_id");
            AddForeignKey("dbo.VacationRequests", "vacation_year_id", "dbo.VacationYears", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VacationRequests", "vacation_year_id", "dbo.VacationYears");
            DropIndex("dbo.VacationRequests", new[] { "vacation_year_id" });
            DropColumn("dbo.VacationRequests", "vacation_year_id");
        }
    }
}
