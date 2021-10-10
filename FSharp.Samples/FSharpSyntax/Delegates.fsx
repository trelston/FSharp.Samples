(*
    F# can do delegates.
*)

// delegates
type MyDelegate = delegate of int -> int
let f = MyDelegate (fun x -> x * x)
let result = f.Invoke(5)

