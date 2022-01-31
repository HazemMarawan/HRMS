namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTables2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProjects", "rejected_by", c => c.Int());
            AddColumn("dbo.UserProjects", "rejected_at", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserProjects", "rejected_at");
            DropColumn("dbo.UserProjects", "rejected_by");
        }
    }
}
