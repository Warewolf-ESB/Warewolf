/* 
   24.08.2012
   Travis.Frisinger
   Created For Plugin Step2 Since Injecting script in the webpart sucks due to <> char issues!!!
*/
var fns = new Array();
var testDataOk = false;

function initPluginStep2(webServer, sourceMethod, sourceName){

	$(document).ready(function(){

		var asmData = loadFunctions(webServer, sourceMethod, sourceName);
	
		if(sourceMethod != ''){
			renderInputTestRegion(sourceMethod, asmData[1], asmData[0], webServer);
		}
	
		/* Dev2PluginFunction Click Handling */
		$("#Dev2PluginFunction").click(function(){
			// now fetch input mapping and render
			var idx = $("#Dev2PluginSource option:selected").index();
			idx -= 1;
			renderInputTestRegion($(this).val(), asmData[1], asmData[0], webServer);
		});

		/* Dev2PluginStep2TestBtn Click Handling */
		$("#Dev2PluginStep2TestBtn").click(function(){
			renderQueryWindow(asmData[1], asmData[0], webServer, true, testDataOkFn);	
		});
		
		/* Bind next button, adjust the buttons passed in*/
		$("#Dev2PluginStep2NextBtn").click(function(){
			if(!testDataOk){
				renderQueryWindow(asmData[1], asmData[0], webServer, false);
			}else{
				// already set data, continue
				if(checkRequired()){
					document.forms[0].submit();
				}
			}			
		});
	});

}

function testDataOkFn(isOk){
	testDataOk = isOk;
}

// The magical fun start here ;)
function renderQueryWindow(asmLoc, asmName, webServer, isTestMode, callbackFn){

	if(asmLoc != undefined){		
		
		// function to handle payload fetch
		var handlerFn = function(theData){
			// Dev2InputTestData
			var rawInputs = "";
			// fetch inputs for object creation
			$(".Dev2InputTestData").each(function(){
				rawInputs += $(this).attr("fnName")+":"+$(this).attr("fnType")+",";
			});
			
			// Display the result to the user ;)
			var sampleData = new Dev2InputMapper(theData);

			var fnName = $("#Dev2PluginFunction option:selected").val();

			if(sampleData.HasPaths()){
				// set extract inputs
				sampleData.SetRawPluginInputs(rawInputs);
				// now display the execution result to the user
				sampleData.DisplayPathsWithDataAndPersist(fnName,"Dev2SamplePersist","Dev2InputPersist",dialogWin, isTestMode, callbackFn );
			}else{
				// no paths returned
				var errorAppend = "The plugin could not execute due to incorrect sample data.";
				if(rawInputs == ""){
					errorAppend = "Please select a new function to execute";
				}
				
				var errorBtns = {
					Ok: function () {
							$(this).dialog("close");
						}
				};
				
				dialogWin.redraw(errorAppend, errorBtns);
			}	
		};
	
		// start query
		var payload = fetchTestData(webServer, asmLoc, asmName, handlerFn);
	
		var queryBtns = {
			Cancel : function(){
				payload.abort();
				$(this).dialog("close");
			}
		};
		
		// display dialog
		var dialogWin = new Dev2Dialog('<div id="loadWait" style="color:#500; width:600px; margin-left:5px; font-size:12px; font-style:italic"><blink>Querying Plugin...</blink></div>', 
										$("#Dev2PluginFunction").val() + " Execution Results", 
										true,
										queryBtns
									   );	
	}		
}



// load the function list 
function loadFunctions(webServer, sourceMethod, sourceName){
	var matchIdx = -1;
	
	// fetch the source data we need to operate on
	var sourceData = fetchMapping(webServer, sourceName, "Source");
	var result = [];
	
	if(sourceData.indexOf("Dev2SourceContents") > 0){
		// we have data

		var locateData = new Dev2Find(sourceData);
		var assemblyName = locateData.GetAttribute("AssemblyName");
		var assemblyLoc = locateData.GetAttribute("AssemblyLocation");

		buildFunctionList(webServer, assemblyLoc, assemblyName, sourceMethod, "Dev2PluginFunction");
		
		result[0] = assemblyName;
		result[1] = assemblyLoc;
		
		// does not work with IE :(
		/*$(sourceData).find("Dev2Resource").each(function(){
			
			var assemblyName = $(this).find("Dev2SourceContents").find("Source").attr("AssemblyName");
			var assemblyLoc = $(this).find("Dev2SourceContents").find("Source").attr("AssemblyLocation");
			
			alert(assemblyName + " " + assemblyLoc);
			
			buildFunctionList(webServer, assemblyLoc, assemblyName, sourceMethod, "Dev2PluginFunction");
			
			result[0] = assemblyName;
			result[1] = assemblyLoc;
			
		});*/
	}else{
		// no data
		$("#Dev2PluginFunction").html("Error : No functions found");
	}
	
	return result;
}

