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
            //CalculateBarRowAmt();
            CalculateBarTotal();
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
    tr += '   <input data-val="true" data-val-number="The field AMT must be a number." id="B_AMT_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].AMT" type="hidden" value="">';
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
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field QNTY must be a number." id="B_QNTY_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].QNTY" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" value="" onchange="CalculateBarRowAmt(' + rowindex + ');" >';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor" id="B_UOM_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].UOM" readonly="readonly" type="text" value="' + UOM + '">';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field NOS must be a number." id="B_NOS_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].NOS" onkeypress="return numericOnly(this,3);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += '     <td class="" title="">';
    tr += '         <input class="atextBoxFor text-right" data-val="true" data-val-number="The field INCLRATE must be a number." id="INCLRATE_' + rowindex + '" maxlength="12" name="TsalePos_TBATCHDTL[' + rowindex + '].INCLRATE" onchange = "CalculateInclusiveRate(' + rowindex + ');CalculateBarRowAmt(' + rowindex + ');", onkeypress="return numericOnly(this,4);" style="font-weight:bold;background-color: bisque;" type="text" value="">';
    tr += '     </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field RATE must be a number." id="B_RATE_' + rowindex + '" maxlength="14" name="TsalePos_TBATCHDTL[' + rowindex + '].RATE" onchange="CalculateBarRowAmt(' + rowindex + ');" onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="' + RATE + '">';
    tr += ' </td>';
    tr += ' <td class="">';
    tr += '     <select class="atextBoxFor" data-val="true" data-val-length="The field DISCTYPE must be a string with a maximum length of 1." data-val-length-max="1" id="B_DISCTYPE_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCTYPE" onchange="CalculateBarRowAmt(' + rowindex + ');" ><option value="P">%</option>';
    tr += '         <option value="N">Nos</option>';
    tr += '         <option value="Q">Qnty</option>';
    tr += '         <option value="F">Fixed</option>';
    tr += '     </select>';
    tr += ' </td>';
    tr += ' <td class="" title="">';
    tr += '     <input class=" atextBoxFor text-box single-line" data-val="true" data-val-number="The field DISCRATE must be a number." id="B_DISCRATE_' + rowindex + '" maxlength="10" name="TsalePos_TBATCHDTL[' + rowindex + '].DISCRATE" onchange="CalculateBarRowAmt(' + rowindex + ');"  onkeypress="return numericOnly(this,2);" style="text-align: right;" type="text" value="">';
    tr += ' </td>';
    tr += ' ';
    tr += ' <td class="" title="">';
    tr += '     <input tabindex="-1" class=" atextBoxFor " id="B_TXBLVAL_' + rowindex + '" name="TsalePos_TBATCHDTL[' + rowindex + '].TXBLVAL" readonly="readonly" type="text" value="">';
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
    tr += '<input class="atextBoxFor textbox_image text-box single-line" id="B_ORDDOCNO_' + rowindex + '" maxlength="15" name="TsalePos_TBATCHDTL[' + rowindex + '].ORDDOCNO" onblur="GetHelpBlur(\'/T_SALE_POS/GetOrderDetails\',\'Order Details\',\'B_ORDAUTONO_' + rowindex + '\',\'B_ORDDOCNO_' + rowindex + '=docno=0/B_ORDDOCDT_' + rowindex + '=docdt=2/B_ORDAUTONO_' + rowindex + '=autono=7/B_ORDSLNO_' + rowindex +'=slno=1\',\'B_ITCD_' + rowindex +'/B_ORDSLNO_' + rowindex + '/RETDEBSLCD\');" onkeydown="GetHelpBlur(\'/T_SALE_POS/GetOrderDetails\',\'Order Details\',\'B_ORDAUTONO_' + rowindex + '\',\'B_ORDDOCNO_' + rowindex + '=docno=0/B_ORDDOCDT_' + rowindex + '=docdt=2/B_ORDAUTONO_' + rowindex + '=autono=7/B_ORDSLNO_' + rowindex +'=slno=1\',\'B_ITCD_' + rowindex +'/B_ORDSLNO_' + rowindex + '/RETDEBSLCD\');" placeholder="Ord Code" type="text" >';
    tr += '<img src="/Image/search.png" id="PARTY_HELP" width="20px" height="20px" class="Help_image_button_grid" title="Help" onmousedown="GetHelpBlur(\'/T_SALE_POS/GetOrderDetails\',\'Order Details\',\'B_ORDAUTONO_' + rowindex + '\',\'B_ORDDOCNO_' + rowindex + '=docno=0/B_ORDDOCDT_' + rowindex + '=docdt=2/B_ORDAUTONO_' + rowindex + '=autono=7/B_ORDSLNO_' + rowindex + '=slno=1\',\'B_ITCD_' + rowindex + '/B_ORDSLNO_' + rowindex + '/RETDEBSLCD\');">';
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
    $("#BARCODE").val('');
    $("#BARCODE").focus();
}

