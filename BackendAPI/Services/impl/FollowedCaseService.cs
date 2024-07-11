using EFCore.BulkExtensions;

namespace BackendAPI.Services.impl;

using Ardalis.Result;
using BackendAPI.Data;
using BackendAPI.Models.Tables;
using BackendAPI.Services;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

public class FollowedCaseService : IFollowedCaseService
{
    private readonly AppDBContext _dbContext;

    private readonly ILogger<FollowedCaseService> _logger;

    private readonly ICaseService _caseService;

    public FollowedCaseService(AppDBContext dbContext, ILogger<FollowedCaseService> logger, ICaseService caseService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _caseService = caseService ?? throw new ArgumentNullException(nameof(caseService));
    }

    public async Task<Result<List<CaseFollowModel>>> DoFollowCaseByCaseIdAndUserName(CaseFollowDTO queryModel)
    {
        var caseRecords = await _caseService.QueryCaseByCaseId(queryModel.CaseID);

        // If the case is invalid
        if (caseRecords.Value == null || !caseRecords.Value.Any())
        {
            string errorMsg =
                $"Checking case ID: [{queryModel.CaseID}] failed, this case is invalid and cannot be followed.";
            _logger.LogError(errorMsg);
            return Result<List<CaseFollowModel>>.Error(errorMsg);
        }

        return await InsertOrUpdateFollowedCases(queryModel);
    }

    public async Task<Result<PagedResult<List<CaseFollowModel>>>> QueryCases(CaseFollowDTO queryModel)
    {
        var caseQuery = _dbContext.CaseFollowModel.AsQueryable();

        if (!string.IsNullOrEmpty(queryModel.CaseStatus))
        {
            caseQuery = caseQuery.Where(c => c.CaseStatus == queryModel.CaseStatus);
        }

        if (!string.IsNullOrEmpty(queryModel.CaseID))
        {
            caseQuery = caseQuery.Where(c => c.CaseID == queryModel.CaseID);
        }

        if (!string.IsNullOrEmpty(queryModel.CaseSubject))
        {
            caseQuery = caseQuery.Where(c => c.CaseSubject.StartsWith(queryModel.CaseSubject));
        }

        if (!string.IsNullOrEmpty(queryModel.CaseSev))
        {
            caseQuery = caseQuery.Where(c => c.CaseSev.Equals(queryModel.CaseSev));
        }

        if (!string.IsNullOrEmpty(queryModel.WhoFollowed))
        {
            caseQuery = caseQuery.Where(c => c.WhoFollowed.Equals(queryModel.WhoFollowed));
        }

        if (queryModel.IsArchive.HasValue)
        {
            caseQuery = caseQuery.Where(c => c.IsArchive.Value == queryModel.IsArchive.Value);
        }
        
        var totalRecords = await caseQuery.CountAsync();

        var cases = await caseQuery
            .OrderBy(v=>v.FollowedTime)
            .Skip((queryModel.PageNumber - 1) * queryModel.RowsPerPage)
            .Take(queryModel.RowsPerPage)
            .ToListAsync();

        var totalPages = (long)Math.Ceiling(totalRecords / (double)queryModel.RowsPerPage);
        var pagedInfo = new PagedInfo(queryModel.PageNumber, queryModel.RowsPerPage, totalPages, totalRecords);
        var pagedResult = new PagedResult<List<CaseFollowModel>>(pagedInfo, cases);

        return Result<PagedResult<List<CaseFollowModel>>>.Success(pagedResult);
    }

    public async Task<Result<List<CaseFollowModel>>> InsertOrUpdateFollowedCases(CaseFollowDTO queryModel)
    {
        // should return only one row
        var followCaseRecords = await GetFollowCaseByCaseId(queryModel);

        if (followCaseRecords.Value != null && followCaseRecords.Value.Any())
        {
            _logger.LogInformation(
                $"###When doing InsertOrUpdate, case ID: [{queryModel.CaseID}], followed by: [{queryModel.WhoFollowed}], could be searched in table, will do update.");
            _logger.LogInformation($"###Current searched Followed Case:[{followCaseRecords.Value}]");
            return await DoUpdateCurrentFollowCaseModel(followCaseRecords.Value[0], queryModel);
        }

        _logger.LogInformation(
            $"###When doing InsertOrUpdate, case ID: [{queryModel.CaseID}], followed by: [{queryModel.WhoFollowed}], cannot be searched in table, will do insert.");
        _logger.LogInformation($"###Current prepare to be inserted Case: [{queryModel}]");
        return await DoInsertNewFollow(queryModel);
    }