// build up the function list
function buildFunctionList(webServer, asmLoc, nameSpace, sourceMethod, appendRegion){

	$.ajax({
		type:"GET",
		url:webServer+"/services/PluginRegistryService",
		data:{"AssemblyLocation": asmLoc, "NameSpace" : nameSpace, "ProtectionLevel" : "public"},
		dataType:"xml",
		success:function(xml){
			$("#" + appendRegion).html('');
			var idx = 0;
			$(xml).find("Dev2Plugin").each(function(){
			
				var namespace = $(this).find("Dev2PluginSourceNameSpace").text();
				
				/* extract each function for this namespace */
				$(this).find("Dev2PluginExposedMethod").each(function(){
					var val = $(this).text();
					var opt = val;
					
					var toAdd = "<option value='" + opt + "'>"+val+"</option>";
										
					if(opt == sourceMethod){
						toAdd = "<option value='" + opt + "' selected>"+val+"</option>";
					}
					
					// do some filtering
					var canAdd = true;
					var pos = 0;
					
					while(pos  < fns.length && canAdd){
						if(fns[pos] === toAdd){
							canAdd = false;
						}
						pos++;
					}
					
					if(canAdd){
						fns[idx] = toAdd;
					}
					
					idx++;
				
				});
				
			});
			
			// now add all that can be ;)
			for(var i = 0; i < fns.length; i++){
				$("#"+appendRegion).append(fns[i]);
			}
		},
		error:function(data){
			var errorHandler = new Dev2ErrorHandler(data);
			errorHandler.HandleNon200ErrorPayload();
		}
	});
}

/*
 Returns either the fetched string in a sync call, or the xmlhttp request object on an async call ;)
*/
function fetchTestData(webServer, asmLoc, asmName, asyncFn){

	// Fetch args ;)
	var args = "";
	// process each arg
	$(".Dev2InputTestData").each(function(){
		var val = $(this).val();
		var pType = $(this).attr("fnType"); // custom attribute we placed there ;)
		args += "<Arg><Value>" + val + "</Value><TypeOf>" + pType +  "</TypeOf></Arg>";
	});
	
	if(args == ""){
		args = "<Arg></Arg>";
	}
	
	// now fetch the payload, in io_mapping_setup.js
	var payload = "";
	
	if(asyncFn == undefined){
		payload = fetchMappingPaths(webServer, asmLoc, asmName, $("#Dev2PluginFunction option:selected").val(), args, asyncFn );
	}else{
		payload = fetchMappingPaths(webServer, asmLoc, asmName, $("#Dev2PluginFunction option:selected").val(), args, asyncFn );
	}
	
	return payload;
}

/* Used on Dev2PluginSearchFilter */
function bindDev2PluginSearchFilter(){
	$(document).ready(function(){
	/* Bind filter */
		$("#Dev2PluginSearchFilter").keyup(function(event){
			var val = $("#Dev2PluginSearchFilter").val().toLowerCase();

			if(val != ''){
				// clear the list
				$("#Dev2PluginFunction").html('');
			
				for(var i = 0; i < fns.length; i++){
					try{
						if(fns[i].toLowerCase().indexOf(val) >= 0){
							$("#Dev2PluginFunction").append(fns[i]);
						}
					}catch(e){}
				}
			}else{
				// clear the list
				$("#Dev2PluginFunction").html('');

				for(var i = 0; i < fns.length; i++){
					$("#Dev2PluginFunction").append(fns[i]);
				}
			}
		});
	});
	
	$("#Dev2PluginSearchFilter").val("");
}

