namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Areas", "ssss", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Areas", "ssss");
        }
    }
}
