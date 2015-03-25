module PublicFunctions

open LanguageAST
//open LanguageEval
open Microsoft.FSharp.Text.Lexing
open DataASTMutable
open WarewolfDataEvaluationCommon
open Dev2.Common.Interfaces
open Where

let PositionColumn = "WarewolfPositionColumn"

let CreateDataSet (a:string) =
    let col = new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing)
    {
        Data = [(PositionColumn,col) ] |> Map.ofList
        Optimisations = Ordinal;
        LastIndex=0;
        Count=0;
        Frame=0;
    }

let CreateEnv (vals:string) = 
     {
       RecordSets =Map.empty;
       Scalar =Map.empty;
     }

let AddRecsetToEnv (name:string) (env:WarewolfEnvironment) = 
    if env.RecordSets.ContainsKey name
    then
       env
    else
       let b = CreateDataSet ""
       let a = {env with RecordSets= (Map.add name b env.RecordSets);}
       a
let EvalEnvExpression (exp:string) (env:WarewolfEnvironment) =
     WarewolfDataEvaluationCommon.Eval env exp

let EvalWithPositions (exp:string) (env:WarewolfEnvironment) = WarewolfDataEvaluationCommon.EvalWithPositions env exp

let innerConvert  (i:int) (a: WarewolfAtom) =
        match a with
        | PositionedValue (a,b) -> new Dev2.Common.RecordSetSearchPayload( a,AtomtoString b)
        | b->  new Dev2.Common.RecordSetSearchPayload( i,AtomtoString b)

let AtomListToSearchTo (atoms :WarewolfAtom seq) =
    
    Seq.mapi innerConvert atoms

let RecordsetToSearchTo (recordset:WarewolfRecordset) =
    let cols = recordset.Data
    let data = Seq.map AtomToInt cols.[PositionColumn] 
    let dataToWorkWith  = (Map.filter (fun a b-> a= PositionColumn) cols)|>Map.toSeq 
    Seq.map snd dataToWorkWith  |> Seq.map (Seq.zip data)  |> Seq.collect (fun a -> a) |> Seq.map (fun (a,b) -> innerConvert a b )

let EvalEnvExpressionWithPositions (exp:string) (env:WarewolfEnvironment) =
     let data = WarewolfDataEvaluationCommon.EvalWithPositions env exp
     match data with
     | WarewolfAtomListresult  a -> AtomListToSearchTo a
     | WarewolfRecordSetResult b -> RecordsetToSearchTo b
     | WarewolfAtomResult a -> Seq.ofList [new Dev2.Common.RecordSetSearchPayload( 1,AtomtoString a)]

let EvalRecordSetIndexes (exp:string) (env:WarewolfEnvironment) =
     WarewolfDataEvaluationCommon.Eval env exp

let AddToScalars (env:WarewolfEnvironment) (name:string) (value:WarewolfAtom)  =
    let rem = Map.remove name env.Scalar |> Map.add name value 
    {       Scalar=rem;
            RecordSets = env.RecordSets
    }

let rec AddToRecordSet (env:WarewolfEnvironment) (name:RecordSetIdentifier) (value:WarewolfAtom)  =
    if(env.RecordSets.ContainsKey name.Name)
    then
        let recordset = env.RecordSets.[name.Name]
        let recsetAdded = match name.Index with
                          | IntIndex a -> AddAtomToRecordSet recordset name.Column value a
                          | Star -> UpdateColumnWithValue recordset name.Column value 
                          | Last -> AddAtomToRecordSet recordset name.Column value (recordset.LastIndex+1)
                          | IndexExpression a -> AddAtomToRecordSet recordset name.Column value (EvalIndex env ( LanguageExpressionToString a ))
        let recsets = Map.remove name.Name env.RecordSets |> fun a-> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets}
    else
        let envwithRecset = AddRecsetToEnv name.Name env
        AddToRecordSet envwithRecset name value
    
let EvalAssign (exp:string) (value:string) (env:WarewolfEnvironment) =
    let left = WarewolfDataEvaluationCommon.ParseLanguageExpression exp 
    let right = WarewolfDataEvaluationCommon.Eval env value
    let x = match right with 
            | WarewolfAtomResult a -> a
            | WarewolfAtomListresult b -> failwith "recset"
    match left with 
    |   ScalarExpression a -> AddToScalars env a x
    |   RecordSetExpression b -> AddToRecordSet env b x
    |   AtomExpression a -> env
    |   _ -> failwith "input must be recordset or value"



                                
    |   _ -> failwith "input must be recordset or value"

