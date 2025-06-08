namespace Flow.Launcher.Plugin.JetBrains

open System
open System.IO

type Project =
    { DisplayName: string
      Location: string }

type IDE =
    | Rider
    | GoLand

module IDEContext =
    let allSupported = [| Rider; GoLand |]

    let recentProjectConfigs =
        [| {| IDE = Rider
              RecentProjectFileName = "recentSolutions.xml" |}
           {| IDE = GoLand
              RecentProjectFileName = "recentProjects.xml" |} |]
        
    let iconConfigs : Map<IDE, string> =
        Map [
            Rider, "assets/rider-icon.png"
            GoLand, "assets/goland-icon.png"
        ]

type FindProjectResults = { IDE: IDE; Projects: Project array }

module JetBrainsService =

    let private getAppdataDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JetBrains")

    let findProjectsByName (namePart: string) : FindProjectResults array =

        let rootDirectory = getAppdataDirectory

        IDEContext.recentProjectConfigs
        |> Array.map (fun config ->
            let searchPatter = $"{config.IDE}*"

            let projects =
                RecentSolutionsReader.collectFromDirectory rootDirectory searchPatter config.RecentProjectFileName
                |> Seq.map (fun projectPath ->
                    { DisplayName = Path.GetFileNameWithoutExtension projectPath
                      Location = projectPath })
                |> Seq.filter _.DisplayName.Contains(namePart, StringComparison.OrdinalIgnoreCase)
                |> Seq.toArray

            { IDE = config.IDE
              Projects = projects })
