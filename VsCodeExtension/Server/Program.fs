module CSharpLanguageServer.Program

open System

[<EntryPoint>]
let entry args =
    try
        Server.start(Console.OpenStandardInput(), Console.OpenStandardOutput())
    with
    | e ->
        printfn "Server crashing error - %s \n %s" e.Message e.StackTrace
        3
