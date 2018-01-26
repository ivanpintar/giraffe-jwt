namespace GiraffeJwt

module DataAccess = 
    open GiraffeJwt.Models
    open System.Collections.Generic
    
    let private passwords =         
        [ { Name = "admin"; Password = "adminpass" }
          { Name = "user1"; Password = "userpass1" } ]

    let private users =
        [ { Name = "admin"; Role = Admin }
          { Name = "user1"; Role = User } ]

    
    let userExists name password = 
        List.exists (fun (u:LoginModel) -> u.Name = name && u.Password = password) passwords

    let findUser name =
        try List.find (fun (u:User) -> u.Name = name) users |> Some
        with 
            | :? KeyNotFoundException -> None
            

            


