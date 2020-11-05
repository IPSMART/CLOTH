﻿function GetAllMtrljobcd() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var GridRowMain = $("#exampleLT > tbody > tr").length;
    var allcode = "";
    var mtrljobcd = $("#MTRLJOBCD").val();
    if (mtrljobcd != "") {
        allcode += "'" + mtrljobcd + "'";
    }
    for (j = 0; j <= GridRowMain - 1; j++) {
        if (document.getElementById("MaterialJobChk_" + j).checked == true) {
            if (allcode == "") {
                allcode += "'" + $("#MaterialJobCode_" + j).val() + "'";
            }
            else {
                allcode += ",'" + $("#MaterialJobCode_" + j).val() + "'";
            }
        }
    }
    $("#ALLMTRLJOBCD").val(allcode);

}
function GetBarnoDetails(id) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    debugger;
    if (id == "") {
        ClearBarcodeArea();
    }
    else {
        if (!emptyFieldCheck("Please Select / Enter Document Date", "DOCDT")) { return false; }
        if ($("#TAXGRPCD").val() == "") { $("#BARCODE").val(""); msgInfo("TaxGrp. Code not available.Please Select / Enter another Party Code to get TaxGrp. Code"); message_value = "SLCD"; return false; }
        if ($("#PRCCD").val() == "") { $("#BARCODE").val(""); msgInfo("Price Code not available.Please Select / Enter another Party Code to get Price Code"); message_value = "SLCD"; return false; }
        var MTRLJOBCD = $("#MTRLJOBCD").val();
        var PARTCD = $("#PARTCD").val();
        var docdt = $("#DOCDT").val();
        var taxgrpcd = $("#TAXGRPCD").val();
        var gocd = $("#GOCD").val();
        var prccd = $("#PRCCD").val();
        var allmtrljobcd = $("#ALLMTRLJOBCD").val();
        var code = MTRLJOBCD + String.fromCharCode(181) + PARTCD + String.fromCharCode(181) + docdt + String.fromCharCode(181) + taxgrpcd + String.fromCharCode(181) + gocd + String.fromCharCode(181) + prccd + String.fromCharCode(181) + allmtrljobcd;
        $.ajax({
            type: 'POST',
            beforesend: $("#WaitingMode").show(),
            url: $("#UrlBarnoDetails").val(),//"@Url.Action("GetBarCodeDetails", PageControllerName)",
            data: "&val=" + id + "&Code=" + code,
            success: function (result) {
                var MSG = result.indexOf('#helpDIV');
                if (MSG >= 0) {
                    ClearAllTextBoxes("BARCODE,MTRLJOBCD,PARTCD,ITCD");
                    $('#SearchFldValue').val('BARCODE');
                    $('#helpDIV').html(result);
                    $('#ReferanceFieldID').val('BARCODE/MTRLJOBCD/PARTCD');
                    $('#ReferanceColumn').val('0/2/8');
                    $('#helpDIV_Header').html('Barno Details');
                }
                else {
                    var MSG = result.indexOf(String.fromCharCode(181));
                    if (MSG >= 0) {
                        FillBarcodeArea(result);
                    }
                    else {
                        $('#helpDIV').html("");
                        msgInfo("" + result + " !");
                        ClearAllTextBoxes("BARCODE,MTRLJOBCD,PARTCD,ITCD");
                        message_value = "BARCODE";
                    }
                }
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
function Add_BarCodeRow() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    $.ajax({
        type: 'POST',
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlAddBarCodeRow").val(),//"@Url.Action("FillBarCodeData", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            //$("#partialdivBarCodeTab").animate({ marginTop: '-10px' }, 50);
            $("#partialdivBarCodeTab").html(result);
            CalculateTotal_Barno();
            $("#WaitingMode").hide();
            ClearBarcodeArea();
            //@*if (MENU_PARA == "PB") {
            //    $("#BALENO").val("");
            //}
            //$("#DISCTYPE").val("P");
            //$("#TDDISCTYPE").val("P");
            //$("#SCMDISCTYPE").val("P");*@
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });

}
function FillBarcodeArea(str, Table, i) {
    var DefaultAction = $("#DefaultAction").val();
    var MENU_PARA = $("#MENU_PARA").val();
    if (DefaultAction == "V") return true;
    if (Table == "COPYLROW") {
        if (event.key != "F8") {
            return true;
        }
        else {
            i = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length - 1;
        }
    }
    if (str != "") {
        $("#BARCODE").val(returncolvalue(str, "BARNO"));
        $("#TXNSLNO").val(returncolvalue(str, "TXNSLNO"));
        $("#ITGRPCD").val(returncolvalue(str, "ITGRPCD"));
        $("#ITGRPNM").val(returncolvalue(str, "ITGRPNM"));
        $("#MTRLJOBCD").val(returncolvalue(str, "MTRLJOBCD"));
        $("#MTRLJOBNM").val(returncolvalue(str, "MTRLJOBNM"));
        $("#MTBARCODE").val(returncolvalue(str, "MTBARCODE"));
        $("#ITCD").val(returncolvalue(str, "ITCD"));
        $("#ITSTYLE").val(returncolvalue(str, "STYLENO") + "" + returncolvalue(str, "ITNM"));
        $("#STYLENO").val(returncolvalue(str, "STYLENO"));
        //$("#STKTYPE").val(returncolvalue(str, "STKTYPE"));
        $("#PARTCD").val(returncolvalue(str, "PARTCD"));
        $("#PARTNM").val(returncolvalue(str, "PARTNM"));
        $("#PRTBARCODE").val(returncolvalue(str, "PRTBARCODE"));
        $("#COLRCD").val(returncolvalue(str, "COLRCD"));
        $("#COLRNM").val(returncolvalue(str, "COLRNM"));
        $("#CLRBARCODE").val(returncolvalue(str, "CLRBARCODE"));
        $("#SIZECD").val(returncolvalue(str, "SIZECD"));
        $("#SIZENM").val(returncolvalue(str, "SIZENM"));
        $("#SZBARCODE").val(returncolvalue(str, "SZBARCODE"));
        $("#BALSTOCK").val(returncolvalue(str, "BALQNTY"));
        $("#QNTY").val(returncolvalue(str, "QNTY"));
        $("#UOM").val(returncolvalue(str, "uomcd"));
        $("#NOS").val(returncolvalue(str, "NOS"));
        $("#FLAGMTR").val(returncolvalue(str, "FLAGMTR"));
        var RATE = returncolvalue(str, "RATE");
        var PRODGRPGSTPER = returncolvalue(str, "PRODGRPGSTPER");
        var GSTPERstr = retGstPerstr(PRODGRPGSTPER, RATE);
        var GSTPERarr = GSTPERstr.split(','); var GSTPER = 0;
        $.each(GSTPERarr, function () { GSTPER += parseFloat(this) || 0; });
        debugger;
        $("#RATE").val(RATE);
        $("#GSTPER").val(GSTPER);
        $("#PRODGRPGSTPER").val(PRODGRPGSTPER);
        $("#DISCRATE").val(returncolvalue(str, "DISCRATE"));
        //$("#DISCTYPE").val(returncolvalue(str, "DISCTYPE"));
        $("#HSNCODE").val(returncolvalue(str, "HSNCODE"));

        $("#SHADE").val(returncolvalue(str, "SHADE"));
        if (MENU_PARA == "PB") {
            $("#BALENO").val(returncolvalue(str, "BALENO"));
            $("#OURDESIGN").val(returncolvalue(str, "OURDESIGN"));
            $("#PDESIGN").val(returncolvalue(str, "PDESIGN"));
            $("#WPPRICEGEN").val(returncolvalue(str, "WPPRICEGEN"));
            $("#RPPRICEGEN").val(returncolvalue(str, "RPPRICEGEN"));
        }
        //$("#TDDISCTYPE").val(returncolvalue(str, "TDDISCTYPE"));
        $("#TDDISCRATE").val(returncolvalue(str, "TDDISCRATE"));
        //$("#SCMDISCTYPE").val(returncolvalue(str, "SCMDISCTYPE"));
        $("#SCMDISCRATE").val(returncolvalue(str, "SCMDISCRATE"));
        $("#LOCABIN").val(returncolvalue(str, "LOCABIN"));
        $("#GLCD").val(returncolvalue(str, "GLCD"));
        $("#BARGENTYPETEMP").val(returncolvalue(str, "BARGENTYPE"));
        $("#NEGSTOCK").val(returncolvalue(str, "NEGSTOCK"));
    }
    else {
        var FieldidStarting = "";
        FieldidStarting = "#B_";
        $("#SLNO").val($(FieldidStarting + "SLNO_" + i).val());
        if (Table != "COPYLROW") {
            $("#TXNSLNO").val($(FieldidStarting + "TXNSLNO_" + i).val());
        }
        $("#STKNAME").val($(FieldidStarting + "STKNAME_" + i).val());
        if (Table != "COPYLROW") {
            $("#QNTY").val($(FieldidStarting + "QNTY_" + i).val());
            $("#NOS").val($(FieldidStarting + "NOS_" + i).val());
        }
        $("#DISCRATE").val($(FieldidStarting + "DISCRATE_" + i).val());
        $("#DISCTYPE").val($(FieldidStarting + "DISCTYPE_" + i).val());
        $("#TDDISCTYPE").val($(FieldidStarting + "TDDISCTYPE_" + i).val());
        $("#TDDISCRATE").val($(FieldidStarting + "TDDISCRATE_" + i).val());
        $("#SCMDISCTYPE").val($(FieldidStarting + "SCMDISCTYPE_" + i).val());
        $("#SCMDISCRATE").val($(FieldidStarting + "SCMDISCRATE_" + i).val());
        if ((Table != "COPYLROW") && (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "PB")) {
            $("#ORDDOCNO").val($(FieldidStarting + "ORDDOCNO_" + i).val());
            $("#ORDDOCDT").val($(FieldidStarting + "ORDDOCDT_" + i).val());
            $("#ORDAUTONO").val($(FieldidStarting + "ORDAUTONO_" + i).val());
            $("#ORDSLNO").val($(FieldidStarting + "ORDSLNO_" + i).val());
        }
        $("#BALSTOCK").val($(FieldidStarting + "BALSTOCK_" + i).val());
        $("#NEGSTOCK").val($(FieldidStarting + "NEGSTOCK_" + i).val());

        $("#BARCODE").val($(FieldidStarting + "BARNO_" + i).val());
        $("#ITGRPCD").val($(FieldidStarting + "ITGRPCD_" + i).val());
        $("#ITGRPNM").val($(FieldidStarting + "ITGRPNM_" + i).val());
        $("#MTRLJOBCD").val($(FieldidStarting + "MTRLJOBCD_" + i).val());
        $("#MTRLJOBNM").val($(FieldidStarting + "MTRLJOBNM_" + i).val());
        $("#MTBARCODE").val($(FieldidStarting + "MTBARCODE_" + i).val());
        $("#ITCD").val($(FieldidStarting + "ITCD_" + i).val());
        $("#ITSTYLE").val($(FieldidStarting + "ITSTYLE_" + i).val());
        $("#STYLENO").val($(FieldidStarting + "STYLENO_" + i).val());
        $("#STKTYPE").val($(FieldidStarting + "STKTYPE_" + i).val());
        $("#PARTCD").val($(FieldidStarting + "PARTCD_" + i).val());
        $("#PRTBARCODE").val($(FieldidStarting + "PRTBARCODE_" + i).val());
        $("#COLRCD").val($(FieldidStarting + "COLRCD_" + i).val());
        $("#COLRNM").val($(FieldidStarting + "COLRNM_" + i).val());
        $("#CLRBARCODE").val($(FieldidStarting + "CLRBARCODE_" + i).val());
        $("#SIZECD").val($(FieldidStarting + "SIZECD_" + i).val());
        $("#SIZENM").val($(FieldidStarting + "SIZENM_" + i).val());
        $("#SZBARCODE").val($(FieldidStarting + "SZBARCODE_" + i).val());
        $("#FLAGMTR").val($(FieldidStarting + "FLAGMTR_" + i).val());
        $("#RATE").val($(FieldidStarting + "RATE_" + i).val());
        $("#HSNCODE").val($(FieldidStarting + "HSNCODE_" + i).val());
        $("#GSTPER").val($(FieldidStarting + "GSTPER_" + i).val());
        $("#PRODGRPGSTPER").val($(FieldidStarting + "PRODGRPGSTPER_" + i).val());
        $("#SHADE").val($(FieldidStarting + "SHADE_" + i).val());
        $("#BARGENTYPETEMP").val($(FieldidStarting + "BARGENTYPE_" + i).val());
        if (MENU_PARA == "PB") {
            $("#BALENO").val($(FieldidStarting + "BALENO_" + i).val());
            $("#OURDESIGN").val($(FieldidStarting + "OURDESIGN_" + i).val());
            $("#PDESIGN").val($(FieldidStarting + "PDESIGN_" + i).val());
            $("#WPPRICEGEN").val($(FieldidStarting + "WPPRICEGEN_" + i).val());
            $("#RPPRICEGEN").val($(FieldidStarting + "RPPRICEGEN_" + i).val());
        }
        $("#LOCABIN").val($(FieldidStarting + "LOCABIN_" + i).val());
        $("#UOM").val($(FieldidStarting + "UOM_" + i).val());
        $("#GLCD").val($(FieldidStarting + "GLCD_" + i).val());
    }
    if (Table == "_T_SALE_PRODUCT_GRID") {
        $("#AddRow_Barcode").hide();
        $("#UpdateRow_Barcode").prop("value", "Update Row [" + $(FieldidStarting + "SLNO_" + i).val() + "] (Alt+W)");
        $("#UpdateRow_Barcode").show();
    }
    changeBARGENTYPE();
}
function changeBARGENTYPE() {
    debugger;
    var BARGENTYPE = $("#BARGENTYPE").val();
    if (BARGENTYPE == "C") {
        $("#divImageUpload").hide();
    }
    else if (BARGENTYPE == "E") {
        $("#divImageUpload").show();
    }
    return 0;
}

function UpdateBarCodeRow() {
    var DefaultAction = $("#DefaultAction").val();
    var MENU_PARA = $("#MENU_PARA").val();
    if (DefaultAction == "V") return true;
    debugger;
    if ($("#ITGRPCD").val() == "") {
        msgInfo("Please enter/select Item Group Code !");
        message_value = "ITGRPCD";
        return false;
    }
    if ($("#MTRLJOBCD").val() == "") {
        msgInfo("Please enter Material Job Code !");
        message_value = "MTRLJOBCD";
        return false;
    }
    if ($("#ITCD").val() == "") {
        msgInfo("Please enter Item Code !");
        message_value = "ITCD";
        return false;
    }
    if ($("#QNTY").val() == "0") {
        msgInfo("Quantity should not be zero(0) !");
        message_value = "QNTY";
        return false;
    }
    if (MENU_PARA != "SR" && MENU_PARA != "SBCMR" && MENU_PARA != "PB") {
        var NEGSTOCK = $("#NEGSTOCK").val();
        var BALSTOCK = $("#BALSTOCK").val();
        if (BALSTOCK == "") { BALSTOCK = parseFloat(0); } else { BALSTOCK = parseFloat(BALSTOCK); }
        var QNTY = $("#QNTY").val();
        if (QNTY == "") { QNTY = parseFloat(0); } else { QNTY = parseFloat(QNTY); }
        var balancestock = BALSTOCK - QNTY;
        if (balancestock < 0) {
            if (NEGSTOCK != "Y") {
                msgInfo("Quantity should not be grater than Stock !");
                message_value = "QNTY";
                return false;
            }
        }
    }
    var TXNSLNO = "";
    if ($("#TXNSLNO").val() == "" || $("#TXNSLNO").val() == "0") {
        var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
        if (GridRowMain == 0) {
            TXNSLNO = 1;
        }
        else {
            var allslno = [parseInt(GridRowMain)];
            for (j = 0; j <= GridRowMain - 1; j++) {
                allslno[j] = parseInt($("#B_TXNSLNO_" + j).val());
            }
            TXNSLNO = Math.max.apply(Math, allslno);
            TXNSLNO++;
        }
    }
    else {
        TXNSLNO = parseInt($("#TXNSLNO").val());
    }
    var FLAGMTR = $("#FLAGMTR").val();
    var QNTY = $("#QNTY").val();

    if (FLAGMTR != "") {
        var flgmtr = FLAGMTR;
        if (flgmtr == "") { flgmtr = parseFloat(0); } else { flgmtr = parseFloat(flgmtr); }
        var qnty = QNTY;
        if (qnty == "") { qnty = parseFloat(0); } else { qnty = parseFloat(qnty); }
        if (flgmtr > qnty) {
            msgInfo("FLAGMTR (" + flgmtr + ") should be less than Quantity (" + qnty + ") !");
            message_value = "FLAGMTR";
            return false;
        }
    }
    if ($("#HSNCODE").val() == "") {
        msgInfo("Please enter HSN Code !");
        message_value = "HSNCODE";
        return false;
    }
    var SLNO = $("#SLNO").val();
    var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    for (j = 0; j <= GridRowMain - 1; j++) {
        if ($("#B_SLNO_" + j).val() == SLNO) {
            $("#B_BARNO_" + j).val($("#BARCODE").val());
            $("#B_TXNSLNO_" + j).val(TXNSLNO);
            $("#B_ITGRPCD_" + j).val($("#ITGRPCD").val());
            $("#B_ITGRPNM_" + j).val($("#ITGRPNM").val());
            $("#B_MTRLJOBCD_" + j).val($("#MTRLJOBCD").val());
            $("#B_MTRLJOBNM_" + j).val($("#MTRLJOBNM").val());
            $("#B_MTBARCODE_" + j).val($("#MTBARCODE").val());
            $("#B_ITCD_" + j).val($("#ITCD").val());
            $("#B_ITSTYLE_" + j).val($("#ITSTYLE").val());
            $("#B_STYLENO_" + j).val($("#STYLENO").val());
            $("#B_STKTYPE_" + j).val($("#STKTYPE").val());
            $("#B_PARTCD_" + j).val($("#PARTCD").val());
            $("#B_PARTNM_" + j).val($("#PARTNM").val());
            $("#B_PRTBARCODE_" + j).val($("#PRTBARCODE").val());
            $("#B_COLRCD_" + j).val($("#COLRCD").val());
            $("#B_COLRNM_" + j).val($("#COLRNM").val());
            $("#B_CLRBARCODE_" + j).val($("#CLRBARCODE").val());
            $("#B_SIZECD_" + j).val($("#SIZECD").val());
            $("#B_SIZENM_" + j).val($("#SIZENM").val());
            $("#B_SZBARCODE_" + j).val($("#SZBARCODE").val());
            $("#B_BALSTOCK_" + j).val($("#BALSTOCK").val());
            $("#B_QNTY_" + j).val($("#QNTY").val());
            $("#B_BALSTOCK_" + j).val($("#BALSTOCK").val());
            $("#B_NEGSTOCK_" + j).val($("#NEGSTOCK").val());
            $("#B_UOM_" + j).val($("#UOM").val());
            $("#B_NOS_" + j).val($("#NOS").val());
            $("#B_FLAGMTR_" + j).val($("#FLAGMTR").val());
            $("#B_RATE_" + j).val($("#RATE").val());
            $("#B_DISCRATE_" + j).val($("#DISCRATE").val());
            $("#B_DISCTYPE_" + j).val($("#DISCTYPE").val());
            $("#B_HSNCODE_" + j).val($("#HSNCODE").val());
            $("#B_GSTPER_" + j).val($("#GSTPER").val());
            $("#B_PRODGRPGSTPER_" + j).val($("#PRODGRPGSTPER").val());
            $("#B_SHADE_" + j).val($("#SHADE").val());
            if (MENU_PARA == "PB") {
                $("#B_BALENO_" + j).val($("#BALENO").val());
                $("#B_OURDESIGN_" + j).val($("#OURDESIGN").val());
                $("#B_PDESIGN_" + j).val($("#PDESIGN").val());
                $("#B_WPPRICEGEN_" + j).val($("#WPPRICEGEN").val());
                $("#B_RPPRICEGEN_" + j).val($("#RPPRICEGEN").val());
            }
            $("#B_TDDISCTYPE_" + j).val($("#TDDISCTYPE").val());
            $("#B_TDDISCRATE_" + j).val($("#TDDISCRATE").val());
            $("#B_SCMDISCTYPE_" + j).val($("#SCMDISCTYPE").val());
            $("#B_SCMDISCRATE_" + j).val($("#SCMDISCRATE").val());
            $("#B_LOCABIN_" + j).val($("#LOCABIN").val());
            var DISCTYPE = $("#DISCTYPE").val() == "P" ? "%" : $("#DISCTYPE").val() == "N" ? "Nos" : $("#DISCTYPE").val() == "Q" ? "Qnty" : "Fixed";
            var TDDISCTYPE = $("#TDDISCTYPE").val() == "P" ? "%" : $("#TDDISCTYPE").val() == "N" ? "Nos" : $("#TDDISCTYPE").val() == "Q" ? "Qnty" : "Fixed";
            var SCMDISCTYPE = $("#SCMDISCTYPE").val() == "P" ? "%" : $("#SCMDISCTYPE").val() == "N" ? "Nos" : $("#SCMDISCTYPE").val() == "Q" ? "Qnty" : "Fixed";
            $("#B_DISCTYPE_DESC_" + j).val(DISCTYPE);
            $("#B_TDDISCTYPE_DESC_" + j).val(TDDISCTYPE);
            $("#B_SCMDISCTYPE_DESC_" + j).val(SCMDISCTYPE);
            $("#B_GLCD_" + j).val($("#GLCD").val());
            if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "PB") {
                $("#B_ORDDOCNO_" + j).val($("#ORDDOCNO").val());
                $("#B_ORDAUTONO_" + j).val($("#ORDAUTONO").val());
                $("#B_ORDSLNO_" + j).val($("#ORDSLNO").val());
                $("#B_ORDDOCDT_" + j).val($("#ORDDOCDT").val());
            }
            if (MENU_PARA == "PB") {
                RateUpdate(j);

            }
        }
    }
    CalculateTotal_Barno();
    ClearBarcodeArea();
    //@*if (MENU_PARA == "PB") {
    //    $("#BALENO").val("");
    //}
    //$("#DISCTYPE").val("P");
    //$("#TDDISCTYPE").val("P");
    //$("#SCMDISCTYPE").val("P");*@
    $("#AddRow_Barcode").show();
    $("#UpdateRow_Barcode").hide();
    if (MENU_PARA == "PB") {
        $("#TXNSLNO").focus();
    } else {
        $("#BARCODE").focus();
    }
    $("#bardatachng").val("Y");
}

function ClearBarcodeArea(TAG) {
    var DefaultAction = $("#DefaultAction").val();
    var MENU_PARA = $("#MENU_PARA").val();
    if (DefaultAction == "V") return true;
    ClearAllTextBoxes("BARCODE,TXNSLNO,ITGRPCD,ITGRPNM,MTRLJOBCD,MTRLJOBNM,MTBARCODE,ITCD,ITSTYLE,STYLENO,STKTYPE,PARTCD,PARTNM,PRTBARCODE,COLRCD,COLRNM,CLRBARCODE,SIZECD,SIZENM,SZBARCODE,BALSTOCK,QNTY,UOM,GLCD,NOS,FLAGMTR,RATE,DISCRATE,HSNCODE,GSTPER,PRODGRPGSTPER,SHADE,TDDISCRATE,SCMDISCRATE,LOCABIN,BARGENTYPETEMP,NEGSTOCK");
    if (MENU_PARA == "PB") {
        ClearAllTextBoxes("BALENO,OURDESIGN,PDESIGN,WPPRICEGEN,RPPRICEGEN");
    }
    if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "PB") {
        ClearAllTextBoxes("ORDDOCNO,ORDDOCDT,ORDAUTONO,ORDSLNO");
    }
    $("#STKTYPE").val("F");
    $("#DISCTYPE").val("P");
    $("#TDDISCTYPE").val("P");
    $("#SCMDISCTYPE").val("P");
    if (TAG == "Y") {
        $("#AddRow_Barcode").show();
        $("#UpdateRow_Barcode").hide();
        if (MENU_PARA == "PB") {
            $("#TXNSLNO").focus();
        } else {
            $("#BARCODE").focus();
        }
    }

}
function Fill_DetailData() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    $.ajax({
        type: 'POST',
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlFillDetailData").val(),//"@Url.Action("FillDetailData", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            //$("#partialdivBarCodeTab").animate({ marginTop: '-10px' }, 50);
            $("#partialdivDetail").html(result);
            $("li").removeClass("active").addClass("");
            $(".nav-tabs li:nth-child(3)").addClass('active');
            //below set the  child sequence
            $(".tab-content div").removeClass("active");
            $(".tab-content div:nth-child(3)").removeClass("tab-pane fade").addClass("tab-pane fade in active");
            var GridRow = $("#_T_SALE_DETAIL_GRID > tbody > tr").length;
            for (var i = 0; i <= GridRow - 1; i++) {
                CalculateAmt_Details(i);
            }
            $("#bardatachng").val("N");
            $("#WaitingMode").hide();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });

}
function UpdateBarCodeRow_FrmDet(i) {
    var MENU_PARA = $("#MENU_PARA").val();
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var TXNSLNO = $("#D_SLNO_" + i).val();
    var ITGRPCD = $("#D_ITGRPCD_" + i).val();
    var MTRLJOBCD = $("#D_MTRLJOBCD_" + i).val();
    var ITCD = $("#D_ITCD_" + i).val();
    var STKTYPE = $("#D_STKTYPE_" + i).val();
    //fill
    var RATE = $("#D_RATE_" + i).val();
    var DISCTYPE = $("#D_DISCTYPE_" + i).val();
    var DISCTYPE_DESC = $("#D_DISCTYPE_DESC_" + i).val();
    var DISCRATE = $("#D_DISCRATE_" + i).val();
    var TDDISCRATE = $("#D_TDDISCRATE_" + i).val();
    var TDDISCTYPE = $("#D_TDDISCTYPE_" + i).val();
    var SCMDISCRATE = $("#D_SCMDISCRATE_" + i).val();
    var SCMDISCTYPE = $("#D_SCMDISCTYPE_" + i).val();
    if (MENU_PARA == "PB") {
        var BALENO = $("#D_BALENO_" + i).val();
    }
    var ITREM = $("#D_ITREM_" + i).val();
    var GSTPER = (parseFloat($("#D_IGSTPER_" + i).val()) || 0) + (parseFloat($("#D_CGSTPER_" + i).val()) || 0) + (parseFloat($("#D_SGSTPER_" + i).val()) || 0);
    var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    for (j = 0; j <= GridRowMain - 1; j++) {
        if ($("#B_TXNSLNO_" + j).val() == TXNSLNO && $("#B_ITGRPCD_" + j).val() == ITGRPCD && $("#B_MTRLJOBCD_" + j).val() == MTRLJOBCD && $("#B_ITCD_" + j).val() == ITCD && $("#B_STKTYPE_" + j).val() == STKTYPE) {
            $("#B_RATE_" + j).val(RATE);
            RateUpdate(j);
            $("#B_DISCTYPE_" + j).val(DISCTYPE);
            $("#B_DISCTYPE_DESC_" + j).val(DISCTYPE_DESC);
            $("#B_DISCRATE_" + j).val(DISCRATE);
            $("#B_TDDISCRATE_" + j).val(TDDISCRATE);
            $("#B_TDDISCTYPE_" + j).val(TDDISCTYPE);
            $("#B_SCMDISCRATE_" + j).val(SCMDISCRATE);
            $("#B_SCMDISCTYPE_" + j).val(SCMDISCTYPE);
            if (MENU_PARA == "PB") {
                $("#B_BALENO_" + j).val(BALENO);
            }
            $("#B_ITREM_" + j).val(ITREM);
            $("#B_GSTPER_" + j).val(GSTPER);
        }
    }
}

function CalculateTotal_Barno() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    var T_QNTY = 0, T_NOS = 0;
    var GridRow = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var QNTY = $("#B_QNTY_" + i).val();
        if (QNTY != "") { T_QNTY = T_QNTY + parseFloat(QNTY); } else { T_QNTY = T_QNTY + parseFloat(0); }
        var NOS = $("#B_NOS_" + i).val();
        if (NOS != "") { T_NOS = T_NOS + parseFloat(NOS); } else { T_NOS = T_NOS + parseFloat(0); }

        if (MENU_PARA != "SR" && MENU_PARA != "SBCMR" && MENU_PARA != "PB") {
            var NEGSTOCK = $("#B_NEGSTOCK_" + i).val();
            var BALSTOCK = $("#B_BALSTOCK_" + i).val();
            if (BALSTOCK == "") { BALSTOCK = parseFloat(0); } else { BALSTOCK = parseFloat(BALSTOCK); }
            if (QNTY == "") { QNTY = parseFloat(0); } else { QNTY = parseFloat(QNTY); }
            var balancestock = BALSTOCK - QNTY;
            if (balancestock < 0) {
                if (NEGSTOCK != "Y") {
                    msgInfo("Quantity should not be grater than Stock !");
                    message_value = "B_QNTY_" + i;
                    return false;
                }

            }
        }
    }
    $("#B_T_QNTY").val(parseFloat(T_QNTY).toFixed(2));
    $("#B_T_NOS").val(parseFloat(T_NOS).toFixed(0));

}
function CalculateAmt_Details(i) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var BLQNTY = $("#D_BLQNTY_" + i).val();
    if (BLQNTY != "") { BLQNTY = parseFloat(BLQNTY); } else { BLQNTY = parseFloat(0); }

    var QNTY = $("#D_QNTY_" + i).val();
    if (QNTY != "") { QNTY = parseFloat(QNTY); } else { QNTY = parseFloat(0); }

    var NOS = $("#D_NOS_" + i).val();
    if (NOS != "") { NOS = parseFloat(NOS); } else { NOS = parseFloat(0); }

    var FLAGMTR = $("#D_FLAGMTR_" + i).val();
    if (FLAGMTR != "") { FLAGMTR = parseFloat(FLAGMTR); } else { FLAGMTR = parseFloat(0); }

    var RATE = $("#D_RATE_" + i).val();
    if (RATE != "") { RATE = parseFloat(RATE); } else { RATE = parseFloat(0); }

    var IGSTPER = $("#D_IGSTPER_" + i).val();
    if (IGSTPER != "") { IGSTPER = parseFloat(IGSTPER); } else { IGSTPER = parseFloat(0); }

    var CGSTPER = $("#D_CGSTPER_" + i).val();
    if (CGSTPER != "") { CGSTPER = parseFloat(CGSTPER); } else { CGSTPER = parseFloat(0); }

    var SGSTPER = $("#D_SGSTPER_" + i).val();
    if (SGSTPER != "") { SGSTPER = parseFloat(SGSTPER); } else { SGSTPER = parseFloat(0); }

    var CESSPER = $("#D_CESSPER_" + i).val();
    if (CESSPER != "") { CESSPER = parseFloat(CESSPER); } else { CESSPER = parseFloat(0); }

    var DISCTYPE = $("#D_DISCTYPE_" + i).val();
    var DISCRATE = $("#D_DISCRATE_" + i).val();
    if (DISCRATE != "") { DISCRATE = parseFloat(DISCRATE); } else { DISCRATE = parseFloat(0); }

    var TDDISCTYPE = $("#D_TDDISCTYPE_" + i).val();
    var TDDISCRATE = $("#D_TDDISCRATE_" + i).val();
    if (TDDISCRATE != "") { TDDISCRATE = parseFloat(TDDISCRATE); } else { TDDISCRATE = parseFloat(0); }

    var SCMDISCTYPE = $("#D_SCMDISCTYPE_" + i).val();
    var SCMDISCRATE = $("#D_SCMDISCRATE_" + i).val();
    if (SCMDISCRATE != "") { SCMDISCRATE = parseFloat(SCMDISCRATE); } else { SCMDISCRATE = parseFloat(0); }


    //AMOUNT CALCULATION
    var amount = 0;
    if (BLQNTY == 0) {
        amount = (parseFloat(QNTY) - parseFloat(FLAGMTR)) * parseFloat(RATE);
    }
    else {
        amount = parseFloat(BLQNTY) * parseFloat(RATE);
    }
    amount = parseFloat(amount).toFixed(2);
    $("#D_AMT_" + i).val(amount);

    //DISCOUNT AMOUNT CALCULATION
    var DISCAMT = 0;
    if (DISCTYPE == "Q") { DISCAMT = DISCRATE * QNTY; }
    else if (DISCTYPE == "N") { DISCAMT = DISCRATE * NOS; }
    else if (DISCTYPE == "P") { DISCAMT = (amount * DISCRATE) / 100; }
    else if (DISCTYPE == "F") { DISCAMT = DISCRATE; }
    else { DISCAMT = 0; }
    DISCAMT = parseFloat(DISCAMT).toFixed(2);
    $("#D_DISCAMT_" + i).val(DISCAMT);

    //TDDISCOUNT AMOUNT CALCULATION
    var TDDISCAMT = 0;
    if (TDDISCTYPE == "Q") { TDDISCAMT = TDDISCRATE * QNTY; }
    else if (TDDISCTYPE == "N") { TDDISCAMT = TDDISCRATE * NOS; }
    else if (TDDISCTYPE == "P") { TDDISCAMT = (amount * TDDISCRATE) / 100; }
    else if (TDDISCTYPE == "F") { TDDISCAMT = TDDISCRATE; }
    else { TDDISCAMT = 0; }
    TDDISCAMT = parseFloat(TDDISCAMT).toFixed(2);
    $("#D_TDDISCAMT_" + i).val(TDDISCAMT);

    //SCMDISCOUNT AMOUNT CALCULATION
    var SCMDISCAMT = 0;
    if (SCMDISCTYPE == "Q") { SCMDISCAMT = SCMDISCRATE * QNTY; }
    else if (SCMDISCTYPE == "N") { SCMDISCAMT = SCMDISCRATE * NOS; }
    else if (SCMDISCTYPE == "P") { SCMDISCAMT = (amount * SCMDISCRATE) / 100; }
    else if (SCMDISCTYPE == "F") { SCMDISCAMT = SCMDISCRATE; }
    else { SCMDISCAMT = 0; }
    SCMDISCAMT = parseFloat(SCMDISCAMT).toFixed(2);
    $("#D_SCMDISCAMT_" + i).val(SCMDISCAMT);

    //TOTAL DISCOUNT AMOUNT CALCULATION
    var TOTDISCAMT = parseFloat(DISCAMT + TDDISCAMT + SCMDISCAMT).toFixed(2);
    $("#D_TOTDISCAMT_" + i).val(TOTDISCAMT);

    //TAXABLE CALCULATION
    var taxbleamt = parseFloat(amount) - parseFloat(TOTDISCAMT);
    taxbleamt = parseFloat(taxbleamt).toFixed(2);
    $("#D_TXBLVAL_" + i).val(taxbleamt);
    //IGST,CGST,SGST,CESS AMOUNT CALCULATION

    var IGST_AMT = 0; var CGST_AMT = 0; var SGST_AMT = 0; var CESS_AMT = 0; var chkAmt = 0;

    //IGST
    if (IGSTPER == 0 || IGSTPER == "") {
        IGSTPER = 0; IGST_AMT = 0;
    }
    else {
        IGST_AMT = parseFloat((taxbleamt * IGSTPER) / 100).toFixed(2);
        chkAmt = $("#D_IGSTAMT_" + i).val();
        if (chkAmt == "") chkAmt = 0;
        if (Math.abs(IGST_AMT - chkAmt) <= 1) IGST_AMT = chkAmt;
    }
    $("#D_IGSTAMT_" + i).val(IGST_AMT);
    //CGST
    if (CGSTPER == "" || CGSTPER == 0) {
        CGSTPER = 0; CGST_AMT = 0;
    }
    else {
        CGST_AMT = parseFloat((taxbleamt * CGSTPER) / 100).toFixed(2);
        chkAmt = $("#D_CGSTAMT_" + i).val();
        if (chkAmt == "") chkAmt = 0;
        if (Math.abs(CGST_AMT - chkAmt) <= 1) CGST_AMT = chkAmt;
    }
    $("#D_CGSTAMT_" + i).val(CGST_AMT);
    //SGST
    if (SGSTPER == "" || SGSTPER == 0) {
        SGSTPER = 0; SGST_AMT = 0;
    }
    else {
        SGST_AMT = parseFloat((taxbleamt * SGSTPER) / 100).toFixed(2);
        chkAmt = $("#D_SGSTAMT_" + i).val();
        if (chkAmt == "") chkAmt = 0;
        if (Math.abs(SGST_AMT - chkAmt) <= 1) SGST_AMT = chkAmt;
    }
    $("#D_SGSTAMT_" + i).val(SGST_AMT);

    //CESS
    if (CESSPER == "" || CESSPER == 0) {
        CESSPER = 0; CESS_AMT = 0;
    }
    else {
        CESS_AMT = parseFloat((taxbleamt * CESSPER) / 100).toFixed(2);
        chkAmt = $("#D_CESSAMT_" + i).val();
        if (chkAmt == "") chkAmt = 0;
        if (Math.abs(CESS_AMT - chkAmt) <= 1) CESS_AMT = chkAmt;
    }
    $("#D_CESSAMT_" + i).val(CESS_AMT);

    var netamt = parseFloat(parseFloat(taxbleamt) + parseFloat(IGST_AMT) + parseFloat(CGST_AMT) + parseFloat(SGST_AMT) + parseFloat(CESS_AMT)).toFixed(2);
    $("#D_NETAMT_" + i).val(netamt);
    CalculateTotal_Details();

}
function CalculateTotal_Details() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var T_NOS = 0; var T_QNTY = 0; var T_AMT = 0; var T_GROSS_AMT = 0; var T_IGST_AMT = 0; var T_CGST_AMT = 0; var T_SGST_AMT = 0; var T_CESS_AMT = 0; var T_NET_AMT = 0;

    var GridRow = $("#_T_SALE_DETAIL_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var NOS = $("#D_NOS_" + i).val();
        if (NOS != "") { T_NOS = T_NOS + parseFloat(NOS); } else { T_NOS = T_NOS + parseFloat(0); }

        var QNTY = $("#D_QNTY_" + i).val();
        if (QNTY != "") { T_QNTY = T_QNTY + parseFloat(QNTY); } else { T_QNTY = T_QNTY + parseFloat(0); }

        var AMT = $("#D_AMT_" + i).val();
        if (AMT != "") { T_AMT = T_AMT + parseFloat(AMT); } else { T_AMT = T_AMT + parseFloat(0); }

        var GROSS_AMT = $("#D_TXBLVAL_" + i).val();
        if (GROSS_AMT != "") { T_GROSS_AMT = T_GROSS_AMT + parseFloat(GROSS_AMT); } else { T_GROSS_AMT = T_GROSS_AMT + parseFloat(0); }

        var IGST_AMT = $("#D_IGSTAMT_" + i).val();
        if (IGST_AMT != "") { T_IGST_AMT = T_IGST_AMT + parseFloat(IGST_AMT); } else { T_IGST_AMT = T_IGST_AMT + parseFloat(0); }

        var CGST_AMT = $("#D_CGSTAMT_" + i).val();
        if (CGST_AMT != "") { T_CGST_AMT = T_CGST_AMT + parseFloat(CGST_AMT); } else { T_CGST_AMT = T_CGST_AMT + parseFloat(0); }

        var SGST_AMT = $("#D_SGSTAMT_" + i).val();
        if (SGST_AMT != "") { T_SGST_AMT = T_SGST_AMT + parseFloat(SGST_AMT); } else { T_SGST_AMT = T_SGST_AMT + parseFloat(0); }

        var CESS_AMT = $("#D_CESSAMT_" + i).val();
        if (CESS_AMT != "") { T_CESS_AMT = T_CESS_AMT + parseFloat(CESS_AMT); } else { T_CESS_AMT = T_CESS_AMT + parseFloat(0); }

        var NET_AMT = $("#D_NETAMT_" + i).val();
        if (NET_AMT != "") { T_NET_AMT = T_NET_AMT + parseFloat(NET_AMT); } else { T_NET_AMT = T_NET_AMT + parseFloat(0); }
    }
    var totaltax = parseFloat(T_IGST_AMT) + parseFloat(T_CGST_AMT) + parseFloat(T_SGST_AMT) + parseFloat(T_CESS_AMT);
    $("#T_NOS").val(parseFloat(T_NOS).toFixed(0));
    $("#T_QNTY").val(parseFloat(T_QNTY).toFixed(2));
    $("#T_AMT").val(parseFloat(T_AMT).toFixed(2));
    $("#T_GROSS_AMT").val(parseFloat(T_GROSS_AMT).toFixed(2));
    $("#T_IGST_AMT").val(parseFloat(T_IGST_AMT).toFixed(2));
    $("#T_CGST_AMT").val(parseFloat(T_CGST_AMT).toFixed(2));
    $("#T_SGST_AMT").val(parseFloat(T_SGST_AMT).toFixed(2));
    $("#T_CESS_AMT").val(parseFloat(T_CESS_AMT).toFixed(2));
    $("#T_NET_AMT").val(parseFloat(T_NET_AMT).toFixed(2));

    //main tab
    //$("#TOTNOS").val(parseFloat(T_NOS).toFixed(2));
    //$("#TOTQNTY").val(parseFloat(T_QNTY).toFixed(2));
    //$("#TOTTAXVAL").val(parseFloat(T_GROSS_AMT).toFixed(2));
    //$("#TOTTAX").val(parseFloat(totaltax).toFixed(2));
    BillAmountCalculate();

}

