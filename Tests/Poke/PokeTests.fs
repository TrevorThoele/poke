module PokeTests

open Xunit
open poke.Program
open FluentAssertions
open Microsoft.CodeAnalysis.CSharp

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
    let source = parseSource(testProgram)

    let accessedExternalVariables = (accessedExternalVariables(source.Root, source.Model)
        |> Seq.map(fun x -> x.Name))

    accessedExternalVariables.Should().BeEquivalentTo(
        [
            "staticString"
        ],
        "",
        []) |> ignore