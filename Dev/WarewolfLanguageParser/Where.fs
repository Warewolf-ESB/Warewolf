module Where
open LanguageAST
//open LanguageEval

open DataASTMutable
open WarewolfParserInterop
open WarewolfDataEvaluationCommon
open CommonFunctions

let rec  evalWhere  (env: WarewolfEnvironment) (lang:string) (update:int) (func: WarewolfAtom->bool) : List<int>=  
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a ->  a
                    | _->    
                        let temp = parseLanguageExpression lang update
                        temp
    match buffer with
        | RecordSetExpression a ->   (evalRecordsSetExpressionWhere a env update func) 
        | ScalarExpression _ -> failwith "unexpected expression"
        | WarewolfAtomAtomExpression _ -> failwith "unexpected expression"
        | RecordSetNameExpression x ->evalRecordsetWhere  x env func
        | ComplexExpression  _ -> failwith "unexpected expression"

and evalRecordsSetExpressionWhere (recset:RecordSetIdentifier) (env: WarewolfEnvironment)  (update:int)  (func: WarewolfAtom->bool)  =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        failwith "invalid recordset"     
    else
            let positions = env.RecordSets.[recset.Name].Data.[PositionColumn]
            match recset.Index with
                | IndexExpression a -> let data = WarewolfDataEvaluationCommon.eval   env update (languageExpressionToString a) |> evalResultToString
                                       evalWhere env (sprintf "[[%s(%s).%s]]" recset.Name data recset.Column) update func 
                | Star ->  let pos = env.RecordSets.[recset.Name].Data.[recset.Column].Where( System.Func<WarewolfAtom,bool>( func))
                           pos|> Seq.map (fun a-> atomToInt positions.[a]) |>List.ofSeq                
                | _ -> failwith "Unknown evaluation type"

and evalListPositions (column: WarewolfAtomList<WarewolfAtom> ) (positions : WarewolfAtomList<WarewolfAtom> ) (func: WarewolfAtom->bool)=
    column.Where( System.Func<WarewolfAtom,bool>( func))|> Seq.map (fun a-> atomToInt positions.[a]) |>List.ofSeq

and evalRecordsetWhere (recset:RecordSetName) (env: WarewolfEnvironment) (func: WarewolfAtom->bool)  =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        List.empty      
    else
          let positions = env.RecordSets.[recset.Name].Data.[PositionColumn]
          let data = env.RecordSets.[recset.Name].Data |> Map.toList |> List.map snd |> List.collect (fun a -> evalListPositions a positions func) 
          data

