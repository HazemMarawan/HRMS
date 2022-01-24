namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateUsers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.IDTypes",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.Users", "vacations_balance", c => c.Int());
            CreateIndex("dbo.Users", "id_type");
            AddForeignKey("dbo.Users", "id_type", "dbo.IDTypes", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "id_type", "dbo.IDTypes");
            DropIndex("dbo.Users", new[] { "id_type" });
            DropColumn("dbo.Users", "vacations_balance");
            DropTable("dbo.IDTypes");
        }
    }
}
