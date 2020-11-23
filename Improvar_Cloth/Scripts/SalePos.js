function GetBarnoDetails(id) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    debugger;
    if (id == "") {
        //ClearBarcodeArea();
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
                    ClearAllTextBoxes("BARCODE,MTRLJOBCD,PARTCD");
                    $('#SearchFldValue').val('BARCODE');
                    $('#helpDIV').html(result);
                    $('#ReferanceFieldID').val('BARCODE/MTRLJOBCD/PARTCD');
                    $('#ReferanceColumn').val('0/2/8');
                    $('#helpDIV_Header').html('Barno Details');
                }
                else {
                    var MSG = result.indexOf(String.fromCharCode(181));
                    if (MSG >= 0) {
                        AddBarnoRow(result);
                    }
                    else {
                        $('#helpDIV').html("");
                        msgInfo("" + result + " !");
                        ClearAllTextBoxes("BARCODE,MTRLJOBCD,PARTCD");
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
function AddBarnoRow(hlpstr) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var BARNO = returncolvalue(hlpstr, "BARNO");
    var ITGRPNM = returncolvalue(hlpstr, "ITGRPNM");
    var ITGRPCD = returncolvalue(hlpstr, "ITGRPCD");
    var ITCD = returncolvalue(hlpstr, "ITCD");
    var ITSTYLE = returncolvalue(hlpstr, "ITSTYLE");
    var COLRNM = returncolvalue(hlpstr, "COLRNM");
    var SIZECD = returncolvalue(hlpstr, "SIZECD");
    var BALSTOCK = returncolvalue(hlpstr, "BALQNTY");
    var QNTY = returncolvalue(hlpstr, "QNTY");
    var UOM = returncolvalue(hlpstr, "UOMCD");
    var NOS = returncolvalue(hlpstr, "NOS");
    var RATE = returncolvalue(hlpstr, "RATE");
    var STKTYPE = returncolvalue(hlpstr, "STKTYPE");
    var GLCD = returncolvalue(hlpstr, "GLCD");

    var PRODGRPGSTPER = returncolvalue(hlpstr, "PRODGRPGSTPER");
    var MTRLJOBCD = returncolvalue(hlpstr, "MTRLJOBCD");
    var GSTPERstr = retGstPerstr(PRODGRPGSTPER, RATE);
    var GSTPERarr = GSTPERstr.split(','); var GSTPER = 0;
    $.each(GSTPERarr, function () { GSTPER += parseFloat(this) || 0; });
    var BarImages = returncolvalue(hlpstr, "BARIMAGE");
    var NoOfBarImages = BarImages.split(String.fromCharCode(179)).length;
    if (BarImages == '') { NoOfBarImages = ''; }
    var SLNO = 1;
    var rowindex = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
    SLNO = rowindex + 1;

    var tr = "";
    tr += '<tr style="font-size:12px; font-weight:bold;">';
    tr += ' <td class="sticky-cell" title="true">';
    tr += '   <input tabindex="-1" data-val="true" data-val-required="The Checked field is required." id="B_Checked_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].Checked" type="checkbox" value="true"><input name="TsalePos_TBATCHDTL[' + rowindex + '].Checked" type="hidden" value="false">';
    tr += '   <input data-val="true" data-val-number="The field TXNSLNO must be a number." data-val-required="The TXNSLNO field is required." id="B_TXNSLNO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TXNSLNO" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field MTRLJOBCD must be a string with a maximum length of 2." data-val-length-max="2" id="B_MTRLJOBCD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].MTRLJOBCD" type="hidden" value="' + MTRLJOBCD + '">';
    tr += '   <input data-val="true" data-val-number="The field FLAGMTR must be a number." id="B_FLAGMTR_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].FLAGMTR" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field BLQNTY must be a number." id="B_BLQNTY_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BLQNTY" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field HSNCODE must be a string with a maximum length of 8." data-val-length-max="8" id="B_HSNCODE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].HSNCODE" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field LOCABIN must be a string with a maximum length of 10." data-val-length-max="10" id="B_LOCABIN_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].LOCABIN" type="hidden" value="">';
    tr += '   <input id="B_BARGENTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BARGENTYPE" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field GLCD must be a string with a maximum length of 8." data-val-length-max="8" id="B_GLCD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].GLCD" type="hidden" value="' + GLCD + '">';
    tr += '   <input id="B_CLASS1CD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CLASS1CD" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field AMT must be a number." id="B_GROSSAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].AMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field DISCAMT must be a number." id="B_DISCAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCAMT" type="hidden" value="">';
    //tr += '   <input data-val="true" data-val-number="The field TXBLVAL must be a number." id="B_TXBLVAL_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TXBLVAL" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field TOTDISCAMT must be a number." id="B_TOTDISCAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TOTDISCAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field IGSTPER must be a number." id="B_IGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].IGSTPER" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field IGSTAMT must be a number." id="B_IGSTAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].IGSTAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field CGSTPER must be a number." id="B_CGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CGSTPER" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field CGSTAMT must be a number." id="B_CGSTAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CGSTAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field CESSPER must be a number." id="B_CESSPER_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CESSPER" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field CESSAMT must be a number." id="B_CESSAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CESSAMT" type="hidden" value="">';
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
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_COLRNM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].COLRNM" readonly="readonly" type="text" value="' + COLRNM + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field SIZECD must be a string with a maximum length of 4." data-val-length-max="4" id="B_SIZECD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SIZECD" readonly="readonly" type="text" value="' + SIZECD + '">';
    tr += '     <input id="B_SIZENM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SIZENM" type="hidden" value="">';
    tr += '     <input id="B_SZBARCODE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SZBARCODE" type="hidden" value="">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field SHADE must be a string with a maximum length of 15." data-val-length-max="15" id="B_SHADE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].SHADE" type="text" value="">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor  text-box single-line" data-val="true" data-val-number="The field BALSTOCK must be a number." id="B_BALSTOCK_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BALSTOCK" readonly="readonly" style="text-align: right;" type="text" value="' + BALSTOCK + '">';
    tr += '     <input id="B_NEGSTOCK_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].NEGSTOCK" type="hidden" value="">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field QNTY must be a number." id="B_QNTY_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].QNTY" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" value="" onblur="CalculateRowAmt(\'_T_SALE_POS_PRODUCT_GRID\',' + rowindex + ');" >';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor" id="B_UOM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].UOM" readonly="readonly" type="text" value="' + UOM + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NOS must be a number." id="B_NOS_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].NOS" onchange = "CalculateRowAmt(\'_T_SALE_POS_PRODUCT_GRID\',' + rowindex + ');", onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += '     <td class="" title="">';
    tr += '         <input class="atextBoxFor text-right" data-val="true" data-val-number="The field INCLRATE must be a number." id="INCLRATE_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].INCLRATE" onchange = "CalculateInclusiveRate(' + rowindex + ');CalculateRowAmt(\'_T_SALE_POS_PRODUCT_GRID\',' + rowindex + ');", onkeypress="return numericOnly(this,4);" style="font-weight:bold;background-color: bisque;" type="text" value="">';
    tr += '     </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RATE must be a number." id="B_RATE_' + rowindex + '" maxlength="14" name="TsalePos_TBATCHDTL[' + rowindex + '].RATE" onchange="CalculateRowAmt(\'_T_SALE_POS_PRODUCT_GRID\',' + rowindex + ');" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="' + RATE + '">';
    tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <select class="atextBoxFor" data-val="true" data-val-length="The field DISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_DISCTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCTYPE" onchange="CalculateRowAmt(\'_T_SALE_POS_PRODUCT_GRID\',' + rowindex + ');" ><option value="P">%</option>';
    tr += '         <option value="N">Nos</option>';
    tr += '         <option value="Q">Qnty</option>';
    tr += '         <option value="F">Fixed</option>';
    tr += '     </select>';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field DISCRATE must be a number." id="B_DISCRATE_' + rowindex + '" maxlength="10" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCRATE" onchange="CalculateRowAmt(\'_T_SALE_POS_PRODUCT_GRID\',' + rowindex + ');"  onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += ' ';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_TXBLVAL_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TXBLVAL" style="text-align: right;" readonly="readonly" type="text" >';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class="atextBoxFor text-box single-line" data-val="true" data-val-number="The field GSTPER must be a number." id="B_GSTPER_' + rowindex + '" maxlength="5" name="TsalePos_TBATCHDTL[' + rowindex + '].GSTPER" onkeypress="return numericOnly(this,2);" readonly="readonly" style="text-align: right;" type="text" value="' + GSTPER + '">';
    tr += '     <input id="B_PRODGRPGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].PRODGRPGSTPER" type="hidden" value="' + PRODGRPGSTPER + '">';
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
    tr += ' <td class="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field PARTCD must be a string with a maximum length of 4." data-val-length-max="4" id="B_PARTCD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].PARTCD" readonly="readonly" type="text" value="">';
    tr += '     <input id="B_PARTNM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].PARTNM" type="hidden" value="">';
    tr += '     <input id="B_PRTBARCODE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].PRTBARCODE" type="hidden" value="">';
    tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <button type="button" onclick="FillImageModal(\'B_BarImages_' + rowindex + '\')" data-toggle="modal" data-target="#ViewImageModal" id="OpenImageModal_' + rowindex + '" class="btn atextBoxFor text-info" style="padding:0px">' + NoOfBarImages + '</button>';
    tr += '     <input id="B_BarImages_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BarImages" type="hidden" value="' + BarImages + '">';
    tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NETAMT must be a number." id="B_NETAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].NETAMT" readonly="readonly" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += '</tr>';
    $("#_T_SALE_POS_PRODUCT_GRID tbody").append(tr);
    CalculateRowAmt('_T_SALE_POS_PRODUCT_GRID', rowindex);
    $("#BARCODE").val('');
    $("#BARCODE").focus();
}
function CalculateRowAmt(GridId, i) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;

    if (GridId == "_T_SALE_POS_PRODUCT_GRID") {
        var QNTY = retFloat($("#B_QNTY_" + i).val());
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
        var B_GROSSAMT_ = B_QNTY_ * B_RATE_;
        $("#B_GROSSAMT_" + i).val(B_GROSSAMT_);//gross amt
        var discamt = CalculateDiscount("B_DISCTYPE_" + i, "B_DISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, "B_GROSSAMT_" + i, "B_DISCRATE_" + i);
        var TXBLVAL_ = retFloat(B_GROSSAMT_ - discamt).toFixed(2);
        var B_PRODGRPGSTPER_ = $("#B_PRODGRPGSTPER_" + i).val();
        var GSTPER = retGstPer(B_PRODGRPGSTPER_, B_RATE_);
        $("#B_TXBLVAL_" + i).val(TXBLVAL_);
        $("#B_GSTPER_" + i).val(GSTPER);
        var rownetamt = retFloat(retFloat((TXBLVAL_ * GSTPER) / 100) + retFloat(TXBLVAL_)).toFixed(2);
        $("#INCLRATE_" + i).val(rownetamt);
        $("#B_NETAMT_" + i).val(rownetamt);
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
    CalculateTotal();
}
function CalculateTotal() {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();

    //POS MAIN GRID TOTAL
    var T_QNTY = 0, T_NOS = 0, T_NET = 0, T_TXBLVAL = 0;
    var GridRow = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var QNTY = retFloat($("#B_QNTY_" + i).val());
        var NOS = retFloat($("#B_NOS_" + i).val());
        var NEGSTOCK = $("#B_NEGSTOCK_" + i).val();
        var BALSTOCK = retFloat($("#B_BALSTOCK_" + i).val());
        var NETAMT = retFloat($("#B_NETAMT_" + i).val());
        var TXBLVAL = retFloat($("#B_TXBLVAL_" + i).val());
        T_QNTY += QNTY; T_NOS += NOS; T_NET += NETAMT; T_TXBLVAL += TXBLVAL;
    }
    $("#B_T_QNTY").val(parseFloat(T_QNTY).toFixed(2));
    $("#B_T_NOS").val(parseFloat(T_NOS).toFixed(0));
    $("#B_T_AMT").val(parseFloat(T_TXBLVAL).toFixed(2));
    $("#B_T_NET_AMT").val(parseFloat(T_NET).toFixed(2));

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
    var T_AMT = 0;
    var GridRow = $("#_T_SALE_POS_PAYMENT > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var AMT = retFloat($("#P_AMT_" + i).val());
        T_AMT += parseFloat(AMT);
    }
    $("#T_PYMT_AMT").val(T_AMT.toFixed(2));

    // BillAmountCalculate
    totalbillamt = parseFloat(T_NET) + parseFloat(T_NET_AMT);
    var REVCHRG = $("#REVCHRG").val();
    var ROUND_TAG = document.getElementById("RoundOff").checked;
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
    var RETAMT = document.getElementById("RETAMT").value;
    if (RETAMT == "") { RETAMT = parseFloat(0); } else { RETAMT = parseFloat(RETAMT) }
    var T_PYMT_AMT = document.getElementById("T_PYMT_AMT").value;
    if (T_PYMT_AMT == "") { T_PYMT_AMT = parseFloat(0); } else { T_PYMT_AMT = parseFloat(T_PYMT_AMT) }

    document.getElementById("PAYAMT").value = parseFloat(T_PYMT_AMT).toFixed(2);
    document.getElementById("RETAMT").value = parseFloat(RETAMT).toFixed(2);
    document.getElementById("PAYABLE").value = retFloat(parseFloat(document.getElementById("BLAMT").value) - parseFloat(RETAMT)).toFixed(2);
    document.getElementById("NETDUE").value = retFloat(parseFloat($("#PAYABLE").val()) - parseFloat(T_PYMT_AMT)).toFixed(2);


    //SALESMAN GRID TOTAL
    var GridRow = $("#_T_SALE_POS_SALESMAN_GRID > tbody > tr").length;
   
    var T_TXBLVAL = retFloat($("#B_T_AMT").val());
    var R_T_GROSS_AMT = retFloat($("#R_T_GROSS_AMT").val());
    var PAYABLE = retFloat($("#PAYABLE").val());

    var t_blamt = 0; var t_itamt = 0; var t_per = 0;
    for (var i = 0; i <= GridRow - 1; i++) {
        var PER_ = retFloat($("#S_PER_" + i).val());
        var blamt = parseFloat(PAYABLE * PER_) / 100;
        $("#S_BLAMT_" + i).val(retFloat(blamt).toFixed(2));
        var Mtaxamt = T_TXBLVAL;
        var Rtaxamt = R_T_GROSS_AMT;
        var itamt = parseFloat(Mtaxamt) - parseFloat(Rtaxamt);
        $("#S_ITAMT_" + i).val(retFloat(itamt).toFixed(2));

        t_blamt += blamt;
        t_itamt += itamt;
        t_per += retFloat($("#S_PER_" + i).val());
    }
    $("#S_T_PER").val(t_per.toFixed(2));
    $("#S_T_ITAMT").val(t_itamt.toFixed(2));
    $("#S_T_BLAMT").val(t_blamt.toFixed(2));


}
function CalculateInclusiveRate(i) {
    debugger; var itemrate = 0;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var INCLRATE_ = retFloat($("#INCLRATE_" + i).val());
    var B_QNTY_ = retFloat($("#B_QNTY_" + i).val());
    var B_DISCRATE_ = retFloat($("#B_DISCRATE_" + i).val());
    var B_PRODGRPGSTPER_ = $("#B_PRODGRPGSTPER_" + i).val();
    //Searchstr value like listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179))
    var fromrt = 0, tort = 0, selrow = -1;
    var mgstrate = [5];
    var rtval = "0,0,0"; //igstper,cgst,sgst
    var SP = String.fromCharCode(179);
    var mrates = B_PRODGRPGSTPER_.split(SP);
    for (var x = 0; x <= mrates.length - 1; x++) {
        //mgstrate = mrates[x].Split(Convert.ToChar(Cn.GCS())).ToArray();
        mgstrate = mrates[x].split('~');
        if (mgstrate[0] == "") { fromrt = parseFloat(0); } else { fromrt = parseFloat(mgstrate[0]); }
        if (mgstrate[1] == "") { tort = parseFloat(0); } else { tort = parseFloat(mgstrate[1]); }
        var taxpercent = parseFloat(mgstrate[2]) + parseFloat(mgstrate[3]) + parseFloat(mgstrate[4]); var tmprt = tort;
        if (B_DISCRATE_ != 0) {
            $("#B_GROSSAMT_" + i).val(tort);
            var discamt = CalculateDiscount("B_DISCTYPE_" + i, "B_DISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, "B_GROSSAMT_" + i, "B_DISCRATE_" + i);
            tmprt = tort - discamt;
        }
        tmprt = tmprt * (taxpercent + 100) / 100;
        if (INCLRATE_ <= tmprt && B_QNTY_ != 0) {
            var itemrate = ((INCLRATE_ * 100 / (100 + taxpercent)) / B_QNTY_).toFixed(2);
            var discamt = CalculateDiscount("B_DISCTYPE_" + i, "B_DISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, "B_GROSSAMT_" + i, "B_DISCRATE_" + i);
            $("#B_RATE_" + i).val(itemrate);
            $("#B_GSTPER_" + i).val(taxpercent);
            break;
        }
    }
    return itemrate;

}
function SalesmanPerChk() {
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
        url:  $("#UrlAddRowPYMT").val(),//"@Url.Action("AddRowPYMT", PageControllerName)",
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
        url:$("#UrlAddRow").val(), //"@Url.Action("AddRow", PageControllerName)",
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
    },
    error: function (XMLHttpRequest, textStatus, errorThrown) {
        $("#WaitingMode").hide();
        msgError(XMLHttpRequest.responseText);
        $("body span h1").remove(); $("#msgbody_error style").remove();
    }
});
}
