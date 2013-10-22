var recordsets = namespace('Warewolf.Recordsets');

function RecordsetField(field, rs) {
    var self = this;

    self.Name = field.Name;
    self.Path = field.Path;
    self.Alias = ko.observable(field.Alias);
    self.RecordsetAlias = ko.observable(field.RecordsetAlias ? field.RecordsetAlias + "()" : "");
    self.FieldID = ko.observable($.Guid.New());
    
    self.Recordset = rs;
    self.FormattedAlias = ko.computed({
        owner: self,
        read: function () {
            return recordsets.createFormattedAlias(self.Recordset.IsScalar ? self.RecordsetAlias() : self.Recordset.Alias(), self.Alias(), self.Recordset.IsScalar);
        },
        write: function (value) {
            if (!value) {
                if (self.Recordset.IsScalar) {
                    self.RecordsetAlias("");
                }
                self.Alias("");
            } else {
                var values = recordsets.splitFormattedAlias(value, self.Recordset.IsScalar ? "" : self.Recordset.Name + "()");
                var newFormattedValue = recordsets.createFormattedAlias(values[0], values[1], self.Recordset.IsScalar);

                // It's not enough to just check the recordset and field alias values. 
                // We need to check the whole string too, as the user may have fiddled
                // with the recordset marker: ().
                if (newFormattedValue == value && self.RecordsetAlias() == values[0] && self.Alias() == values[1]) {
                    // nothing changed
                    return;
                }
                
                // DO NOT store fieldElem on the field as it will cause ko.toJSON() to fail!
                var fieldElem = $("#" + self.FieldID());

                // get input caret
                var caretPos = fieldElem.caret();

                if (self.Recordset.IsScalar) {
                    self.RecordsetAlias(values[0]);
                } else {
                    if (value == values[1]) {
                        // entered a field name into an empty input
                        // which will auto-prepend the self.Recordset alias
                        // so move the caret to the end of the input
                        caretPos = newFormattedValue.length;
                    }
                    self.RecordsetAlias(values[0]);
                    self.Recordset.Alias(values[0]);
                    self.Recordset.Alias.valueHasMutated();
                }
                self.Alias(values[1]);

                // notify subscribers that they should re-evaluate
                self.RecordsetAlias.valueHasMutated();

                // set input caret
                fieldElem.caret(caretPos);
            }

            // Clear recordset alias if it's not being used
            if (!self.Recordset.IsScalar) {
                var aliasesWithRecordset = $.grep(self.Recordset.Fields, function (f) { return f.FormattedAlias().indexOf("()") !== -1; });
                if (aliasesWithRecordset.length == 0) {
                    self.Recordset.Alias("");
                }
            }
        }
    });    
}

RecordsetField.prototype.toJSON = function () {
    
    // easy way to get a clean copy
    var copy = ko.toJS(this);   
    
    // remove extra properties
    delete copy.Recordset;
    delete copy.FormattedAlias;
    delete copy.FieldID;        

    //return the copy to be serialized
    return copy;                
};

function Recordset(rs) {
    var self = this;
    self.IsScalar = rs.Name ? false : true;
    self.Name = rs.Name;
    self.Alias = ko.observable(self.IsScalar || rs.Fields.length == 0 ? "" : rs.Fields[0].RecordsetAlias + "()");
    self.Records = ko.observableArray(rs.Records);
    self.HasErrors = ko.observable(rs.HasErrors);
    self.ErrorMessage = ko.observable(rs.ErrorMessage);
    self.Fields = [];
    self.HasName = ko.computed(function () {
        return !self.IsScalar;
    });
    if (!self.IsScalar) {
        self.Alias.subscribe(function (newValue) {
            $.each(self.Fields, function (index, field) {
                field.RecordsetAlias(newValue);
            });
        });
    }   
    $.each(rs.Fields, function (fieldIndex, field) {
        self.Fields.push(new RecordsetField(field, self));
    });
}

Recordset.prototype.toJSON = function () {

    // easy way to get a clean copy
    var copy = ko.toJS(this);

    // remove extra properties
    delete copy.HasName;
    delete copy.Records;
    
    //return the copy to be serialized
    return copy;
};


recordsets.pushResult = function (recordsetList, result) {
    recordsetList.removeAll();

    if (!result || result.length == 0) {
        return;
    }

    $.each(result, function (index, rs) {
        var rsObj = new Recordset(rs);
        recordsetList.push(rsObj);
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
