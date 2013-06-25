/* Extract Connection string data from sources*/
(function(){

	var _payload;
	var server;
	var db;
	var mode;
	var user;
	var pass;
	
	this.Dev2DBConStringUtil = function(str){
	
		_payload = "";
		_payload = str;
		
		// break into parts
		var parts = magicString.split(";");
		server = "";
		db = "";
		mode = "";
		user = "";
		pass = "";
		
		// Server=SAMEERC;Database=CollectionsProfilingEngine;Integrated Security=True
		
		if(parts.length < 3){
			alert("Malformed Source Information [ " + str + " ] ");
		}else{

		    server = parts[0].split("=")[1];
			db  = parts[1].split("=")[1];
		    mode = parts[2].split("=")[1];
			
			if(parts.length > 3){
				
				user = parts[3].split("=")[1];
				pass = parts[4].split("=")[1];
			}
		}
	},
	
	Dev2DBConStringUtil.prototype.IsDBAuth = function(){
		return (mode.toLowerCase() != "true");
	},
	
	Dev2DBConStringUtil.prototype.FetchServer = function(){
		return server;
	},
	Dev2DBConStringUtil.prototype.FetchDB = function(){
		return db;
	},
	Dev2DBConStringUtil.prototype.FetchMode = function(){
		return mode;
	},
	Dev2DBConStringUtil.prototype.FetchUser = function(){
		return user;
	},
	Dev2DBConStringUtil.prototype.FetchPassword = function(){
		return pass;
	}
})();

/* Hack to work around Internet Exploder's Jquery .find bug */
(function(){
	
	var _payload;
	var _indexes = new Array();
	var _keys = new Array();
	
	this.Dev2Find = function(payload){
		_payload = payload;
	},
	
	Dev2Find.prototype.GetAttribute = function(attr){
		
		var result = this.DoneKey();
		
		var start = _payload.indexOf(attr);
		if(start > 0){
			start += attr.length + 2;
			var end = _payload.indexOf('"', start);
			if(end > 0){
				result = _payload.substr(start, (end - start));
			}
		}
		
		return result;
	}
	
	/* interate across the entire data stream */
	Dev2Find.prototype.GetNextXmlFragment = function(key){
	
		var lastIdx = _indexes[key];
		if(lastIdx == undefined){
			lastIdx = 0;
		}
		
		var result = this.DoneKey();
		
		var startKey = "<"+key+">";
		var start = _payload.indexOf(startKey, lastIdx);
		if(start >= 0){
			var endKey = "</"+key+">";
			var end = _payload.indexOf(endKey, lastIdx);
			if(end > 0){
				// found a match
				
				// update last index
				_indexes[key] = (end + endKey.length);
				
				// extract the value
				result = _payload.substr( (start + startKey.length), end)
			}
		}else{
			start = _payload.indexOf("<"+key+"/>");
			if(start >= 0){
				result = "";
			}
		}
		
		return result;
	},
	
	Dev2Find.prototype.DoneKey = function(){
		return "__EMPTY__";
	}
})();


/* Used to extract existing service def data */
(function(){
	var _payload;
	
	this.Dev2ServiceFetch = function(webServer, serviceName){
		var xmlhttp;
	
		if (window.XMLHttpRequest)
		{	// code for IE7+, Firefox, Chrome, Opera, Safari
			xmlhttp=new XMLHttpRequest();
		}
		else
		{	// code for IE6, IE5
			xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
		}
		
		var uri = webServer + "/services/FindResourcesService";
		xmlhttp.open("POST",uri,false);
		xmlhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
		xmlhttp.send("ResourceName=" + serviceName + "&ResourceType=*" + "&Roles=*");
		
		_payload = xmlhttp.responseText;
	},
	
	/* Extract and return the anything to xml configuration */
	Dev2ServiceFetch.prototype.extractAnythingToXmlConfig = function(){
	// OutputDescription
		var tag = "OutputDescription";
		var idx = _payload.indexOf("<"+tag+">");
		var result = "";
		
		if(idx > 0){
			idx += (tag.length + 2);
			var end = _payload.indexOf("</" + tag + ">");
			end -= (tag.length + 3);
			result = _payload.substr(idx, (end - idx));
			
			result = result.replace("<Dev2XMLResult>","");
			result = result.replace("<![CDATA[","");
			result = result.replace("]]>","");
			result = result.replace("<JSON /","");
			result = result.replace("</Dev2XMLResult>","");
			result = "<"+tag+">" + result + "</"+tag+">";
		}
				
		return result;
	}
	
	Dev2ServiceFetch.prototype.extractInputs = function(){
		// Inputs
		var xDoc = $.parseXML(_payload);
		var result = "";
		
		$(xDoc).find("Input").each(function(){
			// Name, DefaultValue, Required
			var name = $(this).attr("Name");
			var defVal = $(this).attr("DefaultValue")+"";
			if(defVal === "undefined"){
				defVal = "";
			}
			var req = ($(this).find("Validator").attr("Type") == "Required");
			
			result += name+","+defVal+","+req+"|";
			
		});
		
		result = result.substr(0, (result.length - 1));
		
		return result;
	}
	
	Dev2ServiceFetch.prototype.extractOutputs = function(){
		// Inputs
		var xDoc = $.parseXML(_payload);
		var result = "";
		
		$(xDoc).find("Input").each(function(){
			// Name, DefaultValue, Required
			var name = $(this).attr("Name");
			var defVal = $(this).attr("DefaultValue")+"";
			if(defVal === "undefined"){
				defVal = "";
			}
			var req = ($(this).find("Validator").attr("Type") == "Required");
			
			result += name+","+defVal+","+req+"|";
			
		});
		
		result = result.substr(0, (result.length - 1));
		
		return result;
	}

})();

