module IntellisenseStringProvider
open LanguageAST
//open LanguageEval

open WarewolfDataEvaluationCommon

let Tokenisers = "!@#$%^&*()-=_+{}|:\"?><`~<>?:'{}| ".ToCharArray()
let Tokenisers2 = "[]".ToCharArray()
let Tokenisers3 = "()".ToCharArray()


type FilterOption =
    | Recordsets
    | Scalars 
    | RecordSetNames
    | All

let rec getOptions (variables:LanguageExpression seq) (level:int) (filter:FilterOption) =
    Combine  (Seq.filter (fun (a:LanguageExpression) -> filterOptions filter a ) variables) level variables (level=0) |> List.sortBy (fun (a:string) -> a.ToLower())

and filterOptions  (filter:FilterOption) (a:LanguageExpression) =
    match (a,filter) with
        | (LanguageExpression.RecordSetExpression a ,FilterOption.Recordsets ) -> true
        | (LanguageExpression.ScalarExpression a ,FilterOption.Scalars ) -> true
        | (LanguageExpression.RecordSetNameExpression a ,FilterOption.RecordSetNames ) -> true
        | (_ ,FilterOption.All ) -> true
        | _ ->false
// take a list of variables and cartesian product of the options. 
// can take a bias at some point
and  Combine (variables:LanguageExpression seq) (level:int) (unfilteredvariables:LanguageExpression seq) (startAtzero:bool) = 
    List.collect (fun a-> CombineExpressions level (List.ofSeq unfilteredvariables) a startAtzero ) (List.ofSeq variables)  // clean up multiple enumerations

and CombineExpressions  (level:int) (variables:LanguageExpression list) (variable:LanguageExpression) (startAtzero:bool)  =
    match variable with
    | ScalarExpression a -> CombineScalar a 
    | RecordSetExpression b  -> CombineRecset b level  variables startAtzero
    | RecordSetNameExpression c  -> CombineRecsetName c level  variables
    | WarewolfAtomAtomExpression _ -> List.empty
    | ComplexExpression _ -> List.empty // cant have complex expressions in intellisense because the variable list is made up of simple expressions

and CombineScalar (a:ScalarIdentifier)  =
    [ ScalarExpression a |> languageExpressionToString]

and CombineRecset (a:RecordSetIdentifier) (level:int)  (variables:LanguageExpression list)  (startAtzero:bool) =
    match level with
    | 0  when startAtzero -> 
                               let indexes = CombineIndexAtZero level a
                               List.append  (List.map (fun x -> "[["+ a.Name + "(" + x + ")." + a.Column + "]]") indexes)  (List.map (fun x -> "[["+ a.Name + "(" + x ) indexes) 
    | 0  when not startAtzero -> [ RecordSetExpression a |> languageExpressionToString]

    | _ -> let indexes = CombineIndex level variables
           List.append  (List.map (fun x -> "[["+ a.Name + "(" + x + ")." + a.Column + "]]") indexes)  (List.map (fun x -> "[["+ a.Name + "(" + x ) indexes)

and CombineRecsetName (a:RecordSetName) (level:int)  (variables:LanguageExpression list)  =
    match level with
    | 0 -> [ RecordSetNameExpression a |> languageExpressionToString]
    | _ -> let indexes = CombineIndex level variables
           List.map (fun x -> "[["+ a.Name + "(" + x + ")" + "]]") indexes



and CombineIndex (level:int) (variables:LanguageExpression list)   =
    let newLevel = level - 1
    let combined = Combine variables newLevel variables false
    "*" :: "":: combined

and CombineIndexAtZero (level:int) (variables:RecordSetIdentifier)   =
    "*" :: [""]

let rec ProcessLanguageExpressionList (lst:LanguageExpression list) (acc:string) (replacement:string ) (caretPosition:int) = 
    match lst with
    | h::t -> match h with 
                | WarewolfAtomAtomExpression a -> ProcessLanguageExpressionList t  (acc + replacement) "" caretPosition
                | _ -> let exp = WarewolfDataEvaluationCommon.languageExpressionToString h
                       ProcessLanguageExpressionList t  (acc + exp) replacement caretPosition
    |[] ->acc


let rec takeNonAlphabets (a:string) (acc:string) = 
   match a with
   | "" -> a
   | _ -> if a.ToCharArray().[0] > 'z' || a.ToCharArray().[0] <'A' then  takeNonAlphabets (a.Substring(1)) (acc+a.[0].ToString()) else acc

let rec GetCaretPosition (lst:LanguageExpression list) (caretPosition:int) (acc:string) (i:int)=
    match lst with
    | h::t -> let exp = acc+ WarewolfDataEvaluationCommon.languageExpressionToString h
              if exp.Length >= caretPosition then  i   else GetCaretPosition t caretPosition exp (i + 1)

    |[] ->i

