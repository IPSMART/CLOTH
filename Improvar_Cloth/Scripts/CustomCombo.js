function cstableSelect(id, textindex, valindex, lastindex, nameid) {
    debugger;
    textid = nameid + "text";
    valid = nameid + "value";
    unselid = nameid + "unselvalue";
    var totalRow = $('#' + id + ' tr:visible');
    var hh1 = $("#" + textid).val();
    var ff = $("#" + valid).val();
    for (var i = 1; i <= totalRow.length - 1; i++) {
        var textcontaint = totalRow[i].cells[textindex].innerHTML;
        var valcontaint = totalRow[i].cells[valindex].innerHTML;
        totalRow[i].cells[lastindex].childNodes[0].className = "glyphicon glyphicon-ok";

        if (ff.length > 0) {
            ff = ff + "," + valcontaint;
            hh1 = hh1 + "*" + textcontaint;
        }
        else {
            ff = ff + valcontaint;
            hh1 = hh1 + textcontaint;
        }
    }
    $("#" + textid).val(hh1);
    $('#' + textid).attr('title', hh1);
    $("#" + valid).val(ff);
    if (ff.length <= 0) {
        $("#" + valid).removeAttr("name");
        $("#" + textid).removeAttr("name");
    }
    else {
        $("#" + valid).attr('name', valid);
        $("#" + textid).attr('name', textid);
    }
    csupdtunsel(id, textindex, valindex, lastindex, nameid);
}

function cstableDeselect(id, textindex, valindex, lastindex, nameid) {
    debugger;
    textid = nameid + "text";
    valid = nameid + "value";
    unselid = nameid + "unselvalue";
    var totalRow = $('#' + id + ' tr:visible');
    var hh1 = $("#" + textid).val().split('*');
    var ff = $("#" + valid).val().split(',');
    for (var i = 1; i <= totalRow.length - 1; i++) {
        var hh = totalRow[i].cells[lastindex].childNodes[0].className;
        var textcontaint = totalRow[i].cells[textindex].innerHTML;
        var valcontaint = totalRow[i].cells[valindex].innerHTML;
        totalRow[i].cells[lastindex].childNodes[0].className = "";
        for (var j = ff.length - 1; j >= 0; j--) {
            if (ff[j] === valcontaint) {
                ff.splice(j, 1);
                hh1.splice(j, 1);
            }
        }
    }
    $("#" + textid).val(hh1.join("*"));
    $('#' + textid).attr('title', hh1.join("*"));
    var ff1 = ff.join(",");
    $("#" + valid).val(ff1);
    if (ff1.length <= 0) {
        $("#" + valid).removeAttr("name");
        $("#" + textid).removeAttr("name");
    }
    else {
        $("#" + valid).attr('name', valid);
        $("#" + textid).attr('name', textid);
    }
    csupdtunsel(id, textindex, valindex, lastindex, nameid);
}

function rowtoggle(text, val, last, id, nameid) {
    debugger;
    textid = nameid + "text";
    valid = nameid + "value";
    unselid = nameid + "unselvalue";

    var gg = $(id).find('td');
    var hh = gg[last].childNodes[0].className;
    var textcontaint = gg[text].innerHTML;
    var valcontaint = gg[val].innerHTML;
    //var unselrow = ffunsel.findIndex(function(valcontaint),thisvalue);
    if (hh == "") {
        var hh1 = $("#" + textid).val();
        if (hh1.length > 0) {
            hh1 = hh1 + "*" + textcontaint;
        }
        else {
            hh1 = hh1 + textcontaint;
        }
        $("#" + textid).val(hh1);
        $('#' + textid).attr('title', hh1);
        var ff = $("#" + valid).val();
        if (ff.length > 0) {
            ff = ff + "," + valcontaint;
        }
        else {
            ff = ff + valcontaint;
        }
        $("#" + valid).val(ff);
        if (ff.length <= 0) {
            $("#" + valid).removeAttr("name");
            $("#" + textid).removeAttr("name");
        }
        else {
            $("#" + valid).attr('name', valid);
            $("#" + textid).attr('name', textid);
        }
        gg[last].childNodes[0].className = "glyphicon glyphicon-ok";

        var ffunsel = $("#" + unselid).val().split(',');
        for (var i = ffunsel.length - 1; i >= 0; i--) {
            if (ffunsel[i] === valcontaint) {
                ffunsel.splice(i, 1);
                break;
            }
        }
        var ff1 = ffunsel.join(",");
        $("#" + unselid).val(ff1);
    }
    else {
        var hh1 = $("#" + textid).val().split('*');
        var ff = $("#" + valid).val().split(',');
        for (var i = ff.length - 1; i >= 0; i--) {
            if (ff[i] === valcontaint) {
                ff.splice(i, 1);
                hh1.splice(i, 1);
            }
        }
        $("#" + textid).val(hh1.join("*"));
        $('#' + textid).attr('title', hh1.join("*"));
        var ff11 = ff.join(",");
        $("#" + valid).val(ff11);
        if (ff11.length <= 0) {
            $("#" + valid).removeAttr("name");
            $("#" + textid).removeAttr("name");
        }
        else {
            $("#" + valid).attr('name', valid);
            $("#" + textid).attr('name', textid);
        }
        gg[last].childNodes[0].className = "";

        var ffunsel = $("#" + unselid).val();
        if (ffunsel.length > 0) {
            ffunsel = ffunsel + "," + valcontaint;
        }
        else {
            ffunsel = ffunsel + valcontaint;
        }
        if (ffunsel.length <= 0)
            $("#" + unselid).val(ffunsel);
    }
    //var ffunsel = $("#" + unselid).val();
    //debugger;
    //if (ffunsel.length > 0)
    //{
    //    ffunsel = ffunsel + "," + valcontaint;
    //}
    //else {
    //    ffunsel = ffunsel + valcontaint;
    //}
    if ($("#" + valid).val() == "") {
        $("#" + unselid).val("");
    }
    $("#" + unselid).val(ffunsel);
    if (ffunsel.length <= 0) {
        $("#" + unselid).removeAttr("name");
    }
    else {
        $("#" + unselid).attr('name', unselid);
    }
    $("#" + nameid + "src").val("");
    $("#" + nameid + "src").focus();
}

