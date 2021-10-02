//Downloading and processing a list of websites

//Given a list of websites, create an action that finds the site with the largest home page.


//define a millisecond unit of measure
type [<Measure>] ms

//Custom implementation of WebClient with settable timeout
type WebClientWithTimeout(timeout:int<ms>) =
    inherit System.Net.WebClient()

    override this.GetWebRequest(address) =
        let result = base.GetWebRequest(address)
        result.Timeout <- int timeout
        result

//The content of a downloaded page
type UriContent = UriContent of System.Uri * string

//The content size of a downloaded page
type UriContentSize = UriContentSize of System.Uri * int

/// Get the contents of the page at the given Uri
/// Uri -> Async<Result<UriContent>>
let getUriContent (uri:System.Uri) =
    async {
        use client = new WebClientWithTimeout(1000<ms>) // 1 sec timeout
        try
            printfn " [%s] Started ..." uri.Host
            let! html = client.AsyncDownloadString(uri)
            printfn " [%s] ... finished" uri.Host
            let uriContent = UriContent (uri, html)
            return (Ok uriContent)
        with
        | ex ->
            printfn " [%s] ... exception" uri.Host
            let err = sprintf "[%s] %A" uri.Host ex.Message
            return Error [err]
    }

let showContentResult result =
    match result with
    | Ok (UriContent (uri, html)) ->
        printfn "SUCCESS: [%s] First 100 chars: %s" uri.Host (html.Substring(0,100))
    | Error errs ->
        printfn "FAILURE: %A" errs

System.Uri ("http://google.com")
|> getUriContent
|> Async.RunSynchronously
|> showContentResult

System.Uri ("http://example.bad")
|> getUriContent
|> Async.RunSynchronously
|> showContentResult

module Async =

    let map f xAsync = async {
     
        // get the contents of xAsync
        let! x = xAsync

        // apply the function and lift the result
        return f x
    }

    let retn x = async {
        // lift x to Async
        return x
    }

    let apply fAsync xAsync = async {
        
        // start the two asyncs in parallel
        let! fChild = Async.StartChild fAsync
        let! xChild = Async.StartChild xAsync

        // wait for the results
        let! f = fChild
        let! x = xChild

        // apply the function to the results
        return f x     
    }

    let bind f xAsync = async {
        
        // get the contents of xAsync
        let! x = xAsync

        // apply the function but don't lift the result
        // a f will return Async
        return! f x
    }


/// Make a UriContentSize from a UriContent
/// UriContent -> Result<UriContentSize>
let makeContentSize (UriContent (uri, html)) =
    if System.String.IsNullOrEmpty(html) then
        Error ["empty page"]
    else
        let uriContentSize = UriContentSize (uri, html.Length)
        Ok uriContentSize

// The problem is that the outputs and inputs don't match:
// getUriContent is Uri -> Async<Result<UriContent>>
// makeContentSize is UriContent -> Result<UriContentSize>

// First, use Result.bind to convert it from an a -> Result<b> function to a Result<a> -> Result<b> function.

// Next, use Async.map to convert it from an a -> b function to a Async<a> -> Async<b> function.

/// Get the size of the contents of the page at the given Uri
/// Uri -> Async<Result<UriContentSize>>
let getUriContentSize uri =
    getUriContent uri
    |> Async.map (Result.bind makeContentSize)


let showContentSizeResult result =
    match result with
    | Ok (UriContentSize (uri, len)) ->
        printfn "SUCCESS: [%s] Content size is %i" uri.Host len
    | Error errs ->
        printfn "FAILURE: %A" errs

System.Uri ("http://google.com")
|> getUriContentSize
|> Async.RunSynchronously
|> showContentSizeResult

System.Uri ("http://example.bad")
|> getUriContentSize
|> Async.RunSynchronously
|> showContentSizeResult

//The last step in the process is to find the largest page size.
//That's easy. Once we have a list of UriContentSize , we can easily find the largest one using
//List.maxBy :

/// Get the largest UriContentSize from a list
/// UriContentSize list -> UriContentSize
let maxContentSize list =
    // extract the len field from a UriContentSize
    let contentSize (UriContentSize (_, len)) = len
    // use maxBy to find the largest
    list |> List.maxBy contentSize


