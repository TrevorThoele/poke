module DeclaredTests

open Xunit
open poke.Program
open FluentAssertions
open Microsoft.CodeAnalysis.CSharp

let variableScenarios: obj [] list =
    [
        [|"bool value;"; bigint(2)|]
        [|"char value;"; bigint(256)|]
        [|"sbyte value;"; bigint(256)|]
        [|"byte value;"; bigint(256)|]
        [|"short value;"; bigint(65536)|]
        [|"ushort value;"; bigint(65536)|]
        [|"int value;"; bigint.Parse("4294967296")|]
        [|"uint value;"; bigint.Parse("4294967296")|]
        [|"float value;"; bigint.Parse("4294967296")|]
        [|"long value;"; bigint.Parse("18446744073709551616")|]
        [|"ulong value;"; bigint.Parse("18446744073709551616")|]
        [|"double value;"; bigint.Parse("18446744073709551616")|]
        [|"decimal value;"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Class { bool Bool; } Class value;"; bigint(2)|]
        [|"struct Struct { bool Bool; } Struct value;"; bigint(2)|]
    ]

[<Theory>]
[<MemberData(nameof(variableScenarios))>]
let ``variable has correct state space`` (text: string, expectedResult: bigint) =
    let source = parseSource(text)

    let valueVariable = (variables(source.Root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "value"))

    let result = declaredStateSpace(source.Model.GetDeclaredSymbol(valueVariable), source.Model)
    result.Should().Be(expectedResult, "", [])

let functionDomainScenarios: obj [] list =
    [
        [|"void Function(bool value) {}"; bigint.Parse("2")|]
        [|"void Function(char value) {}"; bigint.Parse("256")|]
        [|"void Function(sbyte value) {}"; bigint.Parse("256")|]
        [|"void Function(byte value) {}"; bigint.Parse("256")|]
        [|"void Function(short value) {}"; bigint.Parse("65536")|]
        [|"void Function(ushort value) {}"; bigint.Parse("65536")|]
        [|"void Function(int value) {}"; bigint.Parse("4294967296")|]
        [|"void Function(uint value) {}"; bigint.Parse("4294967296")|]
        [|"void Function(float value) {}"; bigint.Parse("4294967296")|]
        [|"void Function(long value) {}"; bigint.Parse("18446744073709551616")|]
        [|"void Function(ulong value) {}"; bigint.Parse("18446744073709551616")|]
        [|"void Function(double value) {}"; bigint.Parse("18446744073709551616")|]
        [|"void Function(decimal value) {}"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Class { bool Bool; } void Function(Class value) {}"; bigint.Parse("2")|]
        [|"struct Struct { bool Bool; } void Function(Struct value) {}"; bigint.Parse("2")|]
        [|"void Function(bool value, char value2) {}"; bigint.Parse("512")|]
        [|"void Function(char value, sbyte value2) {}"; bigint.Parse("65536")|]
        [|"void Function(sbyte value, byte value2) {}"; bigint.Parse("65536")|]
        [|"void Function(byte value, short value2) {}"; bigint.Parse("16777216")|]
        [|"void Function(short value, ushort value2) {}"; bigint.Parse("4294967296")|]
        [|"void Function(ushort value, int value2) {}"; bigint.Parse("281474976710656")|]
        [|"void Function(int value, uint value2) {}"; bigint.Parse("18446744073709551616")|]
        [|"void Function(uint value, float value2) {}"; bigint.Parse("18446744073709551616")|]
        [|"void Function(float value, long value2) {}"; bigint.Parse("79228162514264337593543950336")|]
        [|"void Function(long value, ulong value2) {}"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"void Function(ulong value, double value2) {}"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"void Function(double value, decimal value2) {}"; bigint.Parse("6277101735386680763835789423207666416102355444464034512896")|]
        [|"class Class { bool Bool; } void Function(bool value, Class value2) {}"; bigint.Parse("4")|]
        [|"struct Struct { bool Bool; } void Function(bool value, Struct value2) {}"; bigint.Parse("4")|]
    ]

[<Theory>]
[<MemberData(nameof(functionDomainScenarios))>]
let ``function has correct domain state space`` (text: string, expectedResult: bigint) =
    let source = parseSource(text)

    let mainMethod = (localFunctions(source.Root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "Function"))
    let result = declaredFunctionStateSpace(mainMethod, source.Model)

    result.Domain.Should().Be(expectedResult, "", []) |> ignore
    result.Codomain.Should().Be(1, "", []) |> ignore

let functionCodomainScenarios: obj [] list =
    [
        [|"bool Function() {}"; bigint.Parse("2")|]
        [|"char Function() {}"; bigint.Parse("256")|]
        [|"sbyte Function() {}"; bigint.Parse("256")|]
        [|"byte Function() {}"; bigint.Parse("256")|]
        [|"short Function() {}"; bigint.Parse("65536")|]
        [|"ushort Function() {}"; bigint.Parse("65536")|]
        [|"int Function() {}"; bigint.Parse("4294967296")|]
        [|"uint Function() {}"; bigint.Parse("4294967296")|]
        [|"float Function() {}"; bigint.Parse("4294967296")|]
        [|"long Function() {}"; bigint.Parse("18446744073709551616")|]
        [|"ulong Function() {}"; bigint.Parse("18446744073709551616")|]
        [|"double Function() {}"; bigint.Parse("18446744073709551616")|]
        [|"decimal Function() {}"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Class { bool Bool; } Class Function() {}"; bigint.Parse("2")|]
        [|"struct Struct { bool Bool; } Struct Function() {}"; bigint.Parse("2")|]
    ]

[<Theory>]
[<MemberData(nameof(functionCodomainScenarios))>]
let ``function has correct codomain state space`` (text: string, expectedResult: bigint) =
    let source = parseSource(text)

    let mainMethod = (localFunctions(source.Root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "Function"))
    let result = declaredFunctionStateSpace(mainMethod, source.Model)

    result.Domain.Should().Be(1, "", []) |> ignore
    result.Codomain.Should().Be(expectedResult, "", []) |> ignore

let classScenarios: obj [] list =
    [
        [|"class Class { bool Bool; }"; bigint.Parse("2")|]
        [|"class Class { char Char; }"; bigint.Parse("256")|]
        [|"class Class { sbyte Sbyte; }"; bigint.Parse("256")|]
        [|"class Class { byte Byte; }"; bigint.Parse("256")|]
        [|"class Class { short Short; }"; bigint.Parse("65536")|]
        [|"class Class { ushort Ushort; }"; bigint.Parse("65536")|]
        [|"class Class { int Int; }"; bigint.Parse("4294967296")|]
        [|"class Class { uint Uint; }"; bigint.Parse("4294967296")|]
        [|"class Class { float Float; }"; bigint.Parse("4294967296")|]
        [|"class Class { long Long; }"; bigint.Parse("18446744073709551616")|]
        [|"class Class { ulong Ulong; }"; bigint.Parse("18446744073709551616")|]
        [|"class Class { double Double; }"; bigint.Parse("18446744073709551616")|]
        [|"class Class { decimal Decimal; }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Class { bool Bool { get; set; } }"; bigint.Parse("2")|]
        [|"class Class { char Char { get; set; } }"; bigint.Parse("256")|]
        [|"class Class { sbyte Sbyte { get; set; } }"; bigint.Parse("256")|]
        [|"class Class { byte Byte { get; set; } }"; bigint.Parse("256")|]
        [|"class Class { short Short { get; set; } }"; bigint.Parse("65536")|]
        [|"class Class { ushort Ushort { get; set; } }"; bigint.Parse("65536")|]
        [|"class Class { int Int { get; set; } }"; bigint.Parse("4294967296")|]
        [|"class Class { uint Uint { get; set; } }"; bigint.Parse("4294967296")|]
        [|"class Class { float Float { get; set; } }"; bigint.Parse("4294967296")|]
        [|"class Class { long Long { get; set; } }"; bigint.Parse("18446744073709551616")|]
        [|"class Class { ulong Ulong { get; set; } }"; bigint.Parse("18446744073709551616")|]
        [|"class Class { double Double { get; set; } }"; bigint.Parse("18446744073709551616")|]
        [|"class Class { decimal Decimal { get; set; } }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Class { bool Bool; char Char }"; bigint.Parse("512")|]
        [|"class Class { char Char; sbyte Sbyte; }"; bigint.Parse("65536")|]
        [|"class Class { sbyte Sbyte; byte Byte; }"; bigint.Parse("65536")|]
        [|"class Class { byte Byte; short Short }"; bigint.Parse("16777216")|]
        [|"class Class { short Short; ushort Ushort; }"; bigint.Parse("4294967296")|]
        [|"class Class { ushort Ushort; int Int; }"; bigint.Parse("281474976710656")|]
        [|"class Class { int Int; uint Uint; }"; bigint.Parse("18446744073709551616")|]
        [|"class Class { uint Uint; float Float; }"; bigint.Parse("18446744073709551616")|]
        [|"class Class { float Float; long Long; }"; bigint.Parse("79228162514264337593543950336")|]
        [|"class Class { long Long; ulong Ulong; }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Class { ulong Ulong; double Double; }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Class { double Double; decimal Decimal; }"; bigint.Parse("6277101735386680763835789423207666416102355444464034512896")|]
        [|"class Class { bool Bool { get; set; } char Char { get; set; } }"; bigint.Parse("512")|]
        [|"class Class { char Char { get; set; } sbyte Sbyte { get; set; } }"; bigint.Parse("65536")|]
        [|"class Class { sbyte Sbyte { get; set; } byte Byte { get; set; } }"; bigint.Parse("65536")|]
        [|"class Class { byte Byte { get; set; } short Short { get; set; } }"; bigint.Parse("16777216")|]
        [|"class Class { short Short { get; set; } ushort Ushort { get; set; } }"; bigint.Parse("4294967296")|]
        [|"class Class { ushort Ushort { get; set; } int Int { get; set; } }"; bigint.Parse("281474976710656")|]
        [|"class Class { int Int { get; set; } uint Uint { get; set; } }"; bigint.Parse("18446744073709551616")|]
        [|"class Class { uint Uint { get; set; } float Float { get; set; } }"; bigint.Parse("18446744073709551616")|]
        [|"class Class { float Float { get; set; } long Long { get; set; } }"; bigint.Parse("79228162514264337593543950336")|]
        [|"class Class { long Long { get; set; } ulong Ulong { get; set; } }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Class { ulong Ulong { get; set; } double Double { get; set; } }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Class { double Double { get; set; } decimal Decimal { get; set; } }"; bigint.Parse("6277101735386680763835789423207666416102355444464034512896")|]
        [|"class Base { bool Bool; } class Class : Base {}"; bigint.Parse("2")|]
        [|"class Base { char Char; } class Class : Base {}"; bigint.Parse("256")|]
        [|"class Base { sbyte Sbyte; } class Class : Base {}"; bigint.Parse("256")|]
        [|"class Base { byte Byte; } class Class : Base {}"; bigint.Parse("256")|]
        [|"class Base { short Short; } class Class : Base {}"; bigint.Parse("65536")|]
        [|"class Base { ushort Ushort; } class Class : Base {}"; bigint.Parse("65536")|]
        [|"class Base { int Int; } class Class : Base {}"; bigint.Parse("4294967296")|]
        [|"class Base { uint Uint; } class Class : Base {}"; bigint.Parse("4294967296")|]
        [|"class Base { float Float; } class Class : Base {}"; bigint.Parse("4294967296")|]
        [|"class Base { long Long; } class Class : Base {}"; bigint.Parse("18446744073709551616")|]
        [|"class Base { ulong Ulong; } class Class : Base {}"; bigint.Parse("18446744073709551616")|]
        [|"class Base { double Double; } class Class : Base {}"; bigint.Parse("18446744073709551616")|]
        [|"class Base { decimal Decimal; } class Class : Base {}"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Base { bool Bool { get; set; } } class Class : Base {}"; bigint.Parse("2")|]
        [|"class Base { char Char { get; set; } } class Class : Base {}"; bigint.Parse("256")|]
        [|"class Base { sbyte Sbyte { get; set; } } class Class : Base {}"; bigint.Parse("256")|]
        [|"class Base { byte Byte { get; set; } } class Class : Base {}"; bigint.Parse("256")|]
        [|"class Base { short Short { get; set; } } class Class : Base {}"; bigint.Parse("65536")|]
        [|"class Base { ushort Ushort { get; set; } } class Class : Base {}"; bigint.Parse("65536")|]
        [|"class Base { int Int { get; set; } } class Class : Base {}"; bigint.Parse("4294967296")|]
        [|"class Base { uint Uint { get; set; } } class Class : Base {}"; bigint.Parse("4294967296")|]
        [|"class Base { float Float { get; set; } } class Class : Base {}"; bigint.Parse("4294967296")|]
        [|"class Base { long Long { get; set; } } class Class : Base {}"; bigint.Parse("18446744073709551616")|]
        [|"class Base { ulong Ulong { get; set; } } class Class : Base {}"; bigint.Parse("18446744073709551616")|]
        [|"class Base { double Double { get; set; } } class Class : Base {}"; bigint.Parse("18446744073709551616")|]
        [|"class Base { decimal Decimal { get; set; } } class Class : Base {}"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Base { bool Bool; char Char } class Class : Base {}"; bigint.Parse("512")|]
        [|"class Base { char Char; sbyte Sbyte; } class Class : Base {}"; bigint.Parse("65536")|]
        [|"class Base { sbyte Sbyte; byte Byte; } class Class : Base {}"; bigint.Parse("65536")|]
        [|"class Base { byte Byte; short Short } class Class : Base {}"; bigint.Parse("16777216")|]
        [|"class Base { short Short; ushort Ushort; } class Class : Base {}"; bigint.Parse("4294967296")|]
        [|"class Base { ushort Ushort; int Int; } class Class : Base {}"; bigint.Parse("281474976710656")|]
        [|"class Base { int Int; uint Uint; } class Class : Base {}"; bigint.Parse("18446744073709551616")|]
        [|"class Base { uint Uint; float Float; } class Class : Base {}"; bigint.Parse("18446744073709551616")|]
        [|"class Base { float Float; long Long; } class Class : Base {}"; bigint.Parse("79228162514264337593543950336")|]
        [|"class Base { long Long; ulong Ulong; } class Class : Base {}"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Base { ulong Ulong; double Double; } class Class : Base {}"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Base { double Double; decimal Decimal; } class Class : Base {}"; bigint.Parse("6277101735386680763835789423207666416102355444464034512896")|]
        [|"class Base { bool Bool { get; set; } char Char { get; set; } } class Class : Base {}"; bigint.Parse("512")|]
        [|"class Base { char Char { get; set; } sbyte Sbyte { get; set; } } class Class : Base {}"; bigint.Parse("65536")|]
        [|"class Base { sbyte Sbyte { get; set; } byte Byte { get; set; } } class Class : Base {}"; bigint.Parse("65536")|]
        [|"class Base { byte Byte { get; set; } short Short { get; set; } } class Class : Base {}"; bigint.Parse("16777216")|]
        [|"class Base { short Short { get; set; } ushort Ushort { get; set; } } class Class : Base {}"; bigint.Parse("4294967296")|]
        [|"class Base { ushort Ushort { get; set; } int Int { get; set; } } class Class : Base {}"; bigint.Parse("281474976710656")|]
        [|"class Base { int Int { get; set; } uint Uint { get; set; } } class Class : Base {}"; bigint.Parse("18446744073709551616")|]
        [|"class Base { uint Uint { get; set; } float Float { get; set; } } class Class : Base {}"; bigint.Parse("18446744073709551616")|]
        [|"class Base { float Float { get; set; } long Long { get; set; } } class Class : Base {}"; bigint.Parse("79228162514264337593543950336")|]
        [|"class Base { long Long { get; set; } ulong Ulong { get; set; } } class Class : Base {}"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Base { ulong Ulong { get; set; } double Double { get; set; } } class Class : Base {}"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"class Base { double Double { get; set; } decimal Decimal { get; set; } } class Class : Base {}"; bigint.Parse("6277101735386680763835789423207666416102355444464034512896")|]
    ]

[<Theory>]
[<MemberData(nameof(classScenarios))>]
let ``class has correct state space`` (text: string, expectedResult: bigint) =
    let source = parseSource(text)

    let ``class`` = (classes(source.Root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "Class"))
    let result = declaredStateSpace(source.Model.GetDeclaredSymbol(``class``), source.Model)
    
    result.Should().Be(expectedResult, "", [])

[<Fact>]
let ``many base classes result in correct state space`` () =
    let source = parseSource("class Base1 { bool Bool; } class Base2 : Base1 { char Char; } class Class : Base2 {}")

    let ``class`` = (classes(source.Root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "Class"))
    let result = declaredStateSpace(source.Model.GetDeclaredSymbol(``class``), source.Model)
    
    result.Should().Be(bigint.Parse("512"), "", [])

let structScenarios: obj [] list =
    [
        [|"struct Struct { bool Bool; }"; bigint.Parse("2")|]
        [|"struct Struct { char Char; }"; bigint.Parse("256")|]
        [|"struct Struct { sbyte Sbyte; }"; bigint.Parse("256")|]
        [|"struct Struct { byte Byte; }"; bigint.Parse("256")|]
        [|"struct Struct { short Short; }"; bigint.Parse("65536")|]
        [|"struct Struct { ushort Ushort; }"; bigint.Parse("65536")|]
        [|"struct Struct { int Int; }"; bigint.Parse("4294967296")|]
        [|"struct Struct { uint Uint; }"; bigint.Parse("4294967296")|]
        [|"struct Struct { float Float; }"; bigint.Parse("4294967296")|]
        [|"struct Struct { long Long; }"; bigint.Parse("18446744073709551616")|]
        [|"struct Struct { ulong Ulong; }"; bigint.Parse("18446744073709551616")|]
        [|"struct Struct { double Double; }"; bigint.Parse("18446744073709551616")|]
        [|"struct Struct { decimal Decimal; }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"struct Struct { bool Bool { get; set; } }"; bigint.Parse("2")|]
        [|"struct Struct { char Char { get; set; } }"; bigint.Parse("256")|]
        [|"struct Struct { sbyte Sbyte { get; set; } }"; bigint.Parse("256")|]
        [|"struct Struct { byte Byte { get; set; } }"; bigint.Parse("256")|]
        [|"struct Struct { short Short { get; set; } }"; bigint.Parse("65536")|]
        [|"struct Struct { ushort Ushort { get; set; } }"; bigint.Parse("65536")|]
        [|"struct Struct { int Int { get; set; } }"; bigint.Parse("4294967296")|]
        [|"struct Struct { uint Uint { get; set; } }"; bigint.Parse("4294967296")|]
        [|"struct Struct { float Float { get; set; } }"; bigint.Parse("4294967296")|]
        [|"struct Struct { long Long { get; set; } }"; bigint.Parse("18446744073709551616")|]
        [|"struct Struct { ulong Ulong { get; set; } }"; bigint.Parse("18446744073709551616")|]
        [|"struct Struct { double Double { get; set; } }"; bigint.Parse("18446744073709551616")|]
        [|"struct Struct { decimal Decimal { get; set; } }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"struct Struct { bool Bool; char Char }"; bigint.Parse("512")|]
        [|"struct Struct { char Char; sbyte Sbyte; }"; bigint.Parse("65536")|]
        [|"struct Struct { sbyte Sbyte; byte Byte; }"; bigint.Parse("65536")|]
        [|"struct Struct { byte Byte; short Short }"; bigint.Parse("16777216")|]
        [|"struct Struct { short Short; ushort Ushort; }"; bigint.Parse("4294967296")|]
        [|"struct Struct { ushort Ushort; int Int; }"; bigint.Parse("281474976710656")|]
        [|"struct Struct { int Int; uint Uint; }"; bigint.Parse("18446744073709551616")|]
        [|"struct Struct { uint Uint; float Float; }"; bigint.Parse("18446744073709551616")|]
        [|"struct Struct { float Float; long Long; }"; bigint.Parse("79228162514264337593543950336")|]
        [|"struct Struct { long Long; ulong Ulong; }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"struct Struct { ulong Ulong; double Double; }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"struct Struct { double Double; decimal Decimal; }"; bigint.Parse("6277101735386680763835789423207666416102355444464034512896")|]
        [|"struct Struct { bool Bool { get; set; } char Char { get; set; } }"; bigint.Parse("512")|]
        [|"struct Struct { char Char { get; set; } sbyte Sbyte { get; set; } }"; bigint.Parse("65536")|]
        [|"struct Struct { sbyte Sbyte { get; set; } byte Byte { get; set; } }"; bigint.Parse("65536")|]
        [|"struct Struct { byte Byte { get; set; } short Short { get; set; } }"; bigint.Parse("16777216")|]
        [|"struct Struct { short Short { get; set; } ushort Ushort { get; set; } }"; bigint.Parse("4294967296")|]
        [|"struct Struct { ushort Ushort { get; set; } int Int { get; set; } }"; bigint.Parse("281474976710656")|]
        [|"struct Struct { int Int { get; set; } uint Uint { get; set; } }"; bigint.Parse("18446744073709551616")|]
        [|"struct Struct { uint Uint { get; set; } float Float { get; set; } }"; bigint.Parse("18446744073709551616")|]
        [|"struct Struct { float Float { get; set; } long Long { get; set; } }"; bigint.Parse("79228162514264337593543950336")|]
        [|"struct Struct { long Long { get; set; } ulong Ulong { get; set; } }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"struct Struct { ulong Ulong { get; set; } double Double { get; set; } }"; bigint.Parse("340282366920938463463374607431768211456")|]
        [|"struct Struct { double Double { get; set; } decimal Decimal { get; set; } }"; bigint.Parse("6277101735386680763835789423207666416102355444464034512896")|]
    ]

[<Theory>]
[<MemberData(nameof(structScenarios))>]
let ``struct has correct state space`` (text: string, expectedResult: bigint) =
    let source = parseSource(text)

    let ``class`` = (structs(source.Root)
        |> Seq.find(fun x -> x.Identifier.ToString() = "Struct"))
    let result = declaredStateSpace(source.Model.GetDeclaredSymbol(``class``), source.Model)
    
    result.Should().Be(expectedResult, "", [])