// Requires FSharp.Data under script directory 
//    nuget install FSharp.Data -o Packages -ExcludeVersion  
#r @"..\..\packages\FSharp.Data\lib\netstandard2.0\FSharp.Data.dll"

(*
This script checks that a website is responding with a 200. 
This might be useful as the basis for a post-deployment smoke test, for example.
*)
open FSharp.Data

let queryServer uri queryParams = 
    try
        let response = Http.Request(uri, query=queryParams, silentHttpErrors = true)
        Some response 
    with
    | :? System.Net.WebException as ex -> None

let sendAlert uri message =
        // send alert via email
        printfn "Error for %s. Message=%O" uri message
        
let checkServer (uri,queryParams) = 
    match queryServer uri queryParams with
    | Some response -> 
        printfn "Response for %s is %O" uri response.StatusCode 
        if (response.StatusCode <> 200) then
            sendAlert uri response.StatusCode 
    | None -> 
        sendAlert uri "No response"

// test the sites    
let google = "http://google.com", ["q","fsharp"]
let bad = "http://example.bad", []

[google;bad]
|> List.iter checkServer
