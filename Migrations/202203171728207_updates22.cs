namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates22 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SalaryBatchDetails", "salary", c => c.Double());
            AlterColumn("dbo.SalaryBatchDetails", "insurance_deductions", c => c.Double());
            AlterColumn("dbo.SalaryBatchDetails", "tax_deductions", c => c.Double());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SalaryBatchDetails", "tax_deductions", c => c.Double(nullable: false));
            AlterColumn("dbo.SalaryBatchDetails", "insurance_deductions", c => c.Double(nullable: false));
            AlterColumn("dbo.SalaryBatchDetails", "salary", c => c.Double(nullable: false));
        }
    }
}
