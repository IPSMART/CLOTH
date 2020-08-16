var topMarginDiff = 0;
function preventEvent(e) {
    var ev = e || window.event;
    if (ev.preventDefault) ev.preventDefault();
    else ev.returnValue = false;
    if (ev.stopPropagation)
        ev.stopPropagation();
    return false;
}

function getStyle(x, styleProp) {
    if (x.currentStyle)
        var y = x.currentStyle[styleProp];
    else if (window.getComputedStyle)
        var y = document.defaultView.getComputedStyle(x, null).getPropertyValue(styleProp);
    return y;
}

function getWidth(x) {
    if (x.currentStyle)
        // in IE
        var y = x.clientWidth - parseInt(x.currentStyle["paddingLeft"]) - parseInt(x.currentStyle["paddingRight"]);
        // for IE5: var y = x.offsetWidth;
    else if (window.getComputedStyle)
        // in Gecko
        var y = document.defaultView.getComputedStyle(x, null).getPropertyValue("width");
    return y || 0;
}
function getHeight(x) {
    if (x.currentStyle)
        // in IE
        var y = x.clientHeight - parseInt(x.currentStyle["paddingTop"]) - parseInt(x.currentStyle["paddingBottom"]);
        // for IE5: var y = x.offsetWidth;
    else if (window.getComputedStyle)
        // in Gecko
        var y = document.defaultView.getComputedStyle(x, null).getPropertyValue("height");
    return y || 0;
}
function setCookie(name, value, expires, path, domain, secure) {
    document.cookie = name + "=" + escape(value) +
        ((expires) ? "; expires=" + expires : "") +
        ((path) ? "; path=" + path : "") +
        ((domain) ? "; domain=" + domain : "") +
        ((secure) ? "; secure" : "");
}

function getCookie(name) {
    var cookie = " " + document.cookie;
    var search = " " + name + "=";
    var setStr = null;
    var offset = 0;
    var end = 0;
    if (cookie.length > 0) {
        offset = cookie.indexOf(search);
        if (offset != -1) {
            offset += search.length;
            end = cookie.indexOf(";", offset)
            if (end == -1) {
                end = cookie.length;
            }
            setStr = unescape(cookie.substring(offset, end));
        }
    }
    return (setStr);
}
// main class prototype
function ColumnResize(table) {
    if (table.tagName != 'TABLE') return;

    this.id = table.id;
    // ============================================================
    // private data
    var self = this;

    var dragColumns = table.rows[0].cells; // first row columns, used for changing of width
    if (!dragColumns) return; // return if no table exists or no one row exists

    var dragColumnNo; // current dragging column
    var dragX;        // last event X mouse coordinate

    var saveOnmouseup;   // save document onmouseup event handler
    var saveOnmousemove; // save document onmousemove event handler
    var saveBodyCursor;  // save body cursor property

    // ============================================================
    // methods

    // ============================================================
    // do changes columns widths
    // returns true if success and false otherwise
    this.changeColumnWidth = function (no, w) {
        if (!dragColumns) return false;

        if (no < 0) return false;
        if (dragColumns.length < no) return false;

        if (parseInt(dragColumns[no].style.width) <= -w) return false;
        if (dragColumns[no + 1] && parseInt(dragColumns[no + 1].style.width) <= w) return false;

        dragColumns[no].style.width = parseInt(dragColumns[no].style.width) + w + 'px';
        if (dragColumns[no + 1])
            dragColumns[no + 1].style.width = parseInt(dragColumns[no + 1].style.width) - w + 'px';

        return true;
    }

    // ============================================================
    // do drag column width
    this.columnDrag = function (e) {
        var e = e || window.event;
        var X = e.clientX || e.pageX;
        if (!self.changeColumnWidth(dragColumnNo, X - dragX)) {
            // stop drag!
            self.stopColumnDrag(e);
        }

        dragX = X;
        // prevent other event handling
        preventEvent(e);
        return false;
    }

    // ============================================================
    // stops column dragging
    this.stopColumnDrag = function (e) {
        var e = e || window.event;
        if (!dragColumns) return;

        // restore handlers & cursor
        document.onmouseup = saveOnmouseup;
        document.onmousemove = saveOnmousemove;
        document.body.style.cursor = saveBodyCursor;

        // remember columns widths in cookies for server side
        var colWidth = '';
        var separator = '';
        for (var i = 0; i < dragColumns.length; i++) {
            colWidth += separator + parseInt(getWidth(dragColumns[i]));
            separator = '+';
        }
        var expire = new Date();
        expire.setDate(expire.getDate() + 365); // year
        document.cookie = self.id + '-width=' + colWidth +
            '; expires=' + expire.toGMTString();

        preventEvent(e);
    }

    // ============================================================
    // init data and start dragging
    this.startColumnDrag = function (e) {
        var e = e || window.event;

        // if not first button was clicked
        //if (e.button != 0) return;

        // remember dragging object
        dragColumnNo = (e.target || e.srcElement).parentNode.parentNode.cellIndex;
        dragX = e.clientX || e.pageX;

        // set up current columns widths in their particular attributes
        // do it in two steps to avoid jumps on page!
        var colWidth = new Array();
        for (var i = 0; i < dragColumns.length; i++)
            colWidth[i] = parseInt(getWidth(dragColumns[i]));
        for (var i = 0; i < dragColumns.length; i++) {
            dragColumns[i].width = ""; // for sure
            dragColumns[i].style.width = colWidth[i] + "px";
        }

        saveOnmouseup = document.onmouseup;
        document.onmouseup = self.stopColumnDrag;

        saveBodyCursor = document.body.style.cursor;
        document.body.style.cursor = 'e-resize';

        // fire!
        saveOnmousemove = document.onmousemove;
        document.onmousemove = self.columnDrag;

        preventEvent(e);
    }

    // prepare table header to be draggable
    // it runs during class creation
    for (var i = 0; i < dragColumns.length; i++) {
        dragColumns[i].innerHTML = "<div style='position:relative;height:100%;width:100%'>" +
            "<div style='" +
            "position:absolute;height:100%;width:5px;margin-right:-5px;" +
            "left:100%;top:0px;cursor:e-resize;z-index:10;'>" +
            "</div>" +
            dragColumns[i].innerHTML +
            "</div>";
        // BUGBUG: calculate real border width instead of 5px!!!
        dragColumns[i].firstChild.firstChild.onmousedown = this.startColumnDrag;
    }
}

