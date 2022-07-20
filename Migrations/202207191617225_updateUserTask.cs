namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateUserTask : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserTasks", "status", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserTasks", "status");
        }
    }
}
