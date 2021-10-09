//Applicative style is when the input parameters of a function are independent
//and do not depend on previous input parameters.

//Monadic style is when the next input parameter of a function depends on the result of the previous
//input parameter to the function

type CustomerId = CustomerId of int
type EmailAddress = EmailAddress of string
type CustomerInfo = {
    id: CustomerId
    email: EmailAddress
}

let createCustomerId id =
    if id > 0 then
        Ok (CustomerId id)
    else
        Error ["CustomerId must be positive"]

let createEmailAddress str =
    if System.String.IsNullOrEmpty(str) then
        Error ["Email must not be empty"]
    elif str.Contains("@") then
        Ok (EmailAddress str)
    else
        Error ["Email must contain @-sign"]

//Notice that createCustomerId has type int -> Result<CustomerId, string list>, and
//createEmailAddress has type string -> Result<EmailAddress, string list>.

//That means that both of these validation functions are world-crossing functions, going from
//the normal world to the Result<_> world.

//map has signature: ('a -> 'b) -> Result<'a> -> Result<'b>
//retn has signature: 'a -> Result<'a>
//apply has signature: Result<('a -> 'b)> -> Result<'a> -> Result<'b>
//bind has signature: ('a -> Result<'b>) -> Result<'a> -> Result<'b>

module Result =

    let apply fResult xResult =
        match fResult,xResult with
        | Ok f, Ok x ->
            Ok (f x)
        | Error errs, Ok x ->
            Error errs
        | Ok f, Error errs ->
            Error errs
        | Error errs1, Error errs2 ->
            // concat both lists of errors
            Error (List.concat [errs1; errs2])


//Validation using the applicative style
let createCustomer customerId email =
    { id = customerId; email = email }

let (<!>) = Result.map
let (<*>) = Result.apply

//applicative version
let createCustomerResultA id email =
    let idResult = createCustomerId id 
    let emailResult = createEmailAddress email
    createCustomer <!> idResult <*> emailResult

//Lets try it out with some good and bad data
let goodId = 1
let badId = 0
let goodEmail = "test@example.com"
let badEmail = "example.com"

let goodCustomerA =
    createCustomerResultA goodId goodEmail
// Result<CustomerInfo> =
// Success {id = CustomerId 1; email = EmailAddress "test@example.com";}

let badCustomerA =
    createCustomerResultA badId badEmail
// Result<CustomerInfo> =
// Failure ["CustomerId must be positive"; "Email must contain @-sign"]

//Validation using the monadic style
//In this case, the logic will be
//1. try to convert an int into a CustomerId
//2. if that is successful, try to convert a string into an EmailAddress
//3. if that is successful, create a CustomerInfo from the customerId and email.

let (>>=) x f = Result.bind f x

//monadic version
let createCustomerResultM id email =
    createCustomerId id >>= (fun customerId ->
    createEmailAddress email >>= (fun emailAddress ->
    let customer = createCustomer customerId emailAddress
    Ok customer))
    // int -> string -> Result<CustomerInfo>

//The signature of the monadic-style createCustomerResultM is exactly the same as the
//applicative-style createCustomerResultA but internally it is doing something different, which
//will be reflected in the different results we get.

let goodCustomerM =
    createCustomerResultM goodId goodEmail
// Result<CustomerInfo> =
// Success {id = CustomerId 1; email = EmailAddress "test@example.com";}

let badCustomerM =
    createCustomerResultM badId badEmail
// Result<CustomerInfo> =
// Failure ["CustomerId must be positive"]


//In the good customer case, the end result is the same, but in the bad customer case, only
//one error is returned, the first one. The rest of the validation was short circuited after the
//CustomerId creation failed.

//Comparing the two styles

//The applicative example did all the validations up front, and then combined the results.
//The benefit was that we didn't lose any of the validation errors. The downside was we
//did work that we might not have needed to do.

//the monadic example did one validation at a time, chained together.
//The benefit was that we short-circuited the rest of the chain as soon as an error
//occurred and avoided extra work. The downside was that we only got the first error.

//Mixing the two styles
//Now there is nothing to say that we can't mix and match applicative and monadic styles.

//For example, we might build a CustomerInfo using applicative style, so that we don't lose
//any errors, but later on in the program, when a validation is followed by a database update,
//we probably want to use monadic style, so that the database update is skipped if the
//validation fails.