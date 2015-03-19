module WarewolfDataEvaluationCommon


open LanguageAST
//open LanguageEval
open Microsoft.FSharp.Text.Lexing
open DataASTMutable

// this method will given a language string return an AST based on FSLex and FSYacc

let mutable ParseCache = Map.empty : Map<string,LanguageExpression>

let PositionColumn = "WarewolfPositionColumn#"

type WarewolfEvalResult = 
    | WarewolfAtomResult of WarewolfAtom
    | WarewolfAtomListresult of System.Collections.Generic.List<WarewolfAtomRecord> 

let AtomtoString (x:WarewolfAtom )=
    match x with 
        | Float a -> a.ToString()
        | Int a -> a.ToString()
        | DataString a -> a
        | Nothing -> ""
let WarewolfAtomRecordtoString (x:WarewolfAtomRecord )=
    match x with 
        | Float a -> a.ToString()
        | Int a -> a.ToString()
        | DataString a -> a
        | Nothing -> ""

let EvalResultToString (a:WarewolfEvalResult) = 
    match a with
    | WarewolfAtomResult x -> AtomtoString x
    | WarewolfAtomListresult x -> Seq.map WarewolfAtomRecordtoString x |> (Seq.fold (+) "")





type PositionValue =
    | IndexFoundPosition of int
    | IndexDoesNotExist

let getRecordSetIndex (recset:WarewolfRecordset) (position:int) =
    match recset.Optimisations with
    | Ordinal -> IndexFoundPosition (position-1)
    | _->
            let indexes = recset.Data.[PositionColumn]
            let positionAsAtom = Int position
            try  Seq.findIndex (fun a->  a=positionAsAtom) indexes |> IndexFoundPosition
            with     
            | :? System.Collections.Generic.KeyNotFoundException as ex -> IndexDoesNotExist

let evalRecordSetIndex (recset:WarewolfRecordset) (identifier:RecordSetIdentifier) (position:int) =
    let index = getRecordSetIndex recset position
    match index with 
    | IndexFoundPosition a -> recset.Data.[identifier.Column].[a]
    | IndexDoesNotExist -> Nothing

let evalRecordSetStarIndex (recset:WarewolfRecordset) (identifier:RecordSetIdentifier)  =
    recset.Data.[identifier.Column]

let evalRecordSetLastIndex (recset:WarewolfRecordset) (identifier:RecordSetIdentifier)  =
    let data = Seq.max recset.Data.[PositionColumn] 
    let index = Seq.findIndex (fun a -> a=data) recset.Data.[PositionColumn] 
    recset.Data.[identifier.Column].[index]







    

let evalScalar (scalarName:ScalarIdentifier) (env:WarewolfEnvironment) =
    if env.Scalar.ContainsKey scalarName
    then     ( env.Scalar.[scalarName])
    else Nothing

let rec IndexToString (x:Index) =
    match x with 
        | IntIndex a -> a.ToString()
        | Star -> "*"
        | Last -> ""
        | IndexExpression a-> LanguageExpressionToString a

and EvalIndex  ( env:WarewolfEnvironment) (exp:string)=
    let getIntFromAtom (a:WarewolfAtom) =
        match a with
        | Int x -> x
        |_ ->failwith "invalid recordset index was not an int"
    let evalled = Eval env exp
    match evalled with
    | WarewolfAtomResult a -> getIntFromAtom a 
    |_ ->failwith "invalid recordset index was a list"



and  LanguageExpressionToString  (x:LanguageExpression) =
    match x with
        | RecordSetExpression a -> sprintf "[[%s(%s).%s]]" a.Name (IndexToString a.Index ) a.Column  
        | ScalarExpression a -> sprintf "[[%s]]" a
        | WarewolfAtomAtomExpression a -> AtomtoString a
        | ComplexExpression a -> List.fold (fun c d -> c + LanguageExpressionToString d ) "" a
        | RecordSetNameExpression a -> sprintf "[[%s(%s)]]" a.Name (IndexToString a.Index ) 

