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
        [|"bool value = true;"; 5|]
        [|"bool value = false;"; 5|]
        [|"char value = 'a';"; 5|]
        [|"sbyte value = 1;"; 6|]
        [|"byte value = 2;"; 5|]
        [|"short value = 3;"; 6|]
        [|"ushort value = 4;"; 7|]
        [|"int value = 5;"; 4|]
        [|"uint value = 6;"; 5|]
        [|"float value = 7;"; 6|]
        [|"long value = 8;"; 5|]
        [|"ulong value = 9;"; 6|]
        [|"double value = 10;"; 7|]
        [|"decimal value = 11;"; 8|]
    ]

[<Theory>]
[<MemberData(nameof(literalAssignmentScenarios))>]
let ``literal assignment has state space of 1`` (text: string, position: int) = async {
    if (not MSBuildLocator.IsRegistered) then MSBuildLocator.RegisterDefaults() |> ignore
    let workspace = new AdhocWorkspace()
    let projectInfo = ProjectInfo.Create(
        ProjectId.CreateNewId(), VersionStamp.Create(), "TestProject", "TestProject", LanguageNames.CSharp)
    let project = workspace.AddProject(projectInfo)
    let document = workspace.AddDocument(project.Id, "File.cs", SourceText.From(text))
    
    let! semanticModel = document.GetSemanticModelAsync() |> Async.AwaitTask
    let! symbol = SymbolFinder.FindSymbolAtPositionAsync(document, position) |> Async.AwaitTask
    let node = toSyntaxNode(symbol)

    let result = instantiatedStateSpace(node, semanticModel)
    result.Should().Be(bigint(1), "", []) |> ignore
}

let transitiveAssignmentToLiteralScenarios: obj [] list =
    [
        [|"bool value = true; var otherValue = value;"; 23|]
        [|"bool value = false; var otherValue = value;"; 24|]
        [|"char value = 'a'; var otherValue = value;"; 22|]
        [|"sbyte value = 1; var otherValue = value;"; 21|]
        [|"byte value = 2; var otherValue = value;"; 20|]
        [|"short value = 3; var otherValue = value;"; 21|]
        [|"ushort value = 4; var otherValue = value;"; 22|]
        [|"int value = 5; var otherValue = value;"; 19|]
        [|"uint value = 6; var otherValue = value;"; 20|]
        [|"float value = 7; var otherValue = value;"; 21|]
        [|"long value = 8; var otherValue = value;"; 29|]
        [|"ulong value = 9; var otherValue = value;"; 21|]
        [|"double value = 10; var otherValue = value;"; 23|]
        [|"decimal value = 11; var otherValue = value;"; 24|]
    ]

[<Theory>]
[<MemberData(nameof(transitiveAssignmentToLiteralScenarios))>]
let ``transitive assignment to literal has state space of 1`` (text: string, position: int) = async {
    if (not MSBuildLocator.IsRegistered) then MSBuildLocator.RegisterDefaults() |> ignore
    let workspace = new AdhocWorkspace()
    let projectInfo = ProjectInfo.Create(
        ProjectId.CreateNewId(), VersionStamp.Create(), "TestProject", "TestProject", LanguageNames.CSharp)
    let project = workspace.AddProject(projectInfo)
    let document = workspace.AddDocument(project.Id, "File.cs", SourceText.From(text))
    
    let! semanticModel = document.GetSemanticModelAsync() |> Async.AwaitTask
    let! symbol = SymbolFinder.FindSymbolAtPositionAsync(document, position) |> Async.AwaitTask
    let node = toSyntaxNode(symbol)
    
    let result = instantiatedStateSpace(node, semanticModel)
    result.Should().Be(bigint(1), "", []) |> ignore
}