using DBTest.models;
using DBTest.Models;
using DBTest.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DBTest;

[TestClass]
public class TestConnectDB
{
    public TestContext TestContext { get; set; }

    private static ServiceProvider ServiceProviderInstance { get; set; }


    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        var config = ConfigurationHelper.GetConfiguration();

        var serviceController = new ServiceCollection();

        serviceController.AddSingleton<IConfiguration>(config);

        //serviceController.AddDbContext<SimpleDBContext>(options =>
        //    options.UseSqlServer(config["ConnectionStrings:DefaultConnection"])
        //);

        serviceController.AddDbContext<SimpleDBContext>(options =>
         options.UseSqlServer(config["ConnectionStrings:CaseDBConnection"])
        );

        serviceController.AddTransient<ISimpleQueryService, SimpleQueryService>();

        ServiceProviderInstance = serviceController.BuildServiceProvider() ?? throw new Exception();

        testContext.WriteLine("Read DB configuration settings done. Injection end. Ready to go on tests.");
    }


    [TestMethod]
    public void TestGetFollowedCaseWithPage()
    {
        ISimpleQueryService simpleQuery = ServiceProviderInstance.GetService<ISimpleQueryService>() ?? throw new NullReferenceException("ISimpleQueryService cannot inject a implementation.");
        List<FollowedCaseTable> result = simpleQuery.GetFollowedCaseWithPage(1, 10);
        TestContext.WriteLine("Get followed cases successful:");
        result.ForEach(v => TestContext.WriteLine(v.ToString()));
    }


    [TestMethod]
    public void TestGetFollowedCaseWithPageAndWhere()
    {
        ISimpleQueryService simpleQuery = ServiceProviderInstance.GetService<ISimpleQueryService>() ?? throw new NullReferenceException("ISimpleQueryService cannot inject a implementation.");



        List<FollowedCaseTable> result = simpleQuery.GetFollowedCaseByQery(new FollowedCaseQuery
        {
            CaseSev = "A",
            PageNumber = 1,
            RowsPerPage = 10,
        });
        TestContext.WriteLine("Get followed cases successful:");
        result.ForEach(v => TestContext.WriteLine(v.ToString()));
    }
}
