module AssignEvaluation

open DataStorage
open EvaluationFunctions
open Dev2.Common.Interfaces
open LanguageAST
open Newtonsoft.Json.Linq
open CommonFunctions
open WarewolfDataEvaluationCommon

let toJObject (obj : JContainer) = 
    match obj with
    | :? JObject as x -> x
    | _ -> failwith "expected jObject but got something else"

let toJOArray (obj : JToken) = 
    match obj with
    | :? JArray as x -> x
    | _ -> failwith "expected jObject but got something else"

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

let getOrAddValueToJArray (arr : JArray) (ind : int) (value : JToken) = 
    let index = ind - 1
    if (ind > arr.Count) then 
        let x = arr.Count
        for _ in x..ind - 1 do
            arr.Add(null)
        arr.[index] <- value
        (arr.[index], arr)
    else (arr.[index], arr)

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

let addJsonArrayPropertyToJsonWithValue (obj : Newtonsoft.Json.Linq.JObject) (name : string) (index : Index) (value : JToken) = 
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

let getObjectProperty (obj : Newtonsoft.Json.Linq.JObject) (name :string) =
    let props = obj.Properties()
    let theProp = Seq.tryFind (fun (a : JProperty) -> a.Name = name) props
    theProp

let getOrAddJsonArrayPropertyToJsonWithValue (obj : Newtonsoft.Json.Linq.JObject) (name : string) (index : Index) (value : JToken) = 
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
        let arr2 = List.map (fun a -> getOrAddValueToJArray arr a (value)) indexes
        List.map fst arr2

let getExpressionName (exp : JsonIdentifierExpression) =
    match exp with
    | NameExpression a ->
        a.Name
    | NestedNameExpression a ->
        a.ObjectName
    | IndexNestedNameExpression b ->
        b.ObjectName
    | _ -> null

let rec expressionToObject (obj : JToken) (exp : JsonIdentifierExpression) (res : WarewolfEvalResult) = 
    match exp with
    | Terminal -> obj
    | NameExpression a -> objectFromExpression exp res (obj :?> JContainer)
    | NestedNameExpression a -> expressionToObject (addPropertyToJsonNoValue (obj :?> JObject) a.ObjectName) (a.Next) res
    | IndexNestedNameExpression a -> 
        match a.Next with
        | Terminal -> 
            let allProperties = addJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName a.Index (new JValue(evalResultToString res))
            List.map (fun x -> expressionToObject (x) (a.Next) res) allProperties |> List.head
        | _ ->
            if (a.Index = Index.Last) then
                let arrTmp = getObjectProperty (obj :?> JObject) a.ObjectName
                match arrTmp with
                | Some someArr ->
                    let arr = someArr.Value :?> JArray
                    let lastOb = arr.Last
                    let prop = getObjectProperty (lastOb :?> JObject) (getExpressionName a.Next)
                    match prop with
                    | Some _ -> //if (props.contains(a.Next.Name)) then
                        let allProperties = getOrAddJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName a.Index (new JObject() :> JToken)
                        List.map (fun x -> expressionToObject (x) (a.Next) res) allProperties |> List.head
                    | None ->
                        let index = IntIndex arr.Count
                        let allProperties = getOrAddJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName index (new JObject() :> JToken)
                        List.map (fun x -> expressionToObject (x) (a.Next) res) allProperties |> List.head
                | _ ->
                    let allProperties = getOrAddJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName a.Index (new JObject() :> JToken)
                    List.map (fun x -> expressionToObject (x) (a.Next) res) allProperties |> List.head
            else
                let allProperties = getOrAddJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName a.Index (new JObject() :> JToken)
                List.map (fun x -> expressionToObject (x) (a.Next) res) allProperties |> List.head

