namespace Flow.Launcher.Plugin.Rider

open System.Collections.Generic
open Flow.Launcher.Plugin

type RiderPlugin() =
    
    let mutable initContext = PluginInitContext()
    
    interface IPlugin with
    
        member this.Init(context) = initContext <- context
        
        member this.Query(query) = List<Result> [
            Result(Title = "Hello from rider plugin")
        ]