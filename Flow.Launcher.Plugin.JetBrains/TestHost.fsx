#r @"bin/Debug/net7.0-windows/Flow.Launcher.Plugin.JetBrains.dll"

open System
open System.IO
open Flow.Launcher.Plugin.JetBrains

let applicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JetBrains");
let files = RecentSolutionsReader.collectFromDirectory applicationDataFolder
files |> Seq.iter (printfn "%s")