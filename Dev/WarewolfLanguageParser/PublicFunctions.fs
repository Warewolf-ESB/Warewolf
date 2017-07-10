module PublicFunctions

open LanguageAST
//open LanguageEval
open DataStorage
open Dev2.Common.Interfaces
open CommonFunctions
open Delete

let PositionColumn = "WarewolfPositionColumn"
///Create a RecordSet
let CreateDataSet(a : string) = 
    let col = new WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>(WarewolfAtomRecord.Nothing)
    { Data = [ (PositionColumn, col) ] |> Map.ofList
      Optimisations = Ordinal
      LastIndex = 0
      Frame = 0 }
///Create an Environment
let CreateEnv(vals : string) = 
    { RecordSets = Map.empty
      Scalar = Map.empty
      JsonObjects = Map.empty }
///Add a Recordset to an environment
let AddRecsetToEnv (name : string) (env : WarewolfEnvironment) = 
    if env.RecordSets.ContainsKey name then env
    else 
        let b = CreateDataSet ""
        let a = { env with RecordSets = (Map.add name b env.RecordSets) }
        a
///Evalutae an expression
let EvalEnvExpression (exp : string) (update : int) (shouldEscape:bool) (env : WarewolfEnvironment) = 
    EvaluationFunctions.eval env update shouldEscape exp
///eval and return positions
let EvalWithPositions (exp : string) (update : int) (env : WarewolfEnvironment) = 
    EvaluationFunctions.evalWithPositions env update exp

let innerConvert (i : int) (a : WarewolfAtom) = 
    match a with
    | PositionedValue(a, b) -> new Dev2.Common.RecordSetSearchPayload(a, atomtoString b)
    | b -> new Dev2.Common.RecordSetSearchPayload(i, atomtoString b)
///helper function. best move this to C#
let AtomListToSearchTo(atoms : WarewolfAtom seq) = Seq.mapi innerConvert atoms
///helper function. best move this to C#
let RecordsetToSearchTo(recordset : WarewolfRecordset) = 
    let cols = recordset.Data
    let data = Seq.map atomToInt cols.[PositionColumn]
    let dataToWorkWith = (Map.filter (fun a _ -> a = PositionColumn) cols) |> Map.toSeq
    Seq.map snd dataToWorkWith
    |> Seq.map (Seq.zip data)
    |> Seq.collect (fun a -> a)
    |> Seq.map (fun (a, b) -> innerConvert a b)

let EvalRecordSetIndexes (exp : string) (update : int) (env : WarewolfEnvironment) = 
    EvaluationFunctions.eval env update false exp

let EvalAssign (exp : string) (value : string) (update : int) (env : WarewolfEnvironment) = 
    AssignEvaluation.evalAssign exp value update env

let EvalMultiAssignOp (env : WarewolfEnvironment) (update : int) (value : IAssignValue) = 
    AssignEvaluation.evalMultiAssignOp env update value

let EvalMultiAssign (values : IAssignValue seq) (update : int) (env : WarewolfEnvironment) = 
    AssignEvaluation.evalMultiAssign values update env

let EvalAssignWithFrame (value : IAssignValue) (update : int) (env : WarewolfEnvironment) = 
    AssignEvaluation.evalAssignWithFrame value update env

let EvalAssignWithFrameStrict (value : IAssignValue) (update : int) (env : WarewolfEnvironment) = 
    AssignEvaluation.evalAssignWithFrameStrict value update env

let EvalAssignFromList (value : string) (data : WarewolfAtom seq) (env : WarewolfEnvironment) (update : int) 
    (shouldUseLast : bool) = AssignEvaluation.evalMultiAssignList env data value update shouldUseLast

let RemoveFraming(env : WarewolfEnvironment) = 
    let recsets = Map.map (fun _ b -> { b with Frame = 0 }) env.RecordSets
    { env with RecordSets = recsets }

let AtomtoString a = atomtoString a

let GetIndexes (name : string) (update : int) (env : WarewolfEnvironment) = 
    EvaluationFunctions.evalIndexes env update name

let EvalDelete (exp : string) (update : int) (env : WarewolfEnvironment) = Delete.evalDelete exp update env

let SortRecset (exp : string) (desc : bool) (update : int) (env : WarewolfEnvironment) = 
    Sort.sortRecset exp desc update env

let EvalWhere (exp : string) (env : WarewolfEnvironment) (update : int) (func : System.Func<WarewolfAtom, bool>) = 
    Where.evalWhere env exp update (fun a -> func.Invoke(a))

let EvalUpdate (exp : string) (env : WarewolfEnvironment) (update : int) 
    (func : System.Func<WarewolfAtom, WarewolfAtom>) = UpdateInPlace.evalUpdate env exp update (fun a -> func.Invoke(a))
//let EvalDistinct (exp:string list)  (env:WarewolfEnvironment)  = Where.EvalDistinct env exp

let EvalDataShape (exp : string) (env : WarewolfEnvironment) = AssignEvaluation.evalDataShape exp 0 env

let IsValidRecsetExpression(exp : string) = 
    let parsed = EvaluationFunctions.parseLanguageExpression exp 0
    match parsed with
    | LanguageExpression.WarewolfAtomExpression _ -> true
    | LanguageExpression.ComplexExpression _ -> true
    | ScalarExpression _ -> true
    | RecordSetExpression recset -> 
        match recset.Index with
        | IntIndex int -> 
            if int < 0 then false
            else true
        | Last -> true
        | Star -> true
        | IndexExpression indexp -> 
            match indexp with

            | _ -> true
    | _ -> true

let RecordsetExpressionExists (exp : string) (env : WarewolfEnvironment) = 
    let parsed = EvaluationFunctions.parseLanguageExpression exp 0
    match parsed with
    | LanguageExpression.WarewolfAtomExpression _ -> false
    | LanguageExpression.ComplexExpression _ -> false
    | ScalarExpression _ -> false
    | RecordSetExpression recset -> 
        if env.RecordSets.ContainsKey recset.Name then env.RecordSets.[recset.Name].Data.ContainsKey recset.Column
        else false
    | _ -> false
