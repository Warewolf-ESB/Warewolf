module UpdateInPlace

open LanguageAST
//open LanguageEval
open Microsoft.FSharp.Text.Lexing
open DataASTMutable
open WarewolfParserInterop
open WarewolfDataEvaluationCommon

let rec  EvalUpdate  (env: WarewolfEnvironment) (lang:string) (func: WarewolfAtom->WarewolfAtom) : WarewolfEnvironment=
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
        | RecordSetExpression a ->   (evalRecordsSetExpressionUpdate a env func) 
        | ScalarExpression a ->  let data = env.Scalar.[a] |> func
                                 AssignEvaluation.EvalAssign (LanguageExpressionToString ( ScalarExpression a)) (data.ToString()) env
        | WarewolfAtomAtomExpression a -> env
        | RecordSetNameExpression x ->failwith "update entire recorset not supported"
        | ComplexExpression  a -> failwith "update not supported on complex expression recorset not supported"

and evalRecordsSetExpressionUpdate (recset:RecordSetIdentifier) (env: WarewolfEnvironment) (func: WarewolfAtom->WarewolfAtom) :WarewolfEnvironment =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        failwith "invalid recordset"     
    else
            let positions = env.RecordSets.[recset.Name].Data.[PositionColumn]
            match recset.Index with
                | IntIndex value ->  let data = WarewolfDataEvaluationCommon.Eval env   (LanguageExpressionToString (RecordSetExpression recset)) |> EvalResultToString |> DataString |> func
                                     AssignEvaluation.EvalAssign (LanguageExpressionToString (RecordSetExpression recset))  (data.ToString()) env  


                | Star ->  let column = env.RecordSets.[recset.Name].Data.[recset.Column]
                           let success = column.Apply(System.Func<WarewolfAtom,WarewolfAtom>(func))
                           env
                | Last ->  let data = WarewolfDataEvaluationCommon.Eval env   (LanguageExpressionToString (RecordSetExpression recset)) |> EvalResultToString |> DataString |> func
                           let index = sprintf "[[%s(%i)%s" recset.Name env.RecordSets.[recset.Name].LastIndex recset.Column
                           AssignEvaluation.EvalAssign (LanguageExpressionToString (RecordSetExpression recset))  (data.ToString()) env  
                | IndexExpression b -> 
                                   let res = Eval env (LanguageExpressionToString b) |> EvalResultToString
                                   match b with 
                                        | WarewolfAtomAtomExpression atom ->
                                                    match atom with
                                                    | Int a ->  let data = WarewolfDataEvaluationCommon.Eval env   (LanguageExpressionToString (RecordSetExpression recset)) |> EvalResultToString |> DataString |> func
                                                                AssignEvaluation.EvalAssign (LanguageExpressionToString (RecordSetExpression recset))  (data.ToString()) env 
                                                    | _ -> failwith "Invalid index"
                                        | _ ->   EvalUpdate env ( sprintf "[[%s(%s).%s]]" recset.Name res recset.Column) func
                                    
                | _ -> failwith "Unknown evaluation type"


and EvalRecordsetUpdate (recset:RecordSetIdentifier) (env: WarewolfEnvironment) (func: WarewolfAtom->WarewolfAtom)  =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        failwith "Unknown recordset type"    
    else
        evalRecordsSetExpressionUpdate  recset env func