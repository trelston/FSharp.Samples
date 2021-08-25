let tuples = [Some (1,2); Some (3,4); None; Some (7,8);]
// List<Option<Tuple<int>>>
// Traversable -> Traversable -> Applicative

let desiredOutput = [Some 1; Some 3; None; Some 7],[Some 2; Some 4; None; Some 8]
// Tuple<List<Option<int>>>
// Applicative -> Traversable(list) -> Traversable(Option)

//optionSequenceTuple will move option down
//listSequenceTuple will move list down

//First, the tuple is playing the role of the applicative, so we need to define the apply and
//return functions
let tupleReturn x = (x, x)
let tupleApply (f,g) (x,y) = (f x, g y)

let listSequenceTuple list =
    // define the applicative functions
    let (<*>) = tupleApply
    let retn = tupleReturn
    // define a "cons" function
    let prependToList head tail = head :: tail
    // right fold over the list
    let initState = retn []
    let folder head newTailState = retn prependToList <*> head <*> newTailState

    List.foldBack folder list initState

[ (1,2); (3,4)] |> listSequenceTuple
// Result => ([1; 3], [2; 4])

let optionSequenceTuple opt =
    // define the applicative functions
    let (<*>) = tupleApply
    let retn = tupleReturn

    // right fold over the option
    let initState = retn None
    let folder x _ = (retn Some) <*> x

    Option.foldBack folder opt initState

Some (1,2) |> optionSequenceTuple
// Result => (Some 1, Some 2)

let convert input =
    input
    // from List<Option<Tuple<int>>> to List<Tuple<Option<int>>>
    |> List.map optionSequenceTuple
    // from List<Tuple<Option<int>>> to Tuple<List<Option<int>>>
    |> listSequenceTuple

let output = convert tuples
// ( [Some 1; Some 3; None; Some 7], [Some 2; Some 4; None; Some 8] )
output = desiredOutput |> printfn "Is output correct? %b"
// Is output correct? true
