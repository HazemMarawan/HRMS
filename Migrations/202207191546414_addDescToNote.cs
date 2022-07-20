namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDescToNote : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserTasks", "description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserTasks", "description");
        }
    }
}
