namespace BlazorServer.CrudSqlServer.Models
{
    public enum LeaveType
    {
        Annual,
        Sick,
        Maternity,
        Paternity,
        Unpaid,
        Study,
        FamilyResponsibility,
        Compassionate
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        HalfDay,
        OnLeave,
        WorkFromHome,
        Sick
    }

    public enum PayrollStatus
    {
        Draft,
        Processed,
        Paid,
        Cancelled
    }
}
