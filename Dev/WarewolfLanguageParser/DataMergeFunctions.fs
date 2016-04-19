module DataMergeFunctions

open LanguageAST
open DataASTMutable
open CommonFunctions
open WarewolfDataEvaluationCommon

let rec evalForDataMerge (env : WarewolfEnvironment) (update : int) (lang : string) : WarewolfEvalResult list = 
    let EvalComplex(exp : LanguageExpression list) : WarewolfEvalResult list = 
        List.map (languageExpressionToString >> eval env update) exp
    let exp = ParseCache.TryFind lang
    
    let buffer = 
        match exp with
        | Some a when update = 0 -> a
        | _ -> 
            let temp = parseLanguageExpression lang update
            temp
    match buffer with
    | RecordSetExpression a -> [ WarewolfAtomListresult((evalRecordsSet a env)) ]
    | ScalarExpression a -> [ WarewolfAtomResult(evalScalar a env) ]
    | WarewolfAtomAtomExpression a -> [ WarewolfAtomResult a ]
    | RecordSetNameExpression x -> [ evalDataSetExpression env update x ]
    | ComplexExpression a -> (EvalComplex(List.filter (fun b -> "" <> (languageExpressionToString b)) a))
    | JsonIdentifierExpression a -> failwith "not sup[ported for json data"
