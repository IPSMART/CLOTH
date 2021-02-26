function GetAllMtrljobcd() {
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
function GetBarnoDetails(id, HelpFrom) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    if (id == "") {
        ClearBarcodeArea();
    }
    else {
        if (!emptyFieldCheck("Please Select / Enter Document Date", "DOCDT")) { return false; }
        if ($("#TAXGRPCD").val() == "") { $("#BARCODE").val(""); msgInfo("TaxGrp. Code not available.Please Select / Enter another Party Code to get TaxGrp. Code"); message_value = "SLCD"; return false; }
        if ($("#PRCCD").val() == "") { $("#BARCODE").val(""); msgInfo("Price Code not available.Please Select / Enter another Party Code to get Price Code"); message_value = "SLCD"; return false; }
        if ($("#GOCD").val() == "") { $("#GOCD").val(""); msgInfo("Godown not available.Please Select / Enter Godown"); message_value = "GOCD"; return false; }
        var MTRLJOBCD = $("#MTRLJOBCD").val();
        var PARTCD = $("#PARTCD").val();
        var docdt = $("#DOCDT").val();
        var taxgrpcd = $("#TAXGRPCD").val();
        var gocd = $("#GOCD").val();
        var prccd = $("#PRCCD").val();
        var allmtrljobcd = $("#ALLMTRLJOBCD").val();
        var BARCODE = $("#BARCODE").val();
        var AUTONO = $("#AUTONO").val();
        var code = MTRLJOBCD + String.fromCharCode(181) + PARTCD + String.fromCharCode(181) + docdt + String.fromCharCode(181) + taxgrpcd + String.fromCharCode(181) + gocd + String.fromCharCode(181) + prccd + String.fromCharCode(181) + allmtrljobcd + String.fromCharCode(181) + HelpFrom + String.fromCharCode(181) + BARCODE + String.fromCharCode(181) + AUTONO;

        var hlpfieldid = "", hlpfieldindex = "", ReferanceFieldID = "", ReferanceFieldIndex = "";
        if (HelpFrom == "Bar") {
            hlpfieldid = "BARCODE";
            hlpfieldindex = "0";
            ReferanceFieldID = "/STYLENO";
            ReferanceFieldIndex = "/3";
        }
        else {
            hlpfieldid = "STYLENO";
            hlpfieldindex = "3";
            ReferanceFieldID = "/BARCODE";
            ReferanceFieldIndex = "/0";
        }
        if ($("#Barnohelpopen").val() == "Y") {
            $.ajax({
                type: 'POST',
                beforesend: $("#WaitingMode").show(),
                url: $("#UrlBarnoDetails").val(),//"@Url.Action("GetBarCodeDetails", PageControllerName)",
                data: "&val=" + id + "&Code=" + code,
                success: function (result) {
                    var MSG = result.indexOf('#helpDIV');
                    if (MSG >= 0) {
                        ClearBarcodeArea();
                        $('#SearchFldValue').val(hlpfieldid);
                        $('#helpDIV').html(result);
                        $('#ReferanceFieldID').val(hlpfieldid + ReferanceFieldID + '/PARTCD');
                        $('#ReferanceColumn').val(hlpfieldindex + ReferanceFieldIndex + '/5');
                        $('#helpDIV_Header').html('Barno Details');
                    }
                    else {
                        var MSG = result.indexOf(String.fromCharCode(181));
                        if (MSG >= 0) {
                            var value1 = modify_check_stylebarno();
                            if (value1 == "true") {
                                FillBarcodeArea(result);
                                changeBARGENTYPE();
                                var value = modify_check();
                                if (value == "true") {
                                    RateHistoryDetails('ITCD', 'ITSTYLE', 'GRID');
                                }
                                if ((HelpFrom == "Bar") && (MENU_PARA == "SBDIR" || MENU_PARA == "SBPCK" || MENU_PARA == "PR") && ($("#QNTY").val() != "")) {
                                    AddBarCodeGrid();
                                }
                            }
                            else {
                                if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
                                    $("#WPPRICEGEN").val(returncolvalue(result, "WPPRICEGEN"));
                                    $("#RPPRICEGEN").val(returncolvalue(result, "RPPRICEGEN"));
                                }
                                if (MENU_PARA != "PB" && MENU_PARA != "SB" && MENU_PARA != "OP" && MENU_PARA != "OTH") {
                                    $("#MTBARCODE").val(returncolvalue(result, "MTBARCODE"));
                                }
                                $("#PRTBARCODE").val(returncolvalue(result, "PRTBARCODE"));
                                $("#CLRBARCODE").val(returncolvalue(result, "CLRBARCODE"));
                                $("#SZBARCODE").val(returncolvalue(result, "SZBARCODE"));
                                $("#NEGSTOCK").val(returncolvalue(result, "NEGSTOCK"));
                                $("#PRODGRPGSTPER").val(returncolvalue(result, "PRODGRPGSTPER"));
                                $("#GLCD").val(returncolvalue(result, "GLCD"));
                            }

                        }
                        else {
                            $('#helpDIV').html("");
                            msgInfo("" + result + " !");
                            ClearBarcodeArea();
                            message_value = hlpfieldid;
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
        else {
            $("#Barnohelpopen").val("Y");
            var value = modify_check();
            if (value == "true") {
                RateHistoryDetails('ITCD', 'ITSTYLE', 'GRID');
            }
        }
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
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    var MENU_PARA = $("#MENU_PARA").val();
    var MNTNLISTPRICE = $("#MNTNLISTPRICE").val();
    if (DefaultAction == "V") return true;
    if (Table == "COPYLROW") {
        if (event.key != "F8") {
            return true;
        }
        else {
            i = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length - 1;
            $("#Barnohelpopen").val("N");
        }
    }
    if (str != "") {
        $("#BARCODE").val(returncolvalue(str, "BARNO"));
        $("#Last_BARCODE").val(returncolvalue(str, "BARNO"));
        $("#TXNSLNO").val(returncolvalue(str, "TXNSLNO"));
        $("#ITGRPCD").val(returncolvalue(str, "ITGRPCD"));
        $("#ITGRPNM").val(returncolvalue(str, "ITGRPNM"));
        //if (MENU_PARA != "PB" && MENU_PARA != "SB" && MENU_PARA != "OP" && MENU_PARA != "OTH") {
        if (retStr(returncolvalue(str, "MTRLJOBCD")) != "") {
            $("#MTRLJOBCD").val(returncolvalue(str, "MTRLJOBCD"));
            $("#MTRLJOBNM").val(returncolvalue(str, "MTRLJOBNM"));
            $("#MTBARCODE").val(returncolvalue(str, "MTBARCODE"));
        }
        $("#ITCD").val(returncolvalue(str, "ITCD"));
        $("#ITSTYLE").val(returncolvalue(str, "STYLENO") + "" + returncolvalue(str, "ITNM"));
        $("#STYLENO").val(returncolvalue(str, "STYLENO"));
        $("#Last_STYLENO").val(returncolvalue(str, "STYLENO"));
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
        if (MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "SBPCK") {
            if (retStr(returncolvalue(str, "uomcd")) == "PCS") {
                $("#QNTY").val(1.000);
                $("#NOS").val(returncolvalue(str, "NOS"));
            }
            else {
                $("#QNTY").val(returncolvalue(str, "QNTY"));
                $("#NOS").val(1);
            }
        }
        else {
            $("#QNTY").val(returncolvalue(str, "QNTY"));
            $("#NOS").val(returncolvalue(str, "NOS"));
        }
        $("#UOM").val(returncolvalue(str, "uomcd"));
        $("#CUTLENGTH").val(returncolvalue(str, "CUTLENGTH"));
        $("#FLAGMTR").val(returncolvalue(str, "FLAGMTR"));
        var RATE = returncolvalue(str, "RATE");
        var PRODGRPGSTPER = returncolvalue(str, "PRODGRPGSTPER");
        var GSTPERstr = retGstPerstr(PRODGRPGSTPER, RATE);
        var GSTPERarr = GSTPERstr.split(','); var GSTPER = 0;
        $.each(GSTPERarr, function () {
            GSTPER += parseFloat(this) || 0;
        });

        $("#RATE").val(RATE);
        $("#GSTPER").val(GSTPER);
        $("#PRODGRPGSTPER").val(PRODGRPGSTPER);
        $("#DISCRATE").val(returncolvalue(str, "DISCRATE"));
        //$("#DISCTYPE").val(returncolvalue(str, "DISCTYPE"));
        $("#HSNCODE").val(returncolvalue(str, "HSNCODE"));

        $("#SHADE").val(returncolvalue(str, "SHADE"));
        $("#BALENO").val(returncolvalue(str, "BALENO"));
        if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
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
        $("#BarImages").val(returncolvalue(str, "BARIMAGE"));
        if ((MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "PI") && MNTNLISTPRICE == "Y") {
            $("#LISTPRICE").val(RATE);
        }
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
            $("#CUTLENGTH").val($(FieldidStarting + "CUTLENGTH_" + i).val());
        }
        else {
            if (MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "SBPCK") {
                if (retStr($(FieldidStarting + "UOM_" + i).val()) == "PCS") {
                    $("#QNTY").val(1.000);
                }
                else {
                    $("#NOS").val(1);
                }
            }
        }
        $("#DISCRATE").val($(FieldidStarting + "DISCRATE_" + i).val());
        $("#DISCTYPE").val($(FieldidStarting + "DISCTYPE_" + i).val());
        $("#TDDISCTYPE").val($(FieldidStarting + "TDDISCTYPE_" + i).val());
        $("#TDDISCRATE").val($(FieldidStarting + "TDDISCRATE_" + i).val());
        $("#SCMDISCTYPE").val($(FieldidStarting + "SCMDISCTYPE_" + i).val());
        $("#SCMDISCRATE").val($(FieldidStarting + "SCMDISCRATE_" + i).val());
        if ((Table != "COPYLROW") && (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH")) {
            $("#ORDDOCNO").val($(FieldidStarting + "ORDDOCNO_" + i).val());
            $("#ORDDOCDT").val($(FieldidStarting + "ORDDOCDT_" + i).val());
            $("#ORDAUTONO").val($(FieldidStarting + "ORDAUTONO_" + i).val());
            $("#ORDSLNO").val($(FieldidStarting + "ORDSLNO_" + i).val());
        }
        $("#BALSTOCK").val($(FieldidStarting + "BALSTOCK_" + i).val());
        $("#NEGSTOCK").val($(FieldidStarting + "NEGSTOCK_" + i).val());

        $("#BARCODE").val($(FieldidStarting + "BARNO_" + i).val());
        $("#Last_BARCODE").val($(FieldidStarting + "BARNO_" + i).val());
        $("#ITGRPCD").val($(FieldidStarting + "ITGRPCD_" + i).val());
        $("#ITGRPNM").val($(FieldidStarting + "ITGRPNM_" + i).val());
        $("#MTRLJOBCD").val($(FieldidStarting + "MTRLJOBCD_" + i).val());
        $("#MTRLJOBNM").val($(FieldidStarting + "MTRLJOBNM_" + i).val());
        $("#MTBARCODE").val($(FieldidStarting + "MTBARCODE_" + i).val());
        $("#ITCD").val($(FieldidStarting + "ITCD_" + i).val());
        $("#ITSTYLE").val($(FieldidStarting + "ITSTYLE_" + i).val().trim());
        $("#STYLENO").val($(FieldidStarting + "STYLENO_" + i).val());
        $("#Last_STYLENO").val($(FieldidStarting + "STYLENO_" + i).val());
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
        $("#BALENO").val($(FieldidStarting + "BALENO_" + i).val());
        if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
            $("#OURDESIGN").val($(FieldidStarting + "OURDESIGN_" + i).val());
            $("#PDESIGN").val($(FieldidStarting + "PDESIGN_" + i).val());
            $("#WPPRICEGEN").val($(FieldidStarting + "WPPRICEGEN_" + i).val());
            $("#RPPRICEGEN").val($(FieldidStarting + "RPPRICEGEN_" + i).val());
            if (FieldidStarting == "#B_") {
                $("#BarImages").val($(FieldidStarting + "BarImages_" + i).val());
            }
        }
        $("#LOCABIN").val($(FieldidStarting + "LOCABIN_" + i).val());
        $("#UOM").val($(FieldidStarting + "UOM_" + i).val());
        $("#GLCD").val($(FieldidStarting + "GLCD_" + i).val());

        if (MENU_PARA == "SR" || MENU_PARA == "PR") {
            $("#AGDOCNO").val($(FieldidStarting + "AGDOCNO_" + i).val());
            $("#AGDOCDT").val($(FieldidStarting + "AGDOCDT_" + i).val());
        }
        if ((MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "PI") && MNTNLISTPRICE == "Y") {
            $("#LISTPRICE").val($(FieldidStarting + "LISTPRICE_" + i).val());
            $("#LISTDISCPER").val($(FieldidStarting + "LISTDISCPER_" + i).val());

        }
    }
    if (Table == "_T_SALE_PRODUCT_GRID") {
        $("#AddRow_Barcode").hide();
        $("#UpdateRow_Barcode").prop("value", "Update Row [" + $(FieldidStarting + "SLNO_" + i).val() + "] (Alt+W)");
        $("#UpdateRow_Barcode").show();
    }
    changeBARGENTYPE();
    var ModuleCode = $("#ModuleCode").val();
    if (event.key == "F8") {
        if (ModuleCode.indexOf("SALESCLOTH") != -1) {
            $("#CUTLENGTH").focus();
        }
        else {
            if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
                $("#ITCD").focus();
            } else {
                if ($("#MNTNBARNO").val() == "Y") {
                    $("#BARCODE").focus();
                }
                else {
                    $("#STYLENO").focus();
                }
            }
        }

    }
    else if (str != "") {
        if (ModuleCode.indexOf("SALESCLOTH") != -1) {
            if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
                $("#NOS").focus();
            }
            else {
                $("#CUTLENGTH").focus();
            }
        }
    }
    if (event.key == "F8" || str == "") {
        var value = modify_check();
        if (value == "true") {
            RateHistoryDetails('ITCD', 'ITSTYLE', 'GRID');
        }
    }
}
function changeBARGENTYPE() {
    var BARGENTYPE = $("#BARGENTYPE").val();
    var GRPBARGENTYPE = $("#BARGENTYPETEMP").val();

    if (BARGENTYPE == "E" || GRPBARGENTYPE == "E") {
        $("#divImageUpload").show();
    }
    else if (BARGENTYPE == "C") {
        $("#divImageUpload").hide();
    }

    return 0;
}

function UpdateBarCodeRow() {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    var MENU_PARA = $("#MENU_PARA").val();
    if (DefaultAction == "V") return true;
    var MNTNLISTPRICE = $("#MNTNLISTPRICE").val();
    var MNTNOURDESIGN = $("#MNTNOURDESIGN").val();
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
    if (MENU_PARA != "SR" && MENU_PARA != "SBCMR" && MENU_PARA != "PB" && MENU_PARA != "OP" && MENU_PARA != "OTH" && retFloat($("#QNTY").val()) != 0) {
        var NEGSTOCK = $("#NEGSTOCK").val();
        var BALSTOCK = $("#BALSTOCK").val();
        if (BALSTOCK == "") { BALSTOCK = parseFloat(0); } else { BALSTOCK = parseFloat(BALSTOCK); }
        var QNTY = $("#QNTY").val();
        if (QNTY == "") { QNTY = parseFloat(0); } else { QNTY = parseFloat(QNTY); }
        var balancestock = BALSTOCK - QNTY;
        if (balancestock < 0) {
            if (NEGSTOCK != "Y") {
                ClearAllTextBoxes("CUTLENGTH,NOS,QNTY");
                msgInfo("Quantity should not be grater than Stock !");
                message_value = "QNTY";
                return false;
            }
        }
    }
    //get bill slno
    var TXNSLNO = "";
    if ($("#TXNSLNO").val() == "" || $("#TXNSLNO").val() == "0") {
        var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
        if (GridRowMain == 0) {
            TXNSLNO = 1;
        }
        else {
            var allslno = [];
            var matchslno = [];
            countmatchslno = 0;
            for (j = 0; j <= GridRowMain - 1; j++) {
                var flag = true;
                if (MENU_PARA == "SR" || MENU_PARA == "PR") {
                    if (retStr($("#AGDOCNO").val()) != retStr($("#B_AGDOCNO_" + j).val()) || retStr($("#AGDOCDT").val()) != retStr($("#B_AGDOCDT_" + j).val())) {
                        flag = false;
                    }
                }
                if ((MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "PI") && MNTNLISTPRICE == "Y") {
                    if (retFloat($("#LISTPRICE").val()) != retFloat($("#B_LISTPRICE_" + j).val()) || retFloat($("#LISTDISCPER").val()) != retFloat($("#B_LISTDISCPER_" + j).val())) {
                        flag = false;
                    }
                }
                //if (MENU_PARA == "PB" || MENU_PARA == "OP") {
                if (retStr($("#BALENO").val()) != retStr($("#B_BALENO_" + j).val())) {
                    flag = false;
                }
                //}
                if (flag == true) {

                    if (retStr($("#ITGRPCD").val()) == retStr($("#B_ITGRPCD_" + j).val()) && retStr($("#MTRLJOBCD").val()) == retStr($("#B_MTRLJOBCD_" + j).val()) &&
                                    retStr($("#MTBARCODE").val()) == retStr($("#B_MTBARCODE_" + j).val()) && retStr($("#ITCD").val()) == retStr($("#B_ITCD_" + j).val()) &&
                                    retStr($("#DISCTYPE").val()) == retStr($("#B_DISCTYPE_" + j).val()) && retStr($("#TDDISCTYPE").val()) == retStr($("#B_TDDISCTYPE_" + j).val()) &&
                                     retStr($("#SCMDISCTYPE").val()) == retStr($("#B_SCMDISCTYPE_" + j).val()) && retStr($("#UOM").val()) == retStr($("#B_UOM_" + j).val()) && retStr($("#STKTYPE").val()) == retStr($("#B_STKTYPE_" + j).val()) && retFloat($("#RATE").val()) == retFloat($("#B_RATE_" + j).val()) &&
                                    retFloat($("#DISCRATE").val()) == retFloat($("#B_DISCRATE_" + j).val()) && retFloat($("#SCMDISCRATE").val()) == retFloat($("#SCMDISCRATE").val()) && retFloat($("#TDDISCRATE").val()) == retFloat($("#TDDISCRATE").val()) && retFloat($("#GSTPER").val()) == retFloat($("#GSTPER").val()) &&
                                    retFloat($("#FLAGMTR").val()) == retFloat($("#B_FLAGMTR_" + j).val()) && retStr($("#HSNCODE").val()) == retStr($("#B_HSNCODE_" + j).val()) && retStr($("#PRODGRPGSTPER").val()) == retStr($("#B_PRODGRPGSTPER_" + j).val()) &&
                                    retStr($("#GLCD").val()) == retStr($("#B_GLCD_" + j).val()) && retStr($("#SLNO").val()) != retStr($("#B_SLNO_" + j).val())) {

                        matchslno[countmatchslno] = retInt($("#B_TXNSLNO_" + j).val());
                        countmatchslno++;
                    }
                }
                allslno[j] = retInt($("#B_TXNSLNO_" + j).val());
            }

            if (matchslno.length > 0) {
                TXNSLNO = Math.max.apply(Math, matchslno);
                if (TXNSLNO == 0) {
                    TXNSLNO = Math.max.apply(Math, allslno);
                    TXNSLNO++;
                }
            }
            else {
                TXNSLNO = Math.max.apply(Math, allslno);
                TXNSLNO++;
            }
        }
    }
    else {
        TXNSLNO = retInt($("#TXNSLNO").val());
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
            $("#B_BARGENTYPE_" + j).val($("#BARGENTYPETEMP").val());
            $("#B_MTRLJOBCD_" + j).val($("#MTRLJOBCD").val());
            $("#B_MTRLJOBNM_" + j).val($("#MTRLJOBNM").val());
            $("#B_MTBARCODE_" + j).val($("#MTBARCODE").val());
            $("#B_ITCD_" + j).val($("#ITCD").val());
            $("#B_ITSTYLE_" + j).val($("#ITSTYLE").val().trim());
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
            $("#B_CUTLENGTH_" + j).val($("#CUTLENGTH").val());
            $("#B_FLAGMTR_" + j).val($("#FLAGMTR").val());
            $("#B_RATE_" + j).val($("#RATE").val());
            $("#B_DISCRATE_" + j).val($("#DISCRATE").val());
            $("#B_DISCTYPE_" + j).val($("#DISCTYPE").val());
            $("#B_HSNCODE_" + j).val($("#HSNCODE").val());
            $("#B_GSTPER_" + j).val($("#GSTPER").val());
            $("#B_PRODGRPGSTPER_" + j).val($("#PRODGRPGSTPER").val());
            $("#B_SHADE_" + j).val($("#SHADE").val());
            $("#B_BALENO_" + j).val($("#BALENO").val());
            if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
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
            var DISCTYPE = $("#DISCTYPE").val() == "P" ? "%" : $("#DISCTYPE").val() == "N" ? "Nos" : $("#DISCTYPE").val() == "Q" ? "Qnty" : $("#DISCTYPE").val() == "A" ? "AftDsc%" : "Fixed";
            var TDDISCTYPE = $("#TDDISCTYPE").val() == "P" ? "%" : $("#TDDISCTYPE").val() == "N" ? "Nos" : $("#TDDISCTYPE").val() == "Q" ? "Qnty" : $("#TDDISCTYPE").val() == "A" ? "AftDsc%" : "Fixed";
            var SCMDISCTYPE = $("#SCMDISCTYPE").val() == "P" ? "%" : $("#SCMDISCTYPE").val() == "N" ? "Nos" : $("#SCMDISCTYPE").val() == "Q" ? "Qnty" : $("#SCMDISCTYPE").val() == "A" ? "AftDsc%" : "Fixed";
            $("#B_DISCTYPE_DESC_" + j).val(DISCTYPE);
            $("#B_TDDISCTYPE_DESC_" + j).val(TDDISCTYPE);
            $("#B_SCMDISCTYPE_DESC_" + j).val(SCMDISCTYPE);
            $("#B_GLCD_" + j).val($("#GLCD").val());
            if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
                $("#B_ORDDOCNO_" + j).val($("#ORDDOCNO").val());
                $("#B_ORDAUTONO_" + j).val($("#ORDAUTONO").val());
                $("#B_ORDSLNO_" + j).val($("#ORDSLNO").val());
                $("#B_ORDDOCDT_" + j).val($("#ORDDOCDT").val());
            }
            if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {

                var BarImages = $("#BarImages").val();
                if ((MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") && ($("#BARGENTYPE").val() == "E" || $("#B_BARGENTYPE_" + j).val() == "E")) {
                    BarImages = "";
                }
                var NoOfBarImages = BarImages.split(String.fromCharCode(179)).length;
                if (BarImages == '') {
                    NoOfBarImages = '';
                }
                $("#BarImagesCount_" + j).val(NoOfBarImages);
                $("#B_BarImages_" + j).val(BarImages);

                if ($("#BARGENTYPE").val() == "E" || $("#B_BARGENTYPE_" + j).val() == "E") {
                    $("#UploadBarnoImage_" + j).show();
                }
                else {
                    $("#UploadBarnoImage_" + j).hide();
                }
                RateUpdate(j, '#B_');

            }
            if (MENU_PARA == "SR" || MENU_PARA == "PR") {
                $("#B_AGDOCNO_" + j).val($("#AGDOCNO").val());
                $("#B_AGDOCDT_" + j).val($("#AGDOCDT").val());
            }
            if ((MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "PI") && MNTNLISTPRICE == "Y") {
                $("#B_LISTPRICE_" + j).val($("#LISTPRICE").val());
                $("#B_LISTDISCPER_" + j).val($("#LISTDISCPER").val());
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
    if (MNTNOURDESIGN != "Y") {
        if ($("#MNTNBARNO").val() == "Y") {
            $("#BARCODE").focus();
        }
        else {
            $("#STYLENO").focus();
        }
    }
    else {
        if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
            $("#ITCD").focus();
        } else {
            if ($("#MNTNBARNO").val() == "Y") {
                $("#BARCODE").focus();
            }
            else {
                $("#STYLENO").focus();
            }
        }
    }
    $("#bardatachng").val("Y");
}

function ClearBarcodeArea(TAG) {
    var DefaultAction = $("#DefaultAction").val();
    var MENU_PARA = $("#MENU_PARA").val();
    if (DefaultAction == "V") return true;
    var MNTNLISTPRICE = $("#MNTNLISTPRICE").val();
    ClearAllTextBoxes("BARCODE,TXNSLNO,ITGRPCD,ITGRPNM,ITCD,ITSTYLE,STYLENO,STKTYPE,PARTCD,PARTNM,PRTBARCODE,COLRCD,COLRNM,CLRBARCODE,SIZECD,SIZENM,SZBARCODE,BALSTOCK,QNTY,UOM,GLCD,NOS,CUTLENGTH,FLAGMTR,RATE,DISCRATE,HSNCODE,GSTPER,PRODGRPGSTPER,SHADE,TDDISCRATE,SCMDISCRATE,LOCABIN,BARGENTYPETEMP,NEGSTOCK,BALENO,Last_STYLENO,Last_BARCODE");
    if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
        ClearAllTextBoxes("OURDESIGN,PDESIGN,WPPRICEGEN,RPPRICEGEN");
    }
    if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
        ClearAllTextBoxes("ORDDOCNO,ORDDOCDT,ORDAUTONO,ORDSLNO");
    }
    $("#STKTYPE").val("F");
    $("#DISCTYPE").val("A");
    $("#TDDISCTYPE").val("A");
    $("#SCMDISCTYPE").val("P");
    if (TAG == "Y") {
        $("#AddRow_Barcode").show();
        $("#UpdateRow_Barcode").hide();
        //if (MENU_PARA == "PB" || MENU_PARA == "OP") {
        //    $("#ITCD").focus();
        //} else {
        if ($("#MNTNBARNO").val() == "Y") {
            $("#BARCODE").focus();
        }
        else {
            $("#STYLENO").focus();
        }
        //}
    }
    if (MENU_PARA == "SR" || MENU_PARA == "PR") {
        ClearAllTextBoxes("AGDOCNO,AGDOCDT");
    }
    if ((MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "PI") && MNTNLISTPRICE == "Y") {
        ClearAllTextBoxes("LISTPRICE,LISTDISCPER");
    }
    if (MENU_PARA != "PB" && MENU_PARA != "SB" && MENU_PARA != "OP" && MENU_PARA != "OTH") {
        ClearAllTextBoxes("MTRLJOBCD,MTRLJOBNM,MTBARCODE");
    }
}
function Fill_DetailData() {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var GridRow = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    var mtrlcdblank = false; var slnoblank = false; var glcdblank = false; var qntyblank = false;
    if (GridRow != 0) {
        for (var i = 0; i <= GridRow - 1; i++) {
            if ($("#B_MTRLJOBCD_" + i).val() == "") {
                mtrlcdblank = true;
                break;
            }
            if ($("#B_TXNSLNO_" + i).val() == "") {
                slnoblank = true;
                break;
            }
            if ($("#B_GLCD_" + i).val() == "") {
                glcdblank = true;
                break;
            }
            if (retFloat($("#B_QNTY_" + i).val()) == 0) {
                qntyblank = true;
                break;
            }
        }
    }
    if (mtrlcdblank == true) {
        msgInfo("Please Fill Material Job in Barcode Grid !!");
        $("li").removeClass("active").addClass("");
        $(".nav-tabs li:nth-child(2)").addClass('active');
        //below set the  child sequence
        $(".tab-content div").removeClass("active");
        $(".tab-content div:nth-child(2)").removeClass("tab-pane fade").addClass("tab-pane fade in active");
        message_value = "B_MTRLJOBCD_" + i;
        return false;
    }
    if (slnoblank == true) {
        msgInfo("Please Fill Bill Sl # in Barcode Grid !!");
        $("li").removeClass("active").addClass("");
        $(".nav-tabs li:nth-child(2)").addClass('active');
        //below set the  child sequence
        $(".tab-content div").removeClass("active");
        $(".tab-content div:nth-child(2)").removeClass("tab-pane fade").addClass("tab-pane fade in active");
        message_value = "B_TXNSLNO_" + i;
        return false;
    }
    if (glcdblank == true) {
        var slno = $("#B_SLNO_" + i).val();
        msgInfo("Glcd blank in Barcode Grid at slno " + slno + " !!Please add glcd in Group Master ");
        $("li").removeClass("active").addClass("");
        $(".nav-tabs li:nth-child(2)").addClass('active');
        //below set the  child sequence
        $(".tab-content div").removeClass("active");
        $(".tab-content div:nth-child(2)").removeClass("tab-pane fade").addClass("tab-pane fade in active");
        message_value = "B_TXNSLNO_" + i;
        return false;
    }
    if (qntyblank == true) {
        var slno = $("#B_SLNO_" + i).val();
        msgInfo("Please Fill Quantity in Barcode Grid at slno " + slno + " !!");
        $("li").removeClass("active").addClass("");
        $(".nav-tabs li:nth-child(2)").addClass('active');
        //below set the  child sequence
        $(".tab-content div").removeClass("active");
        $(".tab-content div:nth-child(2)").removeClass("tab-pane fade").addClass("tab-pane fade in active");
        message_value = "B_QNTY_" + i;
        return false;
    }
    $.ajax({
        type: 'POST',
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlFillDetailData").val(),//"@Url.Action("FillDetailData", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            if (result.indexOf("_T_SALE_DETAIL_GRID") == -1) {
                msgInfo("Bill Sl (" + result + ") duplicate in barcode tab!");
                $("#WaitingMode").hide();
                $("li").removeClass("active").addClass("");
                $(".nav-tabs li:nth-child(2)").addClass('active');
                //below set the  child sequence
                $(".tab-content div").removeClass("active");
                $(".tab-content div:nth-child(2)").removeClass("tab-pane fade").addClass("tab-pane fade in active");
                message_value = "FillDetail";
                return false;
            }
            else {
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
function UpdateBarCodeRow_FrmDet(i) {
    var MENU_PARA = $("#MENU_PARA").val();
    var DefaultAction = $("#DefaultAction").val();
    var ModuleCode = $("#ModuleCode").val();
    if (DefaultAction == "V") return true;
    var MNTNLISTPRICE = $("#MNTNLISTPRICE").val();
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
    //if (MENU_PARA == "PB" || MENU_PARA == "OP") {
    var BALENO = $("#D_BALENO_" + i).val();
    //}
    var ITREM = $("#D_ITREM_" + i).val();
    var GSTPER = (parseFloat($("#D_IGSTPER_" + i).val()) || 0) + (parseFloat($("#D_CGSTPER_" + i).val()) || 0) + (parseFloat($("#D_SGSTPER_" + i).val()) || 0);
    var LISTPRICE = $("#D_LISTPRICE_" + i).val();
    var LISTDISCPER = $("#D_LISTDISCPER_" + i).val();
    var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    for (j = 0; j <= GridRowMain - 1; j++) {
        if ($("#B_TXNSLNO_" + j).val() == TXNSLNO && $("#B_ITGRPCD_" + j).val() == ITGRPCD && $("#B_MTRLJOBCD_" + j).val() == MTRLJOBCD && $("#B_ITCD_" + j).val() == ITCD && $("#B_STKTYPE_" + j).val() == STKTYPE) {
            if ((MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "PI") && MNTNLISTPRICE == "Y") {
                $("#B_LISTPRICE_" + j).val(LISTPRICE);
                $("#B_LISTDISCPER_" + j).val(LISTDISCPER);
            }
            $("#B_RATE_" + j).val(RATE);
            RateUpdate(j, '#B_');
            $("#B_DISCTYPE_" + j).val(DISCTYPE);
            $("#B_DISCTYPE_DESC_" + j).val(DISCTYPE_DESC);
            $("#B_DISCRATE_" + j).val(DISCRATE);
            $("#B_TDDISCRATE_" + j).val(TDDISCRATE);
            $("#B_TDDISCTYPE_" + j).val(TDDISCTYPE);
            $("#B_SCMDISCRATE_" + j).val(SCMDISCRATE);
            $("#B_SCMDISCTYPE_" + j).val(SCMDISCTYPE);
            //if (MENU_PARA == "PB" || MENU_PARA == "OP") {
            $("#B_BALENO_" + j).val(BALENO);
            //}
            $("#B_ITREM_" + j).val(ITREM);
            $("#B_GSTPER_" + j).val(GSTPER);

            if (MENU_PARA == "SR" || MENU_PARA == "PR") {
                $("#B_AGDOCNO_" + j).val($("#D_AGDOCNO_" + i).val());
                $("#B_AGDOCDT_" + j).val($("#D_AGDOCDT_" + i).val());
            }

            if (ModuleCode.indexOf("SALESCLOTH") != -1) {
                $("#B_PAGENO_" + j).val($("#D_PAGENO_" + i).val());
                $("#B_PAGESLNO_" + j).val($("#D_PAGESLNO_" + i).val());
            }
            $("#B_BLUOMCD_" + j).val($("#D_BLUOMCD_" + i).val());

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

        if (MENU_PARA != "SR" && MENU_PARA != "SBCMR" && MENU_PARA != "PB" && MENU_PARA != "OP" && MENU_PARA != "OTH" && retFloat($("#B_QNTY_" + i).val()) != 0) {
            var NEGSTOCK = $("#B_NEGSTOCK_" + i).val();
            var BALSTOCK = $("#B_BALSTOCK_" + i).val();
            if (BALSTOCK == "") { BALSTOCK = parseFloat(0); } else { BALSTOCK = parseFloat(BALSTOCK); }
            if (QNTY == "") { QNTY = parseFloat(0); } else { QNTY = parseFloat(QNTY); }
            var balancestock = BALSTOCK - QNTY;
            if (balancestock < 0) {
                if (NEGSTOCK != "Y") {
                    ClearAllTextBoxes("B_CUTLENGTH_" + i + ",B_NOS_" + i + ",B_QNTY_" + i);
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

    var DiscountedAmt = 0;
    //SCMDISCOUNT AMOUNT CALCULATION
    var SCMDISCAMT = 0;
    SCMDISCAMT = parseFloat(CalculateDiscount("D_SCMDISCTYPE_" + i, "D_SCMDISCRATE_" + i, "D_QNTY_" + i, "D_NOS_" + i, "D_AMT_" + i, DiscountedAmt)).toFixed(2);
    $("#D_SCMDISCAMT_" + i).val(SCMDISCAMT);

    //TDDISCOUNT AMOUNT CALCULATION
    var TDDISCAMT = 0;
    DiscountedAmt = amount - SCMDISCAMT;
    TDDISCAMT = parseFloat(CalculateDiscount("D_TDDISCTYPE_" + i, "D_TDDISCRATE_" + i, "D_QNTY_" + i, "D_NOS_" + i, "D_AMT_" + i, DiscountedAmt)).toFixed(2);
    $("#D_TDDISCAMT_" + i).val(TDDISCAMT);

    //DISCOUNT AMOUNT CALCULATION
    var DISCAMT = 0;
    DiscountedAmt = amount - SCMDISCAMT - TDDISCAMT;
    DISCAMT = parseFloat(CalculateDiscount("D_DISCTYPE_" + i, "D_DISCRATE_" + i, "D_QNTY_" + i, "D_NOS_" + i, "D_AMT_" + i, DiscountedAmt)).toFixed(2);
    $("#D_DISCAMT_" + i).val(DISCAMT);

    //TOTAL DISCOUNT AMOUNT CALCULATION
    var TOTDISCAMT = parseFloat(retFloat(SCMDISCAMT) + retFloat(TDDISCAMT) + retFloat(DISCAMT)).toFixed(2);
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
    var T_DISCAMT = 0; var T_TDDISCAMT = 0; var T_SCMDISCAMT = 0;
    var GridRow = $("#_T_SALE_DETAIL_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var NOS = $("#D_NOS_" + i).val();
        if (NOS != "") { T_NOS = T_NOS + parseFloat(NOS); } else { T_NOS = T_NOS + parseFloat(0); }

        var QNTY = $("#D_QNTY_" + i).val();
        if (QNTY != "") { T_QNTY = T_QNTY + parseFloat(QNTY); } else { T_QNTY = T_QNTY + parseFloat(0); }

        var AMT = $("#D_AMT_" + i).val();
        if (AMT != "") { T_AMT = T_AMT + parseFloat(AMT); } else { T_AMT = T_AMT + parseFloat(0); }

        var DISCAMT = $("#D_DISCAMT_" + i).val();
        if (DISCAMT != "") { T_DISCAMT = T_DISCAMT + parseFloat(DISCAMT); } else { T_DISCAMT = T_DISCAMT + parseFloat(0); }

        var TDDISCAMT = $("#D_TDDISCAMT_" + i).val();
        if (TDDISCAMT != "") { T_TDDISCAMT = T_TDDISCAMT + parseFloat(TDDISCAMT); } else { T_TDDISCAMT = T_TDDISCAMT + parseFloat(0); }

        var SCMDISCAMT = $("#D_SCMDISCAMT_" + i).val();
        if (SCMDISCAMT != "") { T_SCMDISCAMT = T_SCMDISCAMT + parseFloat(SCMDISCAMT); } else { T_SCMDISCAMT = T_SCMDISCAMT + parseFloat(0); }

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

    $("#T_DISCAMT").val(parseFloat(T_DISCAMT).toFixed(2));
    $("#T_TDDISCAMT").val(parseFloat(T_TDDISCAMT).toFixed(2));
    $("#T_SCMDISCAMT").val(parseFloat(T_SCMDISCAMT).toFixed(2));

    //main tab
    //$("#TOTNOS").val(parseFloat(T_NOS).toFixed(2));
    //$("#TOTQNTY").val(parseFloat(T_QNTY).toFixed(2));
    //$("#TOTTAXVAL").val(parseFloat(T_GROSS_AMT).toFixed(2));
    //$("#TOTTAX").val(parseFloat(totaltax).toFixed(2));
    BillAmountCalculate();

}

function Sale_GetGstPer(i, FieldidStarting) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;

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
    if (D_GROSS_AMT == "") {
        D_GROSS_AMT = parseFloat(0);
    }
    if (E_NET_AMT == "") {
        E_NET_AMT = parseFloat(0);
    }
    if (RT == "") {
        RT = parseFloat(0);
    }
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
    var GridRow = $("#AMOUNT_GRID > tbody > tr").length;
    if (GridRow == 0) return true;
    var NEW_AMT = document.getElementById(id.id).value;
    var PERCENTAGE = document.getElementById(PER.id).value;
    var AMT = document.getElementById(AMOUNT.id).value;
    var AMT_1 = document.getElementById(AMT1.id).value;
    var AMT_2 = document.getElementById(AMT2.id).value;
    var AMT_3 = document.getElementById(AMT3.id).value;
    var AMT_4 = "";
    if (AMT4 != "") {
        AMT_4 = retFloat(document.getElementById(AMT4.id).value);
    }
    AMT_4 = retFloat(AMT_4);
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
function UpdateTaxPer() {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;

    if (GridRowMain != 0) {
        var IGST_PER = 0; var CGST_PER = 0; var SGST_PER = 0; var CESS_PER = 0; var DUTY_PER = 0;
        for (i = 0; i <= GridRowMain - 1; i++) {
            var rate = retFloat($("#B_RATE_" + i).val());
            var prodgrpgstper = $("#B_PRODGRPGSTPER_" + i).val();
            var allgst = retGstPerstr(prodgrpgstper, rate);
            var tax = null;
            if (allgst != "") {
                tax = allgst.split(',');
            }
            var IGST = 0, CGST = 0, SGST = 0, CESS = 0, DUTY = 0;
            if (tax.length > 0) {
                IGST = parseFloat(tax[0]).toFixed(2);
                CGST = parseFloat(tax[1]).toFixed(2);
                SGST = parseFloat(tax[2]).toFixed(2);
            }

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
function BillAmountCalculate(TAG) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    //if (event.key != "F8" && TAG == "TCSON" && event.key != undefined) {
    //    return true;
    //}
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
        TCSPER = retFloat(document.getElementById("TCSPER").value).toFixed(3);
        if (TCSPER == "" || TCSPER == "NaN") { TCSPER = parseFloat(0); }
        document.getElementById("TCSPER").value = parseFloat(TCSPER).toFixed(3);
        //if (MENU_PARA == "OP") {
        //if ((MENU_PARA == "PB" || MENU_PARA == "OP") && event.key != "F8") {
        var TCSAUTOCAL = true;
        if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
            TCSAUTOCAL = document.getElementById("TCSAUTOCAL").checked;
        }
        if ((MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") && TCSAUTOCAL == false) {
            TCSON = $("#TCSON").val();
            if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON); }
        }
        else {
            GetTCSON(totalbillamt);
            TCSON = $("#TCSON").val();
            if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON) }
        }
        TCSAMT = parseFloat(parseFloat(TCSON) * parseFloat(TCSPER) / 100).toFixed(2);
        TCSAMT = parseFloat(CalculateTcsAmt(TCSAMT)).toFixed(2);
        if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
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
        $("#DISPTCSAMT").val(parseFloat($("#TCSAMT").val()).toFixed(2));
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
        document.getElementById("DISPBLAMT").value = parseFloat(R_TOTAL_BILL_AMOUNT).toFixed(2);
        document.getElementById("ROAMT").value = parseFloat(TOTAL_ROUND).toFixed(2);
    }
    else {
        TOTAL_ROUND = 0;
        document.getElementById("BLAMT").value = parseFloat(totalbillamt).toFixed(2);
        document.getElementById("DISPBLAMT").value = parseFloat(totalbillamt).toFixed(2);
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
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlDeleteRow").val(),// "@Url.Action("DeleteRow", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            $("#partialdivBarCodeTab").animate({ marginTop: '0px' }, 50);
            $("#partialdivBarCodeTab").html(result);
            Checked_Disable();
            CalculateTotal_Barno();
            HasChangeBarSale();
            $("#WaitingMode").hide();
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
    var ModuleCode = $("#ModuleCode").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    var MNTNPART = $("#MNTNPART").val();
    var MNTNCOLOR = $("#MNTNCOLOR").val();
    var MNTNSIZE = $("#MNTNSIZE").val();
    var MNTNLISTPRICE = $("#MNTNLISTPRICE").val();
    var MNTNPCSTYPE = $("#MNTNPCSTYPE").val();
    var MNTNSHADE = $("#MNTNSHADE").val();
    var MNTNDISC1 = $("#MNTNDISC1").val();
    var MNTNDISC2 = $("#MNTNDISC2").val();
    var MNTNWPRPPER = $("#MNTNWPRPPER").val();
    var MNTNBALE = $("#MNTNBALE").val();
    var MNTNOURDESIGN = $("#MNTNOURDESIGN").val();
    var SHOWMTRLJOBCD = $("#SHOWMTRLJOBCD").val();
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
    if (MENU_PARA != "SR" && MENU_PARA != "SBCMR" && MENU_PARA != "PB" && MENU_PARA != "OP" && MENU_PARA != "OTH" && retFloat($("#QNTY").val()) != 0) {
        var NEGSTOCK = $("#NEGSTOCK").val();
        var BALSTOCK = $("#BALSTOCK").val();
        if (BALSTOCK == "") { BALSTOCK = parseFloat(0); } else { BALSTOCK = parseFloat(BALSTOCK); }
        var QNTY = $("#QNTY").val();
        if (QNTY == "") { QNTY = parseFloat(0); } else { QNTY = parseFloat(QNTY); }
        var balancestock = BALSTOCK - QNTY;
        if (balancestock < 0) {
            if (NEGSTOCK != "Y") {
                ClearAllTextBoxes("CUTLENGTH,NOS,QNTY");
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


    var BARCODE = $("#BARCODE").val();
    var ITGRPCD = $("#ITGRPCD").val();
    var ITGRPNM = $("#ITGRPNM").val();
    var MTRLJOBCD = $("#MTRLJOBCD").val();
    var MTRLJOBNM = $("#MTRLJOBNM").val();
    var MTBARCODE = $("#MTBARCODE").val();
    var ITCD = $("#ITCD").val();
    var ITSTYLE = $("#ITSTYLE").val().trim();
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
    var CUTLENGTH = $("#CUTLENGTH").val();
    var DISCRATE = $("#DISCRATE").val();
    var DISCTYPE = $("#DISCTYPE").val();
    var DISCTYPE_DESC = DISCTYPE == "P" ? "%" : DISCTYPE == "N" ? "Nos" : DISCTYPE == "Q" ? "Qnty" : DISCTYPE == "A" ? "AftDsc%" : "Fixed";
    var HSNCODE = $("#HSNCODE").val();
    var GSTPER = $("#GSTPER").val();
    var PRODGRPGSTPER = $("#PRODGRPGSTPER").val();
    var SHADE = $("#SHADE").val();
    var BALENO = $("#BALENO").val();
    var OURDESIGN = "";
    var PDESIGN = "";

    var WPPRICEGEN = "";
    var RPPRICEGEN = "";
    if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
        OURDESIGN = $("#OURDESIGN").val();
        PDESIGN = $("#PDESIGN").val();
        WPPRICEGEN = $("#WPPRICEGEN").val();
        RPPRICEGEN = $("#RPPRICEGEN").val();
    }
    var TDDISCTYPE = $("#TDDISCTYPE").val();
    var TDDISCTYPE_DESC = TDDISCTYPE == "P" ? "%" : TDDISCTYPE == "N" ? "Nos" : TDDISCTYPE == "Q" ? "Qnty" : TDDISCTYPE == "A" ? "AftDsc%" : "Fixed";
    var TDDISCRATE = $("#TDDISCRATE").val();
    var SCMDISCTYPE = $("#SCMDISCTYPE").val();
    var SCMDISCTYPE_DESC = SCMDISCTYPE == "P" ? "%" : SCMDISCTYPE == "N" ? "Nos" : SCMDISCTYPE == "Q" ? "Qnty" : TDDISCTYPE == "A" ? "AftDsc%" : "Fixed";
    var SCMDISCRATE = $("#SCMDISCRATE").val();
    var LOCABIN = $("#LOCABIN").val();
    var GLCD = $("#GLCD").val();
    var ITMBARGENTYPE = $("#BARGENTYPETEMP").val();
    var ENTRYBARGENTYPE = $("#BARGENTYPE").val();
    if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
        var ORDDOCNO = $("#ORDDOCNO").val();
        var ORDDOCDT = $("#ORDDOCDT").val();
        var ORDAUTONO = $("#ORDAUTONO").val();
        var ORDSLNO = $("#ORDSLNO").val();
    }
    var BarImages = $("#BarImages").val();
    if ((MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") && (ENTRYBARGENTYPE == "E" || ITMBARGENTYPE == "E")) {
        BarImages = "";
    }
    var NoOfBarImages = BarImages.split(String.fromCharCode(179)).length;
    if (BarImages == '') {
        NoOfBarImages = '';
    }
    var rowindex = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    var SLNO = 1;
    if (retInt(rowindex) != 0) {
        var LASTROWINDEX = retInt(rowindex) - 1;
        var LASTSLNO = $("#B_SLNO_" + LASTROWINDEX).val();
        SLNO = retInt(LASTSLNO) + 1;
    }
    var BALSTOCK = $("#BALSTOCK").val();
    var NEGSTOCK = $("#NEGSTOCK").val();

    var AGDOCNO = $("#AGDOCNO").val();
    var AGDOCDT = $("#AGDOCDT").val();
    var LISTPRICE = $("#LISTPRICE").val();
    var LISTDISCPER = $("#LISTDISCPER").val();

    //get bill slno
    var TXNSLNO = "";
    var PAGENO = "";
    var PAGESLNO = "";
    var BLUOMCD = "";
    if ($("#TXNSLNO").val() == "" || $("#TXNSLNO").val() == "0") {
        var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
        if (GridRowMain == 0) {
            TXNSLNO = 1;
        }
        else {
            var allslno = [];
            var matchslno = [];
            var pageno = [];
            var bluom = [];
            countmatchslno = 0;
            for (j = 0; j <= GridRowMain - 1; j++) {
                var flag = true;
                if (MENU_PARA == "SR" || MENU_PARA == "PR") {
                    if (retStr(AGDOCNO) != retStr($("#B_AGDOCNO_" + j).val()) || retStr(AGDOCDT) != retStr($("#B_AGDOCDT_" + j).val())) {
                        flag = false;
                    }
                }
                if ((MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "PI") && MNTNLISTPRICE == "Y") {
                    if (retFloat(LISTPRICE) != retFloat($("#B_LISTPRICE_" + j).val()) || retFloat(LISTDISCPER) != retFloat($("#B_LISTDISCPER_" + j).val())) {
                        flag = false;
                    }
                }
                //if (MENU_PARA == "PB" || MENU_PARA == "OP") {
                if (retStr(BALENO) != retStr($("#B_BALENO_" + j).val())) {
                    flag = false;
                }
                //}
                if (flag == true) {
                    if (retStr(ITGRPCD) == retStr($("#B_ITGRPCD_" + j).val()) && retStr(MTRLJOBCD) == retStr($("#B_MTRLJOBCD_" + j).val()) &&
                 retStr(MTBARCODE) == retStr($("#B_MTBARCODE_" + j).val()) && retStr(ITCD) == retStr($("#B_ITCD_" + j).val()) &&
                 retStr(DISCTYPE) == retStr($("#B_DISCTYPE_" + j).val()) && retStr(TDDISCTYPE) == retStr($("#B_TDDISCTYPE_" + j).val()) &&
                  retStr(SCMDISCTYPE) == retStr($("#B_SCMDISCTYPE_" + j).val()) && retStr(UOM) == retStr($("#B_UOM_" + j).val()) && retStr(STKTYPE) == retStr($("#B_STKTYPE_" + j).val()) && retFloat(RATE) == retFloat($("#B_RATE_" + j).val()) &&
                 retFloat(DISCRATE) == retFloat($("#B_DISCRATE_" + j).val()) && retFloat(SCMDISCRATE) == retFloat($("#SCMDISCRATE").val()) && retFloat(TDDISCRATE) == retFloat($("#TDDISCRATE").val()) && retFloat(GSTPER) == retFloat($("#GSTPER").val()) &&
                 retFloat(FLAGMTR) == retFloat($("#B_FLAGMTR_" + j).val()) && retStr(HSNCODE) == retStr($("#B_HSNCODE_" + j).val()) && retStr(PRODGRPGSTPER) == retStr($("#B_PRODGRPGSTPER_" + j).val()) &&
                 retStr(GLCD) == retStr($("#B_GLCD_" + j).val())) {

                        matchslno[countmatchslno] = retInt($("#B_TXNSLNO_" + j).val());
                        if (ModuleCode.indexOf("SALESCLOTH") != -1) {
                            var str1 = "^TXNSLNO=^" + retInt($("#B_TXNSLNO_" + j).val()) + String.fromCharCode(181);
                            str1 += "^PAGENO=^" + retInt($("#B_PAGENO_" + j).val()) + String.fromCharCode(181);
                            str1 += "^PAGESLNO=^" + retInt($("#B_PAGESLNO_" + j).val()) + String.fromCharCode(181);
                            pageno[countmatchslno] = str1;
                        }
                        var str1 = "^TXNSLNO=^" + retInt($("#B_TXNSLNO_" + j).val()) + String.fromCharCode(181);
                        str1 += "^BLUOMCD=^" + retStr($("#B_BLUOMCD_" + j).val()) + String.fromCharCode(181);
                        bluom[countmatchslno] = str1;

                        countmatchslno++;
                    }
                }
                allslno[j] = retInt($("#B_TXNSLNO_" + j).val());
            }

            if (matchslno.length > 0) {
                TXNSLNO = Math.max.apply(Math, matchslno);
                if (TXNSLNO == 0) {
                    TXNSLNO = Math.max.apply(Math, allslno);
                    TXNSLNO++;
                }
                else {
                    if (ModuleCode.indexOf("SALESCLOTH") != -1) {
                        var strpageno = pageno.find(element => element.indexOf("^TXNSLNO=^" + TXNSLNO + String.fromCharCode(181)) != -1);
                        PAGENO = returncolvalue(strpageno, "PAGENO");
                        PAGESLNO = returncolvalue(strpageno, "PAGESLNO");
                    }

                    var strbluom = bluom.find(element => element.indexOf("^TXNSLNO=^" + TXNSLNO + String.fromCharCode(181)) != -1);
                    BLUOMCD = returncolvalue(strbluom, "BLUOMCD");
                }
            }
            else {
                TXNSLNO = Math.max.apply(Math, allslno);
                TXNSLNO++;
            }
        }
    }
    else {
        TXNSLNO = retInt($("#TXNSLNO").val());
    }
    var tr = "";
    tr += ' <tr style="font-size:12px; font-weight:bold;">';
    tr += '    <td class="sticky-cell">';
    tr += '        <input tabindex="-1" data-val="true" data-val-required="The Checked field is required." id="B_Checked_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].Checked" type="checkbox" value="true"><input name="TBATCHDTL[' + rowindex + '].Checked" type="hidden" value="false">';
    tr += '        <input data-val="true" data-val-length="The field BARNO must be a string with a maximum length of 25." data-val-length-max="25" data-val-required="The BARNO field is required." id="B_BARNO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BARNO" type="hidden" value="' + BARCODE + '">';
    tr += '        <input data-val="true" data-val-number="The field FLAGMTR must be a number." id="B_FLAGMTR_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].FLAGMTR" type="hidden" value="' + FLAGMTR + '">';
    tr += '        <input data-val="true" data-val-length="The field HSNCODE must be a string with a maximum length of 8." data-val-length-max="8" id="B_HSNCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].HSNCODE" type="hidden" value="' + HSNCODE + '">';
    if (MNTNBALE != "Y") {
        tr += '        <input data-val="true" data-val-length="The field BALENO must be a string with a maximum length of 30." data-val-length-max="30" id="B_BALENO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BALENO" type="hidden" value="' + BALENO + '">';
    }
    if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
        tr += '        <input data-val="true" data-val-length="The field OURDESIGN must be a string with a maximum length of 30." data-val-length-max="30" id="B_OURDESIGN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].OURDESIGN" type="hidden" value="' + OURDESIGN + '">';
        tr += '        <input data-val="true" data-val-length="The field PDESIGN must be a string with a maximum length of 30." data-val-length-max="30" id="B_PDESIGN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PDESIGN" type="hidden" value="' + PDESIGN + '">';
    }
    tr += '        <input data-val="true" data-val-length="The field LOCABIN must be a string with a maximum length of 10." data-val-length-max="10" id="B_LOCABIN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].LOCABIN" type="hidden" value="' + LOCABIN + '">';
    tr += '        <input data-val="true" data-val-length="The field GLCD must be a string with a maximum length of 10." data-val-length-max="10" id="B_GLCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].GLCD" type="hidden" value="' + GLCD + '">';
    tr += '        <input id="B_BARGENTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BARGENTYPE" type="hidden" value="' + ITMBARGENTYPE + '">';
    if (ModuleCode.indexOf("SALESCLOTH") != -1) {
        tr += '        <input id="B_PAGENO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PAGENO" type="hidden" value="' + PAGENO + '">';
        tr += '        <input id="B_PAGESLNO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PAGESLNO" type="hidden" value="' + PAGESLNO + '">';
    }
    if (MNTNSHADE != "Y") {
        tr += '        <input id="B_SHADE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SHADE" type="hidden" value="' + SHADE + '">';
    }

    if ((MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") && MNTNWPRPPER != "Y") {
        tr += '        <input id="B_WPRATE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].WPRATE" type="hidden" >';
        tr += '        <input id="B_WPPRICEGEN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].WPPRICEGEN" type="hidden" value="' + WPPRICEGEN + '">';
        tr += '        <input id="B_RPPRICEGEN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].RPPRICEGEN" type="hidden" value="' + RPPRICEGEN + '">';
        tr += '        <input id="B_RPRATE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].RPRATE" type="hidden"  >';
    }
    if (MENU_PARA == "OP" || MENU_PARA == "OTH")
     {
        tr += '        <input id="B_GSTPER_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].GSTPER" type="hidden" value="' + GSTPER + '">';
        tr += '        <input id="B_PRODGRPGSTPER_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PRODGRPGSTPER" type="hidden" value="' + PRODGRPGSTPER + '">';
    }
    if (MNTNDISC1 != "Y") {
        tr += '        <input id="B_TDDISCTYPE_DESC_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].TDDISCTYPE_DESC" type="hidden" value="' + TDDISCTYPE_DESC + '">';
        tr += '        <input id="B_TDDISCTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].TDDISCTYPE" type="hidden" value="' + TDDISCTYPE + '">';
        tr += '        <input id="B_TDDISCRATE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].TDDISCRATE" type="hidden" value="' + TDDISCRATE + '">';

    }
    if (MNTNDISC2 != "Y") {
        tr += '        <input id="B_DISCTYPE_DESC_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].DISCTYPE_DESC" type="hidden" value="' + DISCTYPE_DESC + '">';
        tr += '        <input id="B_DISCTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].DISCTYPE" type="hidden" value="' + DISCTYPE + '">';
        tr += '        <input id="B_DISCRATE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].DISCRATE" type="hidden" value="' + DISCRATE + '">';

    }
    if (SHOWMTRLJOBCD != "Y") {
        tr += '        <input id="B_MTRLJOBCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].MTRLJOBCD" type="hidden" value="' + MTRLJOBCD + '">';
        tr += '        <input id="B_MTRLJOBNM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].MTRLJOBNM" type="hidden" value="' + MTRLJOBNM + '">';
        tr += '        <input id="B_MTBARCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].MTBARCODE" type="hidden" value="' + MTBARCODE + '">';
    }
    tr += '        <input id="B_BLUOMCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BLUOMCD" type="hidden" value="' + BLUOMCD + '">';
    tr += '    </td>';
    tr += '    <td class="sticky-cell" style="left:20px;" title="' + SLNO + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-number="The field SLNO must be a number." data-val-required="The SLNO field is required." id="B_SLNO_' + rowindex + '" maxlength="2" name="TBATCHDTL[' + rowindex + '].SLNO" readonly="readonly" style="text-align:center;" type="text" value="' + SLNO + '">';
    tr += '    </td>';
    tr += '    <td class="sticky-cell" style="left:60px" title="' + TXNSLNO + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-number="The field TXNSLNO must be a number." data-val-required="The TXNSLNO field is required." id="B_TXNSLNO_' + rowindex + '" maxlength="4" name="TBATCHDTL[' + rowindex + '].TXNSLNO"  style="text-align:center;" onkeypress="return numericOnly(this,2);" onchange="HasChangeBarSale();" type="text" value="' + TXNSLNO + '">';
    tr += '    </td>';
    if (MENU_PARA == "SR" || MENU_PARA == "PR") {
        tr += ' <td class="" title="' + AGDOCNO + '">';
        tr += '  <input class=" atextBoxFor " id="B_AGDOCNO_' + rowindex + '" maxlength="16" name="TBATCHDTL[' + rowindex + '].AGDOCNO" type="text" value="' + AGDOCNO + '">';
        tr += '  </td>';

        tr += ' <td class="" title="' + AGDOCDT + '">';
        tr += '     <input class=" atextBoxFor agdocdt text-box single-line " autocomplete="off" onblur="DocumentDateCHK(this)" data-val="true" data-val-length="The field AGDOCDT must be a string with a maximum length of 10." data-val-length-max="10" data-val-required="The AGDOCDT field is required." id="B_AGDOCDT_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].AGDOCDT"  type="text" value="' + AGDOCDT + '">';
        tr += ' </td>';
    }
    tr += '    <td class="" title="' + ITGRPNM + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ITGRPNM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ITGRPNM" readonly="readonly" type="text" value="' + ITGRPNM + '">';
    tr += '        <input id="B_ITGRPCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ITGRPCD" type="hidden" value="' + ITGRPCD + '">';
    tr += '    </td>';
    if (SHOWMTRLJOBCD == "Y") {
        tr += '    <td class="" title="' + MTRLJOBCD + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field MTRLJOBCD must be a string with a maximum length of 2." data-val-length-max="2" id="B_MTRLJOBCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].MTRLJOBCD" readonly="readonly" type="text" value="' + MTRLJOBCD + '">';
        tr += '        <input id="B_MTRLJOBNM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].MTRLJOBNM" type="hidden" value="' + MTRLJOBNM + '">';
        tr += '        <input id="B_MTBARCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].MTBARCODE" type="hidden" value="' + MTBARCODE + '">';
        tr += '    </td>';
    }
    tr += '    <td class="" title="' + ITCD + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ITCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ITCD" readonly="readonly" type="text" value="' + ITCD + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + ITSTYLE + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ITSTYLE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ITSTYLE" readonly="readonly" type="text" value="' + ITSTYLE + '">';
    tr += '        <input id="B_STYLENO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].STYLENO" type="hidden" value="' + STYLENO + '">';
    tr += '    </td>         ';
    if (MNTNBALE == "Y") {
        tr += '    <td class="" title="' + BALENO + '">';
        tr += '        <input data-val="true" data-val-length="The field BALENO must be a string with a maximum length of 30." data-val-length-max="30" id="B_BALENO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BALENO" onblur="HasChangeBarSale();" value="' + BALENO + '">';
        tr += '     </td>';
    }
    tr += '    <td class="" title="' + STKTYPE + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field STKTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_STKTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].STKTYPE" readonly="readonly" type="text" value="' + STKTYPE + '">';
    tr += '     </td>';

    if (MNTNPART == "Y") {
        tr += '    <td class="" title="' + PARTCD + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field PARTCD must be a string with a maximum length of 4." data-val-length-max="4" id="B_PARTCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PARTCD" readonly="readonly" type="text" value="' + PARTCD + '">';
        tr += '        <input id="B_PARTNM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PARTNM" type="hidden" value="' + PARTNM + '">';
        tr += '        <input id="B_PRTBARCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PRTBARCODE" type="hidden" value="' + PRTBARCODE + '">';
        tr += '    </td>';
    }
    if (MNTNCOLOR == "Y") {
        tr += '    <td class="" title="' + COLRCD + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field COLRCD must be a string with a maximum length of 4." data-val-length-max="4" id="B_COLRCD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].COLRCD" readonly="readonly" type="text" value="' + COLRCD + '">';
        tr += '     <input id="B_CLRBARCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].CLRBARCODE" type="hidden" value="' + CLRBARCODE + '">';
        tr += '    </td>';
        tr += '    <td class="" title="' + COLRNM + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_COLRNM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].COLRNM" readonly="readonly" type="text" value="' + COLRNM + '">';
        tr += '    </td>';
    }
    if (MNTNSIZE == "Y") {
        tr += '    <td class="" title="' + SIZECD + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field SIZECD must be a string with a maximum length of 4." data-val-length-max="4" id="B_SIZECD_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SIZECD" readonly="readonly" type="text" value="' + SIZECD + '">';
        tr += '        <input id="B_SIZENM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SIZENM" type="hidden" value="' + SIZENM + '">';
        tr += '        <input id="B_SZBARCODE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SZBARCODE" type="hidden" value="' + SZBARCODE + '">';
        tr += '    </td>';
    }
    if (MNTNSHADE == "Y") {
        tr += '    <td class="" title="' + SHADE + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field SHADE must be a string with a maximum length of 15." data-val-length-max="15" id="B_SHADE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SHADE" readonly="readonly" type="text" value="' + SHADE + '">';
        tr += '    </td>';
    }

    tr += '    <td class="" title="' + BALSTOCK + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_BALSTOCK_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BALSTOCK" readonly="readonly" style="text-align: right;" type="text" value="' + BALSTOCK + '">';
    tr += '        <input id="B_NEGSTOCK_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].NEGSTOCK" type="hidden" value="' + NEGSTOCK + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + CUTLENGTH + '">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field CUTLENGTH must be a number." id="B_CUTLENGTH_' + rowindex + '" maxlength="6" name="TBATCHDTL[' + rowindex + '].CUTLENGTH" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="CalculateBargridQnty(\'_T_SALE_PRODUCT_GRID\', ' + rowindex + ');" value="' + CUTLENGTH + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + NOS + '">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NOS must be a number." id="B_NOS_' + rowindex + '" maxlength="12" name="TBATCHDTL[' + rowindex + '].NOS" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" onchange="CalculateBargridQnty(\'_T_SALE_PRODUCT_GRID\', ' + rowindex + ');" value="' + NOS + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + QNTY + '">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" autocomplete="off" data-val-number="The field QNTY must be a number." id="B_QNTY_' + rowindex + '" maxlength="12" name="TBATCHDTL[' + rowindex + '].QNTY" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" onblur="CalculateBargridQnty(\'_T_SALE_PRODUCT_GRID\', ' + rowindex + ');" value="' + QNTY + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + UOM + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor" id="B_UOM_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].UOM" readonly="readonly" type="text" value="' + UOM + '">';
    tr += '    </td>';


    if ((MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "PI") && MNTNLISTPRICE == "Y") {
        tr += '    <td class="" title="' + LISTPRICE + '">';
        tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field LISTPRICE must be a number." id="B_LISTPRICE_' + rowindex + '" maxlength="14" name="TBATCHDTL[' + rowindex + '].LISTPRICE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="RateUpdate(' + rowindex + ',\'#B_\');" value="' + LISTPRICE + '" >';
        tr += '    </td>';
        tr += '    <td class="" title="' + LISTDISCPER + '">';
        tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field LISTDISCPER must be a number." id="B_LISTDISCPER_' + rowindex + '" maxlength="6" name="TBATCHDTL[' + rowindex + '].LISTDISCPER" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="RateUpdate(' + rowindex + ',\'#B_\');" value="' + LISTDISCPER + '" >';
        tr += '    </td>';
        tr += '    <td class="" title="' + RATE + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RATE must be a number." id="B_RATE_' + rowindex + '" maxlength="14" name="TBATCHDTL[' + rowindex + '].RATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" readonly="readonly" value="' + RATE + '" >';
        tr += '    </td>';
    }
    else {
        tr += '    <td class="" title="' + RATE + '">';
        tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RATE must be a number." id="B_RATE_' + rowindex + '" maxlength="14" name="TBATCHDTL[' + rowindex + '].RATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="RateUpdate(' + rowindex + ',\'#B_\');" value="' + RATE + '" >';
        tr += '    </td>';
    }

    if ((MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") && MNTNWPRPPER == "Y") {
        tr += '    <td class="">';
        tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field WPRATE must be a number." id="B_WPRATE_' + rowindex + '" maxlength="14" name="TBATCHDTL[' + rowindex + '].WPRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text"  readonly="readonly"  >';
        tr += '        <input data-val="true" data-val-length="The field WPPRICEGEN must be a string with a maximum length of 30." data-val-length-max="30" id="B_WPPRICEGEN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].WPPRICEGEN" type="hidden" value="' + WPPRICEGEN + '">';
        tr += '        <input data-val="true" data-val-length="The field RPPRICEGEN must be a string with a maximum length of 30." data-val-length-max="30" id="B_RPPRICEGEN_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].RPPRICEGEN" type="hidden" value="' + RPPRICEGEN + '">';
        tr += '    </td>';

        tr += '    <td class="">';
        tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RPRATE must be a number." id="B_RPRATE_' + rowindex + '" maxlength="14" name="TBATCHDTL[' + rowindex + '].RPRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text"  readonly="readonly" >';
        tr += '    </td>';
    }

    if (MENU_PARA != "OP" && MENU_PARA != "OTH") {
        tr += '    <td class="" title="' + GSTPER + '">';
        tr += '        <input class="atextBoxFor text-box single-line" data-val="true" data-val-number="The field GSTPER must be a number." id="B_GSTPER_' + rowindex + '" maxlength="5" name="TBATCHDTL[' + rowindex + '].GSTPER" onkeypress="return numericOnly(this,2);" style="text-align: right;" readonly="readonly" type="text" value="' + GSTPER + '">';
        tr += '        <input id="B_PRODGRPGSTPER_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].PRODGRPGSTPER" type="hidden" value="' + PRODGRPGSTPER + '">';
        tr += '    </td>';
    }
   
    tr += '    <td class="" title="' + SCMDISCTYPE_DESC + '">';
    tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_SCMDISCTYPE_DESC_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SCMDISCTYPE_DESC" readonly="readonly" type="text" value="' + SCMDISCTYPE_DESC + '">';
    tr += '        <input data-val="true" data-val-length="The field SCMDISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_SCMDISCTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].SCMDISCTYPE" type="hidden" value="' + SCMDISCTYPE + '">';
    tr += '    </td>';
    tr += '    <td class="" title="' + SCMDISCRATE + '">';
    tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field SCMDISCRATE must be a number." id="B_SCMDISCRATE_' + rowindex + '" maxlength="10" name="TBATCHDTL[' + rowindex + '].SCMDISCRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="HasChangeBarSale();" value="' + SCMDISCRATE + '">';
    tr += '    </td>';
    if (MNTNDISC1 == "Y") {
        tr += '     <td class="" title="' + TDDISCTYPE_DESC + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_TDDISCTYPE_DESC_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].TDDISCTYPE_DESC" readonly="readonly" type="text" value="' + TDDISCTYPE_DESC + '">';
        tr += '        <input data-val="true" data-val-length="The field TDDISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_TDDISCTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].TDDISCTYPE" type="hidden" value="' + TDDISCTYPE + '">';
        tr += '                              </td>';
        tr += '    <td class="" title="' + TDDISCRATE + '">';
        tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field TDDISCRATE must be a number." id="B_TDDISCRATE_' + rowindex + '" maxlength="10" name="TBATCHDTL[' + rowindex + '].TDDISCRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="HasChangeBarSale();" value="' + TDDISCRATE + '">';
        tr += '    </td>';
    }

    if (MNTNDISC2 == "Y") {
        tr += '    <td class="" title="' + DISCTYPE_DESC + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor" id="B_DISCTYPE_DESC_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].DISCTYPE_DESC" readonly="readonly" type="text" value="' + DISCTYPE_DESC + '">';
        tr += '        <input data-val="true" data-val-length="The field DISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_DISCTYPE_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].DISCTYPE" type="hidden" value="' + DISCTYPE + '">';
        tr += '    </td>';
        tr += '    <td class="" title="' + DISCRATE + '">';
        tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field DISCRATE must be a number." id="B_DISCRATE_' + rowindex + '" maxlength="10" name="TBATCHDTL[' + rowindex + '].DISCRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" onchange="HasChangeBarSale();" value="' + DISCRATE + '">';
        tr += '    </td>';
    }

    if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
        tr += '    <td class="" title="' + ORDDOCNO + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ORDDOCNO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ORDDOCNO" readonly="readonly" type="text" value="' + ORDDOCNO + '">';
        tr += '        <input id="B_ORDAUTONO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ORDAUTONO" type="hidden" value="' + ORDAUTONO + '">';
        tr += '    </td>';
        tr += '    <td class="" title="' + ORDSLNO + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ORDSLNO_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ORDSLNO" readonly="readonly" type="text" value="' + ORDSLNO + '">';
        tr += '    </td>';
        tr += '    <td class="" title="' + ORDDOCDT + '">';
        tr += '        <input tabindex="-1" class=" atextBoxFor " id="B_ORDDOCDT_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].ORDDOCDT" readonly="readonly" type="text" value="' + ORDDOCDT + '">';
        tr += '    </td>';
    }
    if ((MENU_PARA == "PB" || MENU_PARA == "PR" || MENU_PARA == "OP" || MENU_PARA == "OTH") && MNTNPCSTYPE == "Y") {
        tr += '    <td class="">';
        tr += ' <input class=" atextBoxFor " data-val="true" data-val-length="The field PCSTYPE must be a string with a maximum length of 15." data-val-length-max="15" id="B_PCSTYPE_' + rowindex + '" maxlength="15" name="TBATCHDTL[' + rowindex + '].PCSTYPE" type="text" value="" placeholder="">';
        tr += '    </td>';
    }
    tr += '    <td class="">';
    tr += ' <input class=" atextBoxFor " data-target="#ZoomTextBoxModal" data-toggle="modal" data-val="true" data-val-length="The field ITREM must be a string with a maximum length of 100." data-val-length-max="100" id="B_ITREM_' + rowindex + '" maxlength="100" name="TBATCHDTL[' + rowindex + '].ITREM" onblur="HasChangeBarSale();" onclick="OpenZoomTextBoxModal(this.id)" type="text" value="">';
    //tr += '        <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field ITREM must be a number." id="B_ITREM_' + rowindex + '" maxlength="100" name="TBATCHDTL[' + rowindex + '].ITREM"   type="text"  onclick = "OpenZoomTextBoxModal(this.id)" data_toggle = "modal" data_target = "#ZoomTextBoxModal" onblur = "HasChangeBarSale();" >';
    tr += '    </td>';
    tr += '   <td class=""> ';
    tr += ' <input type="button" onclick="T_Sale_FillImageModal(' + rowindex + ')" data-toggle="modal" data-target="#ViewImageModal" id="BarImagesCount_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BarImagesCount" class="btn atextBoxFor text-info" style="padding:0px" value="' + NoOfBarImages + '" readonly="readonly" placeholder="">';
    tr += '   </td> ';
    tr += '   <td class="">  ';
    if ((MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") && (ENTRYBARGENTYPE == "E" || ITMBARGENTYPE == "E")) {
        tr += '   <input type="button" value="Upload" class="btn-sm atextBoxFor" onclick="UploadBarnoImage(' + rowindex + ');" style="padding:0px" readonly="readonly" placeholder="" id="UploadBarnoImage_' + rowindex + '"> ';

    }
    else {
        tr += '   <input type="button" value="Upload" class="btn-sm atextBoxFor" onclick="UploadBarnoImage(' + rowindex + ');" style="padding:0px;display:none;" readonly="readonly" placeholder="" id="UploadBarnoImage_' + rowindex + '"> ';
    }
    tr += '   <input id="B_BarImages_' + rowindex + '" name="TBATCHDTL[' + rowindex + '].BarImages" type="hidden" readonly="readonly" placeholder="" value=' + BarImages + '> ';
    tr += '   </td> ';
    if (MENU_PARA != "SB") {
        tr += '    <td class="sticky-cell-opposite" style="right:40px;">';
        tr += '        <button class="atextBoxFor btn-info" type="button" id="btnRateHistory_"' + rowindex + ' title="Rate History" onclick="RateHistoryDetails(\'B_ITCD_' + rowindex + '\', \'B_ITSTYLE_' + rowindex + '\', \'POPUP\')" data-toggle="modal" data-target="#RateHistoryModal">Show</button>';
        tr += '    </td>';
        tr += '        <td class="sticky-cell-opposite">';
        tr += '            <button type="button" class="atextBoxFor btn-info" onclick="FillBarcodeArea(\'\', \'_T_SALE_PRODUCT_GRID\', ' + rowindex + ');" title="CLICK HERE TO EDIT BARCODEDATA"><span class="glyphicon glyphicon-pencil"></span></button>';
        tr += '        </td>';
    }
    else {
        tr += '    <td class="sticky-cell-opposite">';
        tr += '        <button class="atextBoxFor btn-info" type="button" id="btnRateHistory_"' + rowindex + ' title="Rate History" onclick="RateHistoryDetails(\'B_ITCD_' + rowindex + '\', \'B_ITSTYLE_' + rowindex + '\', \'POPUP\')" data-toggle="modal" data-target="#RateHistoryModal">Show</button>';
        tr += '    </td>';
    }
    tr += ' </tr>';

    $("#_T_SALE_PRODUCT_GRID tbody").append(tr);
    if (MENU_PARA == "SR" || MENU_PARA == "PR") {
        $("#B_AGDOCDT_" + rowindex).datepicker({ dateFormat: "dd/mm/yy", changeMonth: true, changeYear: true });
    }
    CalculateTotal_Barno();
    if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
        RateUpdate(rowindex, '#B_');

    }

    ClearBarcodeArea();
    if (MNTNOURDESIGN != "Y") {
        if ($("#MNTNBARNO").val() == "Y") {
            $("#BARCODE").focus();
        }
        else {
            $("#STYLENO").focus();
        }
    }
    else {
        if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
            $("#ITCD").focus();
        } else {
            if ($("#MNTNBARNO").val() == "Y") {
                $("#BARCODE").focus();
            }
            else {
                $("#STYLENO").focus();
            }
        }
    }

    $("#bardatachng").val("Y");
    scrollToEnd('BARGRID');
}
function RateUpdate(index, strid) {
    debugger;
    var MENU_PARA = $("#MENU_PARA").val();
    if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "PI") {
        if (strid == "") {
            var LISTPRICE = retFloat($("#LISTPRICE").val());
            var LISTDISCPER = retFloat($("#LISTDISCPER").val());
            var per = retFloat((retFloat(LISTPRICE) * retFloat(LISTDISCPER)) / 100);
            var rate = retFloat(retFloat(LISTPRICE) - retFloat(per));
            $("#RATE").val(parseFloat(rate));
        }
        else {
            var LISTPRICE = retFloat($(strid + "LISTPRICE_" + index).val());
            var LISTDISCPER = retFloat($(strid + "LISTDISCPER_" + index).val());
            var per = retFloat((retFloat(LISTPRICE) * retFloat(LISTDISCPER)) / 100);
            var rate = retFloat(retFloat(LISTPRICE) - retFloat(per));
            $(strid + "RATE_" + index).val(parseFloat(rate));
        }

    }
    else if ((MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") && (($("#BARGENTYPE").val() == "E") || ($("#BARGENTYPE").val() == "C" && $("#B_BARGENTYPE_" + index).val() == "E")) && strid == "#B_") {
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
    Sale_GetGstPer(index, strid);
    if (strid == "#B_") {
        HasChangeBarSale();
    }
}
function BarGridRateUpdate() {
    var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    for (j = 0; j <= GridRowMain - 1; j++) {
        RateUpdate(j, '#B_');
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
    var ImageDesc = $('#ImageDesc').val();
    if (document.getElementById("ImageName").value == "") return;
    var OpenImageModal = $('#BarImagesCount_' + i).val(); var actt = "";
    if (OpenImageModal == "") {
        OpenImageModal = 1; actt = "active"; $("#div_carousel_inner").html('');
    } else {
        OpenImageModal = retInt(OpenImageModal) + 1;
        actt = "";
    }
    $('#BarImagesCount_' + i).val(OpenImageModal);
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
    $("#BarImagesCount_" + ActiveBarRowIndex).val(arr.length);

}
function T_Sale_FillImageModal(index) {
    //var OpenImageModal =
    $('#ActiveBarRowIndex').val(index);
    var actt = ""; $("#div_carousel_inner").html('');
    var arr = $("#B_BarImages_" + index).val();
    arr = arr.split(String.fromCharCode(179));
    $.each(arr, function (index, value) {
        var imgname = (value.split('~')[0]);
        var id = (imgname).split('.')[0];
        var ImageDesc = (value.split('~')[1]);
        var htm = ''; if (index == 0) { actt = "active"; } else {
            actt = "";
        }
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
    var MENU_PARA = $("#MENU_PARA").val();
    if (id == "") {
        ClearAllTextBoxes("SLCD,SLNM,SLAREA,GSTNO,TAXGRPCD,PRCCD,PRCNM,AGSLCD,AGSLNM,DUEDAYS,PSLCD,TCSPER,TDSLIMIT,TDSCALCON,AMT,TCSAPPL,TDSROUNDCAL,TCSCODE,TCSNM,PARTYCD");
    }
    else {
        var code = $("#slcd_tag").val() + String.fromCharCode(181) + $("#DOCDT").val();
        var AUTONO = $("#AUTONO").val();
        var TDSCODE = $("#TDSCODE").val();
        var Partycaption = "";
        if (MENU_PARA == "PB" || MENU_PARA == "PR") {
            Partycaption = "Supplier";
        }
        else if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP") {
            Partycaption = "Buyer";
        }
        else {
            Partycaption = "Party";
        }
        $.ajax({
            type: 'POST',
            beforesend: $("#WaitingMode").show(),
            url: $("#UrlSubLedgerDetails").val(),//"@Url.Action("GetSubLedgerDetails", PageControllerName)",
            data: "&val=" + id + "&Code=" + code + "&Autono=" + AUTONO + "&linktdscode=" + TDSCODE,
            success: function (result) {
                var MSG = result.indexOf('#helpDIV');
                if (MSG >= 0) {
                    ClearAllTextBoxes("SLCD,SLNM,SLAREA,GSTNO,TAXGRPCD,PRCCD,PRCNM,AGSLCD,AGSLNM,DUEDAYS,PSLCD,TCSPER,TDSLIMIT,TDSCALCON,AMT,TCSAPPL,TDSROUNDCAL,TCSCODE,TCSNM,PARTYCD");
                    $('#SearchFldValue').val("SLCD");
                    $('#helpDIV').html(result);
                    $('#ReferanceFieldID').val("SLCD/SLNM/SLAREA/GSTNO");
                    $('#ReferanceColumn').val("1/0/3/2");
                    $('#helpDIV_Header').html(Partycaption + " Details");
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
                        $("#PARTYCD").val(returncolvalue(result, "PARTYCD"));
                        //tcs
                        $("#TDSCODE").val(returncolvalue(result, "TCSCODE"));
                        $("#TDSNM").val(returncolvalue(result, "TCSNM"));
                        $("#TCSPER").val(returncolvalue(result, "TCSPER"));
                        $("#TDSLIMIT").val(returncolvalue(result, "TDSLIMIT"));
                        $("#TDSCALCON").val(returncolvalue(result, "TDSCALCON"));
                        $("#AMT").val(returncolvalue(result, "AMT"));
                        $("#TCSAPPL").val(returncolvalue(result, "TCSAPPL"));
                        $("#TDSROUNDCAL").val(returncolvalue(result, "TDSROUNDCAL"));
                        if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
                            if (retStr(returncolvalue(result, "TCSAPPL")) == "Y") {
                                document.getElementById("TCSAUTOCAL").checked = true;
                            }
                            else {
                                document.getElementById("TCSAUTOCAL").checked = false;
                            }
                        }
                        BillAmountCalculate();//fill value of tcson
                        //
                        if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
                            $("li").removeClass("active").addClass("");
                            $(".nav-tabs li:first-child").addClass('active');
                            $(".tab-content div").removeClass("active");
                            $(".tab-content div:first-child").removeClass("tab-pane fade").addClass("tab-pane fade in active");
                            $("#PREFNO").focus();
                        }
                        else {
                            $("li").removeClass("active").addClass("");
                            $(".nav-tabs li:nth-child(2)").addClass('active');
                            //below set the  child sequence
                            $(".tab-content div").removeClass("active");
                            $(".tab-content div:nth-child(2)").removeClass("tab-pane fade").addClass("tab-pane fade in active");
                            if ($("#MNTNBARNO").val() == "Y") {
                                $("#BARCODE").focus();
                            }
                            else {
                                $("#STYLENO").focus();
                            }

                        }
                    }
                    else {
                        $('#helpDIV').html("");
                        msgInfo("" + result + " !");
                        ClearAllTextBoxes("SLCD,SLNM,SLAREA,GSTNO,TAXGRPCD,PRCCD,PRCNM,AGSLCD,AGSLNM,DUEDAYS,PSLCD,TCSPER,TDSLIMIT,TDSCALCON,AMT,TCSAPPL,TDSROUNDCAL,TCSCODE,TCSNM,PARTYCD");
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
    var allmtrljobcd = $("#ALLMTRLJOBCD").val();
    $.ajax({
        type: 'POST',
        url: $("#UrlPendOrder").val(),// "@Url.Action("GetPendOrder", PageControllerName )",
        beforesend: $("#WaitingMode").show(),
        data: $('form').serialize() + "&SLCD=" + slcd + "&SUBMITBTN=" + BtnId + "&ITCD=" + itcd + "&MTRLJOBCD=" + allmtrljobcd,
        success: function (result) {
            $("#popup").animate({ marginTop: '-10px' }, 50);
            $("#popup").html(result);
            if (BtnId != "SHOWBTN") {
                $("#btnSelectPendOrder").show();
                $("#btnSlctPOBar").hide();
                $("#btnSlctPOGrid").hide();
            }
            else {
                $("#btnSelectPendOrder").hide();
                $("#btnSlctPOBar").show();
                $("#btnSlctPOGrid").show();
            }
            //if (BtnId == "SHOWBTN") {
            //    $("#btnSelectPendOrder").hide();
            //    $("#allchkord").attr('disabled', true);
            //    var GridRow = $("#_T_SALE_PENDINGORDER_GRID > tbody > tr").length;
            //    for (var i = 0; i <= GridRow - 1; i++) {
            //        $("#Ord_Checked_" + i).attr('disabled', true);
            //    }
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
function SelectPendOrder(btnid) {
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
    if (Count > 0 && btnid != "SHOWBTN") {
        $("#haspendorddata").val("Y");
        //$("#show_order").show();
    }
    else if (Count == 0) {
        msgInfo("Please select Order !");
        return false;
    }
    $.ajax({
        type: 'post',
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlSelectPendOrder").val(),//"@Url.Action("SelectPendOrder", PageControllerName)",
        data: $('form').serialize() + "&SUBMITBTN=" + btnid,
        success: function (result) {
            if (result == "0") {
                //$("#hiddenpendordJSON").val(result);
                $("#Pending_Order").hide();
                if (Count > 0) {
                    $("#show_order").show();
                }
                msgInfo("Order Data selected ");
            }
            else {
                $("#partialdivBarCodeTab").html(result);
                var GridRow = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
                for (var i = 0; i <= GridRow - 1; i++) {
                    Sale_GetGstPer(i, '#B_');
                    RateUpdate(i, '#B_');
                }
                CalculateTotal_Barno();
                HasChangeBarSale();
            }
            $("#popup").html("");
            $("#WaitingMode").hide();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });

}

function CharmPrice(ChrmType, Rate, RoundVal) {
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

function RateHistoryDetails(ITCDId, ITNMId, TAG) {
    debugger;
    SLCD = $("#SLCD").val();
    PARTYCD = $("#PARTYCD").val();
    ITCD = $("#" + ITCDId).val();
    ITNM = $("#" + ITNMId).val();
    $.ajax({
        type: 'get',
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlRateHistory").val(),//GetRateHistoryDetails
        data: "SLCD=" + SLCD + "&PARTYCD=" + PARTYCD + "&ITCD=" + ITCD + "&ITNM=" + ITNM + "&TAG=" + TAG,
        success: function (result) {
            $("#WaitingMode").hide();
            if (TAG == "GRID") {
                $("#partialdivRateHistoryGrid").html(result);
            }
            else {
                $("#RateHistoryModal").html(result);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}

//Calculate T_OutIssProcess Amount Details
function CalculateOutIssProcessAmt_Details(i) {
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
    else {
        DISCAMT = 0;
    }
    DISCAMT = parseFloat(DISCAMT).toFixed(2);
    $("#D_DISCAMT_" + i).val(DISCAMT);

    //TDDISCOUNT AMOUNT CALCULATION
    var TDDISCAMT = 0;
    if (TDDISCTYPE == "Q") {
        TDDISCAMT = TDDISCRATE * QNTY;
    }
    else if (TDDISCTYPE == "N") {
        TDDISCAMT = TDDISCRATE * NOS;
    }
    else if (TDDISCTYPE == "P") {
        TDDISCAMT = (amount * TDDISCRATE) / 100;
    }
    else if (TDDISCTYPE == "F") {
        TDDISCAMT = TDDISCRATE;
    }
    else {
        TDDISCAMT = 0;
    }
    TDDISCAMT = parseFloat(TDDISCAMT).toFixed(2);
    $("#D_TDDISCAMT_" + i).val(TDDISCAMT);

    //SCMDISCOUNT AMOUNT CALCULATION
    var SCMDISCAMT = 0;
    if (SCMDISCTYPE == "Q") {
        SCMDISCAMT = SCMDISCRATE * QNTY;
    }
    else if (SCMDISCTYPE == "N") {
        SCMDISCAMT = SCMDISCRATE * NOS;
    }
    else if (SCMDISCTYPE == "P") {
        SCMDISCAMT = (amount * SCMDISCRATE) / 100;
    }
    else if (SCMDISCTYPE == "F") {
        SCMDISCAMT = SCMDISCRATE;
    }
    else {
        SCMDISCAMT = 0;
    }
    SCMDISCAMT = parseFloat(SCMDISCAMT).toFixed(2);
    $("#D_SCMDISCAMT_" + i).val(SCMDISCAMT);

    //TOTAL DISCOUNT AMOUNT CALCULATION
    var TOTDISCAMT = parseFloat(retFloat(DISCAMT) + retFloat(TDDISCAMT) + retFloat(SCMDISCAMT)).toFixed(2);
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
    CalculateOutIssProcessTotal_Details();

}
function CalculateOutIssProcessTotal_Details() {
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


}

function ClosePendOrder() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    //$("#popup").html("");
    var KeyID = (window.event) ? event.keyCode : e.keyCode;
    if (KeyID == 27) {
        $("#popup").html("");
    }
    else if (KeyID == undefined) {
        $("#popup").html("");
    }
}
function FillOrderToBarcode() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    var MNTNLISTPRICE = $("#MNTNLISTPRICE").val();
    var Count = 0;
    var GridRow = $("#_T_SALE_PENDINGORDER_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var Check = document.getElementById("Ord_Checked_" + i).checked;
        if (Check == true) {
            Count = Count + 1;
        }
    }
    if (Count > 1) {
        msgInfo("Please select single order ");
        return false;
    }
    for (var i = 0; i <= GridRow - 1; i++) {
        if (document.getElementById("Ord_Checked_" + i).checked == true) {
            $("#ORDDOCNO").val($("#Ord_ORDDOCNO_" + i).val());
            $("#ORDDOCDT").val($("#Ord_ORDDOCDT_" + i).val());
            $("#ORDAUTONO").val($("#Ord_ORDAUTONO_" + i).val());
            $("#ORDSLNO").val($("#Ord_ORDSLNO_" + i).val());
            $("#ITGRPNM").val($("#Ord_ITGRPNM_" + i).val());
            $("#ITSTYLE").val($("#Ord_ITSTYLE_" + i).val());
            $("#COLRNM").val($("#Ord_COLRNM_" + i).val());
            $("#SIZECD").val($("#Ord_SIZECD_" + i).val());
            $("#ITCD").val($("#Ord_ITCD_" + i).val());
            $("#COLRCD").val($("#Ord_COLRCD_" + i).val());
            $("#PDESIGN").val($("#Ord_PDESIGN_" + i).val());
            $("#RATE").val($("#Ord_RATE_" + i).val());
            $("#ITGRPCD").val($("#Ord_ITGRPCD_" + i).val());
            $("#PARTCD").val($("#Ord_PARTCD_" + i).val());
            $("#PARTNM").val($("#Ord_PARTNM_" + i).val());
            $("#PRTBARCODE").val($("#Ord_PRTBARCODE_" + i).val());
            $("#CLRBARCODE").val($("#Ord_CLRBARCODE_" + i).val());
            $("#SIZENM").val($("#Ord_SIZENM_" + i).val());
            $("#SZBARCODE").val($("#Ord_SZBARCODE_" + i).val());
            $("#UOM").val($("#Ord_UOM_" + i).val());
            $("#HSNCODE").val($("#Ord_HSNCODE_" + i).val());
            $("#BARGENTYPETEMP").val($("#Ord_BARGENTYPE_" + i).val());
            $("#GLCD").val($("#Ord_GLCD_" + i).val());
            $("#PRODGRPGSTPER").val($("#Ord_PRODGRPGSTPER_" + i).val());
            $("#GSTPER").val($("#Ord_GSTPER_" + i).val());
            $("#WPPRICEGEN").val($("#Ord_WPPRICEGEN_" + i).val());
            $("#RPPRICEGEN").val($("#Ord_RPPRICEGEN_" + i).val());
            $("#BarImages").val($("#Ord_BarImages_" + i).val());
            $("#MTRLJOBCD").val($("#Ord_MTRLJOBCD_" + i).val());
            $("#MTRLJOBNM").val($("#Ord_MTRLJOBNM_" + i).val());
            $("#MTBARCODE").val($("#Ord_MTBARCODE_" + i).val());

            var BALQTY = retFloat($("#Ord_BALQTY_" + i).val());
            var CURRENTADJQTY = retFloat($("#Ord_CURRENTADJQTY_" + i).val());
            var qnty = retFloat(BALQTY - CURRENTADJQTY).toFixed(2);
            $("#QNTY").val(qnty);

            if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
                changeBARGENTYPE();
            }
            if ((MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP" || MENU_PARA == "PI") && MNTNLISTPRICE == "Y") {
                $("#LISTPRICE").val($("#Ord_RATE_" + i).val());
            }
        }
    }
    $("#popup").html("");
}
function CopyMtrljobcd() {
    var GridRow = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    if (GridRow != 0) {
        var prev_mtrljobcd = $("#B_MTRLJOBCD_0").val();
        var prev_mtrljobnm = $("#B_MTRLJOBNM_0").val();
        var prev_mtrljobbarcd = $("#B_MTBARCODE_0").val();
        for (var i = 0; i <= GridRow - 1; i++) {
            $("#B_MTRLJOBCD_" + i).val(prev_mtrljobcd);
            $("#B_MTRLJOBNM_" + i).val(prev_mtrljobnm);
            $("#B_MTBARCODE_" + i).val(prev_mtrljobbarcd);
        }
    }
}
function GetItcd(id) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    var ModuleCode = $("#ModuleCode").val();
    if (id == "") {
        ClearAllTextBoxes("ITCD,ITSTYLE,UOM,STYLENO,ITGRPCD,ITGRPNM,HSNCODE,PRODGRPGSTPER,GSTPER,BarImages,GLCD");
    }
    else {
        var code = $("#ITGRPCD").val() + String.fromCharCode(181) + $("#DOCDT").val() + String.fromCharCode(181) + $("#TAXGRPCD").val() + String.fromCharCode(181) + $("#GOCD").val() + String.fromCharCode(181) + $("#PRCCD").val() + String.fromCharCode(181) + $("#MTRLJOBCD").val() + String.fromCharCode(181) + $("#BARCODE").val() + String.fromCharCode(181) + $("#RATE").val();
        $.ajax({
            type: 'POST',
            beforesend: $("#WaitingMode").show(),
            url: $("#UrlItemDetails").val(),// "@Url.Action("GetItemDetails", PageControllerName)",
            data: "&Code=" + code + "&val=" + id,
            success: function (result) {
                var MSG = result.indexOf('#helpDIV');
                if (MSG >= 0) {
                    ClearAllTextBoxes("ITCD,ITSTYLE,UOM,STYLENO,ITGRPCD,ITGRPNM,HSNCODE,PRODGRPGSTPER,GSTPER,BarImages,GLCD");
                    $('#SearchFldValue').val("ITCD");
                    $('#helpDIV').html(result);
                    $('#ReferanceFieldID').val("ITCD/ITSTYLE/UOM/STYLENO/ITGRPCD/ITGRPNM");
                    $('#ReferanceColumn').val("2/1/3/0/5/4");
                    $('#helpDIV_Header').html("Item Details");
                }
                else {
                    var MSG = result.indexOf(String.fromCharCode(181));
                    if (MSG >= 0) {
                        $("#ITCD").val(returncolvalue(result, "itcd"));
                        $("#ITSTYLE").val(returncolvalue(result, "ITSTYLE"));
                        $("#UOM").val(returncolvalue(result, "uomcd"));
                        $("#STYLENO").val(returncolvalue(result, "STYLENO"));
                        $("#ITGRPCD").val(returncolvalue(result, "ITGRPCD"));
                        $("#ITGRPNM").val(returncolvalue(result, "ITGRPNM"));
                        $("#HSNCODE").val(returncolvalue(result, "HSNCODE"));
                        $("#PRODGRPGSTPER").val(returncolvalue(result, "PRODGRPGSTPER"));
                        $("#GSTPER").val(returncolvalue(result, "GSTPER"));
                        $("#BarImages").val(returncolvalue(result, "BARIMAGE"));
                        $("#GLCD").val(returncolvalue(result, "GLCD"));
                        $("#BARGENTYPETEMP").val(returncolvalue(result, "bargentype"));
                        hlpblurval = id;
                        if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
                            changeBARGENTYPE();
                        }
                        var value = modify_check();
                        if (value == "true") {
                            RateHistoryDetails('ITCD', 'ITSTYLE', 'GRID');
                        }
                        if (ModuleCode.indexOf("SALESCLOTH") != -1) {
                            if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
                                $("#NOS").focus();
                            }
                            else {
                                $("#CUTLENGTH").focus();
                            }
                        }
                    }
                    else {
                        $('#helpDIV').html("");
                        msgInfo("" + result + " !");
                        ClearAllTextBoxes("ITCD,ITSTYLE,UOM,STYLENO,ITGRPCD,ITGRPNM,HSNCODE,PRODGRPGSTPER,GSTPER,BarImages,GLCD");
                        message_value = "ITCD";
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
function CalculateBargridQnty(tableid, index) {
    debugger;
    var CUTLENGTHID = "", NOSID = "", QNTY = "";
    if (tableid == "_T_SALE_PRODUCT_GRID") {
        CUTLENGTHID = "B_CUTLENGTH_" + index;
        NOSID = "B_NOS_" + index;
        QNTYID = "B_QNTY_" + index;
    }
    else {
        CUTLENGTHID = "CUTLENGTH";
        NOSID = "NOS";
        QNTYID = "QNTY";
    }
    if (retFloat($("#" + CUTLENGTHID).val()) != 0 && retFloat($("#" + NOSID).val()) != 0) {
        var qnty = retFloat($("#" + CUTLENGTHID).val()) * retFloat($("#" + NOSID).val());
        $("#" + QNTYID).val(retFloat(qnty));
    }
    if (tableid == "_T_SALE_PRODUCT_GRID") {
        CalculateTotal_Barno();
        HasChangeBarSale()
    }
}
var hlpblurval = "";
function GetHelpBlur_T_Sale(urlstring, caption, hlpfield, blurflds, dependfldIds, formdata) {
    debugger;
    const keyName = event.key;
    const keyType = event.type;
    var blurvalue = "";
    var value = $("#" + hlpfield).val();
    var exactbarno = "Y";
    if (value == hlpblurval && (keyType == "mousedown" || keyName == "F2")) {
        value = "";
        exactbarno = "N";
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
        ClearBarcodeArea();
    }
    else {
        var Data = "";
        if (formdata == "Y") {
            Data = $('form').serialize() + "&val=" + value + "&Code=" + dependfldIds + "&exactbarno=" + exactbarno;
        }
        else {
            Data = "&val=" + value + "&Code=" + dependfldIds + "&exactbarno=" + exactbarno;
        }
        if (!emptyFieldCheck("Please Select / Enter Document Date", "DOCDT")) { return false; }
        if ($("#TAXGRPCD").val() == "") { $("#BARCODE").val(""); msgInfo("TaxGrp. Code not available.Please Select / Enter another Party Code to get TaxGrp. Code"); message_value = "SLCD"; return false; }
        if ($("#PRCCD").val() == "") { $("#BARCODE").val(""); msgInfo("Price Code not available.Please Select / Enter another Party Code to get Price Code"); message_value = "SLCD"; return false; }

        $.ajax({
            type: 'POST',
            url: urlstring,
            //data: "&val=" + value + "&Code=" + dependfldIds,
            data: Data,
            success: function (result) {
                var MSG = result.indexOf('#helpDIV');
                if (MSG >= 0) {
                    if (keyType != "mousedown") {
                        ClearBarcodeArea();
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
                        FillBarcodeArea(result);
                        changeBARGENTYPE();
                        hlpblurval = $("#" + hlpfield).val();
                    }
                    else {
                        $('#helpDIV').html("");
                        msgInfo("" + result + " !");
                        ClearBarcodeArea();
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
function CheckBillNumber() {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var BILL_NO = $("#PREFNO").val();
    var SUPPLIER = $("#SLCD").val();
    var AUTO_NO = $("#AUTONO").val();
    if (BILL_NO != "") {
        $.ajax({
            type: 'GET',
            url: $("#UrlCheckBillNumber").val(), //"@Url.Action("CheckBillNumber", PageControllerName)",
            data:
            {
                BILL_NO: BILL_NO, SUPPLIER: SUPPLIER, AUTO_NO: AUTO_NO,
            },
            success: function (result) {
                if (result == "1") {
                    msgInfo("Bill Number Already Exists, for Particular Sub Code : Please Enter a Different Bill Number !! ");
                    message_value = "PREFNO";
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
function modify_check() {
    debugger;
    var ITCD = $("#ITCD").val();
    var ITSTYLE = $("#ITSTYLE").val();
    var Last_ITCD = $("#Last_ITCD").val();
    if (Last_ITCD != ITCD || ITSTYLE == "") {
        //Clear_data();
        Last_ITCD = ITCD;
        $("#Last_ITCD").val(Last_ITCD);
        return "true";
    }
    else {
        return "false";
    }

}
function Sale_GetTTXNDTLDetails() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();

    var Partycaption = "";
    if (MENU_PARA == "PB" || MENU_PARA == "PR") {
        Partycaption = "Supplier";
    }
    else if (MENU_PARA == "SBPCK" || MENU_PARA == "SB" || MENU_PARA == "SBDIR" || MENU_PARA == "SR" || MENU_PARA == "SBEXP") {
        Partycaption = "Buyer";
    }
    else {
        Partycaption = "Party";
    }

    if ($("#SLCD").val() == "") { msgInfo(Partycaption + " Code not available.Please Select / Enter another " + Partycaption + " Code"); message_value = "SLCD"; return false; }
    if ($("#TAXGRPCD").val() == "") { msgInfo("TaxGrp. Code not available.Please Select / Enter another Party Code to get TaxGrp. Code"); message_value = "SLCD"; return false; }
    var FDT = $("#FDT").val();
    var FDT = $("#TDT").val();
    var R_DOCNO = $("#R_DOCNO").val();
    var R_BARNO = $("#R_BARNO").val();
    var TAXGRPCD = $("#TAXGRPCD").val();
    var SLCD = $("#SLCD").val();
    var datachng = "";
    var value = agdocnomodify_check();
    if (value == "true") {
        datachng = "Y";
    }
    $.ajax({
        type: 'POST',
        url: $("#UrlTTXNDTLDetails").val(),//"@Url.Action("GetTTXNDTLDetails", PageControllerName )"
        beforesend: $("#WaitingMode").show(),
        data: $('form').serialize() + "&FDT=" + FDT + "&FDT=" + FDT + "&R_DOCNO=" + R_DOCNO + "&R_BARNO=" + R_BARNO + "&TAXGRPCD=" + TAXGRPCD + "&SLCD=" + SLCD + "&datachng=" + datachng,
        success: function (result) {
            $("#popup_agdocno").animate({ marginTop: '-10px' }, 50);
            $("#popup_agdocno").html(result);
            $("#WaitingMode").hide();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
function Sale_SelectTTXNDTLDetails() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var Count = 0;
    var GridRow = $("#T_SALE_POPUP_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var Check = document.getElementById("P_Checked_" + i).checked;
        if (Check == true) {
            Count = Count + 1;
        }
    }
    if (Count == 0) {
        msgInfo("Please select a Against Docno. !");
        return false;
    }
    $.ajax({
        type: 'post',
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlSelectTTXNDTLDetails").val(),//"@Url.Action("SelectTTXNDTLDetails", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            if (result.indexOf("_T_SALE_PRODUCT_GRID") >= 0) {
                $("#partialdivBarCodeTab").html(result);
                CalculateTotal_Barno();
                $("#popup_agdocno").html("");
            }
            else {
                msgInfo(result);
                $("#popup_agdocno").html("");
                return false;
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
function Sale_CloseTTXNDTLDetails() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var KeyID = (window.event) ? event.keyCode : e.keyCode;
    if (KeyID == 27) {
        $("#popup_agdocno").html("");
    }
    else if (KeyID == undefined) {
        $("#popup_agdocno").html("");
    }
}
function agdocnomodify_check() {
    debugger;
    var SLCD = $("#SLCD").val();
    var FDT = $("#FDT").val();
    var TDT = $("#TDT").val();
    var R_DOCNO = $("#R_DOCNO").val();
    var R_BARNO = $("#R_BARNO").val();
    var Last_SLCD = $("#Last_SLCD").val();
    var Last_FDT = $("#Last_FDT").val();
    var Last_TDT = $("#Last_TDT").val();
    var Last_R_DOCNO = $("#Last_R_DOCNO").val();
    var Last_BARNO = $("#Last_BARNO").val();

    if (Last_SLCD != SLCD || Last_FDT != FDT || Last_TDT != TDT || Last_R_DOCNO != R_DOCNO || Last_BARNO != R_BARNO) {
        $("#Last_SLCD").val(SLCD);
        $("#Last_FDT").val(FDT);
        $("#Last_TDT").val(TDT);
        $("#Last_R_DOCNO").val(R_DOCNO);
        $("#Last_BARNO").val(R_BARNO);
        return "true";
    }
    else {
        return "false";
    }

}
function modify_check_stylebarno() {
    debugger;
    var BARCODE = $("#BARCODE").val();
    var Last_BARCODE = $("#Last_BARCODE").val();
    var STYLENO = $("#STYLENO").val();
    var Last_STYLENO = $("#Last_STYLENO").val();

    if (Last_BARCODE != BARCODE || STYLENO != Last_STYLENO) {
        $("#Last_BARCODE").val(BARCODE);
        $("#Last_STYLENO").val(STYLENO);
        return "true";
    }
    else {
        return "false";
    }
}
function DocdtChange(docdtfld) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    DocumentDateCHK(docdtfld);
    if (MENU_PARA == "PB" || MENU_PARA == "OP" || MENU_PARA == "OTH") {
        $("#PREFDT").val($("#DOCDT").val());
    }
}
function Edit_Pageno_slno() {
    debugger;
    var GridRow = $("#_T_SALE_DETAIL_GRID > tbody > tr").length;
    if (GridRow != 0) {
        for (var i = 0; i <= GridRow - 1; i++) {
            $("#D_PAGENO_" + i).attr("readonly", false);
            $("#D_PAGESLNO_" + i).attr("readonly", false);
            if (i == 0) $("#D_PAGENO_" + i).focus();
        }
    }
    $("#edit_page_slno").hide();
    $("#update_page_slno").show();
    $("#cancel_page_slno").show();
}

function Cancel_Pageno_slno() {
    location.reload();
}
function Update_Pageno_slno() {
    debugger;
    $.ajax({
        type: 'post',
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlUpdatePagenoSlno").val(),//"@Url.Action("Update_PagenoSlno", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            $("#WaitingMode").hide();
            if (result == "1") {
                msgSuccess1("Update Successfully !");
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
function RemoveStyleBarno(RemoveFldId) {
    $("#" + RemoveFldId).val("");
}

function SelectUOMCode(id, i) {
    if (id == "") {
        $("#D_BLUOMCD_" + i).val("");
    }
    else {
        $.ajax({
            type: 'GET',
            url: $("#UrlGetUOMDetails").val(),//"@Url.Action("GetUOMDetails", PageControllerName)",
            data: {
                val: id
            },
            success: function (result) {
                var MSG = result.indexOf(String.fromCharCode(181));
                if (MSG < 0) {
                    $("#Msgdiv1").show();
                    $("#info").show();
                    $("#msgbody_info").html(" Invalid  UOM Code !! ");
                    $("#D_BLUOMCD_" + i).val("");
                    $("#btnok").focus();
                    message_value = "D_BLUOMCD_" + i;
                }
                else {
                    $("#tempHDD").val(result);
                    var str = $("#tempHDD").val().split(String.fromCharCode(181));
                    $("#D_BLUOMCD_" + i).val(str[0]);
                    UpdateBarCodeRow_FrmDet(i);
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





