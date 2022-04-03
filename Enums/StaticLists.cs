using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.Enums
{
    public enum RowStatus
    {
        ACTIVE = 1,
        INACTIVE = 0,
    }

    public enum UserRole
    {
        SuperAdmin = 1,
        BranchAdmin = 2,
        Employee = 3,
        TeamLeader = 4,
        TechnicalManager = 5,
    }

    public enum ProductivityType
    {
        Normal = 1,
        OverTime = 2,
    }

    public enum ProductivityWorkPlace
    {
        FromHome = 1,
        Office = 2,
    }

    public enum ProductivityStatus
    {
        PendingApprove = 1,
        Approved = 2,
        Rejected = 3,
        Returned = 4,
    }

    public enum ApprovementStatus
    {
        PendingApprove = 1,
        ApprovedByTeamLeader = 2,
        ApprovedByBranchAdmin = 3,
        ApprovedBySuperAdmin = 4,
        Rejected = 5,
        ApprovedByTechnicalManager = 6,
    }
}