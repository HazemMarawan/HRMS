namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateParts1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProjects", "substation", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserProjects", "substation");
        }
    }
}
