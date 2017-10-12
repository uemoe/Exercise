# Coding Exercise
### Task description:
	Given some json-data extract-transform-load it to database. Write code using C# and .Net Core.
### Solution
   I decided follow my learning path and use that simple task to make research on whole .net core 2.0 infrastructure.
   I have a second goal in mind, to check for myself how mature became ef 2.0 and code first approach for creating database, changing it incrementally in development and deploy to production. And despite of task description I decided to use real MSSQL local database to get through whole SDLC. Actually, 

## Architecture overview
 * .NET Standard 2.0 library, Net Core 2.0 Console Application
 * ORM: - EntityFrameWorkCore 2.0, Code first model for database
 * Test Framework: xUnit ('cause I'm less familiar with it, and it looks like standart de-facto for .net core projects)	
 * JSON parsing library - NewtonSoft JSON.NET
 * Db: MSSQL server localdb / InMemoryProvider for testing ?SQLite inMemory for more reliable testing.
 * IDE: Visual Studio 2017, Visual Studio Code.


### Structure of solution.
* Library of Core Domain (project named $SolutionName.Domain) - couple of code first classes of domain entities. Actually, each entity consists of two versions  - naive and classical.
*  Two library of DAL (projects named $SolutionName.Data and $SolutionName.DataNaive). DbContext classes with tiny customizations of model.
* Console App named SomeUI (because some ef core tooling needed startup project) also  consists classes for parsing and transforming JsonData. Defenitely, that classes should be relocated in separate project. 
* TestProject named Test.
    
### Instruction
dotnet run

dotnex xunit


## What interesting about task.
### Data modeling
   Why there are two versions of DAL? Because use cases was not very clear and actually I have no idea of roots of JSON data and how persistent it is? And for naive core domain implementation I decided for simlicity use Ids from Json file for my PK of core domain entities. Very controversial idea I would say, even more its definitely antipattern in database design. But it would easy for me to check two fundamental database use-cases I decided to implement.
   Second db-modeling approach I implementing was classical Microsoftish way to do it (with identity column in each table). For entity identification in that model I choosed the same origin Ids, but it could be any other/others fields.
 See in source code. 
 
   Actually on later stages the first Naive DAL project became obstacle to compile whole solution, some cyclic referencee, and I moved in another solution, but because between both of them not much differences. Only customization of model. See DataNaiveContext OnModelCreation() code below.
   ``` CSharp
           protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var meeting = modelBuilder.Entity<Meeting>();
            meeting.Property(m => m.Date)
                .HasColumnType("Date");
            meeting.Property(m => m.Name)
                 .HasMaxLength(512);
            meeting.Ignore(m => m.OuterId);

            meeting.Property(p => p.Id)
                .ValueGeneratedNever();

            var race = modelBuilder.Entity<Race>();
            race.Property(r => r.Id).ValueGeneratedNever();

            race.Ignore("OuterId");

            base.OnModelCreating(modelBuilder);
        }
```

### Two common use cases for saving data.
#### 1. Save whole new parent/childs hierarhy to database.
  First use case was very easy and straightforward to implement. 
  Only pitfall was in the db-migration. I adored how cool migration files are generated and look in DAL projects, really awesome feature ef core. But in pretty simple database lifecycle scenario, when I firstly created PK as usual in MSSQL as identity column, but then decided to use foreign Ids and switch that conventions in my model off. Update-Database command gave me an error that says something like that: 
  
  > "System.InvalidOperationException: To change the IDENTITY property of a column, the  column needs to be dropped and recreated."  

   And it fails, no workarounds was found. Only option was delete all my migration files in that project and related dev-db and reinit it from model again.
 
  I don't know what a big problem to recreate table with data migration, but it looks like in code-first approach there not production ready tooling at all, specially for supporting database in-production development. In SSDT-projects create that migration is very easy task.
  For example, nuget-package with ef core 2-cli don't install from nuget-console or NuGet manager for more than half year but workaround exists hand-editing root project-file - simple copy-paste name of package and version and save.
 #### 2. Save the same JSON-data twice, second time with changes.  
  On implementing second use-case I spent most of the time experimenting and deep diving in in current state of ef core 2.0. I have worked through couple Pluralsight courses and read all recent articles from Julie Lerman about EF core 2.0 [Part 1](https://msdn.microsoft.com/magazine/mt842503) [Part 2](https://msdn.microsoft.com/magazine/mt826347). Also very interesting and informative blog [Artur Vickers](https://blog.oneunicorn.com/2016/11/17/add-attach-update-and-remove-methods-in-ef-core-1-1/), he is developer on the Entity Framework team at Microsoft.
  
  I try to follow only the standard ef core ways (without hardcore sql-guy hacks, like to bulk copying all data in temporary table and then use SQL Server UPSERT operator MERGE to do real work on server in stored proc). 
 And I came up with two main Saving Strategy. 
 ##### 2.1 Before insert whole package, check if exists parent entity already exists then remove parent entity from db.
 "Immutability" way (before insert new staff - delete old staff - then insert) seems only good maintainable enough. If parent entity already exists in our core database, then delete it with children and recreate it from brand new-json file. 
 For me very bad way to do things in relational db but in NoSQL database I think it would working well. Simple insert new version of document and is it. 
  In our case for model with "natural" PK (without identity) Id entity don't change. But delete most resource-consuming operation in RDBS.
  
EF produced not very effective insert command for new entities.

```SQL
 Executed DbCommand (27ms) [Parameters=[@p4='09/01/2017 11:00:00', @p5='1', @p6='Australian Turf Club' (Size = 4000), @p7='1', @p8='90011001', @p9='09/01/2017 10:45:00', @p10='09/01/2017 11:35:00', @p11='1', @p12='Tab Rewards Maiden Hcp' (Size = 4000), @p13='2', @p14='90011002', @p15='09/01/2017 11:15:00', @p16='09/01/2017 15:20:00', @p17='1', @p18='Tab Rewards Maiden Hcp' (Size = 4000), @p19='3', @p20='90011003', @p21='09/01/2017 15:05:00'], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      DECLARE @inserted0 TABLE ([Id] int, [_Position] [int]);
      MERGE [Races] USING (
      VALUES (@p4, @p5, @p6, @p7, @p8, @p9, 0),
      (@p10, @p11, @p12, @p13, @p14, @p15, 1),
      (@p16, @p17, @p18, @p19, @p20, @p21, 2)) AS i ([Endtime], [MeetingId], [Name], [Number], [OuterId], [Starttime], _Position) ON 1=0
      WHEN NOT MATCHED THEN
      INSERT ([Endtime], [MeetingId], [Name], [Number], [OuterId], [Starttime])
      VALUES (i.[Endtime], i.[MeetingId], i.[Name], i.[Number], i.[OuterId], i.[Starttime])
      OUTPUT INSERTED.[Id], i._Position
      INTO @inserted0;

      SELECT [t].[Id] FROM [Races] t
      INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id])
      ORDER BY [i].[_Position];
```
  
##### 2.2 Copy linked to parent data hierarchy from db to the client and compare it by hand with new one graph, than mark existing entity for update. New one would be inserted. 
  Our main update data scenario seems far from main focus of Ef core team. Because they optimized with get data from db and made some changes local and save differences back to db. But our main use case is full disconnected graph with Ids (in naive modeling) or without Ids but with alternate keys and we needed to compare both graphs with version saved in DB and apply differences. And only way to get it done in ef core was compare by hand on client-side an.
  In detail scenario
```SQL

```
### Testing
I made couple tests most of them the integration tests.

Firstly, I tested my dbcontext and logic for three Database Providers. Two inMemory simple and SQLite, and real SQL Server. I wrote special test to show differences between simple inMemory and SQLite inMemory, when it works for inMemory but fails in real database.
I used [theory] attribut for testing the same test simultaneosly in three providers.

#### Testing takeaways
Because of functional differences it impossible without rewriting code or conditions to reuse the same test for both inMemory providers. 
But it seems that SQLite provider close to the real database and you should decide whice evil to choose. Write two sets of tests (SQLite and MSSQL) or just use simple inMemory for stubbing DB, and real testing made in integration tests.

For real MsSQL providers simple test in hundreds time longer than inMemory. 10 sec vs 0.01 sec.


### Production ready and cleaning up source code.
Most of the time I spent investigating features of EF core.
Because writing in .net core is new to me, a lot of infrastructure type task I had to deal with. For example today I spent couple of hours to bring DI to the .net core console app.  I think for that I deserve [FizzBuzzEnterprizeEdition](https://github.com/EnterpriseQualityCoding/FizzBuzzEnterpriseEdition) award. :->

When time come to show the code, I decided to stop cleaning today. 
But maybe I'll think about that tomorrow.