//* show inputs for service tickle */
function renderInputTestRegion(fn, asmLoc, asmName, webServer){
	
	$.ajax({
			type:"GET",
			url:webServer+"/services/PluginRegistryService",
			data:{"AssemblyLocation": asmLoc, "NameSpace" : asmName, "MethodName" : fn },
			dataType:"xml",
			success:function(xml){
				// now extract and display input mapping ;)
				
				var initPayload = "<table><tr><td><b>Input</b></td><td><b>Test Data</b></td></tr>"
				var payload = initPayload;
				
				$(xml).find("Dev2PluginExposedSignature").each(function(){
					// Dev2PluginStep2InputRegion
					var child = $(this).children();
					
					for(var i = 0; i < child.length; i++){
						var name = $(child[i]).text();
						
						if(child[i].nodeName == "Dev2PluginArg"){
							// we have an input to generate
							var idx = name.indexOf(":");
							var paramName = name.substr((idx+2));
							var paramType = name.substr(0, (idx-1));
							payload += "<tr><td>" + name + "</td><td><input class='Dev2InputTestData' id='Dev2InputTestData' fnName='" + paramName + "' fnType='" + paramType +"' style='width:200px;'/></td></tr>";
						}
					}			
				});
				
				if(payload == initPayload){
					payload = "<table>"; // clear out the input region since there are no inputs to test with
				}

				payload += "</table>";
				
				$("#Dev2PluginStep2InputRegion").html(payload);
							
				$("#Dev2PluginStep2TestBtn").removeClass("hiddenBtn");
				$("#Dev2PluginStep2NextBtn").removeClass("hiddenBtn");
				
			},
			error:function(data){
				var errorHandler = new Dev2ErrorHandler(data);
				errorHandler.HandleNon200ErrorPayload();
			}
	});
	
	
}

/* Bound to Dev2PluginStep2DoneBtn*/
function bindDev2PluginStep2DoneBtn(hasService){
	$(document).ready(function(){
		if(hasService != ''){
			$("#Dev2PluginStep2DoneBtn").removeClass("hiddenBtn");
			$("#Dev2PluginStep2NextBtn").removeClass("hiddenBtn");
		}
	});
}

function fetchServicePayload(webServer, serviceName){
	return fetchMapping(webServer, serviceName, "Service");
}

function fetchSourcePayload(webServer, serviceName){
	return fetchMapping(webServer, serviceName, "Source");
}

// Changed to use post update due to large service cut off
function updateServiceDef(webServer,update){
	var xmlhttp;
	
	if (window.XMLHttpRequest)
	{	// code for IE7+, Firefox, Chrome, Opera, Safari
		xmlhttp=new XMLHttpRequest();
	}
	else
	{	// code for IE6, IE5
		xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
	}
	xmlhttp.open("POST",webServer+"/services/AddResourceService",false);
	xmlhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
	xmlhttp.send(update);
	var status = xmlhttp.responseText;
	
	return status;
}

// bindDev2PluginStep2BackBtn("[[Dev2ServiceName]]","[[Dev2ServiceType]]","[[Dev2PluginFunction]]","[[Dev2ServiceDetailsCategory]]","[[Dev2SourceMethod]]","[[Dev2ServiceDetailsHelp]]","[[Dev2ServiceDetailsIcon]]", "", "[[Dev2ServiceDetailsTags]]"); 

// bindDev2PluginStep2BackBtn("[[Dev2ServiceName]]","[[Dev2ServiceType]]","[[Dev2ServiceDetailsSource]]","[[Dev2ServiceDetailsCategory]]","[[Dev2ServiceDetailsHelp]]","[[Dev2ServiceDetailsIcon]]","[[Dev2ServiceDetailsDescription]]","[[Dev2ServiceDetailsTags]]"); 
function bindDev2PluginStep2BackBtn(serviceDetailsName,serviceType, sourceName, category, help, icon, desc, tags, srcStatus, isNew, srcMethod){
	var loc = "/services/Dev2ServiceDetails";

	//if(serviceDetailsName != ''){
		//loc += "?Dev2ServiceType="+serviceType;
		
		loc += "?Dev2ServiceName="+serviceDetailsName+"&Dev2ServiceType="+serviceType+"&Dev2SourceName="+sourceName+"&Dev2Category="+category+"&Dev2Help="+help+"&Dev2Icon="+icon+"&Dev2Description="+desc+"&Dev2Tags="+tags;
	//}
	
	if(srcStatus != undefined){
		loc += "&DevSourceChange=" + srcStatus
	}
	
	if(isNew != undefined){
		loc += "&Dev2NewService=" + isNew;
	}
	
	if(srcMethod != undefined){
		loc += "&Dev2SourceMethod="+srcMethod;
	}
	
	window.location = loc;
}
