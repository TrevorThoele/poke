module poke.Program

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open System
open Microsoft.CodeAnalysis.FindSymbols
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.Build.Locator
open Microsoft.CodeAnalysis.Text

type FunctionStateSpace = {
    Domain: bigint
    Codomain: bigint
}

type Source = {
    Root: CompilationUnitSyntax
    Model: SemanticModel
}

type Analyzed = {
    Symbol: ISymbol
    StateSpace: bigint
}

type Analysis = {
    mutable Symbols: Analyzed seq
}

let basicStateSpace(check: string) =
    match check with
    | "bool" -> bigint(2)
    | "char" -> bigint(Math.Pow(2, 8))
    | "sbyte" -> bigint(Math.Pow(2, 8))
    | "byte" -> bigint(Math.Pow(2, 8))
    | "short" -> bigint(Math.Pow(2, 16))
    | "ushort" -> bigint(Math.Pow(2, 16))
    | "int" -> bigint(Math.Pow(2, 32))
    | "uint" -> bigint(Math.Pow(2, 32))
    | "float" -> bigint(Math.Pow(2, 32))
    | "long" -> bigint(Math.Pow(2, 64))
    | "ulong" -> bigint(Math.Pow(2, 64))
    | "double" -> bigint(Math.Pow(2, 64))
    | "decimal" -> bigint(Math.Pow(2, 128))
    | "void" -> bigint(1)
    | _ -> bigint(1)

let examinableMember(symbol: ISymbol): bool =
    not(symbol :? IMethodSymbol) && not(symbol :? IPropertySymbol)

let rec classMembers(symbol: INamedTypeSymbol): ISymbol seq =
    if isNull(symbol)
        then []
        else symbol.GetMembers() |> Seq.filter(examinableMember) |> Seq.append(classMembers(symbol.BaseType))

let enumStateSpace(symbol: INamedTypeSymbol): bigint =
    let caseCount = symbol.GetMembers() |> Seq.filter(examinableMember) |> Seq.length
    (if caseCount = 0 then 1 else caseCount) |> bigint

let rec declaredStateSpace(symbol: ISymbol, model: SemanticModel): bigint =
    let (stateSpace, isNullable) =
        match symbol with
        | :? IArrayTypeSymbol as array ->
            (declaredStateSpace(array.ElementType, model), false)
        | :? ILocalSymbol as local ->
            (declaredStateSpace(local.Type, model), false)
        | :? IFieldSymbol as field ->
            (declaredStateSpace(field.Type, model), false)
        | :? INamedTypeSymbol as namedType ->
            match (namedType.TypeKind, namedType.SpecialType) with
            | (TypeKind.Enum, _) ->
                (enumStateSpace(namedType), false)
            | (_, SpecialType.None) -> (
                ((bigint(1), classMembers(namedType)) ||> Seq.fold(fun total x -> total * declaredStateSpace(x, model))),
                namedType.IsReferenceType)
            | _ ->
                (basicStateSpace(symbol.ToString()), false)
        | _ ->
            (bigint(1), false)
    stateSpace + if isNullable then bigint(1) else bigint(0)

let parseSource(text: string) =
    let syntaxTree = CSharpSyntaxTree.ParseText(text)
    let mscorlib = MetadataReference.CreateFromFile(typedefof<int>.Assembly.Location)
    let compilation = CSharpCompilation.Create("MyCompilation", [syntaxTree], [mscorlib])
    let model = compilation.GetSemanticModel(syntaxTree, false)
    let root = model.SyntaxTree.GetCompilationUnitRoot()
    {
        Root = root;
        Model = model
    }

let instantiatedStateSpace(node: SyntaxNode) =
    match node with
    | :? VariableDeclaratorSyntax as variableDeclarator ->
        match variableDeclarator.Initializer.Value with
        | :? LiteralExpressionSyntax -> bigint(1)
        | _ -> bigint(0)
    | _ -> bigint(0)

let private typedDescendentNodes<'T when 'T :> SyntaxNode>(node: SyntaxNode): seq<'T> =
    node.DescendantNodes()
        |> Seq.filter(fun x -> x :? 'T)
        |> Seq.cast<'T>

let methods(node: SyntaxNode) =
    typedDescendentNodes<MethodDeclarationSyntax>(node)

let variables(node: SyntaxNode) =
    typedDescendentNodes<VariableDeclaratorSyntax>(node)

let classes(node: SyntaxNode) =
    typedDescendentNodes<ClassDeclarationSyntax>(node)

let structs(node: SyntaxNode) =
    typedDescendentNodes<StructDeclarationSyntax>(node)

let enums(node: SyntaxNode) =
    typedDescendentNodes<EnumDeclarationSyntax>(node)

let assignments(node: SyntaxNode) =
    typedDescendentNodes<AssignmentExpressionSyntax>(node)

let variableDeclarations(node: SyntaxNode) =
    typedDescendentNodes<VariableDeclarationSyntax>(node)

let localFunctions(node: SyntaxNode) =
    typedDescendentNodes<LocalFunctionStatementSyntax>(node)

let localVariables(node: SyntaxNode) =
    methods(node)
        |> Seq.collect(variables)

let accessedExternalVariables(node: SyntaxNode, model: SemanticModel) =
    let locals = (localVariables(node)
        |> Seq.map(model.GetDeclaredSymbol))
    assignments(node)
        |> Seq.map(fun x -> model.GetSymbolInfo(x.Left))
        |> Seq.filter(fun x -> x.Symbol <> null)
        |> Seq.map(fun x -> x.Symbol)
        |> Seq.filter(fun x -> not (locals |> Seq.exists(fun y -> y.Equals(x))))

let declaredFunctionStateSpace(syntax: LocalFunctionStatementSyntax, model: SemanticModel): FunctionStateSpace = {
    Domain = ((bigint(1), syntax.ParameterList.Parameters)
        ||> Seq.fold(fun accumulator x ->
            accumulator * declaredStateSpace(model.GetSymbolInfo(x.Type).Symbol, model)))
    Codomain = declaredStateSpace(model.GetSymbolInfo(syntax.ReturnType).Symbol, model)
}