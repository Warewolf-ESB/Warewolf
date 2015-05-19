module EvalTools
//open DataASTMutable
//open LanguageAST
//open WarewolfDataEvaluationCommon
//open ToolAst
//open Unlimited.Applications.BusinessDesignStudio.Activities
//open System.Activities.Statements
//open Dev2.Data.SystemTemplates.Models
//open Newtonsoft.Json;
////let Eval (env:WarewolfEnvironment) (Tools: Tool list)
//
//
//
//
//let rec EvalForEach (fore:ForEach) (env:WarewolfEnvironment) =
//    let rec evalNumberOfExecutes  (iter:int) (t:Tool) (env:WarewolfEnvironment) =
//        match iter with 
//            | 0 -> env
//            | a -> evalNumberOfExecutes (a - 1) (t) (EvalTool t env)
//    let rec evalInRange (start:int) (endi:int)  (t:Tool) (env:WarewolfEnvironment) =
//        match start with 
//            | a when start < endi -> env
//            | _ -> evalInRange (start+1) endi t env
//    let rec EvalInRecordSet (r:RecordSetName) (t:Tool) (env:WarewolfEnvironment) =
//        env.RecordSets.[r.Name] |>  Map.toList  |> List.map snd |> List.map snd |> List.max |> fun ax-> evalNumberOfExecutes ax t env
//    match fore.Options with
//            | NumberOfExecutes a -> evalNumberOfExecutes a fore.ExecutionAST env
//            | InRange (a,b) -> evalInRange a b fore.ExecutionAST env
//            | RecordsSet a ->EvalInRecordSet a fore.ExecutionAST env
//
//and EvalTool    (t:Tool) (env:WarewolfEnvironment) =
//        match t with
//        | MultiAssignTool a -> EvalMultiAssign a.Assigns env |> (EvalTool a.ExecutionASTTrue)
//        | ForEachTool a ->EvalForEach a env
//        | DecisionTool a -> EvalDecision a env
//        | NOPTool ->env
//
//and EvalDecision (a:Decision) (env:WarewolfEnvironment) =
//    let stack = Seq.map (ParseDecisionValue env) a.Conditions.TheStack
//    let resolvedDecision = new Dev2DecisionStack(TheStack = new System.Collections.Generic.List<Dev2Decision>(stack))
//    let dec = {  Conditions=resolvedDecision;
//                 ExecutionASTTrue=a.ExecutionASTTrue;
//                 ExecutionASTFalse=a.ExecutionASTFalse;
//                
//              }:Decision
//    let factory = Dev2.Data.Decisions.Operations.Dev2DecisionFactory.Instance();
//    let res = Seq.map (fun (a:Dev2Decision) -> (factory.FetchDecisionFunction( a.EvaluationFn):Dev2.Data.Decisions.Operations.IDecisionOperation).Invoke([|a.Col1;a.Col2;a.Col3|]) ) stack
//    let resval = Seq.fold (&&) true res
//    match resval with
//    | true-> EvalTool a.ExecutionASTTrue env
//    | false -> EvalTool a.ExecutionASTFalse env
//
//and ParseDecisionValue (env:WarewolfEnvironment) (decision:Dev2Decision)  =
//   
//    let col1 = LanguageEval.Eval env decision.Col1 |> LanguageEval.EvalResultToString
//    let col2 = LanguageEval.Eval env decision.Col2 |> LanguageEval.EvalResultToString
//    let col3 = LanguageEval.Eval env decision.Col3 |> LanguageEval.EvalResultToString
//    new Dev2Decision( Col1=col1 , Col2=col2, Col3 = col3 , EvaluationFn = decision.EvaluationFn)
//
//let ReturnAssign (assign:DsfMultiAssignActivity) =
//    let coll = assign.FieldsCollection
//    Seq.map (fun (a:ActivityDTO) -> {
//                                        Name= a.FieldName;
//                                        Value = a.FieldValue;
//                                        IsCalc =false;
//                    } ) coll |>  Seq.filter (fun a -> not (a.Name.Equals "") ) |>List.ofSeq |>List.rev 
//
//let rec ParseFlowStep (tool:FlowStep) =
//    match tool.Action with
//        | :? DsfMultiAssignActivity as assign -> {  Assigns= ReturnAssign assign;ExecutionASTTrue = ParseTool tool.Next} |> Tool.MultiAssignTool
//        | _  -> {  Assigns= [ {
//                                  Name= "";
//                                 Value = "";
//                                 IsCalc =false;
//                                }
//                            ];
//                  ExecutionASTTrue = ParseTool tool.Next
//                  }|> Tool.MultiAssignTool
//and  Parse  (flowchart:Flowchart) (resourceID:System.Guid)=
//    let start = (flowchart.StartNode) :?> FlowStep
//    ParseTool (start.Next :?> FlowStep)
//
//and ParseTool (tool:FlowNode)  = 
//    match tool with
//    | :? FlowStep as flowStepTool -> ParseFlowStep flowStepTool
//    | :? FlowDecision as decisionTool -> ParseDecisionStep decisionTool 
//    | _ ->NOPTool
//and ParseDecisionStep (tool:FlowDecision) =   
//    let a = tool.Condition;
//    match a with 
//    | :? DsfFlowDecisionActivity as decision  -> ParseDecision tool decision 
//    |_ -> failwith "moocow"
//
//
//and ParseDecision (tool:FlowDecision) (activity:DsfFlowDecisionActivity) =
//    let rawText = activity.ExpressionText;
//    let activityTextjson = rawText.Substring(rawText.IndexOf("{")).Replace(@""",AmbientDataList)","").Replace("\"","!")
//    let activityText = Dev2DecisionStack.FromVBPersitableModelToJSON(activityTextjson)
//    let decisionStack =  JsonConvert.DeserializeObject<Dev2DecisionStack>(activityText)
//    {
//            Conditions = decisionStack;
//            ExecutionASTTrue = ParseTool tool.True;
//            ExecutionASTFalse =  ParseTool tool.False;
//    }:Decision |> Tool.DecisionTool
// 
//
//
//          
//
//
//let NewEnv  = 
//    {
//       RecordSets=Map.empty;
//       Scalar=Map.empty;
//    }