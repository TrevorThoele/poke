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

let classAssignmentScenarios: obj [] list =
    [
        [|"class MyClass {} var value = new MyClass();"; 21; bigint(1)|]
        [|"class MyClass { MyClass(bool value) {} } var value = new MyClass(true);"; 45; bigint(1)|]
        [|"class MyClass { MyClass(char value) {} } var value = new MyClass('a');"; 45; bigint(1)|]
        [|"class MyClass { MyClass(sbyte value) {} } var value = new MyClass(1);"; 46; bigint(1)|]
        [|"class MyClass { MyClass(byte value) {} } var value = new MyClass(2);"; 45; bigint(1)|]
        [|"class MyClass { MyClass(short value) {} } var value = new MyClass(3);"; 46; bigint(1)|]
        [|"class MyClass { MyClass(ushort value) {} } var value = new MyClass(4);"; 47; bigint(1)|]
        [|"class MyClass { MyClass(int value) {} } var value = new MyClass(5);"; 44; bigint(1)|]
        [|"class MyClass { MyClass(uint value) {} } var value = new MyClass(6);"; 45; bigint(1)|]
        [|"class MyClass { MyClass(float value) {} } var value = new MyClass(7);"; 46; bigint(1)|]
        [|"class MyClass { MyClass(long value) {} } var value = new MyClass(8);"; 45; bigint(1)|]
        [|"class MyClass { MyClass(ulong value) {} } var value = new MyClass(9);"; 46; bigint(1)|]
        [|"class MyClass { MyClass(double value) {} } var value = new MyClass(10);"; 47; bigint(1)|]
        [|"class MyClass { MyClass(decimal value) {} } var value = new MyClass(11);"; 48; bigint(1)|]
        [|"class MyClass { MyClass(bool value, char value2) {} } var value = new MyClass(true, 'a');"; 58; bigint(1)|]
        [|"class MyClass { MyClass(char value, sbyte value2) {} } var value = new MyClass('a', 1);"; 59; bigint(1)|]
        [|"class MyClass { MyClass(sbyte value, byte value2) {} } var value = new MyClass(1, 2);"; 59; bigint(1)|]
        [|"class MyClass { MyClass(byte value, short value2) {} } var value = new MyClass(2, 3);"; 59; bigint(1)|]
        [|"class MyClass { MyClass(short value, ushort value2) {} } var value = new MyClass(3, 4);"; 61; bigint(1)|]
        [|"class MyClass { MyClass(ushort value, int value2) {} } var value = new MyClass(4, 5);"; 59; bigint(1)|]
        [|"class MyClass { MyClass(int value, uint value2) {} } var value = new MyClass(5, 6);"; 57; bigint(1)|]
        [|"class MyClass { MyClass(uint value, float value2) {} } var value = new MyClass(6, 7);"; 59; bigint(1)|]
        [|"class MyClass { MyClass(float value, long value2) {} } var value = new MyClass(7, 8);"; 59; bigint(1)|]
        [|"class MyClass { MyClass(long value, ulong value2) {} } var value = new MyClass(8, 9);"; 59; bigint(1)|]
        [|"class MyClass { MyClass(ulong value, double value2) {} } var value = new MyClass(9, 10);"; 61; bigint(1)|]
        [|"class MyClass { MyClass(double value, decimal value2) {} } var value = new MyClass(10, 11);"; 63; bigint(1)|]
    ]

[<Theory>]
[<MemberData(nameof(classAssignmentScenarios))>]
let ``class assignment has correct state space`` (text: string, position: int, expectedResult: bigint) = async {
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
    result.Should().Be(expectedResult, "", []) |> ignore
}

let structAssignmentScenarios: obj [] list =
    [
        [|"struct MyStruct {} var value = new MyStruct();"; 23; bigint(1)|]
        [|"struct MyStruct { MyStruct(bool value) {} } var value = new MyStruct(true);"; 48; bigint(1)|]
        [|"struct MyStruct { MyStruct(char value) {} } var value = new MyStruct('a');"; 48; bigint(1)|]
        [|"struct MyStruct { MyStruct(sbyte value) {} } var value = new MyStruct(1);"; 49; bigint(1)|]
        [|"struct MyStruct { MyStruct(byte value) {} } var value = new MyStruct(2);"; 48; bigint(1)|]
        [|"struct MyStruct { MyStruct(short value) {} } var value = new MyStruct(3);"; 49; bigint(1)|]
        [|"struct MyStruct { MyStruct(ushort value) {} } var value = new MyStruct(4);"; 50; bigint(1)|]
        [|"struct MyStruct { MyStruct(int value) {} } var value = new MyStruct(5);"; 47; bigint(1)|]
        [|"struct MyStruct { MyStruct(uint value) {} } var value = new MyStruct(6);"; 48; bigint(1)|]
        [|"struct MyStruct { MyStruct(float value) {} } var value = new MyStruct(7);"; 49; bigint(1)|]
        [|"struct MyStruct { MyStruct(long value) {} } var value = new MyStruct(8);"; 48; bigint(1)|]
        [|"struct MyStruct { MyStruct(ulong value) {} } var value = new MyStruct(9);"; 49; bigint(1)|]
        [|"struct MyStruct { MyStruct(double value) {} } var value = new MyStruct(10);"; 50; bigint(1)|]
        [|"struct MyStruct { MyStruct(decimal value) {} } var value = new MyStruct(11);"; 51; bigint(1)|]
        [|"struct MyStruct { MyStruct(bool value, char value2) {} } var value = new MyStruct(true, 'a');"; 61; bigint(1)|]
        [|"struct MyStruct { MyStruct(char value, sbyte value2) {} } var value = new MyStruct('a', 1);"; 62; bigint(1)|]
        [|"struct MyStruct { MyStruct(sbyte value, byte value2) {} } var value = new MyStruct(1, 2);"; 62; bigint(1)|]
        [|"struct MyStruct { MyStruct(byte value, short value2) {} } var value = new MyStruct(2, 3);"; 62; bigint(1)|]
        [|"struct MyStruct { MyStruct(short value, ushort value2) {} } var value = new MyStruct(3, 4);"; 64; bigint(1)|]
        [|"struct MyStruct { MyStruct(ushort value, int value2) {} } var value = new MyStruct(4, 5);"; 62; bigint(1)|]
        [|"struct MyStruct { MyStruct(int value, uint value2) {} } var value = new MyStruct(5, 6);"; 60; bigint(1)|]
        [|"struct MyStruct { MyStruct(uint value, float value2) {} } var value = new MyStruct(6, 7);"; 62; bigint(1)|]
        [|"struct MyStruct { MyStruct(float value, long value2) {} } var value = new MyStruct(7, 8);"; 62; bigint(1)|]
        [|"struct MyStruct { MyStruct(long value, ulong value2) {} } var value = new MyStruct(8, 9);"; 62; bigint(1)|]
        [|"struct MyStruct { MyStruct(ulong value, double value2) {} } var value = new MyStruct(9, 10);"; 64; bigint(1)|]
        [|"struct MyStruct { MyStruct(double value, decimal value2) {} } var value = new MyStruct(10, 11);"; 66; bigint(1)|]
    ]

