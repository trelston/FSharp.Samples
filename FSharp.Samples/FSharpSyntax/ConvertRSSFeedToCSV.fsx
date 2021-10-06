(*
    Here's a little script that uses the Xml type provider to parse an RSS feed (in this case, F# questions on StackOverflow)
    and convert it to a CSV file for later analysis.
    Note that the RSS parsing code is just one line of code! Most of the code is concerned with writing the CSV.
    Yes, I could have used a CSV library (there are lots on NuGet) but I thought I'd leave it as is to show you how simple it is.
*)

// sets the current directory to be same as the script directory
System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

#r @"..\..\packages\FSharp.Data\lib\netstandard2.0\FSharp.Data.dll"
#r "System.Xml.Linq.dll"
open FSharp.Data

type Rss = XmlProvider<"http://stackoverflow.com/feeds/tag/f%23">

// prepare a string for writing to CSV            
let prepareStr obj =
    obj.ToString()
     .Replace("\"","\"\"") // replace single with double quotes
     |> sprintf "\"%s\""   // surround with quotes
     
// convert a list of strings to a CSV
let listToCsv list =
    let combine s1 s2 = s1 + "," + s2
    list 
    |> Seq.map prepareStr 
    |> Seq.reduce combine 

// extract fields from Entry
let extractFields (entry:Rss.Entry) = 
    [entry.Title.Value; 
     entry.Author.Name;     
     entry.Published.ToString()]
    
// write the lines to a file
do 
    use writer = new System.IO.StreamWriter("fsharp-questions.csv")
    let feed = Rss.GetSample()
    feed.Entries
    |> Seq.map (extractFields >> listToCsv)
    |> Seq.iter writer.WriteLine
    // writer will be closed automatically at the end of this scope
    
