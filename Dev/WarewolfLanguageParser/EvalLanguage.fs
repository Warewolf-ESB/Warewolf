module LanguageEval
open LanguageAST
open DataAST
open Microsoft.FSharp.Text.Lexing
open Parser

let ParseCache = new System.Collections.Generic.Dictionary<string,LanguageExpression>()


let IndexToString (x:Index) =
    match x with 
        | IntIndex a -> a.ToString()
        | Star -> "*"
        | Last -> ""

let AtomtoString (x:Atom )=
    match x with 
        | Float a -> a.ToString()
        | Int a -> a.ToString()
        | DataString a -> a
        | Nothing -> ""

let rec LanguageExpressionToString (x:LanguageExpression) =
    match x with
        | RecordSetExpression a -> sprintf "[[%s(%s).%s]]" a.Name (IndexToString a.Index) a.Column  
        | ScalarExpression a -> sprintf "[[%s]]" a
        | AtomExpression a -> AtomtoString a
        | ComplexExpression a -> List.fold (fun c d -> c + LanguageExpressionToString d) "" a

let evalRecorsSet (recset:RecordSetIdentifier) (env: Environment)  =
        match recset.Index with
            |IntIndex a -> [(fst (env.RecordSets.[recset.Name]).[recset.Column]).[a]]
            | Star ->  fst (env.RecordSets.[recset.Name]).[recset.Column]
            | Last -> [ List.rev (fst (env.RecordSets.[recset.Name]).[recset.Column]) |> List.head ]
            | _ -> failwith "bob"

let evalScalar (scalar:ScalarIdentifier) (env:Environment) =
     ( env.Scalar.[scalar])

type EvalResult = 
    | AtomResult of Atom
    | AtomListresult of Atom list

let EvalResultToString (a:EvalResult) = 
    match a with
    | AtomResult x -> AtomtoString x
    | AtomListresult x -> List.map AtomtoString x |> (List.fold (+) "")

        

let evalRecordSetAsString (env: Environment) (a:RecordSetIdentifier) = 
    let b = (evalRecorsSet a env) |> List.map ((fst) >>AtomtoString ) |> (List.fold (+) "")
    DataString b

let rec Clean (buffer :LanguageExpression) =
    match buffer with
        | RecordSetExpression a -> RecordSetExpression a
        | ScalarExpression a -> ScalarExpression a
        | AtomExpression a -> AtomExpression a
        | ComplexExpression  a ->  (List.filter (fun b -> "" <> (LanguageExpressionToString b)) a) |> (fun a -> if (List.length a) =1 then Clean a.[0] else ComplexExpression a)

let Parse (env: Environment) (lang:string) : LanguageExpression=
    if ParseCache.ContainsKey lang 
    then ParseCache.[lang]
    else

        let lexbuf = LexBuffer<string>.FromString lang 
        let buffer = Parser.start Lexer.tokenstream lexbuf
        let res = buffer |> Clean
        ParseCache.Add(lang,res)
        res

let rec Eval  (env: Environment) (lang:string) : EvalResult=
    let EvalComplex (exp:LanguageExpression list) = 
        if((List.length exp) =1) then
            match exp.[0] with
                | RecordSetExpression a ->  evalRecordSetAsString env a
                | ScalarExpression a ->  (evalScalar a env)
                | AtomExpression a ->  a
        else    
            let start = List.map LanguageExpressionToString  exp |> (List.fold (+) "")
            let evaled = (List.map (LanguageExpressionToString >> (Eval  env)>>EvalResultToString)  exp )|> (List.fold (+) "")
            if( evaled = start) then
                DataString evaled
            else DataString (Eval env evaled|>  EvalResultToString)
    let buffer = 
        if ParseCache.ContainsKey lang 
        then 
            ParseCache.[lang]
        else
            let lexbuf = LexBuffer<string>.FromString lang 
            let temp = Parser.start Lexer.tokenstream lexbuf 
            ParseCache.Add(lang,temp)
            temp
    match buffer with
        | RecordSetExpression a -> AtomListresult( List.map (fst) (evalRecorsSet a env) )
        | ScalarExpression a -> AtomResult (evalScalar a env)
        | AtomExpression a -> AtomResult a
        | ComplexExpression  a -> AtomResult (EvalComplex ( List.filter (fun b -> "" <> (LanguageExpressionToString b)) a)) 

