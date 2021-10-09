(*
One very convenient thing about async workflows is that they support a built-in cancellation mechanism.
No special code is needed.

Consider a simple task that prints numbers from 1 to 100:
*)

let testLoop = async {
    for i in [1..100] do
        // do something
        printf "%i before.." i

        // sleep a bit 
        do! Async.Sleep 10  
        printfn "..after"
    }

(*
Now let's say we want to cancel this task half way through. What would be the best way of doing it?

In C#, we would have to create flags to pass in and then check them frequently, but in F# this technique is built in,
using the CancellationToken class.

Here an example of how we might cancel the task:
*)

open System
open System.Threading

// create a cancellation source
let cancellationSource = new CancellationTokenSource()

// start the task, but this time pass in a cancellation token
Async.Start (testLoop,cancellationSource.Token)

// wait a bit
Thread.Sleep(200)  

// cancel after 200ms
cancellationSource.Cancel()

(*
In F#, any nested async call will check the cancellation token automatically!

In this case it was the line:

do! Async.Sleep(10)

As you can see from the output, this line is where the cancellation happened.
*)