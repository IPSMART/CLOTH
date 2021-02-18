/* Custom Functions in JavaScript for ipsmart.co.in
 * Created By Mithun Das
 * on 27th dec 2017 
 */
var ZoomTextBoxModalId;
var COUNTER_FOR_SAVE = 0;
function GlobalFormatNumber(num, currentculture, decimal) {
    currentculture = currentculture || "en-IN";
    decimal = decimal = 2;
    Globalize.culture(currentculture);
    if (isNaN(num))
        return ('Number not valid');
    return (Globalize.format(num, "n" + decimal));
}
function modeView() {
    $('select').css('pointer-events', 'none');
    $('select').css('background-color', '#eeeeee');
    //$("select").attr("disabled", "true");
    $("input").attr("readonly", "readonly");
    $(".Help_image_button").hide();
    $("input[type='search']").prop("readonly", false);
    $(".Help_image_button_grid").hide();
    $(".Help_image_buttonAddRemove").hide();
    $("input[type='file']").attr("disabled", "true");
    $("input[type='radio']").attr("disabled", "true");
    $("input[type='checkbox']").attr("disabled", "true");
    $("input").attr("placeholder", "");
    $("textarea").attr("readonly", "readonly");
    $('.grid_title_box').prop('readonly', false)
    $(".grid_title_box").removeAttr('readonly');
}

function modeEdit() {
    $(".Help_image_button").hide();
    $(".Help_image_button_grid").hide();
}
/*                     End Mode Selection                               */

//Message Box
function msgInfo(msgText) {
    $("#WaitingMode").hide();
    $("#Msgdiv1").show();
    $("#info").show();
    //  $('input').blur(); auto fire blur event from the input field
    $("#msgbody_info").html(msgText + " ! ");
    $("#close_info").focus();
    return false;
}
function msgSuccess(msgText) {
    $("#WaitingMode").hide()
    $('#Msgdiv1').show();
    $('#success').show();
    $("#msgbody_success").html(msgText);
    $("#close_success").focus();
    return false;
}
function msgSuccess1(msgText) {
    $("#WaitingMode").hide()
    $('#Msgdiv1').show();
    $('#success1').show();
    //  $('input').blur(); auto fire blur event from the input field
    $("#msgbody_success1").html(msgText);
    $("#close_success1").focus();
    return false;
}

function msgSuccess2(msgText) {
    $("#WaitingMode").hide();
    $("#message_stay").val($("#loadurl").val());
    $("#Msgdiv1").show();
    $("#success2").show();
    //  $('input').blur(); auto fire blur event from the input field
    $("#msgbody_success2").html(msgText);
    $("#close_success2").focus();
    return false;
}

function msgSuccess3(msgText) {
    $("#WaitingMode").hide();
    $("#message_stay").val($("#loadurl").val());
    $("#Msgdiv1").show();
    $("#success3").show();
    $("#msgbody_success3").html(msgText);
    $("#close_success3").focus();
    return false;
}

function msgWarning(msgText) {
    $("#WaitingMode").hide();
    $("#message_stay").val($("#loadurl").val());
    $("#Msgdiv1").show();
    $("#warning").show();
    $("#msgbody_warning").html(msgText);
    $("#close_warning").focus();
    return false;
}

function msgError(msgText) {
    $("#WaitingMode").hide();
    $("#Msgdiv1").show();
    $("#error").show();
    $("#msgbody_error").html(msgText);
    $("#close_error").focus();
    return false;
}

