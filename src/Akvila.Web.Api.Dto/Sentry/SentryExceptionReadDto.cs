using System.Collections.Generic;
using Akvila.Web.Api.Domains.Sentry;
using AkvilaCore.Interfaces.Launcher;

namespace Akvila.Web.Api.Dto.Sentry;

public class SentryExceptionReadDto {
    public string Exception { get; set; }
    public long CountUsers { get; set; }
    public long Count { get; set; }
    public IEnumerable<SentryGraphic> Graphic { get; set; }
    public IEnumerable<SentryOperationSystem> OperationSystems { get; set; }
    public IBugInfo BugInfo { get; set; }
    public string StackTrace { get; set; }
}