module CSharpLanguageServer.Server

open System
open Ionide.LanguageServerProtocol
open Ionide.LanguageServerProtocol.Server
open Ionide.LanguageServerProtocol.Types
open System.Diagnostics
open System.Threading

let success = LspResult.success

type CSharpMetadataParams = {
    TextDocument: TextDocumentIdentifier
}

type CSharpMetadataResponse = CSharpMetadata

type CSharpLspClient(sendServerNotification: ClientNotificationSender, sendServerRequest: ClientRequestSender) =
    inherit LspClient ()

    override __.WindowShowMessage(p) =
        sendServerNotification "window/showMessage" (box p) |> Async.Ignore

    override __.WindowShowMessageRequest(p) =
        sendServerRequest.Send "window/showMessageRequest" (box p)

    override __.WindowLogMessage(p) =
        sendServerNotification "window/logMessage" (box p) |> Async.Ignore

    override __.TelemetryEvent(p) =
        sendServerNotification "telemetry/event" (box p) |> Async.Ignore

    override __.ClientRegisterCapability(p) =
        sendServerRequest.Send "client/registerCapability" (box p)

    override __.ClientUnregisterCapability(p) =
        sendServerRequest.Send "client/unregisterCapability" (box p)

    override __.WorkspaceWorkspaceFolders () =
        sendServerRequest.Send "workspace/workspaceFolders" ()

    override __.WorkspaceConfiguration (p) =
        sendServerRequest.Send "workspace/configuration" (box p)

    override __.WorkspaceApplyEdit (p) =
        sendServerRequest.Send "workspace/applyEdit" (box p)

    override __.WorkspaceSemanticTokensRefresh () =
        sendServerNotification "workspace/semanticTokens/refresh" () |> Async.Ignore

    override __.TextDocumentPublishDiagnostics(p) =
        sendServerNotification "textDocument/publishDiagnostics" (box p) |> Async.Ignore

type Data<'TParameters> = {
    Parameters: 'TParameters
    LspClient: LspClient
}

let initialize(data: Data<InitializeParams>): AsyncLspResult<InitializeResult> = async {
    return InitializeResult.Default |> success
}

let initialized(data: Data<InitializedParams>): Async<LspResult<unit>> = async {
    return LspResult.Ok()
}

let textDocumentDidOpen(data: Data<Types.DidOpenTextDocumentParams>): Async<LspResult<unit>> = async {
    return LspResult.Ok()
}

let textDocumentDidChange(data: Data<Types.DidChangeTextDocumentParams>): Async<LspResult<unit>> = async {
    return LspResult.Ok()
}

let textDocumentDidSave(data: Data<Types.DidSaveTextDocumentParams>): Async<LspResult<unit>> = async {
    return LspResult.Ok()
}

let textDocumentDidClose(data: Data<Types.DidCloseTextDocumentParams>): Async<LspResult<unit>> = async {
    return LspResult.Ok()
}

let textDocumentCodeAction(data: Data<Types.CodeActionParams>): AsyncLspResult<Types.TextDocumentCodeActionResult option> = async {
    return LspResult.Ok(None)
}

let codeActionResolve(data: Data<CodeAction>): AsyncLspResult<CodeAction option> = async {
    return LspResult.Ok(None)
}

let textDocumentCodeLens(data: Data<CodeLensParams>): AsyncLspResult<CodeLens[] option> = async {
    return LspResult.Ok(None)
}

let codeLensResolve(data: Data<CodeLens>): AsyncLspResult<CodeLens> = async {
    return LspResult.Ok({
        data.Parameters with Command = None
    })
}

let textDocumentDefinition(data: Data<Types.TextDocumentPositionParams>): AsyncLspResult<Types.GotoResult option> = async {
    return LspResult.Ok(None)
}

let textDocumentImplementation(data: Data<Types.TextDocumentPositionParams>): AsyncLspResult<Types.GotoResult option> = async {
    return LspResult.Ok(None)
}

let textDocumentCompletion(data: Data<Types.CompletionParams>): AsyncLspResult<Types.CompletionList option> = async {
    return LspResult.Ok(None)
}

let textDocumentDocumentHighlight(data: Data<Types.TextDocumentPositionParams>): AsyncLspResult<Types.DocumentHighlight[] option> = async {
    return LspResult.Ok(None)
}

