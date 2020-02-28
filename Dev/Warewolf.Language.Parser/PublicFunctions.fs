module PublicFunctions

#nowarn "CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001"

open LanguageAST
//open LanguageEval
open DataStorage
open Dev2.Common.Interfaces
open CommonFunctions
open Delete
open System.Text.RegularExpressions
open Newtonsoft.Json

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
///Evaluate an expression
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
let EvalAssignWithFrameTypeCast (value : IAssignValue) (update : int) (env : WarewolfEnvironment) (shouldTypeCast :  ShouldTypeCast) = 
   AssignEvaluation.evalAssignWithFrameTypeCast value update env shouldTypeCast

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
    let parsed = EvaluationFunctions.parseLanguageExpression exp 0 ShouldTypeCast.Yes
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
    let parsed = EvaluationFunctions.parseLanguageExpression exp 0 ShouldTypeCast.Yes
    match parsed with
    | LanguageExpression.WarewolfAtomExpression _ -> false
    | LanguageExpression.ComplexExpression _ -> false
    | ScalarExpression _ -> false
    | RecordSetExpression recset -> 
        if env.RecordSets.ContainsKey recset.Name then env.RecordSets.[recset.Name].Data.ContainsKey recset.Column
        else false
    | _ -> false


let EvalEnvRecordSets (env : WarewolfEnvironment) =
    Map.toSeq env.RecordSets
let EvalEnvJsonObjects (env : WarewolfEnvironment) =
    Map.toSeq env.JsonObjects

let private escapeChars = Regex("[\n\r\"]", RegexOptions.Compiled)
let private matchev =
    MatchEvaluator(fun m ->
        match m.Value with
        | "\n" -> "\\n"
        | "\r" -> "\\r"
        | "\"" -> "\\\""
        | v -> v)

let EscapeJsonString (s : string) =
    escapeChars.Replace(s, matchev)


let private YieldAtomAsJson(atom : WarewolfAtom) =
    seq {
        match atom with
        | DataString s ->
            match s with
            | z when z.ToLower() = "true" -> yield z.ToLower()
            | z when z.ToLower() = "false" -> yield z.ToLower()
            | _ ->
                yield "\""
                yield string atom
                yield "\""
        | _ -> yield atom.ToString()
    }
let EvalEnv (env : WarewolfEnvironment) =
    seq {
        yield "{\"scalars\":{"
        let mutable i = 0
        for scalar in env.Scalar do
            yield "\""
            yield scalar.Key
            yield "\":"
            yield! YieldAtomAsJson scalar.Value

            if i < env.Scalar.Count - 1 then
                yield ","
            i <- i + 1

        yield "},\"record_sets\":{"
        let mutable i = 0
        for recordset in env.RecordSets do
            yield "\""
            yield recordset.Key
            yield "\":{"
            let columns = recordset.Value.Data
            let mutable j = 0
            for column in columns do
                yield "\""
                yield column.Key
                yield "\":["
                let mutable k = 0
                for item in column.Value do
                    yield! YieldAtomAsJson item
                    if k < column.Value.Count - 1 then
                        yield ","
                    k <- k + 1
                yield "]"
                if j < columns.Count - 1 then
                    yield ","
                j <- j + 1
            yield "}"

            if i < env.RecordSets.Count - 1 then
                yield ","
            i <- i + 1

        yield "},\"json_objects\":{"
        let mutable i = 0
        for jsonObject in env.JsonObjects do
            yield "\""
            yield jsonObject.Key
            yield "\":"
            yield match jsonObject.Value with
                  | null ->  "null"
                  | _ -> jsonObject.Value.ToString(Formatting.None);


            if i < env.JsonObjects.Count - 1 then
                yield ","
            i <- i + 1
        yield "}}"
    }

let EvalEnvExpressionToRecordSet (name : string) (update : int) (env : WarewolfEnvironment) =
    let buffer = EvaluationFunctions.parseLanguageExpression name update ShouldTypeCast.Yes
    match buffer with
    | RecordSetNameExpression a when env.RecordSets.ContainsKey a.Name -> EvaluationFunctions.evalDataSetExpression env update a
    | _ -> raise (new Dev2.Common.Common.NullValueInVariableException("recordset not found",EvaluationFunctions.languageExpressionToString buffer))

let EvalEnvExpressionToArrayTable (name : string) (update : int) (env : WarewolfEnvironment) (throwsifnotexists : bool) =
    let buildRow (list : list<WarewolfAtom list>) =
        match list with
        | [] -> [| |]
        | _ :: tail -> list |> List.map (fun x -> x.[0])
                            |> List.toArray

    let rec buildRows (fieldNamesRow : WarewolfAtom[]) (data : list<WarewolfAtom list>) =
        seq {
            if data.[0].Length > 0 then
                yield buildRow data
                yield! (buildRows fieldNamesRow (data |> List.map (fun x -> x |> List.tail)))
        }

    let buildRows (data : Map<WarewolfColumnHeader, WarewolfColumnData>) =
        let dataWithoutHeader = data
                                  |> Map.filter (fun columnHeader _ -> columnHeader.ToString() <> "WarewolfPositionColumn")
                                  |> Map.fold (fun state key value -> List.append state [value]) []
                                  |> List.map<WarewolfColumnData, WarewolfAtom list> (fun x -> [for y in x do yield y;])
        let fieldNamesRow = data
                                |> Map.filter (fun name _ -> name <> "WarewolfPositionColumn")
                                |> Map.fold (fun state key value -> List.append state [key]) []
                                |> List.map (fun header -> WarewolfAtom.DataString(string header))
                                |> List.toArray

        seq {
            yield fieldNamesRow
            yield! (buildRows fieldNamesRow dataWithoutHeader)
        }

    let recordset = EvalEnvExpressionToRecordSet name update env;
    try
        match recordset with
        | WarewolfRecordSetResult recordsetResult ->
            buildRows recordsetResult.Data
        | _ -> raise (new Dev2.Common.Common.NullValueInVariableException("recordset not found","recordset"))
    with
    | ex when throwsifnotexists -> raise ex
    | _ when not throwsifnotexists -> null

///Evaluate an expression to a Table
let EvalEnvExpressionToTable (name : string) (update : int) (env : WarewolfEnvironment) (throwsifnotexists : bool) =
    seq {
        let table = EvalEnvExpressionToArrayTable name update env throwsifnotexists;

        let fieldNames = Seq.take 1 table |> Seq.exactlyOne |> Array.map (fun x -> string x)

        for row in (table |> Seq.skip 1) do
            let assocRow = Array.zip fieldNames row
            yield assocRow
    }
