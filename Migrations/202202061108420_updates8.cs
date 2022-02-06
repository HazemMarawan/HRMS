namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates8 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VacationTypes", "value", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VacationTypes", "value");
        }
    }
}