/* Dialog window object*/
(function(){
	var _win;
	var _title;
	var btns;
	var _div;
	
	// object constructor
	this.Dev2Dialog = function(payload, title, resize, btns) {
		
		_win = $('<div></div>').appendTo('body')
				.html(payload)
				.dialog({
					modal: true, 
					title: title, 
					zIndex: 10000, 
					autoOpen: true,
					width: 790, 
					height: 500,
					resizable: true,
					close: function (event, ui) {
						$(this).remove();
					}
				});
			
		this.setButtons(btns);

	},
	
	// used to update the dialog window contents
	Dev2Dialog.prototype.redraw = function(payload, newBtns){
		_win.html(payload);
		this.setButtons(newBtns);
	},
	
	// set new buttons on the dialog
	Dev2Dialog.prototype.setButtons = function(btns){
		if(btns != null && btns != undefined){
			_win.dialog('option', 'buttons', btns);
		}
	}
	
})();

/* Object used for rendering IO paths */
(function(){
		var origPayload = "";
		var userExpressions = new Array(); // used to store the user entered expression
		var displayPaths = new Array();
		var sampleData = new Array();
		var defaultMapping = new Array();
		var impactedScalarToRS = new Array();
		
		var rawPluginInputs = "";
		var mappedPluginInputs = "";
		var argsLen = 0;
		var saneParse = false;
		var magicStr = "<OutputExpression";
		var dataFragStr = "SampleData";
		var displayPathStr = "DisplayPath";
		var outputExprStr = "OutputExpression";
		var termTagForExp = "</d1p1:Paths>";
		
		var typeOf = "";
		var inputCount = 0;
		// used to write out the config
		var serviceName;
		var sourceName;
		var help;
		var icon;
		var description;
		var tooltip;
		var tags;
		var category;
		var sourceMethod;
		var webServer;
		
		var manualID = 0;
		var manualDeleteIdx = [];
		var manualPathAddType = [];
		var manualDeleteCnt = 0;
		
		var cdataOpen = "<![CDATA[";
		var cdataEnd = "]]>";
		
		var inAdvMode = false;
		
		// used to store template fragments 
		var outerFragments = new Array(); // should be 4 fragments that are needed to re-create the original document that was passed in

		/* Object constructor */
		this.Dev2InputMapper = function(payload) {

			// init all stuff
			origPayload = "";
			userExpressions = new Array(); // used to store the user entered expression
			displayPaths = new Array();
			sampleData = new Array();
			defaultMapping = new Array();
			impactedScalarToRS = new Array();
			argsLen = 0;
			manualDeleteIdx = [];
			manualID = 0;
			manualDeleteCnt = 0;
		
			rawPluginInputs = "";
			mappedPluginInputs = "";
		
			// save payload for manip, ensure it is CDATA free ;)
			if(payload.indexOf(cdataOpen) == 0){
				// we have a cdata region trim it
				payload = payload.replace(cdataOpen,"");
				payload = payload.replace(cdataEnd, "");
			}
			
			payload = payload.replace("!",""); // for some funny reason this keeps appearing in the data stream
			origPayload = payload;
		
			// load payload into XML document
			xmlDoc = $.parseXML(payload);
			$payload = $(xmlDoc);
			
			// extract current expressions
			var pos = 0;
			$payload.find(outputExprStr).each(function(){
				var val = $(this).text();
				userExpressions[pos] = val;
				pos++;
			});
			
			// extract display paths
			pos = 0;
			$payload.find(displayPathStr).each(function(){
				var val = $(this).text();
				
				displayPaths[pos] = val;
				sampleData[pos] = "NA"; // TODO : Extract real sample data
				pos++;
			});
			
			// finally extract the sample data
			pos = 0;
			$payload.find(dataFragStr).each(function(){
				var val = $(this).text();
				sampleData[pos] = val;
				pos++;
			});
			
			// SetRawPluginInputs sanity
			if(displayPaths.length != userExpressions.length){
				alert("Insane data! Aborted parsing");
				userExpressions = new Array();
				displayPaths = new Array();
			}else{
				argsLen = userExpressions.length;
				saneParse = true;
			}

		},
		
		/* Used to figure out if an object has valid data or not !*/
		Dev2InputMapper.prototype.HasPaths = function(){
			return (displayPaths.length > 0);
		
		},
		
		/* Set Raw Plugin Inputs as extract per the sample generation process */
		Dev2InputMapper.prototype.SetRawPluginInputs = function(raw){
			rawPluginInputs = "";
			rawPluginInputs = raw; 
		},
		
		/* Set predefined inputs */
		Dev2InputMapper.prototype.setDefinedPluginInputs = function(inputs){
			mappedPluginInputs = "";
			mappedPluginInputs = inputs;
		},
		
		/* Used to set the critical details required to persist this service */
		Dev2InputMapper.prototype.SetPersistData = function(servName, srcName, srcMethod, wServer, type, sHelp, sIcon, sDescription, sTooltip,sCategory, sTags){
			serviceName = servName;
			sourceName = srcName;
			sourceMethod = srcMethod;
			webServer = wServer;
			typeOf = type;
			help = sHelp;
			icon = sIcon;
			description = sDescription;
			tooltip = sTooltip;
			category = sCategory;
			tags = sTags;
		},
		
		/* Used to update a path as per Dev2's data lang */
		Dev2InputMapper.prototype.SetUserExpression = function(idx, expression){
			userExpressions[idx] = expression;
		},
		
		/* Used to delete a path as per Dev2's data lang */
		Dev2InputMapper.prototype.DeleteUserExpression = function(idx){
			if(idx < userExpressions.length){
				delete userExpressions[idx];
			}else{
				alert("Invalid index passed to Expression delete?! Operation aborted.");
			}
		},
		
		/* Used to display the IO paths complete with sample data */
		Dev2InputMapper.prototype.DisplayPathsWithDataAndPersist = function(fnName, persistRegion, inputPersist, dialogWin, isTestMode, callBackFn){

			// buttons for the dialog
			var dataBtns = {
						No: function () {
							$("#" + inputPersist).val("");
							$("#" + persistRegion).val("");
							
							saneParse = false;
							rawPluginInputs = "";
							origPayload = "";
							
							if(callBackFn != undefined){
								callBackFn(false);
							}
							
							$(this).dialog("close");
						},
			
						Yes: function () {
							
							// push orig payload into persist region for carry into next step
							// CDATA delimit to get the data through IO shapping
							$("#" + persistRegion).val(cdataOpen + origPayload +cdataEnd);
							
							var inputs = "";
							inputs = rawPluginInputs;
							
							// push input mapping down now
							$("#" + inputPersist).val(inputs);

							// validate the information then send to next screen ;)
							if(!isTestMode){
								if(checkRequired()){
									document.forms[0].submit();
								}
							}else{
								callBackFn(true);
							}
							
							$(this).dialog("close");
						}		
					};
		
		
			if(saneParse){
				var payload = "<table><tr><td><b>Path</b></td><td><b>Sample Data</b></td></tr>";
				
				for(var i = 0; i < displayPaths.length; i++){
					var path = displayPaths[i];
					// format for display
					payload += "<tr><td><i>"+path + "</i></td><td>" + sampleData[i] + "</td></tr>";
				}
				
				if(!isTestMode){
					payload += "</table><div style='width:680px; margin-top:10px;'><center><b><i>Would you like to continue with these results?</i></b></center></div>";
					
				}else{
					payload += "</table><div style='width:680px; margin-top:10px;'><center><b><i>Is this the test data you expected?</i></b></center></div>";
				}
				
				dialogWin.redraw(payload, dataBtns);
			}else{
				
				var errorBtns = {
					Ok: function () {
							$(this).dialog("close");
						}
				};
				
				dialogWin.redraw("Error : There is an error querying the plugin for data!", errorBtns);
			}
		},
		
		/* Used to render the inputs region on the IO mapping screen */
		Dev2InputMapper.prototype.RenderInputsForIOMapping = function(nameRegion){
			
			// use plugin inputs data for rendering
				// does it have input mapping data, should always be populated?
				var namePayload = "";
				//var valuePayload = "<table class='ioMappingRegion'>";
				//var requiredPayload = "<table class='ioMappingRegion'>";
					
				if(rawPluginInputs.length > 0){
					// we all good ;)
					var inputParts = rawPluginInputs.split(",");
					
					for(var i = 0; i < inputParts.length; i++){
						if(inputParts[i].length > 0){
							var disPath = inputParts[i];
							if(disPath.length >= 25){
								disPath = disPath.substr(0,24) + "...";
							}
				
							namePayload += "<div class='ioMappingInputRow'><div class='ioMappingInputCol1' title='" + inputParts[i] +"' >" + disPath +"</div>";
							namePayload += "<div><input id='Dev2IOMappingDefaultValue' type='text' class='ioMappingInputCol2 Dev2IOMappingDefaultValue'/></div>";
							namePayload += "<div><input type='checkbox' id='Dev2IOMappingRequired' class='ioMappingInputCol3 Dev2IOMappingRequired'/></div>";
							namePayload += "<div><input type='checkbox' id='Dev2IOMappingNull' class='ioMappingInputCol3 Dev2IOMappingRequired'/></div></div>";
						}
					}
				}else if(mappedPluginInputs.length > 0){
					// we have pre-defined inputs to render ;)
					var inputParts = mappedPluginInputs.split("|");
					for(var i = 0; i < inputParts.length; i++){
						// PolicyNo,,false|ExtractScheduleID,,false
						var tmpParts = inputParts[i].split(",");
							// Name, default value, required
							var disPath = tmpParts[0];
							if(disPath.length >= 25){
								disPath = disPath.substr(0,24) + "...";
							}
							namePayload += "<div class='ioMappingInputRow'><div class='ioMappingInputCol1' title='" + tmpParts[0] +"' >" + disPath +"</div>";
							namePayload += "<div><input id='Dev2IOMappingDefaultValue' type='text' class='ioMappingInputCol2 Dev2IOMappingDefaultValue' value='" + tmpParts[1] + "'/></div>";
							if(tmpParts[2] == "true"){
								namePayload += "<div><input type='checkbox' id='Dev2IOMappingRequired' class='ioMappingInputCol3 Dev2IOMappingRequired' checked/></div></div>";
							}else{
								namePayload += "<div><input type='checkbox' id='Dev2IOMappingRequired' class='ioMappingInputCol3 Dev2IOMappingRequired'/></div></div>";
							}
					}
				}else{
					// nothing to render, display it as such
					namePayload += "<div class='ioMappingInputRow'><div class='ioMappingInputCol1'>NA</div><div class='ioMappingInputCol2'>NA</div><div class='ioMappingInputCol3'>NA</div></div>";
				}

				// now update the regions
				$("#" + nameRegion).html(namePayload);
		},
		
		/* Toggle the mode for advancedRegion append */
		Dev2InputMapper.prototype.ToggleMode = function(){
			
			if(inAdvMode){
				inAdvMode = false;
			}else{
				inAdvMode = true;
			}
		},
		
		/* Used to append the manual output mapping region */
		Dev2InputMapper.prototype.AppendOutputMapping = function(appendNameRegion){
			var payload = "";
			
			var val = "..";
			var disPath = "..";
			if(disPath.length >= 35){
				disPath = disPath.substr(0,34) + "...";
			}
			// advancedRegion
			payload += "<div class='ioMappingInputRow' id='manual" +manualID +"'><input class='ioMappingOutputCol1Input' title='The Output Path Into the Result Data' id='output" + manualID + "' />";
			payload += "<div><input type='text' name='Dev2OutputLocation' class='Dev2OutputLocation ioMappingOutputCol2' value='' title='The Alias for the Output Path' id='alias" + manualID +"' /></div>";
			payload += "<div class='ioMappingOutputCol3'><img src='/themes/system/images/cross.png' title='Delete Manual Output Entry' height='16' width='16' onClick='deleteEntry(" + manualID + ",true)'/></div><div class='ioMappingOutputCol3'><img src='/themes/system/images/tick.png' title='Confirm Manual Output Entry' height='16' width='16' onClick='addEntry(" + manualID + ")'/></div></div>";

			
			$("#" + appendNameRegion).append(payload);
			
			// make region visible
			if(inAdvMode){
				$('#manual'+manualID).show();
			}
			
			// inc manual id
			manualID++;
			
		},
		
		/* Used to clear object from manual delete */
		Dev2InputMapper.prototype.RemoveManualMapping = function(id){
			var toDelete = manualDeleteIdx[id];
			
			if(toDelete != undefined){
				if(mappedPluginInputs.length == 0){
					defaultMapping.remove(toDelete);
					displayPaths.remove(toDelete);
					delete manualPathAddType[toDelete];
					//manualPathAddType.remove(toDelete);
				}else{
					userExpressions.remove(toDelete);
					displayPaths.remove(toDelete);
					delete manualPathAddType[toDelete];
					//manualPathAddType.remove(toDelete);
				}
				
				// manual iteration since jQuery has scoping issues when using .each, got to love JS
				for(var tmp in manualDeleteIdx){
					if(!isNaN(tmp) && id != tmp){
						var val = manualDeleteIdx[tmp];
						manualDeleteIdx[tmp] = (val -1);
					}
				}
				
				// .remove just foobars the array ?! -- appears to be non-linar array issue
				delete manualDeleteIdx[id];
			}

		},
		
		/* Used to append the manual output mapping region */
		Dev2InputMapper.prototype.InsertOutputMapping = function(appendNameRegion, path, alias){
			var len = 0 ;
			var payload = "";
			
			if(mappedPluginInputs.length == 0){
				len = defaultMapping.length;
				defaultMapping[len] = alias; //add to default stack
				displayPaths[len] = path;	// add to display paths

				var val = alias;
				var disPath = path;
				if(disPath.length >= 35){
					disPath = disPath.substr(0,34) + "...";
				}
				payload += "<div class='ioMappingInputRow' id='manual" + manualID + "'><div class='ioMappingOutputCol1' title='" + path + "'>" + disPath + "</div>";
				payload += "<div><input type='text' name='Dev2OutputLocation' id='Dev2OutputLocation' class='Dev2OutputLocation ioMappingOutputCol2' value='" + val + "' title='" + val +"' /></div><div class='ioMappingOutputCol3'><img src='/themes/system/images/cross.png' title='Delete Manual Output Entry' height='16' width='16' onClick='deleteEntry(" + manualID + ",false)'/></div></div>";
			}else{
				// we have old data to use
				// push on stacks
				len = userExpressions.length;
				userExpressions[len] = alias;
				displayPaths[len] = path;

				var val = alias;
				//var val = "[[ab]]";
				var disPath = path;
				if(disPath.length >= 35){
					disPath = disPath.substr(0,34) + "...";
				}
				payload += "<div class='ioMappingInputRow' id='manual" + manualID + "'><div class='ioMappingOutputCol1' title='" + path + "'>" + disPath + "</div>";
				payload += "<div><input type='text' name='Dev2OutputLocation' id='Dev2OutputLocation' class='Dev2OutputLocation ioMappingOutputCol2' value='" + val + "' title='" + val +"' /></div><div class='ioMappingOutputCol3'><img src='/themes/system/images/cross.png' title='Delete Manual Output Entry' height='16' width='16' onClick='deleteEntry(" + manualID + ",false)'/></div></div>";
			}
			
			// save index to remove
			manualDeleteIdx[manualID] =  len; //(len-1);
			manualID++;
			
			$("#" + appendNameRegion).append(payload);
		},
		
		/* Used to render the output mapping region */
		Dev2InputMapper.prototype.RenderOutputMapping = function(appendNameRegion){
			// Dev2IOMappingOutputDLMapping
			var payload = "";

			this.GenerateDefaultOutputMapping();
			
			if(mappedPluginInputs.length == 0){
				
				for(var i = 0; i < displayPaths.length; i++){
					var val = defaultMapping[i];
					//var val = "[[ab]]";
					var disPath = displayPaths[i];
					if(disPath.length >= 35){
						disPath = disPath.substr(0,34) + "...";
					}
					payload += "<div class='ioMappingInputRow'><div class='ioMappingOutputCol1' title='" + displayPaths[i] + "'>" + disPath + "</div>";
					payload += "<div><input type='text' name='Dev2OutputLocation' id='Dev2OutputLocation' class='Dev2OutputLocation ioMappingOutputCol2' value='" + val + "' title='" + val +"' /></div></div>";
				}
			}else{
				// we have old data to use
				for(var i = 0; i < displayPaths.length; i++){
					var val = userExpressions[i];
					//var val = "[[ab]]";
					var disPath = displayPaths[i];
					if(disPath.length >= 35){
						disPath = disPath.substr(0,34) + "...";
					}
					payload += "<div class='ioMappingInputRow'><div class='ioMappingOutputCol1' title='" + displayPaths[i] + "'>" + disPath + "</div>";
					payload += "<div><input type='text' name='Dev2OutputLocation' id='Dev2OutputLocation' class='Dev2OutputLocation ioMappingOutputCol2' value='" + val + "' title='" + val +"' /></div></div>";
				}
			}

			$("#" + appendNameRegion).html(payload);

		},
		
		/* Used to generate the default mapping for the service */
		Dev2InputMapper.prototype.GenerateDefaultOutputMapping = function(){

			var fullParts = new Array(); // keep the full parts ;)
		
			for(var i = 0; i < argsLen; i++){
				
				var thisPath = displayPaths[i];
				var val = "[["+thisPath+"]]";
				
				// handle a.b 
				if(thisPath.indexOf(".") != thisPath.lastIndexOf(".")){
					var parts = thisPath.split(".");
					fullParts[i] = parts;
					
					// scalar with mult . take last piece
					if(thisPath.indexOf("(") < 0){
						val = "[[" + parts[(parts.length - 1)] + "]]";
					}else{
						// recordset
						var end = thisPath.indexOf("()."); // rs(). portion
						var myRSName = serviceName+"().";
						
						//val = "[["+thisPath.substr(0, (end+3));
						
						val = "[["+myRSName;
						// now get ending field
						val +=  parts[(parts.length - 1)] + "]]";
					}
					
					// search for part already present?
					for(var q = 0; q < defaultMapping.length; q++){
						if(defaultMapping[q] == val){
							// go to ugly conversion :(
							val = "[[" + parts[0];

							for(var q = 1; q < parts.length; q++){
								val += "_" + parts[q];
							}
							
							parts = val.split("()");
							// handle mult rs aka () parts
							if(parts.length > 1){
								//val = parts[0];
								var end = (parts.length - 1);
								for(var q = 1; q < end; q++){
									val += parts[q];
									if(q < ( end-1)){
										val += "_";
									}
								}
							}

							// repace recset with proper recset notation (). ! ()_
							val = val.replace("()_", "().");
							
							val += "]]";	
						
						}
					}
				
				}
				
				// init the default mapping :)
				defaultMapping[i] = val;
			}
		
		},
		/* Used to Create the output mapping to push back into the service def */	
		Dev2InputMapper.prototype.GenerateMapping = function(){
			var result = origPayload;
			
			//if(saneParse){ 
				// push config back out
				var currentStringIdx = 0;

				// extract type
				var i = 0;
				// extra value added all the time
				while(i < userExpressions.length && saneParse){
				
					// manip the xml string to find the next index to update
					// this is required due to the fact there is no cross-browser capable XML serialize technique
					currentStringIdx = result.indexOf(magicStr, (currentStringIdx + 1));

					if(currentStringIdx == -1 ){
						// we now have user added data, time to chuck in the fragements as required
						
						var startLoc = result.indexOf(termTagForExp);
						if(startLoc > 0){
							
							// we now need to inject the new options
							for(var tmp in manualDeleteIdx){

								if(!isNaN(tmp) && saneParse){
									var idx = manualDeleteIdx[tmp];
									if(idx != undefined){
										var typeOf = this.FetchAnythingToXmlType();
										var frag = this.FetchAnythingToXmlStartFragment(typeOf);
										
										if(frag == ""){
											alert("Invalid format [ " + typeOf + " ]");
											saneParse = false;
										}else{
											// set actual path
											frag +='<ActualPath xmlns="http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph">';
											frag += displayPaths[idx];
											frag += "</ActualPath>";
											
											// set display path
											frag +='<DisplayPath xmlns="http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph">';
											frag += displayPaths[idx];
											frag += "</DisplayPath>";
											
											// set output expression
											frag += '<OutputExpression xmlns="http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph">';
											frag += userExpressions[idx];
											frag += "</OutputExpression>";
											
											// append fake sample data to keep it consistent
											frag += '<SampleData xmlns="http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph">';
											frag += 'Manual Entry, No Fetched Data';
											frag += '</SampleData>';
											
											// now append to the result we are building ;)
											
											var preFix = result.substring(0, startLoc);
											var postFix = result.substr( (startLoc+(termTagForExp.length)) );
											
											result = preFix + frag + postFix;

										}
									}
								}
							}
						}else{
							saneParse = false;
							alert("Parse Error");
						}
					}else{
				
						// we have a scalar to push to RS
						if(userExpressions[i] != "" && saneParse){
							// find correct update location in the data to amend
							var end = result.indexOf(">",currentStringIdx);
							var injectLocation = (currentStringIdx + (end - currentStringIdx));
							var preFix = result.substr(0, (injectLocation+1));
							var postFix = result.substr(injectLocation, (result.length - injectLocation));
							var singleTag = preFix.lastIndexOf("/>");
							
							if(singleTag != -1){
								// we have a single tag amend
								preFix = (preFix.substr(0, (preFix.length - 2)) + ">");
								postFix = "</" + magicStr.replace("<","")  + postFix;
							}else{
								// clear out the current data ;)
								var clearTo = postFix.indexOf("<");
								clearTo += 1;
								postFix = postFix.substr(clearTo, (postFix.length - clearTo));
							}
							result = preFix + userExpressions[i] + postFix;
						}
					
						i++;
					}
				}
			
			if(!saneParse){
				result = origPayload;
			}
			
			return result;
		},
		
		/* Fetch fragment type for manual add entries */
		Dev2InputMapper.prototype.FetchAnythingToXmlStartFragment = function(typeOf){
			var result  = "";
		
			if(typeOf == "xml"){
				result = '<d2p1:anyType i:type="d5p1:XmlPath" xmlns:d5p1="http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.String.Xml">';
			}else if(typeOf == "json"){
				result = '<d2p1:anyType i:type="d5p1:JsonPath" xmlns:d5p1="http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.String.Json">';
			}else if(type == "poco"){
				result = '<d2p1:anyType i:type="d5p1:PocoPath" xmlns:d5p1="http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Poco">';
			}
		
			return result;
		},
		
		// /Unlimited.Framework.Converters.Graph.String.Xml
		/* Fetch fragment type for manual add entries */
		Dev2InputMapper.prototype.FetchAnythingToXmlType = function(typeOf){
			var result = "";
			
			// scan for XML
			if(origPayload.indexOf("Unlimited.Framework.Converters.Graph.String.Xml") > 0){
				result = "xml";
			}else if(origPayload.indexOf("Unlimited.Framework.Converters.Graph.String.Json") > 0){
				result = "json";
			}else if(origPayload.indexOf("Unlimited.Framework.Converters.Graph.Poco") > 0){
				result = "poco";
			}
			
			return result;
		},
		
		/* Convert wizard type to source save type */
		Dev2InputMapper.prototype.WizardWriteTypeConverter = function(){
			var result = typeOf;
			
			if(typeOf == "Database"){
				 // InvokeStoredProc
				 result = "InvokeStoredProc";
			}
			
			return result;
		},
		
		/* Generate the service definition */
		Dev2InputMapper.prototype.GeneratePluginDef = function(){
			var update = "";
			var outputMapping = generateOutputMapping();			
			var inputMapping = generateInputMapping(rawPluginInputs);

			// build user expressions
			var i = 0;
			$(".Dev2OutputLocation").each(function(){
				userExpressions[i] = $(this).val();
				i++;
			});
			
			argsLen = userExpressions.length;
			
			update = buildNewService(serviceName, sourceName, sourceMethod,this.WizardWriteTypeConverter(),this.GenerateMapping(),outputMapping, inputMapping, help, icon, description, tooltip, category, tags);
			
			var status = updateServiceDef(webServer, update);
			
			// REMOVE BEFORE RELEASE
			//alert(status, "Service Definition Save Operation Status");
		},
		
		/* Generate mappings where scalar results are pushed into recordset format */
		Dev2InputMapper.prototype.PushScalarsToRecordset = function(){
			
			var i = 0;
			$(".Dev2OutputLocation").each(function(){
				try{
					if(displayPaths[i].indexOf(")") < 0){
						// scalar to push down
						// Dev2OutputLocation+i
						var hasColon = displayPaths[i].lastIndexOf(":");
						var idx = displayPaths[i].lastIndexOf(".");
						if(hasColon > 0){
							idx = hasColon;
						}
						var displayField = displayPaths[i];

						if(idx > 0){
							displayField = displayField.substr((idx+1));
						}
						
						impactedScalarToRS.push(i); // add to impacted scalars so we correctly roll them back
						
						$(this).val("[["+serviceName+"()."+displayField+"]]");
					}
				}catch(e){/* trap error */}
				i++;
			});			
		},
		
		/* Rollback the Scalar to RS values */
		Dev2InputMapper.prototype.RollbackScalarToRecordset = function(){
			
			var i = 0;
			$(".Dev2OutputLocation").each(function(){
				
				var pos = 0;
				var found =false;
				while(pos < impactedScalarToRS.length && !found){
				
					if(impactedScalarToRS[pos] == i){
						found = true;
					}					
					pos++;
				}
				
				if(found){
					var val = defaultMapping[i];
					$(this).val(val);
				}
				i++;
			});			
			
		},
		
		/* Build the exit payload */
		Dev2InputMapper.prototype.BuildExitPayloadAndExit = function(){
		
			var result = "<Dev2WizardPayload>";
			//ResourceName
			result += "<ResourceName>" + serviceName + "</ResourceName>";
			//ResourceType { WorkflowService, Service { Plugin, Database }, Source, Website, HumanInterfaceProcess }
			result += "<ResourceType>"  + convertToStudioType(typeOf) + "</ResourceType>";
			//Category
			result += "<Category>" + category + "</Category>";
			//Comment
			result += "<Comment>" + description + "</Comment>";
			//Tags
			result += "<Tags>" + tags + "</Tags>";
			//IconPath
			result += "<IconPath>" + icon + "</IconPath>";
			//HelpLink
			result += "<HelpLink>" + help + "</HelpLink>";
			// inject type
			result += "<TypeOf>" + typeOf + "</TypeOf>";
		
			result += "</Dev2WizardPayload>";
			
			ExitWithStudioPush(result);	
		}
	})();

