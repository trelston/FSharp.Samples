(*
    Asynchronous workflows can also be created manually. A new workflow is created using the async keyword and curly braces. The braces contain a set of expressions to be executed in the background.
    This simple workflow just sleeps for 2 seconds.
*)
open System
let sleepWorkflow  = async{
    printfn "Starting sleep workflow at %O" DateTime.Now.TimeOfDay
    do! Async.Sleep 2000
    printfn "Finished sleep workflow at %O" DateTime.Now.TimeOfDay
    }

Async.RunSynchronously sleepWorkflow

(*
Note: the code do! Async.Sleep 2000 is similar to Thread.Sleep but designed to work with asynchronous workflows.
*)

(*
    Workflows can contain other async workflows nested inside them.
    Within the braces, the nested workflows can be blocked on by using the let! syntax.
*)

let nestedWorkflow = async {
    
    printfn "Starting parent"
    let! childWorkflow = Async.StartChild sleepWorkflow
    
    // give the child a chance and then keep working
    do! Async.Sleep 100
    printfn "Doing something useful while waiting "
    
    // block on the child
    let! result = childWorkflow

    // done
    printfn "Finished parent" 
}

// run the whole workflow
Async.RunSynchronously nestedWorkflow

