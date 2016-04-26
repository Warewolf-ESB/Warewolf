Feature: DefaultIntellisense
	In order to insert variables from my variable list into a textbox
	As a Warewolf user
	I want to be able to select it from the intellisense drop down

#FilterTypes: All, RecordsetsOnly, RecordsetFields
#Providers: Calculate, File, DateTime, Default
Scenario Outline: Insert for All FilterType and Default Provider
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
	| testName | varlist                               | filterType | input                  | index | dropDownList                                                                        | option          | result                      | provider | caretposition |
	| 1        | <var/><var2/><rec><var/><var2/></rec> | All        | text var               | 8     | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[var]]         | text [[var]]                | Default  | 12            |
	| 2        | <var/><var2/><rec><var/><var2/></rec> | All        | text[[var              | 9     | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[var2]]        | text[[var2]]                | Default  | 12            |
	| 3        | <var/><var2/><rec><var/><var2/></rec> | All        | text[[rec().var]]      | 15    | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[rec().var2]]  | text[[rec().var2]]          | Default  | 16            |
	| 4        | <var/><var2/><rec><var/><var2/></rec> | All        | [[                     | 2     | [[var]],[[var2]],[[rec(,[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]] | [[var2]]        | [[var2]]                    | Default  | 8             |
	| 5        | <var/><var2/><rec><var/><var2/></rec> | All        | text[[rec().[[var      | 17    | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[rec(*).var2]] | text[[rec().[[rec(*).var2]] | Default  | 27            |
	| 6        | <var/><var2/><rec><var/><var2/></rec> | All        | text[[rec().[[var]]    | 17    | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[rec().var2]]  | text[[rec().[[rec().var2]]  | Default  | 26            |
	| 7        | <var/><var2/><rec><var/><var2/></rec> | All        | text[[rec().[[var]]]]  | 17    | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[rec().var]]   | text[[rec().[[rec().var]]]] | Default  | 25            |
	| 8        | <var/><var2/><rec><var/><var2/></rec> | All        | [[var]]v               | 8     | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[rec().var]]   | [[var]][[rec().var]]        | Default  | 20            |
	| 9        | <var/><var2/><rec><var/><var2/></rec> | All        | [[var]][[              | 9     | [[var]],[[var2]],[[rec(,[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]] | [[rec(*).var]]  | [[var]][[rec(*).var]]       | Default  | 21            |
	| 10       | <var/><var2/><rec><var/><var2/></rec> | All        | [[var]][[var           | 12    | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[rec(*).var2]] | [[var]][[rec(*).var2]]      | Default  | 22            |
	| 11       | <var/><var2/><rec><var/><var2/></rec> | All        | [[var]][[var]]         | 12    | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[rec(*).var1]] | [[var]][[rec(*).var1]]      | Default  | 22            |
	| 12       | <var/><var2/><rec><var/><var2/></rec> | All        | [[var]]                | 7     |                                                                                     |                 | [[var]]                     | Default  | 7             |
	| 13       | <var/><var2/><rec><var/><var2/></rec> | All        | [[var]] text           | 13    |                                                                                     |                 | [[var]] text                | Default  | 13            |
	| 14       | <var/><var2/><rec><var/><var2/></rec> | All        | text[[var2]]text       | 10    | [[var2]],[[rec().var2]],[[rec(*).var2]]                                             | [[rec().var2]]  | text[[rec().var2]]text      | Default  | 18            |
	| 15       | <var/><var2/><rec><var/><var2/></rec> | All        | r                      | 1     | [[var]],[[var2]],[[rec(,[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]] | [[rec().var2]]  | [[rec().var2]]              | Default  | 14            |
	| 16       | <var/><var2/><rec><var/><var2/></rec> | All        | re                     | 2     | [[rec(,[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]                  | [[rec(          | [[rec(                      | Default  | 6             |
	| 17       | <var/><var2/><rec><var/><var2/></rec> | All        | [[rec([[va]]).var]]    | 10    | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[var]]         | [[rec([[var]]).var]]        | Default  | 13            |
	| 18       | <var/><var2/><rec><var/><var2/></rec> | All        | [[[[a]]]]              | 5     | [[var]],[[var2]],[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]]        | [[var]]         | [[[[var]]]]                 | Default  | 9             |
	| 19       | <var/><var2/><rec><var/><var2/></rec> | All        | [[                     | 2     | [[var]],[[var2]],[[rec(,[[rec().var]],[[rec(*).var]],[[rec().var2]],[[rec(*).var2]] | [[var]]         | [[var]]                     | Default  | 7             |
#	| 20       | <a/><rec><a/><b/></rec>               | All        | [[rec().[[]]           | 10    | [[a]],[[rec(,[[rec().a]],[[rec(*).a]],[[rec().b]],[[rec(*).b]]                      | [[a]]           | [[rec().[[[[a]]]]           | Default  | 15            |
#	| 21       | <a/><rec><a/><b/></rec>               | All        | [[rec().a[[]]          | 11    | [[a]],[[rec]]                                                                       | [[a]]           | [[rec().a[[a]]]]            | Default  | 14            |
#	| 22       | <a/><b/><rec><a/><b/></rec>           | All        | [[rec()[[a]]           | 9     | [[a]],[[b]],[[rec(,[[rec().a]],[[rec(*).a]],[[rec().b]],[[rec(*).b]]                | [[b]]           | [[rec()[[a]]a]]             | Default  | 12            |
#	| 23       | <a/><b/><rec><a/><b/></rec>           | All        | [[rec().a]]+[[rec().]] | 20    | [[a]],[[b]],[[rec(,[[rec().a]],[[rec(*).a]],[[rec().b]],[[rec(*).b]]                | [[a]]           | [[rec().a]]+[[rec().[[a]]]] | Default  | 25            |
#	| 24       | <a/><b/><rec><a/><b/></rec>           | All        | sin(45.)]]             | 7     | [[a]],[[b]],[[rec(,[[rec().a]],[[rec(*).a]],[[rec().b]],[[rec(*).b]]                | [[a]]           | sin(45.[[a]])               | Default  | 12            |
#Bug 12133
#	| 25       | <rec><a/><b/></rec><xs><a/><b/></xs>  | All        | [[().a]]              | 3     | [[xc(,[[xc().a]],[[xc(*).a]],[[x(,[[x().a]],[[x(*).a]]                              | [[x(            | [[xc().a]]                  | Default  | 5             |

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
	
Scenario Outline: Insert for RecordsetsOnly FilterType and Default Provider
	Given I have the following variable list '<varlist>'
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
	| testName | varlist                                                         | filterType | input              | index | dropDownList                                                    | option                    | result                                    | provider | caretposition |
	| 1        | [[Person]],[[Person.name]]                                      | JsonObject | P                  | 1     | [[Person.name]],[[Person]]                                      | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 2        | [[Person]],[[Person.name]]                                      | JsonObject | [[                 | 2     | [[Person.name]],[[Person]]                                      | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 3        | [[Person]],[[Person.name]]                                      | JsonObject | Person.            | 7     | [[Person.name]]                                                 | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 4        | [[Person]],[[Person.name]]                                      | JsonObject | [[Person.          | 9     | [[Person.name]]                                                 | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 5        | [[Person]],[[Person.name]]                                      | JsonObject | [[Person.name      | 14    | [[Person.name]]                                                 | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 6        | [[Person]],[[Person.name]]                                      | JsonObject | name               | 4     | [[Person.name]]                                                 | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 7        | [[Person]],[[Person.name]]                                      | JsonObject | [                  | 1     | [[Person.name]],[[Person]]                                      | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 8        | [[Person]],[[Person.name]]                                      | JsonObject | rson               | 3     | [[Person.name]],[[Person]]                                      | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 9        | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | P                  | 1     | [[Person.Age]],[[Person.name]],[[Person]]                       | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 10       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | P                  | 1     | [[Person.Age]],[[Person.name]],[[Person]]                       | [[Person.Age]]            | [[Person.Age]]                            | Default  | 14            |
	| 11       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | [[                 | 2     | [[Person.Age]],[[Person.name]],[[Person]]                       | [[Person.Age]]            | [[Person.Age]]                            | Default  | 14            |
	| 12       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | [[                 | 2     | [[Person.Age]],[[Person.name]],[[Person]]                       | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 14       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | Person.            | 7     | [[Person.Age]],[[Person.name]]                                  | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 15       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | Person.            | 7     | [[Person.Age]],[[Person.name]]                                  | [[Person.Age]]            | [[Person.Age]]                            | Default  | 14            |
	| 16       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | [[Person.          | 9     | [[Person.Age]],[[Person.name]]                                  | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 17       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | [[Person.          | 9     | [[Person.name]],[[Person.Age]]                                  | [[Person.Age]]            | [[Person.Age]]                            | Default  | 14            |
	| 18       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | [[Person.name      | 14    | [[Person.name]]                                                 | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 19       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | [[Person.Age       | 9     | [[Person.Age]]                                                  | [[Person.Age]]            | [[Person.Age]]                            | Default  | 14            |
	| 20       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | name               | 4     | [[Person.name]]                                                 | [[Person.name]]           | [[Person.name]]                           | Default  | 15            |
	| 21       | [[Person]],[[Person.name]],[[Person.Age]]                       | JsonObject | Age                | 3     | [[Person.Age]]                                                  | [[Person.Age]]            | [[Person.Age]]                            | Default  | 14            |
	| 22       | [[Person]],[[Person.name]],[[Person.Age]],[[Person.Child.Name]] | JsonObject | Name               | 4     | [[Person.Child.Name]]                                           | [[Person.Child.Name]]     | [[Person.Child.Name]]                     | Default  | 21            |
	| 23       | [[Person]],[[Person.name]],[[Person.Age]],[[Person.Child.Name]] | JsonObject | Name               | 1     | [[Person.Child.Name]]                                           | [[Person.Child.Name]]     | [[Person.Child.Name]]                     | Default  | 21            |
	| 25       | [[Person]],[[Person.name]],[[Person.Age]],[[Person.Child.Name]] | JsonObject | Junk P             | 6     | [[Person.Age]],[[Person.name]],[[Person]],[[Person.Child.Name]] | [[Person.Child.Name]]     | Junk [[Person.Child.Name]]                | Default  | 26            |
	| 26       | [[Person]],[[Person.name]],[[Person.Age]],[[Person.Child.Name]] | JsonObject | [[Junk]] P         | 10    | [[Person.Age]],[[Person.name]],[[Person]],[[Person.Child.Name]] | [[Person.Child.Name]]     | [[Junk]] [[Person.Child.Name]]            | Default  | 30            |
	| 28       | [[Person]],[[Person.name]],[[Person.Age]],[[Person.Child.Name]] | JsonObject | ]] P               | 4     | [[Person.Age]],[[Person.name]],[[Person]],[[Person.Child.Name]] | [[Person.Child.Name]]     | ]] [[Person.Child.Name]]                  | Default  | 24            |
	| 29       | [[Person]],[[Person.name]],[[Person.Age]],[[Person.Child.Name]] | JsonObject | ( P                | 3     | [[Person.Age]],[[Person.name]],[[Person]],[[Person.Child.Name]] | [[Person.Child.Name]]     | ( [[Person.Child.Name]]                   | Default  | 23            |
	| 30       | [[Person]],[[Person.name]],[[Person.Age]],[[Person.Child.Name]] | JsonObject | ) P                | 3     | [[Person.Age]],[[Person.name]],[[Person]],[[Person.Child.Name]] | [[Person.Child.Name]]     | ) [[Person.Child.Name]]                   | Default  | 23            |
	| 35       | [[Person]],[[Person.name]],[[Person.Age]],[[Person.Child.Name]] | JsonObject | . P [[             | 3     | [[Person.Age]],[[Person.name]],[[Person.Child.Name]]            | [[Person.Child.Name]]     | . [[Person.Child.Name]] [[                | Default  | 24            |
	| 39       | [[Person]],[[Person.Childs(*).name]]                            | JsonObject | [[P                | 3     | [[Person]],[[Person.Childs(*).name]]                            | [[Person.Childs(*).name]] | [[Person.Childs(*).name]]                 | Default  | 25            |
	| 41       | [[Person]],[[Person.Childs(*).name]],[[Person.Childs().name]]   | JsonObject | [[Person.Childs(*  | 18    | [[Person.Childs(*).name]]                                       | [[Person.Childs(*).name]] | [[Person.Childs(*).name]]                 | Default  | 25            |
	| 42       | [[Person]],[[Person.Childs(*).name]],[[Person.Childs().name]]   | JsonObject | [[Person.Childs([[ | 18    | [[Person.Childs(*).name]],[[Person.Childs().name]]              | [[Person]]                | [[Person.Childs([[Person]]                | Default  | 26            |
	| 43       | [[Person]],[[Person.Childs(*).name]],[[Person.Childs().name]]   | JsonObject | [[Person.Childs([[ | 18    | [[Person.Childs(*).name]],[[Person.Childs().name]]              | [[Person.Childs(*).name]] | [[Person.Childs([[Person.Childs(*).name]] | Default  | 41            |

Scenario Outline: Insert for RecordsetFields FilterType and Default Provider
	Given I have the following intellisense options '<varlist>'
	And the filter type is '<filterType>'
	And the drop down list as '<dropDownList>'		
	When I select the following option '<option>'
	Then the result text should be '<result>'
	And the caret position will be '<caretposition>'
	Examples: 
	| testName | varlist                                      | filterType      | input                              | index | dropDownList                                                                              | option       | result                                 | provider | caretposition |
	| 1        | <b/><sum><b/></sum><mus><b/></mus>           | RecordsetsOnly  | u                                  | 1     | [[sum()]],[[mus()]]                                                                       | [[sum()]]    | [[sum()]]                              | Default  | 9             |
	| 2        | <b/><sum><b/></sum><mus><b/></mus>           | RecordsetsOnly  | b                                  | 1     |                                                                                           |              | b                                      | Default  | 1             |
	| 3        | <b/><sum><b/></sum><mus><b/></mus>           | RecordsetsOnly  | [[()]]                             | 3     |                                                                                           |              | [[()]]                                 | Default  | 3             |
	| 4        | <b/><sum><b/></sum><mus><b/></mus>           | RecordsetsOnly  | [[sum()]] s                        | 11    | [[sum()]],[[mus()]]                                                                       |              | [[sum()]] s                            | Default  | 11            |
	| 5        | <b/><sum><b/></sum><mus><b/></mus>           | RecordsetsOnly  | [[sum()]] [[                       | 12    | [[sum()]],[[mus()]]                                                                       | [[mus()]]    | [[sum()]] [[mus()]]                    | Default  | 19            |
	| 6        | <b/><sum><b/></sum><mus><b/></mus>           | RecordsetsOnly  | rec s                              | 5     | [[sum()]],[[mus()]]                                                                       | [[mus()]]    | rec [[mus()]]                          | Default  | 13            |
	| 7        | <b/><sum><b/><ba/></sum><mus><b/></mus>      | RecordsetFields | [[sum().b]]                        | 9     | [[sum().b]],[[sum(*).b]],[[sum().ba]],[[sum(*).ba]],[[mus().b]],[[mus(*).b]]              | [[sum().ba]] | [[sum().ba]]                           | Default  | 10            |
	| 8        | <b/><sum><b/><ba/></sum><mus><b/></mus>      | All             | [[sum(b).b]]                       | 7     | [[b]],[[sum().b]],[[sum(*).b]],[[sum().ba]],[[sum(*).ba]],[[mus().b]],[[mus(*).b]]        | [[b]]        | [[sum([[b]]).b]]                       | Default  | 11            |
	| 9        | <b/><ba/><sum><b/><ba/></sum><mus><b/></mus> | All             | [[sum([[b]]).b]]                   | 14    | [[b]],[[ba]],[[sum().b]],[[sum().ba]]                                                     | [[ba]]       | [[sum([[b]]).[[ba]]]]                  | Default  | 19            |
	| 10       | <b/><ba/><sum><b/><ba/></sum><mus><b/></mus> | All             | [[sum([[b]]).b]]                   | 14    | [[b]],[[ba]],[[sum().b]],[[sum().ba]]                                                     | [[sum().ba]] | [[sum([[b]]).ba]]                      | Default  | 15            |
	| 11       | <b/><sum><b/></sum><mus><b/></mus>           | RecordsetsOnly  | u u                                | 3     | [[sum()]],[[mus()]]                                                                       | [[sum()]]    | u [[sum()]]                            | Default  | 11            |
	| 12       | <b/><sum><b/></sum><mus><b/></mus>           | RecordsetsOnly  | [[sum()]] s                        | 11    | [[sum()]],[[mus()]]                                                                       | [[mus()]]    | [[sum()]] [[mus()]]                    | Default  | 19            |
	| 13       | <b/><sum><b/><ba/></sum><mus><b/></mus>      | RecordsetFields | [[sum().b]][[sum().b]]             | 20    | [[sum().b]],[[sum(*).b]],[[sum().ba]],[[sum(*).ba]],[[mus().b]],[[mus(*).b]]              | [[sum().ba]] | [[sum().b]][[sum().ba]]                | Default  | 21            |
	| 14       | <b/><sum><b/><ba/></sum><mus><b/></mus>      | All             | [[sum(b).b]][[sum(b).b]] some more | 19    | [[b]],[[sum().b]],[[sum(*).b]],[[sum().ba]],[[sum(*).ba]],[[mus().b]],[[mus(*).b]]        | [[b]]        | [[sum(b).b]][[sum([[b]]).b]] some more | Default  | 23            |
	| 15       | <b/><ba/><sum><b/><ba/></sum><mus><b/></mus> | All             | [[sum([[b]]).b]]                   | 14    | [[b]],[[ba]],[[sum().b]],[[sum().ba]]                                                     | [[ba]]       | [[sum([[b]]).[[ba]]]]                  | Default  | 19            |
	| 16       | <b/><ba/><sum><b/><ba/></sum><mus><b/></mus> | All             | [[sum().b]]                        | 9     | [[b]],[[ba]],[[sum().b]],[[sum(*).b]],[[sum().ba]],[[sum(*).ba]],[[mus().b]],[[mus(*).b]] | [[ba]]       | [[sum().[[ba]]]]                       | Default  | 14            |
	| 17       | <b/><ba/><sum><b/><ba/></sum><mus><b/></mus> | All             | [[sum().b]]b                       | 12    | [[b]],[[ba]],[[sum().b]],[[sum(*).b]],[[sum().ba]],[[sum(*).ba]],[[mus().b]],[[mus(*).b]] | [[ba]]       | [[sum().b]][[ba]]                      | Default  | 17            |
	| 18       | <b/><sum><b/></sum><mus><b/></mus>           | RecordsetsOnly  | =u                                 | 2     | [[sum()]],[[mus()]]                                                                       | [[sum()]]    | =[[sum()]]                             | Default  | 10            |
