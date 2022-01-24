namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class createDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Branches",
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
            
            CreateTable(
                "dbo.BranchProjects",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        branch_id = c.Int(),
                        project_id = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Branches", t => t.branch_id)
                .ForeignKey("dbo.Projects", t => t.project_id)
                .Index(t => t.branch_id)
                .Index(t => t.project_id);
            
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        project_type_id = c.Int(),
                        start_date = c.DateTime(),
                        end_date = c.DateTime(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.ProjectTypes", t => t.project_type_id)
                .Index(t => t.project_type_id);
            
            CreateTable(
                "dbo.ProjectTypes",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.UserProjects",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(),
                        project_id = c.Int(),
                        working_date = c.DateTime(),
                        no_of_numbers = c.Int(),
                        status = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Projects", t => t.project_id)
                .ForeignKey("dbo.Users", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.project_id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        code = c.String(),
                        user_name = c.String(),
                        password = c.String(),
                        first_name = c.String(),
                        middle_name = c.String(),
                        last_name = c.String(),
                        id_type = c.Int(),
                        id_number = c.String(),
                        birth_date = c.DateTime(),
                        phone = c.String(),
                        address = c.String(),
                        nationality_id = c.Int(),
                        branch_id = c.Int(),
                        department_id = c.Int(),
                        job_title = c.String(),
                        gender = c.Int(),
                        hiring_date = c.DateTime(),
                        image = c.String(),
                        notes = c.String(),
                        type = c.Int(),
                        active = c.Int(),
                        created_by = c.Int(),
                        updated_by = c.Int(),
                        deleted_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        deleted_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Branches", t => t.branch_id)
                .ForeignKey("dbo.Departments", t => t.department_id)
                .ForeignKey("dbo.Nationalities", t => t.nationality_id)
                .Index(t => t.nationality_id)
                .Index(t => t.branch_id)
                .Index(t => t.department_id);
            
            CreateTable(
                "dbo.Departments",
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
            
            CreateTable(
                "dbo.Nationalities",
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BranchProjects", "project_id", "dbo.Projects");
            DropForeignKey("dbo.UserProjects", "user_id", "dbo.Users");
            DropForeignKey("dbo.Users", "nationality_id", "dbo.Nationalities");
            DropForeignKey("dbo.Users", "department_id", "dbo.Departments");
            DropForeignKey("dbo.Users", "branch_id", "dbo.Branches");
            DropForeignKey("dbo.UserProjects", "project_id", "dbo.Projects");
            DropForeignKey("dbo.Projects", "project_type_id", "dbo.ProjectTypes");
            DropForeignKey("dbo.BranchProjects", "branch_id", "dbo.Branches");
            DropIndex("dbo.Users", new[] { "department_id" });
            DropIndex("dbo.Users", new[] { "branch_id" });
            DropIndex("dbo.Users", new[] { "nationality_id" });
            DropIndex("dbo.UserProjects", new[] { "project_id" });
            DropIndex("dbo.UserProjects", new[] { "user_id" });
            DropIndex("dbo.Projects", new[] { "project_type_id" });
            DropIndex("dbo.BranchProjects", new[] { "project_id" });
            DropIndex("dbo.BranchProjects", new[] { "branch_id" });
            DropTable("dbo.Nationalities");
            DropTable("dbo.Departments");
            DropTable("dbo.Users");
            DropTable("dbo.UserProjects");
            DropTable("dbo.ProjectTypes");
            DropTable("dbo.Projects");
            DropTable("dbo.BranchProjects");
            DropTable("dbo.Branches");
        }
    }
}
