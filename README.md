# What is this project about?

This is a windows application design to assist in organizing Dungeons and Dragons campaigns.

# Who is this project for?

This project was primarily created to assist me in managing my own campaigns, however, anyone else who is running a D&D campaign may find it useful.

# Installation instructions

The application is currently in the prototype stage and does not have a proper install aside from cloning the repository into Visual Studio.
However, there is a compiled version under releases available to test out.

The application also requires the installation of the [Neo4j database software](https://neo4j.com/download-center/#community).
Once the community version is downloaded, you must go into neo4j.conf and type in `dbms.security.auth_enabled=false`. ***The application will not function unless this is done.***
Afterwards, you can start up the database by navigating to the folder in powershell and executing the command `powershell -ExecutionPolicy Bypass -File neo4j.ps1 console`.

The first official release will include autostart when you open the app, so this can be done automatically, but for now the database needs to be manually started each time.

# Technical Description

## Code

The code is exclusively in C# and uses the Windows Presentation Foundation (WPF) framework. WPF is almost 20 years old,
however, I found it to be quite intuitive and easy to do many complex UI-related operations with it. Additionally, it is
well supported by Visual Studio.

### Structure

The code is currently broken up into 3 primary sections: the sidebar, the flowchart, and the main window.

#### Sidebar

* Contains all the sidebar functionality, including:
	* The button entities
	* The search bar
	* The database queries
* Also calls the flowchart entity creation functions

#### Flowchart

* Contains all the flowchart related functions, such as:
	* The flowchart entities and lines
	* The draggability functions of the entities and lines
	* The zooming function

#### Main window

* Contains all of the event handlers for the UI controls, and initializes the other components (sidebar, flowchart)

## Styling

The Styling is done in XAML and primarily contians the general structure of the controls, as well as the styles for each of them

## Database

The application utilizes a Neo4j graph database in order to keep track of all the D&D related information.
This allows the application to more easily traverse through the relationships and information on them.

### Structure

There are currently 4 types of nodes: Campaigns, Adventures, NPCs, and Encounters

They are arranged based on hierarchy, where Campaigns are the at the top, adventures in the middle, and NPCs and Encounters on the bottom.
Such that, adventures belong to campaigns, and encounters and npcs belong to adventures.

## Current and future goals

Current goals:

* Allowing the user to add nodes in the application
* Adding a plot chart
* Adding a description control to the nodes
* Improving the styling of the controls

Future goals:

* Adding a custom background functionality (suggested)
* Adding a dice roller (suggested)
* Adding an installer for the application that also installs Neo4j
* Neo4j database autostart on application startup
