function test(){
	alert('hello');
}



var boundFieldId;

function bindAutoComplete(sourceFieldId, searchTermTag, AutoCompleteTextProperty, RelatedFieldName, rowDelimiter, boundServiceName) {

		if(sourceFieldId!=undefined){
			boundFieldId = sourceFieldId
		}

    $("#" + boundFieldId).autocomplete({
        source: function (req, add) {
            xmldoc = $.parseXML('<configData/>');
            newelem = xmldoc.createElement(searchTermTag);
            newText = xmldoc.createTextNode(req.term);
            newelem.appendChild(newText);
            xmldoc.firstChild.appendChild(newelem);

            if (AutoCompleteTextProperty != '') {
                newelem = xmldoc.createElement('Dev2Field');
                newText = xmldoc.createTextNode(AutoCompleteTextProperty);
                newelem.appendChild(newText);
                xmldoc.firstChild.appendChild(newelem);
            }
			
            if (rowDelimiter != '') {
                newelem = xmldoc.createElement('Dev2RowDelimiter');
                newText = xmldoc.createTextNode(rowDelimiter);
                newelem.appendChild(newText);
                xmldoc.firstChild.appendChild(newelem);
            }			

            if (boundServiceName != '') {
                newelem = xmldoc.createElement('Dev2BoundServiceName');
                newText = xmldoc.createTextNode(boundServiceName);
                newelem.appendChild(newText);
                xmldoc.firstChild.appendChild(newelem);
            }

            if (RelatedFieldName != '') {
                relatedfieldVal = $('#' + RelatedFieldName).val();

                if (relatedfieldVal != '') {
                    newelem = xmldoc.createElement(RelatedFieldName);
                    newText = xmldoc.createTextNode(relatedfieldVal);
                    newelem.appendChild(newText);
                    xmldoc.firstChild.appendChild(newelem);
                }
            }

            postData = xmldoc.xml ? xmldoc.xml : (new XMLSerializer()).serializeToString(xmldoc);



            $.ajax({
                type: 'POST',
                url: "/services/JSON Binder?callback=?",
                dataType: "json",
                data: { InputData: postData },
                success: function (data) {
                    var suggestions = [];
                    $.each(data, function (test, obj) {
                        try {
                            var item = data[test];
                            if (item != null) {
                                for (var key in item) {
                                    if (key == AutoCompleteTextProperty) {
                                        suggestions.push(item[key]);
                                    }
                                }
                            }
                        }
                        catch (ex) { }

                    });
                    add(suggestions);
                }
            });
        }
    });





}


	function bindAutoComplete234(sourceFieldId, searchTermTag, AutoCompleteTextProperty, RelatedFieldName, configData, boundServiceName) {
		if(sourceFieldId!=undefined){
			boundFieldId = sourceFieldId
		}
            $("#" + boundFieldId).autocomplete({
                source: function (req, add) {
                    xmldoc = $.parseXML(configData);
                    newelem = xmldoc.createElement(searchTermTag);
                    newText = xmldoc.createTextNode(req.term);
                    newelem.appendChild(newText);
                    xmldoc.firstChild.appendChild(newelem);

                    if (boundServiceName != '') {
                        newelem = xmldoc.createElement('Dev2BoundServiceName');
                        newText = xmldoc.createTextNode(boundServiceName);
                        newelem.appendChild(newText);
                        xmldoc.firstChild.appendChild(newelem);
                    }

                    if (RelatedFieldName != '') {
                        relatedfieldVal = $('#' + RelatedFieldName).val();

                        if (relatedfieldVal != '') {
                            newelem = xmldoc.createElement(RelatedFieldName);
                            newText = xmldoc.createTextNode(relatedfieldVal);
                            newelem.appendChild(newText);
                            xmldoc.firstChild.appendChild(newelem);
                        }
                    }

                    postData = xmldoc.xml ? xmldoc.xml : (new XMLSerializer()).serializeToString(xmldoc);



                    $.ajax({
                        type: 'POST',
                        url: "http://localhost:1234/services/JSON Binder?callback=?",
                        dataType: "json",
                        data: { InputData: postData },
                        success: function (data) {
                            var suggestions = [];
                            $.each(data, function (test, obj) {
                                try {
                                    var item = data[test];
                                    if (item != null) {
                                        for (var key in item) {
                                            if (key == AutoCompleteTextProperty) {
                                                suggestions.push(item[key]);
                                            }
                                        }
                                    }
                                }
                                catch (ex) { }

                            });
                            add(suggestions);
                        }
                    });
                }
            });

        
            


        }