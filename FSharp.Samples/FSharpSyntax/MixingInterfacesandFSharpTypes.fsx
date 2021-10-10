(*
    The ability to create instances of an interface on the fly means that it is easy to mix and match interfaces
    from existing APIs with pure F# types.
*)

(*
 For example, say that you have a preexisting API which uses the IAnimal interface, as shown below.   
*)

type IAnimal = 
   abstract member MakeNoise : unit -> string

let showTheNoiseAnAnimalMakes (animal:IAnimal) = 
   animal.MakeNoise() |> printfn "Making noise %s"
   
(*
    But we want to have all the benefits of pattern matching, etc,
    so we have created pure F# types for cats and dogs instead of classes. 
*)

type Cat = Felix | Socks
type Dog = Butch | Lassie

(*
    But using this pure F# approach means that
    that we cannot pass the cats and dogs to the showTheNoiseAnAnimalMakes function directly.
*)

(*
    However, we don't have to create new sets of concrete classes just to implement IAnimal.
    Instead, we can dynamically create the IAnimal interface by extending the pure F# types.
*)

// now mixin the interface with the F# types
type Cat with
   member this.AsAnimal = 
        { new IAnimal 
          with member a.MakeNoise() = "Meow" }

type Dog with
   member this.AsAnimal = 
        { new IAnimal 
          with member a.MakeNoise() = "Woof" }
        

let dog = Lassie
showTheNoiseAnAnimalMakes (dog.AsAnimal)

let cat = Felix
showTheNoiseAnAnimalMakes (cat.AsAnimal)

(*
    This approach gives us the best of both worlds.
    Pure F# types internally, but the ability to convert them into interfaces as needed to interface with libraries.
*)

