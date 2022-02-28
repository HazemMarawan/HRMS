namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates20 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.VacationTypes", "inform_before_duration_max_range");
        }
        
        public override void Down()
        {
            AddColumn("dbo.VacationTypes", "inform_before_duration_max_range", c => c.Int());
        }
    }
}
