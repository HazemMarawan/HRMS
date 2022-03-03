namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_productivity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProjects", "returned_by_note", c => c.String());
            AddColumn("dbo.UserProjects", "rejected_by_note", c => c.String());
            AddColumn("dbo.UserProjects", "accepted_by_note", c => c.String());
            AddColumn("dbo.UserProjects", "returned_by", c => c.Int());
            AddColumn("dbo.UserProjects", "returned_at", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserProjects", "returned_at");
            DropColumn("dbo.UserProjects", "returned_by");
            DropColumn("dbo.UserProjects", "accepted_by_note");
            DropColumn("dbo.UserProjects", "rejected_by_note");
            DropColumn("dbo.UserProjects", "returned_by_note");
        }
    }
}
