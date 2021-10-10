(*
    F# can create exception classes, raise them and catch them.
*)

// create a new Exception class
exception MyError of string

try
    let e = MyError("Oops!")
    raise e
with 
    | MyError msg -> 
        printfn "The exception error was %s" msg
    | _ -> 
        printfn "Some other exception"