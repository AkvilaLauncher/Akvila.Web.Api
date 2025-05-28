using System.Threading.Tasks;

namespace Akvila.Web.Api.Dto.Sentry;

public class SentryOperationSystem {
    public long Count { get; set; }
    public string OsType { get; set; }
}