// !!!!!!! Not Covering this because it is being replaced by 1601
// !!!!!!! Not Covering this because it is being replaced by 1601
// !!!!!!! Not Covering this because it is being replaced by 1601
// !!!!!!! Not Covering this because it is being replaced by 1601
module IntellisenseStringProvider

open LanguageAST
//open LanguageEval
open CommonFunctions
open EvaluationFunctions
open Microsoft.FSharp.Text.Lexing
open System
open System.Diagnostics.CodeAnalysis

let Tokenisers = "!#$%^&*()-=_+{}|:\"?><`~<>?:'{}| ".ToCharArray()

let Tokenisers2 = "[]".ToCharArray()

let Tokenisers3 = "()".ToCharArray()


let LanguageExpressionToSumOfInt(x : LanguageExpression list) = 
    let expressionToInt (current : int) (y : LanguageExpression) = 
        match current with
        | -1 -> -99
        | _ -> 
            match y with
            | RecordSetExpression _ -> current
            | ScalarExpression _ -> current
            | RecordSetNameExpression _ -> current
            | ComplexExpression _ -> current
            | WarewolfAtomExpression _ when languageExpressionToString y = "]]" -> current - 1
            | WarewolfAtomExpression _ when languageExpressionToString y = "[[" -> current + 1
            | WarewolfAtomExpression _ -> current
            | JsonIdentifierExpression _ -> current
    
    let sum = List.fold expressionToInt 0 x
    sum

type FilterOption = 
    | Recordsets
    | Scalars
    | RecordSetNames
    | All

let rec parseLanguageExpressionAndValidate (lang : string) : LanguageExpression * string =     
    if (lang.Contains "[[") then 
        try 
            let lexbuf = LexBuffer<string>.FromString lang
            let buffer = Parser.start Lexer.tokenstream lexbuf
            let res = buffer |> Clean
            match res with
            | RecordSetExpression e -> (res, validateRecordsetIndex e.Index)
            | RecordSetNameExpression e -> (res, validateRecordsetIndex e.Index)
            | ScalarExpression _ -> (res, validateScalar res)
            | ComplexExpression x -> verifyComplexExpression (x)
            | WarewolfAtomExpression _ -> (res, "")
            | JsonIdentifierExpression _ -> (res, "")
        with ex -> 
          if ex.Message.ToLower() = "parse error" then
            if (lang.Length > 2) then 
                let startswithNum, _ = System.Int32.TryParse(lang.[2].ToString())
                match startswithNum with
                | true -> 
                    (WarewolfAtomExpression(DataStorage.DataString lang), 
                     "Variable name " + lang + " begins with a number")
                | false -> 
                    (WarewolfAtomExpression(DataStorage.DataString lang), 
                     "Variable name " + lang + " contains invalid character(s). Only use alphanumeric _ and - ")
            else 
                (WarewolfAtomExpression(DataStorage.DataString lang), 
                 "Variable name " + lang + " contains invalid character(s). Only use alphanumeric _ and - ")
          else
            (WarewolfAtomExpression(DataStorage.DataString lang), ex.Message)                           
    else (WarewolfAtomExpression(parseAtom lang), "")

and validateScalar (lang: LanguageExpression) =
    let stringValue = languageExpressionToString lang
    if (stringValue.Length > 2) then 
       let startswithNum, _ = System.Int32.TryParse(stringValue.[2].ToString())
       match startswithNum with
       | true -> "Variable name " + stringValue + " begins with a number"
       | false -> ""
    else ""    
