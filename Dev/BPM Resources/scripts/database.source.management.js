// initDatabaseSourceMgt('[[Dev2WebServer]]','[[Dev2SourceManagementDatabaseSource]]');
function initDatabaseSourceMgt(webServer, sourceName){
	/* DatabaseSourceManagement wizard javascript */ 
	$(document).ready(function(){			
		try{
			/* region FIELDS */
			var databaseType = "";
			var databaseAlias = "";
			var databaseServer = "";
			var databaseName = "";
			var username="";
			var password="";
			var errorMessage="";
			var databaseTypes=[];
			var computers=[];
			var sourceTemplate="";
			var sourceExists=false;
			var closeWindow=false;
			/* endregion FIELDS */
			/* region AJAX SERVICE CALLS */
			/* show animation when ajax event has fired  */
			/*$("#Dev2SourceManagementLoadingDialog").dialog({
				autoOpen: false,
				bgiframe:true,
				modal:true,
				width:96,
				height:96,
				resizable:false
			}).bind("ajaxStart",function(){
				$(this).dialog("open");
			}).bind("ajaxStop", function(){
				$(this).dialog("close");
			});*/
			/*
			<div id="Dev2SourceManagementLoadingDialog" style="text-align:center; vertical-align:center"><center><img src="/themes/system/css/images/3.gif" /><h5> Loading... </h5></center></div> 
			
			
			*/
			/* hide the dialog title bar */
			$(".ui-dialog-titlebar").hide();
			/* populate "source" drop down list */
			$.ajax({
				type:"GET",
				url:webServer+"/services/"+FindResourcesService",
				data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
				dataType:"xml",
				success:function(xml){
					$("#Dev2SourceManagementDatabaseSource").html('');
					$("#Dev2SourceManagementDatabaseSource").append("<option>New....</option>");						
					$(xml).find("Dev2Resource").each(function(){
						var type = $(this).find("Dev2SourceType").text();
						

						// Dev2SourceManagementDatabaseSource
						if(type.indexOf("Database") != -1){
							var val = $(this).find("Dev2SourceName").text();
							
							alert(val + " "+ sourceName);
							
							if(val == sourceName){
								$("#Dev2SourceManagementDatabaseSource").append("<option selected>"+val+"</option>");
							}else{
								$("#Dev2SourceManagementDatabaseSource").append("<option>"+val+"</option>");
							}
						
							
						}	
					});
				}
			});
			/* populate "type" drop down list */
			$.ajax({
				type:"GET",
				url:webServer+"/services/"+"FindResourcesService",
				data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
				dataType:"xml",
				success:function(xml){
					$("#Dev2SourceManagementDatabaseType").html('');
					$(xml).find("Dev2Resource").each(function(){
						var type = $(this).find("Dev2SourceType").text();
						if(type.indexOf("Database") != -1){
							databaseTypes.push($(this).find("Dev2SourceType").text());
						}
					});
					var types=[];
					$(databaseTypes).each(function(index,element){
						var typesLen = types.length;
						if(index==0){
							types.push(element);
						}else{
							for(var i=0;i<typesLen;i++){
								if(types[i]==element){
									break;
								}
								if(i==typesLen-1){
									types.push(element);
									break;
								}
							}
						}
					});
					$(types).each(function(index, element){
						$("#Dev2SourceManagementDatabaseType").append("<option>"+element+"</option>");
					});
				}
			});
			/* get computers on the network */
			$.ajax({
				type:"GET",
				url:webServer+"/services/"+"FindNetworkComputersService",
				dataType:"json",
				async:false,
				success:function(data){
					$(data).each(function(index,element){
						computers.push(element.ComputerName);
					});
				}
			});	
			/* endregion AJAX SERVICE CALLS */
			/* region SET PROPERTIES */
			/* disable done button, change color  */
			$("#Dev2SourceManagementDatabaseDoneButton").css('background-color','#CCC');
			$("#Dev2SourceManagementDatabaseDoneButton").attr('disabled',true);
			/* disable username and password textboxes when windows radio button is selected */
			if($("#windows").is(":checked")){
				$("#Dev2SourceManagementDatabaseUsername").prop("disabled",true);
				$("#Dev2SourceManagementDatabasePassword").prop("disabled",true);
			}
			/* autocomplete for the "Server:" input textbox */
			$("#Dev2SourceManagementDatabaseServer").autocomplete({
				source: computers,
				minLength: 0,
				delay: 0
			});
			/* endregion SET PROPERTIES */
			/* region HELPER FUNCTIONS */
			function setDoneButtonState(flag){
				if(flag){
					$("#Dev2SourceManagementDatabaseDoneButton").css('background-color','');
					$("#Dev2SourceManagementDatabaseDoneButton").attr('disabled',false);
				}else{
					$("#Dev2SourceManagementDatabaseDoneButton").css('background-color','#CCC');
					$("#Dev2SourceManagementDatabaseDoneButton").attr('disabled',true);
				}
			}
			function alertDialog(message, callback){
				$("#Dev2SourceManagementAlert").html(message);
				/* password prompt dialog */
				$("#Dev2SourceManagementAlertDialog").dialog({
					autoOpen: false,
					bgiframe:true,
					modal:true,
					width:280,
					height:'auto',
					resizable:false,
					buttons:
					[
						{text:"OK",
							click : function(){
								$(this).dialog("close");
								callback();
							}
						}
					]
				});
				/* hide the dialog title bar */
				$(".ui-dialog-titlebar").hide();
				/* open dialog */
				$("#Dev2SourceManagementAlertDialog").dialog('open');
			}
			/* endregion HELPER FUNCTIONS */
			/* region EVENTS FIRED */
			/* enable/disable authentication text boxes */
			$('input[type=radio]').click(function(){			
				if($(this).hasClass('enable_auth_box')){
					$("#Dev2SourceManagementDatabaseUsername").prop("disabled",false);
					$("#Dev2SourceManagementDatabasePassword").prop("disabled",false);
					$("#Dev2SourceManagementDatabaseDataSource").html('');
					alert("change");
				}else if($(this).hasClass('disable_auth_box')){
					$("#Dev2SourceManagementDatabaseUsername").prop("disabled",true);
					$("#Dev2SourceManagementDatabasePassword").prop("disabled",true);
					$("#Dev2SourceManagementDatabaseUsername").val("");
					$("#Dev2SourceManagementDatabasePassword").val("");
					$("#Dev2SourceManagementDatabaseDataSource").html('');
					username="";
					password="";
					$.ajax({
						type:"GET",
						url:"FindSqlDatabasesService",
						data: {"ServerName":databaseServer,"Username":username,"Password":password},
						dataType:"xml",
						success:function(xml){
							errorMessage = $(xml).find("Error").text();
							if(errorMessage.length == 0){
								setDoneButtonState(true);
								$(xml).find("Table").each(function(){
									$("#Dev2SourceManagementDatabaseDataSource").append("<option>"+$(this).find("DATABASE_NAME").text()+"</option>");
								});
							}else{
								setDoneButtonState(false);
								alertDialog(errorMessage);
								return false;
							}
						}
					});
				}
			});	
			/* get sqlserver databases */
			$("#Dev2DatabaseSourceManagementTestConnection").click(function(){
				databaseServer = $("#Dev2SourceManagementDatabaseServer").val();
				username = $("#Dev2SourceManagementDatabaseUsername").val();
				password = $("#Dev2SourceManagementDatabasePassword").val();
				if($("#specified").is(":checked")){
					if(username.length == 0){
						$("#Dev2SourceManagementDatabaseUsername").focus();	
						setDoneButtonState(false);
						alertDialog("Please enter username");
						return false;
					}					
				}	
				if(databaseServer.length > 0){
					$.ajax({
						type:"GET",
						url:"FindSqlDatabasesService",
						data: {"ServerName":databaseServer,"Username":username,"Password":password},
						dataType:"xml",
						async:false,
						success:function(xml){
							$("#Dev2SourceManagementDatabaseDataSource").html('');
							errorMessage = $(xml).find("Error").text();
							if(errorMessage.length == 0){
								setDoneButtonState(true);
								alertDialog("Connection successful!");							
								$(xml).find("Table").each(function(){
									$("#Dev2SourceManagementDatabaseDataSource").append("<option>"+$(this).find("DATABASE_NAME").text()+"</option>");
								});
							}else{
								setDoneButtonState(false);
								alertDialog(errorMessage);
							}
						}
					});
				}else{
					setDoneButtonState(false);
					alertDialog("Please enter server name");
				}
			});
			/* populate web form based on selected source */
			$("#Dev2SourceManagementDatabaseSource").change(function(){
				$.ajax({
					type:"GET",
					url:webServer+"/services/"+"FindResourcesService",
					data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
					dataType:"xml",
					success:function(xml){
						var sourceName = $("#Dev2SourceManagementDatabaseSource option:selected").val();
						$("#windows").attr("checked",true);
						databaseTypes=[];
						$(xml).find("Dev2Resource").each(function(){
							var type = $(this).find("Dev2SourceType").text();
							if($(this).find("Dev2SourceName").text()==sourceName){
								if(type.indexOf("Database") != -1){
									$("#Dev2SourceManagementDatabaseType").html('');
									var contents = $(this).find("Dev2SourceContents");								
									$("#Dev2SourceManagementDatabaseType").append("<option>"+contents.find("Source").attr("Type")+"</option>");
									$("#Dev2SourceManagementDatabaseAlias").val(contents.find("Source").attr("Name"));
									var tokens = contents.find("Source").attr("ConnectionString").split(";");
									$("#Dev2SourceManagementDatabaseServer").val(tokens[0].split("=")[1]);
									$("#Dev2SourceManagementDatabaseDataSource").html('');
									$("#Dev2SourceManagementDatabaseDataSource").append("<option>"+tokens[1].split("=")[1]+"</option>");
									return false;
								}
							}else{
								$("#Dev2SourceManagementDatabaseAlias").val("");
								$("#Dev2SourceManagementDatabaseServer").val("");
								$("#Dev2SourceManagementDatabaseUsername").val("");
								$("#Dev2SourceManagementDatabasePassword").val("");
								$("#Dev2SourceManagementDatabaseDataSource").html('');
								if(type.indexOf("Database") != -1){
									databaseTypes.push($(this).find("Dev2SourceType").text());
								}
							}
						});
						if(databaseTypes.length > 0){
							$("#Dev2SourceManagementDatabaseType").html('');
							var types=[];
							$(databaseTypes).each(function(index,element){
								var typesLen = types.length;
								if(index==0){
									types.push(element);
								}else{
									for(var i=0;i<typesLen;i++){
										if(types[i]==element){
											break;
										}
										if(i==typesLen-1){
											types.push(element);
											break;
										}
									}
								}
							});
							$(types).each(function(index, element){
								$("#Dev2SourceManagementDatabaseType").append("<option>"+element+"</option>");
							});
						}
					}
				});
			});	
			/* done button clicked, add resource to application server */
			$("#Dev2SourceManagementDatabaseDoneButton").click(function(){
				databaseType = $("#Dev2SourceManagementDatabaseType option:selected").val();
				databaseAlias = $("#Dev2SourceManagementDatabaseAlias").val();
				databaseServer = $("#Dev2SourceManagementDatabaseServer").val();
				databaseName = $("#Dev2SourceManagementDatabaseDataSource option:selected").val();
				if(sourceExists){
					$("#Dev2SourceManagementDatabaseAlias").focus();
					setDoneButtonState(false);
					alertDialog("Source already exists. Please enter a different Alias.");
					return;
				}
				if(databaseAlias.length == 0){
					setDoneButtonState(false);
					alertDialog("Please specify name for source file in the Alias field");
					return;	
				}
				if(databaseServer.length == 0){
					setDoneButtonState(false);
					alertDialog("Please specify name of server the database resides on in the Server field");
					return;	
				}
				if(databaseName == undefined){
					setDoneButtonState(false);
					alertDialog("Please ensure Data Source is populated");
					return;	
				}
				if($("#specified").is(":checked")){
					if(username.length == 0){
						setDoneButtonState(false);
						alertDialog("Please enter username");
						return false;
					}else{
						if(databaseServer.length > 0){
							$.ajax({
								type:"GET",
								url:webServer+"/services/"+"FindSqlDatabasesService",
								data: {"ServerName":databaseServer,"Username":username,"Password":password},
								dataType:"xml",
								success:function(xml){
									errorMessage = $(xml).find("Error").text();
									if(errorMessage.length > 0){
										setDoneButtonState(false);
										alertDialog(errorMessage);
										return false;
									}
								}
							});
						}else{
							setDoneButtonState(false);
							alertDialog("Please enter server name");
							return false;
						}						
					}					
					sourceTemplate =  '<Source Name="'+databaseAlias+'" Type="'+databaseType+'" ConnectionString="server='+databaseServer+';database='+databaseName+';UID='+username+';Password='+password+';">'+
									  '<AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>'+
									  '<Comment>SQL Database</Comment>'+
									  '<Category>Utility</Category>'+
									  '<HelpLink>http://s</HelpLink>'+
									  '<Tags></Tags>'+
									  '<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>'+
									  '<BizRule />'+
									  '<WorkflowActivityDef />'+
									  '<XamlDefinition />'+
									  '<DataList />'+
									  '<DisplayName>Source</DisplayName>'+
									  '</Source>';
				}else{	
					sourceTemplate =  '<Source Name="'+databaseAlias+'" Type="'+databaseType+'" ConnectionString="server='+databaseServer+';database='+databaseName+';integrated security=true">'+
									  '<AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>'+
									  '<Comment>SQL Database</Comment>'+
									  '<Category>Utility</Category>'+
									  '<HelpLink>http://s</HelpLink>'+
									  '<Tags></Tags>'+
									  '<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>'+
									  '<BizRule />'+
									  '<WorkflowActivityDef />'+
									  '<XamlDefinition />'+
									  '<DataList />'+
									  '<DisplayName>Source</DisplayName>'+
									  '</Source>';
				}
				$.ajax({
					type:"GET",
					url:webServer+"/services/"+"AddResourceService",
					data:{"ResourceXml":sourceTemplate,"Roles":"Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access"},
					dataType:"xml",
					success:function(xml){
						/* refresh "source" drop down list */
						$.ajax({
							type:"GET",
							url:"FindResourcesService",
							data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
							dataType:"xml",
							success:function(xml){
								$("#Dev2SourceManagementDatabaseSource").html('');
								$("#Dev2SourceManagementDatabaseSource").append("<option>New....</option>");						
								$(xml).find("Dev2Resource").each(function(){
									var type = $(this).find("Dev2SourceType").text();
									if(type.indexOf("Database") != -1){
										$("#Dev2SourceManagementDatabaseSource").append("<option>"+$(this).find("Dev2SourceName").text()+"</option>");
									}	
								});
							}
						});
						/* refresh "type" drop down list */
						$.ajax({
							type:"GET",
							url:"FindResourcesService",
							data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
							dataType:"xml",
							success:function(xml){
								$("#Dev2SourceManagementDatabaseType").html('');
								$(xml).find("Dev2Resource").each(function(){
									var type = $(this).find("Dev2SourceType").text();
									if(type.indexOf("Database") != -1){
										databaseTypes.push($(this).find("Dev2SourceType").text());
									}
								});
								var types=[];
								$(databaseTypes).each(function(index,element){
									var typesLen = types.length;
									if(index==0){
										types.push(element);
									}else{
										for(var i=0;i<typesLen;i++){
											if(types[i]==element){
												break;
											}
											if(i==typesLen-1){
												types.push(element);
												break;
											}
										}
									}
								});
								$(types).each(function(index, element){
									$("#Dev2SourceManagementDatabaseType").append("<option>"+element+"</option>");
								});
							}
						});
						alertDialog($(xml).text(),function(){
							window.close();
						});
					}
				});
			});	
			/* close this browser window */
			$("#Dev2SourceManagementCloseButton").click(function(){
				window.close();
			});
			/* when input textbox, Server, has focus fire the autocomplete feature */
			$("#Dev2SourceManagementDatabaseServer").focus(function(){
				setDoneButtonState(false);
				$("#Dev2SourceManagementDatabaseDataSource").html('');
				$(this).data("autocomplete").search($(this).val());
			});
			/* populate data source drop down list when it has focus */
			$("#Dev2SourceManagementDatabaseDataSource").focus(function(){
				if($("#Dev2SourceManagementDatabaseDataSource option").length == 0){
					databaseServer = $("#Dev2SourceManagementDatabaseServer").val();
					username = $("#Dev2SourceManagementDatabaseUsername").val();
					password = $("#Dev2SourceManagementDatabasePassword").val();
					if($("#specified").is(":checked")){
						if(username.length == 0){
							$("#Dev2SourceManagementDatabaseUsername").focus();
							setDoneButtonState(false);		
							alertDialog("Please enter username");
							return false;
						}					
					}	
					if(databaseServer.length > 0){
						$.ajax({
							type:"GET",
							url:"FindSqlDatabasesService",
							data: {"ServerName":databaseServer,"Username":username,"Password":password},
							dataType:"xml",
							success:function(xml){
								$("#Dev2SourceManagementDatabaseDataSource").html('');
								errorMessage = $(xml).find("Error").text();
								if(errorMessage.length == 0){
									setDoneButtonState(true);
									$(xml).find("Table").each(function(){
										$("#Dev2SourceManagementDatabaseDataSource").append("<option>"+$(this).find("DATABASE_NAME").text()+"</option>");
									});
								}else{
									setDoneButtonState(false);
									$("#Dev2SourceManagementDatabaseUsername").focus();	
									alertDialog(errorMessage);
									return false;
								}
							}
						});
					}else{
						alertDialog("Please enter server name");
					}
				}
			});			
			/* when alias texbox has lost focus, check value entered against available sources */
			$("#Dev2SourceManagementDatabaseAlias").blur(function(e){
				sourceExists=false;
				$("#Dev2SourceManagementDatabaseSource option").each(function(){
					if($(this).val()==$("#Dev2SourceManagementDatabaseAlias").val()){
						$("#Dev2SourceManagementDatabaseAlias").val("");
						alertDialog("Source already exists. Please enter a different Alias.",function(){			
							$("#Dev2SourceManagementDatabaseAlias").focus();
						});
						sourceExists=true;
						return false;	
					}
				});
			});
		}catch(e){
			alertDialog(e);
		}	
	});
}
