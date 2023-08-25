open RunHelpers
open RunHelpers.BasicShortcuts
open RunHelpers.Shortcuts
open RunHelpers.Templates
open System.IO

[<RequireQualifiedAccess>]
module Config =
    let publishPath = "./src"
    let containerName = "household-backend"

module Task =
    let restore () =
        job {
            DotNet.toolRestore ()
            DotNet.restore Config.publishPath
        }

    let build () =
        job { DotNet.build Config.publishPath Debug }

    let run () = job { DotNet.run Config.publishPath }

    let publish config =
        job {
            let tags =
                match config with
                | Some (version: string, _, _) when version.Contains("-") -> [version; "prerelease"]
                | Some (version, _, _) -> [version; "latest"]
                | None -> [ "dev" ]
                |> String.concat ";"

            let name =
                match config with
                | Some (_, registry, prefix) -> $"{registry}/{prefix}/{Config.containerName}"
                | None -> $"completeinformation/{Config.containerName}"

            dotnet [
                "publish"
                Config.publishPath
                "--os"
                "linux"
                "--arch"
                "x64"
                "/p:PublishProfile=DefaultContainer"
                $"/p:ContainerRepository={name}"
                $"/p:ContainerImageTags=\"{tags}\""
                match config with
                | Some (_, registry, _) -> $"/p:ContainerRegistry=\"{registry}\""
                | None -> ()
                "-c"
                "Release"
            ]
        }

    let runContainer () =
        job { cmd "docker" [ "run"; "-p"; "5000:80"; $"completeinformation/{Config.containerName}:dev" ] }

[<EntryPoint>]
let main args =
    let oldCwd = Directory.GetCurrentDirectory()
    Directory.SetCurrentDirectory(__SOURCE_DIRECTORY__)

    let result =
        args
        |> List.ofArray
        |> function
            | [ "restore" ] -> Task.restore ()
            | [ "build" ] ->
                job {
                    Task.restore ()
                    Task.build ()
                }
            | []
            | [ "run" ] ->
                job {
                    Task.restore ()
                    Task.run ()
                }
            | [ "publish" ] ->
                job {
                    Task.restore ()
                    Task.publish None
                }
            | [ "publish"; version; registry; prefix ] ->
                job {
                    Task.restore ()
                    Task.build ()
                    Task.publish (Some (version, registry, prefix))
                }
            | [ "container" ] -> job { Task.runContainer () }
            | _ -> Job.error [ "Usage: dotnet run [<command>]"; "Look up available commands in run.fs" ]
        |> Job.execute

    Directory.SetCurrentDirectory(oldCwd)

    result
