namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_projrct_info : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "mvoh", c => c.Double());
            AddColumn("dbo.Projects", "lvoh", c => c.Double());
            AddColumn("dbo.Projects", "mvug", c => c.Double());
            AddColumn("dbo.Projects", "lvug", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Projects", "lvug");
            DropColumn("dbo.Projects", "mvug");
            DropColumn("dbo.Projects", "lvoh");
            DropColumn("dbo.Projects", "mvoh");
        }
    }
}
