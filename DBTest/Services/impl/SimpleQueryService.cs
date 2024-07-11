namespace DBTest.Services;

using DBTest.Models;
using System.Collections.Generic;
using System.Linq;

public class SimpleQueryService : ISimpleQueryService
{
    private readonly SimpleDBContext _dbContext;

    public SimpleQueryService(SimpleDBContext dBContext)
    {
        _dbContext = dBContext;
    }

    public List<FollowedCaseTable> GetFollowedCaseByQery(FollowedCaseQuery followedCaseQuery)
    {
        var caseFollows = _dbContext.CaseFollowTable.AsQueryable();

        if (!string.IsNullOrEmpty(followedCaseQuery.CaseStatus))
        {
            caseFollows = caseFollows.Where(cf => cf.CaseStatus == followedCaseQuery.CaseStatus);
        }

        if (followedCaseQuery.IsArchive.HasValue)
        {
            caseFollows = caseFollows.Where(cf => cf.IsArchive == followedCaseQuery.IsArchive.Value);
        }

        if (followedCaseQuery.FollowedTimeStart.HasValue)
        {
            caseFollows = caseFollows.Where(cf => cf.FollowedTime >= followedCaseQuery.FollowedTimeStart.Value);
        }

        if (followedCaseQuery.FollowedTimeEnd.HasValue)
        {
            caseFollows = caseFollows.Where(cf => cf.FollowedTime <= followedCaseQuery.FollowedTimeEnd.Value);
        }

        if (!string.IsNullOrEmpty(followedCaseQuery.CaseSev))
        {
            caseFollows = caseFollows.Where(cf => cf.CaseSev == followedCaseQuery.CaseSev);
        }
        caseFollows = caseFollows
        .OrderByDescending(cf => cf.FollowedTime)
            .Skip((followedCaseQuery.PageNumber - 1) * followedCaseQuery.RowsPerPage)
            .Take(followedCaseQuery.RowsPerPage);

        return caseFollows.ToList();
    }

    public List<FollowedCaseTable> GetFollowedCaseWithPage(int pageNumber, int rows)
    {
        var pagedRes = _dbContext.CaseFollowTable
            .OrderByDescending(d => d.FollowedTime)
            .Skip((pageNumber -1) * rows)
            .Take(rows)
            .ToList();

        return pagedRes;
    }
}
