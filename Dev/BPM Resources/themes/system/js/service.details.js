// DevSourceChange

var nameOk = false;

function initServiceDetails(webServer, editService, editServiceType, sourceName, category, help, icon, desc, tags, tooltip, srcChange, noName){
	
	$(document).ready(function(){

		// Fix for webServer not making it down ;)
		webServer = "http://127.0.0.1:1234";

		$("#Dev2ServiceDetailsServerDialog").hide();
	
		if(editService != ''){
			//09.04.2013: Ashley Lewis - Bug 9189 build up form data with jquery selectors 
			$("#Dev2ServiceName").val(editService);
			$("#Dev2ServiceDetailsCategory").val(category);
			$("#Dev2ServiceDetailsHelp").val(help);
			$("#Dev2ServiceDetailsIcon").val(icon);
			$("#Dev2ServiceDetailsDescription").val(desc);
			$("#Dev2ServiceDetailsTags").val(tags);
			
			$("#Dev2ServiceName").attr("readonly", "readonly");
			nameOk = true;
			
			//09.04.2013: Ashley Lewis - Bug 9198 fetchMapping returns a fatal error :(
			// var serviceData = fetchMapping(webServer, editService, "*");
			
			// // Name="travwebpage"
			// // && srcChanged != "changed"
			// if(serviceData.toLowerCase().indexOf('name="'+editService.toLowerCase()+'"') > 0){
				// $("#Dev2ServiceDetailsDoneButton").css("display", "inline");
				
				// // flip step 3 button 
				// if(isWorkerServiceType(editServiceType) && srcChange != "changed"){
					// $("#Dev2ServiceDetailsStep3").removeClass("hiddenBtn");
					// // we also need to adjust the next button to step 2
					// // Dev2ServiceDetailsNext
					// $("#Dev2ServiceDetailsNext").attr("value","Step 2");
					// $("#Dev2ServiceDetailsNext").html("Go To Step 2");
				// }
				
				// // lock the done button
				// if(srcChange == "changed"){
					// $("#Dev2ServiceDetailsDoneButton").css("display", "none");
					// // Dev2ServiceDetailsStep3
					// //$("#Dev2ServiceDetailsStep3").css("display", "none");
				// }
			// }else{
				// $("#Dev2ServiceDetailsDoneButton").css("display", "none");
			// }
			if(isWorkerServiceType(editServiceType)){
				$("#Dev2ServiceDetailsNext").removeClass("hiddenBtn");
			}
			
			// lock category on edit
			if(editServiceType == "Website" || editServiceType == "Webpage" || category == "Website" || category == "Webpage" || category =="Human Interface Workflow"){
				// Dev2ServiceDetailsCategory
				$("#Dev2ServiceDetailsCategory").attr("disabled", "true");
			}
			
		}else{
		
			if(isWorkerServiceType(editServiceType)){
				$("#Dev2ServiceDetailsNext").removeClass("hiddenBtn");
				$("#Dev2ServiceDetailsDoneButton").css("display", "none");
			}else{
				$("#Dev2ServiceDetailsDoneButton").css("display", "in-line");
				$("#Dev2ServiceDetailsNext").addClass("hiddenBtn");
				// set category depending upon type
				if(editServiceType == "Website"){
					// Dev2ServiceDetailsCategory
					$("#Dev2ServiceDetailsCategory").val("Website");
					$("#Dev2ServiceDetailsCategory").attr("disabled", "true");
				}else if(editServiceType == "Webpage"){
					$("#Dev2ServiceDetailsCategory").val("Human Interface Workflow");
					$("#Dev2ServiceDetailsCategory").attr("disabled", "true");
				}
			}
		}
		
		// if user has altered source, disable next button
		if(noName == 1){
			//$("#Dev2ServiceDetailsDoneButton").css("display", "none");
			$("#Dev2ServiceName").removeAttr("readonly");
		}
		
		// init the type of work drop down
		initDev2ServiceDetails(webServer,editServiceType, editService, sourceName);
		
		
		/* when category textbox has focus fire autocomplete feature */
		$( "#Dev2ServiceDetailsCategory" ).autocomplete({
			source: buildCategories(webServer) ,
			minLength: 0,
			delay: 0
		});
		
		/* Browser server button is clicked */
		$("#Dev2ServiceDetailsSelectIconButton").click(function(){
			$("#Dev2ServiceDetailsServerDialog").dialog('destroy');
			$("#Dev2ServiceDetailsServerTree").dynatree('destroy');
			serverDialog(webServer);
		});
		
		$( "#Dev2ServiceDetailsCategory" ).focus(function(){     
			$(this).data("autocomplete").search($(this).val());
		});
		
		// a change to source invalidates the ability to save from this screen
		$("#Dev2ServiceDetailsSource").change(function(){
			// Is the done button visible?
			var curVal = $(this).val();
			if(curVal != sourceName){
				$("#Dev2ServiceDetailsDoneButton").css("display", "none");
				// flip step 3 button 
				if(isWorkerServiceType(editServiceType)){
					$("#Dev2ServiceDetailsStep3").addClass("hiddenBtn");
					// we also need to adjust the next button to step 2
					// Dev2ServiceDetailsNext
					$("#Dev2ServiceDetailsNext").attr("value","Next");
				}
			}else{
				$("#Dev2ServiceDetailsDoneButton").css("display", "inline");
				// flip step 3 button 
				if(isWorkerServiceType(editServiceType)){
					$("#Dev2ServiceDetailsStep3").removeClass("hiddenBtn");
					// we also need to adjust the next button to step 2
					// Dev2ServiceDetailsNext
					$("#Dev2ServiceDetailsNext").attr("value","Step 2");
					$("#Dev2ServiceDetailsNext").html("Go To Step 2");
				}
			}
			
			
		});
		
		/* Service Name validation */
		$("#Dev2ServiceName").focusout(function(){
			// check name against those in the catalog
			var currentVal = stripNaughtyChars($("#Dev2ServiceName").val());
			
			$("#Dev2ServiceName").val(currentVal);
			
			if(editService != currentVal){
				var serviceData = fetchMapping(webServer, currentVal, "*");
				// Name="travwebpage"
				if(serviceData.toLowerCase().indexOf('name="'+currentVal.toLowerCase()+'"') > 0){
					// we have an issue, 
					alert("Please select a different name, <i>" + currentVal + "</i> is already in use.", "Service Naming Error");
					//$("#Dev2ServiceName").focus();
					$("#Dev2ServiceDetailsNext").css("display", "none");
				}else{
					nameOk = true;
					if(isWorkerServiceType(editServiceType)){
						$("#Dev2ServiceDetailsNext").css("display", "inline");
					}
				}
			}else{
				nameOk = true;
				if(isWorkerServiceType(editServiceType)){
					$("#Dev2ServiceDetailsNext").css("display", "inline");
				}
			}
		});

	});
}

