module Distinct

open LanguageAST
//open LanguageEval
open Microsoft.FSharp.Text.Lexing
open DataASTMutable
open WarewolfParserInterop
open WarewolfDataEvaluationCommon
open Where
open Microsoft.FSharp.Linq
let DistinctIndexes (recset:WarewolfRecordset) (columnName:string) =
    let positions = Seq.zip [0..recset.Data.[PositionColumn].Count] recset.Data.[columnName] 
    Seq.distinctBy (fun (a,b) -> b.GetHashCode()) positions |> Seq.map fst
 
let DistinctValues (recset:WarewolfRecordset) (columnName:string) (positions:int seq)= 
    Seq.map (fun a -> recset.Data.[columnName].[a].ToString()) positions


let EvalDistinct (env:WarewolfEnvironment) (cols:string seq) (distictcols:string seq) (result:string seq )  = 
    let EvalDistinctInner (recset: RecordSetIdentifier) =
         DistinctIndexes env.RecordSets.[recset.Name] recset.Column  
    let ToRecset (exp: LanguageExpression) =
        match exp with 
        |   RecordSetExpression recset ->  recset
        |_ -> failwith "scalar in unique"
    let IsRecset (exp: LanguageExpression) =
        match exp with 
        |   RecordSetExpression recset ->  true
        |_ -> false
    let EvalDistinctValuesFromExp  (indexes: int seq)  (recset: RecordSetIdentifier) =
         DistinctValues env.RecordSets.[recset.Name] recset.Column indexes      
    let inter = Seq.map (EvalToExpression env) cols |> Seq.map ParseLanguageExpression |> Seq.map ToRecset
    let resultsIds = Seq.map (EvalToExpression env) result |> Seq.map ParseLanguageExpression |> Seq.filter IsRecset |> Seq.map ToRecset |> Seq.map (fun a -> a.Name ,env.RecordSets.[a.Name].Count) |> Map.ofSeq

    if 1= (Seq.distinctBy (fun (a:RecordSetIdentifier) -> a.Name.GetHashCode()) inter |> Seq.length) then
        let cols = inter|> Seq.collect EvalDistinctInner |> Seq.distinct   
        let values = Seq.map (EvalToExpression env) distictcols  |> Seq.map ParseLanguageExpression  |> Seq.map ToRecset |>   Seq.map (EvalDistinctValuesFromExp cols) |> Seq.zip  result
        let mutable foldingenv = env
        for (a,b) in values do
          foldingenv<- Seq.fold ( fun x v->  AssignEvaluation.EvalAssign a v x ) foldingenv b  
        foldingenv
    else
        failwith "multiple recordsets detected"