function ExitWithStudioPush(result){
	Dev2Awesomium.Dev2SetValue(result);
	Dev2Awesomium.Dev2Done();
	
	// old IE way
	//window.external.Dev2SetValue(result);
	//window.external.Dev2Done(); // exit the wizard	
}	
	
function ExitWithCancel(){
	Dev2Awesomium.Dev2Cancel();
}	

// tell the studio to relead the tree with source
function SendSourceUpdate(resName){
	Dev2Awesomium.Dev2ReloadResource(resName, "Source");
}
	
// Generate a new service def, if the def ever changes this needs to update as well
function buildNewService(serviceName, sourceName, sourceMethod, typeOf, outputDescription, outputMapping, inputMapping, help, icon, description, tooltip, category, tags ){
	var actionName = serviceName.replace("/ /g", "");
	var result = '<Service Name="'+serviceName+'">';
	result += "<Actions>";
	result += '<Action Name="' + actionName + '" Type="' + typeOf + '" SourceName="' + sourceName + '" SourceMethod="'+sourceMethod+'">';
	result += "<Inputs>";
	result += inputMapping;
	result += "</Inputs>";
	result += "<Outputs>";
	result += outputMapping;
	result += "</Outputs>";
	result += "<OutputDescription><![CDATA[" + outputDescription  + "]]></OutputDescription>";
	result += "</Action>";
	result += "</Actions>";
	result += "<AuthorRoles>Domain Users,All Users,Company Users,Business Design Studio Developers,Build Configuration Engineers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>";
	result += "<Comment>" + description + "</Comment>";
	result += "<Category>" + category +"</Category>";
	result += "<Tags>" + tags +"</Tags>";
	result += "<HelpLink>" + help +"</HelpLink>";
	result += "<UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>";
	result += "<BizRule />";
	result += "<WorkflowActivityDef />";
	result += "<Source />";
    result += "<XamlDefinition />";
    result += "<DisplayName>Service</DisplayName>";
	result +="<DataList />";
	// inject type
	result += "<TypeOf>" + typeOf + "</TypeOf>";
    result +="</Service>";
	
	var roles = "<Roles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,DEV2 Limited Internet Access</Roles>";
	var toReturn = "<Payload>" + roles + "<ResourceXml>" + result +"</ResourceXml></Payload>" ;
	
	return toReturn;
	
}	