function GetGstPer(i, FieldidStarting) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    debugger;
    var prodgrpgstper = "";
    var rate = 0;
    if (FieldidStarting == "") {
        prodgrpgstper = $("#PRODGRPGSTPER").val();
        rate = $("#RATE").val();
        if (rate == "") { rate = parseFloat(0); } else { rate = parseFloat(rate); }
    }
    else {
        prodgrpgstper = $(FieldidStarting + "PRODGRPGSTPER_" + i).val();
        rate = $(FieldidStarting + "RATE_" + i).val();
        if (rate == "") { rate = parseFloat(0); } else { rate = parseFloat(rate); }
    }
    var allgst = retGstPerstr(prodgrpgstper, rate);
    if (allgst != "") {
        var str = allgst.split(',');
        if (str.length > 0) {
            var sumgst = parseFloat(parseFloat(str[0]) + parseFloat(str[1]) + parseFloat(str[2])).toFixed(2);
            if (FieldidStarting == "") {
                $("#GSTPER").val(sumgst);
            }
            else if (FieldidStarting == "#B_") {
                $(FieldidStarting + "GSTPER_" + i).val(sumgst);
            }
            else {
                $(FieldidStarting + "IGSTPER_" + i).val(parseFloat(str[0]).toFixed(2));
                $(FieldidStarting + "CGSTPER_" + i).val(parseFloat(str[1]).toFixed(2));
                $(FieldidStarting + "SGSTPER_" + i).val(parseFloat(str[2]).toFixed(2));
            }
        }
    }
    if (FieldidStarting == "#D_") {
        UpdateBarCodeRow_FrmDet(i);
        CalculateAmt_Details(i);
    }

}

