module AssignEvaluation
open DataASTMutable
open WarewolfDataEvaluationCommon
open Dev2.Common.Interfaces
open LanguageAST
open Newtonsoft.Json.Linq
open CommonFunctions



let createDataSet (a:string) =
    let col = new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing)
    {
        Data = [(PositionColumn,col) ] |> Map.ofList
        Optimisations = Ordinal;
        LastIndex=0;

        Frame=0;
    }

let addRecsetToEnv (name:string) (env:WarewolfEnvironment) = 
    if env.RecordSets.ContainsKey name
    then
       env
    else
       let b = createDataSet ""
       let a = {env with RecordSets= (Map.add name b env.RecordSets);}
       a

let addToScalars (env:WarewolfEnvironment) (name:string) (value:WarewolfAtom)  =
    let rem = Map.remove name env.Scalar |> Map.add name value 
    {    env with   Scalar=rem;
    }

let addToJsonObjects (env:WarewolfEnvironment) (name:string) (value:JContainer)  =
    let rem = Map.remove name env.JsonObjects |> Map.add name value 
    {    env with   JsonObjects=rem;
    }

let rec addToRecordSet (env:WarewolfEnvironment) (name:RecordSetIdentifier) (update:int) (value:WarewolfAtom)  =
    if(env.RecordSets.ContainsKey name.Name)
    then
        let recordset = env.RecordSets.[name.Name]
        let recsetAdded = match name.Index with
                          | IntIndex a -> addAtomToRecordSet recordset name.Column value a
                          | Star -> updateColumnWithValue recordset name.Column value 
                          | Last -> addAtomToRecordSet recordset name.Column value (recordset.LastIndex+1)
                          | IndexExpression a -> addAtomToRecordSet recordset name.Column value (evalIndex env update ( languageExpressionToString a ))
        let recsets = Map.remove name.Name env.RecordSets |> fun a-> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets}
    else
        let envwithRecset = addRecsetToEnv name.Name env
        addToRecordSet envwithRecset name update value
    
and evalAssign (exp:string) (value:string) (update:int) (env:WarewolfEnvironment) =
    evalAssignWithFrame (new WarewolfParserInterop.AssignValue(exp,value)) update env
    

and addToRecordSetFramed (env:WarewolfEnvironment) (name:RecordSetIdentifier) (value:WarewolfAtom)  =
    if(env.RecordSets.ContainsKey name.Name)
    then
        let recordset = env.RecordSets.[name.Name]
        let recsetAdded = match name.Index with
                          | IntIndex a -> addAtomToRecordSetWithFraming recordset name.Column value a false
                          | Star ->  if recordset.Count =0 then
                                        addAtomToRecordSetWithFraming recordset name.Column value 1 false
                                     else
                                        updateColumnWithValue recordset name.Column value 
                          | Last -> addAtomToRecordSetWithFraming recordset name.Column value (getPositionFromRecset recordset  name.Column) true
                          | IndexExpression a -> addAtomToRecordSetWithFraming recordset name.Column value (evalIndex env 0 ( languageExpressionToString a )) false
        let recsets = Map.remove name.Name env.RecordSets |> fun a-> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets}
    else
        let envwithRecset = addRecsetToEnv name.Name env
        addToRecordSetFramed envwithRecset name value

and  addToRecordSetFramedWithAtomList (env:WarewolfEnvironment) (name:RecordSetIdentifier) (value:WarewolfAtom seq) (shouldUseLast:bool) (update:int)  (assignValue :   IAssignValue option  )  =
    
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
                                        | _, Some av  ->   let data = (evalAssignWithFrame  (new WarewolfParserInterop.AssignValue( (sprintf "[[%s(%s).%s]]" name.Name res name.Column), av.Value)) update env) :WarewolfEnvironment
                                                           data.RecordSets.[name.Name] 
                                        |_,_ ->  failwith "Invalid assign from list"
        let recsets = Map.remove name.Name env.RecordSets |> fun a-> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets}
    else
        let envwithRecset = addRecsetToEnv name.Name env
        addToRecordSetFramedWithAtomList envwithRecset name value shouldUseLast update assignValue

and evalMultiAssignOp  (env:WarewolfEnvironment) (update:int)  (value :IAssignValue ) =
    let l = WarewolfDataEvaluationCommon.parseLanguageExpression value.Name update
    let left = match l with 
                    |ComplexExpression a -> if List.exists (fun a -> match a with
                                                                            | ScalarExpression _ -> true
                                                                            | RecordSetExpression _ -> true
                                                                            | _->false) a then    l  else LanguageExpression.WarewolfAtomAtomExpression  (languageExpressionToString l|> DataString )                         
                    | _-> l
    let rightParse = if value.Value=null then LanguageExpression.WarewolfAtomAtomExpression Nothing
                     else WarewolfDataEvaluationCommon.parseLanguageExpression value.Value  update
    
    let right = if value.Value=null then WarewolfAtomResult Nothing
                else WarewolfDataEvaluationCommon.eval env update value.Value 
    let shouldUseLast =  match rightParse with
                            | RecordSetExpression a ->
                                        match a.Index with
                                            | IntIndex _-> true
                                            | Star -> false
                                            | Last -> true
                                            | _-> true
                            | _->true                  
    match right with 
                | WarewolfAtomResult x -> 
                            match left with 
                            |   ScalarExpression a -> addToScalars env a x
                            |   RecordSetExpression b -> addToRecordSetFramed env b x
                            |   WarewolfAtomAtomExpression _ -> failwith (sprintf "invalid variable assigned to%s" value.Name)
                            |   _ -> let expression = (evalToExpression env update value.Name)
                                     if System.String.IsNullOrEmpty(  expression) || ( expression) = "[[]]" || ( expression) = value.Name then
                                        env
                                     else
                                        evalMultiAssignOp env update (new WarewolfParserInterop.AssignValue(  expression , value.Value))
                | WarewolfAtomListresult x -> 
                        match left with 
                        |   ScalarExpression a -> addToScalars env a (Seq.last x)
                        |   RecordSetExpression b ->    match b.Index with 
                                                        | Star -> addToRecordSetFramedWithAtomList env b  x shouldUseLast update (Some value)
                                                        | _ ->      try
                                                                        addToRecordSetFramed env b x.[0]                  
                                                                    with
                                                                        | :? Dev2.Common.Common.NullValueInVariableException as ex -> raise( new  Dev2.Common.Common.NullValueInVariableException("The expression result is  null", value.Value ) )    // added 0 here!
 

                        |   WarewolfAtomAtomExpression _ ->  failwith "invalid variabe assigned to"
                        |    _ -> let expression = (evalToExpression env update value.Name)
                                  if System.String.IsNullOrEmpty(  expression) || ( expression) = "[[]]" || ( expression) = value.Name then
                                        env
                                  else
                                        evalMultiAssignOp env  update (new WarewolfParserInterop.AssignValue(  expression , value.Value))
                |   _ -> failwith "assigning an entire recordset to a variable is not defined"

