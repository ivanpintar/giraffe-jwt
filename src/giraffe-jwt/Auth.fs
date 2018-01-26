namespace GiraffeJwt

module Auth = 
    open Microsoft.IdentityModel.Tokens
    open System.Security.Claims
    open System.IdentityModel.Tokens.Jwt
    open System
    open System.Text
    open Microsoft.AspNetCore.Authentication.JwtBearer
    open Giraffe
    open GiraffeJwt.Models 
    open Microsoft.AspNetCore.Http

    let key = SymmetricSecurityKey(Encoding.UTF8.GetBytes("giberrish ssdgdfgs sdfg sdfgs sdgdgsdfg"))
    let domain = "http://localhost:5001"
    let tokenValidationParams = TokenValidationParameters(ValidateActor = true,
                                                          ValidateAudience = true,
                                                          ValidateLifetime = true,
                                                          ValidateIssuerSigningKey = true,
                                                          ValidIssuer = domain,
                                                          ValidAudience = domain,
                                                          IssuerSigningKey = key)

    
    let unauthorized = setStatusCode 403 >=> text "Forbidden"

    let authenticate : HttpFunc -> HttpContext -> HttpFuncResult =         
        requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)   

    let mustBe roles =
        let roles' = List.map (fun r -> r.ToString()) roles
        requiresRoleOf roles' unauthorized

    let addRoleClaim findUserF next (ctx:HttpContext) =
        let name = ctx.User.FindFirst ClaimTypes.NameIdentifier
        match findUserF name.Value with
        | Some user ->        
            let claimsId = ctx.User.Identity :?> ClaimsIdentity
            claimsId.AddClaim(Claim(ClaimTypes.Role, user.Role.ToString()))
            next ctx
        | None -> next ctx
        
    let authorize roles = authenticate >=> addRoleClaim DataAccess.findUser >=> mustBe roles

    let generateToken name =
        let claims = [|
            Claim(JwtRegisteredClaimNames.Sub, name);
            Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        |]

        let signingCreds = SigningCredentials(key = key, 
                                              algorithm = SecurityAlgorithms.HmacSha256)

        let token = JwtSecurityToken(issuer = domain,
                                     audience = domain,
                                     claims = claims,
                                     expires = Nullable(DateTime.UtcNow.AddHours(1.0)),
                                     notBefore = Nullable(DateTime.UtcNow),
                                     signingCredentials = signingCreds)

        { Token = JwtSecurityTokenHandler().WriteToken(token) } 
