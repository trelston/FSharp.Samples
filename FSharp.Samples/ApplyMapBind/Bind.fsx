//Allows you to compose world-crossing ("monadic") functions
//Signature: (a->E<b>) -> E<a> -> E<b> . 
//Alternatively with the parameters reversed: E<a> -> (a->E<b>) -> E<b>

//A function that parses a string to an int might return an Option<int>
//rather than a normal int, a function that reads lines from a file might return
//IEnumerable<string>, a function that fetches a web page might return Async<string> , and
//so on.

//These kinds of "world-crossing" functions are recognizable by their signature a -> E<b> ;
//their input is in the normal world but their output is in the elevated world.

//What "bind" does is transform a world-crossing function (a->E<b>) (commonly known as a "monadic function")
//into a lifted function E<a> -> E<b>.

//The benefit of doing this is that the resulting lifted functions live purely in the elevated world,
//and so can be combined easily by composition

//For example, a function of type a -> E<b> cannot be directly composed with a function of 
//type b -> E<c> , but after bind is used, the second function becomes of type E<b> ->
//E<c> , which can be composed.

//An alternative interpretation of bind is that it is a two parameter function that takes a
//elevated value ( E<a> ) and a "monadic function" ( a -> E<b> ), and returns a new elevated
//value ( E<b> ) generated by "unwrapping" the value inside the input, and running the function
//a -> E<b> against it. 

//Of course, the "unwrapping" metaphor does not work for every
//elevated world, but still it can often be useful to think of it this way.

//The bind function for options
let bindOption f xOpt =
    match xOpt with
    | Some x -> f x
    | _ -> None


let bindList (f: 'a -> 'b list) (xList: 'a list) =
    [for x in xList do
        for y in f x do
        yield y]

//The functions already exist in F#, called
//Option.bind and List.collect

//Infix version of bind
