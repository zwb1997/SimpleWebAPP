namespace BackendAPI.Services.impl;

using Ardalis.Result;
using BackendAPI.Data;
using BackendAPI.Models;
using BackendAPI.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class CaseService : ICaseService
{
    private readonly AppDBContext _dbContext;
    private readonly ILogger<CaseService> _logger;

    public CaseService(AppDBContext dbContext, ILogger<CaseService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<List<CaseModel>>> QueryCaseByCaseId(string caseId)
    {
        try
        {
            var cases = await _dbContext.CaseModels
                .AsNoTracking()
                .Where(v => v.CaseID == caseId)
                .ToListAsync();

            if (cases == null || cases.Count == 0)
            {
                _logger.LogInformation($"No results found for Case ID: [{caseId}]");
                return Result<List<CaseModel>>.NotFound($"No results found for Case ID: [{caseId}]");
            }

            return Result<List<CaseModel>>.Success(cases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error querying case by ID: {caseId}");
            return Result<List<CaseModel>>.Error(ex.Message);
        }
    }
}
