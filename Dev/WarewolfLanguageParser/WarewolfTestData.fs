module WarewolfTestData
open LanguageAST
open WarewolfDataEvaluationCommon
//open LanguageEval
open Microsoft.FSharp.Text.Lexing
open DataASTMutable

let PositionColumn = "WarewolfPositionColumn"

let CreateTestEnv =     
    let vars = Map.ofList  [("a", DataString "a"); ("b", Int 2344);("c", DataString "a") ] 
    let recordsets =[ ("rec", PublicFunctions.CreateDataSet "a" )] |> Map.ofList
    {RecordSets=recordsets;Scalar=vars}

let CreateTestEnvEmpty (a:string) =     
    {RecordSets=Map.empty;Scalar=Map.empty}


let CreateTestEnvWithData =     
    let vars = Map.ofList  [("a", DataString "a"); ("b", Int 2344);("c", DataString "a");("d", Int 1) ]
    let positionColumn =  (PositionColumn, new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>( WarewolfAtomRecord.Nothing, ([| Int 1; Int 2; Int 3|]),3))
    let aColumn = ( "a",  new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing,  [ Int 2; Int 4; Int 3 ]))
    let data = [positionColumn;aColumn] |> Map.ofList
    let recordsets =[ ("rec", {PublicFunctions.CreateDataSet "a" with Data= data;} )] |> Map.ofList
    {RecordSets=recordsets;Scalar=vars}