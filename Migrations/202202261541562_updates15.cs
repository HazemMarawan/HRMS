namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates15 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Targets",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        mvoh = c.Double(),
                        lvoh = c.Double(),
                        mvug = c.Double(),
                        lvug = c.Double(),
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
            DropTable("dbo.Targets");
        }
    }
}
