(*
    A common case is that a .NET library class has a number of mutually exclusive isSomething, isSomethingElse methods,
        which have to be tested with horrible looking cascading if-else statements.
    Active patterns can hide all the ugly testing, letting the rest of your code use a more natural approach.
*)

(*
    For example, here's the code to test for various isXXX methods for System.Char.
*)
let (|Digit|Letter|Whitespace|Other|) ch =
    if System.Char.IsDigit(ch) then
        Digit
    else if System.Char.IsLetter(ch) then
        Letter
    else if System.Char.IsWhiteSpace(ch) then
        Whitespace
    else
        Other

(*
    Once the choices are defined, the normal code can be straightforward:
*)
let printChar ch =
    match ch with
    | Digit -> printfn "%c is a Digit" ch
    | Letter -> printfn "%c is a Letter" ch
    | Whitespace -> printfn "%c is a Whitespace" ch
    | _ -> printfn "%c is something else" ch

// print a list
[ 'a'; 'b'; '1'; ' '; '-'; 'c' ]
|> List.iter printChar

(*
    Another common case is when you have to parse text or error codes to determine the type of an exception or result.
    Here's an example that uses an active pattern to parse the error number associated with SqlExceptions,
    making them more palatable.
*)

(*
    First, set up the active pattern matching on the error number:
*)
#r "../../packages/System.Data.SqlClient/lib/netcoreapp2.1/System.Data.SqlClient.dll"
open System.Data.SqlClient

let (|ConstraintException|ForeignKeyException|Other|) (ex: SqlException) =
    if ex.Number = 2601 then
        ConstraintException
    else if ex.Number = 2627 then
        ConstraintException
    else if ex.Number = 547 then
        ForeignKeyException
    else
        Other

(*
    Now we can use these patterns when processing SQL commands:
*)
let executeNonQuery (sqlCommand: SqlCommand) =
    try
        let result = sqlCommand.ExecuteNonQuery()
        ignore
    // handle success
    with
    | :? SqlException as sqlException -> // if a SqlException
        match sqlException with // nice pattern matching
        | ConstraintException -> ignore // handle constraint error
        | ForeignKeyException -> ignore // handle FK error
        | _ -> reraise () // don't handle any other cases
// all non SqlExceptions are thrown normally
