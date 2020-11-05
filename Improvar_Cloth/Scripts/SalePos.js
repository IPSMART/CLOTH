var DefaultAction = $("#DefaultAction").val();
function GetBarnoDetails(id) {
  
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
            CalculateTotal_Barno();
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
    var PRODGRPGSTPER = returncolvalue(hlpstr, "PRODGRPGSTPER");
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
    tr += '   <input data-val="true" data-val-number="The field TXNSLNO must be a number." data-val-required="The TXNSLNO field is required." id="B_TXNSLNO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TXNSLNO" type="hidden" value="0">';
    tr += '   <input data-val="true" data-val-length="The field MTRLJOBCD must be a string with a maximum length of 2." data-val-length-max="2" id="B_MTRLJOBCD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].MTRLJOBCD" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field FLAGMTR must be a number." id="B_FLAGMTR_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].FLAGMTR" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field BLQNTY must be a number." id="B_BLQNTY_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BLQNTY" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field HSNCODE must be a string with a maximum length of 8." data-val-length-max="8" id="B_HSNCODE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].HSNCODE" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field LOCABIN must be a string with a maximum length of 10." data-val-length-max="10" id="B_LOCABIN_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].LOCABIN" type="hidden" value="">';
    tr += '   <input id="B_BARGENTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].BARGENTYPE" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-length="The field GLCD must be a string with a maximum length of 8." data-val-length-max="8" id="B_GLCD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].GLCD" type="hidden" value="">';
    tr += '   <input id="B_CLASS1CD_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].CLASS1CD" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field AMT must be a number." id="B_AMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].AMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field DISCAMT must be a number." id="B_DISCAMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCAMT" type="hidden" value="">';
    tr += '   <input data-val="true" data-val-number="The field TXBLVAL must be a number." id="B_TXBLVAL_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TXBLVAL" type="hidden" value="">';
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
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field QNTY must be a number." id="B_QNTY_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].QNTY" onblur="CalculateTotalBarno();" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor" id="B_UOM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].UOM" readonly="readonly" type="text" value="' + UOM + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NOS must be a number." id="B_NOS_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].NOS" onchange="CalculateTotalBarno();" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += '     <td class="" title="">';
    tr += '         <input class="atextBoxFor text-right" data-val="true" data-val-number="The field INCLRATE must be a number." id="INCLRATE_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].INCLRATE" onblur="CalculateRATE("0");" onkeypress="return numericOnly(this,4);" style="font-weight:bold;background-color: bisque;" type="text" value="">';
    tr += '     </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RATE must be a number." id="B_RATE_' + rowindex + '" maxlength="14" name="TsalePos_TBATCHDTL[' + rowindex + '].RATE" onchange="GetGstPer(0,\'#B_\');RateUpdate(0);" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="' + RATE + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class="atextBoxFor text-box single-line" data-val="true" data-val-number="The field GSTPER must be a number." id="B_GSTPER_' + rowindex + '" maxlength="5" name="TsalePos_TBATCHDTL[' + rowindex + '].GSTPER" onkeypress="return numericOnly(this,2);" readonly="readonly" style="text-align: right;" type="text" value="' + GSTPER + '">';
    tr += '     <input id="B_PRODGRPGSTPER_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].PRODGRPGSTPER" type="hidden" value="' + PRODGRPGSTPER + '">';
    tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <select class="atextBoxFor" data-val="true" data-val-length="The field DISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_DISCTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCTYPE"><option value="P">%</option>';
    tr += '         <option value="N">Nos</option>';
    tr += '         <option value="Q">Qnty</option>';
    tr += '         <option value="F">Fixed</option>';
    tr += '     </select>';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field DISCRATE must be a number." id="B_DISCRATE_' + rowindex + '" maxlength="10" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCRATE" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += ' ';
    tr += ' <td class="" title="">';
    tr += '     ';
    tr += '     <input class="atextBoxFor textbox_image text-box single-line" id="B_ORDDOCNO_' + rowindex + '" maxlength="6" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDDOCNO" onblur="GetHelpBlur("/T_SALE_POS/GetOrderDetails","Order Details","B_ORDDOCNO_' + rowindex + '","B_ORDDOCNO_0=PYMTCD=1/B_ORDDOCDT_0=PYMTNM=0","B_ORDDOCNO_' + rowindex + '","Y");" onkeydown="GetHelpBlur("/T_SALE_POS/GetOrderDetails","Order Details","B_ORDDOCNO_' + rowindex + '","B_ORDDOCNO_0=ORDDOCNO=0/B_ORDDOCDT_0=ORDDOCDT=1/B_ORDAUTONO_0=ORDAUTONO/B_ORDSLNO_0=ORDSLNO","B_ORDDOCNO_' + rowindex + '","Y")" placeholder="Code" type="text" value="">';
    tr += '     <img src="/Image/search.png" id="PARTY_HELP" width="20px" height="20px" class="Help_image_button_grid" title="Help" onmousedown="GetHelpBlur("/T_SALE_POS/GetOrderDetails","Order Details","B_ORDDOCNO_' + rowindex + '","B_ORDDOCNO_0=ORDDOCNO=0/B_ORDDOCDT_0=ORDDOCDT=1/B_ORDAUTONO_0=ORDAUTONO/B_ORDSLNO_0=ORDSLNO","B_ITCD_' + rowindex + '","Y")">';
    tr += ' ';
    tr += '     <input data-val="true" data-val-length="The field ORDAUTONO must be a string with a maximum length of 30." data-val-length-max="30" id="B_ORDAUTONO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDAUTONO" type="hidden" value="">';
    tr += '     <input data-val="true" data-val-number="The field ORDSLNO must be a number." id="B_ORDSLNO_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDSLNO" type="hidden" value="">';
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
    tr += '     <input tabindex="-1" class=" atextBoxFor " data-val="true" data-val-length="The field STKTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_STKTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].STKTYPE" readonly="readonly" type="text" value="">';
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
    $("#BARCODE").val('');
    $("#BARCODE").focus();
}