function isCategoryLocked(editServiceType){
	var result = false;

	if(editServiceType == "Webpage" || editServiceType == "Website"){
		result = true;
	}
	
	return result;
}

function isWorkerServiceType(editServiceType){
	var result = false;
	
	if(editServiceType == "Plugin" || editServiceType == "Database" ){
		result = true;
	}
	
	return result;
}

// Pass all related service data for write from screen 1
function writeServiceConfig(method, serviceName, serviceType, sourceName, category, help, icon, desc, tags, webServer){

	var payload = fetchServicePayload(webServer, serviceName);
	// TODO : Change to buildNewService
	var update = buildServiceUpdate(payload, method, serviceName, sourceName, category, help, icon, desc, tags, webServer );
	var status = updateServiceDef(webServer,update);
	
	//alert(status, serviceName+ " Update Status");

}

//serviceName, sourceName, category, help, icon, desc, tags, webServer
function buildServiceUpdate(data, method, serviceName, sourceName, category, help, icon, desc, tags, webServer ){
	// replace name, type, sourcename, sourceMethod, category

	if(data != '' && method != '' && sourceName != ''){
		var idx = data.indexOf("<Dev2WorkerServiceContents>");
		var endIdx = data.indexOf("</Dev2WorkerServiceContents>");
		
		var tmp = data.substr((idx+27), (endIdx - (idx+27)));
		
		data = tmp;
			
		// adjust category
		idx = data.indexOf("<Category>"); // + 11
		
		if(idx >= 0){
			endIdx = data.indexOf("</Category>");
			pt1 = data.substr(0, (idx+10));
			pt2 = data.substr(endIdx);
			pt1 += category + pt2;
			data = pt1;
		}
		
		// adjust comment
		idx = data.indexOf("<Comment>"); // + 11
		
		if(idx >= 0){
			endIdx = data.indexOf("</Comment>");
			pt1 = data.substr(0, (idx+10));
			pt2 = data.substr(endIdx);
			pt1 += desc + pt2;
			data = pt1;
		}
		
		// adjust tags
		idx = data.indexOf("<Tags>"); // + 11
		
		if(idx >= 0){
			endIdx = data.indexOf("</Tags>");
			pt1 = data.substr(0, (idx+10));
			pt2 = data.substr(endIdx);
			pt1 += tags + pt2;
			data = pt1;
		}
		
		// adjust help
		idx = data.indexOf("<HelpLink>"); // + 11
		
		if(idx >= 0){
			endIdx = data.indexOf("</HelpLink>");
			pt1 = data.substr(0, (idx+10));
			pt2 = data.substr(endIdx);
			pt1 += help + pt2;
			data = pt1;
		}
}
	var roles = "<Roles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,DEV2 Limited Internet Access</Roles>";
	var result = "<Payload>" + roles + "<ResourceXml>" + data +"</ResourceXml></Payload>" ;
		
	return result;
}


