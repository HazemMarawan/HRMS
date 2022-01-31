namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProjects", "productivity_type", c => c.Int());
            AddColumn("dbo.UserProjects", "productivity_work_place", c => c.Int());
            AddColumn("dbo.UserProjects", "part_id", c => c.String());
            AddColumn("dbo.UserProjects", "equipment_quantity", c => c.Double());
            AddColumn("dbo.UserProjects", "mvoh", c => c.Double());
            AddColumn("dbo.UserProjects", "lvoh", c => c.Double());
            AddColumn("dbo.UserProjects", "mvug", c => c.Double());
            AddColumn("dbo.UserProjects", "lvug", c => c.Double());
            AddColumn("dbo.UserProjects", "note", c => c.String());
            AddColumn("dbo.Users", "team_leader_id", c => c.Int());
            AddColumn("dbo.Users", "last_salary", c => c.Double());
            AddColumn("dbo.Users", "last_hour_price", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "last_hour_price");
            DropColumn("dbo.Users", "last_salary");
            DropColumn("dbo.Users", "team_leader_id");
            DropColumn("dbo.UserProjects", "note");
            DropColumn("dbo.UserProjects", "lvug");
            DropColumn("dbo.UserProjects", "mvug");
            DropColumn("dbo.UserProjects", "lvoh");
            DropColumn("dbo.UserProjects", "mvoh");
            DropColumn("dbo.UserProjects", "equipment_quantity");
            DropColumn("dbo.UserProjects", "part_id");
            DropColumn("dbo.UserProjects", "productivity_work_place");
            DropColumn("dbo.UserProjects", "productivity_type");
        }
    }
}
