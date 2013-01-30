/*
	This file is ugly and a mess.
	Its current state was already a massive refactor to remove a single function approach to doing JQuery.
	
	Aka -- It used to be far worse
*/

function initDatabaseServiceSetup(webServer, serviceDetailsName, serviceDetailsSource, serviceSourceMethod, dbType ){

	/* Database Service Setup Javascript */ 
	$(document).ready(function(){	
		var theUser = "";
		var thePass = "";

		if(dbType == ''){
			dbType = 'SqlDatabase';
			
			// extract def and get the user and pass
			var serviceData = fetchMapping(webServer, serviceDetailsSource, "*");
			
			// User Id= or UID=
			var start = serviceData.indexOf("UID=");
			if(start < 0){
				start = serviceData.indexOf("User Id=");
				if(start > 0){
					start += 8;
				}
			}else{
				start +=4;
			}
			
			// all well-formed
			if(start > 0){
				var end = serviceData.indexOf(";", start);
				if(end > 0){
					theUser = serviceData.substr(start, (end - start));
				}
				
				start = serviceData.indexOf("Password="); // +9
				if(start > 0){
					start += 9;
					end = serviceData.indexOf(";", start);
					thePass = serviceData.substr(start, (end-start));
				}
			}
		}

	
		try{
			/* region FIELDS */
			var serverName;
			var databaseName;
			var proceduresAndFunctions = '{"Procedures":[';
			var serviceName = serviceDetailsName;
			var sourceName = serviceDetailsSource;
			var fnBodyRepo = [];
			

			$.ajax({
				type:"GET",
				url:webServer+"/services/FindResourcesService",
				data:{"ResourceName":serviceDetailsSource,"ResourceType":"Source","Roles":"*"},
				dataType:"xml",
				async:false,
				success:function(xml){
				
					var errorHandler = new Dev2ErrorHandler(xml);
				
					if(!errorHandler.Has200ErrorPayload()){

						$(xml).find("Dev2Resource").each(function(){
							var type = $(this).find("Dev2SourceType").text();
							if($(this).find("Dev2SourceName").text()==sourceName){
								var contents = $(this).find("Dev2SourceContents");	

								var preProcess = contents.text();
								preProcess.replace('/&lt;/g','>').replace('/&gt;/g','<');
								
								contents = $.parseXML(preProcess);
								
								try{
									var extractedString = $(contents).find("Source").attr("ConnectionString");

									var tokens = extractedString.split(";");
									serverName = tokens[0].split("=")[1];
									databaseName = tokens[1].split("=")[1];
								}catch(e){
									// Handle Error
									alert(e, "Error");
								}

								$("#Dev2DatabaseServiceSetupSource").text(serviceName+" - "+databaseName+" - "+$(contents).find("Source").attr("Type"));
							}
						});
					}else{
						errorHandler.Handle200ErrorPayload();
					}
				},
				error:function(data){
					var errorHandler = new Dev2ErrorHandler(data);
					errorHandler.HandleNon200ErrorPayload();
				}
			});
			
			$.ajax({
				url:webServer+"/services/FindSqlDatabaseSchemaService",
				data:{"ServerName":serverName,"DatabaseName":databaseName, "Username":theUser,"Password":thePass},
				async:false,
				success:function(xml){
				
					// Check for Error
					var errorHandler = new Dev2ErrorHandler(xml);
				
					if(!errorHandler.Has200ErrorPayload()){
						
						/* Build up silly JSON */
						var numProcedures=$(xml).find("Procedure").length;
						var numFunctions=$(xml).find("Function").length;
						var counter=0;
						$(xml).find("Procedure").each(function(){
							proceduresAndFunctions += "{";
							var numChildren = ($(this).children().length); // Body is always last ;)
							var fnBody = $(this).find("Body").text();
							fnBody = fnBody.replace("<![CDATA[","");
							fnBody = fnBody.replace("]]>");
							var fnName = ($(this).find("ProcedureName").text());
							
							fnBodyRepo[fnName] = fnBody;
							
							//fnBody[$($(this).children()[0]).text()] = body;
							for(var i=0;i<numChildren;i++){
								if(i==0){
									proceduresAndFunctions += '"ProcedureName":'+'"'+$($(this).children()[0]).text()+'","Parameters":"';
								}else{
									var v = $(this).children()[i].nodeName;
								
									if(v != "Body"){
										proceduresAndFunctions += v;
										if(i<numChildren-2){
											proceduresAndFunctions += ",";
										}
									}
								}	
							}
							proceduresAndFunctions += '"}';
							if(counter<numProcedures-1){
								proceduresAndFunctions += ",";
							}	
							counter++;
						});
						counter=0;
						proceduresAndFunctions += '],"Functions":[';
						$(xml).find("Function").each(function(){
							proceduresAndFunctions += "{";
							var numChildren = ($(this).children().length); // body is always last
							var fnBody = $(this).find("Body").text();
							fnBody = fnBody.replace("<![CDATA[","");
							fnBody = fnBody.replace("]]>");
							var fnName = ($(this).find("FunctionName").text());
							
							fnBodyRepo[fnName] = fnBody;
							
							for(var i=0;i<numChildren;i++){
								if(i==0){
									proceduresAndFunctions += '"FunctionName":'+'"'+$($(this).children()[0]).text()+'","Parameters":"';
								}else{
									var v = $(this).children()[i].nodeName;
								
									if(v != "Body"){
										proceduresAndFunctions += v;
										if(i<numChildren-2){
											proceduresAndFunctions += ",";
										}
									}
								}	
							}
							proceduresAndFunctions += '"}';
							if(counter<numFunctions-1){
								proceduresAndFunctions += ",";
							}	
							counter++;
						});
						proceduresAndFunctions += ']}';
					
						/* End Build */
						
						$("#Dev2ServiceSetupSourceMethod").html('');		
						var procedures = $(xml).find("Procedures");
						var functions = $(xml).find("Functions");
						$("#Dev2ServiceSetupSourceMethod").append('<optgroup label="Stored Procedures">');
						$(procedures).find("Procedure").each(function(index, element){
							var val = $(this).find("ProcedureName").text();
							// serviceSourceMethod
							if(val == serviceSourceMethod){
								$("#Dev2ServiceSetupSourceMethod").append("<option selected>"+val+"</option>");
							}else{
								$("#Dev2ServiceSetupSourceMethod").append("<option>"+val+"</option>");
							}							
						});
						$("#Dev2ServiceSetupSourceMethod").append('</optgroup>');
						$("#Dev2ServiceSetupSourceMethod").append('<optgroup label="Functions">');
						$(functions).find("Function").each(function(index, element){
							var val = $(this).find("FunctionName").text();
							if(val == serviceSourceMethod){
								$("#Dev2ServiceSetupSourceMethod").append("<option selected>"+val+"</option>");
							}else{
								$("#Dev2ServiceSetupSourceMethod").append("<option>"+val+"</option>");
							}
															
						});
						$("#Dev2ServiceSetupSourceMethod").append('</optgroup>');
					}else{
						errorHandler.Handle200ErrorPayload();
					}
				},
				error:function(data){
					var errorHandler = new Dev2ErrorHandler(data);
					errorHandler.HandleNon200ErrorPayload();
				}
			});
	
			$("#Dev2DatabaseServiceSetupFilter").keyup(function(){ 		
				$('#Dev2DatabaseServiceSetupInputTestData tr').not(':first').not(':last').remove();
				var services = $.parseJSON(proceduresAndFunctions); 
				var procedures = new Array();
				for(var i = 0; i < services.Procedures.length;i++){
					var proc = services.Procedures[i];
					procedures[i] = proc.ProcedureName;
				}				
				var functions = new Array();
				for(var i = 0; i < services.Functions.length;i++){
					var _function = services.Functions[i];
					functions[i] = _function.FunctionName;
				}				
				$("#Dev2ServiceSetupSourceMethod").html(''); 
				$("#Dev2ServiceSetupSourceMethod").append('<optgroup label="Stored Procedures">');
				for(var i=0;i<procedures.length;i++){ 
					var value = $("#Dev2DatabaseServiceSetupFilter").val().toUpperCase(); 
					var tmp = procedures[i]; 
					var tmpSearch = procedures[i].toUpperCase(); 
					if(tmpSearch.indexOf(value) >= 0){ 
						$("#Dev2ServiceSetupSourceMethod").append("<option>"+tmp+"</option>"); 
					} 
				} 
				$("#Dev2ServiceSetupSourceMethod").append('</optgroup>');
				$("#Dev2ServiceSetupSourceMethod").append('<optgroup label="Functions">');
				for(var i=0;i<functions.length;i++){ 
					var value = $("#Dev2DatabaseServiceSetupFilter").val().toUpperCase(); 
					var tmp = functions[i]; 
					var tmpSearch = functions[i].toUpperCase(); 
					if(tmpSearch.indexOf(value) >= 0){ 
						$("#Dev2ServiceSetupSourceMethod").append("<option>"+tmp+"</option>"); 
					} 
				} 
				$("#Dev2ServiceSetupSourceMethod").append('</optgroup>');
			});
			
			/* Display the proc contents */
			$("#Dev2DBSetupViewStoredProcDetail").click(function(){
				// fnBodyRepo
				var selectedVal = $("#Dev2ServiceSetupSourceMethod option:selected").val();
				var fnBody = fnBodyRepo[selectedVal];
				try{
				$(document.createElement('div'))
					.attr({title: selectedVal + " Implementation", 'class': 'alert'})
					.html(fnBody)
					.dialog({
						buttons: {OK: function(){$(this).dialog('close');}},
						close: function(){$(this).remove();},
						draggable: true,
						modal: false,
						resizable: true,
						width: 600,
						height: 350,
						position : [30,50]
					});
				}catch(e){
					// default to the normal JS alert
					alert(message);
				}
			});
			
			// bind next and test buttons
			$("#Dev2DatabaseServiceSetupNextButton").click(function(){
				if(!testDataOk){
					renderDBQueryWindow(webServer, false);
				}else{
					// already set data, continue
					if(checkRequired()){
						document.forms[0].submit();
					}
				}		
			});
			
			/* test button clicked */
			$("#Dev2DatabaseServiceSetupTestBtn").click(function(){
				renderDBQueryWindow(webServer, true);				
			});	
			
			// The magical fun start here ;)
			// Used to query the db for results, allows the user to cancel and change the payload from waiting to results
			function renderDBQueryWindow(webServer, isTestMode){

				var procedureName = $("#Dev2ServiceSetupSourceMethod").val();
				//var numParms = $(document).find('Dev2InputTestData').length;

				var counter=0;
				var xmlString="";
				var parameters="";
				var rawInputs = "";
				
				$(".Dev2InputTestData").each(function(){
					var attrName = $(this).attr("fnName");
					var val = $(this).val();
					
					if(attrName != undefined){
						parameters += attrName+"="+val+";";
						rawInputs += attrName+",";
					}
					
					counter++;	
				});
				
				// remove trailing ;
				if(parameters.lastIndexOf(";") == (parameters.length-1)){
					parameters = parameters.substr(0, (parameters.length-1));
				}
				
				// function to handle payload fetch
				var handlerFn = function(theData){
					// Dev2InputTestData

					var errorHandler = new Dev2ErrorHandler(theData);
				
					if(!errorHandler.Has200ErrorPayload()){
					
						// Display the result to the user ;)
						var sampleData = new Dev2InputMapper(theData);

						var fnName = $("#Dev2ServiceSetupSourceMethod option:selected").val();

						if(sampleData.HasPaths()){
							// set extract inputs
							sampleData.SetRawPluginInputs(rawInputs);
							// now display the execution result to the user
							sampleData.DisplayPathsWithDataAndPersist(fnName,"Dev2SamplePersist","Dev2InputPersist",dialogWin, isTestMode, testDataOkFn );
						}else{
							// no paths returned
							var errorAppend = "Please enter input or select a new function to execute";
							if(rawInputs == ""){
								errorAppend = "An error occured while executing your selcted activity, please try again";
							}
							
							var errorBtns = {
								Ok: function () {
										$(this).dialog("close");
									}
							};
							
							dialogWin.redraw(errorAppend, errorBtns);
						}	
					}else{
						// over ride the default error handler due to the nature of how this interface is setup
						dialogWin.redraw($(theData).find("Error").text(), errorBtns);
					}
				};
			
				// start query
				var payload = fetchDBMappingPaths(webServer, serverName, databaseName, procedureName, parameters, "interrogate", handlerFn, theUser, thePass);
				//var payload = fetchTestData(webServer, asmLoc, asmName, handlerFn);
			
				var queryBtns = {
					Cancel : function(){
						payload.abort();
						$(this).dialog("close");
					}
				};
				
				// display dialog
				var dialogWin = new Dev2Dialog('<div id="loadWait" style="color:#500; width:100%; margin-left:5px; font-size:12px; font-style:italic"><blink>Querying Database...</blink></div>', 
												$("#Dev2ServiceSetupSourceMethod option:selected").val() + " Execution Results", 
												true,
												queryBtns
											   );	
					
			}
			
			/* Display the input parameters of the selected stored procedure or function */
			$("#Dev2ServiceSetupSourceMethod").click(function(){
				
				$("#Dev2DatabaseServiceSetupNextButton").removeClass("hiddenBtn");
				$("#Dev2DatabaseServiceSetupTestBtn").removeClass("hiddenBtn");
				$("#Dev2DBSetupViewStoredProcDetail").removeClass("hiddenBtn");
				
				// Finally, display the input table
				$("#Dev2DBInputRegion").css("display", "inline");
			
				var services = $.parseJSON(proceduresAndFunctions); 
				var procedures = new Array();
				var procedureParams = new Array();
				for(var i = 0; i < services.Procedures.length;i++){
					var proc = services.Procedures[i];
					procedures[i] = proc.ProcedureName;
					procedureParams[i] = proc.Parameters;
				}				
				var functions = new Array();
				var functionParams = new Array();
				for(var i = 0; i < services.Functions.length;i++){
					var _function = services.Functions[i];
					functions[i] = _function.FunctionName;
					functionParams[i] = _function.Parameters;
				}

				// ioMappingInput1
				
				var selected = $(this).find(':selected').text();			
				for(var i = 0; i < functions.length;i++){
					if(functions[i]==selected){					
						try{ 
							//$('#Dev2DatabaseServiceSetupInputTestData tr').not(':first').not(':last').remove();
							if(functionParams[i].indexOf(",") != -1){
								parms = functionParams[i].split(",");
								var html = '';
								
								// Redid SQL Invoke Method to return proper anything to XML
								for(var i = 0; i < parms.length; i++){
									if(!(/^\s*$/).test(parms[i])){
										html += '<div class="ioMappingInputRow"><div class="ioMappingOutputCol1">' + parms[i] + '</div><div class="ioMappingOutputCol2">' + '<input type="text" class="Dev2InputTestData" id="Dev2InputTestDataFn'+i+'" fnName="'+parms[i]+'" fnType="String" /></div><button type="button" value="..." onClick=displayBigInput("Dev2InputTestDataSP' + i+'","' + parms[i] + '") class="ioMappingRegionBtn">...</button></div>';
										//html += '<tr><td>' + parms[i] + '</td><td>' + '<input type="text" id="Dev2ServiceSetupInputs_'+parms[i]+'" name="Dev2ServiceSetupInputs_'+parms[i]+'" />' + '</td></tr>';
									}else{
										html += '<div class="ioMappingInputRow"><div class="ioMappingOutputCol1">No input parameters for this function</div></div>';
									}
								}
								$('#Dev2DatabaseServiceSetupInputTestData').html(html);
							}else{
								parms = functionParams[i];
								var html = '';
								if(!(/^\s*$/).test(parms)){
							
									html += '<div class="ioMappingInputRow"><div class="ioMappingOutputCol1">' + parms + '</div><div class="ioMappingOutputCol2">' + '<input type="text" class="Dev2InputTestData" id="Dev2InputTestDataFn'+i+'" fnName="'+parms+'" fnType="String" /></div><button type="button" value="..." onClick=displayBigInput("Dev2InputTestDataSP0",'+parms[i] +'") class="ioMappingRegionBtn">...</button></div>';
									//html += '<tr><td>' + parms + '</td><td>' + '<input type="text" id="Dev2ServiceSetupInputs_'+parms+'" name="Dev2ServiceSetupInputs_'+parms+'" />' + '</td></tr>';
								}else{
										html += '<div class="ioMappingInputRow"><div class="ioMappingOutputCol1">No input parameters for this function</div></div>';
								}
								$('#Dev2DatabaseServiceSetupInputTestData').html(html);
							}
					
						}catch(e){}
						break;
					}
				}				
				for(var i = 0; i < procedures.length;i++){
					if(procedures[i]==selected){
						try{ 
							//$('#Dev2DatabaseServiceSetupInputTestData tr').not(':first').not(':last').remove();
							if(procedureParams[i].indexOf(",") != -1){
								parms = procedureParams[i].split(",");
								var html = '';

								for(var i = 0; i < parms.length; i++){
									if(!(/^\s*$/).test(parms[i])){
										html += '<div class="ioMappingInputRow"><div class="ioMappingOutputCol1">' + parms[i] + '</div><div class="ioMappingOutputCol2">' + '<input type="text" class="Dev2InputTestData" id="Dev2InputTestDataSP'+i+'" fnName="'+parms[i]+'" fnType="String" /></div><button type="button" value="..." onClick=displayBigInput("Dev2InputTestDataSP' + i+'","' + parms[i] + '") class="ioMappingRegionBtn">...</button></div>';
										/* Because the inputs are created dynamically and "names" change, we're have no way of mapping them to the datalist. So, in order to be able to identify the inputs we prefix each input with "Dev2ServiceSetupInputs_"	*/
										//html += '<tr><td>' + parms[i] + '</td><td>' + '<input type="text" id="Dev2ServiceSetupInputs_'+parms[i]+'" name="Dev2ServiceSetupInputs_'+parms[i]+'" />' + '</td></tr>';
									}else{
										html = '<div class="ioMappingInputRow"><div class="ioMappingOutputCol1">No input parameters for this procedure</div></div>';
									}
								}
								$('#Dev2DatabaseServiceSetupInputTestData').html(html);
							}else{
								parms = procedureParams[i];
								var html = '';
								if(!(/^\s*$/).test(parms)){
									/* Because the inputs are created dynamically and "names" change, we're have no way of mapping them to the datalist. So, in order to be able to identify the inputs we prefix each input with "Dev2ServiceSetupInputs_"	*/
									//html += '<tr><td>' + parms + '</td><td>' + '<input type="text" id="Dev2ServiceSetupInputs_'+parms+'" name="Dev2ServiceSetupInputs_'+parms+'" />' + '</td></tr>';
									html += '<div class="ioMappingInputRow"><div class="ioMappingOutputCol1">' + parms + '</div><div class="ioMappingOutputCol2">' + '<input type="text" class="Dev2InputTestData" id="Dev2InputTestDataSP0" fnName="'+parms+'" fnType="String" /></div><button type="button" value="..." onClick=displayBigInput("Dev2InputTestDataSP0",'+parms[i] +'") class="ioMappingRegionBtn">...</button></div>';
								}else{
									html = '<div class="ioMappingInputRow"><div class="ioMappingOutputCol1">No input parameters for this procedure</div></div>';
								}
								$('#Dev2DatabaseServiceSetupInputTestData').html(html);
							}
					
						}catch(e){} 
						break;
					}
				}	
			});
			
			// trigger the input region
			if(serviceSourceMethod != ""){
				$("#Dev2ServiceSetupSourceMethod option:selected").trigger("click");
			}
			
			/* endregion EVENTS FIRED */
		}catch(e){
			
			alert(e+"");
		}	
	});
}

/*
	This is here to allow formated input into Stored Procs and Functions
*/
var stringCache = [];
function displayBigInput(elmID, parmName){
	var currentVal = $("#"+elmID).val();
	
	var cacheVal = stringCache[elmID];
	if(cacheVal !== undefined){
		if(cacheVal.replace(/(\r\n|\n|\r)/gm,"") == currentVal){
			// same, replace with nicely formated string ;)
			currentVal = cacheVal;
		}
	}
	
	//payload, title, resize, btns
	var payload =  "<textarea id='Dev2Step2BigInput' style='width:745px; height:370px;'>" + currentVal +"</textarea>";
	
	var btns = {
					Cancel : function(){
						$(this).dialog("close");
					},
					Ok : function(){
						var data = $("textarea#Dev2Step2BigInput").val();
						// \n \r -> <br/>
						stringCache[elmID] = data; // place into the cache
						$("#"+elmID).val(data);
						// now disable user input
						if(data.length > 1){
							$("#"+elmID).attr("disabled","disabled");
							$("#"+elmID).css("backgroundColor","#ccc");
							$("#"+elmID).attr("title","Disabled due to large region input - This is to preserve formatting");
						}else{
							$("#"+elmID).removeAttr("disabled");
							$("#"+elmID).css("backgroundColor","#fff");
							$("#"+elmID).attr("title","Enabled due to large region input removal");
						}
						$(this).dialog("close");
					}
				};
	
	var newDialog = new Dev2Dialog(payload, parmName+" Input Region", true, btns);
}

function showOnClickDatabaseNext(){
	// editing existing service show the next button ;)
	$("#Dev2DatabaseServiceSetupNextButton").removeClass("hiddenBtn");
}