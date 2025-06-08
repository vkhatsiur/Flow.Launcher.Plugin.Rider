namespace Flow.Launcher.Plugin.JetBrains

open System.Collections.Generic
open System.IO
open Flow.Launcher.Plugin

type JetBrainsPlugin() =

    let mutable initContext = PluginInitContext()
    let mutable ideConfig: Map<IDE, RegistryAppEntry option> = Map.empty

    interface IPlugin with

        member this.Init(context) =
            initContext <- context

            ideConfig <-
                IDEContext.allSupported
                |> Array.map (fun ide -> (ide, WindowsRegistry.getAppRootDirectory (ide.ToString())))
                |> Map.ofArray

        member this.Query(query) =

            JetBrainsService.findProjectsByName query.Search
            |> Array.collect (fun resultItem ->

                let getCmd =
                    fun location -> $"\"\"{ideConfig[resultItem.IDE].Value.ExePath.Value}\" \"{location}\"\""

                let icon = IDEContext.iconConfigs[resultItem.IDE]

                resultItem.Projects
                |> Array.map (fun project ->

                    let title = Path.GetFileNameWithoutExtension(project.Location)
                    let cmd = getCmd project.Location

                    Result(
                        Title = title,
                        SubTitle = project.Location,
                        IcoPath = icon,
                        Action =
                            (fun _ ->
                                initContext.API.ShellRun(cmd)
                                true)
                    )))
            |> List<Result>
