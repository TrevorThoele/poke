module Tests

open Xunit
open Program
open FluentAssertions
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis

let testProgram = """using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace HelloWorld
{
    class Program
    {
        static string staticString = "zecks-string";

        static void Main(string[] args)
        {
            staticString = "different";
            var output = $"Hello, World!";
            Console.WriteLine(output);
        }
    }
}"""

[<Fact>]
let ``methods retrieves correct methods`` () =
    let syntaxTree = CSharpSyntaxTree.ParseText(testProgram)
    let root = syntaxTree.GetCompilationUnitRoot()

    let methods = (methods(root)
        |> Seq.map(fun x -> x.Identifier.ToString()))
    methods.Should().BeEquivalentTo(
        [
            "Main"
        ],
        "",
        []) |> ignore

[<Fact>]
let ``variables retrieves correct variables`` () =
    let syntaxTree = CSharpSyntaxTree.ParseText(testProgram)
    let root = syntaxTree.GetCompilationUnitRoot()

    let variables = (variables(root)
        |> Seq.map(fun x -> x.Identifier.ToString()))
    variables.Should().BeEquivalentTo(
        [
            "output";
            "staticString"
        ],
        "",
        []) |> ignore

[<Fact>]
let ``assignments retrieves correct assignments`` () =
    let syntaxTree = CSharpSyntaxTree.ParseText(testProgram)
    let root = syntaxTree.GetCompilationUnitRoot()

    let variables = (variables(root)
        |> Seq.map(fun x -> x.Identifier.ToString()))
    variables.Should().BeEquivalentTo(
        [
            "output";
            "staticString"
        ],
        "",
        []) |> ignore

[<Fact>]
let ``localVariables retrieves local variables`` () =
    let syntaxTree = CSharpSyntaxTree.ParseText(testProgram)
    let root = syntaxTree.GetCompilationUnitRoot()

    let localVariables = (localVariables(root)
        |> Seq.map(fun x -> x.Identifier.ToString()))

    localVariables.Should().BeEquivalentTo(
        [
            "output"
        ],
        "",
        []) |> ignore

[<Fact>]
let ``accessedExternalVariables retrieves external variable access`` () =
    let syntaxTree = CSharpSyntaxTree.ParseText(testProgram)
    let mscorlib = MetadataReference.CreateFromFile(typedefof<int>.Assembly.Location)
    let compilation = CSharpCompilation.Create("MyCompilation", [syntaxTree], [mscorlib])
    let model = compilation.GetSemanticModel(syntaxTree, false)
    let root = model.SyntaxTree.GetCompilationUnitRoot()

    let accessedExternalVariables = (accessedExternalVariables(root, model)
        |> Seq.map(fun x -> x.Name))

    accessedExternalVariables.Should().BeEquivalentTo(
        [
            "staticString"
        ],
        "",
        []) |> ignore