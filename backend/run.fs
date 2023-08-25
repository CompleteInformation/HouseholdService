open RunHelpers
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
        | _ -> Job.error [ "Usage: dotnet run [<command>]"; "Look up available commands in run.fs" ]
    |> Job.execute
