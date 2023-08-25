open RunHelpers
open RunHelpers.BasicShortcuts
open RunHelpers.Shortcuts
open RunHelpers.Templates

[<RequireQualifiedAccess>]
module Config =
    let publishPath = "./src"

module Task =
    let restore () =
        job {
            DotNet.toolRestore ()
            DotNet.restore Config.publishPath
        }

    let build () =
        job { DotNet.build Config.publishPath Debug }

    let run () = job { DotNet.run Config.publishPath }

    let publish version =
        job {
            let tags =
                [
                    "latest"
                    match version with
                    | Some version -> version: string
                    | None -> ()
                ]
                |> String.concat ";"

            dotnet [
                "publish"
                Config.publishPath
                "--os"
                "linux"
                "--arch"
                "x64"
                "/t:PublishContainer"
                $"/p:ContainerImageTags=\"{tags}\""
                "-c"
                "Release"
            ]
        }

    let runContainer () =
        job { cmd "docker" [ "run"; "-p"; "5000:80"; "complete-information-household-backend:latest" ] }

[<EntryPoint>]
let main args =
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
        | [ "publish"; version ] ->
            job {
                Task.restore ()
                Task.publish (Some version)
            }
        | [ "container" ] -> job { Task.runContainer () }
        | _ -> Job.error [ "Usage: dotnet run [<command>]"; "Look up available commands in run.fs" ]
    |> Job.execute
