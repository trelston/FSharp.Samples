(*
    F# has another useful feature called "object expressions".
    This is the ability to directly create objects from an interface or abstract class without having to
    define a concrete class first. 
*)

(*
    In the example below, we create some objects that implement IDisposable using a makeResource helper function.
*)

// create a new object that implements IDisposable
let makeResource name = 
   { new System.IDisposable 
     with member this.Dispose() = printfn "%s disposed" name }

let useAndDisposeResources = 
    use r1 = makeResource "first resource"
    printfn "using first resource" 
    for i in [1..3] do
        let resourceName = sprintf "\tinner resource %d" i
        use temp = makeResource resourceName 
        printfn "\tdo something with %s" resourceName 
    use r2 = makeResource "second resource"
    printfn "using second resource" 
    printfn "done."
    
(*
    The example also demonstrates how the "use" keyword automatically disposes a resource when it goes out of scope.
*)