and evalRecordsSet (recset:RecordSetIdentifier) (env: WarewolfEnvironment)  =
    if  not (env.RecordSets.ContainsKey recset.Name)       then 
        new System.Collections.Generic.List<WarewolfAtom>([Nothing])
            
    else
            match recset.Index with
                | IntIndex a -> new System.Collections.Generic.List<WarewolfAtomRecord>([ evalRecordSetIndex env.RecordSets.[recset.Name] recset a])
                | Star ->  evalRecordSetStarIndex env.RecordSets.[recset.Name] recset
                | Last -> new System.Collections.Generic.List<WarewolfAtomRecord>([evalRecordSetLastIndex env.RecordSets.[recset.Name] recset])
                | IndexExpression a -> new System.Collections.Generic.List<WarewolfAtomRecord>([ evalRecordSetIndex env.RecordSets.[recset.Name] recset ( LanguageExpressionToString a|>(EvalIndex env)) ])
                | _ -> failwith "Unknown evaluation type"

and evalRecordSetAsString (env: WarewolfEnvironment) (a:RecordSetIdentifier) = 
    match a.Index with
    | IntIndex i -> (evalRecordsSet a env).[0]
    |_ ->(evalRecordsSet a env) |> Seq.map (AtomtoString ) |> (Seq.fold (+) "") |> DataString

and  Clean (buffer :LanguageExpression) =
    match buffer with
        | RecordSetExpression a -> RecordSetExpression a
        | ScalarExpression a -> ScalarExpression a
        | RecordSetNameExpression a -> RecordSetNameExpression a
        | WarewolfAtomAtomExpression a -> WarewolfAtomAtomExpression a
        | ComplexExpression  a ->  (List.filter (fun b -> "" <> (LanguageExpressionToString b)) a) |> (fun a -> if (List.length a) =1 then Clean a.[0] else ComplexExpression a)

and ParseLanguageExpression  (lang:string) : LanguageExpression=
    let exp = ParseCache.TryFind lang
    match exp with 
    | Some a ->  a
    | None -> 
                let lexbuf = LexBuffer<string>.FromString lang 
                let buffer = Parser.start Lexer.tokenstream lexbuf
                let res = buffer |> Clean
                ParseCache<-ParseCache.Add(lang,res)
                res

        
and  Eval  (env: WarewolfEnvironment) (lang:string) : WarewolfEvalResult=
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
        | RecordSetExpression a -> WarewolfAtomListresult(  (evalRecordsSet a env) )
        | ScalarExpression a -> WarewolfAtomResult (evalScalar a env)
        | WarewolfAtomAtomExpression a -> WarewolfAtomResult a
        | ComplexExpression  a -> WarewolfAtomResult (EvalComplex ( List.filter (fun b -> "" <> (LanguageExpressionToString b)) a)) 

and getRecordSetPositionsAsInts (recset:WarewolfRecordset) =
    let AtomToInt (a:WarewolfAtom) =
        match a with
            | Int a -> a
            | _->failwith "the position column contains non ints"
    let positions = recset.Data.[PositionColumn];
    Seq.map AtomToInt positions |> Seq.sort

and evalRecordSetIndexes (env:WarewolfEnvironment) (recset:RecordSetName) : int seq =
    match recset.Index with
    | IntIndex a -> {1..1}
    | Star -> getRecordSetPositionsAsInts env.RecordSets.[recset.Name]
    | Last -> getRecordSetPositionsAsInts env.RecordSets.[recset.Name]
    | IndexExpression a -> {1..1}

and  EvalIndexes  (env: WarewolfEnvironment) (lang:string) =
    let EvalComplex (exp:LanguageExpression list) = 
        if((List.length exp) =1) then
            match exp.[0] with
                | RecordSetNameExpression a -> evalRecordSetIndexes env a
                | _ ->failwith "not a recordset"
        else    
            failwith "not a recordset"
    let exp = ParseCache.TryFind lang
    let buffer =  match exp with 
                    | Some a ->  a
                    | _->    
                        let temp = ParseLanguageExpression lang
                        temp
    match buffer with
        | ComplexExpression  a ->  EvalComplex ( List.filter (fun b -> "" <> (LanguageExpressionToString b)) a)
        | RecordSetNameExpression a -> evalRecordSetIndexes env a
        | _ ->failwith "not a recordset"


// assigns happen here all tools will use these functions to do assigns. Must be wrapped in c# class

