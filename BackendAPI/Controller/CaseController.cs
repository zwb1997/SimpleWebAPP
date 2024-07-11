namespace BackendAPI.Controller;

using Ardalis.Result;
using BackendAPI.Models.Tables;
using BackendAPI.Services;
using BackendAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[Route("api/[controller]")]
[ApiController]
public class CaseController : ControllerBase
{
    private readonly ILogger<CaseController> _logger;
    private readonly ICaseService _caseService;

    public CaseController(ILogger<CaseController> logger, ICaseService caseService)
    {
        _logger = logger;
        _caseService = caseService;
    }

    [HttpGet("{caseId}")]
    public async Task<IActionResult> GetCase(string caseId)
    {
        if (string.IsNullOrWhiteSpace(caseId))
        {
            return this.BuildResponse(Result<string>.Error("Case ID is required."));
        }

        _logger.LogInformation($"Checking case: [{caseId}]");

        var caseModel = await _caseService.QueryCaseByCaseId(caseId);

        if (!caseModel.IsSuccess)
        {
            return this.BuildResponse(caseModel);
        }

        return this.BuildResponse(caseModel);
    }
}
