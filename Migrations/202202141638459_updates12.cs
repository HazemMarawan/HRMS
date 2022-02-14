namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates12 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Assets",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        code = c.String(),
                        name = c.String(),
                        notes = c.String(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Users", t => t.created_by)
                .Index(t => t.created_by);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Assets", "created_by", "dbo.Users");
            DropIndex("dbo.Assets", new[] { "created_by" });
            DropTable("dbo.Assets");
        }
    }
}
