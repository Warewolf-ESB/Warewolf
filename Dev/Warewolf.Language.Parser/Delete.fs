module Delete
open LanguageAST
open DataStorage
open WarewolfParserInterop
open CommonFunctions
open EvaluationFunctions

/// delete a one or more rows from a recordset
let deleteValues (exp:string)  (env:WarewolfEnvironment) =
    let rset = env.RecordSets.TryFind exp
    match rset with 
    | Some x -> {x with Data = Map.map (fun _ _ -> new WarewolfAtomList<WarewolfAtom>(WarewolfAtom.Nothing)) x.Data ;LastIndex=0;  } 
    | None->failwith "recordset does not exist"

/// delete a  rows from a recordset
let deleteIndex  (exp:string) (index:int)   (env:WarewolfEnvironment) =
    let rset = env.RecordSets.TryFind exp
    match rset with 
    | Some values -> let pos = Seq.findIndex ( fun a-> atomtoString a = index.ToString())  values.Data.[PositionColumn]
                     let posAsInt = pos   
                     let newData =  Map.map (fun (_:string) (b:WarewolfAtomList<WarewolfAtom>) -> b.DeletePosition( posAsInt ) ) values.Data                  
                     {  values  with Optimisations = (if values.Optimisations = Ordinal then Sorted else values.Optimisations) ;
                                              Data = newData  ;                        
                                              LastIndex= if index = values.LastIndex && newData.[PositionColumn].Count>0 then atomToInt (Seq.max  newData.[PositionColumn]) else values.LastIndex ; 
                     } 
    | None->failwith "recordset does not exist"


/// delete a row with an expression index. eg delete [[Rec([[a]]).x]]
let rec deleteExpressionIndex (b:RecordSetName) (ind: LanguageExpression) (update:int)  (env:WarewolfEnvironment)  =
    let data = languageExpressionToString ind |> (eval env update false) |> evalResultToString
    match ind with 
    | WarewolfAtomExpression atom ->
                match atom with
                     | _ -> failwith "recordsets must have an integer star or empty index"
    |_->evalDelete( (sprintf "[[%s(%s)]]" b.Name data)) update env 


///delete overall function
and evalDelete (exp:string) (update:int)   (env:WarewolfEnvironment) =
    let left = parseLanguageExpression exp update ShouldTypeCast.Yes
    match left with 
                |   RecordSetNameExpression b ->  match b.Index with
                                                                 | Star -> deleteValues  b.Name env |> (fun upd-> {env with RecordSets = Map.map (fun ax bx -> if ax=b.Name then upd else bx ) env.RecordSets} )
                                                                 | Last ->  deleteIndex  b.Name (getLastIndexFromRecordSet  b.Name env) env |> (fun upd-> {env with RecordSets = Map.map (fun ax bx -> if ax=b.Name then upd else bx ) env.RecordSets} )
                                                                 | IntIndex a -> deleteIndex  b.Name a env |> (fun upd-> {env with RecordSets = Map.map (fun ax bx -> if ax=b.Name then upd else bx ) env.RecordSets} )
                                                                 | IndexExpression exp ->  deleteExpressionIndex b exp update env
                                                               

                |_-> failwith "only recordsets can be deleted"