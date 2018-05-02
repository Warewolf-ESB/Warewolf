Feature: AdvancedRecordset
    In order to validate sql executing over a recordset
    As a Warewolf developer
    I want to be what sql is supported

Scenario:  Select all
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Bob    |
    | TableCopy().name | Alice  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(3).name]] = Hatter |

Scenario:  Select specific field
    Given I have a recordset with this shape
    | [[person]]        |        |
    | person(1).name    | Bob    |
    | person(2).name    | Alice  |
    | person(1).surname | Smith  |
    | person(2).surname | Jones  |
    | person(1).gender  | Male   |
    | person(2).gender  | Female |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT gender from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | gender      | [[TableCopy().gender]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).gender]]"  will be
    | rs                 | value  |
    | TableCopy().gender | Male   |
    | TableCopy().gender | Female |
    And the execution has "NO" error
    And the debug output as
    |                                  |
    | [[TableCopy(2).gender]] = Female |

Scenario:  Select statement multiple fields but NOT all
    Given I have a recordset with this shape
    | [[person]]        |        |
    | person(1).name    | Bob    |
    | person(2).name    | Alice  |
    | person(1).surname | Smith  |
    | person(2).surname | Jones  |
    | person(1).gender  | Male   |
    | person(2).gender  | Female |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT name, gender from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | name        | [[TableCopy().name]]   |
    | gender      | [[TableCopy().gender]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Bob   |
    | TableCopy(2).name | Alice |
    Then recordset "[[TableCopy(*).gender]]"  will be
    | rs                  | value  |
    | TableCopy(1).gender | Male   |
    | TableCopy(2).gender | Female |
    And the execution has "NO" error
    And the debug output as
    |                                  |
    | [[TableCopy(2).name]] = Alice    |
    | [[TableCopy(2).gender]] = Female |

Scenario:  Select all with Where condition
    Given I have a recordset with this shape
    | [[person]]        |        |
    | person(1).name    | Bob    |
    | person(2).name    | Alice  |
    | person(1).surname | Smith  |
    | person(2).surname | Jones  |
    | person(1).gender  | Male   |
    | person(2).gender  | Female |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT gender from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | gender      | [[TableCopy().gender]] |
    And Recordset is "TableCopy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From | Mapped To               |
    | gender      | [[newPerson().gender]]  |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).gender]]"  will be
    | rs                 | value  |
    | newPerson().gender | Male   |
    | newPerson().gender | Female |
    And the execution has "NO" error
    And the debug output as
    |                                  |
    | [[newPerson(2).gender]] = Female |

Scenario:  Select statement all fields Where condition is true
    Given I have a recordset with this shape
    | [[person]]     |       |
    | person(1).name | Bob   |
    | person(1).age  | 56    |
    | person(2).name | Alice |
    | person(2).age  | 30    |
    | person(3).name | Kim   |
    | person(3).age  | 28    |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where age = 30"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[newPerson().name]] |
    | age         | [[newPerson().age]]  |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).name]]"  will be
    | rs               | value |
    | newPerson().name | Alice |
    Then recordset "[[newPerson(*).age]]"  will be
    | rs              | value |
    | newPerson().age | 30    |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[newPerson(1).name]] = Alice |
    | [[newPerson(1).age]] = 30     |

Scenario:  Select all with Where condition as a Scalar
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | checkName     | Alice  |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value         |
    | filName | [[checkName]] |
    And I have the following sql statement "SELECT * from person where name = @filName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Alice  |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | filName               |
    | String | [[checkName]] = Alice |
    And the debug output as
    |                               |
    | [[TableCopy(1).name]] = Alice |

Scenario:  Select specific field With Where condition
    Given I have a recordset with this shape
    | [[person]]       |        |
    | person(1).name   | Bob    |
    | person(1).age    | 56     |
    | person(2).name   | Alice  |
    | person(2).age    | 30     |
    | person(3).name   | Kim    |
    | person(3).age    | 28     |
    | person(1).gender | Male   |
    | person(2).gender | Female |
    | person(3).gender | Female |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT name from person where age = 30"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value |
    | TableCopy().name | Alice |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(1).name]] = Alice |

Scenario:  Select specific field with Where condition as Scalar
    Given I have a recordset with this shape
    | [[person]]       |        |
    | person(1).name   | Bob    |
    | person(1).age    | 56     |
    | person(2).name   | Alice  |
    | person(2).age    | 30     |
    | person(3).name   | Kim    |
    | person(3).age    | 28     |
    | person(1).gender | Male   |
    | person(2).gender | Female |
    | person(3).gender | Female |
    | checkAge         | 56     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name   | Value        |
    | filAge | [[checkAge]] |
    And I have the following sql statement "SELECT gender from person where age = @filAge"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | gender      | [[TableCopy().gender]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).gender]]"  will be
    | rs                 | value |
    | TableCopy().gender | Male  |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(1).gender]] = Male|

Scenario:  Select multiple fields with Where condition
    Given I have a recordset with this shape
    | [[person]]       |        |
    | person(1).name   | Bob    |
    | person(1).age    | 56     |
    | person(2).name   | Alice  |
    | person(2).age    | 30     |
    | person(3).name   | Kim    |
    | person(3).age    | 28     |
    | person(1).gender | Male   |
    | person(2).gender | Female |
    | person(3).gender | Female |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT name, gender from person where age = 56"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | name        | [[TableCopy().name]]   |
    | gender      | [[TableCopy().gender]] |
    And Recordset is "TableCopy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From | Mapped To              |
    | name        | [[newPerson().name]]   |
    | gender      | [[newPerson().gender]] |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).name]]"  will be
    | rs               | value |
    | newPerson().name | Bob   |
    Then recordset "[[newPerson(*).gender]]"  will be
    | rs                 | value |
    | newPerson().gender | Male  |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[newPerson(1).name]] = Bob    |
    | [[newPerson(1).gender]] = Male |

Scenario:  Select multiple fields with Where condition as a Scalar
    Given I have a recordset with this shape
    | [[person]]       |        |
    | person(1).name   | Bob    |
    | person(1).age    | 56     |
    | person(2).name   | Alice  |
    | person(2).age    | 30     |
    | person(3).name   | Kim    |
    | person(3).age    | 28     |
    | person(1).gender | Male   |
    | person(2).gender | Female |
    | person(3).gender | Female |
    | checkAge         | 28     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name   | Value        |
    | filAge | [[checkAge]] |
    And I have the following sql statement "SELECT name, gender from person where age = @filAge"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | name        | [[TableCopy().name]]   |
    | gender      | [[TableCopy().gender]] |
    And Recordset is "TableCopy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From | Mapped To              |
    | name        | [[newPerson().name]]   |
    | gender      | [[newPerson().gender]] |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).name]]"  will be
    | rs               | value |
    | newPerson().name | Kim   |
    Then recordset "[[newPerson(*).gender]]"  will be
    | rs                 | value  |
    | newPerson().gender | Female |
    And the execution has "NO" error
    And the debug output as
    |                                  |
    | [[newPerson(1).name]] = Kim      |
    | [[newPerson(1).gender]] = Female |

Scenario:  Select all With Condition Is not equal
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where age != 19"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value |
    | TableCopy().name | Bob   |
    | TableCopy().name | Alice |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs              | value |
    | TableCopy().age | 25    |
    | TableCopy().age | 31    |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                               |
    | [[TableCopy(2).name]] = Alice |
    | [[TableCopy(2).age]] = 31     |

Scenario:  Select all With Condition Is not equal in a Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    | checkAge       | 19     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name   | Value        |
    | filAge | [[checkAge]] |
    And I have the following sql statement "SELECT * from person where age != @filAge"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value |
    | TableCopy().name | Bob   |
    | TableCopy().name | Alice |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs              | value |
    | TableCopy().age | 25    |
    | TableCopy().age | 31    |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(2).name]] = Alice |
    | [[TableCopy(2).age]] = 31     |

Scenario:  Select specific field With different field Condition Is not equal
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT name from person where age != 19"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value |
    | TableCopy().name | Bob   |
    | TableCopy().name | Alice |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                               |
    | [[TableCopy(2).name]] = Alice |

Scenario:  Select specific field With different field Condition Is not equal in a Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    | checkAge       | 19     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name   | Value        |
    | filAge | [[checkAge]] |
    And I have the following sql statement "SELECT name from person where age != @filAge"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value |
    | TableCopy().name | Bob   |
    | TableCopy().name | Alice |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(2).name]] = Alice |

Scenario:  Select specific field Filetered by a different field
    Given I have a recordset with this shape
    | [[person]]       |        |
    | person(1).name   | Bob    |
    | person(2).name   | Alice  |
    | person(3).name   | Kim    |
    | person(1).age    | 56     |
    | person(2).age    | 30     |
    | person(3).age    | 28     |
    | person(1).gender | male   |
    | person(2).gender | female |
    | person(3).gender | female |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT age from person where name = 'Kim'"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To           |
    | age         | [[TableCopy().age]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 28    |
    And the execution has "NO" error
    And the debug output as
    |                           |
    | [[TableCopy(1).age]] = 28 |

Scenario:  Select specific field Filetered by a different field as a Scalar
    Given I have a recordset with this shape
    | [[person]]       |        |
    | person(1).name   | Bob    |
    | person(2).name   | Alice  |
    | person(3).name   | Kim    |
    | person(1).age    | 56     |
    | person(2).age    | 30     |
    | person(3).age    | 28     |
    | person(1).gender | male   |
    | person(2).gender | female |
    | person(3).gender | female |
    | checkName        | Kim    |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value         |
    | filName | [[checkName]] |
    And I have the following sql statement "SELECT age from person where name = @filName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To           |
    | age         | [[TableCopy().age]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 28    |
    And the execution has "NO" error
    And the debug output as
    |                           |
    | [[TableCopy(1).age]] = 28 |

Scenario:  Select all with like clause
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where name like '%tt%'"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |

Scenario:  Select all with like clause in a Scalar
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | checkName     | %tt%   |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value         |
    | filName | [[checkName]] |
    And I have the following sql statement "SELECT * from person where name like @filName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |

Scenario:  Select all with begin with clause
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where name like 'B%'"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value |
    | TableCopy().name | Bob   |
    And the execution has "NO" error
    And the debug output as
    |                             |
    | [[TableCopy(1).name]] = Bob |

Scenario:  Select all with begin with clause in a Scalar
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | checkName     | B%     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value         |
    | filName | [[checkName]] |
    And I have the following sql statement "SELECT * from person where name like @filName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value |
    | TableCopy().name | Bob   |
    And the execution has "NO" error
    And the debug output as
    |                             |
    | [[TableCopy(1).name]] = Bob |

Scenario:  Select all with end with clause
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where name like '%r'"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |

Scenario:  Select all with end with clause in a Scalar
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | checkName     | %r     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value         |
    | filName | [[checkName]] |
    And I have the following sql statement "SELECT * from person where name like @filName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |

Scenario:  Select all with NOT like clause
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where name not like '%tt%'"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Bob   |
    | TableCopy(2).name | Alice |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(2).name]] = Alice |

