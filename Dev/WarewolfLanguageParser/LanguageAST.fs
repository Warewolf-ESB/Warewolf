module LanguageAST
open DataAST
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
    }
and ScalarIdentifier = string
and LanguageExpression = 
    | RecordSetExpression of RecordSetIdentifier
    | ScalarExpression of ScalarIdentifier
    | AtomExpression of Atom
    | WarewolfAtomAtomExpression of WarewolfAtom
    | ComplexExpression of LanguageExpression list
    | RecordSetNameExpression of RecordSetName
