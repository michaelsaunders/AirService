(function ($) {
    $.fn.cascade = function (options) {
        var defaults = {};
        var opts = $.extend(defaults, options);

        return this.each(function () {
            $(this).change(function () {
                var selectedValue = $(this).val();
                if (selectedValue != null && selectedValue != '') {
                    var params = {};
                    params[opts.paramName] = selectedValue;
                    $.getJSON(opts.url, params, function (items) {
                        // default select
                        opts.childSelect.empty();
                        opts.childSelect.append($('<option/>')
                                .attr('value', '')
                                .text(' --- Select --- '));
                        $.each(items, function (index, item) {
                            opts.childSelect.append(
                            $('<option/>')
                                .attr('value', item.Id)
                                .text(item.Name)
                        );
                        });
                    });
                }
                else {
                    // default select
                    opts.childSelect.empty();
                    opts.childSelect.append($('<option/>')
                                .attr('value', '')
                                .text(' --- Select --- '));
                }
            });
        });
    };
})(jQuery);

if (typeof (ko) != 'undefined' && typeof (ko.utils) != 'undefined') {
    ko.bindingHandlers['class'] = {
        'update': function(element, valueAccessor) {
            if (element['__ko__previousClassValue__']) {
                $(element).removeClass(element['__ko__previousClassValue__']);
            }
            var value = ko.utils.unwrapObservable(valueAccessor());
            if (typeof(value) == 'function') {
                value = value();
            }

            $(element).addClass(value);
            element['__ko__previousClassValue__'] = value;
        }
    };

    // modified version of ko.utils.sendJson 

    ko.utils.post = function(urlOrForm, data, options) {
        options = options || { };
        var params = options['params'] || { };
        var includeFields = options['includeFields'] || this.fieldsIncludedWithJsonPost;
        var url = urlOrForm;

        // If we were given a form, use its 'action' URL and pick out any requested field values 	
        if ((typeof urlOrForm == 'object') && (urlOrForm.tagName == "FORM")) {
            var originalForm = urlOrForm;
            url = originalForm.action;
            for (var i = includeFields.length - 1; i >= 0; i--) {
                var fields = ko.utils.getFormFields(originalForm, includeFields[i]);
                for (var j = fields.length - 1; j >= 0; j--)
                    params[fields[j].name] = fields[j].value;
            }
        }

        data = ko.utils.unwrapObservable(data);
        var form = document.createElement("FORM");
        form.style.display = "none";
        form.action = url;
        form.method = options.method ? options.method : "post";
        for (var key in data) {
            var input = document.createElement("INPUT");
            input.name = key;
            input.value = options.json ? ko.utils.stringifyJson(ko.utils.unwrapObservable(data[key])) : data[key];
            form.appendChild(input);
        }
        for (var key in params) {
            var input = document.createElement("INPUT");
            input.name = key;
            input.value = params[key];
            form.appendChild(input);
        }
        document.body.appendChild(form);
        options['submitter'] ? options['submitter'](form) : form.submit();
        setTimeout(function() { form.parentNode.removeChild(form); }, 0);
    };

    ko.extenders.number = function (target, options) {
        //create a writeable computed observable to intercept writes to our observable
        var result = ko.computed({
            read: target,
            write: function (newValue) {
                var current = target();
                var valueToWrite = isNaN(newValue) ? 0 : parseFloat(newValue);
                if ((options.min == 0 || options.min) && valueToWrite < options.min) {
                    valueToWrite = options.min;
                } else if ((options.max == 0 || options.max) && valueToWrite > options.max) {
                    valueToWrite = options.max;
                }

                //only write if it changed
                if (valueToWrite !== current) {
                    target(valueToWrite);
                } else {
                    //if the rounded value is the same, but a different value was written, force a notification for the current field
                    if (newValue != current) {
                        target.notifySubscribers(valueToWrite);
                    }
                }
            }
        });
        //initialize with current value to make sure it is rounded appropriately
        result(target());

        //return the new computed observable
        return result;
    };
}

$.ajaxSetup({ cache: false });

