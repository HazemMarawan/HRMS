namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates11 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "required_productivity", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "required_productivity");
        }
    }
}
