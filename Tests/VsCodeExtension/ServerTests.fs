module ServerTests

open CSharpLanguageServer
open FluentAssertions
open System.IO
open System.IO.Pipes
open Xunit

let prependContentLength(request: string) = $"Content-Length: {request.Length}\r\n\r\n{request}"

let readOutput(outputReader: StreamReader): string =
    let contentLengthLine = outputReader.ReadLine()
    let contentLength = int(contentLengthLine.Replace("Content-Length: ", ""))
    outputReader.ReadLine() |> ignore

    let contentChars: char array = Array.zeroCreate contentLength
    outputReader.Read(contentChars, 0, contentLength) |> ignore
    new System.String(contentChars)

[<Fact>]
let ``server responds with data when requesting initialize`` () = async {
    use inputServerPipe = new AnonymousPipeServerStream()
    use inputClientPipe = new AnonymousPipeClientStream(inputServerPipe.GetClientHandleAsString())
    use outputServerPipe = new AnonymousPipeServerStream()
    use outputClientPipe = new AnonymousPipeClientStream(outputServerPipe.GetClientHandleAsString())

    use inputWriter = new StreamWriter(inputServerPipe)
    inputWriter.AutoFlush <- true
    use outputReader = new StreamReader(outputClientPipe)
    let server = async {
        let result = Server.start(inputClientPipe, outputServerPipe)
        if result <> 0 then
            Assert.Fail("Server startup failed")
    }
    
    let! serverAsync = Async.StartChild(server)

    inputWriter.Write(prependContentLength(Requests.initialize(
        Path.Join(Directory.GetCurrentDirectory(), "Solutions", "Basic", "Basic.sln"))))
    let initializeOutput = readOutput(outputReader)
    initializeOutput.Should().NotBeNull("", []) |> ignore
    
    inputWriter.Write(prependContentLength(Requests.shutdown()))
    inputWriter.Write(prependContentLength(Requests.exit()))

    do! serverAsync
}

[<Fact>]
let ``server responds with data when requesting textDocument/documentSymbol`` () = async {
    use inputServerPipe = new AnonymousPipeServerStream()
    use inputClientPipe = new AnonymousPipeClientStream(inputServerPipe.GetClientHandleAsString())
    use outputServerPipe = new AnonymousPipeServerStream()
    use outputClientPipe = new AnonymousPipeClientStream(outputServerPipe.GetClientHandleAsString())

    use inputWriter = new StreamWriter(inputServerPipe)
    inputWriter.AutoFlush <- true
    use outputReader = new StreamReader(outputClientPipe)
    let server = async {
        let result = Server.start(inputClientPipe, outputServerPipe)
        if result <> 0 then
            Assert.Fail("Server startup failed")
    }

    let! serverAsync = Async.StartChild(server)

    inputWriter.Write(prependContentLength(Requests.textDocumentDocumentSymbol()))
    let textDocumentDocumentSymbol = readOutput(outputReader)
    textDocumentDocumentSymbol.Should().Be(
        @"{""jsonrpc"":""2.0"",""id"":1,""error"":{""code"":-32602,""message"":""Unable to find method 'textDocument/documentSymbol/0' on {no object} for the following reasons: An argument was not supplied for a required parameter.""}}", "", []) |> ignore

    inputWriter.Write(prependContentLength(Requests.shutdown()))
    inputWriter.Write(prependContentLength(Requests.exit()))

    do! serverAsync
}

[<Fact>]
let ``server responds with null when requesting initialized`` () = async {
    use inputServerPipe = new AnonymousPipeServerStream()
    use inputClientPipe = new AnonymousPipeClientStream(inputServerPipe.GetClientHandleAsString())
    use outputServerPipe = new AnonymousPipeServerStream()
    use outputClientPipe = new AnonymousPipeClientStream(outputServerPipe.GetClientHandleAsString())

    use inputWriter = new StreamWriter(inputServerPipe)
    inputWriter.AutoFlush <- true
    use outputReader = new StreamReader(outputClientPipe)
    let server = async {
        let result = Server.start(inputClientPipe, outputServerPipe)
        if result <> 0 then
            Assert.Fail("Server startup failed")
    }

    let! serverAsync = Async.StartChild(server)

    inputWriter.Write(prependContentLength(Requests.initialize(
        Path.Join(Directory.GetCurrentDirectory(), "Solutions", "Basic", "Basic.sln"))))
    let initializeOutput = readOutput(outputReader)
    initializeOutput.Should().NotBeNull("", []) |> ignore

    inputWriter.Write(prependContentLength(Requests.initialized()))
    let initializedOutput = readOutput(outputReader)
    initializedOutput.Should().Be(@"{""jsonrpc"":""2.0"",""id"":10,""result"":null}", "", []) |> ignore

    inputWriter.Write(prependContentLength(Requests.shutdown()))
    inputWriter.Write(prependContentLength(Requests.exit()))

    do! serverAsync
}

[<Fact>]
let ``server responds with content when requesting textDocument/hover`` () = async {
    use inputServerPipe = new AnonymousPipeServerStream()
    use inputClientPipe = new AnonymousPipeClientStream(inputServerPipe.GetClientHandleAsString())
    use outputServerPipe = new AnonymousPipeServerStream()
    use outputClientPipe = new AnonymousPipeClientStream(outputServerPipe.GetClientHandleAsString())

    use inputWriter = new StreamWriter(inputServerPipe)
    inputWriter.AutoFlush <- true
    use outputReader = new StreamReader(outputClientPipe)
    let server = async {
        let result = Server.start(inputClientPipe, outputServerPipe)
        if result <> 0 then
            Assert.Fail("Server startup failed")
    }

    let! serverAsync = Async.StartChild(server)

    inputWriter.Write(prependContentLength(Requests.initialize(
        Path.Join(Directory.GetCurrentDirectory(), "Solutions", "Basic", "Basic.sln"))))
    let initializeOutput = readOutput(outputReader)
    initializeOutput.Should().NotBeNull("", []) |> ignore

    inputWriter.Write(prependContentLength(Requests.textDocumentHover()))
    let hoverOutput = readOutput(outputReader)
    hoverOutput.Should().Be(@"{""jsonrpc"":""2.0"",""id"":10,""result"":{""contents"":""Hello world""}}", "", []) |> ignore

    inputWriter.Write(prependContentLength(Requests.shutdown()))
    inputWriter.Write(prependContentLength(Requests.exit()))

    do! serverAsync
}