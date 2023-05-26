module ServerTests

open Xunit
open FluentAssertions
open System
open System.Diagnostics
open System.IO

(*
let parseUntilResponse<'T>(reader: StreamReader, totalOutput: string) =
    let output = reader.ReadLine()
    if output.Contains("Content-Length")

let parseUntilResponse<'T>(reader: StreamReader) =
    parseUntilResponse(reader, "")
    *)

[<Fact>]
let ``server responds with data when requesting textDocument/documentSymbol`` () =
    let server = new Process(StartInfo = ProcessStartInfo(
        CreateNoWindow = true,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        FileName = $"{Environment.CurrentDirectory}/Server.exe"))
    let started = server.Start()
    if not started then
        Assert.Fail("Server not started")

    (*{""jsonrpc"":""2.0"",""method"":""textDocument/documentSymbol"",""params"":{""textDocument"":{""uri"":""source://src/main/scala/Address.scala""}},""id"":10}*)
    (*{ ""jsonrpc"":""2.0"", ""method"": ""textDocument/documentSymbol"", ""id"":1 }*)
    let command = @"{ ""jsonrpc"":""2.0"", ""method"": ""textDocument/documentSymbol"", ""id"":1 }"
    let contentLength = command.Length

    server.StandardInput.Write(@$"Content-Length: {contentLength}

{command}")

    let chars: char array = Array.zeroCreate 10
    let test = server.StandardOutput.Read(chars, 0, 10)

    let output = server.StandardOutput.ReadLine()
    output.Should().Be(@"{ TextDocument = { Uri = ""source://src/main/scala/Address.scala"" } }Content-Length: 39", "", []) |> ignore

[<Fact>]
let ``server responds with data when requesting textDocument/hover`` () =
    let server = new Process(StartInfo = ProcessStartInfo(
        CreateNoWindow = true,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        FileName = $"{Environment.CurrentDirectory}/Server.exe"))
    let started = server.Start()
    if not started then
        Assert.Fail("Server not started")
        
    let command = @"{""jsonrpc"":""2.0"",""method"":""textDocument/hover"",""params"":{""position"":{""line"":0,""character"":0},""textDocument"":{""uri"":""source://src/main/scala/Address.scala""}},""id"":10}"
    let contentLength = command.Length

    server.StandardInput.Write(@$"Content-Length: {contentLength}

{command}")

    let mutable output = ""

    while (not server.StandardOutput.EndOfStream) do output <- output + (server.StandardOutput.Read() |> char).ToString()

    output.Should().Be(@"{ TextDocument = { Uri = ""source://src/main/scala/Address.scala"" }", "", []) |> ignore