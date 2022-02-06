namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates5 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Users", "vacation_balance");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "vacation_balance", c => c.Int());
        }
    }
}
