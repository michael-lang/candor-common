**********************************************************
**
**  Candor Shared Library - Release Scripts & Tools
**
**********************************************************
**
**  PrepareNuSpecsForRelease - 
**
**  Console app that analyzes the project files of the rest of the solution
**  and updates all of the nuspec files' dependency nodes.  It ensures
**  that any time it sees a project reference between two projects that
**  are both being packaged as NuGet packages that a dependency is
**  declared between them in the appropriate nuspec file.
**
**********************************************************
**
**  Release.proj
**
**  This is the MSBuild script that controls the steps of the release process
**
***********************************************************
**
**  Release.cmd
**
**  A command-line wrapper for the build script.  Captures the appropriate
**  MSBuild options.  In order to create a release versioned 1.2.0, execute
**  Release 1 2 0
**
***********************************************************