let textDocumentDocumentSymbol(data: Data<Types.DocumentSymbolParams>): AsyncLspResult<Types.DocumentSymbol[] option> = async {
    return LspResult.Ok(None)
}

let textDocumentHover(data: Data<Types.TextDocumentPositionParams>): AsyncLspResult<Types.Hover option> = async {
    return LspResult.Ok(None)
}

let textDocumentReferences(data: Data<Types.ReferenceParams>): AsyncLspResult<Types.Location[] option> = async {
    return LspResult.Ok(None)
}

let textDocumentPrepareRename(data: Data<PrepareRenameParams>): AsyncLspResult<PrepareRenameResult option> = async {
    return LspResult.Ok(None)
}

let textDocumentRename(data: Data<Types.RenameParams>): AsyncLspResult<Types.WorkspaceEdit option> = async {
    return LspResult.Ok(None)
}

let textDocumentSignatureHelp(data: Data<Types.SignatureHelpParams>): AsyncLspResult<Types.SignatureHelp option> = async {
    return LspResult.Ok(None)
}

let semanticTokensFull(data: Data<Types.SemanticTokensParams>): AsyncLspResult<Types.SemanticTokens option> = async {
    return LspResult.Ok(None)
}

let semanticTokensRange(data: Data<Types.SemanticTokensRangeParams>): AsyncLspResult<Types.SemanticTokens option> = async {
    return LspResult.Ok(None)
}

let textDocumentInlayHint(data: Data<InlayHintParams>): AsyncLspResult<InlayHint[] option> = async {
    return LspResult.Ok(None)
}

let textDocumentPrepareTypeHierarchy(data: Data<TypeHierarchyPrepareParams>): AsyncLspResult<TypeHierarchyItem[] option> = async {
    return LspResult.Ok(None)
}

let typeHierarchySupertypes(data: Data<TypeHierarchySupertypesParams>): AsyncLspResult<TypeHierarchyItem[] option> = async {
    return LspResult.Ok(None)
}

let typeHierarchySubtypes(data: Data<TypeHierarchySubtypesParams>): AsyncLspResult<TypeHierarchyItem[] option> = async {
    return LspResult.Ok(None)
}

let textDocumentPrepareCallHierarchy(data: Data<CallHierarchyPrepareParams>): AsyncLspResult<CallHierarchyItem[] option> = async {
    return LspResult.Ok(None)
}

let callHierarchyIncomingCalls(data: Data<CallHierarchyIncomingCallsParams>): AsyncLspResult<CallHierarchyIncomingCall[] option> = async {
    return LspResult.Ok(None)
}

let callHierarchyOutgoingCalls(data: Data<CallHierarchyOutgoingCallsParams>): AsyncLspResult<CallHierarchyOutgoingCall[] option> = async {
    return LspResult.Ok(None)
}

let workspaceSymbol(data: Data<Types.WorkspaceSymbolParams>): AsyncLspResult<Types.SymbolInformation[] option> = async {
    return LspResult.Ok(None)
}

let workspaceDidChangeWatchedFiles(data: Data<Types.DidChangeWatchedFilesParams>): Async<LspResult<unit>> = async {
    return LspResult.Ok()
}

let workspaceDidChangeConfiguration(data: Data<DidChangeConfigurationParams>): Async<LspResult<unit>> = async {
    return LspResult.Ok()
}

let cSharpMetadata(data: Data<CSharpMetadataParams>): AsyncLspResult<CSharpMetadataResponse option> = async {
    return LspResult.Ok(None)
}

let textDocumentFormatting(data: Data<Types.DocumentFormattingParams>): AsyncLspResult<Types.TextEdit[] option> = async {
    return LspResult.Ok(None)
}

let textDocumentRangeFormatting(data: Data<DocumentRangeFormattingParams>): AsyncLspResult<TextEdit[] option> = async {
    return LspResult.Ok(None)
}

let textDocumentOnTypeFormatting(data: Data<DocumentOnTypeFormattingParams>): AsyncLspResult<TextEdit[] option> = async {
    return LspResult.Ok(None)
}

