﻿module  WarewolfDataEvaluationCommon


open LanguageAST
//open LanguageEval
open Microsoft.FSharp.Text.Lexing
open DataASTMutable
open WarewolfParserInterop
open CommonFunctions

// this method will given a language string return an AST based on FSLex and FSYacc

let mutable ParseCache = Map.empty : Map<string,LanguageExpression>

let PositionColumn = "WarewolfPositionColumn"



let evalRecordSetStarIndex (recset:WarewolfRecordset) (identifier:RecordSetIdentifier)  =
    match recset.Optimisations with 
    |Ordinal -> recset.Data.[identifier.Column] 
    | Sorted -> recset.Data.[identifier.Column]
    | Fragmented-> Seq.zip  recset.Data.[PositionColumn] recset.Data.[identifier.Column] |> Seq.sort |>Seq.map snd |> fun a -> new WarewolfAtomList<WarewolfAtom>(WarewolfAtom.Nothing,a)

let evalRecordSetStarIndexWithPositions (recset:WarewolfRecordset) (identifier:RecordSetIdentifier)  =
    Seq.zip ( Seq.map atomToInt recset.Data.[PositionColumn]) recset.Data.[identifier.Column]  |> Seq.map (fun a -> PositionedValue a)

let evalRecordSetLastIndex (recset:WarewolfRecordset) (identifier:RecordSetIdentifier)  =
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

    let evalled = eval env update exp 
    match evalled with
    | WarewolfAtomResult a -> getIntFromAtom a 
    | WarewolfAtomListresult a -> getIntFromAtomList a 
    |_ ->failwith "invalid recordset index was a list"


and  languageExpressionToString  (x:LanguageExpression) =
    match x with
        | RecordSetExpression a -> sprintf "[[%s(%s).%s]]" a.Name (IndexToString a.Index ) a.Column  
        | ScalarExpression a -> sprintf "[[%s]]" a
        | WarewolfAtomAtomExpression a -> atomtoString a
        | JsonIdentifierExpression a -> jsonExpressionToString  a ""
        | ComplexExpression a -> List.fold (fun c d -> c + languageExpressionToString d ) "" a
        | RecordSetNameExpression a -> sprintf "[[%s(%s)]]" a.Name (IndexToString a.Index ) 
and jsonExpressionToString a acc =
    match a with 
        | NameExpression x -> if acc = "" then  x.Name else sprintf ".%s" x.Name
        | NestedNameExpression x -> let current = if acc = "" then  x.ObjectName  else sprintf "%s.%s" acc x.ObjectName
                                    jsonExpressionToString x.Next current
        | IndexNestedNameExpression x ->   let current = if acc = "" then  x.ObjectName  else sprintf "%s.%s(%s)" acc x.ObjectName (IndexToString x.Index ) 
                                           jsonExpressionToString x.Next current
        | Terminal -> acc



and LanguageExpressionToSumOfInt (x: LanguageExpression list) =
    let expressionToInt (current:int) (y:LanguageExpression) =
        match current with 
        | -1 -> -99
        | _ ->
                match y with
                | RecordSetExpression _ -> current
                | ScalarExpression _ -> current
                | RecordSetNameExpression _ -> current
                | ComplexExpression _ -> current
                | WarewolfAtomAtomExpression _ when languageExpressionToString y = "]]" -> current-1 
                | WarewolfAtomAtomExpression _ when languageExpressionToString y = "[[" -> current+1 
                | WarewolfAtomAtomExpression _ -> current 
                | JsonIdentifierExpression _ -> current
    let sum = List.fold expressionToInt 0 x
    sum

and evalRecordsSet (recset:RecordSetIdentifier) (env: WarewolfEnvironment)  =
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

and evalRecordsSetWithPositions (recset:RecordSetIdentifier) (env: WarewolfEnvironment)  =
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


and evalRecordSetAsString (env: WarewolfEnvironment) (a:RecordSetIdentifier) = 
    match a.Index with
    | IntIndex _ -> (evalRecordsSet a env).[0]
    |_ ->(evalRecordsSet a env) |> Seq.map (atomtoString ) |> (Seq.fold (+) "") |> DataString

