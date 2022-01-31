namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTables5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "full_name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "full_name");
        }
    }
}
