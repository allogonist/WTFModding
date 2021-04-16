In this directory you have everything you need to build a backdrop extension dll with the same contents as BackdropsCore.dll

Getting started:
	The first thing you should do is open WTFModSolution.sln inside the BackdropsCore folder and then find the class file named:
		MyLoader.cs
		
	this class file implements a single function Load(...) which is where all extension data gets put into the data structures used by Wayward Terran Frontier

Repo contents:
	* BackdropsCore - An example project for visual studio that is configured to build a backdrop extension dll
	* PrefabEditor -  source code for our prefab editor. It is old and horrible, but should run and let you edit prefabs. Feel free to improve or replace all of that code
	* WaywardExtensions.dll - Our mod API. It is a library containing a namespace with all of the functions, and types that are shared between the main exe and our extensions
	* MonoGame.Framework.dll - we compile monogame from source and we have made some small modifications to it. This is our compiled binary.
	* built in ships - every built in ship design in profile form. You can load the profile in the WTF ship editor and export it to a flotilla with the same name
	* 280_-200.bdm.png - our main map file in png format so you can easily see its contents for reference when looking at the extension project code

Summary of what you can do:
	This is how all of world generation is accomplished. You can define the full visual contents of every sector at any location in the game up to and including your own rendering code for backdrops.
	- place your own terrain types with custom artwork
	- design your own 3d backdrop artwork
	- add nodes to instruct the built in AI factions to spawn ships and stations

BackdropsCore.dll defines all static world elements in Zero Falls which are not created by procedural generation. That means all biomes, all terrain, and all spawn locations for ships and stations that make up the Zero Falls story zones are defined inside. If there is an example of something we have done with the engine relating to world generation or backdrop artwork that you would like to copy, it is probably in there somewhere.

The game on startup opens every dll in the extensions folder and looks for class files implementing the ExtensionLoader interface. It then calls the function load(...) inside of each one in the order that it finds them.

Everything in BackdropsCore will already be in the default install of your game, it is only left here to serve as an example to help you make you own.