and expressionToObjectForJson (obj : JToken) (exp : JsonIdentifierExpression) (res : WarewolfEvalResult) =
    match exp with
    | Terminal -> obj
    | NameExpression a -> objectFromExpressionForJson exp res (obj :?> JContainer)
    | NestedNameExpression a -> expressionToObjectForJson (addPropertyToJsonNoValue (obj :?> JObject) a.ObjectName) (a.Next) res
    | IndexNestedNameExpression a ->
        match a.Next with
        | Terminal ->
            let allProperties = addJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName a.Index (evalResultToJToken res)
            List.map (fun x -> expressionToObjectForJson (x) (a.Next) res) allProperties |> List.head
        | _ ->
            if (a.Index = Index.Last) then
                let arrTmp = getObjectProperty (obj :?> JObject) a.ObjectName
                match arrTmp with
                | Some someArr ->
                    let arr = someArr.Value :?> JArray
                    let lastOb = arr.Last
                    let prop = getObjectProperty (lastOb :?> JObject) (getExpressionName a.Next)
                    match prop with
                    | Some _ -> //if (props.contains(a.Next.Name)) then
                        let allProperties = getOrAddJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName a.Index (new JObject() :> JToken)
                        List.map (fun x -> expressionToObjectForJson (x) (a.Next) res) allProperties |> List.head
                    | None ->
                        let index = IntIndex arr.Count
                        let allProperties = getOrAddJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName index (new JObject() :> JToken)
                        List.map (fun x -> expressionToObjectForJson (x) (a.Next) res) allProperties |> List.head
                | _ ->
                    let allProperties = getOrAddJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName a.Index (new JObject() :> JToken)
                    List.map (fun x -> expressionToObjectForJson (x) (a.Next) res) allProperties |> List.head
            else
                let jobj = obj :?> JObject
                let props = jobj.Properties()
                let aa = a
                let value = (new JObject() :> JToken)
                let theProp = Seq.tryFind (fun (a : JProperty) -> a.Name = aa.ObjectName) props
                match theProp with
                | None -> 
                    let arr = new JArray()
                    let indexes = (indexToInt a.Index arr)
                    let arr2 = List.map (fun a -> addValueToJArray arr a (value)) indexes
                    let prop = new JProperty(aa.ObjectName, snd arr2.Head)
                    let allProperties = List.map fst arr2
                    let result = List.map (fun x -> expressionToObjectForJson (x) (a.Next) res) allProperties |> List.head
                    jobj.Add(prop) |> ignore
                    result
                | _ -> 
                    let allProperties = getOrAddJsonArrayPropertyToJsonWithValue (obj :?> JObject) a.ObjectName a.Index (new JObject() :> JToken)
                    List.map (fun x -> expressionToObjectForJson (x) (a.Next) res) allProperties |> List.head

and objectFromExpressionForJson (exp : JsonIdentifierExpression) (res : WarewolfEvalResult) (obj : JContainer) =
    match exp with
    | IndexNestedNameExpression b ->
        let asJObj = toJOArray obj
        match b.Index with
        | IntIndex a ->
            let objToFill = addOrGetValueFromJArray (obj:?> JArray) a (new JObject())
            let subObj = expressionToObjectForJson (objToFill) b.Next res
            addValueToJArray asJObj a objToFill |> ignore
        | Last ->
            let objToFill = new JObject()
            let subObj = expressionToObjectForJson (objToFill) b.Next res
            addValueToJArray asJObj (asJObj.Count + 1) objToFill |> ignore
        | Star ->
            for i in 1..asJObj.Count do
                let objToFill = asJObj.[i - 1]
                let subObj = expressionToObjectForJson (objToFill) b.Next res
                addValueToJArray asJObj i (objToFill :?> JObject) |> ignore
        | _ -> failwith "unspecified error"
        asJObj :> JToken
    | NameExpression a ->
        let asJObj = toJObject obj
        let myValue = JsonObject(evalResultToJToken res)
        addAtomicPropertyToJson asJObj a.Name myValue |> ignore
        asJObj :> JToken
    | _ -> failwith "top level assign cannot be a nested expresssion"

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
        let myValue = match res with
                        | WarewolfAtomResult atomResult ->  atomResult
                        | _ -> evalResultToString res |> DataString
        addAtomicPropertyToJson asJObj a.Name myValue |> ignore
        asJObj :> JToken
    | _ -> failwith "top level assign cannot be a nested expresssion"