//and evalAssignToJson (env:WarewolfEnvironment) (update:int)  (value :IAssignValue )   =
//    
//    let l = WarewolfDataEvaluationCommon.evalToExpression env update value.Name |> fun a -> (WarewolfDataEvaluationCommon.parseLanguageExpression a update)
//    let left = match l with 
//                   |ComplexExpression a -> failwith "Complex expression in nested json not supported"
//                   | _-> l
//  
//    let rightParse = if value.Value=null then LanguageExpression.WarewolfAtomAtomExpression Nothing
//                     else WarewolfDataEvaluationCommon.parseLanguageExpression value.Value  update
//    
//    let right = if value.Value=null then WarewolfAtomResult Nothing
//                else WarewolfDataEvaluationCommon.eval env update value.Value 
//    let shouldUseLast =  match rightParse with
//                            | RecordSetExpression a ->
//                                        match a.Index with
//                                            | IntIndex a-> true
//                                            | Star -> false
//                                            | Last -> true
//                                            | _-> true
//                            | _->true                  
//    match right with 
//                | WarewolfAtomResult x -> 
//                            match left with 
//                            |   ScalarExpression a -> let str = JObject.Parse( x.ToString())
//                                                      addToJsonObjects env a str
//                            |   RecordSetExpression b -> addToRecordSetFramed env b 
//                            |   WarewolfAtomAtomExpression _ -> failwith (sprintf "invalid variable assigned to%s" value.Name)
//                            |   _ -> let expression = (evalToExpression env update value.Name)
//                                     if System.String.IsNullOrEmpty(  expression) || ( expression) = "[[]]" || ( expression) = value.Name then
//                                        env
//                                     else
//                                        evalMultiAssignOp env update (new WarewolfParserInterop.AssignValue(  expression , value.Value))
//                | WarewolfAtomListresult x -> 
//                        match left with 
//                        |   ScalarExpression a -> addToScalars env a (Seq.last x)
//                        |   RecordSetExpression b ->    match b.Index with 
//                                                        | Star -> addToRecordSetFramedWithAtomList env b  x shouldUseLast update (Some value)
//                                                        | _ ->      try
//                                                                        addToRecordSetFramed env b x.[0]                  
//                                                                    with
//                                                                        | :? Dev2.Common.Common.NullValueInVariableException as ex -> raise( new  Dev2.Common.Common.NullValueInVariableException("The expression result is  null", value.Value ) )    // added 0 here!
// 
//
//                        |   WarewolfAtomAtomExpression _ ->  failwith "invalid variabe assigned to"
//                        |    _ -> let expression = (evalToExpression env update value.Name)
//                                  if System.String.IsNullOrEmpty(  expression) || ( expression) = "[[]]" || ( expression) = value.Name then
//                                        env
//                                  else
//                                        evalMultiAssignOp env  update (new WarewolfParserInterop.AssignValue(  expression , value.Value))
//                |   _ -> failwith "assigning an entire recordset to a variable is not defined"



and evalMultiAssignList (env:WarewolfEnvironment)  (value :WarewolfAtom seq ) (exp :string) (update:int) (shouldUseLast: bool)=
    let left = WarewolfDataEvaluationCommon.parseLanguageExpression exp update
    match left with 
        |   RecordSetExpression b -> addToRecordSetFramedWithAtomList env b  value shouldUseLast update None
        |   ScalarExpression s ->   let value = System.String.Join("," ,Seq.map (fun a-> a.ToString()) value |> Array.ofSeq) |> WarewolfAtom.DataString
                                    addToScalars env s value
        |    _ ->   failwith "Only recsets and scalars can be assigned from a list"



and evalDataShape (exp:string) (update:int) (env:WarewolfEnvironment) =
    let left = WarewolfDataEvaluationCommon.parseLanguageExpression exp update
    match left with 
    |   ScalarExpression a -> match env.Scalar.TryFind a with
                              | None -> addToScalars env a Nothing
                              | Some _ -> addToScalars env a Nothing
    |   RecordSetExpression name -> match env.RecordSets.TryFind name.Name with
                                      | None -> let envwithRecset = addRecsetToEnv name.Name env
                                                let data = envwithRecset.RecordSets.[name.Name]
                                                let recordset =  if data.Data.ContainsKey( name.Column) 
                                                                 then  data
                                                                 else 
                                                                    { data with Data=  Map.add name.Column (createEmpty data.Data.[PositionColumn].Length data.Data.[PositionColumn].Count )  data.Data    }
                                                replaceDataset env recordset name.Name
                                        
                                      | Some data -> let recordset =  if data.Data.ContainsKey( name.Column)  then  
                                                                         data
                                                                      else 
                                                                          { data with Data=  Map.add name.Column (createEmpty data.Data.[PositionColumn].Length data.Data.[PositionColumn].Count )  data.Data    }
                                                     replaceDataset env recordset name.Name

    |   WarewolfAtomAtomExpression _ -> env
    |   _ -> failwith "input must be recordset or value"

