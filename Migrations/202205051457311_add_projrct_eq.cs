namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_projrct_eq : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "equipment_quantity", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Projects", "equipment_quantity");
        }
    }
}
