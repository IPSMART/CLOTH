function GetBarnoDetails(barhlpId, HelpFrom) {
    debugger;
    var ID = $("#" + barhlpId).val();
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var BARNO = "", ITCD = "", duplicate = "", slno = 0;
    debugger;
    if (ID == "") {
        if (barhlpId == 'M_BARCODE' || barhlpId == 'M_STYLENO') {
            ClearAllTextBoxes("M_BARCODE,M_STYLENO,MTRLJOBCD,PARTCD");
        }
        else if (barhlpId == 'R_BARCODE' || barhlpId == 'R_STYLENO') {
            ClearAllTextBoxes("R_BARCODE,R_STYLENO,MTRLJOBCD,PARTCD");
        }
    }
    else {
        if (!emptyFieldCheck("Please Select / Enter Document Date", "DOCDT")) { return false; }
        var BARCODE = "";
        if (barhlpId == 'M_BARCODE' || barhlpId == 'M_STYLENO') {
            BARCODE = $("#M_BARCODE").val();
            if ($("#TAXGRPCD").val() == "") { $("#M_BARCODE").val(""); msgInfo("TaxGrp. Code not available.Please Select / Enter another Party Code to get TaxGrp. Code"); message_value = "SLCD"; return false; }
            if ($("#PRCCD").val() == "") { $("#M_BARCODE").val(""); msgInfo("Price Code not available.Please Select / Enter another Party Code to get Price Code"); message_value = "SLCD"; return false; }
        } else if (barhlpId == 'R_BARCODE' || barhlpId == 'R_STYLENO') {
            BARCODE = $("#R_BARCODE").val();
            if ($("#TAXGRPCD").val() == "") { $("#R_BARCODE").val(""); msgInfo("TaxGrp. Code not available.Please Select / Enter another Party Code to get TaxGrp. Code"); message_value = "SLCD"; return false; }
            if ($("#PRCCD").val() == "") { $("#R_BARCODE").val(""); msgInfo("Price Code not available.Please Select / Enter another Party Code to get Price Code"); message_value = "SLCD"; return false; }
        }

        var MTRLJOBCD = $("#MTRLJOBCD").val();
        var PARTCD = $("#PARTCD").val();
        var docdt = $("#DOCDT").val();
        var taxgrpcd = $("#TAXGRPCD").val();
        var gocd = $("#GOCD").val();
        var prccd = $("#PRCCD").val();
        var allmtrljobcd = $("#ALLMTRLJOBCD").val();

        var hlpfieldid = "", hlpfieldindex = "", ReferanceFieldID = "", ReferanceFieldIndex = "";
        if (barhlpId == "M_BARCODE") {
            hlpfieldid = "M_BARCODE";
            hlpfieldindex = "0";
            ReferanceFieldID = "/M_STYLENO";
            ReferanceFieldIndex = "/3";
        }
        else if (barhlpId == "M_STYLENO") {
            hlpfieldid = "M_STYLENO";
            hlpfieldindex = "3";
            ReferanceFieldID = "/M_BARCODE";
            ReferanceFieldIndex = "/0";
        }
        else if (barhlpId == "R_BARCODE") {
            hlpfieldid = "R_BARCODE";
            hlpfieldindex = "0";
            ReferanceFieldID = "/R_STYLENO";
            ReferanceFieldIndex = "/3";
        }
        else if (barhlpId == "R_STYLENO") {
            hlpfieldid = "R_STYLENO";
            hlpfieldindex = "3";
            ReferanceFieldID = "/R_BARCODE";
            ReferanceFieldIndex = "/0";
        }
        var code = MTRLJOBCD + String.fromCharCode(181) + PARTCD + String.fromCharCode(181) + docdt + String.fromCharCode(181) + taxgrpcd + String.fromCharCode(181) + gocd + String.fromCharCode(181) + prccd + String.fromCharCode(181) + allmtrljobcd + String.fromCharCode(181) + HelpFrom + String.fromCharCode(181) + BARCODE;

        $.ajax({
            type: 'POST',
            beforesend: $("#WaitingMode").show(),
            url: $("#UrlBarnoDetails").val(),//"@Url.Action("GetBarCodeDetails", PageControllerName)",
            data: "&val=" + ID + "&Code=" + code,
            success: function (result) {
                var MSG = result.indexOf('#helpDIV');
                if (MSG >= 0) {
                    if (barhlpId == 'M_BARCODE' || barhlpId == 'M_STYLENO') {
                        ClearAllTextBoxes("M_BARCODE,M_STYLENO,MTRLJOBCD,PARTCD");
                        $('#SearchFldValue').val(hlpfieldid);
                        $('#helpDIV').html(result);
                        $('#ReferanceFieldID').val(hlpfieldid + ReferanceFieldID + '/MTRLJOBCD/PARTCD');
                        $('#ReferanceColumn').val(hlpfieldindex + ReferanceFieldIndex + '/2/5');
                        $('#helpDIV_Header').html('Barno Details');
                    } else if (barhlpId == 'R_BARCODE' || barhlpId == 'R_STYLENO') {
                        ClearAllTextBoxes("R_BARCODE,R_STYLENO,MTRLJOBCD,PARTCD");
                        $('#SearchFldValue').val(hlpfieldid);
                        $('#helpDIV').html(result);
                        $('#ReferanceFieldID').val(hlpfieldid + ReferanceFieldID + '/MTRLJOBCD/PARTCD');
                        $('#ReferanceColumn').val(hlpfieldindex + ReferanceFieldIndex + '/2/5');
                        $('#helpDIV_Header').html('Barno Details');
                    }

                }
                else {
                    var MSG = result.indexOf(String.fromCharCode(181));
                    if (MSG >= 0) {
                        if (barhlpId == 'M_BARCODE' || barhlpId == 'M_STYLENO') {
                            ClearAllTextBoxes("M_BARCODE,M_STYLENO,MTRLJOBCD,PARTCD");
                            BARNO = returncolvalue(result, "BARNO");
                            //ITCD = returncolvalue(result, "ITCD");
                            var GridRowMain = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
                            for (j = 0; j <= GridRowMain - 1; j++) {
                                if (BARNO == $("#B_BARNO_" + j).val()) {
                                    slno = $("#B_SLNO_" + j).val();

                                }
                                duplicate = false;
                            }
                            if (slno > 0) {
                                UpdateBarCodeRow(result, slno);
                            }

                            else {
                                AddMainRow(result);
                            }

                        } else if (barhlpId == 'R_BARCODE' || barhlpId == 'R_STYLENO') {
                            ClearAllTextBoxes("R_BARCODE,R_STYLENO,MTRLJOBCD,PARTCD");
                            AddReturnRow(result);
                        }
                    }
                    else {
                        $('#helpDIV').html("");
                        msgInfo("" + result + " !");
                        if (barhlpId == 'M_BARCODE' || barhlpId == 'M_STYLENO') {
                            ClearAllTextBoxes("M_BARCODE,M_STYLENO,MTRLJOBCD,PARTCD");
                            message_value = hlpfieldid;
                        } else if (barhlpId == 'R_BARCODE' || barhlpId == 'R_STYLENO') {
                            ClearAllTextBoxes("R_BARCODE,R_STYLENO,MTRLJOBCD,PARTCD");
                            message_value = hlpfieldid;
                        }

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
            //CalculateRowAmt();
            CalculateTotal();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });

}
function DeleteReturnRow() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    $.ajax({
        type: 'POST',
        url: $("#UrlDeleteReturnRow").val(),// "@Url.Action("DeleteRow", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            $("#partialdivReturn").animate({ marginTop: '0px' }, 50);
            $("#partialdivReturn").html(result);
            Checked_Disable();
            //CalculateRowAmt();
            CalculateTotal();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });

}
function AddMainRow(hlpstr) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    var MNTNPART = $("#MNTNPART").val();
    var MNTNCOLOR = $("#MNTNCOLOR").val();
    var MNTNSIZE = $("#MNTNSIZE").val();
    var MNTNSHADE = $("#MNTNSHADE").val();
    var INCLRATEASK = $("#INCLRATEASK").val();

    var BARNO = returncolvalue(hlpstr, "BARNO");
    var ITGRPNM = returncolvalue(hlpstr, "ITGRPNM");
    var ITGRPCD = returncolvalue(hlpstr, "ITGRPCD");
    var ITCD = returncolvalue(hlpstr, "ITCD");
    var ITSTYLE = returncolvalue(hlpstr, "ITSTYLE");
    var COLRNM = returncolvalue(hlpstr, "COLRNM");
    var SIZECD = returncolvalue(hlpstr, "SIZECD");
    var BALSTOCK = returncolvalue(hlpstr, "BALQNTY");
    var NEGSTOCK = returncolvalue(hlpstr, "NEGSTOCK");
    var NEGSTOCK = returncolvalue(hlpstr, "NEGSTOCK");
    var UOM = returncolvalue(hlpstr, "UOMCD");
    var NOS = returncolvalue(hlpstr, "NOS");
    var QNTY = returncolvalue(hlpstr, "QNTY");
    if (UOM == "PCS") {
        NOS = 1;
        QNTY = NOS;
    }

    var RATE = returncolvalue(hlpstr, "RATE");
    var STKTYPE = returncolvalue(hlpstr, "STKTYPE");
    var GLCD = returncolvalue(hlpstr, "GLCD");
    var GROSSAMT = retFloat(QNTY) * retFloat(RATE);
    var PRODGRPGSTPER = returncolvalue(hlpstr, "PRODGRPGSTPER");
    var IGSTPER = 0; var CGSTPER = 0; var SGSTPER = 0;
    var MTRLJOBCD = returncolvalue(hlpstr, "MTRLJOBCD");
    var HSNCODE = returncolvalue(hlpstr, "HSNCODE");
    var DISCTYPE = returncolvalue(hlpstr, "DISCTYPE");
    var DISCRATE = returncolvalue(hlpstr, "DISCRATE");
    var GSTPERstr = retGstPerstr(PRODGRPGSTPER, RATE, DISCTYPE, DISCRATE);
    var GSTPERarr = GSTPERstr.split(','); var GSTPER = 0;
    $.each(GSTPERarr, function () { GSTPER += parseFloat(this) || 0; IGSTPER = parseFloat(GSTPERarr[0]) || 0; CGSTPER = parseFloat(GSTPERarr[1]) || 0; SGSTPER = parseFloat(GSTPERarr[2]) || 0; });

    var BarImages = returncolvalue(hlpstr, "BARIMAGE");
    var NoOfBarImages = BarImages.split(String.fromCharCode(179)).length;
    if (BarImages == '') { NoOfBarImages = ''; }
    var SLNO = 1;
    var rowindex = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
    SLNO = rowindex + 1;
    var INCLRATE = 0;
    if (INCLRATEASK == "Y") {
        INCLRATE = returncolvalue(hlpstr, "RATE");
    }
    var tr = "";
    tr += '<tr style="font-size:12px; font-weight:bold;">';
    tr += ' <td class="sticky-cell" title="true">';
    tr += '   <input tabindex="-1" data-val="true" data-val-required="The Checked field is required." id="B_Checked_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].Checked" type="checkbox" value="true"><input name="TsalePos_TBATCHDTL[' + rowindex + '].Checked" type="hidden" value="false">';
    tr += '   <input data-val="true" data-val-number="The field TXNSLNO must be a number." data-val-required="The TXNSLNO field is required." id="B_TXNSLNO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TXNSLNO" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field MTRLJOBCD must be a string with a maximum length of 2." data-val-length-max="2" id="B_MTRLJOBCD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].MTRLJOBCD" type="hidden" value="' + MTRLJOBCD + '">';
    tr += '   <input data-val="true" data-val-number="The field FLAGMTR must be a number." id="B_FLAGMTR_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].FLAGMTR" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field BLQNTY must be a number." id="B_BLQNTY_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BLQNTY" type="hidden" value="">';
    //tr += '   <input data-val="true" data-val-length="The field HSNCODE must be a string with a maximum length of 8." data-val-length-max="8" id="B_HSNCODE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].HSNCODE" type="hidden" value="' + HSNCODE + '">';
    tr += '   <input data-val="true" data-val-length="The field LOCABIN must be a string with a maximum length of 10." data-val-length-max="10" id="B_LOCABIN_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].LOCABIN" type="hidden" value="">';
    tr += '   <input id="B_BARGENTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BARGENTYPE" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field GLCD must be a string with a maximum length of 8." data-val-length-max="8" id="B_GLCD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].GLCD" type="hidden" value="' + GLCD + '">';
    tr += '   <input id="B_CLASS1CD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CLASS1CD" type="hidden" value="">';
    //tr += '   <input data-val="true" data-val-number="The field AMT must be a number." id="B_GROSSAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].GROSSAMT" type="hidden" value="' + GROSSAMT + '">';
    //tr += '   <input data-val="true" data-val-number="The field DISCAMT must be a number." id="B_DISCAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCAMT" type="hidden" value="">';
    //tr += '   <input data-val="true" data-val-number="The field TXBLVAL must be a number." id="B_TXBLVAL_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TXBLVAL" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field TOTDISCAMT must be a number." id="B_TOTDISCAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TOTDISCAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field IGSTPER must be a number." id="B_IGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].IGSTPER" type="hidden" value="' + IGSTPER + '">';
    tr += '   <input data-val="true" data-val-number="The field IGSTAMT must be a number." id="B_IGSTAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].IGSTAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field CGSTPER must be a number." id="B_CGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CGSTPER" type="hidden" value="' + CGSTPER + '">';
    tr += '   <input data-val="true" data-val-number="The field CGSTAMT must be a number." id="B_CGSTAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CGSTAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field SGSTPER must be a number." id="B_SGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SGSTPER" type="hidden" value="' + SGSTPER + '">';
    tr += '   <input data-val="true" data-val-number="The field SGSTAMT must be a number." id="B_SGSTAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SGSTAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field CESSPER must be a number." id="B_CESSPER_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CESSPER" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field CESSAMT must be a number." id="B_CESSAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CESSAMT" type="hidden" value="">';
    if (MNTNSHADE != "Y") {
        tr += '   <input id="B_SHADE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SHADE" type="hidden" value="">';
    }
    tr += '   <input data-val="true" data-val-number="The field DISCONBILLPERROW must be a number." id="B_DISCONBILLPERROW_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCONBILLPERROW" type="hidden" value="">';
    tr += ' </td>';
    tr += ' <td class="sticky-cell" style="left:20px;" title="1">';
    tr += '     <input class=" atextBoxFor " data-val="true" data-val-number="The field SLNO must be a number." data-val-required="The SLNO field is required." id="B_SLNO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SLNO" readonly="readonly" type="text" value="' + SLNO + '">';
    tr += ' </td>';
    tr += ' <td class="sticky-cell" style="left:50px" title="">';
    tr += '     <input class=" atextBoxFor " data-val="true" data-val-length="The field BARNO must be a string with a maximum length of 25." data-val-length-max="25" data-val-required="The BARNO field is required." id="B_BARNO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BARNO" readonly="readonly" type="text" value="' + BARNO + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_ITGRPNM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ITGRPNM" readonly="readonly" type="text" value="' + ITGRPNM + '">';
    tr += '     <input id="B_ITGRPCD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ITGRPCD" type="hidden" value="">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field ITCD must be a string with a maximum length of 8." data-val-length-max="8" id="B_ITCD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ITCD" readonly="readonly" type="text" value="' + ITCD + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_ITSTYLE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ITSTYLE" readonly="readonly" type="text" value="' + ITSTYLE + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_HSNCODE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].HSNCODE" readonly="readonly" type="text" value="' + HSNCODE + '">';
    tr += ' </td>';
    if (MNTNCOLOR == "Y") {
        tr += ' <td class="" title="">';
        tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_COLRNM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].COLRNM" readonly="readonly" type="text" value="' + COLRNM + '">';
        tr += ' </td>';
    }
    if (MNTNSIZE == "Y") {
        tr += ' <td class="" title="">';
        tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field SIZECD must be a string with a maximum length of 4." data-val-length-max="4" id="B_SIZECD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SIZECD" readonly="readonly" type="text" value="' + SIZECD + '">';
        tr += '     <input id="B_SIZENM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SIZENM" type="hidden" value="">';
        tr += '     <input id="B_SZBARCODE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SZBARCODE" type="hidden" value="">';
        tr += ' </td>';
    }
    if (MNTNSHADE == "Y") {
        tr += ' <td class="" title="">';
        tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field SHADE must be a string with a maximum length of 15." data-val-length-max="15" id="B_SHADE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SHADE" type="text" value="">';
        tr += ' </td>';
    }

    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor  text-box single-line" data-val="true" data-val-number="The field BALSTOCK must be a number." id="B_BALSTOCK_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BALSTOCK" readonly="readonly" style="text-align: right;" type="text" value="' + BALSTOCK + '">';
    tr += '     <input id="B_NEGSTOCK_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].NEGSTOCK" type="hidden" value="' + NEGSTOCK + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NOS must be a number." id="B_NOS_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].NOS" onblur = "CalculateGridQnty(\'_T_SALE_POS_PRODUCT_GRID\',' + rowindex + ',this.value);", onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" value="' + NOS + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field QNTY must be a number." id="B_QNTY_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].QNTY" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" onblur="CalculateGridQnty(\'_T_SALE_POS_PRODUCT_GRID\',' + rowindex + ',this.value);" value="' + QNTY + '" >';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor" id="B_UOM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].UOM" readonly="readonly" type="text" value="' + UOM + '">';
    tr += ' </td>';
    if (INCLRATEASK == "Y") {
        tr += '     <td class="" title="">';
        tr += '         <input class="atextBoxFor text-right" data-val="true" data-val-number="The field INCLRATE must be a number." id="INCLRATE_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].INCLRATE" onchange = "CalculateInclusiveRate(' + rowindex + ',\'_T_SALE_POS_PRODUCT_GRID\');CalculateRowAmt(\'_T_SALE_POS_PRODUCT_GRID\',' + rowindex + ');", onkeypress="return numericOnly(this,2);" style="font-weight:bold;background-color: bisque;" type="text" value="' + INCLRATE + '">';
        tr += '     <input id="B_RATE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].RATE" type="hidden" value="' + RATE + '">';
        tr += '     </td>';
        //tr += ' <td class="" title="">';
        //tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RATE must be a number." id="B_RATE_' + rowindex + '" maxlength="14" name="TsalePos_TBATCHDTL[' + rowindex + '].RATE" readonly="readonly" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="' + RATE + '">';
        //tr += ' </td>';
    }
    else {
        tr += ' <td class="" title="">';
        tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RATE must be a number." id="B_RATE_' + rowindex + '" maxlength="14" name="TsalePos_TBATCHDTL[' + rowindex + '].RATE" onchange="CalculateRowAmt(\'_T_SALE_POS_PRODUCT_GRID\',' + rowindex + ');" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="' + RATE + '">';
        tr += ' </td>';
    }
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_GROSSAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].GROSSAMT" style="text-align: right;" readonly="readonly" type="text" >';
    tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <select class="atextBoxFor" data-val="true" data-val-length="The field DISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_DISCTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCTYPE" onchange="CalculateInclusiveRate(' + rowindex + ',\'_T_SALE_POS_PRODUCT_GRID\');" ><option value="P">%</option>';
    tr += '         <option value="N">Nos</option>';
    tr += '         <option value="Q">Qnty</option>';
    tr += '         <option value="F">Fixed</option>';
    tr += '     </select>';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field DISCRATE must be a number." id="B_DISCRATE_' + rowindex + '" maxlength="10" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCRATE" onblur="CalculateInclusiveRate(' + rowindex + ',\'_T_SALE_POS_PRODUCT_GRID\');" onkeydown="CopyLastDiscData(this.value,B_DISCTYPE_' + rowindex + '.value,\'B_DISCRATE_\',\'B_DISCTYPE_\',\'B_ITCD_\',\'_T_SALE_POS_PRODUCT_GRID\');RemoveLastDiscData(\'B_DISCRATE_\',\'B_ITCD_\',\'_T_SALE_POS_PRODUCT_GRID\');"  onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_DISCAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCAMT" style="text-align: right;" readonly="readonly" type="text" >';
    tr += ' </td>';

    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NETDISCAMT must be a number." id="B_NETDISCAMT_' + rowindex + '" maxlength="10" name="TsalePos_TBATCHDTL[' + rowindex + '].NETDISCAMT" onblur="CalculateInclusiveRate(' + rowindex + ',\'_T_SALE_POS_PRODUCT_GRID\');"  onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    //
    tr += ' <td class="">';
    tr += '     <select class="atextBoxFor" data-val="true" data-val-length="The field SCMDISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_SCMDISCTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SCMDISCTYPE" onchange="CalculateInclusiveRate(' + rowindex + ',\'_T_SALE_POS_PRODUCT_GRID\');" ><option value="P">%</option>';
    tr += '         <option value="N">Nos</option>';
    tr += '         <option value="Q">Qnty</option>';
    tr += '         <option value="F">Fixed</option>';
    tr += '     </select>';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field SCMDISCRATE must be a number." id="B_SCMDISCRATE_' + rowindex + '" maxlength="10" name="TsalePos_TBATCHDTL[' + rowindex + '].SCMDISCRATE" onblur="CalculateInclusiveRate(' + rowindex + ',\'_T_SALE_POS_PRODUCT_GRID\');" onkeydown="CopyLastDiscData(this.value,B_SCMDISCTYPE_' + rowindex + '.value,\'B_SCMDISCRATE_\',\'B_SCMDISCTYPE_\',\'B_ITCD_\',\'_T_SALE_POS_PRODUCT_GRID\');RemoveLastDiscData(\'B_SCMDISCRATE_\',\'B_ITCD_\',\'_T_SALE_POS_PRODUCT_GRID\');"  onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_SCMDISCAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SCMDISCAMT" style="text-align: right;" readonly="readonly" type="text" >';
    tr += ' </td>';
    //
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_TXBLVAL_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TXBLVAL" style="text-align: right;" readonly="readonly" type="text" >';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class="atextBoxFor text-box single-line" data-val="true" data-val-number="The field GSTPER must be a number." id="B_GSTPER_' + rowindex + '" maxlength="5" name="TsalePos_TBATCHDTL[' + rowindex + '].GSTPER" onkeypress="return numericOnly(this,2);" readonly="readonly" style="text-align: right;" type="text" value="' + GSTPER + '">';
    tr += '     <input id="B_PRODGRPGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].PRODGRPGSTPER" type="hidden" value="' + PRODGRPGSTPER + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class="atextBoxFor text-box single-line" data-val="true" data-val-number="The field GSTAMT must be a number." id="B_GSTAMT_' + rowindex + '" maxlength="5" name="TsalePos_TBATCHDTL[' + rowindex + '].GSTAMT" onkeypress="return numericOnly(this,2);" readonly="readonly" style="text-align: right;" type="text" value="">';

    tr += ' </td>';
    //tr += ' <td class="" title="">';
    //tr += '     <input class="atextBoxFor textbox_image text-box single-line" id="B_ORDDOCNO_' + rowindex + '" maxlength="6" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDDOCNO" onblur="GetHelpBlur("/T_SALE_POS/GetOrderDetails","Order Details","B_ORDAUTONO_' + rowindex + '","B_ORDDOCNO_0=PYMTCD=1/B_ORDDOCDT_0=PYMTNM=0","B_ORDDOCNO_' + rowindex + '","B_ITCD_' + rowindex + '"/"B_ORDSLNO_' + rowindex + '"/RETDEBSLCD);" onkeydown="GetHelpBlur("/T_SALE_POS/GetOrderDetails","Order Details","B_ORDDOCNO_' + rowindex + '","B_ORDDOCNO_0=docno=0/B_ORDDOCDT_0=docdt=2/B_ORDAUTONO_0=autono=7/B_ORDSLNO_0=slno=1","B_ITCD_' + rowindex + '"/"B_ORDSLNO_' + rowindex + '","RETDEBSLCD")" placeholder="Code" type="text" value="">';
    //tr += '     <img src="/Image/search.png" id="PARTY_HELP" width="20px" height="20px" class="Help_image_button_grid" title="Help" onmousedown="GetHelpBlur("/T_SALE_POS/GetOrderDetails","Order Details","B_ORDDOCNO_' + rowindex + '","B_ORDDOCNO_0=ORDDOCNO=0/B_ORDDOCDT_0=ORDDOCDT=1/B_ORDAUTONO_0=ORDAUTONO/B_ORDSLNO_0=ORDSLNO","B_ITCD_' + rowindex + '"/"B_ORDSLNO_' + rowindex + '"/RETDEBSLCD)">';

    //tr += '     <input data-val="true" data-val-length="The field ORDAUTONO must be a string with a maximum length of 30." data-val-length-max="30" id="B_ORDAUTONO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDAUTONO" type="hidden" value="">';
    ////tr += '     <input data-val="true" data-val-number="The field ORDSLNO must be a number." id="B_ORDSLNO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDSLNO" type="hidden" value="">';
    //tr += ' </td>';

    tr += '<td class="" title="">';
    tr += '<input class="atextBoxFor textbox_image text-box single-line" id="B_ORDDOCNO_' + rowindex + '" maxlength="15" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDDOCNO" onblur="GetHelpBlur(\'/T_SALE_POS/GetOrderDetails\',\'Order Details\',\'B_ORDDOCNO_' + rowindex + '\',\'B_ORDDOCNO_' + rowindex + '=docno=0/B_ORDDOCDT_' + rowindex + '=docdt=2/B_ORDAUTONO_' + rowindex + '=autono=7/B_ORDSLNO_' + rowindex + '=slno=1\',\'B_ITCD_' + rowindex + '/B_ORDSLNO_' + rowindex + '/B_ORDAUTONO_' + rowindex + '/RETDEBSLCD\');" onkeydown="GetHelpBlur(\'/T_SALE_POS/GetOrderDetails\',\'Order Details\',\'B_ORDDOCNO_' + rowindex + '\',\'B_ORDDOCNO_' + rowindex + '=docno=0/B_ORDDOCDT_' + rowindex + '=docdt=2/B_ORDAUTONO_' + rowindex + '=autono=7/B_ORDSLNO_' + rowindex + '=slno=1\',\'B_ITCD_' + rowindex + '/B_ORDSLNO_' + rowindex + '/B_ORDAUTONO_' + rowindex + '/RETDEBSLCD\');"  type="text" >';
    tr += '<img src="/Image/search.png" id="PARTY_HELP" width="20px" height="20px" class="Help_image_button_grid" title="Help" onmousedown="GetHelpBlur(\'/T_SALE_POS/GetOrderDetails\',\'Order Details\',\'B_ORDDOCNO_' + rowindex + '\',\'B_ORDDOCNO_' + rowindex + '=docno=0/B_ORDDOCDT_' + rowindex + '=docdt=2/B_ORDAUTONO_' + rowindex + '=autono=7/B_ORDSLNO_' + rowindex + '=slno=1\',\'B_ITCD_' + rowindex + '/B_ORDSLNO_' + rowindex + '/B_ORDAUTONO_' + rowindex + '/RETDEBSLCD\');">';
    tr += '<input data-val="true" data-val-length="The field ORDAUTONO must be a string with a maximum length of 30." data-val-length-max="30" id="B_ORDAUTONO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDAUTONO" type="hidden" value="">';
    tr += '</td>';
    ///
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor text-box single-line" id="B_ORDSLNO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDSLNO" readonly="readonly" type="text" value="">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor text-box single-line" id="B_ORDDOCDT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDDOCDT" readonly="readonly" type="text" value="">';
    tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <select class=" atextBoxFor select_3d " id="PCSACTION_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].PCSACTION"><option value=""></option>';
    tr += '         <option value="DL">Delivered</option>';
    tr += '         <option value="FP">Fall</option>';
    tr += '         <option value="PO">Polish</option>';
    tr += '         <option value="PP">Polish/Fall</option>';
    tr += '         <option value="AL">Alteration</option>';
    tr += '     </select>';
    tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field STKTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_STKTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].STKTYPE" readonly="readonly" type="text" value="' + STKTYPE + '">';
    tr += '     ';
    tr += ' </td>';
    if (MNTNPART == "Y") {
        tr += ' <td class="">';
        tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field PARTCD must be a string with a maximum length of 4." data-val-length-max="4" id="B_PARTCD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].PARTCD" readonly="readonly" type="text" value="">';
        tr += '     <input id="B_PARTNM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].PARTNM" type="hidden" value="">';
        tr += '     <input id="B_PRTBARCODE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].PRTBARCODE" type="hidden" value="">';
        tr += ' </td>';
    }
    tr += ' <td class="">';
    tr += '     <button type="button" onclick="FillImageModal(\'B_BarImages_' + rowindex + '\')" data-toggle="modal" data-target="#ViewImageModal" id="OpenImageModal_' + rowindex + '" class="btn atextBoxFor text-info" style="padding:0px">' + NoOfBarImages + '</button>';
    tr += '     <input id="B_BarImages_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BarImages" type="hidden" value="' + BarImages + '">';
    tr += ' </td>';

    tr += '    <td class="">';
    tr += ' <input class=" atextBoxFor " data-target="#ZoomTextBoxModal" data-toggle="modal" data-val="true" data-val-length="The field ITREM must be a string with a maximum length of 100." data-val-length-max="100" id="B_ITREM_' + rowindex + '" maxlength="100" name="TsalePos_TBATCHDTL[' + rowindex + '].ITREM" onclick="OpenZoomTextBoxModal(this.id)" type="text" value="">';
    tr += '    </td>';
    tr += ' <td class="sticky-cell-opposite">';
    tr += '     <input tabindex="-1" class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NETAMT must be a number." id="B_NETAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].NETAMT" readonly="readonly" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += '</tr>';
    $("#_T_SALE_POS_PRODUCT_GRID tbody").append(tr);
    if (INCLRATEASK == "Y") {
        CalculateInclusiveRate(rowindex, '_T_SALE_POS_PRODUCT_GRID')
    }
    // CalculateRowAmt('_T_SALE_POS_PRODUCT_GRID', rowindex);
    if (INCLRATEASK != "Y") {
        $("#M_STYLENO").val('');
        $("#M_STYLENO").focus();
    } else {
        $("#M_BARCODE").val('');
        $("#M_BARCODE").focus();
    }
}
function AddReturnRow(hlpstr) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    var MNTNPART = $("#MNTNPART").val();
    var MNTNCOLOR = $("#MNTNCOLOR").val();
    var MNTNSIZE = $("#MNTNSIZE").val();
    var MNTNSHADE = $("#MNTNSHADE").val();
    var INCLRATEASK = $("#INCLRATEASK").val();
    var BARNO = returncolvalue(hlpstr, "BARNO");
    var ITGRPNM = returncolvalue(hlpstr, "ITGRPNM");
    var ITGRPCD = returncolvalue(hlpstr, "ITGRPCD");
    var ITCD = returncolvalue(hlpstr, "ITCD");
    var ITSTYLE = returncolvalue(hlpstr, "ITSTYLE");
    var COLRNM = returncolvalue(hlpstr, "COLRNM");
    var SIZECD = returncolvalue(hlpstr, "SIZECD");
    var BALSTOCK = returncolvalue(hlpstr, "BALQNTY");
    var NEGSTOCK = returncolvalue(hlpstr, "NEGSTOCK");
    var HSNCODE = returncolvalue(hlpstr, "HSNCODE");
    var QNTY = returncolvalue(hlpstr, "QNTY");
    var UOM = returncolvalue(hlpstr, "UOMCD");
    var NOS = returncolvalue(hlpstr, "NOS");
    if (UOM == "PCS") {
        NOS = 1;
        QNTY = NOS;
    }

    var RATE = returncolvalue(hlpstr, "RATE");
    var STKTYPE = returncolvalue(hlpstr, "STKTYPE");
    var GLCD = returncolvalue(hlpstr, "GLCD");
    var GROSSAMT = retFloat(QNTY) * retFloat(RATE);
    var PRODGRPGSTPER = returncolvalue(hlpstr, "PRODGRPGSTPER");
    var MTRLJOBCD = returncolvalue(hlpstr, "MTRLJOBCD");
    var DISCTYPE = returncolvalue(hlpstr, "DISCTYPE");
    var DISCRATE = returncolvalue(hlpstr, "DISCRATE");
    var GSTPERstr = retGstPerstr(PRODGRPGSTPER, RATE, DISCTYPE, DISCRATE);
    var GSTPERarr = GSTPERstr.split(','); var GSTPER = 0; var IGSTPER = 0; var CGSTPER = 0; var SGSTPER = 0;
    $.each(GSTPERarr, function () { GSTPER += parseFloat(this) || 0; IGSTPER = parseFloat(GSTPERarr[0]) || 0; CGSTPER = parseFloat(GSTPERarr[1]) || 0; SGSTPER = parseFloat(GSTPERarr[2]) || 0; });
    var BarImages = returncolvalue(hlpstr, "BARIMAGE");
    var NoOfBarImages = BarImages.split(String.fromCharCode(179)).length;
    if (BarImages == '') { NoOfBarImages = ''; }
    var SLNO = 1000;
    var rowindex = $("#_T_SALE_POS_RETURN_GRID > tbody > tr").length;
    if (rowindex == 0) { SLNO = SLNO + 1; } else { SLNO = SLNO + rowindex + 1; }

    var INCLRATE = 0;
    if (INCLRATEASK == "Y") {
        INCLRATE = returncolvalue(hlpstr, "RATE");
    }

    var tr = "";
    tr += '<tr style="font-size:12px; font-weight:bold;">';
    tr += ' <td class="sticky-cell" title="true">';
    tr += '   <input tabindex="-1" data-val="true" data-val-required="The Checked field is required." id="R_Checked_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].Checked" type="checkbox" value="true"><input name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].Checked" type="hidden" value="false">';
    tr += '   <input data-val="true" data-val-number="The field TXNSLNO must be a number." data-val-required="The TXNSLNO field is required." id="R_TXNSLNO_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].TXNSLNO" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field MTRLJOBCD must be a string with a maximum length of 2." data-val-length-max="2" id="R_MTRLJOBCD_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].MTRLJOBCD" type="hidden" value="' + MTRLJOBCD + '">';
    tr += '   <input data-val="true" data-val-number="The field FLAGMTR must be a number." id="R_FLAGMTR_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].FLAGMTR" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field BLQNTY must be a number." id="R_BLQNTY_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].BLQNTY" type="hidden" value="">';
    //tr += '   <input data-val="true" data-val-length="The field HSNCODE must be a string with a maximum length of 8." data-val-length-max="8" id="R_HSNCODE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].HSNCODE" type="hidden" value="' + HSNCODE + '">';
    tr += '   <input data-val="true" data-val-length="The field LOCABIN must be a string with a maximum length of 10." data-val-length-max="10" id="R_LOCABIN_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].LOCABIN" type="hidden" value="">';
    tr += '   <input id="R_BARGENTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].BARGENTYPE" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field GLCD must be a string with a maximum length of 8." data-val-length-max="8" id="R_GLCD_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].GLCD" type="hidden" value="' + GLCD + '">';
    tr += '   <input id="R_CLASS1CD_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].CLASS1CD" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field AMT must be a number." id="R_GROSSAMT_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].GROSSAMT" type="hidden" value="' + GROSSAMT + '">';
    tr += '   <input data-val="true" data-val-number="The field DISCAMT must be a number." id="R_DISCAMT_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].DISCAMT" type="hidden" value="">';
    //tr += '   <input data-val="true" data-val-number="The field TXBLVAL must be a number." id="B_TXBLVAL_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].TXBLVAL" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field TOTDISCAMT must be a number." id="R_TOTDISCAMT_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].TOTDISCAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field IGSTPER must be a number." id="R_IGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].IGSTPER" type="hidden" value="' + IGSTPER + '">';
    tr += '   <input data-val="true" data-val-number="The field IGSTAMT must be a number." id="R_IGSTAMT_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].IGSTAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field CGSTPER must be a number." id="R_CGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].CGSTPER" type="hidden" value="' + CGSTPER + '">';
    tr += '   <input data-val="true" data-val-number="The field CGSTAMT must be a number." id="R_CGSTAMT_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].CGSTAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field SGSTPER must be a number." id="R_SGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].SGSTPER" type="hidden" value="' + SGSTPER + '">';
    tr += '   <input data-val="true" data-val-number="The field SGSTAMT must be a number." id="R_SGSTAMT_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].SGSTAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field CESSPER must be a number." id="R_CESSPER_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].CESSPER" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field CESSAMT must be a number." id="R_CESSAMT_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].CESSAMT" type="hidden" value="">';
    tr += ' </td>';
    tr += ' <td class="sticky-cell" style="left:20px;" title="1">';
    tr += '     <input class=" atextBoxFor " data-val="true" data-val-number="The field SLNO must be a number." data-val-required="The SLNO field is required." id="R_SLNO_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].SLNO" readonly="readonly" type="text" value="' + SLNO + '">';
    tr += ' </td>';
    tr += ' <td class="sticky-cell" style="left:60px" title="">';
    tr += '     <input class=" atextBoxFor " data-val="true" data-val-length="The field AGDOCNO must be a string with a maximum length of 25." data-val-length-max="16" data-val-required="The AGDOCNO field is required." id="R_AGDOCNO_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].AGDOCNO"  type="text" value="">';
    tr += ' </td>';
    tr += ' <td class="sticky-cell" style="left:200px" title="">';
    tr += '     <input class=" atextBoxFor agdocdt text-box single-line " autocomplete="off" onblur="DocumentDateCHK(this)" data-val="true" data-val-length="The field AGDOCDT must be a string with a maximum length of 10." data-val-length-max="10" data-val-required="The AGDOCDT field is required." id="R_AGDOCDT_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].AGDOCDT"  type="text" value="">';
    tr += ' </td>';
    tr += ' <td class="sticky-cell" style="left:340px" title="">';
    tr += '     <input class=" atextBoxFor " data-val="true" data-val-length="The field BARNO must be a string with a maximum length of 25." data-val-length-max="25" data-val-required="The BARNO field is required." id="R_BARNO_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].BARNO" readonly="readonly" type="text" value="' + BARNO + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="R_ITGRPNM_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].ITGRPNM" readonly="readonly" type="text" value="' + ITGRPNM + '">';
    tr += '     <input id="R_ITGRPCD_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].ITGRPCD" type="hidden" value="">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field ITCD must be a string with a maximum length of 8." data-val-length-max="8" id="R_ITCD_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].ITCD" readonly="readonly" type="text" value="' + ITCD + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="R_ITSTYLE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].ITSTYLE" readonly="readonly" type="text" value="' + ITSTYLE + '">';
    tr += ' </td>';
    if (MNTNCOLOR == "Y") {
        tr += ' <td class="" title="">';
        tr += '     <input tabindex="-1" class=" atextBoxFor " id="R_COLRNM_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].COLRNM" readonly="readonly" type="text" value="' + COLRNM + '">';
        tr += ' </td>';
    }
    if (MNTNSIZE == "Y") {
        tr += ' <td class="" title="">';
        tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field SIZECD must be a string with a maximum length of 4." data-val-length-max="4" id="R_SIZECD_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].SIZECD" readonly="readonly" type="text" value="' + SIZECD + '">';
        tr += '     <input id="R_SIZENM_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].SIZENM" type="hidden" value="">';
        tr += '     <input id="R_SZBARCODE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].SZBARCODE" type="hidden" value="">';
        tr += ' </td>';
    }
    if (MNTNSHADE == "Y") {
        tr += ' <td class="" title="">';
        tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field SHADE must be a string with a maximum length of 15." data-val-length-max="15" id="R_SHADE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].SHADE" type="text" value="">';
        tr += ' </td>';
    }
    else {
        tr += '   <input id="R_SHADE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].SHADE" type="hidden" value="">';
    }
    //tr += ' <td class="" title="">';
    //tr += '     <input tabindex="-1" class=" atextBoxFor  text-box single-line" data-val="true" data-val-number="The field BALSTOCK must be a number." id="R_BALSTOCK_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].BALSTOCK" readonly="readonly" style="text-align: right;" type="text" value="' + BALSTOCK + '">';
    //tr += '     <input id="R_NEGSTOCK_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].NEGSTOCK" type="hidden" value="">';
    //tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NOS must be a number." id="R_NOS_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].NOS" onblur = "CalculateGridQnty(\'_T_SALE_POS_RETURN_GRID\',' + rowindex + ',this.value);", onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" value="' + NOS + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field QNTY must be a number." id="R_QNTY_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].QNTY" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" value="' + QNTY + '" onblur="CalculateGridQnty(\'_T_SALE_POS_RETURN_GRID\',' + rowindex + ',this.value);" >';
    tr += '     <input id="R_BALSTOCK_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].BALSTOCK" type="hidden" value="' + BALSTOCK + '">';
    tr += '     <input id="R_NEGSTOCK_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].NEGSTOCK" type="hidden" value="' + NEGSTOCK + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor" id="R_UOM_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].UOM" readonly="readonly" type="text" value="' + UOM + '">';
    tr += ' </td>';

    if (INCLRATEASK == "Y") {
        tr += '     <td class="" title="">';
        tr += '         <input class="atextBoxFor text-right" data-val="true" data-val-number="The field INCLRATE must be a number." id="R_INCLRATE_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].INCLRATE" onchange = "CalculateInclusiveRate(' + rowindex + ',\'_T_SALE_POS_RETURN_GRID\');CalculateRowAmt(\'_T_SALE_POS_RETURN_GRID\',' + rowindex + ');", onkeypress="return numericOnly(this,2);" style="font-weight:bold;background-color: bisque;" type="text" value="' + INCLRATE + '">';
        tr += '     </td>';
        //tr += ' <td class="" title="">';
        //tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RATE must be a number." id="R_RATE_' + rowindex + '" maxlength="14" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].RATE" readonly="readonly" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="' + RATE + '">';
        //   tr += ' </td>';
        tr += '     <input id="R_RATE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].RATE" type="hidden" value="' + RATE + '">';
    }
    else {
        tr += ' <td class="" title="">';
        tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RATE must be a number." id="R_RATE_' + rowindex + '" maxlength="14" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].RATE" onchange="CalculateRowAmt(\'_T_SALE_POS_RETURN_GRID\',' + rowindex + ');" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="' + RATE + '">';
        tr += ' </td>';
    }
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="R_HSNCODE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].HSNCODE" readonly="readonly" type="text" value="' + HSNCODE + '">';
    tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <select class="atextBoxFor" data-val="true" data-val-length="The field DISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="R_DISCTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].DISCTYPE" onchange="CalculateInclusiveRate(' + rowindex + ',\'_T_SALE_POS_RETURN_GRID\');" ><option value="P">%</option>';
    tr += '         <option value="N">Nos</option>';
    tr += '         <option value="Q">Qnty</option>';
    tr += '         <option value="F">Fixed</option>';
    tr += '     </select>';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field DISCRATE must be a number." id="R_DISCRATE_' + rowindex + '" maxlength="10" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].DISCRATE" onblur="CalculateInclusiveRate(' + rowindex + ',\'_T_SALE_POS_RETURN_GRID\');" onkeydown="CopyLastDiscData(this.value,R_DISCTYPE_' + rowindex + '.value,\'R_DISCRATE_\',\'R_DISCTYPE_\',\'R_ITCD_\',\'_T_SALE_POS_RETURN_GRID\');RemoveLastDiscData(\'R_DISCRATE_\',\'R_ITCD_\',\'_T_SALE_POS_RETURN_GRID\');"  onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += ' ';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="R_TXBLVAL_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].TXBLVAL" style="text-align: right;" readonly="readonly" type="text" >';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class="atextBoxFor text-box single-line" data-val="true" data-val-number="The field GSTPER must be a number." id="R_GSTPER_' + rowindex + '" maxlength="5" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].GSTPER" onkeypress="return numericOnly(this,2);" readonly="readonly" style="text-align: right;" type="text" value="' + GSTPER + '">';
    tr += '     <input id="R_PRODGRPGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].PRODGRPGSTPER" type="hidden" value="' + PRODGRPGSTPER + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class="atextBoxFor text-box single-line" data-val="true" data-val-number="The field GSTAMT must be a number." id="R_GSTAMT_' + rowindex + '" maxlength="5" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].GSTAMT" onkeypress="return numericOnly(this,2);" readonly="readonly" style="text-align: right;" type="text" value="">';

    tr += ' </td>';
    //tr += ' <td class="" title="">';
    //tr += '     <input class="atextBoxFor textbox_image text-box single-line" id="B_ORDDOCNO_' + rowindex + '" maxlength="6" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDDOCNO" onblur="GetHelpBlur("/T_SALE_POS/GetOrderDetails","Order Details","B_ORDAUTONO_' + rowindex + '","B_ORDDOCNO_0=PYMTCD=1/B_ORDDOCDT_0=PYMTNM=0","B_ORDDOCNO_' + rowindex + '","B_ITCD_' + rowindex + '"/"B_ORDSLNO_' + rowindex + '"/RETDEBSLCD);" onkeydown="GetHelpBlur("/T_SALE_POS/GetOrderDetails","Order Details","B_ORDDOCNO_' + rowindex + '","B_ORDDOCNO_0=docno=0/B_ORDDOCDT_0=docdt=2/B_ORDAUTONO_0=autono=7/B_ORDSLNO_0=slno=1","B_ITCD_' + rowindex + '"/"B_ORDSLNO_' + rowindex + '","RETDEBSLCD")" placeholder="Code" type="text" value="">';
    //tr += '     <img src="/Image/search.png" id="PARTY_HELP" width="20px" height="20px" class="Help_image_button_grid" title="Help" onmousedown="GetHelpBlur("/T_SALE_POS/GetOrderDetails","Order Details","B_ORDDOCNO_' + rowindex + '","B_ORDDOCNO_0=ORDDOCNO=0/B_ORDDOCDT_0=ORDDOCDT=1/B_ORDAUTONO_0=ORDAUTONO/B_ORDSLNO_0=ORDSLNO","B_ITCD_' + rowindex + '"/"B_ORDSLNO_' + rowindex + '"/RETDEBSLCD)">';

    //tr += '     <input data-val="true" data-val-length="The field ORDAUTONO must be a string with a maximum length of 30." data-val-length-max="30" id="B_ORDAUTONO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDAUTONO" type="hidden" value="">';
    ////tr += '     <input data-val="true" data-val-number="The field ORDSLNO must be a number." id="B_ORDSLNO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDSLNO" type="hidden" value="">';
    //tr += ' </td>';

    //tr += '<td class="" title="">';
    //tr += '<input class="atextBoxFor textbox_image text-box single-line" id="R_ORDDOCNO_' + rowindex + '" maxlength="15" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].ORDDOCNO" onblur="GetHelpBlur(\'/T_SALE_POS/GetOrderDetails\',\'Order Details\',\'R_ORDDOCNO_' + rowindex + '\',\'R_ORDDOCNO_' + rowindex + '=docno=0/R_ORDDOCDT_' + rowindex + '=docdt=2/R_ORDAUTONO_' + rowindex + '=autono=7/R_ORDSLNO_' + rowindex + '=slno=1\',\'R_ITCD_' + rowindex + '/R_ORDSLNO_' + rowindex + '/R_ORDAUTONO_' + rowindex + '/RETDEBSLCD\');" onkeydown="GetHelpBlur(\'/T_SALE_POS/GetOrderDetails\',\'Order Details\',\'R_ORDDOCNO_' + rowindex + '\',\'R_ORDDOCNO_' + rowindex + '=docno=0/R_ORDDOCDT_' + rowindex + '=docdt=2/R_ORDAUTONO_' + rowindex + '=autono=7/R_ORDSLNO_' + rowindex + '=slno=1\',\'R_ITCD_' + rowindex + '/R_ORDSLNO_' + rowindex + '/R_ORDAUTONO_' + rowindex + '/RETDEBSLCD\');"  type="text" >';
    //tr += '<img src="/Image/search.png" id="PARTY_HELP" width="20px" height="20px" class="Help_image_button_grid" title="Help" onmousedown="GetHelpBlur(\'/T_SALE_POS/GetOrderDetails\',\'Order Details\',\'R_ORDDOCNO_' + rowindex + '\',\'R_ORDDOCNO_' + rowindex + '=docno=0/R_ORDDOCDT_' + rowindex + '=docdt=2/R_ORDAUTONO_' + rowindex + '=autono=7/R_ORDSLNO_' + rowindex + '=slno=1\',\'R_ITCD_' + rowindex + '/R_ORDSLNO_' + rowindex + '/R_ORDAUTONO_' + rowindex + '/RETDEBSLCD\');">';
    //tr += '<input data-val="true" data-val-length="The field ORDAUTONO must be a string with a maximum length of 30." data-val-length-max="30" id="R_ORDAUTONO_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].ORDAUTONO" type="hidden" value="">';
    //tr += '</td>';
    ///
    //tr += ' <td class="" title="">';
    //tr += '     <input tabindex="-1" class=" atextBoxFor text-box single-line" id="R_ORDSLNO_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].ORDSLNO" readonly="readonly" type="text" value="">';
    //tr += ' </td>';
    //tr += ' <td class="" title="">';
    //tr += '     <input tabindex="-1" class=" atextBoxFor text-box single-line" id="R_ORDDOCDT_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].ORDDOCDT" readonly="readonly" type="text" value="">';
    //tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <select class=" atextBoxFor select_3d " id="R_PCSACTION_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].PCSACTION"><option value=""></option>';
    tr += '         <option value="DL">Delivered</option>';
    tr += '         <option value="FP">Fall</option>';
    tr += '         <option value="PO">Polish</option>';
    tr += '         <option value="PP">Polish/Fall</option>';
    tr += '         <option value="AL">Alteration</option>';
    tr += '     </select>';
    tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field STKTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="R_STKTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].STKTYPE" readonly="readonly" type="text" value="' + STKTYPE + '">';
    tr += '     ';
    tr += ' </td>';
    if (MNTNPART == "Y") {
        tr += ' <td class="">';
        tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field PARTCD must be a string with a maximum length of 4." data-val-length-max="4" id="R_PARTCD_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].PARTCD" readonly="readonly" type="text" value="">';
        tr += '     <input id="R_PARTNM_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].PARTNM" type="hidden" value="">';
        tr += '     <input id="R_PRTBARCODE_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].PRTBARCODE" type="hidden" value="">';
        tr += ' </td>';
    }
    //tr += ' <td class="">';
    //tr += '     <button type="button" onclick="FillImageModal(\'R_BarImages_' + rowindex + '\')" data-toggle="modal" data-target="#ViewImageModal" id="OpenImageModal_' + rowindex + '" class="btn atextBoxFor text-info" style="padding:0px">' + NoOfBarImages + '</button>';
    //tr += '     <input id="R_BarImages_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].BarImages" type="hidden" value="' + BarImages + '">';
    //tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NETAMT must be a number." id="R_NETAMT_' + rowindex + '" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].NETAMT" readonly="readonly" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += '    <td class="">';
    tr += ' <input class=" atextBoxFor " data-target="#ZoomTextBoxModal" data-toggle="modal" data-val="true" data-val-length="The field ITREM must be a string with a maximum length of 100." data-val-length-max="100" id="R_ITREM_' + rowindex + '" maxlength="100" name="TsalePos_TBATCHDTL_RETURN[' + rowindex + '].ITREM" onclick="OpenZoomTextBoxModal(this.id)" type="text" value="">';
    tr += '    </td>';
    tr += '</tr>';
    $("#_T_SALE_POS_RETURN_GRID tbody").append(tr);
    CalculateInclusiveRate(rowindex, '_T_SALE_POS_RETURN_GRID')
    // CalculateRowAmt('_T_SALE_POS_RETURN_GRID', rowindex);

    if (INCLRATEASK != "Y") {
        $("#R_STYLENO").val('');
        $("#R_STYLENO").focus();
    } else {
        $("#R_BARCODE").val('');
        $("#R_BARCODE").focus();
    }
    $("#R_AGDOCDT_" + rowindex).datepicker({ dateFormat: "dd/mm/yy", changeMonth: true, changeYear: true });
}
function CalculateRowAmt(GridId, i) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var INCLRATEASK = $("#INCLRATEASK").val();
    if (GridId == "_T_SALE_POS_PRODUCT_GRID") {
        var QNTY = retFloat($("#B_QNTY_" + i).val());
        var IGSTPER = $("#B_IGSTPER_" + i).val();
        if (IGSTPER != "") { IGSTPER = parseFloat(IGSTPER); } else { IGSTPER = parseFloat(0); }

        var CGSTPER = $("#B_CGSTPER_" + i).val();
        if (CGSTPER != "") { CGSTPER = parseFloat(CGSTPER); } else { CGSTPER = parseFloat(0); }

        var SGSTPER = $("#B_SGSTPER_" + i).val();
        if (SGSTPER != "") { SGSTPER = parseFloat(SGSTPER); } else { SGSTPER = parseFloat(0); }

        var CESSPER = $("#B_CESSPER_" + i).val();
        if (CESSPER != "") { CESSPER = parseFloat(CESSPER); } else { CESSPER = parseFloat(0); }
        if (QNTY != 0) {
            var BALSTOCK = retFloat($("#B_BALSTOCK_" + i).val());
            var NEGSTOCK = $("#B_NEGSTOCK_" + i).val();
            var balancestock = BALSTOCK - QNTY;
            if (balancestock < 0) {
                if (NEGSTOCK != "Y") {
                    msgInfo("Quantity should not be grater than Stock !");
                    message_value = "B_QNTY_" + i;
                    return false;
                }
            }
        }

        var B_QNTY_ = retFloat($("#B_QNTY_" + i).val());
        var B_RATE_ = retFloat($("#B_RATE_" + i).val());
        var INCLRATE_ = $("#INCLRATE_" + i).val();
        var B_RATE_ = $("#B_RATE_" + i).val();
        var B_NETAMT_ = $("#B_NETAMT_" + i).val();
        //if(B_NETAMT_!=0)
        //{
        //    if (B_NETAMT_ != INCLRATE_) {
        //        var RateAdjust = retFloat(B_RATE_) + retFloat(0.01);
        //        $("#B_RATE_" + i).val(RateAdjust.toFixed(2));
        //    }
        //}

        var B_RATE_ = retFloat($("#B_RATE_" + i).val());
        var B_GROSSAMT_ = B_QNTY_ * B_RATE_;
        $("#B_GROSSAMT_" + i).val(B_GROSSAMT_);//gross amt

        var NETDISCAMT = retFloat($("#B_NETDISCAMT_" + i).val());
        if (NETDISCAMT != 0) {
            var GSTPER = retFloat($("#B_GSTPER_" + i).val());
            var QNTY = retFloat($("#B_QNTY_" + i).val());
            var DISCAMT = retFloat(((NETDISCAMT * 100) / (GSTPER + 100)) * QNTY).toFixed(2);
            $("#B_DISCAMT_" + i).val(DISCAMT);
            $("#B_DISCRATE_" + i).val(DISCAMT);
            $("#B_DISCTYPE_" + i).val("F");
        }


        //OVERALL DISCOUNT PROPOTION TO ROW WISE
        var DISCONBILL = retFloat($("#DISCONBILL").val());
        var TOTALGROSSAMT = retFloat($("#B_T_GROSSAMT").val());
        var discamt = 0, baldiscamt = 0, lastslno = 0;;
        baldiscamt = DISCONBILL;
        var GridRowMain = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
        var totalpropotionamt = 0;
        for (var j = 0; j <= GridRowMain - 1; j++) {
            if (retFloat($("#B_SLNO_" + j).val()) != 0 && retStr($("#B_ITCD_" + j).val()) != "" && retFloat($("#B_QNTY_" + j).val()) != 0 && retStr($("#B_MTRLJOBCD_" + j).val()) != "" && retStr($("#B_STKTYPE_" + j).val()) != "") {
                lastslno = j;
                totalpropotionamt += retFloat($("#B_DISCONBILLPERROW_" + j).val());
            }
        }
        if (totalpropotionamt != DISCONBILL) {
            for (var j = 0; j <= GridRowMain - 1; j++) {
                if (retFloat($("#B_SLNO_" + j).val()) != 0 && retStr($("#B_ITCD_" + j).val()) != "" && retFloat($("#B_QNTY_" + j).val()) != 0 && retStr($("#B_MTRLJOBCD_" + j).val()) != "" && retStr($("#B_STKTYPE_" + j).val()) != "") {
                    if (j == lastslno) { discamt = retFloat(baldiscamt).toFixed(2); }
                    else
                    {
                        if (DISCONBILL == 0) { discamt = 0; }
                        else
                        {
                            if (TOTALGROSSAMT != 0) {
                                discamt = retFloat((DISCONBILL / TOTALGROSSAMT) * retFloat($("#B_GROSSAMT_" + j).val())).toFixed(2);
                            }
                        }
                    }
                    baldiscamt = retFloat(baldiscamt) - retFloat(discamt);
                    $("#B_DISCONBILLPERROW_" + j).val(discamt);
                }
            }
        }

        //END OVERALL DISCOUNT PROPOTION TO ROW WISE

        //CALCULATE DISCAMT1 WITH ROW WISE PROPOTIONATE AMT
        var DISCONBILLPERROW = retFloat($("#B_DISCONBILLPERROW_" + i).val());
        if (DISCONBILLPERROW != 0) {
            var GSTPER = retFloat($("#B_GSTPER_" + i).val());
            var QNTY = retFloat($("#B_QNTY_" + i).val());
            var DISCAMT = retFloat(((DISCONBILLPERROW * 100) / (GSTPER + 100)) * QNTY).toFixed(2);
            $("#B_SCMDISCAMT_" + i).val(DISCAMT);
            $("#B_SCMDISCRATE_" + i).val(DISCAMT);
            $("#B_SCMDISCTYPE_" + i).val("F");
        }
        //END CALCULATE DISCAMT1 WITH ROW WISE PROPOTIONATE AMT

        var discamt = CalculateDiscount("B_DISCTYPE_" + i, "B_DISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, "B_GROSSAMT_" + i, "B_DISCRATE_" + i);
        var scmdiscamt = CalculateDiscount("B_SCMDISCTYPE_" + i, "B_SCMDISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, "B_GROSSAMT_" + i, "B_SCMDISCRATE_" + i);
        var TXBLVAL_ = retFloat(B_GROSSAMT_ - discamt - scmdiscamt).toFixed(2);
        var B_PRODGRPGSTPER_ = $("#B_PRODGRPGSTPER_" + i).val();
        var B_DISCTYPE_ = $("#B_DISCTYPE_" + i).val();
        var B_DISCRATE_ = $("#B_DISCRATE_" + i).val();
        $("#B_DISCAMT_" + i).val(discamt);
        $("#B_SCMDISCAMT_" + i).val(scmdiscamt);
        $("#B_TXBLVAL_" + i).val(TXBLVAL_);
        var GSTPER = 0;
        if ($("#INCLRATEASK").val() == "Y") {
            GSTPER =  $("#B_GSTPER_" + i).val();
        }
        else {
            GSTPER = retGstPer(B_PRODGRPGSTPER_, B_RATE_, B_DISCTYPE_, B_DISCRATE_);
            $("#B_GSTPER_" + i).val(GSTPER);
        }
        //IGST,CGST,SGST,CESS AMOUNT CALCULATION

        var IGST_AMT = 0; var CGST_AMT = 0; var SGST_AMT = 0; var CESS_AMT = 0; var chkAmt = 0;
        //IGST
        if (IGSTPER == 0 || IGSTPER == "") {
            IGSTPER = 0; IGST_AMT = 0;
        }
        else {
            IGST_AMT = parseFloat((TXBLVAL_ * IGSTPER) / 100).toFixed(2);
            //chkAmt = $("#B_IGSTAMT_" + i).val();
            //if (chkAmt == "") chkAmt = 0;
            //if (Math.abs(IGST_AMT - chkAmt) <= 1) IGST_AMT = chkAmt;
        }
        $("#B_IGSTAMT_" + i).val(IGST_AMT);
        //CGST
        if (CGSTPER == "" || CGSTPER == 0) {
            CGSTPER = 0; CGST_AMT = 0;
        }
        else {
            CGST_AMT = parseFloat((TXBLVAL_ * CGSTPER) / 100).toFixed(2);
            //chkAmt = $("#B_CGSTAMT_" + i).val();
            //if (chkAmt == "") chkAmt = 0;
            //if (Math.abs(CGST_AMT - chkAmt) <= 1) CGST_AMT = chkAmt;
        }
        $("#B_CGSTAMT_" + i).val(CGST_AMT);
        //SGST
        if (SGSTPER == "" || SGSTPER == 0) {
            SGSTPER = 0; SGST_AMT = 0;
        }
        else {
            SGST_AMT = parseFloat((TXBLVAL_ * SGSTPER) / 100).toFixed(2);
            //chkAmt = $("#B_SGSTAMT_" + i).val();
            //if (chkAmt == "") chkAmt = 0;
            //if (Math.abs(SGST_AMT - chkAmt) <= 1) SGST_AMT = chkAmt;
        }
        $("#B_SGSTAMT_" + i).val(SGST_AMT);

        //CESS
        if (CESSPER == "" || CESSPER == 0) {
            CESSPER = 0; CESS_AMT = 0;
        }
        else {
            CESS_AMT = parseFloat((TXBLVAL_ * CESSPER) / 100).toFixed(2);
            //chkAmt = $("#B_CESSAMT_" + i).val();
            //if (chkAmt == "") chkAmt = 0;
            //if (Math.abs(CESS_AMT - chkAmt) <= 1) CESS_AMT = chkAmt;
        }
        $("#B_CESSAMT_" + i).val(CESS_AMT);
        var gstamt = retFloat(IGST_AMT) + retFloat(CGST_AMT) + retFloat(SGST_AMT) + retFloat(CESS_AMT);
        $("#B_GSTAMT_" + i).val(gstamt.toFixed(2));

        var rownetamt = retFloat(retFloat((TXBLVAL_ * GSTPER) / 100) + retFloat(TXBLVAL_)).toFixed(2);
        var netamt = parseFloat(parseFloat(TXBLVAL_) + parseFloat(IGST_AMT) + parseFloat(CGST_AMT) + parseFloat(SGST_AMT) + parseFloat(CESS_AMT)).toFixed(2);
        //$("#INCLRATE_" + i).val(rownetamt);
        $("#B_NETAMT_" + i).val(netamt);
        //if (B_NETAMT_ != INCLRATE_) { CheckInclusivRateNetAmt(GridId, i); }

    }
    else if (GridId == "_T_SALE_POS_AMOUNT_GRID") {
        var A_NOS = retFloat($("#B_T_NOS").val());
        var B_QNTY = retFloat($("#B_T_QNTY").val());
        var D_GROSS_AMT = retFloat($("#T_B_GROSSAMT").val());
        var E_NET_AMT = retFloat($("#B_T_NET_AMT").val());
        var RT = retFloat($("#AMTRATE_" + i).val());
        var IGST_PER = retFloat($("#AIGSTPER_" + i).val());
        var CGST_PER = retFloat($("#ACGSTPER_" + i).val());
        var SGST_PER = retFloat($("#ASGSTPER_" + i).val());
        var CESS_PER = retFloat($("#ACESSPER_" + i).val());
        var DUTY_PER = retFloat($("#ADUTYPER_" + i).val());
        var CALC_TYPE = $("#CALCTYPE_" + i).val();
        var CALC_FORMULA = retFloat($("#CALCFORMULA_" + i).val());

        var AMOUNT = 0;
        if (CALC_TYPE == "F") { AMOUNT = parseFloat(RT); }
        else if (CALC_TYPE == "P") {
            if (CALC_FORMULA == "A") { AMOUNT = parseFloat(A_NOS) * parseFloat(RT); }
            else if (CALC_FORMULA == "B") { AMOUNT = parseFloat(B_QNTY) * parseFloat(RT); }
            else if (CALC_FORMULA == "D") { AMOUNT = parseFloat(D_GROSS_AMT) * parseFloat(RT); }
            else if (CALC_FORMULA == "E") { AMOUNT = parseFloat(E_NET_AMT) * parseFloat(RT); }
        }
        $("#A_AMT_" + i).val(AMOUNT);
        var IGST_AMT = 0; var CGST_AMT = 0; var SGST_AMT = 0; var CESS_AMT = 0; var DUTY_AMT = 0;

        // IGST CALCULATION
        $("#AIGSTPER_" + i).val(IGST_PER);
        IGST_AMT = (AMOUNT * IGST_PER) / 100;
        var NEWIGST_AMT = $("#AIGSTAMT_" + i).val();
        var BAL_AMT = Math.abs(parseFloat(NEWIGST_AMT) - parseFloat(IGST_AMT));
        if (BAL_AMT <= 1) {
            $("#AIGSTAMT_" + i).val(parseFloat(NEWIGST_AMT).toFixed(2));
        }
        else {
            $("#AIGSTAMT_" + i).val(parseFloat(IGST_AMT).toFixed(2));
        }
        //END

        // CGST CALCULATION
        $("#ACGSTPER_" + i).val(CGST_PER);
        CGST_AMT = (AMOUNT * CGST_PER) / 100;

        var NEWCGST_AMT = $("#ACGSTAMT_" + i).val();
        var BAL_AMT = Math.abs(parseFloat(NEWCGST_AMT) - parseFloat(CGST_AMT));
        if (BAL_AMT <= 1) {
            $("#ACGSTAMT_" + i).val(parseFloat(NEWCGST_AMT).toFixed(2));
        }
        else {
            $("#ACGSTAMT_" + i).val(parseFloat(CGST_AMT).toFixed(2));
        }

        //END
        // SGST CALCULATION
        $("#ASGSTPER_" + i).val(SGST_PER);
        SGST_AMT = (AMOUNT * SGST_PER) / 100;

        var NEWSGST_AMT = $("#ASGSTAMT_" + i).val();
        var BAL_AMT = Math.abs(parseFloat(NEWSGST_AMT) - parseFloat(SGST_AMT));
        if (BAL_AMT <= 1) {
            $("#ASGSTAMT_" + i).val(parseFloat(NEWSGST_AMT).toFixed(2));
        }
        else {
            $("#ASGSTAMT_" + i).val(parseFloat(SGST_AMT).toFixed(2));
        }
        //END
        // CESS CALCULATION
        $("#ACESSPER_" + i).val(CESS_PER);
        CESS_AMT = (AMOUNT * CESS_PER) / 100;

        var NEWCESS_AMT = $("#ACESSAMT_" + i).val();
        var BAL_AMT = Math.abs(parseFloat(NEWCESS_AMT) - parseFloat(CESS_AMT));
        if (BAL_AMT <= 1) {
            $("#ACESSAMT_" + i).val(parseFloat(NEWCESS_AMT).toFixed(2));
        }
        else {
            $("#ACESSAMT_" + i).val(parseFloat(CESS_AMT).toFixed(2));
        }
        //END
        // DUTY CALCULATION

        $("#ADUTYPER_" + i).val(DUTY_PER);
        DUTY_AMT = (AMOUNT * DUTY_PER) / 100;

        var NEWDUTY_AMT = $("#ADUTYAMT_" + i).val();
        var BAL_AMT = Math.abs(parseFloat(NEWDUTY_AMT) - parseFloat(DUTY_AMT));
        if (BAL_AMT <= 1) {
            $("#ADUTYAMT_" + i).val(parseFloat(NEWDUTY_AMT).toFixed(2));
        }
        else {
            $("#ADUTYAMT_" + i).val(parseFloat(DUTY_AMT).toFixed(2));
        }
        //END

        var NET_AMT = AMOUNT + parseFloat($("#AIGSTAMT_" + i).val()) + parseFloat($("#ACGSTAMT_" + i).val()) +
                     parseFloat($("#ASGSTAMT_" + i).val()) + parseFloat($("#ACESSAMT_" + i).val()) + parseFloat($("#ADUTYAMT_" + i).val());
        $("#ANETAMT_" + i).val(parseFloat(NET_AMT).toFixed(2));

    }
    else if (GridId == "_T_SALE_POS_RETURN_GRID") {
        var QNTY = retFloat($("#R_QNTY_" + i).val());
        var IGSTPER = $("#R_IGSTPER_" + i).val();
        if (IGSTPER != "") { IGSTPER = parseFloat(IGSTPER); } else { IGSTPER = parseFloat(0); }

        var CGSTPER = $("#R_CGSTPER_" + i).val();
        if (CGSTPER != "") { CGSTPER = parseFloat(CGSTPER); } else { CGSTPER = parseFloat(0); }

        var SGSTPER = $("#R_SGSTPER_" + i).val();
        if (SGSTPER != "") { SGSTPER = parseFloat(SGSTPER); } else { SGSTPER = parseFloat(0); }

        var CESSPER = $("#R_CESSPER_" + i).val();
        if (CESSPER != "") { CESSPER = parseFloat(CESSPER); } else { CESSPER = parseFloat(0); }

        if (QNTY != 0) {
            var BALSTOCK = retFloat($("#R_BALSTOCK_" + i).val());
            var NEGSTOCK = $("#R_NEGSTOCK_" + i).val();
            var balancestock = BALSTOCK - QNTY;
            //if (balancestock < 0) {
            //    if (NEGSTOCK != "Y") {
            //        msgInfo("Quantity should not be grater than Stock !");
            //        message_value = "R_QNTY_" + i;
            //        return false;
            //    }
            //}
        }

        var B_QNTY_ = retFloat($("#R_QNTY_" + i).val());
        var B_RATE_ = retFloat($("#R_RATE_" + i).val());
        var B_GROSSAMT_ = B_QNTY_ * B_RATE_;
        $("#R_GROSSAMT_" + i).val(B_GROSSAMT_);//gross amt
        var discamt = CalculateDiscount("R_DISCTYPE_" + i, "R_DISCRATE_" + i, "R_NOS_" + i, "R_QNTY_" + i, "R_GROSSAMT_" + i, "R_DISCRATE_" + i);
        var TXBLVAL_ = retFloat(B_GROSSAMT_ - discamt).toFixed(2);
        var B_PRODGRPGSTPER_ = $("#R_PRODGRPGSTPER_" + i).val();
        var B_DISCTYPE_ = $("#R_DISCTYPE_" + i).val();
        var B_DISCRATE_ = $("#R_DISCRATE_" + i).val();
        var GSTPER = retGstPer(B_PRODGRPGSTPER_, B_RATE_, B_DISCTYPE_, B_DISCRATE_);
        $("#R_DISCAMT_" + i).val(discamt);
        $("#R_TXBLVAL_" + i).val(TXBLVAL_);
        var gstper = $("#R_GSTPER_" + i).val();
        if (gstper != "") { $("#R_GSTPER_" + i).val(gstper); } else { $("#R_GSTPER_" + i).val(GSTPER); }
        //  $("#R_GSTPER_" + i).val(GSTPER);
        //IGST,CGST,SGST,CESS AMOUNT CALCULATION

        var IGST_AMT = 0; var CGST_AMT = 0; var SGST_AMT = 0; var CESS_AMT = 0; var chkAmt = 0;
        //IGST
        if (IGSTPER == 0 || IGSTPER == "") {
            IGSTPER = 0; IGST_AMT = 0;
        }
        else {
            IGST_AMT = parseFloat((TXBLVAL_ * IGSTPER) / 100).toFixed(2);
            chkAmt = $("#R_IGSTAMT_" + i).val();
            if (chkAmt == "") chkAmt = 0;
            if (Math.abs(IGST_AMT - chkAmt) <= 1) IGST_AMT = chkAmt;
        }
        $("#R_IGSTAMT_" + i).val(IGST_AMT);
        //CGST
        if (CGSTPER == "" || CGSTPER == 0) {
            CGSTPER = 0; CGST_AMT = 0;
        }
        else {
            CGST_AMT = parseFloat((TXBLVAL_ * CGSTPER) / 100).toFixed(2);
            chkAmt = $("#R_CGSTAMT_" + i).val();
            if (chkAmt == "") chkAmt = 0;
            if (Math.abs(CGST_AMT - chkAmt) <= 1) CGST_AMT = chkAmt;
        }
        $("#R_CGSTAMT_" + i).val(CGST_AMT);
        //SGST
        if (SGSTPER == "" || SGSTPER == 0) {
            SGSTPER = 0; SGST_AMT = 0;
        }
        else {
            SGST_AMT = parseFloat((TXBLVAL_ * SGSTPER) / 100).toFixed(2);
            chkAmt = $("#R_SGSTAMT_" + i).val();
            if (chkAmt == "") chkAmt = 0;
            if (Math.abs(SGST_AMT - chkAmt) <= 1) SGST_AMT = chkAmt;
        }
        $("#R_SGSTAMT_" + i).val(SGST_AMT);

        //CESS
        if (CESSPER == "" || CESSPER == 0) {
            CESSPER = 0; CESS_AMT = 0;
        }
        else {
            CESS_AMT = parseFloat((TXBLVAL_ * CESSPER) / 100).toFixed(2);
            chkAmt = $("#R_CESSAMT_" + i).val();
            if (chkAmt == "") chkAmt = 0;
            if (Math.abs(CESS_AMT - chkAmt) <= 1) CESS_AMT = chkAmt;
        }
        $("#R_CESSAMT_" + i).val(CESS_AMT);
        var gstamt = retFloat(IGST_AMT) + retFloat(CGST_AMT) + retFloat(SGST_AMT) + retFloat(CESS_AMT);
        $("#R_GSTAMT_" + i).val(gstamt.toFixed(2));
        var rownetamt = retFloat(retFloat((TXBLVAL_ * GSTPER) / 100) + retFloat(TXBLVAL_)).toFixed(2);
        var netamt = parseFloat(parseFloat(TXBLVAL_) + parseFloat(IGST_AMT) + parseFloat(CGST_AMT) + parseFloat(SGST_AMT) + parseFloat(CESS_AMT)).toFixed(2);
        //$("#R_INCLRATE_" + i).val(rownetamt);
        $("#R_NETAMT_" + i).val(netamt);
        //CheckInclusivRateNetAmt(GridId, i);
    }
    CalculateTotal();
}
function CalculateTotal() {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();

    //POS MAIN GRID TOTAL
    var T_QNTY = 0, T_NOS = 0, T_NET = 0, T_TXBLVAL = 0, T_GSTAMT = 0, T_DISCAMT = 0, T_SCMDISCAMT = 0, T_GROSSAMT = 0;
    var GridRow = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var QNTY = retFloat($("#B_QNTY_" + i).val());
        var NOS = retFloat($("#B_NOS_" + i).val());
        var NEGSTOCK = $("#B_NEGSTOCK_" + i).val();
        var BALSTOCK = retFloat($("#B_BALSTOCK_" + i).val());
        var NETAMT = retFloat($("#B_NETAMT_" + i).val());
        var TXBLVAL = retFloat($("#B_TXBLVAL_" + i).val());
        var GSTAMT = retFloat($("#B_GSTAMT_" + i).val());
        T_DISCAMT += retFloat($("#B_DISCAMT_" + i).val());
        T_SCMDISCAMT += retFloat($("#B_SCMDISCAMT_" + i).val());
        T_GROSSAMT += retFloat($("#B_GROSSAMT_" + i).val());
        T_QNTY += QNTY; T_NOS += NOS; T_NET += NETAMT; T_TXBLVAL += TXBLVAL, T_GSTAMT += GSTAMT;
    }
    $("#B_T_QNTY").val(parseFloat(T_QNTY).toFixed(2));
    $("#B_T_NOS").val(parseFloat(T_NOS).toFixed(0));
    $("#B_T_DISCAMT").val(parseFloat(T_DISCAMT).toFixed(2));
    $("#B_T_SCMDISCAMT").val(parseFloat(T_SCMDISCAMT).toFixed(2));
    $("#B_T_AMT").val(parseFloat(T_TXBLVAL).toFixed(2));
    $("#B_T_GSTAMT").val(parseFloat(T_GSTAMT).toFixed(2));
    $("#B_T_NET_AMT").val(parseFloat(T_NET).toFixed(2));
    $("#B_T_GROSSAMT").val(parseFloat(T_GROSSAMT).toFixed(2));

    //POS RETURN GRID TOTAL
    var R_T_QNTY = 0, R_T_NOS = 0, R_T_NET = 0, R_T_TXBLVAL = 0, R_T_GSTAMT = 0;
    var GridRow = $("#_T_SALE_POS_RETURN_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var QNTY = retFloat($("#R_QNTY_" + i).val());
        var NOS = retFloat($("#R_NOS_" + i).val());
        var NEGSTOCK = $("#R_NEGSTOCK_" + i).val();
        var BALSTOCK = retFloat($("#R_BALSTOCK_" + i).val());
        var NETAMT = retFloat($("#R_NETAMT_" + i).val());
        var TXBLVAL = retFloat($("#R_TXBLVAL_" + i).val());
        var GSTAMT = retFloat($("#R_GSTAMT_" + i).val());
        R_T_QNTY += QNTY; R_T_NOS += NOS; R_T_NET += NETAMT; R_T_TXBLVAL += TXBLVAL; R_T_GSTAMT += GSTAMT;
    }
    $("#R_T_QNTY").val(parseFloat(R_T_QNTY).toFixed(2));
    $("#R_T_NOS").val(parseFloat(R_T_NOS).toFixed(0));
    $("#R_T_AMT").val(parseFloat(R_T_TXBLVAL).toFixed(2));
    $("#R_T_GSTAMT").val(parseFloat(R_T_GSTAMT).toFixed(2));
    $("#R_T_NET_AMT").val(parseFloat(R_T_NET).toFixed(2));

    //AMT GRID TOTAL
    var T_CURR_AMT = 0; var T_AMT = 0; var T_IGST_AMT = 0; var T_CGST_AMT = 0; var T_SGST_AMT = 0; var T_CESS_AMT = 0; var T_DUTY_AMT = 0; var T_NET_AMT = 0;
    var GridRow = $("#_T_SALE_POS_AMOUNT_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var CURR_AMT = retFloat($("#ACURR_AMT_" + i).val());
        var AMT = retFloat($("#A_AMT_" + i).val());
        var IGST_AMT = retFloat($("#AIGSTAMT_" + i).val());
        var CGST_AMT = retFloat($("#ACGSTAMT_" + i).val());
        var SGST_AMT = retFloat($("#ASGSTAMT_" + i).val());
        var CESS_AMT = retFloat($("#ACESSAMT_" + i).val());
        var DUTY_AMT = retFloat($("#ADUTYAMT_" + i).val());
        var NET_AMT = retFloat($("#ANETAMT_" + i).val());

        T_CURR_AMT += CURR_AMT; T_AMT += parseFloat(AMT); T_IGST_AMT += parseFloat(IGST_AMT); T_CGST_AMT += parseFloat(CGST_AMT); T_SGST_AMT += parseFloat(SGST_AMT);
        T_CESS_AMT += parseFloat(CESS_AMT); T_DUTY_AMT += parseFloat(DUTY_AMT); T_NET_AMT += parseFloat(NET_AMT);

    }
    $("#A_T_CURR").val(parseFloat(T_CURR_AMT).toFixed(2));
    $("#A_T_AMOUNT").val(parseFloat(T_AMT).toFixed(2));
    $("#A_T_IGST").val(parseFloat(T_IGST_AMT).toFixed(2));
    $("#A_T_CGST").val(parseFloat(T_CGST_AMT).toFixed(2));
    $("#A_T_SGST").val(parseFloat(T_SGST_AMT).toFixed(2));
    $("#A_T_CESS").val(parseFloat(T_CESS_AMT).toFixed(2));
    $("#A_T_DUTY").val(parseFloat(T_DUTY_AMT).toFixed(2));
    $("#A_T_NET").val(parseFloat(T_NET_AMT).toFixed(2));

    //PAYMENT GRID TOTAL
    var T_AMT = 0, CARDAMT = 0;
    var GridRow = $("#_T_SALE_POS_PAYMENT > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var AMT = retFloat($("#P_AMT_" + i).val());
        T_AMT += parseFloat(AMT);
        if ($("#P_PYMTTYPE_" + i).val() == "R") {
            CARDAMT += retFloat($("#P_AMT_" + i).val());
        }
    }
    $("#T_PYMT_AMT").val(T_AMT.toFixed(2));

    // BillAmountCalculate
    totalbillamt = parseFloat(T_NET);
    totalRbillamt = parseFloat(R_T_NET);
    var REVCHRG = $("#REVCHRG").val();
    var ROUND_TAG = document.getElementById("RoundOff").checked;
    if (REVCHRG == "Y") {
        totalbillamt = parseFloat(totalbillamt) - parseFloat(totaltax);
        totalRbillamt = parseFloat(totalRbillamt) - parseFloat(totaltax);
    }

    document.getElementById("RETAMT").value = parseFloat(totalRbillamt).toFixed(2);
    var RETAMT = document.getElementById("RETAMT").value;
    if (RETAMT == "") { RETAMT = parseFloat(0); } else { RETAMT = parseFloat(RETAMT) }
    var T_PYMT_AMT = document.getElementById("T_PYMT_AMT").value;
    if (T_PYMT_AMT == "") { T_PYMT_AMT = parseFloat(0); } else { T_PYMT_AMT = parseFloat(T_PYMT_AMT) }

    document.getElementById("PAYAMT").value = parseFloat(T_PYMT_AMT).toFixed(2);
    document.getElementById("RETAMT").value = parseFloat(RETAMT).toFixed(2);
    document.getElementById("OTHAMT").value = parseFloat(T_NET_AMT).toFixed(2);
    document.getElementById("MEMOAMT").value = parseFloat(totalbillamt).toFixed(2);
    //document.getElementById("BLAMT").value = retFloat(parseFloat(document.getElementById("MEMOAMT").value) - parseFloat(RETAMT) + parseFloat(T_NET_AMT)).toFixed(2);

    var blamt = 0;
    if (MENU_PARA == "SBCM") {
        blamt = retFloat(parseFloat(totalbillamt) - parseFloat(RETAMT) + parseFloat(T_NET_AMT));

    }
    else {
        blamt = retFloat((parseFloat(RETAMT) + parseFloat(T_NET_AMT)) * -1);

    }
    if (ROUND_TAG == true) {
        debugger;
        var CMROFFTYPE = $("#CMROFFTYPE").val();
        if (CMROFFTYPE != "") {
            var billamt = CharmPrice(retStr(CMROFFTYPE).substring(0, 2), retInt(blamt), retStr(CMROFFTYPE).substring(2, retStr(CMROFFTYPE).length));
            tbillamt = retFloat(billamt) - retFloat(blamt);

            document.getElementById("BLAMT").value = parseFloat(billamt).toFixed(2);
            document.getElementById("ROAMT").value = parseFloat(tbillamt).toFixed(2);
        }
        else {
            TOTAL_BILL_AMOUNT = Math.round(blamt);
            TOTAL_ROUND = TOTAL_BILL_AMOUNT - blamt;
            R_TOTAL_BILL_AMOUNT = Math.round(totalRbillamt);
            R_TOTAL_ROUND = R_TOTAL_BILL_AMOUNT - totalRbillamt;
            document.getElementById("BLAMT").value = parseFloat(TOTAL_BILL_AMOUNT).toFixed(2);
            document.getElementById("ROAMT").value = parseFloat(TOTAL_ROUND).toFixed(2);
            //document.getElementById("RETAMT").value = parseFloat(R_TOTAL_BILL_AMOUNT).toFixed(2);
        }
    }
    else {
        TOTAL_ROUND = 0;
        document.getElementById("BLAMT").value = parseFloat(blamt).toFixed(2);
        document.getElementById("ROAMT").value = parseFloat(TOTAL_ROUND).toFixed(2);
        //document.getElementById("RETAMT").value = parseFloat(totalRbillamt).toFixed(2);
    }
    debugger;
    if (DefaultAction == "A") {
        var GridRow = $("#_T_SALE_POS_PAYMENT > tbody > tr").length;
        var CASHAMT = 0;
        for (var a = 0; a <= GridRow - 1; a++) {
            if ($("#P_PYMTTYPE_" + a).val() == "C") {
                CASHAMT = retFloat($("#BLAMT").val()) - retFloat(CARDAMT);
                $("#P_AMT_" + a).val(retFloat(CASHAMT).toFixed(2));
                //$("#NETDUE").val(0.00);
            }
        }
        $("#T_PYMT_AMT").val(retFloat(retFloat(CARDAMT) + retFloat(CASHAMT)).toFixed(2));
        $("#PAYAMT").val(retFloat(retFloat(CARDAMT) + retFloat(CASHAMT)).toFixed(2));
        T_PYMT_AMT = retFloat(retFloat(CARDAMT) + retFloat(CASHAMT)).toFixed(2);
    }

    //var aa = retFloat(parseFloat($("#BLAMT").val()) - parseFloat(T_PYMT_AMT)).toFixed(2);
    document.getElementById("NETDUE").value = retFloat(parseFloat($("#BLAMT").val()) - parseFloat(T_PYMT_AMT)).toFixed(2);

    //SALESMAN GRID TOTAL
    var GridRow = $("#_T_SALE_POS_SALESMAN_GRID > tbody > tr").length;

    var T_TXBLVAL = retFloat($("#B_T_AMT").val());
    var R_T_TXBLVAL = retFloat($("#R_T_AMT").val());

    var R_T_GROSS_AMT = retFloat($("#R_T_GROSS_AMT").val());
    var BLAMT = retFloat($("#BLAMT").val());
    var Mtaxamt = T_TXBLVAL;
    //var Rtaxamt = R_T_GROSS_AMT;
    var Rtaxamt = R_T_TXBLVAL;
    var t_blamt = 0; var t_itamt = 0; var t_per = 0;
    for (var i = 0; i <= GridRow - 1; i++) {
        var PER_ = retFloat($("#S_PER_" + i).val());
        var blamt = parseFloat(BLAMT * PER_) / 100;
        $("#S_BLAMT_" + i).val(retFloat(blamt).toFixed(2));

        var itamt = parseFloat(Mtaxamt) - parseFloat(Rtaxamt);
        if ($("#S_PER_" + i).val() != "") { $("#S_ITAMT_" + i).val(retFloat(itamt).toFixed(2)); } else { $("#S_ITAMT_" + i).val(parseFloat(0).toFixed(2)); }

        t_blamt += retFloat($("#S_BLAMT_" + i).val());
        t_itamt += retFloat($("#S_ITAMT_" + i).val());
        t_per += retFloat($("#S_PER_" + i).val());
    }
    $("#S_T_PER").val(t_per.toFixed(2));
    $("#S_T_ITAMT").val(t_itamt.toFixed(2));
    $("#S_T_BLAMT").val(t_blamt.toFixed(2));


}
function CalculateInclusiveRate(i, GridId) {
    debugger; var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;

    if (GridId == "_T_SALE_POS_PRODUCT_GRID") {
        var itemrate = 0;
        if ($("#INCLRATEASK").val() == "Y") {
            var INCLRATE_ = retFloat($("#INCLRATE_" + i).val());
            var B_QNTY_ = retFloat($("#B_QNTY_" + i).val());
            var B_DISCRATE_ = retFloat($("#B_DISCRATE_" + i).val());
            var B_PRODGRPGSTPER_ = $("#B_PRODGRPGSTPER_" + i).val();

            var qntyamt = 0, rate = 0;
            if (INCLRATE_ == 0) {
                rate = retFloat($("#B_RATE_" + i).val());
            }
            else {
                rate = retFloat(INCLRATE_);
            }
            qntyamt = retFloat(retFloat(rate) * retFloat(B_QNTY_)).toFixed(2);
            var discamt1 = 0, discamt2 = 0;
            if (retFloat($("#B_NETDISCAMT_" + i).val()) == 0) {
                discamt1 = CalculateDiscountWithvalue("B_DISCTYPE_" + i, "B_DISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, retFloat(qntyamt), "B_DISCRATE_" + i);
            }
            else {
                discamt1 = retFloat($("#B_NETDISCAMT_" + i).val());
            }
            if (retFloat($("#DISCONBILL").val()) == 0) {
                discamt2 = CalculateDiscountWithvalue("B_SCMDISCTYPE_" + i, "B_SCMDISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, retFloat(qntyamt), "B_SCMDISCRATE_" + i);
            }
            else {
                //OVERALL DISCOUNT PROPOTION TO ROW WISE
                var DISCONBILL = retFloat($("#DISCONBILL").val());
                var TOTALGROSSAMT = retFloat($("#B_T_GROSSAMT").val());
                var discamt = 0, baldiscamt = 0, lastslno = 0;;
                baldiscamt = DISCONBILL;
                var GridRowMain = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
                var totalpropotionamt = 0;
                for (var j = 0; j <= GridRowMain - 1; j++) {
                    if (retFloat($("#B_SLNO_" + j).val()) != 0 && retStr($("#B_ITCD_" + j).val()) != "" && retFloat($("#B_QNTY_" + j).val()) != 0 && retStr($("#B_MTRLJOBCD_" + j).val()) != "" && retStr($("#B_STKTYPE_" + j).val()) != "") {
                        lastslno = j;
                        totalpropotionamt += retFloat($("#B_DISCONBILLPERROW_" + j).val());
                    }
                }
                for (var j = 0; j <= GridRowMain - 1; j++) {
                    if (retFloat($("#B_SLNO_" + j).val()) != 0 && retStr($("#B_ITCD_" + j).val()) != "" && retFloat($("#B_QNTY_" + j).val()) != 0 && retStr($("#B_MTRLJOBCD_" + j).val()) != "" && retStr($("#B_STKTYPE_" + j).val()) != "") {
                        if (j == lastslno) { discamt = retFloat(baldiscamt).toFixed(2); }
                        else
                        {
                            if (DISCONBILL == 0) { discamt = 0; }
                            else
                            {
                                if (TOTALGROSSAMT != 0) {
                                    discamt = retFloat((DISCONBILL / TOTALGROSSAMT) * retFloat($("#B_GROSSAMT_" + j).val())).toFixed(2);
                                }
                            }
                        }
                        baldiscamt = retFloat(baldiscamt) - retFloat(discamt);
                        if (j == i) {
                            if (totalpropotionamt != DISCONBILL) {
                                discamt2 = discamt;
                            }
                            else {
                                discamt2 = retFloat($("#B_DISCONBILLPERROW_" + j).val());
                            }
                        }
                    }
                }
                //END OVERALL DISCOUNT PROPOTION TO ROW WISE
            }
            qntyamt = retFloat(qntyamt) - retFloat(discamt1) - retFloat(discamt2);
            if (B_QNTY_ != 0) {
                qntyamt = retFloat(retFloat(qntyamt) / B_QNTY_);
            }
            //Searchstr value like listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179))
            var fromrt = 0, tort = 0, selrow = -1;
            var mgstrate = [5];
            var rtval = "0,0,0"; //igstper,cgst,sgst
            var SP = String.fromCharCode(179);
            var mrates = B_PRODGRPGSTPER_.split(SP);
            var taxpercent = 0, igstper = 0, cgstper = 0, sgstper = 0;
            for (var x = 0; x <= mrates.length - 1; x++) {
                taxpercent = 0;
                igstper = 0;
                cgstper = 0;
                sgstper = 0;
                mgstrate = mrates[x].split('~');
                if (mgstrate[0] == "") { fromrt = parseFloat(0); } else { fromrt = parseFloat(mgstrate[0]); }
                if (mgstrate[1] == "") { tort = parseFloat(0); } else { tort = parseFloat(mgstrate[1]); }
                taxpercent = parseFloat(mgstrate[2]) + parseFloat(mgstrate[3]) + parseFloat(mgstrate[4]);
                igstper = parseFloat(mgstrate[2]);
                cgstper = parseFloat(mgstrate[3]);
                sgstper = parseFloat(mgstrate[4]);
                var tmprt = tort;
                if (qntyamt <= tmprt) {
                    break;
                }
            }

            $("#B_GSTPER_" + i).val(taxpercent);
            $("#B_IGSTPER_" + i).val(igstper);
            $("#B_CGSTPER_" + i).val(cgstper);
            $("#B_SGSTPER_" + i).val(sgstper);
            var itemrate = ((INCLRATE_ * 100 / (100 + taxpercent))).toFixed(2);
            $("#B_RATE_" + i).val(itemrate);

            $("#B_GROSSAMT_" + i).val(retFloat(itemrate * B_QNTY_).toFixed(2));
        }
        CalculateRowAmt('_T_SALE_POS_PRODUCT_GRID', i);
        return itemrate;
    }
    else if (GridId == "_T_SALE_POS_RETURN_GRID") {
        var itemrate = 0;
        if ($("#INCLRATEASK").val() == "Y") {
            var INCLRATE_ = retFloat($("#R_INCLRATE_" + i).val());
            var B_QNTY_ = retFloat($("#R_QNTY_" + i).val());
            var B_DISCRATE_ = retFloat($("#R_DISCRATE_" + i).val());
            var B_PRODGRPGSTPER_ = $("#R_PRODGRPGSTPER_" + i).val();

            var qntyamt = 0, rate = 0;
            if (INCLRATE_ == 0) {
                rate = retFloat($("#R_RATE_" + i).val());
            }
            else {
                rate = retFloat(INCLRATE_);
            }
            qntyamt = retFloat(retFloat(rate) * retFloat(B_QNTY_)).toFixed(2);
            var discamt1 = CalculateDiscountWithvalue("R_DISCTYPE_" + i, "R_DISCRATE_" + i, "R_NOS_" + i, "R_QNTY_" + i, retFloat(qntyamt), "R_DISCRATE_" + i);
            var discamt2 = CalculateDiscountWithvalue("R_SCMDISCTYPE_" + i, "R_SCMDISCRATE_" + i, "R_NOS_" + i, "R_QNTY_" + i, retFloat(qntyamt), "R_SCMDISCRATE_" + i);
            qntyamt = retFloat(qntyamt) - retFloat(discamt1);
            if (B_QNTY_ != 0) {
                qntyamt = retFloat(retFloat(qntyamt) / B_QNTY_);
            }
            //Searchstr value like listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179))
            var fromrt = 0, tort = 0, selrow = -1;
            var mgstrate = [5];
            var rtval = "0,0,0"; //igstper,cgst,sgst
            var SP = String.fromCharCode(179);
            var mrates = B_PRODGRPGSTPER_.split(SP);
            var taxpercent = 0, igstper = 0, cgstper = 0, sgstper = 0;
            for (var x = 0; x <= mrates.length - 1; x++) {
                taxpercent = 0;
                igstper = 0;
                cgstper = 0;
                sgstper = 0;
                mgstrate = mrates[x].split('~');
                if (mgstrate[0] == "") { fromrt = parseFloat(0); } else { fromrt = parseFloat(mgstrate[0]); }
                if (mgstrate[1] == "") { tort = parseFloat(0); } else { tort = parseFloat(mgstrate[1]); }
                taxpercent = parseFloat(mgstrate[2]) + parseFloat(mgstrate[3]) + parseFloat(mgstrate[4]);
                igstper = parseFloat(mgstrate[2]);
                cgstper = parseFloat(mgstrate[3]);
                sgstper = parseFloat(mgstrate[4]);
                var tmprt = tort;
                if (qntyamt <= tmprt) {
                    break;
                }
            }

            $("#R_GSTPER_" + i).val(taxpercent);
            $("#R_IGSTPER_" + i).val(igstper);
            $("#R_CGSTPER_" + i).val(cgstper);
            $("#R_SGSTPER_" + i).val(sgstper);
            var itemrate = ((INCLRATE_ * 100 / (100 + taxpercent))).toFixed(2);
            $("#R_RATE_" + i).val(itemrate);

            $("#R_GROSSAMT_" + i).val(retFloat(itemrate * B_QNTY_).toFixed(2));
        }
        CalculateRowAmt('_T_SALE_POS_RETURN_GRID', i);
        return itemrate;
    }
}
//function CalculateInclusiveRate(i, GridId) {
//    debugger; var DefaultAction = $("#DefaultAction").val();
//    if (DefaultAction == "V") return true;
//    if (GridId == "_T_SALE_POS_PRODUCT_GRID") {
//        var itemrate = 0;
//        var INCLRATE_ = retFloat($("#INCLRATE_" + i).val());
//        var B_QNTY_ = retFloat($("#B_QNTY_" + i).val());
//        var B_DISCRATE_ = retFloat($("#B_DISCRATE_" + i).val());
//        var B_PRODGRPGSTPER_ = $("#B_PRODGRPGSTPER_" + i).val();
//        //Searchstr value like listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179))
//        var fromrt = 0, tort = 0, selrow = -1;
//        var mgstrate = [5];
//        var rtval = "0,0,0"; //igstper,cgst,sgst
//        var SP = String.fromCharCode(179);
//        var mrates = B_PRODGRPGSTPER_.split(SP);
//        for (var x = 0; x <= mrates.length - 1; x++) {
//            //mgstrate = mrates[x].Split(Convert.ToChar(Cn.GCS())).ToArray();
//            mgstrate = mrates[x].split('~');
//            if (mgstrate[0] == "") { fromrt = parseFloat(0); } else { fromrt = parseFloat(mgstrate[0]); }
//            if (mgstrate[1] == "") { tort = parseFloat(0); } else { tort = parseFloat(mgstrate[1]); }
//            var taxpercent = parseFloat(mgstrate[2]) + parseFloat(mgstrate[3]) + parseFloat(mgstrate[4]); var tmprt = tort;
//            if (B_DISCRATE_ != 0) {
//                $("#B_GROSSAMT_" + i).val(tort);
//                var discamt = CalculateDiscount("B_DISCTYPE_" + i, "B_DISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, "B_GROSSAMT_" + i, "B_DISCRATE_" + i);
//                tmprt = tort - discamt;
//            }
//            tmprt = tmprt * (taxpercent + 100) / 100;
//            if (INCLRATE_ <= tmprt) {
//                var itemrate = ((INCLRATE_ * 100 / (100 + taxpercent))).toFixed(2);
//                var discamt = CalculateDiscount("B_DISCTYPE_" + i, "B_DISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, "B_GROSSAMT_" + i, "B_DISCRATE_" + i);
//                $("#B_RATE_" + i).val(itemrate);
//                $("#B_GSTPER_" + i).val(taxpercent);
//                break;
//            }
//        }
//        CalculateRowAmt('_T_SALE_POS_PRODUCT_GRID', i);
//        return itemrate;
//    } else if (GridId == "_T_SALE_POS_RETURN_GRID") {
//        var itemrate = 0;
//        var INCLRATE_ = retFloat($("#R_INCLRATE_" + i).val());
//        var B_QNTY_ = retFloat($("#R_QNTY_" + i).val());
//        var B_DISCRATE_ = retFloat($("#R_DISCRATE_" + i).val());
//        var B_PRODGRPGSTPER_ = $("#R_PRODGRPGSTPER_" + i).val();
//        //Searchstr value like listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179))
//        var fromrt = 0, tort = 0, selrow = -1;
//        var mgstrate = [5];
//        var rtval = "0,0,0"; //igstper,cgst,sgst
//        var SP = String.fromCharCode(179);
//        var mrates = B_PRODGRPGSTPER_.split(SP);
//        for (var x = 0; x <= mrates.length - 1; x++) {
//            //mgstrate = mrates[x].Split(Convert.ToChar(Cn.GCS())).ToArray();
//            mgstrate = mrates[x].split('~');
//            if (mgstrate[0] == "") { fromrt = parseFloat(0); } else { fromrt = parseFloat(mgstrate[0]); }
//            if (mgstrate[1] == "") { tort = parseFloat(0); } else { tort = parseFloat(mgstrate[1]); }
//            var taxpercent = parseFloat(mgstrate[2]) + parseFloat(mgstrate[3]) + parseFloat(mgstrate[4]); var tmprt = tort;
//            if (B_DISCRATE_ != 0) {
//                $("#R_GROSSAMT_" + i).val(tort);
//                var discamt = CalculateDiscount("R_DISCTYPE_" + i, "R_DISCRATE_" + i, "R_NOS_" + i, "R_QNTY_" + i, "R_GROSSAMT_" + i, "R_DISCRATE_" + i);
//                tmprt = tort - discamt;
//            }
//            tmprt = tmprt * (taxpercent + 100) / 100;
//            if (INCLRATE_ <= tmprt) {
//                var itemrate = ((INCLRATE_ * 100 / (100 + taxpercent))).toFixed(2);
//                var discamt = CalculateDiscount("R_DISCTYPE_" + i, "R_DISCRATE_" + i, "R_NOS_" + i, "R_QNTY_" + i, "R_GROSSAMT_" + i, "R_DISCRATE_" + i);
//                $("#R_RATE_" + i).val(itemrate);
//                $("#R_GSTPER_" + i).val(taxpercent);
//                break;
//            }
//        }
//        CalculateRowAmt('_T_SALE_POS_RETURN_GRID', i);
//        return itemrate;
//    }

//}
function SalesmanPerChk() {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var GridRow = $("#_T_SALE_POS_SALESMAN_GRID > tbody > tr").length;
    var T_PER = 0;
    for (var j = 0; j <= GridRow - 1; j++) {
        T_PER += retFloat($("#S_PER_" + j).val());
        if (T_PER > 100) { msgWarning("Total of Percentage(%) Should be 100 !!"); $("#S_PER_" + j).val(""); $("#S_PER_" + j).focus(); return false; }
    }
    CalculateTotal();
}
function Checked_Disable() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var GridRow = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        if ($("#B_ChildData_" + i).val() == "Y") {
            document.getElementById("B_Checked_" + i).disabled = true;
        }
    }
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
function AddRowPYMT(ID) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var TAG = ""; var COUNT = document.getElementById(ID).value; if (COUNT != "") { COUNT = parseInt(COUNT); } else { COUNT = parseInt(0); } if (COUNT > 0) { TAG = "Y"; }
    $.ajax({
        type: 'POST',
        url: $("#UrlAddRowPYMT").val(),//"@Url.Action("AddRowPYMT", PageControllerName)",
        beforesend: $("#WaitingMode").show(),
        data: $('form').serialize() + "&COUNT=" + COUNT + "&TAG=" + TAG,
        success: function (result) {
            $("#WaitingMode").hide();
            $("#partialdivPayment").animate({ marginTop: '-10px' }, 50);
            $("#partialdivPayment").html(result);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
function DeleteRowPYMT() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    $.ajax({
        type: 'POST',
        url: $("#UrlDeleteRowPYMT").val(),//"@Url.Action("DeleteRowPYMT", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            $("#partialdivPayment").animate({ marginTop: '0px' }, 50);
            $("#partialdivPayment").html(result);
            CalculateTotal();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
function Addrow(ID) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var TAG = ""; var COUNT = document.getElementById(ID).value; if (COUNT != "") { COUNT = parseInt(COUNT); } else { COUNT = parseInt(0); } if (COUNT > 0) { TAG = "Y"; }
    $.ajax({
        type: 'POST',
        url: $("#UrlAddRow").val(), //"@Url.Action("AddRow", PageControllerName)",
        beforesend: $("#WaitingMode").show(),
        data: $('form').serialize() + "&COUNT=" + COUNT + "&TAG=" + TAG,
        success: function (result) {
            $("#WaitingMode").hide();
            $("#partialdivSalesman").animate({ marginTop: '-10px' }, 50);
            $("#partialdivSalesman").html(result);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
function DeleteRow() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    $.ajax({
        type: 'POST',
        url: $("#UrlDeleteRowSlsman").val(),// "@Url.Action("DeleteRow", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            $("#partialdivSalesman").animate({ marginTop: '0px' }, 50);
            $("#partialdivSalesman").html(result);
            CalculateTotal();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
function GetTTXNDTLDetails() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var FDT = $("#FDT").val();
    var FDT = $("#TDT").val();
    var R_DOCNO = $("#R_DOCNO").val();
    var R_BARNO = $("#R_BARNO").val();
    var R_DOCCD = $("#DOCCD").val();
    $.ajax({
        type: 'POST',
        url: $("#UrlTTXNDTLDetails").val(),
        beforesend: $("#WaitingMode").show(),
        data: $('form').serialize() + "&FDT=" + FDT + "&FDT=" + FDT + "&R_DOCNO=" + R_DOCNO + "&R_BARNO=" + R_BARNO + "&R_DOCCD=" + R_DOCCD,
        success: function (result) {
            $("#popup").animate({ marginTop: '-10px' }, 50);
            $("#popup").html(result);
            $("#WaitingMode").hide();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
function SelectTTXNDTLDetails() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var Count = 0;
    var GridRow = $("#_T_SALE_POS_RETURN_POPUP_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var Check = document.getElementById("P_Checked_" + i).checked;
        if (Check == true) {
            Count = Count + 1;
        }
    }
    if (Count == 0) {
        msgInfo("Please select a Bar No. !");
        return false;
    }
    $.ajax({
        type: 'post',
        beforesend: $("#WaitingMode").show(),
        url: $("#UrlSelectTTXNDTLDetails").val(),//"@Url.Action("SelectPendOrder", PageControllerName)",
        data: $('form').serialize(),
        success: function (result) {
            if (result == "0") {
                //$("#hiddenpendordJSON").val(result);
                //$("#Pending_Order").hide();
                //if (Count > 0) {
                //    $("#show_order").show();
                //}
                //msgInfo("Order Data selected ");
            }
            else {
                $("#partialdivReturn").html(result);
                var GridRow = $("#_T_SALE_POS_RETURN > tbody > tr").length;
                for (var i = 0; i <= GridRow - 1; i++) {
                    //Sale_GetGstPer(i, '#B_');
                    //RateUpdate(i);
                }
                CalculateTotal();

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
function CloseTTXNDTLDetails() {
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
function UpdateTaxPer() {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var IGST_PER = 0; var CGST_PER = 0; var SGST_PER = 0; var CESS_PER = 0; var DUTY_PER = 0;
    var GridRowMain = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
    for (i = 0; i <= GridRowMain - 1; i++) {
        var rate = retFloat($("#B_RATE_" + i).val());
        var prodgrpgstper = $("#B_PRODGRPGSTPER_" + i).val();
        var disctype = retFloat($("#B_DISCTYPE_" + i).val());
        var discrate = $("#B_DISCRATE_" + i).val();
        var allgst = retGstPerstr(prodgrpgstper, rate, disctype, discrate);
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
    var GridRowMain = $("#_T_SALE_POS_AMOUNT_GRID > tbody > tr").length;
    for (i = 0; i <= GridRowMain - 1; i++) {
        document.getElementById("AIGSTPER_" + i).value = IGST_PER;
        document.getElementById("ACGSTPER_" + i).value = CGST_PER;
        document.getElementById("ASGSTPER_" + i).value = SGST_PER;
        document.getElementById("ACESSPER_" + i).value = CESS_PER;
        document.getElementById("ADUTYPER_" + i).value = DUTY_PER;
    }

}
function UpdateBarCodeRow(hlpstr, slno) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    var MENU_PARA = $("#MENU_PARA").val();
    if (DefaultAction == "V") return true;
    var MNTNLISTPRICE = $("#MNTNLISTPRICE").val();
    var MNTNOURDESIGN = $("#MNTNOURDESIGN").val();
    var INCLRATE = 0;
    if (INCLRATEASK == "Y") {
        INCLRATE = returncolvalue(hlpstr, "RATE");
    }

    var GridRowMain = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
    for (j = 0; j <= GridRowMain - 1; j++) {
        if ($("#B_SLNO_" + j).val() == slno) {
            var qnty = $("#B_QNTY_" + j).val();
            var nos = $("#B_NOS_" + j).val();
            var newqnty = (returncolvalue(hlpstr, "QNTY"));
            var newnos = (returncolvalue(hlpstr, "NOS"));
            if (returncolvalue(hlpstr, "UOMCD") == "PCS") {
                newnos = 1;
                newqnty = newnos;
            }
            var sumqnty = retFloat(qnty) + retFloat(newqnty);
            var sumnos = retFloat(nos) + retFloat(newnos);
            $("#B_BARNO_" + j).val($("#B_BARNO_" + j).val());
            //$("#B_TXNSLNO_" + j).val(TXNSLNO);
            $("#B_ITGRPCD_" + j).val($("#B_ITGRPCD_" + j).val());
            $("#B_ITGRPNM_" + j).val($("#B_ITGRPNM_" + j).val());
            $("#B_MTRLJOBCD_" + j).val($("#B_MTRLJOBCD_" + j).val());
            $("#B_ITCD_" + j).val($("#B_ITCD_" + j).val());
            $("#B_ITSTYLE_" + j).val($("#B_ITSTYLE_" + j).val());
            $("#B_STYLENO_" + j).val($("#B_STYLENO_" + j).val());
            $("#B_STKTYPE_" + j).val($("#B_STKTYPE_" + j).val());
            $("#B_PARTCD_" + j).val($("#B_PARTCD_" + j).val());
            $("#B_PARTNM_" + j).val($("#B_PARTNM_" + j).val());
            $("#B_PRTBARCODE_" + j).val($("#B_PRTBARCODE_" + j).val());
            $("#B_COLRNM_" + j).val($("#B_COLRNM_" + j).val());
            $("#B_SIZECD_" + j).val($("#B_SIZECD_" + j).val());
            $("#B_SIZENM_" + j).val($("#B_SIZENM_" + j).val());
            $("#B_SZBARCODE_" + j).val($("#B_SZBARCODE_" + j).val());
            $("#B_BALSTOCK_" + j).val($("#B_BALSTOCK_" + j).val());
            $("#B_QNTY_" + j).val(sumqnty);
            $("#B_NEGSTOCK_" + j).val($("#B_NEGSTOCK_" + j).val());
            $("#B_UOM_" + j).val($("#B_UOM_" + j).val());
            $("#B_NOS_" + j).val(sumnos);
            $("#B_FLAGMTR_" + j).val($("#B_FLAGMTR_" + j).val());
            $("#INCLRATE_" + j).val($("#INCLRATE_" + j).val());
            $("#B_RATE_" + j).val($("#B_RATE_" + j).val());
            $("#B_GLCD_" + j).val($("#B_GLCD_" + j).val());
            var RATE = returncolvalue(hlpstr, "RATE");
            var GROSSAMT = retFloat(sumqnty) * retFloat(RATE);
            $("#B_GROSSAMT_" + j).val(GROSSAMT);
            $("#B_PRODGRPGSTPER_" + j).val($("#B_PRODGRPGSTPER_" + j).val());
            //var GSTPERstr = retGstPerstr(PRODGRPGSTPER, RATE);
            //var GSTPERarr = GSTPERstr.split(','); var GSTPER = 0; var IGSTPER = 0; var CGSTPER = 0; var SGSTPER = 0;
            //$.each(GSTPERarr, function () { GSTPER += parseFloat(this) || 0; IGSTPER = parseFloat(GSTPERarr[0]) || 0; CGSTPER = parseFloat(GSTPERarr[1]) || 0; SGSTPER = parseFloat(GSTPERarr[2]) || 0; });
            //var BarImages = returncolvalue(hlpstr, "BARIMAGE");
            //var NoOfBarImages = BarImages.split(String.fromCharCode(179)).length;
            //if (BarImages == '') { NoOfBarImages = ''; }
            $("#B_GSTPER_" + j).val($("#B_GSTPER_" + j).val());
            $("#OpenImageModal_" + j).val($("#OpenImageModal_" + j).val());
            $("#B_BarImages_" + j).val($("#B_BarImages_" + j).val());

            var DISCTYPE = $("#B_DISCTYPE_DESC_" + j).val() == "P" ? "%" : $("#DISCTYPE").val() == "N" ? "Nos" : $("#DISCTYPE").val() == "Q" ? "Qnty" : $("#DISCTYPE").val() == "A" ? "AftDsc%" : "Fixed";
            var TDDISCTYPE = $("#B_TDDISCTYPE_DESC_" + j).val() == "P" ? "%" : $("#TDDISCTYPE").val() == "N" ? "Nos" : $("#TDDISCTYPE").val() == "Q" ? "Qnty" : $("#TDDISCTYPE").val() == "A" ? "AftDsc%" : "Fixed";
            var SCMDISCTYPE = $("#B_SCMDISCTYPE_DESC_" + j).val() == "P" ? "%" : $("#SCMDISCTYPE").val() == "N" ? "Nos" : $("#SCMDISCTYPE").val() == "Q" ? "Qnty" : $("#SCMDISCTYPE").val() == "A" ? "AftDsc%" : "Fixed";
            $("#B_DISCTYPE_DESC_" + j).val(DISCTYPE);
            $("#B_TDDISCTYPE_DESC_" + j).val(TDDISCTYPE);
            $("#B_SCMDISCTYPE_DESC_" + j).val(SCMDISCTYPE);
            CalculateInclusiveRate(j, '_T_SALE_POS_PRODUCT_GRID')

            if (INCLRATEASK != "Y") {
                $("#M_STYLENO").val('');
                $("#M_STYLENO").focus();
            } else {
                $("#M_BARCODE").val('');
                $("#M_BARCODE").focus();
            }

        }

    }


}
function GetData() {
    var DOCDT = $("#DOCDT").val();
    $.ajax({
        type: 'POST',
        url: $("#urlGetData").val(),//"@Url.Action("GetTTXNDTLDetails", PageControllerName )"
        beforesend: $("#WaitingMode").show(),
        data: { EFFDT: DOCDT },
        success: function (result) {
            var MSG = result.indexOf(String.fromCharCode(181));
            if (MSG >= 0) {
                $("#RTDEBCD").val(returncolvalue(result, "RTDEBCD"));
                $("#RTDEBNM").val(returncolvalue(result, "RTDEBNM"));
                var addr = returncolvalue(result, "add1") + returncolvalue(result, "add2") + returncolvalue(result, "add3") + "/" + returncolvalue(result, "city")
                $("#ADDR").val(addr);
                $("#MOBILE").val(returncolvalue(result, "MOBILE"));
                if (returncolvalue(result, "INC_RATE") == "Y") {
                    document.getElementById("INC_RATE").checked = true;
                }
                else {
                    document.getElementById("INC_RATE").checked = false;
                }
                $("#INCLRATEASK").val(returncolvalue(result, "INC_RATE"));
                $("#RETDEBSLCD").val(returncolvalue(result, "RETDEBSLCD"));
                $("#TAXGRPCD").val(returncolvalue(result, "TAXGRPCD"));
                $("#PRCCD").val(returncolvalue(result, "PRCCD"));
                $("#PRCNM").val(returncolvalue(result, "PRCNM"));
                $("#EFFDT").val(returncolvalue(result, "EFFDT"));
                $("#WaitingMode").hide();
            }
            else {
                $("#WaitingMode").hide();
                msgInfo("" + result + " !");
                ClearAllTextBoxes("RTDEBCD,RTDEBNM,ADDR,MOBILE,INCLRATEASK,RETDEBSLCD,TAXGRPCD,PRCCD,PRCNM,EFFDT");
                document.getElementById("INC_RATE").checked = false;
                $("#EFFDT").html("");
                message_value = "DOCDT";
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
function CalculateGridQnty(tableid, index, value) {
    debugger;
    var CUTLENGTHID = "", NOSID = "", QNTYID = "", UOMID = "";
    if (tableid == "_T_SALE_POS_PRODUCT_GRID") {
        NOSID = "B_NOS_" + index;
        QNTYID = "B_QNTY_" + index;
        UOMID = "B_UOM_" + index;
    }
    else {
        NOSID = "R_NOS_" + index;
        QNTYID = "R_QNTY_" + index;
        UOMID = "R_UOM_" + index;
    }
    if ($("#" + UOMID).val() == "PCS") {
        $("#" + QNTYID).val(retFloat(value));
        $("#" + NOSID).val(retFloat(value));
    }
    CalculateRowAmt(tableid, index);
}
function CalculateDiscOnBill() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    var GridRowMain = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;

    for (var i = 0; i <= GridRowMain - 1; i++) {
        CalculateInclusiveRate(i, '_T_SALE_POS_PRODUCT_GRID')
        //CalculateRowAmt('_T_SALE_POS_PRODUCT_GRID', i);
    }
}
//function CheckInclusivRateNetAmt(GridId, i) {
//    debugger;
//    var DefaultAction = $("#DefaultAction").val();
//    if (DefaultAction == "V") return true;
//    var INCLRATEASK = $("#INCLRATEASK").val();
//    if (GridId == "_T_SALE_POS_PRODUCT_GRID") {
//        var B_NETAMT_ = $("#B_NETAMT_" + i).val();
//        if (INCLRATEASK == "Y" && B_NETAMT_!=0) {
//            var INCLRATE_ = $("#INCLRATE_" + i).val();
//            var B_RATE_ = $("#B_RATE_" + i).val();
//            var B_NETAMT_ = $("#B_NETAMT_" + i).val();
//            var diffrate = INCLRATE_ - B_NETAMT_;
//            if (B_NETAMT_ < INCLRATE_) {

//                var RateAdjust = retFloat(B_RATE_) + retFloat(diffrate);
//                $("#B_RATE_" + i).val(RateAdjust.toFixed(2));
//            }
//            else {
//                var RateAdjust = retFloat(B_RATE_) - retFloat(diffrate);
//            $("#B_RATE_" + i).val(RateAdjust.toFixed(2));
//        }

//        }
//        CalculateRowAmt(GridId, i);
//    }
//    else if (GridId == "_T_SALE_POS_RETURN_GRID") {
//        var B_NETAMT_ = $("#R_NETAMT_" + i).val();
//        if (INCLRATEASK == "Y" && B_NETAMT_ != 0) {
//            var INCLRATE_ = $("#INCLRATE_" + i).val();
//            var B_RATE_ = $("#R_RATE_" + i).val();
//            var B_NETAMT_ = $("#R_NETAMT_" + i).val();
//            var diffrate = INCLRATE_ - B_NETAMT_;
//            if (B_NETAMT_ < INCLRATE_) {
//                var RateAdjust = retFloat(B_RATE_) + retFloat(diffrate);
//                $("#R_RATE_" + i).val(RateAdjust.toFixed(2));


//            }
//        }
//        CalculateRowAmt(GridId, i);
//    }


//}





