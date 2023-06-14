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
open Microsoft.Build.Locator

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

type Input<'TParameters> = {
    Parameters: 'TParameters
    Workspace: Workspace option
}

type Output<'TResponse> = {
    Response: LspResult<'TResponse>
    Workspace: Workspace option
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

let initialize(input: Input<InitializeParams>): Async<Output<InitializeResult>> = async {
    MSBuildLocator.RegisterDefaults() |> ignore
    let workspace = MSBuildWorkspace.Create()
    let! _ = (workspace.OpenSolutionAsync(input.Parameters.RootPath.Value) |> Async.AwaitTask)
    return {
        Response = LspResult.Ok({
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
        });
        Workspace = Some(workspace)
    }
}

let initialized(input: Input<InitializedParams>): Async<Output<unit>> = async {
    return {
        Response = LspResult.Ok();
        Workspace = input.Workspace
    }
}

let textDocumentDidOpen(input: Input<Types.DidOpenTextDocumentParams>): Async<Output<unit>> = async {
    return {
        Response = LspResult.Ok();
        Workspace = input.Workspace
    }
}

let textDocumentDidChange(input: Input<Types.DidChangeTextDocumentParams>): Async<Output<unit>> = async {
    return {
        Response = LspResult.Ok();
        Workspace = input.Workspace
    }
}

let textDocumentDidSave(input: Input<Types.DidSaveTextDocumentParams>): Async<Output<unit>> = async {
    return {
        Response = LspResult.Ok();
        Workspace = input.Workspace
    }
}

let textDocumentDidClose(input: Input<Types.DidCloseTextDocumentParams>): Async<Output<unit>> = async {
    return {
        Response = LspResult.Ok();
        Workspace = input.Workspace
    }
}

let textDocumentCodeAction(input: Input<Types.CodeActionParams>): Async<Output<Types.TextDocumentCodeActionResult option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let codeActionResolve(input: Input<CodeAction>): Async<Output<CodeAction option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentCodeLens(input: Input<CodeLensParams>): Async<Output<CodeLens[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let codeLensResolve(input: Input<CodeLens>): Async<Output<CodeLens>> = async {
    return {
        Response = LspResult.Ok({
            input.Parameters with Command = None
        });
        Workspace = input.Workspace
    }
}

let textDocumentDefinition(input: Input<Types.TextDocumentPositionParams>): Async<Output<Types.GotoResult option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentImplementation(input: Input<Types.TextDocumentPositionParams>): Async<Output<Types.GotoResult option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentCompletion(input: Input<Types.CompletionParams>): Async<Output<Types.CompletionList option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentDocumentHighlight(input: Input<Types.TextDocumentPositionParams>): Async<Output<Types.DocumentHighlight[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentDocumentSymbol(input: Input<Types.DocumentSymbolParams>): Async<Output<Types.DocumentSymbol[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentHover(input: Input<Types.TextDocumentPositionParams>): Async<Output<Types.Hover option>> = async {
    (*
    let solution = workspace.Value.CurrentSolution
    let documentId = (solution.GetDocumentIdsWithFilePath(input.Parameters.TextDocument.Uri)
        |> Seq.tryHead)
    let document = solution.GetDocument(documentId.Value)
    let! sourceText = document.GetTextAsync() |> Async.AwaitTask
    let text = sourceText.ToString()
    *)
    (*
    let lines = File.ReadLines(input.Parameters.TextDocument.Uri)
    let text = lines |> Seq.item(input.Parameters.Position.Line)
    let syntaxTree = CSharpSyntaxTree.ParseText(text)
    let mscorlib = MetadataReference.CreateFromFile(typedefof<int>.Assembly.Location)
    let compilation = CSharpCompilation.Create("MyCompilation", [syntaxTree], [mscorlib])
    let model = compilation.GetSemanticModel(syntaxTree, false)
    let root = model.SyntaxTree.GetCompilationUnitRoot()

    let methods = methods(root)
    let method = (localFunctions(root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "MyFunction"))
    *)
    return {
        Response = LspResult.Ok(Some {
            Contents = input.Parameters.TextDocument.Uri.ToString() |> MarkedString.String |> HoverContent.MarkedString
            Range = None
        });
        Workspace = input.Workspace
    }
}

let textDocumentReferences(input: Input<Types.ReferenceParams>): Async<Output<Types.Location[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentPrepareRename(input: Input<PrepareRenameParams>): Async<Output<PrepareRenameResult option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentRename(input: Input<Types.RenameParams>): Async<Output<Types.WorkspaceEdit option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentSignatureHelp(input: Input<Types.SignatureHelpParams>): Async<Output<Types.SignatureHelp option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let semanticTokensFull(input: Input<Types.SemanticTokensParams>): Async<Output<Types.SemanticTokens option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let semanticTokensRange(input: Input<Types.SemanticTokensRangeParams>): Async<Output<Types.SemanticTokens option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentInlayHint(input: Input<InlayHintParams>): Async<Output<InlayHint[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentPrepareTypeHierarchy(input: Input<TypeHierarchyPrepareParams>): Async<Output<TypeHierarchyItem[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let typeHierarchySupertypes(input: Input<TypeHierarchySupertypesParams>): Async<Output<TypeHierarchyItem[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let typeHierarchySubtypes(input: Input<TypeHierarchySubtypesParams>): Async<Output<TypeHierarchyItem[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentPrepareCallHierarchy(input: Input<CallHierarchyPrepareParams>): Async<Output<CallHierarchyItem[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let callHierarchyIncomingCalls(input: Input<CallHierarchyIncomingCallsParams>): Async<Output<CallHierarchyIncomingCall[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let callHierarchyOutgoingCalls(input: Input<CallHierarchyOutgoingCallsParams>): Async<Output<CallHierarchyOutgoingCall[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let workspaceSymbol(input: Input<Types.WorkspaceSymbolParams>): Async<Output<Types.SymbolInformation[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let workspaceDidChangeWatchedFiles(input: Input<Types.DidChangeWatchedFilesParams>): Async<Output<unit>> = async {
    return {
        Response = LspResult.Ok();
        Workspace = input.Workspace
    }
}

let workspaceDidChangeConfiguration(input: Input<DidChangeConfigurationParams>): Async<Output<unit>> = async {
    return {
        Response = LspResult.Ok();
        Workspace = input.Workspace
    }
}

let cSharpMetadata(input: Input<CSharpMetadataParams>): Async<Output<CSharpMetadataResponse option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentFormatting(input: Input<Types.DocumentFormattingParams>): Async<Output<Types.TextEdit[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentRangeFormatting(input: Input<DocumentRangeFormattingParams>): Async<Output<TextEdit[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let textDocumentOnTypeFormatting(input: Input<DocumentOnTypeFormattingParams>): Async<Output<TextEdit[] option>> = async {
    return {
        Response = LspResult.Ok(None);
        Workspace = input.Workspace
    }
}

let setupEndpoints(_: LspClient) =
    let mutable workspace: Workspace option = None

    let handleRequest(requestName, func) =
        let requestHandler parameters = async {
            let! output = func({
                Parameters = parameters;
                Workspace = workspace
            })

            workspace <- output.Workspace

            return output.Response
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
