namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates18 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationTypes", "inform_before_duration_min_range", c => c.Int());
            AddColumn("dbo.VacationTypes", "inform_before_duration_max_range", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VacationTypes", "inform_before_duration_max_range");
            DropColumn("dbo.VacationTypes", "inform_before_duration_min_range");
        }
    }
}
