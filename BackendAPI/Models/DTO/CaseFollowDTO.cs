using System.ComponentModel;

namespace BackendAPI.Models.Tables;

using System;
using System.ComponentModel.DataAnnotations;

public class CaseFollowDTO
{
    [Description("Unique primary key identify this followed case")]
    public Guid DataId { get; set; } = Guid.NewGuid();
    
    [MaxLength(180)] 
    public string? CaseSubject { get; set; }
    
    [MaxLength(80)] public string? CaseID { get; set; }

 
    [MaxLength(8)]
    public string? CaseSev
    {
        get => _caseSev?.Trim();
        set => _caseSev = value?.Trim();
    }

    private string _caseSev;

    [MaxLength(28)] public string? CaseStatus { get; set; }

    [MaxLength(50)] public string? CurrentCaseOwner { get; set; }

    public DateTime? FollowedTime { get; set; } = DateTime.Now;

    [MaxLength(200)] public string? Remark { get; set; }

    [MaxLength(1000)] public string? Resolution { get; set; }

    public bool? IsArchive { get; set; }

    public bool? IsClosed { get; set; }

    public DateTime? LastSyncedTime { get; set; }

    public DateTime? CurrentSyncedTime { get; set; }

    public Boolean? IsSynced { get; set; }
    
    public int PageNumber { get; set; } = 1;
    public int RowsPerPage { get; set; } = 10;
    

    [MaxLength(50)] public string? WhoFollowed { get; set; }

    public override string ToString()
    {
        return "[" + $"{nameof(DataId)}: {DataId}, " +
               $"{nameof(WhoFollowed)}: {WhoFollowed}, " +
               $"{nameof(IsSynced)}: {IsSynced}, " +
               $"{nameof(CaseSubject)}: {CaseSubject}, " +
               $"{nameof(CaseID)}: {CaseID}, " +
               $"{nameof(CaseSev)}: {CaseSev}, " +
               $"{nameof(CaseStatus)}: {CaseStatus}, " +
               $"{nameof(CurrentCaseOwner)}: {CurrentCaseOwner}, " +
               $"{nameof(FollowedTime)}: {FollowedTime}, " +
               $"{nameof(Remark)}: {Remark}, " +
               $"{nameof(Resolution)}: {Resolution}, " +
               $"{nameof(IsArchive)}: {IsArchive}, " +
               $"{nameof(IsClosed)}: {IsClosed}, " +
               $"{nameof(LastSyncedTime)}: {LastSyncedTime?.ToString() ?? "NULL"}, " +
               $"{nameof(CurrentSyncedTime)}: {CurrentSyncedTime?.ToString() ?? "NULL"}" + "]";
    }
}