// Write service details out
function bindDev2ServiceDetailsDoneButton(editServiceType, webServer, method){

	if(checkRequired() && nameOk){ // ensure required fields are present

		if(isWorkerServiceType(editServiceType)){
			writeServiceConfig( method,
								$("#Dev2ServiceName").val() , 
								editServiceType, 
								$("#Dev2ServiceDetailsSource").val(),
								$("#Dev2ServiceDetailsCategory").val(),
								$("#Dev2ServiceDetailsHelp").val(),
								$("#Dev2ServiceDetailsIcon").val(),
								$("#Dev2ServiceDetailsDescription").val(),
								$("#Dev2ServiceDetailsTags").val(),
								webServer
							  );
			
		}
		
		ExitWithStudioPush(buildWorkflowClosePayload());
		
	}
}

function stripNaughtyChars(val){
	
	var result = val.replace(/[^a-zA-Z 0-9_]+/g,'');
	
	return result;
}

// Map wizard types to studio types
function convertToStudioType(wizType){
	var result = "WorkflowService";
	
	if(isWorkerServiceType(wizType)){
		result = "Service";
	}else if(wizType == "Webpage"){
		result = "WorkflowService";
	}
	
	// TODO : Add additional types for conversion 

	return result;
}

function buildWorkflowClosePayload(){
	var result = "<Dev2WizardPayload>";

	//ResourceName
	result += "<ResourceName>" + stripNaughtyChars($("#Dev2ServiceName").val()) + "</ResourceName>";
	//ResourceType { WorkflowService, Service { Plugin, Database }, Source, Website, HumanInterfaceProcess }
	result += "<ResourceType>"  + convertToStudioType(stripNaughtyChars($("#Dev2ServiceDetailsWorkType").val())) + "</ResourceType>";
	//Category
	result += "<Category>" + $("#Dev2ServiceDetailsCategory").val() + "</Category>";
	//Comment
	result += "<Comment>" + $("#Dev2ServiceDetailsDescription").val() + "</Comment>";
	//Tags
	result += "<Tags>" + $("#Dev2ServiceDetailsTags").val() + "</Tags>";
	//IconPath
	result += "<IconPath>" + $("#Dev2ServiceDetailsIcon").val() + "</IconPath>";
	//HelpLink
	result += "<HelpLink>" + $("#Dev2ServiceDetailsHelp").val() + "</HelpLink>";
	
	result += "</Dev2WizardPayload>";
	
	return result;
}