//let Eval (env:Environment) (Tools: Tool list)
let AddToScalars (env:WarewolfEnvironment) (name:string) (value:WarewolfAtom)  =
    let rem = Map.remove name env.Scalar |> Map.add name value 
    {       Scalar=rem;
            RecordSets = env.RecordSets
    }

let AddToRecordSets (env:WarewolfEnvironment) (name:string) (value:WarewolfRecordset) =
    let rem = Map.remove name env.RecordSets |> Map.add name value 
    {       Scalar=env.Scalar;
            RecordSets = rem
    }

let AddColumnValueToRecordset (destination:WarewolfRecordset) (name:string) (values: System.Collections.Generic.List<WarewolfAtomRecord>) =
     let data = destination.Data
     let columnData = WarewolfColumnData values
     let added = data.Add( name, columnData  )
     {
        Data = added
        Optimisations = destination.Optimisations;
        LastIndex= destination.LastIndex;
        Count= destination.Count; 
        Frame = 0;
     }

let CreateEmpty (count:int) =
   new System.Collections.Generic.List<WarewolfAtomRecord> (seq {for x in 1 .. count do yield Nothing });

let AddToList (lst:System.Collections.Generic.List<'t>) (value:'t) =
    lst.Add value
    lst    





let AddAtomToRecordSet (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom) (position:int) =
    let col = rset.Data.TryFind columnName
    let rsAdded= match col with 
                    | Some a -> rset
                    | None-> { rset with Data=  Map.add columnName (CreateEmpty rset.Count)  rset.Data    }
    if position = rsAdded.Count+1    
        then
                  
            let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( WarewolfColumnData (AddToList v value)) else (WarewolfColumnData(AddToList v Nothing))) rsAdded.Data
            let len = addedAtEnd.[PositionColumn].Count 
            
            addedAtEnd.[PositionColumn].[len-1] <- Int position
            { rsAdded with Data=addedAtEnd ; LastIndex = rsAdded.LastIndex+1; Count = rsAdded.Count+1 ; Optimisations = if rsAdded.Count = rsAdded.LastIndex &&  rsAdded.Optimisations <> WarewolfAttribute.Fragmented &&  rsAdded.Optimisations <> WarewolfAttribute.Sorted then WarewolfAttribute.Ordinal else rsAdded.Optimisations }
        else
            if position > rsAdded.Count+1
                then
                let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( WarewolfColumnData (AddToList v value)) else (WarewolfColumnData(AddToList v Nothing))) rsAdded.Data
                let len = addedAtEnd.[PositionColumn].Count 
            
                addedAtEnd.[PositionColumn].[len-1] <- Int position
                { rsAdded with Data=addedAtEnd ; LastIndex = position; Count = len+1 ; Optimisations = if  rsAdded.Optimisations = WarewolfAttribute.Ordinal then WarewolfAttribute.Sorted else rsAdded.Optimisations }

            else
                let lstval = rsAdded.Data.[PositionColumn]
                if Seq.exists (fun vx -> vx=(Int position)) lstval then
                    let index = Seq.findIndex (fun vx -> vx= (Int position)) lstval
                    lstval.[index] <- value
                    rsAdded
                else 
                      let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( WarewolfColumnData (AddToList v value)) else (WarewolfColumnData(AddToList v Nothing))) rsAdded.Data
                      let len = addedAtEnd.[PositionColumn].Count 
            
                      addedAtEnd.[PositionColumn].[len-1] <- Int position
                      { rsAdded with Data=addedAtEnd ; LastIndex = rsAdded.LastIndex; Count = len+1 ; Optimisations = WarewolfAttribute.Fragmented }


let getPositionFromRecset (rset:WarewolfRecordset) (columnName:string) =
    if rset.Data.ContainsKey( columnName) then
        let posValue =  rset.Data.[columnName].[rset.Data.[columnName].Count-1]
        match posValue with
            |   Nothing -> rset.Frame
            | _-> (rset.LastIndex+1)        
    else
        match rset.Frame with
        | 0 ->(rset.LastIndex+1)
        |_ ->max (rset.LastIndex) rset.Frame
        

