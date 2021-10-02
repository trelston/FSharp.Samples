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


