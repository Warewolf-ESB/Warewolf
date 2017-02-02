Feature: DefaultIntellisense
	In order to insert variables from my variable list into a textbox
	As a Warewolf user
	I want to be able to select it from the intellisense drop down

Scenario Outline: Insert for All FilterType and DateTime Provider
	Given I have the following variable list '<varlist>'
	And the filter type is '<filterType>'
	And the current text in the textbox is '<input>'
	And the cursor is at index '<index>'		
	And the provider used is '<provider>'	
	And the drop down list as '<dropDownList>'		
	When I select the following option '<option>'
	Then the result text should be '<result>'
	And the caret position will be '<caretposition>'
	Examples: 
	| testName | varlist                        | filterType | input     | index | dropDownList                                                             | option          | result              | provider          | caretposition |
	| 1        | <var/><var2/><rec><var/></rec> | All        | a         | 1     | [[rec(*).var]],[[rec().var]],[[var]],[[var2]],am/pm                      | am/pm           | am/pm               | Default, DateTime | 5             |
	| 2        | <ww/><min/><rec><minss/></rec> | All        | m         | 1     | [[rec(*).minss]],[[rec().minss]],[[min]],m,M,min,mm,MM                   | min             | min                 | Default, DateTime | 3             |
	| 3        | <ww/><min/><rec><minss/></rec> | All        | text[[m]] | 7     | [[rec(*).minss]],[[rec().minss]],[[min]]                                | [[rec().minss]] | text[[rec().minss]] | Default, DateTime | 19            |
	| 5        | <ww/><min/><rec><minss/></rec> | All        | text mi   | 7     | [[rec(*).minss]],[[rec().minss]],[[min]],min                             | min             | text min            | Default, DateTime | 8             |
	| 6        | <ww/><min/><rec><minss/></rec> | All        | text m    | 6     | [[rec(*).minss]],[[rec().minss]],[[min]],m,M,min,mm,MM                   | min             | text min            | Default, DateTime | 8             |
	| 7        | <ww/><min/><rec><minss/></rec> | All        | text mute | 6     | [[rec(*).minss]],[[rec().minss]],[[min]],m,M,min,mm,MM                   | min             | text min            | Default, DateTime | 8             |
	| 8        | <ww/><min/><rec><minss/></rec> | All        | text mute | 6     | [[rec(*).minss]],[[rec().minss]],[[min]],m,M,min,mm,MM                   | [[min]]         | text [[min]]        | Default, DateTime | 12            |
	| 9        | <ww/><min/><rec><minss/></rec> | All        | [[min]]y  | 8     | yy,yyyy                                                                  | yy              | [[min]]yy           | Default, DateTime | 9             |
	
