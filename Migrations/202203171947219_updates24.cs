namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates24 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalaryBatches", "file_path", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SalaryBatches", "file_path");
        }
    }
}
