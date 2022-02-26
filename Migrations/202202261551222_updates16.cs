namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates16 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProjects", "mvoh_target", c => c.Double());
            AddColumn("dbo.UserProjects", "lvoh_target", c => c.Double());
            AddColumn("dbo.UserProjects", "mvug_target", c => c.Double());
            AddColumn("dbo.UserProjects", "lvug_target", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserProjects", "lvug_target");
            DropColumn("dbo.UserProjects", "mvug_target");
            DropColumn("dbo.UserProjects", "lvoh_target");
            DropColumn("dbo.UserProjects", "mvoh_target");
        }
    }
}
