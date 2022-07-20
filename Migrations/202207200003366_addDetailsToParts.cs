namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDetailsToParts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Parts", "mvoh", c => c.Double());
            AddColumn("dbo.Parts", "lvoh", c => c.Double());
            AddColumn("dbo.Parts", "mvug", c => c.Double());
            AddColumn("dbo.Parts", "lvug", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Parts", "lvug");
            DropColumn("dbo.Parts", "mvug");
            DropColumn("dbo.Parts", "lvoh");
            DropColumn("dbo.Parts", "mvoh");
        }
    }
}
