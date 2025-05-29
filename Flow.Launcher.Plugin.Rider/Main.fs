namespace Flow.Launcher.Plugin.Rider

open System
open System.Collections.Generic
open System.IO
open System.Xml.Linq
open Flow.Launcher.Plugin

module RecentSolutionsReader =

    let private extractSolutionsPath (recentSolutionsXmlPath: string) : seq<string> =

        try
            let xmlDocument = XDocument.Load(recentSolutionsXmlPath)

            xmlDocument.Descendants("entry")
            |> Seq.choose (fun (element: XElement) -> element.Attribute("key") |> Option.ofObj |> Option.map _.Value)
        with _ ->
            Seq.empty // ignore exception for now

    let collectFromDirectory (rootDirectory: string) : seq<string> =
        Directory.GetDirectories(rootDirectory, "Rider*")
        |> Array.map (fun riderDirectory -> Path.Combine(riderDirectory, "options", "recentSolutions.xml"))
        |> Array.filter File.Exists
        |> Seq.collect extractSolutionsPath
        |> Seq.distinct
        |> Seq.filter File.Exists

type RiderPlugin() =

    let mutable initContext = PluginInitContext()

    interface IPlugin with

        member this.Init(context) = initContext <- context

        member this.Query(query) =

            let applicationDataFolder =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JetBrains")

            RecentSolutionsReader.collectFromDirectory applicationDataFolder
            |> Seq.map (fun solutionPath ->
                Result(
                    Title = Path.GetFileNameWithoutExtension(solutionPath),
                    SubTitle = solutionPath,
                    IcoPath = initContext.CurrentPluginMetadata.IcoPath,
                    Action =
                        (fun _ ->
                            initContext.API.ShellRun(solutionPath)
                            true)
                ))
            |> Seq.filter _.Title.Contains(query.Search, StringComparison.OrdinalIgnoreCase)
            |> List<Result>