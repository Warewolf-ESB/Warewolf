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

        Frame=0;
    }

let CreateEnv (vals:string) = 
     {
       RecordSets =Map.empty;
       Scalar =Map.empty;
       JsonObjects = Map.empty
     }

let AddRecsetToEnv (name:string) (env:WarewolfEnvironment) = 
    if env.RecordSets.ContainsKey name
    then
       env
    else
       let b = CreateDataSet ""
       let a = {env with RecordSets= (Map.add name b env.RecordSets);}
       a
let EvalEnvExpression (exp:string) (update:int) (env:WarewolfEnvironment) =
     WarewolfDataEvaluationCommon.eval env update exp

let EvalWithPositions (exp:string) (update:int) (env:WarewolfEnvironment) = WarewolfDataEvaluationCommon.evalWithPositions env update exp

let innerConvert  (i:int) (a: WarewolfAtom) =
        match a with
        | PositionedValue (a,b) -> new Dev2.Common.RecordSetSearchPayload( a,atomtoString b)
        | b->  new Dev2.Common.RecordSetSearchPayload( i,atomtoString b)

let AtomListToSearchTo (atoms :WarewolfAtom seq) =
    
    Seq.mapi innerConvert atoms

let RecordsetToSearchTo (recordset:WarewolfRecordset) =
    let cols = recordset.Data
    let data = Seq.map atomToInt cols.[PositionColumn] 
    let dataToWorkWith  = (Map.filter (fun a b-> a= PositionColumn) cols)|>Map.toSeq 
    Seq.map snd dataToWorkWith  |> Seq.map (Seq.zip data)  |> Seq.collect (fun a -> a) |> Seq.map (fun (a,b) -> innerConvert a b )

let EvalEnvExpressionWithPositions (exp:string) (update:int) (env:WarewolfEnvironment) =
     let data = WarewolfDataEvaluationCommon.evalWithPositions env update exp
     match data with
     | WarewolfAtomListresult  a -> AtomListToSearchTo a
     | WarewolfRecordSetResult b -> RecordsetToSearchTo b
     | WarewolfAtomResult a -> Seq.ofList [new Dev2.Common.RecordSetSearchPayload( 1,atomtoString a)]

let EvalRecordSetIndexes (exp:string) (update:int) (env:WarewolfEnvironment) =
     WarewolfDataEvaluationCommon.eval env update exp


    
let EvalAssign (exp:string) (value:string) (update:int) (env:WarewolfEnvironment) = AssignEvaluation.EvalAssign exp value  update env
   




let EvalMultiAssignOp  (env:WarewolfEnvironment) (update:int)  (value :IAssignValue ) = AssignEvaluation.EvalMultiAssignOp env update value
   


let EvalMultiAssign (values :IAssignValue seq) (update:int) (env:WarewolfEnvironment) = AssignEvaluation.EvalMultiAssign values update env


let EvalAssignWithFrame (value :IAssignValue ) (update:int) (env:WarewolfEnvironment) = AssignEvaluation.EvalAssignWithFrame value update env

let EvalAssignFromList (value :string ) (data:WarewolfAtom seq) (env:WarewolfEnvironment) (update:int) (shouldUseLast:bool) = AssignEvaluation.EvalMultiAssignList env data value update shouldUseLast

let RemoveFraming  (env:WarewolfEnvironment) =
        let recsets = Map.map (fun a b -> {b with Frame = 0 }) env.RecordSets
        {env with RecordSets = recsets}

let AtomtoString a = WarewolfDataEvaluationCommon.atomtoString a;


let getIndexes (name:string) (update:int) (env:WarewolfEnvironment) = WarewolfDataEvaluationCommon.evalIndexes  env update name

let EvalDelete (exp:string) (update:int) (env:WarewolfEnvironment) = WarewolfDataEvaluationCommon.evalDelete exp update env


let SortRecset (exp:string) (desc:bool) (update:int) (env:WarewolfEnvironment)  = WarewolfDataEvaluationCommon.sortRecset exp desc update env 


let EvalWhere (exp:string) (env:WarewolfEnvironment) (update:int)  (func:System.Func<WarewolfAtom,bool>) = Where.evalWhere env exp update (fun a-> func.Invoke( a))


let EvalUpdate (exp:string) (env:WarewolfEnvironment) (update:int)  (func:System.Func<WarewolfAtom,WarewolfAtom>) = UpdateInPlace.EvalUpdate  env exp update (fun a-> func.Invoke( a))
//let EvalDistinct (exp:string list)  (env:WarewolfEnvironment)  = Where.EvalDistinct env exp

let EvalDataShape (exp:string) (env:WarewolfEnvironment) = AssignEvaluation.EvalDataShape exp 0 env

let IsValidRecsetExpression (exp:string) = 
    let parsed = WarewolfDataEvaluationCommon.parseLanguageExpression exp 0
    match parsed with 
        | LanguageExpression.WarewolfAtomAtomExpression a -> true
        | LanguageExpression.ComplexExpression b -> true
        | ScalarExpression b -> true
        | RecordSetExpression recset -> match recset.Index with
                                            | IntIndex int -> if int<0 then false else true
                                            | Last -> true
                                            | Star -> true

                                            | IndexExpression indexp ->  match indexp with 
                                                                            |  WarewolfAtomAtomExpression atom -> let inval = atomToInt atom

                                                                                                                  if inval<0 then false else true
                                                                            | _ -> true
        | _ -> true
  
  
let RecordsetExpressionExists (exp:string) (env:WarewolfEnvironment) =
    let parsed = WarewolfDataEvaluationCommon.parseLanguageExpression exp 0
    match parsed with 
        | LanguageExpression.WarewolfAtomAtomExpression a -> false
        | LanguageExpression.ComplexExpression b -> false
        | ScalarExpression b -> false
        | RecordSetExpression recset ->  if env.RecordSets.ContainsKey recset.Name then
                                            env.RecordSets.[recset.Name].Data.ContainsKey recset.Column
                                         else false
        | _ -> false
              