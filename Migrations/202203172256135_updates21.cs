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
                        salary = c.Double(),
                        insurance_deductions = c.Double(),
                        tax_deductions = c.Double(),
                        absense_days = c.Int(),
                        absense_deductions = c.Double(),
                        gm_amount = c.Double(),
                        reserved_amount = c.Double(),
                        addtional_hours = c.Int(),
                        addtional_hours_amount = c.Double(),
                        total_kilos = c.Double(),
                        total_salary = c.Double(),
                        notes = c.String(),
                        active = c.Int(),
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
                        month = c.Int(),
                        year = c.Int(),
                        count = c.Int(),
                        total = c.Double(),
                        active = c.Int(),
                        created_by = c.Int(),
                        notes = c.String(),
                        file_path = c.String(),
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
