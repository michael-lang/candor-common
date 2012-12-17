using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Common.Logging;

namespace PrepareSharedLibrariesForRelease
{
	class Program
	{
		private static string SolutionPath;// = Path.GetFullPath(@"..\..\..\CandorCore.sln");
		private static  string SolutionFolder; 
		private static string ReleaseVersion;

		private static readonly XNamespace MSBuildNS =
			"http://schemas.microsoft.com/developer/msbuild/2003";

		private static ILog Log { get { return LogManager.GetLogger(typeof (Program)); }}

		static void Main(string[] args)
		{
			try
			{
				SolutionPath = args[0];
				SolutionFolder = Path.GetDirectoryName(SolutionPath);
				ReleaseVersion = args[1];
				var solution = new Solution(SolutionPath);

				// Filter out things other than CSharp Projects that Visual Studio 
				// represents as projects in the solution file, like solution folders
				IEnumerable<SolutionProject> cSharpProjects =
					solution.Projects.Where(project => project.RelativePath.EndsWith(".csproj"));

				// First, wipe out any existing dependencies so we're starting from a clean slate

				foreach (SolutionProject project in cSharpProjects)
				{
					Log.InfoFormat("Removing any dependencies from project {0}", project.ProjectName);
					if (IsProjectNuGetPackage(project))
					{
						Log.InfoFormat("Project {0} found to be a NuGet package project", project.ProjectName);
						RemoveNuGetDependencies(project);
					}
				}

				// Then run through all the projects, adding a NuGet dependency
				// whenever we see a project reference to a project that has a nuspec file
				foreach (SolutionProject project in cSharpProjects)
				{
					Log.InfoFormat("Setting up NuGet dependencies for project {0}", project.ProjectName);
					string projFilePath = Path.Combine(SolutionFolder, project.RelativePath);
					string projFileFolder = Path.GetDirectoryName(projFilePath);
					XDocument projFile = XDocument.Load(projFilePath);
					var projectReferences = projFile.Descendants(MSBuildNS + @"ProjectReference");
					foreach (XElement projectReference in projectReferences)
					{
						if (IsReferencedProjectNuGetPackage(projFileFolder, projectReference) &&
							IsProjectNuGetPackage(project))
						{
							Log.InfoFormat("NuGet project {0} found to be dependent on NuGet project reference {1}", project.ProjectName, projectReference.Value);
							EnsureProjectIsNugetDependency(projFilePath, projectReference, project);
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		private static void RemoveNuGetDependencies(SolutionProject project)
		{
			string projFilePath = Path.Combine(SolutionFolder, project.RelativePath);
			string nuspecPath = GetExpectedNuGetFileFullPath(projFilePath);
			XDocument nuspecDocument = XDocument.Load(nuspecPath);

			XElement dependenciesElement =
				nuspecDocument.Descendants("dependencies").SingleOrDefault();

			if (dependenciesElement != null)
			{
				dependenciesElement.Remove();
				nuspecDocument.Save(nuspecPath);
			}

		}


		private static void EnsureProjectIsNugetDependency(string projFilePath, XElement dependencyToAdd, SolutionProject dependentProject)
		{
			var dependentProjectNuspecPath = GetExpectedNuGetFileFullPath(projFilePath);
			var dependencyProjectNuspecPath = GetNuspecPath(Path.GetDirectoryName(projFilePath),
			                                                dependencyToAdd);
			string packageId = GetNuGetPackageId(dependencyProjectNuspecPath);

			XDocument nuspecDocument = XDocument.Load(dependentProjectNuspecPath);
			
			// First get the dependencies node
			XElement dependenciesElement =
				nuspecDocument.Descendants("dependencies").SingleOrDefault();

			if (dependenciesElement == null)
			{
				XElement metadataElement =
					nuspecDocument.Descendants("metadata").SingleOrDefault();

				if (metadataElement == null)
					throw new ArgumentException(
						"Invalid nuspec file.  It is missing its metadata element");

				dependenciesElement = new XElement("dependencies",
					new XElement("dependency", new XAttribute("id", packageId), new XAttribute("version", ReleaseVersion)));

				metadataElement.Add(dependenciesElement);
			}
			else
			{
				XElement dependencyElement =
					dependenciesElement.Elements("dependency").SingleOrDefault(
						element => element.Attribute("id").Value == packageId);

				if (dependencyElement != null)
				{
					dependencyElement.Attribute("version").Value = ReleaseVersion;
				}
				else
				{
					dependencyElement = new XElement("dependency", new XAttribute("id", packageId), new XAttribute("version", ReleaseVersion));					
					dependenciesElement.Add(dependencyElement);
				}
			}

			nuspecDocument.Save(dependentProjectNuspecPath);
		}

		private static string GetNuGetPackageId(string nuspecPath)
		{
			Log.InfoFormat("Inside GetNuGetPackageId - Loading nuspec at {0}", nuspecPath);
			XDocument nuspecDocument = XDocument.Load(nuspecPath);
			XElement idElement = nuspecDocument.Descendants("id").SingleOrDefault();
			if (idElement == null)
				throw new ArgumentException(
					"Invalid nuspec file.  Metadata does not include an id element");

			return idElement.Value;
		}

		private static string GetNuspecPath(string projFileFolder, XElement projectReference)
		{
			string projectRelativePath = projectReference.Attribute(@"Include").Value;
			string projectFullPath = Path.Combine(projFileFolder, projectRelativePath);
			string nuspecPath = GetExpectedNuGetFileFullPath(projectFullPath);
			return nuspecPath;
		}

		private static bool IsProjectNuGetPackage(SolutionProject project)
		{
			Log.InfoFormat("Checking if project {0} is a NuGet package project", project.ProjectName);
			string projectPath = Path.Combine(SolutionFolder, project.RelativePath);
			Log.InfoFormat("Calculated projectPath = {0}", projectPath);

			return IsProjectNuGetPackage(projectPath);
		}

		private static bool IsReferencedProjectNuGetPackage(string projFileFolder, XElement projectReference)
		{
			string referencedProjectRelativePath = projectReference.Attribute(@"Include").Value;
			string referencedProjectFullPath = Path.Combine(projFileFolder,
			                                                referencedProjectRelativePath);
			return IsProjectNuGetPackage(referencedProjectFullPath);
		}

		private static bool IsProjectNuGetPackage(string referencedProjectPath)
		{
			var expectedNuGetFileFullPath = GetExpectedNuGetFileFullPath(referencedProjectPath);
			Log.InfoFormat("Looking for nuspec file at expected location: {0}", expectedNuGetFileFullPath);
			return File.Exists(expectedNuGetFileFullPath);
		}

		private static string GetExpectedNuGetFileFullPath(string referencedProjectPath)
		{
			string referencedProjectFileName = Path.GetFileName(referencedProjectPath);
			string referencedProjectFolder = Path.GetDirectoryName(referencedProjectPath);

			int lengthOfFileNameWithoutCSProj = referencedProjectFileName.Length - ".csproj".Length;
			string expectedNuGetFile = referencedProjectFileName.Substring(0, lengthOfFileNameWithoutCSProj) + ".nuspec";
			string expectedNuGetFileFullPath = Path.Combine(referencedProjectFolder,
			                                                expectedNuGetFile);
			return expectedNuGetFileFullPath;
		}
	}
}
