function namespace(namespaceString) {
    var parts = namespaceString.split('.'),
        parent = window,
        currentPart = '';

    for (var i = 0, length = parts.length; i < length; i++) {
        currentPart = parts[i];
        parent[currentPart] = parent[currentPart] || {};
        parent = parent[currentPart];
    }

    return parent;
}

function getParameterByName(name, search) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(search ? search : window.location.search);
    if (results == null) {
        return "";
    }
    else {
        return decodeURIComponent(results[1].replace(/\+/g, " "));
    }
}

function ShowValidationError(formID, errors, message) {
    var errDiv = "#" + formID + " div.error";
    var $errDiv = $(errDiv);

    $errDiv.hide();
    if (errors) {
        if (!message) {
            message = errors == 1
                ? 'You missed 1 field. It has been highlighted below'
                : 'You missed ' + errors + ' fields. They have been highlighted below';
        }
        $(errDiv + " span").html(message);
        $errDiv.show();
    }
}

function formValidator(form, validator) {
    var errors = validator.numberOfInvalids();
    ShowValidationError(form.target.id, errors);
}

if (typeof String.prototype.endsWith !== 'function') {
    String.prototype.endsWith = function (suffix) {
        return this.indexOf(suffix, this.length - suffix.length) !== -1;
    };
}