module ToolAst
open LanguageAST
open DataAST

type Assign = 
    {
        Name:string;
        Value : string;
        IsCalc :bool;
    }

type MultiAssign = Assign list

type ForEachOptions = 
    | NumberOfExecutes of int
    | InRange of int * int
    | RecordsSet of RecordSetName
    
type ForEach = 
    {
        ExecutionAST:Tool;
        Options : ForEachOptions;
    }
and Tool =
    | MultiAssignTool of MultiAssign
    | ForEachTool of ForEach
    
