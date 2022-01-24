function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

function addCommas(nStr) {
    nStr += '';
    var x = nStr.split('.');
    var x1 = x[0];
    var x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}

function initSelectize(element, url) {
    var users = element.selectize({
        valueField: 'id',
        labelField: 'name',
        searchField: 'search',
        plugins: ['remove_button'],
        options: [],
        create: false,
        render: {
            option: function(item, escape) {
                return "<div style='padding:5px;'>"+escape(item.name)+"</div>";
            }
        },
        load: function(query, callback) {
            if (!query.length) return callback();
            
            $.ajax({
                url: url,
                type: 'GET',
                dataType: 'jsonp',
                jsonp: 'callback',
                jsonpCallback: 'callback',
                data: {
                    query: query,
                    limit: 10
                },
                error: function() {
                    callback();
                },
                success: function(res) {
                    callback(res);
                }
            });
        }
    });

    return users;
}

function initCharts() {
    sleep(1000);
    google.charts.load('current', {'packages':['corechart']});
    google.setOnLoadCallback(drawChart);
}

var globalVariable = {
    genderPieChart: ''
};

function drawGraphChart(url, title, divChart, data) {

    $.ajax({
        headers: {
            'X-CSRF-TOKEN': $('meta[name="csrf-token"]').attr('content')
        },
        url: url,
        method: "POST",
        data: data,
        dataType: "JSON",
        success: function(jsonData) {
            var data = new google.visualization.DataTable();
            data.addColumn('number', 'Year');
            data.addColumn('number', 'Count');
            $.each(jsonData, function(i, jsonData) {
                var name = jsonData.name;
                var count = jsonData.count;
                data.addRows([
                    [name, count]
                ]);
            });

            var view = new google.visualization.DataView(data);
            view.setColumns([{
                sourceColumn: 0,
                type: 'string',
                calc: function(dt, rowIndex) {
                    return String(dt.getValue(rowIndex, 0));
                }
            }, 1]);

            var options = {
                hAxis: {
                    title: title.year,
                    logScale: false

                },
                vAxis: {
                    title: title.count,
                    logScale: false
                },
                colors: ['#4266b2', '#097138']
            };



            var chart = new google.visualization.LineChart(document.getElementById("columnchart_values"));
            chart.draw(view, options);
        }
    });

}

function drawBarChartClick(url, title, divDraw, data) {
    var chartCount = 0;


    $.ajax({
        headers: {
            'X-CSRF-TOKEN': $('meta[name="csrf-token"]').attr('content')
        },
        url: url,
        method: "POST",
        data: data,
        dataType: "JSON",
        success: function(chart_data) {
            var data = new google.visualization.DataTable();
            data.addColumn('string', 'Name');
            data.addColumn('number', 'Count');
            console.log(chart_data);
            $.each(chart_data, function(i, chart_data) {
                var name = chart_data.name;
                var dataID = chart_data.id;
                var count = chart_data.count;
                var namedataID = chart_data.name + '_' + chart_data.id;
                data.addRows([
                    [namedataID, count]
                ]);
                chartCount = chartCount + chart_data.count;
            });

            var options = {
                title: title,
                hAxis: {
                    title: 'Minor ',
                    viewWindow: {
                        min: [7, 30],
                        max: [17, 30]
                    }
                },
                vAxis: {
                    title: 'Count(' + chartCount + ')'
                }
            };

            var chart = new google.visualization.ColumnChart(document.getElementById(divDraw));

            function selectHandler(e) {
                var selection = chart.getSelection();
                if (selection.length > 0) {
                    var colLabel = data.getColumnLabel(selection[0].column);
                    var mydata = data.getValue(selection[0].row, 0);
                    chart.setSelection([]);
                    globalVariable.genderPieChart = mydata;
                    AlertBar(globalVariable.genderPieChart);
                }
            }
            google.visualization.events.addListener(chart, 'select', selectHandler);
            chart.draw(data, options);
        }
    });
}

