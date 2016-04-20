// !!!!!!! Not Covering this because it is being replaced by 1601
// !!!!!!! Not Covering this because it is being replaced by 1601
// !!!!!!! Not Covering this because it is being replaced by 1601
// !!!!!!! Not Covering this because it is being replaced by 1601
module IntellisenseStringProvider

open LanguageAST
//open LanguageEval
open CommonFunctions
open WarewolfDataEvaluationCommon
open Microsoft.FSharp.Text.Lexing
open System
open System.Diagnostics.CodeAnalysis

[<Obsolete("Deprecated Usewolf 1601 ")>]
[<ExcludeFromCodeCoverage>]
let Tokenisers = "!@#$%^&*()-=_+{}|:\"?><`~<>?:'{}| ".ToCharArray()

[<Obsolete("Deprecated Usewolf 1601 ")>]
[<ExcludeFromCodeCoverage>]
let Tokenisers2 = "[]".ToCharArray()

[<Obsolete("Deprecated Usewolf 1601 ")>]
[<ExcludeFromCodeCoverage>]
let Tokenisers3 = "()".ToCharArray()


let [<System.Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] LanguageExpressionToSumOfInt(x : LanguageExpression list) = 
    let expressionToInt (current : int) (y : LanguageExpression) = 
        match current with
        | -1 -> -99
        | _ -> 
            match y with
            | RecordSetExpression _ -> current
            | ScalarExpression _ -> current
            | RecordSetNameExpression _ -> current
            | ComplexExpression _ -> current
            | WarewolfAtomAtomExpression _ when languageExpressionToString y = "]]" -> current - 1
            | WarewolfAtomAtomExpression _ when languageExpressionToString y = "[[" -> current + 1
            | WarewolfAtomAtomExpression _ -> current
            | JsonIdentifierExpression _ -> current
    
    let sum = List.fold expressionToInt 0 x
    sum

[<Obsolete("Deprecated Usewolf 1601 ")>]
[<ExcludeFromCodeCoverage>]
type FilterOption = 
    | Recordsets
    | Scalars
    | RecordSetNames
    | All

[<Obsolete("Deprecated Usewolf 1601 ")>]
[<ExcludeFromCodeCoverage>]
let rec parseLanguageExpressionAndValidate (lang : string) : LanguageExpression * string = 
    if (lang.Contains "[[") then 
        try 
            let lexbuf = LexBuffer<string>.FromString lang
            let buffer = Parser.start Lexer.tokenstream lexbuf
            let res = buffer |> Clean
            match res with
            | RecordSetExpression e -> (res, validateRecordsetIndex e.Index)
            | RecordSetNameExpression e -> (res, validateRecordsetIndex e.Index)
            | ScalarExpression _ -> (res, "")
            | ComplexExpression x -> verifyComplexExpression (x)
            | WarewolfAtomAtomExpression _ -> (res, "")
            | JsonIdentifierExpression _ -> (res, "")
        with ex when ex.Message.ToLower() = "parse error" -> 
            if (lang.Length > 2) then 
                let startswithNum, _ = System.Int32.TryParse(lang.[2].ToString())
                match startswithNum with
                | true -> 
                    (WarewolfAtomAtomExpression(DataASTMutable.DataString lang), 
                     "Variable name " + lang + " begins with a number")
                | false -> 
                    (WarewolfAtomAtomExpression(DataASTMutable.DataString lang), 
                     "Variable name " + lang + " contains invalid character(s)")
            else 
                (WarewolfAtomAtomExpression(DataASTMutable.DataString lang), 
                 "Variable name " + lang + " contains invalid character(s)")
    else (WarewolfAtomAtomExpression(parseAtom lang), "")

