namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates14 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmailAttachments",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        attachmentPath = c.String(),
                        email_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Emails", t => t.email_id)
                .Index(t => t.email_id);
            
            CreateTable(
                "dbo.Emails",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        subject = c.String(),
                        body = c.String(),
                        from_user = c.Int(),
                        related_id = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.EmailUsers",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        email_id = c.Int(),
                        user_id = c.Int(),
                        is_recieved = c.Int(nullable: false),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmailAttachments", "email_id", "dbo.Emails");
            DropIndex("dbo.EmailAttachments", new[] { "email_id" });
            DropTable("dbo.EmailUsers");
            DropTable("dbo.Emails");
            DropTable("dbo.EmailAttachments");
        }
    }
}
