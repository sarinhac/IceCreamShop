
var Select2Cascade = (function (window, $) {

    function Select2Cascade(parent, child, url, select2Options) {
        var afterActions = [];
        var options = select2Options || {};

        this.then = function (callback) {
            afterActions.push(callback);
            return this;
        };

        parent.select2(select2Options).on("change", function (e) {

            //child.prop("disabled", true);
            child.val(null).trigger("change").empty();

            var _this = this;

            var message = "-- Select an option --";

            var field = child[0].id;

            $.getJSON(url.replace('1', $(this).val()), function (items) {
                var newOptions = '<option value="">' + message + '</option>';

                items.forEach(function (entry) {
                    newOptions += '<option value="' + entry.id + '">' + entry.name + '</option>';
                });

                child.select2('destroy').html(newOptions).prop("disabled", false)
                    .select2(options);

                afterActions.forEach(function (callback) {
                    callback(parent, child, items);
                });
            });
        });
    }

    return Select2Cascade;

})(window, $);


$(document).ready(function () {

    var select2Options = {
        language: {
            noResults: function () {
                return "No results found";
            }
        }
    };

    var apiUrl = "/Products/GetCategory/1";

    $('.select2').select2(select2Options);
    var cascadLoading = new Select2Cascade($('#CompanyId'), $('#CategoryId'), apiUrl, select2Options);
});


var Select2Cascade = (function (window, $) {

    function Select2Cascade(parent, child, url, select2Options) {
        var afterActions = [];
        var options = select2Options || {};

        this.then = function (callback) {
            afterActions.push(callback);
            return this;
        };

        parent.select2(select2Options).on("change", function (e) {

            //child.prop("disabled", true);
            child.val(null).trigger("change").empty();

            var _this = this;

            var message = "-- Select an option --";

            var field = child[0].id;

            $.getJSON(url.replace('1', $(this).val()), function (items) {
                var newOptions = '<option value="">' + message + '</option>';

                items.forEach(function (entry) {
                    newOptions += '<option value="' + entry.id + '">' + entry.name + '</option>';
                });

                child.select2('destroy').html(newOptions).prop("disabled", false)
                    .select2(options);

                afterActions.forEach(function (callback) {
                    callback(parent, child, items);
                });
            });
        });
    }

    return Select2Cascade;

})(window, $);


$(document).ready(function () {

    var select2Options = {
        language: {
            noResults: function () {
                return "No results found";
            }
        }
    };

    var apiUrl = "/Products/GetUnitMeasure/1";

    $('.select2').select2(select2Options);
    var cascadLoading = new Select2Cascade($('#CompanyId'), $('#UnitMeasureId'), apiUrl, select2Options);
});