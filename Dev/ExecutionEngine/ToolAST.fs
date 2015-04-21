module ToolAST

//
//open LanguageAST
//open DataASTMutable
//open Dev2.Data.SystemTemplates.Models
//
//type Condition = 
//    {
//        LeftOperand:string;
//        RightOperand:string;
//        Operation:string;
//    }   
//
//type Assign = 
//    {
//        Name:string;
//        Value : string;
//        IsCalc :bool;
//    }
//
//type MultiAssign = 
//    { 
//        Assigns:Assign list;
//        ExecutionASTTrue:Tool;
//    }
//
//and ForEachOptions = 
//    | NumberOfExecutes of int
//    | InRange of int * int
//    | RecordsSet of RecordSetName
//    
//and ForEach = 
//    {
//        ExecutionAST:Tool;
//        Options : ForEachOptions;
//    }
//and Decision =
//    {
//        Conditions:Dev2DecisionStack;
//        ExecutionASTTrue:Tool;
//        ExecutionASTFalse:Tool;
//    }
//and Tool =
//    | ForEachTool of ForEach
//    | DecisionTool of Decision
//    | SwitchTool of Decision
//    | WarewolfActivity of Unlimited.Applications.BusinessDesignStudio.Activities.IDev2Activity
//    | NOPTool
//
