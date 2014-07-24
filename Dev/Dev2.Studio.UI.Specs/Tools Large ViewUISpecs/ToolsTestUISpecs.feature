#Feature: Tools 
#	In order to drag Tools from Tool box to design surface
#	As a Warewolf user
#	I want to be able to drag tools from tool box to Design surface
#
#Scenario: Assign Large View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Assign" onto design surface
#       Then "Assign" tool “Small” view should be visible
#	   When I select "Show Large View" from the context menu
#       Then "Assign" tool “Large” view should be visible
#       And I enter variable "rec$().a" in row "1"
#       And I press Tab "1" time
#       And I enter Value "Warewolf"
#       When I click "Done" on "Assign"
#       Then "Errors" box is visible
#       Then I click on error 1
#       And the cursor is placed at "[[rec$().a]]" in row "1"
#       And I remove "[[rec$().a]]" In row "1" 
#       And I enter variable "[[rec().a]]" in row "1"
#       Then I click "Done" on "Assign"
#       And "Assign" tool “Small” view should be visible
#
#Scenario: Assign Quick Variable Test
#      Given I have Warewolf running
#	  And I click on "New Workflow"
#	  And I drag an "Assign" onto design surface
#	  Then "Assign" tool “Small” view should be visible 
#	  When I click on "Open Quick Variable Input" on "Assign"
#	  Then "Quick Variable Input" is visible
#	  And I enter variable "[[rec(1).a]]" in variable list
#	  And I click on "Split List In" dropdown
#	  When I select "New Line" in Split List On
#	  Then "Preview" button is "Enabled"
#	  And "Add" button is "Enabled"
#	  When I click on "Add"
#	  Then "Assign" tool “Small” view should be visible
#
#Scenario: Passing Invalid variables in Assign tools from QVI
#      Given I have Warewolf running
#	  And I click on "New Workflow"
#	  And I drag an "Assign" onto design surface
#	  And "Assign" tool “Small” view should be visible 
#	  When I select "Show Large View" from the context menu
#	  Then "Assign" tool “Large” view should be visible
#	  When I click on "Open Quick Variable Input" on "Assign"
#	  Then "Quick Variable Input" is visible
#	  And I enter variable "[[#$$#]]" in variable list
#	  And I click on "Split List In" dropdown
#	  When I select "New Line" in Split List On
#	  Then "Preview" button is "Enabled"
#	  And "Add" button is "Enabled"
#	  And I select "Replace"
#	  When I click on "Add"
#	  Then "Assign" tool “Large” view should be visible
#	  When I click "Done" on "Assign"
#      Then "Errors" box is visible
#      Then I click on error 1
#      And the cursor is placed at "[[#$$#]]" in row "1"
#      And I remove "[[#$$#]]" In row "1" 
#      And I enter variable "[[rec().a]]" in row "1"
#      Then I click "Done" on "Assign"
#      And "Assign" tool “Small” view should be visible
#
#
#Scenario: Data Merge Large View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Data Merge" onto design surface
#       Then "Data Merge" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#       Then "Data Merge" tool “Large” view should be visible
#	   And I click on input box row "1" 
#       And I enter variable "[[rec(1%).a]]" in row "1"
#	   And I enter "Using" as "1" in row "1"
#	   And I enter "Padding" as "@" in row "1" 
#       When I click "Done" on "Data Merge"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "[[rec(1%).a]]" in row "1"
#       And I remove "[[rec(1%).a]]" In row "1" 
#       And I enter variable "[[rec(1%).a]]" in row "1"
#       When I click "Done" on "Data Merge"
#       Then "Data Merge" tool “Small” view should be visible
#
#
#Scenario: Data Merge Small View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Data Merge" onto design surface
#       Then "Data Merge" tool “Small” view should be visible
#	   And I enter variable "[[rec(1%).a]]" in row "1"
#	   And I press Tab "2" times
#	   And I enter "Using" as "1" in row "1"
#	   And I enter result variable "[[a]]" in "Data Merge"
#
#Scenario: Data Split Small View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Data Split" onto design surface
#       Then "Data Split" tool “Small” view should be visible
#	   And I enter "String To Split" data "Warewolf" 
#	   And I enter variable "[[rec(1%).a]]" in row "1"
#	   And I press Tab "2" times
#	   And I enter "Using" as "1" in row "1"
#	  
#Scenario: Data Split Large View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Data Split" onto design surface
#       Then "Data Split" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#       Then "Data Split" tool “Large” view should be visible
#	   And I enter "String To Split" data "Warewolf" 
#	   And I select Process Direction "Backward"
#	   And I click on input box row "1" 
#       And I enter variable "[[rec(1%).a]]" in row "1"
#	   And I enter "Using" as "1" in row "1"
#	   And I select "With"  as "Index"
#	   And "Include" is "Disabled" in row "1"
#	   And "Escapae" is "Disabled" in row "1"
#       When I click "Done" on "Data Split"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "[[rec(1%).a]]" in row "1"
#       And I remove "[[rec(1%).a]]" In row "1" 
#       And I enter variable "[[rec(1).a]]" in row "1"
#       When I click "Done" on "Data Split"
#       Then "Data Split" tool “Small” view should be visible
#
#Scenario: Passing Invalid variables into Data Split through QVI
#      Given I have Warewolf running
#      And  I click on "New Workflow"
#      And I drag an "Data Split" onto design surface
#      Then "Data Split" tool “Small” view should be visible
#      When I select "Show Large View" from the context menu
#      Then "Data Split" tool “Large” view should be visible
#	  When I click on "Open Quick Variable Input" on "Data Split"
#	  Then "Quick Variable Input" is visible
#	  And I enter variable "[[#$$#]]" in variable list
#	  And I click on "Split List In" dropdown
#	  When I select "New Line" in Split List On
#	  Then "Preview" button is "Enabled"
#	  And "Add" button is "Enabled"
#	  And select "Replace"
#	  When I click on "Add"
#	  Then "Data Split" tool “Large” view should be visible
#	  And I enter "String To Split" data "Warewolf"
#	  When I click "Done" on "Data Split"
#      Then "Errors" box is visible
#      Then I click on error 1
#      And the cursor is placed at "[[#$$#]]"
#      And I remove "[[#$$#]]" In row "1" 
#      And I enter variable "[[rec().a]]" in row "1"
#      Then I click "Done" on "Data Split"
#      And "Data Split" tool “Small” view should be visible
#
#Bug-11577 is created for this
#Scenario: Find Record Index Large View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Find Record Index" onto design surface
#       Then "Find Record Index" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#       Then "Find Record Index" tool “Large” view should be visible
#	   And I enter "In Field(s)" data "[[record(&^).a]]" 
#	   And I select Match Type "=" in row "1"
#	   And I enter Match "Test" in row "1"
#	   And I enter result variable "[[a]]" in "Find Record Index" 
#	   And I "Unselect" "Require All Matches To be True"
#	   And I "Select" "Require All Fields To Match"
#       When I click "Done" on "Find Record Index"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "[[record(&^).a]]"
#       And I remove "[[record(&^).a]]" In row "In Fields" 
#	   And I enter "In Field(s)" data "[[record(&^).a]]" 
#       When I click "Done" on "Find Record Index"
#       Then "Find Record Index" tool “Small” view should be visible
#
#Scenario: Find Record Index "Show Large View" Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Find Record Index" onto design surface
#       Then "Find Record Index" tool “Small” view should be visible
#	   And I enter "In Field(s)" data "[[record().a]]" 
#	   And I select Match Type "=" in row "1"
#	   And I enter Match "Test" in row "1"
#	   And I enter result variable "[[a]]" in "Find Record Index"
#     
#
#Scenario: Create Tool Large View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Create" onto design surface
#       Then "Create" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#	   And I enter "File or Folder" as "\test"
#	   And I select "If it exists" as "Yes"
#	   And I enter Username "" for "File or Folder"
#	   And I enter Password "" for "File or Folder"
#	   And I enter result variable "[[a]]" in "Create"
#       When I click "Done" on "Create"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "\test"
#	   And I remove ""\test" In row "File or Folder" 
#	   And I enter "File or Folder" as "c:\test"
#       When I click "Done" on "Create"
#       Then "Create" tool “Small” view should be visible
#
#Scenario: Create Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Create" onto design surface
#       Then "Create" tool “Small” view should be visible
#	   And I enter "File or Folder" as "c:\test"
#	   And I enter result variable "[[a]]" in "Create"
#
#
#Scenario: Delete Tool Large View" Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Delete" onto design surface
#       Then "Delete" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#	   And I enter "File or Folder" as "\test"
#	   And I enter Username "" for "File or Folder"
#	   And I enter Password "" for "File or Folder"
#	   And I enter result variable "[[a]]" in "Delete"
#       When I click "Done" on "Delete"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "\test"
#	   And I remove ""\test" In row "File or Folder" 
#	   And I enter File or Folder as "c:\test"
#       When I click "Done" on "Delete"
#       Then "Delete" tool “Small” view should be visible
#
#Scenario: Delete Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Delete" onto design surface
#       Then "Delete" tool “Small” view should be visible
#	   And I enter "File or Folder" as "c:\test"
#	   And I enter result variable "[[a]]" in "Delete"
#
#Scenario: Read Tool "Show Large View" Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Read" onto design surface
#       Then "Read" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#	   And I enter "File Name" as "\test"
#	   And I enter Username "" for "File or Folder"
#	   And I enter Password "" for "File or Folder"
#	   And I enter result variable "[[a]]" in "Read"
#       When I click "Done" on "Read"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "\test"
#	   And I remove ""\test" In row "File Name" 
#	   And I enter File Name as "c:\test"
#       When I click "Done" on "Read"
#       Then "Read" tool “Small” view should be visible
#
#Scenario: Read File Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Read" onto design surface
#       Then "Read" tool “Small” view should be visible
#	   And I enter "File Name" as "c:\test"
#	   And I enter result variable "[[a]]" in "Read"
#
#Scenario: Copy Tool Large View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Copy" onto design surface
#       Then "Copy" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#	   And I enter "File or Folder" as "\test"
#	   And I enter Username "" for "File or Folder"
#	   And I enter Password "" for "File or Folder"
#	   And I enter "Destination" as "c:\test"
#	   And I enter Username "" for "Destination"
#	   And I enter Password "" for "Destination"
#	   And I enter result variable "[[a]]" in "Read"
#       When I click "Done" on "Read"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "\test"
#	   And I remove ""\test" In row "File or Folder" 
#	   And I enter "File or Folder" as "c:\test"
#	   And I select "If it exists" as "Yes"
#       When I click "Done" on "Copy"
#       Then "Copy" tool “Small” view should be visible
#
#Scenario: Copy Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Copy" onto design surface
#       Then "Copy" tool “Small” view should be visible
#	   And I enter "File or Folder" as "c:\test"
#	   And I enter "Destination" as "c:\test123
#	   And I enter result variable "[[a]]" in "Copy"
#
#
#Scenario: Move Tool Large View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Move" onto design surface
#       Then "Move" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#	   And I enter "File or Folder" as "\test"
#	   And I enter Username "" for "File or Folder"
#	   And I enter Password "" for "File or Folder"
#	   And I enter "Destination" as "c:\test"
#	   And I enter Username "" for "Destination"
#	   And I enter Password "" for "Destination"
#	   And I enter result variable "[[a]]" in "Move"
#	    And I select "If it exists" as "Yes"
#       When I click "Done" on "Move"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "\test"
#	   And I remove ""\test" In row "File or Folder" 
#	   And I enter "File or Folder" "c:\test"
#       When I click "Done" on "Move"
#       Then "Move" tool “Small” view should be visible
#
#Scenario: Move Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Move" onto design surface
#       Then "Move" tool “Small” view should be visible
#	   And I enter "File or Folder" "c:\test"
#	   And I enter Destination "c:\test123
#	   And I enter result variable "[[a]]" in "Move"
#
#Scenario: Rename Tool Large View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Rename" onto design surface
#       Then "Rename" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#	   And I enter "File or Folder" as "\test"
#	   And I enter Username "" for "File or Folder"
#	   And I enter Password "" for "File or Folder"
#	   And I enter New Name "c:/Warewolf"
#	   And I enter Username "" for "Destination"
#	   And I enter Password "" for "Destination"
#	   And I enter result variable "[[a]]" in "Rename"
#	   And I select "If it exists" as "Yes"
#       When I click "Done" on "Rename"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "\test"
#	   And I remove ""\test" In row "File or Folder" 
#	   And I enter "File or Folder" as "c:\test"
#       When I click "Done" on "Rename"
#       Then "Rename" tool “Small” view should be visible
#
#Scenario: Rename Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Rename" onto design surface
#	   And I enter "File or Folder" as "c:\test"
#	   And I enter New Name as "c:\test123
#	   And I enter result variable "[[a]]" in "Rename"
#
#
#Scenario: Unzip Tool Large View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Unzip" onto design surface
#       Then "Unzip" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#	   And I enter "Zip Name" as "\test"
#	   And I enter Username "" for "File or Folder"
#	   And I enter Password "" for "File or Folder"
#	   And I enter "Destination" as "c:\test"
#	   And I enter Username "" for "Destination"
#	   And I enter Password "" for "Destination"
#	   And I enter result variable "[[a]]" in "Unzip"
#	   And I select "If it exists" as "Yes"
#       When I click "Done" on "Move"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "\test"
#	   And I remove "\test" In row "Unzip" 
#	   And I enter "Zip Name" as "c:\test"
#       When I click "Done" on "Unzip"
#       Then "Unzip" tool “Small” view should be visible
#
#Scenario: Unzip Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Unzip" onto design surface
#       Then "Unzip" tool “Small” view should be visible
#	   And I enter "File or Folder" as "c:\test"
#	   And I enter "Destination" as "c:\test123
#	   And I enter result variable "[[a]]" in "Unzip"
#
#
#Scenario: Zip Tool Large View Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Zip" onto design surface
#       Then "Zip" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#	   And I enter "Zip Name" as "\test"
#	   And I enter Username "" for "File or Folder"
#	   And I enter Password "" for "File or Folder"
#	   And I enter "Destination" as "c:\test"
#	   And I enter Username "" for "Destination"
#	   And I enter Password "" for "Destination"
#	   And I enter result variable "[[a]]" in "Unzip"
#	   And I select If it exists "Yes"
#       When I click "Done" on "Zip"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "\test"
#	   And I remove "\test" In row "Zip Name"
#	   And I enter "Zip Name" as "c:\test"
#       When I click "Done" on "Zip"
#       Then "Zip" tool “Small” view should be visible
#
#Scenario: Zip Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Zip" onto design surface
#       Then "Zip" tool “Small” view should be visible
#	   And I enter "File or Folder" as "c:\test"
#	   And I enter "Destination" as "c:\test123
#	   And I enter result variable "[[a]]" in "Zip"
#
#Scenario: Read Folder "Show Large View" Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Read Folder" onto design surface
#       Then "Read Folder" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#	   And I enter "Directory" as "\test"
#	   And I select read "Files"
#	   And I enter Username "" for "Directory"
#	   And I enter Password "" for "Directory"
#	   And I enter result variable "[[a]]" in "Read Folder"
#       When I click "Done" on "Read Folder"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "\test"
#	   And I remove "\test" In row "Directory"
#	   And I enter "Directory" as "c:\test"
#       When I click "Done" on "Read Folder"
#       Then "Read Folder" tool “Small” view should be visible
#
#Scenario: Read Folder File Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Read Folder" onto design surface
#       Then "Read Folder" tool “Small” view should be visible
#	   And I enter "Directory" as "c:\test"
#	   And I enter result variable "[[a]]" in "Read Folder"
#
#Scenario: Write File "Show Large View" Test
#       Given I have Warewolf running
#       And  I click on "New Workflow"
#       And I drag an "Write File" onto design surface
#       Then "Write File" tool “Small” view should be visible
#       When I select "Show Large View" from the context menu
#	   And I enter "File Name" as "\test"
#	   And I select Method "Append Top"
#	   And I enter Contents "Warewolf Testting"
#	   And I enter Username "" for "File Name"
#	   And I enter Password "" for "File Name"
#	   And I enter result variable "[[a]]" in "Write File"
#       When I click "Done" on "Write File"
#       Then "Errors" box is visible
#       When I click on error 1
#       Then the cursor is placed at "\test"
#	   And I remove"File Name" "\test"
#	   And I enter "File Name" as "c:\test"
#       When I click "Done" on "Write File"
#       Then "Write File" tool “Small” view should be visible
#
#Scenario: Write File Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Write File" onto design surface
#       Then "Write File" tool “Small” view should be visible
#	   And I enter "File Name" as "c:\test"
#	   And I enter Contents "Warewolf Testting"
#	   And I enter result variable "[[a]]" in "Read Folder"
#
#
#Scenario Outline: Recordset Tools Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an '<Tool>' onto design surface
#       Then '<Tool>' tool “Small” view should be visible
#	   And I enter "Recordset" "[[rec(1).a]]"
#	   And I enter result variable "[[Result]]" in '<Tool>'
#Examples: 
#     | No | Tool          |
#     | 1  | Count Records |
#     | 2  | Length        |
#     | 3  | Delete Record |
#     | 4  | Sort Records  |
#
#Scenario: Sort Records Tools Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Sort Records" onto design surface
#       Then "Sort Records" tool “Small” view should be visible
#	   And I enter "Sort Field" as "[[rec(1).a]]"
#	   And I Select Sort Order "Backwards"
#
#Scenario: Unique Records Tools Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Unique Records" onto design surface
#       Then "Unique Records" tool “Small” view should be visible
#	   And I press Tab "3" times
#	   And I enter "in Field(s)" as "[[rec(*).a]],[[rs(*).a]]"	
#	   And I press Tab "1" times
#	   And I enter "Return Fields" as "[[rec().a]]"
#	   And I press Tab "1" times
#	   And I enter result variable "[[Result]]" in "Unique Records"
#
#Scenario: Replace Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Replace" onto design surface
#       Then "Replace" tool “Small” view should be visible
#	   And I press Tab "3" times
#	   And I enter "in Field(s)" as "[[rec(*).a]],[[rs(*).a]]"	
#	   And I press Tab "1" times
#	   And I enter "Find" as"A"
#	   And I press Tab "1" times
#	   And I enter "Replace With" as "A"
#	   And I enter result variable "[[Result]]" in "Replace"
#		
#Scenario: Base Coversion Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Base Conversion" onto design surface
#       Then "Base Coversion" tool “Small” view should be visible
#	   And I press Tab "4" times
#	   And I enter variable "test" in row "1"
#	   And I press Tab "1" times
#	   And select "From" type "Text"
#	   And I press Tab "1" times
#	   And I select "To" type "Base64"
#	   
#Scenario: Base Convert Quick Variable Test
#      Given I have Warewolf running
#	  And I click on "New Workflow"
#	  And I drag an "Base Conversion" onto design surface
#	  Then "Base Conversion" tool “Small” view should be visible 
#	  When I click on "Open Quick Variable Input" on "Base Coversion"
#	  Then "Quick Variable Input" is visible
#	  And I enter variable "Warwolf" in variable list
#	  And I click on "Split List In" dropdown
#	  When I select "New Line" in Split List On
#	  Then "Preview" button is "Enabled"
#	  And "Add" button is "Enabled"
#	  And I select "Replace"
#	  When I click on "Add"
#	  Then "Base Coversion" tool “Small” view should be visible
#
#Scenario: Case Coversion Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Case Conversion" onto design surface
#       Then "Case Coversion" tool “Small” view should be visible
#	   And I press Tab "4" times
#	   And I enter variable "test" in row "1"
#	   And I press Tab "1" times
#	   And I select "Case"  as "UPPER"
#	   
#Scenario: Case Convert Quick Variable Test
#      Given I have Warewolf running
#	  And I click on "New Workflow"
#	  And I drag an "Case Conversion" onto design surface
#	  Then "Case Conversion" tool “Small” view should be visible 
#	  When I click on "Open Quick Variable Input" on "Base Coversion"
#	  Then "Quick Variable Input" is visible
#	  And I enter variable "Warwolf" in variable list
#	  And I click on "Split List In" dropdown
#	  When I select "New Line" in Split List On
#	  Then "Preview" button is "Enabled"
#	  And "Add" button is "Enabled"
#	  And I select "Replace"
#	  When I click on "Add"
#	  Then "Case Coversion" tool “Small” view should be visible
#
#Scenario: Find Index Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Find Index" onto design surface
#       Then "Find Index" tool “Small” view should be visible
#	   And I press Tab "3" times
#	   And I enter "In Field" as "[[rec(*).a]]"
#	   And I press Tab "1" times
#	   And I select "Index"  as "All Occurrence"
#	   And I enter "Character" as "a"
#	   And I select "Direction"  as "Left to Right"
#	   And I enter result variable "[[Result]]" in "Find Index"
#
#Scenario: Calculate Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Calculate" onto design surface
#       Then "Calculate" tool “Small” view should be visible
#	   And I press Tab "3" times
#	   And I enter "2+3" in "Calculate"
#	   And I enter result variable "[[Result]]" in "Find Index" 
#
#Scenario: Format Number Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Format Number" onto design surface
#       Then "Format Number" tool “Small” view should be visible
#	   And I press Tab "3" times
#	   And I enter "Number" as "153.6235"
#	   And I press Tab "1" times
#	   And I select "Rounding"  as "None"
#	   And I enter "Decimal to show" as "3"
#	   And I enter result variable "[[Result]]" in "Find Index"
#
#
#Scenario: Random Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Random" onto design surface
#       Then "Random" tool “Small” view should be visible
#	   And I press Tab "3" times
#	   And I select "Type" is "Number"
#	   And I press Tab "1" times
#	   And I enter "Range" from "1" to "10"
#	   And I press Tab "1" times
#	   And I enter result variable "[[Result]]" in "Random"
#
#Scenario: Date and Time Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Date and Time" onto design surface
#       Then "Date and Time" tool “Small” view should be visible
#	   And I press Tab "3" times
#	   And I enter "Input" as "23/07/2014"
#	   And I press Tab "1" times
#	   And I enter "Input Format" as "dd/MM/yyyy"
#	   And I select "Add Time" as "Years"
#	   And I enter "Output Format" as "dd/MM/yyyy"
#	   And I enter result variable "[[Result]]" in "Date and Time"
#
#Scenario: Date and Time Difference Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an " Date and Time Difference" onto design surface
#       Then " Date and Time Difference" tool “Small” view should be visible
#	   And I press Tab "3" times
#	   And I enter "Input1" as "23/07/2014"
#	   And I press Tab "1" times
#	   And I enter "Input2" as "23/07/2014"
#	   And I press Tab "1" times
#	   And I enter "Input Format" as "dd/MM/yyyy"
#	   And I press Tab "1" times
#	   And I select "Output In" as "Years"
#	   And I press Tab "1" times
#	   And I enter "Output Format" as "dd/MM/yyyy"
#	   And I press Tab "1" times
#	   And I enter result variable "[[Result]]" in "Date and Time"
#
#
#Scenario: Gather System Information Tool Quick Variable Test
#      Given I have Warewolf running
#	  And I click on "New Workflow"
#	  And I drag an "Gather System Information" onto design surface
#	  Then "Gather System Information" tool “Small” view should be visible 
#	  When I click on "Open Quick Variable Input" on "Gather System Information"
#	  Then "Quick Variable Input" is visible
#	  And I enter variable "[[rec(1).a]]" in variable list
#	  And I click on "Split List In" dropdown
#	  When I select "New Line" in Split List On
#	  Then "Preview" button is "Enabled"
#	  And "Add" button is "Enabled"
#	  And I select "Replace"
#	  When I click on "Add"
#	  Then "Gather System Information" tool “Small” view should be visible
#
#Scenario:Gather System Information Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Gather System Information" onto design surface
#       Then "Gather System Information" tool “Small” view should be visible
#	   And I press Tab "4" times
#	   And I enter varaible "[[rec(1).a]]" in row "1"
#	   And I press Tab "1" times
#	   And I select "Date&Time" 
#	   
#Scenario:For Each Tool
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "For Each" onto design surface
#       Then "GFor Each" tool “Small” view should be visible
#	   And I press Tab "4" times
#	   And I select "In Range" from "1" to "10"
#	   And I select "In CSV" as "12343"
#	   And I select "No. of Executes" as "3"
#	   And I select "in Recordset" as "3"
#	   
#Scenario: Web Request Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Web Request" onto design surface
#       Then "Web Request" tool “Small” view should be visible
#	   And I press Tab "3" times
#	   And I enter "https://google.com" in "URL"
#	   And I enter result variable "[[Result]]" in "Find Index" 
#
#Scenario: Execute Command Line Tool Small View Test
#       Given I have Warewolf running
#       Then  I click on "New Workflow"
#       When I drag an "Execute Command Line" onto design surface
#       Then "Execute Command Line" tool “Small” view should be visible
#	   And I press Tab "3" times
#	   And I enter "sdfsfdsfdsssd" in "CMD"
#	   And I enter result variable "[[Result]]" in "Find Index" 
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
