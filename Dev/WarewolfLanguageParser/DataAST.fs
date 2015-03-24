module DataAST

type Atom =
    | Float of float
    | Int of int
    | DataString of string
    | Nothing
    
type Record = Atom * int

type ColumnData = Record list * int

type ColumnHeader = string

type Recordset = Map<ColumnHeader,ColumnData>

type Environment = 
    {
       RecordSets : Map<string,Recordset>;
       Scalar : Map<string,Atom>; 
    }


