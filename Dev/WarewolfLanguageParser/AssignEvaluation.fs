﻿module AssignEvaluation
open Microsoft.FSharp.Text.Lexing
open DataASTMutable
open WarewolfDataEvaluationCommon
open Dev2.Common.Interfaces
open LanguageAST
open Newtonsoft.Json.Linq




let CreateDataSet (a:string) =
    let col = new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing)
    {
        Data = [(PositionColumn,col) ] |> Map.ofList
        Optimisations = Ordinal;
        LastIndex=0;

        Frame=0;
    }

let AddRecsetToEnv (name:string) (env:WarewolfEnvironment) = 
    if env.RecordSets.ContainsKey name
    then
       env
    else
       let b = CreateDataSet ""
       let a = {env with RecordSets= (Map.add name b env.RecordSets);}
       a

let AddToScalars (env:WarewolfEnvironment) (name:string) (value:WarewolfAtom)  =
    let rem = Map.remove name env.Scalar |> Map.add name value 
    {    env with   Scalar=rem;
    }

let AddToJsonObjects (env:WarewolfEnvironment) (name:string) (value:JContainer)  =
    let rem = Map.remove name env.JsonObjects |> Map.add name value 
    {    env with   JsonObjects=rem;
    }

let rec AddToRecordSet (env:WarewolfEnvironment) (name:RecordSetIdentifier) (update:int) (value:WarewolfAtom)  =
    if(env.RecordSets.ContainsKey name.Name)
    then
        let recordset = env.RecordSets.[name.Name]
        let recsetAdded = match name.Index with
                          | IntIndex a -> addAtomToRecordSet recordset name.Column value a
                          | Star -> UpdateColumnWithValue recordset name.Column value 
                          | Last -> addAtomToRecordSet recordset name.Column value (recordset.LastIndex+1)
                          | IndexExpression a -> addAtomToRecordSet recordset name.Column value (evalIndex env update ( languageExpressionToString a ))
        let recsets = Map.remove name.Name env.RecordSets |> fun a-> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets}
    else
        let envwithRecset = AddRecsetToEnv name.Name env
        AddToRecordSet envwithRecset name update value
    
and EvalAssign (exp:string) (value:string) (update:int) (env:WarewolfEnvironment) =
    EvalAssignWithFrame (new WarewolfParserInterop.AssignValue(exp,value)) update env
    

and AddToRecordSetFramed (env:WarewolfEnvironment) (name:RecordSetIdentifier) (value:WarewolfAtom)  =
    if(env.RecordSets.ContainsKey name.Name)
    then
        let recordset = env.RecordSets.[name.Name]
        let recsetAdded = match name.Index with
                          | IntIndex a -> addAtomToRecordSetWithFraming recordset name.Column value a false
                          | Star ->  if recordset.Count =0 then
                                        addAtomToRecordSetWithFraming recordset name.Column value 1 false
                                     else
                                        UpdateColumnWithValue recordset name.Column value 
                          | Last -> addAtomToRecordSetWithFraming recordset name.Column value (getPositionFromRecset recordset  name.Column) true
                          | IndexExpression a -> addAtomToRecordSetWithFraming recordset name.Column value (evalIndex env 0 ( languageExpressionToString a )) false
        let recsets = Map.remove name.Name env.RecordSets |> fun a-> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets}
    else
        let envwithRecset = AddRecsetToEnv name.Name env
        AddToRecordSetFramed envwithRecset name value

