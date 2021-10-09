(*

In this more realistic example, we'll see how easy it is to convert some existing code
from a non-asynchronous style to an asynchronous style, and the corresponding performance increase that can be achieved.

*)

(*
    So here is a simple URL downloader, very similar to the one we saw at the start of the series:
*)

open System.Net
open System
open System.IO
open System.Net.Http


let fetchUrl url =
    let req =  WebRequest.Create(Uri(url)) 
    use resp = req.GetResponse()
    use stream = resp.GetResponseStream() 
    use reader = new IO.StreamReader(stream) 
    let html = reader.ReadToEnd() 
    printfn "finished downloading %s" url

// a list of sites to fetch
let sites = ["http://www.bing.com";
             "http://www.google.com";
             "http://www.microsoft.com";
             "http://www.amazon.com";
             "http://www.yahoo.com"]

#time                     // turn interactive timer on
sites                     // start with the list of sites
|> List.map fetchUrl      // loop through each site and download
#time                     // turn timer off


// Fetch the contents of a web page asynchronously
let fetchUrlAsync url =        
    async {                             
        let req = WebRequest.Create(Uri(url)) 
        use! resp = req.AsyncGetResponse()  // new keyword "use!"  
        use stream = resp.GetResponseStream() 
        use reader = new IO.StreamReader(stream) 
        let html = reader.ReadToEnd() 
        printfn "finished downloading %s" url 
        }
    
(*

    - The change from "use resp =" to "use! resp =" is exactly the change that we talked about above
        -- while the async operation is going on, let other tasks have a turn.
    - We also used the extension method AsyncGetResponse defined in the CommonExtensions namespace.
        This returns an async workflow that we can nest inside the main workflow.
    - In addition the whole set of steps is contained in the "async {...}" wrapper which turns it into a block
        that can be run asynchronously.

*)

// a list of sites to fetch
let sites1 = ["http://www.bing.com";
             "http://www.google.com";
             "http://www.microsoft.com";
             "http://www.amazon.com";
             "http://www.yahoo.com"]

#time                      // turn interactive timer on
sites1 
|> List.map fetchUrlAsync  // make a list of async tasks
|> Async.Parallel          // set up the tasks to run in parallel
|> Async.RunSynchronously  // start them off
#time                      // turn timer off

let fetchUrlAsync1 url =        
    async {                             
        let! resp = Async.AwaitTask ((new HttpClient()).GetAsync(Uri(url)))
        use stream = resp.Content.ReadAsStream() 
        use reader = new IO.StreamReader(stream) 
        let html = reader.ReadToEnd() 
        printfn $"finished downloading %s{url}" 
    }
    
#time                      // turn interactive timer on
sites1 
|> List.map fetchUrlAsync  // make a list of async tasks
|> Async.Parallel          // set up the tasks to run in parallel
|> Async.RunSynchronously  // start them off
#time    