function CalculateBarRowAmt(i) {
    debugger;
    if (DefaultAction == "V") return true;
    var B_QNTY_ = retFloat($("#B_QNTY_"+i).val());
    var B_RATE_ = retFloat($("#B_RATE_"+i).val());
    var grsAmt=B_QNTY_*B_RATE_;    
    var B_DISCTYPE_ = $("#B_DISCTYPE_"+i).val();
    var B_DISCRATE_ = retFloat($("#B_DISCRATE_"+i).val());

    B_QNTY_0
    var R_TOTAL_BILL_AMOUNT = 0;
    var TOTAL_ROUND = 0;
    var netamt = 0;
    var ROUND_TAG = document.getElementById("RoundOff").checked;




    if (PRODUCT_TOTAL_AMT == "") { PRODUCT_TOTAL_AMT = parseFloat(0); } else { PRODUCT_TOTAL_AMT = parseFloat(PRODUCT_TOTAL_AMT); }
    var T_GROSS_AMT = document.getElementById("T_GROSS_AMT").value;
    if (T_GROSS_AMT == "") { T_GROSS_AMT = parseFloat(0); } else { T_GROSS_AMT = parseFloat(T_GROSS_AMT); }
    var A_T_AMOUNT = document.getElementById("A_T_AMOUNT").value;
    if (A_T_AMOUNT == "") { A_T_AMOUNT = parseFloat(0); } else { A_T_AMOUNT = parseFloat(A_T_AMOUNT); }
    var PRODUCT_TOTAL_IGST = document.getElementById("T_IGST_AMT").value;
    if (PRODUCT_TOTAL_IGST == "") { PRODUCT_TOTAL_IGST = parseFloat(0); } else { PRODUCT_TOTAL_IGST = parseFloat(PRODUCT_TOTAL_IGST); }
    var PRODUCT_TOTAL_CGST = document.getElementById("T_CGST_AMT").value;
    if (PRODUCT_TOTAL_CGST == "") { PRODUCT_TOTAL_CGST = parseFloat(0); } else { PRODUCT_TOTAL_CGST = parseFloat(PRODUCT_TOTAL_CGST); }
    var PRODUCT_TOTAL_SGST = document.getElementById("T_SGST_AMT").value;
    if (PRODUCT_TOTAL_SGST == "") { PRODUCT_TOTAL_SGST = parseFloat(0); } else { PRODUCT_TOTAL_SGST = parseFloat(PRODUCT_TOTAL_SGST); }

    var AMOUNT_TOTAL_AMT = 0; var AMOUNT_TOTAL_IGST = 0; var AMOUNT_TOTAL_CGST = 0; var AMOUNT_TOTAL_SGST = 0;
    var AmountGridRow = $("#AMOUNT_GRID > tbody > tr").length;
    if (AmountGridRow > 0) {
        AMOUNT_TOTAL_AMT = document.getElementById("A_T_NET").value;
        if (AMOUNT_TOTAL_AMT == "") { AMOUNT_TOTAL_AMT = parseFloat(0); } else { AMOUNT_TOTAL_AMT = parseFloat(AMOUNT_TOTAL_AMT); }
        AMOUNT_TOTAL_IGST = document.getElementById("A_T_IGST").value;
        if (AMOUNT_TOTAL_IGST == "") { AMOUNT_TOTAL_IGST = parseFloat(0); } else { AMOUNT_TOTAL_IGST = parseFloat(AMOUNT_TOTAL_IGST); }
        AMOUNT_TOTAL_CGST = document.getElementById("A_T_CGST").value;
        if (AMOUNT_TOTAL_CGST == "") { AMOUNT_TOTAL_CGST = parseFloat(0); } else { AMOUNT_TOTAL_CGST = parseFloat(AMOUNT_TOTAL_CGST); }
        AMOUNT_TOTAL_SGST = document.getElementById("A_T_SGST").value;
        if (AMOUNT_TOTAL_SGST == "") { AMOUNT_TOTAL_SGST = parseFloat(0); } else { AMOUNT_TOTAL_SGST = parseFloat(AMOUNT_TOTAL_SGST); }
    }

    document.getElementById("TOTAL_TAX").value = parseFloat(PRODUCT_TOTAL_IGST + PRODUCT_TOTAL_CGST + PRODUCT_TOTAL_SGST + AMOUNT_TOTAL_IGST + AMOUNT_TOTAL_CGST + AMOUNT_TOTAL_SGST).toFixed(2);
    document.getElementById("TOTAL_TXBL").value = parseFloat(T_GROSS_AMT + A_T_AMOUNT).toFixed(2);

    var TOTAL_BILL_AMOUNT = parseFloat(PRODUCT_TOTAL_AMT) + parseFloat(AMOUNT_TOTAL_AMT);
    var TCSPER = 0; TCSAMT = 0; TCSON = 0;
        @*if ("@Model.MENU_PARA" == "SB" || "@Model.MENU_PARA" == "SD" || "@Model.MENU_PARA" == "SBPOS" ) {*@
        if ("@Model.MENU_PARA" == "PB" || "@Model.MENU_PARA" == "PDWOQ" || "@Model.MENU_PARA" == "PCWOQ" || "@Model.MENU_PARA" == "PR" || "@Model.MENU_PARA" == "SB" || "@Model.MENU_PARA" == "SD" ||  "@Model.MENU_PARA" == "PI"  || "@Model.MENU_PARA" == "CI"  ) {
            TCSPER = parseFloat(document.getElementById("TCSPER").value).toFixed(3);
            if (TCSPER == "" || TCSPER == "NaN") { TCSPER = parseFloat(0); }
            document.getElementById("TCSPER").value = parseFloat(TCSPER).toFixed(3);
            if ("@Model.MENU_PARA" == "PB"  || "@Model.MENU_PARA" == "PI"   ) {
                TCSON=  $("#TCSON").val();
                if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON); }
            }
            else if ( "@Model.MENU_PARA" == "PDWOQ" || "@Model.MENU_PARA" == "PCWOQ" || "@Model.MENU_PARA" == "PR" ) {
                $("#TCSON").val(TCSON);
            }
            else{
                GetTCSON(TOTAL_BILL_AMOUNT);
                TCSON=$("#TCSON").val();
                if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON) }
            }
            TCSAMT = parseFloat(parseFloat(TCSON) * parseFloat(TCSPER) / 100);
            TCSAMT=  CalculateTcsAmt(TCSAMT);
            if ("@Model.MENU_PARA" == "PB" || "@Model.MENU_PARA" == "PDWOQ" || "@Model.MENU_PARA" == "PCWOQ" || "@Model.MENU_PARA" == "PR" || "@Model.MENU_PARA" == "PI")
            {
                var NEW_TCSAMT=$("#TCSAMT").val();
                if (NEW_TCSAMT == "") { NEW_TCSAMT = parseFloat(0); } else { NEW_TCSAMT = parseFloat(NEW_TCSAMT) }

                var TCSON=$("#TCSON").val();
                if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON) }

                var TCSPER=$("#TCSPER").val();
                if (TCSPER == "") { TCSPER = parseFloat(0); } else { TCSPER = parseFloat(TCSPER) }

                var CAL_TCSAMT = TCSAMT;
                var BAL_AMT = Math.abs(parseFloat(NEW_TCSAMT) - parseFloat(CAL_TCSAMT));
                if (BAL_AMT <= 1 && NEW_TCSAMT>0) {
                    $("#TCSAMT").val(parseFloat(NEW_TCSAMT).toFixed(2));
                    TCSAMT=NEW_TCSAMT;
                }
                else {
                    $("#TCSAMT").val(parseFloat(CAL_TCSAMT).toFixed(2));
                    TCSAMT=CAL_TCSAMT;
                }
            }
            else{
                document.getElementById("TCSAMT").value = parseFloat(TCSAMT).toFixed(2);

            }
        }


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