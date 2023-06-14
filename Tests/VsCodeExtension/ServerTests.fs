module ServerTests

open CSharpLanguageServer
open FluentAssertions
open System.IO
open System.IO.Pipes
open Xunit

let requestWithContentLength(request: string) = $"Content-Length: {request.Length}\r\n\r\n{request}"

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

    inputWriter.Write(requestWithContentLength(
        Requests.initialize(Path.Join(Directory.GetCurrentDirectory(), "Solutions", "Basic", "Basic.sln"))))

    let contentLengthLine = outputReader.ReadLine()
    let contentLength = int(contentLengthLine.Replace("Content-Length: ", ""))
    outputReader.ReadLine() |> ignore

    let contentChars: char array = Array.zeroCreate contentLength
    outputReader.Read(contentChars, 0, contentLength) |> ignore
    
    let output = new System.String(contentChars)
    output.Should().NotBeNull("", []) |> ignore
    
    inputWriter.Write(requestWithContentLength(Requests.shutdown))
    inputWriter.Write(requestWithContentLength(Requests.exit))

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

    inputWriter.Write(requestWithContentLength(Requests.textDocumentDocumentSymbol))

    let contentLengthLine = outputReader.ReadLine()
    let contentLength = int(contentLengthLine.Replace("Content-Length: ", ""))
    outputReader.ReadLine() |> ignore

    let contentChars: char array = Array.zeroCreate contentLength
    outputReader.Read(contentChars, 0, contentLength) |> ignore
    
    let output = new System.String(contentChars)
    output.Should().Be(@"{""jsonrpc"":""2.0"",""id"":1,""error"":{""code"":-32602,""message"":""Unable to find method 'textDocument/documentSymbol/0' on {no object} for the following reasons: An argument was not supplied for a required parameter.""}}", "", []) |> ignore

    inputWriter.Write(requestWithContentLength(Requests.shutdown))
    inputWriter.Write(requestWithContentLength(Requests.exit))

    do! serverAsync
}

[<Fact>]
let ``server responds with data when requesting textDocument/hover`` () = async {
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

    inputWriter.Write(requestWithContentLength(Requests.textDocumentHover))

    let contentLengthLine = outputReader.ReadLine()
    let contentLength = int(contentLengthLine.Replace("Content-Length: ", ""))
    outputReader.ReadLine() |> ignore

    let contentChars: char array = Array.zeroCreate contentLength
    outputReader.Read(contentChars, 0, contentLength) |> ignore
    
    let output = new System.String(contentChars)
    output.Should().Be(@"{""jsonrpc"":""2.0"",""id"":10,""result"":{""contents"":""Hello world""}}", "", []) |> ignore

    inputWriter.Write(requestWithContentLength(Requests.shutdown))
    inputWriter.Write(requestWithContentLength(Requests.exit))

    do! serverAsync
}