Scenario:  Select all with NOT like clause in a Scalar
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | checkName     | %tt%   |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value         |
    | filName | [[checkName]] |
    And I have the following sql statement "SELECT * from person where name not like @filName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value |
    | TableCopy().name | Bob   |
    | TableCopy().name | Alice |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(2).name]] = Alice |

Scenario:  Select all with NOT begin with clause
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Mandy  |
    | person().name | Hatter |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where name not like 'B%'"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Mandy  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(2).name]] = Hatter |

Scenario:  Select all with NOT beggin with clause in a Scalar
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Mandy  |
    | person().name | Hatter |
    | checkName     | B%     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value         |
    | filName | [[checkName]] |
    And I have the following sql statement "SELECT * from person where name not like @filName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Mandy  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(2).name]] = Hatter |

Scenario:  Select all with NOT end with clause
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where name not like '%e'"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Bob    |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(2).name]] = Hatter |

Scenario:  Select all with NOT end with clause in a Scalar
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | checkName     | %e     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value         |
    | filName | [[checkName]] |
    And I have the following sql statement "SELECT * from person where name not like @filName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Bob    |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(2).name]] = Hatter |

Scenario:  Select all With Condition Is Not IN Range
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where age not in (25, 31)"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    When Advanced Recordset tool is executed
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |
    | [[TableCopy(1).age]] = 19      |

    Scenario:  Select all With Condition Is Not IN Range With Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    | Range          | 25, 31 |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value     |
    | filRange | [[Range]] |
    And I have the following sql statement "SELECT * from person where age not in (@filRange)"
    When I click Generate Outputs
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    When Advanced Recordset tool is executed
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |
    | [[TableCopy(1).age]] = 19      |

Scenario:  Select all With Condition Is IN Range
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where age in (25, 31)"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    When Advanced Recordset tool is executed
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                               |
    | [[TableCopy(2).name]] = Alice |
    | [[TableCopy(2).age]] = 31     |

Scenario:  Select all With Condition Is IN Range With Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    | Range          | 25, 31 |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value     |
    | filRange | [[Range]] |
    And I have the following sql statement "SELECT * from person where age in (@filRange)"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Bob   |
    | TableCopy(2).name | Alice |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 25    |
    | TableCopy(2).age | 31    |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | filRange           |
    | String | [[Range]] = 25, 31 |
    And the debug output as
    |                               |
    | [[TableCopy(2).name]] = Alice |
    | [[TableCopy(2).age]] = 31     |

Scenario:  Select all with Inner select clause
    Given I have a recordset with this shape
    | [[person]]           |           |
    | person(1).name       | Bob       |
    | person(2).name       | Alice     |
    | person(3).name       | Kim       |
    | person(1).age        | 56        |
    | person(2).age        | 30        |
    | person(3).age        | 28        |
    | employee(1).name     | Bob       |
    | employee(2).name     | Alice     |
    | employee(3).name     | Kim       |
    | employee(1).Position | Manager   |
    | employee(2).Position | Admin     |
    | employee(3).Position | Developer |
    | employee(1).Salary   | 90000     |
    | employee(2).Salary   | 2000      |
    | employee(3).Salary   | 35000     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where name = (Select name from employee where salary = (select max(salary) as maxAge from employee))"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Bob   |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 56    |
    And the execution has "NO" error
    And the debug output as
    |                             |
    | [[TableCopy(1).name]] = Bob |
    | [[TableCopy(1).age]] = 56   |

Scenario:  Select all With AND Condition
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 56     |
    | person(2).age  | 31     |
    | person(3).age  | 25     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where name = 'Alice' And age = 31"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Alice |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 31    |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                               |
    | [[TableCopy(1).name]] = Alice |
    | [[TableCopy(1).age]] = 31     |

Scenario:  Select all With AND Condition in a Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 25     |
    | checkName      | Alice  |
    | checkAge       | 31     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value         |
    | filName | [[checkName]] |
    | filAge  | [[checkAge]]  |
    And I have the following sql statement "SELECT * from person where age = @filAge And name = @filName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Alice |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 31    |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(1).name]] = Alice |
    | [[TableCopy(1).age]] = 31     |

Scenario:  Select all With OR Condition on different fields
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 25     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where name = 'Alice' OR age = 25"
    When I click Generate Outputs
    And Recordset is "TableCopy"
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Bob    |
    | TableCopy(2).name | Alice  |
    | TableCopy(2).name | Hatter |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 25    |
    | TableCopy(2).age | 31    |
    | TableCopy(3).age | 25    |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(3).name]] = Hatter |
    | [[TableCopy(3).age]] = 25      |

Scenario:  Select all With OR Condition on Different fields in a Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 25     |
    | checkName      | Alice  |
    | checkAge       | 25     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value         |
    | filName1 | [[checkName]] |
    | filAge   | [[checkAge]]  |
    And I have the following sql statement "SELECT * from person where name = @filName1 OR age = @filAge"
    When I click Generate Outputs
    Then The declared Variables are
    | VariableName |
    | filName1     |
    | filAge       |
    And Recordset is "TableCopy"
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Bob    |
    | TableCopy(2).name | Alice  |
    | TableCopy(3).name | Hatter |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 25    |
    | TableCopy(2).age | 31    |
    | TableCopy(3).age | 25    |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(3).name]] = Hatter |
    | [[TableCopy(3).age]] = 25      |

Scenario:  Select all With OR Condition
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 25     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where name = 'Alice' OR name = 'Bob'"
    When I click Generate Outputs
    And Recordset is "TableCopy"
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Bob   |
    | TableCopy(2).name | Alice |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 25    |
    | TableCopy(2).age | 31    |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(2).name]] = Alice |
    | [[TableCopy(2).age]] = 31     |

Scenario:  Select all With OR Condition in a Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 25     |
    | checkName      | Alice  |
    | checkName2     | Bob    |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value          |
    | filName1 | [[checkName]]  |
    | filName2 | [[checkName2]] |
    And I have the following sql statement "SELECT * from person where name = @filName1 OR name = @filName2"
    When I click Generate Outputs
    Then The declared Variables are
    | VariableName |
    | filName1     |
    | filName2     |
    And Recordset is "TableCopy"
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Bob   |
    | TableCopy(2).name | Alice |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 25    |
    | TableCopy(2).age | 31    |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(2).name]] = Alice |
    | [[TableCopy(2).age]] = 31     |

Scenario:  Select all with Alias and variable in where clause
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | checkName     | Alice  |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value         |
    | filName | [[checkName]] |
    And I have the following sql statement "SELECT * from person p where p.name = @filName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[newPerson().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).name]]"  will be
    | rs               | value  |
    | newPerson().name | Alice  |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | filName               |
    | String | [[checkName]] = Alice |
    And the debug output as
    |                               |
    | [[newPerson(1).name]] = Alice |

Scenario:  Select all with Alias for function
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | checkName     | Alice  |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT count(*) as numberOfPeople from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From    | Mapped To                      |
    | numberOfPeople | [[TableCopy().numberOfPeople]] |
    And Recordset is "TableCopy"
    When I update Recordset to ""
    Then Recordset is ""
    And Outputs are as follows
    | Mapped From    | Mapped To          |
    | numberOfPeople | [[numberOfPeople]] |
    When Advanced Recordset tool is executed
    Then the result variable "[[numberOfPeople]]" will be "3"
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                        |
    | [[numberOfPeople]] = 3 |

Scenario:  Select all with Alias non join
    Given I have a recordset with this shape
    | [[person]]      |            |
    | person(1).name  | Bob        |
    | person(2).name  | Alice      |
    | person(3).name  | Hatter     |
    | child(1).name   | Builder    |
    | child(1).parent | Bob        |
    | child(2).name   | Wonderland |
    | child(2).parent | Alice      |
    | child(3).name   | Mad        |
    | child(3).parent | Hatter     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "Select p.name as ParentName, c.name as ChildName from person AS p, child AS c WHERE p.Name = c.parent;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                  |
    | ParentName  | [[TableCopy().ParentName]] |
    | ChildName   | [[TableCopy().ChildName]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).ParentName]]"  will be
    | rs                     | value  |
    | TableCopy().ParentName | Bob    |
    | TableCopy().ParentName | Alice  |
    | TableCopy().ParentName | Hatter |
    Then recordset "[[TableCopy(*).ChildName]]"  will be
    | rs                    | value      |
    | TableCopy().ChildName | Builder    |
    | TableCopy().ChildName | Wonderland |
    | TableCopy().ChildName | Mad        |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                      |
    | [[TableCopy(3).ParentName]] = Hatter |
    | [[TableCopy(3).ChildName]] = Mad     |

Scenario:  Select all With Condition Age Is Greater Than
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where age > 30"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(2).name | Alice |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(2).age | 31    |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(1).name]] = Alice |
    | [[TableCopy(1).age]] = 31     |

Scenario:  Select all With Condition Age Is Greater Than in a Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    | checkAge       | 30     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name  | Value        |
    | inAge | [[checkAge]] |
    And I have the following sql statement "SELECT * from person where age > @inAge"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(2).name | Alice |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(2).age | 31    |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(1).name]] = Alice |
    | [[TableCopy(1).age]] = 31     |

Scenario:  Select all With Condition Age Is Less Than
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where age < 25"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |
    | [[TableCopy(1).age]] = 19      |

Scenario:  Select all With Condition Age Is Less Than in a Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    | checkAge       | 25     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name  | Value        |
    | inAge | [[checkAge]] |
    And I have the following sql statement "SELECT * from person where age < @inAge"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |
    | [[TableCopy(1).age]] = 19      |

Scenario:  Select all With Condition Age Is Between
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT name from person where age between 20 and 30"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Bob   |
    And the execution has "NO" error
    And the debug output as
    |                             |
    | [[TableCopy(1).name]] = Bob |

Scenario:  Select all With Condition Age Is Between in a Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    | minAge         | 20     |
    | maxAge         | 30     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value      |
    | inMinAge | [[minAge]] |
    | inMaxAge | [[maxAge]] |
    And I have the following sql statement "SELECT name from person where age between @inMinAge and @inMaxAge"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Bob   |
    And the execution has "NO" error
    And the debug output as
    |                             |
    | [[TableCopy(1).name]] = Bob |

