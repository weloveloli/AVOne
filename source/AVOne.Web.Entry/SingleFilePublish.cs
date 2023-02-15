using Furion;
using System.Reflection;

namespace AVOne.Web.Entry;

public class SingleFilePublish : ISingleFilePublish
{
    public Assembly[] IncludeAssemblies()
    {
        return Array.Empty<Assembly>();
    }

    public string[] IncludeAssemblyNames()
    {
        return new[]
        {
            "AVOne.Application",
            "AVOne.Core",
            "AVOne.EntityFramework.Core",
            "AVOne.Web.Core"
        };
    }
}