and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] checkForInvalidVariables (lang : LanguageExpression list) = 
    let updateLanguageExpression (a : LanguageExpression) = 
        match a with
        | RecordSetExpression _ -> WarewolfAtomAtomExpression(DataASTMutable.DataString "")
        | RecordSetNameExpression _ -> WarewolfAtomAtomExpression(DataASTMutable.DataString "")
        | ScalarExpression _ -> WarewolfAtomAtomExpression(DataASTMutable.DataString "")
        | ComplexExpression _ -> a
        | WarewolfAtomAtomExpression _ -> a
        | JsonIdentifierExpression _ -> WarewolfAtomAtomExpression(DataASTMutable.DataString "")
    
    let data = 
        List.map (languageExpressionToString << updateLanguageExpression) lang |> fun a -> System.String.Join("", a)
    if data = languageExpressionToString (ComplexExpression lang) then 
        if (data.Length > 2) then 
            let startswithNum, _ = System.Int32.TryParse(data.[2].ToString())
            match startswithNum with
            | true -> (ComplexExpression lang, "Recordset field " + data + " begins with a number")
            | false -> (ComplexExpression lang, "Variable name " + data + " contains invalid character(s)")
        else (ComplexExpression lang, "Variable name " + data + " contains invalid character(s)")
    else 
        let parsed = parseLanguageExpressionAndValidate data
        
        let res = 
            match parsed with
            | (RecordSetExpression _, _) -> parsed
            | (RecordSetNameExpression _, _) -> parsed
            | (ScalarExpression _, _) -> parsed
            | (ComplexExpression _, _) when data.StartsWith "[[" && data.EndsWith("]]") -> 
                (ComplexExpression lang, "invalid variable name")
            | (ComplexExpression _, _) -> parsed
            | (WarewolfAtomAtomExpression _, _) -> parsed
            | (JsonIdentifierExpression _, _) -> failwith "Obsolete"
        res

and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] verifyComplexExpression (lang : LanguageExpression list) = 
    if List.exists IsAtomExpression lang then 
        let balanced = LanguageExpressionToSumOfInt lang
        match balanced with
        | 99 -> (ComplexExpression lang, "The order of [[ and ]] is incorrect")
        | a when a < 0 -> (ComplexExpression lang, "Invalid region detected: A close ]] without a related open [[")
        | 0 -> checkForInvalidVariables lang
        | _ -> (ComplexExpression lang, "Invalid region detected: An open [[ without a related close ]]")
    else (ComplexExpression lang, "")

and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] validateRecordsetIndex (ind : Index) = 
    match ind with
    | IndexExpression a -> 
        match a with
        | RecordSetExpression _ -> ""
        | RecordSetNameExpression _ -> ""
        | ScalarExpression _ -> ""
        | ComplexExpression _ -> parseLanguageExpressionAndValidate (languageExpressionToString a) |> snd
        | WarewolfAtomAtomExpression _ -> 
            "Recordset index (" + languageExpressionToString a + ") contains invalid character(s)"
        | JsonIdentifierExpression _ -> failwith "obsolete"
    | _ -> ""

[<Obsolete("Deprecated Usewolf 1601 ")>]
[<ExcludeFromCodeCoverage>]
let rec getOptions (variables : LanguageExpression seq) (level : int) (filter : FilterOption) = 
    combine (Seq.filter (fun (a : LanguageExpression) -> filterOptions filter a) variables) level variables (level = 0) 
    |> List.sortBy (fun (a : string) -> a.ToLower())

and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] filterOptions (filter : FilterOption) 
                                                                      (a : LanguageExpression) = 
    match (a, filter) with
    | (LanguageExpression.RecordSetExpression _, FilterOption.Recordsets) -> true
    | (LanguageExpression.ScalarExpression _, FilterOption.Scalars) -> true
    | (LanguageExpression.RecordSetNameExpression _, FilterOption.RecordSetNames) -> true
    | (_, FilterOption.All) -> true
    | _ -> false

// take a list of variables and cartesian product of the options. 
// can take a bias at some point
and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] combine (variables : LanguageExpression seq) 
                                                                      (level : int) 
                                                                      (unfilteredvariables : LanguageExpression seq) 
                                                                      (startAtzero : bool) = 
    List.collect (fun a -> combineExpressions level (List.ofSeq unfilteredvariables) a startAtzero) 
        (List.ofSeq variables) // clean up multiple enumerations

