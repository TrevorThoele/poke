module CSharpLanguageServer.Server

open System
open Ionide.LanguageServerProtocol
open Ionide.LanguageServerProtocol.Server
open Ionide.LanguageServerProtocol.Types
open Microsoft.CodeAnalysis.Classification
open poke.Program
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis
open System.IO

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

let mutable workspace: Workspace option = None

type Data<'TParameters> = {
    Parameters: 'TParameters
    LspClient: LspClient
}

let ClassificationTypeMap = Map [
    (ClassificationTypeNames.ClassName,             "class");
    (ClassificationTypeNames.Comment,               "comment");
    (ClassificationTypeNames.ConstantName,          "property");
    (ClassificationTypeNames.ControlKeyword,        "keyword");
    (ClassificationTypeNames.DelegateName,          "class");
    (ClassificationTypeNames.EnumMemberName,        "enumMember");
    (ClassificationTypeNames.EnumName,              "enum");
    (ClassificationTypeNames.EventName,             "event");
    (ClassificationTypeNames.ExtensionMethodName,   "method");
    (ClassificationTypeNames.FieldName,             "property");
    (ClassificationTypeNames.Identifier,            "variable");
    (ClassificationTypeNames.InterfaceName,         "interface");
    (ClassificationTypeNames.LabelName,             "variable");
    (ClassificationTypeNames.LocalName,             "variable");
    (ClassificationTypeNames.Keyword,               "keyword");
    (ClassificationTypeNames.MethodName,            "method");
    (ClassificationTypeNames.NamespaceName,         "namespace");
    (ClassificationTypeNames.NumericLiteral,        "number");
    (ClassificationTypeNames.Operator,              "operator");
    (ClassificationTypeNames.OperatorOverloaded,    "operator");
    (ClassificationTypeNames.ParameterName,         "parameter");
    (ClassificationTypeNames.PropertyName,          "property");
    (ClassificationTypeNames.RecordClassName,       "class");
    (ClassificationTypeNames.RecordStructName,      "struct");
    (ClassificationTypeNames.RegexText,             "regex");
    (ClassificationTypeNames.StringLiteral,         "string");
    (ClassificationTypeNames.StructName,            "struct");
    (ClassificationTypeNames.TypeParameterName,     "typeParameter");
    (ClassificationTypeNames.VerbatimStringLiteral, "string")
]

let flip f x y = f y x

let SemanticTokenTypeMap =
    ClassificationTypeMap
    |> Map.values
    |> Seq.distinct
    |> flip Seq.zip (Seq.initInfinite uint32)
    |> Map.ofSeq

let SemanticTokenTypes =
    SemanticTokenTypeMap
    |> Seq.sortBy (fun kvp -> kvp.Value)
    |> Seq.map (fun kvp -> kvp.Key)

let ClassificationModifierMap = Map [
    (ClassificationTypeNames.StaticSymbol, "static")
]

let SemanticTokenModifierMap =
    ClassificationModifierMap
    |> Map.values
    |> Seq.distinct
    |> flip Seq.zip (Seq.initInfinite uint32)
    |> Map.ofSeq

let SemanticTokenModifiers =
    SemanticTokenModifierMap
    |> Seq.sortBy (fun kvp -> kvp.Value)
    |> Seq.map (fun kvp -> kvp.Key)

