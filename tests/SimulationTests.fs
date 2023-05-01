﻿module simulationTests
open Domain
open Evaluations
open Simulations
open Analysis
open FsUnit
open Xunit

module simulationTests =
    let listsAreEqual (xs: float list) (ys: float list) (tolerance: float) : bool = // checks if two lists are approximately equal
        match List.length xs, List.length ys with
        | xlen, ylen when xlen <> ylen -> failwith "Lists must have the same length"
        | _ -> xs |> List.forall2 (fun x y -> abs(x - y) / y <= tolerance) ys
    
    let simulation (f: 'a -> float) (input : 'a) : float = // runs a function 1_000_000 times and returns the average output
        let n = 1_000_000
        [for _ in 1..n ->
            let res = f input
            res]
        |> List.average
    [<Theory>] 
    [<InlineData(5, 0.9997260724)>]    // 1/((1+0,02/365))^5)    = 0.9997260724
    [<InlineData(20, 0.9989047398)>]   // 1/((1+0,02/365))^20)   = 0.9989047398
    [<InlineData(30, 0.9983575597)>]   // 1/((1+0,02/365))^30)   = 0.9983575597
    [<InlineData(365, 0.9801992104)>]  // 1/((1+0,02/365))^365)  = 0.9801992104
    [<InlineData(1000, 0.9466810723)>] // 1/((1+0,02/365))^1000) = 0.9466810723
    let ``discount function should correctly discount back the value``(input : int, expectedOutput) =
        let tolerance = 1e-7
        let output = I input
        let isEqual = abs (output - expectedOutput) <= tolerance
        isEqual |> should equal true
    
    [<Theory>] 
    [<InlineData(0, 1, 1.0)>]
    [<InlineData(10, 15, 1.0)>]    
    [<InlineData(7, 10, 0.5)>]    
    [<InlineData(10, 15, 0.37)>]    
    [<InlineData(1, 10, 1.53)>]    
    [<InlineData(5, 10, 1.35)>]    
    let ``Wiener Process: E[W(t)] = 0 at any time.``(startTime, endTime, dt) =
        let simulation : float list =
            let n = 1_000_000
            [1..n] |> List.map (fun _ -> WienerProcess(startTime, endTime, dt))
            |> List.transpose
            |> List.map List.average
        let tolerance = 1e-2
        let isZero : bool = simulation |> List.forall (fun x ->
            if abs(x) > tolerance then
                printfn "%A" (abs(x))
                printfn "%A" (abs(x) <= tolerance)
            abs(x) <= tolerance)
        isZero |> should equal true
        
        
    [<Theory>] 
    [<InlineData(100.0, 0, 1, 0.1, 1.0, 0.2)>]     // S_0 = 100.0, start = 0, end = 1, dt = 0.1, mu 1.0, sigma = 0.2
    [<InlineData(15.0, 0, 10, 1.0, -0.2, 0.4)>]     // S_0 = 15.0, start = 0 end = 10, dt = 1.0, mu = 0.5, sigma = 0.4
    //[<InlineData(50.0, 10, 15, 1.0, 0.1, 0.1)>]  // S_0 = 50.0, start = 10, end = 11, dt = 0.01, mu = -0.5, sigma = 0.1
    [<InlineData(1.0, 0, 15, 1.0, 1.0, 0.2)>]      // S_0 = 1.0, start = 0, end = 3, dt = 0.07, mu = 0.0, sigma = 0.6

    let ``Assert GBM Expected Value``(S0, startTime, endTime, dt, mu, sigma) =
        let expectation t = S0 * exp(mu * t) // E[S_t] = S_0 * exp(mu * t)
        let expectedValues : float list =
            [for t in float startTime.. dt .. float endTime -> expectation t]
        let simulation : float list = // GBM Simulations
            let n = 2_000_000
            [1..n] |> List.map (fun _ ->
                let wpValues = WienerProcess(startTime , endTime, dt)
                GeometricBrownianMotion(S0, startTime, endTime, dt, mu, sigma, wpValues))
            |> List.transpose
            |> List.map List.average
        //printfn "%s %A" "dt values:" [float startTime ..dt..float endTime]
        //printfn "%s %A" "simulated values:" simulation
        //printfn "%s %A" "expected values:" expectedValues
        let tolerance = 1e-2
        let test : bool = listsAreEqual expectedValues simulation tolerance
        test |> should equal true

       
       
       

