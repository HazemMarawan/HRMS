namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTables4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "attendance_code", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "attendance_code");
        }
    }
}
