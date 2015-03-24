module DataASTMutable

open System.Collections.Generic;

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
                            | Float a -> a.ToString()
                            | Int a -> a.ToString()
                            | DataString a -> a
                            | Nothing -> ""
    override x.Equals y = x.ToString() = y.ToString()
    override x.GetHashCode() = x.ToString().GetHashCode()
    interface System.IComparable with
         member x.CompareTo y = 
             match y with
                | :? WarewolfAtom as z -> match (x,z) with
                                            | ( Int a, Int b ) -> a.CompareTo(b)
                                            | (Float a, Float b ) -> a.CompareTo(b)
                                            | (a,b) -> ( a.ToString()).CompareTo( b.ToString())

type WarewolfAtomRecord = WarewolfAtom

type WarewolfColumnData = WarewolfParserInterop.WarewolfAtomList<WarewolfAtomRecord>

type WarewolfColumnHeader = string

type WarewolfRecordset = 
    {
        Data : Map<WarewolfColumnHeader,WarewolfColumnData> ;
        Optimisations : WarewolfAttribute;
        LastIndex:int;
        Count:int;
        mutable Frame : int;
    }
type WarewolfEnvironment = 
    {
       RecordSets : Map<string,WarewolfRecordset>;
       Scalar : Map<string,WarewolfAtom>; 
    }