//Empty field Validation
function emptyFieldCheck(msgText, focusfield) {
    var fieldValue = $("#" + focusfield).val();
    if (fieldValue.length <= 0) {
        msgInfo(msgText + " ! ");
        message_value = focusfield;
        return false;
    }
    else {
        return true;
    }
}
function getparkfromhelp(id, urll) {
    debugger
    $.ajax({
        type: 'get',
        url: urll,
        beforesend: $("#WaitingMode").show(),
        data: {
            value: id,
        },
        success: function (result) {
            result = result;
            $("#WaitingMode").hide();
            $('#helpDIV').html("");
            alert("You are going to recall entries from Park, before save it pleasse verify all the things");
            window.location = result;
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide(); $('#helpDIV').html("");
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
    return false;

}
function deleteparkfromhelp(id, id1, index) {
    $.ajax({
        type: 'post',
        url: $("#delete_parkrecords").val(),
        data: {
            ID: id,
        },
        success: function (result) {
            result = result;
            if (result == "1") {
                unparkRecords(id1, index);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
    return false;
}
function unparkRecords(MNUDET, UNQSNO) {
    $.ajax({
        type: 'post',
        url: $("#readparkrecords").val(),
        beforesend: $("#WaitingMode").show(),
        data: {
            MNUDET: MNUDET
        },
        success: function (result) {
            result = result;
            $("#WaitingMode").hide();
            $('#helpDIV').html(result);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
    return false;

}
//Page Validation

function pageValidation(id, MNUDET, UNQSNO) {
    var myEle = document.getElementById("RemoveComma");
    if (myEle) {
        var RemoveField = document.getElementById("RemoveComma").value.split(',');
        for (var i = 0; i <= RemoveField.length - 1; i++) {
            var ele = parseFloat(document.getElementById(RemoveField[i]).value.toString().replace(/,/g, '', "")).toFixed(2);
            document.getElementById(RemoveField[i]).value = ele;
        }
    }
    var myEleGrid = document.getElementById("GRIDCOMMAIDS");
    if (myEleGrid) {
        var GridId = document.getElementById("GRIDCOMMAIDS").value.split(',');
        for (var i = 0; i <= GridId.length - 1; i++) {
            var IDS = GridId[i].toString().split('^^');
            var TABLENAME = IDS[0];
            var GridRow = $("#" + TABLENAME + " > tbody > tr").length;
            for (var z = 1; z < IDS.length; z++) {
                for (var x = 0; x <= GridRow - 1; x++) {
                    if (document.getElementById(IDS[z] + x).value != "") {
                        document.getElementById(IDS[z] + x).value = parseFloat(document.getElementById(IDS[z] + x).value.toString().replace(/,/g, '')).toFixed(2);
                    }
                }
            }
        }
    }
    if (id == "P") {
        $.ajax({
            type: 'post',
            url: $("#parkrecords").val(),
            beforesend: $("#WaitingMode").show(),
            data: $('form').serialize() + "&MNUDET=" + MNUDET + "&UNQSNO=" + UNQSNO,
            success: function (result) {
                result = result;
                if (result == "1") {
                    $("#WaitingMode").hide();
                    $("#unpark").show();
                    msgInfo("Park Successfully.</br>Document image will be lost if you uploaded any with this entry");
                    return false;
                }
                else {
                    $("#WaitingMode").hide()
                    msgWarning(result);
                    return false;
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError(XMLHttpRequest.responseText);
                $("body span h1").remove(); $("#msgbody_error style").remove();
            }
        });
        return false;
    }
    if (id != "D") {
        var checkfld = requiredFieldValidator();
        if (checkfld == false) {
            return false;
        }
    }

    if (id == "D") {
        var ui = delete1();
        return false;
    }
    else {
        if (COUNTER_FOR_SAVE == 0) {
            COUNTER_FOR_SAVE = 1;
            $.ajax({
                type: 'post',
                url: $("#btnSave").val(),
                beforesend: $("#WaitingMode").show(),
                data: $('form').serialize(),
                success: function (result) {
                    result = result;
                    resultIndex = result.substring(0, 1);
                    if (resultIndex == "1") {
                        $("#WaitingMode").hide();
                        var strrel = result.split("~");
                        if (strrel.length == 1) {
                            var outr = strrel[0].substring(1);
                            msgSuccess1("Save Successfully" + outr);
                        }
                        else {
                            var outr = strrel[0].substring(1);
                            msgSuccess1("Save Successfully" + outr);
                            $("#SearchValue").val(strrel[1]);
                        }
                        return false;
                    }
                    else if (resultIndex == "2") {
                        $("#WaitingMode").hide();
                        msgSuccess2("Edited Successfully ");
                        return false;
                    }
                    else {
                        $("#WaitingMode").hide()
                        COUNTER_FOR_SAVE = 0;
                        msgWarning(result);
                        return false;
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $("#WaitingMode").hide(); COUNTER_FOR_SAVE = 0;
                    msgError(XMLHttpRequest.responseText);
                    $("body span h1").remove(); $("#msgbody_error style").remove();
                }
            });
        }
        else {
            $("#WaitingMode").hide();
        }
    }
}

function deletePageData() {
    $.ajax({
        type: 'post',
        url: $("#btnSave").val(),
        beforesend: $("#WaitingMode").show(),
        data: $('form').serialize(),
        success: function (result) {
            if (result == "3") {
                $("#WaitingMode").hide()
                msgSuccess3("Deleted Successfully");
                return false;
            }
            else {
                $("#WaitingMode").hide()
                msgWarning(result);
                return false;
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}

//For Upper Case Convert 
//hook upper
function toUpper(fld) {
    var val = fld.value;
    var upval = val.toUpperCase();
    var id = fld.id;
    $("#" + id).val(upval);
}
//Improver Loader
function showLoader(reference) {
    var href_val = $(reference).attr('href');
    if (href_val != "#") {
        $("#WaitingMode").show();
    }
}
function activeLoader() {
    $("#WaitingMode").show();
}
function hideLoader() {
    $("#WaitingMode").hide();
}
function inactiveLoader() {
    $("#WaitingMode").hide();
}
var specialKeys = new Array();
specialKeys.push(8); //Backspace

function numericOnly(elementRef, position) {
    var TAG = document.getSelection().toString() === elementRef.value;
    if (TAG == true && elementRef.readOnly == false) {
        elementRef.value = null;
    }
    var keyCodeEntered = (event.which) ? event.which : (window.event.keyCode) ? window.event.keyCode : -1;
    if ((keyCodeEntered >= 48) && (keyCodeEntered <= 57)) {
        if ((elementRef.value) && (elementRef.value.indexOf('.') >= 0)) {
            var aa = elementRef.value;
            var ab = aa.indexOf('.');
            var ac = aa.toString().substring(ab);
            if (ac.length <= position) {
                return true;
            }
            else if (elementRef.selectionStart <= ab) {
                return true;
            }
            else {
                return false;
            }
        }
        return true;
    }
        // '.' decimal point...
    else if (keyCodeEntered == 46) {
        // Allow only 1 decimal point ('.')...
        if ((elementRef.value) && (elementRef.value.indexOf('.') >= 0)) {
            return false;
        }
        else {
            return true;
        }
    }
    return false;
}
function GetHelp(urlstring, id, colnm, caption, FLD_ID, TAG) {
    TAG = TAG || "";
    const keyName = event.key;
    if ($('#' + FLD_ID).is('[readonly]')) { return false }
    if (keyName == "F1" || keyName == "F2" || keyName == undefined) {
        $.ajax({
            type: 'get',
            beforesend: $("#WaitingMode").show(),
            url: urlstring,
            data: { TAG: TAG, },
            success: function (result) {
                var MSG = result.indexOf('#helpDIV');
                if (MSG >= 0) {
                    $("#WaitingMode").hide();
                    $('#SearchFldValue').val(FLD_ID);
                    $('#helpDIV').html(result);
                    $('#ReferanceFieldID').val(id);
                    $('#ReferanceColumn').val(colnm);
                    $('#helpDIV_Header').html(caption);
                }
                else {
                    //$("#WaitingMode").hide();
                    //$("#Msgdiv1").show();
                    //$("#info").show();
                    //$("#btnok").focus();
                    //$("#msgbody_info").html(result);
                    msgInfo(result);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError(XMLHttpRequest.responseText);
                $("body span h1").remove(); $("#msgbody_error style").remove();
            }
        });
    }
}
// get Help Search With Single/multiple data data concate with /
function GetHelp_WithId(urlstring, id, colnm, caption, FLD_ID, fld_Ids) {
    var strarr = fld_Ids.split('/');
    fld_Ids = "";
    for (var i = 0; i < strarr.length; i++) {
        if (i == 0) {
            if (strarr[i] != "") {
                fld_Ids += document.getElementById(strarr[i]).value;
            }
        }
        else {
            fld_Ids += String.fromCharCode(181) + document.getElementById(strarr[i]).value;
        }
    }
    const keyName = event.key;
    if (keyName == "F1" || keyName == "F2" || keyName == undefined) {
        $.ajax({
            type: 'post',//First time get method here change for $('form').serialize()
            beforesend: $("#WaitingMode").show(),
            url: urlstring,
            data: $('form').serialize() + "&Code=" + fld_Ids,
            success: function (result) {
                var MSG = result.indexOf('#helpDIV');
                if (MSG >= 0) {
                    $("#WaitingMode").hide();
                    $('#SearchFldValue').val(FLD_ID);
                    $('#helpDIV').html(result);
                    $('#ReferanceFieldID').val(id);
                    $('#ReferanceColumn').val(colnm);
                    $('#helpDIV_Header').html(caption);
                }
                else {
                    $("#WaitingMode").hide();
                    msgInfo(result);
                    message_value = strarr[0];
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError(XMLHttpRequest.responseText);
                $("body span h1").remove(); $("#msgbody_error style").remove();
            }
        });
    }
}
//Select All CkeckBox
function SetAllCheckBoxes(obj, tablename) {

    var Table_name = "#" + tablename + " > tbody > tr";
    var lngth = $(Table_name).length;
    for (var i = 0; i < lngth - 1; i++) {
        var chkbox = document.getElementById("SelectCheck_" + i);
        chkbox.checked = obj.checked;
    }
    //need to change the function
    //var c = new Array();
    //var c1 = new Array();
    //c = document.getElementsByTagName('input');
    //var Table_name = "#" + tablename + " > tbody > tr";
    //var lngth = $(Table_name).length;
    //var totalRowCount = 0;
    //var rowCount = 0;
    //var table = document.getElementById(tablename);
    //var rows = table.getElementsByTagName("tr")
    //var column="";
    //for (var i = 0; i < rows.length; i++) {
    //    totalRowCount++;
    //    if (rows[i].getElementsByTagName("td").length > 0) {
    //        rowCount++;
    //        rows = rows[i].getElementsByTagName("td");
    //        c1 = document.getElementsByTagName('input');
    //    }
    //}




    //for (var i = 0; i < c.length; i++) {
    //    if (c[i].type == 'checkbox' && c[i].id != 'Deactive' && c[i].name == 'CompanyLocationName[1].Checked') {
    //        c[i].checked = obj.checked;
    //    }
    //}
}

//Select All CkeckBox For Grid Filter Data
function SetAllCheckBoxesFilter(obj, tablename, RowCheckID, FILTER_ID) {
    var filter = $("#" + FILTER_ID).val() || '';
    var rows = document.querySelector("#" + tablename + " tbody").rows;
    for (var i = 0; i <= rows.length - 1; i++) {
        var flag = 0;
        for (var x = 0; x <= rows[i].cells.length - 1; x++) {
            var firstCol = rows[i].cells[x].children[0].value.toUpperCase();
            if (firstCol.indexOf(filter.toUpperCase()) > -1) {
                flag = 1;
            }
        }
        if (flag == 1) {
            if ($("#" + RowCheckID + i).is(":visible")) {
                var chkbox = document.getElementById(RowCheckID + i);
                var isdisable = chkbox.getAttribute("disabled");
                if (isdisable == null) {
                    chkbox.checked = obj.checked;
                }
            }
        }
    }
}

//Maximum and Minimum number Limit Set
function set_min_max_limit(id, min, max) {
    var val = document.getElementById(id).value;
    var int_val = parseFloat(val);
    if (int_val < min || int_val > max) {
        msgInfo("Input no. should be " + min + "-" + max + " !! ");
        message_value = id;
        return false;
    }
    else {
        return true;
    }
}

//Search Pannel Open
function searchPannelOpen() {
    var SRC_SLCD = $("#SRC_SLCD").val();
    var SRC_DOCNO = $("#SRC_DOCNO").val();
    var SRC_FDT = $("#SRC_FDT").val();
    var SRC_TDT = $("#SRC_TDT").val();
    var SRC_FLAG = $("#SRC_FLAG").val();
    const keyName = event.key;
    if (keyName == "F" || keyName == "f") {
        $.ajax({
            type: 'get',
            beforesend: $("#WaitingMode").show(),
            url: $("#srhpnl").val(),
            data: "SRC_SLCD=" + SRC_SLCD + "&SRC_DOCNO=" + SRC_DOCNO + "&SRC_FDT=" + SRC_FDT + "&SRC_TDT=" + SRC_TDT + "&SRC_FLAG=" + SRC_FLAG,
            success: function (result) {
                $("#WaitingMode").hide();
                $('#helpDIV').html(result);
                $('#helpDIV_Header').html("Search Pannel");
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError("Error: " + textStatus + "," + errorThrown);
            }
        });
    }
    else if (keyName == undefined) {
        $.ajax({
            type: 'get',
            beforesend: $("#WaitingMode").show(),
            url: $("#srhpnl").val(),
            data: "SRC_SLCD=" + SRC_SLCD + "&SRC_DOCNO=" + SRC_DOCNO + "&SRC_FDT=" + SRC_FDT + "&SRC_TDT=" + SRC_TDT + "&SRC_FLAG=" + SRC_FLAG,
            success: function (result) {
                $("#WaitingMode").hide();
                $('#helpDIV').html(result);
                $('#helpDIV_Header').html("Search Pannel");
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError("Error: " + textStatus + "," + errorThrown);
            }
        });
    }
}

//update querystring
function updateQueryStringParameter(uri, key, value) {
    // remove the hash part before operating on the uri
    var i = uri.indexOf('#');
    var hash = i === -1 ? '' : uri.substr(i);
    uri = i === -1 ? uri : uri.substr(0, i);

    var re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
    var separator = uri.indexOf('?') !== -1 ? "&" : "?";

    if (!value) {
        // remove key-value pair if value is empty
        uri = uri.replace(new RegExp("([?&]?)" + key + "=[^&]*", "i"), '');
        if (uri.slice(-1) === '?') {
            uri = uri.slice(0, -1);
        }
        // replace first occurrence of & by ? if no ? is present
        if (uri.indexOf('?') === -1) uri = uri.replace(/&/, '?');
    } else if (uri.match(re)) {
        uri = uri.replace(re, '$1' + key + "=" + value + '$2');
    } else {
        uri = uri + separator + key + "=" + value;
    }
    return uri + hash;
}
function getQueryStringParameter(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;
    // remove the hash part before operating on the uri
    var i = sPageURL.indexOf('#');
    var hash = i === -1 ? '' : sPageURL.substr(i);
    sPageURL = i === -1 ? sPageURL : sPageURL.substr(0, i);
    ////removed
    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
};


function GetPassword(id, txt) {
    var chk = document.getElementById(id);
    var url = document.getElementById("hdd1").value;
    if (chk.checked == true) {
        $.ajax({
            type: 'GET',
            url: url,
            datatype: "html",
            success: function (data) {
                var txtpass = document.getElementById(txt);
                txtpass.value = data;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError(XMLHttpRequest.responseText);
                $("body span h1").remove(); $("#msgbody_error style").remove();
            }
        });
    }
    else {
        var txtpass = document.getElementById(txt);
        txtpass.value = "";
        chk.checked = false;
        return false;
    }
}

function GetUser(id, txt) {
    var chk = document.getElementById(id);
    var url = document.getElementById("hdd2").value;
    if (chk.checked == true) {
        $.ajax({
            type: 'GET',
            url: url,
            datatype: "html",
            success: function (data) {
                var txtpass = document.getElementById(txt);
                txtpass.value = data;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError(XMLHttpRequest.responseText);
                $("body span h1").remove(); $("#msgbody_error style").remove();
            }
        });
    }
    else {
        var txtpass = document.getElementById(txt);
        txtpass.value = "";
        chk.checked = false;
        return false;
    }
}

function validCompanySelection() {
    var e = document.getElementById("COMPCD");
    var strUser = e.options[e.selectedIndex].value;
    if (strUser == "") {
        document.getElementById("validationMessage").innerHTML = "Select Company First";
        return false;
    }
    var e1 = document.getElementById("LOCCD");
    var strUser1 = e1.options[e1.selectedIndex].value;
    if (strUser1 == "") {
        document.getElementById("validationMessage").innerHTML = "Select Location";
        return false;
    }
    var e2 = document.getElementById("Finyr");
    var strUser2 = e2.options[e2.selectedIndex].value;
    if (strUser2 == "") {
        document.getElementById("validationMessage").innerHTML = "Select Financial year";
        return false;
    }
    return true;
}
function filterTable(event, id) {
    var filter = event.target.value.toLowerCase();
    var rows = document.querySelector("#" + id + " tbody").rows;
    for (var i = 0; i <= rows.length - 1; i++) {
        var flag = 0;
        for (var x = 0; x <= rows[i].cells.length - 1; x++) {
            var TYPE = rows[i].cells[x].children[0];
            var firstCol = "";
            if (TYPE.tagName == "SELECT") {
                var VALUE = rows[i].cells[x].children[0];
                firstCol = VALUE.options[VALUE.selectedIndex].text.toLowerCase();
            }
            else {
                firstCol = rows[i].cells[x].children[0].value.toLowerCase();
            }
            if (firstCol.indexOf(filter) > -1) {
                flag = 1;
            }
        }
        if (flag == 1) {
            rows[i].style.display = "";
        }
        else {
            rows[i].style.display = "none";
        }
    }
}


//Cancel Record
function entryCancelation() {
    document.getElementById("CAN_overlay").style.display = "block";
    document.getElementById("CAN_Remarks").focus();
}
function CAN_Remark_close() {
    document.getElementById("CAN_Remarks").value = "";
    document.getElementById("CAN_overlay").style.display = "none";
}
function entryundoCancelation() {
    document.getElementById("CAN_overlay").style.display = "block";
}
function CAN_Remark_undo() {
    document.getElementById("CAN_overlay").style.display = "none";
}
function CAN_record_save(remarks) {
    var rem = document.getElementById("CAN_Remarks").value;
    if (!emptyFieldCheck("Please Enter Remark for Cancelation", "CAN_Remarks")) { return false; }
    $.ajax({
        type: 'post',
        url: $("#urlcancelrecord").val(),
        beforesend: $("#WaitingMode").show(),
        data: $('form').serialize() + "&par1=" + rem,
        success: function (result) {
            result = result;
            if (result == "1") {
                $("#WaitingMode").hide();
                document.getElementById("CAN_Remarks").value = "";
                document.getElementById("CAN_overlay").style.display = "none";
                msgSuccess1("Record has been cancelled");
                return false;
            }
            else {
                $("#WaitingMode").hide()
                document.getElementById("CAN_Remarks").value = "";
                document.getElementById("CAN_overlay").style.display = "none";
                msgWarning(result);
                return false;
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            document.getElementById("CAN_Remarks").value = "";
            document.getElementById("CAN_overlay").style.display = "none";
            msgError("Error: " + textStatus + "," + errorThrown);
        }
    });
}
function CAN_UNDO_Record() {
    $.ajax({
        type: 'post',
        url: $("#urlcancelrecord").val(),
        beforesend: $("#WaitingMode").show(),
        data: $('form').serialize() + "&par1=*#*",
        success: function (result) {
            result = result;
            if (result == "1") {
                $("#WaitingMode").hide();
                document.getElementById("CAN_overlay").style.display = "none";
                msgSuccess1("Record has been uncancelled");
                return false;
            }
            else {
                $("#WaitingMode").hide()
                document.getElementById("CAN_overlay").style.display = "none";
                msgWarning(result);
                return false;
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            document.getElementById("CAN_overlay").style.display = "none";
            msgError("Error: " + textStatus + "," + errorThrown);
        }
    });
}
//Check Record
function OpenCheckRemarks() {
    document.getElementById("CHK_overlay").style.display = "block";
    document.getElementById("CHK_Remarks").focus();
}
function CHK_Remark_close() {
    document.getElementById("CHK_Remarks").value = ""; 
    document.getElementById("CHK_overlay").style.display = "none";
    document.getElementById("Authorise_overlay").style.display = "none";
}

//function CHK_CheckUncheck_save(action) {
//    var rem = $("#CHK_Remarks").val(); if (rem == undefined) rem = '';
//    if (action == 'Check') {
//        if (!emptyFieldCheck("Please Enter Remark for Check", "CHK_Remarks")) { return false; }
//    }
//    $.ajax({
//        type: 'post',
//        url: $("#urlCheckAuthrecord").val(),
//        beforesend: $("#WaitingMode").show(),
//        data: $('form').serialize() + "&par1=" + rem,
//        success: function (result) {
//            result = result;
//            if (result == "1") {
//                $("#WaitingMode").hide();
//                $("#CHK_Remarks").val("");
//                document.getElementById("CHK_overlay").style.display = "none";
//                msgSuccess1("Record has been Checked");
//                return false;
//            }
//            else if (result == "2") {
//                $("#WaitingMode").hide();
//                $("#CHK_Remarks").val("");
//                document.getElementById("CHK_overlay").style.display = "none";
//                msgSuccess1("Record has been Unchecked");
//                return false;
//            }
//            else {
//                $("#WaitingMode").hide();
//                $("#CHK_Remarks").val("");
//                document.getElementById("CHK_overlay").style.display = "none";
//                msgWarning(result);
//                return false;
//            }
//        },
//        error: function (XMLHttpRequest, textStatus, errorThrown) {
//            $("#WaitingMode").hide();
//            $("#CHK_Remarks").val("");
//            document.getElementById("CHK_overlay").style.display = "none";
//            msgError("Error: " + textStatus + "," + errorThrown);
//        }
//    });
//}

//Authorised Record
function OpenAuthoriseRemarks() {
    document.getElementById("Authorise_overlay").style.display = "block";
    document.getElementById("Authorise_Remarks").focus();
}

function SaveCheckAuthorisation(action) {
    var rem = "";
    if (action == 'UnCheck') {
    }
    else if (action == 'Check') {
        if (!emptyFieldCheck("Please Enter Remark for Check", "CHK_Remarks")) { return false; }
        rem = document.getElementById("CHK_Remarks").value;
    }
    else if (action == 'Unauthorise') {
        if (!emptyFieldCheck("Please Enter Remark for Unauthorise", "Authorise_Remarks")) { return false; }
        rem = document.getElementById("Authorise_Remarks").value;
    }
    else if (action == 'Authorise') {
        if (!emptyFieldCheck("Please Enter Remark for Authorise", "Authorise_Remarks")) { return false; }
        rem = document.getElementById("Authorise_Remarks").value;
    }
    $.ajax({//
        type: 'post',
        url: $("#urlCheckAuthrecord").val(),
        beforesend: $("#WaitingMode").show(),
        data: $('form').serialize() + "&callingfor=" + action + "&remarks=" + rem,
        success: function (result) {
            $("#WaitingMode").hide();
            $("#CHK_Remarks").val("");
            $("#Authorise_Remarks").val("");
            document.getElementById("CHK_overlay").style.display = "none";
            document.getElementById("Authorise_overlay").style.display = "none";
            if (result == "Check") {
                msgSuccess1("Record has been Checked");
                return false;
            }
            else if (result == "UnCheck") {
                msgSuccess1("Record has been Unchecked");
                return false;
            }
            else if (result == "Authorise") {
                msgSuccess1("Record has been Authorised");
            }
            else if (result == "Unauthorise") {
                msgSuccess1("Record has been Unauthorised");
            }
            else {
                msgWarning(result);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            $("#CHK_Remarks").val("");
            msgError("Error: " + textStatus + "," + errorThrown);
        }
    });
}



function returncolvalue(val, colname) {
    if (val == "" || colname == "") return "";
    var findin = val;
    var retval = "";
    var colfind = "^" + colname.toUpperCase() + "=^";
    var spltchar = String.fromCharCode(181);

    var pos = findin.indexOf(colfind);
    if (pos < 0) retval = "";
    else {
        var extpos = colname.length + 3;
        var reststr = findin.substr(pos + extpos);
        var npos = reststr.indexOf(spltchar);
        retval = findin.substr(pos + extpos, npos);
    }
    return retval;
}

function Disc_Cal(TYPE, RATE, QNTY, BOX, AMOUNT, AMT) {
    //if (AMT == "") { AMT = parseInt(0) } else { AMT = parseInt(AMT) }
    AMOUNT = AMOUNT || parseFloat(0).toFixed(2);
    var DISC_AMT = 0;
    if (TYPE == "Q") { DISC_AMT = RATE * QNTY; }
        //else if (TYPE == "B") { DISC_AMT = RATE * BOX * QNTY; }
    else if (TYPE == "B") { DISC_AMT = RATE * BOX; }
    else if (TYPE == "P") { DISC_AMT = (AMOUNT * RATE) / 100; }
    else if (TYPE == "F") { DISC_AMT = RATE; }
    else if (TYPE == "A") { DISC_AMT = (AMT * RATE) / 100; }
    else { DISC_AMT = 0; }
    return parseFloat(DISC_AMT).toFixed(2);
}

function NegativeNumericOnly(elementRef, position) {
    var keyCodeEntered = (event.which) ? event.which : (window.event.keyCode) ? window.event.keyCode : -1;
    if ((keyCodeEntered >= 48) && (keyCodeEntered <= 57)) {
        if ((elementRef.value) && (elementRef.value.indexOf('.') >= 0)) {
            var aa = elementRef.value;
            var ab = aa.indexOf('.');
            var ac = aa.toString().substring(ab);
            if (ac.length <= position) {
                return true;
            }
            else if (elementRef.selectionStart <= ab) {
                return true;
            }
            else {
                return false;
            }
        }
        return true;
    }
    else if (keyCodeEntered == 46) // '.' double point...
    {
        if ((elementRef.value) && (elementRef.value.indexOf('.') >= 0)) // Allow only 1 double point ('.')...
        {
            return false;
        }
        else {
            return true;
        }
    }
    else if (keyCodeEntered == 45) // '-' Negative Value...
    {
        if ((elementRef.value) && (elementRef.value.indexOf('-') >= 0)) // Allow only 1 double point ('.')...
        {
            return false;
        }
        else {
            return true;
        }
    }
    return false;
}
function htmlRetNumber(val) {
    if (val == "") {
        val = parseFloat(0);
    }
    else {
        val = parseFloat(val);
    }
    return val;
}
function SortableColumn(tableNM) {
    var f_sl = 1;
    var lastclick = -1;
    $("#" + tableNM + " thead th").each(function () {
        $(this).hover(function () {
            $(this).css("cursor", "pointer");
            $(this).css("background-color", "rgb(86, 137, 167)");
        }, function () {
            var nm = $(this).prevAll().length;
            if (nm != lastclick) {
                $(this).css("background-color", "#808080");
            }
        });
        $(this).click(function () {
            f_sl *= -1;
            var n = $(this).prevAll().length;
            sortTable(f_sl, n, tableNM);
            $("#" + tableNM + " thead th").each(function () {
                var nn = $(this).prevAll().length;
                if (nn == n) {
                    $(this).css("background-color", "rgb(86, 137, 167)");
                    lastclick = n;
                }
                else {
                    $(this).css("background-color", "#808080");
                }
            });
        });

    });
}
function sortTable(f, n, tableNM) {
    var rows = $('#' + tableNM + ' tbody  tr').get();
    rows.sort(function (a, b) {

        var A = getVal(a);
        var B = getVal(b);
        if (A < B) {
            return -1 * f;
        }
        if (A > B) {
            return 1 * f;
        }
        return 0;
    });

    function getVal(elm) {
        var v = $(elm).children('td').eq(n).children('input')[0].value.toUpperCase();
        if ($.isNumeric(v)) {
            v = parseInt(v, 10);
        }
        return v;
    }

    $.each(rows, function (index, row) {
        $('#' + tableNM).children('tbody').append(row);
    });
}
function FooterFilter(tableNM) {
    $('#' + tableNM + ' tfoot th').each(function () {
        var title = $(this).attr('title');
        if (title == undefined) {
            title = "";
            $(this).html('<input type="text" style="width:100%;background-color:#c8f0f3;border:1px solid darkgray;" placeholder="' + title + '" />');
        }
        else if (title != "TOTAL") {
            $(this).html('<input type="text" style="width:100%;background-color:#c8f0f3;border:1px solid darkgray;" placeholder="' + title + '" />');
        }
    });
    $('#' + tableNM + ' tfoot th').on("keyup", function (e) {
        var filter = e.target.value.toLowerCase();
        var cellindex = e.target.parentNode.cellIndex;
        var rows = $('#' + tableNM + ' tbody  tr:visible').get();
        var hidden = $('#' + tableNM + ' tbody  tr:hidden').get();
        for (var i = 0; i <= rows.length - 1; i++) {
            var flag = 0;
            var firstCol = rows[i].cells[cellindex].children[0].value.toLowerCase();
            if (firstCol.indexOf(filter) > -1) {
                flag = 1;
            }
            if (flag == 1) {
                rows[i].style.display = "";
            }
            else {
                rows[i].style.display = "none";
                rows[i].setAttribute("data-row-filter", cellindex);
                var colfilter = $('#' + tableNM).attr("data-column-filter");
                if (colfilter == undefined) {
                    $('#' + tableNM).attr("data-column-filter", cellindex);
                }
                else {
                    var indexchk = colfilter.split('~');
                    var ss = indexchk.length;
                    var newid = 0;
                    for (var u = 0; u <= ss - 1; u++) {
                        if (indexchk[u] == cellindex.toString()) {
                            newid = 1;
                        }
                    }
                    if (newid == 0) {
                        $('#' + tableNM).attr("data-column-filter", colfilter + "~" + cellindex.toString());
                    }
                }
            }
        }
        var indexfilter = $('#' + tableNM).attr("data-column-filter");
        var arraycol = [];
        var newid1 = "";
        if (indexfilter == undefined) {
            arraycol.push(cellindex.toString());
        }
        else {
            var position = indexfilter.indexOf(cellindex.toString());
            var newindex = indexfilter.substring(position);
            var indexc = newindex.split("~");
            var ss1 = indexc.length;
            newid1 = indexfilter.substring(0, position);
            for (var u = 0; u <= ss1 - 1; u++) {
                arraycol.push(indexc[u]);
            }
        }
        if (position >= 0) {
            for (var i = 0; i <= hidden.length - 1; i++) {
                var flag = 0;
                var getattar = hidden[i].getAttribute("data-row-filter");
                if (getattar != null) {
                    var ss1 = arraycol.length;
                    for (var u = 0; u <= ss1 - 1; u++) {
                        if (getattar == arraycol[u]) {
                            var firstCol = hidden[i].cells[cellindex].children[0].value.toLowerCase();
                            if (firstCol.indexOf(filter) > -1) {
                                flag = 1;
                            }
                            if (flag == 1) {
                                hidden[i].style.display = "";
                                hidden[i].removeAttribute("data-row-filter");
                                if (newid1 == "") {
                                    if (filter == "") {
                                        $('#' + tableNM).removeAttr("data-column-filter");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    });
}

function DisableSpecificDates(date) {
    var m = date.getMonth();
    var d = date.getDate();
    var y = date.getFullYear();
    m = m + 1;
    var currentdate = d.toString().padStart(2, '0') + '/' + m.toString().padStart(2, '0') + '/' + y;
    for (var i = 0; i < disableddates.length; i++) {
        if ($.inArray(currentdate, disableddates) != -1) {
            return [false];
        }
        else {
            return [true];
        }
    }
    if (disableddates.length <= 0) {
        return [true];
    }
}
function ClearAllTextBoxes(val) {
    if (val != "" && val != undefined && val != null) {
        var COL_ID = val.split(',');
        if (COL_ID.length != null && COL_ID.length > 0) {
            for (var i = 0; i <= COL_ID.length - 1; i++) {
                $("#" + COL_ID[i]).val("");
            }
        }
    }
}
function inputfocus() {
    $(this).on('keydown', 'input,select,text,button', function (e) {
        var self = $(this)
          , form = self.parents('form:eq(0)')
          , focusable
          , next
        ;
        if (e.keyCode == 13) {
            focusable = form.find('input,a,select').filter(':visible:not([readonly]):not([tabindex="-1"])');
            next = focusable.eq(focusable.index(this) + 1);
            if (next.length) {
                //if disable try get next 10 fields
                if (next.is(":disabled")) {
                    for (i = 2; i < 10; i++) {
                        next = focusable.eq(focusable.index(this) + i);
                        if (!next.is(":disabled"))
                            break;
                    }
                }
                next.focus();
                next.select();
            }
            else {
            }
            return false;
        }

        //if (e.which == 39) { // right arrow
        //    ////$(this).closest('td').next().find('input').focus(); //only for select
        //    ////$("input").click(function () {
        //    ////$(this).select();
        //    ////});

        //    //e.preventDefault();
        //    //$(this).closest('td').next().find('input').select();//select with
        //}
        //else if (e.which == 37) { // left arrow
        //    e.preventDefault();
        //    $(this).closest('td').prev().find('input').select();
        //}  else
        if (e.which == 40) { // down arrow
            e.preventDefault();
            $(this).closest('tr').next().find('td:eq(' + $(this).closest('td').index() + ')').find('input').filter(':visible:not([readonly]):not([tabindex="-1"])').select();
        } else if (e.which == 38) { // up arrow
            e.preventDefault();
            $(this).closest('tr').prev().find('td:eq(' + $(this).closest('td').index() + ')').find('input').filter(':visible:not([readonly]):not([tabindex="-1"])').select();
        }
    });
}
function DocumentDateCHK(dateField, auto, current_date_auto_if_blank) {
    auto = auto || true;
    current_date_auto_if_blank = current_date_auto_if_blank || false;
    var currentYEAR = new Date().getFullYear().toString();
    var currentMONTH = (new Date().getMonth() + 1).toString();
    currentMONTH = currentMONTH.padStart(2, '0');
    var min = $("#" + dateField.id).datepicker("option", "minDate");
    var max = $("#" + dateField.id).datepicker("option", "maxDate");
    var maxdate = null;
    var mindate = null;
    if (max == null || max == "") {
        maxdate = new Date("2040-03-31");
    }
    else {
        var dateParts = max.split("/");
        maxdate = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);
    }
    if (min == null || min == "") {
        mindate = new Date("1950-04-01");
    }
    else {
        var dateParts1 = min.split("/");
        mindate = new Date(dateParts1[2], dateParts1[1] - 1, dateParts1[0]);
    }
    var dateParts2 = dateField.value.split("/");
    if (auto == true) {
        if (dateParts2[0].length == 0) {
            if (current_date_auto_if_blank == true) {
                dateParts2[0] = new Date().getDate().toString().padStart(2, '0');
            }
            else {
                return false;
            }
        }
        else if (dateParts2[0].length == 1) {
            dateParts2[0] = dateParts2[0].padStart(2, '0');
        }
        else {
            var ddt = parseInt(dateParts2[0]);
            if (ddt <= 31) {
                dateParts2[0] = dateParts2[0].padStart(2, '0');
            }
            else {
                dateParts2[0] = new Date().getDate().toString().padStart(2, '0');
            }
        }
        if (dateParts2[1] == null) {
            dateParts2[1] = currentMONTH;
        }
        else if (dateParts2[1].length == 1) {
            dateParts2[1] = dateParts2[1].padStart(2, '0');
        }
        else {
            var ddt = parseInt(dateParts2[1]);
            if (ddt <= 12) {
                dateParts2[1] = dateParts2[1].padStart(2, '0');
            }
            else {
                dateParts2[1] = currentMONTH;
            }
        }
        if (dateParts2[2] == null) {
            dateParts2[2] = currentYEAR;
        }
        else (dateParts2[2].length < 4)
        {
            dateParts2[2] = dateParts2[2].padStart(4, '20');;
        }
    }
    var inputdate = new Date(dateParts2[2], dateParts2[1] - 1, dateParts2[0]);
    var invalid = inputdate.toString();
    if (invalid == "Invalid Date") {
        msgInfo("Invalid Date [Date Put In Hand].Retype Date to the date box. !!");
        message_value = dateField.id;
        return false;
    }
    if (inputdate < mindate || inputdate > maxdate) {
        msgInfo("Date not correct or Date not valid between permission date .[Date Put In Hand].Retype Date to the date box. !! ");
        message_value = dateField.id;
        return false;
    }
    if (auto == true) {
        var day = inputdate.getDate().toString().padStart(2, '0');
        var mon = (inputdate.getMonth() + 1).toString().padStart(2, '0');
        var yy = inputdate.getFullYear().toString();
        if (typeof disableddates !== 'undefined') {
            var currentdate = day + "/" + mon + "/" + yy;
            if (disableddates.length > 0) {
                if ($.inArray(currentdate, disableddates) != -1) {
                    msgInfo("Date not correct or Date not valid between permission date .[Date Put In Hand].Retype Date to the date box. !! ");
                    message_value = dateField.id;
                    return false;
                }
            }
        }
        dateField.value = day + "/" + mon + "/" + yy;
    }
}

var message_value;
function closeDiv(id, flag) {
    $(id).hide();
    var will_go = message_value;
    $("#" + will_go).focus();
    if (flag == 1) {
        var servl = $("#SearchValue").val();
        if (servl == "") {
            location.reload();
        }
        else {
            if (typeof (Storage) !== "undefined") {
                var TEMPC = localStorage.getItem("ADDCONTROL");
                if (TEMPC == "NOTAUTOADD") {
                    var crntLocation = document.location.href;
                    var ViewLocation = crntLocation.replace("op=A", "op=V");
                    ViewLocation = updateQueryStringParameter(ViewLocation, "searchValue", servl);
                    location.href = ViewLocation;
                }
                else {
                    location.reload();
                }
            } else {
                alert("Sorry, your browser does not support Web Storage...");
            }
        }
    }
    else if (flag == 2) {
        var crntLocation = document.location.href;
        var ViewLocation = crntLocation.replace("op=E", "op=V");
        location.href = ViewLocation;
    }
    else if (flag == 3) {
        debugger;
        var crntLocation = document.location.href;
        var ViewLocation = "";
        //for next
        var keystat = crntLocation.search('searchValue');
        if (keystat != -1) {
            ViewLocation = updateQueryStringParameter(crntLocation, "searchValue", "");
            ViewLocation = updateQueryStringParameter(ViewLocation, "key", "N");
            location.href = ViewLocation;
            return false;
        }
        //for next
        keystat = crntLocation.search('key=N');
        if (keystat != -1) {
            var paramsUrl = new window.URLSearchParams(window.location.search);
            var nind = paramsUrl.get('Nindex').replace(/ /g, "+");
            var nindexval = parseInt(nind);
            nindexval = nindexval - 1;
            ViewLocation = updateQueryStringParameter(crntLocation, "Nindex", nindexval);
            // ViewLocation = updateQueryStringParameter(ViewLocation, "key", "P");
            location.href = ViewLocation;
            return false;
        }
        //for Previous
        keystat = crntLocation.search('key=P');
        if (keystat != -1) {
            var paramsUrl = new window.URLSearchParams(window.location.search);
            var nind = paramsUrl.get('Nindex').replace(/ /g, "+");
            var nindexval = parseInt(nind);
            nindexval = nindexval - 1;
            ViewLocation = updateQueryStringParameter(crntLocation, "Nindex", nindexval);
            location.href = ViewLocation;
            return false;
        }
        else {
            location.reload();
        }
    }
}
function EnabledfilteredSearch() {
    $("#SRC_SLCD").attr("readonly", false);
    $("#SRC_DOCNO").attr("readonly", false);
    $("#SRC_FDT").attr("readonly", false);
    $("#SRC_TDT").attr("readonly", false);
    $("#SRC_FLAG").attr("readonly", false);
    return;
}
$(document).ready(function () {
    $("#ADDCONTROL").click(function () {
        if ($(this).is(":checked")) {
            localStorage.setItem("ADDCONTROL", "AUTOADD");
        }
        else if ($(this).is(":not(:checked)")) {
            localStorage.setItem("ADDCONTROL", "NOTAUTOADD");
        }
    });
    if (typeof (Storage) !== "undefined") {
        var TEMPC = localStorage.getItem("ADDCONTROL");
        if (TEMPC == "NOTAUTOADD") {
            $("#ADDCONTROL").prop('checked', false);
        }
        else {
            $("#ADDCONTROL").prop('checked', true);
        }
    } else {
        alert("Sorry, your browser does not support Web Storage...[_LAYOUT]");
    }
    $("td").on("mouseover", function (e) {
        var titlee = $(this).find("input").val(); $(this).attr('title', titlee);
    });

});
function OpenZoomTextBoxModal(id) {
    ZoomTextBoxModalId = id;
    var tye = $("#" + id).val();
    var maxLength = document.getElementById(id).maxLength;
    $("#txtZoomTextBoxModal").prop('maxLength', maxLength);
    $("#txtZoomTextBoxModal").val(tye);
    return;
}
function CloseZoomTextBoxModal() {
    $("#" + ZoomTextBoxModalId).val($("#txtZoomTextBoxModal").val());
    $("#" + ZoomTextBoxModalId).focus();
}
var hlpblurval = "";
function GetHelpBlur(urlstring, caption, hlpfield, blurflds, dependfldIds,formdata) {
    debugger;
    if($("#" + hlpfield).prop('readonly')) return true
    const keyName = event.key;
    const keyType = event.type;
    var blurvalue = "";
    var value = $("#" + hlpfield).val();
    if (value == hlpblurval && (keyType == "mousedown" || keyName == "F2")) {
        value = "";
    }
    if (keyName != "F2" && keyName != undefined) { return true; }
    //if ($('#' + hlpfield).is('[readonly]')) { return false; }
    var fldIdseq = "", hpnlIndexseq = "";
    if (typeof dependfldIds != "undefined" && dependfldIds != "") {
        var strarr = dependfldIds.split('/');
        dependfldIds = "";
        for (var i = 0; i < strarr.length; i++) {
            if (i == 0) {
                if (strarr[i] != "") {
                    dependfldIds += $("#" + strarr[i]).val();
                }
            }
            else {
                dependfldIds += String.fromCharCode(181) + $("#" + strarr[i]).val();
            }
        }
    }
    else {
        dependfldIds = "";
    }
    var tmpbid = blurflds.split("/");//(e.g:"ColSLCD=SLCD/ColGLCD=glCD")
    var fldid = [20];//(e.g:"ColSLCD,ColGLCD")
    for (var i = 0; i <= tmpbid.length - 1; i++) {
        var tmpvlas = tmpbid[i].split("=");
        fldid[i] = tmpvlas[0];
        if (tmpvlas.length > 2) {
            if (i == 0) {
                hpnlIndexseq = tmpvlas[2];
                fldIdseq = tmpvlas[0];
            }
            else {

                hpnlIndexseq += '/' + tmpvlas[2];
                fldIdseq += '/' + tmpvlas[0];
            }
        }
    }
    if (keyType == "mousedown" || keyName == "F2") {
        $("#WaitingMode").show()
    }
    if (keyType != "mousedown" && keyName != "F2" && value == "") {
        ClearAllTextBoxes(fldid.join());
    }
    else {
        var Data = "";
        if (formdata == "Y") {
            Data = $('form').serialize() + "&val=" + value + "&Code=" + dependfldIds;
        }
        else {
            Data = "&val=" + value + "&Code=" + dependfldIds;
        }
        $.ajax({
            type: 'POST',
            url: urlstring,
            //data: "&val=" + value + "&Code=" + dependfldIds,
            data: Data,
            success: function (result) {
                var MSG = result.indexOf('#helpDIV');
                if (MSG >= 0) {
                    if (keyType != "mousedown") {
                        ClearAllTextBoxes(fldid.join());
                    }
                    $('#SearchFldValue').val(hlpfield);
                    $('#helpDIV').html(result);
                    $('#ReferanceFieldID').val(fldIdseq);
                    $('#ReferanceColumn').val(hpnlIndexseq);
                    $('#helpDIV_Header').html(caption);
                }
                else {
                    var MSG = result.indexOf(String.fromCharCode(181));
                    if (MSG >= 0) {
                        for (var i = 0; i <= tmpbid.length - 1; i++) {
                            var fld = tmpbid[i].split("=");
                            $("#" + fld[0]).val(returncolvalue(result, fld[1]));
                        }
                        hlpblurval = $("#" + hlpfield).val();
                    }
                    else {
                        $('#helpDIV').html("");
                        msgInfo("" + result + " !");
                        ClearAllTextBoxes(fldid.join());
                        message_value = hlpfield;
                    }
                }
                //if (keyName == "F2") {
                //    $("#" + hlpfield).attr("onblur", blurvalue);
                //}
                $("#WaitingMode").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError(XMLHttpRequest.responseText);
                $("body span h1").remove(); $("#msgbody_error style").remove();
            }
        });
    }
}
function RoundOff(num, decimals) {
    var sign = num >= 0 ? 1 : -1;
    return (Math.round((num * Math.pow(10, decimals)) + (sign * 0.001)) / Math.pow(10, decimals)).toFixed(decimals);
}
function retStr(val) {
    var rtval = "";
    if (val == null || val == "") rtval = "";
    else rtval = String(val);
    return rtval;
}
function retInt(val) {
    if (val == null || retStr(val) == "") return 0;
    else return parseInt(val);
}
function retFloat(val) {
    if (val == null || retStr(val) == "") return 0;
    else return parseFloat(val);
}
function scrollToEnd(Id) {
    var chatList = document.getElementById(Id);
    chatList.scrollTop = chatList.scrollHeight;
}