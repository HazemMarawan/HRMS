namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationYears", "remaining", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VacationYears", "remaining");
        }
    }
}
