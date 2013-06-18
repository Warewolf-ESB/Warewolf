var recordsets = namespace('Warewolf.Recordsets');

recordsets.pushResult = function (recordsetList, result) {
    recordsetList.removeAll();

    if (!result || result.length == 0) {
        return;
    }

    result.forEach(function (rs) {
        var isScalarRecordset = rs.Name ? false : true;
        
        rs.Alias = ko.observable(isScalarRecordset || rs.Fields.length == 0 ? "" : rs.Fields[0].RecordsetAlias + "()");
        rs.Records = ko.observableArray(rs.Records);
        rs.HasErrors = ko.observable(rs.HasErrors);
        rs.ErrorMessage = ko.observable(rs.ErrorMessage);
        rs.HasName = ko.computed(function () {
            var hasName = rs.Name != null && rs.Name != "";
            return hasName;
        });
        if (!isScalarRecordset) {
            rs.Alias.subscribe(function (newValue) {
                $.each(rs.Fields, function (index, field) {
                    field.RecordsetAlias(newValue);
                });
            });
        }
        
        $.each(rs.Fields, function (index, field) {
            field.FieldID = ko.observable($.Guid.New());
            field.$FieldElem = null;
            field.Alias = ko.observable(field.Alias);
            field.RecordsetAlias = ko.observable(field.RecordsetAlias ? field.RecordsetAlias + "()" : "");
            field.FormattedAlias = ko.computed({
                read: function () {
                    return recordsets.createFormattedAlias(isScalarRecordset ? this.RecordsetAlias() : rs.Alias(), this.Alias(), isScalarRecordset);
                },
                write: function (value) {
                    if (!value) {
                        if (isScalarRecordset) {
                            this.RecordsetAlias("");
                        }
                        this.Alias("");
                    } else {
                        var values = recordsets.splitFormattedAlias(value, isScalarRecordset ? "" :  rs.Name + "()");
                        var newFormattedValue = recordsets.createFormattedAlias(values[0], values[1], isScalarRecordset);

                        // It's not enough to just check the recordset and field alias values. 
                        // We need to check the whole string too, as the user may have fiddled
                        // with the recordset marker: ().
                        if (newFormattedValue == value && this.RecordsetAlias() == values[0] && this.Alias() == values[1]) {
                            // nothing changed
                            return;
                        }
                        if (!field.$FieldElem) {
                            field.$FieldElem = $("#" + field.FieldID());
                        }

                        // get input caret
                        var caretPos = field.$FieldElem.caret();

                        if (isScalarRecordset) {
                            this.RecordsetAlias(values[0]);
                        } else {
                            if (value == values[1]) {
                                // entered a field name into an empty input
                                // which will auto-prepend the rs alias
                                // so move the caret to the end of the input
                                caretPos = newFormattedValue.length;
                            }
                            this.RecordsetAlias(values[0]);
                            rs.Alias(values[0]);
                            rs.Alias.valueHasMutated();
                        }
                        this.Alias(values[1]);

                        // notify subscribers that they should re-evaluate
                        this.RecordsetAlias.valueHasMutated();

                        // set input caret
                        field.$FieldElem.caret(caretPos);
                    }

                    // Clear recordset alias if it's not being used
                    if (!isScalarRecordset) {
                        var aliasesWithRecordset = $.grep(rs.Fields, function (f) { return f.FormattedAlias().indexOf("()") !== -1; });
                        if (aliasesWithRecordset.length == 0) {
                            rs.Alias("");
                        }
                    }
                },
                owner: field
            });
        });

        recordsetList.push(rs);
    });
};

recordsets.splitFormattedAlias = function (value, defaultRecordsetAlias) {
    var result = ["", ""];  // recordset, field

    var idx = value.indexOf("(");
    if (idx == -1) {
        idx = value.indexOf(")");
        if (idx == -1) {
            idx = value.indexOf(".");
        }
    }

    if (idx == -1) {
        result[1] = value;
    } else {
        result[0] = value.substring(0, idx);
        result[1] = value.substring(idx + 1);
    }

    for (var i = 0; i < result.length; i++) {
        result[i] = result[i].replace(/\(/g, "").replace(/\)/g, "").replace(/\./g, "");
    }

    if (result[0].length > 0) {
        result[0] += "()";
    } else {
        result[0] = defaultRecordsetAlias;
    }
    return result;
};

recordsets.createFormattedAlias = function (rsAlias, fieldAlias, isScalarRecordset) {
    if (isScalarRecordset) {
        if (rsAlias) {
            return rsAlias + "." + fieldAlias;
        }
        return fieldAlias;
    }
    if (rsAlias && fieldAlias) {
        return rsAlias + "." + fieldAlias;
    }
    return fieldAlias;
};
