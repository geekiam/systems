﻿#tool "dotnet:?package=GitVersion.Tool&version=5.10.3"
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
string version = String.Empty;

string projectTag = "FeedManagement";
string rootNamespace = "Geekiam";

string packageName = string.Empty;
string containerRegistry = EnvironmentVariable("CONTAINER_REGISTRY");
//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() => {
    DotNetClean("./Systems.sln");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Description("Restoring the solution dependencies")
    .Does(() => {
    
    var projects = GetFiles("./**/**/*.csproj");
    var settings =  new DotNetRestoreSettings
    {
      Verbosity = DotNetVerbosity.Minimal,
      Sources = new [] { "https://api.nuget.org/v3/index.json" }
    };
    foreach(var project in projects )
    {
      Information($"Restoring { project.ToString()}");
      DotNetRestore(project.ToString(), settings);
    }

});

Task("Version")
  .IsDependentOn("Restore")
   .Does(() =>
{
   var result = GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true
    });
    
    version =  result.FullSemVer.ToString();
    Information($"Nuget Version: { version.ToString() }");
    Information($"Semantic Version: { result.FullSemVer.ToString() }");
});

Task("Build")
    .IsDependentOn("Version")
    .Does(() => {
     var buildSettings = new DotNetBuildSettings {
                        Configuration = configuration,
                       };
     var projects = GetFiles("./**/**/*.csproj");
     foreach(var project in projects )
     {
         Information($"Building {project.ToString()}");
         DotNetBuild(project.ToString(),buildSettings);
     }
});



Task("Test")
    .IsDependentOn("Build")
    .Does(() => {

       var testSettings = new DotNetTestSettings  {
                                  Configuration = configuration,
                                  NoBuild = true,
                              };
     var projects = GetFiles("./tests/Unit/*.csproj");
     foreach(var project in projects )
     {
       Information($"Running Tests : { project.ToString()}");
       DotNetTest(project.ToString(), testSettings );
     }


});

Task("Publish")
    .IsDependentOn("Test")
    .Does(() => {

     
     var projects = GetFiles("./src/front/front.csproj");
     foreach(var project in projects )
     {
       var publishSettings = new DotNetPublishSettings  {
                                       Configuration = configuration,
                                       NoBuild = true,
                                       OutputDirectory = ".publish",
                                   };
       Information($"Publishing API : { project.ToString()}");
       DotNetPublish(project.ToString(), publishSettings );
     }


});



//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Task("Default")
       .IsDependentOn("Clean")
       .IsDependentOn("Restore")
       .IsDependentOn("Version")
       .IsDependentOn("Build")
       .IsDependentOn("Test")
       .IsDependentOn("Publish");

RunTarget(target);