md NuGetRelease

.nuget\nuget pack Candor\Candor.csproj -Build -p Configuration=Release -includeReferencedProjects -o "NuGetRelease"

.nuget\nuget pack Candor.Security\Candor.Security.csproj -Build -p Configuration=Release -includeReferencedProjects -o "NuGetRelease"

.nuget\nuget pack Candor.Security.AzureStorageProvider\Candor.Security.AzureStorageProvider.csproj -Build -p Configuration=Release -includeReferencedProjects -o "NuGetRelease"

.nuget\nuget pack Candor.Security.SqlProvider\Candor.Security.SqlProvider.csproj -Build -p Configuration=Release -includeReferencedProjects -o "NuGetRelease"

.nuget\nuget pack Candor.Tasks.ServiceProcess\Candor.Tasks.ServiceProcess.csproj -Build -p Configuration=Release -includeReferencedProjects -o "NuGetRelease"

.nuget\nuget pack Candor.Web.Mvc\Candor.Web.Mvc.csproj -Build -p Configuration=Release -includeReferencedProjects -o "NuGetRelease"

msbuild Candor.Web.Mvc.Bootstrap.ErrorHandler\Candor.Web.Mvc.ErrorHandler.proj

.nuget\nuget pack Candor.Web.Mvc.Bootstrap.ErrorHandler\Candor.Web.Mvc.ErrorHandler.nuspec -o "NuGetRelease" -verbosity detailed

msbuild Candor.Web.Mvc.Bootstrap.Security\Candor.Web.Mvc.Security.proj

.nuget\nuget pack Candor.Web.Mvc.Bootstrap.Security\Candor.Web.Mvc.Security.nuspec -o "NuGetRelease" -verbosity detailed

.nuget\nuget pack Candor.WindowsAzure\Candor.WindowsAzure.csproj -Build -p Configuration=Release -includeReferencedProjects -o "NuGetRelease"

.nuget\nuget pack Candor.WindowsAzure.Logging.Common\Candor.WindowsAzure.Logging.Common.csproj -Build -p Configuration=Release -includeReferencedProjects -o "NuGetRelease"

.nuget\nuget pack Candor.WindowsAzure.Tasks\Candor.WindowsAzure.Tasks.csproj -Build -p Configuration=Release -includeReferencedProjects -o "NuGetRelease"

.nuget\nuget pack Candor.Security.AzureStorageProvider\Candor.Security.AzureStorageProvider.csproj -Build -p Configuration=Release -includeReferencedProjects -o "NuGetRelease"