// Generate the output mapping
function generateOutputMapping(){
	// Dev2OutputLocation
	var result = "";
	
	$(".Dev2OutputLocation").each(function(){
		// combine each mapping into a 
		// <Output Name="CompanyName" MapsTo="CompanyName" Value="[[Names().CompanyName]]" Recordset="Names" />
		var v = $(this).val();
		var recset = "";
		var mapsTo = "";
		
		if(v.indexOf("(") > 0){
			// recset
			var parts = v.split(".");
			recset = parts[0].replace("()","").replace("[[","");
			mapsTo = parts[1];
		}else{
			mapsTo = v;
		}
		
		mapsTo = mapsTo.replace("]]","").replace("[[","");
		
		if(mapsTo != ""){
			result += '<Output Name="'+mapsTo+'" MapsTo="' + mapsTo + '" Value="' + v + '"';
			if(recset != ""){
				result += ' Recordset="' + recset + '" ';
			}
		
			result += '/>';
		}
	
	});
	
	return result;

}

// Generate the input mapping
function generateInputMapping(rawPluginInputs){

	var inputParts = rawPluginInputs.split(",");
	var result = "";
	
	// DefaultValue="DynamicServiceFramework@theunlimited.co.za"
	
	var i = 0;
	var defVal = new Array();
	var req = new Array();
	
	// gather required
	$(".Dev2IOMappingRequired").each(function(){
		var required = $(this).is(":checked");
		if(required){
			req[i] = '<Validator Type="Required" />';
		}else{
			req[i] = "";
		}
		i++;
	});
	
	// gather default values
	i = 0;
	$(".Dev2IOMappingDefaultValue").each(function(){
		defVal[i] = $(this).val();
		i++;
	});
	
	for(var i = 0; i < inputParts.length; i++){
		if(inputParts[i].length > 0){
			result += '<Input Name="'+inputParts[i]+'" Source="[[' + inputParts[i] + ']]" ';
			// blank input mapping
			//result += '<Input Name="'+inputParts[i]+'" Source="" ';
			if(defVal[i] != ""){
				result += 'DefaultValue="' + defVal[i] + '" '
			}
			result += '>';
			result += req[i];
			result += "</Input>";
		}
	}
	
	return result;
}