(*

and assignGivenAValue (env : WarewolfEnvironment) (res : WarewolfEvalResult) (exp : JsonIdentifierExpression) : WarewolfEnvironment = 
    let evalResult = ((evalResultToString res).TrimEnd ' ').TrimStart ' '
    match exp with
    | NameExpression a -> 
        if (evalResult.StartsWith "{" && evalResult.EndsWith "}") then
            let actualValue = JContainer.Parse evalResult :?> JContainer
            let addedenv = addOrReturnJsonObjects env a.Name (new JObject())
            addToJsonObjects addedenv a.Name actualValue            
        else
            env
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
            let actualIndexes = 
                match b.Index with
                    | IntIndex a -> indexes
                    | Last -> indexes
                    | Star -> if indexes.Length = 0 then [1] else indexes
                    | _ -> failwith "invalid index"
            if (evalResult.StartsWith "{" && evalResult.EndsWith "}") || (evalResult.StartsWith "[{" && evalResult.EndsWith "}]") then
                let actualValue = JContainer.Parse evalResult
                List.map (fun a -> addValueToJArray arr a actualValue) actualIndexes |> ignore
            else
                let actualValue = new JValue(evalResultToString res)
                List.map (fun a -> addValueToJArray arr a actualValue) actualIndexes |> ignore
        else objectFromExpression exp res obj |> ignore
        addedenv
    | _ -> failwith "top level assign cannot be a nested expresssion"
*)

and assignGivenAValueForJson (env : WarewolfEnvironment) (res : WarewolfEvalResult) (exp : JsonIdentifierExpression) : WarewolfEnvironment = 
    let evalResult = ((evalResultToString res).TrimEnd ' ').TrimStart ' '

    match exp with
    | NameExpression a -> 
        if (isJsonString evalResult) then
            let actualValue = JContainer.Parse evalResult :?> JContainer
            let addedenv = addOrReturnJsonObjects env a.Name (new JObject())
            addToJsonObjects addedenv a.Name actualValue            
        else
            env
    | NestedNameExpression a ->
        let actualRes = match res with
                        | WarewolfEvalResult.WarewolfAtomResult atomResult -> match atomResult with
                                                                              | WarewolfAtom.DataString ds ->
                                                                                if (isJsonString evalResult) then
                                                                                    let actualValue = JContainer.Parse evalResult
                                                                                    WarewolfAtomResult(JsonObject(actualValue))
                                                                                else
                                                                                    res
                                                                              | _ -> res
                        | _ -> res
        let addedenv = addOrReturnJsonObjects env a.ObjectName (new JObject())
        let obj = addedenv.JsonObjects.[a.ObjectName]
        expressionToObjectForJson obj a.Next actualRes |> ignore
        addedenv
    | IndexNestedNameExpression b -> 
        let addedenv = addOrReturnJsonObjects env b.ObjectName (new JArray())
        let obj = addedenv.JsonObjects.[b.ObjectName]
        if b.Next = Terminal then 
            let arr = obj :?> JArray
            let indexes = indexToInt b.Index arr
            let actualIndexes = 
                match b.Index with
                    | IntIndex a -> indexes
                    | Last -> indexes
                    | Star -> if indexes.Length = 0 then [1] else indexes
                    | _ -> failwith "invalid index"
            if (isJsonString evalResult) || (evalResult.StartsWith "[{" && evalResult.EndsWith "}]") then
                let actualValue = JContainer.Parse evalResult
                List.map (fun a -> addValueToJArray arr a actualValue) actualIndexes |> ignore
            else
                let actualValue = new JValue(evalResultToString res)
                List.map (fun a -> addValueToJArray arr a actualValue) actualIndexes |> ignore
        else objectFromExpression exp res obj |> ignore
        addedenv
    | _ -> failwith "top level assign cannot be a nested expresssion"

