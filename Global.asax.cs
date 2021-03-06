using AutoMapper;
using HRMS.Models;
using HRMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace HRMS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Mapper.CreateMap<BranchViewModel, Branch>();
            Mapper.CreateMap<DepartmentViewModel, Department>();
            Mapper.CreateMap<IDTypeViewModel, IDType>();
            Mapper.CreateMap<NationalityViewModel, Nationality>();
            Mapper.CreateMap<ProjectViewModel, Project>();
            Mapper.CreateMap<ProjectTypeViewModel, ProjectType>();
            Mapper.CreateMap<UserViewModel, User>();
            Mapper.CreateMap<BranchProjectViewModel, BranchProject>();
            Mapper.CreateMap<UserProjectViewModel, UserProject>();
            Mapper.CreateMap<JobViewModel, Job>();
            Mapper.CreateMap<AreaViewModel, Area>();
            Mapper.CreateMap<VacationTypeViewModel, VacationType>();
            Mapper.CreateMap<VacationRequestViewModel, VacationRequest>();
            Mapper.CreateMap<VacationYearViewModel, VacationYear>();
            Mapper.CreateMap<WorkPermissionRequestViewModel, WorkPermissionRequest>();
            Mapper.CreateMap<MissionMonthYearViewModel, MissionMonthYear>();
            Mapper.CreateMap<MissionRequestViewModel, MissionRequest>();
            Mapper.CreateMap<AssetViewModel, Asset>();
            Mapper.CreateMap<EmailViewModel, Email>();
            Mapper.CreateMap<TargetViewModel, Target>();
            Mapper.CreateMap<PartViewModel, Part>();
            Mapper.CreateMap<TaskViewModel, Task>();
            Mapper.CreateMap<TaskClassificationViewModel, TaskClassification>();
            Mapper.CreateMap<UserTaskViewModel, UserTask>();


        }
    }
}
