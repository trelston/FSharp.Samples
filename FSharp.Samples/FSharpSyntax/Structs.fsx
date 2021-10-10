(*
    F# supports not just classes, but the .NET struct types as well, which can help to boost performance in certain cases.
*)

type Point2D =
    struct
        val X: float
        val Y: float
        new(x: float, y: float) = {X = x; Y = y}
    end
    
//test
let p = Point2D()  // zero initialized
let p2 = Point2D(2.0,3.0)  // explicitly initialized


