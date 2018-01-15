module EvaluationFunctions

open LanguageAST
//open LanguageEval
open Microsoft.FSharp.Text.Lexing
open DataStorage
open WarewolfParserInterop
open CommonFunctions
open System.Diagnostics.CodeAnalysis

// this method will given a language string return an AST based on FSLex and FSYacc
let mutable ParseCache : Map<string, LanguageExpression> = Map.empty
let PositionColumn = "WarewolfPositionColumn"

/// given a recordset identifier, return a list of data that matches the identifier
let evalRecordSetStarIndex (recset : WarewolfRecordset) (identifier : RecordSetColumnIdentifier) = 
    match recset.Optimisations with
    | Ordinal -> recset.Data.[identifier.Column]
    | Sorted -> recset.Data.[identifier.Column]
    | Fragmented -> 
        Seq.zip recset.Data.[PositionColumn] recset.Data.[identifier.Column]
        |> Seq.sort
        |> Seq.map snd
        |> fun a -> new WarewolfAtomList<WarewolfAtom>(WarewolfAtom.Nothing, a)

/// given a recordset identifier, return a list of data that matches the identifier and the positions of the corresponding atoms
let evalRecordSetStarIndexWithPositions (recset : WarewolfRecordset) (identifier : RecordSetColumnIdentifier) = 
    Seq.zip (Seq.map atomToInt recset.Data.[PositionColumn]) recset.Data.[identifier.Column] 
    |> Seq.map (fun a -> PositionedValue a)

/// given a recordset identifier, return a list of data that contains only the last atom from the recset
let evalRecordSetLastIndex (recset : WarewolfRecordset) (identifier : RecordSetColumnIdentifier) = 
    if recset.LastIndex = 0 then Nothing
    else 
        match recset.Optimisations with
        | Ordinal -> recset.Data.[identifier.Column].[recset.LastIndex - 1]
        | _ -> 
            let data = Seq.max recset.Data.[PositionColumn]
            let index = Seq.findIndex (fun a -> a = data) recset.Data.[PositionColumn]
            recset.Data.[identifier.Column].[index]

/// return a scalar from an environment
let evalScalar (scalarName : ScalarIdentifier) (env : WarewolfEnvironment) = 
    if env.Scalar.ContainsKey scalarName then (env.Scalar.[scalarName])
    else 
        raise 
            (new Dev2.Common.Common.NullValueInVariableException(sprintf "Scalar value { %s } is NULL" scalarName, 
                                                                 scalarName))

/// convert hte index type to a string
let rec IndexToString(x : Index) = 
    match x with
    | IntIndex a -> a.ToString()
    | Star -> "*"
    | Last -> ""
    | IndexExpression a -> languageExpressionToString a

/// If an atom is an int that is greater than 0 then return it otherwise fail
and getIntFromAtom (a : WarewolfAtom) = 
    match a with
    | Int x -> 
        if x <= 0 then failwith "invalid recordset index was less than 0"
        else x
    | _ -> 
        let couldParse, parsed = System.Int32.TryParse(a.ToString())
        if couldParse then parsed
        else failwith "index was not an int"

/// convert an indes expression to an actual index. this would evaluate the [[a]] in [[Rec([[a]]).a]]
and evalIndex (env : WarewolfEnvironment) (update : int) (exp : string) = 
    let getIntFromAtomList (a : WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>) = 
        match a.Count with
        | 1 -> a.[0] |> atomToInt
        | _ -> failwith "must be single value only"
    
    let evalled = eval env update false exp
    match evalled with
    | WarewolfAtomResult a -> getIntFromAtom a
    | WarewolfAtomListresult a -> getIntFromAtomList a
    | _ -> failwith "invalid recordset index was a list"

// the reverse of parsing an expression. Takes an expression and returnsa a string
and languageExpressionToString (x : LanguageExpression) = 
    match x with
    | RecordSetExpression a -> sprintf "[[%s(%s).%s]]" a.Name (IndexToString a.Index) a.Column
    | ScalarExpression a -> sprintf "[[%s]]" a
    | WarewolfAtomExpression a -> atomtoString a
    | JsonIdentifierExpression a -> 
        match a with
        | NameExpression x -> sprintf "[[@%s]]" x.Name
        | NestedNameExpression x -> sprintf "[[@%s.%s]]" x.ObjectName (jsonExpressionToString x.Next "")
        | Terminal -> ""
        | IndexNestedNameExpression x -> 
            sprintf "[[@%s(%s).%s]]" x.ObjectName (IndexToString x.Index) (jsonExpressionToString x.Next "")
    | ComplexExpression a -> List.fold (fun c d -> c + languageExpressionToString d) "" a
    | RecordSetNameExpression a -> sprintf "[[%s(%s)]]" a.Name (IndexToString a.Index)

