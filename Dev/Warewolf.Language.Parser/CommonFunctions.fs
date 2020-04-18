module CommonFunctions

open LanguageAST
open DataStorage
open System.Diagnostics.CodeAnalysis
open System
open Newtonsoft.Json.Linq
open Warewolf.Exceptions

[<ExcludeFromCodeCoverage>]
type WarewolfEvalResult = 
    | WarewolfAtomResult of WarewolfAtom
    | WarewolfAtomListresult of WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>
    | WarewolfRecordSetResult of WarewolfRecordset

[<ExcludeFromCodeCoverage>]
type PositionValue = 
    | IndexFoundPosition of int
    | IndexDoesNotExist

let enQuote (atom : WarewolfAtom) = 
    match atom with
    | DataString a -> DataString(sprintf "\"%s\"" a)
    | _ -> atom

let isNotAtom (a : LanguageExpression) = 
    match a with
    | WarewolfAtomExpression _ -> false
    | _ -> true

let getRecordSetPositionsAsInts (recset : WarewolfRecordset) = 
    let AtomToInt(a : WarewolfAtom) = 
        match a with
        | Int a -> a
        | _ -> failwith "the position column contains non ints"
    
    let positions = recset.Data.[PositionColumn]
    Seq.map AtomToInt positions |> Seq.sort

let parseAtom (lang : string) = 
    let at = tryParseAtom lang
    match at with
    | Int _ -> at
    | Float _ -> tryFloatParseAtom lang
    | _ -> at

let IsAtomExpression(a : LanguageExpression) = 
    match a with
    | WarewolfAtomExpression _ -> true
    | _ -> false

let atomtoString (x : WarewolfAtom) = 
    match x with
    | Float a -> 
        let places = GetDecimalPlaces a
        a.ToString(sprintf "F%i" places)
    | Int a -> a.ToString()
    | DataString a -> a
    | JsonObject a -> a.ToString()
    | Nothing -> null
    | NullPlaceholder -> null
    | PositionedValue(_, b) -> b.ToString()

let warewolfAtomRecordtoString (x : WarewolfAtomRecord) = 
    match x with
    | Float a -> 
        let places = GetDecimalPlaces a
        a.ToString(sprintf "F%i" places)
    | Int a -> a.ToString()
    | DataString a -> a
    | JsonObject a -> a.ToString()
    | Nothing -> ""
    | NullPlaceholder -> ""
    | PositionedValue(_, b) -> b.ToString()

let evalResultToString (a : WarewolfEvalResult) = 
    match a with
    | WarewolfAtomResult x -> atomtoString x
    | WarewolfAtomListresult x -> Seq.map warewolfAtomRecordtoString x |> fun a -> System.String.Join(",", a)
    | WarewolfRecordSetResult x -> 
        Map.toList x.Data
        |> List.filter (fun (a, _) -> not (a = PositionColumn))
        |> List.map snd
        |> Seq.collect (fun a -> a)
        |> fun a -> System.String.Join(",", a)

let evalResultToStringAsList (a:WarewolfEvalResult) = 
    match a with
    | WarewolfAtomResult x ->   let f = atomtoString x
                                let list = [ f ]
                                Seq.ofList list
    | WarewolfAtomListresult x -> Seq.map warewolfAtomRecordtoString x |> fun a -> a
    | WarewolfRecordSetResult x -> Map.toList x.Data |> List.filter (fun (a, _) ->not (a=PositionColumn)) |>  List.map snd |> Seq.collect (fun a->a) |> fun a -> Seq.map warewolfAtomRecordtoString a |> fun a -> a

let atomToJsonCompatibleObject (a : WarewolfAtom) : System.Object = 
    match a with
    | Int a -> a :> System.Object
    | DataString a -> 
        match a with
        | z when z.ToLower() = "true" -> true :> System.Object
        | z when z.ToLower() = "false" -> false :> System.Object
        | _ -> a :> System.Object
    | JsonObject jo -> jo :> System.Object
    | Float a -> a :> System.Object
    | Nothing -> null :> System.Object
    | NullPlaceholder -> "" :> System.Object
    | PositionedValue (i, atom) -> "" :> System.Object