let AddAtomToRecordSetWithFraming (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom) (position:int) (isFramed:bool) =
    let rsAdded = if rset.Data.ContainsKey( columnName) 
                  then  rset
                  else 
                       { rset with Data=  Map.add columnName (CreateEmpty rset.Data.[PositionColumn].Count)  rset.Data    }
    let frame = if isFramed then position else 0;
    if position = rsAdded.Count+1    
        then                  
            let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( WarewolfColumnData (AddToList v value)) else (WarewolfColumnData(AddToList v Nothing))) rsAdded.Data
            let len = addedAtEnd.[PositionColumn].Count 
            
            addedAtEnd.[PositionColumn].[len-1] <- Int position
            { rsAdded with Data=addedAtEnd ; LastIndex = rsAdded.LastIndex+1; Count = rsAdded.Count+1; Frame = frame ; Optimisations = if rsAdded.Count = rsAdded.LastIndex &&  rsAdded.Optimisations <> WarewolfAttribute.Fragmented &&  rsAdded.Optimisations <> WarewolfAttribute.Sorted then WarewolfAttribute.Ordinal else rsAdded.Optimisations  }
        else
            if position > rsAdded.Count+1
                then
                let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( WarewolfColumnData (AddToList v value)) else (WarewolfColumnData(AddToList v Nothing))) rsAdded.Data
                let len = addedAtEnd.[PositionColumn].Count 
            
                addedAtEnd.[PositionColumn].[len-1] <- Int position
                { rsAdded with Data=addedAtEnd ; LastIndex = position; Frame = frame ; Count = len+1 ; Optimisations = if  rsAdded.Optimisations = WarewolfAttribute.Ordinal then WarewolfAttribute.Sorted else rsAdded.Optimisations }

            else
                let lstval = rsAdded.Data.[PositionColumn]
                if Seq.exists (fun vx -> vx=(Int position)) lstval then
                    let index = Seq.findIndex (fun vx -> vx= (Int position)) lstval
                    rsAdded.Data.[columnName].[index] <- value
                    rsAdded
                else 
                      let addedAtEnd =  Map.map (fun k v -> if k=columnName then  ( WarewolfColumnData (AddToList v value)) else (WarewolfColumnData(AddToList v Nothing))) rsAdded.Data
                      let len = addedAtEnd.[PositionColumn].Count 
            
                      addedAtEnd.[PositionColumn].[len-1] <- Int position
                      { rsAdded with Data=addedAtEnd ; LastIndex = rsAdded.LastIndex; Count = len+1 ; Frame = frame ; Optimisations = WarewolfAttribute.Fragmented }





let CreateFilled (count:int) (value: WarewolfAtom):WarewolfColumnData=
   new System.Collections.Generic.List<WarewolfAtomRecord> (seq {for x in 1 .. count do yield value }) 


let UpdateColumnWithValue (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom)=
    if rset.Data.ContainsKey( columnName) 
    then
        let x = rset.Data.[columnName];
        for i in [0..x.Count-1] do                         
            x.[i]<-value;    
        rset 
    else 
    {rset with Data=  Map.add columnName ( CreateFilled rset.Count value)  rset.Data    }



//let AddAtomToRecordSet (destination:WarewolfEnvironment)(rsetName:string) (rset:WarewolfRecordset) (name:string) (value: WarewolfAtom) (position:int) =
//    let rsAdded = if rset.Data.ContainsKey( name) 
//                  then  rset
//                  else 
//                       { rset with Data=  Map.add name (CreateEmpty rset.Count)  rset.Data    }
//    // add the index
//    match position with                                  
//    rsAdded.Data.["wsIndex"].Add(Int position)        
//    let addedAtEnd =  Map.map (fun k v -> if k=name then (k ,(AddToList v value)) else (k ,(AddToList v Nothing) ) rsAdded.Data
//
//    
//    if position > rsAdded.LastIndex
//    then
//        let col =rsAdded.Data.[name]
//        let recset = Map.remove name rsAdded |> Map.add name col
//        AddToRecordSets destination rsetName recset 
//    else
//        let col = (value,position)::List.filter (fun a -> position <>  (snd a)) (fst rsAdded.[name])
//        let recset = Map.remove name rsAdded |> Map.add name (col, 1+snd rsAdded.[name])
//        AddToRecordSets destination rsetName recset 
//
//  let maxColumn (col:WarewolfColumnData) =
//    snd col
