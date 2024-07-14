namespace BackendAPI.Models.Mapper;

using AutoMapper;
using BackendAPI.Models.Tables;

/// <summary>
/// AutoMapper help for cutting dup codes validation
/// </summary>
public class CaseFollowMappingProfile: Profile
{

    public CaseFollowMappingProfile()
    {
        CreateMap<CaseFollowDTO, CaseFollowModel>()
            .ForMember(dest => dest.CaseSubject, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.CaseSubject?.Trim())))
            .ForMember(dest => dest.CaseSev, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.CaseSev?.Trim())))
            .ForMember(dest => dest.CaseStatus, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.CaseStatus?.Trim())))
            .ForMember(dest => dest.CurrentCaseOwner, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.CurrentCaseOwner?.Trim())))
            .ForMember(dest => dest.FollowedTime, opt => opt.Condition(src => src.FollowedTime.HasValue))
            .ForMember(dest => dest.Remark, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Remark?.Trim())))
            .ForMember(dest => dest.Resolution, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Resolution?.Trim())))
            .ForMember(dest => dest.IsArchive, opt => opt.Condition(src => src.IsArchive.HasValue))
            .ForMember(dest => dest.IsClosed, opt => opt.Condition(src => src.IsClosed.HasValue))
            .ForMember(dest => dest.LastSyncedTime, opt => opt.Condition(src => src.LastSyncedTime.HasValue))
            .ForMember(dest => dest.CurrentSyncedTime, opt => opt.Condition(src => src.CurrentSyncedTime.HasValue))
            .ForMember(dest => dest.WhoFollowed, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.WhoFollowed?.Trim())));
    }
}
