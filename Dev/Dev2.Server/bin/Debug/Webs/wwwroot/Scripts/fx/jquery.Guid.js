/**
 * jQuery Guid v1.0.0
 * Requires jQuery 1.2.6+ (Not tested with earlier versions).
 * Copyright (c) 2007-2008 Aaron E. [jquery at happinessinmycheeks dot com] 
 * 
 *	Usage:
 *		jQuery.Guid.Value() // Returns value of internal Guid. If no guid has been specified, returns a new one (value is then stored internally).
 *		jQuery.Guid.New() // Returns a new Guid and sets it's value internally.
 *		jQuery.Guid.Empty() // Returns an empty Guid 00000000-0000-0000-0000-000000000000.
 *		jQuery.Guid.IsEmpty() // Returns boolean. True if empty/undefined/blank/null.
 *		jQuery.Guid.IsValid() // Returns boolean. True valid guid, false if not.
 *		jQuery.Guid.Set() // Retrns Guid. Sets Guid to user specified Guid, if invalid, returns an empty guid.
 *
 */

jQuery.extend({
	Guid: {
		Set: function(val) {
			var value;
			if (arguments.length == 1) {
				if (this.IsValid(arguments[0])) {
					value = arguments[0];
				} else {
					value = this.Empty();
				}
			}
			$(this).data("value", value);
			return value;
		},

		Empty: function() {
			return "00000000-0000-0000-0000-000000000000";
		},

		IsEmpty: function(gid) {
			return gid == this.Empty() || typeof (gid) == 'undefined' || gid == null || gid == '';
		},

		IsValid: function(value) {
			rGx = new RegExp("\\b(?:[a-fA-F0-9]{8})(?:-[a-fA-F0-9]{4}){3}-(?:[a-fA-F0-9]{12})\\b");
			return rGx.exec(value) != null;
		},

		New: function() {
			if (arguments.length == 1 && this.IsValid(arguments[0])) {
				$(this).data("value", arguments[0]);
				value = arguments[0];
			}

			var res = [], hv;
			var rgx = new RegExp("[2345]");
			for (var i = 0; i < 8; i++) {
				hv = (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
				if (rgx.exec(i.toString()) != null) {
					if (i == 3) { hv = "6" + hv.substr(1, 3); }
					res.push("-");
				}
				res.push(hv.toUpperCase());
			}
			value = res.join('');
			$(this).data("value", value);
			return value;
		},

		Value: function() {
			if ($(this).data("value")) {
				return $(this).data("value");
			}
			var val = this.New();
			$(this).data("value", val);
			return val;
		}
	}
})();