and languageExpressionToStringNoBrackets (x : LanguageExpression) = 
    match x with
    | RecordSetExpression a -> sprintf "%s(%s).%s" a.Name (IndexToString a.Index) a.Column
    | ScalarExpression a -> sprintf "%s" a
    | WarewolfAtomExpression a -> atomtoString a
    | JsonIdentifierExpression a -> 
        match a with
        | NameExpression x -> sprintf "%s" x.Name
        | NestedNameExpression x -> sprintf "%s.%s" x.ObjectName (jsonExpressionToString x.Next "")
        | Terminal -> ""
        | IndexNestedNameExpression x -> 
            sprintf "%s(%s).%s" x.ObjectName (IndexToString x.Index) (jsonExpressionToString x.Next "")
    | ComplexExpression a -> List.fold (fun c d -> c + languageExpressionToString d) "" a
    | RecordSetNameExpression a -> sprintf "%s(%s)" a.Name (IndexToString a.Index)

/// convert a json expression to a string. uses an accumulater to append to while traversing an expression
and jsonExpressionToString a acc = 
    match a with
    | NameExpression x -> 
        if acc = "" then x.Name
        else sprintf "%s.%s" acc x.Name
    | NestedNameExpression x -> 
        let current = 
            if acc = "" then x.ObjectName
            else sprintf "%s.%s" acc x.ObjectName
        jsonExpressionToString x.Next current
    | IndexNestedNameExpression x -> 
        let accdot = 
            if acc = "" then ""
            else acc + "."
        
        let current = sprintf "%s%s(%s)" accdot x.ObjectName (IndexToString x.Index)
        jsonExpressionToString x.Next current
    | Terminal -> acc

/// Eval a recordset
and evalRecordsSet (recset : RecordSetColumnIdentifier) (env : WarewolfEnvironment) = 
    if not (env.RecordSets.ContainsKey recset.Name) then 
        raise 
            (new Dev2.Common.Common.NullValueInVariableException((sprintf "Variable { %s } is NULL." (recset.Name)), 
                                                                 recset.Name))
    else 
        match recset.Index with
        | IntIndex a -> 
            new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, 
                                                                           [ evalRecordSetIndex 
                                                                                 env.RecordSets.[recset.Name] recset a ])
        | Star -> evalRecordSetStarIndex env.RecordSets.[recset.Name] recset
        | Last -> 
            let value = evalRecordSetLastIndex env.RecordSets.[recset.Name] recset
            
            let data = 
                match value with
                | Nothing -> List.empty
                | _ -> [ value ]
            new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, data)
        | IndexExpression a -> 
            new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, 
                                                                           [ evalRecordSetIndex 
                                                                                 env.RecordSets.[recset.Name] recset 
                                                                                 (languageExpressionToString a 
                                                                                  |> (evalIndex env 0)) ])
///eval a recordset and also return the positions
and evalRecordsSetWithPositions (recset : RecordSetColumnIdentifier) (env : WarewolfEnvironment) = 
    if not (env.RecordSets.ContainsKey recset.Name) then 
        raise (new Dev2.Common.Common.NullValueInVariableException(recset.Index.ToString(), recset.Name))
    else 
        match recset.Index with
        | IntIndex a -> 
            new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, 
                                                                           [ evalRecordSetIndex 
                                                                                 env.RecordSets.[recset.Name] recset a ])
        | Star -> 
            new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, 
                                                                           (evalRecordSetStarIndexWithPositions 
                                                                                env.RecordSets.[recset.Name] recset))
        | Last -> 
            let value = evalRecordSetLastIndex env.RecordSets.[recset.Name] recset
            
            let data = 
                match value with
                | Nothing -> List.empty
                | _ -> [ value ]
            new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, data)
        | IndexExpression a -> 
            new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, 
                                                                           [ evalRecordSetIndex 
                                                                                 env.RecordSets.[recset.Name] recset 
                                                                                 (languageExpressionToString a 
                                                                                  |> (evalIndex env 0)) ])

///Eval a recordset and convert the result to a string
and evalRecordSetAsString (env : WarewolfEnvironment) (a : RecordSetColumnIdentifier) = 
    match a.Index with
    | IntIndex _ -> (evalRecordsSet a env).[0]
    | _ -> 
        (evalRecordsSet a env)
        |> Seq.map (atomtoString)
        |> (Seq.fold (+) "")
        |> DataString

