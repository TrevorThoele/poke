open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax

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