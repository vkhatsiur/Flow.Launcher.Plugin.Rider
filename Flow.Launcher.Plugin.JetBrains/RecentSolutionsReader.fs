namespace Flow.Launcher.Plugin.JetBrains

open System.IO
open System.Xml.Linq

module RecentSolutionsReader =

    let private parseRecentProjects (recentSolutionsXmlPath: string) : seq<string> =

        try
            let xmlDocument = XDocument.Load(recentSolutionsXmlPath)

            xmlDocument.Descendants("entry")
            |> Seq.choose (fun (element: XElement) -> element.Attribute("key") |> Option.ofObj |> Option.map _.Value)
        with _ ->
            Seq.empty // ignore exception for now

    let collectFromDirectory (rootDirectory: string) (searchPattern: string) (recentProjectFileName: string) : seq<string> =
        Directory.GetDirectories(rootDirectory, searchPattern)
        |> Array.map (fun ideDirectory -> Path.Combine(ideDirectory, "options", recentProjectFileName))
        |> Array.filter File.Exists
        |> Seq.collect parseRecentProjects
        |> Seq.distinct
        |> Seq.filter (fun pathToProject -> File.Exists pathToProject || Directory.Exists pathToProject)
