//Example 1: We have a parseInt with signature string -> int option , and we have a
//list of strings. We want to parse all the strings at once. Now of course we can use map
//to convert the list of strings to a list of options. But what we really want is not a "list of
//options" but an "option of list", a list of parsed ints, wrapped in an option in case any fail.

//Example 2: We have a readCustomerFromDb function with signature CustomerId ->
//Result<Customer> , that will return Success if the record can be found and returned, and
//Failure otherwise. And say we have a list of CustomerId s and we want to read all the
//customers at once. Again, we can use map to convert the list of ids to a list of results.
//But what we really want is not a list of Result<Customer> , but a Result containing a
//Customer list , with the Failure case in case of errors.

//Example 3: We have a fetchWebPage function with signature Uri -> Async<string> ,
//that will return a task that will download the page contents on demand. And say we
//have a list of Uris s and we want to fetch all the pages at once. Again, we can use
//map to convert the list of Uri s to a list of Async s. But what we really want is not a list
//of Async , but a Async containing a list of strings.


module Option =
    
    let apply fOpt xOpt =
        match fOpt, xOpt with
        | Some f, Some x ->
            Some (f x)
        | _ ->
            None


let (<*>) = Option.apply
let retn  =  Some

let rec mapOption f list =
    let prependToList head tail = head :: tail
    match list with
    | [] ->
        retn []
    | head :: tail ->
        retn prependToList <*> (f head) <*> (mapOption f tail)

//["1","2"]
//(Some prependToList) <*> Some 1 <*> (Some prependToList <*> Some 2 <*> Some [])
//(Some prependToList) <*> Some 1 <*> Some [2]
//Some [1,2]

//["1", "x"]
//(Some prependToList) <*> Some 1 <*> (Some prependToList <*> None <*> Some [])
//(Some prependToList) <*> Some 1 <*> None
//None

module Result =

    let apply fResult xResult =
        match fResult,xResult with
        | Ok f, Ok x ->
            Ok (f x)
        | Ok f, Error errs ->
            Error errs
        | Error errs, Ok x ->
            Error errs
        | Error errs1, Error errs2 ->
            Error (List.concat [errs1; errs2])


let (<**>) = Result.apply
let retn1 = Ok

let rec mapResult f list =
    let prependToList head tail = head :: tail
    match list with
    | [] ->
        retn1 []
    | head::tail ->
        retn1 prependToList <**> (f head) <**> (mapResult f tail)

let parseInt str =
    match (System.Int32.TryParse str) with
    | true,i -> Ok i
    | false,_ -> Error [str + " is not an int"]
        
//["1","2"]
//(Ok prependToList) <*> Ok 1 <*> (Ok prependToList <*> Ok 2 <*> Ok [])
//(Ok prependToList) <*> Ok 1 <*> OK [2]
//Ok [1,2]

//["1", "x"]
//(Ok prependToList) <*> Ok 1 <*> (Ok prependToList <*> Error "x is not an int" <*> Ok [])
//(Ok prependToList) <*> Ok 1 <*> Error ["x is not an int"]
//Error ["x is not an int"]