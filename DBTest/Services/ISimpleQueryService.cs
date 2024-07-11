namespace DBTest.Services;

using DBTest.models;
using DBTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISimpleQueryService
{
    //List<Inventory> GetAllInventories();
    List<FollowedCaseTable> GetFollowedCaseWithPage(int pageNumber, int rows);

    List<FollowedCaseTable> GetFollowedCaseByQery(FollowedCaseQuery followedCaseQuery);
}