and  Clean (buffer :LanguageExpression) =
    match buffer with
        | RecordSetExpression a -> RecordSetExpression a
        | ScalarExpression a -> ScalarExpression a
        | RecordSetNameExpression a -> RecordSetNameExpression a
        | WarewolfAtomAtomExpression a -> WarewolfAtomAtomExpression a
        | JsonIdentifierExpression a -> JsonIdentifierExpression a
        | ComplexExpression  a ->  (List.filter (fun b -> "" <> (languageExpressionToString b)) a) |> (fun a -> if (List.length a) =1 then Clean a.[0] else ComplexExpression a)

                    
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
    else WarewolfAtomAtomExpression (parseAtom lang)

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
                                   let res = eval env update (languageExpressionToString b) |> evalResultToString
                                   match b with 
                                        | WarewolfAtomAtomExpression atom ->
                                                    match atom with
                                                    | Int a ->  WarewolfRecordSetResult (evalARow (getRecordSetIndexAsInt recset a) recset )
                                                    | _ -> failwith "non int index found"
                                        | _ ->   eval env update ( sprintf "[[%s(%s)]]" name.Name res)
    else
        raise (new Dev2.Common.Common.NullValueInVariableException("Recordset not found",name.Name))
          
and  eval  (env: WarewolfEnvironment)  (update:int) (lang:string) : WarewolfEvalResult=

    if lang.StartsWith(Dev2.Common.GlobalConstants.CalculateTextConvertPrefix) then
        evalForCalculate env update lang
    else
       
        let EvalComplex (exp:LanguageExpression list) = 
            if((List.length exp) =1) then
                match exp.[0] with
                    | RecordSetExpression a ->  evalRecordSetAsString env a
                    | ScalarExpression a ->  (evalScalar a env)
                    | WarewolfAtomAtomExpression a ->  a
                    | _ ->failwith "you should not get here"
            else    
                let start = List.map languageExpressionToString  exp |> (List.fold (+) "")
                let evaled = (List.map (languageExpressionToString >> (eval  env update)>>evalResultToString)  exp )|> (List.fold (+) "")
                if( evaled = start || (not (evaled.Contains("[[")))) then
                    DataString evaled
                else DataString (eval env update evaled|>  evalResultToString)    
        let buffer =  parseLanguageExpression lang update                        
        match buffer with
            | RecordSetExpression a when  env.RecordSets.ContainsKey a.Name -> WarewolfAtomListresult(  (evalRecordsSet a env) )
            | ScalarExpression a  when  env.Scalar.ContainsKey a  -> WarewolfAtomResult (evalScalar a env)
            | WarewolfAtomAtomExpression a  -> WarewolfAtomResult a
            | RecordSetNameExpression a when  env.RecordSets.ContainsKey a.Name -> evalDataSetExpression env update a
            | ComplexExpression  a ->  WarewolfAtomResult (EvalComplex ( List.filter (fun b -> "" <> (languageExpressionToString b)) a)) 
            | _  -> evalJson env update buffer

and languageExpressionToJPath (lang:LanguageExpression) = 
    match lang with
            | RecordSetExpression a ->
                                        match a.Index with  
                                            | IntIndex i -> sprintf "[%i].%s"  (i-1)  a.Column
                                            | Star  -> "[*]."+a.Column 
                                            | Last  -> "[(@.length-1)]."+a.Column  
                                            | _ ->failwith  "not supported for JSON types"
            | ScalarExpression _ -> "" 
            | WarewolfAtomAtomExpression _  -> ""
            | RecordSetNameExpression a -> 
                                        match a.Index with  
                                            | IntIndex i ->  sprintf "[%i]." (i-1)
                                            | Star  -> "[*]." 
                                            | Last  -> "[(@.length-1)]."
                                            | _ ->failwith  "not supported for JSON types"
            | ComplexExpression  a ->  failwith  "not supported for JSON types"
            | JsonIdentifierExpression a ->  jsonIdentifierToJsonPathLevel1 a 

and jsonIdentifierToJsonPath (a:JsonIdentifierExpression) (accx:string)= 
    let acc = if accx=""  then "" else accx + "."
    match a with 
    | NameExpression x -> acc  + x.Name
    | NestedNameExpression x ->   (jsonIdentifierToJsonPath x.Next (acc  + x.ObjectName) )
    | IndexNestedNameExpression x ->    let index =  match x.Index with  
                                                        | IntIndex i -> sprintf "[%i]" (i-1)
                                                        | Star  -> "[*]" 
                                                        | Last  -> "[-1:]"
                                                        | _ ->failwith  "not supported for JSON types"
                                        (jsonIdentifierToJsonPath x.Next (acc + x.ObjectName + "." + index  ) )
    | Terminal -> accx


and jsonIdentifierToJsonPathLevel1 (a:JsonIdentifierExpression) = 
    
    match a with 
    | NameExpression x ->  x.Name
    | NestedNameExpression x ->   (jsonIdentifierToJsonPath x.Next "" )
    | IndexNestedNameExpression x ->   (jsonIdentifierToJsonPath x.Next "")
    | Terminal -> ""