/// Filter out empty expressions when parsing. this will not be needed if the language is simplified
and Clean(buffer : LanguageExpression) = 
    match buffer with
    | RecordSetExpression a -> RecordSetExpression a
    | ScalarExpression a -> ScalarExpression a
    | RecordSetNameExpression a -> RecordSetNameExpression a
    | WarewolfAtomExpression a -> WarewolfAtomExpression a
    | JsonIdentifierExpression a -> JsonIdentifierExpression a
    | ComplexExpression a -> 
        (List.filter (fun b -> "" <> (languageExpressionToString b)) a) |> (fun a -> 
        if (List.length a) = 1 then Clean a.[0]
        else ComplexExpression a)

///Simple parse. convert a string to a language expression
and parseLanguageExpressionWithoutUpdate (lang : string) : LanguageExpression = 
    if (lang.Contains "[[") then 
        let exp = ParseCache.TryFind lang
        match exp with
        | Some a -> a
        | None -> 
            try
                let lexbuf = LexBuffer<string>.FromString lang
                let buffer = Parser.start Lexer.tokenstream lexbuf
                let res = buffer |> Clean
                ParseCache <- ParseCache.Add(lang, res)
                res
            with
                | :? System.IndexOutOfRangeException as ex ->
                     raise ex
    else WarewolfAtomExpression(parseAtom lang)

 and parseLanguageExpressionWithoutUpdateStrict (lang : string) : LanguageExpression = 
    if (lang.Contains "[[" && lang.EndsWith"]]") then 
        let exp = ParseCache.TryFind lang
        match exp with
        | Some a -> a
        | None -> 
            try
                let lexbuf = LexBuffer<string>.FromString lang
                let buffer = Parser.start Lexer.tokenstream lexbuf
                let res = buffer |> Clean
                ParseCache <- ParseCache.Add(lang, res)
                res
            with
                | :? System.IndexOutOfRangeException as ex ->
                     raise ex
    else WarewolfAtomExpression(parseAtom lang)

///Simple parse. convert a string to a language expression and replace * with the update value
and parseLanguageExpression (lang : string) (update : int) : LanguageExpression = 
    let data = parseLanguageExpressionWithoutUpdate lang
    match update with
    | 0 -> data
    | _ -> 
        match data with
        | RecordSetExpression a -> 
            match a.Index with
            | Star -> { a with Index = IntIndex update } |> LanguageExpression.RecordSetExpression
            | _ -> a |> LanguageExpression.RecordSetExpression
        | RecordSetNameExpression a -> 
            match a.Index with
            | Star -> { a with Index = IntIndex update } |> LanguageExpression.RecordSetNameExpression
            | _ -> a |> LanguageExpression.RecordSetNameExpression
        | ComplexExpression p -> List.map (updateComplex update) p |> LanguageExpression.ComplexExpression
        | JsonIdentifierExpression a ->
            match a with
            | IndexNestedNameExpression b-> {b with Index = IntIndex update} |> JsonIdentifierExpression.IndexNestedNameExpression |> JsonIdentifierExpression
            | NestedNameExpression b -> 
                match b.Next with 
                | IndexNestedNameExpression x-> {b with 
                                                    ObjectName = b.ObjectName
                                                    Next = {x with Index = IntIndex update} |> IndexNestedNameExpression} |> NestedNameExpression |> JsonIdentifierExpression
                |_->data
            | Terminal _ -> data
            | _ -> data
        | _ -> data

and parseLanguageExpressionStrict (lang : string) (update : int) : LanguageExpression = 
    let data = parseLanguageExpressionWithoutUpdateStrict lang
    match update with
    | 0 -> data
    | _ -> 
        match data with
        | RecordSetExpression a -> 
            match a.Index with
            | Star -> { a with Index = IntIndex update } |> LanguageExpression.RecordSetExpression
            | _ -> a |> LanguageExpression.RecordSetExpression
        | RecordSetNameExpression a -> 
            match a.Index with
            | Star -> { a with Index = IntIndex update } |> LanguageExpression.RecordSetNameExpression
            | _ -> a |> LanguageExpression.RecordSetNameExpression
        | ComplexExpression p -> List.map (updateComplex update) p |> LanguageExpression.ComplexExpression
        | JsonIdentifierExpression a ->
            match a with
            | IndexNestedNameExpression b-> {b with Index = IntIndex update} |> JsonIdentifierExpression.IndexNestedNameExpression |> JsonIdentifierExpression
            | NestedNameExpression b -> 
                match b.Next with 
                | IndexNestedNameExpression x-> {b with 
                                                    ObjectName = b.ObjectName
                                                    Next = {x with Index = IntIndex update} |> IndexNestedNameExpression} |> NestedNameExpression |> JsonIdentifierExpression
                |_->data
            | Terminal _ -> data
            | _ -> data
        | _ -> data

