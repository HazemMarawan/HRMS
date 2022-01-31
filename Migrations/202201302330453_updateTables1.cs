namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTables1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProjects", "approved_by", c => c.Int());
            AddColumn("dbo.UserProjects", "approved_at", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserProjects", "approved_at");
            DropColumn("dbo.UserProjects", "approved_by");
        }
    }
}
