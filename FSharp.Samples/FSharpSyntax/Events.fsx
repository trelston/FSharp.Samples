(*
    F# classes can have events, and the events can be triggered and responded to.
*)

type MyButton() =
    let clickEvent = new Event<_>()

    [<CLIEvent>]
    member this.OnClick = clickEvent.Publish

    member this.TestEvent(arg) =
        clickEvent.Trigger(this, arg)

// test
let myButton = new MyButton()
myButton.OnClick.Add(fun (sender, arg) -> 
        printfn $"Click event with arg={arg}")

myButton.TestEvent("Hello World!")