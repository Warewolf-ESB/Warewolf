<script>	
	/* Database Service Setup Javascript */ 
	$(document).ready(function(){			
		try{
			/* region FIELDS */
			var serverName;
			var databaseName;
			var proceduresAndFunctions = '{"Procedures":[';
			var serviceName = '[[Dev2ServiceDetailsName]]';
			var sourceName = '[[Dev2ServiceDetailsSource]]';
			var webServer = '[[Dev2WebServer]]';	
			/* endregion FIELDS */
			/* region AJAX SERVICE CALLS */
			/* Get server name and database name given the worker service source */
			$.ajax({
				type:"GET",
				url:webServer+"/services/FindResourcesService",
				data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
				dataType:"xml",
				async:false,
				success:function(xml){
					$(xml).find("Dev2Resource").each(function(){
						var type = $(this).find("Dev2SourceType").text();
						if($(this).find("Dev2SourceName").text()==sourceName){
							var contents = $(this).find("Dev2SourceContents");								
							var tokens = contents.find("Source").attr("ConnectionString").split(";");
							serverName = tokens[0].split("=")[1];
							databaseName = tokens[1].split("=")[1];
							$("#Dev2DatabaseServiceSetupSource").text(serviceName+" - "+databaseName+" - "+contents.find("Source").attr("Type"));
							return false;
						}
					});
				}
			});
			/* endregion AJAX SERVICE CALLS */
			/* Get the information schema of the database set in databaseName as a json object */
			$.ajax({
				type:"GET",
				url:webServer+"/services/FindSqlDatabaseSchemaService",
				data:{"ServerName":serverName,"DatabaseName":databaseName},
				dataType:"xml",
				async:false,
				success:function(xml){
					var numProcedures=$(xml).find("Procedure").length;
					var numFunctions=$(xml).find("Function").length;
					var counter=0;
					$(xml).find("Procedure").each(function(){
						proceduresAndFunctions += "{";
						var numChildren = $(this).children().length;
						for(var i=0;i<numChildren;i++){
							if(i==0){
								proceduresAndFunctions += '"ProcedureName":'+'"'+$($(this).children()[0]).text()+'","Parameters":"';
							}else{
								proceduresAndFunctions += $(this).children()[i].nodeName;
								if(i<numChildren-1){
									proceduresAndFunctions += ",";
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
						var numChildren = $(this).children().length;
						for(var i=0;i<numChildren;i++){
							if(i==0){
								proceduresAndFunctions += '"FunctionName":'+'"'+$($(this).children()[0]).text()+'","Parameters":"';
							}else{
								proceduresAndFunctions += $(this).children()[i].nodeName;
								if(i<numChildren-1){
									proceduresAndFunctions += ",";
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
				}
			});
			/* populate activity listbox on upon page load completion */
			$.ajax({
				url:webServer+"/services/FindSqlDatabaseSchemaService",
				data:{"ServerName":serverName,"DatabaseName":databaseName},
				success:function(xml){
					$("#Dev2ServiceSetupSourceMethod").html('');		
					var procedures = $(xml).find("Procedures");
					var functions = $(xml).find("Functions");
					$("#Dev2ServiceSetupSourceMethod").append('<optgroup label="Stored Procedures">');
					$(procedures).find("Procedure").each(function(index, element){
						$("#Dev2ServiceSetupSourceMethod").append("<option>"+$(this).find("ProcedureName").text()+"</option>");								
					});
					$("#Dev2ServiceSetupSourceMethod").append('</optgroup>');
					$("#Dev2ServiceSetupSourceMethod").append('<optgroup label="Functions">');
					$(functions).find("Function").each(function(index, element){
						$("#Dev2ServiceSetupSourceMethod").append("<option>"+$(this).find("FunctionName").text()+"</option>");								
					});
					$("#Dev2ServiceSetupSourceMethod").append('</optgroup>');
				}
			});
			/* region SET PROPERTIES */
			/* endregion SET PROPERTIES */
			
			/* region HELPER FUNCTIONS */
			/* endregion HELPER FUNCTIONS */
			
			/* region EVENTS FIRED */
			/* filter stored procedures/functions based on text entered in "Search" */
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
			/* Display the input parameters of the selected stored procedure or function */
			$("#Dev2ServiceSetupSourceMethod").click(function(){
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
				var selected = $(this).find(':selected').text();			
				for(var i = 0; i < functions.length;i++){
					if(functions[i]==selected){					
						try{ 
							$('#Dev2DatabaseServiceSetupInputTestData tr').not(':first').not(':last').remove();
							if(functionParams[i].indexOf(",") != -1){
								parms = functionParams[i].split(",");
								var html = '';
								for(var i = 0; i < parms.length; i++){
									if(!(/^\s*$/).test(parms[i])){
										/* Because the inputs are created dynamically and "names" change, we're have no way of mapping them to the datalist. So, in order to be able to identify the inputs we prefix each input with "Dev2ServiceSetupInputs_"	*/
										html += '<tr><td>' + parms[i] + '</td><td>' + '<input type="text" id="Dev2ServiceSetupInputs_'+parms[i]+'" name="Dev2ServiceSetupInputs_'+parms[i]+'" />' + '</td></tr>';
									}else{
										html += '<tr><td colspan="2">No input parameters for this function</td></tr>';
									}
								}
								$('#Dev2DatabaseServiceSetupInputTestData tr').first().after(html);
							}else{
								parms = functionParams[i];
								var html = '';
								if(!(/^\s*$/).test(parms)){
										/* Because the inputs are created dynamically and "names" change, we're have no way of mapping them to the datalist. So, in order to be able to identify the inputs we prefix each input with "Dev2ServiceSetupInputs_"	*/
									html += '<tr><td>' + parms + '</td><td>' + '<input type="text" id="Dev2ServiceSetupInputs_'+parms+'" name="Dev2ServiceSetupInputs_'+parms+'" />' + '</td></tr>';
								}else{
										html += '<tr><td colspan="2">No input parameters for this function</td></tr>';
								}
								$('#Dev2DatabaseServiceSetupInputTestData tr').first().after(html);
							}
					
						}catch(e){}
						break;
					}
				}				
				for(var i = 0; i < procedures.length;i++){
					if(procedures[i]==selected){
						try{ 
							$('#Dev2DatabaseServiceSetupInputTestData tr').not(':first').not(':last').remove();
							if(procedureParams[i].indexOf(",") != -1){
								parms = procedureParams[i].split(",");
								var html = '';
								for(var i = 0; i < parms.length; i++){
									if(!(/^\s*$/).test(parms[i])){
										/* Because the inputs are created dynamically and "names" change, we're have no way of mapping them to the datalist. So, in order to be able to identify the inputs we prefix each input with "Dev2ServiceSetupInputs_"	*/
										html += '<tr><td>' + parms[i] + '</td><td>' + '<input type="text" id="Dev2ServiceSetupInputs_'+parms[i]+'" name="Dev2ServiceSetupInputs_'+parms[i]+'" />' + '</td></tr>';
									}else{
										html += '<tr><td colspan="2">No input parameters for this procedure</td></tr>';
									}
								}
								$('#Dev2DatabaseServiceSetupInputTestData tr').first().after(html);
							}else{
								parms = procedureParams[i];
								var html = '';
								if(!(/^\s*$/).test(parms)){
									/* Because the inputs are created dynamically and "names" change, we're have no way of mapping them to the datalist. So, in order to be able to identify the inputs we prefix each input with "Dev2ServiceSetupInputs_"	*/
									html += '<tr><td>' + parms + '</td><td>' + '<input type="text" id="Dev2ServiceSetupInputs_'+parms+'" name="Dev2ServiceSetupInputs_'+parms+'" />' + '</td></tr>';
								}else{
										html += '<tr><td colspan="2">No input parameters for this procedure</td></tr>';
								}
								$('#Dev2DatabaseServiceSetupInputTestData tr').first().after(html);
							}
					
						}catch(e){} 
						break;
					}
				}				
			});
			/* next button clicked */
			$("#Dev2DatabaseServiceSetupNextButton").click(function(){
				var procedureName = $("#Dev2ServiceSetupSourceMethod").val();
				var numParms = $(document).find('input[name^="Dev2ServiceSetupInputs_"]').length;
				var counter=0;
				var xmlString="";
				var parameters="";
				$(document).find('input[name^="Dev2ServiceSetupInputs_"]').each(function(){
					var attrName = $(this).attr("id");
					if(attrName != undefined){
						parameters += attrName.substring("Dev2ServiceSetupInputs_".length)+"="+$('#'+attrName+'').val();
						xmlString +="<"+attrName.substring("Dev2ServiceSetupInputs_".length)+" />";
						if(counter<numParms-1){
							parameters += ";";
						}
					}
					counter++;	
				});
				$("#Dev2ServiceSetupInputs").val(xmlString);
				$.ajax({
					type:"GET",
					url:webServer+"/services/CallProcedureService",
					data:{"ServerName":serverName,"DatabaseName":databaseName,"Procedure":procedureName,"Parameters":parameters},
					dataType:"xml",
					async:false,
					success:function(xml){
						xmlString="";
						var table = $(xml).find("Table");
						var numChildren = $(table).children().length;
						var recordSetName = "[[Dev2ServiceDetailsName]]".replace(/\s/g,"");
						xmlString+="<"+recordSetName+">";
						for(var i=0;i<numChildren;i++){
							xmlString += "<"+$(table).children()[i].nodeName+" />";
						}
						xmlString+="</"+recordSetName+">";
						$("#Dev2ServiceSetupOutputs").val(xmlString);
					}
				});
			});
			/* endregion EVENTS FIRED */
		}catch(e){
			alert(e);
		}	
	});
</script>