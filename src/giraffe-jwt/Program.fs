namespace GiraffeJwt

module Main =
    open System
    open System.IO
    open Microsoft.AspNetCore.Builder
    open Microsoft.AspNetCore.Cors.Infrastructure
    open Microsoft.AspNetCore.Hosting
    open Microsoft.Extensions.Logging
    open Microsoft.Extensions.DependencyInjection
    open Giraffe
    open Microsoft.AspNetCore.Authentication.JwtBearer
    open Microsoft.IdentityModel.Tokens
    
    module ErrorHandling =
        let errorHandler (ex : Exception) (logger : ILogger) =
            logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
            clearResponse >=> setStatusCode 500 >=> text ex.Message

   
    module Configuration =
        open ErrorHandling

        let configureCors (builder : CorsPolicyBuilder) =
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   |> ignore

        let configureApp (app : IApplicationBuilder) =
            app.UseCors(configureCors)
               .UseGiraffeErrorHandler(errorHandler)
               .UseStaticFiles()
               .UseAuthentication()
               .UseGiraffe(WebApp.webApp)

        let configureServices (services : IServiceCollection) =
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(fun opts -> 
                    opts.RequireHttpsMetadata <- false
                    opts.TokenValidationParameters <- Auth.tokenValidationParams) |> ignore

            services.AddCors() |> ignore

        let configureLogging (builder : ILoggingBuilder) =
            let filter (l : LogLevel) = l.Equals LogLevel.Error
            builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

    module Main = 
        open Configuration

        [<EntryPoint>]
        let main _ =
        
            let contentRoot = Directory.GetCurrentDirectory()
            let webRoot     = Path.Combine(contentRoot, "WebRoot")
            WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(contentRoot)
                .UseUrls("http://*:5001")
                .UseIISIntegration()
                .UseWebRoot(webRoot)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
                .Build()
                .Run()
            0