$(document).ready(function () {
    $(".openDialog").live("click", function (e) {
        e.preventDefault();

        var $dialog = $("<div><div class='wait' style='width:100px;margin-left:auto;margin-right:auto;height: 22px; padding-left: 2px; padding-top: 2px;'/></div>")
            .addClass("dialog")
            .attr("id", $(this).attr("data-dialog-id"))
            .appendTo("body");

        $('.wait', $dialog).progressbar({ value: 100 });
        $dialog.dialog({
            title: $(this).attr("data-dialog-title"),
            close: function () { $(this).remove(); },
            modal: true,
            closeOnEscape: true,
            height: 'auto',
            width: 500,
            position: ['center', 'top']
        }).load(this.href, null, function () {
        });

        return false;
    });

    $(".close").live("click", function (e) {
        e.preventDefault();
        $(this).closest(".dialog").dialog("close");
    });

    //$("body").prepend('<div id="csl" style="position:fixed; z-index:9999; left:0; top:20px; color:#C00; font-size:16pt;"></div>');
    $(window).bind("scroll", function () {
        //$("#csl").html("LOADED");
        var st = $(this).scrollTop();
        var h = Math.max($(document).height(), $(window).height(), document.documentElement.clientHeight);

        if ((st + 813 + 125) >= (h - 173)) {
            //$("#csl").html("OUT OF BOUNDS");
            $("#mobilePreview").css({ position: "absolute", top: -82 });
            $("#saveTheme").css({ position: "absolute", top: 513 });
        }
        else {
            //$("#csl").html("HIT AREA");
            $("#mobilePreview, #saveTheme").attr("style", "");
        }
    });

    if ($.browser.msie) {
        $('input[type=text][placeholder]').each(function (index, item) {
            $(item).val($(item).attr('placeholder'));
        });

        $('input[type=text][placeholder]').focusin(function () {
            var placeholder = $(this).attr('placeholder');
            if ($(this).val() == $.trim(placeholder)) {
                $(this).val('');
            }
        });

        $('input[type=text][placeholder]').focusout(function () {
            if ($.trim($(this).val()) == '') {
                $(this).val($(this).attr('placeholder'));
            }
        });

        $('input[type=text][placeholder][data-val-required]').closest('form').submit(function () {
            var hasError = false;
            $.each($('input[placeholder][data-val-required]', $(this)), function (index, item) {
                var value = $.trim($(item).val());
                var placeholder = $(item).attr('placeholder');
                if (value == placeholder || value == '') {
                    $(item).addClass('input-validation-error');
                    hasError = true;
                    return false;
                }

                return true;
            });

            return !hasError;
        });
    }
});

function dialogErrorMessage(dialogSelector, xhr) {
    var json = $.parseJSON(xhr.responseText);
    $(dialogSelector + " .field-validation-error").empty();
    if (json.HasErrors) {
        for (var i = json.ErrorMessages.length - 1; i > -1; i--) {
            var error = "<span class='field-validation-error' generated='true' data-valmsg-replace='true' data-valmsg-for='" + json.ErrorKeys[i] + "'>" + json.ErrorMessages[i] + "</span>";
            $(dialogSelector + " form").prepend(error);
        }
    } else if (json.HasError) {
        var singleError = "<span class='field-validation-error' generated='true' data-valmsg-replace='true'>" + json.ErrorMessage + "</span>";
        $(dialogSelector + " form").prepend(singleError);
    }
    else {
        var unknownError = "<span class='field-validation-error' generated='true' data-valmsg-replace='true'>" +
            "Unexpected error occurred. Please refresh your browser and try again.</span>";
        $(dialogSelector + " form").prepend(unknownError);
    }
}

function promptErrorMessage(xhr) {
    var json = $.parseJSON(xhr.responseText);
    var errorMessage = '';
    if(json.HasErrors) {
        for (var i = json.ErrorMessages.length - 1; i > -1; i--) {
            errorMessage += json.ErrorMessages[i] + "\n";
        }
    }
    else if (json.HasError) {
        errorMessage = json.ErrorMessage;
    }
    else {
        errorMessage = 'Unexpected error. Please try again';
    }

    alert(errorMessage);
}

//http://stackoverflow.com/questions/891696/jquery-what-is-the-best-way-to-restrict-number-only-input-for-textboxes-all
(function($) {
    $.fn.textInput = function(options) {
        if (typeof(options) == 'object') {
            if (options.expression) {
                this.live('keypress', function(/*event*/) {
                    this.value = this.value.replace( /[^0-9\.]/g , '');
                });
            }
        } else if (options == 'integer' || options == 'float') {
            this.data('data-options', options);
            this.live('keypress', function(event) {
                var integer = $(this).data('data-options') == 'integer';
                // Backspace, tab, enter, end, home, left, right
                // We don't support the del key in Opera because del == . == 46.
                var controlKeys = [8, 9, 13, 35, 36, 37, 39];
                // IE doesn't support indexOf
                var isControlKey = controlKeys.join(",").match(new RegExp(event.which));
                // Some browsers just don't raise events for control keys. Easy.
                // e.g. Safari backspace.
                if ((!integer && event.which == 46) || !event.which || // Control keys in most browsers. e.g. Firefox tab is 0
                    (49 <= event.which && event.which <= 57) || // Always 1 through 9
                        (48 == event.which && $(this).attr("value")) || // No 0 first digit
                            isControlKey) { // Opera assigns values for control keys.
                    return;
                } else {
                    event.preventDefault();
                }
            });
        }
    };
})(jQuery);

//http://stackoverflow.com/questions/149055/how-can-i-format-numbers-as-money-in-javascript
Number.prototype.formatMoney = function (c, d, t) {
    var n = this, c = isNaN(c = Math.abs(c)) ? 2 : c, d = d == undefined ? "." : d, t = t == undefined ? "," : t, s = n < 0 ? "-" : "", i = parseInt(n = Math.abs(+n || 0).toFixed(c)) + "", j = (j = i.length) > 3 ? j % 3 : 0;
    return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
};

Array.max = function (array) {
    return Math.max.apply(Math, array);
};

Array.min = function (array) {
    return Math.min.apply(Math, array);
};