function ItcdClr() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    ClearAllTextBoxes("ITCD,ITSTYLE,UOM,STYLENO,HSNCODE");
}

function AmountCalculation(i) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var A_NOS = document.getElementById("T_NOS").value;
    var B_QNTY = document.getElementById("T_QNTY").value;
    var D_GROSS_AMT = document.getElementById("T_GROSS_AMT").value;
    var E_NET_AMT = document.getElementById("T_NET_AMT").value;
    var RT = document.getElementById("AMTRATE_" + i).value;
    var IGST_PER = document.getElementById("AIGSTPER_" + i).value;
    if (IGST_PER == "") { IGST_PER = parseFloat(0); } else { IGST_PER = parseFloat(IGST_PER) }
    var CGST_PER = document.getElementById("ACGSTPER_" + i).value;
    if (CGST_PER == "") { CGST_PER = parseFloat(0); } else { CGST_PER = parseFloat(CGST_PER) }
    var SGST_PER = document.getElementById("ASGSTPER_" + i).value;
    if (SGST_PER == "") { SGST_PER = parseFloat(0); } else { SGST_PER = parseFloat(SGST_PER) }
    var CESS_PER = document.getElementById("ACESSPER_" + i).value;
    if (CESS_PER == "") { CESS_PER = parseFloat(0); } else { CESS_PER = parseFloat(CESS_PER) }
    var DUTY_PER = document.getElementById("ADUTYPER_" + i).value;
    if (DUTY_PER == "") { DUTY_PER = parseFloat(0); } else { DUTY_PER = parseFloat(DUTY_PER) }
    var CALC_TYPE = document.getElementById("CALCTYPE_" + i).value;
    var CALC_FORMULA = document.getElementById("CALCFORMULA_" + i).value;
    if (A_NOS == "") { A_NOS = parseFloat(0); }
    if (B_QNTY == "") { B_QNTY = parseFloat(0); }
    if (D_GROSS_AMT == "") { D_GROSS_AMT = parseFloat(0); }
    if (E_NET_AMT == "") { E_NET_AMT = parseFloat(0); }
    if (RT == "") { RT = parseFloat(0); }
    var AMOUNT = 0;
    if (CALC_TYPE == "F") { AMOUNT = parseFloat(RT); }
    else if (CALC_TYPE == "P") {
        if (CALC_FORMULA == "A") { AMOUNT = parseFloat(A_NOS) * parseFloat(RT); }
        else if (CALC_FORMULA == "B") { AMOUNT = parseFloat(B_QNTY) * parseFloat(RT); }
        else if (CALC_FORMULA == "D") { AMOUNT = parseFloat(D_GROSS_AMT) * parseFloat(RT); }
        else if (CALC_FORMULA == "E") { AMOUNT = parseFloat(E_NET_AMT) * parseFloat(RT); }
    }
    document.getElementById("A_AMT_" + i).value = AMOUNT;
    var IGST_AMT = 0; var CGST_AMT = 0; var SGST_AMT = 0; var CESS_AMT = 0; var DUTY_AMT = 0;

    // IGST CALCULATION
    document.getElementById("AIGSTPER_" + i).value = IGST_PER;
    IGST_AMT = (AMOUNT * IGST_PER) / 100;

    AmountChange(document.getElementById("AIGSTAMT_" + i), document.getElementById("A_AMT_" + i), document.getElementById("AIGSTPER_" + i), document.getElementById("ANETAMT_" + i),
    document.getElementById("ACGSTAMT_" + i), document.getElementById("ASGSTAMT_" + i), document.getElementById("ACESSAMT_" + i), document.getElementById("ADUTYAMT_" + i));
    document.getElementById("AIGSTAMT_" + i).value = parseFloat(IGST_AMT).toFixed(2);
    //END

    // CGST CALCULATION
    document.getElementById("ACGSTPER_" + i).value = CGST_PER;
    CGST_AMT = (AMOUNT * CGST_PER) / 100;
    AmountChange(document.getElementById("ACGSTAMT_" + i), document.getElementById("A_AMT_" + i), document.getElementById("ACGSTPER_" + i), document.getElementById("ANETAMT_" + i),
        document.getElementById("AIGSTAMT_" + i), document.getElementById("ASGSTAMT_" + i), document.getElementById("ACESSAMT_" + i), document.getElementById("ADUTYAMT_" + i));
    document.getElementById("ACGSTAMT_" + i).value = parseFloat(CGST_AMT).toFixed(2);
    //END
    // SGST CALCULATION
    document.getElementById("ASGSTPER_" + i).value = SGST_PER;
    SGST_AMT = (AMOUNT * SGST_PER) / 100;
    AmountChange(document.getElementById("ASGSTAMT_" + i), document.getElementById("A_AMT_" + i), document.getElementById("ASGSTPER_" + i), document.getElementById("ANETAMT_" + i),
        document.getElementById("AIGSTAMT_" + i), document.getElementById("ASGSTAMT_" + i), document.getElementById("ACESSAMT_" + i), document.getElementById("ADUTYAMT_" + i));
    document.getElementById("ASGSTAMT_" + i).value = parseFloat(SGST_AMT).toFixed(2);
    //END
    // CESS CALCULATION
    document.getElementById("ACESSPER_" + i).value = CESS_PER;
    CESS_AMT = (AMOUNT * CESS_PER) / 100;
    AmountChange(document.getElementById("ACESSAMT_" + i), document.getElementById("A_AMT_" + i), document.getElementById("ACESSPER_" + i), document.getElementById("ANETAMT_" + i),
        document.getElementById("AIGSTAMT_" + i), document.getElementById("ASGSTAMT_" + i), document.getElementById("ACESSAMT_" + i), document.getElementById("ADUTYAMT_" + i));
    document.getElementById("ACESSAMT_" + i).value = parseFloat(CESS_AMT).toFixed(2);
    //END
    // DUTY CALCULATION

    document.getElementById("ADUTYPER_" + i).value = DUTY_PER;
    DUTY_AMT = (AMOUNT * DUTY_PER) / 100;
    AmountChange(document.getElementById("ADUTYAMT_" + i), document.getElementById("A_AMT_" + i), document.getElementById("ADUTYPER_" + i), document.getElementById("ANETAMT_" + i),
    document.getElementById("AIGSTAMT_" + i), document.getElementById("ASGSTAMT_" + i), document.getElementById("ACESSAMT_" + i), document.getElementById("ADUTYAMT_" + i));
    document.getElementById("ADUTYAMT_" + i).value = parseFloat(DUTY_AMT).toFixed(2);
    //END

    var NET_AMT = AMOUNT + parseFloat(document.getElementById("AIGSTAMT_" + i).value) + parseFloat(document.getElementById("ACGSTAMT_" + i).value) +
                 parseFloat(document.getElementById("ASGSTAMT_" + i).value) + parseFloat(document.getElementById("ACESSAMT_" + i).value) + parseFloat(document.getElementById("ADUTYAMT_" + i).value);
    document.getElementById("ANETAMT_" + i).value = parseFloat(NET_AMT).toFixed(2);

    //GRID TOTAL CALCULATION
    AmountCalculateTotal();

}
function AmountChange(id, AMOUNT, PER, NETAMT, AMT1, AMT2, AMT3, AMT4) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var NEW_AMT = document.getElementById(id.id).value;
    var PERCENTAGE = document.getElementById(PER.id).value;
    var AMT = document.getElementById(AMOUNT.id).value;
    var AMT_1 = document.getElementById(AMT1.id).value;
    var AMT_2 = document.getElementById(AMT2.id).value;
    var AMT_3 = document.getElementById(AMT3.id).value;
    var AMT_4 = document.getElementById(AMT4.id).value;
    if (PERCENTAGE != "") {
        var CAL_ABET_AMT = parseFloat(AMT) * parseFloat(PERCENTAGE) / 100;
        var BAL_AMT = Math.abs(parseFloat(NEW_AMT) - parseFloat(CAL_ABET_AMT));
        if (BAL_AMT <= 1) {
            document.getElementById(id.id).value = parseFloat(NEW_AMT).toFixed(2);
            document.getElementById(NETAMT.id).value = parseFloat(parseFloat(NEW_AMT) + parseFloat(AMT) + parseFloat(AMT_1) + parseFloat(AMT_2) + parseFloat(AMT_3) + parseFloat(AMT_4)).toFixed(2);
        }
        else {
            document.getElementById(id.id).value = parseFloat(CAL_ABET_AMT).toFixed(2);
            document.getElementById(NETAMT.id).value = parseFloat(parseFloat(CAL_ABET_AMT) + parseFloat(AMT) + parseFloat(AMT_1) + parseFloat(AMT_2) + parseFloat(AMT_3) + parseFloat(AMT_4)).toFixed(2);
        }
    }
    //GRID TOTAL CALCULATION
    AmountCalculateTotal();

}
function AmountCalculateTotal() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var T_CURR_AMT = 0; var T_AMT = 0; var T_IGST_AMT = 0; var T_CGST_AMT = 0; var T_SGST_AMT = 0; var T_CESS_AMT = 0; var T_DUTY_AMT = 0; var T_NET_AMT = 0;
    var GridRow = $("#AMOUNT_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var CURR_AMT = document.getElementById("ACURR_AMT_" + i).value;
        if (CURR_AMT != "") { T_CURR_AMT = T_CURR_AMT + parseFloat(CURR_AMT); } else { T_CURR_AMT = T_CURR_AMT + parseFloat(0); }

        var AMT = document.getElementById("A_AMT_" + i).value;
        if (AMT != "") { T_AMT = T_AMT + parseFloat(AMT); } else { T_AMT = T_AMT + parseFloat(0); }

        var IGST_AMT = document.getElementById("AIGSTAMT_" + i).value;
        if (IGST_AMT != "") { T_IGST_AMT = T_IGST_AMT + parseFloat(IGST_AMT); } else { T_IGST_AMT = T_IGST_AMT + parseFloat(0); }

        var CGST_AMT = document.getElementById("ACGSTAMT_" + i).value;
        if (CGST_AMT != "") { T_CGST_AMT = T_CGST_AMT + parseFloat(CGST_AMT); } else { T_CGST_AMT = T_CGST_AMT + parseFloat(0); }

        var SGST_AMT = document.getElementById("ASGSTAMT_" + i).value;
        if (SGST_AMT != "") { T_SGST_AMT = T_SGST_AMT + parseFloat(SGST_AMT); } else { T_SGST_AMT = T_SGST_AMT + parseFloat(0); }

        var CESS_AMT = document.getElementById("ACESSAMT_" + i).value;
        if (CESS_AMT != "") { T_CESS_AMT = T_CESS_AMT + parseFloat(CESS_AMT); } else { T_CESS_AMT = T_CESS_AMT + parseFloat(0); }

        var DUTY_AMT = document.getElementById("ADUTYAMT_" + i).value;
        if (DUTY_AMT != "") { T_DUTY_AMT = T_DUTY_AMT + parseFloat(DUTY_AMT); } else { T_DUTY_AMT = T_DUTY_AMT + parseFloat(0); }

        var NET_AMT = document.getElementById("ANETAMT_" + i).value;
        if (NET_AMT != "") { T_NET_AMT = T_NET_AMT + parseFloat(NET_AMT); } else { T_NET_AMT = T_NET_AMT + parseFloat(0); }

    }
    document.getElementById("A_T_CURR").value = parseFloat(T_CURR_AMT).toFixed(2);
    document.getElementById("A_T_AMOUNT").value = parseFloat(T_AMT).toFixed(2);
    document.getElementById("A_T_IGST").value = parseFloat(T_IGST_AMT).toFixed(2);
    document.getElementById("A_T_CGST").value = parseFloat(T_CGST_AMT).toFixed(2);
    document.getElementById("A_T_SGST").value = parseFloat(T_SGST_AMT).toFixed(2);
    document.getElementById("A_T_CESS").value = parseFloat(T_CESS_AMT).toFixed(2);
    document.getElementById("A_T_DUTY").value = parseFloat(T_DUTY_AMT).toFixed(2);
    document.getElementById("A_T_NET").value = parseFloat(T_NET_AMT).toFixed(2);

    //BILL AMOUNT CALCULATION
    BillAmountCalculate();

}
function OpenAmount() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var IGST_PER = 0; var CGST_PER = 0; var SGST_PER = 0; var CESS_PER = 0; var DUTY_PER = 0;
    var GridRowMain = $("#_T_SALE_DETAIL_GRID > tbody > tr").length;
    for (i = 0; i <= GridRowMain - 1; i++) {
        var IGST = parseFloat(document.getElementById("D_IGSTPER_" + i).value);
        var CGST = parseFloat(document.getElementById("D_CGSTPER_" + i).value);
        var SGST = parseFloat(document.getElementById("D_SGSTPER_" + i).value);
        var CESS = parseFloat(document.getElementById("D_CESSPER_" + i).value);
        var DUTY = 0;//parseFloat(document.getElementById("DUTYPER_" + i).value);
        if (IGST > IGST_PER) {
            IGST_PER = IGST;
        }
        if (CGST > CGST_PER) {
            CGST_PER = CGST;
        }
        if (SGST > SGST_PER) {
            SGST_PER = SGST;
        }
        if (CESS > CESS_PER) {
            CESS_PER = CESS;
        }
        if (DUTY > DUTY_PER) {
            DUTY_PER = DUTY;
        }
    }
    var GridRowMain = $("#AMOUNT_GRID > tbody > tr").length;
    for (i = 0; i <= GridRowMain - 1; i++) {
        document.getElementById("AIGSTPER_" + i).value = IGST_PER;
        document.getElementById("ACGSTPER_" + i).value = CGST_PER;
        document.getElementById("ASGSTPER_" + i).value = SGST_PER;
        document.getElementById("ACESSPER_" + i).value = CESS_PER;
        document.getElementById("ADUTYPER_" + i).value = DUTY_PER;
    }

}

