module DataMergeFunctions

open LanguageAST
open DataStorage
open CommonFunctions
open EvaluationFunctions
//specialise eval for calculate. Its just eval with different meaning for *. can be merged into eval function at the expense of complexity for c# developers 
let rec evalForDataMerge (env : WarewolfEnvironment) (update : int) (lang : string) : WarewolfEvalResult list = 
    let EvalComplex(exp : LanguageExpression list) : WarewolfEvalResult list = 
        List.map (languageExpressionToString >> eval env update false) exp
    let exp = ParseCache.TryFind lang
    
    let buffer = 
        match exp with
        | Some a when update = 0 -> a
        | _ -> 
            let temp = parseLanguageExpression lang update ShouldTypeCast.Yes
            temp
    match buffer with
    | RecordSetExpression a -> [ WarewolfAtomListresult((evalRecordsSet a env)) ]
    | ScalarExpression a -> [ WarewolfAtomResult(evalScalar a env) ]
    | WarewolfAtomExpression a -> [ WarewolfAtomResult a ]
    | RecordSetNameExpression x -> [ evalDataSetExpression env update x ]
    | ComplexExpression a -> (EvalComplex(List.filter (fun b -> "" <> (languageExpressionToString b)) a))
    | JsonIdentifierExpression a -> failwith "not sup[ported for json data"
