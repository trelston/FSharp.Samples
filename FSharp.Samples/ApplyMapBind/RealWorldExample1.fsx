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