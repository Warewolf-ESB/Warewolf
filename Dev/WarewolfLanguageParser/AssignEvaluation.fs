module AssignEvaluation

open DataStorage
open EvaluationFunctions
open Dev2.Common.Interfaces
open LanguageAST
open Newtonsoft.Json.Linq
open CommonFunctions

let createDataSet (a : string) = 
    let col = new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing)
    { Data = [ (PositionColumn, col) ] |> Map.ofList
      Optimisations = Ordinal
      LastIndex = 0
      Frame = 0 }

let addRecsetToEnv (name : string) (env : WarewolfEnvironment) = 
    if env.RecordSets.ContainsKey name then env
    else 
        let b = createDataSet ""
        let a = { env with RecordSets = (Map.add name b env.RecordSets) }
        a

let addToScalars (env : WarewolfEnvironment) (name : string) (value : WarewolfAtom) = 
    let rem = Map.remove name env.Scalar |> Map.add name value
    { env with Scalar = rem }

let addToJsonObjects (env : WarewolfEnvironment) (name : string) (value : JContainer) = 
    let rem = Map.remove name env.JsonObjects |> Map.add name value
    { env with JsonObjects = rem }

let rec addOrReturnJsonObjects (env : WarewolfEnvironment) (name : string) (value : JContainer) = 
    match env.JsonObjects.TryFind name with
    | Some _ -> env
    | _ -> addToJsonObjects env name value

let atomtoString a = atomtoString a

let addAtomicPropertyToJson (obj : Newtonsoft.Json.Linq.JObject) (name : string) (value : WarewolfAtom) = 
    let props = obj.Properties()
    let theProp = Seq.tryFind (fun (a : JProperty) -> a.Name = name) props
    match theProp with
    | None -> 
        obj.Add(new JProperty(name, (atomtoString value))) |> ignore
        obj
    | Some a -> 
        a.Value <- new JValue(atomtoString value)
        obj

let addArrayPropertyToJson (obj : Newtonsoft.Json.Linq.JObject) (name : string) (value : WarewolfAtom seq) = 
    let props = obj.Properties()
    let valuesAsStrings = Seq.map atomtoString value
    let theProp = Seq.tryFind (fun (a : JProperty) -> a.Name = name) props
    match theProp with
    | None -> 
        obj.Add(new JProperty(name, new JArray(valuesAsStrings))) |> ignore
        obj
    | Some a -> 
        a.Value <- new JArray(valuesAsStrings)
        obj

let toJObject (obj : JContainer) = 
    match obj with
    | :? JObject as x -> x
    | _ -> failwith "expected jObject but got something else"

let toJOArray (obj : JToken) = 
    match obj with
    | :? JArray as x -> x
    | _ -> failwith "expected jObject but got something else"

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
                            | _->false                  
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
                                                        | Last -> AddToRecordSetFramedWithAtomList env b  x true update (Some value)
                                                        | _ ->      try
                                                                        AddToRecordSetFramed env b x.[0]                  
                                                                    with
                                                                        | :? Dev2.Common.Common.NullValueInVariableException as ex -> raise( new  Dev2.Common.Common.NullValueInVariableException("The expression result is  null", value.Value ) )    // added 0 here!
 
let addValueToJArray (arr : JArray) (ind : int) (value : JToken) = 
    let index = ind - 1
    if (ind > arr.Count) then 
        let x = arr.Count
        for _ in x..ind - 1 do
            arr.Add(null)
        arr.[index] <- value
        (arr.[index], arr)
    else arr.[index] <- value 
         (arr.[index], arr)

let addOrGetValueFromJArray (arr : JArray) (ind : int) (value : JToken) = 
    let index = ind - 1
    if (ind > arr.Count) then 
        let x = arr.Count
        for _ in x..ind - 1 do
            arr.Add(null)
        arr.[index] <- value
        arr.[index]
    else arr.[index]  

let indexToInt (a : Index) (arr : JArray) = 
    match a with
    | IntIndex a -> [ a ]
    | Last -> [ arr.Count + 1 ]
    | Star -> [ 1..arr.Count ]
    | _ -> failwith "invalid index"

let addPropertyToJsonNoValue (obj : Newtonsoft.Json.Linq.JObject) (name : string) = 
    let props = obj.Properties()
    let theProp = Seq.tryFind (fun (a : JProperty) -> a.Name = name) props
    match theProp with
    | None -> 
        let child = new JProperty(name, new JObject())
        obj.Add(child) |> ignore
        child.Value
    | Some a -> a.Value

let addJsonArrayPropertyToJsonWithValue (obj : Newtonsoft.Json.Linq.JObject) (name : string) (index : Index) 
    (value : JToken) = 
    let props = obj.Properties()
    let theProp = Seq.tryFind (fun (a : JProperty) -> a.Name = name) props
    match theProp with
    | None -> 
        let arr = new JArray()
        let indexes = (indexToInt index arr)
        let arr2 = List.map (fun a -> addValueToJArray arr a (value)) indexes
        let prop = new JProperty(name, snd arr2.Head)
        obj.Add(prop) |> ignore
        List.map fst arr2
    | Some a -> 
        let arr = a.Value :?> JArray
        let indexes = (indexToInt index arr)
        let arr2 = List.map (fun a -> addValueToJArray arr a (value)) indexes
        List.map fst arr2

let rec expressionToObject (obj : JToken) (exp : JsonIdentifierExpression) (res : WarewolfEvalResult) = 
    match exp with
    | Terminal -> obj
    | NameExpression a -> objectFromExpression exp res (obj :?> JContainer)
    | NestedNameExpression a -> 
        expressionToObject (addPropertyToJsonNoValue (obj :?> JObject) a.ObjectName) (a.Next) res
    | IndexNestedNameExpression a -> 
        match a.Next with
        | Terminal -> 
            let allProperties = 
                addJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName a.Index 
                    (new JValue(evalResultToString res))
            List.map (fun x -> expressionToObject (x) (a.Next) res) allProperties |> List.head
        | _ -> 
            let allProperties = 
                addJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName a.Index (new JObject() :> JToken)
            List.map (fun x -> expressionToObject (x) (a.Next) res) allProperties |> List.head

and objectFromExpression (exp : JsonIdentifierExpression) (res : WarewolfEvalResult) (obj : JContainer) = 
    match exp with
    | IndexNestedNameExpression b -> 
        let asJObj = toJOArray obj
        match b.Index with
        | IntIndex a -> 
            let objToFill = addOrGetValueFromJArray (obj:?> JArray) a (new JObject())
            let subObj = expressionToObject (objToFill) b.Next res
            addValueToJArray asJObj a objToFill |> ignore
        | Last -> 
            let objToFill = new JObject()
            let subObj = expressionToObject (objToFill) b.Next res
            addValueToJArray asJObj (asJObj.Count + 1) objToFill |> ignore
        | Star -> 
            for i in 1..asJObj.Count do
                let objToFill = asJObj.[i - 1]
                let subObj = expressionToObject (objToFill) b.Next res
                addValueToJArray asJObj i (objToFill :?> JObject) |> ignore
        | _ -> failwith "unspecified error"
        asJObj :> JToken
    | NameExpression a -> 
        let asJObj = toJObject obj
        let myValue = evalResultToString res |> DataString
        addAtomicPropertyToJson asJObj a.Name myValue |> ignore
        asJObj :> JToken
    | _ -> failwith "top level assign cannot be a nested expresssion"

and assignGivenAValue (env : WarewolfEnvironment) (res : WarewolfEvalResult) (exp : JsonIdentifierExpression) : WarewolfEnvironment = 
    match exp with
    | NestedNameExpression a -> 
        let addedenv = addOrReturnJsonObjects env a.ObjectName (new JObject())
        let obj = addedenv.JsonObjects.[a.ObjectName]
        expressionToObject obj a.Next res |> ignore
        addedenv
    | IndexNestedNameExpression b -> 
        let addedenv = addOrReturnJsonObjects env b.ObjectName (new JArray())
        let obj = addedenv.JsonObjects.[b.ObjectName]
        if b.Next = Terminal then 
            let arr = obj :?> JArray
            let indexes = indexToInt b.Index arr
            List.map (fun a -> addValueToJArray arr a (new JValue(evalResultToString res))) indexes |> ignore
        else objectFromExpression exp res obj |> ignore
        addedenv
    | _ -> failwith "top level assign cannot be a nested expresssion"

