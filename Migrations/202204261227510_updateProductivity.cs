namespace HRMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateProductivity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProjects", "transformer", c => c.Double());
            AddColumn("dbo.UserProjects", "pole", c => c.Double());
            AddColumn("dbo.UserProjects", "meter", c => c.Double());
            AddColumn("dbo.UserProjects", "distribution_box", c => c.Double());
            AddColumn("dbo.UserProjects", "rmu", c => c.Double());
            AddColumn("dbo.UserProjects", "switchh", c => c.Double());
            AddColumn("dbo.UserProjects", "transformer_target", c => c.Double());
            AddColumn("dbo.UserProjects", "pole_target", c => c.Double());
            AddColumn("dbo.UserProjects", "meter_target", c => c.Double());
            AddColumn("dbo.UserProjects", "distribution_box_target", c => c.Double());
            AddColumn("dbo.UserProjects", "rmu_target", c => c.Double());
            AddColumn("dbo.UserProjects", "switchh_target", c => c.Double());
            AddColumn("dbo.Targets", "transformer", c => c.Double());
            AddColumn("dbo.Targets", "pole", c => c.Double());
            AddColumn("dbo.Targets", "meter", c => c.Double());
            AddColumn("dbo.Targets", "distribution_box", c => c.Double());
            AddColumn("dbo.Targets", "rmu", c => c.Double());
            AddColumn("dbo.Targets", "switchh", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Targets", "switchh");
            DropColumn("dbo.Targets", "rmu");
            DropColumn("dbo.Targets", "distribution_box");
            DropColumn("dbo.Targets", "meter");
            DropColumn("dbo.Targets", "pole");
            DropColumn("dbo.Targets", "transformer");
            DropColumn("dbo.UserProjects", "switchh_target");
            DropColumn("dbo.UserProjects", "rmu_target");
            DropColumn("dbo.UserProjects", "distribution_box_target");
            DropColumn("dbo.UserProjects", "meter_target");
            DropColumn("dbo.UserProjects", "pole_target");
            DropColumn("dbo.UserProjects", "transformer_target");
            DropColumn("dbo.UserProjects", "switchh");
            DropColumn("dbo.UserProjects", "rmu");
            DropColumn("dbo.UserProjects", "distribution_box");
            DropColumn("dbo.UserProjects", "meter");
            DropColumn("dbo.UserProjects", "pole");
            DropColumn("dbo.UserProjects", "transformer");
        }
    }
}
