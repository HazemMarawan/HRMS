namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VacationYears",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        year = c.Int(),
                        user_id = c.Int(),
                        vacation_balance = c.Int(),
                        a3tyady_vacation_counter = c.Int(),
                        arda_vacation_counter = c.Int(),
                        medical_vacation_counter = c.Int(),
                        married_vacation_counter = c.Int(),
                        work_from_home_vacation_counter = c.Int(),
                        death_vacation_counter = c.Int(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.Users", "vacation_balance", c => c.Int());
            AddColumn("dbo.WorkPermissionRequests", "day", c => c.DateTime());
            DropColumn("dbo.WorkPermissionRequests", "reason");
            DropColumn("dbo.WorkPermissionRequests", "date");
        }
        
        public override void Down()
        {
            AddColumn("dbo.WorkPermissionRequests", "date", c => c.DateTime());
            AddColumn("dbo.WorkPermissionRequests", "reason", c => c.String());
            DropColumn("dbo.WorkPermissionRequests", "day");
            DropColumn("dbo.Users", "vacation_balance");
            DropTable("dbo.VacationYears");
        }
    }
}