Scenario:  Select all With Condition Age Is NOT IN Range
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where age NOT in (25,31)"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(3).name | Hatter |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(3).age | 19    |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |
    | [[TableCopy(1).age]] = 19      |

Scenario:  Select all With Condition Age Is NOT IN Range in a Scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    | minAge         | 25     |
    | maxAge         | 31     |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value      |
    | inMinAge | [[minAge]] |
    | inMaxAge | [[maxAge]] |
    And I have the following sql statement "SELECT * from person where age NOT in (@inMinAge, @inMaxAge)"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(3).name | Hatter |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(3).age | 19    |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |
    | [[TableCopy(1).age]] = 19      |

Scenario: Select all with Orderby Age asc
    Given I have a recordset with this shape
    | [[person]]           |           |
    | person(1).name       | Bob       |
    | person(2).name       | Alice     |
    | person(3).name       | Kim       |
    | person(1).age        | 56        |
    | person(2).age        | 30        |
    | person(3).age        | 28        |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person order by age"
    When I click Generate Outputs
    Then recordset "[[person(*).name]]"  will be
    | rs             | value |
    | person(1).name | Bob   |
    | person(2).name | Alice |
    | person(3).name | Kim   |
    Then recordset "[[person(*).age]]"  will be
    | rs            | value |
    | person(1).age | 56    |
    | person(2).age | 30    |
    | person(3).age | 28    |
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Kim   |
    | TableCopy(2).name | Alice |
    | TableCopy(3).name | Bob   |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 28    |
    | TableCopy(2).age | 30    |
    | TableCopy(3).age | 56    |
    And the execution has "NO" error
    And the debug output as
    |                             |
    | [[TableCopy(3).name]] = Bob |
    | [[TableCopy(3).age]] = 56   |

Scenario: Select all with Orderby Age desc
    Given I have a recordset with this shape
    | [[person]]     |       |
    | person(1).name | Alice |
    | person(2).name | Bob   |
    | person(3).name | Kim   |
    | person(1).age  | 30    |
    | person(2).age  | 56    |
    | person(3).age  | 28    |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person order by age desc"
    When I click Generate Outputs
    Then recordset "[[person(*).name]]"  will be
    | rs             | value |
    | person(1).name | Alice |
    | person(2).name | Bob   |
    | person(3).name | Kim   |
    Then recordset "[[person(*).age]]"  will be
    | rs            | value |
    | person(1).age | 30    |
    | person(2).age | 56    |
    | person(3).age | 28    |
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Bob   |
    | TableCopy(2).name | Alice |
    | TableCopy(3).name | Kim   |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 56    |
    | TableCopy(2).age | 30    |
    | TableCopy(3).age | 28    |
    And the execution has "NO" error
    And the debug output as
    |                             |
    | [[TableCopy(3).name]] = Kim |
    | [[TableCopy(3).age]] = 28   |

    Scenario: Select all with Orderby Age then by age desc and salary asc
    Given I have a recordset with this shape
    | [[person]]       |      |
    | person(1).name   | A    |
    | person(2).name   | B    |
    | person(3).name   | C    |
    | person(4).name   | D    |
    | person(5).name   | E    |
    | person(6).name   | F    |
    | person(7).name   | G    |
    | person(8).name   | H    |
    | person(9).name   | I    |
    | person(1).age    | 56   |
    | person(2).age    | 30   |
    | person(3).age    | 28   |
    | person(4).age    | 56   |
    | person(5).age    | 30   |
    | person(6).age    | 56   |
    | person(7).age    | 28   |
    | person(8).age    | 28   |
    | person(9).age    | 29   |
    | person(1).salary | 4000 |
    | person(2).salary | 4000 |
    | person(3).salary | 3500 |
    | person(4).salary | 3500 |
    | person(5).salary | 2000 |
    | person(6).salary | 5500 |
    | person(7).salary | 1500 |
    | person(8).salary | 7500 |
    | person(9).salary | 3500 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person order by age desc, salary asc"
    When I click Generate Outputs
    Then recordset "[[person(*).age]]"  will be
    | rs            | value |
    | person(1).age | 56    |
    | person(2).age | 30    |
    | person(3).age | 28    |
    | person(4).age | 56    |
    | person(5).age | 30    |
    | person(6).age | 56    |
    | person(7).age | 28    |
    | person(8).age | 28    |
    | person(9).age | 29    |
    Then recordset "[[person(*).salary]]"  will be
    | rs               | value |
    | person(1).salary | 4000  |
    | person(2).salary | 4000  |
    | person(3).salary | 3500  |
    | person(4).salary | 3500  |
    | person(5).salary | 2000  |
    | person(6).salary | 5500  |
    | person(7).salary | 1500  |
    | person(8).salary | 7500  |
    | person(9).salary | 3500  |
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | name        | [[TableCopy().name]]   |
    | age         | [[TableCopy().age]]    |
    | salary      | [[TableCopy().salary]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value |
    | TableCopy(1).name | D    |
    | TableCopy(2).name | A    |
    | TableCopy(3).name | F    |
    | TableCopy(4).name | E    |
    | TableCopy(5).name | B    |
    | TableCopy(6).name | I    |
    | TableCopy(7).name | G    |
    | TableCopy(8).name | C    |
    | TableCopy(9).name | H    |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 56    |
    | TableCopy(2).age | 56    |
    | TableCopy(3).age | 56    |
    | TableCopy(4).age | 30    |
    | TableCopy(5).age | 30    |
    | TableCopy(6).age | 29    |
    | TableCopy(7).age | 28    |
    | TableCopy(8).age | 28    |
    | TableCopy(9).age | 28    |
    Then recordset "[[TableCopy(*).salary]]"  will be
    | rs                  | value |
    | TableCopy(1).salary | 3500  |
    | TableCopy(2).salary | 4000  |
    | TableCopy(3).salary | 5500  |
    | TableCopy(4).salary | 2000  |
    | TableCopy(5).salary | 4000  |
    | TableCopy(6).salary | 3500  |
    | TableCopy(7).salary | 1500  |
    | TableCopy(8).salary | 3500  |
    | TableCopy(9).salary | 7500  |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(9).name]] = H      |
    | [[TableCopy(9).age]] = 28      |
    | [[TableCopy(9).salary]] = 7500 |

Scenario: Select all with Orderby Age then by salary asc
    Given I have a recordset with this shape
    | [[person]]       |      |
    | person(1).age    | 56   |
    | person(2).age    | 30   |
    | person(3).age    | 28   |
    | person(4).age    | 56   |
    | person(5).age    | 30   |
    | person(6).age    | 56   |
    | person(7).age    | 28   |
    | person(8).age    | 28   |
    | person(9).age    | 29   |
    | person(1).salary | 4000 |
    | person(2).salary | 4000 |
    | person(3).salary | 3500 |
    | person(4).salary | 3500 |
    | person(5).salary | 2000 |
    | person(6).salary | 5500 |
    | person(7).salary | 1500 |
    | person(8).salary | 7500 |
    | person(9).salary | 3500 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person order by age, salary"
    When I click Generate Outputs
    Then recordset "[[person(*).age]]"  will be
    | rs            | value |
    | person(1).age | 56    |
    | person(2).age | 30    |
    | person(3).age | 28    |
    | person(4).age | 56    |
    | person(5).age | 30    |
    | person(6).age | 56    |
    | person(7).age | 28    |
    | person(8).age | 28    |
    | person(9).age | 29    |
    Then recordset "[[person(*).salary]]"  will be
    | rs               | value |
    | person(1).salary | 4000  |
    | person(2).salary | 4000  |
    | person(3).salary | 3500  |
    | person(4).salary | 3500  |
    | person(5).salary | 2000  |
    | person(6).salary | 5500  |
    | person(7).salary | 1500  |
    | person(8).salary | 7500  |
    | person(9).salary | 3500  |
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | age         | [[TableCopy().age]]    |
    | salary      | [[TableCopy().salary]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 28    |
    | TableCopy(2).age | 28    |
    | TableCopy(3).age | 28    |
    | TableCopy(4).age | 29    |
    | TableCopy(5).age | 30    |
    | TableCopy(6).age | 30    |
    | TableCopy(7).age | 56    |
    | TableCopy(8).age | 56    |
    | TableCopy(9).age | 56    |
    Then recordset "[[TableCopy(*).salary]]"  will be
    | rs                  | value |
    | TableCopy(1).salary | 1500  |
    | TableCopy(2).salary | 3500  |
    | TableCopy(3).salary | 7500  |
    | TableCopy(4).salary | 3500  |
    | TableCopy(5).salary | 2000  |
    | TableCopy(6).salary | 4000  |
    | TableCopy(7).salary | 3500  |
    | TableCopy(8).salary | 4000  |
    | TableCopy(9).salary | 5500  |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(9).age]] = 56      |
    | [[TableCopy(9).salary]] = 5500 |

Scenario: Select all with Orderby Age then by salary desc
    Given I have a recordset with this shape
    | [[person]]       |      |
    | person(1).age    | 56   |
    | person(2).age    | 30   |
    | person(3).age    | 28   |
    | person(4).age    | 56   |
    | person(5).age    | 30   |
    | person(6).age    | 56   |
    | person(7).age    | 28   |
    | person(8).age    | 28   |
    | person(9).age    | 29   |
    | person(1).salary | 4000 |
    | person(2).salary | 4000 |
    | person(3).salary | 3500 |
    | person(4).salary | 3500 |
    | person(5).salary | 2000 |
    | person(6).salary | 5500 |
    | person(7).salary | 1500 |
    | person(8).salary | 7500 |
    | person(9).salary | 3500 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person order by age desc, salary desc"
    When I click Generate Outputs
    Then recordset "[[person(*).age]]"  will be
    | rs            | value |
    | person(1).age | 56    |
    | person(2).age | 30    |
    | person(3).age | 28    |
    | person(4).age | 56    |
    | person(5).age | 30    |
    | person(6).age | 56    |
    | person(7).age | 28    |
    | person(8).age | 28    |
    | person(9).age | 29    |
    Then recordset "[[person(*).salary]]"  will be
    | rs               | value |
    | person(1).salary | 4000  |
    | person(2).salary | 4000  |
    | person(3).salary | 3500  |
    | person(4).salary | 3500  |
    | person(5).salary | 2000  |
    | person(6).salary | 5500  |
    | person(7).salary | 1500  |
    | person(8).salary | 7500  |
    | person(9).salary | 3500  |
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | age         | [[TableCopy().age]]    |
    | salary      | [[TableCopy().salary]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 56    |
    | TableCopy(2).age | 56    |
    | TableCopy(3).age | 56    |
    | TableCopy(4).age | 30    |
    | TableCopy(5).age | 30    |
    | TableCopy(6).age | 29    |
    | TableCopy(7).age | 28    |
    | TableCopy(8).age | 28    |
    | TableCopy(9).age | 28    |
    Then recordset "[[TableCopy(*).salary]]"  will be
    | rs                  | value |
    | TableCopy(1).salary | 5500  |
    | TableCopy(2).salary | 4000  |
    | TableCopy(3).salary | 3500  |
    | TableCopy(4).salary | 4000  |
    | TableCopy(5).salary | 2000  |
    | TableCopy(6).salary | 3500  |
    | TableCopy(7).salary | 7500  |
    | TableCopy(8).salary | 3500  |
    | TableCopy(9).salary | 1500  |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(9).age]] = 28      |
    | [[TableCopy(9).salary]] = 1500 |