and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] combineExpressions (level : int) 
                                                                      (variables : LanguageExpression list) 
                                                                      (variable : LanguageExpression) 
                                                                      (startAtzero : bool) = 
    match variable with
    | ScalarExpression a -> combineScalar a
    | RecordSetExpression b -> combineRecset b level variables startAtzero
    | RecordSetNameExpression c -> combineRecsetName c level variables
    | WarewolfAtomAtomExpression _ -> List.empty
    | ComplexExpression _ -> List.empty // cant have complex expressions in intellisense because the variable list is made up of simple expressions
    | JsonIdentifierExpression _ -> List.empty

and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] combineScalar (a : ScalarIdentifier) = 
    [ ScalarExpression a |> languageExpressionToString ]

and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] combineRecset (a : RecordSetIdentifier) 
                                                                      (level : int) 
                                                                      (variables : LanguageExpression list) 
                                                                      (startAtzero : bool) = 
    match level with
    | 0 when startAtzero -> 
        let indexes = combineIndexAtZero level a
        List.append (List.map (fun x -> "[[" + a.Name + "(" + x + ")." + a.Column + "]]") indexes) 
            (List.map (fun x -> "[[" + a.Name + "(" + x) indexes)
    | 0 when not startAtzero -> [ RecordSetExpression a |> languageExpressionToString ]
    | _ -> 
        let indexes = combineIndex level variables
        List.append (List.map (fun x -> "[[" + a.Name + "(" + x + ")." + a.Column + "]]") indexes) 
            (List.map (fun x -> "[[" + a.Name + "(" + x) indexes)

and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] combineRecsetName (a : RecordSetName) 
                                                                      (level : int) 
                                                                      (variables : LanguageExpression list) = 
    match level with
    | 0 -> [ RecordSetNameExpression a |> languageExpressionToString ]
    | _ -> 
        let indexes = combineIndex level variables
        List.map (fun x -> "[[" + a.Name + "(" + x + ")" + "]]") indexes

and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] combineIndex (level : int) 
                                                                      (variables : LanguageExpression list) = 
    let newLevel = level - 1
    let combined = combine variables newLevel variables false
    "*" :: "" :: combined

and [<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>] combineIndexAtZero (_ : int) 
                                                                      (_ : RecordSetIdentifier) = "*" :: [ "" ]

[<Obsolete("Deprecated Usewolf 1601 ")>]
[<ExcludeFromCodeCoverage>]
let rec processLanguageExpressionList (lst : LanguageExpression list) (acc : string) (replacement : string) 
        (caretPosition : int) = 
    match lst with
    | h :: t -> 
        match h with
        | WarewolfAtomAtomExpression _ -> processLanguageExpressionList t (acc + replacement) "" caretPosition
        | _ -> 
            let exp = WarewolfDataEvaluationCommon.languageExpressionToString h
            processLanguageExpressionList t (acc + exp) replacement caretPosition
    | [] -> acc

[<Obsolete("Deprecated Usewolf 1601 ")>]
[<ExcludeFromCodeCoverage>]
let rec takeNonAlphabets (a : string) (acc : string) = 
    match a with
    | "" -> a
    | _ -> 
        if a.ToCharArray().[0] > 'z' || a.ToCharArray().[0] < 'A' then 
            takeNonAlphabets (a.Substring(1)) (acc + a.[0].ToString())
        else acc

[<Obsolete("Deprecated Usewolf 1601 "); ExcludeFromCodeCoverage>]
let rec getCaretPosition (lst : LanguageExpression list) (caretPosition : int) (acc : string) (i : int) = 
    match lst with
    | h :: t -> 
        let exp = acc + WarewolfDataEvaluationCommon.languageExpressionToString h
        if exp.Length >= caretPosition then i
        else getCaretPosition t caretPosition exp (i + 1)
    | [] -> i

[<Obsolete("Deprecated Usewolf 1601 ")>]
[<ExcludeFromCodeCoverage>]
let rec getCaretPositionInString (lst : LanguageExpression list) (caretPosition : int) (acc : string) (i : int) 
        (currenti : int) = 
    match lst with
    | h :: t -> 
        let exp = acc + WarewolfDataEvaluationCommon.languageExpressionToString h
        if exp.Length >= caretPosition then caretPosition - currenti
        else 
            getCaretPositionInString t caretPosition exp (i + 1) 
                (currenti + (WarewolfDataEvaluationCommon.languageExpressionToString h).Length)
    | [] -> i