module Result =

    let apply fResult xResult =
        match fResult,xResult with
        | Ok f, Ok x ->
            Ok (f x)
        | Error errs, Ok x ->
            Error errs
        | Ok f, Error errs ->
            Error errs
        | Error errs1, Error errs2 ->
            // concat both lists of errors
            Error (List.concat [errs1; errs2])

module List =
    
    /// Map a Async producing function over a list to get a new Async
    /// using applicative style
    /// ('a -> Async<'b>) -> 'a list -> Async<'b list>
    let rec traverseAsyncA f list =
        
        // define the applicative functions
        let (<*>) = Async.apply
        let retn = Async.retn
        
        // define a "cons" function
        let cons head tail = head :: tail
        
        // right fold over the list
        let initState = retn []
        let folder head tail =
            retn cons <*> (f head) <*> tail
        
        List.foldBack folder list initState
    
    /// Transform a "list<Async>" into a "Async<list>"
    /// and collect the results using apply.
    let sequenceAsyncA x = traverseAsyncA id x
    
    /// Map a Result producing function over a list to get a new Result
    /// using applicative style
    /// ('a -> Result<'b>) -> 'a list -> Result<'b list>
    let rec traverseResultA f list =
        
        // define the applicative functions
        let (<*>) = Result.apply
        let retn = Ok
        
        // define a "cons" function
        let cons head tail = head :: tail
    
        // right fold over the list
        let initState = retn []
        let folder head tail =
            retn cons <*> (f head) <*> tail
    
        List.foldBack folder list initState
    
    /// Transform a "list<Result>" into a "Result<list>"
    /// and collect the results using apply.
    let sequenceResultA x = traverseResultA id x

    /// Map a Async producing function over a list to get a new Async
    /// using monadic style
    /// ('a -> Async<'b>) -> 'a list -> Async<'b list>
    let rec traverseAsyncM f list =
        
        // define the applicative functions
        let (>>=) x f = Async.bind f x
        let retn = Async.retn
        
        // define a "cons" function
        let prependToList head tail = head :: tail
        
        // right fold over the list
        let initState = retn []
        let folder head newTailState =
           (f head) >>= (fun h ->
                newTailState >>= (fun nst ->
                        retn (prependToList h nst)))
                    
        List.foldBack folder list initState
    
    /// Transform a "list<Async>" into a "Async<list>"
    /// and collect the results using apply.
    let sequenceAsyncM x = traverseAsyncM id x
    
    /// Map a Result producing function over a list to get a new Result
    /// using applicative style
    /// ('a -> Result<'b>) -> 'a list -> Result<'b list>
    let rec traverseResultM f list =
        
        // define the applicative functions
        let (>>=) x f = Result.bind f x
        let retn = Ok
        
        // define a "cons" function
        let prependToList head tail = head :: tail
    
        // right fold over the list
        let initState = retn []
        let folder head newTailState =
            (f head) >>= (fun h ->
                newTailState >>= (fun nst ->
                        retn (prependToList h nst)))
    
        List.foldBack folder list initState
    
    /// Transform a "list<Result>" into a "Result<list>"
    /// and collect the results using apply.
    let sequenceResultM x = traverseResultM id x


/// Get the largest page size from a list of websites
let largestPageSizeA urls =
    urls
    // turn the list of strings into a list of Uris
    // (In F# v4, we can call System.Uri directly!)
    |> List.map (fun s -> System.Uri(s))

    // turn the list of Uris into a "Async<Result<UriContentSize>> list"
    |> List.map getUriContentSize

    // turn the "Async<Result<UriContentSize>> list"
    // into an "Async<Result<UriContentSize> list>"
    |> List.sequenceAsyncA

    // turn the "Async<Result<UriContentSize> list>"
    // into a "Async<Result<UriContentSize list>>"
    |> Async.map List.sequenceResultA

    // find the largest in the inner list to get
    // a "Async<Result<UriContentSize>>"
    |> Async.map (Result.map maxContentSize)


//It will be interesting to see how long the download takes for different scenarios, so let's
//create a little timer that runs a function a certain number of times and takes the average:

/// Do countN repetitions of the function f and print the time per run
let time countN label f =
    
    let stopwatch = System.Diagnostics.Stopwatch()

    // do a full GC at the start but not thereafter
    // allow garbage to collect for each iteration
    System.GC.Collect()

    printfn "======================="
    printfn "%s" label
    printfn "======================="

    let mutable totalMs = 0L

    for iteration in [1..countN] do
        stopwatch.Restart()
        f()
        stopwatch.Stop()
        printfn "#%2i elapsed:%6ims " iteration stopwatch.ElapsedMilliseconds
        totalMs <- totalMs + stopwatch.ElapsedMilliseconds

    let avgTimePerRun = totalMs / int64 countN
    printfn "%s: Average time per run:%6ims " label avgTimePerRun

// We'll define two lists of sites: a "good" one, where all the sites should be accessible, and a
// "bad" one, containing invalid sites.

let goodSites = [
    "http://google.com"
    "http://bbc.co.uk"
    "http://fsharp.org"
    "http://microsoft.com"
    ]

let badSites = [
    "http://example.com/nopage"
    "http://bad.example.com"
    "http://verybad.example.com"
    "http://veryverybad.example.com"
    ]

//Let's start by running largestPageSizeA 10 times with the good sites list:
let f() =
    largestPageSizeA goodSites
    |> Async.RunSynchronously
    |> showContentSizeResult
time 10 "largestPageSizeA_Good" f

let f1() =
    largestPageSizeA badSites
    |> Async.RunSynchronously
    |> showContentSizeResult
time 10 "largestPageSizeA_Bad" f1


// The largestPageSizeA has a series of maps and sequences in it which means that the list is
// being iterated over three times and the async mapped over twice.

(*
    
    Here's the original version, with comments removed:
    let largestPageSizeA urls =
        urls
        |> List.map (fun s -> System.Uri(s))
        |> List.map getUriContentSize
        |> List.sequenceAsyncA
        |> Async.map List.sequenceResultA
        |> Async.map (Result.map maxContentSize)

    //The first two List.map s could be combined:
    let largestPageSizeA urls =
        urls
        |> List.map (fun s -> System.Uri(s) |> getUriContentSize)
        |> List.sequenceAsyncA
        |> Async.map List.sequenceResultA
        |> Async.map (Result.map maxContentSize)

    The map-sequence can be replaced with a traverse :
    let largestPageSizeA urls =
        urls
        |> List.traverseAsyncA (fun s -> System.Uri(s) |> getUriContentSize)
        |> Async.map List.sequenceResultA
        |> Async.map (Result.map maxContentSize)

    and finally the two Async.map s can be combined too:
    let largestPageSizeA urls =
        urls
        |> List.traverseAsyncA (fun s -> System.Uri(s) |> getUriContentSize)
        |> Async.map (List.sequenceResultA >> Result.map maxContentSize)

    Personally, I think we've gone too far here. I prefer the original version to this one!
    As an aside, one way to get the best of both worlds is to use a "streams" library that
    automatically merges the maps for you.
    In F#, a good one is Nessos Streams. Here is a blog
    post showing the difference between streams and the standard seq .
    http://trelford.com/blog/post/SeqVsStream.aspx
    https://nessos.github.io/Streams/
*)

let largestPageSizeAOptimized urls =
    urls
    |> List.traverseAsyncA (fun s -> System.Uri(s) |> getUriContentSize)
    |> Async.map (List.sequenceResultA >> Result.map maxContentSize)

let largestPageSizeM urls =
    urls
    |> List.map (fun s -> System.Uri(s))
    |> List.map getUriContentSize
    |> List.sequenceAsyncM // <= "M" version
    |> Async.map List.sequenceResultM // <= "M" version
    |> Async.map (Result.map maxContentSize)

let f2() =
    largestPageSizeM goodSites
    |> Async.RunSynchronously
    |> showContentSizeResult
time 10 "largestPageSizeM_Good" f2

(*
    There is a big difference now -- it is obvious that the downloads are happening in series --
    each one starts only when the previous one has finished.

    As a result, the average time is 955ms per run, almost twice that of the applicative version.

    Now what about if some of the sites are bad? What should we expect? Well, because it's
    monadic, we should expect that after the first error, the remaining sites are skipped, right?
    Let's see if that happens!
*)

