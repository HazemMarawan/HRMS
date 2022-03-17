namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates21 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SalaryBatchDetails",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        salary_batch_id = c.Int(),
                        user_id = c.Int(),
                        bank_code = c.String(),
                        salary = c.Double(nullable: false),
                        insurance_deductions = c.Double(nullable: false),
                        tax_deductions = c.Double(nullable: false),
                        absense_days = c.Int(),
                        absense_deductions = c.Double(),
                        gm_amount = c.Double(),
                        reserved_amount = c.Double(),
                        addtional_hours = c.Int(),
                        addtional_hours_amount = c.Double(),
                        total_kilos = c.Double(),
                        total_salary = c.Double(),
                        notes = c.String(),
                        created_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.SalaryBatches", t => t.salary_batch_id)
                .Index(t => t.salary_batch_id);
            
            CreateTable(
                "dbo.SalaryBatches",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        month = c.Int(nullable: false),
                        year = c.Int(nullable: false),
                        count = c.Int(nullable: false),
                        created_by = c.Int(),
                        notes = c.String(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SalaryBatchDetails", "salary_batch_id", "dbo.SalaryBatches");
            DropIndex("dbo.SalaryBatchDetails", new[] { "salary_batch_id" });
            DropTable("dbo.SalaryBatches");
            DropTable("dbo.SalaryBatchDetails");
        }
    }
}