and checkForInvalidVariables (lang : LanguageExpression list) = 
    let updateLanguageExpression (a : LanguageExpression) = 
        match a with
        | RecordSetExpression _ -> WarewolfAtomExpression(DataStorage.DataString "")
        | RecordSetNameExpression _ -> WarewolfAtomExpression(DataStorage.DataString "")
        | ScalarExpression _ -> WarewolfAtomExpression(DataStorage.DataString "")
        | ComplexExpression _ -> a
        | WarewolfAtomExpression _ -> a
        | JsonIdentifierExpression _ -> WarewolfAtomExpression(DataStorage.DataString "")
    
    let data = 
        List.map (languageExpressionToString << updateLanguageExpression) lang |> fun a -> System.String.Join("", a)
    if data = languageExpressionToString (ComplexExpression lang) then 
        if (data.Length > 2) then 
            let startswithNum, _ = System.Int32.TryParse(data.[2].ToString())
            match startswithNum with
            | true -> (ComplexExpression lang, "Recordset field " + data + " begins with a number")
            | false -> (ComplexExpression lang, "Variable name " + data + " contains invalid character(s). Only use alphanumeric _ and - ")
        else (ComplexExpression lang, "Variable name " + data + " contains invalid character(s). Only use alphanumeric _ and - ")
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
            | (WarewolfAtomExpression _, _) -> parsed
            | (JsonIdentifierExpression _, _) -> parsed
        res

and verifyComplexExpression (lang : LanguageExpression list) = 
    if List.exists IsAtomExpression lang then 
        let balanced = LanguageExpressionToSumOfInt lang
        match balanced with
        | 99 -> (ComplexExpression lang, "The order of [[ and ]] is incorrect")
        | a when a < 0 -> (ComplexExpression lang, "Invalid region detected: A close ]] without a related open [[")
        | 0 -> checkForInvalidVariables lang
        | _ -> (ComplexExpression lang, "Invalid region detected: An open [[ without a related close ]]")
    else (ComplexExpression lang, "")

and validateRecordsetIndex (ind : Index) = 
    match ind with
    | IndexExpression a -> 
        match a with
        | RecordSetExpression _ -> ""
        | RecordSetNameExpression _ -> ""
        | ScalarExpression _ -> ""
        | ComplexExpression _ -> parseLanguageExpressionAndValidate (languageExpressionToString a) |> snd
        | WarewolfAtomExpression _ -> 
            "Recordset index (" + languageExpressionToString a + ") contains invalid character(s)"
        | JsonIdentifierExpression _ -> failwith "obsolete"
    | _ -> ""

let rec takeNonAlphabets (a : string) (acc : string) = 
    match a with
    | "" -> a
    | _ -> 
        if a.ToCharArray().[0] > 'z' || a.ToCharArray().[0] < 'A' then 
            takeNonAlphabets (a.Substring(1)) (acc + a.[0].ToString())
        else acc

let rec getCaretPosition (lst : LanguageExpression list) (caretPosition : int) (acc : string) (i : int) = 
    match lst with
    | h :: t -> 
        let exp = acc + EvaluationFunctions.languageExpressionToString h
        if exp.Length >= caretPosition then i
        else getCaretPosition t caretPosition exp (i + 1)
    | [] -> i

let rec getCaretPositionInString (lst : LanguageExpression list) (caretPosition : int) (acc : string) (i : int) 
        (currenti : int) = 
    match lst with
    | h :: t -> 
        let exp = acc + EvaluationFunctions.languageExpressionToString h
        if exp.Length >= caretPosition then caretPosition - currenti
        else 
            getCaretPositionInString t caretPosition exp (i + 1) 
                (currenti + (EvaluationFunctions.languageExpressionToString h).Length)
    | [] -> i

let rec doReplace (text : string) (caretPosition : int) (replacement : string) = 
    let parsed = EvaluationFunctions.parseLanguageExpressionWithoutUpdate text
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
        b.[caret] <- (LanguageAST.WarewolfAtomExpression(DataStorage.DataString(str + rep + app)))
        if caret > 0 && (languageExpressionToString b.[caret - 1]) = "[[" then 
            b.[caret - 1] <- (LanguageAST.WarewolfAtomExpression(DataStorage.DataString("")))
        let x = Array.map languageExpressionToString b |> fun ax -> System.String.Join("", ax)
        
        let cx = 
            Seq.take (caret + 1) b
            |> Array.ofSeq
            |> Array.map languageExpressionToString
            |> fun ax -> System.String.Join("", ax)
        (x, cx.Length)
    | WarewolfAtomExpression _ -> 
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
