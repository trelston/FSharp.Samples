(*
    In C# (and .NET in general), you can have overloaded methods with many different parameters.
    F# can have trouble with this. For example, here is an attempt to create a StreamReader:
*)

let createReader fileName = new System.IO.StreamReader(fileName)
// error FS0041: A unique overload for method 'StreamReader' 
//               could not be determined

(*
    The problem is that F# does not know if the argument is supposed to be a string or a stream.
    You could explicitly specify the type of the argument, but that is not the F# way! 
*)

(*
    Instead, a nice workaround is enabled by the fact that in F#,
    when calling methods in .NET libraries, you can specify named arguments.
*)

let createReader2 fileName = new System.IO.StreamReader(path=fileName)

(*
    In many cases, such as the one above, just using the argument name is enough to resolve the type issue.
    And using explicit argument names can often help to make the code more legible anyway.
*)