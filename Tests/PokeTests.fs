module PokeTests

open Xunit
open Program
open FluentAssertions
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis
open System

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

let declaredStateSpaceScenarios: obj [] list =
    [
        [|typeof<bool>; bigint(2)|]
        [|typeof<char>; bigint(256)|]
        [|typeof<sbyte>; bigint(256)|]
        [|typeof<byte>; bigint(256)|]
        [|typeof<int16>; bigint(65536)|]
        [|typeof<uint16>; bigint(65536)|]
        [|typeof<int>; bigint.Parse("4294967296")|]
        [|typeof<uint>; bigint.Parse("4294967296")|]
        [|typeof<single>; bigint.Parse("4294967296")|]
        [|typeof<int64>; bigint.Parse("18446744073709551616")|]
        [|typeof<uint64>; bigint.Parse("18446744073709551616")|]
        [|typeof<double>; bigint.Parse("18446744073709551616")|]
        [|typeof<decimal>; bigint.Parse("340282366920938463463374607431768211456")|]
        [|typeof<nativeint>; bigint(Math.Pow(256, IntPtr.Size))|]
        [|typeof<unativeint>; bigint(Math.Pow(256, IntPtr.Size))|]
        [|typeof<unit>; bigint(1)|]
    ]

[<Theory>]
[<MemberData(nameof(declaredStateSpaceScenarios))>]
let ``type has correct stateSpace`` (value: Type, expectedResult: bigint) =
    let result = declaredStateSpace(value)
    result.Should().Be(expectedResult, "", [])

[<Fact>]
let ``function has correct domain`` () =
    let syntaxTree = CSharpSyntaxTree.ParseText(testProgram)
    let mscorlib = MetadataReference.CreateFromFile(typedefof<int>.Assembly.Location)
    let compilation = CSharpCompilation.Create("MyCompilation", [syntaxTree], [mscorlib])
    let model = compilation.GetSemanticModel(syntaxTree, false)
    let root = model.SyntaxTree.GetCompilationUnitRoot()

    let mainMethod = (methods(root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "Main"))
    let functionStateSpace = functionStateSpace(mainMethod, model)

    "1".Should().Be(
        "1",
        "",
        []) |> ignore