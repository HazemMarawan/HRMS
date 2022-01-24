namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateJob : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Jobs",
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
            
            AddColumn("dbo.Users", "job_id", c => c.Int());
            CreateIndex("dbo.Users", "job_id");
            AddForeignKey("dbo.Users", "job_id", "dbo.Jobs", "id");
            DropColumn("dbo.Users", "job_title");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "job_title", c => c.String());
            DropForeignKey("dbo.Users", "job_id", "dbo.Jobs");
            DropIndex("dbo.Users", new[] { "job_id" });
            DropColumn("dbo.Users", "job_id");
            DropTable("dbo.Jobs");
        }
    }
}