// fetch a resource as per the what is on the server
function fetchPluginNameSpace(webServer, sourceName, typeOf){
	var xmlhttp;
	
	if (window.XMLHttpRequest)
	{	// code for IE7+, Firefox, Chrome, Opera, Safari
		xmlhttp=new XMLHttpRequest();
	}
	else
	{	// code for IE6, IE5
		xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
	}
	
	var uri = webServer + "/services/FindResourcesService?ResourceName=" + sourceName + "&ResourceType=" + typeOf + "&Roles=*";
	xmlhttp.open("GET",uri,false);
	xmlhttp.send();
	
	var payload = xmlhttp.responseText;

	return payload;
}

// fetch a resource as per the what is on the server
function fetchMapping(webServer, sourceName, typeOf){
	var xmlhttp;
	
	if (window.XMLHttpRequest)
	{	// code for IE7+, Firefox, Chrome, Opera, Safari
		xmlhttp=new XMLHttpRequest();
	}
	else
	{	// code for IE6, IE5
		xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
	}
	
	var uri = webServer + "/services/FindResourcesService?ResourceName=" + sourceName + "&ResourceType=" + typeOf + "&Roles=*";
	xmlhttp.open("GET",uri,false);
	xmlhttp.send();
	
	var payload = xmlhttp.responseText;

	return payload;
}

function fetchDBMappingPaths(webServer, serverName, databaseName, procedureName, parameters, mode, asyncFn, theUser, thePass){
	var xmlhttp;
		
	if (window.XMLHttpRequest)
	{	// code for IE7+, Firefox, Chrome, Opera, Safari
		xmlhttp=new XMLHttpRequest();
	}
	else{	// code for IE6, IE5
		xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
	}
	
	// no args for now
	var uri = webServer + "/services/CallProcedureService?ServerName="+serverName+"&DatabaseName="+databaseName+"&Procedure="+procedureName+"&Parameters="+parameters+"&Mode="+mode;
	
	if(theUser != ""){
		uri += "&Username="+theUser+"&Password="+thePass
	}
	
	var payload = "";
	if(asyncFn == undefined){
		payload = fetchMappingPathsDB(uri);
	}else{
		payload = fetchMappingPathsDB(uri, asyncFn );
	}

	return payload;
}

// fetch DB mapping path data ;)
function fetchMappingPathsDB(uri, asyncFn){
	var xmlhttp;
	
	if (window.XMLHttpRequest)
	{	// code for IE7+, Firefox, Chrome, Opera, Safari
		xmlhttp=new XMLHttpRequest();
	}
	else{	// code for IE6, IE5
		xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
	}
	
	var payload;
	
	if(asyncFn == undefined){
		xmlhttp.open("GET",uri,false);
		xmlhttp.send();
		
		payload = xmlhttp.responseText;
	}else{
		// async request
		xmlhttp.open("GET",uri,true);
		xmlhttp.onreadystatechange = function(){
		
			if(xmlhttp.readyState == 4){
				try{
					asyncFn(xmlhttp.responseText); // send data through
				}catch(e){}
			}
		}
		xmlhttp.send(null);
		payload = xmlhttp;
	}

	return payload;
}

