namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTables3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProjects", "cost", c => c.Double());
            AddColumn("dbo.Users", "last_over_time_price", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "last_over_time_price");
            DropColumn("dbo.UserProjects", "cost");
        }
    }
}