function CalculateBarRowAmt(i) {
    debugger;
    if (DefaultAction == "V") return true;
    var B_QNTY_ = retFloat($("#B_QNTY_" + i).val());
    var B_RATE_ = retFloat($("#B_RATE_" + i).val());
    var B_AMT_ = B_QNTY_ * B_RATE_;
    $("#B_AMT_" + i).val(B_AMT_);
    var discamt = CalculateDiscount("B_DISCTYPE_" + i, "B_DISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, "B_AMT_" + i, "B_DISCRATE_" + i);
    B_AMT_ = (B_AMT_ - discamt);
    var B_PRODGRPGSTPER_ = $("#B_PRODGRPGSTPER_" + i).val();
    var GSTPER = retGstPer(B_PRODGRPGSTPER_, B_RATE_);
    $("#B_GSTPER_" + i).val(GSTPER);
    var rownetamt = (((B_AMT_ * GSTPER) / 100) + B_AMT_).toFixed(3);
    $("#INCLRATE_" + i).val(rownetamt);
    $("#B_NETAMT_" + i).val(rownetamt);
    CalculateBarTotal();
}
function CalculateInclusiveRate(i) {
    debugger; var itemrate = 0;
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
            $("#B_AMT_" + i).val(tort);
            var discamt = CalculateDiscount("B_DISCTYPE_" + i, "B_DISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, "B_AMT_" + i, "B_DISCRATE_" + i);
            tmprt = tort - discamt;
        }
        tmprt = tmprt * (taxpercent + 100) / 100;
        if (INCLRATE_ <= tmprt && B_QNTY_ != 0) {
            var itemrate = ((INCLRATE_ * 100 / (100 + taxpercent)) / B_QNTY_).toFixed(2);
            var discamt = CalculateDiscount("B_DISCTYPE_" + i, "B_DISCRATE_" + i, "B_NOS_" + i, "B_QNTY_" + i, "B_AMT_" + i, "B_DISCRATE_" + i);
            $("#B_RATE_" + i).val(itemrate);
            $("#B_GSTPER_" + i).val(taxpercent);
            break;
        }
    }
    return itemrate;

}