function drawPieChart(url, title, divChart, data) {
    var chartCount = 0;
    $.ajax({
        headers: {
            'X-CSRF-TOKEN': $('meta[name="csrf-token"]').attr('content')
        },
        url: url,
        method: "POST",
        data: data,
        dataType: "JSON",
        success: function(chart_data) {
            var data = new google.visualization.DataTable();
            data.addColumn('string', 'Total');
            data.addColumn('number', 'Count');

            $.each(chart_data, function(i, chart_data) {
                var name = chart_data.name;
                var count = chart_data.count;
                var nameCount = chart_data.name + ' (' + chart_data.count + ')';
                data.addRows([
                    [nameCount, count]
                ]);
                chartCount = chartCount + chart_data.count;
            });

            var options = {
                title: chartCount + ' ' + title
            };

            var chart = new google.visualization.PieChart(document.getElementById(divChart));
            chart.draw(data, options);
        }
    });

}

function drawBarChart(url, title, divDraw, data) {
    var chartCount = 0;

    $.ajax({
        headers: {
            'X-CSRF-TOKEN': $('meta[name="csrf-token"]').attr('content')
        },
        url: url,
        method: "POST",
        data: data,
        dataType: "JSON",
        success: function(chart_data) {
            var data = new google.visualization.DataTable();
            data.addColumn('string', 'Name');
            data.addColumn('number', 'Count');
            $.each(chart_data, function(i, chart_data) {
                var name = chart_data.name;
                var count = chart_data.count;
                var nameCount = chart_data.name + ' (' + chart_data.count + ')';
                data.addRows([
                    [name, count]
                ]);
                chartCount = chartCount + chart_data.count;
            });

            var options = {
                title: title,
                hAxis: {
                    title: title,
                    viewWindow: {
                        min: [7, 30],
                        max: [17, 30]
                    }
                },
                vAxis: {
                    title: 'Count(' + chartCount + ')'
                }
            };

            var chart = new google.visualization.ColumnChart(document.getElementById(divDraw));
            chart.draw(data, options);
        }
    });
}

function drawChart(url, title, divChart, data) {
    
    var chartCount = 0;

    $.ajax({
        headers: {
            'X-CSRF-TOKEN': $('meta[name="csrf-token"]').attr('content')
        },
        url: url,
        method: "POST",
        data: data,
        dataType: "JSON",
        success: function(chart_data) {
            var data = new google.visualization.DataTable();
            data.addColumn('string', 'Total');
            data.addColumn('number', 'Count');

            $.each(chart_data, function(i, chart_data) {
                var name = chart_data.name;
                var count = chart_data.count;
                var nameCount = chart_data.name + ' (' + chart_data.count + ')';
                data.addRows([
                    [nameCount, count]
                ]);
                chartCount = chartCount + chart_data.count;
            });

            var options = {
                title: chartCount + ' ' + title
            };

            var chart = new google.visualization.PieChart(document.getElementById(divChart));
            chart.draw(data, options);
        }
    });
}

function safeInt(value) {
    if(value==null||value=='') return 0;
    return parseInt(value);
}

function formatDate(date, language) {
    var weekDays = [];
    if(language=='en') {
        weekDays = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    } else if(language=='ar') {
        weekDays = ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'];
    }

    var dayOfWeek = new Date(date).getDay();
    var day = isNaN(dayOfWeek) ? "" : weekDays[dayOfWeek]+", ";
    return day+date;
}

function showRepsonseErrors(response, done) {
    if(response.responseText!=null) {
        response = JSON.parse(response.responseText);
    }
    var errorString = '';
    if(response.error!=null) {
        errorString = response.error;
    }
    else if(response.message!=null) {
        errorString = response.message;
    }
    else if(response.errors!=null) {
        $.each( response.errors, function( key, value) {
            errorString += key + ', ' + value + ',';
        });
    } else {
        errorString = 'Error';
    }
    
    errorBox(errorString, done);
}

function toId(text) {
    return text.toLowerCase().replace(" ", "-");
}

function fileSizeText(size) {
    size = parseInt(size);
    if(size<1024)
        return Math.floor(size) + " Byte";
    if(size<1024*1024)
        return Math.floor(size/1024)  + " KB";
    if(size<1024*1024*1204)
        return Math.floor(size/(1024*1024))  + " MB";
    return Math.floor(size/(1024*1024*1204)) + " GB";
}

