(*
    F# can directly use all the usual .NET suspects, such as Thread AutoResetEvent, BackgroundWorker and IAsyncResult. 
*)

open System

let userTimerWithCallback =
        //create an event to wait on
        let event = new System.Threading.AutoResetEvent(false)
        
        //create a timer and add an event handler that will signal the event
        let timer = new System.Timers.Timer(2000.0)
        timer.Elapsed.Add(fun _ -> event.Set() |> ignore)
        
        //start
        printfn "Waiting for timer at %O" DateTime.Now.TimeOfDay
        timer.Start()

        // keep working
        printfn "Doing something useful while waiting for event"

        // block on the timer via the AutoResetEvent
        event.WaitOne() |> ignore

        //done
        printfn "Timer ticked at %O" DateTime.Now.TimeOfDay
        
(*
    This shows the use of AutoResetEvent as a synchronization mechanism.

    A lambda is registered with the Timer.Elapsed event, and when the event is triggered, the AutoResetEvent is signalled.
    The main thread starts the timer, does something else while waiting, and then blocks until the event is triggered.
    Finally, the main thread continues, about 2 seconds later.

    The code above is reasonably straightforward, but does require you to instantiate an AutoResetEvent,
    and could be buggy if the lambda is defined incorrectly.

*)