(*
The TryParse and TryGetValue functions for values and dictionaries are frequently used to avoid extra exception handling.
But the C# syntax is a bit clunky.
Using them from F# is more elegant because F# will automatically convert the function into a tuple
where the first element is the function return value and the second is the "out" parameter. 
*)

//using an Int32
let (i1success, i1) = System.Int32.TryParse("123")
if i1success then printfn "parsed as %i" i1 else printfn "parse failed"

let (i2success,i2) = System.Int32.TryParse("hello");
if i2success then printfn "parsed as %i" i2 else printfn "parse failed"

//using a DateTime
let (d1success,d1) = System.DateTime.TryParse("1/1/1980");
let (d2success,d2) = System.DateTime.TryParse("hello")

//using a dictionary
let dict = new System.Collections.Generic.Dictionary<string,string>();
dict.Add("a","hello")
let (e1success,e1) = dict.TryGetValue("a");
let (e2success,e2) = dict.TryGetValue("b")
