//"return" (also known as "unit" or "pure") simply creates a elevated value from a normal value.
//a -> E<a>

//A value lifted to the world of Options
let returnOption x = Some x

//A value lifted to the world of lists
let returnList x = [x]