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
