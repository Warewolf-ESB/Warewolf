module LanguageAST
open DataASTMutable

type ScalarId = string
type Index = 
    | IntIndex of int
    | Star
    | Last
    | IndexExpression of LanguageExpression
and RecordSetIdentifier = 
    {
        Name : string;
        Column :string;
        Index: Index;
    }
and JsonIdentifier = 
    {   
        Name:string
    }
and JsonPropertyIdentifier = 
    {
        ObjectName :string;
        Next: JsonIdentifierExpression;
    }
and BasicJsonIndexedPropertyIdentifier = 
    {
        ObjectName :string;
        Next: JsonIdentifierExpression;
        Index:Index;
    }
and JsonIdentifierExpression = 
    | NameExpression of JsonIdentifier
    | NestedNameExpression of JsonPropertyIdentifier
    | IndexNestedNameExpression of BasicJsonIndexedPropertyIdentifier
    | Terminal

and RecordSetName = 
    {
        Name : string;
        Index : Index;
    }
and ScalarIdentifier = string
and LanguageExpression = 
    | RecordSetExpression of RecordSetIdentifier
    | ScalarExpression of ScalarIdentifier
    | WarewolfAtomAtomExpression of WarewolfAtom
    | ComplexExpression of LanguageExpression list
    | RecordSetNameExpression of RecordSetName
    | JsonIdentifierExpression of JsonIdentifierExpression
let tryParseIndex (x:Index) =
   match x  with 
        | IntIndex a -> if a<=0 then raise (new System.IndexOutOfRangeException( (sprintf "Recordset index [ %i ] is not greater than zero" a)) )else x
        | _->x