function CalculateBarTotal() {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var MENU_PARA = $("#MENU_PARA").val();
    var T_QNTY = 0, T_NOS = 0, T_NET = 0;
    var GridRow = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        var QNTY = retFloat($("#B_QNTY_" + i).val());
        var NOS = retFloat($("#B_NOS_" + i).val());
        var NEGSTOCK = $("#B_NEGSTOCK_" + i).val();
        var BALSTOCK = retFloat($("#B_BALSTOCK_" + i).val());
        var NETAMT = retFloat($("#B_NETAMT_" + i).val());
        T_QNTY += QNTY; T_NOS += NOS; T_NET += NETAMT;
        var balancestock = BALSTOCK - QNTY;
        if (balancestock < 0) {
            if (NEGSTOCK != "Y") {
                msgInfo("Quantity should not be grater than Stock !");
                message_value = "B_QNTY_" + i;
                return false;
            }
        }
    }
    $("#B_T_QNTY").val(parseFloat(T_QNTY).toFixed(2));
    $("#B_T_NOS").val(parseFloat(T_NOS).toFixed(0));
    $("#B_T_NET_AMT").val(parseFloat(T_NET).toFixed(2));
    BillAmountCalculate();
}
function Checked_Disable() {
    var GridRow = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
    for (var i = 0; i <= GridRow - 1; i++) {
        if ($("#B_ChildData_" + i).val() == "Y") {
            document.getElementById("B_Checked_" + i).disabled = true;
        }
    }
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
function AmountCalculation(i) {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var A_NOS = document.getElementById("B_T_NOS").value;
    var B_QNTY = document.getElementById("B_T_QNTY").value;
    var D_GROSS_AMT = document.getElementById("T_GROSS_AMT").value;
    var E_NET_AMT = document.getElementById("B_T_NET_AMT").value;
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
    var GridRow = $("#_T_SALE_POS_AMOUNT_GRID > tbody > tr").length;
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
function BillAmountCalculate() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    debugger;
    var R_TOTAL_BILL_AMOUNT = 0;
    var TOTAL_ROUND = 0;
    var netamt = 0;
    var ROUND_TAG = document.getElementById("RoundOff").checked;
    var D_TOTALNOS = 0, D_TOTALQNTY = 0, D_TOTALTAXVAL = 0, A_TOTALTAXVAL = 0, D_TOTALIGST = 0, A_TOTALIGST = 0, D_TOTALCGST = 0, A_TOTALCGST = 0, D_TOTALSGST = 0, A_TOTALSGST = 0, D_TOTALNETAMT = 0, A_TOTALNETAMT = 0;
    var T_NOS = $("#B_T_NOS").val();
    if (T_NOS != "") { D_TOTALNOS = D_TOTALNOS + parseFloat(T_NOS); } else { D_TOTALNOS = D_TOTALNOS + parseFloat(0); }

    var T_QNTY = $("#B_T_QNTY").val();
    if (T_QNTY != "") { D_TOTALQNTY = D_TOTALQNTY + parseFloat(T_QNTY); } else { D_TOTALQNTY = D_TOTALQNTY + parseFloat(0); }

    var T_GROSS_AMT = $("#T_GROSS_AMT").val();
    if (T_GROSS_AMT == "") { T_GROSS_AMT = parseFloat(0); } else { T_GROSS_AMT = parseFloat(T_GROSS_AMT) }
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

    var T_NET_AMT = $("#B_T_NET_AMT").val();
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
    //if (MENU_PARA != "SR" || MENU_PARA != "PR") {
    //    TCSPER = parseFloat(document.getElementById("TCSPER").value).toFixed(3);
    //    if (TCSPER == "" || TCSPER == "NaN") { TCSPER = parseFloat(0); }
    //    document.getElementById("TCSPER").value = parseFloat(TCSPER).toFixed(3);
    //    if (MENU_PARA == "PB") {
    //        TCSON = $("#TCSON").val();
    //        if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON); }
    //    }
    //    else {
    //        GetTCSON(totalbillamt);
    //        TCSON = $("#TCSON").val();
    //        if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON) }
    //    }
    //    TCSAMT = parseFloat(parseFloat(TCSON) * parseFloat(TCSPER) / 100);
    //    TCSAMT = CalculateTcsAmt(TCSAMT);
    //    if (MENU_PARA == "PB") {
    //        var NEW_TCSAMT = $("#TCSAMT").val();
    //        if (NEW_TCSAMT == "") { NEW_TCSAMT = parseFloat(0); } else { NEW_TCSAMT = parseFloat(NEW_TCSAMT) }

    //        var TCSON = $("#TCSON").val();
    //        if (TCSON == "") { TCSON = parseFloat(0); } else { TCSON = parseFloat(TCSON) }

    //        var TCSPER = $("#TCSPER").val();
    //        if (TCSPER == "") { TCSPER = parseFloat(0); } else { TCSPER = parseFloat(TCSPER) }

    //        var CAL_TCSAMT = TCSAMT;
    //        var BAL_AMT = Math.abs(parseFloat(NEW_TCSAMT) - parseFloat(CAL_TCSAMT));
    //        if (BAL_AMT <= 1 && NEW_TCSAMT > 0) {
    //            $("#TCSAMT").val(parseFloat(NEW_TCSAMT).toFixed(2));
    //            TCSAMT = NEW_TCSAMT;
    //        }
    //        else {
    //            $("#TCSAMT").val(parseFloat(CAL_TCSAMT).toFixed(2));
    //            TCSAMT = CAL_TCSAMT;
    //        }
    //    }
    //    else {
    //        document.getElementById("TCSAMT").value = parseFloat(TCSAMT).toFixed(2);

    //    }
    //}
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
    var RETAMT = document.getElementById("RETAMT").value;
    if (RETAMT == "") { RETAMT = parseFloat(0); } else { RETAMT = parseFloat(RETAMT) }
    var T_PYMT_AMT = document.getElementById("T_PYMT_AMT").value;
    if (T_PYMT_AMT == "") { T_PYMT_AMT = parseFloat(0); } else { T_PYMT_AMT = parseFloat(T_PYMT_AMT) }
    var PAYABLE = document.getElementById("PAYABLE").value;
    if (PAYABLE == "") { PAYABLE = parseFloat(0); } else { PAYABLE = parseFloat(PAYABLE) }
    var PAYAMT = document.getElementById("PAYAMT").value;
    if (PAYAMT == "") { PAYAMT = parseFloat(0); } else { PAYAMT = parseFloat(PAYAMT) }
    document.getElementById("RETAMT").value = parseFloat(RETAMT).toFixed(2);
    document.getElementById("PAYABLE").value = parseFloat(document.getElementById("BLAMT").value) - parseFloat(RETAMT).toFixed(2);
    document.getElementById("PAYAMT").value = parseFloat(T_PYMT_AMT).toFixed(2);
    document.getElementById("NETDUE").value = parseFloat(PAYABLE) - parseFloat(PAYAMT).toFixed(2);


}
function CalculateAmt_Details(i) {
    debugger;
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var BLQNTY = $("#B_BLQNTY_" + i).val();
    if (BLQNTY != "") { BLQNTY = parseFloat(BLQNTY); } else { BLQNTY = parseFloat(0); }

    var QNTY = $("#B_QNTY_" + i).val();
    if (QNTY != "") { QNTY = parseFloat(QNTY); } else { QNTY = parseFloat(0); }

    var NOS = $("#B_NOS_" + i).val();
    if (NOS != "") { NOS = parseFloat(NOS); } else { NOS = parseFloat(0); }

    var FLAGMTR = $("#B_FLAGMTR_" + i).val();
    if (FLAGMTR != "") { FLAGMTR = parseFloat(FLAGMTR); } else { FLAGMTR = parseFloat(0); }

    var RATE = $("#B_RATE_" + i).val();
    if (RATE != "") { RATE = parseFloat(RATE); } else { RATE = parseFloat(0); }

    var IGSTPER = $("#B_IGSTPER_" + i).val();
    if (IGSTPER != "") { IGSTPER = parseFloat(IGSTPER); } else { IGSTPER = parseFloat(0); }

    var CGSTPER = $("#B_CGSTPER_" + i).val();
    if (CGSTPER != "") { CGSTPER = parseFloat(CGSTPER); } else { CGSTPER = parseFloat(0); }

    var SGSTPER = $("#B_SGSTPER_" + i).val();
    if (SGSTPER != "") { SGSTPER = parseFloat(SGSTPER); } else { SGSTPER = parseFloat(0); }

    var CESSPER = $("#B_CESSPER_" + i).val();
    if (CESSPER != "") { CESSPER = parseFloat(CESSPER); } else { CESSPER = parseFloat(0); }

    var DISCTYPE = $("#B_DISCTYPE_" + i).val();
    var DISCRATE = $("#B_DISCRATE_" + i).val();
    if (DISCRATE != "") { DISCRATE = parseFloat(DISCRATE); } else { DISCRATE = parseFloat(0); }

    var TDDISCTYPE = $("#B_TDDISCTYPE_" + i).val();
    var TDDISCRATE = $("#B_TDDISCRATE_" + i).val();
    if (TDDISCRATE != "") { TDDISCRATE = parseFloat(TDDISCRATE); } else { TDDISCRATE = parseFloat(0); }

    var SCMDISCTYPE = $("#B_SCMDISCTYPE_" + i).val();
    var SCMDISCRATE = $("#B_SCMDISCRATE_" + i).val();
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
    $("#T_AMT_" + i).val(amount);

    //DISCOUNT AMOUNT CALCULATION
    var DISCAMT = 0;
    if (DISCTYPE == "Q") { DISCAMT = DISCRATE * QNTY; }
    else if (DISCTYPE == "N") { DISCAMT = DISCRATE * NOS; }
    else if (DISCTYPE == "P") { DISCAMT = (amount * DISCRATE) / 100; }
    else if (DISCTYPE == "F") { DISCAMT = DISCRATE; }
    else { DISCAMT = 0; }
    DISCAMT = parseFloat(DISCAMT).toFixed(2);
    $("#B_DISCAMT_" + i).val(DISCAMT);

    //TDDISCOUNT AMOUNT CALCULATION
    //var TDDISCAMT = 0;
    //if (TDDISCTYPE == "Q") { TDDISCAMT = TDDISCRATE * QNTY; }
    //else if (TDDISCTYPE == "N") { TDDISCAMT = TDDISCRATE * NOS; }
    //else if (TDDISCTYPE == "P") { TDDISCAMT = (amount * TDDISCRATE) / 100; }
    //else if (TDDISCTYPE == "F") { TDDISCAMT = TDDISCRATE; }
    //else { TDDISCAMT = 0; }
    //TDDISCAMT = parseFloat(TDDISCAMT).toFixed(2);
    //$("#B_TDDISCAMT_" + i).val(TDDISCAMT);

    //SCMDISCOUNT AMOUNT CALCULATION
    //var SCMDISCAMT = 0;
    //if (SCMDISCTYPE == "Q") { SCMDISCAMT = SCMDISCRATE * QNTY; }
    //else if (SCMDISCTYPE == "N") { SCMDISCAMT = SCMDISCRATE * NOS; }
    //else if (SCMDISCTYPE == "P") { SCMDISCAMT = (amount * SCMDISCRATE) / 100; }
    //else if (SCMDISCTYPE == "F") { SCMDISCAMT = SCMDISCRATE; }
    //else { SCMDISCAMT = 0; }
    //SCMDISCAMT = parseFloat(SCMDISCAMT).toFixed(2);
    //$("#B_SCMDISCAMT_" + i).val(SCMDISCAMT);

    //TOTAL DISCOUNT AMOUNT CALCULATION
    var TOTDISCAMT = parseFloat(DISCAMT).toFixed(2);
    $("#B_TOTDISCAMT_" + i).val(TOTDISCAMT);

    //TAXABLE CALCULATION
    var taxbleamt = parseFloat(amount) - parseFloat(TOTDISCAMT);
    taxbleamt = parseFloat(taxbleamt).toFixed(2);
    $("#B_TXBLVAL_" + i).val(taxbleamt);
    //IGST,CGST,SGST,CESS AMOUNT CALCULATION

    var IGST_AMT = 0; var CGST_AMT = 0; var SGST_AMT = 0; var CESS_AMT = 0; var chkAmt = 0;

    //IGST
    if (IGSTPER == 0 || IGSTPER == "") {
        IGSTPER = 0; IGST_AMT = 0;
    }
    else {
        IGST_AMT = parseFloat((taxbleamt * IGSTPER) / 100).toFixed(2);
        chkAmt = $("#B_IGSTAMT_" + i).val();
        if (chkAmt == "") chkAmt = 0;
        if (Math.abs(IGST_AMT - chkAmt) <= 1) IGST_AMT = chkAmt;
    }
    $("#B_IGSTAMT_" + i).val(IGST_AMT);
    //CGST
    if (CGSTPER == "" || CGSTPER == 0) {
        CGSTPER = 0; CGST_AMT = 0;
    }
    else {
        CGST_AMT = parseFloat((taxbleamt * CGSTPER) / 100).toFixed(2);
        chkAmt = $("#B_CGSTAMT_" + i).val();
        if (chkAmt == "") chkAmt = 0;
        if (Math.abs(CGST_AMT - chkAmt) <= 1) CGST_AMT = chkAmt;
    }
    $("#B_CGSTAMT_" + i).val(CGST_AMT);
    //SGST
    if (SGSTPER == "" || SGSTPER == 0) {
        SGSTPER = 0; SGST_AMT = 0;
    }
    else {
        SGST_AMT = parseFloat((taxbleamt * SGSTPER) / 100).toFixed(2);
        chkAmt = $("#B_SGSTAMT_" + i).val();
        if (chkAmt == "") chkAmt = 0;
        if (Math.abs(SGST_AMT - chkAmt) <= 1) SGST_AMT = chkAmt;
    }
    $("#B_SGSTAMT_" + i).val(SGST_AMT);

    //CESS
    if (CESSPER == "" || CESSPER == 0) {
        CESSPER = 0; CESS_AMT = 0;
    }
    else {
        CESS_AMT = parseFloat((taxbleamt * CESSPER) / 100).toFixed(2);
        chkAmt = $("#B_CESSAMT_" + i).val();
        if (chkAmt == "") chkAmt = 0;
        if (Math.abs(CESS_AMT - chkAmt) <= 1) CESS_AMT = chkAmt;
    }
    $("#B_CESSAMT_" + i).val(CESS_AMT);

    var netamt = parseFloat(parseFloat(taxbleamt) + parseFloat(IGST_AMT) + parseFloat(CGST_AMT) + parseFloat(SGST_AMT) + parseFloat(CESS_AMT)).toFixed(2);
    $("#B_NETAMT_" + i).val(netamt);
    CalculateTotal_Details();

}
function CalculateTotal_Details() {
    var DefaultAction = $("#DefaultAction").val();
    if (DefaultAction == "V") return true;
    var T_NOS = 0; var T_QNTY = 0; var T_AMT = 0; var T_GROSS_AMT = 0; var T_IGST_AMT = 0; var T_CGST_AMT = 0; var T_SGST_AMT = 0; var T_CESS_AMT = 0; var T_NET_AMT = 0;

    var GridRow = $("#_T_SALE_POS_PRODUCT_GRID > tbody > tr").length;
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
    $("#B_T_NOS").val(parseFloat(T_NOS).toFixed(0));
    $("#B_T_QNTY").val(parseFloat(T_QNTY).toFixed(2));
    $("#T_AMT").val(parseFloat(T_AMT).toFixed(2));
    $("#T_GROSS_AMT").val(parseFloat(T_GROSS_AMT).toFixed(2));
    $("#T_IGST_AMT").val(parseFloat(T_IGST_AMT).toFixed(2));
    $("#T_CGST_AMT").val(parseFloat(T_CGST_AMT).toFixed(2));
    $("#T_SGST_AMT").val(parseFloat(T_SGST_AMT).toFixed(2));
    $("#T_CESS_AMT").val(parseFloat(T_CESS_AMT).toFixed(2));
    $("#B_T_NET_AMT").val(parseFloat(T_NET_AMT).toFixed(2));

    //main tab
    //$("#TOTNOS").val(parseFloat(T_NOS).toFixed(2));
    //$("#TOTQNTY").val(parseFloat(T_QNTY).toFixed(2));
    //$("#TOTTAXVAL").val(parseFloat(T_GROSS_AMT).toFixed(2));
    //$("#TOTTAX").val(parseFloat(totaltax).toFixed(2));
    BillAmountCalculate();

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
