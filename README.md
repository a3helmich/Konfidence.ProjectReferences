# ProjectReferences

Report redundant project/package references

## ProjectReferencesTool

The project references tool is a console application which scans your .cs projects for redundant project/package references — the ones a project already gets another way, either through a project it references or through another package it references.

### Using the tool
- package: The project references tool is published as a package on [nuget.org - Konfidence.Project-References](https://www.nuget.org/packages/Konfidence.Project-References).
- install: run 'dotnet tool install --global Konfidence.Project-References'
- basic run: in a console go to your solution folder and run 'project-references'. It scans the solution named after that folder, use '--AllProjects' to scan every project below it instead.
- result 1: console displays the redundant project/package references within the scanned projects, grouped per project. A project reference is marked with a '-' and listed by its .csproj path, a package reference with a '+' and listed by name with a .nupkg extension.
- result 2: creates a 'redundant.txt' file, which contains the results displayed in the console.
- restore: packages a project gets through *another package* are read from the restore output, so those are only checked for projects that have been restored. The tool says how many projects it had to skip. Everything else — project references, and packages brought by a referenced project — needs nothing but the source files.
- actions: manually update the references in your projects and remove the redundant ones. A redundant package reference is worth a second look first: a version pinned on purpose is a reason to keep one. Packages the other project declares with 'PrivateAssets=all' are not reported, because those do not reach you.
- where: because it is a dotnetcore console application, it runs on both windows and linux.
- for whom: all dotnet c# developers creating solutions containing large amounts of projects.
 
### What does it do
- Example:
 
	If this is how the references in our project looks like (take note of the highlighted references).

	![Redundant projects example](https://raw.githubusercontent.com/a3helmich/Konfidence.ProjectReferences/develop/readme/redundant-projects.PNG)
 
	And this is the project, containing a project which should not need to be referenced in our project, because of the implicit reference. 

	![](https://raw.githubusercontent.com/a3helmich/Konfidence.ProjectReferences/develop/readme/tool-classes.PNG)  
  
	You would want to remove the reference to the ToolInterfaces project from our project, because it is already refrenced by, in this case, the ToolClasses project. 
	
	Like this:

	![](https://raw.githubusercontent.com/a3helmich/Konfidence.ProjectReferences/develop/readme/non-redundant-projects.PNG)

	Easy to find when you have like 5 projects in your solution. But a bit harder with something like a 100 projects. 

- Running the tool would give:

	![](https://raw.githubusercontent.com/a3helmich/Konfidence.ProjectReferences/develop/readme/console-output.PNG)

	A package reference works the same way. If 'ToolClasses' references a package and our project references both 'ToolClasses' and that same package, our own package reference adds nothing — 'ToolClasses' already brings it. The same holds between packages: if we reference a package which itself depends on a second package, referencing that second one directly adds nothing either.

	Two things stop a package being reported. A package the other project declares with 'PrivateAssets=all' does not flow to us, so ours is not redundant. And a project which has not been restored has no package dependency information, so packages brought by other packages are not checked for it.

	A report looks like this:

```
Redundant project/package references:
\Test\Konfidence.BaseClasses.UnitTest\Konfidence.BaseClasses.UnitTest.csproj
     - \Konfidence.BaseClasses\Konfidence.BaseClasses.csproj
     + Microsoft.NET.Test.Sdk.nupkg
```

- Also creating the file 'redundant.txt':

	![](https://raw.githubusercontent.com/a3helmich/Konfidence.ProjectReferences/develop/readme/redundant-txt.PNG)

### How to run
- project-references : scans the solution named after your current folder, so 'c:\projects\myapp' uses 'myapp.sln', or 'myapp.slnx' when that is the one present.
- project-references --AllProjects : scans all csproj projects in your current folder and all it's subfolders, ignoring any solution.
- project-references --BasePath=c:\projects\myproject : uses 'c:\projects\myproject\myproject.sln'.
- project-references --Solution=mysolution : scans only the csproj projects listed in 'mysolution.sln' in your current folder.
- project-references --BasePath=c:\projects\myproject --Solution=mysolution : scans only the csproj projects listed in 'c:\projects\myproject\mysolution.sln'.
- project-references --Solution=nosuchsolution : reports 'not found : solution file' and stops.
- project-references --Solution=mysolution --AllProjects : ignores the named solution and scans every csproj project below the base path.
- project-references --Help : shows the available arguments and exits, without scanning anything.

### Arguments
- --BasePath= folder to work from, and to scan for csproj files including it's subfolders. Defaults to the folder you run the tool from. A trailing separator is ignored. [--BasePath=mypath]
- --Solution= name of the solution file, only the csproj files in the solution file are scanned. Both '.sln' and '.slnx' are read. The extension is optional; leave it off and the '.sln' is preferred when both formats are present. Leave the whole argument off and the solution named after the base path folder is used, so a folder 'myapp' looks for 'myapp.sln' and then 'myapp.slnx'. When the solution cannot be found the tool reports it and stops. [--Solution=mysolution]/[--Solution=mysolution.slnx]
- --AllProjects switch, ignores the solution altogether and scans every csproj file below the base path. This is the only way to scan projects which are not part of a solution. [--AllProjects]
- --Help switch, shows the available arguments and exits. A successful run prints a one line reminder that it exists, a run which reports a problem shows the arguments straight away. [--Help]

Both '--argument=value' and '--argument value' are accepted. Always start an argument with '--':
'--Solution mysolution' works, 'Solution mysolution' is not read at all. An argument the tool cannot
read stops the run and is named, rather than being dropped and scanning something else instead:

```
> project-references solution mysolution.sln
ignored : 'solution', 'mysolution.sln' - arguments start with --, see --Help
==============================================================================
valid arguments : [--BasePath=BasePath] [--Solution=Solution] [--AllProjects] [--Help]
...
```

Arguments are case insensitive

### Issues
To be clear: issues with your solution/csproj's, not with the project-references tool.

After removing the project references, you are unable to build/rebuild your solution from visualstudio.

- There is a big chance you have to rebuild your project references tree: from a console run 'dotnet clean [my.sln]' 'dotnet restore [my.sln]'. This will reset everything and building will probably work.
- There are unused usings referencing implicitly referenced projects. This seems to corrupt the project reference tree. Always cleanup your usings when you are finished, then: from a console run 'dotnet clean [my.sln]' 'dotnet restore [my.sln]'. This will reset everything and building will probably work.
- There are old dll files which are not being removed with a clean. Remove all 'bin' && 'obj' folders, then clean and restore. 
