//standard generics
type KeyValuePair<'a,'b>(key:'a, value: 'b) =
    member this.Key = key
    member this.Value = value
    
//generics with constraints
type Container<'a,'b
    when 'a : equality
    and 'b :> System.Collections.ICollection>
    (name: 'a, values: 'b) =
        member this.Name = name
        member this.Values = values
