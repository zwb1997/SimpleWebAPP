namespace BackendAPI.Services;

using Ardalis.Result;
using BackendAPI.Models.Tables;

public interface ICaseService
{
    Task<Result<List<CaseModel>>> QueryCaseByCaseId(string caseId);
}

