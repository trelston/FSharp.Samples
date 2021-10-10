(*
A common scenario is that you have some state that needs to be accessed and changed by multiple concurrent tasks or threads.
We'll use a very simple case, and say that the requirements are:

    - A shared "counter" and "sum" that can be incremented by multiple tasks concurrently.
    - Changes to the counter and sum must be atomic -- we must guarantee that they will both be updated at the same time.

*)

type MessageBasedCounter() =
    
    static let updateState (count,sum) msg =
        
        //increment the counters and
        let newSum = sum + msg
        let newCount = count + 1
        printfn $"Count is: %i{newCount}. Sum is: %i{newSum}" 
        
        // ...emulate a short delay
        //Utility.RandomSleep()

        // return the new state
        (newCount,newSum)
        
    // create the agent
    static let agent = MailboxProcessor.Start(fun inbox -> 

        // the message processing function
        let rec messageLoop oldState = async{

            // read a message
            let! msg = inbox.Receive()

            // do the core logic
            let newState = updateState oldState msg

            // loop to top
            return! messageLoop newState 
            }

        // start the loop 
        messageLoop (0,0)
        )

    // public interface to hide the implementation
    static member Add i = agent.Post i
    
    // test in isolation
MessageBasedCounter.Add 4
MessageBasedCounter.Add 5

let makeCountingTask addFunction taskId  = async {
    let name = $"Task%i{taskId}"
    for i in [1..3] do 
        addFunction i
    }

let task = makeCountingTask MessageBasedCounter.Add 1
Async.RunSynchronously task

let messageExample5 = 
    [1..5]
        |> List.map (fun i -> makeCountingTask MessageBasedCounter.Add i)
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore