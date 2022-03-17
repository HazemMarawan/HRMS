namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates23 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalaryBatchDetails", "active", c => c.Int());
            AddColumn("dbo.SalaryBatches", "total", c => c.Double());
            AddColumn("dbo.SalaryBatches", "active", c => c.Int());
            AlterColumn("dbo.SalaryBatches", "month", c => c.Int());
            AlterColumn("dbo.SalaryBatches", "year", c => c.Int());
            AlterColumn("dbo.SalaryBatches", "count", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SalaryBatches", "count", c => c.Int(nullable: false));
            AlterColumn("dbo.SalaryBatches", "year", c => c.Int(nullable: false));
            AlterColumn("dbo.SalaryBatches", "month", c => c.Int(nullable: false));
            DropColumn("dbo.SalaryBatches", "active");
            DropColumn("dbo.SalaryBatches", "total");
            DropColumn("dbo.SalaryBatchDetails", "active");
        }
    }
}