function messageBox(options, done, cancel) {
    var dialog  = $('#message_box_modal');
    if(dialog.length>0)
        dialog.remove();

    dialog = $("\
        <div class='modal fade' id='message_box_modal' tabindex='-1' role='dialog' aria-labelledby='message_box_modal' aria-hidden='true'>\
        <div class='modal-dialog' role='document'>\
            <div class='modal-content'> \
                <div class='modal-header'>\
                    <h5 class='modal-title' id='message_box_modal_label'></h5>\
                    <button type='button' class='close' data-dismiss='modal' aria-label='Close'>\
                        <span aria-hidden='true'>&times;</span>\
                    </button>\
                </div>\
                <div class='modal-body'>\
                </div>\
                <div class='modal-footer'>\
                </div>\
            </div>\
        </div>\
    </div>");

    if(options.confirm) {
        if(options.danger){
            var html = "\
            <button type='button' class='btn btn-danger' id='action'>OK</button>\
            <button type='button' class='btn btn-secondary' id='close'>Cancel</button>";
            dialog.find('.modal-footer').html(html);
        } else {
            var html = "\
            <button type='button' class='btn btn-primary' id='action'>OK</button>\
            <button type='button' class='btn btn btn-light' id='close'>Cancel</button>";
            dialog.find('.modal-footer').html(html);
        }
    } else {
        if(options.danger){
            var html = "\
            <button type='button' class='btn btn-danger' id='action'>Close</button>";
            dialog.find('.modal-footer').html(html);
        } else {
            var html = "\
            <button type='button' class='btn btn-success' id='action'>Close</button>";
            dialog.find('.modal-footer').html(html);
        }
    }
        
    $("body").append(dialog);

    if(typeof(options)=="string") {
        dialog.find('.modal-title').html("Information");
        dialog.find('.modal-body').html(options);
    } else {
        dialog.find('.modal-title').html(options.title);
        dialog.find('.modal-body').html(options.body);
    }

    dialog.find('#action').on('click', function(){
        dialog.modal('hide').on('hidden.bs.modal', function () {
                if(done!=null)done();
            });
    });

    dialog.find('#close').on('click', function(){
        dialog.modal('hide').on('hidden.bs.modal', function () {
                if(cancel!=null)cancel();
            });        
    });

    dialog.modal({backdrop: "static"});

    dialog.on('hidden.bs.modal', function () {
        dialog.remove();
    });
}

function infoBox(body, done) {
    messageBox({'title':'Information', 'body':body}, done);
}

function confirmBox(body, done, danger) {
    messageBox({'title':'Confirmation?', 'body':body, 'confirm': true, 'danger': danger}, done);
}

function errorBox(body, done) {
    messageBox({'title':'Error!!', 'body':body, danger: true}, done);
}

function warningBox(body, done) {
    messageBox({'title':'Warning!', 'body':body, danger: true, confirm: true}, done);
}

