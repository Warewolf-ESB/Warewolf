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


    
let EvalAssign (exp:string) (value:string) (env:WarewolfEnvironment) = AssignEvaluation.EvalAssign exp value env
   




let EvalMultiAssignOp  (env:WarewolfEnvironment)  (value :IAssignValue ) = AssignEvaluation.EvalMultiAssignOp env value
   


let EvalMultiAssign (values :IAssignValue seq) (env:WarewolfEnvironment) = AssignEvaluation.EvalMultiAssign values env


let EvalAssignWithFrame (value :IAssignValue ) (env:WarewolfEnvironment) = AssignEvaluation.EvalAssignWithFrame value env


let RemoveFraming  (env:WarewolfEnvironment) =
        let recsets = Map.map (fun a b -> {b with Frame = 0 }) env.RecordSets
        {env with RecordSets = recsets}

let AtomtoString a = WarewolfDataEvaluationCommon.AtomtoString a;


let getIndexes (name:string) (env:WarewolfEnvironment) = WarewolfDataEvaluationCommon.EvalIndexes  env name

let EvalDelete (exp:string)  (env:WarewolfEnvironment) = WarewolfDataEvaluationCommon.EvalDelete exp env


let SortRecset (exp:string) (desc:bool) (env:WarewolfEnvironment)  = WarewolfDataEvaluationCommon.SortRecset exp desc env 


let EvalWhere (exp:string) (env:WarewolfEnvironment)  (func:System.Func<WarewolfAtom,bool>) = Where.EvalWhere env exp (fun a-> func.Invoke( a))


let EvalUpdate (exp:string) (env:WarewolfEnvironment)  (func:System.Func<WarewolfAtom,WarewolfAtom>) = UpdateInPlace.EvalUpdate  env exp (fun a-> func.Invoke( a))
//let EvalDistinct (exp:string list)  (env:WarewolfEnvironment)  = Where.EvalDistinct env exp