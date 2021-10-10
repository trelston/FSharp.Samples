(*
    F# has a built-in agent class called MailboxProcessor.
    These agents are very lightweight compared with threads
        - you can instantiate tens of thousands of them at the same time.

    However, the messages are not persistent. If your app crashes, the messages are lost.
*)

#nowarn "40"
let printerAgent = MailboxProcessor.Start(fun inbox ->
    
    //the message processing function
    let rec messageloop = async {
        
        //read a message
        let! msg = inbox.Receive()
        
        // process a message
        printfn $"message is: %s{msg}"
        
        // loop to top
        return! messageloop
    }
    
    //start the loop
    messageloop
    
    )

(*

The MailboxProcessor.Start function takes a simple function parameter.
That function loops forever, reading messages from the queue (or "inbox") and processing them.

*)

// test it
printerAgent.Post "hello" 
printerAgent.Post "hello again" 
printerAgent.Post "hello a third time"