and jsonIdentifierToName (a:JsonIdentifierExpression) = 
    match a with 
    | NameExpression x -> x.Name
    | NestedNameExpression x ->  x.ObjectName 
    | IndexNestedNameExpression x ->     x.ObjectName 
    | Terminal -> ""

and evalJson (env: WarewolfEnvironment)  (update:int) (lang:LanguageExpression) =
    let jPath = "$." + languageExpressionToJPath(lang)
    match lang with 
    | ScalarExpression a -> if env.JsonObjects.ContainsKey a then WarewolfAtomResult  ( DataString  (env.JsonObjects.[a].ToString()) )else failwith "non existent recordset"
    | RecordSetExpression a ->  if  env.JsonObjects.ContainsKey a.Name 
                                then 
                                    let jo =  env.JsonObjects.[a.Name]
                                    
                                    let data = jo.SelectTokens(jPath) |> Seq.map (fun a -> WarewolfAtomRecord.DataString (a.ToString() ) )  
                                    WarewolfAtomListresult ( new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord> (WarewolfAtomRecord.Nothing, data))
                                else failwith "non existent recordset"
    | RecordSetNameExpression a ->  if  env.JsonObjects.ContainsKey a.Name 
                                    then 
                                        let jo =  env.JsonObjects.[a.Name]
                                    
                                        let data = jo.SelectTokens(jPath) |> Seq.map (fun a -> WarewolfAtomRecord.DataString (a.ToString() ) )  
                                        WarewolfAtomListresult ( new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord> (WarewolfAtomRecord.Nothing, data))
                                    else failwith "non existent recordset"
    | JsonIdentifierExpression a ->  if  env.JsonObjects.ContainsKey (jsonIdentifierToName a)
                                        then 
                                            let jo =  env.JsonObjects.[(jsonIdentifierToName a)]
                                    
                                            let data = jo.SelectTokens(jPath) |> Seq.map (fun a -> WarewolfAtomRecord.DataString (a.ToString() ) )  
                                            WarewolfAtomListresult ( new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord> (WarewolfAtomRecord.Nothing, data))
                                        else failwith "non existent recordset"
    | ComplexExpression _ -> failwith "no current use case please contact the warewolf product owner "
    | WarewolfAtomAtomExpression _ ->  failwith "no current use case please contact the warewolf product owner "

and  evalForCalculate  (env: WarewolfEnvironment)  (update:int) (langs:string) : WarewolfEvalResult=
    let lang = reduceForCalculate env update langs

    let EvalComplex (exp:LanguageExpression list) = 
        if((List.length exp) =1) then
            match exp.[0] with
                | RecordSetExpression a ->  evalRecordSetAsString env a
                | ScalarExpression a ->  (evalScalar a env)
                | WarewolfAtomAtomExpression a ->  a
                | _ ->failwith "you should not get here"
        else    
            let start = List.map languageExpressionToString  exp |> (List.fold (+) "")
            let evaled = (List.map (languageExpressionToString >> (evalForCalculate   env update)>>evalResultToString)  exp )|> (List.fold (+) "")
            if( evaled = start || (not (evaled.Contains("[[")))) then
                DataString evaled
            else DataString (eval env update evaled |>  evalResultToString)
    
    let buffer =  parseLanguageExpression lang update
                        
    match buffer with
        | RecordSetExpression a ->  evalRecordsSet a env |> Seq.map enQuote |> (fun x-> new WarewolfAtomList<WarewolfAtom>(Nothing,x) )|> WarewolfAtomListresult 
        | ScalarExpression a -> WarewolfAtomResult (evalScalar a env|>enQuote)
        | WarewolfAtomAtomExpression a -> WarewolfAtomResult a
        | RecordSetNameExpression x ->evalDataSetExpression env update x
        | ComplexExpression  a ->  WarewolfAtomResult (EvalComplex ( List.filter (fun b -> "" <> (languageExpressionToString b)) a)) 
        | JsonIdentifierExpression _ -> failwith "no current use case please contact the warewolf product owner " 



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
                                        List.map languageExpressionToString a|> List.map  (eval env update)  |> List.map evalResultToString |> fun a-> System.String.Join("",a) |> (fun a ->reduceForCalculate env update a  )
                                  else 
                                        lang
        | RecordSetExpression a -> match a.Index with 
                                    | IndexExpression exp -> match exp with
                                                                | WarewolfAtomAtomExpression _ -> lang
                                                                |_->     sprintf "[[%s(%s).%s]]" a.Name (eval  env update  (languageExpressionToString exp)|> evalResultToString) a.Column  
                                    | _->lang
        | _ -> lang






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
                                        let ev =List.map languageExpressionToString a|> List.map  (eval env update)  |> List.map evalResultToString |> fun a-> System.String.Join("",a) |> (fun a ->evalToExpression env update a  )
                                        if ev.Contains("[[") then ev else languageExpressionToString buffer                                  
                                  else lang
        | RecordSetExpression a -> match a.Index with 
                                    | IndexExpression exp -> match exp with
                                                                | WarewolfAtomAtomExpression _ -> lang
                                                                |_->     sprintf "[[%s(%s).%s]]" a.Name (eval  env update  (languageExpressionToString exp)|> evalResultToString) a.Column  
                                    | _->buffer |> languageExpressionToString
        | _ -> lang



and  evalWithPositions  (env: WarewolfEnvironment)  (update:int)  (lang:string) : WarewolfEvalResult=
    let EvalComplex (exp:LanguageExpression list) = 
        if((List.length exp) =1) then
            match exp.[0] with
                | RecordSetExpression a ->  evalRecordSetAsString env a
                | ScalarExpression a ->  (evalScalar a env)
                | WarewolfAtomAtomExpression a ->  a
                | _ ->failwith "you should not get here"
        else    
            let start = List.map languageExpressionToString  exp |> (List.fold (+) "")
            let evaled = (List.map (languageExpressionToString >> (eval  env update)>>evalResultToString)  exp )|> (List.fold (+) "")
            if( evaled = start) then
                DataString evaled
            else DataString (eval env update evaled|>  evalResultToString)
    
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a  when update = 0 ->  a
                    | _->    
                        let temp = parseLanguageExpression lang update
                        temp
    match buffer with
        | RecordSetExpression a -> WarewolfAtomListresult(  (evalRecordsSetWithPositions a env) )
        | ScalarExpression a -> WarewolfAtomResult (evalScalar a env)
        | WarewolfAtomAtomExpression a -> WarewolfAtomResult a
        | RecordSetNameExpression x ->evalDataSetExpression env update x
        | ComplexExpression  a -> WarewolfAtomResult (EvalComplex ( List.filter (fun b -> "" <> (languageExpressionToString b)) a)) 




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


let addToScalars (env:WarewolfEnvironment) (name:string) (value:WarewolfAtom)  =
    let rem = Map.remove name env.Scalar |> Map.add name value 
    { env with     Scalar=rem;

    }

let addToRecordSets (env:WarewolfEnvironment) (name:string) (value:WarewolfRecordset) =
    let rem = Map.remove name env.RecordSets |> Map.add name value 
    { env with RecordSets = rem
    }

let addColumnValueToRecordset (destination:WarewolfRecordset) (name:string) (values: WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>) =
     let data = destination.Data
     let columnData =  values
     let added = data.Add( name, columnData  )
     {
        Data = added
        Optimisations = destination.Optimisations;
        LastIndex= destination.LastIndex;
        Frame = 0;
     }

let createEmpty (length:int) (count:int) =
   new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord> (WarewolfAtomRecord.Nothing,seq {for _ in 1 .. length do yield Nothing },count);

let addToList (lst:WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>) (value:WarewolfAtomRecord) =
    lst.AddSomething value
    lst    

let addNothingToList (lst:WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>) =
    lst.AddNothing()
    lst    



let addAtomToRecordSet (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom) (position:int) =
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


let getPositionFromRecset (rset:WarewolfRecordset) (columnName:string) =
    if rset.Data.ContainsKey( columnName) then
        if rset.Data.[columnName].Count = 0 then
            1
        else
            let posValue =  rset.Data.[columnName].[rset.Data.[columnName].Count-1]
            match posValue with
                |   Nothing -> rset.Frame
                | _-> rset.LastIndex  + 1    
    else
        match rset.Frame with
        | 0 ->(rset.LastIndex)+1
        |_ ->max (rset.LastIndex) (rset.Frame)
        

let addAtomToRecordSetWithFraming (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom) (pos:int) (isFramed:bool) =
    let  position = pos
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





let createFilled (count:int) (value: WarewolfAtom):WarewolfColumnData=
   new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord> (WarewolfAtomRecord.Nothing,seq {for _ in 1 .. count do yield value }) 


let updateColumnWithValue (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom)=
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


let getIndexes (name:string) (env:WarewolfEnvironment) = evalIndexes  env 0 name


