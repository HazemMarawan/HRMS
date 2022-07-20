namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTasks : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Parts", "equipment_quantity", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Parts", "equipment_quantity");
        }
    }
}
