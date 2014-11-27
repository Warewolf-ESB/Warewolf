@ignore
Feature: WebServiceActivity
	In order to use WebService 
	As a Warewolf user
	I want a tool that calls the WebServices into the workflow

@mytag
Scenario: Executing WebService using recordsets
	     Given I have a WebService "country list"
		 And the input is mapped as "[[json]]" and "[[a]]"
	     And the output is mapped as "[[rec(*).CountryID]] " and "[[rec(*).Description]]"
	     When the Service is executed
	     Then the execution has "NO" error
		 And the debug input as 
		 |                       |                         |
		 | [[json]]= json        |                         |
		 | [[rec(1).CountryID]]  | [[rec(1).Description]]  |
		 | [[rec(2).CountryID]]  | [[rec(2).Description]]  |
		 | [[rec(3).CountryID]]  | [[rec(3).Description]]  |
		 | [[rec(4).CountryID]]  | [[rec(4).Description]]  |
		 | [[rec(5).CountryID]]  | [[rec(5).Description]]  |
		 | [[rec(6).CountryID]]  | [[rec(6).Description]]  |
		 | [[rec(7).CountryID]]  | [[rec(7).Description]]  |
		 | [[rec(8).CountryID]]  | [[rec(8).Description]]  |
		 | [[rec(9).CountryID]]  | [[rec(9).Description]]  |
		 | [[rec(10).CountryID]] | [[rec(10).Description]] |

	     And the debug output as
	     |                            |                                      |
	     | [[rec(1).CountryID]] = 1   | [[rec(1).Description]] = Afghanistan |
	     | [[rec(2).CountryID]] = 2   | [[rec(2).Description]] = Albania     |
	     | [[rec(3).CountryID]] = 3   | [[rec(3).Description]] = Algeria     |
	     | [[rec(4).CountryID]] = 4   | [[rec(4).Description]] = Andorra     |
	     | [[rec(5).CountryID]] = 5   | [[rec(5).Description]] = Angola      |
	     | [[rec(6).CountryID]] = 6   | [[rec(6).Description]] = Argentina   |
	     | [[rec(7).CountryID]] = 7   | [[rec(7).Description]] = Armenia     |
	     | [[rec(8).CountryID]] = 8   | [[rec(8).Description]] = Australia   |
	     | [[rec(9).CountryID]] = 9   | [[rec(9).Description]] = Austria     |
	     | [[rec(10).CountryID]] = 10 | [[rec(10).Description]] = Azerbaijan |

Scenario: Executing WebService using negative recordset index 
	     Given I have a WebService "country list"
		 And the input is mapped as "[[json]]" and "[[a]]"
	     And the output is mapped as "[[rec(-1).CountryID]] " and "[[rec(1).Description]]"
	     When the Service is executed
	     Then the execution has "AN" error
		 And the debug input as 
		 | [[json]]=             |                         |
		 | [[json]]=             |                         |
		 | [[rec(-1).CountryID]] | [[rec(1).Description]]  |
		 |                       | [[rec(2).Description]]  |
		 |                       | [[rec(3).Description]]  |
		 |                       | [[rec(4).Description]]  |
		 |                       | [[rec(5).Description]]  |
		 |                       | [[rec(6).Description]]  |
		 |                       | [[rec(7).Description]]  |
		 |                       | [[rec(8).Description]]  |
		 |                       | [[rec(9).Description]]  |
		 |                       | [[rec(10).Description]] |

	     And the debug output as
	     | [[rec(-1).CountryID]] = | [[rec(1).Description]] = Afghanistan |
	     |                         | [[rec(2).Description]] = Albania     |
	     |                         | [[rec(3).Description]] = Algeria     |
	     |                         | [[rec(4).Description]] = Andorra     |
	     |                         | [[rec(5).Description]] = Angola      |
	     |                         | [[rec(6).Description]] = Argentina   |
	     |                         | [[rec(7).Description]] = Armenia     |
	     |                         | [[rec(8).Description]] = Australia   |
	     |                         | [[rec(9).Description]] = Austria     |
	     |                         | [[rec(10).Description]] = Azerbaijan |


Scenario: Executing WebService using negative recordset
	     Given I have a WebService "country list"
		 And the input is mapped as "[[json]]" and "[[a]]"
	     And the output is mapped as "[[rec(*).CountryID]] " and "[[rec(-1).Description]]"
	     When the Service is executed
	     Then the execution has "AN" error
		 And the debug input as 
		 |                       |                         |
		 | [[json]]= json        |                         |
		 | [[rec(1).CountryID]]  | [[rec(-1).Description]] |
		 | [[rec(2).CountryID]]  |                         |
		 | [[rec(3).CountryID]]  |                         |
		 | [[rec(4).CountryID]]  |                         |
		 | [[rec(5).CountryID]]  |                         |
		 | [[rec(6).CountryID]]  |                         |
		 | [[rec(7).CountryID]]  |                         |
		 | [[rec(8).CountryID]]  |                         |
		 | [[rec(9).CountryID]]  |                         |
		 | [[rec(10).CountryID]] |                         |
	      And the debug output as
		   |                            |                           |
		   | [[rec(1).CountryID]] = 1   | [[rec(-1).Description]] = |
		   | [[rec(2).CountryID]] = 2   |                           |
		   | [[rec(3).CountryID]] = 3   |                           |
		   | [[rec(4).CountryID]] = 4   |                           |
		   | [[rec(5).CountryID]] = 5   |                           |
		   | [[rec(6).CountryID]] = 6   |                           |
		   | [[rec(7).CountryID]] = 7   |                           |
		   | [[rec(8).CountryID]] = 8   |                           |
		   | [[rec(9).CountryID]] = 9   |                           |
		   | [[rec(10).CountryID]] = 10 |                           |

Scenario: Executing WebService using scalar 
         Given I have a WebService "country list"
		 And the input is mapped as "[[json]]" and "[[a]]"
	     And the output is mapped as "[[id]] " and "[[dsc]]"
	     When the Service is executed
	     Then the execution has "NO" error     
		  And the debug input as 
		 |               |         |
		 | [[json]]=json |         |
		 | [[id]]        | [[dsc]] |
		  And the debug output as
		  |                      |  |
		  | [[id]] = 10          |  |
		  | [[dsc]] = Azerbaijan |  |

