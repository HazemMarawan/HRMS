namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateUserProject1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserProjects", "no_of_numbers", c => c.Double());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserProjects", "no_of_numbers", c => c.Int());
        }
    }
}