and  AddToRecordSetFramedWithAtomList (env:WarewolfEnvironment) (name:RecordSetIdentifier) (value:WarewolfAtom seq) (shouldUseLast:bool) (update:int)  (assignValue :   IAssignValue option  )  =
    
    if(env.RecordSets.ContainsKey name.Name)
    then
        let data = env.RecordSets.[name.Name]
        let recordset =  if data.Data.ContainsKey( name.Column) 
                         then  data
                         else 
                            { data with Data=  Map.add name.Column (createEmpty data.Data.[PositionColumn].Length data.Data.[PositionColumn].Count )  data.Data    }
        let recsetAdded = match name.Index with
                          | IntIndex a -> addAtomToRecordSetWithFraming recordset name.Column (Seq.last value) a false
                          | Star -> 
                                    let countVals = Seq.length value 
                                    let mutable recsetmutated = recordset
                                    let mutable index = 1 
                                    match  shouldUseLast with  
                                           | false->    for a in value do  
                                                            recsetmutated<-addAtomToRecordSetWithFraming recsetmutated name.Column a index false  
                                                            index<-index+1
                                                        recsetmutated
                                           | true ->        
                                                       let col = recsetmutated.Data.[name.Column]
                                                       let valueToChange = Seq.last value
                                                       for a in [0..col.Count-1]  do  
                                                            recsetmutated<-addAtomToRecordSetWithFraming recsetmutated name.Column valueToChange (a+1) false  
                                                            index<-index+1
                                                       recsetmutated                                   
                                        
                          | Last -> 
                                    let countVals = Seq.length value 
                                    let mutable recsetmutated = recordset
                                    let mutable index = recordset.LastIndex+1   
                                    for a in value do  
                                        recsetmutated<-addAtomToRecordSetWithFraming recordset name.Column a index false  
                                        index<-index+1
                                    recsetmutated   
                          | IndexExpression b -> 
                                   let res = eval env update (languageExpressionToString b ) |> evalResultToString
                                   match b,assignValue with 
                                        | WarewolfAtomAtomExpression atom,_ ->
                                                    match atom with
                                                    | Int a ->  addAtomToRecordSetWithFraming recordset name.Column (Seq.last value) a false
                                                    | _ -> failwith "Invalid index"
                                        | _, Some av  ->   let data = (EvalAssignWithFrame  (new WarewolfParserInterop.AssignValue( (sprintf "[[%s(%s).%s]]" name.Name res name.Column), av.Value)) update env) :WarewolfEnvironment
                                                           data.RecordSets.[name.Name] 
                                        |_,_ ->  failwith "Invalid assign from list"
        let recsets = Map.remove name.Name env.RecordSets |> fun a-> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets}
    else
        let envwithRecset = AddRecsetToEnv name.Name env
        AddToRecordSetFramedWithAtomList envwithRecset name value shouldUseLast update assignValue

and EvalMultiAssignOp  (env:WarewolfEnvironment) (update:int)  (value :IAssignValue ) =
    let l = WarewolfDataEvaluationCommon.parseLanguageExpression value.Name update
    let left = match l with 
                    |ComplexExpression a -> if List.exists (fun a -> match a with
                                                                            | ScalarExpression a -> true
                                                                            | RecordSetExpression a -> true
                                                                            | _->false) a then    l  else LanguageExpression.WarewolfAtomAtomExpression  (languageExpressionToString l|> DataString )                         
                    | _-> l
    let rightParse = if value.Value=null then LanguageExpression.WarewolfAtomAtomExpression Nothing
                     else WarewolfDataEvaluationCommon.parseLanguageExpression value.Value  update
    
    let right = if value.Value=null then WarewolfAtomResult Nothing
                else WarewolfDataEvaluationCommon.eval env update value.Value 
    let shouldUseLast =  match rightParse with
                            | RecordSetExpression a ->
                                        match a.Index with
                                            | IntIndex a-> true
                                            | Star -> false
                                            | Last -> true
                                            | _-> true
                            | _->true                  
    match right with 
                | WarewolfAtomResult x -> 
                            match left with 
                            |   ScalarExpression a -> AddToScalars env a x
                            |   RecordSetExpression b -> AddToRecordSetFramed env b x
                            |   WarewolfAtomAtomExpression a -> failwith (sprintf "invalid variable assigned to%s" value.Name)
                            |   _ -> let expression = (evalToExpression env update value.Name)
                                     if System.String.IsNullOrEmpty(  expression) || ( expression) = "[[]]" || ( expression) = value.Name then
                                        env
                                     else
                                        EvalMultiAssignOp env update (new WarewolfParserInterop.AssignValue(  expression , value.Value))
                | WarewolfAtomListresult x -> 
                        match left with 
                        |   ScalarExpression a -> AddToScalars env a (Seq.last x)
                        |   RecordSetExpression b ->    match b.Index with 
                                                        | Star -> AddToRecordSetFramedWithAtomList env b  x shouldUseLast update (Some value)
                                                        | _ ->      try
                                                                        AddToRecordSetFramed env b x.[0]                  
                                                                    with
                                                                        | :? Dev2.Common.Common.NullValueInVariableException as ex -> raise( new  Dev2.Common.Common.NullValueInVariableException("The expression result is  null", value.Value ) )    // added 0 here!
 

                        |   WarewolfAtomAtomExpression _ ->  failwith "invalid variabe assigned to"
                        |    _ -> let expression = (evalToExpression env update value.Name)
                                  if System.String.IsNullOrEmpty(  expression) || ( expression) = "[[]]" || ( expression) = value.Name then
                                        env
                                  else
                                        EvalMultiAssignOp env  update (new WarewolfParserInterop.AssignValue(  expression , value.Value))
                |   _ -> failwith "assigning an entire recordset to a variable is not defined"

