//Using the apply function as it stands can be awkward, so it is common to create an infix
//version, typically called <*> .

open Extensions

module Option =

    //The apply function for Options
    let apply fOpt xOpt =
        match fOpt,xOpt with
        | Some f, Some x ->
            Some (f x)
        | _ -> None

module List =

    //The apply function for Lists
    let apply (fList: ('a -> 'b) list) (xList: 'a list) =
        [ for f in fList do
          for x in xList do
          yield f x]

let add x = x + 1

let resultOption =
    let (<*>) = Option.apply
    (Some add) <*> (Some 2) <*> (Some 3)
// resultOption = Some 5

let resultList =
    let (<*>) = List.apply
    [add] <*> [1;2] <*> [10;20]
// resultList = [11; 21; 12; 22]

//To construct a lifted function from a normal function, just use return on
//the normal function and then apply. This gives you the same result as if you had simply
//done map in the first place. See below:-
let add1IfSomething = Option.map add
let add2IfSOmething = Option.apply (Some add)

//The initial return then apply can be replaced with map, and we so typically create an infix operator for map as
//well, such as <!> in F#.

let resultOption2 =
    let (<!>) = Option.map
    let (<*>) = Option.apply

    add <!> (Some 2) <*> (Some 2)

let resultList2 =
    let (<!>) = List.map
    let (<*>) = List.apply

    add <!> [1;2] <*> [10;20]

let batman =
    let (<!>) = List.map
    let (<*>) = List.apply
    let andthen = (|>)

    //string concatenation using +
    (+) <!> ["bam"; "kapow"; "zap"] <*> ["!"; "!!"]


    


