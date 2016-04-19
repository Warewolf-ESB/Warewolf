module LanguageAST

open DataASTMutable
open System.Diagnostics.CodeAnalysis

[<ExcludeFromCodeCoverage>]
type ScalarId = string

[<ExcludeFromCodeCoverage>]
type Index = 
    | IntIndex of int
    | Star
    | Last
    | IndexExpression of LanguageExpression

and [<ExcludeFromCodeCoverage>] RecordSetIdentifier = 
    { Name : string
      Column : string
      Index : Index }

and [<ExcludeFromCodeCoverage>] JsonIdentifier = 
    { Name : string }

and [<ExcludeFromCodeCoverage>] JsonPropertyIdentifier = 
    { ObjectName : string
      Next : JsonIdentifierExpression }

and [<ExcludeFromCodeCoverage>] BasicJsonIndexedPropertyIdentifier = 
    { ObjectName : string
      Next : JsonIdentifierExpression
      Index : Index }

and [<ExcludeFromCodeCoverage>] JsonIdentifierExpression = 
    | NameExpression of JsonIdentifier
    | NestedNameExpression of JsonPropertyIdentifier
    | IndexNestedNameExpression of BasicJsonIndexedPropertyIdentifier
    | Terminal

and [<ExcludeFromCodeCoverage>] RecordSetName = 
    { Name : string
      Index : Index }

and [<ExcludeFromCodeCoverage>] ScalarIdentifier = string

and [<ExcludeFromCodeCoverage>] LanguageExpression = 
    | RecordSetExpression of RecordSetIdentifier
    | ScalarExpression of ScalarIdentifier
    | WarewolfAtomAtomExpression of WarewolfAtom
    | ComplexExpression of LanguageExpression list
    | RecordSetNameExpression of RecordSetName
    | JsonIdentifierExpression of JsonIdentifierExpression

let tryParseIndex (x : Index) = 
    match x with
    | IntIndex a -> 
        if a <= 0 then 
            raise (new System.IndexOutOfRangeException((sprintf "Recordset index [ %i ] is not greater than zero" a)))
        else x
    | _ -> x
