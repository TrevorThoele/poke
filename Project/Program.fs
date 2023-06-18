module poke.Program

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open System

type FunctionStateSpace = {
    Domain: bigint
    Codomain: bigint
}

type Source = {
    Root: CompilationUnitSyntax
    Model: SemanticModel
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

let rec declaredStateSpace(symbol: ISymbol, model: SemanticModel): bigint =
    match symbol with
    | :? IArrayTypeSymbol as array -> declaredStateSpace(array.ElementType, model)
    | :? ILocalSymbol as local -> declaredStateSpace(local.Type, model)
    | :? IFieldSymbol as field -> declaredStateSpace(field.Type, model)
    | :? INamedTypeSymbol as namedType ->
        match namedType.SpecialType with
        | SpecialType.None -> ((bigint(1), namedType.GetMembers() |> Seq.filter(examinableMember) |> Seq.append([namedType.BaseType :> ISymbol]))
            ||> Seq.fold(fun accumulator x ->
                accumulator * declaredStateSpace(x, model)))
        | _ -> basicStateSpace(symbol.ToString())
    | _ -> bigint(1)

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

let assignments(node: SyntaxNode) =
    typedDescendentNodes<AssignmentExpressionSyntax>(node)

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