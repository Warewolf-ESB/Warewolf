module Where
open LanguageAST
//open LanguageEval
open Microsoft.FSharp.Text.Lexing
open DataASTMutable
open WarewolfParserInterop
open WarewolfDataEvaluationCommon

let rec  EvalWhere  (env: WarewolfEnvironment) (lang:string) (func: WarewolfAtom->bool) : List<int>=
    let EvalComplex (exp:LanguageExpression list) = 
        if((List.length exp) =1) then
            match exp.[0] with
                | RecordSetExpression a ->  evalRecordSetAsString env a
                | ScalarExpression a ->  (evalScalar a env)
                | WarewolfAtomAtomExpression a ->  a
                | _ ->failwith "you should not get here"
        else    
            let start = List.map LanguageExpressionToString  exp |> (List.fold (+) "")
            let evaled = (List.map (LanguageExpressionToString >> (Eval  env)>>EvalResultToString)  exp )|> (List.fold (+) "")
            if( evaled = start) then
                DataString evaled
            else DataString (Eval env evaled|>  EvalResultToString)
    
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a ->  a
                    | _->    
                        let temp = ParseLanguageExpression lang
                        temp
    match buffer with
        | RecordSetExpression a ->   (evalRecordsSetExpressionWhere a env func) 
        | ScalarExpression a -> failwith "bob"
        | WarewolfAtomAtomExpression a -> failwith "bob"
        | RecordSetNameExpression x ->EvalRecordsetWhere  x env func
        | ComplexExpression  a -> failwith "bob"

and evalRecordsSetExpressionWhere (recset:RecordSetIdentifier) (env: WarewolfEnvironment) (func: WarewolfAtom->bool)  =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        List.empty      
    else
            let positions = env.RecordSets.[recset.Name].Data.[PositionColumn]
            match recset.Index with
                | IndexExpression a -> let data = WarewolfDataEvaluationCommon.Eval   env (LanguageExpressionToString a) |> EvalResultToString
                                       EvalWhere env (sprintf "[[%s(%s).%s]]" recset.Name data recset.Column) func 
                | Star ->  env.RecordSets.[recset.Name].Data.[recset.Column].Where( System.Func<WarewolfAtom,bool>( func))|> Seq.map (fun a-> AtomToInt positions.[a]) |>List.ofSeq
                
                | _ -> failwith "Unknown evaluation type"

and evalListPositions (column: WarewolfAtomList<WarewolfAtom> ) (positions : WarewolfAtomList<WarewolfAtom> ) (func: WarewolfAtom->bool)=
    column.Where( System.Func<WarewolfAtom,bool>( func))|> Seq.map (fun a-> AtomToInt positions.[a]) |>List.ofSeq

and EvalRecordsetWhere (recset:RecordSetName) (env: WarewolfEnvironment) (func: WarewolfAtom->bool)  =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        List.empty      
    else
          let positions = env.RecordSets.[recset.Name].Data.[PositionColumn]
          let data = env.RecordSets.[recset.Name].Data |> Map.toList |> List.map snd |> List.collect (fun a -> evalListPositions a positions func) 
          List.empty