// select all tables and make resizable those that have 'resizable' class
var resizableTables = new Array();
function ResizableColumns() {

    var tables = document.getElementsByTagName('table');
    for (var i = 0; tables.item(i) ; i++) {
        if (tables[i].className.match(/resizable/)) {
            // generate id
            if (!tables[i].id) tables[i].id = 'table' + (i + 1);
            // make table resizable
            resizableTables[resizableTables.length] = new ColumnResize(tables[i]);
        }
    }
    //	alert(resizableTables.length + ' tables was added.');
}
// init tables
/*
if (document.addEventListener)
    document.addEventListener("onload", ResizableColumns, false);
else if (window.attachEvent)
    window.attachEvent("onload", ResizableColumns);
*/

function MargeColumnResize(table, table1) {
    if (table.tagName != 'TABLE') return;

    this.id = table.id;
    // ============================================================
    // private data
    var self = this;
    var marginTOP = table1.style.marginTop;
    topMarginDiff = parseInt(marginTOP.toString().replace("px", "").replace("-", ""));
    var CLINTHT = table.rows[0].cells[0].offsetHeight;
    topMarginDiff = topMarginDiff - CLINTHT;
    var dragColumns = table.rows[0].cells; var dragColumnsDuplicate = table1.rows[0].cells; // first row columns, used for changing of width
    if (!dragColumns) return; // return if no table exists or no one row exists

    var dragColumnNo; // current dragging column
    var dragX;        // last event X mouse coordinate
    var dragY;
    var saveOnmouseup;   // save document onmouseup event handler
    var saveOnmousemove; // save document onmousemove event handler
    var saveBodyCursor;  // save body cursor property

    // ============================================================
    // methods

    // ============================================================
    // do changes columns widths
    // returns true if success and false otherwise
    this.changeColumnWidth = function (no, w, y) {
        if (!dragColumns) return false;

        if (no < 0) return false;
        if (dragColumns.length < no) return false;

        if (parseInt(dragColumns[no].style.width) <= -w) return false;
        if (dragColumns[no + 1] && parseInt(dragColumns[no + 1].style.width) <= w) return false;

        dragColumns[no].style.width = parseInt(dragColumns[no].style.width) + w + 'px';
        dragColumnsDuplicate[no].style.width = parseInt(dragColumnsDuplicate[no].style.width) + w + 'px';
        dragColumnsDuplicate[no].style.height = parseInt(dragColumns[no].style.height) + y + 'px';
        if (dragColumns[no + 1])
            dragColumns[no + 1].style.width = parseInt(dragColumns[no + 1].style.width) - w + 'px';
        dragColumnsDuplicate[no + 1].style.width = parseInt(dragColumnsDuplicate[no + 1].style.width) - w + 'px';
        var topcal = parseInt(dragColumns[no].style.height) - (y + topMarginDiff);
        table1.style.marginTop = -topcal + 'px';
        return true;
    }

    // ============================================================
    // do drag column width
    this.columnDrag = function (e) {
        var e = e || window.event;
        var X = e.clientX || e.pageX;
        var Y = e.clientY || e.pageY;
        if (!self.changeColumnWidth(dragColumnNo, X - dragX, Y - dragY)) {
            // stop drag!
            self.stopColumnDrag(e);
        }

        dragX = X;
        dragY = Y;
        // prevent other event handling
        preventEvent(e);
        return false;
    }

    // ============================================================
    // stops column dragging
    this.stopColumnDrag = function (e) {
        var e = e || window.event;
        if (!dragColumns) return;

        // restore handlers & cursor
        document.onmouseup = saveOnmouseup;
        document.onmousemove = saveOnmousemove;
        document.body.style.cursor = saveBodyCursor;

        // remember columns widths in cookies for server side
        var colWidth = '';
        var colHeight = '';
        var separator = '';
        for (var i = 0; i < dragColumns.length; i++) {
            colWidth += separator + parseInt(getWidth(dragColumns[i]));
            colHeight += separator + parseInt(getHeight(dragColumns[i]));
            separator = '+';
            dragColumnsDuplicate[i].height = ""; // for sure           
            dragColumnsDuplicate[i].height = colHeight[i] + "px";
            var tcal = parseInt(getHeight(dragColumns[i])) + topMarginDiff;
            table1.style.marginTop = -tcal + "px";
        }
        var expire = new Date();
        expire.setDate(expire.getDate() + 365); // year
        document.cookie = self.id + '-width=' + colWidth +
            '; expires=' + expire.toGMTString();

        preventEvent(e);
    }

    // ============================================================
    // init data and start dragging
    this.startColumnDrag = function (e) {
        var e = e || window.event;

        // if not first button was clicked
        //if (e.button != 0) return;

        // remember dragging object
        dragColumnNo = (e.target || e.srcElement).parentNode.parentNode.cellIndex;
        dragX = e.clientX || e.pageX;
        dragY = e.clientY || e.pageY;
        // set up current columns widths in their particular attributes
        // do it in two steps to avoid jumps on page!
        var colWidth = new Array();
        var colHeight = new Array();
        for (var i = 0; i < dragColumns.length; i++) {
            colWidth[i] = parseInt(getWidth(dragColumns[i]));
            colHeight[i] = parseInt(getHeight(dragColumns[i]));
        }

        for (var i = 0; i < dragColumns.length; i++) {
            dragColumns[i].width = ""; // for sure
            dragColumns[i].style.width = colWidth[i] + "px";
            //set marge table
            dragColumnsDuplicate[i].width = ""; // for sure
            dragColumnsDuplicate[i].style.width = colWidth[i] + "px";
            dragColumnsDuplicate[i].height = colHeight[i] + "px";
            var tcall = colHeight[i] + topMarginDiff;
            table1.style.marginTop = -(tcall) + "px";
        }

        saveOnmouseup = document.onmouseup;
        document.onmouseup = self.stopColumnDrag;

        saveBodyCursor = document.body.style.cursor;
        document.body.style.cursor = 'e-resize';

        // fire!
        saveOnmousemove = document.onmousemove;
        document.onmousemove = self.columnDrag;

        preventEvent(e);
    }

    // prepare table header to be draggable
    // it runs during class creation
    for (var i = 0; i < dragColumns.length; i++) {
        dragColumns[i].innerHTML = "<div style='position:relative;height:100%;width:100%'>" +
            "<div style='" +
            "position:absolute;height:100%;width:5px;margin-right:-5px;" +
            "left:100%;top:0px;cursor:e-resize;z-index:10;'>" +
            "</div>" +
            dragColumns[i].innerHTML +
            "</div>";
        dragColumnsDuplicate[i].innerHTML = "<div style='position:relative;height:100%;width:100%'>" +
            "<div style='" +
            "position:absolute;height:100%;width:5px;margin-right:-5px;" +
            "left:100%;top:0px;cursor:e-resize;z-index:10;'>" +
            "</div>" +
            dragColumnsDuplicate[i].innerHTML +
            "</div>";
        // BUGBUG: calculate real border width instead of 5px!!!
        dragColumns[i].firstChild.firstChild.onmousedown = this.startColumnDrag;
        dragColumnsDuplicate[i].firstChild.firstChild.onmousedown = this.startColumnDrag;
    }
}
//try {
//    window.addEventListener('load', ResizableColumns, false);
//} catch (e) {
//    window.onload = ResizableColumns;
//}

//document.body.onload = ResizableColumns;

//============================================================
//
// Usage. In your html code just include the follow:
//
//============================================================
// <table id='objectId'>
// ...
// </table>
// < script >
// var xxx = new ColumnDrag( 'objectId' );
// < / script >
//============================================================
//
// NB! spaces was used to prevent browser interpret it!
//
//============================================================
