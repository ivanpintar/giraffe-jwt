namespace GiraffeJwt.Models

[<CLIMutable>]
type LoginModel = { Name : string
                    Password : string }

[<CLIMutable>]
type TokenResult = { Token : string }    

type Role =
    | Admin
    | User

type User = { Name : string
              Role : Role }