let rec AddToRecordSetFramed (env:WarewolfEnvironment) (name:RecordSetIdentifier) (value:WarewolfAtom)  =
    if(env.RecordSets.ContainsKey name.Name)
    then
        let recordset = env.RecordSets.[name.Name]
        let recsetAdded = match name.Index with
                          | IntIndex a -> AddAtomToRecordSetWithFraming recordset name.Column value a false
                          | Star -> UpdateColumnWithValue recordset name.Column value 
                          | Last -> AddAtomToRecordSetWithFraming recordset name.Column value (getPositionFromRecset recordset  name.Column) true
                          | IndexExpression a -> AddAtomToRecordSetWithFraming recordset name.Column value (EvalIndex env ( LanguageExpressionToString a )) false
        let recsets = Map.remove name.Name env.RecordSets |> fun a-> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets}
    else
        let envwithRecset = AddRecsetToEnv name.Name env
        AddToRecordSetFramed envwithRecset name value

let rec AddToRecordSetFramedWithAtomList (env:WarewolfEnvironment) (name:RecordSetIdentifier) (value:WarewolfAtom list) (shouldUseLast:bool)  =

    if(env.RecordSets.ContainsKey name.Name)
    then
        let recordset = env.RecordSets.[name.Name]
        let recsetAdded = match name.Index with
                          | IntIndex a -> AddAtomToRecordSetWithFraming recordset name.Column (Seq.last value) a false
                          | Star -> 
                                    let countVals = List.length value 
                                    let mutable recsetmutated = recordset
                                    let mutable index = 1 
                                    match  shouldUseLast with  
                                           | false->    for a in value do  
                                                            recsetmutated<-AddAtomToRecordSetWithFraming recsetmutated name.Column a index false  
                                                            index<-index+1
                                                        recsetmutated
                                           | true ->   let col = recsetmutated.Data.[name.Column]
                                                       let valueToChange = Seq.last value
                                                       for a in [0..col.Count-1]  do  
                                                            recsetmutated<-AddAtomToRecordSetWithFraming recsetmutated name.Column valueToChange (a+1) false  
                                                            index<-index+1
                                                       recsetmutated                                   
                                        
                          | Last -> 
                                    let countVals = List.length value 
                                    let mutable recsetmutated = recordset
                                    let mutable index = recordset.LastIndex+1   
                                    for a in value do  
                                        recsetmutated<-AddAtomToRecordSetWithFraming recordset name.Column a index false  
                                        index<-index+1
                                    recsetmutated   
                          |_-> failwith "unlucky "
        let recsets = Map.remove name.Name env.RecordSets |> fun a-> Map.add name.Name recsetAdded a
        { env with RecordSets = recsets}
    else
        let envwithRecset = AddRecsetToEnv name.Name env
        AddToRecordSetFramedWithAtomList envwithRecset name value shouldUseLast

let EvalMultiAssignOp  (env:WarewolfEnvironment)  (value :IAssignValue ) =
    let left = WarewolfDataEvaluationCommon.ParseLanguageExpression value.Name 
    let rightParse = WarewolfDataEvaluationCommon.ParseLanguageExpression value.Value 
   
    let right = WarewolfDataEvaluationCommon.Eval env value.Value
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
                        |   AtomExpression a -> env
                        |   _ -> failwith "input must be recordset or value"
            | WarewolfAtomListresult x -> 
                    match left with 
                    |   ScalarExpression a -> AddToScalars env a (Seq.last x)
                    |   RecordSetExpression b -> AddToRecordSetFramedWithAtomList env b (List.ofSeq x) shouldUseLast
                    |   AtomExpression a -> env
                    |   _ -> failwith "input must be recordset or value"


let EvalMultiAssign (values :IAssignValue seq) (env:WarewolfEnvironment) =
        let env = Seq.fold EvalMultiAssignOp env values
        let recsets = Map.map (fun a b -> {b with Frame = 0 }) env.RecordSets
        {env with RecordSets = recsets}

let EvalAssignWithFrame (value :IAssignValue ) (env:WarewolfEnvironment) =
        let envass = EvalMultiAssignOp env value
        let recsets = envass.RecordSets
        {envass with RecordSets = recsets}

let RemoveFraming  (env:WarewolfEnvironment) =
        let recsets = Map.map (fun a b -> {b with Frame = 0 }) env.RecordSets
        {env with RecordSets = recsets}

let AtomtoString a = WarewolfDataEvaluationCommon.AtomtoString a;


let getIndexes (name:string) (env:WarewolfEnvironment) = WarewolfDataEvaluationCommon.EvalIndexes  env name

let EvalDelete (exp:string)  (env:WarewolfEnvironment) = WarewolfDataEvaluationCommon.EvalDelete exp env


let SortRecset (exp:string) (desc:bool) (env:WarewolfEnvironment)  = WarewolfDataEvaluationCommon.SortRecset exp desc env 


let EvalWhere (exp:string) (env:WarewolfEnvironment)  (func:System.Func<WarewolfAtom,bool>) = Where.EvalWhere env exp (fun a-> func.Invoke( a))