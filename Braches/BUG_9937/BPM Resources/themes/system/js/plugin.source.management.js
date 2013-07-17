// <script>bindDev2SourceManagementSourceForEdit('[[Dev2SourceManagementSource]]','[[Dev2ServiceName]]','[[Dev2Category]]','[[Dev2Help]]','[[Dev2Icon]]','[[Dev2Description]]','[[Dev2Tags]]','[[Dev2StudioExe]]'); </script>		

//<script>bindDev2SourceManagementSourceForEdit(); </script>
function bindDev2SourceManagementSourceForEdit(source, serviceDetailsName ,category, help, icon, desc, tags, Dev2StudioExe, webServer, isNew){

	$(document).ready(function(){	
	
		/* Bind the namespace too */
		var nameSpace = "";
		
		if(source != ''){
			//$("#Dev2SourceManagementNamespace").attr("readonly", "readonly");
			//$("#Dev2SourceManagementNamespace").val(nameSpace);
			var sourceData = fetchSourcePayload(webServer, source);
			var start = sourceData.indexOf("AssemblyName="); // +14
			if(start  > 0){
				start += 14;
				var end = sourceData.indexOf('"',start);
				nameSpace = sourceData.substr(start, (end-start));
				start = sourceData.indexOf("AssemblyLocation="); // +17
				if(start > 0){
					start += 18;
					end = sourceData.indexOf('"', (start+1));
					var srcLoc = sourceData.substr(start, (end-start));
					bindNameSpace(srcLoc, webServer, nameSpace);
				}
				//$("#Dev2SourceManagementNamespace").html('<option selected>' + nameSpace +"</option>");
			}
		}
		
		/* populate "source" drop down list */
		buildResourceTree(source);
		bindv2(serviceDetailsName ,category,help, icon, desc, tags, source, Dev2StudioExe, webServer, isNew);
	
	});
}

function buildResourceTree(source){
	$.ajax({
			type:"GET",
			url:"FindResourcesService",
			data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
			dataType:"xml",
			async:false,
			success:function(xml){
			
				var errorHandler = new Dev2ErrorHandler(xml);
				
				if(!errorHandler.Has200ErrorPayload()){
			
					$("#Dev2SourceManagementSource").html('');
					$("#Dev2SourceManagementSource").append("<option>New....</option>");						
					$(xml).find("Dev2Resource").each(function(){
						var type = $(this).find("Dev2SourceType").text();
						if(type.indexOf("Plugin") != -1){
							var val = $(this).find("Dev2SourceName").text();
							
							// Dev2SourceManagementCategory
							
							if(val == source){
								$("#Dev2SourceManagementSource").append("<option selected>"+val+"</option>");
								$("#Dev2SourceManagementAlias").val(val);
								$("#Dev2SourceManagementAlias").attr('readonly','readonly'); // dis-allow edit if editing service
								var assemblyLoc = $(this).find("Dev2SourceContents").find("Source").attr("AssemblyLocation");
								$("#Dev2SourceManagementFullPath").val(assemblyLoc);
								
								// now set the category
								var cat = $(this).find("Category").text();
								
								$("#Dev2SourceManagementCategory").val(cat);
								
								if(assemblyLoc.indexOf("GAC") != -1){
									$("#Dev2SourceManagementResourceType").text("GAC");
									$("#Dev2SourceManagementServer").val(assemblyLoc.substring(assemblyLoc.indexOf("GAC:")+("GAC:").length));
									$("#Dev2SourceManagementFullPath").val("");
								}else{
									$("#Dev2SourceManagementResourceType").text("Path");
									$("#Dev2SourceManagementFullPath").val(assemblyLoc);
									$("#Dev2SourceManagementServer").val(assemblyLoc.substring(assemblyLoc.lastIndexOf("/")+1));
								}
								
							}else{
								$("#Dev2SourceManagementSource").append("<option>"+val+"</option>");
							}
						}	
					});
				}else{
					var errorHandler = new Dev2ErrorHandler(data);
					errorHandler.HandleNon200ErrorPayload();
				}
			},
			error:function(data){
				var errorHandler = new Dev2ErrorHandler(data);
				errorHandler.HandleNon200ErrorPayload();
			}
		});


}

