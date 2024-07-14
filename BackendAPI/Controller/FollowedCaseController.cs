namespace BackendAPI.Controller;

using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using BackendAPI.Models;
using BackendAPI.Models.DTO;
using BackendAPI.Models.Tables;
using BackendAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


[Route("api/[controller]")]
[ApiController]
public class FollowedCaseController : ControllerBase
{
    private readonly ILogger<FollowedCaseController> _logger;

    private readonly IFollowedCaseService _followedCaseService;

    public FollowedCaseController(ILogger<FollowedCaseController> logger, IFollowedCaseService followedCaseService)
    {
        _logger = logger;
        _followedCaseService = followedCaseService;
    }

    [HttpGet("Health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check.");
        return this.BuildResponse(Result<string>.Success("ok"));
    }

    [HttpGet("FollowCase/{caseId}/{whoFollow}")]
    public async Task<IActionResult> FollowCaseByCaseIDAndName([Required] string caseId, [Required] string whoFollow)
    {
        if (string.IsNullOrEmpty(caseId))
        {
            var errorResult = Result<string>.Error("caseId is required.");

            _logger.LogError("caseId is required.");
            return this.BuildResponse(errorResult);
        }

        if (string.IsNullOrEmpty(whoFollow))
        {
            var errorResult = Result<string>.Error("whoFollow is required.");
            _logger.LogError("whoFollow is required.");
            return this.BuildResponse(errorResult);
        }

        caseId = this.EnsureRemoveNewLineWhenLog(caseId);
        whoFollow = this.EnsureRemoveNewLineWhenLog(whoFollow);

        List<CaseFollowModel> checkList = _followedCaseService.QueryCases(new CaseFollowDTO
        {
            CaseID = caseId,
            WhoFollowed = whoFollow
        }).Result.Value;

        if (checkList.Any())
        {
            return this.BuildResponse(Result<List<CaseFollowModel>>.Error($"Follower: {whoFollow} already followed case: {caseId} at {checkList.First<CaseFollowModel>().FollowedTime}"));
        }

        var queryModel = new CaseFollowDTO()
        {
            CaseID = caseId,
            WhoFollowed = whoFollow
        };

        var results = await _followedCaseService.DoFollowCaseByCaseIdAndUserName(queryModel);

        return this.BuildResponse(results);
    }


    [HttpPost("FollowCase")]
    public async Task<IActionResult> UpdateFollowCaseByCaseIDAndName(
        [FromBody] CaseFollowDTO caseFollowModel)
    {
        if (string.IsNullOrEmpty(caseFollowModel.CaseID) ||
            string.IsNullOrEmpty(caseFollowModel.WhoFollowed))
        {
            var errorResult = Result<string>.Error("CaseID and WhoFollowed are required.");
            _logger.LogError("CaseID and WhoFollowed are required.");
            return this.BuildResponse(errorResult);
        }

        caseFollowModel.CaseID = this.EnsureRemoveNewLineWhenLog(caseFollowModel.CaseID);
        caseFollowModel.WhoFollowed = this.EnsureRemoveNewLineWhenLog(caseFollowModel.WhoFollowed);
        Guid dataID;
        Guid.TryParse(this.EnsureRemoveNewLineWhenLog(caseFollowModel.DataId.ToString()), out dataID);

        if (dataID == Guid.Empty)
        {
            string errorMsg = $"dataID: [{dataID}] is not a valid GUID format";
            var errorResult = Result<string>.Error(errorMsg);
            _logger.LogError(errorMsg);
            return this.BuildResponse(errorResult);
        }

        var results = await _followedCaseService.DoFollowCaseByCaseIdAndUserName(caseFollowModel);

        return this.BuildResponse(results);
    }

    [HttpGet("FollowedCase/{dataId}")]
    public async Task<IActionResult> GetSingleFollowedCaseByDataId(string dataId)
    {
        dataId = this.EnsureRemoveNewLineWhenLog(dataId);

        if (string.IsNullOrEmpty(dataId))
        {
            var errorResult = Result<string>.Error("DataID is required.");
            _logger.LogError("DataID is required.");
            return this.BuildResponse(errorResult);
        }

        Guid dataGUID = Guid.Empty;
        Guid.TryParse(dataId.Trim(), out dataGUID);

        if (dataGUID == Guid.Empty)
        {
            string errorMsg = $"dataID: [{dataId}] is not a valid GUID format";
            var errorResult = Result<string>.Error(errorMsg);
            _logger.LogError(errorMsg);
            return this.BuildResponse(errorResult);
        }

        var result = await _followedCaseService.GetSingleFollowedCaseByDataId(dataGUID);

        return this.BuildResponse(result);
    }

    [HttpGet("SearchCases")]
    public async Task<IActionResult> SearchCases([FromQuery] CaseFollowDTO queryModelDTO)
    {

        var results = await _followedCaseService.QueryCases(queryModelDTO);

        if (!results.IsSuccess)
        {
            return this.BuildResponse(results);
        }

        // Map to custom DTO
        var pagedResultDto = new PagedResultDTO<CaseFollowModel>
        {
            PagedInfo = new PagedInfoDTO
            {
                PageNumber = results.Value.PagedInfo.PageNumber,
                PageSize = results.Value.PagedInfo.PageSize,
                TotalPages = results.Value.PagedInfo.TotalPages,
                TotalRecords = results.Value.PagedInfo.TotalRecords
            },
            Data = results.Value.Value.ToList()
        };
        return this.BuildResponse(Result<PagedResultDTO<CaseFollowModel>>.Success(pagedResultDto));
    }

    [HttpGet("SyncedCase/{whoFollowed}")]
    public async Task<IActionResult> IsSynced([Required] string whoFollowed)
    {
        whoFollowed = this.EnsureRemoveNewLineWhenLog(whoFollowed);

        _logger.LogInformation($"Fetching cases followed by: {whoFollowed}");
        var result = await _followedCaseService.CheckAnyCaseSynced(whoFollowed);
        return this.BuildResponse(result);
    }
}