(function ( $ ) {

    $.fn.initializeSelectize = function(url = null) {
        if(url==null)
            url = $(this).attr('url');        
        $(this).selectize({
            valueField: 'id',
            labelField: 'name',
            searchField: 'search',
            plugins: ['remove_button'],
            options: [],
            create: false,
            render: {
                option: function(item, escape) {
                    return "<div style='padding:5px;'>" + escape(item.name) + "</div>";
                }
            },
            load: function(query, callback) {
                if (!query.length) return callback();
                $.ajax({
                    url: url,
                    type: 'GET',
                    dataType: 'jsonp',
                    jsonp: 'callback',
                    jsonpCallback: 'callback',
                    data: {
                        query: query,
                        limit: 10
                    },
                    error: function() {
                        callback();
                    },
                    success: function(res) {
                        callback(res);
                    }
                });
            }
        });
    }

    $.fn.autoFill = function(slaveSelectObj, updateURL, updateToken, initialSlaveValue, secondaryObj = null, thirdObj = null, fourthObj = null) {
        var masterSelectObj = $(this);
        if(secondaryObj!=null) {
            secondaryObj.on('change', function(){
                    masterSelectObj.trigger('change');
                });
        }
        if(thirdObj!=null) {
            thirdObj.on('change', function(){
                    masterSelectObj.trigger('change');
                });
        }
        if(fourthObj!=null) {
            fourthObj.on('change', function(){
                    masterSelectObj.trigger('change');
                });
        }

        masterSelectObj.on('change', function(){
            var emptyOptionText = slaveSelectObj.find("option[value='']").text();
            if(emptyOptionText)
                slaveSelectObj.find('option').remove().end().append($('<option>', {value:'', text:emptyOptionText})).val('');
            else
                slaveSelectObj.find('option').remove();
            var url = updateURL.replace('#id', $(this).val()).replace('#sid', (secondaryObj)?secondaryObj.val():"").replace('#tid', (thirdObj)?thirdObj.val():"").replace('#fid', (fourthObj)?fourthObj.val():"");
            $.post(url,  {
                "_token": updateToken
            }, function(response) {
                response.forEach(function(entry) {
                    slaveSelectObj.append($('<option>', {value:entry.id, text:entry.name}));
                });
                slaveSelectObj.val(initialSlaveValue);
                slaveSelectObj.trigger('change');
                initialSlaveValue = '';
            },'json');
        });

        if($(this).val()!='') {
            $(this).trigger('change');
        }
    }

    $.fn.inputFile = function() {
        var object = $(this);
        var name = object.attr('name');
        var placeHolder = object.attr('placeholder');
        var title = object.attr('title');
        var multiple = (object.attr('multiple'))?"multiple":"";
        var required = (object.attr('required'))?"required":"";

        var div = $("<div><input "+multiple+" name='"+name+"' "+required+" type='file' class='input-ghost' style='display:none;'>\
            <div class='input-group input-file'>\
            <span class='input-group-btn'>\
            <button style='width: 200px; text-align: left;' class='btn btn-default btn-choose' type='button'>\
            "+title+"</button></span>\
            <input type='text' class='form-control' placeholder='"+placeHolder+"' />\
            </div></div>");

        var element = div.find(":file");
        element.change(function() {
            if(element[0].files.length>1)
                div.find(':text').val(""+element[0].files.length+" files");
            else
                div.find(':text').val((element.val()).split('\\').pop());
        });

        div.find("button.btn-choose").click(function(){
            element.click();
        });

        div.find("button.btn-reset").click(function(){
            element.val(null);
            div.parents(".input-file").find('input').val('');
        });

        div.find('input').css("cursor","pointer");

        div.find(':text').mousedown(function() {
            element.click();
            return false;
        });

        object.replaceWith(div);
    }

    $.fn.showErrors = function(response, done) {
        if(response==null)return;
        if(response.message)
            errorBox(response.message, done);
        else if(response.error)
            errorBox(response.error, done);
        else
            errorBox('Error!', done);

        var form = $(this);
        form.find('.form-control').removeClass('is-invalid');
        form.find('.form-error').remove();
        $.each( response.errors, function( field, messages ) {
            for(var i=0;i<messages.length;i++) {
                field = (form.find('#'+field).length>0)?form.find('#'+field):form.find('[name='+field+']');
                field.after("<div class='form-error'>"+messages[i]+"</div>")
                field.addClass('is-invalid');
            }
        });
    }

    $.fn.execModal = function(options, done, failed, initialize) {
        var dialog = $(this).clone();
        if(initialize)initialize(dialog);
        dialog.find("input[data-remote]").dataRemote();

        dialog.find("form").ajaxForm({
                progressBar: dialog.find('#'+options.progressBar),
                progressText: dialog.find('#'+options.progressText),
            }, function(response){
            dialog.modal('hide').on('hidden.bs.modal', function () {
                if(done!=null)done(response);
            });
        }, function(response){
            if(failed!=null)
                failed(response);
            else
                dialog.showErrors(response);
        });

        dialog.modal({backdrop: "static"});

        dialog.on('hidden.bs.modal', function () {
            dialog.remove();
        });
    }

    $.fn.fixEmptyText = function() {
        $(this).focusout(function(){
            var orgValue = $(this).val();
            var finalValue = orgValue.trim();
            if(orgValue.length!=finalValue.length){
                $(this).val(finalValue);
            }
        });
    }

    $.fn.dataRemote = function() {

        var input = $(this);
        input.css('border-color', '');
        input.attr('data-remote-result', 'success');
        input.attr('saved-title', input.attr('title'));
        input.attr('saved-value', input.val());

        input.focusout(function(){

            var input = $(this);
            input.fixEmptyText();

            if(input.val().length==0 || input.val() == input.attr('saved-value')) {
                input.css('border-color', '');
                input.attr('title', input.attr('saved-title'));
                input.attr('data-remote-result', 'success');
                return;
            }

            $.ajax({
                url: input.attr('data-remote').replace('#value', input.val())
            })
            .done(function(data, textStatus, xhr) {
                if(data=="none") {
                    input.css('border-color', '');
                    input.attr('title', input.attr('saved-title'));
                    input.attr('data-remote-result', 'success');
                } else {
                    input.css('border-color', 'red');
                    input.attr('title', input.attr('data-remote-error'));
                    input.attr('data-remote-result', 'error')
                }
            })
            .fail(function(data, textStatus, xhr) {
                 input.closest("form").attr('data-remote-result', 'error');
            })
        });

        input.closest("form").submit(function(event){
            var inputs = $(this).find("input[data-remote-result='error']");
            if(inputs.length>0) {
                event.preventDefault();
                messageBox({title:'Errors!',body:inputs.first().attr('data-remote-error'),danger:true});
                return false;
            }
        });
    };

    $.fn.uploadFiles = function(options, finish, failed) {
        var token = options.token;
        var multiple = ($(this).attr('multiple')=="multiple")?"multiple":"";
        var name = ($(this).attr('multiple')=="multiple")?"files[]":"file";
        var form  = $("<form action='' hidden method='post' enctype='multipart/form-data'><input type='hidden' name='_token' value='"+token+"'><input type='file' name='"+name+"' "+multiple+"><input type='submit'></form>");
        $(this).after(form);

        form.ajaxForm({
                progressBar: options.progressBar,
                progressText: options.progressText,
            }, finish, failed);

        $(this).on('click', function(){
            form.attr('action', $(this).attr('action'));
            form.find(':file').val('');
            form.find(':file').trigger('click');
        });
        form.find(':file').on('change', function (){
            var files = $(this).prop("files")
            if(files.length>0){
                form.submit();
            }
        });
    };

    $.fn.uploadFilesAndSave = function(options) {
        var token = options.token;
        var multiple = ($(this).attr('multiple')=="multiple")?"multiple":"";
        var name = ($(this).attr('multiple')=="multiple")?"files[]":"file";
        var form  = $("<form action='' target='_blank' hidden method='post' enctype='multipart/form-data'><input type='hidden' name='_token' value='"+token+"'><input type='file' name='"+name+"' "+multiple+"><input type='submit'></form>");
        $(this).after(form);

        $(this).on('click', function(){
            form.attr('action', $(this).attr('action'));
            form.find(':file').val('');
            form.find(':file').trigger('click');
        });
        
        form.find(':file').on('change', function (){
            var files = $(this).prop("files")
            if(files.length>0){
                form.submit();
            }
        });
    };

    $.fn.ajaxForm = function(options, finish, failed, final) {

        $(this).on("submit", function(event){
            event.preventDefault();
            var form = $(this);
            var progressBar = options.progressBar;
            var progressText = options.progressText;
            var formData = new FormData(form[0]);
            var formAction = form.attr('action');
            form.find(':input:disabled').addClass('temp-disabled');
            form.find(':input').prop('disabled', true);

            if(progressBar)progressBar.fadeIn('fast');
            if(progressText)progressText.fadeIn('fast');

            function cleanup() {
                form.find(':input').prop('disabled', false);
                form.find('.temp-disabled').prop('disabled', true);
                form.find('.temp-disabled').removeClass('temp-disabled');
                if(progressBar)progressBar.fadeOut('slow', function(){progressBar.css('width', "0%");});
                if(progressText)progressText.fadeOut('slow', function(){progressText.text("");});
            }

            $.ajax({
                    url: formAction,
                    type: 'POST',
                    data: formData,
                    dataType : 'json', 
                    cache: false,
                    contentType: false,
                    processData: false,
                    success: function(data){
                        if(finish)finish(data);
                        form[0].reset();
                        cleanup();
                        if(final)final();
                    },
                    error:function(data){
                        if(failed)failed(JSON.parse(data.responseText), data.status);
                        cleanup();
                    },
                    failure: function(data){
                        if(failed)failed(JSON.parse(data.responseText));
                        cleanup();
                    },
                    xhr: function() {
                        var myXhr = $.ajaxSettings.xhr();
                        if (myXhr.upload) {
                            myXhr.upload.addEventListener('progress', function(e) {
                                if (e.lengthComputable) {
                                    var progress = ""+ Math.floor(100 * e.loaded/e.total) + "%";
                                    
                                    if(progressBar)progressBar.css('width', progress);
                                    if(progressText)progressText.text("("+progress+")");
                                }
                            } , false);
                        }
                        return myXhr;
                    }
            });
        })
    };

    $.fn.uploadFilesWithThumbs = function(options) {

        $(this).attr('name', 'files');

        var deleteURL = $(this).attr('delete_url');
        var uploadURL = $(this).attr('upload_url');
        var submitToken = $(this).attr('submit_token');

        $(this).fileuploader({
            changeInput: '<div class="fileuploader-input" style=\'background-color:transparent;padding:0px;margin:0px;\'>' +
                      '<div class="col-xl-12 fileuploader-input-inner" style=\'padding:0px;margin:0px;\'>' +
                          '<div width(integer) class="col-md-1 fileuploader-main-icon" style=\'padding:0px;margin:0px;\'></div>' +
                          '<div class="col-md-10 fileuploader-input-caption" style=\'margin:0px;\'><span>${captions.feedback}</span></div>' +
                          '<div class="col-md-2 fileuploader-input-button" style=\'margin:0px;\'><span>${captions.button}</span></div>' +
                      '</div>' +
                  '</div>',
            theme: 'dragdrop',
            upload: {
                url: uploadURL,
                data: {
                    "_token": submitToken
                },
                type: 'POST',
                enctype: 'multipart/form-data',
                start: true,
                synchron: true,
                beforeSend: null,
                onSuccess: function(data, item) {

                    // if success
                    if (data.isSuccess && data.files[0]) {
                        item.name = data.files[0].name;
                        item.id = data.files[0].id;
                        item.html.find('.column-title > div:first-child').text(data.files[0].name).attr('title', data.files[0].name);
                    }

                    // if warnings
                    if (data.hasWarnings) {
                        for (var warning in data.warnings) {
                            alert(data.warnings);
                        }

                        item.html.removeClass('upload-successful').addClass('upload-failed');
                        // go out from success function by calling onError function
                        // in this case we have a animation there
                        // you can also response in PHP with 404
                        return this.onError ? this.onError(item) : null;
                    }

                    var removeButton = item.html.find('.fileuploader-action-remove');
                    removeButton.addClass('fileuploader-action-success');
                    setTimeout(function() {
                        item.html.find('.progress-bar2').fadeOut(400);
                    }, 400);

                    setTimeout(function() {
                        removeButton.removeClass('fileuploader-action-success');
                    }, 2000);
                },
                onError: function(item) {
                    var progressBar = item.html.find('.progress-bar2');

                    if(progressBar.length) {
                        progressBar.find('span').html(0 + "%");
                        progressBar.find('.fileuploader-progressbar .bar').width(0 + "%");
                        item.html.find('.progress-bar2').fadeOut(400);
                    }

                    item.upload.status != 'cancelled' && item.html.find('.fileuploader-action-retry').length == 0 ? item.html.find('.column-actions').prepend(
                        '<a class="fileuploader-action fileuploader-action-retry" title="Retry"><i></i></a>'
                    ) : null;
                },
                onProgress: function(data, item) {
                    var progressBar = item.html.find('.progress-bar2');

                    if(progressBar.length > 0) {
                        progressBar.show();
                        progressBar.find('span').html(data.percentage + "%");
                        progressBar.find('.fileuploader-progressbar .bar').width(data.percentage + "%");
                    }
                },
                onComplete: null,
            },
            onRemove: function(item) {
                $.post(deleteURL.replace('#id', item.id),  {"_token": submitToken}, function(){
                    messageBox({'title':'Information', 'body':"Item deleted successfully"});
                });
            },
            captions: {
                feedback: 'Drag and drop files here',
                feedback2: 'Drag and drop files here',
                drop: 'Drag and drop files here',
                or: 'or',
                button: 'Upload Files',
            },
            dialogs: {
                // alert dialog
                alert: function(text) {
                    return alert(text);
                },
                // confirm dialog
                confirm: function(text, callback) {
                    var message = "Are you sure you want to delete this file?";
                    messageBox({'title': "Warning!", 'body':message, 'danger':true, 'confirm':true}, function() {
                        callback();
                    });
                }
            }
        });    
    };

    $.fn.countDown = function(done) {
            
        var countDownElement = $(this);
        
        countDownElement.addClass('badge');
        countDownElement.css('font-size', '100%');

        function timeDifference() {
            var finalTime = new Date(countDownElement.attr('final-time'));
            var now = new Date();
            return finalTime.getTime() - now.getTime();
        }

        function close() {
            countDownElement.removeClass('badge-primary');
            countDownElement.addClass('badge-danger');
            countDownElement.html(countDownElement.attr('close-message'));
        }
        var initialized = false;
        function initialize() {
            if(!initialized) {
                countDownElement.addClass('badge-primary');
                countDownElement.html("<span id='cd-days'></span>&nbsp;days&nbsp;\
                    <span id='cd-hours'></span>:\
                    <span id='cd-minutes'></span>:\
                    <span id='cd-seconds'></span>");
                initialized = true;
            }
        }

        function update(difference) {
            var seconds = Math.floor(difference / 1000);
            var minutes = Math.floor(seconds / 60);
            var hours = Math.floor(minutes / 60);
            var days = Math.floor(hours / 24);

            hours %= 24;
            minutes %= 60;
            seconds %= 60;

            countDownElement.find("#cd-days").text(days);
            countDownElement.find("#cd-hours").text(hours);
            countDownElement.find("#cd-minutes").text(minutes);
            countDownElement.find("#cd-seconds").text(seconds);
        }

        if(timeDifference()<0) {
            close();
            return;
        }
        
        var timer = setInterval(function() {
            var difference = timeDifference();
            if (difference <= 0) {
                close();
                clearInterval(timer);
                if(done)done();
            } else {
                initialize();
                update(difference);                 
            }               
        }, 1000);
    }

    $.fn.processList = function (listUrl, itemUrl, token) {

        var dialog = $(this).clone();
        var stopFlag = false;

        dialog.on('hidden.bs.modal', function () {
            stopFlag = true;
            dialog.remove();
        });

        $.post(listUrl, {"_token": token }, function(response) {

            var html = '';
            for (item of response) {
                html += `<li class="item" id="${item.id}"> 
                         <span class="icon" style="background-color:#abb2b9;"><i class="icon-time"></i>
                         </span> ${item.title}</li>`;
            }
            dialog.find('.items').html(html);

            dialog.modal({backdrop: "static"});

        }).fail(function(response) {
            showRepsonseErrors(response);
        }).then(function() {
            
            dialog.find('.item').each(function (i) {
                var item = $(this);
                var url = itemUrl.replace('#id', item.attr('id'));
                setTimeout(function(){ 
                    if(stopFlag)return;
                    $.post(url, {"_token": token }, function(response) {
                        item.find('.icon').attr('style', 'background-color:#1e8831;');
                        item.find('i').removeAttr('class').attr('class', 'icon-check');
                    }).fail( function(response) {
                        item.find('.icon').attr('style', 'background-color:red;');
                        item.find('i').removeAttr('class').attr('class', 'icon-error');
                        stopFlag = true;
                    });
                }, i*1000);
            });
        });
    }
    
}( jQuery ));

$(document).ready(function() {
    $("input[type='text']").fixEmptyText();
});