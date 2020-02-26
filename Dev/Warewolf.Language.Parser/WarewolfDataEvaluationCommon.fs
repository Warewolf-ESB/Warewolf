module WarewolfDataEvaluationCommon


open LanguageAST
open Dev2.Common.Interfaces
open Microsoft.FSharp.Text.Lexing
open DataStorage
open WarewolfParserInterop
open Newtonsoft.Json.Linq
open CommonFunctions
open EvaluationFunctions
// this method will given a language string return an AST based on FSLex and FSYacc

let mutable ParseCache = Map.empty : Map<string,LanguageExpression>

let PositionColumn = "WarewolfPositionColumn"

let createDataSet (a : string) = 
    let col = new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing)
    { Data = [ (PositionColumn, col) ] |> Map.ofList
      Optimisations = Ordinal
      LastIndex = 0
      Frame = 0 }


let enQuote (atom:WarewolfAtom) = 
    match atom with 
        | DataString a -> DataString (sprintf "\"%s\"" a)
        |_ -> atom

let atomtoString (x:WarewolfAtom )=
    match x with 
        | Float a -> let places = GetDecimalPlaces a
                     a.ToString(sprintf "F%i" places)
        | Int a -> a.ToString()
        | DataString a -> a
        | JsonObject a -> a.ToString()
        | Nothing -> null
        | NullPlaceholder -> null
        | PositionedValue (_,b) -> b.ToString()
let warewolfAtomRecordtoString (x:WarewolfAtomRecord )=
    match x with 
        | Float a -> let places = GetDecimalPlaces a
                     a.ToString(sprintf "F%i" places)
        | Int a -> a.ToString()
        | DataString a -> a
        | JsonObject a -> a.ToString()
        | Nothing -> ""
        | NullPlaceholder -> ""
        | PositionedValue (_,b) -> b.ToString()

let evalResultToString (a:WarewolfEvalResult) = 
    match a with
    | WarewolfAtomResult x -> atomtoString x
    | WarewolfAtomListresult x -> Seq.map warewolfAtomRecordtoString x |> fun a->System.String.Join(",",a)
    | WarewolfRecordSetResult x -> Map.toList x.Data |> List.filter (fun (a, _) ->not (a=PositionColumn)) |>  List.map snd |> Seq.collect (fun a->a) |> fun a->System.String.Join(",",a)


let evalResultToStringNoCommas (a:WarewolfEvalResult) = 
    match a with
    | WarewolfAtomResult x -> atomtoString x
    | WarewolfAtomListresult x -> Seq.map warewolfAtomRecordtoString x |> (Seq.fold (+) "")
    | WarewolfRecordSetResult x -> Map.toList x.Data |> List.filter (fun (a, _) ->not (a=PositionColumn)) |> List.map snd |> Seq.collect (fun a->a) |> fun a->System.String.Join("",a)
let atomToInt(a:WarewolfAtom) = 
    match a with
        | Int x -> if x<= 0 then failwith "invalid recordset index was less than 0" else x
        |_ ->    let couldParse, parsed = System.Int32.TryParse(a.ToString())
                 if couldParse then parsed else failwith "index was not an int"

type PositionValue =
    | IndexFoundPosition of int
    | IndexDoesNotExist

let getRecordSetIndex (recset:WarewolfRecordset) (position:int) =
    match recset.Optimisations with
    | Ordinal -> if recset.LastIndex <  position then
                    IndexDoesNotExist
                 else
                    IndexFoundPosition (position-1)
    | _->
            let indexes = recset.Data.[PositionColumn]
            let positionAsAtom = Int position
            try  Seq.findIndex (fun a->  a=positionAsAtom) indexes |> IndexFoundPosition
            with     
            | :? System.Collections.Generic.KeyNotFoundException as a -> IndexDoesNotExist

let getRecordSetIndexAsInt (recset:WarewolfRecordset) (position:int) =
    match recset.Optimisations with
    | Ordinal -> if recset.LastIndex <  position then
                    failwith "row does not exist"
                 else
                    position - 1
    | _->
            let indexes = recset.Data.[PositionColumn]
            let positionAsAtom = Int position
            try  Seq.findIndex (fun a->  a=positionAsAtom) indexes 
            with     
            | :? System.Collections.Generic.KeyNotFoundException as ex ->  failwith ("row does not exist" + ex.Message)

let evalRecordSetIndex (recset:WarewolfRecordset) (identifier:RecordSetColumnIdentifier) (position:int) =
    let index = getRecordSetIndex recset position
    match index with 
    | IndexFoundPosition a -> recset.Data.[identifier.Column].[a]
    | IndexDoesNotExist -> raise (new Dev2.Common.Common.NullValueInVariableException("index not found",identifier.Name))


let evalRecordSetStarIndex (recset:WarewolfRecordset) (identifier:RecordSetColumnIdentifier)  =
    match recset.Optimisations with 
    |Ordinal -> recset.Data.[identifier.Column] 
    | Sorted -> recset.Data.[identifier.Column]
    | Fragmented-> Seq.zip  recset.Data.[PositionColumn] recset.Data.[identifier.Column] |> Seq.sort |>Seq.map snd |> fun a -> new WarewolfAtomList<WarewolfAtom>(WarewolfAtom.Nothing,a)
let evalRecordSetStarIndexWithPositions (recset:WarewolfRecordset) (identifier:RecordSetColumnIdentifier)  =
    Seq.zip ( Seq.map atomToInt recset.Data.[PositionColumn]) recset.Data.[identifier.Column]  |> Seq.map (fun a -> PositionedValue a)

let evalRecordSetLastIndex (recset:WarewolfRecordset) (identifier:RecordSetColumnIdentifier)  =
    if Seq.isEmpty recset.Data.[PositionColumn] then
        Nothing
    else
        let data = Seq.max recset.Data.[PositionColumn] 
        let index = Seq.findIndex (fun a -> a=data) recset.Data.[PositionColumn] 
        recset.Data.[identifier.Column].[index]

let evalScalar (scalarName:ScalarIdentifier) (env:WarewolfEnvironment) =
    if env.Scalar.ContainsKey scalarName
    then     ( env.Scalar.[scalarName])
    else raise (new Dev2.Common.Common.NullValueInVariableException(sprintf "Scalar value { %s } is NULL" scalarName,scalarName))
             
let rec IndexToString (x:Index) =
    match x with 
        | IntIndex a -> a.ToString()
        | Star -> "*"
        | Last -> ""
        | IndexExpression a-> languageExpressionToString a

and getIntFromAtom (a:WarewolfAtom) =
        match a with
        | Int x -> if x<= 0 then failwith "invalid recordset index was less than 0" else x
        |_ ->    let couldParse, parsed = System.Int32.TryParse(a.ToString())
                 if couldParse then parsed else failwith "index was not an int"

and evalIndex  ( env:WarewolfEnvironment) (update:int) (exp:string)=

    let getIntFromAtomList (a:WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>) =
        match a.Count with
        | 1 -> a.[0]|>atomToInt
        |_ -> failwith "must be single value only"

    let evalled = eval env update false exp 
    match evalled with
    | WarewolfAtomResult a -> getIntFromAtom a 
    | WarewolfAtomListresult a -> getIntFromAtomList a 
    |_ ->failwith "invalid recordset index was a list"

and evalRecordsSet (recset:RecordSetColumnIdentifier) (env: WarewolfEnvironment)  =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        raise (new Dev2.Common.Common.NullValueInVariableException((sprintf "Variable { %s } is NULL." (recset.Name) ),recset.Name))
            
    else
            match recset.Index with
                | IntIndex a -> new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing,[ evalRecordSetIndex env.RecordSets.[recset.Name] recset a])
                | Star ->  evalRecordSetStarIndex env.RecordSets.[recset.Name] recset
                | Last -> let value = evalRecordSetLastIndex env.RecordSets.[recset.Name] recset 
                          let data = match value with
                                      | Nothing ->List.empty
                                      | _ ->  [ value]
                          new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing,data)
                | IndexExpression a -> new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing,[ evalRecordSetIndex env.RecordSets.[recset.Name] recset ( languageExpressionToString a|>(evalIndex env 0)) ])

and evalRecordsSetWithPositions (recset:RecordSetColumnIdentifier) (env: WarewolfEnvironment)  =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        raise (new Dev2.Common.Common.NullValueInVariableException(recset.Index.ToString(),recset.Name))
            
    else
            match recset.Index with
                | IntIndex a -> new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing,[ evalRecordSetIndex env.RecordSets.[recset.Name] recset a])
                | Star ->  new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing,(evalRecordSetStarIndexWithPositions env.RecordSets.[recset.Name] recset))
                | Last -> let value = evalRecordSetLastIndex env.RecordSets.[recset.Name] recset 
                          let data = match value with
                                      | Nothing ->List.empty
                                      | _ ->  [ value]
                          new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing,data)
                | IndexExpression a -> new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing,[ evalRecordSetIndex env.RecordSets.[recset.Name] recset ( languageExpressionToString a|>(evalIndex env 0)) ])


and evalRecordSetAsString (env: WarewolfEnvironment) (a:RecordSetColumnIdentifier) = 
    match a.Index with
    | IntIndex _ -> (evalRecordsSet a env).[0]
    |_ ->(evalRecordsSet a env) |> Seq.map (atomtoString ) |> (Seq.fold (+) "") |> DataString

and  Clean (buffer :LanguageExpression) =
    match buffer with
        | RecordSetExpression a -> RecordSetExpression a
        | ScalarExpression a -> ScalarExpression a
        | RecordSetNameExpression a -> RecordSetNameExpression a
        | WarewolfAtomExpression a -> WarewolfAtomExpression a
        | ComplexExpression  a ->  (List.filter (fun b -> "" <> (languageExpressionToString b)) a) |> (fun a -> if (List.length a) =1 then Clean a.[0] else ComplexExpression a)
        | JsonIdentifierExpression a -> JsonIdentifierExpression a

and parseAtom (lang:string) =
    let at =  tryParseAtom lang
    match at with
        | Int _ -> at 
        | Float _ ->  tryFloatParseAtom lang
        | _ -> at
            
and parseLanguageExpressionWithoutUpdate  (lang:string) : LanguageExpression=
    if( lang.Contains"[[")
    then 
        let exp = ParseCache.TryFind lang
        match exp with 
        | Some a  ->  a
        | None -> 
                    let lexbuf = LexBuffer<string>.FromString lang 
                    let buffer = Parser.start Lexer.tokenstream lexbuf
                    let res = buffer |> Clean
                    ParseCache<-ParseCache.Add(lang,res)
                    res
    else WarewolfAtomExpression (parseAtom lang)

and parseLanguageExpression  (lang:string)  (update:int): LanguageExpression=
    let data = parseLanguageExpressionWithoutUpdate lang
    match update with 
    |0 -> data
    |_->  match data with 
            | RecordSetExpression a -> match a.Index with
                                        | Star -> {a with Index=IntIndex update } |> LanguageExpression.RecordSetExpression
                                        | _-> a |> LanguageExpression.RecordSetExpression
            | RecordSetNameExpression a -> match a.Index with
                                                    | Star -> {a with Index=IntIndex update } |> LanguageExpression.RecordSetNameExpression
                                                    | _-> a |> LanguageExpression.RecordSetNameExpression
            | ComplexExpression p -> List.map (updateComplex update) p |> LanguageExpression.ComplexExpression
            | _->data

and updateComplex   update data = 
    match data with 
                | RecordSetExpression a -> match a.Index with
                                            | Star -> {a with Index=IntIndex update } |> LanguageExpression.RecordSetExpression
                                            | _-> a |> LanguageExpression.RecordSetExpression
                | RecordSetNameExpression a -> match a.Index with
                                                        | Star -> {a with Index=IntIndex update } |> LanguageExpression.RecordSetNameExpression
                                                        | _-> a |> LanguageExpression.RecordSetNameExpression
                | _->data

and evalARow  ( index:int) (recset:WarewolfRecordset) =
    let blank = Map.map (fun a _ -> new WarewolfAtomList<WarewolfAtom>(WarewolfAtom.Nothing, [ recset.Data.[a].[index] |> warewolfAtomRecordtoString|> DataString ])) recset.Data
    {recset with Data = blank}

and evalDataSetExpression (env: WarewolfEnvironment) (update:int) (name:RecordSetName) =
    if env.RecordSets.ContainsKey( name.Name) then
        let recset =  env.RecordSets.[name.Name]        
        match name.Index with
            | Star -> WarewolfRecordSetResult env.RecordSets.[name.Name]
            | IntIndex a -> WarewolfRecordSetResult (evalARow (getRecordSetIndexAsInt recset a) recset )
            | Last  -> WarewolfRecordSetResult ( evalARow  (getRecordSetIndexAsInt recset recset.LastIndex) recset )
            | IndexExpression b -> 
                                   let res = eval env update false (languageExpressionToString b) |> evalResultToString
                                   match b with 
                                        | WarewolfAtomExpression atom ->
                                                    match atom with
                                                    | Int a ->  WarewolfRecordSetResult (evalARow (getRecordSetIndexAsInt recset a) recset )
                                                    | _ -> failwith "non int index found"
                                        | _ ->   eval env update false ( sprintf "[[%s(%s)]]" name.Name res)
    else
        raise (new Dev2.Common.Common.NullValueInVariableException("Recordset not found",name.Name))

          
and  eval  (env: WarewolfEnvironment)  (update:int) (shouldEscape:bool) (lang:string) : WarewolfEvalResult=
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
    
        let buffer =  parseLanguageExpression lang update
                        
        match buffer with
            | RecordSetExpression a -> WarewolfAtomListresult(  (evalRecordsSet a env) )
            | ScalarExpression a -> WarewolfAtomResult (evalScalar a env)
            | WarewolfAtomExpression a -> WarewolfAtomResult a
            | RecordSetNameExpression x ->evalDataSetExpression env update x
            | ComplexExpression  a -> EvalComplex ( List.filter (fun b -> "" <> (languageExpressionToString b)) a)
            | JsonIdentifierExpression a -> let res = evalJson env update shouldEscape buffer
                                            match res with
                                            | WarewolfAtomListresult x -> WarewolfAtomListresult (x)
                                            | WarewolfAtomResult x -> WarewolfAtomResult(x)
                                            | _ -> failwith (sprintf "failed to evaluate [[%s]]"  (languageExpressionToString buffer))

and  evalForJson  (env: WarewolfEnvironment)  (update:int) (shouldEscape:bool) (lang:string) : WarewolfEvalResult=
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
    
        let buffer =  parseLanguageExpression lang update
                        
        match buffer with
            | RecordSetExpression a -> WarewolfAtomListresult(  (evalRecordsSet a env) )
            | ScalarExpression a -> WarewolfAtomResult (evalScalar a env)
            | WarewolfAtomExpression a -> WarewolfAtomResult a
            | RecordSetNameExpression x ->evalDataSetExpression env update x
            | ComplexExpression  a -> EvalComplex ( List.filter (fun b -> "" <> (languageExpressionToString b)) a)
            | JsonIdentifierExpression a -> let res = evalJsonForJson env update shouldEscape buffer
                                            match res with
                                            | WarewolfAtomListresult x -> WarewolfAtomListresult (x)
                                            | WarewolfAtomResult x -> WarewolfAtomResult(x)
                                            | _ -> failwith (sprintf "failed to evaluate [[%s]]"  (languageExpressionToString buffer))

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
                                            | WarewolfAtomListresult x -> WarewolfAtomListresult (x)
                                            | WarewolfAtomResult x -> WarewolfAtomResult(x)
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
                                            | WarewolfAtomListresult x -> WarewolfAtomListresult (x)
                                            | WarewolfAtomResult x -> WarewolfAtomResult(x)
                                            | _ -> failwith (sprintf "failed to evaluate [[%s]]"  (languageExpressionToString buffer))

and  reduceForCalculate  (env: WarewolfEnvironment) (update:int) (langs:string) : string=
    let lang = langs.Trim() 
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a  when update = 0 ->  a
                    | _->    
                        let temp = parseLanguageExpression lang update
                        temp
    match buffer with
        | ComplexExpression  a -> if (List.exists (isNotAtomAndNotcomplex a) a) 
                                  then 
                                        List.map languageExpressionToString a|> List.map  (eval env update false)  |> List.map evalResultToString |> fun a-> System.String.Join("",a) |> (fun a ->reduceForCalculate env update a  )
                                  else 
                                        lang
        | RecordSetExpression a -> match a.Index with 
                                    | IndexExpression exp -> match exp with
                                                                | WarewolfAtomExpression _ -> lang
                                                                |_->     sprintf "[[%s(%s).%s]]" a.Name (eval  env update false (languageExpressionToString exp)|> evalResultToString) a.Column  
                                    | _->lang
        | _ -> lang

and isNotAtomAndNotcomplex  (b:LanguageExpression list) (a:LanguageExpression) =
    let set = b|> List.map LanguageExpressionToStringWithoutStuff |> Set.ofList
    let reserved =  ["[[";"]]"] |> Set.ofList
    if not (Set.intersect set reserved|> Set.isEmpty)
        then 
            match a with
            | WarewolfAtomExpression _ -> false
            |_ -> true
        else
            false

and  evalForDataMerge  (env: WarewolfEnvironment) (update:int) (lang:string) : WarewolfEvalResult list=

    let EvalComplex (exp:LanguageExpression list) : WarewolfEvalResult list = 
        if((List.length exp) =1) then
            match exp.[0] with
                | RecordSetExpression _ ->  [eval env update false (languageExpressionToString exp.[0])]
                | ScalarExpression _ ->  [eval env update false (languageExpressionToString exp.[0])]
                | WarewolfAtomExpression a ->  [WarewolfEvalResult.WarewolfAtomResult a]
                | _ ->failwith "you should not get here"
        else
            List.map  (languageExpressionToString>>eval   env update false)  exp 
            

    
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a  when update = 0 ->  a
                    | _->    
                        let temp = parseLanguageExpression lang update
                        temp
    match buffer with
        | RecordSetExpression a -> [WarewolfAtomListresult(  (evalRecordsSet a env) )]
        | ScalarExpression a -> [WarewolfAtomResult (evalScalar a env)]
        | WarewolfAtomExpression a -> [WarewolfAtomResult a]
        | RecordSetNameExpression x ->[evalDataSetExpression env update x]
        | ComplexExpression  a ->  (EvalComplex ( List.filter (fun b -> "" <> (languageExpressionToString b)) a)) 
        | _ ->failwith "you should not get here"

and  evalToExpression  (env: WarewolfEnvironment) (update:int) (langs:string) : string=
    let lang = langs.Trim() 
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a  when update = 0 ->  a
                    | _->    
                        let temp = parseLanguageExpression lang update
                        temp
    match buffer with
        | ComplexExpression  a -> if (List.exists isNotAtom a) 
                                  then 
                                        let ev =List.map languageExpressionToString a|> List.map  (eval env update false)  |> List.map evalResultToString |> fun a-> System.String.Join("",a) |> (fun a ->evalToExpression env update a  )
                                        if ev.Contains("[[") then ev else languageExpressionToString buffer                                  
                                  else lang
        | RecordSetExpression a -> match a.Index with 
                                    | IndexExpression exp -> match exp with
                                                                | WarewolfAtomExpression _ -> lang
                                                                |_->     sprintf "[[%s(%s).%s]]" a.Name (eval  env update false (languageExpressionToString exp)|> evalResultToString) a.Column  
                                    | _->buffer |> languageExpressionToString
        | _ -> lang


and isNotAtom (a:LanguageExpression) =
    match a with
    | WarewolfAtomExpression _ -> false
    |_ -> true
and  evalWithPositions  (env: WarewolfEnvironment)  (update:int)  (lang:string) : WarewolfEvalResult=
    let EvalComplex (exp:LanguageExpression list) = 
        if((List.length exp) =1) then
            match exp.[0] with
                | RecordSetExpression a ->  evalRecordSetAsString env a
                | ScalarExpression a ->  (evalScalar a env)
                | WarewolfAtomExpression a ->  a
                | _ ->failwith "you should not get here"
        else    
            let start = List.map languageExpressionToString  exp |> (List.fold (+) "")
            let evaled = (List.map (languageExpressionToString >> (eval  env update false)>>evalResultToString)  exp )|> (List.fold (+) "")
            if( evaled = start) then
                DataString evaled
            else DataString (eval env update false evaled |>  evalResultToString)
    
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a  when update = 0 ->  a
                    | _->    
                        let temp = parseLanguageExpression lang update
                        temp
    match buffer with
        | RecordSetExpression a -> WarewolfAtomListresult(  (evalRecordsSetWithPositions a env) )
        | ScalarExpression a -> WarewolfAtomResult (evalScalar a env)
        | WarewolfAtomExpression a -> WarewolfAtomResult a
        | RecordSetNameExpression x ->evalDataSetExpression env update x
        | ComplexExpression  a -> WarewolfAtomResult (EvalComplex ( List.filter (fun b -> "" <> (languageExpressionToString b)) a)) 
        | _ ->failwith "you should not get here"

and getRecordSetPositionsAsInts (recset:WarewolfRecordset) =
    let AtomToInt (a:WarewolfAtom) =
        match a with
            | Int a -> a
            | _->failwith "the position column contains non ints"
    let positions = recset.Data.[PositionColumn];
    Seq.map AtomToInt positions |> Seq.sort

and evalRecordSetIndexes (env:WarewolfEnvironment) (recset:RecordSetName) : int seq =
    match recset.Index with
    | IntIndex _ -> {1..1}
    | Star -> getRecordSetPositionsAsInts env.RecordSets.[recset.Name]
    | Last -> getRecordSetPositionsAsInts env.RecordSets.[recset.Name]
    | IndexExpression _ -> {1..1}

and  evalIndexes  (env: WarewolfEnvironment) (update:int)  (lang:string) =
    let EvalComplex (exp:LanguageExpression list) = 
        if((List.length exp) =1) then
            match exp.[0] with
                | RecordSetNameExpression a -> evalRecordSetIndexes env a
                | _ ->failwith "not a recordset"
        else    
            failwith "not a recordset"
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a  when update = 0  ->  a
                    | _->    
                        let temp = parseLanguageExpression lang update
                        temp
    match buffer with
        | ComplexExpression  a ->  EvalComplex ( List.filter (fun b -> "" <> (languageExpressionToString b)) a)
        | RecordSetNameExpression a -> evalRecordSetIndexes env a
        | _ ->failwith "not a recordset"


and addToScalars (env:WarewolfEnvironment) (name:string) (value:WarewolfAtom)  =
    let rem = Map.remove name env.Scalar |> Map.add name value 
    {       Scalar=rem;
            RecordSets = env.RecordSets;
            JsonObjects = env.JsonObjects;
    }

and addToRecordSets (env:WarewolfEnvironment) (name:string) (value:WarewolfRecordset) =
    let rem = Map.remove name env.RecordSets |> Map.add name value 
    {       Scalar=env.Scalar;
            RecordSets = rem;
            JsonObjects = env.JsonObjects;
    }

and addColumnValueToRecordset (destination:WarewolfRecordset) (name:string) (values: WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>) =
     let data = destination.Data
     let columnData =  values
     let added = data.Add( name, columnData  )
     {
        Data = added
        Optimisations = destination.Optimisations;
        LastIndex= destination.LastIndex;
        Frame = 0;
     }

and createEmpty (length:int) (count:int) =
   new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord> (WarewolfAtomRecord.Nothing,seq {for _ in 1 .. length do yield Nothing },count);

and addToList (lst:WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>) (value:WarewolfAtomRecord) =
    lst.AddSomething value
    lst    

and addNothingToList (lst:WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>) =
    lst.AddNothing()
    lst    

and addAtomToRecordSet (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom) (position:int) =
    let col = rset.Data.TryFind columnName
    let rsAdded= match col with 
                    | Some _ -> rset
                    | None-> { rset with Data=  Map.add columnName (createEmpty rset.Data.[PositionColumn].Length rset.Data.[PositionColumn].Count)  rset.Data    }
    if position = rsAdded.Count+1    
        then
                  
            let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( addToList v value) else (addNothingToList v )) rsAdded.Data
            let len = addedAtEnd.[PositionColumn].Count 
            
            addedAtEnd.[PositionColumn].[len-1] <- Int position
            { rsAdded with Data=addedAtEnd ; LastIndex = rsAdded.LastIndex+1;  Optimisations = if rsAdded.Count = rsAdded.LastIndex &&  rsAdded.Optimisations <> WarewolfAttribute.Fragmented &&  rsAdded.Optimisations <> WarewolfAttribute.Sorted then WarewolfAttribute.Ordinal else rsAdded.Optimisations }
        else
            if position > rsAdded.Count+1
                then
                let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( addToList v value) else (addNothingToList v )) rsAdded.Data
                let len = addedAtEnd.[PositionColumn].Count 
            
                addedAtEnd.[PositionColumn].[len-1] <- Int position
                { rsAdded with Data=addedAtEnd ; LastIndex = position;Optimisations = if  rsAdded.Optimisations = WarewolfAttribute.Ordinal then WarewolfAttribute.Sorted else rsAdded.Optimisations }

            else
                let lstval = rsAdded.Data.[PositionColumn]
                if Seq.exists (fun vx -> vx=(Int position)) lstval then
                    let index = Seq.findIndex (fun vx -> vx= (Int position)) lstval
                    lstval.[index] <- value
                    rsAdded
                else 
                      let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( addToList v value) else (addNothingToList v )) rsAdded.Data
                      let len = addedAtEnd.[PositionColumn].Count 
            
                      addedAtEnd.[PositionColumn].[len-1] <- Int position
                      { rsAdded with Data=addedAtEnd ; LastIndex = rsAdded.LastIndex; Optimisations = WarewolfAttribute.Fragmented }

and getPositionFromRecset (rset:WarewolfRecordset) (columnName:string) =
    let isNothing (value) =
        match value with
        | Nothing -> true
        | _ -> false

    if rset.Data.ContainsKey( columnName) then
        if rset.Data.[columnName].Count = 0 then
            1
        else
            let posValue =  rset.Data.[columnName].[rset.Data.[columnName].Count-1]
            match posValue with
                |   Nothing ->
                        match rset.Frame with
                        | 0 ->
                            rset.Frame
                        | _ ->
                            let mutable lastIndex = rset.Frame
                            while lastIndex > 0 && (isNothing rset.Data.[columnName].[lastIndex - 1]) do
                                lastIndex <- lastIndex - 1
                            lastIndex + 1
                | _-> rset.LastIndex  + 1    
    else
        match rset.Frame with
        | 0 ->(rset.LastIndex)+1
        |_ ->max (rset.LastIndex) (rset.Frame)
        

and addAtomToRecordSetWithFraming (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom) (pos:int) (isFramed:bool) =
    let position = pos
    let rsAdded = if rset.Data.ContainsKey( columnName) 
                  then  rset
                  else 
                       { rset with Data=  Map.add columnName (createEmpty rset.Data.[PositionColumn].Length rset.Data.[PositionColumn].Count )  rset.Data    }
    let frame = if isFramed then position else 0;
    if (position = rsAdded.LastIndex+1)    
        then                  
            let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( addToList v value) else (addNothingToList v )) rsAdded.Data
            let len = addedAtEnd.[PositionColumn].Count 
            
            addedAtEnd.[PositionColumn].[len-1] <- Int position
            { rsAdded with Data=addedAtEnd ; LastIndex = rsAdded.LastIndex+1; Frame = frame ; Optimisations = if rsAdded.Count = rsAdded.LastIndex &&  rsAdded.Optimisations <> WarewolfAttribute.Fragmented &&  rsAdded.Optimisations <> WarewolfAttribute.Sorted then WarewolfAttribute.Ordinal else rsAdded.Optimisations  }
        else
            if (position = rsAdded.LastIndex+1) || (position = rsAdded.Frame && isFramed)   
            then                  
                let len = rsAdded.Data.[PositionColumn].Count 
                if len = 0 then
                    rsAdded.Data.[columnName].[0] <- value
                    rsAdded
                else
                    rsAdded.Data.[columnName].[len-1] <- value
                    rsAdded
            else
            if position > rsAdded.LastIndex
                then
                let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( addToList v value) else (addNothingToList v )) rsAdded.Data
                let len = addedAtEnd.[PositionColumn].Count 
            
                addedAtEnd.[PositionColumn].[len-1] <- Int position
                { rsAdded with Data=addedAtEnd ; LastIndex = position; Frame = frame ;  Optimisations = if  rsAdded.Optimisations = WarewolfAttribute.Ordinal then WarewolfAttribute.Sorted else rsAdded.Optimisations }

            else
                let lstval = rsAdded.Data.[PositionColumn]
                if rsAdded.Optimisations = WarewolfAttribute.Ordinal then 
                    rsAdded.Data.[columnName].[position-1] <- value
                    rsAdded    
                else
                    if Seq.exists (fun vx -> vx=(Int position)) lstval then
                        let index = Seq.findIndex (fun vx -> vx= (Int position)) lstval
                        rsAdded.Data.[columnName].[index] <- value
                        rsAdded
                    else 
                          let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( addToList v value) else (addNothingToList v )) rsAdded.Data
                          let len = addedAtEnd.[PositionColumn].Count 
            
                          addedAtEnd.[PositionColumn].[len-1] <- Int position
                          { rsAdded with Data=addedAtEnd ; LastIndex = rsAdded.LastIndex; Frame = frame ; Optimisations = WarewolfAttribute.Fragmented }



and addRecsetToEnv (name : string) (env : WarewolfEnvironment) = 
    if env.RecordSets.ContainsKey name then env
    else 
        let b = createDataSet ""
        let a = { env with RecordSets = (Map.add name b env.RecordSets) }
        a

and createFilled (count:int) (value: WarewolfAtom):WarewolfColumnData=
   new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord> (WarewolfAtomRecord.Nothing,seq {for _ in 1 .. count do yield value }) 

and updateColumnWithValue (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom)=
    if rset.Count = 0 then
   
        rset
    else 
        if rset.Data.ContainsKey( columnName) 
        then
            let x = rset.Data.[columnName];
            for i in [0..x.Count-1] do                         
                x.[i]<-value;    
            rset 
        else 
        {rset with Data=  Map.add columnName ( createFilled rset.Count value)  rset.Data    }

and addToRecordSetFramed (env : WarewolfEnvironment) (name : RecordSetColumnIdentifier) (value : WarewolfAtom) = 
    if (env.RecordSets.ContainsKey name.Name) then 
        let recordset = env.RecordSets.[name.Name]
        
        let recsetAdded = 
            match name.Index with
            | IntIndex a -> addAtomToRecordSetWithFraming recordset name.Column value a false
            | Star -> 
                if recordset.Count = 0 then addAtomToRecordSetWithFraming recordset name.Column value 1 false
                else updateColumnWithValue recordset name.Column value
            | Last -> 
                addAtomToRecordSetWithFraming recordset name.Column value (getPositionFromRecset recordset name.Column) true
            | IndexExpression a -> 
                addAtomToRecordSetWithFraming recordset name.Column value 
                    (evalIndex env 0 (languageExpressionToString a)) false
        
        let recsets = Map.remove name.Name env.RecordSets |> fun a -> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets }
    else 
        let envwithRecset = addRecsetToEnv name.Name env
        addToRecordSetFramed envwithRecset name value

and addToRecordSetFramedWithAtomList (env : WarewolfEnvironment) (name : RecordSetColumnIdentifier) (value : WarewolfAtom seq) (shouldUseLast : bool) (update : int) (assignValue : IAssignValue option) = 
    if (env.RecordSets.ContainsKey name.Name) then 
        let data = env.RecordSets.[name.Name]
        
        let recordset = 
            if data.Data.ContainsKey(name.Column) then data
            else 
                { data with Data = 
                                Map.add name.Column 
                                    (createEmpty data.Data.[PositionColumn].Length data.Data.[PositionColumn].Count) 
                                    data.Data }
        
        let recsetAdded = 
            match name.Index with
            | IntIndex a -> addAtomToRecordSetWithFraming recordset name.Column (Seq.last value) a false
            | Star -> 
                let mutable recsetmutated = recordset
                let mutable index = 1
                match shouldUseLast with
                | false -> 
                    for a in value do
                        recsetmutated <- addAtomToRecordSetWithFraming recsetmutated name.Column a index false
                        index <- index + 1
                    recsetmutated
                | true -> 
                    let col = recsetmutated.Data.[name.Column]
                    let valueToChange = Seq.last value
                    for a in [ 0..col.Count - 1 ] do
                        recsetmutated <- addAtomToRecordSetWithFraming recsetmutated name.Column valueToChange (a + 1) false
                        index <- index + 1
                    recsetmutated
            | Last -> 
                let mutable recsetmutated = recordset
                let mutable index = (getPositionFromRecset recordset name.Column)
                for a in value do  
                    recsetmutated<-addAtomToRecordSetWithFraming recordset name.Column a index true
                    index<-index+1
                recsetmutated   
            | IndexExpression _ -> failwith "Invalid assign from list"
        let recsets = Map.remove name.Name env.RecordSets |> fun a -> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets }
    else 
        let envwithRecset = addRecsetToEnv name.Name env
        addToRecordSetFramedWithAtomList envwithRecset name value shouldUseLast update assignValue

let addToJsonObjects (env : WarewolfEnvironment) (name : string) (value : JContainer) = 
    let jsonNameExp = parseLanguageExpressionWithoutUpdate name
    match jsonNameExp with
        | JsonIdentifierExpression a -> 
                                        match a with
                                            | IndexNestedNameExpression b-> let correctName = b.ObjectName
                                                                            let rem = Map.remove correctName env.JsonObjects |> Map.add correctName value
                                                                            { env with JsonObjects = rem }                                                                    
                                            | _ -> let correctName = languageExpressionToStringNoBrackets jsonNameExp
                                                   let rem = Map.remove correctName env.JsonObjects |> Map.add correctName value
                                                   { env with JsonObjects = rem }
        | _ -> let rem = Map.remove name env.JsonObjects |> Map.add name value
               { env with JsonObjects = rem }

let rec addOrReturnJsonObjects (env : WarewolfEnvironment) (name : string) (value : JContainer) = 
    match env.JsonObjects.TryFind name with
    | Some _ -> env
    | _ -> addToJsonObjects env name value

let atomtoJToken (x:WarewolfAtom )=
    match x with 
        | Float a -> new JValue(a) :> JToken
        | Int a -> new JValue(a) :> JToken
        | DataString a -> new JValue(a) :> JToken
        | JsonObject a -> a
        | Nothing -> null :> JToken
        | NullPlaceholder -> null :> JToken
        | PositionedValue (_,b) ->  new JValue(b.ToString()) :> JToken

let addAtomicPropertyToJson (obj : Newtonsoft.Json.Linq.JObject) (name : string) (value : WarewolfAtom) = 
    let props = obj.Properties()
    let theProp = Seq.tryFind (fun (a : JProperty) -> a.Name = name) props
    match theProp with
    | None -> 
        obj.Add(new JProperty(name, (atomtoJToken value))) |> ignore
        obj
    | Some a -> 
        a.Value <- (atomtoJToken value)
        obj

let addArrayPropertyToJson (obj : Newtonsoft.Json.Linq.JObject) (name : string) (value : WarewolfAtom seq) = 
    let props = obj.Properties()
    let valuesAsStrings = Seq.map atomtoJToken value
    let theProp = Seq.tryFind (fun (a : JProperty) -> a.Name = name) props
    match theProp with
    | None -> 
        obj.Add(new JProperty(name, new JArray(valuesAsStrings))) |> ignore
        obj
    | Some a -> 
        a.Value <- new JArray(valuesAsStrings)
        obj




let deleteValues (exp:string)  (env:WarewolfEnvironment) =
    let rset = env.RecordSets.TryFind exp
    match rset with 
    | Some x -> {x with Data = Map.map (fun _ _ -> new WarewolfAtomList<WarewolfAtom>(WarewolfAtom.Nothing)) x.Data ;LastIndex=0;  } 
    | None->failwith "recordset does not exist"

let deleteValue  (exp:string) (index:int)   (env:WarewolfEnvironment) =
    let rset = env.RecordSets.TryFind exp
    match rset with 
    | Some values -> let pos = Seq.find ( fun a-> atomtoString a = index.ToString())  values.Data.[PositionColumn]
                     let posAsInt = match pos with
                                    | Nothing -> failwith "index does not exist"
                                    | Int a -> a
                                    | _  -> failwith "index does not exist"
                     {values  with Data = Map.map (fun (_:string) (b:WarewolfAtomList<WarewolfAtom>) -> b.DeletePosition( posAsInt ) ) values.Data ;  LastIndex= values.LastIndex-1 } 
    | None->failwith "recordset does not exist"

let deleteIndex  (exp:string) (index:int)   (env:WarewolfEnvironment) =
    let rset = env.RecordSets.TryFind exp
    match rset with 
    | Some values -> let pos = Seq.findIndex ( fun a-> atomtoString a = index.ToString())  values.Data.[PositionColumn]
                     let posAsInt = pos   
                     let newData =  Map.map (fun (_:string) (b:WarewolfAtomList<WarewolfAtom>) -> b.DeletePosition( posAsInt ) ) values.Data                  
                     {  values  with Optimisations = (if values.Optimisations = Ordinal then Sorted else values.Optimisations) ;
                                              Data = newData  ;                        
                                              LastIndex= if index = values.LastIndex && newData.[PositionColumn].Count>0 then atomToInt (Seq.max  newData.[PositionColumn]) else values.LastIndex ; 
                     } 
    | None->failwith "recordset does not exist"
let getLastIndexFromRecordSet (exp:string)  (env:WarewolfEnvironment)  =
    let rset = env.RecordSets.TryFind exp
    match rset with 
    | Some values -> values.LastIndex                    
    | None->failwith "recordset does not exist"

let rec deleteExpressionIndex (b:RecordSetName) (ind: LanguageExpression) (update:int)  (env:WarewolfEnvironment)  =
    let data = languageExpressionToString ind |> (eval env update false) |> evalResultToString
    match ind with 
    | WarewolfAtomExpression atom ->
                match atom with
                | Int a -> deleteIndex  b.Name a env |> (fun upd-> {env with RecordSets = Map.map (fun ax bx -> if ax=b.Name then upd else bx ) env.RecordSets} )
                | _ -> failwith "recordsets must have an integer star or empty index"
    |_->evalDelete( (sprintf "[[%s(%s)]]" b.Name data)) update env 

and evalDelete (exp:string) (update:int)   (env:WarewolfEnvironment) =
    let left = parseLanguageExpression exp update
    match left with 
                |   RecordSetNameExpression b ->  match b.Index with
                                                                 | Star -> deleteValues  b.Name env |> (fun upd-> {env with RecordSets = Map.map (fun ax bx -> if ax=b.Name then upd else bx ) env.RecordSets} )
                                                                 | Last ->  deleteIndex  b.Name (getLastIndexFromRecordSet  b.Name env) env |> (fun upd-> {env with RecordSets = Map.map (fun ax bx -> if ax=b.Name then upd else bx ) env.RecordSets} )
                                                                 | IntIndex a -> deleteIndex  b.Name a env |> (fun upd-> {env with RecordSets = Map.map (fun ax bx -> if ax=b.Name then upd else bx ) env.RecordSets} )
                                                                 | IndexExpression exp ->  deleteExpressionIndex b exp update env
                                                               

                |_-> failwith "only recordsets can be deleted"

let getIndexes (name:string) (env:WarewolfEnvironment) = evalIndexes  env 0 name

let ApplyIndexes (data:WarewolfParserInterop.WarewolfAtomList<WarewolfAtom>) (indexes : int[]) (name:string) =
    let newdata = new WarewolfParserInterop.WarewolfAtomList<WarewolfAtom>(WarewolfAtom.Nothing)
    if name = PositionColumn then
        for x in [1..data.Count] do
            newdata.AddSomething ( Int x)
        newdata
    else        
        for x in indexes do
            newdata.AddSomething data.[x]
        newdata


let compare (left:WarewolfAtom) (right:WarewolfAtom) =
    match (left,right) with
    | (Nothing,Nothing) -> 0
    | ( Int a, Int b ) -> a.CompareTo(b)
    | (Float a, Float b ) -> a.CompareTo(b)
    | (a,b) -> (atomtoString a).CompareTo(atomtoString b)


let rec sortRecst (recset:WarewolfRecordset) (colName:string) (desc:bool)=
    let data = recset.Data.[colName]
    let positions = [0..recset.Data.[PositionColumn].Count-1]
    let interpolated = Seq.map2 (fun a b ->  a,  b) data positions
    let sorted = if not desc then Seq.sortBy (fun x ->  (fst x)  ) interpolated else Seq.sortBy (fun x ->  (fst x)   ) interpolated |> List.ofSeq |>List.rev |> Seq.ofList
    let indexes = Seq.map snd sorted  |> Array.ofSeq
    let data = Map.map (fun a b -> ApplyIndexes b indexes a) recset.Data
    {recset with Data = data; Optimisations =Ordinal ; LastIndex = positions.Length  }

and sortRecset (exp:string) (desc:bool) (update:int)  (env:WarewolfEnvironment) =
    let left = parseLanguageExpression exp update
    match left with 
                |   RecordSetExpression b ->  let recsetopt = env.RecordSets.TryFind b.Name
                                              match recsetopt with
                                              | None -> failwith "record set does not exist"  
                                              | Some a -> let sorted =  sortRecst a b.Column desc
                                                          let recset = Map.remove b.Name env.RecordSets  
                                                          let recsets =   Map.add  b.Name sorted recset        
                                                          {env with RecordSets = recsets }
                |_-> failwith "Only recordsets that contain recordset columns can be sorted"

let isNothing (a:WarewolfEvalResult) =
   match a with
   | WarewolfAtomResult a -> a=Nothing
   | _-> false