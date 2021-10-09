(*
 Let's say that you are designing an e-commerce site which has a shopping cart and you are given the following requirements.

    You can only pay for a cart once.
    Once a cart is paid for, you cannot change the items in it.
    Empty carts cannot be paid for.
*)

(*
Looking at these requirements, it's obvious that we have a simple state machine with three states and some state transitions:

    - A Shopping Cart can be Empty, Active or PaidFor
    - When you add an item to an Empty cart, it becomes Active
    - When you remove the last item from an Active cart, it becomes Empty
    - When you pay for an Active cart, it becomes PaidFor

And now we can add the business rules to this model:

    - You can add an item only to carts that are Empty or Active
    - You can remove an item only from carts that are Active
    - You can only pay for carts that are Active
*)

(*
It's worth noting that these kinds of state-oriented models are very common in business systems.
Product development, customer relationship management, order processing, and other workflows can often be modeled this way.
*)

type CartItem = string //placeholder for a more complicated type

type EmptyState = NoItems //don't use empty list! We want to force clients to handle this as a seperate case
//E.g. "You have no items in your cart."

type ActiveState = { UnpaidItems: CartItem list }

type PaidForState =
    { PaidItems: CartItem list
      Payment: decimal }

type Cart =
    | Empty of EmptyState
    | Active of ActiveState
    | PaidFor of PaidForState

(*
We create a type for each state, and Cart type that is a choice of any one of the states.
I have given everything a distinct name (e.g. PaidItems and UnpaidItems rather than just Items)
because this helps the inference engine and makes the code more self documenting.
*)

(*
Next we can create the operations for each state.
The main thing to note is each operation will always take one of the States as input and return a new Cart.
That is, you start off with a particular known state, but you return a Cart which is a wrapper for a choice
of three possible states.
*)

// =============================
// operations on empty state
// =============================

let addToEmptyState item =
    //returns a new active cart
    Cart.Active { UnpaidItems = [ item ] }

// =============================
// operations on active state
// =============================

let addToActiveState state itemToAdd =
    let newList = itemToAdd :: state.UnpaidItems
    Cart.Active { state with UnpaidItems = newList }

let removeFromActiveState state itemToRemove =
    let newList =
        state.UnpaidItems
        |> List.filter (fun i -> i <> itemToRemove)

    match newList with
    | [] -> Cart.Empty NoItems
    | _ -> Cart.Active { state with UnpaidItems = newList }

let payForActiveState state amount =
    //returns a paid for cart
    Cart.PaidFor
        { PaidItems = state.UnpaidItems
          Payment = amount }

(*
Next, we attach the operations to the states as methods
*)

type EmptyState with
    member this.Add = addToEmptyState

type ActiveState with
    member this.Add = addToActiveState this
    member this.Remove = removeFromActiveState this
    member this.Pay = payForActiveState this

(*
And we can create some cart level helper methods as well.
At the cart level, we have to explicitly handle each possibility for the internal state with a match..with expression.
*)

let addItemToCart cart item =
    match cart with
    | Empty state -> state.Add item
    | Active state -> state.Add item
    | PaidFor state ->
        printfn "ERROR: The cart is paid for"
        cart

let removeItemFromCart cart item =
    match cart with
    | Empty state ->
        printfn "ERROR: The cart is empty"
        cart
    | Active state -> state.Remove item
    | PaidFor state ->
        printfn "ERROR: The cart is paid for"
        cart

let displayCart cart =
    match cart with
    | Empty state -> printfn "The cart is empty" // can't do state.Items
    | Active state -> printfn "The cart contains %A unpaid items" state.UnpaidItems
    | PaidFor state -> printfn "The cart contains %A paid items. Amount paid: %f" state.PaidItems state.Payment

type Cart with
    static member NewCart = Cart.Empty NoItems
    member this.Add = addItemToCart this
    member this.Remove = removeItemFromCart this
    member this.Display = displayCart this