    /// <summary>
    /// Do update
    /// </summary>
    /// <param name="existingCaseFollowModel"></param>
    /// <param name="queryModel"></param>
    /// <returns></returns>
    private async Task<Result<List<CaseFollowModel>>> DoUpdateCurrentFollowCaseModel(
        CaseFollowModel existingCaseFollowModel, CaseFollowDTO queryModel)
    {
        if (queryModel == null)
        {
            string errorMsg =
                $"You are trying to UPDATE existed followed case: [{queryModel.CaseID}], followed by: [{queryModel.WhoFollowed}], but no updates provided.";
            _logger.LogError(errorMsg);
            return Result<List<CaseFollowModel>>.Error(errorMsg);
        }

        try
        {
            SetUpProperties(existingCaseFollowModel, queryModel);
            _dbContext.CaseFollowModel.Update(existingCaseFollowModel);
            await _dbContext.SaveChangesAsync();
            // await _dbContext.SaveChangesAsync();
            string updateMsg =
                $"Successfully updated CaseFollowModel for DataId: {existingCaseFollowModel.DataId} CaseID: {existingCaseFollowModel.CaseID}";
            _logger.LogInformation(updateMsg);
            return Result<List<CaseFollowModel>>.Success(new List<CaseFollowModel> { existingCaseFollowModel });
        }
        catch (Exception ex)
        {
            string errorMsg =
                $"Failed to update CaseFollowModel for CaseID: {existingCaseFollowModel.CaseID}, error: {ex.Message}";
            _logger.LogError(errorMsg);
            return Result<List<CaseFollowModel>>.Error(errorMsg);
        }
    }

    private void SetUpProperties(CaseFollowModel existingCaseFollowModel, CaseFollowDTO queryModel)
    {
        if (existingCaseFollowModel == null || queryModel == null)
        {
            throw new NullReferenceException("When do update, CaseFollowModel obj and CaseFollowDTO both cannot empty");
        }

        // DataId should not be modify
        // existingCaseFollowModel.DataId;
        existingCaseFollowModel.CaseSubject = string.IsNullOrWhiteSpace(queryModel.CaseSubject?.Trim())
            ? existingCaseFollowModel.CaseSubject
            : queryModel.CaseSubject?.Trim();

        existingCaseFollowModel.CaseSev = string.IsNullOrWhiteSpace(queryModel.CaseSev?.Trim())
            ? existingCaseFollowModel.CaseSev
            : queryModel.CaseSev?.Trim();


        existingCaseFollowModel.CaseStatus = string.IsNullOrWhiteSpace(queryModel.CaseStatus?.Trim())
            ? existingCaseFollowModel.CaseStatus
            : queryModel.CaseStatus?.Trim();

        existingCaseFollowModel.CurrentCaseOwner = string.IsNullOrWhiteSpace(queryModel.CurrentCaseOwner?.Trim())
            ? existingCaseFollowModel.CurrentCaseOwner
            : queryModel.CurrentCaseOwner?.Trim();

        existingCaseFollowModel.FollowedTime = queryModel.FollowedTime != null && queryModel.FollowedTime.HasValue
            ? queryModel.FollowedTime.Value
            : existingCaseFollowModel.FollowedTime;

        existingCaseFollowModel.Remark = string.IsNullOrWhiteSpace(queryModel.Remark?.Trim())
            ? existingCaseFollowModel.Remark
            : queryModel.Remark?.Trim();

        existingCaseFollowModel.Resolution = string.IsNullOrWhiteSpace(queryModel.Resolution?.Trim())
            ? existingCaseFollowModel.Resolution
            : queryModel.Resolution?.Trim();

        existingCaseFollowModel.IsArchive = queryModel.IsArchive != null && queryModel.IsArchive.HasValue
            ? queryModel.IsArchive.Value
            : existingCaseFollowModel.IsArchive;

        existingCaseFollowModel.IsClosed = queryModel.IsClosed != null && queryModel.IsClosed.HasValue
            ? queryModel.IsClosed.Value
            : existingCaseFollowModel.IsClosed;

        existingCaseFollowModel.LastSyncedTime = queryModel.LastSyncedTime != null && queryModel.LastSyncedTime.HasValue
            ? queryModel.LastSyncedTime.Value
            : existingCaseFollowModel.LastSyncedTime;

        existingCaseFollowModel.CurrentSyncedTime =
            queryModel.CurrentSyncedTime != null && queryModel.CurrentSyncedTime.HasValue
                ? queryModel.CurrentSyncedTime.Value
                : existingCaseFollowModel.CurrentSyncedTime;

        existingCaseFollowModel.WhoFollowed = string.IsNullOrWhiteSpace(queryModel.WhoFollowed?.Trim())
            ? existingCaseFollowModel.WhoFollowed
            : queryModel.WhoFollowed?.Trim();
    }

