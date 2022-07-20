namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateAreas : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Areas", "equipment_quantity", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Areas", "equipment_quantity");
        }
    }
}
