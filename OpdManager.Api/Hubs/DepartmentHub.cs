using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OpdManager.Api.Hubs;

[Authorize]
public class DepartmentHub : Hub
{
    /// <summary>
    /// Department staff join their department's group on connect.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var departmentId = Context.User?.FindFirst("DepartmentId")?.Value;
        if (!string.IsNullOrEmpty(departmentId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"department-{departmentId}");
        }

        // Doctors join a "doctors" group to receive referral completion notifications
        var doctorId = Context.User?.FindFirst("DoctorId")?.Value;
        if (!string.IsNullOrEmpty(doctorId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"doctor-{doctorId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var departmentId = Context.User?.FindFirst("DepartmentId")?.Value;
        if (!string.IsNullOrEmpty(departmentId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"department-{departmentId}");
        }

        var doctorId = Context.User?.FindFirst("DoctorId")?.Value;
        if (!string.IsNullOrEmpty(doctorId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"doctor-{doctorId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
