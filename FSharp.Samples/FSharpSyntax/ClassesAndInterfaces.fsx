(*
First, here are some examples of an interface, an abstract class, and a concrete class that inherits from the abstract class.
*)

// interface
type IEnumerator<'a> =
    abstract member Current : 'a
    abstract MoveNext : unit -> bool

//abstract base class with virtual methods
[<AbstractClass>]
type Shape() =
    //readonly properties
    abstract member Width : int
    abstract member Height : int

    //non-virtual method
    member this.BoundingArea = this.Height * this.Width

    //virtual method with base implementation
    abstract member Print : unit -> unit
    default this.Print() = printfn "I'm in shape"

//concrete class that inherits from base class and overrides
type Rectangle(x: int, y: int) =
    inherit Shape()
    override this.Width = x
    override this.Height = y
    override this.Print() = printfn "I'm a Rectangle"

//test
let r = Rectangle(2, 3)
printfn $"The width is %i{r.Width}"
printfn $"The area is %i{r.BoundingArea}"
r.Print()

(*
    Classes can have multiple constructors, mutable properties, and so on.
*)
type Circle(rad:int) =
    inherit Shape()
    
    //mutable field
    let mutable radius = rad
    
    //property overrides
    override this.Width = radius * 2
    override this.Height = radius * 2
    
    //alternate constructor with default radius
    new() = Circle(10)
    
    //property with get and set
    member this.Radius
        with get() = radius
        and set(value) = radius <- value
        
// test constructors
let c1 = Circle()   // parameterless ctor
printfn $"The width is %i{c1.Width}"
let c2 = Circle(2)  // main ctor
printfn $"The width is %i{c2.Width}"

// test mutable property
c2.Radius <- 3
printfn $"The width is %i{c2.Width}"