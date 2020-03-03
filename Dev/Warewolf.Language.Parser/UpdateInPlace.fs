module UpdateInPlace

open LanguageAST
open DataStorage
open EvaluationFunctions
open CommonFunctions


/// apply a function to a recordset
let rec evalUpdate (env : WarewolfEnvironment) (lang : string) (update : int) (func : WarewolfAtom -> WarewolfAtom) : WarewolfEnvironment = 
    let exp = ParseCache.TryFind lang
    
    let buffer = 
        match exp with
        | Some a -> a
        | _ -> 
            let temp = parseLanguageExpression lang update ShouldTypeCast.Yes
            temp
    match buffer with
    | RecordSetExpression a -> (evalRecordsSetExpressionUpdate a env update func)
    | ScalarExpression a -> 
        let data = evalScalar a env |> func
        AssignEvaluation.evalAssign (languageExpressionToString (ScalarExpression a)) (data.ToString()) update env
    | WarewolfAtomExpression _ -> failwith "invalid convert"
    | RecordSetNameExpression x -> 
        let data = env.RecordSets.[x.Name].Data
        Map.map (fun a _ -> 
            (applyStarToColumn func env { Name = x.Name
                                          Column = a
                                          Index = Star })) data
        |> ignore
        env
    | ComplexExpression a -> 
        let bob = 
            List.map languageExpressionToString a
            |> List.map (eval env update false)
            |> List.map evalResultToString
            |> fun a -> System.String.Join("", a)
        if bob = lang then failwith "invalid convert"
        else bob |> (fun a -> evalUpdate env a update func)
    | JsonIdentifierExpression _ -> failwith "update not supported for json"
/// apply a function to a recordset column using star
and applyStarToColumn (func : WarewolfAtom -> WarewolfAtom) (env : WarewolfEnvironment) (recset : RecordSetColumnIdentifier) = 
    if recset.Column = PositionColumn then env
    else 
        let column = env.RecordSets.[recset.Name].Data.[recset.Column]
        column.Apply(System.Func<WarewolfAtom, WarewolfAtom>(func)) |> ignore
        env
/// apply a function to a recordset with an update
and evalRecordsSetExpressionUpdate (recset : RecordSetColumnIdentifier) (env : WarewolfEnvironment) (update : int) 
    (func : WarewolfAtom -> WarewolfAtom) : WarewolfEnvironment = 
    if not (env.RecordSets.ContainsKey recset.Name) then failwith "invalid recordset"
    else 
        match recset.Index with
        | IntIndex _ -> 
            let data = 
                EvaluationFunctions.eval env update false (languageExpressionToString (RecordSetExpression recset))
                |> evalResultToString
                |> DataString
                |> func
            AssignEvaluation.evalAssign (languageExpressionToString (RecordSetExpression recset)) (data.ToString()) 
                update env
        | Star -> 
            let column = env.RecordSets.[recset.Name].Data.[recset.Column]
            column.Apply(System.Func<WarewolfAtom, WarewolfAtom>(func)) |> ignore
            env
        | Last -> 
            let data = 
                EvaluationFunctions.eval env update false (languageExpressionToString (RecordSetExpression recset))
                |> evalResultToString
                |> DataString
                |> func
            AssignEvaluation.evalAssign (languageExpressionToString (RecordSetExpression recset)) (data.ToString()) 
                update env
        | IndexExpression b -> 
            let res = eval env update false (languageExpressionToString b) |> evalResultToString
            match b with
            | WarewolfAtomExpression atom -> 
                match atom with
                | Int _ -> 
                    let data = 
                        EvaluationFunctions.eval env update false
                            (languageExpressionToString (RecordSetExpression recset))
                        |> evalResultToString
                        |> DataString
                        |> func
                    AssignEvaluation.evalAssign (languageExpressionToString (RecordSetExpression recset)) 
                        (data.ToString()) update env
                | _ -> failwith "Invalid index"
            | _ -> evalUpdate env (sprintf "[[%s(%s).%s]]" recset.Name res recset.Column) update func