and EvalMultiAssignList (env:WarewolfEnvironment)  (value :WarewolfAtom seq ) (exp :string) (update:int) (shouldUseLast: bool)=
    let left = WarewolfDataEvaluationCommon.parseLanguageExpression exp update
    match left with 
        |   RecordSetExpression b -> AddToRecordSetFramedWithAtomList env b  value shouldUseLast update None
        |   ScalarExpression s ->   let value = System.String.Join("," ,Seq.map (fun a-> a.ToString()) value |> Array.ofSeq) |> WarewolfAtom.DataString
                                    AddToScalars env s value
        |    _ ->   failwith "Only recsets and scalars can be assigned from a list"


and EvalDataShape (exp:string) (update:int) (env:WarewolfEnvironment) =
    let left = WarewolfDataEvaluationCommon.parseLanguageExpression exp update
    match left with 
    |   ScalarExpression a -> match env.Scalar.TryFind a with
                              | None -> AddToScalars env a Nothing
                              | Some x -> AddToScalars env a Nothing
    |   RecordSetExpression name -> match env.RecordSets.TryFind name.Name with
                                      | None -> let envwithRecset = AddRecsetToEnv name.Name env
                                                let data = envwithRecset.RecordSets.[name.Name]
                                                let recordset =  if data.Data.ContainsKey( name.Column) 
                                                                 then  data
                                                                 else 
                                                                    { data with Data=  Map.add name.Column (createEmpty data.Data.[PositionColumn].Length data.Data.[PositionColumn].Count )  data.Data    }
                                                ReplaceDataset env recordset name.Name
                                        
                                      | Some data -> let recordset =  if data.Data.ContainsKey( name.Column)  then  
                                                                         data
                                                                      else 
                                                                          { data with Data=  Map.add name.Column (createEmpty data.Data.[PositionColumn].Length data.Data.[PositionColumn].Count )  data.Data    }
                                                     ReplaceDataset env recordset name.Name

    |   WarewolfAtomAtomExpression a -> env
    |   _ -> failwith "input must be recordset or value"

and ReplaceDataset (env:WarewolfEnvironment)  (data:WarewolfRecordset) (name:string)=
    let recsets = Map.remove name env.RecordSets |> fun a-> Map.add name data a
    { env with RecordSets = recsets} 
and EvalMultiAssign (values :IAssignValue seq) (update:int) (env:WarewolfEnvironment) =
        let env = Seq.fold (fun a b->  EvalMultiAssignOp a update b)  env  values
        let recsets = Map.map (fun a b -> {b with Frame = 0 }) env.RecordSets
        {env with RecordSets = recsets}

and  UpdateColumnWithValue (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom)=
        if rset.Data.ContainsKey( columnName) 
        then
            let x = rset.Data.[columnName];
            for i in [0..x.Count-1] do                         
                x.[i]<-value;    
            rset 
        else 
        {rset with Data=  Map.add columnName ( createFilled rset.Count value)  rset.Data    }



and EvalAssignWithFrame (value :IAssignValue ) (update:int) (env:WarewolfEnvironment) =
        let envass = EvalMultiAssignOp env update value
        let recsets = envass.RecordSets
        {envass with RecordSets = recsets}

let RemoveFraming  (env:WarewolfEnvironment) =
        let recsets = Map.map (fun a b -> {b with Frame = 0 }) env.RecordSets
        {env with RecordSets = recsets}

let AtomtoString a = WarewolfDataEvaluationCommon.atomtoString a;
