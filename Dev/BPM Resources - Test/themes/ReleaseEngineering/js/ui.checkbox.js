/*
 * jQuery UI Checkbox 0.1
 *
 * Copyright (c) 2009 Jeremy Lea <reg@openpave.org>
 * Dual licensed under the MIT and GPL licenses.
 *
 * http://docs.jquery.com/Licensing
 *
 * Based loosely on plugin by alexander.farkas.
 * http://www.protofunc.com/scripts/jquery/checkbox-radiobutton/
 *
 * Label, id and title handling by Bela Gabor Zakar <zbacsi@gmail.com>
 * 
 * 23/01/2012 - Travis :  Corrected to work with latest jQuery / UI
 *						  for BDS reasons
 *
 */

 // hack
 var selectedArray = new Array();
 
(function($){

// Set up IE for VML if we have not done so already...
if ($.browser.msie) {
	// IE6 background flicker fix
	try	{
		document.execCommand('BackgroundImageCache', false, true);
	} catch (e) {}

	if (!document.namespaces["v"]) {
		$("head").prepend("<xml:namespace ns='urn:schemas-microsoft-com:vml' prefix='v' />");
		$("head").prepend("<?import namespace='v' implementation='#default#VML' ?>");
	}
}

$.widget("ui.checkbox", {
	_init: function() {
		// XXX: UI widget will not actually fail...
		if (!this.element.is(":radio,:checkbox")) {
			return false;
		}
	
		// _radio stores the members of the radio group (if there is one).
		if (this.element.is(":radio")) {
			
			this._radio = $(this.element[0].form).find("input:radio")
				.filter('[name="'+this.element[0].name+'"]');
			this._radioCheckVal = $("input[@"+this.element[0].name+"]:checked").val();
			
			selectedArray[this.element[0].name] = $("input[@"+this.element[0].name+"]:checked").val();
		} else {
			this._radio = false;
		}

		var self = this, o = this.options; // closures for callbacks.
		// Set the ARIA properties on the native input
		this.element
			.attr({
				role: (this._radio ? "radio" : "checkbox"),
				"aria-checked": !!this.element[0].checked
			});
		// Create the main wrapper element (which gives the background box)
		this._wrapper = this.element.wrap($("<span />")).parent()
			.addClass((this._radio ? "ui-radio" : "ui-checkbox") +
				" ui-state-default").attr({'title':$(this.element).attr('title')});
		// Create the icon element
		var iconType = this.element[0].checked;
		
		this._wrapper.prepend($("<span/>")
			.addClass("ui-icon " + this._icon(iconType))
			.click(function(event) {
				// The icon covers the entire box, but is not in a bubbling
				// path, so use it to trigger the native event, and let it
				// take care of the rest.  Gobble up this fake event.
				self.element[0].click();
				event.preventDefault();
				event.stopImmediatePropagation();
				return false;
			}));
		// handle label click
		$('label[for='+this.element.attr('id')+']').click(function(event){				
			self.element[0].click();
			event.preventDefault();
			event.stopImmediatePropagation();
			return false;
		});
		if ($.browser.msie) {
			// IE does not support rounded corners...  We should check
			// something to see if it does.   But anyway, we make another
			// element which is a VML roundrect, and hide the normal wrapper.
			//
			// XXX: Check if we can use this in place of the span.
			// XXX: Implement background images.
			// XXX: Tidy this up to be more jQuery'ish
			//
			// Play tricks to get around arcsize bugs...
			this._wrapper[0].insertAdjacentHTML("afterBegin",
				"<v:roundrect arcsize='" + (this._radio ? "1" : "0.1") +
				"'><v:stroke /><v:fill /></v:roundrect>");
			this._vml = this._wrapper[0].childNodes[0];
			var ss = this._wrapper[0].currentStyle;
			this._vml.style.top = "-1px";
			this._vml.style.left = "-1px";
			this._vml.style.width = parseInt(ss.width)+1+"px";
			this._vml.style.height = parseInt(ss.height)+1+"px";
			this._doVML();
			this._vml.style.visibility = "visible";
			this._wrapper.css('visibility','hidden');
			// Listen for class or other changes to recreate the elements.
			this._wrapper[0].onpropertychange = function() {
				switch (event.propertyName) {
				case 'className':
				case 'style.borderTopWidth':
				case 'style.borderTopColor':
				case 'style.backgroundColor':
				case 'style.filter':
					self._doVML();
					break;
				}
			}
		}
		if ($.browser.opera) {
			// Opera also does not support rounded corners...  Use an SVG
			// element instead.  Same as above, but a little simpler.
			//
			// XXX: Check if we can use this in place of the span.
			// XXX: Implement background images.
			// XXX: Tidy this up to be more jQuery'ish
			var svg = document.createElementNS("http://www.w3.org/2000/svg","svg");
			var rect = document.createElementNS("http://www.w3.org/2000/svg","rect");
			var ss = this._wrapper[0].currentStyle;
			rect.setAttributeNS(null, "x", "1px");
			rect.setAttributeNS(null, "y", "1px");
			rect.setAttributeNS(null, "width", ss.width);
			rect.setAttributeNS(null, "height", ss.height);
			rect.setAttributeNS(null, "rx", (this._radio ? "6px" : "2px"));
			svg.appendChild(rect);
			this._wrapper.prepend(svg);
			this._svg = this._wrapper[0].firstChild;
			this._svg.style.width = parseInt(ss.width)+2+"px";
			this._svg.style.height = parseInt(ss.height)+2+"px";
			this._doSVG();
			this._svg.style.visibility = "visible";
			this._wrapper.css('visibility','hidden');
			// Listen for class changes.
			this._wrapper.bind("DOMAttrModified", function(event) {
				if (event.attrName === 'class') {
					self._doSVG();
				}
			});
		}

		// Set up events...
		this._wrapper
			.hover(function(event) {
				if (!o.disabled) {
					$(this).addClass("ui-state-hover");
				}
			}, function(event) {
				if (!o.disabled) {
					$(this).removeClass("ui-state-hover");
				}
			})
			.bind("mousedown", function(event) {
				if (!o.disabled) {
					$(this).addClass("ui-state-active");
				}
			})
			.bind("mouseup", function(event) {
				if (!o.disabled) {
					$(this).removeClass("ui-state-active");
				}
			})
			.bind(this.widgetEventPrefix + "focus", function(event) {
				if (!o.disabled) {
					if (self._radio) {
						self._radio.not(self.element)
							.removeClass("ui-state-focus");
					}
					$(this).addClass("ui-state-focus");
				}
			})
			.bind(this.widgetEventPrefix + "blur", function(event) {
				if (!o.disabled) {
					$(this).removeClass("ui-state-focus");
				}
			})
			.bind(this.widgetEventPrefix + "click", function(event) {
				if (!o.disabled) {
					// Travis : Amended
					if (self._radio) {
						self.toggle();
					} else {
						self.toggle();
					}
				}
			});
		this.element
			.bind("focus." + this.widgetName, function(event) {
				self._trigger("focus", event); // Actually checkboxfocus
			})
			.bind("blur." + this.widgetName, function(event) {
				self._trigger("blur", event); // Actually checkboxblur
			})
			.bind("click." + this.widgetName, function(event) {
				self._trigger("click", event); // Actually checkboxclick
			});

		// Capture the initial value
		this._setData("checked", !!this.element[0].checked);
	},
	destroy: function() {
		this._wrapper.replaceWith(this.element);
		this.element.removeAttr("role")
			.removeAttr("aria-checked")
			.unbind("."+this.widgetName);
		$.widget.prototype.destroy.apply(this, arguments);
	},

	// Most of the work is done here.
	_setData: function(key, value) {
		//$.widget.prototype._setData.apply(this, arguments);
		//alert(this " " + arguments);
		
		/*if (key == "disabled") {
			if (value) {
				this.element.attr("disabled","disabled");
				this._wrapper.removeClass("ui-state-focus ui-state-hover ui-state-active");
			} else {
				this.element.removeAttr("disabled");
			}
			this._wrapper
				[value ? "addClass" : "removeClass"](
					this.widgetName + "-disabled " +
					this.namespace + "-state-disabled");
		} else if (key == "checked") {
			this.element[0].checked = !!value;
			this.element.attr("aria-checked", !!value);
			this._wrapper.find(".ui-icon")
				.addClass(this._icon(!!value))
				.removeClass(this._icon(!value));
		}*/
	},

	check: function() {
		this._setData("checked", true);
	},
	uncheck: function() {
		this._setData("checked", false);
	},
	toggle: function() {
		var iconType = this.element[0].checked;
		
		if(this._radio){
			var iType = this._icon(true);
			var rType = this._icon(false);
			var setChecked = true;
			
			var selected = selectedArray[this.element[0].name];

			if(this.element[0].value == selected){
				var iType = this._icon(false);
				var rType = this._icon(true);
				setChecked = false;
			}
			
			for(var i = 0; i < this._radio.length; i++){
				
				// this._radioCheckVal
				if(this.element[0].value == this._radio[i].value){
					this._wrapper.find(".ui-icon").removeClass(rType).addClass(iType);
				}else{
					try{
						if(setChecked){
							$(this._radio[i]).parent().find(".ui-icon").removeClass(iType).addClass(rType);
						}
					}catch(e){
						//alert(e);
					}
				}
			}
			
			if(!setChecked){
				selectedArray[this.element[0].name] = "undefined";
			}else{
				selectedArray[this.element[0].name] = this.element[0].value;
			}
		}else{
			var iType = this._icon(iconType);
			var rType = this._icon(!iconType);	
			this._wrapper.find(".ui-icon").addClass(iType).removeClass(rType);
		}
	
		//this._setData("checked", !this._getData("checked"));
	},

	_icon: function(state) {
		var result = "";
		if (this._radio) {
			result = "ui-icon-" + (state?"radio-on":"radio-off");
		} else {
			result = "ui-icon-" + (state?"checkbox-on":"checkbox-off");
		}
		return result;
	},

	_opacityFixed: false,
	_inFixup: false,
	_fixStyle: function(jq, re) {
		var s = jq.attr("style").replace(re,"");
		if (s !== "") {
			jq.attr("style",s);
		} else {
			jq.removeAttr("style");
		}
	},
	// Only called for IE
	_doVML: function() {
		if (!this._vml || this._inFixup) {
			return;
		}
		this._inFixup = true;
		var ss, op;
		if (this._opacityFixed) {
			this._vml.childNodes[0].opacity = '1';
			this._vml.childNodes[1].opacity = '1';
			this._fixStyle(this._wrapper.find(".ui-icon"),/filter[^;]*\;?/i);
			this._fixStyle(this._wrapper,/filter[^;]*\;?/i);
			this._opacityFixed = false;
		}
		ss = this._wrapper[0].currentStyle;
		// IE6 needs both of these...
		this._vml.strokecolor = ss.borderTopColor;
		this._vml.strokeweight = ss.borderTopWidth;
		this._vml.fillcolor = ss.backgroundColor;
		this._vml.childNodes[0].color = ss.borderTopColor;
		this._vml.childNodes[0].weight = ss.borderTopWidth;
		this._vml.childNodes[1].color = ss.backgroundColor;
		if (ss.filter && ss.filter.search(/Alpha/i) !== -1) {
			op = /(\d+)/.exec(ss.filter);
			this._wrapper.find(".ui-icon").css("filter",ss.filter);
			this._vml.childNodes[0].opacity = op[1]/100;
			this._vml.childNodes[1].opacity = op[1]/100;
			this._wrapper.css("filter","");
			this._opacityFixed = true;
		}
		this._inFixup = false;
	},
	// Only called for Opera
	_doSVG: function() {
		if (!this._svg || this._inFixup) {
			return;
		}
		this._inFixup = true;
		var ss, op;
		// Opera doesn't carry over opacity from the hidden container...
		if (this._opacityFixed) {
			this._fixStyle(this._wrapper.find(".ui-icon"),/opacity[^;]*\;?/i);
			this._fixStyle(this._wrapper.find("rect"),/opacity[^;]*\;?/i);
			this._fixStyle(this._wrapper,/opacity[^;]*\;?/i);
			this._opacityFixed = false;
		}
		ss = this._wrapper[0].currentStyle;
		this._svg.firstChild.style.stroke = ss.borderTopColor;
		this._svg.firstChild.style.strokeWidth = ss.borderTopWidth;
		this._svg.firstChild.style.fill = ss.backgroundColor;
		if (ss.opacity && ss.opacity !== 1) {
			op = ss.opacity;
			this._wrapper.find(".ui-icon").css("opacity",op);
			this._wrapper.find("rect").css("opacity",op);
			this._wrapper[0].style.opacity = "1";
			this._opacityFixed = true;
		}
		this._inFixup = false;
	}

});

})(jQuery);
