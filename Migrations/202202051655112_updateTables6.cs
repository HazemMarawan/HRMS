namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTables6 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VacationRequests",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(),
                        vacation_type_id = c.Int(),
                        status = c.Int(),
                        active = c.Int(),
                        year = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.VacationTypes", t => t.vacation_type_id)
                .Index(t => t.vacation_type_id);
            
            CreateTable(
                "dbo.VacationTypes",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        must_inform_before_duration = c.Int(),
                        inform_before_duration = c.Int(),
                        inform_before_duration_measurement = c.Int(),
                        need_approve = c.Int(),
                        closed_at_specific_time = c.Int(),
                        closed_at = c.Time(precision: 7),
                        max_days = c.Int(),
                        include_official_vacation = c.Int(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VacationRequests", "vacation_type_id", "dbo.VacationTypes");
            DropIndex("dbo.VacationRequests", new[] { "vacation_type_id" });
            DropTable("dbo.VacationTypes");
            DropTable("dbo.VacationRequests");
        }
    }
}
