namespace BackendAPI.Services;

using Ardalis.Result;
using BackendAPI.Models;
using BackendAPI.Models.Tables;

public interface IFollowedCaseService
{

    Task<Result<CaseFollowModel>> GetSingleFollowedCaseByDataId(Guid dataId);

    Task<Result<List<CaseFollowModel>>> GetFollowCaseByCaseId(CaseFollowDTO queryModel);
    
    Task<Result<List<CaseFollowModel>>> DoFollowCaseByCaseIdAndUserName(CaseFollowDTO queryModel);

    Task<Result<PagedResult<List<CaseFollowModel>>>> QueryCases(CaseFollowDTO queryModel);
    
    Task<Result<List<CaseFollowModel>>> CheckAnyCaseSynced(string whoFollowed);

    void BatchUpdate(List<CaseFollowModel> cases);
    
}