and languageExpressionToJsonIdentifier (a : LanguageExpression) : JsonIdentifierExpression = 
    match a with
    | ScalarExpression _ -> failwith "unspecified error"
    | ComplexExpression _ -> failwith "unspecified error"
    | WarewolfAtomExpression _ -> failwith "literal value on the left hand side of an assign"
    | RecordSetNameExpression x -> 
        let next = Terminal
        { ObjectName = x.Name
          Index = x.Index
          Next = next }
        |> IndexNestedNameExpression
    | RecordSetExpression x -> 
        let next = ({ Name = x.Column } : JsonIdentifier) |> NameExpression
        { ObjectName = x.Name
          Index = x.Index
          Next = next }
        |> IndexNestedNameExpression
    | JsonIdentifierExpression x -> x

and evalJsonAssign (value : IAssignValue) (update : int) (env : WarewolfEnvironment) = 
    let left = parseLanguageExpression value.Name update
    let jsonId = languageExpressionToJsonIdentifier left
    let right = eval env update value.Value
    assignGivenAValue env right jsonId

and evalAssign (exp : string) (value : string) (update : int) (env : WarewolfEnvironment) = 
    evalAssignWithFrame (new WarewolfParserInterop.AssignValue(exp, value)) update env

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
                addAtomToRecordSetWithFraming recordset name.Column value (getPositionFromRecset recordset name.Column) 
                    true
            | IndexExpression a -> 
                addAtomToRecordSetWithFraming recordset name.Column value 
                    (evalIndex env 0 (languageExpressionToString a)) false
        
        let recsets = Map.remove name.Name env.RecordSets |> fun a -> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets }
    else 
        let envwithRecset = addRecsetToEnv name.Name env
        addToRecordSetFramed envwithRecset name value

and addToRecordSetFramedWithAtomList (env : WarewolfEnvironment) (name : RecordSetColumnIdentifier) (value : WarewolfAtom seq) 
    (shouldUseLast : bool) (update : int) (assignValue : IAssignValue option) = 
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
                        recsetmutated <- addAtomToRecordSetWithFraming recsetmutated name.Column valueToChange (a + 1) 
                                             false
                        index <- index + 1
                    recsetmutated
            //!!!!!!!!!!!!!!!!!Note the calling method handles Star and everything els uses the first index. 
            // If this is incorrect then uncomment the lines below.
            | Last -> 
                let mutable recsetmutated = recordset
                let mutable index = recordset.LastIndex + 1
                for a in value do
                    recsetmutated <- addAtomToRecordSetWithFraming recordset name.Column a index false
                    index <- index + 1
                recsetmutated
            | IndexExpression _ -> failwith "Invalid assign from list" // logic below does not make sense. removed it. If there is a use case then add it back it. 
//                let res = eval env update (languageExpressionToString b) |> evalResultToString
//                match b, assignValue with
//                | WarewolfAtomExpression atom, _ -> 
//                    match atom with
//                    | Int a -> addAtomToRecordSetWithFraming recordset name.Column (Seq.last value) a false
//                    | _ -> failwith "Invalid index"
//                | _, Some av -> 
//                    let data : WarewolfEnvironment = 
//                        (evalAssignWithFrame 
//                             (new WarewolfParserInterop.AssignValue((sprintf " // added 0 here!
//                                                                               [[%s(%s).%s]]" name.Name res name.Column), 
//                                                                    av.Value)) update env)
//                    data.RecordSets.[name.Name]
//                | _, _ -> failwith "Invalid assign from list"
        
        let recsets = Map.remove name.Name env.RecordSets |> fun a -> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets }
    else 
        let envwithRecset = addRecsetToEnv name.Name env
        addToRecordSetFramedWithAtomList envwithRecset name value shouldUseLast update assignValue