function csupdtunsel(id, textindex, valindex, lastindex, nameid) {
    debugger;
    textid = nameid + "text";
    valid = nameid + "value";
    unselid = nameid + "unselvalue";
    var totalRow = $('#' + id + ' tr');
    $("#" + unselid).val("");
    var ff = "";
    for (var i = 1; i <= totalRow.length - 1; i++) {
        var hh = totalRow[i].cells[lastindex].childNodes[0].className;
        var valcontaint = totalRow[i].cells[valindex].innerHTML;
        if (hh == "") {
            if (ff.length > 0) {
                ff = ff + "," + valcontaint;
            }
            else {
                ff = ff + valcontaint;
            }
        }
    }
    debugger;
    if ($("#" + valid).val() == "") {
        ff = "";
    }
    $("#" + unselid).val(ff);
    if (ff.length <= 0) {
        $("#" + unselid).removeAttr("name");
    }
    else {
        $("#" + unselid).attr('name', unselid);
    }
    $("#" + nameid + "src").val("");
    $("#" + nameid + "src").focus();
}

function filterCSTABLETable(event, id) {
    evCompat = event ? event : window.event;
    if (evCompat.keyCode == 40) {
        var totalRow = $('#' + id + ' tr:visible');
        var trow = totalRow[1];
        trow.focus();
    }
    else {
        var filter = event.target.value.toLowerCase();
        var rows = document.querySelector("#" + id + " tbody").rows;
        for (var i = 0; i <= rows.length - 1; i++) {
            var flag = 0;
            for (var x = 0; x <= rows[i].cells.length - 1; x++) {
                var firstCol = rows[i].cells[x].innerHTML.toLowerCase();
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
}

function csTableOnOff(id, id1) {
    $("#" + id).toggle();
    $("#" + id1).focus();
    $("#csTableid").val(id);
}

function SortableColumncsTable(tableNM)//Avijit
{
    var f_sl = 1;
    var lastclick = -1;
    $("#" + tableNM + " thead th").each(function () {
        $(this).hover(function () {
            $(this).css("cursor", "pointer");
            //$(this).css("background-color", "rgb(86, 137, 167)");
        }, function () {
            var nm = $(this).prevAll().length;
            if (nm != lastclick) {
                //$(this).css("color", "red");
            }
        });
        $(this).click(function () {
            f_sl *= -1;
            var n = $(this).prevAll().length;
            cssortTable(f_sl, n, tableNM);
            $("#" + tableNM + " thead th").each(function () {
                var nn = $(this).prevAll().length;
                if (nn == n) {
                    $(this).css("color", "rgb(232, 7, 59)");
                    lastclick = n;
                }
                else {
                    $(this).css("color", "#767777");
                }
            });
        });

    });
}
function cssortTable(f, n, tableNM) {
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
        var v = elm.cells[n].innerHTML.toLowerCase();
        if ($.isNumeric(v)) {
            v = parseInt(v, 10);
        }
        return v;
    }

    $.each(rows, function (index, row) {
        $('#' + tableNM).children('tbody').append(row);
    });
}