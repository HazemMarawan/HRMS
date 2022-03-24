namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateSubstation : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.UserProjects", "substation");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserProjects", "substation", c => c.String());
        }
    }
}