/// replace the * in a complex expression with an update value
and updateComplex update data = 
    match data with
    | RecordSetExpression a -> 
        match a.Index with
        | Star -> { a with Index = IntIndex update } |> LanguageExpression.RecordSetExpression
        | _ -> a |> LanguageExpression.RecordSetExpression
    | RecordSetNameExpression a -> 
        match a.Index with
        | Star -> { a with Index = IntIndex update } |> LanguageExpression.RecordSetNameExpression
        | _ -> a |> LanguageExpression.RecordSetNameExpression
    | _ -> data

/// Evaluate a row from a record set
and evalARow (index : int) (recset : WarewolfRecordset) = 
    let blank = 
        Map.map (fun a _ -> 
            new WarewolfAtomList<WarewolfAtom>(WarewolfAtom.Nothing, 
                                               [ recset.Data.[a].[index]
                                                 |> warewolfAtomRecordtoString
                                                 |> DataString ])) recset.Data
    { recset with Data = blank }

//. evaluate a complete recordset [[Rec()]]
and evalDataSetExpression (env : WarewolfEnvironment) (update : int) (name : RecordSetName) = 
    if env.RecordSets.ContainsKey(name.Name) then 
        let recset = env.RecordSets.[name.Name]
        match name.Index with
        | Star -> WarewolfRecordSetResult env.RecordSets.[name.Name]
        | IntIndex a -> WarewolfRecordSetResult(evalARow (getRecordSetIndexAsInt recset a) recset)
        | Last -> WarewolfRecordSetResult(evalARow (getRecordSetIndexAsInt recset recset.LastIndex) recset)
        | IndexExpression b -> 
            let res = eval env update false (languageExpressionToString b) |> evalResultToString
            match b with
            | _ -> eval env update false (sprintf "[[%s(%s)]]" name.Name res)
    else raise (new Dev2.Common.Common.NullValueInVariableException("Recordset not found", name.Name))

///overiding eval functions
/// take a string then pasre it and call one of the child functions

