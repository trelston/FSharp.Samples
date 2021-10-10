(*
    Just like C#'s variable length "params" keyword,
    this allows a variable length list of arguments to be converted to a single array parameter.
*)

open System
type MyConsole() =
    member this.WriteLine([<ParamArray>] args: Object[]) =
        for arg in args do
             printfn "%A" arg
             
let cons = new MyConsole()
cons.WriteLine("abc", 42, 3.14, true)

