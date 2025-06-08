namespace Flow.Launcher.Plugin.JetBrains

open System
open Microsoft.Win32

type RegistryAppEntry =
    { DisplayName: string
      ExePath: string option }

module WindowsRegistry =

    let private readAllKeys (hive: RegistryHive) (view: RegistryView) : seq<string * string option> =

        let tryGetValueAsString (key: RegistryKey) (name: string) : string option =
            match key.GetValue(name) with
            | :? string as s when not (String.IsNullOrWhiteSpace(s)) -> Some s
            | _ -> None

        use baseKey = RegistryKey.OpenBaseKey(hive, view)

        baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
        |> Option.ofObj
        |> Option.map (fun uninstallKey ->
            uninstallKey.GetSubKeyNames()
            |> Seq.choose (fun subKeyName ->
                match uninstallKey.OpenSubKey(subKeyName) with
                | null -> None
                | sk ->
                    use sk = sk

                    tryGetValueAsString sk "DisplayName"
                    |> Option.map (fun displayName ->
                        let exePath = tryGetValueAsString sk "DisplayIcon"
                        (displayName, exePath))))
        |> Option.defaultValue Seq.empty


    let getAppRootDirectory (appName: string) : RegistryAppEntry option =
        readAllKeys RegistryHive.LocalMachine RegistryView.Registry32
        |> Seq.tryFind (fun (displayName, _) -> displayName.Contains(appName))
        |> Option.map (fun (displayName, exePath) ->
            { DisplayName = displayName
              ExePath = exePath })