and replaceDataset (env:WarewolfEnvironment)  (data:WarewolfRecordset) (name:string)=
    let recsets = Map.remove name env.RecordSets |> fun a-> Map.add name data a
    { env with RecordSets = recsets} 
and evalMultiAssign (values :IAssignValue seq) (update:int) (env:WarewolfEnvironment) =
        let env = Seq.fold (fun a b->  evalMultiAssignOp a update b)  env  values
        let recsets = Map.map (fun _ b -> {b with Frame = 0 }) env.RecordSets
        {env with RecordSets = recsets}

and  updateColumnWithValue (rset:WarewolfRecordset) (columnName:string) (value: WarewolfAtom)=
        if rset.Data.ContainsKey( columnName) 
        then
            let x = rset.Data.[columnName];
            for i in [0..x.Count-1] do                         
                x.[i]<-value;    
            rset 
        else 
        {rset with Data=  Map.add columnName ( createFilled rset.Count value)  rset.Data    }



and evalAssignWithFrame (value :IAssignValue ) (update:int) (env:WarewolfEnvironment) =
        let envass = evalMultiAssignOp env update value
        let recsets = envass.RecordSets
        {envass with RecordSets = recsets}

let removeFraming  (env:WarewolfEnvironment) =
        let recsets = Map.map (fun _ b -> {b with Frame = 0 }) env.RecordSets
        {env with RecordSets = recsets}

let atomtoString a = atomtoString a

let addAtomicPropertyToJson (obj:Newtonsoft.Json.Linq.JObject) (name:string) (value:WarewolfAtom)  =
    let props = obj.Properties()
    let theProp = Seq.tryFind (fun (a:JProperty)-> a.Name=name) props
    match theProp with 
        | None -> obj.Add(new JProperty(name, (atomtoString value))) |> ignore    
                  obj
        | Some a -> a.Value <- new JValue( atomtoString value)
                    obj
let addArrayPropertyToJson (obj:Newtonsoft.Json.Linq.JObject) (name:string) (value:WarewolfAtom seq)  =
    let props = obj.Properties()
    let valuesAsStrings = Seq.map atomtoString value
    let theProp = Seq.tryFind (fun (a:JProperty)-> a.Name=name) props
    match theProp with 
                    | None -> obj.Add(new JProperty(name, new JArray(valuesAsStrings))) |> ignore    
                              obj
                    | Some a -> a.Value <-  new JArray(valuesAsStrings)     
                                obj
let toJObject (obj:JContainer) =
     match obj with
       | :? JObject as x -> x
       |_ -> failwith "expected jObject but got something else"

let objectFromExpression (exp:JsonIdentifierExpression)  (res : WarewolfEvalResult) (obj:JContainer) =
        match exp with                               
        | IndexNestedNameExpression b ->  failwith "top level assign cannot be a n"
        | NameExpression a -> let asJObj = toJObject obj
                              let myValue = evalResultToString res |> DataString
                              addAtomicPropertyToJson asJObj a.Name myValue |> ignore
                              asJObj

        | _ -> failwith "top level assign cannot be a n
        ested expresssion"


let assignGivenAValue (env:WarewolfEnvironment) (res : WarewolfEvalResult) (exp:JsonIdentifierExpression) : WarewolfEnvironment=
    match exp with 
        | NestedNameExpression a -> let addedenv = addToJsonObjects env a.ObjectName (new JObject())
                                    let obj = addedenv.JsonObjects.[a.ObjectName]
                                    objectFromExpression a.Next res obj |> ignore
                                    addedenv
        | IndexNestedNameExpression b -> let addedenv = addToJsonObjects env b.ObjectName (new JObject())
                                         addedenv
        | _ -> failwith "top level assign cannot be a nested expresssion"