let initialize(data: Data<InitializeParams>): AsyncLspResult<InitializeResult> = async {
    let mutable createdWorkspace: Workspace option = None
    try
        createdWorkspace <- Some(MSBuildWorkspace.Create())
    with
    | e -> 3
    (*
    let! _ = (createdWorkspace.OpenSolutionAsync($"{data.Parameters.RootPath}/Test.sln") |> Async.AwaitTask)
    workspace <- Some(createdWorkspace)
    *)
    return LspResult.Ok({
        InitializeResult.Default with
            Capabilities = {
                ServerCapabilities.Default with
                    HoverProvider = Some true
                    RenameProvider = true |> First |> Some
                    DefinitionProvider = Some true
                    TypeDefinitionProvider = None
                    ImplementationProvider = Some true
                    ReferencesProvider = Some true
                    DocumentHighlightProvider = Some true
                    DocumentSymbolProvider = Some true
                    WorkspaceSymbolProvider = Some true
                    DocumentFormattingProvider = Some true
                    DocumentRangeFormattingProvider = Some true
                    DocumentOnTypeFormattingProvider = Some {
                        FirstTriggerCharacter = ';'
                        MoreTriggerCharacter = Some([| '}'; ')' |]) }
                    SignatureHelpProvider = Some {
                        TriggerCharacters = Some([| '('; ','; '<'; '{'; '[' |])
                        RetriggerCharacters = None }
                    CompletionProvider = Some {
                        ResolveProvider = None
                        TriggerCharacters = Some ([| '.'; '''; |])
                        AllCommitCharacters = None }
                    CodeLensProvider = Some { ResolveProvider = Some true }
                    CodeActionProvider = Some {
                        CodeActionKinds = None
                        ResolveProvider = Some true }
                    TextDocumentSync = Some {
                        TextDocumentSyncOptions.Default with
                            OpenClose = Some true
                            Save = Some { IncludeText = Some true }
                            Change = Some TextDocumentSyncKind.Incremental }
                    FoldingRangeProvider = None
                    SelectionRangeProvider = None
                    SemanticTokensProvider = Some {
                        Legend = {
                            TokenTypes = SemanticTokenTypes |> Seq.toArray
                            TokenModifiers = SemanticTokenModifiers |> Seq.toArray }
                        Range = Some true
                        Full = true |> First |> Some }
                    InlayHintProvider = Some { ResolveProvider = Some false }
                    TypeHierarchyProvider = Some true
                    CallHierarchyProvider = Some true
        }
    })
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
    (*
    let solution = workspace.Value.CurrentSolution
    let documentId = (solution.GetDocumentIdsWithFilePath(data.Parameters.TextDocument.Uri)
        |> Seq.tryHead)
    let document = solution.GetDocument(documentId.Value)
    let! sourceText = document.GetTextAsync() |> Async.AwaitTask
    let text = sourceText.ToString()
    *)
    (*
    let lines = File.ReadLines(data.Parameters.TextDocument.Uri)
    let text = lines |> Seq.item(data.Parameters.Position.Line)
    let syntaxTree = CSharpSyntaxTree.ParseText(text)
    let mscorlib = MetadataReference.CreateFromFile(typedefof<int>.Assembly.Location)
    let compilation = CSharpCompilation.Create("MyCompilation", [syntaxTree], [mscorlib])
    let model = compilation.GetSemanticModel(syntaxTree, false)
    let root = model.SyntaxTree.GetCompilationUnitRoot()

    let methods = methods(root)
    let method = (localFunctions(root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "MyFunction"))
    *)
    return LspResult.Ok(Some {
        Contents = data.Parameters.TextDocument.Uri.ToString() |> MarkedString.String |> HoverContent.MarkedString
        Range = None
    })
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
    let handleRequest(requestName, func) =
        let requestHandler parameters = async {
            return! func({
                Parameters = parameters;
                LspClient = lspClient
            })
        }

        (requestName, requestHandling(requestHandler))

    [
        handleRequest("initialize", initialize)
        handleRequest("initialized", initialized)
        handleRequest("textDocument/didOpen", textDocumentDidOpen)
        handleRequest("textDocument/didChange", textDocumentDidChange)
        handleRequest("textDocument/didClose", textDocumentDidClose)
        handleRequest("textDocument/didSave", textDocumentDidSave)
        handleRequest("textDocument/codeAction", textDocumentCodeAction)
        handleRequest("codeAction/resolve", codeActionResolve)
        handleRequest("textDocument/codeLens", textDocumentCodeLens)
        handleRequest("codeLens/resolve", codeLensResolve)
        handleRequest("textDocument/completion", textDocumentCompletion)
        handleRequest("textDocument/definition", textDocumentDefinition)
        handleRequest("textDocument/documentHighlight", textDocumentDocumentHighlight)
        handleRequest("textDocument/documentSymbol", textDocumentDocumentSymbol)
        handleRequest("textDocument/hover", textDocumentHover)
        handleRequest("textDocument/implementation", textDocumentImplementation)
        handleRequest("textDocument/formatting", textDocumentFormatting)
        handleRequest("textDocument/onTypeFormatting", textDocumentOnTypeFormatting)
        handleRequest("textDocument/rangeFormatting", textDocumentRangeFormatting)
        handleRequest("textDocument/references", textDocumentReferences)
        handleRequest("textDocument/prepareRename", textDocumentPrepareRename)
        handleRequest("textDocument/rename", textDocumentRename)
        handleRequest("textDocument/signatureHelp", textDocumentSignatureHelp)
        handleRequest("textDocument/semanticTokens/full", semanticTokensFull)
        handleRequest("textDocument/semanticTokens/range", semanticTokensRange)
        handleRequest("textDocument/inlayHint", textDocumentInlayHint)
        handleRequest("textDocument/prepareTypeHierarchy", textDocumentPrepareTypeHierarchy)
        handleRequest("typeHierarchy/supertypes", typeHierarchySupertypes)
        handleRequest("typeHierarchy/subtypes", typeHierarchySubtypes)
        handleRequest("textDocument/prepareCallHierarchy", textDocumentPrepareCallHierarchy)
        handleRequest("callHierarchy/incomingCalls", callHierarchyIncomingCalls)
        handleRequest("callHierarchy/outgoingCalls", callHierarchyOutgoingCalls)
        handleRequest("workspace/symbol", workspaceSymbol)
        handleRequest("workspace/didChangeWatchedFiles", workspaceDidChangeWatchedFiles)
        handleRequest("workspace/didChangeConfiguration", workspaceDidChangeConfiguration)
        handleRequest("csharp/metadata", cSharpMetadata)
    ]
    |> Map.ofList

let start(input: IO.Stream, output: IO.Stream) =
    try
        let result = (Ionide.LanguageServerProtocol.Server.startWithSetup
            setupEndpoints
            input
            output
            CSharpLspClient
            defaultRpc)

        int result
    with
    | _ex ->
        3
