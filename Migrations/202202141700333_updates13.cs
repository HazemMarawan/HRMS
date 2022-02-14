namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates13 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Assets", "active", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Assets", "active");
        }
    }
}
