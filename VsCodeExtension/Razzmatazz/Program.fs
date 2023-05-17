module CSharpLanguageServer.Program

[<EntryPoint>]
let entry args =
    try
        Server.start()
    with
    | e ->
        printfn "Server crashing error - %s \n %s" e.Message e.StackTrace
        3
