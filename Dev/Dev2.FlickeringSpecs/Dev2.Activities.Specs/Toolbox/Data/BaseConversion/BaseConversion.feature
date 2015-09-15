Feature: BaseConversion
	In order to convert base encoding types
	As a Warewolf user
	I want a tool that converts data from one base econding to another

Scenario Outline: Convert an empty recordset * 
	Given I convert a variable "<Variable>" with a value of "<value>"
	And I convert a variable '<Variable>' from type '<From>' to type '<To>' 
	When the case conversion tool is executed
	Then the execution has "AN" error
	And the execution has "<Error>" error
Examples: 
	| No | Variable            | Value | From    | To      | Error             |
	| 1  | [[rs(*).row]]       |       | Binary  | Binary  | Invalid Recordset |
	| 2  | [[rs(*).row]]       |       | Binary  | Text    | Invalid Recordset |
	| 3  | [[rs(*).row]]       |       | Binary  | Hex     | Invalid Recordset |
	| 4  | [[rs(*).row]]       |       | Binary  | Base 64 | Invalid Recordset |
	| 5  | [[rs(*).row]]       |       | Text    | Binary  | Invalid Recordset |
	| 6  | [[rs(*).row]]       |       | Text    | Text    | Invalid Recordset |
	| 7  | [[rs(*).row]]       |       | Text    | Hex     | Invalid Recordset |
	| 8  | [[rs(*).row]]       |       | Text    | Base 64 | Invalid Recordset |
	| 9  | [[rs(*).row]]       |       | Hex     | Binary  | Invalid Recordset |
	| 10 | [[rs(*).row]]       |       | Hex     | Text    | Invalid Recordset |
	| 11 | [[rs(*).row]]       |       | Hex     | Hex     | Invalid Recordset |
	| 12 | [[rs(*).row]]       |       | Hex     | Base 64 | Invalid Recordset |
	| 13 | [[rs(*).row]]       |       | Base 64 | Binary  | Invalid Recordset |
	| 14 | [[rs(*).row]]       |       | Base 64 | Text    | Invalid Recordset |
	| 15 | [[rs(*).row]]       |       | Base 64 | Hex     | Invalid Recordset |
	| 16 | [[rs(*).row]]       |       | Base 64 | Base 64 | Invalid Recordset |
	| 17 | [[rs([[var]]).row]] |       | Binary  | Binary  | Invalid Index     |
	| 18 | [[rs([[var]]).row]] |       | Binary  | Text    | Invalid Index     |
	| 19 | [[rs([[var]]).row]] |       | Binary  | Hex     | Invalid Index     |
	| 20 | [[rs([[var]]).row]] |       | Binary  | Base 64 | Invalid Index     |
	| 21 | [[rs([[var]]).row]] |       | Text    | Binary  | Invalid Index     |
	| 22 | [[rs([[var]]).row]] |       | Text    | Text    | Invalid Index     |
	| 23 | [[rs([[var]]).row]] |       | Text    | Hex     | Invalid Index     |
	| 24 | [[rs([[var]]).row]] |       | Text    | Base 64 | Invalid Index     |
	| 25 | [[rs([[var]]).row]] |       | Hex     | Binary  | Invalid Index     |
	| 26 | [[rs([[var]]).row]] |       | Hex     | Text    | Invalid Index     |
	| 27 | [[rs([[var]]).row]] |       | Hex     | Hex     | Invalid Index     |
	| 28 | [[rs([[var]]).row]] |       | Hex     | Base 64 | Invalid Index     |
	| 29 | [[rs([[var]]).row]] |       | Base 64 | Binary  | Invalid Index     |
	| 30 | [[rs([[var]]).row]] |       | Base 64 | Text    | Invalid Index     |
	| 31 | [[rs([[var]]).row]] |       | Base 64 | Hex     | Invalid Index     |
	| 32 | [[rs([[var]]).row]] |       | Base 64 | Base 64 | Invalid Index     |
	| 33 | [[rs().row]]        |       | Binary  | Binary  | Invalid Recordset |
	| 34 | [[rs().row]]        |       | Binary  | Text    | Invalid Recordset |
	| 35 | [[rs().row]]        |       | Binary  | Hex     | Invalid Recordset |
	| 36 | [[rs().row]]        |       | Binary  | Base 64 | Invalid Recordset |
	| 37 | [[rs().row]]        |       | Text    | Binary  | Invalid Recordset |
	| 38 | [[rs().row]]        |       | Text    | Text    | Invalid Recordset |
	| 39 | [[rs().row]]        |       | Text    | Hex     | Invalid Recordset |
	| 40 | [[rs().row]]        |       | Text    | Base 64 | Invalid Recordset |
	| 41 | [[rs().row]]        |       | Hex     | Binary  | Invalid Recordset |
	| 42 | [[rs().row]]        |       | Hex     | Text    | Invalid Recordset |
	| 43 | [[rs().row]]        |       | Hex     | Hex     | Invalid Recordset |
	| 44 | [[rs().row]]        |       | Hex     | Base 64 | Invalid Recordset |
	| 45 | [[rs().row]]        |       | Base 64 | Binary  | Invalid Recordset |
	| 46 | [[rs().row]]        |       | Base 64 | Text    | Invalid Recordset |
	| 47 | [[rs().row]]        |       | Base 64 | Hex     | Invalid Recordset |
	| 48 | [[rs().row]]        |       | Base 64 | Base 64 | Invalid Recordset |
 