function bindPluginSrcCancelBtn(pluginSrc, serviceDetailsName ,category, help, icon, desc, tags, origSrc, Dev2StudioExe, webServer, isNew){

	if(Dev2StudioExe != 'yes'){
	// webServer, sourceName, serviceName, category, help, icon, desc, tags, Dev2StudioExe, isNew
		bindDev2PluginStep2BackBtn(serviceDetailsName,"Plugin", pluginSrc,category, help, icon, desc, tags, "equal", isNew); 
	}else{
		Dev2Awesomium.Cancel();
	}	
}

function bindv2(serviceDetailsName ,category, help, icon, desc, tags, origSrc, Dev2StudioExe, webServer, isNew){
/* PluginSourceManagement wizard javascript   */
	$(document).ready(function(){
		try{
			/* region FIELDS */
			var path="";
			var domain="";
			var username="";
			var password="";
			var plugins=[];
			var serverPath="";
			var serverUsername ="";
			var isValid=false;
			var errorMessage="";
			/* endregion FIELDS */

			/* region AJAX SERVICE CALLS */
			/* get the netbios name of the computer the application server is running on */
			
			$.ajax({
				url:"FindMachineNameService",
				dataType:"xml",
				async:false,
				success: function(xml){
					var errorHandler = new Dev2ErrorHandler(xml);
				
					if(!errorHandler.Has200ErrorPayload()){
						$("#Dev2SourceManagementMachineName").text("Path from " + $(xml).find("Dev2MachineName").text()+":");
					}else{
						errorHandler.Handle200ErrorPayload();
					}
				},
				error:function(data){
					var errorHandler = new Dev2ErrorHandler(data);
					errorHandler.HandleNon200ErrorPayload();
				}
			});
			
			
			/* when category textbox has focus fire autocomplete feature */
			$( "#Dev2SourceManagementCategory" ).autocomplete({
				source: buildCategories(webServer) ,
				minLength: 0,
				delay: 0
			});
			
			/* get username of user logged in to the server  */
			$.ajax({
				type:"GET",
				url:"FindServerUsernameService",
				dataType:"xml",
				async:false,
				success:function(xml){
					var errorHandler = new Dev2ErrorHandler(xml);
				
					if(!errorHandler.Has200ErrorPayload()){
						serverUsername = $(xml).find("Result").text();
					}else{
						errorHandler.Handle200ErrorPayload();
					}
				},
				error:function(data){
					var errorHandler = new Dev2ErrorHandler(data);
					errorHandler.HandleNon200ErrorPayload();
				}
			});			
			/* endregion AJAX SERVICE CALLS */
			
			// bind alias check
			$("#Dev2SourceManagementAlias").focusout(function(){
				// check name against those in the catalog
				var currentVal = stripNaughtyChars($(this).val());
				
				$("#Dev2SourceManagementAlias").val(currentVal);
				
				if($("#Dev2SourceManagementSource option:selected").val()=="New...."){
					var serviceData = fetchMapping(webServer, currentVal, "*");
					// Name="travwebpage"
					if(serviceData.toLowerCase().indexOf('name="'+currentVal.toLowerCase()+'"') > 0){
						// we have an issue, 
						alert("Please select a different name, <i>" + currentVal + "</i> is already in use.", "Service Naming Error");
						//$("#Dev2ServiceName").focus();
						$("#Dev2SourceManagementDoneButton").css("display", "none");
					}
				}else{
					//nameOk = true;
					if(isWorkerServiceType(editServiceType)){
						$("#Dev2SourceManagementDoneButton").css("display", "inline");
					}
				}
			});
			
			/* region SET PROPERTIES */
			/* hide dialog contents before it's shown */
			$("#Dev2SourceManagementServerDialog").hide();
			$("#Dev2SourceManagementPasswordPrompt").hide();
			/* if dialog is open, bind tabs to the dialog */
			$("#Dev2SourceManagementServerDialog").bind('dialogopen',function(){
					$("#Dev2SourceManagementServerDialog").tabs();
			});
			/* if dialog is closing, destroy tabs */
			$("#Dev2SourceManagementServerDialog").bind('dialogclose',function(){
				$("#Dev2SourceManagementServerDialog").tabs('destroy');
			});
			/* build application server path recursively */
			/* endregion SET PROPERTIES */
			
			function getURI(node){
				var pNode = node.parent;
				var result = "";
				
				while(node.data.title != null){					
					var toPrefix = node.data.title.replace("\\","");
				
					result =  toPrefix + "/"+ result;
					node = node.parent;
				}
				
				result = result.substr(0, (result.length-1));
				
				return result;
			}
			/* get the credentials entered in the username and password textboxes */
			function setCredentials(){
				username = $("#Dev2SourceManagementUsername").val();
				password = $("#Dev2SourceManagementPassword").val();
				if(username.indexOf("\\") == -1){
					if(username.length > 0){
						domain=".";
					}
				}else{
					domain=username.split("\\")[0];
					username=username.split("\\")[1];
				}			
				if((/^\s*$/).test(username)){
					isValid=false;
				}
			}
			/* test username and password entered test */
			function checkCredentials(){				
				$.ajax({
					url:"CheckCredentialsService",
					data: {"Domain":domain,"Username":username,"Password":password},
					dataType:"xml",
					async:false,
					success: function(xml){
						errorMessage = $(xml).find("Result").text();
						if(errorMessage.indexOf("failed") != -1){
							isValid = false;
						}else{
							isValid = true;
						}
					},
					error:function(data){
						var errorHandler = new Dev2ErrorHandler(data);
						errorHandler.HandleNon200ErrorPayload();
					}
				});
			}
			/* open server dialog */
			function serverDialog(webServer){
				/* If the value Dev2SourceManagementServer textbox is a path, set the value of Dev2SourceManagementPath to it */
				if($("#Dev2SourceManagementResourceType").text().indexOf("GAC:") == -1){
					$("#Dev2SourceManagementPath").val($("#Dev2SourceManagementFullPath").val().replace("\\",""));
				}else{
					$("#Dev2SourceManagementPath").val("");
				}
				//if($("#Dev2SourceManagementRegisteredName option").length == 0){
				
				var path = $("#Dev2SourceManagementPath").val();
				var isGAC = false;
				
				var gacPath = "";
				if(path.indexOf("GAC:") >= 0){
					isGAC = true;
					gacPath = path.substr(4, (path.length-4));
				}

				/* populate GAC listbox */
				$.ajax({
					type:"GET",
					url:"RegisteredAssemblyService",
					dataType:"json",
					async:false,
					success:function(data){
					
						var errorHandler = new Dev2ErrorHandler(xml);
				
						if(!errorHandler.Has200ErrorPayload()){
							$("#Dev2SourceManagementRegisteredName").html('');
							plugins=[];
							var myAdd= [];
							var ok = true;
							var i = 0;
							$.each(data, function(index, element){
								while(i < myAdd.length && ok){
									if(myAdd[i] === element.AssemblyName){
										ok = false;
									}
									i++;
								}
								if(ok){
									plugins.push(element.AssemblyName);
									myAdd.push(element.AssemblyName);
								}
								i = 0;
								ok = true;
							});
							var myAdd= new Array();

							$(plugins).each(function(index, element){
								if(gacPath == element){
									$("#Dev2SourceManagementRegisteredName").append("<option selected>"+element+"</option>");
								}else{
									$("#Dev2SourceManagementRegisteredName").append("<option>"+element+"</option>");
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
				//}
				/* set the value of GAC filter textbox to what is selected in the GAC listbox */ 
				$("#Dev2SourceManagementFilter").val($("#Dev2SourceManagementRegisteredName option:selected").text());
				/* build application server file system tree */
				$("#Dev2SourceManagementServerTree").dynatree({
					autoCollapse:false,
					checkbox: false,
					selectMode: 1,
					keyPathSeparator: "/",
					initAjax:{
						url:"FindDriveService",
						data: {"Domain":domain,"Username":username,"Password":password}
					},
					onPostInit: function(isReloading, isError) {
						this.reactivate();
						if($("#Dev2SourceManagementFullPath").val().length > 0){
							var tree = $("#Dev2SourceManagementServerTree").dynatree("getTree");
							tree.loadKeyPath($("#Dev2SourceManagementFullPath").val().replace(/ /g, "_").replace("(", "40").replace(")", "41"),function(node,status){
								if(status == "loaded"){
									node.expand(true);
								}else if(status == "ok"){
									node.activate();
								}else{
									alertDialog(status);
								}
							});
						}
					},
					onLazyRead: function(node){
						path="";
						path = getURI(node);
						node.appendAjax({
							url:"FindDirectoryService",
							data: {"DirectoryPath":path,"Domain":domain,"Username":username,"Password":password}
						});
						node.setLazyNodeStatus(DTNodeStatus_Ok);
					},
					onActivate: function(node){
						path="";
						path = getURI(node);
						$.ajax({
							url:"CheckPermissionsService",
							data: {"Path":path},
							success:function(xml){
								errorMessage = $(xml).find("Error").text();
								if(errorMessage.length>0){
									alertDialog("e ->" + errorMessage);
									$("#Dev2SourceManagementPath").val("");
								}else{
									$("#Dev2SourceManagementPath").val(path.replace("\\",""));
								}
							}
						});
						serverPath=path;
					}
				});
				/* build application server/GAC dialog */
				$("#Dev2SourceManagementServerDialog").dialog({
					autoOpen: false,
					bgiframe:true,
					modal:true,
					width:480,
					height:'auto',
					resizable:false,
					buttons:
					[
						{text:"Done",
							click : function(){
								if(serverPath.indexOf("GAC") != -1){
									$("#Dev2SourceManagementResourceType").text("GAC");
									$("#Dev2SourceManagementServer").val(serverPath.substring(serverPath.indexOf("GAC:")+("GAC:").length));
									$("#Dev2SourceManagementFullPath").val(serverPath);
								}else{
									$("#Dev2SourceManagementResourceType").text("Path");
									$("#Dev2SourceManagementFullPath").val(serverPath);
									$("#Dev2SourceManagementServer").val(serverPath.substring(serverPath.lastIndexOf("/")+1));
								}
								
								// Bind the namespaces here
								var queryPath = serverPath.replace("\\","");

								bindNameSpace(queryPath, webServer);
								
								$(this).dialog("close"); 
							}
						},
						{text:"Cancel",
							click : function(){ 
								$(this).dialog("close"); 
							}
						}
					]
				});
				/* hide the dialog title bar */
				$(".ui-dialog-titlebar").hide();
				/* open dialog */
				$("#Dev2SourceManagementServerDialog").dialog('open');
			}
			
			
			
			function alertDialog(message,callback){
				$("#Dev2SourceManagementAlert").html(message);
				/* password prompt dialog */
				$("#Dev2SourceManagementAlertDialog").dialog({
					autoOpen: false,
					bgiframe:true,
					modal:true,
					width:'auto',
					height:'auto',
					resizable:false,
					buttons:
					[
						{text:"OK",
							click : function(){
								$(this).dialog("close"); 
								try{
									callback();
								}catch(e){}
							}
						}
					]
				});
				/* hide the dialog title bar */
				$(".ui-dialog-titlebar").hide();
				/* open dialog */
				$("#Dev2SourceManagementAlertDialog").dialog('open');
			}
			function passwordDialog(callback){				
				/* password prompt dialog */
				$("#Dev2SourceManagementPasswordPrompt").dialog({
					autoOpen: false,
					bgiframe:true,
					modal:true,
					width:280,
					height:'auto',
					resizable:false,
					buttons:
					[
						{text:"Done",
							click : function(){
								password=$("#Dev2SourceManagementPasswordPromptValue").val();
								checkCredentials();
								$(this).dialog("close");
								try{
									callback();
								}catch(e){}
							}
						},
						{text:"Cancel",
							click : function(){ 
								$(this).dialog("close"); 
							}
						}
					]
				});
				/* hide the dialog title bar */
				$(".ui-dialog-titlebar").hide();
				/* open dialog */
				$("#Dev2SourceManagementPasswordPrompt").dialog('open');
			}
			/* endregion HELPER FUNCTIONS */
			
			/* region EVENTS FIRED */
			/* Test connection button clicked */
			$("#Dev2SourceManagementTestConnection").click(function(){
				setCredentials();
				if($("#Dev2SourceManagementFullPath").val().length > 0 && $("#Dev2SourceManagementServer").val().length > 0){
					if(username.length > 0){
						
						checkCredentials();
						if(!isValid){
							alertDialog(errorMessage);
							return false;
						}else{
							alertDialog(errorMessage);					
						}
					}else{
						alertDialog("Please enter a username");
						return false;					
					}
				}else{
					alertDialog("Please select a path to check.");
				}
			});

			/* Set the value of Dev2SourceManagementFilter textbox to the Dev2SourceManagementRegisteredName listbox selected item value */
			$("#Dev2SourceManagementRegisteredName").click(function(){
				$("#Dev2SourceManagementFilter").val($("#Dev2SourceManagementRegisteredName option:selected").text());
				serverPath="GAC:"+$("#Dev2SourceManagementRegisteredName option:selected").text();
			});
			/* Browser server button is clicked */
			$("#Dev2SourceManagementBrowseServer").click(function(){
				$("#Dev2SourceManagementServerDialog").dialog('destroy');
				$("#Dev2SourceManagementServerTree").dynatree('destroy');
				$("#Dev2SourceManagementPasswordPromptValue").val('');
				setCredentials();
				if(username.length > 0){
					checkCredentials();
					if(!isValid){
						alertDialog(errorMessage);
						return false;
					}else{
						serverDialog(webServer);
					}
				}else /*if(isValid)*/{
					serverDialog(webServer);
				}
			});
			
			/* done button clicked, update/add resource to application server */
			$("#Dev2SourceManagementDoneButton").click(function(){
				if(checkRequired()){
			
					var registeredName = $("#Dev2SourceManagementRegisteredName option:selected").val();
					var path = $("#Dev2SourceManagementServer").val();
					if(path.length==0){
						alertDialog("Please select the resource from server");
						return false;
					}
					var alias = $("#Dev2SourceManagementAlias").val();
					if(alias.length==0){
						alertDialog("Please enter Alias");
						return false;
					}
					
					// make happy paths ;)
					var loc = $("#Dev2SourceManagementFullPath").val().replace(/\\/g,"/");
					loc = loc.replace("//","/");
					
					// fetch the source information again, to get the 
					var nameSpace = $("#Dev2SourceManagementNamespace").val();

					var sourceTemplate =  '<Source Name="'+stripNaughtyChars(alias)+'" Type="Plugin" AssemblyName="' + nameSpace + '" AssemblyLocation="'+loc+'">'+
										  '<AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>'+
										  '<Comment>Plugin</Comment>'+
										  '<Category>' + $("#Dev2SourceManagementCategory").val() + '</Category>'+
										  '<HelpLink>http://dummyhelp.link</HelpLink>'+
										  '<Tags></Tags>'+
										  '<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>'+
										  '<BizRule />'+
										  '<WorkflowActivityDef />'+
										  '<XamlDefinition />'+
										  '<DataList />'+
										  '<TypeOf>Plugin</TypeOf>' +
										  '<DisplayName>Source</DisplayName>'+
										  '</Source>';
										  
					$.ajax({
						type:"GET",
						url:"AddResourceService",
						data:{"ResourceXml":sourceTemplate,"Roles":"Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access"},
						dataType:"xml",
						async:false,
						success:function(xml){
							
							var errorHandler = new Dev2ErrorHandler(xml);
				
							if(!errorHandler.Has200ErrorPayload()){
							
								var srcStatus = "changed";
								
								if(origSrc == $("#Dev2SourceManagementAlias").val()){
									srcStatus = "equal";
								}
								
								var resName = stripNaughtyChars($("#Dev2SourceManagementAlias").val());
								
								if(Dev2StudioExe != 'yes'){
									SendSourceUpdate(resName);
									bindDev2PluginStep2BackBtn(serviceDetailsName,"Plugin", resName,category, help, icon, desc, tags, srcStatus, isNew); 
								}else{
									//if(srcStatus == "changed"){
										var result = "<Dev2WizardPayload>";
										//ResourceName
										result += "<ResourceName>" + resName + "</ResourceName>";
										//ResourceType { WorkflowService, Service { Plugin, Database }, Source, Website, HumanInterfaceProcess }
										result += "<ResourceType>Source</ResourceType>";
										//Category
										result += "<Category>" + $("#Dev2SourceManagementCategory").val() + "</Category>";
										//Comment
										result += "<Comment>Binary Source</Comment>";
										//Tags
										result += "<Tags>" + resName +"</Tags>";
										//IconPath
										result += "<IconPath></IconPath>";
										//HelpLink
										result += "<HelpLink></HelpLink>";
										// reflect the fact that this is a close operation
										result += "<IsTreeUpdate>false</IsTreeUpdate>";
										
										result += "</Dev2WizardPayload>";
										
										// pass data back to the studio to close
										ExitWithStudioPush(result);
									//}else{
									//	Dev2Awesomium.Cancel();
									//}
								}
							}else{
								errorHandler.Handle200ErrorPayload();
							}
						},
						error:function(data){
							var errorHandler = new Dev2ErrorHandler(data);
							errorHandler.HandleNon200ErrorPayload();
							
						}						
						
					});
				}
			});	
			
			/* if source selected items changes hide, show items */
			$("#Dev2SourceManagementSource").change(function(){
				if($("#Dev2SourceManagementSource option:selected").val()=="New...."){
					$("#Dev2SourceManagementAlias").removeAttr('readonly'); // dis-allow edit if editing service
					$("#Dev2SourceManagementResourceType").text("");
					$("#Dev2SourceManagementFullPath").val(""); 
					$("#Dev2SourceManagementNamespace").html("");
					
				}else{
					$("#Dev2SourceManagementAlias").attr('readonly','readonly'); // dis-allow edit if editing service
				}

				$("#Dev2SourceManagementRegisteredName").show();
				$("#Dev2SourceManagementRegisteredNameLabel").show();
				$("#Dev2SourceManagementServer").show();
				$("#Dev2SourceManagementResourceType").show();					
				$("#Dev2SourceManagementMachineName").show();
				$("#Dev2SourceManagementBrowseServer").show();
			});
			/* populate web form based on selected source */
			$("#Dev2SourceManagementSource").change(function(){
				$.ajax({
					type:"GET",
					url:"FindResourcesService",
					data:{"ResouceName":"*","ResourceType":"Source","Roles":"Business Design Studio Developers"},
					dataType:"xml",
					success:function(xml){
					
						var errorHandler = new Dev2ErrorHandler(xml);
				
						if(!errorHandler.Has200ErrorPayload()){
					
							password="";
							var sourceName = $("#Dev2SourceManagementSource option:selected").val();						
							$("#Dev2SourceManagementRegisteredName").html('');
							$(xml).find("Dev2Resource").each(function(){
								var type = $(this).find("Dev2SourceType").text();
								if($(this).find("Dev2SourceName").text()==sourceName){
									var contents = $(this).find("Dev2SourceContents");								
									$("#Dev2SourceManagementRegisteredName").append("<option>"+contents.find("Source").attr("AssemblyName")+"</option>");
									$("#Dev2SourceManagementAlias").val(contents.find("Source").attr("Name"));
									var val = contents.find("Source").attr("AssemblyLocation");
									var end = val.lastIndexOf("/");
									var toDisplay = val;
									/* Travis */
									if(end != -1){
										toDisplay = val.substring((end+1));
									}else{
										end = val.lastIndexOf("GAC:");
										if(end != -1){
											toDisplay = val.substring((end+1));
										}
									}
									
									// Now refresh the namespaces
									var asmName = contents.find("Source").attr("AssemblyName");
									var asmLoc = contents.find("Source").attr("AssemblyLocation")
									bindNameSpace(asmLoc, webServer, asmName);
									
									$("#Dev2SourceManagementServer").val(toDisplay);
									if(val.indexOf("GAC:") == -1){
										$("#Dev2SourceManagementResourceType").text("Path");
										$("#Dev2SourceManagementPath").val(toDisplay);
										$("#Dev2SourceManagementFilter").val("");
									}else{
										$("#Dev2SourceManagementResourceType").text("GAC");
										$("#Dev2SourceManagementPath").val("");
										$("#Dev2SourceManagementFilter").val();
									}
									$("#Dev2SourceManagementFullPath").val(val);
									
									
									return false;
								}else{
									$("#Dev2SourceManagementAlias").val("");
									$("#Dev2SourceManagementServer").val("");
									$("#Dev2SourceManagementUsername").val("");
									$("#Dev2SourceManagementPassword").val("");					
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
			});	
			/* GAC filter function */
			$("#Dev2SourceManagementFilter").keyup(function(){ 		
				$("#Dev2SourceManagementRegisteredName").html(''); 
				var value = $("#Dev2SourceManagementFilter").val().toUpperCase(); 
				var opt="";
				for(var i=0; i < plugins.length; i++){
					var element = plugins[i].toUpperCase();
					if(element.indexOf(value) >= 0){
						opt += "<option>"+plugins[i]+"</option>";
					}
				}
				$("#Dev2SourceManagementRegisteredName").html(opt); 				
			});
			/* endregion EVENTS FIRED */
		}catch(e){
			alertDialog(e);
		}
	});
}

function bindNameSpace(queryPath, webServer, ns){

	$.ajax({
		type:"GET",
		url:webServer+"/services/PluginRegistryService",
		data:{"AssemblyLocation": queryPath, "NameSpace" : "", "ProtectionLevel" : "public"},
		dataType:"xml",
		success:function(xml){
		
			var errorHandler = new Dev2ErrorHandler(xml);
				
			if(!errorHandler.Has200ErrorPayload()){
		
				// fetch name spaces
				var payload = "";
				$(xml).find("Dev2PluginSourceNameSpace").each(function(){
					var val = $(this).text();

					if(val.indexOf("`") < 0){
						if(val == ns){
							payload += "<option selected>" + val +"</option>";
						}else{
							payload += "<option>" + val +"</option>";
						}
					}
				
				});
				
				$(xml).find("Dev2PluginStatusMessage").each(function(){
					payload = "<option value='' selected>An error occured while inspecting namespaces</option>";
					payload += "<option value=''>Error : " + $(this).text()+"</option>";
				});
				
				// set the payload
				$("#Dev2SourceManagementNamespace").html(payload);
			}else{
				errorHandler.Handle200ErrorPayload();
			}
		},
		error:function(data){
			var errorHandler = new Dev2ErrorHandler(data);
			errorHandler.HandleNon200ErrorPayload();
		}
	});
}