function ReverceCharges() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var GridRowMain = $("#_T_SALE_DETAIL_GRID > tbody > tr").length;
    var REVCHRG = $("#REVCHRG").val();
    for (var i = 0; i <= GridRowMain - 1; i++) {

        if (REVCHRG == "N") {
            $("#D_IGSTPER_" + i).val(0);
            $("#D_IGSTAMT_" + i).val(parseFloat(0).toFixed(2));
            $("#D_CGSTPER_" + i).val(0);
            $("#D_CGSTAMT_" + i).val(parseFloat(0).toFixed(2));
            $("#D_SGSTPER_" + i).val(0);
            $("#D_SGSTAMT_" + i).val(parseFloat(0).toFixed(2));
            $("#D_CESSPER_" + i).val(0);
            $("#D_CESSAMT_" + i).val(parseFloat(0).toFixed(2));
            $("#D_IGSTPER_" + i).attr('readonly', 'readonly');
            $("#D_IGSTAMT_" + i).attr('readonly', 'readonly');
            $("#D_CGSTPER_" + i).attr('readonly', 'readonly');
            $("#D_CGSTAMT_" + i).attr('readonly', 'readonly');
            $("#D_SGSTPER_" + i).attr('readonly', 'readonly');
            $("#D_SGSTAMT_" + i).attr('readonly', 'readonly');
            $("#D_CESSPER_" + i).attr('readonly', 'readonly');
            $("#D_CESSAMT_" + i).attr('readonly', 'readonly');
            //var taxableamt = $("#D_TXBLVAL_" + i).val();
            //var netamount = parseFloat(taxableamt).toFixed(2);
            //$("#D_NETAMT_" + i).val(netamount);
        }
        else {
            var taxableamt = $("#D_TXBLVAL_" + i).val();
            $("#D_IGSTPER_" + i).removeAttr('readonly');
            $("#D_IGSTAMT_" + i).removeAttr('readonly');
            $("#D_CGSTPER_" + i).removeAttr('readonly');
            $("#D_CGSTAMT_" + i).removeAttr('readonly');
            $("#D_SGSTPER_" + i).removeAttr('readonly');
            $("#D_SGSTAMT_" + i).removeAttr('readonly');
            $("#D_CESSPER_" + i).removeAttr('readonly');
            $("#D_CESSAMT_" + i).removeAttr('readonly');

            var igstp = 0, cgstp = 0, sgstp = 0, cessper = 0;
            prodgrpgstper = $("#D_PRODGRPGSTPER_" + i).val();
            rate = $("#D_RATE_" + i).val();
            if (rate == "") { rate = parseFloat(0); } else { rate = parseFloat(rate); }
            var allgst = retGstPerstr(prodgrpgstper, rate);
            if (allgst != "") {
                var str = allgst.split(',');
                if (str.length > 0) {
                    igstp = $(parseFloat(str[0]).toFixed(2)).val();
                    cgstp = $(parseFloat(str[1]).toFixed(2)).val();
                    sgstp = $(parseFloat(str[2]).toFixed(2)).val();
                }

                $("#D_IGSTPER_" + i).val(igstp);
                //var igstamount = parseFloat(taxableamt * igstp / 100).toFixed(2);
                //$("#D_IGSTAMT_" + i).val(igstamount);


                $("#D_CGSTPER_" + i).val(cgstp);
                //var cgstamount = parseFloat(taxableamt * cgstp / 100).toFixed(2);
                //$("#D_CGSTAMT_" + i).val(cgstamount);


                $("#D_SGSTPER_" + i).val(sgstp);
                //var sgstamount = parseFloat(taxableamt * sgstp / 100).toFixed(2);
                //$("#D_SGSTAMT_" + i).val(sgstamount);

                //cessper = $("#BackupCESSTPER").val();
                //$("#D_CESSPER_" + i).val(cessper);
                //var cessamount = parseFloat(taxableamt * cessper / 100).toFixed(2);
                //$("#D_CESSAMT_" + i).val(cessamount);
                //var netamount = parseFloat(taxableamt) + parseFloat(igstamount) + parseFloat(cgstamount) + parseFloat(sgstamount) + parseFloat(cessamount);
                //netamount = parseFloat(netamount).toFixed(2);
                //$("#D_NETAMT_" + i).val(netamount);
            }
        }
        CalculateAmt_Details(i);
    }
    //BillAmountCalculate();
    //DRCRBillAmount();
    //SBILLBillAmount();

}
function BillAmountCalculate() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    debugger;
    var R_TOTAL_BILL_AMOUNT = 0;
    var TOTAL_ROUND = 0;
    var netamt = 0;
    var ROUND_TAG = document.getElementById("RoundOff").checked;
    var D_TOTALNOS = 0, D_TOTALQNTY = 0, D_TOTALTAXVAL = 0, A_TOTALTAXVAL = 0, D_TOTALIGST = 0, A_TOTALIGST = 0, D_TOTALCGST = 0, A_TOTALCGST = 0, D_TOTALSGST = 0, A_TOTALSGST = 0, D_TOTALNETAMT = 0, A_TOTALNETAMT = 0;
    var T_NOS = $("#T_NOS").val();
    if (T_NOS != "") { D_TOTALNOS = D_TOTALNOS + parseFloat(T_NOS); } else { D_TOTALNOS = D_TOTALNOS + parseFloat(0); }

    var T_QNTY = $("#T_QNTY").val();
    if (T_QNTY != "") { D_TOTALQNTY = D_TOTALQNTY + parseFloat(T_QNTY); } else { D_TOTALQNTY = D_TOTALQNTY + parseFloat(0); }

    var T_GROSS_AMT = $("#T_GROSS_AMT").val();
    if (T_GROSS_AMT != "") { D_TOTALTAXVAL = D_TOTALTAXVAL + parseFloat(T_GROSS_AMT); } else { D_TOTALTAXVAL = D_TOTALTAXVAL + parseFloat(0); }

    var A_T_AMOUNT = $("#A_T_AMOUNT").val();
    if (A_T_AMOUNT != "") { A_TOTALTAXVAL = A_TOTALTAXVAL + parseFloat(A_T_AMOUNT); } else { A_TOTALTAXVAL = A_TOTALTAXVAL + parseFloat(0); }
    //
    var T_IGST_AMT = $("#T_IGST_AMT").val();
    if (T_IGST_AMT != "") { D_TOTALIGST = D_TOTALIGST + parseFloat(T_IGST_AMT); } else { D_TOTALIGST = D_TOTALIGST + parseFloat(0); }

    var A_T_IGST = $("#A_T_IGST").val();
    if (A_T_IGST != "") { A_TOTALIGST = A_TOTALIGST + parseFloat(A_T_IGST); } else { A_TOTALIGST = A_TOTALIGST + parseFloat(0); }

    var T_CGST_AMT = $("#T_CGST_AMT").val();
    if (T_CGST_AMT != "") { D_TOTALCGST = D_TOTALCGST + parseFloat(T_CGST_AMT); } else { D_TOTALCGST = D_TOTALCGST + parseFloat(0); }

    var A_T_CGST = $("#A_T_CGST").val();
    if (A_T_CGST != "") { A_TOTALCGST = A_TOTALCGST + parseFloat(A_T_CGST); } else { A_TOTALCGST = A_TOTALCGST + parseFloat(0); }

    var T_SGST_AMT = $("#T_SGST_AMT").val();
    if (T_SGST_AMT != "") { D_TOTALSGST = D_TOTALSGST + parseFloat(T_SGST_AMT); } else { D_TOTALSGST = D_TOTALSGST + parseFloat(0); }

    var A_T_SGST = $("#A_T_SGST").val();
    if (A_T_SGST != "") { A_TOTALSGST = A_TOTALSGST + parseFloat(A_T_SGST); } else { A_TOTALSGST = A_TOTALSGST + parseFloat(0); }

    var T_NET_AMT = $("#T_NET_AMT").val();
    if (T_NET_AMT != "") { D_TOTALNETAMT = D_TOTALNETAMT + parseFloat(T_NET_AMT); } else { D_TOTALNETAMT = D_TOTALNETAMT + parseFloat(0); }

    var A_T_NET = $("#A_T_NET").val();
    if (A_T_NET != "") { A_TOTALNETAMT = A_TOTALNETAMT + parseFloat(A_T_NET); } else { A_TOTALNETAMT = A_TOTALNETAMT + parseFloat(0); }

    var totaltaxval = 0;
    totaltaxval = parseFloat(parseFloat(D_TOTALTAXVAL) + parseFloat(A_TOTALTAXVAL)).toFixed(2);

    var totaltax = 0;
    totaltax = parseFloat(parseFloat(D_TOTALIGST) + parseFloat(A_TOTALIGST) + parseFloat(D_TOTALCGST) + parseFloat(A_TOTALCGST) + parseFloat(D_TOTALSGST) + parseFloat(A_TOTALSGST)).toFixed(2);

    var totalbillamt = 0;
    totalbillamt = parseFloat(parseFloat(D_TOTALNETAMT) + parseFloat(A_TOTALNETAMT)).toFixed(2);

    $("#TOTNOS").val(parseFloat(D_TOTALNOS).toFixed(0));
    $("#TOTQNTY").val(parseFloat(D_TOTALQNTY).toFixed(2));
    $("#TOTTAXVAL").val(parseFloat(totaltaxval).toFixed(2));
    $("#TOTTAX").val(parseFloat(totaltax).toFixed(2));

    //tcs
    var TCSPER = 0; TCSAMT = 0; TCSON = 0;
    var MENU_PARA = $("#MENU_PARA").val();
    if (MENU_PARA != "SR" || MENU_PARA != "PR") {
        TCSPER = parseFloat(document.getElementById("TCSPER").value).toFixed(3);
        if (TCSPER == "" || TCSPER == "NaN") { TCSPER = parseFloat(0); }
        document.getElementById("TCSPER").value = parseFloat(TCSPER).toFixed(3);
        if (MENU_PARA == "PB") {
            TCSON = $("#TCSON").val();
            if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON); }
        }
        else {
            GetTCSON(totalbillamt);
            TCSON = $("#TCSON").val();
            if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON) }
        }
        TCSAMT = parseFloat(parseFloat(TCSON) * parseFloat(TCSPER) / 100);
        TCSAMT = CalculateTcsAmt(TCSAMT);
        if (MENU_PARA == "PB") {
            var NEW_TCSAMT = $("#TCSAMT").val();
            if (NEW_TCSAMT == "") { NEW_TCSAMT = parseFloat(0); } else { NEW_TCSAMT = parseFloat(NEW_TCSAMT) }

            var TCSON = $("#TCSON").val();
            if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON) }

            var TCSPER = $("#TCSPER").val();
            if (TCSPER == "") { TCSPER = parseFloat(0); } else { TCSPER = parseFloat(TCSPER) }

            var CAL_TCSAMT = TCSAMT;
            var BAL_AMT = Math.abs(parseFloat(NEW_TCSAMT) - parseFloat(CAL_TCSAMT));
            if (BAL_AMT <= 1 && NEW_TCSAMT > 0) {
                $("#TCSAMT").val(parseFloat(NEW_TCSAMT).toFixed(2));
                TCSAMT = NEW_TCSAMT;
            }
            else {
                $("#TCSAMT").val(parseFloat(CAL_TCSAMT).toFixed(2));
                TCSAMT = CAL_TCSAMT;
            }
        }
        else {
            document.getElementById("TCSAMT").value = parseFloat(TCSAMT).toFixed(2);

        }
    }
    //
    totalbillamt = parseFloat(totalbillamt) + parseFloat(TCSAMT);
    var REVCHRG = $("#REVCHRG").val();
    if (REVCHRG == "Y") {
        totalbillamt = parseFloat(totalbillamt) - parseFloat(totaltax);
    }
    if (ROUND_TAG == true) {
        R_TOTAL_BILL_AMOUNT = Math.round(totalbillamt);
        TOTAL_ROUND = R_TOTAL_BILL_AMOUNT - totalbillamt;
        document.getElementById("BLAMT").value = parseFloat(R_TOTAL_BILL_AMOUNT).toFixed(2);
        document.getElementById("ROAMT").value = parseFloat(TOTAL_ROUND).toFixed(2);
    }
    else {
        TOTAL_ROUND = 0;
        document.getElementById("BLAMT").value = parseFloat(totalbillamt).toFixed(2);
        document.getElementById("ROAMT").value = parseFloat(TOTAL_ROUND).toFixed(2);
    }

}