let rec GetCaretPositionInString (lst:LanguageExpression list) (caretPosition:int) (acc:string) (i:int) (currenti:int)=
    match lst with
    | h::t -> let exp = acc+ WarewolfDataEvaluationCommon.languageExpressionToString h
              if exp.Length >= caretPosition then  caretPosition-currenti   else GetCaretPositionInString t caretPosition exp (i + 1) (currenti + (WarewolfDataEvaluationCommon.languageExpressionToString h).Length)

    |[] ->i

let rec DoReplace (text:string)  (caretPosition:int ) (replacement:string) =
    let parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate text
    match parsed with 
        |   ComplexExpression a ->  let caret = GetCaretPosition a caretPosition "" 0
                                    let b =  Array.ofList a  
                                    let bcaret = languageExpressionToString b.[caret]
                                    let app = if bcaret.EndsWith(" ") then " " else "" 
                                    let str = takeNonAlphabets (bcaret) ""
                                    let rep = match b.[caret] with
                                                    | RecordSetExpression xs -> fst (DoReplace (languageExpressionToString b.[caret])  (GetCaretPositionInString a caretPosition "" 0 0 ) replacement)
                                                    |_ -> replacement                                    
                                    b.[caret] <- (LanguageAST.WarewolfAtomAtomExpression ( DataASTMutable.DataString (str+ rep+app) ))
                                    
                                    if caret >0 && (languageExpressionToString b.[caret-1]) = "[[" then  b.[caret-1] <-(LanguageAST.WarewolfAtomAtomExpression ( DataASTMutable.DataString ("") ))
                                    let x = Array.map languageExpressionToString b |> fun ax -> System.String.Join("",ax) 
                                    let cx = Seq.take (caret+1) b |> Array.ofSeq |>Array.map languageExpressionToString  |> fun ax -> System.String.Join("",ax) 
                                    (x,cx.Length)
        | WarewolfAtomAtomExpression a ->   let b = (languageExpressionToString parsed)
                                            let first = b.Substring(0,caretPosition)
                                            let last = b.Substring(caretPosition)
                                            let indexOfData = first.LastIndexOfAny(Tokenisers)
                                            let indexOfEndData = last.IndexOfAny(Tokenisers)
                                            match (indexOfData,indexOfEndData) with
                                                | (-1,-1) -> (replacement,replacement.Length)
                                                | (_,-1) -> (b.Substring(0,indexOfData+1)+replacement,(b.Substring(0,indexOfData+1)+replacement).Length)
                                                | (-1,_) -> (replacement+ last.Substring(indexOfEndData), (replacement).Length)
                                                | (_,_) -> (b.Substring(0,indexOfData+1)+replacement+ b.Substring(indexOfEndData), (replacement+ b.Substring(indexOfEndData)).Length)

                                          
        | RecordSetExpression a ->  let b = (languageExpressionToString parsed)
                                    let first = b.Substring(0,caretPosition)
                                    let last = b.Substring(caretPosition)
                                    let indexOfData = first.IndexOf("[[")
                                    let indexOfEndData = b.IndexOfAny(Tokenisers2,caretPosition)
                                    let indexOfBData = first.LastIndexOfAny(Tokenisers3)
                                    let indexOfEndBData = b.IndexOfAny(Tokenisers3,caretPosition)
                                    if(caretPosition>indexOfBData && caretPosition<= indexOfEndBData)
                                    then
                                        match (indexOfBData,indexOfEndBData) with
                                            | (-1,-1) -> (replacement,replacement.Length)
                                            | (_,-1) ->  (b.Substring(0,indexOfData-1)+replacement,(b.Substring(0,indexOfData-11)+replacement).Length)
                                            | (-1,_) ->  (replacement+ b.Substring(indexOfEndData+2), ( (replacement+ b.Substring(indexOfEndData+2)).Length))
                                            | (_,_) ->   (b.Substring(0,indexOfBData+1)+replacement+ b.Substring(indexOfEndBData), (b.Substring(0,indexOfBData+1)+replacement).Length)
                                    else
                                        match (indexOfData,indexOfEndData) with
                                            | (-1,-1) -> (replacement,replacement.Length)
                                            | (_,-1) -> (b.Substring(0,indexOfData-1)+replacement,(b.Substring(0,indexOfData-11)+replacement).Length)
                                            | (-1,_) -> (replacement+ b.Substring(indexOfEndData+2), (replacement+ b.Substring(indexOfEndData)).Length)
                                            | (_,_) -> (b.Substring(0,indexOfData)+replacement+ b.Substring(indexOfEndData+2), (b.Substring(0,indexOfData)+replacement).Length)
                
        | _ -> (replacement,replacement.Length)

