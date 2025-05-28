using System.Collections.Frozen;
using System.Net;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.Notifications;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Notifications;

namespace Akvila.Web.Api.Core.Handlers;

public class NotificationHandler : INotificationsHandler {
    public static Task<IResult> GetNotifications(IAkvilaManager akvilaManager) {
        FrozenSet<INotification> history = akvilaManager.Notifications.History.ToFrozenSet();

        var result = Results.Ok(ResponseMessage.Create(new NotificationReadDto {
                Notifications = history,
                Amount = history.Count
            }, "Notification list",
            HttpStatusCode.OK));

        return Task.FromResult(result);
    }

    public static Task<IResult> ClearNotification(IAkvilaManager akvilaManager) {
        akvilaManager.Notifications.Clear();

        var result = Results.Ok(ResponseMessage.Create("Notifications have been successfully cleared",
            HttpStatusCode.OK));

        return Task.FromResult(result);
    }
}