and isJsonString (str : string) : bool =
    let isJsonOb = (str.StartsWith "{" && str.EndsWith "}")
    let isJsonArr = (str.StartsWith "[" && str.EndsWith "]" && str.[1] <> '[')
    isJsonOb || isJsonArr


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

and evalJsonAssign (value : IAssignValue) (update : int) (env : WarewolfEnvironment) (shouldTypeCast : ShouldTypeCast) = 
    let left = parseLanguageExpression value.Name update
    let jsonId = languageExpressionToJsonIdentifier left
    let right = 
        if (shouldTypeCast = ShouldTypeCast.No) then
            WarewolfAtomResult(DataString value.Value)
        else
            evalForJson env update false value.Value
    assignGivenAValueForJson env right jsonId

and evalAssign (exp : string) (value : string) (update : int) (env : WarewolfEnvironment) = 
    evalAssignWithFrame (new WarewolfParserInterop.AssignValue(exp, value)) update env

and evalMultiAssignOpStrict (env : WarewolfEnvironment) (update : int) (value : IAssignValue) (shouldTypeCast : ShouldTypeCast) = 
    let l = EvaluationFunctions.parseLanguageExpressionStrict value.Name update    
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
        else EvaluationFunctions.parseLanguageExpressionStrict value.Value update    


    let (right, hadException) = 
        try
            if value.Value = null then ((WarewolfAtomResult Nothing), null)
            //else eval env update false value.Value  
            else (WarewolfAtomResult(DataString value.Value), null)
        with
        | e -> (WarewolfAtomResult Nothing, e)

    let shouldUseLast = 
        match rightParse with
        | RecordSetExpression a -> 
            match a.Index with
            | IntIndex _ -> true
            | Star -> false
            | Last -> true
            | _ -> true
        | ComplexExpression a -> let exp = languageExpressionToString rightParse
                                 not (exp.Contains("(*)"))
        | _ -> true
    
    let result = match right with
                    | WarewolfAtomResult x -> 
                        match left with
                        | ScalarExpression a -> addToScalars env a x
                        | RecordSetExpression b -> addToRecordSetFramed env b x
                        | RecordSetNameExpression c ->
                                                    if env.RecordSets.ContainsKey(value.Name) then env
                                                    else evalJsonAssign value  update env shouldTypeCast
                        | JsonIdentifierExpression d -> failwith (sprintf "invalid variable assigned to %s" value.Name)
                        | WarewolfAtomExpression _ -> failwith (sprintf "invalid variable assigned to %s" value.Name)
                        | _ -> 
                            let expression = (evalToExpression env update value.Name)
                            if System.String.IsNullOrEmpty(expression) || (expression) = "[[]]" || (expression) = value.Name then env
                            else evalMultiAssignOp env update (new WarewolfParserInterop.AssignValue(expression, value.Value)) shouldTypeCast
                    | WarewolfAtomListresult x -> 
                        match left with
                        | ScalarExpression a -> addToScalars env a (Seq.last x)
                        | RecordSetExpression b -> 
                            match b.Index with
                            | Star -> addToRecordSetFramedWithAtomList env b x shouldUseLast update (Some value)
                            | Last -> addToRecordSetFramedWithAtomList env b x true update (Some value)
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
                            else evalMultiAssignOp env update (new WarewolfParserInterop.AssignValue(expression, value.Value)) shouldTypeCast
                    | _ -> failwith "assigning an entire recordset to a variable is not defined"

    match hadException with
    | null -> result
    | _ -> raise hadException


