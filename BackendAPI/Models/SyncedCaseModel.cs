namespace BackendAPI.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class SyncedCaseModel
{
    public string CaseId { get; set; }

    public string CaseName { get; set; }

    [JsonConverter(typeof(IsoDateTimeConverter),"yyyy-MM-dd HH:mm:ss")]
    public DateTime SyncedTime { get; set; }

    [JsonConverter(typeof(IsoDateTimeConverter), "yyyy-MM-dd HH:mm:ss")]
    public DateTime LastSyncedTime { get; set; }
}