    /// <summary>
    /// Do insert
    /// </summary>
    /// <param name="queryModel"></param>
    /// <returns></returns>
    private async Task<Result<List<CaseFollowModel>>> DoInsertNewFollow(CaseFollowDTO queryModel)
    {
        var caseModel = (await _caseService.QueryCaseByCaseId(queryModel.CaseID)).Value?.FirstOrDefault();

        if (caseModel == null)
        {
            string errorMsg = $"No case found for CaseID: [{queryModel.CaseID}]. This case may invalid.";
            _logger.LogError(errorMsg);
            return Result<List<CaseFollowModel>>.Error(errorMsg);
        }

        var result = await _dbContext.CaseFollowModel.AddAsync(new CaseFollowModel
        {
            CaseID = caseModel.CaseID,
            CaseSubject = caseModel.CaseSubject,
            CaseSev = caseModel.CaseSev,
            CaseStatus = caseModel.CaseStatus,
            CurrentCaseOwner = caseModel.CurrentCaseOwner,
            Remark = caseModel.Remark,
            Resolution = caseModel.Resolution,
            IsArchive = caseModel.IsArchive,
            IsClosed = caseModel.IsClosed,
            LastSyncedTime = DateTime.Now,
            CurrentSyncedTime = DateTime.Now,
            WhoFollowed = queryModel.WhoFollowed
        });

        await _dbContext.SaveChangesAsync();

        return Result<List<CaseFollowModel>>.Success(new List<CaseFollowModel> { result.Entity });
    }

    public async Task<Result<List<CaseFollowModel>>> GetFollowCaseByCaseId(CaseFollowDTO queryModel)
    {
        _logger.LogInformation(
            "Checking case id: [{CaseId}] whether followed by User: [{WhoFollowed}]",
            queryModel.CaseID, queryModel.WhoFollowed);

        IQueryable<CaseFollowModel> query = _dbContext.CaseFollowModel.AsQueryable();

        if (!string.IsNullOrEmpty(queryModel.CaseID))
        {
            query = query.Where(val => val.CaseID == queryModel.CaseID);
        }

        if (!string.IsNullOrEmpty(queryModel.WhoFollowed))
        {
            query = query.Where(val => val.WhoFollowed == queryModel.WhoFollowed);
        }

        _logger.LogInformation("###query by CaseId=[{CaseId}], WhoFollowed=[{WhoFollowed}]", queryModel.CaseID,
            queryModel.WhoFollowed);
        var results = await query.ToListAsync();

        if (results.IsNullOrEmpty())
        {
            _logger.LogInformation("No result found");
            return Result<List<CaseFollowModel>>.NotFound();
        }

        _logger.LogInformation("Found result Count=[{Count}]", results.Count);
        return Result<List<CaseFollowModel>>.Success(results);
    }

    public async Task<Result<CaseFollowModel>> GetSingleFollowedCaseByDataId(Guid dataId)
    {
        try
        {
            var followedCase = await _dbContext.CaseFollowModel.FirstOrDefaultAsync(v => v.DataId == dataId);

            if (followedCase == null)
            {
                string errorMsg = $"No followed case found for DataID: {dataId}";
                _logger.LogInformation(errorMsg);
                return Result<CaseFollowModel>.NotFound(errorMsg);
            }

            return Result<CaseFollowModel>.Success(followedCase);
        }
        catch (Exception ex)
        {
            string errorMsg = $"Error retrieving followed case for DataID: {dataId}, error: {ex.Message}";
            _logger.LogError(errorMsg);
            return Result<CaseFollowModel>.Error(errorMsg);
        }
    }

    /// <summary>
    /// Check any cases synced. Query by whoFollowed property
    /// </summary>
    /// <param name="whoFollowed"></param>
    /// <returns></returns>
    public async Task<Result<List<CaseFollowModel>>> CheckAnyCaseSynced(string whoFollowed)
    {
        // Get follow
        List<CaseFollowModel> userFollowedCases = QueryCases(new CaseFollowDTO
        {
            WhoFollowed = whoFollowed
        }).Result.Value.Value;

        List<CaseFollowModel> updatedCases =
            userFollowedCases.Where(curCase =>
            {
                if (curCase.CurrentSyncedTime - curCase.LastSyncedTime > TimeSpan.FromMinutes(1))
                {
                    return true;
                }
                return false;
            }).ToList();

        updatedCases.ForEach(v =>
        {
            _logger.LogInformation(
                $"Followed Case=[{v}] has been updated since the CurrentSyncedTime is greater than LastSyncedTime");
            v.IsSynced = true;
        });
        
        Result<List<CaseFollowModel>> result = Result<List<CaseFollowModel>>.Success(new List<CaseFollowModel>(updatedCases));
        
        updatedCases.ForEach(v =>
        {
            v.LastSyncedTime = v.CurrentSyncedTime;
        });

        _logger.LogInformation("Prepare to async override the updatedCases LastSyncedTime");
        this.BatchUpdate(updatedCases);
        
        return result;
    }

    /// <summary>
    /// batch update cases with case list
    /// </summary>
    /// <param name="cases"></param>
    /// <returns></returns>
    public void BatchUpdate(List<CaseFollowModel> caseFollowModels)
    {
        try
        {
            _dbContext.BulkUpdate(caseFollowModels);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Batch update failed => {string.Join(", ", caseFollowModels.Select(c => c.DataId))}");
            _logger.LogError($"Failed Exception => {ex.Message}");
            throw; // Re-throw the exception to be handled at a higher level if needed
        }
    }
    
}