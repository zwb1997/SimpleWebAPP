namespace BackendAPI.Services.impl;

using Ardalis.Result;
using Data;
using Models.Tables;
using Services;
using Controller;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using AutoMapper;
using LinqKit;

public class FollowedCaseService : IFollowedCaseService
{
    private readonly AppDBContext _dbContext;

    private readonly ILogger<FollowedCaseService> _logger;

    private readonly ICaseService _caseService;

    private readonly IMapper _caseFollowMapper;

    public FollowedCaseService(
        AppDBContext dbContext,
        ILogger<FollowedCaseService> logger,
        ICaseService caseService,
        IMapper caseFollowMapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _caseService = caseService ?? throw new ArgumentNullException(nameof(caseService));
        _caseFollowMapper = caseFollowMapper ?? throw new ArgumentNullException(nameof(caseFollowMapper));
    }

    public async Task<Result<List<CaseFollowModel>>> DoFollowCaseByCaseIdAndUserName(CaseFollowDTO queryModel)
    {
        queryModel.CaseID = AppUtilities.EnsureRemoveNewLineWhenLog(queryModel.CaseID ?? string.Empty);

        if (string.IsNullOrWhiteSpace(queryModel.CaseID))
        {
            string errorMsg =
                $"Follow case ID: [{queryModel.CaseID}] failed, case id is empty.";
            _logger.LogError(errorMsg);
            return Result<List<CaseFollowModel>>.Error(errorMsg);
        }

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
        var predicate = PredicateBuilder.New<CaseFollowModel>(true);
        

        var conditions = new List<(bool condition, Expression<Func<CaseFollowModel, bool>> predicate)>
        {
            ( !string.IsNullOrEmpty(queryModel.CaseStatus), c => c.CaseStatus == queryModel.CaseStatus.Trim() ),
            ( !string.IsNullOrEmpty(queryModel.CaseID), c => c.CaseID == queryModel.CaseID.Trim() ),
            ( !string.IsNullOrEmpty(queryModel.CaseSubject), c => c.CaseSubject.Contains(queryModel.CaseSubject.Trim()) ),
            ( !string.IsNullOrEmpty(queryModel.Remark), c => c.Remark.Contains(queryModel.Remark.Trim()) ),
            ( !string.IsNullOrEmpty(queryModel.CaseSev), c => c.CaseSev == queryModel.CaseSev.Trim() ),
            ( !string.IsNullOrEmpty(queryModel.WhoFollowed), c => c.WhoFollowed == queryModel.WhoFollowed.Trim() ),
            ( queryModel.IsArchive.HasValue, c => c.IsArchive == queryModel.IsArchive.Value)
        };

        foreach (var (condition, pred) in conditions)
        {
            if (condition)
            {
                predicate = predicate.And(pred);
            }
        }
        
        var caseQuery = _dbContext.CaseFollowModel.AsQueryable().AsExpandable().Where(predicate);

        var totalRecords = await caseQuery.CountAsync();

        var cases = await caseQuery
            .OrderBy(v => v.FollowedTime)
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
        CaseFollowModel existingCaseFollowModel, CaseFollowDTO? queryModel)
    {
        if (queryModel == null)
        {
            string errorMsg =
                $"You are trying to UPDATE existed followed case: [{existingCaseFollowModel.CaseID}], followed by: [{existingCaseFollowModel.WhoFollowed}], but have not provided the update objects";
            _logger.LogError(errorMsg);
            return Result<List<CaseFollowModel>>.Error(errorMsg);
        }

        try
        {
            SetupUpdateProperties(existingCaseFollowModel, queryModel);
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

    private void SetupUpdateProperties(CaseFollowModel existingCaseFollowModel, CaseFollowDTO queryModel)
    {
        if (existingCaseFollowModel == null || queryModel == null)
        {
            throw new NullReferenceException("When do update, CaseFollowModel obj and CaseFollowDTO both cannot empty");
        }
        
        // DataId should not be modified
        var dataId = existingCaseFollowModel.DataId;
        _caseFollowMapper.Map(queryModel, existingCaseFollowModel);
        existingCaseFollowModel.DataId = dataId;
    }

    /// <summary>
    /// Do insert
    /// </summary>
    /// <param name="queryModel"></param>
    /// <returns></returns>
    private async Task<Result<List<CaseFollowModel>>> DoInsertNewFollow(CaseFollowDTO queryModel)
    {
        queryModel.CaseID = AppUtilities.EnsureRemoveNewLineWhenLog(queryModel.CaseID);

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
        queryModel.CaseID = AppUtilities.EnsureRemoveNewLineWhenLog(queryModel.CaseID ?? string.Empty);
        queryModel.WhoFollowed = AppUtilities.EnsureRemoveNewLineWhenLog(queryModel.WhoFollowed ?? string.Empty);

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

        updatedCases.ForEach(v => { v.LastSyncedTime = v.CurrentSyncedTime; });

        _logger.LogInformation("Prepare to async override the updatedCases LastSyncedTime");
        this.BatchUpdate(updatedCases);

        return result;
    }

    /// <summary>
    /// batch update cases with case list
    /// </summary>
    /// <param name="caseFollowModels">The preparation List to be updated</param>
    /// <returns>void</returns>
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