and eval (env : WarewolfEnvironment) (update : int) (shouldEscape:bool) (lang : string) : WarewolfEvalResult = 
    if lang.StartsWith(Dev2.Common.GlobalConstants.CalculateTextConvertPrefix) then
        evalForCalculate env update lang
    elif lang.StartsWith(Dev2.Common.GlobalConstants.AggregateCalculateTextConvertPrefix) then
       evalForCalculateAggregate env update lang
    else
        let EvalComplex (exp:LanguageExpression list) = 
            if((List.length exp) =1) then
                match exp.[0] with
                    | RecordSetExpression a ->  WarewolfAtomListresult(  (evalRecordsSet a env) )
                    | ScalarExpression a ->  WarewolfAtomResult (evalScalar a env)
                    | WarewolfAtomExpression a ->  WarewolfAtomResult a
                    | _ ->failwith "you should not get here"
            else    
                let start = List.map languageExpressionToString  exp |> (List.fold (+) "")
                let evaled = (List.map (languageExpressionToString >> (eval  env update shouldEscape)>>evalResultToStringAsList)  exp )  
                let apply (fList: ('a->'b) list) (xList: 'a list)  = 
                    [ for f in fList do
                        for x in xList do
                        yield f x ]  

                let (<!>) = List.map
                let (<*>) = apply
                if Seq.isEmpty evaled then WarewolfAtomResult (DataString "") 
                else
                let vals = List.ofSeq (List.ofSeq evaled |> List.reduce (fun acc elem ->
                                                                let x = List.ofSeq acc
                                                                let y = List.ofSeq elem
                                                                let z = (+) <!> x <*> y
                                                                Seq.ofList z)) 
                if (List.length vals = 1 && (not (vals.Head.Contains("[[")))) then
                    WarewolfAtomResult (DataString vals.Head)
                else
                    let checkVal (str:string) =
                        if(str.Contains("[[") && (not (str=start))) then
                            eval env update shouldEscape str
                        else
                            WarewolfAtomResult (DataString str)                    
                    let atoms = List.map checkVal vals
                    let v = List.map evalResultToString atoms
                    let x = List.map DataString v 
                    WarewolfAtomListresult( WarewolfAtomList<WarewolfAtomRecord>(DataString "",x))
        
        let buffer = parseLanguageExpression lang update
        match buffer with
        | RecordSetExpression a when env.RecordSets.ContainsKey a.Name -> let b = evalRecordsSet a env
                                                                          match shouldEscape with
                                                                            |true -> let d =  Seq.map (warewolfAtomRecordtoString >> System.Text.RegularExpressions.Regex.Escape >> DataString) b |> fun a -> a                                                                                      
                                                                                     WarewolfAtomListresult( WarewolfAtomList<WarewolfAtomRecord>(DataString "",d))
                                                                            |false-> WarewolfAtomListresult(b)
                                                                          
        | ScalarExpression a when env.Scalar.ContainsKey a ->
                                                             let b =  evalScalar a env
                                                             match shouldEscape with 
                                                                | true -> let c = System.Text.RegularExpressions.Regex.Escape(atomtoString b)
                                                                          WarewolfAtomResult(DataString c)
                                                                | false -> WarewolfAtomResult(b)
                                                             
        | WarewolfAtomExpression a -> WarewolfAtomResult a
        | ComplexExpression a -> EvalComplex(List.filter (fun b -> "" <> (languageExpressionToString b)) a)
        | RecordSetNameExpression a when env.RecordSets.ContainsKey a.Name -> evalDataSetExpression env update a
        | JsonIdentifierExpression a -> evalJson env update shouldEscape buffer
        | _ -> raise (new Dev2.Common.Common.NullValueInVariableException("variable not found",languageExpressionToString buffer))
///convert a warewolf language expressiom to JsonPath
and languageExpressionToJPath (lang : LanguageExpression) = 
    match lang with
    | RecordSetExpression a -> 
        match a.Index with
        | IntIndex i -> sprintf "[%i].%s" (i - 1) a.Column
        | Star -> "[*]." + a.Column
        | Last -> "[-1:]." + a.Column
        | _ -> failwith "not supported for JSON types"
    | ScalarExpression _ -> ""
    | WarewolfAtomExpression _ -> ""
    | RecordSetNameExpression a -> 
        match a.Index with
        | IntIndex i -> sprintf "[%i]" (i - 1)
        | Star -> "[*]"
        | Last -> "[-1:]"
        | _ -> failwith "not supported for JSON types"
    | ComplexExpression _ -> failwith "not supported for JSON types"
    | JsonIdentifierExpression a -> jsonIdentifierToJsonPathLevel1 a
    
///Convert a jsonIdentifierExpression to jsonPath
and jsonIdentifierToJsonPath (a : JsonIdentifierExpression) (accx : string) = 
    let acc = 
        if accx = "" then ""
        else accx + "."
    match a with
    | NameExpression x -> acc + x.Name
    | NestedNameExpression x -> (jsonIdentifierToJsonPath x.Next (acc + x.ObjectName))
    | IndexNestedNameExpression x -> 
        let index = 
            match x.Index with
            | IntIndex i -> sprintf "[%i]" (i - 1)
            | Star -> "[*]"
            | Last -> "[-1:]"
            | _ -> failwith "not supported for JSON types"
        (jsonIdentifierToJsonPath x.Next (acc + x.ObjectName+ "." + index))       
    | Terminal -> accx
// treat the head of the expression as a special case
and jsonIdentifierToJsonPathLevel1 (a : JsonIdentifierExpression) = 
    match a with
    | NameExpression x -> x.Name
    | NestedNameExpression x -> (jsonIdentifierToJsonPath x.Next "")
    | IndexNestedNameExpression x ->
                                    let index = 
                                        match x.Index with
                                            | IntIndex i -> sprintf "[%i]" (i - 1)
                                            | Star -> "[*]"
                                            | Last -> "[-1:]"
                                            | _ -> failwith "not supported for JSON types"
                                    match x.Next with
                                        | Terminal -> "."+index
                                        | _->    (jsonIdentifierToJsonPath x.Next index)
    | Terminal -> ""
/// get the name of the object from an expression
and jsonIdentifierToName (a : JsonIdentifierExpression) = 
    match a with
    | NameExpression x -> x.Name
    | NestedNameExpression x -> x.ObjectName
    | IndexNestedNameExpression x -> x.ObjectName
    | Terminal -> ""
///evaluate Json. Convert warewolf expression to jsonpath and evaluate it
and evalJson (env : WarewolfEnvironment) (update : int) (shouldEscape:bool) (lang : LanguageExpression) =     
    match lang with
    | ScalarExpression a -> 
        if env.JsonObjects.ContainsKey a then WarewolfAtomResult(DataString(env.JsonObjects.[a].ToString()))
        else failwith "non existent object"
    | RecordSetExpression a -> 
        let jPath = "$." + languageExpressionToJPath (lang)
        if env.JsonObjects.ContainsKey a.Name then 
            let jo = env.JsonObjects.[a.Name]
            let data = jo.SelectTokens(jPath) |> Seq.map (fun a -> WarewolfAtomRecord.DataString(a.ToString()))
            WarewolfAtomListresult
                (new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, data))
        else failwith "non existent object"
    | RecordSetNameExpression a -> 
        let jPath = "$." + languageExpressionToJPath (lang)
        if env.JsonObjects.ContainsKey a.Name then 
            let jo = env.JsonObjects.[a.Name]
            let data = jo.SelectTokens(jPath) |> Seq.map (fun a -> WarewolfAtomRecord.DataString(a.ToString()))
            WarewolfAtomListresult
                (new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, data))
        else failwith "non existent object"
    
    | JsonIdentifierExpression a -> 
        let jPath = "$." + languageExpressionToJPath (lang)
        if env.JsonObjects.ContainsKey(jsonIdentifierToName a) then 
            let jo = env.JsonObjects.[(jsonIdentifierToName a)]
            let data = jo.SelectTokens(jPath) |> Seq.map (fun a -> WarewolfAtomRecord.DataString(a.ToString()))
            if Seq.length data = 1 then
                WarewolfAtomResult(Seq.exactlyOne data)
            else
                if Seq.isEmpty data then
                    if jPath = "$."+(jsonIdentifierToName a) then
                        WarewolfAtomResult(WarewolfAtom.DataString(jo.ToString()))
                    else failwith "non existent object"                        
                else
                    WarewolfAtomListresult (new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, data))
        else failwith "non existent object"
    | ComplexExpression a -> eval env update shouldEscape (languageExpressionToString lang)
    | WarewolfAtomExpression a -> WarewolfAtomResult(a)
