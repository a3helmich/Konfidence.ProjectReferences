# ProjectReferences

Report redundant project references

## ProjectReferencesTool

The project references tool is a console application which scans your .cs projects for redundant references to other projects.

### Using the tool
- package: The project references tool is published as a package on [nuget.org - Konfidence.Project-References](https://www.nuget.org/packages/Konfidence.Project-References).
- install: run 'dotnet tool install --global Konfidence.Project-References'
- basic run: go to your project folder and run 'project-references'.
- result 1: displays redundant project references within the found projects in your (sub-)folders.
- result 2: creates a 'redundant.txt' file, which contains the results displayed in the console.
- actions: update the references in your projects and remove the redundant project references
- where: because it is a dotnetcore console application, it runs on both windows and linux.
- for whom: all dotnet c# developers creating solutions containing large amounts of projects.
 
### What does it do
- Example:
 
	If this is how the references in our project looks like (take note of the highlighted references).

	![Redundant projects example](./readme/redundant-projects.PNG)
 
	And this is the project, containing a project which should not need to be referenced in our project, because of the implicit reference. 

	![](./readme/tool-classes.PNG)  
  
	You would want to remove the reference to the ToolInterfaces project from our project, because it is already refrenced by, in this case, the ToolClasses project. 
	
	Like this:

	![](./readme/non-redundant-projects.PNG)

	Easy to find when you have like 5 projects in your solution. But a bit harder with something like a 100 projects. 

- Running the tool would give:

	![](./readme/console-output.PNG)

- Also creating the file 'redundant.txt':

	![](./readme/redundant-txt.PNG)