and evalMultiAssignOp (env : WarewolfEnvironment) (update : int) (value : IAssignValue) = 
    let l = EvaluationFunctions.parseLanguageExpression value.Name update    
    let left = 
        match l with
        | ComplexExpression a -> 
            if List.exists (fun a -> 
                   match a with
                   | ScalarExpression _ -> true
                   | RecordSetExpression _ -> true
                   | _ -> false) a
            then l
            else LanguageExpression.WarewolfAtomExpression(languageExpressionToString l |> DataString)
        | _ -> l    
    let rightParse = 
        if value.Value = null then LanguageExpression.WarewolfAtomExpression Nothing
        else EvaluationFunctions.parseLanguageExpression value.Value update    
    let right = 
        if value.Value = null then WarewolfAtomResult Nothing
        else EvaluationFunctions.eval env update value.Value    
    let shouldUseLast = 
        match rightParse with
        | RecordSetExpression a -> 
            match a.Index with
            | IntIndex _ -> true
            | Star -> false
            | Last -> true
            | _ -> true
        | _ -> true
    
    match right with
    | WarewolfAtomResult x -> 
        match left with
        | ScalarExpression a -> addToScalars env a x
        | RecordSetExpression b -> addToRecordSetFramed env b x
        | RecordSetNameExpression c ->
                                    if env.RecordSets.ContainsKey(value.Name) then env
                                    else evalJsonAssign value  update env
        | JsonIdentifierExpression d -> evalJsonAssign value update env                                
        | WarewolfAtomExpression _ -> failwith (sprintf "invalid variable assigned to%s" value.Name)
        | _ -> 
            let expression = (evalToExpression env update value.Name)
            if System.String.IsNullOrEmpty(expression) || (expression) = "[[]]" || (expression) = value.Name then env
            else evalMultiAssignOp env update (new WarewolfParserInterop.AssignValue(expression, value.Value))
    | WarewolfAtomListresult x -> 
        match left with
        | ScalarExpression a -> addToScalars env a (Seq.last x)
        | RecordSetExpression b -> 
            match b.Index with
            | Star -> addToRecordSetFramedWithAtomList env b x shouldUseLast update (Some value)
            | _ -> 
                try 
                    addToRecordSetFramed env b x.[0]
                with :? Dev2.Common.Common.NullValueInVariableException as ex -> 
                    raise 
                        (new Dev2.Common.Common.NullValueInVariableException("The expression result is  null", 
                                                                             value.Value))
        | WarewolfAtomExpression _ -> failwith "invalid variable assigned to"
        | _ -> 
            let expression = (evalToExpression env update value.Name)
            if System.String.IsNullOrEmpty(expression) || (expression) = "[[]]" || (expression) = value.Name then env
            else evalMultiAssignOp env update (new WarewolfParserInterop.AssignValue(expression, value.Value))
    | _ -> failwith "assigning an entire recordset to a variable is not defined"

and addAtomToRecordSetWithFraming (rset : WarewolfRecordset) (columnName : string) (value : WarewolfAtom) (pos : int) 
    (isFramed : bool) = 
    let position = pos
    
    let rsAdded = 
        if rset.Data.ContainsKey(columnName) then rset
        else 
            { rset with Data = 
                            Map.add columnName 
                                (createEmpty rset.Data.[PositionColumn].Length rset.Data.[PositionColumn].Count) 
                                rset.Data }
    
    let frame = 
        if isFramed then position
        else 0
    
    if (position = rsAdded.LastIndex + 1) then 
        let addedAtEnd = 
            Map.map (fun k v -> 
                if k = columnName then (addToList v value)
                else (addNothingToList v)) rsAdded.Data
        
        let len = addedAtEnd.[PositionColumn].Count
        addedAtEnd.[PositionColumn].[len - 1] <- Int position
        { rsAdded with Data = addedAtEnd
                       LastIndex = rsAdded.LastIndex + 1
                       Frame = frame
                       Optimisations = 
                           if rsAdded.Count = rsAdded.LastIndex && rsAdded.Optimisations <> WarewolfAttribute.Fragmented 
                              && rsAdded.Optimisations <> WarewolfAttribute.Sorted then WarewolfAttribute.Ordinal
                           else rsAdded.Optimisations }
    else if (position = rsAdded.LastIndex + 1) || (position = rsAdded.Frame && isFramed) then 
        let len = rsAdded.Data.[PositionColumn].Count
        if len = 0 then 
            rsAdded.Data.[columnName].[0] <- value
            rsAdded
        else 
            rsAdded.Data.[columnName].[len - 1] <- value
            rsAdded
    else if position > rsAdded.LastIndex then 
        let addedAtEnd = 
            Map.map (fun k v -> 
                if k = columnName then (addToList v value)
                else (addNothingToList v)) rsAdded.Data
        
        let len = addedAtEnd.[PositionColumn].Count
        addedAtEnd.[PositionColumn].[len - 1] <- Int position
        { rsAdded with Data = addedAtEnd
                       LastIndex = position
                       Frame = frame
                       Optimisations = 
                           if rsAdded.Optimisations = WarewolfAttribute.Ordinal then WarewolfAttribute.Sorted
                           else rsAdded.Optimisations }
    else 
        let lstval = rsAdded.Data.[PositionColumn]
        if rsAdded.Optimisations = WarewolfAttribute.Ordinal then 
            rsAdded.Data.[columnName].[position - 1] <- value
            rsAdded
        else if Seq.exists (fun vx -> vx = (Int position)) lstval then 
            let index = Seq.findIndex (fun vx -> vx = (Int position)) lstval
            rsAdded.Data.[columnName].[index] <- value
            rsAdded
        else 
            let addedAtEnd = 
                Map.map (fun k v -> 
                    if k = columnName then (addToList v value)
                    else (addNothingToList v)) rsAdded.Data
            
            let len = addedAtEnd.[PositionColumn].Count
            addedAtEnd.[PositionColumn].[len - 1] <- Int position
            { rsAdded with Data = addedAtEnd
                           LastIndex = rsAdded.LastIndex
                           Frame = frame
                           Optimisations = WarewolfAttribute.Fragmented }