let setupEndpoints(lspClient: LspClient) =
    let handleRequest nameAndAsyncFn =
        let requestName = nameAndAsyncFn |> fst
        let asyncFn = nameAndAsyncFn |> snd

        let requestHandler param = async {
            System.Console.Write(param |> string)
            return! asyncFn param
        }

        (requestName, requestHandler |> requestHandling)

    let on(func: (Data<'T>) -> AsyncLspResult<'U>): ('T -> Async<LspResult<'U>>) = fun (parameters: 'T) ->
        func({
            Parameters = parameters;
            LspClient = lspClient
        })

    [
        ("initialize"                       , on(initialize))                       |> handleRequest
        ("initialized"                      , on(initialized))                      |> handleRequest
        ("textDocument/didOpen"             , on(textDocumentDidOpen))              |> handleRequest
        ("textDocument/didChange"           , on(textDocumentDidChange))            |> handleRequest
        ("textDocument/didClose"            , on(textDocumentDidClose))             |> handleRequest
        ("textDocument/didSave"             , on(textDocumentDidSave))              |> handleRequest
        ("textDocument/codeAction"          , on(textDocumentCodeAction))           |> handleRequest
        ("codeAction/resolve"               , on(codeActionResolve))                |> handleRequest
        ("textDocument/codeLens"            , on(textDocumentCodeLens))             |> handleRequest
        ("codeLens/resolve"                 , on(codeLensResolve))                  |> handleRequest
        ("textDocument/completion"          , on(textDocumentCompletion))           |> handleRequest
        ("textDocument/definition"          , on(textDocumentDefinition))           |> handleRequest
        ("textDocument/documentHighlight"   , on(textDocumentDocumentHighlight))    |> handleRequest
        ("textDocument/documentSymbol"      , on(textDocumentDocumentSymbol))       |> handleRequest
        ("textDocument/hover"               , on(textDocumentHover))                |> handleRequest
        ("textDocument/implementation"      , on(textDocumentImplementation))       |> handleRequest
        ("textDocument/formatting"          , on(textDocumentFormatting))           |> handleRequest
        ("textDocument/onTypeFormatting"    , on(textDocumentOnTypeFormatting))     |> handleRequest
        ("textDocument/rangeFormatting"     , on(textDocumentRangeFormatting))      |> handleRequest
        ("textDocument/references"          , on(textDocumentReferences))           |> handleRequest
        ("textDocument/prepareRename"       , on(textDocumentPrepareRename))        |> handleRequest
        ("textDocument/rename"              , on(textDocumentRename))               |> handleRequest
        ("textDocument/signatureHelp"       , on(textDocumentSignatureHelp))        |> handleRequest
        ("textDocument/semanticTokens/full" , on(semanticTokensFull))               |> handleRequest
        ("textDocument/semanticTokens/range", on(semanticTokensRange))              |> handleRequest
        ("textDocument/inlayHint"           , on(textDocumentInlayHint))            |> handleRequest
        ("textDocument/prepareTypeHierarchy", on(textDocumentPrepareTypeHierarchy)) |> handleRequest
        ("typeHierarchy/supertypes"         , on(typeHierarchySupertypes))          |> handleRequest
        ("typeHierarchy/subtypes"           , on(typeHierarchySubtypes))            |> handleRequest
        ("textDocument/prepareCallHierarchy", on(textDocumentPrepareCallHierarchy)) |> handleRequest
        ("callHierarchy/incomingCalls"      , on(callHierarchyIncomingCalls))       |> handleRequest
        ("callHierarchy/outgoingCalls"      , on(callHierarchyOutgoingCalls))       |> handleRequest
        ("workspace/symbol"                 , on(workspaceSymbol))                  |> handleRequest
        ("workspace/didChangeWatchedFiles"  , on(workspaceDidChangeWatchedFiles))   |> handleRequest
        ("workspace/didChangeConfiguration" , on(workspaceDidChangeConfiguration))  |> handleRequest
        ("csharp/metadata"                  , on(cSharpMetadata))                   |> handleRequest
    ]
    |> Map.ofList

let startCore =
    use input = Console.OpenStandardInput()
    use output = Console.OpenStandardOutput()

    Ionide.LanguageServerProtocol.Server.startWithSetup
        setupEndpoints
        input
        output
        CSharpLspClient
        defaultRpc

let start() =
    try
        let result = startCore
        int result
    with
    | _ex ->
        3