function CalulateTareWt(GRWT, NTWT, TRWT) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var GROSS = document.getElementById(GRWT.id).value;
    if (GROSS == "") {
        GROSS = parseFloat(0);
    }
    var NET = document.getElementById(NTWT.id).value;
    if (NET == "") {
        NET = parseFloat(0);
    }
    var TARE = GROSS - NET;
    document.getElementById(TRWT.id).value = TARE.toFixed(3);

}

function Checked_Disable() {
    var GridRow = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        if ($("#B_ChildData_" + i).val() == "Y") {
            document.getElementById("B_Checked_" + i).disabled = true;
        }
    }
}

function DeleteBarnoRow() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    $.ajax({
        type: 'POST',
        url: $("#UrlDeleteRow").val(),// "@Url.Action("DeleteRow", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            $("#partialdivBarCodeTab").animate({ marginTop: '0px' }, 50);
            $("#partialdivBarCodeTab").html(result);
            Checked_Disable();
            CalculateTotal_Barno();
            HasChangeBarSale();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });

}

function AddDOCrow() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    $.ajax({
        type: 'POST',
        url: $("#UrlAddDOCRow").val(),//"@Url.Action("AddDOCRow", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            $("#partialdivDocument").animate({ marginTop: '-10px' }, 50);
            $("#partialdivDocument").html(result);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });

}
function DeleteDOCrow() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    $.ajax({
        type: 'POST',
        url: $("#UrlDeleteDOCRow").val(),// "@Url.Action("DeleteDOCRow", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            $("#partialdivDocument").animate({ marginTop: '0px' }, 50);
            $("#partialdivDocument").html(result);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });

}

