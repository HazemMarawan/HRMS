using System;
using System.Data.Entity;
using System.Linq;

namespace HRMS.Models
{
    public class HRMSDBContext : DbContext
    {
        // Your context has been configured to use a 'HRMSDBContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'HRMS.Models.HRMSDBContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'HRMSDBContext' 
        // connection string in the application configuration file.
        public HRMSDBContext()
            : base("name=HRMSDBContext")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

         public virtual DbSet<Branch> Branches { get; set; }
         public virtual DbSet<BranchProject> BranchProjects { get; set; }
         public virtual DbSet<Department> Departments { get; set; }
         public virtual DbSet<UserProject> UserProjects { get; set; }
         public virtual DbSet<Nationality> Nationalities { get; set; }
         public virtual DbSet<Project> Projects { get; set; }
         public virtual DbSet<ProjectType> ProjectTypes { get; set; }
         public virtual DbSet<User> Users { get; set; }
         public virtual DbSet<IDType> IDTypes { get; set; }
         public virtual DbSet<Job> Jobs { get; set; }
         public virtual DbSet<Area> Areas { get; set; }
         public virtual DbSet<VacationType> VacationTypes { get; set; }
         public virtual DbSet<VacationRequest> VacationRequests { get; set; }
         public virtual DbSet<VacationYear> VacationYears { get; set; }
         public virtual DbSet<WorkPermissionRequest> WorkPermissionRequests { get; set; }
         public virtual DbSet<WorkPermissionMonthYear> WorkPermissionMonthYears { get; set; }
         public virtual DbSet<MissionRequest> MissionRequests { get; set; }
         public virtual DbSet<MissionMonthYear> MissionMonthYears { get; set; }
         public virtual DbSet<Asset> Assets { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}