## Jam – Interactive Story Game Builder ##

  The project Jam is a web application built with ASP.NET Core MVC and Entity Framework Core, 
  allowing users to create, edit, and play interactive story-based games.
  
  Each game features branching narratives, dynamic questions, and multiple endings.

--

## Technology Stack ##

  Backend:  ASP.NET Core MVC (.NET 8.0)
  
  Database: SQLite
  
  ORM: Entity Framework Core
  
  Frontend: Bootstrap 5.3.8, jQuery 3.7.1
  
  Build Tools: Node.js (npm)
  
  Architecture: MVC + Repository Pattern + ViewModels

--

## Setup & Installation ##

  ### Requirements ###
  
  	NET 8.0 SDK → (https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
  	
  	Node.js 20+ (https://nodejs.org/) (for frontend dependencies)
  --
  
  ### How to Start the Webapplication ###
  
  1. Clone the repository
  		git clone https://github.com/jaaza9995/Webapplikasjoner-Eksamen.git
  		cd Jam
  
  2. Install frontend dependencies
  		npm install
  
  3. Create the database
  		If using Entity Framework Core migrations:
  			dotnet ef database update
  
  4. Run the application
  		dotnet run
  
  5. Open in your browser
  		https://localhost:<port-number>
--

## Developers: ##

	Sigrid Aarnes
	Jara Arif Azad-Nouri 
	Michael Nedgård Isaksen
