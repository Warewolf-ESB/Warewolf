module UpdateInPlace

open LanguageAST
//open LanguageEval
open Microsoft.FSharp.Text.Lexing
open DataASTMutable
open WarewolfParserInterop
open WarewolfDataEvaluationCommon
open CommonFunctions
let rec  EvalUpdate  (env: WarewolfEnvironment) (lang:string) (update:int) (func: WarewolfAtom->WarewolfAtom) : WarewolfEnvironment=
   
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a ->  a
                    | _->    
                        let temp = parseLanguageExpression lang update
                        temp
    match buffer with
        | RecordSetExpression a ->   (evalRecordsSetExpressionUpdate a env update func) 
        | ScalarExpression a ->  let data = evalScalar a env |> func
                                 AssignEvaluation.EvalAssign (languageExpressionToString ( ScalarExpression a)) (data.ToString()) update env
        | WarewolfAtomAtomExpression a -> failwith "invalid convert"
        | RecordSetNameExpression x -> let data = env.RecordSets.[x.Name].Data
                                       let newData = Map.map (fun a b->   (ApplyStarToColumn func env {Name=x.Name;Column =a; Index=Star} )) data
                                       env
        | ComplexExpression  a -> let bob = List.map languageExpressionToString a |> List.map  (eval env update)  |> List.map evalResultToString |> fun a-> System.String.Join("",a)
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
            match recset.Index with
                | IntIndex value ->  let data = WarewolfDataEvaluationCommon.eval env update  (languageExpressionToString (RecordSetExpression recset)) |> evalResultToString |> DataString |> func
                                     AssignEvaluation.EvalAssign (languageExpressionToString (RecordSetExpression recset))  (data.ToString()) update env  


                | Star ->  let column = env.RecordSets.[recset.Name].Data.[recset.Column]
                           let success = column.Apply(System.Func<WarewolfAtom,WarewolfAtom>(func))
                           env
                | Last ->  let data = WarewolfDataEvaluationCommon.eval env update  (languageExpressionToString (RecordSetExpression recset)) |> evalResultToString |> DataString |> func
                           let index = sprintf "[[%s(%i)%s" recset.Name env.RecordSets.[recset.Name].LastIndex recset.Column
                           AssignEvaluation.EvalAssign (languageExpressionToString (RecordSetExpression recset))  (data.ToString()) update env  
                | IndexExpression b -> 
                                   let res = eval env update (languageExpressionToString b) |> evalResultToString
                                   match b with 
                                        | WarewolfAtomAtomExpression atom ->
                                                    match atom with
                                                    | Int a ->  let data = WarewolfDataEvaluationCommon.eval env update   (languageExpressionToString (RecordSetExpression recset)) |> evalResultToString |> DataString |> func
                                                                AssignEvaluation.EvalAssign (languageExpressionToString (RecordSetExpression recset))  (data.ToString()) update env 
                                                    |  a -> failwith "Invalid index"
                                        | _ ->   EvalUpdate env  ( sprintf "[[%s(%s).%s]]" recset.Name res recset.Column) update func


and EvalRecordsetUpdate (recset:RecordSetIdentifier) (env: WarewolfEnvironment) (update:int) (func: WarewolfAtom->WarewolfAtom)  =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        failwith "Unknown recordset type"    
    else
        evalRecordsSetExpressionUpdate  recset env update func