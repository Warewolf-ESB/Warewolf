module LanguageAST

open DataStorage
open System.Diagnostics.CodeAnalysis

///Sclar identifier
[<ExcludeFromCodeCoverage>]
type ScalarId = string

///Indexes are ints, floats star or another expression
[<ExcludeFromCodeCoverage>]
type Index = 
    | IntIndex of int
    | Star
    | Last
    | IndexExpression of LanguageExpression
///RecordSet column identifier [[Rec().a]]
and [<ExcludeFromCodeCoverage>] RecordSetColumnIdentifier = 
    { Name : string
      Column : string
      Index : Index }
/// Json terminal Property Identifier [[%%.JsonIdentifier]]
and [<ExcludeFromCodeCoverage>] JsonIdentifier = 
    { Name : string }
/// Object Notation [[Object.SomethingElse]]
/// Object Notation [[@Object.SomethingElse]]
and [<ExcludeFromCodeCoverage>] JsonPropertyIdentifier = 
    { ObjectName : string
      Next : JsonIdentifierExpression }
///IndexedObjectNotation [[Object(*).SomethingElse 
///IndexedObjectNotation [[@Object(*).SomethingElse 
and [<ExcludeFromCodeCoverage>] BasicJsonIndexedPropertyIdentifier = 
    { ObjectName : string
      Next : JsonIdentifierExpression
      Index : Index }
///Algebraic types that combine all the possible forms of json expression
and [<ExcludeFromCodeCoverage>] JsonIdentifierExpression = 
    | NameExpression of JsonIdentifier
    | NestedNameExpression of JsonPropertyIdentifier
    | IndexNestedNameExpression of BasicJsonIndexedPropertyIdentifier
    | Terminal
/// Recordset Identifier [[Rec()]]
and [<ExcludeFromCodeCoverage>] RecordSetName = 
    { Name : string
      Index : Index }
///Alias for string. identifies a scalar name
and [<ExcludeFromCodeCoverage>] ScalarIdentifier = string
///AlgebraicType that identifies all legal language expressions
and [<ExcludeFromCodeCoverage>] LanguageExpression = 
    | RecordSetExpression of RecordSetColumnIdentifier
    | ScalarExpression of ScalarIdentifier
    | WarewolfAtomExpression of WarewolfAtom
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