and evalMultiAssignOp (env : WarewolfEnvironment) (update : int) (value : IAssignValue) (shouldTypeCast : ShouldTypeCast) = 
    let l = EvaluationFunctions.parseLanguageExpression value.Name update ShouldTypeCast.Yes
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
        else EvaluationFunctions.parseLanguageExpression value.Value update shouldTypeCast
    let (right, excep) = try
                            if value.Value = null then (WarewolfAtomResult Nothing, null)
                            else
                                if (shouldTypeCast = ShouldTypeCast.Yes) then
                                    ((eval env update false value.Value), null)
                                else
                                    (WarewolfAtomResult(DataString value.Value), null)
                         with
                            | e -> (WarewolfAtomResult NullPlaceholder, e)
    let shouldUseLast = 
        match rightParse with
        | RecordSetExpression a -> 
            match a.Index with
            | IntIndex _ -> true
            | Star -> false
            | Last -> true
            | _ -> true
        | ComplexExpression a -> let exp = languageExpressionToString rightParse
                                 not (exp.Contains("(*)"))
        | _ -> true
    
    let result = match right with
                    | WarewolfAtomResult x -> 
                        match left with
                        | ScalarExpression a -> addToScalars env a x
                        | RecordSetExpression b -> addToRecordSetFramed env b x
                        | RecordSetNameExpression c ->
                                                    if env.RecordSets.ContainsKey(value.Name) then env
                                                    else evalJsonAssign value  update env shouldTypeCast
                        | JsonIdentifierExpression d -> evalJsonAssign (new WarewolfParserInterop.AssignValue(value.Name, evalResultToString right)) update env shouldTypeCast
                        | WarewolfAtomExpression _ -> failwith (sprintf "invalid variable assigned to %s" value.Name)
                        | _ -> 
                            let expression = (evalToExpression env update value.Name)
                            if System.String.IsNullOrEmpty(expression) || (expression) = "[[]]" || (expression) = value.Name then env
                            else evalMultiAssignOp env update (new WarewolfParserInterop.AssignValue(expression, value.Value)) shouldTypeCast
                    | WarewolfAtomListresult x -> 
                        match left with
                        | ScalarExpression a -> addToScalars env a (Seq.last x)
                        | RecordSetExpression b -> 
                            match b.Index with
                            | Star -> addToRecordSetFramedWithAtomList env b x shouldUseLast update (Some value)
                            | Last -> addToRecordSetFramedWithAtomList env b x true update (Some value)
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
                            else evalMultiAssignOp env update (new WarewolfParserInterop.AssignValue(expression, value.Value)) shouldTypeCast
                    | _ -> failwith "assigning an entire recordset to a variable is not defined"

    match excep with
    | null -> result
    | _ -> raise excep

and addAtomToRecordSetWithFraming (rset : WarewolfRecordset) (columnName : string) (value : WarewolfAtom) (pos : int) (isFramed : bool) = 
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
    let left = EvaluationFunctions.parseLanguageExpression exp update ShouldTypeCast.Yes
    match left with
    | RecordSetExpression b -> addToRecordSetFramedWithAtomList env b value shouldUseLast update None
    | ScalarExpression s -> 
        let value = 
            System.String.Join(",", Seq.map (fun a -> a.ToString()) value |> Array.ofSeq) |> WarewolfAtom.DataString
        addToScalars env s value
    | _ -> failwith "Only recsets and scalars can be assigned from a list"

and evalDataShape (exp : string) (update : int) (env : WarewolfEnvironment) = 
    let left = EvaluationFunctions.parseLanguageExpression exp update ShouldTypeCast.Yes
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
    let env = Seq.fold (fun a b -> evalMultiAssignOp a update b ShouldTypeCast.Yes) env values
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
    let envass = evalMultiAssignOp env update value ShouldTypeCast.Yes
    let recsets = envass.RecordSets
    { envass with RecordSets = recsets }
and evalAssignWithFrameTypeCast (value : IAssignValue) (update : int) (env : WarewolfEnvironment) (shouldTypeCast : ShouldTypeCast) = 
    let envass = evalMultiAssignOp env update value shouldTypeCast
    let recsets = envass.RecordSets
    { envass with RecordSets = recsets }

and evalAssignWithFrameStrict (value : IAssignValue) (update : int) (env : WarewolfEnvironment) = 
    let envass = evalMultiAssignOpStrict env update value ShouldTypeCast.Yes
    let recsets = envass.RecordSets
    { envass with RecordSets = recsets }

let removeFraming (env : WarewolfEnvironment) = 
    let recsets = Map.map (fun _ b -> { b with Frame = 0 }) env.RecordSets
    { env with RecordSets = recsets }