[<Theory>]
[<MemberData(nameof(structAssignmentScenarios))>]
let ``struct assignment has correct state space`` (text: string, position: int, expectedResult: bigint) = async {
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
    result.Should().Be(expectedResult, "", []) |> ignore
}

let transitiveAssignmentToLiteralScenarios: obj [] list =
    [
        [|"bool value = true; var transitive1 = value;"; 23|]
        [|"bool value = false; var transitive1 = value;"; 24|]
        [|"char value = 'a'; var transitive1 = value;"; 22|]
        [|"sbyte value = 1; var transitive1 = value;"; 21|]
        [|"byte value = 2; var transitive1 = value;"; 20|]
        [|"short value = 3; var transitive1 = value;"; 21|]
        [|"ushort value = 4; var transitive1 = value;"; 22|]
        [|"int value = 5; var transitive1 = value;"; 19|]
        [|"uint value = 6; var transitive1 = value;"; 20|]
        [|"float value = 7; var transitive1 = value;"; 21|]
        [|"long value = 8; var transitive1 = value;"; 29|]
        [|"ulong value = 9; var transitive1 = value;"; 21|]
        [|"double value = 10; var transitive1 = value;"; 23|]
        [|"decimal value = 11; var transitive1 = value;"; 24|]
        [|"class MyClass { MyClass(bool value) {} } bool value = true; var transitive1 = new MyClass(value);"; 64|]
        [|"class MyClass { MyClass(bool value) {} } MyClass value = new MyClass(true); var transitive1 = value;"; 80|]
        [|"class MyStruct { MyStruct(bool value) {} } bool value = true; var transitive1 = new MyStruct(value);"; 66|]
        [|"class MyStruct { MyStruct(bool value) {} } MyStruct value = new MyStruct(true); var transitive1 = value;"; 84|]
        [|"bool value = true; var transitive1 = value; var transitive2 = transitive1;"; 48|]
        [|"bool value = false; var transitive1 = value; var transitive2 = transitive1;"; 49|]
        [|"char value = 'a'; var transitive1 = value; var transitive2 = transitive1;"; 47|]
        [|"sbyte value = 1; var transitive1 = value; var transitive2 = transitive1;"; 46|]
        [|"byte value = 2; var transitive1 = value; var transitive2 = transitive1;"; 45|]
        [|"short value = 3; var transitive1 = value; var transitive2 = transitive1;"; 46|]
        [|"ushort value = 4; var transitive1 = value; var transitive2 = transitive1;"; 47|]
        [|"int value = 5; var transitive1 = value; var transitive2 = transitive1;"; 44|]
        [|"uint value = 6; var transitive1 = value; var transitive2 = transitive1;"; 45|]
        [|"float value = 7; var transitive1 = value; var transitive2 = transitive1;"; 46|]
        [|"long value = 8; var transitive1 = value; var transitive2 = transitive1;"; 45|]
        [|"ulong value = 9; var transitive1 = value; var transitive2 = transitive1;"; 46|]
        [|"double value = 10; var transitive1 = value; var transitive2 = transitive1;"; 48|]
        [|"decimal value = 11; var transitive1 = value; var transitive2 = transitive1;"; 49|]
        [|"class MyClass { MyClass(bool value) {} } bool value = true; var transitive1 = value; var transitive2 = new MyClass(transitive1);"; 89|]
        [|"class MyClass { MyClass(bool value) {} } MyClass value = new MyClass(true); var transitive1 = value; var transitive2 = transitive1;"; 105|]
        [|"class MyStruct { MyStruct(bool value) {} } bool value = true; var transitive1 = value; var transitive2 = new MyStruct(transitive1);"; 91|]
        [|"class MyStruct { MyStruct(bool value) {} } MyStruct value = new MyStruct(true); var transitive1 = value; var transitive2 = transitive1;"; 109|]
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