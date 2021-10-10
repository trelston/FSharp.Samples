(*
    Just as in C#, F# can extend existing classes with extension methods.
*)

type System.String with
    member this.StartsWithA = this.StartsWith "A"
    
//test
let s = "Alice"
printfn $"'%s{s}' starts with an 'A' = %A{s.StartsWithA}"

type System.Int32 with
    member this.IsEven = this % 2 = 0
    
//test
let i = 20
if i.IsEven then printfn $"'%i{i}' is even"

