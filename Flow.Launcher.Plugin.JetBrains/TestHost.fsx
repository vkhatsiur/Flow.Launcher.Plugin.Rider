#load "RecentSolutionsReader.fs"
#load "JetBrainsService.fs"
#load "WindowsRegistry.fs"

open System
open System.IO
open Flow.Launcher.Plugin.JetBrains

let applicationDataFolder =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JetBrains")

let results = JetBrainsService.findProjectsByName ""

let configs =
    IDEContext.allSupported
    |> Array.map (fun ide -> (ide, WindowsRegistry.getAppRootDirectory (ide.ToString())))
    |> Map.ofArray


let testQueryResults =
    JetBrainsService.findProjectsByName "superfile"
    |> Array.collect (fun resultItem ->

        let getCmd =
            fun location -> $"\"{configs[resultItem.IDE].Value.ExePath.Value}\" \"{location}\""

        let icon = IDEContext.iconConfigs[resultItem.IDE]

        resultItem.Projects
        |> Array.map (fun project ->

            let title = Path.GetFileNameWithoutExtension(project.Location)

            getCmd project.Location))

Console.ReadLine()
