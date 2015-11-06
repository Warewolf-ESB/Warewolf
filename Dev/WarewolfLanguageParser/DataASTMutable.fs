module DataASTMutable

open System.Collections.Generic;
let PositionColumn = "WarewolfPositionColumn"


let GetDecimalPlaces (decimalNumber:float) =
    let mutable decimalPlaces = 1;
    let mutable powers = 10.0;
    if (decimalNumber > 0.0) || (decimalNumber < 0.0) then
        while not ( abs((decimalNumber * powers) % 1.0) = 0.0) do
            powers <- powers*10.0
            decimalPlaces <-decimalPlaces+1
    decimalPlaces;

type WarewolfAttribute =
    | Ordinal
    | Sorted
    | Fragmented
[<CustomEquality;CustomComparison>]
type WarewolfAtom =
    | Float of float
    | Int of int
    | DataString of string
    | Nothing
    | PositionedValue of (int * WarewolfAtom)
    override x.ToString() = match x with 
                            | Float a -> let places = GetDecimalPlaces a
                                         a.ToString(sprintf "F%i" places)
                            | Int a -> a.ToString()
                            | DataString a -> a
                            | Nothing -> ""
                            | PositionedValue (a,b) -> b.ToString()
    override x.Equals y = x.ToString() = y.ToString()
    override x.GetHashCode() = x.ToString().GetHashCode()
    interface System.IComparable with
         member x.CompareTo y = 
             match y with
                | :? WarewolfAtom as z -> match (x,z) with
                                            | (Nothing ,DataString b) when System.String.IsNullOrEmpty(b) -> 0
                                            | (DataString b ,Nothing) when System.String.IsNullOrEmpty(b) -> 0
                                            | ( Int a, Int b ) -> a.CompareTo(b)
                                            | (Float a, Float b ) -> a.CompareTo(b)
                                            | (Int a, Float b ) -> System.Double.Parse(a.ToString()).CompareTo(b)
                                            | (Float a, Int b ) -> a.CompareTo(System.Double.Parse((b.ToString())))
                                            | (Int a, DataString b ) -> a.ToString().CompareTo(b)
                                            | (Float a, DataString b ) -> a.ToString().CompareTo(b)
                                            | (DataString a, DataString b ) -> a.CompareTo(b)
                                            | (DataString a, Float b ) -> a.CompareTo(b.ToString())
                                            | (DataString a, Int b ) -> a.CompareTo(b.ToString())
                                            | (Nothing ,Nothing) -> 0
                                            | (Nothing,_) -> -1
                                            | (a,b) -> ( a.ToString()).CompareTo( b.ToString())
                | a ->x.ToString().CompareTo(a.ToString())
type WarewolfAtomRecord = WarewolfAtom

type WarewolfColumnData = WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>

type WarewolfColumnHeader = string

type WarewolfRecordset = 
    {
        Data : Map<WarewolfColumnHeader,WarewolfColumnData> ;
        Optimisations : WarewolfAttribute;
        LastIndex:int;
        mutable Frame : int;
        

    } with
    member this.Count = this.Data.[PositionColumn].Count


type WarewolfEnvironment = 
    {
       RecordSets : Map<string,WarewolfRecordset>;
       Scalar : Map<string,WarewolfAtom>; 
    }


let rec tryParseAtom (data:string) = 
    let mutable value = 0;  
    if data="0" then
         Int(0)
    else
        if data.StartsWith("0") || data.StartsWith("+") then DataString data
        else
           let success = System.Int32.TryParse(data,&value)
           if success then Int value
           else tryFloatParseAtom data

and tryFloatParseAtom (data:string) = 
    let mutable value=0.0m;
    let mutable valuse=0.0;
    if data.StartsWith("0")  && (not (data.StartsWith("0.")) )then 
        DataString data
    else 
        if(data.Contains(".")) then
               let success = System.Decimal.TryParse(data,&value) && System.Double.TryParse(data,&valuse)
               if success then
                    if(data.EndsWith("0")) && success then
                        DataString data
                    else
                        Float (System.Convert.ToDouble(value))
                else DataString data
        else DataString data


let CompareAtoms (x:WarewolfAtom) (y:WarewolfAtom) = 
             match (x,y) with
                                            | (Nothing ,DataString b) when System.String.IsNullOrEmpty(b) -> 0
                                            | (DataString b ,Nothing) when System.String.IsNullOrEmpty(b) -> 0
                                            | ( Int a, Int b ) -> a.CompareTo(b)
                                            | (Float a, Float b ) -> a.CompareTo(b)
                                            | (Int a, Float b ) -> System.Double.Parse(a.ToString()).CompareTo(b)
                                            | (Float a, Int b ) -> a.CompareTo(System.Double.Parse((b.ToString())))
                                            | (Int a, DataString b ) -> a.ToString().CompareTo(b)
                                            | (Float a, DataString b ) -> a.ToString().CompareTo(b)
                                            | (DataString a, DataString b ) -> a.ToLower().CompareTo(b.ToLower())
                                            | (DataString a, Float b ) -> a.CompareTo(b.ToString())
                                            | (DataString a, Int b ) -> a.CompareTo(b.ToString())
                                            | (Nothing ,Nothing) -> 0
                                            | (Nothing,_) -> -1
                                            | (a,b) -> ( a.ToString()).CompareTo( b.ToString())