Scenario: Select Ascending Ordered Distinct Age Asc
    Given I have a recordset with this shape
    | [[person]]    |    |
    | person(1).age | 32 |
    | person(2).age | 28 |
    | person(3).age | 30 |
    | person(4).age | 56 |
    | person(5).age | 30 |
    | person(6).age | 28 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT Distinct(age) as UniqueAge from person order by UniqueAge asc"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                 |
    | UniqueAge   | [[TableCopy().UniqueAge]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).UniqueAge]]"  will be
    | rs                     | value |
    | TableCopy(1).UniqueAge | 28    |
    | TableCopy(2).UniqueAge | 30    |
    | TableCopy(3).UniqueAge | 32    |
    | TableCopy(4).UniqueAge | 56    |
    And the execution has "NO" error
    And the debug output as
    |                                 |
    | [[TableCopy(4).UniqueAge]] = 56 |

    Scenario: Select Distinct Age
    Given I have a recordset with this shape
    | [[person]]    |    |
    | person(1).age | 32 |
    | person(2).age | 28 |
    | person(3).age | 30 |
    | person(4).age | 56 |
    | person(5).age | 30 |
    | person(6).age | 28 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT Distinct(age) from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 32    |
    | TableCopy(2).age | 28    |
    | TableCopy(3).age | 30    |
    | TableCopy(4).age | 56    |
    And the execution has "NO" error
    And the debug output as
    |                             |
    | [[TableCopy(4).age]] = 56   |

Scenario: Select Ascending Ordered Distinct Age Desc
    Given I have a recordset with this shape
    | [[person]]    |    |
    | person(1).age | 32 |
    | person(2).age | 28 |
    | person(3).age | 30 |
    | person(4).age | 56 |
    | person(5).age | 30 |
    | person(6).age | 28 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT Distinct(age) as UniqueAge from person order by UniqueAge desc"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                 |
    | UniqueAge   | [[TableCopy().UniqueAge]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).UniqueAge]]"  will be
    | rs                     | value |
    | TableCopy(1).UniqueAge | 56    |
    | TableCopy(2).UniqueAge | 32    |
    | TableCopy(3).UniqueAge | 30    |
    | TableCopy(4).UniqueAge | 28    |
    And the execution has "NO" error
    And the debug output as
    |                                 |
    | [[TableCopy(4).UniqueAge]] = 28 |

Scenario: Select Multiple Distinct Fields Given 2 People with same name with different Jobs
    Given I have a recordset with this shape
    | [[person]]        |           |
    | person(1).name    | Bob       |
    | person(2).name    | Alice     |
    | person(3).name    | Alice     |
    | person(4).name    | Garry     |
    | person(1).surname | Smith     |
    | person(2).surname | Jacobs    |
    | person(3).surname | Jones     |
    | person(4).surname | Smith     |
    | person(1).job     | Developer |
    | person(2).job     | Developer |
    | person(3).job     | Manager   |
    | person(4).job     | Admin     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT Distinct name as UniqueName, job as UniqueJob from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                  |
    | UniqueName  | [[TableCopy().UniqueName]] |
    | UniqueJob   | [[TableCopy().UniqueJob]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).UniqueName]]"  will be
    | rs                      | value |
    | TableCopy(1).UniqueName | Bob   |
    | TableCopy(2).UniqueName | Alice |
    | TableCopy(3).UniqueName | Alice |
    | TableCopy(4).UniqueName | Garry |
    And the execution has "NO" error
    And the debug output as
    |                                     |
    | [[TableCopy(4).UniqueName]] = Garry |
    | [[TableCopy(4).UniqueJob]] = Admin  |

Scenario: Select Multiple Distinct Fields Given 2 People with same name with same Jobs
    Given I have a recordset with this shape
    | [[person]]        |           |
    | person(1).name    | Bob       |
    | person(2).name    | Alice     |
    | person(3).name    | Alice     |
    | person(4).name    | Garry     |
    | person(1).surname | Smith     |
    | person(2).surname | Jacobs    |
    | person(3).surname | Jones     |
    | person(4).surname | Smith     |
    | person(1).job     | Manager   |
    | person(2).job     | Developer |
    | person(3).job     | Developer |
    | person(4).job     | Admin     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT Distinct name as UniqueName, job as UniqueJob from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                  |
    | UniqueName  | [[TableCopy().UniqueName]] |
    | UniqueJob   | [[TableCopy().UniqueJob]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).UniqueName]]"  will be
    | rs                      | value |
    | TableCopy(1).UniqueName | Bob   |
    | TableCopy(2).UniqueName | Alice |
    | TableCopy(3).UniqueName | Garry |
    Then recordset "[[TableCopy(*).UniqueJob]]"  will be
    | rs                     | value     |
    | TableCopy(1).UniqueJob | Manager   |
    | TableCopy(2).UniqueJob | Developer |
    | TableCopy(3).UniqueJob | Admin     |
    And the execution has "NO" error
    And the debug output as
    |                                     |
    | [[TableCopy(3).UniqueName]] = Garry |
    | [[TableCopy(3).UniqueJob]] = Admin  |

Scenario: Select all with fields but Limit to 5 rows
    Given I have a recordset with this shape
    | [[person]]       |      |
    | person(1).name   | A    |
    | person(2).name   | B    |
    | person(3).name   | C    |
    | person(4).name   | D    |
    | person(5).name   | E    |
    | person(6).name   | F    |
    | person(7).name   | G    |
    | person(8).name   | H    |
    | person(9).name   | I    |
    | person(1).age    | 56   |
    | person(2).age    | 30   |
    | person(3).age    | 28   |
    | person(4).age    | 56   |
    | person(5).age    | 30   |
    | person(6).age    | 56   |
    | person(7).age    | 28   |
    | person(8).age    | 28   |
    | person(9).age    | 29   |
    | person(1).salary | 4000 |
    | person(2).salary | 4000 |
    | person(3).salary | 3500 |
    | person(4).salary | 3500 |
    | person(5).salary | 2000 |
    | person(6).salary | 5500 |
    | person(7).salary | 1500 |
    | person(8).salary | 7500 |
    | person(9).salary | 3500 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person limit 5"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | name        | [[TableCopy().name]]   |
    | age         | [[TableCopy().age]]    |
    | salary      | [[TableCopy().salary]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | A     |
    | TableCopy(2).name | B     |
    | TableCopy(3).name | C     |
    | TableCopy(4).name | D     |
    | TableCopy(5).name | E     |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 56    |
    | TableCopy(2).age | 30    |
    | TableCopy(3).age | 28    |
    | TableCopy(4).age | 56    |
    | TableCopy(5).age | 30    |
    Then recordset "[[TableCopy(*).salary]]"  will be
    | rs                  | value |
    | TableCopy(1).salary | 4000  |
    | TableCopy(2).salary | 4000  |
    | TableCopy(3).salary | 3500  |
    | TableCopy(4).salary | 3500  |
    | TableCopy(5).salary | 2000  |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(5).name]] = E      |
    | [[TableCopy(5).age]] = 30      |
    | [[TableCopy(5).salary]] = 2000 |

Scenario: Select all with fields from 2 but Limit to 5 rows
    Given I have a recordset with this shape
    | [[person]]       |      |
    | person(1).name   | A    |
    | person(2).name   | B    |
    | person(3).name   | C    |
    | person(4).name   | D    |
    | person(5).name   | E    |
    | person(6).name   | F    |
    | person(7).name   | G    |
    | person(8).name   | H    |
    | person(9).name   | I    |
    | person(1).age    | 56   |
    | person(2).age    | 30   |
    | person(3).age    | 28   |
    | person(4).age    | 56   |
    | person(5).age    | 30   |
    | person(6).age    | 56   |
    | person(7).age    | 28   |
    | person(8).age    | 28   |
    | person(9).age    | 29   |
    | person(1).salary | 4000 |
    | person(2).salary | 4000 |
    | person(3).salary | 3500 |
    | person(4).salary | 3500 |
    | person(5).salary | 2000 |
    | person(6).salary | 5500 |
    | person(7).salary | 1500 |
    | person(8).salary | 7500 |
    | person(9).salary | 3500 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person limit 5 offset 2"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | name        | [[TableCopy().name]]   |
    | age         | [[TableCopy().age]]    |
    | salary      | [[TableCopy().salary]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | C     |
    | TableCopy(2).name | D     |
    | TableCopy(3).name | E     |
    | TableCopy(4).name | F     |
    | TableCopy(5).name | G     |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 28    |
    | TableCopy(2).age | 56    |
    | TableCopy(3).age | 30    |
    | TableCopy(4).age | 56    |
    | TableCopy(5).age | 28    |
    Then recordset "[[TableCopy(*).salary]]"  will be
    | rs                  | value |
    | TableCopy(1).salary | 3500  |
    | TableCopy(2).salary | 3500  |
    | TableCopy(3).salary | 2000  |
    | TableCopy(4).salary | 5500  |
    | TableCopy(5).salary | 1500  |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(5).name]] = G      |
    | [[TableCopy(5).age]] = 28      |
    | [[TableCopy(5).salary]] = 1500 |

Scenario: Select all with Case
    Given I have a recordset with this shape
    | [[person]]       |      |
    | person(1).name   | A    |
    | person(2).name   | B    |
    | person(3).name   | C    |
    | person(4).name   | D    |
    | person(5).name   | E    |
    | person(6).name   | F    |
    | person(7).name   | G    |
    | person(8).name   | H    |
    | person(9).name   | I    |
    | person(1).age    | 56   |
    | person(2).age    | 30   |
    | person(3).age    | 28   |
    | person(4).age    | 56   |
    | person(5).age    | 30   |
    | person(6).age    | 56   |
    | person(7).age    | 28   |
    | person(8).age    | 28   |
    | person(9).age    | 29   |
    | person(1).salary | 4000 |
    | person(2).salary | 4000 |
    | person(3).salary | 3500 |
    | person(4).salary | 3500 |
    | person(5).salary | 2000 |
    | person(6).salary | 5500 |
    | person(7).salary | 1500 |
    | person(8).salary | 7500 |
    | person(9).salary | 3500 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT name, salary, CASE WHEN salary < 3500  THEN 'Underpaid' ELSE 'Well paid' END FairPaid from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                |
    | name        | [[TableCopy().name]]     |
    | salary      | [[TableCopy().salary]]   |
    | FairPaid    | [[TableCopy().FairPaid]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | A     |
    | TableCopy(2).name | B     |
    | TableCopy(3).name | C     |
    | TableCopy(4).name | D     |
    | TableCopy(5).name | E     |
    | TableCopy(6).name | F     |
    | TableCopy(7).name | G     |
    | TableCopy(8).name | H     |
    | TableCopy(9).name | I     |
    Then recordset "[[TableCopy(*).FairPaid]]"  will be
    | rs                    | value     |
    | TableCopy(1).FairPaid | Well paid |
    | TableCopy(2).FairPaid | Well paid |
    | TableCopy(3).FairPaid | Well paid |
    | TableCopy(4).FairPaid | Well paid |
    | TableCopy(5).FairPaid | Underpaid |
    | TableCopy(6).FairPaid | Well paid |
    | TableCopy(7).FairPaid | Underpaid |
    | TableCopy(8).FairPaid | Well paid |
    | TableCopy(9).FairPaid | Well paid |
    Then recordset "[[TableCopy(*).salary]]"  will be
    | rs                  | value |
    | TableCopy(1).salary | 4000  |
    | TableCopy(2).salary | 4000  |
    | TableCopy(3).salary | 3500  |
    | TableCopy(4).salary | 3500  |
    | TableCopy(5).salary | 2000  |
    | TableCopy(6).salary | 5500  |
    | TableCopy(7).salary | 1500  |
    | TableCopy(8).salary | 7500  |
    | TableCopy(9).salary | 3500  |
    And the execution has "NO" error
    And the debug output as
    |                                       |
    | [[TableCopy(9).name]] = I             |
    | [[TableCopy(9).salary]] = 3500        |
    | [[TableCopy(9).FairPaid]] = Well paid |

Scenario: Using a recordset for IN values
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | other(1).value | Hatter |
    | other(2).value | Bob    |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name  | Value              |
    | names | [[other(*).value]] |
    And I have the following sql statement "SELECT * from person where name IN (@names);"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                |
    | name        | [[TableCopy().name]]     |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Hatter |
    | TableCopy(2).name | Bob    |
    And the debug inputs as
    | Query  | names                       |
    | String | [[other(1).value]] = Hatter |
    |        | [[other(2).value]] = Bob    |
    And the debug output as
    |                             |
    | [[TableCopy(2).name]] = Bob |




Scenario: Using multiple recordset for IN value
	Given I have a recordset with this shape
	| [[person]]     |        |
	| person(1).name | Bob    |
	| person(2).name | Alice  |
	| person(3).name | Hatter |
	| other(1).value | Hatter |
	| other(2).value | Bob    |
	And I drag on an Advanced Recordset tool	
	And Declare variables as
	| Name   | Value             |
	| names  | [[other().value]] |
	| people | [[person().name]] |
	And I have the following sql statement "SELECT * from person where name IN (@names) or name in (@people);"
	When I click Generate Outputs
	Then Outputs are as follows
	| Mapped From | Mapped To                |
	| name        | [[TableCopy().name]]     |
	And Recordset is "TableCopy"	
	When Advanced Recordset tool is executed	
	Then recordset "[[TableCopy(*).name]]"  will be 
	| rs                | value  |
	| TableCopy(1).name | Bob    |
	| TableCopy(2).name | Hatter |
	And the debug inputs as  
	| Query  | names                    | people                      |
	| String | [[other(2).value]] = Bob | [[person(3).name]] = Hatter |
	And the debug output as
	|                                |
	| [[TableCopy(2).name]] = Hatter |
	

Scenario: Using a recordset for IN value
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | other(1).value | Hatter |
    | other(2).value | Bob    |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name  | Value             |
    | names | [[other().value]] |
    And I have the following sql statement "SELECT * from person where name IN (@names);"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                |
    | name        | [[TableCopy().name]]     |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Bob    |
    And the debug inputs as
    | Query  | names                    |
    | String | [[other(2).value]] = Bob |
    And the debug output as
    |                             |
    | [[TableCopy(1).name]] = Bob |

Scenario: Using a recordset for IN value using scalar
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | value          | Hatter |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name  | Value     |
    | names | [[value]] |
    And I have the following sql statement "SELECT * from person where name IN (@names);"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                |
    | name        | [[TableCopy().name]]     |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Hatter    |
    And the debug inputs as
    | Query  | names           |
    | String | [[value]] = Hatter |
    And the debug output as
    |                             |
    | [[TableCopy(1).name]] = Hatter |


Scenario:   Update statement with variable in where clause
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | updateName    | Hatter |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value          |
    | newName | [[updateName]] |
    And I have the following sql statement "UPDATE person SET name = 'Mad Hatter' WHERE name = @newName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From      | Mapped To                         |
    | records_affected | [[Table1Copy().records_affected]] |
    And Recordset is "Table1Copy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From      | Mapped To                        |
    | records_affected | [[newPerson().records_affected]] |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).records_affected]]"  will be
    | rs                           | value |
    | newPerson().records_affected | 1     |
    Then recordset "[[person(*).name]]"  will be
    | rs             | row        |
    | person(1).name | Bob        |
    | person(2).name | Alice      |
    | person(3).name | Mad Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 |
    | String | [[updateName]] = Hatter |
    And the debug output as
    |                                       |
    | [[newPerson(1).records_affected]] = 1 |

Scenario:   Update statement with variable in where clause and Select statement
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | updateName    | Hatter |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value          |
    | newName | [[updateName]] |
    And I have the following sql statement "UPDATE person SET name = 'Mad Hatter' where name = @newName; SELECT * FROM person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[newPerson().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).name]]"  will be
    | rs               | value      |
    | newPerson().name | Bob        |
    | newPerson().name | Alice      |
    | newPerson().name | Mad Hatter |
    Then recordset "[[person(*).name]]"  will be
    | rs             | row        |
    | person(1).name | Bob        |
    | person(2).name | Alice      |
    | person(3).name | Mad Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 |
    | String | [[updateName]] = Hatter |
    And the debug output as
    |                                       |
    | [[newPerson(3).name]] = Mad Hatter    |

Scenario:   complex Update statement
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | other().name | Hatter  |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "UPDATE person SET name = 'Mad Hatter' where name IN (SELECT name FROM other);"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From      | Mapped To                         |
    | records_affected | [[Table1Copy().records_affected]] |
    And Recordset is "Table1Copy"
    When I update Recordset to ""
    Then Recordset is ""
    And Outputs are as follows
    | Mapped From      | Mapped To            |
    | records_affected | [[records_affected]] |
    When Advanced Recordset tool is executed
    Then the result variable "[[records_affected]]" will be "1"
    Then recordset "[[person(*).name]]"  will be
    | rs             | row        |
    | person(1).name | Bob        |
    | person(2).name | Alice      |
    | person(3).name | Mad Hatter |
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                          |
    | [[records_affected]] = 1 |

Scenario:   Update statement with variable in where clause and Select Where statement
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | updateName    | Hatter |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value          |
    | newName | [[updateName]] |
    And I have the following sql statement "UPDATE person SET name = 'Mad Hatter' where name = @newName; SELECT * FROM person WHERE name = 'Mad Hatter'"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When I update Recordset to "TableCopy"
    Then Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value      |
    | TableCopy().name | Mad Hatter |
    Then recordset "[[person(*).name]]"  will be
    | rs             | row        |
    | person(1).name | Bob        |
    | person(2).name | Alice      |
    | person(3).name | Mad Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 |
    | String | [[updateName]] = Hatter |
    And the debug output as
    |                                     |
    | [[TableCopy(1).name]] =  Mad Hatter |

Scenario:   Delete statement with variable in where clause
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | deleteName    | Hatter |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value          |
    | newName | [[deleteName]] |
    And I have the following sql statement "DELETE FROM person WHERE name = @newName"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From      | Mapped To                         |
    | records_affected | [[Table1Copy().records_affected]] |
    And Recordset is "Table1Copy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From      | Mapped To                        |
    | records_affected | [[newPerson().records_affected]] |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).records_affected]]"  will be
    | rs                           | value |
    | newPerson().records_affected | 1     |
    Then recordset "[[person(*).name]]"  will be
    | rs             | row   |
    | person(1).name | Bob   |
    | person(2).name | Alice |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 |
    | String | [[deleteName]] = Hatter |
    And the debug output as
    |                                       |
    | [[newPerson(1).records_affected]] = 1 |

Scenario:   Delete statement with variable in where clause and Select statement
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | deleteName    | Hatter |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value          |
    | newName | [[deleteName]] |
    And I have the following sql statement "DELETE FROM person WHERE name = @newName; SELECT * FROM person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[newPerson().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).name]]"  will be
    | rs               | value |
    | newPerson().name | Bob   |
    | newPerson().name | Alice |
    Then recordset "[[person(*).name]]"  will be
    | rs | row |
    | person(1).name               | Bob        |
    | person(2).name               | Alice      |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 |
    | String | [[deleteName]] = Hatter |
    And the debug output as
    |                                |
    | [[newPerson(2).name]] =  Alice |

Scenario:   Delete statement with variable in where clause and Select Where statement
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | deleteName    | Hatter |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value          |
    | newName | [[deleteName]] |
    And I have the following sql statement "DELETE FROM person WHERE name = @newName; SELECT * FROM person WHERE name = 'Bob'"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[newPerson().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).name]]"  will be
    | rs               | value |
    | newPerson().name | Bob   |
    Then recordset "[[person(*).name]]"  will be
    | rs             | row   |
    | person(1).name | Bob   |
    | person(2).name | Alice |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 |
    | String | [[deleteName]] = Hatter |
    And the debug output as
    |                             |
    | [[newPerson(1).name]] = Bob |