and evalMultiAssignList (env : WarewolfEnvironment) (value : WarewolfAtom seq) (exp : string) (update : int) 
    (shouldUseLast : bool) = 
    let left = EvaluationFunctions.parseLanguageExpression exp update
    match left with
    | RecordSetExpression b -> addToRecordSetFramedWithAtomList env b value shouldUseLast update None
    | ScalarExpression s -> 
        let value = 
            System.String.Join(",", Seq.map (fun a -> a.ToString()) value |> Array.ofSeq) |> WarewolfAtom.DataString
        addToScalars env s value
    | _ -> failwith "Only recsets and scalars can be assigned from a list"

and evalDataShape (exp : string) (update : int) (env : WarewolfEnvironment) = 
    let left = EvaluationFunctions.parseLanguageExpression exp update
    match left with
    | ScalarExpression a -> 
        match env.Scalar.TryFind a with
        | None -> addToScalars env a Nothing
        | Some _ -> addToScalars env a Nothing
    | RecordSetExpression name -> 
        match env.RecordSets.TryFind name.Name with
        | None -> 
            let envwithRecset = addRecsetToEnv name.Name env
            let data = envwithRecset.RecordSets.[name.Name]
            
            let recordset = 
                if data.Data.ContainsKey(name.Column) then data
                else 
                    { data with Data = 
                                    Map.add name.Column 
                                        (createEmpty data.Data.[PositionColumn].Length data.Data.[PositionColumn].Count) 
                                        data.Data }
            replaceDataset env recordset name.Name
        | Some data -> 
            let recordset = 
                if data.Data.ContainsKey(name.Column) then data
                else 
                    { data with Data = 
                                    Map.add name.Column 
                                        (createEmpty data.Data.[PositionColumn].Length data.Data.[PositionColumn].Count) 
                                        data.Data }
            replaceDataset env recordset name.Name
    | WarewolfAtomExpression _ -> env
    | _ -> failwith "input must be recordset or value"

and replaceDataset (env : WarewolfEnvironment) (data : WarewolfRecordset) (name : string) = 
    let recsets = Map.remove name env.RecordSets |> fun a -> Map.add name data a
    { env with RecordSets = recsets }

and evalMultiAssign (values : IAssignValue seq) (update : int) (env : WarewolfEnvironment) = 
    let env = Seq.fold (fun a b -> evalMultiAssignOp a update b) env values
    let recsets = Map.map (fun _ b -> { b with Frame = 0 }) env.RecordSets
    { env with RecordSets = recsets }

and updateColumnWithValue (rset : WarewolfRecordset) (columnName : string) (value : WarewolfAtom) = 
    if rset.Data.ContainsKey(columnName) then 
        let x = rset.Data.[columnName]
        for i in [ 0..x.Count - 1 ] do
            x.[i] <- value
        rset
    else { rset with Data = Map.add columnName (createFilled rset.Count value) rset.Data }

and evalAssignWithFrame (value : IAssignValue) (update : int) (env : WarewolfEnvironment) = 
    let envass = evalMultiAssignOp env update value
    let recsets = envass.RecordSets
    { envass with RecordSets = recsets }

let removeFraming (env : WarewolfEnvironment) = 
    let recsets = Map.map (fun _ b -> { b with Frame = 0 }) env.RecordSets
    { env with RecordSets = recsets }

