//What it does: Transforms a list of elevated values into an elevated value containing a list
//E<a> list -> E<a list> (or variants where list is replaced with other collection types)

/// parse an int and return a Result
/// string -> Result<int>
let parseInt str =
    match (System.Int32.TryParse str) with
        | true,i -> Ok i
        | false,_ -> Error [str + " is not an int"]

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

    let traverseResultUsingFoldbackA f list =

        let (<*>) = Result.apply
        let retn = Ok

        let prependToList head tail = head :: tail

        //right fold over the list
        let initTailState = retn []
        let folder head newTailState =
            retn prependToList <*> (f head) <*> newTailState

        List.foldBack folder list initTailState

    let traverseResultUsingFoldbackM f list =

        let (>>=) x f = Result.bind f x
        let retn = Ok

        let prependToList head tail = head::tail

        //right fold overt the list
        let initTailState = retn []
        let folder head newTailState =
            (f head) >>= (fun h ->
                newTailState >>= (fun ns ->
                retn (prependToList h ns)))

        List.foldBack folder list initTailState

    /// Transform a "list<Result>" into a "Result<list>"
    /// and collect the results using apply
    /// Result<'a> list -> Result<'a list>
    let sequenceResultA x = traverseResultUsingFoldbackA id x

    /// Transform a "list<Result>" into a "Result<list>"
    /// and collect the results using bind.
    /// Result<'a> list -> Result<'a list>
    let sequenceResultM x = traverseResultUsingFoldbackM id x


let goodSequenceA = ["1"; "2"; "3"]
                    |> List.map parseInt
                    |> List.sequenceResultA
                    // Ok [1; 2; 3]

let badSequenceA = ["1"; "x"; "y"]
                    |> List.map parseInt
                    |> List.sequenceResultA
                    // Error ["x is not an int"; "y is not an int"]

let goodSequenceM = ["1"; "2"; "3"]
                    |> List.map parseInt
                    |> List.sequenceResultM
                    // Ok [1; 2; 3]

let badSequenceM = ["1"; "x"; "y"]
                    |> List.map parseInt
                    |> List.sequenceResultM
                    // Failure ["x is not an int"]

