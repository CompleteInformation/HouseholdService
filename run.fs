open Fake.IO
open RunHelpers
open RunHelpers.Templates
open System.IO

[<RequireQualifiedAccess>]
module Config =
    let publishPath = "./publish"

module Task =
    let getProjects () =
        Directory.EnumerateFiles(".", "*.fsproj", SearchOption.AllDirectories)
        |> Seq.filter ((=) "./run.fsproj" >> not)

    let restore () =
        job {
            DotNet.toolRestore ()

            for project in getProjects () do
                DotNet.restore project
        }

    let build () =
        job {
            for project in getProjects () do
                DotNet.build project Debug
        }

[<EntryPoint>]
let main args =
    args
    |> List.ofArray
    |> function
        | [ "restore" ] -> Task.restore ()
        | []
        | [ "build" ] ->
            job {
                Task.restore ()
                Task.build ()
            }
        | _ -> Job.error [ "Usage: dotnet run [<command>]"; "Look up available commands in run.fs" ]
    |> Job.execute
