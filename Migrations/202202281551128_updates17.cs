namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates17 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationTypes", "inform_before_duration_2", c => c.Int());
            AddColumn("dbo.VacationTypes", "inform_before_duration_measurement_2", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VacationTypes", "inform_before_duration_measurement_2");
            DropColumn("dbo.VacationTypes", "inform_before_duration_2");
        }
    }
}
