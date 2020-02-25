module DataStorage

open System.Diagnostics.CodeAnalysis
open Newtonsoft.Json.Linq

[<ExcludeFromCodeCoverage>]
let PositionColumn = "WarewolfPositionColumn"

let GetDecimalPlaces(decimalNumber : float) = 
    let mutable decimalPlaces = 1
    let mutable powers = 10.0
    if (decimalNumber > 0.0) || (decimalNumber < 0.0) then 
        while not (abs ((decimalNumber * powers) % 1.0) = 0.0) do
            powers <- powers * 10.0
            decimalPlaces <- decimalPlaces + 1
    decimalPlaces

/// Performance enhancements
[<ExcludeFromCodeCoverage>]
type WarewolfAttribute = 
    | Ordinal
    | Sorted
    | Fragmented

/// basic atomic types supported by warewolf
[<CustomEquality; CustomComparison>]
type WarewolfAtom = 
    | Float of float
    | Int of int
    | DataString of string
    | JsonObject of JContainer
    | Nothing
    | NullPlaceholder
    | PositionedValue of (int * WarewolfAtom)
    
    override x.ToString() = 
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
    
    override x.Equals y = x.ToString() = y.ToString()
    override x.GetHashCode() = x.ToString().GetHashCode()
    interface System.IComparable with
        member x.CompareTo y = 
            match y with
            | :? WarewolfAtom as z -> 
                match (x, z) with
                | (Nothing, DataString b) when System.String.IsNullOrEmpty(b) -> 0
                | (DataString b, Nothing) when System.String.IsNullOrEmpty(b) -> 0
                | (Int a, Int b) -> a.CompareTo(b)
                | (Float a, Float b) -> a.CompareTo(b)
                | (Int a, Float b) -> System.Double.Parse(a.ToString()).CompareTo(b)
                | (Float a, Int b) -> a.CompareTo(System.Double.Parse((b.ToString())))
                | (Int a, DataString b) -> a.ToString().CompareTo(b)
                | (Float a, DataString b) -> a.ToString().CompareTo(b)
                | (DataString a, DataString b) -> a.CompareTo(b)
                | (DataString a, Float b) -> a.CompareTo(b.ToString())
                | (DataString a, Int b) -> a.CompareTo(b.ToString())
                | (Nothing, Nothing) -> 0
                | (Nothing, _) -> -1
                | (a, b) -> (a.ToString()).CompareTo(b.ToString())
            | a -> x.ToString().CompareTo(a.ToString())

/// Atom Alias. Actually has no real purpose other than supporting a possible future divergance between scalars and recordset types 
type WarewolfAtomRecord = WarewolfAtom

/// Recordset Column
type WarewolfColumnData = WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>

///Name of a Column
type WarewolfColumnHeader = string

exception WarewolfInvalidComparisonException of string

/// A Recordset is a dictionary od strings to lists of attoms
/// Last index is maintained as well as the count
[<ExcludeFromCodeCoverage>]
type WarewolfRecordset = 
    { Data : Map<WarewolfColumnHeader, WarewolfColumnData>
      Optimisations : WarewolfAttribute
      LastIndex : int
      mutable Frame : int }
    member this.PositionColumn = this.Data.[PositionColumn]
    member this.Count = this.PositionColumn.Count

///An Environment is a dictionary of recordsets, a dictionary of scalars and a dictionary of json objects
[<ExcludeFromCodeCoverage>]
type WarewolfEnvironment = 
    { RecordSets : Map<string, WarewolfRecordset>
      Scalar : Map<string, WarewolfAtom>
      JsonObjects : Map<string, Newtonsoft.Json.Linq.JContainer> }

///Parse atom from string. Order of precedence is int then float then string
let rec tryParseAtom (data : string) = 
    let mutable value = 0
    if data = "0" then Int(0)
    else if data.StartsWith("0") || data.StartsWith("+") || data.EndsWith("\n") || data.EndsWith("\r") || data.EndsWith("\r\n") ||data.EndsWith(" ") then DataString data
    else 
        let success = System.Int32.TryParse(data, &value)
        if success then Int value
        else tryFloatParseAtom data

///Parse a float. 
and tryFloatParseAtom (data : string) = 
    let mutable value = 0.0m
    let mutable valuse = 0.0
    if data.StartsWith("0") && (not (data.StartsWith("0."))) then DataString data
    else if (data.Contains(".")) then 
        let success = System.Decimal.TryParse(data, &value) && System.Double.TryParse(data, &valuse)
        if success then 
            if (data.EndsWith("0")) && success then DataString data
            else Float(System.Convert.ToDouble(value))
        else DataString data
    else DataString data


let CompareDataStringWithAtom(x : WarewolfAtom) (y : WarewolfAtom) =
    let mutable stringValue = 0.0m : decimal
    match (x, y) with
    | (DataString a, Int b) ->
        let hasDecimalValue = System.Decimal.TryParse(a, &stringValue)
        if (hasDecimalValue) then
            stringValue.CompareTo(decimal b)
        else
            raise (WarewolfInvalidComparisonException "incompatible types in comparison")
    | (DataString a, Float b) ->
        let hasDecimalValue = System.Decimal.TryParse(a, &stringValue)
        if (hasDecimalValue) then
            stringValue.CompareTo(decimal b)
        else
            raise (WarewolfInvalidComparisonException "incompatible types in comparison")
    | (Float a, DataString b) ->
        let hasDecimalValue = System.Decimal.TryParse(b, &stringValue)
        if (hasDecimalValue) then
            (decimal a).CompareTo(stringValue)
        else
            raise (WarewolfInvalidComparisonException "incompatible types in comparison")
    | (Int a, DataString b) ->
        let hasDecimalValue = System.Decimal.TryParse(b, &stringValue)
        if (hasDecimalValue) then
            (decimal a).CompareTo(stringValue)
        else
            raise (WarewolfInvalidComparisonException "incompatible types in comparison")
    | (a, b) -> failwith "unexpected datastring comparison"


/// Comparison between atoms
let CompareAtoms (x : WarewolfAtom) (y : WarewolfAtom) = 
    match (x, y) with
    | (Nothing, DataString b) when System.String.IsNullOrEmpty(b) -> 0
    | (DataString b, Nothing) when System.String.IsNullOrEmpty(b) -> 0
    | (Int a, Int b) -> a.CompareTo(b)
    | (Float a, Float b) -> a.CompareTo(b)
    | (Int a, Float b) -> (float a).CompareTo(b)
    | (Float a, Int b) -> a.CompareTo(float b)
    | (Int a, DataString b) -> (CompareDataStringWithAtom x y)
    | (Float a, DataString b) -> (CompareDataStringWithAtom x y)
    | (DataString a, DataString b) -> a.ToLower().CompareTo(b.ToLower())
    | (DataString a, Float b) -> (CompareDataStringWithAtom x y)
    | (DataString a, Int b) -> (CompareDataStringWithAtom x y)
    | (Nothing, Nothing) -> 0
    | (Nothing, _) -> -1
    | (a, b) -> (a.ToString()).CompareTo(b.ToString())