Scenario:   simple Join statement
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | other().name  | Hatter |
    | other().name  | Gary   |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "select o.name from person p join other o on p.name=o.name"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |

Scenario:   simple Join statement with star in field names
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | other().name  | Hatter |
    | other().name  | Gary   |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "select o.* from person p join other o on p.name=o.name"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |

Scenario:   simple inner Join statement with star in field names
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | other().name  | Hatter |
    | other().name  | Gary   |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "select o.* from person p join other o on p.name=o.name"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |

Scenario:   simple cross Join statement with star in field names
    Given I have a recordset with this shape
    | [[person]]      |        |
    | person().name   | Bob    |
    | person().name   | Alice  |
    | person().name   | Hatter |
    | other().surname | tt     |
    | other().surname | uu     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "select * from person cross join other"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To               |
    | name        | [[TableCopy().name]]    |
    | surname     | [[TableCopy().surname]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Bob    |
    | TableCopy().name | Bob    |
    | TableCopy().name | Alice  |
    | TableCopy().name | Alice  |
    | TableCopy().name | Hatter |
    | TableCopy().name | Hatter |
    Then recordset "[[TableCopy(*).surname]]"  will be
    | rs                  | value |
    | TableCopy().surname | tt    |
    | TableCopy().surname | uu    |
    | TableCopy().surname | tt    |
    | TableCopy().surname | uu    |
    | TableCopy().surname | tt    |
    | TableCopy().surname | uu    |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                |
    | [[TableCopy(6).name]] = Hatter |
    | [[TableCopy(6).surname]] = uu  |

Scenario:   Update statement with variable in where clause and Like starts with
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | updateName    | Hatter |
    | likeName      | Ma%    |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value          |
    | newName  | [[updateName]] |
    | newMatch | [[likeName]]   |
    And I have the following sql statement "UPDATE person SET name = 'Mad Hatter' WHERE name = @newName; SELECT * FROM person WHERE name LIKE @newMatch;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value      |
    | TableCopy().name | Mad Hatter |
    Then recordset "[[person(*).name]]"  will be
    | rs | row |
    | person(1).name               | Bob        |
    | person(2).name               | Alice      |
    | person(3).name               | Mad Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 | newMatch           |
    | String | [[updateName]] = Hatter | [[likeName]] = Ma% |
    And the debug output as
    |                                    |
    | [[TableCopy(1).name]] = Mad Hatter |

Scenario:   Update statement with variable in where clause and Like ends with
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | updateName    | Hatter |
    | likeName      | %ter   |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value          |
    | newName  | [[updateName]] |
    | newMatch | [[likeName]]   |
    And I have the following sql statement "UPDATE person SET name = 'Mad Hatter' WHERE name = @newName; SELECT * FROM person WHERE name LIKE @newMatch;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value      |
    | TableCopy().name | Mad Hatter |
    Then recordset "[[person(*).name]]"  will be
    | rs | row |
    | person(1).name               | Bob        |
    | person(2).name               | Alice      |
    | person(3).name               | Mad Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 | newMatch            |
    | String | [[updateName]] = Hatter | [[likeName]] = %ter |
    And the debug output as
    |                                    |
    | [[TableCopy(1).name]] = Mad Hatter |

Scenario:   Update statement with variable in where clause and Like in any position
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | updateName    | Hatter |
    | likeName      | %att%  |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value          |
    | newName  | [[updateName]] |
    | newMatch | [[likeName]]   |
    And I have the following sql statement "UPDATE person SET name = 'Mad Hatter' WHERE name = @newName; SELECT * FROM person WHERE name LIKE @newMatch;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value      |
    | TableCopy().name | Mad Hatter |
    Then recordset "[[person(*).name]]"  will be
    | rs | row |
    | person(1).name               | Bob        |
    | person(2).name               | Alice      |
    | person(3).name               | Mad Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 | newMatch             |
    | String | [[updateName]] = Hatter | [[likeName]] = %att% |
    And the debug output as
    |                                    |
    | [[TableCopy(1).name]] = Mad Hatter |

Scenario:   Update statement with variable in where clause and Like in second position
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | updateName    | Hatter |
    | likeName      | _a%    |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value          |
    | newName  | [[updateName]] |
    | newMatch | [[likeName]]   |
    And I have the following sql statement "UPDATE person SET name = 'Mad Hatter' WHERE name = @newName; SELECT * FROM person WHERE name LIKE @newMatch;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value      |
    | TableCopy().name | Mad Hatter |
    Then recordset "[[person(*).name]]"  will be
    | rs | row |
    | person(1).name               | Bob        |
    | person(2).name               | Alice      |
    | person(3).name               | Mad Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 | newMatch           |
    | String | [[updateName]] = Hatter | [[likeName]] = _a% |
    And the debug output as
    |                                    |
    | [[TableCopy(1).name]] = Mad Hatter |

Scenario:   Update statement with variable in where clause and Like starts with and at least three characters in length
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | updateName    | Hatter |
    | likeName      | M_%_%  |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value          |
    | newName  | [[updateName]] |
    | newMatch | [[likeName]]   |
    And I have the following sql statement "UPDATE person SET name = 'Mad Hatter' WHERE name = @newName; SELECT * FROM person WHERE name LIKE @newMatch;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value      |
    | TableCopy().name | Mad Hatter |
    Then recordset "[[person(*).name]]"  will be
    | rs | row |
    | person(1).name               | Bob        |
    | person(2).name               | Alice      |
    | person(3).name               | Mad Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 | newMatch             |
    | String | [[updateName]] = Hatter | [[likeName]] = M_%_% |
    And the debug output as
    |                                    |
    | [[TableCopy(1).name]] = Mad Hatter |

Scenario:   Update statement with variable in where clause and Like starts with and ends with
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    | updateName    | Hatter |
    | likeName      | M%r    |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value          |
    | newName  | [[updateName]] |
    | newMatch | [[likeName]]   |
    And I have the following sql statement "UPDATE person SET name = 'Mad Hatter' WHERE name = @newName; SELECT * FROM person WHERE name LIKE @newMatch;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value      |
    | TableCopy().name | Mad Hatter |
    Then recordset "[[person(*).name]]"  will be
    | rs | row |
    | person(1).name               | Bob        |
    | person(2).name               | Alice      |
    | person(3).name               | Mad Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName                 | newMatch           |
    | String | [[updateName]] = Hatter | [[likeName]] = M%r |
    And the debug output as
    |                                    |
    | [[TableCopy(1).name]] = Mad Hatter |

Scenario:   Update statement with variable in where clause and Like starts with and ends with Multiple
    Given I have a recordset with this shape
    | [[person]]      |        |
    | person(1).name  | Bober  |
    | person(1).aid   | 1      |
    | person(2).name  | Alice  |
    | person(2).aid   | 2      |
    | person(3).name  | Hatter |
    | person(3).aid   | 1      |
    | address(1).id   | 2      |
    | address(1).name | addr1  |
    | address(2).id   | 1      |
    | address(2).name | addr2  |
    | updateName      | %er    |
    | likeName        | %ter   |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name     | Value          |
    | nameLike | [[updateName]] |
    | newMatch | [[likeName]]   |
    And I have the following sql statement "UPDATE address SET name = 'Mad Hatter address' WHERE EXISTS (SELECT aid FROM person WHERE name LIKE @nameLike AND person.aid=address.id); UPDATE person SET name='Lucy' WHERE aid=2; SELECT * FROM person WHERE name LIKE @newMatch;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | aid         | [[TableCopy().aid]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value      |
    | TableCopy().name | Hatter     |
    Then recordset "[[address(*).name]]"  will be
    | rs              | row                |
    | address(1).name | addr1              |
    | address(2).name | Mad Hatter address |
    Then recordset "[[person(*).name]]"  will be
    | rs | row |
    | person(1).name | Bober  |
    | person(2).name | Lucy   |
    | person(3).name | Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | nameLike              | newMatch            |
    | String | [[updateName]] = %er  | [[likeName]] = %ter |
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |
    | [[TableCopy(1).aid]]  = 1      |

Scenario:  aggregate functions Select with AVG
    Given I have a recordset with this shape
    | [[avg_tests]]   |      |
    | avg_tests().val | 1    |
    | avg_tests().val | 2    |
    | avg_tests().val | 10.1 |
    | avg_tests().val | 20.5 |
    | avg_tests().val | 8    |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT avg(val) as val FROM avg_tests;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To           |
    | val         | [[TableCopy().val]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).val]]"  will be
    | rs              | value |
    | TableCopy().val | 8.32  |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                             |
    | [[TableCopy(1).val]] = 8.32 |

    Scenario: aggregate functions  Select with AVG using variable
    Given I have a recordset with this shape
    | [[avg_tests]]   |      |
    | avg_tests().val | 1    |
    | avg_tests().val | 2    |
    | avg_tests().val | 10.1 |
    | avg_tests().val | 20.5 |
    | avg_tests().val | 8    |
    | avg_variable    | 2    |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value            |
    | newName | [[avg_variable]] |
    And I have the following sql statement "SELECT avg(val) as val FROM avg_tests where val > @newName ;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To           |
    | val         | [[TableCopy().val]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).val]]"  will be
    | rs              | value             |
    | TableCopy().val | 12.86666666666670 |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName              |
    | String | [[avg_variable]] = 2 |
    And the debug output as
    |                                          |
    | [[TableCopy(1).val]] = 12.86666666666670 |

Scenario:  aggregate functions Select With MAX
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT MAX(age) as MaxAge from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | MaxAge      | [[TableCopy().MaxAge]] |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To              |
    | MaxAge      | [[TableCopy().MaxAge]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).MaxAge]]"  will be
    | rs                  | value |
    | TableCopy(1).MaxAge | 31    |
    And the execution has "NO" error
    And the debug output as
    |                              |
    | [[TableCopy(1).MaxAge]] = 31 |

Scenario:  aggregate functions Select With MAX with Max in where clause
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where age  =(select MAX(age) from person);"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(2).name | Alice |
        Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(2).age | 31    |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(1).name]] = Alice |
    | [[TableCopy(1).age]] = 31     |

