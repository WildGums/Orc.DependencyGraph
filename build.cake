var projectName = "Orc.DependencyGraph";
var projectsToPackage = new [] { "Orc.DependencyGraph" };
var company = "WildGums";
var startYear = 2010;
var defaultRepositoryUrl = string.Format("https://github.com/{0}/{1}", company, projectName);

#l "./deployment/cake/variables.cake"
#l "./deployment/cake/tasks.cake"
