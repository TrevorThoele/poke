module PokeTests

open Xunit
open poke.Program
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
        [|"bool value"; bigint(2)|]
        [|"char value"; bigint(256)|]
        [|"sbyte value"; bigint(256)|]
        [|"byte value"; bigint(256)|]
        [|"short value"; bigint(65536)|]
        [|"ushort value"; bigint(65536)|]
        [|"int value"; bigint.Parse("4294967296")|]
        [|"uint value"; bigint.Parse("4294967296")|]
        [|"float value"; bigint.Parse("4294967296")|]
        [|"long value"; bigint.Parse("18446744073709551616")|]
        [|"ulong value"; bigint.Parse("18446744073709551616")|]
        [|"double value"; bigint.Parse("18446744073709551616")|]
        [|"decimal value"; bigint.Parse("340282366920938463463374607431768211456")|]
    ]

[<Theory>]
[<MemberData(nameof(declaredStateSpaceScenarios))>]
let ``variable has correct stateSpace`` (text: string, expectedResult: bigint) =
    let syntaxTree = CSharpSyntaxTree.ParseText(text)
    let mscorlib = MetadataReference.CreateFromFile(typedefof<int>.Assembly.Location)
    let compilation = CSharpCompilation.Create("MyCompilation", [syntaxTree], [mscorlib])
    let model = compilation.GetSemanticModel(syntaxTree, false)
    let root = model.SyntaxTree.GetCompilationUnitRoot()

    let valueVariable = (variables(root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "value"))

    let result = declaredStateSpace(model.GetDeclaredSymbol(valueVariable), model)
    result.Should().Be(expectedResult, "", [])

let functionStateSpaceScenarios: obj [] list =
    [
        [|"void MyFunction(bool value) {}"; bigint(2)|]
        [|"void MyFunction(char value) {}"; bigint(256)|]
        [|"void MyFunction(sbyte value) {}"; bigint(256)|]
        [|"void MyFunction(byte value) {}"; bigint(256)|]
        [|"void MyFunction(short value) {}"; bigint(65536)|]
        [|"void MyFunction(ushort value) {}"; bigint(65536)|]
        [|"void MyFunction(int value) {}"; bigint.Parse("4294967296")|]
        [|"void MyFunction(uint value) {}"; bigint.Parse("4294967296")|]
        [|"void MyFunction(float value) {}"; bigint.Parse("4294967296")|]
        [|"void MyFunction(long value) {}"; bigint.Parse("18446744073709551616")|]
        [|"void MyFunction(ulong value) {}"; bigint.Parse("18446744073709551616")|]
        [|"void MyFunction(double value) {}"; bigint.Parse("18446744073709551616")|]
        [|"void MyFunction(decimal value) {}"; bigint.Parse("340282366920938463463374607431768211456")|]
    ]

[<Theory>]
[<MemberData(nameof(functionStateSpaceScenarios))>]
let ``function has correct domain`` (text: string, expectedResult: bigint) =
    let syntaxTree = CSharpSyntaxTree.ParseText(text)
    let mscorlib = MetadataReference.CreateFromFile(typedefof<int>.Assembly.Location)
    let compilation = CSharpCompilation.Create("MyCompilation", [syntaxTree], [mscorlib])
    let model = compilation.GetSemanticModel(syntaxTree, false)
    let root = model.SyntaxTree.GetCompilationUnitRoot()

    let mainMethod = (localFunctions(root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "MyFunction"))
    let functionStateSpace = functionStateSpace(mainMethod, model)

    functionStateSpace.Domain.Should().Be(expectedResult, "", []) |> ignore