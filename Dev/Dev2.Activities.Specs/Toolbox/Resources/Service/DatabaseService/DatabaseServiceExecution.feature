@ignore
Feature: DatabaseServiceExecution
	In order to use Database service 
	As a Warewolf user
	I want a tool that calls the Database services into the workflow
	As a windows authenticated user and a sql authenticated user:

@mytag
Scenario Outline: Executing Database service using numeric indexes and scalar
	     Given I have a <DatabaseType> service "mails"
	     And the output is mapped as
	     	| Name           | Email           |
	     	| <nameVariable> | <emailVariable> |
	     When the Service is executed
	     Then the execution has "<errorOccured>" error
	     And the debug output as
	     |                          |
	     | <nameVariable>  = String |
	     | <emailVariable> = String |
Examples: 
        | DatabaseType | nameVariable    | emailVariable    | errorOccured |
        | SQL          | [[rec(1).name]] | [[rec(1).email]] | NO           |
        | SQL          | [[rec(2).name]] | [[rec(2).email]] | NO           |
        | SQL          | [[rec(3).name]] | [[rec(3).email]] | NO           |
        | SQL          | [[name]]        | [[email]]]]      | NO           |
        | MySQL        | [[rec(1).name]] | [[rec(1).email]] | NO           |
        | MySQL        | [[rec(2).name]] | [[rec(2).email]] | NO           |
        | MySQL        | [[rec(3).name]] | [[rec(3).email]] | NO           |
        | MySQL        | [[name]]        | [[email]]]]      | NO           |
       
       
Scenario: Executing Database service with recordset using star.
     Given I have a <DataBaseServiceType> "mails"  
     And the output is mapped as "[[rec(*).name]]" and "[[rec(*).email]]" 
     When the Service is executed
     Then the execution has "NO" error
     And the debug output as

Examples: 
	
    |DataServiceaseType|                                               |
    |SQL               | [[rec(1).name]]  = Tshepo                     |
    |SQL               | [[rec(2).name]]  = Hags                       |
    |SQL               | [[rec(3).name]]  = Ashley                     |
    |SQL               | [[rec(1).email]] = tshepo.ntlhokoa@dev2.co.za |
    |SQL               | [[rec(2).email]] = hagashen.naidu@dev2.co.za  |
    |SQL               | [[rec(3).email]] = ashley.lewis@dev2.co.za    |
	|MySQL             | [[rec(1).name]]  = Tshepo                     |
    |MySQL             | [[rec(2).name]]  = Hags                       |
    |MySQL             | [[rec(3).name]]  = Ashley                     |
    |MySQL             | [[rec(1).email]] = tshepo.ntlhokoa@dev2.co.za |
    |MySQL             | [[rec(2).email]] = hagashen.naidu@dev2.co.za  |
    |MySQL             | [[rec(3).email]] = ashley.lewis@dev2.co.za    |
	
        
Scenario: Executing Email service with negative recordset in name.
       Given I have a <DataBaseServiceType> "mails"  
       And the output is mapped as "[[rec(-1).name]]" and "[[rec(*).email]]" 
       When the Service is executed
       Then the execution has "AN" error
       And the debug output as

      |DataServiceaseType	|                                               |
	  |SQL               	| [[rec(-1).name]]  =                           |
	  |SQL               	| [[rec(1).email]] = tshepo.ntlhokoa@dev2.co.za |
	  |SQL               	| [[rec(1).email]] = hagashen.naidu@dev2.co.za  |
	  |SQL               	| [[rec(3).email]] = ashley.lewis@dev2.co.za    |    
      |MySQL               	| [[rec(-1).name]]  =                           |
	  |MySQL               	| [[rec(1).email]] = tshepo.ntlhokoa@dev2.co.za |
	  |MySQL               	| [[rec(1).email]] = hagashen.naidu@dev2.co.za  |
	  |MySQL              	| [[rec(3).email]] = ashley.lewis@dev2.co.za    |  
	    
Scenario: Executing Email service with negative recordset in email.
      Given I have <DataBaseServiceType> "mails"  
      And the output as  "[[rec(1).name]]" and  "[[rec(-1).email]]" 
      When the Service is executed
      Then the execution has "AN" error
      And the debug output as

       |DataServiceaseType	| Name  | [[rec(1).name]]   =  Tsepho |
       |SQL               	| Email | [[rec(-1).email]] =         |
	   |MySQL               | Email | [[rec(-1).email]] =         |
       
Scenario: Executing 'country service' with valid input.
      Given I have a <DataBaseServiceType> "country" 
      And the Input as "Alb" 
	  And the output varaibles as "[[country]]" and "[[Id]]"
      When the Service is executed
      Then the execution has "NO" error
      And the debug input as
            | Alb    |
       And the debug output as

       |DataServiceaseType	|             |         |
       |SQL               	| [[country]] | Albania |
       |SQL               	| [[Id]]      | 2       |
	   |MySQL               | [[country]] | Albania |
       |MySQL               | [[Id]]      | 2       |

Scenario: Executing 'country service' with Invalid input.
        Given I have a <DataBaseServiceType> "country" 
        And the Input as "jajjj" 
		And the output varaibles as "[[country]]" and "[[Id]]"
        When the Service is executed
        Then the execution has "AN" error
        And the debug input as
       | jajjj  |
        And the debug output as

       |DataServiceaseType|             | 
       |SQL               | [[country]] | 
       |SQL				  | [[Id]]      | 
	   |MySQL             | [[country]] | 
       |MySQL			  | [[Id]]      | 

Scenario: Executing 'insertdummyuser' with input.      
       Given I have a <DataBaseServiceType> "insertdummyuser"
       And input name as "Murali"
       And input  Iname as "Naidu"
       And input username as "Murali1"
       And input password as "I can't say"
       And input lastAccessDate as "22/01/1992"
       Then the execution has "NO" error   
       And the debug input as
       |DataServiceaseType| fname  | Iname | username | password    | lastAccessDate |
       |SQL               | Murali | Naidu | Murali1  | I can't say | 22/01/1992     |                            
	   |MySQL             | Murali | Naidu | Murali1  | I can't say | 22/01/1992     |

Scenario: Executing 'Stored proceedure' with SQL defined error.      
       Given I have a <DataBaseServiceType> with Stored proceedure with SQL error
       And  the input of the proc used is invalid 
	   And the output must be  SQL error
       When the Service is executed
       Then the execution has "AN" error
       And the debug output as

Examples: 
	   |DataServiceaseType| Error  |
	   |SQL               | 1/0    |
	   |MySQL             | 1/0    |

Scenario: Use wildcards values in the Assign tool with SQL/MySQL database connector
		Given I have a <DataBaseServiceType> "country with cities table" 
		And the Input as "India" for country variable
		And the Input as [ja]% or [!ja]%
		And the output varaibles as "[[country]]", "[[cities]]" and "[[Id]]"
		When the Service is executed
		Then the execution has "NO" error
		And the debug input as
	   |DataServiceaseType	|             |																					  |
       |SQL               	| [[country]] | India																		      |
	   |SQL               	| [[city]]    | All cities that start with "j" and "a" or cities that dont start with "j" and "a" |
       |SQL               	| [[Id]]      | %Numeric%																	      |
	   |MySQL               | [[country]] | India																			  |
	   |MySQL               | [[country]] | All cities that start with "j" and "a" or cities that dont start with "j" and "a" |
       |MySQL               | [[Id]]      | %Numeric%																	      |