Scenario:  aggregate functions Select With ROUND
    Given I have a recordset with this shape
    | [[avg_tests]]   |      |
    | avg_tests().val | 1    |
    | avg_tests().val | 2    |
    | avg_tests().val | 10.1 |
    | avg_tests().val | 20.5 |
    | avg_tests().val | 8    |
    | avg_variable    | 2    |
    And I drag on an Advanced Recordset tool
    And Declare variables as
    | Name    | Value            |
    | newName | [[avg_variable]] |
    And I have the following sql statement "SELECT round(avg(val),2)  as val FROM avg_tests where val > @newName ;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To           |
    | val         | [[TableCopy().val]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).val]]"  will be
    | rs              | value |
    | TableCopy().val | 12.87 |
    And the execution has "NO" error
    And the debug inputs as
    | Query  | newName              |
    | String | [[avg_variable]] = 2 |
    And the debug output as
    |                              |
    | [[TableCopy(1).val]] = 12.87 |

Scenario:  aggregate functions Select With MIN
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT MIN(age) as MinAge from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To              |
    | MinAge      | [[TableCopy().MinAge]] |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To              |
    | MinAge      | [[TableCopy().MinAge]] |
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).MinAge]]"  will be
    | rs                  | value |
    | TableCopy(1).MinAge | 19    |
    And the execution has "NO" error
    And the debug output as
    |                              |
    | [[TableCopy(1).MinAge]] = 19 |

Scenario:  Aggregate functions Select With MIN with min in where clause
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person where age  =(select MIN(age) from person);"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(2).name | Hatter |
        Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(2).age | 19    |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(1).name]] = Hatter |
    | [[TableCopy(1).age]] = 19      |

Scenario:  aggregate functions Select With SUM
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT SUM(age)  as ages from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | ages        | [[TableCopy().ages]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).ages]]"  will be
    | rs                | value |
    | TableCopy(2).ages | 75    |
    And the execution has "NO" error
    And the debug output as
    |                            |
    | [[TableCopy(1).ages]] = 75 |

Scenario: String Function Select With Substr
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT substr(name, 0, 4) as name from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | Bob   |
    | TableCopy(2).name | Ali   |
    | TableCopy(3).name | Hat   |
    And the execution has "NO" error
    And the debug output as
    |                             |
    | [[TableCopy(3).name]] = Hat |

Scenario: String Function Select With Trim
    Given I have a recordset with this shape
    | [[person]]     |         |
    | person(1).name | Bob     |
    | person(2).name | Alice^^ |
    | person(3).name | Hatter^ |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT trim(name, '^') as name from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                    | value  |
    | [[TableCopy(1).name   | Bob    |
    | [[TableCopy(2).name]] | Alice  |
    | [[TableCopy(3).name]] | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(3).name]] = Hatter |

Scenario: String  Function Select With LTrim
    Given I have a recordset with this shape
    | [[person]]     |           |
    | person(1).name | Bob       |
    | person(2).name | Alice     |
    | person(3).name | ^^^Hatter |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT ltrim(name, '^') as name from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                    | value  |
    | [[TableCopy(1).name   | Bob    |
    | [[TableCopy(2).name]] | Alice  |
    | [[TableCopy(3).name]] | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(3).name]] = Hatter |

Scenario: String  Function Select With RTrim
    Given I have a recordset with this shape
    | [[person]]     |           |
    | person(1).name | Bob       |
    | person(2).name | Alice     |
    | person(3).name | Hatter^^^ |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT rtrim(name, '^') as name from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                    | value  |
    | [[TableCopy(1).name   | Bob    |
    | [[TableCopy(2).name]] | Alice  |
    | [[TableCopy(3).name]] | Hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(3).name]] = Hatter |

Scenario: String  Function Select With Length
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT length(name) as name from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value |
    | TableCopy(1).name | 3     |
    | TableCopy(2).name | 5     |
    | TableCopy(3).name | 6     |
    And the execution has "NO" error
    And the debug output as
    |                           |
    | [[TableCopy(3).name]] = 6 |

Scenario: String  Function Select With Complex Length
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT name, length(name) as name2 FROM person ORDER BY length(name) DESC"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To             |
    | name        | [[TableCopy().name]]  |
    | name2       | [[TableCopy().name2]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Hatter |
    | TableCopy(2).name | Alice  |
    | TableCopy(3).name | Bob    |
    Then recordset "[[TableCopy(*).name2]]"  will be
    | rs                 | value |
    | TableCopy(1).name2 | 6     |
    | TableCopy(2).name2 | 5     |
    | TableCopy(3).name2 | 3     |
    And the execution has "NO" error
    And the debug output as
    |                             |
    | [[TableCopy(3).name]] = Bob |
    | [[TableCopy(3).name2]] = 3  |

Scenario: String  Function Select With Replace Then Select
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "REPLACE INTO person (name, age) VALUES ('Robocop', 1000) ;select * from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value   |
    | TableCopy(1).name | Bob     |
    | TableCopy(2).name | Alice   |
    | TableCopy(3).name | Hatter  |
    | TableCopy(4).name | Robocop |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 25    |
    | TableCopy(2).age | 31    |
    | TableCopy(3).age | 19    |
    | TableCopy(4).age | 1000  |
    And the execution has "NO" error
    And the debug output as
    |                                 |
    | [[TableCopy(4).name]] = Robocop |
    | [[TableCopy(4).age]] = 1000     |

Scenario: String  Function Select With Replace
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "REPLACE INTO person (name, age) VALUES ('Robocop', 1000)"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From      | Mapped To                         |
    | records_affected | [[Table1Copy().records_affected]] |
    And Recordset is "Table1Copy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From      | Mapped To                        |
    | records_affected | [[newPerson().records_affected]] |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).records_affected]]"  will be
    | rs                           | value |
    | newPerson().records_affected | 1     |
    And the execution has "NO" error
    And the debug output as
    |                                       |
    | [[newPerson(1).records_affected]] = 1 |

Scenario: String  Function Select With INSERT Then Select
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "INSERT INTO person (name, age) VALUES ('Robocop', 1000) ;select * from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value   |
    | TableCopy(1).name | Bob     |
    | TableCopy(2).name | Alice   |
    | TableCopy(3).name | Hatter  |
    | TableCopy(4).name | Robocop |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 25    |
    | TableCopy(2).age | 31    |
    | TableCopy(3).age | 19    |
    | TableCopy(4).age | 1000  |
    And the execution has "NO" error
    And the debug output as
    |                                 |
    | [[TableCopy(4).name]] = Robocop |
    | [[TableCopy(4).age]] = 1000     |


Scenario: Insert with looped execution through recordset s a variable
    Given I have a recordset with this shape
    | [[person]]      |        |
    | person(1).name  | Bob    |
    | person(2).name  | Alice  |
    | person(3).name  | Hatter |
    | person(1).age   | 25     |
    | person(2).age   | 31     |
    | person(3).age   | 19     |
    | insertNew(1).val | Jack   |
    | insertNew(2).val | Harry  |
    And I drag on an Advanced Recordset tool
	And Declare variables as
    | Name    | Value         |
    | InsertStatement | [[insertNew(*).val]] |
    And I have the following sql statement "INSERT INTO person (name, age) VALUES (@InsertStatement,90)"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From      | Mapped To                         |
    | records_affected | [[Table1Copy().records_affected]] |
   When Advanced Recordset tool is executed
    And the execution has "NO" error
    And the debug output as
    |                                        |
    | [[Table1Copy(2).records_affected]] = 2 |

Scenario: Insert with looped execution through recordset with one field
    Given I have a recordset with this shape
    | [[person]]      |        |
    | person(1).name  | Bob    |
    | person(2).name  | Alice  |
    | person(3).name  | Hatter |
    | insertNew(1).val | Jack   |
    | insertNew(2).val | Harry  |
    And I drag on an Advanced Recordset tool
	And Declare variables as
    | Name    | Value         |
    | InsertStatement | [[insertNew(*).val]] |
    And I have the following sql statement "INSERT INTO person (name) VALUES (@InsertStatement)"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From      | Mapped To                         |
    | records_affected | [[Table1Copy().records_affected]] |
   When Advanced Recordset tool is executed
    And the execution has "NO" error
    And the debug output as
    |                                        |
    | [[Table1Copy(2).records_affected]] = 2 |

Scenario: String  Function Select With Insert
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "INSERT INTO person (name, age) VALUES ('Robocop', 1000)"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From      | Mapped To                         |
    | records_affected | [[Table1Copy().records_affected]] |
    And Recordset is "Table1Copy"
    When I update Recordset to "newPerson"
    Then Recordset is "newPerson"
    And Outputs are as follows
    | Mapped From      | Mapped To                        |
    | records_affected | [[newPerson().records_affected]] |
    When Advanced Recordset tool is executed
    Then recordset "[[newPerson(*).records_affected]]"  will be
    | rs                           | value |
    | newPerson().records_affected | 1     |
    And the execution has "NO" error
    And the debug output as
    |                                       |
    | [[newPerson(1).records_affected]] = 1 |

Scenario:  String Function Select With INSTR
    Given I have a recordset with this shape
    | [[person]]     |            |
    | person(1).name | Bob        |
    | person(2).name | Alice      |
    | person(3).name | Mad Hatter |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT INSTR(name,'Hatter') as position from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                |
    | position    | [[TableCopy().position]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).position]]"  will be
    | rs                    | value |
    | TableCopy(1).position | 0     |
    | TableCopy(2).position | 0     |
    | TableCopy(3).position | 5     |
    And the execution has "NO" error
    And the debug output as
    |                               |
    | [[TableCopy(3).position]] = 5 |

Scenario:  String Function Select With Upper
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT upper(name) as name from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | BOB    |
    | TableCopy(2).name | ALICE  |
    | TableCopy(3).name | HATTER |
    And the execution has "NO" error
    And the debug output as
    |                                 |
    | [[TableCopy(3).name]] = HATTER  |

Scenario:  String Function Select With Lower
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT lower(name) as name from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | bob    |
    | TableCopy(2).name | alice  |
    | TableCopy(3).name | hatter |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(3).name]] = hatter |

Scenario:   Select All UNION ALL clause which DOES remove duplicates.
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person UNION  SELECT * from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Bob    |
    | TableCopy(2).name | Alice  |
    | TableCopy(3).name | Hatter |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs               | value |
    | TableCopy(1).age | 25    |
    | TableCopy(2).age | 31    |
    | TableCopy(3).age | 19    |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(3).name]] = Hatter |
    | [[TableCopy(3).age]] = 19      |

