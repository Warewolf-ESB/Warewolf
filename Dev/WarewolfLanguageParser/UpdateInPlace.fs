module UpdateInPlace

open LanguageAST
//open LanguageEval
open Microsoft.FSharp.Text.Lexing
open DataASTMutable
open WarewolfParserInterop
open WarewolfDataEvaluationCommon

let rec  EvalUpdate  (env: WarewolfEnvironment) (lang:string) (update:int) (func: WarewolfAtom->WarewolfAtom) : WarewolfEnvironment=
    let EvalComplex (exp:LanguageExpression list) = 
        if((List.length exp) =1) then
            match exp.[0] with
                | RecordSetExpression a ->  evalRecordSetAsString env a
                | ScalarExpression a ->  (evalScalar a env)
                | WarewolfAtomAtomExpression a ->  a
                | _ ->failwith "you should not get here"
        else    
            let start = List.map LanguageExpressionToString  exp |> (List.fold (+) "")
            let evaled = (List.map (LanguageExpressionToString >> (Eval  env update)>>EvalResultToString)  exp )|> (List.fold (+) "")
            if( evaled = start) then
                DataString evaled
            else DataString (Eval env update evaled|>  EvalResultToString)
    
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a ->  a
                    | _->    
                        let temp = ParseLanguageExpression lang update
                        temp
    match buffer with
        | RecordSetExpression a ->   (evalRecordsSetExpressionUpdate a env update func) 
        | ScalarExpression a ->  let data = env.Scalar.[a] |> func
                                 AssignEvaluation.EvalAssign (LanguageExpressionToString ( ScalarExpression a)) (data.ToString()) update env
        | WarewolfAtomAtomExpression a -> failwith "invalid convert"
        | RecordSetNameExpression x -> let data = env.RecordSets.[x.Name].Data
                                       let newData = Map.map (fun a b->   (ApplyStarToColumn func env {Name=x.Name;Column =a; Index=Star} )) data
                                       env
        | ComplexExpression  a -> let bob = List.map LanguageExpressionToString a |> List.map  (Eval env update)  |> List.map EvalResultToString |> fun a-> System.String.Join("",a)
                                  if bob = lang
                                  then  failwith "invalid convert"
                                  else bob|> (fun a ->EvalUpdate env a update func )

and ApplyStarToColumn (func: WarewolfAtom->WarewolfAtom) (env:WarewolfEnvironment) (recset:RecordSetIdentifier)  = 
    if recset.Column = PositionColumn then
        env
    else
        let column = env.RecordSets.[recset.Name].Data.[recset.Column]
        let success = column.Apply(System.Func<WarewolfAtom,WarewolfAtom>(func))
        env

and evalRecordsSetExpressionUpdate (recset:RecordSetIdentifier) (env: WarewolfEnvironment)  (update:int)   (func: WarewolfAtom->WarewolfAtom) :WarewolfEnvironment =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        failwith "invalid recordset"     
    else
            let positions = env.RecordSets.[recset.Name].Data.[PositionColumn]
            match recset.Index with
                | IntIndex value ->  let data = WarewolfDataEvaluationCommon.Eval env update  (LanguageExpressionToString (RecordSetExpression recset)) |> EvalResultToString |> DataString |> func
                                     AssignEvaluation.EvalAssign (LanguageExpressionToString (RecordSetExpression recset))  (data.ToString()) update env  


                | Star ->  let column = env.RecordSets.[recset.Name].Data.[recset.Column]
                           let success = column.Apply(System.Func<WarewolfAtom,WarewolfAtom>(func))
                           env
                | Last ->  let data = WarewolfDataEvaluationCommon.Eval env update  (LanguageExpressionToString (RecordSetExpression recset)) |> EvalResultToString |> DataString |> func
                           let index = sprintf "[[%s(%i)%s" recset.Name env.RecordSets.[recset.Name].LastIndex recset.Column
                           AssignEvaluation.EvalAssign (LanguageExpressionToString (RecordSetExpression recset))  (data.ToString()) update env  
                | IndexExpression b -> 
                                   let res = Eval env update (LanguageExpressionToString b) |> EvalResultToString
                                   match b with 
                                        | WarewolfAtomAtomExpression atom ->
                                                    match atom with
                                                    | Int a ->  let data = WarewolfDataEvaluationCommon.Eval env update   (LanguageExpressionToString (RecordSetExpression recset)) |> EvalResultToString |> DataString |> func
                                                                AssignEvaluation.EvalAssign (LanguageExpressionToString (RecordSetExpression recset))  (data.ToString()) update env 
                                                    |  a -> failwith "Invalid index"
                                        | _ ->   EvalUpdate env  ( sprintf "[[%s(%s).%s]]" recset.Name res recset.Column) update func


and EvalRecordsetUpdate (recset:RecordSetIdentifier) (env: WarewolfEnvironment) (update:int) (func: WarewolfAtom->WarewolfAtom)  =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        failwith "Unknown recordset type"    
    else
        evalRecordsSetExpressionUpdate  recset env update func