function buildCategories(webServer){
	/* compile a list of available worker service categories */
	var categories=[];
	var cleanCategories =[];

	$.ajax({
		type:"GET",
		url:webServer+"/services/FindResourcesService",
		data:{"ResourceName":"*","ResourceType":"*","Roles":"*"},
		dataType:"xml",
		async:false,
		success:function(xml){
		
			var errorHandler = new Dev2ErrorHandler(xml);
				
			if(!errorHandler.Has200ErrorPayload()){
		
				$(xml).find("Dev2WorkerServiceCategory").each(function(){
					var val = $(this).text();
					var found = false;
					var pos = 0;
					while(pos < categories.length && !found){
						if(val.toLowerCase() == categories[pos].toLowerCase()){
							found = true;
						}
						pos++;
					}
					
					if(!found){
						categories.push(val);
					}
				});	
			}else{
				errorHandler.Handle200ErrorPayload();
			}
		},
		error:function(data){
			prompt("Error " + webServer);
			var errorHandler = new Dev2ErrorHandler(data);
			errorHandler.HandleNon200ErrorPayload();
		}
	});
	
	return categories;
}

function initDev2ServiceDetails(webServer,editServiceType, editService, sourceName){

	// build up the source list
	buildSourceList(webServer, sourceName, editServiceType);

	// Dev2ServiceDetailsWorkType
	$("#Dev2ServiceDetailsWorkType >option").each(function(){
		if($(this).val() == editServiceType){
			$(this).attr("selected", "selected");
		}
	});
	
	// default selection
	if(editServiceType == ''){
		$("#Dev2ServiceDetailsWorkType:eq(1)").attr("selected", "selected");
	}
}

function toggleServiceAdvanced(editServiceType){
	toggleAdvanced(); 
	$("#Dev2ServiceDetailsWorkType").attr("disabled", "true"); 
	
	// if worker service disable the icon junk
	if(isWorkerServiceType(editServiceType)){
		$("#Dev2ServiceDetailsIcon").css("display", "none");
		$("#Dev2ServiceDetailsIconLabel").css("display", "none");
		$("#Dev2ServiceDetailsSelectIconButton").css("display", "none");
	}
	
	// if webpage or website lock category
	if(isCategoryLocked(editServiceType)){	
		$("#Dev2ServiceDetailsCategory").attr("disabled", "true");
	}
		
}

function toggleSourceRegion(webServer, serviceType){

	$("#Dev2ServiceDetailsWorkType").attr("disabled", "true");
	
	if(isWorkerServiceType(serviceType)){
		$("#Dev2ServiceDetailsOpenSourceManagement").css("display", "inline");
	}else{
		$("#Dev2ServiceDetailsOpenSourceManagement").css("display", "none");
		$("#Dev2ServiceDetailsSource").css("display", "none");
		$("#Dev2ServiceDetailsSourceLabel").css("display", "none");
	}
}