[<Obsolete("Deprecated Usewolf 1601 ")>]
[<ExcludeFromCodeCoverage>]
let rec doReplace (text : string) (caretPosition : int) (replacement : string) = 
    let parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate text
    match parsed with
    | ComplexExpression a -> 
        let caret = getCaretPosition a caretPosition "" 0
        let b = Array.ofList a
        let bcaret = languageExpressionToString b.[caret]
        
        let app = 
            if bcaret.EndsWith(" ") then " "
            else ""
        
        let str = takeNonAlphabets (bcaret) ""
        
        let rep = 
            match b.[caret] with
            | RecordSetExpression _ -> 
                fst 
                    (doReplace (languageExpressionToString b.[caret]) (getCaretPositionInString a caretPosition "" 0 0) 
                         replacement)
            | _ -> replacement
        b.[caret] <- (LanguageAST.WarewolfAtomAtomExpression(DataASTMutable.DataString(str + rep + app)))
        if caret > 0 && (languageExpressionToString b.[caret - 1]) = "[[" then 
            b.[caret - 1] <- (LanguageAST.WarewolfAtomAtomExpression(DataASTMutable.DataString("")))
        let x = Array.map languageExpressionToString b |> fun ax -> System.String.Join("", ax)
        
        let cx = 
            Seq.take (caret + 1) b
            |> Array.ofSeq
            |> Array.map languageExpressionToString
            |> fun ax -> System.String.Join("", ax)
        (x, cx.Length)
    | WarewolfAtomAtomExpression _ -> 
        let b = (languageExpressionToString parsed)
        let first = b.Substring(0, caretPosition)
        let last = b.Substring(caretPosition)
        let indexOfData = first.LastIndexOfAny(Tokenisers)
        let indexOfEndData = last.IndexOfAny(Tokenisers)
        match (indexOfData, indexOfEndData) with
        | (-1, -1) -> (replacement, replacement.Length)
        | (_, -1) -> 
            (b.Substring(0, indexOfData + 1) + replacement, (b.Substring(0, indexOfData + 1) + replacement).Length)
        | (-1, _) -> (replacement + last.Substring(indexOfEndData), (replacement).Length)
        | (_, _) -> 
            (b.Substring(0, indexOfData + 1) + replacement + b.Substring(indexOfEndData), 
             (replacement + b.Substring(indexOfEndData)).Length)
    | RecordSetExpression _ -> 
        let b = (languageExpressionToString parsed)
        let first = b.Substring(0, caretPosition)
        let indexOfData = first.IndexOf("[[")
        let indexOfEndData = b.IndexOfAny(Tokenisers2, caretPosition)
        let indexOfBData = first.LastIndexOfAny(Tokenisers3)
        let indexOfEndBData = b.IndexOfAny(Tokenisers3, caretPosition)
        if (caretPosition > indexOfBData && caretPosition <= indexOfEndBData) then 
            match (indexOfBData, indexOfEndBData) with
            | (-1, -1) -> (replacement, replacement.Length)
            | (_, -1) -> 
                (b.Substring(0, indexOfData - 1) + replacement, (b.Substring(0, indexOfData - 11) + replacement).Length)
            | (-1, _) -> 
                (replacement + b.Substring(indexOfEndData + 2), ((replacement + b.Substring(indexOfEndData + 2)).Length))
            | (_, _) -> 
                (b.Substring(0, indexOfBData + 1) + replacement + b.Substring(indexOfEndBData), 
                 (b.Substring(0, indexOfBData + 1) + replacement).Length)
        else 
            match (indexOfData, indexOfEndData) with
            | (-1, -1) -> (replacement, replacement.Length)
            | (_, -1) -> 
                (b.Substring(0, indexOfData - 1) + replacement, (b.Substring(0, indexOfData - 11) + replacement).Length)
            | (-1, _) -> 
                (replacement + b.Substring(indexOfEndData + 2), (replacement + b.Substring(indexOfEndData)).Length)
            | (_, _) -> 
                (b.Substring(0, indexOfData) + replacement + b.Substring(indexOfEndData + 2), 
                 (b.Substring(0, indexOfData) + replacement).Length)
    | _ -> (replacement, replacement.Length)
