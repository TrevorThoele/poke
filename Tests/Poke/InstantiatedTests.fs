module InstantiatedTests

open Xunit
open poke.Program
open FluentAssertions
open Microsoft.Build.Locator
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Text
open Microsoft.CodeAnalysis.FindSymbols

let literalAssignmentScenarios: obj [] list =
    [
        [|"bool value = true;"; 5; bigint.Parse("1")|]
        [|"bool value = false;"; 5; bigint.Parse("1")|]
        [|"char value = 'a';"; 5; bigint.Parse("1")|]
        [|"sbyte value = 1;"; 6; bigint.Parse("1")|]
        [|"byte value = 2;"; 5; bigint.Parse("1")|]
        [|"short value = 3;"; 6; bigint.Parse("1")|]
        [|"ushort value = 4;"; 7; bigint.Parse("1")|]
        [|"int value = 5;"; 4; bigint.Parse("1")|]
        [|"uint value = 6;"; 5; bigint.Parse("1")|]
        [|"float value = 7;"; 6; bigint.Parse("1")|]
        [|"long value = 8;"; 5; bigint.Parse("1")|]
        [|"ulong value = 9;"; 6; bigint.Parse("1")|]
        [|"double value = 10;"; 7; bigint.Parse("1")|]
        [|"decimal value = 11;"; 8; bigint.Parse("1")|]
    ]

[<Theory>]
[<MemberData(nameof(literalAssignmentScenarios))>]
let ``literal assignment has correct state space`` (text: string, position: int, expectedResult: bigint) = async {
    if (not MSBuildLocator.IsRegistered) then MSBuildLocator.RegisterDefaults() |> ignore
    let workspace = new AdhocWorkspace()
    let projectInfo = ProjectInfo.Create(
        ProjectId.CreateNewId(), VersionStamp.Create(), "TestProject", "TestProject", LanguageNames.CSharp)
    let project = workspace.AddProject(projectInfo)
    let document = workspace.AddDocument(project.Id, "File.cs", SourceText.From(text))
    
    let! symbol = SymbolFinder.FindSymbolAtPositionAsync(document, position) |> Async.AwaitTask
    let firstLocation = symbol.Locations |> Seq.head
    let node = firstLocation.SourceTree.GetRoot().FindNode(firstLocation.SourceSpan)
    let result = instantiatedStateSpace(node)
    result.Should().Be(expectedResult, "", []) |> ignore
}