add nuspec file
add .proj file, at build do the following:
	- copy in AccountController to Content\Controllers\
	- copy in classes in \Models\Account\ to Content\Models\Account\
	- copy in views from \Views\Account\ to Content\Views\Account
	- replace namespace 'CandorMvcApplication' with $safenamespace$? check article on proper token
	- build nupkg from folder (and nuspec)
update release.proj to include nuget pack of new project.



http://blog.avangardo.com/2010/11/how-copy-projects-files-using-msbuild/

http://docs.nuget.org/docs/creating-packages/creating-and-publishing-a-package

http://docs.nuget.org/docs/creating-packages/configuration-file-and-source-code-transformations