/*
	This little gem is a fn expression that returns data in an async call or a xmlrequest object in an async call ;)
*/
function fetchMappingPaths(webServer, asmLoc, asmName, method, args, asyncFn){
	var xmlhttp;
	
	if (window.XMLHttpRequest)
	{	// code for IE7+, Firefox, Chrome, Opera, Safari
		xmlhttp=new XMLHttpRequest();
	}
	else{	// code for IE6, IE5
		xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
	}
	
	// no args for now
	var uri = webServer + "/services/InterogatePluginService?AssemblyLocation=" + asmLoc + "&AssemblyName=" + asmName + "&Method=" + method+"&Args="+args;
	
	var payload;
	
	if(asyncFn == undefined){
		xmlhttp.open("GET",uri,false);
		xmlhttp.send();
		
		payload = xmlhttp.responseText;
	}else{
		// async request
		xmlhttp.open("GET",uri,true);
		xmlhttp.onreadystatechange = function(){
		
			if(xmlhttp.readyState == 4){
				try{
					asyncFn(xmlhttp.responseText); // send data through
				}catch(e){}
			}
		}
		xmlhttp.send(null);
		payload = xmlhttp;
	}

	return payload;
}

// '[[Dev2ServiceDetailsHelp]]','[[Dev2ServiceDetailsIcon]]','[[Dev2ServiceDetailsDescription]]','[[Dev2ServiceDetailsTooltipText]]'

