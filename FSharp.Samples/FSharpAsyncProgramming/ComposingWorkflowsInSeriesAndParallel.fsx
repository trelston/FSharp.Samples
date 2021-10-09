(*
    Another useful thing about async workflows is that they can be easily combined in various ways:
        both in series and in parallel.
    
    Let's again create a simple workflow that just sleeps for a given time:
*)

// create a workflow to sleep for a time
let sleepWorkflowMs ms = async {
    printfn "%i ms workflow started" ms
    do! Async.Sleep ms
    printfn "%i ms workflow finished" ms
    }
(*
    Here's a version that combines two of these in series:
*)
let workflowInSeries = async {
    let! sleep1 = sleepWorkflowMs 1000
    printfn "Finished one" 
    let! sleep2 = sleepWorkflowMs 2000
    printfn "Finished two" 
    }

#time
Async.RunSynchronously workflowInSeries 
#time

(*
    And here's a version that combines two of these in parallel:
*)

let workflowInParallel =
    // Create them
    let sleep1 = sleepWorkflowMs 1000
    let sleep2 = sleepWorkflowMs 2000
    
    // run them in parallel
    
    [sleep1; sleep2] 
        |> Async.Parallel
        |> Async.RunSynchronously 
    
    
#time
workflowInParallel
#time
    
    