Scenario:  Select All UNION  clause which DOES NOT remove duplicates.
    Given I have a recordset with this shape
    | [[person]]     |        |
    | person(1).name | Bob    |
    | person(2).name | Alice  |
    | person(3).name | Hatter |
    | person(1).age  | 25     |
    | person(2).age  | 31     |
    | person(3).age  | 19     |

    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person UNION ALL  SELECT * from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    | age         | [[TableCopy().age]]  |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Bob    |
    | TableCopy(2).name | Alice  |
    | TableCopy(3).name | Hatter |
    | TableCopy(4).name | Bob    |
    | TableCopy(5).name | Alice  |
    | TableCopy(6).name | Hatter |
    Then recordset "[[TableCopy(*).age]]"  will be
    | rs            | value |
    | TableCopy(1).age | 25   |
    | TableCopy(2).age | 31   |
    | TableCopy(3).age | 19   |
    | TableCopy(4).age | 25   |
    | TableCopy(5).age | 31   |
    | TableCopy(6).age | 19   |
    And the execution has "NO" error
    And the debug output as
    |                                |
    | [[TableCopy(6).name]] = Hatter |
    | [[TableCopy(6).age]] = 19      |

Scenario:  math functions Select With ABS
    Given I have a recordset with this shape
    | [[avg_tests]]   |       |
    | avg_tests().val | -1000 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT ABS(val) absValue from avg_tests;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                |
    | absValue    | [[TableCopy().absValue]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).absValue]]"  will be
    | rs                   | value |
    | TableCopy().absValue | 1000  |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                  |
    | [[TableCopy(1).absValue]] = 1000 |
	
Scenario:  math functions Select With ABS TextLastNumber
    Given I have a recordset with this shape
    | [[avg_tests]]   |       |
    | avg_tests().val | 3000abs |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT ABS('3000abs') as absValue;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                |
    | absValue    | [[TableCopy().absValue]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).absValue]]"  will be
    | rs                   | value |
    | TableCopy().absValue | 3000  |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                  |
    | [[TableCopy(1).absValue]] = 3000 |

Scenario:  math functions Select With ABS Text Returns0
    Given I have a recordset with this shape
    | [[avg_tests]]   |       |
    | avg_tests().val | 3000abs |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT ABS('aba3000') as absValue;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                |
    | absValue    | [[TableCopy().absValue]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).absValue]]"  will be
    | rs                   | value |
    | TableCopy().absValue | 0  |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                               |
    | [[TableCopy(1).absValue]] = 0 |

    Scenario:   Select With Self Join
    Given I have a recordset with this shape
    | [[person]]          |        |
    | person(1).name      | Bob    |
    | person(2).name      | Alice  |
    | person(3).name      | Hatter |
    | person(4).name      | Rabbit |
    | person(5).name      | Puff   |
    | person(1).reportsto | Puff   |
    | person(2).reportsto | Bob    |
    | person(3).reportsto | Bob    |
    | person(4).reportsto | Bob    |
    | person(5).reportsto | Puff   |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT e.name as Manager,f.name as DirectReport from person e inner join person f on e.reportsto = f.name;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From  | Mapped To                    |
    | Manager      | [[TableCopy().Manager]]      |
    | DirectReport | [[TableCopy().DirectReport]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).Manager]]"  will be
    | rs                   | value  |
    | TableCopy(1).Manager | Bob    |
    | TableCopy(2).Manager | Alice  |
    | TableCopy(3).Manager | Hatter |
    | TableCopy(4).Manager | Rabbit |
    | TableCopy(5).Manager | Puff   |
    Then recordset "[[TableCopy(*).DirectReport]]"  will be
    | rs                        | value |
    | TableCopy(1).DirectReport | Puff  |
    | TableCopy(2).DirectReport | Bob   |
    | TableCopy(3).DirectReport | Bob   |
    | TableCopy(4).DirectReport | Bob   |
    | TableCopy(5).DirectReport | Puff  |
    And the execution has "NO" error
    And the debug output as
    |                                      |
    | [[TableCopy(5).Manager]] = Puff      |
    | [[TableCopy(5).DirectReport]] = Puff |

    Scenario: strftime Select the number of seconds since a particular moment:
    Given I have a recordset with this shape
    | [[person]]     |                     |
    | person(1).name | Bob                 |
    | person(2).name | Alice               |
    | person(3).name | Hatter              |
    | person(1).dob  | 1978-01-01 02:34:56 |
    | person(2).dob  | 1979-01-01 02:34:56 |
    | person(3).dob  | 1980-01-01 02:34:56 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT name, strftime('%s',date('1981-01-01 02:34:56')) - strftime('%s',dob) as numberofseconds from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From     | Mapped To                       |
    | numberofseconds | [[TableCopy().numberofseconds]] |
    | name            | [[TableCopy().name]]            |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Bob    |
    | TableCopy(2).name | Alice  |
    | TableCopy(3).name | Hatter |
    Then recordset "[[TableCopy(*).numberofseconds]]"  will be
    | rs               | value |
    | TableCopy(1).numberofseconds | 94685104   |
    | TableCopy(2).numberofseconds | 63149104   |
    | TableCopy(3).numberofseconds | 31613104   |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                             |
    | [[TableCopy(3).name]] = Hatter              |
    | [[TableCopy(3).numberofseconds]] = 31613104 |

    Scenario: datetime Select the date and time given a unix timestamp 1092941466:
    Given I have a recordset with this shape
    | [[person]]     |                     |
    | person(1).name | Bob                 |
    | person(2).name | Alice               |
    | person(3).name | Hatter              |
    | person(1).dob  | 1978-01-01 02:34:56 |
    | person(2).dob  | 1979-01-01 02:34:56 |
    | person(3).dob  | 1980-01-01 02:34:56 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT datetime(1092941466, 'unixepoch') as dob,name from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | dob         | [[TableCopy().dob]]  |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Bob    |
    | TableCopy(2).name | Alice  |
    | TableCopy(3).name | Hatter |
    Then recordset "[[TableCopy(*).dob]]"  will be
    | rs               | value |
    | TableCopy(1).dob | 2004-08-19 18:51:06   |
    | TableCopy(2).dob | 2004-08-19 18:51:06   |
    | TableCopy(3).dob | 2004-08-19 18:51:06   |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                            |
    | [[TableCopy(3).dob]] = 2004-08-19 18:51:06 |
    | [[TableCopy(3).name]] = Hatter             |

    Scenario: time Select the time given a dob:
    Given I have a recordset with this shape
    | [[person]]     |                     |
    | person(1).name | Bob                 |
    | person(2).name | Alice               |
    | person(3).name | Hatter              |
    | person(1).dob  | 1978-01-01 02:34:56 |
    | person(2).dob  | 1979-01-01 15:34:56 |
    | person(3).dob  | 1980-01-01 10:34:56 |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT time(dob) as dob,name from person;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | dob         | [[TableCopy().dob]]  |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).name]]"  will be
    | rs                | value  |
    | TableCopy(1).name | Bob    |
    | TableCopy(2).name | Alice  |
    | TableCopy(3).name | Hatter |
    Then recordset "[[TableCopy(*).dob]]"  will be
    | rs               | value    |
    | TableCopy(1).dob | 02:34:56 |
    | TableCopy(2).dob | 15:34:56 |
    | TableCopy(3).dob | 10:34:56 |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                 |
    | [[TableCopy(3).dob]] = 10:34:56 |
    | [[TableCopy(3).name]] = Hatter  |

Scenario:  Use an undeclare variable on the query returns error
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Mandy  |
    | person().name | Hatter |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT * from person  where name = @undeclaredVariable"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    And the advancerecodset execution has "AN" error

	Scenario:  Select Random
    Given I have a recordset with this shape
    | [[person]]    |        |
    | person().name | Bob    |
    | person().name | Alice  |
    | person().name | Hatter |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "SELECT name from person ORDER BY RANDOM() LIMIT 1;"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    And Recordset is "TableCopy"
    And Outputs are as follows
    | Mapped From | Mapped To            |
    | name        | [[TableCopy().name]] |
    When Advanced Recordset tool is executed
   
Scenario: Select Group By Having statement
    Given I have a recordset with this shape
    | [[person]]         |              |
    | person(1).name     | Bob          |
    | person(2).name     | Alice        |
    | person(3).name     | Hatter       |
    | person(1).surname  | Bob          |
    | person(2).surname  | Alice        |
    | person(3).surname  | Hatter       |
    | person(1).city     | Durban       |
    | person(2).city     | Durban       |
    | person(3).city     | CPT          |
    | person(1).province | KZN          |
    | person(2).province | KZN          |
    | person(3).province | Western Cape |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "select province,city,count(*) as people from person group by province,city having city = 'Durban'; "
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                |
    | province    | [[TableCopy().province]] |
    | city        | [[TableCopy().city]]     |
    | people      | [[TableCopy().people]]   |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).province]]"  will be
    | rs               | value  |
    | TableCopy().province | KZN |
	Then recordset "[[TableCopy(*).city]]"  will be
    | rs               | value  |
    | TableCopy().city | Durban |
	Then recordset "[[TableCopy(*).people]]"  will be
    | rs                 | value |
    | TableCopy().people | 2     |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                 |
    | [[TableCopy(1).province]] = KZN |
    | [[TableCopy(1).city]] = Durban  |
    | [[TableCopy(1).people]] = 2     |

Scenario: Handle Nulls set to Nothing
    Given I have a recordset with this shape
    | [[person]]         |              |
    | person(1).name     | Bob          |
    | person(2).name     | Alice        |
    | person(3).name     | Hatter       |
    | person(1).city     |              |
    | person(2).city     | Durban       |
    | person(3).city     |              |
    | person(1).province |              |
    | person(2).province |              |
    | person(3).province | Western Cape |
    And I drag on an Advanced Recordset tool
    And I have the following sql statement "select * from person"
    When I click Generate Outputs
    Then Outputs are as follows
    | Mapped From | Mapped To                |
    | name        | [[TableCopy().name]]     |
    | city        | [[TableCopy().city]]     |
    | province    | [[TableCopy().province]] |
    And Recordset is "TableCopy"
    When Advanced Recordset tool is executed
    Then recordset "[[TableCopy(*).province]]"  will be
    | rs                   | value        |
    | TableCopy().province |              |
    | TableCopy().province |              |
    | TableCopy().province | Western Cape |
	Then recordset "[[TableCopy(*).city]]"  will be
    | rs               | value  |
    | TableCopy().city |        |
    | TableCopy().city | Durban |
    | TableCopy().city |        |
	Then recordset "[[TableCopy(*).name]]"  will be
    | rs               | value  |
    | TableCopy().name | Bob    |
    | TableCopy().name | Alice  |
    | TableCopy().name | Hatter |
    And the execution has "NO" error
    And the debug inputs as
    | Query  |
    | String |
    And the debug output as
    |                                          |
    | [[TableCopy(3).name]] = Hatter           |
    | [[TableCopy(3).city]] =                  |
    | [[TableCopy(3).province]] = Western Cape |