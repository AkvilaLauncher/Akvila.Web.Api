using System;
using AkvilaCore.Interfaces.User;

namespace Akvila.Web.Api.Domains.User;

public class UserLauncherInfo {
    public DateTimeOffset ExpiredDate { get; set; }
    public IDisposable Subscription { get; set; }
    public IUser User { get; set; }
    public dynamic Connection { get; set; }
}   