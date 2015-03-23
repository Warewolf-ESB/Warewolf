module DataASTMutable

open System.Collections.Generic;

type WarewolfAttribute =
    | Ordinal
    | Sorted
    | Fragmented

type WarewolfAtom =
    | Float of float
    | Int of int
    | DataString of string
    | Nothing

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
