namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateProjectType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectTypes", "name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectTypes", "name");
        }
    }
}