var serviceDef = null;
// inputs not set, not is any other pices of data
function bindInputGathering(srcType, sourceName, methodName, webServer, step2Data, step2Inputs, serviceName, help, icon, description, tooltip, category, tags){
	
	$(document).ready(function(){
		
		// no test data, init the hard way
		if(step2Data == ''){
		
			var fetchObj = new Dev2ServiceFetch(webServer, serviceName);
			var mappingData = fetchObj.extractAnythingToXmlConfig();
			if(mappingData.length > 0){
				serviceDef = new Dev2InputMapper(mappingData);
				serviceDef.setDefinedPluginInputs(fetchObj.extractInputs());
			}else{
				alert("The system encountered problems parsing the service definition for " + serviceName);
			}
		}else{
			// we have a payload already, parse it ;)
			serviceDef = new Dev2InputMapper(step2Data);	
			serviceDef.SetRawPluginInputs(step2Inputs);
		}
		
		// set persist info
		serviceDef.SetPersistData(serviceName, sourceName, methodName, webServer,srcType, help, icon, description, tooltip, category, tags);
		// Dev2IOMappingInputNameRegion, Dev2IOMappingInputDefaultValRegion, Dev2IOMappingInputRequiredRegion
		serviceDef.RenderInputsForIOMapping("Dev2IOMappingInputNameRegion");
		// Now render output mapping
		serviceDef.RenderOutputMapping("Dev2IOMappingOutputNameRegion");
		serviceDef.AppendOutputMapping("Dev2IOMappingOutputNameRegion");
		
		if(serviceDef != null){

			/* Bind the done button to create and write the service def */
			$("#Dev2IOMappingDoneBtn").click(function(){
				serviceDef.GeneratePluginDef();
				serviceDef.BuildExitPayloadAndExit();
			});
			
			/* Bind the checkbox Dev2IOMappingForceScalarToRS */
			$('#Dev2IOMappingForceScalarToRS').live('change', function(){
				if($(this).is(':checked')){
					serviceDef.PushScalarsToRecordset();
				}else{
					serviceDef.RollbackScalarToRecordset();
				}
			});
		}
	});
}


function addEntry(id){

	var path = $("#output"+id).val();
	var alias = $("#alias"+id).val();
	
	if(path.length > 0 && alias.length > 0){
	
		if(alias.indexOf("[[") != 0){
			alias = "[[" + alias;
		}
		
		if(alias.indexOf("]]") < 0){
			alias = alias + "]]";
		}
	
		deleteEntry(id);
		serviceDef.InsertOutputMapping("Dev2IOMappingOutputNameRegion",path, alias);
		serviceDef.AppendOutputMapping("Dev2IOMappingOutputNameRegion");
	}
}

function deleteEntry(id, isAddRow){

	if(!isAddRow){
		// delete from mapping data
		serviceDef.RemoveManualMapping(id);
		$("#manual"+id).remove();
	}else{
		$("#manual"+id).remove();
		// add it back so it looks like it cleared
		serviceDef.AppendOutputMapping("Dev2IOMappingOutputNameRegion");
	}
}

function setMode(){
	serviceDef.ToggleMode();
}
