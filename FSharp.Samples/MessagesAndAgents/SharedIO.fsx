(*
    A similar concurrency problem occurs when accessing a shared IO resource such as a file:

    - If the IO is slow, the clients can spend a lot of time waiting, even without locks.
    - If multiple threads write to the resource at the same time, you can get corrupted data.

    Both problems can be solved by using asynchronous calls combined with buffering -- exactly what a message queue does.

    In this next example, we'll consider the example of a logging service that many clients will write to concurrently.
    (In this trivial case, we'll just write directly to the Console.)
*)

(*
    The agent inside SerializedLogger simply reads a message from its input queue and writes it to the slow console.
    Again there is no code dealing with concurrency and no locks are used.
*)

let slowConsoleWrite msg = 
    msg |> String.iter (fun ch->
        System.Threading.Thread.Sleep(1)
        System.Console.Write ch
        )
    
type SerializedLogger() = 

    // create the mailbox processor
    let agent = MailboxProcessor.Start(fun inbox -> 

        // the message processing function
        let rec messageLoop () = async{

            // read a message
            let! msg = inbox.Receive()

            // write it to the log
            slowConsoleWrite msg

            // loop to top
            return! messageLoop ()
            }

        // start the loop
        messageLoop ()
        )

    // public interface
    member this.Log msg = agent.Post msg

// test in isolation
//let serializedLogger = SerializedLogger()
//serializedLogger.Log "hello"

(*
    Next, we will create a simple task that loops a few times, writing its name each time to the logger:
*)

let makeTask logger taskId = async {
    let name = sprintf "Task%i" taskId
    for i in [1..3] do 
        let msg = sprintf "-%s:Loop%i-" name i
        logger msg 
    }
// test in isolation
let task = makeTask slowConsoleWrite 1
Async.RunSynchronously task

let serializedExample = 
    let logger = new SerializedLogger()
    [1..5]
        |> List.map (fun i -> makeTask logger.Log i)
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore