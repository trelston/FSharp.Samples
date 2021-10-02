//Part 1: Using F# to explore and develop interactively


(*1. Use F# to explore the .NET framework interactively
When I'm coding, I often have little questions about how the .NET library works. 
For example, here are some questions that I have had recently that I answered 
by using F# interactively:
    Have I got a custom DateTime format string correct?
    How does XML serialization handle local DateTimes vs. UTC DateTimes?
    Is GetEnvironmentVariable case-sensitive?
All these questions can be found in the MSDN documentation, of course, but can also answered 
in seconds by running some simple F# snippets, shown below.
*)


//Have I got a custom DateTime format string correct?
open System
DateTime.Now.ToString("yyyy-MM-dd hh:mm")
DateTime.Now.ToString("yyyy-MM-dd HH:mm")

//------------------------------------------------------------------------------------


//How does XML serialization handle local DateTimes vs. UTC DateTimes?
//Tip: sets the current directory to be the same as the script directory
System.IO.Directory.SetCurrentDirectory(__SOURCE_DIRECTORY__)

[<CLIMutable>]
type DateSerTest = {Local: DateTime;Utc:DateTime}

let ser = new System.Xml.Serialization.XmlSerializer(typeof<DateSerTest>)

let testSerialization (dt:DateSerTest) = 
    let filename = "serialization.xml"
    use fs = new IO.FileStream(filename , IO.FileMode.Create)
    ser.Serialize(fs, o=dt)
    fs.Close()
    IO.File.ReadAllText(filename) |> printfn "%s"

let d = { 
    Local = DateTime.SpecifyKind(new DateTime(2014,7,4), DateTimeKind.Local)
    Utc = DateTime.SpecifyKind(new DateTime(2014,7,4), DateTimeKind.Utc)
    }

testSerialization d
(*Output
<?xml version="1.0"?>
<DateSerTest xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Local>2014-07-04T00:00:00+05:30</Local>
  <Utc>2014-07-04T00:00:00Z</Utc>
</DateSerTest>
*)
//So i can see it uses "Z" for UTC times.

//------------------------------------------------------------------------------------------------------------------


//Is GetEnvironmentVariable case-sensitive?

//This can be answered with a simple snippet:

Environment.GetEnvironmentVariable "ProgramFiles" = 
    Environment.GetEnvironmentVariable "PROGRAMFILES"
// answer => true

//The answer is therefore "not case-sensitive".

//------------------------------------------------------------------------------------------------------------------

//2. Use F# to test your own code interactively

(* 
    You are not restricted to playing with the .NET libraries, of course. 
    Sometimes it can be quite useful to test your own code.
*)

//To do this, just reference the DLL and then open the namespace as shown below.
//set the current directory to be same as the script directory
System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)


// pass in the relative path to the DLL
#r @"../MyApp/bin/Debug/net5.0/MyApp.dll"

// open the namespace
open MyApp

// do something
Class1().DoSomething()


//------------------------------------------------------------------------------------------------------------------

//3. Use F# to play with webservices interactively

(*
If you want to play with the WebAPI and Owin libraries, you don't need to create an executable 
-- you can do it through script alone!

There is a little bit of setup involved, as you will need a number of library DLLs to make this work.

So, assuming you have got the NuGet command line set up (see above), 
go to your script directory, and install the self hosting libraries via 
dotnet add package Microsoft.AspNet.WebApi.OwinSelfHost --package-directory Packages

Once these libraries are in place, you can use the code below as a skeleton for a simple WebAPI app.
*)

// sets the current directory to be same as the script directory
System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

// assumes nuget install Microsoft.AspNet.WebApi.OwinSelfHost has been run 
// so that assemblies are available under the current directory
#r "nuget: Microsoft.AspNet.WebApi.OwinSelfHost"
#r "System.Net.Http.dll"

open System
open Owin
open Microsoft.Owin
open System.Web.Http 
open System.Web.Http.Dispatcher
open System.Net.Http.Formatting

/// a record to return
[<CLIMutable>]
type Greeting = { Text : string }

/// A simple Controller
type GreetingController() =
    inherit ApiController()

    // GET api/greeting
    member this.Get()  =
        {Text="Hello!"}

/// Another Controller that parses URIs
type ValuesController() =
    inherit ApiController()

    // GET api/values 
    member this.Get()  =
        ["value1";"value2"]

    // GET api/values/5 
    member this.Get id = 
        sprintf "id is %i" id 

    // POST api/values 
    member this.Post ([<FromBody>]value:string) = 
        ()

    // PUT api/values/5 
    member this.Put(id:int, [<FromBody>]value:string) =
        ()

    // DELETE api/values/5 
    member this.Delete(id:int) =
        () 

/// A helper class to store routes, etc.
type ApiRoute = { id : RouteParameter }

