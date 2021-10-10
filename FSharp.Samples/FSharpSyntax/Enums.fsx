(*
    F# supports CLI enums types, which look similar to the "union" types, but are actually different behind the scenes.
*)
// enums
type Color = | Red=1 | Green=2 | Blue=3

let color1  = Color.Red    // simple assignment
let color2:Color = enum 2  // cast from int
// created from parsing a string
let color3 = System.Enum.Parse(typeof<Color>,"Green") :?> Color // :?> is a downcast

[<System.FlagsAttribute>]
type FileAccess = | Read=1 | Write=2 | Execute=4 
let fileaccess = FileAccess.Read ||| FileAccess.Write