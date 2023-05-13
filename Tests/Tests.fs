module Tests

open Xunit
open Program
open FluentAssertions

[<Fact>]
let ``1 is 1`` () =
    oneIsOne().Should().Be(true, "", []) |> ignore