Scenario Outline: Insert for All FilterType and File Provider
	Given I have the following variable list '<varlist>'
	And the file path structure is '<pathStructure>'
	And the filter type is '<filterType>'
	And the current text in the textbox is '<input>'
	And the cursor is at index '<index>'		
	And the provider used is '<provider>'	
	And the drop down list as '<dropDownList>'		
	When I select the following option '<option>'
	Then the result text should be '<result>'
	And the caret position will be '<caretposition>'
	Examples: 
	| testName | pathStructure                       | varlist                       | filterType | input              | index | dropDownList                                                            | option               | result                              | provider      | caretposition |
	| 1        |                                     | <myfile/><file><name/></file> | All        | c:\[[fil           | 8     | [[file(*).name]],[[file(*)]],[[file().name]],[[file()]],[[myfile]]                  | [[myfile]]           | c:\[[myfile]]                       | Default, File | 13            |
	| 2        |                                     | <myfile/><file><name/></file> | All        | c:\[[fil]]         | 8     | [[file(*).name]],[[file(*)]],[[file().name]],[[file()]],[[myfile]] | [[myfile]]           | c:\[[myfile]]                       | Default, File | 13            |
	| 3        |                                     | <myfile/><file><name/></file> | All        | c:\[[fil]]         | 8     | [[file(*).name]],[[file(*)]],[[file().name]],[[file()]],[[myfile]] | [[file().name]]      | c:\[[file().name]]                  | Default, File | 18            |
	| 4        |                                     | <myfile/><file><name/></file> | All        | c:\[[myfile]][[    | 13    |                                                                         |                      | c:\[[myfile]][[                     | Default, File | 13            |
	| 5        |                                     | <myfile/><file><name/></file> | All        | c:\[[myfile]][[fil | 18    | [[file(*).name]],[[file(*)]],[[file().name]],[[file()]],[[myfile]] | [[file().name]]      | c:\[[myfile]][[file().name]]        | Default, File | 28            |
	| 6        |                                     | <myfile/><file><name/></file> | All        | [[myfile]].        | 11    | [[file(*).name]],[[file().name]]                                        |                      | [[myfile]].                         | Default, File | 11            |
	| 7        | c:\,c:\FolderA,c:\FolderA\FileA.txt |                               | All        | del c              | 5     | c:\,c:\FolderA,c:\FolderA\FileA.txt                                     | c:\FolderA\FileA.txt | del c:\FolderA\FileA.txt            | Default, File | 24            |
	| 9        | c:\,c:\FolderA,c:\FolderA\FileA.txt |                               | All        | del c:\FolderA c   | 16    | c:\,c:\FolderA,c:\FolderA\FileA.txt                                     | c:\FolderA\FileA.txt | del c:\FolderA c:\FolderA\FileA.txt | Default, File | 35            |
	| 10       | c:\,c:\FolderA,c:\FolderA\FileA.txt | <c/><cd/>                     | All        | del c:\FolderA c   | 16    | [[c]],[[cd]],c:\,c:\FolderA,c:\FolderA\FileA.txt                        | [[cd]]               | del c:\FolderA [[cd]]               | Default, File | 21            |
	| 11       | c:\,c:\FolderA,c:\FolderA\FileA.txt | <c/><cd/>                     | All        | del c:\FolderA\    | 15    | c:\FolderA\FileA.txt                                                    | c:\FolderA\FileA.txt | del c:\FolderA\FileA.txt            | Default, File | 24            |

Scenario Outline: Insert for All FilterType and Calculate Provider
	Given I have the following variable list '<varlist>'
	And the filter type is '<filterType>'
	And the current text in the textbox is '<input>'
	And the cursor is at index '<index>'		
	And the provider used is '<provider>'	
	And the drop down list as '<dropDownList>'		
	When I select the following option '<option>'
	Then the result text should be '<result>'
	And the caret position will be '<caretposition>'
	Examples: 
	| testName | varlist                   | filterType | input                  | index | dropDownList                  | option  | result                  | provider           | caretposition |
	| 1        | <att/><sum><b/></sum>     | All        | at                     | 2     | [[att]],atan,atan2,atanh      | atan    | atan                    | Default, Calculate | 4             |
	| 2        | <att/><sum><b/></sum>     | All        | tan(at                 | 6     | [[att]],atan,atan2,atanh      | [[att]] | tan([[att]]             | Default, Calculate | 11            |
	| 3        | <att/><sum><b/></sum>     | All        | tan([[at               | 8     | [[att]]                       | [[att]] | tan([[att]]             | Default, Calculate | 11            |
	| 4        | <att/><sum><b/></sum>     | All        | tan([[at]]             | 8     | [[att]]                       | [[att]] | tan([[att]]             | Default, Calculate | 11            |
	| 5        | <att/><sum><b/></sum>     | All        | tan([[at]])            | 8     | [[att]]                       | [[att]] | tan([[att]])            | Default, Calculate | 11            |
	| 6        | <att/><sum><b/></sum>     | All        | tan([[at]])            | 11    |                               |         | tan([[at]])             | Default, Calculate | 11            |
	| 7        | <a/><b/><c/>              | All        | =[[b]]+b               | 8     | [[b]],bin2dec,bin2hex,bin2oct | [[b]]   | =[[b]]+[[b]]            | Default, Calculate | 12            |
	| 8        | <a/><att/><sum><b/></sum> | All        | tan([[at]]) tan([[a]]) | 8     | [[att]]                       | [[att]] | tan([[att]]) tan([[a]]) | Default, Calculate | 11            |

Scenario Outline: Recset only has no errors for valid variable indexes
	Given I have the following variable list '<varlist>'
	And the filter type is 'RecordsetsOnly'
	And the current text in the textbox is '<input>'
	And the cursor is at index '11'		
	And the provider used is 'Default'	
	Then the result has '<expectError>' errors
Examples: 	
	| testName | varlist                            | expectError | input             |
	| 1        | <x/><sum><b/></sum><mus><b/></mus> | true       | [[sum([[x]])]]    |
	| 1        | <x/><sum><b/></sum><mus><b/></mus> | true        | [[sum([[assc]])]] |
	
Scenario Outline: Insert for All FilterType and Default Provider
	Given I have the following intellisense options '<varlist>'
	And the filter type is '<filterType>'
	And the current text in the textbox is '<input>'
	And the cursor is at index '<index>'		
	And the provider used is '<provider>'	
	And the suggestion list as '<dropDownList>'		
	When I select the following string option '<option>'
	Then the result text should be "<result>" with caret position will be '<caretposition>'
Examples: 
	| testName | varlist                                                                                | filterType | input                 | index | dropDownList                                                                 | option          | result                 | provider | caretposition |
	| 1        | [[var]],[[var2]],[[rec()]],[[rec().var]],[[rec().var2]]                                | All        | text var              | 8     | [[var]],[[var2]],[[rec().var]],[[rec().var2]]                                | [[var]]         | text [[var]]           | Default  | 12            |
	| 2        | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | text[[var             | 9     | [[var]],[[var2]],[[rec().var]],[[rec().var2]]                                | [[var2]]        | text[[var2]]           | Default  | 12            |
	| 3        | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | text[[rec().var]]     | 15    | [[rec().var]],[[rec().var2]]                                                 | [[rec().var2]]  | text[[rec().var2]]     | Default  | 18            |
	| 4        | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | [[                    | 2     | [[var]],[[var2]],[[rec().var]],[[rec().var2]]                                | [[var2]]        | [[var2]]               | Default  | 8             |
	| 5        | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | text[[rec().[[var     | 17    | [[var]],[[var2]]                                                             | [[var2]]        | text[[rec().[[var2]]   | Default  | 20            |
	| 6        | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | text[[rec().[[var]]   | 17    | [[var]],[[var2]]                                                             | [[var2]]        | text[[rec().[[var2]]   | Default  | 20            |
	| 7        | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | text[[rec().[[var]]]] | 17    | [[var]],[[var2]]                                                             | [[var2]]        | text[[rec().[[var2]]]] | Default  | 20            |
	| 8        | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec(*).var2]],[[rec().var]],[[rec(*).var]] | All        | [[var]]v              | 8     | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]] | [[rec(*).var]]  | [[var]][[rec(*).var]]  | Default  | 21            |
	| 9        | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | [[var]][[             | 9     | [[var]],[[var2]],[[rec().var]],[[rec().var2]]                                | [[rec().var]]   | [[var]][[rec().var]]   | Default  | 20            |
	| 10       | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec(*).var2]],[[rec().var]],[[rec(*).var]] | All        | [[var]][[var          | 12    | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]] | [[rec(*).var2]] | [[var]][[rec(*).var2]] | Default  | 22            |
	| 11       | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec(*).var2]],[[rec().var]],[[rec(*).var]] | All        | [[var]][[var]]        | 12    | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]] | [[rec(*).var2]] | [[var]][[rec(*).var2]] | Default  | 22            |
	| 12       | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | [[var]]               | 7     |                                                                              |                 | [[var]]                | Default  | 7             |
	| 13       | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | [[var]] text          | 13    |                                                                              |                 | [[var]] text           | Default  | 13            |
	| 14       | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | text[[var2]]text      | 10    | [[var2]],[[rec().var2]]                                                      | [[rec().var2]]  | text[[rec().var2]]text | Default  | 18            |
	| 15       | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | r                     | 1     | [[var]],[[var2]],[[rec().var]],[[rec().var2]]                                | [[rec().var2]]  | [[rec().var2]]         | Default  | 14            |
	| 16       | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | [[rec([[va]]).var]]   | 10    | [[var]],[[var2]],[[rec().var]],[[rec().var2]]                                | [[var]]         | [[rec([[var]]).var]]   | Default  | 13            |
	| 17       | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | [[[[a]]]]             | 5     | [[var]],[[var2]],[[rec().var]],[[rec().var2]]                                | [[var]]         | [[var]]]]              | Default  | 7             |
	| 18       | [[var]],[[var2]],[[rec()]],[[rec().var2]],[[rec().var]]                                | All        | [[                    | 2     | [[var]],[[var2]],[[rec().var]],[[rec().var2]]                                | [[var]]         | [[var]]                | Default  | 7             |

Scenario Outline: Insert for RecordsetsOnly FilterType and Default Provider
	Given I have the following intellisense options '<varlist>'
		And the filter type is '<filterType>'
	And the current text in the textbox is '<input>'
	And the cursor is at index '<index>'		
	And the provider used is '<provider>'	
	And the suggestion list as '<dropDownList>'		
	When I select the following string option '<option>'
	Then the result text should be "<result>" with caret position will be '<caretposition>'
	Examples: 
	| testName | varlist                                                               | filterType      | input                  | index | dropDownList                                      | option       | result                  | provider | caretposition |
	| 1        | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]]                     | RecordsetsOnly  | u                      | 1     | [[sum()]],[[mus()]]                               | [[sum()]]    | [[sum()]]               | Default  | 9             |
	| 2        | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]]                     | RecordsetsOnly  | b                      | 1     |                                                   |              | b                       | Default  | 1             |
	| 3        | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]]                     | RecordsetsOnly  | [[()]]                 | 3     |                                                   |              | [[()]]                  | Default  | 3             |
	| 4        | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]]                     | RecordsetsOnly  | [[sum()]] s            | 11    | [[sum()]],[[mus()]]                               |              | [[sum()]] s             | Default  | 11            |
	| 5        | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]]                     | RecordsetsOnly  | [[sum()]] [[           | 12    | [[sum()]],[[mus()]]                               | [[mus()]]    | [[sum()]] [[mus()]]     | Default  | 19            |
	| 6        | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]]                     | RecordsetsOnly  | rec s                  | 5     | [[sum()]],[[mus()]]                               | [[mus()]]    | rec [[mus()]]           | Default  | 13            |
	| 7        | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]],[[sum().ba]]        | RecordsetFields | [[sum().b]]            | 9     | [[sum().b]],[[sum().ba]]                          | [[sum().ba]] | [[sum().ba]]            | Default  | 12            |
	| 8        | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]],[[sum().ba]]        | All             | [[sum(b).b]]           | 7     | [[b]],[[sum().b]],[[sum().ba]],[[mus().b]]        | [[b]]        | [[sum([[b]]).b]]        | Default  | 11            |
	| 9        | [[ba]],[[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]],[[sum().ba]] | All             | [[sum([[b]]).b]]       | 14    | [[sum().b]],[[sum().ba]]                          | [[sum().ba]] | [[sum().ba]]            | Default  | 12            |
	| 10       | [[ba]],[[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]],[[sum().ba]] | All             | [[sum([[b]]).b]]       | 14    | [[sum().b]],[[sum().ba]]                          | [[sum().b]]  | [[sum().b]]             | Default  | 11            |
	| 11       | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]]                     | RecordsetsOnly  | u u                    | 3     | [[sum()]],[[mus()]]                               | [[sum()]]    | u [[sum()]]             | Default  | 11            |
	| 12       | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]]                     | RecordsetsOnly  | [[sum()]] s            | 11    | [[sum()]],[[mus()]]                               | [[mus()]]    | [[sum()]] [[mus()]]     | Default  | 19            |
	| 13       | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]],[[sum().ba]]        | RecordsetFields | [[sum().b]][[sum().b]] | 20    | [[sum().b]],[[sum().ba]],[[mus().b]]              | [[sum().ba]] | [[sum().b]][[sum().ba]] | Default  | 23            |
	| 14       | [[ba]],[[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]],[[sum().ba]] | All             | [[sum().b]]            | 9     | [[sum().b]],[[sum().ba]]                          | [[sum().ba]] | [[sum().ba]]            | Default  | 12            |
	| 15       | [[ba]],[[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]],[[sum().ba]] | All             | [[sum().b]]b           | 12    | [[b]],[[ba]],[[sum().b]],[[sum().ba]],[[mus().b]] | [[ba]]       | [[sum().b]][[ba]]       | Default  | 17            |
	| 16       | [[b]],[[sum().b]],[[mus().b]],[[sum()]],[[mus()]]                     | RecordsetsOnly  | =u                     | 2     | [[sum()]],[[mus()]]                               | [[sum()]]    | =[[sum()]]              | Default  | 10            |


Scenario Outline: Insert for Json FilterType and Default Provider
	Given I have the following intellisense options '<varlist>'
		And the filter type is '<filterType>'
	And the current text in the textbox is '<input>'
	And the cursor is at index '<index>'		
	And the provider used is '<provider>'	
	And the suggestion list as '<dropDownList>'		
	When I select the following string option '<option>'
	Then the result text should be "<result>" with caret position will be '<caretposition>'
	Examples: 
	| testName | varlist                                                             | filterType | input               | index | dropDownList                                                        | option                     | result                                      | provider | caretposition |
	| 1        | [[@Person]],[[@Person.name]]                                        | JsonObject | @P                  | 2     | [[@Person.name]],[[@Person]]                                        | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 2        | [[@Person]],[[@Person.name]]                                        | JsonObject | [[                  | 3     | [[@Person.name]],[[@Person]]                                        | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 3        | [[@Person]],[[@Person.name]]                                        | JsonObject | @Person.            | 8     | [[@Person.name]]                                                    | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 4        | [[@Person]],[[@Person.name]]                                        | JsonObject | [[@Person.          | 10    | [[@Person.name]]                                                    | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 5        | [[@Person]],[[@Person.name]]                                        | JsonObject | [[@Person.name      | 15    | [[@Person.name]]                                                    | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 6        | [[@Person]],[[@Person.name]]                                        | JsonObject | name                | 4     | [[@Person.name]]                                                    | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 7        | [[@Person]],[[@Person.name]]                                        | JsonObject | [                   | 1     | [[@Person.name]],[[@Person]]                                        | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 8        | [[@Person]],[[@Person.name]]                                        | JsonObject | rson                | 3     | [[@Person.name]],[[@Person]]                                        | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 9        | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | @P                  | 2     | [[@Person.Age]],[[@Person.name]],[[@Person]]                        | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 10       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | @P                  | 2     | [[@Person.Age]],[[@Person.name]],[[@Person]]                        | [[@Person.Age]]            | [[@Person.Age]]                             | Default  | 15            |
	| 11       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | [[                  | 2     | [[@Person.Age]],[[@Person.name]],[[@Person]]                        | [[@Person.Age]]            | [[@Person.Age]]                             | Default  | 15            |
	| 12       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | [[                  | 2     | [[@Person.Age]],[[@Person.name]],[[@Person]]                        | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 14       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | @Person.            | 8     | [[@Person.Age]],[[@Person.name]]                                    | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 15       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | @Person.            | 8     | [[@Person.Age]],[[@Person.name]]                                    | [[@Person.Age]]            | [[@Person.Age]]                             | Default  | 15            |
	| 16       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | [[@Person.          | 10    | [[@Person.Age]],[[@Person.name]]                                    | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 17       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | [[@Person.          | 10    | [[@Person.name]],[[@Person.Age]]                                    | [[@Person.Age]]            | [[@Person.Age]]                             | Default  | 15            |
	| 18       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | [[@Person.name      | 15    | [[@Person.name]]                                                    | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 19       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | [[@Person.Age       | 10    | [[@Person.Age]]                                                     | [[@Person.Age]]            | [[@Person.Age]]                             | Default  | 15            |
	| 20       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | name                | 4     | [[@Person.name]]                                                    | [[@Person.name]]           | [[@Person.name]]                            | Default  | 16            |
	| 21       | [[@Person]],[[@Person.name]],[[@Person.Age]]                        | JsonObject | Age                 | 3     | [[@Person.Age]]                                                     | [[@Person.Age]]            | [[@Person.Age]]                             | Default  | 15            |
	| 22       | [[@Person]],[[@Person.name]],[[@Person.Age]],[[@Person.Child.Name]] | JsonObject | Name                | 4     | [[@Person.Child.Name]]                                              | [[@Person.Child.Name]]     | [[@Person.Child.Name]]                      | Default  | 22            |
	| 23       | [[@Person]],[[@Person.name]],[[@Person.Age]],[[@Person.Child.Name]] | JsonObject | Name                | 1     | [[@Person.Child.Name]]                                              | [[@Person.Child.Name]]     | [[@Person.Child.Name]]                      | Default  | 22            |
	| 25       | [[@Person]],[[@Person.name]],[[@Person.Age]],[[@Person.Child.Name]] | JsonObject | Junk P              | 6     | [[@Person.Age]],[[@Person.name]],[[@Person]],[[@Person.Child.Name]] | [[@Person.Child.Name]]     | Junk [[@Person.Child.Name]]                 | Default  | 27            |
	| 26       | [[@Person]],[[@Person.name]],[[@Person.Age]],[[@Person.Child.Name]] | JsonObject | [[Junk]] P          | 10    | [[@Person.Age]],[[@Person.name]],[[@Person]],[[@Person.Child.Name]] | [[@Person.Child.Name]]     | [[Junk]] [[@Person.Child.Name]]             | Default  | 31            |
	| 28       | [[@Person]],[[@Person.name]],[[@Person.Age]],[[@Person.Child.Name]] | JsonObject | ]] P                | 4     | [[@Person.Age]],[[@Person.name]],[[@Person]],[[@Person.Child.Name]] | [[@Person.Child.Name]]     | ]] [[@Person.Child.Name]]                   | Default  | 25            |
	| 29       | [[@Person]],[[@Person.name]],[[@Person.Age]],[[@Person.Child.Name]] | JsonObject | ( P                 | 3     | [[@Person.Age]],[[@Person.name]],[[@Person]],[[@Person.Child.Name]] | [[@Person.Child.Name]]     | ( [[@Person.Child.Name]]                    | Default  | 24            |
	| 30       | [[@Person]],[[@Person.name]],[[@Person.Age]],[[@Person.Child.Name]] | JsonObject | ) P                 | 3     | [[@Person.Age]],[[@Person.name]],[[@Person]],[[@Person.Child.Name]] | [[@Person.Child.Name]]     | ) [[@Person.Child.Name]]                    | Default  | 24            |
	| 35       | [[@Person]],[[@Person.name]],[[@Person.Age]],[[@Person.Child.Name]] | JsonObject | . P [[              | 3     | [[@Person.Age]],[[@Person.name]],[[@Person.Child.Name]]             | [[@Person.Child.Name]]     | . [[@Person.Child.Name]] [[                 | Default  | 25            |
	| 39       | [[@Person]],[[@Person.Childs(*).name]]                              | JsonObject | [[P                 | 3     | [[@Person]],[[@Person.Childs(*).name]]                              | [[@Person.Childs(*).name]] | [[@Person.Childs(*).name]]                  | Default  | 26            |
	| 41       | [[@Person]],[[@Person.Childs(*).name]],[[@Person.Childs().name]]    | JsonObject | [[@Person.Childs(*  | 19    | [[@Person.Childs(*).name]]                                          | [[@Person.Childs(*).name]] | [[@Person.Childs(*).name]]                  | Default  | 26            |
	| 42       | [[@Person]],[[@Person.Childs(*).name]],[[@Person.Childs().name]]    | JsonObject | [[@Person.Childs([[ | 19    | [[@Person.Childs(*).name]],[[@Person.Childs().name]]                | [[@Person]]                | [[@Person.Childs([[@Person]]                | Default  | 28            |
	| 43       | [[@Person]],[[@Person.Childs(*).name]],[[@Person.Childs().name]]    | JsonObject | [[@Person.Childs([[ | 19    | [[@Person.Childs(*).name]],[[@Person.Childs().name]]                | [[@Person.Childs(*).name]] | [[@Person.Childs([[@Person.Childs(*).name]] | Default  | 43            |
	| 44       | [[@Person]],[[@Person.Childs(*).name]],[[@Person.Childs().name]]    | JsonObject | @                   | 1     | [[@Person.Childs(*).name]],[[@Person.Childs().name]]                | [[@Person.Childs(*).name]] | [[@Person.Childs(*).name]]                  | Default  | 26            |
	| 45       | [[@Person]],[[@Person.Childs(*).name]],[[@Person.Childs().name]]    | JsonObject | [[@                 | 3     | [[@Person.Childs(*).name]],[[@Person.Childs().name]]                | [[@Person.Childs(*).name]] | [[@Person.Childs(*).name]]                  | Default  | 26            |



Scenario Outline: Insert for RecordsetFields FilterType and Default Provider
	Given I have the following intellisense options '<varlist>'
		And the filter type is '<filterType>'
	And the current text in the textbox is '<input>'
	And the cursor is at index '<index>'		
	And the provider used is '<provider>'	
	And the suggestion list as '<dropDownList>'		
	When I select the following string option '<option>'
	Then the result text should be "<result>" with caret position will be '<caretposition>'
	Examples: 
	| testName | varlist                                                                                           | filterType      | input                   | index | dropDownList                                      | option       | result                  | provider | caretposition |
	| 1        | [[a]],[[rec()]],[[rec().a]],[[set()]],[[set().z]],[[rec(*)]],[[rec(*).a]],[[set(*)]],[[set(*).z]] | RecordsetFields | a                       | 1     | [[rec().a]],[[rec(*).a]]                          | [[rec(*).a]] | [[rec(*).a]]            | Default  | 12            |
	| 2        | [[a]],[[rec()]],[[rec().a]],[[set()]],[[set().z]],[[rec(*)]],[[rec(*).a]],[[set(*)]],[[set(*).z]] | RecordsetFields | b                       | 1     |                                                   |              | b                       | Default  | 1             |
	| 3        | [[a]],[[rec()]],[[rec().a]],[[set()]],[[set().z]],[[rec(*)]],[[rec(*).a]],[[set(*)]],[[set(*).z]] | RecordsetFields | [[()]]                  | 3     |                                                   |              | [[()]]                  | Default  | 3             |
	| 4        | [[a]],[[rec()]],[[rec().a]],[[set()]],[[set().z]],[[rec(*)]],[[rec(*).a]],[[set(*)]],[[set(*).z]] | RecordsetFields | [[rec().a]]             | 11    |                                                   |              | [[rec().a]]             | Default  | 11            |
	| 5        | [[rec()]],[[rec().a]],[[a]],[[rec().z]],[[rec(*)]],[[rec(*).a]],[[rec(*).z]]                      | RecordsetFields | [[rec().a]]             | 5     | [[rec().a]],[[rec(*).a]],[[rec().z]],[[rec(*).z]] | [[rec().z]]  | [[rec().z]]             | Default  | 11            |
	| 6        | [[rec()]],[[rec().a]],[[a]],[[rec().z]],[[rec(*)]],[[rec(*).a]],[[rec(*).z]]                      | RecordsetFields | [[rec                   | 5     | [[rec().a]],[[rec(*).a]],[[rec().z]],[[rec(*).z]] | [[rec().z]]  | [[rec().z]]             | Default  | 11            |
	| 7        | [[a]],[[rec()]],[[rec().a]],[[set()]],[[set().z]],[[rec(*)]],[[rec(*).a]],[[set(*)]],[[set(*).z]] | RecordsetFields | rec e                   | 5     | [[rec().a]],[[rec(*).a]],[[set().z]],[[set(*).z]] | [[rec().a]]  | rec [[rec().a]]         | Default  | 15            |
	| 8        | [[a]],[[rec()]],[[rec().a]],[[set()]],[[set().z]],[[rec(*)]],[[rec(*).a]],[[set(*)]],[[set(*).z]] | RecordsetFields | a a                     | 1     | [[rec().a]],[[rec(*).a]]                          | [[rec(*).a]] | [[rec(*).a]] a          | Default  | 12            |
	| 9        | [[a]],[[rec()]],[[rec().a]],[[set()]],[[set().z]],[[rec(*)]],[[rec(*).a]],[[set(*)]],[[set(*).z]] | RecordsetFields | b b                     | 1     |                                                   |              | b b                     | Default  | 1             |
	| 10       | [[rec()]],[[rec().a]],[[a]],[[rec().z]],[[rec(*)]],[[rec(*).a]],[[rec(*).z]]                      | RecordsetFields | [[rec().a]],[[rec().a]] | 5     | [[rec().a]],[[rec(*).a]],[[rec().z]],[[rec(*).z]] | [[rec().z]]  | [[rec().z]],[[rec().a]] | Default  | 11            |
	| 11       | [[rec()]],[[rec().a]],[[a]],[[rec().z]],[[rec(*)]],[[rec(*).a]],[[rec(*).z]]                      | RecordsetFields | [[rec [[rec().a]]       | 5     | [[rec().a]],[[rec(*).a]],[[rec().z]],[[rec(*).z]] | [[rec().z]]  | [[rec().z]] [[rec().a]] | Default  | 12            |


Scenario Outline: Validation messages when Invalid Variables  
	Given the current text in the textbox is '<Variable>'
	And the provider used is 'Default'	
	Then the result has the error '<Error>'
Examples: 
	| No | Variable                                  | Error                                                                                                                |
	| 1  | [[my(-1).var]]                            | Recordset index [ -1 ] is not greater than zero                                                                      |
	| 2  | [[rec"()".a]]                             | Variable name [[rec"()".a]] contains invalid character(s). Only use alphanumeric _ and -                             |
	| 3  | [[rec.a]]                                 | Variable name [[rec.a]] contains invalid character(s). Only use alphanumeric _ and -                                 |
	| 4  | [[1]]                                     | Recordset field [[1]] begins with a number                                                                           |
	| 5  | [[@]]                                     | Variable name [[@]] contains invalid character(s). Only use alphanumeric _ and -                                     |
	| 6  | [[var#]]                                  | Variable name [[var#]] contains invalid character(s). Only use alphanumeric _ and -                                  |
	| 7  | [[var]]00]]                               | Invalid region detected: A close ]] without a related open [[                                                        |
	| 8  | [[]]                                      | Variable name [[]] contains invalid character(s). Only use alphanumeric _ and -                                      |
	| 9  | [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] | Variable name [[r(q).a]][[r()..]][[r"]][[r()]][[]][[1]] contains invalid character(s). Only use alphanumeric _ and - |
	| 10 | [[var]]00[[                               | Invalid region detected: An open [[ without a related close ]]                                                       |