function buildSourceList(webServer, sourceName, serviceType){

	$.ajax({
		type:"GET",
		url:webServer+"/services/FindResourcesService",
		data:{"ResouceName":"*","ResourceType":"Source","Roles":"*"},
		dataType:"xml",
		async : false,
		success:function(xml){
		
			var errorHandler = new Dev2ErrorHandler(xml);
				
			if(!errorHandler.Has200ErrorPayload()){
		
				$("#Dev2ServiceDetailsSource").html('');
				
				// enable button, else disable it
				toggleSourceRegion(webServer, serviceType);
				
				$(xml).find("Dev2Resource").each(function(){
					var type = $(this).find("Dev2SourceType").text();
					if(type.indexOf(serviceType) != -1){
						var val = $(this).find("Dev2SourceName").text();
						// sourceName
						if(val == sourceName){
							$("#Dev2ServiceDetailsSource").append("<option selected>"+val+"</option>");
						}else{
							$("#Dev2ServiceDetailsSource").append("<option>"+val+"</option>");
						}
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
}

function initDev2ServiceDetailsSource(webServer, editService, editServiceType){
	/* populate source drop down list */
	$.ajax({
		type:"GET",
		url:webServer+"/services/FindResourcesService",
		data:{"ResouceName":editService,"ResourceType":editServiceType,"Roles":"*"},
		dataType:"xml",
		success:function(xml){
		
			var errorHandler = new Dev2ErrorHandler(xml);
				
			if(!errorHandler.Has200ErrorPayload()){
		
				$(xml).find("Dev2Resource").each(function(){
					var type = $(this).find("Dev2SourceType").text();
					if(type.indexOf("Database") != -1){
						$("#Dev2ServiceDetailsSource").append("<option>"+$(this).find("Dev2SourceName").text()+"</option>");
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
}

/* region HELPER FUNCTIONS */
function getPath(node){
	var pNode = node.parent;
	if(path==""){
		path=node.data.title;
	}
	if(pNode.data.title!=null){
		path=pNode.data.title+"/"+path;
		getPath(pNode);
	}
}

/* open server dialog */
function serverDialog(webServer){
	/* build application server file system tree */
	var serverPath = "";
	
	var origVal = $("#Dev2ServiceDetailsIcon").val();
	
	$("#Dev2ServiceDetailsServerTree").dynatree({
		autoCollapse:false,
		checkbox: false,
		selectMode: 1,
		keyPathSeparator: "/",
		initAjax:{
			url:webServer+"/list/icons/"
		},
		onActivate: function(node){
			path="";
			$("#Dev2ServiceDetailsIcon").val(node.data.title);
		}
	});
	
	/* build application server dialog */
	$("#Dev2ServiceDetailsServerDialog").dialog({
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
					$(this).dialog("close"); 
				}
			},
			{text:"Cancel",
				click : function(){ 
					$(this).dialog("close"); 
					$("#Dev2ServiceDetailsIcon").val(origVal);
				}
			}
		]
	});
	
	/* hide the dialog title bar */
	$(".ui-dialog-titlebar").hide();
	/* open dialog */
	$("#Dev2ServiceDetailsServerDialog").dialog('open');
}

// bindDev2ServiceDetailsOpenSourceManagement("[[Dev2WebServer]]","[[Dev2ServiceType]]","[[Dev2SourceMethod]]");
function bindDev2ServiceDetailsOpenSourceManagement(webServer, serviceType, pluginFn, isNew){
	// Dev2ServiceDetailsWorkType
	//var work = $('#Dev2ServiceDetailsWorkType option:selected').val();
	var srcMgtLoc = "";
	var sourceName = $("#Dev2ServiceDetailsSource option:selected").val();
	
	// build up the data values
	var serviceName = $("#Dev2ServiceName").val();
	var category = $("#Dev2ServiceDetailsCategory").val();
	var help = $("#Dev2ServiceDetailsHelp").val();
	var icon = $("#Dev2ServiceDetailsIcon").val();
	var desc = $("#Dev2ServiceDetailsDescription").val();
	var tags = $("#Dev2ServiceDetailsTags").val();
	
	var base = "&Dev2ServiceName="+serviceName+"&Dev2ServiceType="+serviceType+"&Dev2SourceName="+sourceName+"&Dev2Category="+category+"&Dev2Help="+help+"&Dev2Icon="+icon+"&Dev2Description="+desc+"&Dev2Tags="+tags;

	if(serviceType == "Database"){
		srcMgtLoc = "DatabaseSourceManagement?Dev2SourceManagementDatabaseSource="+sourceName+base;
	}else if(serviceType == "Plugin"){
		srcMgtLoc = "PluginSourceManagement?Dev2SourceManagementSource="+sourceName+base;
	}
	
	if(isNew != undefined){
		srcMgtLoc += "&Dev2NewService=" + isNew;
	}
	
	if(srcMgtLoc != ''){
		window.location = webServer +"/services/"+srcMgtLoc;
	}
}
