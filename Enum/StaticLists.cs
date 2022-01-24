using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.Enum
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
    }
}