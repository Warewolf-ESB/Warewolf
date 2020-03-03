module Distinct

open LanguageAST
open DataStorage
open EvaluationFunctions
open CommonFunctions

///Get the indexes of distinct columns
let distinctIndexes (recset:WarewolfRecordset) (columnName:string) =
    let positions = Seq.zip [0..recset.Data.[PositionColumn].Count] recset.Data.[columnName] 
    Seq.distinctBy (fun (_,b) -> b.GetHashCode()) positions |> Seq.map fst
 
let distinctValues (recset:WarewolfRecordset) (columnName:string) (positions:int seq)= 
    Seq.map (fun a -> recset.Data.[columnName].[a].ToString()) positions 

/// asign a listof values to a column
let assignFromList (oldenv:WarewolfEnvironment) (datas:string seq) (exp:string) (update:int) (startPositions:Map<string,int>) =
    let parsed = EvaluationFunctions.parseLanguageExpression exp update ShouldTypeCast.Yes
    let data = List.ofSeq datas
    let mutable env = oldenv
    match parsed with 
        | LanguageExpression.WarewolfAtomExpression _ -> env
        | LanguageExpression.ComplexExpression _ -> failwith "this method is not intended for use with complex expressions"
        | ScalarExpression _ -> AssignEvaluation.evalAssign exp (System.String.Join("," ,data)) update env
        | RecordSetExpression recset -> match recset.Index with
                                            | IntIndex int -> if int<=0 then failwith (sprintf "Recordset index [ %i ] is not greater than zero" int) else  AssignEvaluation.evalAssign exp (Seq.last data) update env
                                            | Last -> let start = match Map.tryFind recset.Name startPositions with
                                                                    | Some a -> a
                                                                    | None -> 0
                                                      for i in [0.. (Seq.length data)-1] do
                                                         env<-  AssignEvaluation.evalAssign (sprintf "[[%s(%i).%s]]" recset.Name  (1+i+start) recset.Column) data.[i] update env
                                                      env
                                            | Star -> for i in [0.. (Seq.length data)-1] do
                                                         env<-  AssignEvaluation.evalAssign (sprintf "[[%s(%i).%s]]" recset.Name  (1+i) recset.Column) data.[i] update env
                                                      env

                                            | IndexExpression indexp ->  match indexp with 
                                                                            |  WarewolfAtomExpression atom ->   let inval = atomToInt atom
                                                                                                                if inval<=0 then failwith (sprintf "Recordset index [ %i ] is not greater than zero" inval) else AssignEvaluation.evalAssign exp (Seq.last data) update env
                                                                            | _ -> failwith "this method is not intended for use with complex expressions"
        | _ -> failwith "only recsets and scalars allowed" 

//apply distinctness to a recset. the weird function definition is a product of the tool
let evalDistinct (env:WarewolfEnvironment) (cols:string seq) (distictcols:string seq) (update:int) (result:string seq )  = 
    let EvalDistinctInner (recset: RecordSetColumnIdentifier) =
         distinctIndexes env.RecordSets.[recset.Name] recset.Column  
    let ToRecset (exp: LanguageExpression) =
        match exp with 
        |   RecordSetExpression recset ->  recset
        |_ -> failwith "scalar in unique"
    let EvalDistinctValuesFromExp  (indexes: int seq)  (recset: RecordSetColumnIdentifier) =
         distinctValues env.RecordSets.[recset.Name] recset.Column indexes      
    let baseexps = Seq.map (evalToExpression env update) cols 
    let inter = baseexps|> Seq.map (fun a -> parseLanguageExpression a update ShouldTypeCast.Yes) |> Seq.map ToRecset
    let resultsIds = Map.map (fun (_:string) (b:WarewolfRecordset) -> b.Count:int) env.RecordSets

    if 1= (Seq.distinctBy (fun (a:RecordSetColumnIdentifier) -> a.Name.GetHashCode()) inter |> Seq.length) then
        let cols = inter|> Seq.collect EvalDistinctInner |> Seq.distinct |> Seq.sort  
        let values = Seq.map (evalToExpression env update) distictcols  |> Seq.map (fun a ->  parseLanguageExpression a update ShouldTypeCast.Yes)  |> Seq.map ToRecset |>   Seq.map (EvalDistinctValuesFromExp cols) |> Seq.zip  result
        let mutable foldingenv = env
        for (a,b) in values do
          foldingenv<- assignFromList foldingenv b a update resultsIds
        foldingenv
    else
        failwith "multiple recordsets detected"