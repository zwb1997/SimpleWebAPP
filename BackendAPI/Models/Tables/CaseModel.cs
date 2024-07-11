namespace BackendAPI.Models.Tables;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("CaseTable")]
public class CaseModel
{
    [Key]
    public Guid DataId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(180)]
    public string CaseSubject { get; set; }

    [Required]
    [MaxLength(80)]
    public string CaseID { get; set; }

    [Required]
    [MaxLength(8)]
    public string CaseSev
    {
        get => _caseSev.Trim();
        set => _caseSev = value?.Trim();
    }

    private string _caseSev;

    [Required]
    [MaxLength(28)]
    public string CaseStatus { get; set; }

    [Required]
    [MaxLength(22)]
    public string CurrentCaseOwner { get; set; }

    [Required]
    public DateTime FollowedTime { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(200)]
    public string Remark { get; set; }

    [Required]
    [MaxLength(500)]
    public string Resolution { get; set; }

    [Required]
    public bool IsArchive { get; set; } = false;

    [Required]
    public bool IsClosed { get; set; } = false;

    public DateTime? LastSyncedTime { get; set; }

    public DateTime? CurrentSyncedTime { get; set; }

    public override string ToString()
    {
        return "[" + $"{nameof(DataId)}: {DataId}, " +
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

