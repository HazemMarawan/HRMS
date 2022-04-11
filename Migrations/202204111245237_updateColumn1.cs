namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateColumn1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Areas", "ssss");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Areas", "ssss", c => c.Double());
        }
    }
}