function SelectTDSCode(id, TDSHD, TDSNM, TCSPER) {
    if (id == "") {
        $("#" + TDSHD.id).val("");
        $("#" + TDSNM.id).val("");
        $("#" + TCSPER.id).val("");
        $("#TDSLIMIT").val("");
        $("#TDSCALCON").val("");
        $("#TDSROUNDCAL").val("");
        $("#AMT").val("");
        BillAmountCalculate();//fill value of tcson
    }
    else {
        if (!emptyFieldCheck("Enter Document Date", "DOCDT")) { return false; }
        if (!emptyFieldCheck("Enter Supplier Code", "SLCD")) { return false; }
        var PARTY = document.getElementById("SLCD").value;
        var DOCDT = document.getElementById("DOCDT").value;
        var autono = document.getElementById("AUTONO").value;
        $.ajax({
            type: 'GET',
            url: $("#UrlSelectTDSCode").val(),//"@Url.Action("GetTDSDetails", PageControllerName)",
            data:
        {
            val: id,
            PARTY: PARTY,
            TAG: DOCDT,
            AUTONO: autono
        },
            success: function (result) {
                var MSG = result.indexOf(String.fromCharCode(181));
                if (MSG >= 0) {
                    document.getElementById(TDSHD.id).value = returncolvalue(result, "TDSCODE");
                    document.getElementById(TDSNM.id).value = returncolvalue(result, "TDSNM");
                    var CMPNONCMP = returncolvalue(result, "CMPNONCMP");
                    if (CMPNONCMP == "L") {
                        document.getElementById(TCSPER.id).value = returncolvalue(result, "TDSPER");
                    } else {
                        document.getElementById(TCSPER.id).value = returncolvalue(result, "TDSPERNONCMP");
                    }
                    document.getElementById("TDSLIMIT").value = returncolvalue(result, "TDSLIMIT");
                    document.getElementById("TDSCALCON").value = returncolvalue(result, "TDSCALCON");
                    document.getElementById("TDSROUNDCAL").value = returncolvalue(result, "TDSROUNDCAL");
                    document.getElementById("AMT").value = returncolvalue(result, "AMT");
                    BillAmountCalculate();//fill value of tcson
                }
                else {
                    $("#" + TDSHD.id).val("");
                    $("#" + TDSNM.id).val("");
                    $("#" + TCSPER.id).val("");
                    $("#TDSLIMIT").val("");
                    $("#TDSCALCON").val("");
                    $("#TDSROUNDCAL").val("");
                    $("#AMT").val("");
                    BillAmountCalculate();//fill value of tcson
                    msgInfo(result);
                    message_value = TDSHD.id;
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
function AddBarCodeGrid() {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    if ($("#ITGRPCD").val() == "") {
        msgInfo("Please enter/select Item Group Code !");
        message_value = "ITGRPCD";
        return false;
    }
    if ($("#MTRLJOBCD").val() == "") {
        msgInfo("Please enter Material Job Code !");
        message_value = "MTRLJOBCD";
        return false;
    }
    if ($("#ITCD").val() == "") {
        msgInfo("Please enter Item Code !");
        message_value = "ITCD";
        return false;
    }
    if ($("#QNTY").val() == "0") {
        msgInfo("Quantity should not be zero(0) !");
        message_value = "QNTY";
        return false;
    }
    if (MENU_PARA != "SR" && MENU_PARA != "SBCMR" && MENU_PARA != "PB") {
        var NEGSTOCK = $("#NEGSTOCK").val();
        var BALSTOCK = $("#BALSTOCK").val();
        if (BALSTOCK == "") { BALSTOCK = parseFloat(0); } else { BALSTOCK = parseFloat(BALSTOCK); }
        var QNTY = $("#QNTY").val();
        if (QNTY == "") { QNTY = parseFloat(0); } else { QNTY = parseFloat(QNTY); }
        var balancestock = BALSTOCK - QNTY;
        if (balancestock < 0) {
            if (NEGSTOCK != "Y") {
                msgInfo("Quantity should not be grater than Stock !");
                message_value = "QNTY";
                return false;
            }

        }
    }

    var FLAGMTR = $("#FLAGMTR").val();
    var QNTY = $("#QNTY").val();

    if (FLAGMTR != "") {
        var flgmtr = FLAGMTR;
        if (flgmtr == "") { flgmtr = parseFloat(0); } else { flgmtr = parseFloat(flgmtr); }
        var qnty = QNTY;
        if (qnty == "") { qnty = parseFloat(0); } else { qnty = parseFloat(qnty); }
        if (flgmtr > qnty) {
            msgInfo("FLAGMTR (" + flgmtr + ") should be less than Quantity (" + qnty + ") !");
            message_value = "FLAGMTR";
            return false;
        }
    }
    if ($("#HSNCODE").val() == "") {
        msgInfo("Please enter HSN Code !");
        message_value = "HSNCODE";
        return false;
    }
    var TXNSLNO = "";
    if ($("#TXNSLNO").val() == "" || $("#TXNSLNO").val() == "0") {
        var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
        if (GridRowMain == 0) {
            TXNSLNO = 1;
        }
        else {
            var allslno = [parseInt(GridRowMain)];
            for (j = 0; j <= GridRowMain - 1; j++) {
                allslno[j] = parseInt($("#B_TXNSLNO_" + j).val());
            }
            TXNSLNO = Math.max.apply(Math, allslno);
            TXNSLNO++;
        }
    }
    else {
        TXNSLNO = parseInt($("#TXNSLNO").val());
    }

    var BARCODE = $("#BARCODE").val();
    var ITGRPCD = $("#ITGRPCD").val();
    var ITGRPNM = $("#ITGRPNM").val();
    var MTRLJOBCD = $("#MTRLJOBCD").val();
    var MTRLJOBNM = $("#MTRLJOBNM").val();
    var MTBARCODE = $("#MTBARCODE").val();
    var ITCD = $("#ITCD").val();
    var ITSTYLE = $("#ITSTYLE").val();
    var STYLENO = $("#STYLENO").val();
    var STKTYPE = $("#STKTYPE").val();
    var PARTCD = $("#PARTCD").val();
    var PARTNM = $("#PARTNM").val();
    var PRTBARCODE = $("#PRTBARCODE").val();
    var COLRCD = $("#COLRCD").val();
    var COLRNM = $("#COLRNM").val();
    var CLRBARCODE = $("#CLRBARCODE").val();
    var SIZECD = $("#SIZECD").val();
    var SIZENM = $("#SIZENM").val();
    var SZBARCODE = $("#SZBARCODE").val();

    var BALSTOCK = $("#BALSTOCK").val();
    var UOM = $("#UOM").val();
    var NOS = $("#NOS").val();
    var RATE = $("#RATE").val();

    var DISCRATE = $("#DISCRATE").val();
    var DISCTYPE = $("#DISCTYPE").val();
    var DISCTYPE_DESC = DISCTYPE == "P" ? "%" : DISCTYPE == "N" ? "Nos" : DISCTYPE == "Q" ? "Qnty" : "Fixed";
    var HSNCODE = $("#HSNCODE").val();
    var GSTPER = $("#GSTPER").val();
    var PRODGRPGSTPER = $("#PRODGRPGSTPER").val();
    var SHADE = $("#SHADE").val();
    var BALENO = "";
    var OURDESIGN = "";
    var PDESIGN = "";

    var WPPRICEGEN = "";
    var RPPRICEGEN = "";
    if (MENU_PARA == "PB") {
        BALENO = $("#BALENO").val();
        OURDESIGN = $("#OURDESIGN").val();
        PDESIGN = $("#PDESIGN").val();
        WPPRICEGEN = $("#WPPRICEGEN").val();
        RPPRICEGEN = $("#RPPRICEGEN").val();
    }
    var TDDISCTYPE = $("#TDDISCTYPE").val();
    var TDDISCTYPE_DESC = TDDISCTYPE == "P" ? "%" : TDDISCTYPE == "N" ? "Nos" : TDDISCTYPE == "Q" ? "Qnty" : "Fixed";
    var TDDISCRATE = $("#TDDISCRATE").val();
    var SCMDISCTYPE = $("#SCMDISCTYPE").val();
    var SCMDISCTYPE_DESC = SCMDISCTYPE == "P" ? "%" : SCMDISCTYPE == "N" ? "Nos" : SCMDISCTYPE == "Q" ? "Qnty" : "Fixed";
    var SCMDISCRATE = $("#SCMDISCRATE").val();
    var LOCABIN = $("#LOCABIN").val();
    var GLCD = $("#GLCD").val();
    var ITMBARGENTYPE = $("#BARGENTYPETEMP").val();
    var ENTRYBARGENTYPE = $("#BARGENTYPE").val();
    if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "PB") {
        var ORDDOCNO = $("#ORDDOCNO").val();
        var ORDDOCDT = $("#ORDDOCDT").val();
        var ORDAUTONO = $("#ORDAUTONO").val();
        var ORDSLNO = $("#ORDSLNO").val();
    }
    var BarImages = $("#BarImages").val();
    var NoOfBarImages = BarImages.split(String.fromCharCode(179)).length;
    if (BarImages == '') { NoOfBarImages = ''; }
    var rowindex = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    var SLNO = 1;
    if (parseInt(rowindex) != 0) {
        var LASTROWINDEX = parseInt(rowindex) - 1;
        var LASTSLNO = $("#B_SLNO_" + LASTROWINDEX).val();
        SLNO = parseInt(LASTSLNO) + 1;
    }
    var BALSTOCK = $("#BALSTOCK").val();
    var NEGSTOCK = $("#NEGSTOCK").val();

    var tr = "";
    tr += ' <tr style="font-size:12px; font-weight:bold;">';
    tr += '    <td class="sticky-cell">';
    tr += '        <input tabindex="-1" data-val="true" data-val-required="The Checked field is required." id="B_Checked_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].Checked" type="checkbox" value="true"><input name="TBATCHDTL[' + rowindex + '].Checked" type="hidden" value="false">';
    tr += '        <input data-val="true" data-val-length="The field BARNO must be a string with a maximum length of 25." data-val-length-max="25" data-val-required="The BARNO field is required." id="B_BARNO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BARNO" type="hidden" value="' + BARCODE + '">';
    tr += '        <input data-val="true" data-val-number="The field FLAGMTR must be a number." id="B_FLAGMTR_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].FLAGMTR" type="hidden" value="' + FLAGMTR + '">';
    tr += '        <input data-val="true" data-val-length="The field HSNCODE must be a string with a maximum length of 8." data-val-length-max="8" id="B_HSNCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].HSNCODE" type="hidden" value="' + HSNCODE + '">';
    if (MENU_PARA == "PB") {
        tr += '        <input data-val="true" data-val-length="The field BALENO must be a string with a maximum length of 30." data-val-length-max="30" id="B_BALENO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BALENO" type="hidden" value="' + BALENO + '">';
        tr += '        <input data-val="true" data-val-length="The field OURDESIGN must be a string with a maximum length of 30." data-val-length-max="30" id="B_OURDESIGN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].OURDESIGN" type="hidden" value="' + OURDESIGN + '">';
        tr += '        <input data-val="true" data-val-length="The field PDESIGN must be a string with a maximum length of 30." data-val-length-max="30" id="B_PDESIGN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PDESIGN" type="hidden" value="' + PDESIGN + '">';
    }
    tr += '        <input data-val="true" data-val-length="The field LOCABIN must be a string with a maximum length of 10." data-val-length-max="10" id="B_LOCABIN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].LOCABIN" type="hidden" value="' + LOCABIN + '">';
    tr += '        <input data-val="true" data-val-length="The field GLCD must be a string with a maximum length of 10." data-val-length-max="10" id="B_GLCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].GLCD" type="hidden" value="' + GLCD + '">';
    tr += '        <input id="B_BARGENTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BARGENTYPE" type="hidden" value="' + ITMBARGENTYPE + '">';
    tr += '    </td>';
    tr += '    <td class="sticky-cell" style="left:20px;" title="' + SLNO + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-number="The field SLNO must be a number." data-val-required="The SLNO field is required." id="B_SLNO_' + rowindex + '" maxlength="2" name="TBATCHDTL[' + rowindex + '].SLNO" readonly="readonly" style="text-align:center;" type="text" value="' + SLNO + '">';
    tr += '    </td>';
    tr += '    <td class="sticky-cell" style="left:40px" title="' + TXNSLNO + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-number="The field TXNSLNO must be a number." data-val-required="The TXNSLNO field is required." id="B_TXNSLNO_' + rowindex + '" maxlength="4" name="TBATCHDTL[' + rowindex + '].TXNSLNO"  style="text-align:center;" onkeypress="return numericOnly(this,2);" onchange="HasChangeBarSale();" type="text" value="' + TXNSLNO + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + ITGRPNM + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ITGRPNM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ITGRPNM" readonly="readonly" type="text" value="' + ITGRPNM + '">';
    tr += '        <input id="B_ITGRPCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ITGRPCD" type="hidden" value="' + ITGRPCD + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + MTRLJOBCD + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field MTRLJOBCD must be a string with a maximum length of 2." data-val-length-max="2" id="B_MTRLJOBCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].MTRLJOBCD" readonly="readonly" type="text" value="' + MTRLJOBCD + '">';
    tr += '        <input id="B_MTRLJOBNM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].MTRLJOBNM" type="hidden" value="' + MTRLJOBNM + '">';
    tr += '        <input id="B_MTBARCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].MTBARCODE" type="hidden" value="' + MTBARCODE + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + ITCD + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ITCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ITCD" readonly="readonly" type="text" value="' + ITCD + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + ITSTYLE + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ITSTYLE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ITSTYLE" readonly="readonly" type="text" value="' + ITSTYLE + '">';
    tr += '        <input id="B_STYLENO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].STYLENO" type="hidden" value="' + STYLENO + '">';
    tr += '    </td>         ';
    tr += '    <td class="" title="' + STKTYPE + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field STKTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_STKTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].STKTYPE" readonly="readonly" type="text" value="' + STKTYPE + '">';
    tr += '     </td>';
    tr += '    <td class="" title="' + PARTCD + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field PARTCD must be a string with a maximum length of 4." data-val-length-max="4" id="B_PARTCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PARTCD" readonly="readonly" type="text" value="' + PARTCD + '">';
    tr += '        <input id="B_PARTNM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PARTNM" type="hidden" value="' + PARTNM + '">';
    tr += '        <input id="B_PRTBARCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PRTBARCODE" type="hidden" value="' + PRTBARCODE + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + COLRCD + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field COLRCD must be a string with a maximum length of 4." data-val-length-max="4" id="B_COLRCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].COLRCD" readonly="readonly" type="text" value="' + COLRCD + '">';
    tr += '     <input id="B_CLRBARCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].CLRBARCODE" type="hidden" value="' + CLRBARCODE + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + COLRNM + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_COLRNM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].COLRNM" readonly="readonly" type="text" value="' + COLRNM + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + SIZECD + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field SIZECD must be a string with a maximum length of 4." data-val-length-max="4" id="B_SIZECD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SIZECD" readonly="readonly" type="text" value="' + SIZECD + '">';
    tr += '        <input id="B_SIZENM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SIZENM" type="hidden" value="' + SIZENM + '">';
    tr += '        <input id="B_SZBARCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SZBARCODE" type="hidden" value="' + SZBARCODE + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + SHADE + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field SHADE must be a string with a maximum length of 15." data-val-length-max="15" id="B_SHADE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SHADE" readonly="readonly" type="text" value="' + SHADE + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + BALSTOCK + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_BALSTOCK_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BALSTOCK" readonly="readonly" style="text-align: right;" type="text" value="' + BALSTOCK + '">';
    tr += '        <input id="B_NEGSTOCK_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].NEGSTOCK" type="hidden" value="' + NEGSTOCK + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + QNTY + '">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field QNTY must be a number." id="B_QNTY_' + rowindex + '" maxlength="12" name="TBATCHDTL[' + rowindex + '].QNTY" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" onblur="CalculateTotal_Barno();HasChangeBarSale();" value="' + QNTY + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + UOM + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor" id="B_UOM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].UOM" readonly="readonly" type="text" value="' + UOM + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + NOS + '">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NOS must be a number." id="B_NOS_' + rowindex + '" maxlength="12" name="TBATCHDTL[' + rowindex + '].NOS" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" onchange="CalculateTotal_Barno();HasChangeBarSale();" value="' + NOS + '">';
    tr += '    </td>';
    tr += '    <td class="">';
    tr += '        <button class="atextBoxFor btn-info" type="button" id="btnRateHistory_"' + rowindex + ' title="Rate History">Rate Hist</button>';
    tr += '    </td>';
    tr += '    <td class="" title="' + RATE + '">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RATE must be a number." id="B_RATE_' + rowindex + '" maxlength="14" name="TBATCHDTL[' + rowindex + '].RATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="GetGstPer(' + rowindex + ',\'#B_\');RateUpdate(' + rowindex + ');HasChangeBarSale();" value="' + RATE + '" >';
    tr += '    </td>';
    if (MENU_PARA == "PB") {
        tr += '    <td class="">';
        tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field WPRATE must be a number." id="B_WPRATE_' + rowindex + '" maxlength="14" name="TBATCHDTL[' + rowindex + '].WPRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text"  readonly="readonly"  >';
        tr += '        <input data-val="true" data-val-length="The field WPPRICEGEN must be a string with a maximum length of 30." data-val-length-max="30" id="B_WPPRICEGEN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].WPPRICEGEN" type="hidden" value="' + WPPRICEGEN + '">';
        tr += '        <input data-val="true" data-val-length="The field RPPRICEGEN must be a string with a maximum length of 30." data-val-length-max="30" id="B_RPPRICEGEN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].RPPRICEGEN" type="hidden" value="' + RPPRICEGEN + '">';
        tr += '    </td>';

        tr += '    <td class="">';
        tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RPRATE must be a number." id="B_RPRATE_' + rowindex + '" maxlength="14" name="TBATCHDTL[' + rowindex + '].RPRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text"  readonly="readonly" >';
        tr += '    </td>';
    }

    tr += '    <td class="" title="' + GSTPER + '">';
    tr += '        <input class="atextBoxFor text-box single-line" data-val="true" data-val-number="The field GSTPER must be a number." id="B_GSTPER_' + rowindex + '" maxlength="5" name="TBATCHDTL[' + rowindex + '].GSTPER" onkeypress="return numericOnly(this,2);" style="text-align: right;" readonly="readonly" type="text" value="' + GSTPER + '">';
    tr += '        <input id="B_PRODGRPGSTPER_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PRODGRPGSTPER" type="hidden" value="' + PRODGRPGSTPER + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + DISCTYPE_DESC + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor" id="B_DISCTYPE_DESC_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].DISCTYPE_DESC" readonly="readonly" type="text" value="' + DISCTYPE_DESC + '">';
    tr += '        <input data-val="true" data-val-length="The field DISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_DISCTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].DISCTYPE" type="hidden" value="' + DISCTYPE + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + DISCRATE + '">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field DISCRATE must be a number." id="B_DISCRATE_' + rowindex + '" maxlength="10" name="TBATCHDTL[' + rowindex + '].DISCRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="HasChangeBarSale();" value="' + DISCRATE + '">';
    tr += '    </td>';
    tr += '     <td class="" title="' + TDDISCTYPE_DESC + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_TDDISCTYPE_DESC_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].TDDISCTYPE_DESC" readonly="readonly" type="text" value="' + TDDISCTYPE_DESC + '">';
    tr += '        <input data-val="true" data-val-length="The field TDDISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_TDDISCTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].TDDISCTYPE" type="hidden" value="' + TDDISCTYPE + '">';
    tr += '                              </td>';
    tr += '    <td class="" title="' + TDDISCRATE + '">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field TDDISCRATE must be a number." id="B_TDDISCRATE_' + rowindex + '" maxlength="10" name="TBATCHDTL[' + rowindex + '].TDDISCRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="HasChangeBarSale();" value="' + TDDISCRATE + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + SCMDISCTYPE_DESC + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_SCMDISCTYPE_DESC_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SCMDISCTYPE_DESC" readonly="readonly" type="text" value="' + SCMDISCTYPE_DESC + '">';
    tr += '        <input data-val="true" data-val-length="The field SCMDISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_SCMDISCTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SCMDISCTYPE" type="hidden" value="' + SCMDISCTYPE + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + SCMDISCRATE + '">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field SCMDISCRATE must be a number." id="B_SCMDISCRATE_' + rowindex + '" maxlength="10" name="TBATCHDTL[' + rowindex + '].SCMDISCRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="HasChangeBarSale();" value="' + SCMDISCRATE + '">';
    tr += '    </td>';
    if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "PB") {
        tr += '    <td class="" title="' + ORDDOCNO + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ORDDOCNO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ORDDOCNO" readonly="readonly" type="text" value="' + ORDDOCNO + '">';
        tr += '        <input id="B_ORDAUTONO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ORDAUTONO" type="hidden" value="' + ORDAUTONO + '">';
        tr += '        <input id="B_ORDSLNO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ORDSLNO" type="hidden" value="' + ORDSLNO + '">';
        tr += '    </td>';
        tr += '    <td class="" title="' + ORDDOCDT + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ORDDOCDT_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ORDDOCDT" readonly="readonly" type="text" value="' + ORDDOCDT + '">';
        tr += '    </td>';
    }
    tr += '    <td class="">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field ITREM must be a number." id="B_ITREM_' + rowindex + '" maxlength="100" name="TBATCHDTL[' + rowindex + '].ITREM"   type="text"  onclick = "OpenZoomTextBoxModal(this.id)" data_toggle = "modal" data_target = "#ZoomTextBoxModal" onblur = "HasChangeBarSale();" >';
    tr += '    </td>';
    tr += '   <td class=""> ';
    tr += '   <button type="button" onclick="T_Sale_FillImageModal(' + rowindex + ')" data-toggle="modal" data-target="#ViewImageModal" id="OpenImageModal_' + rowindex + '" class="btn atextBoxFor text-info" style="padding:0px">' + NoOfBarImages + '</button> ';
    tr += '   </td> ';
    tr += '   <td class="">  ';
    if (MENU_PARA == "PB" && ENTRYBARGENTYPE == "E") {
        tr += '   <input type="button" value="Upload" class="btn-sm atextBoxFor" onclick="UploadBarnoImage(' + rowindex + ');" style="padding:0px" readonly="readonly" placeholder=""> ';
    }
    tr += '   <input id="B_BarImages_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BarImages" type="hidden" readonly="readonly" placeholder="" value=' + BarImages + '> ';
    tr += '   </td> ';
    if (MENU_PARA != "SB") {
        tr += '        <td class="sticky-cell-opposite">';
        tr += '            <button type="button" class="atextBoxFor btn-info" onclick="FillBarcodeArea(\'\', \'_T_SALE_PRODUCT_GRID\', ' + rowindex + ');" title="CLICK HERE TO EDIT BARCODEDATA"><span class="glyphicon glyphicon-pencil"></span></button>';
        tr += '        </td>';
    }
    tr += ' </tr>';

    $("#_T_SALE_PRODUCT_GRID tbody").append(tr);
    CalculateTotal_Barno();
    if (MENU_PARA == "PB") {
        RateUpdate(rowindex);

    }

    ClearBarcodeArea();
    if (MENU_PARA == "PB") {
        $("#TXNSLNO").focus();
    } else {
        $("#BARCODE").focus();
    }
    $("#bardatachng").val("Y");
}
function RateUpdate(index) {
    var MENU_PARA = $("#MENU_PARA").val();
    if ((MENU_PARA == "PB") && (($("#BARGENTYPE").val() == "E") || ($("#BARGENTYPE").val() == "C" && $("#B_BARGENTYPE_" + index).val() == "E"))) {
        var WPPER = retFloat($("#WPPER").val());
        var RPPER = retFloat($("#RPPER").val());
        var RATE = retFloat($("#B_RATE_" + index).val());
        var WPPRICEGEN = $("#B_WPPRICEGEN_" + index).val();
        var RPPRICEGEN = $("#B_RPPRICEGEN_" + index).val();
        if (WPPER != 0) {
            var wprt = retFloat(((retFloat(RATE) * retFloat(WPPER)) / 100) + retFloat(RATE));
            var B_WPRATE = CharmPrice(retStr(WPPRICEGEN).substring(0, 2), wprt, retStr(WPPRICEGEN).substring(2, retStr(WPPRICEGEN).length));
            $("#B_WPRATE_" + index).val(parseFloat(B_WPRATE).toFixed(2))
            $("#B_WPRATE_" + index).attr('title', parseFloat(B_WPRATE).toFixed(2));
        }
        if (RPPER != 0) {
            var rprt = retFloat(((retFloat(RATE) * retFloat(RPPER)) / 100) + retFloat(RATE));
            var B_RPRATE = CharmPrice(retStr(RPPRICEGEN).substring(0, 2), rprt, retStr(RPPRICEGEN).substring(2, retStr(RPPRICEGEN).length));
            $("#B_RPRATE_" + index).val(parseFloat(B_RPRATE).toFixed(2))
            $("#B_RPRATE_" + index).attr('title', parseFloat(B_RPRATE).toFixed(2))
        }
    }
}
function BarGridRateUpdate() {
    var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    for (j = 0; j <= GridRowMain - 1; j++) {
        RateUpdate(j);
    }
}
function fileTypeCheck(id) {
    var fileUpload = $(id).get(0);
    var filesSelected = fileUpload.files;
    if (filesSelected.length > 0) {
        var fileToLoad = filesSelected[0];
        document.getElementById("ImageName").value = fileToLoad.name;
        var fileReader = new FileReader();
        fileReader.onload = function (fileLoadedEvent) {
            document.getElementById("ImageStr").value = fileLoadedEvent.target.result;
        };
        fileReader.readAsDataURL(fileToLoad);
    }
}