//specialise eval for calculate. Its just eval with the addition of some quotes. can be merged into eval function at the expense of complexity for c# developers
and  evalForCalculate  (env: WarewolfEnvironment)  (update:int) (langs:string) : WarewolfEvalResult=
    let lang = reduceForCalculate env update langs

    let EvalComplex (exp:LanguageExpression list) = 
            if((List.length exp) =1) then
                match exp.[0] with
                    | RecordSetExpression a ->  WarewolfAtomListresult(  (evalRecordsSet a env) )
                    | ScalarExpression a ->  WarewolfAtomResult (evalScalar a env)
                    | WarewolfAtomExpression a ->  WarewolfAtomResult a
                    | _ ->failwith "you should not get here"
            else    
                let start = List.map languageExpressionToString  exp |> (List.fold (+) "")
                let evaled = (List.map (languageExpressionToString >> (evalForCalculate  env update)>>evalResultToStringAsList)  exp )
                let apply (fList: ('a->'b) list) (xList: 'a list)  = 
                    [ for f in fList do
                        for x in xList do
                        yield f x ]  

                let (<!>) = List.map
                let (<*>) = apply
                if Seq.isEmpty evaled then WarewolfAtomResult (DataString "") 
                else
                let vals = List.ofSeq (List.ofSeq evaled |> List.reduce (fun acc elem ->
                                                                let x = List.ofSeq acc
                                                                let y = List.ofSeq elem
                                                                let z = (+) <!> x <*> y
                                                                Seq.ofList z)) 
                if (List.length vals = 1 && (not (vals.Head.Contains("[[")))) then
                    WarewolfAtomResult (DataString vals.Head)
                else
                    let checkVal (str:string) =
                        if(str.Contains("[[") && (not (str=start))) then
                            eval env update false str
                        else
                            WarewolfAtomResult (DataString str)                    
                    let atoms = List.map checkVal vals
                    let v = List.map evalResultToString atoms
                    let x = List.map DataString v 
                    WarewolfAtomListresult( WarewolfAtomList<WarewolfAtomRecord>(DataString "",x))
    
    let buffer =  parseLanguageExpression lang update
                        
    match buffer with
        | RecordSetExpression a ->  evalRecordsSet a env |> Seq.map enQuote |> (fun x-> new WarewolfAtomList<WarewolfAtom>(Nothing,x) )|> WarewolfAtomListresult 
        | ScalarExpression a -> WarewolfAtomResult (evalScalar a env|>enQuote)
        | WarewolfAtomExpression a -> WarewolfAtomResult a
        | RecordSetNameExpression x ->evalDataSetExpression env update x
        | ComplexExpression  a ->  EvalComplex ( List.filter (fun b -> "" <> (languageExpressionToString b)) a) 
        | JsonIdentifierExpression a -> let res = evalJson env update false buffer
                                        match res with
                                            | WarewolfAtomListresult x -> WarewolfAtomListresult (x|> Seq.map enQuote |> (fun x-> new WarewolfAtomList<WarewolfAtom>(Nothing,x) ))
                                            | WarewolfAtomResult x -> WarewolfAtomResult(x|>enQuote)
                                            | _ -> failwith (sprintf "failed to evaluate [[%s]]"  (languageExpressionToString buffer))
                                                    
and  evalForCalculateAggregate  (env: WarewolfEnvironment)  (update:int) (langs:string) : WarewolfEvalResult=
    let lang = reduceForCalculate env update langs

    let EvalComplex (exp:LanguageExpression list) = 
        if((List.length exp) =1) then
            match exp.[0] with
                | RecordSetExpression a ->  evalRecordSetAsString env a
                | ScalarExpression a ->  (evalScalar a env)
                | WarewolfAtomExpression a ->  a
                | _ ->failwith "you should not get here"
        else    
            let start = List.map languageExpressionToString  exp |> (List.fold (+) "")
            let evaled = (List.map (languageExpressionToString >> (evalForCalculateAggregate   env update)>>evalResultToString)  exp )|> (List.fold (+) "")
            if( evaled = start || (not (evaled.Contains("[[")))) then
                DataString evaled
            else DataString (eval env update false evaled |>  evalResultToString)
    
    let buffer =  parseLanguageExpression lang update
                        
    match buffer with
        | RecordSetExpression a ->  evalRecordsSet a env |> Seq.map enQuote |> (fun x-> new WarewolfAtomList<WarewolfAtom>(Nothing,x) )|> WarewolfAtomListresult 
        | ScalarExpression a -> WarewolfAtomResult (evalScalar a env|>enQuote)
        | WarewolfAtomExpression a -> WarewolfAtomResult a
        | RecordSetNameExpression x ->evalDataSetExpression env update x
        | ComplexExpression  a ->  WarewolfAtomResult (EvalComplex ( List.filter (fun b -> "" <> (languageExpressionToString b)) a)) 
        | JsonIdentifierExpression a -> let res = evalJson env update false buffer
                                        match res with
                                            | WarewolfAtomListresult x -> WarewolfAtomListresult (x|> Seq.map enQuote |> (fun x-> new WarewolfAtomList<WarewolfAtom>(Nothing,x) ))
                                            | WarewolfAtomResult x -> WarewolfAtomResult(x|>enQuote)
                                            | _ -> failwith (sprintf "failed to evaluate [[%s]]"  (languageExpressionToString buffer))
/// simplify a calculate expression
and reduceForCalculate (env : WarewolfEnvironment) (update : int) (langs : string) : string = 
    let lang =
        match langs with
        | " " -> langs
        | _ -> langs.Trim()
    let exp = ParseCache.TryFind lang
    
    let buffer = 
        match exp with
        | Some a when update = 0 -> a
        | _ -> 
            let temp = parseLanguageExpression lang update
            temp
    match buffer with
    | ComplexExpression a -> 
        if (List.exists (isNotAtomAndNotcomplex a) a) then 
            List.map languageExpressionToString a
            |> List.map (eval env update false)
            |> List.map evalResultToString
            |> fun a -> System.String.Join("", a) |> (fun a -> reduceForCalculate env update a)
        else lang
    | RecordSetExpression a -> 
        match a.Index with
        | IndexExpression exp -> 
            match exp with
            | WarewolfAtomExpression _ -> lang
            | _ -> 
                sprintf "[[%s(%s).%s]]" a.Name (eval env update false (languageExpressionToString exp) |> evalResultToString) 
                    a.Column
        | _ -> lang
    | JsonIdentifierExpression a -> lang
    | _ -> lang

/// get the base expression given a string [[[[a]]] could evaluate to [[b]] for instance
and evalToExpressionAndParse (env : WarewolfEnvironment) (update : int) (langs : string) = 
    evalToExpression env update langs |> fun a -> parseLanguageExpression a update

/// get the base expression given a string [[[[a]]] could evaluate to [[b]] for instance
and evalToExpression (env : WarewolfEnvironment) (update : int) (langs : string) : string = 
    let lang = langs.Trim()
    let exp = ParseCache.TryFind lang
    
    let buffer = 
        match exp with
        | Some a when update = 0 -> a
        | _ -> 
            let temp = parseLanguageExpression lang update
            temp
    match buffer with
    | ComplexExpression a -> 
        if (List.exists isNotAtom a) then 
            let ev = 
                List.map languageExpressionToString a
                |> List.map (eval env update false)
                |> List.map evalResultToString
                |> fun a -> System.String.Join("", a) |> (fun a -> evalToExpression env update a)
            if ev.Contains("[[") then ev
            else languageExpressionToString buffer
        else lang
    | RecordSetExpression a -> 
        match a.Index with
        | IndexExpression exp -> 
            match exp with
            | WarewolfAtomExpression _ -> lang
            | _ -> 
                sprintf "[[%s(%s).%s]]" a.Name (eval env update false (languageExpressionToString exp) |> evalResultToString) 
                    a.Column
        | _ -> buffer |> languageExpressionToString
    | _ -> lang


//specialise eval with Positions. Its just eval with the addition of some ints. can be merged into eval function at the expense of complexity for c# developers
and evalWithPositions (env : WarewolfEnvironment) (update : int) (lang : string) : WarewolfEvalResult = 
    let EvalComplex(exp : LanguageExpression list) = 
            let start = List.map languageExpressionToString exp |> (List.fold (+) "")
            
            let evaled = 
                (List.map (languageExpressionToString
                           >> (eval env update false)
                           >> evalResultToString) exp)
                |> (List.fold (+) "")
            if (evaled = start) then DataString evaled
            else DataString(eval env update false evaled |> evalResultToString)
    
    let exp = ParseCache.TryFind lang
    
    let buffer = 
        match exp with
        | Some a when update = 0 -> a
        | _ -> 
            let temp = parseLanguageExpression lang update
            temp
    match buffer with
    | RecordSetExpression a -> WarewolfAtomListresult((evalRecordsSetWithPositions a env))
    | ScalarExpression a -> WarewolfAtomResult(evalScalar a env)
    | WarewolfAtomExpression a -> WarewolfAtomResult a
    | RecordSetNameExpression x -> evalDataSetExpression env update x
    | ComplexExpression a -> 
        WarewolfAtomResult(EvalComplex(List.filter (fun b -> "" <> (languageExpressionToString b)) a))
    | JsonIdentifierExpression _ -> failwith "Json Expression Not Supported"

///helper function for eval with positions
and evalRecordSetIndexes (env : WarewolfEnvironment) (recset : RecordSetName) : int seq = 
    match recset.Index with
    | IntIndex _ -> { 1..1 }
    | Star -> getRecordSetPositionsAsInts env.RecordSets.[recset.Name]
    | Last -> getRecordSetPositionsAsInts env.RecordSets.[recset.Name]
    | IndexExpression _ -> { 1..1 }

///helper function for eval with positions
and evalIndexes (env : WarewolfEnvironment) (update : int) (lang : string) = 
    let EvalComplex(exp : LanguageExpression list) = 
        if ((List.length exp) = 1) then 
            match exp.[0] with
            | RecordSetNameExpression a -> evalRecordSetIndexes env a
            | _ -> failwith "not a recordset"
        else failwith "not a recordset"
    
    let exp = ParseCache.TryFind lang
    
    let buffer = 
        match exp with
        | Some a when update = 0 -> a
        | _ -> 
            let temp = parseLanguageExpression lang update
            temp
    match buffer with
    | ComplexExpression a -> EvalComplex(List.filter (fun b -> "" <> (languageExpressionToString b)) a)
    | RecordSetNameExpression a -> evalRecordSetIndexes env a
    | _ -> failwith "not a recordset"
///Create the list data for a column
let createEmpty (length : int) (count : int) = 
    new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, 
                                                                   seq { 
                                                                       for _ in 1..length do
                                                                           yield Nothing
                                                                   }, count)
/// Add to a list
let addToList (lst : WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>) (value : WarewolfAtomRecord) = 
    lst.AddSomething value
    lst
/// add nothing to a column
let addNothingToList (lst : WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>) = 
    lst.AddNothing()
    lst

/// all recordsets have a hidden column for positions. this function gets that column. 
let getPositionFromRecset (rset : WarewolfRecordset) (columnName : string) = 
    if rset.Data.ContainsKey(columnName) then 
        if rset.Data.[columnName].Count = 0 then 1
        else 
            let posValue = rset.Data.[columnName].[rset.Data.[columnName].Count - 1]
            match posValue with
            | Nothing -> rset.Frame
            | _ -> rset.LastIndex + 1
    else 
        match rset.Frame with
        | 0 -> (rset.LastIndex) + 1
        | _ -> max (rset.LastIndex) (rset.Frame)
/// create a column filled with a single value
let createFilled (count : int) (value : WarewolfAtom) : WarewolfColumnData = 
    new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing, 
                                                                   seq { 
                                                                       for _ in 1..count do
                                                                           yield value
                                                                   })
/// get the indexes froma column
let getIndexes (name : string) (env : WarewolfEnvironment) = evalIndexes env 0 name
