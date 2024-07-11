namespace DBTest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

internal class ConfigurationHelper
{
    public static IConfigurationRoot GetConfiguration()
    {
        var build = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .SetFileLoadExceptionHandler(ex =>
            {
                throw new FileNotFoundException("appsettings.json not found!",ex.Exception);
            });

        return build.Build();
    }
}