function UploadBarnoImage(i) {
    debugger;
    var ImageDesc = $('#ImageDesc').val();
    if (document.getElementById("ImageName").value == "") return;
    var OpenImageModal = $('#OpenImageModal_' + i).html(); var actt = "";
    if (OpenImageModal == "") {
        OpenImageModal = 1; actt = "active"; $("#div_carousel_inner").html('');
    } else {
        OpenImageModal = parseInt(OpenImageModal) + 1;
        actt = "";
    }
    $('#OpenImageModal_' + i).html(OpenImageModal);
    $.ajax({
        type: 'POST',
        url: $("#UrlUploadImages").val(),// "@Url.Action("UploadImages", PageControllerName )",
        beforesend: $("#WaitingMode").show(),
        data: "ImageStr=" + $('#ImageStr').val() + "&ImageName=" + $('#ImageName').val() + "&ImageDesc=" + ImageDesc,
        success: function (result) {
            $("#WaitingMode").hide(); var newid = '';
            if ($("#B_BarImages_" + i).val() != "") {
                newid = $("#B_BarImages_" + i).val() + String.fromCharCode(179) + (result.split('/')[2] + '~' + ImageDesc);
            }
            else {
                newid = (result.split('/')[2] + '~' + ImageDesc);
            }
            $("#B_BarImages_" + i).val(newid);
            //var htm = '';
            //htm += '<div class="item ' + actt + '">';
            //htm += '    <img src="' + result + '"  alt="Img Not Found" style="width:100%;">';
            //htm += '    <span class="carousel-caption">';
            //htm += '    <p> ' + ImageDesc + ' </p>';
            //htm += '    </span>';
            //htm += '</div>';
            //$("#div_carousel_inner").append(htm);

            //$('.carousel').carousel(); // $("#carousel").carousel();
            //htm += '<div class="col-lg-4" id="' + id + '">';
            //htm += '       <div class="thumbnail">';
            //htm += '           <button type="button" style="position:absolute;top:5px;right:11px;padding:0px 5px;cursor:pointer;border-radius:10px;" class="btn-danger" onclick= deleteBarImages("' + id + '")>X</button>';
            //htm += '           <a href="' + result + '" target="_blank">';
            //htm += '                <img src="' + result + '" alt="" style="width:100%">';
            //htm += '                <div class="caption">';
            //htm += '                   ' + ImageDesc;
            //htm += '          </div>';
            //htm += '      </a>';
            //htm += '  </div>';
            //htm += '</div>';
            //$("#divUploadImage").append(htm);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}

function deleteBarImages() {
    debugger;
    var ActiveBarRowIndex = $('#ActiveBarRowIndex').val();
    var id = $("#div_carousel_inner div.active").attr('id')
    var arr = $("#B_BarImages_" + ActiveBarRowIndex).val().split(String.fromCharCode(179)); var deleteindex = 0;
    $.each(arr, function (index, value) {
        var divid = (value.split('~')[0]).split('.')[0];
        if (id == divid) {
            deleteindex = index;
        }
    });
    arr.splice(deleteindex, 1);
    $("#" + id).remove();
    var newimg = arr.join(String.fromCharCode(179));
    $("#B_BarImages_" + ActiveBarRowIndex).val(newimg);
    $("#OpenImageModal_" + ActiveBarRowIndex).html(arr.length);

}
function T_Sale_FillImageModal(index) {
    debugger;
    //var OpenImageModal =
    $('#ActiveBarRowIndex').val(index);
    var actt = ""; $("#div_carousel_inner").html('');
    var arr = $("#B_BarImages_" + index).val();
    arr = arr.split(String.fromCharCode(179));
    $.each(arr, function (index, value) {
        var imgname = (value.split('~')[0]);
        var id = (imgname).split('.')[0];
        var ImageDesc = (value.split('~')[1]);
        var htm = ''; if (index == 0) { actt = "active"; } else { actt = ""; }
        htm += '<div id="' + id + '" class="item ' + actt + '">';
        htm += '    <img src="/UploadDocuments/' + imgname + '"  alt="Img Not Found" style="width:100%;">';
        htm += '    <span class="carousel-caption">';
        htm += '    <p> ' + ImageDesc + ' </p>';
        htm += '    </span>';
        htm += '</div>';
        $("#div_carousel_inner").append(htm);
    });
}
function GetPartyDetails(id) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    debugger;
    if (id == "") {
        ClearAllTextBoxes("SLCD,SLNM,SLAREA,GSTNO,TAXGRPCD,PRCCD,PRCNM,AGSLCD,AGSLNM,DUEDAYS,PSLCD,TCSPER,TDSLIMIT,TDSCALCON,AMT,TCSAPPL,TDSROUNDCAL,TCSCODE,TCSNM");
    }
    else {
        var code = $("#slcd_tag").val() + String.fromCharCode(181) + $("#DOCDT").val();
        var AUTONO = $("#AUTONO").val();
        var TDSCODE = $("#TDSCODE").val();
        $.ajax({
            type: 'POST',
            beforesend: $("#WaitingMode").show(),
            url: $("#UrlSubLedgerDetails").val(),//"@Url.Action("GetSubLedgerDetails", PageControllerName)",
            data: "&val=" + id + "&Code=" + code + "&Autono=" + AUTONO + "&linktdscode=" + TDSCODE,
            success: function (result) {
                var MSG = result.indexOf('#helpDIV');
                if (MSG >= 0) {
                    ClearAllTextBoxes("SLCD,SLNM,SLAREA,GSTNO,TAXGRPCD,PRCCD,PRCNM,AGSLCD,AGSLNM,DUEDAYS,PSLCD,TCSPER,TDSLIMIT,TDSCALCON,AMT,TCSAPPL,TDSROUNDCAL,TCSCODE,TCSNM");
                    $('#SearchFldValue').val("SLCD");
                    $('#helpDIV').html(result);
                    $('#ReferanceFieldID').val("SLCD/SLNM/SLAREA/GSTNO");
                    $('#ReferanceColumn').val("1/0/3/2");
                    $('#helpDIV_Header').html("Party Details");
                }
                else {
                    var MSG = result.indexOf(String.fromCharCode(181));
                    if (MSG >= 0) {
                        $("#SLCD").val(returncolvalue(result, "slcd"));
                        $("#SLNM").val(returncolvalue(result, "slnm"));
                        $("#SLAREA").val(returncolvalue(result, "slarea"));
                        $("#GSTNO").val(returncolvalue(result, "gstno"));
                        $("#TAXGRPCD").val(returncolvalue(result, "TAXGRPCD"));
                        $("#PRCCD").val(returncolvalue(result, "PRCCD"));
                        $("#PRCNM").val(returncolvalue(result, "PRCNM"));
                        $("#AGSLCD").val(returncolvalue(result, "AGSLCD"));
                        $("#AGSLNM").val(returncolvalue(result, "AGSLNM"));
                        $("#DUEDAYS").val(returncolvalue(result, "crdays"));
                        $("#PSLCD").val(returncolvalue(result, "PSLCD"));

                        //tcs
                        $("#TDSCODE").val(returncolvalue(result, "TCSCODE"));
                        $("#TDSNM").val(returncolvalue(result, "TCSNM"));
                        $("#TCSPER").val(returncolvalue(result, "TCSPER"));
                        $("#TDSLIMIT").val(returncolvalue(result, "TDSLIMIT"));
                        $("#TDSCALCON").val(returncolvalue(result, "TDSCALCON"));
                        $("#AMT").val(returncolvalue(result, "AMT"));
                        $("#TCSAPPL").val(returncolvalue(result, "TCSAPPL"));
                        $("#TDSROUNDCAL").val(returncolvalue(result, "TDSROUNDCAL"));
                        BillAmountCalculate();//fill value of tcson
                        //
                    }
                    else {
                        $('#helpDIV').html("");
                        msgInfo("" + result + " !");
                        ClearAllTextBoxes("SLCD,SLNM,SLAREA,GSTNO,TAXGRPCD,PRCCD,PRCNM,AGSLCD,AGSLNM,DUEDAYS,PSLCD,TCSPER,TDSLIMIT,TDSCALCON,AMT,TCSAPPL,TDSROUNDCAL,TCSCODE,TCSNM");
                        message_value = "SLCD";
                    }
                }
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
function GetTCSON(billamount) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    var TCSON = 0;
    if ($("#TCSAPPL").val() != "Y") {
        TCSON = parseFloat(0);
    }
    else {
        var tdslimit = $("#TDSLIMIT").val();
        if (tdslimit == "") { tdslimit = parseFloat(0); } else { tdslimit = parseFloat(tdslimit) }

        var amt = $("#AMT").val();
        if (amt == "") { amt = parseFloat(0); } else { amt = parseFloat(amt) }

        var blamt = billamount;//$("#BILL_AMT").val();
        if (blamt == "") { blamt = parseFloat(0); } else { blamt = parseFloat(blamt) }

        var tcsamt = $("#TCSAMT").val();
        if (tcsamt == "") { tcsamt = parseFloat(0); } else { tcsamt = parseFloat(tcsamt) }

        var totaltaxableamt = $("#TOTTAXVAL").val();
        if (totaltaxableamt == "") { totaltaxableamt = parseFloat(0); } else { totaltaxableamt = parseFloat(totaltaxableamt) }

        var tdscalcon = $("#TDSCALCON").val();

        var TL = tdslimit;	//tdslimit from m_tds_cntrl_dtl
        var O = amt;	//amt from slcdtcscalcon func
        var B = blamt;//parseFloat(blamt - tcsamt);//	blamt - tcsamt
        var T = totaltaxableamt;//	total taxable amt


        if (tdscalcon == "B") {
            TCSON = parseFloat(parseFloat(O) + parseFloat(B) - parseFloat(TL));
        }
        else {// tdscalcon = "T"
            TCSON = parseFloat(parseFloat(O) + parseFloat(T) - parseFloat(TL));
        }

        if (TCSON < 0) TCSON = parseFloat(0);
    }
    $("#TCSON").val(parseFloat(TCSON).toFixed(2));
}
function CalculateTcsAmt(TCSAMT) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var TDSROUNDCAL = $("#TDSROUNDCAL").val();
    if (TDSROUNDCAL == "Y" || TDSROUNDCAL == "") {
        TCSAMT = Math.round(TCSAMT); //==yes or null or blank
    }
    else
        if (TDSROUNDCAL == "N") {
            TCSAMT = Math.ceil(TCSAMT); //==nxt
        }
        else
            if (TDSROUNDCAL == "L") {
                TCSAMT = Math.floor(TCSAMT); //==least
            }
            else {
                TCSAMT = TCSAMT; //==2
            }
    return TCSAMT;
}
function HasChangeBarSale() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    $("#bardatachng").val("Y");
}
function GetPendOrder(BtnId) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var slcd = $("#SLCD").val();
    var itcd = $("#ITCD").val();
    $.ajax({
        type: 'POST',
        url: $("#UrlPendOrder").val(),// "@Url.Action("GetPendOrder", PageControllerName )",
        beforesend: $("#WaitingMode").show(),
        data: $('form').serialize() + "&SLCD=" + slcd + "&SUBMITBTN=" + BtnId + "&ITCD=" + itcd,
        success: function (result) {
            $("#popup").animate({ marginTop: '-10px' }, 50);
            $("#popup").html(result);
            if (BtnId == "SHOWBTN") {
                $("#btnSelectPendOrder").hide();
                $("#allchkord").attr('disabled', true);
                var GridRow = $("#_T_SALE_PENDINGORDER_GRID > tbody > tr").length;
                for (var i = 0; i <= GridRow - 1; i++) {
                    $("#Ord_Checked_" + i).attr('disabled', true);
                }
            }
            $("#WaitingMode").hide();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
function SelectPendOrder() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var Count = 0;
    var GridRow = $("#_T_SALE_PENDINGORDER_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var Check = document.getElementById("Ord_Checked_" + i).checked;
        if (Check == true) {
            Count = Count + 1;
        }
    }
    if (Count > 0) {
        $("#haspendorddata").val("Y");
        //$("#show_order").show();
    }
    else {
        msgInfo("Please select Order !");
        return false;
    }
    $.ajax({
        type: 'post',
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlSelectPendOrder").val(),//"@Url.Action("SelectPendOrder", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            $("#WaitingMode").hide();
            $("#hiddenpendordJSON").val(result);
            $("#Pending_Order").hide();
            if (Count > 0) {
                $("#show_order").show();
            }
            $("#popup").html("");
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
function retStr(val) {
    var rtval = "";
    if (val == null) rtval = "";
    else if (String(val) == "") rtval = "";
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
function CharmPrice(ChrmType, Rate, RoundVal) {
    debugger;
    RoundVal = RoundVal.padStart(2, '0');
    if (Rate < 10) Rate = 10 + Rate;
    var RoundAmt = retInt(RoundVal);
    if (RoundAmt == 0) RoundAmt = 100;
    var small = ((Rate / RoundAmt) * RoundAmt);
    var big = small + RoundAmt;
    if (ChrmType == "RD")//ROUND
    {
        var roundValu = (Rate - small >= big - Rate) ? big : small;
        return roundValu;
    }
    else if (ChrmType == "RN")//ROUNDNEXT
    {
        return big;
    }
    else if (ChrmType == "NT")//NEXT
    {
        var last2rate = (Rate % 100);
        if (last2rate <= retInt(RoundVal)) {
            big = retInt(retStr(Rate).substring(0, retStr(Rate).length - 2) + RoundVal);
        }
        else {
            big = retInt(retStr(Rate + 100).substring(0, retStr(Rate + 100).length - 2) + RoundVal);
        }
        return retInt(big);
    }
    else if (ChrmType == "NR")//NEAR
    {
        var last2rate = (Rate % 100);
        if (last2rate <= retInt(RoundVal)) {
            big = retInt(retStr(Rate).substring(0, retStr(Rate).length - 2) + RoundVal);
            small = retInt(retStr(Rate - 100).substring(0, retStr(Rate - 100).length - 2) + RoundVal);
        }
        else {
            big = retInt(retStr(Rate + 100).substring(0, retStr(Rate + 100).length - 2) + RoundVal);
            small = retInt(retStr(Rate).substring(0, retStr(Rate).length - 2) + RoundVal);
        }
        var NEAR = (Rate - small >= big - Rate) ? big : small;
        return retInt(NEAR);
    }
    else {
        return 0;
    }
}

function RateHistoryDetails(i) {
    $.ajax({
        type: 'get',
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlRateHistory").val(),
        data: "ITCD=" + $('#B_ITCD_' + i).val(),
        success: function (result) {
            $("#WaitingMode").hide();
            $("#RateHistoryModal").html(result);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}

