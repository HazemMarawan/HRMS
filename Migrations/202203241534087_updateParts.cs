namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateParts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Parts",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        part = c.String(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.UserProjects", "part_id_fk", c => c.Int());
            CreateIndex("dbo.UserProjects", "part_id_fk");
            AddForeignKey("dbo.UserProjects", "part_id_fk", "dbo.Parts", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserProjects", "part_id_fk", "dbo.Parts");
            DropIndex("dbo.UserProjects", new[] { "part_id_fk" });
            DropColumn("dbo.UserProjects", "part_id_fk");
            DropTable("dbo.Parts");
        }
    }
}