let atomToJToken (a : WarewolfAtom) : JToken =
    match a with
    | Int a -> JValue(a) :> JToken
    | DataString a ->
        match a with
        | z when z.ToLower() = "true" -> JValue(true) :> JToken
        | z when z.ToLower() = "false" -> JValue(false) :> JToken
        | _ -> JValue(a) :> JToken
    | JsonObject jo -> jo
    | Float a -> JValue(a) :> JToken
    | Nothing -> null :> JToken
    | NullPlaceholder -> null :> JToken
    | PositionedValue (i,atom) -> JValue("") :> JToken

let evalResultToJsonCompatibleObject (a : WarewolfEvalResult) : System.Object = 
    match a with
    | WarewolfAtomResult x -> atomToJsonCompatibleObject x
    | WarewolfAtomListresult x -> Seq.map atomToJsonCompatibleObject x :> System.Object
    | _ -> failwith "json eval results can only work on atoms"

let evalResultToJToken (a : WarewolfEvalResult) : Newtonsoft.Json.Linq.JToken =
    match a with
    | WarewolfAtomResult x -> atomToJToken x
    | WarewolfAtomListresult x -> JArray(Seq.map atomToJToken x) :> JToken
    | _ -> failwith "json eval results can only work on atoms"

let atomToInt (a : WarewolfAtom) = 
    match a with
    | Int x -> 
        if x <= 0 then failwith "invalid recordset index was less than 0"
        else x
    | _ -> 
        let couldParse, parsed = System.Int32.TryParse(a.ToString())
        if couldParse then parsed
        else failwith "index was not an int"

let getRecordSetIndex (recset : WarewolfRecordset) (position : int) = 
    match recset.Optimisations with
    | Ordinal -> 
        if recset.LastIndex < position then IndexDoesNotExist
        else IndexFoundPosition(position - 1)
    | _ -> 
        let indexes = recset.Data.[PositionColumn]
        let positionAsAtom = Int position
        try 
            Seq.findIndex (fun a -> a = positionAsAtom) indexes |> IndexFoundPosition
        with :? System.Collections.Generic.KeyNotFoundException as a -> IndexDoesNotExist

let getRecordSetIndexAsInt (recset : WarewolfRecordset) (position : int) = 
    match recset.Optimisations with
    | Ordinal -> 
        if recset.LastIndex < position then failwith "row does not exist"
        else position - 1
    | _ -> 
        let indexes = recset.Data.[PositionColumn]
        let positionAsAtom = Int position
        try 
            Seq.findIndex (fun a -> a = positionAsAtom) indexes
        with :? System.Collections.Generic.KeyNotFoundException as ex -> failwith ("row does not exist" + ex.Message)

let evalRecordSetIndex (recset : WarewolfRecordset) (identifier : RecordSetColumnIdentifier) (position : int) = 
    let index = getRecordSetIndex recset position
    match index with
    | IndexFoundPosition a -> recset.Data.[identifier.Column].[a]
    | IndexDoesNotExist -> 
        raise (new NullValueInVariableException("index not found", identifier.Name))

let LanguageExpressionToStringWithoutStuff(x : LanguageExpression) = 
    match x with
    | RecordSetExpression _ -> ""
    | ScalarExpression _ -> ""
    | WarewolfAtomExpression a -> atomtoString a
    | ComplexExpression _ -> ""
    | RecordSetNameExpression _ -> ""
    | JsonIdentifierExpression _ -> ""

let isNotAtomAndNotcomplex (b : LanguageExpression list) (a : LanguageExpression) = 
    let set = 
        b
        |> List.map LanguageExpressionToStringWithoutStuff
        |> Set.ofList
    
    let reserved = [ "[["; "]]" ] |> Set.ofList
    if not (Set.intersect set reserved |> Set.isEmpty) then 
        match a with
        | WarewolfAtomExpression _ -> false
        | _ -> true
    else false

let isNothing (a : WarewolfEvalResult) = 
    match a with
    | WarewolfAtomResult a -> a = Nothing
    | _ -> false

let getLastIndexFromRecordSet (exp:string)  (env:WarewolfEnvironment)  =
    let rset = env.RecordSets.TryFind exp
    match rset with 
    | Some values -> values.LastIndex                    
    | None->failwith "recordset does not exist"