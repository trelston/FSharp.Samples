//Transforms a world-crossing function into a world-crossing function that works
//with collections

//(a->E<b>) -> a list -> E<b list>

//It turns out that traverse can be implemented in an applicative style or a monadic style, so
//there are often two separate implementations to choose from. The applicative versions tend
//to end in A and the monadic versions end in M

//NOTE: The following implementations are for demonstration only! Neither of these implementations
//are tail-recursive, and so they will fail on large lists!


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

    //Map a Result producing function over a list to get a new Result using applicative style
    //('a -> Result<'b>) -> 'a list -> Result<'b list>

    let rec traverseResultA f list =
        // define the applicative functions
        let (<*>) = Result.apply
        let retn = Ok

        // define a "cons" function
        let prependToList head tail = head :: tail
        
        // loop through the list
        match list with
        | [] ->
            // if empty, lift [] to a Result
             retn []
        | head::tail ->
            // otherwise lift the head to a Result using f
            // and cons it with the lifted version of the remaining list
            retn prependToList <*> (f head) <*> (traverseResultA f tail)

    //Map a Result producing function over a list to get a new Result using Monadic style
    //('a -> Result<'b>) -> 'a list -> Result<'b list>
    let rec traverseResultM f list =
        
        //define the monadic functions
        let (>>=) x f = Result.bind f x
        let retn = Ok

        //define a "prepend" function
        let prependToList head tail = head :: tail

        //loop through the list
        match list with
        | [] ->
            // if empty, lift [] to a Result
            retn []
        | head::tail ->
            //otherwise lift the head to a Result using f
            //then lift the tail to a Result using travers
            //then cons the head and tail and return it
            (f head) >>= (fun h -> 
            traverseResultM f tail >>=(fun t ->
            retn (prependToList h t)))

//The applicative version returns all the errors, while the monadic version returns only the first
//error.

//Implementing traverse using fold
//

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

// pass in strings wrapped in a List
// (applicative version)
let goodA = ["1"; "2"; "3"] |> List.traverseResultUsingFoldbackA parseInt
// get back a Result containing a list of ints
// Success [1; 2; 3]
// OK prependToList <*> parseInt "3" <*> Ok []
// OK prependToList <*> parseInt "2" <*> Ok [3]
// OK prependToList <*> parseInt "1" <*> OK [2,3]
// OK [1,2,3]

// pass in strings wrapped in a List
// (applicative version)
let badA = ["1"; "x"; "y"] |> List.traverseResultUsingFoldbackA parseInt
// get back a Result containing a list of ints
// Failure ["x is not an int"; "y is not an int"]
// OK prependToList <*> parseInt "y" <*> Ok []
// OK prependToList <*> parseInt "x" <*> Error ["y is not an int"]
// OK prependToList <*> parseInt "1" <*> Error ["x is not an int"; "y is not an int"]
//Error ["x is not an int"; "y is not an int"]

// pass in strings wrapped in a List
// (applicative version)
let goodM = ["1"; "2"; "3"] |> List.traverseResultUsingFoldbackM parseInt
// get back a Result containing a list of ints
// Success [1; 2; 3]
// parseInt "3" >>= 3 -> [] >>= [] -> Ok (prependToList 3 [])
// parseInt "2" >>= 2 -> [3] >>= [3] -> Ok (prependToList 2 [3])
// parseInt "1" >>= 1 -> [2,3] >>= [2,3] -> Ok (prependToList 1 [2,3])
// Ok [1,2,3]

// OK [1,2,3]

// pass in strings wrapped in a List
// (applicative version)
let badM = ["1"; "x"; "3"] |> List.traverseResultUsingFoldbackM parseInt
// get back a Result containing a list of ints
// Failure ["x is not an int"; "y is not an int"]
// parseInt "3" >>= 3 -> [] >>= [] -> Ok (prependToList 3 [])
// parseInt "x"
// Error ["x is not an int"]



module Option =

    /// Map a Result producing function over an Option to get a new Result
    /// ('a -> Result<'b>) -> 'a option -> Result<'b option>
    let traverseResultA f opt =
        
        // define the applicative functions
        let (<*>) = Result.apply
        let retn = Ok

        // loop through the option
        match opt with
        | None ->
            // if empty, lift None to an Result
            retn None
        | Some x ->
            // lift value to an Result
            retn Some <*> (f x)

// pass in an string wrapped in an Option
let good = Some "1" |> Option.traverseResultA parseInt
// get back a Result containing an Option
// Ok (Some 1)

// pass in an string wrapped in an Option
let bad = Some "x" |> Option.traverseResultA parseInt
// get back a Result containing an Option
// Error ["x is not an int"]

// pass in an string wrapped in an Option
let goodNone = None |> Option.traverseResultA parseInt
// get back a Result containing an Option
// Ok (None)