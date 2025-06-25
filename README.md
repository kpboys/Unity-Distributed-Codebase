# Unity-Distributed-Codebase
This is a proof-of-concept for distributing various scenarios in Unity across multiple projects, rather than having a single codebase.

Made as part of a bachelor project, we had experienced an issue at a company we were interns at, where the company developed various Virtual Reality training scenarios in a single Unity project, which of course created various problems, including limited ability to configure project settings per scenario, created uncertain ownership of code and duplicate code from that, as well as increased the complexity of automated testing massively.

So this project was made as a proof-of-concept to try and solve those issues.

The first part of the solution was to allow to split the monolithic Unity project up into multiple projects, one for each scenario, by developing a plugin that would allow one Unity build to open another one, allowing for seamless transition between scenarios with no user input needed.
We managed to develop this plugin, called 'ScenarioLauncherPluginVer0.7' in the repo, on any android platform by starting the other Unity build as a Android activity.

The second part of our solution was to create an internal tool for the company to install these scenarios on the headsets. As parts of the business model they deliver VR headsets pre-configured with all the necessary scenarios, so a tool to install the correct scenarios was needed.
This tool can be found in the folder 'Prim8VR_Installer', which can be used to install Unity builds onto Android devices. The builds will be downloaded from github repos added to it's config.

The third part of the solution was to explore how to effectively automate testing in distributed Unity projects, which was done by making an example scenario with minimal functionality and then covering it with Unit/Integration/Gameplay tests. 
The scenario can be found in the folder 'scenario_a'.
We found during development that Unity is particurlarly unsuited for Unit testing due to the extra work involved in isolating the code from components, 
integration tests were much more obviously useful due to the need to often integrate against the Unity engine, 
and gameplay tests were very difficult to implement due to the need to simulate player input and the added complexity of doing so with a VR headset.
Overall, for optimal value of testing in Unity, use alternative kinds of tests such as graphical tests, and be prepared to invest in better input simulation, for example a generic domain ai planner.
