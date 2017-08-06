(function ($) {
    "use strict";
    $.fn.multifilter = function (options) {
        var settings = $.extend({
            'target': $('#executionList'),
            'method': 'thead' // This can be thead or class
        }, options);

        jQuery.expr[":"].Contains = function (a, i, m) {
            return (a.textContent || a.innerText || "").toUpperCase().indexOf(m[3].toUpperCase()) >= 0;
        };

        this.each(function () {
            var $this = $(this);
            var container = settings.target;
            var row_tag = 'tr';
            var item_tag = 'td';
            var rows = container.find($(row_tag));

            if (settings.method === 'thead') {
                // Match the data-col attribute to the text in the thead
                var col = container.find('th:Contains(' + $this.data('col') + ')');
                var col_index = container.find($('thead th')).index(col);
            };

            if (settings.method === 'class') {
                // Match the data-col attribute to the class on each column
                var col = rows.first().find('td.' + $this.data('col') + '');
                var col_index = rows.first().find('td').index(col);
            };

            $this.change(function () {
                var filter = $this.val();
                rows.each(function () {
                    var row = $(this);
                    var cell = $(row.children(item_tag)[col_index]);
                    if (filter)
                    {
                        var valueToFilterStr = cell.text().toLowerCase();
                        if (col_index == 2) {
                            if (valueToFilterStr.indexOf(filter.toLowerCase()) !== -1) {
                                cell.attr('data-filtered', 'positive');
                            } else {
                                cell.attr('data-filtered', 'negative');
                            }
                            if (row.find(item_tag + "[data-filtered=negative]").length > 0) {
                                row.hide();
                            } else {
                                if (row.find(item_tag + "[data-filtered=positive]").length > 0) {
                                    row.show();
                                }
                            }
                        }
                        else if (col_index == 4 || col_index == 5) {
                            console.log(cell);
                            var dateTimeValue = Date.parse(valueToFilterStr);
                            var filterDateTimeValue = Date.parse(filter);
                            if (dateTimeValue & filterDateTimeValue) {
                                if (col_index == 4) {
                                    if (dateTimeValue >= filterDateTimeValue) {
                                        cell.attr('data-filtered', 'positive');
                                    }
                                    else {
                                        cell.attr('data-filtered', 'negative');
                                    }
                                }
                                else if (col_index == 5) {
                                    if (dateTimeValue <= filterDateTimeValue) {
                                        cell.attr('data-filtered', 'positive');
                                    }
                                    else {
                                        cell.attr('data-filtered', 'negative');
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (valueToFilterStr.indexOf(filter.toLowerCase()) !== -1)
                            {
                                cell.attr('data-filtered', 'positive');
                            }
                            else
                            {
                                cell.attr('data-filtered', 'negative');
                            }
                            if (row.find(item_tag + "[data-filtered=negative]").length > 0)
                            {
                                row.hide();
                            }
                            else
                            {
                                if (row.find(item_tag + "[data-filtered=positive]").length > 0) {
                                    row.show();
                                }
                            }
                        }
                    }
                    else
                    {
                        cell.attr('data-filtered', 'positive');
                        if (row.find(item_tag + "[data-filtered=negative]").length > 0)
                        {
                            row.hide();
                        }
                        else
                        {
                            if (row.find(item_tag + "[data-filtered=positive]").length > 0)
                            {
                                row.show();
                            }
                        }
                    }
                });
                return false;
            }).keyup(function () {
                $this.change();
            });
        });
    };
})(jQuery);
