using System.Collections.Generic;
using Akvila.Web.Api.Domains.Sentry;

namespace Akvila.Web.Api.Dto.Sentry;

public class BaseSentryError {
    public IEnumerable<SentryBugs> Bugs { get; set; }
    public long CountUsers { get; set; }
    public long Count { get; set; }
}