let f3() =
    largestPageSizeM badSites
    |> Async.RunSynchronously
    |> showContentSizeResult
time 10 "largestPageSizeM_Bad" f3

(*
    
    Well that was unexpected! All of the sites were visited in series, even though the first one
    had an error. But in that case, why is only the first error returned, rather than all the the
    errors?

    Can you see what went wrong?

    The reason why the implementation did not work as expected is that the chaining of the
    Async s was independent of the chaining of the Result s.

    If you step through this in a debugger you can see what is happening:
    - The first Async in the list was run, resulting in a failure.
    - Async.bind was used with the next Async in the list. But Async.bind has no concept
        of error, so the next Async was run, producing another failure.
    - In this way, all the Async s were run, producing a list of failures.
    - This list of failures was then traversed using Result.bind . Of course, because of the
        bind, only the first one was processed and the rest ignored.
    - The final result was that all the Async s were run but only the first failure was returned.


    The fundamental problem is that we are treating the Async list and Result list as separate
    things to be traversed over.
    But that means that a failed Result has no influence on
    whether the next Async is run.

    And in order to do that, we need to treat the Async and the Result as a single type -- let's
    imaginatively call it AsyncResult .

    OK, let's define the AsyncResult type and it's associated map , return , apply and bind
    functions.

*)

/// type alias (optional)
type AsyncResult<'a, 'b> = Async<Result<'a, 'b>>

/// functions for AsyncResult
module AsyncResult =

    let map f =
        f |> Result.map |> Async.map

    let retn x =
        x |> Ok |> Async.retn

    let apply fAsyncResult xAsyncResult =
        fAsyncResult |> Async.bind (fun fResult ->
        xAsyncResult |> Async.map (fun xResult ->
        Result.apply fResult xResult))

    let bind f xAsyncResult = async {
        let! xResult = xAsyncResult
        match xResult with
        | Ok x -> return! f x
        | Error err -> return (Error err)
    }

    (*
        
        - The type alias is optional. We can use Async<Result<'a>> directly in the code and it wil
            work fine. The point is that conceptually AsyncResult is a separate type.
        - The bind implementation is new. The continuation function f is now crossing two
            worlds, and has the signature 'a -> Async<Result<'b>>.
            - If the inner Result is successful, the continuation function f is evaluated with the
               result. The return! syntax means that the return value is already lifted.
            -  If the inner Result is a failure, we have to lift the failure to an Async.
    *)


//With bind and return in place, we can create the appropriate traverse and sequence
//functions for AsyncResult :

module List =
    /// Map an AsyncResult producing function over a list to get a new AsyncResult
    /// using monadic style
    /// ('a -> AsyncResult<'b>) -> 'a list -> AsyncResult<'b list>
    let rec traverseAsyncResultM f list =
        // define the monadic functions
        let (>>=) x f = AsyncResult.bind f x
        let retn = AsyncResult.retn
        
        // define a "cons" function
        let cons head tail = head :: tail
        
        // right fold over the list
        let initState = retn []
        let folder head tail =
            f head >>= (fun h ->
                tail >>= (fun t ->
                    retn (cons h t) ))
        
        List.foldBack folder list initState
        /// Transform a "list<AsyncResult>" into a "AsyncResult<list>"
        /// and collect the results using bind.
    let sequenceAsyncResultM x = traverseAsyncResultM id x

//Finally, the largestPageSize function is simpler now, with only one sequence needed.
let largestPageSizeM_AR urls =
    urls
    |> List.map (fun s -> System.Uri(s) |> getUriContentSize)
    |> List.sequenceAsyncResultM
    |> AsyncResult.map maxContentSize

let f3() =
    largestPageSizeM_AR goodSites
    |> Async.RunSynchronously
    |> showContentSizeResult
time 10 "largestPageSizeM_AR_Good" f3

    //And now the moment we've been waiting for! Will it skip the downloading after the first bad
    //site?

let f4() =
    largestPageSizeM_AR badSites
    |> Async.RunSynchronously
    |> showContentSizeResult
time 10 "largestPageSizeM_AR_Bad" f4

//Success! The error from the first bad site prevented the rest of the downloads, and the short
//run time is proof of that.