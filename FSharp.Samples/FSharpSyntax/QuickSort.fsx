let rec quicksort list =
    match list with
    | [] ->                             //If list is empty
        []                              //return an empty list
    | firstElem :: otherElements ->     //If the list is not empty
        let smallerElements =           //extract the smaller ones
            otherElements
            |> List.filter (fun e -> e < firstElem)
            |> quicksort                //and sort them
        let largerElements =
            otherElements
            |> List.filter (fun e -> e >= firstElem)
            |> quicksort                //and sort them
        //Combine the three parts into a new list and return it
        List.concat [smallerElements; [firstElem]; largerElements]
        
let rec quicksort2 = function
    | [] -> []
    | first::rest ->
        let smaller,larger = List.partition ((>=) first) rest
        List.concat [quicksort2 smaller; [first]; quicksort2 larger]
            
    
        