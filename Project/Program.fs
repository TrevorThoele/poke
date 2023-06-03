open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open System

type FunctionStateSpace = {
    Domain: bigint
    Codomain: bigint
}

let declaredStateSpace(object: Type): bigint =
    match object with
        | _ when object = typeof<bool> -> bigint(2)
        | _ when object = typeof<char> -> bigint(Math.Pow(2, 8))
        | _ when object = typeof<sbyte> -> bigint(Math.Pow(2, 8))
        | _ when object = typeof<byte> -> bigint(Math.Pow(2, 8))
        | _ when object = typeof<int16> -> bigint(Math.Pow(2, 16))
        | _ when object = typeof<uint16> -> bigint(Math.Pow(2, 16))
        | _ when object = typeof<int> -> bigint(Math.Pow(2, 32))
        | _ when object = typeof<uint> -> bigint(Math.Pow(2, 32))
        | _ when object = typeof<single> -> bigint(Math.Pow(2, 32))
        | _ when object = typeof<int64> -> bigint(Math.Pow(2, 64))
        | _ when object = typeof<uint64> -> bigint(Math.Pow(2, 64))
        | _ when object = typeof<double> -> bigint(Math.Pow(2, 64))
        | _ when object = typeof<decimal> -> bigint(Math.Pow(2, 128))
        | _ when object = typeof<nativeint> -> bigint(Math.Pow(256, IntPtr.Size))
        | _ when object = typeof<unativeint> -> bigint(Math.Pow(256, UIntPtr.Size))
        | _ when object = typeof<unit> -> bigint(1)
        | _ -> bigint(0)

let rec declaredStateSpace2(symbol: ISymbol, model: SemanticModel): bigint =
    match symbol.Kind with
    | SymbolKind.ArrayType -> declaredStateSpace2((symbol :?> IArrayTypeSymbol).ElementType, model)
    | SymbolKind.NamedType ->
        match symbol.ToString() with
        | "string" -> bigint(1)
        | _ -> bigint(2)
    | _ -> bigint(0)

let private typedDescendentNodes<'T when 'T :> SyntaxNode>(node: SyntaxNode): seq<'T> =
    node.DescendantNodes()
        |> Seq.filter(fun x -> x :? 'T)
        |> Seq.cast<'T>

let methods(node: SyntaxNode) =
    typedDescendentNodes<MethodDeclarationSyntax>(node)

let variables(node: SyntaxNode) =
    typedDescendentNodes<VariableDeclaratorSyntax>(node)

let assignments(node: SyntaxNode) =
    typedDescendentNodes<AssignmentExpressionSyntax>(node)

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

let functionStateSpace(syntax: MethodDeclarationSyntax, model: SemanticModel) =
    syntax.ParameterList.Parameters |> Seq.map(fun x -> declaredStateSpace2(model.GetSymbolInfo(x.Type).Symbol, model))

    (*
let functionStateSpace(syntax: MethodDeclarationSyntax, model: SemanticModel): FunctionStateSpace2 =
    {
        Domain = (bigint(0), syntax.ParameterList.Parameters)
            ||> Seq.fold(fun accumulator x ->
                accumulator + declaredStateSpace(name(x, model)))
        Codomain = bigint(0)
    }
    *)