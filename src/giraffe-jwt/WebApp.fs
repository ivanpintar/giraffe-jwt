namespace GiraffeJwt

module WebApp =
    open GiraffeJwt.Models
    open System.Security.Claims
    open Microsoft.AspNetCore.Http
    open Giraffe
    open DataAccess
        
    let checkUnAndPass (login:LoginModel) =
        if userExists login.Name login.Password
        then login.Name |> Some
        else None
    
    let handlePostToken checkUsernameF next (ctx:HttpContext) =
        task {
            let! model = ctx.BindJsonAsync<LoginModel>()            
            let generateToken = 
                function
                | None -> { Token = "Invalid Token" }
                | Some n -> Auth.generateToken n
            let tokenResult = model |> checkUsernameF |> generateToken           
            return! json tokenResult next ctx
        } 

    let handleGetSecured next (ctx:HttpContext) =
        let name = ctx.User.FindFirst ClaimTypes.NameIdentifier
        let role = ctx.User.FindFirst ClaimTypes.Role
        text (sprintf "User %s is authorized to access this resource with role %s" name.Value role.Value) next ctx

    let webApp : HttpFunc -> HttpContext -> HttpFuncResult =
        choose [
            GET >=> 
                choose [
                    route "/" >=> text "public route"
                    route "/admin" >=> Auth.authorize [ Admin ] >=> handleGetSecured
                    route "/user" >=> Auth.authorize [ Role.Admin; Role.User ] >=> handleGetSecured
                ]
            POST >=>
                choose [
                    route "/token" >=> handlePostToken checkUnAndPass
                ]
            RequestErrors.NOT_FOUND "Page not found"       
        ]