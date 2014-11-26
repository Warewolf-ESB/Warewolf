@ignore
Feature: DatabaseServiceExecution
	In order to use Database service 
	As a Warewolf user
	I want a tool that calls the Database services into the workflow

@mytag
Scenario Outline: Executing Database service using numeric indexes and scalar
	     Given I have a database service "mails"
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
        | nameVariable    | emailVariable    | errorOccured |
        | [[rec(1).name]] | [[rec(1).email]] | NO           |
        | [[rec(2).name]] | [[rec(2).email]] | NO           |
        | [[rec(3).name]] | [[rec(3).email]] | NO           |
        | [[name]]        | [[email]]]]      | NO           |
       
       
Scenario: Executing Database service with recordset using star.
     Given I have a DataBase service "mails"  
     And the output is mapped as "[[rec(*).name]]" and "[[rec(*).email]]" 
     When the Service is executed
     Then the execution has "NO" error
     And the debug output as
      |                                               |
      | [[rec(1).name]]  = Tshepo                     |
      | [[rec(2).name]]  = Hags                       |
      | [[rec(3).name]]  = Ashley                     |
      | [[rec(1).email]] = tshepo.ntlhokoa@dev2.co.za |
      | [[rec(2).email]] = hagashen.naidu@dev2.co.za  |
      | [[rec(3).email]] = ashley.lewis@dev2.co.za    |
       
        
Scenario: Executing Email service with negative recordset in name.
       Given I have a DataBase service "mails"  
       And the output is mapped as "[[rec(-1).name]]" and "[[rec(*).email]]" 
       When the Service is executed
       Then the execution has "AN" error
       And the debug output as
      |                                               |
      | [[rec(-1).name]]  =                           |
      | [[rec(1).email]] = tshepo.ntlhokoa@dev2.co.za |
      | [[rec(1).email]] = hagashen.naidu@dev2.co.za  |
      | [[rec(3).email]] = ashley.lewis@dev2.co.za    |    
        
Scenario: Executing Email service with negative recordset in email.
      Given I have DataBase service "mails"  
      And the output as  "[[rec(1).name]]" and  "[[rec(-1).email]]" 
      When the Service is executed
      Then the execution has "AN" error
      And the debug output as
       | Name  | [[rec(1).name]]   =  Tsepho |
       | Email | [[rec(-1).email]] =         |

       
Scenario: Executing 'country service' with valid input.
      Given I have a DataBase service "country" 
      And the Input as "Alb" 
	  And the output varaibles as "[[country]]" and "[[Id]]"
      When the Service is executed
      Then the execution has "NO" error
      And the debug input as
            | Alb    |
       And the debug output as
       |             |         |
       | [[country]] | Albania |
       | [[Id]]      | 2       |

Scenario: Executing 'country service' with Invalid input.
        Given I have a DataBase service "country" 
        And the Input as "jajjj" 
		And the output varaibles as "[[country]]" and "[[Id]]"
        When the Service is executed
        Then the execution has "AN" error
        And the debug input as
       | jajjj  |
        And the debug output as
       |             | 
       | [[country]] | 
       | [[Id]]      | 

Scenario: Executing 'insertdummyuser' with input.      
       Given I have a DataBase service "insertdummyuser"
       And input name as "Murali"
       And input  Iname as "Naidu"
       And input username as "Murali1"
       And input password as "I can't say"
       And input lastAccessDate as "22/01/1992"
       Then the execution has "NO" error   
       And the debug input as
       | fname  | Iname | username | password    | lastAccessDate |
       | Murali | Naidu | Murali1  | I can't say | 22/01/1992     |                            

